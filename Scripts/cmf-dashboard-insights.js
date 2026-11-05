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
                + (item.AiReviewed ? '<small class="ci-small">AI-reviewed'
                    + (item.Kind === 'trait' ? ' | Shared trait' : item.Kind === 'technical' ? ' | Technical similarity' : item.Kind === 'trend' ? ' | Dated pattern' : '')
                    + '</small>' : '') + '<p class="ci-evidence">' + escape(item.Evidence) + '</p>'
                + explanation
                + '<p class="ci-action"><strong>Suggested review:</strong> ' + escape(item.Action) + '</p>'
                + '<details><summary>Evidence: ' + escape(item.DistinctIssues) + ' distinct HSDs</summary>'
                + '<p class="ci-small">' + (item.SourceInsightIds && item.SourceInsightIds.length
                    ? escape(item.DistinctIssues) + ' distinct linked HSDs' : escape(item.Occurrences) + ' record occurrence(s)') + ' | ' + escape(item.PlatformCount)
                + ' platform(s) | all linked HSDs</p><div class="ci-links">' + links(item.Issues) + '</div>'
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
                return '<tr><th scope="row">' + escape(row.Name) + (row.Issues.length ? '<details><summary>HSDs</summary><div class="ci-links">'
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
        badge.textContent = data.Mode === 'ai' ? (data.CandidateCount === 0 ? 'No groups' : 'AI-reviewed') : 'Analyzing';
        badge.title = data.Message || 'AI analysis in progress.';
        state.host.querySelector('.ci-metadata').textContent = data.Records + ' accepted CMF records | ' + data.DistinctIssues
            + ' distinct HSDs | ' + data.DuplicateRecords + ' duplicate rows | ' + data.ExcludedRecords + ' excluded from open balance comparison'
            + (data.CandidateCount == null ? '' : ' | ' + data.CandidateCount + ' calculated groups | ' + data.ReviewedCandidates + ' AI-reviewed groups');
        var updated = new Date(data.GeneratedAt);
        state.host.querySelector('.ci-time').textContent = 'Updated ' + updated.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        state.host.querySelector('.ci-time').dateTime = data.GeneratedAt;
        if (state.tab === 'trends') {
            content.innerHTML = '<div class="ci-trend-toolbar"><select aria-label="Trend dimension">'
                + ['Component', 'Ingredient', 'Platform'].map(function (name) {
                    return '<option' + (name === state.dimension ? ' selected' : '') + '>' + name + '</option>';
                }).join('') + '</select><span class="ci-small">' + escape(data.TrendCount) + ' groups total</span></div>'
                + '<p class="ci-small ci-comparison">' + escape(data.Comparison) + '</p>'
                + trendTable(data, state.dimension) + '<p class="ci-small">Oldest: days since acceptance among comparable open records.</p>';
            content.querySelector('select').addEventListener('change', function (event) { state.dimension = event.target.value; draw(state); });
        } else if (state.tab === 'similar') {
            content.innerHTML = pagination(state, 'similar')
                + findings(state.pages.similar.items, 'AI found no sufficiently supported technical similarity groups in this scope.')
                + '<p class="ci-small">Technical similarity is not proof of a shared root cause.</p>';
        } else {
            content.innerHTML = pagination(state, 'highlights')
                + findings(state.pages.highlights.items, data.CandidateCount === 0 ? 'No qualifying multi-field issue groups in this scope.'
                    : 'AI review completed without sufficiently supported insights in this scope.');
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
        var controller = state.controller;
        var deadline = setTimeout(function () { controller.abort(); }, 45000);
        return fetch('CMF_Web_portal.aspx/' + (ai ? 'RankDashboardInsights' : 'GetDashboardInsights'), {
            method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json; charset=utf-8' },
            signal: controller.signal,
            body: JSON.stringify(ai ? { evidenceKey: state.evidenceKey, refresh: refresh } : { platform: state.platform })
        }).then(function (response) {
            if (!response.ok) throw new Error('Insights request failed.');
            return response.json();
        }).then(function (payload) {
            if (active !== state || state.serial !== serial || !state.host.isConnected) return null;
            var data = payload.d || payload;
            if (!data || data.Success !== true) throw new Error(data && data.Message || 'AI insights unavailable.');
            return data;
        }).finally(function () { clearTimeout(deadline); });
    }

    function load(state, refresh) {
        if (state.controller) state.controller.abort();
        if (state.pageController) state.pageController.abort();
        clearTimeout(state.expiryTimer);
        clearTimeout(state.pollTimer);
        state.controller = new AbortController();
        var serial = ++state.serial;
        state.data = null;
        state.host.querySelector('.ci-content').innerHTML = '';
        state.host.querySelector('.ci-content').setAttribute('aria-busy', 'true');
        state.host.querySelector('.ci-mode').textContent = 'Analyzing';
        state.host.querySelector('.ci-time').textContent = '';
        state.host.querySelector('.ci-metadata').textContent = '';
        state.host.querySelector('.ci-provenance').textContent = '';
        status(state, 'Grouping Closed, Implemented and Open issues by shared classifications...', true);
        function unavailable(error) {
            if (active !== state || state.serial !== serial || !state.host.isConnected) return;
            state.host.querySelector('.ci-mode').textContent = 'Unavailable';
            state.host.querySelector('.ci-content').setAttribute('aria-busy', 'false');
            state.host.querySelector('.ci-content').innerHTML = '<p class="ci-empty">AI insights unavailable.</p>';
            status(state, error.message || 'AI analysis unavailable. Refresh to retry.', false);
        }
        function poll(forceRefresh) {
            request(state, true, forceRefresh, serial).then(function (data) {
                if (!data) return;
                if (data.Mode === 'ai') {
                    receive(state, data);
                    draw(state);
                    status(state, data.Message, false);
                } else if (data.Mode === 'ai-loading') {
                    state.host.querySelector('.ci-metadata').textContent = data.Records + ' accepted records | ' + data.DistinctIssues
                        + ' distinct HSDs | ' + data.CandidateCount + ' ranked groups | ' + data.ReviewedCandidates + ' reviewed';
                    status(state, data.Message, true);
                    state.pollTimer = setTimeout(function () { poll(false); }, 3000);
                } else throw new Error('AI analysis returned an incomplete result. Refresh to retry.');
            }).catch(unavailable);
        }
        request(state, false, refresh, serial).then(function (data) {
            if (!data) return;
            state.evidenceKey = data.EvidenceKey;
            state.host.querySelector('.ci-metadata').textContent = data.Records + ' accepted records | ' + data.DistinctIssues + ' distinct HSDs';
            poll(refresh);
        }).catch(unavailable);
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
            clearTimeout(active.pollTimer);
        }
        var state = active = { host: host, scope: document.getElementById(host.id + 'Scope'), key: key, platform: platform, tab: 'highlights', dimension: 'Component', serial: 0 };
        if (state.scope) state.scope.textContent = snapshot.PlatformLabel || (overall ? 'Overall' : '');
        host.innerHTML = '<div class="ci-controls"><div class="ci-tabs" role="tablist" aria-label="Insight views">'
            + [['highlights', 'Highlights', 'lightbulb'], ['trends', 'Trends', 'chart-line'], ['similar', 'Similar Issues', 'link']].map(function (tab) {
                return '<button type="button" role="tab" id="' + host.id + '-' + tab[0] + '" data-view="' + tab[0]
                    + '" aria-controls="' + host.id + '-content" aria-selected="' + (tab[0] === 'highlights') + '">'
                    + '<i class="fas fa-' + tab[2] + '" aria-hidden="true"></i> ' + tab[1] + '</button>';
            }).join('') + '</div><div class="ci-toolbar"><span class="ai-summary-status-value ci-mode">Analyzing</span>'
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