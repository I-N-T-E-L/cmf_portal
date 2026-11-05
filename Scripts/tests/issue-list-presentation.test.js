const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const root = path.join(__dirname, '..', '..');
const page = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx'), 'utf8');
const server = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx.cs'), 'utf8');
const theme = fs.readFileSync(path.join(root, 'Content', 'cmf-portal-theme.css'), 'utf8');

test('ingredient risk heatmap orders platforms by descending total without misaligning cells', () => {
    const host = { innerHTML: '', querySelectorAll: () => [], querySelector: () => null };
    const portal = { overallRiskSubtractLos: false };
    const context = vm.createContext({
        document: { getElementById: () => host },
        window: { CMF_PORTAL: portal },
        normalizeComponentRiskThresholds: () => ({ low: 0, med: 3, high: 6 }),
        getOverallHeatClassFromOpenUniqueByThreshold: value => 'risk-' + value,
        escapeHtml: value => String(value)
    });
    const definition = page.match(/        function renderOverallHeatmap\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    const snapshot = {
        HeatmapPlatforms: ['Zero', 'Beta', 'Alpha', 'Top', 'Blank'],
        ComponentRiskHeatmap: [
            { Ingredient: 'Driver', Values: [0, 3, 2, 6], StatusBreakdowns: ['', 'Beta Driver LOS: 2', 'Alpha Driver LOS: 1', 'Top Driver LOS: 6'] },
            { Ingredient: 'Firmware', Values: [0, 1, 2, 1], StatusBreakdowns: ['', 'Beta Firmware', 'Alpha Firmware', 'Top Firmware'] }
        ]
    };
    const original = JSON.stringify(snapshot);
    for (const subtractLos of [false, true]) {
        portal.overallRiskSubtractLos = subtractLos;
        context.renderOverallHeatmap(snapshot);
        const headers = Array.from(host.innerHTML.matchAll(/class="overall-heatmap-platform">([^<]*)/g), match => match[1]);
        assert.deepEqual(headers, ['Top', 'Alpha', 'Beta', 'Blank', 'Zero']);
        const tiles = Array.from(host.innerHTML.matchAll(/<div class="overall-heatmap-tile ([^"]*)"([^>]*)>([^<]*)<\/div>/g));
        assert.deepEqual(tiles.map(tile => tile[3]), subtractLos
            ? ['6 (6)', '2 (1)', '3 (2)', '0', '0', '1', '2', '1', '0', '0']
            : ['6', '2', '3', '0', '0', '1', '2', '1', '0', '0']);
        assert.ok(tiles[0][1].includes(subtractLos ? 'risk-0' : 'risk-6'));
        for (const [tileIndex, platform, ingredient] of [[0, 'Top', 'Driver'], [1, 'Alpha', 'Driver'], [2, 'Beta', 'Driver'], [5, 'Top', 'Firmware'], [6, 'Alpha', 'Firmware'], [7, 'Beta', 'Firmware']]) {
            assert.ok(tiles[tileIndex][2].includes('data-platform-label="' + platform + '"'));
            assert.ok(tiles[tileIndex][2].includes('data-ingredient="' + ingredient + '"'));
            assert.ok(tiles[tileIndex][2].includes('title="' + platform + ' ' + ingredient));
        }
        assert.equal(JSON.stringify(snapshot), original);
    }
    context.renderOverallHeatmap({ HeatmapPlatforms: [], ComponentRiskHeatmap: [] });
    assert.ok(host.innerHTML.includes('No heatmap data available.'));
});

test('OEM rolling-week trends display signed request counts and red increases instead of percentages', () => {
    const host = { innerHTML: '' };
    const context = vm.createContext({
        document: { getElementById: () => host },
        escapeHtml: value => String(value),
        renderOverallOemNameCell: value => String(value)
    });
    const definition = page.match(/        function renderOverallOemTable\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    context.renderOverallOemTable([
        { OemName: 'Increase', OpenIssues: 3, TrendCount: 5, RecentRequestCount: 15, PreviousRequestCount: 10 },
        { OemName: 'Decrease', OpenIssues: 2, TrendCount: -3, RecentRequestCount: 7, PreviousRequestCount: 10 },
        { OemName: 'Flat', OpenIssues: 1, TrendCount: 0, RecentRequestCount: 2, PreviousRequestCount: 2 },
        { OemName: 'New', OpenIssues: 0, TrendCount: 2, RecentRequestCount: 2, PreviousRequestCount: 0 }
    ]);
    const trends = Array.from(host.innerHTML.matchAll(/<span class="overall-oem-trend ([^"]*)" title="([^"]*)">[\s\S]*?<span class="overall-oem-trend-value">([^<]*)<\/span>/g));
    assert.deepEqual(trends.map(trend => trend[3]), ['+5', '-3', '0', '+2']);
    assert.deepEqual(trends.map(trend => trend[1]), ['overall-trend-up', 'overall-trend-down', 'overall-trend-flat', 'overall-trend-up']);
    assert.ok(trends[0][2].includes('last 7 days (including today): 15; previous 7 days: 10; change: +5'));
    assert.ok(trends[3][2].includes('previous 7 days: 0; change: +2'));
    assert.ok(trends.every(trend => !trend[3].includes('%')));
    assert.ok(host.innerHTML.includes('(7d vs Previous 7d)'));
    assert.match(page, /\.overall-oem-trend\s*\{[^}]*display:\s*inline-flex;[^}]*column-gap:\s*0\.5em;[^}]*justify-content:\s*center;/);
    assert.match(page, /\.overall-oem-trend-arrow:empty\s*\{\s*display:\s*none;/);
    const trendValueStyle = page.match(/\.overall-oem-trend-value\s*\{([^}]*)\}/);
    assert.ok(trendValueStyle && !/min-width/.test(trendValueStyle[1]));
    assert.match(page, /\.overall-trend-up\s*\{\s*color:\s*#b42318;/);
    assert.match(page, /\.overall-trend-down\s*\{\s*color:\s*#067647;/);
    context.renderOverallOemTable([]);
    assert.ok(host.innerHTML.includes('No OEM/customer summary data available.'));
});

test('OEM request trends use fixed rolling windows separately from dashboard-filtered totals', () => {
    const definition = server.match(/    private void PopulateOverallOemSummary\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(definition);
    assert.match(definition[0], /overallComponentSourceSql = BuildDashboardComponentSourceSql\(OverallDashboardPlatformTable, true\)/);
    assert.match(definition[0], /overallTrendSourceSql = BuildDashboardComponentSourceSql\(OverallDashboardPlatformTable\)/);
    assert.match(definition[0], /ask_date >= DATEADD\(DAY, -6, @today\)\s+AND ask_date < DATEADD\(DAY, 1, @today\)/);
    assert.match(definition[0], /ask_date >= DATEADD\(DAY, -13, @today\)\s+AND ask_date < DATEADD\(DAY, -6, @today\)/);
    assert.match(definition[0], /ISNULL\(t\.recent_requests, 0\) - ISNULL\(t\.previous_requests, 0\) AS trend_count/);
    assert.match(definition[0], /command\.Parameters\.Add\("@today", SqlDbType\.Date\)\.Value = DateTime\.Today/);
    assert.doesNotMatch(definition[0], /DATEPART\(WEEKDAY|trend_percent/);
});

test('header stays outside native workspace horizontal scrolling without JavaScript correction', () => {
    const mainScript = fs.readFileSync(path.join(root, 'Scripts', 'cmf-portal-main.js'), 'utf8');
    assert.match(page, /<\/header>[\s\S]*?<div class="container-scroller">/);
    assert.match(theme, /body \.container-scroller \{\s*overflow: visible !important;\s*\}/);
    assert.match(theme, /body #portalLayoutShell > \.portal-main-workspace \{\s*overflow-x: auto !important;/);
    assert.match(theme, /body \.portal-app-header \{\s*position: relative !important;\s*top: auto !important;/);
    assert.doesNotMatch(mainScript, /syncHeaderHorizontalPosition|header\.style\.setProperty\('left'/);
});

test('left menu uses native sticky positioning without scroll-frame corrections', () => {
    const mainScript = fs.readFileSync(path.join(root, 'Scripts', 'cmf-portal-main.js'), 'utf8');
    assert.match(theme, /body #mainMenuSidebar \{\s*position: sticky !important;/);
    assert.match(theme, /body:not\(\.modal-open\) \{\s*overflow: visible !important;/);
    assert.doesNotMatch(mainScript, /function (?:sync|clear)SidebarViewportStickiness|sidebar\.style\.setProperty\('(?:top|position)'/);
});

test('AI Insights keeps content-independent card dimensions and internal scrolling', () => {
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    assert.match(styles, /#homeOverallLayoutV2 > \.overall-ai-span-card,\s*#homeLegacyAiHealthCard\s*\{\s*contain: size;\s*overflow: hidden;/);
    assert.match(styles, /#homeLegacyAiHealthCard\s*\{\s*min-height: var\(--ccip-ai-fixed-height\) !important;\s*max-height: var\(--ccip-ai-fixed-height\) !important;/);
    assert.match(styles, /\.cmf-insights\s*\{[^}]*min-width: 0;[^}]*min-height: 0;[^}]*overflow: auto;/);
    assert.match(styles, /\.ci-toolbar\s*\{[^}]*flex: 0 0 auto;/);
    assert.match(styles, /\.ci-content\s*\{\s*flex: 1 1 0;\s*min-width: 0;\s*min-height: 0;\s*max-height: none;\s*overflow: auto;/);
    assert.doesNotMatch(styles, /\.ci-content\s*\{[^}]*max-height:\s*\d/);
    assert.match(styles, /@media \(max-width: 900px\)\s*\{\s*#homeOverallLayoutV2 > \.overall-ai-span-card\s*\{\s*height: 503\.85px;/);
});

test('AI Insights scope sits beside each heading at 13px without the acceptance-date label', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    new vm.Script(script);
    for (const hostId of ['homeOverallInsights', 'homePlatformInsights']) {
        assert.match(page, new RegExp('AI INSIGHTS</span><span id="' + hostId + 'Scope" class="ci-scope"></span>'));
    }
    assert.match(styles, /\.ci-heading\s*\{[^}]*align-items: baseline;[^}]*flex-wrap: wrap;/);
    assert.match(styles, /\.ci-scope\s*\{[^}]*font-size: 13px;/);
    assert.doesNotMatch(script, /data\.DateRange|<span class="ci-scope">/);
    const states = [];
    const elements = new Map();
    for (const hostId of ['homeOverallInsights', 'homePlatformInsights']) {
        const parts = new Map(['.ci-content', '.ci-mode', '.ci-metadata', '.ci-time', '.ci-provenance', '.ci-refresh'].map(selector => [selector, {
            textContent: '', innerHTML: '', setAttribute() {}, addEventListener() {}, querySelectorAll: () => []
        }]));
        elements.set(hostId, { id: hostId, isConnected: true, innerHTML: '', querySelector: selector => parts.get(selector), querySelectorAll: () => [] });
        elements.set(hostId + 'Scope', { textContent: '' });
    }
    const portal = { currentDashboardPlatform: '' };
    const context = vm.createContext({
        active: null,
        window: { CMF_PORTAL: portal },
        document: { getElementById: id => elements.get(id) },
        load: state => states.push(state),
        clearTimeout() {}, setTimeout() {},
        pagination: () => '', findings: () => '', escape: value => String(value)
    });
    for (const name of ['render', 'draw']) {
        const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    for (const [platform, hostId, label] of [
        ['CMF_OVERALL_ALL_COMPONENTS_TABLE', 'homeOverallInsights', 'Overall (12 included platforms)'],
        ['CMF_NVL_S_ALL_COMPONENTS_TABLE', 'homePlatformInsights', 'NVL-S']
    ]) {
        portal.currentDashboardPlatform = platform;
        context.render({ GeneratedAt: '2026-11-04T12:00:00Z', PlatformLabel: label });
        const state = states.at(-1);
        assert.equal(elements.get(hostId + 'Scope').textContent, label);
        assert.doesNotMatch(state.host.innerHTML, /ci-scope|All acceptance dates/);
        assert.match(state.host.innerHTML, /^<div class="ci-controls"><div class="ci-tabs"/);
        assert.match(state.host.innerHTML, /<\/button><\/div><div class="ci-toolbar"><span class="ai-summary-status-value ci-mode">/);
        state.pages = { highlights: { items: [] } };
        state.data = { Scope: label, DateRange: 'All acceptance dates', Mode: 'ai', Records: 0, DistinctIssues: 0, DuplicateRecords: 0, ExcludedRecords: 0, GeneratedAt: '2026-11-04T12:00:00Z', ExpiresAt: '2099-01-01T00:00:00Z', SourceStatus: 'All returned text AI-reviewed' };
        context.draw(state);
        assert.equal(elements.get(hostId + 'Scope').textContent, label);
    }
});

test('AI Insights polls hybrid group review and never presents calculated groups as AI opinions', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    function harness(replies) {
        const parts = new Map(['.ci-content', '.ci-mode', '.ci-time', '.ci-metadata', '.ci-provenance', '.ci-status', '.ci-refresh'].map(selector => [selector, {
            textContent: '', innerHTML: 'old calculated content', attributes: {},
            setAttribute(name, value) { this.attributes[name] = value; }
        }]));
        const state = { serial: 0, platform: 'CMF_NVL_H_ALL_COMPONENTS_TABLE', data: { Mode: 'evidence' },
            host: { isConnected: true, querySelector: selector => parts.get(selector) } };
        const requests = [], received = [], timers = new Map();
        let nextTimer = 0;
        const context = vm.createContext({
            active: state, AbortController,
            setTimeout(callback, delay) { const id = ++nextTimer; timers.set(id, { callback, delay }); return id; },
            clearTimeout(id) { timers.delete(id); },
            async fetch(url, options) {
                requests.push({ url, body: JSON.parse(options.body), signal: options.signal });
                const reply = replies.shift();
                if (reply instanceof Error) throw reply;
                assert.ok(reply, 'Unexpected extra request');
                return { ok: true, async json() { return { d: reply }; } };
            },
            receive(target, data) { assert.equal(data.Mode, 'ai'); received.push(data); target.data = data; },
            draw(target) { parts.get('.ci-content').innerHTML = 'AI result: ' + target.data.Highlights[0].Title; }
        });
        for (const name of ['request', 'status', 'load']) {
            const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
            assert.ok(definition);
            vm.runInContext(definition[0], context);
        }
        return { context, state, parts, requests, received, timers };
    }
    const source = { Success: true, Mode: 'ai-loading', EvidenceKey: 'opaque', Records: 12, DistinctIssues: 11, CandidateCount: 6, ReviewedCandidates: 0, Highlights: [] };
    const complete = { Success: true, Mode: 'ai', Message: 'AI review complete.', Highlights: [{ Title: 'Technical pattern' }] };
    const running = harness([source, { ...source, ReviewedCandidates: 2, Message: 'AI reviewed 2/6 ranked groups.' }, complete]);
    running.context.load(running.state, true);
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(running.received.length, 0);
    assert.equal(running.parts.get('.ci-content').innerHTML, '');
    assert.equal(running.parts.get('.ci-mode').textContent, 'Analyzing');
    assert.match(running.parts.get('.ci-metadata').textContent, /6 ranked groups \| 2 reviewed/);
    assert.equal(running.requests.length, 2, 'An empty pre-AI highlights list must not skip the AI pipeline');
    assert.deepEqual(running.requests[1].body, { evidenceKey: 'opaque', refresh: true });
    const poll = Array.from(running.timers.values()).find(timer => timer.delay === 3000);
    assert.ok(poll, 'Long analysis must poll without a total elapsed-time cutoff');
    poll.callback();
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(running.received.length, 1);
    assert.equal(running.requests[2].body.refresh, false);
    assert.match(running.parts.get('.ci-content').innerHTML, /Technical pattern/);
    for (const failure of [new Error('Network unavailable.'), { Success: false, Mode: 'ai-unavailable', Message: 'AI analysis could not complete.' }]) {
        const unavailable = harness([source, failure]);
        unavailable.context.load(unavailable.state, false);
        await new Promise(resolve => setImmediate(resolve));
        assert.equal(unavailable.received.length, 0);
        assert.equal(unavailable.parts.get('.ci-mode').textContent, 'Unavailable');
        assert.equal(unavailable.parts.get('.ci-content').attributes['aria-busy'], 'false');
        assert.doesNotMatch(unavailable.parts.get('.ci-content').innerHTML, /old calculated content|Technical pattern/);
        assert.equal(unavailable.parts.get('.ci-refresh').disabled, false);
    }
    assert.doesNotMatch(script, /Calculated|Showing calculated|calculated evidence is available/);
});

test('hybrid Insights warms all platforms, preserves source evidence, and derives Overall without raw HSD analysis', () => {
    assert.match(server, /WarmPlatforms\(CmfPlatformCatalog\.GetAllPlatformTables\(\), LoadDashboardInsightEvidence\)/);
    assert.match(server, /GetSummaryIssueSourceTablesForWeb\(selection\)/);
    assert.match(server, /evidence\.PlatformEvidence\.Add\(platformEvidence\)/);
    assert.match(server, /cmf-insight-result:/);
    assert.match(server, /new\[\] \{ "processor", "family", "release", "sysdebug_forum" \}/);
    const service = fs.readFileSync(path.join(root, 'App_Code', 'CmfDashboardInsightsService.cs'), 'utf8');
    assert.match(service, /new SemaphoreSlim\(2\)/);
    assert.match(service, /evidence\.PlatformEvidence == null[\s\S]*?AnalyzeHybrid[\s\S]*?: AnalyzeRollup/);
    const rollup = service.match(/    public static CmfDashboardInsights AnalyzeRollup\([\s\S]*?\r?\n    \}/)[0];
    assert.doesNotMatch(rollup, /FetchArticle|AnalyzeHybrid|\.Documents/);
    assert.match(rollup, /platform\.Highlights/);
});

test('AI Insights findings reuse the shared assistant sections without changing evidence or links', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    const context = vm.createContext({});
    for (const name of ['escape', 'links', 'findings']) {
        const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    const markup = context.findings([{
        Title: 'Merge HSD 14012345678', RelatedId: '14012345678', AiReviewed: true,
        Evidence: '2 distinct HSDs <unconfirmed>', Action: 'Compare linked records.',
        DistinctIssues: 2, Occurrences: 2, PlatformCount: 1,
        Reasons: [{ Text: 'Same explicit link.' }, { Text: 'Shared component.', Source: 'component', IssueIds: ['14012345679'] }],
        Issues: [{ Id: '14012345679', Platform: 'NVL-H', Title: 'Example <title>' }]
    }], 'No findings.');
    assert.match(markup, /<li class="cmf-assistant-text-section cmf-assistant-description-section"><h3>/);
    assert.match(markup, /<p class="ci-evidence">2 distinct HSDs &lt;unconfirmed&gt;<\/p>/);
    assert.match(markup, /Why related:[\s\S]*Same explicit link\./);
    assert.match(markup, /Supporting comparisons[\s\S]*Shared component\./);
    assert.match(markup, /Suggested review:[\s\S]*Compare linked records\./);
    assert.match(markup, /href="https:\/\/hsdes\.intel\.com\/resource\/14012345679"/);
    assert.match(markup, /Example &lt;title&gt;/);
    assert.match(context.findings([], 'No findings.'), /class="ci-empty">No findings\./);
    const rollup = context.findings([{ Title: 'Platform-derived trait', Evidence: 'Cached platform opinions.', Action: 'Review sources.',
        DistinctIssues: 2, Occurrences: 99, PlatformCount: 2, SourceInsightIds: ['source'], Issues: [] }], 'No findings.');
    assert.match(rollup, /2 distinct linked HSDs/);
    assert.doesNotMatch(rollup, /99 record occurrence/);
    assert.match(styles, /\.ci-controls\s*\{[^}]*grid-template-columns: minmax\(0, 1fr\) auto;/);
    assert.match(styles, /\.ci-status:empty\s*\{\s*display: none;/);
    assert.match(styles, /\.ci-footer\s*\{[^}]*display: flex;[^}]*flex-wrap: wrap;/);
});

test('dashboard date controls share the platform row and align to the visible AI card across scales', () => {
    assert.match(page, /<div class="dashboard-platform-toolbar">\s*<div class="platform-chip-rail"[^>]*><\/div>\s*<div class="dashboard-date-filter">/);
    const definition = page.match(/        function syncDashboardPlatformToolbarWidth\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    let overall = true;
    let scale = 0.9;
    let left = 20;
    const style = { setProperty(name, value) { this[name] = value; } };
    const toolbar = { offsetWidth: 1000, style, getBoundingClientRect: () => ({ left, width: 1000 * scale }) };
    const overallAi = { offsetWidth: 700, getBoundingClientRect: () => ({ right: left + 1800 * scale }) };
    const platformAi = { offsetWidth: 700, getBoundingClientRect: () => ({ right: left + 1700 * scale }) };
    const context = vm.createContext({
        document: {
            querySelector: selector => selector.includes('dashboard-platform-toolbar') ? toolbar : overallAi,
            getElementById: () => platformAi
        },
        getComputedStyle: () => ({ width: '1000px' }),
        isOverallDashboardSelection: () => overall
    });
    vm.runInContext(definition[0], context);
    context.syncDashboardPlatformToolbarWidth();
    assert.equal(style.width, '1800.00px');
    assert.equal(style['--dashboard-toolbar-scale'], '0.90000');
    left = -200;
    context.syncDashboardPlatformToolbarWidth();
    assert.equal(style.width, '1800.00px');
    overall = false;
    scale = 1;
    context.syncDashboardPlatformToolbarWidth();
    assert.equal(style.width, '1700.00px');
    assert.equal(style['--dashboard-toolbar-scale'], '1.00000');
});

test('milestone Total CMFs opens all milestones through the scoped detail and export query', () => {
    const host = { classList: { add() {} }, innerHTML: '', querySelector: () => null };
    const context = vm.createContext({
        window: { CMF_PORTAL: {} },
        escapeHtml: value => String(value)
    });
    const definition = page.match(/        function renderMilestoneSummaryPie\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    context.renderMilestoneSummaryPie(host, { Rows: [['QS', '47'], ['Beta', '12'], ['Total', '59']] });
    assert.match(host.innerHTML, /milestone-stacked-total">Total CMFs: <a[^>]*showDriverIssues\(decodeURIComponent\('__all_milestones__'\)\);">59<\/a>/);
    assert.match(host.innerHTML, /showDriverIssues\(decodeURIComponent\('QS'\)\)/);
    const query = server.match(/    private DataTable GetDriverIssuesData\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(query);
    assert.match(query[0], /allMilestones \? ResolveDashboardPlatformSelection\(\) : selectedPlatform/);
    assert.match(query[0], /@AllMilestones = 1 OR LTRIM\(RTRIM\(drivers\)\) = @Driver/);
    assert.match(query[0], /allMilestones \? GetDashboardDateFilter\(\)\.BuildPredicate\("A"\)/);
    assert.match(server, /GridView_driver_issues\.DataSource = GetDriverIssuesData\(driver\)/);
});

test('platform group headings omit the table suffix without changing group identifiers', () => {
    const body = {};
    const meta = {};
    const context = vm.createContext({
        document: { getElementById: id => id === 'summaryIssuesPopupBody' ? body : meta },
        window: { CMF_PORTAL: { summaryIssuesPopupState: { groupBy: 'platform' } } },
        syncSummaryIssuePopupControls: () => {},
        syncSummaryIssueGroupByOptions: (options, selected) => selected,
        syncSummaryIssueSortByOptions: selected => selected,
        syncSummaryTrendIssueStatusControls: () => ({}),
        normalizeSummaryIssueSortBy: selected => selected,
        escapeHtml: value => String(value)
    });
    for (const name of ['encodeForInlineJsDecodeArg', 'formatSummaryIssueCountText', 'renderSummaryIssueLinks']) {
        const definition = page.match(new RegExp('        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    const result = {
        Success: true,
        GroupBy: 'platform',
        Groups: [
            { GroupName: 'CMF_NVL_H_ALL_COMPONENTS_TABLE', HsdIds: ['123'] },
            { GroupName: 'CMF_PTL_ALL_components_table', HsdIds: ['456'] },
            { GroupName: 'WCL', HsdIds: [] }
        ]
    };
    context.renderSummaryIssueLinks(result);
    assert.match(body.innerHTML, /summary-issues-group-name">CMF_NVL_H_ALL<\/span>/);
    assert.match(body.innerHTML, /summary-issues-group-name">CMF_PTL_ALL<\/span>/);
    assert.match(body.innerHTML, /summary-issues-group-name">WCL<\/span>/);
    assert.equal(result.Groups[0].GroupName, 'CMF_NVL_H_ALL_COMPONENTS_TABLE');
    result.GroupBy = 'component';
    context.renderSummaryIssueLinks(result);
    assert.match(body.innerHTML, /summary-issues-group-name">CMF_NVL_H_ALL_COMPONENTS_TABLE<\/span>/);
});

test('record-type controls are hidden only for Pending summary-card popups', () => {
    const nodes = new Map();
    const getNode = key => {
        if (!nodes.has(key)) nodes.set(key, { style: {} });
        return nodes.get(key);
    };
    const context = vm.createContext({ document: { getElementById: getNode, querySelector: getNode } });
    const definition = page.match(/        function syncSummaryIssuePopupControls\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    for (const [source, metric, hidden] of [
        ['summary', 'pending', true], ['summary', 'open', false],
        ['summary', ' Pending ', true], ['summary', 'implemented', false],
        ['overall-heatmap', 'open', false], ['trend-week', 'open', false]
    ]) {
        context.syncSummaryIssuePopupControls(source, metric);
        assert.equal(getNode('summaryIssuesRecordTypeLabel').style.display, hidden ? 'none' : '');
        assert.equal(getNode('summaryIssuesRecordTypeChecks').style.display, hidden ? 'none' : 'flex');
        assert.notEqual(getNode('summaryIssuesGroupBy').style.display, 'none');
    }
    assert.match(page, /syncSummaryIssuePopupControls\(sourceType, metricKey\)/);
    assert.match(page, /syncSummaryIssuePopupControls\(popupState\.sourceType \|\| 'summary', popupState\.metricKey\)/);
});

test('issue-count popup requests share record-type filters and ignore stale responses', async () => {
    const duplicateDefault = page.match(/includeDuplicates:\s*(sourceType[^\r\n,]+)/);
    assert.ok(duplicateDefault);
    for (const [sourceType, expected] of [['summary', true], ['overall-heatmap', false], ['trend-week', false]]) {
        assert.equal(vm.runInNewContext(duplicateDefault[1], { sourceType }), expected);
    }
    assert.match(page, /checkbox\.checked = window\.CMF_PORTAL\.summaryIssuesPopupState\[toggle\.key\] !== false/);
    const requests = [];
    const rendered = [];
    const state = { metricKey: 'open', metricLabel: 'Open', groupBy: 'ingredient' };
    const context = vm.createContext({
        document: { getElementById: () => ({}) },
        window: { CMF_PORTAL: { summaryIssuesPopupState: state, currentDashboardPlatform: 'Overall' } },
        fetch: (endpoint, options) => {
            requests.push({ endpoint, payload: JSON.parse(options.body) });
            return Promise.resolve({ json: () => Promise.resolve({ d: { Success: true } }) });
        },
        renderSummaryIssueLinks: result => rendered.push(result)
    });
    const definition = page.match(/        function loadSummaryIssueLinks\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    const flush = () => new Promise(resolve => setImmediate(resolve));
    for (const sourceType of ['summary', 'overall-heatmap', 'trend-week']) {
        state.sourceType = sourceType;
        delete state.includeUnique;
        delete state.includeDuplicates;
        context.loadSummaryIssueLinks('open', 'Open', 'ingredient');
        await flush();
        assert.equal(requests[requests.length - 1].payload.includeUnique, true);
        assert.equal(requests[requests.length - 1].payload.includeDuplicates, true);
        state.includeUnique = false;
        state.includeDuplicates = true;
        context.loadSummaryIssueLinks('open', 'Open', 'ingredient');
        await flush();
        assert.equal(requests[requests.length - 1].payload.includeUnique, false);
        assert.equal(requests[requests.length - 1].payload.includeDuplicates, true);
    }
    const pending = [];
    context.fetch = () => new Promise(resolve => pending.push(resolve));
    context.loadSummaryIssueLinks('open', 'Open', 'ingredient');
    context.loadSummaryIssueLinks('open', 'Open', 'ingredient');
    pending[1]({ json: () => Promise.resolve({ d: { TotalIssues: 6 } }) });
    await flush();
    pending[0]({ json: () => Promise.resolve({ d: { TotalIssues: 23 } }) });
    await flush();
    assert.equal(rendered[rendered.length - 1].TotalIssues, 6);
    for (const id of ['summaryIssuesIncludeUnique', 'summaryIssuesIncludeDuplicates']) {
        assert.match(page, new RegExp('type="checkbox" id="' + id + '" checked="checked"'));
    }
});

test('Overall and platform trend titles select backlog or weekly flow without a separate view selector', () => {
    for (const selectorId of ['homeOverallTrendView', 'homeSpecificTrendView']) {
        assert.equal((page.match(new RegExp('id="' + selectorId + '"', 'g')) || []).length, 1);
        assert.match(page, new RegExp('<h4 class="overall-card-title overall-trend-title">[\\s\\S]*?<select id="' + selectorId + '" class="overall-trend-title-select"[\\s\\S]*?<option value="line" selected="selected">OPEN BACKLOG TREND BY WORK WEEK</option>[\\s\\S]*?<option value="bar">WEEKLY ISSUE FLOW</option>[\\s\\S]*?</select>\\s*</h4>'));
    }
});

test('Overall trend stacks week-end balances and retains weekly clustered bars and drilldowns', () => {
    const options = [];
    const popups = [];
    const view = { value: 'line' };
    const specificView = { value: 'line' };
    const chart = {
        setOption: option => options.push(option),
        on() {}, off() {}, resize() {},
        getZr: () => ({ on() {}, off() {} }),
        containPixel: () => true,
        convertFromPixel: () => 0
    };
    const context = vm.createContext({
        document: { getElementById: id => id === 'homeOverallTrendView' ? view : (id === 'homeSpecificTrendView' ? specificView : {}), body: {} },
        window: { CMF_PORTAL: { ids: {}, currentPlatform: 'Overall' } },
        echarts: { init: () => chart },
        getComputedStyle: () => ({ fontFamily: 'Cambria' }),
        getTextById: () => '',
        escapeHtml: value => String(value),
        openTrendWeekIssueLinksPopup: (...args) => popups.push(args)
    });
    const definition = page.match(/        function renderHomeTrendChart\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    const snapshot = { Trend: Array.from({ length: 12 }, (_, index) => ({
        WeekLabel: 'WW' + index, NewIssues: index + 1, ResolvedIssues: index, ImplementedIssues: index + 2,
        WeekEndOpenIssues: index + 40, WeekEndImplementedIssues: index + 80
    })) };
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 6);
    assert.equal(options[0].series[0].type, 'line');
    assert.equal(options[0].series[0].name, 'Implemented');
    assert.equal(options[0].series[0].itemStyle.color, '#800000');
    assert.equal(options[0].series[0].data[0].value, 86);
    assert.equal(options[0].series[1].name, 'Open');
    assert.equal(options[0].series[1].itemStyle.color, '#0369a1');
    assert.equal(options[0].series[1].data[0].value, 46);
    assert.ok(options[0].series.every(series => series.stack === 'cmf-status'));
    assert.equal(options[0].series[0].lineStyle.width, 0);
    assert.equal(options[0].series[0].emphasis.lineStyle.width, 0);
    assert.equal(options[0].series[1].lineStyle.color, '#0369a1');
    assert.equal(options[0].series[1].lineStyle.width, 3);
    assert.equal(options[0].series[0].symbol, 'none');
    assert.equal(options[0].series[1].symbol, 'circle');
    assert.equal(options[0].series[1].showAllSymbol, true);
    assert.ok(options[0].series.every(series => !series.label));
    assert.equal(options[0].grid.top, 30);
    assert.equal(options[0].series[0].areaStyle.color, 'rgba(128,0,0,0.12)');
    assert.equal(options[0].series[1].areaStyle.color, 'rgba(3,105,161,0.12)');
    const tooltip = options[0].tooltip.formatter([
        { seriesName: 'Open', value: 46, axisValueLabel: 'WW6', marker: '[blue]' },
        { seriesName: 'Implemented', value: 86, axisValueLabel: 'WW6', marker: '[maroon]' }
    ]);
    assert.match(tooltip, /WW6<\/span><span style="font-weight:900">Total: 132/);
    assert.match(tooltip, /\[blue\]Open[\s\S]*46[\s\S]*\[maroon\]Implemented[\s\S]*86/);
    assert.equal(options[0].tooltip.formatter([]), '');
    chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
    assert.deepEqual(popups[0], ['Open and Implemented', 'WW6', 5]);
    view.value = 'bar';
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 6);
    const bars = options[1];
    assert.equal(bars.series[0].type, 'bar');
    assert.equal(bars.series[1].type, 'bar');
    assert.equal(bars.tooltip.formatter, undefined);
    assert.equal(bars.color[0], '#0369a1');
    assert.equal(bars.color[1], '#16a34a');
    assert.ok(bars.series.every(series => series.barMaxWidth === 18 && series.itemStyle.borderWidth === 1));
    assert.equal(bars.series[0].itemStyle.color, '#f0f9ff');
    assert.equal(bars.series[0].itemStyle.borderColor, '#0369a1');
    assert.equal(bars.series[1].itemStyle.color, '#dcfce7');
    assert.equal(bars.series[1].itemStyle.borderColor, '#16a34a');
    assert.equal(bars.grid.top, 40);
    for (const series of bars.series) {
        assert.equal(series.label.show, true);
        assert.equal(series.label.position, 'top');
        assert.equal(series.label.formatter({ value: 0 }), '');
        assert.equal(series.label.formatter({ value: 1 }), '1');
        assert.equal(series.label.formatter({ value: 123 }), '123');
    }
    assert.equal(bars.xAxis.boundaryGap, true);
    assert.ok(bars.series.every(series => !series.stack));
    assert.equal(bars.series[0].data[0].value, 7);
    assert.equal(bars.series[1].name, 'Closed');
    assert.equal(bars.series[1].data[0].value, 6);
    chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
    assert.deepEqual(popups[1], ['All', 'WW6', 5]);
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 12);
    assert.equal(options[2].series[0].data.length, 12);
    assert.ok(options[2].series.every(series => series.barMaxWidth === 8));
    view.value = 'line';
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 12);
    assert.equal(options[3].series[0].type, 'line');
    assert.equal(options[3].series[0].data[0].value, 80);
    assert.equal(options[3].series[1].data[0].value, 40);
    view.value = 'bar';
    context.renderHomeTrendChart(snapshot, 'homeTrendChart', 'specificChart', 6);
    assert.equal(options[4].series[0].type, 'line');
    assert.equal(options[4].series[0].name, 'Implemented');
    assert.equal(options[4].series[1].name, 'Open');
    assert.equal(options[4].series[0].data[0].value, 86);
    assert.equal(options[4].series[1].data[0].value, 46);
    assert.equal(options[4].tooltip.formatter([
        { seriesName: 'Open', value: 46, axisValueLabel: 'WW6', marker: '[blue]' },
        { seriesName: 'Implemented', value: 86, axisValueLabel: 'WW6', marker: '[maroon]' }
    ]), tooltip);
    assert.equal(JSON.stringify(options[4].series), JSON.stringify(options[0].series));
    assert.equal(JSON.stringify(options[4].legend), JSON.stringify(options[0].legend));
    assert.equal(JSON.stringify(options[4].xAxis), JSON.stringify(options[0].xAxis));
    assert.equal(JSON.stringify(options[4].yAxis), JSON.stringify(options[0].yAxis));
    assert.equal(options[4].series[1].lineStyle.color, '#0369a1');
    assert.ok(options[4].series.every(series => series.stack === 'cmf-status'));
    chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
    assert.deepEqual(popups[2], ['Open and Implemented', 'WW6', 5]);
    specificView.value = 'bar';
    context.renderHomeTrendChart(snapshot, 'homeTrendChart', 'specificChart', 6);
    assert.equal(options[5].series[0].type, 'bar');
    assert.equal(options[5].series[1].name, 'Closed');
    assert.equal(options[5].series[0].data[0].value, 7);
    assert.equal(options[5].series[1].data[0].value, 6);
    assert.ok(options[5].series.every(series => !series.stack && series.barMaxWidth === 18));
    context.renderHomeTrendChart(snapshot, 'homeTrendChart', 'specificChart', 12);
    assert.ok(options[6].series.every(series => series.data.length === 12 && series.barMaxWidth === 8));
    assert.equal(JSON.stringify(options[6].series), JSON.stringify(options[2].series));
});

['homeOverallTrendChart', 'homeTrendChart'].forEach(hostId => test('filtered ' + hostId + ' trends disable the week selector and preserve the full filtered timeline and week offsets', () => {
    let option;
    const popups = [];
    const view = { value: 'line' };
    const dateMode = { value: '7days' };
    const rangeSelector = { value: '6', disabled: false, parentElement: { style: { display: '' } } };
    const otherRangeSelector = { value: '6', disabled: false, parentElement: { style: { display: '' } } };
    const rangeSelectorId = hostId === 'homeOverallTrendChart' ? 'homeOverallTrendRange' : 'homeSpecificTrendRange';
    const chart = {
        setOption: value => { option = value; },
        on() {}, off() {}, resize() {},
        getZr: () => ({ on() {}, off() {} }),
        containPixel: () => true,
        convertFromPixel: () => 0
    };
    const context = vm.createContext({
        document: {
            getElementById: id => id.indexOf('ddlDashboardDateMode') >= 0 ? dateMode
                : (id === rangeSelectorId ? rangeSelector : (/TrendRange$/.test(id) ? otherRangeSelector : (/TrendView$/.test(id) ? view : {}))),
            body: {}
        },
        window: { CMF_PORTAL: { ids: {}, currentPlatform: 'Overall' } },
        echarts: { init: () => chart },
        getComputedStyle: () => ({ fontFamily: 'Cambria' }),
        getTextById: () => '',
        escapeHtml: value => String(value),
        openTrendWeekIssueLinksPopup: (...args) => popups.push(args)
    });
    const definition = page.match(/        function renderHomeTrendChart\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    const snapshot = { Trend: Array.from({ length: 12 }, (_, index) => ({
        WeekLabel: 'WW' + index,
        WeekEndOpenIssues: index === 10 ? 2 : 0,
        WeekEndImplementedIssues: index === 2 ? 4 : 0,
        NewIssues: 0,
        ResolvedIssues: index === 3 ? 3 : 0
    })) };
    const original = JSON.stringify(snapshot);
    for (const chartType of ['line', 'bar']) {
        view.value = chartType;
        const start = chartType === 'line' ? 0 : 3;
        const expected = snapshot.Trend.slice(start).map(item => item.WeekLabel);
        for (const mode of ['today', '7days', '30days', '12months', 'custom']) {
            dateMode.value = mode;
            context.renderHomeTrendChart(snapshot, hostId, 'trendChart', 6);
            assert.equal(rangeSelector.disabled, true);
            assert.equal(rangeSelector.parentElement.style.display, 'none');
            assert.deepEqual(Array.from(option.xAxis.data), expected);
            context.renderHomeTrendChart(snapshot, hostId, 'trendChart', 12);
            assert.deepEqual(Array.from(option.xAxis.data), expected);
            for (const series of option.series) {
                assert.equal(series.data[0].weekOffset, 11 - start);
                assert.equal(series.data.at(-1).weekOffset, 0);
                assert.equal(series.data.at(-1).value, 0);
            }
            chart.__cmfTrendLastAxisIndex = 0;
            if (chartType === 'line') {
                assert.ok(option.series.every(series => series.data[0].value === 0));
                const popupCount = popups.length;
                chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
                assert.equal(popups.length, popupCount);
                chart.__cmfTrendLastAxisIndex = 2;
            }
            chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
            const populatedIndex = chartType === 'line' ? start + 2 : start;
            assert.deepEqual(popups.at(-1), [chartType === 'line' ? 'Open and Implemented' : 'All', 'WW' + populatedIndex, 11 - populatedIndex]);
        }
        dateMode.value = 'all';
        for (const weeks of [6, 12]) {
            rangeSelector.value = String(weeks);
            context.renderHomeTrendChart(snapshot, hostId, 'trendChart', weeks);
            assert.equal(rangeSelector.disabled, false);
            assert.equal(rangeSelector.parentElement.style.display, '');
            assert.deepEqual(Array.from(option.xAxis.data), snapshot.Trend.slice(-weeks).map(item => item.WeekLabel));
        }
        dateMode.value = '7days';
        context.renderHomeTrendChart(snapshot, hostId, 'trendChart', 6);
        assert.equal(rangeSelector.value, '12');
        assert.equal(rangeSelector.disabled, true);
        assert.equal(rangeSelector.parentElement.style.display, 'none');
        assert.deepEqual(Array.from(option.xAxis.data), expected);
        assert.equal(otherRangeSelector.disabled, false);
        assert.equal(otherRangeSelector.parentElement.style.display, '');
    }
    assert.equal(JSON.stringify(snapshot), original);
    const emptySnapshot = { Trend: snapshot.Trend.map(item => ({ WeekLabel: item.WeekLabel })) };
    context.renderHomeTrendChart(emptySnapshot, hostId, 'trendChart', 6);
    assert.deepEqual(Array.from(option.xAxis.data), ['WW11']);
    assert.ok(option.series.every(series => series.data[0].value === 0 && series.data[0].weekOffset === 0));
    context.renderHomeTrendChart({ Trend: [] }, hostId, 'trendChart', 6);
    assert.equal(option.xAxis.data.length, 0);
    context.renderHomeTrendChart({ Trend: [{ WeekLabel: 'Only', ResolvedIssues: 2 }] }, hostId, 'trendChart', 6);
    assert.deepEqual(Array.from(option.xAxis.data), ['Only']);
    assert.equal(option.series[1].data[0].weekOffset, 0);
    view.value = 'line';
    for (const trend of [
        emptySnapshot.Trend,
        [],
        [{ WeekLabel: 'Only', WeekEndOpenIssues: 2 }],
        [{ WeekLabel: 'Zero' }, { WeekLabel: 'Rise', WeekEndOpenIssues: 2 }]
    ]) {
        context.renderHomeTrendChart({ Trend: trend }, hostId, 'trendChart', 6);
        const expected = trend.length === 12 ? ['WW11'] : trend.map(item => item.WeekLabel);
        assert.deepEqual(Array.from(option.xAxis.data), expected);
    }
}));

test('Overall week-end line uses summary-card scope and current status buckets without duplicates', () => {
    assert.match(server, /string weekEndSourceSql = "\(SELECT \* FROM " \+ BuildDashboardComponentSourceSql\(dashboardPlatform, true\)/);
    assert.match(server, /WHERE cmf_request = 'cmf_ok' AND sysdebug LIKE '%customer_must_fix%'/);
    const predicate = server.match(/    private static string BuildTrendWeekEndStatusPredicate\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(predicate);
    assert.match(predicate[0], /status NOT IN \('complete', 'rejected', 'implemented', 'future'\)/);
    assert.match(predicate[0], /status IN \('implemented', 'future'\)/);
    assert.match(predicate[0], /weekOffsetSql \+ " = 0 AND " \+ currentStatusPredicate/);
    assert.match(predicate[0], /weekOffsetSql \+ " > 0 AND " \+ historicalPredicate/);
    const drilldown = server.match(/    public static SummaryIssueLinksResponse GetTrendWeekIssueLinks\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(drilldown);
    assert.doesNotMatch(drilldown[0], /AppendDashboardDriverFilter|AppendDashboardColumnFilters/);
});

test('dashboard activity and weekly-flow drilldowns cannot inherit Issue List session filters across platforms', () => {
    const source = server.match(/    private string BuildHomeDashboardSourceSql\([^)]*\)[\s\S]*?\r?\n    \}/);
    const drilldown = server.match(/    public static SummaryIssueLinksResponse GetTrendWeekIssueLinks\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(source);
    assert.ok(drilldown);
    for (const definition of [source[0], drilldown[0]]) {
        assert.doesNotMatch(definition, /Session\[|GetAllFilterValues|BuildDefaultIssueDriverFilter|GetDashboardDriverFilterValueForWeb|GetDashboardColumnFiltersForWeb|AppendDashboardDriverFilter|AppendDashboardColumnFilters/);
    }
    assert.match(source[0], /ResolveDashboardPlatformSelection\(platformTable\)/);
    assert.match(source[0], /BuildDashboardComponentSourceSql\(platformTable, true\)/);
    assert.match(source[0], /status NOT IN \('rejected'\)/);
    assert.match(source[0], /sysdebug LIKE \('%customer_must_fix%'\) AND cmf_request IN \('cmf_ok'\)/);
    assert.match(drilldown[0], /BuildSummaryIssueComponentSourceSql\(resolvedPlatform\)/);
    assert.match(drilldown[0], /BuildSummaryIssueRecordTypePredicate\("src", includeUnique, includeDuplicates\)/);
    assert.match(drilldown[0], /if \(!isWeekEndBalance\)\s*\{\s*baseWhere.Append\(" AND LOWER\(LTRIM\(RTRIM\(ISNULL\(src.status, ''\)\)\)\) <> 'rejected'"\);/);
});

test('all record-type drilldowns show the CMF request field separately from issue status', () => {
    const details = server.match(/    private DataTable GetSummaryMetricIssueDetailsData\([^)]*\)[\s\S]*?\r?\n    \}/);
    const grid = page.match(/<asp:GridView ID="GridView_summaryMetricIssues"[\s\S]*?<\/asp:GridView>/);
    assert.ok(details);
    assert.ok(grid);
    assert.match(details[0], /ISNULL\(NULLIF\(LTRIM\(RTRIM\(issue_src\.cmf_request\)\), ''\), 'N\/A'\) AS CmfRequest/);
    assert.match(grid[0], /DataField="Status" HeaderText="Status"\s*\/>\s*<asp:BoundField DataField="CmfRequest" HeaderText="CMF Request"/);
    assert.match(page, /GridView_summaryMetricIssues td:nth-child\(21\)/);
    assert.match(page, /var canOpenSummaryMetricDetails = !isPendingSummaryMetric/);
});

test('Overall summary detail freeze dates use platform-aware metadata without multiplying design rows', () => {
    const details = server.match(/    private DataTable GetSummaryMetricIssueDetailsData\([^)]*\)[\s\S]*?\r?\n    \}/);
    const metadata = server.match(/    private static string BuildSummaryDesignMetadataSourceSql\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(details);
    assert.ok(metadata);
    assert.match(details[0], /BuildSummaryDesignMetadataSourceSql\(dashboardPlatform, con\)/);
    assert.match(details[0], /design_src\.source_platform = issue_src\.source_platform/);
    assert.match(details[0], /dup_design\.source_platform = dup_src\.source_platform/);
    assert.match(details[0], /NULLIF\(sw_image_freeze, 'N\/A'\)/);
    assert.match(details[0], /'Unknown'\) AS ImageFreeze/);
    assert.match(metadata[0], /connection != null && !SqlTableExists\(connection, designTable\)/);
    assert.match(metadata[0], /GROUP BY LTRIM\(RTRIM\(customer_detail\)\)/);
    assert.match(metadata[0], /MAX\(NULLIF\(LTRIM\(RTRIM\(sw_image_freeze\)\), ''\)\)/);
});

test('Issue List omits confidence badges and their update path, but keeps drawer confidence', () => {
    assert.doesNotMatch(server, /status-confidence-pill|data-ai-confidence-issue/);
    assert.doesNotMatch(page, /updateIssueListConfidenceBadge|status-confidence-pill/);
    assert.match(page, /confidenceNode\.textContent = 'Confidence: ' \+ preparedSummary\.confidence/);
});

test('empty-cell placeholders run only for non-editing Issue List data rows', () => {
    assert.match(server, /if \(e\.Row\.RowType == DataControlRowType\.DataRow && e\.Row\.RowIndex != overall_request_details\.EditIndex\)\s*\{\s*FillEmptyIssueListCells\(e\.Row\);/);
    const method = server.match(/    private static void FillEmptyIssueListCells\(GridViewRow row\)[\s\S]*?\r?\n    \}/);
    assert.ok(method);
    assert.match(method[0], /cell\.CssClass\.Split\(' '\)\.Contains\("field-actions"\)/);
    assert.match(method[0], /cell\.RenderControl\(writer\)/);
    assert.match(method[0], /HttpUtility\.HtmlDecode/);
    assert.match(method[0], /if \(string\.IsNullOrWhiteSpace\(text\)\)\s*\{\s*cell\.Controls\.Clear\(\);\s*cell\.Text = "unknown";/);
    const daysOpen = server.match(/    protected string RenderDaysOpen\(object daysOpenValue\)[\s\S]*?\r?\n    \}/);
    assert.match(daysOpen[0], /daysOpenValue == DBNull\.Value \? "unknown"/);
    assert.doesNotMatch(daysOpen[0], /"0"/);
});

test('the Component column displays only the component and preserves component-group filters', () => {
    const column = page.match(/<asp:TemplateField HeaderText="Component" ItemStyle-CssClass="field-component"[\s\S]*?<\/asp:TemplateField>/);
    assert.ok(column);
    const item = column[0].match(/<ItemTemplate>([\s\S]*?)<\/ItemTemplate>/)[1];
    assert.match(item, /Eval\("component"\)/);
    assert.doesNotMatch(item, /component_group/);
    assert.match(column[0], /ID="ddlComponentHeader"[\s\S]*?OnSelectedIndexChanged="ddlComponentHeader_SelectedIndexChanged"/);
    assert.match(page, /<span>Component Group<\/span><asp:DropDownList ID="ddlComponentTop"/);
    assert.match(server, /case "Component":\s*clauses\.Add\(" AND LTRIM\(RTRIM\(" \+ prefix \+ "component_group\)\) = @ComponentFilter "\)/);
});

test('both customer column headers use Customer Detail', () => {
    assert.match(page, /HeaderText="Customer Detail" ItemStyle-CssClass="field-customer_detail"/);
    assert.match(page, /DataField="customer_detail" HeaderText="Customer Detail" ItemStyle-CssClass="pending-field-customer"/);
});

test('12px typography is scoped to Issue List table headers, cells, and descendants', () => {
    assert.match(theme, /body #issueTabShell \.issue-grid-inner #overall_request_details\.table-primary :is\(th, td\),\s*body #issueTabShell \.issue-grid-inner #overall_request_details\.table-primary :is\(th, td\) \* \{\s*font-size: 12px !important;\s*\}/);
});

test('company labels retain name normalization and customer detail without brand colors', () => {
    function cell(text, issue) {
        return {
            textContent: text,
            dataset: {},
            children: [],
            classList: { contains: name => issue && name === 'issue-company-name' },
            querySelector() { return this.children.find(child => child.className === 'pending-customer-label'); },
            querySelectorAll: () => [],
            insertBefore(label) { this.children.unshift(label); }
        };
    }
    const issue = cell('DE', true);
    const pending = cell('lenovo_customer_platform', false);
    const unknown = cell('Other Company', true);
    const context = vm.createContext({
        document: {
            querySelectorAll: () => [issue, pending, unknown],
            createElement: () => ({})
        }
    });
    const definition = page.match(/        function formatPendingCustomerLogos\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    context.formatPendingCustomerLogos();
    assert.equal(issue.children[0].textContent, 'Dell');
    assert.equal(pending.children[0].textContent, 'Lenovo');
    assert.equal(pending.textContent, 'lenovo_customer_platform');
    assert.equal(unknown.children[0].textContent, 'Other Company');
    context.formatPendingCustomerLogos();
    assert.equal(issue.children.length, 1);
    assert.equal(pending.children.length, 1);
    assert.match(page, /document\.addEventListener\('DOMContentLoaded', function \(\) \{\s*formatPendingCustomerLogos\(\)/);
    assert.match(page, /add_endRequest\(formatPendingCustomerLogos\)/);
    const rule = theme.match(/body #pendingTabShell #GridView_cmf_pending \.pending-customer-label,\s*body #issueTabShell #overall_request_details \.pending-customer-label \{([^}]+)\}/);
    assert.ok(rule);
    assert.match(rule[1], /background: #e5e7eb;/);
    assert.match(rule[1], /color: #000;/);
    assert.match(rule[1], /font-weight: 400;/);
});
