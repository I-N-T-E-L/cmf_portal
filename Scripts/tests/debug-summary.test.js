const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const source = fs.readFileSync(path.join(__dirname, '..', '..', 'CMF_Web_portal.aspx'), 'utf8');

function createSummaryDrawer(response, mode = 'AI Debug Summary', history) {
    const ids = ['aiSummaryBody', 'aiSummaryActions', 'aiSummaryConfidence',
        'aiSummaryStatusBadge', 'aiSummaryChat', 'aiSummaryChatMessages', 'aiSummaryChatInput'];
    const elements = new Map(ids.map(id => [id, {
        innerHTML: '', textContent: '', className: '', style: { display: 'none' }
    }]));
    const messages = [];
    const key = '123_' + mode + '_debug-bullets-v8';
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
