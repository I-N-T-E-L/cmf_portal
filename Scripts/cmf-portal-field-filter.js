// Simple, reliable functions that work with ASP.NET UpdatePanel
function toggleFieldSelector() {
    var panel = document.getElementById('field-checkboxes');
    var button = document.getElementById('toggle-panel');

    if (panel && button) {
        if (panel.classList.contains('show')) {
            panel.classList.remove('show');
            button.innerHTML = 'Columns';
        } else {
            panel.classList.add('show');
            button.innerHTML = 'Hide columns';
        }
    }
}

function selectAllFields() {
    var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
    for (var i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = true;
    }
    updateSelectedInfo();
}

function deselectAllFields() {
    var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
    for (var i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = false;
    }
    updateSelectedInfo();
}

function applyFieldFilter() {
    var selectedFields = getSelectedFields();
    updateGridViewDisplay(selectedFields);
    savePreferences(selectedFields);
    updateSelectedInfo();

    // Save selected columns to server session
    saveSelectedColumnsToServer(selectedFields);

    // Show success message
    showMessage('Filter applied! Showing ' + selectedFields.length + ' columns.', 'success');

    // Hide the panel
    var panel = document.getElementById('field-checkboxes');
    var button = document.getElementById('toggle-panel');
    if (panel && button) {
        panel.classList.remove('show');
        button.innerHTML = 'Columns';
    }
}

function saveSelectedColumnsToServer(selectedFields) {
    // Use PageMethods to call server-side method
    if (typeof PageMethods !== 'undefined' && PageMethods.SaveSelectedColumns) {
        PageMethods.SaveSelectedColumns(selectedFields);
    } else {
        // Alternative: use AJAX call
        var xhr = new XMLHttpRequest();
        xhr.open('POST', window.location.pathname + '/SaveSelectedColumns', true);
        xhr.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
        xhr.send(JSON.stringify({ selectedColumns: selectedFields }));
    }
}

function resetToDefault() {
    // Reset to show all columns (same as selectAllFields)
    selectAllFields();
    applyFieldFilter();
    showMessage('Reset to show all columns!', 'info');
}

function getSelectedFields() {
    var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]:checked');
    var selectedFields = [];
    for (var i = 0; i < checkboxes.length; i++) {
        selectedFields.push(checkboxes[i].value);
    }
    return selectedFields;
}

function updateGridViewDisplay(selectedFields) {
    var allFieldClasses = [
        'field-sno',
        'field-milestone',
        'field-progress',
        'field-sightingid',
        'field-promotedid',
        'field-customer_detail',
        'field-imagefreeze',
        'field-duplicatedetails',
        'field-customer_company',
        'field-title',
        'field-component',
        'field-component_group',
        'field-owner',
        'field-promoted_owner',
        'field-rvp_repro',
        'field-status',
        'field-idst',
        'field-los',
        'field-processor',
        'field-impact',
        'field-days_open',
        'field-cmf_request',
        'field-closed_reason',
        'field-edit_column'
    ];

    var gridView = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
    if (!gridView) {
        console.log('GridView not found');
        return;
    }

    allFieldClasses.forEach(function(fieldClass) {
        var fieldName = fieldClass.replace('field-', '');
        var elements = gridView.querySelectorAll('.' + fieldClass);

        for (var i = 0; i < elements.length; i++) {
            var element = elements[i];
            if (selectedFields.indexOf(fieldName) !== -1) {
                element.classList.remove('hidden');
            } else {
                element.classList.add('hidden');
            }
        }
    });

    // Force table reflow
    var tables = gridView.querySelectorAll('table');
    for (var j = 0; j < tables.length; j++) {
        var table = tables[j];
        var originalDisplay = table.style.display;
        table.style.display = 'none';
        table.offsetHeight;
        table.style.display = originalDisplay || 'table';
    }
}

function updateSelectedInfo() {
    var selectedFields = getSelectedFields();
    var info = document.getElementById('selected-info');
    if (info) {
        if (selectedFields.length === 0) {
            info.textContent = 'No columns selected';
        }
    }
}

function savePreferences(selectedFields) {
    if (typeof(Storage) !== 'undefined') {
        localStorage.setItem('gridViewSelectedFields', JSON.stringify(selectedFields));
    }
}

function loadSavedPreferences() {
    var isPageLoad = !!window.CMF_PORTAL.isInitialLoad;

    if (isPageLoad) {
        var allFields = [
            'sno',
            'milestone',
            'progress',
            'sightingid',
            'promotedid',
            'customer_detail',
            'imagefreeze',
            'duplicatedetails',
            'customer_company',
            'title',
            'component',
            'component_group',
            'owner',
            'promoted_owner',
            'rvp_repro',
            'status',
            'idst',
            'los',
            'processor',
            'impact',
            'days_open',
            'cmf_request',
            'closed_reason',
            'edit_column'
        ];

        var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
        for (var i = 0; i < checkboxes.length; i++) {
            checkboxes[i].checked = true;
        }

        updateGridViewDisplay(allFields);
        updateSelectedInfo();
    } else if (typeof(Storage) !== 'undefined') {
        var saved = localStorage.getItem('gridViewSelectedFields');
        if (saved) {
            try {
                var selectedFields = JSON.parse(saved);
                var boxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
                for (var b = 0; b < boxes.length; b++) {
                    var checkbox = boxes[b];
                    checkbox.checked = selectedFields.indexOf(checkbox.value) !== -1;
                }
                updateGridViewDisplay(selectedFields);
                updateSelectedInfo();
            } catch (e) {
                var fallbackSelectedFields = getSelectedFields();
                updateGridViewDisplay(fallbackSelectedFields);
                updateSelectedInfo();
            }
        } else {
            var currentSelectedFields = getSelectedFields();
            updateGridViewDisplay(currentSelectedFields);
            updateSelectedInfo();
        }
    } else {
        var selectedFieldsNoStorage = getSelectedFields();
        updateGridViewDisplay(selectedFieldsNoStorage);
        updateSelectedInfo();
    }
}

function showMessage(message, type) {
    var existingMessages = document.querySelectorAll('.field-filter-message');
    for (var i = 0; i < existingMessages.length; i++) {
        existingMessages[i].remove();
    }

    var messageDiv = document.createElement('div');
    messageDiv.className = 'alert alert-' + type + ' field-filter-message';
    messageDiv.textContent = message;
    messageDiv.style.position = 'fixed';
    messageDiv.style.top = '20px';
    messageDiv.style.right = '20px';
    messageDiv.style.zIndex = '9999';
    messageDiv.style.padding = '12px 18px';
    messageDiv.style.borderRadius = '6px';
    messageDiv.style.fontSize = '14px';
    messageDiv.style.fontWeight = '500';
    messageDiv.style.backgroundColor = type === 'success' ? '#d4edda' : '#d1ecf1';
    messageDiv.style.color = type === 'success' ? '#155724' : '#0c5460';
    messageDiv.style.border = '1px solid ' + (type === 'success' ? '#c3e6cb' : '#bee5eb');
    messageDiv.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';

    document.body.appendChild(messageDiv);

    setTimeout(function() {
        if (messageDiv.parentNode) {
            messageDiv.remove();
        }
    }, 3000);
}

function initializeFieldFilter() {
    if (document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails)) {
        loadSavedPreferences();
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeFieldFilter);
} else {
    initializeFieldFilter();
}

function pageLoad() {
    initializeFieldFilter();
    initTableSorting();
}

function makeTableSortable(tableId) {
    var table = document.getElementById(tableId);
    if (!table) return;
    var thead = table.tHead;
    if (!thead) return;

    var headerRow = thead.rows[thead.rows.length - 1];
    for (var i = 0; i < headerRow.cells.length; i++) {
        (function(col) {
            var th = headerRow.cells[col];
            th.style.cursor = 'pointer';
            th.setAttribute('data-sort-dir', 'asc');
            th.addEventListener('click', function(e) {
                if (e.target.tagName === 'SELECT' || e.target.tagName === 'OPTION') return;
                var dir = this.getAttribute('data-sort-dir');
                sortTableByColumn(table, col, dir === 'asc');
                this.setAttribute('data-sort-dir', dir === 'asc' ? 'desc' : 'asc');
                for (var j = 0; j < headerRow.cells.length; j++) {
                    var sp = headerRow.cells[j].querySelector('.cmf-sort-ic');
                    if (sp) sp.textContent = '';
                }
                var indicator = this.querySelector('.cmf-sort-ic');
                if (!indicator) {
                    indicator = document.createElement('span');
                    indicator.className = 'cmf-sort-ic';
                    indicator.style.cssText = 'margin-left:4px;font-size:9px;';
                    this.appendChild(indicator);
                }
                indicator.textContent = dir === 'asc' ? ' ▲' : ' ▼';
            });
        })(i);
    }
}

function sortTableByColumn(table, colIndex, ascending) {
    var tbody = table.tBodies[0];
    if (!tbody) return;

    var rows = Array.prototype.slice.call(tbody.rows);
    rows.sort(function(a, b) {
        var ac = a.cells[colIndex];
        var bc = b.cells[colIndex];
        var av = ac ? (ac.textContent || ac.innerText || '').trim() : '';
        var bv = bc ? (bc.textContent || bc.innerText || '').trim() : '';

        var an = parseFloat(av.replace(/[^0-9.\-]/g, ''));
        var bn = parseFloat(bv.replace(/[^0-9.\-]/g, ''));
        if (!isNaN(an) && !isNaN(bn)) {
            return ascending ? an - bn : bn - an;
        }

        return ascending
            ? av.localeCompare(bv, undefined, { sensitivity: 'base' })
            : bv.localeCompare(av, undefined, { sensitivity: 'base' });
    });

    for (var i = 0; i < rows.length; i++) {
        tbody.appendChild(rows[i]);
    }
}

function initTableSorting() {
    var issueGrid = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
    if (issueGrid) makeTableSortable(issueGrid.id);

    var pendingGrid = document.getElementById(window.CMF_PORTAL.ids.gridViewCmfPending);
    if (pendingGrid) makeTableSortable(pendingGrid.id);

    var analyticsGrid = document.getElementById(window.CMF_PORTAL.ids.gridViewAnalyticsSummary);
    if (analyticsGrid) makeTableSortable(analyticsGrid.id);
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initTableSorting);
} else {
    initTableSorting();
}
