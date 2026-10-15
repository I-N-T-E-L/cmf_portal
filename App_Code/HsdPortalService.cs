using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Principal;
using System.Web.Script.Serialization;

// Represents the fields fetched from the HSD portal for a single sighting/article.
public class HsdArticleData
{
    public string ArticleId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Family { get; set; }
    public string Component { get; set; }
    public string Status { get; set; }
    public string Sysdebug { get; set; }
    public string SysdebugForum { get; set; }
    public string Priority { get; set; }
    public string Submitter { get; set; }
    public string Owner { get; set; }
    public string StepsToReproduce { get; set; }
    public string ExpectedBehavior { get; set; }
    public string ActualBehavior { get; set; }
    public string WhatHappened { get; set; }
    public string WhatShouldHaveHappened { get; set; }
    public string DebugLogAttached { get; set; }
    public string SystemScopeFilename { get; set; }
    public string CustomerDetail { get; set; }
    public string CustomerSummary { get; set; }
    public string Board { get; set; }
    public string Processor { get; set; }
    public string SocStepping { get; set; }
    public string Exposure { get; set; }
    public string MeSku { get; set; }
    public string MandatoryFields { get; set; }
    public List<string> Comments { get; set; }
    public bool FetchSuccess { get; set; }
    public string FetchError { get; set; }
    public string FixDescription { get; set; }
    public string ClosedReason { get; set; }
    public string FixedVersion { get; set; }
    public string CustomerImpact { get; set; }
    public string Reproducibility { get; set; }
    public string ImplementedDate { get; set; }
    public string VerifiedDate { get; set; }
    public string ClosedDate { get; set; }
    public string CmfJustification { get; set; }
    public string InvestigationHistory { get; set; }
    public string FetchSource { get; set; }
}

public static class HsdPortalService
{
    private const string DefaultBaseUrl = "https://hsdes.intel.com/appbuilder/rest/article";
    private const int DescriptionMaxChars = 2500;
    private const int CommentMaxChars = 600;
    private const int MaxComments = 5;

    // Fetches article fields + recent comments using configured providers.
    public static HsdArticleData FetchArticle(string articleId)
    {
        var result = CreateEmptyArticleData(articleId);

        string enabledStr = ConfigurationManager.AppSettings["HSD:Enabled"];
        if (string.Equals(enabledStr, "false", StringComparison.OrdinalIgnoreCase))
        {
            result.FetchError = "HSD integration disabled (HSD:Enabled=false).";
            return result;
        }

        if (string.IsNullOrWhiteSpace(articleId))
        {
            result.FetchError = "Article ID is empty.";
            return result;
        }

        string normalizedArticleId = articleId.Trim();
        List<string> providerErrors = new List<string>();
        string[] providerOrder = ResolveProviderOrder();

        for (int providerIndex = 0; providerIndex < providerOrder.Length; providerIndex++)
        {
            string provider = providerOrder[providerIndex];
            if (string.IsNullOrWhiteSpace(provider))
            {
                continue;
            }

            HsdArticleData candidate = CreateEmptyArticleData(normalizedArticleId);

            try
            {
                bool fetched = false;
                string normalizedProvider = provider.Trim().ToLowerInvariant();

                if (normalizedProvider == "gnai" || normalizedProvider == "gnai-plugin" || normalizedProvider == "gnaiplugin")
                {
                    fetched = TryFetchWithGnaiPlugin(normalizedArticleId, candidate);
                }
                else if (normalizedProvider == "mcp")
                {
                    fetched = TryFetchWithMcp(normalizedArticleId, candidate);
                }
                else if (normalizedProvider == "rest" || normalizedProvider == "hsd" || normalizedProvider == "sspi")
                {
                    fetched = TryFetchWithRest(normalizedArticleId, candidate);
                }
                else
                {
                    candidate.FetchError = "Unknown HSD provider '" + provider + "'.";
                }

                if (fetched && HasRecognizableArticleFields(candidate))
                {
                    candidate.FetchSuccess = true;
                    candidate.FetchSource = provider.Trim();
                    return candidate;
                }

                if (string.IsNullOrWhiteSpace(candidate.FetchError))
                {
                    candidate.FetchError = "No recognizable HSD article fields returned.";
                }
            }
            catch (Exception ex)
            {
                candidate.FetchError = ex.Message;
            }

            providerErrors.Add(provider.Trim() + ": " + candidate.FetchError);
        }

        result.FetchError = providerErrors.Count > 0
            ? string.Join(" | ", providerErrors.ToArray())
            : "No HSD providers are configured.";

        return result;
    }

    // Formats HsdArticleData into a context block for the AI prompt.
    public static string FormatForAiContext(HsdArticleData data, string sectionLabel)
    {
        if (data == null || !data.FetchSuccess) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("--- " + sectionLabel + " (HSD Portal) ---");
        AppendIfSet(sb, "HSD Fetch Source", data.FetchSource);
        AppendIfSet(sb, "Title", data.Title);
        AppendIfSet(sb, "Problem Description", Truncate(data.Description, DescriptionMaxChars));
        AppendIfSet(sb, "Family", data.Family);
        AppendIfSet(sb, "Component", data.Component);
        AppendIfSet(sb, "Status", data.Status);
        AppendIfSet(sb, "Priority", data.Priority);
        AppendIfSet(sb, "Exposure", data.Exposure);
        AppendIfSet(sb, "Customer Impact", Truncate(data.CustomerImpact, 1000));
        AppendIfSet(sb, "Reproducibility", data.Reproducibility);
        AppendIfSet(sb, "Sysdebug Category", data.Sysdebug);
        AppendIfSet(sb, "Sysdebug Forum", Truncate(data.SysdebugForum, 1000));
        AppendIfSet(sb, "Submitter", data.Submitter);
        AppendIfSet(sb, "Owner", data.Owner);
        AppendIfSet(sb, "Repro Steps", Truncate(data.StepsToReproduce, 800));
        AppendIfSet(sb, "Expected Behavior", Truncate(data.ExpectedBehavior, 500));
        AppendIfSet(sb, "Actual Behavior", Truncate(data.ActualBehavior, 500));
        AppendIfSet(sb, "What Happened", Truncate(data.WhatHappened, 800));
        AppendIfSet(sb, "What Should Have Happened", Truncate(data.WhatShouldHaveHappened, 500));
        AppendIfSet(sb, "Customer Detail", Truncate(data.CustomerDetail, 500));
        AppendIfSet(sb, "Customer Summary", Truncate(data.CustomerSummary, 500));
        AppendIfSet(sb, "Mandatory Fields", Truncate(data.MandatoryFields, 1200));
        AppendIfSet(sb, "Board", data.Board);
        AppendIfSet(sb, "Processor", data.Processor);
        AppendIfSet(sb, "SoC Stepping", data.SocStepping);
        AppendIfSet(sb, "ME SKU", data.MeSku);
        AppendIfSet(sb, "Debug Log Attached", data.DebugLogAttached);
        AppendIfSet(sb, "System Scope Filename", data.SystemScopeFilename);
        AppendIfSet(sb, "CMF Justification", Truncate(data.CmfJustification, 1000));
        AppendIfSet(sb, "Fix Description", Truncate(data.FixDescription, 1000));
        AppendIfSet(sb, "Fixed Version", data.FixedVersion);
        AppendIfSet(sb, "Closed Reason", data.ClosedReason);
        AppendIfSet(sb, "Implemented Date", data.ImplementedDate);
        AppendIfSet(sb, "Verified Date", data.VerifiedDate);
        AppendIfSet(sb, "Closed Date", data.ClosedDate);

        if (data.Comments != null && data.Comments.Count > 0)
        {
            sb.AppendLine("Discussion Comments (" + data.Comments.Count + " shown):");
            for (int i = 0; i < data.Comments.Count; i++)
                sb.AppendLine("  [" + (i + 1) + "] " + data.Comments[i]);
        }

        if (!string.IsNullOrWhiteSpace(data.InvestigationHistory))
        {
            sb.AppendLine();
            sb.AppendLine("=== HSD INVESTIGATION HISTORY ===");
            sb.AppendLine(data.InvestigationHistory);
        }

        return sb.ToString().Trim();
    }

    private static HsdArticleData CreateEmptyArticleData(string articleId)
    {
        return new HsdArticleData
        {
            ArticleId = articleId,
            Comments = new List<string>(),
            FetchSuccess = false
        };
    }

    private static string[] ResolveProviderOrder()
    {
        string configured = ConfigurationManager.AppSettings["HSD:ProviderOrder"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            configured = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["HSD:McpEndpoint"])
                ? "gnai,rest"
                : "gnai,mcp,rest";
        }

        return configured.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool HasRecognizableArticleFields(HsdArticleData data)
    {
        if (data == null) return false;
        return !string.IsNullOrWhiteSpace(data.Title)
            || !string.IsNullOrWhiteSpace(data.Description)
            || !string.IsNullOrWhiteSpace(data.Status)
            || !string.IsNullOrWhiteSpace(data.Sysdebug)
            || !string.IsNullOrWhiteSpace(data.InvestigationHistory)
            || (data.Comments != null && data.Comments.Count > 0)
            || !string.IsNullOrWhiteSpace(data.FixDescription)
            || !string.IsNullOrWhiteSpace(data.ClosedReason)
            || !string.IsNullOrWhiteSpace(data.FixedVersion);
    }

    private static bool TryFetchWithRest(string articleId, HsdArticleData result)
    {
        string baseUrl = FirstNonEmpty(
            ConfigurationManager.AppSettings["HSD:ArticleApiBaseUrl"],
            ConfigurationManager.AppSettings["HSD:ApiBaseUrl"],
            DefaultBaseUrl).TrimEnd('/');

        string articleUrl = baseUrl + "/" + Uri.EscapeUriString(articleId.Trim());

        try
        {
            string json = HttpGetSspi(articleUrl);
            if (string.IsNullOrWhiteSpace(json))
            {
                result.FetchError = "Empty response from HSD API.";
                return false;
            }

            MergeArticleJsonIntoResult(json, result);

            // Try to fetch discussion comments; ignore errors since comments are optional.
            TryFetchComments(articleUrl, result);
            return HasRecognizableArticleFields(result);
        }
        catch (WebException webEx)
        {
            HttpWebResponse resp = webEx.Response as HttpWebResponse;
            result.FetchError = resp != null
                ? "HSD API HTTP " + (int)resp.StatusCode + AppendProviderBody(resp)
                : "HSD API unreachable: " + webEx.Message;
            return false;
        }
    }

    private static bool TryFetchWithGnaiPlugin(string articleId, HsdArticleData result)
    {
        string enabled = ConfigurationManager.AppSettings["HSD:GnaiEnabled"];
        if (string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase))
        {
            result.FetchError = "GNAI HSD provider disabled (HSD:GnaiEnabled=false).";
            return false;
        }

        string token = ResolveGnaiToken();
        string endpoint = FirstNonEmpty(
            Environment.GetEnvironmentVariable("GNAI_ENDPOINT"),
            ConfigurationManager.AppSettings["GNAI:Endpoint"]);
        string model = FirstNonEmpty(
            Environment.GetEnvironmentVariable("GNAI_MODEL"),
            ConfigurationManager.AppSettings["GNAI:Model"],
            "gpt-5-mini");

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(endpoint) || endpoint.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            result.FetchError = "GNAI endpoint/token not configured.";
            return false;
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;

        Hashtable payload = new Hashtable();
        payload["model"] = model;
        payload["temperature"] = 0;

        ArrayList messages = new ArrayList();
        Hashtable systemMessage = new Hashtable();
        systemMessage["role"] = "system";
        systemMessage["content"] = "You retrieve HSDES bugs/articles using available HSDES search or retrieval plugins/tools. Return only strict JSON. If the HSDES plugin/tool cannot access the requested article, return {\"fetch_success\":false,\"fetch_error\":\"reason\"}. Do not invent ticket facts.";
        messages.Add(systemMessage);

        Hashtable userMessage = new Hashtable();
        userMessage["role"] = "user";
        userMessage["content"] = BuildGnaiHsdPrompt(articleId);
        messages.Add(userMessage);
        payload["messages"] = messages;

        try
        {
            string responseBody = PostJson(endpoint, serializer.Serialize(payload), token, BuildGnaiProxy(), "CMF-Portal-HSD-GNAI/1.0");
            string content = ExtractModelContent(responseBody);
            string articleJson = ExtractJsonObject(FirstNonEmpty(content, responseBody));

            if (string.IsNullOrWhiteSpace(articleJson))
            {
                result.FetchError = "GNAI HSD provider did not return JSON article data.";
                return false;
            }

            MergeArticleJsonIntoResult(articleJson, result);
            return HasRecognizableArticleFields(result);
        }
        catch (WebException webEx)
        {
            result.FetchError = "GNAI HSD provider failed: " + BuildWebExceptionMessage(webEx);
            return false;
        }
    }

    private static bool TryFetchWithMcp(string articleId, HsdArticleData result)
    {
        string endpoint = ConfigurationManager.AppSettings["HSD:McpEndpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            result.FetchError = "MCP endpoint not configured (HSD:McpEndpoint).";
            return false;
        }

        string toolName = FirstNonEmpty(ConfigurationManager.AppSettings["HSD:McpToolName"], "hsdes_get_article");
        string authorization = ConfigurationManager.AppSettings["HSD:McpAuthorization"];

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;

        Hashtable arguments = new Hashtable();
        arguments["id"] = articleId;
        arguments["article_id"] = articleId;
        arguments["bug_id"] = articleId;

        Hashtable parameters = new Hashtable();
        parameters["name"] = toolName;
        parameters["arguments"] = arguments;

        Hashtable payload = new Hashtable();
        payload["jsonrpc"] = "2.0";
        payload["id"] = 1;
        payload["method"] = "tools/call";
        payload["params"] = parameters;

        try
        {
            string responseBody = PostJson(endpoint, serializer.Serialize(payload), authorization, BuildHsdProxy(), "CMF-Portal-HSD-MCP/1.0");
            string articleJson = ExtractMcpArticleJson(responseBody);
            if (string.IsNullOrWhiteSpace(articleJson))
            {
                articleJson = ExtractJsonObject(responseBody);
            }

            if (string.IsNullOrWhiteSpace(articleJson))
            {
                result.FetchError = "MCP provider did not return JSON article data.";
                return false;
            }

            MergeArticleJsonIntoResult(articleJson, result);
            return HasRecognizableArticleFields(result);
        }
        catch (WebException webEx)
        {
            result.FetchError = "MCP provider failed: " + BuildWebExceptionMessage(webEx);
            return false;
        }
    }

    private static string BuildGnaiHsdPrompt(string articleId)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Retrieve HSDES bug/article " + articleId + " using the available HSDES plugin/tool.");
        builder.AppendLine("Return only a JSON object with these keys when available:");
        builder.AppendLine("fetch_success, fetch_error, article_id, title, problem_description, description, family, component, status, sysdebug, sysdebug_forum, priority, exposure, submitter, owner, repro_steps, steps_to_reproduce, mandatory_fields, expected_behavior, actual_behavior, customer_summary, customer_impact, reproducibility, processor, soc_stepping, me_sku, cmf_justification, fix_description, fixed_version, closed_reason, implemented_date, verified_date, closed_date, investigation_history, comments.");
        builder.AppendLine("comments must be an array of concise recent discussion/update strings. investigation_history should include relevant engineering updates, debug findings, root-cause/fix/closure notes, and customer/external history when present.");
        builder.AppendLine("If the plugin/tool cannot retrieve this article, return fetch_success=false and a short fetch_error. Do not use prior knowledge or guess.");
        return builder.ToString();
    }

    private static void MergeArticleJsonIntoResult(string json, HsdArticleData result)
    {
        if (string.IsNullOrWhiteSpace(json) || result == null) return;

        ParseArticleJson(json, result);

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;
        object raw = serializer.DeserializeObject(json);
        IDictionary root = raw as IDictionary;
        if (root == null) return;

        IDictionary fields = FindArticleFieldDictionary(root);
        if (fields == null) fields = root;

        ApplyCanonicalFields(fields, result);
        ParseInlineComments(fields, result);

        object fetchSuccess = fields["fetch_success"];
        if (fetchSuccess != null && string.Equals(fetchSuccess.ToString(), "false", StringComparison.OrdinalIgnoreCase))
        {
            result.FetchError = FirstNonEmpty(Pick(fields, "fetch_error", "error", "message"), "Provider reported fetch_success=false.");
        }
    }

    private static IDictionary FindArticleFieldDictionary(IDictionary root)
    {
        if (root == null) return null;

        IDictionary article = root["article"] as IDictionary;
        if (article != null) return article;

        IDictionary bug = root["bug"] as IDictionary;
        if (bug != null) return bug;

        IDictionary fields = root["fields"] as IDictionary;
        if (fields != null) return fields;

        IDictionary dataObject = root["data"] as IDictionary;
        if (dataObject != null) return dataObject;

        IList dataArray = root["data"] as IList;
        if (dataArray != null && dataArray.Count > 0)
        {
            IDictionary firstData = dataArray[0] as IDictionary;
            if (firstData != null) return firstData;
        }

        IList articlesArray = root["articles"] as IList;
        if (articlesArray != null && articlesArray.Count > 0)
        {
            IDictionary firstArticle = articlesArray[0] as IDictionary;
            if (firstArticle != null) return firstArticle;
        }

        IList bugsArray = root["bugs"] as IList;
        if (bugsArray != null && bugsArray.Count > 0)
        {
            IDictionary firstBug = bugsArray[0] as IDictionary;
            if (firstBug != null) return firstBug;
        }

        return null;
    }

    private static void ApplyCanonicalFields(IDictionary fields, HsdArticleData result)
    {
        if (fields == null || result == null) return;

        result.ArticleId = FirstNonEmpty(Pick(fields, "article_id", "articleId", "id", "bug_id", "hsdes_id"), result.ArticleId);
        result.InvestigationHistory = FirstNonEmpty(
            Pick(fields, "investigation_history", "investigationHistory", "engineering_updates", "debug_history", "history"),
            result.InvestigationHistory);
        result.FetchError = FirstNonEmpty(Pick(fields, "fetch_error", "fetchError"), result.FetchError);
    }

    private static void ParseInlineComments(IDictionary fields, HsdArticleData result)
    {
        if (fields == null || result == null) return;
        IList comments = fields["comments"] as IList;
        if (comments == null) comments = fields["discussion_comments"] as IList;
        if (comments == null) return;

        for (int index = 0; index < comments.Count && result.Comments.Count < MaxComments; index++)
        {
            object item = comments[index];
            if (item == null) continue;

            IDictionary comment = item as IDictionary;
            string text = comment == null
                ? item.ToString()
                : FirstNonEmpty(Pick(comment, "text", "body", "description", "content", "comment"), item.ToString());

            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Comments.Add(Truncate(text.Trim(), CommentMaxChars));
            }
        }
    }

    private static string PostJson(string endpoint, string payloadJson, string authorization, IWebProxy proxy, string userAgent)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Accept = "application/json";
        request.UserAgent = userAgent;
        request.Timeout = ResolveTimeoutMs();
        request.ReadWriteTimeout = ResolveTimeoutMs();
        request.Proxy = proxy;

        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers["Authorization"] = authorization.Trim().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization.Trim()
                : "Bearer " + authorization.Trim();
        }

        using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
        {
            writer.Write(payloadJson);
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }

    private static int ResolveTimeoutMs()
    {
        int timeoutMs = 10000;
        string timeoutStr = ConfigurationManager.AppSettings["HSD:TimeoutSeconds"];
        int secs;
        if (!string.IsNullOrWhiteSpace(timeoutStr) && int.TryParse(timeoutStr, out secs) && secs > 0)
        {
            timeoutMs = secs * 1000;
        }

        return timeoutMs;
    }

    private static IWebProxy BuildGnaiProxy()
    {
        string explicitProxy = FirstNonEmpty(
            Environment.GetEnvironmentVariable("GNAI_PROXY"),
            ConfigurationManager.AppSettings["GNAI:Proxy"],
            ConfigurationManager.AppSettings["HSD:GnaiProxy"]);
        if (!string.IsNullOrWhiteSpace(explicitProxy))
        {
            WebProxy proxy = new WebProxy(explicitProxy.Trim(), true);
            proxy.Credentials = CredentialCache.DefaultCredentials;
            return proxy;
        }

        return null;
    }

    private static string ResolveGnaiToken()
    {
        return FirstNonEmpty(
            Environment.GetEnvironmentVariable("GNAI_TOKEN"),
            Environment.GetEnvironmentVariable("GNAI_API_KEY"),
            ConfigurationManager.AppSettings["GNAI:ApiKey"]);
    }

    private static string ExtractModelContent(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return string.Empty;

        try
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object raw = serializer.DeserializeObject(responseBody);
            IDictionary root = raw as IDictionary;
            if (root == null) return string.Empty;

            IList choices = root["choices"] as IList;
            if (choices != null && choices.Count > 0)
            {
                IDictionary choice = choices[0] as IDictionary;
                if (choice != null)
                {
                    IDictionary message = choice["message"] as IDictionary;
                    if (message != null && message["content"] != null)
                    {
                        return message["content"].ToString();
                    }
                }
            }

            if (root["content"] != null) return root["content"].ToString();
            if (root["text"] != null) return root["text"].ToString();
        }
        catch
        {
        }

        return string.Empty;
    }

    private static string ExtractMcpArticleJson(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return string.Empty;

        try
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            object raw = serializer.DeserializeObject(responseBody);
            IDictionary root = raw as IDictionary;
            if (root == null) return string.Empty;

            IDictionary result = root["result"] as IDictionary;
            if (result == null) return string.Empty;

            IDictionary structuredContent = result["structuredContent"] as IDictionary;
            if (structuredContent != null)
            {
                return serializer.Serialize(structuredContent);
            }

            IList content = result["content"] as IList;
            if (content != null)
            {
                for (int index = 0; index < content.Count; index++)
                {
                    IDictionary item = content[index] as IDictionary;
                    if (item != null && item["text"] != null)
                    {
                        string json = ExtractJsonObject(item["text"].ToString());
                        if (!string.IsNullOrWhiteSpace(json)) return json;
                    }
                }
            }
        }
        catch
        {
        }

        return string.Empty;
    }

    private static string ExtractJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        string trimmed = text.Trim();
        if (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            return trimmed;
        }

        int start = trimmed.IndexOf('{');
        int end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed.Substring(start, end - start + 1);
        }

        return string.Empty;
    }

    private static string BuildWebExceptionMessage(WebException webEx)
    {
        HttpWebResponse response = webEx.Response as HttpWebResponse;
        if (response == null) return webEx.Message;
        return "HTTP " + (int)response.StatusCode + AppendProviderBody(response);
    }

    private static string AppendProviderBody(HttpWebResponse response)
    {
        if (response == null) return string.Empty;

        try
        {
            using (Stream stream = response.GetResponseStream())
            {
                if (stream == null) return string.Empty;
                using (StreamReader reader = new StreamReader(stream))
                {
                    string body = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(body)) return string.Empty;
                    body = SanitizeProviderMessage(body.Trim());
                    if (body.Length > 220) body = body.Substring(0, 220) + "...";
                    return ": " + body;
                }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SanitizeProviderMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;

        if (message.IndexOf("You do not belong to any of the group", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "permission denied because the account is not in an allowed access group for this HSD/GNAI resource";
        }

        return message;
    }

    private static void TryFetchComments(string articleUrl, HsdArticleData result)
    {
        string[] commentEndpoints = new[]
        {
            articleUrl + "/children?type=discussion&count=" + MaxComments,
            articleUrl + "/comments?count=" + MaxComments
        };

        foreach (string endpoint in commentEndpoints)
        {
            try
            {
                string json = HttpGetSspi(endpoint);
                if (string.IsNullOrWhiteSpace(json)) continue;

                ParseCommentsJson(json, result);
                if (result.Comments.Count > 0) break; // stop once we got comments
            }
            catch { }
        }
    }

    private static string HttpGetSspi(string url)
    {
        int timeoutMs = 10000;

        string timeoutStr = ConfigurationManager.AppSettings["HSD:TimeoutSeconds"];
        int secs;

        if (!string.IsNullOrWhiteSpace(timeoutStr) &&
            int.TryParse(timeoutStr, out secs) &&
            secs > 0)
        {
            timeoutMs = secs * 1000;
        }

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

        request.Method = "GET";
        request.Accept = "application/json";
        request.ContentType = "application/json";
        request.Timeout = timeoutMs;
        request.ReadWriteTimeout = timeoutMs;
        request.AllowAutoRedirect = true;
        request.KeepAlive = true;

        // Use the user / process identity that is already logged into the Intel domain so
        // the challenge is negotiated as Kerberos/NTLM (SSPI) rather than using a hardcoded
        // network account that usually doesn't have HSD access.
        bool useDefaultCredentials = true;
        string useDefaultCredentialsSetting = ConfigurationManager.AppSettings["HSD:UseDefaultCredentials"];
        if (!string.IsNullOrWhiteSpace(useDefaultCredentialsSetting) &&
            !bool.TryParse(useDefaultCredentialsSetting, out useDefaultCredentials))
        {
            useDefaultCredentials = true;
        }

        request.UseDefaultCredentials = useDefaultCredentials;
        request.Credentials = useDefaultCredentials
            ? CredentialCache.DefaultCredentials
            : CredentialCache.DefaultNetworkCredentials;
        request.PreAuthenticate = true;

        // HSD is usually accessible from the corporate network. If a proxy is required,
        // preserve the same Windows identity so the proxy and the upstream HSD endpoint
        // can negotiate authentication correctly.
        request.Proxy = BuildHsdProxy();

        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        System.Diagnostics.Debug.WriteLine(
            "HSD SSPI Windows Identity: " +
            (identity != null ? identity.Name : "NULL")
        );

        System.Diagnostics.Debug.WriteLine(
            "HSD SSPI Authentication Type: " +
            (identity != null ? identity.AuthenticationType : "NULL")
        );

        using (HttpWebResponse response =
            (HttpWebResponse)request.GetResponse())
        using (StreamReader reader =
            new StreamReader(response.GetResponseStream(), Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }

    private static IWebProxy BuildHsdProxy()
    {
        string explicitProxy = ConfigurationManager.AppSettings["HSD:Proxy"];
        if (!string.IsNullOrWhiteSpace(explicitProxy))
        {
            WebProxy proxy = new WebProxy(explicitProxy.Trim(), true);
            proxy.Credentials = CredentialCache.DefaultCredentials;
            return proxy;
        }

        IWebProxy systemProxy = WebRequest.GetSystemWebProxy();
        if (systemProxy != null)
        {
            systemProxy.Credentials = CredentialCache.DefaultCredentials;
            return systemProxy;
        }

        return null;
    }

    private static void ParseArticleJson(string json, HsdArticleData result)
    {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        ser.MaxJsonLength = int.MaxValue;

        object raw = ser.DeserializeObject(json);

        IDictionary root = raw as IDictionary;

        if (root == null)
        {
            result.FetchError = "HSD response root is not a JSON object.";
            return;
        }

        IDictionary fields = FindArticleFieldDictionary(root);
        if (fields == null) fields = root;

        if (fields == null)
        {
            result.FetchError = "Unable to locate HSD article fields.";
            return;
        }

        MapArticleFields(fields, result);
    }

    private static void MapArticleFields(IDictionary fields, HsdArticleData result)
    {
        result.Title = FirstNonEmpty(Pick(fields, "title", "subject", "name"), result.Title);
        result.Description = FirstNonEmpty(Pick(fields, "problem_description", "problemDescription", "description", "sighting.description", "body", "text", "summary", "client_platf.bug.problem_description", "client_platf.bug.root_cause_description", "client_platf.bug.what_happened"), result.Description);
        result.Family = FirstNonEmpty(Pick(fields, "family", "product_family", "sighting.family", "cpu_family"), result.Family);
        result.Component = FirstNonEmpty(Pick(fields, "component", "sighting.component", "component_name"), result.Component);
        result.Status = FirstNonEmpty(Pick(fields, "status", "sighting.status", "state"), result.Status);
        result.Sysdebug = FirstNonEmpty(Pick(fields, "sysdebug", "sighting.sysdebug", "debug_category", "sysdebug_category", "client_platf.bug.sysdebug"), result.Sysdebug);
        result.SysdebugForum = FirstNonEmpty(Pick(fields, "sysdebug_forum", "sighting.sysdebug_forum", "debug_forum", "client_platf.bug.sysdebug_forum"), result.SysdebugForum);
        result.Priority = FirstNonEmpty(Pick(fields, "priority", "sighting.priority", "severity"), result.Priority);
        result.Submitter = FirstNonEmpty(Pick(fields, "submitter", "reporter", "created_by", "author"), result.Submitter);
        result.Owner = FirstNonEmpty(Pick(fields, "owner", "assigned_to", "assignee"), result.Owner);
        result.StepsToReproduce = FirstNonEmpty(Pick(fields, "repro_steps", "reproSteps", "steps_to_reproduce", "stepsToReproduce", "sighting.steps_to_reproduce", "sighting.repro_steps", "repro", "to_reproduce", "bug.to_reproduce", "client_platf.bug.repro_steps", "client_platf.bug.steps_to_reproduce", "client_platf.bug.ext_cust_to_reproduce"), result.StepsToReproduce);
        result.ExpectedBehavior = FirstNonEmpty(Pick(fields, "expected_behavior", "expected", "sighting.expected_behavior", "client_platf.bug.what_should_have_happened"), result.ExpectedBehavior);
        result.ActualBehavior = FirstNonEmpty(Pick(fields, "actual_behavior", "actual", "sighting.actual_behavior", "client_platf.bug.what_happened"), result.ActualBehavior);
        result.WhatHappened = FirstNonEmpty(Pick(fields, "what_happened", "client_platf.bug.what_happened"), result.WhatHappened);
        result.WhatShouldHaveHappened = FirstNonEmpty(Pick(fields, "what_should_have_happened", "client_platf.bug.what_should_have_happened"), result.WhatShouldHaveHappened);
        result.DebugLogAttached = FirstNonEmpty(Pick(fields, "debug_log_attached", "client_platf.bug.debug_log_attached", "is_system_scope_log_attached", "client_platf.bug.is_system_scope_log_attached"), result.DebugLogAttached);
        result.SystemScopeFilename = FirstNonEmpty(Pick(fields, "system_scope_filename", "client_platf.bug.system_scope_filename"), result.SystemScopeFilename);
        result.CustomerDetail = FirstNonEmpty(Pick(fields, "customer_detail", "client_platf.bug.customer_detail"), result.CustomerDetail);
        result.CustomerSummary = FirstNonEmpty(Pick(fields, "customer_summary", "customerSummary", "client_platf.bug.customer_summary", "client_platf.bug.ext_cust_summary", "ext_cust_summary"), result.CustomerSummary);
        result.MandatoryFields = FirstNonEmpty(
            Pick(fields,
                "mandatory_fields",
                "mandatoryFields",
                "required_fields",
                "requiredFields",
                "fields_marked_mandatory",
                "starred_fields",
                "client_platf.bug.mandatory_fields",
                "client_platf.bug.required_fields"),
            result.MandatoryFields,
            BuildMandatoryFieldsSummary(fields));
        result.Board = FirstNonEmpty(Pick(fields, "board", "client_platf.bug.board", "customer_board", "client_platf.bug.customer_board"), result.Board);
        result.Processor = FirstNonEmpty(Pick(fields, "processor", "sighting.processor", "cpu", "cpu_model", "processor_model", "client_platf.bug.processor", "client_platf.bug.cpu", "client_platf.bug.cpu_model"), result.Processor);
        result.SocStepping = FirstNonEmpty(Pick(fields, "soc_stepping", "socstepping", "soc.stepping", "stepping", "silicon_stepping", "client_platf.bug.soc_stepping", "client_platf.bug.stepping", "client_platf.bug.silicon_stepping"), result.SocStepping);
        result.Exposure = FirstNonEmpty(Pick(fields, "exposure", "customer_exposure", "business_exposure", "client_platf.bug.exposure", "client_platf.bug.customer_exposure"), result.Exposure);
        result.MeSku = FirstNonEmpty(Pick(fields, "me_sku", "meSKU", "me.sku", "me_sku_type", "me_type", "client_platf.bug.me_sku", "client_platf.bug.me_sku_type", "client_platf.bug.me_type"), result.MeSku);
        result.FixDescription = FirstNonEmpty(Pick(
            fields,
            "fix_description",
            "bugfixdescription",
            "bug.fix_description",
            "client_platf.bug.corrective_action_description",
            "fixdescription",
            "resolution"), result.FixDescription);

        result.ClosedReason = FirstNonEmpty(Pick(
            fields,
            "closed_reason",
            "bugclosedreason",
            "bug.closed_reason",
            "closedreason",
            "closure_reason"), result.ClosedReason);

        result.FixedVersion = FirstNonEmpty(Pick(
            fields,
            "fixed_version",
            "clientplatfbugfixedinversion",
            "client_platf.bug.fixed_in_version",
            "bug.fixed_in",
            "fixedinversion",
            "fixed_in_version"), result.FixedVersion);

        result.CustomerImpact = FirstNonEmpty(Pick(
            fields,
            "customer_impact",
            "bug.customer_impact",
            "client_platf.bug.impact",
            "client_platf.bug.ext_cust_impact",
            "clientplatfbugimpact",
            "bugcustomerimpact",
            "customerimpact",
            "impact"), result.CustomerImpact);

        result.Reproducibility = FirstNonEmpty(Pick(
            fields,
            "reproducibility",
            "bug.reproducibility",
            "client_platf.bug.can_reproduce",
            "client_platf.bug.score_to_reproduce",
            "bugreproducibility",
            "repro"), result.Reproducibility);

        result.ImplementedDate = FirstNonEmpty(Pick(
            fields,
            "implemented_date",
            "bug.implemented_date",
            "client_platf.bug.date_implemented",
            "bugimplementeddate",
            "dateimplemented"), result.ImplementedDate);

        result.VerifiedDate = FirstNonEmpty(Pick(
            fields,
            "verified_date",
            "bug.verified_date",
            "bugverifieddate",
            "dateverified"), result.VerifiedDate);

        result.ClosedDate = FirstNonEmpty(Pick(
            fields,
            "closed_date",
            "closed_date",
            "closeddate",
            "dateclosed"), result.ClosedDate);

        result.CmfJustification = FirstNonEmpty(Pick(
            fields,
            "cmf_justification",
            "client_platf.bug.why_cmf_change",
            "clientplatfbugwhycmfchange",
            "whycmfchange"), result.CmfJustification);

        string commentsHistory = Pick(
            fields,
            "investigation_history",
            "investigationHistory",
            "engineering_updates",
            "debug_history",
            "client_platf.bug.root_cause_description",
            "client_platf.bug.score_comments",
            "client_platf.bug.ext_agent_comments",
            "comments",
            "discussion_comments",
            "discussioncomments");

        if (!string.IsNullOrWhiteSpace(commentsHistory))
        {
            result.InvestigationHistory = commentsHistory;
        }

        string customerBlogHistory = Pick(
            fields,
            "clientplatfbugextcustbloghist",
            "custbloghist",
            "customerbloghistory");

        if (!string.IsNullOrWhiteSpace(customerBlogHistory))
        {
            if (!string.IsNullOrWhiteSpace(result.InvestigationHistory))
            {
                result.InvestigationHistory += "\n\n=== CUSTOMER / EXTERNAL HISTORY ===\n"
                    + customerBlogHistory;
            }
            else
            {
                result.InvestigationHistory = customerBlogHistory;
            }
        }
    }

    private static void ParseCommentsJson(string json, HsdArticleData result)
    {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        ser.MaxJsonLength = int.MaxValue;

        object raw = ser.DeserializeObject(json);
        IDictionary root = raw as IDictionary;
        if (root == null) return;

        IList items = root["data"] as IList ?? root["comments"] as IList ?? root["children"] as IList;
        if (items == null) return;

        foreach (object item in items)
        {
            IDictionary c = item as IDictionary;
            if (c == null) continue;

            string text = Pick(c, "description", "body", "text", "content", "comment");
            if (string.IsNullOrWhiteSpace(text)) continue;

            string author = Pick(c, "submitter", "author", "created_by");
            string entry = string.IsNullOrWhiteSpace(author)
                ? text
                : "[" + author + "]: " + text;

            result.Comments.Add(Truncate(entry, CommentMaxChars));
            if (result.Comments.Count >= MaxComments) break;
        }
    }

    // Returns the first non-empty value matching any of the given keys.
    private static string Pick(IDictionary dict, params string[] keys)
    {
        if (dict == null) return null;
        foreach (string key in keys)
        {
            if (!dict.Contains(key)) continue;
            object val = dict[key];
            if (val == null) continue;
            string s = val.ToString().Trim();
            if (!string.IsNullOrEmpty(s) && !string.Equals(s, "null", StringComparison.OrdinalIgnoreCase))
                return s;
        }
        return null;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        if (values == null) return string.Empty;
        for (int index = 0; index < values.Length; index++)
        {
            if (!string.IsNullOrWhiteSpace(values[index]))
            {
                return values[index];
            }
        }

        return string.Empty;
    }

    private static string BuildMandatoryFieldsSummary(IDictionary fields)
    {
        if (fields == null) return string.Empty;

        StringBuilder builder = new StringBuilder();
        AppendPickedMandatoryField(builder, fields, "Family", "family", "product_family", "sighting.family", "cpu_family");
        AppendPickedMandatoryField(builder, fields, "Component", "component", "sighting.component", "component_name");
        AppendPickedMandatoryField(builder, fields, "Priority", "priority", "sighting.priority", "severity");
        AppendPickedMandatoryField(builder, fields, "Status", "status", "sighting.status", "state");
        AppendPickedMandatoryField(builder, fields, "Owner", "owner", "assigned_to", "assignee");
        AppendPickedMandatoryField(builder, fields, "Sysdebug", "sysdebug", "sighting.sysdebug", "debug_category", "sysdebug_category", "client_platf.bug.sysdebug");
        AppendPickedMandatoryField(builder, fields, "Reproducibility", "reproducibility", "bug.reproducibility", "client_platf.bug.can_reproduce", "client_platf.bug.score_to_reproduce", "bugreproducibility", "repro");
        AppendPickedMandatoryField(builder, fields, "Customer Impact", "customer_impact", "bug.customer_impact", "client_platf.bug.impact", "client_platf.bug.ext_cust_impact", "clientplatfbugimpact", "bugcustomerimpact", "customerimpact", "impact");

        return builder.ToString().Trim().TrimEnd(';');
    }

    private static void AppendPickedMandatoryField(StringBuilder builder, IDictionary fields, string label, params string[] keys)
    {
        string value = Pick(fields, keys);
        if (string.IsNullOrWhiteSpace(value)) return;
        if (builder.Length > 0) builder.Append("; ");
        builder.Append(label).Append(": ").Append(Truncate(value.Trim(), 220));
    }

    private static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLen) return value;
        return value.Substring(0, maxLen) + "... [truncated]";
    }

    private static void AppendIfSet(StringBuilder sb, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            sb.AppendLine(label + ": " + value);
    }
}
