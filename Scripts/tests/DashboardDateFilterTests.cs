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
        Console.WriteLine("Dashboard date range checks passed.");
    }
}