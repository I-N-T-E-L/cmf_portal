using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

public sealed class CmfSummaryTrend
{
    public decimal? RecentValue { get; private set; }
    public decimal? PreviousValue { get; private set; }
    public string Direction { get; private set; }
    public string Tone { get; private set; }
    public string Display { get; private set; }
    public string Tooltip { get; private set; }

    public static CmfSummaryTrend Create(decimal? recent, decimal? previous,
        bool percentage, bool increaseIsGood, DateTime today, string description)
    {
        var culture = CultureInfo.InvariantCulture;
        string unit = percentage ? " days" : " CMFs";
        var trend = new CmfSummaryTrend
        {
            RecentValue = recent,
            PreviousValue = previous,
            Direction = "unavailable",
            Tone = "unavailable",
            Display = "N/A",
            Tooltip = description + ". "
                + today.Date.AddDays(-6).ToString("MMM d, yyyy", culture) + " - "
                + today.Date.ToString("MMM d, yyyy", culture) + ": "
                + (recent.HasValue ? recent.Value.ToString("0.###", culture) + unit : "no samples")
                + "; " + today.Date.AddDays(-13).ToString("MMM d, yyyy", culture) + " - "
                + today.Date.AddDays(-7).ToString("MMM d, yyyy", culture) + ": "
                + (previous.HasValue ? previous.Value.ToString("0.###", culture) + unit : "no samples")
                + ". Includes today (partial). Fixed rolling windows, independent of the dashboard date filter."
        };
        if (!recent.HasValue || !previous.HasValue)
        {
            return trend;
        }

        decimal delta = recent.Value - previous.Value;
        trend.Direction = delta == 0 ? "flat" : delta > 0 ? "up" : "down";
        trend.Tone = delta == 0 ? "neutral"
            : (delta > 0) == increaseIsGood ? "good" : "bad";
        if (!percentage)
        {
            trend.Display = Math.Abs(delta).ToString("0", culture);
        }
        else if (delta == 0)
        {
            trend.Display = "0%";
        }
        else if (previous.Value == 0)
        {
            trend.Tooltip += " Percentage unavailable because the previous average is zero.";
        }
        else
        {
            decimal change = Math.Abs(delta / previous.Value * 100m);
            decimal rounded = Math.Round(change, 1, MidpointRounding.AwayFromZero);
            trend.Display = rounded == 0 ? "<0.1%" : rounded.ToString("0.#", culture) + "%";
        }
        return trend;
    }
}

public static class CmfSummaryTrendService
{
    public static SqlCommand CreateCommand(SqlConnection connection, string sourceSql, DateTime today)
    {
        var command = new SqlCommand(@"
WITH base AS (
    SELECT TRY_CAST(date_cmf_ask AS date) AS ask_date,
        TRY_CAST(date_cmf_decided AS date) AS decided_date,
        TRY_CAST(implemented_date AS date) AS implemented_date,
        TRY_CAST(closed_date AS date) AS closed_date
    FROM " + sourceSql + @"
    WHERE sysdebug LIKE '%customer_must_fix%'
      AND cmf_request IN ('cmf_ok','cmf_duplicate')
), windows AS (
    SELECT 'recent' AS period_name, DATEADD(day,-6,@cmfTrendToday) AS start_date,
        DATEADD(day,1,@cmfTrendToday) AS end_date
    UNION ALL
    SELECT 'previous', DATEADD(day,-13,@cmfTrendToday), DATEADD(day,-6,@cmfTrendToday)
)
SELECT windows.period_name,
    COUNT(CASE WHEN decided_date >= start_date AND decided_date < end_date THEN 1 END) AS TotalCount,
    COUNT(CASE WHEN closed_date >= start_date AND closed_date < end_date THEN 1 END) AS ClosedCount,
    COUNT(CASE WHEN implemented_date >= start_date AND implemented_date < end_date THEN 1 END) AS ImplementedCount,
    AVG(CASE WHEN decided_date >= start_date AND decided_date < end_date AND decided_date >= ask_date
        THEN CAST(DATEDIFF(day,ask_date,decided_date) AS float) END) AS DispositionAverage,
    AVG(CASE WHEN implemented_date >= start_date AND implemented_date < end_date AND implemented_date >= decided_date
        THEN CAST(DATEDIFF(day,decided_date,implemented_date) AS float) END) AS ResolutionAverage
FROM windows
LEFT JOIN base ON (decided_date >= start_date AND decided_date < end_date)
    OR (closed_date >= start_date AND closed_date < end_date)
    OR (implemented_date >= start_date AND implemented_date < end_date)
GROUP BY windows.period_name", connection);
        command.Parameters.Add("@cmfTrendToday", SqlDbType.Date).Value = today.Date;
        return command;
    }

    public static Dictionary<string, CmfSummaryTrend> Load(SqlConnection connection, string sourceSql, DateTime today)
    {
        decimal?[] recent = new decimal?[5];
        decimal?[] previous = new decimal?[5];
        using (SqlCommand command = CreateCommand(connection, sourceSql, today))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                decimal?[] values = reader.GetString(0) == "recent" ? recent : previous;
                for (int index = 0; index < values.Length; index++)
                {
                    values[index] = reader.IsDBNull(index + 1) ? (decimal?)null
                        : Convert.ToDecimal(reader.GetValue(index + 1), CultureInfo.InvariantCulture);
                }
            }
        }
        string[] labels = { "Total CMFs", "Closed", "Implemented", "CMF Disposition TPT", "CMF Resolution TPT" };
        string[] descriptions = { "CMFs accepted per week", "Recorded CMF closures per week",
            "Recorded CMF implementations per week, including now-closed CMFs",
            "Average request-to-decision days for decisions each week; missing or reversed dates excluded",
            "Average decision-to-implementation days for implementations each week; missing or reversed dates excluded" };
        var trends = new Dictionary<string, CmfSummaryTrend>(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < labels.Length; index++)
        {
            trends[labels[index]] = CmfSummaryTrend.Create(recent[index], previous[index],
                index >= 3, index == 1 || index == 2, today, descriptions[index]);
        }
        return trends;
    }
}