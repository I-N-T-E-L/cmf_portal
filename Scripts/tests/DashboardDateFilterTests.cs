using System;
using System.Data.SqlClient;

public static class DashboardDateFilterTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
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
        Check(estimate.PreviousValue == 6m && estimate.RecentValue == 5m, "Both Pending trend counts use valid deduplicated requests; the same ID on different platforms is retained.");
        Check(estimate.Display == "1" && estimate.Tone == "good", "The eligible Pending cohort produces a green reduction without a display prefix.");
        Check(estimate.Tooltip.Contains("4 records excluded from both trend counts")
            && estimate.Tooltip.Contains("headline card count is unchanged (10)"), "SQL reports symmetric date exclusions and the unchanged headline separately.");
        CmfSummaryTrend platformA = CmfSummaryTrendService.LoadPendingEstimate(connection,
            pendingSource + " WHERE source_platform = 'A'", componentSource + " WHERE source_platform = 'A'", 9, today);
        CmfSummaryTrend platformB = CmfSummaryTrendService.LoadPendingEstimate(connection,
            pendingSource + " WHERE source_platform = 'B'", componentSource + " WHERE source_platform = 'B'", 1, today);
        Check(platformA.PreviousValue == 5m && platformA.RecentValue == 4m && platformA.Display == "1", "Platform A uses only its own valid pending and decision records on both sides.");
        Check(platformB.PreviousValue == 1m && platformB.RecentValue == 1m && platformB.Display == "0" && platformB.Tone == "neutral", "Platform B is isolated from platform A despite a shared HSD ID.");
        string emptyPending = pendingSource + " WHERE 1 = 0";
        string emptyComponents = componentSource + " WHERE 1 = 0";
        CmfSummaryTrend empty = CmfSummaryTrendService.LoadPendingEstimate(connection, emptyPending, emptyComponents, 0, today);
        Check(empty.PreviousValue == 0m && empty.Display == "0", "Empty history has a valid zero estimate.");
        CmfSummaryTrend invalid = CmfSummaryTrendService.LoadPendingEstimate(connection,
            pendingSource + " WHERE cp_id = 'missing'", emptyComponents, 1, today);
        Check(invalid.RecentValue == 0m && invalid.PreviousValue == null && invalid.Display == string.Empty && invalid.Direction == "unavailable", "Wholly unusable history excludes today's records and leaves the trend blank.");
        string conflictingPending = "SELECT * FROM (" + pendingSource + @") AS original WHERE cp_id = 'stay'
UNION ALL SELECT 'A','stay','open',NULL";
        CmfSummaryTrend conflicting = CmfSummaryTrendService.LoadPendingEstimate(connection,
            conflictingPending, emptyComponents, 4, today);
        Check(conflicting.RecentValue == 1m && conflicting.PreviousValue == 1m && conflicting.Display == "0",
            "An invalid duplicate excludes that platform/ID from both counts, without excluding the same ID on another platform.");
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
        Console.WriteLine("Dashboard date range and weekly trend checks passed.");
    }
}