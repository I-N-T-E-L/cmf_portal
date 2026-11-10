using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Script.Serialization;

public sealed class CmfPulseChartSeries
{
    public string Name { get; set; }
    public string Color { get; set; }
    public List<int> Data { get; set; }
}

public sealed class CmfPulseChartPayload
{
    public string Title { get; set; }
    public string ChartType { get; set; }
    public List<string> Categories { get; set; }
    public List<CmfPulseChartSeries> Series { get; set; }
}

public sealed class CmfGlobalAssistantTurn
{
    public string Role { get; set; }
    public string Text { get; set; }
}

public sealed class CmfPulseReportTemplate
{
    public string TemplateId { get; set; }
    public string Name { get; set; }
    public string FocusMode { get; set; }
    public string Slide1Title { get; set; }
    public string Slide3Title { get; set; }
    public string Slide4Title { get; set; }
    public string Blueprint { get; set; }
    public bool IsCustom { get; set; }
}

public sealed class CmfGlobalAssistantResponse
{
    public bool Success { get; set; }
    public string Answer { get; set; }
    public string ScopeLabel { get; set; }
    public bool HasChart { get; set; }
    public CmfPulseChartPayload Chart { get; set; }
    public bool HasPptx { get; set; }
    public string PptxUrl { get; set; }
    public string PptxFileName { get; set; }
    public int SlideCount { get; set; }
    public string Message { get; set; }
}

public sealed class CmfPlatformMetricsSnapshot
{
    public string PlatformLabel { get; set; }
    public string TableName { get; set; }
    public int TotalCount { get; set; }
    public int PendingAskCount { get; set; }
    public int ApprovedOkCount { get; set; }
    public int IncompleteCount { get; set; }
    public int DuplicateCount { get; set; }
    public int RejectedCount { get; set; }
    public int OpenCount { get; set; }
    public int ClosedCount { get; set; }
    public int RvpReproYesCount { get; set; }
    public int ImplementedCount { get; set; }
    public int VerifiedCount { get; set; }
    public int CompleteCount { get; set; }
    public int OfficialCmfApprovedCount { get; set; }
    public int RegressionCount { get; set; }
    public int PromotedCount { get; set; }
    public int HighPriorityCount { get; set; }
    public int HasLosCount { get; set; }
    public int MissingLosCount { get; set; }
    public int DispTptDays { get; set; }
    public int ResolveTptDays { get; set; }
    public int CritTptDays { get; set; }
}

public sealed class CmfPendingIssueRecord
{
    public string PlatformLabel { get; set; }
    public string SightingId { get; set; }
    public string PromotedId { get; set; }
    public string Title { get; set; }
    public string Component { get; set; }
    public string Owner { get; set; }
    public string CustomerCompany { get; set; }
    public string CustomerDetail { get; set; }
    public string CmfRequest { get; set; }
    public string Status { get; set; }
    public string ClosedReason { get; set; }
    public string Priority { get; set; }
    public string Drivers { get; set; }
    public string Los { get; set; }
    public string WorkweekSubmitted { get; set; }
    public string DateCmfAsk { get; set; }
    public DateTime ParsedAskDate { get; set; }
    public string SubmittedDate { get; set; }
    public DateTime ParsedSubmittedDate { get; set; }
    public string ImplementedDate { get; set; }
    public string Impact { get; set; }
    public string Reproducibility { get; set; }
    public string ReproOnRvp { get; set; }
    public string DaysOpen { get; set; }
}

public sealed class CmfPortalDimensionItem
{
    public string Name { get; set; }
    public string ExtraInfo { get; set; }
    public int TotalCount { get; set; }
    public int OpenCount { get; set; }
    public int PendingAskCount { get; set; }
    public int ApprovedOkCount { get; set; }
    public int ImplementedCount { get; set; }
    public int RejectedCount { get; set; }
}

public sealed class CmfPortalMultiTabContext
{
    public List<CmfPortalDimensionItem> TopCustomersOem { get; set; }
    public List<CmfPortalDimensionItem> TopDesigns { get; set; }
    public List<CmfPortalDimensionItem> TopOwners { get; set; }
    public List<CmfPortalDimensionItem> TopDriversMilestones { get; set; }
    public List<CmfPortalDimensionItem> PriorityBreakdown { get; set; }
    public List<CmfPortalDimensionItem> ClosedReasonBreakdown { get; set; }
    public List<CmfPortalDimensionItem> WorkweekBreakdown { get; set; }
    public List<CmfPendingIssueRecord> RecentAllIssues { get; set; }
    public Dictionary<string, Dictionary<string, CmfComponentMetricsSnapshot>> PlatformComponentMap { get; set; }
    public CmfPendingIssueRecord MatchedSpecificIssue { get; set; }
    public int PendingAge0To3Days { get; set; }
    public int PendingAge4To7Days { get; set; }
    public int PendingAge8To14Days { get; set; }
    public int PendingAgeOver14Days { get; set; }

    public CmfPortalMultiTabContext()
    {
        TopCustomersOem = new List<CmfPortalDimensionItem>();
        TopDesigns = new List<CmfPortalDimensionItem>();
        TopOwners = new List<CmfPortalDimensionItem>();
        TopDriversMilestones = new List<CmfPortalDimensionItem>();
        PriorityBreakdown = new List<CmfPortalDimensionItem>();
        ClosedReasonBreakdown = new List<CmfPortalDimensionItem>();
        WorkweekBreakdown = new List<CmfPortalDimensionItem>();
        RecentAllIssues = new List<CmfPendingIssueRecord>();
        PlatformComponentMap = new Dictionary<string, Dictionary<string, CmfComponentMetricsSnapshot>>(StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class CmfComponentMetricsSnapshot
{
    public string ComponentName { get; set; }
    public int TotalCount { get; set; }
    public int PendingAskCount { get; set; }
    public int ApprovedOkCount { get; set; }
    public int IncompleteCount { get; set; }
    public int DuplicateCount { get; set; }
    public int RejectedCount { get; set; }
}

public static class CmfGlobalAssistantService
{
    private static readonly object SchemaLock = new object();
    private static Dictionary<string, string> CachedDynamicPlatformTables;
    private static DateTime CachedDynamicSchemaExpiresUtc = DateTime.MinValue;

    public static bool IsPptxRequested(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return false;
        return Regex.IsMatch(
            prompt,
            @"\b(ppt|pptx|powerpoint|power\s+point|presentation|slide|slides|deck)\b",
            RegexOptions.IgnoreCase);
    }

    public static bool IsGreetingOrHelpPrompt(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return false;
        string clean = prompt.Trim();
        return Regex.IsMatch(
            clean,
            @"^(?:hi|hello|hey|good\s+(?:morning|afternoon|evening)|help|what\s+can\s+you\s+do|who\s+are\s+you|how\s+do\s+i\s+use\s+this)[.!?\s]*$",
            RegexOptions.IgnoreCase);
    }

    public static bool IsOffTopicPrompt(string prompt, List<CmfGlobalAssistantTurn> history)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return false;
        if (IsGreetingOrHelpPrompt(prompt) || IsPptxRequested(prompt)) return false;

        string lower = prompt.Trim().ToLowerInvariant();

        // Explicit off-topic patterns (weather, sports, cooking, general trivia, etc.)
        if (Regex.IsMatch(lower, @"\b(weather|recipe|cook|movie|song|poem|joke|stock\s+price|bitcoin|world\s+cup|cricket|football|horoscope|translate\s+to\s+french)\b"))
        {
            return true;
        }

        // Domain keywords across all CMF Portal tabs (Dashboard, Issue List, CMF Summary, CMF Pending, Reports & Analytics, Config/CMF Rules)
        if (Regex.IsMatch(lower, @"\b(cmf|cmf_ok|cmf_ask|cmf_review|cmf_incomplete|cmf_duplicate|cmf_reject|sighting|sightings|issue|issues|ticket|tickets|hsd|hsdes|platform|platforms|component|components|rvp|repro|trend|trends|compare|comparison|vs|versus|summary|summarize|dashboard|pending|approved|rejected|duplicate|duplicates|incomplete|open|closed|blocker|showstopper|priority|p1|p2|p3|p4|owner|owners|driver|drivers|bios|igfx|graphics|display|gfx|audio|memory|tbt|thunderbolt|usb4|hw|csme|os|ec|pnp|tnp|security|tools|iomt|dtt|count|how\s+many|total|top|status|health|chart|graph|report|table|export|tab|tabs|portal|list|design|designs|freeze|oem|oems|customer|customers|lenovo|hp|dell|asus|acer|samsung|microsoft|lcik|engineer|assigned|tpt|turnaround|disposition|resolution|age|bucket|stale|days|recent|latest|newest|oldest|first|last|when|date|workweek|ww|milestone|milestones|release|implemented|verified|complete|reason|reasons|noise|regression|regressions|promoted|bug|bugs|los|line\s+of\s+sight|impact|reproducibility|frequency|score|scoring|rule|rules|weight|weights|config|model|models|gnai|filter|filters|kpi|metric|metrics|analytics|novalake|nova\s+lake|pantherlake|panther\s+lake|lunarlake|lunar\s+lake|arrowlake|arrow\s+lake|wildcatlake|wildcat\s+lake|graniterapids|granite\s+rapids|\d{8,14})\b"))
        {
            return false;
        }

        // Check configured jargon mappings (platform families, SKUs, and component synonyms)
        try
        {
            CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
            if (cfg != null)
            {
                if (ContainsAnyJargonAlias(lower, cfg.PlatformFamilyJargons)
                    || ContainsAnyJargonAlias(lower, cfg.PlatformSkuJargons)
                    || ContainsAnyJargonAlias(lower, cfg.ComponentSynonymJargons))
                {
                    return false;
                }
            }
        }
        catch { }

        // Check dynamically discovered platform aliases
        Dictionary<string, string> allPlatforms = GetAvailablePlatformTables();
        foreach (KeyValuePair<string, string> kvp in allPlatforms)
        {
            string code = kvp.Value.ToLowerInvariant();
            if (lower.Contains(code) || lower.Contains(code.Replace("-", " ")) || lower.Contains(code.Replace("-", "_")))
            {
                return false;
            }
        }

        // Allow short conversational follow-ups when there is prior history
        if (history != null && history.Count > 0 && lower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length <= 8)
        {
            return false;
        }

        return true;
    }

    private static bool ContainsAnyJargonAlias(string lowerText, Dictionary<string, List<string>> map)
    {
        if (string.IsNullOrWhiteSpace(lowerText) || map == null) return false;
        foreach (KeyValuePair<string, List<string>> kvp in map)
        {
            if (kvp.Value == null) continue;
            foreach (string alias in kvp.Value)
            {
                if (!string.IsNullOrWhiteSpace(alias) && lowerText.Contains(alias.ToLowerInvariant()))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static CmfGlobalAssistantResponse ProcessQuery(
        string prompt,
        string activePlatform,
        string activeTab,
        List<CmfGlobalAssistantTurn> history = null,
        string dashboardContext = null,
        string templateId = null)
    {
        string cleanPrompt = (prompt ?? string.Empty).Trim();
        if ( string.IsNullOrWhiteSpace(cleanPrompt))
        {
            return new CmfGlobalAssistantResponse
            {
                Success = false,
                Message = "Please enter a question or PowerPoint request for Ask CMF Pulse."
            };
        }

        if (cleanPrompt.Length > 2000)
        {
            cleanPrompt = cleanPrompt.Substring(0, 2000);
        }

        // Check if the user is explicitly teaching a new jargon/synonym directly in chat
        if (Regex.IsMatch(cleanPrompt, @"^(?:teach|learn|add\s+jargon|add\s+synonym|map\s+jargon)\s*[:\-]?\s*", RegexOptions.IgnoreCase))
        {
            string body = Regex.Replace(cleanPrompt, @"^(?:teach|learn|add\s+jargon|add\s+synonym|map\s+jargon)\s*[:\-]?\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
            string learnedCat;
            string learnedTarget;
            string learnedAlias;
            if (CmfAssistantConfigService.TryAutoLearnJargonFromText(body, out learnedCat, out learnedTarget, out learnedAlias))
            {
                return new CmfGlobalAssistantResponse
                {
                    Success = true,
                    ScopeLabel = "Jargon Learned",
                    HasChart = false,
                    HasPptx = false,
                    Answer = "- **Jargon mapping saved:** Mapped **`" + learnedAlias + "`** -> **`" + learnedTarget + "`** under `" + learnedCat + "` in `cmf-assistant-config.json`.\n- **Immediate effect:** You can now use **`" + learnedAlias + "`** in any platform/component query or PowerPoint request.",
                    Message = "Jargon mapping saved."
                };
            }
        }

        if (IsGreetingOrHelpPrompt(cleanPrompt))
        {
            return new CmfGlobalAssistantResponse
            {
                Success = true,
                ScopeLabel = "CMF Portal",
                HasChart = false,
                HasPptx = false,
                Answer = "Hello! I am **Ask CMF Pulse** (**CMF Pulse AI**), your live assistant across all tabs of the CMF Portal.\n\n" +
                         "- **Platform & Component Trends:** Ask me to compare any platforms or families (for example, *\"Compare NovaLake vs PantherLake\"* or *\"Compare NVL-H vs NVL-S\"*) or check component trends (for example, *\"How many CMF_DUPLICATE issues are in graphics vs other components?\"*).\n" +
                         "- **Executive PowerPoint Decks:** Ask me to generate a slide deck (for example, *\"Create me a PPT with NVL-H vs NVL-S to understand the trend of CMF pending, cmf_ok, and cmf_incomplete\"*).\n" +
                         "- **Live Dashboard & Multi-Tab Q&A:** Ask about Issue List sightings, CMF Summary designs/SW image freeze/TPT, OEMs, owners, drivers/milestones, or CMF Pending items.",
                Message = "Greeting response returned."
            };
        }

        if (IsOffTopicPrompt(cleanPrompt, history))
        {
            return new CmfGlobalAssistantResponse
            {
                Success = true,
                ScopeLabel = "Out of Scope",
                HasChart = false,
                HasPptx = false,
                Answer = "I can only answer questions related to the **CMF Portal**, **HSDES sightings**, **platform and component trends**, or **dashboard metrics**.\n\n" +
                         "- Try asking: *\"Compare NVL-H vs NVL-S CMF pending and approved issues\"*\n" +
                         "- Or: *\"How many CMF_DUPLICATE issues are reported in Graphics vs other components?\"*",
                Message = "Off-topic request declined."
            };
        }

        List<string> targetTables = ResolveTargetPlatforms(cleanPrompt, activePlatform, history);

        List<CmfPlatformMetricsSnapshot> platformMetrics = new List<CmfPlatformMetricsSnapshot>();
        List<CmfComponentMetricsSnapshot> componentMetrics = new List<CmfComponentMetricsSnapshot>();
        List<string[]> topIssuesRows = new List<string[]>();
        List<CmfPendingIssueRecord> pendingIssues = new List<CmfPendingIssueRecord>();
        CmfPortalMultiTabContext multiTab = new CmfPortalMultiTabContext();

        LoadDatabaseSnapshots(targetTables, cleanPrompt, platformMetrics, componentMetrics, topIssuesRows, pendingIssues, multiTab);

        List<string> platformLabels = new List<string>();
        foreach (CmfPlatformMetricsSnapshot pm in platformMetrics)
        {
            if (pm != null && !string.IsNullOrWhiteSpace(pm.PlatformLabel) && !platformLabels.Contains(pm.PlatformLabel))
            {
                platformLabels.Add(pm.PlatformLabel);
            }
        }
        if (platformLabels.Count == 0)
        {
            foreach (string tbl in targetTables)
            {
                platformLabels.Add(ResolvePlatformDisplayLabel(tbl));
            }
        }
        string scopeLabel = platformLabels.Count > 0 ? string.Join(" vs ", platformLabels.ToArray()) : "Current Dashboard";

        CmfPulseReportTemplate activeTemplate = ResolveReportTemplate(templateId, cleanPrompt);
        bool hasExplicitTemplate = !string.IsNullOrWhiteSpace(templateId)
            && !string.Equals(templateId, "none", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(templateId, "auto", StringComparison.OrdinalIgnoreCase);
        bool wantsPptx = IsPptxRequested(cleanPrompt) || hasExplicitTemplate;
        bool wantsSpecificIssueDetails = !wantsPptx && (IsSpecificIssueOrPendingItemQuery(cleanPrompt) || IsFocusedTabDimensionQuery(cleanPrompt));
        bool wantsComponentFocus = WantsComponentComparison(cleanPrompt, componentMetrics, platformMetrics.Count);
        bool showChart = !wantsSpecificIssueDetails && (wantsPptx || WantsVisualChart(cleanPrompt, platformMetrics.Count, wantsComponentFocus));

        CmfPulseChartPayload chartPayload = showChart
            ? BuildChartPayload(cleanPrompt, scopeLabel, platformMetrics, componentMetrics, multiTab, wantsComponentFocus)
            : null;

        string aiAnswer = GenerateExecutiveAnswerWithModel(
            cleanPrompt,
            scopeLabel,
            activeTab,
            platformMetrics,
            componentMetrics,
            topIssuesRows,
            pendingIssues,
            multiTab,
            history,
            wantsPptx,
            dashboardContext,
            activeTemplate);

        string pptxPublicUrl = string.Empty;
        string pptxFileName = string.Empty;
        int slideCount = 0;

        if (wantsPptx)
        {
            try
            {
                CmfExecutiveDeckSpec deckSpec = BuildExecutiveDeckSpec(cleanPrompt, scopeLabel, aiAnswer, platformMetrics, componentMetrics, topIssuesRows, multiTab, activeTemplate);
                string outputDir = ResolveGeneratedReportsDirectory();
                string fullPptxPath = CmfExecutivePptxService.GenerateExecutiveDeck(deckSpec, outputDir, "CMF_Pulse_" + string.Join("_", platformLabels.ToArray()));
                pptxFileName = Path.GetFileName(fullPptxPath);
                pptxPublicUrl = "Content/generated-reports/" + pptxFileName;
                slideCount = 4;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("CMF Pulse PPTX generation failed: {0}", ex.Message);
            }
        }

        return new CmfGlobalAssistantResponse
        {
            Success = true,
            Answer = aiAnswer,
            ScopeLabel = scopeLabel,
            HasChart = chartPayload != null && chartPayload.Categories != null && chartPayload.Categories.Count > 0,
            Chart = chartPayload,
            HasPptx = !string.IsNullOrWhiteSpace(pptxPublicUrl),
            PptxUrl = pptxPublicUrl,
            PptxFileName = pptxFileName,
            SlideCount = slideCount,
            Message = wantsPptx && !string.IsNullOrWhiteSpace(pptxPublicUrl)
                ? "Executive PowerPoint deck and analysis generated."
                : "CMF Pulse AI analysis generated."
        };
    }

    public static Dictionary<string, string> GetAvailablePlatformTables()
    {
        lock (SchemaLock)
        {
            if (CachedDynamicPlatformTables != null && DateTime.UtcNow < CachedDynamicSchemaExpiresUtc)
            {
                return new Dictionary<string, string>(CachedDynamicPlatformTables, StringComparer.OrdinalIgnoreCase);
            }

            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (CmfPlatformDefinition def in CmfPlatformCatalog.GetOrderedPlatforms())
            {
                if (def != null && !string.IsNullOrWhiteSpace(def.TableName))
                {
                    map[def.TableName] = def.Label;
                }
            }

            string connStr = ResolveDatabaseConnectionString();
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        con.Open();
                        string sql = @"
                            SELECT TABLE_NAME
                            FROM INFORMATION_SCHEMA.TABLES
                            WHERE TABLE_NAME LIKE 'CMF[_]%[_]ALL[_]COMPONENTS[_]TABLE'
                              AND TABLE_NAME NOT LIKE '%DUMMY%'";
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string tbl = Convert.ToString(r[0]).Trim();
                                if (string.IsNullOrWhiteSpace(tbl) || !Regex.IsMatch(tbl, @"^CMF_[A-Za-z0-9_]+_ALL_COMPONENTS_TABLE$")) continue;
                                if (string.Equals(tbl, "CMF_NVL_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) continue;
                                if (!map.ContainsKey(tbl))
                                {
                                    string code = tbl.Substring(4, tbl.Length - 4 - "_ALL_COMPONENTS_TABLE".Length).Replace("_", "-");
                                    map[tbl] = code;
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            CachedDynamicPlatformTables = map;
            CachedDynamicSchemaExpiresUtc = DateTime.UtcNow.AddMinutes(5);
            return new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase);
        }
    }

    public static List<string> ResolveTargetPlatforms(string prompt, string activePlatform, List<CmfGlobalAssistantTurn> history = null)
    {
        List<string> matched = MatchPlatformsInText(prompt);

        // If follow-up prompt refers to prior turn (e.g. "create a PPT for them", "what about audio?", "also compare PTL"), merge prior turn platforms
        bool isFollowUpReference = Regex.IsMatch(prompt ?? string.Empty, @"\b(them|those|that|it|these|same|also|too|as\s+well|for\s+this|above)\b", RegexOptions.IgnoreCase);
        if ((matched.Count == 0 || isFollowUpReference) && history != null && history.Count > 0)
        {
            for (int i = history.Count - 1; i >= 0; i--)
            {
                CmfGlobalAssistantTurn turn = history[i];
                if (turn == null || string.IsNullOrWhiteSpace(turn.Text)) continue;
                List<string> priorPlatforms = MatchPlatformsInText(turn.Text);
                if (priorPlatforms.Count > 0)
                {
                    foreach (string pt in priorPlatforms)
                    {
                        if (!matched.Contains(pt)) matched.Add(pt);
                    }
                    break;
                }
            }
        }

        Dictionary<string, string> available = GetAvailablePlatformTables();

        if (matched.Count == 0 && Regex.IsMatch(prompt ?? string.Empty, @"\b(all\s+platforms|across\s+platforms|overall|compare\s+platforms|every\s+platform)\b", RegexOptions.IgnoreCase))
        {
            foreach (KeyValuePair<string, string> kvp in available)
            {
                if (matched.Count < 6 && !matched.Contains(kvp.Key)) matched.Add(kvp.Key);
            }
        }

        if (matched.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(activePlatform)
                && !string.Equals(activePlatform, CmfPlatformCatalog.OverallPlatformTableName, StringComparison.OrdinalIgnoreCase)
                && available.ContainsKey(activePlatform))
            {
                matched.Add(activePlatform);
            }
            else
            {
                if (available.ContainsKey("CMF_NVL_H_ALL_COMPONENTS_TABLE")) matched.Add("CMF_NVL_H_ALL_COMPONENTS_TABLE");
                if (available.ContainsKey("CMF_NVL_S_ALL_COMPONENTS_TABLE")) matched.Add("CMF_NVL_S_ALL_COMPONENTS_TABLE");
            }
        }

        return matched;
    }

    private static List<string> MatchPlatformsInText(string text)
    {
        List<string> matched = new List<string>();
        string lower = (text ?? string.Empty).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(lower)) return matched;

        Dictionary<string, string> available = GetAvailablePlatformTables();
        HashSet<string> matchedFamiliesWithExplicitSku = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Match explicit SKU labels/codes & configured SKU jargons first (sort longest first so NVL-HX matches before NVL-H)
        List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>(available);
        entries.Sort((a, b) => b.Value.Length.CompareTo(a.Value.Length));

        foreach (KeyValuePair<string, string> kvp in entries)
        {
            string label = kvp.Value.ToLowerInvariant();
            string[] variants = new[] { label, label.Replace("-", " "), label.Replace("-", "_") };
            foreach (string v in variants)
            {
                if (string.IsNullOrWhiteSpace(v)) continue;
                string pattern = @"(?<![a-z0-9_-])" + Regex.Escape(v) + @"(?![a-z0-9_-])";
                if (Regex.IsMatch(lower, pattern))
                {
                    if (!matched.Contains(kvp.Key)) matched.Add(kvp.Key);
                    if (label.Contains("-")) matchedFamiliesWithExplicitSku.Add(ExtractFamilyPrefix(kvp.Value));
                    break;
                }
            }
        }

        foreach (CmfPlatformDefinition def in CmfPlatformCatalog.GetOrderedPlatforms())
        {
            foreach (string alias in def.PromptAliases)
            {
                if (string.IsNullOrWhiteSpace(alias)) continue;
                string pattern = @"(?<![a-z0-9_-])" + Regex.Escape(alias.ToLowerInvariant()) + @"(?![a-z0-9_-])";
                if (Regex.IsMatch(lower, pattern))
                {
                    if (!matched.Contains(def.TableName)) matched.Add(def.TableName);
                    if (def.Label.Contains("-")) matchedFamiliesWithExplicitSku.Add(ExtractFamilyPrefix(def.Label));
                    break;
                }
            }
        }

        // Also check configured PlatformSkuJargons from cmf-assistant-config.json
        try
        {
            CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
            if (cfg != null && cfg.PlatformSkuJargons != null)
            {
                foreach (KeyValuePair<string, List<string>> skuEntry in cfg.PlatformSkuJargons)
                {
                    if (skuEntry.Value == null) continue;
                    foreach (string alias in skuEntry.Value)
                    {
                        if (string.IsNullOrWhiteSpace(alias)) continue;
                        string pattern = @"(?<![a-z0-9_-])" + Regex.Escape(alias.ToLowerInvariant()) + @"(?![a-z0-9_-])";
                        if (Regex.IsMatch(lower, pattern))
                        {
                            foreach (KeyValuePair<string, string> av in available)
                            {
                                if (string.Equals(av.Value, skuEntry.Key, StringComparison.OrdinalIgnoreCase) && !matched.Contains(av.Key))
                                {
                                    matched.Add(av.Key);
                                    matchedFamiliesWithExplicitSku.Add(ExtractFamilyPrefix(av.Value));
                                }
                            }
                        }
                    }
                }
            }
        }
        catch { }

        // 2. Always check family/group codenames (e.g. "novalake", "nova lake", "nvl", "pantherlake", "lunarlake", "arrowlake", etc.)
        // even when another platform (like PTL) was already matched!
        foreach (KeyValuePair<string, string[]> kvp in CmfPlatformCatalog.GetGroupAliasToTablesMap())
        {
            string pattern = @"(?<![a-z0-9_-])" + Regex.Escape(kvp.Key.ToLowerInvariant()) + @"(?![a-z0-9_-])";
            if (Regex.IsMatch(lower, pattern))
            {
                string fam = kvp.Key.IndexOf("nvl", StringComparison.OrdinalIgnoreCase) >= 0 || kvp.Key.IndexOf("nova", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "NVL"
                    : (kvp.Key.IndexOf("arl", StringComparison.OrdinalIgnoreCase) >= 0 || kvp.Key.IndexOf("arrow", StringComparison.OrdinalIgnoreCase) >= 0 ? "ARL" : kvp.Key.ToUpperInvariant());
                if (matchedFamiliesWithExplicitSku.Contains(fam)) continue;

                foreach (string tbl in kvp.Value)
                {
                    if (available.ContainsKey(tbl) && !matched.Contains(tbl)) matched.Add(tbl);
                }
                if (string.Equals(fam, "NVL", StringComparison.OrdinalIgnoreCase) && available.ContainsKey("CMF_NVL_Hx_ALL_COMPONENTS_TABLE") && !matched.Contains("CMF_NVL_Hx_ALL_COMPONENTS_TABLE"))
                {
                    matched.Add("CMF_NVL_Hx_ALL_COMPONENTS_TABLE");
                }
            }
        }

        // Also check configured PlatformFamilyJargons from cmf-assistant-config.json for any family prefix (NVL, PTL, LNL, ARL, WCL, GNR, etc.)
        try
        {
            CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
            if (cfg != null && cfg.PlatformFamilyJargons != null)
            {
                foreach (KeyValuePair<string, List<string>> famEntry in cfg.PlatformFamilyJargons)
                {
                    string famCode = (famEntry.Key ?? string.Empty).Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(famCode) || famEntry.Value == null) continue;
                    if (matchedFamiliesWithExplicitSku.Contains(famCode)) continue;

                    bool familyMentioned = false;
                    foreach (string alias in famEntry.Value)
                    {
                        if (string.IsNullOrWhiteSpace(alias)) continue;
                        string pattern = @"(?<![a-z0-9_-])" + Regex.Escape(alias.ToLowerInvariant()) + @"(?![a-z0-9_-])";
                        if (Regex.IsMatch(lower, pattern) || IsFuzzyTokenMatch(lower, alias.ToLowerInvariant()))
                        {
                            familyMentioned = true;
                            break;
                        }
                    }

                    if (familyMentioned)
                    {
                        foreach (KeyValuePair<string, string> av in available)
                        {
                            if (string.Equals(ExtractFamilyPrefix(av.Value), famCode, StringComparison.OrdinalIgnoreCase) && !matched.Contains(av.Key))
                            {
                                matched.Add(av.Key);
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return matched;
    }

    public static bool IsFuzzyTokenMatch(string lowerText, string targetAlias)
    {
        if (string.IsNullOrWhiteSpace(lowerText) || string.IsNullOrWhiteSpace(targetAlias)) return false;
        string cleanAlias = Regex.Replace(targetAlias.Trim().ToLowerInvariant(), @"[\s_-]+", string.Empty);
        if (cleanAlias.Length < 5) return false;
        int maxDist = cleanAlias.Length >= 8 ? 2 : 1;

        string[] words = Regex.Split(lowerText, @"[^a-z0-9]+");
        for (int i = 0; i < words.Length; i++)
        {
            string w = words[i];
            if (string.IsNullOrWhiteSpace(w)) continue;
            if (w.Length >= 5 && Math.Abs(w.Length - cleanAlias.Length) <= maxDist)
            {
                if (ComputeLevenshteinDistance(w, cleanAlias) <= maxDist) return true;
            }
            if (i + 1 < words.Length && !string.IsNullOrWhiteSpace(words[i + 1]))
            {
                string combined = w + words[i + 1];
                if (combined.Length >= 5 && Math.Abs(combined.Length - cleanAlias.Length) <= maxDist)
                {
                    if (ComputeLevenshteinDistance(combined, cleanAlias) <= maxDist) return true;
                }
            }
        }
        return false;
    }

    private static int ComputeLevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    private static string ExtractFamilyPrefix(string platformLabel)
    {
        string clean = (platformLabel ?? string.Empty).Trim();
        int dash = clean.IndexOf('-');
        return dash > 0 ? clean.Substring(0, dash).ToUpperInvariant() : clean.ToUpperInvariant();
    }

    private static string FormatFamilyDisplayName(string familyPrefix)
    {
        string prefix = (familyPrefix ?? string.Empty).Trim().ToUpperInvariant();
        switch (prefix)
        {
            case "NVL": return "NVL (Nova Lake)";
            case "PTL": return "PTL (Panther Lake)";
            case "LNL": return "LNL (Lunar Lake)";
            case "ARL": return "ARL (Arrow Lake)";
            case "WCL": return "WCL (Wildcat Lake)";
            case "GNR": return "GNR (Granite Rapids)";
            default: return prefix;
        }
    }

    private static bool IsSpecificIssueOrPendingItemQuery(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return false;
        if (IsPptxRequested(prompt)) return false;
        string lower = prompt.ToLowerInvariant();
        if (Regex.IsMatch(lower, @"\b\d{8,14}\b")) return true;
        if (Regex.IsMatch(lower, @"\b(most\s+recent|latest|newest|oldest|first|last|which\s+issue|which\s+sighting|when\s+did\s+it|details\s+of\s+the|list\s+the\s+pending|what\s+are\s+the\s+pending\s+issues|recent\s+issue|recent\s+sighting)\b"))
        {
            return true;
        }
        return false;
    }

    private static bool IsFocusedTabDimensionQuery(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return false;
        if (IsPptxRequested(prompt)) return false;
        string lower = prompt.ToLowerInvariant();
        return Regex.IsMatch(lower, @"\b(tpt|turnaround|disposition\s+time|resolution\s+time|design|designs|sw\s+image\s+freeze|image\s+freeze|freeze\s+date|oem|oems|customer\s+company|who\s+owns|top\s+owner|top\s+owners|owner\s+breakdown|owners|driver|drivers|milestone|milestones|los|line\s+of\s+sight|priority|priorities|showstopper|showstoppers|p1|p2|regression|regressions|promoted|closed\s+reason|closed\s+reasons|workweek|ww\s*\d*|age\s+bucket|aging|stale\s+pending|scoring\s+rule|scoring\s+rules|config\s+tab|cmf\s+rules|weights)\b");
    }

    private static bool WantsVisualChart(string prompt, int platformCount, bool wantsComponentFocus)
    {
        if (wantsComponentFocus || platformCount >= 2) return true;
        string lower = (prompt ?? string.Empty).ToLowerInvariant();
        return Regex.IsMatch(lower, @"\b(trend|trends|compare|comparison|vs|versus|chart|graph|plot|distribution|breakdown|summary|overview|health|status)\b");
    }

    private static bool WantsComponentComparison(string prompt, List<CmfComponentMetricsSnapshot> componentMetrics, int platformCount)
    {
        string lower = (prompt ?? string.Empty).ToLowerInvariant();
        if (Regex.IsMatch(lower, @"\b(component|components|igfx|graphics|audio|memory|tbt|thunderbolt|bios|csme|iomt|tnp|pnp|dtt|ec|os|security|tools)\b"))
        {
            return true;
        }
        if (componentMetrics != null)
        {
            foreach (CmfComponentMetricsSnapshot c in componentMetrics)
            {
                if (c != null && !string.IsNullOrWhiteSpace(c.ComponentName) && lower.Contains(c.ComponentName.ToLowerInvariant()))
                {
                    return true;
                }
            }
        }
        return platformCount <= 1 && Regex.IsMatch(lower, @"\b(duplicate|duplicates|cmf_duplicate)\b");
    }

    private static void LoadDatabaseSnapshots(
        List<string> targetTables,
        string prompt,
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        List<CmfComponentMetricsSnapshot> componentMetrics,
        List<string[]> topIssuesRows,
        List<CmfPendingIssueRecord> pendingIssues,
        CmfPortalMultiTabContext multiTab)
    {
        Dictionary<string, CmfComponentMetricsSnapshot> compMap = new Dictionary<string, CmfComponentMetricsSnapshot>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> oemMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> designMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> ownerMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> driverMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> priorityMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> closedReasonMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, CmfPortalDimensionItem> wwMap = new Dictionary<string, CmfPortalDimensionItem>(StringComparer.OrdinalIgnoreCase);

        Dictionary<string, string> available = GetAvailablePlatformTables();
        string connStr = ResolveDatabaseConnectionString();

        Match specificIdMatch = Regex.Match(prompt ?? string.Empty, @"\b(\d{8,14})\b");
        string specificId = specificIdMatch.Success ? specificIdMatch.Groups[1].Value : string.Empty;

        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    foreach (string tableName in targetTables)
                    {
                        if (!available.ContainsKey(tableName) && !Regex.IsMatch(tableName ?? string.Empty, @"^CMF_[A-Za-z0-9_]+_ALL_COMPONENTS_TABLE$")) continue;
                        string label = ResolvePlatformDisplayLabel(tableName);
                        CmfPlatformMetricsSnapshot pSnap = QueryPlatformSnapshot(con, tableName, label);
                        if (pSnap != null) platformMetrics.Add(pSnap);

                        QueryComponentSnapshots(con, tableName, label, compMap, multiTab != null ? multiTab.PlatformComponentMap : null);
                        QueryPendingListIssues(con, tableName, label, pendingIssues, 10, multiTab);
                        QueryTopIssues(con, tableName, label, topIssuesRows, 4);
                        QueryMultiTabBreakdowns(con, tableName, label, specificId, multiTab, oemMap, designMap, ownerMap, driverMap, priorityMap, closedReasonMap, wwMap);
                    }

                    // When multiple tables were resolved from a family codename (e.g. NVL -> NVL-H, NVL-S, NVL-Hx, NVL-U), drop 0-row placeholder SKU tables if populated SKUs exist
                    if (platformMetrics.Count > 1)
                    {
                        bool hasPopulated = false;
                        foreach (CmfPlatformMetricsSnapshot pm in platformMetrics)
                        {
                            if (pm != null && (pm.TotalCount > 0 || pm.PendingAskCount > 0)) { hasPopulated = true; break; }
                        }
                        if (hasPopulated)
                        {
                            platformMetrics.RemoveAll(pm => pm == null || (pm.TotalCount == 0 && pm.PendingAskCount == 0));
                        }
                    }

                    // If user asked for a specific Sighting/Promoted ID and it wasn't found in the active platform, search across all available platform tables
                    if (!string.IsNullOrWhiteSpace(specificId) && multiTab != null && multiTab.MatchedSpecificIssue == null)
                    {
                        foreach (KeyValuePair<string, string> kvp in available)
                        {
                            if (targetTables.Contains(kvp.Key)) continue;
                            CmfPendingIssueRecord found = QuerySpecificSightingFromTable(con, kvp.Key, kvp.Value, specificId);
                            if (found != null)
                            {
                                multiTab.MatchedSpecificIssue = found;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("CMF Pulse DB query error: {0}", ex.Message);
            }
        }

        if (pendingIssues != null && pendingIssues.Count > 1)
        {
            pendingIssues.Sort((a, b) => b.ParsedAskDate.CompareTo(a.ParsedAskDate));
        }

        if (multiTab != null)
        {
            if (multiTab.RecentAllIssues.Count > 1)
            {
                multiTab.RecentAllIssues.Sort((a, b) => b.ParsedSubmittedDate.CompareTo(a.ParsedSubmittedDate));
            }
            PopulateSortedDimensions(oemMap, multiTab.TopCustomersOem, 8);
            PopulateSortedDimensions(designMap, multiTab.TopDesigns, 8);
            PopulateSortedDimensions(ownerMap, multiTab.TopOwners, 8);
            PopulateSortedDimensions(driverMap, multiTab.TopDriversMilestones, 8);
            PopulateSortedDimensions(priorityMap, multiTab.PriorityBreakdown, 6);
            PopulateSortedDimensions(closedReasonMap, multiTab.ClosedReasonBreakdown, 8);
            PopulateSortedDimensions(wwMap, multiTab.WorkweekBreakdown, 8);
        }

        List<CmfComponentMetricsSnapshot> sortedComps = new List<CmfComponentMetricsSnapshot>(compMap.Values);
        bool sortByDuplicate = Regex.IsMatch(prompt ?? string.Empty, @"\b(duplicate|duplicates|cmf_duplicate)\b", RegexOptions.IgnoreCase);
        sortedComps.Sort((a, b) => sortByDuplicate
            ? b.DuplicateCount.CompareTo(a.DuplicateCount)
            : b.TotalCount.CompareTo(a.TotalCount));

        for (int i = 0; i < sortedComps.Count && i < 10; i++)
        {
            componentMetrics.Add(sortedComps[i]);
        }
    }

    private static void PopulateSortedDimensions(Dictionary<string, CmfPortalDimensionItem> map, List<CmfPortalDimensionItem> target, int limit)
    {
        if (map == null || target == null) return;
        List<CmfPortalDimensionItem> list = new List<CmfPortalDimensionItem>(map.Values);
        list.Sort((a, b) => b.TotalCount.CompareTo(a.TotalCount));
        for (int i = 0; i < list.Count && i < limit; i++)
        {
            target.Add(list[i]);
        }
    }

    private static void QueryPendingListIssues(SqlConnection con, string tableName, string platformLabel, List<CmfPendingIssueRecord> pendingIssues, int limit, CmfPortalMultiTabContext multiTab = null)
    {
        if (pendingIssues == null) return;
        string basePlatform = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
        string askTable = basePlatform + "_CMF_ASK";

        // First try querying the dedicated _CMF_ASK table joined with _ALL_COMPONENTS_TABLE (exact match with CMF Pending tab)
        try
        {
            string askSql = @"
                SELECT TOP " + limit + @"
                    CAST(d.cp_id AS VARCHAR(50)) AS CpId,
                    ISNULL(CAST(a.promoted_id AS VARCHAR(50)), 'NA') AS PromotedIdVal,
                    ISNULL(d.title, '') AS TitleVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(d.component)), ''), 'Unassigned')) AS CompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.customer_owner)), ''), 'N/A') AS OwnerVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(a.customer_company)), ''), 'N/A') AS CustCompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.customer_detail)), ''), 'N/A') AS CustDetailVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.cmf_request)), ''), 'cmf_ask') AS CmfReqVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.status)), ''), 'open') AS StatusVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(a.priority)), ''), 'N/A') AS PriorityVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(a.drivers)), ''), 'N/A') AS DriversVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(a.los)), ''), 'N/A') AS LosVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(a.ww_submitted)), ''), 'N/A') AS WwVal,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, d.date_cmf_ask), 120), ISNULL(CAST(d.date_cmf_ask AS VARCHAR(50)), 'N/A')) AS AskDateStr,
                    TRY_CONVERT(datetime, d.date_cmf_ask) AS ParsedAskDt,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, d.submitted_date), 120), ISNULL(CAST(d.submitted_date AS VARCHAR(50)), 'N/A')) AS SubDateStr,
                    TRY_CONVERT(datetime, d.submitted_date) AS ParsedSubDt,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.impact)), ''), 'N/A') AS ImpactVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(d.reproducibility)), ''), 'N/A') AS ReproVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(a.repro_on_rvp)), ''), 'No') AS RvpVal,
                    ISNULL(CAST(a.days_active AS VARCHAR(20)), ISNULL(CAST(d.days_open AS VARCHAR(20)), 'N/A')) AS DaysVal
                FROM " + askTable + @" d
                LEFT JOIN " + tableName + @" a ON d.cp_id = a.cp_id
                WHERE LOWER(LTRIM(RTRIM(ISNULL(d.status, '')))) NOT IN ('complete', 'rejected')
                ORDER BY TRY_CONVERT(datetime, d.date_cmf_ask) DESC, d.cp_id DESC";

            int addedFromAskTable = 0;
            using (SqlCommand cmd = new SqlCommand(askSql, con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    DateTime parsedDt = r["ParsedAskDt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["ParsedAskDt"]);
                    DateTime parsedSubDt = r["ParsedSubDt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["ParsedSubDt"]);
                    pendingIssues.Add(new CmfPendingIssueRecord
                    {
                        PlatformLabel = platformLabel,
                        SightingId = Convert.ToString(r["CpId"]),
                        PromotedId = Convert.ToString(r["PromotedIdVal"]),
                        Title = Convert.ToString(r["TitleVal"]),
                        Component = CleanComponentLabel(Convert.ToString(r["CompVal"])),
                        Owner = Convert.ToString(r["OwnerVal"]),
                        CustomerCompany = Convert.ToString(r["CustCompVal"]),
                        CustomerDetail = Convert.ToString(r["CustDetailVal"]),
                        CmfRequest = Convert.ToString(r["CmfReqVal"]).ToUpperInvariant(),
                        Status = Convert.ToString(r["StatusVal"]),
                        Priority = Convert.ToString(r["PriorityVal"]),
                        Drivers = Convert.ToString(r["DriversVal"]),
                        Los = Convert.ToString(r["LosVal"]),
                        WorkweekSubmitted = Convert.ToString(r["WwVal"]),
                        DateCmfAsk = Convert.ToString(r["AskDateStr"]),
                        ParsedAskDate = parsedDt,
                        SubmittedDate = Convert.ToString(r["SubDateStr"]),
                        ParsedSubmittedDate = parsedSubDt,
                        Impact = Convert.ToString(r["ImpactVal"]),
                        Reproducibility = Convert.ToString(r["ReproVal"]),
                        ReproOnRvp = NormalizeYesNoRvp(Convert.ToString(r["RvpVal"])),
                        DaysOpen = Convert.ToString(r["DaysVal"])
                    });
                    addedFromAskTable++;
                }
            }

            if (multiTab != null)
            {
                string ageSql = @"
                    SELECT
                        SUM(CASE WHEN ISNULL(DATEDIFF(DAY, TRY_CONVERT(date, d.date_cmf_ask), CAST(GETDATE() AS date)), 0) <= 3 THEN 1 ELSE 0 END) AS Age0To3,
                        SUM(CASE WHEN DATEDIFF(DAY, TRY_CONVERT(date, d.date_cmf_ask), CAST(GETDATE() AS date)) BETWEEN 4 AND 7 THEN 1 ELSE 0 END) AS Age4To7,
                        SUM(CASE WHEN DATEDIFF(DAY, TRY_CONVERT(date, d.date_cmf_ask), CAST(GETDATE() AS date)) BETWEEN 8 AND 14 THEN 1 ELSE 0 END) AS Age8To14,
                        SUM(CASE WHEN DATEDIFF(DAY, TRY_CONVERT(date, d.date_cmf_ask), CAST(GETDATE() AS date)) > 14 THEN 1 ELSE 0 END) AS AgeOver14
                    FROM " + askTable + @" d
                    WHERE LOWER(LTRIM(RTRIM(ISNULL(d.status, '')))) NOT IN ('complete', 'rejected')";
                using (SqlCommand ageCmd = new SqlCommand(ageSql, con))
                using (SqlDataReader ar = ageCmd.ExecuteReader())
                {
                    if (ar.Read())
                    {
                        multiTab.PendingAge0To3Days += SafeInt(ar["Age0To3"]);
                        multiTab.PendingAge4To7Days += SafeInt(ar["Age4To7"]);
                        multiTab.PendingAge8To14Days += SafeInt(ar["Age8To14"]);
                        multiTab.PendingAgeOver14Days += SafeInt(ar["AgeOver14"]);
                    }
                }
            }

            if (addedFromAskTable > 0) return;
        }
        catch { }

        // Fallback: query pending/incomplete issues from the main _ALL_COMPONENTS_TABLE ordered by date_cmf_ask DESC
        try
        {
            string mainSql = @"
                SELECT TOP " + limit + @"
                    CAST(cp_id AS VARCHAR(50)) AS CpId,
                    ISNULL(CAST(promoted_id AS VARCHAR(50)), 'NA') AS PromotedIdVal,
                    ISNULL(title, '') AS TitleVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(component)), ''), 'Unassigned')) AS CompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_owner)), ''), 'N/A') AS OwnerVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_company)), ''), 'N/A') AS CustCompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_detail)), ''), 'N/A') AS CustDetailVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(cmf_request)), ''), 'cmf_ask') AS CmfReqVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(status)), ''), 'open') AS StatusVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(priority)), ''), 'N/A') AS PriorityVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(drivers)), ''), 'N/A') AS DriversVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(los)), ''), 'N/A') AS LosVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(ww_submitted)), ''), 'N/A') AS WwVal,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, date_cmf_ask), 120), ISNULL(CAST(date_cmf_ask AS VARCHAR(50)), 'N/A')) AS AskDateStr,
                    TRY_CONVERT(datetime, date_cmf_ask) AS ParsedAskDt,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, submitted_date), 120), ISNULL(CAST(submitted_date AS VARCHAR(50)), 'N/A')) AS SubDateStr,
                    TRY_CONVERT(datetime, submitted_date) AS ParsedSubDt,
                    ISNULL(NULLIF(LTRIM(RTRIM(impact)), ''), 'N/A') AS ImpactVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(reproducibility)), ''), 'N/A') AS ReproVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(repro_on_rvp)), ''), 'No') AS RvpVal,
                    ISNULL(CAST(days_active AS VARCHAR(20)), 'N/A') AS DaysVal
                FROM " + tableName + @"
                WHERE LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected')
                  AND LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete')
                ORDER BY TRY_CONVERT(datetime, date_cmf_ask) DESC, cp_id DESC";

            using (SqlCommand cmd = new SqlCommand(mainSql, con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    DateTime parsedDt = r["ParsedAskDt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["ParsedAskDt"]);
                    DateTime parsedSubDt = r["ParsedSubDt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["ParsedSubDt"]);
                    pendingIssues.Add(new CmfPendingIssueRecord
                    {
                        PlatformLabel = platformLabel,
                        SightingId = Convert.ToString(r["CpId"]),
                        PromotedId = Convert.ToString(r["PromotedIdVal"]),
                        Title = Convert.ToString(r["TitleVal"]),
                        Component = CleanComponentLabel(Convert.ToString(r["CompVal"])),
                        Owner = Convert.ToString(r["OwnerVal"]),
                        CustomerCompany = Convert.ToString(r["CustCompVal"]),
                        CustomerDetail = Convert.ToString(r["CustDetailVal"]),
                        CmfRequest = Convert.ToString(r["CmfReqVal"]).ToUpperInvariant(),
                        Status = Convert.ToString(r["StatusVal"]),
                        Priority = Convert.ToString(r["PriorityVal"]),
                        Drivers = Convert.ToString(r["DriversVal"]),
                        Los = Convert.ToString(r["LosVal"]),
                        WorkweekSubmitted = Convert.ToString(r["WwVal"]),
                        DateCmfAsk = Convert.ToString(r["AskDateStr"]),
                        ParsedAskDate = parsedDt,
                        SubmittedDate = Convert.ToString(r["SubDateStr"]),
                        ParsedSubmittedDate = parsedSubDt,
                        Impact = Convert.ToString(r["ImpactVal"]),
                        Reproducibility = Convert.ToString(r["ReproVal"]),
                        ReproOnRvp = NormalizeYesNoRvp(Convert.ToString(r["RvpVal"])),
                        DaysOpen = Convert.ToString(r["DaysVal"])
                    });
                }
            }
        }
        catch { }
    }

    private static string NormalizeYesNoRvp(string val)
    {
        string lower = (val ?? string.Empty).Trim().ToLowerInvariant();
        return (lower == "yes" || lower == "true" || lower == "1") ? "Yes" : "No";
    }

    private static CmfPlatformMetricsSnapshot QueryPlatformSnapshot(SqlConnection con, string tableName, string label)
    {
        string basePlatform = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
        string askTable = basePlatform + "_CMF_ASK";

        CmfPlatformMetricsSnapshot snap = new CmfPlatformMetricsSnapshot
        {
            PlatformLabel = label,
            TableName = tableName
        };

        string sql = @"
            SELECT
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ask' THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_incomplete' THEN 1 ELSE 0 END) AS IncompCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_duplicate' THEN 1 ELSE 0 END) AS DupCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejectCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS ClosedCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(repro_on_rvp, '')))) IN ('yes', 'true', '1') THEN 1 ELSE 0 END) AS RvpYesCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'implemented' THEN 1 ELSE 0 END) AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'verified' THEN 1 ELSE 0 END) AS VerCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'complete' THEN 1 ELSE 0 END) AS CompCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(sysdebug, '')))) LIKE '%customer_must_fix%' AND LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) IN ('cmf_ok', 'cmf_duplicate') THEN 1 ELSE 0 END) AS OfficialCmfCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(regression, '')))) IN ('yes', 'true', '1') THEN 1 ELSE 0 END) AS RegCount,
                SUM(CASE WHEN LTRIM(RTRIM(ISNULL(CAST(promoted_id AS VARCHAR(50)), ''))) NOT IN ('', '0', 'NA', 'N/A') THEN 1 ELSE 0 END) AS PromotedCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) LIKE '%1%' OR LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) LIKE '%showstopper%' OR LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) LIKE '%high%' THEN 1 ELSE 0 END) AS HighPrioCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'open' AND LTRIM(RTRIM(ISNULL(los, ''))) NOT IN ('', 'N/A', 'NA') THEN 1 ELSE 0 END) AS HasLosCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'open' AND LTRIM(RTRIM(ISNULL(los, ''))) IN ('', 'N/A', 'NA') THEN 1 ELSE 0 END) AS MissingLosCount,
                ISNULL(AVG(CAST(CASE WHEN TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND LOWER(ISNULL(sysdebug, '')) LIKE '%customer_must_fix%' THEN DATEDIFF(DAY, ISNULL(TRY_CAST(date_cmf_ask AS DATE), TRY_CAST(date_cmf_decided AS DATE)), TRY_CAST(date_cmf_decided AS DATE)) END AS INT)), 0) AS DispTpt,
                ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) IS NOT NULL AND LOWER(ISNULL(sysdebug, '')) LIKE '%customer_must_fix%' THEN CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(TRY_CAST(date_cmf_decided AS DATE), TRY_CAST(implemented_date AS DATE)), TRY_CAST(implemented_date AS DATE)) END END AS INT)), 0) AS ResolveTpt,
                ISNULL(AVG(CAST(CASE WHEN LOWER(ISNULL(sysdebug, '')) LIKE '%customer_must_fix%' AND LOWER(ISNULL(cmf_request, '')) IN ('cmf_ok', 'cmf_duplicate') THEN TRY_CAST(days_active AS INT) END AS INT)), 0) AS CritTpt
            FROM " + tableName;

        using (SqlCommand cmd = new SqlCommand(sql, con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            if (r.Read())
            {
                snap.TotalCount = SafeInt(r["TotalCount"]);
                snap.PendingAskCount = SafeInt(r["AskCount"]);
                snap.ApprovedOkCount = SafeInt(r["OkCount"]);
                snap.IncompleteCount = SafeInt(r["IncompCount"]);
                snap.DuplicateCount = SafeInt(r["DupCount"]);
                snap.RejectedCount = SafeInt(r["RejectCount"]);
                snap.OpenCount = SafeInt(r["OpenCount"]);
                snap.ClosedCount = SafeInt(r["ClosedCount"]);
                snap.RvpReproYesCount = SafeInt(r["RvpYesCount"]);
                snap.ImplementedCount = SafeInt(r["ImplCount"]);
                snap.VerifiedCount = SafeInt(r["VerCount"]);
                snap.CompleteCount = SafeInt(r["CompCount"]);
                snap.OfficialCmfApprovedCount = SafeInt(r["OfficialCmfCount"]);
                snap.RegressionCount = SafeInt(r["RegCount"]);
                snap.PromotedCount = SafeInt(r["PromotedCount"]);
                snap.HighPriorityCount = SafeInt(r["HighPrioCount"]);
                snap.HasLosCount = SafeInt(r["HasLosCount"]);
                snap.MissingLosCount = SafeInt(r["MissingLosCount"]);
                snap.DispTptDays = SafeInt(r["DispTpt"]);
                snap.ResolveTptDays = SafeInt(r["ResolveTpt"]);
                snap.CritTptDays = SafeInt(r["CritTpt"]);
            }
        }

        // Also check dedicated _CMF_ASK table if AskCount in main table was 0
        try
        {
            string askSql = "SELECT COUNT(1) FROM " + askTable + " WHERE LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected')";
            using (SqlCommand askCmd = new SqlCommand(askSql, con))
            {
                int pendingFromAskTable = SafeInt(askCmd.ExecuteScalar());
                if (pendingFromAskTable > snap.PendingAskCount) snap.PendingAskCount = pendingFromAskTable;
            }
        }
        catch { }

        return snap;
    }

    private static void QueryMultiTabBreakdowns(
        SqlConnection con,
        string tableName,
        string platformLabel,
        string specificId,
        CmfPortalMultiTabContext multiTab,
        Dictionary<string, CmfPortalDimensionItem> oemMap,
        Dictionary<string, CmfPortalDimensionItem> designMap,
        Dictionary<string, CmfPortalDimensionItem> ownerMap,
        Dictionary<string, CmfPortalDimensionItem> driverMap,
        Dictionary<string, CmfPortalDimensionItem> priorityMap,
        Dictionary<string, CmfPortalDimensionItem> closedReasonMap,
        Dictionary<string, CmfPortalDimensionItem> wwMap)
    {
        if (multiTab == null) return;
        string basePlatform = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
        string designTable = basePlatform + "_DESIGN_TABLE";

        // 1. Specific Sighting / Promoted ID lookup across Issue List
        if (!string.IsNullOrWhiteSpace(specificId) && multiTab.MatchedSpecificIssue == null)
        {
            multiTab.MatchedSpecificIssue = QuerySpecificSightingFromTable(con, tableName, platformLabel, specificId);
        }

        // 2. Recent Issue List records (all sightings across Issue List tab ordered by submitted_date DESC)
        try
        {
            string recentAllSql = @"
                SELECT TOP 6
                    CAST(cp_id AS VARCHAR(50)) AS CpId,
                    ISNULL(CAST(promoted_id AS VARCHAR(50)), 'NA') AS PromotedIdVal,
                    ISNULL(title, '') AS TitleVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(component)), ''), 'Unassigned')) AS CompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_owner)), ''), ISNULL(NULLIF(LTRIM(RTRIM(owners_name)), ''), 'N/A')) AS OwnerVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_company)), ''), 'N/A') AS CustCompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_detail)), ''), 'N/A') AS CustDetailVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(cmf_request)), ''), 'N/A') AS CmfReqVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(status)), ''), 'open') AS StatusVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(closed_reason)), ''), 'N/A') AS ClosedReasonVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(priority)), ''), 'N/A') AS PriorityVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(drivers)), ''), 'N/A') AS DriversVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(los)), ''), 'N/A') AS LosVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(ww_submitted)), ''), 'N/A') AS WwVal,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, date_cmf_ask), 120), ISNULL(CAST(date_cmf_ask AS VARCHAR(50)), 'N/A')) AS AskDateStr,
                    TRY_CONVERT(datetime, date_cmf_ask) AS ParsedAskDt,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, submitted_date), 120), ISNULL(CAST(submitted_date AS VARCHAR(50)), 'N/A')) AS SubDateStr,
                    TRY_CONVERT(datetime, submitted_date) AS ParsedSubDt,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, implemented_date), 120), ISNULL(CAST(implemented_date AS VARCHAR(50)), 'N/A')) AS ImplDateStr,
                    ISNULL(NULLIF(LTRIM(RTRIM(impact)), ''), ISNULL(NULLIF(LTRIM(RTRIM(customer_impact)), ''), 'N/A')) AS ImpactVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(reproducibility)), ''), 'N/A') AS ReproVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(repro_on_rvp)), ''), 'No') AS RvpVal,
                    ISNULL(CAST(days_active AS VARCHAR(20)), ISNULL(CAST(days_open AS VARCHAR(20)), 'N/A')) AS DaysVal
                FROM " + tableName + @"
                ORDER BY TRY_CONVERT(datetime, submitted_date) DESC, cp_id DESC";

            using (SqlCommand cmd = new SqlCommand(recentAllSql, con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    multiTab.RecentAllIssues.Add(ReadIssueRecordFromReader(r, platformLabel));
                }
            }
        }
        catch { }

        // 3. OEM / Customer Company breakdown (OEM Summary & Issue List)
        AccumulateDimensionBreakdown(con, oemMap, @"
            SELECT TOP 10
                ISNULL(NULLIF(LTRIM(RTRIM(customer_company)), ''), 'Unassigned') AS DimName,
                '' AS ExtraVal,
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete') AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
            FROM " + tableName + @"
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(customer_company)), ''), 'Unassigned')
            ORDER BY COUNT(1) DESC");

        // 4. Customer Design & SW Image Freeze breakdown (CMF Summary Design Table)
        bool loadedDesignWithFreeze = false;
        try
        {
            AccumulateDimensionBreakdown(con, designMap, @"
                SELECT TOP 10
                    ISNULL(NULLIF(LTRIM(RTRIM(a.customer_detail)), ''), 'Unassigned') AS DimName,
                    ISNULL(MAX(NULLIF(LTRIM(RTRIM(d.sw_image_freeze)), '')), 'N/A') AS ExtraVal,
                    COUNT(1) AS TotalCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(a.status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(a.cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete') AND LOWER(LTRIM(RTRIM(ISNULL(a.status, '')))) NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS AskCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(a.cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(a.status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(a.cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
                FROM " + tableName + @" a
                LEFT JOIN " + designTable + @" d ON LTRIM(RTRIM(ISNULL(a.customer_detail, ''))) = LTRIM(RTRIM(ISNULL(d.customer_detail, '')))
                GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(a.customer_detail)), ''), 'Unassigned')
                ORDER BY COUNT(1) DESC");
            loadedDesignWithFreeze = true;
        }
        catch { }

        if (!loadedDesignWithFreeze)
        {
            AccumulateDimensionBreakdown(con, designMap, @"
                SELECT TOP 10
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_detail)), ''), 'Unassigned') AS DimName,
                    'N/A' AS ExtraVal,
                    COUNT(1) AS TotalCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete') AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS AskCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
                FROM " + tableName + @"
                GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(customer_detail)), ''), 'Unassigned')
                ORDER BY COUNT(1) DESC");
        }

        // 5. Top Owners breakdown (Issue List & CMF Pending)
        AccumulateDimensionBreakdown(con, ownerMap, @"
            SELECT TOP 10
                ISNULL(NULLIF(LTRIM(RTRIM(customer_owner)), ''), ISNULL(NULLIF(LTRIM(RTRIM(owners_name)), ''), 'Unassigned')) AS DimName,
                '' AS ExtraVal,
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete') AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
            FROM " + tableName + @"
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(customer_owner)), ''), ISNULL(NULLIF(LTRIM(RTRIM(owners_name)), ''), 'Unassigned'))
            ORDER BY COUNT(1) DESC");

        // 6. Drivers / Milestones breakdown (CMF Summary & Dashboard)
        AccumulateDimensionBreakdown(con, driverMap, @"
            SELECT TOP 10
                ISNULL(NULLIF(LTRIM(RTRIM(drivers)), ''), ISNULL(NULLIF(LTRIM(RTRIM(must_fix_for)), ''), 'Unassigned')) AS DimName,
                '' AS ExtraVal,
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'open' THEN 1 ELSE 0 END) AS OpenCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ask' THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
            FROM " + tableName + @"
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(drivers)), ''), ISNULL(NULLIF(LTRIM(RTRIM(must_fix_for)), ''), 'Unassigned'))
            ORDER BY COUNT(1) DESC");

        // 7. Priority breakdown (Issue List & Dashboard)
        AccumulateDimensionBreakdown(con, priorityMap, @"
            SELECT TOP 8
                ISNULL(NULLIF(LTRIM(RTRIM(priority)), ''), 'Unspecified') AS DimName,
                '' AS ExtraVal,
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ask' THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
            FROM " + tableName + @"
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(priority)), ''), 'Unspecified')
            ORDER BY COUNT(1) DESC");

        // 8. Closed Reason breakdown (CMF Summary Noise vs Fix & Reports)
        AccumulateDimensionBreakdown(con, closedReasonMap, @"
            SELECT TOP 10
                ISNULL(NULLIF(LTRIM(RTRIM(closed_reason)), ''), 'Open / None') AS DimName,
                '' AS ExtraVal,
                COUNT(1) AS TotalCount,
                0 AS OpenCount,
                0 AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                0 AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
            FROM " + tableName + @"
            WHERE LTRIM(RTRIM(ISNULL(closed_reason, ''))) <> ''
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(closed_reason)), ''), 'Open / None')
            ORDER BY COUNT(1) DESC");

        // 9. Workweek submitted breakdown (Reports & Analytics / Issue List)
        AccumulateDimensionBreakdown(con, wwMap, @"
            SELECT TOP 10
                ISNULL(NULLIF(LTRIM(RTRIM(ww_submitted)), ''), 'Unspecified') AS DimName,
                '' AS ExtraVal,
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected', 'closed', 'verified') THEN 1 ELSE 0 END) AS OpenCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ask' THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejCount
            FROM " + tableName + @"
            WHERE LTRIM(RTRIM(ISNULL(ww_submitted, ''))) <> ''
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(ww_submitted)), ''), 'Unspecified')
            ORDER BY COUNT(1) DESC");
    }

    private static void AccumulateDimensionBreakdown(SqlConnection con, Dictionary<string, CmfPortalDimensionItem> map, string sql)
    {
        if (map == null) return;
        try
        {
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    string name = Convert.ToString(r["DimName"]).Trim();
                    if (string.IsNullOrWhiteSpace(name)) name = "Unassigned";
                    string extra = Convert.ToString(r["ExtraVal"]).Trim();

                    CmfPortalDimensionItem item;
                    if (!map.TryGetValue(name, out item))
                    {
                        item = new CmfPortalDimensionItem { Name = name, ExtraInfo = extra };
                        map[name] = item;
                    }
                    else if ((string.IsNullOrWhiteSpace(item.ExtraInfo) || item.ExtraInfo == "N/A") && !string.IsNullOrWhiteSpace(extra) && extra != "N/A")
                    {
                        item.ExtraInfo = extra;
                    }

                    item.TotalCount += SafeInt(r["TotalCount"]);
                    item.OpenCount += SafeInt(r["OpenCount"]);
                    item.PendingAskCount += SafeInt(r["AskCount"]);
                    item.ApprovedOkCount += SafeInt(r["OkCount"]);
                    item.ImplementedCount += SafeInt(r["ImplCount"]);
                    item.RejectedCount += SafeInt(r["RejCount"]);
                }
            }
        }
        catch { }
    }

    private static CmfPendingIssueRecord QuerySpecificSightingFromTable(SqlConnection con, string tableName, string platformLabel, string specificId)
    {
        if (string.IsNullOrWhiteSpace(specificId)) return null;
        try
        {
            string sql = @"
                SELECT TOP 1
                    CAST(cp_id AS VARCHAR(50)) AS CpId,
                    ISNULL(CAST(promoted_id AS VARCHAR(50)), 'NA') AS PromotedIdVal,
                    ISNULL(title, '') AS TitleVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(component)), ''), 'Unassigned')) AS CompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_owner)), ''), ISNULL(NULLIF(LTRIM(RTRIM(owners_name)), ''), 'N/A')) AS OwnerVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_company)), ''), 'N/A') AS CustCompVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(customer_detail)), ''), 'N/A') AS CustDetailVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(cmf_request)), ''), 'N/A') AS CmfReqVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(status)), ''), 'open') AS StatusVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(closed_reason)), ''), 'N/A') AS ClosedReasonVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(priority)), ''), 'N/A') AS PriorityVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(drivers)), ''), 'N/A') AS DriversVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(los)), ''), 'N/A') AS LosVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(ww_submitted)), ''), 'N/A') AS WwVal,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, date_cmf_ask), 120), ISNULL(CAST(date_cmf_ask AS VARCHAR(50)), 'N/A')) AS AskDateStr,
                    TRY_CONVERT(datetime, date_cmf_ask) AS ParsedAskDt,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, submitted_date), 120), ISNULL(CAST(submitted_date AS VARCHAR(50)), 'N/A')) AS SubDateStr,
                    TRY_CONVERT(datetime, submitted_date) AS ParsedSubDt,
                    ISNULL(CONVERT(VARCHAR(30), TRY_CONVERT(datetime, implemented_date), 120), ISNULL(CAST(implemented_date AS VARCHAR(50)), 'N/A')) AS ImplDateStr,
                    ISNULL(NULLIF(LTRIM(RTRIM(impact)), ''), ISNULL(NULLIF(LTRIM(RTRIM(customer_impact)), ''), 'N/A')) AS ImpactVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(reproducibility)), ''), 'N/A') AS ReproVal,
                    ISNULL(NULLIF(LTRIM(RTRIM(repro_on_rvp)), ''), 'No') AS RvpVal,
                    ISNULL(CAST(days_active AS VARCHAR(20)), ISNULL(CAST(days_open AS VARCHAR(20)), 'N/A')) AS DaysVal
                FROM " + tableName + @"
                WHERE CAST(cp_id AS VARCHAR(50)) = @id OR CAST(promoted_id AS VARCHAR(50)) = @id";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", specificId.Trim());
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        return ReadIssueRecordFromReader(r, platformLabel);
                    }
                }
            }
        }
        catch { }
        return null;
    }

    private static CmfPendingIssueRecord ReadIssueRecordFromReader(SqlDataReader r, string platformLabel)
    {
        DateTime parsedDt = r["ParsedAskDt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["ParsedAskDt"]);
        DateTime parsedSubDt = r["ParsedSubDt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["ParsedSubDt"]);
        return new CmfPendingIssueRecord
        {
            PlatformLabel = platformLabel,
            SightingId = Convert.ToString(r["CpId"]),
            PromotedId = Convert.ToString(r["PromotedIdVal"]),
            Title = Convert.ToString(r["TitleVal"]),
            Component = CleanComponentLabel(Convert.ToString(r["CompVal"])),
            Owner = Convert.ToString(r["OwnerVal"]),
            CustomerCompany = Convert.ToString(r["CustCompVal"]),
            CustomerDetail = Convert.ToString(r["CustDetailVal"]),
            CmfRequest = Convert.ToString(r["CmfReqVal"]).ToUpperInvariant(),
            Status = Convert.ToString(r["StatusVal"]),
            ClosedReason = Convert.ToString(r["ClosedReasonVal"]),
            Priority = Convert.ToString(r["PriorityVal"]),
            Drivers = Convert.ToString(r["DriversVal"]),
            Los = Convert.ToString(r["LosVal"]),
            WorkweekSubmitted = Convert.ToString(r["WwVal"]),
            DateCmfAsk = Convert.ToString(r["AskDateStr"]),
            ParsedAskDate = parsedDt,
            SubmittedDate = Convert.ToString(r["SubDateStr"]),
            ParsedSubmittedDate = parsedSubDt,
            ImplementedDate = Convert.ToString(r["ImplDateStr"]),
            Impact = Convert.ToString(r["ImpactVal"]),
            Reproducibility = Convert.ToString(r["ReproVal"]),
            ReproOnRvp = NormalizeYesNoRvp(Convert.ToString(r["RvpVal"])),
            DaysOpen = Convert.ToString(r["DaysVal"])
        };
    }

    private static void QueryComponentSnapshots(
        SqlConnection con,
        string tableName,
        string platformLabel,
        Dictionary<string, CmfComponentMetricsSnapshot> compMap,
        Dictionary<string, Dictionary<string, CmfComponentMetricsSnapshot>> platformCompMap)
    {
        string sql = @"
            SELECT TOP 15
                ISNULL(NULLIF(LTRIM(RTRIM(component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(component)), ''), 'Unassigned')) AS CompName,
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ask' THEN 1 ELSE 0 END) AS AskCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok' THEN 1 ELSE 0 END) AS OkCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_incomplete' THEN 1 ELSE 0 END) AS IncompCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_duplicate' THEN 1 ELSE 0 END) AS DupCount,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_reject' THEN 1 ELSE 0 END) AS RejectCount
            FROM " + tableName + @"
            GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(component)), ''), 'Unassigned'))
            ORDER BY COUNT(1) DESC";

        Dictionary<string, CmfComponentMetricsSnapshot> perPlatMap = null;
        if (platformCompMap != null && !string.IsNullOrWhiteSpace(platformLabel))
        {
            if (!platformCompMap.TryGetValue(platformLabel, out perPlatMap))
            {
                perPlatMap = new Dictionary<string, CmfComponentMetricsSnapshot>(StringComparer.OrdinalIgnoreCase);
                platformCompMap[platformLabel] = perPlatMap;
            }
        }

        using (SqlCommand cmd = new SqlCommand(sql, con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read())
            {
                string rawName = CleanComponentLabel(Convert.ToString(r["CompName"]));
                int total = SafeInt(r["TotalCount"]);
                int ask = SafeInt(r["AskCount"]);
                int ok = SafeInt(r["OkCount"]);
                int inc = SafeInt(r["IncompCount"]);
                int dup = SafeInt(r["DupCount"]);
                int rej = SafeInt(r["RejectCount"]);

                CmfComponentMetricsSnapshot item;
                if (!compMap.TryGetValue(rawName, out item))
                {
                    item = new CmfComponentMetricsSnapshot { ComponentName = rawName };
                    compMap[rawName] = item;
                }
                item.TotalCount += total;
                item.PendingAskCount += ask;
                item.ApprovedOkCount += ok;
                item.IncompleteCount += inc;
                item.DuplicateCount += dup;
                item.RejectedCount += rej;

                if (perPlatMap != null)
                {
                    CmfComponentMetricsSnapshot platItem;
                    if (!perPlatMap.TryGetValue(rawName, out platItem))
                    {
                        platItem = new CmfComponentMetricsSnapshot { ComponentName = rawName };
                        perPlatMap[rawName] = platItem;
                    }
                    platItem.TotalCount += total;
                    platItem.PendingAskCount += ask;
                    platItem.ApprovedOkCount += ok;
                    platItem.IncompleteCount += inc;
                    platItem.DuplicateCount += dup;
                    platItem.RejectedCount += rej;
                }
            }
        }
    }

    private static void QueryTopIssues(SqlConnection con, string tableName, string platformLabel, List<string[]> rows, int limitPerPlatform)
    {
        if (rows.Count >= 8) return;
        string sql = @"
            SELECT TOP " + limitPerPlatform + @"
                CAST(cp_id AS VARCHAR(50)) AS CpId,
                ISNULL(NULLIF(LTRIM(RTRIM(component_group)), ''), ISNULL(NULLIF(LTRIM(RTRIM(component)), ''), 'N/A')) AS Comp,
                ISNULL(NULLIF(LTRIM(RTRIM(cmf_request)), ''), 'cmf_ask') AS CmfReq,
                ISNULL(NULLIF(LTRIM(RTRIM(status)), ''), 'open') AS StatusVal,
                ISNULL(title, '') AS TitleVal
            FROM " + tableName + @"
            WHERE LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected')
            ORDER BY CASE WHEN LOWER(ISNULL(cmf_request, '')) = 'cmf_ask' THEN 0 WHEN LOWER(ISNULL(cmf_request, '')) = 'cmf_incomplete' THEN 1 ELSE 2 END, cp_id DESC";

        using (SqlCommand cmd = new SqlCommand(sql, con))
        using (SqlDataReader r = cmd.ExecuteReader())
        {
            while (r.Read() && rows.Count < 8)
            {
                rows.Add(new[]
                {
                    platformLabel,
                    Convert.ToString(r["CpId"]),
                    CleanComponentLabel(Convert.ToString(r["Comp"])),
                    Convert.ToString(r["CmfReq"]).ToUpperInvariant(),
                    Convert.ToString(r["StatusVal"]),
                    Convert.ToString(r["TitleVal"])
                });
            }
        }
    }

    private static CmfPulseChartPayload BuildChartPayload(
        string prompt,
        string scopeLabel,
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        List<CmfComponentMetricsSnapshot> componentMetrics,
        CmfPortalMultiTabContext multiTab,
        bool wantsComponentFocus)
    {
        CmfPulseChartPayload chart = new CmfPulseChartPayload
        {
            ChartType = "bar",
            Categories = new List<string>(),
            Series = new List<CmfPulseChartSeries>()
        };

        CmfPulseChartSeries sPending = new CmfPulseChartSeries { Name = "Pending (ASK)", Color = "#f59e0b", Data = new List<int>() };
        CmfPulseChartSeries sOk = new CmfPulseChartSeries { Name = "Approved (OK)", Color = "#10b981", Data = new List<int>() };
        CmfPulseChartSeries sIncomp = new CmfPulseChartSeries { Name = "Incomplete", Color = "#3b82f6", Data = new List<int>() };
        CmfPulseChartSeries sDup = new CmfPulseChartSeries { Name = "Duplicate", Color = "#8b5cf6", Data = new List<int>() };

        CmfComponentMetricsSnapshot requestedComp = FindMentionedComponent(prompt, componentMetrics);

        // If comparing multiple platforms/SKUs for a specific component (e.g. "Compare NovaLake vs PantherLake for graphics"), plot each platform/SKU's counts for that component
        if (platformMetrics.Count >= 2 && requestedComp != null && multiTab != null && multiTab.PlatformComponentMap != null)
        {
            chart.Title = requestedComp.ComponentName + " Comparison (" + scopeLabel + ")";
            for (int i = 0; i < platformMetrics.Count; i++)
            {
                CmfPlatformMetricsSnapshot p = platformMetrics[i];
                chart.Categories.Add(p.PlatformLabel);
                Dictionary<string, CmfComponentMetricsSnapshot> perPlat;
                CmfComponentMetricsSnapshot cSnap = null;
                if (multiTab.PlatformComponentMap.TryGetValue(p.PlatformLabel, out perPlat) && perPlat != null)
                {
                    perPlat.TryGetValue(requestedComp.ComponentName, out cSnap);
                }
                sPending.Data.Add(cSnap != null ? cSnap.PendingAskCount : 0);
                sOk.Data.Add(cSnap != null ? cSnap.ApprovedOkCount : 0);
                sIncomp.Data.Add(cSnap != null ? cSnap.IncompleteCount : 0);
                sDup.Data.Add(cSnap != null ? cSnap.DuplicateCount : 0);
            }
        }
        else if (wantsComponentFocus && componentMetrics.Count > 0 && platformMetrics.Count <= 1)
        {
            chart.Title = "Component CMF Breakdown (" + scopeLabel + ")";
            for (int i = 0; i < componentMetrics.Count && i < 6; i++)
            {
                CmfComponentMetricsSnapshot c = componentMetrics[i];
                chart.Categories.Add(c.ComponentName);
                sPending.Data.Add(c.PendingAskCount);
                sOk.Data.Add(c.ApprovedOkCount);
                sIncomp.Data.Add(c.IncompleteCount);
                sDup.Data.Add(c.DuplicateCount);
            }
        }
        else if (platformMetrics.Count > 0)
        {
            chart.Title = "Platform CMF Disposition Trend (" + scopeLabel + ")";
            for (int i = 0; i < platformMetrics.Count; i++)
            {
                CmfPlatformMetricsSnapshot p = platformMetrics[i];
                chart.Categories.Add(p.PlatformLabel);
                sPending.Data.Add(p.PendingAskCount);
                sOk.Data.Add(p.ApprovedOkCount);
                sIncomp.Data.Add(p.IncompleteCount);
                sDup.Data.Add(p.DuplicateCount);
            }
        }

        chart.Series.Add(sPending);
        chart.Series.Add(sOk);
        chart.Series.Add(sIncomp);
        chart.Series.Add(sDup);
        return chart;
    }

    private static string GenerateExecutiveAnswerWithModel(
        string prompt,
        string scopeLabel,
        string activeTab,
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        List<CmfComponentMetricsSnapshot> componentMetrics,
        List<string[]> topIssuesRows,
        List<CmfPendingIssueRecord> pendingIssues,
        CmfPortalMultiTabContext multiTab,
        List<CmfGlobalAssistantTurn> history,
        bool wantsPptx,
        string dashboardContext = null,
        CmfPulseReportTemplate activeTemplate = null)
    {
        StringBuilder dataContext = new StringBuilder();
        dataContext.AppendLine("Scope: " + scopeLabel + " | Active Tab: " + (string.IsNullOrWhiteSpace(activeTab) ? "Dashboard" : activeTab));
        dataContext.AppendLine("\n--- VERIFIED SQL DATABASE PLATFORM METRICS Across All Tabs (DO NOT ALTER THESE NUMBERS) ---");
        foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
        {
            dataContext.AppendFormat(
                "- {0}: Total={1}, Pending_Review(CMF_ASK)={2}, Approved_Must_Fix(CMF_OK)={3}, Needs_More_Info(CMF_INCOMPLETE)={4}, Duplicate(CMF_DUPLICATE)={5}, Rejected(CMF_REJECT)={6}, Open={7}, Closed={8}, Implemented={9}, Verified={10}, Complete={11}, Official_CMF_Approved={12}, RVP_Repro_Yes={13}, Regressions={14}, Promoted_Bugs={15}, High_Priority={16}, Open_With_LOS={17}, Open_Missing_LOS={18}, Avg_Disp_TPT_Days={19}, Avg_Resolve_TPT_Days={20}, Avg_Active_Crit_TPT_Days={21}\n",
                p.PlatformLabel, p.TotalCount, p.PendingAskCount, p.ApprovedOkCount, p.IncompleteCount, p.DuplicateCount, p.RejectedCount, p.OpenCount, p.ClosedCount, p.ImplementedCount, p.VerifiedCount, p.CompleteCount, p.OfficialCmfApprovedCount, p.RvpReproYesCount, p.RegressionCount, p.PromotedCount, p.HighPriorityCount, p.HasLosCount, p.MissingLosCount, p.DispTptDays, p.ResolveTptDays, p.CritTptDays);
        }

        string familyRollupSection = BuildFamilyRollupAndSkuContext(platformMetrics, multiTab, FindMentionedComponent(prompt, componentMetrics));
        if (!string.IsNullOrWhiteSpace(familyRollupSection))
        {
            dataContext.AppendLine("\n--- PLATFORM FAMILY ROLLUP & INDIVIDUAL SKU BREAKDOWN ---");
            dataContext.AppendLine(familyRollupSection.Trim());
        }

        dataContext.AppendLine("\n--- VERIFIED SQL DATABASE COMPONENT METRICS (Note: iGFX = Integrated Graphics) ---");
        foreach (CmfComponentMetricsSnapshot c in componentMetrics)
        {
            dataContext.AppendFormat(
                "- {0}: Total={1}, Pending_Review(CMF_ASK)={2}, Approved(CMF_OK)={3}, Incomplete(CMF_INCOMPLETE)={4}, Duplicate(CMF_DUPLICATE)={5}, Rejected(CMF_REJECT)={6}\n",
                c.ComponentName, c.TotalCount, c.PendingAskCount, c.ApprovedOkCount, c.IncompleteCount, c.DuplicateCount, c.RejectedCount);
        }

        if (multiTab != null)
        {
            if (multiTab.MatchedSpecificIssue != null)
            {
                CmfPendingIssueRecord m = multiTab.MatchedSpecificIssue;
                dataContext.AppendLine("\n--- MATCHED SPECIFIC SIGHTING RECORD (Issue List / CMF Database) ---");
                dataContext.AppendFormat(
                    "- [{0}] Sighting ID: {1} | Promoted ID: {2} | Status: {3} | Closed Reason: {4} | CMF Tag: {5} | Priority: {6} | Component: {7} | Owner: {8} | OEM: {9} | Design: {10} | Driver/Milestone: {11} | LOS: {12} | WW Submitted: {13} | Submitted Date: {14} | CMF Ask Date: {15} | Implemented Date: {16} | Days Open: {17} | RVP Repro: {18} | Reproducibility: {19} | Impact: {20} | Title: {21}\n",
                    m.PlatformLabel, m.SightingId, m.PromotedId, m.Status, m.ClosedReason, m.CmfRequest, m.Priority, m.Component, m.Owner, m.CustomerCompany, m.CustomerDetail, m.Drivers, m.Los, m.WorkweekSubmitted, m.SubmittedDate, m.DateCmfAsk, m.ImplementedDate, m.DaysOpen, m.ReproOnRvp, m.Reproducibility, m.Impact, m.Title);
            }

            dataContext.AppendFormat(
                "\n--- CMF PENDING TAB AGE BUCKETS ---\n- 0-3 Days: {0} | 4-7 Days: {1} | 8-14 Days: {2} | Over 14 Days (Stale): {3}\n",
                multiTab.PendingAge0To3Days, multiTab.PendingAge4To7Days, multiTab.PendingAge8To14Days, multiTab.PendingAgeOver14Days);

            AppendDimensionSection(dataContext, "CUSTOMER / OEM SUMMARY (CMF Summary & Issue List Tabs)", multiTab.TopCustomersOem, false);
            AppendDimensionSection(dataContext, "CUSTOMER DESIGN & SW IMAGE FREEZE SUMMARY (CMF Summary Design Table)", multiTab.TopDesigns, true);
            AppendDimensionSection(dataContext, "TOP ISSUE OWNERS (Issue List & CMF Pending Tabs)", multiTab.TopOwners, false);
            AppendDimensionSection(dataContext, "DRIVERS & MILESTONES BREAKDOWN (CMF Summary & Dashboard Tabs)", multiTab.TopDriversMilestones, false);
            AppendDimensionSection(dataContext, "PRIORITY BREAKDOWN (Issue List & Dashboard Tabs)", multiTab.PriorityBreakdown, false);
            AppendDimensionSection(dataContext, "TOP CLOSED REASONS (CMF Summary Noise/Fix & Reports Tabs)", multiTab.ClosedReasonBreakdown, false);
            AppendDimensionSection(dataContext, "WORKWEEK SUBMISSIONS (Reports & Issue List Tabs)", multiTab.WorkweekBreakdown, false);

            if (multiTab.RecentAllIssues != null && multiTab.RecentAllIssues.Count > 0)
            {
                dataContext.AppendLine("\n--- ISSUE LIST TAB: MOST RECENTLY SUBMITTED SIGHTINGS (All Categories) ---");
                for (int i = 0; i < multiTab.RecentAllIssues.Count && i < 6; i++)
                {
                    CmfPendingIssueRecord rec = multiTab.RecentAllIssues[i];
                    dataContext.AppendFormat(
                        "- #{0} [{1}] Sighting ID: {2} (Promoted: {3}) | Submitted: {4} (WW: {5}) | Status: {6} | CMF Tag: {7} | Priority: {8} | Component: {9} | Owner: {10} | OEM/Design: {11}/{12} | Title: {13}\n",
                        i + 1, rec.PlatformLabel, rec.SightingId, rec.PromotedId, rec.SubmittedDate, rec.WorkweekSubmitted, rec.Status, rec.CmfRequest, rec.Priority, rec.Component, rec.Owner, rec.CustomerCompany, rec.CustomerDetail, rec.Title);
                }
            }
        }

        if (pendingIssues != null && pendingIssues.Count > 0)
        {
            dataContext.AppendLine("\n--- CMF PENDING LIST ISSUES (Ordered from MOST RECENT to oldest by CMF Ask Date) ---");
            for (int i = 0; i < pendingIssues.Count && i < 10; i++)
            {
                CmfPendingIssueRecord rec = pendingIssues[i];
                dataContext.AppendFormat(
                    "- #{0} [{1}] Sighting ID: {2} (Promoted: {3}) | Entered CMF Pending List (CMF Ask Date): {4} | Submitted Date: {5} | Days Open: {6} | Component: {7} | Owner: {8} | Customer Detail: {9} | Request Tag: {10} | RVP Repro: {11} | Reproducibility: {12} | Impact: {13} | Title: {14}\n",
                    i + 1, rec.PlatformLabel, rec.SightingId, rec.PromotedId, rec.DateCmfAsk, rec.SubmittedDate, rec.DaysOpen, rec.Component, rec.Owner, rec.CustomerDetail, rec.CmfRequest, rec.ReproOnRvp, rec.Reproducibility, rec.Impact, rec.Title);
            }
        }

        try
        {
            CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
            if (cfg != null)
            {
                dataContext.AppendFormat(
                    "\n--- CONFIG / CMF RULES TAB SETTINGS ---\n- Primary AI Model: {0} | Fast Summary Model: {1} | Fallback Models: {2}\n- Scoring Thresholds: CMF_OK >= {3}, CMF_REVIEW >= {4} (Customer Experience Override Score: {5})\n- Compiled Scoring Rules:\n{6}\n",
                    cfg.PrimaryModel, cfg.FastSummaryModel, cfg.FallbackModels != null ? string.Join(", ", cfg.FallbackModels.ToArray()) : "none",
                    cfg.ThresholdScore, cfg.ReviewThresholdScore, cfg.CustomerExperienceOverrideScore, cfg.CompiledRulesText);
            }
        }
        catch { }

        if (topIssuesRows != null && topIssuesRows.Count > 0)
        {
            dataContext.AppendLine("\n--- SAMPLE ACTIVE / PENDING SIGHTINGS ---");
            foreach (string[] row in topIssuesRows)
            {
                if (row != null && row.Length >= 6)
                {
                    dataContext.AppendFormat("- [{0}] Sighting {1} ({2} | {3} | {4}): {5}\n", row[0], row[1], row[2], row[3], row[4], row[5]);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(dashboardContext))
        {
            dataContext.AppendLine("\n--- LIVE DASHBOARD UI SNAPSHOT (Current user screen context) ---");
            dataContext.AppendLine(dashboardContext.Trim());
        }

        string pulseLearnedFeedback = CmfFeedbackMemoryService.BuildPulseFewShotFeedbackContext(prompt, activeTab);
        if (!string.IsNullOrWhiteSpace(pulseLearnedFeedback))
        {
            dataContext.AppendLine(pulseLearnedFeedback);
        }

        if (activeTemplate != null && !string.IsNullOrWhiteSpace(activeTemplate.Blueprint))
        {
            dataContext.AppendLine("\n--- SELECTED REPORT TEMPLATE BLUEPRINT (" + activeTemplate.Name + ") ---");
            dataContext.AppendLine(activeTemplate.Blueprint);
        }

        if (history != null && history.Count > 0)
        {
            dataContext.AppendLine("\n--- RECENT CONVERSATION HISTORY ---");
            int start = Math.Max(0, history.Count - 4);
            for (int i = start; i < history.Count; i++)
            {
                if (history[i] != null && !string.IsNullOrWhiteSpace(history[i].Text))
                {
                    dataContext.AppendLine((history[i].Role ?? "user") + ": " + history[i].Text);
                }
            }
        }

        StringBuilder llmPrompt = new StringBuilder();
        llmPrompt.AppendLine("You are CMF Pulse AI, the reliable executive assistant across ALL tabs of the Intel CMF Web Portal (Dashboard, Issue List, CMF Summary, CMF Pending, Reports & Analytics, and Config/CMF Rules).");
        llmPrompt.AppendLine("Strict Rules:");
        llmPrompt.AppendLine("1. ANSWER THE EXACT QUESTION FIRST across any portal tab or category: Whether the user asks about Dashboard KPIs, Issue List sightings, CMF Summary TPT/Designs/SW Image Freeze/OEMs/Drivers, CMF Pending issues/age buckets, Reports & Analytics workweeks/closed reasons, or Config/CMF Rules scoring weights & AI models, directly answer that specific question using the verified sections below -- do NOT dump generic portfolio totals unless asked.");
        llmPrompt.AppendLine("2. ZERO DATA MANIPULATION: Use ONLY the exact numbers, dates, and IDs from the Verified SQL Database Metrics and Live Dashboard UI Snapshot below. Never invent, round, or alter any count or date.");
        llmPrompt.AppendLine("3. SIMPLE HUMAN LANGUAGE: Explain findings in clear, friendly everyday English (3 to 5 bullet points starting with '- ' and a **bold** lead-in label). Translate codes naturally (for example, 'Approved as Must-Fix (CMF_OK)', 'Pending Review (CMF_ASK)', 'Needs More Info (CMF_INCOMPLETE)', 'Duplicate (CMF_DUPLICATE)').");
        llmPrompt.AppendLine("4. Note that 'iGFX (Graphics)' is the graphics component group in the database.");
        if (wantsPptx)
        {
            llmPrompt.AppendLine("5. The user requested a PowerPoint presentation: state in the final bullet that a 4-slide widescreen Executive PowerPoint (.pptx) deck with KPI scorecards, executive highlights, vector comparison charts, and top sightings has been generated and is ready to download below.");
        }
        llmPrompt.AppendLine("\nLive Context:\n" + dataContext.ToString());
        llmPrompt.AppendLine("\nUser Request: " + prompt);

        string modelReply;
        if (TryCallPulseModel(llmPrompt.ToString(), out modelReply))
        {
            return modelReply.Trim();
        }

        return BuildDeterministicExecutiveSummary(prompt, scopeLabel, platformMetrics, componentMetrics, pendingIssues, multiTab, wantsPptx, dashboardContext);
    }

    private static void AppendDimensionSection(StringBuilder sb, string title, List<CmfPortalDimensionItem> items, bool includeExtraAsFreeze)
    {
        if (sb == null || items == null || items.Count == 0) return;
        sb.AppendLine("\n--- " + title + " ---");
        for (int i = 0; i < items.Count && i < 8; i++)
        {
            CmfPortalDimensionItem d = items[i];
            if (includeExtraAsFreeze)
            {
                sb.AppendFormat("- {0} (SW Image Freeze: {1}): Total={2}, Open={3}, Pending_Ask={4}, Approved_OK={5}, Implemented/Verified={6}, Rejected={7}\n",
                    d.Name, string.IsNullOrWhiteSpace(d.ExtraInfo) ? "N/A" : d.ExtraInfo, d.TotalCount, d.OpenCount, d.PendingAskCount, d.ApprovedOkCount, d.ImplementedCount, d.RejectedCount);
            }
            else
            {
                sb.AppendFormat("- {0}: Total={1}, Open={2}, Pending_Ask={3}, Approved_OK={4}, Implemented/Verified={5}, Rejected={6}\n",
                    d.Name, d.TotalCount, d.OpenCount, d.PendingAskCount, d.ApprovedOkCount, d.ImplementedCount, d.RejectedCount);
            }
        }
    }

    private static bool TryCallPulseModel(string userPrompt, out string reply)
    {
        reply = string.Empty;
        string token = CmfAssistantConfigService.ResolveGnaiToken();
        string baseEndpoint = CmfAssistantConfigService.ResolveGnaiEndpoint();
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(baseEndpoint) || baseEndpoint.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        List<string> models = CmfAssistantConfigService.ResolveModelChain(CmfAssistantConfigService.ResolveFastSummaryModel());
        JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        foreach (string model in models)
        {
            try
            {
                string endpoint = CmfAssistantConfigService.ResolveGnaiEndpointForModel(baseEndpoint, model);
                bool isAnthropic = CmfAssistantConfigService.IsAnthropicMessagesEndpoint(endpoint, model);

                object payload;
                if (isAnthropic)
                {
                    payload = new
                    {
                        model = model,
                        max_tokens = 700,
                        temperature = 0,
                        system = "You are CMF Pulse AI, an executive analytics assistant for Intel CMF Portal. Return concise '- **Label:** ...' bullet points grounded in the provided SQL metrics.",
                        messages = new object[] { new { role = "user", content = userPrompt } }
                    };
                }
                else
                {
                    payload = new
                    {
                        model = model,
                        reasoning_effort = "low",
                        messages = new object[]
                        {
                            new { role = "system", content = "You are CMF Pulse AI, an executive analytics assistant for Intel CMF Portal. Return concise '- **Label:** ...' bullet points grounded in the provided SQL metrics." },
                            new { role = "user", content = userPrompt }
                        }
                    };
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(endpoint);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.Headers["Authorization"] = "Bearer " + token;
                req.Timeout = 20000;
                req.ReadWriteTimeout = 20000;
                req.Proxy = null;

                using (StreamWriter sw = new StreamWriter(req.GetRequestStream()))
                {
                    sw.Write(serializer.Serialize(payload));
                }

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    IDictionary raw = serializer.DeserializeObject(sr.ReadToEnd()) as IDictionary;
                    if (raw != null)
                    {
                        IList contentBlocks = raw["content"] as IList;
                        if (contentBlocks != null && contentBlocks.Count > 0)
                        {
                            IDictionary b0 = contentBlocks[0] as IDictionary;
                            if (b0 != null && b0["text"] != null)
                            {
                                reply = b0["text"].ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(reply)) return true;
                            }
                        }
                        IList choices = raw["choices"] as IList;
                        if (choices != null && choices.Count > 0)
                        {
                            IDictionary c0 = choices[0] as IDictionary;
                            IDictionary msg = c0 != null ? c0["message"] as IDictionary : null;
                            if (msg != null && msg["content"] != null)
                            {
                                reply = msg["content"].ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(reply)) return true;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        // Fallback to GitHub Models if GNAI token is expired/unavailable
        try
        {
            string ghKey = ResolveWebConfigAppSetting("GitHubModels:ApiKey");
            string ghEndpoint = ResolveWebConfigAppSetting("GitHubModels:Endpoint");
            string ghModel = ResolveWebConfigAppSetting("GitHubModels:Model");
            string ghProxy = ResolveWebConfigAppSetting("GitHubModels:Proxy");
            if (!string.IsNullOrWhiteSpace(ghKey) && !ghKey.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(ghEndpoint)) ghEndpoint = "https://models.inference.ai.azure.com/chat/completions";
                if (string.IsNullOrWhiteSpace(ghModel)) ghModel = "gpt-4o-mini";

                object ghPayload = new
                {
                    model = ghModel,
                    temperature = 0,
                    messages = new object[]
                    {
                        new { role = "system", content = "You are CMF Pulse AI, an executive analytics assistant for Intel CMF Portal. Return concise '- **Label:** ...' bullet points grounded in the provided SQL metrics." },
                        new { role = "user", content = userPrompt }
                    }
                };

                HttpWebRequest ghReq = (HttpWebRequest)WebRequest.Create(ghEndpoint);
                ghReq.Method = "POST";
                ghReq.ContentType = "application/json";
                ghReq.Accept = "application/json";
                ghReq.Headers["Authorization"] = "Bearer " + ghKey;
                ghReq.Timeout = 15000;
                ghReq.ReadWriteTimeout = 15000;
                if (!string.IsNullOrWhiteSpace(ghProxy))
                {
                    WebProxy proxy = new WebProxy(ghProxy.Trim(), true);
                    proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    ghReq.Proxy = proxy;
                }

                using (StreamWriter sw = new StreamWriter(ghReq.GetRequestStream()))
                {
                    sw.Write(serializer.Serialize(ghPayload));
                }

                using (HttpWebResponse resp = (HttpWebResponse)ghReq.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    IDictionary raw = serializer.DeserializeObject(sr.ReadToEnd()) as IDictionary;
                    IList choices = raw != null ? raw["choices"] as IList : null;
                    if (choices != null && choices.Count > 0)
                    {
                        IDictionary c0 = choices[0] as IDictionary;
                        IDictionary msg = c0 != null ? c0["message"] as IDictionary : null;
                        if (msg != null && msg["content"] != null)
                        {
                            reply = msg["content"].ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(reply)) return true;
                        }
                    }
                }
            }
        }
        catch { }

        return false;
    }

    private static string BuildDeterministicExecutiveSummary(
        string prompt,
        string scopeLabel,
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        List<CmfComponentMetricsSnapshot> componentMetrics,
        List<CmfPendingIssueRecord> pendingIssues,
        CmfPortalMultiTabContext multiTab,
        bool wantsPptx,
        string dashboardContext)
    {
        StringBuilder sb = new StringBuilder();
        string lowerPrompt = (prompt ?? string.Empty).ToLowerInvariant();

        // 0. Check if user asked about a specific sighting or promoted ID across the entire Issue List / CMF Portal
        Match idMatch = Regex.Match(prompt ?? string.Empty, @"\b(\d{8,14})\b");
        if (idMatch.Success && !wantsPptx)
        {
            string askedId = idMatch.Groups[1].Value;
            CmfPendingIssueRecord item = (multiTab != null) ? multiTab.MatchedSpecificIssue : null;
            if (item == null && pendingIssues != null)
            {
                foreach (CmfPendingIssueRecord pItem in pendingIssues)
                {
                    if (string.Equals(pItem.SightingId, askedId, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(pItem.PromotedId, askedId, StringComparison.OrdinalIgnoreCase))
                    {
                        item = pItem;
                        break;
                    }
                }
            }

            if (item != null)
            {
                sb.AppendFormat("- **Sighting {0} ({1} | Promoted ID: {2}):** *{3}*.\n",
                    item.SightingId, item.PlatformLabel, string.IsNullOrWhiteSpace(item.PromotedId) ? "NA" : item.PromotedId, item.Title);
                sb.AppendFormat("- **Timeline & status:** Current status is **{0}** (Request tag: **{1}**, Priority: **{2}**); submitted on **{3}** (WW: **{4}**), CMF Ask date: **{5}**, active/open for **{6}** days.\n",
                    item.Status, item.CmfRequest, item.Priority, item.SubmittedDate, item.WorkweekSubmitted, item.DateCmfAsk, item.DaysOpen);
                sb.AppendFormat("- **Component, owner & customer:** Tracked under **{0}** (Owner: **{1}**, OEM: **{2}**, Customer design: **{3}**, Driver/Milestone: **{4}**).\n",
                    item.Component, item.Owner, string.IsNullOrWhiteSpace(item.CustomerCompany) ? "N/A" : item.CustomerCompany, item.CustomerDetail, item.Drivers);
                sb.AppendFormat("- **Impact & reproducibility:** Impact is **{0}**; reproducibility is **{1}** (RVP repro: **{2}**).\n",
                    item.Impact, HumanizeReproForPulse(item.Reproducibility), item.ReproOnRvp);
                return sb.ToString().Trim();
            }
        }

        // 1. Check if user asked about Config / CMF Rules tab (scoring weights, thresholds, or AI models)
        if (!wantsPptx && Regex.IsMatch(lowerPrompt, @"\b(scoring\s+rule|scoring\s+rules|config\s+tab|cmf\s+rules|dimension\s+weight|weights|threshold|thresholds|which\s+model|ai\s+model|gnai\s+model)\b"))
        {
            CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
            if (cfg != null)
            {
                sb.AppendFormat("- **Config / CMF Rules tab -- AI Models:** Primary recommendation model is **{0}**, fast summary model is **{1}**, and fallback chain is **{2}**.\n",
                    cfg.PrimaryModel, cfg.FastSummaryModel, cfg.FallbackModels != null ? string.Join(" -> ", cfg.FallbackModels.ToArray()) : "none");
                sb.AppendFormat("- **CMF disposition thresholds (4-Point Scale):** Sightings scoring **>= {0:0.00}/4** (with `B >= 3` and `D >= 3`) qualify for **CMF_OK**, scores **2.75 to 3.49/4** (or capped by a guardrail such as `B <= 2` or `D <= 2`) qualify for **CMF_REVIEW**, and scores **< {1:0.00}/4** map to **CMF_REJECT**.\n",
                    cfg.CmfOkMinScore > 0 ? cfg.CmfOkMinScore : 3.50, cfg.CmfReviewMinScore > 0 ? cfg.CmfReviewMinScore : 2.75);
                sb.AppendLine("- **6-dimension scoring rubric (1 to 4 scale):** Evaluates **Severity (S: 20%)**, **Occurrence (O: 15%)**, **Detection (D: 15%)**, **Recovery (R: 10%)**, **Business Impact (B: 20%)**, and **Customer Experience (CX: 20%)**.");
                return sb.ToString().Trim();
            }
        }

        // 2. Check if user asked about recent/oldest issues in CMF Pending or Issue List
        if (IsSpecificIssueOrPendingItemQuery(prompt))
        {
            bool explicitlyIssueList = Regex.IsMatch(lowerPrompt, @"\b(issue\s+list|submitted|all\s+issues|overall\s+list)\b") && !Regex.IsMatch(lowerPrompt, @"\b(pending|cmf_ask)\b");
            if (explicitlyIssueList && multiTab != null && multiTab.RecentAllIssues.Count > 0)
            {
                CmfPendingIssueRecord latestAll = multiTab.RecentAllIssues[0];
                sb.AppendFormat("- **Most recently submitted issue in Issue List ({0}):** Sighting **{1}** (Promoted ID: **{2}**) -- *{3}*.\n",
                    latestAll.PlatformLabel, latestAll.SightingId, latestAll.PromotedId, latestAll.Title);
                sb.AppendFormat("- **Submission & lifecycle status:** Submitted on **{0}** (Workweek: **{1}**), currently in **{2}** status with CMF tag **{3}** (Priority: **{4}**, Days open: **{5}**).\n",
                    latestAll.SubmittedDate, latestAll.WorkweekSubmitted, latestAll.Status, latestAll.CmfRequest, latestAll.Priority, latestAll.DaysOpen);
                sb.AppendFormat("- **Component, owner & customer:** Component **{0}**, assigned to **{1}** (OEM: **{2}**, Design: **{3}**).\n",
                    latestAll.Component, latestAll.Owner, latestAll.CustomerCompany, latestAll.CustomerDetail);
                return sb.ToString().Trim();
            }

            if (pendingIssues != null && pendingIssues.Count > 0)
            {
                if (Regex.IsMatch(lowerPrompt, @"\b(list\s+all|list\s+the|what\s+are\s+the\s+pending\s+issues|show\s+pending\s+issues)\b"))
                {
                    sb.AppendFormat("- **CMF Pending list ({0}):** Showing the **{1}** most recent open CMF Pending issues ordered by CMF Ask Date:\n",
                        scopeLabel, Math.Min(5, pendingIssues.Count));
                    for (int i = 0; i < pendingIssues.Count && i < 5; i++)
                    {
                        CmfPendingIssueRecord p = pendingIssues[i];
                        sb.AppendFormat("- **#{0} Sighting {1}** (Added: **{2}** | **{3}** | Owner: **{4}**): *{5}*\n",
                            i + 1, p.SightingId, p.DateCmfAsk, p.Component, p.Owner, p.Title);
                    }
                    return sb.ToString().Trim();
                }

                bool wantsOldest = Regex.IsMatch(lowerPrompt, @"\b(oldest|first|longest)\b");
                CmfPendingIssueRecord target = wantsOldest ? pendingIssues[pendingIssues.Count - 1] : pendingIssues[0];
                string labelPrefix = wantsOldest ? "Oldest" : "Most recent";

                sb.AppendFormat("- **{0} CMF Pending issue ({1}):** Sighting **{2}** (Promoted ID: **{3}**) -- *{4}*.\n",
                    labelPrefix, target.PlatformLabel, target.SightingId, string.IsNullOrWhiteSpace(target.PromotedId) ? "NA" : target.PromotedId, target.Title);
                sb.AppendFormat("- **When it entered the CMF Pending list:** Added to the CMF Pending list on **{0}** (originally submitted on **{1}**, active for **{2}** days).\n",
                    target.DateCmfAsk, target.SubmittedDate, target.DaysOpen);
                sb.AppendFormat("- **Component, owner & customer:** Tracked under **{0}** (Owner: **{1}**, Customer configuration: **{2}**, Request status: **{3}**).\n",
                    target.Component, target.Owner, target.CustomerDetail, target.CmfRequest);
                sb.AppendFormat("- **Impact & reproducibility:** Impact is recorded as **{0}**; reproducibility is **{1}** (RVP repro: **{2}**).\n",
                    target.Impact, HumanizeReproForPulse(target.Reproducibility), target.ReproOnRvp);
                return sb.ToString().Trim();
            }
        }

        // 3. Check if user asked about specific multi-tab dimensions (TPT, Design/Freeze, OEM/Customer, Owner, Driver/Milestone, Priority/Regression, Closed Reason, Workweek, Pending Age)
        if (!wantsPptx && multiTab != null)
        {
            if (Regex.IsMatch(lowerPrompt, @"\b(tpt|turnaround|disposition\s+time|resolution\s+time)\b") && platformMetrics.Count > 0)
            {
                foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
                {
                    sb.AppendFormat("- **Turnaround Time (TPT) for {0} (CMF Summary tab):** Average Disposition TPT is **{1} days**, Average Resolution TPT is **{2} days**, and Average Critical Active TPT is **{3} days** across **{4}** official CMF approved items.\n",
                        p.PlatformLabel, p.DispTptDays, p.ResolveTptDays, p.CritTptDays, p.OfficialCmfApprovedCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(design|designs|sw\s+image\s+freeze|image\s+freeze|freeze\s+date)\b") && multiTab.TopDesigns.Count > 0)
            {
                sb.AppendFormat("- **Customer Design & SW Image Freeze summary ({0} -- CMF Summary tab):**\n", scopeLabel);
                for (int i = 0; i < multiTab.TopDesigns.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.TopDesigns[i];
                    sb.AppendFormat("- **{0}** (SW Image Freeze: **{1}**): **{2}** total sightings (**{3}** open, **{4}** pending `CMF_ASK`, **{5}** approved `CMF_OK`, **{6}** implemented/verified).\n",
                        d.Name, string.IsNullOrWhiteSpace(d.ExtraInfo) ? "N/A" : d.ExtraInfo, d.TotalCount, d.OpenCount, d.PendingAskCount, d.ApprovedOkCount, d.ImplementedCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(oem|oems|customer\s+company|which\s+customer|top\s+customer|top\s+customers|lenovo|hp|dell|asus|acer|samsung|microsoft|lcik)\b") && multiTab.TopCustomersOem.Count > 0)
            {
                sb.AppendFormat("- **Customer / OEM breakdown ({0} -- OEM Summary & Issue List tabs):**\n", scopeLabel);
                for (int i = 0; i < multiTab.TopCustomersOem.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.TopCustomersOem[i];
                    sb.AppendFormat("- **{0}:** **{1}** total sightings (**{2}** open, **{3}** pending `CMF_ASK`, **{4}** approved `CMF_OK`, **{5}** implemented/verified, **{6}** rejected).\n",
                        d.Name, d.TotalCount, d.OpenCount, d.PendingAskCount, d.ApprovedOkCount, d.ImplementedCount, d.RejectedCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(who\s+owns|top\s+owner|top\s+owners|owner\s+breakdown|owners|assigned\s+to)\b") && multiTab.TopOwners.Count > 0)
            {
                sb.AppendFormat("- **Top issue owners ({0} -- Issue List & CMF Pending tabs):**\n", scopeLabel);
                for (int i = 0; i < multiTab.TopOwners.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.TopOwners[i];
                    sb.AppendFormat("- **{0}:** **{1}** total sightings (**{2}** open, **{3}** pending `CMF_ASK`, **{4}** approved `CMF_OK`, **{5}** implemented/verified).\n",
                        d.Name, d.TotalCount, d.OpenCount, d.PendingAskCount, d.ApprovedOkCount, d.ImplementedCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(driver|drivers|milestone|milestones|los|line\s+of\s+sight)\b") && multiTab.TopDriversMilestones.Count > 0)
            {
                int hasLos = 0, missingLos = 0;
                foreach (CmfPlatformMetricsSnapshot p in platformMetrics) { hasLos += p.HasLosCount; missingLos += p.MissingLosCount; }
                sb.AppendFormat("- **Driver / Milestone & Line of Sight (LOS) summary ({0} -- CMF Summary & Dashboard tabs):** Among open sightings, **{1}** have an active Line of Sight (LOS) and **{2}** are missing LOS.\n",
                    scopeLabel, hasLos, missingLos);
                for (int i = 0; i < multiTab.TopDriversMilestones.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.TopDriversMilestones[i];
                    sb.AppendFormat("- **Driver / Milestone {0}:** **{1}** total sightings (**{2}** open, **{3}** implemented/verified, **{4}** `CMF_OK`, **{5}** `CMF_ASK`).\n",
                        d.Name, d.TotalCount, d.OpenCount, d.ImplementedCount, d.ApprovedOkCount, d.PendingAskCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(priority|priorities|showstopper|showstoppers|p1|p2|regression|regressions|promoted)\b") && multiTab.PriorityBreakdown.Count > 0)
            {
                int regTotal = 0, promTotal = 0, highPrioTotal = 0;
                foreach (CmfPlatformMetricsSnapshot p in platformMetrics) { regTotal += p.RegressionCount; promTotal += p.PromotedCount; highPrioTotal += p.HighPriorityCount; }
                sb.AppendFormat("- **Priority, Regression & Promoted Bug summary ({0} -- Issue List & Dashboard tabs):** **{1}** high-priority/P1/showstopper sightings, **{2}** regressions, and **{3}** sightings with promoted bug IDs.\n",
                    scopeLabel, highPrioTotal, regTotal, promTotal);
                for (int i = 0; i < multiTab.PriorityBreakdown.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.PriorityBreakdown[i];
                    sb.AppendFormat("- **Priority {0}:** **{1}** total sightings (**{2}** open, **{3}** `CMF_OK`, **{4}** `CMF_ASK`, **{5}** implemented/verified).\n",
                        d.Name, d.TotalCount, d.OpenCount, d.ApprovedOkCount, d.PendingAskCount, d.ImplementedCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(closed\s+reason|closed\s+reasons|noise|why\s+were\s+issues\s+closed)\b") && multiTab.ClosedReasonBreakdown.Count > 0)
            {
                sb.AppendFormat("- **Top Closed Reasons ({0} -- CMF Summary & Reports tabs):**\n", scopeLabel);
                for (int i = 0; i < multiTab.ClosedReasonBreakdown.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.ClosedReasonBreakdown[i];
                    sb.AppendFormat("- **{0}:** **{1}** closed sightings (**{2}** `CMF_OK`, **{3}** `CMF_REJECT`).\n",
                        d.Name, d.TotalCount, d.ApprovedOkCount, d.RejectedCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(workweek|ww\s*\d*|submission\s+trend|submitted\s+by\s+week)\b") && multiTab.WorkweekBreakdown.Count > 0)
            {
                sb.AppendFormat("- **Workweek submission breakdown ({0} -- Reports & Issue List tabs):**\n", scopeLabel);
                for (int i = 0; i < multiTab.WorkweekBreakdown.Count && i < 5; i++)
                {
                    CmfPortalDimensionItem d = multiTab.WorkweekBreakdown[i];
                    sb.AppendFormat("- **Workweek {0}:** **{1}** sightings submitted (**{2}** open, **{3}** `CMF_OK`, **{4}** `CMF_ASK`).\n",
                        d.Name, d.TotalCount, d.OpenCount, d.ApprovedOkCount, d.PendingAskCount);
                }
                return sb.ToString().Trim();
            }

            if (Regex.IsMatch(lowerPrompt, @"\b(age\s+bucket|aging|stale\s+pending|how\s+long\s+in\s+pending)\b"))
            {
                sb.AppendFormat("- **CMF Pending Age Buckets ({0} -- CMF Pending tab):**\n", scopeLabel);
                sb.AppendFormat("- **0 to 3 days:** **{0}** pending issues.\n", multiTab.PendingAge0To3Days);
                sb.AppendFormat("- **4 to 7 days:** **{0}** pending issues.\n", multiTab.PendingAge4To7Days);
                sb.AppendFormat("- **8 to 14 days:** **{0}** pending issues.\n", multiTab.PendingAge8To14Days);
                sb.AppendFormat("- **Over 14 days (Stale):** **{0}** pending issues.\n", multiTab.PendingAgeOver14Days);
                return sb.ToString().Trim();
            }
        }

        int total = 0, ask = 0, ok = 0, inc = 0, dup = 0, rej = 0;
        foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
        {
            total += p.TotalCount;
            ask += p.PendingAskCount;
            ok += p.ApprovedOkCount;
            inc += p.IncompleteCount;
            dup += p.DuplicateCount;
            rej += p.RejectedCount;
        }

        if (platformMetrics.Count == 0 && !string.IsNullOrWhiteSpace(dashboardContext))
        {
            sb.AppendLine("- **Live dashboard view:** Based on the currently active dashboard view: " + dashboardContext.Trim());
            return sb.ToString().Trim();
        }

        sb.AppendFormat("- **Overall summary ({0}):** There are **{1}** total sightings in the database for this scope -- **{2}** approved as must-fix (`CMF_OK`), **{3}** waiting for review (`CMF_ASK`), **{4}** needing more information (`CMF_INCOMPLETE`), **{5}** marked as duplicates (`CMF_DUPLICATE`), and **{6}** rejected (`CMF_REJECT`).\n",
            scopeLabel, total, ok, ask, inc, dup, rej);

        if (platformMetrics.Count >= 2)
        {
            string familyBreakdownBullet = BuildFamilyRollupAndSkuSummaryBullet(platformMetrics);
            if (!string.IsNullOrWhiteSpace(familyBreakdownBullet))
            {
                sb.AppendLine(familyBreakdownBullet);
            }

            List<string> pParts = new List<string>();
            for (int i = 0; i < platformMetrics.Count; i++)
            {
                CmfPlatformMetricsSnapshot p = platformMetrics[i];
                pParts.Add(string.Format(
                    "**{0}** has **{1}** total issues (**{2}** pending review, **{3}** approved `CMF_OK`, **{4}** incomplete, **{5}** duplicates)",
                    p.PlatformLabel, p.TotalCount, p.PendingAskCount, p.ApprovedOkCount, p.IncompleteCount, p.DuplicateCount));
            }
            sb.AppendFormat("- **Platform-by-platform SKU comparison ({0}):** {1}.\n", scopeLabel, string.Join("; ", pParts.ToArray()));
        }

        if (componentMetrics.Count > 0)
        {
            CmfComponentMetricsSnapshot requestedComp = FindMentionedComponent(prompt, componentMetrics);
            if (requestedComp != null)
            {
                sb.AppendFormat("- **Requested component spotlight ({0}):** **{0}** has **{1}** total sightings across this scope, including **{2}** duplicates (`CMF_DUPLICATE`), **{3}** approved (`CMF_OK`), **{4}** pending review (`CMF_ASK`), and **{5}** incomplete (`CMF_INCOMPLETE`).\n",
                    requestedComp.ComponentName, requestedComp.TotalCount, requestedComp.DuplicateCount, requestedComp.ApprovedOkCount, requestedComp.PendingAskCount, requestedComp.IncompleteCount);

                string compByFamilySku = BuildFamilyAndSkuComponentBreakdownBullet(requestedComp.ComponentName, platformMetrics, multiTab);
                if (!string.IsNullOrWhiteSpace(compByFamilySku))
                {
                    sb.AppendLine(compByFamilySku);
                }
            }

            CmfComponentMetricsSnapshot topC = componentMetrics[0];
            sb.AppendFormat("- **Top component comparison:** **{0}** has the highest count (**{1}** total, **{2}** `CMF_DUPLICATE`, **{3}** `CMF_OK`, **{4}** `CMF_ASK`)",
                topC.ComponentName, topC.TotalCount, topC.DuplicateCount, topC.ApprovedOkCount, topC.PendingAskCount);
            if (componentMetrics.Count > 1)
            {
                CmfComponentMetricsSnapshot secondC = componentMetrics[1];
                sb.AppendFormat(", followed by **{0}** (**{1}** total, **{2}** `CMF_DUPLICATE`, **{3}** `CMF_OK`).\n",
                    secondC.ComponentName, secondC.TotalCount, secondC.DuplicateCount, secondC.ApprovedOkCount);
            }
            else
            {
                sb.AppendLine(".");
            }
        }

        if (wantsPptx)
        {
            sb.AppendLine("- **Executive PowerPoint ready:** A 4-slide widescreen **Executive PowerPoint (`.pptx`)** presentation with KPI scorecards, plain-English highlights, vector comparison charts, and data tables has been generated below.");
        }

        return sb.ToString().Trim();
    }

    private static string BuildFamilyRollupAndSkuContext(
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        CmfPortalMultiTabContext multiTab,
        CmfComponentMetricsSnapshot requestedComp)
    {
        if (platformMetrics == null || platformMetrics.Count == 0) return string.Empty;
        StringBuilder sb = new StringBuilder();
        string famSummary = BuildFamilyRollupAndSkuSummaryBullet(platformMetrics);
        if (!string.IsNullOrWhiteSpace(famSummary)) sb.AppendLine(famSummary);
        if (requestedComp != null)
        {
            string compFamSummary = BuildFamilyAndSkuComponentBreakdownBullet(requestedComp.ComponentName, platformMetrics, multiTab);
            if (!string.IsNullOrWhiteSpace(compFamSummary)) sb.AppendLine(compFamSummary);
        }
        return sb.ToString().Trim();
    }

    private static string BuildFamilyRollupAndSkuSummaryBullet(List<CmfPlatformMetricsSnapshot> platformMetrics)
    {
        if (platformMetrics == null || platformMetrics.Count < 2) return string.Empty;
        List<string> famOrder = new List<string>();
        Dictionary<string, List<CmfPlatformMetricsSnapshot>> famMap = new Dictionary<string, List<CmfPlatformMetricsSnapshot>>(StringComparer.OrdinalIgnoreCase);

        foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
        {
            if (p == null) continue;
            string fam = ExtractFamilyPrefix(p.PlatformLabel);
            List<CmfPlatformMetricsSnapshot> list;
            if (!famMap.TryGetValue(fam, out list))
            {
                list = new List<CmfPlatformMetricsSnapshot>();
                famMap[fam] = list;
                famOrder.Add(fam);
            }
            list.Add(p);
        }

        List<string> famParts = new List<string>();
        foreach (string fam in famOrder)
        {
            List<CmfPlatformMetricsSnapshot> skus = famMap[fam];
            int famTotal = 0, famAsk = 0, famOk = 0;
            List<string> skuStrings = new List<string>();
            foreach (CmfPlatformMetricsSnapshot s in skus)
            {
                famTotal += s.TotalCount;
                famAsk += s.PendingAskCount;
                famOk += s.ApprovedOkCount;
                skuStrings.Add(string.Format("**{0}** is **{1}** ({2} `CMF_OK`, {3} `CMF_ASK`)", s.PlatformLabel, s.TotalCount, s.ApprovedOkCount, s.PendingAskCount));
            }
            famParts.Add(string.Format(
                "Total **{0}** issues are **{1}** (**{2}** `CMF_OK`, **{3}** `CMF_ASK` -- SKU breakdown: {4})",
                FormatFamilyDisplayName(fam), famTotal, famOk, famAsk, string.Join(", ", skuStrings.ToArray())));
        }

        return "- **Family totals & individual SKU breakdown:** " + string.Join("; ", famParts.ToArray()) + ".";
    }

    private static string BuildFamilyAndSkuComponentBreakdownBullet(
        string componentName,
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        CmfPortalMultiTabContext multiTab)
    {
        if (string.IsNullOrWhiteSpace(componentName) || platformMetrics == null || platformMetrics.Count < 2 || multiTab == null || multiTab.PlatformComponentMap == null)
        {
            return string.Empty;
        }

        List<string> famOrder = new List<string>();
        Dictionary<string, List<CmfPlatformMetricsSnapshot>> famMap = new Dictionary<string, List<CmfPlatformMetricsSnapshot>>(StringComparer.OrdinalIgnoreCase);
        foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
        {
            if (p == null) continue;
            string fam = ExtractFamilyPrefix(p.PlatformLabel);
            List<CmfPlatformMetricsSnapshot> list;
            if (!famMap.TryGetValue(fam, out list))
            {
                list = new List<CmfPlatformMetricsSnapshot>();
                famMap[fam] = list;
                famOrder.Add(fam);
            }
            list.Add(p);
        }

        List<string> famParts = new List<string>();
        foreach (string fam in famOrder)
        {
            List<CmfPlatformMetricsSnapshot> skus = famMap[fam];
            int famCompTotal = 0, famCompOk = 0, famCompAsk = 0, famCompDup = 0;
            List<string> skuStrings = new List<string>();
            foreach (CmfPlatformMetricsSnapshot s in skus)
            {
                Dictionary<string, CmfComponentMetricsSnapshot> perPlat;
                CmfComponentMetricsSnapshot cSnap = null;
                if (multiTab.PlatformComponentMap.TryGetValue(s.PlatformLabel, out perPlat) && perPlat != null)
                {
                    perPlat.TryGetValue(componentName, out cSnap);
                }
                int cTotal = cSnap != null ? cSnap.TotalCount : 0;
                int cOk = cSnap != null ? cSnap.ApprovedOkCount : 0;
                int cAsk = cSnap != null ? cSnap.PendingAskCount : 0;
                int cDup = cSnap != null ? cSnap.DuplicateCount : 0;
                famCompTotal += cTotal;
                famCompOk += cOk;
                famCompAsk += cAsk;
                famCompDup += cDup;
                skuStrings.Add(string.Format("**{0}** is **{1}** ({2} `CMF_OK`, {3} `CMF_ASK`, {4} `CMF_DUPLICATE`)", s.PlatformLabel, cTotal, cOk, cAsk, cDup));
            }
            famParts.Add(string.Format(
                "Total **{0}** issues for **{1}** are **{2}** (**{3}** `CMF_OK`, **{4}** `CMF_ASK`, **{5}** `CMF_DUPLICATE` -- SKU breakdown: {6})",
                FormatFamilyDisplayName(fam), componentName, famCompTotal, famCompOk, famCompAsk, famCompDup, string.Join(", ", skuStrings.ToArray())));
        }

        return "- **Component comparison by family & SKU (" + componentName + "):** " + string.Join("; ", famParts.ToArray()) + ".";
    }

    private static CmfComponentMetricsSnapshot FindMentionedComponent(string prompt, List<CmfComponentMetricsSnapshot> componentMetrics)
    {
        if (string.IsNullOrWhiteSpace(prompt) || componentMetrics == null) return null;
        string lower = prompt.ToLowerInvariant();

        // 1. Check configured ComponentSynonymJargons from cmf-assistant-config.json
        try
        {
            CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
            if (cfg != null && cfg.ComponentSynonymJargons != null)
            {
                foreach (KeyValuePair<string, List<string>> entry in cfg.ComponentSynonymJargons)
                {
                    if (entry.Value == null) continue;
                    bool matchedAlias = false;
                    foreach (string alias in entry.Value)
                    {
                        if (!string.IsNullOrWhiteSpace(alias)
                            && (Regex.IsMatch(lower, @"(?<![a-z0-9])" + Regex.Escape(alias.ToLowerInvariant()) + @"(?![a-z0-9])")
                                || IsFuzzyTokenMatch(lower, alias.ToLowerInvariant())))
                        {
                            matchedAlias = true;
                            break;
                        }
                    }
                    if (matchedAlias)
                    {
                        foreach (CmfComponentMetricsSnapshot c in componentMetrics)
                        {
                            if (c != null && !string.IsNullOrWhiteSpace(c.ComponentName)
                                && (string.Equals(c.ComponentName, entry.Key, StringComparison.OrdinalIgnoreCase)
                                    || c.ComponentName.IndexOf(entry.Key, StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                return c;
                            }
                        }
                    }
                }
            }
        }
        catch { }

        // 2. Direct component name match
        foreach (CmfComponentMetricsSnapshot c in componentMetrics)
        {
            if (c == null || string.IsNullOrWhiteSpace(c.ComponentName)) continue;
            string cLower = c.ComponentName.ToLowerInvariant();
            if (lower.Contains(cLower)) return c;
            if ((lower.Contains("graphics") || lower.Contains("igfx") || lower.Contains("display")) && cLower.Contains("igfx")) return c;
            if ((lower.Contains("thunderbolt") || lower.Contains("usb-c") || lower.Contains("tbt")) && cLower.Contains("tbt")) return c;
        }
        return null;
    }

    private static CmfExecutiveDeckSpec BuildExecutiveDeckSpec(
        string prompt,
        string scopeLabel,
        string aiAnswer,
        List<CmfPlatformMetricsSnapshot> platformMetrics,
        List<CmfComponentMetricsSnapshot> componentMetrics,
        List<string[]> topIssuesRows,
        CmfPortalMultiTabContext multiTab = null,
        CmfPulseReportTemplate activeTemplate = null)
    {
        int total = 0, ask = 0, ok = 0, inc = 0, dup = 0, openCount = 0;
        foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
        {
            total += p.TotalCount;
            ask += p.PendingAskCount;
            ok += p.ApprovedOkCount;
            inc += p.IncompleteCount;
            dup += p.DuplicateCount;
            openCount += p.OpenCount;
        }

        List<CmfExecutiveKpiItem> kpis = new List<CmfExecutiveKpiItem>
        {
            new CmfExecutiveKpiItem { Label = "TOTAL CMF SIGHTINGS", Value = total.ToString(), Subtext = openCount + " currently active/open", AccentHex = "0F5EA8" },
            new CmfExecutiveKpiItem { Label = "CMF PENDING (ASK)", Value = ask.ToString(), Subtext = "Awaiting CMF disposition", AccentHex = "F59E0B" },
            new CmfExecutiveKpiItem { Label = "CMF APPROVED (OK)", Value = ok.ToString(), Subtext = "Approved must-fix items", AccentHex = "10B981" },
            new CmfExecutiveKpiItem { Label = "CMF INCOMPLETE", Value = inc.ToString(), Subtext = "Needs missing repro/impact info", AccentHex = "3B82F6" },
            new CmfExecutiveKpiItem { Label = "CMF DUPLICATE", Value = dup.ToString(), Subtext = "Linked to primary sightings", AccentHex = "8B5CF6" }
        };

        List<string> bullets = new List<string>();
        foreach (string line in (aiAnswer ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed)) bullets.Add(trimmed);
        }

        List<string> highlights = new List<string>();
        if (platformMetrics.Count >= 2)
        {
            highlights.Add(platformMetrics[0].PlatformLabel + " carries " + platformMetrics[0].PendingAskCount + " pending asks & " + platformMetrics[0].ApprovedOkCount + " approved CMFs vs. " + platformMetrics[1].PlatformLabel + " (" + platformMetrics[1].PendingAskCount + " pending, " + platformMetrics[1].ApprovedOkCount + " approved).");
        }
        else if (platformMetrics.Count == 1)
        {
            highlights.Add(platformMetrics[0].PlatformLabel + " has " + platformMetrics[0].PendingAskCount + " pending CMF asks and " + platformMetrics[0].IncompleteCount + " incomplete tickets requiring submitter follow-up.");
        }

        if (componentMetrics.Count > 0)
        {
            highlights.Add("Top component driver is " + componentMetrics[0].ComponentName + " (" + componentMetrics[0].TotalCount + " total, " + componentMetrics[0].DuplicateCount + " CMF_DUPLICATE, " + componentMetrics[0].PendingAskCount + " pending).");
        }
        highlights.Add("Prioritize disposition of the " + ask + " CMF_ASK items and resolve evidence gaps on the " + inc + " CMF_INCOMPLETE tickets to accelerate milestone sign-off.");

        List<CmfExecutiveChartSeriesItem> chartItems = new List<CmfExecutiveChartSeriesItem>();
        List<string[]> matrixRows = new List<string[]>();
        List<string> tableHeaders = new List<string> { "Platform", "Total", "CMF_ASK", "CMF_OK", "Incomp", "Duplicate" };
        string focusMode = activeTemplate != null && !string.IsNullOrWhiteSpace(activeTemplate.FocusMode) ? activeTemplate.FocusMode : "platform_comparison";

        if (string.Equals(focusMode, "oem_freeze_readiness", StringComparison.OrdinalIgnoreCase) && multiTab != null && multiTab.TopDesigns != null && multiTab.TopDesigns.Count > 0)
        {
            tableHeaders = new List<string> { "Customer Design", "SW Freeze", "Total", "Open", "CMF_ASK", "CMF_OK" };
            for (int i = 0; i < multiTab.TopDesigns.Count && i < 6; i++)
            {
                CmfPortalDimensionItem d = multiTab.TopDesigns[i];
                chartItems.Add(new CmfExecutiveChartSeriesItem
                {
                    Category = d.Name.Length > 18 ? d.Name.Substring(0, 18) : d.Name,
                    PrimaryValue = d.PendingAskCount,
                    SecondaryValue = d.ApprovedOkCount,
                    TertiaryValue = d.ImplementedCount
                });
                matrixRows.Add(new[]
                {
                    d.Name,
                    string.IsNullOrWhiteSpace(d.ExtraInfo) ? "N/A" : d.ExtraInfo,
                    d.TotalCount.ToString(),
                    d.OpenCount.ToString(),
                    d.PendingAskCount.ToString(),
                    d.ApprovedOkCount.ToString()
                });
            }
        }
        else if (string.Equals(focusMode, "component_owner_triage", StringComparison.OrdinalIgnoreCase) && multiTab != null && multiTab.TopOwners != null && multiTab.TopOwners.Count > 0)
        {
            tableHeaders = new List<string> { "Owner", "Total", "Open", "CMF_ASK", "CMF_OK", "Impl/Ver" };
            for (int i = 0; i < multiTab.TopOwners.Count && i < 6; i++)
            {
                CmfPortalDimensionItem o = multiTab.TopOwners[i];
                chartItems.Add(new CmfExecutiveChartSeriesItem
                {
                    Category = o.Name,
                    PrimaryValue = o.PendingAskCount,
                    SecondaryValue = o.ApprovedOkCount,
                    TertiaryValue = o.ImplementedCount
                });
                matrixRows.Add(new[]
                {
                    o.Name,
                    o.TotalCount.ToString(),
                    o.OpenCount.ToString(),
                    o.PendingAskCount.ToString(),
                    o.ApprovedOkCount.ToString(),
                    o.ImplementedCount.ToString()
                });
            }
        }
        else
        {
            foreach (CmfPlatformMetricsSnapshot p in platformMetrics)
            {
                chartItems.Add(new CmfExecutiveChartSeriesItem
                {
                    Category = p.PlatformLabel,
                    PrimaryValue = p.PendingAskCount,
                    SecondaryValue = p.ApprovedOkCount,
                    TertiaryValue = p.IncompleteCount + p.DuplicateCount
                });
                matrixRows.Add(new[]
                {
                    p.PlatformLabel,
                    p.TotalCount.ToString(),
                    p.PendingAskCount.ToString(),
                    p.ApprovedOkCount.ToString(),
                    p.IncompleteCount.ToString(),
                    p.DuplicateCount.ToString()
                });
            }
        }

        List<CmfExecutiveChartSeriesItem> compChartItems = new List<CmfExecutiveChartSeriesItem>();
        for (int i = 0; i < componentMetrics.Count && i < 5; i++)
        {
            CmfComponentMetricsSnapshot c = componentMetrics[i];
            compChartItems.Add(new CmfExecutiveChartSeriesItem
            {
                Category = c.ComponentName,
                PrimaryValue = c.PendingAskCount,
                SecondaryValue = c.ApprovedOkCount,
                TertiaryValue = c.DuplicateCount
            });
        }

        string deckTitle = (activeTemplate != null && !string.IsNullOrWhiteSpace(activeTemplate.Slide1Title))
            ? activeTemplate.Slide1Title + ": " + scopeLabel
            : "CMF Executive Trend & Disposition Review: " + scopeLabel;
        string slide3ChartTitle = (activeTemplate != null && !string.IsNullOrWhiteSpace(activeTemplate.Slide3Title))
            ? activeTemplate.Slide3Title + " (" + scopeLabel + ")"
            : "Platform CMF Disposition Comparison (" + scopeLabel + ")";
        string slide4Title = (activeTemplate != null && !string.IsNullOrWhiteSpace(activeTemplate.Slide4Title))
            ? activeTemplate.Slide4Title + " (" + scopeLabel + ")"
            : "Component Breakdown & Top Priority Sightings (" + scopeLabel + ")";

        return new CmfExecutiveDeckSpec
        {
            Title = deckTitle,
            Subtitle = (activeTemplate != null ? "[" + activeTemplate.Name + "] " : "") + "Query: \"" + (prompt.Length > 75 ? prompt.Substring(0, 72) + "..." : prompt) + "\"  |  Generated " + DateTime.UtcNow.ToString("dd MMM yyyy"),
            ScopeSummary = scopeLabel,
            ExecutiveTakeaway = bullets.Count > 0 ? bullets[0].Replace("**", string.Empty).TrimStart('-', ' ') : "Executive comparison of CMF pending, approved, incomplete, and duplicate trends.",
            Kpis = kpis,
            ExecutiveBullets = bullets,
            HighlightBoxes = highlights,
            ChartTitle = slide3ChartTitle,
            PrimarySeriesName = "CMF_ASK (Pending)",
            SecondarySeriesName = "CMF_OK (Approved)",
            TertiarySeriesName = "Incomplete + Duplicate",
            ChartItems = chartItems,
            ComparisonTableHeaders = tableHeaders,
            ComparisonTableRows = matrixRows,
            Slide4Title = slide4Title,
            ComponentChartItems = compChartItems,
            TopIssuesTableHeaders = new List<string> { "Platform", "Sighting ID", "Component", "CMF Tag", "Status", "Issue Title" },
            TopIssuesTableRows = topIssuesRows
        };
    }

    public static List<CmfPulseReportTemplate> GetPulseReportTemplates()
    {
        List<CmfPulseReportTemplate> list = new List<CmfPulseReportTemplate>
        {
            new CmfPulseReportTemplate
            {
                TemplateId = "default_executive",
                Name = "Executive Platform & Disposition Review (Default)",
                FocusMode = "platform_comparison",
                Slide1Title = "CMF Executive Trend & Disposition Review",
                Slide3Title = "Platform CMF Disposition Comparison",
                Slide4Title = "Component Breakdown & Top Priority Sightings",
                Blueprint = "Slide 1: KPI Scorecard; Slide 2: Executive Highlights; Slide 3: Platform CMF Comparison Chart & Matrix; Slide 4: Component Breakdown & Top Sightings.",
                IsCustom = false
            },
            new CmfPulseReportTemplate
            {
                TemplateId = "oem_freeze_readiness",
                Name = "OEM & SW Image Freeze Readiness Deck",
                FocusMode = "oem_freeze_readiness",
                Slide1Title = "OEM & Customer Design Freeze Readiness Review",
                Slide3Title = "Customer Design & SW Image Freeze Matrix",
                Slide4Title = "Component Risk & Top Customer Blocker Sightings",
                Blueprint = "Focus executive narrative on Customer Designs, SW Image Freeze dates, OEM readiness, and open CMF_OK/CMF_ASK risks.",
                IsCustom = false
            },
            new CmfPulseReportTemplate
            {
                TemplateId = "component_owner_triage",
                Name = "Component & Owner Bottleneck Triage Deck",
                FocusMode = "component_owner_triage",
                Slide1Title = "CMF Component & Owner Triage Review",
                Slide3Title = "Top Issue Owners & Disposition Backlog",
                Slide4Title = "Component Bottlenecks & Actionable Sightings",
                Blueprint = "Focus executive narrative on Component bottlenecks, Turnaround Time (TPT), Top Issue Owners, and pending CMF_ASK triage.",
                IsCustom = false
            }
        };

        string customPath = ResolvePulseTemplatesFilePath();
        if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
        {
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
                List<CmfPulseReportTemplate> customList = serializer.Deserialize<List<CmfPulseReportTemplate>>(File.ReadAllText(customPath, Encoding.UTF8));
                if (customList != null)
                {
                    foreach (CmfPulseReportTemplate ct in customList)
                    {
                        if (ct != null && !string.IsNullOrWhiteSpace(ct.TemplateId))
                        {
                            ct.IsCustom = true;
                            list.Add(ct);
                        }
                    }
                }
            }
            catch { }
        }

        return list;
    }

    public static CmfPulseReportTemplate SaveCustomPulseReportTemplate(string name, string blueprint, string focusMode = "platform_comparison")
    {
        string cleanName = (name ?? string.Empty).Trim();
        string cleanBlueprint = (blueprint ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleanName)) cleanName = "Custom Executive Template";
        string id = "custom_" + Regex.Replace(cleanName.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_') + "_" + DateTime.UtcNow.ToString("HHmmss");

        string slide1 = cleanName;
        string slide3 = "Custom Trend & Matrix Analysis";
        string slide4 = "Component Breakdown & Priority Sightings";

        Match s1Match = Regex.Match(cleanBlueprint, @"Slide\s*1\s*[:\-]\s*([^\r\n;]+)", RegexOptions.IgnoreCase);
        if (s1Match.Success) slide1 = s1Match.Groups[1].Value.Trim();
        Match s3Match = Regex.Match(cleanBlueprint, @"Slide\s*3\s*[:\-]\s*([^\r\n;]+)", RegexOptions.IgnoreCase);
        if (s3Match.Success) slide3 = s3Match.Groups[1].Value.Trim();
        Match s4Match = Regex.Match(cleanBlueprint, @"Slide\s*4\s*[:\-]\s*([^\r\n;]+)", RegexOptions.IgnoreCase);
        if (s4Match.Success) slide4 = s4Match.Groups[1].Value.Trim();

        if (string.IsNullOrWhiteSpace(focusMode) || focusMode == "auto")
        {
            if (Regex.IsMatch(cleanName + " " + cleanBlueprint, @"\b(oem|freeze|design)\b", RegexOptions.IgnoreCase)) focusMode = "oem_freeze_readiness";
            else if (Regex.IsMatch(cleanName + " " + cleanBlueprint, @"\b(owner|triage|bottleneck)\b", RegexOptions.IgnoreCase)) focusMode = "component_owner_triage";
            else focusMode = "platform_comparison";
        }

        CmfPulseReportTemplate created = new CmfPulseReportTemplate
        {
            TemplateId = id,
            Name = cleanName,
            FocusMode = focusMode,
            Slide1Title = slide1,
            Slide3Title = slide3,
            Slide4Title = slide4,
            Blueprint = cleanBlueprint,
            IsCustom = true
        };

        string path = ResolvePulseTemplatesFilePath();
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
            List<CmfPulseReportTemplate> existing = File.Exists(path)
                ? (serializer.Deserialize<List<CmfPulseReportTemplate>>(File.ReadAllText(path, Encoding.UTF8)) ?? new List<CmfPulseReportTemplate>())
                : new List<CmfPulseReportTemplate>();
            existing.RemoveAll(t => t != null && string.Equals(t.Name, cleanName, StringComparison.OrdinalIgnoreCase));
            existing.Insert(0, created);
            File.WriteAllText(path, serializer.Serialize(existing), Encoding.UTF8);
        }
        catch { }

        return created;
    }

    private static CmfPulseReportTemplate ResolveReportTemplate(string templateId, string prompt)
    {
        List<CmfPulseReportTemplate> all = GetPulseReportTemplates();
        if (!string.IsNullOrWhiteSpace(templateId))
        {
            foreach (CmfPulseReportTemplate t in all)
            {
                if (t != null && (string.Equals(t.TemplateId, templateId.Trim(), StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(t.Name, templateId.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    return t;
                }
            }
        }

        if (Regex.IsMatch(prompt ?? string.Empty, @"\b(oem|freeze|sw\s+image\s+freeze|design\s+readiness)\b", RegexOptions.IgnoreCase) && all.Count > 1)
        {
            return all[1];
        }
        if (Regex.IsMatch(prompt ?? string.Empty, @"\b(owner|owners|triage)\b", RegexOptions.IgnoreCase) && all.Count > 2)
        {
            return all[2];
        }
        return all.Count > 0 ? all[0] : null;
    }

    private static string ResolvePulseTemplatesFilePath()
    {
        try
        {
            string mapped = HostingEnvironment.MapPath("~/App_Data/reports-assistant/pulse-templates.json");
            if (!string.IsNullOrWhiteSpace(mapped)) return mapped;
        }
        catch { }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory ?? string.Empty;
        if (Directory.Exists(Path.Combine(baseDir, "cmf_portal", "App_Data")))
        {
            return Path.Combine(baseDir, "cmf_portal", "App_Data", "reports-assistant", "pulse-templates.json");
        }
        return Path.Combine(baseDir, "App_Data", "reports-assistant", "pulse-templates.json");
    }

    private static string ResolveGeneratedReportsDirectory()
    {
        try
        {
            string mapped = HostingEnvironment.MapPath("~/Content/generated-reports");
            if (!string.IsNullOrWhiteSpace(mapped)) return mapped;
        }
        catch { }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory ?? string.Empty;
        if (Directory.Exists(Path.Combine(baseDir, "cmf_portal")))
        {
            return Path.Combine(baseDir, "cmf_portal", "Content", "generated-reports");
        }
        return Path.Combine(baseDir, "Content", "generated-reports");
    }

    private static string HumanizeReproForPulse(string value)
    {
        string text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text)) return "not specified";
        string lower = text.ToLowerInvariant();
        if (lower.Contains("very_sporadic")) return "rare (<10% of tries)";
        if (lower.Contains("frequently_50")) return "frequent (50-90% of tries)";
        if (lower.Contains("sporadic_10")) return "occasional (10-40% of tries)";
        if (lower.Contains("always_100")) return "every time (100% reproducible)";
        if (lower == "once") return "seen only once";
        return text.Replace("_", " ");
    }

    private static string ResolveWebConfigAppSetting(string key)
    {
        string val = ConfigurationManager.AppSettings[key];
        if (!string.IsNullOrWhiteSpace(val)) return val.Trim();

        string[] candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "Web.config"),
            Path.Combine(Environment.CurrentDirectory ?? string.Empty, "Web.config"),
            Path.Combine(Environment.CurrentDirectory ?? string.Empty, "cmf_portal", "Web.config")
        };
        foreach (string path in candidates)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(path);
                    System.Xml.XmlNode node = doc.SelectSingleNode("//appSettings/add[@key='" + key + "']");
                    if (node != null && node.Attributes != null && node.Attributes["value"] != null)
                    {
                        return node.Attributes["value"].Value.Trim();
                    }
                }
            }
            catch { }
        }
        return string.Empty;
    }

    private static string ResolveDatabaseConnectionString()
    {
        return CmfAssistantConfigService.ResolveDatabaseConnectionString();
    }

    private static string CleanComponentLabel(string raw)
    {
        string text = (raw ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text)) return "Unassigned";
        text = Regex.Replace(text, @"^(?:platform\.)?(?:Common\.sw\.(?:win-driver|driver))?\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
        if (string.Equals(text, "iGFX", StringComparison.OrdinalIgnoreCase)) return "iGFX (Graphics)";
        return string.IsNullOrWhiteSpace(text) ? "Driver / SW" : text;
    }

    private static string ResolvePlatformDisplayLabel(string tableName)
    {
        CmfPlatformDefinition def;
        if (CmfPlatformCatalog.TryGetPlatformByTable(tableName, out def) && def != null && !string.IsNullOrWhiteSpace(def.Label))
        {
            return def.Label;
        }
        Dictionary<string, string> dynamicMap = GetAvailablePlatformTables();
        string dynamicLabel;
        if (!string.IsNullOrWhiteSpace(tableName) && dynamicMap.TryGetValue(tableName, out dynamicLabel) && !string.IsNullOrWhiteSpace(dynamicLabel))
        {
            return dynamicLabel;
        }
        return CmfPlatformCatalog.ResolvePlatformCode(tableName).Replace("_", "-");
    }

    private static int SafeInt(object value)
    {
        if (value == null || value == DBNull.Value) return 0;
        int parsed;
        return int.TryParse(Convert.ToString(value), out parsed) ? parsed : 0;
    }
}
