using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Hosting;
using System.Web.Script.Serialization;

public sealed class CmfFeedbackEntry
{
    public string ScenarioId { get; set; }
    public string SightingId { get; set; }
    public string Title { get; set; }
    public string Component { get; set; }
    public string AiTag { get; set; }
    public string ReviewerTag { get; set; }
    public bool Helpful { get; set; }
    public string FeedbackNote { get; set; }
    public string TimestampUtc { get; set; }
}

public sealed class CmfFeedbackSubmitResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int TotalRecordedCount { get; set; }
}

public sealed class CmfPulseFeedbackEntry
{
    public string ScenarioId { get; set; }
    public string UserPrompt { get; set; }
    public string ScopeLabel { get; set; }
    public string ActiveTab { get; set; }
    public bool Helpful { get; set; }
    public string FeedbackNote { get; set; }
    public string TimestampUtc { get; set; }
}

public static class CmfFeedbackMemoryService
{
    private static readonly object SyncRoot = new object();

    public static CmfFeedbackSubmitResponse RecordFeedback(string sightingId, string title, string component, string aiTag, string reviewerTag, bool helpful, string feedbackNote)
    {
        if (string.IsNullOrWhiteSpace(sightingId) && string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(feedbackNote))
        {
            return new CmfFeedbackSubmitResponse { Success = false, Message = "Scenario details or feedback note are required." };
        }

        lock (SyncRoot)
        {
            List<CmfFeedbackEntry> entries = LoadAllEntriesInternal();
            string cleanSightingId = (sightingId ?? string.Empty).Trim();

            // Append every feedback submission as a new scenario learning entry (never overwrite previous feedback entries)
            entries.Insert(0, new CmfFeedbackEntry
            {
                ScenarioId = "SCN-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                SightingId = cleanSightingId,
                Title = (title ?? string.Empty).Trim(),
                Component = (component ?? string.Empty).Trim(),
                AiTag = (aiTag ?? string.Empty).Trim().ToUpperInvariant(),
                ReviewerTag = string.IsNullOrWhiteSpace(reviewerTag) ? (aiTag ?? string.Empty).Trim().ToUpperInvariant() : reviewerTag.Trim().ToUpperInvariant(),
                Helpful = helpful,
                FeedbackNote = (feedbackNote ?? string.Empty).Trim(),
                TimestampUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });

            if (entries.Count > 500)
            {
                entries = entries.GetRange(0, 500);
            }

            SaveAllEntriesInternal(entries);
            CmfAssistantResponseCache.ClearRecommendations();

            return new CmfFeedbackSubmitResponse
            {
                Success = true,
                Message = "Reviewer scenario feedback appended to learning log (" + entries.Count + " total scenarios recorded).",
                TotalRecordedCount = entries.Count
            };
        }
    }

    public static string BuildFewShotFeedbackContext(string component, string title)
    {
        CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
        if (!cfg.FeedbackMemoryEnabled) return string.Empty;

        List<CmfFeedbackEntry> entries;
        lock (SyncRoot)
        {
            entries = LoadAllEntriesInternal();
        }
        if (entries == null || entries.Count == 0) return string.Empty;

        int maxExamples = Math.Max(1, Math.Min(8, cfg.MaxFewShotExamples));
        string cleanComponent = (component ?? string.Empty).Trim();
        List<CmfFeedbackEntry> selected = new List<CmfFeedbackEntry>();

        foreach (CmfFeedbackEntry entry in entries)
        {
            if (entry == null) continue;
            if (!string.IsNullOrWhiteSpace(cleanComponent)
                && string.Equals(entry.Component, cleanComponent, StringComparison.OrdinalIgnoreCase))
            {
                selected.Add(entry);
                if (selected.Count >= maxExamples) break;
            }
        }

        if (selected.Count < maxExamples)
        {
            foreach (CmfFeedbackEntry entry in entries)
            {
                if (entry == null || selected.Contains(entry)) continue;
                if (!entry.Helpful || !string.IsNullOrWhiteSpace(entry.FeedbackNote) || !string.Equals(entry.AiTag, entry.ReviewerTag, StringComparison.OrdinalIgnoreCase))
                {
                    selected.Add(entry);
                    if (selected.Count >= maxExamples) break;
                }
            }
        }

        if (selected.Count == 0) return string.Empty;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("### LEARNED REVIEWER SCENARIO LOG (Apply these past reviewer scenario lessons to similar future tickets; always score the current ticket's latest metrics first) ###");
        int idx = 1;
        foreach (CmfFeedbackEntry item in selected)
        {
            sb.Append("- Scenario #").Append(idx++);
            if (!string.IsNullOrWhiteSpace(item.Component)) sb.Append(" [Component: ").Append(item.Component).Append("]");
            if (!string.IsNullOrWhiteSpace(item.Title)) sb.Append(" (Pattern: \"").Append(item.Title).Append("\")");
            sb.Append(": AI suggested ").Append(string.IsNullOrWhiteSpace(item.AiTag) ? "N/A" : item.AiTag)
              .Append(" -> Reviewer decision: ").Append(string.IsNullOrWhiteSpace(item.ReviewerTag) ? item.AiTag : item.ReviewerTag);
            if (!string.IsNullOrWhiteSpace(item.FeedbackNote))
            {
                sb.Append(" | Reviewer rationale: ").Append(item.FeedbackNote);
            }
            sb.AppendLine();
        }
        return sb.ToString().Trim();
    }

    private static string ResolveStoragePath()
    {
        CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
        string virtualPath = string.IsNullOrWhiteSpace(cfg.FeedbackStoragePath) ? "~/App_Data/cmf-feedback-memory.json" : cfg.FeedbackStoragePath;
        try
        {
            if (virtualPath.StartsWith("~", StringComparison.Ordinal))
            {
                string mapped = HostingEnvironment.MapPath(virtualPath);
                if (!string.IsNullOrWhiteSpace(mapped)) return mapped;
            }
        }
        catch { }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory ?? string.Empty;
        if (Directory.Exists(Path.Combine(baseDir, "cmf_portal", "App_Data")))
        {
            return Path.Combine(baseDir, "cmf_portal", "App_Data", "cmf-feedback-memory.json");
        }
        return Path.Combine(baseDir, "App_Data", "cmf-feedback-memory.json");
    }

    private static List<CmfFeedbackEntry> LoadAllEntriesInternal()
    {
        string path = ResolveStoragePath();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return new List<CmfFeedbackEntry>();
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
            return serializer.Deserialize<List<CmfFeedbackEntry>>(json) ?? new List<CmfFeedbackEntry>();
        }
        catch
        {
            return new List<CmfFeedbackEntry>();
        }
    }

    private static void SaveAllEntriesInternal(List<CmfFeedbackEntry> entries)
    {
        string path = ResolveStoragePath();
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
            File.WriteAllText(path, serializer.Serialize(entries ?? new List<CmfFeedbackEntry>()), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceWarning("CmfFeedbackMemoryService save failed: {0}", ex.Message);
        }
    }

    public static CmfFeedbackSubmitResponse RecordPulseFeedback(string userPrompt, string scopeLabel, string activeTab, bool helpful, string feedbackNote)
    {
        if (string.IsNullOrWhiteSpace(userPrompt) && string.IsNullOrWhiteSpace(feedbackNote))
        {
            return new CmfFeedbackSubmitResponse { Success = false, Message = "Prompt or feedback note is required." };
        }

        lock (SyncRoot)
        {
            List<CmfPulseFeedbackEntry> entries = LoadAllPulseEntriesInternal();
            string cleanNote = (feedbackNote ?? string.Empty).Trim();

            entries.Insert(0, new CmfPulseFeedbackEntry
            {
                ScenarioId = "PULSE-SCN-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                UserPrompt = (userPrompt ?? string.Empty).Trim(),
                ScopeLabel = (scopeLabel ?? string.Empty).Trim(),
                ActiveTab = (activeTab ?? string.Empty).Trim(),
                Helpful = helpful,
                FeedbackNote = cleanNote,
                TimestampUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            });

            if (entries.Count > 500)
            {
                entries = entries.GetRange(0, 500);
            }

            SaveAllPulseEntriesInternal(entries);

            string learnedJargonMsg = string.Empty;
            if (!string.IsNullOrWhiteSpace(cleanNote))
            {
                string targetCode;
                string aliasTerm;
                string category;
                if (CmfAssistantConfigService.TryAutoLearnJargonFromText(cleanNote, out category, out targetCode, out aliasTerm))
                {
                    learnedJargonMsg = " Also auto-learned jargon mapping: '" + aliasTerm + "' -> '" + targetCode + "'.";
                }
            }

            return new CmfFeedbackSubmitResponse
            {
                Success = true,
                Message = "Ask CMF Pulse scenario feedback saved (" + entries.Count + " total scenarios)." + learnedJargonMsg,
                TotalRecordedCount = entries.Count
            };
        }
    }

    public static string BuildPulseFewShotFeedbackContext(string prompt, string activeTab)
    {
        List<CmfPulseFeedbackEntry> entries;
        lock (SyncRoot)
        {
            entries = LoadAllPulseEntriesInternal();
        }
        if (entries == null || entries.Count == 0) return string.Empty;

        List<CmfPulseFeedbackEntry> selected = new List<CmfPulseFeedbackEntry>();
        foreach (CmfPulseFeedbackEntry entry in entries)
        {
            if (entry == null) continue;
            if (!string.IsNullOrWhiteSpace(entry.FeedbackNote))
            {
                selected.Add(entry);
                if (selected.Count >= 5) break;
            }
        }

        if (selected.Count == 0) return string.Empty;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n--- LEARNED ASK CMF PULSE SCENARIO FEEDBACK (Apply these user preferences and corrections) ---");
        int idx = 1;
        foreach (CmfPulseFeedbackEntry item in selected)
        {
            sb.AppendFormat("- Pulse Scenario #{0} (Tab: {1} | Query: \"{2}\"): User feedback: {3}\n",
                idx++,
                string.IsNullOrWhiteSpace(item.ActiveTab) ? "All" : item.ActiveTab,
                item.UserPrompt,
                item.FeedbackNote);
        }
        return sb.ToString().Trim();
    }

    private static string ResolvePulseStoragePath()
    {
        try
        {
            string mapped = HostingEnvironment.MapPath("~/App_Data/cmf-pulse-feedback.json");
            if (!string.IsNullOrWhiteSpace(mapped)) return mapped;
        }
        catch { }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory ?? string.Empty;
        if (Directory.Exists(Path.Combine(baseDir, "cmf_portal", "App_Data")))
        {
            return Path.Combine(baseDir, "cmf_portal", "App_Data", "cmf-pulse-feedback.json");
        }
        return Path.Combine(baseDir, "App_Data", "cmf-pulse-feedback.json");
    }

    private static List<CmfPulseFeedbackEntry> LoadAllPulseEntriesInternal()
    {
        string path = ResolvePulseStoragePath();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return new List<CmfPulseFeedbackEntry>();
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
            return serializer.Deserialize<List<CmfPulseFeedbackEntry>>(json) ?? new List<CmfPulseFeedbackEntry>();
        }
        catch
        {
            return new List<CmfPulseFeedbackEntry>();
        }
    }

    private static void SaveAllPulseEntriesInternal(List<CmfPulseFeedbackEntry> entries)
    {
        string path = ResolvePulseStoragePath();
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
            File.WriteAllText(path, serializer.Serialize(entries ?? new List<CmfPulseFeedbackEntry>()), Encoding.UTF8);
        }
        catch { }
    }
}
