using System;
using System.Data.SqlClient;

public static class PlatformMembershipTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        string h = "CMF_NVL_H_ALL_COMPONENTS_TABLE";
        string hx = "CMF_NVL_Hx_ALL_COMPONENTS_TABLE";
        string query = "SELECT A.cp_id FROM [dbo].[" + h + "] AS A JOIN " + hx
            + " B ON A.cp_id = B.cp_id WHERE A.title = 'FROM " + hx + "' -- " + h;
        string scoped = CmfPlatformCatalog.ScopeIssueSql(query);
        Check(scoped.Contains("CHARINDEX(',novalake_h,'") && scoped.Contains("CHARINDEX(',novalake_hx,'"), "Exact H and HX tokens are scoped separately.");
        Check(scoped.Contains("FROM [cmf_membership_0] AS A JOIN [cmf_membership_1] B"), "Existing table aliases and qualified sources are preserved.");
        Check(scoped.Contains("'FROM " + hx + "' -- " + h), "String literals and comments are not rewritten.");
        Check(CmfPlatformCatalog.ScopeIssueSql(scoped) == scoped, "Scoping is idempotent.");
        string cte = CmfPlatformCatalog.ScopeIssueSql(";WITH source AS (SELECT * FROM " + h + ") SELECT * FROM source");
        Check(cte.Contains(", source AS (SELECT * FROM [cmf_membership_0])"), "Existing CTEs are composed without a nested WITH.");
        string update = "UPDATE " + h + " SET title = @title WHERE cp_id = @id";
        Check(CmfPlatformCatalog.ScopeIssueSql(update) == update, "Writes are not changed.");
        string cteUpdate = ";WITH source AS (SELECT * FROM " + h + ") UPDATE source SET title = @title";
        Check(CmfPlatformCatalog.ScopeIssueSql(cteUpdate) == cteUpdate, "CTE-based writes are not changed.");
        string pending = CmfPlatformCatalog.ScopeIssueSql("SELECT * FROM CMF_NVL_H_CMF_ASK");
        Check(pending.Contains("pending.cp_id") && pending.Contains("CHARINDEX(',novalake_h,'")
            && pending.Contains("NOT EXISTS"), "Pending uses known issue membership and preserves records without processor evidence.");
        Check(CmfPlatformCatalog.GetProcessorTokens("CMF_ARL_Refresh_ALL_COMPONENTS_TABLE")[0] == "arrowlake_hx_816_refresh", "Refresh has a distinct processor token.");
        Console.WriteLine("Platform membership SQL composition fixtures passed.");
    }

    public static void CheckSql(SqlConnection connection)
    {
        string[] tables = { "CMF_NVL_H_ALL_COMPONENTS_TABLE", "CMF_NVL_Hx_ALL_COMPONENTS_TABLE",
            "CMF_NVL_S_ALL_COMPONENTS_TABLE", "CMF_NVL_U_ALL_COMPONENTS_TABLE",
            "CMF_ARL_H_ALL_COMPONENTS_TABLE", "CMF_ARL_HX_ALL_COMPONENTS_TABLE", "CMF_ARL_Refresh_ALL_COMPONENTS_TABLE",
            "CMF_ARL_S_ALL_COMPONENTS_TABLE", "CMF_ARL_U_ALL_COMPONENTS_TABLE",
            "CMF_PTL_ALL_COMPONENTS_TABLE", "CMF_LNL_ALL_COMPONENTS_TABLE", "CMF_GNR_ALL_COMPONENTS_TABLE", "CMF_WCL_ALL_COMPONENTS_TABLE" };
        foreach (string table in tables)
        {
            foreach (string token in CmfPlatformCatalog.GetProcessorTokens(table))
            {
                string sql = "SELECT COUNT(*) FROM (VALUES ('" + token + "',1),(' " + token.ToUpperInvariant()
                    + " , unassigned ',1),('unassigned',1),('',1),(NULL,1),('" + token
                    + "_unrelated',0),('different_platform',0)) fixture(processor,expected) WHERE CASE WHEN "
                    + CmfPlatformCatalog.BuildProcessorMembershipPredicate(table, "fixture") + " THEN 1 ELSE 0 END <> expected";
                using (SqlCommand command = new SqlCommand(sql, connection))
                    Check(Convert.ToInt32(command.ExecuteScalar()) == 0, "Processor-token boundary failed for " + table + ": " + token);
            }
        }
        using (SqlCommand command = new SqlCommand("SELECT CASE WHEN "
            + CmfPlatformCatalog.BuildProcessorMembershipPredicate(tables[0], "fixture")
            + " THEN 1 ELSE 0 END FROM (VALUES ('novalake_hx')) fixture(processor)", connection))
            Check(Convert.ToInt32(command.ExecuteScalar()) == 0, "HX-only records must never match H.");
        foreach (string table in new[] { tables[0], tables[1] })
            using (SqlCommand command = new SqlCommand("SELECT CASE WHEN "
                + CmfPlatformCatalog.BuildProcessorMembershipPredicate(table, "fixture")
                + " THEN 1 ELSE 0 END FROM (VALUES ('novalake_h,novalake_hx')) fixture(processor)", connection))
                Check(Convert.ToInt32(command.ExecuteScalar()) == 1, "Explicitly shared records retain each tagged membership.");
        string pending = CmfPlatformCatalog.ScopeIssueSql("SELECT COUNT(*) FROM CMF_NVL_H_CMF_ASK")
            .Replace("[dbo].[CMF_NVL_H_CMF_ASK]", "(SELECT cp_id FROM (VALUES ('1'),('2'),('3'),('4'),('5')) fixture(cp_id))")
            .Replace("[dbo].[CMF_NVL_H_ALL_COMPONENTS_TABLE]", "(SELECT cp_id,processor FROM (VALUES ('1','novalake_hx'),('2','novalake_h'),('4','unassigned'),('5','novalake_h,novalake_hx')) fixture(cp_id,processor))");
        using (SqlCommand command = new SqlCommand(pending, connection))
            Check(Convert.ToInt32(command.ExecuteScalar()) == 4, "Pending excludes known HX-only records, retains H, multi-tag, unassigned and pending-only records.");
        string h = "CMF_NVL_H_ALL_COMPONENTS_TABLE";
        string hx = "CMF_NVL_Hx_ALL_COMPONENTS_TABLE";
        using (SqlCommand command = CmfPlatformCatalog.CreateReadCommand("SELECT TOP (0) [dbo].[" + h
            + "].cp_id FROM [dbo].[" + h + "] JOIN " + hx + " AS other ON other.cp_id = [dbo].[" + h + "].cp_id WHERE other.title = @title", connection))
        {
            command.Parameters.AddWithValue("@title", "' FROM " + h);
            using (SqlDataReader reader = command.ExecuteReader()) Check(!reader.Read(), "Qualified joins remain executable and parameterized.");
        }
        using (SqlCommand command = CmfPlatformCatalog.CreateReadCommand(";WITH source AS (SELECT cp_id FROM " + h
            + " UNION ALL SELECT cp_id FROM " + hx + ") SELECT COUNT(*) FROM source", connection))
            Check(Convert.ToInt32(command.ExecuteScalar()) >= 0, "Existing CTE and UNION composition executes against real source tables.");
        Console.WriteLine("Platform membership SQL behavior fixtures passed.");
    }
}