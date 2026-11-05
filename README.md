# CMF Portal Demo

This repository contains an ASP.NET Web Forms CMF portal demo that runs locally with IIS Express. The portal provides platform-scoped CMF dashboards, Issue List and CMF Pending views, AI-assisted summaries/recommendations, Reports & Analytics, and CMF rule configuration.

## Prerequisites

Install or confirm the following on your Windows machine:

1. Visual Studio Code
2. .NET Framework 4.8 Developer Pack or runtime
3. IIS Express
4. Access to the configured SQL Server database used by the `gfxitt` connection string
5. Python, only if you want to use the Reports Assistant Python analytics flow
6. Network access to the configured AI and HSD services, if AI features are required

Recommended IIS Express path:

```powershell
C:\Program Files\IIS Express\iisexpress.exe
```

## Project Structure

Important files and folders:

```text
CMF_Web_portal.aspx        Main Web Forms page and client-side UI
CMF_Web_portal.aspx.cs     Main code-behind logic for tabs, dashboard, grids, AI WebMethods
Web.config.example         Template local configuration file
Web.config                 Local runtime configuration file, not safe for secrets in source control
App_Code/                  Server-side services for AI, CMF recommendations, HSD, reports
App_Data/                  Local data/rules/generated report artifacts
Content/                   CSS and generated report content
Scripts/                   JavaScript and Python analytics scripts
bin/                       Runtime assemblies required by the Web Forms app
```

The runtime entry point is `CMF_Web_portal.aspx`.

### Platform Discovery

Platform buttons and Overall aggregation discover populated HSD source tables on
each request, rather than enumerating a fixed platform catalog. Sources must be
`dbo.CMF_<platform>_ALL_COMPONENTS_TABLE` tables with at least one positive numeric
`cp_id`. Empty tables, the synthetic Overall name, and names containing dummy,
backup, archive, temporary, or test tokens are excluded. Table identifiers are
validated before being used in SQL.

New imported platforms become selectable automatically, without code changes or
an application restart. Labels are derived from source names; known metadata only
provides optional display labels, links, and report prompt aliases. Selection
validation, Overall sources, and report platform resolution share the discovered
list. Discovery is cached only within the current request, not across requests.
It is independent of dashboard date filters.

Platform buttons are ordered by descending CMF Summary Open count, then descending
Implemented count, then descending Pending count, then descending Total CMFs count.
Complete ties are alphabetical.
The ranking follows the active dashboard date filter and the Summary's count
definitions, including duplicates. Overall is always last and aligned to the
right edge of the dashboard platform rail when there is available space.
Each fresh page load, including a reload, selects and displays the leftmost
ranked individual platform. Explicit platform selections persist during postbacks.

### Platform Membership

Platform reads use exact comma-separated `processor` tokens, not substring matches.
For example, `novalake_hx` belongs to NVL-Hx and does not match NVL-H;
`arrowlake_hx_816_refresh` belongs to ARL-Refresh rather than ARL-H or ARL-Hx.
`CmfPlatformCatalog.GetProcessorTokens` maps the current platform aliases and
processor variants. New platform codes default to their lower-case table code;
add aliases there when the HSD processor vocabulary differs from that code.

The shared read boundary applies this rule to platform rankings, dashboards,
Overall sources, issue lists and details, Pending, exports, Reports Assistant,
Insights and trends. Existing date, status and `cmf_duplicate` definitions are
unchanged. An issue explicitly tagged with multiple supported processors remains
in each corresponding platform. Blank, null and unassigned-only processor values
are retained because they do not establish a different platform membership.

Pending tables have no processor column, so their reads check matching issue IDs
in the corresponding platform issue table. Known mismatches are excluded;
pending-only records without processor evidence are retained. The filtering is
read-time only: it does not delete database rows, alter imports, or update HSD.

### Dashboard AI Insights

Overall and individual platform dashboards share an Insights panel with Highlights,
Trends, and Similar Issues views. Evidence comes from the canonical Summary source
across all acceptance dates, independent of the dashboard date filter. Platform
selection and Overall platform exclusions still apply.
Only accepted CMFs are included; pending requests retain their separate summaries.

Component (`component_group`), ingredient (`component`), and platform views compare
reconstructed open balances with the same weekday seven days earlier. They reuse
Summary eligibility rules and retain duplicate rows. Weekly comparisons always use
the all-date Insights scope, including when the dashboard displays a short date
range. Aging is measured from acceptance. Counts of
excluded records are shown; historical reopenings cannot be recovered.

Insights uses a hybrid pipeline. All distinct accepted issues participate in local
categorical grouping: Closed (`complete`, `rejected`), Implemented (`implemented`,
`future`), and Open (the Summary complement). Pending requests are excluded.
Duplicate occurrences remain distinct from HSD membership. Invalid historical
dates affect balance metrics, not grouping eligibility.

Each platform is grouped independently using exact categorical tokens from
processor, component group, component/ingredient, family and release. A candidate
requires at least two matching fields and two distinct HSDs. Platform-wide groups,
unknown placeholder values and equivalent memberships are excluded. Candidates
are ranked by categorical specificity, group size and open-issue share; these are
calculated ranks, not model confidence scores. No top-three or total issue/group
limit is applied.

AI reviews every qualifying ranked group using shared classifications, calculated
counts, member titles, dates and available local forum text. It may reject a weak
group, support a useful shared trait, or request rich HSD details for that group.
Only requested group members are fetched, once per platform analysis. Enrichment
selects useful descriptions, summaries, comments, forum, reproduction steps,
investigation and fix text rather than sending the entire provider field/schema
tree. Long selected fields are split into overlapping chunks without dropping
their tails. Requests are batched by size rather than a total issue-count cap;
there is no exhaustive all-pairs observation comparison.
Each Insights model request defaults to a 120-second timeout, independently of
other assistants. `AI:InsightsRequestTimeoutSeconds` can set it between 15 and 300
seconds; this is a per-request timeout, not an issue-count or whole-job limit.

Shared classification traits appear in Highlights and are not presented as proof
of common symptoms or cause. Similar Issues contains only AI-supported technical
comparisons citing specific text from at least two distinct HSDs. The server checks
group coverage and in-scope evidence keys, and supplies source text itself rather
than trusting model-written quotations. This validates provenance, not the truth
of every model interpretation. All counts remain server-derived; no HSD is changed.
Ungrouped issues are not individually reviewed. A scope without qualifying groups
shows that fact without claiming an AI review occurred.

Loading the dashboard automatically queues all catalog platforms, not just the
selected tab. The selected scope starts first; at most two analysis jobs execute
concurrently. Platform tabs share cached jobs with the warm-up path. Overall waits
for all included platform reviews, then feeds their cached findings to AI for
individual and cross-platform shared-trait interpretation; it does not fetch raw
HSDs again. Overall exclusions still apply. A failed platform is reported explicitly
rather than silently omitted from a supposedly complete Overall result.

ASP.NET background work runs outside the polling request/session lock. The panel
shows calculated group counts and AI-review progress separately; calculated
patterns are never substituted for AI opinions on model failure. Source preparation
is cached for five minutes and completed analysis for thirty minutes, keyed by
source, platform, prompt version and provider. Refresh bypasses completed AI results
but may reuse the five-minute source cache. Evidence tokens and paginated result
access are bound to the authenticated user. All findings remain accessible through
pages of eight; pagination is display-only, not an analysis limit.

### Dashboard Date Filter

The controls at the right of the dashboard platform-selector row filter Overall and platform dashboards,
including their charts, summaries, and dashboard drill-downs. Issue List, CMF
Pending, and Reports & Analytics retain their independent scopes.
The date controls align with the AI Insights card's right edge in both Overall
and platform views, including menu expansion/collapse and horizontal scrolling.

The default range is **All dates**. **CMFs Accepted** is the fixed date basis;
there is no date-basis dropdown. Saved sessions also use this basis, regardless
of any previously selected request or submission basis.
It uses `date_cmf_decided` and includes accepted unique and
duplicate CMFs (`cmf_ok`, `cmf_duplicate`), excluding other decision states.
An active range excludes records without a decision date; All Time remains unfiltered.
Dashboard pending-request summaries show no matches for an acceptance-date range,
because pending requests have not been accepted. The CMF Pending tab is unchanged.
Today, Last 7 Days, and Last 30 Days include the current calendar day; Last 12
Months starts on the same calendar date twelve months earlier. Custom Range
includes both selected dates and is applied using the filter icon button.
Calendar boundaries use the web server's local date, consistent with the existing
portal. Date filtering itself does not reconstruct historical status or convert timezones.
Null dates remain visible under All Time and are excluded by an active range.
The selection persists in the session while switching dashboard platforms.
The existing dashboard snapshot's **Updated** date and time appear immediately
to the right of **Mode** in the shared navigation bar and remain visible on every
portal tab.

The header remains outside the workspace's native horizontal scroll container,
so it stays horizontally aligned without JavaScript scroll corrections. It stays
in normal vertical document flow and can scroll out of view.
Only the content column scrolls horizontally; the left menu stays outside it.
The menu uses native sticky positioning, following the header until it reaches
the viewport top, without JavaScript scroll-frame positioning.

### CMF Summary: Weekly Comparisons

Summary Implemented includes `status` values `implemented` and `future`; Open excludes
`complete`, `rejected`, `implemented`, and `future`. This split applies to Overall and
platform counts, duplicate notes, breakdowns, drilldowns, and summary-aligned balance
trends. Total, Closed, and TPT definitions are unchanged. Historical placement of
`future` records uses their retained implementation dates, with the same missing-date
exclusions as `implemented` records.

The same `future` grouping applies to OEM/customer summaries, ingredient/component
and design tables, Issue List counters and visibility, AI Insights Open balances,
analytics chatbot summaries/count answers, and matching drilldowns/exports. Existing
Impl/Verified tables retain `verified` and add `future`. Dated implementation activity
includes `future` only when its retained implementation date meets the existing window.
Stored status values, duplicate eligibility, Pending-request counts, Total, Closed,
and TPT calculations are not rewritten by this grouping policy.

Overall and platform-specific Summaries use the server's local date; today is partial.
Count cards compare today's reconstruction-eligible balance with an estimated balance
at the end of the same weekday seven days earlier. Records excluded from historical
reconstruction are also excluded from today's trend count. Headline card counts and
detail tables remain unchanged, so a trend's current count can be lower than its headline;
the tooltip explains the matching exclusions. TPT cards retain their rolling-week average comparisons.
Platform-specific indicators use only that platform's records, while Overall respects
the platforms included in its scope.

All CMF Summary trend indicators, including Pending and TPT, are hidden when the
applied dashboard date range is shorter than 30 days: Today, Last 7 days, or a
custom range of fewer than 30 inclusive calendar days. All dates, Last 30 days,
Last 12 months, and custom ranges of at least 30 days retain the indicators.
Headline metrics and dashboard charts are unchanged by this visibility rule.

Total CMFs, Closed, Implemented and Open compare current-state counts with reconstructed
historical balances, not weekly activity totals. Both sides use the selected acceptance-date
cohort and retain existing duplicate-row counting. Previous Total uses acceptances by
`date_cmf_decided`; implementation and closure dates place those accepted rows in their
estimated earlier states. A now-closed CMF can therefore count as Implemented or Open in
the earlier balance. Missing or inconsistent dates/status exclude a record from both
trend counts for that metric and are reported in the tooltip. Rows accepted after the
historical cutoff remain eligible when their acceptance date establishes a zero earlier
balance, even if later lifecycle dates are incomplete. If all history is unusable, the indicator is blank. Empty cohorts retain
a valid zero change. Removed records, historical eligibility changes, and reopened or
repeated lifecycles cannot be recovered, so these are estimates, not saved snapshots.
Total and Open decreases are green and increases red; Closed and Implemented use the reverse.

TPT compares the seven calendar days ending today with the preceding seven days,
independently of the dashboard date filter. Disposition TPT compares average
request-to-decision days for decisions in each window.
Resolution TPT compares average decision-to-implementation days for implementations in
each window. Missing or reversed date pairs are excluded from these averages.
Overall TPT compares average current recorded `days_active` for accepted unique and
duplicate CMFs in each window, grouped by `date_cmf_decided`. Missing, nonnumeric or
negative durations are excluded. This compares two acceptance cohorts using today's
recorded durations; it does not reconstruct last week's headline or infer historical
`days_active`, reopenings, or paused time. Its headline remains the existing scoped
average, independent of the trend's cohorts.
TPT indicators show percentage change against the previous average, with the decimal
part truncated for display. Underlying averages and tooltip precision are unchanged.
Changes below 1% display as 0% but retain their actual direction and color: decreases
green, increases red. All equal comparisons use a flat blue bar. Missing TPT cohorts and
undefined percentages against a zero baseline leave the trend indicator blank.
Valid zero-percent changes remain visible, and the headline TPT day values are unchanged.
Hovering an indicator shows the compared dates and their counts or average days.
Avg Resolution has no trend indicator.

The Overall and platform-specific Pending cards also show a count change
between today's eligible pending count (partial day) and the reconstructed pending count
at the end of the same weekday seven days earlier. Both use the selected dashboard
date scope and the same reconstruction-eligible requests. The previous estimate includes still-pending requests made by that date
and retained accepted, duplicate, or rejected requests decided after that date.
Requests are deduplicated by platform and HSD ID on both sides; current pending records take
precedence over their component-table copies. Missing or reversed dates exclude the request
from both trend counts and are reported in the tooltip. The headline Pending count retains
its existing row counting, and the tooltip shows that unchanged headline separately.
Removed records, closures without decision dates, and
reopened/repeated pending periods cannot be reconstructed, so this is not an exact
historical balance. No snapshots or recording tables are created. Increases are red,
decreases green, and equal counts blue. The display has no `Est.` prefix; the
tooltip identifies the historical count as an estimate. Each platform uses its
own pending and decision records. Wholly unusable history and active
acceptance-date filters leave the trend blank. Valid zero changes remain visible.

The Pending card's secondary line shows `Oldest: N days`, calculated in calendar
days from the oldest valid request date among the current scoped pending requests,
not from the last reopening. Missing, invalid and future request dates are excluded.
It shows `Oldest: N/A` when pending requests exist but none have usable dates;
the line is blank when there are no pending requests. The headline and trend are unchanged.

### Dashboard Line Trend: Week-End Balances

Overall and platform tabs share the same SVG-rendered line/bar chart, heading
styling, dark labels, and controls, using each tab's own data. Line charts stack
blue **Open** area above contrasting maroon **Implemented** area. A single blue
upper boundary with a marker at each weekly data point shows
their combined total; there is no separate line between the two areas.
The line tooltip lists Open above Implemented and shows their combined
total to the right of the work-week heading.
Each historical point is a balance at the end of a Monday-Sunday work week, not
the number of changes during that week. The current point uses the summary
cards' current-status counts minus their duplicate counts. Open follows the
card's broad definition: statuses other than complete, rejected, or implemented;
Implemented uses status implemented only. Dashboard charts and their weekly
drilldowns use the selected dashboard platform/date scope, never Issue List
milestone or column-filter sessions. Switching platforms or returning from Issue
List cannot carry a hidden filter into dashboard activity counts, and dashboard
rendering does not change Issue List filter selections.
Historical Open includes earlier requests not yet implemented or closed at the
cutoff, with other active statuses retained in the Open bucket. Historical
Implemented includes implementations before the cutoff that were not yet closed.
Weekly drilldowns use the same cutoff and lifecycle predicates as the line chart.
Each dashboard's title dropdown selects **OPEN BACKLOG TREND BY WORK WEEK**
(line) or **WEEKLY ISSUE FLOW** (clustered bars). Both support 6- and 12-week
ranges. Clustered bars retain their weekly activity counts.

Balances are reconstructed from `date_cmf_ask`, `implemented_date`, and
`closed_date`, assuming a single request-to-implementation-to-closure lifecycle.
They are not historical snapshots: reopened records, previous CMF eligibility,
and missing or incorrect transition dates cannot be reconstructed exactly.
Records without a request date are excluded from historical balances, but the
current point includes them when the summary card does.

### Issue-Count Popup Filters

The Milestone CMF Summary's Total CMFs number opens the milestone issue table
across all milestones, using the current dashboard platform and date filter.
It excludes duplicate CMFs and records without a milestone, matching the total.
The popup and its Excel export share the same query.

Overall summary detail tables resolve ImageFreeze and duplicate ImageFreeze dates
from each issue's source-platform design table and customer detail. Missing
tables, unmatched designs, and empty dates retain the Unknown fallback. Grouped
design metadata prevents additional issue rows from duplicate design matches.

The shared issue-count popup for summary cards, heatmaps, and weekly charts
includes independent **Unique** and **Duplicates** checkboxes, styled like the
Open/Implemented status selectors. Line/bar graph popups open with Unique checked
and Duplicates unchecked, matching the graphs' nonduplicate counts. Other popups
select both by default. Unique selects `cmf_ok`; Duplicates selects
`cmf_duplicate`. Selecting neither returns no issues. The selections apply
before grouping and update the total, group counts, and linked issue lists.
They persist while changing grouping, sorting, or trend status filters.
Pending-card popups omit these controls: active pending requests use pending
request states such as `cmf_ask` and `cmf_incomplete`, rather than the approved
Unique/Duplicates classification. Their counts remain unfiltered by record type.
These popup filters do not change dashboard cards or chart counts.

### Issue List: AI Debug Summary

AI Debug Summary drafts a brief update that a reviewer, owner, or iDST can review
and copy into the sighting's HSD `sysdebug_forum` field. It does not write to HSD.
The existing `sysdebug_forum` value is excluded from summary evidence and debug
follow-up chat; Issue Details and other features retain their existing sources.
The draft uses the most recent retrieved discussion/engineering activity,
test results, and fix/validation notes, including clearly attributed linked
promoted articles. It returns 1-4 simple bullets, at most 100 words, rather than a
full investigation history. Planned tests and suspected causes stay distinct
from completed tests and confirmed results. The draft is written in simple,
grammatically sound English understandable to a layman or general reviewer,
explaining technical parameters and firmware settings in plain words while
remaining technically accurate. It contains only debugging activity, results,
and supported technical uncertainty; it does not append source-coverage
disclaimers or commentary about retrieval or the feature.

The summary uses retrieved HSD text, not attachment contents, and discussion
history may be partial. Debug retrieval keeps the newest five dated comments
among the returned entries without the shared 600-character comment truncation.
REST discussion retrieval examines up to 100 returned entries; plugin retrieval
requests the newest five entries and their own timestamps. Article modification
dates do not establish when tests occurred, and undated/omitted history cannot
guarantee the latest overall activity. These limits constrain factual claims
internally and must not be filled with assumptions. If the AI provider
fails or returns an invalid output format, the feature shows a retryable error
instead of a heuristic summary. The existing summary window is unchanged.

### Issue List: columns and export

The Issue List follows the reference portal's column order. Progress displays the
issue status against the database progress color, with black or white text chosen
for contrast. Title and Status show their full wrapped text in a horizontally
scrollable table. Debug Summary, Issue Details, and Edit appear as icons beneath
each title, with clickable labels on hover or keyboard focus (always labelled on
touch devices). Save/Cancel appear there while editing. There is no separate
Actions column; exports contain title text only, not action labels.
Short-value columns use compact widths; the filter row wraps
on smaller screens, keeping the Platform selector accessible.

Use **Columns** to open **Personalize List Columns**. Move columns between
Available and Selected with the left/right arrows; use the up/down arrows to
order the selected columns. **OK** saves the layout in this browser and applies
it across filtering and paging. **Cancel**, the close button, or Escape discards
unapplied changes. **Reset to column defaults** stages all columns in their
original order; select **OK** to save the reset. At least one column must remain
selected. Existing saved column visibility is retained when first using the
dialog. Row styling, editing, and text wrapping are unchanged.

The **Status** filter selects issue lifecycle states, independently of the debug
updates in the Status column.

**Export** downloads an `.xlsx` workbook containing all rows matching the current
platform, filters, and search, across every page. Only selected data columns are
exported, in table order; UI controls are excluded.

## First-Time Setup

1. Clone or extract the project to a local folder.

2. Open the project folder in VS Code.

```powershell
code "C:\Path\To\CMF_Portal_Demo"
```

3. Create a local `Web.config` file if it does not already exist.

```powershell
Copy-Item .\Web.config.example .\Web.config
```

4. Open `Web.config` and fill in environment-specific values.

At minimum, configure:

```xml
<connectionStrings>
  <add name="gfxitt" connectionString="YOUR_DATABASE_CONNECTION_STRING" providerName="System.Data.SqlClient" />
</connectionStrings>
```

For AI features, configure either GNAI or GitHub Models settings. The current preferred path is GNAI:

```xml
<add key="GNAI:Endpoint" value="https://gnai.intel.com/api/providers/openai/v1/chat/completions" />
<add key="GNAI:Model" value="gpt-5-mini" />
<add key="GNAI:ApiKey" value="YOUR_GNAI_TOKEN" />
```

Do not commit real API keys, tokens, passwords, or internal connection strings.

5. Confirm required assemblies exist in `bin/`.

The app expects its Web Forms dependencies to be available at runtime. If a runtime error mentions a missing assembly, restore or copy the required DLL into `bin/` based on the error message.

Owner display names use the real .NET Framework `System.DirectoryServices.AccountManagement` assembly configured in `Web.config`. Corporate directory access is required to resolve account aliases. CMF Pending preserves the original owner value if resolution is unavailable; do not replace the directory implementation with a no-op stub or infer names from aliases.

## Run Locally With IIS Express

Open a PowerShell terminal in the project root and run:

```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\Path\To\CMF_Portal_Demo" /port:8080
```

Replace `C:\Path\To\CMF_Portal_Demo` with your actual local path.

For this workspace, the command would look like:

```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\Users\vavnss\OneDrive - Intel Corporation\Desktop\CMF_Portal_Demo" /port:8080
```

Then open:

```text
http://localhost:8080/CMF_Web_portal.aspx
```

You can also open the root URL:

```text
http://localhost:8080/
```

## If Port 8080 Is Already Used

Use another port, for example `8081`:

```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\Path\To\CMF_Portal_Demo" /port:8081
```

Then open:

```text
http://localhost:8081/CMF_Web_portal.aspx
```

To check what is using port 8080:

```powershell
netstat -ano | findstr :8080
```

To stop an existing IIS Express process:

```powershell
taskkill /F /IM iisexpress.exe
```

## Quick Health Check

After starting IIS Express, verify that the page compiles and responds:

```powershell
curl.exe -I http://localhost:8080/CMF_Web_portal.aspx
```

A healthy response should include:

```text
HTTP/1.1 200 OK
```

If you receive `500 Internal Server Error`, capture the detailed ASP.NET error page:

```powershell
$resp = curl.exe -s http://localhost:8080/CMF_Web_portal.aspx
$resp | Out-File -FilePath page500.html -Encoding utf8
Select-String -Path page500.html -Pattern 'Parser Error Message|Compiler Error Message|Exception Details|CS[0-9]{4}' -CaseSensitive:$false
```

## Portal Entry Points

Main local page:

```text
http://localhost:8080/CMF_Web_portal.aspx
```

Primary views:

```text
Dashboard
Issue List
CMF Pending List
Reports & Analytics
Config/CMF Rules, visible in admin mode only
```

The portal resolves platform options dynamically from the shared platform catalog.

## Evidence-Led CMF Recommendations

Recommendation policy was changed on October 30, 2026 to assess fix ownership from technical evidence. The meanings below are the product-owner-specified meanings, not the previous score-band approval meanings:

- `CMF_OK`: evidence supports an OEM design/implementation fault that the OEM must correct.
- `CMF_REJECT`: evidence supports an Intel design/component/firmware/software fault that Intel must correct. This does not mean the problem is harmless or low severity.
- `CMF_REVIEW`: evidence is inconclusive/conflicting, ownership cannot be established, the two assessment passes disagree, or an AI/evidence check fails. Human judgment is required.
- `CMF_INCOMPLETE`: identified decision-critical factual inputs are genuinely absent. The initial assessment and evidence-review pass must corroborate the gap and explain its decision relevance. Missing causal analysis alone is review, not incomplete.

All six risk metrics remain part of the assessment and retain their weights. The weighted score is advisory: no numeric threshold or CX override assigns the tag. A score needs an exact source excerpt; an unverifiable metric is shown as `N/A`, not a fabricated low score. A complete overall score is shown only when all six metrics have supported numeric values. A citation-check failure does not establish that the source information is absent. Detection distinguishes spontaneous ordinary-use/idle failures (5), ordinary daily transitions such as resume (4), less-frequent workflows (3), stress-only cases (2), and special/debug-only setups (1). Visibility alone does not turn a triggered resume failure into level 5.

The service requests a structured assessment and then a separate evidence-review call using the same configured provider/model. This is a second AI check, not independent human verification. Final ownership tags require a quoted causal finding or a controlled comparison identifying responsibility; single-OEM scope, RVP reproduction, assigned contacts and component names alone are not sufficient. Reasoning remains short, complete, evidence-based bullets, with the final bullet explaining the tag rather than a score threshold. Model-proposed tags, risk considerations, citations and review status remain available in the response for inspection. The recommendation drawer exposes source-matched technical excerpts beneath the reasoning, HTML-escaped and only for completed evidence reviews. Finding an exact quote confirms its presence, not the truth or interpretation of the underlying finding.

Only recommendations use the full technical text returned by the HSD service, without the additional shared-summary truncation. Source retrieval can still omit older comments, attachment contents or other records. Bounded-source incompleteness therefore requires an explicit source statement supporting the absence, not merely a blank field or omitted history; failed HSD retrieval requires manual review instead of an asserted absence. Scores, ownership and absence claims remain fallible model judgments even when their quoted text exists. All tags remain recommendations requiring reviewer confirmation; no saved decision is updated automatically.

Both assessment passes must corroborate the same factual gap type and field, not just reuse a field name. A bounded-source absence quote must also address that fact type: a statement about missing reproduction conditions cannot confirm missing diagnostics. This conservative topic check uses English terminology and may require manual review for unfamiliar wording. The incomplete-reasoning guard rejects direct assertions of unrelated missing log, recovery, or business-impact information when no such critical gap was confirmed; inability to assess a risk is not by itself a missing-field claim. These conservative checks request manual review when validation fails rather than asserting ownership or evidence absence. Neither wording check is a comprehensive semantic verifier. The model contract specifies string fields and boolean confirmation flags; malformed value types also require manual review rather than silent repair.

Local deterministic regression checks (no model/database calls):

```powershell
(Invoke-WebRequest -UseBasicParsing -Uri 'http://localhost:8080/others/test_scripts/cmf_recommendation_contract.aspx').Content
```

The local-only test page also provides fixed synthetic end-to-end cases. The harness is in the existing Git-ignored `others/` folder and is not included in the versioned deployment. A POST intentionally makes model calls; use `oem`, `intel`, `review`, or `incomplete` as the scenario. `Match` reports tag agreement plus completed evidence review; `MetricMatrixMatches` separately reports agreement with the fixture's supporting metric rubric, so correct classification does not hide a scoring error:

```powershell
(Invoke-WebRequest -UseBasicParsing -Uri 'http://localhost:8080/others/test_scripts/cmf_recommendation_contract.aspx?scenario=oem' -Method POST).Content
```

These fixtures test policy behavior, not real-world accuracy. The earlier six historical examples must be re-evaluated with reviewer-confirmed ownership findings and decision-time evidence before claiming historical agreement or expanding reliance on recommendations. The earlier four-sighting comparison report describes the previous score-led implementation and is not an accuracy result for this policy. A broader, adjudicated, unseen evaluation set is needed to compare GPT, Gemini and Claude reliably. Provider/model configuration, SQA logic, database decisions and normal decision workflows are not changed by this recommendation policy.

## Configuration Notes

### Database

The app reads database settings from the `gfxitt` connection string in `Web.config`.

The dashboard and issue views resolve platform tables from the single source in `App_Code/CmfPlatformCatalog.cs`, and runtime UI lists are generated from that source.

### AI

AI features use settings from `Web.config`. GNAI is preferred when these values are configured:

```xml
<add key="GNAI:Endpoint" value="..." />
<add key="GNAI:Model" value="..." />
<add key="GNAI:ApiKey" value="..." />
```

If GNAI is not configured, the services can fall back to GitHub Models settings when available.

AI-powered features include:

```text
Dashboard AI Executive Summary
Dashboard AI Program Health / blocker context
Issue List AI Summary
Issue List Issue Details
CMF Pending AI Recommendation
AI CMF Assessment / Decision Details

The CMF Assistant checks HSD context on access, with a 60-second source cache
by default (`CMF:AssistantHsdRefreshSeconds`, bounded to 15-300 seconds). Reopen
the assistant after that window to pick up changes; an already-open drawer is
not automatically pushed updates. The combined recommendation context no
longer adds another ten-minute cache window.

Successful model-generated recommendation, description, impact, and quality
assessment responses are saved as JSON under `App_Data/cmf-assistant-cache`.
Their keys include the HSD source fingerprint, database/request context, model
configuration, and the section's prompt version. Unchanged inputs reuse the
saved response across application restarts. Changed source data or a supplied
HSD last-modified timestamp/revision produces a new key and a fresh response.
The fingerprint covers the full article data returned by the provider, before
prompt-display truncation; it cannot detect data the provider does not return.
HSD extraction requests verbatim fields and newest-first comments. Direct
REST/MCP source data is preferable to model-mediated extraction when available.

Fallback/error results are not persisted and are retried after a short memory
cache. HSD failures do not replace a verified saved result, but the assistant
may show a temporary database-only fallback rather than claim HSD is current.
The application identity needs write permission on this App_Data subfolder;
disk-cache failures are traced and do not prevent ordinary generation. Files
contain internal sighting content: apply the same access controls, backup, and
retention policy as other App_Data content. There is no automatic file expiry;
delete this subfolder to purge saved responses (and restart the app to clear
memory caches). Follow-up chat and dashboard caches are unchanged.
Reports Assistant
```

### HSD Enrichment

HSD-related settings live in `Web.config`:

```xml
<add key="HSD:Enabled" value="true" />
<add key="HSD:ProviderOrder" value="rest,gnai" />
<add key="HSD:ArticleApiBaseUrl" value="https://hsdes-api.intel.com/rest/article" />
<add key="HSD:UseDefaultCredentials" value="true" />
```

If HSD access is unavailable, AI features may still work but with less context.

### Python Reports Assistant

If Reports Assistant needs Python analytics, verify the configured Python executable:

```xml
<add key="Python:Executable" value="C:\Program Files\Python314\python.exe" />
```

Update this path if Python is installed elsewhere.

## Common Development Workflow In VS Code

1. Open the project folder in VS Code.
2. Edit `.aspx`, `.aspx.cs`, `App_Code`, `Content`, or `Scripts` files as needed.
3. Keep IIS Express running in a terminal.
4. Refresh `http://localhost:8080/CMF_Web_portal.aspx` after changes.
5. Run the curl health check after code-behind or markup changes.
6. Use the ASP.NET 500 capture command to diagnose compile/runtime errors.

ASP.NET Web Forms compiles dynamically at runtime, so a browser or curl check is often more reliable than editor-only diagnostics for `.aspx` pages.

## Troubleshooting

### IIS Express Is Not Found

Confirm IIS Express is installed and check this path:

```powershell
Test-Path "C:\Program Files\IIS Express\iisexpress.exe"
```

If it returns `False`, install IIS Express or adjust the command to the installed location.

### Port Already In Use

Use another port or stop the existing IIS Express process:

```powershell
taskkill /F /IM iisexpress.exe
```

Then restart IIS Express.

### 500 Internal Server Error

Capture the server error details:

```powershell
$resp = curl.exe -s http://localhost:8080/CMF_Web_portal.aspx
$resp | Out-File -FilePath page500.html -Encoding utf8
Select-String -Path page500.html -Pattern 'Parser Error Message|Compiler Error Message|Exception Details|CS[0-9]{4}' -CaseSensitive:$false
```

Common causes:

```text
Missing server control referenced by code-behind
Invalid ASPX markup
Missing DLL in bin/
Invalid Web.config setting
Database connection failure
AI/HSD endpoint access issue
```

### Database Connection Fails

Check:

```text
The gfxitt connection string is present
Your account has access to the SQL Server/database
VPN or corporate network access is active if required
The target platform tables exist
```

### AI Buttons Fail Or Return Fallback

Check:

```text
GNAI:Endpoint is correct
GNAI:Model is correct
GNAI:ApiKey is populated
Network/proxy access is available
AI:RequestTimeoutSeconds is high enough for your environment
```

### HSD Context Is Missing

Check:

```text
HSD:Enabled is true
HSD:ProviderOrder is configured
HSD:UseDefaultCredentials is correct for your environment
HSD API or GNAI HSD provider is reachable
```

## Generated Files

During development, runtime checks and report generation may create local artifacts such as:

```text
page*.html
rendered*.html
portal-after-*.html
App_Data/reports-assistant/*.csv
Content/generated-reports/*
```

These are development or generated outputs and generally should not be committed unless intentionally needed for a demo artifact.

## Recommended Local Validation Before Sharing Changes

Run:

```powershell
curl.exe -I http://localhost:8080/CMF_Web_portal.aspx
```

Confirm:

```text
HTTP/1.1 200 OK
```

Then manually verify the main flows in the browser:

```text
Dashboard loads for the selected/default platform context
Platform chips switch platform data
Overall dashboard summary cards, trends, OEM table, and heatmap load
CMF Summary loads for an individual platform
Issue List loads and filters work
Issue List paging and filters retain a consistent issue count
Issue AI Summary and AI Issue Details open
CMF Pending List loads
CMF Pending KPI cards, search, component/owner/customer/iDST filters, and Reset work
CMF Pending AI Insights opens for the visible rows
CMF Pending AI Recommendation, sighting quality assessment, and impact details open
Switching between Dashboard, Issue List, and CMF Pending hides the inactive views
Reports & Analytics opens
Admin Rules tab appears only in admin mode
```

The dashboard and list views share the same page and code-behind. When integrating
dashboard changes, preserve the list markup, client-side helpers, and matching
WebMethods together; a merge can compile while still removing visible features.
Platform handling must support both individual platforms and the Overall view.

To compile the current portal without the archived test pages:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -v / -p "$PWD" -x /others -nologo
```

The archived pages under `others/` are not runtime entry points and may reference
old code-behind files or controls that are no longer available.

With Node.js installed, run the dependency-free drawer integration regression tests:

```powershell
node --test .\Scripts\tests\portal-integration.test.js
```

These tests cover drawer-mode visibility, Pending Insights success/error responses,
and impact details without contacting the AI service.
