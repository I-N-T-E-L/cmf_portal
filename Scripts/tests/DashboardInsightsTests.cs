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
        CheckPlatformBriefingProjection();
        CheckPlatformBriefingPayload();
        CheckPlatformBriefingProviderFailures();
        CheckPlatformBriefingLatency();
        CheckOverallBriefingResume();
        CheckBriefingBacklogMateriality();
        CheckBriefingHighlightCandidates();
        CheckBriefingPanelIsolation();
        CheckBriefingCustomerIssueGrouping();
        CheckBriefingTptMagnitude();
        CheckBriefingIdstExclusion();
        CheckPlatformBriefingValidation();
        CheckPlatformBriefingHistory();
        CheckOverallBriefingProjection();
        CheckOverallBriefingBatching();
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

    public static void CheckPlatformBriefingProjection()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var snapshot = new CmfPanelSnapshotCache { Platform = "CMF_NVL_H_ALL_COMPONENTS_TABLE", CapturedAt = new DateTime(2026, 11, 19),
            DateScope = "all", Configuration = "fixture", ShowComparisons = true,
            Data = serializer.Deserialize<Dictionary<string, object>>(@"{""PlatformLabel"":""NVL H Dashboard"",
                ""SummaryFacts"":[{""Label"":""Open"",""Value"":""12"",""WeeklyTrend"":{""PreviousValue"":8}}],
                ""ComponentSummary"":{""Columns"":[""Ingredient"",""QS:::Open""],""Rows"":[[""Graphics"",""4(2)+3(1)""],[""Total"",""4(2)+3(1)""]]},
                ""PendingSummaryAgeThresholdDays"":2,""OldestPendingAgeDays"":14,
                ""PendingSummary"":{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],""Rows"":[[""Graphics"",""Unassigned"",""3:::OVER_TWO_DAYS:::2:::OPEN_HSDS:::16011111111""]]},
                ""DesignSummary"":{""Columns"":[""Design Name"",""SW Image Freeze"",""Support Model"",""QS"",""Impl/Verified""],""Rows"":[[""Design A"",""2026-11-25"",""Full"",""2"",""5""]]},
                ""MilestoneSummary"":{""Columns"":[""Milestone"",""Unique CMF Count""],""Rows"":[[""QS"",""12""]]}}") };
        var source = CmfPlatformBriefingService.BuildSource(snapshot);
        string priorStyleHash = CmfAssistantResponseCache.Fingerprint(new { Pipeline = CmfPlatformBriefingService.Pipeline,
            snapshot.Platform, snapshot.Configuration, source.Scope, source.DateScope,
            Date = snapshot.CapturedAt.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), source.Evidence });
        Check(source.Hash != priorStyleHash, "A previous writing style reused its cached platform briefing.");
        Check(source.Evidence.Any(item => item.Section == "DesignSummary") && source.Evidence.Any(item => item.Section == "MilestoneSummary"), "Platform briefing lost relevant page tables.");
        var component = source.Evidence.Single(item => item.Section == "ComponentSummary");
        Check(component.Measurements.Single(item => item.Name == "QS Open unique").Current == 4
            && component.Measurements.Single(item => item.Name == "QS Open duplicates").Current == 3
            && component.Measurements.Single(item => item.Name == "QS Open LOS subset").Current == 2, "Unique, duplicate and LOS values were conflated.");
        Check(component.Measurements.First().Denominator == 4, "Total row was counted again.");
        Check(component.Measurements.Single(item => item.Name == "Open records across milestones").Current == 7,
            "Ingredient Open totals double-counted LOS subsets.");
        Check(component.Measurements.Single(item => item.Name == "Open unique across milestones").Current == 4
            && component.Measurements.Single(item => item.Name == "Open duplicates across milestones").Current == 3
            && component.Measurements.Single(item => item.Name == "Open LOS subset across milestones").Current == 3,
            "Prominent ingredient aggregate split was lost or counted LOS as extra Open records.");
        var conciseIngredientOutput = serializer.Serialize(new { findings = new[] { new {
            title = "Graphics open backlog", summary = "Graphics has 7 open records across milestones.",
            kind = "pattern", tone = "neutral", section = "highlights", evidenceIds = new[] { component.Id },
            support = new[] { new { evidenceId = component.Id, measurementNames = new[] { "Open records across milestones" },
                explanation = "The component table totals 7 Open records for Graphics across milestone columns, without adding the LOS subset." } } } } });
        var conciseIngredient = CmfPlatformBriefingService.Validate(source, conciseIngredientOutput).Single();
        Check(conciseIngredient.Evidence.Single().Text.Contains("Field Open records across milestones: 7 records")
            && !conciseIngredient.Evidence.Single().Text.Contains("Field Open unique across milestones")
            && !conciseIngredient.Evidence.Single().Text.Contains("Field Open duplicates across milestones")
            && !conciseIngredient.Evidence.Single().Text.Contains("Field Open LOS subset across milestones"),
            "A concise ingredient finding forced unnecessary unique/duplicate/LOS fields into its Source.");
        Check(source.Evidence.Single(item => item.Section == "DesignExposure").Measurements
            .Single(item => item.Name == "Designs with Open records").Current == 1, "Design breadth was not projected.");
        Check(source.Evidence.Single(item => item.Subject == "Pending queue coverage").Measurements.Single(item => item.Name == "Aged pending").Current == 2,
            "Aged pending subset was lost.");
        Check(source.Evidence.Single(item => item.Subject == "Pending queue coverage").Measurements.Single(item => item.Name == "Oldest pending age").Current == 14,
            "Typed oldest pending age was lost.");
        Check(source.Evidence.Single(item => item.Subject == "Pending queue coverage").Context["Population"].Contains("not Total CMFs"),
            "Pending-assessment denominators were mixed with accepted CMF totals.");
        Check(!serializer.Serialize(source).Contains("16011111111"), "Hidden pending issue IDs leaked into the briefing input.");
        Check(!source.Evidence.Single(item => item.Section == "CMF Summary").Measurements.Single().Previous.HasValue,
            "A seven-day comparison leaked into today's platform briefing.");
        Check(!source.Evidence.Any(item => item.Section == "Weekly chart" || item.Kind == "history" || item.Kind == "flow"),
            "Historical chart evidence leaked into today's platform briefing.");
        var splitSnapshot = new CmfPanelSnapshotCache { Platform = snapshot.Platform, CapturedAt = snapshot.CapturedAt,
            DateScope = snapshot.DateScope, Configuration = snapshot.Configuration, Data = new Dictionary<string, object> {
                { "PlatformLabel", "NVL H Dashboard" }, { "PendingSummaryAgeThresholdDays", 2 },
                { "PendingSummary", serializer.DeserializeObject(@"{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],""Rows"":[
                    [""HW"",""Unassigned"",""2:::OVER_TWO_DAYS:::1""],[""HW"",""Owner"",""3:::OVER_TWO_DAYS:::2""],
                    [""Memory"",""Owner"",""1:::OVER_TWO_DAYS:::1""],[""Total"","""",""6""]]}") } } };
        var splitSource = CmfPlatformBriefingService.BuildSource(splitSnapshot);
        var pendingIngredient = splitSource.Evidence.Single(item => item.Subject == "HW ingredient total");
        Check(pendingIngredient.Measurements.Single(item => item.Name == "CMF Pending").Current == 5
            && pendingIngredient.Measurements.Single(item => item.Name == "CMF Pending").Denominator == 6
            && pendingIngredient.Measurements.Single(item => item.Name == "Aged pending").Current == 3
            && pendingIngredient.Measurements.Single(item => item.Name == "Aged pending").Denominator == 5
            && pendingIngredient.Measurements.Single(item => item.Name == "Pending without iDST").Current == 2
            && pendingIngredient.Measurements.Single(item => item.Name == "Pending without iDST").Denominator == 5,
            "Pending ingredient split mixed owner rows, queue/ingredient denominators or Total records.");
        Check(splitSource.Evidence.Single(item => item.Subject == "Pending queue coverage").Measurements
            .Single(item => item.Name == "Aged pending").Current == 4, "Ingredient aggregates were double-counted in queue coverage.");
        var ingredientOutput = serializer.Serialize(new { findings = new[] { new {
            title = "Pending HW concentration", summary = "HW accounts for 5 of 6 pending assessments, including 3 above the 2-day threshold; 2 of the 5 have no iDST.",
            kind = "pattern", tone = "attention", section = "focus", evidenceIds = new[] { pendingIngredient.Id },
            support = new[] { new { evidenceId = pendingIngredient.Id,
                measurementNames = new[] { "CMF Pending", "Aged pending", "Pending age threshold", "Pending without iDST" },
                explanation = "The HW total combines its iDST rows; aging and assignment are independent subsets of its five pending assessments." } } } } });
        var ingredientFinding = CmfPlatformBriefingService.Validate(splitSource, ingredientOutput).Single();
        Check(ingredientFinding.Evidence.Single().Text.Contains("Field CMF Pending: 5 records")
            && ingredientFinding.Evidence.Single().Text.Contains("Field Aged pending: 3 records")
            && ingredientFinding.Evidence.Single().Text.Contains("Field Pending without iDST: 2 records")
            && !ingredientFinding.Evidence.Single().Text.Contains("Oldest pending age"),
            "Prominent ingredient split did not reach the selected Sources or leaked excluded age detail.");
        Check(CmfPlatformBriefingService.AnalysisPrompt(splitSource).Contains("omit unique/duplicate/LOS unless material to the observation")
            && CmfPlatformBriefingService.AnalysisPrompt(splitSource).Contains("splits remain available, not mandatory")
            && CmfPlatformBriefingService.AnalysisPrompt(splitSource).Contains("use ingredient totals across iDST rows")
            && CmfPlatformBriefingService.VerificationPrompt(splitSource, new List<CmfPlatformBriefingFinding>())
                .Contains("needs no unique/duplicate/LOS detail unless its claim depends on the split; do not reject its omission")
            && CmfPlatformBriefingService.VerificationPrompt(splitSource, new List<CmfPlatformBriefingFinding>())
                .Contains("Pending ingredient findings retain material leading ingredient counts"),
            "Generation/review still mandate Open split details or lose the separate Pending ingredient policy.");
        splitSnapshot.Data["PendingSummary"] = serializer.DeserializeObject(@"{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],""Rows"":[
            [""HW"",""Unassigned"",""2:::OVER_TWO_DAYS:::1""],[""HW"",""Owner"",""3""],[""Memory"",""Owner"",""1:::OVER_TWO_DAYS:::1""]]}");
        Check(!CmfPlatformBriefingService.BuildSource(splitSnapshot).Evidence.Single(item => item.Subject == "HW ingredient total")
            .Measurements.Any(item => item.Name == "Aged pending" || item.Name == "Pending age threshold"),
            "Incomplete per-ingredient aging data was presented as a complete subtotal.");
        snapshot.ShowComparisons = false;
        Check(!CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Single(item => item.Section == "CMF Summary").Measurements.Single().Previous.HasValue,
            "Hidden KPI comparison leaked into the briefing.");
        string sameDataHash = CmfPlatformBriefingService.BuildSource(snapshot).Hash;
        snapshot.CapturedAt = snapshot.CapturedAt.AddMinutes(10);
        Check(CmfPlatformBriefingService.BuildSource(snapshot).Hash == sameDataHash, "An unchanged page reload restarted platform analysis solely because its capture time changed.");
        snapshot.Configuration = "changed-fixture";
        Check(CmfPlatformBriefingService.BuildSource(snapshot).Hash != sameDataHash, "Changed dashboard configuration reused an unrelated briefing.");
        snapshot.Configuration = "fixture";
        snapshot.CapturedAt = snapshot.CapturedAt.AddDays(1);
        Check(CmfPlatformBriefingService.BuildSource(snapshot).Hash != sameDataHash, "A new calendar day reused stale dated briefing evidence.");
        snapshot.Data["SummaryFacts"] = serializer.DeserializeObject(@"[
            {""Label"":""Total CMFs"",""Value"":""111""},
            {""Label"":""Closed"",""Value"":""81""},
            {""Label"":""Open"",""Value"":""13""},
            {""Label"":""Implemented"",""Value"":""17""},
            {""Label"":""Pending"",""Value"":""6""}]");
        var populations = CmfPlatformBriefingService.BuildSource(snapshot);
        var closed = populations.Evidence.Single(item => item.Subject == "Closed");
        Check(closed.Measurements.Single().Denominator == 111 && closed.Context["Status definition"].Contains("subset of Total CMFs"),
            "The same-query Closed/Total population relationship was lost from AI evidence.");
        Check(populations.Evidence.Single(item => item.Subject == "Pending").Context["Population"].Contains("not a disjoint status bucket")
            && !populations.Evidence.Single(item => item.Subject == "Pending").Measurements.Single().Denominator.HasValue
            && populations.Evidence.Single(item => item.Subject == "Open").Measurements.Single().Denominator == 111
            && populations.Evidence.Single(item => item.Subject == "Open").Context["Status definition"].Contains("includes duplicates")
            && populations.Evidence.Single(item => item.Subject == "Open").Context["Status definition"].Contains("Not interchangeable"),
            "Open lost its accepted-summary denominator or Pending was incorrectly merged with accepted/unique-only populations.");
        string closure = serializer.Serialize(new { findings = new[] { new { title = "CMF closure", summary = "81 of 111 CMFs are closed.",
            kind = "pattern", tone = "neutral", section = "highlights", evidenceIds = new[] { closed.Id },
            support = new[] { new { evidenceId = closed.Id, measurementNames = new[] { "Closed" },
                explanation = "The closed count is a majority of the accepted population and explicitly includes duplicates." } } } } });
        var closureFindings = CmfPlatformBriefingService.Validate(populations, closure);
        Check(closureFindings.Count == 1 && CmfPlatformBriefingService.VerificationPrompt(populations, closureFindings).Contains("not an additional disjoint status bucket"),
            "A valid closure share was missing its reviewer population guidance.");
        snapshot.Data["DesignSummary"] = serializer.DeserializeObject(@"{""Columns"":[""Design Name"",""SW Image Freeze"",""Support Model"",""QS"",""QSHF"",""Impl/Verified""],
            ""Rows"":[[""Design A"",""NA"","""",""1"",""1"",""0""],[""Design B"",""NA"","""",""0"",""0"",""3""],[""Design C"",""NA"","""",""0"",""0"",""0""],[""Total"","""","""",""1"",""1"",""3""]]}");
        var breadth = CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Single(item => item.Section == "DesignExposure");
        Check(breadth.Measurements.Single(item => item.Name == "Designs in scope").Current == 3
            && breadth.Measurements.Single(item => item.Name == "Designs with Open records").Current == 1
            && breadth.Measurements.Single(item => item.Name == "Designs without Open records").Current == 2
            && breadth.Measurements.Single(item => item.Name == "Designs with at most one Open record").Current == 2
            && breadth.Measurements.Single(item => item.Name == "Designs with at most one Open record").Denominator == 3
            && breadth.Measurements.Single(item => item.Name == "Designs with exactly one Open record").Current == 0
            && breadth.Measurements.Single(item => item.Name == "Designs with more than one Open record").Current == 1
            && breadth.Measurements.Single(item => item.Name == "Open records in design table").Current == 2
            && breadth.Measurements.Single(item => item.Name == "Maximum Open records per design").Current == 2,
            "Design breadth lost zero rows, included Total, or counted implemented records as Open.");
        var originalDesignSummary = snapshot.Data["DesignSummary"];
        snapshot.Data["DesignSummary"] = serializer.DeserializeObject(@"{""Columns"":[""Design Name"",""SW Image Freeze"",""Support Model"",""QS"",""Impl/Verified""],
            ""Rows"":[[""Design A"",""NA"","""",""2"",""0""],[""Design B"",""NA"","""",""1"",""4""],[""Design C"",""NA"","""",""0"",""0""],[""Total"","""","""",""3"",""4""]]}");
        var mixedBreadth = CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Single(item => item.Section == "DesignExposure");
        Check(mixedBreadth.Measurements.Single(item => item.Name == "Designs without Open records").Current == 1
            && mixedBreadth.Measurements.Single(item => item.Name == "Designs with exactly one Open record").Current == 1
            && mixedBreadth.Measurements.Single(item => item.Name == "Designs with at most one Open record").Current == 2
            && mixedBreadth.Measurements.Single(item => item.Name == "Designs with at most one Open record").Denominator == 3
            && mixedBreadth.Measurements.Single(item => item.Name == "Designs with more than one Open record").Current == 1,
            "Design coverage conflates zero, one and multiple Open records or counts implemented/Total records as designs.");
        snapshot.Data["DesignSummary"] = originalDesignSummary;
        snapshot.Data["CustomerIssueFacts"] = serializer.DeserializeObject(@"[
            {""RecordKey"":""NVL:16011111111"",""Population"":""Pending"",""SourcePlatform"":""NVL"",""Customer"":""Lenovo"",""Design"":""Design A"",""Title"":""System hangs with MCA errors""},
            {""RecordKey"":""NVL:16011111111"",""Population"":""Pending"",""SourcePlatform"":""NVL"",""Customer"":""Lenovo"",""Design"":""Design A"",""Title"":""System hangs with MCA errors""},
            {""RecordKey"":""NVL:16022222222"",""Population"":""Open"",""SourcePlatform"":""NVL"",""Customer"":""HP"",""Title"":""100% display blanking""},
            {""RecordKey"":""NVL:16033333333"",""Population"":""Pending"",""Customer"":"""",""Title"":""Unknown customer issue""},
            {""RecordKey"":""NVL:16044444444"",""Population"":""Pending"",""Customer"":""ASUS"",""Title"":""ALS failure""},
            {""RecordKey"":""NVL:16044444444"",""Population"":""Pending"",""Customer"":""Dell"",""Title"":""Conflicting metadata""},
            {""RecordKey"":""NVL:16055555555"",""Population"":""Closed"",""Customer"":""ASUS"",""Title"":""Closed ALS failure""}]");
        var customers = CmfPlatformBriefingService.BuildSource(snapshot);
        Check(customers.Evidence.Count(item => item.Section == "CustomerIssues") == 2
            && customers.Evidence.Single(item => item.Subject == "Pending customer description coverage").Measurements
                .Single(item => item.Name == "Scoped issue records").Current == 3,
            "Customer evidence duplicated keys, combined populations or accepted conflicting/missing metadata.");
        Check(!serializer.Serialize(customers).Contains("16011111111") && !serializer.Serialize(customers).Contains("Closed ALS failure")
            && customers.Evidence.Single(item => item.Section == "CustomerIssues" && item.Context["Issue population"] == "Pending")
                .Context["Customer"] == "Lenovo", "Customer source leaked issue IDs or inferred a customer from unsupported metadata.");
        var reported = customers.Evidence.Single(item => item.Section == "CustomerIssues" && item.Context["Issue population"] == "Pending");
        var customerOutput = serializer.Serialize(new { findings = new[] { new { title = "Reported customer exposure",
            summary = "Pending issues include Lenovo system hangs with MCA errors.", kind = "pattern", tone = "attention", section = "focus",
            evidenceIds = new[] { reported.Id }, support = new[] { new { evidenceId = reported.Id,
                measurementNames = new[] { "Reported issue record" }, explanation = "The scoped pending record explicitly identifies Lenovo and reports system hangs with MCA errors." } } } } });
        var customerFindings = CmfPlatformBriefingService.Validate(customers, customerOutput);
        Check(customerFindings.Single().Evidence.Single().Text.Contains("Reported issue title: System hangs with MCA errors")
            && CmfPlatformBriefingService.VerificationPrompt(customers, customerFindings).Contains("same selected CustomerIssues row"),
            "Reported customer briefing lost exact source attribution or independent review guidance.");
        bool wrongCustomerSection = false;
        try { CmfPlatformBriefingService.Validate(customers, customerOutput.Replace("\"attention\"", "\"neutral\"").Replace("\"focus\"", "\"highlights\"")); }
        catch (InvalidOperationException) { wrongCustomerSection = true; }
        Check(wrongCustomerSection, "Customer exposure was incorrectly treated as favorable performance.");
        var opened = customers.Evidence.Single(item => item.Section == "CustomerIssues" && item.Context["Issue population"] == "Open");
        Check(opened.Context["Reported issue title"] == "display blanking" && !serializer.Serialize(customers).Contains("100%"),
            "Source title percentages leaked into count-only customer backing data.");
        bool numericCustomer = false;
        try { CmfPlatformBriefingService.Validate(customers, customerOutput.Replace("Pending issues include", "14 Pending issues include")); }
        catch (InvalidOperationException) { numericCustomer = true; }
        Check(numericCustomer, "Customer briefing invented a numeric issue total.");
        string mixed = serializer.Serialize(new { findings = new[] { new { title = "Reported customer exposure",
            summary = "Pending issues include Lenovo hangs and HP display blanking.", kind = "pattern", tone = "attention", section = "focus",
            evidenceIds = new[] { reported.Id, opened.Id }, support = new[] {
                new { evidenceId = reported.Id, measurementNames = new[] { "Reported issue record" }, explanation = "Lenovo reports pending system hangs." },
                new { evidenceId = opened.Id, measurementNames = new[] { "Reported issue record" }, explanation = "HP reports Open display blanking." } } } } });
        bool mixedRejected = false;
        try { CmfPlatformBriefingService.Validate(customers, mixed); }
        catch (InvalidOperationException) { mixedRejected = true; }
        Check(mixedRejected, "Open symptoms were incorrectly assigned to the Pending population.");
        Check(CmfPlatformBriefingService.AnalysisPrompt(customers).Contains("Oldest pending age")
            && CmfPlatformBriefingService.AnalysisPrompt(customers).Contains("DesignExposure: breadth/maxima")
            && CmfPlatformBriefingService.AnalysisPrompt(customers).Contains("Open records across milestones"),
            "New exposure and severity evidence was not explained to the model.");
        Console.WriteLine("Platform briefing projection checks passed.");
    }

    public static void CheckOverallBriefingProjection()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var snapshot = new CmfPanelSnapshotCache { Platform = CmfPlatformCatalog.OverallPlatformTableName,
            CapturedAt = new DateTime(2026, 11, 20), Configuration = "overall-fixture", DateScope = "all", ShowComparisons = true,
            Data = serializer.Deserialize<Dictionary<string, object>>(@"{""PlatformLabel"":""Overall Dashboard"",
                ""SummaryFacts"":[{""Label"":""Total CMFs"",""Value"":""40""},{""Label"":""Closed"",""Value"":""25""}],
                ""TptFacts"":[{""Label"":""Overall TPT"",""Value"":""9""}],
                ""PendingAgeBuckets"":[{""Name"":""0-1 Days"",""Value"":1},{""Name"":""1-2 Days"",""Value"":2},{""Name"":""2-4 Days"",""Value"":0},{""Name"":""> 4 Days"",""Value"":1}],
                ""PendingSummaryAgeThresholdDays"":2,
                ""PendingSummary"":{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],""Rows"":[[""HW"",""Unassigned"",""4:::OVER_TWO_DAYS:::3:::OPEN_HSDS:::16011111111""]]},
                ""OemSummary"":[
                    {""OemName"":""OEM A"",""TotalIssues"":20,""Closed"":15,""Implemented"":3,""OpenIssues"":2,""CmfPending"":4,""DuplicateTotalIssues"":4,""DuplicateClosed"":2,""DuplicateImplemented"":1,""DuplicateOpenIssues"":1,""AvgResolutionDays"":1.25,""RecentRequestCount"":9999,""RiskLevel"":""SECRET_RISK""},
                    {""OemName"":""OEM B"",""TotalIssues"":10,""Closed"":5,""Implemented"":3,""OpenIssues"":2,""CmfPending"":0,""DuplicateTotalIssues"":6,""DuplicateClosed"":3,""DuplicateImplemented"":2,""DuplicateOpenIssues"":1,""AvgResolutionDays"":0}],
                ""HeatmapPlatforms"":[""NVL H"",""PTL""],
                ""ComponentRiskHeatmap"":[
                    {""Ingredient"":""HW"",""Values"":[3,2],""DuplicateValues"":[1,0],""StatusBreakdowns"":[""Open Unique: 3\nLOS: 1"",""Open Unique: 2\nLOS: 0""],""DuplicateStatusBreakdowns"":[""Open Duplicates: 1\nLOS: 1"",""No issues""]},
                    {""Ingredient"":""GFX"",""Values"":[0,1],""DuplicateValues"":[0,1],""StatusBreakdowns"":[""No issues"",""Open Unique: 1\nLOS: 1""],""DuplicateStatusBreakdowns"":[""No issues"",""Open Duplicates: 1\nLOS: 0""]}],
                ""Trend"":[{""NewIssues"":9999}],""PredictedBlockers"":[""SECRET_PREDICTION""]}") };
        snapshot.Data["CustomerIssueFacts"] = serializer.DeserializeObject(@"[
            {""RecordKey"":""NVL:customer-open"",""Population"":""Open"",""Customer"":""Customer A"",""Title"":""Display blanking""},
            {""RecordKey"":""PTL:customer-pending"",""Population"":""Pending"",""Customer"":""Customer B"",""Title"":""System hangs""}]");
        var source = CmfPlatformBriefingService.BuildSource(snapshot);
        Check(source.Platform == snapshot.Platform && source.Scope == "Overall Dashboard", "Overall briefing used individual-platform scope.");
        Check(!source.Evidence.Any(item => item.Section == "CustomerIssues" || item.Section == "CustomerIssueCoverage"),
            "Overall still spends analysis/review requests on excluded Customer Issues evidence.");
        var age = source.Evidence.Single(item => item.Section == "Pending Age" && item.Subject == "1-2 Days");
        Check(age.Context["Age boundary"] == "Exactly 2 days" && age.Measurements.Single(item => item.Unit == "records").Current == 2
            && age.Measurements.Single(item => item.Unit == "records").Denominator == 4
            && age.Measurements.Any(item => item.Name == "Minimum age" && item.Unit == "days" && item.Current == 2)
            && age.Context["Population"].Contains("not necessarily the entire pending queue"),
            "Overall pending ages lost exact bucket boundaries or their dated-only population.");
        var oem = source.Evidence.Single(item => item.Section == "OEM Summary" && item.Subject == "OEM A");
        Check(oem.Measurements.Single(item => item.Name == "Total unique CMFs").Current == 20
            && oem.Measurements.Single(item => item.Name == "Total unique CMFs").Denominator == 30
            && oem.Measurements.Single(item => item.Name == "Total duplicates").Denominator == 10, "OEM unique/duplicate column populations were conflated.");
        Check(oem.Measurements.Single(item => item.Name == "Resolution TPT").Current == 1.3m
            && !source.Evidence.Single(item => item.Subject == "OEM B").Measurements.Any(item => item.Unit == "days"), "OEM TPT rounding or missing-duration handling changed.");
        var cell = source.Evidence.Single(item => item.Subject == "HW / NVL H");
        Check(cell.Measurements.Single(item => item.Name == "Open unique CMFs").Current == 3
            && cell.Measurements.Single(item => item.Name == "Open unique CMFs").Denominator == 6
            && cell.Measurements.Single(item => item.Name == "Open unique LOS subset").Current == 1
            && cell.Measurements.Single(item => item.Name == "Open duplicates").Denominator == 2, "Overall heatmap lost counts, LOS subsets or matrix denominators.");
        Check(source.Evidence.Single(item => item.Section == "Open CMFs by platform" && item.Subject == "PTL")
            .Measurements.Single(item => item.Name == "Open unique CMFs").Current == 3,
            "Overall platform comparisons did not use exact sums of their own heatmap cells.");
        var overallIngredient = source.Evidence.Single(item => item.Section == "Open CMFs by ingredient" && item.Subject == "HW");
        Check(overallIngredient.Measurements.Single(item => item.Name == "Open records including duplicates").Current == 6
            && overallIngredient.Measurements.Single(item => item.Name == "Open records including duplicates").Denominator == 8
            && source.Evidence.Single(item => item.Section == "Open CMFs by platform" && item.Subject == "PTL")
                .Measurements.Single(item => item.Name == "Open records including duplicates").Current == 4,
            "Overall combined Open counts double-counted LOS or mixed matrix and headline populations.");
        string overallIngredientOutput = serializer.Serialize(new { findings = new[] { new {
            title = "Leading open-CMF ingredient", summary = "HW has 6 of 8 open records in the Overall heatmap.",
            kind = "cluster", tone = "neutral", section = "highlights", evidenceIds = new[] { overallIngredient.Id },
            support = new[] { new { evidenceId = overallIngredient.Id, measurementNames = new[] { "Open records including duplicates" },
                explanation = "HW has the highest combined Open count in the complete ingredient heatmap, without adding LOS subsets." } } } } });
        var overallIngredientFinding = CmfPlatformBriefingService.Validate(source, overallIngredientOutput).Single();
        Check(overallIngredientFinding.Evidence.Single().Text.Contains("Field Open records including duplicates: 6 records")
            && overallIngredientFinding.Evidence.Single().Text.Contains("not interchangeable with headline or OEM Open counts")
            && !overallIngredientFinding.Evidence.Single().Text.Contains("Field Open unique CMFs")
            && !overallIngredientFinding.Evidence.Single().Text.Contains("Field Open duplicate LOS subset"),
            "Overall did not retain concise total-only ingredient Sources and their distinct population.");
        string overallWriting = CmfPlatformBriefingService.AnalysisPrompt(source);
        string overallReview = CmfPlatformBriefingService.VerificationPrompt(source, new List<CmfPlatformBriefingFinding>());
        Check(overallWriting.Contains("OVERALL CUSTOMER EXCLUSION") && overallReview.Contains("OVERALL CUSTOMER EXCLUSION")
            && !overallWriting.Contains("REPORTED CUSTOMER EXPOSURE") && !overallReview.Contains("REPORTED CUSTOMER EXPOSURE"),
            "Overall still sends excluded customer-generation or customer-review instructions to the provider.");
        Check(overallWriting.Contains("SINGLE-PROMINENT-ITEM") && overallReview.Contains("SINGLE-PROMINENT-ITEM")
            && overallWriting.Contains("NO target, minimum, top-N or maximum number of findings")
            && overallWriting.Contains("one separate assessment per qualifying measure")
            && overallWriting.Contains("separate material aging and unassigned-iDST observations")
            && overallWriting.Contains("splits remain available, not mandatory")
            && overallReview.Contains("do not reject its omission")
            && overallWriting.Contains("CustomerIssues uses issue-based bullets")
            && overallWriting.Contains("omit oldest age from bullets and selected Sources")
            && overallReview.Contains("NO STANDALONE LOS"),
            "Overall lost applicable platform Summary, optional-split, pending, TPT or Customer Issues rules.");
        string json = serializer.Serialize(source);
        Check(!json.Contains("9999") && !json.Contains("SECRET") && !json.Contains("16011111111"), "Hidden predictions, historical activity or issue IDs leaked into Overall evidence.");
        Check(source.Evidence.Single(item => item.Subject == "Closed").Measurements.Single().Denominator == 40, "Overall headline closure lost its own denominator.");
        Check(CmfPlatformBriefingService.AnalysisPrompt(source).Contains("Overall dashboard's own metrics"), "Overall did not receive independent Overall instructions.");
        var yesterday = serializer.Deserialize<CmfPlatformBriefingSource>(serializer.Serialize(source));
        yesterday.CapturedAt = snapshot.CapturedAt.AddDays(-1).ToString("o");
        foreach (var measurement in yesterday.Evidence.SelectMany(item => item.Measurements)) measurement.CurrentDate = "2026-11-19";
        yesterday.Evidence.Single(item => item.Subject == "Total CMFs").Measurements.Single().Current = 30;
        Check(CmfPlatformBriefingService.BuildSource(snapshot, yesterday).Evidence.Single(item => item.Subject == "Total CMFs").Measurements.Single().Previous == 30,
            "Matching yesterday Overall evidence was not compared.");
        yesterday.Platform = "CMF_NVL_H_ALL_COMPONENTS_TABLE";
        Check(!CmfPlatformBriefingService.BuildSource(snapshot, yesterday).Evidence.Any(item => item.Kind == "comparison"), "An individual-platform baseline leaked into Overall comparisons.");
        snapshot.Configuration = "other-membership";
        Check(CmfPlatformBriefingService.BuildSource(snapshot).Hash != source.Hash, "Overall membership changes reused cached findings.");
        snapshot.Data["ComponentRiskHeatmap"] = serializer.DeserializeObject(@"[{""Ingredient"":""HW"",""Values"":[3],""DuplicateValues"":[1]}]");
        Check(!CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Any(item => item.Section == "Open CMF Heatmap"), "Incomplete heatmap invented a denominator.");
        snapshot.Data.Clear();
        Check(CmfPlatformBriefingService.Get(CmfPlatformBriefingService.BuildSource(snapshot), false).Pipeline == CmfPlatformBriefingService.OverallPipeline,
            "Overall responses were labeled as individual-platform findings.");
        Console.WriteLine("Overall own-snapshot briefing projection checks passed.");
    }

    public static void CheckOverallBriefingBatching()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = 1024 * 1024 };
        var source = new CmfPlatformBriefingSource { Platform = CmfPlatformCatalog.OverallPlatformTableName, Scope = "Overall",
            Hash = "overall-batches", DateScope = "all", CapturedAt = "2026-11-20", Evidence = Enumerable.Range(1, 160)
                .Select(index => new CmfPlatformBriefingEvidence { Id = "e" + index, Section = index <= 100 ? "MilestoneSummary" : "OEM Summary",
                    Subject = "Row " + index, Kind = "distribution", Context = new Dictionary<string, string> { { "Population", new string('x', 600) } },
                    Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement { Name = "CMFs", Current = 1,
                        Denominator = index <= 100 ? 100 : 60, Unit = "records", CurrentDate = "2026-11-20" } } }).ToList() };
        source.Evidence[0].Context["Population"] = "Different scoped population";
        var sections = new Dictionary<string, int>();
        Check(CmfPlatformBriefingService.AnalysisPrompt(source).Contains("Current aged or unassigned pending exposure requires at least two assessments"),
            "Overall generation must omit insignificant lone-assessment exposure rather than rely only on final rejection.");
        var evidenceText = typeof(CmfPlatformBriefingService).GetMethod("EvidenceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        foreach (string section in new[] { "Pending Age", "OEM Summary", "Open CMF Heatmap", "Open CMFs by platform", "Open CMFs by ingredient" })
        {
            var row = new CmfPlatformBriefingEvidence { Section = section, Subject = "Test row", Context = new Dictionary<string, string>(),
                Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement { Name = "Count", Current = 2, Denominator = 4,
                    Unit = "records", CurrentDate = "2026-11-20" } } };
            string text = (string)evidenceText.Invoke(null, new object[] { source, row, "The selected count establishes this row's share of its own population." });
            Check(!text.Contains("in the accepted CMF population, including duplicates")
                && text.Contains(section == "Pending Age" ? "dated pending-assessment age-chart population"
                    : section == "OEM Summary" ? "across OEMs in this same field" : "full open-CMF ingredient/platform matrix"),
                "Overall Source denominator must retain the exact " + section + " population.");
        }
        int generations = 0, reviews = 0;
        var findings = CmfPlatformBriefingService.Generate(source, prompt => {
            generations++;
            var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
            var defaults = (Dictionary<string, object>)packet["SectionContexts"];
            foreach (Dictionary<string, object> row in (object[])packet["Evidence"])
            {
                var inherited = new Dictionary<string, object>((Dictionary<string, object>)defaults[(string)row["Section"]]);
                foreach (var context in (Dictionary<string, object>)row["Context"]) inherited[context.Key] = context.Value;
                var original = source.Evidence.Single(item => item.Id == (string)row["Id"]);
                Check(inherited.Count == original.Context.Count && original.Context.All(item => inherited[item.Key].ToString() == item.Value),
                    "Compaction changed an Overall population or failed to retain a row-specific context override.");
                var expectedMeasurements = original.Measurements.Select(measurement => new object[] { measurement.Name, measurement.Current,
                    measurement.Previous, measurement.Denominator, measurement.Unit, measurement.CurrentDate, measurement.PreviousDate, measurement.PreviousIsEstimate });
                Check(serializer.Serialize(row["Measurements"]) == serializer.Serialize(expectedMeasurements),
                    "Overall compaction lost or changed measurement values, units, dates or unavailable fields.");
            }
            foreach (var group in ((object[])packet["Evidence"]).Cast<Dictionary<string, object>>().GroupBy(row => (string)row["Section"]))
            {
                Check(!sections.ContainsKey(group.Key), "An Overall table was split between generation/review batches.");
                sections[group.Key] = group.Count();
            }
            return "{\"findings\":[]}";
        }, prompt => {
            reviews++;
            Check(prompt.Contains("Each Evidence.Context inherits SectionContexts[Section]") && prompt.Contains("Different scoped population"),
                "Independent review lost shared context decoding or specific population overrides.");
            return "{\"complete\":true,\"checks\":[]}";
        }, prompt => {
            throw new Exception("Empty approved batches must not request ranking.");
        }, value => {});
        Check(generations == 1 && reviews == 1 && findings.Count == 0
            && sections["MilestoneSummary"] == 100 && sections["OEM Summary"] == 60,
            "Repeated Overall context still forces extra calls, or complete tables/independent review were lost.");
        foreach (var row in source.Evidence) row.Context["Population"] = new string('x', 21000);
        generations = 0;
        CmfPlatformBriefingService.Generate(source, prompt => { generations++; return "{\"findings\":[]}"; },
            prompt => "{\"complete\":true,\"checks\":[]}", prompt => "");
        Check(generations == 2, "Overall batch sizing ignored the shared context size and combined oversized tables.");
        Console.WriteLine("Overall complete-table batching checks passed.");
    }

    public static void CheckPlatformBriefingPayload()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var source = new CmfPlatformBriefingSource { Platform = "NVL_H", Scope = "NVL H", Hash = "fixture", DateScope = "all",
            CapturedAt = "2026-11-19", Evidence = Enumerable.Range(1, 35).Select(index => new CmfPlatformBriefingEvidence {
                Id = "e" + index, Section = "ComponentSummary", Subject = "Ingredient " + index, Kind = "distribution",
                Context = new Dictionary<string, string> { { "Population", "Unique records" } },
                Measurements = Enumerable.Range(0, 8).Select(metric => new CmfFactMeasurement { Name = "Metric " + metric,
                    Current = 12, Previous = 8, Denominator = 20, Unit = "records", CurrentDate = "2026-11-19",
                    PreviousDate = "2026-11-12", PreviousIsEstimate = true }).ToList() }).ToList() };
        string prompt = CmfPlatformBriefingService.AnalysisPrompt(source);
        var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf("\n", StringComparison.Ordinal) + 1));
        Check(serializer.Serialize(packet["MeasurementColumns"]) == serializer.Serialize(new[] {
            "Name", "Current", "Previous", "Denominator", "Unit", "CurrentDate", "PreviousDate", "PreviousIsEstimate" }), "Compact evidence lost its measurement schema.");
        var records = (object[])packet["Evidence"];
        Check(records.Length == 35, "Compact evidence dropped table rows.");
        foreach (Dictionary<string, object> record in records)
        {
            var measurements = (object[])record["Measurements"];
            Check(measurements.Length == 8 && ((Dictionary<string, object>)record["Context"])["Population"].ToString() == "Unique records", "Compact evidence dropped columns or population context.");
            for (int metric = 0; metric < measurements.Length; metric++)
                Check(serializer.Serialize(measurements[metric]) == serializer.Serialize(new object[] { "Metric " + metric, 12, 8, 20, "records", "2026-11-19", "2026-11-12", true }),
                    "Compact evidence changed a value, unit, date, denominator or estimate qualification.");
        }
        int generations = 0, verifications = 0;
        CmfPlatformBriefingService.Generate(source, input => { generations++; return "{\"findings\":[]}"; }, input => {
            verifications++;
            Check(input.Contains("\"MeasurementColumns\"") && input.Contains("\"Id\":\"e35\""), "Verifier did not receive complete compact evidence.");
            return "{\"complete\":true,\"checks\":[]}";
        }, input => { throw new Exception("Compact single batch unnecessarily requested global ranking."); });
        Check(serializer.Serialize(source.Evidence).Length > 40000 && prompt.Length < 40000 && generations == 1 && verifications == 1,
            "Repeated measurement field names still force unnecessary model batches.");
        Check(prompt.Contains("Do not expand abbreviations or decode identifier tokens"),
            "Customer title abbreviations can be expanded beyond the supplied evidence.");
        var grouped = new CmfPlatformBriefingSource { Platform = source.Platform, Scope = source.Scope,
            CapturedAt = source.CapturedAt, DateScope = source.DateScope, Hash = "customer-batches", Evidence = source.Evidence.ToList() };
        foreach (string population in new[] { "Open", "Pending" })
        for (int record = 1; record <= 2; record++)
            grouped.Evidence.Add(new CmfPlatformBriefingEvidence { Id = "customer-" + population + "-" + record,
                Section = "CustomerIssues", Subject = "Reported customer issue", Kind = "current",
                Context = new Dictionary<string, string> { { "Issue population", population }, { "Reported issue title", new string('x', 16000) } },
                Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement { Name = "Reported issue record", Current = 1, Unit = "records", CurrentDate = "2026-11-19" } } });
        int groupedCalls = 0;
        CmfPlatformBriefingService.Generate(grouped, input => {
            groupedCalls++;
            foreach (string population in new[] { "Open", "Pending" })
                Check(input.Contains("customer-" + population + "-1") == input.Contains("customer-" + population + "-2"),
                    "One customer population was split into independent, overlapping briefings.");
            return "{\"findings\":[]}";
        }, input => "{\"complete\":true,\"checks\":[]}", input => { throw new Exception("Empty customer fixtures requested ranking."); });
        Check(groupedCalls == 3, "Atomic customer populations were truncated or combined beyond the batch budget.");
        Console.WriteLine("Platform briefing compact payload checks passed: " + serializer.Serialize(source.Evidence).Length + " original evidence characters, " + prompt.Length + " full prompt characters; one analysis and one verification.");
    }

    public static void CheckPlatformBriefingProviderFailures()
    {
        Check(CmfPlatformBriefingService.ProviderFailureCode("Dashboard AI request timed out.") == "provider-timeout", "Provider timeout was hidden by a generic failure.");
        Check(CmfPlatformBriefingService.ProviderFailureCode("HTTP 429. Bounded retries exhausted.") == "provider-rate-limited", "Provider throttling was hidden.");
        Check(CmfPlatformBriefingService.ProviderFailureCode("HTTP 403") == "provider-authorization"
            && CmfPlatformBriefingService.ProviderFailureCode("No AI credentials.") == "provider-authorization", "Provider authentication failure was hidden.");
        Check(CmfPlatformBriefingService.ProviderFailureCode("Model response was empty.") == "provider-empty", "Empty provider output was not classified.");
        Check(CmfPlatformBriefingService.ProviderFailureCode("Untrusted provider response with private details") == "provider-unavailable"
            && CmfPlatformBriefingService.ProviderFailureCode(null) == "provider-unavailable", "Raw provider details leaked into public diagnostics.");
        Console.WriteLine("Platform briefing safe provider diagnostics checks passed.");
    }

    public static void CheckPlatformBriefingLatency()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var source = new CmfPlatformBriefingSource { Platform = "NVL_H", Scope = "NVL H", Hash = "latency-fixture", DateScope = "all",
            CapturedAt = "2026-11-23", Evidence = Enumerable.Range(1, 2).Select(index => new CmfPlatformBriefingEvidence {
                Id = "e" + index, Section = "ComponentSummary", Subject = "Ingredient " + index, Kind = "distribution",
                Context = new Dictionary<string, string> { { "Population", new string('x', 25000) } },
                Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement {
                    Name = "Open", Current = 12, Denominator = 20, Unit = "records", CurrentDate = "2026-11-23" } } }).ToList() };
        int generations = 0, reviews = 0;
        var findings = CmfPlatformBriefingService.Generate(source, prompt => {
            generations++;
            string id = prompt.Contains("\"Id\":\"e1\"") ? "e1" : "e2";
            return serializer.Serialize(new { findings = new[] { new {
                title = "Ingredient " + id + " backlog", summary = "This ingredient has 12 of 20 Open records.",
                kind = "cluster", tone = "neutral", section = "highlights", evidenceIds = new[] { id },
                support = new[] { new { evidenceId = id, measurementNames = new[] { "Open" },
                    explanation = "This ingredient contains a majority of the same-field Open population." } } } } });
        }, prompt => {
            reviews++;
            var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
            var skeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
            skeleton["complete"] = true;
            foreach (Dictionary<string, object> check in (object[])skeleton["checks"])
            {
                foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons", "significance" }) check[flag] = true;
                check["reason"] = "";
            }
            return serializer.Serialize(skeleton);
        }, prompt => { throw new Exception("Already ranked and independently verified batches made another ranking-only request."); });
        Check(generations == 2 && reviews == 2 && findings.Count == 2 && findings[0].Id == "f1" && findings[1].Id == "f2",
            "Faster batching dropped a finding, changed AI batch order or skipped independent review.");
        DateTime now = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);
        var saved = new CmfPlatformBriefing { Success = true, Mode = "ai", Stage = "complete", Pipeline = CmfPlatformBriefingService.Pipeline,
            Scope = source.Scope, SourceHash = source.Hash, Findings = findings, ExpiresAt = now.AddMinutes(30).ToString("o") };
        Check(CmfPlatformBriefingService.IsReusableResult(source, saved, now), "A fresh approved result cannot be reused after restart.");
        foreach (string mode in new[] { "ai-loading", "ai-unavailable", "ai-review", "unknown" })
        {
            saved.Mode = mode;
            Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now), "An unapproved result was reused as completed insights.");
        }
        saved.Mode = "ai";
        Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now.AddMinutes(31)), "An expired result was reused.");
        saved.SourceHash = "changed";
        Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now), "Changed dashboard evidence reused stale insights.");
        saved.SourceHash = source.Hash;
        saved.Scope = "other-scope";
        Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now), "A different dashboard scope reused cached findings.");
        saved.Scope = source.Scope;
        saved.Stage = "verify";
        Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now), "An incomplete review was reused.");
        saved.Stage = "complete";
        saved.Pipeline = CmfPlatformBriefingService.OverallPipeline;
        Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now), "Overall results were reused for a platform.");
        source.Platform = CmfPlatformCatalog.OverallPlatformTableName;
        Check(CmfPlatformBriefingService.IsReusableResult(source, saved, now), "Overall approved results cannot be reused.");
        saved.Success = false;
        Check(!CmfPlatformBriefingService.IsReusableResult(source, saved, now), "A failed result was reused.");
        Console.WriteLine("Briefing latency checks passed: four required calls for two complete verified batches, no ranking-only call; approved cache reuse remains scoped and expiring.");
    }

    public static void CheckOverallBriefingResume()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var source = new CmfPlatformBriefingSource { Platform = CmfPlatformCatalog.OverallPlatformTableName, Scope = "Overall", Hash = "resume-fixture",
            DateScope = "all", CapturedAt = "2026-11-23", Evidence = Enumerable.Range(1, 2).Select(index => new CmfPlatformBriefingEvidence {
                Id = "e" + index, Section = index == 1 ? "ComponentSummary" : "DesignSummary", Subject = "Group " + index, Kind = "distribution",
                Context = new Dictionary<string, string> { { "Population", new string('x', 25000) } },
                Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement { Name = "Open", Current = 12,
                    Denominator = 20, Unit = "records", CurrentDate = "2026-11-23" } } }).ToList() };
        var saved = new Dictionary<string, List<CmfPlatformBriefingFinding>>();
        int generated = 0, reviewed = 0;
        bool failSecond = true;
        Func<string, string> generate = prompt => {
            generated++;
            string id = prompt.Contains("\"Id\":\"e1\"") ? "e1" : "e2";
            if (id == "e2" && failSecond) throw new ApplicationException("Temporary provider failure.");
            return serializer.Serialize(new { findings = new[] { new { title = "Backlog " + id, summary = "This group has 12 of 20 Open records.",
                kind = "cluster", tone = "attention", section = "focus", evidenceIds = new[] { id }, support = new[] { new {
                    evidenceId = id, measurementNames = new[] { "Open" }, explanation = "The group holds a majority of its supplied Open population." } } } } });
        };
        Func<string, string> verify = prompt => {
            reviewed++;
            var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
            var skeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
            skeleton["complete"] = true;
            foreach (Dictionary<string, object> check in (object[])skeleton["checks"])
            {
                foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons", "significance" }) check[flag] = true;
                check["reason"] = "";
            }
            return serializer.Serialize(skeleton);
        };
        Func<CmfPlatformBriefingSource, List<CmfPlatformBriefingFinding>> read = part => saved.ContainsKey(part.Evidence[0].Id) ? saved[part.Evidence[0].Id] : null;
        Action<CmfPlatformBriefingSource, List<CmfPlatformBriefingFinding>> write = (part, findings) => saved[part.Evidence[0].Id] = findings;
        bool failed = false;
        try { CmfPlatformBriefingService.Generate(source, generate, verify, prompt => "", null, read, write); }
        catch (ApplicationException) { failed = true; }
        Check(failed && generated == 2 && reviewed == 1 && saved.Count == 1 && saved.ContainsKey("e1"),
            "A failed batch was cached as approved, or earlier verified work was lost.");
        failSecond = false;
        generated = 0;
        reviewed = 0;
        var resumed = CmfPlatformBriefingService.Generate(source, generate, verify, prompt => "", null, read, write);
        Check(generated == 1 && reviewed == 1 && resumed.Count == 2 && resumed[0].Id == "f1" && resumed[1].Id == "f2",
            "Overall retry redid approved work, skipped review of new work, or published an incomplete result.");
        generated = 0;
        reviewed = 0;
        CmfPlatformBriefingService.Generate(source, generate, verify, prompt => "", null, null, write);
        Check(generated == 2 && reviewed == 2, "A fresh successful-result refresh reused earlier batch approvals.");
        var key = typeof(CmfPlatformBriefingService).GetMethod("BatchCacheKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        string originalKey = (string)key.Invoke(null, new object[] { source, "provider-a" });
        Check(originalKey != (string)key.Invoke(null, new object[] { source, "provider-b" }), "A different provider reused batch approval.");
        source.Evidence[0].Measurements[0].Current = 13;
        Check(originalKey != (string)key.Invoke(null, new object[] { source, "provider-a" }), "Changed measurements reused batch approval.");
        Console.WriteLine("Overall retry checks passed: only the failed batch is regenerated and independently reviewed; all approved findings remain retained.");
    }

    public static void CheckBriefingBacklogMateriality()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        foreach (string platform in new[] { "NVL_H", CmfPlatformCatalog.OverallPlatformTableName })
        {
            var source = new CmfPlatformBriefingSource { Platform = platform, Scope = "Backlog materiality fixture", Hash = "materiality-fixture",
                DateScope = "all", CapturedAt = "2026-11-23", Evidence = new List<CmfPlatformBriefingEvidence> {
                    new CmfPlatformBriefingEvidence { Id = "closed", Section = "Summary", Subject = "Closed CMFs", Kind = "current",
                        Context = new Dictionary<string, string> { { "Population", "Accepted CMF records including duplicates" } },
                        Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement { Name = "Closed CMFs", Current = 900,
                            Denominator = 926, Unit = "records", CurrentDate = "2026-11-23" } } },
                    new CmfPlatformBriefingEvidence { Id = "open", Section = "Summary", Subject = "Open CMFs", Kind = "current",
                        Context = new Dictionary<string, string> { { "Population", "Accepted CMF records including duplicates" } },
                        Measurements = new List<CmfFactMeasurement> { new CmfFactMeasurement { Name = "Open CMFs", Current = 19,
                            Denominator = 926, Unit = "records", CurrentDate = "2026-11-23" } } } } };
            string closure = "{\"title\":\"Predominantly closed CMFs\",\"summary\":\"900 of 926 accepted CMF records are closed, including duplicates.\",\"kind\":\"pattern\",\"tone\":\"neutral\",\"section\":\"highlights\",\"evidenceIds\":[\"closed\"],\"support\":[{\"evidenceId\":\"closed\",\"measurementNames\":[\"Closed CMFs\"],\"explanation\":\"Closed CMFs form a majority of the same accepted population.\"}]}";
            string smallBalance = "{\"title\":\"Active open CMF backlog\",\"summary\":\"19 of 926 accepted CMF records are open, including duplicates.\",\"kind\":\"pattern\",\"tone\":\"attention\",\"section\":\"focus\",\"evidenceIds\":[\"open\"],\"support\":[{\"evidenceId\":\"open\",\"measurementNames\":[\"Open CMFs\"],\"explanation\":\"The Open count is a small part of the accepted population.\"}]}";
            int generated = 0, reviewed = 0;
            var findings = CmfPlatformBriefingService.Generate(source, prompt => {
                generated++;
                Check(prompt.Contains("Nonzero backlog alone is insufficient") && prompt.Contains("Do not force standalone Open/pending balances"),
                    "Generation still mandates a Lowlight for every available nonzero backlog.");
                if (generated > 1) Check(prompt.Contains("PreviousDraft") && prompt.Contains("small Open share"), "Materiality correction lost reviewer feedback.");
                return "{\"findings\":[" + closure + (generated == 1 ? "," + smallBalance : "") + "]}";
            }, prompt => {
                reviewed++;
                Check(prompt.Contains("Omission of routine small balances does not fail completeness")
                    && prompt.Contains("Material aging, concentrations or qualifying adverse changes remain eligible")
                    && prompt.Contains(platform == CmfPlatformCatalog.OverallPlatformTableName ? "IDST SCOPE: Overall retains" : "IDST SCOPE: Platform excludes"),
                    "Independent review forces small balances or suppresses separately material concerns.");
                var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
                var proposed = (object[])packet["Findings"];
                var skeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
                skeleton["complete"] = true;
                foreach (Dictionary<string, object> check in (object[])skeleton["checks"])
                {
                    var finding = (Dictionary<string, object>)proposed[Convert.ToInt32(check["index"])];
                    bool routine = (string)finding["Title"] == "Active open CMF backlog";
                    foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons" }) check[flag] = true;
                    check["significance"] = !routine;
                    check["reason"] = routine ? "A small Open share has no separate evidenced review need; omit the routine balance." : "";
                }
                return serializer.Serialize(skeleton);
            }, prompt => { throw new Exception("Materiality correction requested unnecessary ranking."); });
            Check(generated == 2 && reviewed == 2 && findings.Count == 1 && findings[0].Title == "Predominantly closed CMFs"
                && findings[0].Section == "highlights", "A routine small backlog escaped review or was relabeled rather than regenerated and reviewed.");
        }
        Console.WriteLine("Backlog materiality checks passed: small routine Open balances are omitted through fresh generation/review in both scopes.");
    }

    public static void CheckBriefingHighlightCandidates()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        foreach (string platform in new[] { "NVL_H", CmfPlatformCatalog.OverallPlatformTableName })
        {
            var snapshot = new CmfPanelSnapshotCache { Platform = platform, Configuration = "highlight-fixture", DateScope = "all",
                CapturedAt = new DateTime(2026, 11, 23), Data = new Dictionary<string, object> {
                    { "PlatformLabel", "Highlight fixture" },
                    { "SummaryFacts", serializer.DeserializeObject(@"[{""Label"":""Total CMFs"",""Value"":""240""},{""Label"":""Open"",""Value"":""8"",""Note"":""3 duplicates""}]") },
                    { "TptFacts", serializer.DeserializeObject(@"[{""Label"":""CMF Disposition TPT"",""Value"":""2""}]") },
                    { "PendingSummaryAgeThresholdDays", 2 },
                    { "PendingSummary", serializer.DeserializeObject(@"{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],""Rows"":[[""HW"",""Owner"",""3:::OVER_TWO_DAYS:::0""],[""Total"","""",""3""]]}") },
                    { "DesignSummary", serializer.DeserializeObject(@"{""Columns"":[""Design Name"",""SW Image Freeze"",""Support Model"",""QS"",""Impl/Verified""],""Rows"":[[""A"",""NA"","""",""0"",""2""],[""B"",""NA"","""",""0"",""0""],[""C"",""NA"","""",""0"",""0""],[""D"",""NA"","""",""1"",""0""],[""E"",""NA"","""",""3"",""0""]]}") } } };
            var source = CmfPlatformBriefingService.BuildSource(snapshot);
            var open = source.Evidence.Single(item => item.Section == "CMF Summary" && item.Subject == "Open");
            var tpt = source.Evidence.Single(item => item.Section == "TPT");
            var design = source.Evidence.Single(item => item.Section == "DesignExposure");
            var pending = source.Evidence.Single(item => item.Subject == "Pending queue coverage");
            Check(open.Measurements.Single(item => item.Name == "Open duplicates").Current == 3
                && open.Measurements.Single(item => item.Name == "Open unique CMFs").Current == 5
                && open.Measurements.Single(item => item.Name == "Open").Denominator == 240,
                "Open composition uses invented example counts or another status population.");
            Check(design.Measurements.Single(item => item.Name == "Designs with at most one Open record").Current == 4
                && design.Measurements.Single(item => item.Name == "Designs with at most one Open record").Denominator == 5,
                "Design Highlight counts are not based on the complete current table.");
            Check(pending.Measurements.Single(item => item.Name == "Pending with iDST").Current == 3
                && !pending.Measurements.Any(item => item.Name == "Pending within age threshold")
                && tpt.Context["Threshold assessment"] == "At target", "Additional Highlight candidate measures are unavailable or misclassified.");
            Func<CmfPlatformBriefingEvidence, string, string, string[], object> candidate = (row, title, summary, names) => new {
                title, summary, kind = "pattern", tone = "neutral", section = "highlights", evidenceIds = new[] { row.Id },
                support = new[] { new { evidenceId = row.Id, measurementNames = names,
                    explanation = "The selected current fields establish this relative share or reference coverage within their explicit population, not history or delivery readiness." } } };
            var candidates = new List<object> {
                candidate(open, "Small active backlog", "8 of 240 accepted CMFs are Open, including 3 duplicates and 5 unique CMFs.", new[] { "Open", "Open duplicates", "Open unique CMFs" }),
                candidate(tpt, "On-target disposition", "CMF disposition meets its target.", new[] { "CMF Disposition TPT", "TPT target" }),
                candidate(design, "Broad low-open design coverage", "4 of 5 designs have no more than one open design-table record.", new[] { "Designs with at most one Open record" }) };
            if (platform == CmfPlatformCatalog.OverallPlatformTableName)
                candidates.Add(candidate(pending, "Complete pending assignment coverage", "3 of 3 pending assessments have an iDST assigned.", new[] { "Pending with iDST" }));
            string output = serializer.Serialize(new { findings = candidates });
            int generated = 0, reviewed = 0;
            var findings = CmfPlatformBriefingService.Generate(source, prompt => {
                generated++;
                Check(prompt.Contains("HIGHLIGHT CANDIDATES") && prompt.Contains("Use supplied measures, not example counts")
                    && prompt.Contains("Design wording: 'have no more than one open'")
                    && prompt.Contains("At target supports neutral/highlights") && prompt.Contains("no quota"), "Highlight generation still excludes supported candidates or copies a count quota.");
                return output;
            }, prompt => {
                reviewed++;
                Check(prompt.Contains("At-target alone does not justify fast or maintained") && prompt.Contains("complete requires all distinct material candidates"),
                    "Candidate completeness or unsupported temporal/performance review is missing.");
                Check(prompt.Contains("Design wording: 'have no more than one open'"), "Independent review lost the requested design-coverage wording.");
                var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
                var skeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
                skeleton["complete"] = true;
                foreach (Dictionary<string, object> check in (object[])skeleton["checks"])
                {
                    foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons", "significance" }) check[flag] = true;
                    check["reason"] = "";
                }
                return serializer.Serialize(skeleton);
            }, prompt => { throw new Exception("Highlight candidates requested unnecessary ranking."); });
            Check(generated == 1 && reviewed == 1 && findings.Count == candidates.Count && findings.All(item => item.Section == "highlights"),
                "Multiple evidence-backed Highlights were suppressed or bypassed independent review.");
            foreach (string note in new[] { "9 duplicates", "3 duplicates from all statuses", "unavailable" })
            {
                snapshot.Data["SummaryFacts"] = serializer.DeserializeObject("[{\"Label\":\"Open\",\"Value\":\"8\",\"Note\":" + serializer.Serialize(note) + "}]");
                var invalidSplit = CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Single(item => item.Subject == "Open");
                Check(invalidSplit.Measurements.Count == 1, "Invalid or mismatched duplicate metadata was converted into a unique Open count.");
            }
            snapshot.Data["PendingSummary"] = serializer.DeserializeObject(@"{""Columns"":[""Ingredient"",""iDST Name"",""CMF Pending""],""Rows"":[[""HW"",""Owner"",""3""]]}");
            Check(!CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Single(item => item.Subject == "Pending queue coverage")
                .Measurements.Any(item => item.Name == "Pending within age threshold"), "Unknown aging was treated as within threshold.");
        }
        Console.WriteLine("Highlight candidate checks passed in both scopes: current Open splits, on-target TPT, complete design coverage and pending assignment coverage, without sample copying or relaxed review.");
    }

    public static void CheckBriefingPanelIsolation()
    {
        var source = new CmfPlatformBriefingSource { Platform = "NVL_H", Scope = "Panel isolation test", DateScope = "2026-11-23",
            Configuration = "test-config", CapturedAt = "2026-11-23", Hash = "combined-source", Evidence = new List<CmfPlatformBriefingEvidence>() };
        foreach (string section in new[] { "CMF Summary", "TPT", "CustomerIssues", "CustomerIssueCoverage" })
            source.Evidence.Add(new CmfPlatformBriefingEvidence { Id = section, Section = section, Subject = section,
                Context = new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                    new CmfFactMeasurement { Name = section, Current = 1, Unit = "records", CurrentDate = "2026-11-23" } } });
        var summary = CmfPlatformBriefingService.ForPanel(source, "summary");
        var customers = CmfPlatformBriefingService.ForPanel(source, "customers");
        Check(summary.Evidence.Count == 2 && summary.Evidence.All(row => row.Section != "CustomerIssues" && row.Section != "CustomerIssueCoverage")
            && customers.Evidence.Count == 2 && customers.Evidence.All(row => row.Section == "CustomerIssues" || row.Section == "CustomerIssueCoverage"),
            "Summary and customer panel evidence is not isolated.");
        Check(summary.Hash != customers.Hash && CmfPlatformBriefingService.PipelineFor(summary) == CmfPlatformBriefingService.SummaryPipeline
            && CmfPlatformBriefingService.PipelineFor(customers) == CmfPlatformBriefingService.CustomerPipeline, "Panel jobs or cache identities collide.");
        string originalSummaryHash = summary.Hash;
        source.Evidence.Single(row => row.Section == "CustomerIssues").Measurements[0].Current = 2;
        Check(CmfPlatformBriefingService.ForPanel(source, "summary").Hash == originalSummaryHash
            && CmfPlatformBriefingService.ForPanel(source, "customers").Hash != customers.Hash, "Customer-only data changes invalidate the Summary cache.");
        foreach (var row in source.Evidence) row.Id = "shifted-" + row.Id;
        Check(CmfPlatformBriefingService.ForPanel(source, "summary").Hash == originalSummaryHash,
            "Original combined-row ID shifts invalidate otherwise unchanged Summary evidence.");
        foreach (var panel in new[] { summary, customers })
        {
            string label = panel.Panel == "summary" ? "PANEL: Summary only" : "PANEL: Customer Issues only";
            Check(CmfPlatformBriefingService.AnalysisPrompt(panel).Contains(label)
                && CmfPlatformBriefingService.VerificationPrompt(panel, new List<CmfPlatformBriefingFinding>()).Contains(label),
                "Generation/review is not constrained to the selected panel.");
            CmfPlatformBriefingService.Generate(panel, prompt => {
                Check(prompt.Contains(label), "A generation batch lost its panel scope.");
                return "{\"findings\":[]}";
            }, prompt => {
                Check(prompt.Contains(label), "An independent review batch lost its panel scope.");
                return "{\"complete\":true,\"checks\":[]}";
            }, prompt => { throw new Exception("Panel fixtures requested unnecessary ranking."); });
        }
        var approved = new CmfPlatformBriefing { Success = true, Mode = "ai", Stage = "complete", Pipeline = CmfPlatformBriefingService.SummaryPipeline,
            SourceHash = summary.Hash, Scope = summary.Scope, ExpiresAt = DateTime.UtcNow.AddMinutes(10).ToString("o"), Findings = new List<CmfPlatformBriefingFinding>() };
        Check(CmfPlatformBriefingService.IsReusableResult(summary, approved, DateTime.UtcNow)
            && !CmfPlatformBriefingService.IsReusableResult(customers, approved, DateTime.UtcNow), "Approved panel results leak into another panel.");
        Console.WriteLine("Panel isolation checks passed: independent evidence, pipelines, cache hashes and approvals; customer changes do not invalidate Summary.");
    }

    public static void CheckBriefingCustomerIssueGrouping()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var source = new CmfPlatformBriefingSource { Platform = "NVL_H", Scope = "Customer issue grouping test", DateScope = "2026-11-23",
            CapturedAt = "2026-11-23", Hash = "customer-issue-grouping-test", Evidence = new List<CmfPlatformBriefingEvidence>() };
        foreach (var report in new[] {
            new { Id = "open-contoso-hang", Population = "Open", Customer = "Contoso", Title = "System hangs after resume" },
            new { Id = "open-fabrikam-hang", Population = "Open", Customer = "Fabrikam", Title = "System hangs after resume" },
            new { Id = "open-contoso-display", Population = "Open", Customer = "Contoso", Title = "External monitor blanks after resume" },
            new { Id = "pending-contoso-dock", Population = "Pending", Customer = "Contoso", Title = "USB dock is not recognized after startup" } })
        {
            source.Evidence.Add(new CmfPlatformBriefingEvidence { Id = report.Id, Section = "CustomerIssues",
                Subject = report.Population + " reported customer issue", Kind = "current",
                Context = new Dictionary<string, string> { { "Issue population", report.Population }, { "Customer", report.Customer },
                    { "Reported issue title", report.Title } }, Measurements = new List<CmfFactMeasurement> {
                    new CmfFactMeasurement { Name = "Reported issue record", Current = 1, Unit = "records", CurrentDate = "2026-11-23" } } });
        }
        Func<string, string, string[], object> issue = (title, summary, ids) => new {
            title, summary, kind = "pattern", tone = "attention", section = "focus", evidenceIds = ids,
            support = ids.Select(id => new { evidenceId = id, measurementNames = new[] { "Reported issue record" },
                explanation = source.Evidence.Single(row => row.Id == id).Context["Customer"] + " reports "
                    + source.Evidence.Single(row => row.Id == id).Context["Reported issue title"] + " in the "
                    + source.Evidence.Single(row => row.Id == id).Context["Issue population"]
                    + " population; the report establishes this symptom, not a diagnosis." }).ToArray() };
        object dock = issue("Reported dock recognition", "Contoso reports USB dock recognition problems after startup.", new[] { "pending-contoso-dock" });
        string combined = serializer.Serialize(new { findings = new[] {
            issue("Reported resume problems", "Contoso and Fabrikam report system hangs and Contoso reports external monitor blanking after resume.",
                new[] { "open-contoso-hang", "open-fabrikam-hang", "open-contoso-display" }), dock } });
        string separated = serializer.Serialize(new { findings = new[] {
            issue("Reported resume hangs", "Contoso and Fabrikam report system hangs after resume.", new[] { "open-contoso-hang", "open-fabrikam-hang" }),
            issue("Reported external monitor blanking", "Contoso reports external monitor blanking after resume.", new[] { "open-contoso-display" }), dock } });
        foreach (string prefix in new[] { "Open customer issues include", "Open customer issue include", "open reports include",
            "Pending assessments include", "Pending customer issues include", "Pending reports include" })
        {
            bool pending = prefix.StartsWith("Pending", StringComparison.OrdinalIgnoreCase);
            string output = serializer.Serialize(new { findings = new[] { issue("Reported customer issue", prefix
                + (pending ? " Contoso USB dock recognition problems after startup." : " Contoso system hangs after resume."),
                new[] { pending ? "pending-contoso-dock" : "open-contoso-hang" }) } });
            bool rejected = false;
            try { CmfPlatformBriefingService.Validate(source, output); }
            catch (InvalidOperationException exception) { rejected = exception.Data["RejectedCheck"] as string == "customer-wording"; }
            Check(rejected, "A redundant customer status lead-in was accepted: " + prefix);
        }
        int generated = 0, reviewed = 0;
        var findings = CmfPlatformBriefingService.Generate(source, prompt => {
            generated++;
            Check(prompt.Contains("ISSUE-BASED CUSTOMER BULLETS") && prompt.Contains("Split unrelated symptoms even for one customer")
                && prompt.Contains("not one omnibus per status"), "Customer generation still permits an unrelated omnibus bulletin.");
            Check(prompt.Contains("Headings supply status; start with the customer or symptom")
                && !prompt.Contains("State the population and use includes"), "Customer generation still requires a redundant status lead-in.");
            foreach (var evidence in source.Evidence) Check(prompt.Contains(evidence.Id), "Issue grouping lost complete customer evidence.");
            if (generated > 1) Check(prompt.Contains("PreviousDraft") && prompt.Contains("Unrelated issue types"), "Issue grouping correction lost review feedback.");
            return generated == 1 ? combined : separated;
        }, prompt => {
            reviewed++;
            Check(prompt.Contains("ISSUE-BASED CUSTOMER BULLETS") && prompt.Contains("a shared customer/status is not grounds to merge")
                && prompt.Contains("Require all distinct material impacts for complete"), "Customer review does not enforce issue-level grouping/completeness.");
            Check(prompt.Contains("Reject repeated status lead-ins") && prompt.Contains("Reports are partial coverage, not exhaustive claims"),
                "Customer review no longer enforces concise, nonexhaustive customer wording.");
            var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
            var skeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
            skeleton["complete"] = true;
            foreach (Dictionary<string, object> check in (object[])skeleton["checks"])
            {
                foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons", "significance" }) check[flag] = true;
                bool bundled = reviewed == 1 && Convert.ToInt32(check["index"]) == 0;
                check["significance"] = !bundled;
                check["reason"] = bundled ? "Unrelated issue types must be separate bullets; resume hangs and external monitor blanking are distinct." : "";
            }
            return serializer.Serialize(skeleton);
        }, prompt => { throw new Exception("Customer issue grouping requested unnecessary ranking."); });
        Check(generated == 2 && reviewed == 2 && findings.Count == 3, "Bundled customer issues were published or split without fresh review.");
        Check(findings[0].EvidenceIds.SequenceEqual(new[] { "open-contoso-hang", "open-fabrikam-hang" })
            && findings[1].EvidenceIds.SequenceEqual(new[] { "open-contoso-display" })
            && findings[2].EvidenceIds.SequenceEqual(new[] { "pending-contoso-dock" })
            && findings.SelectMany(item => item.EvidenceIds).Distinct().Count() == source.Evidence.Count,
            "Issue-based bullets lost a distinct issue, mixed populations or duplicated overlapping reports.");
        Check(findings[0].Evidence.All(item => item.Text.Contains("Reported issue title: System hangs after resume"))
            && findings[1].Evidence.Single().Text.Contains("Reported issue title: External monitor blanks after resume")
            && findings[2].Evidence.Single().Text.Contains("Issue population: Pending"), "Issue bullets lost exact customer/title Source attribution.");
        Console.WriteLine("Customer issue grouping checks passed: distinct issues split, overlapping same-issue reports merged, exact Sources and fresh independent review retained.");
    }

    public static void CheckBriefingTptMagnitude()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        foreach (string platform in new[] { "NVL_H", CmfPlatformCatalog.OverallPlatformTableName })
        foreach (var band in new[] {
            new { Measure = "CMF Disposition TPT", Target = 2m, Current = 3m },
            new { Measure = "CMF Disposition TPT", Target = 2m, Current = 4m },
            new { Measure = "CMF Resolution TPT", Target = 18m, Current = 22m },
            new { Measure = "CMF Overall TPT", Target = 21m, Current = 26m },
            new { Measure = "CMF Overall TPT", Target = 21m, Current = 31m },
            new { Measure = "CMF Overall TPT", Target = 21m, Current = 32m },
            new { Measure = "CMF Overall TPT", Target = 21m, Current = 41m },
            new { Measure = "CMF Overall TPT", Target = 21m, Current = 42m },
            new { Measure = "CMF Overall TPT", Target = 21m, Current = 80m } })
        {
            var source = new CmfPlatformBriefingSource { Platform = platform, Scope = "TPT magnitude test", DateScope = "2026-11-23",
                CapturedAt = "2026-11-23", Hash = "tpt-magnitude-test",
                Evidence = new List<CmfPlatformBriefingEvidence> {
                    new CmfPlatformBriefingEvidence { Id = "tpt", Section = "TPT", Subject = band.Measure, Kind = "current",
                        Context = new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                            new CmfFactMeasurement { Name = band.Measure, Current = band.Current, Unit = "days", CurrentDate = "2026-11-23" },
                            new CmfFactMeasurement { Name = "TPT target", Current = band.Target, Unit = "days", CurrentDate = "2026-11-23" },
                            new CmfFactMeasurement { Name = "Difference from TPT target", Current = band.Current - band.Target, Unit = "days", CurrentDate = "2026-11-23" } } } } };
            string gap = (band.Current - band.Target).ToString(System.Globalization.CultureInfo.InvariantCulture);
            string dayUnit = gap == "1" ? "day" : "days";
            Func<string, string> output = wording => serializer.Serialize(new { findings = new[] { new {
                title = band.Measure.Replace("CMF ", "") + " above target", summary = band.Measure + " is " + wording + " target by " + gap + " " + dayUnit + ".",
                kind = "pattern", tone = "attention", section = "focus", evidenceIds = new[] { "tpt" },
                support = new[] { new { evidenceId = "tpt", measurementNames = new[] { band.Measure, "TPT target", "Difference from TPT target" },
                    explanation = "The current average and its own target establish the exact excess in days, not a historical change or an SLA breach." } } } } });
            var findings = CmfPlatformBriefingService.Validate(source, output("above"));
            Check(findings.Count == 1 && findings[0].Evidence[0].Text.Contains("Field TPT target: " + band.Target + " days")
                && findings[0].Evidence[0].Text.Contains("Field " + band.Measure + ": " + band.Current + " days")
                && findings[0].Evidence[0].Text.Contains("Field Difference from TPT target: " + gap + " day")
                && findings[0].Summary.Contains(gap + " " + dayUnit),
                "Plain TPT wording lost its exact current, target or gap Source fields.");
            Check(CmfPlatformBriefingService.Validate(source, output("above").Replace("is above target", "exceeds target")).Count == 1,
                "A clear exceeds-target paraphrase was rejected.");
            foreach (string invalid in new[] {
                output("above").Replace(" by " + gap + " " + dayUnit, ""),
                output("above").Replace(" by " + gap + " " + dayUnit, " by " + (band.Current - band.Target + 1m) + " days"),
                output("above").Replace(",\"Difference from TPT target\"", "") })
            {
                bool gapRejected = false;
                try { CmfPlatformBriefingService.Validate(source, invalid); }
                catch (InvalidOperationException exception) { gapRejected = exception.Data["RejectedCheck"] as string == "tpt-gap"; }
                Check(gapRejected, "A missing, incorrect or unselected exact TPT target gap was accepted.");
            }
            foreach (string exaggerated in new[] { "well above", "far above", "much beyond", "substantially above" })
            {
                bool rejected = false;
                try { CmfPlatformBriefingService.Validate(source, output(exaggerated)); }
                catch (InvalidOperationException exception) { rejected = exception.Data["RejectedCheck"] as string == "tpt-magnitude"; }
                Check(rejected, "An exaggerated TPT qualifier was accepted.");
            }
            var titlePacket = (Dictionary<string, object>)serializer.DeserializeObject(output("above"));
            ((Dictionary<string, object>)((object[])titlePacket["findings"])[0])["title"] = band.Measure.Replace("CMF ", "") + " well above target";
            bool titleRejected = false;
            try { CmfPlatformBriefingService.Validate(source, serializer.Serialize(titlePacket)); }
            catch (InvalidOperationException exception) { titleRejected = exception.Data["RejectedCheck"] as string == "tpt-magnitude"; }
            Check(titleRejected, "An exaggerated TPT heading was accepted with a plain sentence.");
            foreach (string prompt in new[] { CmfPlatformBriefingService.AnalysisPrompt(source), CmfPlatformBriefingService.VerificationPrompt(source, findings) })
                Check(prompt.Contains("plain above target or exceeds target for every excess")
                    && !prompt.Contains("half the target") && !prompt.Contains("generic above for larger gaps")
                    && prompt.Contains("exact Difference from TPT target in days"),
                    "Generation and independent review lack consistent plain TPT wording.");
            if (band.Current == 80m)
            {
                int generated = 0, reviewed = 0;
                var corrected = CmfPlatformBriefingService.Generate(source, prompt => {
                    generated++;
                    if (generated > 1) Check(prompt.Contains("PreviousDraft") && prompt.Contains("Source exaggeration"),
                        "Plain TPT correction lost the rejected draft or independent review feedback.");
                    return generated == 1 ? output("above").Replace(
                        "The current average and its own target establish the exact excess in days, not a historical change or an SLA breach.",
                        "The gap proves severe delivery delays.") : output("above").Replace("is above target", "exceeds target");
                }, prompt => {
                    reviewed++;
                    Check(prompt.Contains("Reject exaggerated magnitude qualifiers in title, sentence or Source"),
                        "Independent review no longer checks exaggerated TPT explanations.");
                    var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
                    var skeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
                    skeleton["complete"] = true;
                    foreach (Dictionary<string, object> check in (object[])skeleton["checks"])
                    {
                        foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons", "significance" }) check[flag] = true;
                        check["comparisons"] = reviewed > 1;
                        check["reason"] = reviewed > 1 ? "" : "Source exaggeration: the exact target gap does not prove severe delivery delays.";
                    }
                    return serializer.Serialize(skeleton);
                }, prompt => { throw new Exception("TPT magnitude requested an unnecessary ranking call."); });
                Check(generated == 2 && reviewed == 2 && corrected.Single().Summary.Contains("exceeds target")
                    && !corrected.Single().Support.Single().Explanation.Contains("severe"),
                    "Exaggerated TPT evidence was published or corrected without fresh independent review.");
            }
        }
        Console.WriteLine("TPT wording checks passed in both scopes: one-day and large gaps, plain paraphrases, exaggeration rejection and exact Sources.");
    }

    public static void CheckBriefingIdstExclusion()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        foreach (string platform in new[] { "NVL_H", CmfPlatformCatalog.OverallPlatformTableName })
        {
            var source = new CmfPlatformBriefingSource { Platform = platform, Scope = "Assignment exclusion fixture", Hash = "idst-exclusion",
                DateScope = "all", CapturedAt = "2026-11-23", Evidence = new List<CmfPlatformBriefingEvidence> {
                    new CmfPlatformBriefingEvidence { Id = "pending", Section = "PendingSummary", Subject = "Pending queue coverage", Kind = "current",
                        Context = new Dictionary<string, string> { { "Population", "Separate pending-assessment queue" } },
                        Measurements = new List<CmfFactMeasurement> {
                            new CmfFactMeasurement { Name = "Pending without iDST", Current = 2, Denominator = 5, Unit = "records", CurrentDate = "2026-11-23" },
                            new CmfFactMeasurement { Name = "Pending with iDST", Current = 3, Denominator = 5, Unit = "records", CurrentDate = "2026-11-23" },
                            new CmfFactMeasurement { Name = "CMF Pending", Current = 5, Unit = "records", CurrentDate = "2026-11-23" },
                            new CmfFactMeasurement { Name = "Aged pending", Current = 3, Denominator = 5, Unit = "records", CurrentDate = "2026-11-23" },
                            new CmfFactMeasurement { Name = "Pending age threshold", Current = 2, Unit = "days", CurrentDate = "2026-11-23" } } } } };
            Func<string, string, string[], string> output = (summary, section, names) => serializer.Serialize(new { findings = new[] { new {
                title = "Pending assessments", summary, kind = "pattern", tone = section == "focus" ? "attention" : "neutral", section,
                evidenceIds = new[] { "pending" }, support = new[] { new { evidenceId = "pending", measurementNames = names,
                    explanation = "The selected fields identify this observation in the separate pending queue without assuming overlap." } } } } });
            foreach (string excluded in new[] {
                output("2 of 5 pending assessments have no iDST assigned.", "focus", new[] { "Pending without iDST" }),
                output("3 of 5 pending assessments have an iDST assigned.", "highlights", new[] { "Pending with iDST" }),
                output("2 of 5 pending assessments have no iDST assigned.", "focus", new[] { "Pending without iDST", "CMF Pending" }) })
            {
                if (platform == CmfPlatformCatalog.OverallPlatformTableName)
                {
                    Check(CmfPlatformBriefingService.Validate(source, excluded).Count == 1, "Overall iDST assignment/coverage was incorrectly excluded.");
                    continue;
                }
                bool rejected = false;
                try { CmfPlatformBriefingService.Validate(source, excluded); }
                catch (InvalidOperationException exception) { rejected = exception.Data["RejectedCheck"] as string == "idst-item"; }
                Check(rejected, "An assignment-only finding bypassed the iDST exclusion or hid behind a queue-total selection.");
            }
            Check(CmfPlatformBriefingService.Validate(source, output("3 of 5 pending assessments exceed the 2-day threshold.",
                "focus", new[] { "Aged pending", "Pending age threshold" })).Count == 1, "iDST exclusion suppressed a material aging observation.");
            string policy = platform == CmfPlatformCatalog.OverallPlatformTableName ? "IDST SCOPE: Overall retains" : "IDST SCOPE: Platform excludes";
            Check(CmfPlatformBriefingService.AnalysisPrompt(source).Contains(policy)
                && CmfPlatformBriefingService.VerificationPrompt(source, new List<CmfPlatformBriefingFinding>()).Contains(policy),
                "Generation and independent review disagree with the requested iDST scope.");
        }
        Console.WriteLine("iDST scope checks passed: assignment/coverage excluded from platforms, retained for Overall; aging remains available.");
    }

    public static void CheckPlatformBriefingValidation()
    {
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var designSnapshot = new CmfPanelSnapshotCache { Platform = "NVL_H", Configuration = "entity-fixture", DateScope = "all",
            CapturedAt = new DateTime(2026, 11, 23), Data = new Dictionary<string, object> {
                { "PlatformLabel", "NVL H" },
                { "DesignSummary", serializer.DeserializeObject(@"{""Columns"":[""Design Name"",""SW Image Freeze"",""Support Model"",""QS"",""Impl/Verified""],""Rows"":[[""dell_nvl_trinity"",""NA"","""",""3"",""0""],[""Vendor_X-Board.2"",""NA"","""",""1"",""0""]]}") } } };
        var designSource = CmfPlatformBriefingService.BuildSource(designSnapshot);
        var namedDesigns = designSource.Evidence.Where(item => item.Section == "DesignSummary").ToList();
        Check(namedDesigns.Select(item => item.Subject).SequenceEqual(new[] { "dell_nvl_trinity", "Vendor_X-Board.2" })
            && namedDesigns.All(item => !item.Context.ContainsKey("Heading name")),
            "Design names were aliased or their original casing/separators were changed.");
        var evidence = new CmfPlatformBriefingEvidence { Id = "e1", Section = "ComponentSummary", Subject = "Graphics", Kind = "distribution",
            Context = new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                new CmfFactMeasurement { Name = "Open", Current = 12, Denominator = 20, Unit = "records", CurrentDate = "2026-11-19" } } };
        var source = new CmfPlatformBriefingSource { Platform = "NVL_H", Scope = "NVL H", Hash = "fixture", CapturedAt = "2026-11-19",
            Evidence = new List<CmfPlatformBriefingEvidence> { evidence } };
        Func<string, string> finding = title => serializer.Serialize(new { title = title, summary = "Graphics contains 12 of 20 Open records.",
            kind = "cluster", tone = "neutral", section = "highlights", evidenceIds = new[] { "e1" },
            support = new[] { new { evidenceId = "e1", measurementNames = new[] { "Open" },
                explanation = "Graphics holds a majority of the open records." } } });
        string output = "{\"findings\":[" + string.Join(",", Enumerable.Range(0, 17).Select(index => finding("Observation " + new string('a', index + 1)))) + "]}";
        Func<Dictionary<string, object>> trendPacket = () => {
            string input = CmfPlatformBriefingService.AnalysisPrompt(source);
            return (Dictionary<string, object>)serializer.DeserializeObject(input.Substring(input.IndexOf("\n", StringComparison.Ordinal) + 1));
        };
        Check(((object[])trendPacket()["TrendEvidenceIds"]).Length == 0, "Current distributions were advertised as sustained-trend evidence.");
        evidence.Kind = "history";
        evidence.Context["Sustained direction supported"] = "yes";
        bool historyRejected = false;
        try { CmfPlatformBriefingService.Validate(source, output.Replace("\"cluster\"", "\"trend\"")); }
        catch (InvalidOperationException) { historyRejected = true; }
        Check(((object[])trendPacket()["TrendEvidenceIds"]).Length == 0 && historyRejected, "Historical trends were allowed in today's briefing.");
        evidence.Kind = "distribution";
        evidence.Context.Clear();
        var validated = CmfPlatformBriefingService.Validate(source, output);
        evidence.Measurements.Add(new CmfFactMeasurement { Name = "PV Impl/Verified duplicates", Current = 0,
            Denominator = 1, Unit = "records", CurrentDate = "2026-11-19" });
        var specific = CmfPlatformBriefingService.Validate(source, output);
        Check(specific[0].Evidence[0].Source == "Component Summary - Graphics"
            && specific[0].Evidence[0].Text.Contains("Why this supports the finding: Graphics holds a majority")
            && specific[0].Evidence[0].Text.Contains("denominator: 20 records across ingredients in this same field")
            && specific[0].Evidence[0].Text.Contains("Measurement date: 2026-11-19")
            && !specific[0].Evidence[0].Text.Contains("PV Impl/Verified"), "Source drawer dumps unrelated fields or loses its precise rationale/population.");
        evidence.Measurements.RemoveAt(1);
        Check(validated.Count == 17, "A fixed platform finding limit remains.");
        var quantity = typeof(CmfPlatformBriefingService).GetMethod("Quantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Check((string)quantity.Invoke(null, new object[] { 1m, "records" }) == "1 record"
            && (string)quantity.Invoke(null, new object[] { -1m, "days" }) == "-1 day"
            && (string)quantity.Invoke(null, new object[] { 2.5m, "days" }) == "2.5 days", "Source quantities use incorrect singular units.");
        Check(validated.All(item => item.Section == "highlights"), "Model-selected sections were lost.");
        string sectionPrompt = CmfPlatformBriefingService.VerificationPrompt(source, validated);
        Check(sectionPrompt.Contains("\"Section\":\"highlights\"") && sectionPrompt.Contains("Tone and Section")
            && sectionPrompt.Contains("explicit target or supported comparison"), "Independent review does not cover section choice and evaluative subheadings.");
        string writingPrompt = CmfPlatformBriefingService.AnalysisPrompt(source);
        foreach (string platform in new[] { source.Platform, CmfPlatformCatalog.OverallPlatformTableName })
        {
            var sectionSource = new CmfPlatformBriefingSource { Platform = platform, Scope = source.Scope, Hash = source.Hash,
                CapturedAt = source.CapturedAt, DateScope = source.DateScope, Evidence = source.Evidence };
            string generationRules = CmfPlatformBriefingService.AnalysisPrompt(sectionSource);
            string reviewRules = CmfPlatformBriefingService.VerificationPrompt(sectionSource, validated);
            string scopeAssignmentRule = platform == CmfPlatformCatalog.OverallPlatformTableName
                ? "unassigned exposure uses attention/focus" : "Reject disguised recaps";
            Check(generationRules.Contains("BACKLOG FOCUS") && generationRules.Contains("MUST use focus (Lowlights) with attention tone")
                && !generationRules.Contains("Neutral current balances belong under highlights")
                && generationRules.Contains("Only material review needs justify attention/focus")
                && reviewRules.Contains("BACKLOG FOCUS") && reviewRules.Contains("findings in highlights or without attention tone")
                && generationRules.Contains("leading Open/Pending ingredients, maximum-open designs and aging")
                && generationRules.Contains(scopeAssignmentRule) && reviewRules.Contains(scopeAssignmentRule)
                && reviewRules.Contains("Only material review needs justify Lowlights")
                && generationRules.Contains("Supported favorable changes or broad zero-open design coverage may remain highlights"),
                "Platform/Overall generation or independent review allows backlog-focused neutral Highlights or overstates poor performance.");
            Check(generationRules.Contains("SAMPLE PRESENTATION") && reviewRules.Contains("SAMPLE PRESENTATION")
                && generationRules.Contains("bold topic label, colon, one data-first sentence")
                && generationRules.Contains("Broad design coverage") && generationRules.Contains("Disposition TPT on target")
                && generationRules.Contains("Entity labels use supplied names")
                && !generationRules.Contains("Ingredient leads open CMFs") && !generationRules.Contains("Ingredient leads pending assessments")
                && !generationRules.Contains("Joint-leading ingredients")
                && generationRules.Contains("never decode design identifiers") && reviewRules.Contains("never decoded identifiers")
                && generationRules.Contains("Closure must retain 'including duplicates'")
                && reviewRules.Contains("Closure retains 'including duplicates' only for that supplied population")
                && generationRules.Contains("name 'accepted CMFs' when the denominator is accepted")
                && reviewRules.Contains("explicitly names accepted CMFs for an accepted denominator")
                && generationRules.Contains("NEVER percentages") && reviewRules.Contains("No percentages or repeated ratios")
                && generationRules.Contains("No inferred SLA compliance, stability or delivery success")
                && reviewRules.Contains("no invented cause, forecast, diagnosis, SLA breach"),
                "Sample presentation lost concise subject-led wording, explicit duplicate populations or factual counts-only safeguards.");
            Check(generationRules.Contains("ENTITY HEADINGS") && reviewRules.Contains("ENTITY HEADINGS")
                && generationRules.Contains("joint concentrations name all major members")
                && generationRules.Contains("Name distinctly high runners-up retained in the sentence too")
                && reviewRules.Contains("must appear in the title even when framed as a leading comparison")
                && generationRules.Contains("Use exact design Subject names in titles and sentences")
                && reviewRules.Contains("Preserve exact design Subject names in titles and sentences")
                && reviewRules.Contains("reject aliases or reformatted design identifiers")
                && generationRules.Contains("No generic Design concentration")
                && reviewRules.Contains("Reject generic titles or missing entities with significance=false")
                && reviewRules.Contains("require fresh generation, never repair prose")
                && generationRules.Contains("unequal counts are not joint-leading")
                && reviewRules.Contains("Never attribute causes from concentration alone"),
                "Summary headings lost exact entity names, joint-concentration distinctions or independent correction.");
            Check(generationRules.Contains("exclude full entity names") && reviewRules.Contains("exclude full entity names")
                && generationRules.Contains("at most 3 remaining words") && reviewRules.Contains("at most 3 remaining words")
                && generationRules.Contains("including connectors/category labels") && reviewRules.Contains("including connectors/category labels")
                && reviewRules.Contains("Reject overlong descriptions with significance=false"),
                "Entity subheadings permit elaborate descriptions or count the entity names against the descriptive limit.");
            Check(generationRules.Contains("Keep only leaders, tied leaders and distinctly high contributors")
                && reviewRules.Contains("Keep only leaders, tied leaders and distinctly high contributors")
                && generationRules.Contains("omit minor tails, low-count lists and 'no other design' comparisons")
                && reviewRules.Contains("Reject minor tails, low-count lists and 'no other design' comparisons with significance=false")
                && generationRules.Contains("Two records alone do not make a runner-up material")
                && reviewRules.Contains("minor omissions do not fail completeness or justify false sole-leader claims"),
                "Summary generation/review permits trivial comparison tails or forces minor rows into visible findings.");
            Check(generationRules.Contains("A unique current leader qualifies even with small counts or a one-record lead")
                && reviewRules.Contains("A unique current leader qualifies even with small counts or a one-record lead")
                && generationRules.Contains("Include a peculiar entity when supplied attributes/comparisons prove it stands out")
                && reviewRules.Contains("Do not reject a standout solely for small counts, near balance or a one-record lead")
                && generationRules.Contains("use kind=pattern for a factual standout without concentration claims")
                && reviewRules.Contains("complete requires these distinct standouts too")
                && generationRules.Contains("Queue-total/aging coverage does not replace entity standouts")
                && reviewRules.Contains("Reject arbitrary rankings in balanced ties, not a supported unique leader"),
                "A supported entity standout is suppressed as a weak concentration or mistaken for queue-aging coverage.");
            Check(generationRules.Contains("Pending ingredient sentences use '[subject] has N of D CMF pending assessments.'")
                && generationRules.Contains("Keep iDST-row aggregation in Source only, never visible prose")
                && reviewRules.Contains("Reject iDST-row aggregation in visible sentences with significance=false"),
                "Pending ingredient prose still narrates row aggregation or loses its concise supplied-count form.");
        }
        int backlogGenerations = 0, backlogReviews = 0;
        var correctedBacklog = CmfPlatformBriefingService.Generate(source, prompt => {
            backlogGenerations++;
            string draft = "{\"findings\":[" + finding("Leading open-CMF ingredient") + "]}";
            return backlogGenerations == 1 ? draft : draft.Replace("\"neutral\"", "\"attention\"").Replace("\"highlights\"", "\"focus\"");
        }, prompt => {
            backlogReviews++;
            Check(prompt.Contains("BACKLOG FOCUS"), "Backlog correction lost independent section-review rules.");
            var backlogPacket = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
            var backlogProposed = (Dictionary<string, object>)((object[])backlogPacket["Findings"])[0];
            bool correctSection = (string)backlogProposed["Section"] == "focus" && (string)backlogProposed["Tone"] == "attention";
            var backlogSkeleton = (Dictionary<string, object>)backlogPacket["ResponseSkeleton"];
            backlogSkeleton["complete"] = true;
            var backlogCheck = (Dictionary<string, object>)((object[])backlogSkeleton["checks"])[0];
            foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons" }) backlogCheck[flag] = true;
            backlogCheck["significance"] = correctSection;
            backlogCheck["reason"] = correctSection ? "" : "This current Open concentration is a backlog review priority and must be attention/focus, not a neutral Highlight.";
            return serializer.Serialize(backlogSkeleton);
        }, prompt => { throw new Exception("A single backlog finding requested ranking."); });
        Check(backlogGenerations == 2 && backlogReviews == 2 && correctedBacklog.Single().Section == "focus"
            && correctedBacklog.Single().Tone == "attention", "A backlog Highlight bypassed rejection or was relabeled without fresh generation and independent review.");
        var entitySource = new CmfPlatformBriefingSource { Platform = source.Platform, Scope = source.Scope, Hash = "entity-heading-fixture",
            CapturedAt = source.CapturedAt, Evidence = new List<CmfPlatformBriefingEvidence> { evidence,
                new CmfPlatformBriefingEvidence { Id = "hw", Section = "ComponentSummary", Subject = "HW", Kind = "distribution",
                    Context = new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                        new CmfFactMeasurement { Name = "Open", Current = 7, Denominator = 20, Unit = "records", CurrentDate = "2026-11-19" } } } } };
        foreach (string initialTitle in new[] { "Graphics leads open CMFs", "Graphics leads open CMFs ahead of HW" })
        {
            int entityGenerations = 0, entityReviews = 0;
            var namedComparison = CmfPlatformBriefingService.Generate(entitySource, prompt => {
                entityGenerations++;
                return serializer.Serialize(new { findings = new[] { new {
                    title = entityGenerations == 1 ? initialTitle : "Graphics and HW open concentration",
                    summary = "Graphics has 12 of 20 open records, ahead of HW with 7.", kind = "cluster", tone = "attention", section = "focus",
                    evidenceIds = new[] { "e1", "hw" }, support = new[] {
                        new { evidenceId = "e1", measurementNames = new[] { "Open" }, explanation = "Graphics has the largest count within this ingredient population." },
                        new { evidenceId = "hw", measurementNames = new[] { "Open" }, explanation = "HW is a substantial runner-up within the same open population." } } } } });
            }, prompt => {
                entityReviews++;
                var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
                var proposed = (Dictionary<string, object>)((object[])packet["Findings"])[0];
                string proposedTitle = (string)proposed["Title"];
                bool hasRunnerUp = proposedTitle.Contains("HW");
                string description = System.Text.RegularExpressions.Regex.Replace(proposedTitle, @"\b(?:Graphics|HW)\b", "");
                bool shortDescription = System.Text.RegularExpressions.Regex.Matches(description, @"\S+").Count <= 3;
                var entitySkeleton = (Dictionary<string, object>)packet["ResponseSkeleton"];
                entitySkeleton["complete"] = true;
                var entityCheck = (Dictionary<string, object>)((object[])entitySkeleton["checks"])[0];
                foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons" }) entityCheck[flag] = true;
                entityCheck["significance"] = hasRunnerUp && shortDescription;
                entityCheck["reason"] = !hasRunnerUp ? "The heading omits the material HW runner-up; regenerate with both names."
                    : !shortDescription ? "Regenerate with full entity names and at most three other words; leave the comparison in the sentence." : "";
                return serializer.Serialize(entitySkeleton);
            }, prompt => { throw new Exception("A single entity comparison requested ranking."); });
            Check(entityGenerations == 2 && entityReviews == 2 && namedComparison.Single().Title == "Graphics and HW open concentration"
                && namedComparison.Single().Summary == "Graphics has 12 of 20 open records, ahead of HW with 7.",
                "Missing names or elaborate descriptions survived review, or headings were rewritten without fresh generation/review.");
        }
        foreach (var crispCase in new[] {
            new { Subject = "HW", Section = "PendingAssessments", Measure = "CMF Pending", Count = 2m, Denominator = 5m, OtherCount = 1m,
                Others = new[] { "memory", "PnP", "TBT" }, Title = "HW leads pending assessments",
                Summary = "HW has 2 of 5 CMF pending assessments.", Tail = ", while memory, PnP and TBT each have 1." },
            new { Subject = "HW", Section = "PendingAssessments", Measure = "CMF Pending", Count = 2m, Denominator = 5m, OtherCount = 1m,
                Others = new[] { "memory", "PnP", "TBT" }, Title = "HW leads pending assessments",
                Summary = "HW has 2 of 5 CMF pending assessments.", Tail = " across all its iDST rows." },
            new { Subject = "dell_nvl_trinity", Section = "DesignSummary", Measure = "Open", Count = 3m, Denominator = 4m, OtherCount = 1m,
                Others = new[] { "Vendor_X-Board.2" }, Title = "dell_nvl_trinity highest open records",
                Summary = "dell_nvl_trinity has 3 of 4 open design-table records.", Tail = ", and no other design has more than 1." },
            new { Subject = "Sensor_module", Section = "ComponentSummary", Measure = "Open", Count = 1m, Denominator = 1m, OtherCount = 0m,
                Others = new[] { "Controller" }, Title = "Sensor_module open standout",
                Summary = "Sensor_module has 1 of 1 open component-table records.", Tail = ", while Controller has 0." } })
        {
            var crispEvidence = new[] { crispCase.Subject }.Concat(crispCase.Others).Select((subject, index) =>
                new CmfPlatformBriefingEvidence { Id = "crisp-" + index, Section = crispCase.Section, Subject = subject, Kind = "distribution",
                    Context = index == 0 && crispCase.Measure == "CMF Pending"
                        ? new Dictionary<string, string> { { "Aggregation basis", "All iDST rows for this ingredient; not additive with those rows or proof of aged/unassigned overlap." } }
                        : new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                        new CmfFactMeasurement { Name = crispCase.Measure, Current = index == 0 ? crispCase.Count : crispCase.OtherCount,
                            Denominator = crispCase.Denominator, Unit = "records", CurrentDate = source.CapturedAt } } }).ToList();
            var crispSource = new CmfPlatformBriefingSource { Platform = source.Platform, Scope = source.Scope,
                Hash = "crisp-" + crispCase.Subject, CapturedAt = source.CapturedAt, Evidence = crispEvidence };
            int crispGenerations = 0, crispReviews = 0;
            var crispFindings = CmfPlatformBriefingService.Generate(crispSource, prompt => {
                crispGenerations++;
                return serializer.Serialize(new { findings = new[] { new { title = crispCase.Title,
                    summary = crispGenerations == 1 ? crispCase.Summary.TrimEnd('.') + crispCase.Tail : crispCase.Summary,
                    kind = "pattern", tone = "attention", section = "focus", evidenceIds = crispEvidence.Select(item => item.Id),
                    support = crispEvidence.Select((item, index) => new { evidenceId = item.Id, measurementNames = new[] { crispCase.Measure },
                        explanation = index == 0 ? "This count establishes the largest contributor in the same population."
                            : "This lower count supports the leading comparison; minor detail belongs in Source." }) } } });
            }, prompt => {
                crispReviews++;
                Check(prompt.Contains("Reject minor tails, low-count lists"), "Crisp correction lost its independent review rule.");
                Check(prompt.Contains("Do not reject a standout solely for small counts, near balance or a one-record lead"),
                    "A factual leader or lone standout still requires dominant concentration.");
                Check(prompt.Contains("Reject iDST-row aggregation in visible sentences with significance=false"),
                    "Pending correction does not review unwanted aggregation narration.");
                var crispPacket = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
                var crispProposed = (Dictionary<string, object>)((object[])crispPacket["Findings"])[0];
                bool hasMinorTail = ((string)crispProposed["Summary"]).Contains(crispCase.Tail);
                var crispSkeleton = (Dictionary<string, object>)crispPacket["ResponseSkeleton"];
                crispSkeleton["complete"] = true;
                var crispCheck = (Dictionary<string, object>)((object[])crispSkeleton["checks"])[0];
                foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons" }) crispCheck[flag] = true;
                crispCheck["significance"] = !hasMinorTail;
                crispCheck["reason"] = hasMinorTail ? "Regenerate with only the leading count; keep this minor comparison in Source." : "";
                return serializer.Serialize(crispSkeleton);
            }, prompt => { throw new Exception("A single crisp finding requested ranking."); });
            Check(crispGenerations == 2 && crispReviews == 2 && crispFindings.Single().Summary == crispCase.Summary
                && crispFindings.Single().Title == crispCase.Title && crispFindings.Single().Kind == "pattern"
                && crispFindings.Single().Evidence.Count == crispEvidence.Count
                && (crispCase.Measure != "CMF Pending" || crispFindings.Single().Evidence[0].Text.Contains("Aggregation basis: All iDST rows")),
                "Minor tails survived review, Source detail was lost, or approved prose was edited without fresh generation/review.");
        }
        Check(writingPrompt.Contains("SPECIFIC SOURCES") && writingPrompt.Contains("exact supplied measurement Names")
            && sectionPrompt.Contains("Review Support and the rendered backing Evidence")
            && sectionPrompt.Contains("Approval hashes bind the exact source selections"), "Specific source selections and explanations bypass independent review.");
        Check(writingPrompt.Contains("plain executive CMF briefing") && writingPrompt.Contains("data-first")
            && writingPrompt.Contains("2-5 words") && writingPrompt.Contains("roughly 12-25 words")
            && writingPrompt.Contains("do not repeat the capture/as-of date")
            && writingPrompt.Contains("X and Y are placeholders, never facts")
            && writingPrompt.Contains("optional language examples, NEVER required findings or evidence")
            && writingPrompt.Contains("NOT its sample numbers, percentages, customer names, symptoms or judgments"), "Sample language style is not separated from source facts.");
        Check(writingPrompt.Contains("INTERPRETIVE SUBHEADINGS") && writingPrompt.Contains("modifier + topic")
            && writingPrompt.Contains("joint-leading require same-population rankings")
            && writingPrompt.Contains("Entity labels use supplied names") && writingPrompt.Contains("NEVER force them from an absolute count or duration")
            && writingPrompt.Contains("When no modifier is supported, use a factual topic label instead"),
            "Subheadings do not convey an evidence-backed hint while retaining judgment safeguards.");
        Check(writingPrompt.Contains("Keep title and sentence population-equivalent")
            && writingPrompt.Contains("Impl/Verified means implemented or verified in BOTH title and sentence")
            && writingPrompt.Contains("REQUESTED METRIC COVERAGE")
            && writingPrompt.Contains("Add only missing coverage")
            && writingPrompt.Contains("TPT THRESHOLD ASSESSMENT")
            && writingPrompt.Contains("If no TPT card qualifies, there is no TPT point")
            && writingPrompt.Contains("Do not duplicate it with a separate recap")
            && sectionPrompt.Contains("no duplicate coverage and no implied performance judgment")
            && writingPrompt.Contains("Add only missing coverage that is independently material")
            && writingPrompt.Contains("Do not force standalone Open/pending balances"),
            "Requested TPT/open/pending coverage loses category, deduplication or independent review safeguards.");
        Check(writingPrompt.Contains("SINGLE-PROMINENT-ITEM") && sectionPrompt.Contains("SINGLE-PROMINENT-ITEM")
            && writingPrompt.Contains("each eligible TPT measure") && sectionPrompt.Contains("each eligible TPT measure")
            && writingPrompt.Contains("CustomerIssues uses issue-based bullets")
            && writingPrompt.Contains("IDST SCOPE: Platform excludes")
            && sectionPrompt.Contains("IDST SCOPE: Platform excludes")
            && sectionPrompt.Contains("Reject inferred aged/unassigned overlap"), "Pending-queue synthesis lacks independent overlap and fact-preservation safeguards.");
        var pendingEvidence = new CmfPlatformBriefingEvidence { Id = "pending", Section = "PendingSummary",
            Subject = "Pending queue coverage", Kind = "current", Context = new Dictionary<string, string>(),
            Measurements = new List<CmfFactMeasurement> {
                new CmfFactMeasurement { Name = "Aged pending", Current = 4, Denominator = 6, Unit = "records", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "Pending without iDST", Current = 2, Denominator = 6, Unit = "records", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "Pending age threshold", Current = 2, Unit = "days", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "Oldest pending age", Current = 17, Unit = "days", CurrentDate = "2026-11-19" } } };
        source.Evidence.Add(pendingEvidence);
        Func<string, string[], string> queueFinding = (summary, names) => serializer.Serialize(new {
            title = names.Contains("Aged pending") ? "Aged CMF pending" : "Pending assignments",
            summary, kind = "pattern", tone = "attention", section = "focus", evidenceIds = new[] { "pending" },
            support = new[] { new { evidenceId = "pending", measurementNames = names,
                explanation = "The selected fields describe concerns in this pending queue without identifying overlap between assessments." } } });
        string agedQueue = queueFinding("4 of 6 CMF pending assessments are stale (> 2 days).",
            new[] { "Aged pending", "Pending age threshold" });
        string assignments = queueFinding("2 of 6 pending assessments have no iDST assigned.", new[] { "Pending without iDST" });
        var overallQueueSource = new CmfPlatformBriefingSource { Platform = CmfPlatformCatalog.OverallPlatformTableName, Scope = source.Scope,
            Hash = source.Hash, CapturedAt = source.CapturedAt, Evidence = source.Evidence };
        var splitQueue = CmfPlatformBriefingService.Validate(overallQueueSource, "{\"findings\":[" + agedQueue + "," + assignments + "]}");
        Check(splitQueue.Count == 2 && splitQueue[0].Evidence[0].Text.Contains("Field Aged pending: 4 records")
            && splitQueue[0].Title == "Aged CMF pending"
            && splitQueue[0].Summary == "4 of 6 CMF pending assessments are stale (> 2 days)."
            && splitQueue[1].Evidence[0].Text.Contains("Field Pending without iDST: 2 records")
            && !splitQueue[0].Evidence[0].Text.Contains("Field Pending without iDST"),
            "Distinct aging/assignment observations were merged or lost their exact Sources.");
        foreach (var queueSource in new[] { source, overallQueueSource })
        {
            var staleFinding = CmfPlatformBriefingService.Validate(queueSource, "{\"findings\":[" + agedQueue + "]}");
            foreach (string prompt in new[] { CmfPlatformBriefingService.AnalysisPrompt(queueSource),
                CmfPlatformBriefingService.VerificationPrompt(queueSource, staleFinding) })
                Check(prompt.Contains("exact title 'Aged CMF pending'") && prompt.Contains("no extra words")
                    && prompt.Contains("N of D CMF pending assessments are stale (> T days)")
                    && prompt.Contains("Aged pending, its denominator and Pending age threshold"),
                    "Pending aging wording lost its requested title, strict threshold or evidence-bound figures.");
        }
        bool wrongStaleThresholdRejected = false;
        try { CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + queueFinding(
            "4 of 6 CMF pending assessments are stale (> 3 days).", new[] { "Aged pending", "Pending age threshold" }) + "]}"); }
        catch (InvalidOperationException exception) { wrongStaleThresholdRejected = exception.Data["RejectedNumber"] is decimal; }
        Check(wrongStaleThresholdRejected, "An unsupported stale-pending threshold was accepted.");
        bool combinedQueueRejected = false;
        try { CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + queueFinding(
            "4 of 6 pending assessments exceed the 2-day threshold, and 2 have no iDST assigned.",
            new[] { "Aged pending", "Pending age threshold", "Pending without iDST" }) + "]}"); }
        catch (InvalidOperationException exception) { combinedQueueRejected = exception.Data["RejectedCheck"] as string == "summary-single-item"; }
        Check(combinedQueueRejected, "A compound pending aging/assignment Summary bullet was accepted.");
        bool oldestRejected = false;
        try { CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + queueFinding(
            "4 of 6 pending assessments exceed the 2-day threshold, the oldest is 17 days old, and 2 have no iDST assigned.",
            new[] { "Aged pending", "Pending age threshold", "Oldest pending age", "Pending without iDST" }) + "]}"); }
        catch (InvalidOperationException exception) { oldestRejected = exception.Data["RejectedCheck"] as string == "pending-age-detail"; }
        Check(oldestRejected && writingPrompt.Contains("omit oldest age from bullets and selected Sources")
            && sectionPrompt.Contains("its omission does not fail completeness"), "Oldest pending age is still required or allowed in briefings.");
        Check(CmfPlatformBriefingService.Validate(overallQueueSource, "{\"findings\":[" + assignments + "]}").Count == 1
            && CmfPlatformBriefingService.Validate(overallQueueSource, "{\"findings\":[" + finding("Graphics open backlog") + "," + assignments + "]}").Count == 2,
            "Standalone assignments were suppressed without another same-queue finding or merged with unrelated Open records.");
        source.Evidence.Remove(pendingEvidence);
        var tptEvidence = new CmfPlatformBriefingEvidence { Id = "e2", Section = "TPT", Subject = "CMF Overall TPT", Kind = "current",
            Context = new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                new CmfFactMeasurement { Name = "CMF Overall TPT", Current = 38, Unit = "days", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "TPT target", Current = 21, Unit = "days", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "Difference from TPT target", Current = 17, Unit = "days", CurrentDate = "2026-11-19" } } };
        source.Evidence.Add(tptEvidence);
        const string tptSummary = "Overall CMF turnaround exceeds target by 17 days.";
        string tptOutput = serializer.Serialize(new { findings = new[] { new { title = "Slow overall turnaround", summary = tptSummary,
            kind = "pattern", tone = "attention", section = "focus", evidenceIds = new[] { "e2" },
            support = new[] { new { evidenceId = "e2", measurementNames = new[] { "CMF Overall TPT", "TPT target", "Difference from TPT target" },
                explanation = "The overall average is materially above its own turnaround reference, establishing an adverse threshold comparison rather than a historical change." } } } } });
        var tptFindings = CmfPlatformBriefingService.Validate(source, tptOutput);
        Check(tptFindings.Count == 1 && tptFindings[0].Evidence[0].Text.Contains("Field CMF Overall TPT: 38 days")
            && tptFindings[0].Evidence[0].Text.Contains("Field TPT target: 21 days")
            && !tptFindings[0].Evidence[0].Text.Contains("Field Open"), "TPT assessment must retain current and reference fields without unrelated counts.");
        var resolution = new CmfPlatformBriefingEvidence { Id = "e3", Section = "TPT", Subject = "CMF Resolution TPT", Kind = "current",
            Context = new Dictionary<string, string>(), Measurements = new List<CmfFactMeasurement> {
                new CmfFactMeasurement { Name = "CMF Resolution TPT", Current = 25, Unit = "days", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "TPT target", Current = 18, Unit = "days", CurrentDate = "2026-11-19" },
                new CmfFactMeasurement { Name = "Difference from TPT target", Current = 7, Unit = "days", CurrentDate = "2026-11-19" } } };
        source.Evidence.Add(resolution);
        string resolutionOutput = tptOutput.Replace("\"e2\"", "\"e3\"").Replace("CMF Overall TPT", "CMF Resolution TPT")
            .Replace("Slow overall turnaround", "Slow resolution turnaround").Replace("Overall CMF turnaround", "Resolution CMF turnaround")
            .Replace("exceeds target by 17 days", "exceeds target by 7 days")
            .Replace("overall average", "resolution average");
        var overallPacket = (Dictionary<string, object>)serializer.DeserializeObject(tptOutput);
        var resolutionPacket = (Dictionary<string, object>)serializer.DeserializeObject(resolutionOutput);
        Check(CmfPlatformBriefingService.Validate(source, serializer.Serialize(new {
            findings = ((object[])overallPacket["findings"]).Concat((object[])resolutionPacket["findings"]).ToArray() })).Count == 2,
            "Distinct qualifying TPT measures with the same direction were merged or omitted.");
        string combinedTpt = serializer.Serialize(new { findings = new[] { new {
            title = "Slow CMF turnaround", summary = "Overall and resolution turnaround are slower than expected.",
            kind = "pattern", tone = "attention", section = "focus", evidenceIds = new[] { "e2", "e3" }, support = new[] {
                new { evidenceId = "e2", measurementNames = new[] { "CMF Overall TPT", "TPT target" }, explanation = "Overall exceeds its own reference." },
                new { evidenceId = "e3", measurementNames = new[] { "CMF Resolution TPT", "TPT target" }, explanation = "Resolution exceeds its own reference." } } } } });
        bool combinedTptRejected = false;
        try { CmfPlatformBriefingService.Validate(source, combinedTpt); }
        catch (InvalidOperationException exception) { combinedTptRejected = exception.Data["RejectedCheck"] as string == "tpt-gap"; }
        Check(combinedTptRejected && writingPrompt.Contains("One assessment per qualifying measure")
            && !writingPrompt.Contains("grouping only compatible good/bad directions"), "Independent TPT observations still share one Summary bullet.");
        source.Evidence.Remove(resolution);
        string tptReview = CmfPlatformBriefingService.VerificationPrompt(source, tptFindings);
        var reviewPacket = (Dictionary<string, object>)serializer.DeserializeObject(tptReview.Substring(tptReview.IndexOf('\n') + 1));
        var skeleton = (Dictionary<string, object>)reviewPacket["ResponseSkeleton"];
        skeleton["complete"] = true;
        var check = (Dictionary<string, object>)((object[])skeleton["checks"])[0];
        foreach (string flag in new[] { "supported", "numbers", "scope", "comparisons", "significance" }) check[flag] = true;
        check["reason"] = "";
        CmfPlatformBriefingService.ValidateGrounding(source, tptFindings, serializer.Serialize(skeleton));
        check["significance"] = false;
        check["reason"] = "This duplicates an existing TPT finding.";
        bool duplicateTptRejected = false;
        try { CmfPlatformBriefingService.ValidateGrounding(source, tptFindings, serializer.Serialize(skeleton)); }
        catch (InvalidOperationException) { duplicateTptRejected = true; }
        Check(duplicateTptRejected, "Requested coverage bypassed independent rejection of duplicate TPT findings.");
        Check(writingPrompt.Contains("EXCLUDED MILESTONE SHARES") && sectionPrompt.Contains("Omission of those points does not fail completeness")
            && writingPrompt.Contains("TPT title must be qualitative")
            && tptReview.Contains("exact Difference from TPT target in days"), "Exact-gap TPT or excluded milestone-share rules are missing from generation/review.");
        foreach (var invalidTpt in new[] {
            new { Summary = "CMF Overall TPT averages 38 days against its 21-day target.", Reason = "tpt-gap" },
            new { Summary = "Overall turnaround is seventeen days above its target.", Reason = "tpt-gap" },
            new { Summary = "Overall CMF turnaround for accepted CMF records, including duplicates, exceeds target by 17 days.", Reason = "tpt-wording" } })
        {
            bool numericRejected = false;
            try { CmfPlatformBriefingService.Validate(source, tptOutput.Replace(tptSummary, invalidTpt.Summary)); }
            catch (InvalidOperationException exception) { numericRejected = exception.Data["RejectedCheck"] as string == invalidTpt.Reason; }
            Check(numericRejected, "An unsupported numeric or verbose TPT sentence bypassed the exact-gap and length gates.");
        }
        evidence.Section = "MilestoneSummary";
        evidence.Measurements[0].Name = "Unique CMFs";
        string milestoneOutput = "{\"findings\":[" + finding("Dominant milestone share").Replace("Open", "Unique CMFs") + "]}";
        bool milestoneRejected = false;
        try { CmfPlatformBriefingService.Validate(source, milestoneOutput); }
        catch (InvalidOperationException exception) { milestoneRejected = exception.Data["RejectedCheck"] as string == "milestone-share"; }
        Check(milestoneRejected, "A pure milestone-share point bypassed the scope gate.");
        evidence.Measurements[0].Name = "Open";
        Check(CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + finding("Open milestone exposure") + "]}").Count == 1,
            "Milestone context for a distinct open exposure was discarded.");
        evidence.Section = "ComponentSummary";
        evidence.Measurements.Add(new CmfFactMeasurement { Name = "Open LOS subset", Current = 1,
            Denominator = 1, Unit = "records", CurrentDate = "2026-11-19" });
        string losOnly = serializer.Serialize(new { findings = new[] { new {
            title = "Open LOS in Graphics", summary = "Graphics has 1 open LOS record.", kind = "pattern",
            tone = "attention", section = "focus", evidenceIds = new[] { "e1" },
            support = new[] { new { evidenceId = "e1", measurementNames = new[] { "Open LOS subset" },
                explanation = "The LOS record is a subset of the ingredient's open backlog." } } } } });
        bool losRejected = false;
        try { CmfPlatformBriefingService.Validate(source, losOnly); }
        catch (InvalidOperationException exception) { losRejected = exception.Data["RejectedCheck"] as string == "los-only"; }
        Check(losRejected, "A dedicated LOS-only item bypassed the requested scope gate.");
        string combinedLos = losOnly.Replace("Open LOS in Graphics", "Graphics open backlog")
            .Replace("Graphics has 1 open LOS record.", "Graphics has 12 of 20 Open records, including 1 LOS record.")
            .Replace("\"measurementNames\":[\"Open LOS subset\"]", "\"measurementNames\":[\"Open\",\"Open LOS subset\"]");
        Check(CmfPlatformBriefingService.Validate(source, combinedLos).Count == 1,
            "Relevant LOS detail was removed from a broader Open finding.");
        evidence.Measurements.RemoveAt(1);
        Check(writingPrompt.Contains("NO STANDALONE LOS") && sectionPrompt.Contains("Omission of standalone LOS does not fail completeness")
            && writingPrompt.Contains("Avoid exposure in titles"), "Standalone LOS or repetitive exposure wording is still encouraged.");
        foreach (string invalid in new[] { tptOutput.Replace("\"attention\"", "\"neutral\"").Replace("\"focus\"", "\"highlights\""),
            tptOutput.Replace("\"CMF Overall TPT\",\"TPT target\"", "\"CMF Overall TPT\"") })
        {
            bool rejected = false;
            try { CmfPlatformBriefingService.Validate(source, invalid); }
            catch (InvalidOperationException exception) { rejected = exception.Data["RejectedCheck"] as string == "tpt-target"; }
            Check(rejected, "A raw TPT recap or adverse assessment in Highlights bypassed the threshold gate.");
        }
        tptEvidence.Measurements[0].Current = 21;
        tptEvidence.Measurements[2].Current = 0;
        bool nearTargetRejected = false;
        try { CmfPlatformBriefingService.Validate(source, tptOutput); }
        catch (InvalidOperationException exception) { nearTargetRejected = exception.Data["RejectedCheck"] as string == "tpt-target"; }
        Check(nearTargetRejected, "An at-target TPT point was incorrectly accepted as adverse.");
        string atTarget = tptOutput.Replace("Slow overall turnaround", "On-target overall turnaround")
            .Replace(tptSummary, "Overall CMF turnaround meets its target.")
            .Replace("The overall average is materially above its own turnaround reference, establishing an adverse threshold comparison rather than a historical change.",
                "The positive overall average equals its own current target without establishing historical stability.")
            .Replace("\"attention\"", "\"neutral\"").Replace("\"focus\"", "\"highlights\"");
        Check(CmfPlatformBriefingService.Validate(source, atTarget).Single().Section == "highlights",
            "A positive at-target TPT card cannot qualify as a neutral Highlight with its own target.");
        tptEvidence.Subject = "CMF Disposition TPT";
        tptEvidence.Measurements[0].Name = "CMF Disposition TPT";
        tptEvidence.Measurements[0].Current = 1;
        tptEvidence.Measurements[1].Current = 2;
        tptEvidence.Measurements[2].Current = 1;
        string favorable = tptOutput.Replace("Slow overall turnaround", "Faster disposition turnaround")
            .Replace("CMF Overall TPT", "CMF Disposition TPT")
            .Replace(tptSummary, "CMF disposition is faster than expected.")
            .Replace("The overall average is materially above its own turnaround reference, establishing an adverse threshold comparison rather than a historical change.",
                "The disposition average is materially below its own reference, establishing a favorable threshold comparison rather than a historical change.")
            .Replace("\"attention\"", "\"progress\"").Replace("\"focus\"", "\"highlights\"");
        Check(CmfPlatformBriefingService.Validate(source, favorable).Single().Section == "highlights",
            "A materially below-reference TPT assessment was not accepted as favorable.");
        tptEvidence.Section = "OEM Summary";
        bool oemSubstituteRejected = false;
        try { CmfPlatformBriefingService.Validate(source, favorable); }
        catch (InvalidOperationException exception) { oemSubstituteRejected = exception.Data["RejectedCheck"] as string == "tpt-target"; }
        Check(oemSubstituteRejected, "An OEM duration was substituted for a headline TPT card.");
        source.Evidence.Remove(tptEvidence);
        foreach (var test in new[] {
            new { Label = "CMF Disposition TPT", Current = 0m, Target = 2m, Eligible = false },
            new { Label = "CMF Disposition TPT", Current = 1m, Target = 2m, Eligible = true },
            new { Label = "CMF Disposition TPT", Current = 2m, Target = 2m, Eligible = true },
            new { Label = "CMF Disposition TPT", Current = 3m, Target = 2m, Eligible = true },
            new { Label = "CMF Resolution TPT", Current = 14m, Target = 18m, Eligible = true },
            new { Label = "CMF Resolution TPT", Current = 15m, Target = 18m, Eligible = true },
            new { Label = "CMF Resolution TPT", Current = 18m, Target = 18m, Eligible = true },
            new { Label = "CMF Resolution TPT", Current = 19m, Target = 18m, Eligible = false },
            new { Label = "CMF Resolution TPT", Current = 21m, Target = 18m, Eligible = false },
            new { Label = "CMF Resolution TPT", Current = 22m, Target = 18m, Eligible = true },
            new { Label = "CMF Overall TPT", Current = 16m, Target = 21m, Eligible = true },
            new { Label = "CMF Overall TPT", Current = 17m, Target = 21m, Eligible = true },
            new { Label = "CMF Overall TPT", Current = 21m, Target = 21m, Eligible = true },
            new { Label = "CMF Overall TPT", Current = 25m, Target = 21m, Eligible = false },
            new { Label = "CMF Overall TPT", Current = 26m, Target = 21m, Eligible = true },
            new { Label = "CMF Overall TPT", Current = 38m, Target = 21m, Eligible = true } })
        {
            var snapshot = new CmfPanelSnapshotCache { Platform = "NVL_H", Configuration = "fixture", DateScope = "all",
                CapturedAt = new DateTime(2026, 11, 19), Data = new Dictionary<string, object> {
                    { "PlatformLabel", "NVL H" }, { "TptFacts", new object[] { new Dictionary<string, object> {
                        { "Label", test.Label }, { "Value", test.Current } } } } } };
            var projected = CmfPlatformBriefingService.BuildSource(snapshot).Evidence.Where(item => item.Section == "TPT").ToList();
            Check((projected.Count == 1) == test.Eligible, "Incorrect TPT eligibility for " + test.Label + " at " + test.Current + " days.");
            if (test.Eligible)
                Check(projected[0].Measurements.Single(item => item.Name == "TPT target").Current == test.Target
                    && projected[0].Measurements.Single(item => item.Name == "Difference from TPT target").Current == Math.Abs(test.Current - test.Target),
                    "TPT source lost its specific threshold or explicit gap.");
        }
        Check(sectionPrompt.Contains("Check the modifier independently") && sectionPrompt.Contains("These factual modifiers do not need a performance benchmark")
            && sectionPrompt.Contains("Reject unsupported or sensational modifiers, not valid descriptive ones"),
            "Independent review confuses descriptive modifiers with unsupported performance judgments.");
        Check(CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + finding("Dominant Graphics concentration") + "]}").Count == 1,
            "A concise interpretive subheading was rejected by format validation.");
        Check(sectionPrompt.Contains("Also check clarity") && sectionPrompt.Contains("not source evidence")
            && sectionPrompt.Contains("capture-date boilerplate")
            && sectionPrompt.Contains("essential population, date, unit or estimated-history qualifications"), "Independent review does not cover plain-language clarity and retained qualifications.");
        Check(CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + finding("Open CMFs by ingredient").Replace("Graphics contains 12 of 20 Open records.", "Graphics leads with 12 of 20 Open CMFs.") + "]}").Count == 1,
            "Concise, evidence-grounded sample-style wording was rejected.");
        string changes = output.Replace("\"cluster\"", "\"change\"");
        bool noYesterdayRejected = false;
        try { CmfPlatformBriefingService.Validate(source, changes); }
        catch (InvalidOperationException exception) { noYesterdayRejected = exception.Data["RejectedCheck"] as string == "day-comparison"; }
        Check(noYesterdayRejected, "A current-only distribution was treated as a previous-day change.");
        evidence.Kind = "comparison";
        evidence.Context["Compared with"] = "2026-11-18";
        evidence.Measurements[0].Previous = 8;
        evidence.Measurements[0].PreviousDate = "2026-11-18";
        Check(CmfPlatformBriefingService.Validate(source, changes).Count == 17, "Valid dynamic change sections were rejected.");
        evidence.Kind = "distribution";
        evidence.Context.Clear();
        evidence.Measurements[0].Previous = null;
        evidence.Measurements[0].PreviousDate = null;
        Check(CmfPlatformBriefingService.Validate(source, output.Replace("\"highlights\"", "\"focus\"").Replace("\"neutral\"", "\"attention\"")).Count == 17,
            "Valid Lowlights were rejected.");
        foreach (string invalidSentence in new[] { "Graphics contains 12 of 20 Open records. Review Graphics.", "Graphics contains 12 of 20 Open records! Review Graphics.",
            "Graphics contains 12 of 20 Open records.\nReview Graphics.", "Graphics contains 12 of 20 Open records", new string('x', 281) + ".",
            string.Join(" ", Enumerable.Repeat("Graphics", 36)) + "." })
        {
            bool sentenceRejected = false;
            try { CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + finding("Graphics concentration").Replace("Graphics contains 12 of 20 Open records.", invalidSentence.Replace("\n", "\\n")) + "]}"); }
            catch (InvalidOperationException exception) { sentenceRejected = exception.Data["RejectedCheck"] as string == "sentence-format"; }
            Check(sentenceRejected, "A long or multi-sentence bullet passed format validation.");
        }
        evidence.Measurements.Add(new CmfFactMeasurement { Name = "Assessment duration", Current = 2.5m, Unit = "days", CurrentDate = "2026-11-19" });
        Check(CmfPlatformBriefingService.Validate(source, "{\"findings\":[" + finding("Assessment duration")
            .Replace("Graphics contains 12 of 20 Open records.", "Assessment takes 2.5 days.")
            .Replace("\"measurementNames\":[\"Open\"]", "\"measurementNames\":[\"Assessment duration\"]")
            .Replace("Graphics holds a majority of the open records.", "This average measures assessment duration.") + "]}").Count == 1,
            "A decimal point was mistaken for a sentence boundary.");
        evidence.Measurements.RemoveAt(1);
        Check(CmfPlatformBriefingService.Validate(source, "{\"findings\":[]}").Count == 0, "Empty material findings were rejected.");
        foreach (string invalid in new[] { output.Replace("12 of 20", "999 of 20"), output.Replace("12 of 20 Open records.", "60 percent of Open records."),
            output.Replace("\"e1\"", "\"unknown\""), output.Replace("\"cluster\"", "\"trend\""), output.Replace("Graphics contains", "The displayed Graphics contains"),
            output.Replace("\"highlights\"", "\"unknown\""), output.Replace("\"section\":\"highlights\",", ""),
            output.Replace("\"highlights\"", "\"focus\""), output.Replace("\"highlights\"", "\"changes\""),
            output.Replace("\"neutral\"", "\"attention\""), output.Replace("\"support\":", "\"unexpected\":"),
            output.Replace("\"measurementNames\":[\"Open\"]", "\"measurementNames\":[\"unknown\"]"),
            output.Replace("\"measurementNames\":[\"Open\"]", "\"measurementNames\":[\"Open\",\"Open\"]"),
            output.Replace("Graphics holds a majority of the open records.", "Graphics has 999 records.") })
        {
            bool rejected = false;
            try { CmfPlatformBriefingService.Validate(source, invalid); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "Unsupported platform prose or citations passed validation.");
        }
        Func<string, string> approval = prompt => {
            var packet = serializer.DeserializeObject(prompt.Substring(prompt.IndexOf("\n", StringComparison.Ordinal) + 1)) as Dictionary<string, object>;
            var findings = (object[])packet["Findings"];
            return serializer.Serialize(new { complete = true, checks = findings.Cast<Dictionary<string, object>>().Select((item, index) => new {
                index = index, hash = item["Hash"], supported = true, numbers = true, scope = true, comparisons = true, significance = true, reason = "" }).ToArray() });
        };
        int generations = 0;
        var progress = new List<int>();
        var generated = CmfPlatformBriefingService.Generate(source, prompt => {
            Check(progress.Last() == 5, "Analysis progress advanced before model output was available.");
            generations++; return output;
        }, prompt => {
            Check(progress.Last() == 45, "Verification stage did not advance progress after validated model output.");
            return approval(prompt);
        }, prompt => { throw new Exception("Unexpected rank call for a single batch."); }, progress.Add);
        Check(generated.Count == 17 && generations == 1, "Generation truncated validated findings.");
        Check(progress.SequenceEqual(new[] { 5, 45, 85, 98 }), "Single-batch progress did not follow actual completed stages.");
        int corrections = 0;
        int trendCorrections = 0;
        var classified = CmfPlatformBriefingService.Generate(source, prompt => {
            trendCorrections++;
            if (trendCorrections == 1) return output.Replace("\"cluster\"", "\"trend\"");
            Check(prompt.Contains("\"RejectedCheck\":\"trend-support\"") && prompt.Contains("correct every ineligible trend classification"), "Trend correction received no actionable classification feedback.");
            return output;
        }, approval, prompt => "");
        Check(trendCorrections == 2 && classified.Count == 17, "Corrected classification skipped verification or discarded supported findings.");
        var corrected = CmfPlatformBriefingService.Generate(source, prompt => {
            corrections++;
            if (corrections == 1) return output.Replace("12 of 20", "999 of 20");
            Check(prompt.Contains("\"Code\":\"unsupported-number\"") && prompt.Contains("\"RejectedNumber\":999"), "Numeric correction received no discriminating rejection feedback.");
            return output;
        }, approval, prompt => "");
        Check(corrections == 2 && corrected.Count == 17, "A supported numeric correction was not independently verified.");
        int reviews = 0, semanticCorrections = 0;
        progress.Clear();
        var supportedCorrection = CmfPlatformBriefingService.Generate(source, prompt => {
            semanticCorrections++;
            if (semanticCorrections > 1) Check(prompt.Contains("\"RejectedCheck\":\"numbers\"") && prompt.Contains("Quantity role mismatch")
                && prompt.Contains("\"PreviousDraft\":") && prompt.Contains("\"RejectedChecks\":")
                && prompt.Contains("\\\"Index\\\":16") && prompt.Contains("Observation aaaaaaaaaaaaaaaaa"), "Semantic rejection lost its complete correction evidence or supported draft observations.");
            return output;
        }, prompt => { reviews++; return reviews == 1 ? approval(prompt).Replace("\"numbers\":true", "\"numbers\":false").Replace("\"reason\":\"\"", "\"reason\":\"Quantity role mismatch\"") : approval(prompt); }, prompt => "", progress.Add);
        Check(reviews == 2 && semanticCorrections == 2 && supportedCorrection.Count == 17, "Corrected prose bypassed fresh independent verification.");
        Check(progress.SequenceEqual(new[] { 5, 45, 85, 98 }), "Corrective generation reset or falsely completed progress.");
        int mixedGenerations = 0, mixedReviews = 0;
        progress.Clear();
        var mixedCorrection = CmfPlatformBriefingService.Generate(source, prompt => {
            mixedGenerations++;
            if (mixedGenerations == 1) return output.Replace("12 of 20", "999 of 20");
            if (mixedGenerations == 3) Check(prompt.Contains("Quantity role mismatch") && prompt.Contains("\"PreviousDraft\":"), "Local repair consumed or lost independent verification feedback.");
            return output;
        }, prompt => { mixedReviews++; return mixedReviews == 1 ? approval(prompt).Replace("\"numbers\":true", "\"numbers\":false").Replace("\"reason\":\"\"", "\"reason\":\"Quantity role mismatch\"") : approval(prompt); }, prompt => "", progress.Add);
        Check(mixedGenerations == 3 && mixedReviews == 2 && mixedCorrection.Count == 17, "A local validation repair prevented the one bounded semantic correction.");
        Check(progress.SequenceEqual(new[] { 5, 45, 85, 98 }), "Separate bounded corrections reset progress or published a draft.");
        bool stale = false;
        try { CmfPlatformBriefingService.ValidateGrounding(source, validated, "{\"complete\":true,\"checks\":[]}"); }
        catch (InvalidOperationException) { stale = true; }
        Check(stale, "Incomplete independent verification was accepted.");
        string boundApproval = approval(CmfPlatformBriefingService.VerificationPrompt(source, validated));
        validated[0].Section = "focus";
        bool changedSectionRejected = false;
        try { CmfPlatformBriefingService.ValidateGrounding(source, validated, boundApproval); }
        catch (InvalidOperationException) { changedSectionRejected = true; }
        Check(changedSectionRejected, "A changed section reused approval for a different finding.");
        validated[0].Section = "highlights";
        string originalExplanation = validated[0].Support[0].Explanation;
        validated[0].Support[0].Explanation = "A changed rationale.";
        bool changedSourceRejected = false;
        try { CmfPlatformBriefingService.ValidateGrounding(source, validated, boundApproval); }
        catch (InvalidOperationException) { changedSourceRejected = true; }
        Check(changedSourceRejected, "A changed source explanation reused an earlier approval.");
        validated[0].Support[0].Explanation = originalExplanation;
        foreach (string badReview in new[] { "not JSON", "{\"complete\":true,\"checks\":[]}",
            boundApproval.Replace("\"supported\":true", "\"supported\":null"),
            boundApproval.Replace("\"hash\":\"", "\"hash\":\"wrong-") })
        {
            int formatGenerations = 0, formatReviews = 0;
            string originalFindingsPacket = null;
            var formatRecovered = CmfPlatformBriefingService.Generate(source, prompt => { formatGenerations++; return output; }, prompt => {
                formatReviews++;
                var packet = (Dictionary<string, object>)serializer.DeserializeObject(prompt.Substring(prompt.IndexOf('\n') + 1));
                string findingsPacket = serializer.Serialize(packet["Findings"]);
                if (formatReviews == 1) { originalFindingsPacket = findingsPacket; return badReview; }
                Check(prompt.Contains("UNCHANGED draft") && prompt.Contains("Controlled error data")
                    && originalFindingsPacket == findingsPacket, "Review-only recovery changed findings, hashes or lost contract feedback.");
                return approval(prompt);
            }, prompt => "");
            Check(formatGenerations == 1 && formatReviews == 2 && formatRecovered.Count == 17,
                "Malformed review regenerated prose or bypassed complete independent approval.");
        }
        int coverageGenerations = 0, coverageReviews = 0;
        const string missingCoverage = "Missing material CMF Overall TPT evidence; regenerate its supported assessment.";
        var coverageRecovered = CmfPlatformBriefingService.Generate(source, prompt => {
            coverageGenerations++;
            if (coverageGenerations > 1) Check(prompt.Contains(missingCoverage) && prompt.Contains("PreviousDraft"),
                "Missing coverage lost its actionable review reason or original draft.");
            return output;
        }, prompt => {
            coverageReviews++;
            if (coverageReviews > 1) return approval(prompt);
            var coveragePacket = (Dictionary<string, object>)serializer.DeserializeObject(approval(prompt));
            coveragePacket["complete"] = false;
            coveragePacket["coverageReason"] = missingCoverage;
            return serializer.Serialize(coveragePacket);
        }, prompt => "");
        Check(coverageGenerations == 2 && coverageReviews == 2 && coverageRecovered.Count == 17,
            "A completeness rejection retried only the verifier or was accepted without fresh generation/review.");
        int unexplainedGenerations = 0, unexplainedReviews = 0;
        var explainedCoverage = CmfPlatformBriefingService.Generate(source, prompt => { unexplainedGenerations++; return output; }, prompt => {
            unexplainedReviews++;
            if (unexplainedReviews == 3) return approval(prompt);
            var coveragePacket = (Dictionary<string, object>)serializer.DeserializeObject(approval(prompt));
            coveragePacket["complete"] = false;
            if (unexplainedReviews == 2) coveragePacket["coverageReason"] = missingCoverage;
            return serializer.Serialize(coveragePacket);
        }, prompt => "");
        Check(unexplainedGenerations == 2 && unexplainedReviews == 3 && explainedCoverage.Count == 17,
            "An unexplained completeness rejection was accepted or consumed the semantic correction budget.");
        int rejectedGenerations = 0, rejectedReviews = 0;
        bool unsupported = false;
        try { CmfPlatformBriefingService.Generate(source, prompt => { rejectedGenerations++; return output; }, prompt => {
            rejectedReviews++; return "{\"complete\":false,\"checks\":[]}";
        }, prompt => ""); }
        catch (InvalidOperationException exception) { unsupported = exception.Data["RejectedCheck"] as string == "review-count"
            && (exception.Data["Reason"] as string).Contains("17 checks"); }
        Check(unsupported && rejectedGenerations == 1 && rejectedReviews == 2, "Malformed review retries were unbounded or an unapproved draft was published.");
        evidence.Context["Fixture padding"] = new string('x', 24000);
        source.Evidence.Add(new CmfPlatformBriefingEvidence { Id = "e2", Section = "DesignSummary", Subject = "Design", Kind = "distribution",
            Context = new Dictionary<string, string>(evidence.Context), Measurements = evidence.Measurements });
        int batches = 0, batchReviews = 0;
        progress.Clear();
        var batched = CmfPlatformBriefingService.Generate(source, prompt => { batches++; return prompt.Contains("\"Id\":\"e2\"") ? output.Replace("\"e1\"", "\"e2\"") : output; },
            prompt => { batchReviews++; return approval(prompt); },
            prompt => { throw new Exception("Multi-batch results made a redundant ranking-only request."); }, progress.Add);
        Check(batches == 2 && batchReviews == 2 && batched.Count == 34 && batched[0].Id == "f1" && batched.Last().Id == "f34",
            "Batched findings lost rows, independent review or AI-generated batch order.");
        Check(progress.SequenceEqual(new[] { 5, 25, 45, 65, 85, 98 }), "Multi-batch progress skipped, reversed or invented a stage.");
        int providerCalls = 0;
        try { CmfPlatformBriefingService.Generate(source, prompt => { providerCalls++; throw new ApplicationException("Provider unavailable"); }, approval, prompt => ""); }
        catch (ApplicationException) { }
        Check(providerCalls == 1, "A provider failure received an extra outer generation retry.");
        Console.WriteLine("Platform briefing unlimited findings and validation checks passed.");
    }

    public static void CheckPlatformBriefingHistory()
    {
        DateTime captured = new DateTime(2026, 11, 19);
        var weeks = Enumerable.Range(0, 7).Select(offset => new Dictionary<string, object> {
            { "WeekStart", captured.AddDays(-7 * offset) }, { "NewIssues", offset == 0 ? 999 : (7 - offset) * 2 },
            { "ResolvedIssues", 2 }, { "WeekEndOpenIssues", (7 - offset) * 3 }, { "WeekEndImplementedIssues", 3 }
        }).ToList();
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        Func<DateTime, int, int, string, CmfPanelSnapshotCache> snapshot = (date, open, tpt, support) => new CmfPanelSnapshotCache {
            Platform = "NVL_H", CapturedAt = date, DateScope = "all", Configuration = "fixture", ShowComparisons = true,
            Data = (Dictionary<string, object>)serializer.DeserializeObject(serializer.Serialize(new {
                PlatformLabel = "NVL H", Trend = weeks,
                SummaryFacts = new[] { new { Label = "Open", Value = open, WeeklyTrend = new { PreviousValue = 999 } } },
                TptFacts = new[] { new { Label = "Resolution TPT", Value = tpt } },
                ComponentSummary = new { Columns = new[] { "Ingredient", "QS:::Open" }, Rows = new[] { new[] { "Graphics", open.ToString() } } },
                DesignSummary = new { Columns = new[] { "Design Name", "SW Image Freeze", "Support Model", "Open" },
                    Rows = new[] { new[] { "Design A", "2026-11-25", support, "4" } } }
            })) };
        var today = snapshot(captured, 12, 5, "Full");
        var yesterday = CmfPlatformBriefingService.BuildSource(snapshot(captured.AddDays(-1), 8, 4, "Limited"));
        var currentOnly = CmfPlatformBriefingService.BuildSource(today);
        var source = CmfPlatformBriefingService.BuildSource(today, yesterday);
        Check(!source.Evidence.Any(item => item.Section == "Weekly chart" || item.Kind == "history" || item.Kind == "flow"), "Historical chart evidence reached today's briefing.");
        var metric = source.Evidence.Single(item => item.Section == "CMF Summary").Measurements.Single();
        Check(metric.Previous == 8 && metric.PreviousDate == "2026-11-18" && !metric.PreviousIsEstimate,
            "A significant observed previous-day change was missing or mislabeled as a weekly estimate.");
        Check(source.Evidence.Single(item => item.Section == "ComponentSummary").Measurements.Single(item => item.Name == "QS Open unique").Previous == 8,
            "Previous-day table-field changes were not supported.");
        Check(!source.Evidence.Single(item => item.Section == "TPT").Measurements.Single(item => item.Name == "Resolution TPT").Previous.HasValue,
            "A one-day TPT fluctuation was treated as a significant change.");
        var design = source.Evidence.Single(item => item.Section == "DesignSummary");
        Check(design.Context["Yesterday Support model"] == "Limited" && design.Context["Compared with"] == "2026-11-18"
            && !design.Measurements.Single(item => item.Name == "Days until SW image freeze").Previous.HasValue,
            "Support-field changes were lost or routine countdown aging was treated as a sudden change.");
        Check(source.Hash != currentOnly.Hash && yesterday.Evidence.All(item => item.Measurements.All(value => !value.Previous.HasValue)),
            "Yesterday evidence did not invalidate the briefing or the baseline was mutated.");
        foreach (var values in new[] { new[] { 1, 0, 0 }, new[] { 2, 0, 1 }, new[] { 102, 100, 0 }, new[] { 0, 2, 1 }, new[] { 12, 8, 1 } })
        {
            var comparison = CmfPlatformBriefingService.BuildSource(snapshot(captured, values[0], 4, "Full"),
                CmfPlatformBriefingService.BuildSource(snapshot(captured.AddDays(-1), values[1], 4, "Full")));
            Check(comparison.Evidence.Single(item => item.Section == "CMF Summary").Measurements.Single().Previous.HasValue == (values[2] == 1),
                "Day-over-day materiality thresholds were not enforced.");
        }
        yesterday.CapturedAt = captured.AddDays(-7).ToString("o");
        Check(CmfPlatformBriefingService.BuildSource(today, yesterday).Hash == currentOnly.Hash, "A stale baseline became yesterday evidence.");
        yesterday.CapturedAt = captured.AddDays(-1).ToString("o");
        var priorMetric = yesterday.Evidence.Single(item => item.Section == "CMF Summary").Measurements.Single();
        priorMetric.CurrentDate = "2026-11-12";
        Check(!CmfPlatformBriefingService.BuildSource(today, yesterday).Evidence.Single(item => item.Section == "CMF Summary").Measurements.Single().Previous.HasValue,
            "A historical measurement was relabeled as yesterday by its snapshot timestamp.");
        priorMetric.CurrentDate = "2026-11-18";
        yesterday.Configuration = "other";
        Check(CmfPlatformBriefingService.BuildSource(today, yesterday).Hash == currentOnly.Hash, "Different configuration populations were compared.");
        yesterday.Configuration = "fixture";
        yesterday.DateScope = "different";
        Check(CmfPlatformBriefingService.BuildSource(today, yesterday).Hash == currentOnly.Hash, "Different date scopes were compared.");
        yesterday.DateScope = "all";
        today.ShowComparisons = false;
        Check(CmfPlatformBriefingService.BuildSource(today, yesterday).Hash == currentOnly.Hash, "Hidden daily comparisons leaked into the briefing.");
        Console.WriteLine("Platform briefing current situation and previous-day change checks passed.");
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
        var slots = (System.Threading.SemaphoreSlim)typeof(AiSummaryService).GetField("DashboardRequestSlots", flags).GetValue(null);
        int acquired = 0;
        try
        {
            for (; acquired < 3; acquired++) Check(slots.Wait(0), "Independent dashboard requests are still serialized.");
            Check(!slots.Wait(0), "Dashboard request concurrency is unbounded.");
        }
        finally { if (acquired > 0) slots.Release(acquired); }
        var nextStart = typeof(AiSummaryService).GetField("DashboardNextRequestUtc", flags);
        var cooldown = typeof(AiSummaryService).GetField("DashboardCooldownUntilUtc", flags);
        var startDelay = typeof(AiSummaryService).GetMethod("DashboardStartDelay", flags);
        var extendCooldown = typeof(AiSummaryService).GetMethod("ExtendDashboardCooldown", flags);
        object savedStart = nextStart.GetValue(null), savedCooldown = cooldown.GetValue(null);
        try
        {
            nextStart.SetValue(null, DateTime.MinValue);
            cooldown.SetValue(null, DateTime.MinValue);
            Check((TimeSpan)startDelay.Invoke(null, new object[] { now }) == TimeSpan.Zero
                && (TimeSpan)startDelay.Invoke(null, new object[] { now }) == TimeSpan.FromSeconds(3)
                && (TimeSpan)startDelay.Invoke(null, new object[] { now.AddSeconds(3) }) == TimeSpan.Zero,
                "Concurrent requests bypass global start pacing or wait for the previous response to finish.");
            extendCooldown.Invoke(null, new object[] { now, TimeSpan.FromSeconds(45) });
            extendCooldown.Invoke(null, new object[] { now, TimeSpan.FromSeconds(7) });
            extendCooldown.Invoke(null, new object[] { now, TimeSpan.Zero });
            Check((TimeSpan)startDelay.Invoke(null, new object[] { now.AddSeconds(6) }) == TimeSpan.FromSeconds(39)
                && (TimeSpan)startDelay.Invoke(null, new object[] { now.AddSeconds(45) }) == TimeSpan.Zero
                && (TimeSpan)startDelay.Invoke(null, new object[] { now.AddSeconds(45) }) == TimeSpan.FromSeconds(3),
                "Provider cooldown was shortened by another response or resumed concurrent requests bypass pacing.");
        }
        finally { nextStart.SetValue(null, savedStart); cooldown.SetValue(null, savedCooldown); }
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