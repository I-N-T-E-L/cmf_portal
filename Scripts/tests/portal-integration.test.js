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
