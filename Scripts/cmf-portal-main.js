function toggleSidebar() {
    var sidebar = document.querySelector('.sidebar');
    if (!sidebar) return;
    sidebar.style.right = (sidebar.style.right === '0px' || sidebar.style.right === '0') ? '-250px' : '0px';
}
(function () {
    var selector = '#issueTabShell .issue-top-filter-grid select, #pendingTabShell .cmf-pending-filter-select, #ddlSharedPlatform';
    var controls = new Map();
    var active = null;
    var refreshQueued = false;

    function icon(name) {
        var element = document.createElement('i');
        element.className = 'fas fa-' + name;
        element.setAttribute('aria-hidden', 'true');
        return element;
    }

    function closeMenu(restoreFocus) {
        if (!active) return;
        var trigger = active.control.trigger;
        active.popup.remove();
        active = null;
        trigger.setAttribute('aria-expanded', 'false');
        trigger.removeAttribute('aria-controls');
        if (restoreFocus && trigger.isConnected) trigger.focus();
    }

    function sync(control) {
        var option = control.select.options[control.select.selectedIndex];
        var selected = Array.from(control.select.selectedOptions).filter(function (item) { return item.value; });
        control.text.textContent = control.select.multiple && selected.length ? (selected.length === 1 ? selected[0].textContent : selected.length + ' selected') : option ? option.textContent : 'All';
        control.trigger.title = control.select.multiple && selected.length ? selected.map(function (item) { return item.title || item.textContent; }).join(', ') : option ? option.title || option.textContent : control.label;
        control.trigger.disabled = control.select.disabled;
        control.trigger.setAttribute('aria-label', control.label + ': ' + control.text.textContent);
    }

    function choose(control, index) {
        if (!control.select.options[index] || control.select.options[index].disabled) return;
        if (control.select.multiple) {
            var option = control.select.options[index];
            option.selected = !option.selected;
            if (!option.value) {
                Array.from(control.select.options).forEach(function (item) { item.selected = !item.value; });
            } else if (control.select.options[0] && !control.select.options[0].value) {
                control.select.options[0].selected = !Array.from(control.select.selectedOptions).some(function (item) { return item.value; });
            }
        } else {
            closeMenu(true);
            control.select.selectedIndex = index;
        }
        sync(control);
        control.select.dispatchEvent(new Event('change', { bubbles: true }));
        if (active && active.control === control) active.renderOptions();
    }

    function openMenu(control) {
        if (active && active.control === control) {
            closeMenu(true);
            return;
        }
        closeMenu(false);
        var popup = document.createElement('div');
        popup.className = 'portal-filter-menu';
        var searchRow = document.createElement('div');
        searchRow.className = 'portal-filter-menu-search';
        searchRow.appendChild(icon('search'));
        var search = document.createElement('input');
        search.type = 'search';
        search.placeholder = 'Search ' + control.label.toLowerCase();
        search.setAttribute('aria-label', search.placeholder);
        searchRow.appendChild(search);
        var summary = document.createElement('div');
        summary.className = 'portal-filter-menu-summary';
        var selected = document.createElement('span');
        var option = control.select.options[control.select.selectedIndex];
        selected.textContent = option && option.value && !/^all\b/i.test(option.textContent) ? '1 selected' : 'All';
        summary.appendChild(selected);
        var reset = document.createElement('button');
        reset.type = 'button';
        reset.title = 'Clear ' + control.label + ' filter';
        reset.setAttribute('aria-label', reset.title);
        reset.appendChild(icon('rotate-left'));
        reset.addEventListener('click', function () {
            if (control.select.id === 'ddlSharedPlatform' && control.select.multiple) {
                control.select.selectedIndex = 0;
                sync(control);
                control.select.dispatchEvent(new Event('change', { bubbles: true }));
                return;
            }
            choose(control, 0);
        });
        summary.appendChild(reset);
        var list = document.createElement('div');
        list.className = 'portal-filter-menu-options';
        list.id = control.select.id + '_filterOptions';
        list.setAttribute('role', 'listbox');
        list.setAttribute('aria-label', control.label);
        if (control.select.multiple) list.setAttribute('aria-multiselectable', 'true');
        popup.append(searchRow, summary, list);

        function renderOptions() {
            var focusedIndex = document.activeElement.dataset.optionIndex;
            var selectedCount = Array.from(control.select.selectedOptions).filter(function (item) { return item.value; }).length;
            selected.textContent = selectedCount ? selectedCount + ' selected' : 'All';
            list.replaceChildren();
            var query = search.value.toLowerCase().trim();
            Array.from(control.select.options).forEach(function (item, index) {
                if (item.hidden || (item.textContent + ' ' + item.title).toLowerCase().indexOf(query) < 0) return;
                var button = document.createElement('button');
                button.type = 'button';
                button.className = 'portal-filter-option';
                button.setAttribute('role', 'option');
                button.setAttribute('aria-selected', item.selected ? 'true' : 'false');
                button.dataset.optionIndex = index;
                button.tabIndex = -1;
                button.disabled = item.disabled;
                button.title = item.title || item.textContent;
                var check = document.createElement('span');
                check.className = 'portal-filter-option-check';
                check.setAttribute('aria-hidden', 'true');
                if (item.selected) check.appendChild(icon('check'));
                var text = document.createElement('span');
                text.textContent = item.textContent;
                button.append(check, text);
                button.addEventListener('click', function (event) {
                    event.preventDefault();
                    if (control.select.multiple) event.stopPropagation();
                    choose(control, index);
                });
                list.appendChild(button);
                if (String(index) === focusedIndex) button.focus();
            });
            if (!list.children.length) {
                var empty = document.createElement('div');
                empty.className = 'portal-filter-menu-empty';
                empty.setAttribute('role', 'status');
                empty.textContent = 'No matches';
                list.appendChild(empty);
            }
        }

        search.addEventListener('input', renderOptions);
        popup.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                event.preventDefault();
                closeMenu(true);
                return;
            }
            var options = Array.from(list.querySelectorAll('button:not(:disabled)'));
            var index = options.indexOf(document.activeElement);
            var target = null;
            if (event.key === 'ArrowDown') target = options[Math.min(index + 1, options.length - 1)];
            if (event.key === 'ArrowUp') target = options[Math.max(0, index - 1)];
            if (event.key === 'Home' && index >= 0) target = options[0];
            if (event.key === 'End' && index >= 0) target = options[options.length - 1];
            if (target) {
                event.preventDefault();
                target.focus();
            }
        });
        active = { control: control, popup: popup, renderOptions: renderOptions };
        document.body.appendChild(popup);
        renderOptions();
        var bounds = control.trigger.getBoundingClientRect();
        var viewportWidth = document.documentElement.clientWidth;
        popup.style.width = Math.min(Math.max(bounds.width, 336), viewportWidth - 16) + 'px';
        popup.style.left = Math.max(8, Math.min(bounds.left, viewportWidth - popup.offsetWidth - 8)) + 'px';
        var below = window.innerHeight - bounds.bottom - 14;
        var above = bounds.top - 14;
        var openAbove = below < Math.min(popup.offsetHeight, 220) && above > below;
        var available = Math.max(100, openAbove ? above : below);
        list.style.maxHeight = Math.max(40, Math.min(256, available - 100)) + 'px';
        popup.style.top = (openAbove ? Math.max(8, bounds.top - popup.offsetHeight - 6) : bounds.bottom + 6) + 'px';
        control.trigger.setAttribute('aria-expanded', 'true');
        control.trigger.setAttribute('aria-controls', list.id);
        search.focus();
    }

    function initialize() {
        controls.forEach(function (control, select) {
            if (!select.isConnected) controls.delete(select);
        });
        document.querySelectorAll(selector).forEach(function (select) {
            if (select.matches('#pendingTabShell .cmf-pending-filter-select') && !select.multiple) {
                var previousIndex = select.selectedIndex;
                select.multiple = true;
                if (previousIndex >= 0) select.options[previousIndex].selected = true;
            }
            if (controls.has(select)) {
                sync(controls.get(select));
                return;
            }
            var field = select.closest('.issue-top-filter-item, .portal-filter-field, .overview-platform-filter') || select.parentElement;
            var label = select.labels.length ? select.labels[0] : field.querySelector('label, :scope > span');
            var name = label ? Array.from(label.childNodes).filter(function (node) { return node.nodeType === 3; }).map(function (node) { return node.textContent; }).join(' ').trim().replace(/:$/, '') : 'Filter';
            var wrapper = document.createElement('div');
            wrapper.className = 'portal-filter-dropdown';
            var trigger = document.createElement('button');
            trigger.type = 'button';
            trigger.className = 'portal-filter-trigger';
            trigger.setAttribute('aria-haspopup', 'listbox');
            trigger.setAttribute('aria-expanded', 'false');
            var text = document.createElement('span');
            text.className = 'portal-filter-trigger-text';
            trigger.append(text, icon('chevron-down'));
            select.before(wrapper);
            wrapper.append(select, trigger);
            select.classList.add('portal-filter-native');
            select.tabIndex = -1;
            select.setAttribute('aria-hidden', 'true');
            var control = { select: select, trigger: trigger, text: text, label: name || 'Filter' };
            controls.set(select, control);
            trigger.addEventListener('click', function (event) {
                event.preventDefault();
                openMenu(control);
            });
            trigger.addEventListener('keydown', function (event) {
                if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
                    event.preventDefault();
                    openMenu(control);
                }
            });
            select.addEventListener('change', function () { sync(control); });
            select.addEventListener('focus', function () { trigger.focus(); });
            sync(control);
        });
    }

    document.addEventListener('click', function (event) {
        if (active && !active.popup.contains(event.target) && !active.control.trigger.contains(event.target)) closeMenu(false);
        controls.forEach(sync);
    });
    document.addEventListener('focusin', function (event) {
        if (active && !active.popup.contains(event.target) && event.target !== active.control.trigger) closeMenu(false);
    });
    document.addEventListener('scroll', function (event) {
        if (active && !active.popup.contains(event.target)) closeMenu(false);
    }, true);
    window.addEventListener('resize', function () { closeMenu(false); });
    document.addEventListener('DOMContentLoaded', function () {
        initialize();
        window.CMF_PORTAL = window.CMF_PORTAL || {};
        window.CMF_PORTAL.refreshFilterMenus = initialize;
        new MutationObserver(function (records) {
            var changed = records.some(function (record) {
                return record.target.nodeName === 'SELECT' || Array.from(record.addedNodes).some(function (node) {
                    return node.nodeType === 1 && (node.matches(selector) || node.querySelector(selector));
                });
            });
            if (!changed || refreshQueued) return;
            refreshQueued = true;
            requestAnimationFrame(function () {
                refreshQueued = false;
                initialize();
            });
        }).observe(document.body, { childList: true, subtree: true });
        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var manager = Sys.WebForms.PageRequestManager.getInstance();
            manager.add_beginRequest(function () { closeMenu(false); });
            manager.add_endRequest(initialize);
        }
    });
}());

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

            __doPostBack('ShowCMFIssues', component + '|' + driver + '|' + issueType);
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

                // Trigger postback manually
                __doPostBack('ShowDriverModal', design + ',' + driver);
        }

        function showImplementedVerifiedDetailsModal(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

                // Trigger postback manually
                __doPostBack('ShowImplementedVerifiedModal', design);
            }

        

        function showDetailsModal(design) {
            // Store design in a hidden field so server can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = design;

            // Trigger postback manually
            __doPostBack('ShowModal', design);
        }


        function showDetailsModal2(ingred) {
            // Store design in a hidden field so seriver can access it
            document.getElementById(window.CMF_PORTAL.ids.hiddenModalDesign).value = ingred;

            // Trigger postback manually
            __doPostBack('ShowModal2', ingred);
        }
            function showDriverIssues(milestoneDriver) {
          var hf = document.getElementById(window.CMF_PORTAL.ids.hiddenDriverName);
          if (hf) hf.value = milestoneDriver || '';
          __doPostBack('ShowDriverIssues', milestoneDriver || '');
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

            // Trigger postback manually
            __doPostBack('ShowModal3', oem);
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

        $(document).ready(function () {
            bindModalEntryCountHandlers();
        });
