using System;

public static class DashboardDateFilterTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        var today = new DateTime(2026, 10, 29);
        CmfDashboardDateFilter filter;
        string error;
        foreach (string mode in new[] { "all", "today", "7days", "30days", "12months" })
        {
            Check(CmfDashboardDateFilter.TryCreate(mode, "cmf-request", "", "", today, out filter, out error), mode);
            if (mode == "all") Check(filter.BuildPredicate("src") == "1 = 1", "All Time must not exclude null dates.");
            else Check(filter.EndExclusive == today.AddDays(1), "Presets must include the whole current day.");
            if (mode == "7days") Check(filter.Start == today.AddDays(-6), "Last 7 Days boundary.");
            if (mode == "30days") Check(filter.Start == today.AddDays(-29), "Last 30 Days boundary.");
            if (mode == "12months") Check(filter.Start == today.AddMonths(-12), "Last 12 Months boundary.");
        }
        Check(CmfDashboardDateFilter.TryCreate("custom", "submitted", "2026-10-01", "2026-10-29", today, out filter, out error), error);
        Check(filter.Start == new DateTime(2026, 10, 1) && filter.EndExclusive == today.AddDays(1), "Custom boundaries.");
        Check(filter.BuildPredicate("src").Contains("src.submitted_date") && !filter.BuildPredicate("src").Contains("date_cmf_ask"), "Submission basis.");
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
        foreach (bool percentage in new[] { false, true })
        {
            var flat = CmfSummaryTrend.Create(0m, 0m, percentage, false, trendToday, "Flat");
            Check(flat.Direction == "flat" && flat.Tone == "neutral" && flat.Display == (percentage ? "0%" : "0"), "Equal values, including zero, use a blue flat indicator.");
        }
        var missing = CmfSummaryTrend.Create(null, 4m, true, false, trendToday, "Missing");
        Check(missing.Direction == "unavailable" && missing.Display == "N/A", "Missing cohorts are not zero or flat.");
        Check(CmfSummaryTrend.Create(4m, null, true, false, trendToday, "Missing").Direction == "unavailable", "A missing previous cohort is unavailable.");
        Check(CmfSummaryTrend.Create(null, null, true, false, trendToday, "Missing").Direction == "unavailable", "Two missing cohorts are not a flat trend.");
        var zeroBaseline = CmfSummaryTrend.Create(4m, 0m, true, false, trendToday, "Zero");
        Check(zeroBaseline.Display == "N/A" && zeroBaseline.Direction == "up", "A zero baseline never invents a percentage.");
        Check(CmfSummaryTrend.Create(0m, 4m, true, false, trendToday, "Zero").Display == "100%", "Dropping to zero is a 100 percent reduction.");
        Check(CmfSummaryTrend.Create(10.001m, 10m, true, false, trendToday, "Small").Display == "<0.1%", "Small changes are not displayed as no change.");
        Check(totalDown.Tooltip.Contains("Oct 28, 2026 - Nov 3, 2026") && totalDown.Tooltip.Contains("Oct 21, 2026 - Oct 27, 2026"), "Two adjacent seven-day calendar windows include today.");
        Console.WriteLine("Dashboard date range and weekly trend checks passed.");
    }
}