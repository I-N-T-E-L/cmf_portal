const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const vm = require('node:vm');
const { spawnSync } = require('node:child_process');
const { test } = require('node:test');

const root = path.join(__dirname, '..', '..');
const page = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx'), 'utf8');
const server = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx.cs'), 'utf8');
const theme = fs.readFileSync(path.join(root, 'Content', 'cmf-portal-theme.css'), 'utf8');

test('Issue List preserves the reference column order with separate promoted fields', () => {
    function fields(source) {
        const grid = source.replace(/<%--[\s\S]*?--%>/g, '').match(/<asp:GridView ID="overall_request_details"[\s\S]*?<Columns>([\s\S]*?)<\/Columns>/)[1];
        return Array.from(grid.matchAll(/HeaderStyle-CssClass="(field-[^"]+)"/g), match => match[1]);
    }
    const old = fs.readFileSync(path.join(root, 'others', 'test_scripts', 'CMF_Web_portal_old.aspx'), 'utf8');
    const expected = fields(old).map(name => name === 'field-edit_column' ? 'field-actions' : name);
    const actual = fields(page).map(name => name === 'field-impact_processor' ? 'field-impact' : name);
    assert.deepEqual(actual, expected);
    assert.doesNotMatch(page, /HeaderText="Promoted ID \/ Owner"/);
    assert.equal((page.match(/id="issueColumnPicker"/g) || []).length, 1);
});

test('lifecycle status filtering is wired through selection, SQL, parameters, cache, and clear', () => {
    assert.match(page, /ID="ddlStatusTop"[\s\S]*?OnSelectedIndexChanged="ddlStatusTop_SelectedIndexChanged"/);
    assert.match(server, /Session\["issueStatusFilter"\] = ddlStatusTop.SelectedValue/);
    assert.match(server, /\{"IssueStatus", Session\["issueStatusFilter"\]/);
    assert.match(server, /BindFilterData\(con, "status", "IssueStatusItems", currentPlatform, activeFilters, "IssueStatus"\)/);
    assert.match(server, /"status\)\) = @IssueStatusFilter "/);
    assert.equal((server.match(/Parameters.AddWithValue\("@IssueStatusFilter"/g) || []).length, 2);
    assert.match(server, /Session\["issueStatusFilter"\] = null/);
    assert.match(server, /ddlStatusTop.SelectedValue = "All"/);
    assert.match(server, /foreach \(KeyValuePair<string, string> filter in columnFilters.OrderBy/);
});

test('title and debug status are not clamped or included in hover truncation', () => {
    const formatter = page.match(/        function formatIssueLongCells\([^)]*\) \{[\s\S]*?\r?\n        \}/)[0];
    assert.match(formatter, /:not\(\.field-status\):not\(\.field-title\)/);
    assert.match(theme, /:is\(\.field-title, \.field-status\)[\s\S]*?-webkit-line-clamp: unset !important/);
    assert.match(page, /issueTable.style.setProperty\('width', 'max-content'/);
    const status = server.match(/    protected string RenderStatusWithAiSummaryButton[\s\S]*?\r?\n    \}/)[0];
    assert.doesNotMatch(status, /class=\\"status-pill/);
    assert.match(status, /title=\\"AI Debug Summary\\"/);
});

test('short-value columns use compact widths and filters keep usable space when wrapping', () => {
    for (const [field, width] of [['sno', 52], ['milestone', 96], ['progress', 88], ['days_open', 72]]) {
        assert.match(theme, new RegExp('\\.field-' + field + ' \\{\\s*--issue-column-width: ' + width + 'px;'));
    }
    assert.match(theme, /width: var\(--issue-column-width, auto\) !important/);
    assert.match(theme, /grid-template-columns: repeat\(auto-fit, minmax\(120px, 1fr\)\)/);
});

test('Issue List docks its platform control without depending on Pending slots', () => {
    const platform = {};
    const links = {};
    const issueSlot = { appendChild: node => { issueSlot.child = node; } };
    const linksSlot = { appendChild: node => { linksSlot.child = node; } };
    const panel = { classList: { add() {} } };
    const context = vm.createContext({
        document: {
            getElementById: id => ({
                sharedFilterPanel: panel, issuePlatformSlot: issueSlot, issueExternalLinks: linksSlot
            }[id] || null),
            querySelector: selector => selector.includes('.overview-platform-filter') ? platform : links
        }
    });
    const definition = page.match(/        function syncPendingPlatformControls\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    context.syncPendingPlatformControls(false, true);
    assert.equal(issueSlot.child, platform);
    assert.equal(linksSlot.child, links);
    assert.match(page, /function bootstrapPortalLoadHooks\(\) \{\s*if \(window.CMF_PORTAL.activeFocusedTab === 'issue'\) \{\s*syncPendingPlatformControls\(false, true\)/);
    assert.match(page, /Sys.Application.add_load\(function \(\) \{\s*bootstrapPortalLoadHooks\(\)/);
});

test('export collects only selected data fields and rejects an Actions-only selection', () => {
    const headers = [
        { classList: ['field-sno'], hidden: false },
        { classList: ['field-title'], hidden: false },
        { classList: ['field-promoted_owner'], hidden: true },
        { classList: ['field-actions'], hidden: false }
    ];
    for (const header of headers) {
        header.classList.contains = name => name === 'hidden' ? header.hidden : header.classList.includes(name);
    }
    const values = {};
    let restored = 0;
    const context = vm.createContext({
        document: {
            querySelectorAll: () => headers,
            getElementById: name => values[name] || (values[name] = {})
        },
        restoreHiddenColumns: () => restored++
    });
    const definition = page.match(/        function prepareIssueViewExport\([^)]*\) \{[\s\S]*?\r?\n        \}/)[0]
        .replace(/<%= hfIssueExportColumns.ClientID %>/g, 'columns')
        .replace(/<%= issueExportMessage.ClientID %>/g, 'message');
    vm.runInContext(definition, context);
    assert.equal(context.prepareIssueViewExport(), true);
    assert.deepEqual(JSON.parse(values.columns.value), ['field-sno', 'field-title']);
    headers[0].hidden = headers[1].hidden = true;
    assert.equal(context.prepareIssueViewExport(), false);
    assert.match(values.message.textContent, /at least one data column/);
    assert.equal(restored, 2);
    assert.match(server, /overall_request_details.AllowPaging = false/);
    assert.match(server, /BindGridView\(Session\["filterValue"\] as string, GetAllFilterValues\(\), bindRelatedGrids: false\)/);
    assert.match(server, /GetIssueExportColumnIndexes\(overall_request_details.Columns, requestedColumns\)/);
    assert.match(server, /ScriptManager1.RegisterPostBackControl\(btnIssueViewExport\)/);
});

function createColumnPicker(saved = {}) {
    const storage = new Map(Object.entries(saved));
    const notices = [];
    const columns = [
        ['field-sno', 'S.No'], ['field-milestone', 'Milestone'],
        ['field-title', 'Title'], ['field-status', 'Status'], ['field-actions', 'Actions']
    ].map(([field, title]) => ({ field, title }));
    function createRow() {
        const row = {
            cells: columns.map(column => {
                const classes = [column.field];
                classes.contains = value => classes.includes(value);
                classes.toggle = (value, enabled) => {
                    if (enabled && !classes.includes(value)) classes.push(value);
                    if (!enabled && classes.includes(value)) classes.splice(classes.indexOf(value), 1);
                };
                return { classList: classes, value: column.field + '-value' };
            }),
            insertBefore(cell, before) {
                this.cells.splice(this.cells.indexOf(cell), 1);
                this.cells.splice(before ? this.cells.indexOf(before) : this.cells.length, 0, cell);
            }
        };
        return row;
    }
    function createList() {
        return {
            options: [],
            get selectedOptions() { return this.options.filter(option => option.selected); },
            set textContent(value) { assert.equal(value, ''); this.options = []; },
            appendChild(option) { this.options.push(option); },
            focus() {}
        };
    }
    const elements = {
        issueColumnsAvailable: createList(), issueColumnsSelected: createList(),
        issueColumnMessage: {}, issueColumnStatus: {}, issueColumnAdd: {}, issueColumnRemove: {},
        issueColumnUp: {}, issueColumnDown: {}, exportColumns: {}, exportMessage: {},
        overall_request_details: { rows: [createRow(), createRow(), { cells: [] }] },
        issueColumnPicker: {
            open: false,
            showModal() { this.open = true; },
            close() { this.open = false; context.CMF_ISSUE_COLUMN_DRAFT = null; }
        }
    };
    const context = vm.createContext({
        document: {
            getElementById: id => elements[id],
            createElement: tag => { assert.equal(tag, 'option'); return { selected: false }; },
            querySelectorAll: () => elements.overall_request_details.rows[0].cells
        },
        localStorage: {
            getItem: key => storage.get(key) || null,
            setItem: (key, value) => storage.set(key, value)
        },
        console: { warn: (...args) => notices.push(args), error: (...args) => notices.push(args) },
        requestAnimationFrame: callback => callback(),
        formatIssueLongCells() {},
        getIssueColumns: () => columns
    });
    vm.runInContext(`
        var CMF_HIDDEN_COLS_KEY = 'cmf_issue_columns_v2';
        var CMF_COLUMN_LAYOUT_KEY = 'cmf_issue_column_layout_v1';
        var CMF_ISSUE_COLUMN_DRAFT = null;
    `, context);
    for (const name of [
        'setIssueColumnNotice', 'readIssueColumnLayout', 'saveIssueColumnLayout',
        'setIssueColumnVisible', 'restoreHiddenColumns', 'openIssueColumnPicker',
        'renderIssueColumnChoices', 'updateIssueColumnButtons', 'moveIssueColumns',
        'reorderIssueColumns', 'resetIssueColumnChoices', 'applyIssueColumnChoices',
        'handleIssueColumnDialogKeydown', 'prepareIssueViewExport'
    ]) {
        const definition = page.match(new RegExp('        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'));
        assert.ok(definition, name);
        vm.runInContext(definition[0]
            .replace(/<%= hfIssueExportColumns.ClientID %>/g, 'exportColumns')
            .replace(/<%= issueExportMessage.ClientID %>/g, 'exportMessage'), context);
    }
    function select(id, fields) {
        elements[id].options.forEach(option => { option.selected = fields.includes(option.value); });
        context.updateIssueColumnButtons();
    }
    return { context, elements, storage, notices, select, columns };
}

test('column personalization uses an accessible modal and stages edits until OK', () => {
    assert.match(page, /<dialog id="issueColumnPicker"[^>]*aria-labelledby="issueColumnTitle"/);
    assert.match(page, /onclose="CMF_ISSUE_COLUMN_DRAFT = null"/);
    assert.match(page, /id="issueColumnsAvailable" multiple/);
    assert.match(page, /id="issueColumnsSelected" multiple/);
    const { context, elements, storage, select } = createColumnPicker();
    context.openIssueColumnPicker();
    select('issueColumnsSelected', ['field-status']);
    context.moveIssueColumns(false);
    select('issueColumnsSelected', ['field-title']);
    context.reorderIssueColumns(-1);
    assert.equal(elements.issueColumnPicker.open, true);
    assert.equal(storage.size, 0);
    assert.equal(elements.overall_request_details.rows[0].cells[1].value, 'field-milestone-value');
    assert.equal(elements.overall_request_details.rows[0].cells[3].classList.contains('hidden'), false);
    elements.issueColumnPicker.close();
    context.openIssueColumnPicker();
    assert.equal(elements.issueColumnsSelected.options.length, 5);
    assert.equal(elements.issueColumnsSelected.options[1].value, 'field-milestone');
    assert.equal(elements.issueColumnsAvailable.options.length, 0);
});

test('OK applies and persists visibility and order consistently for headers, cells, and exports', () => {
    const { context, elements, storage, select } = createColumnPicker();
    context.openIssueColumnPicker();
    select('issueColumnsSelected', ['field-status']);
    context.moveIssueColumns(false);
    select('issueColumnsSelected', ['field-title']);
    context.reorderIssueColumns(-1);
    context.reorderIssueColumns(-1);
    context.applyIssueColumnChoices();
    assert.equal(elements.issueColumnPicker.open, false);
    const saved = JSON.parse(storage.get('cmf_issue_column_layout_v1'));
    assert.deepEqual(saved.order, ['field-title', 'field-sno', 'field-milestone', 'field-actions', 'field-status']);
    assert.deepEqual(saved.hidden, ['field-status']);
    for (const row of elements.overall_request_details.rows.slice(0, 2)) {
        assert.deepEqual(row.cells.map(cell => cell.value), saved.order.map(field => field + '-value'));
        assert.equal(row.cells.at(-1).classList.contains('hidden'), true);
    }
    assert.equal(context.prepareIssueViewExport(), true);
    assert.deepEqual(JSON.parse(elements.exportColumns.value), ['field-title', 'field-sno', 'field-milestone']);
    context.openIssueColumnPicker();
    assert.deepEqual(elements.issueColumnsSelected.options.map(option => option.value), saved.order.slice(0, -1));
    assert.equal(elements.issueColumnsAvailable.options[0].value, 'field-status');
});

test('moving multiple columns preserves relative order and enforces a visible column', () => {
    const { context, elements, select } = createColumnPicker();
    context.openIssueColumnPicker();
    select('issueColumnsSelected', ['field-milestone', 'field-title']);
    context.reorderIssueColumns(-1);
    assert.deepEqual(elements.issueColumnsSelected.options.map(option => option.value),
        ['field-milestone', 'field-title', 'field-sno', 'field-status', 'field-actions']);
    context.reorderIssueColumns(1);
    assert.deepEqual(elements.issueColumnsSelected.options.map(option => option.value),
        ['field-sno', 'field-milestone', 'field-title', 'field-status', 'field-actions']);
    select('issueColumnsSelected', elements.issueColumnsSelected.options.map(option => option.value));
    context.moveIssueColumns(false);
    assert.equal(elements.issueColumnsSelected.options.length, 5);
    assert.match(elements.issueColumnMessage.textContent, /at least one/);
    select('issueColumnsSelected', ['field-title', 'field-status']);
    context.moveIssueColumns(false);
    assert.deepEqual(elements.issueColumnsAvailable.options.map(option => option.value), ['field-status', 'field-title']);
    select('issueColumnsAvailable', ['field-status', 'field-title']);
    context.moveIssueColumns(true);
    assert.deepEqual(elements.issueColumnsSelected.options.map(option => option.value),
        ['field-sno', 'field-milestone', 'field-actions', 'field-status', 'field-title']);
    assert.equal(elements.issueColumnDown.disabled, true);
});

test('reset restores the original column order and visibility only after confirmation', () => {
    const initial = JSON.stringify({ order: ['field-title', 'field-sno'], hidden: ['field-status'] });
    const { context, elements, storage, columns, select } = createColumnPicker({ cmf_issue_column_layout_v1: initial });
    context.restoreHiddenColumns();
    context.openIssueColumnPicker();
    context.resetIssueColumnChoices();
    assert.equal(elements.issueColumnsAvailable.options.length, 0);
    assert.equal(storage.get('cmf_issue_column_layout_v1'), initial);
    elements.issueColumnPicker.close();
    assert.equal(elements.overall_request_details.rows[0].cells[0].value, 'field-title-value');
    context.openIssueColumnPicker();
    context.resetIssueColumnChoices();
    context.applyIssueColumnChoices();
    assert.deepEqual(JSON.parse(storage.get('cmf_issue_column_layout_v1')), {
        order: columns.map(column => column.field), hidden: []
    });
    context.setIssueColumnVisible('field-title', false);
    context.openIssueColumnPicker();
    select('issueColumnsSelected', ['field-sno']);
    assert.equal(elements.issueColumnsAvailable.options[0].value, 'field-title');
    assert.equal(elements.overall_request_details.rows[0].cells[2].classList.contains('hidden'), true);
});

test('legacy hidden-column preferences migrate and stale or duplicate fields do not affect the layout', () => {
    const { context, elements, storage } = createColumnPicker({
        cmf_issue_columns_v2: JSON.stringify(['field-status', 'field-obsolete', 'field-status'])
    });
    context.openIssueColumnPicker();
    assert.deepEqual(elements.issueColumnsAvailable.options.map(option => option.value), ['field-status']);
    context.applyIssueColumnChoices();
    const saved = JSON.parse(storage.get('cmf_issue_column_layout_v1'));
    assert.deepEqual(saved.hidden, ['field-status']);
    assert.equal(saved.order.length, 5);
    storage.set('cmf_issue_column_layout_v1', JSON.stringify({
        order: ['field-title', 'field-obsolete', 'field-title'], hidden: ['field-obsolete']
    }));
    context.restoreHiddenColumns();
    assert.equal(elements.overall_request_details.rows[0].cells[0].value, 'field-title-value');
    assert.equal(elements.overall_request_details.rows[0].cells.length, 5);
});

test('storage failures are reported without applying or closing an unsaved draft', () => {
    const { context, elements, notices, select } = createColumnPicker();
    context.openIssueColumnPicker();
    select('issueColumnsSelected', ['field-title']);
    context.moveIssueColumns(false);
    context.localStorage.setItem = () => { throw Object.assign(new Error('Storage disabled'), { name: 'SecurityError' }); };
    context.applyIssueColumnChoices();
    assert.equal(elements.issueColumnPicker.open, true);
    assert.match(elements.issueColumnMessage.textContent, /Unable to save columns/);
    assert.equal(elements.overall_request_details.rows[0].cells[2].classList.contains('hidden'), false);
    assert.equal(notices.length, 1);
    context.localStorage.getItem = () => '{invalid json';
    context.restoreHiddenColumns();
    assert.match(elements.issueColumnMessage.textContent, /Unable to load saved columns/);
    assert.equal(notices.length, 2);
    context.localStorage.setItem = () => { throw new Error('Unexpected failure'); };
    assert.throws(() => context.saveIssueColumnLayout({ order: [], hidden: [] }), /Unexpected failure/);
});

test('Escape discards the draft without reaching other dialogs and Enter in a list confirms it', () => {
    const { context, elements, storage, select } = createColumnPicker();
    context.openIssueColumnPicker();
    select('issueColumnsSelected', ['field-status']);
    context.moveIssueColumns(false);
    let prevented = 0;
    let stopped = 0;
    context.handleIssueColumnDialogKeydown({
        key: 'Escape', preventDefault() { prevented++; }, stopPropagation() { stopped++; }
    });
    assert.equal(elements.issueColumnPicker.open, false);
    assert.equal(storage.size, 0);
    assert.equal(stopped, 1);
    context.openIssueColumnPicker();
    context.handleIssueColumnDialogKeydown({
        key: 'Enter', target: { tagName: 'SELECT' }, preventDefault() { prevented++; }
    });
    assert.equal(prevented, 2);
    assert.equal(elements.issueColumnPicker.open, false);
    assert.equal(JSON.parse(storage.get('cmf_issue_column_layout_v1')).hidden.length, 0);
});

const compiler = path.join(process.env.WINDIR || 'C:\\Windows', 'Microsoft.NET', 'Framework64', 'v4.0.30319', 'csc.exe');
test('compiled server helpers preserve export order and values and choose WCAG-readable progress text', {
    skip: !fs.existsSync(compiler)
}, () => {
    const methods = ['GetIssueProgressTextColor', 'GetIssueExportCellText', 'GetIssueExportColumnIndexes', 'GetIssueFieldClass'].map(name => {
        const match = server.match(new RegExp('    private static [^\\r\\n]+ ' + name + '\\([^\\r\\n]*\\)[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(match, name);
        return match[0];
    }).join('\n');
    const source = `
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
public class IssueListHelperTests {
${methods}
static double L(System.Drawing.Color c) {
    double[] a = {c.R/255.0,c.G/255.0,c.B/255.0};
    for(int i=0;i<3;i++) a[i]=a[i]<=0.04045?a[i]/12.92:Math.Pow((a[i]+0.055)/1.055,2.4);
    return 0.2126*a[0]+0.7152*a[1]+0.0722*a[2];
}
public static void Main() {
    foreach(string name in new[]{"red","green","yellow","orange","black","white","#123456"}) {
        var bg=System.Drawing.ColorTranslator.FromHtml(name);
        var fg=GetIssueProgressTextColor(bg);
        double contrast=(Math.Max(L(bg),L(fg))+0.05)/(Math.Min(L(bg),L(fg))+0.05);
        if(contrast<4.5) throw new Exception("Insufficient contrast: "+name);
    }
    var cell=new TableCell();
    cell.Controls.Add(new LiteralControl("<button>AI Debug Summary</button><div>First &amp; second</div><div>Unconfirmed finding.</div>"));
    string text=GetIssueExportCellText(cell);
    if(text!="First & second\\nUnconfirmed finding.") throw new Exception("Export text changed: "+text);
    if(GetIssueExportCellText(new TableCell{Text="&nbsp;"})!="unknown") throw new Exception("Blank export value.");
    if(GetIssueExportCellText(new TableCell{Text="=literal"})!="=literal") throw new Exception("Literal value changed.");
    var grid=new GridView();
    foreach(string field in new[]{"field-sno","field-title","field-status","field-actions","field-unavailable"}) {
        var column=new BoundField();
        column.ItemStyle.CssClass=field;
        column.Visible=field!="field-unavailable";
        grid.Columns.Add(column);
    }
    var indexes=GetIssueExportColumnIndexes(grid.Columns,new[]{"field-title",null,"field-sno","field-title","field-actions","field-unknown","field-unavailable","field-status"});
    if(string.Join(",",indexes)!="1,0,2") throw new Exception("Export order or whitelist changed.");
    if(GetIssueExportColumnIndexes(grid.Columns,null).Count!=0) throw new Exception("Null export selection.");
    Console.WriteLine("PASS: Contrast >=4.5, complete export values, and selected export order.");
}
}`;
    const directory = fs.mkdtempSync(path.join(os.tmpdir(), 'cmf-issue-helpers-'));
    const input = path.join(directory, 'Tests.cs');
    const output = path.join(directory, 'Tests.exe');
    try {
        fs.writeFileSync(input, source);
        const build = spawnSync(compiler, ['/nologo', '/target:exe', '/out:' + output, '/r:System.Web.dll', '/r:System.Drawing.dll', input], { encoding: 'utf8' });
        assert.equal(build.status, 0, build.stdout + build.stderr);
        const run = spawnSync(output, [], { encoding: 'utf8' });
        assert.equal(run.status, 0, run.stdout + run.stderr);
    } finally {
        fs.rmSync(input, { force: true });
        fs.rmSync(output, { force: true });
        fs.rmdirSync(directory);
    }
});
