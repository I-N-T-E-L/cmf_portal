using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

public static class DashboardInsightsTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static void Main()
    {
        DateTime today = new DateTime(2026, 11, 3);
        DataTable data = new DataTable();
        foreach (string column in new[] { "cp_id", "title", "source_platform", "component", "component_group", "promoted_id", "merge_id", "cmf_request", "status" })
            data.Columns.Add(column, typeof(string));
        foreach (string column in new[] { "ask_date", "decided_date", "implemented_date", "closed_date" }) data.Columns.Add(column, typeof(DateTime));
        data.Columns.Add("comparable_status", typeof(int));
        data.Rows.Add("16011111111", "Screen hang on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        data.Rows.Add("16022222222", "Screen flicker on external display", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-5), today.AddDays(-2), DBNull.Value, DBNull.Value, 1);
        data.Rows.Add("16022222222", "Screen flicker on external display", "CMF_NVL_H_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_duplicate", "open", today.AddDays(-5), today.AddDays(-2), DBNull.Value, DBNull.Value, 1);
        data.Rows.Add("16033333333", "Closed case", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "", "", "cmf_ok", "complete", today.AddDays(-30), today.AddDays(-20), DBNull.Value, today.AddDays(-1), 1);
        data.Rows.Add("16044444444", "Bad history", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "", "", "cmf_ok", "implemented", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 0);
        data.Rows[0]["promoted_id"] = "16099999999,16099999999";
        data.Columns.Add("description", typeof(string));
        data.Rows[0]["description"] = new string('x', 24000) + " full description tail";
        var result = CmfDashboardInsightsService.Build(data, "Overall", today);
        var article = new HsdArticleData { Comments = new List<string>() };
        typeof(HsdPortalService).GetMethod("MergeArticleJsonIntoResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { "{\"title\":\"A title\",\"summary\":\"Technical summary\",\"sysdebug_forum\":\"Forum evidence\",\"steps_to_reproduce\":\"Reproduction evidence\",\"comments\":[\"one\",\"two\",\"three\",\"four\",\"five\",\"six\"]}", article, false, true });
        Check(article.InsightFields["comments[5]"] == "six" && article.InsightFields["sysdebug_forum"] == "Forum evidence",
            "Insights source path dropped comments or forum evidence.");
        int requests = 0;
        HsdPortalService.ReadInsightCommentPages("https://example.test/article/comments", url => {
            requests++;
            return requests == 1 ? "{\"data\":[{\"text\":\"First full comment\"}],\"next\":\"?page=2\"}"
                : "{\"data\":[{\"text\":\"Final full comment\"}]}";
        }, article);
        Check(requests == 2 && article.InsightFields.Values.Contains("Final full comment"), "Insights did not follow all discussion pages.");
        bool unsafePaginationRejected = false;
        try { HsdPortalService.ReadInsightCommentPages("https://example.test/article/comments", url => "{\"data\":[],\"next\":\"https://other.test/comments\"}", article); }
        catch (InvalidOperationException) { unsafePaginationRejected = true; }
        Check(unsafePaginationRejected, "Discussion pagination could send authenticated retrieval to another origin.");
        Check(result.Documents.Count == 4 && result.Documents.Sum(document => document.Occurrences) == 5,
            "AI input does not cover every scoped issue and duplicate occurrence.");
        Check(result.Documents.Single(document => document.Id == "16011111111").Fields["Local.description"].EndsWith("full description tail"),
            "Detailed AI input was truncated.");
        Check(result.Documents.Any(document => document.Buckets.Contains("Closed"))
            && result.Documents.Any(document => document.Buckets.Contains("Implemented"))
            && result.Documents.Any(document => document.Buckets.Contains("Open")), "AI input lost a Summary status bucket.");
        var ingredient = result.Trends.Single(trend => trend.Dimension == "Ingredient");
        Check(ingredient.Open == 3 && ingredient.PreviousOpen == 2 && ingredient.Change == 1, "Open balances or duplicate row semantics changed.");
        Check(ingredient.Excluded == 1 && ingredient.OldestDays == 20, "Exclusions or acceptance aging changed.");
        Check(result.Records == 5 && result.DistinctIssues == 4 && result.DuplicateRecords == 1, "Occurrence and distinct issue counts were conflated.");
        Check(result.Groups.Count == 0 && result.Highlights.Count == 0, "Merge IDs or shared components fabricated non-AI insights.");
        Check(result.CanCompare && result.DateRange == "All acceptance dates", "Insights lost its date-filter-independent scope.");
        DataTable reversed = data.Clone();
        foreach (DataRow row in data.AsEnumerable().Reverse()) reversed.ImportRow(row);
        Check(CmfDashboardInsightsService.Build(reversed, "Overall", today).SourceHash == result.SourceHash, "Source fingerprint depended on SQL row order.");
        DataTable platform = data.AsEnumerable().Where(row => (string)row["source_platform"] == "CMF_PTL_ALL_COMPONENTS_TABLE").CopyToDataTable();
        var scoped = CmfDashboardInsightsService.Build(platform, "PTL", today);
        Check(scoped.Groups.Count == 0 && scoped.Trends.Where(trend => trend.Dimension == "Platform").Count() == 1, "Platform scope leaked cross-platform relationships.");
        for (int index = 0; index < 24; index++)
            data.Rows.Add((16060000000L + index).ToString(), "Repeated screen hang", "CMF_PTL_ALL_COMPONENTS_TABLE", "Ingredient " + index, "Group " + index, "", "", "cmf_ok", "open", today.AddDays(-40), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        var expanded = CmfDashboardInsightsService.Build(data, "Overall", today);
        var expandedData = data.Copy();
        Check(expanded.Documents.Count == 28 && expanded.TrendCount > 30, "A total issue or trend analysis cap remains.");
        DataTable futureData = data.Clone();
        futureData.Rows.Add("16011111111", "Screen hang on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "open", today.AddDays(-30), today.AddDays(-20), DBNull.Value, DBNull.Value, 1);
        futureData.Rows.Add("16022222222", "Screen flicker on external display", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_duplicate", "future", today.AddDays(-30), today.AddDays(-20), today.AddDays(-2), DBNull.Value, 1);
        futureData.Rows.Add("16033333333", "Screen hang with future fix", "CMF_PTL_ALL_COMPONENTS_TABLE", "Display", "Graphics", "16099999999", "", "cmf_ok", "future", today.AddDays(-30), today.AddDays(-20), today.AddDays(-10), DBNull.Value, 1);
        var futureResult = CmfDashboardInsightsService.Build(futureData, "Overall", today);
        var futureIngredient = futureResult.Trends.Single(trend => trend.Dimension == "Ingredient");
        Check(futureIngredient.Open == 1 && futureIngredient.PreviousOpen == 2, "Future statuses were counted as currently Open or lost their historical implementation boundary.");
        Check(futureResult.Documents.Count(document => document.Buckets.Contains("Implemented")) == 2
            && futureResult.Records == 3 && futureResult.DuplicateRecords == 1, "Future classification changed AI scope, record totals or duplicate scope.");
        data.Clear();
        Check(CmfDashboardInsightsService.Build(data, "Empty", today).Highlights.Count == 0, "Empty scope fabricated findings.");
        CheckAiPipeline(data.Clone(), today);
        CheckAiPipeline(expandedData, today);
        CheckAiPipeline(futureData, today);
        CheckStructuredCandidates(today);
        CheckHybridPipeline(expandedData, today);
        Console.WriteLine("Dashboard Insights hybrid grouping, on-demand evidence, rollup and pagination tests passed.");
    }

    private static void CheckStructuredCandidates(DateTime today)
    {
        var documents = Enumerable.Range(0, 8).Select(index => new CmfInsightDocument {
            Id = (16070000000L + index).ToString(), Platforms = new List<string> { "PTL" },
            Buckets = new List<string> { index == 0 ? "Closed" : index == 1 ? "Implemented" : "Open" },
            Fields = new Dictionary<string, string> {
                { "Local.processor", "pantherlake" }, { "Local.component_group", index < 3 ? "Graphics" : "Media" },
                { "Local.component", index < 3 ? "Display" : "Codec " + index }, { "Local.family", index < 3 ? "Client" : "Other " + index },
                { "Local.release", index < 3 ? "R1" : "R" + index }, { "Local.title", "Identical words must not drive grouping" }
            }
        }).ToList();
        var candidates = CmfDashboardInsightsService.RankCandidates(documents);
        Check(candidates.Any(candidate => candidate.IssueIds.Count == 3 && candidate.SharedFields["component"] == "display"), "Multi-field categorical cluster was missed.");
        Check(candidates.All(candidate => candidate.SharedFields.Count >= 2 && candidate.Score > 0 && candidate.IssueIds.Count < documents.Count), "Broad platform classification was ranked as a meaningful pattern.");
        Check(candidates.Select(candidate => string.Join(",", candidate.IssueIds)).Distinct().Count() == candidates.Count, "Equivalent candidate membership was duplicated.");
        Check(candidates.First().ClosedIssues == 1 && candidates.First().ImplementedIssues == 1, "Closed or Implemented issues were excluded from clustering.");
        Check(CmfDashboardInsightsService.RankCandidates(documents.AsEnumerable().Reverse()).Select(candidate => candidate.Id).SequenceEqual(candidates.Select(candidate => candidate.Id)), "Candidate ranks depend on source row order.");
        foreach (var document in documents) document.Fields = new Dictionary<string, string> { { "Local.processor", "pantherlake" }, { "Local.title", "same title" } };
        Check(CmfDashboardInsightsService.RankCandidates(documents).Count == 0, "Platform/processor or arbitrary text alone fabricated a cluster.");
    }

    private static void CheckHybridPipeline(DataTable data, DateTime today)
    {
        data = data.Copy();
        foreach (DataRow row in data.Rows) row["title"] = "External display loses signal during resume handshake";
        var evidence = CmfDashboardInsightsService.Build(data, "PTL", today);
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var fetched = new List<string>();
        var supplied = new HashSet<string>();
        int calls = 0;
        bool requestDetails = false;
        bool sawTail = false;
        const string technical = "Display resume loses the external panel after the firmware handshake times out.";
        Func<string, HsdArticleData> fetch = id => {
            fetched.Add(id);
            return new HsdArticleData { ArticleId = id, FetchSuccess = true,
                Description = technical, Comments = new List<string> { technical },
                InsightFields = new Dictionary<string, string> { { "description", new string('x', 24000) + technical },
                    { "comments[8]", technical }, { "discussion_page[0][1].fields.description", technical },
                    { "discussion_page[0][1].schema.enormous_schema", new string('z', 100000) },
                    { "comments[0].definitions.enormous_schema", new string('z', 100000) },
                    { "definitions.enormous_schema", new string('z', 100000) } } };
        };
        Func<string, string> generate = prompt => {
            calls++;
            Check(!prompt.Contains("enormous_schema"), "Provider schema metadata leaked into AI context.");
            sawTail = sawTail || prompt.Contains(technical);
            var input = serializer.Deserialize<Dictionary<string, object>>(prompt.Substring(prompt.IndexOf("\nINPUT_JSON\n", StringComparison.Ordinal) + 12));
            var packets = ((System.Collections.IEnumerable)input["Groups"]).Cast<object>()
                .Select(item => serializer.Deserialize<CmfInsightClusterPacket>(serializer.Serialize(item))).ToList();
            foreach (var packet in packets) foreach (var source in packet.Evidence) supplied.Add(source.IssueId);
            Check(packets.All(packet => packet.Evidence.All(chunk => chunk.Field != "Local.processor" && chunk.Field != "Local.family")),
                "Repeated per-member categories were not removed from compact AI evidence.");
            return serializer.Serialize(new CmfInsightClusterReply { Reviews = packets.Select(packet => new CmfInsightClusterOpinion {
                Id = packet.Id, NeedsDetails = requestDetails && packet.Stage == "structured",
                Supported = !(requestDetails && packet.Stage == "structured"), Kind = "trait", Title = "Display-focused cohort",
                Opinion = "Shared display classification is useful for review but does not establish a common cause.",
                Action = "Compare the recorded symptoms before combining investigations.",
                EvidenceKeys = packet.Stage == "rollup" ? packet.PlatformInsights.Select(finding => finding.Id).ToList()
                    : requestDetails ? packet.Evidence.Where(chunk => chunk.Field == "Local.title").Select(chunk => chunk.Key).ToList() : new List<string>()
            }).ToList() });
        };
        var result = CmfDashboardInsightsService.AnalyzeHybrid(evidence, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        Check(calls > 0 && fetched.Count == 0, "Structured review fetched all HSDs eagerly.");
        Check(supplied.SetEquals(evidence.Candidates.SelectMany(candidate => candidate.IssueIds)), "AI received ungrouped issues or skipped candidate members.");
        Check(result.Mode == "ai" && result.HighlightCount > 0 && result.GroupCount == 0, "A classification trait was mislabeled as technical similarity.");
        Check(result.Highlights.All(finding => finding.AiReviewed && finding.SharedFields.Count >= 2), "Hybrid output lost AI provenance or structured justification.");
        requestDetails = true;
        supplied.Clear();
        var detailed = CmfDashboardInsightsService.AnalyzeHybrid(evidence, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        Check(fetched.Count == fetched.Distinct().Count() && fetched.All(id => evidence.Candidates.Any(candidate => candidate.IssueIds.Contains(id))), "On-demand evidence was duplicated or fetched outside clusters.");
        Check(sawTail && detailed.RetrievedIssues == fetched.Count, "Requested rich text was truncated or retrieval progress omitted.");
        int before = calls;
        var empty = CmfDashboardInsightsService.Build(data.Clone(), "Empty", today);
        var noGroups = CmfDashboardInsightsService.AnalyzeHybrid(empty, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        Check(calls == before && noGroups.Highlights.Count == 0 && noGroups.SourceStatus.Contains("0 ranked groups"), "Empty grouping fabricated an AI call or insights.");
        var second = CmfDashboardInsightsService.Build(data.Copy(), "Second", today);
        foreach (var document in second.Documents) document.Platforms = new List<string> { "SECOND" };
        second.Candidates = CmfDashboardInsightsService.RankCandidates(second.Documents);
        var secondResult = CmfDashboardInsightsService.AnalyzeHybrid(second, fetch, generate, ignored => {}, System.Threading.CancellationToken.None);
        var rollup = CmfDashboardInsightsService.AnalyzeRollup(evidence, new List<CmfDashboardInsights> { result, secondResult }, generate, System.Threading.CancellationToken.None);
        Check(rollup.Mode == "ai" && rollup.PlatformsReady == 2 && rollup.SourceStatus.Contains("no raw-HSD re-fetch"), "Overall did not derive from cached platform insights.");
        Check(rollup.Highlights.Any(finding => finding.PlatformCount >= 2) && rollup.HighlightCount >= result.HighlightCount,
            "Overall collapsed all platform insights to a single summary or omitted cross-platform shared traits.");
        bool failed = false;
        try { CmfDashboardInsightsService.AnalyzeRollup(evidence, new List<CmfDashboardInsights> { evidence }, generate, System.Threading.CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "Overall accepted incomplete platform reviews.");
        foreach (string defect in new[] { "missing-review", "foreign-source", "taxonomy-technical", "undated-trend" })
        {
            failed = false;
            try {
                CmfDashboardInsightsService.AnalyzeHybrid(evidence, fetch, prompt => {
                    var reply = serializer.Deserialize<CmfInsightClusterReply>(generate(prompt));
                    if (defect == "missing-review") reply.Reviews.RemoveAt(0);
                    else { reply.Reviews[0].Supported = true; reply.Reviews[0].NeedsDetails = false;
                        reply.Reviews[0].Kind = defect == "taxonomy-technical" ? "technical" : defect == "undated-trend" ? "trend" : "trait";
                        reply.Reviews[0].EvidenceKeys = defect == "foreign-source" ? new List<string> { "foreign" } : new List<string>(); }
                    return serializer.Serialize(reply);
                }, ignored => {}, System.Threading.CancellationToken.None);
            } catch (InvalidOperationException) { failed = true; }
            Check(failed, "Invalid hybrid opinion accepted: " + defect);
        }
        requestDetails = false;
        var bulk = CmfDashboardInsightsService.Build(data.Clone(), "Bulk", today);
        bulk.Documents = Enumerable.Range(0, 24).Select(index => new CmfInsightDocument {
            Id = (16080000000L + index).ToString(), Platforms = new List<string> { "PTL" },
            Buckets = new List<string> { "Open" }, Occurrences = 1, OpenRecords = 1,
            Fields = new Dictionary<string, string> { { "Local.title", technical }, { "Local.processor", "pantherlake" },
                { "Local.component_group", "Group " + index / 2 }, { "Local.component", "Ingredient " + index / 2 },
                { "Local.family", "Family " + index / 4 } }
        }).ToList();
        bulk.Candidates = CmfDashboardInsightsService.RankCandidates(bulk.Documents);
        bulk.CandidateCount = bulk.Candidates.Count;
        bulk.ClusteredIssues = bulk.Records = bulk.DistinctIssues = bulk.Documents.Count;
        var many = CmfDashboardInsightsService.AnalyzeHybrid(bulk, id => { throw new Exception("Unexpected eager fetch."); }, generate,
            ignored => {}, System.Threading.CancellationToken.None);
        Check(many.HighlightCount > 8 && many.ReviewedCandidates == bulk.CandidateCount,
            "Hybrid candidate review has a fixed issue/group/highlight cap.");
        var pages = new List<string>();
        for (int page = 0; page * CmfDashboardInsightsService.FindingsPerPage < many.HighlightCount; page++)
            pages.AddRange(CmfDashboardInsightsService.CreatePage(many, "highlights", page).Items.Select(finding => finding.Id));
        Check(pages.Count == many.HighlightCount && pages.Distinct().Count() == pages.Count,
            "Hybrid pagination lost or duplicated reviewed groups.");
    }

    private static void CheckAiPipeline(DataTable data, DateTime today)
    {
        var evidence = CmfDashboardInsightsService.Build(data, "PTL", today);
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        var fetched = new List<string>();
        var reviewed = new HashSet<string>();
        int modelCalls = 0;
        string technical = "Display resume loses the external panel after the firmware handshake times out.";
        Func<string, HsdArticleData> fetch = id => {
            fetched.Add(id);
            return new HsdArticleData { ArticleId = id, FetchSuccess = true,
                InsightFields = new Dictionary<string, string> {
                    { "description", new string('x', 24000) + technical }, { "summary", technical },
                    { "comments[8]", technical }, { "sysdebug_forum", technical }, { "steps_to_reproduce", technical }
                } };
        };
        Func<string, string> generate = prompt => {
            modelCalls++;
            var input = serializer.Deserialize<Dictionary<string, object>>(prompt.Substring(prompt.IndexOf("\nINPUT_JSON\n", StringComparison.Ordinal) + 12));
            if (input.ContainsKey("Chunks"))
            {
                var chunks = ((System.Collections.IEnumerable)input["Chunks"]).Cast<object>().Select(item => serializer.Deserialize<CmfInsightChunk>(serializer.Serialize(item))).ToList();
                foreach (var chunk in chunks) reviewed.Add(chunk.Key);
                return serializer.Serialize(new CmfInsightReviewReply {
                    Reviewed = chunks.Select(chunk => chunk.Key).ToList(),
                    Observations = chunks.Where(chunk => chunk.Text.Contains(technical)).Select(chunk => new CmfInsightObservation {
                        Title = "Resume handshake timeout", Summary = "External panel lost after firmware handshake timeout.", Action = "Compare the recorded resume trace.",
                        Citations = new List<CmfInsightCitation> { new CmfInsightCitation { Chunk = chunk.Key, Quote = technical } }
                    }).ToList()
                });
            }
            var observations = ((System.Collections.IEnumerable)input["Observations"]).Cast<object>().Select(item => serializer.Deserialize<CmfInsightObservation>(serializer.Serialize(item))).ToList();
            return serializer.Serialize(new CmfInsightComparisonReply {
                Reviewed = observations.Select(observation => observation.Id).ToList(),
                Findings = observations.SelectMany(observation => observation.Citations).Select(citation => citation.IssueId).Distinct().Count() < 2
                    ? new List<CmfInsightSemanticFinding>() : new List<CmfInsightSemanticFinding> { new CmfInsightSemanticFinding {
                        Title = "External-panel resume handshake", Explanation = "Each excerpt reports external panel loss after a firmware handshake timeout; root cause remains unconfirmed.",
                        Action = "Compare the recorded resume traces.", Confidence = "high", Citations = observations.SelectMany(observation => observation.Citations).ToList()
                    } }
            });
        };
        var result = CmfDashboardInsightsService.Analyze(evidence, fetch, generate, progress => {}, System.Threading.CancellationToken.None);
        Check(fetched.Count == evidence.DistinctIssues && fetched.Distinct().Count() == evidence.DistinctIssues, "AI did not retrieve every distinct scoped issue exactly once.");
        Check(result.Mode == "ai" && result.AnalyzedIssues == evidence.DistinctIssues && result.AnalyzedChunks == result.TotalChunks
            && reviewed.Count == result.TotalChunks, "AI skipped input chunks or returned a non-AI result.");
        if (evidence.DistinctIssues == 0)
        {
            Check(modelCalls == 1 && result.Highlights.Count == 0 && result.Groups.Count == 0, "Empty scope bypassed AI or fabricated insights.");
            return;
        }
        Check(result.Highlights.Count > 3 && result.Highlights.All(finding => finding.AiReviewed), "AI findings were capped or mixed with calculated results.");
        Check(result.Highlights.Count(finding => finding.Id.StartsWith("highlight-", StringComparison.Ordinal)) == evidence.DistinctIssues,
            "The same verbatim technical evidence repeated across fields created redundant highlights.");
        Check(result.Groups.Count > 0 && result.Groups.All(group => group.Reasons.Count > 2), "Semantic similarity lacks source excerpts.");
        if (evidence.DistinctIssues > 20)
            Check(result.TotalComparisons > 1 && result.ComparisonsCompleted == result.TotalComparisons, "Cross-block AI comparisons were skipped.");
        Check(result.Highlights.SelectMany(finding => finding.Reasons).Any(reason => reason.Source == "HSD.description")
            && result.Highlights.SelectMany(finding => finding.Reasons).Any(reason => reason.Source == "HSD.comments[8]")
            && result.Highlights.SelectMany(finding => finding.Reasons).Any(reason => reason.Source == "HSD.sysdebug_forum"), "Long text tails, older comments or forum evidence were omitted.");
        Check(!CmfDashboardInsightsService.CreatePage(evidence, "highlights", 0).Success
            && CmfDashboardInsightsService.CreateView(evidence, "test").Highlights.Count == 0, "Calculated results were publicly exposed before AI analysis.");
        var view = CmfDashboardInsightsService.CreateView(result, "opaque-token");
        Check(view.Highlights.Count == Math.Min(CmfDashboardInsightsService.FindingsPerPage, result.Highlights.Count) && view.HighlightCount == result.Highlights.Count
            && view.Documents == null, "Public pagination capped AI analysis or exposed raw source documents.");
        var ids = new List<string>();
        for (int page = 0; page * CmfDashboardInsightsService.FindingsPerPage < result.HighlightCount; page++)
            ids.AddRange(CmfDashboardInsightsService.CreatePage(result, "highlights", page).Items.Select(finding => finding.Id));
        Check(ids.Count == result.HighlightCount && ids.Distinct().Count() == ids.Count, "Pagination lost or duplicated AI findings.");
        Check(!CmfDashboardInsightsService.CreatePage(result, "other", 0).Success
            && !CmfDashboardInsightsService.CreatePage(result, "highlights", -1).Success
            && !CmfDashboardInsightsService.CreatePage(result, "highlights", int.MaxValue).Success, "Invalid page/view accepted.");
        bool failed = false;
        try { CmfDashboardInsightsService.Analyze(evidence, fetch, prompt => "{}", progress => {}, System.Threading.CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "AI failure silently returned calculated findings.");
        var adaptive = CmfDashboardInsightsService.Analyze(evidence, fetch, prompt => {
            string output = generate(prompt);
            if (prompt.Contains("\"Chunks\""))
            {
                var reply = serializer.Deserialize<CmfInsightReviewReply>(output);
                if (reply.Reviewed.Count > 5) reply.Reviewed.RemoveAt(0);
                return serializer.Serialize(reply);
            }
            return output;
        }, progress => {}, System.Threading.CancellationToken.None);
        Check(adaptive.Mode == "ai" && adaptive.AnalyzedChunks == adaptive.TotalChunks && adaptive.AnalyzedIssues == evidence.DistinctIssues,
            "Adaptive AI batch retries skipped inputs or accepted incomplete acknowledgments.");
        foreach (string defect in new[] { "missing-input", "invented-quote", "invented-chunk", "taxonomy-only", "low-confidence", "foreign-comparison" })
        {
            failed = false;
            try
            {
                CmfDashboardInsightsService.Analyze(evidence, fetch, prompt => {
                    string output = generate(prompt);
                    if (prompt.Contains("\"Chunks\""))
                    {
                        var reply = serializer.Deserialize<CmfInsightReviewReply>(output);
                        if (defect == "missing-input" && reply.Reviewed.Count > 0) reply.Reviewed.RemoveAt(0);
                        if (reply.Observations.Count > 0)
                        {
                            var citation = reply.Observations[0].Citations[0];
                            if (defect == "invented-quote") citation.Quote = "This technical claim does not occur in any source field.";
                            if (defect == "invented-chunk") citation.Chunk = "out-of-scope";
                            if (defect == "taxonomy-only") citation.Quote = "Display";
                        }
                        return serializer.Serialize(reply);
                    }
                    if (defect == "low-confidence" || defect == "foreign-comparison")
                    {
                        var reply = serializer.Deserialize<CmfInsightComparisonReply>(output);
                        if (reply.Findings.Count > 0)
                        {
                            if (defect == "low-confidence") reply.Findings[0].Confidence = "low";
                            else reply.Findings[0].Citations[0].Chunk = "out-of-scope";
                        }
                        return serializer.Serialize(reply);
                    }
                    return output;
                }, progress => {}, System.Threading.CancellationToken.None);
            }
            catch (InvalidOperationException) { failed = true; }
            Check(failed, "Invalid AI evidence accepted: " + defect);
        }
        failed = false;
        try { CmfDashboardInsightsService.Analyze(evidence, id => null, generate, progress => {}, System.Threading.CancellationToken.None); }
        catch (InvalidOperationException) { failed = true; }
        Check(failed, "Missing HSD retrieval silently reduced all-issue AI coverage.");
    }

    public static void CheckBalanceEvidenceSql(SqlConnection connection)
    {
        DateTime today = new DateTime(2026, 11, 3);
        string source = @"(SELECT cp_id, title, source_platform, component, component_group, promoted_id, merge_id,
            cmf_request, status, sysdebug, TRY_CAST(asked AS date) AS date_cmf_ask,
            TRY_CAST(decided AS date) AS date_cmf_decided, TRY_CAST(implemented AS date) AS implemented_date,
            TRY_CAST(closed AS date) AS closed_date FROM (VALUES
            ('16011111111','Old open','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_ok','open','customer_must_fix','2026-10-01','2026-10-10',NULL,NULL),
            ('16022222222','New open','CMF_NVL_H_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_duplicate','open','customer_must_fix','2026-11-01','2026-11-02',NULL,NULL),
            ('16033333333','Closed','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','complete','customer_must_fix','2026-10-01','2026-10-10',NULL,'2026-11-02'),
            ('16044444444','Invalid status dates','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','implemented','customer_must_fix','2026-10-01','2026-10-10',NULL,NULL),
            ('16055555555','Reversed decision','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','open','customer_must_fix','2026-10-10','2026-10-01',NULL,NULL),
            ('16066666666','Not CMF','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','open','other','2026-10-01','2026-10-10',NULL,NULL),
            ('16077777777','Future old implementation','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_ok','future','customer_must_fix','2026-10-01','2026-10-10','2026-10-20',NULL),
            ('16088888888','Future recent implementation','CMF_NVL_H_ALL_COMPONENTS_TABLE','Display','Graphics','16099999999','','cmf_duplicate','future','customer_must_fix','2026-10-01','2026-10-10','2026-11-02',NULL),
            ('16099999999','Future missing implementation','CMF_PTL_ALL_COMPONENTS_TABLE','Display','Graphics','','','cmf_ok','future','customer_must_fix','2026-10-01','2026-10-10',NULL,NULL)
            ) AS fixture(cp_id,title,source_platform,component,component_group,promoted_id,merge_id,cmf_request,status,sysdebug,asked,decided,implemented,closed)) AS source";
        DataTable evidence = new DataTable();
        using (SqlCommand command = CmfSummaryTrendService.CreateBalanceEvidenceCommand(connection, source, today))
        using (SqlDataReader reader = command.ExecuteReader()) evidence.Load(reader);
        var insights = CmfDashboardInsightsService.Build(evidence, "Overall", today);
        using (SqlCommand command = CmfSummaryTrendService.CreateBalanceEstimateCommand(connection, source, today))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            reader.Read();
            var platforms = insights.Trends.Where(trend => trend.Dimension == "Platform");
            Check(platforms.Sum(trend => trend.Open) == reader.GetInt32(3), "Grouped current balances differ from Summary SQL.");
            Check(platforms.Sum(trend => trend.PreviousOpen.GetValueOrDefault()) == reader.GetInt32(7), "Grouped historical balances differ from Summary SQL.");
            Check(insights.ExcludedRecords == reader.GetInt32(9) && insights.Records == reader.GetInt32(10), "Group eligibility differs from Summary SQL.");
        }
        Console.WriteLine("Dashboard Insights SQL eligibility parity passed.");
    }
}