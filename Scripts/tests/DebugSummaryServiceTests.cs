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
            "- One.\n- Two.\n- Three.\n- Four.\n- Five.\n- Six.",
            "- " + string.Join(" ", new string[151]).Replace(" ", "word ") + "word"
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
        Check(prompt.Contains("most recent debugging") && prompt.Contains("debug suggestions"),
            "The update does not include debug suggestions.");
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
        Check(followUpPrompt.Contains("strictly answer questions only related to this particular selected sighting"),
            "Sighting scope restriction missing from follow-up prompt.");
        Check(followUpPrompt.Contains("Never combine, merge, or modify first and last names"),
            "Name accuracy instruction missing from follow-up prompt.");

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

        MethodInfo snapMethod = typeof(CmfRecommendationService).GetMethod("FormatSnappedFivePointScore", BindingFlags.NonPublic | BindingFlags.Static);
        Check((string)snapMethod.Invoke(null, new object[] { 86 }) == "3.25", "Score 86/100 (3.44/4) must stay in CMF_REVIEW band (3.25/4, below 3.50).");
        Check((string)snapMethod.Invoke(null, new object[] { 88 }) == "3.5", "Score 88/100 (3.52/4) must snap to 3.5 in CMF_OK band.");
        Check((string)snapMethod.Invoke(null, new object[] { 65 }) == "2.5", "Score 65/100 (2.60/4) must snap to 2.5 in CMF_REJECT band.");

        MethodInfo humanizeMethod = typeof(CmfRecommendationService).GetMethod("HumanizeOccurrenceCategory", BindingFlags.NonPublic | BindingFlags.Static);
        Check((string)humanizeMethod.Invoke(null, new object[] { "very_sporadic_<10%" }) == "Rare (<10% of tries)", "Raw reproducibility enum was not humanized.");

        MethodInfo normNarrativeMethod = typeof(CmfRecommendationService).GetMethod("NormalizeCmfRecommendationNarrative", BindingFlags.NonPublic | BindingFlags.Static);
        string cleanedNarrative = (string)normNarrativeMethod.Invoke(null, new object[] { "- **AI Recommendation (*CMF_REVIEW*):** Keep in *CMF_REVIEW* while testing patch." });
        Check(cleanedNarrative == "**AI Recommendation:** Keep in **CMF_REVIEW** while testing patch.", "AI Recommendation label or stray single-asterisk around CMF_REVIEW was not normalized: " + cleanedNarrative);

        MethodInfo dupMethod = typeof(CmfRecommendationService).GetMethod("HasDuplicateIssueEvidence", BindingFlags.NonPublic | BindingFlags.Static);
        object[] dupArgs = new object[] { "14028813189", "Hang issue", "cmf_ask", "Blocks test", "Discussion Comments:\n[1] Marked as duplicate of 14028999999", null };
        Check((bool)dupMethod.Invoke(null, dupArgs) && ((string)dupArgs[5]).Contains("14028999999"), "Duplicate issue reference in ticket was not detected.");

        string cleanedImpact = (string)Invoke("CleanupSightingImpactSummary", "- **Validation & customer blockage:** Customer FVT sign-off is blocked by the system hang.\n- Issue is rarer or 1/12 prototypes.\n- **Real-world & program risk:** End users could lose unsaved work if the laptop freezes during normal use.");
        Check(!cleanedImpact.Contains("prototypes") && !cleanedImpact.Contains("rarer"), "Occurrence/prototype-count line was not stripped from Impact Summary: " + cleanedImpact);

        string hyphenImpact = (string)Invoke("CleanupSightingImpactSummary", "- **Validation & customer blockage:** Lenovo's HLK FVT System - PNP (disable/enable) with IO Before/After reliability test is blocked by BSOD 0x50 and is treated as a showstopper.\n- **Real-world & program risk:** End users could hit a blue-screen crash during device reconnect.");
        Check(hyphenImpact.Contains("System - PNP (disable/enable)"), "Hyphen inside 'System - PNP' was split into a separate bullet: " + hyphenImpact);

        string cleanedDesc = (string)Invoke("CleanupSightingDescription", "Running System - PNP test causes a 0x50 BSOD.\n\n- **Root cause & lab repro:** Memory dumps show a use-after-free in IntcSSDW.sys (IntcSSDW.sys; Bugcheck 0x50; freed obj ffff908426dd0cd0).\n- **Current fix status:** Patch pending.", "Title", "");
        Check(cleanedDesc.Contains("IntcSSDW.sys") && !cleanedDesc.Contains("ffff908426dd0cd0"), "Hex memory pointer was not stripped from Problem Description: " + cleanedDesc);
        Check(cleanedDesc.Contains("**Repro details:**") && !cleanedDesc.Contains("Root cause & lab repro"), "Repro Details label was not normalized: " + cleanedDesc);
        Check(!cleanedDesc.Contains("Current fix status") && !cleanedDesc.Contains("Patch pending"), "Fix status bullet was not removed from Problem Description: " + cleanedDesc);
        Check(cleanedDesc.Contains("Recovery not documented"), "Missing recovery was not defaulted to 'Recovery not documented': " + cleanedDesc);

        string descPrompt = (string)Invoke("BuildSightingDescriptionPrompt", "14028965450", "Flicker issue", "Context");
        Check(descPrompt.Contains("**Repro details:**") && descPrompt.Contains("reproduction details") && descPrompt.Contains("Do NOT repeat the problem statement and do NOT include debug analysis") && !descPrompt.Contains("Root cause & lab repro") && !descPrompt.Contains("**Current fix status:**"), "Description prompt did not update to strictly reproduction details only.");

        // Verify Repro details strips debug analysis / instruction-fetch / root-cause sentences (Sighting 14028740117 screenshot)
        string strippedReproDesc = (string)Invoke("CleanupSightingDescription",
            "[Platform] WinPVT-MeatGrinder+Direct3D+AC/DC cycling triggers 0xA BSOD (IRQL_NOT_LESS_OR_EQUAL) on HP NVL-H Graham units.\n\n" +
            "- **Frequency & recovery:** Fails 3-5 of 15 units per test cycle; recovery not documented.\n" +
            "- **Repro details:** RVP repro: No; customer units reproduce on pure OS 26H1 builds 28020.2731 and 28020.2818 but not on earlier or later builds. Debug analysis indicates P-core instruction-fetch anomaly where a frequently-jumped-to compare-and-branch instruction (cmp rbx,rdi in nt!MiFindSuitableLookasideReplacement) spans two 64-byte cache lines only in failing builds, suggesting CPU front-end defect rather than memory corruption (in promoted issue 15020064833).",
            "0xA BSOD", "Component: Platform\nRVP Platform Debug Details: No");
        Check(strippedReproDesc.Contains("customer units reproduce on pure OS 26H1 builds") && !strippedReproDesc.Contains("Debug analysis indicates") && !strippedReproDesc.Contains("instruction-fetch anomaly"), "Debug analysis sentence was not stripped from Repro details: " + strippedReproDesc);

        string conflictDesc = (string)Invoke("CleanupSightingDescription",
            "Plugging/unplugging the power adapter while FurMark runs causes a full-screen flicker.\n\n" +
            "- **What happens & recovery:** Whole-screen flicker on adapter plug/unplug during FurMark; customer saw frequent failures (~50-90%); closing FurMark stops the issue.\n" +
            "- **Root cause & lab repro:** Reproduced on the Lenovo Y712P customer unit but not on RVP; a display Pipe A underrun was observed.\n" +
            "- **Current fix status:** ETL collection and RVP validation are pending.",
            "Flicker", "");
        Check(!conflictDesc.Contains("Whole-screen flicker on adapter plug/unplug"), "Redundant opening symptom clause was not stripped from first bullet: " + conflictDesc);
        Check(conflictDesc.Contains("Status unclear") && !conflictDesc.Contains("but not on RVP"), "Conflicting RVP repro statement was not reconciled: " + conflictDesc);

        string fixPrompt = (string)Invoke("BuildSightingFixStatusPrompt", "14028965450", "Flicker issue", "Context");
        Check(fixPrompt.Contains("**Fix & workaround:**") && fixPrompt.Contains("**Validation status:**") && fixPrompt.Contains("Promoted Issue"), "Fix status prompt is missing expected labels or promoted issue attribution.");
        string cleanedFix = (string)Invoke("CleanupSightingFixStatus", "- **Fix & workaround:** Disabling FeatureTestControl suppresses the flicker.\n- **Validation status:** ETL collection and RVP validation are pending.", "");
        Check(cleanedFix.Contains("**Fix & workaround:**") && cleanedFix.Contains("**Validation status:**"), "Fix status cleanup failed: " + cleanedFix);

        string qiraDesc = (string)Invoke("CleanupSightingDescription",
            "Saying \"hey Qira\" sometimes fails to wake the Qira app.\n\n" +
            "- **Frequency & recovery:** Happens about 2 of 7 cycles; status unclear on recovery - one note says reboot unit, other notes list Recovery not documented.\n" +
            "- **Repro details:** Reproduced on customer UUTs.",
            "[Client][Lenovo][Huaqin][Flex_7i][NVL][Win11_26H1][FVT] Say \"hey Qira\", there're two cases that Qira app may not be waken up.(FR:2/2UUTs,2/7Cycles).",
            "Problem Description: 7. Frequency of Occurrence: 1/1UUTs,2/3Cycles");
        Check(qiraDesc.Contains("2/2UUTs,2/7Cycles") && qiraDesc.Contains("1/1UUTs,2/3Cycles"), "Conflicting UUT/cycle counts between title and description were not recorded: " + qiraDesc);
        Check(qiraDesc.Contains("recovery requires reboot unit") && !qiraDesc.Contains("Recovery not documented"), "Documented reboot recovery was contradicted by Recovery not documented: " + qiraDesc);

        MethodInfo normRuleMethod = typeof(CmfRecommendationService).GetMethod("NormalizeModelMetricEvaluation", BindingFlags.NonPublic | BindingFlags.Static);
        CmfRecommendationRuleScore rRule = new CmfRecommendationRuleScore {
            RuleId = "R", RuleName = "Recovery", Score = "3",
            Evaluation = "Contributing factor: Requires driver update/retest. Evidence: Recovery NA and please retest with ISST WZS 45.1.0.402."
        };
        normRuleMethod.Invoke(null, new object[] { rRule, "Recovery Mechanism: reboot unit" });
        Check(!rRule.Evaluation.Contains("driver update/retest") && rRule.Evaluation.Contains("Requires system reboot"), "Software update/retest was not replaced with runtime recovery on R metric: " + rRule.Evaluation);

        MethodInfo sqaFallbackMethod = typeof(CmfRecommendationService).GetMethod("BuildFallbackSightingQualityAssessment", BindingFlags.NonPublic | BindingFlags.Static);
        SightingQualityAssessmentResponse sqaResp = (SightingQualityAssessmentResponse)sqaFallbackMethod.Invoke(null, new object[] {
            "14028995309",
            "[Client][Lenovo] Qira issue (FR:2/2UUTs,2/7Cycles)",
            "audio", "Blocks Qira test", "", "no", "frequently_50%-90%", "lenovo", "owner",
            "Problem Description: 7. Frequency of Occurrence 1/1UUTs,2/3Cycles"
        });
        SightingQualityFieldAssessment occField = sqaResp.Fields.Find(f => f.FieldName == "Reproducibility / Failure Rate");
        Check(occField != null && occField.Status == "Conflicting" && occField.Evidence.Contains("2/2UUTs,2/7Cycles") && occField.Evidence.Contains("1/1UUTs,2/3Cycles"), "SQA did not flag conflicting UUT/cycle counts between title and description.");

        // Verify unified JSON configuration service & Anthropic/OpenAI endpoint routing
        CmfAssistantUnifiedConfig unifiedCfg = CmfAssistantConfigService.GetConfig();
        Check(unifiedCfg != null && unifiedCfg.ScaleMax == 4 && unifiedCfg.CmfOkMinScore == 3.50 && unifiedCfg.CmfReviewMinScore == 2.75 && !string.IsNullOrEmpty(unifiedCfg.CompiledRulesText), "Unified JSON configuration failed to load 4-point scoring rules.");
        Check(CmfAssistantConfigService.ResolveGnaiEndpointForModel("https://gnai.intel.com/api/providers/openai/v1/chat/completions", "claude-4-5-sonnet") == "https://gnai.intel.com/api/providers/anthropic/v1/messages", "Claude Sonnet did not route to GNAI Anthropic endpoint.");
        Check(CmfAssistantConfigService.ResolveGnaiEndpointForModel("https://gnai.intel.com/api/providers/openai/v1/chat/completions", "claude-4-5-haiku") == "https://gnai.intel.com/api/providers/anthropic/v1/messages", "Claude Haiku did not route to GNAI Anthropic endpoint.");
        Check(CmfAssistantConfigService.ResolveGnaiEndpointForModel("https://gnai.intel.com/api/providers/anthropic/v1/messages", "gpt-5-mini") == "https://gnai.intel.com/api/providers/openai/v1/chat/completions", "gpt-5-mini did not route to GNAI OpenAI endpoint.");

        // Verify 4-Point Gating Guardrails (B<=2 or D<=2 cap at CMF_REVIEW, and B=4/CX=4/D=4 High Normal-Flow UX Exception as CMF_OK)
        MethodInfo resolveDispMethod = typeof(CmfRecommendationService).GetMethod("ResolveCmfDisposition", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string), typeof(int), typeof(int), typeof(List<CmfRecommendationRuleScore>), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) }, null);
        List<CmfRecommendationRuleScore> uxExceptionRules = new List<CmfRecommendationRuleScore> {
            new CmfRecommendationRuleScore { RuleId = "S", RuleName = "Severity", Score = "3", Evaluation = "Contributing factor: Persistent display flicker. Evidence: flicker" },
            new CmfRecommendationRuleScore { RuleId = "O", RuleName = "Occurrence", Score = "4", Evaluation = "Contributing factor: Every time (100% repro). Evidence: 100%" },
            new CmfRecommendationRuleScore { RuleId = "D", RuleName = "Detection", Score = "4", Evaluation = "Contributing factor: Triggered on boot to desktop. Evidence: desktop" },
            new CmfRecommendationRuleScore { RuleId = "R", RuleName = "Recovery", Score = "4", Evaluation = "Contributing factor: Requires system reboot. Evidence: reboot" },
            new CmfRecommendationRuleScore { RuleId = "B", RuleName = "Business Impact", Score = "4", Evaluation = "Contributing factor: Blocks customer FVT validation. Evidence: FVT" },
            new CmfRecommendationRuleScore { RuleId = "CX", RuleName = "Customer Experience", Score = "4", Evaluation = "Contributing factor: Core display unusable. Evidence: display" }
        };
        string uxDisp = (string)resolveDispMethod.Invoke(null, new object[] { "CMF_OK", 95, 88, uxExceptionRules, "111", "Display flicker", "cmf_ask", "Blocks FVT", "" });
        Check(uxDisp == "CMF_OK", "High normal-flow UX exception (S=3, O=4, D=4, R=4, B=4, CX=4 -> 3.80/4) did not qualify as CMF_OK: " + uxDisp);

        List<CmfRecommendationRuleScore> guardrailCapRules = new List<CmfRecommendationRuleScore> {
            new CmfRecommendationRuleScore { RuleId = "S", RuleName = "Severity", Score = "4", Evaluation = "Contributing factor: System blue-screen crash. Evidence: BSOD" },
            new CmfRecommendationRuleScore { RuleId = "O", RuleName = "Occurrence", Score = "4", Evaluation = "Contributing factor: Every time (100% repro). Evidence: 100%" },
            new CmfRecommendationRuleScore { RuleId = "D", RuleName = "Detection", Score = "2", Evaluation = "Contributing factor: Single app button click. Evidence: button" },
            new CmfRecommendationRuleScore { RuleId = "R", RuleName = "Recovery", Score = "4", Evaluation = "Contributing factor: Requires system reboot. Evidence: reboot" },
            new CmfRecommendationRuleScore { RuleId = "B", RuleName = "Business Impact", Score = "2", Evaluation = "Contributing factor: Low business exposure. Evidence: isolated" },
            new CmfRecommendationRuleScore { RuleId = "CX", RuleName = "Customer Experience", Score = "4", Evaluation = "Contributing factor: Laptop becomes unusable. Evidence: BSOD" }
        };
        string capDisp = (string)resolveDispMethod.Invoke(null, new object[] { "CMF_OK", 90, 88, guardrailCapRules, "222", "Single app BSOD", "cmf_ask", "Isolated", "" });
        Check(capDisp == "CMF_REVIEW", "Low business or narrow trigger guardrail (B=2, D=2) did not cap disposition at CMF_REVIEW: " + capDisp);

        // Verify 5-Tag Model: CMF_DUPLICATE for duplicate ticket and CMF_INCOMPLETE when core metrics are missing (Guardrail #3)
        string dupDisp = (string)resolveDispMethod.Invoke(null, new object[] { "CMF_OK", 90, 88, uxExceptionRules, "14028813189", "Hang issue", "cmf_ask", "Blocks test", "Discussion Comments:\n[1] Marked as duplicate of 14028999999" });
        Check(dupDisp == "CMF_DUPLICATE", "Duplicate ticket mention should resolve to CMF_DUPLICATE in 5-Tag Model: " + dupDisp);

        MethodInfo singlePassMethod = typeof(CmfRecommendationService).GetMethod("BuildSinglePassAlignedReasoning", BindingFlags.NonPublic | BindingFlags.Static);
        string dupReasoning = (string)singlePassMethod.Invoke(null, new object[] {
            "CMF_DUPLICATE", "CMF_DUPLICATE", "", uxExceptionRules, "Hang issue", "graphics", "Blocks test", "always_100%", "Discussion Comments:\n[1] Marked as duplicate of 14028999999", "No"
        });
        Check(dupReasoning.Contains("**CMF_DUPLICATE**") && dupReasoning.Contains("14028999999"), "CMF_DUPLICATE reasoning did not state duplicate ticket ID: " + dupReasoning);

        List<CmfRecommendationRuleScore> lowIncompleteRules = new List<CmfRecommendationRuleScore> {
            new CmfRecommendationRuleScore { RuleId = "S", RuleName = "Severity", Score = "2", Evaluation = "Contributing factor: Minor UI glitch. Evidence: glitch" },
            new CmfRecommendationRuleScore { RuleId = "O", RuleName = "Occurrence", Score = "1", Evaluation = "Contributing factor: Occurrence details not stated. Evidence: missing evidence" },
            new CmfRecommendationRuleScore { RuleId = "D", RuleName = "Detection", Score = "2", Evaluation = "Contributing factor: Corner-case flow. Evidence: test" },
            new CmfRecommendationRuleScore { RuleId = "R", RuleName = "Recovery", Score = "2", Evaluation = "Contributing factor: Recovery not documented. Evidence: missing evidence" },
            new CmfRecommendationRuleScore { RuleId = "B", RuleName = "Business Impact", Score = "1", Evaluation = "Contributing factor: Business impact details not stated. Evidence: missing evidence" },
            new CmfRecommendationRuleScore { RuleId = "CX", RuleName = "Customer Experience", Score = "2", Evaluation = "Contributing factor: Minimal user impact. Evidence: minor" }
        };
        string incompDisp = (string)resolveDispMethod.Invoke(null, new object[] { "CMF_INCOMPLETE", 40, 88, lowIncompleteRules, "333", "Minor glitch", "cmf_ask", "", "" });
        Check(incompDisp == "CMF_INCOMPLETE", "Missing core metrics should resolve to CMF_INCOMPLETE in 5-Tag Model: " + incompDisp);
        string incompReasoning = (string)singlePassMethod.Invoke(null, new object[] {
            "CMF_INCOMPLETE", "CMF_INCOMPLETE", "", lowIncompleteRules, "Minor glitch", "graphics", "", "", "", "No"
        });
        Check(incompReasoning.Contains("**CMF_INCOMPLETE**") && incompReasoning.Contains("Occurrence") && incompReasoning.Contains("Recovery"), "CMF_INCOMPLETE reasoning did not list incomplete metrics: " + incompReasoning);

        // Verify Component in opening sentence, preservation of sentences ending in 'platform.', & strict Yes/No RVP repro prefix
        string compRvpDesc = (string)Invoke("CleanupSightingDescription",
            "[Common.sw.win-driverAudio DSP] [Audio DSP] Saying \"hey Qira\" fails to wake the Qira app on the Lenovo Flex_7i NVL platform.\n\n- **Frequency & recovery:** Frequent; closing app recovers.\n- **Repro details:** RVP repro: Not applicable; reproduced on customer unit.",
            "Flicker",
            "Component: platform.Common.sw.win-driver\nComponent: Audio DSP\nRVP Platform Debug Details: NA");
        Check(compRvpDesc.StartsWith("[Audio DSP] Saying \"hey Qira\"") && !compRvpDesc.Contains("Common.sw.win-driver"), "Opening sentence had duplicate or raw win-driver component prefix: " + compRvpDesc);
        Check(compRvpDesc.Contains("RVP repro: No;") && !compRvpDesc.Contains("Not applicable"), "RVP repro was not normalized to strict Yes/No: " + compRvpDesc);

        // Verify Promoted ID extraction
        string extractedPromoted = AiSummaryService.ExtractPromotedIdFromContext("14028995309", "Discussion: This issue is promoted to sfip: https://hsdes.intel.com/appstore/article-one/#/15020108437");
        Check(extractedPromoted == "15020108437", "Promoted ID was not extracted from context: " + extractedPromoted);
        Check(AiSummaryService.ExtractPromotedIdFromContext("14028995309", "Promoted ID: N/A") == "NA", "Missing promoted ID did not default to NA.");

        // Verify duplicate CMF_REVIEW tag stripping, next-steps replacement with justification, and mandatory RVP repro mention in reasoning
        List<CmfRecommendationRuleScore> sampleRules = new List<CmfRecommendationRuleScore> {
            new CmfRecommendationRuleScore { RuleId = "S", RuleName = "Severity", Score = "3", Evaluation = "Contributing factor: Momentary full-screen flicker. Evidence: flicker" },
            new CmfRecommendationRuleScore { RuleId = "O", RuleName = "Occurrence", Score = "4", Evaluation = "Contributing factor: Frequent (50-90% of tries). Evidence: frequent" },
            new CmfRecommendationRuleScore { RuleId = "D", RuleName = "Detection", Score = "4", Evaluation = "Contributing factor: AC adapter plug/unplug. Evidence: plug" },
            new CmfRecommendationRuleScore { RuleId = "R", RuleName = "Recovery", Score = "3", Evaluation = "Contributing factor: Requires closing FurMark. Evidence: close" },
            new CmfRecommendationRuleScore { RuleId = "B", RuleName = "Business Impact", Score = "4", Evaluation = "Contributing factor: Blocks validation stress test. Evidence: block" },
            new CmfRecommendationRuleScore { RuleId = "CX", RuleName = "Customer Experience", Score = "4", Evaluation = "Contributing factor: Disrupts normal display use. Evidence: display" }
        };
        string alignedReasoning = (string)singlePassMethod.Invoke(null, new object[] {
            "CMF_OK", "CMF_OK",
            "- **Key risk drivers:** Display flickers on AC plug.\n- **Score tradeoff:** High severity and business urgency.\n- **AI Recommendation ([CMF_OK]):** CMF_OK: Confirm true reproducibility and attach fail videos plus display ETL for pass and fail.",
            sampleRules, "Title", "graphics", "Blocks test", "frequently_50%-90%", "Repro On RVP: Yes", "Yes"
        });
        Check(!alignedReasoning.Contains("([CMF_OK])") && !alignedReasoning.Contains("CMF_OK: Confirm") && alignedReasoning.Contains("because") && alignedReasoning.IndexOf("reproduced on RVP", StringComparison.OrdinalIgnoreCase) >= 0, "Duplicate CMF tag, next-step sentence, or mandatory RVP repro mention failed: " + alignedReasoning);

        // Verify RVP repro = No / NA handling in CMF_OK reasoning (preserves CMF_OK and explicitly states RVP repro: No)
        string rvpNoReasoning = (string)singlePassMethod.Invoke(null, new object[] {
            "CMF_OK", "CMF_OK",
            "- **Key risk drivers:** Display flickers on AC plug.\n- **Score tradeoff:** High severity and business urgency.\n- **AI Recommendation:** Approve as **CMF_OK** because this reproducible failure blocks customer validation.",
            sampleRules, "Title", "graphics", "Blocks test", "frequently_50%-90%", "Repro On RVP: NA", "NA"
        });
        Check(rvpNoReasoning.Contains("**CMF_OK**") && rvpNoReasoning.IndexOf("RVP repro: No", StringComparison.OrdinalIgnoreCase) >= 0, "RVP repro = NA/No was not explicitly noted in CMF_OK reasoning: " + rvpNoReasoning);

        // Verify Database Connection String in cmf-assistant-config.json & Scenario-Based Feedback Log (append without overwrite)
        Check(!string.IsNullOrWhiteSpace(CmfAssistantConfigService.ResolveDatabaseConnectionString()) && CmfAssistantConfigService.ResolveDatabaseConnectionString().IndexOf("gfxitt_db", StringComparison.OrdinalIgnoreCase) >= 0, "Database connection string was not resolved from cmf-assistant-config.json.");
        CmfFeedbackSubmitResponse fb1 = CmfFeedbackMemoryService.RecordFeedback("9990001", "Display flicker on desktop", "iGFX", "CMF_REVIEW", "CMF_OK", false, "Persistent desktop flicker blocks daily use");
        CmfFeedbackSubmitResponse fb2 = CmfFeedbackMemoryService.RecordFeedback("9990001", "Display flicker on desktop", "iGFX", "CMF_OK", "CMF_OK", true, "Confirmed CMF_OK scenario");
        Check(fb1.Success && fb2.Success && fb2.TotalRecordedCount == fb1.TotalRecordedCount + 1, "Submitting feedback on the same sighting ID overwrote the previous entry instead of appending a new scenario entry.");
        string scenarioContext = CmfFeedbackMemoryService.BuildFewShotFeedbackContext("iGFX", "Display flicker");
        Check(scenarioContext.Contains("LEARNED REVIEWER SCENARIO LOG") && scenarioContext.Contains("Scenario #1"), "Few-shot feedback context was not formatted as a scenario learning log: " + scenarioContext);

        // Run unit-test-only Critique Audit across all generated sections
        RunUnitTestCritiqueAudit(compRvpDesc, cleanedFix, cleanedImpact, alignedReasoning);

        MethodInfo chainMethod = typeof(CmfRecommendationService).GetMethod("ResolveGnaiModelChain", BindingFlags.NonPublic | BindingFlags.Static);
        List<string> modelChain = (List<string>)chainMethod.Invoke(null, new object[] { "claude-4-5-sonnet" });
        Check(modelChain.Count >= 3 && modelChain[0] == "claude-4-5-sonnet" && modelChain[1] == "claude-4-5-haiku" && modelChain[2] == "gpt-5-mini", "3-tier GNAI model chain was not resolved properly.");

        // Verify CMF Pulse AI Global Assistant & Executive PPTX Generation
        Check(CmfGlobalAssistantService.IsPptxRequested("Create me a PPT with NVL-H vs NVL-S to understand the trend of CMF pending issues"), "PPTX request prompt was not detected.");
        Check(!CmfGlobalAssistantService.IsPptxRequested("How many CMF_DUPLICATE issues are reported in graphics component vs other components?"), "Non-PPTX prompt should not trigger PPTX in prompt_only mode.");

        CmfGlobalAssistantResponse pulsePptxResp = CmfGlobalAssistantService.ProcessQuery(
            "Create me a PPT with NVL-H vs NVL-S to understand the trend of CMF pending issues, cmf_ok, cmf_incomplete",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "Dashboard",
            null);
        Check(pulsePptxResp != null && pulsePptxResp.Success && pulsePptxResp.HasChart && pulsePptxResp.HasPptx && pulsePptxResp.SlideCount == 4, "CMF Pulse AI failed to generate 4-slide executive PPTX and chart for NVL-H vs NVL-S.");
        Check(pulsePptxResp.ScopeLabel.Contains("NVL-H") && pulsePptxResp.ScopeLabel.Contains("NVL-S"), "CMF Pulse AI did not resolve NVL-H vs NVL-S scope: " + pulsePptxResp.ScopeLabel);

        CmfGlobalAssistantResponse pulseCompResp = CmfGlobalAssistantService.ProcessQuery(
            "How many CMF_DUPLICATE issues are reported in graphics component vs other components?",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "CMF Pending",
            null);
        Check(pulseCompResp != null && pulseCompResp.Success && pulseCompResp.HasChart && !pulseCompResp.HasPptx, "CMF Pulse AI component duplicate query failed or unexpectedly generated PPTX.");

        // Run full CMF Pulse AI Critique Agent evaluation (Dynamic schema, Dashboard context, Zero manipulation, General chatbot checks)
        RunCmfPulseCritiqueEvaluation();

        Console.WriteLine("PASS: Debug summary and CMF AI Assistant formatting, scoring, and guardrail tests.");
    }

    private static void RunCmfPulseCritiqueEvaluation()
    {
        // 1. General Chatbot Checks: Empty input, Greeting, Off-topic refusal
        CmfGlobalAssistantResponse emptyResp = CmfGlobalAssistantService.ProcessQuery("   ", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Dashboard");
        Check(!emptyResp.Success, "Critique failed: Empty prompt should be rejected.");

        CmfGlobalAssistantResponse greetResp = CmfGlobalAssistantService.ProcessQuery("Hello", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Dashboard");
        Check(greetResp.Success && !greetResp.HasChart && !greetResp.HasPptx && greetResp.Answer.Contains("CMF Pulse AI"), "Critique failed: Greeting check did not return conversational help.");

        CmfGlobalAssistantResponse offTopicResp = CmfGlobalAssistantService.ProcessQuery("What is the weather forecast and a good pasta recipe?", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Dashboard");
        Check(offTopicResp.Success && !offTopicResp.HasChart && !offTopicResp.HasPptx && offTopicResp.ScopeLabel == "Out of Scope", "Critique failed: Off-topic prompt was not declined.");

        // 2. Dynamic Platform Discovery Check (NVL-Hx is in SQL INFORMATION_SCHEMA.TABLES, not hardcoded in CmfPlatformCatalog)
        Dictionary<string, string> discoveredPlatforms = CmfGlobalAssistantService.GetAvailablePlatformTables();
        Check(discoveredPlatforms.ContainsKey("CMF_NVL_Hx_ALL_COMPONENTS_TABLE"), "Critique failed: Dynamic SQL table CMF_NVL_Hx_ALL_COMPONENTS_TABLE was not auto-discovered.");
        List<string> dynamicTarget = CmfGlobalAssistantService.ResolveTargetPlatforms("Compare NVL-Hx vs NVL-S", "");
        Check(dynamicTarget.Contains("CMF_NVL_Hx_ALL_COMPONENTS_TABLE") && dynamicTarget.Contains("CMF_NVL_S_ALL_COMPONENTS_TABLE"), "Critique failed: Dynamic platform NVL-Hx was not resolved from prompt.");

        // 3. Multi-turn Follow-Up Resolution Check ("Create a PPT for them" inherits NVL-H vs NVL-S from history)
        List<CmfGlobalAssistantTurn> history = new List<CmfGlobalAssistantTurn> {
            new CmfGlobalAssistantTurn { Role = "user", Text = "Compare NVL-H vs PTL pending issues" },
            new CmfGlobalAssistantTurn { Role = "assistant", Text = "Compared NVL-H vs PTL." }
        };
        List<string> followUpTargets = CmfGlobalAssistantService.ResolveTargetPlatforms("Now create a PPT for them", "CMF_LNL_ALL_COMPONENTS_TABLE", history);
        Check(followUpTargets.Contains("CMF_NVL_H_ALL_COMPONENTS_TABLE") && followUpTargets.Contains("CMF_PTL_ALL_COMPONENTS_TABLE"), "Critique failed: Multi-turn follow-up did not carry forward NVL-H and PTL from history.");

        // 4. Zero Data Manipulation Check against live SQL Server counts (NVL-H Total=172, NVL-S Total=51)
        CmfGlobalAssistantResponse liveCompare = CmfGlobalAssistantService.ProcessQuery(
            "Compare NVL-H vs NVL-S CMF status",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "Dashboard",
            null,
            "Dashboard KPI Card: Critical SLA Met 94% | Active Filter: All Milestones");
        Check(liveCompare.Success && liveCompare.HasChart && liveCompare.Chart.Categories.Count == 2, "Critique failed: Live NVL-H vs NVL-S chart categories missing.");
        Check(liveCompare.Chart.Series[1].Data[0] > 0 && liveCompare.Chart.Series[1].Data[1] > 0, "Critique failed: Chart series did not return expected SQL CMF_OK counts.");

        // 5. Specific Pending Issue Query Check (Most recent issue in CMF Pending list & when it came in)
        CmfGlobalAssistantResponse recentPendingResp = CmfGlobalAssistantService.ProcessQuery(
            "give me brief details of the most recent issue in cmf pending list and when did it come in the list",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "CMF Pending",
            null);
        Check(recentPendingResp.Success && !recentPendingResp.HasChart && !recentPendingResp.HasPptx, "Critique failed: Specific pending issue query should not show a platform trend chart.");
        Check(System.Text.RegularExpressions.Regex.IsMatch(recentPendingResp.Answer, @"\b\d{10,12}\b") && recentPendingResp.Answer.IndexOf("Added to the CMF Pending list", StringComparison.OrdinalIgnoreCase) >= 0, "Critique failed: Most recent pending issue ID and ask date were not returned: " + recentPendingResp.Answer);

        // 6. Verify GNAI API Key is stored directly in cmf-assistant-config.json (not FROM_ENV_OR_WEB_CONFIG)
        CmfAssistantUnifiedConfig cfg = CmfAssistantConfigService.GetConfig();
        Check(cfg != null && !string.IsNullOrWhiteSpace(cfg.GnaiApiKey) && cfg.GnaiApiKey != "FROM_ENV_OR_WEB_CONFIG" && cfg.GnaiApiKey.Length > 50, "Critique failed: GNAI apiKey is not populated directly in cmf-assistant-config.json.");
        Check(CmfAssistantConfigService.ResolveGnaiToken() == cfg.GnaiApiKey, "Critique failed: ResolveGnaiToken() did not return the key from cmf-assistant-config.json.");

        // 7. Verify Multi-Tab Portal Coverage (CMF Summary Designs/Freeze, TPT, OEMs, Owners, Config/CMF Rules)
        CmfGlobalAssistantResponse designResp = CmfGlobalAssistantService.ProcessQuery(
            "What are the top customer designs and their SW image freeze dates?",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "CMF Summary",
            null);
        Check(designResp.Success && designResp.Answer.IndexOf("Freeze", StringComparison.OrdinalIgnoreCase) >= 0, "Critique failed: Design & SW Image Freeze query did not return freeze details: " + designResp.Answer);

        CmfGlobalAssistantResponse tptResp = CmfGlobalAssistantService.ProcessQuery(
            "What is the average disposition TPT and resolution TPT for NVL-H?",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "CMF Summary",
            null);
        Check(tptResp.Success && tptResp.Answer.IndexOf("TPT", StringComparison.OrdinalIgnoreCase) >= 0, "Critique failed: TPT query did not return turnaround time metrics: " + tptResp.Answer);

        CmfGlobalAssistantResponse ownerResp = CmfGlobalAssistantService.ProcessQuery(
            "Who are the top owners with the most issues on NVL-H?",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "Issue List",
            null);
        Check(ownerResp.Success && ownerResp.Answer.Length > 40, "Critique failed: Owner breakdown query failed: " + ownerResp.Answer);

        CmfGlobalAssistantResponse rulesResp = CmfGlobalAssistantService.ProcessQuery(
            "What are the scoring rules, weights, and AI models in the Config tab?",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "Config/CMF Rules",
            null);
        Check(rulesResp.Success && rulesResp.Answer.Contains("claude-4-5-sonnet") && rulesResp.Answer.Contains("3.50"), "Critique failed: Config/CMF Rules query did not return model and 3.50 threshold details: " + rulesResp.Answer);

        // 8. Verify Jargon & Family Codename Comparison ("comparison of novalake vs pantherlake for graphics component")
        // Must resolve both NVL (NVL-H, NVL-S, NVL-Hx) and PTL, report Family Totals + Individual SKU breakdown, and generate a component comparison chart
        CmfGlobalAssistantResponse jargonCompResp = CmfGlobalAssistantService.ProcessQuery(
            "comparison of novalake vs pantherlake for graphics component",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "Dashboard",
            null);
        Check(jargonCompResp.Success && jargonCompResp.HasChart, "Critique failed: 'novalake vs pantherlake for graphics component' did not succeed or return a chart.");
        Check(jargonCompResp.ScopeLabel.Contains("NVL-H") && jargonCompResp.ScopeLabel.Contains("NVL-S") && jargonCompResp.ScopeLabel.Contains("PTL"), "Critique failed: 'novalake vs pantherlake' did not resolve both NVL SKUs and PTL in scope: " + jargonCompResp.ScopeLabel);
        Check(jargonCompResp.Answer.IndexOf("NVL", StringComparison.OrdinalIgnoreCase) >= 0 && jargonCompResp.Answer.IndexOf("PTL", StringComparison.OrdinalIgnoreCase) >= 0 && jargonCompResp.Answer.IndexOf("NVL-H", StringComparison.OrdinalIgnoreCase) >= 0 && jargonCompResp.Answer.IndexOf("NVL-S", StringComparison.OrdinalIgnoreCase) >= 0, "Critique failed: Answer did not include both Family Totals and Individual SKU breakdown for NovaLake vs PantherLake: " + jargonCompResp.Answer);

        // 9. Verify Fuzzy Jargon Matching (typos like "novalke vs pantherlak for grafics")
        List<string> fuzzyPlatforms = CmfGlobalAssistantService.ResolveTargetPlatforms("compare novalke vs pantherlak", "");
        Check(fuzzyPlatforms.Contains("CMF_NVL_H_ALL_COMPONENTS_TABLE") && fuzzyPlatforms.Contains("CMF_PTL_ALL_COMPONENTS_TABLE"), "Critique failed: Fuzzy Levenshtein matching did not resolve 'novalke vs pantherlak'.");

        // 10. Verify Ask CMF Pulse Scenario Feedback Log & Auto-Learning Jargon
        CmfFeedbackSubmitResponse pulseFb1 = CmfFeedbackMemoryService.RecordPulseFeedback("Compare NVL-H vs PTL audio", "NVL-H vs PTL", "Dashboard", false, "map soundwire to Audio");
        CmfFeedbackSubmitResponse pulseFb2 = CmfFeedbackMemoryService.RecordPulseFeedback("Compare NVL-H vs PTL audio", "NVL-H vs PTL", "Dashboard", true, "Include SKU breakdown");
        Check(pulseFb1.Success && pulseFb2.Success && pulseFb2.TotalRecordedCount == pulseFb1.TotalRecordedCount + 1, "Critique failed: Pulse feedback log did not append scenario entries.");
        Check(pulseFb1.Message.IndexOf("soundwire", StringComparison.OrdinalIgnoreCase) >= 0, "Critique failed: Pulse feedback did not auto-learn jargon 'soundwire -> Audio': " + pulseFb1.Message);
        string pulseFewShot = CmfFeedbackMemoryService.BuildPulseFewShotFeedbackContext("audio", "Dashboard");
        Check(pulseFewShot.Contains("LEARNED ASK CMF PULSE SCENARIO FEEDBACK"), "Critique failed: Pulse few-shot feedback context was not generated.");

        // 11. Verify Excel (.xlsx) Importer for Scoring Guidelines & Guardrails
        string excelPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "CMF_6_Category_Scoring_Mechanism.xlsx");
        if (!System.IO.File.Exists(excelPath)) excelPath = @"c:\Users\pdn\Downloads\check_cmf_portal\CMF_6_Category_Scoring_Mechanism.xlsx";
        if (System.IO.File.Exists(excelPath))
        {
            using (System.IO.FileStream fs = System.IO.File.Open(excelPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                string importMsg;
                bool imported = CmfAssistantConfigService.ImportScoringGuidelinesFromExcel(fs, out importMsg);
                Check(imported && importMsg.Contains("4-point scoring guidelines"), "Critique failed: Excel scoring guidelines import failed: " + importMsg);
            }
        }

        // 12. Verify Custom Report Template Saving & Template-Driven PPTX Generation in Ask CMF Pulse
        CmfPulseReportTemplate savedTpl = CmfGlobalAssistantService.SaveCustomPulseReportTemplate(
            "Weekly OEM Freeze Executive Deck",
            "Slide 1: Weekly OEM & Freeze Executive Review; Slide 3: OEM & SW Image Freeze Matrix; Slide 4: Top Component Blockers",
            "oem_freeze_readiness");
        Check(savedTpl != null && !string.IsNullOrWhiteSpace(savedTpl.TemplateId), "Critique failed: Custom Pulse report template was not saved.");
        CmfGlobalAssistantResponse tplPptResp = CmfGlobalAssistantService.ProcessQuery(
            "Generate executive report for NVL-H",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "CMF Summary",
            null,
            null,
            savedTpl.TemplateId);
        Check(tplPptResp != null && tplPptResp.Success && tplPptResp.HasPptx && tplPptResp.SlideCount == 4, "Critique failed: Custom template + 1-line prompt did not generate a 4-slide PPTX deck.");
    }

    private static void RunUnitTestCritiqueAudit(string description, string fixStatus, string impactSummary, string reasoning)
    {
        Check(!description.TrimStart().StartsWith("- "), "Critique audit failed: Problem Description is missing its opening sentence.");
        Check(System.Text.RegularExpressions.Regex.IsMatch(description, @"\bRVP\s+repro\s*:\s*(?:Yes|No)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase), "Critique audit failed: Problem Description is missing strict Yes/No RVP repro status.");
        Check(!description.Contains("Current fix status") && !description.Contains("Customer fix status"), "Critique audit failed: Fix status leaked into Problem Description.");
        Check(fixStatus.Contains("**Fix & workaround:**") && fixStatus.Contains("**Validation status:**"), "Critique audit failed: Fix Status section is missing required structure.");
        Check(!System.Text.RegularExpressions.Regex.IsMatch(impactSummary, @"\b\d+\s*/\s*\d+\s*(?:UUTs?|units?|cycles?|prototypes?)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase), "Critique audit failed: Occurrence/sample count leaked into Impact Summary.");
        Check(reasoning.Contains("**AI Recommendation:**") && !System.Text.RegularExpressions.Regex.IsMatch(reasoning, @"\*\*AI\s+Recommendation\s*\([^)]*\)\s*:\*\*", System.Text.RegularExpressions.RegexOptions.IgnoreCase), "Critique audit failed: Reasoning still has redundant tag inside AI Recommendation label.");
    }
}
