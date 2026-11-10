# CMF Portal Demo

This repository contains an ASP.NET Web Forms CMF portal demo that runs locally with IIS Express. The portal provides platform-scoped CMF dashboards, Issue List and CMF Pending views, AI-assisted summaries/recommendations, Reports & Analytics, and CMF rule configuration.

## What's New in `updated_recommendation`

1. **Modular JSON Configuration & Plugin Architecture (`App_Data/cmf-assistant-config.json` & `App_Code/CmfAssistantConfigService.cs`):**
  - Keeps SQL and the portal-wide GNAI API key/proxy/timeout settings in `Web.config`. GNAI endpoints, primary/fast/fallback models, HSD enrichment, Reports Python path, unused Syncfusion license placeholder, scoring rules, guardrails, feedback and jargon remain in the hot-reloading JSON file. `CmfAssistantConfigService` supplies both sources to existing consumers.
  - **Required Files to Run:** Ensure both **`App_Data/cmf-assistant-config.json`** and **`Web.config`** are present. Configure SQL and shared GNAI credentials in the local `Web.config`; the separate legacy JSON credential override file is no longer read.

2. **6-Category / 4-Point Scoring Mechanism & 5 Gating Guardrails (`App_Code/CmfRecommendationService.cs`):**
   - **4-Point Scale (`1` to `4`):** `Overall Score = (S * 0.20) + (O * 0.15) + (D * 0.15) + (R * 0.10) + (B * 0.20) + (CX * 0.20)`.
   - **5-Tag Disposition Model:**
     - `CMF_OK` (`Approve`): Weighted score `>= 3.50/4` with minimum conditions `B >= 3` and `D >= 3`.
     - `CMF_REVIEW` (`Review`): Weighted score `2.75 - 3.49/4` or capped/escalated by a gating guardrail.
     - `CMF_REJECT` (`Reject`): Weighted score `< 2.75/4` with complete evidence and no mandatory escalation.
     - `CMF_INCOMPLETE` (`Incomplete`): Condition-based override when core metric evidence is missing (Guardrail #3).
     - `CMF_DUPLICATE` (`Duplicate`): Condition-based override when a duplicate sighting or merge ID is identified.
   - **5 Gating Guardrails:**
     - *Low business or narrow trigger cap:* If `B <= 2` or `D <= 2`, capped at `CMF_REVIEW`.
     - *Mandatory escalation:* Safety, security/privacy, data loss/corruption, permanent HW damage, or certification/WHQL blocker is at least `CMF_REVIEW`.
     - *Incomplete evidence:* Missing core category evidence returns `CMF_INCOMPLETE`.
     - *High normal-flow UX exception:* Persistent normal-flow issue with `B = 4`, `CX = 4`, and `D = 4` qualifies as `CMF_OK` when weighted score `>= 3.50` even if `Severity = 3`.
     - *Occurrence discipline:* Uses actual fail/attempt cycles and unit/configuration counts without inflating `Occurrence` for symptom severity.

3. **One-Click Excel (`.xlsx`) Scoring Guidelines Importer (`Config/CMF Rules` Tab):**
   - Upload an updated `CMF_6_Category_Scoring_Mechanism.xlsx` file in the Admin **Config/CMF Rules** tab to automatically parse Sheet 1 (`1-4` rubrics and weights) and Sheet 2 (CMF Disposition Table and Gating Guardrails) via `EPPlus.dll` and update `App_Data/cmf-assistant-config.json`.

4. **AI CMF Assistant Drawer & Scenario Feedback Learning (`CMF Pending` Tab):**
   - **Problem Description (`App_Code/AiSummaryService.cs`):** Enforces `[Component]` in the opening sentence, reconciles conflicting cycle/UUT counts, and restricts `**Repro details:**` strictly to reproduction details (`RVP repro: Yes/No;` + customer/lab reproduction status and builds) without debug/root-cause leakage.
   - **Dedicated `Current Status` Section:** Summarizes candidate fixes, workarounds, and validation status (with promoted issue ID attribution).
   - **Promoted ID Header Link & Click-Only Score Guide Tooltip:** Displays `Promoted ID: <ID/NA>` next to `Sighting ID` and shows the `Score to Tag Guide (Out of 4)` tooltip when clicking the `i` icon.
   - **Mandatory `RVP repro` Reasoning:** Explicitly evaluates and cites `RVP repro` status (`Yes` = platform-wide risk across OEM customer units; `No` = isolated to customer unit without penalizing the 6-category score).
   - **Scenario-Based Feedback Log (`App_Code/CmfFeedbackMemoryService.cs` & `App_Data/cmf-feedback-memory.json`):** Appends every reviewer feedback submission as a reusable scenario lesson (`SCN-...`) for future recommendations.

5. **Global `Ask CMF Pulse` Assistant & Native Executive PowerPoint (`.pptx`) Generator (`App_Code/CmfGlobalAssistantService.cs` & `App_Code/CmfExecutivePptxService.cs`):**
   - Accessible across all portal tabs via the bottom-right **`Ask CMF Pulse`** button (`Analyze. Generate. Assist.`).
   - **Multi-Tab Q&A & Family/SKU Rollups:** Answers questions across Dashboard KPIs, Issue List sightings, CMF Summary TPT/Designs/SW Image Freeze/OEMs/Drivers, CMF Pending items/age buckets, Reports workweeks/closed reasons, and Config rules. Reports both **Family Totals (`NVL (Nova Lake)`)** and **individual SKU breakdowns (`NVL-H`, `NVL-S`, `NVL-Hx` vs `PTL`)**.
   - **3-Layer Hybrid Jargon Scaling:** Combines dynamic SQL discovery + fuzzy Levenshtein matching, LLM semantic mapping, and self-learning synonym persistence via chat, feedback notes, or the Admin UI.
   - **Custom PPT Report Templates & Native 16:9 `.pptx` Generation:** Supports preset and uploaded/saved custom slide blueprints (`App_Data/reports-assistant/pulse-templates.json`) + natural prompts to generate 4-slide widescreen PowerPoint decks using `DocumentFormat.OpenXml` and inline ECharts visuals, plus a dedicated Pulse scenario feedback log (`App_Data/cmf-pulse-feedback.json`).

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
CMF_Web_portal.aspx                  Main Web Forms page and client-side UI
CMF_Web_portal.aspx.cs               Main code-behind logic for tabs, dashboard, grids, AI WebMethods
App_Data/cmf-assistant-config.json   GNAI endpoints/models, HSD, Reports Python, UI integration and feature rules
Web.config                           SQL, shared GNAI token/proxy/timeout plus ASP.NET .NET 4.8 and IIS configuration
Web.config.example                   Deployment template for infrastructure and runtime settings
App_Code/                            Server-side services for AI, CMF recommendations, Ask CMF Pulse, HSD, reports
App_Data/                            Configuration JSONs, feedback memory logs, and generated report artifacts
Content/                             CSS and generated report content
Scripts/                             JavaScript and Python analytics scripts
bin/                                 Runtime assemblies required by the Web Forms app
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

Overall and individual platform dashboards share an Insights panel. Overall adds
a Summary tab before Highlights and Trends; individual platforms retain Highlights
and Trends. The active fact pipeline is `CmfRenderedInsightsService`: inputs are
projected from the same server-owned `HomeDashboardSnapshot` that supplies the
selected panel's KPI cards, component/ingredient summary, pending summary,
and milestone tables. Calendar-week chart buckets are not Insights inputs. Platform selection,
dashboard date filters and Overall exclusions apply. Hidden fields, raw HSD text,
forum enrichment and categorical issue grouping are not inputs.

Highlights is a short briefing grouped into Needs Attention and Progress.
Mixed Signals is a finding label within Needs Attention, not a separate group.
Current pending age and iDST coverage findings also belong in Needs Attention,
without claiming a proven bottleneck. Related measures become one conclusion,
such as Closed totals and Open backlog both being higher. Open, Pending and Closed changes require
both at least two requests and a 10% change (at least two requests when the
previous count is zero). Both components must qualify for a combined closure/backlog finding.
The rule applies before ranking and Overall aggregation; one-request changes are
omitted even when their percentage is large. Flat comparisons, routine totals and redundant
individual signals are suppressed. Each conclusion has a short implication;
numbers and source labels sit in expandable Supporting evidence. Summary
comparisons use only a rolling seven-day lookback: current snapshot values versus
reconstructed end-of-day estimates exactly seven dates earlier. Evidence names
both dates. These are balance/average changes, not event counts or averages for
only cases handled during the last seven days. Short date scopes that hide KPI
comparisons do not supply them.

TPT metrics moving in the same direction share one finding per category in both
platform and Overall Insights. Improvements and degradations remain separate.
Only changes of at least two days and 10% are included (two days when the
previous value is zero). Small or unchanged metrics are omitted before grouping.
Each grouped finding retains the individual
metric comparisons in Supporting evidence. Grouping happens before ranking and
pagination, and Overall preserves the per-platform comparisons without averaging
the different TPT metrics together.

Trends uses Direction Over Time and Concentration. Workload, pending-queue and
milestone distributions are labeled subtypes of Concentration, not extra groups.
Direction Over Time requires supported rolling-window history; two KPI endpoints
alone do not establish a sustained daily trend or reversal.
Only populated groups are rendered. This small, server-owned vocabulary changes
only when a finding represents a genuinely different decision; the ranker cannot
create categories. Concentration shares use the same table's
row population, exclude its Total row and do not count parenthesized LOS subsets
again. Unrecognized cell notation is not guessed. Pending findings can identify
missing iDST contacts and the displayed age-marked portion, without asserting a
cause or a fixed SLA. Missing-contact findings require at least two affected
requests. Age findings require at least two aged requests, at least half the queue,
and a known positive age threshold; zero-day and missing/invalid thresholds do
not produce delay highlights. Titles use actual counts, not "many" or "doubled".
Comparison titles say higher/lower than a week earlier rather than claiming a
sustained trend, acceleration or proven throughput improvement.
Component distributions are current, not historical trends.
Calendar-week flow, week-end snapshots and multi-week chart directions are not
used to infer seven-day findings. The dashboard charts themselves are unchanged.
There is no partial-calendar-week caveat or separate missing-history card.

Each platform makes one short ranking request per changed input/configuration,
plus bounded retries if throttled. The model may return only arrays of known
fact IDs; the server rejects unknown, duplicate or wrong-view IDs and unexpected
response fields. Every Highlights/Trends title, count, explanation and source label comes from
canonical server-generated facts, never model-written prose. No HSD retrieval,
technical-similarity analysis, reasoning, predictions or generated root causes
are performed by the active pipeline. Five findings are shown per page; remaining
findings stay accessible through pagination. Empty briefings skip the model call.
The ranking request includes compact finding summaries rather than full tables.

Overall Summary uses `CmfDashboardSummaryService` and the user-bound
`GetDashboardSummary` endpoint. A background LLM generation request receives only
eligible findings, including titles, summaries, scores, platform/date scope,
current/prior values, dates, units, population denominators where available and
the reconstructed-history flag. Missing denominators remain null, not zero.
The model writes up to three natural-language claims, each with a separate
verified platform scope, one to three cited fact IDs and an optional cautious
review suggestion. There are no C# sentence alternatives or exact prose matches.
Server checks enforce schema, citations, single-platform scope, text limits,
supported dates, numeric values and directly associated units. AI Insights never
shows percentages: Summary prose, review suggestions, Highlights, Trends and
supporting evidence use counts, count differences and days instead. Generation
and verification prompts prohibit percentages; server validation rejects percent
symbols and percentage wording, including written-out values and percentage-point
changes. Internal materiality and ranking thresholds remain unchanged.
A draft remains unpublished in `ai-review`.
A second, separate LLM request checks every assertion against the cited evidence:
factual support, numeric roles (not just matching values), platform scope, units,
comparison direction, estimated-history qualifiers, citation relevance and review
suggestions. One clear history qualifier per paragraph can cover all reconstructed
prior values and their derived changes; it need not repeat "estimated" beside
each number. Summary uses "estimated", never "reconstructed"; server validation
rejects the latter in generated summaries and review suggestions. Unqualified
historical comparisons remain invalid. The verifier
must approve every check; missing, uncertain or negative checks
reject the whole draft. A validation rejection allows one corrected LLM draft
using the same source facts and safe rejection feedback, followed by fresh
verification. Repeated rejection remains unavailable; no draft or C# fallback is
rendered. Provider-level retry and cancellation policies remain unchanged.
Only controlled failure-stage and reason codes are exposed for diagnostics,
never drafts, provider errors or configuration values. Review results are bound to the exact claim text,
citations and source hash, preventing stale approvals. This semantic review is
model-based, not a formal proof; the numeric guards do not attempt to parse all
English. Unsupported causes, sustained trends, ownership bottlenecks and
cross-platform totals must be rejected. Evidence remains canonical source data,
not model-authored prose. Platform subheadings come from verified scope metadata.
Summary never substitutes canonical C# text. During generation it shows a loading
state; invalid or unavailable model output produces an unavailable state with no
claims. Only validated LLM responses can render summary claims. Empty inputs skip
the model call and show an empty state. Successful Summary results are
cached for 30 minutes per source/provider signature; failed results expire after
one minute so reloads do not remain stuck on a long-lived cached failure. Refresh
can retry immediately. Summary requests share the existing
provider pacing, retry and cancellation behavior. Each uncached Summary uses a
generation request followed by a verification request. Only opening Overall Summary
starts this analysis; Highlights and Trends keep their existing behavior.

The browser submits a user-bound snapshot token, not
raw DOM text or client-written metric values. The server validates token expiry,
platform, date and current scope before using its cached data. Legacy chart
arguments remain accepted for API compatibility but do not select weekly data.
Changing chart range/view does not restart or change Insights. Verified briefings are visible
while ranking runs and remain available if ranking fails; only completed rankings
receive the shared AI-ranked label. They are server-derived dashboard findings,
not fallback AI opinions. Caveats appear once under Scope & sources.
Each Insights model request defaults to a 120-second timeout, independently of
other assistants. `AI:InsightsRequestTimeoutSeconds` can set it between 15 and 300
seconds; this is a per-request timeout, not an issue-count or whole-job limit.

Insights model calls share one paced request slot across platform jobs, with a
three-second minimum gap between request chains. HTTP 429 triggers up to three
automatic retries, honoring numeric/date `Retry-After` or otherwise waiting
15, 30 and 60 seconds. Cooldown is shown in the status while the blue loading
strip and validated findings remain visible. Shutdown cancels waits, and a model
change stops old-config requests before retries. A cooldown longer than five
minutes or persistent throttling stops with a quota/rate-limit message rather
than retrying forever. Refresh cannot bypass the shared cooldown. Non-429 errors
and other assistants keep their previous behavior; provider quota exhaustion
still requires a quota reset or administrator action.

Dashboard GNAI model selection is stored in `App_Data/dashboard-ai-config.json`:

```json
{
  "gnaiModel": "claude-sonnet-5-5",
  "gnaiEndpoints": {
    "openai": "https://gnai.intel.com/api/providers/openai/v1/chat/completions",
    "anthropic": "https://gnai.intel.com/api/providers/anthropic/v1/messages"
  }
}
```

Changing only `gnaiModel` to a supported GNAI chat model changes the next backend
AI request without restarting IIS. Claude names select the Anthropic endpoint,
Messages payload and text-block parser. Other supported chat models (GPT,
DeepSeek and Grok) select the OpenAI-compatible endpoint and payload. Endpoint URLs
can be changed in the same JSON; HTTPS is required, and credentials stay in the
existing protected settings. With this JSON present, dashboard requests require
GNAI credentials and never silently substitute a GitHub model. Missing JSON retains
the previous shared provider settings. Malformed JSON and invalid values are rejected.

The override applies to dashboard Insights, executive summaries and blocker
forecasts, not issue assistants. The Claude Sonnet ID above is verified against
the GNAI catalog (`/meta?section=models`). Anthropic's required `max_tokens` is
16384 per request, an output budget rather than an issue/group analysis cap.
Model, endpoint and credential changes invalidate dashboard AI cache keys and
platform warm-up. Already-running Insights jobs stop further work when their
configuration changes; an already-sent provider call may finish, but its changed-
configuration result is discarded. If the panel has finished and stopped polling,
use its Refresh button to request new findings with the saved model.

Platform ranking results are cached by fact content, scope, date and provider
configuration. Polling preserves unchanged pages and updates the ordering when
ranking completes. A thin blue progress bar sits on the bottom edge of Insights
while work is running, takes no layout space and disappears on completion.
There is no normal-state ranking status box or badge. Failures retain the
unranked verified facts and show a notice with Refresh available.

Overall groups compatible findings by topic and direction/category across its
included platforms, with expandable platform-specific evidence. Repeated inputs
from the same platform are deduplicated; opposite directions remain separate.
Within Highlights and Trends, each completed platform ranking contributes the
reciprocal of a finding's position to its grouped topic's rank score. Overall
sorts by the combined rank scores, then fixed priority scores and finding IDs.
Before any ranking completes, the fixed scores provide the preliminary order;
pending or failed rankings contribute no AI rank votes and their findings remain
available. As platform rankings complete, Overall updates its priorities and
pagination without changing the verified finding text or evidence. Unchanged
ordering is possible when AI agrees with the preliminary priorities.
Platform issue totals are not added as unique HSD counts. It reuses cached
platform snapshot inputs/results; missing
inputs are built by the same dashboard snapshot builder used by that platform,
with the same seven-day comparison basis for unvisited panels. Ranking is only
queued for missing or changed platform inputs. Overall Highlights/Trends make no
additional LLM synthesis request; the separate Summary tab uses the bounded,
validated analysis described above. Neither path infers cross-platform root causes or deduplicates
aggregate counts as if they were unique HSD memberships. Pending platform rankings
are visible in its progress, and unranked facts remain available if one fails.
Opening an individual platform no longer warms unrelated platform analyses.

ASP.NET background work runs outside the polling request/session lock. The panel
shows briefing counts and AI-ranking progress separately; the model prioritizes
approved findings but does not write their text. Source preparation
is cached for five minutes and completed analysis for thirty minutes, keyed by
source, platform, prompt version and provider. Refresh bypasses completed AI results
but may reuse the five-minute source cache. Evidence tokens and paginated result
access are bound to the authenticated user. All findings remain accessible through
pages of five; pagination is display-only, not an analysis limit.

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
Count cards compare their displayed current metric with an estimated balance
at the end of the date exactly seven days earlier, not a previous calendar-week total.
Historical reconstruction exclusions do not replace the current value with a lower
eligible-only count. Headline counts and detail tables remain unchanged; accessible
comparison labels disclose historical exclusions. TPT cards also compare today's displayed metric with
its estimated value at the end of the date exactly seven days earlier, in whole days.
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
the earlier balance. Missing or inconsistent dates/status exclude a record from the
historical estimate for that metric and are reported in the accessible comparison label; the current
value remains the card's actual count. Rows accepted after the
historical cutoff remain eligible when their acceptance date establishes a zero earlier
balance, even if later lifecycle dates are incomplete. If all history is unusable, the indicator is blank. Empty cohorts retain
a valid zero change. Removed records, historical eligibility changes, and reopened or
repeated lifecycles cannot be recovered, so these are estimates, not saved snapshots.
Total and Open decreases are green and increases red; Closed and Implemented use the reverse.

TPT compares the displayed current headline with its estimated headline at the end of
the date seven days earlier, using the same selected dashboard scope. It does not
compare weekly cohorts, previous changes, or percentages. Disposition history averages
request-to-decision days for retained decisions made by that date. Resolution history
averages decision-to-implementation days for retained implementations made by that date.
Their existing headline formulas remain unchanged; missing or reversed historical date
pairs are excluded and disclosed. Disposition and Resolution retain their headline's
record eligibility, rather than inheriting Overall's accepted-only population.
Overall history averages estimated `days_active` for accepted unique and duplicate CMFs
already accepted by the comparison date. Rows already closed by that date retain their
recorded durations. For later closures or active rows, subtract the calendar days between
the comparison date and closure or today respectively. This assumes one active day per
calendar day with no pauses or reopenings; it is not an exact historical snapshot and
does not replace recorded durations with ask-to-close intervals. Missing closure dates,
nonnumeric/negative durations, inconsistent dates, and rollbacks exceeding recorded
durations are excluded and disclosed. Actual historical field updates cannot be recovered.
Both TPT averages are truncated to whole days like the headline before subtraction.
Indicators show the absolute integer day difference with a direction arrow: decreases
green, increases red, equal whole-day values a flat blue bar. Valid zero baselines support
integer differences. Missing historical samples leave the indicator blank, not zero.
The headline TPT day values remain unchanged.
Trend indicators do not show hover tooltips. Compared dates, counts, average days,
and estimation caveats remain in their accessible labels.
Avg Resolution has no trend indicator.

The Overall and platform-specific Pending cards also show a count change
between today's displayed pending count (partial day) and the reconstructed pending count
at the end of the date exactly seven days earlier. Both use the selected dashboard
date scope. The previous estimate includes still-pending requests made by that date
and retained accepted, duplicate, or rejected requests decided after that date.
Historical requests are deduplicated by platform and HSD ID; current pending records take
precedence over their component-table copies. Missing or reversed dates exclude the request
from historical reconstruction and are reported in the accessible comparison label. The current side of
the comparison uses the displayed Pending count with its existing row counting.
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
Weekly Closed bars and their click-through details group complete/rejected CMFs
by `closed_date`, not `implemented_date`. Both bar series count unique accepted
CMFs only (`cmf_ok`), excluding duplicate CMFs. Open remains grouped by request
week and counts all requests in that week, including those implemented or closed
within the same week or later. It is an intake count, not the current Open status
or the entire week-end backlog. Missing, invalid and future request dates are
excluded. A same-week opened/closed CMF contributes to both bars but is listed
only once in combined details. The chart and details share these date rules;
details default to unique CMFs only,
with duplicate records available only through the explicit popup filter.
Missing or invalid implementation/closure dates do not suppress a valid Open
intake entry. Missing or invalid closure dates are
excluded from Closed rather than assigned to an implementation week. Summary seven-day
balance comparisons and the Implemented series are unchanged.
The backlog chart's dark-blue contour is independent of the Open and Implemented
legend entries. It follows the sum of the selected areas and disappears when both
are deselected; it does not add a legend item or tooltip total.

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
promoted articles. It targets 2-5 simple bullets, at most 20 words per bullet and
100 words overall, rather than a full investigation history. Relevant next checks are labeled `[Suggestion]`,
not presented as completed debugging. Planned tests and suspected causes stay distinct
from completed tests and confirmed results. The draft is written in simple,
grammatically sound English understandable to a layman or general reviewer,
explaining technical parameters and firmware settings in plain words while
remaining technically accurate. It contains only debugging activity, results,
and supported technical uncertainty; it does not append source-coverage
disclaimers or commentary about retrieval or the feature.

If the model returns an invalid bullet list or exceeds the validated length limit,
the server allows one fresh generation with explicit format feedback and the same
original evidence and the specific format/length failure. Only drafts that pass
the existing plain-bullet validator are shown or cached. Provider failures receive no extra
format-correction attempt, and no fabricated fallback summary is used.

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

> **Important Requirement:** Both **`App_Data/cmf-assistant-config.json`** and **`Web.config`** must be present and configured in the project folder to run the portal.

1. Clone or extract the project to a local folder.

2. Open the project folder in VS Code.

```powershell
code "C:\Path\To\CMF_Portal_Demo"
```

3. Confirm **`Web.config`** is present in the root folder (used for ASP.NET `.NET Framework 4.8` compilation, assemblies, and IIS pipeline settings). If missing, copy it from `Web.config.example`:

```powershell
Copy-Item .\Web.config.example .\Web.config
```

4. Configure shared infrastructure in **`Web.config`**:

- Set the portal-wide `GNAI:ApiKey`, shared proxy and request timeout in `appSettings`.
- Set the named `gfxitt` entry in `connectionStrings`; `Database:ConnectionName` selects that entry.
- Set `gnai.endpoint`, `gnai.anthropicEndpoint`, `gnai.primaryModel`, `gnai.fastSummaryModel` and the `gnai.fallbackModels` array in **`App_Data/cmf-assistant-config.json`**. Keep `hsd`, `reports.pythonExecutable`, `ui.syncfusionLicenseKey`, `plugins`, `scoringRules` and `jargonMappings` there too. JSON `gnai.apiKey` is not read; all GNAI features share the XML token.

The JSON `gnai.fallbackModels` and `hsd.providerOrder` arrays retain their listed order. ASP.NET restarts the application when `Web.config` changes; JSON feature settings retain their existing reload behavior. `ValidationSettings:UnobtrusiveValidationMode` remains in XML because ASP.NET reads it directly.

Do not commit real API keys, tokens, passwords, or internal connection strings. The example contains placeholders only; legacy `cmf-assistant-config.local.json` credential overrides are no longer read.

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

> **Required Configuration Files:** Keep **`Web.config`** for SQL, the portal-wide GNAI API key/proxy/timeout and ASP.NET/IIS configuration. Keep **`App_Data/cmf-assistant-config.json`** for GNAI endpoints, primary/fast/fallback models, HSD settings, Reports Python, UI integration settings, scoring rules, guardrails, feedback and jargon.

### Database (`Web.config`)

The app reads the named SQL connection from `connectionStrings` via `CmfAssistantConfigService.ResolveDatabaseConnectionString()`. `Database:ConnectionName` selects the entry (default `gfxitt`); its `providerName` remains `System.Data.SqlClient`.

The dashboard and issue views resolve platform tables from the single source in `App_Code/CmfPlatformCatalog.cs`, and runtime UI lists are generated from that source.

### AI (`App_Data/cmf-assistant-config.json` and `Web.config`)

AI features use `CmfAssistantConfigService` to read GNAI endpoints and model settings from the JSON `gnai` object:

```json
"gnai": {
  "endpoint": "https://gnai.intel.com/api/providers/anthropic/v1/messages",
  "anthropicEndpoint": "https://gnai.intel.com/api/providers/anthropic/v1/messages",
  "primaryModel": "claude-4-5-sonnet",
  "fastSummaryModel": "claude-4-5-haiku",
  "fallbackModels": ["claude-4-5-haiku"]
}
```

The shared GNAI token, proxy and request timeout remain in XML `appSettings`. Dashboard AI, issue summaries, CMF recommendations, Ask CMF Pulse, HSD AI enrichment and Reports all resolve the same token through `CmfAssistantConfigService.ResolveGnaiToken()`:

```xml
<add key="GNAI:ApiKey" value="REPLACE_WITH_GNAI_TOKEN" />
<add key="GNAI:Proxy" value="" />
<add key="GNAI:RequestTimeoutSeconds" value="30" />
```

The sighting-only fast model is configured separately as `gnai.fastSummaryModel` in `App_Data/cmf-assistant-config.json`; Dashboard model overrides remain in `App_Data/dashboard-ai-config.json`.

AI-powered features include:

```text
Dashboard AI Executive Summary
Dashboard AI Program Health / blocker context
Issue List AI Summary
Issue List Issue Details
CMF Pending AI Recommendation
AI CMF Assessment / Decision Details
Ask CMF Pulse (Global Cross-Tab Assistant & 4-Slide PPTX Generator)

The CMF Assistant checks HSD context on access, with a configurable source cache
(`hsd.cacheRefreshSeconds` in `App_Data/cmf-assistant-config.json`, bounded to 15-300 seconds). Reopen
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

### HSD Enrichment (`App_Data/cmf-assistant-config.json`)

Assistant enrichment settings live in the JSON `hsd` object. The active Dashboard Insights path does not retrieve HSD articles:

```json
"hsd": {
  "enabled": true,
  "providerOrder": ["rest", "gnai"],
  "articleApiBaseUrl": "https://hsdes-api.intel.com/rest/article",
  "timeoutSeconds": 12,
  "cacheRefreshSeconds": 120,
  "skipCommentEndpointsWhenHistoryPresent": true
}
```

If HSD access is unavailable, AI features may still work but with less context.

### Python Reports Assistant

If Reports Assistant needs Python analytics, verify `reports.pythonExecutable` in `App_Data/cmf-assistant-config.json`:

```json
"reports": {
  "pythonExecutable": "C:\\Program Files\\Python314\\python.exe"
}
```

Update this path if Python is installed elsewhere. `ui.syncfusionLicenseKey` preserves the unused Syncfusion license placeholder; the portal currently has no reader that registers it with Syncfusion.

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
gnai.endpoint is correct in App_Data/cmf-assistant-config.json
gnai.primaryModel is correct in the same JSON file
GNAI:ApiKey is populated in Web.config (shared by all portal GNAI features)
Network/proxy access is available
AI:RequestTimeoutSeconds is high enough for your environment
```

### HSD Context Is Missing

Check:

```text
hsd.enabled is true in App_Data/cmf-assistant-config.json
hsd.providerOrder is configured in the same JSON file
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
