const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const source = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx'), 'utf8');

function createDrawer(response) {
    const ids = [
        'cmfRecTitleRow', 'cmfRecSightingRow', 'cmfAssistantSightingDetails',
        'cmfAssistantQualityAssessment', 'cmfAssistantRecommendationSection',
        'cmfAssistantImpactSection', 'cmfRecQuality', 'cmfRecReasoningSection',
        'cmfRecActions', 'cmfDecisionDetailsBody', 'cmfRecHeading', 'cmfRecDrawerBg',
        'cmfRecDrawer', 'cmfRecRecommendation', 'cmfRecEvidence', 'cmfRecCpId',
        'cmfRecTitle', 'cmfRecComponent'
    ];
    const elements = new Map(ids.map(id => {
        const classes = new Set(id === 'cmfDecisionDetailsBody' ? ['cmf-rec-hidden'] : []);
        return [id, {
            textContent: '',
            innerHTML: '',
            style: {},
            classList: {
                add: value => classes.add(value),
                contains: value => classes.has(value),
                toggle(value, hidden) {
                    if (hidden) classes.add(value);
                    else classes.delete(value);
                }
            }
        }];
    }));
    elements.get('cmfRecReasoningSection').style.display = 'none';
    const requests = [];
    const context = vm.createContext({
        document: { getElementById: id => elements.get(id) },
        console,
        getIssuePendingPlatformValue: () => 'CMF_OVERALL_ALL_COMPONENTS_TABLE',
        buildCmfPendingRowsContext: () => 'Filtered visible sightings',
        renderCmfBriefList: items => items.join('|'),
        renderCmfRecommendationHighlights: text => text,
        fetch: async (url, options) => {
            requests.push({ url, body: JSON.parse(options.body) });
            return { json: async () => ({ d: response }) };
        }
    });
    for (const name of ['setCmfDrawerMode', 'openCmfPendingInsights', 'openCmfPendingImpactModal']) {
        const definition = source.match(new RegExp(
            '        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'
        ));
        assert.ok(definition, name + ' must remain defined in the portal');
        vm.runInContext(definition[0], context);
    }
    return { context, elements, requests };
}

test('the integrated inline JavaScript is syntactically valid', () => {
    const script = source.match(/<script type="text\/javascript">([\s\S]*?)<\/script>/);
    assert.ok(script);
    new vm.Script(script[1].replace(/<%[\s\S]*?%>/g, '0'));
});

test('pending summary normalizes missing iDST placeholders before counting and building issue links', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const expressions = Array.from(server.matchAll(/CASE\s+WHEN src_comp\.idst IS NULL[\s\S]*?END AS idst_name/g), match => match[0]);
    assert.equal(expressions.length, 2);
    assert.equal(expressions[0], expressions[1]);
    assert.match(expressions[0], /LOWER\(LTRIM\(RTRIM\(src_comp\.idst\)\)\) = 'no idst assigned' THEN 'Unassigned'/);
    assert.match(expressions[0], /ELSE LTRIM\(RTRIM\(src_comp\.idst\)\)/);
    assert.match(server, /GROUP BY component_group, idst_name/);
});

test('weekly Closed bars exclude duplicates and use closure dates for complete and rejected records', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const closed = server.match(/resolved_issues AS \(([\s\S]*?)\r?\n\),\r?\nimplemented_issues AS/);
    assert.ok(closed);
    assert.match(closed[1], /BuildWeeklyClosedPredicate\("src", "w.week_offset"\)/);
    assert.doesNotMatch(closed[1], /implemented_date/);
    assert.match(closed[1], /JOIN " \+ weeklyFlowSourceSql \+ @"/);
    assert.match(server, /string weeklyFlowSourceSql = "\(" \+ BuildSummaryIssueComponentSourceSql\(dashboardPlatform\)/);
    assert.match(server, /string weeklyFlowPredicate = BuildSummaryIssueRecordTypePredicate\("src", true, false\)/);
    const implemented = server.match(/implemented_issues AS \(([\s\S]*?)\r?\n\)\r?\nSELECT/);
    assert.ok(implemented);
    assert.match(implemented[1], /TRY_CAST\(implemented_date AS DATE\)/);
    assert.doesNotMatch(implemented[1], /closed_date/);
    const drilldown = server.match(/    public static SummaryIssueLinksResponse GetTrendWeekIssueLinks\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(drilldown);
    assert.match(drilldown[0], /BuildWeeklyClosedPredicate\("src", "@week_offset"\)/);
    assert.match(drilldown[0], /bool includeDuplicates = false/);
    assert.doesNotMatch(drilldown[0], /TRY_CAST\(src\.implemented_date AS DATE\)|<> 'rejected'/);
    assert.match(drilldown[0], /BuildTrendWeekEndStatusPredicate\("src", cutoffSql, false, "@week_offset"\)/);
    assert.match(drilldown[0], /BuildTrendWeekEndStatusPredicate\("src", cutoffSql, true, "@week_offset"\)/);
});

test('weekly Open bars and details retain all request-week intake regardless of resolution', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const open = server.match(/new_issues AS \(([\s\S]*?)\r?\n\),\r?\nresolved_issues AS/);
    assert.ok(open);
    assert.match(open[1], /CmfSummaryTrendService\.BuildWeeklyOpenPredicate\("src", "w.week_offset"\)/);
    assert.doesNotMatch(open[1], /status.*= 'open'/);
    assert.match(server, /CmfSummaryTrendService\.BuildWeeklyOpenPredicate\("src", "@week_offset"\)/);
    assert.match(server, /weekPredicate = allowOpen && allowClosed \? "\(\(" \+ openPredicate/);
    assert.match(server, /: allowOpen \? openPredicate : closedPredicate/);
    const service = fs.readFileSync(path.join(__dirname, '..', '..', 'App_Code', 'CmfSummaryTrendService.cs'), 'utf8');
    const predicate = service.match(/    public static string BuildWeeklyOpenPredicate\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(predicate);
    assert.match(predicate[0], /date_cmf_ask/);
    assert.doesNotMatch(predicate[0], /implemented_date|closed_date|status|cutoff/);
});

test('one shared Updated timestamp follows Mode outside tab-specific panels', () => {
    const header = source.match(/<header class="portal-app-header">([\s\S]*?)<\/header>/);
    assert.ok(header);
    assert.match(header[1], /<div class="header-user-mode">[\s\S]*?ID="ddlUserMode"[\s\S]*?<\/asp:DropDownList>\s*<div class="portal-header-updated">Updated <asp:Label ID="lblHomeDashboardGeneratedAt"/);
    assert.ok(header[1].indexOf('lblHomeDashboardGeneratedAt') < header[1].indexOf('ID="dashboardHeaderTools"'));
    assert.equal((source.match(/ID="lblHomeDashboardGeneratedAt"/g) || []).length, 1);
    assert.doesNotMatch(source, /lblCmfSummaryGeneratedAt|dashboard-platform-updated/);
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    assert.match(server, /lblHomeDashboardGeneratedAt.Text = snapshot.GeneratedAt;/);
    assert.doesNotMatch(server, /lblCmfSummaryGeneratedAt|lblHomeDashboardGeneratedAt.Visible\s*=\s*false/);
    const theme = fs.readFileSync(path.join(__dirname, '..', '..', 'Content', 'cmf-portal-theme.css'), 'utf8');
    assert.match(theme, /body \.portal-app-header \.portal-header-updated \{[^}]*white-space: nowrap;/);
});

test('accepted date filtering is fixed for saved sessions and range changes without a basis dropdown', () => {
    assert.match(source, /<asp:DropDownList ID="ddlDashboardDateMode"/);
    assert.match(source, /<% if \(ddlDashboardDateMode.SelectedValue != "all"\) \{ %>\s*<span class="dashboard-date-basis-label">CMFs Accepted<%= ddlDashboardDateMode.SelectedValue == "today" \? string.Empty : " in" %><\/span>\s*<% \} %>\s*<asp:DropDownList ID="ddlDashboardDateMode"/);
    assert.doesNotMatch(source, /ddlDashboardDateBasis/);
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    assert.doesNotMatch(server, /ddlDashboardDateBasis/);
    assert.match(server, /CmfDashboardDateFilter\.TryCreate\(saved == null \? "all" : saved\.Mode,\s*"cmf-accepted",/);
    assert.match(server, /CmfDashboardDateFilter\.TryCreate\(ddlDashboardDateMode\.SelectedValue,\s*"cmf-accepted",/);
    assert.match(server, /BuildPendingPredicate\("pending_dates"\)/);
    assert.match(server, /BuildPendingPredicate\("pending_bucket_src"\)/);
    assert.doesNotMatch(server, /BuildPredicate\("pending_/);
    assert.doesNotMatch(server, /FROM " \+ pendingTable \+ " WHERE " \+ GetDashboardDateFilter\(\)\.BuildPredicate/);
});

test('Summary trends obey date visibility and suppress hover tooltips while preserving accessible comparisons', () => {
    const definition = source.match(/        function renderSummaryWeeklyTrend\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    assert.match(definition[0], /<%= ShowSummaryTrends \? "true" : "false" %>/);
    for (const visible of [false, true]) {
        const context = vm.createContext({ escapeHtml: value => String(value) });
        vm.runInContext(definition[0].replace('<%= ShowSummaryTrends ? "true" : "false" %>', String(visible)), context);
        for (const trend of [
            { Direction: 'up', Tone: 'bad', Display: '1' },
            { Direction: 'down', Tone: 'good', Display: '2' },
            { Direction: 'flat', Tone: 'neutral', Display: '0' },
            { Direction: 'unavailable', Tone: 'unavailable', Display: 'N/A' }
        ]) {
            trend.Tooltip = 'Current versus the estimated count seven days earlier.';
            const html = context.renderSummaryWeeklyTrend(trend);
            if (visible) {
                assert.match(html, /class="cmf-weekly-trend /);
                assert.match(html, /title=""/, 'An empty title suppresses both the trend tooltip and inherited card tooltips.');
                assert.match(html, /aria-label="Current versus the estimated count seven days earlier\."/);
            }
            else assert.equal(html, '');
        }
        assert.equal(context.renderSummaryWeeklyTrend(null), '');
        assert.equal(context.renderSummaryWeeklyTrend({ Display: '' }), '');
    }
});

test('CMF summary includes future in Implemented and excludes it from Open and their duplicate counts', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const summary = server.slice(server.indexOf('private void PopulateHomeCmfSummaryTables('));
    for (const column of ['ImplementedCount', 'ImplementedDup']) {
        assert.match(summary, new RegExp("status IN \\('implemented','future'\\) THEN 1 ELSE 0 END\\) AS " + column));
    }
    for (const column of ['PendingCount', 'PendingDup']) {
        assert.match(summary, new RegExp("status NOT IN \\('complete','rejected','implemented','future'\\) THEN 1 ELSE 0 END\\) AS " + column));
    }
    assert.match(summary, /componentSourceSql,\s*"status IN \('implemented','future'\)"/);
    assert.match(summary, /componentSourceSql,\s*"status NOT IN \('complete','rejected','implemented','future'\)"/);
    const drilldown = server.match(/    public static SummaryIssueLinksResponse GetSummaryCardIssueLinks\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(drilldown);
    assert.match(drilldown[0], /src.status, ''\)\)\)\) IN \('implemented', 'future'\)/);
    assert.match(drilldown[0], /src.status, ''\)\)\)\) NOT IN \('complete', 'rejected', 'implemented', 'future'\)/);
});

test('OEM summary and drilldowns group future with Implemented instead of Open', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const summary = server.match(/    private void PopulateOverallOemSummary\([^)]*\)[\s\S]*?\r?\n    \}/);
    const drilldown = server.match(/    private DataTable GetOemSummaryModalData\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(summary);
    assert.ok(drilldown);
    assert.match(summary[0], /status_norm NOT IN \('complete', 'rejected', 'implemented', 'future'\)/);
    assert.match(summary[0], /status_norm IN \('implemented', 'future'\)/);
    assert.match(drilldown[0], /issue_src.status, ''\)\)\)\) NOT IN \('complete', 'rejected', 'implemented', 'future'\)/);
    assert.match(drilldown[0], /issue_src.status, ''\)\)\)\) IN \('implemented', 'future'\)/);
});

test('shared tables and Issue List include future in implementation buckets and visibility', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const summary = server.match(/    private void PopulateHomeCmfSummaryTables\([^)]*\)[\s\S]*?\r?\n    \}/);
    const milestone = server.match(/    private void PopulateMilestoneWiseComponentSummary\([^)]*\)[\s\S]*?\r?\n    \}/);
    const issueGrid = server.match(/    private void BindIssueGridFromDataTable\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(summary);
    assert.ok(milestone);
    assert.ok(issueGrid);
    for (const table of [summary[0], milestone[0]]) {
        assert.match(table, /status IN \('open','implemented','verified','future'\)/);
        assert.match(table, /status IN \('implemented','verified','future'\) AND cmf_request = 'cmf_ok'/);
        assert.doesNotMatch(table, /status IN \('(?:open',')?implemented','verified'\)/);
    }
    assert.match(summary[0], /status IN \('implemented','verified','future'\) AND cmf_request = 'cmf_duplicate'/);
    assert.match(server, /string.Equals\(statusNorm, "future", StringComparison.OrdinalIgnoreCase\)/);
    assert.match(server, /modalQuery.Append\("AND status IN \('implemented', 'verified', 'future'\)"\)/);
    assert.match(issueGrid[0], /status == "implemented" \|\| status == "verified" \|\| status == "future"/);
    assert.match(server, /\? "\(status IN \('open', 'implemented', 'future'\) OR/);
    assert.match(server, /: "status IN \('open', 'implemented', 'future'\)";/);
    assert.match(server, /" AND status in \('open','implemented','future'\) "/);
});

test('all live implementation count predicates and component drilldowns include future', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const active = server.split(/\r?\n/).filter(line => !/^\s*\/\//.test(line)).join('\n');
    assert.doesNotMatch(active, /\bstatus\s+IN\s*\(\s*'(?:open'\s*,\s*')?implemented'\s*,\s*'verified'\s*\)/i);
    assert.doesNotMatch(active, /\bstatus\s+IN\s*\(\s*'open'\s*,\s*'implemented'\s*\)/i);
    assert.doesNotMatch(active, /\[status\]\s*=\s*'implemented'/i);
    assert.doesNotMatch(active, /GetDistinctDrivers\(platformTable, "open", "implemented"(?:, "verified")?\)/);
    assert.match(active, /DataTable dt = GetCMFIssuesData\(component, driver, issueType\);/);
    for (const name of ['BindCMFIssues', 'GetCMFIssuesData']) {
        const method = active.match(new RegExp('    private \\S+ ' + name + '\\([^)]*\\)[\\s\\S]*?(?=\\n    (?:private|protected|public) |$)'));
        assert.ok(method, name);
        assert.equal((method[0].match(/status IN \('implemented','verified','future'\)/g) || []).length, 6);
        assert.match(method[0], /cmf_request = 'cmf_duplicate'/);
        assert.match(method[0], /cmf_request IN \('cmf_ok', 'cmf_duplicate'\)/);
        assert.match(method[0], /cmf_request = 'cmf_ok'/);
        assert.match(method[0], /status IN \('open'\)/);
    }
});

test('analytics summaries and count answers group future with Implemented', context => {
    const { spawnSync } = require('node:child_process');
    const script = path.join(__dirname, '..', 'analytics_chatbot.py');
    const result = spawnSync(process.env.CMF_TEST_PYTHON || 'python', ['-c', `
import ast
import collections
import pathlib
import sys

tree = ast.parse(pathlib.Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
names = {"_extract_status_tokens", "_summary_message", "_deterministic_rag_answer"}
helpers = ast.Module(body=[node for node in tree.body if isinstance(node, ast.FunctionDef) and node.name in names], type_ignores=[])
scope = {"HAS_PANDAS": False, "_extract_platform_tokens": lambda prompt: [], "_is_graph_request": lambda prompt: False, "_is_count_question": lambda prompt: True}
exec(compile(helpers, str(sys.argv[1]), "exec"), scope)
statuses = ["future", " Future ", "implemented", "open", "complete", "rejected"]
rows = [{"status": status} for status in statuses]
fallback = scope["_summary_message"](rows)
for expected in ["Total issues analyzed: 6", "Open: 1", "Implemented: 3", "Closed (complete/rejected): 2"]:
    assert expected in fallback, fallback

class Mask(list):
    def sum(self):
        return sum(self)
    def __invert__(self):
        return Mask(not value for value in self)

class Series(list):
    @property
    def str(self):
        return self
    def fillna(self, value):
        return Series(value if item is None else item for item in self)
    def astype(self, kind):
        return Series(kind(item) for item in self)
    def strip(self):
        return Series(item.strip() for item in self)
    def lower(self):
        return Series(item.lower() for item in self)
    def value_counts(self):
        return collections.Counter(self)
    def isin(self, values):
        return Mask(item in values for item in self)
    def __eq__(self, value):
        return Mask(item == value for item in self)

class Frame:
    columns = ["status"]
    def __len__(self):
        return len(statuses)
    def __getitem__(self, column):
        assert column == "status"
        return Series(statuses)

scope["HAS_PANDAS"] = True
frame = Frame()
summary = scope["_summary_message"](frame)
for expected in ["Total issues analyzed: 6", "Open: 1", "Implemented: 3", "Closed (complete/rejected): 2"]:
    assert expected in summary, summary
for status, count in [("implemented", 3), ("open", 1), ("pending", 1), ("future", 2)]:
    message, image = scope["_deterministic_rag_answer"](frame, "How many " + status + " issues?", None, None, None, "")
    assert "- " + status + ": " + str(count) + " records" in message, message
    assert image == ""
print("Analytics future-status grouping passed for both summary paths and count answers.")
`, script], { encoding: 'utf8', env: { ...process.env, PYTHONDONTWRITEBYTECODE: '1' } });
    if (result.error && result.error.code === 'ENOENT') {
        context.skip('Python interpreter unavailable; set CMF_TEST_PYTHON to run analytics regressions.');
        return;
    }
    assert.ifError(result.error);
    assert.equal(result.status, 0, result.stderr || result.stdout);
    assert.match(result.stdout, /Analytics future-status grouping passed/);
});

test('all Open percentage numerators and their drilldowns use the Summary Open bucket', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const percentages = Array.from(server.matchAll(/AS CMFOpenPercentage/g));
    assert.equal(percentages.length, 6);
    for (const match of percentages) {
        const start = Math.max(server.lastIndexOf('100.0 * SUM(CASE', match.index), server.lastIndexOf('ELSE CAST(ROUND(', match.index));
        assert.ok(start >= 0);
        const numerator = server.slice(start, match.index);
        assert.equal((numerator.match(/status(?:_norm)? NOT IN\s*\('complete', 'rejected', 'implemented', 'future'\)/g) || []).length, 2);
        assert.doesNotMatch(numerator, /status(?:_norm)? NOT IN\s*\('complete', 'rejected'\)/);
    }
    for (const name of ['GetDesignSummaryModalData', 'GetIngredientSummaryModalData']) {
        const method = server.match(new RegExp('    private DataTable ' + name + '\\([^)]*\\)[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(method);
        assert.match(method[0], /case "trg4":[\s\S]*?status NOT IN\('complete', 'rejected', 'implemented', 'future'\)/);
    }
    assert.match(source, /Calculated as count where status is implemented or future\./);
    assert.match(source, /CMFs that are not complete, rejected, implemented, or future\./);
});

test('all summary metrics use same-weekday scoped estimates and TPT subtracts whole-day headlines', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const summary = server.slice(server.indexOf('private void PopulateHomeCmfSummaryTables('));
    assert.match(summary, /componentSourceSql = BuildDashboardComponentSourceSql\(platformTable, true\)/);
    assert.match(summary, /balanceTrends = CmfSummaryTrendService\.LoadBalanceEstimates\(\s*con, componentSourceSql, DateTime.Today\)/);
    assert.match(summary, /tptTrends = CmfSummaryTrendService\.LoadTptEstimates\(\s*con, componentSourceSql, DateTime.Today\)/);
    for (const [label, headline] of [['Total CMFs', 'total'], ['Closed', 'closed'], ['Implemented', 'implemented'], ['Open', 'pending']]) {
        const fact = summary.match(new RegExp('Label = "' + label + '",([\\s\\S]*?)\\n                    \\}\\);'));
        assert.ok(fact, label + ' summary card remains defined');
        assert.ok(fact[1].includes('WeeklyTrend = balanceTrends["' + label + '"].WithCurrentCount(' + headline + ')'));
    }
    for (const [label, headline, trend] of [['CMF Disposition TPT', 'dispositionTpt', 'dispositionTrend'],
        ['CMF Resolution TPT', 'resolutionTpt', 'resolutionTrend'], ['CMF Overall TPT', 'overallTpt', 'overallTrend']]) {
        assert.ok(summary.includes('tptTrends["' + label + '"].WithCurrentTpt(' + headline + ')'));
        const fact = summary.match(new RegExp('Label = "' + label + '",([\\s\\S]*?)\\n                    \\}\\);'));
        assert.ok(fact, label + ' summary card remains defined');
        assert.ok(fact[1].includes('WeeklyTrend = ' + trend));
        assert.ok(fact[1].includes('FormatTptTrendNote(' + headline + ' - decimal.ToInt32(' + trend + '.PreviousValue.Value))'));
    }
    assert.doesNotMatch(summary, /weeklyTrends|dispositionThisWeek|resolutionThisWeek|overallThisWeek|CROSS JOIN wk/);
    assert.ok(summary.includes('CmfSummaryTrendService.LoadPendingEstimate('));
});

test('hover tooltips omit click instructions without removing sorting handlers', () => {
    const sorting = fs.readFileSync(path.join(__dirname, '..', 'cmf-portal-field-filter.js'), 'utf8');
    assert.doesNotMatch(source, /\bclick\s+to\b/i);
    assert.doesNotMatch(sorting, /\bclick\s+to\b/i);
    assert.match(sorting, /th\.addEventListener\('click'/);
    new vm.Script(sorting);
});

test('Pending summary tooltips omit oldest-age explanations while retaining the visible age', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const pending = server.match(/Label = "Pending",[\s\S]*?\r?\n\s*\}\);/);
    assert.ok(pending);
    assert.match(pending[0], /Note = oldestPendingAgeDays\.HasValue\s*\? "Oldest: "/);
    assert.match(pending[0], /ConstituentTooltip = pendingConstituentLines\.Count > 0\s*\? string\.Join\("\\n", pendingConstituentLines\.ToArray\(\)\)\s*: "No pending constituent counts in this scope\."/);
});

test('non-TPT summary cards place duplicate notes beneath values and trends in the footer', () => {
    const definition = source.match(/        function renderOverallSummaryFacts\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    for (const hostId of ['homeOverallSummaryFactsCore', 'homeSpecificSummaryFactsDashboardCore', 'homeSpecificSummaryFactsCmfCore']) {
        for (const display of ['1', '0', '']) {
            const cards = [];
            const host = { innerHTML: '', appendChild: card => cards.push(card) };
            const context = vm.createContext({
                document: {
                    getElementById: () => host,
                    createElement: () => ({ innerHTML: '', attributes: {},
                        setAttribute(name, value) { this.attributes[name] = value; }, addEventListener() {} })
                },
                shouldRenderOverallSummaryFact: () => true,
                formatSummaryFactValueAndNote: (value, note) => ({ valueText: value, noteText: note || '' }),
                renderValueWithDaysSuffix: value => String(value),
                escapeHtml: value => String(value),
                getOverallSummaryToneClass: () => 'tone',
                buildOverallSummaryFactTooltip: () => 'Summary details',
                normalizeSummaryIssueMetricKey: label => label.toLowerCase(),
                renderSummaryWeeklyTrend: trend => trend && trend.Display ? '<span class="cmf-weekly-trend">' + trend.Display + '</span>' : ''
            });
            vm.runInContext(definition[0], context);
            context.renderOverallSummaryFacts({ SummaryFacts: [
                { Label: 'Open', Value: '21', Note: '9 duplicates', WeeklyTrend: { Display: display } },
                { Label: 'Pending', Value: '6', Note: 'Oldest: 18 days', WeeklyTrend: { Display: display } },
                { Label: 'Pending', Value: '6', Note: '', WeeklyTrend: { Display: display } }
            ] }, hostId);
            assert.equal(cards.length, 3);
            assert.match(cards[0].innerHTML, /class="cmf-summary-value-stack"><div class="ccip-summary-fact-value">21<\/div><div class="ccip-summary-fact-note">9 duplicates<\/div><\/div>/);
            assert.match(cards[1].innerHTML, /class="cmf-summary-value-stack"><div class="ccip-summary-fact-value">6<\/div><div class="ccip-summary-fact-note">Oldest: 18 days<\/div><\/div>/);
            assert.match(cards[2].innerHTML, /class="ccip-summary-fact-value">6<\/div>/);
            assert.doesNotMatch(cards[2].innerHTML, /cmf-summary-value-stack|CMF pending table/);
            for (const card of cards) {
                assert.equal(card.attributes.title, 'Summary details');
                assert.doesNotMatch(card.attributes['aria-label'], /\bclick\s+to\b/i);
                assert.equal(card.attributes.role, 'button');
                assert.equal(card.attributes.tabindex, '0');
                if (display) assert.ok(card.innerHTML.endsWith('<div class="ccip-summary-fact-note"><span class="cmf-weekly-trend">' + display + '</span></div>'));
                else assert.doesNotMatch(card.innerHTML, /cmf-weekly-trend|N\/A/);
            }
        }
    }
});

test('Overall TPT aligns legends and footer trends with summary cards without changing platform layouts', () => {
    const definition = source.match(/        function renderOverallTptBoxes\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    assert.match(source, /#homeOverallSummaryTptInline \.overall-tpt-value\s*\{\s*font-size: 30px;\s*line-height: 1\.05;/);
    assert.match(source, /#homeOverallSummaryTptInline \.overall-tpt-legend\s*\{\s*font-size: 10px;\s*line-height: 1\.2;/);
    assert.match(source, /#homeOverallSummaryTptInline \.overall-tpt-trend\s*\{\s*display: flex;\s*justify-content: center;\s*align-self: end;\s*min-height: 14px;/);
    for (const hostId of ['homeOverallSummaryTptInline', 'homeSpecificSummaryFactsDashboardTptInline', 'homeSpecificSummaryFactsCmfTptInline']) {
        const host = { id: hostId, innerHTML: '', querySelectorAll: () => [] };
        const context = vm.createContext({
            document: { getElementById: () => host },
            findTptFactByLabel: (facts, pattern) => facts.find(fact => pattern.test(fact.Label)),
            escapeHtml: value => String(value),
            getLegacyTptValueToneClass: () => 'tone',
            buildTptColorLegend: kind => '<div class="overall-tpt-legend">' + kind + ' legend</div>',
            renderSummaryWeeklyTrend: trend => trend.Display ? '<span class="cmf-weekly-trend">' + trend.Display + '</span>' : ''
        });
        vm.runInContext(definition[0], context);
        for (const display of ['50%', '0%', '', null]) {
            const metrics = [['disposition', '3'], ['resolution', '16'], ['overall', '41']];
            context.renderOverallTptBoxes(metrics.map(([kind, value]) => ({
                Label: 'CMF ' + kind + ' TPT', Value: value,
                WeeklyTrend: display === null ? null : { Display: display }
            })), hostId);
            for (const [kind, value] of metrics) {
                const card = host.innerHTML.split('data-tpt-kind="' + kind + '"')[1].split('<div class="overall-tpt-box"')[0];
                const legend = '<div class="overall-tpt-legend">' + kind + ' legend</div>';
                const trend = display ? '<span class="cmf-weekly-trend">' + display + '</span>' : '';
                assert.ok(card.includes('>' + value + ' <span'));
                if (hostId === 'homeOverallSummaryTptInline') {
                    assert.ok(card.includes(legend + '</div><div class="overall-tpt-trend">' + trend + '</div>'));
                    assert.match(card, /cmf-summary-value-stack/);
                } else {
                    assert.doesNotMatch(card, /overall-tpt-trend/);
                    assert.ok(card.endsWith((display === null ? '' : trend + '</div>') + legend + '</div>'));
                }
                if (!display) assert.doesNotMatch(card, /cmf-weekly-trend|N\/A/);
            }
        }
    }
});

test('detail and insights modes expose the content area and hide recommendation sections', () => {
    const { context, elements } = createDrawer();
    for (const mode of ['details', 'insights']) {
        context.setCmfDrawerMode(mode);
        assert.equal(elements.get('cmfDecisionDetailsBody').classList.contains('cmf-rec-hidden'), false);
        assert.equal(elements.get('cmfAssistantRecommendationSection').classList.contains('cmf-rec-hidden'), true);
        assert.equal(elements.get('cmfAssistantQualityAssessment').classList.contains('cmf-rec-hidden'), true);
    }
    context.setCmfDrawerMode('recommendation');
    assert.equal(elements.get('cmfDecisionDetailsBody').classList.contains('cmf-rec-hidden'), true);
    assert.equal(elements.get('cmfAssistantRecommendationSection').classList.contains('cmf-rec-hidden'), false);
    assert.equal(elements.get('cmfAssistantQualityAssessment').classList.contains('cmf-rec-hidden'), false);
});

test('Pending Insights displays its response outside the permanently hidden legacy panel', async () => {
    const { context, elements, requests } = createDrawer({ Success: true, Summary: '- Review visible sightings' });
    context.openCmfPendingInsights();
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(elements.get('cmfDecisionDetailsBody').classList.contains('cmf-rec-hidden'), false);
    assert.equal(elements.get('cmfDecisionDetailsBody').innerHTML, 'Review visible sightings');
    assert.equal(elements.get('cmfRecEvidence').innerHTML, '');
    assert.equal(elements.get('cmfRecDrawer').classList.contains('show'), true);
    assert.equal(requests[0].url, 'CMF_Web_portal.aspx/GetCmfPendingListInsights');
    assert.equal(requests[0].body.platform, 'CMF_OVERALL_ALL_COMPONENTS_TABLE');
    assert.equal(requests[0].body.rowsContext, 'Filtered visible sightings');
});

test('Pending Insights service errors remain visible', async () => {
    const { context, elements } = createDrawer({ Success: false, Message: 'Insights service unavailable' });
    context.openCmfPendingInsights();
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(elements.get('cmfDecisionDetailsBody').textContent, 'Insights service unavailable');
    assert.equal(elements.get('cmfDecisionDetailsBody').classList.contains('cmf-rec-hidden'), false);
});

test('Pending impact details display their response in the visible detail area', async () => {
    const { context, elements, requests } = createDrawer({ Success: true, Evidence: 'Customer impact evidence' });
    context.openCmfPendingImpactModal('123', 'Test issue', 'memory', '', 'Impact', '', '', '', '', '');
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(elements.get('cmfDecisionDetailsBody').innerHTML, 'Customer impact evidence');
    assert.equal(elements.get('cmfDecisionDetailsBody').classList.contains('cmf-rec-hidden'), false);
    assert.equal(elements.get('cmfAssistantRecommendationSection').classList.contains('cmf-rec-hidden'), true);
    assert.equal(requests[0].url, 'CMF_Web_portal.aspx/GetCmfPendingImpactDetails');
    assert.equal(requests[0].body.cpId, '123');
});
