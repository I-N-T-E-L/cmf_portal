using System;
using System.Data.SqlClient;

public static class DashboardDateFilterTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void CheckWeeklyClosedFlowSql(SqlConnection connection, string portalSource)
    {
        string fixture = @"(SELECT status, 'customer_must_fix' AS sysdebug,
    cmf_request,
    DATEADD(WEEK, implemented_offset, CAST(GETDATE() AS DATE)) AS implemented_date,
    DATEADD(WEEK, ask_offset, CAST(GETDATE() AS DATE)) AS date_cmf_ask,
    CASE WHEN closed_offset IS NOT NULL
        THEN CONVERT(VARCHAR(10), DATEADD(WEEK, closed_offset, CAST(GETDATE() AS DATE)), 23)
        ELSE raw_closed END AS closed_date
FROM (VALUES
    ('complete','cmf_ok',-20,0,NULL,-30),
    ('rejected','cmf_ok',NULL,0,NULL,-30),
    ('complete','cmf_duplicate',0,0,NULL,-30),
    ('complete','cmf_ok',0,-1,NULL,-30),
    ('complete','cmf_ok',0,1,NULL,-30),
    ('complete','cmf_ok',0,NULL,NULL,-30),
    ('complete','cmf_ok',0,NULL,'invalid',-30),
    ('complete','cmf_ok',0,-104,NULL,-105),
    ('open','cmf_ok',NULL,NULL,NULL,0),
    ('implemented','cmf_ok',0,0,NULL,-30),
    ('future','cmf_ok',0,0,NULL,-30)
) AS dates(status,cmf_request,implemented_offset,closed_offset,raw_closed,ask_offset)) AS src";
        var closed = System.Text.RegularExpressions.Regex.Match(portalSource,
            @"resolved_issues AS \((?<query>[\s\S]*?)\r?\n\),\r?\nimplemented_issues AS");
        Check(closed.Success, "Weekly Closed query could not be located.");
        string query = closed.Groups["query"].Value.Replace("\" + weeklyFlowSourceSql + @\"", fixture)
            .Replace("\" + weeklyFlowPredicate + @\"", "src.cmf_request = 'cmf_ok' AND src.sysdebug LIKE '%customer_must_fix%'")
            .Replace("\" + CmfSummaryTrendService.BuildWeeklyClosedPredicate(\"src\", \"w.week_offset\") + @\"",
                CmfSummaryTrendService.BuildWeeklyClosedPredicate("src", "w.week_offset"));
        using (var command = new SqlCommand("WITH weeks AS (SELECT 0 AS week_offset UNION ALL SELECT 1), resolved_issues AS (" + query
            + ") SELECT week_offset, count_value FROM resolved_issues ORDER BY week_offset", connection))
        using (var reader = command.ExecuteReader())
        {
            Check(reader.Read() && reader.GetInt32(0) == 0 && reader.GetInt32(1) == 2,
                "Weekly Closed did not count recent complete/rejected closures independently of implementation dates.");
            Check(reader.Read() && reader.GetInt32(0) == 1 && reader.GetInt32(1) == 1 && !reader.Read(),
                "Weekly Closed misplaced previous closures or included missing/invalid/future/out-of-range dates or active statuses.");
        }
        using (var command = new SqlCommand("SELECT COUNT(*) FROM " + fixture
            + " WHERE src.cmf_request = 'cmf_ok' AND " + CmfSummaryTrendService.BuildWeeklyClosedPredicate("src", "@week_offset"), connection))
        {
            command.Parameters.AddWithValue("@week_offset", 0);
            Check(Convert.ToInt32(command.ExecuteScalar()) == 2, "Closed drilldown did not match the current week's closure count.");
            command.Parameters["@week_offset"].Value = 1;
            Check(Convert.ToInt32(command.ExecuteScalar()) == 1, "Closed drilldown did not match the previous week's closure count.");
        }
        using (var command = new SqlCommand("SELECT COUNT(*) FROM " + fixture + " WHERE src.cmf_request = 'cmf_ok' AND (("
            + CmfSummaryTrendService.BuildWeeklyOpenPredicate("src", "@week_offset") + ") OR ("
            + CmfSummaryTrendService.BuildWeeklyClosedPredicate("src", "@week_offset") + "))", connection))
        {
            command.Parameters.AddWithValue("@week_offset", 0);
            Check(Convert.ToInt32(command.ExecuteScalar()) == 3, "Combined flow drilldown did not retain request dates for Open and closure dates for Closed.");
        }
        Console.WriteLine("Weekly Closed SQL fixtures passed (unique-only complete/rejected closures, closure-week placement, invalid dates and both drilldown paths).");
    }

    public static void CheckWeeklyOpenFlowSql(SqlConnection connection, string portalSource)
    {
        string fixture = @"(SELECT fixture_id, status, cmf_request, 'customer_must_fix' AS sysdebug,
    CASE WHEN fixture_id = 'invalid-ask' THEN 'invalid'
        ELSE CONVERT(VARCHAR(10), DATEADD(DAY, ask_day, request_week), 23) END AS date_cmf_ask,
    COALESCE(raw_implemented, CONVERT(VARCHAR(10), DATEADD(DAY, implemented_day, request_week), 23)) AS implemented_date,
    COALESCE(raw_closed, CONVERT(VARCHAR(10), DATEADD(DAY, closed_day, request_week), 23)) AS closed_date
FROM (VALUES
    ('still-open','open','cmf_ok',1,NULL,NULL,NULL,NULL),
    ('later-implemented','implemented','cmf_ok',1,8,NULL,NULL,NULL),
    ('later-verified','verified','cmf_ok',1,8,NULL,NULL,NULL),
    ('later-complete','complete','cmf_ok',1,8,15,NULL,NULL),
    ('later-rejected','rejected','cmf_ok',1,NULL,8,NULL,NULL),
    ('next-week-boundary','implemented','cmf_ok',1,7,NULL,NULL,NULL),
    ('same-week-implemented','implemented','cmf_ok',1,2,NULL,NULL,NULL),
    ('last-day-implemented','implemented','cmf_ok',1,6,NULL,NULL,NULL),
    ('same-week-closed','complete','cmf_ok',1,NULL,2,NULL,NULL),
    ('same-week-rejected','rejected','cmf_ok',1,NULL,2,NULL,NULL),
    ('already-implemented','implemented','cmf_ok',1,0,NULL,NULL,NULL),
    ('duplicate-open','open','cmf_duplicate',1,NULL,NULL,NULL,NULL),
    ('missing-ask','open','cmf_ok',NULL,NULL,NULL,NULL,NULL),
    ('invalid-ask','open','cmf_ok',1,NULL,NULL,NULL,NULL),
    ('future-ask','open','cmf_ok',29,NULL,NULL,NULL,NULL),
    ('missing-implementation','implemented','cmf_ok',1,NULL,NULL,NULL,NULL),
    ('missing-closure','complete','cmf_ok',1,NULL,NULL,NULL,NULL),
    ('invalid-implementation','open','cmf_ok',1,NULL,NULL,'invalid',NULL),
    ('invalid-closure','complete','cmf_ok',1,NULL,NULL,NULL,'invalid')
) AS dates(fixture_id,status,cmf_request,ask_day,implemented_day,closed_day,raw_implemented,raw_closed)
CROSS APPLY (SELECT DATEADD(DAY, -21 - DATEDIFF(DAY, CONVERT(date, '19000107', 112), CAST(GETDATE() AS DATE)) % 7,
    CAST(GETDATE() AS DATE)) AS request_week) AS calendar) AS src";
        string expected = "already-implemented,invalid-closure,invalid-implementation,last-day-implemented,later-complete,later-implemented,later-rejected,later-verified,missing-closure,missing-implementation,next-week-boundary,same-week-closed,same-week-implemented,same-week-rejected,still-open";
        var ids = new System.Collections.Generic.List<string>();
        using (var command = new SqlCommand("SELECT fixture_id FROM " + fixture + " WHERE src.cmf_request = 'cmf_ok' AND "
            + CmfSummaryTrendService.BuildWeeklyOpenPredicate("src", "@week_offset") + " ORDER BY fixture_id", connection))
        {
            command.Parameters.AddWithValue("@week_offset", 3);
            using (var reader = command.ExecuteReader()) while (reader.Read()) ids.Add(reader.GetString(0));
        }
        Check(string.Join(",", ids.ToArray()) == expected,
            "Weekly Open lost requested CMFs because of resolution status/dates or included duplicates or unusable request dates.");
        var open = System.Text.RegularExpressions.Regex.Match(portalSource,
            @"new_issues AS \((?<query>[\s\S]*?)\r?\n\),\r?\nresolved_issues AS");
        Check(open.Success, "Weekly Open query could not be located.");
        string query = open.Groups["query"].Value.Replace("\" + weeklyFlowSourceSql + @\"", fixture)
            .Replace("\" + weeklyFlowPredicate + @\"", "src.cmf_request = 'cmf_ok' AND src.sysdebug LIKE '%customer_must_fix%'")
            .Replace("\" + CmfSummaryTrendService.BuildWeeklyOpenPredicate(\"src\", \"w.week_offset\") + @\"",
                CmfSummaryTrendService.BuildWeeklyOpenPredicate("src", "w.week_offset"));
        using (var command = new SqlCommand("WITH weeks AS (SELECT 0 AS week_offset UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3), new_issues AS ("
            + query + ") SELECT week_offset, count_value FROM new_issues", connection))
        using (var reader = command.ExecuteReader())
            Check(reader.Read() && reader.GetInt32(0) == 3 && reader.GetInt32(1) == 15 && !reader.Read(),
                "Weekly Open chart and drilldown counts disagree for the full request-week intake.");
        using (var command = new SqlCommand("SELECT COUNT(*) FROM " + fixture + " WHERE src.cmf_request = 'cmf_ok' AND (("
            + CmfSummaryTrendService.BuildWeeklyOpenPredicate("src", "@week_offset") + ") OR ("
            + CmfSummaryTrendService.BuildWeeklyClosedPredicate("src", "@week_offset") + "))", connection))
        {
            command.Parameters.AddWithValue("@week_offset", 3);
            Check(Convert.ToInt32(command.ExecuteScalar()) == 15,
                "Combined weekly details counted same-week opened/closed CMFs twice.");
        }
        string current = @"(SELECT status, CAST(GETDATE() AS DATE) AS date_cmf_ask,
    DATEADD(DAY, implementation_day, CAST(GETDATE() AS DATE)) AS implemented_date,
    DATEADD(DAY, closure_day, CAST(GETDATE() AS DATE)) AS closed_date
FROM (VALUES ('open',NULL,NULL),('implemented',0,NULL),('complete',NULL,0)) AS dates(status,implementation_day,closure_day)) AS src";
        using (var command = new SqlCommand("SELECT COUNT(*) FROM " + current + " WHERE "
            + CmfSummaryTrendService.BuildWeeklyOpenPredicate("src", "0"), connection))
            Check(Convert.ToInt32(command.ExecuteScalar()) == 3, "The current partial week lost requests implemented or closed today.");
        Console.WriteLine("Weekly Open SQL fixtures passed (same-week/later resolutions retained, unique-only intake, valid request dates, chart/detail parity and combined-detail deduplication).");
    }

    public static void CheckPendingEstimateSql(SqlConnection connection)
    {
        string pendingSource = @"SELECT source_platform, cp_id, status, date_cmf_ask
FROM (VALUES
    ('A','stay','open','2026-10-26'),
    ('A','stay','open','2026-10-26'),
    ('B','stay','open','2026-10-25'),
    ('A','boundary','open','2026-10-27T23:59:59'),
    ('A','new','open','2026-10-28'),
    ('A','future','open','2026-11-04'),
    ('A','missing','open',NULL),
    ('A','terminal','complete','2026-10-01')
) AS pending(source_platform,cp_id,status,date_cmf_ask)";
        string componentSource = @"SELECT source_platform, cp_id, cmf_request, date_cmf_ask, date_cmf_decided
FROM (VALUES
    ('A','decided_after','cmf_ok','2026-10-26','2026-10-28'),
    ('A','decided_on','cmf_ok','2026-10-26','2026-10-27'),
    ('A','new_decided','cmf_ok','2026-10-28','2026-10-30'),
    ('A','missing_ask','cmf_ok',NULL,'2026-10-28'),
    ('A','missing_decision','cmf_ok','2026-10-25',NULL),
    ('A','reversed','cmf_ok','2026-10-28','2026-10-27'),
    ('A','stay','cmf_ok','2026-10-26','2026-10-28'),
    ('A','rejected_after','cmf_reject','2026-10-26','2026-10-29'),
    ('A','duplicate_after','cmf_duplicate','2026-10-20','2026-11-01'),
    ('A','duplicate_after','cmf_duplicate','2026-10-20','2026-11-01')
) AS components(source_platform,cp_id,cmf_request,date_cmf_ask,date_cmf_decided)";
        DateTime today = new DateTime(2026, 11, 3);
        CmfSummaryTrend estimate = CmfSummaryTrendService.LoadPendingEstimate(
            connection, pendingSource, componentSource, 10, today);
        Check(estimate.PreviousValue == 6m && estimate.RecentValue == 10m, "Pending compares the displayed headline with the historical deduplicated request balance.");
        Check(estimate.Display == "4" && estimate.Tone == "bad", "The actual Pending headline increase is red without a display prefix.");
        Check(estimate.Tooltip.Contains("4 records excluded from historical reconstruction")
            && estimate.Tooltip.Contains("displayed Pending card metric"), "SQL discloses historical exclusions without substituting a different current count.");
        CmfSummaryTrend platformA = CmfSummaryTrendService.LoadPendingEstimate(connection,
            pendingSource + " WHERE source_platform = 'A'", componentSource + " WHERE source_platform = 'A'", 9, today);
        CmfSummaryTrend platformB = CmfSummaryTrendService.LoadPendingEstimate(connection,
            pendingSource + " WHERE source_platform = 'B'", componentSource + " WHERE source_platform = 'B'", 1, today);
        Check(platformA.PreviousValue == 5m && platformA.RecentValue == 9m && platformA.Display == "4", "Platform A uses its actual headline and only its own prior-week history.");
        Check(platformB.PreviousValue == 1m && platformB.RecentValue == 1m && platformB.Display == "0" && platformB.Tone == "neutral", "Platform B is isolated from platform A despite a shared HSD ID.");
        string emptyPending = pendingSource + " WHERE 1 = 0";
        string emptyComponents = componentSource + " WHERE 1 = 0";
        CmfSummaryTrend empty = CmfSummaryTrendService.LoadPendingEstimate(connection, emptyPending, emptyComponents, 0, today);
        Check(empty.PreviousValue == 0m && empty.Display == "0", "Empty history has a valid zero estimate.");
        CmfSummaryTrend invalid = CmfSummaryTrendService.LoadPendingEstimate(connection,
            pendingSource + " WHERE cp_id = 'missing'", emptyComponents, 1, today);
        Check(invalid.RecentValue == 1m && invalid.PreviousValue == null && invalid.Display == string.Empty && invalid.Direction == "unavailable", "Wholly unusable history retains today's headline but leaves the trend blank.");
        string conflictingPending = "SELECT * FROM (" + pendingSource + @") AS original WHERE cp_id = 'stay'
UNION ALL SELECT 'A','stay','open',NULL";
        CmfSummaryTrend conflicting = CmfSummaryTrendService.LoadPendingEstimate(connection,
            conflictingPending, emptyComponents, 4, today);
        Check(conflicting.RecentValue == 4m && conflicting.PreviousValue == 1m && conflicting.Display == "3",
            "An invalid duplicate affects historical reconstruction without rewriting today's headline or excluding another platform's history.");
        Console.WriteLine("Pending estimate SQL fixtures passed (request/decision boundaries, row deduplication, cross-platform IDs, overlap, rejection, missing/reversed dates, empty history).");
    }

    public static void CheckBalanceEstimateSql(SqlConnection connection)
    {
        string futureSource = @"(SELECT * FROM (VALUES
    ('cmf_ok','future','customer_must_fix','2026-10-01','2026-10-02','2026-10-28'),
    ('cmf_duplicate','future','customer_must_fix','2026-10-01','2026-10-02','2026-10-29'),
    ('cmf_duplicate','future','customer_must_fix','2026-10-01','2026-10-02',NULL),
    ('cmf_ok','open','customer_must_fix','2026-10-01','2026-10-02',NULL),
    ('cmf_ok','implemented','customer_must_fix','2026-10-01','2026-10-02','2026-10-27')
) AS fixture(cmf_request,status,sysdebug,date_cmf_ask,date_cmf_decided,implemented_date)
CROSS APPLY (SELECT CAST(NULL AS date) AS closed_date) AS dates) AS future_balances";
        var futureTrends = CmfSummaryTrendService.LoadBalanceEstimates(connection, futureSource, new DateTime(2026, 11, 4));
        Check(futureTrends["Implemented"].RecentValue == 3m && futureTrends["Implemented"].PreviousValue == 2m,
            "Future joins Implemented, including duplicate rows, and uses implementation dates for the earlier balance.");
        Check(futureTrends["Open"].RecentValue == 1m && futureTrends["Open"].PreviousValue == 2m,
            "Future leaves today's Open count but remains historically open before its implementation date.");
        Check(futureTrends["Implemented"].Tooltip.Contains("1 records excluded from both trend counts"),
            "Future without an implementation date is excluded symmetrically from reconstruction, not misclassified as Open.");
        Check(futureTrends["Total CMFs"].RecentValue == 5m && futureTrends["Closed"].RecentValue == 0m,
            "Reclassifying Future does not change Total or Closed.");
        string source = @"(SELECT * FROM (VALUES
    ('A','cmf_ok','open','customer_must_fix','2026-10-01','2026-10-02',NULL,NULL),
    ('A','cmf_ok','open','customer_must_fix','2026-10-01','2026-10-28T23:59:59',NULL,NULL),
    ('A','cmf_ok','open','customer_must_fix','2026-10-01','2026-10-29',NULL,NULL),
    ('A','cmf_ok','implemented','customer_must_fix','2026-10-01','2026-10-02','2026-10-29',NULL),
    ('A','cmf_ok','implemented','customer_must_fix','2026-10-01','2026-10-02','2026-10-28',NULL),
    ('A','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-02','2026-10-25','2026-10-29'),
    ('A','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-02','2026-10-20','2026-10-28T23:59:59'),
    ('A','cmf_duplicate','rejected','customer_must_fix','2026-10-01','2026-10-02',NULL,'2026-10-27'),
    ('A','cmf_duplicate','open','customer_must_fix','2026-10-01','2026-10-02',NULL,NULL),
    ('A','cmf_duplicate','open','customer_must_fix','2026-10-01','2026-10-02',NULL,NULL),
    ('B','cmf_ok','open','customer_must_fix','2026-10-01','2026-10-02',NULL,NULL),
    ('A','cmf_ok','verified','customer_must_fix','2026-10-01','2026-10-02','2026-10-24',NULL),
    ('A','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-02',NULL,NULL),
    ('A','cmf_ok','implemented','customer_must_fix','2026-10-01','2026-10-02',NULL,NULL),
    ('A','cmf_ok','open','customer_must_fix','2026-10-01',NULL,NULL,NULL),
    ('A','cmf_ok','open','customer_must_fix','2026-10-20','2026-10-19',NULL,NULL),
    ('A','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-02','2026-10-25','2026-10-24'),
    ('A','cmf_reject','rejected','customer_must_fix','2026-10-01','2026-10-02',NULL,'2026-10-27'),
    ('A','cmf_ok','open','other','2026-10-01','2026-10-02',NULL,NULL),
    ('A','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-30',NULL,NULL)
) AS fixture(platform,cmf_request,status,sysdebug,date_cmf_ask,date_cmf_decided,implemented_date,closed_date)) AS balances";
        DateTime today = new DateTime(2026, 11, 4, 15, 30, 0);
        var trends = CmfSummaryTrendService.LoadBalanceEstimates(connection, source, today);
        Check(trends["Total CMFs"].RecentValue == 16m && trends["Total CMFs"].PreviousValue == 14m, "Total excludes the same two invalid decisions from both trend counts while retaining duplicate rows.");
        Check(trends["Closed"].RecentValue == 4m && trends["Closed"].PreviousValue == 2m, "Closed excludes invalid historical lifecycles from today's trend as well, but retains newly accepted records with a known zero historical count.");
        Check(trends["Implemented"].RecentValue == 2m && trends["Implemented"].PreviousValue == 2m, "Implemented compares the same eligible cohort, including now-closed CMFs in the earlier balance.");
        Check(trends["Open"].RecentValue == 7m && trends["Open"].PreviousValue == 7m, "Open excludes invalid decisions from both sides instead of inventing backlog growth.");
        Check(trends["Total CMFs"].Display == "2" && trends["Closed"].Display == "2"
            && trends["Implemented"].Display == "0" && trends["Open"].Display == "0", "Matching reconstruction exclusions removes artificial deltas.");
        string[] metrics = { "Total CMFs", "Closed", "Implemented", "Open" };
        int[] headlines = { 18, 6, 3, 9 };
        int[] previous = { 14, 2, 2, 7 };
        for (int index = 0; index < metrics.Length; index++)
        {
            var displayed = trends[metrics[index]].WithCurrentCount(headlines[index]);
            Check(displayed.RecentValue == headlines[index] && displayed.PreviousValue == previous[index]
                && displayed.Display == (headlines[index] - previous[index]).ToString(), "Displayed " + metrics[index] + " must compare its headline with exactly seven days earlier.");
            Check(displayed.Tooltip.Contains("Wed Oct 28, 2026") && displayed.Tooltip.Contains("exactly seven days earlier"), "Displayed comparison uses a calendar-week boundary instead of the prior date.");
        }
        Check(trends["Total CMFs"].Tooltip.Contains("2 records excluded")
            && trends["Open"].Tooltip.Contains("5 records excluded")
            && trends["Open"].Tooltip.Contains("from both trend counts")
            && trends["Open"].Tooltip.Contains("headline card count is unchanged"), "Tooltips disclose matching exclusions and unchanged headline counts.");
        var platformA = CmfSummaryTrendService.LoadBalanceEstimates(connection,
            "(SELECT * FROM " + source + " WHERE platform = 'A') AS platform_source", today);
        var platformB = CmfSummaryTrendService.LoadBalanceEstimates(connection,
            "(SELECT * FROM " + source + " WHERE platform = 'B') AS platform_source", today);
        Check(platformA["Total CMFs"].RecentValue == 15m && platformA["Total CMFs"].PreviousValue == 13m
            && platformA["Open"].PreviousValue == 6m, "Platform A balance history is isolated.");
        Check(platformB["Total CMFs"].RecentValue == 1m && platformB["Open"].PreviousValue == 1m
            && platformB["Open"].Display == "0", "Platform B retains a valid zero change.");
        var scoped = CmfSummaryTrendService.LoadBalanceEstimates(connection,
            "(SELECT * FROM " + source + " WHERE TRY_CAST(date_cmf_decided AS date) >= '20261029') AS scoped_source", today);
        Check(scoped["Total CMFs"].RecentValue == 2m && scoped["Total CMFs"].PreviousValue == 0m
            && scoped["Closed"].PreviousValue == 0m, "Both balances use the same selected acceptance-date cohort; post-cutoff lifecycle gaps cannot affect earlier counts.");
        var invalid = CmfSummaryTrendService.LoadBalanceEstimates(connection,
            "(SELECT * FROM " + source + " WHERE date_cmf_decided IS NULL) AS invalid_source", today);
        foreach (string metric in new[] { "Total CMFs", "Closed", "Implemented", "Open" })
            Check(invalid[metric].RecentValue == 0m && invalid[metric].PreviousValue == null && invalid[metric].Display == string.Empty, "Unusable history excludes today's records and leaves " + metric + " blank.");
        var empty = CmfSummaryTrendService.LoadBalanceEstimates(connection,
            "(SELECT * FROM " + source + " WHERE 1 = 0) AS empty_source", today);
        foreach (string metric in new[] { "Total CMFs", "Closed", "Implemented", "Open" })
            Check(empty[metric].RecentValue == 0m && empty[metric].PreviousValue == 0m && empty[metric].Display == "0", "An empty cohort has a valid zero " + metric + " balance.");
        Console.WriteLine("Same-weekday balance SQL fixtures passed (acceptance/implementation/closure boundaries, duplicates, platform isolation, missing/reversed dates, date scopes and empty history).");
    }

    public static void CheckWeeklyTptSql(SqlConnection connection)
    {
        string source = @"(SELECT * FROM (VALUES
    ('cmf_ok','customer_must_fix','2026-10-28','2026-10-29','2026-11-01',NULL,'10'),
    ('cmf_duplicate','customer_must_fix','2026-10-28','2026-11-04T23:59:59','2026-11-04',NULL,'20'),
    ('cmf_ok','customer_must_fix','2026-10-21','2026-10-22','2026-10-25',NULL,'20'),
    ('cmf_ok','customer_must_fix','2026-10-21','2026-10-28T23:59:59','2026-10-28',NULL,'40'),
    ('cmf_ok','customer_must_fix','2026-10-20','2026-10-21','2026-10-21',NULL,'900'),
    ('cmf_ok','customer_must_fix','2026-11-05','2026-11-05','2026-11-05',NULL,'900'),
    ('cmf_ok','customer_must_fix','2026-11-01','2026-11-02','2026-11-04',NULL,NULL),
    ('cmf_ok','customer_must_fix','2026-11-01','2026-11-02','2026-11-04',NULL,'bad'),
    ('cmf_ok','customer_must_fix','2026-11-01','2026-11-02','2026-11-04',NULL,'-1'),
    ('cmf_reject','customer_must_fix','2026-10-28','2026-10-29','2026-11-01',NULL,'900'),
    ('cmf_ok','other','2026-10-28','2026-10-29','2026-11-01',NULL,'900')
) AS fixture(cmf_request,sysdebug,date_cmf_ask,date_cmf_decided,implemented_date,closed_date,days_active)) AS tpt_source";
        DateTime today = new DateTime(2026, 11, 4, 15, 30, 0);
        var trends = CmfSummaryTrendService.Load(connection, source, today);
        var overall = trends["CMF Overall TPT"];
        Check(overall.RecentValue == 15m && overall.PreviousValue == 30m,
            "Overall TPT averages valid recorded durations by acceptance week, including duplicates and excluding dates outside the windows.");
        Check(overall.Display == "50%" && overall.Direction == "down" && overall.Tone == "good",
            "Overall TPT uses the shared percentage, arrow and improvement color rules.");
        Check(overall.Tooltip.Contains("current recorded days_active") && overall.Tooltip.Contains("acceptance cohorts")
            && overall.Tooltip.Contains("not reconstructed historical headline averages"),
            "Overall TPT discloses its recorded-duration cohort comparison, not a reconstructed historical headline.");
        var missing = CmfSummaryTrendService.Load(connection,
            "(SELECT * FROM " + source + " WHERE TRY_CAST(date_cmf_decided AS date) >= '20261029') AS recent_only", today);
        Check(missing["CMF Overall TPT"].PreviousValue == null && missing["CMF Overall TPT"].Display == string.Empty,
            "An absent Overall TPT comparison cohort leaves the indicator blank.");
        var empty = CmfSummaryTrendService.Load(connection,
            "(SELECT * FROM " + source + " WHERE 1 = 0) AS empty_tpt", today);
        Check(empty["CMF Overall TPT"].RecentValue == null && empty["CMF Overall TPT"].Display == string.Empty,
            "Empty Overall TPT history is not displayed as a zero trend.");
        var zero = CmfSummaryTrendService.Load(connection,
            "(SELECT cmf_request,sysdebug,date_cmf_ask,date_cmf_decided,implemented_date,closed_date,"
            + "CASE WHEN TRY_CAST(date_cmf_decided AS date) < '20261029' THEN '0' ELSE days_active END AS days_active FROM "
            + source + ") AS zero_tpt", today);
        Check(zero["CMF Overall TPT"].PreviousValue == 0m && zero["CMF Overall TPT"].Display == string.Empty,
            "An undefined Overall TPT percentage against zero remains hidden.");
        Console.WriteLine("Overall TPT weekly SQL fixtures passed (acceptance windows, duplicate rows, invalid durations, missing cohorts and zero baseline).");
    }

    public static void CheckTptEstimateSql(SqlConnection connection)
    {
        string source = @"(SELECT * FROM (VALUES
    ('cmf_ok','customer_must_fix','complete','2026-10-01','2026-10-03','2026-10-13','2026-10-20','19'),
    ('cmf_duplicate','customer_must_fix','implemented','2026-10-04','2026-10-08','2026-11-01',NULL,'31'),
    ('cmf_ok','customer_must_fix','open','2026-10-25','2026-10-28T23:59:59',NULL,NULL,'10'),
    ('cmf_ok','customer_must_fix','open','2026-10-27','2026-10-29T00:00:00',NULL,NULL,'6'),
    ('cmf_ok','customer_must_fix','complete','2026-10-12','2026-10-14','2026-10-27','2026-11-02','21'),
    ('cmf_ok','customer_must_fix','complete','2026-09-25','2026-09-30','2026-10-28T23:59:59','2026-10-28','30'),
    ('cmf_reject','customer_must_fix','rejected','2026-10-20','2026-10-21','2026-10-23','2026-10-23','900'),
    ('cmf_reject','customer_must_fix','rejected','2026-10-10','2026-10-12','2026-10-29T00:00:00','2026-10-30','70'),
    ('cmf_ok','other','open','2026-10-20','2026-10-21',NULL,NULL,'900')
) AS fixture(cmf_request,sysdebug,status,date_cmf_ask,date_cmf_decided,implemented_date,closed_date,days_active)) AS tpt_source";
        DateTime today = new DateTime(2026, 11, 4, 15, 30, 0);
        using (SqlCommand command = CmfSummaryTrendService.CreateTptEstimateCommand(connection, source, today))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            Check(reader.Read(), "Historical TPT query returns its aggregates.");
            Check(Math.Abs(Convert.ToDecimal(reader.GetValue(3)) - 19m / 7m) < 0.00001m,
                "Disposition uses every retained decision through the end of seven days ago, not the preceding week or its midnight successor.");
            Check(Convert.ToDecimal(reader.GetValue(4)) == 13.25m,
                "Resolution includes old implementations and the comparison day's final second, excluding implementations at the next midnight.");
            Check(Convert.ToDecimal(reader.GetValue(5)) == 18.4m,
                "Overall keeps old closed durations, rolls later closures back only to closure, rolls active durations back seven days, includes duplicates and excludes new acceptances.");
        }
        var trends = CmfSummaryTrendService.LoadTptEstimates(connection, source, today);
        var disposition = trends["CMF Disposition TPT"];
        var resolution = trends["CMF Resolution TPT"];
        var overall = trends["CMF Overall TPT"];
        Check(disposition.RecentValue == 2m && disposition.PreviousValue == 2m && disposition.Display == "0", "Disposition compares integer headline averages.");
        Check(resolution.RecentValue == 15m && resolution.PreviousValue == 13m && resolution.Display == "2", "Resolution subtracts its own historical average.");
        Check(overall.RecentValue == 19m && overall.PreviousValue == 18m && overall.Display == "1", "Overall subtracts its own estimated historical headline.");
        Check(overall.Tooltip.Contains("assuming one active day per calendar day") && overall.Tooltip.Contains("no pauses or reopenings"), "Overall discloses its aging assumption, not a saved historical duration.");
        var missing = CmfSummaryTrendService.LoadTptEstimates(connection,
            "(SELECT * FROM " + source + " WHERE TRY_CAST(date_cmf_decided AS date) >= '20261029') AS new_only", today);
        foreach (var trend in missing.Values)
        {
            Check(trend.PreviousValue == null && trend.Display == string.Empty, "New-only scopes have no historical TPT average, not a zero baseline.");
        }
        var zero = CmfSummaryTrendService.LoadTptEstimates(connection,
            "(SELECT cmf_request,sysdebug,status,date_cmf_ask,date_cmf_decided,implemented_date,closed_date,'7' AS days_active FROM "
                + source + " WHERE status = 'open' AND TRY_CAST(date_cmf_decided AS date) < '20261029') AS zero_history", today);
        Check(zero["CMF Overall TPT"].PreviousValue == 0m && zero["CMF Overall TPT"].Display == "7", "A valid zero historical TPT supports whole-day subtraction.");
        string invalidSource = @"(SELECT * FROM (VALUES
    ('cmf_ok','customer_must_fix','open','2026-10-20','2026-10-21',NULL,NULL,'bad'),
    ('cmf_ok','customer_must_fix','open','2026-10-20','2026-10-21',NULL,NULL,'-1'),
    ('cmf_ok','customer_must_fix','open','2026-10-20','2026-10-21',NULL,NULL,'2'),
    ('cmf_ok','customer_must_fix','complete','2026-10-20','2026-10-21','2026-10-22',NULL,'40'),
    ('cmf_ok','customer_must_fix','implemented','2026-10-23','2026-10-22','2026-10-21',NULL,'25')
) AS fixture(cmf_request,sysdebug,status,date_cmf_ask,date_cmf_decided,implemented_date,closed_date,days_active)) AS invalid_tpt";
        var invalid = CmfSummaryTrendService.LoadTptEstimates(connection, invalidSource, today);
        Check(invalid["CMF Overall TPT"].PreviousValue == null && invalid["CMF Overall TPT"].Display == string.Empty
            && invalid["CMF Overall TPT"].Tooltip.Contains("5 records excluded"), "Unknown closure history, invalid durations, negative rollbacks and reversed dates cannot fabricate Overall TPT history.");
        var empty = CmfSummaryTrendService.LoadTptEstimates(connection,
            "(SELECT * FROM " + source + " WHERE 1 = 0) AS empty_tpt", today);
        foreach (var trend in empty.Values) Check(trend.Display == string.Empty, "Empty TPT scopes keep unavailable history blank.");
        Console.WriteLine("Same-weekday TPT estimate SQL fixtures passed.");
    }

    public static void Main()
    {
        var today = new DateTime(2026, 10, 29);
        CmfDashboardDateFilter filter;
        string error;
        foreach (string mode in new[] { "all", "today", "7days", "30days", "12months" })
        {
            Check(CmfDashboardDateFilter.TryCreate(mode, "cmf-request", "", "", today, out filter, out error), mode);
            Check(filter.ShowSummaryTrends == (mode != "today" && mode != "7days"), "Summary trend visibility for preset: " + mode);
            if (mode == "all") Check(filter.BuildPredicate("src") == "1 = 1", "All Time must not exclude null dates.");
            else Check(filter.EndExclusive == today.AddDays(1), "Presets must include the whole current day.");
            if (mode == "7days") Check(filter.Start == today.AddDays(-6), "Last 7 Days boundary.");
            if (mode == "30days") Check(filter.Start == today.AddDays(-29), "Last 30 Days boundary.");
            if (mode == "12months") Check(filter.Start == today.AddMonths(-12), "Last 12 Months boundary.");
        }
        Check(CmfDashboardDateFilter.TryCreate("custom", "submitted", "2026-10-01", "2026-10-29", today, out filter, out error), error);
        Check(filter.Start == new DateTime(2026, 10, 1) && filter.EndExclusive == today.AddDays(1), "Custom boundaries.");
        Check(filter.BuildPredicate("src").Contains("src.submitted_date") && !filter.BuildPredicate("src").Contains("date_cmf_ask"), "Submission basis.");
        Check(!filter.ShowSummaryTrends, "A 29-day custom range hides summary trends.");
        foreach (int duration in new[] { 1, 7, 29, 30, 31 })
        {
            string endText = new DateTime(2026, 10, 1).AddDays(duration - 1).ToString("yyyy-MM-dd");
            Check(CmfDashboardDateFilter.TryCreate("custom", "cmf-accepted", "2026-10-01", endText, today, out filter, out error), error);
            Check(filter.ShowSummaryTrends == (duration >= 30), "Custom inclusive duration controls summary trends: " + duration);
        }
        Check(!CmfDashboardDateFilter.TryCreate("custom", "submitted", "2026-10-29", "2026-10-01", today, out filter, out error), "Reversed dates.");
        Check(!CmfDashboardDateFilter.TryCreate("custom", "submitted", "bad", "2026-10-01", today, out filter, out error), "Malformed date.");
        Check(!CmfDashboardDateFilter.TryCreate("custom", "submitted", "9999-12-31", "9999-12-31", today, out filter, out error), "Date overflow.");
        Check(!CmfDashboardDateFilter.TryCreate("today", "unexpected", "", "", today, out filter, out error), "Unapproved basis.");
        Check(CmfDashboardDateFilter.TryCreate("today", "cmf-request", "", "", today, out filter, out error)
            && filter.BuildPredicate("src").Contains("src.date_cmf_ask"), "CMF basis.");
        Check(filter.BuildPendingPredicate("src") == filter.BuildPredicate("src"), "Pending request date filtering is unchanged.");
        Check(CmfDashboardDateFilter.TryCreate("today", "submitted", "", "", today, out filter, out error)
            && filter.BuildPendingPredicate("src") == filter.BuildPredicate("src"), "Pending submission date filtering is unchanged.");
        foreach (string mode in new[] { "all", "today", "7days", "30days", "12months", "custom" })
        {
            Check(CmfDashboardDateFilter.TryCreate(mode, "cmf-accepted", "2026-10-01", "2026-10-29", today, out filter, out error), "Accepted basis: " + mode);
            if (mode == "all")
            {
                Check(filter.BuildPredicate("src") == "1 = 1", "All dates must remain unfiltered for accepted basis.");
                Check(filter.BuildPendingPredicate("src") == "1 = 1", "All dates must retain pending requests.");
                continue;
            }
            string predicate = filter.BuildPredicate("src");
            Check(predicate.Contains("src.cmf_request IN ('cmf_ok', 'cmf_duplicate') AND src.date_cmf_decided >="), "Accepted CMF decisions only.");
            Check(predicate.Contains("src.date_cmf_decided < CONVERT(datetime, '20261030', 112)"), "Accepted end date is exclusive.");
            Check(!predicate.Contains("date_cmf_ask") && !predicate.Contains("submitted_date"), "Acceptance must not use request or submission dates.");
            Check(filter.BuildPredicate(null).StartsWith("cmf_request IN ('cmf_ok', 'cmf_duplicate') AND date_cmf_decided >="), "Accepted predicate without alias.");
            Check(filter.BuildPendingPredicate("pending_dates") == "1 = 0", "Pending requests cannot match an acceptance date and must not reference absent decision columns.");
        }
        var trendToday = new DateTime(2026, 11, 3, 15, 30, 0);
        var totalDown = CmfSummaryTrend.Create(10m, 16m, false, false, trendToday, "Accepted");
        Check(totalDown.Direction == "down" && totalDown.Tone == "good" && totalDown.Display == "6", "Total decreases are green count differences.");
        var totalUp = CmfSummaryTrend.Create(16m, 10m, false, false, trendToday, "Accepted");
        Check(totalUp.Direction == "up" && totalUp.Tone == "bad", "Total increases are red.");
        var closedUp = CmfSummaryTrend.Create(8m, 4m, false, true, trendToday, "Closed");
        Check(closedUp.Direction == "up" && closedUp.Tone == "good" && closedUp.Display == "4", "Closed and implemented increases are green.");
        var implementedDown = CmfSummaryTrend.Create(3m, 7m, false, true, trendToday, "Implemented");
        Check(implementedDown.Direction == "down" && implementedDown.Tone == "bad" && implementedDown.Display == "4", "Closed and implemented decreases are red.");
        var faster = CmfSummaryTrend.Create(5m, 10m, true, false, trendToday, "TPT");
        Check(faster.Direction == "down" && faster.Tone == "good" && faster.Display == "50%", "TPT compares percentages of unrounded averages.");
        var slower = CmfSummaryTrend.Create(15m, 10m, true, false, trendToday, "TPT");
        Check(slower.Direction == "up" && slower.Tone == "bad" && slower.Display == "50%", "TPT increases are red.");
        var fractionalIncrease = CmfSummaryTrend.Create(11.39m, 10m, true, false, trendToday, "TPT");
        Check(fractionalIncrease.Display == "13%" && fractionalIncrease.Direction == "up" && fractionalIncrease.Tone == "bad", "Fractional TPT increases truncate instead of rounding.");
        Check(fractionalIncrease.RecentValue == 11.39m && fractionalIncrease.PreviousValue == 10m
            && fractionalIncrease.Tooltip.Contains("11.39 days"), "TPT display truncation preserves the underlying averages and tooltip precision.");
        var fractionalDecrease = CmfSummaryTrend.Create(5.61m, 10m, true, false, trendToday, "TPT");
        Check(fractionalDecrease.Display == "43%" && fractionalDecrease.Direction == "down" && fractionalDecrease.Tone == "good", "Fractional TPT decreases truncate instead of rounding.");
        Check(CmfSummaryTrend.Create(20.099m, 10m, true, false, trendToday, "TPT").Display == "100%", "TPT changes above 100 percent also truncate.");
        foreach (bool percentage in new[] { false, true })
        {
            var flat = CmfSummaryTrend.Create(0m, 0m, percentage, false, trendToday, "Flat");
            Check(flat.Direction == "flat" && flat.Tone == "neutral" && flat.Display == (percentage ? "0%" : "0"), "Equal values, including zero, use a blue flat indicator.");
        }
        var missing = CmfSummaryTrend.Create(null, 4m, true, false, trendToday, "Missing");
        Check(missing.Direction == "unavailable" && missing.Display == string.Empty, "Missing TPT cohorts leave the trend blank, not zero or N/A.");
        Check(CmfSummaryTrend.Create(4m, null, true, false, trendToday, "Missing").Direction == "unavailable", "A missing previous cohort is unavailable.");
        Check(CmfSummaryTrend.Create(null, null, true, false, trendToday, "Missing").Direction == "unavailable", "Two missing cohorts are not a flat trend.");
        Check(CmfSummaryTrend.Create(4m, null, true, false, trendToday, "Missing").Display == string.Empty, "A missing previous TPT cohort leaves the trend blank.");
        Check(CmfSummaryTrend.Create(null, null, true, false, trendToday, "Missing").Display == string.Empty, "Two missing TPT cohorts leave the trend blank.");
        Check(CmfSummaryTrend.Create(null, 4m, false, false, trendToday, "Missing count").Display == "N/A", "Non-TPT unavailable count handling is unchanged.");
        var zeroBaseline = CmfSummaryTrend.Create(4m, 0m, true, false, trendToday, "Zero");
        Check(zeroBaseline.Display == string.Empty && zeroBaseline.Direction == "up", "An undefined TPT percentage against a zero baseline leaves the trend blank.");
        Check(CmfSummaryTrend.Create(0m, 4m, true, false, trendToday, "Zero").Display == "100%", "Dropping to zero is a 100 percent reduction.");
        foreach (decimal recent in new[] { 10.001m, 10.099m, 9.999m, 9.901m })
        {
            var small = CmfSummaryTrend.Create(recent, 10m, true, false, trendToday, "Small");
            Check(small.Display == "0%" && small.Direction == (recent > 10m ? "up" : "down")
                && small.Tone == (recent > 10m ? "bad" : "good"), "Sub-one-percent TPT changes truncate to zero while retaining their actual direction and tone.");
        }
        Check(totalDown.Tooltip.Contains("Oct 28, 2026 - Nov 3, 2026") && totalDown.Tooltip.Contains("Oct 21, 2026 - Oct 27, 2026"), "Two adjacent seven-day calendar windows include today.");
        var pendingDown = CmfSummaryTrend.CreatePendingEstimate(10, 14m, trendToday, 2);
        Check(pendingDown.Display == "4" && pendingDown.Direction == "down" && pendingDown.Tone == "good", "Pending reductions are green without a display prefix.");
        Check(pendingDown.Tooltip.Contains("This is an estimate, not a historical snapshot."), "The tooltip retains the estimate caveat.");
        Check(pendingDown.RecentValue == 10m && pendingDown.PreviousValue == 14m, "Pending compares stock counts, not weekly events.");
        Check(pendingDown.Tooltip.Contains("Tue Nov 3, 2026") && pendingDown.Tooltip.Contains("Tue Oct 27, 2026"), "Pending compares the same weekday seven days apart.");
        Check(pendingDown.Tooltip.Contains("2 records excluded") && pendingDown.Tooltip.Contains("end of") && pendingDown.Tooltip.Contains("partial"), "Pending estimate explains date exclusions and day boundaries.");
        var pendingUp = CmfSummaryTrend.CreatePendingEstimate(14, 10m, trendToday, 0);
        Check(pendingUp.Display == "4" && pendingUp.Direction == "up" && pendingUp.Tone == "bad", "Pending increases are red.");
        Check(CmfSummaryTrend.CreatePendingEstimate(10, 10m, trendToday, 0).Tone == "neutral", "Unchanged pending is neutral.");
        Check(CmfSummaryTrend.CreatePendingEstimate(2, 0m, trendToday, 0).Display == "2", "A zero baseline supports a count delta.");
        Check(CmfSummaryTrend.CreatePendingEstimate(10, null, trendToday, 2).Display == string.Empty, "Unusable pending history leaves the trend blank.");
        Check(CmfSummaryTrend.CreatePendingEstimate(0, null, trendToday, 0).Display == string.Empty, "Unavailable filtered pending history leaves the trend blank.");
        Check(CmfSummaryTrend.CreatePendingEstimate(0, 0m, trendToday, 0).Display == "0", "A valid zero pending change remains visible.");
        var balance = CmfSummaryTrend.CreateBalanceEstimate("Open", 10, 14m, false, trendToday, 2);
        Check(balance.RecentValue == 10m && balance.PreviousValue == 14m && balance.Display == "4", "Balance trends compare current and historical stocks, not weekly events.");
        Check(balance.Direction == "down" && balance.Tone == "good", "Open backlog reductions are green.");
        Check(balance.Tooltip.Contains("Tue Nov 3, 2026") && balance.Tooltip.Contains("end of Tue Oct 27, 2026"), "Balances compare the same weekday seven days apart.");
        Check(balance.Tooltip.Contains("2 records excluded") && balance.Tooltip.Contains("same dashboard date scope")
            && balance.Tooltip.Contains("estimate, not a historical snapshot"), "Balance tooltips disclose reconstruction, exclusions and matching date scope.");
        Check(CmfSummaryTrend.CreateBalanceEstimate("Closed", 14, 10m, true, trendToday, 0).Tone == "good", "Closed balance increases retain the green tone.");
        Check(CmfSummaryTrend.CreateBalanceEstimate("Implemented", 4, 7m, true, trendToday, 0).Tone == "bad", "Implemented balance decreases retain the red tone.");
        Check(CmfSummaryTrend.CreateBalanceEstimate("Total CMFs", 2, 0m, false, trendToday, 0).Display == "2", "A zero baseline supports a count balance delta.");
        Check(CmfSummaryTrend.CreateBalanceEstimate("Open", 0, 0m, false, trendToday, 0).Display == "0", "Valid zero balance changes remain visible.");
        Check(CmfSummaryTrend.CreateBalanceEstimate("Open", 2, null, false, trendToday, 2).Display == string.Empty, "Unusable balance history leaves the indicator blank.");
        foreach (string metric in new[] { "Total CMFs", "Closed", "Implemented", "Open" })
        {
            for (int weekday = 0; weekday < 7; weekday++)
            {
                DateTime day = trendToday.AddDays(weekday);
                var displayed = CmfSummaryTrend.CreateBalanceEstimate(metric, 70, 68m, metric == "Closed" || metric == "Implemented", day, 1).WithCurrentCount(71);
                Check(displayed.RecentValue == 71m && displayed.PreviousValue == 68m && displayed.Display == "3", "Current card count was replaced by a reconstruction-eligible count.");
                Check(displayed.Tooltip.Contains(day.AddDays(-7).ToString("ddd MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)), "Comparison is not exactly seven days earlier for " + day.DayOfWeek);
            }
        }
        foreach (string metric in new[] { "CMF Disposition TPT", "CMF Resolution TPT", "CMF Overall TPT" })
        {
            for (int weekday = 0; weekday < 7; weekday++)
            {
                DateTime day = trendToday.AddDays(weekday);
                var tpt = CmfSummaryTrend.CreateTptEstimate(metric, 0, 10.9m, day, 2, "Retained date estimate.").WithCurrentTpt(5);
                Check(tpt.RecentValue == 5m && tpt.PreviousValue == 10m && tpt.Display == "5",
                    "TPT subtracts whole-day headline values, not percentages or weekly changes.");
                Check(tpt.Direction == "down" && tpt.Tone == "good", "Lower TPT is green.");
                Check(tpt.Tooltip.Contains("5 days; estimated " + metric + " at end of "
                    + day.AddDays(-7).ToString("ddd MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)
                    + ": 10 days"), "TPT uses the same weekday exactly seven days earlier.");
                Check(tpt.Tooltip.Contains("2 records excluded") && tpt.Tooltip.Contains("same dashboard date scope"), "TPT discloses historical exclusions and scope.");
            }
            var slowerTpt = CmfSummaryTrend.CreateTptEstimate(metric, 15, 10m, trendToday, 0, "Date estimate.");
            Check(slowerTpt.Display == "5" && slowerTpt.Direction == "up" && slowerTpt.Tone == "bad", "Higher TPT is red.");
            var equalTpt = CmfSummaryTrend.CreateTptEstimate(metric, 10, 10.9m, trendToday, 0, "Date estimate.");
            Check(equalTpt.Display == "0" && equalTpt.Direction == "flat" && equalTpt.Tone == "neutral", "Equal whole-day TPT values are flat.");
            Check(CmfSummaryTrend.CreateTptEstimate(metric, 4, 0m, trendToday, 0, "Date estimate.").Display == "4", "A zero TPT baseline supports an integer difference.");
            Check(CmfSummaryTrend.CreateTptEstimate(metric, 0, null, trendToday, 1, "Date estimate.").Display == string.Empty, "Missing historical TPT samples leave the indicator blank.");
        }
        Console.WriteLine("Dashboard date range and weekly trend checks passed.");
    }
}