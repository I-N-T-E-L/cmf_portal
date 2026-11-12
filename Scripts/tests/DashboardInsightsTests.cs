using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

public static class DashboardInsightsTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        CheckDashboardAiConfiguration();
        CheckDashboardRateLimitRecovery();
        CheckRenderedFactSelection();
        CheckRenderedPanelProjection();
        CheckRenderedBriefing();
        DateTime today = new DateTime(2026, 11, 3);
        DataTable data = new DataTable();
        foreach (string column in new[] { "cp_id", "title", "source_platform", "component", "component_group", "promoted_id", "merge_id", "cmf_request", "status" })
            data.Columns.Add(column, typeof(string));
        foreach (string column in new[] { "ask_date", "decided_date", "implemented_date", "closed_date" }) data.Columns.Add(column, typeof(DateTime));
        data.Columns.Add("comparable_status", typeof(int));
        data.Rows.Add("16011111111", "Screen hang on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        data.Rows.Add("16022222222", "Screen flicker on external display", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-5), today.AddDays(-2), DBNull.Value, DBNull.Value, 1);
        data.Rows.Add("16022222222", "Screen flicker on external display", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_duplicate", "open", today.AddDays(-5), today.AddDays(-2), DBNull.Value, DBNull.Value, 1);
        data.Rows.Add("16033333333", "Closed case", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "", "", "cmf_ok", "complete", today.AddDays(-30), today.AddDays(-20), DBNull.Value, today.AddDays(-1), 1);
        data.Rows.Add("16044444444", "Bad history", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "", "", "cmf_ok", "implemented", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 0);
        data.Rows[0]["promoted_id"] = "16099999999,16099999999";
        data.Columns.Add("description", typeof(string));
        data.Rows[0]["description"] = new string('x', 24000) + " full description tail";
        var result = CmfDashboardInsightsService.Build(data, "Overall", today);
        var article = new HsdArticleData { Comments = new List<string>() };
        typeof(HsdPortalService).GetMethod("MergeArticleJsonIntoResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { "{\"title\":\"A title\",\"summary\":\"Technical summary\",\"sysdebug_forum\":\"Forum evidence\",\"steps_to_reproduce\":\"Reproduction evidence\",\"comments\":[\"one\",\"two\",\"three\",\"four\",\"five\",\"six\"]}", article, false, true });
        Check(article.InsightFields["comments[5]"] == "six" && article.InsightFields["sysdebug_forum"] == "Forum evidence",
            "Insights source path dropped comments or forum evidence.");
        int requests = 0;
        HsdPortalService.ReadInsightCommentPages("https://example.test/article/comments", url => {
            requests++;
            return requests == 1 ? "{\"data\":[{\"text\":\"First full comment\"}],\"next\":\"?page=2\"}"
                : "{\"data\":[{\"text\":\"Final full comment\"}]}";
        }, article);
        Check(requests == 2 && article.InsightFields.Values.Contains("Final full comment"), "Insights did not follow all discussion pages.");
        bool unsafePaginationRejected = false;
        try { HsdPortalService.ReadInsightCommentPages("https://example.test/article/comments", url => "{\"data\":[],\"next\":\"https://other.test/comments\"}", article); }
        catch (InvalidOperationException) { unsafePaginationRejected = true; }
        Check(unsafePaginationRejected, "Discussion pagination could send authenticated retrieval to another origin.");
        Check(result.Documents.Count == 4 && result.Documents.Sum(document => document.Occurrences) == 5,
            "AI input does not cover every scoped issue and duplicate occurrence.");
        Check(result.Documents.Single(document => document.Id == "16011111111").Fields["Local.description"].EndsWith("full description tail"),
            "Detailed AI input was truncated.");
        Check(result.Documents.Any(document => document.Buckets.Contains("Closed"))
            && result.Documents.Any(document => document.Buckets.Contains("Implemented"))
            && result.Documents.Any(document => document.Buckets.Contains("Open")), "AI input lost a Summary status bucket.");
        var ingredient = result.Trends.Single(trend => trend.Dimension == "Ingredient");
        Check(ingredient.Open == 3 && ingredient.PreviousOpen == 2 && ingredient.Change == 1, "Open balances or duplicate row semantics changed.");
        Check(ingredient.Excluded == 1 && ingredient.OldestDays == 20, "Exclusions or acceptance aging changed.");
        Check(result.Records == 5 && result.DistinctIssues == 4 && result.DuplicateRecords == 1, "Occurrence and distinct issue counts were conflated.");
        Check(result.Groups.Count == 0 && result.Highlights.Count == 0, "Merge IDs or shared components fabricated non-AI insights.");
        Check(result.CanCompare && result.DateRange == "All acceptance dates", "Insights lost its date-filter-independent scope.");
        DataTable reversed = data.Clone();
        foreach (DataRow row in data.AsEnumerable().Reverse()) reversed.ImportRow(row);
        Check(CmfDashboardInsightsService.Build(reversed, "Overall", today).SourceHash == result.SourceHash, "Source fingerprint depended on SQL row order.");
        DataTable platform = data.AsEnumerable().Where(row => (string)row["source_platform"] == "CMF_PTL_ALL_COMPONENTS_TABLE").CopyToDataTable();
        var scoped = CmfDashboardInsightsService.Build(platform, "PTL", today);
        Check(scoped.Groups.Count == 0 && scoped.Trends.Where(trend => trend.Dimension == "Platform").Count() == 1, "Platform scope leaked cross-platform relationships.");
        for (int index = 0; index < 24; index++)
            data.Rows.Add((16060000000L + index).ToString(), "Repeated screen hang", "CMF_PTL_ALL_COMPONENTS_TABLE", "Ingredient " + index, "Group " + index, "", "", "cmf_ok", "open", today.AddDays(-40), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        var expanded = CmfDashboardInsightsService.Build(data, "Overall", today);
        var expandedData = data.Copy();
        Check(expanded.Documents.Count == 28 && expanded.TrendCount > 30, "A total issue or trend analysis cap remains.");
        DataTable futureData = data.Clone();
        futureData.Rows.Add("16011111111", "Screen hang on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        futureData.Rows.Add("16022222222", "Screen flicker on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_duplicate", "future", today.AddDays(-30), today.AddDays(-20), today.AddDays(-2), DBNull.Value, 1);
        futureData.Rows.Add("16033333333", "Screen hang with future fix", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "future", today.AddDays(-30), today.AddDays(-20), today.AddDays(-10), DBNull.Value, 1);
        var futureResult = CmfDashboardInsightsService.Build(futureData, "Overall", today);
        var futureIngredient = futureResult.Trends.Single(trend => trend.Dimension == "Ingredient");
        Check(futureIngredient.Open == 1 && futureIngredient.PreviousOpen == 2, "Future statuses were counted as currently Open or lost their historical implementation boundary.");
        Check(futureResult.Documents.Count(document => document.Buckets.Contains("Implemented")) == 2
            && futureResult.Records == 3 && futureResult.DuplicateRecords == 1, "Future classification changed AI scope, record totals or duplicate scope.");
        data.Clear();
        Check(CmfDashboardInsightsService.Build(data, "Empty", today).Highlights.Count == 0, "Empty scope fabricated findings.");
        CheckAiPipeline(data.Clone(), today);
        CheckAiPipeline(expandedData, today);
        CheckAiPipeline(futureData, today);
        CheckStructuredCandidates(today);
        CheckHybridPipeline(expandedData, today);
        Console.WriteLine("Dashboard Insights hybrid grouping, on-demand evidence, rollup and pagination tests passed.");
    }

    private static void CheckDashboardAiConfiguration()
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        var parse = typeof(AiSummaryService).GetMethod("ParseDashboardGnaiModel", flags);
        var scope = typeof(AiSummaryService).GetMethod("IsDashboardAiRequest", flags);
        string json = System.IO.File.ReadAllText(System.IO.Path.Combine(System.Environment.CurrentDirectory, "App_Data", "dashboard-ai-config.json"));
        Check(!string.IsNullOrWhiteSpace((string)parse.Invoke(null, new object[] { json })), "Saved dashboard model is invalid.");
        Check((string)parse.Invoke(null, new object[] { "{\"gnaiModel\":\"claude-sonnet-5-5\"}" }) == "claude-sonnet-5-5", "Verified GNAI model ID did not parse correctly.");
        Check((string)parse.Invoke(null, new object[] { "{\"gnaiModel\":\"  model-for-test  \"}" }) == "model-for-test", "Dashboard model whitespace was not normalized.");
        foreach (string invalid in new[] { "{}", "null", "{\"gnaiModel\":123}", "{\"gnaiModel\":\" \"}", "{",
            "{\"gnaiModel\":\"gpt-5-mini\",\"gnaiEndpoints\":null}",
            "{\"gnaiModel\":\"gpt-5-mini\",\"gnaiEndpoints\":{\"openai\":\"http://example.test/chat\"}}" })
        {
            bool rejected = false;
            try { parse.Invoke(null, new object[] { invalid }); }
            catch (System.Reflection.TargetInvocationException ex) { rejected = ex.InnerException is ArgumentException || ex.InnerException is InvalidOperationException; }
            Check(rejected, "Invalid dashboard model configuration was silently accepted.");
        }
        foreach (string id in new[] { "dashboard", "dashboard-insights", "dashboard-blockers" })
            Check((bool)scope.Invoke(null, new object[] { id }), "Dashboard model override missed a dashboard AI request.");
        foreach (string id in new[] { "16011111111", "16011111111-followup", "issue-details", "" })
            Check(!(bool)scope.Invoke(null, new object[] { id }), "Dashboard model override leaked into an issue assistant.");
        string shared = (string)typeof(AiSummaryService).GetMethod("GetAiProviderCacheSignature", flags).Invoke(null, null);
        Check(AiSummaryService.DashboardInsightsProviderSignature().StartsWith(shared + "|dashboard-model:", StringComparison.Ordinal),
            "Dashboard caches do not distinguish the configured model from shared assistant results.");
        var endpoint = typeof(AiSummaryService).GetMethod("ResolveDashboardGnaiEndpoint", flags);
        const string openai = "https://gnai.intel.com/api/providers/openai/v1/chat/completions";
        const string anthropic = "https://gnai.intel.com/api/providers/anthropic/v1/messages";
        Check((string)endpoint.Invoke(null, new object[] { openai + "?api-version=test", "claude-sonnet-5-5" }) == anthropic,
            "Claude dashboard selection did not use the catalog's Anthropic route.");
        Check((string)endpoint.Invoke(null, new object[] { "https://gnai.intel.com/api/chat", "claude-sonnet-5-5" }) == anthropic,
            "Claude routing did not retain the configured GNAI base path.");
        Check((string)endpoint.Invoke(null, new object[] { openai, "gpt-5-mini" }) == openai,
            "Claude routing changed a non-Claude model's endpoint.");
        Check((string)endpoint.Invoke(null, new object[] { anthropic, "gpt-5-mini" }) == openai,
            "Changing from Claude to GPT did not select the OpenAI-compatible endpoint.");
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var payload = typeof(AiSummaryService).GetMethod("CreateModelPayload", flags);
        var claude = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(payload.Invoke(null,
            new object[] { "claude-sonnet-5-5", "question", "system instructions", true })));
        Check((string)claude["model"] == "claude-sonnet-5-5" && (string)claude["system"] == "system instructions"
            && Convert.ToInt32(claude["max_tokens"]) > 0 && ((System.Collections.IEnumerable)claude["messages"]).Cast<object>().Count() == 1,
            "Dashboard Claude request did not use the Anthropic Messages format.");
        var compatible = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(payload.Invoke(null,
            new object[] { "gpt-5-mini", "question", "system instructions", false })));
        Check(!compatible.ContainsKey("system") && !compatible.ContainsKey("max_tokens")
            && ((System.Collections.IEnumerable)compatible["messages"]).Cast<object>().Count() == 2,
            "The existing OpenAI-compatible request format changed.");
        var extract = typeof(AiSummaryService).GetMethod("ExtractContentFromResponse", flags);
        Check((string)extract.Invoke(null, new[] { serializer.DeserializeObject("{\"content\":[{\"type\":\"thinking\",\"thinking\":\"hidden\"},{\"type\":\"text\",\"text\":\"verified \"},{\"type\":\"text\",\"text\":\"response\"}]}") }) == "verified response",
            "Claude response text was not extracted or hidden thinking leaked into the dashboard.");
        Check((string)extract.Invoke(null, new[] { serializer.DeserializeObject("{\"choices\":[{\"message\":{\"content\":\"existing response\"}}]}") }) == "existing response",
            "OpenAI-compatible response parsing regressed.");
        var read = typeof(AiSummaryService).GetMethod("ReadDashboardAiConfiguration", flags);
        var signature = typeof(AiSummaryService).GetMethod("BuildDashboardAiProviderSignature", flags);
        string fixture = System.IO.Path.Combine(System.Environment.CurrentDirectory, "App_Data", "dashboard-ai-test-" + Guid.NewGuid().ToString("N") + ".json");
        string firstSignature = null;
        try
        {
            foreach (string model in new[] { "claude-sonnet-5-5", "gpt-5-mini", "claude-4-5-sonnet" })
            {
                System.IO.File.WriteAllText(fixture, serializer.Serialize(new { gnaiModel = model,
                    gnaiEndpoints = new { openai = openai, anthropic = anthropic } }));
                object configuration = read.Invoke(null, new object[] { fixture });
                Type type = configuration.GetType();
                bool native = (bool)type.GetProperty("UseAnthropic").GetValue(configuration, null);
                string selected = (string)type.GetProperty("Model").GetValue(configuration, null);
                string selectedEndpoint = (string)type.GetProperty("Endpoint").GetValue(configuration, null);
                Check(selected == model && selectedEndpoint == (native ? anthropic : openai)
                    && (bool)type.GetProperty("HasOverride").GetValue(configuration, null),
                    "Changing only the file's model did not select the matching GNAI request configuration.");
                var outgoing = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(payload.Invoke(null,
                    new object[] { selected, "test", "system", native })));
                Check((string)outgoing["model"] == model && outgoing.ContainsKey("system") == native,
                    "The reloaded model did not reach the outgoing provider payload.");
                string currentSignature = (string)signature.Invoke(null, new[] { configuration });
                Check(firstSignature == null || firstSignature != currentSignature, "Model changes reused the previous model's cache signature.");
                firstSignature = currentSignature;
            }
            const string custom = "https://gnai.example.test/api/providers/anthropic/v1/messages";
            System.IO.File.WriteAllText(fixture, serializer.Serialize(new { gnaiModel = "claude-4-5-sonnet",
                gnaiEndpoints = new { openai = openai, anthropic = custom } }));
            object changed = read.Invoke(null, new object[] { fixture });
            Check((string)changed.GetType().GetProperty("Endpoint").GetValue(changed, null) == custom
                && firstSignature != (string)signature.Invoke(null, new[] { changed }), "Endpoint changes did not invalidate AI caches.");
        }
        finally { if (System.IO.File.Exists(fixture)) System.IO.File.Delete(fixture); }
        var guard = typeof(CmfDashboardInsightsService).GetMethod("EnsureDashboardAiConfiguration", flags);
        string active = AiSummaryService.DashboardInsightsProviderSignature();
        guard.Invoke(null, new object[] { active });
        bool staleRejected = false;
        try { guard.Invoke(null, new object[] { active + "-changed" }); }
        catch (System.Reflection.TargetInvocationException ex) { staleRejected = ex.InnerException is OperationCanceledException; }
        Check(staleRejected, "An analysis could continue publishing after its configured model or endpoint changed.");
    }

    private static void CheckRenderedPanelProjection()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var data = serializer.Deserialize<Dictionary<string, object>>("{\"PlatformLabel\":\"NVL-H\",\"NewToday\":987654,\"Description\":\"HSD text must never be supplied\",\"SummaryFacts\":[{\"Label\":\"Open\",\"Value\":\"20\",\"Note\":\"8 duplicates\",\"WeeklyTrend\":{\"PreviousValue\":18,\"Display\":\"2\",\"Direction\":\"up\"}}],\"ComponentSummary\":{\"Columns\":[\"Ingredient\",\"QS:::Open\"],\"Rows\":[[\"HW\",\"11(2)+6(0)\"]]},\"PendingSummary\":{\"Columns\":[\"Ingredient\",\"CMF Pending\"],\"Rows\":[[\"memory\",\"2:::OPEN_HSDS:::14000000000\"]]},\"Trend\":[{\"WeekLabel\":\"WW45\",\"NewIssues\":3,\"ResolvedIssues\":2,\"WeekEndOpenIssues\":18,\"WeekEndImplementedIssues\":4},{\"WeekLabel\":\"WW46\",\"NewIssues\":5,\"ResolvedIssues\":1,\"WeekEndOpenIssues\":20,\"WeekEndImplementedIssues\":6}]} ");
        var snapshot = new CmfPanelSnapshotCache { Platform = "CMF_NVL_H_ALL_COMPONENTS_TABLE", DateScope = "all", Configuration = "scope-a",
            CapturedAt = new DateTime(2026, 11, 9, 12, 0, 0), Data = data, ShowComparisons = true };
        var source = CmfRenderedInsightsService.BuildSource(snapshot, 1, "line");
        string prompt = CmfRenderedInsightsService.RankingPrompt(source);
        Check(!prompt.Contains("987654") && !prompt.Contains("HSD text must never") && !prompt.Contains("14000000000"), "Hidden fields or undisplayed issue IDs leaked into the dashboard-only LLM input.");
        Check(source.Facts.Any(fact => fact.Title == "Open backlog is higher than a week earlier" && fact.Evidence.Contains("20") && fact.Evidence.Contains("18")), "Metric changes were not combined into a supported conclusion.");
        var measurement = source.Facts.Single(fact => fact.Topic == "Open").Measurements.Single();
        Check(measurement.Current == 20 && measurement.Previous == 18 && measurement.Unit == "CMF records"
            && measurement.CurrentDate == "2026-11-09" && measurement.PreviousDate == "2026-11-02" && measurement.PreviousIsEstimate,
            "Summary evidence lost numeric values, units, dates or reconstructed-history provenance.");
        Check(source.Facts.Single(fact => fact.Topic == "component-concentration").Measurements.Single().Denominator == 17,
            "Summary population evidence lost its actual table denominator.");
        Check(!source.Facts.Any(fact => fact.Title == "Open today"), "A repeated current KPI was emitted as an insight.");
        Check(!source.Facts.Any(fact => fact.Source == "Weekly chart"), "A chart week outside the selected display range was used.");
        Check(source.Facts.Single(fact => fact.Id == "component-concentration").Evidence.Contains("QS Open: 11(2)+6(0)"), "Component table cells were altered or recounted.");
        data["TptFacts"] = serializer.DeserializeObject("[{\"Label\":\"Resolution TPT\",\"Value\":\"19\",\"Note\":\"days\"}]");
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "line").Facts.Any(fact => fact.Title == "Resolution TPT today"),
            "An unchanged or non-comparable duration was emitted as a conclusion.");
        var result = CmfRenderedInsightsService.BuildResult(source, source.Facts, true);
        var view = CmfDashboardInsightsService.CreateView(result, "token");
        Check(view.Pipeline == CmfRenderedInsightsService.Pipeline && view.PageSize == 5 && view.Highlights.Count <= 5 && view.TrendFindings.Count > 0, "Rendered Insights views lost their factual trends or five-item briefing size.");
        var cumulative = CmfRenderedInsightsService.Aggregate("Overall", new[] { result, result });
        Check(cumulative.HighlightCount == result.HighlightCount && cumulative.TrendCount == result.TrendCount,
            "Overall duplicated the same platform's findings instead of regrouping them.");
        Check(cumulative.Highlights.First().Title.StartsWith("NVL-H:"), "Overall findings lost their platform source.");
        var unranked = CmfRenderedInsightsService.BuildResult(source, source.Facts, false);
        Check(CmfDashboardInsightsService.CreateView(unranked, "token").Highlights.Count > 0 && !unranked.Highlights[0].AiReviewed,
            "Unranked factual values were hidden or mislabeled AI-reviewed.");
        snapshot.ShowComparisons = false;
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "bar").Facts.Any(fact => fact.Source == "CMF Summary comparison"),
            "Hidden KPI comparison values leaked into a short date-filter scope.");
        snapshot.DateScope = "today";
        data["Trend"] = serializer.DeserializeObject("[{\"WeekLabel\":\"WW45\",\"NewIssues\":0,\"ResolvedIssues\":0},{\"WeekLabel\":\"WW46\",\"NewIssues\":5,\"ResolvedIssues\":1}]");
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "bar").Facts.Any(fact => fact.Source == "Weekly chart" && fact.Evidence.Contains("WW45")),
            "Trimmed empty chart weeks leaked into the filtered panel input.");
    }

    private static void CheckRenderedBriefing()
    {
        CheckRenderedOverallRanking();
        CheckRenderedConcentrationTies();
        CheckRenderedTptGrouping();
        CheckRenderedMixedSignalMateriality();
        CheckRenderedQueueChangeMateriality();
        CheckRenderedPendingAgeThreshold();
        CheckRenderedSummaryAnalysis();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var data = serializer.Deserialize<Dictionary<string, object>>(@"{
            ""PlatformLabel"":""Platform A"",
            ""PendingSummaryAgeThresholdDays"":2,
            ""SummaryFacts"":[
                {""Label"":""Open"",""Value"":""30"",""WeeklyTrend"":{""PreviousValue"":20}},
                {""Label"":""Closed"",""Value"":""12"",""WeeklyTrend"":{""PreviousValue"":10}},
                {""Label"":""Pending"",""Value"":""4"",""WeeklyTrend"":{""PreviousValue"":2}},
                {""Label"":""Total CMFs"",""Value"":""100"",""WeeklyTrend"":{""PreviousValue"":90}}],
            ""TptFacts"":[{""Label"":""Resolution TPT"",""Value"":""4"",""WeeklyTrend"":{""PreviousValue"":4}}],
            ""ComponentSummary"":{""Columns"":[""Ingredient"",""QS:::Open"",""QS:::Impl/Verified""],
                ""Rows"":[[""HW"",""11(2)+6(0)"",""100+20""],[""SW"",""3(1)+0(0)"",""0+0""],[""Total"",""14(3)+6(0)"",""100+20""]]},
            ""PendingSummary"":{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],
                ""Rows"":[[""HW"",""Unassigned"",""3:::OVER_TWO_DAYS:::2:::OPEN_HSDS:::14000000000""],[""HW"",""Named contact"",""1:::OVER_TWO_DAYS:::0""]]},
            ""MilestoneSummary"":{""Columns"":[""Milestone"",""Unique CMF Count""],""Rows"":[[""QS"",""6""],[""PV"",""4""]]},
            ""Trend"":[{""WeekLabel"":""WW44"",""WeekEndOpenIssues"":10},{""WeekLabel"":""WW45"",""WeekEndOpenIssues"":20},{""WeekLabel"":""WW46"",""WeekEndOpenIssues"":30}]
        }");
        var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
            CapturedAt = new DateTime(2026, 11, 9), Data = data, ShowComparisons = true };
        var source = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        var mixed = source.Facts.Single(fact => fact.Topic == "closures-backlog");
        Check(mixed.Category == CmfRenderedInsightsService.NeedsAttention && mixed.Label == "Mixed Signals"
            && mixed.Evidence.Contains("+10") && mixed.Evidence.Contains("+2"), "Mixed signals did not remain a labeled attention finding with both supporting comparisons.");
        Check(mixed.Summary == "Closed totals increased, while open backlog was higher than a week earlier.",
            "The mixed-signal summary lost its neutral comparison wording.");
        Check(source.Facts.All(fact => fact.View == "highlights"
            ? fact.Category == CmfRenderedInsightsService.NeedsAttention || fact.Category == CmfRenderedInsightsService.Progress
            : fact.Category == CmfRenderedInsightsService.DirectionOverTime || fact.Category == CmfRenderedInsightsService.Concentration),
            "The briefing introduced a category outside the small, view-specific vocabulary.");
        Check(source.Facts.Where(fact => fact.Topic == "pending-idst-coverage" || fact.Topic == "pending-age-review")
            .All(fact => fact.View == "highlights" && fact.Category == CmfRenderedInsightsService.NeedsAttention),
            "Current pending bottlenecks were presented as historical trends.");
        Check(source.Facts.Where(fact => fact.Topic.EndsWith("-concentration")).All(fact => fact.View == "trends"
            && fact.Category == CmfRenderedInsightsService.Concentration && !string.IsNullOrEmpty(fact.Label)),
            "Workload, pending and milestone concentrations were split into separate decision categories.");
        Check(!source.Facts.Any(fact => fact.Topic == "Open" || fact.Topic == "Closed" || fact.Topic == "Resolution TPT" || fact.Topic == "Total CMFs"), "Routine totals, flat TPT or redundant mixed-signal components leaked into the briefing.");
        Check(source.Facts.Single(fact => fact.Topic == "component-concentration").Evidence.Contains("17 of 20."), "Concentration double-counted LOS, implemented values or the Total row.");
        Check(source.Facts.Single(fact => fact.Topic == "pending-idst-coverage").Evidence.Contains("3 of 4"), "Pending iDST coverage used the wrong denominator.");
        Check(source.Facts.Single(fact => fact.Topic == "pending-age-review").Evidence.Contains("2 of 4"), "Pending age review lost the displayed age-marked count.");
        Check(source.Facts.Single(fact => fact.Topic == "milestone-concentration").Title.Contains("QS"), "Milestone workload concentration was lost.");
        Check(source.Facts.Where(fact => fact.Period != "current").All(fact => fact.Period == "past-7-days"
            && fact.Evidence.Contains("2026-11-09") && fact.Evidence.Contains("2026-11-02")), "Weekly findings did not use an explicit seven-day comparison window.");
        Check(!source.Facts.Any(fact => fact.Source == "Weekly chart") && !CmfRenderedInsightsService.RankingPrompt(source).Contains("WW44"),
            "Calendar-week data leaked into the seven-day-only Insights input.");
        Check(CmfRenderedInsightsService.BuildSource(snapshot, 1, "bar").Hash == source.Hash,
            "Weekly Insights still depend on chart range or calendar-week view.");
        Check(!CmfRenderedInsightsService.RankingPrompt(source).Contains("14000000000"), "Pending link metadata leaked into the compact rank request.");
        var first = CmfRenderedInsightsService.BuildResult(source, source.Facts, true);
        var component = first.TrendFindings.Single(item => item.Topic == "component-concentration");
        Check(component.Kind == "briefing" && component.Evidence.Contains("More than half") && component.Reasons[0].Text.Contains("17 of 20"), "Briefing summary and expandable numeric evidence were not separated.");
        var paged = CmfRenderedInsightsService.BuildResult(source, Enumerable.Range(0, 12).Select(index => new CmfPanelInsightFact {
            Id = "page-" + index, View = index < 6 ? "highlights" : "trends",
            Category = index < 6 ? CmfRenderedInsightsService.NeedsAttention : CmfRenderedInsightsService.Concentration }), true);
        var pagedView = CmfDashboardInsightsService.CreateView(paged, "token");
        Check(pagedView.Highlights.Count == 5 && pagedView.TrendFindings.Count == 5, "First briefing page did not contain five findings in each view.");
        foreach (string name in new[] { "highlights", "trends" })
        {
            var page = CmfDashboardInsightsService.CreatePage(paged, name, 1);
            Check(page.PageSize == 5 && page.Items.Count == 1 && page.Items[0].Id == "A:page-" + (name == "highlights" ? 5 : 11),
                "Briefing paging skipped or repeated findings after regrouping.");
        }
        data["PlatformLabel"] = "Platform B";
        snapshot.Platform = "B";
        var secondSource = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        var second = CmfRenderedInsightsService.BuildResult(secondSource, secondSource.Facts, false);
        var overall = CmfRenderedInsightsService.Aggregate("Overall", new[] { first, second, first });
        Check(!serializer.Serialize(first).Contains("%") && !serializer.Serialize(overall).Contains("%"),
            "Platform or Overall Insights still expose percentages in findings or supporting evidence.");
        var grouped = overall.Highlights.Single(item => item.Topic == "closures-backlog");
        Check(grouped.PlatformCount == 2 && grouped.Reasons.Count == 2 && grouped.SourceInsightIds.Count == 2 && !grouped.AiReviewed
            && grouped.Title.Contains("2 platforms") && overall.HighlightCount == first.HighlightCount, "Overall failed to merge compatible patterns, deduplicate a platform or retain unranked evidence.");
        Check(grouped.Category == CmfRenderedInsightsService.NeedsAttention && grouped.Label == "Mixed Signals",
            "Overall lost the distinction between a decision group and a finding label.");
        Check(grouped.Evidence == "Closed totals increased, while open backlogs were higher than a week earlier.",
            "Overall lost the neutral closure/backlog comparison wording.");
        foreach (var finding in first.Highlights.Concat(first.TrendFindings).Concat(overall.Highlights).Concat(overall.TrendFindings))
            Check(!System.Text.RegularExpressions.Regex.IsMatch(finding.Title + " " + finding.Label + " " + finding.Evidence,
                @"\b(displayed|doubled|dominate|bottleneck|surge|spike|slightly|many|growing|shrinking|improving|taking longer)\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase),
                "A platform or Overall briefing exaggerated a comparison or implied an unsupported ongoing trend.");
        var pending = second.Highlights.Single(item => item.Topic == "Pending");
        pending.Category = "Progress";
        pending.Title = "The pending queue is shrinking";
        Check(CmfRenderedInsightsService.Aggregate("Overall", new[] { first, second }).Highlights.Count(item => item.Topic == "Pending") == 2, "Overall merged opposite queue directions into one conclusion.");
        data["Trend"] = serializer.DeserializeObject("[{\"WeekLabel\":\"WW44\",\"WeekEndOpenIssues\":10},{\"WeekLabel\":\"WW45\",\"WeekEndOpenIssues\":20},{\"WeekLabel\":\"WW46\",\"WeekEndOpenIssues\":15}]");
        Check(CmfRenderedInsightsService.BuildSource(snapshot, 6, "line").Hash == secondSource.Hash,
            "Changing calendar-week history altered rolling seven-day findings.");
        data["ComponentSummary"] = serializer.DeserializeObject("{\"Columns\":[\"Ingredient\",\"Open\"],\"Rows\":[[\"HW\",\"5 invalid\"]]}");
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "line").Facts.Any(fact => fact.Topic == "component-concentration"), "Unrecognized table cell notation produced an invented concentration.");
        snapshot.Data = new Dictionary<string, object>();
        var empty = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        var emptyResult = CmfRenderedInsightsService.AddAi(empty, false);
        Check(empty.Facts.Count == 0 && emptyResult.Mode == "ai" && emptyResult.CandidateCount == 0, "An empty briefing queued an unnecessary model request.");
    }

    private static void CheckRenderedSummaryAnalysis()
    {
        var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
            CapturedAt = new DateTime(2026, 11, 10), ShowComparisons = true,
            Data = new Dictionary<string, object> {
                { "PlatformLabel", "Platform A" }, { "SummaryFacts", new[] {
                    new Dictionary<string, object> { { "Label", "Pending" }, { "Value", 10 },
                        { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", 5 } } } },
                    new Dictionary<string, object> { { "Label", "Closed" }, { "Value", 101 },
                        { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", 90 } } } },
                    new Dictionary<string, object> { { "Label", "Open" }, { "Value", 2 },
                        { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", 1 } } } }
                } }
            } };
        var sources = new List<CmfPanelInsightSource> { CmfRenderedInsightsService.BuildSource(snapshot, 6, "line") };
        string prompt = CmfDashboardSummaryService.AnalysisPrompt(sources);
        Check(prompt.Contains("\"Current\":10") && prompt.Contains("\"Previous\":5") && prompt.Contains("2026-11-03")
            && prompt.Contains("CMF records") && prompt.Contains("\"Denominator\":null") && prompt.Contains("\"PreviousIsEstimate\":true")
            && !prompt.Contains("change-Open"), "Summary input omitted structured evidence or admitted a minor change.");
        Check(!prompt.Contains("AllowedStatements") && !prompt.Contains("AllowedReview") && !prompt.Contains("copied verbatim"),
            "Summary still supplies or requires canned wording.");
        Check(prompt.Contains("once per paragraph") && prompt.Contains("single shared qualification")
            && prompt.Contains("Do not repeat estimated") && !prompt.Contains("explicitly qualify any prior-value comparison as estimated"),
            "Summary instructions still force repetitive uncertainty qualifiers.");
        Check(prompt.Contains("Use estimated, never reconstructed") && !prompt.Contains("Prior values are reconstructed estimates"),
            "Summary instructions still encourage the forbidden history wording.");
        Check(prompt.Contains("Never use percentages") && !prompt.Contains("percentages only when supported")
            && !sources.SelectMany(source => source.Facts).Any(fact => fact.Evidence.Contains("%")),
            "Summary instructions or source evidence still permit percentages.");
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        string[] factIds = { "A:change-Pending" };
        string natural = "There are 10 pending CMFs, compared with an estimated 5 a week earlier.";
        Func<string, string, string[], string, string> reply = (summary, scope, ids, review) => serializer.Serialize(new {
            claims = new[] { new { scope = scope, factIds = ids, summary = summary, reviewSuggestion = review } } });
        Func<CmfDashboardSummary, string, string> assessment = (draft, failedFlag) => {
            string reviewPrompt = CmfDashboardSummaryService.VerificationPrompt(sources, draft);
            var packet = (Dictionary<string, object>)serializer.DeserializeObject(reviewPrompt.Substring(reviewPrompt.IndexOf('\n') + 1));
            var reviewClaims = (object[])packet["Claims"];
            return serializer.Serialize(new { checks = draft.Claims.Select((claim, index) => {
                var check = new Dictionary<string, object> { { "claimIndex", index },
                    { "claimHash", ((Dictionary<string, object>)reviewClaims[index])["Hash"] }, { "factIds", claim.FactIds } };
                foreach (string flag in new[] { "supported", "numbers", "scope", "units", "comparisons", "citations", "review" })
                    check[flag] = flag != failedFlag;
                return check;
            }).ToArray() });
        };
        Action<string, string> rejected = (output, review) => {
            bool failed = false;
            try {
                if (review == null) CmfDashboardSummaryService.Validate(sources, output);
                else CmfDashboardSummaryService.ValidateGrounding(sources, output, review);
            } catch { failed = true; }
            Check(failed, "Summary accepted unsupported content or incomplete verification: " + output.Substring(0, Math.Min(output.Length, 120)));
        };
        string response = reply(natural, "Platform A", factIds, "Consider checking the pending queue before drawing conclusions.");
        var pending = CmfDashboardSummaryService.Validate(sources, response);
        Check(pending.Mode == "ai-review" && pending.Claims.Single().Summary == natural && pending.Claims.Single().Scope == "Platform A",
            "Natural wording was rewritten or published before semantic review.");
        string report = assessment(pending, null);
        var valid = CmfDashboardSummaryService.ValidateGrounding(sources, response, report);
        Check(valid.Mode == "ai" && valid.Claims.Single().Evidence.Single().Text.Contains("Pending: 10 versus 5")
            && valid.Claims.Single().FactIds.Single() == factIds[0], "Summary lost citations or canonical evidence.");
        foreach (string percentage in new[] { "100%", "100 percent", "one hundred percent", "100 per cent",
            "100 per-cent", "100 percentage points", "100 PERCENT", "100\uFF05", "100\u066A" })
        {
            rejected(reply("The pending total increased by " + percentage + ".", "Platform A", factIds, null), null);
            rejected(reply(natural, "Platform A", factIds, "Review the " + percentage + " change."), null);
        }
        int percentageAttempts = 0;
        int percentageReviews = 0;
        var correctedPercentage = CmfDashboardSummaryService.Generate(sources,
            instruction => ++percentageAttempts == 1
                ? reply("There are 10 pending CMFs, 100% above the estimated 5 a week earlier.", "Platform A", factIds, null)
                : response,
            instruction => { percentageReviews++; return report; });
        Check(correctedPercentage.Mode == "ai" && percentageAttempts == 2 && percentageReviews == 1
            && correctedPercentage.Claims.Single().Summary == natural,
            "A percentage draft was published or failed to receive a fresh LLM correction and verification.");
        string shared = "Week-earlier figures are estimated. Closed CMFs total 101 versus 90, while pending CMFs total 10 versus 5.";
        string sharedResponse = reply(shared, "Platform A", new[] { factIds[0], "A:change-Closed" }, null);
        var sharedDraft = CmfDashboardSummaryService.Validate(sources, sharedResponse);
        string sharedPrompt = CmfDashboardSummaryService.VerificationPrompt(sources, sharedDraft);
        Check(sharedPrompt.Contains("Accept a single shared qualification") && sharedPrompt.Contains("Still reject unqualified historical comparisons"),
            "The verifier demands repeated qualifiers or permits unqualified history.");
        Check(sharedPrompt.Contains("Reject the word reconstructed"), "The verifier permits forbidden Summary wording.");
        Check(sharedPrompt.Contains("Reject percentages in any form"), "The verifier permits percentage wording.");
        var sharedValid = CmfDashboardSummaryService.ValidateGrounding(sources, sharedResponse, assessment(sharedDraft, null));
        Check(sharedValid.Claims.Single().Summary == shared && sharedValid.Claims.Single().FactIds.Count == 2,
            "One shared history qualifier could not cover multiple cited comparisons.");
        string unqualified = reply("Closed CMFs total 101 versus 90 a week earlier, while pending CMFs total 10 versus 5.",
            "Platform A", new[] { factIds[0], "A:change-Closed" }, null);
        rejected(unqualified, assessment(CmfDashboardSummaryService.Validate(sources, unqualified), "comparisons"));
        foreach (string wording in new[] {
            "The pending queue contains 10 CMFs; the estimated count a week earlier was 5.",
            "Pending requests total 10, versus an estimated 5 on 2026-11-03."
        })
        {
            string output = reply(wording, "Platform A", factIds, null);
            var draft = CmfDashboardSummaryService.Validate(sources, output);
            Check(CmfDashboardSummaryService.ValidateGrounding(sources, output, assessment(draft, null)).Claims.Single().Summary == wording,
                "A grounded paraphrase still depends on exact sentence matching.");
        }
        foreach (string output in new[] {
            reply(natural, "Platform B", factIds, null),
            reply(natural.Replace("10", "100"), "Platform A", factIds, null),
            reply("The estimated count was 5 on 2026-11-04.", "Platform A", factIds, null),
            reply("There are 10 days in the pending queue.", "Platform A", factIds, null),
            reply("The pending total increased by 99%.", "Platform A", factIds, null),
            reply(natural, "Platform A", new[] { "A:change-Open" }, null),
            reply(natural, "Platform A", new[] { factIds[0], factIds[0] }, null),
            reply(natural + " displayed", "Platform A", factIds, null),
            reply(natural.Replace("estimated", "reconstructed"), "Platform A", factIds, null),
            reply(natural + " RECONSTRUCTED", "Platform A", factIds, null),
            reply(natural, "Platform A", factIds, "Review the reconstructed history."),
            reply("<script>10</script>", "Platform A", factIds, null),
            "{\"claims\":[],\"summary\":\"unsupported\"}", "not json"
        }) rejected(output, null);
        foreach (string flag in new[] { "supported", "numbers", "scope", "units", "comparisons", "citations", "review" })
            rejected(response, assessment(pending, flag));
        foreach (string wording in new[] {
            "There are 5 pending CMFs, compared with an estimated 10 a week earlier.",
            "There are 10 pending days.",
            "There are 10 unique issues pending.",
            "Pending counts are falling steadily because of improved ownership.",
            "Platform B has 10 pending CMFs.",
            "There are ten thousand pending requests."
        })
        {
            string output = reply(wording, "Platform A", factIds, null);
            var draft = CmfDashboardSummaryService.Validate(sources, output);
            rejected(output, assessment(draft, "supported"));
        }
        string badReview = reply(natural, "Platform A", factIds, "Fix the driver bottleneck immediately.");
        rejected(badReview, assessment(CmfDashboardSummaryService.Validate(sources, badReview), "review"));
        rejected(response, "{}");
        rejected(response, "{\"checks\":[]}");
        rejected(response, report.Replace("\"supported\":true", "\"supported\":\"true\""));
        rejected(response, report.Replace("\"claimIndex\":0", "\"claimIndex\":1"));
        rejected(response, report.Replace("\"factIds\":[\"A:change-Pending\"]", "\"factIds\":[\"unknown\"]"));
        rejected(reply(natural.Replace("There are", "The snapshot contains"), "Platform A", factIds, pending.Claims[0].ReviewSuggestion), report);
        Check(pending.Mode == "ai-review", "Verification mutated the unpublished draft.");
        int generationCalls = 0, reviewCalls = 0;
        string correctionPrompt = null;
        var recovered = CmfDashboardSummaryService.Generate(sources, text => {
            generationCalls++;
            if (generationCalls == 1) return reply(natural + " reconstructed", "Platform A", factIds, null);
            correctionPrompt = text;
            return response;
        }, text => { reviewCalls++; return report; });
        Check(recovered.Mode == "ai" && recovered.Claims.Single().Summary == natural && generationCalls == 2 && reviewCalls == 1
            && correctionPrompt.Contains("invalid-text") && correctionPrompt.Contains("same supplied facts"),
            "An invalid draft was not corrected once and reverified without a fallback.");
        generationCalls = reviewCalls = 0;
        var groundedRecovery = CmfDashboardSummaryService.Generate(sources, text => { generationCalls++; return response; }, text => {
            reviewCalls++;
            return reviewCalls == 1 ? assessment(pending, "review") : report;
        });
        Check(groundedRecovery.Mode == "ai" && generationCalls == 2 && reviewCalls == 2,
            "A failed grounding check bypassed review or could not recover through a fresh draft.");
        generationCalls = 0;
        var permanentlyInvalid = CmfDashboardSummaryService.Generate(sources, text => {
            generationCalls++;
            return reply(natural + " reconstructed", "Platform A", factIds, null);
        }, text => { throw new Exception("Invalid drafts must never reach verification."); });
        Check(permanentlyInvalid.Mode == "ai-unavailable" && permanentlyInvalid.Claims.Count == 0 && generationCalls == 2
            && permanentlyInvalid.FailureStage == "draft-validation" && permanentlyInvalid.FailureCode == "invalid-text",
            "Summary recovery exceeded its bound, exposed a draft or lost safe diagnostics.");
        generationCalls = reviewCalls = 0;
        var permanentlyUnsupported = CmfDashboardSummaryService.Generate(sources, text => { generationCalls++; return response; },
            text => { reviewCalls++; return assessment(pending, "review"); });
        Check(permanentlyUnsupported.Mode == "ai-unavailable" && permanentlyUnsupported.Claims.Count == 0
            && generationCalls == 2 && reviewCalls == 2 && permanentlyUnsupported.FailureCode == "grounding-review",
            "A repeatedly unsupported assertion was published or retried without a bound.");
        generationCalls = 0;
        var providerFailure = CmfDashboardSummaryService.Generate(sources, text => {
            generationCalls++;
            throw new InvalidOperationException("Summary request unavailable.");
        }, text => report);
        Check(providerFailure.Mode == "ai-unavailable" && providerFailure.Claims.Count == 0 && generationCalls == 1
            && providerFailure.FailureCode == "generation-unavailable", "Validation recovery duplicated provider-level retries or exposed fallback text.");
        var cancelled = CmfDashboardSummaryService.Generate(sources, text => { throw new OperationCanceledException(); }, text => report);
        Check(cancelled.Mode == "ai-unavailable" && cancelled.Claims.Count == 0 && cancelled.FailureCode == "cancelled",
            "A cancelled Summary request was retried or published.");
        foreach (string mode in new[] { "ai-loading", "ai-unavailable", "empty" })
            Check(CmfDashboardSummaryService.WithoutClaims(sources, mode).Claims.Count == 0,
                "A non-AI Summary state exposed canonical summary claims.");
        snapshot.Platform = "B";
        snapshot.Data["PlatformLabel"] = "Platform B";
        sources.Add(CmfRenderedInsightsService.BuildSource(snapshot, 6, "line"));
        rejected(reply("There are 20 pending requests across platforms.", "Platform A", new[] { factIds[0], "B:change-Pending" }, null), null);
        string separate = serializer.Serialize(new { claims = sources.Select(source => new { scope = source.Scope,
            factIds = new[] { source.Platform + ":change-Pending" }, summary = natural, reviewSuggestion = (string)null }).ToArray() });
        var separateDraft = CmfDashboardSummaryService.Validate(sources, separate);
        var separateValid = CmfDashboardSummaryService.ValidateGrounding(sources, separate, assessment(separateDraft, null));
        Check(separateValid.Claims.Count == 2 && separateValid.Claims.Select(claim => claim.Scope).Distinct().Count() == 2
            && separateValid.Claims.All(claim => claim.Evidence.Count == 1), "Natural analysis merged platform populations or lost evidence.");
        snapshot.Data.Clear();
        sources = new List<CmfPanelInsightSource> { CmfRenderedInsightsService.BuildSource(snapshot, 6, "line") };
        var empty = CmfDashboardSummaryService.Get(sources, false);
        Check(empty.Mode == "empty" && empty.Claims.Count == 0, "Empty Summary input fabricated a claim or queued AI.");
    }

    private static void CheckRenderedPendingAgeThreshold()
    {
        var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
            CapturedAt = new DateTime(2026, 11, 9), Data = new Dictionary<string, object> {
                { "PlatformLabel", "Platform A" },
                { "PendingSummary", new Dictionary<string, object> {
                    { "Columns", new[] { "Ingredient", "iDST Name", "CMF Pending" } },
                    { "Rows", new[] { new[] { "HW", "Named contact", "4:::OVER_TWO_DAYS:::4" } } }
                } }
            } };
        foreach (int threshold in new[] { -1, 0, 1, 2, 7 })
        {
            snapshot.Data["PendingSummaryAgeThresholdDays"] = threshold;
            var source = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
            if (threshold <= 0)
            {
                Check(!source.Facts.Any(item => item.Topic == "pending-age-review"),
                    "A nonpositive age threshold produced an unnecessary delay highlight.");
                continue;
            }
            var fact = source.Facts.Single(item => item.Topic == "pending-age-review");
            string expected = "more than " + threshold + (threshold == 1 ? " day" : " days");
            Check(fact.Summary.Contains(expected) && fact.Evidence.Contains(expected) && fact.Evidence.Contains("4 of 4")
                && !fact.Evidence.Contains("%") && !fact.Evidence.Contains("SLA") && !fact.Summary.Contains("age setting")
                && fact.Title.StartsWith("4 pending requests") && fact.Label == "Pending age review",
                "Pending age wording omitted actual counts/thresholds or overstated the observation.");
            Check(fact.Summary.StartsWith("All pending CMFs have been waiting "),
                "A fully aged pending queue must be described as all, not merely at least half.");
        }
        snapshot.Data["PendingSummaryAgeThresholdDays"] = 2;
        var pendingTable = (Dictionary<string, object>)snapshot.Data["PendingSummary"];
        foreach (var scenario in new[] {
            Tuple.Create(2, 2, "All pending CMFs"),
            Tuple.Create(4, 2, "Half of pending CMFs"),
            Tuple.Create(4, 3, "Most pending CMFs"),
            Tuple.Create(20, 19, "Most pending CMFs") })
        {
            pendingTable["Rows"] = new[] { new[] { "HW", "Named contact",
                scenario.Item1 + ":::OVER_TWO_DAYS:::" + scenario.Item2 } };
            var scenarioSource = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
            var scenarioFact = scenarioSource.Facts.Single(item => item.Topic == "pending-age-review");
            Check(scenarioFact.Summary.StartsWith(scenario.Item3 + " have been waiting ")
                && scenarioFact.Evidence.Contains(scenario.Item2 + " of " + scenario.Item1)
                && !scenarioFact.Summary.Contains("%"),
                "Pending age wording must reflect the actual affected share without changing its evidence.");
        }
        pendingTable["Rows"] = new[] { new[] { "HW", "Named contact", "4:::OVER_TWO_DAYS:::4" } };
        var firstSource = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        var first = CmfRenderedInsightsService.BuildResult(firstSource, firstSource.Facts, false);
        snapshot.Platform = "B";
        snapshot.Data["PlatformLabel"] = "Platform B";
        var sameSource = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        var same = CmfRenderedInsightsService.BuildResult(sameSource, sameSource.Facts, false);
        Check(CmfRenderedInsightsService.Aggregate("Overall", new[] { first, same }).Highlights
            .Single(item => item.Topic == "pending-age-review").Evidence.Contains("more than 2 days"),
            "Overall omitted a shared pending age threshold.");
        snapshot.Data["PendingSummaryAgeThresholdDays"] = 7;
        var changedSource = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        Check(changedSource.Hash != sameSource.Hash, "Changing the age threshold reused stale Insights text when counts stayed unchanged.");
        var changed = CmfRenderedInsightsService.BuildResult(changedSource, changedSource.Facts, false);
        var overall = CmfRenderedInsightsService.Aggregate("Overall", new[] { first, changed }).Highlights
            .Single(item => item.Topic == "pending-age-review");
        Check(overall.Evidence.Contains("Platform A:") && overall.Evidence.Contains("more than 2 days")
            && overall.Evidence.Contains("Platform B:") && overall.Evidence.Contains("more than 7 days"),
            "Overall conflated different pending age thresholds.");
        snapshot.Data.Remove("PendingSummaryAgeThresholdDays");
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "line").Facts.Any(item => item.Topic == "pending-age-review"),
            "Missing age threshold metadata produced an unsupported age highlight.");
        snapshot.Data["PendingSummaryAgeThresholdDays"] = "invalid";
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "line").Facts.Any(item => item.Topic == "pending-age-review"),
            "An invalid age threshold produced an unsupported age highlight.");
        snapshot.Data["PendingSummaryAgeThresholdDays"] = 2;
        foreach (var values in new[] { new[] { 1, 1, 1 }, new[] { 2, 1, 1 }, new[] { 4, 2, 1 },
            new[] { 4, 1, 2 }, new[] { 10, 2, 2 }, new[] { 4, 2, 2 }, new[] { 0, 0, 0 } })
        {
            int population = values[0], unassigned = values[1], aged = values[2];
            int agedUnassigned = Math.Min(unassigned, aged);
            snapshot.Data["PendingSummary"] = new Dictionary<string, object> {
                { "Columns", new[] { "Ingredient", "iDST Name", "CMF Pending" } },
                { "Rows", new[] {
                    new[] { "HW", "Unassigned", unassigned + ":::OVER_TWO_DAYS:::" + agedUnassigned },
                    new[] { "SW", "Named contact", (population - unassigned) + ":::OVER_TWO_DAYS:::" + (aged - agedUnassigned) }
                } }
            };
            var source = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
            bool ageHighlight = aged >= 2 && aged * 2 >= population;
            Check(source.Facts.Any(item => item.Topic == "pending-idst-coverage") == (unassigned >= 2)
                && source.Facts.Any(item => item.Topic == "pending-age-review") == ageHighlight,
                "A single pending request created a contact/age highlight or a supported multi-request finding was lost.");
            string prompt = CmfRenderedInsightsService.RankingPrompt(source);
            Check((unassigned >= 2 || !prompt.Contains("pending-idst-coverage"))
                && (ageHighlight || !prompt.Contains("pending-age-review")),
                "A suppressed single-request pending highlight reached the ranking prompt.");
            var result = CmfRenderedInsightsService.BuildResult(source, source.Facts, false);
            var combined = CmfRenderedInsightsService.Aggregate("Overall", new[] { result });
            Check(combined.Highlights.Any(item => item.Topic == "pending-idst-coverage") == (unassigned >= 2)
                && combined.Highlights.Any(item => item.Topic == "pending-age-review") == ageHighlight,
                "Overall restored a suppressed single-request pending highlight.");
        }
    }

    private static void CheckRenderedMixedSignalMateriality()
    {
        Func<int, int, int, CmfPanelInsightSource> project = (previous, current, closed) => {
            var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
                CapturedAt = new DateTime(2026, 11, 9), ShowComparisons = true,
                Data = new Dictionary<string, object> {
                    { "PlatformLabel", "Platform A" },
                    { "SummaryFacts", new[] {
                        new Dictionary<string, object> { { "Label", "Open" }, { "Value", current },
                            { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", previous } } } },
                        new Dictionary<string, object> { { "Label", "Closed" }, { "Value", closed },
                            { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", 70 } } } }
                    } }
                } };
            return CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        };
        foreach (var values in new[] { new[] { 19, 20 }, new[] { 100, 101 }, new[] { 0, 1 }, new[] { 19, 19 },
            new[] { 10, 11 }, new[] { 1000, 1002 } })
        {
            var source = project(values[0], values[1], 80);
            Check(!source.Facts.Any(fact => fact.Topic == "closures-backlog" || fact.Topic == "Open")
                && source.Facts.Any(fact => fact.Topic == "Closed" && fact.Category == CmfRenderedInsightsService.Progress),
                "Minor or unchanged backlog triggered a mixed alert or hid independent closure progress.");
            var result = CmfRenderedInsightsService.BuildResult(source, source.Facts, false);
            var overall = CmfRenderedInsightsService.Aggregate("Overall", new[] { result });
            Check(!overall.Highlights.Any(item => item.Topic == "closures-backlog")
                && overall.Highlights.Any(item => item.Topic == "Closed"),
                "Overall retained a minor-backlog mixed alert or lost closure progress.");
        }
        foreach (var values in new[] { new[] { 10, 12 }, new[] { 100, 110 }, new[] { 0, 2 } })
        {
            var source = project(values[0], values[1], 80);
            Check(source.Facts.Count(fact => fact.Topic == "closures-backlog") == 1
                && !source.Facts.Any(fact => fact.Topic == "Open" || fact.Topic == "Closed"),
                "Material backlog growth lost its combined finding or duplicated the constituent metrics.");
        }
        foreach (int closed in new[] { 70, 71, 72 })
        {
            var source = project(20, 30, closed);
            Check(!source.Facts.Any(fact => fact.Topic == "closures-backlog" || fact.Topic == "Closed")
                && source.Facts.Any(fact => fact.Topic == "Open"),
                "A minor closure change entered a mixed alert or suppressed independent material backlog growth.");
        }
        var improving = project(20, 19, 80);
        Check(!improving.Facts.Any(fact => fact.Topic == "closures-backlog"),
            "A shrinking backlog triggered a mixed-growth finding.");
    }

    private static void CheckRenderedQueueChangeMateriality()
    {
        foreach (string label in new[] { "Open", "Pending", "Closed" })
        foreach (var values in new[] {
            new[] { 1, 2, 0 }, new[] { 2, 1, 0 }, new[] { 0, 1, 0 }, new[] { 1, 0, 0 },
            new[] { 10, 11, 0 }, new[] { 11, 10, 0 }, new[] { 1000, 1002, 0 }, new[] { 1002, 1000, 0 },
            new[] { 100, 109, 0 }, new[] { 100, 91, 0 }, new[] { 2, 2, 0 },
            new[] { 10, 12, 1 }, new[] { 20, 18, 1 }, new[] { 100, 110, 1 }, new[] { 100, 90, 1 },
            new[] { 0, 2, 1 }, new[] { 2, 0, 1 }, new[] { 5, 10, 1 } })
        {
            var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
                CapturedAt = new DateTime(2026, 11, 10), ShowComparisons = true,
                Data = new Dictionary<string, object> {
                    { "PlatformLabel", "Platform A" },
                    { "SummaryFacts", new[] {
                        new Dictionary<string, object> { { "Label", label }, { "Value", values[1] },
                            { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", values[0] } } } }
                    } }
                } };
            var source = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
            bool material = values[2] == 1;
            Check(source.Facts.Any(fact => fact.Topic == label) == material,
                label + " change did not require both two requests and ten percent (two requests from zero).");
            if (!material)
                Check(!CmfRenderedInsightsService.RankingPrompt(source).Contains("change-" + label),
                    "A suppressed small count change reached the AI ranking prompt.");
            if (material)
                Check(source.Facts.Single(fact => fact.Topic == label).Category == ((label == "Closed" ? values[1] > values[0] : values[1] < values[0])
                    ? CmfRenderedInsightsService.Progress : CmfRenderedInsightsService.NeedsAttention),
                    label + " materiality changed the direction of a retained finding.");
            var result = CmfRenderedInsightsService.BuildResult(source, source.Facts, false);
            Check(CmfRenderedInsightsService.Aggregate("Overall", new[] { result }).Highlights
                .Any(item => item.Topic == label) == material,
                "Overall restored a suppressed " + label + " change or lost a material change.");
        }
    }

    private static void CheckRenderedTptGrouping()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
            CapturedAt = new DateTime(2026, 11, 9), ShowComparisons = true,
            Data = serializer.Deserialize<Dictionary<string, object>>(@"{
                ""PlatformLabel"":""Platform A"",
                ""SummaryFacts"":[{""Label"":""Open"",""Value"":""20"",""WeeklyTrend"":{""PreviousValue"":18}}],
                ""TptFacts"":[
                    {""Label"":""CMF Resolution TPT"",""Value"":""25"",""WeeklyTrend"":{""PreviousValue"":28}},
                    {""Label"":""CMF Overall TPT"",""Value"":""55"",""WeeklyTrend"":{""PreviousValue"":62}},
                    {""Label"":""CMF Minor TPT"",""Value"":""19"",""WeeklyTrend"":{""PreviousValue"":20}},
                    {""Label"":""CMF Validation TPT"",""Value"":""10"",""WeeklyTrend"":{""PreviousValue"":10}}]
            }") };
        var source = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        Check(source.Facts.Count(fact => fact.Title.Contains("TPT")) == 1,
            "Same-direction TPT changes were not combined into one platform finding.");
        var progress = source.Facts.Single(fact => fact.Topic == "TPT" && fact.Category == CmfRenderedInsightsService.Progress);
        Check(progress.Summary == "CMF Overall TPT and CMF Resolution TPT averages are lower than a week earlier.",
            "Grouped TPT summary should use direct wording without the redundant displayed qualifier.");
        Check(progress.Evidence.Contains("CMF Resolution TPT: 25 days versus 28")
            && progress.Evidence.Contains("CMF Overall TPT: 55 days versus 62")
            && progress.Evidence.Contains("2026-11-02") && !progress.Evidence.Contains("Validation TPT")
            && !progress.Evidence.Contains("Minor TPT")
            && source.Facts.Any(fact => fact.Topic == "Open"),
            "Grouped TPT lost material comparisons, included small/flat metrics or absorbed non-TPT findings.");
        var first = CmfRenderedInsightsService.BuildResult(source, source.Facts, true);
        snapshot.Platform = "B";
        snapshot.Data["PlatformLabel"] = "Platform B";
        snapshot.Data["TptFacts"] = serializer.DeserializeObject(@"[
            {""Label"":""CMF Resolution TPT"",""Value"":""25"",""WeeklyTrend"":{""PreviousValue"":22}},
            {""Label"":""CMF Overall TPT"",""Value"":""55"",""WeeklyTrend"":{""PreviousValue"":48}}]");
        var slower = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        Check(slower.Facts.Count(fact => fact.Topic == "TPT") == 1
            && slower.Facts.Single(fact => fact.Topic == "TPT").Category == CmfRenderedInsightsService.NeedsAttention,
            "Same-direction TPT degradations were not combined into one attention finding.");
        Check(slower.Facts.Single(fact => fact.Topic == "TPT").Summary
            == "CMF Overall TPT and CMF Resolution TPT averages are higher than a week earlier.",
            "Degrading TPT summary lost its direction or direct wording.");
        snapshot.Data["TptFacts"] = serializer.DeserializeObject(@"[
            {""Label"":""CMF Resolution TPT"",""Value"":""25"",""WeeklyTrend"":{""PreviousValue"":28}},
            {""Label"":""CMF Overall TPT"",""Value"":""55"",""WeeklyTrend"":{""PreviousValue"":48}}]");
        var mixed = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        Check(mixed.Facts.Count(fact => fact.Topic == "TPT") == 2
            && mixed.Facts.Where(fact => fact.Topic == "TPT").Select(fact => fact.Category).Distinct().Count() == 2,
            "Opposite TPT directions were merged into the same finding.");
        var second = CmfRenderedInsightsService.BuildResult(mixed, mixed.Facts, false);
        var overall = CmfRenderedInsightsService.Aggregate("Overall", new[] { first, second });
        var combined = overall.Highlights.Single(item => item.Topic == "TPT" && item.Category == CmfRenderedInsightsService.Progress);
        Check(combined.Evidence.StartsWith("One or more TPT averages are ")
            && overall.Highlights.Where(item => item.Topic == "TPT").All(item =>
                item.Evidence.IndexOf("displayed", StringComparison.OrdinalIgnoreCase) < 0),
            "Overall TPT summaries retained the redundant displayed qualifier.");
        Check(overall.Highlights.Count(item => item.Topic == "TPT") == 2 && combined.PlatformCount == 2
            && !combined.AiReviewed && combined.Reasons.Count == 2
            && combined.Reasons.Any(reason => reason.Text.Contains("CMF Overall TPT: 55 days versus 62"))
            && combined.Reasons.Any(reason => reason.Text.Contains("CMF Resolution TPT: 25 days versus 28")),
            "Overall duplicated same-direction TPT items or lost platform-specific evidence and partial-ranking state.");
        snapshot.Data["TptFacts"] = serializer.DeserializeObject(@"[
            {""Label"":""CMF Overall TPT"",""Value"":""55"",""WeeklyTrend"":{""PreviousValue"":62}}]");
        var single = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        Check(single.Facts.Single(fact => fact.Topic == "TPT").Summary
            == "CMF Overall TPT average is lower than a week earlier.",
            "Single-metric TPT summary lost singular grammar or direct wording.");
        var singleResult = CmfRenderedInsightsService.BuildResult(single, single.Facts, true);
        var rollup = CmfRenderedInsightsService.Aggregate("Overall", new[] { first, singleResult });
        Check(rollup.Highlights.Count(item => item.Topic == "TPT") == 1
            && CmfDashboardInsightsService.CreatePage(rollup, "highlights", 0).Items.Count(item => item.Topic == "TPT") == 1,
            "Different platform TPT combinations did not merge before pagination.");
        foreach (var values in new[] {
            new decimal[] { 37, 39, 0 }, new decimal[] { 20, 19, 0 },
            new decimal[] { 10, 11, 0 }, new decimal[] { 10, 9, 0 },
            new decimal[] { 100, 103, 0 }, new decimal[] { 100, 97, 0 },
            new decimal[] { 20, 21.99m, 0 }, new decimal[] { 20, 18.01m, 0 },
            new decimal[] { 20.01m, 22.01m, 0 }, new decimal[] { 20.01m, 18.01m, 0 },
            new decimal[] { 20, 22, 1 }, new decimal[] { 20, 18, 1 },
            new decimal[] { 0, 1, 0 }, new decimal[] { 0, 2, 1 }, new decimal[] { 20, 20, 0 } })
        {
            snapshot.Data["TptFacts"] = new[] {
                new Dictionary<string, object> { { "Label", "CMF Overall TPT" }, { "Value", values[1] },
                    { "WeeklyTrend", new Dictionary<string, object> { { "PreviousValue", values[0] } } } }
            };
            bool material = values[2] == 1;
            var filtered = CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
            Check(filtered.Facts.Any(fact => fact.Topic == "TPT") == material
                && filtered.Facts.Any(fact => fact.Topic == "Open"),
                "TPT materiality did not require two days and ten percent, or changed non-TPT findings.");
            if (material)
                Check(filtered.Facts.Single(fact => fact.Topic == "TPT").Category
                    == (values[1] < values[0] ? CmfRenderedInsightsService.Progress : CmfRenderedInsightsService.NeedsAttention),
                    "TPT materiality changed the direction of a retained finding.");
            else
                Check(!CmfRenderedInsightsService.RankingPrompt(filtered).Contains("CMF Overall TPT"),
                    "A suppressed small TPT change reached the AI ranking prompt.");
            var filteredResult = CmfRenderedInsightsService.BuildResult(filtered, filtered.Facts, false);
            Check(CmfRenderedInsightsService.Aggregate("Overall", new[] { filteredResult }).Highlights
                .Any(item => item.Topic == "TPT") == material,
                "Overall did not retain the platform TPT materiality filter.");
        }
        snapshot.ShowComparisons = false;
        Check(!CmfRenderedInsightsService.BuildSource(snapshot, 6, "line").Facts.Any(fact => fact.Topic == "TPT"),
            "Grouping exposed hidden TPT comparisons in short date scopes.");
    }

    private static void CheckRenderedConcentrationTies()
    {
        Func<string, int[], CmfPanelInsightSource> project = (section, counts) => {
            string column = section == "PendingSummary" ? "CMF Pending" : section == "MilestoneSummary" ? "Unique CMF Count" : "Open";
            var snapshot = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
                CapturedAt = new DateTime(2026, 11, 9), Data = new Dictionary<string, object> {
                    { "PlatformLabel", "Platform A" },
                    { section, new Dictionary<string, object> {
                        { "Columns", new[] { "Ingredient", column } },
                        { "Rows", counts.Select((count, index) => new[] {
                            index == 0 ? "CSME" : index == 1 ? "PnP" : "Other " + index,
                            count.ToString(System.Globalization.CultureInfo.InvariantCulture) }).ToArray() }
                    } }
                } };
            return CmfRenderedInsightsService.BuildSource(snapshot, 6, "line");
        };
        foreach (string section in new[] { "PendingSummary", "ComponentSummary", "MilestoneSummary" })
        {
            foreach (var counts in new[] { new[] { 1 }, new[] { 1, 0, 0 }, new[] { 0, 1, 0 }, new[] { 1, 1 }, new[] { 1, 1, 1 } })
            {
                var balanced = project(section, counts);
                Check(!balanced.Facts.Any(fact => fact.Category == CmfRenderedInsightsService.Concentration),
                    section + " described a single-issue or evenly split population as a concentration.");
                var result = CmfRenderedInsightsService.BuildResult(balanced, balanced.Facts, false);
                Check(CmfRenderedInsightsService.Aggregate("Overall", new[] { result }).TrendCount == 0,
                    "Overall retained a fabricated concentration from " + section + ".");
            }
            Check(project(section, new[] { 2 }).Facts.Single(fact => fact.Category == CmfRenderedInsightsService.Concentration)
                .Evidence.Contains("2 of 2"), section + " suppressed a concentration with more than one issue.");
            var leader = project(section, new[] { 2, 1 }).Facts.Single(fact => fact.Category == CmfRenderedInsightsService.Concentration);
            Check(leader.Title.StartsWith("CSME") && leader.Evidence.Contains("2 of 3"),
                section + " lost a genuinely leading cluster or its evidence.");
            string subject = section == "PendingSummary" ? "pending queue" : section == "MilestoneSummary" ? "milestone workload" : "open workload";
            Check(leader.Summary == "More than half of the " + subject + " sits in CSME. PnP is the next-largest cluster.",
                section + " did not explicitly name the leading cluster in the majority description.");
            var tiedRunners = project(section, new[] { 4, 3, 3 }).Facts.Single(fact => fact.Category == CmfRenderedInsightsService.Concentration);
            Check(!tiedRunners.Summary.Contains("next-largest"), section + " arbitrarily selected one of two equal runners-up.");
            Check(tiedRunners.Summary == "A large share of the " + subject + " sits in CSME.",
                section + " did not explicitly name the leading cluster in the large-share description.");
            var tiedLeaders = project(section, new[] { 4, 4, 2 }).Facts;
            Check(!tiedLeaders.Any(fact => fact.Title.Contains("is the largest cluster")),
                section + " incorrectly described one tied leader as uniquely largest.");
            if (section != "MilestoneSummary")
            {
                Check(tiedLeaders.Single(fact => fact.Category == CmfRenderedInsightsService.Concentration).Title.StartsWith("CSME and PnP")
                    && tiedLeaders.Single(fact => fact.Category == CmfRenderedInsightsService.Concentration).Evidence.Contains("Combined count: 8 of 10."),
                    section + " lost a genuine two-cluster concentration above the remaining population.");
                Check(project(section, new[] { 7, 6, 4, 3 }).Facts.Single(fact => fact.Category == CmfRenderedInsightsService.Concentration)
                    .Evidence.Contains("Combined count: 13 of 20."), section + " lost the existing two-leading-cluster finding.");
            }
        }
        var singlePending = new CmfPanelSnapshotCache { Platform = "A", DateScope = "all", Configuration = "scope",
            CapturedAt = new DateTime(2026, 11, 9), Data = new Dictionary<string, object> {
                { "PlatformLabel", "Platform A" },
                { "PendingSummaryAgeThresholdDays", 2 },
                { "PendingSummary", new Dictionary<string, object> {
                    { "Columns", new[] { "Ingredient", "iDST Name", "CMF Pending" } },
                    { "Rows", new[] { new[] { "CSME", "Unassigned", "1:::OVER_TWO_DAYS:::1" } } }
                } }
            } };
        var pendingFacts = CmfRenderedInsightsService.BuildSource(singlePending, 6, "line").Facts;
        Check(!pendingFacts.Any(fact => fact.Category == CmfRenderedInsightsService.Concentration)
            && !pendingFacts.Any(fact => fact.Topic == "pending-idst-coverage" || fact.Topic == "pending-age-review"),
            "A single pending request produced an unnecessary concentration, contact or age highlight.");
    }

    private static void CheckRenderedOverallRanking()
    {
        var source = new CmfPanelInsightSource { Platform = "A", Scope = "Platform A", Hash = "same-facts",
            CapturedAt = "2026-11-09T12:00:00", Facts = new List<CmfPanelInsightFact> {
                new CmfPanelInsightFact { Id = "open", Topic = "Open", Title = "Open backlog is growing", View = "highlights",
                    Category = CmfRenderedInsightsService.NeedsAttention, Score = 90, Evidence = "Open evidence" },
                new CmfPanelInsightFact { Id = "pending", Topic = "Pending", Title = "Pending queue is growing", View = "highlights",
                    Category = CmfRenderedInsightsService.NeedsAttention, Score = 10, Evidence = "Pending evidence" },
                new CmfPanelInsightFact { Id = "component", Topic = "component-concentration", Title = "Component concentration", View = "trends",
                    Category = CmfRenderedInsightsService.Concentration, Score = 90, Evidence = "Component evidence" },
                new CmfPanelInsightFact { Id = "milestone", Topic = "milestone-concentration", Title = "Milestone concentration", View = "trends",
                    Category = CmfRenderedInsightsService.Concentration, Score = 10, Evidence = "Milestone evidence" }
            } };
        var pending = CmfRenderedInsightsService.BuildResult(source, source.Facts, false);
        var initial = CmfRenderedInsightsService.Aggregate("Overall", new[] { pending });
        Check(initial.Highlights[0].Topic == "Open" && initial.TrendFindings[0].Topic == "component-concentration",
            "Preliminary Overall lost its deterministic fallback order.");
        var ranked = CmfRenderedInsightsService.BuildResult(source, source.Facts.AsEnumerable().Reverse(), true);
        var complete = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked });
        Check(complete.Highlights[0].Topic == "Pending" && complete.TrendFindings[0].Topic == "milestone-concentration",
            "Overall discarded completed platform AI rankings in Highlights or Trends.");
        Check(complete.SourceHash != initial.SourceHash && complete.HighlightCount == initial.HighlightCount
            && complete.TrendCount == initial.TrendCount && complete.Highlights[0].Reasons[0].Text.Contains("Pending evidence"),
            "Applying platform rankings lost findings, evidence or the client invalidation hash.");
        source.Platform = "B";
        source.Scope = "Platform B";
        var waiting = CmfRenderedInsightsService.BuildResult(source, source.Facts, false);
        var partial = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, waiting });
        Check(partial.Mode == "ai-loading" && partial.Highlights[0].Topic == "Pending"
            && partial.TrendFindings[0].Topic == "milestone-concentration" && !partial.Highlights[0].AiReviewed,
            "Incomplete platform rankings overrode completed AI priorities or were labeled reviewed.");
        waiting.Mode = "ai-unavailable";
        var unavailable = CmfRenderedInsightsService.Aggregate("Overall", new[] { waiting });
        Check(unavailable.Mode == "ai-unavailable" && unavailable.Highlights[0].Topic == "Open"
            && unavailable.HighlightCount == initial.HighlightCount, "Failed AI removed the verified fallback briefing.");
        var partialFailure = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, waiting });
        Check(partialFailure.Mode == "ai-unavailable" && partialFailure.Highlights[0].Topic == "Pending"
            && partialFailure.Highlights[0].RankScore == complete.Highlights[0].RankScore,
            "A failed platform ranking overrode or inflated completed AI priorities.");
        var second = CmfRenderedInsightsService.BuildResult(source, source.Facts.AsEnumerable().Reverse(), true);
        var combined = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, second });
        var duplicate = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, second, ranked });
        Check(combined.Highlights[0].Topic == "Pending" && combined.TrendFindings[0].Topic == "milestone-concentration"
            && combined.Highlights[0].PlatformCount == 2 && combined.Highlights[0].AiReviewed
            && duplicate.Highlights.Select(item => item.Id).SequenceEqual(combined.Highlights.Select(item => item.Id)),
            "Cross-platform ranking consensus or duplicate-platform handling was lost.");
        var page = CmfDashboardInsightsService.CreatePage(combined, "highlights", 0);
        Check(page.Items[0].Topic == "Pending", "Overall pagination did not preserve the AI-derived ordering.");
        var opposite = CmfRenderedInsightsService.BuildResult(source, source.Facts, true);
        var tied = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, opposite });
        var repeated = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, opposite, ranked });
        Check(tied.Highlights[0].Topic == "Open" && tied.TrendFindings[0].Topic == "component-concentration"
            && tied.Highlights.Select(item => item.RankScore).Distinct().Count() == 1
            && repeated.Highlights.Select(item => item.RankScore).SequenceEqual(tied.Highlights.Select(item => item.RankScore)),
            "Conflicting AI rankings lost the fixed-score tie-breaker or duplicate inputs received extra votes.");
        source.Platform = "C";
        source.Scope = "Platform C";
        var third = CmfRenderedInsightsService.BuildResult(source, source.Facts, true);
        var consensus = CmfRenderedInsightsService.Aggregate("Overall", new[] { ranked, opposite, third });
        Check(consensus.Highlights[0].Topic == "Open" && consensus.TrendFindings[0].Topic == "component-concentration",
            "Overall ignored the combined priorities from multiple completed platform rankings.");
    }

    private static void CheckRenderedFactSelection()
    {
        var source = new CmfPanelInsightSource { Platform = "NVL-H", Scope = "NVL-H", Facts = new List<CmfPanelInsightFact> {
            new CmfPanelInsightFact { Id = "day-total", View = "highlights", Period = "day", Source = "CMF Summary", Evidence = "99 CMFs." },
            new CmfPanelInsightFact { Id = "ingredient-hw", View = "trends", Source = "Component Summary", Evidence = "HW: 11 open." }
        } };
        var selected = CmfRenderedInsightsService.SelectFacts(source, "{\"highlights\":[\"day-total\"],\"trends\":[\"ingredient-hw\"]}");
        Check(selected.Count == 2 && object.ReferenceEquals(selected[0], source.Facts[0]), "Ranked Insights did not preserve the exact server-owned facts.");
        foreach (string invalid in new[] {
            "{\"highlights\":[\"invented-issue\"],\"trends\":[\"ingredient-hw\"]}",
            "{\"highlights\":[\"ingredient-hw\"],\"trends\":[\"day-total\"]}",
            "{\"highlights\":[\"day-total\",\"day-total\"],\"trends\":[\"ingredient-hw\"]}",
            "{\"highlights\":[],\"trends\":[\"ingredient-hw\"]}",
            "{\"highlights\":[\"day-total\"],\"trends\":[\"ingredient-hw\"],\"opinion\":[\"invented\"]}"
        })
        {
            bool rejected = false;
            try { CmfRenderedInsightsService.SelectFacts(source, invalid); }
            catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "Unsupported or fabricated Insights ranking was accepted.");
        }
    }

    private static void CheckDashboardRateLimitRecovery()
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        Type limited = typeof(AiSummaryService).GetNestedType("DashboardRateLimitException", flags);
        var execute = typeof(AiSummaryService).GetMethod("ExecuteDashboardAiRequest", flags);
        var delay = typeof(AiSummaryService).GetMethod("DashboardRetryDelay", flags);
        DateTime now = new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);
        Check((TimeSpan)delay.Invoke(null, new object[] { "45", 0, now }) == TimeSpan.FromSeconds(45), "Retry-After seconds were ignored.");
        Check((TimeSpan)delay.Invoke(null, new object[] { now.AddSeconds(90).ToString("r"), 0, now }) == TimeSpan.FromSeconds(90), "Retry-After HTTP date was ignored.");
        Check((TimeSpan)delay.Invoke(null, new object[] { "invalid", 1, now }) == TimeSpan.FromSeconds(30), "Missing Retry-After did not use exponential backoff.");
        var waits = new List<TimeSpan>();
        var attempts = new List<int>();
        int calls = 0;
        Func<string> send = () => {
            if (++calls < 3) throw (Exception)Activator.CreateInstance(limited, new object[] { "7" });
            return "validated provider response";
        };
        Action<TimeSpan> wait = duration => waits.Add(duration);
        Action<int, TimeSpan> progress = (attempt, duration) => attempts.Add(attempt);
        string output = (string)execute.Invoke(null, new object[] { send, wait, progress, System.Threading.CancellationToken.None });
        Check(output == "validated provider response" && calls == 3 && waits.SequenceEqual(new[] { TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(7) })
            && attempts.SequenceEqual(new[] { 1, 2, 0 }), "A recoverable 429 did not preserve the request, publish cooldown progress or clear the wait on success.");
        calls = 0;
        waits.Clear();
        attempts.Clear();
        send = () => { calls++; throw (Exception)Activator.CreateInstance(limited, new object[] { "" }); };
        bool exhausted = false;
        try { execute.Invoke(null, new object[] { send, wait, progress, System.Threading.CancellationToken.None }); }
        catch (System.Reflection.TargetInvocationException ex) { exhausted = ex.InnerException is InvalidOperationException && ex.InnerException.Message.Contains("429"); }
        Check(exhausted && calls == 4 && waits.SequenceEqual(new[] { TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60) })
            && attempts.SequenceEqual(new[] { 1, 2, 3, -1 }),
            "Persistent rate limits were retried indefinitely or did not back off.");
        waits.Clear();
        attempts.Clear();
        send = () => { throw (Exception)Activator.CreateInstance(limited, new object[] { "900" }); };
        exhausted = false;
        try { execute.Invoke(null, new object[] { send, wait, progress, System.Threading.CancellationToken.None }); }
        catch (System.Reflection.TargetInvocationException ex) { exhausted = ex.InnerException is InvalidOperationException; }
        Check(exhausted && waits.Count == 0 && attempts.SequenceEqual(new[] { -1 }), "Long provider cooldown was ignored, announced an unattempted retry or blocked a job indefinitely.");
        calls = 0;
        send = () => { calls++; throw new System.Net.WebException("Non-rate-limit failure"); };
        try { execute.Invoke(null, new object[] { send, wait, progress, System.Threading.CancellationToken.None }); }
        catch (System.Reflection.TargetInvocationException ex) { Check(ex.InnerException is System.Net.WebException, "A non-429 failure was masked."); }
        Check(calls == 1, "A non-rate-limit failure was retried.");
        var canceled = new System.Threading.CancellationTokenSource();
        canceled.Cancel();
        calls = 0;
        try { execute.Invoke(null, new object[] { send, wait, progress, canceled.Token }); }
        catch (System.Reflection.TargetInvocationException ex) { Check(ex.InnerException is OperationCanceledException, "Canceled rate-limit waits were not stopped."); }
        Check(calls == 0, "A canceled analysis sent a provider request.");
        canceled.Dispose();
        using (var cancelDuringWait = new System.Threading.CancellationTokenSource())
        {
            calls = 0;
            send = () => { calls++; throw (Exception)Activator.CreateInstance(limited, new object[] { "7" }); };
            wait = duration => cancelDuringWait.Cancel();
            bool interrupted = false;
            try { execute.Invoke(null, new object[] { send, wait, progress, cancelDuringWait.Token }); }
            catch (System.Reflection.TargetInvocationException ex) { interrupted = ex.InnerException is OperationCanceledException; }
            Check(interrupted && calls == 1, "Cancellation during cooldown did not prevent the next provider request.");
        }
    }

    private static void CheckStructuredCandidates(DateTime today)
    {
        var documents = Enumerable.Range(0, 8).Select(index => new CmfInsightDocument {
            Id = (16070000000L + index).ToString(), Platforms = new List<string> { "PTL" },
            Buckets = new List<string> { index == 0 ? "Closed" : index == 1 ? "Implemented" : "Open" },
            Fields = new Dictionary<string, string> {
                { "Local.processor", "pantherlake" }, { "Local.component_group", index < 3 ? "Graphics" : "Media" },
                { "Local.component", index < 3 ? "Display" : "Codec " + index }, { "Local.family", index < 3 ? "Client" : "Other " + index },
                { "Local.release", index < 3 ? "R1" : "R" + index }, { "Local.title", "Identical words must not drive grouping" }
            }
        }).ToList();
        var candidates = CmfDashboardInsightsService.RankCandidates(documents);
        Check(candidates.Any(candidate => candidate.IssueIds.Count == 3 && candidate.SharedFields["component"] == "display"), "Multi-field categorical cluster was missed.");
        Check(candidates.All(candidate => candidate.SharedFields.Count >= 2 && candidate.Score > 0 && candidate.IssueIds.Count < documents.Count), "Broad platform classification was ranked as a meaningful pattern.");
        Check(candidates.Select(candidate => string.Join(",", candidate.IssueIds)).Distinct().Count() == candidates.Count, "Equivalent candidate membership was duplicated.");
        Check(candidates.First().ClosedIssues == 1 && candidates.First().ImplementedIssues == 1, "Closed or Implemented issues were excluded from clustering.");
        Check(CmfDashboardInsightsService.RankCandidates(documents.AsEnumerable().Reverse()).Select(candidate => candidate.Id).SequenceEqual(candidates.Select(candidate => candidate.Id)), "Candidate ranks depend on source row order.");
        foreach (var document in documents) document.Fields = new Dictionary<string, string> { { "Local.processor", "pantherlake" }, { "Local.title", "same title" } };
        Check(CmfDashboardInsightsService.RankCandidates(documents).Count == 0, "Platform/processor or arbitrary text alone fabricated a cluster.");
    }

    private static void CheckHybridPipeline(DataTable data, DateTime today)
    {
        data = data.Copy();
        foreach (DataRow row in data.Rows) row["title"] = "External display loses signal during resume handshake";
        var evidence = CmfDashboardInsightsService.Build(data, "PTL", today);
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var fetched = new List<string>();
        var supplied = new HashSet<string>();
        int calls = 0;
        bool requestDetails = false;
        bool sawTail = false;
        const string technical = "Display resume loses the external panel after the firmware handshake times out.";
        Func<string, HsdArticleData> fetch = id => {
            fetched.Add(id);
            return new HsdArticleData { ArticleId = id, FetchSuccess = true,
                Description = technical, Comments = new List<string> { technical },
                InsightFields = new Dictionary<string, string> { { "description", new string('x', 24000) + technical },
                    { "comments[8]", technical }, { "discussion_page[0][1].fields.description", technical },
                    { "discussion_page[0][1].schema.enormous_schema", new string('z', 100000) },
                    { "comments[0].definitions.enormous_schema", new string('z', 100000) },
                    { "definitions.enormous_schema", new string('z', 100000) } } };
        };
        Func<string, string> generate = prompt => {
            calls++;
            Check(!prompt.Contains("enormous_schema"), "Provider schema metadata leaked into AI context.");
            sawTail = sawTail || prompt.Contains(technical);
            var input = serializer.Deserialize<Dictionary<string, object>>(prompt.Substring(prompt.IndexOf("\nINPUT_JSON\n", StringComparison.Ordinal) + 12));
            var packets = ((System.Collections.IEnumerable)input["Groups"]).Cast<object>()
                .Select(item => serializer.Deserialize<CmfInsightClusterPacket>(serializer.Serialize(item))).ToList();
            foreach (var packet in packets) foreach (var source in packet.Evidence) supplied.Add(source.IssueId);
            Check(packets.All(packet => packet.Evidence.All(chunk => chunk.Field != "Local.processor" && chunk.Field != "Local.family")),
                "Repeated per-member categories were not removed from compact AI evidence.");
            return serializer.Serialize(new CmfInsightClusterReply { Reviews = packets.Select(packet => new CmfInsightClusterOpinion {
                Id = packet.Id, NeedsDetails = requestDetails && packet.Stage == "structured",
                Supported = !(requestDetails && packet.Stage == "structured"), Kind = "trait", Title = "Display-focused cohort",
                Opinion = "Shared display classification is useful for review but does not establish a common cause.",
                Action = "Compare the recorded symptoms before combining investigations.",
                EvidenceKeys = packet.Stage == "rollup" ? packet.PlatformInsights.Select(finding => finding.Id).ToList()
                    : requestDetails ? packet.Evidence.Where(chunk => chunk.Field == "Local.title").Select(chunk => chunk.Key).ToList() : new List<string>()
            }).ToList() });
        };
        var result = CmfDashboardInsightsService.AnalyzeHybrid(evidence, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        Check(calls > 0 && fetched.Count == 0, "Structured review fetched all HSDs eagerly.");
        Check(supplied.SetEquals(evidence.Candidates.SelectMany(candidate => candidate.IssueIds)), "AI received ungrouped issues or skipped candidate members.");
        Check(result.Mode == "ai" && result.HighlightCount > 0 && result.GroupCount == 0, "A classification trait was mislabeled as technical similarity.");
        Check(result.Highlights.All(finding => finding.AiReviewed && finding.SharedFields.Count >= 2), "Hybrid output lost AI provenance or structured justification.");
        requestDetails = true;
        supplied.Clear();
        var snapshots = new List<CmfDashboardInsights>();
        Action<CmfDashboardInsights> capture = snapshot => snapshots.Add(serializer.Deserialize<CmfDashboardInsights>(serializer.Serialize(snapshot)));
        var detailed = CmfDashboardInsightsService.AnalyzeHybrid(evidence, fetch, generate, capture, System.Threading.CancellationToken.None);
        Check(fetched.Count == fetched.Distinct().Count() && fetched.All(id => evidence.Candidates.Any(candidate => candidate.IssueIds.Contains(id))), "On-demand evidence was duplicated or fetched outside clusters.");
        Check(sawTail && detailed.RetrievedIssues == fetched.Count, "Requested rich text was truncated or retrieval progress omitted.");
        Check(snapshots.Any(snapshot => snapshot.Mode == "ai-loading" && snapshot.HighlightCount > 0
            && snapshot.ReviewedCandidates < evidence.CandidateCount && snapshot.Message.Contains("technical evidence")),
            "Nested technical findings were withheld until their outer group completed.");
        int before = calls;
        var empty = CmfDashboardInsightsService.Build(data.Clone(), "Empty", today);
        var noGroups = CmfDashboardInsightsService.AnalyzeHybrid(empty, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        Check(calls == before && noGroups.Highlights.Count == 0 && noGroups.SourceStatus.Contains("0 ranked groups"), "Empty grouping fabricated an AI call or insights.");
        var second = CmfDashboardInsightsService.Build(data.Copy(), "Second", today);
        foreach (var document in second.Documents) document.Platforms = new List<string> { "SECOND" };
        second.Candidates = CmfDashboardInsightsService.RankCandidates(second.Documents);
        var secondResult = CmfDashboardInsightsService.AnalyzeHybrid(second, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        snapshots.Clear();
        var rollup = CmfDashboardInsightsService.AnalyzeRollup(evidence, new List<CmfDashboardInsights> { result, secondResult }, generate, System.Threading.CancellationToken.None, capture);
        Check(rollup.Mode == "ai" && rollup.PlatformsReady == 2 && rollup.SourceStatus.Contains("no raw-HSD re-fetch"), "Overall did not derive from cached platform insights.");
        Check(snapshots.Any(snapshot => snapshot.Mode == "ai-loading" && snapshot.HighlightCount > 0 && snapshot.Highlights.All(finding => finding.AiReviewed)),
            "Overall withheld validated findings until its full synthesis completed.");
        Check(snapshots.SelectMany(snapshot => snapshot.Highlights).All(finding => finding.PlatformCount == finding.Issues.Select(issue => issue.Platform).Distinct().Count()
            && finding.DistinctIssues == finding.Issues.Select(issue => issue.Id).Distinct().Count()),
            "Partial Overall findings published incorrect platform or distinct-HSD counts.");
        Check(rollup.Highlights.Any(finding => finding.PlatformCount >= 2) && rollup.HighlightCount >= result.HighlightCount,
            "Overall collapsed all platform insights to a single summary or omitted cross-platform shared traits.");
        bool failed = false;
        try { CmfDashboardInsightsService.AnalyzeRollup(evidence, new List<CmfDashboardInsights> { evidence }, generate, System.Threading.CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "Overall accepted incomplete platform reviews.");
        foreach (string defect in new[] { "missing-review", "foreign-source", "taxonomy-technical", "undated-trend" })
        {
            failed = false;
            try {
                CmfDashboardInsightsService.AnalyzeHybrid(evidence, fetch, prompt => {
                    var reply = serializer.Deserialize<CmfInsightClusterReply>(generate(prompt));
                    if (defect == "missing-review") reply.Reviews.RemoveAt(0);
                    else { reply.Reviews[0].Supported = true; reply.Reviews[0].NeedsDetails = false;
                        reply.Reviews[0].Kind = defect == "taxonomy-technical" ? "technical" : defect == "undated-trend" ? "trend" : "trait";
                        reply.Reviews[0].EvidenceKeys = defect == "foreign-source" ? new List<string> { "foreign" } : new List<string>(); }
                    return serializer.Serialize(reply);
                }, ignored => {}, System.Threading.CancellationToken.None);
            } catch (InvalidOperationException) { failed = true; }
            Check(failed, "Invalid hybrid opinion accepted: " + defect);
        }
        requestDetails = false;
        var bulk = CmfDashboardInsightsService.Build(data.Clone(), "Bulk", today);
        bulk.Documents = Enumerable.Range(0, 24).Select(index => new CmfInsightDocument {
            Id = (16080000000L + index).ToString(), Platforms = new List<string> { "PTL" },
            Buckets = new List<string> { "Open" }, Occurrences = 1, OpenRecords = 1,
            Fields = new Dictionary<string, string> { { "Local.title", technical }, { "Local.processor", "pantherlake" },
                { "Local.component_group", "Group " + index / 2 }, { "Local.component", "Ingredient " + index / 2 },
                { "Local.family", "Family " + index / 4 } }
        }).ToList();
        bulk.Candidates = CmfDashboardInsightsService.RankCandidates(bulk.Documents);
        bulk.CandidateCount = bulk.Candidates.Count;
        bulk.ClusteredIssues = bulk.Records = bulk.DistinctIssues = bulk.Documents.Count;
        snapshots.Clear();
        var many = CmfDashboardInsightsService.AnalyzeHybrid(bulk, id => { throw new Exception("Unexpected eager fetch."); }, generate,
            capture, System.Threading.CancellationToken.None);
        Check(many.HighlightCount > 8 && many.ReviewedCandidates == bulk.CandidateCount,
            "Hybrid candidate review has a fixed issue/group/highlight cap.");
        Check(snapshots.Any(snapshot => snapshot.HighlightCount > 0 && snapshot.ReviewedCandidates < bulk.CandidateCount),
            "Validated findings were not published sequentially before the complete analysis.");
        Check(Enumerable.Range(1, bulk.CandidateCount).All(reviewed => snapshots.Any(snapshot => snapshot.ReviewedCandidates == reviewed)),
            "Review progress skipped completed groups within a batch.");
        var partial = snapshots.First(snapshot => snapshot.HighlightCount > 8);
        Check(CmfDashboardInsightsService.CreateView(partial, "partial-test").Highlights.Count == 8,
            "Loading views dropped already validated findings or bypassed pagination.");
        Check(CmfDashboardInsightsService.CreatePage(partial, "highlights", 1).Items.Count == partial.HighlightCount - 8,
            "Partial finding pagination did not expose every reviewed group.");
        partial.Success = false;
        partial.Mode = "ai-unavailable";
        Check(CmfDashboardInsightsService.CreateView(partial, "partial-test").HighlightCount == partial.HighlightCount
            && CmfDashboardInsightsService.CreatePage(partial, "highlights", 1).Success,
            "A stopped review discarded completed AI findings.");
        snapshots.Clear();
        failed = false;
        try {
            CmfDashboardInsightsService.AnalyzeHybrid(bulk, fetch, prompt => {
                var reply = serializer.Deserialize<CmfInsightClusterReply>(generate(prompt));
                reply.Reviews[1].EvidenceKeys = new List<string> { "foreign" };
                return serializer.Serialize(reply);
            }, capture, System.Threading.CancellationToken.None);
        } catch (InvalidOperationException) { failed = true; }
        Check(failed && snapshots.Any(snapshot => snapshot.HighlightCount > 0),
            "Later invalid evidence either bypassed validation or erased previously published AI findings.");
        var pages = new List<string>();
        for (int page = 0; page * CmfDashboardInsightsService.FindingsPerPage < many.HighlightCount; page++)
            pages.AddRange(CmfDashboardInsightsService.CreatePage(many, "highlights", page).Items.Select(finding => finding.Id));
        Check(pages.Count == many.HighlightCount && pages.Distinct().Count() == pages.Count,
            "Hybrid pagination lost or duplicated reviewed groups.");
    }

    private static void CheckAiPipeline(DataTable data, DateTime today)
    {
        var evidence = CmfDashboardInsightsService.Build(data, "PTL", today);
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var fetched = new List<string>();
        var reviewed = new HashSet<string>();
        int modelCalls = 0;
        string technical = "Display resume loses the external panel after the firmware handshake times out.";
        Func<string, HsdArticleData> fetch = id => {
            fetched.Add(id);
            return new HsdArticleData { ArticleId = id, FetchSuccess = true,
                InsightFields = new Dictionary<string, string> {
                    { "description", new string('x', 24000) + technical }, { "summary", technical },
                    { "comments[8]", technical }, { "sysdebug_forum", technical }, { "steps_to_reproduce", technical }
                } };
        };
        Func<string, string> generate = prompt => {
            modelCalls++;
            var input = serializer.Deserialize<Dictionary<string, object>>(prompt.Substring(prompt.IndexOf("\nINPUT_JSON\n", StringComparison.Ordinal) + 12));
            if (input.ContainsKey("Chunks"))
            {
                var chunks = ((System.Collections.IEnumerable)input["Chunks"]).Cast<object>().Select(item => serializer.Deserialize<CmfInsightChunk>(serializer.Serialize(item))).ToList();
                foreach (var chunk in chunks) reviewed.Add(chunk.Key);
                return serializer.Serialize(new CmfInsightReviewReply {
                    Reviewed = chunks.Select(chunk => chunk.Key).ToList(),
                    Observations = chunks.Where(chunk => chunk.Text.Contains(technical)).Select(chunk => new CmfInsightObservation {
                        Title = "Resume handshake timeout", Summary = "External panel lost after firmware handshake timeout.", Action = "Compare the recorded resume trace.",
                        Citations = new List<CmfInsightCitation> { new CmfInsightCitation { Chunk = chunk.Key, Quote = technical } }
                    }).ToList()
                });
            }
            var observations = ((System.Collections.IEnumerable)input["Observations"]).Cast<object>().Select(item => serializer.Deserialize<CmfInsightObservation>(serializer.Serialize(item))).ToList();
            return serializer.Serialize(new CmfInsightComparisonReply {
                Reviewed = observations.Select(observation => observation.Id).ToList(),
                Findings = observations.SelectMany(observation => observation.Citations).Select(citation => citation.IssueId).Distinct().Count() < 2
                    ? new List<CmfInsightSemanticFinding>() : new List<CmfInsightSemanticFinding> { new CmfInsightSemanticFinding {
                        Title = "External-panel resume handshake", Explanation = "Each excerpt reports external panel loss after a firmware handshake timeout; root cause remains unconfirmed.",
                        Action = "Compare the recorded resume traces.", Confidence = "high", Citations = observations.SelectMany(observation => observation.Citations).ToList()
                    } }
            });
        };
        var result = CmfDashboardInsightsService.Analyze(evidence, fetch, generate, progress => {}, System.Threading.CancellationToken.None);
        Check(fetched.Count == evidence.DistinctIssues && fetched.Distinct().Count() == evidence.DistinctIssues, "AI did not retrieve every distinct scoped issue exactly once.");
        Check(result.Mode == "ai" && result.AnalyzedIssues == evidence.DistinctIssues && result.AnalyzedChunks == result.TotalChunks
            && reviewed.Count == result.TotalChunks, "AI skipped input chunks or returned a non-AI result.");
        if (evidence.DistinctIssues == 0)
        {
            Check(modelCalls == 1 && result.Highlights.Count == 0 && result.Groups.Count == 0, "Empty scope bypassed AI or fabricated insights.");
            return;
        }
        Check(result.Highlights.Count > 3 && result.Highlights.All(finding => finding.AiReviewed), "AI findings were capped or mixed with calculated results.");
        Check(result.Highlights.Count(finding => finding.Id.StartsWith("highlight-", StringComparison.Ordinal)) == evidence.DistinctIssues,
            "The same verbatim technical evidence repeated across fields created redundant highlights.");
        Check(result.Groups.Count > 0 && result.Groups.All(group => group.Reasons.Count > 2), "Semantic similarity lacks source excerpts.");
        if (evidence.DistinctIssues > 20)
            Check(result.TotalComparisons > 1 && result.ComparisonsCompleted == result.TotalComparisons, "Cross-block AI comparisons were skipped.");
        Check(result.Highlights.SelectMany(finding => finding.Reasons).Any(reason => reason.Source == "HSD.description")
            && result.Highlights.SelectMany(finding => finding.Reasons).Any(reason => reason.Source == "HSD.comments[8]")
            && result.Highlights.SelectMany(finding => finding.Reasons).Any(reason => reason.Source == "HSD.sysdebug_forum"), "Long text tails, older comments or forum evidence were omitted.");
        Check(!CmfDashboardInsightsService.CreatePage(evidence, "highlights", 0).Success
            && CmfDashboardInsightsService.CreateView(evidence, "test").Highlights.Count == 0, "Calculated results were publicly exposed before AI analysis.");
        var view = CmfDashboardInsightsService.CreateView(result, "opaque-token");
        Check(view.Highlights.Count == Math.Min(CmfDashboardInsightsService.FindingsPerPage, result.Highlights.Count) && view.HighlightCount == result.Highlights.Count
            && view.Documents == null, "Public pagination capped AI analysis or exposed raw source documents.");
        var ids = new List<string>();
        for (int page = 0; page * CmfDashboardInsightsService.FindingsPerPage < result.HighlightCount; page++)
            ids.AddRange(CmfDashboardInsightsService.CreatePage(result, "highlights", page).Items.Select(finding => finding.Id));
        Check(ids.Count == result.HighlightCount && ids.Distinct().Count() == ids.Count, "Pagination lost or duplicated AI findings.");
        Check(!CmfDashboardInsightsService.CreatePage(result, "other", 0).Success
            && !CmfDashboardInsightsService.CreatePage(result, "highlights", -1).Success
            && !CmfDashboardInsightsService.CreatePage(result, "highlights", int.MaxValue).Success, "Invalid page/view accepted.");
        bool failed = false;
        try { CmfDashboardInsightsService.Analyze(evidence, fetch, prompt => "{}", progress => {}, System.Threading.CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "AI failure silently returned calculated findings.");
        var adaptive = CmfDashboardInsightsService.Analyze(evidence, fetch, prompt => {
            string output = generate(prompt);
            if (prompt.Contains("\"Chunks\""))
            {
                var reply = serializer.Deserialize<CmfInsightReviewReply>(output);
                if (reply.Reviewed.Count > 5) reply.Reviewed.RemoveAt(0);
                return serializer.Serialize(reply);
            }
            return output;
        }, progress => {}, System.Threading.CancellationToken.None);
        Check(adaptive.Mode == "ai" && adaptive.AnalyzedChunks == adaptive.TotalChunks && adaptive.AnalyzedIssues == evidence.DistinctIssues,
            "Adaptive AI batch retries skipped inputs or accepted incomplete acknowledgments.");
        foreach (string defect in new[] { "missing-input", "invented-quote", "invented-chunk", "taxonomy-only", "low-confidence", "foreign-comparison" })
        {
            failed = false;
            try
            {
                CmfDashboardInsightsService.Analyze(evidence, fetch, prompt => {
                    string output = generate(prompt);
                    if (prompt.Contains("\"Chunks\""))
                    {
                        var reply = serializer.Deserialize<CmfInsightReviewReply>(output);
                        if (defect == "missing-input" && reply.Reviewed.Count > 0) reply.Reviewed.RemoveAt(0);
                        if (reply.Observations.Count > 0)
                        {
                            var citation = reply.Observations[0].Citations[0];
                            if (defect == "invented-quote") citation.Quote = "This technical claim does not occur in any source field.";
                            if (defect == "invented-chunk") citation.Chunk = "out-of-scope";
                            if (defect == "taxonomy-only") citation.Quote = "Display";
                        }
                        return serializer.Serialize(reply);
                    }
                    if (defect == "low-confidence" || defect == "foreign-comparison")
                    {
                        var reply = serializer.Deserialize<CmfInsightComparisonReply>(output);
                        if (reply.Findings.Count > 0)
                        {
                            if (defect == "low-confidence") reply.Findings[0].Confidence = "low";
                            else reply.Findings[0].Citations[0].Chunk = "out-of-scope";
                        }
                        return serializer.Serialize(reply);
                    }
                    return output;
                }, progress => {}, System.Threading.CancellationToken.None);
            }
            catch (InvalidOperationException) { failed = true; }
            Check(failed, "Invalid AI evidence accepted: " + defect);
        }
        failed = false;
        try { CmfDashboardInsightsService.Analyze(evidence, id => null, generate, progress => {}, System.Threading.CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "Missing HSD retrieval silently reduced all-issue AI coverage.");
    }

    public static void CheckBalanceEvidenceSql(SqlConnection connection)
    {
        DateTime today = new DateTime(2026, 11, 3);
        string source = @"(SELECT cp_id, title, source_platform, component, component_group, promoted_id, merge_id,
            cmf_request, status, sysdebug, TRY_CAST(asked AS date) AS date_cmf_ask,
            TRY_CAST(decided AS date) AS date_cmf_decided, TRY_CAST(implemented AS date) AS implemented_date,
            TRY_CAST(closed AS date) AS closed_date FROM (VALUES
            ('16011111111','Old open','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_ok','open','customer_must_fix','2026-10-01','2026-10-10',NULL,NULL),
            ('16022222222','New open','CMF_NVL_H_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_duplicate','open','customer_must_fix','2026-11-01','2026-11-02',NULL,NULL),
            ('16033333333','Closed','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-10',NULL,'2026-11-02'),
            ('16044444444','Invalid status dates','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','implemented','customer_must_fix','2026-10-01','2026-10-10',NULL,NULL),
            ('16055555555','Reversed decision','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','open','customer_must_fix','2026-10-10','2026-10-01',NULL,NULL),
            ('16066666666','Not CMF','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','open','other','2026-10-01','2026-10-10',NULL,NULL),
            ('16077777777','Future old implementation','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_ok','future','customer_must_fix','2026-10-01','2026-10-10','2026-10-20',NULL),
            ('16088888888','Future recent implementation','CMF_NVL_H_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_duplicate','future','customer_must_fix','2026-10-01','2026-10-10','2026-11-02',NULL),
            ('16099999999','Future missing implementation','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','future','customer_must_fix','2026-10-01','2026-10-10',NULL,NULL)
            ) AS fixture(cp_id,title,source_platform,component,component_group,promoted_id,merge_id,cmf_request,status,sysdebug,asked,decided,implemented,closed)) AS source";
        DataTable evidence = new DataTable();
        using (SqlCommand command = CmfSummaryTrendService.CreateBalanceEvidenceCommand(connection, source, today))
        using (SqlDataReader reader = command.ExecuteReader()) evidence.Load(reader);
        var insights = CmfDashboardInsightsService.Build(evidence, "Overall", today);
        using (SqlCommand command = CmfSummaryTrendService.CreateBalanceEstimateCommand(connection, source, today))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            reader.Read();
            var platforms = insights.Trends.Where(trend => trend.Dimension == "Platform");
            Check(platforms.Sum(trend => trend.Open) == reader.GetInt32(3), "Grouped current balances differ from Summary SQL.");
            Check(platforms.Sum(trend => trend.PreviousOpen.GetValueOrDefault()) == reader.GetInt32(7), "Grouped historical balances differ from Summary SQL.");
            Check(insights.ExcludedRecords == reader.GetInt32(9) && insights.Records == reader.GetInt32(10), "Group eligibility differs from Summary SQL.");
        }
        Console.WriteLine("Dashboard Insights SQL eligibility parity passed.");
    }
}