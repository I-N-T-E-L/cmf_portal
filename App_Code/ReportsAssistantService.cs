using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using System.Xml;

public class ReportsAssistantResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Intent { get; set; }
    public string ImageUrl { get; set; }
    public string ReportUrl { get; set; }
}

public static class ReportsAssistantService
{
    private const string ReportsContextSessionKey = "ReportsAssistantContextByPlatform";
    private const string AllCmfDataContextKey = "ALL_CMF_DATA";
    private const int MaxHsdRowsForReports = 40;
    private static readonly Dictionary<string, string> PlatformPromptMap = CmfPlatformCatalog.GetPromptAliasToTableMap();
    private static readonly Dictionary<string, string[]> PlatformGroupAliasMap = CmfPlatformCatalog.GetGroupAliasToTablesMap();
    private static readonly HashSet<string> AllowedPlatformTables = CmfPlatformCatalog.GetAllowedPlatformTables();

    public static ReportsAssistantResponse ProcessPrompt(string prompt, string platform)
    {
        string safePrompt = (prompt ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(safePrompt))
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Please enter a prompt for the reports assistant."
            };
        }

        string resolvedPlatform = ResolvePlatform(platform);
        List<string> promptPlatforms = ResolvePlatformsFromPrompt(safePrompt);
        List<string> reportPlatforms = ResolveReportPlatformScope(promptPlatforms, resolvedPlatform);
        string reportScopeKey = BuildReportScopeKey(reportPlatforms);

        if (reportPlatforms.Count == 0)
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Unable to resolve CMF data scope for reports assistant."
            };
        }

        string conversationContextJson = ReadConversationContext(reportScopeKey);
        bool hasPriorConversation = !string.IsNullOrWhiteSpace(conversationContextJson) && conversationContextJson != "{}";

        // Validate that the prompt is relevant to the CMF portal.
        // Allow short pronoun-based follow-ups when there is stored conversation context.
        string relevanceCheck = ValidatePortalRelevance(safePrompt);
        if (!string.IsNullOrWhiteSpace(relevanceCheck) && LooksLikeFollowUpPrompt(safePrompt) && hasPriorConversation)
        {
            relevanceCheck = string.Empty;
        }
        if (!string.IsNullOrWhiteSpace(relevanceCheck))
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = relevanceCheck
            };
        }

        string intent = ResolveIntent(safePrompt);
        string platformCode = BuildCombinedPlatformCode(reportPlatforms);
        if (string.IsNullOrWhiteSpace(platformCode)) platformCode = "ALL_CMF";
        if (string.IsNullOrWhiteSpace(conversationContextJson))
        {
            conversationContextJson = "{}";
        }

        // Resolve AI credentials: prefer GNAI token (env or config) + configured endpoint over GitHub PAT
        string resolvedGnaiToken = ResolveGnaiToken();
        string resolvedGnaiEndpoint = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_ENDPOINT"), GetAppSetting("GNAI:Endpoint"));
        string resolvedGnaiModel = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_MODEL"), GetAppSetting("GNAI:Model"));
        string resolvedGnaiProxy = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_PROXY"), GetAppSetting("GNAI:Proxy"));
        bool resolvedUseGnai = !string.IsNullOrWhiteSpace(resolvedGnaiToken)
            && !string.IsNullOrWhiteSpace(resolvedGnaiEndpoint)
            && !resolvedGnaiEndpoint.StartsWith("REPLACE");

        string githubEndpoint = resolvedUseGnai ? resolvedGnaiEndpoint : ConfigurationManager.AppSettings["GitHubModels:Endpoint"];
        string githubModel = resolvedUseGnai
            ? (string.IsNullOrWhiteSpace(resolvedGnaiModel) ? "gpt-5-mini" : resolvedGnaiModel)
            : ConfigurationManager.AppSettings["GitHubModels:Model"];
        string githubApiKey = resolvedUseGnai ? resolvedGnaiToken : ConfigurationManager.AppSettings["GitHubModels:ApiKey"];
        string githubProxy = resolvedUseGnai ? resolvedGnaiProxy : ConfigurationManager.AppSettings["GitHubModels:Proxy"];

        DataTable issues;

        bool wantsComparison = safePrompt.IndexOf("compare", StringComparison.OrdinalIgnoreCase) >= 0
            || safePrompt.IndexOf("comparison", StringComparison.OrdinalIgnoreCase) >= 0
            || safePrompt.IndexOf("versus", StringComparison.OrdinalIgnoreCase) >= 0
            || safePrompt.IndexOf(" vs ", StringComparison.OrdinalIgnoreCase) >= 0;

        issues = reportPlatforms.Count == 1 ? LoadIssueData(reportPlatforms[0]) : LoadCombinedIssueData(reportPlatforms);
        EnrichIssueDataWithHsdContext(issues, MaxHsdRowsForReports);

        if (issues.Rows.Count == 0)
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "No issue rows available for report generation in the selected platform."
            };
        }

        string appDataRoot = HostingEnvironment.MapPath("~/App_Data/reports-assistant");
        string webOutputRoot = HostingEnvironment.MapPath("~/Content/generated-reports");
        if (string.IsNullOrWhiteSpace(appDataRoot) || string.IsNullOrWhiteSpace(webOutputRoot))
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Unable to resolve report output folders."
            };
        }

        Directory.CreateDirectory(appDataRoot);
        Directory.CreateDirectory(webOutputRoot);

        string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        string csvPath = Path.Combine(appDataRoot, "issues_" + stamp + ".csv");
        WriteCsv(issues, csvPath);

        string pythonScript = HostingEnvironment.MapPath("~/Scripts/analytics_chatbot.py");
        if (string.IsNullOrWhiteSpace(pythonScript) || !File.Exists(pythonScript))
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Python analytics script not found at Scripts/analytics_chatbot.py."
            };
        }

        string pythonExe = ConfigurationManager.AppSettings["Python:Executable"];
        if (string.IsNullOrWhiteSpace(pythonExe))
        {
            pythonExe = "python";
        }

        string args = string.Join(" ", new[]
        {
            QuoteArg(pythonScript),
            "--csv", QuoteArg(csvPath),
            "--intent", QuoteArg(intent),
            "--prompt", QuoteArg(safePrompt),
            "--context-json", QuoteArg(conversationContextJson),
            "--platform", QuoteArg(platformCode),
            "--output-dir", QuoteArg(webOutputRoot),
            "--max-rows", "4000"
        });

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(pythonScript)
        };

        psi.EnvironmentVariables["CMF_GITHUB_ENDPOINT"] = githubEndpoint ?? string.Empty;
        psi.EnvironmentVariables["CMF_GITHUB_MODEL"] = githubModel ?? string.Empty;
        psi.EnvironmentVariables["CMF_GITHUB_API_KEY"] = githubApiKey ?? string.Empty;
        psi.EnvironmentVariables["CMF_GITHUB_PROXY"] = githubProxy ?? string.Empty;
        psi.EnvironmentVariables["CMF_PLATFORM_ALIAS_MAP_JSON"] = BuildPlatformAliasPayloadJson();

        string stdout;
        string stderr;
        int exitCode;

        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.Start();
            stdout = process.StandardOutput.ReadToEnd();
            stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(120000))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                }

                return new ReportsAssistantResponse
                {
                    Success = false,
                    Message = "Python analytics process timed out."
                };
            }

            exitCode = process.ExitCode;
        }

        if (exitCode != 0)
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Python analytics failed: " + SafeError(stderr)
            };
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;

        IDictionary payload;
        try
        {
            object parsed = serializer.DeserializeObject(stdout);
            payload = parsed as IDictionary;
        }
        catch (Exception)
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Unable to parse analytics response: " + stdout
            };
        }

        if (payload == null)
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Analytics response was empty."
            };
        }

        bool success = ReadBool(payload, "success");
        string message = ReadString(payload, "message");
        string imageFile = ReadString(payload, "image_file");
        string reportFile = ReadString(payload, "report_file");
        string updatedContextJson = ReadJsonObject(payload, "context", serializer);

        if (success && !string.IsNullOrWhiteSpace(updatedContextJson))
        {
            WriteConversationContext(reportScopeKey, updatedContextJson);
        }

        return new ReportsAssistantResponse
        {
            Success = success,
            Intent = intent,
            Message = success
                ? (string.IsNullOrWhiteSpace(message) ? "Analytics result generated successfully." : message)
                : (string.IsNullOrWhiteSpace(message) ? "Analytics request failed." : message),
            ImageUrl = BuildPublicAssetPath(imageFile),
            ReportUrl = BuildPublicAssetPath(reportFile)
        };
    }

    public static ReportsAssistantResponse GenerateFromTemplate(string templateContent, string platform)
    {
        // Similar to ProcessPrompt but uses provided template content to populate report
        List<string> templatePlatforms = ResolvePlatformsFromPrompt(templateContent);
        string resolvedPlatform = ResolvePlatform(platform);
        List<string> reportPlatforms = ResolveReportPlatformScope(templatePlatforms, resolvedPlatform);
        DataTable issues;
        if (reportPlatforms.Count == 0)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Unable to resolve CMF data scope for report generation." };
        }

        resolvedPlatform = reportPlatforms[0];
        issues = reportPlatforms.Count == 1 ? LoadIssueData(reportPlatforms[0]) : LoadCombinedIssueData(reportPlatforms);
        EnrichIssueDataWithHsdContext(issues, MaxHsdRowsForReports);
        if (issues.Rows.Count == 0)
        {
            return new ReportsAssistantResponse { Success = false, Message = "No issue rows available for report generation in the selected platform." };
        }

        string appDataRoot = HostingEnvironment.MapPath("~/App_Data/reports-assistant");
        string webOutputRoot = HostingEnvironment.MapPath("~/Content/generated-reports");
        if (string.IsNullOrWhiteSpace(appDataRoot) || string.IsNullOrWhiteSpace(webOutputRoot))
        {
            return new ReportsAssistantResponse { Success = false, Message = "Unable to resolve report output folders." };
        }

        Directory.CreateDirectory(appDataRoot);
        Directory.CreateDirectory(webOutputRoot);

        string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        string csvPath = Path.Combine(appDataRoot, "issues_" + stamp + ".csv");
        WriteCsv(issues, csvPath);

        // Write template file
        string templateFileName = "template_" + stamp + ".md";
        string templatePath = Path.Combine(appDataRoot, templateFileName);
        try
        {
            File.WriteAllText(templatePath, templateContent ?? string.Empty, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Unable to write template file: " + ex.Message };
        }

        string pythonScript = HostingEnvironment.MapPath("~/Scripts/analytics_chatbot.py");
        if (string.IsNullOrWhiteSpace(pythonScript) || !File.Exists(pythonScript))
        {
            return new ReportsAssistantResponse { Success = false, Message = "Python analytics script not found at Scripts/analytics_chatbot.py." };
        }

        string pythonExe = ConfigurationManager.AppSettings["Python:Executable"];
        if (string.IsNullOrWhiteSpace(pythonExe))
        {
            pythonExe = "python";
        }

        string args = string.Join(" ", new[]
        {
            QuoteArg(pythonScript),
            "--csv", QuoteArg(csvPath),
            "--intent", QuoteArg("issue_report"),
            "--prompt", QuoteArg("Generate report from saved template"),
            "--context-json", QuoteArg("{}"),
            "--template", QuoteArg(templatePath),
            "--platform", QuoteArg(BuildCombinedPlatformCode(reportPlatforms)),
            "--output-dir", QuoteArg(webOutputRoot)
        });

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(pythonScript)
        };

        // Propagate model env as in ProcessPrompt
        string resolvedGnaiToken = ResolveGnaiToken();
        string resolvedGnaiEndpoint = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_ENDPOINT"), GetAppSetting("GNAI:Endpoint"));
        string resolvedGnaiModel = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_MODEL"), GetAppSetting("GNAI:Model"));
        bool resolvedUseGnai = !string.IsNullOrWhiteSpace(resolvedGnaiToken)
            && !string.IsNullOrWhiteSpace(resolvedGnaiEndpoint)
            && !resolvedGnaiEndpoint.StartsWith("REPLACE");

        string githubEndpoint = resolvedUseGnai ? resolvedGnaiEndpoint : ConfigurationManager.AppSettings["GitHubModels:Endpoint"];
        string githubModel = resolvedUseGnai
            ? (string.IsNullOrWhiteSpace(resolvedGnaiModel) ? "gpt-5-mini" : resolvedGnaiModel)
            : ConfigurationManager.AppSettings["GitHubModels:Model"];
        string githubApiKey = resolvedUseGnai ? resolvedGnaiToken : ConfigurationManager.AppSettings["GitHubModels:ApiKey"];
        string githubProxy = resolvedUseGnai ? FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_PROXY"), GetAppSetting("GNAI:Proxy")) : (ConfigurationManager.AppSettings["GitHubModels:Proxy"] ?? string.Empty);

        psi.EnvironmentVariables["CMF_GITHUB_ENDPOINT"] = githubEndpoint ?? string.Empty;
        psi.EnvironmentVariables["CMF_GITHUB_MODEL"] = githubModel ?? string.Empty;
        psi.EnvironmentVariables["CMF_GITHUB_API_KEY"] = githubApiKey ?? string.Empty;
        psi.EnvironmentVariables["CMF_GITHUB_PROXY"] = githubProxy ?? string.Empty;
        psi.EnvironmentVariables["CMF_PLATFORM_ALIAS_MAP_JSON"] = BuildPlatformAliasPayloadJson();

        string stdout;
        string stderr;
        int exitCode;

        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.Start();
            stdout = process.StandardOutput.ReadToEnd();
            stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(120000))
            {
                try { process.Kill(); } catch { }
                return new ReportsAssistantResponse { Success = false, Message = "Python analytics process timed out." };
            }
            exitCode = process.ExitCode;
        }

        if (exitCode != 0)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Python analytics failed: " + SafeError(stderr) };
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;

        IDictionary payload;
        try
        {
            object parsed = serializer.DeserializeObject(stdout);
            payload = parsed as IDictionary;
        }
        catch (Exception)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Unable to parse analytics response: " + stdout };
        }

        if (payload == null)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Analytics response was empty." };
        }

        bool success = ReadBool(payload, "success");
        string message = ReadString(payload, "message");
        string imageFile = ReadString(payload, "image_file");
        string reportFile = ReadString(payload, "report_file");
        string updatedContextJson = ReadJsonObject(payload, "context", serializer);

        if (success && !string.IsNullOrWhiteSpace(updatedContextJson))
        {
            WriteConversationContext(BuildReportScopeKey(reportPlatforms), updatedContextJson);
        }

        return new ReportsAssistantResponse
        {
            Success = success,
            Intent = "issue_report",
            Message = success ? (string.IsNullOrWhiteSpace(message) ? "Analytics result generated successfully." : message) : (string.IsNullOrWhiteSpace(message) ? "Analytics request failed." : message),
            ImageUrl = BuildPublicAssetPath(imageFile),
            ReportUrl = BuildPublicAssetPath(reportFile)
        };
    }

    private static string ResolvePlatform(string platform)
    {
        string selected = string.IsNullOrWhiteSpace(platform)
            ? CmfPlatformCatalog.GetDefaultPlatformTableName()
            : platform.Trim();

        return AllowedPlatformTables.Contains(selected) ? selected : string.Empty;
    }

    private static List<string> ResolveReportPlatformScope(List<string> requestedPlatforms, string fallbackPlatform)
    {
        if (requestedPlatforms != null && requestedPlatforms.Count > 0)
        {
            return new List<string>(requestedPlatforms);
        }

        return GetAllPlatformTables();
    }

    private static List<string> GetAllPlatformTables()
    {
        return new List<string>(CmfPlatformCatalog.GetAllPlatformTables());
    }

    private static string BuildReportScopeKey(List<string> platformTables)
    {
        if (platformTables == null || platformTables.Count == 0) return AllCmfDataContextKey;
        if (platformTables.Count == CmfPlatformCatalog.GetPlatformCount()) return AllCmfDataContextKey;
        return BuildCombinedPlatformCode(platformTables);
    }

    private static DataTable LoadCombinedIssueData(List<string> platformTables)
    {
        DataTable combined = new DataTable();
        bool initialized = false;
        if (platformTables == null) return combined;

        for (int index = 0; index < platformTables.Count; index++)
        {
            DataTable next = LoadIssueData(platformTables[index]);
            if (!initialized)
            {
                combined = next.Clone();
                initialized = true;
            }

            for (int rowIndex = 0; rowIndex < next.Rows.Count; rowIndex++)
            {
                combined.ImportRow(next.Rows[rowIndex]);
            }
        }

        return combined;
    }

    private static void EnrichIssueDataWithHsdContext(DataTable table, int maxRows)
    {
        if (table == null || table.Rows.Count == 0 || maxRows <= 0)
        {
            return;
        }

        EnsureColumn(table, "HSD_Sighting_Context");
        EnsureColumn(table, "HSD_Promoted_Context");

        int enriched = 0;
        for (int index = 0; index < table.Rows.Count && enriched < maxRows; index++)
        {
            DataRow row = table.Rows[index];
            string sightingId = FirstRowValue(row, "SightingID", "cp_id");
            string promotedId = FirstRowValue(row, "promoted_id", "merge_id");

            if (!string.IsNullOrWhiteSpace(sightingId))
            {
                row["HSD_Sighting_Context"] = BuildCompactHsdContext(sightingId, "Sighting " + sightingId);
            }

            if (!string.IsNullOrWhiteSpace(promotedId) && !string.Equals(promotedId.Trim(), sightingId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                row["HSD_Promoted_Context"] = BuildCompactHsdContext(promotedId, "Promoted " + promotedId);
            }

            enriched++;
        }
    }

    private static void EnsureColumn(DataTable table, string columnName)
    {
        if (table != null && !table.Columns.Contains(columnName))
        {
            table.Columns.Add(columnName, typeof(string));
        }
    }

    private static string BuildCompactHsdContext(string articleId, string label)
    {
        try
        {
            HsdArticleData article = HsdPortalService.FetchArticle(articleId.Trim());
            string context = HsdPortalService.FormatForAiContext(article, label);
            return BuildCompactText(context, 1800);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string FirstRowValue(DataRow row, params string[] columnNames)
    {
        if (row == null || columnNames == null) return string.Empty;
        for (int index = 0; index < columnNames.Length; index++)
        {
            string columnName = columnNames[index];
            if (!row.Table.Columns.Contains(columnName)) continue;
            object value = row[columnName];
            if (value == null || value == DBNull.Value) continue;
            string text = value.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }

        return string.Empty;
    }

    private static string BuildCompactText(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string normalized = text.Replace("\r", " ").Replace("\n", " ").Trim();
        while (normalized.IndexOf("  ", StringComparison.Ordinal) >= 0)
        {
            normalized = normalized.Replace("  ", " ");
        }

        if (normalized.Length <= maxLength) return normalized;
        return normalized.Substring(0, maxLength) + "...";
    }

    private static string BuildCombinedPlatformCode(List<string> platformTables)
    {
        if (platformTables == null || platformTables.Count == 0) return string.Empty;
        List<string> codes = new List<string>();
        for (int index = 0; index < platformTables.Count; index++)
        {
            string code = ResolvePlatformCode(platformTables[index]);
            if (!string.IsNullOrWhiteSpace(code) && !codes.Contains(code))
            {
                codes.Add(code);
            }
        }

        return string.Join("+", codes.ToArray());
    }

    private static string BuildPlatformAliasPayloadJson()
    {
        Dictionary<string, object> payload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, object> aliasToCodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, object> groupAliasToCodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> entry in PlatformPromptMap)
        {
            string code = ResolvePlatformCode(entry.Value);
            if (!string.IsNullOrWhiteSpace(code) && !string.Equals(code, "UNKNOWN", StringComparison.OrdinalIgnoreCase))
            {
                aliasToCodes[entry.Key] = code.ToLowerInvariant();
            }
        }

        foreach (KeyValuePair<string, string[]> entry in PlatformGroupAliasMap)
        {
            List<string> codes = new List<string>();
            string[] tableNames = entry.Value ?? new string[0];
            for (int index = 0; index < tableNames.Length; index++)
            {
                string code = ResolvePlatformCode(tableNames[index]);
                if (string.IsNullOrWhiteSpace(code) || string.Equals(code, "UNKNOWN", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                code = code.ToLowerInvariant();
                if (!codes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            if (codes.Count > 0)
            {
                groupAliasToCodes[entry.Key] = codes;
            }
        }

        payload["alias_to_codes"] = aliasToCodes;
        payload["group_alias_to_codes"] = groupAliasToCodes;

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        return serializer.Serialize(payload);
    }

    private static string ResolvePlatformFromPrompt(string prompt)
    {
        string lowered = (prompt ?? string.Empty).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(lowered))
        {
            return string.Empty;
        }

        foreach (KeyValuePair<string, string> entry in PlatformPromptMap)
        {
            if (ContainsWholePlatformAlias(lowered, entry.Key))
            {
                return AllowedPlatformTables.Contains(entry.Value) ? entry.Value : string.Empty;
            }
        }

        return string.Empty;
    }

    private static List<string> ResolvePlatformsFromPrompt(string prompt)
    {
        List<string> platforms = new List<string>();
        string lowered = (prompt ?? string.Empty).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(lowered))
        {
            return platforms;
        }

        foreach (KeyValuePair<string, string[]> entry in PlatformGroupAliasMap)
        {
            if (!ContainsWholePlatformAlias(lowered, entry.Key) || entry.Value == null)
            {
                continue;
            }

            for (int index = 0; index < entry.Value.Length; index++)
            {
                string tableName = entry.Value[index];
                if (AllowedPlatformTables.Contains(tableName) && !platforms.Contains(tableName))
                {
                    platforms.Add(tableName);
                }
            }
        }

        foreach (KeyValuePair<string, string> entry in PlatformPromptMap)
        {
            if (ContainsWholePlatformAlias(lowered, entry.Key) && AllowedPlatformTables.Contains(entry.Value) && !platforms.Contains(entry.Value))
            {
                platforms.Add(entry.Value);
            }
        }

        return platforms;
    }

    private static bool ContainsWholePlatformAlias(string text, string alias)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(alias)) return false;
        string escaped = System.Text.RegularExpressions.Regex.Escape(alias.Trim().ToLowerInvariant()).Replace("\\ ", @"[\s_-]+");
        return System.Text.RegularExpressions.Regex.IsMatch(text, @"(?<![a-z0-9])" + escaped + @"(?![a-z0-9])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string ResolveIntent(string prompt)
    {
        string lowered = (prompt ?? string.Empty).ToLowerInvariant();

        bool asksForChart =
            lowered.Contains("chart") ||
            lowered.Contains("graph") ||
            lowered.Contains("plot") ||
            lowered.Contains("visual") ||
            lowered.Contains("bar") ||
            lowered.Contains("pie") ||
            lowered.Contains("histogram");

        bool asksForRankedCustomerOrOwner =
            (lowered.Contains("customer") || lowered.Contains("owner")) &&
            (lowered.Contains("top") || lowered.Contains("highest") || lowered.Contains("most") || lowered.Contains("largest"));

        if (asksForRankedCustomerOrOwner && !asksForChart)
        {
            return "rag_qa";
        }

        // First, try to use the model for better intent classification after deterministic guards.
        string modelIntent = string.Empty;
        if (TryResolveIntentWithModel(prompt, out modelIntent) && !string.IsNullOrWhiteSpace(modelIntent))
        {
            return modelIntent;
        }

        bool asksForReport =
            lowered.Contains("report") ||
            lowered.Contains("csv") ||
            lowered.Contains("download") ||
            lowered.Contains("excel") ||
            lowered.Contains("xlsx") ||
            lowered.Contains("export");

        bool asksForSummary =
            lowered.Contains("summary") ||
            lowered.Contains("overview") ||
            lowered.Contains("high level") ||
            lowered.Contains("high-level") ||
            lowered.Contains("snapshot");

        if (asksForReport)
        {
            return "issue_report";
        }

        if (lowered.Contains("compare") || lowered.Contains("versus") || lowered.Contains(" vs "))
        {
            return "comparison";
        }

        if (asksForChart && lowered.Contains("owner"))
        {
            return "owner_chart";
        }

        if (asksForChart && (lowered.Contains("stale") || lowered.Contains("age") || lowered.Contains("days")))
        {
            return "stale_chart";
        }

        if (asksForChart && lowered.Contains("status"))
        {
            return "status_chart";
        }

        if (asksForSummary)
        {
            return "summary";
        }

        // Default to retrieval QA for natural-language analytics questions.
        return "rag_qa";
    }

    private static bool TryResolveIntentWithModel(string prompt, out string intent)
    {
        intent = string.Empty;

        string gnaiToken = ResolveGnaiToken();
        string gnaiCfgEndpoint = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_ENDPOINT"), GetAppSetting("GNAI:Endpoint"));
        string gnaiCfgModel = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_MODEL"), GetAppSetting("GNAI:Model"));
        bool useGnai = !string.IsNullOrWhiteSpace(gnaiToken)
            && !string.IsNullOrWhiteSpace(gnaiCfgEndpoint)
            && !gnaiCfgEndpoint.StartsWith("REPLACE");

        string apiKey, endpoint, model;
        if (useGnai)
        {
            apiKey = gnaiToken;
            endpoint = gnaiCfgEndpoint;
            model = string.IsNullOrWhiteSpace(gnaiCfgModel) ? "gpt-5-mini" : gnaiCfgModel;
        }
        else
        {
            apiKey = ConfigurationManager.AppSettings["GitHubModels:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;
            endpoint = ConfigurationManager.AppSettings["GitHubModels:Endpoint"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(endpoint))
                endpoint = "https://models.inference.ai.azure.com/chat/completions";
            model = ConfigurationManager.AppSettings["GitHubModels:Model"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(model))
                model = "gpt-4o-mini";
        }

        string classificationPrompt =
            "Classify this analytics request into exactly one intent from: " +
            "summary, status_chart, owner_chart, stale_chart, issue_report, comparison, rag_qa. " +
            "Respond as JSON object with key 'intent' and value as one of those intents. Request: " + prompt;

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        var payload = new
        {
            model = model,
            messages = new object[]
            {
                new { role = "system", content = "You are an intent classifier." },
                new { role = "user", content = classificationPrompt }
            }
        };

        string payloadJson = serializer.Serialize(payload);

        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers["Authorization"] = "Bearer " + apiKey;
            request.Timeout = 20000;
            request.UserAgent = "CMF-Reports-Assistant/1.0";
            ConfigureProxy(request);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(payloadJson);
            }

            string responseBody;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseBody = reader.ReadToEnd();
            }

            object raw = serializer.DeserializeObject(responseBody);
            string modelText = ExtractResponseContent(raw);
            if (string.IsNullOrWhiteSpace(modelText))
                return false;

            modelText = modelText.Trim();
            
            // Extract intent from response (e.g., "summary" or {"intent": "summary"})
            if (modelText.StartsWith("{") && modelText.Contains("intent"))
            {
                try
                {
                    var intentObj = serializer.DeserializeObject(modelText) as IDictionary;
                    if (intentObj != null && intentObj.Contains("intent"))
                    {
                        intent = (intentObj["intent"] as string ?? "").Trim().ToLowerInvariant();
                    }
                }
                catch { }
            }
            else
            {
                intent = modelText.ToLowerInvariant();
            }

            // Validate intent - avoid using out parameter in lambda
            if (IsAllowedIntent(intent))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void ConfigureProxy(HttpWebRequest request)
    {
        // GNAI is internal, so NO proxy needed for it
        // Only GitHub Models (external) requires the DMZ proxy
        string gnaiToken = ResolveGnaiToken();
        string gnaiEndpoint = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_ENDPOINT"), GetAppSetting("GNAI:Endpoint"));
        bool usingGnai = !string.IsNullOrWhiteSpace(gnaiToken)
            && !string.IsNullOrWhiteSpace(gnaiEndpoint)
            && !gnaiEndpoint.StartsWith("REPLACE");
        
        if (usingGnai)
        {
            string gnaiProxy = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_PROXY"), GetAppSetting("GNAI:Proxy"));
            if (!string.IsNullOrWhiteSpace(gnaiProxy))
            {
                WebProxy explicitGnaiProxy = new WebProxy(gnaiProxy.Trim(), true);
                explicitGnaiProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                request.Proxy = explicitGnaiProxy;
                return;
            }

            request.Proxy = null;
            return;
        }
        
        // GitHub Models is external - needs proxy
        string proxyAddress = ConfigurationManager.AppSettings["GitHubModels:Proxy"];
        if (!string.IsNullOrWhiteSpace(proxyAddress))
        {
            WebProxy explicitProxy = new WebProxy(proxyAddress.Trim(), true);
            explicitProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            request.Proxy = explicitProxy;
            return;
        }

        IWebProxy systemProxy = WebRequest.GetSystemWebProxy();
        if (systemProxy != null)
        {
            systemProxy.Credentials = CredentialCache.DefaultCredentials;
            request.Proxy = systemProxy;
        }
    }

    private static string ResolveGnaiToken()
    {
        string token = FirstNonEmpty(System.Environment.GetEnvironmentVariable("GNAI_TOKEN"), System.Environment.GetEnvironmentVariable("GNAI_API_KEY"));
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        return GetAppSetting("GNAI:ApiKey");
    }

    private static string FirstNonEmpty(params string[] values)
    {
        if (values == null) return string.Empty;
        for (int index = 0; index < values.Length; index++)
        {
            if (!string.IsNullOrWhiteSpace(values[index])) return values[index];
        }

        return string.Empty;
    }

    private static string GetAppSetting(string key)
    {
        string value = ConfigurationManager.AppSettings[key] ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(value)) return value;

        try
        {
            List<string> configPaths = new List<string>();
            configPaths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web.config"));
            try
            {
                string mappedPath = HostingEnvironment.MapPath("~/Web.config");
                if (!string.IsNullOrWhiteSpace(mappedPath)) configPaths.Add(mappedPath);
            }
            catch { }

            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Server != null)
                {
                    string serverPath = HttpContext.Current.Server.MapPath("~/Web.config");
                    if (!string.IsNullOrWhiteSpace(serverPath)) configPaths.Add(serverPath);
                }
            }
            catch { }

            for (int index = 0; index < configPaths.Count; index++)
            {
                string configPath = configPaths[index];
                if (string.IsNullOrWhiteSpace(configPath) || !File.Exists(configPath)) continue;

                XmlDocument document = new XmlDocument();
                document.Load(configPath);
                XmlNode node = document.SelectSingleNode("/configuration/appSettings/add[@key='" + key.Replace("'", "&apos;") + "']");
                if (node != null && node.Attributes != null && node.Attributes["value"] != null)
                {
                    return node.Attributes["value"].Value ?? string.Empty;
                }
            }
        }
        catch { }

        return string.Empty;
    }

    private static bool IsAllowedIntent(string intent)
    {
        return intent == "summary" ||
               intent == "status_chart" ||
               intent == "owner_chart" ||
               intent == "stale_chart" ||
               intent == "issue_report" ||
               intent == "comparison" ||
               intent == "rag_qa";
    }

    private static string ExtractResponseContent(object raw)
    {
        IDictionary root = raw as IDictionary;
        if (root == null || root["choices"] == null)
        {
            return string.Empty;
        }

        IList choices = root["choices"] as IList;
        if (choices == null || choices.Count == 0)
        {
            return string.Empty;
        }

        IDictionary firstChoice = choices[0] as IDictionary;
        if (firstChoice == null || firstChoice["message"] == null)
        {
            return string.Empty;
        }

        IDictionary message = firstChoice["message"] as IDictionary;
        if (message == null || message["content"] == null)
        {
            return string.Empty;
        }

        return message["content"].ToString();
    }

    private static DataTable LoadIssueData(string platformTable)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        DataTable table = new DataTable();
        string platformCode = ResolvePlatformCode(platformTable);

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            HashSet<string> availableColumns = GetTableColumns(connection, platformTable);

            string sightingExpr = BuildAliasedColumnExpression(availableColumns, "SightingID", "SightingID", "cp_id", "cpid");
            string titleExpr = BuildAliasedColumnExpression(availableColumns, "title", "title");
            string statusExpr = BuildAliasedColumnExpression(availableColumns, "status", "status");
            string sysdebugExpr = BuildAliasedColumnExpression(availableColumns, "sysdebug", "sysdebug");
            string componentGroupExpr = BuildAliasedColumnExpression(availableColumns, "component_group", "component_group", "component");
            string driversExpr = BuildAliasedColumnExpression(availableColumns, "drivers", "drivers");
            string ownerExpr = BuildAliasedColumnExpression(availableColumns, "Owner", "Owner", "owner", "customer_owner", "owners_name");
            string daysActiveExpr = BuildAliasedColumnExpression(availableColumns, "days_active", "days_active");
            string submittedDateExpr = BuildAliasedColumnExpression(availableColumns, "SubmittedDate", "SubmittedDate", "submitted_date", "created_date");
            string customerCompanyExpr = BuildAliasedColumnExpression(availableColumns, "customer_company", "customer_company");
            string componentExpr = BuildAliasedColumnExpression(availableColumns, "component", "component");
            string losExpr = BuildAliasedColumnExpression(availableColumns, "los", "los");
            string idstExpr = BuildAliasedColumnExpression(availableColumns, "idst", "idst");
            string cmfRequestExpr = BuildAliasedColumnExpression(availableColumns, "cmf_request", "cmf_request");
            string impactExpr = BuildAliasedColumnExpression(availableColumns, "impact", "impact", "customer_impact");
            string reproducibilityExpr = BuildAliasedColumnExpression(availableColumns, "reproducibility", "reproducibility");
            string reproOnRvpExpr = BuildAliasedColumnExpression(availableColumns, "repro_on_rvp", "repro_on_rvp");
            string dateCmfAskExpr = BuildAliasedColumnExpression(availableColumns, "date_cmf_ask", "date_cmf_ask");
            string priorityExpr = BuildAliasedColumnExpression(availableColumns, "priority", "priority");
            string promotedIdExpr = BuildAliasedColumnExpression(availableColumns, "promoted_id", "promoted_id", "merge_id");
            string closedReasonExpr = BuildAliasedColumnExpression(availableColumns, "closed_reason", "closed_reason");
            string fixedVersionExpr = BuildAliasedColumnExpression(availableColumns, "fixed_in_version", "fixed_in_version");
            string platformExpr = "'" + platformCode.Replace("'", "''") + "' AS [Platform]";

            // Keep chatbot analytics aligned with full platform table counts.
            // Do not apply implicit cmf_request filters here.
            string whereClause = string.Empty;

            string sql = "SELECT TOP 2000 " + string.Join(", ", new[]
            {
                platformExpr,
                sightingExpr,
                titleExpr,
                statusExpr,
                sysdebugExpr,
                componentGroupExpr,
                driversExpr,
                ownerExpr,
                daysActiveExpr,
                submittedDateExpr,
                customerCompanyExpr,
                componentExpr,
                losExpr,
                idstExpr,
                cmfRequestExpr,
                impactExpr,
                reproducibilityExpr,
                reproOnRvpExpr,
                dateCmfAskExpr,
                priorityExpr,
                promotedIdExpr,
                closedReasonExpr,
                fixedVersionExpr
            }) + " FROM " + platformTable + whereClause;

            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(table);
            }
        }

        return table;
    }

    private static HashSet<string> GetTableColumns(SqlConnection connection, string platformTable)
    {
        HashSet<string> columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string sql = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName";

        using (SqlCommand command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@tableName", platformTable);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(reader["COLUMN_NAME"].ToString());
                }
            }
        }

        return columns;
    }

    private static string BuildAliasedColumnExpression(HashSet<string> availableColumns, string alias, params string[] candidates)
    {
        for (int index = 0; index < candidates.Length; index++)
        {
            string candidate = candidates[index];
            if (availableColumns.Contains(candidate))
            {
                return "[" + candidate + "] AS [" + alias + "]";
            }
        }

        return "NULL AS [" + alias + "]";
    }

    private static string ResolvePlatformCode(string platformTable)
    {
        return CmfPlatformCatalog.ResolvePlatformCode(platformTable);
    }

    private static string ReadConversationContext(string platform)
    {
        HttpContext context = HttpContext.Current;
        if (context == null || context.Session == null || string.IsNullOrWhiteSpace(platform))
        {
            return string.Empty;
        }

        Dictionary<string, string> contextMap = context.Session[ReportsContextSessionKey] as Dictionary<string, string>;
        if (contextMap == null)
        {
            return string.Empty;
        }

        string stored;
        return contextMap.TryGetValue(platform, out stored) ? (stored ?? string.Empty) : string.Empty;
    }

    private static void WriteConversationContext(string platform, string contextJson)
    {
        HttpContext context = HttpContext.Current;
        if (context == null || context.Session == null || string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(contextJson))
        {
            return;
        }

        Dictionary<string, string> contextMap = context.Session[ReportsContextSessionKey] as Dictionary<string, string>;
        if (contextMap == null)
        {
            contextMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        contextMap[platform] = contextJson;
        context.Session[ReportsContextSessionKey] = contextMap;
    }

    private static void WriteCsv(DataTable table, string path)
    {
        using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            for (int index = 0; index < table.Columns.Count; index++)
            {
                if (index > 0)
                {
                    writer.Write(',');
                }

                writer.Write(EscapeCsv(table.Columns[index].ColumnName));
            }
            writer.WriteLine();

            foreach (DataRow row in table.Rows)
            {
                for (int index = 0; index < table.Columns.Count; index++)
                {
                    if (index > 0)
                    {
                        writer.Write(',');
                    }

                    object value = row[index];
                    writer.Write(EscapeCsv(value == null || value == DBNull.Value ? string.Empty : value.ToString()));
                }
                writer.WriteLine();
            }
        }
    }

    private static string EscapeCsv(string value)
    {
        string safe = value ?? string.Empty;
        if (safe.Contains("\"") || safe.Contains(",") || safe.Contains("\n") || safe.Contains("\r"))
        {
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        return safe;
    }

    private static string QuoteArg(string arg)
    {
        if (arg == null)
        {
            return "\"\"";
        }

        return "\"" + arg.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    private static string BuildPublicAssetPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        string safeName = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(safeName))
        {
            return string.Empty;
        }

        return "Content/generated-reports/" + safeName;
    }

    private static bool ReadBool(IDictionary payload, string key)
    {
        if (payload == null || payload[key] == null)
        {
            return false;
        }

        bool parsed;
        if (bool.TryParse(payload[key].ToString(), out parsed))
        {
            return parsed;
        }

        return false;
    }

    private static string ReadString(IDictionary payload, string key)
    {
        if (payload == null || payload[key] == null)
        {
            return string.Empty;
        }

        return payload[key].ToString();
    }

    private static string ReadJsonObject(IDictionary payload, string key, JavaScriptSerializer serializer)
    {
        if (payload == null || serializer == null || payload[key] == null)
        {
            return string.Empty;
        }

        object value = payload[key];
        string existingJson = value as string;
        if (!string.IsNullOrWhiteSpace(existingJson))
        {
            return existingJson;
        }

        try
        {
            return serializer.Serialize(value);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeError(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return "No details provided.";
        }

        string[] lines = stderr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return "No details provided.";
        }

        return lines[Math.Max(0, lines.Length - 1)].Trim();
    }

    private static string ValidatePortalRelevance(string prompt)
    {
        string lower = (prompt ?? string.Empty).ToLowerInvariant();

        // Whitelisted patterns for chart, report, and analytics generation
        // These are always allowed as they're portal-native features
        string[] whitelistedPatterns = new[]
        {
            "chart", "graph", "plot", "report", "csv", "export", "generate",
            "status distribution", "owner", "stale", "trend", "trend analysis"
        };

        // Check if the prompt matches whitelisted chart/report patterns
        foreach (string pattern in whitelistedPatterns)
        {
            if (lower.Contains(pattern) && (lower.Contains("chart") || lower.Contains("report") || lower.Contains("csv") || lower.Contains("graph")))
            {
                return string.Empty; // Allowed - chart/report generation request
            }
        }

        // Portal-relevant keywords that should be present.
        List<string> portalKeywords = new List<string>
        {
            "issue", "bug", "problem", "sighting", "cmf", "defect", "status", "owner",
            "component", "driver", "platform", "milestone", "customer", "resolved",
            "implemented", "verified", "closed", "open", "pending", "chart", "report",
            "analytics", "data", "trend", "analysis", "summary", "stale", "days",
            "distribution", "count", "number of", "how many", "what",
            "show", "list", "find", "get", "which", "top", "highest", "lowest"
        };

        foreach (string alias in PlatformPromptMap.Keys)
        {
            if (!portalKeywords.Contains(alias))
            {
                portalKeywords.Add(alias);
            }
        }

        foreach (string alias in PlatformGroupAliasMap.Keys)
        {
            if (!portalKeywords.Contains(alias))
            {
                portalKeywords.Add(alias);
            }
        }

        string[] allTables = CmfPlatformCatalog.GetAllPlatformTables();
        for (int tableIndex = 0; tableIndex < allTables.Length; tableIndex++)
        {
            string platformCode = ResolvePlatformCode(allTables[tableIndex]).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(platformCode) && !portalKeywords.Contains(platformCode))
            {
                portalKeywords.Add(platformCode);
            }
        }

        // Non-portal generic keywords that should be rejected
        string[] genericKeywords = new[]
        {
            "sing", "song", "movie", "film", "cast", "actor", "actress", "sport",
            "game", "weather", "recipe", "cook", "travel", "book", "author",
            "joke", "riddle", "poem", "art", "music", "band", "concert",
            "celebrity", "politician", "president", "prime minister", "captain",
            "team", "player", "score", "goal", "match", "basketball", "football",
            "soccer", "chess", "code", "program", "write", "create", "build",
            "python", "javascript", "java", "c++", "help me", "can you"
        };

        // Check if prompt contains generic non-portal keywords
        foreach (string keyword in genericKeywords)
        {
            if (lower.Contains(keyword))
            {
                // Special cases where these words might be legitimate
                if (keyword == "help me" && lower.Contains("issue"))
                    continue;
                if (keyword == "can you" && (lower.Contains("analyze") || lower.Contains("show") || lower.Contains("chart")))
                    continue;
                if (keyword == "code" && lower.Contains("issue"))
                    continue;
                if (keyword == "build" && lower.Contains("report"))
                    continue;

                return "I can only help with questions related to CMF issues, platforms, and data analysis. " +
                       "Please ask about issues, components, platforms, owners, trends, or analytics related to the CMF portal.";
            }
        }

        // Check if prompt contains at least one portal-relevant keyword
        bool hasRelevantKeyword = false;
        foreach (string keyword in portalKeywords)
        {
            if (lower.Contains(keyword))
            {
                hasRelevantKeyword = true;
                break;
            }
        }

        if (!hasRelevantKeyword)
        {
            return "I can only help with questions related to CMF issues, platforms, and data analysis. " +
                   "Please ask about issues, components, platforms, owners, trends, or analytics related to the CMF portal.";
        }

        return string.Empty; // Prompt is relevant
    }

    private static bool LooksLikeFollowUpPrompt(string prompt)
    {
        string lower = (prompt ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(lower))
        {
            return false;
        }

        string[] followUpMarkers = new[]
        {
            "what about",
            "how about",
            "and for",
            "same for",
            "those",
            "these",
            "them",
            "that one",
            "this one",
            "previous",
            "above"
        };

        for (int index = 0; index < followUpMarkers.Length; index++)
        {
            if (lower.Contains(followUpMarkers[index]))
            {
                return true;
            }
        }

        return false;
    }
}
