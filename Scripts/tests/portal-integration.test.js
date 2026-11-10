const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const source = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx'), 'utf8');

function createDrawer(response) {
    const ids = [
        'cmfRecTitleRow', 'cmfRecSightingRow', 'cmfAssistantSightingDetails',
        'cmfAssistantFixStatusSection', 'cmfAssistantFixStatus',
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

test('all CMF summary trend indicators are omitted when the applied date range is shorter than 30 days', () => {
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
            const html = context.renderSummaryWeeklyTrend(trend);
            if (visible) assert.match(html, /class="cmf-weekly-trend /);
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

test('summary counts use same-weekday scoped balances while TPT retains rolling-week comparisons', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    const summary = server.slice(server.indexOf('private void PopulateHomeCmfSummaryTables('));
    assert.match(summary, /componentSourceSql = BuildDashboardComponentSourceSql\(platformTable, true\)/);
    assert.match(summary, /balanceTrends = CmfSummaryTrendService\.LoadBalanceEstimates\(\s*con, componentSourceSql, DateTime.Today\)/);
    assert.match(summary, /weeklyTrends = CmfSummaryTrendService\.Load\(\s*con, BuildDashboardComponentSourceSql\(platformTable\), DateTime.Today\)/);
    for (const label of ['Total CMFs', 'Closed', 'Implemented', 'Open']) {
        const fact = summary.match(new RegExp('Label = "' + label + '",([\\s\\S]*?)\\n                    \\}\\);'));
        assert.ok(fact, label + ' summary card remains defined');
        assert.ok(fact[1].includes('WeeklyTrend = balanceTrends["' + label + '"]'));
    }
    for (const label of ['CMF Disposition TPT', 'CMF Resolution TPT', 'CMF Overall TPT']) {
        assert.ok(summary.includes('WeeklyTrend = weeklyTrends["' + label + '"]'));
    }
    assert.ok(summary.includes('CmfSummaryTrendService.LoadPendingEstimate('));
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
                    createElement: () => ({ innerHTML: '', setAttribute() {}, addEventListener() {} })
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
                if (display) assert.ok(card.innerHTML.endsWith('<div class="ccip-summary-fact-note"><span class="cmf-weekly-trend">' + display + '</span></div>'));
                else assert.doesNotMatch(card.innerHTML, /cmf-weekly-trend|N\/A/);
            }
        }
    }
});

test('Overall TPT uses the shared indicator renderer without changing its headline', () => {
    const definition = source.match(/        function renderOverallTptBoxes\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    for (const hostId of ['homeOverallSummaryTptInline', 'homeSpecificSummaryFactsDashboardTptInline', 'homeSpecificSummaryFactsCmfTptInline']) {
        const host = { id: hostId, innerHTML: '', querySelectorAll: () => [] };
        const context = vm.createContext({
            document: { getElementById: () => host },
            findTptFactByLabel: (facts, pattern) => facts.find(fact => pattern.test(fact.Label)),
            escapeHtml: value => String(value),
            getLegacyTptValueToneClass: () => 'tone',
            buildTptColorLegend: () => '',
            renderSummaryWeeklyTrend: trend => trend.Display ? '<span class="cmf-weekly-trend">' + trend.Display + '</span>' : ''
        });
        vm.runInContext(definition[0], context);
        for (const display of ['50%', '0%', '']) {
            context.renderOverallTptBoxes([{ Label: 'CMF Overall TPT', Value: '41', WeeklyTrend: { Display: display } }], hostId);
            const overall = host.innerHTML.split('data-tpt-kind="overall"')[1];
            assert.match(overall, /cmf-summary-value-stack/);
            assert.match(overall, />41 <span/);
            if (display) assert.ok(overall.includes('class="cmf-weekly-trend">' + display));
            else assert.doesNotMatch(overall, /cmf-weekly-trend|N\/A/);
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

test('AI CMF Assistant formats scores in threshold-safe 0.5 steps, humanizes occurrence, and deduplicates metric cards', () => {
    const helperContext = vm.createContext({
        escapeHtml: text => String(text || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    });
    for (const name of [
        'normalizeDimensionScore', 'normalizeWeightedCmfScore', 'formatWeightedCmfScore',
        'humanizeCmfOccurrenceFactor', 'buildDistinctClientMetricFactor',
        'extractBriefCmfContributingFactor', 'buildCmfMetricDisplayName',
        'getCmfMetricScoreClass', 'renderCmfMetricChips', 'renderAssistantRichText'
    ]) {
        const definition = source.match(new RegExp(
            '        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'
        ));
        assert.ok(definition, name + ' must be defined');
        vm.runInContext(definition[0], helperContext);
    }

    // 3.45 is in CMF_REVIEW (< 3.50 on 4-point scale) so it must snap to '3.25', never '3.5'
    assert.equal(helperContext.formatWeightedCmfScore(3.45), '3.25');
    assert.equal(helperContext.formatWeightedCmfScore(3.50), '3.5');
    assert.equal(helperContext.formatWeightedCmfScore(4.0), '4');
    assert.equal(helperContext.formatWeightedCmfScore(2.70), '2.5');
    assert.equal(helperContext.formatWeightedCmfScore(1.0), '1');

    // Raw database occurrence category is humanized
    assert.equal(
        helperContext.extractBriefCmfContributingFactor({
            RuleId: 'O',
            Score: '2',
            Evaluation: 'Contributing factor: very_sporadic_<10%. Evidence: very_sporadic_<10%'
        }),
        'Rare (<10% of tries)'
    );

    // Duplicate phrases across S, D, and CX are automatically differentiated
    const chipsHtml = helperContext.renderCmfMetricChips([
        { RuleId: 'S', RuleName: 'Severity', Score: '4', Evaluation: 'Contributing factor: System hang on TBT plug. Evidence: hang' },
        { RuleId: 'D', RuleName: 'Detection', Score: '5', Evaluation: 'Contributing factor: System hang on TBT plug. Evidence: TBT plug' },
        { RuleId: 'CX', RuleName: 'Customer Experience', Score: '5', Evaluation: 'Contributing factor: System hang on TBT attach. Evidence: hang' }
    ]);
    assert.match(chipsHtml, /Normal USB-C \/ TBT plug-in/);
    assert.match(chipsHtml, /Laptop becomes unusable/);

    // Stray single asterisk on CMF_REVIEW is repaired cleanly
    assert.equal(
        helperContext.renderAssistantRichText('CMF_REVIEW* because of balanced factors.'),
        '<strong>CMF_REVIEW</strong> because of balanced factors.'
    );
});

test('AI CMF Assistant supports CMF_DUPLICATE tag and hides SQA category boxes when all fields are complete', () => {
    const tagNode = { className: '', textContent: '', setAttribute: () => {}, removeAttribute: () => {} };
    const sqaContext = vm.createContext({
        window: {},
        document: { getElementById: id => id === 'cmfAssistantRecTag' ? tagNode : null },
        escapeHtml: text => String(text || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    });
    for (const name of [
        'normalizeCmfRecommendationLabel', 'updateCmfAssistantTag',
        'getQualityStateClass', 'formatSqaScoreOutOfTen', 'isSqaCoreField',
        'buildSqaCategoryItems', 'renderSqaCategoryCard', 'normalizeSqaDimensionScores',
        'buildIssuesFromQualityFields', 'normalizeSightingQualityAssessment',
        'renderSightingQualityAssessment'
    ]) {
        const definition = source.match(new RegExp(
            '        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'
        ));
        assert.ok(definition, name + ' must be defined');
        vm.runInContext(definition[0], sqaContext);
    }

    assert.equal(sqaContext.normalizeCmfRecommendationLabel('cmf_duplicate'), 'CMF_DUPLICATE');
    sqaContext.updateCmfAssistantTag('cmf_duplicate');
    assert.equal(tagNode.textContent, 'CMF_DUPLICATE');
    assert.match(tagNode.className, /\bis-duplicate\b/);

    // Main SQA card shows compact Quality Score + View details button without the 3 category boxes
    const sqaHtml = sqaContext.renderSightingQualityAssessment({
        QualityScore: 95,
        Fields: [{ FieldName: 'SoC Stepping', Status: 'Partial', Characteristic: 'SoC stepping not explicitly called out' }]
    }, {});
    assert.doesNotMatch(sqaHtml, /cmf-sqa-category-grid/);
    assert.match(sqaHtml, /Quality Score: 9\.5\/10/);
    assert.match(sqaHtml, /showSightingQualityDetails\(\)/);

    // CMF Pending sidebar uses hourglass-half icon instead of clipboard-check
    assert.match(source, /id="lnkNavPendingList"[\s\S]*?fa-hourglass-half/i);

    // Overall Score includes the click-triggered Score to Tag Guide tooltip on the 'i' button
    assert.match(source, /cmf-score-band-tooltip/);
    assert.match(source, /Score to Tag Guide/);
    assert.match(source, /toggleCmfScoreBandTooltip\(event\)/);
    assert.doesNotMatch(source, /\.cmf-assistant-overall-score:hover\s+\.cmf-score-band-tooltip/);
});

test('Impact Summary preserves hyphenated test names and Reasoning preserves bold lead-ins', () => {
    const textContext = vm.createContext({
        escapeHtml: text => String(text || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    });
    for (const name of [
        'renderAssistantRichText', 'normalizeImpactFixLine', 'renderImpactSummaryText',
        'extractCmfReasoningText', 'buildCmfModelReasoningBullets'
    ]) {
        const definition = source.match(new RegExp(
            '        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'
        ));
        assert.ok(definition, name + ' must be defined');
        vm.runInContext(definition[0], textContext);
    }

    const impactHtml = textContext.renderImpactSummaryText(
        "- **Validation & customer blockage:** Lenovo's HLK FVT System - PNP (disable/enable) with IO Before/After reliability test is blocked by BSOD 0x50.\n- **Real-world & program risk:** End users could lose unsaved work."
    );
    assert.match(impactHtml, /System - PNP \(disable\/enable\)/);
    assert.equal((impactHtml.match(/<li>/g) || []).length, 2);

    const reasoningBullets = textContext.buildCmfModelReasoningBullets(
        "**Key risk drivers:** Intel SoundWire driver causes BSOD.\n*Why the score is 4/5:** High severity and 100% repro.",
        {}
    );
    assert.equal(reasoningBullets[0], '**Key risk drivers:** Intel SoundWire driver causes BSOD.');
    assert.equal(reasoningBullets[1], '**Why the score is 4/5:** High severity and 100% repro.');
});

test('AI CMF Assistant drawer places Current Status and Impact Summary between Problem Description and CMF Recommendation with consistent typography', () => {
    const descIdx = source.indexOf('id="cmfAssistantSightingDetails"');
    const fixIdx = source.indexOf('id="cmfAssistantFixStatusSection"');
    const impactIdx = source.indexOf('id="cmfAssistantImpactSection"');
    const recIdx = source.indexOf('id="cmfAssistantRecommendationSection"');
    const sqaIdx = source.indexOf('id="cmfAssistantQualityAssessment"');
    assert.ok(descIdx > 0 && fixIdx > descIdx && impactIdx > fixIdx && recIdx > impactIdx && sqaIdx > recIdx);
    assert.match(source, /<h3>Current Status<\/h3>/);
    assert.match(source, /id="cmfAssistantPromotedId"/);
    assert.match(source, /id="cmfAssistantPromotedHsdLink"/);
    assert.match(source, /\.cmf-assistant-impact-summary\s*\{[\s\S]*?color:\s*#28425c;[\s\S]*?text-align:\s*left;/);
    assert.match(source, /\.cmf-assistant-impact-summary strong\s*\{[\s\S]*?font-size:\s*inherit;[\s\S]*?font-weight:\s*700;/);
    assert.match(source, /\.cmf-score-band-tooltip strong\s*\{[\s\S]*?color:\s*#7dd3fc;/);
});

test('Ask CMF Pulse global assistant includes pulse orb logo, single bottom-right floating launcher with Analyze. Generate. Assist. tooltip, template selector, and feedback controls', () => {
    assert.doesNotMatch(source, /id="cmfPulseHeaderLauncher"/);
    assert.match(source, /id="cmfPulseFloatingLauncher"[^>]*title="Analyze\. Generate\. Assist\."/);
    assert.match(source, /id="cmfPulseDrawer"/);
    assert.match(source, /class="cmf-pulse-orb"/);
    assert.match(source, /<span>Ask CMF Pulse<\/span>/);
    assert.match(source, /id="cmfPulseTemplateSelect"/);
    assert.match(source, /function submitCmfPulseFeedback\(/);
    assert.match(source, /function saveCustomCmfPulseTemplate\(/);
    assert.match(source, /ID="fuScoringExcel"/);
    assert.match(source, /ID="btnImportScoringExcel"/);
    assert.match(source, /function openCmfPulseDrawer\(/);
    assert.match(source, /function sendCmfPulsePrompt\(/);
});
