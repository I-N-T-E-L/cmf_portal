using System;
using System.Globalization;

public sealed class CmfDashboardDateFilter
{
    public string Mode { get; private set; }
    public string Basis { get; private set; }
    public DateTime? Start { get; private set; }
    public DateTime? EndExclusive { get; private set; }

    public bool ShowSummaryTrends
    {
        get { return !Start.HasValue || !EndExclusive.HasValue || (EndExclusive.Value - Start.Value).TotalDays >= 30; }
    }

    public static bool TryCreate(string mode, string basis, string startText, string endText,
        DateTime today, out CmfDashboardDateFilter filter, out string error)
    {
        filter = null;
        error = string.Empty;
        if (basis != "cmf-request" && basis != "submitted" && basis != "cmf-accepted")
        {
            error = "Select a valid date basis.";
            return false;
        }

        DateTime? start = null;
        DateTime? end = null;
        today = today.Date;
        switch (mode)
        {
            case "all": break;
            case "today": start = today; end = today.AddDays(1); break;
            case "7days": start = today.AddDays(-6); end = today.AddDays(1); break;
            case "30days": start = today.AddDays(-29); end = today.AddDays(1); break;
            case "12months": start = today.AddMonths(-12); end = today.AddDays(1); break;
            case "custom":
                DateTime customStart;
                DateTime customEnd;
                if (!DateTime.TryParseExact(startText, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out customStart)
                    || !DateTime.TryParseExact(endText, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out customEnd)
                    || customStart < new DateTime(1753, 1, 1)
                    || customEnd >= DateTime.MaxValue.Date || customEnd < customStart)
                {
                    error = "Enter a valid start and end date; the end date must not precede the start date.";
                    return false;
                }
                start = customStart;
                end = customEnd.AddDays(1);
                break;
            default:
                error = "Select a valid date range.";
                return false;
        }

        filter = new CmfDashboardDateFilter { Mode = mode, Basis = basis, Start = start, EndExclusive = end };
        return true;
    }

    public string BuildPendingPredicate(string alias)
    {
        return Start.HasValue && Basis == "cmf-accepted" ? "1 = 0" : BuildPredicate(alias);
    }

    public string BuildPredicate(string alias)
    {
        if (!Start.HasValue) return "1 = 1";
        string prefix = string.IsNullOrEmpty(alias) ? string.Empty : alias + ".";
        string column = prefix + (Basis == "submitted" ? "submitted_date"
            : Basis == "cmf-accepted" ? "date_cmf_decided" : "date_cmf_ask");
        string acceptedPredicate = Basis == "cmf-accepted"
            ? prefix + "cmf_request IN ('cmf_ok', 'cmf_duplicate') AND " : string.Empty;
        return acceptedPredicate + column + " >= CONVERT(datetime, '" + Start.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            + "', 112) AND " + column + " < CONVERT(datetime, '"
            + EndExclusive.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "', 112)";
    }
}