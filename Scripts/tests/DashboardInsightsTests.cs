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
        var result = CmfDashboardInsightsService.Build(data, "Overall", today);
        var ingredient = result.Trends.Single(trend => trend.Dimension == "Ingredient");
        Check(ingredient.Open == 3 && ingredient.PreviousOpen == 2 && ingredient.Change == 1, "Open balances or duplicate row semantics changed.");
        Check(ingredient.Excluded == 1 && ingredient.OldestDays == 20, "Exclusions or acceptance aging changed.");
        Check(result.Records == 5 && result.DistinctIssues == 4 && result.DuplicateRecords == 1, "Occurrence and distinct issue counts were conflated.");
        Check(result.Groups.Count == 1 && result.Groups[0].DistinctIssues == 2 && result.Groups[0].Occurrences == 3 && result.Groups[0].PlatformCount == 2, "Explicit relationship grouping is incorrect.");
        Check(result.Groups[0].Evidence.Contains("common cause is not established"), "Relationship certainty label missing.");
        Check(result.Groups[0].Reasons.Any(reason => reason.Source == "promoted_id" && reason.Text.Contains("16099999999")), "Explicit relationship justification missing.");
        Check(result.Groups[0].Reasons.Any(reason => reason.Source == "component" && reason.Text.Contains("Display")), "Shared ingredient justification missing.");
        Check(result.Groups[0].Reasons.Any(reason => reason.Source == "title" && reason.Text.Contains("external display") && reason.IssueIds.Count == 2), "Exact title overlap did not use distinct HSD evidence.");
        Check(result.CanCompare && result.DateRange == "All acceptance dates", "Insights lost its date-filter-independent scope.");
        List<CmfInsightSelection> selected;
        string valid = "[{\"Id\":\"" + result.Highlights[0].Id + "\",\"Action\":\"review\"}]";
        Check(CmfDashboardInsightsService.TryParseSelection(valid, result.Highlights, out selected), "Valid model selection rejected.");
        string many = "[" + string.Join(",", result.Highlights.Select(finding => "{\"Id\":\"" + finding.Id + "\",\"Action\":\"review\"}")) + "]";
        Check(result.Highlights.Count > 3 && CmfDashboardInsightsService.TryParseSelection(many, result.Highlights, out selected)
            && selected.Count == result.Highlights.Count, "Justified highlights are still capped at three.");
        foreach (string invalid in new[] { "[]", "{}", "not json", "[{\"Id\":\"invented\",\"Action\":\"review\"}]", valid.Replace("review", "assign-owner"), valid.Replace("}]", ",\"Observation\":\"99999 unrelated HSDs\"}]"), valid.Replace("}]", "}," + valid.Substring(1)) })
            Check(!CmfDashboardInsightsService.TryParseSelection(invalid, result.Highlights, out selected), "Invalid or out-of-scope AI output accepted: " + invalid);
        DataTable reversed = data.Clone();
        foreach (DataRow row in data.AsEnumerable().Reverse()) reversed.ImportRow(row);
        Check(CmfDashboardInsightsService.Build(reversed, "Overall", today).SourceHash == result.SourceHash, "Source fingerprint depended on SQL row order.");
        DataTable platform = data.AsEnumerable().Where(row => (string)row["source_platform"] == "CMF_PTL_ALL_COMPONENTS_TABLE").CopyToDataTable();
        var scoped = CmfDashboardInsightsService.Build(platform, "PTL", today);
        Check(scoped.Groups.Count == 0 && scoped.Trends.Where(trend => trend.Dimension == "Platform").Count() == 1, "Platform scope leaked cross-platform relationships.");
        for (int index = 0; index < 24; index++)
            data.Rows.Add((16060000000L + index).ToString(), "Repeated screen hang", "CMF_PTL_ALL_COMPONENTS_TABLE", "Ingredient " + index, "Group " + index, "", "", "cmf_ok", "open", today.AddDays(-40), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        var expanded = CmfDashboardInsightsService.Build(data, "Overall", today);
        Check(expanded.Highlights.Count > 18 && expanded.HighlightCount == expanded.Highlights.Count, "Candidate generation still drops justified findings.");
        var view = CmfDashboardInsightsService.CreateView(expanded, "opaque-token");
        Check(view.Highlights.Count == CmfDashboardInsightsService.FindingsPerPage && view.HighlightCount == expanded.Highlights.Count
            && expanded.Highlights.Count > view.Highlights.Count, "Display pagination mutated or capped complete evidence.");
        var ids = new List<string>();
        for (int page = 0; page * CmfDashboardInsightsService.FindingsPerPage < expanded.HighlightCount; page++)
            ids.AddRange(CmfDashboardInsightsService.CreatePage(expanded, "highlights", page).Items.Select(finding => finding.Id));
        Check(ids.Count == expanded.HighlightCount && ids.Distinct().Count() == ids.Count, "Pagination lost or duplicated findings.");
        Check(!CmfDashboardInsightsService.CreatePage(expanded, "other", 0).Success
            && !CmfDashboardInsightsService.CreatePage(expanded, "highlights", -1).Success
            && !CmfDashboardInsightsService.CreatePage(expanded, "highlights", int.MaxValue).Success, "Invalid page/view accepted.");
        DataTable futureData = data.Clone();
        futureData.Rows.Add("16011111111", "Screen hang on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        futureData.Rows.Add("16022222222", "Screen flicker on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_duplicate", "future", today.AddDays(-30), today.AddDays(-20), today.AddDays(-2), DBNull.Value, 1);
        futureData.Rows.Add("16033333333", "Screen hang with future fix", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "future", today.AddDays(-30), today.AddDays(-20), today.AddDays(-10), DBNull.Value, 1);
        var futureResult = CmfDashboardInsightsService.Build(futureData, "Overall", today);
        var futureIngredient = futureResult.Trends.Single(trend => trend.Dimension == "Ingredient");
        Check(futureIngredient.Open == 1 && futureIngredient.PreviousOpen == 2, "Future statuses were counted as currently Open or lost their historical implementation boundary.");
        Check(futureResult.Groups.Single().OpenRecords == 1 && futureResult.Records == 3 && futureResult.DuplicateRecords == 1, "Future classification changed relationship membership, record totals or duplicate scope.");
        data.Clear();
        Check(CmfDashboardInsightsService.Build(data, "Empty", today).Highlights.Count == 0, "Empty scope fabricated findings.");
        Console.WriteLine("Dashboard Insights evidence and AI selection tests passed.");
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