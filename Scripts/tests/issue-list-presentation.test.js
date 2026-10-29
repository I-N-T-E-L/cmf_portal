const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const root = path.join(__dirname, '..', '..');
const page = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx'), 'utf8');
const server = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx.cs'), 'utf8');
const theme = fs.readFileSync(path.join(root, 'Content', 'cmf-portal-theme.css'), 'utf8');

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
