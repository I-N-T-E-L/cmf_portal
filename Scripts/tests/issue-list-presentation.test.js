const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const root = path.join(__dirname, '..', '..');
const page = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx'), 'utf8');
const server = fs.readFileSync(path.join(root, 'CMF_Web_portal.aspx.cs'), 'utf8');
const theme = fs.readFileSync(path.join(root, 'Content', 'cmf-portal-theme.css'), 'utf8');

test('summary top-row format headings retain LOS only on unique counts', () => {
    assert.equal((page.match(/<span class="ccip-header-format">Unique\(LOS\)\+Duplicate<\/span>/g) || []).length, 2);
    assert.doesNotMatch(page, /Unique\(LOS\)\+Duplicate\(LOS\)/);
    assert.match(page, /Impl\/Vrf\\'d<span class="ccip-header-format">Unique\+Duplicate<\/span>/);
});

test('component and design summaries abbreviate verified in display headings without changing source keys', () => {
    const definition = page.match(/var renderColumns = columns\.map\(function \(column\) \{[\s\S]*?\}\);/);
    assert.ok(definition);
    const columns = ['Ingredient', 'Milestone:::Open', 'Milestone:::Impl/Verified', 'Impl/Verified'];
    const context = vm.createContext({ columns, isComponentSummary: true, isDesignSummary: false });
    vm.runInContext(definition[0], context);
    assert.deepEqual(Array.from(context.renderColumns), ['Ingredient', 'Milestone:::Open', "Milestone:::Impl/Vrf'd", "Impl/Vrf'd"]);
    assert.equal(columns[2], 'Milestone:::Impl/Verified');
    context.isComponentSummary = false;
    context.isDesignSummary = true;
    vm.runInContext(definition[0], context);
    assert.deepEqual(Array.from(context.renderColumns), ['Ingredient', 'Milestone:::Open', "Milestone:::Impl/Vrf'd", "Impl/Vrf'd"]);
    assert.equal(columns[3], 'Impl/Verified');
    context.isDesignSummary = false;
    vm.runInContext(definition[0], context);
    assert.deepEqual(Array.from(context.renderColumns), columns);
});

test('collapsed-menu dashboard keeps chart rendering and pointer targets in unscaled CSS coordinates', () => {
    const compact = page.match(/\.portal-layout-shell\.main-menu-collapsed #homeWelcomePanel \.ccip-dashboard \{([^}]*)\}/);
    assert.ok(compact);
    assert.match(compact[1], /zoom: 1;/);
    assert.doesNotMatch(compact[1], /--ccip-fit-scale-1080|zoom: var\(/);
    assert.match(compact[1], /transform: none;/);
    assert.doesNotMatch(compact[1], /transform:\s*scale\(/);
    assert.match(compact[1], /width: 100%;/);
    assert.doesNotMatch(compact[1], /width: calc\(100% \/ /);
});

test('shared dashboard table scrollbars match the visible OEM scrollbar and stay stable on hover', () => {
    const base = page.match(/#homeLegacyTablesGrid \.ccip-live-table-wrap,[\s\S]*?#homeLegacyOverview \.ccip-inline-side-stack \.ccip-live-table-wrap \{([^}]*)\}/);
    assert.ok(base);
    assert.match(base[1], /scrollbar-gutter: stable;/);
    assert.match(base[1], /scrollbar-width: auto;/);
    assert.match(base[1], /scrollbar-color: auto;/);
    const scrollbar = page.match(/#homeLegacyTablesGrid \.ccip-live-table-wrap::-webkit-scrollbar,[\s\S]*?#homeLegacyOverview \.ccip-inline-side-stack \.ccip-live-table-wrap::-webkit-scrollbar \{([^}]*)\}/);
    assert.ok(scrollbar);
    assert.match(scrollbar[1], /width: 8px;/);
    assert.match(scrollbar[1], /height: 8px;/);
    const hover = page.match(/#homeLegacyTablesGrid \.ccip-live-table-wrap:hover,[\s\S]*?#homeLegacyOverview \.ccip-inline-side-stack \.ccip-live-table-wrap:hover \{([^}]*)\}/);
    assert.ok(hover);
    assert.match(hover[1], /scrollbar-width: auto;/);
    assert.match(hover[1], /scrollbar-color: auto;/);
    const thumb = page.match(/#homeLegacyTablesGrid \.ccip-live-table-wrap::-webkit-scrollbar-thumb,[\s\S]*?#homeLegacyOverview \.ccip-inline-side-stack \.ccip-live-table-wrap::-webkit-scrollbar-thumb \{([^}]*)\}/);
    assert.ok(thumb);
    assert.match(thumb[1], /background: #cbd3dd;/);
    assert.match(thumb[1], /border-radius: 999px;/);
    const track = page.match(/#homeLegacyTablesGrid \.ccip-live-table-wrap::-webkit-scrollbar-track,[\s\S]*?#homeLegacyOverview \.ccip-inline-side-stack \.ccip-live-table-wrap::-webkit-scrollbar-track \{([^}]*)\}/);
    assert.ok(track);
    assert.match(track[1], /background: transparent;/);
    const component = page.match(/#homeComponentSummaryTable\.ccip-live-table-wrap \{([^}]*)\}/);
    assert.ok(component);
    assert.match(component[1], /scrollbar-width: auto !important;/);
    assert.match(component[1], /scrollbar-color: auto !important;/);
});

test('Overall pending age legend sits above the scrollable breakdown', () => {
    assert.match(page, /<div class="overall-pending-bottom">\s*<div id="homeOverallPendingLegend"[^>]*><\/div>\s*<div id="homeOverallPendingBreakdown"[^>]*><\/div>/);
    const layout = page.match(/\.overall-pending-bottom \{([^}]*)\}/);
    assert.ok(layout);
    assert.match(layout[1], /grid-template-rows: auto minmax\(0, 1fr\);/);
    const legend = page.match(/#homeOverallPendingLegend \{([^}]*)\}/);
    assert.ok(legend);
    assert.match(legend[1], /justify-self: end;/);
});

test('platform pending threshold legend sits opposite its title above the table', () => {
    const card = page.match(/<section class="ccip-card ccip-inline-summary-card ccip-pending-summary-card">([\s\S]*?)<\/section>/);
    assert.ok(card);
    assert.match(card[1], /<div class="ccip-card-head">\s*<h3[^>]*>[\s\S]*?<\/h3>\s*<div class="component-summary-legend overall-pending-legend pending-summary-threshold-legend"[^>]*><\/div>\s*<\/div>\s*<div id="homePendingSummaryTable"/);
    assert.equal((card[1].match(/pending-summary-threshold-legend/g) || []).length, 1);
    const legend = page.match(/#homeLegacyTablesGrid \.ccip-pending-summary-card \.pending-summary-threshold-legend \{([^}]*)\}/);
    assert.ok(legend);
    assert.match(legend[1], /margin-left: auto;/);
    assert.match(legend[1], /margin-right: var\(--ccip-pending-scrollbar-width, 0px\);/);
    assert.match(legend[1], /flex-shrink: 0;/);
});

test('component summary zero counts retain metric styling without links or hover behavior', () => {
    const start = page.indexOf('            function hasPositiveComponentMetricValue(');
    const end = page.indexOf('            function parseComponentOpenMetricParts(', start);
    assert.ok(start >= 0 && end > start);
    const context = vm.createContext({ escapeHtml: String });
    vm.runInContext(page.slice(start, end), context);
    for (const [issueType, value] of [
        ['OpenCombined', 0], ['OpenCombined', '0'], ['OpenCombined', '0(0)+0(0)'],
        ['ImplementedCombined', '0'], ['ImplementedCombined', '0+0']
    ]) {
        const html = context.buildComponentSummaryMetricLinkHtml('HW', 'PV', issueType, value);
        assert.equal(html, '<span class="ccip-component-summary-link">0</span>');
        assert.doesNotMatch(html, /<a\b|href=|onclick=|tabindex=|role=/);
    }
    for (const [issueType, value, display] of [
        ['OpenCombined', '2(1)+0(0)', '2(1)'], ['ImplementedCombined', '3+0', '3']
    ]) {
        const html = context.buildComponentSummaryMetricLinkHtml('HW', 'PV', issueType, value);
        assert.match(html, /^<a href="javascript:void\(0\);" class="ccip-component-summary-link"/);
        assert.ok(html.includes('onclick="return showCMFIssues('));
        assert.ok(html.endsWith('>' + display + '</a>'));
    }
    const style = page.match(/\.ccip-component-summary-link \{([^}]*)\}/);
    assert.ok(style);
    assert.match(style[1], /color: #1d4ed8;/);
    assert.match(style[1], /font-weight: 800;/);
    assert.match(style[1], /text-decoration: none;/);
    assert.match(page, /a\.ccip-component-summary-link:hover,\s*a\.ccip-component-summary-link:focus/);
    assert.doesNotMatch(page, /^\.ccip-component-summary-link:(?:hover|focus)/m);
});

test('Design Summary zero counts retain metric styling without links or pointer behavior', () => {
    const start = page.indexOf('        function buildDesignSummaryMetricLinkHtml(');
    const end = page.indexOf('        function renderOverallOemTable(', start);
    assert.ok(start >= 0 && end > start);
    const context = vm.createContext({ escapeHtml: String });
    vm.runInContext(page.slice(start, end), context);
    for (const design of ['Design A', 'Total']) {
        for (const column of ['NVLH_PV', 'Impl/Verified', "Impl/Vrf'd"]) {
            for (const value of [0, '0', ' 0 ']) {
                const html = context.buildDesignSummaryMetricLinkHtml(design, column, value);
                const totalClass = design === 'Total' ? ' is-total' : '';
                assert.equal(html, '<span class="ccip-design-metric-link' + totalClass + '">0</span>');
                assert.doesNotMatch(html, /<a\b|href=|onclick=|tabindex=|role=/);
            }
            const html = context.buildDesignSummaryMetricLinkHtml(design, column, 2);
            const handler = column === 'Impl/Verified' ? 'showImplementedVerifiedDetailsModal' : 'showDriverDetailsModal';
            assert.match(html, /^<a href="javascript:void\(0\);" class="ccip-design-metric-link/);
            assert.ok(html.includes('onclick="return ' + handler + '('));
            assert.ok(html.endsWith('>2</a>'));
        }
    }
    for (const value of ['', null, undefined]) {
        assert.equal(context.buildDesignSummaryMetricLinkHtml('Design A', 'NVLH_PV', value), '');
    }
    const style = page.match(/\.ccip-design-metric-link \{([^}]*)\}/);
    assert.ok(style);
    assert.match(style[1], /color: #1d4ed8;/);
    assert.match(style[1], /font-weight: 700;/);
    assert.match(style[1], /text-decoration: none;/);
    assert.doesNotMatch(style[1], /cursor: pointer/);
    assert.match(page, /a\.ccip-design-metric-link \{\s*cursor: pointer;/);
    assert.match(page, /a\.ccip-design-metric-link:hover \{/);
    assert.doesNotMatch(page, /^\.ccip-design-metric-link:hover/m);
});

test('Overall component summary sizes columns to fit both header rows', () => {
    const rule = page.match(/#homeLegacyTablesGrid \.ccip-live-table\.ccip-component-summary-table \{([^}]*)\}/);
    assert.ok(rule);
    assert.match(rule[1], /table-layout: auto;/);
});

test('Design Summary fills available width while preserving each existing column minimum', () => {
    const wrappers = page.match(/#homeLegacyTablesGrid \.ccip-inline-side-stack \.ccip-inline-summary-card \.ccip-live-table-wrap \{([^}]*)\}/);
    assert.ok(wrappers);
    assert.match(wrappers[1], /width: 100%;/);
    const gutters = page.match(/#homeLegacyTablesGrid #homeComponentSummaryTable,\s*#homeLegacyTablesGrid #homePendingSummaryTable \{([^}]*)\}/);
    assert.ok(gutters);
    assert.match(gutters[1], /scrollbar-gutter: auto;/);
    const sizing = page.match(/                var designOtherColCount = Math\.max\(1, columns\.length - 1\);[\s\S]*?html \+= '<\/colgroup>';/);
    assert.ok(sizing);
    const columns = ['Design Name', 'SW Image Freeze', 'Support Model', 'NVLH_PV', 'NVLH_QS', "Impl/Vrf'd"];
    const headerWidths = [108, 104, 65, 67, 77];
    const context = vm.createContext({
        columns,
        renderColumns: columns,
        tableClasses: 'ccip-live-table ccip-design-summary-table',
        host: { clientWidth: 828 },
        document: { createElement: () => ({ getContext: () => ({
            measureText: text => ({ width: headerWidths[columns.slice(1).map(column => column.toUpperCase()).indexOf(text)] - 16 - (text.length - 1) * 0.2 - 0.25 })
        }) }) }
    });
    vm.runInContext(sizing[0], context);
    assert.match(context.html, /style="width:100%; min-width:724px; table-layout:fixed;"/);
    const widths = Array.from(context.html.matchAll(/<col style="width:([\d.]+)px; min-width:([\d.]+)px;">/g), match => [Number(match[1]), Number(match[2])]);
    assert.deepEqual(widths, [231, 108, 104, 65, 67, 77, 72].map(width => [width, width]));
    assert.doesNotMatch(context.html, /max-width:/);
    const scrollLayout = page.match(/#homeLegacyTablesGrid \.ccip-summary-canvas \.ccip-design-summary-card #homeDesignSummaryTable \{([^}]*)\}/);
    assert.ok(scrollLayout);
    assert.match(scrollLayout[1], /scrollbar-gutter: auto;/);
    const hover = page.match(/#homeLegacyTablesGrid \.ccip-summary-canvas \.ccip-design-summary-card #homeDesignSummaryTable\.ccip-hover-width-lock \{([^}]*)\}/);
    assert.ok(hover);
    assert.match(hover[1], /overflow-x: auto;/);
    assert.doesNotMatch(page, /#homeDesignSummaryTable\.ccip-hover-width-lock \.ccip-live-table \{/);
});

test('Component Open CMF fills available width and grows its minimum for extra milestone columns', () => {
    const sizing = page.match(/                var firstHeader = String\(renderColumns\[0\][\s\S]*?html \+= '<\/colgroup>';/);
    assert.ok(sizing);
    const wrapper = page.match(/#homeLegacyTablesGrid \.ccip-inline-side-stack \.ccip-inline-summary-card \.ccip-live-table-wrap \{([^}]*)\}/);
    assert.ok(wrapper);
    assert.match(wrapper[1], /overflow-x: auto;/);
    const groupedColumns = [1, 2, 4, 8, 12].map(count => ['Ingredient', ...Array.from({ length: count }, (_, index) => [
        'Milestone_' + (index + 1) + ':::Open',
        'Milestone_' + (index + 1) + ":::Impl/Vrf'd"
    ]).flat()]);
    for (const renderColumns of [
        ['Ingredient', 'Open', 'Impl/Verified'],
        ...groupedColumns
    ]) {
        const context = vm.createContext({ renderColumns, hasMilestoneColumns: renderColumns[1].includes(':::'), tableClasses: 'ccip-live-table ccip-component-summary-table' });
        vm.runInContext(sizing[0], context);
        assert.match(context.html, /style="width:100%; min-width:calc\(92px \+ [\d.]+ch\);"/);
        assert.equal((context.html.match(/<col style=/g) || []).length, renderColumns.length);
        assert.match(context.html, /<col style="width:92px; min-width:92px;">/);
        const minimum = context.html.match(/min-width:calc\(92px \+ ([\d.]+)ch\)/);
        assert.ok(minimum);
        assert.equal(Number(minimum[1]), Number(((renderColumns.length - 1) * context.uniformDataColCh).toFixed(1)));
        const metricColumns = Array.from(context.html.matchAll(/<col style="width:([\d.]+)ch; min-width:([\d.]+)ch;">/g));
        assert.equal(metricColumns.length, renderColumns.length - 1);
        assert.ok(metricColumns.every(column => Number(column[1]) === context.uniformDataColCh && column[1] === column[2]));
        assert.doesNotMatch(context.html, /max-width:|NaN|undefined/);
    }
});

test('CMF Pending Count fills available width while retaining count and iDST column minima', () => {
    const sizing = page.match(/                var pendingFirstColPx = 94;[\s\S]*?html \+= '<\/colgroup>';/);
    assert.ok(sizing);
    for (const columns of [['Ingredient', 'CMF Pending'], ['Ingredient', 'iDST Name', 'CMF Pending']]) {
        const context = vm.createContext({ columns, renderColumns: columns, pendingValueColumnIndex: columns.length - 1, tableClasses: 'ccip-live-table' });
        vm.runInContext(sizing[0], context);
        assert.match(context.html, /style="width:100%; min-width:calc\(94px \+ [\d.]+ch \+ [\d.]+ch\); table-layout:auto;"/);
        assert.equal((context.html.match(/<col style=/g) || []).length, columns.length);
        assert.match(context.html, /<col style="width:94px; min-width:94px;">/);
        assert.doesNotMatch(context.html, /max-width:|NaN|undefined/);
    }
});

test('CMF Pending Count leaves unassigned iDST cells blank without changing rows or issue links', () => {
    const cells = page.match(/                    if \(isPendingSummary && renderColumns.length >= 3\) \{\s*(var totalRowIds[\s\S]*?)\r?\n                    \} else/);
    assert.ok(cells);
    const cases = [
        ['HW', 'Unassigned', '1', ''],
        ['Unassigned', 'UNASSIGNED', '2', ''],
        ['HW', ' Unassigned ', '1', ''],
        ['HW', null, '1', ''],
        ['HW', 'Ok, Arun; Bao, Xulei', '(1)', 'Ok, Arun; Bao, Xulei'],
        ['HW', '<mapped&name>', '1', '&lt;mapped&amp;name&gt;'],
        ['Total', '', '5 (2)', '']
    ];
    for (const [ingredient, idst, value, expected] of cases) {
        const row = [ingredient, idst, value];
        const original = row.slice();
        const calls = [];
        const context = vm.createContext({
            row, html: '', pendingValueColumnIndex: 2, isTotalRow: ingredient === 'Total',
            pendingSummaryAggregateIdsCsv: '14029072573,14029150490',
            pendingSummaryAggregateOverThresholdIdsCsv: '14029072573',
            escapeHtml: text => String(text).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'),
            buildPendingSummaryCellHtml: (...args) => { calls.push(args); return 'linked-count'; }
        });
        vm.runInContext(cells[1], context);
        assert.equal(context.html, '<td>' + ingredient + '</td><td class="ccip-pending-summary-links-col">linked-count</td><td>' + expected + '</td>');
        assert.deepEqual(row, original);
        assert.deepEqual(calls, [[value, ingredient === 'Total', ingredient,
            ingredient === 'Total' ? '14029072573,14029150490' : '',
            ingredient === 'Total' ? '14029072573' : '']]);
    }
});

test('pending threshold legend tracks only the required scrollbar gutter and reuses its observer', () => {
    const definition = page.match(/        function applyPendingSummaryLegendGutter\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    const properties = new Map();
    const card = { style: { setProperty: (name, value) => properties.set(name, value) } };
    const host = { offsetWidth: 420, clientWidth: 420, closest: () => card };
    const observers = [];
    const context = vm.createContext({ ResizeObserver: class {
        constructor(callback) { this.callback = callback; observers.push(this); }
        observe(element) { assert.equal(element, host); }
    } });
    vm.runInContext(definition[0], context);
    context.applyPendingSummaryLegendGutter(host);
    assert.equal(properties.get('--ccip-pending-scrollbar-width'), '0px');
    host.clientWidth = 412;
    observers[0].callback();
    assert.equal(properties.get('--ccip-pending-scrollbar-width'), '8px');
    host.clientWidth = 420;
    context.applyPendingSummaryLegendGutter(host);
    assert.equal(properties.get('--ccip-pending-scrollbar-width'), '0px');
    assert.equal(observers.length, 1);
    assert.match(page, /if \(isPendingSummary\) \{\s*applyPendingSummaryLegendGutter\(host\);/);
});

test('Design Summary keeps dynamic column minima aligned when milestones are removed, added or renamed', () => {
    const sizing = page.match(/                var designOtherColCount = Math\.max\(1, columns\.length - 1\);[\s\S]*?html \+= '<\/colgroup>';/);
    assert.ok(sizing);
    const milestoneSets = [
        [],
        ['NVLH_PV'],
        ['NVLH_QS'],
        ['NVLH_QS', 'NVLH_PV'],
        ['Customer_Qualification_Release_WW48', 'Beta'],
        Array.from({ length: 12 }, (_, index) => 'Platform_Milestone_' + (index + 1))
    ];
    for (const milestones of milestoneSets) {
        const columns = ['Design Name', 'SW Image Freeze', 'Support Model', ...milestones, 'Impl/Verified'];
        const renderColumns = columns.map(column => column === 'Impl/Verified' ? "Impl/Vrf'd" : column);
        for (const availableWidth of [0, 360, 828, 1200]) {
            for (const canvasAvailable of [true, false]) {
                const context = vm.createContext({
                    columns, renderColumns,
                    tableClasses: 'ccip-live-table ccip-design-summary-table',
                    host: { clientWidth: availableWidth },
                    document: { createElement: () => ({ getContext: () => canvasAvailable ? {
                        measureText: text => ({ width: text.length * 6.5 })
                    } : null }) }
                });
                vm.runInContext(sizing[0], context);
                const widths = Array.from(context.html.matchAll(/<col style="width:([\d.]+)px; min-width:([\d.]+)px;">/g), match => [Number(match[1]), Number(match[2])]);
                const firstMinimum = availableWidth > 0 ? Math.max(180, Math.floor(availableWidth * 0.28)) : 220;
                const expected = [firstMinimum, ...renderColumns.slice(1).map(header => canvasAvailable
                    ? Math.max(64, Math.ceil(header.length * 6.5 + (header.length - 1) * 0.2 + 16))
                    : Math.max(64, header.length * 7 + 16)), 72];
                const scenario = JSON.stringify({ milestones, availableWidth, canvasAvailable });
                assert.equal(widths.length, columns.length + 1, 'Column groups must match data columns plus Edit: ' + scenario);
                assert.deepEqual(widths, expected.map(width => [width, width]), 'Each width must follow its current heading, not its previous position: ' + scenario);
                const minimumTotal = Number(context.html.match(/min-width:(\d+)px; table-layout:fixed/)[1]);
                assert.equal(minimumTotal, expected.reduce((sum, width) => sum + width, 0), 'Table minimum must include every current column exactly once: ' + scenario);
                assert.match(context.html, /style="width:100%;/);
                assert.doesNotMatch(context.html, /NaN|undefined|max-width:/);
            }
        }
    }
});

test('Overall component summary centers only its second-row headings', () => {
    const rule = page.match(/#homeLegacyTablesGrid \.ccip-live-table\.ccip-component-summary-table \.ccip-component-sub-header \{([^}]*)\}/);
    assert.ok(rule);
    assert.match(rule[1], /text-align: center;/);
});

test('component summary subheaders use unscaled CSS heights and resize observation instead of scroll corrections', () => {
    const definition = page.match(/        function applyComponentGroupHeaderSticky\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    function makeTable(height) {
        const firstCell = { style: {}, getBoundingClientRect: () => ({ width: 92 }) };
        const group = {
            style: {}, cssHeight: height, offsetHeight: 50,
            classList: { contains: () => true },
            getBoundingClientRect: () => ({ height: 45 })
        };
        const subCells = [{ style: {} }, { style: {} }];
        const rows = [{ querySelectorAll: () => [firstCell, group] }, { querySelectorAll: () => subCells }];
        const properties = new Map();
        return {
            group, subCells, rows,
            querySelectorAll: () => rows,
            style: {
                setProperty: (name, value) => properties.set(name, value),
                getPropertyValue: name => properties.get(name) || ''
            }
        };
    }
    let table = makeTable('50px');
    const observers = [];
    const host = {
        querySelector: () => table,
        addEventListener: () => assert.fail('Sticky headers must not install per-scroll correction handlers.')
    };
    const context = vm.createContext({
        getComputedStyle: cell => ({ height: cell.cssHeight }),
        ResizeObserver: class {
            constructor(callback) {
                this.callback = callback;
                this.observed = [];
                this.disconnects = 0;
                observers.push(this);
            }
            observe(row) { this.observed.push(row); }
            disconnect() { this.disconnects++; }
        }
    });
    vm.runInContext(definition[0], context);
    context.applyComponentGroupHeaderSticky(host);
    assert.ok(table.subCells.every(cell => cell.style.top === '50px'), 'CSS height must not be replaced by the 45px zoomed screen height.');
    assert.equal(table.style.getPropertyValue('--ccip-component-group-header-height'), '50px');
    assert.equal(observers.length, 1);
    assert.equal(observers[0].observed[0], table.rows[0]);
    table.group.cssHeight = '60.5px';
    observers[0].callback();
    assert.ok(table.subCells.every(cell => cell.style.top === '60.5px'), 'A real header resize updates the offset in CSS pixels.');
    assert.equal(observers.length, 1, 'Resize callbacks must reuse the observer rather than creating an observation loop.');
    assert.equal(observers[0].observed.length, 1);
    table = makeTable('44px');
    context.applyComponentGroupHeaderSticky(host);
    assert.ok(table.subCells.every(cell => cell.style.top === '44px'));
    assert.equal(observers[0].observed[1], table.rows[0], 'A rerender observes the replacement header row.');
    table.group.cssHeight = 'auto';
    observers[0].callback();
    assert.ok(table.subCells.every(cell => cell.style.top === '50px'), 'Fallback measurements also use layout CSS pixels.');
    context.applyComponentGroupHeaderSticky(null);
});

test('ingredient risk heatmap orders platforms by descending total without misaligning cells', () => {
    let markup = '';
    let scroll = null;
    const host = {
        get innerHTML() { return markup; },
        set innerHTML(value) {
            markup = value;
            scroll = value.includes('component-summary-heatmap-scroll') ? { scrollTop: 0, scrollLeft: 0 } : null;
        },
        querySelectorAll: () => [],
        querySelector: selector => selector === '.component-summary-heatmap-scroll' ? scroll : null
    };
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
        const previousScroll = scroll;
        context.renderOverallHeatmap(snapshot);
        assert.notEqual(scroll, previousScroll, 'Redraw replaces the heatmap scroll container.');
        assert.equal(scroll.scrollTop, previousScroll ? 145 : 0, 'LOS redraw must retain vertical scroll.');
        assert.equal(scroll.scrollLeft, previousScroll ? 92 : 0, 'LOS redraw must retain horizontal scroll.');
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
        scroll.scrollTop = 145;
        scroll.scrollLeft = 92;
    }
    portal.overallRiskSubtractLos = false;
    context.renderOverallHeatmap(snapshot);
    assert.equal(scroll.scrollTop, 145, 'Turning LOS off must also preserve vertical scroll.');
    assert.equal(scroll.scrollLeft, 92, 'Turning LOS off must also preserve horizontal scroll.');
    context.renderOverallHeatmap({ HeatmapPlatforms: [], ComponentRiskHeatmap: [] });
    assert.ok(host.innerHTML.includes('No heatmap data available.'));
});

test('ingredient risk heatmap projects separate unique and duplicate open counts within the same scope', () => {
    const projection = server.match(/private void PopulateOverallComponentRiskHeatmap\([\s\S]*?\r?\n    \}/);
    assert.ok(projection);
    const source = projection[0];
    assert.match(source, /SUM\(CASE WHEN cmf_request = 'cmf_ok' THEN 1 ELSE 0 END\) AS open_unique_count/);
    assert.match(source, /SUM\(CASE WHEN cmf_request = 'cmf_duplicate' THEN 1 ELSE 0 END\) AS open_duplicate_count/);
    assert.match(source, /cmf_request = 'cmf_ok' AND[^\n]*AS open_unique_los_count/);
    assert.match(source, /cmf_request = 'cmf_duplicate' AND[^\n]*AS open_duplicate_los_count/);
    assert.match(source, /WHERE status = 'open'\s+AND cmf_request IN \('cmf_ok', 'cmf_duplicate'\)/);
    assert.match(source, /ISNULL\(LTRIM\(RTRIM\(drivers\)\), ''\) <> ''/);
    assert.match(source, /sysdebug LIKE '%customer_must_fix%'/);
    assert.match(source, /GetDashboardDateFilter\(\)\.BuildPredicate\(null\)/);
    assert.match(source, /CmfPlatformCatalog\.CreateReadCommand\(sql, connection\)/);
    assert.match(source, /if \(count <= 0 && duplicateCount <= 0\)/);
    assert.match(source, /row\.DuplicateValues\.Add\(duplicateValue\)/);
    assert.match(source, /row\.DuplicateStatusBreakdowns\.Add\(BuildHeatmapOpenUniqueTooltip\(duplicateValue, duplicateLosValue, true\)\)/);
});

test('ingredient risk heatmap record types update counts, LOS, ordering and drilldowns independently', () => {
    assert.match(page, /id="homeOverallHeatmapUnique" checked="checked"/);
    assert.match(page, /id="homeOverallHeatmapDuplicates" autocomplete="off"/);
    const unique = { checked: true };
    const duplicates = { checked: false };
    let markup = '';
    let scroll = { scrollTop: 145, scrollLeft: 92 };
    const host = {
        get innerHTML() { return markup; },
        set innerHTML(value) { markup = value; scroll = { scrollTop: 0, scrollLeft: 0 }; },
        querySelectorAll: () => [],
        querySelector: selector => selector === '.component-summary-heatmap-scroll' ? scroll : null
    };
    let popup;
    const context = vm.createContext({
        document: { getElementById: id => ({ homeOverallHeatmap: host, homeOverallHeatmapUnique: unique, homeOverallHeatmapDuplicates: duplicates })[id] },
        window: { CMF_PORTAL: { overallRiskSubtractLos: true } },
        normalizeComponentRiskThresholds: () => ({ low: 0, med: 3, high: 6 }),
        getOverallHeatClassFromOpenUniqueByThreshold: value => 'risk-' + value,
        escapeHtml: value => String(value),
        openSummaryIssueLinksPopup: (metric, label, options) => { popup = options; }
    });
    for (const name of ['renderOverallHeatmap', 'openOverallHeatmapIssueLinksPopup']) {
        const definition = page.match(new RegExp('        function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n        \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    const snapshot = {
        HeatmapPlatforms: ['Alpha', 'Beta'],
        ComponentRiskHeatmap: [
            { Ingredient: 'Driver', Values: [2, 4], DuplicateValues: [6, 1], StatusBreakdowns: ['Open Unique: 2\nLOS: 1', 'Open Unique: 4\nLOS: 2'], DuplicateStatusBreakdowns: ['Open Duplicates: 6\nLOS: 3', 'Open Duplicates: 1\nLOS: 1'] },
            { Ingredient: 'Duplicate Only', Values: [0, 0], DuplicateValues: [0, 2], StatusBreakdowns: ['No issues', 'No issues'], DuplicateStatusBreakdowns: ['No issues', 'Open Duplicates: 2\nLOS: 1'] }
        ]
    };
    const original = JSON.stringify(snapshot);
    const cases = [
        { unique: true, duplicates: false, headers: ['Beta', 'Alpha'], values: ['4 (2)', '2 (1)'], risks: [2, 1] },
        { unique: true, duplicates: true, headers: ['Alpha', 'Beta'], values: ['8 (4)', '5 (3)', '0', '2 (1)'], risks: [4, 2, 0, 1] },
        { unique: false, duplicates: true, headers: ['Alpha', 'Beta'], values: ['6 (3)', '1 (1)', '0', '2 (1)'], risks: [3, 0, 0, 1] },
        { unique: false, duplicates: false, headers: ['Alpha', 'Beta'], values: ['0', '0'], risks: [0, 0] }
    ];
    context.renderOverallHeatmap(snapshot);
    for (const selected of cases) {
        unique.checked = selected.unique;
        duplicates.checked = selected.duplicates;
        duplicates.onchange();
        const headers = Array.from(markup.matchAll(/class="overall-heatmap-platform">([^<]*)/g), match => match[1]);
        const tiles = Array.from(markup.matchAll(/<div class="overall-heatmap-tile ([^"]*)"([^>]*)>([^<]*)<\/div>/g));
        assert.deepEqual(headers, selected.headers);
        assert.deepEqual(tiles.map(tile => tile[3]), selected.values);
        assert.deepEqual(tiles.map(tile => Number(tile[1].match(/risk-(\d+)/)[1])), selected.risks);
        assert.equal(scroll.scrollTop, 145);
        assert.equal(scroll.scrollLeft, 92);
        if (!selected.unique && !selected.duplicates) {
            assert.ok(tiles.every(tile => !tile[2].includes('data-heatmap-clickable')));
        }
        context.openOverallHeatmapIssueLinksPopup('Driver', headers[0]);
        assert.equal(popup.includeUnique, selected.unique);
        assert.equal(popup.includeDuplicates, selected.duplicates);
        assert.equal(popup.ingredient, 'Driver');
        assert.equal(popup.platformLabel, headers[0]);
        assert.equal(JSON.stringify(snapshot), original);
    }
    unique.checked = true;
    duplicates.checked = false;
    context.renderOverallHeatmap({ HeatmapPlatforms: ['Alpha'], ComponentRiskHeatmap: [{ Ingredient: 'Updated', Values: [9] }] });
    unique.onchange();
    assert.ok(markup.includes('>Updated</div>'));
    assert.ok(markup.includes('>9</div>'));
    assert.ok(!markup.includes('>Driver</div>'));
    assert.match(page, /includeUnique: sourceType === 'overall-heatmap' \? popupOptions.includeUnique !== false : true/);
    assert.match(page, /includeDuplicates: sourceType === 'overall-heatmap' \? popupOptions.includeDuplicates === true/);
});

test('OEM duplicate counts share dashboard scope but stay separate from unique metrics and trends', () => {
    const definition = server.match(/    private void PopulateOverallOemSummary\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(definition);
    const sql = definition[0];
    assert.match(sql, /IN \('cmf_ok', 'cmf_duplicate'\)/);
    assert.match(sql, /FROM issue_base\s+WHERE request_norm = 'cmf_ok'/);
    assert.match(sql, /duplicate_agg AS \([\s\S]*?FROM issue_base\s+WHERE request_norm = 'cmf_duplicate'/);
    assert.match(sql, /status_norm NOT IN \('complete', 'rejected', 'implemented', 'future'\)[^\n]*AS duplicate_open_issues/);
    assert.match(sql, /status_norm IN \('implemented', 'future'\)[^\n]*AS duplicate_implemented/);
    assert.match(sql, /status_norm IN \('complete', 'rejected'\)[^\n]*AS duplicate_closed/);
    assert.match(sql, /FULL OUTER JOIN duplicate_agg d\s+ON d.oem = COALESCE\(i.oem, p.oem\)/);
    assert.match(sql, /LEFT JOIN trend_agg t ON t.oem = c.oem\s+AND \(c.total_issues > 0 OR c.cmf_pending > 0\)/);
    assert.match(sql, /trend_base AS \([\s\S]*?cmf_request, ''\)\)\)\) = 'cmf_ok'/);
    for (const [index, field] of ['DuplicateTotalIssues', 'DuplicateOpenIssues', 'DuplicateImplemented', 'DuplicateClosed'].entries()) {
        assert.ok(server.includes('public int ' + field + ' { get; set; }'));
        assert.ok(sql.includes('row.' + field + ' = ReadInt(reader, ' + (index + 12) + ');'));
    }
});

test('OEM record-type checkboxes default to unique and support all four count and link selections', () => {
    const uniqueInput = page.match(/<input\b[^>]*id="homeOverallOemUnique"[^>]*>/);
    assert.ok(uniqueInput);
    assert.match(uniqueInput[0], /checked="checked"/);
    assert.match(uniqueInput[0], /aria-controls="homeOverallOemTable"/);
    const input = page.match(/<input\b[^>]*id="homeOverallOemDuplicates"[^>]*>/);
    assert.ok(input);
    assert.doesNotMatch(input[0], /\bchecked\b/);
    assert.match(input[0], /aria-controls="homeOverallOemTable"/);
    const host = { innerHTML: '', scrollTop: 137 };
    const uniqueCheckbox = { checked: true };
    const checkbox = { checked: false };
    const context = vm.createContext({
        document: { getElementById: id => id === 'homeOverallOemUnique' ? uniqueCheckbox : (id === 'homeOverallOemDuplicates' ? checkbox : host) },
        escapeHtml: value => String(value),
        renderOverallOemNameCell: value => String(value)
    });
    const definition = page.match(/        function renderOverallOemTable\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    const rows = [
        { OemName: 'Duplicate only', DuplicateTotalIssues: 3, DuplicateOpenIssues: 3 },
        { OemName: 'Mixed', TotalIssues: 3, Closed: 1, Implemented: 1, OpenIssues: 1, CmfPending: 2,
            AvgResolutionDays: 10, TrendCount: -1, RecentRequestCount: 1, PreviousRequestCount: 2,
            DuplicateTotalIssues: 4, DuplicateClosed: 2, DuplicateImplemented: 1, DuplicateOpenIssues: 1 }
    ];
    context.renderOverallOemTable(rows);
    const original = host.innerHTML;
    assert.doesNotMatch(original, /overall-oem-duplicate-note|Duplicate only/);
    assert.ok(original.includes('Mixed%7C%7Ctotal%7C%7Cunique'));
    checkbox.checked = true;
    checkbox.onchange();
    assert.ok(host.innerHTML.includes('Duplicate only'));
    assert.doesNotMatch(host.innerHTML, /overall-oem-duplicate-note/);
    assert.ok(host.innerHTML.indexOf('Duplicate only') < host.innerHTML.indexOf('Mixed'));
    const metricValues = html => Array.from(html.matchAll(/<(?:a|span)[^>]*class="overall-oem-metric-link [^"]*"[^>]*>(\d+)<\/(?:a|span)>/g), match => Number(match[1]));
    const total = host.innerHTML.match(/<tr class="overall-oem-total-row">([\s\S]*?)<\/tr>/)[1];
    assert.deepEqual(metricValues(total), [10, 3, 2, 5, 2]);
    const mixed = host.innerHTML.match(/<tr><td>[^]*?Mixed[^]*?<\/tr>/)[0];
    assert.deepEqual(metricValues(mixed).slice(-5), [7, 3, 2, 2, 2]);
    const duplicateOnly = host.innerHTML.match(/<tr><td>[^]*?Duplicate only[^]*?<\/tr>/)[0];
    assert.deepEqual(metricValues(duplicateOnly), [3, 0, 0, 3, 0]);
    assert.ok(duplicateOnly.includes('class="overall-oem-metric-link overall-oem-metric-open" onclick='));
    assert.ok(host.innerHTML.includes('Mixed%7C%7Ctotal%7C%7Call'));
    assert.ok(total.includes('__all_oems__%7C%7Copen%7C%7Call'));
    assert.ok(total.includes('<td>10.0</td>'));
    assert.ok(total.includes('last 7 days (including today): 1; previous 7 days: 2; change: -1.'));
    assert.equal(host.scrollTop, 137);
    assert.equal(rows[0].OemName, 'Duplicate only', 'Rendering must not reorder snapshot rows.');
    uniqueCheckbox.checked = false;
    uniqueCheckbox.onchange();
    const duplicateTotal = host.innerHTML.match(/<tr class="overall-oem-total-row">([\s\S]*?)<\/tr>/)[1];
    assert.deepEqual(metricValues(duplicateTotal), [7, 2, 1, 4, 2]);
    assert.ok(host.innerHTML.includes('Mixed%7C%7Ctotal%7C%7Cduplicates'));
    assert.ok(duplicateTotal.includes('__all_oems__%7C%7Copen%7C%7Cduplicates'));
    assert.ok(duplicateTotal.includes('<td>10.0</td>'));
    assert.equal(host.scrollTop, 137);
    checkbox.checked = false;
    checkbox.onchange();
    const emptyTotal = host.innerHTML.match(/<tr class="overall-oem-total-row">([\s\S]*?)<\/tr>/)[1];
    assert.deepEqual(metricValues(emptyTotal), [0, 0, 0, 0, 2]);
    assert.doesNotMatch(host.innerHTML, /class="overall-oem-name-link"|<a[^>]*class="overall-oem-metric-link overall-oem-metric-(?:total|closed|implemented|open)"/);
    uniqueCheckbox.checked = true;
    uniqueCheckbox.onchange();
    assert.equal(host.innerHTML, original);
    assert.equal(host.scrollTop, 137);
    checkbox.checked = true;
    context.renderOverallOemTable([{ OemName: 'Refreshed', TotalIssues: 1, DuplicateTotalIssues: 7 }]);
    checkbox.onchange();
    assert.deepEqual(metricValues(host.innerHTML), [8, 0, 0, 0, 0, 8, 0, 0, 0, 0]);
    assert.doesNotMatch(host.innerHTML, /Mixed/);
    context.renderOverallOemTable([{ OemName: 'No duplicates', TotalIssues: 1 }]);
    assert.doesNotMatch(host.innerHTML, /overall-oem-duplicate-note/);
    context.renderOverallOemTable([]);
    assert.ok(host.innerHTML.includes('No OEM/customer summary data available.'));
});

test('OEM drilldowns and exports honor selected record types while retaining legacy combined tokens', () => {
    const drilldown = server.match(/    private DataTable GetOemSummaryModalData\([^)]*\)[\s\S]*?\r?\n    \}/);
    assert.ok(drilldown);
    assert.match(drilldown[0], /recordTypeParts\.Length >= 3 && string\.Equals\(recordTypeParts\[2\], "unique", StringComparison\.OrdinalIgnoreCase\)/);
    assert.match(drilldown[0], /recordTypeParts\.Length >= 3 && string\.Equals\(recordTypeParts\[2\], "duplicates", StringComparison\.OrdinalIgnoreCase\)/);
    assert.match(drilldown[0], /recordTypeParts\.Length >= 3 && string\.Equals\(recordTypeParts\[2\], "none", StringComparison\.OrdinalIgnoreCase\)/);
    assert.match(drilldown[0], /whereClause\.Append\(uniqueOnly\s+\? "AND [^\n]*= 'cmf_ok' "\s+: duplicatesOnly\s+\? "AND [^\n]*= 'cmf_duplicate' "/);
    assert.match(drilldown[0], /: noRecordTypes\s+\? "AND 1 = 0 "\s+: "AND [^\n]*IN \('cmf_ok', 'cmf_duplicate'\) "/);
    assert.match(server, /return GetOemSummaryModalData\(oem\);/);
});

test('OEM record-type controls align with the table edge inside its scrollbar gutter', () => {
    const controls = page.match(/\.overall-oem-record-type-toggles \{([^}]*)\}/);
    assert.ok(controls);
    assert.match(controls[1], /margin-left: auto;/);
    assert.match(controls[1], /margin-right: 8px;/);
    assert.match(page, /\.overall-oem-table-wrap \{[^}]*scrollbar-gutter: stable;/);
});

test('OEM table matches Design Summary cell borders with stable sticky-header geometry', () => {
    const table = page.match(/\.overall-oem-table \{([^}]*)\}/);
    assert.ok(table);
    assert.match(table[1], /border-collapse: separate;/);
    assert.match(table[1], /border-spacing: 0;/);
    assert.match(table[1], /border-radius: 0;/);
    const header = page.match(/\.overall-oem-table th \{([^}]*)\}/);
    assert.ok(header);
    assert.match(header[1], /position: sticky;/);
    assert.match(header[1], /top: 0;/);
    assert.match(header[1], /border: 1px solid #d9dde5;/);
    assert.match(header[1], /border-left: 0;/);
    assert.match(header[1], /background-clip: padding-box;/);
    const cell = page.match(/\.overall-oem-table td \{([^}]*)\}/);
    assert.ok(cell);
    assert.match(cell[1], /border: 1px solid #d9dde5;/);
    assert.match(cell[1], /border-top: 0;/);
    assert.match(cell[1], /border-left: 0;/);
    assert.match(page, /\.overall-oem-table th,\s*\.overall-oem-table td \{\s*border-left: 0 !important;/);
    assert.match(page, /\.overall-oem-table-wrap \{[^}]*scrollbar-gutter: stable;/);
    const total = page.match(/\.overall-oem-table tbody tr\.overall-oem-total-row td \{([^}]*)\}/);
    assert.ok(total);
    assert.doesNotMatch(total[1], /border-/);
    assert.match(total[1], /background: #f8fafc;/);
});

test('OEM zero counts retain metric styling without links or hover underlines', () => {
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
        { OemName: 'Empty', TotalIssues: 0, Closed: 0, Implemented: 0, OpenIssues: 0, CmfPending: 0 },
        { OemName: 'Mixed', TotalIssues: 2, Closed: 1, Implemented: 0, OpenIssues: 1, CmfPending: 0 }
    ]);
    assert.equal((host.innerHTML.match(/<span class="overall-oem-metric-link [^"]*">0<\/span>/g) || []).length, 9);
    const links = Array.from(host.innerHTML.matchAll(/<a[^>]*class="overall-oem-metric-link [^"]*"[^>]*>([^<]*)<\/a>/g));
    assert.equal(links.length, 6);
    assert.ok(links.every(link => Number(link[1]) > 0 && link[0].includes('showDetailsModal3')));
    assert.match(page, /a\.overall-oem-metric-link:hover\s*\{[^}]*text-decoration: underline;/);
    assert.match(page, /span\.overall-oem-metric-link\s*\{\s*cursor: default;/);
    assert.doesNotMatch(page, /(?:^|\n)\.overall-oem-metric-link:hover\s*\{/);
});

test('OEM rolling-week trends omit minus signs on decreases and preserve signed tooltips', () => {
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
    assert.deepEqual(trends.map(trend => trend[3]), ['+5', '3', '0', '+2', '+4']);
    assert.deepEqual(trends.map(trend => trend[1]), ['overall-trend-up', 'overall-trend-down', 'overall-trend-flat', 'overall-trend-up', 'overall-trend-up']);
    assert.ok(trends[0][2].includes('last 7 days (including today): 15; previous 7 days: 10; change: +5'));
    assert.ok(trends[1][2].includes('last 7 days (including today): 7; previous 7 days: 10; change: -3'));
    assert.ok(trends[3][2].includes('previous 7 days: 0; change: +2'));
    assert.ok(trends[4][2].includes('last 7 days (including today): 26; previous 7 days: 22; change: +4'));
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

test('OEM total trend compares summed rolling windows for increases, decreases and flat totals', () => {
    const host = { innerHTML: '' };
    const context = vm.createContext({
        document: { getElementById: () => host },
        escapeHtml: value => String(value),
        renderOverallOemNameCell: value => String(value)
    });
    const definition = page.match(/        function renderOverallOemTable\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    for (const scenario of [
        { counts: [[10, 2], [1, 8]], recent: 11, previous: 10, value: '+1', tone: 'up', signed: '+1' },
        { counts: [[2, 7], [3, 4]], recent: 5, previous: 11, value: '6', tone: 'down', signed: '-6' },
        { counts: [[2, 4], [4, 2]], recent: 6, previous: 6, value: '0', tone: 'flat', signed: '0' },
        { counts: [[5, 0], [2, 0]], recent: 7, previous: 0, value: '+7', tone: 'up', signed: '+7' },
        { counts: [[0, 0]], recent: 0, previous: 0, value: '0', tone: 'flat', signed: '0' }
    ]) {
        context.renderOverallOemTable(scenario.counts.map(([recent, previous], index) => ({
            OemName: 'OEM ' + index, RecentRequestCount: recent, PreviousRequestCount: previous,
            TrendCount: recent - previous
        })));
        const total = host.innerHTML.match(/<tr class="overall-oem-total-row">([\s\S]*?)<\/tr>/);
        assert.ok(total);
        assert.ok(total[1].includes('overall-trend-' + scenario.tone));
        assert.ok(total[1].includes('<span class="overall-oem-trend-value">' + scenario.value + '</span>'));
        assert.ok(total[1].includes('last 7 days (including today): ' + scenario.recent
            + '; previous 7 days: ' + scenario.previous + '; change: ' + scenario.signed + '.'));
        assert.doesNotMatch(total[1], /overall-oem-total-trend">-|%/);
    }
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

test('header shows the existing total visits label below the updated timestamp', () => {
    const header = page.match(/<header class="portal-app-header">[\s\S]*?<\/header>/);
    assert.ok(header);
    assert.match(header[0], /<div class="portal-header-metadata">\s*<div class="portal-header-updated">Updated <asp:Label ID="lblHomeDashboardGeneratedAt"[^>]*\/><\/div>\s*<asp:Label ID="session_count"[^>]*CssClass="portal-header-visits"[^>]*\/>\s*<\/div>/);
    assert.equal((page.match(/ID="session_count"/g) || []).length, 1);
    assert.doesNotMatch(header[0].match(/<div id="updateinfo"[\s\S]*?<\/div>\s*<\/header>/)[0], /session_count/);
    assert.match(theme, /body \.portal-app-header \.portal-header-metadata \{\s*display: flex;\s*flex-direction: column;\s*align-items: flex-start;/);
    assert.match(theme, /body \.portal-app-header \.portal-header-visits \{[^}]*font-size: 12px;[^}]*font-variant-numeric: tabular-nums;/);
    assert.match(page, /cmf-portal-theme\.css\?v=20261120-header-visits-v8/);
    assert.match(server, /string session_count_table = "CMFUserVisitCounts_Final";/);
    assert.match(server, /SET SessionCount = SessionCount \+ 1 WHERE Username = @username/);
    assert.match(server, /SELECT ISNULL\(SUM\(SessionCount\), 0\) FROM/);
});

test('left menu uses native sticky positioning without scroll-frame corrections', () => {
    const mainScript = fs.readFileSync(path.join(root, 'Scripts', 'cmf-portal-main.js'), 'utf8');
    assert.match(theme, /body #mainMenuSidebar \{\s*position: sticky !important;/);
    assert.match(theme, /body:not\(\.modal-open\) \{\s*overflow: visible !important;/);
    assert.doesNotMatch(mainScript, /function (?:sync|clear)SidebarViewportStickiness|sidebar\.style\.setProperty\('(?:top|position)'/);
});

test('AI Insights hides pagination for empty views while retaining the empty-state message', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const context = vm.createContext({});
    for (const name of ['escape', 'findings', 'pagination']) {
        const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    for (const view of ['highlights', 'trends', 'similar']) {
        const state = { data: { PageSize: 5 }, pages: { [view]: { page: 0, items: [], total: 0 } } };
        const message = 'No material concentration or directional change in the data.';
        assert.equal(context.pagination(state, view), '');
        assert.equal(context.pagination(state, view) + context.findings([], message), '<p class="ci-empty">' + message + '</p>');
        state.pages[view].total = 1;
        assert.match(context.pagination(state, view), /<span>1-1 of 1 /);
        assert.doesNotMatch(context.pagination(state, view), /data-page-step/);
        state.pages[view].total = 6;
        assert.match(context.pagination(state, view), /<span>1-5 of 6 /);
        assert.match(context.pagination(state, view), /data-page-step="1"/);
        state.pages[view].page = 1;
        assert.match(context.pagination(state, view), /<span>6-6 of 6 /);
    }
});

test('AI Insights Summary retains escaping while the active panel uses reviewed snapshot briefings', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const context = vm.createContext({});
    for (const name of ['escape', 'summaryContent']) {
        const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    assert.match(context.summaryContent(null), /Generating AI summary/);
    assert.match(context.summaryContent({ Mode: 'empty', Claims: [] }), /No material findings/);
    for (const mode of ['canonical', 'ai-loading', 'ai-review', 'ai-unavailable', 'unknown']) {
        const hidden = context.summaryContent({ Success: true, Mode: mode, Claims: [{ Summary: 'CANONICAL SENTINEL' }] });
        assert.doesNotMatch(hidden, /CANONICAL SENTINEL/);
    }
    const result = context.summaryContent({ Success: true, Mode: 'ai', Claims: [{ FactIds: ['A:pending'], Summary: 'Platform <A>: <script>\nPlatform <A>: Pending is higher.',
        ReviewSuggestion: 'Review <contacts>.', Evidence: [{ Source: 'Platform <A>', Text: '10 versus 5 & scope.' }] }] });
    assert.match(result, /&lt;script&gt;/);
    assert.doesNotMatch(result, /<script>|<contacts>/);
    assert.match(result, /Suggested review/);
    assert.match(result, /Supporting evidence/);
    assert.match(result, /10 versus 5 &amp; scope/);
    assert.equal((result.match(/<h4 class="ci-category">Platform &lt;A&gt;<\/h4>/g) || []).length, 1);
    assert.doesNotMatch(result, /Platform &lt;A&gt;:/);

    const analysis = { Success: true, Mode: 'ai', Claims: [
        { Summary: 'NVL H Dashboard: First observation.\nNVL H Dashboard: Second observation.',
            ReviewSuggestion: 'Review contacts.', Evidence: [{ Source: 'NVL H Dashboard', Text: 'First evidence.' }] },
        { Summary: 'NVL Hx Dashboard: Different platform.\nNVL H Dashboard: Third observation.',
            ReviewSuggestion: 'Review the cited scopes.', Evidence: [
                { Source: 'NVL Hx Dashboard', Text: 'Other platform evidence.' },
                { Source: 'NVL H Dashboard', Text: 'Third evidence.' }] },
        { Summary: 'NVL H Dashboard: Fourth observation.', ReviewSuggestion: 'Review contacts.',
            Evidence: [{ Source: 'NVL H Dashboard', Text: 'Fourth evidence.' }] }
    ] };
    const original = JSON.stringify(analysis);
    const grouped = context.summaryContent(analysis);
    assert.equal((grouped.match(/<h4 class="ci-category">NVL H Dashboard<\/h4>/g) || []).length, 1);
    assert.equal((grouped.match(/<h4 class="ci-category">NVL Hx Dashboard<\/h4>/g) || []).length, 1);
    assert.equal((grouped.match(/<li class="cmf-assistant-text-section/g) || []).length, 2);
    assert.equal((grouped.match(/<details class="ci-support">/g) || []).length, 2);
    assert.equal((grouped.match(/Review contacts\./g) || []).length, 1);
    assert.doesNotMatch(grouped, /NVL H(?:x)? Dashboard:/);
    for (const text of ['First observation.', 'Second observation.', 'Third observation.', 'Fourth observation.',
        'Different platform.', 'First evidence.', 'Third evidence.', 'Fourth evidence.', 'Other platform evidence.'])
        assert.ok(grouped.includes(text), text);
    assert.match(grouped, /Suggested review \(NVL Hx Dashboard, NVL H Dashboard\)/);
    assert.equal(JSON.stringify(analysis), original);
    const fluent = context.summaryContent({ Success: true, Mode: 'ai', Claims: [
        { Scope: 'Platform <A>', FactIds: ['A:pending'],
            Summary: 'There are 10 pending CMFs, compared with an estimated 5 a week earlier.',
            ReviewSuggestion: 'Consider reviewing <contacts>.', Evidence: [{ Source: 'Platform <A>', Text: '10 versus 5.' }] },
        { Scope: 'Platform <A>', FactIds: ['A:age'], Summary: 'Several requests are beyond the cited age threshold.',
            Evidence: [{ Source: 'Platform <A>', Text: 'Age evidence.' }] }
    ] });
    assert.equal((fluent.match(/<h4 class="ci-category">Platform &lt;A&gt;<\/h4>/g) || []).length, 1);
    assert.equal((fluent.match(/<li class="cmf-assistant-text-section/g) || []).length, 1);
    assert.match(fluent, /<p class="ci-evidence">There are 10 pending CMFs, compared with an estimated 5 a week earlier\.<\/p>/);
    assert.match(fluent, /Several requests are beyond the cited age threshold/);
    assert.match(fluent, /&lt;contacts&gt;/);
    assert.doesNotMatch(fluent, /Platform &lt;A&gt;:|<contacts>/);
    const render = script.match(/    function render\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    assert.match(render, /loadPlatform\(state, false\)/);
    assert.doesNotMatch(render, /role="tab"|loadSummary\(|load\(state/);
    assert.match(script, /GetDashboardSummary/);
    assert.match(script, /serial !== state.serial \|\| evidenceKey !== state.evidenceKey/);
    assert.doesNotMatch(script, /Mode: 'canonical'|item.Title \+ '\. ' \+ item.Evidence/);
});

test('AI Insights Summary never substitutes canonical claims during loading, failure or stale responses', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function loadSummary\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    const state = { overall: true, serial: 1, evidenceKey: 'first', host: { isConnected: true },
        data: { Mode: 'ai', Highlights: [{ Title: 'Canonical title', Evidence: 'Canonical summary', Reasons: [{ Source: 'A', Text: '10 versus 5' }] }] } };
    const requests = [], timers = new Map();
    let timerId = 0, pending;
    const context = vm.createContext({ active: state, AbortController,
        setTimeout(callback, delay) { timers.set(++timerId, { callback, delay }); return timerId; },
        clearTimeout(timer) { timers.delete(timer); }, draw() {}, status() {},
        fetch(url, options) { requests.push({ url, body: JSON.parse(options.body) }); return new Promise(resolve => { pending = resolve; }); }
    });
    vm.runInContext(definition, context);
    context.loadSummary(state, true);
    assert.equal(state.analysis.Mode, 'ai-loading');
    assert.equal(state.analysis.Claims.length, 0);
    assert.equal(requests[0].body.evidenceKey, 'first');
    assert.equal(requests[0].body.refresh, true);
    pending({ ok: true, json: async () => ({ d: { Success: true, Mode: 'ai-loading', Claims: [{ Summary: 'CANONICAL SENTINEL' }] } }) });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.analysis.Mode, 'ai-loading');
    assert.equal(state.analysis.Claims.length, 0);
    const poll = Array.from(timers.values()).find(timer => timer.delay === 3000);
    assert.ok(poll);
    poll.callback();
    assert.equal(requests[1].body.refresh, false);
    pending({ ok: false });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.analysis.Mode, 'ai-unavailable');
    assert.equal(state.analysis.Claims.length, 0);
    context.loadSummary(state, false);
    pending({ ok: true, json: async () => ({ d: { Success: true, Mode: 'ai-review', Claims: [{ Summary: 'UNVERIFIED DRAFT' }] } }) });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.analysis.Mode, 'ai-unavailable');
    assert.equal(state.analysis.Claims.length, 0);
    context.loadSummary(state, false);
    state.serial++;
    state.evidenceKey = 'second';
    pending({ ok: true, json: async () => ({ d: { Success: true, Mode: 'ai', Claims: [{ Summary: 'STALE RESPONSE' }] } }) });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.analysis.Claims.length, 0);
    const platform = { overall: false };
    context.loadSummary(platform, false);
    assert.equal(requests.length, 4);
});

test('AI Insights Overall finding labels use compact teal badges with softly rounded rectangular corners', () => {
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    const badge = styles.match(/#homeOverallInsights \.ci-findings \.ci-finding-label\s*\{([^}]+)\}/);
    assert.ok(badge, 'Badge styling must be scoped to Overall finding labels');
    for (const property of ['display: inline-block;', 'max-width: 100%;', 'padding: 2px 8px;',
        'border-radius: 4px;', 'background: #e0f3ed;', 'color: #0f6654;', 'font-size: 10px;',
        'font-weight: 800;', 'line-height: 1.2;', 'letter-spacing: 0;']) assert.ok(badge[1].includes(property), property);
    assert.doesNotMatch(badge[1], /border-radius: 999px|#f3e8ff|#7e22ce/);
    assert.match(page, /Content\/cmf-dashboard-insights\.css\?v=39/);
});

test('all Overall sections use 9px padding and matching border corners and shadow', () => {
    const selected = page.match(/#homeOverallLayoutV2 > \.overall-ai-span-card,\s*#homeOverallLayoutV2 > \.overall-row-second,\s*#homeOverallLayoutV2 > \.overall-row-summary > \.overall-summary-card,\s*#homeOverallLayoutV2 > \.overall-summary-side-card\.overall-summary-side-span,\s*#homeOverallLayoutV2 > \.overall-row-third \{([^}]*)\}/);
    const reference = page.match(/^\.overall-row \{([^}]*)\}/m);
    assert.ok(selected);
    assert.ok(reference);
    assert.match(selected[1], /(?:^|\n)\s*padding: 9px !important;/);
    for (const property of ['border', 'border-radius', 'box-shadow']) {
        const declaration = new RegExp('(?:^|\\n)\\s*' + property + ':\\s*([^;]+);');
        const actual = selected[1].match(declaration);
        const expected = reference[1].match(declaration);
        assert.ok(actual);
        assert.ok(expected);
        assert.match(actual[1], /!important$/);
        assert.equal(actual[1].replace(/\s*!important$/, ''), expected[1], property + ' must match the bottom section');
    }
});

test('equivalent platform containers match the Overall section padding border corners and shadow', () => {
    const platform = page.match(/#homeLegacyTopLayout #homeLegacySummarySection \.overall-summary-side-card,\s*#homeLegacyTopLayout #homeLegacySummarySection \.overall-summary-card,\s*#homeLegacyTopLayout #homeLegacyOverview \.ccip-dashboard-left,\s*#homeLegacyTopLayout #homeLegacyAiHealthCard,\s*#homeLegacyTablesGrid \.ccip-summary-canvas \{([^}]*)\}/);
    const overall = page.match(/#homeOverallLayoutV2 > \.overall-row-third \{([^}]*)\}/);
    assert.ok(platform);
    assert.ok(overall);
    for (const property of ['padding', 'border', 'border-radius', 'box-shadow']) {
        const declaration = new RegExp('(?:^|\\n)\\s*' + property + ':\\s*([^;]+);');
        const actual = platform[1].match(declaration);
        const expected = overall[1].match(declaration);
        assert.ok(actual);
        assert.ok(expected);
        assert.equal(actual[1], expected[1], property + ' must match Overall');
    }
});

test('platform bottom section matches the Overall section outer styling', () => {
    const platform = page.match(/#homeLegacyTablesGrid \.ccip-summary-canvas \{([^}]*)\}/);
    const overall = page.match(/^\.overall-row \{([^}]*)\}/m);
    assert.ok(platform);
    assert.ok(overall);
    for (const property of ['border', 'border-radius', 'background', 'box-shadow', 'padding']) {
        const declaration = new RegExp('(?:^|\\n)\\s*' + property + ':\\s*([^;]+);');
        const platformValue = platform[1].match(declaration);
        const overallValue = overall[1].match(declaration);
        assert.ok(platformValue, 'Platform must define ' + property);
        assert.ok(overallValue, 'Overall must define ' + property);
        assert.equal(platformValue[1], overallValue[1], property + ' must match Overall');
    }
});

test('platform Q.Links fills the full two-row grid without changing its width', () => {
    const grid = page.match(/\.ccip-specific-top-layout \{([^}]*)\}/);
    assert.ok(grid);
    assert.match(grid[1], /position: relative;/);
    const summary = page.match(/#homeLegacyTopLayout #homeLegacySummarySection \{([^}]*)\}/);
    assert.ok(summary);
    assert.match(summary[1], /position: static;/);
    const card = page.match(/#homeLegacyTopLayout #homeLegacySummarySection \.overall-summary-side-card \{([^}]*)\}/);
    assert.ok(card);
    assert.match(card[1], /grid-column: 1;/);
    assert.match(card[1], /grid-row: 1 \/ span 2;/);
    assert.match(card[1], /height: 100%;/);
    assert.match(card[1], /min-height: 0;/);
    assert.match(card[1], /max-height: none;/);
    assert.match(card[1], /width: var\(--cmf-summary-side-card-width\) !important;/);
    assert.match(card[1], /position: absolute;/);
    assert.match(card[1], /left: 0;/);
    assert.match(card[1], /top: 0;/);
});

test('AI Insights keeps content-independent card dimensions and internal scrolling', () => {
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    assert.match(page, /#homeLegacyAiHealthCard \{\s*--ccip-ai-left-extend: 0px;\s*--ccip-ai-fixed-height: 100%;/);
    assert.match(page, /@media \(max-width: 900px\) \{\s*#homeLegacyAiHealthCard \{\s*--ccip-ai-fixed-height: 503\.85px;/);
    assert.match(styles, /#homeOverallLayoutV2 > \.overall-ai-span-card,\s*#homeLegacyAiHealthCard\s*\{\s*position: relative;\s*contain: size;\s*overflow: hidden;/);
    assert.match(styles, /#homeLegacyAiHealthCard\s*\{\s*min-height: var\(--ccip-ai-fixed-height\) !important;\s*max-height: var\(--ccip-ai-fixed-height\) !important;/);
    assert.match(styles, /\.cmf-insights\s*\{[^}]*min-width: 0;[^}]*min-height: 0;[^}]*overflow: auto;/);
    assert.match(styles, /\.ci-toolbar\s*\{[^}]*flex: 0 0 auto;/);
    assert.match(styles, /\.ci-content\s*\{\s*flex: 1 1 0;\s*min-width: 0;\s*min-height: 0;\s*max-height: none;\s*overflow: auto;/);
    assert.doesNotMatch(styles, /\.ci-content\s*\{[^}]*max-height:\s*\d/);
    assert.match(styles, /\.ci-findings\s*\{[^}]*padding: 0 2px 6px 0;/);
    assert.match(styles, /#homeOverallInsights \.ci-content > \.ci-findings\s*\{\s*padding-top: 6px;\s*\}/);
    assert.match(styles, /@media \(max-width: 900px\)\s*\{\s*#homeOverallLayoutV2 > \.overall-ai-span-card\s*\{\s*height: 503\.85px;/);
    assert.match(styles, /#homeWelcomePanel \.ccip-dashboard\s*\{\s*container-type: inline-size;/);
    assert.match(styles, /#homeLegacyTopLayout > #homeLegacyAiHealthCard\s*\{\s*max-width: min\(100cqw, calc\(100vw - 40px\)\);/);
});

test('platform AI summary scope wraps current labels in parentheses', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    new vm.Script(script);
    const elements = new Map();
    for (const id of ['homePlatformInsights', 'homeOverallInsights']) {
        elements.set(id, { id, innerHTML: '', querySelector: () => ({ addEventListener() {} }) });
        elements.set(id + 'Scope', { textContent: '' });
    }
    const portal = { currentDashboardPlatform: 'CMF_NVL_H_ALL_COMPONENTS_TABLE' };
    const context = vm.createContext({
        active: null, window: { CMF_PORTAL: portal },
        document: { getElementById: id => elements.get(id) }, loadPlatform() {}, clearTimeout() {}
    });
    const definition = script.match(/    function render\([^)]*\) \{[\s\S]*?\r?\n    \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    for (const [label, expected] of [
        ['NVL H', '(NVL H)'], ['NVL H Dashboard', '(NVL H)'],
        [' NVL H dAsHbOaRd ', '(NVL H)'], ['PTL', '(PTL)'], ['', ''], ['   ', ''], [undefined, '']
    ]) {
        context.render({ InsightsSnapshotKey: String(label), PlatformLabel: label });
        assert.equal(elements.get('homePlatformInsightsScope').textContent, expected);
    }
    portal.currentDashboardPlatform = 'CMF_OVERALL_ALL_COMPONENTS_TABLE';
    context.render({ InsightsSnapshotKey: 'overall', PlatformLabel: 'Overall (12 included platforms)' });
    assert.equal(elements.get('homeOverallInsightsScope').textContent, 'Overall (12 included platforms)');
});

test('AI Insights scope sits beside each heading at 13px without the acceptance-date label', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    new vm.Script(script);
    for (const hostId of ['homeOverallInsights', 'homePlatformInsights']) {
        const heading = hostId === 'homePlatformInsights' ? 'AI SUMMARY' : 'AI INSIGHTS';
        assert.match(page, new RegExp(heading + '</span><span id="' + hostId + 'Scope" class="ci-scope"></span>'));
    }
    assert.match(styles, /\.ci-heading\s*\{[^}]*align-items: baseline;[^}]*flex-wrap: wrap;/);
    assert.match(styles, /\.ci-scope\s*\{[^}]*font-size: 13px;/);
    assert.doesNotMatch(script, /data\.DateRange|<span class="ci-scope">/);
    const states = [];
    const elements = new Map();
    for (const hostId of ['homeOverallInsights', 'homePlatformInsights']) {
        const parts = new Map(['.ci-content', '.ci-metadata', '.ci-time', '.ci-refresh'].map(selector => [selector, {
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
        loadPlatform: state => states.push(state),
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
        const scopeLabel = hostId === 'homePlatformInsights' ? '(' + label + ')' : label;
        assert.equal(elements.get(hostId + 'Scope').textContent, scopeLabel);
        assert.doesNotMatch(state.host.innerHTML, /ci-scope|All acceptance dates/);
        assert.match(state.host.innerHTML, /ci-platform-title/);
        assert.match(state.host.innerHTML, new RegExp('aria-label="Ranked ' + (hostId === 'homeOverallInsights' ? 'Overall' : 'platform') + ' findings"'));
        assert.doesNotMatch(state.host.innerHTML, /ci-mode|AI-ranked|Ranking/);
        assert.deepEqual(Array.from(state.host.innerHTML.matchAll(/data-view="([^"]+)"/g), match => match[1]), []);
        assert.doesNotMatch(state.host.innerHTML, /Similar Issues|data-view="similar"/);
        assert.doesNotMatch(state.host.innerHTML, /Scope &amp; sources|ci-provenance|<details>/);
        assert.match(state.host.innerHTML, /<div class="ci-footer"><time class="ci-time"><\/time><p class="ci-metadata"><\/p><\/div>/);
        state.pages = { highlights: { items: [] } };
        state.data = { Scope: label, DateRange: 'All acceptance dates', Mode: 'ai', Records: 0, DistinctIssues: 0, DuplicateRecords: 0, ExcludedRecords: 0, GeneratedAt: '2026-11-04T12:00:00Z', ExpiresAt: '2099-01-01T00:00:00Z', SourceStatus: 'All returned text AI-reviewed' };
        context.draw(state);
        assert.equal(elements.get(hostId + 'Scope').textContent, scopeLabel);
    }
    for (const label of ['NVL S Dashboard', 'NVL S', 'NVL S dAsHbOaRd ']) {
        context.render({ InsightsSnapshotKey: label, PlatformLabel: label });
        assert.equal(elements.get('homePlatformInsightsScope').textContent, '(NVL S)');
    }
});

test('AI Insights progressively renders reviewed groups with blue loading progress and retains completed findings', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    function harness(replies) {
        const parts = new Map(['.ci-content', '.ci-time', '.ci-metadata', '.ci-status', '.ci-refresh', '.ci-progress'].map(selector => [selector, {
            textContent: '', innerHTML: 'old calculated content', attributes: {},
            setAttribute(name, value) { this.attributes[name] = value; },
            removeAttribute(name) { delete this.attributes[name]; },
            classList: { toggle() {} }, style: { setProperty(name, value) { this[name] = value; } }
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
                return { ok: true, async json() { return { d: JSON.parse(JSON.stringify(reply)) }; } };
            },
            draw(target) {
                received.push(target.data);
                parts.get('.ci-content').innerHTML = target.data && (target.data.Highlights || []).length
                    ? 'AI result: ' + target.data.Highlights.map(item => item.Title).join(', ') : 'Reviewing issue groups...';
            }
        });
        for (const name of ['receive', 'request', 'status', 'load']) {
            const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
            assert.ok(definition);
            vm.runInContext(definition[0], context);
        }
        return { context, state, parts, requests, received, timers };
    }
    const source = { Success: true, Mode: 'ai-loading', EvidenceKey: 'opaque', Records: 12, DistinctIssues: 11, CandidateCount: 6, ReviewedCandidates: 0,
        GeneratedAt: '2026-11-08T12:00:00Z', HighlightCount: 0, GroupCount: 0, Highlights: [], Groups: [] };
    const partial = { ...source, GeneratedAt: '2026-11-08T12:00:01Z', ReviewedCandidates: 2, HighlightCount: 1,
        Message: 'AI reviewed 2/6 ranked groups.', Highlights: [{ AiReviewed: true, Title: 'Technical pattern' }] };
    const complete = { ...partial, Mode: 'ai', Message: 'AI review complete.', ReviewedCandidates: 6 };
    const running = harness([source, partial, { ...partial }, complete]);
    running.context.load(running.state, true);
    await new Promise(resolve => setImmediate(resolve));
    assert.match(running.parts.get('.ci-content').innerHTML, /Technical pattern/);
    assert.equal(running.parts.get('.ci-status').textContent, '');
    assert.match(running.parts.get('.ci-metadata').textContent, /6 ranked groups \| 2 completed/);
    assert.equal(running.parts.get('.ci-progress').hidden, false);
    assert.equal(running.parts.get('.ci-progress').attributes['aria-valuenow'], '2');
    assert.equal(running.parts.get('.ci-progress').style['--ci-progress'], (100 / 3) + '%');
    assert.equal(running.requests.length, 2, 'An empty pre-AI highlights list must not skip the AI pipeline');
    assert.deepEqual(running.requests[1].body, { evidenceKey: 'opaque', refresh: true });
    const poll = Array.from(running.timers.values()).find(timer => timer.delay === 3000);
    assert.ok(poll, 'Long analysis must poll without a total elapsed-time cutoff');
    poll.callback();
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(running.received.length, 3, 'Unchanged snapshots must not redraw or reset the current page');
    assert.equal(running.requests[2].body.refresh, false);
    Array.from(running.timers.values()).filter(timer => timer.delay === 3000).at(-1).callback();
    await new Promise(resolve => setImmediate(resolve));
    assert.match(running.parts.get('.ci-content').innerHTML, /Technical pattern/);
    assert.equal(running.parts.get('.ci-progress').hidden, true);
    assert.equal(running.parts.get('.ci-status').textContent, '');
    for (const failure of [new Error('Network unavailable.'), { Success: false, Mode: 'ai-unavailable', Message: 'AI analysis could not complete.' }]) {
        const unavailable = harness([source, failure]);
        unavailable.context.load(unavailable.state, false);
        await new Promise(resolve => setImmediate(resolve));
        assert.match(unavailable.parts.get('.ci-status').textContent, /unavailable|could not complete/i);
        assert.equal(unavailable.parts.get('.ci-progress').hidden, true);
        assert.doesNotMatch(unavailable.parts.get('.ci-content').innerHTML, /old calculated content|Technical pattern/);
        assert.equal(unavailable.parts.get('.ci-refresh').disabled, false);
    }
    for (const failure of [new Error('Network unavailable.'), { ...partial, Success: false, Mode: 'ai-unavailable', Message: 'Remaining review stopped.' },
        { ...partial, Mode: 'ai-unavailable', Message: 'Overall priorities use 2/6 platform rankings.' }]) {
        const interrupted = harness([source, partial, failure]);
        interrupted.context.load(interrupted.state, false);
        await new Promise(resolve => setImmediate(resolve));
        Array.from(interrupted.timers.values()).find(timer => timer.delay === 3000).callback();
        await new Promise(resolve => setImmediate(resolve));
        assert.match(interrupted.parts.get('.ci-content').innerHTML, /Technical pattern/);
        assert.match(interrupted.parts.get('.ci-status').textContent, /unavailable|could not complete/i);
        assert.doesNotMatch(interrupted.parts.get('.ci-status').textContent, /rankings/i);
        assert.equal(interrupted.parts.get('.ci-progress').hidden, true);
        assert.equal(interrupted.parts.get('.ci-refresh').disabled, false);
    }
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    assert.match(styles, /\.ci-progress span\s*\{[^}]*background: #1768b2;/);
    assert.match(styles, /#homeOverallLayoutV2 > \.overall-ai-span-card,\s*#homeLegacyAiHealthCard\s*\{\s*position: relative;/);
    assert.match(styles, /\.ci-progress\s*\{[^}]*position: absolute;[^}]*bottom: 0;[^}]*left: 0;[^}]*right: 0;/);
    assert.doesNotMatch(styles, /\.ci-progress\s*\{[^}]*(?:flex:|margin)/);
    assert.match(styles, /\.ci-progress-indeterminate span\s*\{[^}]*animation: ci-review-loading/);
    assert.match(styles, /prefers-reduced-motion: reduce/);
    assert.doesNotMatch(script, /Calculated|Showing calculated|calculated evidence is available/);
});

test('AI Insights resets ranking pages when the same dashboard facts finish ranking', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function receive\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    const context = vm.createContext({});
    vm.runInContext(definition, context);
    const pending = { Mode: 'ai-loading', GeneratedAt: '2026-11-09T12:00:00Z', SourceHash: 'same-facts',
        HighlightCount: 2, TrendCount: 2, GroupCount: 0, Highlights: [{ Id: 'unranked' }], TrendFindings: [{ Id: 'current' }] };
    const state = { pageVersion: 0 };
    assert.equal(context.receive(state, pending), true);
    assert.equal(context.receive(state, { ...pending }), false);
    assert.equal(context.receive(state, { ...pending, Mode: 'ai', Highlights: [{ Id: 'ranked-first' }], TrendFindings: [{ Id: 'ranked-trend' }] }), true);
    assert.equal(state.pages.highlights.items[0].Id, 'ranked-first');
    assert.equal(state.pages.trends.items[0].Id, 'ranked-trend');
});

test('AI Insights keeps its seven-day input independent of calendar-week chart controls', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function render\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    const loads = [];
    const host = { id: 'homePlatformInsights', querySelector: () => ({ addEventListener() {} }), querySelectorAll: () => [] };
    const context = vm.createContext({
        active: null,
        window: { CMF_PORTAL: { currentDashboardPlatform: 'NVL-H' } },
        document: { getElementById(id) {
            if (id === host.id) return host;
            if (id === host.id + 'Scope') return {};
            throw new Error('Insights must not read chart controls: ' + id);
        } },
        load: state => loads.push(state.key), clearTimeout() {},
        loadPlatform: state => loads.push(state.key)
    });
    vm.runInContext(definition, context);
    context.render({ InsightsSnapshotKey: 'snapshot-a' });
    context.render({ InsightsSnapshotKey: 'snapshot-a' });
    assert.equal(loads.length, 1);
    context.render({ InsightsSnapshotKey: 'snapshot-b' });
    assert.equal(loads.length, 2);
    assert.equal(context.active.key, 'NVL-H|snapshot-b');
    const service = fs.readFileSync(path.join(root, 'App_Code', 'CmfRenderedInsightsService.cs'), 'utf8');
    const projection = service.match(/    public static CmfPanelInsightSource BuildSource\([\s\S]*?\r?\n    \}/)[0];
    assert.doesNotMatch(projection, /Field\(snapshot\.Data, "Trend"\)|WeekEndOpenIssues|NewIssues|ResolvedIssues/);
    assert.doesNotMatch(service, /current week is partial|current week remains partial|Partial-week flow/);
    assert.match(service, /Period = "past-7-days"/);
    assert.match(script, /rendered-dashboard-v4/);
});

test('platform briefing separates Summary and Customer Issues panels with conditional Open and Pending boxes', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const context = vm.createContext({});
    for (const name of ['escape', 'platformFindings']) {
        const definition = script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'));
        assert.ok(definition);
        vm.runInContext(definition[0], context);
    }
    const sections = ['highlights', 'focus'];
    const findings = Array.from({ length: 19 }, (_, index) => ({ Id: 'f' + index, Title: 'Observation <' + index + '>',
        Summary: 'Evidence & analysis ' + index, Kind: index % 2 ? 'cluster' : 'change', Tone: index % 2 ? 'progress' : 'attention',
        Section: sections[index % sections.length],
        Evidence: [{ Source: 'Table <A>', Text: 'Why: this ingredient holds most open CMFs.\nField Open: Count 12 of 20. <script>untrusted</script>' }] }));
    const html = context.platformFindings({ Mode: 'ai', Findings: findings });
    assert.equal((html.match(/data-finding-id=/g) || []).length, 19);
    assert.equal((html.match(/<details class="ci-support"/g) || []).length, 19);
    assert.equal((html.match(/<button type="button" class="ci-source-link"/g) || []).length, 19);
    assert.equal((html.match(/<section class="cmf-assistant-text-section/g) || []).length, 2);
    assert.equal((html.match(/<ul class="ci-platform-bullets">/g) || []).length, 2);
    for (let index = 0; index < 17; index++) {
        assert.ok(html.indexOf('Observation &lt;' + index + '&gt;') < html.indexOf('Observation &lt;' + (index + 2) + '&gt;'), 'Within-section LLM order changed.');
    }
    assert.match(html, /<strong>Observation &lt;0&gt;:<\/strong>/);
    assert.match(html, /Highlights[\s\S]*Lowlights/);
    assert.doesNotMatch(html, /Changes Over Time|Concentrations &amp; Exposure|Assessment Queue|Lowlights \/ Focus Areas/);
    assert.match(html, /Evidence &amp; analysis 0 <button[^>]+aria-expanded="false"[^>]+aria-controls="platform-insight-source-f0"/);
    assert.match(html, /<i class="fas fa-link" aria-hidden="true"><\/i><\/button><\/p><details class="ci-support" id="platform-insight-source-f0"/);
    assert.equal((html.match(/title="Source and justification for Observation &lt;/g) || []).length, 19);
    assert.doesNotMatch(html, />\s*Source<\/button>/);
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    assert.match(styles, /:is\(#homePlatformInsights, #homeOverallInsights\) \.ci-platform-bullets \{[^}]*padding-left: 48px/);
    assert.match(styles, /:is\(#homePlatformInsights, #homeOverallInsights\) \.ci-source-summary \{ display: none; \}/);
    assert.match(styles, /:is\(#homePlatformInsights, #homeOverallInsights\) \.ci-support \{ border-top: 0; margin-top: 0; \}/);
    assert.match(styles, /:is\(#homePlatformInsights, #homeOverallInsights\) \.ci-support\[open\] \{ margin-top: 4px; \}/);
    assert.match(styles, /:is\(#homePlatformInsights, #homeOverallInsights\) \.ci-platform-bullets > li \{[^}]*margin: 0 0 6px/);
    assert.match(styles, /:is\(#homePlatformInsights, #homeOverallInsights\) \.ci-source-link \{[^}]*display: inline-block;[^}]*width: 16px; height: 12px; margin-left: 0;[^}]*vertical-align: baseline/);
    assert.match(html, /Evidence &amp; analysis/);
    assert.match(html, /Table &lt;A&gt;/);
    assert.equal((html.match(/<p class="ci-source-detail">/g) || []).length, 38);
    assert.match(html, /<p class="ci-source-detail">Why: this ingredient holds most open CMFs\.<\/p><p class="ci-source-detail">Field Open:/);
    assert.match(html, /&lt;script&gt;untrusted&lt;\/script&gt;/);
    assert.doesNotMatch(html, /<A>|<0>|<script>|ci-rank|ci-finding-label|ci-pagination|data-page-step|<li[^>]+class="cmf-assistant/);
    const customerFindings = ['Pending', 'Open', 'Pending', 'Open'].map((population, index) => ({
        Id: 'customer-' + index, Title: 'Customer pattern <' + index + '>', Summary: population + ' reported symptom & detail.',
        Section: index % 2 ? 'highlights' : 'focus',
        Evidence: [{ Source: 'CustomerIssues - ' + population + ' reported customer issue',
            Text: 'Issue population: ' + population + '\nReported issue title: <script>source title</script>' }]
    }));
    const operational = { ...findings[1], Id: 'pending-queue', Title: 'Pending assessment aging',
        Evidence: [{ Source: 'Pending Assessments - Pending queue coverage', Text: 'Oldest pending age: 17 days' }] };
    for (const pipeline of ['platform-page-briefing-v3', 'overall-page-briefing-v1']) {
        const enriched = context.platformFindings({ Mode: 'ai', Pipeline: pipeline,
            Findings: [...findings, ...customerFindings, operational] });
        const boxes = enriched.match(/<section class="cmf-assistant-text-section[\s\S]*?<\/section>/g);
        assert.equal(boxes.length, pipeline === 'overall-page-briefing-v1' ? 2 : 4);
        assert.match(boxes[0], /Highlights/);
        assert.match(boxes[1], /Lowlights[\s\S]*Pending assessment aging/);
        if (pipeline === 'overall-page-briefing-v1') {
            assert.doesNotMatch(enriched, /data-briefing-panel="customers"|Customer pattern|Reported issue title/);
            assert.equal((enriched.match(/data-finding-id=/g) || []).length, 20);
            assert.equal((enriched.match(/<button type="button" class="ci-source-link"/g) || []).length, 20);
            continue;
        }
        assert.match(boxes[2], /<h3><span>Open<\/span><\/h3>/);
        assert.match(boxes[3], /<h3><span>Pending<\/span><\/h3>/);
        assert.match(enriched, /data-briefing-panel="summary"[\s\S]*Highlights[\s\S]*Lowlights[\s\S]*data-briefing-panel="customers"/);
        assert.doesNotMatch(boxes[2] + boxes[3], /Pending assessment aging|Observation &lt;/);
        assert.doesNotMatch(boxes[0] + boxes[1], /Customer pattern/);
        assert.equal((enriched.match(/data-finding-id=/g) || []).length, 24);
        assert.equal((enriched.match(/<button type="button" class="ci-source-link"/g) || []).length, 24);
        for (const finding of customerFindings) {
            assert.equal((enriched.match(new RegExp('data-finding-id="' + finding.Id + '"', 'g')) || []).length, 1);
        }
        assert.ok(boxes[2].indexOf('customer-1') < boxes[2].indexOf('customer-3'), 'Open finding order changed.');
        assert.ok(boxes[3].indexOf('customer-0') < boxes[3].indexOf('customer-2'), 'Pending finding order changed.');
        assert.match(boxes[2], /&lt;script&gt;source title&lt;\/script&gt;/);
        assert.doesNotMatch(boxes[2], /<script>/);
    }
    const customerOnly = context.platformFindings({ Mode: 'ai', Findings: [customerFindings[1]] });
    assert.equal((customerOnly.match(/<section /g) || []).length, 1);
    assert.match(customerOnly, /data-briefing-panel="customers"[\s\S]*>Open<\/span>/);
    assert.doesNotMatch(customerOnly, /data-briefing-panel="summary"|Highlights|Lowlights|>Pending<\/span>/);
    const overallCustomerOnly = context.platformFindings({ Mode: 'ai', Pipeline: 'overall-page-briefing-v1', Findings: customerFindings });
    assert.match(overallCustomerOnly, /No significant patterns/);
    assert.doesNotMatch(overallCustomerOnly, /data-briefing-panel="customers"|Customer pattern|Reported issue title/);
    const pendingOnly = context.platformFindings({ Mode: 'ai', Findings: [customerFindings[0]] });
    assert.equal((pendingOnly.match(/<section /g) || []).length, 1);
    assert.match(pendingOnly, /data-briefing-panel="customers"[\s\S]*>Pending<\/span>/);
    assert.doesNotMatch(pendingOnly, /data-briefing-panel="summary"|>Open<\/span>/);
    const sparse = context.platformFindings({ Mode: 'ai', Findings: findings.filter(finding => finding.Section === 'highlights') });
    assert.equal((sparse.match(/<section /g) || []).length, 1);
    assert.doesNotMatch(sparse, /data-briefing-panel="customers"|Lowlights|Changes Over Time|Concentrations|Assessment Queue/);
    for (const section of [undefined, 'unknown', '<script>', 'changes', 'exposure', 'assessment']) {
        const invalid = context.platformFindings({ Mode: 'ai', Findings: [{ ...findings[0], Section: section }] });
        assert.match(invalid, /Insights aren't available right now\. Try again\./);
        assert.doesNotMatch(invalid, /Observation|<section|<script>/);
    }
    assert.match(context.platformFindings({ Mode: 'ai', Findings: [] }), /No significant patterns/);
    for (const mode of ['ai-loading', 'ai-review', 'ai-unavailable', 'unknown']) {
        assert.doesNotMatch(context.platformFindings({ Mode: mode, Findings: findings }), /Observation|Count 12/);
    }
});

test('platform briefing view tabs preserve scroll, support keyboard switching and omit empty views', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const panels = ['summary', 'customers'].map((view, index) => ({
        id: 'platform-briefing-' + view, dataset: { briefingPanel: view }, hidden: false,
        getAttribute: () => 'platform-briefing-tab-' + view,
        querySelectorAll: () => Array.from({ length: index ? 1 : 2 }, () => ({}))
    }));
    const buttons = ['summary', 'customers'].map(view => ({
        dataset: { briefingView: view }, attributes: {},
        setAttribute(name, value) { this.attributes[name] = value; },
        addEventListener(name, callback) { this[name] = callback; }, focus() { this.focused = true; }
    }));
    let availablePanels = panels, availableButtons = buttons;
    const parts = new Map(['.ci-content', '.ci-metadata', '.ci-time', '.ci-refresh', '.ci-progress', '.ci-status', '.ci-panel-tabs'].map(selector => [selector, {
        textContent: '', innerHTML: '', scrollTop: 0, attributes: {}, style: { setProperty() {} },
        setAttribute(name, value) { this.attributes[name] = value; },
        querySelectorAll: query => query === '[data-briefing-panel]' ? availablePanels
            : query === '[data-briefing-view]' ? availableButtons : []
    }]));
    const state = { host: { isConnected: true, querySelector: selector => parts.get(selector), querySelectorAll: () => availableButtons },
        data: { Mode: 'ai', Findings: [{}, {}, {}], GeneratedAt: '2026-11-23T03:00:00Z' } };
    const context = vm.createContext({ active: state, platformFindings: () => 'Approved panels', fetch() { throw new Error('View switches must not request AI.'); } });
    for (const name of ['platformPanelMarkup', 'updatePlatformStatus', 'selectPlatformView', 'drawPlatform'])
        vm.runInContext(script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'))[0], context);
    context.drawPlatform(state);
    assert.equal(state.briefingView, 'summary');
    assert.equal(panels[0].hidden, false);
    assert.equal(panels[1].hidden, true);
    assert.equal(parts.get('.ci-metadata').textContent, '2 findings');
    assert.match(parts.get('.ci-panel-tabs').innerHTML, /aria-controls="platform-briefing-customers"[\s\S]*Customer Issues/);
    const content = parts.get('.ci-content');
    content.scrollTop = 25;
    buttons[1].click();
    assert.equal(state.briefingView, 'customers');
    assert.equal(content.scrollTop, 0);
    assert.equal(panels[0].hidden, true);
    assert.equal(buttons[1].attributes['aria-selected'], 'true');
    assert.equal(buttons[1].tabIndex, 0);
    assert.equal(buttons[0].tabIndex, -1);
    assert.equal(parts.get('.ci-metadata').textContent, '1 finding');
    content.scrollTop = 12;
    let prevented = false;
    buttons[1].keydown({ key: 'ArrowLeft', preventDefault() { prevented = true; } });
    assert.equal(prevented, true);
    assert.equal(buttons[0].focused, true);
    assert.equal(content.scrollTop, 25);
    buttons[0].keydown({ key: 'End', preventDefault() {} });
    assert.equal(content.scrollTop, 12);
    buttons[1].keydown({ key: 'Home', preventDefault() {} });
    assert.equal(content.scrollTop, 25);
    context.drawPlatform(state);
    assert.equal(content.scrollTop, 25);
    availablePanels = [panels[1]];
    availableButtons = [buttons[1]];
    context.drawPlatform(state);
    assert.equal(state.briefingView, 'customers');
    assert.doesNotMatch(parts.get('.ci-panel-tabs').innerHTML, />Summary</);
    availablePanels = [];
    availableButtons = [];
    state.data = null;
    context.drawPlatform(state);
    assert.equal(parts.get('.ci-panel-tabs').hidden, true);
    assert.equal(parts.get('.ci-panel-tabs').innerHTML, '');
    assert.match(page, /cmf-dashboard-insights\.js\?v=43/);
    assert.match(page, /cmf-dashboard-insights\.css\?v=39/);
    const styles = fs.readFileSync(path.join(root, 'Content', 'cmf-dashboard-insights.css'), 'utf8');
    assert.match(styles, /\.ci-panel-tabs \{[^}]*gap: 0;[^}]*padding: 0 8px/);
    assert.match(styles, /\.ci-controls:has\(\.ci-panel-tabs\) \{[^}]*position: relative; isolation: isolate/);
    assert.match(styles, /\.ci-controls:has\(\.ci-panel-tabs\) \{[^}]*border-bottom-color: transparent/);
    assert.match(styles, /\.ci-controls:has\(\.ci-panel-tabs\)::after \{[^}]*left: 0; right: 0; bottom: -1\.625px; height: 8px;[^}]*d='M0 35 H120'[^}]*stroke='%23cad7e4' stroke-width='1\.25'[^}]*border-image-slice: 0 0 8 0;[^}]*border-image-width: 0 0 8px 0;[^}]*z-index: 1; pointer-events: none/);
    assert.match(styles, /\.ci-panel-tabs \{[^}]*isolation: auto/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\] \{[^}]*position: relative; z-index: 0/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]\[aria-selected="true"\] \{[^}]*z-index: 2/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\] \+ \[role="tab"\] \{ margin-left: -1px; \}/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]::before \{[^}]*inset: -1px; bottom: -1\.875px;[^}]*border-image-source:[^}]*stroke='%23cad7e4' stroke-width='1\.25'[^}]*border-image-slice: 12 16 8 16;[^}]*border-image-outset: 0 8px/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]::after \{[^}]*inset: -1px; bottom: -1\.875px; z-index: -1;[^}]*border-image-source: var\(--ci-tab-fill\);[^}]*border-image-slice: 12 16 8 16 fill;[^}]*border-image-outset: 0 8px/);
    const tabImages = Array.from(styles.matchAll(/%3Csvg[^"]+/g)).filter(match => match[0].includes("viewBox='0 0 120 36'") && match[0].includes("d='M0 35 C"));
    assert.equal(tabImages.length, 4);
    const tabPaths = tabImages.map(match => decodeURIComponent(match[0]).match(/\sd='([^']+)'/)[1]);
    assert.equal(new Set(tabPaths).size, 1, 'every tab fill follows the exact outline path');
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\] \{[^}]*fill='%23eaf0f6'/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]:hover \{[^}]*fill='%23e1eaf3'/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]\[aria-selected="true"\] \{[^}]*fill='%23fff'/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\] \{[^}]*min-height: 34px;[^}]*margin-bottom: -1px;[^}]*border-radius: 8px 8px 0 0/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\] \{[^}]*border: 1px solid transparent;[^}]*background-clip: border-box/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]\[aria-selected="true"\] \{[^}]*--ci-tab-surface: #fff;[^}]*background-clip: border-box; box-shadow: 0 1px 0 #fff/);
    assert.match(styles, /\.ci-panel-tabs \[role="tab"\]\[aria-selected="true"\]::before \{ left: -9px; right: -9px; border-image-outset: 0; background: linear-gradient\(#fff, #fff\) left bottom \/ 100% 1\.25px no-repeat; background-origin: border-box; \}/);
    assert.match(styles, /\.cmf-insights \.ci-panel-tabs \[role="tab"\]:focus:not\(:focus-visible\) \{ outline: none; \}/);
    assert.match(styles, /\.cmf-insights :focus-visible \{ outline: 2px solid #1768b2; outline-offset: 2px; \}/);
});

test('platform briefing Source toggles only its backing data and preserves open state on redraw', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const items = ['f1', 'f2'].map(id => {
        const detail = { open: false };
        const trigger = { attributes: {}, setAttribute(name, value) { this.attributes[name] = value; },
            addEventListener(name, callback) { this[name] = callback; } };
        return { dataset: { findingId: id }, detail, trigger, querySelector(selector) { return selector === 'details' ? detail : trigger; } };
    });
    const parts = new Map(['.ci-content', '.ci-metadata', '.ci-time', '.ci-refresh', '.ci-progress', '.ci-status'].map(selector => [selector, {
        textContent: '', innerHTML: '', scrollTop: 25, style: { setProperty() {} }, setAttribute() {}, querySelectorAll: () => items
    }]));
    const state = { host: { isConnected: true, querySelector: selector => parts.get(selector) },
        data: { Mode: 'ai', Findings: items, GeneratedAt: '2026-11-20T05:00:00Z' } };
    const context = vm.createContext({ active: state, platformFindings: () => 'Rendered bullets' });
    for (const name of ['platformPanelMarkup', 'updatePlatformStatus'])
        vm.runInContext(script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'))[0], context);
    vm.runInContext(script.match(/    function drawPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0], context);
    context.drawPlatform(state);
    items[0].trigger.click();
    assert.equal(items[0].detail.open, true);
    assert.equal(items[0].trigger.attributes['aria-expanded'], 'true');
    assert.equal(items[1].detail.open, false);
    context.drawPlatform(state);
    assert.equal(items[0].detail.open, true);
    assert.equal(items[0].trigger.attributes['aria-expanded'], 'true');
    assert.equal(parts.get('.ci-content').scrollTop, 25);
    items[0].trigger.click();
    assert.equal(items[0].detail.open, false);
    assert.equal(items[0].trigger.attributes['aria-expanded'], 'false');
});

test('platform briefing fills the existing bar from real stages without looping or visible numeric labels', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const parts = new Map(['.ci-content', '.ci-metadata', '.ci-time', '.ci-refresh', '.ci-progress', '.ci-status'].map(selector => [selector, {
        textContent: '', innerHTML: '', scrollTop: 0, attributes: {},
        style: { setProperty(name, value) { this[name] = value; } },
        setAttribute(name, value) { this.attributes[name] = value; }, querySelectorAll: () => []
    }]));
    const state = { host: { isConnected: true, querySelector: selector => parts.get(selector) }, data: null };
    const context = vm.createContext({ active: state, platformFindings: () => '<p>Analyzing dashboard data...</p>' });
    for (const name of ['platformPanelMarkup', 'updatePlatformStatus'])
        vm.runInContext(script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'))[0], context);
    vm.runInContext(script.match(/    function drawPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0], context);
    const loader = parts.get('.ci-progress');
    for (const stage of [5, 25, 45, 65, 85, 90, 98]) {
        state.data = { Mode: 'ai-loading', Progress: stage, Findings: [] };
        context.drawPlatform(state);
        assert.equal(loader.style['--ci-progress'], stage + '%');
        assert.equal(loader.attributes['aria-valuenow'], String(stage));
        assert.equal(loader.hidden, false);
        assert.equal(loader.textContent, '');
        assert.equal(parts.get('.ci-metadata').textContent, '');
        assert.equal(parts.get('.ci-status').textContent, '');
    }
    for (const invalid of [-20, 200, 'not-a-stage']) {
        state.data.Progress = invalid;
        context.drawPlatform(state);
        assert.equal(loader.style['--ci-progress'], (invalid === 200 ? 98 : 2) + '%');
    }
    state.data = { Mode: 'ai', Progress: 100, Findings: [], GeneratedAt: '2026-11-19T13:00:00Z' };
    context.drawPlatform(state);
    assert.equal(loader.hidden, true);
    assert.equal(loader.style['--ci-progress'], '100%');
    state.data = { Mode: 'ai-unavailable', Findings: [] };
    context.drawPlatform(state);
    assert.equal(loader.hidden, true);
    assert.equal(loader.style['--ci-progress'], '0%');
    state.data = null;
    context.drawPlatform(state);
    assert.equal(loader.hidden, false);
    assert.equal(loader.style['--ci-progress'], '2%');
    const platformMarkup = script.match(/    function render\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    assert.doesNotMatch(platformMarkup, /ci-progress-indeterminate|aria-valuetext/);
    assert.match(platformMarkup, /class="ci-progress"[^>]*><span><\/span>/);
    assert.match(page, /Scripts\/cmf-dashboard-insights\.js\?v=43/);
});

test('Overall briefing uses only its own snapshot endpoint and isolated reviewed Source controls', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const state = { overall: true, serial: 0, platform: 'CMF_OVERALL_ALL_COMPONENTS_TABLE', snapshotKey: 'overall-snapshot',
        host: { isConnected: true, querySelector: () => ({ scrollTop: 0 }) } };
    const requests = [];
    let pending;
    const context = vm.createContext({ active: state, AbortController, setTimeout: () => 1, clearTimeout() {}, drawPlatform() {},
        fetch(url, options) { requests.push({ url, body: JSON.parse(options.body) }); return new Promise(resolve => { pending = resolve; }); }
    });
    for (const name of ['escape', 'platformFindings', 'loadPlatform'])
        vm.runInContext(script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'))[0], context);
    const settle = () => new Promise(resolve => setImmediate(resolve));
    context.loadPlatform(state, true);
    assert.deepEqual(requests[0], { url: 'CMF_Web_portal.aspx/GetOverallDashboardBriefing',
        body: { platform: state.platform, snapshotKey: state.snapshotKey, refresh: true } });
    pending({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-page-briefing-v3', Mode: 'ai', Findings: [{ Title: 'WRONG SCOPE' }] } }) });
    await settle();
    assert.equal(state.data.Mode, 'ai-unavailable');
    assert.equal(state.data.Findings.length, 0);
    context.loadPlatform(state, false);
    pending({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'overall-page-briefing-v1', Mode: 'ai-loading', Findings: [{ Title: 'DRAFT' }] } }) });
    await settle();
    assert.equal(state.data.Findings.length, 0);
    const findings = [{ Id: 'f1', Title: 'Leading OEM volume', Summary: 'OEM A has 20 of 30 unique CMFs.', Section: 'highlights',
        Evidence: [{ Source: 'OEM Summary', Text: 'Evidence <A>' }] }];
    const overall = context.platformFindings({ Mode: 'ai', Pipeline: 'overall-page-briefing-v1', Findings: findings });
    const platform = context.platformFindings({ Mode: 'ai', Pipeline: 'platform-page-briefing-v3', Findings: findings });
    assert.match(overall, /aria-controls="overall-insight-source-f1"/);
    assert.doesNotMatch(overall, /platform-insight-source|ci-pagination|role="tab"|<A>/);
    assert.match(platform, /aria-controls="platform-insight-source-f1"/);
    const endpoint = server.match(/    public static CmfPlatformBriefing GetOverallDashboardBriefing\([\s\S]*?\r?\n    \}/)[0];
    assert.match(endpoint, /selection != OverallDashboardPlatformTable/);
    assert.match(endpoint, /DashboardSnapshotCacheKey\(snapshotKey\)/);
    assert.match(endpoint, /snapshot\.Platform != selection/);
    assert.match(endpoint, /snapshot\.Configuration != DashboardPanelConfiguration\(selection\)/);
    assert.match(endpoint, /snapshot\.CapturedAt\.Date != DateTime\.Today/);
    assert.match(endpoint, /CmfPlatformBriefingService\.CaptureSource\(snapshot\)/);
    assert.doesNotMatch(endpoint, /BuildHomeDashboardSnapshot|CmfRenderedInsightsService|GetSummaryIssueSourceTables|RankDashboardInsights/);
});

test('platform panels publish Summary first and isolate customer polling failures and refresh', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const state = { serial: 0, platform: 'CMF_NVL_H_ALL_COMPONENTS_TABLE', snapshotKey: 'panel-snapshot',
        host: { isConnected: true, querySelector: () => ({ scrollTop: 0 }) } };
    const requests = [], timers = new Map(), events = [];
    let timerId = 0;
    const context = vm.createContext({ active: state, AbortController,
        setTimeout(callback, delay) { timers.set(++timerId, { callback, delay }); return timerId; },
        clearTimeout(id) { timers.delete(id); },
        drawPlatform(target) { if (target.data && target.data.Mode === 'ai') events.push('summary-visible'); },
        fetch(url, options) {
            events.push(url);
            return new Promise(resolve => requests.push({ url, body: JSON.parse(options.body), resolve }));
        }
    });
    vm.runInContext(script.match(/    function loadPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0], context);
    const settle = () => new Promise(resolve => setImmediate(resolve));
    const reply = async (index, data) => { requests[index].resolve({ ok: true, json: async () => ({ d: data }) }); await settle(); };
    context.loadPlatform(state, false);
    assert.equal(requests[0].url, 'CMF_Web_portal.aspx/GetPlatformDashboardSummaryBriefing');
    await reply(0, { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai-loading', Progress: 45, Findings: [{ Title: 'DRAFT' }] });
    assert.equal(requests.length, 1);
    assert.equal(state.data.Findings.length, 0);
    Array.from(timers.values()).find(timer => timer.delay === 3000).callback();
    const summary = { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai', Findings: [{ Id: 'f1', Title: 'Reviewed Summary' }] };
    await reply(1, summary);
    assert.equal(requests[2].url, 'CMF_Web_portal.aspx/GetPlatformDashboardCustomerBriefing');
    assert.ok(events.indexOf('summary-visible') < events.indexOf(requests[2].url));
    assert.equal(state.data, summary);
    await reply(2, { Success: true, Pipeline: 'platform-customer-briefing-v1', Mode: 'ai-loading', Progress: 20, Findings: [{ Title: 'CUSTOMER DRAFT' }] });
    assert.equal(state.data, summary);
    assert.equal(state.customerData.Findings.length, 0);
    Array.from(timers.values()).filter(timer => timer.delay === 3000).at(-1).callback();
    await reply(3, { Success: true, Pipeline: 'platform-customer-briefing-v1', Mode: 'ai-unavailable', FailureCode: 'provider-authorization', Findings: [] });
    assert.equal(state.data, summary);
    assert.equal(state.customerData.Mode, 'ai-unavailable');
    context.loadPlatform(state, true, 'customers');
    assert.equal(requests[4].url, 'CMF_Web_portal.aspx/GetPlatformDashboardCustomerBriefing');
    assert.equal(requests[4].body.refresh, true);
    assert.equal(state.data, summary);
    await reply(4, { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai', Findings: [{ Title: 'WRONG PANEL' }] });
    assert.equal(state.customerData.Mode, 'ai-unavailable');
    assert.equal(state.data, summary);
    context.loadPlatform(state, true, 'customers');
    state.serial++;
    await reply(5, { Success: true, Pipeline: 'platform-customer-briefing-v1', Mode: 'ai', Findings: [{ Title: 'STALE CUSTOMER' }] });
    assert.equal(state.customerData.Mode, 'ai-loading');
    assert.equal(state.data, summary);
    context.loadPlatform(state, true, 'summary');
    assert.equal(requests[6].url, 'CMF_Web_portal.aspx/GetPlatformDashboardSummaryBriefing');
    await reply(6, summary);
    assert.equal(requests[7].url, 'CMF_Web_portal.aspx/GetPlatformDashboardCustomerBriefing');
    assert.equal(requests[7].body.refresh, false);
    await reply(7, { Success: false, ReloadRequired: true });
    assert.equal(state.data, summary);
    assert.equal(state.customerReloadRequired, true);
    context.loadPlatform(state, false);
    await reply(8, { Success: false, ReloadRequired: true });
    assert.equal(requests.length, 9);
    assert.equal(state.customerData.Mode, 'ai-unavailable');
    assert.equal(state.customerReloadRequired, true);
    state.data = { Mode: 'ai-loading', Progress: 45, Findings: [] };
    context.loadPlatform(state, true, 'customers');
    await reply(9, { Success: true, Pipeline: 'platform-customer-briefing-v1', Mode: 'ai', Findings: [] });
    assert.equal(requests[10].url, 'CMF_Web_portal.aspx/GetPlatformDashboardSummaryBriefing');
    assert.equal(requests[10].body.refresh, false);
    await reply(10, summary);
    assert.equal(state.data, summary);
});

test('platform panel markup and selected status preserve Summary during customer loading', () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const parts = new Map(['.ci-content', '.ci-metadata', '.ci-time', '.ci-refresh', '.ci-progress', '.ci-status'].map(key => [key, {
        textContent: '', setAttribute() {}, style: { setProperty() {} }
    }]));
    const state = { decoupled: true, summaryFinished: true, briefingView: 'summary', notice: '', customerNotice: '',
        host: { querySelector: key => parts.get(key) },
        data: { Pipeline: 'platform-summary-briefing-v1', Mode: 'ai', GeneratedAt: '2026-11-23T12:00:00Z', Findings: [
            { Id: 'f1', Title: 'Reviewed Summary', Summary: 'Summary text.', Section: 'highlights', Evidence: [] }] },
        customerData: { Mode: 'ai-loading', Progress: 30, Findings: [] } };
    const context = vm.createContext({});
    for (const name of ['escape', 'platformFindings', 'platformPanelMarkup', 'updatePlatformStatus'])
        vm.runInContext(script.match(new RegExp('    function ' + name + '\\([^)]*\\) \\{[\\s\\S]*?\\r?\\n    \\}'))[0], context);
    let html = context.platformPanelMarkup(state);
    assert.match(html, /Reviewed Summary/);
    assert.match(html, /platform-briefing-customers/);
    assert.match(html, /Analyzing customer issues/);
    context.updatePlatformStatus(state);
    assert.equal(parts.get('.ci-progress').hidden, true);
    assert.equal(parts.get('.ci-refresh').disabled, false);
    state.briefingView = 'customers';
    context.updatePlatformStatus(state);
    assert.equal(parts.get('.ci-progress').hidden, false);
    assert.equal(parts.get('.ci-refresh').disabled, true);
    state.customerData = { Pipeline: 'platform-customer-briefing-v1', Mode: 'ai', GeneratedAt: '2026-11-23T12:01:00Z', Findings: [
        { Id: 'f1', Title: 'Reviewed Customer', Summary: 'Customer text.', Section: 'focus',
            Evidence: [{ Source: 'CustomerIssues - Open reported customer issue', Text: 'Exact customer source.' }] }] };
    html = context.platformPanelMarkup(state);
    assert.match(html, /platform-insight-source-f1/);
    assert.match(html, /platform-customer-insight-source-f1/);
    assert.match(html, /data-finding-id="customer-f1"/);
    assert.equal((html.match(/id="platform-briefing-summary"/g) || []).length, 1);
    assert.equal((html.match(/id="platform-briefing-customers"/g) || []).length, 1);
    state.customerData = { Mode: 'ai-unavailable', Findings: [] };
    state.customerNotice = 'Customer provider unavailable.';
    assert.match(context.platformPanelMarkup(state), /Reviewed Summary/);
    context.updatePlatformStatus(state);
    assert.equal(parts.get('.ci-status').textContent, state.customerNotice);
    state.briefingView = 'summary';
    context.updatePlatformStatus(state);
    assert.equal(parts.get('.ci-status').textContent, '');
    assert.equal(parts.get('.ci-metadata').textContent, '1 finding');
});

test('platform briefing sends only snapshot scope and rejects stale or unapproved responses', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function loadPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    const state = { serial: 0, platform: 'CMF_NVL_H_ALL_COMPONENTS_TABLE', snapshotKey: 'opaque-snapshot',
        host: { isConnected: true, querySelector: () => ({ scrollTop: 0 }) } };
    const requests = [], timers = new Map();
    let timerId = 0, pending;
    const context = vm.createContext({ active: state, AbortController,
        setTimeout(callback, delay) { timers.set(++timerId, { callback, delay }); return timerId; },
        clearTimeout(timer) { timers.delete(timer); }, drawPlatform() {},
        fetch(url, options) { requests.push({ url, body: JSON.parse(options.body) }); return new Promise(resolve => { pending = resolve; }); }
    });
    vm.runInContext(definition, context);
    context.loadPlatform(state, true, 'summary');
    assert.deepEqual(requests[0], { url: 'CMF_Web_portal.aspx/GetPlatformDashboardSummaryBriefing',
        body: { platform: state.platform, snapshotKey: 'opaque-snapshot', refresh: true } });
    pending({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai-loading', Progress: 45, Findings: [{ Title: 'UNAPPROVED' }] } }) });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.data.Mode, 'ai-loading');
    assert.equal(state.data.Progress, 45);
    assert.equal(state.data.Findings.length, 0);
    Array.from(timers.values()).find(timer => timer.delay === 3000).callback();
    assert.equal(requests[1].body.refresh, false);
    pending({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai-review', Findings: [{ Title: 'DRAFT' }] } }) });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.data.Mode, 'ai-unavailable');
    assert.equal(state.data.Findings.length, 0);
    context.loadPlatform(state, false, 'summary');
    state.serial++;
    pending({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai', Findings: [{ Title: 'STALE' }] } }) });
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(state.data.Mode, 'ai-loading');
    const endpoint = server.match(/    private static CmfPlatformBriefing GetPlatformBriefingPanel\([\s\S]*?\r?\n    \}/)[0];
    assert.match(endpoint, /selection == OverallDashboardPlatformTable/);
    assert.match(endpoint, /DashboardSnapshotCacheKey\(snapshotKey\)/);
    assert.match(endpoint, /snapshot\.Platform != selection/);
    assert.match(endpoint, /snapshot\.Configuration != DashboardPanelConfiguration\(selection\)/);
    assert.match(endpoint, /snapshot\.CapturedAt\.Date != DateTime\.Today/);
    for (const [method, panel] of [['GetPlatformDashboardSummaryBriefing', 'summary'], ['GetPlatformDashboardCustomerBriefing', 'customers']]) {
        const wrapper = server.match(new RegExp('    public static CmfPlatformBriefing ' + method + '\\([\\s\\S]*?\\r?\\n    \\}'))[0];
        assert.match(wrapper, new RegExp('GetPlatformBriefingPanel\\(platform, snapshotKey, refresh, "' + panel + '"\\)'));
    }
    assert.doesNotMatch(endpoint, /snapshotContext|FetchArticle|CmfRenderedInsightsService/);
});

test('platform briefing retries transient polls without restarting generation and bounds repeated failures', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function loadPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    const state = { serial: 0, platform: 'NVL-S', snapshotKey: 'snapshot', host: { isConnected: true, querySelector: () => ({ scrollTop: 0 }) } };
    const requests = [], timers = new Map();
    let timerId = 0;
    const context = vm.createContext({ active: state, AbortController,
        setTimeout(callback, delay) { timers.set(++timerId, { callback, delay }); return timerId; },
        clearTimeout(timer) { timers.delete(timer); }, drawPlatform() {},
        fetch(url, options) { return new Promise((resolve, reject) => requests.push({ options, resolve, reject })); }
    });
    vm.runInContext(definition, context);
    const settle = () => new Promise(resolve => setImmediate(resolve));
    const fire = delay => {
        const entry = Array.from(timers.entries()).find(([, timer]) => timer.delay === delay);
        assert.ok(entry, 'Expected retry or poll timer at ' + delay);
        timers.delete(entry[0]);
        entry[1].callback();
    };
    context.loadPlatform(state, true, 'summary');
    const oldDeadline = Array.from(timers.values()).find(timer => timer.delay === 45000).callback;
    requests[0].resolve({ ok: false, status: 503 });
    await settle();
    assert.equal(state.data.Mode, 'ai-loading', 'A temporary server failure should leave the analysis loading.');
    fire(1000);
    oldDeadline();
    assert.equal(requests[1].options.signal.aborted, false, 'An old request deadline aborted the replacement request.');
    assert.equal(JSON.parse(requests[1].options.body).refresh, false, 'Transport retry restarted model generation.');
    requests[1].resolve({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai-loading', Progress: 45, Findings: [] } }) });
    await settle();
    fire(3000);
    requests[2].reject(new TypeError('Network interrupted'));
    await settle();
    assert.equal(state.data.Progress, 45, 'A transient failed poll reset completed analysis progress.');
    fire(1000);
    requests[3].resolve({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-summary-briefing-v1', Mode: 'ai', Findings: [], ExpiresAt: new Date(Date.now() + 60000).toISOString() } }) });
    await settle();
    assert.equal(state.data.Mode, 'ai');
    assert.equal(state.notice, '');
    assert.equal(timers.size, 0, 'Completed insights scheduled an unwanted latest-update reminder.');
    context.loadPlatform(state, false, 'summary');
    for (let attempt = 0; attempt < 3; attempt++) {
        requests[4 + attempt].reject(new TypeError('Offline'));
        await settle();
        if (attempt < 2) fire((attempt + 1) * 1000);
    }
    assert.equal(state.data.Mode, 'ai-unavailable');
    assert.equal(timers.size, 0, 'Transport failures retried without a bound.');
    context.loadPlatform(state, true, 'summary');
    requests[7].resolve({ ok: true, json: async () => ({ d: { Success: false, ReloadRequired: true, Message: 'Dashboard data expired.' } }) });
    await settle();
    assert.equal(state.reloadRequired, true);
    assert.equal(state.notice, 'Refresh the dashboard to load insights.');
    assert.equal(timers.size, 0, 'An invalid snapshot must not be retried automatically.');
    assert.match(script, /customerReloadRequired : state\.summaryReloadRequired/);
    assert.match(server, /Mode = "ai-unavailable", ReloadRequired = true/);
});

test('platform briefing retries recoverable generation once for each host and publishes only approved findings', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function loadPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    for (const overall of [false, true]) {
        const pipeline = overall ? 'overall-page-briefing-v1' : 'platform-summary-briefing-v1';
        const state = { overall, serial: 0, platform: overall ? 'CMF_OVERALL_ALL_COMPONENTS_TABLE' : 'NVL-H', snapshotKey: 'snapshot',
            host: { isConnected: true, querySelector: () => ({ scrollTop: 0 }) } };
        const requests = [], timers = new Map();
        let timerId = 0;
        const context = vm.createContext({ active: state, AbortController,
            setTimeout(callback, delay) { timers.set(++timerId, { callback, delay }); return timerId; },
            clearTimeout(timer) { timers.delete(timer); }, drawPlatform() {},
            fetch(url, options) { return new Promise(resolve => requests.push({ url, body: JSON.parse(options.body), resolve })); }
        });
        vm.runInContext(definition, context);
        const settle = () => new Promise(resolve => setImmediate(resolve));
        const respond = async data => {
            requests[requests.length - 1].resolve({ ok: true, json: async () => ({ d: { Success: true, Pipeline: pipeline, ...data } }) });
            await settle();
        };
        const fire = () => {
            const entry = Array.from(timers.entries()).find(([, timer]) => timer.delay === 3000);
            assert.ok(entry);
            timers.delete(entry[0]);
            entry[1].callback();
        };
        context.loadPlatform(state, false, 'summary');
        await respond({ Mode: 'ai-unavailable', FailureCode: 'unsupported-number', Message: 'Technical verification details', Findings: [{ Title: 'REJECTED' }] });
        assert.equal(state.data.Mode, 'ai-loading');
        assert.equal(state.data.Findings.length, 0);
        assert.equal(state.notice, 'Preparing insights...');
        fire();
        assert.deepEqual(requests[1].body, { platform: state.platform, snapshotKey: 'snapshot', refresh: true });
        await respond({ Mode: 'ai-loading', Progress: 65, Findings: [{ Title: 'DRAFT' }] });
        assert.equal(state.data.Findings.length, 0);
        fire();
        assert.equal(requests[2].body.refresh, false);
        await respond({ Mode: 'ai', Findings: [{ Title: 'APPROVED' }], ExpiresAt: new Date(Date.now() + 60000).toISOString() });
        assert.equal(state.data.Findings[0].Title, 'APPROVED');
        assert.equal(state.notice, '');
        assert.ok(requests.every(request => request.url.endsWith(overall ? 'GetOverallDashboardBriefing' : 'GetPlatformDashboardSummaryBriefing')));

        context.loadPlatform(state, true, 'summary');
        await respond({ Mode: 'ai-unavailable', FailureCode: 'grounding-rejected', Findings: [] });
        fire();
        await respond({ Mode: 'ai-unavailable', FailureCode: 'grounding-rejected', Message: 'Evidence verification failed', Findings: [{ Title: 'REJECTED AGAIN' }] });
        assert.equal(state.data.Mode, 'ai-unavailable');
        assert.equal(state.data.Findings.length, 0);
        assert.equal(state.notice, "Insights aren't available right now. Try again.");
        assert.equal(timers.size, 0, 'Generation retried more than once.');

        context.loadPlatform(state, false, 'summary');
        await respond({ Mode: 'ai-unavailable', FailureCode: 'unsupported-number', Findings: [] });
        const beforeSwitch = requests.length;
        state.serial++;
        fire();
        assert.equal(requests.length, beforeSwitch, 'A stale automatic retry started new work.');
    }
});

test('platform briefing stops permanent or unknown failures and keeps technical details out of user messages', async () => {
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    const definition = script.match(/    function loadPlatform\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0];
    for (const failureCode of ['provider-authorization', 'provider-rate-limited', 'cancelled', 'unexpected-error', 'unknown', undefined]) {
        const state = { serial: 0, platform: 'NVL-H', snapshotKey: 'snapshot', host: { isConnected: true, querySelector: () => ({ scrollTop: 0 }) } };
        const timers = new Map();
        let timerId = 0, respond;
        const context = vm.createContext({ active: state, AbortController,
            setTimeout(callback, delay) { timers.set(++timerId, { callback, delay }); return timerId; },
            clearTimeout(timer) { timers.delete(timer); }, drawPlatform() {},
            fetch() { return new Promise(resolve => { respond = resolve; }); }
        });
        vm.runInContext(definition, context);
        context.loadPlatform(state, false, 'summary');
        respond({ ok: true, json: async () => ({ d: { Success: true, Pipeline: 'platform-summary-briefing-v1',
            Mode: 'ai-unavailable', FailureCode: failureCode, Message: 'The generated insights did not pass evidence verification.', Findings: [] } }) });
        await new Promise(resolve => setImmediate(resolve));
        assert.equal(state.data.Mode, 'ai-unavailable');
        assert.equal(timers.size, 0);
        assert.equal(state.notice, failureCode === 'provider-rate-limited' ? 'Insights are busy. Try again shortly.'
            : "Insights aren't available right now. Try again.");
        vm.runInContext(script.match(/    function platformFindings\([^)]*\) \{[\s\S]*?\r?\n    \}/)[0], context);
        assert.equal(context.platformFindings(state.data), '', 'Failure message appeared twice.');
    }
});

test('AI Insights uses server-owned rendered snapshots and combines platform facts without HSD enrichment', () => {
    const endpoint = server.match(/    public static CmfDashboardInsights GetDashboardInsights\([\s\S]*?\r?\n    \}/)[0];
    assert.match(endpoint, /DashboardSnapshotCacheKey\(snapshotKey\)/);
    assert.match(endpoint, /snapshot\.Platform != selection/);
    assert.match(endpoint, /GetSummaryIssueSourceTablesForWeb\(selection\)/);
    assert.match(endpoint, /BuildHomeDashboardSnapshot\(table\)/);
    assert.doesNotMatch(endpoint, /FetchArticle|AnalyzeHybrid|CreateBalanceEvidenceCommand|WarmPlatforms/);
    assert.doesNotMatch(server, /LoadDashboardInsightEvidence/);
    assert.match(server, /CmfRenderedInsightsService\.Aggregate\(evidence\.Scope, platforms\)/);
    const service = fs.readFileSync(path.join(root, 'App_Code', 'CmfRenderedInsightsService.cs'), 'utf8');
    assert.doesNotMatch(service, /FetchArticle|AnalyzeHybrid|TryGenerateDashboardInsights/);
    assert.match(service, /TryRankDashboardFacts/);
    const aggregate = service.match(/    public static CmfDashboardInsights Aggregate\([\s\S]*?\r?\n    \}/)[0];
    assert.doesNotMatch(aggregate, /\b(?:TryRank\w*|Generate\w*|SelectFacts|BuildSource)\s*\(/);
    assert.match(aggregate, /GroupBy\(entry => new \{ entry\.Item\.Topic, entry\.Item\.Category \}\)/);
    assert.match(aggregate, /GroupBy\(entry => entry\.Item\.Id, StringComparer\.Ordinal\)/);
    const script = fs.readFileSync(path.join(root, 'Scripts', 'cmf-dashboard-insights.js'), 'utf8');
    assert.match(script, /snapshotKey: state\.snapshotKey, trendWeeks: state\.trendWeeks, chartView: state\.chartView/);
    assert.doesNotMatch(script, /snapshotContext:|JSON\.stringify\(snapshot\)/);
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
    assert.match(markup, /<li data-finding-id="[^"]*" class="cmf-assistant-text-section cmf-assistant-description-section"><h3>/);
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
    const factual = context.findings([{ Id: 'kpi', Kind: 'dashboard', Title: 'Open today', Evidence: '20 <CMFs>',
        AiReviewed: false, Reasons: [{ Source: 'CMF Summary' }] }], 'No facts.');
    assert.match(factual, /Verified dashboard facts \| Unranked/);
    assert.match(factual, /20 &lt;CMFs&gt;/);
    assert.match(factual, /Source: CMF Summary/);
    assert.doesNotMatch(factual, /distinct HSDs|Suggested review|AI-reviewed|Why related/);
    const briefing = context.findings([
        { Id: 'progress', Kind: 'briefing', Category: 'Progress', Title: 'Backlog is shrinking', Evidence: 'Less unresolved work.',
            PlatformCount: 1, Reasons: [{ Source: 'Summary', Text: 'Open: 10 versus 20.' }] },
        { Id: 'mixed', Kind: 'briefing', Category: 'Needs Attention', Label: 'Mixed Signals', Title: 'Closures rose <but> backlog grew', Evidence: 'Measures moved in different directions.',
            PlatformCount: 2, Reasons: [{ Source: 'NVL-H <source>', Text: 'Open: 20 versus 18 <estimate>.' }] }
    ], 'No material changes.');
    assert.ok(briefing.indexOf('Needs Attention') < briefing.indexOf('Progress'));
    assert.match(briefing, /data-finding-id="mixed" class="cmf-assistant-text-section cmf-assistant-impact-section"><h3><span>/);
    assert.match(briefing, /data-finding-id="progress" class="cmf-assistant-text-section cmf-assistant-metrics-section"><h3><span>/);
    assert.match(briefing, /<h4 class="ci-category">Needs Attention<\/h4>/);
    assert.match(briefing, /<p class="ci-small ci-finding-label">Mixed Signals<\/p>/);
    assert.doesNotMatch(briefing, /<h4 class="ci-category">(?:Mixed Signals|Concentration|Direction Over Time)<\/h4>/);
    assert.match(briefing, /Closures rose &lt;but&gt; backlog grew/);
    assert.match(briefing, /<details class="ci-support"><summary>Evidence from 2 platforms<\/summary>/);
    assert.match(briefing, /NVL-H &lt;source&gt;[\s\S]*20 versus 18 &lt;estimate&gt;/);
    assert.doesNotMatch(briefing, /<details[^>]*open|Verified dashboard facts|AI-reviewed|distinct HSDs|Suggested review/);
    const concentrations = context.findings([
        { Id: 'component', Kind: 'briefing', Category: 'Concentration', Label: 'Workload', Title: 'HW leads the open workload', Reasons: [] },
        { Id: 'milestone', Kind: 'briefing', Category: 'Concentration', Label: 'Milestone', Title: 'QS leads the milestone workload', Reasons: [] }
    ], 'No concentrations.');
    assert.equal((concentrations.match(/class="cmf-assistant-text-section cmf-assistant-description-section"/g) || []).length, 2);
    assert.equal((concentrations.match(/<h4 class="ci-category">Concentration<\/h4>/g) || []).length, 1);
    assert.equal((concentrations.match(/data-finding-id=/g) || []).length, 2);
    assert.doesNotMatch(concentrations, /<h4 class="ci-category">(?:Needs Attention|Progress|Direction Over Time|Workload|Milestone)<\/h4>/);
    assert.doesNotMatch(script, /categories\.push|Other Findings/);
    const platformBriefing = context.findings([{ Id: 'platform', Kind: 'briefing', Category: 'Needs Attention',
        Title: 'Platform finding', Evidence: 'Shared section layout.', Reasons: [] }], 'No findings.');
    assert.match(platformBriefing, /class="cmf-assistant-text-section cmf-assistant-impact-section"/);
    assert.doesNotMatch(script, /assistantSections|state\.host\.id === 'homeOverallInsights'/);
    assert.match(styles, /\.cmf-insights \.ci-briefing \.cmf-assistant-text-section\s*\{[^}]*padding: 8px 9px;/);
    assert.doesNotMatch(styles, /#homeOverallInsights \.ci-briefing/);
});

test('Overall platform button keeps shared rail spacing rather than separate right alignment', () => {
    assert.doesNotMatch(theme, /\.platform-chip\[data-platform="CMF_OVERALL_ALL_COMPONENTS_TABLE"\]\s*\{[^}]*margin-left:\s*auto;/);
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
    assert.match(host.innerHTML, /<span class="overall-break-name">QS<\/span><span class="overall-break-pct">\(80%\)<\/span><\/span>/);
    assert.match(host.innerHTML, /<span class="overall-break-name">Beta<\/span><span class="overall-break-pct">\(20%\)<\/span><\/span>/);
    const metrics = Array.from(host.innerHTML.matchAll(/<span class="overall-break-metrics">([\s\S]*?)<\/span>/g), match => match[1]);
    assert.equal(metrics.length, 2);
    assert.ok(metrics.every(metric => /^<strong class="overall-break-count"><a[^>]*>\d+<\/a><\/strong>$/.test(metric)));
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
    const rangeStyle = page.match(/\.overall-trend-range-select \{([^}]*)\}/);
    assert.ok(rangeStyle);
    assert.match(rangeStyle[1], /font-size: 12px;/);
    for (const selectorId of ['homeOverallTrendRange', 'homeSpecificTrendRange']) {
        assert.match(page, new RegExp('<select id="' + selectorId + '" class="overall-trend-range-select"'));
    }
});

test('Overall trend stacks week-end balances and retains weekly clustered bars and drilldowns', () => {
    const options = [];
    const popups = [];
    const view = { value: 'line' };
    const specificView = { value: 'line' };
    const chart = {
        setOption: option => options.push(option),
        getOption: () => undefined,
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
    assert.ok(options[0].series.slice(0, 2).every(series => series.stack === 'cmf-status'));
    assert.equal(options[0].series[0].lineStyle.width, 0);
    assert.equal(options[0].series[0].emphasis.lineStyle.width, 0);
    assert.equal(options[0].series[1].lineStyle.width, 0);
    assert.equal(options[0].series[0].symbol, 'none');
    assert.equal(options[0].series[1].symbol, 'none');
    const contour = options[0].series[2];
    assert.equal(contour.id, 'cmf-backlog-contour');
    assert.equal(contour.stack, undefined);
    assert.equal(contour.lineStyle.color, '#0369a1');
    assert.equal(contour.lineStyle.width, 3);
    assert.equal(contour.symbol, 'circle');
    assert.equal(contour.showAllSymbol, true);
    assert.equal(contour.silent, true);
    assert.equal(contour.tooltip.show, false);
    assert.equal(contour.data[0].value, 132);
    assert.equal(contour.data[0].weekOffset, 5);
    assert.equal(JSON.stringify(options[0].legend.data), JSON.stringify(['Open', 'Implemented']));
    assert.deepEqual(Array.from(options[0].legend.data, options[0].legend.formatter), ['Open', 'Implemented']);
    assert.ok(options[0].series.every(series => !series.label));
    assert.equal(options[0].grid.top, 30);
    assert.equal(options[0].grid.left, 30, 'Reserve a compact stable gutter with room for two-digit issue counts.');
    assert.equal(options[0].grid.containLabel, false, 'Automatic label sizing must not shift work-week positions.');
    assert.equal(options[0].grid.bottom, 29, 'Reserve space for work-week labels explicitly.');
    assert.equal(options[0].grid.right, 20, 'Reserve only the space needed for the last WW label.');
    assert.equal(options[0].yAxis.minInterval, 1, 'Overall backlog uses whole-number issue ticks.');
    assert.equal(options[0].xAxis.axisLabel.showMinLabel, true);
    assert.equal(options[0].xAxis.axisLabel.showMaxLabel, true);
    assert.equal(options[0].xAxis.axisLabel.hideOverlap, true);
    assert.equal(options[0].series[0].areaStyle.color, 'rgba(128,0,0,0.12)');
    assert.equal(options[0].series[1].areaStyle.color, 'rgba(3,105,161,0.12)');
    const tooltip = options[0].tooltip.formatter([
        { seriesName: 'Open', value: 46, axisValueLabel: 'WW6', marker: '[blue]' },
        { seriesName: 'Implemented', value: 86, axisValueLabel: 'WW6', marker: '[maroon]' },
        { seriesName: 'Backlog contour', value: 132, axisValueLabel: 'WW6' }
    ]);
    assert.match(tooltip, /WW6<\/span><span style="font-weight:900">Total: 132/);
    assert.match(tooltip, /\[blue\]Open[\s\S]*46[\s\S]*\[maroon\]Implemented[\s\S]*86/);
    assert.doesNotMatch(tooltip, /Backlog contour|264/);
    assert.equal(options[0].tooltip.formatter([]), '');
    chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
    assert.deepEqual(popups[0].slice(0, 3), ['Open and Implemented', 'WW6', 5]);
    assert.equal(popups[0][3].showOpen, true);
    assert.equal(popups[0][3].showClosed, true);
    view.value = 'bar';
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 6);
    const bars = options[1];
    assert.equal(bars.series[0].type, 'bar');
    assert.equal(bars.series[1].type, 'bar');
    assert.equal(bars.series[0].name, 'Open', 'Keep the internal series key for legend toggling and drilldowns.');
    assert.deepEqual(Array.from(bars.series, series => bars.legend.formatter(series.name)), ['Accepted', 'Closed']);
    const flowPoints = [
        { seriesName: 'Open', value: 7, axisValueLabel: 'WW6', marker: '[blue]' },
        { seriesName: 'Closed', value: 6, axisValueLabel: 'WW6', marker: '[green]' }
    ];
    const flowTooltip = bars.tooltip.formatter(flowPoints);
    assert.match(flowTooltip, /WW6[\s\S]*\[blue\]Accepted: 7[\s\S]*\[green\]Closed: 6/);
    assert.doesNotMatch(flowTooltip, /\bOpen\b|\bOpened\b/);
    assert.equal(bars.tooltip.formatter([]), '');
    assert.equal(bars.tooltip.formatter(flowPoints.slice(1)), 'WW6<br/>[green]Closed: 6');
    assert.equal(bars.color[0], '#0369a1');
    assert.equal(bars.color[1], '#16a34a');
    assert.ok(bars.series.every(series => series.barMaxWidth === 18 && series.itemStyle.borderWidth === 1));
    assert.equal(bars.series[0].itemStyle.color, '#f0f9ff');
    assert.equal(bars.series[0].itemStyle.borderColor, '#0369a1');
    assert.equal(bars.series[1].itemStyle.color, '#dcfce7');
    assert.equal(bars.series[1].itemStyle.borderColor, '#16a34a');
    assert.equal(bars.grid.top, 40);
    assert.equal(bars.grid.left, 30);
    assert.equal(bars.grid.containLabel, false);
    assert.equal(bars.grid.bottom, 29);
    assert.equal(bars.grid.right, 12);
    assert.equal(bars.yAxis.minInterval, 1, 'Overall weekly flow uses whole-number issue ticks.');
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
    assert.deepEqual(popups[1].slice(0, 3), ['All', 'WW6', 5]);
    for (const name of ['Open', 'Closed']) {
        const event = { offsetX: 10, offsetY: 10, target: {} };
        const count = popups.length;
        chart.__cmfTrendZrClickHandler(event);
        assert.equal(popups.length, count, 'Series graphic targets never use the combined grid fallback, regardless of event order.');
        chart.__cmfTrendClickHandler({ componentType: 'series', seriesName: name, dataIndex: 5, event });
        assert.deepEqual(popups[count].slice(0, 3), [name, 'WW11', 0]);
        assert.equal(popups[count][3].showOpen, name === 'Open');
        assert.equal(popups[count][3].showClosed, name === 'Closed');
        chart.__cmfTrendZrClickHandler(event);
        assert.equal(popups.length, count + 1, 'The grid fallback must not overwrite the clicked series filter.');
        popups.pop();
    }
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 12);
    assert.equal(options[2].series[0].data.length, 12);
    assert.ok(options[2].series.every(series => series.barMaxWidth === 8));
    view.value = 'line';
    context.renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'overallChart', 12);
    assert.equal(options[3].series[0].type, 'line');
    assert.equal(chart.__cmfTrendClickHandler, null, 'Backlog charts retain their combined-grid drilldown.');
    assert.equal(options[3].series[0].data[0].value, 80);
    assert.equal(options[3].series[1].data[0].value, 40);
    assert.deepEqual(Array.from(options[3].legend.data, options[3].legend.formatter), ['Open', 'Implemented']);
    view.value = 'bar';
    context.renderHomeTrendChart(snapshot, 'homeTrendChart', 'specificChart', 6);
    assert.equal(options[4].series[0].type, 'line');
    assert.equal(options[4].series[0].name, 'Implemented');
    assert.equal(options[4].series[1].name, 'Open');
    assert.deepEqual(Array.from(options[4].legend.data, options[4].legend.formatter), ['Open', 'Implemented']);
    assert.equal(options[4].yAxis.minInterval, 1, 'Platform backlog uses whole-number issue ticks.');
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
    assert.equal(options[4].series[2].lineStyle.color, '#0369a1');
    assert.ok(options[4].series.slice(0, 2).every(series => series.stack === 'cmf-status'));
    const legendHandler = chart.__cmfTrendLegendHandler;
    legendHandler({ selected: { Open: false, Implemented: true } });
    const implementedOption = options.pop();
    assert.equal(implementedOption.grid.left, 30);
    const implementedOnly = implementedOption.series[0];
    assert.equal(implementedOnly.id, 'cmf-backlog-contour');
    assert.equal(implementedOnly.data[0].value, 86);
    legendHandler({ selected: { Open: true, Implemented: false } });
    assert.equal(options.pop().series[0].data[0].value, 46);
    legendHandler({ selected: { Open: false, Implemented: false } });
    const hiddenOption = options.pop();
    assert.equal(hiddenOption.grid.left, 30, 'Retain the two-digit gutter for the zero-level contour.');
    assert.equal(hiddenOption.series[0].data.length, 6, 'Keep a contour point for each week when both areas are deselected.');
    assert.ok(hiddenOption.series[0].data.every(point => point.value === 0));
    legendHandler({ selected: { Open: true, Implemented: true } });
    const restoredOption = options.pop();
    assert.equal(restoredOption.grid.left, 30);
    assert.equal(restoredOption.series[0].data[0].value, 132);
    chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
    assert.deepEqual(popups[2].slice(0, 3), ['Open and Implemented', 'WW6', 5]);
    specificView.value = 'bar';
    context.renderHomeTrendChart(snapshot, 'homeTrendChart', 'specificChart', 6);
    assert.equal(chart.__cmfTrendLegendHandler, null, 'Weekly bars remove the contour legend listener.');
    assert.equal(options[5].series[0].type, 'bar');
    assert.equal(options[5].series[1].name, 'Closed');
    assert.deepEqual(Array.from(options[5].series, series => options[5].legend.formatter(series.name)), ['Accepted', 'Closed']);
    assert.equal(options[5].tooltip.formatter(flowPoints), flowTooltip);
    for (const name of ['Open', 'Closed']) {
        const event = { offsetX: 10, offsetY: 10, target: {} };
        const count = popups.length;
        chart.__cmfTrendZrClickHandler(event);
        assert.equal(popups.length, count);
        chart.__cmfTrendClickHandler({ componentType: 'series', seriesName: name, dataIndex: 0, event });
        assert.deepEqual(popups[count].slice(0, 3), [name, 'WW6', 5]);
        chart.__cmfTrendZrClickHandler(event);
        assert.equal(popups.length, count + 1);
        popups.pop();
    }
    assert.equal(options[5].yAxis.minInterval, 1, 'Platform weekly flow uses whole-number issue ticks.');
    assert.equal(options[5].series[0].data[0].value, 7);
    assert.equal(options[5].series[1].data[0].value, 6);
    assert.ok(options[5].series.every(series => !series.stack && series.barMaxWidth === 18));
    options.forEach(option => {
        assert.equal(option.legend.textStyle.fontSize, 12);
        assert.equal(option.xAxis.axisLabel.fontSize, 12);
        assert.equal(option.yAxis.axisLabel.fontSize, 12);
        assert.equal(option.tooltip.textStyle.fontSize, 12);
        option.series.filter(series => series.type === 'bar').forEach(series => assert.equal(series.label.fontSize, 12));
    });
    context.renderHomeTrendChart(snapshot, 'homeTrendChart', 'specificChart', 12);
    assert.ok(options[6].series.every(series => series.data.length === 12 && series.barMaxWidth === 8));
    assert.equal(JSON.stringify(options[6].series), JSON.stringify(options[2].series));
});

test('weekly flow leaves other weeks unaffected and blanks only the paired bar on bar hover', () => {
    const definition = page.match(/        function renderHomeTrendChart\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    for (const hostId of ['homeTrendChart', 'homeOverallTrendChart']) {
        const options = [];
        const actions = [];
        const handlers = new Map();
        const view = { value: 'bar' };
        const chart = {
            setOption: option => options.push(option), getOption: () => ({}), resize() {},
            dispatchAction: action => actions.push(action),
            on: (name, handler) => handlers.set(name, handler),
            off: (name, handler) => { if (handlers.get(name) === handler) handlers.delete(name); },
            getZr: () => ({ on() {}, off() {} })
        };
        const context = vm.createContext({
            document: { getElementById: id => /TrendView$/.test(id) ? view : {}, body: {} },
            window: { CMF_PORTAL: { ids: {} } }, echarts: { init: () => chart },
            getComputedStyle: () => ({ fontFamily: 'Cambria' }), getTextById: () => '', escapeHtml: String
        });
        vm.runInContext(definition[0], context);
        const snapshot = { Trend: [
            { WeekLabel: 'WW47', NewIssues: 2, ResolvedIssues: 5, WeekEndOpenIssues: 6, WeekEndImplementedIssues: 4 },
            { WeekLabel: 'WW48', NewIssues: 5, ResolvedIssues: 3, WeekEndOpenIssues: 8, WeekEndImplementedIssues: 5 }
        ] };
        context.renderHomeTrendChart(snapshot, hostId, 'chart', 6);
        assert.equal(options[0].tooltip.axisPointer.type, 'shadow');
        assert.equal(options[0].tooltip.showContent, false, 'Weekly flow has no floating hover count tooltip.');
        assert.equal(options[0].xAxis.axisPointer.triggerEmphasis, false, 'The hover handler controls bar emphasis without competing axis highlights.');
        options[0].series.forEach(series => {
            assert.equal(series.emphasis.focus, 'none', 'Other bars must not be dimmed.');
            assert.equal(series.emphasis.itemStyle.color, series.name === 'Open' ? '#7dd3fc' : '#86efac');
            assert.notEqual(series.emphasis.itemStyle.color, series.itemStyle.borderColor);
            assert.notEqual(series.emphasis.itemStyle.color, series.itemStyle.color);
        });
        const over = handlers.get('mouseover');
        const out = handlers.get('mouseout');
        assert.ok(over && out);
        for (const seriesName of ['Open', 'Closed']) {
            const seriesIndex = seriesName === 'Open' ? 0 : 1;
            over({ componentType: 'series', seriesName, seriesIndex, dataIndex: 0 });
            const barOption = options.pop();
            assert.equal(barOption.xAxis.axisPointer.shadowStyle.opacity, 0);
            barOption.series.forEach((series, index) => {
                assert.equal(series.data[0].itemStyle.color, index === seriesIndex ? options[0].series[index].itemStyle.color : 'transparent');
                assert.equal(series.data[1].itemStyle.color, options[0].series[index].itemStyle.color, 'Other weeks retain normal fills.');
                assert.equal(series.data[1].value, options[0].series[index].data[1].value);
            });
            assert.equal(JSON.stringify(actions.splice(0)), JSON.stringify([
                { type: 'downplay' },
                { type: 'showTip', seriesIndex, dataIndex: 0 },
                { type: 'downplay', seriesIndex: 1 - seriesIndex, dataIndex: 0 },
                { type: 'highlight', seriesIndex, dataIndex: 0 }
            ]));
            out({ componentType: 'series', seriesName, seriesIndex, dataIndex: 0 });
            const weekOption = options.pop();
            assert.equal(weekOption.xAxis.axisPointer.shadowStyle.opacity, 1);
            weekOption.series.forEach((series, index) => {
                assert.equal(series.data[0].itemStyle.color, options[0].series[index].itemStyle.color);
                assert.equal(series.data[1].itemStyle.color, options[0].series[index].itemStyle.color, 'Other weeks retain their fills during week shading.');
            });
            assert.equal(JSON.stringify(actions.splice(0)), JSON.stringify([
                { type: 'downplay' },
                { type: 'showTip', seriesIndex: 0, dataIndex: 0 },
                { type: 'highlight', batch: [
                { seriesIndex: 0, dataIndex: 0 }, { seriesIndex: 1, dataIndex: 0 }
                ] }
            ]));
            assert.equal(actions.length, 0);
        }
        handlers.get('updateAxisPointer')({ axesInfo: [{ value: 1 }] });
        const nextWeek = options.pop();
        nextWeek.series.forEach((series, index) => series.data.forEach(point => assert.equal(point.itemStyle.color, options[0].series[index].itemStyle.color)));
        actions.splice(0);
        handlers.get('updateAxisPointer')({ axesInfo: [{ value: 1 }] });
        assert.equal(actions.length, 0, 'Unchanged pointer updates cannot recursively reapply styles.');
        chart.__cmfTrendZrGlobalOutHandler();
        const reset = options.pop();
        reset.series.forEach((series, index) => series.data.forEach(point => assert.equal(point.itemStyle.color, options[0].series[index].itemStyle.color)));
        assert.equal(actions.pop().type, 'downplay');
        const count = options.length;
        over({ componentType: 'legend', seriesName: 'Open' });
        out(null);
        assert.equal(options.length, count);
        view.value = 'line';
        context.renderHomeTrendChart(snapshot, hostId, 'chart', 6);
        assert.equal(handlers.has('mouseover'), false);
        assert.equal(handlers.has('mouseout'), false);
        assert.equal(chart.__cmfTrendBarMouseOverHandler, null);
        assert.equal(chart.__cmfTrendZrGlobalOutHandler, null);
        assert.equal(options.at(-1).tooltip.axisPointer.type, 'line');
        assert.equal(options.at(-1).tooltip.showContent, undefined, 'Backlog hover counts remain enabled.');
        assert.equal(options.at(-1).xAxis.axisPointer, undefined, 'Backlog axis behavior is unchanged.');
    }
});

test('weekly flow popup defaults to the clicked series and labels Accepted without locking status filters', () => {
    const calls = [];
    const context = vm.createContext({ openSummaryIssueLinksPopup: (...args) => calls.push(args) });
    const definition = page.match(/        function openTrendWeekIssueLinksPopup\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    vm.runInContext(definition[0], context);
    for (const name of ['Open', 'Closed', 'All', 'Open and Implemented']) {
        context.openTrendWeekIssueLinksPopup(name, "WW'48", 0);
        const [metric, label, options] = calls.pop();
        assert.equal(metric, name === 'Closed' ? 'closed' : 'open');
        assert.equal(label, (name === 'Open' ? 'Accepted' : name) + " • WW'48");
        assert.equal(options.seriesName, name === 'Open' || name === 'Closed' ? 'All' : name,
            'Weekly flow filters remain switchable after opening a single series.');
        assert.equal(options.showOpen, name !== 'Closed');
        assert.equal(options.showClosed, name !== 'Open');
        assert.equal(options.weekOffset, 0);
    }
    context.openTrendWeekIssueLinksPopup('Open and Implemented', "WW'47", 1, { showOpen: false, showClosed: true });
    const backlog = calls.pop()[2];
    assert.equal(backlog.seriesName, 'Open and Implemented');
    assert.equal(backlog.showOpen, false);
    assert.equal(backlog.showClosed, true);
    context.openTrendWeekIssueLinksPopup('All', "WW'47", 1, { showOpen: true, showClosed: false });
    const flow = calls.pop()[2];
    assert.equal(flow.showOpen, true);
    assert.equal(flow.showClosed, false);
    assert.match(page, /syncSummaryTrendIssueStatusControls\(!popupOptions \|\| popupOptions.showOpen !== false,\s*!popupOptions \|\| popupOptions.showClosed !== false\)/);
    assert.match(page, /id="summaryIssuesOpenLabel">Open<\/span>/);
    assert.match(page, /getElementById\('summaryIssuesOpenLabel'\).textContent = sourceType === 'trend-week' && !isBacklogTrendPopup \? 'Accepted' : 'Open'/);
});

test('trend week drilldowns inherit live legend selections in both views and hosts', () => {
    const definition = page.match(/        function renderHomeTrendChart\([^)]*\) \{[\s\S]*?\r?\n        \}/);
    assert.ok(definition);
    for (const hostId of ['homeTrendChart', 'homeOverallTrendChart']) {
        for (const chartType of ['bar', 'line']) {
            let selected = {};
            const popups = [];
            const chart = {
                setOption() {}, getOption: () => ({ legend: [{ selected }] }), on() {}, off() {}, resize() {},
                dispatchAction() {},
                getZr: () => ({ on() {}, off() {} }), containPixel: () => true, convertFromPixel: () => 1
            };
            const context = vm.createContext({
                document: { getElementById: id => /TrendView$/.test(id) ? { value: chartType } : {}, body: {} },
                window: { CMF_PORTAL: { ids: {} } }, echarts: { init: () => chart },
                getComputedStyle: () => ({ fontFamily: 'Cambria' }), getTextById: () => '', escapeHtml: String,
                openTrendWeekIssueLinksPopup: (...args) => popups.push(args)
            });
            vm.runInContext(definition[0], context);
            context.renderHomeTrendChart({ Trend: [
                { WeekLabel: 'WW47', NewIssues: 0, ResolvedIssues: 3, WeekEndOpenIssues: 0, WeekEndImplementedIssues: 3 },
                { WeekLabel: 'WW48', NewIssues: 4, ResolvedIssues: 5, WeekEndOpenIssues: 4, WeekEndImplementedIssues: 5 }
            ] }, hostId, 'chart', 6);
            const second = chartType === 'line' ? 'Implemented' : 'Closed';
            for (const showOpen of [true, false]) for (const showClosed of [true, false]) {
                selected = { Open: showOpen, [second]: showClosed };
                const count = popups.length;
                chart.__cmfTrendAxisPointerHandler({ axesInfo: [{ value: 1 }] });
                chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
                assert.equal(popups.length, count + (showOpen || showClosed ? 1 : 0));
                if (showOpen || showClosed) {
                    const call = popups.pop();
                    assert.deepEqual(call.slice(0, 3), [chartType === 'line' ? 'Open and Implemented' : 'All', 'WW48', 0]);
                    assert.equal(call[3].showOpen, showOpen);
                    assert.equal(call[3].showClosed, showClosed);
                }
                if (chartType === 'bar') for (const name of ['Open', 'Closed']) {
                    const enabled = name === 'Open' ? showOpen : showClosed;
                    chart.__cmfTrendClickHandler({ componentType: 'series', seriesName: name, dataIndex: 1 });
                    assert.equal(popups.length, enabled ? 1 : 0, 'A hidden series cannot open a popup.');
                    if (enabled) {
                        const call = popups.pop();
                        assert.equal(call[3].showOpen, name === 'Open');
                        assert.equal(call[3].showClosed, name === 'Closed');
                    }
                }
            }
            selected = { Open: true, [second]: false };
            chart.__cmfTrendAxisPointerHandler({ axesInfo: [{ value: 0 }] });
            chart.__cmfTrendZrClickHandler({ offsetX: 10, offsetY: 10 });
            assert.equal(popups.length, 0, 'A week with issues only in the hidden series must not open a popup.');
        }
    }
});

['homeOverallTrendChart', 'homeTrendChart'].forEach(hostId => test('filtered ' + hostId + ' trends disable the week selector and preserve the full filtered timeline and week offsets', () => {
    let option;
    let legendSelected = {};
    const popups = [];
    const view = { value: 'line' };
    const dateMode = { value: '7days' };
    const rangeSelector = { value: '6', disabled: false, parentElement: { style: { display: '' } } };
    const otherRangeSelector = { value: '6', disabled: false, parentElement: { style: { display: '' } } };
    const rangeSelectorId = hostId === 'homeOverallTrendChart' ? 'homeOverallTrendRange' : 'homeSpecificTrendRange';
    const chart = {
        setOption: value => { option = value; },
        getOption: () => ({ legend: [{ selected: legendSelected }] }),
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
            assert.deepEqual(popups.at(-1).slice(0, 3), [chartType === 'line' ? 'Open and Implemented' : 'All', 'WW' + populatedIndex, 11 - populatedIndex]);
            assert.equal(popups.at(-1)[3].showOpen, true);
            assert.equal(popups.at(-1)[3].showClosed, true);
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
    dateMode.value = 'all';
    for (const chartType of ['line', 'bar']) {
        view.value = chartType;
        for (const trend of [[], emptySnapshot.Trend, [{ WeekLabel: 'Zero', WeekEndOpenIssues: 0, WeekEndImplementedIssues: 0, NewIssues: 0, ResolvedIssues: 0 }]]) {
            delete context.window.CMF_PORTAL['lastHomeDashboardKey_' + hostId];
            context.renderHomeTrendChart({ Trend: trend }, hostId, 'trendChart', 6);
            assert.equal(option.yAxis.show, false, 'Raw empty data hides the vertical axis in either view.');
            assert.equal(option.yAxis.axisLabel.show, false, 'Hidden vertical-axis labels must not change the plotting width.');
            const emptyLeft = 30;
            assert.equal(option.grid.left, emptyLeft, 'Reserve the same two-digit gutter for empty charts.');
            assert.equal(option.grid.containLabel, false);
            assert.deepEqual(Array.from(option.xAxis.data), trend.slice(-6).map(item => item.WeekLabel));
            if (chartType === 'line') {
                assert.equal(option.series[2].data.length, 0, 'Raw empty data has no zero-level contour or markers.');
                for (const selected of [
                    { Open: false, Implemented: true },
                    { Open: false, Implemented: false },
                    { Open: true, Implemented: false },
                    { Open: true, Implemented: true }
                ]) {
                    chart.__cmfTrendLegendHandler({ selected });
                    assert.equal(option.grid.left, emptyLeft, 'Legend changes preserve the empty chart gutter.');
                    assert.equal(option.series[0].data.length, 0, 'Legend changes cannot restore a contour for raw empty data.');
                }
            }
        }
        delete context.window.CMF_PORTAL['lastHomeDashboardKey_' + hostId];
        context.renderHomeTrendChart({ Trend: [{
            WeekLabel: 'Populated', WeekEndOpenIssues: 0, WeekEndImplementedIssues: 2, NewIssues: 0, ResolvedIssues: 2
        }] }, hostId, 'trendChart', 6);
        const populatedOption = option;
        assert.equal(populatedOption.yAxis.show, true, 'A chart with issues retains its vertical axis.');
        assert.equal(populatedOption.yAxis.axisLabel.show, true);
        assert.equal(populatedOption.yAxis.minInterval, 1);
        if (chartType === 'line') {
            for (const selected of [
                { Open: true, Implemented: false },
                { Open: false, Implemented: true },
                { Open: false, Implemented: false },
                { Open: true, Implemented: true }
            ]) {
                chart.__cmfTrendLegendHandler({ selected });
                assert.equal(option.yAxis, undefined, 'Legend filtering must not change vertical-axis visibility.');
                const expected = selected.Implemented ? 2 : 0;
                assert.equal(option.series[0].data.length, 1, 'Legend filtering retains the contour even when both areas are deselected.');
                assert.equal(option.series[0].data[0].value, expected);
            }
            assert.equal(populatedOption.yAxis.show, true);
        }
    }
    const rangeSnapshot = { Trend: Array.from({ length: 12 }, (_, index) => ({
        WeekLabel: 'WW' + index, WeekEndOpenIssues: 2, WeekEndImplementedIssues: 3, NewIssues: 2, ResolvedIssues: 3
    })) };
    for (const chartType of ['line', 'bar']) {
        view.value = chartType;
        for (const openSelected of [false, true]) {
            for (const secondSelected of [false, true]) {
                legendSelected = { Open: openSelected, [chartType === 'line' ? 'Implemented' : 'Closed']: secondSelected };
                delete context.window.CMF_PORTAL['lastHomeDashboardKey_' + hostId];
                for (const weeks of [6, 12, 6]) {
                    context.renderHomeTrendChart(rangeSnapshot, hostId, 'trendChart', weeks);
                    assert.equal(JSON.stringify(option.legend.selected), JSON.stringify(legendSelected), 'Week-range changes retain every legend selection combination.');
                    assert.equal(option.xAxis.data.length, weeks);
                    if (chartType === 'line') {
                        const contour = option.series.find(series => series.id === 'cmf-backlog-contour');
                        const expected = (openSelected ? 2 : 0) + (secondSelected ? 3 : 0);
                        assert.equal(contour.data.length, weeks);
                        assert.ok(contour.data.every(point => point.value === expected), 'The rebuilt contour reflects the preserved legend selections.');
                    }
                }
            }
        }
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
    assert.match(predicate[0], /string historicalDateColumn = implemented \? "date_cmf_ask" : "date_cmf_decided";/);
    assert.match(predicate[0], /prefix \+ historicalDateColumn \+ " AS DATE\) < \("/);
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
    assert.match(drilldown[0], /CmfSummaryTrendService\.BuildWeeklyOpenPredicate\("src", "@week_offset"\)/);
    assert.match(drilldown[0], /CmfSummaryTrendService\.BuildWeeklyClosedPredicate\("src", "@week_offset"\)/);
    assert.doesNotMatch(drilldown[0], /baseWhere\.Append\([^\r\n]*rejected/);
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
    assert.match(server, /case "Component":\s*if \(filter.Value.Contains\(","\)\)\s*AppendInOrEqualClause\(clauses, null, "LTRIM\(RTRIM\(" \+ prefix \+ "component_group\)\)", "@ComponentFilter", filter.Value\);\s*else\s*clauses\.Add\(" AND LTRIM\(RTRIM\(" \+ prefix \+ "component_group\)\) = @ComponentFilter "\)/);
});

test('both customer column headers use Customer Detail', () => {
    assert.match(page, /HeaderText="Customer Detail" ItemStyle-CssClass="field-customer_detail"/);
    assert.match(page, /<asp:TemplateField HeaderText="Customer Detail" ItemStyle-CssClass="pending-field-customer"/);
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
