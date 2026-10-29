using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using OfficeOpenXml;
using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceModel.Activities;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Spreadsheet = DocumentFormat.OpenXml.Spreadsheet;

public class CMFIssueCountTemplate : ITemplate
{
    private string _dataField;
    private string _driver;
    private string _issueType;

    public CMFIssueCountTemplate(string dataField, string driver, string issueType)
    {
        _dataField = dataField;
        _driver = driver;
        _issueType = issueType;
    }

    public void InstantiateIn(System.Web.UI.Control container)
    {
        System.Web.UI.WebControls.Literal literal = new System.Web.UI.WebControls.Literal();
        literal.DataBinding += new EventHandler(literal_DataBinding);
        container.Controls.Add(literal);
    }

    void literal_DataBinding(object sender, EventArgs e)
    {
        System.Web.UI.WebControls.Literal literal = (System.Web.UI.WebControls.Literal)sender;
        GridViewRow row = (GridViewRow)literal.NamingContainer;

        object valueObj = DataBinder.Eval(row.DataItem, _dataField);
        object componentObj = DataBinder.Eval(row.DataItem, "Component");

        string value = valueObj != null ? valueObj.ToString() : "0";
        string component = componentObj != null ? componentObj.ToString() : "";

        string numericValue = value;
        if (value.Contains("("))
        {
            numericValue = value.Substring(0, value.IndexOf("("));
        }

        string cleanValue = System.Text.RegularExpressions.Regex.Replace(numericValue, @"[^\d]", "");

        if (!string.IsNullOrEmpty(cleanValue) && cleanValue != "0")
        {
            string linkColor = component == "Total" ? "black" : "";

            literal.Text = string.Format(
                @"<a href=""javascript:void(0)"" onclick='showCMFIssues(""{0}"", ""{1}"", ""{2}""); return false;' style=""color:{3}; text-decoration:none; font-weight:bold;"" class=""btn btn-link p-0"">{4}</a>",
                HttpUtility.JavaScriptStringEncode(component),
                HttpUtility.JavaScriptStringEncode(_driver),
                HttpUtility.JavaScriptStringEncode(_issueType),
                linkColor,
                value
            );
        }
        else
        {
            literal.Text = string.Format("<span style=\"font-weight:bold;\">{0}</span>", value);
        }
    }
}

public class PageNumberItem
{
    public int PageNumber { get; set; }
    public bool IsCurrentPage { get; set; }
}

public class PageGroupInfo
{
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public bool HasPreviousGroup { get; set; }
    public bool HasNextGroup { get; set; }
}

public class HomeDashboardTrendPoint
{
    public string WeekLabel { get; set; }
    public DateTime WeekStart { get; set; }
    public int NewIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public int NeedsAttention { get; set; }
}

public class HomeDashboardCategoryPoint
{
    public string Name { get; set; }
    public int Value { get; set; }
}

public class HomeDashboardFact
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string Note { get; set; }
    public string TrendNote { get; set; }
    public string ConstituentTooltip { get; set; }
}

public class HomeDashboardTable
{
    public List<string> Columns { get; set; }
    public List<List<string>> Rows { get; set; }
}

public class HomeDashboardOemRow
{
    public string OemName { get; set; }
    public int TotalIssues { get; set; }
    public int OpenIssues { get; set; }
    public int HighRiskCmfs { get; set; }
    public int CmfPending { get; set; }
    public int Implemented { get; set; }
    public int Closed { get; set; }
    public decimal AvgResolutionDays { get; set; }
    public string RiskLevel { get; set; }
    public int TrendPercent { get; set; }
}

public class HomeDashboardHeatmapRow
{
    public string Ingredient { get; set; }
    public List<int> Values { get; set; }
    public List<string> StatusBreakdowns { get; set; }
}

public class SummaryIssueLinksGroup
{
    public string GroupName { get; set; }
    public List<string> HsdIds { get; set; }
}

public class SummaryIssueLinksResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string MetricKey { get; set; }
    public string GroupBy { get; set; }
    public bool IsOverallScope { get; set; }
    public List<string> AvailableGroupBy { get; set; }
    public int TotalIssues { get; set; }
    public List<SummaryIssueLinksGroup> Groups { get; set; }
    public List<string> IdstNames { get; set; }
}

public class HomeDashboardSnapshot
{
    public string PlatformLabel { get; set; }
    public string GeneratedAt { get; set; }
    public int ActiveIssues { get; set; }
    public int NeedsAttention { get; set; }
    public int ClosedIssues { get; set; }
    public int StaleIssues { get; set; }
    public int ResolvedThisWeek { get; set; }
    public decimal AverageResolutionDays { get; set; }
    public int CustomersAffected { get; set; }
    public int PendingIssues { get; set; }
    public int NewToday { get; set; }
    public int ResolvedToday { get; set; }
    public int AiWatchlist { get; set; }
    public int ProgramReadinessScore { get; set; }
    public string ProgramRiskLevel { get; set; }
    public string TopRisk { get; set; }
    public string RiskConcentration { get; set; }
    public List<string> PredictedBlockers { get; set; }
    public List<string> WeeklyChanges { get; set; }
    public List<HomeDashboardFact> SummaryFacts { get; set; }
    public List<HomeDashboardFact> TptFacts { get; set; }
    public HomeDashboardTable MilestoneSummary { get; set; }
    public HomeDashboardTable ComponentSummary { get; set; }
    public HomeDashboardTable PendingSummary { get; set; }
    public HomeDashboardTable DesignSummary { get; set; }
    public List<HomeDashboardTrendPoint> Trend { get; set; }
    public List<HomeDashboardCategoryPoint> StatusDistribution { get; set; }
    public List<HomeDashboardCategoryPoint> TopComponents { get; set; }
    public List<HomeDashboardOemRow> OemSummary { get; set; }
    public List<string> HeatmapPlatforms { get; set; }
    public List<HomeDashboardHeatmapRow> ComponentRiskHeatmap { get; set; }
    public List<HomeDashboardCategoryPoint> PendingAgeBuckets { get; set; }
}

public partial class CMF_Web_portal : System.Web.UI.Page
{
    private const string PendingSummaryLinksDelimiter = ":::OPEN_HSDS:::";
    private const string PendingSummaryOverTwoDaysDelimiter = ":::OVER_TWO_DAYS:::";
    private const string PendingSummaryOverTwoDaysIdsDelimiter = ":::OVER_TWO_DAYS_IDS:::";
    private const string PendingSummaryAgeThresholdSessionKey = "pendingSummaryAgeThresholdDays";
    private const int DefaultPendingSummaryAgeThresholdDays = 2;
    private const string ActiveFocusedTabSessionKey = "activeFocusedTab";
    private const string UserModeSessionKey = "portalUserMode";
    private const string DashboardPlatformSessionKey = "dashboardSelectedPlatform";
    private const string IssuePendingPlatformSessionKey = "issuePendingSelectedPlatform";
    private const string PendingPlatformsSessionKey = "cmfPendingSelectedPlatforms";
    private const string DefaultIssuePendingPlatformTable = "CMF_NVL_H_ALL_COMPONENTS_TABLE";
    private const string OverallExcludedPlatformsSessionKey = "overallExcludedPlatforms";
    private const string AllMilestonesDriverToken = "__ALL_MILESTONES__";
    private const string IssueGridCacheKeySessionKey = "issueGridCacheKey";
    private const string IssueGridCacheDataSessionKey = "issueGridCacheData";
    private const string IssueGlobalSearchSessionKey = "issueGlobalSearch";
    private static readonly string DefaultPlatformTable = CmfPlatformCatalog.GetDefaultPlatformTableName();
    private const string OverallDashboardPlatformTable = CmfPlatformCatalog.OverallPlatformTableName;

    private static readonly string[] DashboardAggregatePlatformTables = CmfPlatformCatalog.GetAllPlatformTables();

    private static readonly object AssistantContextCacheSync = new object();
    private static readonly HashSet<string> AllowedPlatformTables = CmfPlatformCatalog.GetAllowedPlatformTables();
    private static readonly CmfPlatformDefinition[] OrderedPlatformOptions = CmfPlatformCatalog.GetOrderedPlatforms();

    private List<string> drivers = new List<string>();

    public string DriversJson
    {
        get
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(drivers.ToArray());
        }
    }

    public string[] DriversArray
    {
        get
        {
            return drivers.ToArray();
        }
    }

    protected string AvailablePlatformTablesJson { get; private set; }

    protected string AvailablePlatformChipOptionsJson { get; private set; }

    protected string OverallExcludedPlatformsJson { get; private set; }

    protected string HomeDashboardSnapshotJson { get; private set; }

    protected string HomeDashboardPlatformLabel { get; private set; }

    protected int PendingSummaryAgeThresholdDaysForClient
    {
        get { return GetPendingSummaryAgeThresholdDays(); }
    }

    public static string WorkWeek;
    private Dictionary<string, string> driverColumns;

    private void SetWorkWeek()
    {
        DateTime currentDate = DateTime.Now;
        CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        int workWeekNumber = cultureInfo.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        WorkWeek = string.Format("WW'{0:D2}", workWeekNumber);
    }

    private string headerTemplate = WorkWeek + " | CMF Live Dashboard - ";
    private string selectedPlatform;
    private string selectedValue;
    private string driver = " | CMF Live Dashboard - All Milestones";
    private int totalCount = 0;
    private int duplicates = 0;
    private int closedCount = 0;
    private int implementedCount = 0;
    private string implementedComponents = "";

    private string ConnectionString
    {
        get { return ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString; }
    }

    protected string ResolvePlatformTable(string platform = null)
    {
        string resolvedPlatform = platform
            ?? Session["selectedPlatform"] as string
            ?? ddlTables.SelectedValue
            ?? selectedPlatform;

        if (string.IsNullOrWhiteSpace(resolvedPlatform) || !AllowedPlatformTables.Contains(resolvedPlatform))
        {
            throw new InvalidOperationException("Invalid platform table selection.");
        }

        return resolvedPlatform;
    }

    private bool IsOverallDashboardPlatform(string platform)
    {
        return string.Equals(platform, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase);
    }

    protected string ResolveDashboardPlatformSelection(string platform = null)
    {
        string resolvedSelection = platform
            ?? Session[DashboardPlatformSessionKey] as string
            ?? Session["selectedPlatform"] as string
            ?? ddlTables.SelectedValue
            ?? selectedPlatform
            ?? DefaultPlatformTable;

        if (IsOverallDashboardPlatform(resolvedSelection))
        {
            return OverallDashboardPlatformTable;
        }

        return ResolvePlatformTable(resolvedSelection);
    }

    private static int NormalizePendingSummaryAgeThresholdDays(int thresholdDays)
    {
        if (thresholdDays < 0)
        {
            return 0;
        }

        return thresholdDays;
    }

    private int GetPendingSummaryAgeThresholdDays()
    {
        object raw = Session[PendingSummaryAgeThresholdSessionKey];
        if (raw == null)
        {
            return DefaultPendingSummaryAgeThresholdDays;
        }

        if (raw is int)
        {
            return NormalizePendingSummaryAgeThresholdDays((int)raw);
        }

        int parsed;
        if (int.TryParse(Convert.ToString(raw, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
        {
            return NormalizePendingSummaryAgeThresholdDays(parsed);
        }

        return DefaultPendingSummaryAgeThresholdDays;
    }

    private int ParsePendingSummaryAgeThresholdDaysFromArgument(string eventArgument)
    {
        string decoded = HttpUtility.UrlDecode(eventArgument ?? string.Empty) ?? string.Empty;
        long parsedLong;
        if (long.TryParse(decoded, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedLong))
        {
            if (parsedLong > int.MaxValue)
            {
                return int.MaxValue;
            }

            return NormalizePendingSummaryAgeThresholdDays((int)parsedLong);
        }

        StringBuilder digits = new StringBuilder();
        for (int index = 0; index < decoded.Length; index++)
        {
            char token = decoded[index];
            if (char.IsDigit(token))
            {
                digits.Append(token);
            }
        }

        if (digits.Length > 0 && long.TryParse(digits.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedLong))
        {
            if (parsedLong > int.MaxValue)
            {
                return int.MaxValue;
            }

            return NormalizePendingSummaryAgeThresholdDays((int)parsedLong);
        }

        return GetPendingSummaryAgeThresholdDays();
    }

    private void SetPendingSummaryAgeThresholdDays(int thresholdDays)
    {
        Session[PendingSummaryAgeThresholdSessionKey] = NormalizePendingSummaryAgeThresholdDays(thresholdDays);
    }

    private void SetDashboardPlatformSelection(string platform)
    {
        Session[DashboardPlatformSessionKey] = ResolveDashboardPlatformSelection(platform);
    }

    private List<CmfPlatformDefinition> GetAvailableIndividualPlatforms()
    {
        const string cacheKey = "CMF_AvailableIndividualPlatforms";
        if (HttpContext.Current != null)
        {
            List<CmfPlatformDefinition> cached = HttpContext.Current.Items[cacheKey] as List<CmfPlatformDefinition>;
            if (cached != null)
            {
                return cached;
            }
        }

        List<CmfPlatformDefinition> available = new List<CmfPlatformDefinition>();
        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            connection.Open();
            foreach (CmfPlatformDefinition option in OrderedPlatformOptions)
            {
                if (!SqlTableExists(connection, option.TableName))
                {
                    continue;
                }

                if (SqlTableHasRows(connection, option.TableName))
                {
                    available.Add(option);
                }
            }
        }

        if (available.Count == 0)
        {
            CmfPlatformDefinition fallback = OrderedPlatformOptions.FirstOrDefault(p =>
                string.Equals(p.TableName, DefaultPlatformTable, StringComparison.OrdinalIgnoreCase))
                ?? OrderedPlatformOptions.FirstOrDefault();

            if (fallback != null)
            {
                available.Add(fallback);
            }
        }

        if (HttpContext.Current != null)
        {
            HttpContext.Current.Items[cacheKey] = available;
        }

        return available;
    }

    private string ResolveFirstAvailablePlatform(IEnumerable<string> candidates, HashSet<string> availableSet, List<CmfPlatformDefinition> availablePlatforms)
    {
        if (candidates != null)
        {
            foreach (string candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && availableSet.Contains(candidate))
                {
                    return candidate;
                }
            }
        }

        if (availablePlatforms != null && availablePlatforms.Count > 0)
        {
            return availablePlatforms[0].TableName;
        }

        return DefaultPlatformTable;
    }

    private void RebuildGlobalPlatformDropdown(List<CmfPlatformDefinition> availablePlatforms, HashSet<string> availableSet)
    {
        if (ddlTables == null)
        {
            return;
        }

        string postedPlatform = Request.Form[ddlTables.UniqueID];
        string sessionPlatform = Session["selectedPlatform"] as string;
        string controlPlatform = ddlTables.SelectedValue;

        ddlTables.Items.Clear();
        foreach (CmfPlatformDefinition option in availablePlatforms)
        {
            ddlTables.Items.Add(new ListItem(option.Label, option.TableName));
        }

        string preferredPlatform = ResolveFirstAvailablePlatform(
            new[] { postedPlatform, sessionPlatform, controlPlatform, selectedPlatform, DefaultPlatformTable },
            availableSet,
            availablePlatforms);

        ListItem preferredItem = ddlTables.Items.FindByValue(preferredPlatform);
        if (preferredItem != null)
        {
            ddlTables.ClearSelection();
            preferredItem.Selected = true;
        }
    }

    private void NormalizePlatformSelections(List<CmfPlatformDefinition> availablePlatforms, HashSet<string> availableSet)
    {
        string fallbackPlatform = ResolveFirstAvailablePlatform(new[] { DefaultPlatformTable }, availableSet, availablePlatforms);
        string currentSelection = ddlTables != null ? ddlTables.SelectedValue : null;
        if (string.IsNullOrWhiteSpace(currentSelection) || !availableSet.Contains(currentSelection))
        {
            currentSelection = fallbackPlatform;
            if (ddlTables != null)
            {
                ListItem selectedItem = ddlTables.Items.FindByValue(currentSelection);
                if (selectedItem != null)
                {
                    ddlTables.ClearSelection();
                    selectedItem.Selected = true;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(currentSelection))
        {
            selectedPlatform = currentSelection;
            Session["selectedPlatform"] = currentSelection;
        }

        string issuePendingSelection = Session[IssuePendingPlatformSessionKey] as string;
        if (!IsOverallDashboardPlatform(issuePendingSelection))
        {
            Session[IssuePendingPlatformSessionKey] = ResolveFirstAvailablePlatform(
                new[] { issuePendingSelection, currentSelection, fallbackPlatform },
                availableSet,
                availablePlatforms);
        }

        string dashboardSelection = Session[DashboardPlatformSessionKey] as string;
        if (!string.IsNullOrWhiteSpace(dashboardSelection) && !IsOverallDashboardPlatform(dashboardSelection))
        {
            Session[DashboardPlatformSessionKey] = ResolveFirstAvailablePlatform(
                new[] { dashboardSelection, currentSelection, fallbackPlatform },
                availableSet,
                availablePlatforms);
        }
    }

    private void ApplyRuntimePlatformAvailability()
    {
        List<CmfPlatformDefinition> availablePlatforms = GetAvailableIndividualPlatforms();
        HashSet<string> availableSet = new HashSet<string>(
            availablePlatforms.Select(option => option.TableName),
            StringComparer.OrdinalIgnoreCase);

        RebuildGlobalPlatformDropdown(availablePlatforms, availableSet);
        NormalizePlatformSelections(availablePlatforms, availableSet);

        List<string> availableForClient = new List<string>();
        List<Dictionary<string, string>> chipOptionsForClient = new List<Dictionary<string, string>>();
        if (availablePlatforms.Count > 0)
        {
            availableForClient.Add(OverallDashboardPlatformTable);
            chipOptionsForClient.Add(new Dictionary<string, string>
            {
                { "label", "Overall" },
                { "value", OverallDashboardPlatformTable }
            });
        }

        foreach (CmfPlatformDefinition option in availablePlatforms)
        {
            availableForClient.Add(option.TableName);
            chipOptionsForClient.Add(new Dictionary<string, string>
            {
                { "label", option.Label },
                { "value", option.TableName }
            });
        }

        EnsureOverallChipOptionPresent(availablePlatforms, availableForClient, chipOptionsForClient);

        NormalizeOverallExcludedPlatforms(availableSet);
        HashSet<string> excludedSet = GetOverallExcludedPlatformsFromSession();
        List<string> excludedForClient = new List<string>();
        foreach (CmfPlatformDefinition option in availablePlatforms)
        {
            if (option != null && excludedSet.Contains(option.TableName))
            {
                excludedForClient.Add(option.TableName);
            }
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        AvailablePlatformTablesJson = serializer.Serialize(availableForClient);
        AvailablePlatformChipOptionsJson = serializer.Serialize(chipOptionsForClient);
        OverallExcludedPlatformsJson = serializer.Serialize(excludedForClient);
    }

    private void RefreshOverallExcludedPlatformsJsonForClient()
    {
        List<CmfPlatformDefinition> availablePlatforms = GetAvailableIndividualPlatforms();
        HashSet<string> availableSet = new HashSet<string>(
            availablePlatforms.Select(option => option.TableName),
            StringComparer.OrdinalIgnoreCase);

        NormalizeOverallExcludedPlatforms(availableSet);
        HashSet<string> excludedSet = GetOverallExcludedPlatformsFromSession();
        List<string> excludedForClient = new List<string>();
        foreach (CmfPlatformDefinition option in availablePlatforms)
        {
            if (option != null && excludedSet.Contains(option.TableName))
            {
                excludedForClient.Add(option.TableName);
            }
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        OverallExcludedPlatformsJson = serializer.Serialize(excludedForClient);
    }

    private HashSet<string> GetOverallExcludedPlatformsFromSession()
    {
        HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string raw = Session[OverallExcludedPlatformsSessionKey] as string;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return excluded;
        }

        string[] tokens = raw.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            string candidate = tokens[i] == null ? string.Empty : tokens[i].Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (!AllowedPlatformTables.Contains(candidate))
            {
                continue;
            }

            if (string.Equals(candidate, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            excluded.Add(candidate);
        }

        return excluded;
    }

    private void SetOverallExcludedPlatformsSession(IEnumerable<string> platforms)
    {
        if (platforms == null)
        {
            Session.Remove(OverallExcludedPlatformsSessionKey);
            return;
        }

        HashSet<string> normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string platform in platforms)
        {
            string candidate = platform == null ? string.Empty : platform.Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (!AllowedPlatformTables.Contains(candidate))
            {
                continue;
            }

            if (string.Equals(candidate, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            normalized.Add(candidate);
        }

        if (normalized.Count == 0)
        {
            Session.Remove(OverallExcludedPlatformsSessionKey);
            return;
        }

        Session[OverallExcludedPlatformsSessionKey] = string.Join(",", normalized.ToArray());
    }

    private void NormalizeOverallExcludedPlatforms(HashSet<string> availableSet)
    {
        HashSet<string> current = GetOverallExcludedPlatformsFromSession();
        if (current.Count == 0)
        {
            return;
        }

        HashSet<string> normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string platform in current)
        {
            if (availableSet == null || availableSet.Count == 0 || availableSet.Contains(platform))
            {
                normalized.Add(platform);
            }
        }

        if (availableSet != null && availableSet.Count > 0 && normalized.Count >= availableSet.Count)
        {
            normalized.Clear();
        }

        SetOverallExcludedPlatformsSession(normalized);
    }

    private void UpdateOverallExcludedPlatformsFromClient(string rawExcludedPlatforms)
    {
        List<CmfPlatformDefinition> availablePlatforms = GetAvailableIndividualPlatforms();
        HashSet<string> availableSet = new HashSet<string>(
            availablePlatforms.Select(option => option.TableName),
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> exclusions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(rawExcludedPlatforms))
        {
            string[] tokens = rawExcludedPlatforms.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                string candidate = tokens[i] == null ? string.Empty : tokens[i].Trim();
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (!AllowedPlatformTables.Contains(candidate))
                {
                    continue;
                }

                if (!availableSet.Contains(candidate))
                {
                    continue;
                }

                exclusions.Add(candidate);
            }
        }

        if (availableSet.Count > 0)
        {
            int includedCount = 0;
            foreach (string platform in availableSet)
            {
                if (!exclusions.Contains(platform))
                {
                    includedCount++;
                }
            }

            if (includedCount <= 0)
            {
                exclusions.Clear();
            }
        }

        SetOverallExcludedPlatformsSession(exclusions);
    }

    private List<string> GetOverallAggregateSourceTables()
    {
        HashSet<string> excludedSet = GetOverallExcludedPlatformsFromSession();
        List<string> included = new List<string>();

        for (int index = 0; index < DashboardAggregatePlatformTables.Length; index++)
        {
            string tableName = DashboardAggregatePlatformTables[index];
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            if (excludedSet.Contains(tableName))
            {
                continue;
            }

            included.Add(tableName);
        }

        if (included.Count > 0)
        {
            return included;
        }

        SetOverallExcludedPlatformsSession(null);

        for (int index = 0; index < DashboardAggregatePlatformTables.Length; index++)
        {
            string tableName = DashboardAggregatePlatformTables[index];
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                included.Add(tableName);
            }
        }

        return included;
    }

    private void EnsureOverallChipOptionPresent(
        List<CmfPlatformDefinition> availablePlatforms,
        List<string> availableForClient,
        List<Dictionary<string, string>> chipOptionsForClient)
    {
        bool hasAvailablePlatforms = availablePlatforms != null && availablePlatforms.Count > 0;
        if (!hasAvailablePlatforms)
        {
            return;
        }

        bool hasOverallInAvailable = availableForClient != null &&
            availableForClient.Any(value => string.Equals(value, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase));

        if (!hasOverallInAvailable)
        {
            availableForClient.Insert(0, OverallDashboardPlatformTable);
            System.Diagnostics.Trace.TraceWarning("CMF portal guard: injected missing Overall entry into available platform list.");
        }

        bool hasOverallChip = false;
        if (chipOptionsForClient != null)
        {
            foreach (Dictionary<string, string> option in chipOptionsForClient)
            {
                if (option == null)
                {
                    continue;
                }

                string value;
                if (option.TryGetValue("value", out value) &&
                    string.Equals(value, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
                {
                    hasOverallChip = true;
                    break;
                }
            }
        }

        if (!hasOverallChip)
        {
            chipOptionsForClient.Insert(0, new Dictionary<string, string>
            {
                { "label", "Overall" },
                { "value", OverallDashboardPlatformTable }
            });

            System.Diagnostics.Trace.TraceWarning("CMF portal guard: injected missing Overall chip option into platform chip list.");
        }
    }

    private static CmfDashboardDateFilter GetDashboardDateFilter()
    {
        var saved = HttpContext.Current.Session["DashboardDateFilter"] as CmfDashboardDateFilter;
        CmfDashboardDateFilter filter;
        string error;
        CmfDashboardDateFilter.TryCreate(saved == null ? "all" : saved.Mode,
            saved == null ? "cmf-request" : saved.Basis,
            saved != null && saved.Start.HasValue ? saved.Start.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty,
            saved != null && saved.EndExclusive.HasValue ? saved.EndExclusive.Value.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty,
            DateTime.Today, out filter, out error);
        return filter;
    }

    private static string FilterDashboardTableSource(string tableName)
    {
        var filter = GetDashboardDateFilter();
        return filter.Start.HasValue
            ? "(SELECT * FROM " + tableName + " WHERE " + filter.BuildPredicate(null) + ")"
            : tableName;
    }

    protected void DashboardDateFilter_Changed(object sender, EventArgs e)
    {
        dashboardCustomDates.Visible = ddlDashboardDateMode.SelectedValue == "custom";
        CmfDashboardDateFilter filter;
        string error;
        if (!CmfDashboardDateFilter.TryCreate(ddlDashboardDateMode.SelectedValue,
            ddlDashboardDateBasis.SelectedValue, txtDashboardDateStart.Text, txtDashboardDateEnd.Text,
            DateTime.Today, out filter, out error))
        {
            ShowWelcomeHome();
            lblDashboardDateError.Text = error;
            return;
        }

        Session["DashboardDateFilter"] = filter;
        lblDashboardDateError.Text = string.Empty;
        ShowWelcomeHome();
        SetActiveFocusedTab("home");
    }

    private string BuildDashboardComponentSourceSql(string platformSelection, bool applyDashboardDateFilter = false)
    {
        string resolvedSelection = ResolveDashboardPlatformSelection(platformSelection);
        if (!IsOverallDashboardPlatform(resolvedSelection))
        {
            return applyDashboardDateFilter ? FilterDashboardTableSource(resolvedSelection) + " AS dashboard_platform" : resolvedSelection;
        }

        List<string> unionParts = new List<string>();
        foreach (string tableName in GetOverallAggregateSourceTables())
        {
            unionParts.Add(@"SELECT
    cp_id,
    status,
    priority,
    customer_impact,
    title,
    implemented_date,
    date_cmf_decided,
    date_cmf_ask,
    submitted_date,
    must_fix_for,
    component_group,
    drivers,
    customer_owner,
    repro_on_rvp,
    idst,
    los,
    customer_company,
    customer_detail,
    cmf_request,
    sysdebug,
    days_active
FROM " + tableName + (applyDashboardDateFilter ? " WHERE " + GetDashboardDateFilter().BuildPredicate(null) : string.Empty));
        }

        return "(" + string.Join(" UNION ALL ", unionParts) + ") AS overall_platform";
    }

    private string ResolveIssuePendingPlatformSelection(string platform = null)
    {
        string resolvedSelection = platform
            ?? Session[IssuePendingPlatformSessionKey] as string
            ?? Session["selectedPlatform"] as string
            ?? ddlTables.SelectedValue
            ?? selectedPlatform
            ?? DefaultPlatformTable;

        if (IsOverallDashboardPlatform(resolvedSelection))
        {
            return OverallDashboardPlatformTable;
        }

        return ResolvePlatformTable(resolvedSelection);
    }

    private string BuildIssueDataCteSql(string platformSelection)
    {
        string resolvedSelection = ResolveIssuePendingPlatformSelection(platformSelection);
        List<string> sourceTables = IsOverallDashboardPlatform(resolvedSelection)
            ? GetOverallAggregateSourceTables()
            : new List<string> { resolvedSelection };

        List<string> unionParts = new List<string>();
        foreach (string tableName in sourceTables)
        {
            unionParts.Add(@"SELECT
    progress,
    cp_id,
    title,
    component,
    customer_owner,
    owners_name,
    repro_on_rvp,
    processor,
    cmf_status,
    status,
    date_cmf_ask,
    sysdebug,
    idst,
    los,
    drivers,
    must_fix_for,
    days_active,
    cmf_request,
    merge_id,
    promoted_id,
    customer_company,
    customer_detail,
    component_group,
    customer_affected,
    impact,
    closed_reason,
    fixed_in_version,
    reproducibility,
    '" + tableName + @"' AS source_platform
FROM " + tableName);
        }

        return string.Join(" UNION ALL ", unionParts);
    }

    private string BuildIssueDataSourceSql(string platformSelection, string alias = null)
    {
        string resolvedSelection = ResolveIssuePendingPlatformSelection(platformSelection);
        bool isOverall = IsOverallDashboardPlatform(resolvedSelection);
        string sourceSql = isOverall
            ? "(" + BuildIssueDataCteSql(resolvedSelection) + ")"
            : resolvedSelection;

        if (!string.IsNullOrWhiteSpace(alias))
        {
            sourceSql += " AS " + alias;
        }

        return sourceSql;
    }

    private string BuildPendingAskCteSql(SqlConnection connection, string platformSelection)
    {
        string resolvedSelection = ResolveIssuePendingPlatformSelection(platformSelection);
        List<string> sourceTables = IsOverallDashboardPlatform(resolvedSelection)
            ? GetOverallAggregateSourceTables()
            : new List<string> { resolvedSelection };

        List<string> unionParts = new List<string>();
        foreach (string tableName in sourceTables)
        {
            string basePlatform = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
            string pendingTable = basePlatform + "_CMF_ASK";
            if (connection != null && !SqlTableExists(connection, pendingTable))
            {
                continue;
            }

            unionParts.Add(@"SELECT
    cp_id,
    title,
    component,
    component_group,
    reproducibility,
    customer_owner,
    customer_detail,
    date_cmf_ask,
    submitted_date,
    cmf_request,
    why_cmf_change,
    impact,
    status,
    '" + tableName + @"' AS source_platform
FROM " + pendingTable);
        }

        return unionParts.Count == 0 ? null : string.Join(" UNION ALL ", unionParts);
    }

    private string BuildPendingAskSourceSql(SqlConnection connection, string platformSelection, string alias = null)
    {
        string pendingCteSql = BuildPendingAskCteSql(connection, platformSelection);
        if (string.IsNullOrWhiteSpace(pendingCteSql))
        {
            return null;
        }

        string sourceSql = "(" + pendingCteSql + ")";
        if (!string.IsNullOrWhiteSpace(alias))
        {
            sourceSql += " AS " + alias;
        }

        return sourceSql;
    }

    private static string NormalizePlatformForLookup(string platform)
    {
        if (string.Equals(platform, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return platform;
    }

    private static bool IsAllowedOrOverallPlatform(string platform)
    {
        return string.IsNullOrWhiteSpace(platform)
            || AllowedPlatformTables.Contains(platform)
            || string.Equals(platform, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveDashboardPlatformSelectionForWeb(string platform)
    {
        string resolvedSelection = platform;
        if (string.IsNullOrWhiteSpace(resolvedSelection) && HttpContext.Current != null && HttpContext.Current.Session != null)
        {
            resolvedSelection = HttpContext.Current.Session[DashboardPlatformSessionKey] as string;
            if (string.IsNullOrWhiteSpace(resolvedSelection))
            {
                resolvedSelection = HttpContext.Current.Session["selectedPlatform"] as string;
            }
        }

        if (string.IsNullOrWhiteSpace(resolvedSelection))
        {
            resolvedSelection = DefaultPlatformTable;
        }

        if (string.Equals(resolvedSelection, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
        {
            return OverallDashboardPlatformTable;
        }

        if (!AllowedPlatformTables.Contains(resolvedSelection))
        {
            return DefaultPlatformTable;
        }

        return resolvedSelection;
    }

    private static HashSet<string> GetOverallExcludedPlatformsFromSessionForWeb()
    {
        HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (HttpContext.Current == null || HttpContext.Current.Session == null)
        {
            return excluded;
        }

        string raw = HttpContext.Current.Session[OverallExcludedPlatformsSessionKey] as string;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return excluded;
        }

        string[] tokens = raw.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
        for (int index = 0; index < tokens.Length; index++)
        {
            string candidate = tokens[index] == null ? string.Empty : tokens[index].Trim();
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (!AllowedPlatformTables.Contains(candidate))
            {
                continue;
            }

            if (string.Equals(candidate, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            excluded.Add(candidate);
        }

        return excluded;
    }

    private static List<string> GetOverallAggregateSourceTablesForWeb()
    {
        HashSet<string> excludedSet = GetOverallExcludedPlatformsFromSessionForWeb();
        List<string> included = new List<string>();

        for (int index = 0; index < DashboardAggregatePlatformTables.Length; index++)
        {
            string tableName = DashboardAggregatePlatformTables[index];
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            if (excludedSet.Contains(tableName))
            {
                continue;
            }

            included.Add(tableName);
        }

        if (included.Count > 0)
        {
            return included;
        }

        for (int index = 0; index < DashboardAggregatePlatformTables.Length; index++)
        {
            string tableName = DashboardAggregatePlatformTables[index];
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                included.Add(tableName);
            }
        }

        return included;
    }

    private static List<string> GetSummaryIssueSourceTablesForWeb(string dashboardPlatform)
    {
        if (string.Equals(dashboardPlatform, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase))
        {
            return GetOverallAggregateSourceTablesForWeb();
        }

        return new List<string> { dashboardPlatform };
    }

    private static string BuildSummaryIssueComponentSourceSql(string dashboardPlatform, bool applyDashboardDateFilter = true)
    {
        List<string> sourceTables = GetSummaryIssueSourceTablesForWeb(dashboardPlatform);
        List<string> unionParts = new List<string>();

        for (int index = 0; index < sourceTables.Count; index++)
        {
            string tableName = sourceTables[index];
            unionParts.Add(@"SELECT
    cp_id,
    status,
    cmf_request,
    sysdebug,
    closed_reason,
    date_cmf_ask,
    submitted_date,
    implemented_date,
            must_fix_for,
    component_group,
    drivers,
            repro_on_rvp,
            idst,
            los,
            customer_company,
    customer_detail,
    component,
    customer_owner,
    '" + tableName + @"' AS source_platform
FROM " + tableName + (applyDashboardDateFilter ? " WHERE " + GetDashboardDateFilter().BuildPredicate(null) : string.Empty));
        }

        return string.Join(" UNION ALL ", unionParts);
    }

    private static string BuildSummaryDesignMetadataSourceSql(string dashboardPlatform)
    {
        List<string> sourceTables = GetSummaryIssueSourceTablesForWeb(dashboardPlatform);
        List<string> unionParts = new List<string>();

        for (int index = 0; index < sourceTables.Count; index++)
        {
            string tableName = sourceTables[index];
            string designTable = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty) + "_DESIGN_TABLE";

                unionParts.Add(@"SELECT
            LTRIM(RTRIM(customer_detail)) AS customer_detail,
            ISNULL(MAX(NULLIF(LTRIM(RTRIM(sw_image_freeze)), '')), 'N/A') AS sw_image_freeze,
            '" + tableName + @"' AS source_platform
        FROM " + designTable + @"
        GROUP BY LTRIM(RTRIM(customer_detail))");
        }

        return unionParts.Count == 0 ? string.Empty : string.Join(" UNION ALL ", unionParts);
    }

    private static string BuildSummaryIssuePendingSourceSql(SqlConnection connection, string dashboardPlatform)
    {
        if (connection == null)
        {
            return null;
        }

        List<string> sourceTables = GetSummaryIssueSourceTablesForWeb(dashboardPlatform);
        List<string> unionParts = new List<string>();

        for (int index = 0; index < sourceTables.Count; index++)
        {
            string tableName = sourceTables[index];
            string pendingTable = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty) + "_CMF_ASK";
            if (!SqlTableExists(connection, pendingTable))
            {
                continue;
            }

            unionParts.Add(@"SELECT
    cp_id,
    status,
    component_group,
    customer_detail,
    component,
    customer_owner,
    '" + tableName + @"' AS source_platform
FROM " + pendingTable + " WHERE " + GetDashboardDateFilter().BuildPredicate(null));
        }

        return unionParts.Count == 0 ? null : string.Join(" UNION ALL ", unionParts);
    }

    private static string NormalizeSummaryMetricKey(string metricKey)
    {
        string normalized = (metricKey ?? string.Empty).Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "total":
            case "total cmfs":
            case "total_cmfs":
                return "total";
            case "closed":
                return "closed";
            case "implemented":
                return "implemented";
            case "open":
                return "open";
            case "pending":
                return "pending";
            default:
                return string.Empty;
        }
    }

    private static string NormalizeTrendSeriesMetricKey(string seriesName)
    {
        string normalized = (seriesName ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized == "all")
        {
            return "all";
        }

        if (normalized == "open")
        {
            return "open";
        }

        if (normalized == "closed")
        {
            return "closed";
        }

        return string.Empty;
    }

    private static string NormalizeSummaryGroupBy(string groupBy)
    {
        string normalized = (groupBy ?? string.Empty).Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "ingredient":
            case "platform":
            case "milestone":
            case "design":
            case "component":
            case "owner":
            case "idst":
                return normalized;
            default:
                return "ingredient";
        }
    }

    private static string BuildSummaryGroupExpression(string groupBy)
    {
        switch (groupBy)
        {
            case "platform":
                return @"CASE
    WHEN src.source_platform IS NULL OR LTRIM(RTRIM(src.source_platform)) = '' THEN 'Unknown Platform'
    ELSE LTRIM(RTRIM(src.source_platform))
END";
            case "milestone":
                return @"CASE
    WHEN src.drivers IS NULL OR LTRIM(RTRIM(src.drivers)) = '' THEN 'Unassigned'
    ELSE LTRIM(RTRIM(src.drivers))
END";
            case "design":
                return @"CASE
    WHEN src.customer_detail IS NULL OR LTRIM(RTRIM(src.customer_detail)) = '' THEN 'unassigned'
    ELSE LTRIM(RTRIM(src.customer_detail))
END";
            case "component":
                return @"CASE
    WHEN src.component IS NULL OR LTRIM(RTRIM(src.component)) = '' THEN 'Unassigned'
    ELSE LTRIM(RTRIM(src.component))
END";
            case "owner":
                return @"CASE
    WHEN src.customer_owner IS NULL OR LTRIM(RTRIM(src.customer_owner)) = '' THEN 'Unassigned'
    ELSE LTRIM(RTRIM(src.customer_owner))
END";
            case "idst":
                return @"CASE
    WHEN src.idst IS NULL OR LTRIM(RTRIM(src.idst)) = '' THEN 'Unassigned'
    ELSE LTRIM(RTRIM(src.idst))
END";
            default:
                return @"CASE
    WHEN src.component_group IS NULL
      OR LTRIM(RTRIM(src.component_group)) = ''
      OR LOWER(LTRIM(RTRIM(src.component_group))) = 'no idst assigned'
    THEN 'Unassigned'
    ELSE LTRIM(RTRIM(src.component_group))
END";
        }
    }

    private static List<string> BuildTrendGroupByOptions(bool isOverallScope)
    {
        List<string> options = new List<string>
        {
            "ingredient"
        };

        if (isOverallScope)
        {
            options.Add("platform");
        }

        options.Add("milestone");
        options.Add("design");
        options.Add("component");
        options.Add("owner");
        options.Add("idst");
        return options;
    }

    private static string NormalizeTrendGroupBy(string groupBy, bool isOverallScope)
    {
        string normalized = (groupBy ?? string.Empty).Trim().ToLowerInvariant();
        List<string> allowed = BuildTrendGroupByOptions(isOverallScope);
        return allowed.Contains(normalized, StringComparer.OrdinalIgnoreCase) ? normalized : "ingredient";
    }

    private static string GetDashboardDriverFilterValueForWeb()
    {
        if (HttpContext.Current == null || HttpContext.Current.Session == null)
        {
            return string.Empty;
        }

        string filterValue = HttpContext.Current.Session["filterValue"] as string;
        return string.IsNullOrWhiteSpace(filterValue) ? string.Empty : filterValue.Trim();
    }

    private static Dictionary<string, string> GetDashboardColumnFiltersForWeb()
    {
        Dictionary<string, string> filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "owner", "All" },
            { "rvpRepro", "All" },
            { "idst", "All" },
            { "los", "All" },
            { "milestone", "All" },
            { "Company", "All" },
            { "Detail", "All" },
            { "Component", "All" }
        };

        if (HttpContext.Current == null || HttpContext.Current.Session == null)
        {
            return filters;
        }

        string owner = HttpContext.Current.Session["ownerFilter"] as string;
        string rvpRepro = HttpContext.Current.Session["rvpReproFilter"] as string;
        string idst = HttpContext.Current.Session["idstFilter"] as string;
        string los = HttpContext.Current.Session["losFilter"] as string;
        string milestone = HttpContext.Current.Session["milestoneFilter"] as string;
        string company = HttpContext.Current.Session["companyFilter"] as string;
        string detail = HttpContext.Current.Session["detailFilter"] as string;
        string component = HttpContext.Current.Session["componentFilter"] as string;

        filters["owner"] = string.IsNullOrWhiteSpace(owner) ? "All" : owner;
        filters["rvpRepro"] = string.IsNullOrWhiteSpace(rvpRepro) ? "All" : rvpRepro;
        filters["idst"] = string.IsNullOrWhiteSpace(idst) ? "All" : idst;
        filters["los"] = string.IsNullOrWhiteSpace(los) ? "All" : los;
        filters["milestone"] = string.IsNullOrWhiteSpace(milestone) ? "All" : milestone;
        filters["Company"] = string.IsNullOrWhiteSpace(company) ? "All" : company;
        filters["Detail"] = string.IsNullOrWhiteSpace(detail) ? "All" : detail;
        filters["Component"] = string.IsNullOrWhiteSpace(component) ? "All" : component;
        return filters;
    }

    private static string BuildOverallHeatmapIngredientExpression()
    {
        return @"CASE
    WHEN ISNULL(LTRIM(RTRIM(src.component_group)), '') = '' THEN 'Unassigned'
    ELSE LTRIM(RTRIM(src.component_group))
END";
    }

    private static string ResolvePlatformTableFromHeatmapLabel(string platformLabel)
    {
        string normalized = (platformLabel ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (CmfPlatformCatalog.IsAllowedPlatformTable(normalized))
        {
            return normalized;
        }

        CmfPlatformDefinition[] orderedPlatforms = CmfPlatformCatalog.GetOrderedPlatforms();
        for (int index = 0; index < orderedPlatforms.Length; index++)
        {
            CmfPlatformDefinition platform = orderedPlatforms[index];
            if (platform == null || string.IsNullOrWhiteSpace(platform.TableName))
            {
                continue;
            }

            if (string.Equals(platform.Label, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return platform.TableName;
            }

            string fallbackLabel = CmfPlatformCatalog.ResolvePlatformCode(platform.TableName).Replace("_", "-");
            if (string.Equals(fallbackLabel, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return platform.TableName;
            }
        }

        return string.Empty;
    }

    private static int CompareIssueIds(string left, string right)
    {
        long leftNumeric;
        long rightNumeric;
        bool leftOk = long.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out leftNumeric);
        bool rightOk = long.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out rightNumeric);

        if (leftOk && rightOk)
        {
            return leftNumeric.CompareTo(rightNumeric);
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    [WebMethod(EnableSession = true)]
    public static SummaryIssueLinksResponse GetSummaryCardIssueLinks(string metricKey, string groupBy, string dashboardPlatform)
    {
        SummaryIssueLinksResponse response = new SummaryIssueLinksResponse
        {
            Success = false,
            Message = string.Empty,
            MetricKey = NormalizeSummaryMetricKey(metricKey),
            GroupBy = NormalizeSummaryGroupBy(groupBy),
            AvailableGroupBy = new List<string>(),
            Groups = new List<SummaryIssueLinksGroup>()
        };

        if (string.IsNullOrWhiteSpace(response.MetricKey))
        {
            response.Message = "Unsupported summary metric.";
            return response;
        }

        string resolvedPlatform = ResolveDashboardPlatformSelectionForWeb(dashboardPlatform);
        response.IsOverallScope = string.Equals(resolvedPlatform, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase);

        response.AvailableGroupBy.Add("ingredient");
        if (response.IsOverallScope)
        {
            response.AvailableGroupBy.Add("platform");
        }
        response.AvailableGroupBy.Add("milestone");
        response.AvailableGroupBy.Add("design");
        response.AvailableGroupBy.Add("component");
        response.AvailableGroupBy.Add("owner");
        response.AvailableGroupBy.Add("idst");

        if (!response.AvailableGroupBy.Contains(response.GroupBy, StringComparer.OrdinalIgnoreCase))
        {
            response.GroupBy = "ingredient";
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
            {
                connection.Open();

                string sourceSql;
                string wherePredicate;

                if (string.Equals(response.MetricKey, "pending", StringComparison.OrdinalIgnoreCase))
                {
                    string pendingSource = BuildSummaryIssuePendingSourceSql(connection, resolvedPlatform);
                    if (string.IsNullOrWhiteSpace(pendingSource))
                    {
                        response.Success = true;
                        response.Message = "No pending CMF records were found for the selected scope.";
                        response.TotalIssues = 0;
                        return response;
                    }

                    string componentSource = BuildSummaryIssueComponentSourceSql(resolvedPlatform, false);
                    sourceSql = @"(
SELECT
    CAST(p.cp_id AS VARCHAR(64)) AS cp_id,
    p.status,
    p.component_group,
    p.customer_detail,
    p.component,
    p.customer_owner,
    p.source_platform,
    COALESCE(NULLIF(LTRIM(RTRIM(c.drivers)), ''), 'Unassigned') AS drivers,
    COALESCE(NULLIF(LTRIM(RTRIM(c.idst)), ''), 'Unassigned') AS idst,
    '' AS cmf_request,
    '' AS sysdebug
FROM (" + pendingSource + @") AS p
LEFT JOIN (" + componentSource + @") AS c
  ON CAST(p.cp_id AS VARCHAR(64)) = CAST(c.cp_id AS VARCHAR(64))
 AND p.source_platform = c.source_platform
) AS src";

                    wherePredicate = @"LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) NOT IN ('complete', 'rejected')";
                }
                else
                {
                    sourceSql = "(" + BuildSummaryIssueComponentSourceSql(resolvedPlatform) + ") AS src";
                    StringBuilder whereBuilder = new StringBuilder();
                    whereBuilder.Append("LOWER(LTRIM(RTRIM(ISNULL(src.sysdebug, '')))) LIKE '%customer_must_fix%'");
                    whereBuilder.Append(" AND LOWER(LTRIM(RTRIM(ISNULL(src.cmf_request, '')))) IN ('cmf_ok', 'cmf_duplicate')");

                    if (string.Equals(response.MetricKey, "closed", StringComparison.OrdinalIgnoreCase))
                    {
                        whereBuilder.Append(" AND LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) IN ('complete', 'rejected')");
                    }
                    else if (string.Equals(response.MetricKey, "implemented", StringComparison.OrdinalIgnoreCase))
                    {
                        whereBuilder.Append(" AND LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) = 'implemented'");
                    }
                    else if (string.Equals(response.MetricKey, "open", StringComparison.OrdinalIgnoreCase))
                    {
                        whereBuilder.Append(" AND LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) NOT IN ('complete', 'rejected', 'implemented')");
                    }

                    wherePredicate = whereBuilder.ToString();
                }

                string groupExpression = BuildSummaryGroupExpression(response.GroupBy);
                string sql = @"
SELECT DISTINCT
    CAST(src.cp_id AS VARCHAR(64)) AS cp_id,
    " + groupExpression + @" AS group_name
FROM " + sourceSql + @"
WHERE " + wherePredicate + @"
  AND src.cp_id IS NOT NULL
  AND LTRIM(RTRIM(CAST(src.cp_id AS VARCHAR(64)))) <> ''
ORDER BY group_name, cp_id";

                Dictionary<string, HashSet<string>> idsByGroup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                using (SqlCommand command = new SqlCommand(sql, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cpId = ReadString(reader, 0).Trim();
                        if (string.IsNullOrWhiteSpace(cpId))
                        {
                            continue;
                        }

                        string groupName = ReadString(reader, 1).Trim();
                        if (string.IsNullOrWhiteSpace(groupName))
                        {
                            groupName = "Unassigned";
                        }

                        HashSet<string> ids;
                        if (!idsByGroup.TryGetValue(groupName, out ids))
                        {
                            ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            idsByGroup[groupName] = ids;
                        }

                        ids.Add(cpId);
                    }
                }

                List<string> orderedGroups = idsByGroup.Keys
                    .OrderBy(group => group, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int totalIssues = 0;
                for (int index = 0; index < orderedGroups.Count; index++)
                {
                    string groupName = orderedGroups[index];
                    List<string> orderedIds = idsByGroup[groupName]
                        .OrderBy(id => id, Comparer<string>.Create(CompareIssueIds))
                        .ToList();

                    totalIssues += orderedIds.Count;

                    response.Groups.Add(new SummaryIssueLinksGroup
                    {
                        GroupName = groupName,
                        HsdIds = orderedIds
                    });
                }

                response.TotalIssues = totalIssues;
                response.Success = true;
                response.Message = totalIssues > 0
                    ? "Loaded individual CMF HSD links."
                    : "No CMF records matched this summary card and grouping.";
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "Unable to load CMF issue links: " + ex.Message;
        }

        return response;
    }

    [WebMethod(EnableSession = true)]
    public static SummaryIssueLinksResponse GetOverallHeatmapIssueLinks(string ingredient, string platformLabel, string groupBy)
    {
        SummaryIssueLinksResponse response = new SummaryIssueLinksResponse
        {
            Success = false,
            Message = string.Empty,
            MetricKey = "open",
            GroupBy = NormalizeSummaryGroupBy(groupBy),
            IsOverallScope = true,
            AvailableGroupBy = new List<string>(),
            Groups = new List<SummaryIssueLinksGroup>(),
            IdstNames = new List<string>()
        };

        response.AvailableGroupBy.Add("milestone");
        response.AvailableGroupBy.Add("design");
        response.AvailableGroupBy.Add("component");
        response.AvailableGroupBy.Add("owner");
        response.AvailableGroupBy.Add("idst");

        if (!response.AvailableGroupBy.Contains(response.GroupBy, StringComparer.OrdinalIgnoreCase))
        {
            response.GroupBy = "milestone";
        }

        string normalizedIngredient = string.IsNullOrWhiteSpace(ingredient)
            ? "Unassigned"
            : ingredient.Trim();
        string sourcePlatform = ResolvePlatformTableFromHeatmapLabel(platformLabel);

        if (string.IsNullOrWhiteSpace(sourcePlatform))
        {
            response.Message = "Unsupported platform selection for heatmap links.";
            return response;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
            {
                connection.Open();

                string sourceSql = "(" + BuildSummaryIssueComponentSourceSql(OverallDashboardPlatformTable) + ") AS src";
                string wherePredicate = @"LOWER(LTRIM(RTRIM(ISNULL(src.sysdebug, '')))) LIKE '%customer_must_fix%'
  AND LOWER(LTRIM(RTRIM(ISNULL(src.cmf_request, '')))) = 'cmf_ok'
  AND LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) = 'open'
  AND ISNULL(LTRIM(RTRIM(src.drivers)), '') <> ''
  AND " + BuildOverallHeatmapIngredientExpression() + @" = @ingredient
  AND src.source_platform = @source_platform";

                string groupExpression = BuildSummaryGroupExpression(response.GroupBy);
                                string sql = @"
SELECT DISTINCT
    CAST(src.cp_id AS VARCHAR(64)) AS cp_id,
        " + groupExpression + @" AS group_name,
        CASE
                WHEN src.idst IS NULL OR LTRIM(RTRIM(src.idst)) = '' THEN 'Unassigned'
                ELSE LTRIM(RTRIM(src.idst))
        END AS idst_name
FROM " + sourceSql + @"
WHERE " + wherePredicate + @"
  AND src.cp_id IS NOT NULL
  AND LTRIM(RTRIM(CAST(src.cp_id AS VARCHAR(64)))) <> ''
ORDER BY group_name, CAST(src.cp_id AS VARCHAR(64))";

                Dictionary<string, HashSet<string>> idsByGroup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                                HashSet<string> idstNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ingredient", normalizedIngredient);
                    command.Parameters.AddWithValue("@source_platform", sourcePlatform);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cpId = ReadString(reader, 0).Trim();
                            if (string.IsNullOrWhiteSpace(cpId))
                            {
                                continue;
                            }

                            string groupName = ReadString(reader, 1).Trim();
                            if (string.IsNullOrWhiteSpace(groupName))
                            {
                                groupName = "Unassigned";
                            }

                            string idstName = ReadString(reader, 2).Trim();
                            if (string.IsNullOrWhiteSpace(idstName))
                            {
                                idstName = "Unassigned";
                            }
                            idstNames.Add(idstName);

                            HashSet<string> ids;
                            if (!idsByGroup.TryGetValue(groupName, out ids))
                            {
                                ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                idsByGroup[groupName] = ids;
                            }

                            ids.Add(cpId);
                        }
                    }
                }

                List<string> orderedGroups = idsByGroup.Keys
                    .OrderBy(group => group, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int totalIssues = 0;
                for (int index = 0; index < orderedGroups.Count; index++)
                {
                    string groupName = orderedGroups[index];
                    List<string> orderedIds = idsByGroup[groupName]
                        .OrderBy(id => id, Comparer<string>.Create(CompareIssueIds))
                        .ToList();

                    totalIssues += orderedIds.Count;

                    response.Groups.Add(new SummaryIssueLinksGroup
                    {
                        GroupName = groupName,
                        HsdIds = orderedIds
                    });
                }

                response.TotalIssues = totalIssues;
                response.IdstNames = idstNames
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                response.Success = true;
                response.Message = totalIssues > 0
                    ? "Loaded individual CMF HSD links."
                    : "No CMF records matched this heatmap cell and grouping.";
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "Unable to load heatmap issue links: " + ex.Message;
        }

        return response;
    }

    [WebMethod(EnableSession = true)]
    public static SummaryIssueLinksResponse GetTrendWeekIssueLinks(string seriesName, string weekLabel, int weekOffset, string dashboardPlatform, string groupBy, bool includeOpen, bool includeClosed)
    {
        string resolvedPlatform = ResolveDashboardPlatformSelectionForWeb(dashboardPlatform);
        bool isOverallScope = string.Equals(resolvedPlatform, OverallDashboardPlatformTable, StringComparison.OrdinalIgnoreCase);

        SummaryIssueLinksResponse response = new SummaryIssueLinksResponse
        {
            Success = false,
            Message = string.Empty,
            MetricKey = NormalizeTrendSeriesMetricKey(seriesName),
            GroupBy = NormalizeTrendGroupBy(groupBy, isOverallScope),
            IsOverallScope = isOverallScope,
            AvailableGroupBy = new List<string>(),
            Groups = new List<SummaryIssueLinksGroup>()
        };

        response.AvailableGroupBy = BuildTrendGroupByOptions(response.IsOverallScope);

        if (string.IsNullOrWhiteSpace(response.MetricKey))
        {
            response.Message = "Unsupported trend series selection.";
            return response;
        }

        if (weekOffset < 0 || weekOffset > 520)
        {
            response.Message = "Unsupported work week selection.";
            return response;
        }

        if (!response.AvailableGroupBy.Contains(response.GroupBy, StringComparer.OrdinalIgnoreCase))
        {
            response.GroupBy = "ingredient";
        }

        string normalizedWeekLabel = string.IsNullOrWhiteSpace(weekLabel) ? "Selected Week" : weekLabel.Trim();
        bool metricAllowsOpen = string.Equals(response.MetricKey, "all", StringComparison.OrdinalIgnoreCase)
            || string.Equals(response.MetricKey, "open", StringComparison.OrdinalIgnoreCase);
        bool metricAllowsClosed = string.Equals(response.MetricKey, "all", StringComparison.OrdinalIgnoreCase)
            || string.Equals(response.MetricKey, "closed", StringComparison.OrdinalIgnoreCase);

        bool allowOpen = includeOpen && metricAllowsOpen;
        bool allowClosed = includeClosed && metricAllowsClosed;

        if (!allowOpen && !allowClosed)
        {
            response.Success = true;
            response.TotalIssues = 0;
            response.Message = "No CMF records matched the selected status filters for this work week point.";
            return response;
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
            {
                connection.Open();

                string sourceSql = "(" + BuildSummaryIssueComponentSourceSql(resolvedPlatform) + ") AS src";
                string statusPredicate;
                string weekPredicate;
                string groupExpression = BuildSummaryGroupExpression(response.GroupBy);

                StringBuilder baseWhere = new StringBuilder();
                baseWhere.Append("LOWER(LTRIM(RTRIM(ISNULL(src.cmf_request, '')))) = 'cmf_ok'");
                baseWhere.Append(" AND LOWER(LTRIM(RTRIM(ISNULL(src.sysdebug, '')))) LIKE '%customer_must_fix%'");
                baseWhere.Append(" AND LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) <> 'rejected'");

                string driverFilterValue = GetDashboardDriverFilterValueForWeb();
                Dictionary<string, string> columnFilters = GetDashboardColumnFiltersForWeb();
                AppendDashboardDriverFilter(baseWhere, driverFilterValue);
                AppendDashboardColumnFilters(baseWhere, columnFilters);

                if (allowOpen && !allowClosed)
                {
                    statusPredicate = "LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) = 'open'";
                    weekPredicate = "TRY_CAST(src.date_cmf_ask AS DATE) IS NOT NULL AND DATEDIFF(WEEK, TRY_CAST(src.date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) = @week_offset";
                }
                else if (!allowOpen && allowClosed)
                {
                    statusPredicate = "LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) IN ('complete', 'rejected')";
                    weekPredicate = "TRY_CAST(src.implemented_date AS DATE) IS NOT NULL AND DATEDIFF(WEEK, TRY_CAST(src.implemented_date AS DATE), CAST(GETDATE() AS DATE)) = @week_offset";
                }
                else
                {
                    statusPredicate = @"(
        LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) = 'open'
        OR LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) IN ('complete', 'rejected')
    )";

                    weekPredicate = @"(
        (
            LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) = 'open'
            AND TRY_CAST(src.date_cmf_ask AS DATE) IS NOT NULL
            AND DATEDIFF(WEEK, TRY_CAST(src.date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) = @week_offset
        )
        OR
        (
            LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) IN ('complete', 'rejected')
            AND TRY_CAST(src.implemented_date AS DATE) IS NOT NULL
            AND DATEDIFF(WEEK, TRY_CAST(src.implemented_date AS DATE), CAST(GETDATE() AS DATE)) = @week_offset
        )
    )";
                }

                string sql = @"
SELECT DISTINCT
    CAST(src.cp_id AS VARCHAR(64)) AS cp_id,
    " + groupExpression + @" AS group_name
FROM " + sourceSql + @"
WHERE " + baseWhere + @"
    AND " + statusPredicate + @"
  AND " + weekPredicate + @"
  AND src.cp_id IS NOT NULL
  AND LTRIM(RTRIM(CAST(src.cp_id AS VARCHAR(64)))) <> ''
ORDER BY group_name, CAST(src.cp_id AS VARCHAR(64))";

                Dictionary<string, HashSet<string>> idsByGroup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@week_offset", weekOffset);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cpId = ReadString(reader, 0).Trim();
                            if (string.IsNullOrWhiteSpace(cpId))
                            {
                                continue;
                            }

                            string groupName = ReadString(reader, 1).Trim();
                            if (string.IsNullOrWhiteSpace(groupName))
                            {
                                groupName = "Unassigned";
                            }

                            HashSet<string> ids;
                            if (!idsByGroup.TryGetValue(groupName, out ids))
                            {
                                ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                idsByGroup[groupName] = ids;
                            }

                            ids.Add(cpId);
                        }
                    }
                }

                List<string> orderedGroups = idsByGroup.Keys
                    .OrderBy(group => group, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                int totalIssues = 0;
                for (int index = 0; index < orderedGroups.Count; index++)
                {
                    string groupName = orderedGroups[index];
                    List<string> orderedIds = idsByGroup[groupName]
                        .OrderBy(id => id, Comparer<string>.Create(CompareIssueIds))
                        .ToList();

                    totalIssues += orderedIds.Count;

                    response.Groups.Add(new SummaryIssueLinksGroup
                    {
                        GroupName = groupName,
                        HsdIds = orderedIds
                    });
                }

                response.TotalIssues = totalIssues;
                response.Success = true;
                response.Message = totalIssues > 0
                    ? "Loaded individual CMF HSD links."
                    : "No CMF records matched this work week point.";
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "Unable to load trend work-week links: " + ex.Message;
        }

        return response;
    }

    private string BuildOverallPendingSourceSql(SqlConnection connection)
    {
        if (connection == null)
        {
            return null;
        }

        List<string> unionParts = new List<string>();
        foreach (string tableName in GetOverallAggregateSourceTables())
        {
            string basePlatform = tableName.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
            string pendingTable = basePlatform + "_CMF_ASK";
            if (SqlTableExists(connection, pendingTable))
            {
                unionParts.Add("SELECT cp_id, status, component_group, date_cmf_ask, '" + tableName + "' AS source_platform FROM " + pendingTable + " WHERE " + GetDashboardDateFilter().BuildPredicate(null));
            }
        }

        if (unionParts.Count == 0)
        {
            return null;
        }

        return string.Join(" UNION ALL ", unionParts);
    }

    private DataTable ExecuteDataTable(string query, Action<SqlCommand> configureCommand = null)
    {
        using (SqlConnection connection = new SqlConnection(ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
        {
            if (configureCommand != null)
            {
                configureCommand(command);
            }

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }
    }

    private List<string> GetDistinctDrivers(string platformTable, params string[] statuses)
    {
        platformTable = ResolveDashboardPlatformSelection(platformTable);
        string sourceSql = BuildDashboardComponentSourceSql(platformTable);

        StringBuilder query = new StringBuilder();
        query.Append("SELECT DISTINCT [drivers] FROM ");
        query.Append(sourceSql);
        query.Append(" WHERE cmf_request NOT IN ('cmf_reject') AND sysdebug LIKE ('%customer_must_fix%')");

        if (statuses != null && statuses.Length > 0)
        {
            query.Append(" AND status IN (");
            for (int index = 0; index < statuses.Length; index++)
            {
                if (index > 0)
                {
                    query.Append(", ");
                }

                query.Append("@status");
                query.Append(index);
            }
            query.Append(")");
        }

        DataTable driversTable = ExecuteDataTable(
            query.ToString(),
            command =>
            {
                if (statuses != null)
                {
                    for (int index = 0; index < statuses.Length; index++)
                    {
                        command.Parameters.AddWithValue("@status" + index, statuses[index]);
                    }
                }
            });

        return driversTable
            .AsEnumerable()
            .Select(row => row["drivers"] == DBNull.Value ? string.Empty : row["drivers"].ToString().Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private List<string> GetSelectedDrivers()
    {
        string filterValue = Session["filterValue"] as string;
        if (string.IsNullOrWhiteSpace(filterValue) || filterValue == "AllDrivers")
        {
            return new List<string>();
        }

        return filterValue
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AppendDriversInClause(StringBuilder sql, SqlCommand command, IEnumerable<string> selectedDrivers, string parameterPrefix)
    {
        List<string> drivers = selectedDrivers
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        if (drivers.Count == 0)
        {
            return;
        }

        List<string> parameterNames = new List<string>();
        for (int index = 0; index < drivers.Count; index++)
        {
            string parameterName = "@" + parameterPrefix + index;
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, drivers[index]);
        }

        sql.Append(" AND drivers IN (");
        sql.Append(string.Join(", ", parameterNames));
        sql.Append(")");
    }

    private static void ParseTriggeredValue(string input, out string rawValue, out string trigger)
    {
        rawValue = string.Empty;
        trigger = string.Empty;

        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        int lastUnderscoreIndex = input.LastIndexOf('_');
        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < input.Length - 1)
        {
            rawValue = input.Substring(0, lastUnderscoreIndex);
            trigger = input.Substring(lastUnderscoreIndex + 1);
        }

        if (rawValue == "unassigned" || rawValue == "")
        {
            rawValue = string.Empty;
        }
    }

    private const string DesignSummaryAllToken = "__all_designs__";
    private const string IngredientSummaryAllToken = "__all_ingredients__";

    private static string NormalizeLegacyTrigger(string trigger)
    {
        string normalized = (trigger ?? string.Empty).Trim();
        if (normalized.StartsWith("_", StringComparison.Ordinal))
        {
            normalized = normalized.Substring(1);
        }

        return normalized.ToLowerInvariant();
    }

    private static string MapQuickLinkMetricToLegacyTrigger(string metricKey)
    {
        string normalizedMetric = NormalizeSummaryMetricKey(metricKey);
        switch (normalizedMetric)
        {
            case "pending":
                return "trg2";
            case "open":
                return "trg4";
            case "total":
            default:
                return "trg1";
        }
    }

    private static void ParseDesignSummaryModalArgument(string input, out string rawDesign, out string trigger, out bool includeAllDesigns)
    {
        rawDesign = string.Empty;
        trigger = string.Empty;
        includeAllDesigns = false;

        string token = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        string[] parts = token.Split(new[] { "||" }, StringSplitOptions.None);
        if (parts.Length >= 2)
        {
            rawDesign = (parts[0] ?? string.Empty).Trim();
            trigger = MapQuickLinkMetricToLegacyTrigger(parts[1]);
            includeAllDesigns = string.Equals(rawDesign, DesignSummaryAllToken, StringComparison.OrdinalIgnoreCase);
            if (string.Equals(rawDesign, "unassigned", StringComparison.OrdinalIgnoreCase))
            {
                rawDesign = string.Empty;
            }

            return;
        }

        ParseTriggeredValue(token, out rawDesign, out trigger);
        trigger = NormalizeLegacyTrigger(trigger);
        includeAllDesigns = string.Equals(rawDesign, DesignSummaryAllToken, StringComparison.OrdinalIgnoreCase);
    }

    private static void ParseIngredientSummaryModalArgument(string input, out string rawIngredient, out string trigger, out bool includeAllIngredients)
    {
        rawIngredient = string.Empty;
        trigger = string.Empty;
        includeAllIngredients = false;

        string token = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        string[] parts = token.Split(new[] { "||" }, StringSplitOptions.None);
        if (parts.Length >= 2)
        {
            rawIngredient = (parts[0] ?? string.Empty).Trim();
            trigger = MapQuickLinkMetricToLegacyTrigger(parts[1]);
            includeAllIngredients = string.Equals(rawIngredient, IngredientSummaryAllToken, StringComparison.OrdinalIgnoreCase);
            if (string.Equals(rawIngredient, "unassigned", StringComparison.OrdinalIgnoreCase))
            {
                rawIngredient = string.Empty;
            }

            return;
        }

        ParseTriggeredValue(token, out rawIngredient, out trigger);
        trigger = NormalizeLegacyTrigger(trigger);
        includeAllIngredients = string.Equals(rawIngredient, IngredientSummaryAllToken, StringComparison.OrdinalIgnoreCase);
    }

    private DataTable GetDesignSummaryModalData(string design)
    {
        string rawDesign;
        string trigger;
        bool includeAllDesigns;
        ParseDesignSummaryModalArgument(design, out rawDesign, out trigger, out includeAllDesigns);

        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "issue_src");
        string query;
        string designFilterClause = includeAllDesigns ? string.Empty : " AND customer_detail = @detail";

        switch (trigger)
        {
            case "trg1":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE CMF_REQUEST in ('cmf_ok','cmf_duplicate')" + designFilterClause;
                break;
            case "trg2":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE CMF_REQUEST in ('cmf_ask','cmf_incomplete') AND status not in ('complete','rejected')" + designFilterClause;
                break;
            case "trg4":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected')" + designFilterClause;
                break;
            case "trg5":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))" + designFilterClause;
                break;
            case "trg6":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))" + designFilterClause;
                break;
            case "trg7":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))" + designFilterClause;
                break;
            case "trg8":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))" + designFilterClause;
                break;
            default:
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE CMF_REQUEST in ('cmf_reject')" + designFilterClause;
                break;
        }

        return ExecuteDataTable(
            query,
            command =>
            {
                if (!includeAllDesigns)
                {
                    command.Parameters.AddWithValue("@detail", rawDesign);
                }
            });
    }

    private DataTable GetIngredientSummaryModalData(string ingred)
    {
        string rawIngredient;
        string trigger;
        bool includeAllIngredients;
        ParseIngredientSummaryModalArgument(ingred, out rawIngredient, out trigger, out includeAllIngredients);

        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "issue_src");
        string query;
        string ingredientFilterClause = includeAllIngredients ? string.Empty : " AND component_group = @componentGroup";

        switch (trigger)
        {
            case "trg1":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE ((CMF_REQUEST in ('cmf_ok') and [sysdebug] like ('%customer_must_fix%')) OR ((CMF_REQUEST in ('cmf_ok') AND CMF_REQUEST NOT IN ('cmf_ok','cmf_duplicate'))))" + ingredientFilterClause;
                break;
            case "trg2":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE CMF_REQUEST in ('cmf_ask','cmf_incomplete')" + ingredientFilterClause;
                break;
            case "trg4":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected')" + ingredientFilterClause;
                break;
            case "trg5":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))" + ingredientFilterClause;
                break;
            case "trg6":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))" + ingredientFilterClause;
                break;
            case "trg7":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))" + ingredientFilterClause;
                break;
            case "trg8":
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))" + ingredientFilterClause;
                break;
            default:
                query = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + issueSourceSql + " WHERE ((CMF_REQUEST in ('cmf_reject') and [sysdebug] like ('%customer_must_fix%')) OR ((CMF_REQUEST in ('cmf_reject') AND CMF_REQUEST NOT IN ('cmf_ok','cmf_duplicate'))))" + ingredientFilterClause;
                break;
        }

        return ExecuteDataTable(
            query,
            command =>
            {
                if (!includeAllIngredients)
                {
                    command.Parameters.AddWithValue("@componentGroup", rawIngredient);
                }
            });
    }

    private void ShowModal0()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModal0').modal('show');", true);
    }

    private void ShowModal()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModal').modal('show');", true);
    }

    private void ShowModal2()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModal2').modal('show');", true);
    }

    private void ShowIngredientIndicatorSummaryModal()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowIngredientIndicatorSummaryModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModalIngredientIndicator').modal('show');", true);
    }

    private void ShowModal3()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModal3').modal('show');", true);
    }

    private void ShowOemIndicatorSummaryModal()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowOemIndicatorSummaryModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModalOemIndicator').modal('show');", true);
    }

    private void ShowDesignIndicatorSummaryModal()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowDesignIndicatorSummaryModalScript", "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModalDesignIndicator').modal('show');", true);
    }

    private void BindImplementedVerifiedDetailsModal(string design)
    {
        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "issue_src");
        List<string> selectedDrivers = GetSelectedDrivers();
        StringBuilder modalQuery = new StringBuilder();

        modalQuery.Append("SELECT cp_id, title, status, component, cmf_request, customer_owner, ");
        modalQuery.Append("promoted_id, closed_reason, days_active, idst, drivers ");
        modalQuery.Append("FROM ");
        modalQuery.Append(issueSourceSql);
        modalQuery.Append(" WHERE sysdebug LIKE('%customer_must_fix%') ");
        modalQuery.Append("AND status IN ('implemented', 'verified')");

        if (design == "RVP")
        {
            modalQuery.Append(" AND (customer_detail = '' OR customer_detail IS NULL)");
        }
        else if (design != "Total")
        {
            modalQuery.Append(" AND customer_detail = @design");
        }

        DataTable dt = ExecuteDataTable(
            modalQuery.ToString(),
            command =>
            {
                if (design != "Total" && design != "RVP")
                {
                    command.Parameters.AddWithValue("@design", design);
                }

                AppendDriversInClause(modalQuery, command, selectedDrivers, "modalDriver");
                command.CommandText = modalQuery.ToString();
            });

        GridView_design_summary_modal0.DataSource = dt;
        GridView_design_summary_modal0.DataBind();
    }

    //private void BindImplementedVerifiedDetailsModal(string design)
    //{
    //    string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
    //    string modalquery;
    //    if (design == "Total")
    //    {
    //        modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE sysdebug LIKE('%customer_must_fix%') AND status IN ('implemented', 'verified')";
    //    }
    //    else if (design == "RVP")
    //    {
    //        modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] " +
    //                "FROM " + selectedPlatform + " " +
    //                "WHERE drivers = '" + driver + "' " +
    //                "AND (customer_detail = '' OR customer_detail IS NULL) " + // Check for empty or NULL customer_detail
    //                "AND sysdebug LIKE('%customer_must_fix%') " +
    //                "AND status IN ('implemented', 'verified')";
    //    }
    //    else
    //    {
    //        modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE customer_detail = '" + design + "' AND sysdebug LIKE('%customer_must_fix%') AND status IN ('implemented', 'verified')";
    //    }


    //    using (SqlConnection con = new SqlConnection(connectionString))
    //    using (SqlCommand cmd = new SqlCommand(modalquery, con))
    //    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
    //    {
    //        DataTable dt = new DataTable();
    //        da.Fill(dt);
    //        GridView_design_summary_modal0.DataSource = dt;
    //        GridView_design_summary_modal0.DataBind();
    //    }
    //}

    private void BindDriverDetailsModal(string design, string driver)
    {
        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "issue_src");
        StringBuilder modalQuery = new StringBuilder();
        modalQuery.Append("SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] ");
        modalQuery.Append("FROM ");
        modalQuery.Append(issueSourceSql);
        modalQuery.Append(" WHERE drivers = @driver AND sysdebug LIKE('%customer_must_fix%') AND status IN ('open')");

        if (design == "RVP")
        {
            modalQuery.Append(" AND (customer_detail = '' OR customer_detail IS NULL)");
        }
        else if (design != "Total")
        {
            modalQuery.Append(" AND customer_detail = @design");
        }

        DataTable dt = ExecuteDataTable(
            modalQuery.ToString(),
            command =>
            {
                command.Parameters.AddWithValue("@driver", driver);
                if (design != "Total" && design != "RVP")
                {
                    command.Parameters.AddWithValue("@design", design);
                }
            });

        GridView_design_summary_modal0.DataSource = dt;
        GridView_design_summary_modal0.DataBind();
    }

    private void RegisterFieldSelectorScript()
    {
        string script = @"
            // Ensure field selector is initialized after postback
            if (typeof(resetToDefault) === 'function') {
                resetToDefault();
            }
        ";

        ClientScript.RegisterStartupScript(this.GetType(), "FieldSelectorInit", script, true);
    }

    private void ApplyFocusedPortalMode()
    {
        // Keep only the requested tabs in the UI.
        btnShowGridView2.Visible = false;
        btnShowGridView3.Visible = false;
        btnShowGridView5.Visible = false;
        btnShowGridView6.Visible = false;
        btnShowGridView7.Visible = false;

        bool isAdminMode = ddlUserMode != null &&
            string.Equals(ddlUserMode.SelectedValue, "admin", StringComparison.OrdinalIgnoreCase);

        btnShowGridView9.Visible = isAdminMode;
        lnkNavConfigRules.Visible = isAdminMode;

        if (ddlFocusedView != null)
        {
            ListItem configItem = ddlFocusedView.Items.FindByValue("config");
            if (configItem != null)
            {
                configItem.Enabled = isAdminMode;
                configItem.Attributes["style"] = isAdminMode ? string.Empty : "display:none;";
            }
        }
    }

    private bool IsAdminMode()
    {
        return ddlUserMode != null &&
            string.Equals(ddlUserMode.SelectedValue, "admin", StringComparison.OrdinalIgnoreCase);
    }

    protected override void OnPreRender(EventArgs e)
    {
        ApplyFocusedPortalMode();
        dashboardHeaderTools.Visible = homeWelcomePanel.Visible;
        ddlDashboardDateBasis.Visible = ddlDashboardDateMode.SelectedValue != "all";
        bool isTodayRange = ddlDashboardDateMode.SelectedValue == "today";
        ddlDashboardDateBasis.Items.FindByValue("cmf-request").Text = isTodayRange ? "CMFs Requested" : "CMFs Requested in";
        ddlDashboardDateBasis.Items.FindByValue("submitted").Text = isTodayRange ? "Issues submitted" : "Issues Submitted in";
        string target = Request["__EVENTTARGET"] ?? string.Empty;
        if (IsPostBack && string.IsNullOrWhiteSpace(HomeDashboardSnapshotJson)
            && (target == ddlDashboardDateMode.UniqueID || target == ddlDashboardDateBasis.UniqueID
                || target == btnApplyDashboardDates.UniqueID))
        {
            BindHomeDashboard();
        }
        if (ScriptManager1 != null && lnkPendingBreadcrumbHome != null)
        {
            ScriptManager1.RegisterPostBackControl(lnkPendingBreadcrumbHome);
        }
        if (ScriptManager1 != null && btnPendingViewExport != null)
        {
            ScriptManager1.RegisterPostBackControl(btnPendingViewExport);
        }
        if (ScriptManager1 != null && btnIssueViewExport != null)
        {
            ScriptManager1.RegisterPostBackControl(btnIssueViewExport);
        }
        RegisterActiveTabClientState();
        base.OnPreRender(e);
    }

    private void RegisterActiveTabClientState()
    {
        string activeTab = GetActiveFocusedTab();
        string script = "window.CMF_PORTAL=window.CMF_PORTAL||{};" +
            "window.CMF_PORTAL.activeFocusedTab='" + HttpUtility.JavaScriptStringEncode(activeTab) + "';" +
            "setTimeout(function(){if(typeof syncIssuePendingSidePanelVisibility==='function')syncIssuePendingSidePanelVisibility();},0);" +
            "setTimeout(function(){if(typeof syncIssuePendingSidePanelVisibility==='function')syncIssuePendingSidePanelVisibility();},120);";

        ScriptManager.RegisterStartupScript(this, GetType(), "activeFocusedTabState", script, true);
    }

    private static string NormalizeFocusedTabKey(string tabKey)
    {
        string normalized = (tabKey ?? string.Empty).Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "home":
            case "issue":
            case "cmf":
            case "pending":
            case "reports":
            case "config":
                return normalized;
            default:
                return "home";
        }
    }

    private void SetActiveFocusedTab(string tabKey)
    {
        tabKey = NormalizeFocusedTabKey(tabKey);
        string baseClass = "modern-button tab-pill";
        string navBaseClass = "portal-nav-link";
        btnShowGridView1.CssClass = baseClass;
        btnShowGridView4.CssClass = baseClass;
        btnShowGridView8.CssClass = baseClass;
        btnShowGridView9.CssClass = baseClass;
        lnkNavHome.CssClass = navBaseClass;
        lnkNavCmfSummary.CssClass = navBaseClass;
        lnkNavIssueList.CssClass = navBaseClass;
        lnkNavPendingList.CssClass = navBaseClass;
        lnkNavReports.CssClass = navBaseClass;
        lnkNavConfigRules.CssClass = navBaseClass;
        Session[ActiveFocusedTabSessionKey] = tabKey;
        SyncFocusedViewDropdown(tabKey);

        if (tabKey == "issue")
        {
            btnShowGridView1.CssClass = baseClass + " is-active";
            lnkNavIssueList.CssClass = navBaseClass + " is-active";
            lblActiveViewTitle.Text = "Issue List";
        }
        else if (tabKey == "cmf")
        {
            lnkNavCmfSummary.CssClass = navBaseClass + " is-active";
            lblActiveViewTitle.Text = "CMF Summary";
        }
        else if (tabKey == "pending")
        {
            btnShowGridView4.CssClass = baseClass + " is-active";
            lnkNavPendingList.CssClass = navBaseClass + " is-active";
            lblActiveViewTitle.Text = "CMF Pending";
        }
        else if (tabKey == "reports")
        {
            btnShowGridView8.CssClass = baseClass + " is-active";
            lnkNavReports.CssClass = navBaseClass + " is-active";
            lblActiveViewTitle.Text = "Reports & Analytics";
        }
        else if (tabKey == "config")
        {
            btnShowGridView9.CssClass = baseClass + " is-active";
            lnkNavConfigRules.CssClass = navBaseClass + " is-active";
            lblActiveViewTitle.Text = "Config/CMF Rules";
        }
        else
        {
            lnkNavHome.CssClass = navBaseClass + " is-active";
            lblActiveViewTitle.Text = "Dashboard";
        }
    }

    private void SyncFocusedViewDropdown(string tabKey)
    {
        if (ddlFocusedView == null || ddlFocusedView.Items.Count == 0)
        {
            return;
        }

        ListItem selectedItem = ddlFocusedView.Items.FindByValue(tabKey);
        if (selectedItem == null)
        {
            return;
        }

        ddlFocusedView.ClearSelection();
        selectedItem.Selected = true;
    }

    protected string GetActiveFocusedTab()
    {
        string activeTab = Session[ActiveFocusedTabSessionKey] as string;
        return NormalizeFocusedTabKey(activeTab);
    }

    private static bool IsQuickLinkDesignEventTarget(string eventTarget)
    {
        return string.Equals(eventTarget, "lnkQuickOverallDesign", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventTarget, "lnkQuickPlatformDesign", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventTarget, "lnkQuickLegacyDesign", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsQuickLinkIngredientEventTarget(string eventTarget)
    {
        return string.Equals(eventTarget, "lnkQuickOverallIngredient", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventTarget, "lnkQuickPlatformIngredient", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventTarget, "lnkQuickLegacyIngredient", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsQuickLinkOemEventTarget(string eventTarget)
    {
        return string.Equals(eventTarget, "lnkQuickOverallOem", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventTarget, "lnkQuickPlatformOem", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventTarget, "lnkQuickLegacyOem", StringComparison.OrdinalIgnoreCase);
    }

    private void RehydrateFocusedTabForPopupFlow()
    {
        string activeFocusedTab = GetActiveFocusedTab();
        SetActiveFocusedTab(activeFocusedTab);

        if (string.Equals(activeFocusedTab, "home", StringComparison.OrdinalIgnoreCase))
        {
            ShowWelcomeHome();
            return;
        }

        if (string.Equals(activeFocusedTab, "cmf", StringComparison.OrdinalIgnoreCase))
        {
            ShowModernCmfSummary();
            return;
        }

        RebindFocusedTabData(string.Equals(activeFocusedTab, "reports", StringComparison.OrdinalIgnoreCase));
    }

    private void PrepareHomeQuickLinkPopupFlow()
    {
        if (ScriptManager1 != null && ScriptManager1.IsInAsyncPostBack)
        {
            selectedPlatform = ResolveDashboardPlatformSelection();
            return;
        }

        SetActiveFocusedTab("home");
        ShowWelcomeHome();
    }

    protected void btnPopupAsyncDispatch_Click(object sender, EventArgs e)
    {
        string action = (hfPopupAsyncAction != null ? hfPopupAsyncAction.Value : string.Empty) ?? string.Empty;
        string arg1 = (hfPopupAsyncArg1 != null ? hfPopupAsyncArg1.Value : string.Empty) ?? string.Empty;
        string arg2 = (hfPopupAsyncArg2 != null ? hfPopupAsyncArg2.Value : string.Empty) ?? string.Empty;
        string arg3 = (hfPopupAsyncArg3 != null ? hfPopupAsyncArg3.Value : string.Empty) ?? string.Empty;

        string packedArgument = Request["__EVENTARGUMENT"] ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(packedArgument))
        {
            string[] packedParts = packedArgument.Split(new[] { "||" }, StringSplitOptions.None);

            if (string.IsNullOrWhiteSpace(action) && packedParts.Length > 0)
            {
                action = packedParts[0] ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(arg1) && packedParts.Length > 1)
            {
                arg1 = HttpUtility.UrlDecode(packedParts[1] ?? string.Empty);
            }

            if (string.IsNullOrWhiteSpace(arg2) && packedParts.Length > 2)
            {
                arg2 = HttpUtility.UrlDecode(packedParts[2] ?? string.Empty);
            }

            if (string.IsNullOrWhiteSpace(arg3) && packedParts.Length > 3)
            {
                arg3 = HttpUtility.UrlDecode(packedParts[3] ?? string.Empty);
            }
        }

        switch (action)
        {
            case "ShowDesignIndicatorSummaryModal":
                PrepareHomeQuickLinkPopupFlow();
                BindGridView_design_summary();
                ShowDesignIndicatorSummaryModal();
                break;

            case "ShowIngredientIndicatorSummaryModal":
                PrepareHomeQuickLinkPopupFlow();
                BindGridView_component_summary();
                ShowIngredientIndicatorSummaryModal();
                break;

            case "ShowOemIndicatorSummaryModal":
                PrepareHomeQuickLinkPopupFlow();
                BindGridView_oem_summary();
                ShowOemIndicatorSummaryModal();
                break;

            case "ShowModal":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenModalDesign.Value = arg1;
                    Session["DesignValue"] = arg1;
                    BindModalGrid(arg1);
                    ShowModal();
                }
                break;

            case "ShowModal2":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenModalDesign.Value = arg1;
                    Session["DesignValue"] = arg1;
                    BindModalGrid2(arg1);
                    ShowModal2();
                }
                break;

            case "ShowModal3":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenModalDesign.Value = arg1;
                    Session["DesignValue"] = arg1;
                    BindModalGrid3(arg1);
                    ShowModal3();
                }
                break;

            case "ShowDriverModal":
                if (!string.IsNullOrWhiteSpace(arg1) && !string.IsNullOrWhiteSpace(arg2))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenModalDesign.Value = arg1;
                    if (HiddenModalDriver != null)
                    {
                        HiddenModalDriver.Value = arg2;
                    }
                    Session["DesignValue"] = arg1;
                    BindDriverDetailsModal(arg1, arg2);
                    ShowModal0();
                }
                break;

            case "ShowImplementedVerifiedModal":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenModalDesign.Value = arg1;
                    Session["DesignValue"] = arg1;
                    BindImplementedVerifiedDetailsModal(arg1);
                    ShowModal0();
                }
                break;

            case "ShowDriverIssues":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenDriverName.Value = arg1;
                    Session["DriverName"] = arg1;
                    BindDriverIssues(arg1);
                    ShowDriverIssues();
                }
                break;

            case "ShowCMFIssues":
                if (!string.IsNullOrWhiteSpace(arg1) && !string.IsNullOrWhiteSpace(arg2) && !string.IsNullOrWhiteSpace(arg3))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    HiddenComponentName.Value = arg1;
                    HiddenDriverName.Value = arg2;
                    HiddenIssueType.Value = arg3;

                    Session["ComponentName"] = arg1;
                    Session["DriverName"] = arg2;
                    Session["IssueType"] = arg3;

                    BindCMFIssues(arg1, arg2, arg3);
                    ShowCMFIssues();
                }
                break;

            case "ShowSummaryMetricIssues":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    BindSummaryMetricIssues(arg1);
                    ShowSummaryMetricIssuesModal();
                }
                break;

            case "ShowPendingSummaryIssues":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    BindPendingSummaryIssues(arg1);
                    ShowCMFIssues();
                }
                break;

            case "ShowPendingAgeBucketIssues":
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    PrepareHomeQuickLinkPopupFlow();
                    BindPendingAgeBucketIssues(arg1);
                    ShowCMFIssues();
                }
                break;
        }

        if (hfPopupAsyncAction != null) hfPopupAsyncAction.Value = string.Empty;
        if (hfPopupAsyncArg1 != null) hfPopupAsyncArg1.Value = string.Empty;
        if (hfPopupAsyncArg2 != null) hfPopupAsyncArg2.Value = string.Empty;
        if (hfPopupAsyncArg3 != null) hfPopupAsyncArg3.Value = string.Empty;
    }

    protected string GetIssueGlobalSearchQuery()
    {
        return Session[IssueGlobalSearchSessionKey] as string ?? string.Empty;
    }

    private void RebindFocusedTabData(bool includeReportsData)
    {
        // Keep postback refreshes scoped to the three supported tabs.
        ApplyIssuePendingPlatformContext();
        string activeTab = GetActiveFocusedTab();
        if (string.Equals(activeTab, "pending", StringComparison.OrdinalIgnoreCase))
        {
            EnsurePendingTabVisibleForPostback();
            BindGridView_cmf_pending();
            UpdateCmfPendingKpis();
            return;
        }

        if (string.Equals(activeTab, "reports", StringComparison.OrdinalIgnoreCase))
        {
            if (includeReportsData)
            {
                BindDistinctValues();
                BindDistinctFilters();
            }
            return;
        }
        
        if (string.Equals(activeTab, "home", StringComparison.OrdinalIgnoreCase))
        {
            ShowWelcomeHome();
            return;
        }

        if (string.Equals(activeTab, "cmf", StringComparison.OrdinalIgnoreCase))
        {
            ShowModernCmfSummary();
            return;
        }

        EnsureIssueTabVisibleForPostback();
        BindAllFilters();
        BindGridView(null, null, bindRelatedGrids: false);
    }

    private string GetIssuePendingPlatform()
    {
        string value = Session[IssuePendingPlatformSessionKey] as string;
        if (string.IsNullOrWhiteSpace(value) || IsOverallDashboardPlatform(value))
        {
            value = DefaultIssuePendingPlatformTable;
        }

        return ResolveIssuePendingPlatformSelection(value);
    }

    private List<string> GetPendingPlatformSelections()
    {
        string[] selections = Session[PendingPlatformsSessionKey] as string[];
        List<string> platforms = (selections ?? new string[0])
            .Where(value => AllowedPlatformTables.Contains(value) && !IsOverallDashboardPlatform(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (platforms.Count == 0) platforms.Add(GetIssuePendingPlatform());
        return platforms;
    }

    private void ApplyIssuePendingPlatformContext()
    {
        selectedPlatform = GetIssuePendingPlatform();
    }

    private void ResetIssueFiltersToAll()
    {
        Session["ownerFilter"] = "All";
        Session["rvpReproFilter"] = "All";
        Session["idstFilter"] = "All";
        Session["losFilter"] = "All";
        Session["milestoneFilter"] = "All";
        Session["companyFilter"] = "All";
        Session["detailFilter"] = "All";
        Session["componentFilter"] = "All";
        Session["issueStatusFilter"] = "All";
    }

    private void SetIssuePagerVisible(bool isVisible)
    {
        if (issuePagerPanel != null)
        {
            issuePagerPanel.Visible = isVisible;
        }
    }

    private void ApplyIssuePageSizeFromSession()
    {
        int pageSize;
        if (!int.TryParse(Convert.ToString(Session["issuePageSize"]), out pageSize))
        {
            pageSize = 12;
        }

        if (pageSize != 10 && pageSize != 12 && pageSize != 25 && pageSize != 50 && pageSize != 100)
        {
            pageSize = 12;
        }

        overall_request_details.PageSize = pageSize;

        if (ddlIssuePageSize != null)
        {
            ListItem item = ddlIssuePageSize.Items.FindByValue(pageSize.ToString(CultureInfo.InvariantCulture));
            if (item != null)
            {
                ddlIssuePageSize.ClearSelection();
                item.Selected = true;
            }
        }
    }

    private void SetMainDataWrapperVisible(bool isVisible)
    {
        if (mainDataWrapper != null)
        {
            // Keep wrapper rendered so async popup dispatch controls remain available on Home.
            mainDataWrapper.Visible = true;
            mainDataWrapper.Style["display"] = isVisible ? string.Empty : "none";
            if (isVisible)
            {
                mainDataWrapper.Attributes.Remove("aria-hidden");
            }
            else
            {
                mainDataWrapper.Attributes["aria-hidden"] = "true";
            }
        }
    }

    private static string GetTimeBasedGreeting()
    {
        int hour = DateTime.Now.Hour;

        if (hour < 12)
        {
            return "Good Morning";
        }

        if (hour < 17)
        {
            return "Good Afternoon";
        }

        return "Good Evening";
    }

    private void ShowWelcomeHome()
    {
        LoadCmfRulesEditor();

        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = true;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = false;
        }

        if (lblWelcomeMode != null && ddlUserMode != null && ddlUserMode.SelectedItem != null)
        {
            lblWelcomeMode.Text = ddlUserMode.SelectedItem.Text;
        }

        if (lblHomeGreeting != null)
        {
            lblHomeGreeting.Text = GetTimeBasedGreeting();
        }

        BindHomeDashboard();

        overall_request_details.Visible = false;
        GridView_cmf_pending.Visible = false;
        analyticsPanel.Visible = false;
        SetMainDataWrapperVisible(false);
        issueListHeaderPanel.Visible = false;
        sharedFilterPanel.Visible = false;
        fieldSelectorPanel.Visible = false;
        SetIssuePagerVisible(false);
        configRulesPanel.Visible = false;
        reportsPlaceholderPanel.Visible = false;

        pane3.Visible = false;
        pane4.Visible = false;
        pane1.Visible = false;
        pane8.Visible = false;
        pane9.Visible = false;

        btnExportToExcel.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
    }

    private void ShowModernCmfSummary()
    {
        LoadCmfRulesEditor();

        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = true;
        }

        BindHomeDashboard();

        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        analyticsPanel.Visible = false;
        SetMainDataWrapperVisible(false);
        issueListHeaderPanel.Visible = false;
        sharedFilterPanel.Visible = false;
        fieldSelectorPanel.Visible = false;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(false);
        configRulesPanel.Visible = false;
        reportsPlaceholderPanel.Visible = false;

        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        pane8.Visible = false;
        pane9.Visible = false;

        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;

    }

    private void EnsureIssueTabVisibleForPostback()
    {
        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = false;
        }

        SetMainDataWrapperVisible(true);
        searchfilters.Visible = false;
        analyticsPanel.Visible = false;

        overall_request_details.Visible = true;
        GridView_cmf_pending.Visible = false;
        fieldSelectorPanel.Visible = true;
        issueListHeaderPanel.Visible = true;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(true);

        pane3.Visible = true;
        pane4.Visible = false;
        pane8.Visible = false;
        pane9.Visible = false;

        btnExportToExcel.Visible = true;
        btnExportToExcel_cmf_pending.Visible = false;
        reportsPlaceholderPanel.Visible = false;
        configRulesPanel.Visible = false;

        InitializeSharedFilterPanel();
    }

    private void EnsurePendingTabVisibleForPostback()
    {
        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = false;
        }

        SetMainDataWrapperVisible(true);
        searchfilters.Visible = false;
        analyticsPanel.Visible = false;

        overall_request_details.Visible = false;
        GridView_cmf_pending.Visible = true;
        fieldSelectorPanel.Visible = false;
        issueListHeaderPanel.Visible = false;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(false);

        pane3.Visible = false;
        pane4.Visible = true;
        pane8.Visible = false;
        pane9.Visible = false;

        btnExportToExcel.Visible = false;
        btnExportToExcel_cmf_pending.Visible = true;
        reportsPlaceholderPanel.Visible = false;
        configRulesPanel.Visible = false;

        InitializeSharedFilterPanel();
    }

    private void LoadCmfRulesEditor()
    {
        if (txtCmfRules != null)
        {
            txtCmfRules.Text = CmfRecommendationService.GetActiveRulesText();
        }

        if (lblCmfRulesStatus != null && string.IsNullOrWhiteSpace(lblCmfRulesStatus.Text))
        {
            lblCmfRulesStatus.Text = string.Empty;
        }
    }

    private void BindHomeDashboard()
    {
        HomeDashboardSnapshot snapshot = BuildHomeDashboardSnapshot();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;

        HomeDashboardSnapshotJson = serializer.Serialize(snapshot);
        HomeDashboardPlatformLabel = snapshot.PlatformLabel;

        if (lblHomeActiveIssuesValue != null)
        {
            lblHomeActiveIssuesValue.Text = snapshot.ActiveIssues.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeNeedsAttentionValue != null)
        {
            lblHomeNeedsAttentionValue.Text = snapshot.NeedsAttention.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeResolvedThisWeekValue != null)
        {
            lblHomeResolvedThisWeekValue.Text = snapshot.ClosedIssues.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeResolutionDaysValue != null)
        {
            lblHomeResolutionDaysValue.Text = snapshot.StaleIssues.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeCustomersAffectedValue != null)
        {
            lblHomeCustomersAffectedValue.Text = snapshot.CustomersAffected.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeAiNewTodayValue != null)
        {
            lblHomeAiNewTodayValue.Text = snapshot.NewToday.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeAiClosedTodayValue != null)
        {
            lblHomeAiClosedTodayValue.Text = snapshot.ResolvedToday.ToString(CultureInfo.InvariantCulture);
        }

        if (lblHomeDashboardGeneratedAt != null)
        {
            lblHomeDashboardGeneratedAt.Text = snapshot.GeneratedAt;
        }

        if (lnkPlatformDashboard != null)
        {
            lnkPlatformDashboard.Visible = lnkPlatformDashboard.Visible;
        }
    }

    [WebMethod(EnableSession = true)]
    public static AiSummaryResponse GetHomeExecutiveSummary(string platformLabel, string snapshotContext)
    {
        try
        {
            return AiSummaryService.GenerateDashboardExecutiveSummary(platformLabel, snapshotContext);
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "Dashboard executive summary failed: " + ex.Message
            };
        }
    }

    [WebMethod(EnableSession = true)]
    public static AiSummaryResponse GetHomePredictedBlockers(string platformLabel, string snapshotContext)
    {
        try
        {
            return AiSummaryService.GenerateDashboardPredictedBlockers(platformLabel, snapshotContext);
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "Dashboard blocker prediction failed: " + ex.Message
            };
        }
    }

    private string BuildHomeDashboardSourceSql(string platformTable)
    {
        platformTable = ResolveDashboardPlatformSelection(platformTable);
        string dashboardSourceSql = BuildDashboardComponentSourceSql(platformTable, true);
        string filterValue = Session["filterValue"] as string;
        if (string.IsNullOrWhiteSpace(filterValue) || filterValue == "AllDrivers")
        {
            filterValue = BuildDefaultIssueDriverFilter(platformTable);
            Session["filterValue"] = filterValue;
        }

        Dictionary<string, string> filters = GetAllFilterValues();
        StringBuilder whereClause = new StringBuilder();
        whereClause.Append("WHERE status NOT IN ('rejected') AND sysdebug LIKE ('%customer_must_fix%') AND cmf_request IN ('cmf_ok') ");
        AppendDashboardDriverFilter(whereClause, filterValue);
        AppendDashboardColumnFilters(whereClause, filters);

        return @"(SELECT
    status,
    priority,
    customer_impact,
    title,
    implemented_date,
    date_cmf_decided,
    date_cmf_ask,
    component_group,
    drivers,
    customer_owner,
    repro_on_rvp,
    idst,
    los,
    customer_company,
    customer_detail
FROM " + dashboardSourceSql + @"
" + whereClause.ToString() + ") AS platform_cmf_issues";
    }

    private string BuildDefaultIssueDriverFilter(string platformTable)
    {
        return string.Join(",", GetDistinctDrivers(platformTable, "open", "implemented").ToArray());
    }

    private static bool UsesPublicIssueStatusScope(string platformTable)
    {
        return string.Equals(platformTable, "CMF_NVL_H_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(platformTable, "CMF_NVL_S_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendDashboardDriverFilter(StringBuilder whereClause, string filterValue)
    {
        if (whereClause == null || string.IsNullOrWhiteSpace(filterValue) || filterValue == "AllDrivers")
        {
            return;
        }

        if (filterValue.Contains(","))
        {
            whereClause.Append("AND ((");
            whereClause.Append("'");
            whereClause.Append(EscapeSqlLiteral(filterValue));
            whereClause.Append("' = 'Pre-PV' AND drivers LIKE '%WW%' AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) OR '");
            whereClause.Append(EscapeSqlLiteral(filterValue));
            whereClause.Append("' LIKE '%,' + drivers + ',%' OR '");
            whereClause.Append(EscapeSqlLiteral(filterValue));
            whereClause.Append("' LIKE drivers + ',%' OR '");
            whereClause.Append(EscapeSqlLiteral(filterValue));
            whereClause.Append("' LIKE '%,' + drivers) ");
            return;
        }

        whereClause.Append("AND (( '");
        whereClause.Append(EscapeSqlLiteral(filterValue));
        whereClause.Append("' = 'Pre-PV' AND drivers LIKE '%WW%' AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) OR drivers = '");
        whereClause.Append(EscapeSqlLiteral(filterValue));
        whereClause.Append("') ");
    }

    private static void AppendDashboardColumnFilters(StringBuilder whereClause, Dictionary<string, string> filters)
    {
        if (whereClause == null || filters == null)
        {
            return;
        }

        foreach (KeyValuePair<string, string> filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Value) || filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string columnName = null;
            switch (filter.Key)
            {
                case "owner": columnName = "customer_owner"; break;
                case "rvpRepro": columnName = "repro_on_rvp"; break;
                case "idst": columnName = "idst"; break;
                case "los": columnName = "los"; break;
                case "milestone": columnName = "drivers"; break;
                case "Company": columnName = "customer_company"; break;
                case "Detail": columnName = "customer_detail"; break;
                case "Component": columnName = "component_group"; break;
            }

            if (!string.IsNullOrWhiteSpace(columnName))
            {
                whereClause.Append("AND LTRIM(RTRIM(ISNULL(");
                whereClause.Append(columnName);
                whereClause.Append(", ''))) = '");
                whereClause.Append(EscapeSqlLiteral(filter.Value.Trim()));
                whereClause.Append("' ");
            }
        }
    }

    private static string EscapeSqlLiteral(string value)
    {
        return (value ?? string.Empty).Replace("'", "''");
    }

    private HomeDashboardSnapshot BuildHomeDashboardSnapshot()
    {
        string dashboardPlatform = ResolveDashboardPlatformSelection();
        string dashboardSourceSql = BuildHomeDashboardSourceSql(dashboardPlatform);
        HomeDashboardSnapshot snapshot = new HomeDashboardSnapshot();
        snapshot.PlatformLabel = BuildPlatformDisplayName(dashboardPlatform);
        snapshot.GeneratedAt = DateTime.Now.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);
        snapshot.PredictedBlockers = new List<string>();
        snapshot.WeeklyChanges = new List<string>();
        snapshot.SummaryFacts = new List<HomeDashboardFact>();
        snapshot.TptFacts = new List<HomeDashboardFact>();
        snapshot.MilestoneSummary = NewDashboardTable("Milestone", "Unique CMF Count");
        snapshot.ComponentSummary = NewDashboardTable("Ingredient");
        snapshot.PendingSummary = NewDashboardTable("Ingredient", "iDST Name", "CMF Pending");
        snapshot.DesignSummary = NewDashboardTable(
            "Design Name",
            "SW Image Freeze",
            "Support Model",
            "Impl/Verified");
        snapshot.Trend = new List<HomeDashboardTrendPoint>();
        snapshot.StatusDistribution = new List<HomeDashboardCategoryPoint>();
        snapshot.TopComponents = new List<HomeDashboardCategoryPoint>();
        snapshot.OemSummary = new List<HomeDashboardOemRow>();
        snapshot.HeatmapPlatforms = new List<string>();
        snapshot.ComponentRiskHeatmap = new List<HomeDashboardHeatmapRow>();
        snapshot.PendingAgeBuckets = new List<HomeDashboardCategoryPoint>();
        HomeDashboardCategoryPoint milestoneRiskConcentration = null;
        HomeDashboardCategoryPoint predictedTopRisk = null;

        using (SqlConnection con = new SqlConnection(ConnectionString))
        {
            con.Open();

            using (SqlCommand metricsCommand = new SqlCommand(@"
SELECT
                COUNT(1) AS ActiveIssues,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'open' THEN 1 ELSE 0 END) AS NeedsAttention,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS ClosedIssues,
                SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS StaleIssues,
    SUM(CASE WHEN TRY_CAST(implemented_date AS DATE) >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) AS ResolvedThisWeek,
    ISNULL(AVG(CAST(CASE
        WHEN TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND TRY_CAST(implemented_date AS DATE) IS NOT NULL
             AND TRY_CAST(implemented_date AS DATE) >= TRY_CAST(date_cmf_decided AS DATE)
        THEN DATEDIFF(DAY, TRY_CAST(date_cmf_decided AS DATE), TRY_CAST(implemented_date AS DATE))
        ELSE NULL END AS DECIMAL(10, 2))), 0) AS AverageResolutionDays,
    SUM(CASE WHEN ISNULL(status, '') NOT IN ('complete', 'rejected', 'implemented', 'verified')
              AND ISNULL(customer_impact, '') IN ('1-critical', '2-high', '3-medium')
        THEN 1 ELSE 0 END) AS CustomersAffected,
        SUM(CASE WHEN TRY_CAST(date_cmf_ask AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS NewToday,
        SUM(CASE WHEN TRY_CAST(implemented_date AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS ResolvedToday
FROM " + dashboardSourceSql, con))
            using (SqlDataReader reader = metricsCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    snapshot.ActiveIssues = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);
                    snapshot.NeedsAttention = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);
                    snapshot.ClosedIssues = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture);
                    snapshot.StaleIssues = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture);
                    snapshot.ResolvedThisWeek = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4), CultureInfo.InvariantCulture);
                    snapshot.AverageResolutionDays = reader.IsDBNull(5) ? 0 : Convert.ToDecimal(reader.GetValue(5), CultureInfo.InvariantCulture);
                    snapshot.CustomersAffected = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6), CultureInfo.InvariantCulture);
                    snapshot.NewToday = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7), CultureInfo.InvariantCulture);
                    snapshot.ResolvedToday = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8), CultureInfo.InvariantCulture);
                    snapshot.AiWatchlist = snapshot.NeedsAttention;
                }
            }

            using (SqlCommand trendCommand = new SqlCommand(@"
WITH weeks AS (
    SELECT 0 AS week_offset
    UNION ALL
    SELECT week_offset + 1
    FROM weeks
    WHERE week_offset < 103
),
new_issues AS (
    SELECT DATEDIFF(WEEK, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) AS week_offset, COUNT(*) AS count_value
    FROM " + dashboardSourceSql + @"
    WHERE TRY_CAST(date_cmf_ask AS DATE) IS NOT NULL
      AND DATEDIFF(WEEK, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) BETWEEN 0 AND 103
            AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'open'
    GROUP BY DATEDIFF(WEEK, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE))
),
resolved_issues AS (
    SELECT DATEDIFF(WEEK, TRY_CAST(implemented_date AS DATE), CAST(GETDATE() AS DATE)) AS week_offset, COUNT(*) AS count_value
    FROM " + dashboardSourceSql + @"
    WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL
      AND DATEDIFF(WEEK, TRY_CAST(implemented_date AS DATE), CAST(GETDATE() AS DATE)) BETWEEN 0 AND 103
            AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('complete', 'rejected')
    GROUP BY DATEDIFF(WEEK, TRY_CAST(implemented_date AS DATE), CAST(GETDATE() AS DATE))
)
SELECT
    DATEADD(WEEK, -w.week_offset, CAST(GETDATE() AS DATE)) AS week_date,
    ISNULL(n.count_value, 0) AS new_issues,
        ISNULL(r.count_value, 0) AS resolved_issues,
        0 AS needs_attention
FROM weeks w
LEFT JOIN new_issues n ON n.week_offset = w.week_offset
LEFT JOIN resolved_issues r ON r.week_offset = w.week_offset
ORDER BY week_date
OPTION (MAXRECURSION 0)", con))
            using (SqlDataReader reader = trendCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    DateTime weekDate = reader.IsDBNull(0) ? DateTime.Now.Date : reader.GetDateTime(0);
                    HomeDashboardTrendPoint point = new HomeDashboardTrendPoint();
                    point.WeekStart = weekDate;
                    point.WeekLabel = FormatWorkWeekLabel(weekDate);
                    point.NewIssues = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    point.ResolvedIssues = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    point.NeedsAttention = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                    snapshot.Trend.Add(point);
                }
            }

            using (SqlCommand statusCommand = new SqlCommand(@"
SELECT TOP 5
    CASE
        WHEN ISNULL(status, '') = '' THEN 'Unknown'
        WHEN status IN ('complete', 'rejected') THEN 'Closed'
        WHEN status IN ('implemented', 'verified') THEN 'Validation'
        ELSE status
    END AS status_bucket,
    COUNT(*) AS bucket_count
FROM " + dashboardSourceSql + @"
GROUP BY CASE
        WHEN ISNULL(status, '') = '' THEN 'Unknown'
        WHEN status IN ('complete', 'rejected') THEN 'Closed'
        WHEN status IN ('implemented', 'verified') THEN 'Validation'
        ELSE status
    END
ORDER BY bucket_count DESC", con))
            using (SqlDataReader reader = statusCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    HomeDashboardCategoryPoint point = new HomeDashboardCategoryPoint();
                    point.Name = reader.IsDBNull(0) ? "Unknown" : reader.GetString(0);
                    point.Value = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    snapshot.StatusDistribution.Add(point);
                }
            }

            using (SqlCommand componentCommand = new SqlCommand(@"
SELECT TOP 6
    CASE WHEN ISNULL(component_group, '') = '' THEN 'Unassigned' ELSE component_group END AS component_group,
    COUNT(*) AS issue_count
FROM " + dashboardSourceSql + @"
GROUP BY CASE WHEN ISNULL(component_group, '') = '' THEN 'Unassigned' ELSE component_group END
ORDER BY issue_count DESC", con))
            using (SqlDataReader reader = componentCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    HomeDashboardCategoryPoint point = new HomeDashboardCategoryPoint();
                    point.Name = reader.IsDBNull(0) ? "Unassigned" : reader.GetString(0);
                    point.Value = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    snapshot.TopComponents.Add(point);
                }
            }

            using (SqlCommand milestoneRiskCommand = new SqlCommand(@"
SELECT TOP 1
    CASE WHEN ISNULL(LTRIM(RTRIM(drivers)), '') = '' THEN 'Unassigned milestone' ELSE LTRIM(RTRIM(drivers)) END AS milestone,
    COUNT(*) AS risk_count
FROM " + dashboardSourceSql + @"
GROUP BY CASE WHEN ISNULL(LTRIM(RTRIM(drivers)), '') = '' THEN 'Unassigned milestone' ELSE LTRIM(RTRIM(drivers)) END
ORDER BY risk_count DESC, milestone", con))
            using (SqlDataReader reader = milestoneRiskCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    milestoneRiskConcentration = new HomeDashboardCategoryPoint();
                    milestoneRiskConcentration.Name = reader.IsDBNull(0) ? "Unassigned milestone" : reader.GetString(0);
                    milestoneRiskConcentration.Value = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                }
            }

            using (SqlCommand topRiskCommand = new SqlCommand(@"
SELECT TOP 1
    risk_name,
    SUM(risk_weight) AS risk_score
FROM (
    SELECT
        CASE
            WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p0-blocker', 'p1-showstopper') THEN 'Priority blocker / showstopper risk'
            WHEN LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) IN ('1-critical', '2-high') THEN 'Critical customer impact risk'
            WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p2-high', 'p2') THEN 'High priority execution risk'
            WHEN LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) = '3-medium' THEN 'Medium customer impact risk'
            ELSE 'General workload risk'
        END AS risk_name,
        CASE
            WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p0-blocker', 'p1-showstopper') THEN 6
            WHEN LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) IN ('1-critical', '2-high') THEN 5
            WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p2-high', 'p2') THEN 4
            WHEN LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) = '3-medium' THEN 3
            ELSE 1
        END AS risk_weight
    FROM " + dashboardSourceSql + @"
) risk_candidates
GROUP BY risk_name
ORDER BY risk_score DESC, risk_name", con))
            using (SqlDataReader reader = topRiskCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    predictedTopRisk = new HomeDashboardCategoryPoint();
                    predictedTopRisk.Name = reader.IsDBNull(0) ? "General workload risk" : reader.GetString(0);
                    predictedTopRisk.Value = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture);
                }
            }

            using (SqlCommand blockerCommand = new SqlCommand(@"
SELECT TOP 3
    CASE WHEN LEN(LTRIM(RTRIM(ISNULL(title, '')))) > 92 THEN LEFT(LTRIM(RTRIM(title)), 89) + '...' ELSE LTRIM(RTRIM(ISNULL(title, 'Untitled issue'))) END AS issue_title
FROM " + dashboardSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(title)), '') <> ''
ORDER BY
    CASE
        WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p0-blocker', 'p1-showstopper') THEN 1
        WHEN LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) IN ('1-critical', '2-high') THEN 2
        WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p2-high', 'p2') THEN 3
        WHEN LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) = '3-medium' THEN 4
        ELSE 5
    END,
    TRY_CAST(date_cmf_ask AS DATE) DESC", con))
            using (SqlDataReader reader = blockerCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string blocker = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    if (!string.IsNullOrWhiteSpace(blocker))
                    {
                        snapshot.PredictedBlockers.Add(blocker);
                    }
                }
            }

            using (SqlCommand weeklyChangesCommand = new SqlCommand(@"
SELECT
    SUM(CASE WHEN TRY_CAST(date_cmf_ask AS DATE) >= DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) AS NewThisWeek,
    SUM(CASE WHEN TRY_CAST(implemented_date AS DATE) >= DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)) AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('implemented', 'verified') THEN 1 ELSE 0 END) AS ImplementedThisWeek,
    SUM(CASE WHEN TRY_CAST(implemented_date AS DATE) >= DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)) AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS ClosedThisWeek,
    SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) IN ('p0-blocker', 'p1-showstopper') OR LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) IN ('1-critical', '2-high') THEN 1 ELSE 0 END) AS HighRiskNow
FROM " + dashboardSourceSql, con))
            using (SqlDataReader reader = weeklyChangesCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    int newThisWeek = ReadInt(reader, 0);
                    int implementedThisWeek = ReadInt(reader, 1);
                    int closedThisWeek = ReadInt(reader, 2);
                    int highRiskNow = ReadInt(reader, 3);

                    snapshot.WeeklyChanges.Add(newThisWeek > 0
                        ? newThisWeek.ToString(CultureInfo.InvariantCulture) + " new issue" + (newThisWeek == 1 ? " was" : "s were") + " added this week."
                        : "No new issues were added this week.");
                    snapshot.WeeklyChanges.Add(implementedThisWeek > 0
                        ? implementedThisWeek.ToString(CultureInfo.InvariantCulture) + " issue" + (implementedThisWeek == 1 ? " moved" : "s moved") + " into implemented or verified status."
                        : "No issues moved into implemented or verified status this week.");
                    snapshot.WeeklyChanges.Add(closedThisWeek > 0
                        ? closedThisWeek.ToString(CultureInfo.InvariantCulture) + " issue" + (closedThisWeek == 1 ? " was" : "s were") + " closed this week."
                        : "No issues were closed this week.");
                    snapshot.WeeklyChanges.Add(highRiskNow > 0
                        ? highRiskNow.ToString(CultureInfo.InvariantCulture) + " high-priority or high-impact issue" + (highRiskNow == 1 ? " remains" : "s remain") + " in the current workload."
                        : "No high-priority or high-impact issues are visible in the current workload.");
                }
            }

            PopulateOverallDashboardLayoutData(snapshot, dashboardPlatform, dashboardSourceSql, con);
        }

        PopulateHomeCmfSummaryTables(snapshot, dashboardPlatform);
        PopulateHomePortalHealth(snapshot, milestoneRiskConcentration, predictedTopRisk);

        if (snapshot.Trend.Count > 1)
        {
            snapshot.Trend = snapshot.Trend.OrderBy(point => point.WeekStart).ToList();
        }

        return snapshot;
    }

    private void PopulateOverallDashboardLayoutData(HomeDashboardSnapshot snapshot, string dashboardPlatform, string dashboardSourceSql, SqlConnection connection)
    {
        if (snapshot == null || connection == null)
        {
            return;
        }

        if (!IsOverallDashboardPlatform(dashboardPlatform))
        {
            return;
        }

        PopulateOverallOemSummary(snapshot, dashboardSourceSql, connection);
        PopulateOverallComponentRiskHeatmap(snapshot, connection);
        PopulateOverallPendingAgeBuckets(snapshot, dashboardPlatform, connection);
    }

    private void PopulateOverallOemSummary(HomeDashboardSnapshot snapshot, string dashboardSourceSql, SqlConnection connection)
    {
        if (snapshot == null || connection == null)
        {
            return;
        }

        string overallComponentSourceSql = BuildDashboardComponentSourceSql(OverallDashboardPlatformTable, true);
        if (string.IsNullOrWhiteSpace(overallComponentSourceSql))
        {
            return;
        }

        string sql = @"
WITH issue_base AS (
    SELECT
        CASE WHEN ISNULL(LTRIM(RTRIM(customer_company)), '') = '' THEN 'Unassigned' ELSE LTRIM(RTRIM(customer_company)) END AS oem,
        LOWER(LTRIM(RTRIM(ISNULL(status, '')))) AS status_norm,
        LOWER(LTRIM(RTRIM(ISNULL(priority, '')))) AS priority_norm,
        LOWER(LTRIM(RTRIM(ISNULL(customer_impact, '')))) AS impact_norm,
        TRY_CAST(date_cmf_ask AS DATE) AS ask_date,
        TRY_CAST(date_cmf_decided AS DATE) AS decided_date,
        TRY_CAST(implemented_date AS DATE) AS implemented_date
    FROM " + overallComponentSourceSql + @"
    WHERE LOWER(LTRIM(RTRIM(ISNULL(sysdebug, '')))) LIKE '%customer_must_fix%'
      AND LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) = 'cmf_ok'
),
issue_agg AS (
    SELECT
        oem,
        COUNT(1) AS total_issues,
        SUM(CASE WHEN status_norm NOT IN ('complete', 'rejected', 'implemented') THEN 1 ELSE 0 END) AS open_issues,
        SUM(CASE WHEN status_norm = 'implemented' THEN 1 ELSE 0 END) AS implemented_issues,
        SUM(CASE WHEN status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS closed_issues,
        ISNULL(AVG(CAST(CASE
            WHEN decided_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= decided_date
            THEN DATEDIFF(DAY, decided_date, implemented_date)
            ELSE NULL END AS DECIMAL(10, 2))), 0) AS avg_resolution_days,
        SUM(CASE WHEN priority_norm IN ('p0-blocker', 'p1-showstopper') OR impact_norm IN ('1-critical', '2-high') THEN 1 ELSE 0 END) AS high_risk_signals,
        SUM(CASE WHEN ask_date >= DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) AS this_week,
        SUM(CASE WHEN ask_date >= DATEADD(DAY, -6 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE))
                  AND ask_date < DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)) THEN 1 ELSE 0 END) AS last_week
    FROM issue_base
    GROUP BY oem
),
pending_base AS (
    SELECT
        CASE WHEN ISNULL(LTRIM(RTRIM(customer_company)), '') = '' THEN 'Unassigned' ELSE LTRIM(RTRIM(customer_company)) END AS oem
    FROM " + overallComponentSourceSql + @"
    WHERE LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete')
      AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) NOT IN ('complete', 'rejected')
),
pending_agg AS (
    SELECT
        oem,
        COUNT(1) AS cmf_pending
    FROM pending_base
    GROUP BY oem
),
combined AS (
    SELECT
        COALESCE(i.oem, p.oem) AS oem,
        ISNULL(i.total_issues, 0) AS total_issues,
        ISNULL(i.open_issues, 0) AS open_issues,
        ISNULL(p.cmf_pending, 0) AS cmf_pending,
        ISNULL(i.implemented_issues, 0) AS implemented_issues,
        ISNULL(i.closed_issues, 0) AS closed_issues,
        ISNULL(i.avg_resolution_days, 0) AS avg_resolution_days,
        ISNULL(i.high_risk_signals, 0) AS high_risk_signals,
        ISNULL(i.this_week, 0) AS this_week,
        ISNULL(i.last_week, 0) AS last_week
    FROM issue_agg i
    FULL OUTER JOIN pending_agg p
      ON p.oem = i.oem
)
SELECT
    oem,
    total_issues,
    open_issues,
    high_risk_signals,
    cmf_pending,
    implemented_issues,
    closed_issues,
    CAST(ROUND(avg_resolution_days, 1) AS DECIMAL(10, 1)) AS avg_resolution_days,
    CASE
        WHEN high_risk_signals > 0 THEN 'High'
        WHEN total_issues > 0 AND (open_issues * 1.0 / NULLIF(total_issues, 0)) >= 0.35 THEN 'Medium'
        ELSE 'Low'
    END AS risk_level,
    CASE
        WHEN last_week = 0 THEN CASE WHEN this_week = 0 THEN 0 ELSE 100 END
        ELSE CAST(ROUND(((this_week - last_week) * 100.0) / NULLIF(last_week, 0), 0) AS INT)
    END AS trend_percent
FROM combined
ORDER BY total_issues DESC, oem";

        using (SqlCommand command = new SqlCommand(sql, connection))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                HomeDashboardOemRow row = new HomeDashboardOemRow();
                row.OemName = ReadString(reader, 0);
                row.TotalIssues = ReadInt(reader, 1);
                row.OpenIssues = ReadInt(reader, 2);
                row.HighRiskCmfs = ReadInt(reader, 3);
                row.CmfPending = ReadInt(reader, 4);
                row.Implemented = ReadInt(reader, 5);
                row.Closed = ReadInt(reader, 6);
                row.AvgResolutionDays = reader.IsDBNull(7) ? 0 : Convert.ToDecimal(reader.GetValue(7), CultureInfo.InvariantCulture);
                row.RiskLevel = ReadString(reader, 8);
                row.TrendPercent = ReadInt(reader, 9);
                snapshot.OemSummary.Add(row);
            }
        }
    }

    private void PopulateOverallComponentRiskHeatmap(HomeDashboardSnapshot snapshot, SqlConnection connection)
    {
        if (snapshot == null || connection == null)
        {
            return;
        }

        List<string> platformLabels = new List<string>();
        List<Dictionary<string, int>> platformIngredientCounts = new List<Dictionary<string, int>>();
        List<Dictionary<string, int>> platformIngredientLosCounts = new List<Dictionary<string, int>>();
        Dictionary<string, int> ingredientTotals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        List<string> sourceTables = GetOverallAggregateSourceTables();
        for (int index = 0; index < sourceTables.Count; index++)
        {
            string tableName = sourceTables[index];
            if (string.IsNullOrWhiteSpace(tableName) || !SqlTableExists(connection, tableName))
            {
                continue;
            }

            CmfPlatformDefinition platform;
            string label = CmfPlatformCatalog.TryGetPlatformByTable(tableName, out platform) && platform != null
                ? platform.Label
                : CmfPlatformCatalog.ResolvePlatformCode(tableName).Replace("_", "-");

            platformLabels.Add(label);

                        string sql = @"
SELECT
        CASE WHEN ISNULL(LTRIM(RTRIM(component_group)), '') = '' THEN 'Unassigned' ELSE LTRIM(RTRIM(component_group)) END AS ingredient,
        COUNT(1) AS open_unique_count,
        SUM(CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(los, '')))) = 'yes' THEN 1 ELSE 0 END) AS open_unique_los_count
FROM " + tableName + @"
WHERE status = 'open'
    AND cmf_request = 'cmf_ok'
    AND ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND sysdebug LIKE '%customer_must_fix%'
    AND " + GetDashboardDateFilter().BuildPredicate(null) + @"
GROUP BY
        CASE WHEN ISNULL(LTRIM(RTRIM(component_group)), '') = '' THEN 'Unassigned' ELSE LTRIM(RTRIM(component_group)) END";

            Dictionary<string, int> ingredientCountsForPlatform = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        Dictionary<string, int> ingredientLosCountsForPlatform = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string ingredient = ReadString(reader, 0);
                    if (string.IsNullOrWhiteSpace(ingredient))
                    {
                        ingredient = "Unassigned";
                    }

                    int count = ReadInt(reader, 1);
                    if (count <= 0)
                    {
                        continue;
                    }

                    int losCount = ReadInt(reader, 2);
                    if (losCount < 0)
                    {
                        losCount = 0;
                    }
                    if (losCount > count)
                    {
                        losCount = count;
                    }

                    int existingCount;
                    ingredientCountsForPlatform.TryGetValue(ingredient, out existingCount);
                    ingredientCountsForPlatform[ingredient] = existingCount + count;

                    int existingLosCount;
                    ingredientLosCountsForPlatform.TryGetValue(ingredient, out existingLosCount);
                    ingredientLosCountsForPlatform[ingredient] = existingLosCount + losCount;

                    int existingTotal;
                    ingredientTotals.TryGetValue(ingredient, out existingTotal);
                    ingredientTotals[ingredient] = existingTotal + count;
                }
            }

            platformIngredientCounts.Add(ingredientCountsForPlatform);
            platformIngredientLosCounts.Add(ingredientLosCountsForPlatform);
        }

        List<HomeDashboardHeatmapRow> heatmapRows = new List<HomeDashboardHeatmapRow>();
        List<string> orderedIngredients = ingredientTotals
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(item => item.Key)
            .ToList();

        for (int ingredientIndex = 0; ingredientIndex < orderedIngredients.Count; ingredientIndex++)
        {
            string ingredient = orderedIngredients[ingredientIndex];
            HomeDashboardHeatmapRow row = new HomeDashboardHeatmapRow
            {
                Ingredient = ingredient,
                Values = new List<int>(),
                StatusBreakdowns = new List<string>()
            };

            for (int platformIndex = 0; platformIndex < platformIngredientCounts.Count; platformIndex++)
            {
                Dictionary<string, int> platformCounts = platformIngredientCounts[platformIndex];
                int value;
                value = platformCounts.TryGetValue(ingredient, out value) ? value : 0;
                row.Values.Add(value);

                Dictionary<string, int> platformLosCounts = platformIngredientLosCounts[platformIndex];
                int losValue = platformLosCounts.TryGetValue(ingredient, out losValue) ? losValue : 0;
                row.StatusBreakdowns.Add(BuildHeatmapOpenUniqueTooltip(value, losValue));
            }

            heatmapRows.Add(row);
        }

        snapshot.HeatmapPlatforms = platformLabels;
        snapshot.ComponentRiskHeatmap = heatmapRows;
    }

    private static string BuildHeatmapOpenUniqueTooltip(int openUniqueCount, int losCount)
    {
        if (openUniqueCount <= 0)
        {
            return "No issues";
        }

        int normalizedLos = losCount;
        if (normalizedLos < 0)
        {
            normalizedLos = 0;
        }
        if (normalizedLos > openUniqueCount)
        {
            normalizedLos = openUniqueCount;
        }

        return "Open Unique: " + openUniqueCount.ToString(CultureInfo.InvariantCulture)
            + "\nLOS: " + normalizedLos.ToString(CultureInfo.InvariantCulture);
    }

    private static string BuildHeatmapStatusBreakdownTooltip(int total, Dictionary<string, int> statusCounts)
    {
        if (total <= 0)
        {
            return "No issues";
        }

        if (statusCounts == null || statusCounts.Count == 0)
        {
            return "Status unavailable";
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, int> item in statusCounts
            .Where(entry => entry.Value > 0)
            .OrderByDescending(entry => entry.Value)
            .ThenBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            parts.Add(
                ToHeatmapStatusLabel(item.Key)
                + ": "
                + item.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (parts.Count == 0)
        {
            return "Status unavailable";
        }

        return string.Join("\n", parts);
    }

    private static string ToHeatmapStatusLabel(string status)
    {
        string normalized = (status ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "Unknown";
        }

        normalized = normalized.Replace("_", " ").Replace("-", " ").ToLowerInvariant();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized);
    }

    private void PopulateOverallPendingAgeBuckets(HomeDashboardSnapshot snapshot, string dashboardPlatform, SqlConnection connection)
    {
        if (snapshot == null || connection == null)
        {
            return;
        }

        string pendingAgeCte = BuildPendingAskCteSql(connection, dashboardPlatform);
        string pendingSourceSql = string.IsNullOrWhiteSpace(pendingAgeCte) ? null
            : "(SELECT * FROM (" + pendingAgeCte + ") AS pending_dates WHERE "
                + GetDashboardDateFilter().BuildPredicate("pending_dates") + ") AS pending_age_src";
        if (string.IsNullOrWhiteSpace(pendingSourceSql))
        {
            string componentSourceSql = BuildDashboardComponentSourceSql(dashboardPlatform, true);
            if (!string.IsNullOrWhiteSpace(componentSourceSql))
            {
                pendingSourceSql = @"(
SELECT
    date_cmf_ask,
    status
FROM " + componentSourceSql + @"
WHERE cmf_request IN ('cmf_ask', 'cmf_incomplete')
) AS pending_age_src";
            }
        }

        if (string.IsNullOrWhiteSpace(pendingSourceSql))
        {
            return;
        }

        string sql = @"
SELECT
            SUM(CASE WHEN TRY_CAST(date_cmf_ask AS DATE) IS NOT NULL AND DATEDIFF(DAY, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) BETWEEN 0 AND 1 THEN 1 ELSE 0 END) AS age_0_1,
            SUM(CASE WHEN TRY_CAST(date_cmf_ask AS DATE) IS NOT NULL AND DATEDIFF(DAY, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) = 2 THEN 1 ELSE 0 END) AS age_1_2,
    SUM(CASE WHEN TRY_CAST(date_cmf_ask AS DATE) IS NOT NULL AND DATEDIFF(DAY, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) BETWEEN 3 AND 4 THEN 1 ELSE 0 END) AS age_2_4,
    SUM(CASE WHEN TRY_CAST(date_cmf_ask AS DATE) IS NOT NULL AND DATEDIFF(DAY, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) > 4 THEN 1 ELSE 0 END) AS age_over_4
FROM " + pendingSourceSql + @"
WHERE status NOT IN ('complete', 'rejected')";

        using (SqlCommand command = new SqlCommand(sql, connection))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                snapshot.PendingAgeBuckets.Add(new HomeDashboardCategoryPoint { Name = "0-1 Days", Value = ReadInt(reader, 0) });
                snapshot.PendingAgeBuckets.Add(new HomeDashboardCategoryPoint { Name = "1-2 Days", Value = ReadInt(reader, 1) });
                snapshot.PendingAgeBuckets.Add(new HomeDashboardCategoryPoint { Name = "2-4 Days", Value = ReadInt(reader, 2) });
                snapshot.PendingAgeBuckets.Add(new HomeDashboardCategoryPoint { Name = "> 4 Days", Value = ReadInt(reader, 3) });
            }
        }
    }

    private static string FormatWorkWeekLabel(DateTime date)
    {
        int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        return string.Format(CultureInfo.InvariantCulture, "WW'{0:D2}", weekNumber);
    }

    private static HomeDashboardTable NewDashboardTable(params string[] columns)
    {
        HomeDashboardTable table = new HomeDashboardTable();
        table.Columns = new List<string>(columns ?? new string[0]);
        table.Rows = new List<List<string>>();
        return table;
    }

    private static string BuildConstituentTooltipFromCounts(
        Dictionary<string, int> countsByIngredient,
        string emptyMessage)
    {
        if (countsByIngredient == null || countsByIngredient.Count == 0)
        {
            return string.IsNullOrWhiteSpace(emptyMessage) ? "No constituent counts available." : emptyMessage;
        }

        List<string> lines = countsByIngredient
            .Where(entry => entry.Value > 0)
            .OrderByDescending(entry => entry.Value)
            .ThenBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
            .Select(entry => (entry.Key ?? string.Empty).Trim() + ": " + entry.Value.ToString(CultureInfo.InvariantCulture))
            .ToList();

        if (lines.Count == 0)
        {
            return string.IsNullOrWhiteSpace(emptyMessage) ? "No constituent counts available." : emptyMessage;
        }

        return string.Join("\n", lines);
    }

    private static Dictionary<string, int> GetConstituentCountsByIngredient(
        SqlConnection connection,
        string componentSourceSql,
        string wherePredicate)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (connection == null || string.IsNullOrWhiteSpace(componentSourceSql) || string.IsNullOrWhiteSpace(wherePredicate))
        {
            return counts;
        }

        string sql = @"
SELECT
    CASE
        WHEN component_group IS NULL
          OR LTRIM(RTRIM(component_group)) = ''
          OR component_group = 'no iDST assigned'
        THEN 'Unassigned'
        ELSE component_group
    END AS ingredient,
    COUNT(*) AS item_count
FROM " + componentSourceSql + @"
WHERE sysdebug LIKE '%customer_must_fix%'
  AND cmf_request IN ('cmf_ok','cmf_duplicate')
  AND " + wherePredicate + @"
GROUP BY
    CASE
        WHEN component_group IS NULL
          OR LTRIM(RTRIM(component_group)) = ''
          OR component_group = 'no iDST assigned'
        THEN 'Unassigned'
        ELSE component_group
    END";

        using (SqlCommand command = new SqlCommand(sql, connection))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string ingredient = ReadString(reader, 0);
                int count = ReadInt(reader, 1);
                if (string.IsNullOrWhiteSpace(ingredient) || count <= 0)
                {
                    continue;
                }

                counts[ingredient.Trim()] = count;
            }
        }

        return counts;
    }

    private void PopulateHomeCmfSummaryTables(HomeDashboardSnapshot snapshot, string platformTable)
    {
        if (snapshot == null) return;
        platformTable = ResolveDashboardPlatformSelection(platformTable);
        bool isOverall = IsOverallDashboardPlatform(platformTable);
        string componentSourceSql = BuildDashboardComponentSourceSql(platformTable, true);
        string basePlatform = platformTable.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
        string pendingTable = basePlatform + "_CMF_ASK";
        decimal averageResolutionDays = 0m;
        int averageResolutionSampleSize = 0;

        using (SqlConnection con = new SqlConnection(ConnectionString))
        {
            con.Open();

            Dictionary<string, int> implementedConstituents = GetConstituentCountsByIngredient(
                con,
                componentSourceSql,
                "status = 'implemented'");
            Dictionary<string, int> openConstituents = GetConstituentCountsByIngredient(
                con,
                componentSourceSql,
                "status NOT IN ('complete','rejected','implemented')");

            using (SqlCommand summaryCommand = new SqlCommand(@"
WITH base AS (
    SELECT
        cmf_request,
        status,
        sysdebug,
        TRY_CAST(date_cmf_ask AS DATE) AS ask_date,
        TRY_CAST(date_cmf_decided AS DATE) AS decided_date,
        TRY_CAST(implemented_date AS DATE) AS implemented_date,
        TRY_CAST(days_active AS INT) AS days_active_int
    FROM " + componentSourceSql + @"
    WHERE sysdebug LIKE '%customer_must_fix%'
),
wk AS (
    SELECT DATEADD(DAY, 1 - DATEPART(WEEKDAY, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)) AS current_week_start
)
SELECT
    SUM(CASE WHEN cmf_request IN ('cmf_ok','cmf_duplicate') THEN 1 ELSE 0 END) AS TotalCount,
    SUM(CASE WHEN cmf_request = 'cmf_duplicate' THEN 1 ELSE 0 END) AS Duplicates,
    SUM(CASE WHEN cmf_request IN ('cmf_ok','cmf_duplicate') AND status IN ('complete','rejected') THEN 1 ELSE 0 END) AS ClosedCount,
    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND status IN ('complete','rejected') THEN 1 ELSE 0 END) AS ClosedDup,
    SUM(CASE WHEN cmf_request IN ('cmf_ok','cmf_duplicate') AND status = 'implemented' THEN 1 ELSE 0 END) AS ImplementedCount,
    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND status = 'implemented' THEN 1 ELSE 0 END) AS ImplementedDup,
    SUM(CASE WHEN cmf_request IN ('cmf_ok','cmf_duplicate') AND status NOT IN ('complete','rejected','implemented') THEN 1 ELSE 0 END) AS PendingCount,
    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND status NOT IN ('complete','rejected','implemented') THEN 1 ELSE 0 END) AS PendingDup,
    ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(ask_date, decided_date), decided_date) AS FLOAT)), 0) AS DispositionTpt,
    ISNULL(AVG(CAST(CASE WHEN implemented_date < decided_date THEN 0 ELSE DATEDIFF(DAY, ISNULL(decided_date, implemented_date), implemented_date) END AS FLOAT)), 0) AS ResolutionTpt,
    ISNULL(AVG(CASE WHEN cmf_request IN ('cmf_ok','cmf_duplicate') THEN CAST(days_active_int AS FLOAT) ELSE NULL END), 0) AS OverallTpt,
    ISNULL(AVG(CASE
        WHEN ask_date >= wk.current_week_start
        THEN CAST(DATEDIFF(DAY, ISNULL(ask_date, decided_date), decided_date) AS FLOAT)
        ELSE NULL END), 0) AS DispositionThisWeek,
    ISNULL(AVG(CASE
        WHEN ask_date >= DATEADD(DAY, -7, wk.current_week_start)
         AND ask_date < wk.current_week_start
        THEN CAST(DATEDIFF(DAY, ISNULL(ask_date, decided_date), decided_date) AS FLOAT)
        ELSE NULL END), 0) AS DispositionLastWeek,
    ISNULL(AVG(CASE
        WHEN ask_date >= wk.current_week_start
        THEN CAST(CASE WHEN implemented_date < decided_date THEN 0 ELSE DATEDIFF(DAY, ISNULL(decided_date, implemented_date), implemented_date) END AS FLOAT)
        ELSE NULL END), 0) AS ResolutionThisWeek,
    ISNULL(AVG(CASE
        WHEN ask_date >= DATEADD(DAY, -7, wk.current_week_start)
         AND ask_date < wk.current_week_start
        THEN CAST(CASE WHEN implemented_date < decided_date THEN 0 ELSE DATEDIFF(DAY, ISNULL(decided_date, implemented_date), implemented_date) END AS FLOAT)
        ELSE NULL END), 0) AS ResolutionLastWeek,
    ISNULL(AVG(CASE
        WHEN ask_date >= wk.current_week_start
         AND cmf_request IN ('cmf_ok','cmf_duplicate')
        THEN CAST(days_active_int AS FLOAT)
        ELSE NULL END), 0) AS OverallThisWeek,
    ISNULL(AVG(CASE
        WHEN ask_date >= DATEADD(DAY, -7, wk.current_week_start)
         AND ask_date < wk.current_week_start
         AND cmf_request IN ('cmf_ok','cmf_duplicate')
        THEN CAST(days_active_int AS FLOAT)
        ELSE NULL END), 0) AS OverallLastWeek,
    ISNULL(AVG(CASE
        WHEN cmf_request IN ('cmf_ok','cmf_duplicate')
         AND status IN ('complete','rejected','implemented','verified')
         AND decided_date IS NOT NULL
         AND implemented_date IS NOT NULL
         AND implemented_date >= decided_date
        THEN CAST(DATEDIFF(DAY, decided_date, implemented_date) AS FLOAT)
        ELSE NULL END), 0) AS SummaryAverageResolutionDays,
    SUM(CASE
        WHEN cmf_request IN ('cmf_ok','cmf_duplicate')
         AND status IN ('complete','rejected','implemented','verified')
         AND decided_date IS NOT NULL
         AND implemented_date IS NOT NULL
         AND implemented_date >= decided_date
        THEN 1 ELSE 0 END) AS SummaryAverageResolutionCount
FROM base
CROSS JOIN wk", con))
            using (SqlDataReader reader = summaryCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    int total = ReadInt(reader, 0);
                    int duplicates = ReadInt(reader, 1);
                    int closed = ReadInt(reader, 2);
                    int closedDup = ReadInt(reader, 3);
                    int implemented = ReadInt(reader, 4);
                    int implementedDup = ReadInt(reader, 5);
                    int pending = ReadInt(reader, 6);
                    int pendingDup = ReadInt(reader, 7);
                    int dispositionTpt = ReadLegacyAvgInt(reader, 8);
                    int resolutionTpt = ReadLegacyAvgInt(reader, 9);
                    int overallTpt = ReadLegacyAvgInt(reader, 10);
                    int dispositionThisWeek = ReadLegacyAvgInt(reader, 11);
                    int dispositionLastWeek = ReadLegacyAvgInt(reader, 12);
                    int resolutionThisWeek = ReadLegacyAvgInt(reader, 13);
                    int resolutionLastWeek = ReadLegacyAvgInt(reader, 14);
                    int overallThisWeek = ReadLegacyAvgInt(reader, 15);
                    int overallLastWeek = ReadLegacyAvgInt(reader, 16);
                    averageResolutionDays = reader.IsDBNull(17)
                        ? 0m
                        : Convert.ToDecimal(reader.GetValue(17), CultureInfo.InvariantCulture);
                    averageResolutionSampleSize = ReadInt(reader, 18);

                    int dispositionTrendDays = dispositionThisWeek - dispositionLastWeek;
                    int resolutionTrendDays = resolutionThisWeek - resolutionLastWeek;
                    int overallTrendDays = overallThisWeek - overallLastWeek;

                    snapshot.SummaryFacts.Add(new HomeDashboardFact { Label = "Total CMFs", Value = total.ToString(CultureInfo.InvariantCulture), Note = duplicates + " duplicates" });
                    snapshot.SummaryFacts.Add(new HomeDashboardFact { Label = "Closed", Value = closed.ToString(CultureInfo.InvariantCulture), Note = closedDup + " duplicates" });
                    snapshot.SummaryFacts.Add(new HomeDashboardFact
                    {
                        Label = "Implemented",
                        Value = implemented.ToString(CultureInfo.InvariantCulture),
                        Note = implementedDup + " duplicates",
                        ConstituentTooltip = BuildConstituentTooltipFromCounts(
                            implementedConstituents,
                            "No implemented constituent counts in this scope.")
                    });
                    snapshot.SummaryFacts.Add(new HomeDashboardFact
                    {
                        Label = "Open",
                        Value = pending.ToString(CultureInfo.InvariantCulture),
                        Note = pendingDup + " duplicates",
                        ConstituentTooltip = BuildConstituentTooltipFromCounts(
                            openConstituents,
                            "No open constituent counts in this scope.")
                    });
                    snapshot.TptFacts.Add(new HomeDashboardFact
                    {
                        Label = "CMF Disposition TPT",
                        Value = dispositionTpt.ToString(CultureInfo.InvariantCulture),
                        Note = "days",
                        TrendNote = FormatTptTrendNote(dispositionTrendDays)
                    });
                    snapshot.TptFacts.Add(new HomeDashboardFact
                    {
                        Label = "CMF Resolution TPT",
                        Value = resolutionTpt.ToString(CultureInfo.InvariantCulture),
                        Note = "days",
                        TrendNote = FormatTptTrendNote(resolutionTrendDays)
                    });
                    snapshot.TptFacts.Add(new HomeDashboardFact
                    {
                        Label = "CMF Overall TPT",
                        Value = overallTpt.ToString(CultureInfo.InvariantCulture),
                        Note = "days",
                        TrendNote = FormatTptTrendNote(overallTrendDays)
                    });
                }
            }

            using (SqlCommand milestoneCommand = new SqlCommand(@"
SELECT LTRIM(RTRIM(drivers)) AS Driver, COUNT(*) AS CMFCount
FROM " + componentSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND cmf_request IN ('cmf_ok')
  AND sysdebug LIKE '%customer_must_fix%'
GROUP BY LTRIM(RTRIM(drivers))
ORDER BY LTRIM(RTRIM(drivers))", con))
            using (SqlDataReader reader = milestoneCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    snapshot.MilestoneSummary.Rows.Add(new List<string> { ReadString(reader, 0), ReadInt(reader, 1).ToString(CultureInfo.InvariantCulture) });
                }
            }

            List<string> milestoneDrivers = new List<string>();
            using (SqlCommand driverListCommand = new SqlCommand(@"
SELECT DISTINCT LTRIM(RTRIM(drivers)) AS Driver
FROM " + componentSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND status IN ('open','implemented','verified')
  AND sysdebug LIKE '%customer_must_fix%'
  AND cmf_request NOT IN ('cmf_reject')
ORDER BY LTRIM(RTRIM(drivers))", con))
            using (SqlDataReader reader = driverListCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string driverName = ReadString(reader, 0);
                    if (!string.IsNullOrWhiteSpace(driverName))
                    {
                        milestoneDrivers.Add(driverName);
                    }
                }
            }

            snapshot.ComponentSummary.Columns = new List<string>();
            snapshot.ComponentSummary.Columns.Add("Ingredient");
            if (milestoneDrivers.Count == 0)
            {
                snapshot.ComponentSummary.Columns.Add("Open");
                snapshot.ComponentSummary.Columns.Add("Impl/Verified");
            }
            else
            {
                for (int index = 0; index < milestoneDrivers.Count; index++)
                {
                    string driverName = milestoneDrivers[index];
                    snapshot.ComponentSummary.Columns.Add(driverName + ":::Open");
                    snapshot.ComponentSummary.Columns.Add(driverName + ":::Impl/Verified");
                }
            }

            using (SqlCommand componentCommand = new SqlCommand(@"
SELECT
    CASE WHEN ISNULL(component_group, '') = '' THEN 'Unassigned' ELSE component_group END AS Component,
    LTRIM(RTRIM(drivers)) AS Driver,
    SUM(CASE WHEN status = 'open' AND cmf_request NOT IN ('cmf_duplicate','cmf_reject') THEN 1 ELSE 0 END) AS OpenUniqueCount,
    SUM(CASE WHEN status = 'open' AND cmf_request NOT IN ('cmf_duplicate','cmf_reject') AND los = 'Yes' THEN 1 ELSE 0 END) AS OpenUniqueLosCount,
    SUM(CASE WHEN status = 'open' AND cmf_request = 'cmf_duplicate' THEN 1 ELSE 0 END) AS OpenDuplicateCount,
    SUM(CASE WHEN status = 'open' AND cmf_request = 'cmf_duplicate' AND los = 'Yes' THEN 1 ELSE 0 END) AS OpenDuplicateLosCount,
    SUM(CASE WHEN status IN ('implemented','verified') AND cmf_request = 'cmf_ok' THEN 1 ELSE 0 END) AS ImplementedUniqueCount,
    SUM(CASE WHEN status IN ('implemented','verified') AND cmf_request = 'cmf_duplicate' THEN 1 ELSE 0 END) AS ImplementedDuplicateCount
FROM " + componentSourceSql + @"
WHERE status IN ('open','implemented','verified')
  AND sysdebug LIKE '%customer_must_fix%'
  AND cmf_request NOT IN ('cmf_reject')
  AND ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
GROUP BY
    CASE WHEN ISNULL(component_group, '') = '' THEN 'Unassigned' ELSE component_group END,
    LTRIM(RTRIM(drivers))
ORDER BY Component, Driver", con))
            using (SqlDataReader reader = componentCommand.ExecuteReader())
            {
                Dictionary<string, Dictionary<string, int[]>> componentDriverCounts =
                    new Dictionary<string, Dictionary<string, int[]>>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, int[]> totalsByDriver =
                    new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);

                while (reader.Read())
                {
                    string component = ReadString(reader, 0);
                    string driver = ReadString(reader, 1);
                    if (string.IsNullOrWhiteSpace(driver))
                    {
                        continue;
                    }

                    int[] counts = new int[6]
                    {
                        ReadInt(reader, 2),
                        ReadInt(reader, 3),
                        ReadInt(reader, 4),
                        ReadInt(reader, 5),
                        ReadInt(reader, 6),
                        ReadInt(reader, 7)
                    };

                    Dictionary<string, int[]> perDriver;
                    if (!componentDriverCounts.TryGetValue(component, out perDriver))
                    {
                        perDriver = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
                        componentDriverCounts[component] = perDriver;
                    }

                    perDriver[driver] = counts;

                    int[] totalCounts;
                    if (!totalsByDriver.TryGetValue(driver, out totalCounts))
                    {
                        totalCounts = new int[6];
                        totalsByDriver[driver] = totalCounts;
                    }

                    for (int valueIndex = 0; valueIndex < counts.Length; valueIndex++)
                    {
                        totalCounts[valueIndex] += counts[valueIndex];
                    }
                }

                List<string> orderedComponents = new List<string>(componentDriverCounts.Keys);
                orderedComponents.Sort(StringComparer.OrdinalIgnoreCase);

                for (int componentIndex = 0; componentIndex < orderedComponents.Count; componentIndex++)
                {
                    string component = orderedComponents[componentIndex];
                    Dictionary<string, int[]> perDriver = componentDriverCounts[component];
                    List<string> row = new List<string>();
                    row.Add(component);

                    for (int driverIndex = 0; driverIndex < milestoneDrivers.Count; driverIndex++)
                    {
                        string driver = milestoneDrivers[driverIndex];
                        int[] counts;
                        if (!perDriver.TryGetValue(driver, out counts))
                        {
                            counts = new int[6];
                        }

                        row.Add(
                            counts[0].ToString(CultureInfo.InvariantCulture) + "(" + counts[1].ToString(CultureInfo.InvariantCulture) + ")+" +
                            counts[2].ToString(CultureInfo.InvariantCulture) + "(" + counts[3].ToString(CultureInfo.InvariantCulture) + ")");
                        row.Add(
                            counts[4].ToString(CultureInfo.InvariantCulture) + "+" + counts[5].ToString(CultureInfo.InvariantCulture));
                    }

                    snapshot.ComponentSummary.Rows.Add(row);
                }

                if (milestoneDrivers.Count > 0)
                {
                    List<string> totalRow = new List<string>();
                    totalRow.Add("Total");
                    for (int driverIndex = 0; driverIndex < milestoneDrivers.Count; driverIndex++)
                    {
                        string driver = milestoneDrivers[driverIndex];
                        int[] counts;
                        if (!totalsByDriver.TryGetValue(driver, out counts))
                        {
                            counts = new int[6];
                        }

                        totalRow.Add(
                            counts[0].ToString(CultureInfo.InvariantCulture) + "(" + counts[1].ToString(CultureInfo.InvariantCulture) + ")+" +
                            counts[2].ToString(CultureInfo.InvariantCulture) + "(" + counts[3].ToString(CultureInfo.InvariantCulture) + ")");
                        totalRow.Add(
                            counts[4].ToString(CultureInfo.InvariantCulture) + "+" + counts[5].ToString(CultureInfo.InvariantCulture));
                    }

                    snapshot.ComponentSummary.Rows.Add(totalRow);
                }
            }

            string pendingSourceSql;
            if (isOverall)
            {
                pendingSourceSql = BuildOverallPendingSourceSql(con);
            }
            else if (SqlTableExists(con, pendingTable))
            {
                pendingSourceSql = "SELECT cp_id, status, component_group, date_cmf_ask, '" + platformTable + "' AS source_platform FROM " + pendingTable + " WHERE " + GetDashboardDateFilter().BuildPredicate(null);
            }
            else
            {
                pendingSourceSql = null;
            }

            string componentLookupSourceSql = "(" + BuildSummaryIssueComponentSourceSql(platformTable, false) + ") AS src_comp";
            if (!string.IsNullOrWhiteSpace(pendingSourceSql))
            {
                int pendingAgeThresholdDays = GetPendingSummaryAgeThresholdDays();
                Dictionary<string, List<string>> pendingOpenIdsByRowKey = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, List<string>> pendingOverTwoDaysIdsByRowKey = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                using (SqlCommand pendingIdsCommand = new SqlCommand(@"
WITH pending_rows AS (
    SELECT
        CASE WHEN pending.component_group IS NULL OR pending.component_group = '' OR pending.component_group = 'no iDST assigned' THEN 'Unassigned' ELSE pending.component_group END AS component_group,
        CASE
            WHEN src_comp.idst IS NULL OR LTRIM(RTRIM(src_comp.idst)) = '' THEN 'Unassigned'
            ELSE LTRIM(RTRIM(src_comp.idst))
        END AS idst_name,
        CAST(pending.cp_id AS VARCHAR(64)) AS cp_id,
        TRY_CAST(pending.date_cmf_ask AS DATE) AS date_cmf_ask
        FROM (" + pendingSourceSql + @") AS pending
    LEFT JOIN " + componentLookupSourceSql + @"
      ON CAST(pending.cp_id AS VARCHAR(64)) = CAST(src_comp.cp_id AS VARCHAR(64))
     AND pending.source_platform = src_comp.source_platform
    WHERE pending.status NOT IN ('complete', 'rejected')
)
SELECT component_group, idst_name, cp_id, date_cmf_ask
FROM pending_rows", con))
                using (SqlDataReader idsReader = pendingIdsCommand.ExecuteReader())
                {
                    Dictionary<string, HashSet<string>> pendingOpenIdsSets = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                    Dictionary<string, HashSet<string>> pendingOverTwoDaysIdsSets = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                    while (idsReader.Read())
                    {
                        string component = ReadString(idsReader, 0);
                        string idstName = ReadString(idsReader, 1);
                        string cpId = ReadString(idsReader, 2);
                        if (string.IsNullOrWhiteSpace(component) || string.IsNullOrWhiteSpace(cpId))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(idstName))
                        {
                            idstName = "Unassigned";
                        }

                        string rowKey = component.Trim() + "\u001f" + idstName.Trim();

                        HashSet<string> idSet;
                        if (!pendingOpenIdsSets.TryGetValue(rowKey, out idSet))
                        {
                            idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            pendingOpenIdsSets[rowKey] = idSet;
                        }

                        string normalizedCpId = cpId.Trim();
                        idSet.Add(normalizedCpId);

                        bool isOverTwoDays = pendingAgeThresholdDays <= 0;
                        if (!isOverTwoDays && !idsReader.IsDBNull(3))
                        {
                            DateTime askDate = idsReader.GetDateTime(3).Date;
                            isOverTwoDays = (DateTime.Today - askDate).TotalDays > pendingAgeThresholdDays;
                        }

                        if (isOverTwoDays)
                        {
                            HashSet<string> overTwoDaysSet;
                            if (!pendingOverTwoDaysIdsSets.TryGetValue(rowKey, out overTwoDaysSet))
                            {
                                overTwoDaysSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                pendingOverTwoDaysIdsSets[rowKey] = overTwoDaysSet;
                            }

                            overTwoDaysSet.Add(normalizedCpId);
                        }
                    }

                    foreach (KeyValuePair<string, HashSet<string>> entry in pendingOpenIdsSets)
                    {
                        List<string> orderedIds = new List<string>(entry.Value);
                        orderedIds.Sort(StringComparer.OrdinalIgnoreCase);
                        pendingOpenIdsByRowKey[entry.Key] = orderedIds;
                    }

                    foreach (KeyValuePair<string, HashSet<string>> entry in pendingOverTwoDaysIdsSets)
                    {
                        List<string> orderedIds = new List<string>(entry.Value);
                        orderedIds.Sort(StringComparer.OrdinalIgnoreCase);
                        pendingOverTwoDaysIdsByRowKey[entry.Key] = orderedIds;
                    }
                }

                using (SqlCommand pendingCommand = new SqlCommand(@"
WITH pending_rows AS (
    SELECT
        CASE WHEN pending.component_group IS NULL OR pending.component_group = '' OR pending.component_group = 'no iDST assigned' THEN 'Unassigned' ELSE pending.component_group END AS component_group,
        CASE
            WHEN src_comp.idst IS NULL OR LTRIM(RTRIM(src_comp.idst)) = '' THEN 'Unassigned'
            ELSE LTRIM(RTRIM(src_comp.idst))
        END AS idst_name,
        CAST(pending.cp_id AS VARCHAR(64)) AS cp_id,
        TRY_CAST(pending.date_cmf_ask AS DATE) AS date_cmf_ask
        FROM (" + pendingSourceSql + @") AS pending
    LEFT JOIN " + componentLookupSourceSql + @"
      ON CAST(pending.cp_id AS VARCHAR(64)) = CAST(src_comp.cp_id AS VARCHAR(64))
     AND pending.source_platform = src_comp.source_platform
    WHERE pending.status NOT IN ('complete', 'rejected')
)
SELECT
    component_group,
    idst_name,
    COUNT(cp_id) AS pending_count,
    SUM(CASE
    WHEN @pending_age_threshold_days <= 0 THEN 1
    WHEN TRY_CAST(date_cmf_ask AS DATE) IS NOT NULL
         AND DATEDIFF(DAY, TRY_CAST(date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) > @pending_age_threshold_days
        THEN 1 ELSE 0 END) AS pending_over_two_days
FROM pending_rows
GROUP BY component_group, idst_name
ORDER BY component_group, idst_name", con))
                {
                    pendingCommand.Parameters.Add("@pending_age_threshold_days", SqlDbType.Int).Value = pendingAgeThresholdDays;
                    using (SqlDataReader reader = pendingCommand.ExecuteReader())
                    {
                    int totalPending = 0;
                    List<string> pendingConstituentLines = new List<string>();
                    Dictionary<string, int> pendingCountsByComponent = isOverall
                        ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                        : null;
                    while (reader.Read())
                    {
                        string component = ReadString(reader, 0);
                        string idstName = ReadString(reader, 1);
                        int count = ReadInt(reader, 2);
                        int overTwoDaysCount = ReadInt(reader, 3);
                        totalPending += count;

                        if (string.IsNullOrWhiteSpace(idstName))
                        {
                            idstName = "Unassigned";
                        }

                        string rowKey = component.Trim() + "\u001f" + idstName.Trim();

                        List<string> openIds;
                        List<string> overTwoDaysIds;
                        string pendingCellValue = count.ToString(CultureInfo.InvariantCulture);
                        pendingCellValue = pendingCellValue
                            + PendingSummaryOverTwoDaysDelimiter
                            + overTwoDaysCount.ToString(CultureInfo.InvariantCulture);
                        if (pendingOverTwoDaysIdsByRowKey.TryGetValue(rowKey, out overTwoDaysIds) && overTwoDaysIds != null && overTwoDaysIds.Count > 0)
                        {
                            pendingCellValue = pendingCellValue + PendingSummaryOverTwoDaysIdsDelimiter + string.Join(",", overTwoDaysIds.ToArray());
                        }
                        if (pendingOpenIdsByRowKey.TryGetValue(rowKey, out openIds) && openIds != null && openIds.Count > 0)
                        {
                            pendingCellValue = pendingCellValue + PendingSummaryLinksDelimiter + string.Join(",", openIds.ToArray());
                        }

                        snapshot.PendingSummary.Rows.Add(new List<string> { component, idstName, pendingCellValue });

                        if (isOverall)
                        {
                            int existingCount;
                            if (!pendingCountsByComponent.TryGetValue(component, out existingCount))
                            {
                                existingCount = 0;
                            }

                            pendingCountsByComponent[component] = existingCount + count;
                        }
                        else
                        {
                            pendingConstituentLines.Add(component + " / " + idstName + ": " + count.ToString(CultureInfo.InvariantCulture));
                        }
                    }

                    if (isOverall && pendingCountsByComponent != null && pendingCountsByComponent.Count > 0)
                    {
                        List<string> orderedComponents = new List<string>(pendingCountsByComponent.Keys);
                        orderedComponents.Sort(StringComparer.OrdinalIgnoreCase);

                        for (int componentIndex = 0; componentIndex < orderedComponents.Count; componentIndex++)
                        {
                            string componentName = orderedComponents[componentIndex];
                            pendingConstituentLines.Add(
                                componentName + ": " + pendingCountsByComponent[componentName].ToString(CultureInfo.InvariantCulture));
                        }
                    }

                    snapshot.PendingIssues = totalPending;
                    snapshot.SummaryFacts.Add(new HomeDashboardFact
                    {
                        Label = "Pending",
                        Value = totalPending.ToString(CultureInfo.InvariantCulture),
                        Note = "CMF pending table",
                        ConstituentTooltip = pendingConstituentLines.Count > 0
                            ? string.Join("\n", pendingConstituentLines.ToArray())
                            : "No pending constituent counts in this scope."
                    });
                    snapshot.PendingSummary.Rows.Add(new List<string> { "Total", string.Empty, totalPending.ToString(CultureInfo.InvariantCulture) });
                    }
                }
            }

            PopulateHomeDesignSummaryTable(snapshot, platformTable, con);

            snapshot.SummaryFacts.Add(new HomeDashboardFact
            {
                Label = "Avg Resolution",
                Value = averageResolutionDays.ToString("0.0", CultureInfo.InvariantCulture),
                Note = "days | " + averageResolutionSampleSize.ToString(CultureInfo.InvariantCulture) + " resolved CMFs"
            });
        }
    }

    private static string FormatDashboardPercentageWithCounts(int numerator, int denominator)
    {
        if (denominator <= 0)
        {
            return "N/A";
        }

        decimal ratio = Math.Round((numerator * 100m) / denominator, 1, MidpointRounding.AwayFromZero);
        return ratio.ToString("0.#", CultureInfo.InvariantCulture)
            + "% ("
            + numerator.ToString(CultureInfo.InvariantCulture)
            + "/"
            + denominator.ToString(CultureInfo.InvariantCulture)
            + ")";
    }

    private void PopulateHomeDesignSummaryTable(HomeDashboardSnapshot snapshot, string platformTable, SqlConnection connection)
    {
        if (snapshot == null || connection == null)
        {
            return;
        }

        if (IsOverallDashboardPlatform(platformTable))
        {
            return;
        }

        string resolvedPlatformTable = ResolvePlatformTable(platformTable);
        if (string.IsNullOrWhiteSpace(resolvedPlatformTable) || !SqlTableExists(connection, resolvedPlatformTable))
        {
            return;
        }

        string basePlatform = resolvedPlatformTable.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
        string designTable = basePlatform + "_DESIGN_TABLE";
        if (!SqlTableExists(connection, designTable))
        {
            return;
        }

        // Legacy parity: fixed base columns + dynamic driver columns + Impl/Verified.
        List<string> dynamicDrivers = new List<string>();
        Dictionary<string, string> dynamicDriverMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        string datedPlatformSource = FilterDashboardTableSource(resolvedPlatformTable);
        string dynamicDriverSql = @"
SELECT DISTINCT LTRIM(RTRIM(ISNULL(drivers, ''))) AS driver_name
    FROM " + datedPlatformSource + @" AS design_drivers
WHERE LOWER(LTRIM(RTRIM(ISNULL(sysdebug, '')))) LIKE '%customer_must_fix%'
  AND LOWER(LTRIM(RTRIM(ISNULL(status, '')))) = 'open'
  AND LOWER(LTRIM(RTRIM(ISNULL(cmf_request, '')))) <> 'cmf_reject'
ORDER BY LTRIM(RTRIM(ISNULL(drivers, '')))";

        using (SqlCommand driverCommand = new SqlCommand(dynamicDriverSql, connection))
        using (SqlDataReader driverReader = driverCommand.ExecuteReader())
        {
            while (driverReader.Read())
            {
                string driverName = ReadString(driverReader, 0).Trim();
                if (string.IsNullOrWhiteSpace(driverName))
                {
                    continue;
                }

                if (!dynamicDriverMap.ContainsKey(driverName))
                {
                    dynamicDriverMap[driverName] = driverName;
                    dynamicDrivers.Add(driverName);
                }
            }
        }

        snapshot.DesignSummary.Columns = new List<string>();
        snapshot.DesignSummary.Columns.Add("Design Name");
        snapshot.DesignSummary.Columns.Add("SW Image Freeze");
        snapshot.DesignSummary.Columns.Add("Support Model");
        for (int i = 0; i < dynamicDrivers.Count; i++)
        {
            snapshot.DesignSummary.Columns.Add(dynamicDrivers[i]);
        }
        snapshot.DesignSummary.Columns.Add("Impl/Verified");

        string summarySql = @"
SELECT
    CASE WHEN A.customer_detail IS NULL OR LTRIM(RTRIM(A.customer_detail)) = '' THEN 'unassigned' ELSE LTRIM(RTRIM(A.customer_detail)) END AS design_name,
    ISNULL(NULLIF(LTRIM(RTRIM(B.sw_image_freeze)), ''), 'NA') AS sw_image_freeze,
    ISNULL(NULLIF(LTRIM(RTRIM(B.support_model)), ''), '') AS support_model,
    LOWER(LTRIM(RTRIM(ISNULL(A.status, '')))) AS status_norm,
    LTRIM(RTRIM(ISNULL(A.drivers, ''))) AS driver_name
FROM " + datedPlatformSource + @" A
INNER JOIN " + designTable + @" B ON A.customer_detail = B.customer_detail
WHERE LOWER(LTRIM(RTRIM(ISNULL(A.sysdebug, '')))) LIKE '%customer_must_fix%'
  AND LOWER(LTRIM(RTRIM(ISNULL(A.status, '')))) IN ('open', 'implemented', 'verified')
  AND LOWER(LTRIM(RTRIM(ISNULL(A.cmf_request, '')))) <> 'cmf_reject'";

        Dictionary<string, string> designNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> swImageFreezeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> supportModelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, Dictionary<string, int>> designOpenByDriver = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> designImplVerifiedCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        using (SqlCommand summaryCommand = new SqlCommand(summarySql, connection))
        using (SqlDataReader summaryReader = summaryCommand.ExecuteReader())
        {
            while (summaryReader.Read())
            {
                string designName = ReadString(summaryReader, 0);
                string swImageFreeze = ReadString(summaryReader, 1);
                string supportModel = ReadString(summaryReader, 2);
                string statusNorm = ReadString(summaryReader, 3);
                string driverName = ReadString(summaryReader, 4).Trim();

                if (string.IsNullOrWhiteSpace(designName))
                {
                    designName = "unassigned";
                }

                designNameMap[designName] = designName;
                swImageFreezeMap[designName] = swImageFreeze;
                supportModelMap[designName] = supportModel;

                if (!designImplVerifiedCount.ContainsKey(designName))
                {
                    designImplVerifiedCount[designName] = 0;
                }

                if (!designOpenByDriver.ContainsKey(designName))
                {
                    designOpenByDriver[designName] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                }

                if (string.Equals(statusNorm, "open", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(driverName)
                    && dynamicDriverMap.ContainsKey(driverName))
                {
                    Dictionary<string, int> designDriverOpenCounts = designOpenByDriver[designName];
                    if (!designDriverOpenCounts.ContainsKey(driverName))
                    {
                        designDriverOpenCounts[driverName] = 0;
                    }

                    designDriverOpenCounts[driverName] += 1;
                }
                else if (string.Equals(statusNorm, "implemented", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(statusNorm, "verified", StringComparison.OrdinalIgnoreCase))
                {
                    designImplVerifiedCount[designName] += 1;
                }
            }
        }

        List<string> orderedDesigns = new List<string>(designNameMap.Keys);
        orderedDesigns.Sort(delegate (string left, string right)
        {
            int leftImpl = designImplVerifiedCount.ContainsKey(left) ? designImplVerifiedCount[left] : 0;
            int rightImpl = designImplVerifiedCount.ContainsKey(right) ? designImplVerifiedCount[right] : 0;
            int implCompare = rightImpl.CompareTo(leftImpl);
            return implCompare != 0
                ? implCompare
                : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        });

        int totalImplVerified = 0;
        Dictionary<string, int> totalOpenByDriver = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < dynamicDrivers.Count; i++)
        {
            totalOpenByDriver[dynamicDrivers[i]] = 0;
        }

        for (int d = 0; d < orderedDesigns.Count; d++)
        {
            string design = orderedDesigns[d];
            List<string> row = new List<string>();
            row.Add(designNameMap.ContainsKey(design) ? designNameMap[design] : design);
            row.Add(swImageFreezeMap.ContainsKey(design) ? swImageFreezeMap[design] : "NA");
            row.Add(supportModelMap.ContainsKey(design) ? supportModelMap[design] : string.Empty);

            Dictionary<string, int> perDriver = designOpenByDriver.ContainsKey(design)
                ? designOpenByDriver[design]
                : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int driverIndex = 0; driverIndex < dynamicDrivers.Count; driverIndex++)
            {
                string driver = dynamicDrivers[driverIndex];
                int count = perDriver.ContainsKey(driver) ? perDriver[driver] : 0;
                row.Add(count.ToString(CultureInfo.InvariantCulture));
                totalOpenByDriver[driver] += count;
            }

            int implCount = designImplVerifiedCount.ContainsKey(design) ? designImplVerifiedCount[design] : 0;
            row.Add(implCount.ToString(CultureInfo.InvariantCulture));
            totalImplVerified += implCount;

            snapshot.DesignSummary.Rows.Add(row);
        }

        if (snapshot.DesignSummary.Rows.Count > 0)
        {
            List<string> totalRow = new List<string>();
            totalRow.Add("Total");
            totalRow.Add(string.Empty);
            totalRow.Add(string.Empty);

            for (int driverIndex = 0; driverIndex < dynamicDrivers.Count; driverIndex++)
            {
                string driver = dynamicDrivers[driverIndex];
                int driverTotal = totalOpenByDriver.ContainsKey(driver) ? totalOpenByDriver[driver] : 0;
                totalRow.Add(driverTotal.ToString(CultureInfo.InvariantCulture));
            }

            totalRow.Add(totalImplVerified.ToString(CultureInfo.InvariantCulture));
            snapshot.DesignSummary.Rows.Add(totalRow);
        }
    }

    private string SaveDesignSummaryInlineEdit(string designName, string swImageFreeze, string supportModel)
    {
        string trimmedDesignName = (designName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedDesignName) || string.Equals(trimmedDesignName, "Total", StringComparison.OrdinalIgnoreCase))
        {
            return "Unable to save design summary row.";
        }

        string dashboardPlatform = ResolveDashboardPlatformSelection();
        if (IsOverallDashboardPlatform(dashboardPlatform))
        {
            return "Inline edit is available only in platform-specific dashboard view.";
        }

        string resolvedPlatformTable = ResolvePlatformTable(dashboardPlatform);
        if (string.IsNullOrWhiteSpace(resolvedPlatformTable))
        {
            return "Unable to resolve active platform for inline edit.";
        }

        string basePlatform = resolvedPlatformTable.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
        string designTable = basePlatform + "_DESIGN_TABLE";

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            if (!SqlTableExists(connection, designTable))
            {
                return "Design metadata table is unavailable for the selected platform.";
            }

            const string updateSql = "UPDATE {0} SET sw_image_freeze = @SWImageFreeze, support_model = @SupportModel WHERE customer_detail = @Design";
            string finalSql = string.Format(CultureInfo.InvariantCulture, updateSql, designTable);

            using (SqlCommand command = new SqlCommand(finalSql, connection))
            {
                command.Parameters.AddWithValue("@SWImageFreeze", (object)(swImageFreeze ?? string.Empty));
                command.Parameters.AddWithValue("@SupportModel", (object)(supportModel ?? string.Empty));
                command.Parameters.AddWithValue("@Design", trimmedDesignName);

                int affectedRows = command.ExecuteNonQuery();
                if (affectedRows <= 0)
                {
                    return "No matching design row found to update.";
                }
            }
        }

        return "Design summary updated.";
    }

    private void PopulateMilestoneWiseComponentSummary(HomeDashboardSnapshot snapshot, SqlConnection con, string platformTable)
    {
        if (snapshot == null || con == null) return;

        List<string> drivers = new List<string>();
        using (SqlCommand driverCommand = new SqlCommand(@"
SELECT DISTINCT LTRIM(RTRIM(drivers)) AS Driver
FROM " + platformTable + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND status IN ('open','implemented','verified')
  AND sysdebug LIKE '%customer_must_fix%'
  AND cmf_request NOT IN ('cmf_reject')
ORDER BY LTRIM(RTRIM(drivers))", con))
        using (SqlDataReader reader = driverCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                string driver = ReadString(reader, 0);
                if (!string.IsNullOrWhiteSpace(driver)) drivers.Add(driver);
            }
        }

        if (drivers.Count == 0)
        {
            snapshot.ComponentSummary = NewDashboardTable("Component", "Open (LOS)", "Impl/Verified");
            return;
        }

        List<string> columns = new List<string>();
        columns.Add("Component");
        foreach (string driver in drivers)
        {
            columns.Add(driver + " Open (LOS)");
            columns.Add(driver + " Impl/Verified");
        }

        snapshot.ComponentSummary = NewDashboardTable(columns.ToArray());

        StringBuilder query = new StringBuilder();
        query.Append("SELECT Component");
        for (int index = 0; index < drivers.Count; index++)
        {
            query.Append(", SUM(CASE WHEN drivers = @driver").Append(index).Append(" AND status = 'open' AND cmf_request NOT IN ('cmf_duplicate','cmf_reject') THEN 1 ELSE 0 END) AS OpenCount").Append(index);
            query.Append(", SUM(CASE WHEN drivers = @driver").Append(index).Append(" AND status = 'open' AND los = 'Yes' THEN 1 ELSE 0 END) AS LosCount").Append(index);
            query.Append(", SUM(CASE WHEN drivers = @driver").Append(index).Append(" AND status = 'open' AND cmf_request = 'cmf_duplicate' THEN 1 ELSE 0 END) AS DuplicateCount").Append(index);
            query.Append(", SUM(CASE WHEN drivers = @driver").Append(index).Append(" AND status IN ('implemented','verified') AND cmf_request = 'cmf_ok' THEN 1 ELSE 0 END) AS ImplementedCount").Append(index);
        }
        query.Append(" FROM (SELECT CASE WHEN ISNULL(component_group, '') = '' THEN 'Unassigned' ELSE component_group END AS Component, LTRIM(RTRIM(drivers)) AS drivers, status, cmf_request, los FROM ").Append(platformTable).Append(" WHERE status IN ('open','implemented','verified') AND sysdebug LIKE '%customer_must_fix%' AND cmf_request NOT IN ('cmf_reject')) AS SourceRows GROUP BY Component ORDER BY Component");

        int[] totalOpen = new int[drivers.Count];
        int[] totalLos = new int[drivers.Count];
        int[] totalDuplicates = new int[drivers.Count];
        int[] totalImplemented = new int[drivers.Count];

        using (SqlCommand componentCommand = new SqlCommand(query.ToString(), con))
        {
            for (int index = 0; index < drivers.Count; index++)
            {
                componentCommand.Parameters.AddWithValue("@driver" + index.ToString(CultureInfo.InvariantCulture), drivers[index]);
            }

            using (SqlDataReader reader = componentCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> row = new List<string>();
                    row.Add(ReadString(reader, 0));
                    for (int index = 0; index < drivers.Count; index++)
                    {
                        int baseIndex = 1 + (index * 4);
                        int open = ReadInt(reader, baseIndex);
                        int los = ReadInt(reader, baseIndex + 1);
                        int duplicates = ReadInt(reader, baseIndex + 2);
                        int implemented = ReadInt(reader, baseIndex + 3);
                        totalOpen[index] += open;
                        totalLos[index] += los;
                        totalDuplicates[index] += duplicates;
                        totalImplemented[index] += implemented;
                        row.Add(open.ToString(CultureInfo.InvariantCulture) + "(" + los.ToString(CultureInfo.InvariantCulture) + ")");
                        row.Add(implemented.ToString(CultureInfo.InvariantCulture));
                    }
                    snapshot.ComponentSummary.Rows.Add(row);
                }
            }
        }

        List<string> totalRow = new List<string>();
        totalRow.Add("Total (LOS) + Duplicates + Implemented");
        int allOpen = 0;
        int allImplemented = 0;
        for (int index = 0; index < drivers.Count; index++)
        {
            allOpen += totalOpen[index];
            allImplemented += totalImplemented[index];
            totalRow.Add(totalOpen[index].ToString(CultureInfo.InvariantCulture) + "(" + totalLos[index].ToString(CultureInfo.InvariantCulture) + ") + " + totalDuplicates[index].ToString(CultureInfo.InvariantCulture) + " Dups");
            totalRow.Add(totalImplemented[index].ToString(CultureInfo.InvariantCulture));
        }
        snapshot.ComponentSummary.Rows.Add(totalRow);
        SetHomeDashboardFact(snapshot.SummaryFacts, "Open", allOpen.ToString(CultureInfo.InvariantCulture), "from component summary");
        SetHomeDashboardFact(snapshot.SummaryFacts, "Implemented", allImplemented.ToString(CultureInfo.InvariantCulture), "from component summary");
    }

    private static void SetHomeDashboardFact(List<HomeDashboardFact> facts, string label, string value, string note)
    {
        if (facts == null) return;
        HomeDashboardFact fact = facts.FirstOrDefault(item => string.Equals(item.Label, label, StringComparison.OrdinalIgnoreCase));
        if (fact == null)
        {
            facts.Add(new HomeDashboardFact { Label = label, Value = value, Note = note });
            return;
        }

        fact.Value = value;
        fact.Note = note;
    }

    private static string FormatTptTrendNote(int deltaDays)
    {
        int magnitude = Math.Abs(deltaDays);
        string unit = magnitude == 1 ? "day" : "days";

        if (deltaDays > 0)
        {
            return "+" + magnitude.ToString(CultureInfo.InvariantCulture) + " " + unit + " vs last week";
        }

        if (deltaDays < 0)
        {
            return "-" + magnitude.ToString(CultureInfo.InvariantCulture) + " " + unit + " vs last week";
        }

        return "0 days vs last week";
    }

    private void PopulateHomePortalHealth(HomeDashboardSnapshot snapshot, HomeDashboardCategoryPoint milestoneRiskConcentration, HomeDashboardCategoryPoint predictedTopRisk)
    {
        if (snapshot == null) return;
        int overallTpt = 0;
        if (snapshot.TptFacts != null && snapshot.TptFacts.Count >= 3)
        {
            int.TryParse(snapshot.TptFacts[2].Value, out overallTpt);
        }

        int score = 100;
        score -= Math.Min(35, snapshot.NeedsAttention * 4);
        score -= Math.Min(20, snapshot.ActiveIssues * 2);
        score -= Math.Min(25, Math.Max(0, overallTpt - 30) / 2);
        score += Math.Min(10, snapshot.ResolvedThisWeek * 2);
        snapshot.ProgramReadinessScore = Math.Max(0, Math.Min(100, score));
        snapshot.ProgramRiskLevel = snapshot.ProgramReadinessScore >= 85 ? "Low Risk" : (snapshot.ProgramReadinessScore >= 70 ? "Moderate Risk" : "High Risk");
        snapshot.TopRisk = predictedTopRisk != null && !string.IsNullOrWhiteSpace(predictedTopRisk.Name)
            ? predictedTopRisk.Name
            : "No priority or impact risk detected";

        int topValue = milestoneRiskConcentration != null ? milestoneRiskConcentration.Value : 0;
        int active = Math.Max(1, snapshot.ActiveIssues);
        snapshot.RiskConcentration = topValue > 0 && milestoneRiskConcentration != null
            ? milestoneRiskConcentration.Name + " (" + Math.Round((topValue * 100.0) / active).ToString("0", CultureInfo.InvariantCulture) + "%)"
            : "No milestone concentration";

        if (snapshot.PredictedBlockers.Count == 0)
        {
            snapshot.PredictedBlockers.Add("No high-risk issue blockers detected");
        }
    }

    private static bool SqlTableExists(SqlConnection connection, string tableName)
    {
        using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName", connection))
        {
            command.Parameters.AddWithValue("@TableName", tableName);
            return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
        }
    }

    private static bool SqlTableHasRows(SqlConnection connection, string tableName)
    {
        using (SqlCommand command = new SqlCommand("SELECT TOP 1 1 FROM " + tableName, connection))
        {
            object result = command.ExecuteScalar();
            return result != null && result != DBNull.Value;
        }
    }

    private static int ReadInt(SqlDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? 0 : Convert.ToInt32(reader.GetValue(index), CultureInfo.InvariantCulture);
    }

    private static int ReadLegacyAvgInt(SqlDataReader reader, int index)
    {
        if (reader.IsDBNull(index))
        {
            return 0;
        }

        decimal numeric = Convert.ToDecimal(reader.GetValue(index), CultureInfo.InvariantCulture);
        if (numeric > int.MaxValue)
        {
            return int.MaxValue;
        }

        if (numeric < int.MinValue)
        {
            return int.MinValue;
        }

        return decimal.ToInt32(decimal.Truncate(numeric));
    }

    private static string ReadString(SqlDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : Convert.ToString(reader.GetValue(index), CultureInfo.InvariantCulture);
    }

    private string BuildPlatformDisplayName(string platformTable)
    {
        if (string.IsNullOrWhiteSpace(platformTable))
        {
            return "Current Platform";
        }

        if (IsOverallDashboardPlatform(platformTable))
        {
            return "Overall Dashboard";
        }

        string display = platformTable.Replace("CMF_", string.Empty)
            .Replace("_ALL_COMPONENTS_TABLE", string.Empty)
            .Replace("_", " ");

        return display.Trim() + " Dashboard";
    }

    protected void ddlUserMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlUserMode != null)
        {
            Session[UserModeSessionKey] = ddlUserMode.SelectedValue;
        }

        ApplyFocusedPortalMode();
        if (IsAdminMode())
        {
            btnShowGridView9_Click(sender, e);
            return;
        }

        ShowWelcomeHome();
        SetActiveFocusedTab("home");
    }

    protected void btnShowHomeDashboard_Click(object sender, EventArgs e)
    {
        ShowWelcomeHome();
        SetActiveFocusedTab("home");
    }

    protected void btnShowGridView1_Click(object sender, EventArgs e)
    {
        ApplyIssuePendingPlatformContext();

        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = false;
        }

        searchfilters.Visible = false;
        analyticsPanel.Visible = false;
        SetMainDataWrapperVisible(true);
        // Show GridView1 and hide GridView2
        overall_request_details.Visible = true;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        btnExportToExcel.Visible = true;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = true;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        pane8.Visible = false;
        fieldSelectorPanel.Visible = true;
        issueListHeaderPanel.Visible = true;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(true);
        configRulesPanel.Visible = false;
        reportsPlaceholderPanel.Visible = false;
        // Show or hide the div based on your condition
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        BindAllFilters();
        BindGridView(null, null, bindRelatedGrids: false);
        if (lnkPlatformDashboardPending != null)
        {
            lnkPlatformDashboardPending.Visible = false;
        }
        UpdatePlatformDashboardLink();
        SetActiveFocusedTab("issue");
        InitializeSharedFilterPanel();
        //RegisterFieldSelectorScript();
        ScriptManager.RegisterStartupScript(this, GetType(), "initColHideButtons",
            "setTimeout(function(){ if(typeof initColumnHideButtons==='function') initColumnHideButtons(); }, 300);", true);
    }

    protected void btnShowGridView2_Click(object sender, EventArgs e)
    {
        searchfilters.Visible = false;
        analyticsPanel.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = true;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = true;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = true;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = true;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        pane9.Visible = false;
        fieldSelectorPanel.Visible = false;
        issueListHeaderPanel.Visible = false;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(false);
    }

    protected void btnShowGridView3_Click(object sender, EventArgs e)
    {
        ShowModernCmfSummary();
        SetActiveFocusedTab("cmf");
    }

    protected void btnShowGridView4_Click(object sender, EventArgs e)
    {
        ApplyIssuePendingPlatformContext();

        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = false;
        }

        searchfilters.Visible = false;
        SetMainDataWrapperVisible(true);
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        analyticsPanel.Visible = false;
        btnExportToExcel_cmf_pending.Visible = true;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_cmf_pending.Visible = true;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        btnImportPopup.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = true;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        pane8.Visible = false;
        pane9.Visible = false;
        fieldSelectorPanel.Visible = false;
        issueListHeaderPanel.Visible = false;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(false);
        configRulesPanel.Visible = false;
        reportsPlaceholderPanel.Visible = false;
        BindGridView_cmf_pending();
        UpdateCmfPendingKpis();
        if (lnkPlatformDashboard != null)
        {
            lnkPlatformDashboard.Visible = false;
        }
        UpdateCmfPendingAccessibilityLinks();
        SetActiveFocusedTab("pending");
        InitializeSharedFilterPanel();
    }
    protected void btnShowGridView5_Click(object sender, EventArgs e)
    {
        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        searchfilters.Visible = false;
        analyticsPanel.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = true;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = true;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = true;
        pane6.Visible = false;
        pane7.Visible = false;
        pane9.Visible = false;
        fieldSelectorPanel.Visible = false;
        SetIssuePagerVisible(false);
    }
    protected void btnShowGridView6_Click(object sender, EventArgs e)
    {
        searchfilters.Visible = false;
        analyticsPanel.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = true;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = true;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = true;
        pane7.Visible = false;
        pane9.Visible = false;
        fieldSelectorPanel.Visible = false;
        SetIssuePagerVisible(false);
    }

    protected void btnShowGridView7_Click(object sender, EventArgs e)
    {
        searchfilters.Visible = false;
        analyticsPanel.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = true;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = true;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = true;
        pane9.Visible = false;
        fieldSelectorPanel.Visible = false;
        SetIssuePagerVisible(false);
        BindGridView_oem_summary();
    }

    // ================================================================
    //  REPORTS & ANALYTICS TAB
    // ================================================================

    protected void btnShowGridView8_Click(object sender, EventArgs e)
    {
        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        searchfilters.Visible = false;
        SetMainDataWrapperVisible(false);
        // Hide all data grids
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        btnImportPopup.Visible = false;
        // Hide all export buttons
        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        // Pane headings
        pane1.Visible = false; pane2.Visible = false; pane3.Visible = false;
        pane4.Visible = false; pane5.Visible = false; pane6.Visible = false;
        pane7.Visible = false; pane8.Visible = true; pane9.Visible = false;
        fieldSelectorPanel.Visible = false;
        issueListHeaderPanel.Visible = false;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(false);
        configRulesPanel.Visible = false;
        reportsPlaceholderPanel.Visible = false;
        // Show chat-only reports panel
        analyticsPanel.Visible = true;
        SetActiveFocusedTab("reports");
        InitializeSharedFilterPanel();
    }

    protected void btnShowGridView9_Click(object sender, EventArgs e)
    {
        bool isAdminMode = IsAdminMode();

        if (!isAdminMode)
        {
            ShowWelcomeHome();
            return;
        }

        searchfilters.Visible = false;
        SetMainDataWrapperVisible(false);
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        btnImportPopup.Visible = false;
        analyticsPanel.Visible = false;
        fieldSelectorPanel.Visible = false;
        issueListHeaderPanel.Visible = false;
        SetIssuePagerVisible(false);

        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;

        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        pane8.Visible = false;
        pane9.Visible = true;

        if (homeWelcomePanel != null)
        {
            homeWelcomePanel.Visible = false;
        }

        if (homeCmfSummaryPanel != null)
        {
            homeCmfSummaryPanel.Visible = false;
        }

        fieldSelectorPanel.Visible = false;
        issueListHeaderPanel.Visible = false;
        cmf_pending_header_panel.Visible = false;
        SetIssuePagerVisible(false);
        LoadCmfRulesEditor();
        configRulesPanel.Visible = true;
        reportsPlaceholderPanel.Visible = false;
        SetActiveFocusedTab("config");
        InitializeSharedFilterPanel();
    }

    protected void btnSaveCmfRules_Click(object sender, EventArgs e)
    {
        if (!IsAdminMode())
        {
            ShowWelcomeHome();
            return;
        }

        string rulesText = txtCmfRules == null ? string.Empty : txtCmfRules.Text;
        CmfRecommendationService.SaveActiveRulesText(rulesText);
        if (lblCmfRulesStatus != null)
        {
            lblCmfRulesStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml("#059669");
            lblCmfRulesStatus.Text = "Rules saved. New CMF Pending recommendations will use this policy.";
        }

        btnShowGridView9_Click(sender, e);
    }

    protected void btnResetCmfRules_Click(object sender, EventArgs e)
    {
        if (!IsAdminMode())
        {
            ShowWelcomeHome();
            return;
        }

        CmfRecommendationService.ResetActiveRulesText();
        if (lblCmfRulesStatus != null)
        {
            lblCmfRulesStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml("#0f5ea8");
            lblCmfRulesStatus.Text = "Default CMF rules restored.";
        }

        btnShowGridView9_Click(sender, e);
    }

    protected void ddlFocusedView_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedView = ddlFocusedView.SelectedValue;

        if (string.Equals(selectedView, "home", StringComparison.OrdinalIgnoreCase))
        {
            btnShowHomeDashboard_Click(sender, e);
            return;
        }

        if (string.Equals(selectedView, "cmf", StringComparison.OrdinalIgnoreCase))
        {
            btnShowGridView3_Click(sender, e);
            return;
        }

        if (string.Equals(selectedView, "pending", StringComparison.OrdinalIgnoreCase))
        {
            btnShowGridView4_Click(sender, e);
            return;
        }

        if (string.Equals(selectedView, "reports", StringComparison.OrdinalIgnoreCase))
        {
            btnShowGridView8_Click(sender, e);
            return;
        }

        if (string.Equals(selectedView, "config", StringComparison.OrdinalIgnoreCase))
        {
            bool isAdminMode = IsAdminMode();
            if (isAdminMode)
            {
                btnShowGridView9_Click(sender, e);
                return;
            }
        }

        btnShowGridView1_Click(sender, e);
    }

    private void BindDistinctValues()

    {
        string issuePlatform = ResolveIssuePendingPlatformSelection(selectedPlatform);
        string sourceSql = BuildDashboardComponentSourceSql(issuePlatform);
        string query = "SELECT DISTINCT drivers FROM " + sourceSql + " WHERE drivers <> ' ' AND sysdebug LIKE ('%customer_must_fix%')";

        DataTable dt = ExecuteDataTable(query);
        rptDistinctValues.DataSource = dt;
        rptDistinctValues.DataBind();
    }



    private void BindDistinctFilters()

    {
        string issuePlatform = ResolveIssuePendingPlatformSelection(selectedPlatform);
        string sourceSql = BuildDashboardComponentSourceSql(issuePlatform);
        string query = "SELECT DISTINCT drivers FROM " + sourceSql + " WHERE drivers <> ' '";

        DataTable dt = ExecuteDataTable(query);
        rptDistinctFilters.DataSource = dt;
        rptDistinctFilters.DataBind();
    }




    protected void ddlTables_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Capture the selected value from the dropdown
        selectedPlatform = ResolvePlatformTable(ddlTables.SelectedValue);
        overall_request_details.PageIndex = 0;

        // Store the selected table value in the session to persist it across postbacks
        Session["selectedPlatform"] = selectedPlatform;
        Session[IssuePendingPlatformSessionKey] = selectedPlatform;
        SetDashboardPlatformSelection(selectedPlatform);
        ResetIssueFiltersToAll();
        InitializeFilterValue();
        BindAllFilters();
        headerTitle.InnerText = WorkWeek + " | CMF Live Dashboard - All Milestones";

        // Update the platform-specific dashboard link
        UpdatePlatformDashboardLink();

        string activeTab = GetActiveFocusedTab();
        if (string.Equals(activeTab, "home", StringComparison.OrdinalIgnoreCase))
        {
            ShowWelcomeHome();
            return;
        }

        bool reportsActive = string.Equals(activeTab, "reports", StringComparison.OrdinalIgnoreCase);
        RebindFocusedTabData(reportsActive);
    }

    protected void btnQuickPlatformApply_Click(object sender, EventArgs e)
    {
        string requestedPlatform = hfQuickPlatform.Value;
        if (string.IsNullOrWhiteSpace(requestedPlatform))
        {
            return;
        }

        string requestedSourceTab = hfQuickPlatformSourceTab != null
            ? (hfQuickPlatformSourceTab.Value ?? string.Empty).Trim().ToLowerInvariant()
            : string.Empty;

        string dashboardSelection = ResolveDashboardPlatformSelection(requestedPlatform.Trim());
        string excludedPlatformsRaw = hfOverallExcludedPlatforms != null ? hfOverallExcludedPlatforms.Value : null;
        SetDashboardPlatformSelection(dashboardSelection);
        string activeTab = GetActiveFocusedTab();

        if (!string.IsNullOrWhiteSpace(requestedSourceTab) &&
            (requestedSourceTab == "home" || requestedSourceTab == "cmf" || requestedSourceTab == "issue" || requestedSourceTab == "pending" || requestedSourceTab == "reports" || requestedSourceTab == "config"))
        {
            SetActiveFocusedTab(requestedSourceTab);
            activeTab = requestedSourceTab;
        }

        bool preserveDashboardFilters = string.Equals(activeTab, "home", StringComparison.OrdinalIgnoreCase)
            || string.Equals(activeTab, "cmf", StringComparison.OrdinalIgnoreCase);

        if (IsOverallDashboardPlatform(dashboardSelection))
        {
            UpdateOverallExcludedPlatformsFromClient(excludedPlatformsRaw);
            RefreshOverallExcludedPlatformsJsonForClient();
            Session[IssuePendingPlatformSessionKey] = OverallDashboardPlatformTable;
            selectedPlatform = OverallDashboardPlatformTable;
            overall_request_details.PageIndex = 0;

            if (!preserveDashboardFilters)
            {
                ResetIssueFiltersToAll();
                InitializeFilterValue();
                BindAllFilters();
            }

            UpdatePlatformDashboardLink();

            if (string.Equals(activeTab, "pending", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activeTab, "issue", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activeTab, "reports", StringComparison.OrdinalIgnoreCase))
            {
                RebindFocusedTabData(string.Equals(activeTab, "reports", StringComparison.OrdinalIgnoreCase));
                InitializeSharedFilterPanel();
                return;
            }

            if (string.Equals(activeTab, "cmf", StringComparison.OrdinalIgnoreCase))
            {
                ShowModernCmfSummary();
                return;
            }

            ShowWelcomeHome();
            return;
        }

        string resolvedPlatform = ResolvePlatformTable(requestedPlatform.Trim());
        ListItem matchedItem = ddlTables.Items.FindByValue(resolvedPlatform);
        if (matchedItem == null)
        {
            if (ddlTables.Items.Count == 0)
            {
                return;
            }

            matchedItem = ddlTables.Items[0];
            resolvedPlatform = matchedItem.Value;
        }

        ddlTables.ClearSelection();
        matchedItem.Selected = true;
        overall_request_details.PageIndex = 0;
        selectedPlatform = resolvedPlatform;
        Session["selectedPlatform"] = resolvedPlatform;
        Session[IssuePendingPlatformSessionKey] = resolvedPlatform;
        SetDashboardPlatformSelection(resolvedPlatform);

        if (!preserveDashboardFilters)
        {
            ResetIssueFiltersToAll();
            InitializeFilterValue();
            BindAllFilters();
        }

        UpdatePlatformDashboardLink();

        if (string.Equals(activeTab, "home", StringComparison.OrdinalIgnoreCase))
        {
            ShowWelcomeHome();
            return;
        }

        bool reportsActive = string.Equals(activeTab, "reports", StringComparison.OrdinalIgnoreCase);
        RebindFocusedTabData(reportsActive);
    }

    // Helper method to update platform-specific dashboard link
    private void UpdatePlatformDashboardLink()
    {
        string currentPlatform = GetIssuePendingPlatform();
        CmfPlatformDefinition platform;

        if (CmfPlatformCatalog.TryGetPlatformByTable(currentPlatform, out platform) &&
            !string.IsNullOrWhiteSpace(platform.CdrDocumentUrl))
        {
            lnkPlatformDashboard.NavigateUrl = platform.CdrDocumentUrl;
            lnkPlatformDashboard.Text = platform.Label + " Dashboard";
            lnkPlatformDashboard.Visible = true;
            if (lnkPlatformDashboardPending != null)
            {
                lnkPlatformDashboardPending.Visible = false;
            }
        }
        else
        {
            // Hide the link if no dashboard exists for this platform
            lnkPlatformDashboard.Visible = false;
        }
    }

    [Serializable]
    public class FilterItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public FilterItem() { }

        public FilterItem(string text, string value)
        {
            Text = text;
            Value = value;
        }
    }

    // Remove ddlProgressHeader_SelectedIndexChanged and ddlComponentHeader_SelectedIndexChanged

    protected void ddlOwnerHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["ownerFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlRvpReproHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["rvpReproFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlIdstHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["idstFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlCompanyHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["companyFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlDetailHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["detailFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlComponentHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["componentFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlLosHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["losFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlMilestoneHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["milestoneFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void btnClearFilters_Click(object sender, EventArgs e)
    {
        // Clear all filter sessions
        Session["ownerFilter"] = null;
        Session["rvpReproFilter"] = null;
        Session["idstFilter"] = null;
        Session["losFilter"] = null;
        Session["milestoneFilter"] = null;
        Session["companyFilter"] = null;
        Session["detailFilter"] = null;
        Session["componentFilter"] = null;
        Session["issueStatusFilter"] = null;

        // Reset dropdown selections to "All"
        ddlOwnerTop.SelectedValue = "All";
        ddlRvpReproTop.SelectedValue = "All";
        ddlIdstTop.SelectedValue = "All";
        ddlLosTop.SelectedValue = "All";
        ddlMilestoneTop.SelectedValue = "All";
        ddlCompanyTop.SelectedValue = "All";
        ddlDetailTop.SelectedValue = "All";
        ddlComponentTop.SelectedValue = "All";
        ddlStatusTop.SelectedValue = "All";

        // Apply the cleared filters
        ApplyAllFilters();
    }

    private void ApplyAllFilters()
    {
        EnsureIssueTabVisibleForPostback();
        overall_request_details.PageIndex = 0;
        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        BindAllFilters();
        BindGridView(filterValue, filters, bindRelatedGrids: false);
    }

    protected void ddlStatusTop_SelectedIndexChanged(object sender, EventArgs e)
    {
        Session["issueStatusFilter"] = ddlStatusTop.SelectedValue;
        ApplyAllFilters();
    }

    // Updated to remove progress and component filters
    private Dictionary<string, string> GetAllFilterValues()
    {
        return new Dictionary<string, string>
    {
        {"owner", Session["ownerFilter"] as string ?? "All"},
        {"rvpRepro", Session["rvpReproFilter"] as string ?? "All"},
        {"idst", Session["idstFilter"] as string ?? "All"},
        {"los", Session["losFilter"] as string ?? "All"},
        {"milestone", Session["milestoneFilter"] as string ?? "All"},
        {"Company", Session["companyFilter"] as string ?? "All"},      // Fixed: use "companyFilter"
        {"Detail", Session["detailFilter"] as string ?? "All"},        // Fixed: use "detailFilter"
        {"Component", Session["componentFilter"] as string ?? "All"},  // Fixed: use "componentFilter"
        {"IssueStatus", Session["issueStatusFilter"] as string ?? "All"},
    };
    }

    private string BuildIssueGridCacheKey(string filterValue, Dictionary<string, string> columnFilters)
    {
        StringBuilder keyBuilder = new StringBuilder();
        keyBuilder.Append(ResolveIssuePendingPlatformSelection(selectedPlatform));
        keyBuilder.Append("|driver=");
        keyBuilder.Append(string.IsNullOrWhiteSpace(filterValue) ? "" : filterValue.Trim());
        keyBuilder.Append("|search=");
        keyBuilder.Append(GetIssueGlobalSearchQuery().Trim());

        if (columnFilters != null)
        {
            foreach (KeyValuePair<string, string> filter in columnFilters.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
            {
                keyBuilder.Append("|");
                keyBuilder.Append(filter.Key ?? string.Empty);
                keyBuilder.Append("=");
                keyBuilder.Append(filter.Value ?? string.Empty);
            }
        }

        return keyBuilder.ToString();
    }

    private DataTable ApplyIssueGlobalSearch(DataTable dt)
    {
        string query = GetIssueGlobalSearchQuery().Trim();
        if (dt == null || string.IsNullOrWhiteSpace(query))
        {
            return dt;
        }

        DataTable filtered = dt.Clone();
        foreach (DataRow row in dt.Rows)
        {
            foreach (object value in row.ItemArray)
            {
                if (value != null && value != DBNull.Value && value.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    filtered.ImportRow(row);
                    break;
                }
            }
        }

        return filtered;
    }

    private void CacheIssueGridData(string filterValue, Dictionary<string, string> columnFilters, DataTable dt)
    {
        Session[IssueGridCacheKeySessionKey] = BuildIssueGridCacheKey(filterValue, columnFilters);
        Session[IssueGridCacheDataSessionKey] = dt;
    }

    private bool TryBindIssueGridFromCache(string filterValue, Dictionary<string, string> columnFilters)
    {
        string expectedKey = BuildIssueGridCacheKey(filterValue, columnFilters);
        string cachedKey = Session[IssueGridCacheKeySessionKey] as string;
        DataTable cachedData = Session[IssueGridCacheDataSessionKey] as DataTable;

        if (cachedData == null || string.IsNullOrWhiteSpace(cachedKey) || !string.Equals(expectedKey, cachedKey, StringComparison.Ordinal))
        {
            return false;
        }

        BindIssueGridFromDataTable(cachedData, bindRelatedGrids: false);
        return true;
    }

    private void BindIssueGridFromDataTable(DataTable dt, bool bindRelatedGrids)
    {
        if (dt == null)
        {
            dt = new DataTable();
        }

        int totalIssues = dt.Rows.Count;
        int openIssues = 0;
        int closedIssues = 0;
        int implementedIssues = 0;

        foreach (DataRow row in dt.Rows)
        {
            string status = row["IssueStatus"] == DBNull.Value
                ? string.Empty
                : row["IssueStatus"].ToString().Trim().ToLowerInvariant();

            if (status == "complete" || status == "rejected")
            {
                closedIssues++;
            }
            else if (status == "implemented" || status == "verified")
            {
                implementedIssues++;
            }
            else
            {
                openIssues++;
            }
        }

        lblIssueTotal.Text = totalIssues.ToString();
        lblIssueInProgress.Text = openIssues.ToString();
        lblIssueClosed.Text = closedIssues.ToString();
        lblIssueStale.Text = implementedIssues.ToString();

        int maxPageIndex = Math.Max(0, (int)Math.Ceiling(dt.Rows.Count / (double)overall_request_details.PageSize) - 1);
        if (overall_request_details.PageIndex > maxPageIndex)
        {
            overall_request_details.PageIndex = maxPageIndex;
        }

        overall_request_details.DataSource = dt;
        overall_request_details.DataKeyNames = new string[] { "SightingID" };
        overall_request_details.DataBind();

        int pageCount = Math.Max(1, overall_request_details.PageCount);
        int currentPage = overall_request_details.PageIndex + 1;

        const int pagesPerGroup = 10;
        int currentGroupStartPage = ((currentPage - 1) / pagesPerGroup) * pagesPerGroup + 1;
        int currentGroupEndPage = Math.Min(currentGroupStartPage + pagesPerGroup - 1, pageCount);

        List<PageNumberItem> pageNumbers = new List<PageNumberItem>();
        for (int i = currentGroupStartPage; i <= currentGroupEndPage; i++)
        {
            pageNumbers.Add(new PageNumberItem
            {
                PageNumber = i,
                IsCurrentPage = (i == currentPage)
            });
        }
        rptPageNumbers.DataSource = pageNumbers;
        rptPageNumbers.DataBind();

        bool hasPreviousGroup = currentGroupStartPage > 1;
        bool hasNextGroup = currentGroupEndPage < pageCount;

        btnPageGroupPrev.Enabled = hasPreviousGroup;
        btnPageGroupNext.Enabled = hasNextGroup;

        btnPageGroupPrev.CssClass = btnPageGroupPrev.Enabled ? "issue-pager-group-btn" : "issue-pager-group-btn disabled";
        btnPageGroupNext.CssClass = btnPageGroupNext.Enabled ? "issue-pager-group-btn" : "issue-pager-group-btn disabled";

        int rangeStart = totalIssues == 0 ? 0 : (overall_request_details.PageIndex * overall_request_details.PageSize) + 1;
        int rangeEnd = totalIssues == 0 ? 0 : Math.Min(totalIssues, rangeStart + overall_request_details.PageSize - 1);
        lblIssuePageStatus.Text = string.Format("Showing {0}-{1} of {2} | Current: {3} of {4}",
            rangeStart,
            rangeEnd,
            totalIssues,
            currentPage,
            pageCount);

        if (bindRelatedGrids)
        {
            BindGridView_design_open();
            BindGridView_cmf_summary();
            BindGridView_cmf_pending();
            BindGridView_design_summary();
            BindGridView_component_summary();
            BindGridView_oem_summary();
        }
    }

    private static Dictionary<string, string> GetFilterValuesExcluding(Dictionary<string, string> filters, string excludeKey)
    {
        Dictionary<string, string> scopedFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (filters == null)
        {
            return scopedFilters;
        }

        foreach (KeyValuePair<string, string> filter in filters)
        {
            if (string.Equals(filter.Key, excludeKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            scopedFilters[filter.Key] = filter.Value;
        }

        return scopedFilters;
    }

    private static void AddColumnFilterParameters(SqlCommand cmd, Dictionary<string, string> filters)
    {
        if (cmd == null || filters == null)
        {
            return;
        }

        foreach (KeyValuePair<string, string> filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Value) || filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            switch (filter.Key)
            {
                case "owner":
                    cmd.Parameters.AddWithValue("@OwnerFilter", filter.Value);
                    break;
                case "rvpRepro":
                    cmd.Parameters.AddWithValue("@RvpReproFilter", filter.Value);
                    break;
                case "idst":
                    cmd.Parameters.AddWithValue("@IdstFilter", filter.Value);
                    break;
                case "los":
                    cmd.Parameters.AddWithValue("@LosFilter", filter.Value);
                    break;
                case "milestone":
                    cmd.Parameters.AddWithValue("@MilestoneFilter", filter.Value);
                    break;
                case "Company":
                    cmd.Parameters.AddWithValue("@CompanyFilter", filter.Value);
                    break;
                case "Detail":
                    cmd.Parameters.AddWithValue("@DetailFilter", filter.Value);
                    break;
                case "Component":
                    cmd.Parameters.AddWithValue("@ComponentFilter", filter.Value);
                    break;
                case "IssueStatus":
                    cmd.Parameters.AddWithValue("@IssueStatusFilter", filter.Value);
                    break;
            }
        }
    }

    // Updated to remove progress and component filters
    private void BindAllFilters()
    {
        // Get the current platform
        ApplyIssuePendingPlatformContext();
        string currentPlatform = GetIssuePendingPlatform();
        Dictionary<string, string> activeFilters = GetAllFilterValues();

        using (var con = new SqlConnection(ConnectionString))
        {
            con.Open();
            EnsurePublicIssueDriverFilter(con, currentPlatform);

            BindOwnerFilter(con, currentPlatform, activeFilters);
            BindFilterData(con, "repro_on_rvp", "RvpReproItems", currentPlatform, activeFilters, "rvpRepro");
            BindFilterData(con, "idst", "IdstItems", currentPlatform, activeFilters, "idst");
            BindFilterData(con, "los", "LosItems", currentPlatform, activeFilters, "los");
            BindFilterData(con, "drivers", "MilestoneItems", currentPlatform, activeFilters, "milestone");
            BindFilterData(con, "customer_company", "CompanyItems", currentPlatform, activeFilters, "Company");
            BindFilterData(con, "customer_detail", "DetailItems", currentPlatform, activeFilters, "Detail");
            BindFilterData(con, "component_group", "ComponentItems", currentPlatform, activeFilters, "Component");
            BindFilterData(con, "status", "IssueStatusItems", currentPlatform, activeFilters, "IssueStatus");
        }

        PopulateTopFilterDropdowns();
    }

    private void PopulateTopFilterDropdowns()
    {
        PopulateTopFilterDropdown("ddlOwnerTop", "OwnerItems", "ownerFilter");
        PopulateTopFilterDropdown("ddlRvpReproTop", "RvpReproItems", "rvpReproFilter");
        PopulateTopFilterDropdown("ddlIdstTop", "IdstItems", "idstFilter");
        PopulateTopFilterDropdown("ddlLosTop", "LosItems", "losFilter");
        PopulateTopFilterDropdown("ddlCompanyTop", "CompanyItems", "companyFilter");
        PopulateTopFilterDropdown("ddlDetailTop", "DetailItems", "detailFilter");
        PopulateTopFilterDropdown("ddlComponentTop", "ComponentItems", "componentFilter");
        PopulateTopFilterDropdown("ddlMilestoneTop", "MilestoneItems", "milestoneFilter");
        PopulateTopFilterDropdown("ddlStatusTop", "IssueStatusItems", "issueStatusFilter");
    }

    private void BindFilterData(SqlConnection con, string columnName, string viewStateKey, string platform, Dictionary<string, string> activeFilters, string excludeFilterKey)
    {
        string sourceSql = BuildDashboardComponentSourceSql(platform);
        string selectClause;
        string initialWhereClause;

        if (columnName == "los")
        {
            // For LOS, convert NULL/empty to "No" to match GridView processing
            selectClause = "SELECT DISTINCT " +
                          "CASE " +
                          "    WHEN los IS NULL OR LTRIM(RTRIM(los)) = '' THEN 'No' " +
                          "    ELSE LTRIM(RTRIM(los)) " +
                          "END AS filterValue ";
            initialWhereClause = "WHERE 1=1 "; // Always true, we handle all cases in CASE statement
        }
        else
        {
            selectClause = "SELECT DISTINCT LTRIM(RTRIM(" + columnName + ")) AS filterValue ";
            initialWhereClause = "WHERE " + columnName + " IS NOT NULL AND LTRIM(RTRIM(" + columnName + ")) <> '' ";
        }

        string query = selectClause + "FROM " + sourceSql + " " + initialWhereClause;

        // Get the current filter value from session
        string filterValue = Session["filterValue"] as string;

        // Add the same WHERE conditions as the main GridView query
        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
        {
            if (filterValue.Contains(","))
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR ( " +
                        "       @FilterValue LIKE '%,' + drivers + ',%' " +
                        "       OR @FilterValue LIKE drivers + ',%' " +
                        "       OR @FilterValue LIKE '%,' + drivers) ) " +
                        "AND sysdebug Like ('%customer_must_fix%') AND status NOT IN ('rejected') AND cmf_request in ('cmf_ok') ";
            }
            else
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR drivers = @FilterValue ) AND status NOT IN ('rejected') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
            }
        }
        else
        {
            query += " AND status NOT IN ('rejected') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
        }

        Dictionary<string, string> dependentFilters = GetFilterValuesExcluding(activeFilters, excludeFilterKey);
        query += BuildFilterClauses(dependentFilters);

        query += " ORDER BY filterValue;";

        using (var cmd = new SqlCommand(query, con))
        {
            // Add parameter if filterValue exists and is not AllDrivers
            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                cmd.Parameters.AddWithValue("@FilterValue", filterValue);
            }

            AddColumnFilterParameters(cmd, dependentFilters);

            using (var rdr = cmd.ExecuteReader())
            {
                List<FilterItem> items = new List<FilterItem>();
                items.Add(new FilterItem("All", "All"));

                while (rdr.Read())
                {
                    var v = rdr["filterValue"].ToString();
                    if (!string.IsNullOrEmpty(v)) // Skip empty values
                    {
                        items.Add(new FilterItem(v, v));
                    }
                }

                ViewState[viewStateKey] = items;
            }
        }
    }

    private void BindOwnerFilter(SqlConnection con, string platform, Dictionary<string, string> activeFilters)
    {
        string sourceSql = BuildDashboardComponentSourceSql(platform);
        // Get the current filter value from session
        string filterValue = Session["filterValue"] as string;

        string query = "SELECT DISTINCT LTRIM(RTRIM(customer_owner)) AS owner " +
                       "FROM " + sourceSql + " " +
                       "WHERE customer_owner IS NOT NULL AND LTRIM(RTRIM(customer_owner)) <> '' ";

        // Add the same WHERE conditions as the main GridView query
        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
        {
            if (filterValue.Contains(","))
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR ( " +
                        "       @FilterValue LIKE '%,' + drivers + ',%' " +
                        "       OR @FilterValue LIKE drivers + ',%' " +
                        "       OR @FilterValue LIKE '%,' + drivers) ) " +
                        "AND sysdebug Like ('%customer_must_fix%') AND status NOT IN ('rejected') AND cmf_request in ('cmf_ok') ";
            }
            else
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR drivers = @FilterValue ) AND status NOT IN ('rejected') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
            }
        }
        else
        {
            query += " AND status NOT IN ('rejected') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
        }

        Dictionary<string, string> dependentFilters = GetFilterValuesExcluding(activeFilters, "owner");
        query += BuildFilterClauses(dependentFilters);

        query += " ORDER BY owner;";

        using (var cmd = new SqlCommand(query, con))
        {
            // Add parameter if filterValue exists and is not AllDrivers
            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                cmd.Parameters.AddWithValue("@FilterValue", filterValue);
            }

            AddColumnFilterParameters(cmd, dependentFilters);

            using (var rdr = cmd.ExecuteReader())
            {
                List<string> ownerAliases = new List<string>();
                while (rdr.Read())
                {
                    ownerAliases.Add(rdr["owner"].ToString());
                }

                List<FilterItem> items = new List<FilterItem>();
                items.Add(new FilterItem("All", "All"));

                var ownerCache = Session["OwnerDisplayCache"] as Dictionary<string, string>
                                 ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var aliasesNeeded = ownerAliases
                    .Select(a => ProcessOwnerAlias(a))
                    .Where(a => !string.IsNullOrEmpty(a) && !ownerCache.ContainsKey(a))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (aliasesNeeded.Count > 0)
                {
                    PrincipalContext context_gar = null;
                    PrincipalContext context_amr = null;
                    PrincipalContext context_ccr = null;
                    PrincipalContext context_ger = null;

                    try { context_gar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
                    try { context_amr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
                    try { context_ccr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
                    try { context_ger = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

                    foreach (var alias in aliasesNeeded)
                        ownerCache[alias] = GetOwnerDisplayName(alias, context_gar, context_amr, context_ccr, context_ger);

                    if (context_gar != null) context_gar.Dispose();
                    if (context_amr != null) context_amr.Dispose();
                    if (context_ccr != null) context_ccr.Dispose();
                    if (context_ger != null) context_ger.Dispose();
                }

                foreach (string rawOwner in ownerAliases)
                {
                    string processedAlias = ProcessOwnerAlias(rawOwner);
                    string displayName;
                    if (!ownerCache.TryGetValue(processedAlias, out displayName))
                        displayName = processedAlias;

                    items.Add(new FilterItem(displayName, rawOwner));
                }

                ViewState["OwnerItems"] = items;
            }
        }
    }

    private string ProcessOwnerAlias(string rawOwner)
    {
        if (string.IsNullOrEmpty(rawOwner)) return string.Empty;

        string processed = rawOwner.Trim();
        if (processed.Contains("\\")) processed = processed.Substring(processed.LastIndexOf('\\') + 1);
        if (processed.Contains("@")) processed = processed.Split('@')[0];

        return processed.Trim();
    }

    private string GetOwnerDisplayName(string alias, PrincipalContext context_gar, PrincipalContext context_amr, PrincipalContext context_ccr, PrincipalContext context_ger)
    {
        if (string.IsNullOrEmpty(alias)) return "Unable to fetch";

        UserPrincipal up = null;

        if (context_gar != null) up = UserPrincipal.FindByIdentity(context_gar, IdentityType.SamAccountName, alias);
        if (up == null && context_amr != null) up = UserPrincipal.FindByIdentity(context_amr, IdentityType.SamAccountName, alias);
        if (up == null && context_ccr != null) up = UserPrincipal.FindByIdentity(context_ccr, IdentityType.SamAccountName, alias);
        if (up == null && context_ger != null) up = UserPrincipal.FindByIdentity(context_ger, IdentityType.SamAccountName, alias);

        return up != null ? up.DisplayName : alias;
    }

    private string ProcessOwnerName(string rawOwner)
    {
        if (string.IsNullOrEmpty(rawOwner)) return "Unable to fetch";

        string processed = rawOwner.Trim();
        if (processed.Contains("\\")) processed = processed.Substring(processed.LastIndexOf('\\') + 1);
        if (processed.Contains("@")) processed = processed.Split('@')[0];

        return processed;
    }

    // Populate issue filter controls above the table.
    protected void overall_request_details_DataBound(object sender, EventArgs e)
    {
        PopulateTopFilterDropdown("ddlOwnerTop", "OwnerItems", "ownerFilter");
        PopulateTopFilterDropdown("ddlRvpReproTop", "RvpReproItems", "rvpReproFilter");
        PopulateTopFilterDropdown("ddlIdstTop", "IdstItems", "idstFilter");
        PopulateTopFilterDropdown("ddlLosTop", "LosItems", "losFilter");
        PopulateTopFilterDropdown("ddlCompanyTop", "CompanyItems", "companyFilter");
        PopulateTopFilterDropdown("ddlDetailTop", "DetailItems", "detailFilter");
        PopulateTopFilterDropdown("ddlComponentTop", "ComponentItems", "componentFilter");
        PopulateTopFilterDropdown("ddlMilestoneTop", "MilestoneItems", "milestoneFilter");
    }

    private void PopulateTopFilterDropdown(string dropdownId, string viewStateKey, string sessionKey)
    {
        DropDownList ddl = fieldSelectorPanel.FindControl(dropdownId) as DropDownList;
        if (ddl != null && ViewState[viewStateKey] != null)
        {
            List<FilterItem> items = ViewState[viewStateKey] as List<FilterItem>;
            if (items != null)
            {
                ddl.Items.Clear();

                foreach (FilterItem item in items)
                {
                    ddl.Items.Add(new ListItem(item.Text, item.Value));
                }

                if (Session[sessionKey] != null)
                {
                    string value = Session[sessionKey].ToString();
                    var item = ddl.Items.FindByValue(value);
                    if (item != null)
                        ddl.SelectedValue = value;
                }
            }
        }
    }

    // Updated BuildFilterClauses (remove progress and component)
    private static string BuildFilterClauses(Dictionary<string, string> filters)
    {
        return BuildFilterClauses(filters, string.Empty);
    }

    private static string BuildFilterClauses(Dictionary<string, string> filters, string tableAlias)
    {
        List<string> clauses = new List<string>();
        string prefix = string.IsNullOrWhiteSpace(tableAlias) ? string.Empty : tableAlias + ".";

        foreach (var filter in filters)
        {
            if (!string.IsNullOrWhiteSpace(filter.Value) && !filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                switch (filter.Key)
                {
                    case "owner":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "customer_owner)) = @OwnerFilter ");
                        break;
                    case "rvpRepro":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "repro_on_rvp)) = @RvpReproFilter ");
                        break;
                    case "idst":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "idst)) = @IdstFilter ");
                        break;
                    case "Company":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "customer_company)) = @CompanyFilter ");
                        break;
                    case "Detail":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "customer_detail)) = @DetailFilter ");
                        break;
                    case "Component":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "component_group)) = @ComponentFilter ");
                        break;
                    case "IssueStatus":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "status)) = @IssueStatusFilter ");
                        break;
                    //case "los":
                    //    clauses.Add(" AND LTRIM(RTRIM(los)) = @LosFilter ");
                    //    break;

                    case "los":
                        // Special handling: 'No' in the UI means NULL or empty in DB
                        clauses.Add(
                            " AND ( " +
                            "     (@LosFilter = 'No' AND (" + prefix + "los IS NULL OR LTRIM(RTRIM(" + prefix + "los)) = '')) " +
                            "  OR (@LosFilter <> 'No' AND LTRIM(RTRIM(" + prefix + "los)) = @LosFilter) " +
                            ") "
                        );
                        break;

                    case "milestone":
                        clauses.Add(" AND LTRIM(RTRIM(" + prefix + "drivers)) = @MilestoneFilter ");
                        break;
                }
            }
        }

        return string.Join("", clauses);
    }

    private void InitializeFilterValue()
    {
        using (SqlConnection con = new SqlConnection(ConnectionString))
        {
            con.Open();
            string currentPlatform = GetIssuePendingPlatform();
            EnsurePublicIssueDriverFilter(con, currentPlatform, forceRefresh: true);
        }
    }

    private void EnsurePublicIssueDriverFilter(SqlConnection con, string platform, bool forceRefresh = false)
    {
        string currentFilterValue = Session["filterValue"] as string;
        if (!forceRefresh && !string.IsNullOrWhiteSpace(currentFilterValue) && !currentFilterValue.Equals("AllDrivers", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string sourceSql = BuildDashboardComponentSourceSql(platform);
        string driverQuery = "SELECT DISTINCT([drivers]) FROM " + sourceSql +
                             " WHERE drivers IS NOT NULL AND LTRIM(RTRIM(drivers)) <> '' " +
                             " AND status in ('open','implemented') " +
                             " AND cmf_request not in ('cmf_reject') " +
                             " AND sysdebug Like ('%customer_must_fix%') ";

        using (SqlCommand cmd = new SqlCommand(driverQuery, con))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
            List<string> driversList = new List<string>();
            while (reader.Read())
            {
                string driverValue = reader["drivers"] == DBNull.Value ? string.Empty : reader["drivers"].ToString().Trim();
                if (!string.IsNullOrEmpty(driverValue))
                {
                    driversList.Add(driverValue);
                }
            }

            Session["filterValue"] = driversList.Count > 0 ? string.Join(",", driversList) : null;
        }
    }

    private static PrincipalContext TryCreateContext(string domain)
    {
        try { return new PrincipalContext(ContextType.Domain, domain); }
        catch { return null; }
    }


    private static string ResolveDisplayName(
    string alias,
    IEnumerable<PrincipalContext> contexts)
    {
        foreach (PrincipalContext ctx in contexts)
        {
            if (ctx == null)
                continue;

            UserPrincipal up = UserPrincipal.FindByIdentity(
                ctx, IdentityType.SamAccountName, alias);

            if (up != null)
                return up.DisplayName ?? alias;
        }

        // ✅ REQUIRED return to satisfy compiler
        return "Unable to fetch";
    }
    private void EnrichNewUsers()
    {
        // ✅ Old‑style instantiation
        List<string> users = new List<string>();

        using (SqlConnection con = new SqlConnection(
            ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
        {
            con.Open();

            using (SqlCommand cmd = new SqlCommand(@"
         SELECT Username
         FROM dbo.CMFUserVisitCounts_Final
         WHERE DisplayName IS NULL;", con))
            {
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        users.Add(rdr.GetString(0));
                    }
                }
            }
        }

        // ✅ Nothing to resolve
        if (users.Count == 0)
            return;

        // ✅ Explicit type (no target-typed new)
        PrincipalContext[] contexts = new PrincipalContext[]
        {
     TryCreateContext("gar.corp.intel.com"),
     TryCreateContext("amr.corp.intel.com"),
     TryCreateContext("ccr.corp.intel.com"),
     TryCreateContext("ger.corp.intel.com")
        };

        using (SqlConnection updateCon = new SqlConnection(
            ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
        {
            updateCon.Open();

            foreach (string alias in users)
            {
                string displayName = ResolveDisplayName(alias, contexts);

                using (SqlCommand updateCmd = new SqlCommand(@"
             UPDATE dbo.CMFUserVisitCounts_Final
             SET DisplayName = @name,
                 LastUpdated = SYSDATETIME()
             WHERE Username = @user;", updateCon))
                {
                    updateCmd.Parameters.AddWithValue("@user", alias);
                    updateCmd.Parameters.AddWithValue("@name", displayName);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }

    protected void Page_Error(object sender, EventArgs e)
    {
        Exception error = Server.GetLastError();
        bool invalidViewState = error is ViewStateException
            || (error is HttpException && error.Message.IndexOf("state information is invalid", StringComparison.OrdinalIgnoreCase) >= 0)
            || (error != null && error.ToString().IndexOf("ViewStateException", StringComparison.OrdinalIgnoreCase) >= 0);

        if (!invalidViewState)
        {
            return;
        }

        Server.ClearError();
        string cleanUrl = Request.Url.AbsolutePath + "?vsreset=" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
        Response.Redirect(cleanUrl, false);
        Context.ApplicationInstance.CompleteRequest();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        overall_request_details.DataBound += overall_request_details_DataBound;
        if (!IsPostBack)
        {
            var dateFilter = GetDashboardDateFilter();
            ddlDashboardDateMode.SelectedValue = dateFilter.Mode;
            ddlDashboardDateBasis.SelectedValue = dateFilter.Basis;
            txtDashboardDateStart.Text = (dateFilter.Start ?? DateTime.Today.AddDays(-6)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtDashboardDateEnd.Text = (dateFilter.EndExclusive.HasValue ? dateFilter.EndExclusive.Value.AddDays(-1) : DateTime.Today).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            dashboardCustomDates.Visible = dateFilter.Mode == "custom";
        }
        //GridView_cmf_summary.RowCreated += GridView_cmf_summary_RowCreated;

        if (ddlUserMode != null && !IsPostBack)
        {
            string persistedMode = Session[UserModeSessionKey] as string;
            if (string.IsNullOrWhiteSpace(persistedMode))
            {
                persistedMode = "program_manager";
                Session[UserModeSessionKey] = persistedMode;
            }

            ListItem persistedItem = ddlUserMode.Items.FindByValue(persistedMode);
            if (persistedItem != null)
            {
                ddlUserMode.ClearSelection();
                persistedItem.Selected = true;
            }
        }

        lnkAllDrivers.Visible = true;
        ApplyFocusedPortalMode();
        ApplyRuntimePlatformAvailability();

        if (!IsPostBack)
        {
            //EnrichNewUsers(); --to trigger enrichment of new users in the visit count table, can be commented out when not required

            DateTime currentDate = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            int workWeekNumber = cultureInfo.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            WorkWeek = string.Format("WW'{0:D2}", workWeekNumber);

            // Set initial platform correctly
            ListItem defaultPlatformItem = ddlTables.Items.FindByValue(DefaultPlatformTable);
            if (defaultPlatformItem != null)
            {
                ddlTables.ClearSelection();
                defaultPlatformItem.Selected = true;
            }

            selectedPlatform = ResolvePlatformTable(ddlTables.SelectedValue);
            Session["selectedPlatform"] = selectedPlatform;
            Session[IssuePendingPlatformSessionKey] = DefaultIssuePendingPlatformTable;
            SetDashboardPlatformSelection(OverallDashboardPlatformTable);
            Session["isFirstLoad"] = null;

            btnExportToExcel.Visible = false;
            btnExportToExcel_ig.Visible = false;
            btnExportToExcel_des.Visible = false;
            btnExportToExcel_cmf_pending.Visible = false;
            btnExportToExcel_des_summary.Visible = false;
            overall_request_details.Visible = true;
            GridView_design_open.Visible = false;
            GridView_cmf_summary.Visible = false;
            GridView_cmf_summary1.Visible = false;
            GridView_milestone_map.Visible = false;
            GridView_notes.Visible = false;
            GridView_comp.Visible = false;
            tptdefdiv.Visible = false;
            GridView_cmf_pending.Visible = false;
            btnImportPopup.Visible = false;
            analyticsPanel.Visible = false;
            fieldSelectorPanel.Visible = true;
            issueListHeaderPanel.Visible = true;
            pane1.Visible = false;
            pane2.Visible = false;
            pane3.Visible = true;
            pane4.Visible = false;
            pane5.Visible = false;
            pane6.Visible = false;
            pane7.Visible = false;
            pane8.Visible = false;
            ShowWelcomeHome();
            SetActiveFocusedTab("home");

            headerTitle.InnerText = WorkWeek + driver;

            // Updated filter initialization (removed progress and component):
            if (Session["ownerFilter"] == null) ResetIssueFiltersToAll();

            // Initialize filterValue for first load - this is important for BindAllFilters
            InitializeFilterValue();

            // Initialize the shared filter panel for Issue List and CMF Pending List
            InitializeSharedFilterPanel();

            // Set the platform-specific dashboard link
            UpdatePlatformDashboardLink();

            // Total Visits Logic
            string session_count_table = "CMFUserVisitCounts_Final";
            string identity = HttpContext.Current.User.Identity.Name;
            int checkidentity = identity.LastIndexOf('\\');
            string var_username = identity.Substring(checkidentity + 1);

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM " + session_count_table + " WHERE Username = @username";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@username", var_username);
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    query = "UPDATE " + session_count_table + " SET SessionCount = SessionCount + 1 WHERE Username = @username";
                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@username", var_username);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    query = "INSERT INTO " + session_count_table + " (Username, SessionCount) VALUES (@username, 1)";
                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@username", var_username);
                    cmd.ExecuteNonQuery();
                }

                query = "SELECT ISNULL(SUM(SessionCount), 0) FROM " + session_count_table;
                cmd = new SqlCommand(query, con);
                count = (int)cmd.ExecuteScalar();
                session_count.Text = "Total Visits: " + count.ToString();
            }
        }
        else
        {
            Session["isFirstLoad"] = false;
            SetWorkWeek();

            string eventTarget = Request["__EVENTTARGET"];
            bool isGlobalPlatformPostback = ddlTables != null &&
                string.Equals(eventTarget, ddlTables.UniqueID, StringComparison.OrdinalIgnoreCase);

            // Keep ddlTables aligned with session-driven platform unless user explicitly changed ddlTables.
            if (ddlTables != null &&
                !isGlobalPlatformPostback)
            {
                string sessionPlatform = Session["selectedPlatform"] as string;
                if (!string.IsNullOrWhiteSpace(sessionPlatform))
                {
                    string resolvedSessionPlatform = ResolvePlatformTable(sessionPlatform);
                    ListItem ddlPlatformItem = ddlTables.Items.FindByValue(resolvedSessionPlatform);
                    if (ddlPlatformItem != null)
                    {
                        ddlTables.ClearSelection();
                        ddlPlatformItem.Selected = true;
                    }
                }
            }

            // Check if platform has changed
            string newPlatform = ResolvePlatformTable(ddlTables.SelectedValue);
            string currentPlatform = Session["selectedPlatform"] as string;

            if (isGlobalPlatformPostback && newPlatform != currentPlatform)
            {
                // Platform changed - update and rebind filters
                Session["selectedPlatform"] = newPlatform;
                selectedPlatform = newPlatform;
                SetDashboardPlatformSelection(newPlatform);

                Session["OwnerDisplayCache"] = null; // invalidate on platform change

                // Clear all filters when platform changes
                ResetIssueFiltersToAll();

                // IMPORTANT: Initialize filterValue for the new platform before binding filters
                InitializeFilterValue();

                // Rebind filters for new platform
                BindAllFilters();

                // Rebind only required tab datasets when platform changes
                RebindFocusedTabData(string.Equals(GetActiveFocusedTab(), "reports", StringComparison.OrdinalIgnoreCase));

                // Update the platform-specific dashboard link
                UpdatePlatformDashboardLink();
            }
            else
            {
                string activeFocusedTab = GetActiveFocusedTab();
                if (string.Equals(activeFocusedTab, "issue", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(activeFocusedTab, "pending", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyIssuePendingPlatformContext();
                }
                else
                {
                    bool dashboardDrivenTab = string.Equals(activeFocusedTab, "home", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(activeFocusedTab, "cmf", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(activeFocusedTab, "reports", StringComparison.OrdinalIgnoreCase);
                    if (dashboardDrivenTab)
                    {
                        selectedPlatform = ResolveDashboardPlatformSelection();
                    }
                    else
                    {
                        selectedPlatform = ResolvePlatformTable(Session["selectedPlatform"] as string ?? ddlTables.SelectedValue);
                    }
                }
            }

            string eventArgument = Request["__EVENTARGUMENT"];

            if (eventTarget == "ShowDesignIndicatorSummaryModal" || IsQuickLinkDesignEventTarget(eventTarget))
            {
                PrepareHomeQuickLinkPopupFlow();

                BindGridView_design_summary();
                ShowDesignIndicatorSummaryModal();
            }

            else if (eventTarget == "ShowModal" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;
                BindModalGrid(eventArgument);
                ShowModal();
            }
            else if (eventTarget == "ShowIngredientIndicatorSummaryModal" || IsQuickLinkIngredientEventTarget(eventTarget))
            {
                PrepareHomeQuickLinkPopupFlow();

                BindGridView_component_summary();
                ShowIngredientIndicatorSummaryModal();
            }
            else if (eventTarget == "ShowModal2" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;
                BindModalGrid2(eventArgument);
                ShowModal2();
            }
            else if (eventTarget == "ShowOemIndicatorSummaryModal" || IsQuickLinkOemEventTarget(eventTarget))
            {
                PrepareHomeQuickLinkPopupFlow();

                BindGridView_oem_summary();
                ShowOemIndicatorSummaryModal();
            }
            else if (eventTarget == "ShowDriverIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                HiddenDriverName.Value = eventArgument;
                Session["DriverName"] = eventArgument; // persist for export

                BindDriverIssues(eventArgument);
                ShowDriverIssues(); // opens the drivers modal
            }
            else if (eventTarget == "ExportDriverIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                string driver = Session["DriverName"] as string ?? eventArgument;
                if (!string.IsNullOrWhiteSpace(driver))
                {
                    HiddenDriverName.Value = driver;
                    ExportDriverIssues(driver);
                    return;
                }
            }
            else if (eventTarget == "ShowCMFIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                string[] parts = eventArgument.Split('|');
                if (parts.Length == 3)
                {
                    string component = parts[0];
                    string driver = parts[1];
                    string issueType = parts[2];

                    HiddenComponentName.Value = component;
                    HiddenDriverName.Value = driver;
                    HiddenIssueType.Value = issueType;

                    Session["ComponentName"] = component;
                    Session["DriverName"] = driver;
                    Session["IssueType"] = issueType;

                    BindCMFIssues(component, driver, issueType);
                    ShowCMFIssues();
                }
            }
            else if (eventTarget == "ShowPendingSummaryIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                BindPendingSummaryIssues(eventArgument);
                ShowCMFIssues();
            }
            else if (eventTarget == "SetPendingSummaryAgeThreshold")
            {
                int thresholdDays = ParsePendingSummaryAgeThresholdDaysFromArgument(eventArgument);
                SetPendingSummaryAgeThresholdDays(thresholdDays);

                string activeFocusedTab = GetActiveFocusedTab();
                if (string.Equals(activeFocusedTab, "home", StringComparison.OrdinalIgnoreCase))
                {
                    ShowWelcomeHome();
                }
                else if (string.Equals(activeFocusedTab, "cmf", StringComparison.OrdinalIgnoreCase))
                {
                    ShowModernCmfSummary();
                }
                else
                {
                    RebindFocusedTabData(string.Equals(activeFocusedTab, "reports", StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (eventTarget == "ShowSummaryMetricIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                BindSummaryMetricIssues(eventArgument);
                ShowSummaryMetricIssuesModal();
            }
            else if (eventTarget == "ShowPendingAgeBucketIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                BindPendingAgeBucketIssues(eventArgument);
                ShowCMFIssues();
            }
            else if (eventTarget == "ExportCMFIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                string[] parts = eventArgument.Split('|');
                if (parts.Length == 3)
                {
                    string component = parts[0];
                    string driver = parts[1];
                    string issueType = parts[2];

                    HiddenComponentName.Value = component;
                    HiddenDriverName.Value = driver;
                    HiddenIssueType.Value = issueType;

                    ExportCMFIssues(component, driver, issueType);
                    return;
                }
            }
            else if (eventTarget == "ShowModal3" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;

                BindModalGrid3(eventArgument);
                ShowModal3();
            }
            else if (eventTarget == "ShowDriverModal" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                string[] args = eventArgument.Split(',');
                if (args.Length == 2)
                {
                    string design = args[0];
                    string driver = args[1];
                    Session["DesignValue"] = design;

                    BindDriverDetailsModal(design, driver);
                    ShowModal0();
                }
            }
            else if (eventTarget == "ShowImplementedVerifiedModal" && !string.IsNullOrEmpty(eventArgument))
            {
                PrepareHomeQuickLinkPopupFlow();

                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;

                BindImplementedVerifiedDetailsModal(eventArgument);
                ShowModal0();
            }
            else if (eventTarget == "ExportToExcel" && !string.IsNullOrEmpty(eventArgument))
            {
                string designValue = Session["DesignValue"] as string;
                if (!string.IsNullOrEmpty(designValue))
                {
                    HiddenModalDesign.Value = designValue;
                    ExportToExcel(designValue);
                }
            }
            else if (eventTarget == "ExportToExcel2" && !string.IsNullOrEmpty(eventArgument))
            {
                string designValue = Session["DesignValue"] as string;
                if (!string.IsNullOrEmpty(designValue))
                {
                    HiddenModalDesign.Value = designValue;
                    ExportToExcel2(designValue);
                }
            }
            else if (eventTarget == "ExportToExcel3" && !string.IsNullOrEmpty(eventArgument))
            {
                string designValue = Session["DesignValue"] as string;
                if (!string.IsNullOrEmpty(designValue))
                {
                    HiddenModalDesign.Value = designValue;
                    ExportToExcel3(designValue);
                }
            }
            else if (eventTarget == "SaveDesignSummaryInline")
            {
                string inlineDesignName = HiddenInlineDesignName != null ? HiddenInlineDesignName.Value : string.Empty;
                string inlineSwImageFreeze = HiddenInlineSwImageFreeze != null ? HiddenInlineSwImageFreeze.Value : string.Empty;
                string inlineSupportModel = HiddenInlineSupportModel != null ? HiddenInlineSupportModel.Value : string.Empty;

                string inlineSaveMessage = SaveDesignSummaryInlineEdit(inlineDesignName, inlineSwImageFreeze, inlineSupportModel);

                string activeFocusedTab = GetActiveFocusedTab();
                if (string.Equals(activeFocusedTab, "home", StringComparison.OrdinalIgnoreCase))
                {
                    ShowWelcomeHome();
                }
                else if (string.Equals(activeFocusedTab, "cmf", StringComparison.OrdinalIgnoreCase))
                {
                    ShowModernCmfSummary();
                }
                else
                {
                    BindHomeDashboard();
                }

                if (!string.IsNullOrWhiteSpace(inlineSaveMessage))
                {
                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "designInlineSaveToast",
                        "if (typeof showPortalToast === 'function') { showPortalToast('" + HttpUtility.JavaScriptStringEncode(inlineSaveMessage) + "'); }",
                        true);
                }

                if (HiddenInlineDesignName != null) HiddenInlineDesignName.Value = string.Empty;
                if (HiddenInlineSwImageFreeze != null) HiddenInlineSwImageFreeze.Value = string.Empty;
                if (HiddenInlineSupportModel != null) HiddenInlineSupportModel.Value = string.Empty;
            }
        }
    }

    // Keep your existing BuildIdstClause and BindGridView methods unchanged
    private static string BuildIdstClause(string idstFilter)
    {
        if (string.IsNullOrWhiteSpace(idstFilter) || idstFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return " AND LTRIM(RTRIM(idst)) = @IdstFilter ";
    }

    //private void BindGridView(string filterValue = null, Dictionary<string, string> columnFilters = null)
    //{
    //    if (columnFilters == null)
    //        columnFilters = GetAllFilterValues();

    //    string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
    //    using (SqlConnection con = new SqlConnection(connectionString))
    //    {
    //        con.Open();

    //        // First Load Condition
    //        if (Session["isFirstLoad"] == null)
    //        {
    //            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";
    //            using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
    //            {
    //                SqlDataReader reader = cmd.ExecuteReader();
    //                List<string> driversList = new List<string>();

    //                while (reader.Read())
    //                {
    //                    driversList.Add(reader["drivers"].ToString());
    //                }
    //                reader.Close();

    //                if (driversList.Count > 0)
    //                {
    //                    Session["filterValue"] = string.Join(",", driversList);
    //                    filterValue = Session["filterValue"].ToString();
    //                }
    //            }
    //        }

    //        // Build base query
    //        string base_master_query = "SELECT progress, cp_id As SightingID, title , component , customer_owner As Owner, owners_name, repro_on_rvp, processor, cmf_status As Status, idst, los, drivers AS Driver, days_active, CASE WHEN cmf_request = 'cmf_duplicate' THEN merge_id ELSE promoted_id END AS Merged_PromotedID , cmf_request, customer_company, customer_detail, component_group,customer_affected, impact, closed_reason + CHAR(13) + CHAR(10) +  CASE WHEN status = '/*rejected*/' THEN 'NA' ELSE fixed_in_version END AS ClosedDetails FROM " + selectedPlatform;

    //        // Collect DISTINCT aliases for owner processing
    //        SqlDataAdapter masterAdapt = new SqlDataAdapter(base_master_query, con);
    //        DataTable dt_master = new DataTable();
    //        masterAdapt.Fill(dt_master);

    //        HashSet<string> aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    //        foreach (DataRow r in dt_master.Rows)
    //        {
    //            if (r["Owner"] == DBNull.Value) continue;
    //            string raw = r["Owner"].ToString().Trim();
    //            if (string.IsNullOrEmpty(raw)) continue;

    //            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //            if (raw.Contains("@")) raw = raw.Split('@')[0];
    //            raw = raw.Trim();

    //            if (!string.IsNullOrEmpty(raw))
    //                aliasSet.Add(raw);

    //            if (r["Owners_name"] == DBNull.Value) continue;
    //            raw = r["owners_name"].ToString().Trim();
    //            if (string.IsNullOrEmpty(raw)) continue;

    //            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //            if (raw.Contains("@")) raw = raw.Split('@')[0];
    //            raw = raw.Trim();

    //            if (!string.IsNullOrEmpty(raw))
    //                aliasSet.Add(raw);
    //        }

    //        // Create AD contexts
    //        PrincipalContext context_gar = null;
    //        PrincipalContext context_amr = null;
    //        PrincipalContext context_ccr = null;
    //        PrincipalContext context_ger = null;

    //        try { context_gar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
    //        try { context_amr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
    //        try { context_ccr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
    //        try { context_ger = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

    //        Dictionary<string, string> ownerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    //        foreach (string alias in aliasSet)
    //        {
    //            string displayName = "Unable to fetch";
    //            UserPrincipal up = null;

    //            if (context_gar != null) up = UserPrincipal.FindByIdentity(context_gar, IdentityType.SamAccountName, alias);
    //            if (up == null && context_amr != null) up = UserPrincipal.FindByIdentity(context_amr, IdentityType.SamAccountName, alias);
    //            if (up == null && context_ccr != null) up = UserPrincipal.FindByIdentity(context_ccr, IdentityType.SamAccountName, alias);
    //            if (up == null && context_ger != null) up = UserPrincipal.FindByIdentity(context_ger, IdentityType.SamAccountName, alias);

    //            if (up != null) displayName = up.DisplayName;

    //            ownerMap[alias] = displayName;
    //        }

    //        // Build dynamic master_query
    //        string master_query = base_master_query;
    //        string order_clause = "ORDER BY \r\n    CASE \r\n        WHEN progress = 'Green' THEN 1  -- Give 'green' a lower priority, so it appears later\r\n        ELSE 0  -- Non-green rows get a higher priority (appear first)\r\n    END,\r\n    Driver, \r\n    CASE \r\n        WHEN progress = 'Red' THEN 1\r\n        WHEN progress = 'Orange' THEN 2\r\n        WHEN progress = 'Yellow' THEN 3\r\n        WHEN progress = 'Green' THEN 4  -- Green comes last, after all other colors\r\n    END,\r\n    component;\r\n ";

    //        // Build WHERE clause - FIXED: Use consistent status filtering
    //        string whereClause = "";

    //        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
    //        {
    //            if (filterValue.Contains(","))
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n" +
    //                "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR ( " +
    //                "       @FilterValue LIKE '%,' + drivers + ',%' " +
    //                "       OR @FilterValue LIKE drivers + ',%' " +
    //                "       OR @FilterValue LIKE '%,' + drivers) ) " +
    //                "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
    //            }
    //            else
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR drivers = @FilterValue ) AND status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
    //            }
    //        }
    //        else
    //        {
    //            // Handle AllDrivers case
    //            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";
    //            using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
    //            {
    //                SqlDataReader reader = cmd.ExecuteReader();
    //                List<string> driversList = new List<string>();

    //                while (reader.Read())
    //                {
    //                    driversList.Add(reader["drivers"].ToString());
    //                }
    //                reader.Close();

    //                if (driversList.Count > 0)
    //                {
    //                    Session["filterValue"] = string.Join(",", driversList);
    //                    filterValue = Session["filterValue"].ToString();
    //                }
    //            }

    //            if (!string.IsNullOrEmpty(filterValue) && filterValue.Contains(","))
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n" +
    //                "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR ( " +
    //                "       @FilterValue LIKE '%,' + drivers + ',%' " +
    //                "       OR @FilterValue LIKE drivers + ',%' " +
    //                "       OR @FilterValue LIKE '%,' + drivers) ) " +
    //                "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
    //            }
    //            else if (string.IsNullOrEmpty(filterValue))
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
    //            }
    //            else
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR drivers = @FilterValue ) AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
    //            }
    //        }

    //        // Add column filters
    //        whereClause += BuildFilterClauses(columnFilters);

    //        master_query += whereClause + order_clause;

    //        // Execute final query
    //        using (SqlCommand cmd = new SqlCommand(master_query, con))
    //        {
    //            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
    //            {
    //                if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
    //                    sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

    //                // Add column filter parameters
    //                foreach (var filter in columnFilters)
    //                {
    //                    if (!string.IsNullOrWhiteSpace(filter.Value) && !filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        switch (filter.Key)
    //                        {
    //                            case "owner":
    //                                sda.SelectCommand.Parameters.AddWithValue("@OwnerFilter", filter.Value);
    //                                break;
    //                            case "rvpRepro":
    //                                sda.SelectCommand.Parameters.AddWithValue("@RvpReproFilter", filter.Value);
    //                                break;
    //                            case "idst":
    //                                sda.SelectCommand.Parameters.AddWithValue("@IdstFilter", filter.Value);
    //                                break;
    //                            case "los":
    //                                sda.SelectCommand.Parameters.AddWithValue("@LosFilter", filter.Value);
    //                                break;
    //                            case "milestone":
    //                                sda.SelectCommand.Parameters.AddWithValue("@MilestoneFilter", filter.Value);
    //                                break;
    //                            case "Company":
    //                                sda.SelectCommand.Parameters.AddWithValue("@CompanyFilter", filter.Value);
    //                                break;
    //                            case "Detail":
    //                                sda.SelectCommand.Parameters.AddWithValue("@DetailFilter", filter.Value);
    //                                break;
    //                            case "Component":
    //                                sda.SelectCommand.Parameters.AddWithValue("@ComponentFilter", filter.Value);
    //                                break;
    //                            case "cmfRequest":
    //                                sda.SelectCommand.Parameters.AddWithValue("@CmfRequestFilter", filter.Value);
    //                                break;
    //                        }
    //                    }
    //                }

    //                DataTable dt = new DataTable();
    //                sda.Fill(dt);

    //                // Process data rows
    //                foreach (DataRow row in dt.Rows)
    //                {
    //                    // Replace Owner with actual names
    //                    string raw = (row["Owner"] == DBNull.Value) ? string.Empty : row["Owner"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(raw))
    //                    {
    //                        row["Owner"] = "Unable to fetch";
    //                    }
    //                    else
    //                    {
    //                        if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //                        if (raw.Contains("@")) raw = raw.Split('@')[0];
    //                        raw = raw.Trim();

    //                        string actualName;
    //                        if (ownerMap.TryGetValue(raw, out actualName))
    //                            row["Owner"] = actualName;
    //                        else
    //                            row["Owner"] = "Unable to fetch";
    //                    }

    //                    raw = (row["owners_name"] == DBNull.Value) ? string.Empty : row["owners_name"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(raw))
    //                    {
    //                        row["owners_name"] = "Unable to fetch";
    //                    }
    //                    else
    //                    {
    //                        if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //                        if (raw.Contains("@")) raw = raw.Split('@')[0];
    //                        raw = raw.Trim();

    //                        string actualName;
    //                        if (ownerMap.TryGetValue(raw, out actualName))
    //                            row["Owners_name"] = actualName;
    //                        else
    //                            row["owners_name"] = "Unable to fetch";
    //                    }

    //                    // Handle LOS - Set to "No" if blank/null
    //                    string losValue = (row["los"] == DBNull.Value) ? string.Empty : row["los"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(losValue))
    //                    {
    //                        row["los"] = "No";
    //                    }

    //                    // Handle ClosedDetails - Set to "NA" if blank/null
    //                    string closedDetailsValue = (row["ClosedDetails"] == DBNull.Value) ? string.Empty : row["ClosedDetails"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(closedDetailsValue))
    //                    {
    //                        row["ClosedDetails"] = "NA";
    //                    }
    //                }

    //                // Bind
    //                overall_request_details.DataSource = dt;
    //                overall_request_details.DataKeyNames = new string[] { "SightingID" };
    //                overall_request_details.DataBind();

    //                BindGridView_design_open();
    //                BindGridView_cmf_summary();
    //                BindGridView_cmf_pending();
    //                BindGridView_design_summary();
    //                BindGridView_component_summary();
    //                BindGridView_oem_summary();
    //            }
    //        }
    //    }
    //}

    private void BindGridView(string filterValue = null, Dictionary<string, string> columnFilters = null, bool bindRelatedGrids = true)
    {
        if (columnFilters == null)
            columnFilters = GetAllFilterValues();

        string platformSelection = ResolveIssuePendingPlatformSelection(selectedPlatform);
        bool isOverallSelection = IsOverallDashboardPlatform(platformSelection);
        string platformTable = isOverallSelection ? null : ResolvePlatformTable(platformSelection);
        string basePlatform = isOverallSelection ? string.Empty : platformTable.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = isOverallSelection ? string.Empty : basePlatform + "_DESIGN_TABLE";

        using (SqlConnection con = new SqlConnection(ConnectionString))
        {
            con.Open();
            EnsurePublicIssueDriverFilter(con, platformSelection);
            filterValue = Session["filterValue"] as string;

            //            string base_master_query = @"
            //SELECT 
            //    progress, 
            //    cp_id As SightingID, 
            //    title, 
            //    component, 
            //    customer_owner As Owner, 
            //    owners_name, 
            //    repro_on_rvp, 
            //    processor, 
            //    cmf_status As Status, 
            //    idst, 
            //    los, 
            //    drivers AS Driver, 
            //    days_active, 
            //    CASE WHEN cmf_request = 'cmf_duplicate' THEN merge_id ELSE promoted_id END AS Merged_PromotedID,
            //    cmf_request, 
            //    customer_company, 
            //    customer_detail, 
            //    component_group,
            //    customer_affected, 
            //    impact, 
            //    closed_reason + CHAR(13) + CHAR(10) + CASE WHEN status = 'rejected' THEN 'NA' ELSE fixed_in_version END AS ClosedDetails,
            //    -- NEW: Duplicate Details column with HSD ID and Customer Company
            //    STUFF((
            //        SELECT ', '
            //       + CAST(dup.cp_id AS VARCHAR)
            //       + '|' + ISNULL(dup.customer_detail, 'Unknown')
            //       + '|' + ISNULL(CONVERT(VARCHAR(10), d2.sw_image_freeze, 120), 'Unknown')
            //FROM " + selectedPlatform + @" dup
            //LEFT JOIN " + designTable + @" AS d2
            //       ON d2.customer_detail = dup.customer_detail
            //WHERE dup.cmf_request = 'cmf_duplicate'
            //  AND dup.status = 'open'
            //  AND dup.merge_id = main.cp_id
            //FOR XML PATH('')
            //    ), 1, 2, '') AS DuplicateDetails
            //FROM " + selectedPlatform + @" main";

            //string base_master_query = @"
            //SELECT 
            //    main.progress, 
            //    main.cp_id As SightingID, 
            //    main.title, 
            //    main.component, 
            //    main.customer_owner As Owner, 
            //    main.owners_name, 
            //    main.repro_on_rvp, 
            //    main.processor, 
            //    main.cmf_status As Status, 
            //    main.idst, 
            //    main.los, 
            //    main.drivers AS Driver, 
            //    main.days_active, 
            //    CASE WHEN main.cmf_request = 'cmf_duplicate' THEN main.merge_id ELSE main.promoted_id END AS Merged_PromotedID,
            //    main.cmf_request, 
            //    main.customer_company, 
            //    main.customer_detail, 
            //    main.component_group,
            //    main.customer_affected, 
            //    main.impact, 
            //    main.closed_reason + CHAR(13) + CHAR(10) + CASE WHEN main.status = 'rejected' THEN 'NA' ELSE main.fixed_in_version END AS ClosedDetails,

            //    -- NEW: bring sw_image_freeze from design table
            //    d.sw_image_freeze AS ImageFreeze,

            //    -- Duplicate Details column with HSD ID and Customer Company
            //    STUFF((
            //                        SELECT ', '
            //           + CAST(dup.cp_id AS VARCHAR)
            //           + '|' + ISNULL(dup.customer_detail, 'Unknown')
            //           + '|' + ISNULL(CONVERT(VARCHAR(10), d2.ImageFreeze, 120), 'Unknown')
            //    FROM " + selectedPlatform + @" dup
            //    LEFT JOIN " + designTable + @" AS d2
            //           ON d2.customer_detail = dup.customer_detail
            //    WHERE dup.cmf_request = 'cmf_duplicate'
            //      AND dup.status = 'open'
            //      AND dup.merge_id = main.cp_id
            //    FOR XML PATH('')
            //    ), 1, 2, '') AS DuplicateDetails
            //FROM " + selectedPlatform + @" AS main
            //LEFT JOIN " + designTable + @" AS d
            //    ON d.customer_detail = main.customer_detail";

            string base_master_query;
            if (isOverallSelection)
            {
                string issueCteSql = BuildIssueDataCteSql(platformSelection);
                base_master_query = @"
WITH issue_source AS (
" + issueCteSql + @"
)
SELECT 
    main.progress, 
    main.cp_id As SightingID, 
    main.title, 
    main.component, 
    main.customer_owner As Owner, 
    main.owners_name, 
    main.repro_on_rvp, 
    main.processor, 
    main.cmf_status As Status, 
    main.status AS IssueStatus,
    TRY_CONVERT(date, main.date_cmf_ask) AS SubmittedDate,
    main.sysdebug,
    main.idst, 
    main.los, 
    main.drivers AS Driver, 
    main.must_fix_for AS MustFixFor,
    main.days_active, 
    CASE WHEN main.cmf_request = 'cmf_duplicate' THEN main.merge_id ELSE main.promoted_id END AS Merged_PromotedID,
    main.cmf_request, 
    main.customer_company, 
    main.customer_detail, 
    main.component_group,
    main.customer_affected, 
    main.impact, 
    main.closed_reason + CHAR(13) + CHAR(10) + CASE WHEN main.status = 'rejected' THEN 'NA' ELSE main.fixed_in_version END AS ClosedDetails,
    CAST(NULL AS DATETIME) AS ImageFreeze,
    STUFF((
        SELECT ', '
             + CAST(dup.cp_id AS VARCHAR)
             + '|' + ISNULL(dup.customer_detail, 'Unknown')
             + '|Unknown'
        FROM issue_source AS dup
        WHERE dup.cmf_request = 'cmf_duplicate'
          AND dup.merge_id = main.cp_id
          AND dup.source_platform = main.source_platform
        FOR XML PATH('')
    ), 1, 2, '') AS DuplicateDetails
FROM issue_source AS main";
            }
            else
            {
                base_master_query = @"
SELECT 
    main.progress, 
    main.cp_id As SightingID, 
    main.title, 
    main.component, 
    main.customer_owner As Owner, 
    main.owners_name, 
    main.repro_on_rvp, 
    main.processor, 
    main.cmf_status As Status, 
    main.status AS IssueStatus,
    TRY_CONVERT(date, main.date_cmf_ask) AS SubmittedDate,
    main.sysdebug,
    main.idst, 
    main.los, 
    main.drivers AS Driver, 
    main.must_fix_for AS MustFixFor,
    main.days_active, 
    CASE WHEN main.cmf_request = 'cmf_duplicate' THEN main.merge_id ELSE main.promoted_id END AS Merged_PromotedID,
    main.cmf_request, 
    main.customer_company, 
    main.customer_detail, 
    main.component_group,
    main.customer_affected, 
    main.impact, 
    main.closed_reason + CHAR(13) + CHAR(10) + CASE WHEN main.status = 'rejected' THEN 'NA' ELSE main.fixed_in_version END AS ClosedDetails,

    -- Expose a single field name to the UI
    d.sw_image_freeze AS ImageFreeze,

    -- Duplicate Details: HSD|CustomerDetail|ImageFreezeDate
    STUFF((
        SELECT ', '
             + CAST(dup.cp_id AS VARCHAR)
             + '|' + ISNULL(dup.customer_detail, 'Unknown')
             + '|' + ISNULL(CONVERT(VARCHAR(10), d2.sw_image_freeze, 120), 'Unknown')
        FROM " + platformTable + @" dup
        LEFT JOIN " + designTable + @" AS d2
               ON d2.customer_detail = dup.customer_detail
        WHERE dup.cmf_request = 'cmf_duplicate'
          AND dup.merge_id = main.cp_id
        FOR XML PATH('')
    ), 1, 2, '') AS DuplicateDetails
FROM " + platformTable + @" AS main
LEFT JOIN " + designTable + @" AS d
    ON d.customer_detail = main.customer_detail";
            }

            // ownerMap is populated after the filtered query (see below)
            Dictionary<string, string> ownerMap = null;

            // Build dynamic master_query
            string master_query = base_master_query;
            string order_clause = "ORDER BY \r\n    CASE WHEN LTRIM(RTRIM(ISNULL(main.drivers, ''))) = '' THEN 1 ELSE 0 END,\r\n    CASE \r\n        WHEN progress = 'Green' THEN 1  -- Give 'green' a lower priority, so it appears later\r\n        ELSE 0  -- Non-green rows get a higher priority (appear first)\r\n    END,\r\n    Driver, \r\n    CASE \r\n        WHEN progress = 'Red' THEN 1\r\n        WHEN progress = 'Orange' THEN 2\r\n        WHEN progress = 'Yellow' THEN 3\r\n        WHEN progress = 'Green' THEN 4  -- Green comes last, after all other colors\r\n    END,\r\n    component;\r\n ";

            ApplyIssuePageSizeFromSession();

            // Keep the same status scope on initial loads and postbacks.
            string whereClause = "";
            string issueStatusClause = isOverallSelection
                ? "(status IN ('open', 'implemented') OR (main.source_platform IN ('CMF_NVL_H_ALL_COMPONENTS_TABLE', 'CMF_NVL_S_ALL_COMPONENTS_TABLE') AND status NOT IN ('rejected')))"
                : UsesPublicIssueStatusScope(platformTable)
                    ? "status NOT IN ('rejected')"
                    : "status IN ('open', 'implemented')";

            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                if (filterValue.Contains(","))
                {
                    whereClause = " WHERE \r\n" +
                    "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR ( " +
                    "       @FilterValue LIKE '%,' + drivers + ',%' " +
                    "       OR @FilterValue LIKE drivers + ',%' " +
                    "       OR @FilterValue LIKE '%,' + drivers) ) " +
                    "AND sysdebug Like ('%customer_must_fix%') AND " + issueStatusClause + " AND cmf_request in ('cmf_ok') ";
                }
                else
                {
                    whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR drivers = @FilterValue ) AND " + issueStatusClause + " AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filterValue) && filterValue.Contains(","))
                {
                    whereClause = " WHERE \r\n" +
                    "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR ( " +
                    "       @FilterValue LIKE '%,' + drivers + ',%' " +
                    "       OR @FilterValue LIKE drivers + ',%' " +
                    "       OR @FilterValue LIKE '%,' + drivers) ) " +
                    "AND sysdebug Like ('%customer_must_fix%') AND status NOT IN ('rejected') AND cmf_request in ('cmf_ok') ";
                }
                else if (string.IsNullOrEmpty(filterValue))
                {
                    whereClause = " WHERE status NOT IN ('rejected') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
                }
                else
                {
                    whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR drivers = @FilterValue ) AND sysdebug Like ('%customer_must_fix%') AND status not in ( 'rejected') AND cmf_request in ('cmf_ok') ";
                }
            }
            // Add column filters
            whereClause += BuildFilterClauses(columnFilters, "main");

            master_query += whereClause + order_clause;

            // Execute final query
            using (SqlCommand cmd = new SqlCommand(master_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
                        sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

                    // Add column filter parameters
                    foreach (var filter in columnFilters)
                    {
                        if (!string.IsNullOrWhiteSpace(filter.Value) && !filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
                        {
                            switch (filter.Key)
                            {
                                case "owner":
                                    sda.SelectCommand.Parameters.AddWithValue("@OwnerFilter", filter.Value);
                                    break;
                                case "rvpRepro":
                                    sda.SelectCommand.Parameters.AddWithValue("@RvpReproFilter", filter.Value);
                                    break;
                                case "idst":
                                    sda.SelectCommand.Parameters.AddWithValue("@IdstFilter", filter.Value);
                                    break;
                                case "los":
                                    sda.SelectCommand.Parameters.AddWithValue("@LosFilter", filter.Value);
                                    break;
                                case "milestone":
                                    sda.SelectCommand.Parameters.AddWithValue("@MilestoneFilter", filter.Value);
                                    break;
                                case "Company":
                                    sda.SelectCommand.Parameters.AddWithValue("@CompanyFilter", filter.Value);
                                    break;
                                case "Detail":
                                    sda.SelectCommand.Parameters.AddWithValue("@DetailFilter", filter.Value);
                                    break;
                                case "Component":
                                    sda.SelectCommand.Parameters.AddWithValue("@ComponentFilter", filter.Value);
                                    break;
                                case "IssueStatus":
                                    sda.SelectCommand.Parameters.AddWithValue("@IssueStatusFilter", filter.Value);
                                    break;
                            }
                        }
                    }

                    DataTable dt = new DataTable();
                    sda.Fill(dt);

                    ownerMap = ResolveOwnerDisplayNamesWithCache(dt, new[] { "Owner", "owners_name" });

                    // Add DuplicateDetails column if it doesn't exist
                    if (!dt.Columns.Contains("DuplicateDetails"))
                    {
                        dt.Columns.Add("DuplicateDetails", typeof(string));
                    }

                    // Process data rows
                    foreach (DataRow row in dt.Rows)
                    {
                        // Replace Owner with actual names
                        string raw = (row["Owner"] == DBNull.Value) ? string.Empty : row["Owner"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw))
                        {
                            row["Owner"] = "N/A";
                        }
                        else
                        {
                            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                            if (raw.Contains("@")) raw = raw.Split('@')[0];
                            raw = raw.Trim();

                            string actualName;
                            if (ownerMap.TryGetValue(raw, out actualName))
                                row["Owner"] = actualName;
                            else
                                row["Owner"] = raw;
                        }

                        raw = (row["owners_name"] == DBNull.Value) ? string.Empty : row["owners_name"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw))
                        {
                            row["owners_name"] = "N/A";
                        }
                        else
                        {
                            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                            if (raw.Contains("@")) raw = raw.Split('@')[0];
                            raw = raw.Trim();

                            string actualName;
                            if (ownerMap.TryGetValue(raw, out actualName))
                                row["Owners_name"] = actualName;
                            else
                                row["owners_name"] = raw;
                        }

                        // Handle LOS - Set to "No" if blank/null
                        string losValue = (row["los"] == DBNull.Value) ? string.Empty : row["los"].ToString().Trim();
                        if (string.IsNullOrEmpty(losValue))
                        {
                            row["los"] = "No";
                        }

                        // Handle ClosedDetails - Set to "NA" if blank/null
                        string closedDetailsValue = (row["ClosedDetails"] == DBNull.Value) ? string.Empty : row["ClosedDetails"].ToString().Trim();
                        if (string.IsNullOrEmpty(closedDetailsValue))
                        {
                            row["ClosedDetails"] = "NA";
                        }
                    }

                    dt = ApplyIssueGlobalSearch(dt);
                    BindIssueGridFromDataTable(dt, bindRelatedGrids);
                    CacheIssueGridData(filterValue, columnFilters, dt);
                }
            }
        }
    }

    protected void btnIssueGlobalSearchApply_Click(object sender, EventArgs e)
    {
        ApplyIssuePendingPlatformContext();
        EnsureIssueTabVisibleForPostback();
        SetActiveFocusedTab("issue");
        Session[IssueGlobalSearchSessionKey] = (hfIssueGlobalSearch.Value ?? string.Empty).Trim();
        overall_request_details.PageIndex = 0;
        ApplyIssuePageSizeFromSession();

        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        BindGridView(filterValue, filters, bindRelatedGrids: false);
    }

    protected void overall_request_details_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        ApplyIssuePendingPlatformContext();
        EnsureIssueTabVisibleForPostback();
        ApplyIssuePageSizeFromSession();
        overall_request_details.PageIndex = e.NewPageIndex;
        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        if (!TryBindIssueGridFromCache(filterValue, filters))
        {
            BindGridView(filterValue, filters, bindRelatedGrids: false);
        }
    }

    protected void ddlIssuePageSize_SelectedIndexChanged(object sender, EventArgs e)
    {
        ApplyIssuePendingPlatformContext();
        EnsureIssueTabVisibleForPostback();
        Session["issuePageSize"] = ddlIssuePageSize.SelectedValue;
        overall_request_details.PageIndex = 0;
        ApplyIssuePageSizeFromSession();

        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        if (!TryBindIssueGridFromCache(filterValue, filters))
        {
            BindGridView(filterValue, filters, bindRelatedGrids: false);
        }
    }

    protected void rptPageNumbers_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "SelectPage")
        {
            ApplyIssuePendingPlatformContext();
            EnsureIssueTabVisibleForPostback();
            ApplyIssuePageSizeFromSession();
            int pageNumber = int.Parse(e.CommandArgument.ToString());
            overall_request_details.PageIndex = pageNumber - 1;

            string filterValue = Session["filterValue"] as string;
            Dictionary<string, string> filters = GetAllFilterValues();
            if (!TryBindIssueGridFromCache(filterValue, filters))
            {
                BindGridView(filterValue, filters, bindRelatedGrids: false);
            }
        }
    }

    protected void btnPageGroupPrev_Click(object sender, EventArgs e)
    {
        ApplyIssuePendingPlatformContext();
        EnsureIssueTabVisibleForPostback();
        ApplyIssuePageSizeFromSession();
        const int pagesPerGroup = 10;
        int currentPage = overall_request_details.PageIndex + 1;
        int currentGroupStartPage = ((currentPage - 1) / pagesPerGroup) * pagesPerGroup + 1;
        int prevGroupLastPage = currentGroupStartPage - 1;
        
        if (prevGroupLastPage >= 1)
        {
            overall_request_details.PageIndex = prevGroupLastPage - 1;
        }

        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        if (!TryBindIssueGridFromCache(filterValue, filters))
        {
            BindGridView(filterValue, filters, bindRelatedGrids: false);
        }
    }

    protected void btnPageGroupNext_Click(object sender, EventArgs e)
    {
        ApplyIssuePendingPlatformContext();
        EnsureIssueTabVisibleForPostback();
        ApplyIssuePageSizeFromSession();
        const int pagesPerGroup = 10;
        int currentPage = overall_request_details.PageIndex + 1;
        int currentGroupStartPage = ((currentPage - 1) / pagesPerGroup) * pagesPerGroup + 1;
        int nextGroupFirstPage = currentGroupStartPage + pagesPerGroup;
        
        if (nextGroupFirstPage <= overall_request_details.PageCount)
        {
            overall_request_details.PageIndex = nextGroupFirstPage - 1;
        }

        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        if (!TryBindIssueGridFromCache(filterValue, filters))
        {
            BindGridView(filterValue, filters, bindRelatedGrids: false);
        }
    }

    protected string CreateDuplicateLinks(object duplicateDetails)
    {
        if (duplicateDetails == null || duplicateDetails == DBNull.Value)
            return "NA";

        string details = duplicateDetails.ToString().Trim();
        if (string.IsNullOrEmpty(details))
            return "NA";

        // Split by comma and create hyperlinks for each duplicate with customer detail + image freeze date
        string[] duplicateEntries = details.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        List<string> links = new List<string>();

        foreach (string entry in duplicateEntries)
        {
            string trimmedEntry = entry.Trim();
            if (!string.IsNullOrEmpty(trimmedEntry))
            {
                string[] parts = trimmedEntry.Split('|');

                string hsdId = parts.Length > 0 ? parts[0].Trim() : "";
                string customerDetail = parts.Length > 1 ? (string.IsNullOrWhiteSpace(parts[1]) ? "Unknown" : parts[1].Trim()) : "Unknown";
                string freezeDate = parts.Length > 2 ? (string.IsNullOrWhiteSpace(parts[2]) ? "Unknown" : parts[2].Trim()) : "Unknown";

                if (!string.IsNullOrEmpty(hsdId))
                {
                    string link = string.Format(
                        "<a href='https://hsdes.intel.com/appstore/article/#{0}' target='_blank' title='Customer: {1} | ImageFreeze: {2}'>{0} | {1} | {2}</a>",
                        hsdId,
                        System.Web.HttpUtility.HtmlEncode(customerDetail),
                        System.Web.HttpUtility.HtmlEncode(freezeDate)
                    );
                    links.Add(link);
                }
            }
        }

        return links.Count > 0 ? string.Join(", ", links) : "NA";
    }

    protected string RenderIssueDetails(object sightingIdValue, object promotedIdValue, object titleValue, object cmfRequestValue, object submittedDateValue, object statusValue, object sysdebugValue, bool includeTitle = true)
    {
        string sightingId = sightingIdValue == null || sightingIdValue == DBNull.Value ? string.Empty : sightingIdValue.ToString().Trim();
        string title = titleValue == null || titleValue == DBNull.Value ? string.Empty : titleValue.ToString().Trim();
        string status = statusValue == null || statusValue == DBNull.Value ? string.Empty : statusValue.ToString().Trim();
        string sysdebug = sysdebugValue == null || sysdebugValue == DBNull.Value ? string.Empty : sysdebugValue.ToString().Replace("\r", " ").Replace("\n", " ").Trim();
        string onclick = "openAiSummaryModal(\"" + JsEncode(sightingIdValue) + "\", \"" + JsEncode(titleValue) + "\", \"" + JsEncode(FormatDateOnly(submittedDateValue)) + "\", \"" + JsEncode(status) + "\", \"" + JsEncode(sysdebug) + "\", \"AI issue details\")";

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"issue-details-cell\">");
        sb.Append("<span class=\"issue-meta-row issue-meta-row-primary\">");
        if (!string.IsNullOrWhiteSpace(sightingId))
        {
            sb.AppendFormat(
                "<a class=\"issue-id-link\" href=\"https://hsdes.intel.com/appstore/article/#{0}\" target=\"_blank\">{1}</a>",
                HttpUtility.HtmlAttributeEncode(sightingId),
                HttpUtility.HtmlEncode(sightingId));
        }
        sb.Append("<button type=\"button\" class=\"ai-summary-btn ai-summary-btn-inline issue-details-ai-btn\" style=\"padding: 2px 8px; border-radius: 4px; display: inline-flex; align-items: center; justify-content: center; gap: 4px; border: 1px solid #7cb5ec; background-color: #f3f9ff; font-size: 14px; width: 26px; height: 26px;\" onclick='" + onclick + "' title=\"AI Issue details\" aria-label=\"AI Issue details\">&#10024;</button>");
        sb.Append("</span>");
        if (includeTitle)
        {
            sb.AppendFormat("<span class=\"issue-title-text\">{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(title) ? "Untitled issue" : title));
        }
        sb.Append("</span>");
        return sb.ToString();
    }

    protected string RenderPendingId(object cpIdValue)
    {
        string cpId = cpIdValue == null || cpIdValue == DBNull.Value ? string.Empty : cpIdValue.ToString().Trim();
        if (string.IsNullOrWhiteSpace(cpId)) return "<span class=\"pending-mini-label\">N/A</span>";
        return string.Format(
            "<a class=\"pending-id-link\" href=\"https://hsdes.intel.com/appstore/article/#{0}\" target=\"_blank\">#{1}</a>",
            HttpUtility.HtmlAttributeEncode(cpId),
            HttpUtility.HtmlEncode(cpId));
    }

    protected string RenderPendingSelector(object cmfRequestValue)
    {
        string cmfRequest = cmfRequestValue == null || cmfRequestValue == DBNull.Value ? string.Empty : cmfRequestValue.ToString().Trim();
        string title = string.IsNullOrWhiteSpace(cmfRequest) ? "Pending issue row" : "Pending issue: " + cmfRequest.Replace('_', ' ');
        return string.Format(
            "<span class=\"pending-select-cell\" title=\"{0}\"><span class=\"pending-row-check\" aria-hidden=\"true\"></span></span>",
            HttpUtility.HtmlAttributeEncode(title));
    }

    protected string RenderPendingIssueDetails(object cpIdValue, object titleValue, object componentValue)
    {
        string title = titleValue == null || titleValue == DBNull.Value ? string.Empty : titleValue.ToString().Trim();
        string component = componentValue == null || componentValue == DBNull.Value ? string.Empty : componentValue.ToString().Trim();

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"pending-issue-cell\">");
        sb.Append(RenderPendingId(cpIdValue));
        sb.AppendFormat("<span class=\"pending-title-text\">{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(title) ? "Untitled pending issue" : title));
        if (!string.IsNullOrWhiteSpace(component))
        {
            sb.AppendFormat("<span class=\"pending-mini-label\">Component</span><span>{0}</span>", HttpUtility.HtmlEncode(component));
        }
        sb.Append("</span>");
        return sb.ToString();
    }

    protected string RenderPendingIssueDetailsWithRecommendation(object cpIdValue, object titleValue, object cmfRequestValue, object componentValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue)
    {
        return "<span class=\"pending-issue-with-action\">" +
            RenderPendingIssueDetails(cpIdValue, titleValue, componentValue) +
            "</span>";
    }

    protected string RenderPendingDecisionDetailsButton(object cpIdValue, object titleValue, object componentValue, object cmfRequestValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue)
    {
        return string.Format(
            "<button type=\"button\" class=\"pending-recommendation-btn pending-decision-details-btn\" onclick='openCmfPendingDetailsModal(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\")' title=\"AI CMF decision details\" aria-label=\"AI CMF decision details\">&#10024;</button>",
            JsEncode(cpIdValue),
            JsEncode(titleValue),
            JsEncode(componentValue),
            JsEncode(cmfRequestValue),
            JsEncode(impactValue),
            JsEncode(idstValue),
            JsEncode(reproOnRvpValue),
            JsEncode(reproducibilityValue),
            JsEncode(customerDetailValue),
            JsEncode(customerOwnerValue));
    }

    protected string RenderPendingRecommendationCell(object cpIdValue, object titleValue, object componentValue, object componentGroupValue, object cmfRequestValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue, object platformValue = null)
    {
        return "<span class=\"pending-ai-rec-cell\">" +
            RenderPendingRecommendationButton(cpIdValue, titleValue, componentValue, componentGroupValue, cmfRequestValue, impactValue, idstValue, reproOnRvpValue, reproducibilityValue, customerDetailValue, customerOwnerValue, platformValue) +
            "</span>";
    }

    protected string RenderPendingRecommendationButton(object cpIdValue, object titleValue, object componentValue, object componentGroupValue, object cmfRequestValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue, object platformValue = null)
    {
        string cpId = cpIdValue == null || cpIdValue == DBNull.Value ? string.Empty : cpIdValue.ToString();
        return string.Format(
            "<button type=\"button\" class=\"pending-recommendation-btn pending-ai-rec-btn\" data-cmf-rec-id=\"{11}\" data-cmf-platform=\"{12}\" onclick='openCmfPendingRecommendationModal(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", this.getAttribute(\"data-cmf-platform\"))' title=\"Open CMF Analysis\" aria-label=\"Open CMF Analysis\"><span class=\"pending-ai-rec-icon\" aria-hidden=\"true\">&#10024;</span><span class=\"pending-ai-rec-label\">CMF Analysis</span></button>",
            JsEncode(cpIdValue),
            JsEncode(titleValue),
            JsEncode(componentValue),
            JsEncode(componentGroupValue),
            JsEncode(cmfRequestValue),
            JsEncode(impactValue),
            JsEncode(idstValue),
            JsEncode(reproOnRvpValue),
            JsEncode(reproducibilityValue),
            JsEncode(customerDetailValue),
            JsEncode(customerOwnerValue),
            HttpUtility.HtmlAttributeEncode(cpId),
            HttpUtility.HtmlAttributeEncode(Convert.ToString(platformValue)));
    }

    protected string RenderSightingDetailsPanel(object cpIdValue, object titleValue, object componentValue, object cmfRequestValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue)
    {
        string cpId = cpIdValue == null || cpIdValue == DBNull.Value ? string.Empty : cpIdValue.ToString().Trim();
        string title = titleValue == null || titleValue == DBNull.Value ? string.Empty : titleValue.ToString();
        string component = componentValue == null || componentValue == DBNull.Value ? string.Empty : componentValue.ToString();

        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"sighting-details-panel\">");

        // Sighting ID
        sb.AppendFormat("<div class=\"sighting-id-section\">");
        sb.AppendFormat("<span class=\"sighting-id-label\">Sighting ID:</span>");
        sb.AppendFormat("<span class=\"sighting-id-value\">{0}</span>", HttpUtility.HtmlEncode(cpId));
        sb.Append("</div>");

        // AI-Generated Description (will be loaded asynchronously)
        sb.AppendFormat("<div class=\"sighting-description-section\">");
        sb.AppendFormat("<span class=\"sighting-description-label\">Description:</span>");
        sb.AppendFormat("<div class=\"sighting-description-content\" id=\"desc-{0}\">", HttpUtility.HtmlAttributeEncode(cpId));
        sb.Append("<span class=\"sighting-description-loading\">Loading...</span>");
        sb.Append("</div>");
        sb.Append("</div>");

        // Hidden container for data attributes
        sb.AppendFormat("<div class=\"sighting-data-container\" data-sighting-id=\"{0}\" data-title=\"{1}\" data-component=\"{2}\" style=\"display:none;\"></div>",
            HttpUtility.HtmlAttributeEncode(cpId),
            HttpUtility.HtmlAttributeEncode(title),
            HttpUtility.HtmlAttributeEncode(component));

        sb.Append("</div>");

        return sb.ToString();
    }

    protected string RenderPendingCustomer(object customerDetailValue, object ownerValue)
    {
        string customerDetail = customerDetailValue == null || customerDetailValue == DBNull.Value ? string.Empty : customerDetailValue.ToString().Trim();
        string owner = ownerValue == null || ownerValue == DBNull.Value ? string.Empty : ownerValue.ToString().Trim();

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"pending-customer-cell\">");
        sb.AppendFormat("<span class=\"pending-mini-label\">Customer</span><span>{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(customerDetail) ? "N/A" : customerDetail.Replace('_', ' ')));
        sb.AppendFormat("<span class=\"pending-mini-label\">Owner</span><span>{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(owner) ? "N/A" : owner));
        sb.Append("</span>");
        return sb.ToString();
    }

    protected string RenderPendingEvidence(object idstValue, object reproOnRvpValue, object reproducibilityValue)
    {
        string idst = idstValue == null || idstValue == DBNull.Value ? string.Empty : idstValue.ToString().Trim();
        string reproOnRvp = reproOnRvpValue == null || reproOnRvpValue == DBNull.Value ? string.Empty : reproOnRvpValue.ToString().Trim();
        string reproducibility = reproducibilityValue == null || reproducibilityValue == DBNull.Value ? string.Empty : reproducibilityValue.ToString().Trim();

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"pending-signals-cell\">");
        sb.Append("<span class=\"pending-chip-row\">");
        sb.AppendFormat("<span class=\"pending-chip\">iDST: {0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(idst) ? "Missing" : idst));
        sb.AppendFormat("<span class=\"pending-chip\">RVP: {0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(reproOnRvp) ? "Missing" : reproOnRvp));
        sb.Append("</span>");
        sb.AppendFormat("<span class=\"pending-mini-label\">Reproducibility</span><span>{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(reproducibility) ? "Not provided" : reproducibility));
        sb.Append("</span>");
        return sb.ToString();
    }

    protected bool IsPendingRequestStale(object dateCmfAskValue)
    {
        DateTime askDate;
        return dateCmfAskValue != null && dateCmfAskValue != DBNull.Value
            && DateTime.TryParse(Convert.ToString(dateCmfAskValue), out askDate)
            && DateTime.Now - askDate > TimeSpan.FromDays(2);
    }

    protected string RenderPendingAskImpact(object cpIdValue, object titleValue, object componentValue, object dateCmfAskValue, object cmfRequestValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue)
    {
        string dateCmfAsk = dateCmfAskValue == null || dateCmfAskValue == DBNull.Value ? string.Empty : dateCmfAskValue.ToString().Trim();
        string impact = impactValue == null || impactValue == DBNull.Value ? string.Empty : impactValue.ToString().Trim();

        DateTime parsedDate;
        bool hasAskDate = DateTime.TryParse(dateCmfAsk, out parsedDate);
        if (hasAskDate)
        {
            dateCmfAsk = parsedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"pending-status-cell\">");
        sb.Append("<span class=\"pending-chip-row\">");
        sb.AppendFormat("<span class=\"pending-chip\">Date: {0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(dateCmfAsk) ? "N/A" : dateCmfAsk));
        if (hasAskDate && DateTime.Now - parsedDate > TimeSpan.FromDays(2))
        {
            sb.Append("<span class=\"pending-stale-label\" title=\"CMF ask date is more than 2 days old\"><i class=\"fas fa-clock\" aria-hidden=\"true\"></i>Stale</span>");
        }
        sb.Append("</span>");
        sb.AppendFormat("<span class=\"pending-mini-label\">Impact</span><span>{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(impact) ? "Impact not specified" : impact));
        sb.Append("</span>");
        return sb.ToString();
    }

    protected string RenderPendingImpactDetailsButton(object cpIdValue, object titleValue, object componentValue, object cmfRequestValue, object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue, object customerOwnerValue)
    {
        return string.Format(
            "<button type=\"button\" class=\"pending-recommendation-btn pending-impact-details-btn\" onclick='openCmfPendingImpactModal(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\")' title=\"AI impact details\" aria-label=\"AI impact details\"><i class=\"fas fa-bolt\" aria-hidden=\"true\"></i><span>AI Impact</span></button>",
            JsEncode(cpIdValue),
            JsEncode(titleValue),
            JsEncode(componentValue),
            JsEncode(cmfRequestValue),
            JsEncode(impactValue),
            JsEncode(idstValue),
            JsEncode(reproOnRvpValue),
            JsEncode(reproducibilityValue),
            JsEncode(customerDetailValue),
            JsEncode(customerOwnerValue));
    }

    private static string EstimatePendingRecommendationLabel(object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue)
    {
        string impact = impactValue == null || impactValue == DBNull.Value ? string.Empty : impactValue.ToString();
        string idst = idstValue == null || idstValue == DBNull.Value ? string.Empty : idstValue.ToString();
        string rvp = reproOnRvpValue == null || reproOnRvpValue == DBNull.Value ? string.Empty : reproOnRvpValue.ToString();
        string repro = reproducibilityValue == null || reproducibilityValue == DBNull.Value ? string.Empty : reproducibilityValue.ToString();
        bool hasRepro = ContainsText(rvp, "yes") || ContainsText(repro, "100") || ContainsText(repro, "frequent") || ContainsText(repro, "always");
        bool hasImpact = ContainsText(impact, "high") || ContainsText(impact, "block") || ContainsText(impact, "critical") || ContainsText(impact, "customer");
        bool hasDebug = !string.IsNullOrWhiteSpace(idst);
        if (hasRepro && hasImpact && hasDebug) return "CMF_OK";
        if (hasRepro || hasImpact) return "CMF_INCOMPLETE";
        return "CMF_REJECT";
    }

    private static int EstimatePendingRecommendationConfidence(object impactValue, object idstValue, object reproOnRvpValue, object reproducibilityValue, object customerDetailValue)
    {
        int score = 45;
        if (HasValue(impactValue)) score += 12;
        if (HasValue(idstValue)) score += 12;
        if (HasValue(reproOnRvpValue)) score += 10;
        if (HasValue(reproducibilityValue)) score += 12;
        if (HasValue(customerDetailValue)) score += 9;
        return Math.Min(92, score);
    }

    private static bool HasValue(object value)
    {
        return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
    }

    private static bool ContainsText(string value, string token)
    {
        return !string.IsNullOrWhiteSpace(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    protected string RenderMilestoneProgress(object milestoneValue, object progressValue, object fallbackMilestoneValue)
    {
        string fallbackMilestone = fallbackMilestoneValue == null || fallbackMilestoneValue == DBNull.Value ? string.Empty : fallbackMilestoneValue.ToString().Trim();
        string milestone = fallbackMilestone;
        string progress = progressValue == null || progressValue == DBNull.Value ? string.Empty : progressValue.ToString().Trim();
        if (string.IsNullOrWhiteSpace(milestone))
        {
            milestone = milestoneValue == null || milestoneValue == DBNull.Value ? string.Empty : milestoneValue.ToString().Trim();
        }
        string cssClass = ResolveProgressColorClass(progress);
        return string.Format(
            "<span class=\"milestone-progress-fill {0}\">{1}</span>",
            HttpUtility.HtmlAttributeEncode(cssClass),
            HttpUtility.HtmlEncode(milestone));
    }

    protected string RenderMilestoneText(object driverValue, object mustFixForValue)
    {
        string milestone = driverValue == null || driverValue == DBNull.Value ? string.Empty : driverValue.ToString().Trim();
        if (string.IsNullOrWhiteSpace(milestone))
        {
            milestone = mustFixForValue == null || mustFixForValue == DBNull.Value ? string.Empty : mustFixForValue.ToString().Trim();
        }

        return string.Format("<span class=\"milestone-text-only\">{0}</span>", HttpUtility.HtmlEncode(milestone));
    }

    private static string ResolveProgressColorClass(string progress)
    {
        if (string.IsNullOrWhiteSpace(progress)) return "milestone-progress-neutral";
        string normalized = progress.Trim().ToLowerInvariant();
        if (normalized.Contains("green")) return "milestone-progress-green";
        if (normalized.Contains("yellow")) return "milestone-progress-yellow";
        if (normalized.Contains("orange")) return "milestone-progress-orange";
        if (normalized.Contains("red")) return "milestone-progress-red";
        return "milestone-progress-neutral";
    }

    protected string RenderImpactWithProcessor(object impactValue, object processorValue)
    {
        string impact = impactValue == null || impactValue == DBNull.Value ? string.Empty : impactValue.ToString().Trim();
        string processor = processorValue == null || processorValue == DBNull.Value ? string.Empty : processorValue.ToString().Trim();
        string displayProcessor = FormatProcessorName(processor);

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"impact-processor-cell\">");
        sb.AppendFormat("<span class=\"impact-text\">{0}</span>", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(impact) ? "Impact not specified" : impact));
        if (!string.IsNullOrWhiteSpace(displayProcessor) && !string.Equals(processor, "unassigned", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendFormat("<span class=\"processor-chip\">{0}</span>", HttpUtility.HtmlEncode(displayProcessor));
        }
        sb.Append("</span>");
        return sb.ToString();
    }

    protected string RenderDaysOpen(object daysOpenValue)
    {
        string daysOpen = daysOpenValue == null || daysOpenValue == DBNull.Value ? "unknown" : daysOpenValue.ToString().Trim();
        if (string.IsNullOrWhiteSpace(daysOpen)) daysOpen = "unknown";
        return HttpUtility.HtmlEncode(daysOpen);
    }

    protected string RenderCustomerDetailWithCompanyAndProcessor(
        object customerDetailValue, object customerCompanyValue, object processorValue)
    {
        string detail = customerDetailValue == null || customerDetailValue == DBNull.Value ? string.Empty : customerDetailValue.ToString().Trim();
        string company = customerCompanyValue == null || customerCompanyValue == DBNull.Value ? string.Empty : customerCompanyValue.ToString().Trim();

        string companyName = ResolveFullCompanyName(company, detail);
        string badgeClass = BuildCompanyBadgeClass(string.IsNullOrWhiteSpace(company) ? (detail.Contains("_") ? detail.Split('_')[0] : detail) : company);
        string displayDetail = detail.Replace('_', ' ').Trim();

        StringBuilder sb = new StringBuilder();
        sb.Append("<span class=\"customer-detail-cell\">");
        if (!string.IsNullOrWhiteSpace(displayDetail))
        {
            sb.AppendFormat("<span class=\"customer-detail-text\">{0}</span>", HttpUtility.HtmlEncode(displayDetail));
        }
        if (!string.IsNullOrWhiteSpace(companyName))
        {
            sb.AppendFormat(
                "<span class=\"company-badge company-badge-full {0}\" title=\"{1}\">{2}</span>",
                HttpUtility.HtmlAttributeEncode(badgeClass),
                HttpUtility.HtmlAttributeEncode(company),
                HttpUtility.HtmlEncode(companyName));
        }
        sb.Append("</span>");
        return sb.ToString();
    }

    private static string ResolveFullCompanyName(string companyCode, string customerDetail)
    {
        if (!string.IsNullOrWhiteSpace(companyCode))
        {
            var codeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "DE", "Dell" }, { "AS", "ASUS" }, { "HP", "HP" }, { "LE", "Lenovo" },
                { "SA", "Samsung" }, { "AC", "Acer" }, { "LG", "LG" }, { "SO", "Sony" },
                { "TO", "Toshiba" }, { "PA", "Panasonic" }, { "FU", "Fujitsu" },
                { "MS", "MSI" }, { "RA", "Razer" }, { "GO", "Google" }, { "AP", "Apple" },
                { "MI", "Microsoft" }, { "HO", "Honor" }, { "HU", "Huawei" },
                { "AL", "Alienware" }, { "NE", "NEC" }, { "TS", "Toshiba" }
            };
            string mapped;
            if (codeMap.TryGetValue(companyCode, out mapped)) return mapped;
        }
        if (!string.IsNullOrWhiteSpace(customerDetail))
        {
            string prefix = customerDetail.Split('_')[0].ToLowerInvariant();
            var prefixMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "dell", "Dell" }, { "asustek", "ASUS" }, { "asus", "ASUS" }, { "hp", "HP" },
                { "lenovo", "Lenovo" }, { "samsung", "Samsung" }, { "acer", "Acer" },
                { "lg", "LG" }, { "sony", "Sony" }, { "toshiba", "Toshiba" },
                { "panasonic", "Panasonic" }, { "fujitsu", "Fujitsu" }, { "msi", "MSI" },
                { "razer", "Razer" }, { "google", "Google" }, { "microsoft", "Microsoft" },
                { "honor", "Honor" }, { "huawei", "Huawei" }, { "alienware", "Alienware" }
            };
            string name;
            if (prefixMap.TryGetValue(prefix, out name)) return name;
            if (prefix.Length > 0)
                return char.ToUpperInvariant(prefix[0]) + prefix.Substring(1);
        }
        return string.Empty;
    }

    private static string FormatProcessorName(string processor)
    {
        if (string.IsNullOrWhiteSpace(processor)) return string.Empty;
        return string.Join(" ", processor.Split('_').Select(p =>
            string.IsNullOrEmpty(p) ? p : char.ToUpperInvariant(p[0]) + p.Substring(1)));
    }

    protected string RenderMilestoneWithCompanyBadge(object milestoneValue, object companyValue)
    {
        string milestone = milestoneValue == null || milestoneValue == DBNull.Value ? string.Empty : milestoneValue.ToString().Trim();
        string company = companyValue == null || companyValue == DBNull.Value ? string.Empty : companyValue.ToString().Trim();

        string milestoneHtml = HttpUtility.HtmlEncode(milestone);
        if (string.IsNullOrWhiteSpace(company))
        {
            return milestoneHtml;
        }

        string badgeText = BuildCompanyBadgeText(company);
        string badgeClass = BuildCompanyBadgeClass(company);

        return string.Format(
            "<span class=\"milestone-with-company\"><span class=\"company-badge {0}\" title=\"{1}\">{2}</span><span class=\"milestone-label\">{3}</span></span>",
            HttpUtility.HtmlAttributeEncode(badgeClass),
            HttpUtility.HtmlAttributeEncode(company),
            HttpUtility.HtmlEncode(badgeText),
            milestoneHtml);
    }

    protected string RenderComponentWithGroupBadge(object componentValue, object componentGroupValue)
    {
        string component = componentValue == null || componentValue == DBNull.Value ? string.Empty : componentValue.ToString().Trim();
        string componentGroup = componentGroupValue == null || componentGroupValue == DBNull.Value ? string.Empty : componentGroupValue.ToString().Trim();

        string componentHtml = HttpUtility.HtmlEncode(component);
        if (string.IsNullOrWhiteSpace(componentGroup))
        {
            return componentHtml;
        }

        return string.Format(
            "<span class=\"component-with-group\">" +
            "<span class=\"component-detail\">{0}</span>" +
            "<span class=\"component-group-pill\">{1}</span>" +
            "</span>",
            HttpUtility.HtmlEncode(component),
            HttpUtility.HtmlEncode(componentGroup));
    }

    protected string RenderOwnerWithPromotedOwner(object ownerValue, object promotedOwnerValue)
    {
        string owner = ownerValue == null || ownerValue == DBNull.Value ? string.Empty : ownerValue.ToString().Trim();
        string promotedOwner = promotedOwnerValue == null || promotedOwnerValue == DBNull.Value ? string.Empty : promotedOwnerValue.ToString().Trim();

        if (string.IsNullOrWhiteSpace(owner) && string.IsNullOrWhiteSpace(promotedOwner))
        {
            return string.Empty;
        }

        string promotedText = string.IsNullOrWhiteSpace(promotedOwner) ? "N/A" : promotedOwner;

        return string.Format(
            "<span class=\"owner-pair\">" +
            "<span class=\"owner-inline-row\"><span class=\"owner-inline-label\">Owner:</span><span class=\"owner-inline-value\">{0}</span></span>" +
            "<span class=\"owner-inline-row\"><span class=\"owner-inline-label\">Promoted To:</span><span class=\"owner-inline-value owner-inline-muted\">{1}</span></span>" +
            "</span>",
            HttpUtility.HtmlEncode(owner),
            HttpUtility.HtmlEncode(promotedText));
    }

    protected string GetSysdebugForumValue(object sysdebugForumValue)
    {
        return sysdebugForumValue != null && sysdebugForumValue != DBNull.Value ? sysdebugForumValue.ToString().Trim() : "";
    }

    protected string RenderStatusWithAiSummaryButton(object statusValue, object rawStatusValue, object sightingIdValue, object titleValue, object submittedDateValue, object sysdebugValue, object dataItemObj = null)
    {
        string status = statusValue == null || statusValue == DBNull.Value ? string.Empty : statusValue.ToString().Trim();
        string rawStatus = rawStatusValue == null || rawStatusValue == DBNull.Value ? string.Empty : rawStatusValue.ToString().Trim();

        string sysdebugForum = "";
        try {
            if (dataItemObj != null) {
                var drv = dataItemObj as System.Data.DataRowView;
                if (drv != null) {
                    if (drv.Row.Table.Columns.Contains("sysdebug_forum")) {
                         var val = drv["sysdebug_forum"];
                         if (val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString())) {
                             sysdebugForum = val.ToString().Trim();
                         }
                    }
                    else if (drv.Row.Table.Columns.Contains("cmf_status")) {
                         var val = drv["cmf_status"];
                         if (val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString())) {
                             sysdebugForum = val.ToString().Trim();
                         }
                    }
                }
            }
        } catch { }

        // Lifecycle status is shown in Progress; this column contains only debug updates.
        string primaryStatusToDisplay = string.Empty;
        if (!string.IsNullOrWhiteSpace(sysdebugForum)) primaryStatusToDisplay = sysdebugForum;
        else if (!string.IsNullOrWhiteSpace(rawStatus)) primaryStatusToDisplay = rawStatus;
        else primaryStatusToDisplay = string.Empty;

        if (string.Equals(primaryStatusToDisplay, status, StringComparison.OrdinalIgnoreCase)
            || new[] { "open", "closed", "complete", "implemented", "verified", "rejected" }
            .Contains(primaryStatusToDisplay, StringComparer.OrdinalIgnoreCase))
        {
            primaryStatusToDisplay = string.Empty;
        }

        string sysdebug = sysdebugValue == null || sysdebugValue == DBNull.Value ? string.Empty : sysdebugValue.ToString().Replace("\r", " ").Replace("\n", " ").Trim();
        string onclick = "openAiSummaryModal(\"" + JsEncode(sightingIdValue) + "\", \"" + JsEncode(titleValue) + "\", \"" + JsEncode(FormatDateOnly(submittedDateValue)) + "\", \"" + JsEncode(statusValue) + "\", \"" + JsEncode(sysdebugValue) + "\", \"AI Summary\")";

        return "<div class=\"status-cell-wrap status-cell-wrap-compact\">" +
            "<div class=\"status-row status-row-primary\">" +
                "<span class=\"status-label-group\">" +
                "<button type=\"button\" class=\"ai-summary-btn ai-summary-btn-inline issue-details-ai-btn\" style=\"padding: 2px 8px; border-radius: 4px; display: inline-flex; align-items: center; justify-content: center; gap: 4px; border: 1px solid #7cb5ec; background-color: #f3f9ff; font-size: 14px; width: 26px; height: 26px; margin-right: 6px;\" onclick='" + onclick + "' title=\"AI Debug Summary\" aria-label=\"AI Debug Summary\">&#10024;</button>" +
                "</span>" +
            "</div>" +
            "<div class=\"status-one-line\">" + HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(primaryStatusToDisplay) ? "unknown" : primaryStatusToDisplay) + "</div>" +
            "</div>";
    }

    private string GetLatestOneLineUpdateForIssue(string issueId, string title, string status, string sysdebug, out int confidence)
    {
        confidence = 40;
        if (string.IsNullOrWhiteSpace(issueId)) return string.Empty;

        try
        {
            string platform = Session[IssuePendingPlatformSessionKey] as string;
            if (string.IsNullOrWhiteSpace(platform)) platform = Session["selectedPlatform"] as string;
            string issueContext = BuildIssueSummaryContext(platform, issueId.Trim());
            confidence = AiSummaryService.EstimateSummaryConfidence(new AiSummaryRequest
            {
                IssueId = issueId,
                Title = title,
                Status = status,
                Sysdebug = sysdebug,
                ContextDetails = issueContext
            });

            string generated = AiSummaryService.GenerateOneLineStatus(new AiSummaryRequest
            {
                IssueId = issueId,
                Title = title,
                Status = status,
                Sysdebug = sysdebug,
                ContextDetails = issueContext
            });

            if (!string.IsNullOrWhiteSpace(generated))
            {
                return generated;
            }
        }
        catch { }

        return string.Empty;
    }

    private static string BuildOneLineStatusUpdate(string status, string updateText)
    {
        string cleaned = string.IsNullOrWhiteSpace(updateText) ? string.Empty : updateText;
        cleaned = cleaned.Replace('\r', ' ').Replace('\n', ' ').Replace("\t", " ").Trim();
        while (cleaned.IndexOf("  ", StringComparison.Ordinal) >= 0)
        {
            cleaned = cleaned.Replace("  ", " ");
        }

        if (cleaned.StartsWith(":", StringComparison.Ordinal))
        {
            cleaned = cleaned.Substring(1).Trim();
        }

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return string.Empty;
        }

        const int maxLength = 105;
        if (cleaned.Length > maxLength)
        {
            int sentenceEnd = cleaned.IndexOfAny(new[] { '.', '!', '?' });
            if (sentenceEnd > 30 && sentenceEnd < maxLength)
            {
                cleaned = cleaned.Substring(0, sentenceEnd + 1).Trim();
            }
            else
            {
                cleaned = cleaned.Substring(0, maxLength).TrimEnd('.', ',', ';', ':', ' ');
            }
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            return "Latest update: " + cleaned;
        }

        return cleaned;
    }

    private static string BuildFallbackStatusSentence(string status)
    {
        string effectiveStatus = string.IsNullOrWhiteSpace(status) ? "active" : status.Trim();
        return BuildOneLineStatusUpdate(string.Empty, "Current status is " + effectiveStatus + "; latest HSD follow-up is pending.");
    }

    protected string RenderSightingAndPromotedLinks(object sightingIdValue, object promotedIdValue)
    {
        string sightingId = sightingIdValue == null || sightingIdValue == DBNull.Value ? string.Empty : sightingIdValue.ToString().Trim();
        string promotedId = promotedIdValue == null || promotedIdValue == DBNull.Value ? string.Empty : promotedIdValue.ToString().Trim();

        if (string.IsNullOrWhiteSpace(sightingId))
        {
            return string.Empty;
        }

        string promotedDisplay = string.IsNullOrWhiteSpace(promotedId)
            ? "N/A"
            : string.Format(
                "<a href='https://hsdes.intel.com/appstore/article/#{0}' target='_blank'>{1}</a>",
                HttpUtility.HtmlAttributeEncode(promotedId),
                HttpUtility.HtmlEncode(promotedId));

        return string.Format(
            "<span class=\"id-pair-stack\">" +
            "<span class=\"id-inline-row\"><span class=\"id-inline-label\">Sighting:</span><a class=\"id-inline-link\" href='https://hsdes.intel.com/appstore/article/#{0}' target='_blank'>{1}</a></span>" +
            "<span class=\"id-inline-row\"><span class=\"id-inline-label\">Promoted:</span><span class=\"id-inline-value\">{2}</span></span>" +
            "</span>",
            HttpUtility.HtmlAttributeEncode(sightingId),
            HttpUtility.HtmlEncode(sightingId),
            promotedDisplay);
    }

    private static string BuildCompanyBadgeText(string company)
    {
        if (string.IsNullOrWhiteSpace(company))
        {
            return string.Empty;
        }

        string[] companyParts = System.Text.RegularExpressions.Regex.Split(company.Trim(), @"[\s/_\-]+")
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (companyParts.Length == 0)
        {
            return company.Length <= 2 ? company.ToUpperInvariant() : company.Substring(0, 2).ToUpperInvariant();
        }

        if (companyParts.Length == 1)
        {
            return companyParts[0].Length <= 2 ? companyParts[0].ToUpperInvariant() : companyParts[0].Substring(0, 2).ToUpperInvariant();
        }

        StringBuilder initials = new StringBuilder();
        foreach (string part in companyParts)
        {
            if (!string.IsNullOrEmpty(part))
            {
                initials.Append(char.ToUpperInvariant(part[0]));
            }

            if (initials.Length == 3)
            {
                break;
            }
        }

        return initials.ToString();
    }

    private static string BuildCompanyBadgeClass(string company)
    {
        if (string.IsNullOrWhiteSpace(company))
        {
            return "company-badge-tone-4";
        }

        int bucket = 0;
        foreach (char character in company.ToUpperInvariant())
        {
            bucket = (bucket * 31 + character) % 6;
        }

        return "company-badge-tone-" + bucket.ToString(CultureInfo.InvariantCulture);
    }

    protected string JsEncode(object value)
    {
        string text = value == null || value == DBNull.Value ? string.Empty : value.ToString();
        return HttpUtility.JavaScriptStringEncode(text);
    }

    protected string FormatDateOnly(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return string.Empty;
        }

        DateTime parsed;
        if (DateTime.TryParse(value.ToString(), out parsed))
        {
            return parsed.ToString("yyyy-MM-dd");
        }

        return value.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static AiSummaryResponse GetIssueAiSummary(
        string issueId,
        string title,
        string submittedDate,
        string status,
        string sysdebug,
        string platform,
        string mode)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session[IssuePendingPlatformSessionKey] as string;
                if (string.IsNullOrWhiteSpace(resolvedPlatform))
                {
                    resolvedPlatform = HttpContext.Current.Session["selectedPlatform"] as string;
                }
            }

            if (!IsAllowedOrOverallPlatform(resolvedPlatform))
            {
                return new AiSummaryResponse
                {
                    Success = false,
                    Message = "Invalid platform input for summary generation."
                };
            }

            string issueContext = BuildIssueSummaryContext(NormalizePlatformForLookup(resolvedPlatform), issueId);

            AiSummaryRequest request = new AiSummaryRequest
            {
                IssueId = issueId,
                Title = title,
                SubmittedDate = submittedDate,
                Status = status,
                Sysdebug = sysdebug,
                ContextDetails = issueContext
            };

            if (string.Equals(mode, "details", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "AI issue details", StringComparison.OrdinalIgnoreCase))
            {
                return AiSummaryService.GenerateIssueDetails(request);
            }

            return AiSummaryService.GenerateIssueSummary(request);
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "Summary generation failed: " + ex.Message
            };
        }
    }

    [WebMethod(EnableSession = true)]
    public static AiSummaryResponse AskIssueAiFollowUp(
        string issueId,
        string title,
        string submittedDate,
        string status,
        string sysdebug,
        string platform,
        string question)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session[IssuePendingPlatformSessionKey] as string;
                if (string.IsNullOrWhiteSpace(resolvedPlatform))
                {
                    resolvedPlatform = HttpContext.Current.Session["selectedPlatform"] as string;
                }
            }

            if (!IsAllowedOrOverallPlatform(resolvedPlatform))
            {
                return new AiSummaryResponse
                {
                    Success = false,
                    Message = "Invalid platform input for follow-up generation."
                };
            }

            string issueContext = BuildIssueSummaryContext(NormalizePlatformForLookup(resolvedPlatform), issueId);

            AiSummaryRequest request = new AiSummaryRequest
            {
                IssueId = issueId,
                Title = title,
                SubmittedDate = submittedDate,
                Status = status,
                Sysdebug = sysdebug,
                ContextDetails = issueContext
            };

            return AiSummaryService.GenerateFollowUpResponse(request, question ?? string.Empty);
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "Follow-up generation failed: " + ex.Message
            };
        }
    }

    private static string BuildIssueSummaryContext(string platformTable, string issueId)
    {
        if (string.IsNullOrWhiteSpace(issueId))
        {
            return string.Empty;
        }

        try
        {
            string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string effectivePlatformTable = platformTable;
                if (string.IsNullOrWhiteSpace(effectivePlatformTable)
                    || !AllowedPlatformTables.Contains(effectivePlatformTable)
                    || !IssueExistsInPlatform(con, effectivePlatformTable, issueId))
                {
                    effectivePlatformTable = FindIssuePlatformTable(con, issueId);
                }

                if (string.IsNullOrWhiteSpace(effectivePlatformTable))
                {
                    return "Context Lookup: Issue " + issueId.Trim() + " was not found in the selected platform or any allowed CMF platform table.";
                }

                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1
    main.cp_id,
    main.title,
    main.operating_system,
    main.component,
    main.cmf_request,
    main.must_fix_for,
    main.closed_reason,
    main.customer_impact,
    main.priority,
    main.promoted_status,
    main.drivers,
    main.sysdebug,
    main.impact,
    main.processor,
    main.reproducibility,
    main.repro_on_rvp,
    main.promoted_id,
    main.merge_id,
    main.fixed_in_version,
    main.cmf_status,
    main.status
FROM " + effectivePlatformTable + @" AS main
WHERE CAST(main.cp_id AS VARCHAR(50)) = @issueId", con))
                {
                cmd.Parameters.AddWithValue("@issueId", issueId.Trim());
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    if (table.Rows.Count == 0)
                    {
                        return string.Empty;
                    }

                    DataRow row = table.Rows[0];
                    StringBuilder builder = new StringBuilder();

                    // ── Section 1: CMF portal database fields ──────────────────
                    builder.AppendLine("--- CMF Portal Database ---");
                    builder.AppendLine("Context Platform Table: " + effectivePlatformTable);
                    AppendSummaryContextLine(builder, "Sighting ID", row, "cp_id");
                    AppendSummaryContextLine(builder, "Title", row, "title");
                    AppendSummaryContextLine(builder, "Operating System", row, "operating_system", "os");
                    AppendSummaryContextLine(builder, "Component", row, "component");
                    AppendSummaryContextLine(builder, "CMF Request", row, "cmf_request");
                    AppendSummaryContextLine(builder, "Must Fix For", row, "must_fix_for");
                    AppendSummaryContextLine(builder, "Closed Reason", row, "closed_reason");
                    AppendSummaryContextLine(builder, "Customer Impact", row, "customer_impact");
                    AppendSummaryContextLine(builder, "Priority", row, "priority");
                    AppendSummaryContextLine(builder, "CMF Status", row, "cmf_status");
                    AppendSummaryContextLine(builder, "Promoted Status", row, "promoted_status", "cmf_status", "status");
                    AppendSummaryContextLine(builder, "Drivers", row, "drivers");
                    AppendSummaryContextLine(builder, "Sysdebug", row, "sysdebug");
                    AppendSummaryContextLine(builder, "Impact", row, "impact");
                    AppendSummaryContextLine(builder, "Processor", row, "processor");
                    AppendSummaryContextLine(builder, "Reproducibility", row, "reproducibility", "repro_on_rvp");
                    AppendSummaryContextLine(builder, "RVP Platform Debug Details", row, "repro_on_rvp");

                    string promotedId = FirstNonEmptyColumnValue(row, "promoted_id", "merge_id");
                    if (string.IsNullOrWhiteSpace(promotedId))
                    {
                        builder.AppendLine("Promoted ID: N/A");
                    }
                    else
                    {
                        builder.AppendLine("Promoted ID: " + promotedId);
                        if (!string.Equals(promotedId.Trim(), issueId.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            DataRow promotedRow = LoadIssueRowById(con, effectivePlatformTable, promotedId.Trim())
                                ?? LoadIssueRowByIdAcrossPlatforms(con, promotedId.Trim());
                            if (promotedRow != null)
                            {
                                AppendSummaryContextLine(builder, "Promoted Issue ID", promotedRow, "cp_id");
                                AppendSummaryContextLine(builder, "Promoted Issue Title", promotedRow, "title");
                                AppendSummaryContextLine(builder, "Promoted Issue Status", promotedRow, "status", "cmf_status");
                                AppendSummaryContextLine(builder, "Promoted Issue CMF Request", promotedRow, "cmf_request");
                                AppendSummaryContextLine(builder, "Promoted Issue Priority", promotedRow, "priority");
                                AppendSummaryContextLine(builder, "Promoted Issue Customer Impact", promotedRow, "customer_impact");
                                AppendSummaryContextLine(builder, "Promoted Issue Closed Reason", promotedRow, "closed_reason");
                                AppendSummaryContextLine(builder, "Promoted Issue Fixed Version", promotedRow, "fixed_in_version");
                                AppendSummaryContextLine(builder, "Promoted Issue Impact", promotedRow, "impact");
                            }
                            else
                            {
                                builder.AppendLine("Promoted Issue Lookup: Not found in current platform table");
                            }
                        }
                    }

                    // ── Section 2: HSD portal – sighting article ───────────────
                    HsdArticleData hsdSighting =
                        HsdPortalService.FetchArticle(issueId.Trim());

                    string hsdSightingContext =
                        HsdPortalService.FormatForAiContext(
                            hsdSighting,
                            "Sighting " + issueId.Trim());

                    if (!string.IsNullOrWhiteSpace(hsdSightingContext))
                        builder.AppendLine(hsdSightingContext);

                    // ── Section 3: HSD portal – promoted article (if different) ─
                    if (!string.IsNullOrWhiteSpace(promotedId)
                        && !string.Equals(promotedId.Trim(), issueId.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        HsdArticleData hsdPromoted = HsdPortalService.FetchArticle(promotedId.Trim());
                        string hsdPromotedContext = HsdPortalService.FormatForAiContext(hsdPromoted, "Promoted Issue " + promotedId.Trim());
                        if (!string.IsNullOrWhiteSpace(hsdPromotedContext))
                            builder.AppendLine(hsdPromotedContext);
                    }

                    return builder.ToString().Trim();
                }
            }
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool IssueExistsInPlatform(SqlConnection connection, string platformTable, string issueId)
    {
        if (connection == null || string.IsNullOrWhiteSpace(platformTable) || string.IsNullOrWhiteSpace(issueId) || !AllowedPlatformTables.Contains(platformTable))
        {
            return false;
        }

        using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 1 FROM " + platformTable + " WHERE CAST(cp_id AS VARCHAR(50)) = @issueId", connection))
        {
            cmd.Parameters.AddWithValue("@issueId", issueId.Trim());
            object result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value;
        }
    }

    private static string FindIssuePlatformTable(SqlConnection connection, string issueId)
    {
        if (connection == null || string.IsNullOrWhiteSpace(issueId))
        {
            return string.Empty;
        }

        foreach (string candidateTable in AllowedPlatformTables)
        {
            if (IssueExistsInPlatform(connection, candidateTable, issueId))
            {
                return candidateTable;
            }
        }

        return string.Empty;
    }

    private static DataRow LoadIssueRowByIdAcrossPlatforms(SqlConnection connection, string lookupIssueId)
    {
        if (connection == null || string.IsNullOrWhiteSpace(lookupIssueId))
        {
            return null;
        }

        foreach (string candidateTable in AllowedPlatformTables)
        {
            DataRow row = LoadIssueRowById(connection, candidateTable, lookupIssueId);
            if (row != null)
            {
                return row;
            }
        }

        return null;
    }

    private static DataRow LoadIssueRowById(SqlConnection connection, string platformTable, string lookupIssueId)
    {
        if (connection == null || string.IsNullOrWhiteSpace(platformTable) || string.IsNullOrWhiteSpace(lookupIssueId))
        {
            return null;
        }

        using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1
    main.cp_id,
    main.title,
    main.status,
    main.cmf_status,
    main.cmf_request,
    main.priority,
    main.customer_impact,
    main.closed_reason,
    main.fixed_in_version,
    main.impact
FROM " + platformTable + @" AS main
WHERE CAST(main.cp_id AS VARCHAR(50)) = @lookupIssueId", connection))
        {
            cmd.Parameters.AddWithValue("@lookupIssueId", lookupIssueId.Trim());
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                if (table.Rows.Count == 0)
                {
                    return null;
                }

                return table.Rows[0];
            }
        }
    }

    private static string FirstNonEmptyColumnValue(DataRow row, params string[] candidateColumns)
    {
        if (row == null || candidateColumns == null || candidateColumns.Length == 0)
        {
            return string.Empty;
        }

        foreach (string candidate in candidateColumns)
        {
            if (!row.Table.Columns.Contains(candidate))
            {
                continue;
            }

            object candidateValue = row[candidate];
            if (candidateValue == null || candidateValue == DBNull.Value)
            {
                continue;
            }

            string text = candidateValue.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private static void AppendSummaryContextLine(StringBuilder builder, string label, DataRow row, params string[] candidateColumns)
    {
        if (builder == null || row == null || candidateColumns == null || candidateColumns.Length == 0)
        {
            return;
        }

        string value = string.Empty;
        foreach (string candidate in candidateColumns)
        {
            if (!row.Table.Columns.Contains(candidate))
            {
                continue;
            }

            object candidateValue = row[candidate];
            if (candidateValue == null || candidateValue == DBNull.Value)
            {
                continue;
            }

            string text = candidateValue.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                value = text;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            value = "N/A";
        }

        builder.AppendLine(label + ": " + value);
    }

    [WebMethod(EnableSession = true)]
    public static AiSummaryResponse GetCmfPendingListInsights(string platform, string rowsContext)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session[IssuePendingPlatformSessionKey] as string
                    ?? HttpContext.Current.Session["selectedPlatform"] as string;
            }

            if (!IsAllowedOrOverallPlatform(resolvedPlatform))
            {
                return new AiSummaryResponse
                {
                    Success = false,
                    Message = "Invalid platform input for CMF Pending insights."
                };
            }

            string platformLabel = BuildStaticPlatformDisplayName(resolvedPlatform);
            string context = "Summarize the visible CMF Pending sightings for this platform. " +
                "Return 3-4 concise bullet sentences. Explain the main pattern, repeated components/customers/owners, evidence gaps, and what a reviewer should notice next. " +
                "Use only the supplied rows; do not invent counts or issue details.\n\n" +
                (string.IsNullOrWhiteSpace(rowsContext) ? "No visible CMF Pending rows were supplied." : rowsContext);

            return AiSummaryService.GenerateDashboardExecutiveSummary(platformLabel + " CMF Pending", context);
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "CMF Pending insights generation failed: " + ex.Message
            };
        }
    }

    private static string BuildStaticPlatformDisplayName(string platformTable)
    {
        string normalized = (platformTable ?? string.Empty).Trim();
        if (string.Equals(normalized, "NVL-H", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_NVL_H_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "NVL-S", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_NVL_S_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "PTL", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_PTL_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "LNL", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_LNL_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "ARL-S", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_ARL_S_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "ARL-H", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_ARL_H_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "ARL-U", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_ARL_U_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "ARL-Hx", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_ARL_HX_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "ARL-Refresh", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_ARL_Refresh_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "GNR", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_GNR_ALL_COMPONENTS_TABLE";
        else if (string.Equals(normalized, "WCL", StringComparison.OrdinalIgnoreCase)) normalized = "CMF_WCL_ALL_COMPONENTS_TABLE";
        if (string.Equals(normalized, "CMF_NVL_H_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "NVL-H";
        if (string.Equals(normalized, "CMF_NVL_S_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "NVL-S";
        if (string.Equals(normalized, "CMF_PTL_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "PTL";
        if (string.Equals(normalized, "CMF_LNL_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "LNL";
        if (string.Equals(normalized, "CMF_ARL_S_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "ARL-S";
        if (string.Equals(normalized, "CMF_ARL_H_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "ARL-H";
        if (string.Equals(normalized, "CMF_ARL_U_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "ARL-U";
        if (string.Equals(normalized, "CMF_ARL_HX_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "ARL-Hx";
        if (string.Equals(normalized, "CMF_ARL_Refresh_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "ARL-Refresh";
        if (string.Equals(normalized, "CMF_GNR_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "GNR";
        if (string.Equals(normalized, "CMF_WCL_ALL_COMPONENTS_TABLE", StringComparison.OrdinalIgnoreCase)) return "WCL";
        return string.IsNullOrWhiteSpace(normalized) ? "Selected Platform" : normalized;
    }

    [WebMethod(EnableSession = true)]
    public static CmfRecommendationResponse GetCmfPendingRecommendation(
        string cpId,
        string title,
        string component,
        string cmfRequest,
        string impact,
        string idst,
        string reproOnRvp,
        string reproducibility,
        string customerDetail,
        string customerOwner,
        string platform)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session["selectedPlatform"] as string;
            }

            if (!IsAllowedOrOverallPlatform(resolvedPlatform))
            {
                return new CmfRecommendationResponse
                {
                    Success = false,
                    Message = "Invalid platform input for recommendation generation."
                };
            }

            CmfRecommendationRequest request = new CmfRecommendationRequest
            {
                CpId = cpId,
                Title = title,
                Component = component,
                CmfRequest = cmfRequest,
                Impact = impact,
                Idst = idst,
                ReproOnRvp = reproOnRvp,
                Reproducibility = reproducibility,
                CustomerDetail = customerDetail,
                CustomerOwner = customerOwner,
                Rules = CmfRecommendationService.GetActiveRulesText(),
                HsdContext = BuildPendingRecommendationContext(NormalizePlatformForLookup(resolvedPlatform), cpId)
            };

            return CmfRecommendationService.GenerateCmfPendingRecommendation(request);
        }
        catch (Exception ex)
        {
            return new CmfRecommendationResponse
            {
                Success = false,
                Message = "Recommendation generation failed: " + ex.Message
            };
        }
    }

    [WebMethod(EnableSession = true)]
    public static SightingQualityAssessmentResponse GetSightingQualityAssessment(
        string cpId,
        string title,
        string component,
        string cmfRequest,
        string impact,
        string idst,
        string reproOnRvp,
        string reproducibility,
        string customerDetail,
        string customerOwner,
        string platform)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session[IssuePendingPlatformSessionKey] as string
                    ?? HttpContext.Current.Session["selectedPlatform"] as string;
            }

            if (!IsAllowedOrOverallPlatform(resolvedPlatform))
            {
                return new SightingQualityAssessmentResponse
                {
                    Success = false,
                    Message = "Invalid platform input for sighting quality assessment."
                };
            }

            CmfRecommendationRequest request = new CmfRecommendationRequest
            {
                CpId = cpId,
                Title = title,
                Component = component,
                CmfRequest = cmfRequest,
                Impact = impact,
                Idst = idst,
                ReproOnRvp = reproOnRvp,
                Reproducibility = reproducibility,
                CustomerDetail = customerDetail,
                CustomerOwner = customerOwner,
                HsdContext = BuildPendingRecommendationContext(resolvedPlatform, cpId)
            };

            return CmfRecommendationService.GenerateSightingQualityAssessment(request);
        }
        catch (Exception ex)
        {
            return new SightingQualityAssessmentResponse
            {
                Success = false,
                Message = "Sighting quality assessment failed: " + ex.Message
            };
        }
    }

    [WebMethod(EnableSession = true)]
    public static SightingQualityValidationTraceResponse GetSightingQualityValidationTrace(
        string cpId,
        string title,
        string component,
        string cmfRequest,
        string impact,
        string idst,
        string reproOnRvp,
        string reproducibility,
        string customerDetail,
        string customerOwner,
        string platform)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session[IssuePendingPlatformSessionKey] as string
                    ?? HttpContext.Current.Session["selectedPlatform"] as string;
            }

            if (!IsAllowedOrOverallPlatform(resolvedPlatform))
            {
                return new SightingQualityValidationTraceResponse
                {
                    Success = false,
                    Message = "Invalid platform input for sighting quality validation."
                };
            }

            CmfRecommendationRequest request = new CmfRecommendationRequest
            {
                CpId = cpId,
                Title = title,
                Component = component,
                CmfRequest = cmfRequest,
                Impact = impact,
                Idst = idst,
                ReproOnRvp = reproOnRvp,
                Reproducibility = reproducibility,
                CustomerDetail = customerDetail,
                CustomerOwner = customerOwner,
                HsdContext = BuildPendingRecommendationContext(resolvedPlatform, cpId)
            };

            return CmfRecommendationService.GenerateSightingQualityValidationTrace(request);
        }
        catch (Exception ex)
        {
            return new SightingQualityValidationTraceResponse
            {
                Success = false,
                Message = "Sighting quality validation trace failed: " + ex.Message
            };
        }
    }

    [WebMethod(EnableSession = true)]
    public static CmfRecommendationResponse GetCmfPendingDecisionDetails(
        string cpId,
        string title,
        string component,
        string cmfRequest,
        string impact,
        string idst,
        string reproOnRvp,
        string reproducibility,
        string customerDetail,
        string customerOwner,
        string platform)
    {
        try
        {
            if (!IsAllowedOrOverallPlatform(platform))
            {
                return new CmfRecommendationResponse
                {
                    Success = false,
                    Message = "Invalid platform input for CMF decision details."
                };
            }

            CmfRecommendationRequest request = new CmfRecommendationRequest
            {
                CpId = cpId,
                Title = title,
                Component = component,
                CmfRequest = cmfRequest,
                Impact = impact,
                Idst = idst,
                ReproOnRvp = reproOnRvp,
                Reproducibility = reproducibility,
                CustomerDetail = customerDetail,
                CustomerOwner = customerOwner,
                Rules = CmfRecommendationService.GetActiveRulesText(),
                HsdContext = BuildPendingRecommendationContext(NormalizePlatformForLookup(platform), cpId)
            };

            return CmfRecommendationService.GenerateCmfPendingDecisionDetails(request);
        }
        catch (Exception ex)
        {
            return new CmfRecommendationResponse
            {
                Success = false,
                Message = "CMF decision details failed: " + ex.Message
            };
        }
    }

    [WebMethod]
    public static CmfRecommendationResponse GetCmfPendingImpactDetails(
        string cpId,
        string title,
        string component,
        string cmfRequest,
        string impact,
        string idst,
        string reproOnRvp,
        string reproducibility,
        string customerDetail,
        string customerOwner,
        string platform)
    {
        try
        {
            if (!IsAllowedOrOverallPlatform(platform))
            {
                return new CmfRecommendationResponse
                {
                    Success = false,
                    Message = "Invalid platform input for CMF impact details."
                };
            }

            CmfRecommendationRequest request = new CmfRecommendationRequest
            {
                CpId = cpId,
                Title = title,
                Component = component,
                CmfRequest = cmfRequest,
                Impact = impact,
                Idst = idst,
                ReproOnRvp = reproOnRvp,
                Reproducibility = reproducibility,
                CustomerDetail = customerDetail,
                CustomerOwner = customerOwner,
                Rules = CmfRecommendationService.GetActiveRulesText(),
                HsdContext = BuildPendingRecommendationContext(NormalizePlatformForLookup(platform), cpId)
            };

            return CmfRecommendationService.GenerateCmfPendingImpactDetails(request);
        }
        catch (Exception ex)
        {
            return new CmfRecommendationResponse
            {
                Success = false,
                Message = "CMF impact details failed: " + ex.Message
            };
        }
    }

    [WebMethod]
    public static AiSummaryResponse GetSightingDescription(string cpId, string title, string platform)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cpId) || string.IsNullOrWhiteSpace(title))
            {
                return new AiSummaryResponse
                {
                    Success = false,
                    Message = "Sighting ID and title are required."
                };
            }

            // Build HSD context for better AI description generation
            string hsdContext = BuildPendingHsdContext(cpId);

            // Build database context (issue details from CMF database)
            string dbContext = BuildIssueSummaryContext(platform, cpId);

            // Combine contexts
            StringBuilder contextBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(dbContext))
            {
                contextBuilder.AppendLine(dbContext);
            }
            if (!string.IsNullOrWhiteSpace(hsdContext))
            {
                if (contextBuilder.Length > 0) contextBuilder.AppendLine();
                contextBuilder.AppendLine(hsdContext);
            }

            string contextDetails = contextBuilder.ToString();

            // Call the AI service to generate the sighting description
            AiSummaryResponse response = AiSummaryService.GenerateSightingDescription(cpId, title, contextDetails);

            return response;
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "Failed to generate sighting description: " + ex.Message
            };
        }
    }

    [WebMethod]
    public static AiSummaryResponse GetSightingImpactSummary(string cpId, string title, string impact, string platform)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cpId) || string.IsNullOrWhiteSpace(title))
            {
                return new AiSummaryResponse
                {
                    Success = false,
                    Message = "Sighting ID and title are required."
                };
            }

            string hsdContext = BuildPendingHsdContext(cpId);
            string dbContext = BuildIssueSummaryContext(platform, cpId);

            StringBuilder contextBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(dbContext))
            {
                contextBuilder.AppendLine(dbContext);
            }
            if (!string.IsNullOrWhiteSpace(hsdContext))
            {
                if (contextBuilder.Length > 0) contextBuilder.AppendLine();
                contextBuilder.AppendLine(hsdContext);
            }

            return AiSummaryService.GenerateSightingImpactSummary(cpId, title, impact, contextBuilder.ToString());
        }
        catch (Exception ex)
        {
            return new AiSummaryResponse
            {
                Success = false,
                Message = "Failed to generate sighting impact summary: " + ex.Message
            };
        }
    }

    [WebMethod]
    public static CmfSightingChatResponse AskCmfSightingQuestion(string cpId, string title, string question, string platform, List<CmfSightingChatTurn> history)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cpId) || string.IsNullOrWhiteSpace(question))
            {
                return new CmfSightingChatResponse
                {
                    Success = false,
                    Message = "Sighting ID and question are required."
                };
            }

            if (!IsAllowedOrOverallPlatform(platform))
            {
                return new CmfSightingChatResponse
                {
                    Success = false,
                    Message = "Invalid platform input for sighting chat."
                };
            }

            string hsdContext = BuildPendingHsdContext(cpId);
            string dbContext = BuildIssueSummaryContext(platform, cpId);

            StringBuilder contextBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(dbContext)) contextBuilder.AppendLine(dbContext);
            if (!string.IsNullOrWhiteSpace(hsdContext))
            {
                if (contextBuilder.Length > 0) contextBuilder.AppendLine();
                contextBuilder.AppendLine(hsdContext);
            }

            return AiSummaryService.GenerateSightingFollowUpAnswer(cpId, title, contextBuilder.ToString(), question, history);
        }
        catch (Exception ex)
        {
            return new CmfSightingChatResponse
            {
                Success = false,
                Message = "Failed to answer sighting question: " + ex.Message
            };
        }
    }

    private static string BuildPendingHsdContext(string cpId)
    {
        if (string.IsNullOrWhiteSpace(cpId)) return string.Empty;
        string cacheKey = "cmf-assistant-hsd-context:" + cpId.Trim();
        using (CmfAssistantResponseCache.Enter(cacheKey))
        {
            string cached = HttpRuntime.Cache[cacheKey] as string;
            if (cached != null) return cached;

        try
        {
            HsdArticleData article = HsdPortalService.FetchArticle(cpId.Trim());
            if (article == null || !article.FetchSuccess) return string.Empty;
            string context = HsdPortalService.FormatForAiContext(article, "Pending Sighting");
            context += "\nHSD Source Fingerprint: " + CmfAssistantResponseCache.Fingerprint(article);
            int refreshSeconds;
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["CMF:AssistantHsdRefreshSeconds"], out refreshSeconds)) refreshSeconds = 60;
            refreshSeconds = Math.Max(15, Math.Min(300, refreshSeconds));
            HttpRuntime.Cache.Insert(cacheKey, context, null, DateTime.UtcNow.AddSeconds(refreshSeconds), System.Web.Caching.Cache.NoSlidingExpiration);
            return context;
        }
        catch
        {
            return string.Empty;
        }
        }
    }

    private static string BuildPendingRecommendationContext(string platformTable, string cpId)
    {
        if (string.IsNullOrWhiteSpace(cpId)) return string.Empty;
        string resolvedPlatform = NormalizePlatformForLookup(platformTable);

        StringBuilder builder = new StringBuilder();
        string dbContext = BuildIssueSummaryContext(resolvedPlatform, cpId);
        if (!string.IsNullOrWhiteSpace(dbContext))
        {
            builder.AppendLine(dbContext);
        }

        string hsdContext = BuildPendingHsdContext(cpId);
        if (!string.IsNullOrWhiteSpace(hsdContext) && builder.ToString().IndexOf(hsdContext, StringComparison.OrdinalIgnoreCase) < 0)
        {
            if (builder.Length > 0) builder.AppendLine();
            builder.AppendLine(hsdContext);
        }

        string context = builder.ToString().Trim();
        return context;
    }

    [WebMethod(EnableSession = true)]
    public static ReportsAssistantResponse GetReportsAssistantResponse(string prompt, string platform)
    {
        try
        {
            string resolvedPlatform = platform;
            if (string.IsNullOrWhiteSpace(resolvedPlatform) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                resolvedPlatform = HttpContext.Current.Session[IssuePendingPlatformSessionKey] as string;
                if (string.IsNullOrWhiteSpace(resolvedPlatform))
                {
                    resolvedPlatform = HttpContext.Current.Session["selectedPlatform"] as string;
                }
            }

            return ReportsAssistantService.ProcessPrompt(prompt, resolvedPlatform);
        }
        catch (Exception ex)
        {
            return new ReportsAssistantResponse
            {
                Success = false,
                Message = "Reports assistant failed: " + ex.Message
            };
        }
    }

    [WebMethod(EnableSession = true)]
    public static ReportsAssistantResponse SaveReportsTemplate(string template, string platform, string fileName = null)
    {
        try
        {
            string appDataRoot = HostingEnvironment.MapPath("~/App_Data/reports-assistant");
            if (string.IsNullOrWhiteSpace(appDataRoot))
            {
                return new ReportsAssistantResponse { Success = false, Message = "Unable to resolve server storage path." };
            }

            string templatesDir = Path.Combine(appDataRoot, "templates");
            Directory.CreateDirectory(templatesDir);

            string safeName = string.IsNullOrWhiteSpace(fileName) ? "template.md" : Path.GetFileName(fileName);
            string extension = Path.GetExtension(safeName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                safeName += ".md";
            }
            string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            string outPath = Path.Combine(templatesDir, "ALL_CMF_" + stamp + "_" + safeName);
            File.WriteAllText(outPath, template ?? string.Empty, Encoding.UTF8);

            return new ReportsAssistantResponse { Success = true, Message = "Template saved." };
        }
        catch (Exception ex)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Unable to save template: " + ex.Message };
        }
    }

    [WebMethod(EnableSession = true)]
    public static ReportsAssistantResponse GenerateReportFromTemplate(string platform)
    {
        try
        {
            string appDataRoot = HostingEnvironment.MapPath("~/App_Data/reports-assistant");
            string templatesDir = Path.Combine(appDataRoot ?? string.Empty, "templates");
            if (!Directory.Exists(templatesDir))
            {
                return new ReportsAssistantResponse { Success = false, Message = "No saved templates found on server." };
            }

            string[] candidates = Directory.GetFiles(templatesDir, "ALL_CMF_*.*");
            string templatePath = GetNewestTemplatePath(candidates);
            if (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath))
            {
                return new ReportsAssistantResponse { Success = false, Message = "No global report template found." };
            }

            string templateContent = File.ReadAllText(templatePath, Encoding.UTF8);
            return ReportsAssistantService.GenerateFromTemplate(templateContent, string.Empty);
        }
        catch (Exception ex)
        {
            return new ReportsAssistantResponse { Success = false, Message = "Report generation failed: " + ex.Message };
        }
    }

    [WebMethod(EnableSession = true)]
    public static ReportsAssistantResponse GetSavedTemplate(string platform)
    {
        try
        {
            string appDataRoot = HostingEnvironment.MapPath("~/App_Data/reports-assistant");
            string templatesDir = Path.Combine(appDataRoot ?? string.Empty, "templates");
            if (!Directory.Exists(templatesDir))
            {
                return new ReportsAssistantResponse { Success = false, Message = "" };
            }

            string[] candidates = Directory.GetFiles(templatesDir, "ALL_CMF_*.*");
            string templatePath = GetNewestTemplatePath(candidates);
            if (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath))
            {
                return new ReportsAssistantResponse { Success = false, Message = "" };
            }

            string templateContent = File.ReadAllText(templatePath, Encoding.UTF8);
            return new ReportsAssistantResponse { Success = true, Message = templateContent };
        }
        catch
        {
            return new ReportsAssistantResponse { Success = false, Message = "" };
        }
    }

    private static string GetNewestTemplatePath(string[] candidates)
    {
        if (candidates == null || candidates.Length == 0)
        {
            return null;
        }

        string newest = null;
        DateTime newestTime = DateTime.MinValue;
        foreach (string candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate) || !File.Exists(candidate)) continue;
            string extension = Path.GetExtension(candidate).ToLowerInvariant();
            if (extension != ".md" && extension != ".txt" && extension != ".csv" && extension != ".json" && extension != ".html") continue;

            DateTime writeTime = File.GetLastWriteTimeUtc(candidate);
            if (newest == null || writeTime > newestTime)
            {
                newest = candidate;
                newestTime = writeTime;
            }
        }

        return newest;
    }

    //protected string CreateDuplicateLinks(object duplicateDetails)
    //{
    //    if (duplicateDetails == null || duplicateDetails == DBNull.Value)
    //        return "NA";

    //    string details = duplicateDetails.ToString().Trim();
    //    if (string.IsNullOrEmpty(details))
    //        return "NA";

    //    // Split by comma and create hyperlinks for each duplicate ID with customer company
    //    string[] duplicateEntries = details.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //    List<string> links = new List<string>();

    //    foreach (string entry in duplicateEntries)
    //    {
    //        string trimmedEntry = entry.Trim();
    //        if (!string.IsNullOrEmpty(trimmedEntry))
    //        {
    //            // Split by pipe to get HSD ID and Customer Company
    //            string[] parts = trimmedEntry.Split('|');
    //            string hsdId = parts[0].Trim();
    //            string customerCompany = parts.Length > 1 ? parts[1].Trim() : "Unknown";

    //            // Create hyperlink with HSD ID and show customer company - removed curly braces
    //            string link = string.Format(
    //                "<a href='https://hsdes.intel.com/appstore/article/#{0}' target='_blank' title='Customer: {2}'>{0} | {2}</a>",
    //                hsdId,
    //                hsdId,
    //                customerCompany
    //            );
    //            links.Add(link);
    //        }
    //    }

    //    // Return "NA" if no valid links were created
    //    return links.Count > 0 ? string.Join(", ", links) : "NA";
    //}


    public List<string> SelectedColumns
    {
        get
        {
            if (ViewState["SelectedColumns"] == null)
            {
                // Default to all columns

                ViewState["SelectedColumns"] = new List<string>
{
    "sno",
    "milestone",
    "progress",
    "sightingid",
    "promotedid",
    "customer_detail",
    "duplicatedetails",          // NEW
    "customer_company",          // Keeping this (see note below)
    "title",
    "component",
    "component_group",           // comp_grp
    "owner",
    "promoted_owner",
    "rvp_repro",
    "status",
    "idst",
    "los",
    "processor",                 // keep before customer_affected
    "customer_affected",
    "impact",
    "days_open",
    //"cmf_request",
    "closed_reason",
    "edit_column"
};

            }
            return (List<string>)ViewState["SelectedColumns"];
        }
        set
        {
            ViewState["SelectedColumns"] = value;
        }
    }

    // Add a web method to receive selected columns from JavaScript
    [WebMethod]
    public static void SaveSelectedColumns(List<string> selectedColumns)
    {
        HttpContext.Current.Session["SelectedColumns"] = selectedColumns;
    }

    protected void btnExportToExcel_Click(object sender, EventArgs e)
    {
        // Get selected columns from session or use all columns as default
        List<string> selectedColumns = Session["SelectedColumns"] as List<string>;

        if (selectedColumns == null || selectedColumns.Count == 0)
        {
            selectedColumns = new List<string>
    {
        "sno",
        "milestone",
        "progress",
        "sightingid",
        "promotedid",
        "customer_detail",
        "imagefreeze",
        "duplicatedetails",
        "customer_company",
        "title",
        "component",
        "component_group",
        "owner",
        "promoted_owner",
        "rvp_repro",
        "status",
        "idst",
        "los",
        "processor",
        "impact",
        "days_open",
        //"cmf_request",
        "closed_reason",
        "edit_column"
    };
        }



        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=GridViewExport.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        overall_request_details.AllowPaging = false;

        // Store original visibility states
        Dictionary<int, bool> originalVisibility = new Dictionary<int, bool>();

        // Map field names to column indices (you'll need to adjust this based on your actual column order)

        //var columnMapping = new Dictionary<string, int>
        //{
        //    {"sno", 0},
        //    {"milestone", 1},
        //    {"progress", 2},
        //    {"sightingid", 3},
        //    {"promotedid", 4},
        //    {"customer_detail", 5},
        //    {"duplicatedetails", 6},   // NEW index
        //    {"customer_company", 7},
        //    {"title", 8},
        //    {"component", 9},
        //    {"component_group", 10},
        //    {"owner", 11},
        //    {"promoted_owner", 12},
        //    {"rvp_repro", 13},
        //    {"status", 14},
        //    {"idst", 15},
        //    {"los", 16},
        //    {"processor", 17},
        //    {"customer_affected", 18},
        //    {"impact", 19},
        //    {"days_open", 20},
        //    //{"cmf_request", 21},
        //    {"closed_reason", 21},
        //    {"edit_column", 22}
        //};
        var columnMapping = new Dictionary<string, int>
{
    {"sno", 0},
    {"milestone", 1},
    {"progress", 2},
    {"sightingid", 3},
    {"promotedid", 4},
    {"customer_detail", 5},
    {"imagefreeze", 6},           // NEW
    {"duplicatedetails", 7},
    {"customer_company", 8},
    {"title", 9},
    {"component", 10},
    {"component_group", 11},
    {"owner", 12},
    {"promoted_owner", 13},
    {"rvp_repro", 14},
    {"status", 15},
    {"idst", 16},
    {"los", 17},
    {"processor", 18},
    {"impact", 19},
    {"days_open", 20},
    {"closed_reason", 21},
    {"edit_column", 22}
};


        // Hide columns that are not selected
        for (int i = 0; i < overall_request_details.Columns.Count; i++)
        {
            originalVisibility[i] = overall_request_details.Columns[i].Visible;

            // Find the field name for this column index
            string fieldName = columnMapping.FirstOrDefault(x => x.Value == i).Key;

            if (!string.IsNullOrEmpty(fieldName))
            {
                overall_request_details.Columns[i].Visible = selectedColumns.Contains(fieldName);
            }
        }

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        overall_request_details.RenderControl(hw);

        // Write the StringWriter content to the Response output stream
        Response.Output.Write(sw.ToString());

        // Restore original visibility states
        foreach (var kvp in originalVisibility)
        {
            if (kvp.Key < overall_request_details.Columns.Count)
            {
                overall_request_details.Columns[kvp.Key].Visible = kvp.Value;
            }
        }

        // Re-enable paging
        overall_request_details.AllowPaging = true;

        // End the response to download the Excel file
        Response.End();
    }

    //protected void btnExportToExcel_Click(object sender, EventArgs e)
    //{
    //    // Set the response properties for the Excel download
    //    Response.ContentType = "application/vnd.ms-excel";
    //    Response.AddHeader("Content-Disposition", "attachment;filename=GridViewExport.xls");
    //    Response.Charset = "";
    //    Response.Buffer = true;
    //    Response.Clear();

    //    // Temporarily disable paging for the export
    //    overall_request_details.AllowPaging = false;

    //    // Hide the last column for export
    //    if (overall_request_details.Columns.Count > 0)
    //    {
    //        overall_request_details.Columns[overall_request_details.Columns.Count - 1].Visible = false;
    //    }

    //    // Create a StringWriter to capture the HTML output
    //    StringWriter sw = new StringWriter();
    //    HtmlTextWriter hw = new HtmlTextWriter(sw);

    //    // Render the GridView to the StringWriter
    //    overall_request_details.RenderControl(hw);

    //    // Write the StringWriter content to the Response output stream
    //    Response.Output.Write(sw.ToString());

    //    // End the response to download the Excel file
    //    Response.End();

    //    // Re-enable the visibility of the last column after the export
    //    overall_request_details.Columns[overall_request_details.Columns.Count - 1].Visible = true;
    //}

    protected void btnExportToExcel_Click_ingred(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=Ingredient.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_component_summary.AllowPaging = false;

        // Hide the last column for export
        //if (GridView_component_summary.Columns.Count > 0)
        //{
        //    GridView_component_summary.Columns[GridView_component_summary.Columns.Count - 1].Visible = false;
        //}

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_component_summary.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_component_summary.Columns[GridView_component_summary.Columns.Count - 1].Visible = true;
    }

    private string RemoveHyperlinksFromHtml(string html)
    {
        // Remove <a> tags but keep the inner text
        string pattern = @"<a[^>]*>(.*?)</a>";
        string result = System.Text.RegularExpressions.Regex.Replace(html, pattern, "$1",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return result;
    }
    protected void btnExportToExcel_Click_design(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=Design.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_design_summary.AllowPaging = false;

        // Hide the last column for export
        //if (GridView_design_summary.Columns.Count > 0)
        //{
        //    GridView_design_summary.Columns[GridView_design_summary.Columns.Count - 1].Visible = false;
        //}

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_design_summary.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_design_summary.Columns[GridView_design_summary.Columns.Count - 1].Visible = true;
    }

    // Export to Excel for Design Summary
    protected void btnExportToExcel_Click_designsummary(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=DesignSummary.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_design_open.AllowPaging = false;

        // Hide the last column for export
        if (GridView_design_open.Columns.Count > 0)
        {
            GridView_design_open.Columns[GridView_design_open.Columns.Count - 1].Visible = false;
        }

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_design_open.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_design_open.Columns[GridView_design_open.Columns.Count - 1].Visible = true;
    }

    protected void btnIssueViewExport_Click(object sender, EventArgs e)
    {
        List<string> requestedColumns;
        try
        {
            requestedColumns = new JavaScriptSerializer().Deserialize<List<string>>(hfIssueExportColumns.Value);
        }
        catch (ArgumentException)
        {
            issueExportMessage.InnerText = "Unable to export: invalid column selection.";
            return;
        }
        catch (InvalidOperationException)
        {
            issueExportMessage.InnerText = "Unable to export: invalid column selection.";
            return;
        }

        var indexes = GetIssueExportColumnIndexes(overall_request_details.Columns, requestedColumns);
        if (indexes.Count == 0)
        {
            issueExportMessage.InnerText = "Select at least one data column to export.";
            return;
        }

        bool paging = overall_request_details.AllowPaging;
        int pageIndex = overall_request_details.PageIndex;
        int editIndex = overall_request_details.EditIndex;
        byte[] bytes;
        try
        {
            overall_request_details.AllowPaging = false;
            overall_request_details.PageIndex = 0;
            overall_request_details.EditIndex = -1;
            ApplyIssuePendingPlatformContext();
            BindGridView(Session["filterValue"] as string, GetAllFilterValues(), bindRelatedGrids: false);
            if (overall_request_details.Rows.Count == 0)
            {
                issueExportMessage.InnerText = "No matching issues to export.";
                return;
            }
            using (var stream = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Spreadsheet.Workbook();
                    var styles = workbookPart.AddNewPart<WorkbookStylesPart>();
                    styles.Stylesheet = new Spreadsheet.Stylesheet(
                        new Spreadsheet.Fonts(new Spreadsheet.Font()),
                        new Spreadsheet.Fills(new Spreadsheet.Fill(new Spreadsheet.PatternFill { PatternType = Spreadsheet.PatternValues.None }),
                            new Spreadsheet.Fill(new Spreadsheet.PatternFill { PatternType = Spreadsheet.PatternValues.Gray125 })),
                        new Spreadsheet.Borders(new Spreadsheet.Border()),
                        new Spreadsheet.CellStyleFormats(new Spreadsheet.CellFormat()),
                        new Spreadsheet.CellFormats(new Spreadsheet.CellFormat {
                            Alignment = new Spreadsheet.Alignment { WrapText = true, Vertical = Spreadsheet.VerticalAlignmentValues.Top },
                            ApplyAlignment = true
                        }));
                    var part = workbookPart.AddNewPart<WorksheetPart>();
                    var data = new Spreadsheet.SheetData();
                    var columns = new Spreadsheet.Columns();
                    for (int i = 0; i < indexes.Count; i++)
                    {
                        string field = GetIssueFieldClass(overall_request_details.Columns[indexes[i]].ItemStyle.CssClass);
                        columns.Append(new Spreadsheet.Column { Min = (uint)i + 1, Max = (uint)i + 1,
                            Width = field == "field-title" || field == "field-status" ? 65 : 24, CustomWidth = true });
                    }
                    part.Worksheet = new Spreadsheet.Worksheet(columns, data);
                    var sheets = workbookPart.Workbook.AppendChild(new Spreadsheet.Sheets());
                    sheets.Append(new Spreadsheet.Sheet { Id = workbookPart.GetIdOfPart(part), SheetId = 1, Name = "Issue List" });
                    var header = new Spreadsheet.Row();
                    foreach (int i in indexes) header.Append(CreatePendingExportCell(overall_request_details.Columns[i].HeaderText));
                    data.Append(header);
                    foreach (GridViewRow row in overall_request_details.Rows)
                    {
                        var exportRow = new Spreadsheet.Row();
                        foreach (int i in indexes) exportRow.Append(CreatePendingExportCell(GetIssueExportCellText(row.Cells[i])));
                        data.Append(exportRow);
                    }
                    styles.Stylesheet.Save();
                    part.Worksheet.Save();
                    workbookPart.Workbook.Save();
                }
                bytes = stream.ToArray();
            }
        }
        finally
        {
            overall_request_details.AllowPaging = paging;
            overall_request_details.PageIndex = pageIndex;
            overall_request_details.EditIndex = editIndex;
        }
        Response.Clear();
        Response.Buffer = true;
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.AddHeader("Content-Disposition", "attachment;filename=IssueList_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".xlsx");
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.BinaryWrite(bytes);
        Response.End();
    }

    private static List<int> GetIssueExportColumnIndexes(DataControlFieldCollection columns, IEnumerable<string> requestedColumns)
    {
        var allowed = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < columns.Count; i++)
        {
            string field = GetIssueFieldClass(columns[i].ItemStyle.CssClass);
            if (columns[i].Visible && !string.IsNullOrEmpty(field) && field != "field-actions") allowed[field] = i;
        }
        var indexes = new List<int>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string field in requestedColumns ?? new List<string>())
        {
            int index;
            if (field != null && allowed.TryGetValue(field, out index) && seen.Add(field)) indexes.Add(index);
        }
        return indexes;
    }

    private static string GetIssueExportCellText(TableCell cell)
    {
        using (var buffer = new StringWriter())
        using (var writer = new HtmlTextWriter(buffer))
        {
            cell.RenderControl(writer);
            string html = System.Text.RegularExpressions.Regex.Replace(buffer.ToString(), @"<button\b[^>]*>[\s\S]*?</button>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<br\s*/?>|</(?:div|p)>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            string text = HttpUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "")).Replace('\u00a0', ' ').Trim();
            return string.IsNullOrWhiteSpace(text) ? "unknown" : text;
        }
    }

    // Export to Excel for CMF Pending List
    protected void btnExportToExcel_Click_cmf_pending(object sender, EventArgs e)
    {
        HashSet<string> selectedIds = null;
        if (sender == btnPendingViewExport)
        {
            try
            {
                var ids = new JavaScriptSerializer().Deserialize<List<string>>(hfPendingExportIds.Value);
                if (ids == null || ids.Count == 0) return;
                selectedIds = new HashSet<string>(ids, StringComparer.Ordinal);
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        BindGridView_cmf_pending();
        byte[] workbookBytes;
        using (var stream = new MemoryStream())
        {
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Spreadsheet.Workbook();
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new Spreadsheet.SheetData();
                var columns = new Spreadsheet.Columns();
                for (int columnIndex = 0; columnIndex < GridView_cmf_pending.Columns.Count; columnIndex++)
                {
                    var field = GridView_cmf_pending.Columns[columnIndex] as BoundField;
                    columns.Append(new Spreadsheet.Column
                    {
                        Min = (uint)columnIndex + 1,
                        Max = (uint)columnIndex + 1,
                        Width = field != null && (field.DataField == "title" || field.DataField == "impact") ? 60 : 24,
                        CustomWidth = true
                    });
                }
                worksheetPart.Worksheet = new Spreadsheet.Worksheet(columns, sheetData);
                var sheets = workbookPart.Workbook.AppendChild(new Spreadsheet.Sheets());
                sheets.Append(new Spreadsheet.Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "CMF Pending List" });
                var headerRow = new Spreadsheet.Row();
                foreach (DataControlField column in GridView_cmf_pending.Columns)
                {
                    headerRow.Append(CreatePendingExportCell(column.HeaderText));
                }
                sheetData.Append(headerRow);

                foreach (GridViewRow gridRow in GridView_cmf_pending.Rows)
                {
                    var keys = GridView_cmf_pending.DataKeys[gridRow.RowIndex];
                    string issueId = Convert.ToString(keys.Values["cp_id"], CultureInfo.InvariantCulture);
                    if (selectedIds != null && !selectedIds.Contains(issueId)) continue;
                    var exportRow = new Spreadsheet.Row();
                    for (int columnIndex = 0; columnIndex < GridView_cmf_pending.Columns.Count; columnIndex++)
                    {
                        var column = GridView_cmf_pending.Columns[columnIndex];
                        string value;
                        if (columnIndex == 0)
                        {
                            value = issueId;
                        }
                        else if (column.ItemStyle.CssClass == "pending-field-rvp")
                        {
                            value = Convert.ToString(keys.Values["repro_on_rvp"], CultureInfo.InvariantCulture);
                        }
                        else if (column.ItemStyle.CssClass == "pending-field-request-date")
                        {
                            var request = (Label)gridRow.FindControl("PendingRequestValue");
                            var date = (Label)gridRow.FindControl("PendingAskDateValue");
                            value = HttpUtility.HtmlDecode(request.Text) + "\n" + date.Text;
                        }
                        else
                        {
                            value = HttpUtility.HtmlDecode(gridRow.Cells[columnIndex].Text).Replace("\u00a0", " ");
                        }
                        exportRow.Append(CreatePendingExportCell(value));
                    }
                    sheetData.Append(exportRow);
                }
                worksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
            }
            workbookBytes = stream.ToArray();
        }

        Response.Clear();
        Response.Buffer = true;
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.AddHeader("Content-Disposition", "attachment;filename=CMFPendingList_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".xlsx");
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetNoStore();
        Response.BinaryWrite(workbookBytes);
        Response.End();
    }

    private static Spreadsheet.Cell CreatePendingExportCell(string value)
    {
        return new Spreadsheet.Cell
        {
            DataType = Spreadsheet.CellValues.InlineString,
            InlineString = new Spreadsheet.InlineString(new Spreadsheet.Text(value ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve })
        };
    }

    // Export to Excel for OEM Summary
    protected void btnExportToExcel_Click_oem(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=OEMSummary.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_oem_summary.AllowPaging = false;

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_oem_summary.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();
    }

    //private string RemoveHyperlinksFromHtml(string html)
    //{
    //    // Remove <a> tags but keep the inner text
    //    string pattern = @"<a[^>]*>(.*?)</a>";
    //    string result = System.Text.RegularExpressions.Regex.Replace(html, pattern, "$1",
    //        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    //    return result;
    //}

    private void BindGridView_design_open(string filtervalue = null)
    {
        // Initialize the fields
        drivers = new List<string>();
        driverColumns = new Dictionary<string, string>();

        int dl;
        int dc = 0;
        string driverList = "(";
        string main_query = "";
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
            {
                Session["filterValue"] = filtervalue;
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
                dl = drivers.Count;
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    dc += 1;
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_"); // Ensure column name is safe
                    driverColumns[driver] = safeColumnName;
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    if (dc != dl)
                    {
                        driverList += "'" + driver + "', ";
                    }
                    else
                    {
                        driverList += "'" + driver + "'";
                    }
                }
                driverList += ")";
                main_query = "SELECT A.customer_detail AS Design, B.sw_image_freeze AS SWImageFreeze, B.support_model AS SupportModel " +
                             driverCaseStatements +
                             ", SUM(CASE WHEN A.status IN ('implemented', 'verified') AND A.drivers in " + driverList + " THEN 1 ELSE 0 END) AS Implemented_Verified " +
                             "FROM " + selectedPlatform + " A INNER JOIN " + designTable + " B ON A.customer_detail = B.customer_detail " +
                             "Where sysdebug Like ('%customer_must_fix%') AND status in ('open','implemented','verified') and cmf_request not in ('cmf_reject') GROUP BY A.customer_detail, B.sw_image_freeze, B.support_model " +
                             "ORDER BY Implemented_Verified DESC  ";
            }
            else
            {
                string ddrivers;
                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";

                using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder driversList = new StringBuilder();

                        while (reader.Read())
                        {
                            string driver = reader["drivers"].ToString();

                            if (driversList.Length > 0)
                            {
                                driversList.Append(", ");
                            }

                            driversList.Append(driver);
                        }

                        ddrivers = driversList.ToString();
                    }
                }

                filtervalue = ddrivers;

                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
                dl = drivers.Count;
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    dc += 1;
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    if (dc != dl)
                    {
                        driverList += "'" + driver + "', ";
                    }
                    else
                    {
                        driverList += "'" + driver + "'";
                    }
                }
                driverList += ")";

                string implementedVerifiedCondition = drivers.Count > 0 ? "A.status IN ('implemented', 'verified') " : "A.status IN ('implemented', 'verified')";

                main_query = "SELECT A.customer_detail AS Design, B.sw_image_freeze AS SWImageFreeze, B.support_model AS SupportModel " +
                             driverCaseStatements +
                             ", SUM(CASE WHEN " + implementedVerifiedCondition + " THEN 1 ELSE 0 END) AS Implemented_Verified " +
                             "FROM " + selectedPlatform + " A INNER JOIN " + designTable + " B ON A.customer_detail = B.customer_detail " +
                             "Where sysdebug Like ('%customer_must_fix%') AND status in ('open','implemented','verified') and cmf_request not in ('cmf_reject') GROUP BY A.customer_detail, B.sw_image_freeze, B.support_model " +
                             "ORDER BY Implemented_Verified DESC ";
            }

            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow totalRow = dt.NewRow();
                        totalRow["Design"] = "Total";
                        totalRow["SWImageFreeze"] = DBNull.Value;
                        totalRow["SupportModel"] = DBNull.Value;

                        if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                        {
                            foreach (var driver in drivers)
                            {
                                string colName = driverColumns[driver] + "_Issues";
                                totalRow[colName] = dt.AsEnumerable().Sum(row => row[colName] != DBNull.Value ? Convert.ToInt32(row[colName]) : 0);
                            }
                        }
                        else
                        {
                            if (dt.Columns.Contains("Driver_issues"))
                            {
                                totalRow["Driver_issues"] = dt.AsEnumerable().Sum(row => row["Driver_issues"] != DBNull.Value ? Convert.ToInt32(row["Driver_issues"]) : 0);
                            }
                        }

                        if (dt.Columns.Contains("Implemented_Verified"))
                        {
                            totalRow["Implemented_Verified"] = dt.AsEnumerable().Sum(row => row["Implemented_Verified"] != DBNull.Value ? Convert.ToInt32(row["Implemented_Verified"]) : 0);
                        }
                        dt.Rows.Add(totalRow);
                    }

                    // Update rows with empty Design
                    //foreach (DataRow row in dt.Rows)
                    //{
                    //    if (string.IsNullOrEmpty(row["Design"].ToString()))
                    //    {
                    //        row["Design"] = "RVP";
                    //        row["SWImageFreeze"] = "NA";
                    //        row["SupportModel"] = "NA";
                    //    }
                    //}

                    GridView_design_open.Columns.Clear();
                    if (dt.Columns.Contains("Design"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "Design", HeaderText = "Design Name", ReadOnly = true });
                    if (dt.Columns.Contains("SWImageFreeze"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "SWImageFreeze", HeaderText = "SW Image Freeze" });
                    if (dt.Columns.Contains("SupportModel"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "SupportModel", HeaderText = "Support Model" });

                    if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                    {
                        foreach (var driver in drivers)
                        {
                            string colName = driverColumns[driver] + "_Issues";
                            if (dt.Columns.Contains(colName))
                            {
                                GridView_design_open.Columns.Add(new BoundField { DataField = colName, HeaderText = driver, ReadOnly = true });
                            }
                        }
                    }
                    else
                    {
                        if (dt.Columns.Contains("Driver_issues"))
                        {
                            GridView_design_open.Columns.Add(new BoundField { DataField = "Driver_issues", HeaderText = "Driver Issues", ReadOnly = true });
                        }
                    }

                    if (dt.Columns.Contains("Implemented_Verified"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "Implemented_Verified", HeaderText = "Impl/Verified", ReadOnly = true });
                    GridView_design_open.Columns.Add(new CommandField { ShowEditButton = true });

                    GridView_design_open.RowDataBound += new GridViewRowEventHandler(GridView_design_open_RowDataBound);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.IsNullOrEmpty(row["Design"].ToString()))
                        {
                            row["Design"] = "RVP";
                            row["SWImageFreeze"] = "NA";
                            row["SupportModel"] = "NA";
                        }
                    }

                    GridView_design_open.DataSource = dt;
                    GridView_design_open.DataBind();

                    GridView2_edit.DataSource = dt;
                    GridView2_edit.DataBind();
                }
            }
        }
    }

    private void BindMilestoneMapping()
    {
        // Uses the exact table/query you provided.
        string platformTable = ResolvePlatformTable(selectedPlatform);

        // If you want to make the table dynamic by platform, swap the table name below with `selectedPlatform`
        // or a platform-specific name. For now, we use the exact table as requested.
        string milestoneQuery = @"
    SELECT 
        LTRIM(RTRIM(drivers)) AS Driver, 
        COUNT(*) AS CMFCount
    FROM " + platformTable + @"
    WHERE 
        ISNULL(LTRIM(RTRIM(drivers)), '') <> '' 
        AND cmf_request IN ('cmf_ok') 
        AND sysdebug LIKE ('%customer_must_fix%')
    GROUP BY LTRIM(RTRIM(drivers))
    ORDER BY LTRIM(RTRIM(drivers));
";

    using (SqlConnection con = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(milestoneQuery, con))
        {
            System.Data.DataTable mapTable = new System.Data.DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(mapTable);

            GridView_milestone_map.DataSource = mapTable;
            GridView_milestone_map.DataBind();
        }
    }
    private void BindGridView_cmf_summary(string filtervalue = null)
    {
        string platformTable = ResolvePlatformTable(selectedPlatform);
        string full_query = "";
        string main_query = "";
        string los_query = "";
        string total_query = "";
        string basePlatform = platformTable.Replace("_ALL_COMPONENTS_TABLE", "");
        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";
        string firstPatternCaseStatements = "";
        string secondPatternNulls = "";
        //string driver_filter = "";

        //CMF Ask Count
        string designTable = basePlatform + "_CMF_ASK";
        string cmf_pending_count = "SELECT COUNT(cp_id) FROM " + designTable + " WHERE status  IN ('open')";

        //(SELECT COUNT(cp_id) FROM " + designTable + " WHERE status NOT IN ('complete','rejected')) AS cmf_pending_count,

        string connectionString = ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string notesquery = @"SELECT (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) FROM " + platformTable + " WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS disp_tpt,(SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + platformTable + " WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS resolve_tpt, (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0) FROM " + platformTable + " WHERE sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok','cmf_duplicate')) AS crit_tpt";

            System.Data.DataTable notestable = new System.Data.DataTable();
            using (SqlCommand cmd = new SqlCommand(notesquery, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(notestable);
                GridView_notes.DataSource = notestable;
                GridView_notes.DataBind();
            }
        }

        using (SqlConnection con2 = new SqlConnection(connectionString))
        {
            //string compquery = @"SELECT component_group, COUNT(cp_id) AS [CMF Pending Count] FROM " + designTable + " WHERE status NOT IN ('complete', 'rejected') GROUP BY component_group";

            string compquery = @"
        SELECT 
            CASE 
                WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
                THEN 'Unassigned' 
                ELSE component_group 
            END AS component_group, 
            COUNT(cp_id) AS [CMF Pending Count] 
        FROM " + designTable + @"  
        WHERE status NOT IN ('complete', 'rejected')
        GROUP BY 
            CASE 
                WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
                THEN 'Unassigned' 
                ELSE component_group 
            END

        UNION ALL

        SELECT 'Total' AS component_group, COUNT(cp_id) AS [CMF Pending Count] 
        FROM " + designTable + @" 
        WHERE status NOT IN ('complete', 'rejected');

        ";

            System.Data.DataTable comptable = new System.Data.DataTable();

            using (SqlCommand cmd2 = new SqlCommand(compquery, con2))
            {
                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                da2.Fill(comptable);
                GridView_comp.DataSource = comptable;
                GridView_comp.DataBind();
            }
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            List<string> drivers = new List<string>();
            Dictionary<string, string> driverColumns = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
            {
                Session["filterValue"] = filtervalue;
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }

                string Driver_name = "";
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;
                    // Issues count (open records only)
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    // Implemented count (implemented records only)
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented], " +
                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_ImplementedDup]";
                    Driver_name = safeColumnName + "_Issues";
                }

                string losCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;

                    losCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

                    Driver_name = safeColumnName + "_LOS";
                }

                firstPatternCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;

                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' AS [" + safeColumnName + "_OpenUnique]";
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) AS [" + safeColumnName + "_OpenDuplicate]";
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_ImplUnique]";
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_ImplementedDup] AS VARCHAR) AS [" + safeColumnName + "_ImplDuplicate]";
                }

                // Build the main query
                full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total'  ELSE Component END AS Component " + firstPatternCaseStatements +
                " FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
                "  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE A.status IN ('open','implemented','verified') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
                "UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
                "  FROM " + selectedPlatform + " A   INNER JOIN " + componentTable + " B         ON A.component_group = B.component_group  WHERE A.status IN ('open','implemented','verified') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject') GROUP BY B.component_group";

                // Add HAVING clause to filter out components with all zeros
                //if (drivers.Count > 0)
                //{
                //    full_query += " HAVING (";

                //    List<string> driverConditions = new List<string>();
                //    foreach (string driver in drivers)
                //    {
                //        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                //        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                //                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
                //    }

                //    full_query += string.Join(" OR ", driverConditions) + ")";
                //}

                //full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";

                // ⬇️ only this line changes
                full_query += ") AS FinalResult ORDER BY CASE WHEN FinalResult.Component = 'Total' THEN 1 ELSE 0 END, Component";


                main_query = full_query;
            }
            else
            {
                string ddrivers;
                List<string> allDrivers = GetDistinctDrivers(platformTable, "open", "implemented", "verified");
                ddrivers = string.Join(", ", allDrivers);

                filtervalue = ddrivers;

                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }

                string Driver_name = "";
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;
                    // Issues count (open records only)
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    // Implemented count (implemented records only)
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented], " +
                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_ImplementedDup]";
                    Driver_name = safeColumnName + "_Issues";
                }

                string losCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;

                    losCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

                    Driver_name = safeColumnName + "_LOS";
                }

                firstPatternCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;

                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' AS [" + safeColumnName + "_OpenUnique]";
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) AS [" + safeColumnName + "_OpenDuplicate]";
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_ImplUnique]";
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_ImplementedDup] AS VARCHAR) AS [" + safeColumnName + "_ImplDuplicate]";
                }

                //// Build the main query
                //full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
                //" FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
                //"  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
                //"UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
                //"  FROM " + selectedPlatform + " A INNER JOIN " + componentTable + " B ON A.component_group = B.component_group WHERE (A.status IN ('open','implemented','verified'))  AND A.sysdebug LIKE '%customer_must_fix%'  AND A.cmf_request NOT IN ('cmf_reject')  AND A.drivers IN ( SELECT DISTINCT drivers FROM " + selectedPlatform + @" WHERE status IN ('open') AND cmf_request NOT IN ('cmf_reject') AND sysdebug LIKE '%customer_must_fix%') GROUP BY B.component_group";



                //// Add HAVING clause to filter out components with all zeros
                //if (drivers.Count > 0)
                //{
                //    full_query += " HAVING (";

                //    List<string> driverConditions = new List<string>();
                //    foreach (string driver in drivers)
                //    {
                //        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                //        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                //                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
                //    }

                //    full_query += string.Join(" OR ", driverConditions) + ")";
                //}

                //// Add HAVING clause to filter out components with no CMFs at all (platform-based)
                ////full_query += @"
                ////    HAVING
                ////        SUM(CASE
                ////                WHEN A.sysdebug LIKE '%customer_must_fix%'
                ////                 AND A.cmf_request NOT IN ('cmf_reject')
                ////               THEN 1
                ////            END) > 0
                ////    ";

                ////full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";
                //full_query += ") AS FinalResult ORDER BY CASE WHEN FinalResult.Component = 'Total' THEN 1 ELSE 0 END, Component";

                full_query =
"SELECT CASE " +
"  WHEN Component = 'Total' THEN 'Total' " +
"  ELSE Component END AS Component " +
firstPatternCaseStatements +

" FROM ( " +

"   SELECT 'Total' AS Component " +
        driverCaseStatements +
        losCaseStatements +
"   FROM " + selectedPlatform + " A " +
"   INNER JOIN " + componentTable + " B " +
"       ON A.component_group = B.component_group " +
"   WHERE A.status IN ('open','implemented','verified') " +
"     AND A.sysdebug LIKE '%customer_must_fix%' " +
"     AND A.cmf_request NOT IN ('cmf_reject') " +

"   UNION ALL " +

"   SELECT A.component_group AS Component " +
        driverCaseStatements +
        losCaseStatements +
"   FROM " + selectedPlatform + " A " +
"   INNER JOIN " + componentTable + " B " +
"       ON A.component_group = B.component_group " +
"   WHERE A.status IN ('open','implemented','verified') " +
"     AND A.sysdebug LIKE '%customer_must_fix%' " +
"     AND A.cmf_request NOT IN ('cmf_reject') " +
"   GROUP BY A.component_group " +

") AS FinalResult " +
"ORDER BY CASE WHEN Component = 'Total' THEN 1 ELSE 0 END, Component";

                main_query = full_query;


            }


            // Summary Query to fetch metrics
            string summaryQuery = @"
        SELECT 
            SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS TotalCount,
            SUM(CASE WHEN [cmf_request] = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS Duplicates,
            SUM(CASE WHEN [cmf_request] in ('cmf_ok', 'cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] NOT IN ('complete', 'rejected', 'implemented')) THEN 1 ELSE 0 END) AS OpenCount,
            SUM(CASE WHEN [cmf_request] in ('cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] NOT IN ('complete', 'rejected', 'implemented')) THEN 1 ELSE 0 END ) AS OpenDup,
            SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedCount,
            SUM(CASE WHEN cmf_request in ('cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedDup,
            SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' THEN 1 ELSE 0 END) AS ImplementedCount,
            SUM(CASE WHEN cmf_request in ('cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' THEN 1 ELSE 0 END) AS ImplementedDup,
            STUFF((
                SELECT ', ' + [component_group]
                FROM " + selectedPlatform + @"
                WHERE cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' 
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ImplementedComponents
        FROM " + selectedPlatform;

            // Fetch summary data
            System.Data.DataTable summaryTable = new System.Data.DataTable();
            using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                {
                    da.SelectCommand.Parameters.AddWithValue("@FilterValue", filtervalue);
                }
                da.Fill(summaryTable);
                //Add custom columns to the summary table
                summaryTable.Columns.Add("Total", typeof(string));
                summaryTable.Columns.Add("Closed", typeof(string));
                summaryTable.Columns.Add("Open", typeof(string));
                //summaryTable.Columns.Add("Implemented", typeof(string));
                //summaryTable.Columns.Add("ImplementedDetails", typeof(string));
                // Add these columns to your DataTable
                summaryTable.Columns.Add("ImplementedCountAndDup", typeof(string));
                summaryTable.Columns.Add("ImplementedComponentsOnly", typeof(string));

                // Then in your data processing:


                if (summaryTable.Rows.Count > 0)
                {
                    DataRow row = summaryTable.Rows[0];
                    // Safely access columns and handle DBNull values
                    int totalCount = row["TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["TotalCount"]);
                    int duplicates = row["Duplicates"] is DBNull ? 0 : Convert.ToInt32(row["Duplicates"]);
                    int openCount = row["OpenCount"] is DBNull ? 0 : Convert.ToInt32(row["OpenCount"]);
                    int openDup = row["OpenDup"] is DBNull ? 0 : Convert.ToInt32(row["OpenDup"]);
                    int closedCount = row["ClosedCount"] is DBNull ? 0 : Convert.ToInt32(row["ClosedCount"]);
                    int closedDup = row["ClosedDup"] is DBNull ? 0 : Convert.ToInt32(row["ClosedDup"]);
                    int implementedCount = row["ImplementedCount"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedCount"]);
                    int implementedDup = row["ImplementedDup"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedDup"]);
                    //string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();
                    string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();

                    // Split the string into a list of components
                    string[] componentsList = implementedComponents.Split(',');

                    // Create a dictionary to count occurrences of each component
                    Dictionary<string, int> componentCounts = new Dictionary<string, int>();

                    // Initialize a variable to store the total count
                    int totalCountComp = 0;

                    foreach (string component in componentsList)
                    {
                        string trimmedComponent = component.Trim(); // Trim spaces
                        if (componentCounts.ContainsKey(trimmedComponent))
                        {
                            componentCounts[trimmedComponent]++; // Increment count
                        }
                        else
                        {
                            componentCounts[trimmedComponent] = 1; // Initialize count
                        }

                        // Increment the total count for each component
                        totalCountComp++;
                    }

                    // Get the original number of components (from the split list)
                    int originalCount = implementedCount;

                    // Build the formatted result using string.Format
                    List<string> resultList = new List<string>();

                    foreach (KeyValuePair<string, int> kvp in componentCounts)
                    {
                        resultList.Add(string.Format("{0} - {1}", kvp.Key, kvp.Value));
                    }

                    // Check if total count does not match the original count
                    if (totalCountComp != originalCount)
                    {
                        int difference = Math.Abs(originalCount - totalCountComp);
                        resultList.Add(string.Format("Other - {0}", difference));
                    }

                    // Join the result into a single string
                    string resultComp = string.Join(", ", resultList);

                    row["Total"] = String.Format("{0} ({1} CMF Duplicates)", totalCount, duplicates);
                    row["Open"] = String.Format("{0} ({1} CMF Duplicates)", openCount, openDup);
                    row["Closed"] = String.Format("{0} ({1} CMF Duplicates)", closedCount, closedDup);
                    //if (implementedCount != 0)
                    //{
                    //    //row["ImplementedDetails"] = String.Format("{0} ({1} CMF Duplicates) - ({2})",
                    //    //    implementedCount, implementedDup, resultComp);
                    //    row["ImplementedDetails"] = String.Format("({0} CMF Duplicates) ({1})",
                    //        implementedDup, resultComp);
                    //}
                    //else
                    //{
                    //    row["ImplementedDetails"] = "";
                    //}

                    if (implementedCount != 0)
                    {
                        row["ImplementedCountAndDup"] = String.Format("{0} ({1} CMF Duplicates)",
                            implementedCount, implementedDup);
                        row["ImplementedComponentsOnly"] = String.Format("({0})", resultComp);
                    }
                    else
                    {
                        row["ImplementedCountAndDup"] = "0 (0 CMF Duplicates)";
                        row["ImplementedComponentsOnly"] = "";
                    }

                }
            }

            using (SqlCommand cmd = new SqlCommand(full_query, conn))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);

                    // Clear existing columns
                    GridView_cmf_summary.Columns.Clear();
                    GridView_cmf_summary.Width = Unit.Percentage(100);

                    double componentWidth = 20.0;
                    double driverGroupWidth = (80.0) / drivers.Count;

                    ViewState["Drivers"] = drivers.ToArray();
                    ViewState["DriverGroupWidth"] = driverGroupWidth;

                    // Add Component column
                    BoundField componentField = new BoundField
                    {
                        DataField = "Component",
                        HeaderText = "Component",
                        ReadOnly = true
                    };
                    componentField.ItemStyle.Width = Unit.Percentage(componentWidth);
                    componentField.HeaderStyle.Width = Unit.Percentage(componentWidth);
                    componentField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                    componentField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                    componentField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                    componentField.HeaderStyle.ForeColor = System.Drawing.Color.White;
                    GridView_cmf_summary.Columns.Add(componentField);

                    // Add columns for each driver dynamically
                    foreach (var driver in drivers)
                    {
                        string safeColumnName = driverColumns[driver];

                        TemplateField openUniqueField = new TemplateField();
                        openUniqueField.HeaderText = "Unique";
                        openUniqueField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        openUniqueField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        openUniqueField.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                        openUniqueField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        openUniqueField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        openUniqueField.HeaderStyle.ForeColor = System.Drawing.Color.White;
                        openUniqueField.ItemTemplate = new CMFIssueCountTemplate(safeColumnName + "_OpenUnique", driver, "OpenUnique");
                        GridView_cmf_summary.Columns.Add(openUniqueField);

                        TemplateField openDuplicateField = new TemplateField();
                        openDuplicateField.HeaderText = "Duplicate";
                        openDuplicateField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        openDuplicateField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        openDuplicateField.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                        openDuplicateField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        openDuplicateField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        openDuplicateField.HeaderStyle.ForeColor = System.Drawing.Color.White;
                        openDuplicateField.ItemTemplate = new CMFIssueCountTemplate(safeColumnName + "_OpenDuplicate", driver, "OpenDuplicate");
                        GridView_cmf_summary.Columns.Add(openDuplicateField);

                        TemplateField implUniqueField = new TemplateField();
                        implUniqueField.HeaderText = "Unique";
                        implUniqueField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        implUniqueField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        implUniqueField.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                        implUniqueField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        implUniqueField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        implUniqueField.HeaderStyle.ForeColor = System.Drawing.Color.White;
                        implUniqueField.ItemTemplate = new CMFIssueCountTemplate(safeColumnName + "_ImplUnique", driver, "ImplementedUnique");
                        GridView_cmf_summary.Columns.Add(implUniqueField);

                        TemplateField implDuplicateField = new TemplateField();
                        implDuplicateField.HeaderText = "Duplicate";
                        implDuplicateField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        implDuplicateField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 4);
                        implDuplicateField.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                        implDuplicateField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        implDuplicateField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        implDuplicateField.HeaderStyle.ForeColor = System.Drawing.Color.White;
                        implDuplicateField.ItemTemplate = new CMFIssueCountTemplate(safeColumnName + "_ImplDuplicate", driver, "ImplementedDuplicate");
                        GridView_cmf_summary.Columns.Add(implDuplicateField);
                    }

                    GridView_cmf_summary.RowCreated -= new GridViewRowEventHandler(GridView_cmf_summary_RowCreated);
                    GridView_cmf_summary.RowCreated += new GridViewRowEventHandler(GridView_cmf_summary_RowCreated);

                    GridView_cmf_summary.DataSource = dt;
                    GridView_cmf_summary.DataBind();
                }
            }

            GridView_cmf_summary1.DataSource = summaryTable;
            GridView_cmf_summary1.DataBind();
            BindMilestoneMapping();
        }
    }

    //    private void BindGridView_cmf_summary(string filtervalue = null)
    //    {
    //        string full_query = "";
    //        string main_query = "";
    //        string los_query = "";
    //        string total_query = "";
    //        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
    //        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";
    //        string firstPatternCaseStatements = "";
    //        string secondPatternNulls = "";
    //        //string driver_filter = "";

    //        //CMF Ask Count
    //        string designTable = basePlatform + "_CMF_ASK";
    //        string cmf_pending_count = "SELECT COUNT(cp_id) FROM " + designTable + " WHERE status  IN ('open')";

    //        //(SELECT COUNT(cp_id) FROM " + designTable + " WHERE status NOT IN ('complete','rejected')) AS cmf_pending_count,

    //        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

    //        using (SqlConnection con = new SqlConnection(connectionString))
    //        {
    //            string notesquery = @"SELECT 
    //(SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) 
    // FROM " + selectedPlatform + @" 
    // WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS disp_tpt,
    //(SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) AS INT)), 0) 
    // FROM " + selectedPlatform + @" 
    // WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS resolve_tpt, 
    //(SELECT ISNULL(AVG(CAST(days_active AS INT)), 0) 
    // FROM " + selectedPlatform + @" 
    // WHERE sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok','cmf_duplicate')) AS crit_tpt";

    //            System.Data.DataTable notestable = new System.Data.DataTable();
    //            using (SqlCommand cmd = new SqlCommand(notesquery, con))
    //            {
    //                SqlDataAdapter da = new SqlDataAdapter(cmd);
    //                da.Fill(notestable);
    //                GridView_notes.DataSource = notestable;
    //                GridView_notes.DataBind();
    //            }
    //        }

    //        using (SqlConnection con2 = new SqlConnection(connectionString))
    //        {
    //            //string compquery = @"SELECT component_group, COUNT(cp_id) AS [CMF Pending Count] FROM " + designTable + " WHERE status NOT IN ('complete', 'rejected') GROUP BY component_group";

    //            string compquery = @"
    //SELECT 
    //    CASE 
    //        WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
    //        THEN 'Unassigned' 
    //        ELSE component_group 
    //    END AS component_group, 
    //    COUNT(cp_id) AS [CMF Pending Count] 
    //FROM " + designTable + @"  
    //WHERE status NOT IN ('complete', 'rejected')
    //GROUP BY 
    //    CASE 
    //        WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
    //        THEN 'Unassigned' 
    //        ELSE component_group 
    //    END

    //UNION ALL

    //SELECT 'Total' AS component_group, COUNT(cp_id) AS [CMF Pending Count] 
    //FROM " + designTable + @" 
    //WHERE status NOT IN ('complete', 'rejected');
    //";

    //            System.Data.DataTable comptable = new System.Data.DataTable();

    //            using (SqlCommand cmd2 = new SqlCommand(compquery, con2))
    //            {
    //                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
    //                da2.Fill(comptable);
    //                GridView_comp.DataSource = comptable;
    //                GridView_comp.DataBind();
    //            }
    //        }

    //        using (SqlConnection conn = new SqlConnection(connectionString))
    //        {
    //            List<string> drivers = new List<string>();
    //            Dictionary<string, string> driverColumns = new Dictionary<string, string>();

    //            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
    //            {
    //                Session["filterValue"] = filtervalue;
    //                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

    //                foreach (string driver in rawDrivers)
    //                {
    //                    string trimmedDriver = driver.Trim();
    //                    if (!string.IsNullOrEmpty(trimmedDriver))
    //                    {
    //                        drivers.Add(trimmedDriver);
    //                    }
    //                }

    //                string Driver_name = "";
    //                string driverCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;
    //                    // Issues count (open records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
    //                    // Implemented count (implemented records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented]";
    //                    Driver_name = safeColumnName + "_Issues";
    //                }

    //                string losCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    losCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
    //                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

    //                    Driver_name = safeColumnName + "_LOS";
    //                }

    //                firstPatternCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    // For Total row: show individual driver totals in the format Open(LOS) + Duplicates
    //                    // For regular rows: show just Open(LOS) format
    //                    firstPatternCaseStatements += ", CASE " +
    //                    "WHEN Component = 'Total' THEN " +
    //                    "CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' + " +
    //                    "' + ' + CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) + ' Duplicates'" +
    //                    " ELSE CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')'" +
    //                    " END AS [" + safeColumnName + "_Issues]";

    //                    // Implemented column - show individual counts for each driver
    //                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_Implemented]";
    //                }

    //                // Build the main query
    //                full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
    //                " FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
    //                "UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A   INNER JOIN " + componentTable + " B         ON A.component_group = B.component_group  WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject') GROUP BY B.component_group";

    //                // Add HAVING clause to filter out components with all zeros
    //                if (drivers.Count > 0)
    //                {
    //                    full_query += " HAVING (";

    //                    List<string> driverConditions = new List<string>();
    //                    foreach (string driver in drivers)
    //                    {
    //                        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
    //                                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
    //                    }

    //                    full_query += string.Join(" OR ", driverConditions) + ")";
    //                }

    //                full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";

    //                main_query = full_query;
    //            }
    //            else
    //            {
    //                string ddrivers;
    //                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open') and cmf_request not in ('cmf_reject') and sysdebug Like ('%customer_must_fix%') ";

    //                using (SqlCommand cmd = new SqlCommand(alldriver_query, conn))
    //                {
    //                    conn.Open();
    //                    using (SqlDataReader reader = cmd.ExecuteReader())
    //                    {
    //                        StringBuilder driversList = new StringBuilder();

    //                        while (reader.Read())
    //                        {
    //                            string driver = reader["drivers"].ToString();

    //                            if (driversList.Length > 0)
    //                            {
    //                                driversList.Append(", ");
    //                            }

    //                            driversList.Append(driver);
    //                        }

    //                        ddrivers = driversList.ToString();
    //                    }
    //                    conn.Close();
    //                }

    //                filtervalue = ddrivers;

    //                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

    //                foreach (string driver in rawDrivers)
    //                {
    //                    string trimmedDriver = driver.Trim();
    //                    if (!string.IsNullOrEmpty(trimmedDriver))
    //                    {
    //                        drivers.Add(trimmedDriver);
    //                    }
    //                }

    //                string Driver_name = "";
    //                string driverCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;
    //                    // Issues count (open records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
    //                    // Implemented count (implemented records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented]";
    //                    Driver_name = safeColumnName + "_Issues";
    //                }

    //                string losCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    losCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
    //                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

    //                    Driver_name = safeColumnName + "_LOS";
    //                }

    //                firstPatternCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    firstPatternCaseStatements += ", CASE " +
    //                    "WHEN Component = 'Total' THEN " +
    //                    "CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' + " +
    //                    "' + ' + CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) + ' Duplicates'" +
    //                    " ELSE CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')'" +
    //                    " END AS [" + safeColumnName + "_Issues]";

    //                    // Implemented column - show individual counts for each driver
    //                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_Implemented]";
    //                }

    //                // Build the main query
    //                full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
    //                " FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
    //                "UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A   INNER JOIN " + componentTable + " B         ON A.component_group = B.component_group  WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject') GROUP BY B.component_group";

    //                // Add HAVING clause to filter out components with all zeros
    //                if (drivers.Count > 0)
    //                {
    //                    full_query += " HAVING (";

    //                    List<string> driverConditions = new List<string>();
    //                    foreach (string driver in drivers)
    //                    {
    //                        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
    //                                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
    //                    }

    //                    full_query += string.Join(" OR ", driverConditions) + ")";
    //                }

    //                full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";

    //                main_query = full_query;
    //            }

    //            // ===========================
    //            // Summary Query to fetch metrics (UPDATED to include DuplicatesClosed & DuplicatesImplemented)
    //            // ===========================
    //            string summaryQuery = @"
    //SELECT 
    //    -- Totals
    //    SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS TotalCount,
    //    SUM(CASE WHEN [cmf_request] = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS Duplicates,

    //    -- Closed metrics
    //    SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%')
    //             AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedCount,
    //    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%')
    //             AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS DuplicatesClosed,

    //    -- Implemented metrics
    //    SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%')
    //             AND [status] = 'implemented' THEN 1 ELSE 0 END) AS ImplementedCount,
    //    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%')
    //             AND [status] = 'implemented' THEN 1 ELSE 0 END) AS DuplicatesImplemented,

    //    -- Implemented components list (for details)
    //    STUFF((
    //        SELECT ', ' + [component_group]
    //        FROM " + selectedPlatform + @"
    //        WHERE cmf_request in ('cmf_ok','cmf_duplicate') 
    //          AND sysdebug Like ('%customer_must_fix%') 
    //          AND [status] = 'implemented' 
    //        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ImplementedComponents
    //FROM " + selectedPlatform;

    //            // Fetch summary data
    //            System.Data.DataTable summaryTable = new System.Data.DataTable();
    //            using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
    //            {
    //                SqlDataAdapter da = new SqlDataAdapter(cmd);

    //                // (No parameter needed here since the query doesn't use @FilterValue)
    //                da.Fill(summaryTable);

    //                // Add custom columns to the summary table
    //                summaryTable.Columns.Add("Total", typeof(string));
    //                summaryTable.Columns.Add("Closed", typeof(string));
    //                summaryTable.Columns.Add("Implemented", typeof(string));         // NEW display column
    //                summaryTable.Columns.Add("ImplementedDetails", typeof(string));

    //                if (summaryTable.Rows.Count > 0)
    //                {
    //                    DataRow row = summaryTable.Rows[0];
    //                    // Safely access columns and handle DBNull values
    //                    int totalCount = row["TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["TotalCount"]);
    //                    int duplicates = row["Duplicates"] is DBNull ? 0 : Convert.ToInt32(row["Duplicates"]);

    //                    int closedCount = row["ClosedCount"] is DBNull ? 0 : Convert.ToInt32(row["ClosedCount"]);
    //                    int duplicatesClosed = row["DuplicatesClosed"] is DBNull ? 0 : Convert.ToInt32(row["DuplicatesClosed"]); // NEW

    //                    int implementedCount = row["ImplementedCount"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedCount"]);
    //                    int duplicatesImplemented = row["DuplicatesImplemented"] is DBNull ? 0 : Convert.ToInt32(row["DuplicatesImplemented"]); // NEW

    //                    string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();

    //                    // Split and count component occurrences for ImplementedDetails
    //                    string[] componentsList = implementedComponents.Split(',');
    //                    Dictionary<string, int> componentCounts = new Dictionary<string, int>();
    //                    int totalCountComp = 0;

    //                    foreach (string component in componentsList)
    //                    {
    //                        string trimmedComponent = component.Trim(); // Trim spaces
    //                        if (string.IsNullOrEmpty(trimmedComponent)) continue;

    //                        if (componentCounts.ContainsKey(trimmedComponent))
    //                        {
    //                            componentCounts[trimmedComponent]++; // Increment count
    //                        }
    //                        else
    //                        {
    //                            componentCounts[trimmedComponent] = 1; // Initialize count
    //                        }

    //                        // Increment the total count for each component
    //                        totalCountComp++;
    //                    }

    //                    // The original count based on ImplementedCount
    //                    int originalCount = implementedCount;

    //                    // Build the formatted result using string.Format
    //                    List<string> resultList = new List<string>();

    //                    foreach (KeyValuePair<string, int> kvp in componentCounts)
    //                    {
    //                        resultList.Add(string.Format("{0} - {1}", kvp.Key, kvp.Value));
    //                    }

    //                    // Check if total count does not match the original count
    //                    if (totalCountComp != originalCount)
    //                    {
    //                        int difference = Math.Abs(originalCount - totalCountComp);
    //                        resultList.Add(string.Format("Other - {0}", difference));
    //                    }

    //                    // Join the result into a single string
    //                    string resultComp = string.Join(", ", resultList);

    //                    // Display strings with duplicates in brackets
    //                    row["Total"] = string.Format("{0} ({1} CMF Duplicates)", totalCount, duplicates);
    //                    row["Closed"] = string.Format("{0} ({1} CMF Duplicates)", closedCount, duplicatesClosed);                   // UPDATED
    //                    row["Implemented"] = string.Format("{0} ({1} CMF Duplicates)", implementedCount, duplicatesImplemented);   // NEW

    //                    if (implementedCount != 0)
    //                    {
    //                        row["ImplementedDetails"] = "(" + resultComp + ")";
    //                    }
    //                    else
    //                    {
    //                        row["ImplementedDetails"] = "";
    //                    }
    //                }
    //            }

    //            using (SqlCommand cmd = new SqlCommand(full_query, conn))
    //            {
    //                System.Data.DataTable dt = new System.Data.DataTable();
    //                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
    //                {
    //                    sda.Fill(dt);

    //                    // Clear existing columns
    //                    GridView_cmf_summary.Columns.Clear();
    //                    GridView_cmf_summary.Width = Unit.Percentage(100);

    //                    double componentWidth = 20.0;
    //                    double driverGroupWidth = (drivers.Count > 0) ? (80.0 / drivers.Count) : 80.0; // guard against division by zero

    //                    // Add Component column
    //                    BoundField componentField = new BoundField
    //                    {
    //                        DataField = "Component",
    //                        HeaderText = "Component_Open_CMFs",
    //                        ReadOnly = true
    //                    };
    //                    componentField.ItemStyle.Width = Unit.Percentage(componentWidth);
    //                    componentField.HeaderStyle.Width = Unit.Percentage(componentWidth);
    //                    componentField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
    //                    componentField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
    //                    componentField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                    componentField.HeaderStyle.ForeColor = System.Drawing.Color.White;
    //                    GridView_cmf_summary.Columns.Add(componentField);

    //                    // Add columns for each driver dynamically
    //                    foreach (var driver in drivers)
    //                    {
    //                        string safeColumnName = driverColumns[driver];

    //                        // Open (LOS) Column
    //                        BoundField openField = new BoundField
    //                        {
    //                            DataField = safeColumnName + "_Issues",
    //                            HeaderText = safeColumnName + "<br/>Open (LOS)",
    //                            ReadOnly = true,
    //                            HtmlEncode = false
    //                        };
    //                        openField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        openField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        openField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        openField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        openField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                        openField.HeaderStyle.ForeColor = System.Drawing.Color.White;
    //                        GridView_cmf_summary.Columns.Add(openField);

    //                        // Implemented/Verified Column
    //                        BoundField implField = new BoundField
    //                        {
    //                            DataField = safeColumnName + "_Implemented",
    //                            HeaderText = safeColumnName + "<br/>Impl/Verified",
    //                            ReadOnly = true,
    //                            HtmlEncode = false
    //                        };
    //                        implField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        implField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        implField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        implField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        implField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                        implField.HeaderStyle.ForeColor = System.Drawing.Color.White;
    //                        GridView_cmf_summary.Columns.Add(implField);
    //                    }

    //                    GridView_cmf_summary.DataSource = dt;
    //                    GridView_cmf_summary.DataBind();
    //                }
    //            }

    //            GridView_cmf_summary1.DataSource = summaryTable;
    //            GridView_cmf_summary1.DataBind();
    //        }
    //    }
    protected void GridView_cmf_summary_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.Header)
        {
            return;
        }

        string[] drivers = ViewState["Drivers"] as string[];
        object widthObj = ViewState["DriverGroupWidth"];
        double driverGroupWidth = widthObj is double ? (double)widthObj : 0.0;

        if (drivers == null || drivers.Length == 0)
        {
            return;
        }

        GridView grid = sender as GridView;
        if (grid == null || grid.Controls.Count == 0)
        {
            return;
        }

        System.Web.UI.WebControls.Table headerTable = grid.Controls[0] as System.Web.UI.WebControls.Table;
        if (headerTable == null)
        {
            return;
        }

        GridViewRow driverHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
        driverHeaderRow.TableSection = TableRowSection.TableHeader;

        TableHeaderCell componentHeaderCell = new TableHeaderCell();
        componentHeaderCell.Text = "Component_Open_CMFs";
        componentHeaderCell.RowSpan = 3;
        componentHeaderCell.Width = Unit.Percentage(20.0);
        componentHeaderCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
        componentHeaderCell.ForeColor = System.Drawing.Color.White;
        componentHeaderCell.HorizontalAlign = HorizontalAlign.Center;
        componentHeaderCell.VerticalAlign = VerticalAlign.Middle;
        componentHeaderCell.Font.Bold = true;
        driverHeaderRow.Cells.Add(componentHeaderCell);

        for (int i = 0; i < drivers.Length; i++)
        {
            TableHeaderCell driverCell = new TableHeaderCell();
            driverCell.Text = drivers[i];
            driverCell.ColumnSpan = 4;
            driverCell.Width = Unit.Percentage(driverGroupWidth);
            driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
            driverCell.ForeColor = System.Drawing.Color.White;
            driverCell.HorizontalAlign = HorizontalAlign.Center;
            driverCell.Font.Bold = true;
            driverHeaderRow.Cells.Add(driverCell);
        }

        GridViewRow metricHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
        metricHeaderRow.TableSection = TableRowSection.TableHeader;

        for (int i = 0; i < drivers.Length; i++)
        {
            TableHeaderCell openCell = new TableHeaderCell();
            openCell.Text = "Open (LOS)";
            openCell.ColumnSpan = 2;
            openCell.Width = Unit.Percentage(driverGroupWidth / 2);
            openCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
            openCell.ForeColor = System.Drawing.Color.White;
            openCell.HorizontalAlign = HorizontalAlign.Left;
            openCell.Font.Bold = true;
            metricHeaderRow.Cells.Add(openCell);

            TableHeaderCell implCell = new TableHeaderCell();
            implCell.Text = "Impl/Verified";
            implCell.ColumnSpan = 2;
            implCell.Width = Unit.Percentage(driverGroupWidth / 2);
            implCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
            implCell.ForeColor = System.Drawing.Color.White;
            implCell.HorizontalAlign = HorizontalAlign.Left;
            implCell.Font.Bold = true;
            metricHeaderRow.Cells.Add(implCell);
        }

        if (e.Row.Cells.Count > 0)
        {
            e.Row.Cells[0].Visible = false;
        }

        for (int i = 1; i < e.Row.Cells.Count; i++)
        {
            e.Row.Cells[i].BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
            e.Row.Cells[i].ForeColor = System.Drawing.Color.White;
            e.Row.Cells[i].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[i].Font.Bold = true;
        }

        headerTable.Rows.AddAt(0, driverHeaderRow);
        headerTable.Rows.AddAt(1, metricHeaderRow);
    }


    private void AddDriverNamesRow()
    {
        try
        {
            List<string> drivers = Session["CurrentDrivers"] as List<string>;

            if (drivers != null && drivers.Count > 0)
            {
                System.Web.UI.WebControls.Table gridTable = GridView_cmf_summary.Controls[0] as System.Web.UI.WebControls.Table;
                if (gridTable != null)
                {
                    // Create a new row for driver names
                    TableRow driverRow = new TableRow();
                    driverRow.TableSection = TableRowSection.TableHeader;

                    // Add empty cell for Component column
                    TableHeaderCell componentCell = new TableHeaderCell();
                    componentCell.Text = "";
                    componentCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                    componentCell.ForeColor = System.Drawing.Color.White;
                    componentCell.HorizontalAlign = HorizontalAlign.Center;
                    componentCell.Font.Bold = true;
                    driverRow.Cells.Add(componentCell);

                    // Add driver names and empty cells
                    foreach (string driver in drivers)
                    {
                        // Driver name cell (above "Open (LOS)")
                        TableHeaderCell driverCell = new TableHeaderCell();
                        driverCell.Text = driver;
                        driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        driverCell.ForeColor = System.Drawing.Color.White;
                        driverCell.HorizontalAlign = HorizontalAlign.Center;
                        driverCell.Font.Bold = true;
                        driverRow.Cells.Add(driverCell);

                        // Empty cell (above "Impl/Verified")
                        TableHeaderCell emptyCell = new TableHeaderCell();
                        emptyCell.Text = "";
                        emptyCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        emptyCell.ForeColor = System.Drawing.Color.White;
                        emptyCell.HorizontalAlign = HorizontalAlign.Center;
                        driverRow.Cells.Add(emptyCell);
                    }

                    // Insert at the top (position 0)
                    gridTable.Rows.AddAt(0, driverRow);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle silently
            System.Diagnostics.Debug.WriteLine("Error adding driver row: " + ex.Message);
        }
    }
    private List<string> GetDriversList()
    {
        List<string> drivers = new List<string>();

        // Try to get from session first
        if (Session["CurrentDrivers"] != null)
        {
            return (List<string>)Session["CurrentDrivers"];
        }

        // If not in session, recreate from filtervalue
        string filtervalue = null;
        if (Session["filterValue"] != null)
        {
            filtervalue = Session["filterValue"].ToString();
        }

        if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
        {
            string[] rawDrivers = filtervalue.Split(new char[] { ',' });
            foreach (string driver in rawDrivers)
            {
                string trimmedDriver = driver.Trim();
                if (!string.IsNullOrEmpty(trimmedDriver))
                {
                    drivers.Add(trimmedDriver);
                }
            }
        }

        return drivers;
    }


    //protected void GridView_cmf_summary_RowCreated(object sender, GridViewRowEventArgs e)
    //{
    //    if (e.Row.RowType == DataControlRowType.Header)
    //    {
    //        // Get the drivers from session or recreate the list
    //        List<string> drivers = GetDriversList(); // You'll need to implement this method

    //        if (drivers != null && drivers.Count > 0)
    //        {
    //            // Create the top header row for driver names
    //            GridViewRow topHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
    //            topHeaderRow.TableSection = TableRowSection.TableHeader;

    //            // Component column header (spans 2 rows)
    //            TableHeaderCell componentCell = new TableHeaderCell();
    //            componentCell.Text = "Component_Open_CMFs";
    //            componentCell.RowSpan = 2;
    //            componentCell.HorizontalAlign = HorizontalAlign.Center;
    //            componentCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //            componentCell.ForeColor = System.Drawing.Color.White;
    //            topHeaderRow.Cells.Add(componentCell);

    //            // Driver headers (each spans 2 columns)
    //            foreach (var driver in drivers)
    //            {
    //                TableHeaderCell driverCell = new TableHeaderCell();
    //                driverCell.Text = driver;
    //                driverCell.ColumnSpan = 2;
    //                driverCell.HorizontalAlign = HorizontalAlign.Center;
    //                driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                driverCell.ForeColor = System.Drawing.Color.White;
    //                topHeaderRow.Cells.Add(driverCell);
    //            }

    //            // Add the top header row
    //            ((Table)GridView_cmf_summary.Controls[0]).Rows.AddAt(0, topHeaderRow);

    //            // Modify the existing header row
    //            e.Row.Cells[0].Visible = false; // Hide the component cell since we're using rowspan
    //        }
    //    }
    //}

    private void AddDriverHeaderRow()
    {
        if (GridView_cmf_summary.Rows.Count > 0)
        {
            // Get the existing header row
            GridViewRow existingHeader = GridView_cmf_summary.HeaderRow;

            // Create new main header row
            GridViewRow mainHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);

            // Component header cell (spans 1 column, 2 rows)
            TableHeaderCell componentCell = new TableHeaderCell();
            componentCell.Text = "Component_Open_CMFs";
            componentCell.RowSpan = 2;
            componentCell.HorizontalAlign = HorizontalAlign.Center;
            componentCell.VerticalAlign = VerticalAlign.Middle;
            componentCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#2a62c2");
            componentCell.ForeColor = System.Drawing.Color.White;
            componentCell.Font.Bold = true;
            componentCell.CssClass = "table-primary-header";
            mainHeaderRow.Cells.Add(componentCell);

            // Get drivers list
            List<string> drivers = new List<string>();
            if (Session["filterValue"] != null)
            {
                string filtervalue = Session["filterValue"].ToString();
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });
                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
            }

            // Add driver header cells (each spans 2 columns)
            foreach (string driver in drivers)
            {
                TableHeaderCell driverCell = new TableHeaderCell();
                driverCell.Text = driver;
                driverCell.ColumnSpan = 2;
                driverCell.HorizontalAlign = HorizontalAlign.Center;
                driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#2a62c2");
                driverCell.ForeColor = System.Drawing.Color.White;
                driverCell.Font.Bold = true;
                driverCell.CssClass = "table-primary-header";
                mainHeaderRow.Cells.Add(driverCell);
            }

            // Remove the component cell from existing header (since it's now in main header with rowspan)
            existingHeader.Cells.RemoveAt(0);

            // Fix: Cast to System.Web.UI.WebControls.Table explicitly
            System.Web.UI.WebControls.Table table = (System.Web.UI.WebControls.Table)GridView_cmf_summary.Controls[0];
            table.Rows.AddAt(0, mainHeaderRow);
        }
    }

    // Resolves display names for owner aliases using a session-scoped cache; only hits AD for uncached aliases.
    private Dictionary<string, string> ResolveOwnerDisplayNamesWithCache(DataTable dt, string[] ownerColumns)
    {
        var cache = Session["OwnerDisplayCache"] as Dictionary<string, string>
                    ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var needed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (DataRow r in dt.Rows)
        {
            foreach (string col in ownerColumns)
            {
                if (!dt.Columns.Contains(col) || r[col] == DBNull.Value) continue;
                string raw = r[col].ToString().Trim();
                if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                if (raw.Contains("@")) raw = raw.Split('@')[0];
                raw = raw.Trim();
                if (!string.IsNullOrEmpty(raw) && !cache.ContainsKey(raw))
                    needed.Add(raw);
            }
        }

        if (needed.Count > 0)
        {
            ScriptManager sm = ScriptManager.GetCurrent(this);
            bool isAsyncPostback = sm != null && sm.IsInAsyncPostBack;

            if (isAsyncPostback)
            {
                foreach (string alias in needed)
                {
                    cache[alias] = alias;
                }

                Session["OwnerDisplayCache"] = cache;
                return cache;
            }

            PrincipalContext ctxGar = null, ctxAmr = null, ctxCcr = null, ctxGer = null;
            try { ctxGar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
            try { ctxAmr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
            try { ctxCcr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
            try { ctxGer = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

            foreach (string alias in needed)
            {
                UserPrincipal up = null;
                if (ctxGar != null) up = UserPrincipal.FindByIdentity(ctxGar, IdentityType.SamAccountName, alias);
                if (up == null && ctxAmr != null) up = UserPrincipal.FindByIdentity(ctxAmr, IdentityType.SamAccountName, alias);
                if (up == null && ctxCcr != null) up = UserPrincipal.FindByIdentity(ctxCcr, IdentityType.SamAccountName, alias);
                if (up == null && ctxGer != null) up = UserPrincipal.FindByIdentity(ctxGer, IdentityType.SamAccountName, alias);
                cache[alias] = (up != null && !string.IsNullOrWhiteSpace(up.DisplayName)) ? up.DisplayName.Trim() : alias;
            }

            if (ctxGar != null) ctxGar.Dispose();
            if (ctxAmr != null) ctxAmr.Dispose();
            if (ctxCcr != null) ctxCcr.Dispose();
            if (ctxGer != null) ctxGer.Dispose();

            Session["OwnerDisplayCache"] = cache;
        }

        return cache;
    }

    private void BindGridView_cmf_pending()
    {
        List<string> pendingPlatforms = GetPendingPlatformSelections();
        string platformSelection = pendingPlatforms[0];
        bool isOverallSelection = pendingPlatforms.Count > 1;
        string basePlatform = isOverallSelection ? string.Empty : platformSelection.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_CMF_ASK";
        string allComponentsTable = basePlatform + "_ALL_COMPONENTS_TABLE";
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string main_query;
            if (isOverallSelection)
            {
                string pendingCteSql = string.Join(" UNION ALL ", pendingPlatforms
                    .Select(platform => BuildPendingAskCteSql(con, platform))
                    .Where(sql => !string.IsNullOrWhiteSpace(sql)));
                string issueCteSql = string.Join(" UNION ALL ", pendingPlatforms.Select(BuildIssueDataCteSql));
                if (string.IsNullOrWhiteSpace(pendingCteSql) || string.IsNullOrWhiteSpace(issueCteSql))
                {
                    GridView_cmf_pending.DataSource = new DataTable();
                    GridView_cmf_pending.DataBind();
                    UpdateCmfPendingKpis();
                    UpdateCmfPendingAccessibilityLinks();
                    return;
                }

                main_query = @"
            WITH pending_source AS (
" + pendingCteSql + @"
            ),
            issue_source AS (
" + issueCteSql + @"
            )
            SELECT 
                d.cp_id, 
                d.source_platform,
                d.title, 
                d.component, 
                d.component_group,
                d.reproducibility,
                a.repro_on_rvp, 
                a.days_active AS days_open,
                d.customer_owner,
                a.idst, 
                d.customer_detail, 
                d.date_cmf_ask, 
                d.cmf_request, 
                d.impact,
                CASE WHEN d.cmf_request = 'cmf_incomplete' THEN ISNULL(NULLIF(d.why_cmf_change, ''), 'NA') ELSE 'NA' END AS why_cmf_change,
                CASE
                    WHEN d.cmf_request = 'cmf_ask' THEN 'iDST'
                    WHEN d.cmf_request = 'cmf_incomplete' THEN 'CE'
                    WHEN d.cmf_request = 'cmf_ok' AND ISNULL(a.sysdebug, '') NOT LIKE '%customer_must_fix%' THEN 'CMF CHAIR'
                    ELSE 'NA'
                END AS pending_reason
            FROM pending_source AS d
            LEFT JOIN issue_source AS a
                ON d.cp_id = a.cp_id
               AND d.source_platform = a.source_platform
            WHERE d.status NOT IN ('complete', 'rejected')
            ORDER BY TRY_CONVERT(datetime, d.date_cmf_ask) DESC, d.cp_id DESC";
            }
            else
            {
                // Modify the query to join the designTable with the ALL_COMPONENTS_TABLE
                main_query = @"
            SELECT 
                d.cp_id, 
                '" + platformSelection + @"' AS source_platform,
                d.title, 
                d.component, 
                d.component_group,
                d.reproducibility,
                a.repro_on_rvp, 
                a.days_active AS days_open,
                d.customer_owner,
                a.idst, 
                d.customer_detail, 
                d.date_cmf_ask, 
                d.cmf_request, 
                d.impact,
                CASE WHEN d.cmf_request = 'cmf_incomplete' THEN ISNULL(NULLIF(d.why_cmf_change, ''), 'NA') ELSE 'NA' END AS why_cmf_change,
                CASE
                    WHEN d.cmf_request = 'cmf_ask' THEN 'iDST'
                    WHEN d.cmf_request = 'cmf_incomplete' THEN 'CE'
                    WHEN d.cmf_request = 'cmf_ok' AND ISNULL(a.sysdebug, '') NOT LIKE '%customer_must_fix%' THEN 'CMF CHAIR'
                    ELSE 'NA'
                END AS pending_reason
            FROM " + designTable + @" AS d
            JOIN " + allComponentsTable + @" AS a ON d.cp_id = a.cp_id
            WHERE d.status NOT IN ('complete', 'rejected')
            ORDER BY d.date_cmf_ask DESC";
            }

            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);

                    var ownerMap = ResolveOwnerDisplayNamesWithCache(dt, new[] { "customer_owner" });

                    // Process data rows to replace customer_owner with display names
                    foreach (DataRow row in dt.Rows)
                    {
                        string raw = (row["customer_owner"] == DBNull.Value) ? string.Empty : row["customer_owner"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw))
                        {
                            row["customer_owner"] = "N/A";
                        }
                        else
                        {
                            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                            if (raw.Contains("@")) raw = raw.Split('@')[0];
                            raw = raw.Trim();

                            string actualName;
                            if (ownerMap.TryGetValue(raw, out actualName))
                                row["customer_owner"] = actualName;
                            else
                                row["customer_owner"] = raw;
                        }
                    }

                    GridView_cmf_pending.DataSource = dt;
                    GridView_cmf_pending.DataBind();

                    // Update KPIs and accessibility links after binding
                    UpdateCmfPendingKpis();
                    UpdateCmfPendingAccessibilityLinks();
                    ScriptManager.RegisterStartupScript(this, GetType(), "initSightingDescriptions",
                        "setTimeout(function() { loadSightingDescriptions(); }, 100);", true);
                }
            }
        }
    }

    private string ResolveSummaryQuickLinkPlatform()
    {
        string summaryPlatform = this.selectedPlatform;

        if (string.IsNullOrWhiteSpace(summaryPlatform))
        {
            summaryPlatform = ResolveDashboardPlatformSelection();
        }

        if (IsOverallDashboardPlatform(summaryPlatform))
        {
            return OverallDashboardPlatformTable;
        }

        if (!string.IsNullOrWhiteSpace(summaryPlatform) && AllowedPlatformTables.Contains(summaryPlatform))
        {
            return summaryPlatform;
        }

        string sessionPlatform = Session["selectedPlatform"] as string;
        if (!string.IsNullOrWhiteSpace(sessionPlatform) && AllowedPlatformTables.Contains(sessionPlatform))
        {
            return sessionPlatform;
        }

        return DefaultPlatformTable;
    }

    private void BindGridView_design_summary(string filtervalue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string summaryPlatform = ResolveSummaryQuickLinkPlatform();
        bool isOverallSummary = IsOverallDashboardPlatform(summaryPlatform);

        if (isOverallSummary)
        {
            string overallSourceSql = "(" + BuildSummaryIssueComponentSourceSql(summaryPlatform) + ") AS src";
            string overallDesignMetadataSql = BuildSummaryDesignMetadataSourceSql(summaryPlatform);
            string overallDesignMetadataSourceSql = "(" + overallDesignMetadataSql + ") AS dm";
            string overallQuery = @"
WITH base AS (
    SELECT
        CASE WHEN src.customer_detail IS NULL OR LTRIM(RTRIM(src.customer_detail)) = '' THEN 'unassigned' ELSE LTRIM(RTRIM(src.customer_detail)) END AS Design,
        ISNULL(NULLIF(LTRIM(RTRIM(dm.sw_image_freeze)), ''), 'N/A') AS sw_image_freeze,
        LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) AS status_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.cmf_request, '')))) AS cmf_request_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.sysdebug, '')))) AS sysdebug_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.closed_reason, '')))) AS closed_reason_norm,
        TRY_CAST(src.date_cmf_ask AS DATE) AS ask_date,
        TRY_CAST(src.implemented_date AS DATE) AS implemented_date
    FROM " + overallSourceSql + @"
    LEFT JOIN " + overallDesignMetadataSourceSql + @" ON LTRIM(RTRIM(ISNULL(src.customer_detail, ''))) = dm.customer_detail
        AND src.source_platform = dm.source_platform
)
SELECT
    Design,
    ISNULL(MAX(CASE WHEN sw_image_freeze = 'N/A' THEN NULL ELSE sw_image_freeze END), 'N/A') AS SWImageFreeze,
    SUM(CASE WHEN cmf_request_norm IN ('cmf_ask', 'cmf_incomplete') AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS Issues_in_CMF_ASK,
    SUM(CASE WHEN cmf_request_norm = 'cmf_reject' THEN 1 ELSE 0 END) AS Total_CMF_REJECT,
    SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) AS Total_CMF_Approved,
    ISNULL(AVG(CAST(CASE WHEN ask_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= ask_date THEN DATEDIFF(DAY, ask_date, implemented_date) END AS DECIMAL(10, 2))), 0) AS crit_tpt,
    CAST(0 AS DECIMAL(10, 2)) AS disp_tpt,
    ISNULL(AVG(CAST(CASE WHEN ask_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= ask_date THEN DATEDIFF(DAY, ask_date, implemented_date) END AS DECIMAL(10, 2))), 0) AS resolve_tpt,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS CMFOpenPercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm NOT LIKE '%internal%'
                         AND closed_reason_norm IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm NOT LIKE '%internal%'
                     AND closed_reason_norm IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS Noise,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND (
                             closed_reason_norm LIKE '%internal%'
                             OR closed_reason_norm IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')
                         )
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND (
                         closed_reason_norm LIKE '%internal%'
                         OR closed_reason_norm IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')
                     )
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS IntelIssuePercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS ThirdPartyPercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS CustomerIssuePercentage
FROM base
GROUP BY Design
ORDER BY Total_CMF_Approved DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(overallQuery, con))
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                sda.Fill(dt);
                NormalizeDashboardMissingValues(dt);

                GridView_design_summary.DataSource = dt;
                GridView_design_summary.DataBind();

                GridView_design_indicator_modal.DataSource = dt;
                GridView_design_indicator_modal.DataBind();
            }

            return;
        }

        string selectedPlatform = FilterDashboardTableSource(summaryPlatform) + " AS dated_summary";
        string basePlatform = summaryPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";
        string swImageFreezeSelect = "B.sw_image_freeze AS SWImageFreeze,";
        string designFromClause = FilterDashboardTableSource(summaryPlatform) + " A INNER JOIN " + designTable + " B ON A.customer_detail = B.customer_detail";
        string designGroupByClause = "A.customer_detail, B.sw_image_freeze";

        string main_query = @"SELECT
                                CASE 
                                    WHEN A.customer_detail IS NULL OR A.customer_detail = '' THEN 'unassigned'
                                    ELSE A.customer_detail
                                END AS Design,  
                                " + swImageFreezeSelect + @" 
                                 
                                (SELECT COUNT(cp_id)
                                 FROM " + selectedPlatform + @"
                                 WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND customer_detail = A.customer_detail AND status not in ('complete','rejected') ) AS Issues_in_CMF_ASK,
                                (SELECT COUNT(cp_id)
                                 FROM " + selectedPlatform + @"
                                 WHERE cmf_request IN('cmf_reject') AND customer_detail = A.customer_detail) AS Total_CMF_REJECT,
                                (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0)
                                 FROM " + selectedPlatform + @"
                                 WHERE sysdebug LIKE('%customer_must_fix%') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail) AS crit_tpt,
                                (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_detail = A.customer_detail) AS disp_tpt, 
                                (SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_detail = A.customer_detail) AS resolve_tpt,
                                (CASE WHEN(SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail AND sysdebug LIKE('%customer_must_fix%')) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail AND sysdebug LIKE('%customer_must_fix%'))
                                          * 100.0, 1) AS VARCHAR) + '%' +' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_detail = A.customer_detail) AS VARCHAR) +'/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail AND sysdebug LIKE('%customer_must_fix%')) AS VARCHAR) +')'
                                END) AS CMFOpenPercentage,
                                (SELECT COUNT(cp_id)
                                 FROM " + selectedPlatform + @"
                                 WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND customer_detail = A.customer_detail) AS Total_CMF_Approved,
                                (CASE WHEN(SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_detail = A.customer_detail) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND(closed_reason NOT LIKE('%internal%')
                                           AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                           AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND customer_detail = A.customer_detail)
                                          * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status IN('complete', 'rejected')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND(closed_reason NOT LIKE('%internal%')
                                                AND sysdebug LIKE('%customer_must_fix%')
                                                AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +'/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +')'
                                END) AS Noise,
                                (CASE WHEN(SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_detail = A.customer_detail) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND(closed_reason LIKE('%internal%')
                                           OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                           AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND customer_detail = A.customer_detail)
                                          * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND(closed_reason LIKE('%internal%')
                                                OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +'/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +')'
                                END) AS IntelIssuePercentage,
                                (CASE WHEN (SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_detail = A.customer_detail) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                           AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND customer_detail = A.customer_detail)
                                          * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status IN('complete', 'rejected')
                                                AND sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                                AND customer_detail = A.customer_detail) AS VARCHAR) + '/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND customer_detail = A.customer_detail) AS VARCHAR) + ')'
                                END) AS ThirdPartyPercentage,
                                (CASE WHEN (SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND customer_detail = A.customer_detail) = 0
                                THEN 'N/A'
                                ELSE CAST(ROUND(
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                    AND customer_detail = A.customer_detail)
                                    /
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND customer_detail = A.customer_detail)
                                    * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE status IN('complete', 'rejected')
                                        AND sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                        AND customer_detail = A.customer_detail) AS VARCHAR) + '/' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_detail = A.customer_detail) AS VARCHAR) + ')'
                        END) AS CustomerIssuePercentage
                            FROM
                                " + designFromClause + @"
                            GROUP BY
                                " + designGroupByClause + @"
                            ORDER BY
                                Total_CMF_Approved desc";

        using (SqlConnection con = new SqlConnection(connectionString))
        {

            using (SqlCommand cmd = new SqlCommand(main_query, con))

            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {

                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    NormalizeDashboardMissingValues(dt);

                    GridView_design_summary.DataSource = dt;
                    GridView_design_summary.DataBind();

                    GridView_design_indicator_modal.DataSource = dt;
                    GridView_design_indicator_modal.DataBind();
                }
            }

        }


    }


    private void BindModalGrid(string design)
    {
        DataTable dt = GetDesignSummaryModalData(design);
        GridView_design_summary_modal.DataSource = dt;
        GridView_design_summary_modal.DataBind();
    }

    private DataTable BindModalGridExcel(string design)
    {
        return GetDesignSummaryModalData(design);
    }
    private void BindModalGrid2(string ingred)
    {
        DataTable dt = GetIngredientSummaryModalData(ingred);
        GridView_component_summary_modal.DataSource = dt;
        GridView_component_summary_modal.DataBind();
    }

    private static string SanitizeCpIdCsv(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string[] tokens = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
        List<string> cleaned = new List<string>();

        for (int i = 0; i < tokens.Length; i++)
        {
            string digits = System.Text.RegularExpressions.Regex.Replace(tokens[i] ?? string.Empty, @"[^0-9]", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(digits) || !unique.Add(digits))
            {
                continue;
            }

            cleaned.Add(digits);
        }

        return string.Join(",", cleaned);
    }

    private const string PendingSummaryDriverTokenPrefix = "__PENDING_SUMMARY_CPIDS__:";

    private static string ExtractPendingSummaryCpIdsFromDriverToken(string driverToken)
    {
        string token = (driverToken ?? string.Empty).Trim();
        if (token.StartsWith(PendingSummaryDriverTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(PendingSummaryDriverTokenPrefix.Length);
        }

        return SanitizeCpIdCsv(token);
    }

    private static string NormalizePendingIssueSortLabel(string rawSortLabel)
    {
        string key = (rawSortLabel ?? string.Empty).Trim().ToLowerInvariant();
        return key == "alphabetically" ? "Alphabetically" : "by Count";
    }

    private static string NormalizePendingIssueGroupingLabel(string rawGroupingLabel, string ingredient)
    {
        string key = (rawGroupingLabel ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            return "Component";
        }

        switch (key.ToLowerInvariant())
        {
            case "issue":
                return "Issue";
            case "platform":
                return "Platform";
            case "milestone":
                return "Milestone";
            case "design":
                return "Design";
            case "component":
                return "Component";
            case "owner":
                return "Owner";
            case "idst":
            case "idst name":
                return "iDST Name";
            case "age bucket":
                return "Age Bucket";
            case "ingredient":
                return "Ingredient";
            default:
                string fallback = key.Trim();
                return string.IsNullOrWhiteSpace(fallback) ? "Component" : fallback;
        }
    }

    private static string BuildIssueListModalTitle(string sortingLabel, string groupingLabel)
    {
        string normalizedSort = NormalizePendingIssueSortLabel(sortingLabel);
        string normalizedGrouping = NormalizePendingIssueGroupingLabel(groupingLabel, null);
        return string.Format(CultureInfo.InvariantCulture, "Issue List (Sorted {0} {1} Wise)", normalizedSort, normalizedGrouping);
    }

    private static void ParsePendingSummaryModalArgument(
        string rawArgument,
        out string ingredient,
        out string cpIdsCsv,
        out string groupingLabel,
        out string sortingLabel)
    {
        ingredient = "Pending Summary";
        cpIdsCsv = string.Empty;
        groupingLabel = "Component";
        sortingLabel = "by Count";

        string token = (rawArgument ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        string[] parts = token.Split(new[] { "||" }, StringSplitOptions.None);
        if (parts.Length >= 2)
        {
            ingredient = HttpUtility.UrlDecode(parts[0] ?? string.Empty);
            cpIdsCsv = HttpUtility.UrlDecode(parts[1] ?? string.Empty);
            if (parts.Length >= 3)
            {
                groupingLabel = HttpUtility.UrlDecode(parts[2] ?? string.Empty);
            }

            if (parts.Length >= 4)
            {
                sortingLabel = HttpUtility.UrlDecode(parts[3] ?? string.Empty);
            }
        }
        else
        {
            cpIdsCsv = token;
        }

        if (string.IsNullOrWhiteSpace(ingredient))
        {
            ingredient = "Pending Summary";
        }

        groupingLabel = NormalizePendingIssueGroupingLabel(groupingLabel, ingredient);
        sortingLabel = NormalizePendingIssueSortLabel(sortingLabel);
        cpIdsCsv = SanitizeCpIdCsv(cpIdsCsv);
    }

    private static void ParseSummaryMetricModalArgument(
        string rawArgument,
        out string metricLabel,
        out string groupName,
        out string cpIdsCsv)
    {
        metricLabel = "Summary Metric";
        groupName = "Unassigned";
        cpIdsCsv = string.Empty;

        string token = (rawArgument ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        string[] parts = token.Split(new[] { "||" }, StringSplitOptions.None);
        if (parts.Length >= 3)
        {
            metricLabel = HttpUtility.UrlDecode(parts[0] ?? string.Empty);
            groupName = HttpUtility.UrlDecode(parts[1] ?? string.Empty);
            cpIdsCsv = HttpUtility.UrlDecode(parts[2] ?? string.Empty);
        }
        else
        {
            cpIdsCsv = HttpUtility.UrlDecode(token);
        }

        metricLabel = string.IsNullOrWhiteSpace(metricLabel) ? "Summary Metric" : metricLabel.Trim();
        groupName = string.IsNullOrWhiteSpace(groupName) ? "Unassigned" : groupName.Trim();
        cpIdsCsv = SanitizeCpIdCsv(cpIdsCsv);
    }

    private static string BuildSummaryMetricModalTitle(string metricLabel, string groupName)
    {
        string safeMetric = string.IsNullOrWhiteSpace(metricLabel) ? "Summary Metric" : metricLabel.Trim();
        string safeGroup = string.IsNullOrWhiteSpace(groupName) ? "Unassigned" : groupName.Trim();
        return string.Format(CultureInfo.InvariantCulture, "Issue List - {0} ({1})", safeMetric, safeGroup);
    }

    private DataTable GetSummaryMetricIssueDetailsData(string cpIdsCsv)
    {
        DataTable dt = new DataTable();
        if (string.IsNullOrWhiteSpace(cpIdsCsv))
        {
            return dt;
        }

        string dashboardPlatform = ResolveDashboardPlatformSelection();
        bool isOverallSelection = IsOverallDashboardPlatform(dashboardPlatform);
        string issueSourceSql = BuildIssueDataSourceSql(dashboardPlatform);

        using (SqlConnection con = new SqlConnection(ConnectionString))
        {
            con.Open();

            string designTable = string.Empty;
            bool includeDesignTable = false;
            if (!isOverallSelection)
            {
                string resolvedPlatform = ResolvePlatformTable(dashboardPlatform);
                string basePlatform = resolvedPlatform.Replace("_ALL_COMPONENTS_TABLE", string.Empty);
                designTable = basePlatform + "_DESIGN_TABLE";
                includeDesignTable = SqlTableExists(con, designTable);
            }

            StringBuilder duplicateDetailsBuilder = new StringBuilder();
            duplicateDetailsBuilder.Append("STUFF((\n");
            duplicateDetailsBuilder.Append("    SELECT ', ' + CAST(dup_src.cp_id AS VARCHAR(64))\n");
            duplicateDetailsBuilder.Append("         + ' | ' + ISNULL(NULLIF(LTRIM(RTRIM(dup_src.customer_detail)), ''), 'Unknown')\n");
            if (includeDesignTable)
            {
                duplicateDetailsBuilder.Append("         + ' | ' + ISNULL(CONVERT(VARCHAR(10), dup_design.sw_image_freeze, 120), 'Unknown')\n");
            }
            else
            {
                duplicateDetailsBuilder.Append("         + ' | Unknown'\n");
            }

            duplicateDetailsBuilder.Append("    FROM " + issueSourceSql + " AS dup_src\n");
            if (includeDesignTable)
            {
                duplicateDetailsBuilder.Append("    LEFT JOIN " + designTable + " AS dup_design ON dup_design.customer_detail = dup_src.customer_detail\n");
            }

            duplicateDetailsBuilder.Append("    WHERE LOWER(LTRIM(RTRIM(ISNULL(dup_src.cmf_request, '')))) = 'cmf_duplicate'\n");
            duplicateDetailsBuilder.Append("      AND CAST(dup_src.merge_id AS VARCHAR(64)) = CAST(issue_src.cp_id AS VARCHAR(64))\n");
            if (isOverallSelection)
            {
                duplicateDetailsBuilder.Append("      AND dup_src.source_platform = issue_src.source_platform\n");
            }

            duplicateDetailsBuilder.Append("    FOR XML PATH('')\n");
            duplicateDetailsBuilder.Append("), 1, 2, '') AS DuplicateDetails");

            string imageFreezeSelect = includeDesignTable
                ? "ISNULL(NULLIF(LTRIM(RTRIM(design_src.sw_image_freeze)), ''), 'Unknown') AS ImageFreeze"
                : "CAST('Unknown' AS NVARCHAR(64)) AS ImageFreeze";

            string sql = @"
SELECT
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.drivers)), ''), 'Unassigned') AS Milestone,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.progress)), ''), 'NA') AS Progress,
    CAST(issue_src.cp_id AS VARCHAR(64)) AS SightingID,
    ISNULL(CASE
        WHEN LOWER(LTRIM(RTRIM(ISNULL(issue_src.cmf_request, '')))) = 'cmf_duplicate'
            THEN NULLIF(CAST(issue_src.merge_id AS VARCHAR(64)), '')
        ELSE NULLIF(CAST(issue_src.promoted_id AS VARCHAR(64)), '')
    END, 'N/A') AS PromotedID,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.customer_detail)), ''), 'N/A') AS CustomerDetail,
    " + imageFreezeSelect + @",
    " + duplicateDetailsBuilder.ToString() + @",
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.customer_company)), ''), 'N/A') AS CustomerCompany,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.title)), ''), 'N/A') AS Title,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.component)), ''), 'N/A') AS Component,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.component_group)), ''), 'Unassigned') AS Ingredient,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.customer_owner)), ''), 'N/A') AS Owner,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.owners_name)), ''), 'N/A') AS PromotedOwner,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.repro_on_rvp)), ''), 'N/A') AS RvpRepro,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.status)), ''), ISNULL(NULLIF(LTRIM(RTRIM(issue_src.cmf_status)), ''), 'N/A')) AS Status,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.idst)), ''), 'N/A') AS Idst,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.los)), ''), 'N/A') AS Los,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.processor)), ''), 'N/A') AS Processor,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.impact)), ''), 'N/A') AS Impact,
    ISNULL(CAST(issue_src.days_active AS VARCHAR(32)), 'N/A') AS DaysOpen,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.closed_reason)), ''), 'N/A') + CHAR(13) + CHAR(10)
        + CASE
            WHEN LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) = 'rejected' THEN 'NA'
            ELSE ISNULL(NULLIF(LTRIM(RTRIM(issue_src.fixed_in_version)), ''), 'N/A')
          END AS ClosedDetails
FROM " + issueSourceSql + @" AS issue_src
" + (includeDesignTable ? ("LEFT JOIN " + designTable + " AS design_src ON design_src.customer_detail = issue_src.customer_detail") : string.Empty) + @"
INNER JOIN (
    SELECT DISTINCT LTRIM(RTRIM(value)) AS cp_id
    FROM STRING_SPLIT(@cpIds, ',')
    WHERE LTRIM(RTRIM(value)) <> ''
) selected_ids ON CAST(issue_src.cp_id AS VARCHAR(64)) = selected_ids.cp_id
ORDER BY
    LOWER(LTRIM(RTRIM(ISNULL(issue_src.title, '')))),
    TRY_CAST(issue_src.cp_id AS BIGINT),
    CAST(issue_src.cp_id AS VARCHAR(64));";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@cpIds", cpIdsCsv);
                da.Fill(dt);
            }
        }

        return dt;
    }

    private void BindSummaryMetricIssues(string rawArgument)
    {
        string metricLabel;
        string groupName;
        string cpIdsCsv;
        ParseSummaryMetricModalArgument(rawArgument, out metricLabel, out groupName, out cpIdsCsv);

        lblSummaryMetricIssuesTitle.Text = Server.HtmlEncode(BuildSummaryMetricModalTitle(metricLabel, groupName));

        DataTable dt = GetSummaryMetricIssueDetailsData(cpIdsCsv);
        GridView_summaryMetricIssues.DataSource = dt;
        GridView_summaryMetricIssues.DataBind();
    }

    private void ShowSummaryMetricIssuesModal()
    {
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowSummaryMetricIssuesModal",
            "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModalSummaryMetrics').modal('show');", true);
    }

    private static bool TryParsePendingAgeBucketRange(string bucketLabel, out int minDays, out int maxDays, out bool isGreaterThanRange)
    {
        minDays = 0;
        maxDays = 0;
        isGreaterThanRange = false;

        string normalized = (bucketLabel ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        normalized = normalized.Replace("days", string.Empty).Trim();

        if (normalized.StartsWith("0-1", StringComparison.OrdinalIgnoreCase) || normalized.StartsWith("0 - 1", StringComparison.OrdinalIgnoreCase))
        {
            minDays = 0;
            maxDays = 1;
            return true;
        }

        if (normalized.StartsWith("1-2", StringComparison.OrdinalIgnoreCase) || normalized.StartsWith("1 - 2", StringComparison.OrdinalIgnoreCase))
        {
            minDays = 2;
            maxDays = 2;
            return true;
        }

        if (normalized.StartsWith("2-4", StringComparison.OrdinalIgnoreCase) || normalized.StartsWith("2 - 4", StringComparison.OrdinalIgnoreCase))
        {
            minDays = 3;
            maxDays = 4;
            return true;
        }

        if (normalized.StartsWith("> 4", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith(">4", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("over 4", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("4+", StringComparison.OrdinalIgnoreCase))
        {
            minDays = 4;
            isGreaterThanRange = true;
            return true;
        }

        return false;
    }

    private void BindPendingAgeBucketIssues(string rawBucketArgument)
    {
        string bucketLabel = HttpUtility.UrlDecode(rawBucketArgument ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(bucketLabel))
        {
            bucketLabel = "Pending Age Bucket";
        }

        List<string> cpIds = new List<string>();
        int minDays;
        int maxDays;
        bool isGreaterThanRange;

        if (TryParsePendingAgeBucketRange(bucketLabel, out minDays, out maxDays, out isGreaterThanRange))
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                string pendingSourceSql = BuildPendingAskSourceSql(con, ResolveDashboardPlatformSelection(), "pending_bucket_src");
                if (!string.IsNullOrWhiteSpace(pendingSourceSql))
                {
                    string agePredicate = isGreaterThanRange
                        ? "DATEDIFF(DAY, TRY_CAST(pending_bucket_src.date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) > @minDays"
                        : "DATEDIFF(DAY, TRY_CAST(pending_bucket_src.date_cmf_ask AS DATE), CAST(GETDATE() AS DATE)) BETWEEN @minDays AND @maxDays";

                    string sql = @"
SELECT DISTINCT CAST(pending_bucket_src.cp_id AS VARCHAR(64)) AS cp_id
FROM " + pendingSourceSql + @"
WHERE LOWER(LTRIM(RTRIM(ISNULL(pending_bucket_src.status, '')))) NOT IN ('complete', 'rejected')
  AND TRY_CAST(pending_bucket_src.date_cmf_ask AS DATE) IS NOT NULL
    AND " + GetDashboardDateFilter().BuildPredicate("pending_bucket_src") + @"
    AND " + agePredicate + @"
ORDER BY cp_id";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@minDays", minDays);
                        if (!isGreaterThanRange)
                        {
                            cmd.Parameters.AddWithValue("@maxDays", maxDays);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string cpId = ReadString(reader, 0).Trim();
                                if (!string.IsNullOrWhiteSpace(cpId))
                                {
                                    cpIds.Add(cpId);
                                }
                            }
                        }
                    }
                }
            }
        }

        cpIds = cpIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, Comparer<string>.Create(CompareIssueIds))
            .ToList();

        string cpIdsCsv = SanitizeCpIdCsv(string.Join(",", cpIds.ToArray()));
        string pendingArgument = HttpUtility.UrlEncode(bucketLabel)
            + "||" + HttpUtility.UrlEncode(cpIdsCsv)
            + "||" + HttpUtility.UrlEncode("Age Bucket")
            + "||" + HttpUtility.UrlEncode("by Count");
        BindPendingSummaryIssues(pendingArgument);
    }

    private void BindPendingSummaryIssues(string rawArgument)
    {
        string ingredient;
        string cpIdsCsv;
        string groupingLabel;
        string sortingLabel;
        ParsePendingSummaryModalArgument(rawArgument, out ingredient, out cpIdsCsv, out groupingLabel, out sortingLabel);

        string pendingDriverToken = PendingSummaryDriverTokenPrefix + cpIdsCsv;
        HiddenComponentName.Value = ingredient;
        HiddenDriverName.Value = pendingDriverToken;
        HiddenIssueType.Value = "PendingSummary";
        Session["ComponentName"] = ingredient;
        Session["DriverName"] = pendingDriverToken;
        Session["IssueType"] = "PendingSummary";

        if (btnExportDriverIssues1 != null)
        {
            btnExportDriverIssues1.Visible = true;
        }

        lblDriverName1.Text = Server.HtmlEncode("Issue List - " + ingredient + " (Pending)");

        DataTable dt = new DataTable();
        if (!string.IsNullOrWhiteSpace(cpIdsCsv))
        {
            string dashboardPlatform = ResolveDashboardPlatformSelection();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                string pendingSourceSql = BuildPendingAskSourceSql(con, dashboardPlatform, "pending_src");
                string issueSourceSql = BuildIssueDataSourceSql(dashboardPlatform, "issue_src");

                if (!string.IsNullOrWhiteSpace(pendingSourceSql))
                {
                    string sql = @"
SELECT
    CAST(pending_src.cp_id AS VARCHAR(64)) AS cp_id,
    pending_src.title,
    ISNULL(NULLIF(LTRIM(RTRIM(pending_src.customer_detail)), ''), 'N/A') AS customer_detail,
    pending_src.component,
    ISNULL(NULLIF(LTRIM(RTRIM(pending_src.component_group)), ''), 'Unassigned') AS component_group,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.idst)), ''), 'Unassigned') AS idst,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.owners_name)), ''), ISNULL(NULLIF(LTRIM(RTRIM(pending_src.customer_owner)), ''), 'N/A')) AS customer_owner,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.repro_on_rvp)), ''), 'N/A') AS repro_on_rvp,
    ISNULL(NULLIF(LTRIM(RTRIM(pending_src.reproducibility)), ''), 'N/A') AS reproducibility,
    ISNULL(FORMAT(TRY_CONVERT(DATETIME, pending_src.date_cmf_ask), 'M/d/yyyy h:mm:ss tt'), 'N/A') AS date_cmf_ask,
    pending_src.cmf_request,
    CASE
        WHEN pending_src.cmf_request = 'cmf_incomplete' THEN ISNULL(NULLIF(LTRIM(RTRIM(pending_src.why_cmf_change)), ''), 'NA')
        ELSE 'NA'
    END AS why_cmf_change,
    CASE
        WHEN pending_src.cmf_request = 'cmf_ask' THEN 'iDST'
        WHEN pending_src.cmf_request = 'cmf_incomplete' THEN 'CE'
        WHEN pending_src.cmf_request = 'cmf_ok' AND ISNULL(issue_src.sysdebug, '') NOT LIKE '%customer_must_fix%' THEN 'CMF CHAIR'
        ELSE 'NA'
    END AS pending_reason,
    ISNULL(NULLIF(LTRIM(RTRIM(pending_src.impact)), ''), 'N/A') AS justification
FROM " + pendingSourceSql + @"
LEFT JOIN " + issueSourceSql + @" ON CAST(issue_src.cp_id AS VARCHAR(64)) = CAST(pending_src.cp_id AS VARCHAR(64))
INNER JOIN (
    SELECT DISTINCT LTRIM(RTRIM(value)) AS cp_id
    FROM STRING_SPLIT(@cpIds, ',')
    WHERE LTRIM(RTRIM(value)) <> ''
) ids ON CAST(pending_src.cp_id AS VARCHAR(64)) = ids.cp_id
WHERE LOWER(LTRIM(RTRIM(ISNULL(pending_src.status, '')))) NOT IN ('complete', 'rejected')
ORDER BY TRY_CONVERT(DATETIME, pending_src.date_cmf_ask) DESC,
         TRY_CAST(pending_src.cp_id AS BIGINT) DESC,
         CAST(pending_src.cp_id AS VARCHAR(64)) DESC;";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        cmd.Parameters.AddWithValue("@cpIds", cpIdsCsv);
                        da.Fill(dt);
                    }
                }
            }
        }

            if (dt.Rows.Count > 0 && dt.Columns.Contains("customer_owner"))
            {
                var ownerMap = ResolveOwnerDisplayNamesWithCache(dt, new[] { "customer_owner" });
                foreach (DataRow row in dt.Rows)
                {
                    string raw = row["customer_owner"] == DBNull.Value ? string.Empty : row["customer_owner"].ToString().Trim();
                    if (string.IsNullOrEmpty(raw))
                    {
                        row["customer_owner"] = "N/A";
                        continue;
                    }

                    if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                    if (raw.Contains("@")) raw = raw.Split('@')[0];
                    raw = raw.Trim();

                    string actualName;
                    if (ownerMap.TryGetValue(raw, out actualName))
                        row["customer_owner"] = actualName;
                    else
                        row["customer_owner"] = raw;
                }
            }

            NormalizeCmfIssuesGridSchema(dt);

        GridView_cmf_issues.DataSource = dt;
        GridView_cmf_issues.DataBind();
    }

    private void BindCMFIssues(string component, string driver, string issueType)
    {
        if (btnExportDriverIssues1 != null)
        {
            btnExportDriverIssues1.Visible = true;
        }

        bool includeAllMilestones = string.Equals(driver, AllMilestonesDriverToken, StringComparison.OrdinalIgnoreCase);
        string driverLabel = includeAllMilestones ? "All Milestones" : driver;
        string modalTitle = BuildIssueListModalTitle("by Count", "Milestone") + " - " + component + " - " + driverLabel + " (" + issueType + ")";
        lblDriverName1.Text = Server.HtmlEncode(modalTitle);

        string connectionString = ConnectionString;
        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "A");
        string issueSourceSqlForEnrichment = BuildIssueDataSourceSql(selectedPlatform);

        string sql = "";

                if (issueType == "OpenDuplicate")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND status IN ('open')
    AND cmf_request = 'cmf_duplicate'
    AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.status IN ('open')
    AND A.cmf_request = 'cmf_duplicate'
    AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "OpenCombined")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND status IN ('open')
    AND cmf_request != 'cmf_reject'
    AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.status IN ('open')
    AND A.cmf_request != 'cmf_reject'
    AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "OpenUnique" || issueType == "Open")
        {
            if (component == "Total")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND drivers = @Driver
  AND status IN ('open')
    AND cmf_request != 'cmf_duplicate'
    AND cmf_request != 'cmf_reject'
  AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND A.drivers = @Driver
    AND A.component_group = @Component
  AND A.status IN ('open')
  AND A.cmf_request != 'cmf_duplicate'
  AND A.cmf_request != 'cmf_reject'
  AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
            }
        }
                else if (issueType == "ImplementedDuplicate")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND cmf_request = 'cmf_duplicate'
    AND sysdebug LIKE '%customer_must_fix%'
    AND status IN ('implemented','verified')
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.cmf_request = 'cmf_duplicate'
    AND A.sysdebug LIKE '%customer_must_fix%'
    AND A.status IN ('implemented','verified')
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "ImplementedCombined")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND cmf_request IN ('cmf_ok', 'cmf_duplicate')
    AND sysdebug LIKE '%customer_must_fix%'
    AND status IN ('implemented','verified')
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.cmf_request IN ('cmf_ok', 'cmf_duplicate')
    AND A.sysdebug LIKE '%customer_must_fix%'
    AND A.status IN ('implemented','verified')
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "ImplementedUnique" || issueType == "Implemented")
        {
            if (component == "Total")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND drivers = @Driver
  AND cmf_request = 'cmf_ok'
  AND sysdebug LIKE '%customer_must_fix%'
  AND status IN ('implemented','verified')
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND A.drivers = @Driver
    AND A.component_group = @Component
  AND A.cmf_request = 'cmf_ok'
  AND A.sysdebug LIKE '%customer_must_fix%'
  AND A.status IN ('implemented','verified')
ORDER BY A.cmf_request desc";
            }
        }

        if (includeAllMilestones)
        {
            sql = RemoveDriverFilterForAllMilestones(sql);
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            object driverParam = (driver != null) ? (object)driver.Trim() : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            if (component != "Total")
            {
                object componentParam = (component != null) ? (object)component.Trim() : (object)DBNull.Value;
                cmd.Parameters.Add("@Component", SqlDbType.NVarChar, 256).Value = componentParam;
            }

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                EnrichCmfIssuesGridData(dt, issueSourceSqlForEnrichment);
                NormalizeCmfIssuesGridSchema(dt);
                GridView_cmf_issues.DataSource = dt;
                GridView_cmf_issues.DataBind();
            }
        }

        // Show modal using your existing modal
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowCMFIssuesModal",
            "$('#detailsModalDrivers1').modal('show');", true);
    }

    private void EnrichCmfIssuesGridData(DataTable table, string issueSourceSql)
    {
        if (table == null || table.Rows.Count == 0 || string.IsNullOrWhiteSpace(issueSourceSql))
        {
            return;
        }

        if (!table.Columns.Contains("cp_id"))
        {
            return;
        }

        List<string> cpIds = new List<string>();
        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            string cpId = Convert.ToString(table.Rows[rowIndex]["cp_id"], CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(cpId))
            {
                continue;
            }

            string normalized = cpId.Trim();
            if (seen.Add(normalized))
            {
                cpIds.Add(normalized);
            }
        }

        if (cpIds.Count == 0)
        {
            return;
        }

        DataTable detailTable = new DataTable();
        string detailSql = @"
SELECT
    CAST(issue_src.cp_id AS VARCHAR(64)) AS cp_id,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.customer_detail)), ''), 'N/A') AS customer_detail,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.component_group)), ''), 'Unassigned') AS component_group,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.idst)), ''), 'Unassigned') AS idst,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.customer_owner)), ''), 'N/A') AS customer_owner,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.repro_on_rvp)), ''), 'N/A') AS repro_on_rvp,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.reproducibility)), ''), 'N/A') AS reproducibility,
    ISNULL(CONVERT(VARCHAR(10), TRY_CONVERT(DATE, issue_src.date_cmf_ask), 120), 'N/A') AS date_cmf_ask,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.impact)), ''), 'N/A') AS why_cmf_change,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.cmf_status)), ''), ISNULL(NULLIF(LTRIM(RTRIM(issue_src.status)), ''), 'N/A')) AS pending_reason,
    ISNULL(NULLIF(LTRIM(RTRIM(issue_src.customer_affected)), ''), 'N/A') AS justification
FROM " + issueSourceSql + @" AS issue_src
INNER JOIN (
    SELECT DISTINCT LTRIM(RTRIM(value)) AS cp_id
    FROM STRING_SPLIT(@cpIds, ',')
    WHERE LTRIM(RTRIM(value)) <> ''
) ids ON CAST(issue_src.cp_id AS VARCHAR(64)) = ids.cp_id";

        using (SqlConnection con = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(detailSql, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            cmd.Parameters.AddWithValue("@cpIds", string.Join(",", cpIds.ToArray()));
            da.Fill(detailTable);
        }

        Dictionary<string, DataRow> detailsByCpId = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < detailTable.Rows.Count; i++)
        {
            DataRow detailRow = detailTable.Rows[i];
            string cpId = Convert.ToString(detailRow["cp_id"], CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(cpId))
            {
                continue;
            }

            string normalized = cpId.Trim();
            if (!detailsByCpId.ContainsKey(normalized))
            {
                detailsByCpId[normalized] = detailRow;
            }
        }

        string[] enrichColumns = new string[]
        {
            "customer_detail",
            "component_group",
            "idst",
            "customer_owner",
            "repro_on_rvp",
            "reproducibility",
            "date_cmf_ask",
            "why_cmf_change",
            "pending_reason",
            "justification"
        };

        for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            DataRow row = table.Rows[rowIndex];
            string cpId = Convert.ToString(row["cp_id"], CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(cpId))
            {
                continue;
            }

            DataRow detailRow;
            if (!detailsByCpId.TryGetValue(cpId.Trim(), out detailRow))
            {
                continue;
            }

            for (int colIndex = 0; colIndex < enrichColumns.Length; colIndex++)
            {
                string columnName = enrichColumns[colIndex];
                if (!table.Columns.Contains(columnName))
                {
                    table.Columns.Add(columnName, typeof(string));
                }

                string enrichedValue = Convert.ToString(detailRow[columnName], CultureInfo.InvariantCulture);
                string currentValue = Convert.ToString(row[columnName], CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(currentValue) || string.Equals(currentValue, "N/A", StringComparison.OrdinalIgnoreCase))
                {
                    row[columnName] = string.IsNullOrWhiteSpace(enrichedValue) ? "N/A" : enrichedValue;
                }
            }
        }
    }

    private static string RemoveDriverFilterForAllMilestones(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return sql;
        }

        return System.Text.RegularExpressions.Regex.Replace(
            sql,
            @"\s+AND\s+(?:A\.)?drivers\s*=\s*@Driver\s*",
            Environment.NewLine,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static void EnsureGridColumn(DataTable table, string columnName, string defaultValue)
    {
        if (table == null || string.IsNullOrWhiteSpace(columnName))
        {
            return;
        }

        if (!table.Columns.Contains(columnName))
        {
            table.Columns.Add(columnName, typeof(string));
        }

        for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            DataRow row = table.Rows[rowIndex];
            string current = Convert.ToString(row[columnName], CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(current))
            {
                row[columnName] = defaultValue;
            }
        }
    }

    private static void NormalizeCmfIssuesGridSchema(DataTable table)
    {
        if (table == null)
        {
            return;
        }

        EnsureGridColumn(table, "cp_id", "N/A");
        EnsureGridColumn(table, "title", "N/A");
        EnsureGridColumn(table, "customer_detail", "N/A");
        EnsureGridColumn(table, "component", "N/A");
        EnsureGridColumn(table, "component_group", "N/A");
        EnsureGridColumn(table, "idst", "N/A");
        EnsureGridColumn(table, "customer_owner", "N/A");
        EnsureGridColumn(table, "repro_on_rvp", "N/A");
        EnsureGridColumn(table, "reproducibility", "N/A");
        EnsureGridColumn(table, "date_cmf_ask", "N/A");
        EnsureGridColumn(table, "cmf_request", "N/A");
        EnsureGridColumn(table, "why_cmf_change", "N/A");
        EnsureGridColumn(table, "pending_reason", "N/A");
        EnsureGridColumn(table, "justification", "N/A");
    }

    private void ShowCMFIssues()
    {
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowCMFIssuesModal",
            "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModalDrivers1').modal('show');", true);
    }

    private void ExportCMFIssues(string component, string driver, string issueType)
    {
        try
        {
            DataTable dt = GetCMFIssuesData(component, driver, issueType);

            Response.Clear();
            Response.Buffer = true;

            string safeComponent = (component ?? "").Replace("\"", "");
            string safeDriver = (driver ?? "").Replace("\"", "");
            if (string.Equals(issueType, "PendingSummary", StringComparison.OrdinalIgnoreCase))
            {
                safeDriver = "PendingSummary";
            }
            string fileName = "CMF_Issues_" + safeComponent + "_" + safeDriver + "_" + issueType + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                GridView gv = new GridView();
                gv.AutoGenerateColumns = false;
                gv.EnableViewState = false;

                HyperLinkField linkField = new HyperLinkField();
                linkField.DataTextField = "cp_id";
                linkField.HeaderText = "Sighting ID";
                linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                linkField.Target = "_blank";
                gv.Columns.Add(linkField);

                BoundField bfTitle = new BoundField();
                bfTitle.DataField = "title";
                bfTitle.HeaderText = "Title";
                gv.Columns.Add(bfTitle);

                BoundField bfCmfReq = new BoundField();
                bfCmfReq.DataField = "cmf_request";
                bfCmfReq.HeaderText = "CMF Request";
                gv.Columns.Add(bfCmfReq);

                BoundField bfLOS = new BoundField();
                bfLOS.DataField = "los";
                bfLOS.HeaderText = "LOS";
                gv.Columns.Add(bfLOS);

                BoundField bfComponent = new BoundField();
                bfComponent.DataField = "component";
                bfComponent.HeaderText = "Component";
                gv.Columns.Add(bfComponent);

                gv.DataSource = dt;
                gv.DataBind();
                gv.RenderControl(hw);

                Response.ContentEncoding = System.Text.Encoding.Unicode;
                Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during CMF export: " + ex.Message);
        }
    }


    private DataTable GetCMFIssuesData(string component, string driver, string issueType)
    {
        string connectionString = ConnectionString;
        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "A");

        // Use the same SQL logic as in BindCMFIssues
        string sql = "";
        bool requiresDriverParam = true;
        bool requiresComponentParam = component != "Total";
        string pendingCpIdsCsv = string.Empty;

            if (issueType == "PendingSummary")
            {
                pendingCpIdsCsv = ExtractPendingSummaryCpIdsFromDriverToken(driver);
                if (string.IsNullOrWhiteSpace(pendingCpIdsCsv))
                {
                    return new DataTable();
                }

                sql = @"
    SELECT
        CAST(A.cp_id AS VARCHAR(64)) AS cp_id,
        A.title,
        A.cmf_request,
        ISNULL(NULLIF(LTRIM(RTRIM(A.los)), ''), 'No') AS los,
        A.component_group AS component
    FROM " + issueSourceSql + @"
    INNER JOIN (
        SELECT DISTINCT LTRIM(RTRIM(value)) AS cp_id
        FROM STRING_SPLIT(@cpIds, ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ) ids ON CAST(A.cp_id AS VARCHAR(64)) = ids.cp_id
    ORDER BY TRY_CAST(A.cp_id AS BIGINT), CAST(A.cp_id AS VARCHAR(64));";

                requiresDriverParam = false;
                requiresComponentParam = false;
            }

            else if (issueType == "OpenDuplicate")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND status IN ('open')
    AND cmf_request = 'cmf_duplicate'
    AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.status IN ('open')
    AND A.cmf_request = 'cmf_duplicate'
    AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "OpenCombined")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND status IN ('open')
    AND cmf_request != 'cmf_reject'
    AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.status IN ('open')
    AND A.cmf_request != 'cmf_reject'
    AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "OpenUnique" || issueType == "Open")
        {
            if (component == "Total")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND drivers = @Driver
  AND status IN ('open')
    AND cmf_request != 'cmf_duplicate'
    AND cmf_request != 'cmf_reject'
  AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND A.drivers = @Driver
    AND A.component_group = @Component
  AND A.status IN ('open')
  AND A.cmf_request != 'cmf_duplicate'
  AND A.cmf_request != 'cmf_reject'
  AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
            }
        }
                else if (issueType == "ImplementedDuplicate")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND cmf_request = 'cmf_duplicate'
    AND sysdebug LIKE '%customer_must_fix%'
    AND status IN ('implemented','verified')
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.cmf_request = 'cmf_duplicate'
    AND A.sysdebug LIKE '%customer_must_fix%'
    AND A.status IN ('implemented','verified')
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "ImplementedCombined")
                {
                        if (component == "Total")
                        {
                                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
    AND drivers = @Driver
    AND cmf_request IN ('cmf_ok', 'cmf_duplicate')
    AND sysdebug LIKE '%customer_must_fix%'
    AND status IN ('implemented','verified')
ORDER BY cmf_request desc";
                        }
                        else
                        {
                                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
    AND A.drivers = @Driver
        AND A.component_group = @Component
    AND A.cmf_request IN ('cmf_ok', 'cmf_duplicate')
    AND A.sysdebug LIKE '%customer_must_fix%'
    AND A.status IN ('implemented','verified')
ORDER BY A.cmf_request desc";
                        }
                }
                                else if (issueType == "ImplementedUnique" || issueType == "Implemented")
        {
            if (component == "Total")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND drivers = @Driver
  AND cmf_request = 'cmf_ok'
  AND sysdebug LIKE '%customer_must_fix%'
  AND status IN ('implemented','verified')
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, A.component_group as component
FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND A.drivers = @Driver
    AND A.component_group = @Component
  AND A.cmf_request = 'cmf_ok'
  AND A.sysdebug LIKE '%customer_must_fix%'
  AND A.status IN ('implemented','verified')
ORDER BY A.cmf_request desc";
            }
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            if (requiresDriverParam)
            {
                object driverParam = (driver != null) ? (object)driver.Trim() : (object)DBNull.Value;
                cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;
            }

            if (requiresComponentParam)
            {
                object componentParam = (component != null) ? (object)component.Trim() : (object)DBNull.Value;
                cmd.Parameters.Add("@Component", SqlDbType.NVarChar, 256).Value = componentParam;
            }

            if (!string.IsNullOrWhiteSpace(pendingCpIdsCsv))
            {
                cmd.Parameters.AddWithValue("@cpIds", pendingCpIdsCsv);
            }

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
    private void BindDriverIssues(string driver)
    {
        lblDriverName.Text = Server.HtmlEncode(driver);

        string connectionString = ConnectionString;
        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "A");

        string sql = @"
SELECT cp_id, title, component
    FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND cmf_request IN ('cmf_ok')
  AND sysdebug LIKE '%customer_must_fix%'
  AND LTRIM(RTRIM(drivers)) = @Driver
ORDER BY cp_id";

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            object driverParam = (driver != null) ? (object)driver.Trim() : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                GridView_driver_issues.DataSource = dt;
                GridView_driver_issues.DataBind();
            }
        }

        // Show modal
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowDriverIssuesModal",
            "$('#detailsModalDrivers').modal('show');", true);
    }

    // Show the modal (Bootstrap)
    private void ShowDriverIssues()
    {
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowDriverIssuesModal",
            "if (typeof window.hoistDashboardModalsToRoot === 'function') { window.hoistDashboardModalsToRoot(); } $('#detailsModalDrivers').modal('show');", true);
    }

    private DataTable GetDriverIssuesData(string driver)
    {
        string connectionString = ConnectionString;
        string issueSourceSql = BuildIssueDataSourceSql(selectedPlatform, "A");

        string sql = @"
SELECT cp_id, title, component
    FROM " + issueSourceSql + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND cmf_request IN ('cmf_ok')
  AND sysdebug LIKE '%customer_must_fix%'
  AND LTRIM(RTRIM(drivers)) = @Driver
ORDER BY cp_id";

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            object driverParam = (driver != null) ? (object)driver.Trim() : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    private void ExportDriverIssues(string driver)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export Driver Issues started.");

            DataTable dt = GetDriverIssuesData(driver);

            Response.Clear();
            Response.Buffer = true;

            string safeDriver = (driver ?? "").Replace("\"", "");
            string fileName = "Driver_Issues_" + safeDriver + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                GridView gv = new GridView();
                gv.AutoGenerateColumns = false;
                gv.EnableViewState = false;

                HyperLinkField linkField = new HyperLinkField();
                linkField.DataTextField = "cp_id";
                linkField.HeaderText = "Sighting ID";
                linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                linkField.Target = "_blank";
                gv.Columns.Add(linkField);

                BoundField bfTitle = new BoundField();
                bfTitle.DataField = "title";
                bfTitle.HeaderText = "Title";
                gv.Columns.Add(bfTitle);

                // ADD THIS: Component column
                BoundField bfComponent = new BoundField();
                bfComponent.DataField = "component";
                bfComponent.HeaderText = "Component";
                gv.Columns.Add(bfComponent);

                gv.DataSource = dt;
                gv.DataBind();
                gv.RenderControl(hw);

                Response.ContentEncoding = System.Text.Encoding.Unicode;
                Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End(); // or use CompleteRequest() to avoid ThreadAbortException
            }

            System.Diagnostics.Debug.WriteLine("Export Driver Issues completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during driver export: " + ex.Message);
        }
    }

    private DataTable BindModalGridExcel2(string ingred)
    {
        return GetIngredientSummaryModalData(ingred);
    }

    private void BindModalGrid3(string oem)
    {
        DataTable dt = GetOemSummaryModalData(oem);
        GridView_oem_summary_modal7.DataSource = dt;
        GridView_oem_summary_modal7.DataBind();
    }

    private DataTable BindModalGridExcel3(string oem)
    {
        return GetOemSummaryModalData(oem);
    }

    private void BindModalGrid3_LegacyRemovedAnchor()
    {
    }

    private static string MapLegacyOemModalTriggerToMetric(string trigger)
    {
        switch ((trigger ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "_trg1":
                return "total";
            case "_trg2":
                return "pending";
            case "_trg3":
                return "reject";
            case "_trg4":
                return "open";
            case "_trg5":
                return "noise";
            case "_trg6":
                return "intel";
            case "_trg7":
                return "thirdparty";
            case "_trg8":
                return "customer";
            default:
                return "total";
        }
    }

    private static string NormalizeOemSummaryMetricKey(string metricKey)
    {
        string normalized = (metricKey ?? string.Empty).Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "total":
            case "total cmfs":
            case "total_cmfs":
                return "total";
            case "pending":
                return "pending";
            case "reject":
            case "rejected":
                return "reject";
            case "open":
                return "open";
            case "implemented":
                return "implemented";
            case "closed":
                return "closed";
            case "noise":
                return "noise";
            case "intel":
            case "intelissue":
            case "intel_issue":
                return "intel";
            case "thirdparty":
            case "3rdparty":
            case "3rd_party":
                return "thirdparty";
            case "customer":
            case "customerissue":
            case "customer_issue":
                return "customer";
            default:
                return "total";
        }
    }

    private const string OverallOemAllToken = "__all_oems__";

    private static void ParseOemSummaryModalArgument(string rawArgument, out string oemName, out string metricKey)
    {
        string token = (rawArgument ?? string.Empty).Trim();
        oemName = string.Empty;
        metricKey = "total";

        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        string[] modernParts = token.Split(new[] { "||" }, StringSplitOptions.None);
        if (modernParts.Length >= 2)
        {
            oemName = (modernParts[0] ?? string.Empty).Trim();
            metricKey = NormalizeSummaryMetricKey(modernParts[1]);
            if (string.IsNullOrWhiteSpace(metricKey))
            {
                metricKey = "total";
            }

            if (string.IsNullOrWhiteSpace(oemName))
            {
                oemName = "Unassigned";
            }

            return;
        }

        string[] legacyTriggers = { "_trg1", "_trg2", "_trg3", "_trg4", "_trg5", "_trg6", "_trg7", "_trg8" };
        for (int index = 0; index < legacyTriggers.Length; index++)
        {
            string legacyTrigger = legacyTriggers[index];
            if (!token.EndsWith(legacyTrigger, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            oemName = token.Substring(0, token.Length - legacyTrigger.Length).Trim();
            metricKey = MapLegacyOemModalTriggerToMetric(legacyTrigger);
            if (string.IsNullOrWhiteSpace(oemName))
            {
                oemName = "Unassigned";
            }

            return;
        }

        oemName = token;
        if (string.IsNullOrWhiteSpace(oemName))
        {
            oemName = "Unassigned";
        }
    }

    private DataTable GetOemSummaryModalData(string oem)
    {
        string parsedOem;
        string parsedMetric;
        ParseOemSummaryModalArgument(oem, out parsedOem, out parsedMetric);
        bool includeAllOems = string.Equals(parsedOem, OverallOemAllToken, StringComparison.OrdinalIgnoreCase);

        // Keep drilldown source aligned with the Overall OEM table aggregation source.
        string issueSourceSql = BuildIssueDataSourceSql(OverallDashboardPlatformTable, "issue_src");
        string normalizedMetric = NormalizeOemSummaryMetricKey(parsedMetric);

        StringBuilder whereClause = new StringBuilder();
        whereClause.Append("WHERE ");

        if (string.Equals(normalizedMetric, "pending", StringComparison.OrdinalIgnoreCase))
        {
            whereClause.Append("LOWER(LTRIM(RTRIM(ISNULL(issue_src.cmf_request, '')))) IN ('cmf_ask', 'cmf_incomplete') ");
            whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) NOT IN ('complete', 'rejected') ");
        }
        else if (string.Equals(normalizedMetric, "reject", StringComparison.OrdinalIgnoreCase))
        {
            whereClause.Append("LOWER(LTRIM(RTRIM(ISNULL(issue_src.cmf_request, '')))) = 'cmf_reject' ");
        }
        else
        {
            whereClause.Append("LOWER(LTRIM(RTRIM(ISNULL(issue_src.sysdebug, '')))) LIKE '%customer_must_fix%' ");
            whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.cmf_request, '')))) IN ('cmf_ok', 'cmf_duplicate') ");

            if (string.Equals(normalizedMetric, "open", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) NOT IN ('complete', 'rejected', 'implemented') ");
            }
            else if (string.Equals(normalizedMetric, "implemented", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) = 'implemented' ");
            }
            else if (string.Equals(normalizedMetric, "closed", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) IN ('complete', 'rejected') ");
            }
            else if (string.Equals(normalizedMetric, "noise", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) IN ('complete', 'rejected') ");
                whereClause.Append("AND (LOWER(LTRIM(RTRIM(ISNULL(issue_src.closed_reason, '')))) NOT LIKE '%internal%' ");
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.closed_reason, '')))) IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) ");
            }
            else if (string.Equals(normalizedMetric, "intel", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) IN ('complete', 'rejected') ");
                whereClause.Append("AND (LOWER(LTRIM(RTRIM(ISNULL(issue_src.closed_reason, '')))) LIKE '%internal%' ");
                whereClause.Append("OR LOWER(LTRIM(RTRIM(ISNULL(issue_src.closed_reason, '')))) IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) ");
            }
            else if (string.Equals(normalizedMetric, "thirdparty", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) IN ('complete', 'rejected') ");
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.closed_reason, '')))) IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue') ");
            }
            else if (string.Equals(normalizedMetric, "customer", StringComparison.OrdinalIgnoreCase))
            {
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.status, '')))) IN ('complete', 'rejected') ");
                whereClause.Append("AND LOWER(LTRIM(RTRIM(ISNULL(issue_src.closed_reason, '')))) IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug') ");
            }
        }

        StringBuilder sql = new StringBuilder();
        sql.Append(@"SELECT
    CAST(issue_src.cp_id AS VARCHAR(64)) AS cp_id,
    issue_src.title,
    issue_src.status,
    issue_src.component,
    issue_src.cmf_request,
    issue_src.customer_owner,
    issue_src.customer_company,
    issue_src.promoted_id,
    issue_src.closed_reason,
    issue_src.days_active,
    issue_src.idst,
    issue_src.drivers
FROM ");
        sql.Append(issueSourceSql);
    sql.Append("\n");
    sql.Append(whereClause.ToString());
    if (!includeAllOems)
    {
        sql.Append(@"
AND (
    (@oem = 'Unassigned' AND ISNULL(LTRIM(RTRIM(issue_src.customer_company)), '') = '')
    OR LOWER(LTRIM(RTRIM(ISNULL(issue_src.customer_company, '')))) = LOWER(@oem)
)");
    }

        sql.Append(@"
ORDER BY
    TRY_CAST(issue_src.cp_id AS BIGINT),
    CAST(issue_src.cp_id AS VARCHAR(64))");

        DataTable dt = new DataTable();
        using (SqlConnection con = new SqlConnection(ConnectionString))
        using (SqlCommand cmd = new SqlCommand(sql.ToString(), con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            if (!includeAllOems)
            {
                cmd.Parameters.AddWithValue("@oem", parsedOem ?? "Unassigned");
            }

            da.Fill(dt);
        }

        return dt;
    }

    private void ExportToExcel(string design)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export to Excel started.");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=IssueList.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Get the data from BindModalGrid2 method
                    DataTable dt = BindModalGridExcel(design);

                    // Bind data to a temporary GridView
                    GridView gridView = new GridView();
                    gridView.DataSource = dt;
                    gridView.AutoGenerateColumns = false;

                    // Add HyperLinkField for cp_id
                    HyperLinkField linkField = new HyperLinkField();
                    linkField.DataTextField = "cp_id";
                    linkField.HeaderText = "Sighting ID";
                    linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                    linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                    linkField.Target = "_blank";
                    gridView.Columns.Add(linkField);

                    // Add remaining BoundFields
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "cp_id")
                        {
                            BoundField bf = new BoundField();
                            bf.DataField = col.ColumnName;
                            bf.HeaderText = col.ColumnName;
                            gridView.Columns.Add(bf);
                        }
                    }

                    gridView.DataBind();
                    gridView.EnableViewState = false;

                    // Render the GridView to HTML
                    gridView.RenderControl(hw);

                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            System.Diagnostics.Debug.WriteLine("Export to Excel completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during export: " + ex.Message);
        }
        finally
        {
            Session["DesignValue"] = null;
        }
    }

    private void ExportToExcel2(string design)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export to Excel started.");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=IssueList.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Get the data from BindModalGrid2 method
                    DataTable dt = BindModalGridExcel2(design);

                    // Bind data to a temporary GridView
                    GridView gridView = new GridView();
                    gridView.DataSource = dt;
                    gridView.AutoGenerateColumns = false;

                    // Add HyperLinkField for cp_id
                    HyperLinkField linkField = new HyperLinkField();
                    linkField.DataTextField = "cp_id";
                    linkField.HeaderText = "Sighting ID";
                    linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                    linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                    linkField.Target = "_blank";
                    gridView.Columns.Add(linkField);

                    // Add remaining BoundFields
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "cp_id")
                        {
                            BoundField bf = new BoundField();
                            bf.DataField = col.ColumnName;
                            bf.HeaderText = col.ColumnName;
                            gridView.Columns.Add(bf);
                        }
                    }

                    gridView.DataBind();
                    gridView.EnableViewState = false;

                    // Render the GridView to HTML
                    gridView.RenderControl(hw);

                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            System.Diagnostics.Debug.WriteLine("Export to Excel completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during export: " + ex.Message);
        }
        finally
        {
            Session["DesignValue"] = null;
        }
    }

    private void ExportToExcel3(string design)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export to Excel started.");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=IssueList.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Get the data from BindModalGrid2 method
                    DataTable dt = BindModalGridExcel3(design);

                    // Bind data to a temporary GridView
                    GridView gridView = new GridView();
                    gridView.DataSource = dt;
                    gridView.AutoGenerateColumns = false;

                    // Add HyperLinkField for cp_id
                    HyperLinkField linkField = new HyperLinkField();
                    linkField.DataTextField = "cp_id";
                    linkField.HeaderText = "Sighting ID";
                    linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                    linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                    linkField.Target = "_blank";
                    gridView.Columns.Add(linkField);

                    // Add remaining BoundFields
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "cp_id")
                        {
                            BoundField bf = new BoundField();
                            bf.DataField = col.ColumnName;
                            bf.HeaderText = col.ColumnName;
                            gridView.Columns.Add(bf);
                        }
                    }

                    gridView.DataBind();
                    gridView.EnableViewState = false;

                    // Render the GridView to HTML
                    gridView.RenderControl(hw);

                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            System.Diagnostics.Debug.WriteLine("Export to Excel completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during export: " + ex.Message);
        }
        finally
        {
            Session["DesignValue"] = null;
        }
    }
    protected void GridViewdesign_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        string designfilter = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column index for "Total CMF_Approved"
            int colIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "Total CMF_Approved")
                {
                    colIndex = i;
                    break;
                }
            }

            if (colIndex != -1)
            {
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                designfilter = design;

                //string link = "<a href='Details.aspx?sku=" + Server.UrlEncode(meSku) + "'>" + val + "</a>";

                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";


                e.Row.Cells[colIndex].Text = link;
            }
        }

    }

    protected void GridView5_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string trigger = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column indices for both red and yellow highlighted columns
            int colIndexApproved = -1;
            int colIndexASK = -1;
            int colIndexReject = -1;
            int colIndexCMFOpen = -1;
            int colIndexNoise = -1;
            int colIndexIntelIssue = -1;
            int colIndex3rdParty = -1;
            int colIndexCustomerIssue = -1;
            int colIndexDispTPT = -1;
            int colIndexResolveTPT = -1;
            int colIndexTotalTPT = -1;

            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                string headerText = grid.HeaderRow.Cells[i].Text.Trim();

                if (headerText == "Total CMF_Approved")
                {
                    colIndexApproved = i;
                }
                else if (headerText == "Issues in CMF_ASK")
                {
                    colIndexASK = i;
                }
                else if (headerText == "Total CMF_REJECT")
                {
                    colIndexReject = i;
                }
                else if (headerText == "CMF Open %")
                {
                    colIndexCMFOpen = i;
                }
                else if (headerText == "Noise%")
                {
                    colIndexNoise = i;
                }
                else if (headerText == "Intel Issue %")
                {
                    colIndexIntelIssue = i;
                }
                else if (headerText == "3rd Party %")
                {
                    colIndex3rdParty = i;
                }
                else if (headerText == "Customer Issue %")
                {
                    colIndexCustomerIssue = i;
                }
            }

            colIndexDispTPT = FindColumnIndexByHeaderContains(grid, "disposition tpt");
            colIndexResolveTPT = FindColumnIndexByHeaderContains(grid, "resolution tpt");
            colIndexTotalTPT = FindColumnIndexByHeaderContains(grid, "overall tpt");

            bool isOverallSummary = IsOverallDashboardPlatform(ResolveSummaryQuickLinkPlatform());

            // Modify the columns for red highlighted sections
            if (colIndexApproved != -1)
            {
                trigger = "_trg1";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";

                e.Row.Cells[colIndexApproved].Text = link;
            }

            if (colIndexASK != -1)
            {
                trigger = "_trg2";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Issues_in_CMF_ASK");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";

                e.Row.Cells[colIndexASK].Text = link;
            }

            if (colIndexReject != -1)
            {
                trigger = "_trg3";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_REJECT");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";

                e.Row.Cells[colIndexReject].Text = link;
            }

            // Modify the columns for yellow highlighted sections
            if (colIndexCMFOpen != -1)
            {
                trigger = "_trg4";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CMFOpenPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCMFOpen].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexNoise != -1)
            {
                trigger = "_trg5";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Noise");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexNoise].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexIntelIssue != -1)
            {
                trigger = "_trg6";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "IntelIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexIntelIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndex3rdParty != -1)
            {
                trigger = "_trg7";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "ThirdPartyPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndex3rdParty].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexCustomerIssue != -1)
            {
                trigger = "_trg8";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CustomerIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCustomerIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            ApplyTptCellStyle(e.Row, colIndexDispTPT, 2m, isOverallSummary);
            ApplyTptCellStyle(e.Row, colIndexResolveTPT, 18m, isOverallSummary);
            ApplyTptCellStyle(e.Row, colIndexTotalTPT, 21m, isOverallSummary);
            NormalizeGridRowMissingValues(e.Row);
        }
    }

    // Helper method to extract the numerator from the ratio string
    private string ExtractNumerator(string ratio)
    {
        if (string.IsNullOrEmpty(ratio))
        {
            return "0";
        }

        // Find the opening parenthesis
        int startIndex = ratio.IndexOf('(');
        if (startIndex == -1)
        {
            return "0";
        }

        // Find the slash
        int slashIndex = ratio.IndexOf('/', startIndex);
        if (slashIndex == -1)
        {
            return "0";
        }

        // Extract the numerator
        string numerator = ratio.Substring(startIndex + 1, slashIndex - startIndex - 1).Trim();
        return numerator;
    }

    private static int FindColumnIndexByHeaderContains(GridView grid, string fragment)
    {
        if (grid == null || grid.HeaderRow == null || string.IsNullOrWhiteSpace(fragment))
        {
            return -1;
        }

        string target = fragment.Trim().ToLowerInvariant();
        for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
        {
            string header = HttpUtility.HtmlDecode(grid.HeaderRow.Cells[i].Text ?? string.Empty)
                .Trim()
                .ToLowerInvariant();

            if (header.Contains(target))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool TryParseGridCellDecimal(TableCell cell, out decimal value)
    {
        value = 0m;
        if (cell == null)
        {
            return false;
        }

        string raw = HttpUtility.HtmlDecode(cell.Text ?? string.Empty)
            .Replace("\u00A0", " ")
            .Replace(",", string.Empty)
            .Trim();

        if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "N/A", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }

    private static void ApplyTptCellStyle(GridViewRow row, int columnIndex, decimal threshold, bool normalizeDisplayForOverall)
    {
        if (row == null || columnIndex < 0 || columnIndex >= row.Cells.Count)
        {
            return;
        }

        TableCell cell = row.Cells[columnIndex];
        decimal numericValue;
        if (!TryParseGridCellDecimal(cell, out numericValue))
        {
            return;
        }

        if (normalizeDisplayForOverall)
        {
            cell.Text = Math.Round(numericValue, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);
        }

        cell.ForeColor = numericValue > threshold ? System.Drawing.Color.Red : System.Drawing.Color.Green;
    }

    private static string FormatPercentRatioOneDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string trimmed = value.Trim();
        if (string.Equals(trimmed, "N/A", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        int percentIndex = trimmed.IndexOf('%');
        if (percentIndex <= 0)
        {
            return value;
        }

        string numericPart = trimmed.Substring(0, percentIndex).Trim();
        decimal percentage;
        if (!(decimal.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out percentage)
            || decimal.TryParse(numericPart, NumberStyles.Any, CultureInfo.CurrentCulture, out percentage)))
        {
            return value;
        }

        string suffix = trimmed.Substring(percentIndex + 1).TrimStart();
        string formattedPercent = percentage.ToString("0.0", CultureInfo.InvariantCulture) + "%";
        return string.IsNullOrEmpty(suffix) ? formattedPercent : (formattedPercent + " " + suffix);
    }

    private static bool IsMissingValueToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        string normalized = value.Trim();
        return string.Equals(normalized, "N/A", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "NA", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDashboardPercentColumn(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            return false;
        }

        return string.Equals(columnName, "CMFOpenPercentage", StringComparison.OrdinalIgnoreCase)
            || string.Equals(columnName, "Noise", StringComparison.OrdinalIgnoreCase)
            || string.Equals(columnName, "IntelIssuePercentage", StringComparison.OrdinalIgnoreCase)
            || string.Equals(columnName, "ThirdPartyPercentage", StringComparison.OrdinalIgnoreCase)
            || string.Equals(columnName, "CustomerIssuePercentage", StringComparison.OrdinalIgnoreCase);
    }

    private static void NormalizeDashboardMissingValues(DataTable table)
    {
        if (table == null)
        {
            return;
        }

        for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            DataRow row = table.Rows[rowIndex];
            for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
            {
                DataColumn column = table.Columns[columnIndex];
                object rawValue = row[column];
                if (rawValue == null || rawValue == DBNull.Value)
                {
                    continue;
                }

                string text = Convert.ToString(rawValue, CultureInfo.InvariantCulture);
                if (!IsMissingValueToken(text))
                {
                    continue;
                }

                row[column] = IsDashboardPercentColumn(column.ColumnName) ? "0.0% (0/0)" : "-";
            }
        }
    }

    private static void NormalizeGridRowMissingValues(GridViewRow row)
    {
        if (row == null)
        {
            return;
        }

        for (int i = 0; i < row.Cells.Count; i++)
        {
            TableCell cell = row.Cells[i];
            string text = HttpUtility.HtmlDecode(cell.Text ?? string.Empty)
                .Replace("\u00A0", " ")
                .Trim();

            if (IsMissingValueToken(text))
            {
                cell.Text = "-";
            }
        }
    }


    private void BindGridView_component_summary(string filtervalue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string summaryPlatform = ResolveSummaryQuickLinkPlatform();
        bool isOverallSummary = IsOverallDashboardPlatform(summaryPlatform);

        if (isOverallSummary)
        {
            string overallSourceSql = "(" + BuildSummaryIssueComponentSourceSql(summaryPlatform) + ") AS src";
            string overallQuery = @"
WITH base AS (
    SELECT
        CASE WHEN src.component_group IS NULL OR LTRIM(RTRIM(src.component_group)) = '' THEN 'unassigned' ELSE LTRIM(RTRIM(src.component_group)) END AS Component,
        LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) AS status_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.cmf_request, '')))) AS cmf_request_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.sysdebug, '')))) AS sysdebug_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.closed_reason, '')))) AS closed_reason_norm,
        TRY_CAST(src.date_cmf_ask AS DATE) AS ask_date,
        TRY_CAST(src.implemented_date AS DATE) AS implemented_date
    FROM " + overallSourceSql + @"
)
SELECT
    Component,
    SUM(CASE WHEN cmf_request_norm IN ('cmf_ask', 'cmf_incomplete') AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS Issues_in_CMF_ASK,
    SUM(CASE WHEN cmf_request_norm = 'cmf_reject' THEN 1 ELSE 0 END) AS Total_CMF_REJECT,
    ISNULL(AVG(CAST(CASE WHEN ask_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= ask_date THEN DATEDIFF(DAY, ask_date, implemented_date) END AS DECIMAL(10, 2))), 0) AS crit_tpt,
    CAST(0 AS DECIMAL(10, 2)) AS disp_tpt,
    ISNULL(AVG(CAST(CASE WHEN ask_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= ask_date THEN DATEDIFF(DAY, ask_date, implemented_date) END AS DECIMAL(10, 2))), 0) AS resolve_tpt,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS CMFOpenPercentage,
    SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) AS Total_CMF_Approved,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm = 'cmf_ok'
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm NOT LIKE '%internal%'
                         AND closed_reason_norm IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm = 'cmf_ok'
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm NOT LIKE '%internal%'
                     AND closed_reason_norm IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS Noise,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm = 'cmf_ok'
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND (
                             closed_reason_norm LIKE '%internal%'
                             OR closed_reason_norm IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')
                         )
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm = 'cmf_ok'
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND (
                         closed_reason_norm LIKE '%internal%'
                         OR closed_reason_norm IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')
                     )
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS IntelIssuePercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm = 'cmf_ok'
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm = 'cmf_ok'
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS ThirdPartyPercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm = 'cmf_ok'
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm = 'cmf_ok'
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm = 'cmf_ok' AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS CustomerIssuePercentage
FROM base
GROUP BY Component
ORDER BY Total_CMF_Approved DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(overallQuery, con))
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                sda.Fill(dt);
                NormalizeDashboardMissingValues(dt);

                GridView_component_summary.DataSource = dt;
                GridView_component_summary.DataBind();

                GridView_ingredient_indicator_modal.DataSource = dt;
                GridView_ingredient_indicator_modal.DataBind();
            }

            return;
        }

        string selectedPlatform = FilterDashboardTableSource(summaryPlatform) + " AS dated_summary";
        string componentFromClause = FilterDashboardTableSource(summaryPlatform) + " A";



        string main_query = @"SELECT CASE
                                    WHEN A.component_group IS NULL OR A.component_group = '' THEN 'unassigned'
                                    ELSE A.component_group
                                END AS Component,                             
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND component_group = A.component_group AND status not in ('complete','rejected')) AS Issues_in_CMF_ASK,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_reject') AND component_group = A.component_group) AS Total_CMF_REJECT,
                            (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0)
                             FROM " + selectedPlatform + @"
                             WHERE sysdebug LIKE('%customer_must_fix%') AND cmf_request IN('cmf_ok') AND component_group = A.component_group) AS crit_tpt,
                            (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0)
                             FROM " + selectedPlatform + @"
                             WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug LIKE('%customer_must_fix%') AND component_group = A.component_group) AS disp_tpt,
                            (SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND component_group = A.component_group) AS resolve_tpt,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE cmf_request IN ('cmf_ok') AND component_group = A.component_group AND sysdebug LIKE('%customer_must_fix%')) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND component_group = A.component_group)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE cmf_request IN('cmf_ok') AND component_group = A.component_group AND sysdebug LIKE('%customer_must_fix%'))
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND component_group = A.component_group) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE cmf_request IN ('cmf_ok') AND component_group = A.component_group AND sysdebug LIKE('%customer_must_fix%')) AS VARCHAR) +')'
                            END) AS CMFOpenPercentage,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_ok') AND sysdebug Like ('%customer_must_fix%') AND component_group = A.component_group) AS Total_CMF_Approved,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND status IN('complete', 'rejected')
                                        AND component_group = A.component_group) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                       AND status IN('complete', 'rejected')
                                       AND(closed_reason NOT LIKE('%internal%')
                                       AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                       AND component_group = A.component_group)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND status IN('complete', 'rejected')
                                       AND component_group = A.component_group)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status IN('complete', 'rejected')
                                            AND (closed_reason NOT LIKE('%internal%')
                                            AND sysdebug LIKE('%customer_must_fix%')
                                            AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                            AND component_group = A.component_group) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE ('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN ('complete', 'rejected')
                                            AND component_group = A.component_group) AS VARCHAR) +')'
                            END) AS Noise,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND status IN('complete', 'rejected')
                                        AND component_group = A.component_group) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE ('%customer_must_fix%')
                                       AND cmf_request IN ('cmf_ok')
                                       AND status IN ('complete', 'rejected')
                                       AND(closed_reason LIKE ('%internal%')
                                       OR closed_reason IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                       AND component_group = A.component_group)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN ('cmf_ok')
                                       AND status IN('complete', 'rejected')
                                       AND component_group = A.component_group)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE ('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN ('complete', 'rejected')
                                            AND (closed_reason LIKE ('%internal%')
                                            OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                            AND component_group = A.component_group) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN('complete', 'rejected')
                                            AND component_group = A.component_group) AS VARCHAR) +')'
                            END) AS IntelIssuePercentage,
                            (CASE WHEN (SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN('complete', 'rejected')
                                            AND component_group = A.component_group) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN ('cmf_ok')
                                           AND status IN('complete', 'rejected')
                                           AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                           AND component_group = A.component_group)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN ('cmf_ok')
                                           AND status IN('complete', 'rejected')
                                           AND component_group = A.component_group)
                                          * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status IN('complete', 'rejected')
                                                AND cmf_request IN ('cmf_ok')
                                                AND sysdebug LIKE('%customer_must_fix%')
                                                AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                                AND component_group = A.component_group) AS VARCHAR) + '/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN ('cmf_ok')
                                                AND status IN('complete', 'rejected')
                                                AND component_group = A.component_group) AS VARCHAR) + ')'
                                END) AS ThirdPartyPercentage,
                                (CASE WHEN (SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN ('cmf_ok')
                                    AND status IN('complete', 'rejected')
                                    AND component_group = A.component_group) = 0
                                THEN 'N/A'
                                ELSE CAST(ROUND(
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN ('cmf_ok')
                                    AND status IN('complete', 'rejected')
                                    AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                    AND component_group = A.component_group)
                                    /
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN ('cmf_ok')
                                    AND status IN('complete', 'rejected')
                                    AND component_group = A.component_group)
                                    * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE status IN('complete', 'rejected')
                                        AND sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                        AND component_group = A.component_group) AS VARCHAR) + '/' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND status IN('complete', 'rejected')
                                        AND component_group = A.component_group) AS VARCHAR) + ')'
                        END) AS CustomerIssuePercentage
                        FROM
                            " + componentFromClause + @" 
                        WHERE cmf_request Not IN ('cmf_duplicate')
                        GROUP BY
                            A.component_group
                        ORDER BY
                            Total_CMF_Approved desc";
        using (SqlConnection con = new SqlConnection(connectionString))
        {

            using (SqlCommand cmd = new SqlCommand(main_query, con))

            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {

                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    NormalizeDashboardMissingValues(dt);

                    GridView_component_summary.DataSource = dt;
                    GridView_component_summary.DataBind();

                    GridView_ingredient_indicator_modal.DataSource = dt;
                    GridView_ingredient_indicator_modal.DataBind();
                }
            }

        }


    }

    // Bind data to OEM Summary GridView
    private void BindGridView_oem_summary(string filtervalue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string summaryPlatform = ResolveSummaryQuickLinkPlatform();
        bool isOverallSummary = IsOverallDashboardPlatform(summaryPlatform);

        if (isOverallSummary)
        {
            string overallSourceSql = "(" + BuildSummaryIssueComponentSourceSql(summaryPlatform) + ") AS src";
            string overallQuery = @"
WITH base AS (
    SELECT
        CASE WHEN src.customer_company IS NULL OR LTRIM(RTRIM(src.customer_company)) = '' THEN 'unassigned' ELSE LTRIM(RTRIM(src.customer_company)) END AS OEM,
        LOWER(LTRIM(RTRIM(ISNULL(src.status, '')))) AS status_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.cmf_request, '')))) AS cmf_request_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.sysdebug, '')))) AS sysdebug_norm,
        LOWER(LTRIM(RTRIM(ISNULL(src.closed_reason, '')))) AS closed_reason_norm,
        TRY_CAST(src.date_cmf_ask AS DATE) AS ask_date,
        TRY_CAST(src.implemented_date AS DATE) AS implemented_date
    FROM " + overallSourceSql + @"
)
SELECT
    OEM,
    SUM(CASE WHEN cmf_request_norm IN ('cmf_ask', 'cmf_incomplete') AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS Issues_in_CMF_ASK,
    SUM(CASE WHEN cmf_request_norm = 'cmf_reject' THEN 1 ELSE 0 END) AS Total_CMF_REJECT,
    SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) AS Total_CMF_Approved,
    ISNULL(AVG(CAST(CASE WHEN ask_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= ask_date THEN DATEDIFF(DAY, ask_date, implemented_date) END AS DECIMAL(10, 2))), 0) AS crit_tpt,
    CAST(0 AS DECIMAL(10, 2)) AS disp_tpt,
    ISNULL(AVG(CAST(CASE WHEN ask_date IS NOT NULL AND implemented_date IS NOT NULL AND implemented_date >= ask_date THEN DATEDIFF(DAY, ask_date, implemented_date) END AS DECIMAL(10, 2))), 0) AS resolve_tpt,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm NOT IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS CMFOpenPercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm NOT LIKE '%internal%'
                         AND closed_reason_norm IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm NOT LIKE '%internal%'
                     AND closed_reason_norm IN ('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS Noise,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND (
                             closed_reason_norm LIKE '%internal%'
                             OR closed_reason_norm IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')
                         )
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND (
                         closed_reason_norm LIKE '%internal%'
                         OR closed_reason_norm IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')
                     )
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS IntelIssuePercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm IN ('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS ThirdPartyPercentage,
    CASE
        WHEN SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) = 0 THEN 'N/A'
        ELSE
            CAST(ROUND(
                100.0 * SUM(CASE
                    WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                         AND sysdebug_norm LIKE '%customer_must_fix%'
                         AND status_norm IN ('complete', 'rejected')
                         AND closed_reason_norm IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')
                    THEN 1 ELSE 0 END)
                / NULLIF(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END), 0),
                1
            ) AS VARCHAR(20))
            + '% ('
            + CAST(SUM(CASE
                WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate')
                     AND sysdebug_norm LIKE '%customer_must_fix%'
                     AND status_norm IN ('complete', 'rejected')
                     AND closed_reason_norm IN ('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')
                THEN 1 ELSE 0 END) AS VARCHAR(20))
            + '/'
            + CAST(SUM(CASE WHEN cmf_request_norm IN ('cmf_ok', 'cmf_duplicate') AND sysdebug_norm LIKE '%customer_must_fix%' AND status_norm IN ('complete', 'rejected') THEN 1 ELSE 0 END) AS VARCHAR(20))
            + ')'
    END AS CustomerIssuePercentage
FROM base
GROUP BY OEM
ORDER BY Total_CMF_Approved DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(overallQuery, con))
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                sda.Fill(dt);
                NormalizeDashboardMissingValues(dt);

                GridView_oem_summary.DataSource = dt;
                GridView_oem_summary.DataBind();

                GridView_oem_indicator_modal.DataSource = dt;
                GridView_oem_indicator_modal.DataBind();
            }

            return;
        }

        string selectedPlatform = FilterDashboardTableSource(summaryPlatform) + " AS dated_summary";
        string oemFromClause = FilterDashboardTableSource(summaryPlatform) + " A";

        string main_query = @"SELECT
                            CASE 
                                WHEN A.customer_company IS NULL OR A.customer_company = '' THEN 'unassigned'
                                ELSE A.customer_company
                            END AS OEM,  
                             
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND customer_company = A.customer_company AND status not in ('complete','rejected') ) AS Issues_in_CMF_ASK,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_reject') AND customer_company = A.customer_company) AS Total_CMF_REJECT,
                            (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0)
                             FROM " + selectedPlatform + @"
                             WHERE sysdebug LIKE('%customer_must_fix%') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company) AS crit_tpt,
                            (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_company = A.customer_company) AS disp_tpt, 
                            (SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_company = A.customer_company) AS resolve_tpt,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company AND sysdebug LIKE('%customer_must_fix%')) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company AND sysdebug LIKE('%customer_must_fix%'))
                                      * 100.0, 1) AS VARCHAR) + '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_company = A.customer_company) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company AND sysdebug LIKE('%customer_must_fix%')) AS VARCHAR) +')'
                            END) AS CMFOpenPercentage,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND customer_company = A.customer_company) AS Total_CMF_Approved,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_company = A.customer_company) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND(closed_reason NOT LIKE('%internal%')
                                       AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                       AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND customer_company = A.customer_company)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status IN('complete', 'rejected')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND(closed_reason NOT LIKE('%internal%')
                                            AND sysdebug LIKE('%customer_must_fix%')
                                            AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                            AND customer_company = A.customer_company) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_company = A.customer_company) AS VARCHAR) +')'
                            END) AS Noise,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_company = A.customer_company) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND(closed_reason LIKE('%internal%')
                                       OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                       AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND customer_company = A.customer_company)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND(closed_reason LIKE('%internal%')
                                            OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                            AND customer_company = A.customer_company) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_company = A.customer_company) AS VARCHAR) +')'
                            END) AS IntelIssuePercentage,
                            (CASE WHEN (SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_company = A.customer_company) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                       AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND customer_company = A.customer_company)
                                      * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status IN('complete', 'rejected')
                                            AND sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                            AND customer_company = A.customer_company) AS VARCHAR) + '/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_company = A.customer_company) AS VARCHAR) + ')'
                            END) AS ThirdPartyPercentage,
                            (CASE WHEN (SELECT COUNT(cp_id)
                                FROM " + selectedPlatform + @"
                                WHERE sysdebug LIKE('%customer_must_fix%')
                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                AND status IN('complete', 'rejected')
                                AND customer_company = A.customer_company) = 0
                            THEN 'N/A'
                            ELSE CAST(ROUND(
                                (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                FROM " + selectedPlatform + @"
                                WHERE sysdebug LIKE('%customer_must_fix%')
                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                AND status IN('complete', 'rejected')
                                AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                AND customer_company = A.customer_company)
                                /
                                (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                FROM " + selectedPlatform + @"
                                WHERE sysdebug LIKE('%customer_must_fix%')
                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                AND status IN('complete', 'rejected')
                                AND customer_company = A.customer_company)
                                * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                CAST((SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE status IN('complete', 'rejected')
                                    AND sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                    AND customer_company = A.customer_company) AS VARCHAR) + '/' +
                                CAST((SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND customer_company = A.customer_company) AS VARCHAR) + ')'
                    END) AS CustomerIssuePercentage
                        FROM
                            " + oemFromClause + @"
                        GROUP BY
                            A.customer_company
                        ORDER BY
                            Total_CMF_Approved desc";

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    NormalizeDashboardMissingValues(dt);

                    GridView_oem_summary.DataSource = dt;
                    GridView_oem_summary.DataBind();

                    GridView_oem_indicator_modal.DataSource = dt;
                    GridView_oem_indicator_modal.DataBind();
                }
            }
        }
    }

    protected void GridViewoem_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string oemfilter = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column index for "days_active"
            int daysActiveIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "days_active")
                {
                    daysActiveIndex = i;
                    break;
                }
            }

            // Apply color coding to days_active column
            if (daysActiveIndex != -1)
            {
                string daysActiveText = e.Row.Cells[daysActiveIndex].Text;
                int daysActive;

                if (int.TryParse(daysActiveText, out daysActive))
                {
                    if (daysActive > 21)
                    {
                        e.Row.Cells[daysActiveIndex].ForeColor = System.Drawing.Color.Red;
                        e.Row.Cells[daysActiveIndex].Font.Bold = true;
                    }
                    else if (daysActive > 14)
                    {
                        e.Row.Cells[daysActiveIndex].ForeColor = System.Drawing.Color.Orange;
                    }
                    else
                    {
                        e.Row.Cells[daysActiveIndex].ForeColor = System.Drawing.Color.Green;
                    }
                }
            }

            // If you want to add clickable links similar to the design version, uncomment below:
            /*
            // Find the column index for "Total CMF_Approved" (if you want clickable links)
            int colIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "Total CMF_Approved")
                {
                    colIndex = i;
                    break;
                }
            }

            if (colIndex != -1)
            {
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "customer_company");

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                oemfilter = oem;

                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndex].Text = link;
            }
            */
        }
    }
    protected void GridView6_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string trigger = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column indices for both red and yellow highlighted columns
            int colIndexApproved = -1;
            int colIndexASK = -1;
            int colIndexReject = -1;
            int colIndexCMFOpen = -1;
            int colIndexNoise = -1;
            int colIndexIntelIssue = -1;
            int colIndex3rdParty = -1;
            int colIndexCustomerIssue = -1;
            int colIndexDispTPT = -1;
            int colIndexResolveTPT = -1;
            int colIndexTotalTPT = -1;

            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                string headerText = grid.HeaderRow.Cells[i].Text.Trim();

                if (headerText == "Total CMF_Approved")
                {
                    colIndexApproved = i;
                }
                else if (headerText == "Issues in CMF_ASK")
                {
                    colIndexASK = i;
                }
                else if (headerText == "Total CMF_REJECT")
                {
                    colIndexReject = i;
                }
                else if (headerText == "CMF Open %")
                {
                    colIndexCMFOpen = i;
                }
                else if (headerText == "Noise%")
                {
                    colIndexNoise = i;
                }
                else if (headerText == "Intel Issue %")
                {
                    colIndexIntelIssue = i;
                }
                else if (headerText == "3rd Party %")
                {
                    colIndex3rdParty = i;
                }
                else if (headerText == "Customer Issue %")
                {
                    colIndexCustomerIssue = i;
                }
            }

            colIndexDispTPT = FindColumnIndexByHeaderContains(grid, "disposition tpt");
            colIndexResolveTPT = FindColumnIndexByHeaderContains(grid, "resolution tpt");
            colIndexTotalTPT = FindColumnIndexByHeaderContains(grid, "overall tpt");

            bool isOverallSummary = IsOverallDashboardPlatform(ResolveSummaryQuickLinkPlatform());

            // Modify the columns for red highlighted sections
            if (colIndexApproved != -1)
            {
                trigger = "_trg1";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + val + "</a>";

                e.Row.Cells[colIndexApproved].Text = link;
            }

            if (colIndexASK != -1)
            {
                trigger = "_trg2";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Issues_in_CMF_ASK");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + val + "</a>";

                e.Row.Cells[colIndexASK].Text = link;
            }

            if (colIndexReject != -1)
            {
                trigger = "_trg3";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_REJECT");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + val + "</a>";

                e.Row.Cells[colIndexReject].Text = link;
            }

            // Modify the columns for yellow highlighted sections
            if (colIndexCMFOpen != -1)
            {
                trigger = "_trg4";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CMFOpenPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCMFOpen].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexNoise != -1)
            {
                trigger = "_trg5";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Noise");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexNoise].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexIntelIssue != -1)
            {
                trigger = "_trg6";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "IntelIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexIntelIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndex3rdParty != -1)
            {
                trigger = "_trg7";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "ThirdPartyPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndex3rdParty].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexCustomerIssue != -1)
            {
                trigger = "_trg8";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CustomerIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCustomerIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            ApplyTptCellStyle(e.Row, colIndexDispTPT, 2m, isOverallSummary);
            ApplyTptCellStyle(e.Row, colIndexResolveTPT, 18m, isOverallSummary);
            ApplyTptCellStyle(e.Row, colIndexTotalTPT, 21m, isOverallSummary);
            NormalizeGridRowMissingValues(e.Row);
        }


    }

    protected void GridView7_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string trigger = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column indices for both red and yellow highlighted columns
            int colIndexApproved = -1;
            int colIndexASK = -1;
            int colIndexReject = -1;
            int colIndexCMFOpen = -1;
            int colIndexNoise = -1;
            int colIndexIntelIssue = -1;
            int colIndex3rdParty = -1;
            int colIndexCustomerIssue = -1;
            int colIndexDispTPT = -1;
            int colIndexResolveTPT = -1;
            int colIndexTotalTPT = -1;

            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                string headerText = grid.HeaderRow.Cells[i].Text.Trim();

                if (headerText == "Total CMF_Approved")
                {
                    colIndexApproved = i;
                }
                else if (headerText == "Issues in CMF_ASK")
                {
                    colIndexASK = i;
                }
                else if (headerText == "Total CMF_REJECT")
                {
                    colIndexReject = i;
                }
                else if (headerText == "CMF Open %")
                {
                    colIndexCMFOpen = i;
                }
                else if (headerText == "Noise%")
                {
                    colIndexNoise = i;
                }
                else if (headerText == "Intel Issue %")
                {
                    colIndexIntelIssue = i;
                }
                else if (headerText == "3rd Party %")
                {
                    colIndex3rdParty = i;
                }
                else if (headerText == "Customer Issue %")
                {
                    colIndexCustomerIssue = i;
                }
            }

            colIndexDispTPT = FindColumnIndexByHeaderContains(grid, "disposition tpt");
            colIndexResolveTPT = FindColumnIndexByHeaderContains(grid, "resolution tpt");
            colIndexTotalTPT = FindColumnIndexByHeaderContains(grid, "overall tpt");

            bool isOverallSummary = IsOverallDashboardPlatform(ResolveSummaryQuickLinkPlatform());

            // Modify the columns for red highlighted sections
            if (colIndexApproved != -1)
            {
                trigger = "_trg1";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndexApproved].Text = link;
            }

            if (colIndexASK != -1)
            {
                trigger = "_trg2";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Issues_in_CMF_ASK");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndexASK].Text = link;
            }

            if (colIndexReject != -1)
            {
                trigger = "_trg3";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_REJECT");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndexReject].Text = link;
            }

            // Modify the columns for yellow highlighted sections
            if (colIndexCMFOpen != -1)
            {
                trigger = "_trg4";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CMFOpenPercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCMFOpen].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexNoise != -1)
            {
                trigger = "_trg5";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Noise");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexNoise].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexIntelIssue != -1)
            {
                trigger = "_trg6";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "IntelIssuePercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexIntelIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndex3rdParty != -1)
            {
                trigger = "_trg7";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "ThirdPartyPercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndex3rdParty].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexCustomerIssue != -1)
            {
                trigger = "_trg8";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CustomerIssuePercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                val = FormatPercentRatioOneDecimal(val);
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCustomerIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            ApplyTptCellStyle(e.Row, colIndexDispTPT, 2m, isOverallSummary);
            ApplyTptCellStyle(e.Row, colIndexResolveTPT, 18m, isOverallSummary);
            ApplyTptCellStyle(e.Row, colIndexTotalTPT, 21m, isOverallSummary);
            NormalizeGridRowMissingValues(e.Row);
        }
    }

    protected void overall_request_details_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        // Cancel the edit mode
        overall_request_details.EditIndex = -1;
        selectedValue = Session["selectedValue"] as string ?? null;

        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);


        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void overall_request_details_RowEditing(object sender, GridViewEditEventArgs e)
    {
        // Set the row to be edited
        overall_request_details.EditIndex = e.NewEditIndex;
        selectedValue = Session["selectedValue"] as string ?? null;

        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void overall_request_details_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Get the row being updated
        GridViewRow row = overall_request_details.Rows[e.RowIndex];

        // Retrieve the new values with explicit null checks


        string los = null;
        DropDownList ddllos = row.FindControl("ddllos") as DropDownList;
        if (ddllos != null)
        {
            los = ddllos.SelectedValue;
        }


        string impact = null;
        TextBox txtimpact = row.FindControl("txtimpact") as TextBox;
        if (txtimpact != null)
        {
            impact = txtimpact.Text;
        }

        string ownerTextbox = null;
        TextBox txtOwner = row.FindControl("txtOwner") as TextBox;
        if (txtOwner != null)
        {
            ownerTextbox = txtOwner.Text;
        }



        string newidst = null;
        TextBox txtidst = row.FindControl("txtidst") as TextBox;
        if (txtidst != null)
        {
            newidst = txtidst.Text;
        }

        // Get the SightingID (unique key) for the row
        string sightingId = overall_request_details.DataKeys[e.RowIndex].Value.ToString();

        // Fetch the current value of the Status field from the database

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;


        // Update the database
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            //string query = "UPDATE " + selectedPlatform + " SET customer_owner = @Owner, idst=@idst, los=@los, impact=@impact, progress = CASE WHEN @los = 'Yes' THEN 'orange' ELSE progress END WHERE cp_id = @SightingID";
            string query = @"UPDATE " + selectedPlatform + @" 
                SET customer_owner = @Owner, 
                    idst = @idst, 
                    los = @los, 
                    impact = @impact,
                    original_progress = CASE 
                        WHEN @los = 'Yes' AND (original_progress IS NULL OR original_progress = '') 
                        THEN progress 
                        ELSE original_progress 
                    END,
                    progress = CASE 
                        WHEN @los = 'Yes' THEN 'orange'
                        WHEN @los = 'No' AND (original_progress IS NOT NULL AND original_progress != '') 
                        THEN original_progress
                        ELSE progress 
                    END
                WHERE cp_id = @SightingID";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Owner", ownerTextbox);

                cmd.Parameters.AddWithValue("@SightingID", sightingId);
                cmd.Parameters.AddWithValue("@idst", newidst);
                cmd.Parameters.AddWithValue("@los", los);
                cmd.Parameters.AddWithValue("@impact", impact);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        // Reset the edit index and rebind the data
        overall_request_details.EditIndex = -1;
        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void GridView_notes_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Check if the row is a data row (not header, footer, etc.)
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Get the values from the current row
            int dispTPT = 0;
            int resolveTPT = 0;
            int totalTPT = 0;

            // Try parsing the values to integers
            int.TryParse(e.Row.Cells[0].Text, out dispTPT);
            int.TryParse(e.Row.Cells[1].Text, out resolveTPT);
            int.TryParse(e.Row.Cells[2].Text, out totalTPT);

            // Add suffix to CMF_Disposition_TPT cell
            if (e.Row.Cells[0].Text != "")
            {
                e.Row.Cells[0].Text += " Days";
            }

            // Add suffix to CMF_Resolution_TPT cell
            if (e.Row.Cells[1].Text != "")
            {
                e.Row.Cells[1].Text += " Days";
            }

            // Add suffix to critical_tpt cell
            if (e.Row.Cells[2].Text != "")
            {
                e.Row.Cells[2].Text += " Days";
            }

            // Apply red color if conditions are met
            if (dispTPT > 2)
            {
                e.Row.Cells[0].ForeColor = System.Drawing.Color.Red; // Assuming disp_tpt is the first column
            }
            else
            {
                e.Row.Cells[0].ForeColor = System.Drawing.Color.Green; // Assuming disp_tpt is the first column
            }

            if (resolveTPT > 18)
            {
                e.Row.Cells[1].ForeColor = System.Drawing.Color.Red; // Assuming resolve_tpt is the second column
            }
            else
            {
                e.Row.Cells[1].ForeColor = System.Drawing.Color.Green; // Assuming resolve_tpt is the second column
            }

            if (totalTPT > 21)
            {
                e.Row.Cells[2].ForeColor = System.Drawing.Color.Red; // Assuming resolve_tpt is the second column
            }
            else
            {
                e.Row.Cells[2].ForeColor = System.Drawing.Color.Green; // Assuming resolve_tpt is the second column
            }
        }
    }


    protected void overall_request_details_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            AddIssueColumnHideButtons(e.Row);
            return;
        }

        if (e.Row.RowType == DataControlRowType.DataRow && e.Row.RowIndex != overall_request_details.EditIndex)
        {
            FillEmptyIssueListCells(e.Row);
        }

        if (e.Row.RowType == DataControlRowType.DataRow && e.Row.RowIndex == overall_request_details.EditIndex)
        {


            // Populate the Color DropDownList
            var ddllos = e.Row.FindControl("ddllos") as DropDownList;
            if (ddllos != null)
            {
                ddllos.Items.Add(new ListItem("No", "No"));
                ddllos.Items.Add(new ListItem("Yes", "Yes"));

                ddllos.SelectedValue = DataBinder.Eval(e.Row.DataItem, "los").ToString();
            }




        }
        else
        {



            string losValue = null;
            if (DataBinder.Eval(e.Row.DataItem, "los") != null)
            {
                losValue = DataBinder.Eval(e.Row.DataItem, "los").ToString();
            }

            // Check if the row is a data row (not a header or footer row)
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Resolve progress cell by CSS class so color rendering remains stable
                // even if column order changes.
                TableCell progressCell = null;
                for (int cellIndex = 0; cellIndex < e.Row.Cells.Count; cellIndex++)
                {
                    TableCell candidate = e.Row.Cells[cellIndex];
                    if (GetIssueFieldClass(overall_request_details.Columns[cellIndex].ItemStyle.CssClass) == "field-progress")
                    {
                        progressCell = candidate;
                        break;
                    }
                }

                if (progressCell == null)
                {
                    return;
                }

                // Use the bound field value directly; this is more reliable than cell text.
                string cellValue = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "progress"));
                if (!string.IsNullOrWhiteSpace(cellValue))
                {
                    cellValue = cellValue.Trim().ToLowerInvariant();
                }

                // Check if the cell has a valid color value
                if (!string.IsNullOrEmpty(cellValue))
                {
                    try
                    {
                        // Set the background color and text color based on the cell value
                        System.Drawing.Color background = System.Drawing.ColorTranslator.FromHtml(cellValue);
                        progressCell.BackColor = background;
                        progressCell.ForeColor = GetIssueProgressTextColor(background);
                        progressCell.Attributes["style"] = "background-color:" + System.Drawing.ColorTranslator.ToHtml(background)
                            + " !important;color:" + System.Drawing.ColorTranslator.ToHtml(progressCell.ForeColor) + " !important;";
                        progressCell.ToolTip = cellValue;
                    }
                    catch (ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("Invalid Issue List progress color: {0}", cellValue);
                        progressCell.ToolTip = "Progress color is unavailable.";
                    }
                }
            }

        }
    }

    private static System.Drawing.Color GetIssueProgressTextColor(System.Drawing.Color background)
    {
        double[] channels = { background.R / 255.0, background.G / 255.0, background.B / 255.0 };
        for (int i = 0; i < channels.Length; i++)
            channels[i] = channels[i] <= 0.04045 ? channels[i] / 12.92 : Math.Pow((channels[i] + 0.055) / 1.055, 2.4);
        double luminance = 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2];
        return luminance > 0.179 ? System.Drawing.Color.Black : System.Drawing.Color.White;
    }

    private static void FillEmptyIssueListCells(GridViewRow row)
    {
        foreach (TableCell cell in row.Cells)
        {
            if (!cell.Visible || cell.CssClass.Split(' ').Contains("field-actions")) continue;

            using (StringWriter buffer = new StringWriter())
            using (HtmlTextWriter writer = new HtmlTextWriter(buffer))
            {
                cell.RenderControl(writer);
                string text = HttpUtility.HtmlDecode(
                    System.Text.RegularExpressions.Regex.Replace(buffer.ToString(), "<[^>]*>", string.Empty));
                if (string.IsNullOrWhiteSpace(text))
                {
                    cell.Controls.Clear();
                    cell.Text = "unknown";
                }
            }
        }
    }

    private static void AddIssueColumnHideButtons(GridViewRow headerRow)
    {
        if (headerRow == null)
        {
            return;
        }

        foreach (TableCell cell in headerRow.Cells)
        {
            string fieldClass = GetIssueFieldClass(cell.CssClass);
            if (string.IsNullOrWhiteSpace(fieldClass) || ContainsColumnHideButton(cell))
            {
                continue;
            }

            LiteralControl button = new LiteralControl("<button type=\"button\" class=\"col-hide-btn\" title=\"Hide column\" data-field=\"" + HttpUtility.HtmlAttributeEncode(fieldClass) + "\" onclick=\"hideColumnByClass('" + HttpUtility.JavaScriptStringEncode(fieldClass) + "'); return false;\">&#x2715;</button>");
            System.Web.UI.Control target = FindControlByCssClass(cell, "filter-header-text") ?? cell;
            target.Controls.Add(button);
        }
    }

    private static string GetIssueFieldClass(string cssClass)
    {
        if (string.IsNullOrWhiteSpace(cssClass))
        {
            return string.Empty;
        }

        string[] classes = cssClass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string className in classes)
        {
            if (className.StartsWith("field-", StringComparison.OrdinalIgnoreCase))
            {
                return className;
            }
        }

        return string.Empty;
    }

    private static bool ContainsColumnHideButton(System.Web.UI.Control root)
    {
        if (root == null)
        {
            return false;
        }

        WebControl webControl = root as WebControl;
        if (webControl != null && !string.IsNullOrWhiteSpace(webControl.CssClass) && webControl.CssClass.IndexOf("col-hide-btn", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        foreach (System.Web.UI.Control child in root.Controls)
        {
            if (ContainsColumnHideButton(child))
            {
                return true;
            }
        }

        return false;
    }

    private static System.Web.UI.Control FindControlByCssClass(System.Web.UI.Control root, string cssClass)
    {
        if (root == null || string.IsNullOrWhiteSpace(cssClass))
        {
            return null;
        }

        WebControl webControl = root as WebControl;
        if (webControl != null && !string.IsNullOrWhiteSpace(webControl.CssClass))
        {
            string[] classes = webControl.CssClass.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (classes.Any(className => string.Equals(className, cssClass, StringComparison.OrdinalIgnoreCase)))
            {
                return root;
            }
        }

        foreach (System.Web.UI.Control child in root.Controls)
        {
            System.Web.UI.Control match = FindControlByCssClass(child, cssClass);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    protected void GridView_design_open_RowEditing(object sender, GridViewEditEventArgs e)
    {
        // Set the EditIndex for the edit GridView and make it visible
        GridView2_edit.EditIndex = e.NewEditIndex;
        GridView2_edit.Visible = true;
        GridView_design_open.Visible = false;
        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void GridView_design_open_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Assuming selectedPlatform is a class-level variable or accessible in this method
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";


        // Get the row being updated
        GridViewRow row = GridView2_edit.Rows[e.RowIndex];

        // Retrieve the new value for SWImageFreeze from the TextBox
        TextBox txtSWImageFreeze = row.FindControl("txtSWImageFreeze") as TextBox;
        string newSWImageFreeze = txtSWImageFreeze != null ? txtSWImageFreeze.Text : null;

        // Retrieve the new value for SWImageFreeze from the TextBox
        TextBox txtSupportModel = row.FindControl("txtSupportModel") as TextBox;
        string newSupportModel = txtSupportModel != null ? txtSupportModel.Text : null;

        // Get the design key for the row being updated
        string design = GridView2_edit.DataKeys[e.RowIndex].Value.ToString();

        // Update the database with the new value
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "UPDATE " + designTable + " SET sw_image_freeze = @SWImageFreeze, support_model=@SupportModel WHERE customer_detail = @Design";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@SWImageFreeze", newSWImageFreeze);
                cmd.Parameters.AddWithValue("@SupportModel", newSupportModel);
                cmd.Parameters.AddWithValue("@Design", design);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Reset the EditIndex and rebind both GridViews
        GridView2_edit.EditIndex = -1;
        GridView2_edit.Visible = false;

        GridView_design_open.EditIndex = -1;
        GridView_design_open.Visible = true;


        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
        string script = "window.onload = function () { adjustGridHeights(); };";  // Call your custom JS function
        ClientScript.RegisterStartupScript(this.GetType(), "TriggerJavaScriptFunction", script, true);

    }



    protected void GridView_design_open_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        // Reset the EditIndex and hide the edit GridView
        GridView2_edit.EditIndex = -1;
        GridView2_edit.Visible = false;
        GridView_design_open.EditIndex = -1;
        GridView_design_open.Visible = true;
        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);

    }

    protected void GridView_design_open_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            foreach (var driver in drivers)
            {
                string colName = driverColumns[driver] + "_Issues";
                int colIndex = -1;

                for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
                {
                    if (grid.HeaderRow.Cells[i].Text.Trim() == driver)
                    {
                        colIndex = i;
                        break;
                    }
                }

                if (colIndex != -1)
                {
                    try
                    {
                        object rawVal = DataBinder.Eval(e.Row.DataItem, colName);
                        object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                        string val = rawVal != null ? rawVal.ToString() : "";
                        string design = desVal != null ? desVal.ToString() : "";

                        // Determine the link color based on the design value
                        string linkColor = design == "Total" ? "black" : "blue";

                        string link = "<a href='javascript:void(0);' style='color:" + linkColor + ";' onclick=\"showDriverDetailsModal('" + design + "','" + driver + "')\">" + val + "</a>";

                        e.Row.Cells[colIndex].Text = link;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error accessing column " + colName + ": " + ex.Message);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Column index not found for driver: " + driver);
                }
            }

            // Make Implemented/Verified clickable
            int implementedVerifiedIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "Impl/Verified")
                {
                    implementedVerifiedIndex = i;
                    break;
                }
            }

            if (implementedVerifiedIndex != -1)
            {
                try
                {
                    object rawVal = DataBinder.Eval(e.Row.DataItem, "Implemented_Verified");
                    object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                    string val = rawVal != null ? rawVal.ToString() : "";
                    string design = desVal != null ? desVal.ToString() : "";

                    // Determine the link color based on the design value
                    string linkColor = design == "Total" ? "black" : "blue";

                    string link = "<a href='javascript:void(0);' style='color:" + linkColor + ";' onclick=\"showImplementedVerifiedDetailsModal('" + design + "')\">" + val + "</a>";

                    e.Row.Cells[implementedVerifiedIndex].Text = link;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error accessing Implemented_Verified: " + ex.Message);
                }
            }

            // Hide the edit button for the "Total" row
            object designValue = DataBinder.Eval(e.Row.DataItem, "Design");
            if (designValue != null && designValue.ToString() == "Total")
            {
                foreach (System.Web.UI.Control control in e.Row.Cells[e.Row.Cells.Count - 1].Controls)
                {
                    System.Diagnostics.Debug.WriteLine("Control Type: " + control.GetType().ToString());

                    // Check if the control is a button type
                    if (control is Button || control is LinkButton || control is ImageButton)
                    {
                        System.Diagnostics.Debug.WriteLine("Hiding control: " + control.GetType().ToString());
                        control.Visible = false;
                    }
                }
            }
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }

    protected void lnkValue_Command(object sender, System.Web.UI.WebControls.CommandEventArgs e)
    {

        string collectedDrivers = driverCollectorhf.Value;
        // Get the selected distinct value from the CommandArgument
        selectedValue = e.CommandArgument.ToString();

        if (selectedValue != "")
        {
            if (selectedValue == "AllDrivers")
            {
                collectedDrivers = "AllDrivers";
            }

        }


        if (collectedDrivers == "AllDrivers")
        {
            Session["selectedPlatform"] = selectedPlatform;
            Session["selectedValue"] = selectedValue;


            string ddrivers;
            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";

            string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder driversList = new StringBuilder();

                        while (reader.Read())
                        {
                            // Assuming the column name is 'drivers' and contains the driver's name/ID
                            string driver = reader["drivers"].ToString();

                            if (driversList.Length > 0)
                            {
                                driversList.Append(","); // Append comma if not the first value
                            }

                            driversList.Append(driver);
                        }

                        // Now driversList contains the comma-separated string
                        ddrivers = driversList.ToString();

                        // Optionally, you can store this string in a session or variable

                    }
                }
            }

            selectedValue = ddrivers;


            headerTitle.InnerText = headerTemplate += "All Milestones";
            // Rebind the table with the filtered data
            BindGridView();
            BindGridView_design_open(selectedValue);
            BindGridView_cmf_summary(selectedValue);
            BindGridView_design_summary(selectedValue);
            BindGridView_component_summary(selectedValue);
            BindGridView_oem_summary(selectedValue);
            //GetGridViewData(selectedValue);
        }
        else
        {
            // Collect all the selected checkbox values from the Repeater

            // Step 1: Split the collectedDrivers string into a list of drivers
            var drivers = collectedDrivers.Split(',').ToList();

            // Step 2: Identify duplicates by grouping and filtering those with count > 1
            var duplicateDrivers = drivers.GroupBy(d => d)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

            // Step 3: If no duplicates, stop here
            if (duplicateDrivers.Count == 0)
            {

            }
            else
            {
                // Step 4: Keep only drivers with duplicates
                var driversWithDuplicatesOnly = drivers.Where(d => duplicateDrivers.Contains(d)).ToList();

                // Step 5: Remove duplicates and keep only one copy of each element
                var uniqueDrivers = driversWithDuplicatesOnly.Distinct().ToList();

                // Update collectedDrivers with the result as a comma-separated string
                collectedDrivers = string.Join(",", uniqueDrivers);

            }



            selectedValue = collectedDrivers;
            Session["selectedValue"] = collectedDrivers;
            headerTitle.InnerText = headerTemplate += selectedValue;
            // Rebind the table with the filtered data
            BindGridView(selectedValue);
            BindGridView_design_open(selectedValue);
            BindGridView_cmf_summary(selectedValue);
            BindGridView_design_summary(selectedValue);
            BindGridView_component_summary(selectedValue);
            BindGridView_oem_summary(selectedValue);

        }


    }

    protected void chkAllDrivers_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox chkAllDrivers = (CheckBox)sender;
        // Handle the event when the "All Drivers" checkbox is checked or unchecked
        // You can access the checked state using chkAllDrivers.Checked
    }

    protected void chkValue_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox chkValue = (CheckBox)sender;
        // Handle the event when a distinct value checkbox is checked or unchecked
        // You can access the checked state using chkValue.Checked
        // You can also access the text (which is the distinct value) using chkValue.Text
    }


    protected void btnExportInModal_Click(object sender, EventArgs e)
    {
        ExportGridViewToPPT(sender, e);
    }





    protected void ExportGridViewToPPT(object sender, EventArgs e)
    {
        //Get the selected values from the hidden field
        string selectedValuesString = hfSelectedValues.Value;
        if (string.IsNullOrEmpty(selectedValuesString))
        {
            // No values selected, handle accordingly
            return;
        }

        // Split the selected values into an array
        string[] selectedValues = selectedValuesString.Split(',');

        // Define the export path in the App_Data folder
        string exportPath = Server.MapPath("~/App_Data/ExportedGridView.pptx");

        using (PresentationDocument presentation = PresentationDocument.Create(exportPath, PresentationDocumentType.Presentation))
        {
            PresentationPart presentationPart = presentation.AddPresentationPart();
            presentationPart.Presentation = new DocumentFormat.OpenXml.Presentation.Presentation();

            SlideIdList slideIdList = new SlideIdList();
            uint slideId = 256;

            // Fetch data tables
            selectedValue = Session["selectedValue"] as string ?? null;
            System.Data.DataTable dt = GetGridViewData_maindriver(selectedValue);
            System.Data.DataTable dt2 = GetGridViewDataFromControl(GridView_design_open, true);
            System.Data.DataTable dt5 = GetGridViewDataFromControl_TotalTable();
            System.Data.DataTable dt6 = GetGridViewDataFromControl(GridView_notes, false);
            System.Data.DataTable dt3 = GetGridViewDataFromControl(GridView_cmf_summary, false);
            System.Data.DataTable dt4 = GetGridViewDataFromControl(GridView_cmf_pending, false);
            int rowsPerSlide = 10;

            // Add a slide for dt5 and dt3 together
            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
            slidePart.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

            // Add both tables to the same slide
            AddThreeTablesToSlide(slidePart, dt5, dt3, dt6, 0, dt5.Rows.Count, 0, dt3.Rows.Count, 0, dt6.Rows.Count);

            // Add the slide to the slide list
            slideIdList.Append(new SlideId
            {
                Id = slideId,
                RelationshipId = presentationPart.GetIdOfPart(slidePart)
            });

            slideId++;

            // Continue with other slides
            slideId = AddSlidesForGridView2_3(presentationPart, slideIdList, dt2, rowsPerSlide = 20, slideId);

            for (int i = 0; i < dt.Rows.Count; i += rowsPerSlide)
            {
                // Create a new slide for the first GridView data table (dt)
                SlidePart slidePartMain = presentationPart.AddNewPart<SlidePart>();
                slidePartMain.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

                // Add a table to the slide
                AddTableToSlide(slidePartMain, dt, i, rowsPerSlide);

                // Add the slide to the slide list
                slideIdList.Append(new SlideId
                {
                    Id = slideId,
                    RelationshipId = presentationPart.GetIdOfPart(slidePartMain)
                });

                slideId++;
            }

            slideId = AddSlidesForGridView2_3(presentationPart, slideIdList, dt4, rowsPerSlide = 20, slideId);

            foreach (string selectedValue1 in selectedValues)
            {
                System.Data.DataTable dt1 = GetGridViewData(selectedValue1);
                for (int i = 0; i < dt1.Rows.Count; i += rowsPerSlide)
                {
                    // Create a new slide for the first GridView data table (dt1)
                    SlidePart slidePartForDt1 = presentationPart.AddNewPart<SlidePart>();
                    slidePartForDt1.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

                    // Add a table to the slide
                    AddTableToSlide(slidePartForDt1, dt1, i, rowsPerSlide);

                    // Add the slide to the slide list
                    slideIdList.Append(new SlideId
                    {
                        Id = slideId,
                        RelationshipId = presentationPart.GetIdOfPart(slidePartForDt1)
                    });

                    slideId++;
                }
            }

            // Append slide ID list to the presentation and save it
            presentationPart.Presentation.Append(slideIdList);
            presentationPart.Presentation.Save();
        }

        // Provide the file as a download to the user
        Response.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        Response.AddHeader("Content-Disposition", "attachment; filename=ExportedGridView.pptx");

        Response.TransmitFile(exportPath);
        Response.End();
    }

    private uint AddSlidesForGridView2_3(PresentationPart presentationPart, SlideIdList slideIdList, System.Data.DataTable dt, int rowsPerSlide, uint slideId)
    {
        for (int i = 0; i < dt.Rows.Count; i += rowsPerSlide)
        {
            // Create a new slide
            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
            slidePart.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

            // Add a table to the slide
            AddTableToSlide_grid2_3(slidePart, dt, i, rowsPerSlide);

            // Add the slide to the slide list
            slideIdList.Append(new SlideId
            {
                Id = slideId,
                RelationshipId = presentationPart.GetIdOfPart(slidePart)
            });

            slideId++;
        }
        return slideId;
    }

    protected System.Data.DataTable GetGridViewDataFromControl(GridView gridView, bool excludeLastColumn = false, string filterValue = null)
    {

        System.Data.DataTable dt = new System.Data.DataTable();

        // Add columns to DataTable
        int columnCount = gridView.Columns.Count;
        if (excludeLastColumn)
        {
            columnCount -= 1; // Exclude the last column
        }

        for (int i = 0; i < columnCount; i++)
        {
            dt.Columns.Add(gridView.Columns[i].HeaderText);
        }

        // Add rows to DataTable
        foreach (GridViewRow row in gridView.Rows)
        {
            DataRow dr = dt.NewRow();
            for (int i = 0; i < columnCount; i++)
            {
                // Decode HTML-encoded content
                dr[i] = HttpUtility.HtmlDecode(row.Cells[i].Text);
            }

            // Ensure the first column is not empty
            if (string.IsNullOrWhiteSpace(dr[0].ToString()))
            {
                dr[0] = HttpUtility.HtmlDecode(row.Cells[0].Text);
            }

            dt.Rows.Add(dr);
        }

        return dt;
    }




    protected System.Data.DataTable GetGridViewData_maindriver(string filterValue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string master_query = "SELECT progress As color, cp_id As SightingID, title , component_group , customer_owner As Owner, repro_on_rvp, cmf_status As Status, idst, los, drivers AS Driver FROM " + selectedPlatform;
            //!string.IsNullOrEmpty(filtervalue) && filtervalue!="AllDrivers"
            //!string.IsNullOrEmpty(filtervalue) && filtervalue!="AllDrivers" && filtervalue!="AllDrivers"
            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {


                if (filterValue.Contains(","))
                {
                    Session["filterValue"] = filterValue;
                    // Add a filter based on the selected distinct value
                    //master_query += " WHERE must_fix_for = @FilterValue";
                    master_query += " WHERE \r\n" +
                    "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR ( " +
                    "       @FilterValue LIKE '%,' + drivers + ',%' " +
                    "       OR @FilterValue LIKE drivers + ',%' " +
                    "       OR @FilterValue LIKE '%,' + drivers) ) " +
                    "AND status NOT IN ('complete', 'rejected') order by drivers";


                }
                else
                {
                    Session["filterValue"] = filterValue;
                    // Add a filter based on the selected distinct value
                    //master_query += " WHERE must_fix_for = @FilterValue";
                    master_query += " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
        "    OR drivers = @FilterValue )AND status not in ('complete', 'rejected') order by drivers";
                }


            }
            else
            {
                Session["filterValue"] = WorkWeek + " | CMF Live Dashboard - All";
                headerTitle.InnerText = WorkWeek + " | CMF Live Dashboard - All";
                master_query += " WHERE status not in ('complete', 'rejected') Order by drivers";
            }
            using (SqlCommand cmd = new SqlCommand(master_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    // Add the parameter if filtering
                    if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
                    {
                        if (filterValue.Contains(","))
                        {
                            sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

                        }
                        else
                        {
                            sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

                        }
                    }
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    return dt;
                }
            }
        }
    }




    protected System.Data.DataTable GetGridViewData(string filterValue = null)
    {

        // Reuse the existing BindGridView method logic to fetch the data
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string main_query = "SELECT progress, cp_id As SightingID, title , component_group , customer_owner As Owner, repro_on_rvp, cmf_status As Status, idst As iDST, los AS LOS, drivers AS Diver FROM " + selectedPlatform;

            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                Session["filterValue"] = filterValue;
                // Add a filter based on the selected distinct value
                main_query += "  WHERE drivers = @FilterValue ";
            }

            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    // Add the parameter if filtering
                    if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
                    {
                        sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);
                    }
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    return dt;
                }
            }
        }
    }

    protected System.Data.DataTable GetGridViewDataFromControl_TotalTable(string filtervalue = null)
    {

        string main_query = "";
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";
        //string driver_filter = "";
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            List<string> drivers = new List<string>();
            Dictionary<string, string> driverColumns = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
            {
                Session["filterValue"] = filtervalue;
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
                string Driver_name = "";
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_"); // Ensure column name is safe
                    driverColumns[driver] = safeColumnName;
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    Driver_name = safeColumnName + "_Issues";
                }



                main_query = "SELECT B.component_group AS Component " +
                             driverCaseStatements +
                             "FROM " + selectedPlatform + " A INNER JOIN " + componentTable + " B ON A.component_group = B.component_group " +
                             "Where status not in ('complete','rejected') GROUP BY B.component_group ";

            }
            else
            {


                main_query = "SELECT B.component_group AS Component, COUNT(A.cp_id) AS Driver_issues FROM " + selectedPlatform + " A INNER JOIN " + componentTable + " B ON A.component_group = B.component_group " +
                             "Where status not in ('complete','rejected') GROUP BY B.component_group order by Driver_issues desc";


            }


            // Summary Query to fetch metrics
            string summaryQuery = @"
            SELECT 
                SUM(CASE WHEN cmf_request = 'cmf_ok' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS TotalCount,
                SUM(CASE WHEN [cmf_request] = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS Duplicates,
                SUM(CASE WHEN cmf_request = 'cmf_ok' AND sysdebug Like ('%customer_must_fix%') AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedCount,
                SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' THEN 1 ELSE 0 END) AS ImplementedCount,
                STUFF((
                    SELECT ', ' + [component_group]
                    FROM " + selectedPlatform + @"
                    WHERE cmf_request = 'cmf_ok' AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' 
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ImplementedComponents
            FROM " + selectedPlatform;


            conn.Open();

            // Fetch summary data
            System.Data.DataTable summaryTable = new System.Data.DataTable();
            using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                {
                    da.SelectCommand.Parameters.AddWithValue("@FilterValue", filtervalue);
                }
                da.Fill(summaryTable);
                //Add custom columns to the summary table
                summaryTable.Columns.Add("Total", typeof(string));
                summaryTable.Columns.Add("Closed", typeof(string));
                summaryTable.Columns.Add("ImplementedDetails", typeof(string));

                if (summaryTable.Rows.Count > 0)
                {
                    DataRow row = summaryTable.Rows[0];
                    // Safely access columns and handle DBNull values
                    int totalCount = row["TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["TotalCount"]);
                    int duplicates = row["Duplicates"] is DBNull ? 0 : Convert.ToInt32(row["Duplicates"]);
                    int closedCount = row["ClosedCount"] is DBNull ? 0 : Convert.ToInt32(row["ClosedCount"]);
                    int implementedCount = row["ImplementedCount"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedCount"]);
                    //string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();
                    string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();




                    // Split the string into a list of components
                    string[] componentsList = implementedComponents.Split(',');

                    // Create a dictionary to count occurrences of each component
                    Dictionary<string, int> componentCounts = new Dictionary<string, int>();

                    // Initialize a variable to store the total count
                    int totalCountComp = 0;

                    foreach (string component in componentsList)
                    {
                        string trimmedComponent = component.Trim(); // Trim spaces
                        if (componentCounts.ContainsKey(trimmedComponent))
                        {
                            componentCounts[trimmedComponent]++; // Increment count
                        }
                        else
                        {
                            componentCounts[trimmedComponent] = 1; // Initialize count
                        }

                        // Increment the total count for each component
                        totalCountComp++;
                    }

                    // Get the original number of components (from the split list)
                    int originalCount = implementedCount;

                    // Build the formatted result using string.Format
                    List<string> resultList = new List<string>();

                    foreach (KeyValuePair<string, int> kvp in componentCounts)
                    {
                        resultList.Add(string.Format("{0} - {1}", kvp.Key, kvp.Value));
                    }

                    // Check if total count does not match the original count
                    if (totalCountComp != originalCount)
                    {
                        int difference = Math.Abs(originalCount - totalCountComp);
                        resultList.Add(string.Format("Other - {0}", difference));
                    }

                    // Join the result into a single string
                    string resultComp = string.Join(", ", resultList);

                    string implementedCount_final = originalCount.ToString();

                    int nondupes = totalCount - duplicates;

                    row["Total"] = String.Format("{0} ({1}) + Duplicates", totalCount, duplicates);
                    row["Closed"] = String.Format("{0}", closedCount);
                    row["ImplementedDetails"] = String.Format(resultComp);
                }


                // **Create a new DataTable with only the last 3 columns**
                System.Data.DataTable filteredTable = new System.Data.DataTable();
                filteredTable.Columns.Add("Total", typeof(string));
                filteredTable.Columns.Add("Closed", typeof(string));
                filteredTable.Columns.Add("ImplementedCount", typeof(string));
                filteredTable.Columns.Add("ImplementedDetails", typeof(string));


                // Copy rows from ,summaryTable,
                foreach (DataRow row in summaryTable.Rows)
                {
                    DataRow newRow = filteredTable.NewRow();
                    newRow["Total"] = row["Total"];
                    newRow["Closed"] = row["Closed"];
                    newRow["ImplementedCount"] = row["ImplementedCount"];
                    newRow["ImplementedDetails"] = row["ImplementedDetails"];

                    filteredTable.Rows.Add(newRow);
                }

                return filteredTable;
            }
        }
    }
    protected SlidePart AddSlide(PresentationPart presentationPart, System.Data.DataTable dt)
    {
        SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
        slidePart.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

        // Add Title to the Slide
        AddSlideTitle(slidePart, "GridView Data Export");

        // Add Table to Slide
        AddTableToSlide(slidePart, dt, startRow: 0, rowCount: 10);

        return slidePart;
    }

    protected void AddSlideTitle(SlidePart slidePart, string titleText)
    {
        // Use Presentation namespace for slide components
        DocumentFormat.OpenXml.Presentation.Shape titleShape = new DocumentFormat.OpenXml.Presentation.Shape(
            new DocumentFormat.OpenXml.Presentation.NonVisualShapeProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Title" },
                new DocumentFormat.OpenXml.Presentation.NonVisualShapeDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()),
            new DocumentFormat.OpenXml.Presentation.ShapeProperties(),
            new DocumentFormat.OpenXml.Presentation.TextBody(
                new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                new DocumentFormat.OpenXml.Drawing.ListStyle(),
                new DocumentFormat.OpenXml.Drawing.Paragraph(
                    new DocumentFormat.OpenXml.Drawing.Run(
                        new DocumentFormat.OpenXml.Drawing.Text { Text = titleText })))
        );

        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(titleShape);
    }

    protected void AddTableToSlide(SlidePart slidePart, System.Data.DataTable dt, int startRow, int rowCount)
    {
        var table = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid(dt.Columns.Count),  // Use the updated CreateTableGrid with percentage-based widths
            CreateTableHeader(dt)
        );

        // Append rows
        foreach (var row in CreateTableRows(dt, startRow, rowCount))
        {
            table.AppendChild(row);
        }

        // Define the slide width and height in EMUs
        long slideWidth = 9144000;  // Full slide width in EMUs
        long slideHeight = 5144000; // Full slide height in EMUs

        // Set table position as a percentage of slide width and height
        double percentageX = 0.02; // 10% of slide width from the left
        double percentageY = 0.04; // 10% of slide height from the top

        // Calculate the position (Offset) based on percentages
        long offsetX = (long)(slideWidth * percentageX);
        long offsetY = (long)(slideHeight * percentageY);

        // Define the graphic frame and ensure the table fits within the slide dimensions
        var graphicFrame = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Table" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX, Y = offsetY },  // Position the table
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.5) }  // Set the table size
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Append the graphic frame to the slide's shape tree
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame);
    }

    protected void AddTableToSlide_grid2_3(SlidePart slidePart, System.Data.DataTable dt, int startRow, int rowCount)
    {
        var table = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt.Columns.Count),  // Use the updated CreateTableGrid with percentage-based widths
            CreateTableHeader(dt)
        );

        // Append rows
        foreach (var row in CreateTableRows_grid2_3(dt, startRow, rowCount))
        {
            table.AppendChild(row);
        }

        // Define the slide width and height in EMUs
        long slideWidth = 9144000;  // Full slide width in EMUs
        long slideHeight = 5144000; // Full slide height in EMUs

        // Set table position as a percentage of slide width and height
        double percentageX = 0.02; // 10% of slide width from the left
        double percentageY = 0.04; // 10% of slide height from the top

        // Calculate the position (Offset) based on percentages
        long offsetX = (long)(slideWidth * percentageX);
        long offsetY = (long)(slideHeight * percentageY);

        // Define the graphic frame and ensure the table fits within the slide dimensions
        var graphicFrame = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Table" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX, Y = offsetY },  // Position the table
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.5) }  // Set the table size
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Append the graphic frame to the slide's shape tree
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame);
    }
    private void AddThreeTablesToSlide(SlidePart slidePart, System.Data.DataTable dt1, System.Data.DataTable dt2, System.Data.DataTable dt3, int startRow1, int rowCount1, int startRow2, int rowCount2, int startRow3, int rowCount3)
    {
        // Create the first table
        var table1 = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt1.Columns.Count),
            CreateTableHeader(dt1)
        );

        foreach (var row in CreateTableRows_grid2_3(dt1, startRow1, rowCount1))
        {
            table1.AppendChild(row);
        }

        // Create the second table
        var table2 = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt2.Columns.Count),
            CreateTableHeader(dt2)
        );

        foreach (var row in CreateTableRows_grid2_3(dt2, startRow2, rowCount2))
        {
            table2.AppendChild(row);
        }

        // Create the third table
        var table3 = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt3.Columns.Count),
            CreateTableHeader(dt3)
        );

        foreach (var row in CreateTableRows_grid2_3(dt3, startRow3, rowCount3))
        {
            table3.AppendChild(row);
        }

        // Define the slide width and height in EMUs
        long slideWidth = 9144000;  // Full slide width in EMUs
        long slideHeight = 5144000; // Full slide height in EMUs

        // Set table positions
        long offsetX1 = (long)(slideWidth * 0.02);
        long offsetY1 = (long)(slideHeight * 0.04);

        long offsetX2 = (long)(slideWidth * 0.02);
        long offsetY2 = (long)(slideHeight * 0.4);

        long offsetX3 = (long)(slideWidth * 0.02);
        long offsetY3 = (long)(slideHeight * 0.74); // Position the third table lower on the slide

        // Add the first table to the slide
        var graphicFrame1 = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Table1" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX1, Y = offsetY1 },
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.3) }
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table1)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Add the second table to the slide
        var graphicFrame2 = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 2, Name = "Table2" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX2, Y = offsetY2 },
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.3) }
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table2)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Add the third table to the slide
        var graphicFrame3 = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 3, Name = "Table3" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX3, Y = offsetY3 },
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.3) }
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table3)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Append the graphic frames to the slide's shape tree
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame1);
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame2);
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame3);
    }




    private DocumentFormat.OpenXml.Drawing.TableGrid CreateTableGrid(int columnCount)
    {
        var grid = new DocumentFormat.OpenXml.Drawing.TableGrid();

        // Define column widths as percentages of the slide width (100% = 9144000 EMUs)
        long slideWidth = 9144000; // Full slide width in EMUs
        double[] columnPercentages = new double[] { 0.05, 0.075, 0.2, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075 }; // Example for column widths

        // Calculate the EMU width for each column based on the percentage
        for (int i = 0; i < columnCount; i++)
        {
            // Calculate width based on percentage
            long columnWidth = (long)(slideWidth * columnPercentages[i]);

            grid.AppendChild(new DocumentFormat.OpenXml.Drawing.GridColumn() { Width = columnWidth });
        }

        return grid;
    }

    private DocumentFormat.OpenXml.Drawing.TableGrid CreateTableGrid2_3(int columnCount)
    {
        var grid = new DocumentFormat.OpenXml.Drawing.TableGrid();

        // Define column widths as percentages of the slide width (100% = 9144000 EMUs)
        long slideWidth = 9144000; // Full slide width in EMUs
        double[] columnPercentages = new double[] { 0.16, 0.16, 0.16, 0.16, 0.16, 0.16, 0.16 }; // Example for column widths

        // Calculate the EMU width for each column based on the percentage
        for (int i = 0; i < columnCount; i++)
        {
            // Calculate width based on percentage
            long columnWidth = (long)(slideWidth * columnPercentages[i]);

            grid.AppendChild(new DocumentFormat.OpenXml.Drawing.GridColumn() { Width = columnWidth });
        }

        return grid;
    }


    protected DocumentFormat.OpenXml.Drawing.TableRow CreateTableHeader(System.Data.DataTable dt)
    {
        var headerRow = new DocumentFormat.OpenXml.Drawing.TableRow();
        foreach (DataColumn column in dt.Columns)
        {
            var headerCell = new DocumentFormat.OpenXml.Drawing.TableCell(
                new DocumentFormat.OpenXml.Drawing.TextBody(
                    new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                    new DocumentFormat.OpenXml.Drawing.Paragraph(
                        new DocumentFormat.OpenXml.Drawing.Run(
                            new DocumentFormat.OpenXml.Drawing.RunProperties() { FontSize = 800 },  // Set font size (600 = 12 pt)
                            new DocumentFormat.OpenXml.Drawing.Text(column.ColumnName)))));
            headerRow.AppendChild(headerCell);
        }
        return headerRow;
    }

    protected IEnumerable<DocumentFormat.OpenXml.Drawing.TableRow> CreateTableRows(System.Data.DataTable dt, int startRow, int rowCount)
    {
        for (int i = startRow; i < Math.Min(startRow + rowCount, dt.Rows.Count); i++)
        {
            var dataRow = new DocumentFormat.OpenXml.Drawing.TableRow();
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                var cellValue = dt.Rows[i].ItemArray[j].ToString();
                var dataCell = new DocumentFormat.OpenXml.Drawing.TableCell();

                // If it's the first column, apply background color and no text
                if (j == 0)
                {
                    // Set the background color based on the value of the first column
                    string cellColor = GetCellColorBasedOnValue(cellValue);

                    // Apply the background color to the first column cell
                    dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TableCellProperties(
                        new DocumentFormat.OpenXml.Drawing.SolidFill(
                            new DocumentFormat.OpenXml.Drawing.RgbColorModelHex() { Val = cellColor }
                        )
                    ));

                    // Apply font formatting (keeping the font size)
                    dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TextBody(
                        new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                        new DocumentFormat.OpenXml.Drawing.ListStyle(),
                        new DocumentFormat.OpenXml.Drawing.Paragraph()
                    ));
                }
                else
                {
                    // For other columns, print the value with font formatting (e.g., size)
                    dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TextBody(
                        new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                        new DocumentFormat.OpenXml.Drawing.ListStyle(),
                        new DocumentFormat.OpenXml.Drawing.Paragraph(
                            new DocumentFormat.OpenXml.Drawing.Run(
                                new DocumentFormat.OpenXml.Drawing.RunProperties() { FontSize = 800 },  // Set font size (800 = 16 pt)
                                new DocumentFormat.OpenXml.Drawing.Text(cellValue)
                            )
                        )
                    ));
                }

                // Append the cell to the row
                dataRow.AppendChild(dataCell);
            }
            yield return dataRow;
        }
    }


    // Helper method to get the color based on the value in the first column
    private string GetCellColorBasedOnValue(string value)
    {
        // Example: Set color based on value
        if (value == "red")
        {
            return "ff0000";
        }
        else if (value == "green")
        {
            return "008000";
        }
        else if (value == "yellow")
        {
            return "ffff00";
        }
        else if (value == "orange")
        {
            return "ffa500";
        }
        else
        {
            return "FFFFFF"; // Default: White
        }
    }

    protected IEnumerable<DocumentFormat.OpenXml.Drawing.TableRow> CreateTableRows_grid2_3(System.Data.DataTable dt, int startRow, int rowCount)
    {
        for (int i = startRow; i < Math.Min(startRow + rowCount, dt.Rows.Count); i++)
        {
            var dataRow = new DocumentFormat.OpenXml.Drawing.TableRow();
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                var cellValue = dt.Rows[i].ItemArray[j].ToString();
                var dataCell = new DocumentFormat.OpenXml.Drawing.TableCell();


                // For other columns, print the value with font formatting (e.g., size)
                dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TextBody(
                    new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                    new DocumentFormat.OpenXml.Drawing.ListStyle(),
                    new DocumentFormat.OpenXml.Drawing.Paragraph(
                        new DocumentFormat.OpenXml.Drawing.Run(
                            new DocumentFormat.OpenXml.Drawing.RunProperties() { FontSize = 800 },  // Set font size (800 = 16 pt)
                            new DocumentFormat.OpenXml.Drawing.Text(cellValue)
                        )
                    )
                ));


                // Append the cell to the row
                dataRow.AppendChild(dataCell);
            }
            yield return dataRow;
        }
    }

    protected void btnImportExcel_Click(object sender, EventArgs e)
    {
        if (fileUploadExcel.HasFile)
        {
            try
            {
                string filePath = Path.Combine(Server.MapPath("~/Uploads"), fileUploadExcel.FileName);
                fileUploadExcel.SaveAs(filePath);

                System.Data.DataTable dt = ReadExcelFile(filePath);
                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowsUpdated = UpdateDatabase(dt);
                    lblMessage.Text = rowsUpdated + " rows updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "No matching data found in the uploaded file.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                // Escape quotes to avoid breaking JavaScript
                string errorMessage = ex.Message.Replace("'", "\\'").Replace(Environment.NewLine, " ");

                string script = string.Format(@"
            <script type='text/javascript'>
                window.onload = function() {{
                    showToast('{0}');
                }};
            </script>", errorMessage);

                ClientScript.RegisterStartupScript(this.GetType(), "showErrorToast", script);
            }
            //catch (Exception ex)
            //{
            //    lblMessage.Text = "Error: " + ex.Message;
            //    lblMessage.ForeColor = System.Drawing.Color.Red;
            //}
        }
        else
        {
            lblMessage.Text = "Please select an Excel file.";
            lblMessage.ForeColor = System.Drawing.Color.Red;
        }

        BindGridView_design_open();
    }

    private System.Data.DataTable ReadExcelFile(string filePath)
    {
        System.Data.DataTable dt = new System.Data.DataTable();

        using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets["Report"]; // Explicitly fetch "Report"

            if (worksheet == null)
            {
                throw new Exception("Please upload a Valid Excel File. Worksheet 'Report' not found in the uploaded file.");
            }

            if (worksheet.Dimension == null)
            {
                throw new Exception("The worksheet 'Report' is empty.");
            }// Read first sheet
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column; // Get total columns

            int customerDetailIndex = -1;
            int swImageIndex = -1;
            int supportModel = -1;

            // Read the header row (Row 1)
            for (int col = 1; col <= colCount; col++)
            {
                string header = worksheet.Cells[1, col].Text.Trim();
                if (header == "HSD-ES Customer Detail")
                    customerDetailIndex = col;
                else if (header == "Software Image Freeze (IF) Planned Date")
                    swImageIndex = col;
                else if (header == "Support Model")
                    supportModel = col;
            }

            // Ensure both columns exist
            if (customerDetailIndex == -1 || swImageIndex == -1 || supportModel == -1)
            {
                throw new Exception("Required columns not found in Excel file.");
            }

            dt.Columns.Add("HSD-ES Customer Detail");
            dt.Columns.Add("Software Image Freeze (IF) Planned Date");
            dt.Columns.Add("Support Model");

            // Read data rows
            for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip headers)
            {
                DataRow dr = dt.NewRow();
                dr["HSD-ES Customer Detail"] = worksheet.Cells[row, customerDetailIndex].Text.Trim();
                dr["Software Image Freeze (IF) Planned Date"] = worksheet.Cells[row, swImageIndex].Text.Trim();
                dr["Support Model"] = worksheet.Cells[row, supportModel].Text.Trim();
                dt.Rows.Add(dr);
            }
        }

        return dt;
    }


    private int UpdateDatabase(System.Data.DataTable dt)
    {
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";

        int rowsUpdated = 0;
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            foreach (DataRow row in dt.Rows)
            {
                string customerDetails = row["HSD-ES Customer Detail"].ToString();
                string swValue = row["Software Image Freeze (IF) Planned Date"].ToString();
                string supportMod = row["Support Model"].ToString();

                string query = "UPDATE " + designTable +
                               " SET sw_image_freeze = @SWValue, support_model = @SupportMod WHERE customer_detail = @CustomerDetails";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SWValue", swValue);
                    cmd.Parameters.AddWithValue("@CustomerDetails", customerDetails);
                    cmd.Parameters.AddWithValue("@SupportMod", supportMod);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        rowsUpdated += result;
                }
            }
        }

        return rowsUpdated;
    }

    // ============================================================================
    // SHARED FILTER PANEL FOR ISSUE LIST & CMF PENDING LIST
    // ============================================================================

    /// <summary>
    /// Called during Page_Load to initialize the shared filter panel visibility and populate filter dropdowns
    /// </summary>
    protected void InitializeSharedFilterPanel()
    {
        // Don't proceed if sharedFilterPanel doesn't exist
        if (sharedFilterPanel == null)
        {
            return;
        }

        // Don't show shared filter panel if welcome page is visible
        if (homeWelcomePanel != null && homeWelcomePanel.Visible)
        {
            sharedFilterPanel.Visible = false;
            return;
        }

        // Show shared filter panel only when Issue List or CMF Pending List is active
        string activeTab = GetActiveFocusedTab();
        sharedFilterPanel.Visible = (activeTab == "issue" || activeTab == "pending");
        
        if (sharedFilterPanel.Visible)
        {
            PopulateSharedFilterDropdowns();
        }
    }

    /// <summary>
    /// Populate Platform dropdown with available platforms
    /// </summary>
    private void PopulateSharedFilterDropdowns()
    {
        try
        {
            List<CmfPlatformDefinition> availablePlatforms = GetAvailableIndividualPlatforms();
            HashSet<string> availableSet = new HashSet<string>(
                availablePlatforms.Select(option => option.TableName),
                StringComparer.OrdinalIgnoreCase);

            bool pendingActive = string.Equals(GetActiveFocusedTab(), "pending", StringComparison.OrdinalIgnoreCase);
            ddlSharedPlatform.SelectionMode = pendingActive ? ListSelectionMode.Multiple : ListSelectionMode.Single;
            ddlSharedPlatform.Items.Clear();
            if (availablePlatforms.Count > 0)
            {
                foreach (CmfPlatformDefinition option in availablePlatforms)
                {
                    ddlSharedPlatform.Items.Add(new ListItem(option.Label, option.TableName));
                }
            }

            if (pendingActive)
            {
                HashSet<string> pendingSelections = new HashSet<string>(GetPendingPlatformSelections(), StringComparer.OrdinalIgnoreCase);
                ddlSharedPlatform.ClearSelection();
                foreach (ListItem option in ddlSharedPlatform.Items)
                {
                    option.Selected = pendingSelections.Contains(option.Value);
                }
                if (ddlSharedPlatform.SelectedIndex < 0 && ddlSharedPlatform.Items.Count > 0)
                {
                    ddlSharedPlatform.Items[0].Selected = true;
                }
                return;
            }

            string selectedValue = ResolveFirstAvailablePlatform(
                new[] { GetIssuePendingPlatform(), DefaultIssuePendingPlatformTable },
                availableSet,
                availablePlatforms);

            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                ddlSharedPlatform.ClearSelection();
                ListItem item = ddlSharedPlatform.Items.FindByValue(selectedValue);
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error populating platform filters: " + ex.Message);
        }
    }

    /// <summary>
    /// Handle platform selection change - switch to selected platform and rebind GridViews
    /// </summary>
    protected void ddlSharedPlatform_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (string.Equals(GetActiveFocusedTab(), "pending", StringComparison.OrdinalIgnoreCase))
            {
                HashSet<string> available = new HashSet<string>(
                    GetAvailableIndividualPlatforms().Select(option => option.TableName), StringComparer.OrdinalIgnoreCase);
                string[] selections = (Request.Form.GetValues(ddlSharedPlatform.UniqueID) ?? new string[0])
                    .Where(available.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                Session[PendingPlatformsSessionKey] = selections.Length > 0
                    ? selections : new[] { DefaultIssuePendingPlatformTable };
                EnsurePendingTabVisibleForPostback();
                RebindFocusedTabData(false);
                PopulateSharedFilterDropdowns();
                return;
            }

            string postedPlatform = Request.Form[ddlSharedPlatform.UniqueID];
            string selectedPlatformTable = ResolveIssuePendingPlatformSelection(
                string.IsNullOrWhiteSpace(postedPlatform) ? ddlSharedPlatform.SelectedValue : postedPlatform);

            if (!string.IsNullOrEmpty(selectedPlatformTable))
            {
                // This filter is scoped only to Issue List and CMF Pending tabs.
                Session[IssuePendingPlatformSessionKey] = selectedPlatformTable;
                selectedPlatform = selectedPlatformTable;

                string dashboardSelection = Session[DashboardPlatformSessionKey] as string;
                if (IsOverallDashboardPlatform(selectedPlatformTable))
                {
                    SetDashboardPlatformSelection(selectedPlatformTable);
                }
                else if (string.IsNullOrWhiteSpace(dashboardSelection) || !IsOverallDashboardPlatform(dashboardSelection))
                {
                    SetDashboardPlatformSelection(selectedPlatformTable);
                }

                overall_request_details.PageIndex = 0;
                ResetIssueFiltersToAll();
                InitializeFilterValue();
                UpdatePlatformDashboardLink();

                string activeTab = GetActiveFocusedTab();
                if (string.Equals(activeTab, "pending", StringComparison.OrdinalIgnoreCase))
                {
                    EnsurePendingTabVisibleForPostback();
                }
                else
                {
                    EnsureIssueTabVisibleForPostback();
                }

                RebindFocusedTabData(false);

                // Re-sync dropdown selections to the resolved platform.
                PopulateSharedFilterDropdowns();

            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error changing platform: " + ex.Message);
        }
    }

    /// <summary>
    /// Update CMF Pending List KPI values based on GridView_cmf_pending data
    /// </summary>
    private void UpdateCmfPendingKpis()
    {
        try
        {
            if (GridView_cmf_pending.DataSource == null)
            {
                lblPendingSightings.Text = "0";
                lblQualifyForCmf.Text = "0";
                lblLikelyDuplicate.Text = "0";
                lblIncompleteSysScope.Text = "0";
                return;
            }

            DataTable dt = GridView_cmf_pending.DataSource as DataTable;
            if (dt == null) return;

            int totalPending = dt.Rows.Count;
            int qualifyForCmf = 0;
            int likelyDuplicate = 0;
            int incompleteSysScope = 0;
            string activeRules = CmfRecommendationService.GetActiveRulesText();
            Dictionary<string, int> normalizedTitleCounts = BuildPendingTitleCounts(dt);

            foreach (DataRow row in dt.Rows)
            {
                if (IsPendingLikelyCmfCandidate(row, activeRules))
                {
                    qualifyForCmf++;
                }

                if (IsPendingSysScopeIncomplete(row))
                {
                    incompleteSysScope++;
                }

                if (IsPendingLikelyDuplicate(row, normalizedTitleCounts))
                {
                    likelyDuplicate++;
                }
            }

            // Update labels
            lblPendingSightings.Text = totalPending.ToString();
            lblQualifyForCmf.Text = qualifyForCmf.ToString();
            lblLikelyDuplicate.Text = likelyDuplicate.ToString();
            lblIncompleteSysScope.Text = incompleteSysScope.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error updating CMF Pending KPIs: " + ex.Message);
            lblPendingSightings.Text = "0";
            lblQualifyForCmf.Text = "0";
            lblLikelyDuplicate.Text = "0";
            lblIncompleteSysScope.Text = "0";
        }
    }

    private static Dictionary<string, int> BuildPendingTitleCounts(DataTable dt)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (DataRow row in dt.Rows)
        {
            string normalizedTitle = NormalizePendingText(GetPendingValue(row, "title"));
            if (string.IsNullOrWhiteSpace(normalizedTitle)) continue;
            if (!counts.ContainsKey(normalizedTitle)) counts[normalizedTitle] = 0;
            counts[normalizedTitle]++;
        }
        return counts;
    }

    private static bool IsPendingLikelyCmfCandidate(DataRow row, string activeRules)
    {
        string title = GetPendingValue(row, "title");
        string component = GetPendingValue(row, "component");
        string impact = GetPendingValue(row, "impact");
        string cmfRequest = GetPendingValue(row, "cmf_request");
        string reproducibility = GetPendingValue(row, "reproducibility");
        string reproOnRvp = GetPendingValue(row, "repro_on_rvp");

        bool hasContext = !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(component) && !string.IsNullOrWhiteSpace(impact);
        bool hasReproSignal = HasAny(reproducibility, "repro", "always", "consistent", "yes", "100") || HasAny(reproOnRvp, "yes", "y", "true", "repro");
        bool hasRequestIntent = HasAny(cmfRequest, "cmf_ok", "cmf", "request", "pending", "ask");
        bool highImpact = HasAny(impact, "critical", "high", "block", "hang", "crash", "data loss", "certification", "customer");
        bool lowImpact = HasAny(impact, "no impact", "low", "minor", "cosmetic");

        string normalizedRules = (activeRules ?? string.Empty).ToLowerInvariant();
        bool rulesRequireRepro = normalizedRules.Contains("reproducibility") || normalizedRules.Contains("rvp repro");
        bool rulesRequireRequest = normalizedRules.Contains("cmf_request") || normalizedRules.Contains("cmf review") || normalizedRules.Contains("cmf_ok");

        return hasContext && highImpact && !lowImpact && (!rulesRequireRepro || hasReproSignal) && (!rulesRequireRequest || hasRequestIntent);
    }

    private static bool IsPendingSysScopeIncomplete(DataRow row)
    {
        return string.IsNullOrWhiteSpace(GetPendingValue(row, "reproducibility"))
            || string.IsNullOrWhiteSpace(GetPendingValue(row, "repro_on_rvp"))
            || string.IsNullOrWhiteSpace(GetPendingValue(row, "idst"))
            || string.IsNullOrWhiteSpace(GetPendingValue(row, "impact"));
    }

    private static bool IsPendingLikelyDuplicate(DataRow row, Dictionary<string, int> normalizedTitleCounts)
    {
        string title = GetPendingValue(row, "title");
        string impact = GetPendingValue(row, "impact");
        string cmfRequest = GetPendingValue(row, "cmf_request");
        string normalizedTitle = NormalizePendingText(title);

        if (!string.IsNullOrWhiteSpace(normalizedTitle) && normalizedTitleCounts.ContainsKey(normalizedTitle) && normalizedTitleCounts[normalizedTitle] > 1)
        {
            return true;
        }

        return HasAny(title, "duplicate", "dup", "same as", "matches")
            || HasAny(impact, "duplicate", "same issue", "already reported", "matches")
            || HasAny(cmfRequest, "duplicate", "dup", "merge");
    }

    private static string GetPendingValue(DataRow row, string columnName)
    {
        if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value) return string.Empty;
        return row[columnName].ToString().Trim();
    }

    private static string NormalizePendingText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        string normalized = new string(value.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        return string.Join(" ", normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool HasAny(string value, params string[] terms)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        string normalized = value.ToLowerInvariant();
        foreach (string term in terms)
        {
            if (!string.IsNullOrWhiteSpace(term) && normalized.Contains(term.ToLowerInvariant())) return true;
        }
        return false;
    }

    /// <summary>
    /// Update CMF Pending List accessibility links (Platform Dashboard)
    /// </summary>
    private void UpdateCmfPendingAccessibilityLinks()
    {
        try
        {
            List<string> platforms = GetPendingPlatformSelections();
            if (platforms.Count > 1)
            {
                lnkPlatformDashboardPending.Visible = false;
                lnkPlatformDashboard.Visible = false;
                return;
            }
            string currentPlatform = platforms[0];
            
            if (string.IsNullOrEmpty(currentPlatform))
            {
                lnkPlatformDashboardPending.Visible = false;
                return;
            }

            CmfPlatformDefinition platform;
            if (CmfPlatformCatalog.TryGetPlatformByTable(currentPlatform, out platform) &&
                !string.IsNullOrWhiteSpace(platform.DashboardProjectUrl))
            {
                lnkPlatformDashboardPending.NavigateUrl = platform.DashboardProjectUrl;
                lnkPlatformDashboardPending.Text = platform.Label + " Dashboard";
                lnkPlatformDashboardPending.Visible = true;
                if (lnkPlatformDashboard != null)
                {
                    lnkPlatformDashboard.Visible = false;
                }
            }
            else
            {
                lnkPlatformDashboardPending.Visible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error updating CMF Pending accessibility links: " + ex.Message);
            lnkPlatformDashboardPending.Visible = false;
        }
    }

}