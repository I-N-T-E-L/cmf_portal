using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;

public sealed class CmfPlatformDefinition
{
    public CmfPlatformDefinition(
        string label,
        string tableName,
        string dashboardProjectUrl,
        string cdrDocumentUrl,
        string[] promptAliases)
    {
        Label = label ?? string.Empty;
        TableName = tableName ?? string.Empty;
        DashboardProjectUrl = dashboardProjectUrl ?? string.Empty;
        CdrDocumentUrl = cdrDocumentUrl ?? string.Empty;
        PromptAliases = promptAliases ?? new string[0];
    }

    public string Label { get; private set; }
    public string TableName { get; private set; }
    public string DashboardProjectUrl { get; private set; }
    public string CdrDocumentUrl { get; private set; }
    public string[] PromptAliases { get; private set; }
}

public sealed class CmfPlatformSummaryCounts
{
    public CmfPlatformSummaryCounts(long open, long implemented, long pending, long total = 0)
    {
        Open = open;
        Implemented = implemented;
        Pending = pending;
        Total = total;
    }

    public long Open { get; private set; }
    public long Implemented { get; private set; }
    public long Pending { get; private set; }
    public long Total { get; private set; }
}

public static class CmfPlatformCatalog
{
    public const string OverallPlatformTableName = "CMF_OVERALL_ALL_COMPONENTS_TABLE";

    public static int GetPlatformDisplayPriority(string label)
    {
        if (string.IsNullOrWhiteSpace(label)) return 999;
        string clean = label.Trim().ToUpperInvariant();
        if (clean == "NVL-H") return 1;
        if (clean == "NVL-S") return 2;
        if (clean == "NVL-HX" || clean == "NVL_HX") return 3;
        if (clean.StartsWith("NVL")) return 4;
        return 10;
    }

    public static string[] GetProcessorTokens(string platformTable)
    {
        string code = ResolvePlatformCode(platformTable).ToLowerInvariant();
        switch (code)
        {
            case "nvl_h": return new[] { "novalake_h" };
            case "nvl_hx": return new[] { "novalake_hx" };
            case "nvl_s": return new[] { "novalake_s" };
            case "nvl_u": return new[] { "novalake_u" };
            case "arl_h": return new[] { "arrowlake_h", "arrowlake_h20-65", "arrowlake_h35-65" };
            case "arl_hx": return new[] { "arrowlake_hx" };
            case "arl_refresh": return new[] { "arrowlake_hx_816_refresh" };
            case "arl_s": return new[] { "arrowlake_s" };
            case "arl_u": return new[] { "arrowlake_u" };
            case "ptl": return new[] { "pantherlake_h12xe", "pantherlake_h484", "pantherlake_h404" };
            case "lnl": return new[] { "lunarlake_m", "lunarlake_p" };
            case "gnr": return new[] { "graniterapids_xcc", "graniterapids_hcc", "graniterapids_lcc" };
            case "wcl": return new[] { "wildcatlake", "wildcatlak" };
            default: return new[] { code };
        }
    }

    public static string BuildProcessorMembershipPredicate(string platformTable, string alias = null)
    {
        if (!IsDiscoverablePlatformTable(platformTable))
            throw new ArgumentException("A physical platform issue table is required.", "platformTable");
        if (!string.IsNullOrEmpty(alias) && !Regex.IsMatch(alias, @"\A[A-Za-z_][A-Za-z0-9_]*\z"))
            throw new ArgumentException("Invalid source alias.", "alias");
        string column = string.IsNullOrEmpty(alias) ? "[processor]" : alias + ".[processor]";
        string normalized = "LOWER(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(" + column
            + ", ''), ' ', ''), CHAR(9), ''), CHAR(13), ''), CHAR(10), ''))";
        List<string> predicates = new List<string>();
        foreach (string token in GetProcessorTokens(platformTable))
            predicates.Add("CHARINDEX('," + token + ",', ',' + " + normalized + " + ',') > 0");
        predicates.Add("REPLACE(REPLACE(" + normalized + ", 'unassigned', ''), ',', '') = ''");
        return "(" + string.Join(" OR ", predicates) + ")";
    }

    public static string ScopeIssueSql(string sql)
    {
        const string marker = "/*cmf-platform-membership-v1*/";
        if (string.IsNullOrWhiteSpace(sql) || sql.StartsWith(marker, StringComparison.Ordinal)) return sql;
        MatchCollection tokens = Regex.Matches(sql,
            @"N?'(?:''|[^'])*'|--[^\r\n]*|/\*[\s\S]*?\*/|\[(?:\]\]|[^\]])*\]|[A-Za-z_][A-Za-z0-9_]*|[^\s]",
            RegexOptions.IgnoreCase);
        List<Match> syntax = new List<Match>();
        foreach (Match token in tokens)
        {
            if (!token.Value.StartsWith("--", StringComparison.Ordinal)
                && !token.Value.StartsWith("/*", StringComparison.Ordinal)) syntax.Add(token);
        }
        int first = 0;
        while (first < syntax.Count && syntax[first].Value == ";") first++;
        if (first == syntax.Count || (!string.Equals(syntax[first].Value, "SELECT", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(syntax[first].Value, "WITH", StringComparison.OrdinalIgnoreCase))) return sql;
        foreach (Match token in syntax)
            if (Regex.IsMatch(token.Value, @"\A(?:INSERT|UPDATE|DELETE|MERGE|TRUNCATE|ALTER|DROP|CREATE|EXEC|EXECUTE)\z", RegexOptions.IgnoreCase)) return sql;
        Dictionary<string, string> sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        List<string> definitions = new List<string>();
        List<Tuple<int, int, string>> replacements = new List<Tuple<int, int, string>>();
        for (int index = first; index < syntax.Count; index++)
        {
            Match token = syntax[index];
            string tableName = token.Value.Trim('[', ']');
            bool isPending = Regex.IsMatch(tableName, @"\ACMF_[A-Z0-9]+(?:_[A-Z0-9]+)*_CMF_ASK\z", RegexOptions.IgnoreCase);
            string platformTable = isPending ? tableName.Substring(0, tableName.Length - "_CMF_ASK".Length) + "_ALL_COMPONENTS_TABLE" : tableName;
            if (!IsDiscoverablePlatformTable(platformTable)) continue;
            string scopedName;
            if (!sources.TryGetValue(tableName, out scopedName))
            {
                scopedName = "[cmf_membership_" + sources.Count + "]";
                sources.Add(tableName, scopedName);
                if (isPending)
                    definitions.Add(scopedName + " AS (SELECT pending.* FROM [dbo].[" + tableName
                        + "] AS pending WHERE NOT EXISTS (SELECT 1 FROM [dbo].[" + platformTable
                        + "] AS assigned WHERE assigned.cp_id = pending.cp_id) OR EXISTS (SELECT 1 FROM [dbo].["
                        + platformTable + "] AS assigned WHERE assigned.cp_id = pending.cp_id AND "
                        + BuildProcessorMembershipPredicate(platformTable, "assigned") + "))");
                else
                    definitions.Add(scopedName + " AS (SELECT * FROM [dbo].[" + tableName + "] WHERE "
                        + BuildProcessorMembershipPredicate(platformTable) + ")");
            }
            int start = token.Index;
            if (index >= 2 && syntax[index - 1].Value == "."
                && string.Equals(syntax[index - 2].Value.Trim('[', ']'), "dbo", StringComparison.OrdinalIgnoreCase))
                start = syntax[index - 2].Index;
            replacements.Add(Tuple.Create(start, token.Index + token.Length - start, scopedName));
        }
        if (definitions.Count == 0) return sql;
        for (int index = replacements.Count - 1; index >= 0; index--)
        {
            Tuple<int, int, string> replacement = replacements[index];
            sql = sql.Remove(replacement.Item1, replacement.Item2).Insert(replacement.Item1, replacement.Item3);
        }
        if (string.Equals(syntax[first].Value, "WITH", StringComparison.OrdinalIgnoreCase))
            sql = sql.Remove(syntax[first].Index, syntax[first].Length).Insert(syntax[first].Index,
                "WITH " + string.Join(", ", definitions) + ",");
        else sql = "WITH " + string.Join(", ", definitions) + " " + sql.Substring(syntax[first].Index);
        return marker + sql;
    }

    public static SqlCommand CreateReadCommand(string sql, SqlConnection connection)
    {
        return new SqlCommand(ScopeIssueSql(sql), connection);
    }

    public static List<CmfPlatformDefinition> OrderBySummaryCounts(
        IEnumerable<CmfPlatformDefinition> platforms,
        IDictionary<string, CmfPlatformSummaryCounts> counts)
    {
        List<CmfPlatformDefinition> ordered = new List<CmfPlatformDefinition>(platforms);
        CmfPlatformSummaryCounts empty = new CmfPlatformSummaryCounts(0, 0, 0);
        ordered.Sort(delegate(CmfPlatformDefinition left, CmfPlatformDefinition right)
        {
            int p1 = GetPlatformDisplayPriority(left.Label);
            int p2 = GetPlatformDisplayPriority(right.Label);
            if (p1 <= 3 || p2 <= 3)
            {
                if (p1 != p2) return p1.CompareTo(p2);
            }

            CmfPlatformSummaryCounts leftCounts;
            CmfPlatformSummaryCounts rightCounts;
            if (!counts.TryGetValue(left.TableName, out leftCounts)) leftCounts = empty;
            if (!counts.TryGetValue(right.TableName, out rightCounts)) rightCounts = empty;
            int comparison = rightCounts.Open.CompareTo(leftCounts.Open);
            if (comparison == 0) comparison = rightCounts.Implemented.CompareTo(leftCounts.Implemented);
            if (comparison == 0) comparison = rightCounts.Pending.CompareTo(leftCounts.Pending);
            if (comparison == 0) comparison = rightCounts.Total.CompareTo(leftCounts.Total);
            if (comparison == 0) comparison = StringComparer.OrdinalIgnoreCase.Compare(left.Label, right.Label);
            return comparison;
        });
        return ordered;
    }

    public static bool IsDiscoverablePlatformTable(string tableName)
    {
        return !string.IsNullOrWhiteSpace(tableName)
            && Regex.IsMatch(tableName, @"\ACMF_[A-Z0-9]+(?:_[A-Z0-9]+)*_ALL_COMPONENTS_TABLE\z", RegexOptions.IgnoreCase)
            && !string.Equals(tableName, OverallPlatformTableName, StringComparison.OrdinalIgnoreCase)
            && !Regex.IsMatch(tableName, @"_(?:DUMMY|BACKUP|BAK|ARCHIVE|TEMP|TEST)(?:_|$)", RegexOptions.IgnoreCase);
    }

    public static CmfPlatformDefinition[] DiscoverPlatforms(SqlConnection connection)
    {
        if (connection == null) throw new ArgumentNullException("connection");
        List<string> candidates = new List<string>();
        using (SqlCommand command = new SqlCommand(@"
SELECT t.name
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE s.name = 'dbo'
  AND t.name LIKE 'CMF[_]%[_]ALL[_]COMPONENTS[_]TABLE'
  AND EXISTS (SELECT 1 FROM sys.columns AS c WHERE c.object_id = t.object_id AND c.name = 'cp_id')
ORDER BY t.name", connection))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string tableName = reader.GetString(0);
                if (IsDiscoverablePlatformTable(tableName)) candidates.Add(tableName);
            }
        }

        List<CmfPlatformDefinition> platforms = new List<CmfPlatformDefinition>();
        foreach (string tableName in candidates)
        {
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 1 FROM [dbo].[" + tableName
                + "] WHERE TRY_CONVERT(bigint, cp_id) > 0", connection))
            {
                if (command.ExecuteScalar() == null) continue;
            }

            CmfPlatformDefinition metadata;
            if (PlatformByTable.TryGetValue(tableName, out metadata))
            {
                platforms.Add(metadata);
                continue;
            }

            string label = ResolvePlatformCode(tableName).Replace("_", "-");
            platforms.Add(new CmfPlatformDefinition(label, tableName,
                "https://dashboards.intel.com/projects/" + label.ToLowerInvariant(), string.Empty,
                new[] { NormalizeAlias(label), NormalizeAlias(label.Replace("-", " ")), NormalizeAlias(label.Replace("-", "_")) }));
        }

        platforms.Sort(delegate(CmfPlatformDefinition left, CmfPlatformDefinition right)
        {
            int p1 = GetPlatformDisplayPriority(left.Label);
            int p2 = GetPlatformDisplayPriority(right.Label);
            if (p1 != p2) return p1.CompareTo(p2);
            return StringComparer.OrdinalIgnoreCase.Compare(left.Label, right.Label);
        });
        return platforms.ToArray();
    }

    private static readonly CmfPlatformDefinition[] KnownPlatformMetadata =
    {
        new CmfPlatformDefinition(
            "NVL-H",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/nvl-h",
            string.Empty,
            new[] { "nvl-h", "nvl h", "nvl_h", "novalake-h", "novalake h", "nova lake-h", "nova lake h" }),

        new CmfPlatformDefinition(
            "NVL-S",
            "CMF_NVL_S_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/nvl-s",
            string.Empty,
            new[] { "nvl-s", "nvl s", "nvl_s", "novalake-s", "novalake s", "nova lake-s", "nova lake s" }),

        new CmfPlatformDefinition(
            "NVL-U",
            "CMF_NVL_U_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/nvl-u",
            string.Empty,
            new[] { "nvl-u", "nvl u", "nvl_u", "novalake-u", "novalake u", "nova lake-u", "nova lake u" }),

        new CmfPlatformDefinition(
            "PTL",
            "CMF_PTL_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/ptl",
            "https://cdrdv2.intel.com/v1/dl/getContent/833017",
            new[] { "ptl", "panther lake", "pantherlake" }),

        new CmfPlatformDefinition(
            "LNL",
            "CMF_LNL_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/lnl",
            string.Empty,
            new[] { "lnl", "lunar lake", "lunarlake" }),

        new CmfPlatformDefinition(
            "ARL-S",
            "CMF_ARL_S_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-s",
            string.Empty,
            new[] { "arl-s", "arl s", "arl_s", "arrowlake-s", "arrowlake s", "arrow lake-s", "arrow lake s" }),

        new CmfPlatformDefinition(
            "ARL-H",
            "CMF_ARL_H_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-h",
            string.Empty,
            new[] { "arl-h", "arl h", "arl_h", "arrowlake-h", "arrowlake h", "arrow lake-h", "arrow lake h" }),

        new CmfPlatformDefinition(
            "ARL-U",
            "CMF_ARL_U_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-u",
            string.Empty,
            new[] { "arl-u", "arl u", "arl_u", "arrowlake-u", "arrowlake u", "arrow lake-u", "arrow lake u" }),

        new CmfPlatformDefinition(
            "ARL-Hx",
            "CMF_ARL_HX_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-hx",
            string.Empty,
            new[] { "arl-hx", "arl hx", "arl_hx", "arrowlake-hx", "arrowlake hx", "arrow lake-hx", "arrow lake hx" }),

        new CmfPlatformDefinition(
            "ARL-Refresh",
            "CMF_ARL_Refresh_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-refresh",
            string.Empty,
            new[] { "arl-refresh", "arl refresh", "arl_refresh", "arrowlake-refresh", "arrowlake refresh", "arrow lake refresh" }),

        new CmfPlatformDefinition(
            "GNR",
            "CMF_GNR_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/gnr",
            "https://cdrdv2.intel.com/v1/dl/getContent/824182",
            new[] { "gnr", "granite rapids", "graniterapids" }),

        new CmfPlatformDefinition(
            "WCL",
            "CMF_WCL_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/wcl",
            "https://cdrdv2.intel.com/v1/dl/getContent/858122",
            new[] { "wcl", "wildcat lake", "wildcatlake" })
    };

    private static readonly Dictionary<string, string[]> FamilyPromptAliases =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "nvl", new[] { "nova lake", "novalake" } },
            { "arl", new[] { "arrow lake", "arrowlake" } },
            { "ptl", new[] { "panther lake", "pantherlake" } },
            { "lnl", new[] { "lunar lake", "lunarlake" } },
            { "wcl", new[] { "wildcat lake", "wildcatlake" } },
            { "gnr", new[] { "granite rapids", "graniterapids" } }
        };

    private static readonly Dictionary<string, CmfPlatformDefinition> PlatformByTable =
        new Dictionary<string, CmfPlatformDefinition>(StringComparer.OrdinalIgnoreCase);

    static CmfPlatformCatalog()
    {
        for (int i = 0; i < KnownPlatformMetadata.Length; i++)
        {
            CmfPlatformDefinition platform = KnownPlatformMetadata[i];
            if (platform == null || string.IsNullOrWhiteSpace(platform.TableName))
            {
                continue;
            }

            PlatformByTable[platform.TableName] = platform;
        }
    }

    public static string GetDefaultPlatformTableName()
    {
        CmfPlatformDefinition[] platforms = GetOrderedPlatforms();
        return platforms.Length == 0 ? string.Empty : platforms[0].TableName;
    }

    public static CmfPlatformDefinition[] GetOrderedPlatforms()
    {
        const string cacheKey = "CMF_DiscoveredHsdPlatforms";
        HttpContext context = HttpContext.Current;
        CmfPlatformDefinition[] platforms = context == null ? null : context.Items[cacheKey] as CmfPlatformDefinition[];
        if (platforms == null)
        {
            string connStr = CmfAssistantConfigService.ResolveDatabaseConnectionString();
            if (string.IsNullOrWhiteSpace(connStr)) throw new InvalidOperationException("The CMF database connection is not configured.");
            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.Open();
                platforms = DiscoverPlatforms(connection);
            }
            if (context != null) context.Items[cacheKey] = platforms;
        }
        return (CmfPlatformDefinition[])platforms.Clone();
    }

    public static string[] GetAllPlatformTables()
    {
        CmfPlatformDefinition[] platforms = GetOrderedPlatforms();
        string[] values = new string[platforms.Length];
        for (int i = 0; i < platforms.Length; i++)
        {
            values[i] = platforms[i].TableName;
        }

        return values;
    }

    public static int GetPlatformCount()
    {
        return GetOrderedPlatforms().Length;
    }

    public static HashSet<string> GetAllowedPlatformTables()
    {
        return new HashSet<string>(GetAllPlatformTables(), StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsAllowedPlatformTable(string tableName)
    {
        return !string.IsNullOrWhiteSpace(tableName) && GetAllowedPlatformTables().Contains(tableName);
    }

    public static bool TryGetPlatformByTable(string tableName, out CmfPlatformDefinition platform)
    {
        platform = null;
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return false;
        }

        foreach (CmfPlatformDefinition candidate in GetOrderedPlatforms())
        {
            if (string.Equals(candidate.TableName, tableName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                platform = candidate;
                return true;
            }
        }
        return false;
    }

    public static Dictionary<string, string> GetPromptAliasToTableMap()
    {
        Dictionary<string, string> aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (CmfPlatformDefinition platform in GetOrderedPlatforms())
        {
            foreach (string alias in platform.PromptAliases)
            {
                aliases[NormalizeAlias(alias)] = platform.TableName;
            }
        }
        return aliases;
    }

    public static Dictionary<string, string[]> GetGroupAliasToTablesMap()
    {
        Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (CmfPlatformDefinition platform in GetOrderedPlatforms())
        {
            string family = NormalizeAlias(ResolvePlatformCode(platform.TableName).Split('_')[0]);
            List<string> tables;
            if (!groups.TryGetValue(family, out tables))
            {
                tables = new List<string>();
                groups[family] = tables;
            }
            tables.Add(platform.TableName);
        }

        Dictionary<string, string[]> aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, List<string>> group in groups)
        {
            aliases[group.Key] = group.Value.ToArray();
            string[] familyAliases;
            if (FamilyPromptAliases.TryGetValue(group.Key, out familyAliases))
            {
                foreach (string alias in familyAliases) aliases[alias] = group.Value.ToArray();
            }
        }
        return aliases;
    }

    public static string NormalizeAlias(string alias)
    {
        return (alias ?? string.Empty).Trim().ToLowerInvariant();
    }

    public static string ResolvePlatformCode(string platformTable)
    {
        if (string.IsNullOrWhiteSpace(platformTable))
        {
            return "UNKNOWN";
        }

        string value = platformTable.Trim();
        if (value.StartsWith("CMF_", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring(4);
        }

        const string suffix = "_ALL_COMPONENTS_TABLE";
        int suffixIndex = value.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
        if (suffixIndex >= 0)
        {
            value = value.Substring(0, suffixIndex);
        }

        return string.IsNullOrWhiteSpace(value) ? "UNKNOWN" : value;
    }
}
