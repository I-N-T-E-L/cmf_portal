(function () {
    var active = null;
    function close(restoreFocus) {
        if (!active) return;
        var state = active;
        active = null;
        state.menu.hidden = true;
        state.parent.appendChild(state.menu);
        state.trigger.setAttribute('aria-expanded', 'false');
        if (restoreFocus && state.trigger.isConnected) state.trigger.focus();
    }
    window.toggleIssueRowActions = function (trigger) {
        if (active && active.trigger === trigger) { close(true); return; }
        close(false);
        var parent = trigger.closest('.issue-row-actions');
        var menu = parent.querySelector('.issue-row-actions-menu');
        active = { trigger: trigger, parent: parent, menu: menu };
        document.body.appendChild(menu);
        menu.hidden = false;
        var bounds = trigger.getBoundingClientRect();
        menu.style.left = Math.max(8, Math.min(bounds.right - menu.offsetWidth, window.innerWidth - menu.offsetWidth - 8)) + 'px';
        menu.style.top = (bounds.bottom + menu.offsetHeight + 8 < window.innerHeight ? bounds.bottom + 4 : Math.max(8, bounds.top - menu.offsetHeight - 4)) + 'px';
        trigger.setAttribute('aria-expanded', 'true');
        menu.querySelector('[role="menuitem"]').focus();
    };
    window.runIssueRowAiAction = function (mode) {
        if (!active) return;
        var row = active.trigger.closest('tr');
        close(false);
        var button = row.querySelector(mode === 'details' ? '.field-sightingid .issue-details-ai-btn' : '.field-status .issue-details-ai-btn');
        if (button) button.click();
    };
    document.addEventListener('click', function (event) {
        if (!active) return;
        if (active.menu.contains(event.target)) {
            if (event.target.closest('a')) close(false);
        } else if (!active.trigger.contains(event.target)) close(false);
    });
    document.addEventListener('keydown', function (event) {
        if (!active) return;
        if (event.key === 'Escape') { event.preventDefault(); close(true); return; }
        if (event.key === 'Tab') { close(false); return; }
        var items = Array.from(active.menu.querySelectorAll('[role="menuitem"]'));
        var index = items.indexOf(document.activeElement);
        var next = event.key === 'ArrowDown' ? (index + 1) % items.length : event.key === 'ArrowUp' ? (index + items.length - 1) % items.length : event.key === 'Home' ? 0 : event.key === 'End' ? items.length - 1 : -1;
        if (next >= 0) { event.preventDefault(); items[next].focus(); }
    });
    document.addEventListener('scroll', function (event) { if (active && !active.menu.contains(event.target)) close(false); }, true);
    window.addEventListener('resize', function () { close(false); });
    document.addEventListener('DOMContentLoaded', function () {
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function () { close(false); });
    });
}());

function toggleSidebar() {
    var sidebar = document.querySelector('.sidebar');
    if (!sidebar) return;
    sidebar.style.right = (sidebar.style.right === '0px' || sidebar.style.right === '0') ? '-250px' : '0px';
}

function submitLegacyPostback(eventTarget, eventArgument) {
    var form = document.forms && document.forms.length ? document.forms[0] : null;
    if (!form) {
        return false;
    }

    if (form.__EVENTTARGET) {
        form.__EVENTTARGET.value = eventTarget || '';
    }

    if (form.__EVENTARGUMENT) {
        form.__EVENTARGUMENT.value = eventArgument || '';
    }

    if (typeof form.onsubmit === 'function' && form.onsubmit() === false) {
        return false;
    }

    form.submit();
    return true;
}

function dispatchPopupAsyncFlow(action, arg1, arg2, arg3, fallbackEventTarget, fallbackEventArgument) {
    var ids = window.CMF_PORTAL && window.CMF_PORTAL.ids ? window.CMF_PORTAL.ids : null;
    var actionInput = ids ? document.getElementById(ids.hfPopupAsyncAction || '') : null;
    var arg1Input = ids ? document.getElementById(ids.hfPopupAsyncArg1 || '') : null;
    var arg2Input = ids ? document.getElementById(ids.hfPopupAsyncArg2 || '') : null;
    var arg3Input = ids ? document.getElementById(ids.hfPopupAsyncArg3 || '') : null;
    var dispatchTarget = ids ? ids.btnPopupAsyncDispatch : '';

    if (actionInput && arg1Input && arg2Input && arg3Input && dispatchTarget) {
        if (window.CMF_PORTAL) {
            window.CMF_PORTAL.isPopupAsyncDispatchInFlight = true;
        }
        actionInput.value = action || '';
        arg1Input.value = arg1 || '';
        arg2Input.value = arg2 || '';
        arg3Input.value = arg3 || '';

        var packedArgument = String(action || '')
            + '||' + encodeURIComponent(String(arg1 || ''))
            + '||' + encodeURIComponent(String(arg2 || ''))
            + '||' + encodeURIComponent(String(arg3 || ''));

        __doPostBack(dispatchTarget, packedArgument);
        return false;
    }

    // Compatibility fallback for cases where async dispatcher fields are unavailable.
    markPopupTablePostbackFlow();
    submitLegacyPostback(fallbackEventTarget || '', fallbackEventArgument || '');
    return false;
}

function isElementVisibleForPopupSnapshot(element) {
    if (!element) {
        return false;
    }

    var style = window.getComputedStyle ? window.getComputedStyle(element) : null;
    if (!style) {
        return true;
    }

    if (style.display === 'none' || style.visibility === 'hidden') {
        return false;
    }

    return element.offsetParent !== null || style.position === 'fixed';
}

function sanitizePopupSnapshotMarkup(html) {
    return String(html || '')
        .replace(/\sid="[^"]*"/gi, '')
        .replace(/\sfor="[^"]*"/gi, '');
}

function buildPopupFlowSnapshotHtml() {
    var header = document.querySelector('header.portal-app-header');
    var leftNav = document.getElementById('mainMenuSidebar');
    var mainWorkspace = document.querySelector('.portal-main-workspace');

    var preferredPanelIds = [
        'homeWelcomePanel',
        'homeCmfSummaryPanel',
        'issueTabShell',
        'pendingTabShell',
        'analyticsPanel',
        'configRulesPanel'
    ];

    var visiblePanel = null;
    for (var i = 0; i < preferredPanelIds.length; i++) {
        var candidate = document.getElementById(preferredPanelIds[i]);
        if (isElementVisibleForPopupSnapshot(candidate)) {
            visiblePanel = candidate;
            break;
        }
    }

    var shellHtml = '';
    if (leftNav || visiblePanel || mainWorkspace) {
        shellHtml += '<div class="portal-layout-shell">';
        if (leftNav) {
            shellHtml += leftNav.outerHTML;
        }

        shellHtml += '<div class="portal-main-workspace">';
        if (visiblePanel) {
            shellHtml += visiblePanel.outerHTML;
        } else if (mainWorkspace) {
            shellHtml += mainWorkspace.outerHTML;
        }
        shellHtml += '</div>';
        shellHtml += '</div>';
    }

    if (!shellHtml && !header) {
        return '';
    }

    var snapshotHtml = '<div class="cmf-popup-flow-snapshot-root">';
    if (header) {
        snapshotHtml += header.outerHTML;
    }
    snapshotHtml += '<div class="container-fluid page-body-wrapper">' + shellHtml + '</div>';
    snapshotHtml += '</div>';

    return sanitizePopupSnapshotMarkup(snapshotHtml);
}

function markPopupTablePostbackFlow() {
    try {
        if (window.sessionStorage) {
            window.sessionStorage.setItem('cmfPopupTableFlow', '1');
            window.sessionStorage.setItem('cmfPopupTableScrollY', String(window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0));

            var snapshotHtml = buildPopupFlowSnapshotHtml();
            if (snapshotHtml) {
                window.sessionStorage.setItem('cmfPopupTableSnapshotHtml', snapshotHtml);
            } else {
                window.sessionStorage.removeItem('cmfPopupTableSnapshotHtml');
            }
        }
    } catch (error) {
        // Ignore storage availability issues.
    }
}

function clearPopupTablePostbackFlowMask() {
    document.documentElement.classList.remove('cmf-popup-flow-mask');

    var frozenLayer = document.getElementById('cmfPopupFlowFrozenLayer');
    if (frozenLayer) {
        frozenLayer.classList.remove('active');
        frozenLayer.innerHTML = '';
    }

    try {
        if (window.sessionStorage) {
            window.sessionStorage.removeItem('cmfPopupTableFlow');
            window.sessionStorage.removeItem('cmfPopupTableScrollY');
            window.sessionStorage.removeItem('cmfPopupTableSnapshotHtml');
        }
    } catch (error) {
        // Ignore storage availability issues.
    }
}

function bindPopupTableFlowMaskLifecycle() {
    var flowArmed = false;
    var restoreScrollY = null;
    try {
        flowArmed = !!(window.sessionStorage && window.sessionStorage.getItem('cmfPopupTableFlow') === '1');
        if (flowArmed && window.sessionStorage) {
            var storedScrollY = window.sessionStorage.getItem('cmfPopupTableScrollY');
            if (storedScrollY !== null && storedScrollY !== '') {
                var parsedScrollY = parseInt(storedScrollY, 10);
                if (!isNaN(parsedScrollY) && parsedScrollY >= 0) {
                    restoreScrollY = parsedScrollY;
                }
            }
        }
    } catch (error) {
        flowArmed = false;
        restoreScrollY = null;
    }

    if (!flowArmed) {
        clearPopupTablePostbackFlowMask();
        return;
    }

    var cleared = false;
    var clearOnce = function () {
        if (cleared) {
            return;
        }
        cleared = true;

        if (restoreScrollY !== null) {
            window.requestAnimationFrame(function () {
                window.scrollTo(0, restoreScrollY);
            });
        }

        clearPopupTablePostbackFlowMask();
        if (window.jQuery) {
            window.jQuery(document).off('shown.bs.modal.cmfPopupFlowMask');
        }
    };

    if (window.jQuery) {
        var modalSelector = '#detailsModal,#detailsModal0,#detailsModal2,#detailsModal3,#detailsModalDesignIndicator,#detailsModalIngredientIndicator,#detailsModalOemIndicator,#detailsModalDrivers,#detailsModalDrivers1,#detailsModalSummaryMetrics';
        window.jQuery(document)
            .off('shown.bs.modal.cmfPopupFlowMask')
            .on('shown.bs.modal.cmfPopupFlowMask', modalSelector, clearOnce);
    }

    window.setTimeout(clearOnce, 5000);
}

$(document).ready(function () {
        if (!$.fn.select2) {
            return;
        }
            $('.searchable-dropdown').select2({
                placeholder: "Select an owner",
                allowClear: true
            });
        });

        function closeSidebar() {
            document.querySelector('.sidebar').style.right = '-250px';
        }

        function openExportPopup() {
            // Show the modal when export button is clicked
            $('#exportModal').modal('show');
        }

        function submitExport() {
            // Create an array to hold the selected values
            var selectedValues = [];

            // Loop through each checkbox in the Repeater and check if it's selected
            $('input[type="checkbox"]:checked').each(function () {
                // Get the actual value of the checkbox (from the "Value" attribute, which matches Eval)
                selectedValues.push($(this).val());
            });

            // Check if any values were selected
            if (selectedValues.length == 0) {
                alert('Please select at least one value.');
                return; // Don't proceed if no values are selected
            }

            // Store selected values in a hidden field as a comma-separated string
            document.getElementById(window.CMF_PORTAL.ids.hfSelectedValues).value = selectedValues.join(',');

            // Trigger postback to call a server-side function
            __doPostBack(window.CMF_PORTAL.ids.exportBtn, '');
        }

        function driverCollector() {
            // Create an array to hold the selected values
            var selectedValues = [];

            // ONLY select checkboxes with the milestone-checkbox class (excluding Select All)
            $('.milestone-checkbox:checked').not('#chkSelectAll').each(function () {
                // Get the actual value of the checkbox
                selectedValues.push($(this).val());
            });

            // Check the Select All checkbox state
            var selectAllCheckbox = document.getElementById('chkSelectAll');

            if (selectAllCheckbox && selectAllCheckbox.checked) {
                // If Select All is checked, set to "AllDrivers"
                document.getElementById(window.CMF_PORTAL.ids.driverCollectorhf).value = "AllDrivers";
            } else {
                // Check if any individual values were selected
                if (selectedValues.length == 0) {
                    alert('Please select at least one value.');
                    return; // Don't proceed if no values are selected
                }
                // Store selected individual values
                document.getElementById(window.CMF_PORTAL.ids.driverCollectorhf).value = selectedValues.join(',');
            }
        }

        function toggleSelectAll() {
            var selectAllCheckbox = document.getElementById('chkSelectAll');
            var individualCheckboxes = $('.milestone-checkbox').not('#chkSelectAll');

            if (selectAllCheckbox.checked) {
                // If Select All is checked, uncheck all individual checkboxes
                individualCheckboxes.prop('checked', false);
            }

            // Call driverCollector to update the hidden field
            driverCollector();
        }

        function toggleIndividualCheckbox() {
            var selectAllCheckbox = document.getElementById('chkSelectAll');

            // If any individual checkbox is checked, uncheck Select All
            if ($('.milestone-checkbox:checked').not('#chkSelectAll').length > 0) {
                selectAllCheckbox.checked = false;
            }

            // Call driverCollector to update the hidden field
            driverCollector();
        }

        


        function openPopup() {
            document.getElementById("importPopup").style.display = "block";
        }

        function closePopup() {
            document.getElementById("importPopup").style.display = "none";
        }

        // Function to toggle "Select All" checkbox
        function toggleSelectAll(selectAllCheckbox) {
            var checkboxes = document.querySelectorAll('.chkValue');
            checkboxes.forEach(function (checkbox) {
                checkbox.checked = selectAllCheckbox.checked;
            });
        }

        // Function to update the "Select All" checkbox state
        function updateSelectAllCheckbox() {
            var checkboxes = document.querySelectorAll('.chkValue');
            var selectAllCheckbox = document.getElementById('chkSelectAll');
            var allChecked = true;

            checkboxes.forEach(function (checkbox) {
                if (!checkbox.checked) {
                    allChecked = false;
                }
            });

            // If not all checkboxes are checked, uncheck the "Select All" checkbox
            selectAllCheckbox.checked = allChecked;
        }

        function showCMFIssues(component, driver, issueType) {
            var hfComponent = document.getElementById(window.CMF_PORTAL.ids.hiddenComponentName);
            var hfDriver = document.getElementById(window.CMF_PORTAL.ids.hiddenDriverName);
            var hfIssueType = document.getElementById(window.CMF_PORTAL.ids.hiddenIssueType);

            if (hfComponent) hfComponent.value = component || '';
            if (hfDriver) hfDriver.value = driver || '';
            if (hfIssueType) hfIssueType.value = issueType || '';

                    return dispatchPopupAsyncFlow(
                        'ShowCMFIssues',
                        component || '',
                        driver || '',
                        issueType || '',
                        'ShowCMFIssues',
                        (component || '') + '|' + (driver || '') + '|' + (issueType || '')
                    );
        }

        function getComponentValue() {
            // Get from hidden field if it exists
            var hfComponent = document.getElementById(window.CMF_PORTAL.ids.hiddenComponentName);
            if (hfComponent && hfComponent.value) {
                return hfComponent.value;
            }

            // Or get from session/viewstate - you'll need to determine the source
            return window.CMF_PORTAL.defaults.componentName || '';
        }

        function getIssueTypeValue() {
            // Get from hidden field if it exists
            var hfIssueType = document.getElementById(window.CMF_PORTAL.ids.hiddenIssueType);
            if (hfIssueType && hfIssueType.value) {
                return hfIssueType.value;
            }

            // Or determine based on your business logic
            return window.CMF_PORTAL.defaults.issueType || 'Open';
        }

        function getDriverValue() {
            // Get from hidden field if it exists
            var hfDriver = document.getElementById(window.CMF_PORTAL.ids.hiddenDriverName);
            if (hfDriver && hfDriver.value) {
                return hfDriver.value;
            }

            // Or get from session/viewstate - you'll need to determine the source
            return window.CMF_PORTAL.defaults.driverName || '';
        }

        function exportCMFIssues(component, driver, issueType) {
            var hfComponent = document.getElementById(window.CMF_PORTAL.ids.hiddenComponentName);
    var hfDriver = document.getElementById(window.CMF_PORTAL.ids.hiddenDriverName);
    var hfIssueType = document.getElementById(window.CMF_PORTAL.ids.hiddenIssueType);

            if (hfComponent) hfComponent.value = component || '';
            if (hfDriver) hfDriver.value = driver || '';
            if (hfIssueType) hfIssueType.value = issueType || '';

            __doPostBack('ExportCMFIssues', component + '|' + driver + '|' + issueType);

            setTimeout(function () {
                var f = document.forms[0];
                if (f && f.__EVENTTARGET) f.__EVENTTARGET.value = '';
                if (f && f.__EVENTARGUMENT) f.__EVENTARGUMENT.value = '';
            }, 0);

            return false;
        }


        function adjustGridHeights() {
            // Get the height of the gridviews on the right side (GridView_cmf_summary and GridView_comp)
            var gridViewComp = document.getElementById('GridView_comp');
            var gridViewcmf_summary = document.getElementById('GridView_cmf_summary');

            // Get the height of the gridviews on the left side (GridView_cmf_summary1 and GridView_notes)
            var gridViewcmf_summary1 = document.getElementById('GridView_cmf_summary1');
            var gridViewNotes = document.getElementById('GridView_notes');

            // Get the total height of the left-side gridviews (without the gap first)
            var leftHeight = gridViewcmf_summary1.offsetHeight + gridViewNotes.offsetHeight;

            // Get the total height of the right-side gridviews (without the gap first)
            var rightHeight = gridViewcmf_summary.offsetHeight + gridViewComp.offsetHeight;

            // Calculate a dynamic gap as 2% of the total height (a visual balance)
            var gap = 0.02 * Math.max(leftHeight, rightHeight);  // 2% gap, depending on the larger height

            // Adjust total heights by adding the calculated gap
            leftHeight += gap;
            rightHeight += gap;

            // If the left and right heights are not equal, adjust accordingly
            if (leftHeight < rightHeight) {
                // If left side is shorter, distribute the remaining height equally between the two left-side gridviews
                var remainingHeight = rightHeight - leftHeight;
                var extraHeight = remainingHeight / 2;

                // Apply the extra height to both left gridviews (GridView_cmf_summary1 and GridView_notes)
                gridViewcmf_summary1.style.height = (gridViewcmf_summary1.offsetHeight + (extraHeight + 0.5)) + 'px';
                gridViewNotes.style.height = (gridViewNotes.offsetHeight + (extraHeight + 0.5)) + 'px';
            } else if (leftHeight > rightHeight) {
                // If left side is taller, adjust the right-side gridviews to match the total height of the left side
                var heightDifference = leftHeight - rightHeight;
                gridViewcmf_summary.style.height = (gridViewcmf_summary.offsetHeight + heightDifference) + 'px';
            }
            // If the heights are already equal, no adjustment is needed
            else {
                // No action needed if the heights are equal
                gridViewcmf_summary.style.height = gridViewcmf_summary.offsetHeight + 'px';
                gridViewComp.style.height = gridViewComp.offsetHeight + 'px';
                gridViewcmf_summary1.style.height = gridViewcmf_summary1.offsetHeight + 'px';
                gridViewNotes.style.height = gridViewNotes.offsetHeight + 'px';
            }
        }

        function changeFirstColumnColors() {
            // Get the gridview element
            var gridView = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);

            // Get all rows in the grid (skipping the header row if it exists)
            var rows = gridView.getElementsByTagName('tr');

            // Loop through each row (starting from the second row if header exists)
            for (var i = 1; i < rows.length; i++) {
                // Get the first cell (td) in the current row
                var firstCell = rows[i].cells[0];

                // Get the value of the first cell
                var cellValue = firstCell.textContent || firstCell.innerText;

                // Set the background color and text color to the value of the cell
                if (cellValue) {
                    firstCell.style.backgroundColor = cellValue;
                    firstCell.style.color = cellValue;
                }
            }
        }

        function moveUnassignedRowsToBottom() {
            // Get both GridViews
            const gridView1 = document.getElementById("GridView_design_summary");
            const gridView2 = document.getElementById("GridView_cmf_pending");

            // Function to move rows in any gridview
            function moveRows(gridView) {
                // Get all rows in the GridView (excluding header)
                const rows = Array.from(gridView.getElementsByTagName("tr")).slice(1);

                // Filter rows where the first column has the value "Unassigned"
                const unassignedRows = rows.filter(row => row.cells[0]?.innerText.trim() === "unassigned");
                const assignedRows = rows.filter(row => row.cells[0]?.innerText.trim() !== "unassigned");

                // Append unassigned rows to the bottom
                unassignedRows.forEach(row => gridView.appendChild(row));

                // Reorder rows in the GridView
                assignedRows.forEach(row => gridView.appendChild(row));
            }

            // Move rows in both GridViews
            moveRows(gridView1);
            moveRows(gridView2);
        }

        function moveUnassignedRowsToBottom() {
            const gridView1 = document.getElementById(window.CMF_PORTAL.ids.gridViewDesignSummary);
            const gridView2 = document.getElementById(window.CMF_PORTAL.ids.gridViewComponentSummary);

            if (!gridView1 && !gridView2) return;

            // Function to move unassigned rows to the bottom
            function processGridView(gridView) {
                const rows = Array.from(gridView.getElementsByTagName('tr'));
                const header = rows[0]; // Header row
                const dataRows = rows.slice(1); // Skip header

                const assignedRows = [];
                const unassignedRows = [];

                dataRows.forEach(row => {
                    const designCell = row.cells[0]; // Design is the first column
                    const designValue = designCell.textContent.trim().toLowerCase();
                    console.log("Design Value: ", designValue);

                    if (designValue === "unassigned") {
                        unassignedRows.push(row);
                    } else {
                        assignedRows.push(row);
                    }
                });

                // Clear existing rows (excluding header)
                while (gridView.rows.length > 1) {
                    gridView.deleteRow(1);
                }

                // Append rows in order (assigned first, then unassigned)
                assignedRows.forEach(row => gridView.appendChild(row));
                unassignedRows.forEach(row => gridView.appendChild(row));
            }

            // Process both GridViews
            if (gridView1) processGridView(gridView1);
            if (gridView2) processGridView(gridView2);
        }

        window.onload = function () {
            var gridView = document.getElementById(window.CMF_PORTAL.ids.gridViewCmfSummary);
    var gridView_comp = document.getElementById(window.CMF_PORTAL.ids.gridViewComp);

            if (gridView) {

                var rows = gridView.rows;
                var lastDataRowIndex = rows.length - 1;
                if (lastDataRowIndex >= 0) {
                    var lastRow = rows[lastDataRowIndex];
                    if (lastRow) {
                        lastRow.style.backgroundColor = "orange";
                        lastRow.style.color = "black";
                    }
                }
                //createMergedHeaders(gridView);
                //formatTotalRow(gridView);
            }

            if (gridView_comp) {
                var rows = gridView_comp.rows;
                var lastDataRowIndex = rows.length - 1;
                if (lastDataRowIndex >= 0) {
                    var lastRow = rows[lastDataRowIndex];
                    if (lastRow) {
                        lastRow.style.backgroundColor = "orange";
                        lastRow.style.color = "black";
                    }
                }
            }
        }

        function createMergedHeaders(gridView) {
            var headerRow = gridView.rows[0];
            if (!headerRow) return;

            // Use the public property instead
            var drivers = window.CMF_PORTAL.driversJson || [];

    // Create new header row for driver names
    var newHeaderRow = gridView.insertRow(0);
            newHeaderRow.style.backgroundColor = "#0056b3"; // Match css1 first row color
    newHeaderRow.style.color = "black"; // Changed to black to match css1
    newHeaderRow.style.fontWeight = "bold";
    newHeaderRow.style.borderBottom = "solid 2px black";

    // Component header cell
    var componentCell = newHeaderRow.insertCell(0);
    componentCell.innerHTML = "Component";
    componentCell.rowSpan = 2;
    componentCell.style.textAlign = "left"; // Changed to left to match css1
    componentCell.style.verticalAlign = "middle";
    componentCell.style.border = "1px solid #ddd"; // Match css1 border
    componentCell.style.borderLeft = "solid 2px black";
    componentCell.style.padding = "10px";
            componentCell.style.backgroundColor = "#0056b3";
    componentCell.style.color = "white";

    // Driver header cells (merged)
    for (var i = 0; i < drivers.length; i++) {
        var driverCell = newHeaderRow.insertCell(-1);
        driverCell.innerHTML = drivers[i];
        driverCell.colSpan = 2;
        driverCell.style.textAlign = "left"; // Changed to left to match css1
        driverCell.style.border = "1px solid #ddd"; // Match css1 border
        driverCell.style.borderLeft = "solid 2px black";
        driverCell.style.borderBottom = "solid 2px black";
        driverCell.style.padding = "10px";
        driverCell.style.backgroundColor = "#0056b3"; // Match css1 color
        driverCell.style.color = "white"; // Changed to black
        driverCell.style.fontWeight = "bold";
    }

    // Update original header row (now second row)
    headerRow.deleteCell(0); // Remove component cell since it's merged

    // Style the sub-headers to match css1
            headerRow.style.backgroundColor = "#0056b3";
    headerRow.style.borderBottom = "solid 2px black";

    for (var i = 0; i < headerRow.cells.length; i++) {
        headerRow.cells[i].style.backgroundColor = "#0056b3"; // Match css1 header color
        headerRow.cells[i].style.color = "white"; // Changed to black
        headerRow.cells[i].style.textAlign = "left"; // Changed to left to match css1
        headerRow.cells[i].style.padding = "10px"; // Match css1 padding
        headerRow.cells[i].style.border = "1px solid #ddd"; // Match css1 border
        headerRow.cells[i].style.borderLeft = "solid 2px black";
        headerRow.cells[i].style.fontWeight = "bold";
    }
}


function formatTotalRow(gridView) {
    var drivers = window.CMF_PORTAL.driversJson || [];
    var rows = gridView.rows;
    var totalRowIndex = -1;

    // Find the total row
    for (var i = 0; i < rows.length; i++) {
        if (rows[i].cells[0] && (rows[i].cells[0].innerText.includes("Total") || rows[i].cells[0].textContent.includes("Total"))) {
            totalRowIndex = i;
            break;
        }
    }

    if (totalRowIndex >= 0) {
        var totalRow = rows[totalRowIndex];

        // Style the total row
        totalRow.style.backgroundColor = "orange";
        totalRow.style.color = "black";
        totalRow.style.fontWeight = "bold";

        // Update the first cell text
        totalRow.cells[0].innerText = "Total";

        // Merge the driver columns for total row
        for (var i = 0; i < drivers.length; i++) {
            var openCellIndex = 1 + (i * 2);
            var implCellIndex = 2 + (i * 2);

            if (openCellIndex < totalRow.cells.length && implCellIndex < totalRow.cells.length) {
                var openValue = totalRow.cells[openCellIndex].innerText || totalRow.cells[openCellIndex].textContent;
                var implValue = totalRow.cells[implCellIndex].innerText || totalRow.cells[implCellIndex].textContent;

                // Create merged content
                var mergedContent = openValue + " + " + implValue;

                // Update the first cell with merged content and hide the second
                totalRow.cells[openCellIndex].innerHTML = mergedContent;
                totalRow.cells[openCellIndex].colSpan = 2;
                totalRow.cells[openCellIndex].style.textAlign = "center";

                // Hide the second cell
                totalRow.cells[implCellIndex].style.display = "none";
            }
        }
    }
}

        function pad(num) {
            return num.toString().padStart(2, '0');
        }

        function formatIST(date) {
            const istOffset = 330;
            const utc = date.getTime() + (date.getTimezoneOffset() * 60000);
            const istTime = new Date(utc + (istOffset * 60000));

            const day = pad(istTime.getDate());
            const month = pad(istTime.getMonth() + 1);
            const year = istTime.getFullYear();
            const hours = pad(istTime.getHours());
            const minutes = pad(istTime.getMinutes());

            return `${day}/${month}/${year} ${hours}:${minutes} IST`;
        }

        function updateTimestamps() {
            const now = new Date();
            const lastUpdate = new Date(now);
            const minutes = lastUpdate.getMinutes();

            lastUpdate.setMinutes(minutes < 30 ? 0 : 30, 0, 0);
            const nextUpdate = new Date(lastUpdate);
            nextUpdate.setMinutes(lastUpdate.getMinutes() + 30);

            document.getElementById('last-update').innerText = `Last Updated: ${formatIST(lastUpdate)}`;
            document.getElementById('next-update').innerText = `Next Update: ${formatIST(nextUpdate)}`;
        }

        // Wait for full DOM load to avoid ASP.NET conflicts
        window.addEventListener('load', function () {
            updateTimestamps();
            setInterval(updateTimestamps, 60 * 1000);
        });

        function filterGrid() {
            const grid = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
          const rows = grid.getElementsByTagName('tr');

            // Get filter values from the input fields
            const filter0 = document.getElementById('searchColumn0').value.toLowerCase();
          const filter1 = document.getElementById('searchColumn5').value.toLowerCase();
          const filter2 = document.getElementById('searchColumn10').value.toLowerCase();
          const filter3 = document.getElementById('searchColumn4').value.toLowerCase();
          const filter4 = document.getElementById('searchColumn6').value.toLowerCase();

          console.log("Filter values:", { filter1, filter2, filter3, filter4 });
          console.log("Total rows:", rows.length);

          // Loop through rows and apply the filter
          for (let i = 1; i < rows.length; i++) {
              const cells = rows[i].getElementsByTagName('td');

              if (cells.length === 0) continue;  // Skip header rows

              // Get column values (make sure these are the correct column indexes)
              const progressCell = cells[0];
              const driverCell = cells[5];  // Column for "Driver"
              const idstCell = cells[10];    // Column for "iDST"
              const componentCell = cells[4]; // Column for "Component"
              const rvpCell = cells[6];      // Column for "RVP Repro"

              // Get text content for each cell (trim and convert to lowercase)
              const progressText = progressCell.textContent.trim().toLowerCase();
              const driverText = driverCell.textContent.trim().toLowerCase();
              const idstText = idstCell.textContent.trim().toLowerCase();
              const componentText = componentCell.textContent.trim().toLowerCase();
              const rvpText = rvpCell.textContent.trim().toLowerCase();

              // Check if each cell matches its corresponding filter
              const matchProgress = progressText.includes(filter0);
              const matchDriver = driverText.includes(filter1);
              const matchIDST = idstText.includes(filter2);
              const matchComponent = componentText.includes(filter3);
              const matchRVP = rvpText.includes(filter4);

              // Show or hide row based on filter matches
              const showRow = matchProgress && matchDriver && matchIDST && matchComponent && matchRVP;
              rows[i].style.display = showRow ? '' : 'none';
          }
      }

        //function showDetailsModal(design) {

        //    // For now just show dummy content
        //    document.getElementById('modalContent').innerHTML = "Dummy modal content for Design: " + design;
        //    $('#detailsModal').modal('show');

        //}

        function showTotalDriverIssuesModal(design) {
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;
    __doPostBack('ShowTotalDriverIssuesModal', design);
}

function showTotalImplementedVerifiedModal(design) {
    document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;
    __doPostBack('ShowTotalImplementedVerifiedModal', design);
}

        function showDriverDetailsModal(design, driver) {
            // Store design and driver in hidden fields so server can access them
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;
                document.getElementById(window.CMF_PORTAL.ids.hiddenModalDriver).value = driver;

                return dispatchPopupAsyncFlow(
                    'ShowDriverModal',
                    design || '',
                    driver || '',
                    '',
                    'ShowDriverModal',
                    String(design || '') + ',' + String(driver || '')
                );
        }

        function showImplementedVerifiedDetailsModal(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

                return dispatchPopupAsyncFlow(
                    'ShowImplementedVerifiedModal',
                    design || '',
                    '',
                    '',
                    'ShowImplementedVerifiedModal',
                    design || ''
                );
            }

        

        function showDetailsModal(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

            return dispatchPopupAsyncFlow(
                'ShowModal',
                design || '',
                '',
                '',
                'ShowModal',
                design || ''
            );
        }

        function showDesignIndicatorSummaryModal() {
            return dispatchPopupAsyncFlow('ShowDesignIndicatorSummaryModal', '', '', '', 'ShowDesignIndicatorSummaryModal', '');
        }

        function showIngredientIndicatorSummaryModal() {
            return dispatchPopupAsyncFlow('ShowIngredientIndicatorSummaryModal', '', '', '', 'ShowIngredientIndicatorSummaryModal', '');
        }

        function showOemIndicatorSummaryModal() {
            return dispatchPopupAsyncFlow('ShowOemIndicatorSummaryModal', '', '', '', 'ShowOemIndicatorSummaryModal', '');
        }

        function showDetailsModal2(ingred) {
            // Store design in a hidden field so seriver can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = ingred;

            return dispatchPopupAsyncFlow(
                'ShowModal2',
                ingred || '',
                '',
                '',
                'ShowModal2',
                ingred || ''
            );
        }
            function showDriverIssues(milestoneDriver) {
          var hf = document.getElementById(window.CMF_PORTAL.ids.hiddenDriverName);
          if (hf) hf.value = milestoneDriver || '';
          return dispatchPopupAsyncFlow(
              'ShowDriverIssues',
              milestoneDriver || '',
              '',
              '',
              'ShowDriverIssues',
              milestoneDriver || ''
          );
      }

      function exportDriverIssues(milestoneDriver) {
          var hf = document.getElementById(window.CMF_PORTAL.ids.hiddenDriverName);
                if (hf) hf.value = milestoneDriver || '';
          __doPostBack('ExportDriverIssues', milestoneDriver || '');


          // Clear sticky __EVENT* so next click doesn't repeat the export
          setTimeout(function () {
              var f = document.forms[0];
              if (f && f.__EVENTTARGET) f.__EVENTTARGET.value = '';
              if (f && f.__EVENTARGUMENT) f.__EVENTARGUMENT.value = '';
          }, 0);

          return false;

      }


        function showDetailsModal3(oem) {
            // Store design in a hidden field so seriver can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = oem;

            return dispatchPopupAsyncFlow(
                'ShowModal3',
                oem || '',
                '',
                '',
                'ShowModal3',
                oem || ''
            );
        }

        function exportToExcel(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

            // Trigger postback manually for exporting to Excel
            __doPostBack('ExportToExcel', design);
        }

        function exportToExcel2(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

            // Trigger postback manually for exporting to Excel
            __doPostBack('ExportToExcel2', design);
        }

        function exportToExcel3(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

            // Trigger postback manually for exporting to Excel
            __doPostBack('ExportToExcel3', design);
        }

        function exportGridViewTableToExcel(gridId, filePrefix) {
            var table = gridId ? document.getElementById(gridId) : null;
            if (!table) {
                return false;
            }

            var html = '<html><head><meta charset="utf-8"></head><body>' + table.outerHTML + '</body></html>';
            var blob = new Blob(['\ufeff', html], { type: 'application/vnd.ms-excel' });
            var url = URL.createObjectURL(blob);
            var link = document.createElement('a');

            var stamp = new Date();
            var safePrefix = String(filePrefix || 'Export').replace(/\s+/g, '_');
            var fileName = safePrefix + '_'
                + stamp.getFullYear()
                + String(stamp.getMonth() + 1).padStart(2, '0')
                + String(stamp.getDate()).padStart(2, '0')
                + '_'
                + String(stamp.getHours()).padStart(2, '0')
                + String(stamp.getMinutes()).padStart(2, '0')
                + String(stamp.getSeconds()).padStart(2, '0')
                + '.xls';

            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
            return false;
        }

        function exportDesignDetailsModal0ToExcel() {
            var gridId = window.CMF_PORTAL && window.CMF_PORTAL.ids
                ? window.CMF_PORTAL.ids.gridViewDesignSummaryModal0
                : '';
            return exportGridViewTableToExcel(gridId, 'DesignSummary_Details');
        }

        function exportSummaryMetricIssuesToExcel() {
            var gridId = window.CMF_PORTAL && window.CMF_PORTAL.ids
                ? window.CMF_PORTAL.ids.gridViewSummaryMetricIssues
                : '';
            return exportGridViewTableToExcel(gridId, 'SummaryMetric_IssueList');
        }

        function addSummaryRowToGridView(gridViewId) {
            const grid = document.getElementById(gridViewId);
            if (!grid) {
                console.error("GridView not found with ID:", gridViewId);
                return;
            }

            const rows = grid.getElementsByTagName("tr");
            if (rows.length < 2) return;

            const columnSums = [];
            const headerCells = rows[0].cells;
            const numColumns = headerCells.length;

            // Iterate through data rows only (skip header and footer if any)
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];

                // Skip footer or already-inserted summary rows
                if (row.className.includes("GridViewFooter") || row.getAttribute("data-summary")) {
                    continue;
                }

                const cells = row.cells;
                for (let j = 0; j < numColumns; j++) {
                    const text = cells[j]?.innerText?.trim() || '';
                    let value = parseFloat(text);

                    // Handle format like "3.8% (1/26)"
                    if (isNaN(value)) {
                        const match = text.match(/\((\d+(?:\.\d+)?)\/\d+\)/);
                        if (match) {
                            value = parseFloat(match[1]);
                        } else {
                            value = 0;
                        }
                    }

                    columnSums[j] = (columnSums[j] || 0) + value;
                }
            }

            // Create summary row
            const summaryRow = grid.insertRow(-1);
            summaryRow.setAttribute("data-summary", "true"); // Tag it so we can skip later if re-running
            for (let j = 0; j < numColumns; j++) {
                const cell = summaryRow.insertCell(j);
                if (j === 0) {
                    cell.innerText = "Total";
                    cell.style.fontWeight = "bold";
                } else {
                    const sum = columnSums[j] || 0;
                    cell.innerText = sum.toFixed(2);
                }
            }

            summaryRow.style.backgroundColor = "#eef0f4";
        }

        function showToast(message) {
            const toastEl = document.getElementById('errorToast');
            const toastBody = document.getElementById('toastMessage');

            toastBody.textContent = message;

            var toast = new bootstrap.Toast(toastEl, {
                delay: 7000 // Show toast for 8 seconds
            });
            toast.show();
        }

        function getModalTableEntryCount(modalElement) {
            if (!modalElement) {
                return 0;
            }

            var table = modalElement.querySelector('.gridview-container table');
            if (!table) {
                return 0;
            }

            var rows = Array.prototype.slice.call(table.querySelectorAll('tr'));
            var count = 0;

            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                var cells = row.querySelectorAll('td');
                if (!cells || cells.length === 0) {
                    continue;
                }

                // GridView "no data" rows are typically a single merged cell.
                if (cells.length === 1) {
                    var message = String(cells[0].textContent || '').trim().toLowerCase();
                    if (message.indexOf('no ') === 0 || message.indexOf('there are no') === 0) {
                        continue;
                    }
                }

                count += 1;
            }

            return count;
        }

        function updateModalTitleWithEntryCount(modalElement) {
            if (!modalElement) {
                return;
            }

            var title = modalElement.querySelector('.modal-title-container .modal-title');
            if (!title) {
                return;
            }

            var currentTitle = String(title.getAttribute('data-base-title') || '').trim();
            if (!currentTitle) {
                currentTitle = String(title.textContent || '').replace(/\s*\(\d+\s+entr(?:y|ies)\)\s*$/i, '').trim();
                title.setAttribute('data-base-title', currentTitle);
            }

            var count = getModalTableEntryCount(modalElement);
            var suffix = count === 1 ? ' (1 entry)' : ' (' + String(count) + ' entries)';
            title.textContent = currentTitle + suffix;
        }

        var cmfModalStackState = {
            pendingRestore: false,
            idsToRestore: []
        };
        var CMF_MODAL_STACK_STORAGE_KEY = 'cmf.modalStack.restore.v1';

        function persistModalStackForReload(ids) {
            try {
                if (!window.sessionStorage) {
                    return;
                }
                window.sessionStorage.setItem(CMF_MODAL_STACK_STORAGE_KEY, JSON.stringify(ids || []));
            } catch (error) {
                // Ignore storage errors and keep runtime behavior.
            }
        }

        function readPersistedModalStack() {
            try {
                if (!window.sessionStorage) {
                    return [];
                }

                var raw = window.sessionStorage.getItem(CMF_MODAL_STACK_STORAGE_KEY);
                if (!raw) {
                    return [];
                }

                var parsed = JSON.parse(raw);
                if (!Array.isArray(parsed)) {
                    return [];
                }

                return parsed
                    .filter(function (id) { return typeof id === 'string' && id.length > 0; });
            } catch (error) {
                return [];
            }
        }

        function clearPersistedModalStack() {
            try {
                if (!window.sessionStorage) {
                    return;
                }
                window.sessionStorage.removeItem(CMF_MODAL_STACK_STORAGE_KEY);
            } catch (error) {
                // Ignore storage errors.
            }
        }

        function getVisibleBootstrapModals() {
            return Array.prototype.slice.call(document.querySelectorAll('.modal.show'));
        }

        function getDashboardModalIds() {
            return [
                'detailsModalIngredientIndicator',
                'detailsModalDesignIndicator',
                'detailsModalOemIndicator',
                'detailsModal',
                'detailsModal2',
                'detailsModal0',
                'detailsModal3',
                'detailsModalDrivers',
                'detailsModalDrivers1',
                'detailsModalSummaryMetrics'
            ];
        }

        function hoistDashboardModalsToRoot() {
            var host = document.getElementById('form1') || document.body;
            if (!host) {
                return;
            }

            var ids = getDashboardModalIds();
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i];
                var matches = document.querySelectorAll('[id="' + id + '"]');
                if (!matches || !matches.length) {
                    continue;
                }

                if (matches.length === 1) {
                    var singleModal = matches[0];
                    if (singleModal.parentNode !== host) {
                        host.appendChild(singleModal);
                    }
                    continue;
                }

                var hoistedModal = null;
                var freshModal = null;
                for (var m = 0; m < matches.length; m++) {
                    if (matches[m].parentNode === host && !hoistedModal) {
                        hoistedModal = matches[m];
                    } else {
                        freshModal = matches[m];
                    }
                }

                if (!hoistedModal) {
                    hoistedModal = matches[0];
                    freshModal = matches[matches.length - 1];
                }

                if (freshModal && freshModal !== hoistedModal) {
                    hoistedModal.innerHTML = freshModal.innerHTML;
                    for (var k = 0; k < matches.length; k++) {
                        if (matches[k] !== hoistedModal && matches[k].parentNode) {
                            matches[k].parentNode.removeChild(matches[k]);
                        }
                    }
                    if (typeof updateModalTitleWithEntryCount === 'function') {
                        updateModalTitleWithEntryCount(hoistedModal);
                    }
                }

                if (hoistedModal.parentNode !== host) {
                    host.appendChild(hoistedModal);
                }
            }
        }

        window.hoistDashboardModalsToRoot = hoistDashboardModalsToRoot;

        function applyBootstrapModalStacking() {
            var modals = getVisibleBootstrapModals();

            if (modals.length > 0 && document.querySelectorAll('.modal-backdrop').length === 0) {
                var fallbackBackdrop = document.createElement('div');
                fallbackBackdrop.className = 'modal-backdrop fade show';
                document.body.appendChild(fallbackBackdrop);
            }

            for (var i = 0; i < modals.length; i++) {
                modals[i].style.zIndex = String(1050 + (i * 20));
            }

            var backdrops = Array.prototype.slice.call(document.querySelectorAll('.modal-backdrop'));
            for (var j = 0; j < backdrops.length; j++) {
                if (!backdrops[j].classList.contains('show')) {
                    backdrops[j].classList.add('show');
                }
                backdrops[j].style.zIndex = String(1040 + (j * 20));
            }

            if (modals.length > 0) {
                document.body.classList.add('modal-open');
                document.body.classList.add('cmf-modal-blur-active');
            } else {
                document.body.classList.remove('modal-open');
                document.body.classList.remove('cmf-modal-blur-active');
                // Guardrail: remove stale backdrops left by interrupted modal flows.
                for (var k = 0; k < backdrops.length; k++) {
                    if (backdrops[k] && backdrops[k].parentNode) {
                        backdrops[k].parentNode.removeChild(backdrops[k]);
                    }
                }
            }
        }

        function fallbackHideModalElement(modalEl) {
            if (!modalEl) {
                return;
            }

            if (window.jQuery && typeof window.jQuery(modalEl).modal === 'function') {
                window.jQuery(modalEl).modal('hide');
                return;
            }

            modalEl.classList.remove('show');
            modalEl.style.display = 'none';
            modalEl.setAttribute('aria-hidden', 'true');

            var backdrops = Array.prototype.slice.call(document.querySelectorAll('.modal-backdrop'));
            for (var i = 0; i < backdrops.length; i++) {
                if (backdrops[i] && backdrops[i].parentNode) {
                    backdrops[i].parentNode.removeChild(backdrops[i]);
                }
            }

            document.body.classList.remove('modal-open');
            document.body.classList.remove('cmf-modal-blur-active');
        }

        function bindModalDismissFallbackHandlers() {
            if (!window.jQuery) {
                return;
            }

            var $document = window.jQuery(document);
            var selector = '.modal [data-dismiss="modal"], .modal [data-bs-dismiss="modal"], .modal .close';

            $document.off('click.cmfModalDismissFallback', selector);
            $document.on('click.cmfModalDismissFallback', selector, function (event) {
                var modalEl = this.closest ? this.closest('.modal') : null;
                if (!modalEl) {
                    return;
                }

                event.preventDefault();
                event.stopPropagation();
                fallbackHideModalElement(modalEl);

                setTimeout(function () {
                    applyBootstrapModalStacking();
                }, 0);
            });
        }

        function wireModalCloseButtonsDirect() {
            var buttons = document.querySelectorAll('.modal .close, .modal [data-dismiss="modal"], .modal [data-bs-dismiss="modal"]');
            for (var i = 0; i < buttons.length; i++) {
                var button = buttons[i];
                if (!button || button.getAttribute('data-cmf-dismiss-bound') === '1') {
                    continue;
                }

                button.setAttribute('data-cmf-dismiss-bound', '1');
                button.addEventListener('click', function (event) {
                    var modalEl = this.closest ? this.closest('.modal') : null;
                    if (!modalEl) {
                        return;
                    }

                    event.preventDefault();
                    event.stopPropagation();
                    fallbackHideModalElement(modalEl);

                    setTimeout(function () {
                        applyBootstrapModalStacking();
                    }, 0);
                });
            }
        }

        function bindModalEscapeAndBackdropFallbacks() {
            if (window.__cmfModalEscapeBackdropBound) {
                return;
            }

            document.addEventListener('keydown', function (event) {
                if ((event.key || '').toLowerCase() !== 'escape') {
                    return;
                }

                var openModals = getVisibleBootstrapModals();
                if (!openModals.length) {
                    return;
                }

                fallbackHideModalElement(openModals[openModals.length - 1]);
                applyBootstrapModalStacking();
            });

            document.addEventListener('click', function (event) {
                var target = event.target;
                if (!target) {
                    return;
                }

                var modalEl = null;
                if (target.classList && target.classList.contains('modal') && target.classList.contains('show')) {
                    modalEl = target;
                } else if (target.closest) {
                    modalEl = target.closest('.modal.show');
                }

                if (!modalEl) {
                    return;
                }

                var dialog = modalEl.querySelector('.modal-dialog');
                if (dialog && dialog.contains(target)) {
                    return;
                }

                fallbackHideModalElement(modalEl);
                applyBootstrapModalStacking();
            }, true);

            window.__cmfModalEscapeBackdropBound = true;
        }

        function captureModalStackBeforePostback() {
            var modals = getVisibleBootstrapModals();
            cmfModalStackState.idsToRestore = modals
                .map(function (modal) { return modal && modal.id ? modal.id : ''; })
                .filter(function (id) { return !!id; });
            cmfModalStackState.pendingRestore = cmfModalStackState.idsToRestore.length > 0;
            persistModalStackForReload(cmfModalStackState.idsToRestore);
        }

        function restoreModalStackAfterPostback() {
            if (!cmfModalStackState.pendingRestore) {
                applyBootstrapModalStacking();
                return;
            }

            var ids = cmfModalStackState.idsToRestore.slice();
            cmfModalStackState.pendingRestore = false;
            cmfModalStackState.idsToRestore = [];
            clearPersistedModalStack();

            for (var i = 0; i < ids.length; i++) {
                var id = ids[i];
                var modalEl = document.getElementById(id);
                if (!modalEl || !window.jQuery) {
                    continue;
                }

                var $modal = window.jQuery(modalEl);
                if (!$modal.hasClass('show')) {
                    $modal.modal('show');
                }
            }

            setTimeout(function () {
                applyBootstrapModalStacking();
            }, 0);
        }

        function restorePersistedModalStackOnLoad() {
            var ids = readPersistedModalStack();
            if (!ids.length || !window.jQuery) {
                return;
            }

            clearPersistedModalStack();

            setTimeout(function () {
                for (var i = 0; i < ids.length; i++) {
                    var id = ids[i];
                    var modalEl = document.getElementById(id);
                    if (!modalEl) {
                        continue;
                    }

                    var $modal = window.jQuery(modalEl);
                    if (!$modal.hasClass('show')) {
                        $modal.modal('show');
                    }
                }

                applyBootstrapModalStacking();
            }, 0);
        }

        function ensureModalPostbackCapture() {
            if (window.__cmfDoPostBackWrapped || typeof window.__doPostBack !== 'function') {
                return;
            }

            var originalDoPostBack = window.__doPostBack;
            window.__doPostBack = function (eventTarget, eventArgument) {
                captureModalStackBeforePostback();
                return originalDoPostBack(eventTarget, eventArgument);
            };
            window.__cmfDoPostBackWrapped = true;
        }

        function bindModalStackPersistence() {
            if (!window.jQuery) {
                return;
            }

            hoistDashboardModalsToRoot();

            var $document = window.jQuery(document);
            $document.off('shown.bs.modal.cmfStack hidden.bs.modal.cmfStack', '.modal');

            $document.on('shown.bs.modal.cmfStack', '.modal', function () {
                applyBootstrapModalStacking();
            });

            $document.on('hidden.bs.modal.cmfStack', '.modal', function () {
                setTimeout(function () {
                    applyBootstrapModalStacking();
                }, 0);
            });

            ensureModalPostbackCapture();

            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                var prm = Sys.WebForms.PageRequestManager.getInstance();
                prm.add_pageLoaded(function () {
                    hoistDashboardModalsToRoot();
                    wireModalCloseButtonsDirect();
                });
                prm.add_endRequest(function () {
                    hoistDashboardModalsToRoot();
                    ensureModalPostbackCapture();
                    restoreModalStackAfterPostback();
                    wireModalCloseButtonsDirect();
                });
            }

            applyBootstrapModalStacking();
        }

        function bindModalEntryCountHandlers() {
            if (!window.jQuery) {
                return;
            }

            var modalSelector = '#detailsModal,#detailsModal0,#detailsModal2,#detailsModal3,#detailsModalDrivers,#detailsModalDrivers1,#detailsModalSummaryMetrics';
            var $modals = window.jQuery(modalSelector);
            if (!$modals.length) {
                return;
            }

            $modals.off('shown.bs.modal.modalEntryCount');
            $modals.on('shown.bs.modal.modalEntryCount', function () {
                updateModalTitleWithEntryCount(this);
            });

            $modals.each(function () {
                if (window.jQuery(this).hasClass('show')) {
                    updateModalTitleWithEntryCount(this);
                }
            });
        }

        function bindModalBackdropWatchdog() {
            if (window.__cmfModalBackdropWatchdogBound) {
                return;
            }

            window.__cmfModalBackdropWatchdogBound = true;
            window.setInterval(function () {
                var hasVisibleModals = getVisibleBootstrapModals().length > 0;
                var hasBackdrop = document.querySelectorAll('.modal-backdrop').length > 0;
                var hasModalOpenClass = document.body.classList.contains('modal-open');

                if (hasVisibleModals && (!hasBackdrop || !hasModalOpenClass)) {
                    applyBootstrapModalStacking();
                    return;
                }

                if (!hasVisibleModals && (hasBackdrop || hasModalOpenClass)) {
                    applyBootstrapModalStacking();
                }
            }, 250);
        }

        function clearSidebarViewportStickinessStyles(sidebar) {
            if (!sidebar) {
                return;
            }

            sidebar.style.removeProperty('position');
            sidebar.style.removeProperty('top');
            sidebar.style.removeProperty('left');
            sidebar.style.removeProperty('height');
            sidebar.style.removeProperty('max-height');
            sidebar.style.removeProperty('min-height');
            sidebar.style.removeProperty('z-index');
        }

        function syncSidebarViewportStickiness() {
            var sidebar = document.getElementById('mainMenuSidebar');
            if (!sidebar) {
                return;
            }

            // Keep mobile/tablet behavior unchanged.
            if (window.innerWidth <= 900) {
                clearSidebarViewportStickinessStyles(sidebar);
                return;
            }

            var shell = document.getElementById('portalLayoutShell');
            var header = document.querySelector('header.portal-app-header');
            var topOffset = 0;
            var leftOffset = 0;

            if (header) {
                var headerRect = header.getBoundingClientRect();
                topOffset = Math.max(0, Math.round(headerRect.bottom));
            }

            if (shell) {
                var shellRect = shell.getBoundingClientRect();
                leftOffset = Math.max(0, Math.round(shellRect.left));
            }

            sidebar.style.setProperty('position', 'fixed', 'important');
            sidebar.style.setProperty('top', String(topOffset) + 'px', 'important');
            sidebar.style.setProperty('left', String(leftOffset) + 'px', 'important');
            sidebar.style.setProperty('height', 'calc(100vh - ' + String(topOffset) + 'px)', 'important');
            sidebar.style.setProperty('max-height', 'calc(100vh - ' + String(topOffset) + 'px)', 'important');
            sidebar.style.setProperty('min-height', '0px', 'important');
            sidebar.style.setProperty('z-index', '90', 'important');
        }

        function bindSidebarViewportStickiness() {
            if (window.__cmfSidebarViewportStickinessBound) {
                syncSidebarViewportStickiness();
                return;
            }

            window.__cmfSidebarViewportStickinessBound = true;

            var scheduled = false;
            var scheduleSync = function () {
                if (scheduled) {
                    return;
                }

                scheduled = true;
                window.requestAnimationFrame(function () {
                    scheduled = false;
                    syncSidebarViewportStickiness();
                });
            };

            window.addEventListener('scroll', scheduleSync, { passive: true });
            window.addEventListener('resize', scheduleSync, { passive: true });
            window.addEventListener('orientationchange', scheduleSync, { passive: true });

            var sidebar = document.getElementById('mainMenuSidebar');
            if (sidebar && window.MutationObserver) {
                var observer = new MutationObserver(function () {
                    scheduleSync();
                });
                observer.observe(sidebar, {
                    attributes: true,
                    attributeFilter: ['class', 'style']
                });
            }

            if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    scheduleSync();
                });
            }

            scheduleSync();
        }

        $(document).ready(function () {
            bindPopupTableFlowMaskLifecycle();
            hoistDashboardModalsToRoot();
            bindModalStackPersistence();
            bindModalDismissFallbackHandlers();
            wireModalCloseButtonsDirect();
            bindModalEscapeAndBackdropFallbacks();
            restorePersistedModalStackOnLoad();
            bindModalEntryCountHandlers();
            bindModalBackdropWatchdog();
            bindSidebarViewportStickiness();
        });
