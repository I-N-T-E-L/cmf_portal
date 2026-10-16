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
Issue List loads and filters work
Issue AI Summary opens
CMF Pending List loads
CMF Pending AI Recommendation opens
Reports & Analytics opens
Admin Rules tab appears only in admin mode
```
