using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

public static class DebugSummaryServiceTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static object Invoke(string method, params object[] arguments)
    {
        return typeof(AiSummaryService).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, arguments);
    }

    public static void Main()
    {
        string bullets = "- Engineers reproduced the screen hang with seven applications open.\n"
            + "- The comparison did not resolve the hang; the cause remains unconfirmed.";
        object[] valid = { "\r\n" + bullets.Replace("\n", "\r\n") + "\r\n", null };
        Check((bool)Invoke("TryNormalizeDebugSummary", valid), "Valid bullets were rejected.");
        Check((string)valid[1] == bullets, "Evidence or complete sentences were changed.");
        foreach (string invalid in new[] {
            "", "A paragraph.", "**Issue Summary**\n- Test result.",
            "- **Risk Assessment**: High.", "- # Heading", "- ",
            string.Join("\n", new string[11]).Replace("\n", "- Test result.\n") + "- Test result."
        })
        {
            Check(!(bool)Invoke("TryNormalizeDebugSummary", new object[] { invalid, null }),
                "Non-bullet or sectioned output was accepted: " + invalid);
        }

        string prompt = (string)Invoke("BuildPrompt", "123", "", "Screen hang", "open", "category",
            "Investigation History: Driver comparison did not resolve the hang.");
        Check(prompt.Contains("Driver comparison did not resolve the hang."), "Source evidence was omitted.");
        Check(prompt.Contains("debugging already performed"), "The prompt lost its debugging focus.");
        Check(prompt.Contains("No debugging activity is documented"), "Missing-evidence guidance was omitted.");
        Check(prompt.Contains("suspected causes from confirmed causes"), "Uncertainty guidance was omitted.");
        Check(prompt.Contains("HSD retrieval limitation"), "Partial-source guidance was omitted.");
        Check(!prompt.Contains("**Risk Assessment**") && !prompt.Contains("**Decision Impact**"),
            "The old five-section template remains.");

        string longEvidence = new string('x', 3500) + " Final test did not resolve the hang.";
        HsdArticleData article = new HsdArticleData {
            FetchSuccess = true, ArticleId = "123", UpdatedDate = "2026-10-28",
            SysdebugForum = longEvidence, FixDescription = longEvidence,
            InvestigationHistory = "Cause not confirmed.",
            Comments = new List<string> { "Verification remains pending." }
        };
        string context = HsdPortalService.FormatForDebugSummary(article, "Sighting 123");
        Check(context.Contains("Sysdebug Forum: " + longEvidence), "Debug forum evidence was truncated.");
        Check(context.Contains("Fix Description: " + longEvidence), "Fix evidence was truncated.");
        Check(context.Contains("Cause not confirmed.") && context.Contains("Verification remains pending."),
            "Investigation evidence was omitted.");
        Check(context.Contains("Last Updated: 2026-10-28") && context.Contains("history may be partial"),
            "Source freshness or limitations were omitted.");
        Check(HsdPortalService.FormatForDebugSummary(new HsdArticleData(), "Missing") == "",
            "An unavailable article was treated as evidence.");
        Check(HsdPortalService.FormatForAiContext(article, "Other feature").Contains("[truncated]"),
            "The shared formatter changed behavior for other features.");

        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings.Add("HSD:Enabled", "false");
        config.AppSettings.Settings.Add("GNAI:Endpoint", "REPLACE_TEST_ENDPOINT");
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
        string limited = (string)Invoke("EnrichContextWithHsdArticles", "123", "Promoted ID: 456", true);
        Check(limited.Contains("HSD retrieval limitation: The sighting")
            && limited.Contains("Promoted article 456"), "Fetch failures were silently ignored.");
        Check(AiSummaryService.GenerateIssueSummary(null).Success == false, "Invalid requests succeeded.");
        AiSummaryResponse unavailable = AiSummaryService.GenerateIssueSummary(new AiSummaryRequest {
            IssueId = "123", Title = "Screen hang", ContextDetails = "Promoted ID: 456"
        });
        Check(!unavailable.Success && !unavailable.UsedFallback && string.IsNullOrEmpty(unavailable.Summary)
            && unavailable.Message.Contains("Please retry"), "AI failure produced a heuristic summary.");
        Console.WriteLine("PASS: Debug summary formatting, prompt, evidence scope, and retrieval-limit tests.");
    }
}
