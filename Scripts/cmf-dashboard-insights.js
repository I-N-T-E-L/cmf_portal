(function () {
    'use strict';
    var active;

    function escape(value) {
        return String(value == null ? '' : value).replace(/[&<>"']/g, function (character) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character];
        });
    }

    function links(issues) {
        return (issues || []).map(function (issue) {
            if (!/^\d{1,20}$/.test(issue.Id)) return '';
            return '<a href="https://hsdes.intel.com/resource/' + issue.Id + '" target="_blank" rel="noopener noreferrer" title="'
                + escape(issue.Platform + ': ' + issue.Title) + '">' + escape(issue.Id) + ' <small>' + escape(issue.Platform) + '</small></a>';
        }).join('');
    }

    function findings(items, empty) {
        if (!items.length) return '<p class="ci-empty">' + escape(empty) + '</p>';
        return '<ol class="ci-findings">' + items.map(function (item) {
            var title = escape(item.Title);
            if (/^\d{6,20}$/.test(item.RelatedId || '')) title = '<a href="https://hsdes.intel.com/resource/' + item.RelatedId
                + '" target="_blank" rel="noopener noreferrer">' + title + '</a>';
            var reasons = item.Reasons || [];
            var explanation = reasons.length ? '<p class="ci-relationship"><strong>Why related:</strong> ' + escape(reasons[0].Text) + '</p>' : '';
            if (reasons.length > 1) explanation += '<details class="ci-reasons"><summary>Supporting comparisons</summary><ul>'
                + reasons.slice(1).map(function (reason) {
                    return '<li>' + escape(reason.Text) + '<div class="ci-links">' + links((reason.IssueIds || []).map(function (id) {
                        return { Id: id, Title: 'Source record for ' + reason.Source, Platform: '' };
                    })) + '</div></li>';
                }).join('') + '</ul></details>';
            return '<li class="cmf-assistant-text-section cmf-assistant-description-section"><h3><span>' + title + '</span></h3>'
                + (item.AiReviewed ? '<small class="ci-small">AI-prioritized</small>' : '') + '<p class="ci-evidence">' + escape(item.Evidence) + '</p>'
                + explanation
                + '<p class="ci-action"><strong>Suggested review:</strong> ' + escape(item.Action) + '</p>'
                + '<details><summary>Evidence: ' + escape(item.DistinctIssues) + ' distinct HSDs</summary>'
                + '<p class="ci-small">' + escape(item.Occurrences) + ' record occurrence(s) | ' + escape(item.PlatformCount)
                + ' platform(s) | linked HSD sample</p><div class="ci-links">' + links(item.Issues) + '</div>'
                + '<ul class="ci-source-titles">' + (item.Issues || []).map(function (issue) {
                    return '<li><strong>' + escape(issue.Id) + ':</strong> ' + escape(issue.Title) + '</li>';
                }).join('') + '</ul></details></li>';
        }).join('') + '</ol>';
    }

    function receive(state, data) {
        if (state.pageController) state.pageController.abort();
        state.data = data;
        state.pageVersion = (state.pageVersion || 0) + 1;
        state.pageBusy = false;
        state.pageError = '';
        state.pages = {
            highlights: { page: 0, items: data.Highlights, total: data.HighlightCount },
            similar: { page: 0, items: data.Groups, total: data.GroupCount }
        };
    }

    function pagination(state, view) {
        var current = state.pages[view];
        var size = state.data.PageSize;
        var first = current.total ? current.page * size + 1 : 0;
        var last = Math.min((current.page + 1) * size, current.total);
        return '<nav class="ci-pagination" aria-label="' + (view === 'similar' ? 'Related issue' : 'Highlight') + ' pages">'
            + '<span>' + first + '-' + last + ' of ' + current.total + (view === 'similar' ? ' relationship groups' : ' highlights') + '</span>'
            + (current.total > size ? '<button type="button" data-page-step="-1" title="Previous page" aria-label="Previous page"'
                + (state.pageBusy || current.page === 0 ? ' disabled' : '') + '><i class="fas fa-chevron-left" aria-hidden="true"></i></button>'
                + '<button type="button" data-page-step="1" title="Next page" aria-label="Next page"'
                + (state.pageBusy || last >= current.total ? ' disabled' : '') + '><i class="fas fa-chevron-right" aria-hidden="true"></i></button>' : '')
            + '</nav><div class="ci-page-message" role="status">' + escape(state.pageBusy ? 'Loading page...' : state.pageError) + '</div>';
    }

    function loadPage(state, view, pageNumber) {
        if (state.pageBusy) return;
        var version = state.pageVersion;
        var serial = state.serial;
        var generated = state.data.GeneratedAt;
        var controller = state.pageController = new AbortController();
        var deadline = setTimeout(function () { controller.abort(); }, 30000);
        state.pageBusy = true;
        state.pageError = '';
        draw(state);
        fetch('CMF_Web_portal.aspx/GetDashboardInsightPage', {
            method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json; charset=utf-8' },
            signal: controller.signal,
            body: JSON.stringify({ evidenceKey: state.data.EvidenceKey, view: view, page: pageNumber })
        }).then(function (response) {
            if (!response.ok) throw new Error('Page unavailable.');
            return response.json();
        }).then(function (payload) {
            if (active !== state || serial !== state.serial || version !== state.pageVersion || !state.host.isConnected) return;
            var data = payload.d || payload;
            if (!data || !data.Success || data.GeneratedAt !== generated) throw new Error('Page changed or expired.');
            state.pages[view] = { page: data.Page, items: data.Items, total: data.Total };
            state.pageBusy = false;
            draw(state);
            if (state.tab === view) state.host.querySelector('.ci-content').scrollTop = 0;
        }).catch(function () {
            if (active !== state || serial !== state.serial || version !== state.pageVersion) return;
            state.pageBusy = false;
            state.pageError = 'Page unavailable or evidence changed. Retry, or refresh insights.';
            draw(state);
        }).then(function () { clearTimeout(deadline); });
    }

    function signed(value) {
        return value == null ? 'N/A' : (value > 0 ? '+' : '') + value;
    }

    function trendTable(data, dimension) {
        var rows = data.Trends.filter(function (row) { return row.Dimension === dimension; });
        if (!rows.length) return '<p class="ci-empty">No accepted CMF records in this scope.</p>';
        return '<div class="ci-table-wrap"><table class="ci-table"><thead><tr><th scope="col">' + escape(dimension)
            + '</th><th scope="col">Open</th><th scope="col">Prior</th><th scope="col">Change</th><th scope="col">Oldest</th></tr></thead><tbody>'
            + rows.map(function (row) {
                return '<tr><th scope="row">' + escape(row.Name) + (row.Issues.length ? '<details><summary>HSD sample</summary><div class="ci-links">'
                    + links(row.Issues) + '</div></details>' : '') + (row.Excluded ? '<small class="ci-excluded">' + escape(row.Excluded) + ' excluded</small>' : '')
                    + '</th><td>' + escape(row.Open) + '</td><td>' + escape(row.PreviousOpen == null ? 'N/A' : row.PreviousOpen)
                    + '</td><td class="' + (row.Change > 0 ? 'ci-up' : row.Change < 0 ? 'ci-down' : '') + '">'
                    + escape(signed(row.Change)) + '</td><td>' + escape(row.OldestDays == null ? 'N/A' : row.OldestDays + ' d') + '</td></tr>';
            }).join('') + '</tbody></table></div>';
    }

    function draw(state) {
        if (!state.data || active !== state || !state.host.isConnected) return;
        var data = state.data;
        var content = state.host.querySelector('.ci-content');
        if (state.scope) state.scope.textContent = data.Scope;
        var badge = state.host.querySelector('.ci-mode');
        badge.textContent = data.Mode === 'ai-ranked' ? 'AI-ranked' : 'Calculated';
        badge.title = data.Message || 'Server-calculated evidence; AI prioritization not yet available.';
        state.host.querySelector('.ci-metadata').textContent = data.Records + ' accepted CMF records | ' + data.DistinctIssues
            + ' distinct HSDs | ' + data.DuplicateRecords + ' duplicate rows | ' + data.ExcludedRecords + ' excluded from open balance comparison';
        var updated = new Date(data.GeneratedAt);
        state.host.querySelector('.ci-time').textContent = 'Updated ' + updated.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        state.host.querySelector('.ci-time').dateTime = data.GeneratedAt;
        if (state.tab === 'trends') {
            content.innerHTML = '<div class="ci-trend-toolbar"><select aria-label="Trend dimension">'
                + ['Component', 'Ingredient', 'Platform'].map(function (name) {
                    return '<option' + (name === state.dimension ? ' selected' : '') + '>' + name + '</option>';
                }).join('') + '</select><span class="ci-small">Top 15 per dimension | ' + escape(data.TrendCount) + ' groups total</span></div>'
                + '<p class="ci-small ci-comparison">' + escape(data.Comparison) + '</p>'
                + trendTable(data, state.dimension) + '<p class="ci-small">Oldest: days since acceptance among comparable open records.</p>';
            content.querySelector('select').addEventListener('change', function (event) { state.dimension = event.target.value; draw(state); });
        } else if (state.tab === 'similar') {
            content.innerHTML = pagination(state, 'similar')
                + findings(state.pages.similar.items, 'No shared promoted or merge HSD groups found in this scope.')
                + '<p class="ci-small">Groups require explicit HSD links. Shared classifications and exact title overlap are supporting evidence, not proof of a common cause.</p>';
        } else {
            content.innerHTML = pagination(state, 'highlights')
                + findings(state.pages.highlights.items, 'No qualifying open-balance, aging or shared-link findings among accepted CMFs in this scope.');
        }
        content.setAttribute('aria-busy', String(!!state.pageBusy));
        content.querySelectorAll('[data-page-step]').forEach(function (button) {
            button.addEventListener('click', function () {
                loadPage(state, state.tab, state.pages[state.tab].page + Number(button.dataset.pageStep));
            });
        });
        state.host.querySelector('.ci-provenance').textContent = data.SourceStatus + ' Accepted CMFs only; pending requests are not included.';
        state.host.querySelectorAll('[role="tab"]').forEach(function (tab) {
            var selected = tab.dataset.view === state.tab;
            tab.setAttribute('aria-selected', String(selected));
            tab.tabIndex = selected ? 0 : -1;
        });
        content.setAttribute('aria-labelledby', state.host.id + '-' + state.tab);
        clearTimeout(state.expiryTimer);
        state.expiryTimer = setTimeout(function () {
            status(state, 'Cached insight expired. Refresh for current evidence.', false);
        }, Math.max(0, Date.parse(data.ExpiresAt) - Date.now()));
    }

    function status(state, message, busy) {
        if (active !== state || !state.host.isConnected) return;
        state.host.querySelector('.ci-status').textContent = message;
        state.host.querySelector('.ci-refresh').disabled = !!busy;
    }

    function request(state, ai, refresh, serial) {
        return fetch('CMF_Web_portal.aspx/' + (ai ? 'RankDashboardInsights' : 'GetDashboardInsights'), {
            method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json; charset=utf-8' },
            signal: state.controller.signal,
            body: JSON.stringify(ai ? { evidenceKey: state.data.EvidenceKey, refresh: refresh } : { platform: state.platform })
        }).then(function (response) {
            if (!response.ok) throw new Error('Insights request failed.');
            return response.json();
        }).then(function (payload) {
            if (active !== state || state.serial !== serial || !state.host.isConnected) return null;
            var data = payload.d || payload;
            if (!data || data.Success !== true) throw new Error('Insights unavailable.');
            return data;
        });
    }

    function load(state, refresh) {
        if (state.controller) state.controller.abort();
        if (state.pageController) state.pageController.abort();
        clearTimeout(state.expiryTimer);
        var controller = state.controller = new AbortController();
        var serial = ++state.serial;
        status(state, state.data ? 'Refreshing evidence...' : 'Loading evidence...', true);
        var deadline = setTimeout(function () { controller.abort(); }, 180000);
        request(state, false, refresh, serial).then(function (data) {
            if (!data) return null;
            receive(state, data);
            draw(state);
            if (!data.Highlights.length) { status(state, 'No findings in this scope.', false); return null; }
            status(state, 'Reviewing AI priorities; calculated evidence is available.', true);
            return request(state, true, refresh, serial).then(function (ranked) {
                if (!ranked) return;
                receive(state, ranked);
                draw(state);
                status(state, ranked.Message, false);
            });
        }).catch(function () {
            if (state.serial !== serial) return;
            status(state, state.data ? 'AI ranking unavailable. Showing calculated evidence.' : 'Insights unavailable. Refresh to retry.', false);
        }).then(function () { clearTimeout(deadline); });
    }

    function render(snapshot) {
        var platform = window.CMF_PORTAL.currentDashboardPlatform;
        var overall = platform === 'CMF_OVERALL_ALL_COMPONENTS_TABLE';
        var host = document.getElementById(overall ? 'homeOverallInsights' : 'homePlatformInsights');
        if (!host) return;
        var key = platform + '|' + snapshot.GeneratedAt;
        if (active && active.host === host && active.key === key) return;
        if (active) {
            if (active.controller) active.controller.abort();
            if (active.pageController) active.pageController.abort();
            clearTimeout(active.expiryTimer);
        }
        var state = active = { host: host, scope: document.getElementById(host.id + 'Scope'), key: key, platform: platform, tab: 'highlights', dimension: 'Component', serial: 0 };
        if (state.scope) state.scope.textContent = snapshot.PlatformLabel || (overall ? 'Overall' : '');
        host.innerHTML = '<div class="ci-controls"><div class="ci-tabs" role="tablist" aria-label="Insight views">'
            + [['highlights', 'Highlights', 'lightbulb'], ['trends', 'Trends', 'chart-line'], ['similar', 'Similar Issues', 'link']].map(function (tab) {
                return '<button type="button" role="tab" id="' + host.id + '-' + tab[0] + '" data-view="' + tab[0]
                    + '" aria-controls="' + host.id + '-content" aria-selected="' + (tab[0] === 'highlights') + '">'
                    + '<i class="fas fa-' + tab[2] + '" aria-hidden="true"></i> ' + tab[1] + '</button>';
            }).join('') + '</div><div class="ci-toolbar"><span class="ai-summary-status-value ci-mode">Calculated</span>'
            + '<button type="button" class="ci-refresh" title="Refresh insights" aria-label="Refresh insights"><i class="fas fa-sync-alt" aria-hidden="true"></i></button></div></div>'
            + '<div class="ci-status" role="status" aria-live="polite"></div>'
            + '<div class="ci-content" role="tabpanel" tabindex="0" id="' + host.id + '-content"></div>'
            + '<div class="ci-footer"><time class="ci-time"></time><details><summary>Scope &amp; sources</summary>'
            + '<p class="ci-metadata"></p><p class="ci-provenance"></p></details></div>';
        host.querySelector('.ci-refresh').addEventListener('click', function () { load(state, true); });
        host.querySelectorAll('[role="tab"]').forEach(function (tab) {
            tab.addEventListener('click', function () { state.tab = tab.dataset.view; draw(state); });
            tab.addEventListener('keydown', function (event) {
                if (['ArrowLeft', 'ArrowRight', 'Home', 'End'].indexOf(event.key) < 0) return;
                event.preventDefault();
                var tabs = Array.from(host.querySelectorAll('[role="tab"]'));
                var index = tabs.indexOf(tab);
                index = event.key === 'Home' ? 0 : event.key === 'End' ? tabs.length - 1
                    : (index + (event.key === 'ArrowRight' ? 1 : -1) + tabs.length) % tabs.length;
                tabs[index].focus();
                tabs[index].click();
            });
        });
        load(state, false);
    }

    window.CmfDashboardInsights = { render: render };
}());