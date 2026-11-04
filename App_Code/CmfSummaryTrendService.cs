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
            Display = percentage ? string.Empty : "N/A",
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
            trend.Display = Math.Truncate(change).ToString("0", culture) + "%";
        }
        return trend;
    }

    public static CmfSummaryTrend CreateBalanceEstimate(string metric, int current, decimal? previous,
        bool increaseIsGood, DateTime today, int excludedRecords)
    {
        var culture = CultureInfo.InvariantCulture;
        var trend = Create(current, previous, false, increaseIsGood, today, metric);
        if (!previous.HasValue) trend.Display = string.Empty;
        trend.Tooltip = metric + " today (partial), " + today.Date.ToString("ddd MMM d, yyyy", culture)
            + ": " + current.ToString(culture) + "; estimated " + metric + " at end of "
            + today.Date.AddDays(-7).ToString("ddd MMM d, yyyy", culture) + ": "
            + (previous.HasValue ? previous.Value.ToString("0", culture) : "unavailable")
            + ". Reconstructed from retained acceptance, implementation and closure dates, not saved snapshots. "
            + "Both trend counts use the same dashboard date scope, reconstruction-eligible records and duplicate-row counting. "
            + excludedRecords.ToString(culture) + " records excluded from both trend counts for missing or inconsistent dates/status. "
            + "The headline card count is unchanged and may include these excluded records. "
            + "Removed records, historical eligibility changes and repeated/reopened lifecycles cannot be reconstructed. "
            + "This is an estimate, not a historical snapshot.";
        return trend;
    }

    public static CmfSummaryTrend CreatePendingEstimate(int current, decimal? previous,
        DateTime today, int excludedRecords, int? headlineCount = null)
    {
        var culture = CultureInfo.InvariantCulture;
        var trend = Create(current, previous, false, false, today, "Pending estimate");
        if (!previous.HasValue)
        {
            trend.Display = string.Empty;
        }
        trend.Tooltip = "Pending today (partial), " + today.Date.ToString("ddd MMM d, yyyy", culture)
            + ": " + current.ToString(culture) + "; estimated pending at end of "
            + today.Date.AddDays(-7).ToString("ddd MMM d, yyyy", culture) + ": "
            + (previous.HasValue ? previous.Value.ToString("0", culture) : "unavailable")
            + ". Reconstructed from retained request and decision dates, not saved snapshots. "
            + "Both trend counts use the same dashboard date scope and reconstruction-eligible requests, deduplicated by platform and HSD ID. "
            + excludedRecords.ToString(culture) + " records excluded from both trend counts for missing or reversed dates. "
            + "The headline card count is unchanged"
            + (headlineCount.HasValue ? " (" + headlineCount.Value.ToString(culture) + ")" : string.Empty)
            + " and may differ because of exclusions or duplicate rows. "
            + "Closures without a decision, removed records, and repeated pending/reopened periods cannot be reconstructed. "
            + "This is an estimate, not a historical snapshot.";
        return trend;
    }
}

public static class CmfSummaryTrendService
{
    private static string BuildBalanceEvidenceSql(string sourceSql, bool includeMetadata)
    {
        return @"
WITH base AS (
    SELECT " + (includeMetadata ? "cp_id, title, source_platform, component, component_group, promoted_id, merge_id, cmf_request, " : string.Empty) + @"status, TRY_CAST(date_cmf_ask AS date) AS ask_date,
        TRY_CAST(date_cmf_decided AS date) AS decided_date,
        TRY_CAST(implemented_date AS date) AS implemented_date,
        TRY_CAST(closed_date AS date) AS closed_date
    FROM " + sourceSql + @"
    WHERE sysdebug LIKE '%customer_must_fix%'
      AND cmf_request IN ('cmf_ok','cmf_duplicate')
), assessed AS (
    SELECT *,
        CASE WHEN decided_date IS NULL OR decided_date < ask_date THEN 1 ELSE 0 END AS invalid_decision,
        CASE WHEN status IS NULL
            OR (status IN ('complete','rejected') AND closed_date IS NULL)
            OR (status IN ('implemented','future') AND implemented_date IS NULL)
            OR (status NOT IN ('complete','rejected') AND closed_date IS NOT NULL)
            OR implemented_date < decided_date OR closed_date < decided_date
            OR closed_date < implemented_date
            THEN 1 ELSE 0 END AS invalid_status_dates
    FROM base
), comparable AS (
    SELECT *, CASE WHEN invalid_decision = 0
        AND (decided_date >= DATEADD(day,-6,@cmfBalanceToday) OR invalid_status_dates = 0)
        THEN 1 ELSE 0 END AS comparable_status
    FROM assessed
)
";
    }

    public static SqlCommand CreateBalanceEvidenceCommand(SqlConnection connection, string sourceSql, DateTime today)
    {
        var command = CmfPlatformCatalog.CreateReadCommand(BuildBalanceEvidenceSql(sourceSql, true) + "SELECT * FROM comparable", connection);
        command.Parameters.Add("@cmfBalanceToday", SqlDbType.Date).Value = today.Date;
        return command;
    }

    public static SqlCommand CreateBalanceEstimateCommand(SqlConnection connection, string sourceSql, DateTime today)
    {
        var command = CmfPlatformCatalog.CreateReadCommand(BuildBalanceEvidenceSql(sourceSql, false) + @"
SELECT COUNT(CASE WHEN invalid_decision = 0 THEN 1 END) AS current_total,
    COUNT(CASE WHEN comparable_status = 1 AND status IN ('complete','rejected') THEN 1 END) AS current_closed,
    COUNT(CASE WHEN comparable_status = 1 AND status IN ('implemented','future') THEN 1 END) AS current_implemented,
    COUNT(CASE WHEN comparable_status = 1 AND status NOT IN ('complete','rejected','implemented','future') THEN 1 END) AS current_open,
    COUNT(CASE WHEN invalid_decision = 0 AND decided_date < DATEADD(day,-6,@cmfBalanceToday)
        THEN 1 END) AS previous_total,
    COUNT(CASE WHEN comparable_status = 1
        AND decided_date < DATEADD(day,-6,@cmfBalanceToday)
        AND closed_date < DATEADD(day,-6,@cmfBalanceToday)
        THEN 1 END) AS previous_closed,
    COUNT(CASE WHEN comparable_status = 1
        AND decided_date < DATEADD(day,-6,@cmfBalanceToday)
        AND status IN ('implemented','future','complete','rejected')
        AND implemented_date < DATEADD(day,-6,@cmfBalanceToday)
        AND (closed_date IS NULL OR closed_date >= DATEADD(day,-6,@cmfBalanceToday))
        THEN 1 END) AS previous_implemented,
    COUNT(CASE WHEN comparable_status = 1
        AND decided_date < DATEADD(day,-6,@cmfBalanceToday)
        AND (closed_date IS NULL OR closed_date >= DATEADD(day,-6,@cmfBalanceToday))
        AND (status NOT IN ('complete','rejected','implemented','future')
            OR (status IN ('complete','rejected','implemented','future')
                AND (implemented_date IS NULL OR implemented_date >= DATEADD(day,-6,@cmfBalanceToday))))
        THEN 1 END) AS previous_open,
    COUNT(CASE WHEN invalid_decision = 1 THEN 1 END) AS excluded_decisions,
    COUNT(CASE WHEN comparable_status = 0 THEN 1 END) AS excluded_status_dates,
    COUNT(*) AS candidate_count
FROM comparable", connection);
        command.Parameters.Add("@cmfBalanceToday", SqlDbType.Date).Value = today.Date;
        return command;
    }

    public static Dictionary<string, CmfSummaryTrend> LoadBalanceEstimates(SqlConnection connection,
        string sourceSql, DateTime today)
    {
        string[] labels = { "Total CMFs", "Closed", "Implemented", "Open" };
        var trends = new Dictionary<string, CmfSummaryTrend>(StringComparer.OrdinalIgnoreCase);
        using (SqlCommand command = CreateBalanceEstimateCommand(connection, sourceSql, today))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            reader.Read();
            int candidates = reader.GetInt32(10);
            for (int index = 0; index < labels.Length; index++)
            {
                int excluded = reader.GetInt32(index == 0 ? 8 : 9);
                decimal? previous = candidates > 0 && excluded == candidates
                    ? (decimal?)null : reader.GetInt32(index + 4);
                trends[labels[index]] = CmfSummaryTrend.CreateBalanceEstimate(labels[index], reader.GetInt32(index),
                    previous, index == 1 || index == 2, today, excluded);
            }
        }
        return trends;
    }

    public static SqlCommand CreatePendingEstimateCommand(SqlConnection connection,
        string pendingSourceSql, string componentSourceSql, DateTime today)
    {
        var command = CmfPlatformCatalog.CreateReadCommand(@"
WITH current_pending AS (
    SELECT source_platform, CAST(cp_id AS varchar(64)) AS cp_id,
        TRY_CAST(date_cmf_ask AS date) AS ask_date
    FROM (" + pendingSourceSql + @") AS pending
    WHERE status NOT IN ('complete','rejected')
      AND cp_id IS NOT NULL
), decided_records AS (
    SELECT src.source_platform, CAST(src.cp_id AS varchar(64)) AS cp_id,
        TRY_CAST(src.date_cmf_ask AS date) AS ask_date,
        TRY_CAST(src.date_cmf_decided AS date) AS decided_date
    FROM (" + componentSourceSql + @") AS src
    WHERE src.cmf_request IN ('cmf_ok','cmf_duplicate','cmf_reject')
      AND src.cp_id IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM current_pending AS pending
          WHERE pending.source_platform = src.source_platform
            AND pending.cp_id = CAST(src.cp_id AS varchar(64))
      )
), candidates AS (
    SELECT source_platform, cp_id, ask_date, CAST(NULL AS date) AS decided_date,
        CASE WHEN ask_date IS NULL THEN 1 ELSE 0 END AS invalid_dates,
        1 AS is_current_pending
    FROM current_pending
    UNION ALL
    SELECT source_platform, cp_id, ask_date, decided_date,
        CASE WHEN ask_date IS NULL OR decided_date IS NULL OR decided_date < ask_date
            THEN 1 ELSE 0 END, 0
    FROM decided_records
), requests AS (
    SELECT source_platform, cp_id, MIN(ask_date) AS ask_date,
        MAX(decided_date) AS decided_date, MAX(invalid_dates) AS invalid_dates,
        MAX(is_current_pending) AS is_current_pending
    FROM candidates
    GROUP BY source_platform, cp_id
)
SELECT COUNT(CASE WHEN invalid_dates = 0
        AND ask_date < DATEADD(day,-6,@cmfPendingToday)
        AND (decided_date IS NULL OR decided_date >= DATEADD(day,-6,@cmfPendingToday))
        THEN 1 END) AS previous_pending,
    COUNT(CASE WHEN invalid_dates = 1 THEN 1 END) AS excluded_records,
    COUNT(*) AS candidate_count,
    COUNT(CASE WHEN invalid_dates = 0 AND is_current_pending = 1 THEN 1 END) AS current_pending
FROM requests", connection);
        command.Parameters.Add("@cmfPendingToday", SqlDbType.Date).Value = today.Date;
        return command;
    }

    public static CmfSummaryTrend LoadPendingEstimate(SqlConnection connection,
        string pendingSourceSql, string componentSourceSql, int current, DateTime today)
    {
        using (SqlCommand command = CreatePendingEstimateCommand(connection, pendingSourceSql, componentSourceSql, today))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            reader.Read();
            int previous = reader.GetInt32(0);
            int excluded = reader.GetInt32(1);
            int candidates = reader.GetInt32(2);
            return CmfSummaryTrend.CreatePendingEstimate(reader.GetInt32(3),
                candidates > 0 && excluded == candidates ? (decimal?)null : previous,
                today, excluded, current);
        }
    }

    public static SqlCommand CreateCommand(SqlConnection connection, string sourceSql, DateTime today)
    {
        var command = CmfPlatformCatalog.CreateReadCommand(@"
WITH base AS (
    SELECT TRY_CAST(date_cmf_ask AS date) AS ask_date,
        TRY_CAST(date_cmf_decided AS date) AS decided_date,
        TRY_CAST(implemented_date AS date) AS implemented_date,
        TRY_CAST(closed_date AS date) AS closed_date,
        TRY_CAST(days_active AS int) AS days_active
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
        THEN CAST(DATEDIFF(day,decided_date,implemented_date) AS float) END) AS ResolutionAverage,
    AVG(CASE WHEN decided_date >= start_date AND decided_date < end_date AND days_active >= 0
        THEN CAST(days_active AS float) END) AS OverallAverage
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
        decimal?[] recent = new decimal?[6];
        decimal?[] previous = new decimal?[6];
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
        string[] labels = { "Total CMFs", "Closed", "Implemented", "CMF Disposition TPT", "CMF Resolution TPT", "CMF Overall TPT" };
        string[] descriptions = { "CMFs accepted per week", "Recorded CMF closures per week",
            "Recorded CMF implementations per week, including now-closed CMFs",
            "Average request-to-decision days for decisions each week; missing or reversed dates excluded",
            "Average decision-to-implementation days for implementations each week; missing or reversed dates excluded",
            "Average current recorded days_active for CMFs accepted each week; missing, nonnumeric or negative durations excluded. Compares acceptance cohorts, not reconstructed historical headline averages" };
        var trends = new Dictionary<string, CmfSummaryTrend>(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < labels.Length; index++)
        {
            trends[labels[index]] = CmfSummaryTrend.Create(recent[index], previous[index],
                index >= 3, index == 1 || index == 2, today, descriptions[index]);
        }
        return trends;
    }
}