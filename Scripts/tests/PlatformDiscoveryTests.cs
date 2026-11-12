using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

public static class PlatformDiscoveryTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static HttpContext CreateContext(CmfPlatformDefinition[] platforms)
    {
        HttpContext context = new HttpContext(
            new HttpRequest("", "http://localhost/CMF_Web_portal.aspx", ""),
            new HttpResponse(new StringWriter()));
        context.Items["CMF_DiscoveredHsdPlatforms"] = platforms;
        return context;
    }

    public static void Main()
    {
        CmfPlatformDefinition[] rankedPlatforms = new[] {
            new CmfPlatformDefinition("Low", "low", "", "", null),
            new CmfPlatformDefinition("Zeta", "zeta", "", "", null),
            new CmfPlatformDefinition("Alpha", "alpha", "", "", null),
            new CmfPlatformDefinition("Total", "total", "", "", null),
            new CmfPlatformDefinition("Pending", "pending", "", "", null),
            new CmfPlatformDefinition("Implemented", "implemented", "", "", null),
            new CmfPlatformDefinition("Open", "open", "", "", null),
            new CmfPlatformDefinition("Missing", "missing", "", "", null)
        };
        Dictionary<string, CmfPlatformSummaryCounts> rankingCounts = new Dictionary<string, CmfPlatformSummaryCounts> {
            { "low", new CmfPlatformSummaryCounts(1, 100, 100, 1000) },
            { "zeta", new CmfPlatformSummaryCounts(4, 2, 7, 100) },
            { "alpha", new CmfPlatformSummaryCounts(4, 2, 7, 100) },
            { "total", new CmfPlatformSummaryCounts(4, 2, 7, 101) },
            { "pending", new CmfPlatformSummaryCounts(4, 2, 8, 6) },
            { "implemented", new CmfPlatformSummaryCounts(4, 3, 0, 7) },
            { "open", new CmfPlatformSummaryCounts(5, 0, 0, 5) }
        };
        List<CmfPlatformDefinition> ranked = CmfPlatformCatalog.OrderBySummaryCounts(rankedPlatforms, rankingCounts);
        string[] expected = { "open", "implemented", "pending", "total", "alpha", "zeta", "low", "missing" };
        for (int index = 0; index < expected.Length; index++)
        {
            Check(ranked[index].TableName == expected[index], "Descending Open, Implemented, Pending, Total ordering with alphabetical final ties.");
        }
        Check(rankedPlatforms[0].TableName == "low", "Ranking must not mutate discovery order or platform defaults.");
        CmfPlatformDefinition[] shuffledPlatforms = (CmfPlatformDefinition[])rankedPlatforms.Clone();
        Array.Reverse(shuffledPlatforms);
        List<CmfPlatformDefinition> shuffledRanking = CmfPlatformCatalog.OrderBySummaryCounts(shuffledPlatforms, rankingCounts);
        for (int index = 0; index < expected.Length; index++)
        {
            Check(shuffledRanking[index].TableName == expected[index], "Input order must not affect platform ranking.");
        }

        CmfPlatformDefinition knownPlatform = new CmfPlatformDefinition("NVL-H", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "", "", null);
        CmfPlatformDefinition unknownPlatform = new CmfPlatformDefinition("FUTURE-99", "CMF_FUTURE_99_ALL_COMPONENTS_TABLE", "", "", null);
        CmfPlatformDefinition[] changingPlatforms = new[] { knownPlatform, unknownPlatform };
        Dictionary<string, CmfPlatformSummaryCounts> changingCounts = new Dictionary<string, CmfPlatformSummaryCounts> {
            { knownPlatform.TableName, new CmfPlatformSummaryCounts(1, 0, 0, 1) },
            { unknownPlatform.TableName, new CmfPlatformSummaryCounts(2, 0, 0, 2) }
        };
        Check(CmfPlatformCatalog.OrderBySummaryCounts(changingPlatforms, changingCounts)[0] == unknownPlatform,
            "An unknown platform with higher counts must rank ahead of a predefined platform.");
        changingCounts[knownPlatform.TableName] = new CmfPlatformSummaryCounts(3, 0, 0, 3);
        Check(CmfPlatformCatalog.OrderBySummaryCounts(changingPlatforms, changingCounts)[0] == knownPlatform,
            "Changed counts must change platform order without modifying platform metadata.");
        Check(CmfPlatformCatalog.OrderBySummaryCounts(new CmfPlatformDefinition[0], rankingCounts).Count == 0, "Empty platform rankings must remain empty.");

        Check(CmfPlatformCatalog.IsDiscoverablePlatformTable("CMF_FUTURE_9_ALL_COMPONENTS_TABLE"), "Unknown platforms must be eligible without a catalog entry.");
        foreach (string invalid in new[] {
            "CMF_OVERALL_ALL_COMPONENTS_TABLE", "CMF_FUTURE_DUMMY_ALL_COMPONENTS_TABLE",
            "CMF_FUTURE_BACKUP_ALL_COMPONENTS_TABLE", "CMF_FUTURE_TEST_ALL_COMPONENTS_TABLE",
            "CMF_FUTURE_ALL_COMPONENTS_TABLE;DROP TABLE HSD", "dbo.CMF_FUTURE_ALL_COMPONENTS_TABLE",
            "CMF_FUTURE]ALL_COMPONENTS_TABLE", "CMF_FUTURE_ALL_COMPONENTS_TABLE\n", "", null })
        {
            Check(!CmfPlatformCatalog.IsDiscoverablePlatformTable(invalid), "Unsafe or non-production source accepted: " + invalid);
        }

        HttpContext original = HttpContext.Current;
        try
        {
            CmfPlatformDefinition future = new CmfPlatformDefinition("FUTURE-9", "CMF_FUTURE_9_ALL_COMPONENTS_TABLE", "", "", new[] { "future-9", "future 9" });
            HttpContext.Current = CreateContext(new[] { future });
            Check(CmfPlatformCatalog.GetPlatformCount() == 1, "Only discovered sources may be listed.");
            Check(CmfPlatformCatalog.GetDefaultPlatformTableName() == future.TableName, "Default must come from discovered HSD sources.");
            Check(CmfPlatformCatalog.GetAllPlatformTables()[0] == future.TableName, "Overall sources must include newly discovered platforms.");
            Check(CmfPlatformCatalog.IsAllowedPlatformTable(future.TableName.ToLowerInvariant()), "New sources must be selectable case-insensitively.");
            Check(!CmfPlatformCatalog.IsAllowedPlatformTable("CMF_NVL_U_ALL_COMPONENTS_TABLE"), "Metadata must not authorize an undiscovered source.");
            CmfPlatformDefinition found;
            Check(CmfPlatformCatalog.TryGetPlatformByTable(future.TableName, out found) && found.Label == future.Label, "New sources must resolve without predefined metadata.");
            Check(!CmfPlatformCatalog.TryGetPlatformByTable("CMF_PTL_ALL_COMPONENTS_TABLE", out found), "Absent platforms must not resolve through metadata.");
            Check(CmfPlatformCatalog.GetPromptAliasToTableMap()["future 9"] == future.TableName, "Reports must recognize discovered aliases.");
            Check(CmfPlatformCatalog.GetGroupAliasToTablesMap()["future"][0] == future.TableName, "Family groups must include new platforms.");
            CmfPlatformDefinition[] snapshot = CmfPlatformCatalog.GetOrderedPlatforms();
            snapshot[0] = null;
            Check(CmfPlatformCatalog.GetOrderedPlatforms()[0] != null, "Caller changes must not corrupt request discovery.");

            HttpContext.Current = CreateContext(new CmfPlatformDefinition[0]);
            Check(CmfPlatformCatalog.GetPlatformCount() == 0, "An empty request must not reuse a previous request's sources.");
            Check(CmfPlatformCatalog.GetDefaultPlatformTableName() == string.Empty, "Empty discovery must not invent a default platform.");
            Check(CmfPlatformCatalog.GetAllowedPlatformTables().Count == 0, "Empty discovery must not fall back to metadata.");
            Check(CmfPlatformCatalog.GetGroupAliasToTablesMap().Count == 0, "Empty discovery must not invent family groups.");
        }
        finally
        {
            HttpContext.Current = original;
        }
        Console.WriteLine("HSD platform discovery checks passed.");
    }
}