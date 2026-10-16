using System;
using System.Collections.Generic;

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

public static class CmfPlatformCatalog
{
    public const string OverallPlatformTableName = "CMF_OVERALL_ALL_COMPONENTS_TABLE";

    private static readonly CmfPlatformDefinition[] OrderedPlatforms =
    {
        new CmfPlatformDefinition(
            "NVL-H",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/nvl-h",
            string.Empty,
            new[] { "nvl-h", "nvl h", "nvl_h" }),

        new CmfPlatformDefinition(
            "NVL-S",
            "CMF_NVL_S_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/nvl-s",
            string.Empty,
            new[] { "nvl-s", "nvl s", "nvl_s" }),

        new CmfPlatformDefinition(
            "NVL-U",
            "CMF_NVL_U_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/nvl-u",
            string.Empty,
            new[] { "nvl-u", "nvl u", "nvl_u" }),

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
            new[] { "arl-s", "arl s", "arl_s" }),

        new CmfPlatformDefinition(
            "ARL-H",
            "CMF_ARL_H_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-h",
            string.Empty,
            new[] { "arl-h", "arl h", "arl_h" }),

        new CmfPlatformDefinition(
            "ARL-U",
            "CMF_ARL_U_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-u",
            string.Empty,
            new[] { "arl-u", "arl u", "arl_u" }),

        new CmfPlatformDefinition(
            "ARL-Hx",
            "CMF_ARL_HX_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-hx",
            string.Empty,
            new[] { "arl-hx", "arl hx", "arl_hx" }),

        new CmfPlatformDefinition(
            "ARL-Refresh",
            "CMF_ARL_Refresh_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/arl-refresh",
            string.Empty,
            new[] { "arl-refresh", "arl refresh", "arl_refresh" }),

        new CmfPlatformDefinition(
            "GNR",
            "CMF_GNR_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/gnr",
            "https://cdrdv2.intel.com/v1/dl/getContent/824182",
            new[] { "gnr" }),

        new CmfPlatformDefinition(
            "WCL",
            "CMF_WCL_ALL_COMPONENTS_TABLE",
            "https://dashboards.intel.com/projects/wcl",
            "https://cdrdv2.intel.com/v1/dl/getContent/858122",
            new[] { "wcl", "wildcat lake", "wildcatlake" })
    };

    private static readonly Dictionary<string, string[]> GroupAliasToTables =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "nvl",
                new[]
                {
                    "CMF_NVL_S_ALL_COMPONENTS_TABLE",
                    "CMF_NVL_H_ALL_COMPONENTS_TABLE",
                    "CMF_NVL_U_ALL_COMPONENTS_TABLE"
                }
            },
            {
                "nova lake",
                new[]
                {
                    "CMF_NVL_S_ALL_COMPONENTS_TABLE",
                    "CMF_NVL_H_ALL_COMPONENTS_TABLE",
                    "CMF_NVL_U_ALL_COMPONENTS_TABLE"
                }
            },
            {
                "novalake",
                new[]
                {
                    "CMF_NVL_S_ALL_COMPONENTS_TABLE",
                    "CMF_NVL_H_ALL_COMPONENTS_TABLE",
                    "CMF_NVL_U_ALL_COMPONENTS_TABLE"
                }
            }
        };

    private static readonly Dictionary<string, CmfPlatformDefinition> PlatformByTable =
        new Dictionary<string, CmfPlatformDefinition>(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> PromptAliasToTable =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AllowedPlatformTables =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    static CmfPlatformCatalog()
    {
        for (int i = 0; i < OrderedPlatforms.Length; i++)
        {
            CmfPlatformDefinition platform = OrderedPlatforms[i];
            if (platform == null || string.IsNullOrWhiteSpace(platform.TableName))
            {
                continue;
            }

            AllowedPlatformTables.Add(platform.TableName);
            PlatformByTable[platform.TableName] = platform;

            if (platform.PromptAliases == null)
            {
                continue;
            }

            for (int aliasIndex = 0; aliasIndex < platform.PromptAliases.Length; aliasIndex++)
            {
                string alias = NormalizeAlias(platform.PromptAliases[aliasIndex]);
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    PromptAliasToTable[alias] = platform.TableName;
                }
            }
        }
    }

    public static string GetDefaultPlatformTableName()
    {
        return OrderedPlatforms.Length == 0 ? string.Empty : OrderedPlatforms[0].TableName;
    }

    public static CmfPlatformDefinition[] GetOrderedPlatforms()
    {
        return (CmfPlatformDefinition[])OrderedPlatforms.Clone();
    }

    public static string[] GetAllPlatformTables()
    {
        string[] values = new string[OrderedPlatforms.Length];
        for (int i = 0; i < OrderedPlatforms.Length; i++)
        {
            values[i] = OrderedPlatforms[i].TableName;
        }

        return values;
    }

    public static int GetPlatformCount()
    {
        return OrderedPlatforms.Length;
    }

    public static HashSet<string> GetAllowedPlatformTables()
    {
        return new HashSet<string>(AllowedPlatformTables, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsAllowedPlatformTable(string tableName)
    {
        return !string.IsNullOrWhiteSpace(tableName) && AllowedPlatformTables.Contains(tableName);
    }

    public static bool TryGetPlatformByTable(string tableName, out CmfPlatformDefinition platform)
    {
        platform = null;
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return false;
        }

        return PlatformByTable.TryGetValue(tableName.Trim(), out platform);
    }

    public static Dictionary<string, string> GetPromptAliasToTableMap()
    {
        return new Dictionary<string, string>(PromptAliasToTable, StringComparer.OrdinalIgnoreCase);
    }

    public static Dictionary<string, string[]> GetGroupAliasToTablesMap()
    {
        Dictionary<string, string[]> clone = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string[]> entry in GroupAliasToTables)
        {
            string[] values = entry.Value ?? new string[0];
            string[] copied = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                copied[i] = values[i];
            }

            clone[entry.Key] = copied;
        }

        return clone;
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
