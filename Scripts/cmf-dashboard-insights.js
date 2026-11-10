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
        if (items.every(function (item) { return item.Kind === 'briefing'; })) {
            var categories = ['Needs Attention', 'Progress', 'Direction Over Time', 'Concentration'];
            return '<div class="ci-briefing">' + categories.map(function (category) {
                var group = items.filter(function (item) { return item.Category === category; });
                if (!group.length) return '';
                var sectionClass = ' class="cmf-assistant-text-section '
                    + (category === 'Needs Attention' ? 'cmf-assistant-impact-section'
                        : category === 'Progress' || category === 'Direction Over Time' ? 'cmf-assistant-metrics-section'
                        : 'cmf-assistant-description-section') + '"';
                return '<section class="ci-briefing-group" aria-label="' + escape(category) + '"><h4 class="ci-category">'
                    + escape(category) + '</h4><ol class="ci-findings">' + group.map(function (item) {
                        return '<li data-finding-id="' + escape(item.Id) + '"' + sectionClass + '><h3><span>' + escape(item.Title) + '</span></h3>'
                            + (item.Label ? '<p class="ci-small ci-finding-label">' + escape(item.Label) + '</p>' : '')
                            + '<p class="ci-evidence">' + escape(item.Evidence) + '</p><details class="ci-support"><summary>'
                            + (item.PlatformCount > 1 ? 'Evidence from ' + escape(item.PlatformCount) + ' platforms' : 'Supporting evidence')
                            + '</summary><ul>' + (item.Reasons || []).map(function (reason) {
                                return '<li><strong>' + escape(reason.Source) + ':</strong> ' + escape(reason.Text) + '</li>';
                            }).join('') + '</ul></details></li>';
                    }).join('') + '</ol></section>';
            }).join('') + '</div>';
        }
        return '<ol class="ci-findings">' + items.map(function (item) {
            var title = escape(item.Title);
            if (item.Kind === 'dashboard') return '<li data-finding-id="' + escape(item.Id)
                + '" class="cmf-assistant-text-section cmf-assistant-description-section"><h3><span>' + title + '</span></h3>'
                + '<small class="ci-small">Verified dashboard facts' + (item.AiReviewed ? ' | AI-ranked' : ' | Unranked') + '</small>'
                + '<p class="ci-evidence">' + escape(item.Evidence) + '</p><p class="ci-small">Source: '
                + escape((item.Reasons || []).map(function (reason) { return reason.Source; }).join(', ')) + '</p></li>';
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
            return '<li data-finding-id="' + escape(item.Id) + '" class="cmf-assistant-text-section cmf-assistant-description-section"><h3><span>' + title + '</span></h3>'
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

    function summaryContent(analysis) {
        if (!analysis || analysis.Mode === 'ai-loading') return '<p class="ci-empty">Generating AI summary...</p>';
        if (analysis.Mode === 'empty') return '<p class="ci-empty">No material findings in this scope.</p>';
        if (analysis.Mode !== 'ai' || analysis.Success !== true || !Array.isArray(analysis.Claims) || !analysis.Claims.length)
            return '<p class="ci-empty">AI summary unavailable.</p>';
        var groups = [], sharedReviews = [];
        function groupFor(source) {
            var group = groups.filter(function (item) { return item.source === source; })[0];
            if (!group) {
                group = { source: source, lines: [], reviews: [], evidence: [] };
                groups.push(group);
            }
            return group;
        }
        analysis.Claims.forEach(function (claim) {
            var reasons = claim.Evidence || [];
            var scope = typeof claim.Scope === 'string' ? claim.Scope.replace(/\s+/g, ' ').trim() : '';
            var sources = reasons.map(function (reason) { return String(reason.Source || '').replace(/\s+/g, ' ').trim(); })
                .filter(function (source, index, all) { return source && all.indexOf(source) === index; })
                .sort(function (left, right) { return right.length - left.length; });
            String(claim.Summary || '').split('\n').forEach(function (line) {
                var source = scope || sources.filter(function (label) { return line.indexOf(label + ': ') === 0; })[0] || '';
                groupFor(source).lines.push(scope ? line : source ? line.slice(source.length + 2) : line);
            });
            reasons.forEach(function (reason) {
                groupFor(String(reason.Source || '').replace(/\s+/g, ' ').trim()).evidence.push(reason);
            });
            if (claim.ReviewSuggestion) {
                if (sources.length === 1) {
                    var reviews = groupFor(sources[0]).reviews;
                    if (reviews.indexOf(claim.ReviewSuggestion) < 0) reviews.push(claim.ReviewSuggestion);
                } else sharedReviews.push({ sources: sources, text: claim.ReviewSuggestion });
            }
        });
        return '<ol class="ci-findings">' + groups.map(function (group) {
            return '<li class="cmf-assistant-text-section cmf-assistant-description-section">'
                + (group.source ? '<h4 class="ci-category">' + escape(group.source) + '</h4>' : '')
                + group.lines.map(function (line) { return '<p class="ci-evidence">' + escape(line) + '</p>'; }).join('')
                + group.reviews.map(function (review) { return '<p class="ci-action"><strong>Suggested review:</strong> ' + escape(review) + '</p>'; }).join('')
                + '<details class="ci-support"><summary>Supporting evidence</summary><ul>' + group.evidence.map(function (reason) {
                    return '<li>' + (group.source ? '' : '<strong>' + escape(reason.Source) + ':</strong> ') + escape(reason.Text) + '</li>';
                }).join('') + '</ul></details></li>';
        }).join('') + '</ol>' + sharedReviews.map(function (review) {
            return '<p class="ci-action"><strong>Suggested review'
                + (review.sources.length ? ' (' + escape(review.sources.join(', ')) + ')' : '') + ':</strong> ' + escape(review.text) + '</p>';
        }).join('');
    }

    function loadSummary(state, refresh) {
        if (!state.overall || !state.evidenceKey) return;
        clearTimeout(state.summaryTimer);
        if (state.summaryController) state.summaryController.abort();
        var serial = state.serial;
        var evidenceKey = state.evidenceKey;
        var controller = state.summaryController = new AbortController();
        var deadline = setTimeout(function () { controller.abort(); }, 30000);
        if (!state.analysis || refresh) state.analysis = { Mode: 'ai-loading', Claims: [] };
        state.summaryStarted = true;
        fetch('CMF_Web_portal.aspx/GetDashboardSummary', {
            method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json; charset=utf-8' }, signal: controller.signal,
            body: JSON.stringify({ evidenceKey: evidenceKey, refresh: !!refresh })
        }).then(function (response) {
            if (!response.ok) throw new Error('Summary unavailable.');
            return response.json();
        }).then(function (payload) {
            if (active !== state || serial !== state.serial || evidenceKey !== state.evidenceKey || !state.host.isConnected) return;
            var analysis = payload.d || payload;
            if (!analysis || !analysis.Success || !Array.isArray(analysis.Claims)
                || ['ai', 'ai-loading', 'ai-unavailable', 'empty'].indexOf(analysis.Mode) < 0) throw new Error('Summary unavailable.');
            state.analysis = { Success: analysis.Success, Mode: analysis.Mode, SourceHash: analysis.SourceHash,
                Claims: analysis.Mode === 'ai' ? analysis.Claims : [] };
            draw(state);
            status(state, '', state.data.Mode === 'ai-loading');
            if (analysis.Mode === 'ai-loading') state.summaryTimer = setTimeout(function () { loadSummary(state, false); }, 3000);
        }).catch(function () {
            if (active !== state || serial !== state.serial || evidenceKey !== state.evidenceKey || !state.host.isConnected) return;
            state.analysis = { Success: false, Mode: 'ai-unavailable', Claims: [] };
            draw(state);
            status(state, '', state.data.Mode === 'ai-loading');
        }).finally(function () { clearTimeout(deadline); });
    }

    function receive(state, data) {
        var unchanged = state.data && state.data.GeneratedAt === data.GeneratedAt
            && state.data.HighlightCount === data.HighlightCount && state.data.GroupCount === data.GroupCount
            && state.data.TrendCount === data.TrendCount && state.data.Mode === data.Mode && state.data.SourceHash === data.SourceHash;
        state.data = data;
        if (unchanged && state.pages) return false;
        if (state.pageController) state.pageController.abort();
        state.pageVersion = (state.pageVersion || 0) + 1;
        state.pageBusy = false;
        state.pageError = '';
        state.pages = {
            highlights: { page: 0, items: data.Highlights || [], total: data.HighlightCount || 0 },
            trends: { page: 0, items: data.TrendFindings || [], total: data.TrendCount || 0 },
            similar: { page: 0, items: data.Groups || [], total: data.GroupCount || 0 }
        };
        return true;
    }

    function pagination(state, view) {
        var current = state.pages[view];
        if (!current.total) return '';
        var size = state.data.PageSize;
        var first = current.total ? current.page * size + 1 : 0;
        var last = Math.min((current.page + 1) * size, current.total);
        return '<nav class="ci-pagination" aria-label="' + (view === 'trends' ? 'Trend' : view === 'similar' ? 'Related issue' : 'Highlight') + ' pages">'
            + '<span>' + first + '-' + last + ' of ' + current.total + (view === 'trends' ? ' trends' : view === 'similar' ? ' relationship groups' : ' highlights') + '</span>'
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
        if (active !== state || !state.host.isConnected) return;
        var content = state.host.querySelector('.ci-content');
        state.host.querySelectorAll('[role="tab"]').forEach(function (tab) {
            var selected = tab.dataset.view === state.tab;
            tab.setAttribute('aria-selected', String(selected));
            tab.tabIndex = selected ? 0 : -1;
        });
        content.setAttribute('aria-labelledby', state.host.id + '-' + state.tab);
        if (!state.data) {
            content.innerHTML = '<p class="ci-empty">Loading dashboard facts...</p>';
            return;
        }
        var data = state.data;
        var preserve = state.renderedTab === state.tab;
        var scroll = preserve ? content.scrollTop : 0;
        var expanded = {};
        if (preserve) content.querySelectorAll('[data-finding-id]').forEach(function (item) {
            expanded[item.dataset.findingId] = Array.from(item.querySelectorAll('details')).map(function (detail) { return detail.open; });
        });
        if (state.scope) state.scope.textContent = data.Scope;
        var rendered = data.Pipeline === 'rendered-dashboard-v4';
        state.host.querySelector('.ci-metadata').textContent = rendered
            ? data.HighlightCount + ' highlights | ' + data.TrendCount + ' trends'
            : data.Records + ' accepted CMF records | ' + data.DistinctIssues
            + ' distinct HSDs | ' + data.DuplicateRecords + ' duplicate rows | ' + data.ExcludedRecords + ' excluded from open balance comparison'
            + (data.CandidateCount == null ? '' : ' | ' + data.CandidateCount + ' calculated groups | ' + data.ReviewedCandidates + ' AI-reviewed groups');
        var updated = new Date(data.GeneratedAt);
        state.host.querySelector('.ci-time').textContent = data.HighlightCount || data.Mode === 'ai'
            ? 'Updated ' + updated.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '';
        state.host.querySelector('.ci-time').dateTime = data.GeneratedAt;
        if (rendered && state.tab === 'summary') {
            content.innerHTML = summaryContent(state.analysis);
        } else if (rendered) {
            content.innerHTML = pagination(state, state.tab) + findings(state.pages[state.tab].items,
                state.tab === 'trends' ? 'No material concentration or directional change in the data.' : 'No material KPI changes in this scope.');
        } else if (state.tab === 'trends') {
            content.innerHTML = data.Mode !== 'ai' ? '<p class="ci-empty">' + (data.Mode === 'ai-loading'
                ? 'Trend review in progress...' : 'Trend review did not complete.') + '</p>' : '<div class="ci-trend-toolbar"><select aria-label="Trend dimension">'
                + ['Component', 'Ingredient', 'Platform'].map(function (name) {
                    return '<option' + (name === state.dimension ? ' selected' : '') + '>' + name + '</option>';
                }).join('') + '</select><span class="ci-small">' + escape(data.TrendCount) + ' groups total</span></div>'
                + '<p class="ci-small ci-comparison">' + escape(data.Comparison) + '</p>'
                + trendTable(data, state.dimension) + '<p class="ci-small">Oldest: days since acceptance among comparable open records.</p>';
            var dimension = content.querySelector('select');
            if (dimension) dimension.addEventListener('change', function (event) { state.dimension = event.target.value; draw(state); });
        } else if (state.tab === 'similar') {
            content.innerHTML = pagination(state, 'similar')
                + findings(state.pages.similar.items, data.Mode === 'ai-loading' ? 'Reviewing technical similarities...'
                    : data.Mode === 'ai-unavailable' ? 'No validated technical similarity findings are available.'
                    : 'AI found no sufficiently supported technical similarity groups in this scope.')
                + '<p class="ci-small">Technical similarity is not proof of a shared root cause.</p>';
        } else {
            content.innerHTML = pagination(state, 'highlights')
                + findings(state.pages.highlights.items, data.Mode === 'ai-loading' ? 'Reviewing issue groups...'
                    : data.Mode === 'ai-unavailable' ? 'No validated AI findings are available.'
                    : data.CandidateCount === 0 ? 'No qualifying multi-field issue groups in this scope.'
                    : 'AI review completed without sufficiently supported insights in this scope.');
        }
        content.setAttribute('aria-busy', String(!!state.pageBusy || data.Mode === 'ai-loading'
            || (state.tab === 'summary' && state.analysis && state.analysis.Mode === 'ai-loading')));
        content.querySelectorAll('[data-finding-id]').forEach(function (item) {
            var opened = expanded[item.dataset.findingId] || [];
            item.querySelectorAll('details').forEach(function (detail, index) { detail.open = !!opened[index]; });
        });
        content.scrollTop = scroll;
        state.renderedTab = state.tab;
        content.querySelectorAll('[data-page-step]').forEach(function (button) {
            button.addEventListener('click', function () {
                loadPage(state, state.tab, state.pages[state.tab].page + Number(button.dataset.pageStep));
            });
        });
        clearTimeout(state.expiryTimer);
        if (data.Mode === 'ai') state.expiryTimer = setTimeout(function () {
            status(state, 'Cached insight expired. Refresh for current evidence.', false, true);
        }, Math.max(0, Date.parse(data.ExpiresAt) - Date.now()));
    }

    function status(state, message, busy, notice) {
        if (active !== state || !state.host.isConnected) return;
        busy = busy || (state.tab === 'summary' && state.analysis && state.analysis.Mode === 'ai-loading');
        state.host.querySelector('.ci-status').textContent = notice ? message
            : !busy && state.data && state.data.Mode === 'ai-unavailable' ? 'Insights could not complete. Refresh to retry.' : '';
        state.host.querySelector('.ci-refresh').disabled = !!busy;
        var loader = state.host.querySelector('.ci-progress');
        loader.hidden = !busy;
        var total = state.data && state.data.CandidateCount || 0;
        var reviewed = state.data && state.data.ReviewedCandidates || 0;
        loader.classList.toggle('ci-progress-indeterminate', !total || !reviewed);
        loader.style.setProperty('--ci-progress', (total ? Math.min(100, reviewed * 100 / total) : 0) + '%');
        loader.setAttribute('aria-valuemin', '0');
        loader.setAttribute('aria-valuemax', String(total || 100));
        if (total) loader.setAttribute('aria-valuenow', String(Math.min(total, reviewed)));
        else loader.removeAttribute('aria-valuenow');
    }

    function request(state, ai, refresh, serial) {
        var controller = state.controller;
        var deadline = setTimeout(function () { controller.abort(); }, 45000);
        return fetch('CMF_Web_portal.aspx/' + (ai ? 'RankDashboardInsights' : 'GetDashboardInsights'), {
            method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/json; charset=utf-8' },
            signal: controller.signal,
            body: JSON.stringify(ai ? { evidenceKey: state.evidenceKey, refresh: refresh }
                : { platform: state.platform, snapshotKey: state.snapshotKey, trendWeeks: state.trendWeeks, chartView: state.chartView })
        }).then(function (response) {
            if (!response.ok) throw new Error('Insights request failed.');
            return response.json();
        }).then(function (payload) {
            if (active !== state || state.serial !== serial || !state.host.isConnected) return null;
            var data = payload.d || payload;
            if (!data || (data.Success !== true && data.Mode !== 'ai-unavailable')) throw new Error(data && data.Message || 'AI insights unavailable.');
            return data;
        }).finally(function () { clearTimeout(deadline); });
    }

    function load(state, refresh) {
        if (state.controller) state.controller.abort();
        if (state.pageController) state.pageController.abort();
        if (state.summaryController) state.summaryController.abort();
        clearTimeout(state.summaryTimer);
        state.analysis = null;
        state.summaryStarted = false;
        state.summaryRefresh = refresh;
        clearTimeout(state.expiryTimer);
        clearTimeout(state.pollTimer);
        state.controller = new AbortController();
        var serial = ++state.serial;
        state.data = null;
        state.host.querySelector('.ci-content').innerHTML = '';
        state.host.querySelector('.ci-content').setAttribute('aria-busy', 'true');
        state.host.querySelector('.ci-time').textContent = '';
        state.host.querySelector('.ci-metadata').textContent = '';
        draw(state);
        status(state, 'Loading server-verified values from this dashboard panel...', true);
        function unavailable(error) {
            if (active !== state || state.serial !== serial || !state.host.isConnected) return;
            if (state.data && (state.data.HighlightCount || state.data.TrendCount)) {
                state.data.Mode = 'ai-unavailable';
                state.data.Message = error.message || 'The remaining review stopped.';
                draw(state);
            } else {
                state.host.querySelector('.ci-content').innerHTML = '<p class="ci-empty">AI insights unavailable.</p>';
            }
            state.host.querySelector('.ci-content').setAttribute('aria-busy', 'false');
            status(state, error.message || 'AI analysis unavailable. Refresh to retry.', false, true);
        }
        function poll(forceRefresh) {
            request(state, true, forceRefresh, serial).then(function (data) {
                if (!data) return;
                if (data.Mode === 'ai' || data.Mode === 'ai-loading' || data.Mode === 'ai-unavailable') {
                    var previousMode = state.data && state.data.Mode;
                    if (receive(state, data) || previousMode !== data.Mode) draw(state);
                    status(state, data.Message, data.Mode === 'ai-loading');
                    if (data.Mode !== 'ai-loading') return;
                    if (data.Pipeline !== 'rendered-dashboard-v4') state.host.querySelector('.ci-metadata').textContent = data.Records + ' accepted records | ' + data.DistinctIssues
                        + ' distinct HSDs | ' + data.CandidateCount + ' ranked groups | ' + data.ReviewedCandidates + ' completed';
                    state.pollTimer = setTimeout(function () { poll(false); }, 3000);
                } else throw new Error('AI analysis returned an incomplete result. Refresh to retry.');
            }).catch(unavailable);
        }
        request(state, false, refresh, serial).then(function (data) {
            if (!data) return;
            state.evidenceKey = data.EvidenceKey;
            receive(state, data);
            if (state.overall && state.tab === 'summary') loadSummary(state, refresh);
            draw(state);
            poll(refresh);
        }).catch(unavailable);
    }

    function render(snapshot) {
        var platform = window.CMF_PORTAL.currentDashboardPlatform;
        var overall = platform === 'CMF_OVERALL_ALL_COMPONENTS_TABLE';
        var host = document.getElementById(overall ? 'homeOverallInsights' : 'homePlatformInsights');
        if (!host) return;
        var key = platform + '|' + (snapshot.InsightsSnapshotKey || snapshot.GeneratedAt);
        if (active && active.host === host && active.key === key) return;
        if (active) {
            if (active.controller) active.controller.abort();
            if (active.pageController) active.pageController.abort();
            if (active.summaryController) active.summaryController.abort();
            clearTimeout(active.summaryTimer);
            clearTimeout(active.expiryTimer);
            clearTimeout(active.pollTimer);
        }
        var state = active = { host: host, scope: document.getElementById(host.id + 'Scope'), key: key, platform: platform,
            snapshotKey: snapshot.InsightsSnapshotKey, trendWeeks: 6, chartView: 'line',
            tab: overall ? 'summary' : 'highlights', overall: overall, dimension: 'Component', serial: 0 };
        if (state.scope) state.scope.textContent = snapshot.PlatformLabel || (overall ? 'Overall' : '');
        host.innerHTML = '<div class="ci-controls"><div class="ci-tabs" role="tablist" aria-label="Insight views">'
            + (overall ? [['summary', 'Summary', 'file-alt']] : []).concat([['highlights', 'Highlights', 'lightbulb'], ['trends', 'Trends', 'chart-line']]).map(function (tab) {
                return '<button type="button" role="tab" id="' + host.id + '-' + tab[0] + '" data-view="' + tab[0]
                    + '" aria-controls="' + host.id + '-content" aria-selected="' + (tab[0] === state.tab) + '">'
                    + '<i class="fas fa-' + tab[2] + '" aria-hidden="true"></i> ' + tab[1] + '</button>';
            }).join('') + '</div><div class="ci-toolbar">'
            + '<button type="button" class="ci-refresh" title="Refresh insights" aria-label="Refresh insights"><i class="fas fa-sync-alt" aria-hidden="true"></i></button></div></div>'
            + '<div class="ci-status" role="status" aria-live="polite"></div>'
            + '<div class="ci-content" role="tabpanel" tabindex="0" id="' + host.id + '-content"></div>'
            + '<div class="ci-footer"><time class="ci-time"></time><p class="ci-metadata"></p></div>'
            + '<div class="ci-progress ci-progress-indeterminate" role="progressbar" aria-label="Insights progress" hidden><span></span></div>';
        host.querySelector('.ci-refresh').addEventListener('click', function () { load(state, true); });
        host.querySelectorAll('[role="tab"]').forEach(function (tab) {
            tab.addEventListener('click', function () {
                state.tab = tab.dataset.view;
                if (state.tab === 'summary' && !state.summaryStarted) loadSummary(state, state.summaryRefresh);
                draw(state);
                if (state.data) status(state, '', state.data.Mode === 'ai-loading');
            });
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