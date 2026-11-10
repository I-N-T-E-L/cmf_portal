using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using OfficeOpenXml;

public sealed class CmfAssistantUnifiedConfig
{
    public string Version { get; set; }
    public string GnaiEndpoint { get; set; }
    public string GnaiAnthropicEndpoint { get; set; }
    public string GnaiOpenAiEndpoint { get; set; }
    public string GnaiApiKey { get; set; }
    public string PrimaryModel { get; set; }
    public string FastSummaryModel { get; set; }
    public List<string> FallbackModels { get; set; }
    public string GnaiProxy { get; set; }
    public int RequestTimeoutSeconds { get; set; }

    public bool HsdEnabled { get; set; }
    public List<string> HsdProviderOrder { get; set; }
    public string HsdArticleApiBaseUrl { get; set; }
    public int HsdTimeoutSeconds { get; set; }
    public int HsdCacheRefreshSeconds { get; set; }
    public bool SkipCommentEndpointsWhenHistoryPresent { get; set; }

    public string DatabaseConnectionName { get; set; }
    public string DatabaseConnectionString { get; set; }
    public string DatabaseProviderName { get; set; }
    public bool DatabaseFallbackToWebConfig { get; set; }

    public bool FeedbackMemoryEnabled { get; set; }
    public string FeedbackStoragePath { get; set; }
    public int MaxFewShotExamples { get; set; }

    public int ScaleMax { get; set; }
    public string Formula { get; set; }
    public double CmfOkMinScore { get; set; }
    public double CmfReviewMinScore { get; set; }
    public int ThresholdScore { get; set; }
    public int ReviewThresholdScore { get; set; }
    public int CustomerExperienceOverrideScore { get; set; }
    public bool RequireComponentInDescription { get; set; }
    public bool RequireRvpReproInDescription { get; set; }
    public bool RequirePromotedIdInHeader { get; set; }
    public bool RequireRvpReproInReasoning { get; set; }
    public string RvpReproPolicy { get; set; }
    public Dictionary<string, int> DimensionWeights { get; set; }
    public string CompiledRulesText { get; set; }

    public Dictionary<string, List<string>> PlatformFamilyJargons { get; set; }
    public Dictionary<string, List<string>> PlatformSkuJargons { get; set; }
    public Dictionary<string, List<string>> ComponentSynonymJargons { get; set; }
}

public static class CmfAssistantConfigService
{
    private static readonly object SyncRoot = new object();
    private static CmfAssistantUnifiedConfig CachedConfig;
    private static DateTime CachedLastWriteUtc = DateTime.MinValue;
    private static string CachedConfigPath;

    public static CmfAssistantUnifiedConfig GetConfig()
    {
        lock (SyncRoot)
        {
            string path = ResolveConfigFilePath();
            DateTime lastWriteUtc = (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                ? File.GetLastWriteTimeUtc(path)
                : DateTime.MinValue;

            if (CachedConfig != null && string.Equals(CachedConfigPath, path, StringComparison.OrdinalIgnoreCase) && lastWriteUtc == CachedLastWriteUtc)
            {
                return CachedConfig;
            }

            CachedConfig = LoadConfigFromFile(path);
            CachedConfigPath = path;
            CachedLastWriteUtc = lastWriteUtc;
            return CachedConfig;
        }
    }

    public static string ResolveGnaiEndpoint()
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        if (!string.IsNullOrWhiteSpace(cfg.GnaiEndpoint))
        {
            return cfg.GnaiEndpoint.Trim();
        }
        return "https://gnai.intel.com/api/providers/anthropic/v1/messages";
    }

    public static string ResolveGnaiEndpointForModel(string baseEndpoint, string model)
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        if (!string.IsNullOrWhiteSpace(cfg.GnaiAnthropicEndpoint))
        {
            return cfg.GnaiAnthropicEndpoint.Trim();
        }
        if (!string.IsNullOrWhiteSpace(baseEndpoint))
        {
            return baseEndpoint.Trim();
        }
        return ResolveGnaiEndpoint();
    }

    public static bool IsAnthropicMessagesEndpoint(string endpoint, string model)
    {
        if (!string.IsNullOrWhiteSpace(endpoint) && endpoint.IndexOf("/providers/anthropic/", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }
        return !string.IsNullOrWhiteSpace(model) && model.Trim().StartsWith("claude-", StringComparison.OrdinalIgnoreCase);
    }

    public static string ResolveGnaiToken()
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        if (!string.IsNullOrWhiteSpace(cfg.GnaiApiKey)
            && !string.Equals(cfg.GnaiApiKey, "FROM_ENV_OR_WEB_CONFIG", StringComparison.OrdinalIgnoreCase)
            && !cfg.GnaiApiKey.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            return cfg.GnaiApiKey.Trim();
        }
        return string.Empty;
    }

    public static string ResolveDatabaseConnectionString()
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        if (cfg != null
            && !string.IsNullOrWhiteSpace(cfg.DatabaseConnectionString)
            && !string.Equals(cfg.DatabaseConnectionString, "FROM_WEB_CONFIG", StringComparison.OrdinalIgnoreCase)
            && !cfg.DatabaseConnectionString.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            return cfg.DatabaseConnectionString.Trim();
        }
        return string.Empty;
    }

    public static string ResolvePrimaryModel()
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        if (!string.IsNullOrWhiteSpace(cfg.PrimaryModel)) return cfg.PrimaryModel.Trim();
        return "claude-4-5-sonnet";
    }

    public static string ResolveFastSummaryModel()
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        if (!string.IsNullOrWhiteSpace(cfg.FastSummaryModel)) return cfg.FastSummaryModel.Trim();
        return "claude-4-5-haiku";
    }

    public static List<string> ResolveModelChain(string preferredPrimaryModel)
    {
        CmfAssistantUnifiedConfig cfg = GetConfig();
        List<string> chain = new List<string>();
        string primary = string.IsNullOrWhiteSpace(preferredPrimaryModel) ? ResolvePrimaryModel() : preferredPrimaryModel.Trim();
        chain.Add(primary);

        if (cfg.FallbackModels != null)
        {
            foreach (string candidate in cfg.FallbackModels)
            {
                if (!string.IsNullOrWhiteSpace(candidate)
                    && candidate.Trim().StartsWith("claude-", StringComparison.OrdinalIgnoreCase)
                    && !chain.Exists(m => string.Equals(m, candidate.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    chain.Add(candidate.Trim());
                }
            }
        }

        if (!chain.Exists(m => string.Equals(m, "claude-4-5-haiku", StringComparison.OrdinalIgnoreCase)))
        {
            chain.Add("claude-4-5-haiku");
        }

        return chain;
    }

    private static string ResolveConfigFilePath()
    {
        try
        {
            string mapped = HostingEnvironment.MapPath("~/App_Data/cmf-assistant-config.json");
            if (!string.IsNullOrWhiteSpace(mapped) && File.Exists(mapped)) return mapped;
        }
        catch { }

        string[] candidates = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "App_Data", "cmf-assistant-config.json"),
            Path.Combine(Environment.CurrentDirectory ?? string.Empty, "App_Data", "cmf-assistant-config.json"),
            Path.Combine(Environment.CurrentDirectory ?? string.Empty, "cmf_portal", "App_Data", "cmf-assistant-config.json")
        };

        foreach (string candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate)) return candidate;
        }
        return candidates[0];
    }

    private static CmfAssistantUnifiedConfig LoadConfigFromFile(string path)
    {
        CmfAssistantUnifiedConfig config = CreateDefaultConfig();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return config;

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
            IDictionary root = serializer.DeserializeObject(json) as IDictionary;
            if (root == null) return config;

            config.Version = ReadString(root, "version", config.Version);

            IDictionary gnai = root["gnai"] as IDictionary;
            if (gnai != null)
            {
                config.GnaiEndpoint = ReadString(gnai, "endpoint", config.GnaiEndpoint);
                config.GnaiAnthropicEndpoint = ReadString(gnai, "anthropicEndpoint", config.GnaiAnthropicEndpoint);
                config.GnaiOpenAiEndpoint = ReadString(gnai, "openaiEndpoint", config.GnaiOpenAiEndpoint);
                config.GnaiApiKey = ReadString(gnai, "apiKey", config.GnaiApiKey);
                config.PrimaryModel = ReadString(gnai, "primaryModel", config.PrimaryModel);
                config.FastSummaryModel = ReadString(gnai, "fastSummaryModel", config.FastSummaryModel);
                config.GnaiProxy = ReadString(gnai, "proxy", config.GnaiProxy);
                config.RequestTimeoutSeconds = ReadInt(gnai, "requestTimeoutSeconds", config.RequestTimeoutSeconds);
                List<string> fallbacks = ReadStringList(gnai, "fallbackModels");
                if (fallbacks.Count > 0) config.FallbackModels = fallbacks;
            }

            IDictionary hsd = root["hsd"] as IDictionary;
            if (hsd != null)
            {
                config.HsdEnabled = ReadBool(hsd, "enabled", config.HsdEnabled);
                config.HsdArticleApiBaseUrl = ReadString(hsd, "articleApiBaseUrl", config.HsdArticleApiBaseUrl);
                config.HsdTimeoutSeconds = ReadInt(hsd, "timeoutSeconds", config.HsdTimeoutSeconds);
                config.HsdCacheRefreshSeconds = ReadInt(hsd, "cacheRefreshSeconds", config.HsdCacheRefreshSeconds);
                config.SkipCommentEndpointsWhenHistoryPresent = ReadBool(hsd, "skipCommentEndpointsWhenHistoryPresent", config.SkipCommentEndpointsWhenHistoryPresent);
                List<string> providers = ReadStringList(hsd, "providerOrder");
                if (providers.Count > 0) config.HsdProviderOrder = providers;
            }

            IDictionary database = root["database"] as IDictionary;
            if (database != null)
            {
                config.DatabaseConnectionName = ReadString(database, "connectionName", config.DatabaseConnectionName);
                config.DatabaseConnectionString = ReadString(database, "connectionString", config.DatabaseConnectionString);
                config.DatabaseProviderName = ReadString(database, "providerName", config.DatabaseProviderName);
                config.DatabaseFallbackToWebConfig = ReadBool(database, "fallbackToWebConfig", config.DatabaseFallbackToWebConfig);
            }

            IDictionary plugins = root["plugins"] as IDictionary;
            if (plugins != null)
            {
                IDictionary feedback = plugins["feedbackMemory"] as IDictionary;
                if (feedback != null)
                {
                    config.FeedbackMemoryEnabled = ReadBool(feedback, "enabled", config.FeedbackMemoryEnabled);
                    config.FeedbackStoragePath = ReadString(feedback, "storagePath", config.FeedbackStoragePath);
                    config.MaxFewShotExamples = ReadInt(feedback, "maxFewShotExamples", config.MaxFewShotExamples);
                }
            }

            IDictionary scoring = root["scoringRules"] as IDictionary;
            if (scoring != null)
            {
                config.ScaleMax = ReadInt(scoring, "scaleMax", config.ScaleMax);
                config.Formula = ReadString(scoring, "formula", config.Formula);
                config.CmfOkMinScore = ReadDouble(scoring, "cmfOkMinScore", config.CmfOkMinScore);
                config.CmfReviewMinScore = ReadDouble(scoring, "cmfReviewMinScore", config.CmfReviewMinScore);
                config.ThresholdScore = ReadInt(scoring, "thresholdScore", config.ThresholdScore);
                config.ReviewThresholdScore = ReadInt(scoring, "reviewThresholdScore", config.ReviewThresholdScore);
                config.CustomerExperienceOverrideScore = ReadInt(scoring, "customerExperienceOverrideScore", config.CustomerExperienceOverrideScore);

                IDictionary mandatory = scoring["mandatoryRequirements"] as IDictionary;
                if (mandatory != null)
                {
                    config.RequireComponentInDescription = ReadBool(mandatory, "requireComponentInDescription", config.RequireComponentInDescription);
                    config.RequireRvpReproInDescription = ReadBool(mandatory, "requireRvpReproInDescription", config.RequireRvpReproInDescription);
                    config.RequirePromotedIdInHeader = ReadBool(mandatory, "requirePromotedIdInHeader", config.RequirePromotedIdInHeader);
                    config.RequireRvpReproInReasoning = ReadBool(mandatory, "requireRvpReproInReasoning", config.RequireRvpReproInReasoning);
                    config.RvpReproPolicy = ReadString(mandatory, "rvpReproPolicy", config.RvpReproPolicy);
                }

                IList dimensions = scoring["dimensions"] as IList;
                if (dimensions != null && dimensions.Count > 0)
                {
                    StringBuilder rulesBuilder = new StringBuilder();
                    rulesBuilder.AppendLine("scale_max: " + config.ScaleMax);
                    rulesBuilder.AppendLine("formula: " + config.Formula);
                    rulesBuilder.AppendLine("cmf_ok_min_score: " + config.CmfOkMinScore.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    rulesBuilder.AppendLine("cmf_review_min_score: " + config.CmfReviewMinScore.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    rulesBuilder.AppendLine("threshold_for_cmf_tag: " + config.ThresholdScore);
                    rulesBuilder.AppendLine("review_threshold: " + config.ReviewThresholdScore);
                    rulesBuilder.AppendLine("customer_experience_override: " + config.CustomerExperienceOverrideScore);
                    if (!string.IsNullOrWhiteSpace(config.RvpReproPolicy))
                    {
                        rulesBuilder.AppendLine("rvp_repro_policy: " + config.RvpReproPolicy);
                    }
                    rulesBuilder.AppendLine("scoring_dimensions (1 to " + config.ScaleMax + " scale):");
                    foreach (object item in dimensions)
                    {
                        IDictionary dim = item as IDictionary;
                        if (dim == null) continue;
                        string id = ReadString(dim, "id", string.Empty);
                        string name = ReadString(dim, "name", string.Empty);
                        int weight = ReadInt(dim, "weight", 15);
                        string rubric = ReadString(dim, "rubric", string.Empty);
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            config.DimensionWeights[id.ToUpperInvariant()] = weight;
                            rulesBuilder.AppendLine("  " + id + " | " + name + " | Weight " + weight + " | " + rubric);
                        }
                    }

                    IList dispositions = scoring["dispositionTable"] as IList;
                    if (dispositions != null && dispositions.Count > 0)
                    {
                        rulesBuilder.AppendLine("cmf_disposition_table:");
                        foreach (object item in dispositions)
                        {
                            IDictionary disp = item as IDictionary;
                            if (disp == null) continue;
                            rulesBuilder.AppendFormat(
                                "  {0} ({1}): Score {2} | Min Conditions: {3} | Meaning: {4} | Action: {5}\n",
                                ReadString(disp, "disposition", string.Empty),
                                ReadString(disp, "output", string.Empty),
                                ReadString(disp, "scoreBand", string.Empty),
                                ReadString(disp, "minimumConditions", string.Empty),
                                ReadString(disp, "meaning", string.Empty),
                                ReadString(disp, "requiredAction", string.Empty));
                        }
                    }

                    IList guardrails = scoring["gatingGuardrails"] as IList;
                    if (guardrails != null && guardrails.Count > 0)
                    {
                        rulesBuilder.AppendLine("gating_guardrails:");
                        foreach (object item in guardrails)
                        {
                            IDictionary gr = item as IDictionary;
                            if (gr == null) continue;
                            rulesBuilder.AppendFormat(
                                "  - {0}: {1} ({2})\n",
                                ReadString(gr, "name", string.Empty),
                                ReadString(gr, "rule", string.Empty),
                                ReadString(gr, "rationale", string.Empty));
                        }
                    }

                    config.CompiledRulesText = rulesBuilder.ToString().Trim();
                }
            }

            IDictionary jargons = root["jargonMappings"] as IDictionary;
            if (jargons != null)
            {
                Dictionary<string, List<string>> famMap = ReadDictionaryOfStringLists(jargons["platformFamilies"] as IDictionary);
                if (famMap.Count > 0) config.PlatformFamilyJargons = famMap;

                Dictionary<string, List<string>> skuMap = ReadDictionaryOfStringLists(jargons["platformSkus"] as IDictionary);
                if (skuMap.Count > 0) config.PlatformSkuJargons = skuMap;

                Dictionary<string, List<string>> compMap = ReadDictionaryOfStringLists(jargons["componentSynonyms"] as IDictionary);
                if (compMap.Count > 0) config.ComponentSynonymJargons = compMap;
            }

            // Overlay local-only credentials from cmf-assistant-config.local.json (git-ignored) when main config uses placeholders
            try
            {
                string dir = Path.GetDirectoryName(path) ?? string.Empty;
                string localPath = Path.Combine(dir, "cmf-assistant-config.local.json");
                if (File.Exists(localPath))
                {
                    IDictionary localRoot = serializer.DeserializeObject(File.ReadAllText(localPath, Encoding.UTF8)) as IDictionary;
                    if (localRoot != null)
                    {
                        IDictionary localGnai = localRoot["gnai"] as IDictionary;
                        if (localGnai != null)
                        {
                            string localKey = ReadString(localGnai, "apiKey", string.Empty);
                            if (!string.IsNullOrWhiteSpace(localKey) && !string.Equals(localKey, "FROM_ENV_OR_WEB_CONFIG", StringComparison.OrdinalIgnoreCase))
                            {
                                config.GnaiApiKey = localKey;
                            }
                        }
                        IDictionary localDb = localRoot["database"] as IDictionary;
                        if (localDb != null)
                        {
                            string localConn = ReadString(localDb, "connectionString", string.Empty);
                            if (!string.IsNullOrWhiteSpace(localConn) && !string.Equals(localConn, "FROM_WEB_CONFIG", StringComparison.OrdinalIgnoreCase))
                            {
                                config.DatabaseConnectionString = localConn;
                            }
                        }
                    }
                }
            }
            catch { }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceWarning("CmfAssistantConfigService load error: {0}", ex.Message);
        }

        return config;
    }

    private static CmfAssistantUnifiedConfig CreateDefaultConfig()
    {
        return new CmfAssistantUnifiedConfig
        {
            Version = "1.0",
            GnaiEndpoint = "https://gnai.intel.com/api/providers/anthropic/v1/messages",
            GnaiAnthropicEndpoint = "https://gnai.intel.com/api/providers/anthropic/v1/messages",
            GnaiOpenAiEndpoint = string.Empty,
            GnaiApiKey = "REPLACE_WITH_GNAI_TOKEN",
            PrimaryModel = "claude-4-5-sonnet",
            FastSummaryModel = "claude-4-5-haiku",
            FallbackModels = new List<string> { "claude-4-5-haiku" },
            GnaiProxy = string.Empty,
            RequestTimeoutSeconds = 30,
            HsdEnabled = true,
            HsdProviderOrder = new List<string> { "rest", "gnai" },
            HsdArticleApiBaseUrl = "https://hsdes-api.intel.com/rest/article",
            HsdTimeoutSeconds = 12,
            HsdCacheRefreshSeconds = 120,
            SkipCommentEndpointsWhenHistoryPresent = true,
            DatabaseConnectionName = "gfxitt",
            DatabaseConnectionString = "REPLACE_WITH_SQL_CONNECTION_STRING",
            DatabaseProviderName = "System.Data.SqlClient",
            DatabaseFallbackToWebConfig = false,
            FeedbackMemoryEnabled = true,
            FeedbackStoragePath = "~/App_Data/cmf-feedback-memory.json",
            MaxFewShotExamples = 4,
            ScaleMax = 4,
            Formula = "Overall Score = (S * 0.20) + (O * 0.15) + (D * 0.15) + (R * 0.10) + (B * 0.20) + (CX * 0.20)",
            CmfOkMinScore = 3.50,
            CmfReviewMinScore = 2.75,
            ThresholdScore = 88,
            ReviewThresholdScore = 69,
            CustomerExperienceOverrideScore = 3,
            RequireComponentInDescription = true,
            RequireRvpReproInDescription = true,
            RequirePromotedIdInHeader = true,
            RequireRvpReproInReasoning = true,
            RvpReproPolicy = "RVP repro evidence is critical: if RVP_repro is Yes, the issue reproduces on the Intel Reference Validation Platform (RVP), meaning there is a high likelihood the defect appears across most OEM customer units designed on that RVP platform (critical platform-wide impact). Always consider and explicitly mention RVP repro status in the CMF reasoning bullet points and recommendation justification.",
            DimensionWeights = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "S", 20 }, { "O", 15 }, { "D", 15 }, { "R", 10 }, { "B", 20 }, { "CX", 20 }
            },
            CompiledRulesText = string.Empty,
            PlatformFamilyJargons = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "NVL", new List<string> { "novalake", "nova lake", "nvl" } },
                { "PTL", new List<string> { "pantherlake", "panther lake", "ptl" } },
                { "LNL", new List<string> { "lunarlake", "lunar lake", "lnl" } },
                { "ARL", new List<string> { "arrowlake", "arrow lake", "arl" } },
                { "WCL", new List<string> { "wildcatlake", "wildcat lake", "wcl" } },
                { "GNR", new List<string> { "graniterapids", "granite rapids", "gnr" } }
            },
            PlatformSkuJargons = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "NVL-H", new List<string> { "novalake-h", "novalake h", "nova lake-h", "nova lake h", "nvl-h", "nvl h", "nvl_h" } },
                { "NVL-S", new List<string> { "novalake-s", "novalake s", "nova lake-s", "nova lake s", "nvl-s", "nvl s", "nvl_s" } },
                { "NVL-Hx", new List<string> { "novalake-hx", "novalake hx", "nova lake-hx", "nova lake hx", "nvl-hx", "nvl hx", "nvl_hx" } },
                { "NVL-U", new List<string> { "novalake-u", "novalake u", "nova lake-u", "nova lake u", "nvl-u", "nvl u", "nvl_u" } }
            },
            ComponentSynonymJargons = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "iGFX (Graphics)", new List<string> { "graphics", "igfx", "display", "gfx", "igpu", "media" } },
                { "Audio", new List<string> { "audio", "sound", "speaker", "microphone", "mic", "voice" } },
                { "TBT", new List<string> { "thunderbolt", "tbt", "usb4", "type-c", "usb-c", "usbc" } },
                { "BIOS", new List<string> { "bios", "ifwi", "firmware", "uefi" } },
                { "CSME", new List<string> { "csme", "cse", "manageability" } },
                { "PnP", new List<string> { "pnp", "power", "battery", "modern standby", "s0ix" } },
                { "DTT", new List<string> { "dtt", "thermal", "temperature" } }
            }
        };
    }

    private static Dictionary<string, List<string>> ReadDictionaryOfStringLists(IDictionary dict)
    {
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (dict == null) return result;
        foreach (DictionaryEntry entry in dict)
        {
            if (entry.Key == null) continue;
            string key = entry.Key.ToString().Trim();
            if (string.IsNullOrWhiteSpace(key)) continue;
            IList rawList = entry.Value as IList;
            if (rawList == null) continue;
            List<string> values = new List<string>();
            foreach (object item in rawList)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ToString()))
                {
                    values.Add(item.ToString().Trim());
                }
            }
            if (values.Count > 0) result[key] = values;
        }
        return result;
    }

    private static string ReadString(IDictionary dict, string key, string fallback)
    {
        if (dict == null || !dict.Contains(key) || dict[key] == null) return fallback;
        string val = dict[key].ToString().Trim();
        return string.IsNullOrWhiteSpace(val) ? fallback : val;
    }

    private static int ReadInt(IDictionary dict, string key, int fallback)
    {
        if (dict == null || !dict.Contains(key) || dict[key] == null) return fallback;
        int parsed;
        return int.TryParse(dict[key].ToString().Trim(), out parsed) ? parsed : fallback;
    }

    private static double ReadDouble(IDictionary dict, string key, double fallback)
    {
        if (dict == null || !dict.Contains(key) || dict[key] == null) return fallback;
        double parsed;
        return double.TryParse(dict[key].ToString().Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsed) ? parsed : fallback;
    }

    private static bool ReadBool(IDictionary dict, string key, bool fallback)
    {
        if (dict == null || !dict.Contains(key) || dict[key] == null) return fallback;
        bool parsed;
        return bool.TryParse(dict[key].ToString().Trim(), out parsed) ? parsed : fallback;
    }

    private static List<string> ReadStringList(IDictionary dict, string key)
    {
        List<string> list = new List<string>();
        if (dict == null || !dict.Contains(key)) return list;
        IList raw = dict[key] as IList;
        if (raw == null) return list;
        foreach (object item in raw)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.ToString()))
            {
                list.Add(item.ToString().Trim());
            }
        }
        return list;
    }

    public static bool TryAutoLearnJargonFromText(string text, out string category, out string targetCode, out string aliasTerm)
    {
        category = string.Empty;
        targetCode = string.Empty;
        aliasTerm = string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Patterns such as: "map soundwire to Audio", "MTL means Meteor Lake", "treat display engine as iGFX (Graphics)"
        Match m = Regex.Match(
            text.Trim(),
            @"(?:map|treat|alias|jargon|synonym)?\s*[""']?(?<left>[A-Za-z0-9 _/-]{2,32})[""']?\s+(?:means|stands\s+for|maps\s+to|is\s+alias\s+for|->|=|\bis\b|\bto\b|\bas\b)\s+[""']?(?<right>[A-Za-z0-9 ()_/-]{2,32})[""']?",
            RegexOptions.IgnoreCase);
        if (!m.Success) return false;

        string left = m.Groups["left"].Value.Trim();
        string right = m.Groups["right"].Value.Trim();
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right) || string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        CmfAssistantUnifiedConfig cfg = GetConfig();
        string candidateTarget = right;
        string candidateAlias = left;

        // If left is a short code (2-6 chars like MTL, RPL, NVL-P) and right is a longer name ("Meteor Lake"), left is the target code
        if (left.Length <= 6 && right.Length > left.Length && !IsKnownComponentTarget(right, cfg))
        {
            candidateTarget = left.ToUpperInvariant();
            candidateAlias = right.ToLowerInvariant();
        }
        else if (IsKnownComponentTarget(right, cfg))
        {
            candidateTarget = ResolveCanonicalComponentTarget(right, cfg);
            candidateAlias = left.ToLowerInvariant();
            category = "componentSynonyms";
        }
        else if (IsKnownComponentTarget(left, cfg))
        {
            candidateTarget = ResolveCanonicalComponentTarget(left, cfg);
            candidateAlias = right.ToLowerInvariant();
            category = "componentSynonyms";
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            category = candidateTarget.Contains("-") ? "platformSkus" : "platformFamilies";
        }

        targetCode = candidateTarget;
        aliasTerm = candidateAlias;
        string statusMsg;
        return RegisterLearnedJargonSynonym(category, targetCode, aliasTerm, out statusMsg);
    }

    private static bool IsKnownComponentTarget(string val, CmfAssistantUnifiedConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(val)) return false;
        string v = val.Trim();
        if (Regex.IsMatch(v, @"^(?:iGFX|Graphics|Audio|TBT|BIOS|CSME|PnP|DTT)$", RegexOptions.IgnoreCase)) return true;
        if (cfg != null && cfg.ComponentSynonymJargons != null && cfg.ComponentSynonymJargons.ContainsKey(v)) return true;
        return false;
    }

    private static string ResolveCanonicalComponentTarget(string val, CmfAssistantUnifiedConfig cfg)
    {
        string v = (val ?? string.Empty).Trim();
        if (Regex.IsMatch(v, @"^(?:igfx|graphics)$", RegexOptions.IgnoreCase)) return "iGFX (Graphics)";
        if (cfg != null && cfg.ComponentSynonymJargons != null)
        {
            foreach (string key in cfg.ComponentSynonymJargons.Keys)
            {
                if (string.Equals(key, v, StringComparison.OrdinalIgnoreCase)) return key;
            }
        }
        return v;
    }

    public static bool RegisterLearnedJargonSynonym(string category, string canonicalTarget, string synonym, out string message)
    {
        message = string.Empty;
        string cleanCat = string.IsNullOrWhiteSpace(category) ? "platformFamilies" : category.Trim();
        string cleanTarget = (canonicalTarget ?? string.Empty).Trim();
        string cleanSynonym = (synonym ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(cleanTarget) || string.IsNullOrWhiteSpace(cleanSynonym))
        {
            message = "Both target code/component and synonym are required.";
            return false;
        }

        if (!string.Equals(cleanCat, "platformFamilies", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(cleanCat, "platformSkus", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(cleanCat, "componentSynonyms", StringComparison.OrdinalIgnoreCase))
        {
            cleanCat = cleanTarget.Contains("-") ? "platformSkus" : "platformFamilies";
        }

        lock (SyncRoot)
        {
            try
            {
                string path = ResolveConfigFilePath();
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    message = "Config file not found.";
                    return false;
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
                IDictionary root = serializer.DeserializeObject(File.ReadAllText(path, Encoding.UTF8)) as IDictionary;
                if (root == null)
                {
                    message = "Unable to parse config JSON.";
                    return false;
                }

                IDictionary jargons = root["jargonMappings"] as IDictionary;
                if (jargons == null)
                {
                    jargons = new Dictionary<string, object>();
                    root["jargonMappings"] = jargons;
                }

                IDictionary subMap = jargons[cleanCat] as IDictionary;
                if (subMap == null)
                {
                    subMap = new Dictionary<string, object>();
                    jargons[cleanCat] = subMap;
                }

                List<string> list = ReadStringList(subMap, cleanTarget);
                bool exists = false;
                foreach (string s in list)
                {
                    if (string.Equals(s, cleanSynonym, StringComparison.OrdinalIgnoreCase)) { exists = true; break; }
                }
                if (!exists)
                {
                    list.Add(cleanSynonym);
                }
                subMap[cleanTarget] = list.ToArray();

                File.WriteAllText(path, FormatPrettyJson(serializer.Serialize(root)), Encoding.UTF8);
                CachedConfig = null;
                CachedLastWriteUtc = DateTime.MinValue;
                message = "Saved jargon mapping: '" + cleanSynonym + "' -> '" + cleanTarget + "' (" + cleanCat + ").";
                return true;
            }
            catch (Exception ex)
            {
                message = "Failed to save jargon mapping: " + ex.Message;
                return false;
            }
        }
    }

    public static bool ImportScoringGuidelinesFromExcel(Stream excelStream, out string statusMessage)
    {
        statusMessage = string.Empty;
        if (excelStream == null)
        {
            statusMessage = "No Excel file stream provided.";
            return false;
        }

        lock (SyncRoot)
        {
            try
            {
                using (ExcelPackage pkg = new ExcelPackage(excelStream))
                {
                    if (pkg.Workbook == null || pkg.Workbook.Worksheets == null || pkg.Workbook.Worksheets.Count == 0)
                    {
                        statusMessage = "The uploaded Excel workbook contains no worksheets.";
                        return false;
                    }

                    // 1. Locate rubric sheet ("Updated Scoring Guidelines" preferred, otherwise first sheet with Severity/Occurrence headers)
                    ExcelWorksheet rubricSheet = pkg.Workbook.Worksheets["Updated Scoring Guidelines"]
                        ?? pkg.Workbook.Worksheets["Scoring Guidelines"]
                        ?? pkg.Workbook.Worksheets[1];

                    string[] dimIds = new[] { "S", "O", "D", "R", "B", "CX" };
                    string[] dimNames = new[] { "Severity", "Occurrence", "Detection", "Recovery", "Business Impact", "Customer Experience" };
                    int[] dimCols = new[] { 2, 3, 4, 5, 6, 7 };
                    Dictionary<string, int> weights = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "S", 20 }, { "O", 15 }, { "D", 15 }, { "R", 10 }, { "B", 20 }, { "CX", 20 }
                    };
                    Dictionary<string, List<string>> dimScoreParts = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                    foreach (string id in dimIds) dimScoreParts[id] = new List<string>();

                    int maxScoreFound = 4;
                    string currentScoreLabel = string.Empty;
                    int endRow = rubricSheet.Dimension != null ? rubricSheet.Dimension.End.Row : 0;

                    for (int r = 1; r <= endRow; r++)
                    {
                        string c1 = (rubricSheet.Cells[r, 1].Text ?? string.Empty).Trim();
                        if (Regex.IsMatch(c1, @"^[1-5]$"))
                        {
                            currentScoreLabel = c1;
                            int parsedS;
                            if (int.TryParse(c1, out parsedS) && parsedS > maxScoreFound) maxScoreFound = parsedS;
                        }
                        else if (c1.StartsWith("Weight", StringComparison.OrdinalIgnoreCase))
                        {
                            currentScoreLabel = string.Empty;
                            for (int d = 0; d < dimIds.Length; d++)
                            {
                                string wText = (rubricSheet.Cells[r, dimCols[d]].Text ?? string.Empty).Replace("%", string.Empty).Trim();
                                int wVal;
                                if (int.TryParse(wText, out wVal) && wVal > 0)
                                {
                                    weights[dimIds[d]] = wVal;
                                }
                            }
                            continue;
                        }
                        else if (!string.IsNullOrWhiteSpace(c1))
                        {
                            currentScoreLabel = string.Empty;
                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(currentScoreLabel))
                        {
                            for (int d = 0; d < dimIds.Length; d++)
                            {
                                string cellText = (rubricSheet.Cells[r, dimCols[d]].Text ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
                                if (!string.IsNullOrWhiteSpace(cellText))
                                {
                                    List<string> list = dimScoreParts[dimIds[d]];
                                    string prefix = currentScoreLabel + "=";
                                    int existingIdx = -1;
                                    for (int k = 0; k < list.Count; k++)
                                    {
                                        if (list[k].StartsWith(prefix, StringComparison.Ordinal)) { existingIdx = k; break; }
                                    }
                                    if (existingIdx >= 0)
                                    {
                                        list[existingIdx] = list[existingIdx].TrimEnd('.', ';', ' ') + " OR " + cellText;
                                    }
                                    else
                                    {
                                        list.Add(prefix + cellText);
                                    }
                                }
                            }
                        }
                    }

                    // 2. Locate disposition & guardrails sheet ("6-Category Scoring Mechanism" if present)
                    ExcelWorksheet mechSheet = pkg.Workbook.Worksheets["6-Category Scoring Mechanism"];
                    double cmfOkMin = 3.50;
                    double cmfReviewMin = 2.75;
                    List<Dictionary<string, object>> dispRows = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> guardrailRows = new List<Dictionary<string, object>>();

                    if (mechSheet != null && mechSheet.Dimension != null)
                    {
                        bool inDisposition = false;
                        bool inGuardrails = false;
                        for (int r = 1; r <= mechSheet.Dimension.End.Row; r++)
                        {
                            string col1 = (mechSheet.Cells[r, 1].Text ?? string.Empty).Trim();
                            string col2 = (mechSheet.Cells[r, 2].Text ?? string.Empty).Trim();
                            if (col1.IndexOf("CMF Disposition", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                inDisposition = true;
                                inGuardrails = false;
                                continue;
                            }
                            if (col1.IndexOf("Gating Guardrails", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                inDisposition = false;
                                inGuardrails = true;
                                continue;
                            }

                            if (inDisposition && col1.StartsWith("CMF_", StringComparison.OrdinalIgnoreCase))
                            {
                                string col3 = (mechSheet.Cells[r, 3].Text ?? string.Empty).Trim();
                                string col4 = (mechSheet.Cells[r, 4].Text ?? string.Empty).Trim();
                                string col5 = (mechSheet.Cells[r, 5].Text ?? string.Empty).Trim();
                                string col8 = (mechSheet.Cells[r, 8].Text ?? string.Empty).Trim();
                                dispRows.Add(new Dictionary<string, object>
                                {
                                    { "disposition", col1.ToUpperInvariant() },
                                    { "scoreBand", col2 },
                                    { "minimumConditions", col3 },
                                    { "meaning", col4 },
                                    { "requiredAction", col5 },
                                    { "output", string.IsNullOrWhiteSpace(col8) ? col1 : col8 }
                                });

                                Match numMatch = Regex.Match(col2, @"(\d+\.\d+)");
                                if (numMatch.Success)
                                {
                                    double parsedNum;
                                    if (double.TryParse(numMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedNum))
                                    {
                                        if (string.Equals(col1, "CMF_OK", StringComparison.OrdinalIgnoreCase)) cmfOkMin = parsedNum;
                                        else if (string.Equals(col1, "CMF_REVIEW", StringComparison.OrdinalIgnoreCase)) cmfReviewMin = parsedNum;
                                    }
                                }
                            }
                            else if (inGuardrails && !string.IsNullOrWhiteSpace(col1) && !string.IsNullOrWhiteSpace(col2))
                            {
                                string col6 = (mechSheet.Cells[r, 6].Text ?? string.Empty).Trim();
                                guardrailRows.Add(new Dictionary<string, object>
                                {
                                    { "id", Regex.Replace(col1.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_') },
                                    { "name", col1 },
                                    { "rule", col2 },
                                    { "rationale", col6 }
                                });
                            }
                        }
                    }

                    // Ensure 5-Tag Model condition entries (CMF_INCOMPLETE & CMF_DUPLICATE) remain in dispositionTable
                    bool hasIncompleteDisp = false;
                    bool hasDuplicateDisp = false;
                    foreach (Dictionary<string, object> dr in dispRows)
                    {
                        string dName = Convert.ToString(dr["disposition"]);
                        if (string.Equals(dName, "CMF_INCOMPLETE", StringComparison.OrdinalIgnoreCase)) hasIncompleteDisp = true;
                        if (string.Equals(dName, "CMF_DUPLICATE", StringComparison.OrdinalIgnoreCase)) hasDuplicateDisp = true;
                    }
                    if (!hasIncompleteDisp)
                    {
                        dispRows.Add(new Dictionary<string, object>
                        {
                            { "disposition", "CMF_INCOMPLETE" },
                            { "scoreBand", "Condition-based (Any Score)" },
                            { "minimumConditions", "Core metric evidence missing (Guardrail #3)" },
                            { "meaning", "One or more core categories cannot be scored from the available ticket evidence." },
                            { "requiredAction", "Request missing reproduction, recovery, or impact details from the submitter before disposition." },
                            { "output", "Incomplete" }
                        });
                    }
                    if (!hasDuplicateDisp)
                    {
                        dispRows.Add(new Dictionary<string, object>
                        {
                            { "disposition", "CMF_DUPLICATE" },
                            { "scoreBand", "Condition-based (Any Score)" },
                            { "minimumConditions", "Duplicate sighting or merge ID identified in ticket/comments" },
                            { "meaning", "Issue reports the same underlying failure already tracked under a primary sighting." },
                            { "requiredAction", "Mark as cmf_duplicate and link to the primary sighting." },
                            { "output", "Duplicate" }
                        });
                    }

                    // 3. Persist updated scoringRules into cmf-assistant-config.json
                    string configPath = ResolveConfigFilePath();
                    JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = 4194304 };
                    IDictionary root = (File.Exists(configPath) ? serializer.DeserializeObject(File.ReadAllText(configPath, Encoding.UTF8)) as IDictionary : null)
                        ?? new Dictionary<string, object>();

                    IDictionary scoringNode = root["scoringRules"] as IDictionary ?? new Dictionary<string, object>();
                    scoringNode["scaleMax"] = maxScoreFound;
                    scoringNode["cmfOkMinScore"] = cmfOkMin;
                    scoringNode["cmfReviewMinScore"] = cmfReviewMin;
                    scoringNode["thresholdScore"] = (int)Math.Round((cmfOkMin / maxScoreFound) * 100.0);
                    scoringNode["reviewThresholdScore"] = (int)Math.Round((cmfReviewMin / maxScoreFound) * 100.0);

                    List<Dictionary<string, object>> updatedDims = new List<Dictionary<string, object>>();
                    for (int d = 0; d < dimIds.Length; d++)
                    {
                        string id = dimIds[d];
                        List<string> rParts = dimScoreParts[id];
                        string rubricStr = rParts.Count > 0 ? string.Join(" ", rParts.ToArray()) : string.Empty;
                        updatedDims.Add(new Dictionary<string, object>
                        {
                            { "id", id },
                            { "name", dimNames[d] },
                            { "weight", weights[id] },
                            { "rubric", rubricStr }
                        });
                    }
                    scoringNode["dimensions"] = updatedDims.ToArray();
                    if (dispRows.Count > 0) scoringNode["dispositionTable"] = dispRows.ToArray();
                    if (guardrailRows.Count > 0) scoringNode["gatingGuardrails"] = guardrailRows.ToArray();
                    root["scoringRules"] = scoringNode;

                    File.WriteAllText(configPath, FormatPrettyJson(serializer.Serialize(root)), Encoding.UTF8);
                    CachedConfig = null;
                    CachedLastWriteUtc = DateTime.MinValue;
                    CmfRecommendationService.ClearRecommendationCache();

                    statusMessage = string.Format(
                        "Successfully imported {0}-point scoring guidelines ({1} dimensions, {2} disposition bands, {3} gating guardrails) into cmf-assistant-config.json.",
                        maxScoreFound, updatedDims.Count, dispRows.Count, guardrailRows.Count);
                    return true;
                }
            }
            catch (Exception ex)
            {
                statusMessage = "Excel scoring import failed: " + ex.Message;
                return false;
            }
        }
    }

    private static string FormatPrettyJson(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson)) return "{}";
        StringBuilder sb = new StringBuilder();
        int indent = 0;
        bool inQuotes = false;
        bool escaped = false;

        foreach (char ch in rawJson)
        {
            if (escaped)
            {
                sb.Append(ch);
                escaped = false;
                continue;
            }
            if (ch == '\\' && inQuotes)
            {
                sb.Append(ch);
                escaped = true;
                continue;
            }
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                sb.Append(ch);
                continue;
            }
            if (!inQuotes)
            {
                if (ch == '{' || ch == '[')
                {
                    sb.Append(ch);
                    sb.AppendLine();
                    indent++;
                    sb.Append(new string(' ', indent * 2));
                }
                else if (ch == '}' || ch == ']')
                {
                    sb.AppendLine();
                    indent = Math.Max(0, indent - 1);
                    sb.Append(new string(' ', indent * 2));
                    sb.Append(ch);
                }
                else if (ch == ',')
                {
                    sb.Append(ch);
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 2));
                }
                else if (ch == ':')
                {
                    sb.Append(": ");
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    sb.Append(ch);
                }
            }
            else
            {
                sb.Append(ch);
            }
        }
        return sb.ToString();
    }
}
