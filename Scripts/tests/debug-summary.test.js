const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { spawnSync } = require('node:child_process');
const { test } = require('node:test');

const source = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx'), 'utf8');

function createSummaryDrawer(response, mode = 'AI Debug Summary', history) {
    const ids = ['aiSummaryBody', 'aiSummaryActions', 'aiSummaryConfidence',
        'aiSummaryStatusBadge', 'aiSummaryChat', 'aiSummaryChatMessages', 'aiSummaryChatInput'];
    const elements = new Map(ids.map(id => [id, {
        innerHTML: '', textContent: '', className: '', style: { display: 'none' }
    }]));
    const messages = [];
    const key = '123_' + mode + '_debug-recent-v14';
    const context = vm.createContext({
        document: { getElementById: id => elements.get(id) },
        window: { _aiChatStore: history ? { [key]: history } : {} },
        fetch: async () => ({ json: async () => ({ d: response }) }),
        escapeHtml: text => text,
        prepareAiSummaryForDrawer: () => ({ confidence: '', status: 'open' }),
        prepareIssueDetailsForDrawer: text => 'details:' + text,
        updateIssueListConfidenceBadge: () => {},
        getAiStatusClass: () => 'status-open',
        appendAiChatMessage: (role, text) => messages.push({ role, text })
    });
    const definition = source.match(/        function fetchAiSummary\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    context.fetchAiSummary({ issueId: '123', status: 'open', mode });
    return { context, elements, messages };
}

for (const mode of ['AI Debug Summary', 'AI Summary']) {
    test(mode + ' renders and copies the original flat bullet list without subsections', async () => {
        const summary = '- Engineers reproduced the screen hang with seven applications open.\n'
            + '- The driver comparison did not resolve the hang.\n'
            + '- The root cause remains unconfirmed.';
        const { context, elements, messages } = createSummaryDrawer({
            Success: true, Summary: summary, Confidence: 80
        }, mode);
        await new Promise(resolve => setImmediate(resolve));
        assert.equal(messages.length, 1);
        assert.equal(messages[0].text, summary);
        assert.equal(context.window._aiSummaryLastText, summary);
        assert.equal(elements.get('aiSummaryStatusBadge').textContent, 'Status: open');
        assert.equal(elements.get('aiSummaryConfidence').textContent, 'Confidence: 80%');
        assert.equal(elements.get('aiSummaryChat').style.display, '');
    });
}

test('a refreshed debug summary replaces the cached first response without discarding follow-ups', async () => {
    const history = [{ role: 'assistant', text: '- Old findings.' }, { role: 'user', text: 'Which driver?' }];
    const { messages } = createSummaryDrawer({ Success: true, Summary: '- Updated findings.' }, undefined, history);
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(messages[0].text, '- Updated findings.');
    assert.equal(messages[1].text, 'Which driver?');
});

test('AI failures display a retryable error instead of a fabricated debug summary', async () => {
    const { elements, messages } = createSummaryDrawer({
        Success: false, Message: 'AI Debug Summary could not be generated.'
    });
    await new Promise(resolve => setImmediate(resolve));
    assert.match(elements.get('aiSummaryBody').innerHTML, /could not be generated/);
    assert.match(elements.get('aiSummaryBody').innerHTML, /Retry/);
    assert.equal(elements.get('aiSummaryBody').style.display, '');
    assert.equal(messages.length, 0);
});

test('issue details keeps its existing renderer and conversation key', async () => {
    const { context, messages } = createSummaryDrawer({ Success: true, Summary: 'Title: Test' }, 'AI issue details');
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(messages[0].text, 'details:Title: Test');
    assert.equal(context.window._aiCurrentIssueId, '123_AI issue details');
});

test('prepareIssueDetailsForDrawer populates structured grid fields without N/A when values are present', () => {
    const context = vm.createContext({
        escapeHtml: text => text
    });
    const definition = source.match(/        function prepareIssueDetailsForDrawer\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);

    const summary = '**Issue Details**\n'
        + '- Problem / Issue: System hard hangs under heavy camera and graphics workload.\n'
        + '- Current State: Open with ongoing debug.\n'
        + '- Scenario / Trigger: Plug and unplug USB device while running camera recording.\n'
        + '- Reproduction: 100% on sample units.\n'
        + '- Affected Platform / System: Nova Lake H with Samsung LPDDR5x.\n'
        + '- Environment: Windows 11 24H2 with MRC 160.3.\n'
        + '- Current Investigation: Memory timings and firmware testing in progress.\n'
        + '- Suspected Root Cause: Memory turnaround timing margins.\n'
        + '- Workaround: Lower memory speed to 6400 MT/s.\n'
        + '- Evidence / Logs: Machine check errors observed in uncore PCU.\n\n'
        + '**Linked HSD / CMF Data**\n'
        + '- Promoted ID: 15019951876\n'
        + '- Owner: hsuck\n'
        + '- Fixed Version / Closure: Target MRC 162\n'
        + '- Latest Activity: TAT+5 passed overnight test.';

    const html = context.prepareIssueDetailsForDrawer(summary);
    assert.ok(html.includes('System hard hangs under heavy camera and graphics workload.'));
    assert.ok(html.includes('Open with ongoing debug.'));
    assert.ok(html.includes('Plug and unplug USB device'));
    assert.ok(html.includes('100% on sample units.'));
    assert.ok(html.includes('Nova Lake H with Samsung LPDDR5x.'));
    assert.ok(html.includes('Windows 11 24H2 with MRC 160.3.'));
    assert.ok(html.includes('Memory timings and firmware testing in progress.'));
    assert.ok(html.includes('Memory turnaround timing margins.'));
    assert.ok(html.includes('Lower memory speed to 6400 MT/s.'));
    assert.ok(html.includes('Machine check errors observed in uncore PCU.'));
    assert.ok(html.includes('15019951876'));
    assert.ok(html.includes('hsuck'));
    assert.ok(html.includes('Target MRC 162'));
    assert.ok(html.includes('TAT+5 passed overnight test.'));
    assert.ok(!html.includes('>N/A<'));
});

test('debug generation and follow-ups bypass shared forum context without changing other consumers', () => {
    const server = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx.cs'), 'utf8');
    assert.match(server, /BuildIssueSummaryContext\(NormalizePlatformForLookup\(resolvedPlatform\), issueId, !isDetails\)/);
    assert.match(server, /BuildIssueSummaryContext\(NormalizePlatformForLookup\(resolvedPlatform\), issueId, forDebugSummary\)/);
    assert.match(server, /Sysdebug = isDetails \? sysdebug : string.Empty/);
    const context = server.match(/    private static string BuildIssueSummaryContext\([^)]*\)[\s\S]*?\r?\n    \}/)[0];
    assert.ok(context.indexOf('if (forDebugSummary) return') < context.indexOf('HsdPortalService.FormatForAiContext'));
    assert.match(context, /if \(!forDebugSummary\) AppendSummaryContextLine\(builder, "Sysdebug"/);
    assert.match(server, /GenerateFollowUpResponse\(request, question \?\? string.Empty, forDebugSummary\)/);
    const service = fs.readFileSync(path.join(__dirname, '..', '..', 'App_Code', 'AiSummaryService.cs'), 'utf8');
    assert.match(service, /ConversationStore\["debug-recent-v14:" \+ issueId\]/);
    assert.match(service, /HsdPortalService.FetchArticle\(issueId, forDebugSummary\)/);
    assert.match(service, /HsdPortalService.FetchArticle\(pid, forDebugSummary\)/);
});

const compiler = path.join(process.env.WINDIR || 'C:\\Windows', 'Microsoft.NET', 'Framework64', 'v4.0.30319', 'csc.exe');
test('compiled debug assistant isolates forum data, orders recent evidence, and enforces brief output', {
    skip: !fs.existsSync(compiler)
}, () => {
    const root = path.join(__dirname, '..', '..');
    const directory = fs.mkdtempSync(path.join(root, 'App_Data', 'cmf-debug-summary-'));
    const output = path.join(directory, 'Tests.exe');
    try {
        const assemblies = fs.readdirSync(path.join(root, 'bin'))
            .filter(file => file.endsWith('.dll'));
        for (const assembly of assemblies) {
            fs.copyFileSync(path.join(root, 'bin', assembly), path.join(directory, assembly));
        }
        const build = spawnSync(compiler, [
            '/nologo', '/target:exe', '/out:' + output, '/r:System.Web.dll',
            '/r:System.Web.Extensions.dll', '/r:System.Configuration.dll', '/r:System.Data.dll',
            '/r:System.Data.DataSetExtensions.dll', '/r:System.Xml.dll', '/r:System.Net.Http.dll',
            '/r:System.Drawing.dll', '/r:System.Web.Services.dll', '/r:System.Runtime.Caching.dll',
            '/r:' + path.join(path.dirname(compiler), 'WPF', 'WindowsBase.dll'),
            ...assemblies.map(assembly => '/r:' + path.join(root, 'bin', assembly)),
            ...fs.readdirSync(path.join(root, 'App_Code')).filter(file => file.endsWith('.cs'))
                .map(file => path.join(root, 'App_Code', file)),
            path.join(__dirname, 'DebugSummaryServiceTests.cs')
        ], { encoding: 'utf8', timeout: 120000 });
        assert.equal(build.status, 0, build.stdout + build.stderr);
        const run = spawnSync(output, [], { encoding: 'utf8', timeout: 30000,
            env: { ...process.env, GNAI_ENDPOINT: 'REPLACE_TEST_ENDPOINT', GNAI_TOKEN: '' } });
        assert.equal(run.status, 0, run.stdout + run.stderr);
    } finally {
        fs.rmSync(directory, { recursive: true, force: true });
    }
});
