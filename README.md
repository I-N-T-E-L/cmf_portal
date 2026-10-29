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

### Dashboard Date Filter

The controls beside the dashboard greeting filter Overall and platform dashboards,
including their charts, summaries, and dashboard drill-downs. Issue List, CMF
Pending, and Reports & Analytics retain their independent scopes.

The defaults are **All Time** and **CMF Request Date** (`date_cmf_ask`).
**Issue Submission Date** uses the database's `submitted_date` instead.
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
Implemented uses status implemented only. The line uses the same platform/date
scope as the cards, without the activity chart's additional driver/column filters.
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

Overall summary detail tables resolve ImageFreeze and duplicate ImageFreeze dates
from each issue's source-platform design table and customer detail. Missing
tables, unmatched designs, and empty dates retain the Unknown fallback. Grouped
design metadata prevents additional issue rows from duplicate design matches.

The shared issue-count popup for summary cards, heatmaps, and weekly charts
includes independent **Unique** and **Duplicates** checkboxes, styled like the
Open/Implemented status selectors. Both are
selected whenever a popup opens. Unique selects `cmf_ok`; Duplicates selects
`cmf_duplicate`. Selecting neither returns no issues. The selections apply
before grouping and update the total, group counts, and linked issue lists.
They persist while changing grouping, sorting, or trend status filters.
Pending-card popups omit these controls: active pending requests use pending
request states such as `cmf_ask` and `cmf_incomplete`, rather than the approved
Unique/Duplicates classification. Their counts remain unfiltered by record type.
These popup filters do not change dashboard cards or chart counts.

### Issue List: AI Debug Summary

AI Debug Summary produces a single list of plain-English bullets describing the
debugging documented for the sighting and its linked promoted HSD articles:
tests performed, findings, and recorded fix or validation outcomes. Suspected
causes and planned work must remain distinct from confirmed results. It does not
generate risk, escalation, decision, or next-action subsections.

The summary uses retrieved HSD text, not attachment contents, and discussion
history may be partial. Missing debugging evidence or article retrieval failures
must be acknowledged rather than filled with assumptions. If the AI provider
fails or returns an invalid output format, the feature shows a retryable error
instead of a heuristic summary. The existing summary window is unchanged.

### Issue List: columns and export

The Issue List follows the reference portal's column order. Progress displays the
issue status against the database progress color, with black or white text chosen
for contrast. Title and Status show their full wrapped text in a horizontally
scrollable table. Short-value columns use compact widths; the filter row wraps
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
exported, in table order; the Actions column and UI controls are excluded.

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
