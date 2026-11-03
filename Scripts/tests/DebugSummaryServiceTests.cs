using System;
using System.Collections;
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
            "- One.\n- Two.\n- Three.\n- Four.\n- Five.",
            "- " + string.Join(" ", new string[101]).Replace(" ", "word ") + "word"
        })
        {
            Check(!(bool)Invoke("TryNormalizeDebugSummary", new object[] { invalid, null }),
                "Non-bullet or sectioned output was accepted: " + invalid);
        }
        Check((bool)Invoke("TryNormalizeDebugSummary", new object[] {
            "- " + string.Join(" ", new string[100]).Replace(" ", "word ") + "word", null
        }), "A complete 100-word update was rejected.");

        string prompt = (string)Invoke("BuildPrompt", "123", "", "Screen hang", "open", "category",
            "Investigation History: Driver comparison did not resolve the hang.");
        Check(prompt.Contains("Driver comparison did not resolve the hang."), "Source evidence was omitted.");
        Check(prompt.Contains("debugging already performed"), "The prompt lost its debugging focus.");
        Check(prompt.Contains("No debugging activity is documented"), "Missing-evidence guidance was omitted.");
        Check(prompt.Contains("suspected causes from confirmed causes"), "Uncertainty guidance was omitted.");
        Check(prompt.Contains("Use source-coverage and retrieval notes only to constrain factual claims internally"),
            "Source limitations no longer constrain factual claims.");
        Check(prompt.Contains("Do not include source-coverage disclaimers")
            && !prompt.Contains("explicitly state that the summary is limited")
            && !prompt.Contains("Paraphrase evidence limitations"),
            "The summary still requests source-coverage commentary.");
        Check(prompt.Contains("without claiming it is the latest"),
            "Undated activity can be presented as the latest.");
        Check(prompt.Contains("layman can easily comprehend") && prompt.Contains("explain their meaning or practical purpose in plain words"),
            "Layman-friendly guidance or technical term explanations were omitted.");
        Check(prompt.Contains("most recent debugging") && prompt.Contains("no more than 100 words"),
            "The update is not recent and brief.");
        Check(prompt.Contains("existing sysdebug_forum field is excluded"), "Forum exclusion was omitted.");
        Check(!prompt.Contains("Sysdebug: category"), "The debug category leaked into the evidence prompt.");
        Check(!prompt.Contains("**Risk Assessment**") && !prompt.Contains("**Decision Impact**"),
            "The old five-section template remains.");

        string longEvidence = new string('x', 3500) + " Final test did not resolve the hang.";
        HsdArticleData article = new HsdArticleData {
            FetchSuccess = true, ArticleId = "123", UpdatedDate = "2026-10-28",
            SysdebugForum = "FORUM_ONLY_SENTINEL", FixDescription = longEvidence,
            InvestigationHistory = "Cause not confirmed.",
            Comments = new List<string> { "Verification remains pending." }
        };
        string context = HsdPortalService.FormatForDebugSummary(article, "Sighting 123");
        Check(!context.Contains("FORUM_ONLY_SENTINEL") && !context.Contains("Sysdebug Forum:"),
            "The existing forum was used as debug evidence.");
        Check(context.Contains("Fix Description: " + longEvidence), "Fix evidence was truncated.");
        Check(context.Contains("Cause not confirmed.") && context.Contains("Verification remains pending."),
            "Investigation evidence was omitted.");
        Check(context.Contains("Last Updated: 2026-10-28") && context.Contains("history may be partial"),
            "Source freshness or limitations were omitted.");
        Check(HsdPortalService.FormatForDebugSummary(new HsdArticleData(), "Missing") == "",
            "An unavailable article was treated as evidence.");
        Check(HsdPortalService.FormatForAiContext(article, "Other feature").Contains("FORUM_ONLY_SENTINEL"),
            "The shared formatter lost forum evidence for other features.");

        string debugRetrieval = (string)typeof(HsdPortalService).GetMethod("BuildDebugHsdPrompt", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { "123" });
        Check(debugRetrieval.Contains("newest five") && debugRetrieval.Contains("Do not retrieve, copy, paraphrase"),
            "The dedicated retrieval prompt does not isolate recent non-forum evidence.");

        var parsed = new HsdArticleData { Comments = new List<string>() };
        var comments = new ArrayList {
            new Hashtable { {"text", "Older comparison."}, {"updated_date", "2026-10-25T10:00:00Z"} },
            new Hashtable { {"text", "2020-01-01 is the build date; the latest test still hangs."}, {"updated_date", "2026-11-03T10:00:00Z"} },
            new Hashtable { {"text", "New driver did not reproduce the hang."}, {"updated_date", "2026-11-02T10:00:00Z"} },
            new Hashtable { {"text", "FORUM_CHANGE_SENTINEL"}, {"field", "client_platf.bug.sysdebug_forum"}, {"updated_date", "2026-11-04"} }
        };
        var mergeComments = typeof(HsdPortalService).GetMethod("MergeDebugComments", BindingFlags.NonPublic | BindingFlags.Static);
        mergeComments.Invoke(null, new object[] { comments, parsed });
        Check(parsed.Comments.Count == 3 && parsed.Comments[0].StartsWith("[2026-11-03T10:00:00Z]"),
            "Comments were not ordered by their own timestamps.");
        Check(parsed.Comments[1].Contains("New driver"), "Recent results were superseded by older evidence.");
        Check(!string.Join("\n", parsed.Comments).Contains("FORUM_CHANGE_SENTINEL"), "Forum change records were used.");
        mergeComments.Invoke(null, new object[] { new ArrayList { "An undated comment mentions 2026-11-05 in its body." }, parsed });
        Check(parsed.DebugEvidenceLimitation.Contains("undated"), "Undated activity was assigned an invented chronology.");
        var many = new ArrayList();
        for (int day = 1; day <= 8; day++) many.Add(new Hashtable {
            {"text", new string('x', 700) + " Day " + day},
            {"updated_date", "2026-10-" + day.ToString("00") + "T10:00:00Z"}
        });
        var recent = new HsdArticleData { Comments = new List<string>() };
        mergeComments.Invoke(null, new object[] { many, recent });
        Check(recent.Comments.Count == 5 && recent.Comments[0].EndsWith("Day 8") && recent.Comments[4].EndsWith("Day 4"),
            "The newest five entries were not retained.");
        Check(recent.Comments[0].Length > 700, "Debug comments were truncated to the shared 600-character limit.");
        var shared = new HsdArticleData { Comments = new List<string>() };
        typeof(HsdPortalService).GetMethod("ParseInlineComments", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { new Hashtable { {"comments", many} }, shared, false });
        Check(shared.Comments.Count == 5 && shared.Comments[0] == new string('x', 600) + "... [truncated]",
            "The shared comment parser changed behavior for other features.");

        string followUpPrompt = (string)Invoke("BuildFollowUpPrompt", "123", "", "Hang", "open", "FORUM_PARAMETER_SENTINEL",
            context, "- Previous recent finding.", "What was tried?", true);
        Check(!followUpPrompt.Contains("FORUM_PARAMETER_SENTINEL") && followUpPrompt.Contains("Never use the existing sysdebug_forum"),
            "Follow-up chat reused excluded forum information.");
        Check(followUpPrompt.Contains("Do not add unsolicited source-coverage disclaimers")
            && !followUpPrompt.Contains("acknowledge retrieval limitations"),
            "Debug follow-ups still append source-coverage commentary.");

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
        AiSummaryResponse unavailableFollowUp = AiSummaryService.GenerateFollowUpResponse(new AiSummaryRequest {
            IssueId = "123", Title = "Screen hang"
        }, "What was tried?", true);
        Check(!unavailableFollowUp.Success && !unavailableFollowUp.UsedFallback && unavailableFollowUp.Message.Contains("Please retry"),
            "Debug follow-up failure produced a heuristic answer.");
        Console.WriteLine("PASS: Debug summary formatting, prompt, evidence scope, and retrieval-limit tests.");
    }
}
