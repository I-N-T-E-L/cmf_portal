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
    assert.match(server, /field != "field-actions" && selected.Contains\(field\)/);
    assert.match(server, /ScriptManager1.RegisterPostBackControl\(btnIssueViewExport\)/);
});

const compiler = path.join(process.env.WINDIR || 'C:\\Windows', 'Microsoft.NET', 'Framework64', 'v4.0.30319', 'csc.exe');
test('compiled server helpers preserve values and choose WCAG-readable progress text', {
    skip: !fs.existsSync(compiler)
}, () => {
    const methods = ['GetIssueProgressTextColor', 'GetIssueExportCellText'].map(name => {
        const match = server.match(new RegExp('    private static [^\\r\\n]+ ' + name + '\\([^\\r\\n]*\\)[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(match, name);
        return match[0];
    }).join('\n');
    const source = `
using System;
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
    Console.WriteLine("PASS: Contrast >=4.5 and clean, complete export values.");
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
