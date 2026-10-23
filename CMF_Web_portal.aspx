<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CMF_Web_portal.aspx.cs" Inherits="CMF_Web_portal" EnableEventValidation="false" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <!-- Required meta tags -->
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <title>CMF Web Portal</title>

    <!-- Stylesheets -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.2.1/css/all.min.css" type="text/css" />
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800;900&family=Manrope:wght@400;500;600;700;800&family=Outfit:wght@500;600;700;800&display=swap" />
    <link rel="stylesheet" href="Content/cmf-portal.css">

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <!-- Bootstrap CSS -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" rel="stylesheet">

    <!-- Bootstrap JS -->
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <!-- ECharts for Reports & Analytics charts -->
    <script src="https://cdn.jsdelivr.net/npm/echarts@5.5.0/dist/echarts.min.js"></script>
    
    <!-- Markdown rendering library -->
    <script src="https://cdn.jsdelivr.net/npm/marked@11.0.0"></script>
    <!-- HTML sanitization library -->
    <script src="https://cdn.jsdelivr.net/npm/dompurify@3.0.6/dist/purify.min.js"></script>

    <style>
        :root {
            --portal-bg: #edf3f8;
            --portal-surface: #ffffff;
            --portal-surface-soft: #f7fafc;
            --portal-ink: #19324a;
            --portal-ink-muted: #5c7087;
            --portal-primary: #0f5ea8;
            --portal-primary-deep: #083d74;
            --portal-accent: #00a6b2;
            --portal-border: #cdd9e5;
            --portal-shadow: 0 18px 42px rgba(17, 43, 70, 0.12);
            --portal-radius: 18px;
            --portal-font-body: "Plus Jakarta Sans", "Segoe UI", "Trebuchet MS", sans-serif;
            --portal-font-display: "Outfit", "Manrope", "Segoe UI", sans-serif;
            --portal-font-card: "Manrope", "Plus Jakarta Sans", "Segoe UI", sans-serif;
        }

        body {
            overflow-x: hidden;
            font-family: var(--portal-font-body);
            margin: 0;
            padding: 0;
            color: var(--portal-ink);
            background:
                radial-gradient(circle at top left, rgba(0, 166, 178, 0.10), transparent 24%),
                radial-gradient(circle at top right, rgba(15, 94, 168, 0.12), transparent 28%),
                linear-gradient(180deg, #f4f8fb 0%, var(--portal-bg) 100%);
        }

        h1,
        h2,
        h3,
        h4,
        h5,
        h6,
        .ccip-dash-greeting,
        .ccip-cmf-summary-title,
        .ccip-card-title,
        .ccip-card-mini-title,
        .overall-card-title {
            font-family: var(--portal-font-display);
            letter-spacing: 0.01em;
        }

        /* Markdown content styles */
        @keyframes shimmer {
            0% { background-position: -468px 0; }
            100% { background-position: 468px 0; }
        }
        .ai-skeleton-loader {
            display: flex;
            flex-direction: column;
            gap: 15px;
            padding: 10px 0;
            width: 100%;
        }
        .ai-skeleton-line {
            height: 16px;
            background: #f6f7f8;
            background-image: linear-gradient(to right, #f6f7f8 0%, #edeef1 20%, #f6f7f8 40%, #f6f7f8 100%);
            background-repeat: no-repeat;
            background-size: 800px 100%;
            animation-duration: 1.5s;
            animation-fill-mode: forwards;
            animation-iteration-count: infinite;
            animation-name: shimmer;
            animation-timing-function: linear;
            border-radius: 4px;
        }
        .ai-skeleton-title { width: 40%; height: 20px; }
        .ai-skeleton-label { width: 25%; }
        .ai-skeleton-value { width: 85%; }

        .ai-sk-block {
            display: flex;
            flex-direction: column;
            gap: 8px;
            min-width: 0;
        }
        .ai-sk-row {
            display: flex;
            gap: 12px;
            align-items: center;
        }
        .ai-semantic-badge {
            display: inline-flex;
            padding: 2px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: 600;
            white-space: normal;
            word-wrap: break-word;
        }
        .badge-green { background: #E6F4EA; color: #1e5a2f; border: 1px solid #B7DEc5; }
        .badge-amber { background: #FFF4CE; color: #8F6200; border: 1px solid #FDE08B; }
        .badge-red { background: #FDE8E9; color: #9A242B; border: 1px solid #F9C3C6; }
        .badge-neutral { background: #f0f2f5; color: #444; border: 1px solid #e0e2e5; }

        .issue-details-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 12px;
            margin-bottom: 20px;
        }
        .issue-details-grid-full {
            grid-column: 1 / -1;
        }
        .idg-label {
            font-size: 11px;
            color: #5c7087;
            text-transform: uppercase;
            font-weight: 700;
            letter-spacing: 0.5px;
            margin-bottom: 4px;
        }
        .idg-val {
            font-size: 13px;
            color: #19324a;
            min-width: 0;
            max-width: 100%;
            word-break: break-word;
            overflow-wrap: break-word;
            white-space: normal;
        }
        .markdown-content {
            word-wrap: break-word;
            overflow-wrap: break-word;
        }

        .markdown-content h1,
        .markdown-content h2,
        .markdown-content h3,
        .markdown-content h4,
        .markdown-content h5,
        .markdown-content h6 {
            margin-top: 16px;
            margin-bottom: 8px;
            font-weight: 700;
            color: var(--portal-ink);
        }

        .markdown-content h1 { font-size: 1.8rem; }
        .markdown-content h2 { font-size: 1.5rem; }
        .markdown-content h3 { font-size: 1.3rem; }
        .markdown-content h4 { font-size: 1.1rem; }
        .markdown-content h5 { font-size: 1rem; }
        .markdown-content h6 { font-size: 0.95rem; }

        .markdown-content strong,
        .markdown-content b {
            font-weight: 700;
            color: var(--portal-ink);
        }

        .markdown-content em,
        .markdown-content i {
            font-style: italic;
        }

        .markdown-content p {
            margin: 6px 0;
            line-height: 1.4;
        }

        .markdown-content ul,
        .markdown-content ol {
            margin: 4px 0;
            padding-left: 24px;
        }

        .markdown-content li {
            margin: 2px 0;
            line-height: 1.4;
        }

        .markdown-content code {
            background: #f0f4f8;
            padding: 2px 6px;
            border-radius: 4px;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
            color: #d63384;
        }

        .markdown-content pre {
            background: #f0f4f8;
            border: 1px solid var(--portal-border);
            border-radius: 8px;
            padding: 12px;
            overflow-x: auto;
            margin: 8px 0;
        }

        .markdown-content pre code {
            background: transparent;
            padding: 0;
            color: var(--portal-ink);
        }

        .markdown-content blockquote {
            border-left: 4px solid var(--portal-primary);
            padding-left: 12px;
            margin: 8px 0;
            color: var(--portal-ink-muted);
            font-style: italic;
        }

        .markdown-content a {
            color: var(--portal-primary);
            text-decoration: none;
        }

        .markdown-content a:hover {
            text-decoration: underline;
        }

        .markdown-content hr {
            border: none;
            border-top: 1px solid var(--portal-border);
            margin: 12px 0;
        }

            .cmf-rec-reason-list {
                margin: 6px 0 8px;
                padding-left: 18px;
                display: grid;
                gap: 5px;
            }

            .cmf-rec-reason-list li {
                color: #24364a;
                line-height: 1.45;
            }

        header {
            z-index: 100;
            min-height: 88px;
            display: flex;
            justify-content: center;
            align-items: center;
            background: linear-gradient(120deg, var(--portal-primary-deep) 0%, var(--portal-primary) 48%, var(--portal-accent) 100%);
            color: white;
            padding: 20px 30px;
            box-shadow: 0 14px 35px rgba(8, 61, 116, 0.28);
            position: sticky;
            top: 0;
        }

        .header-title {
            font-size: 1.2rem;
            font-weight: 700;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            text-align: center;
            z-index: 110;
            text-shadow: 0 1px 12px rgba(0, 0, 0, 0.16);
        }

        .header-user-mode {
            position: absolute;
            right: 72px;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            z-index: 111;
            color: #eaf3ff;
            font-size: 12px;
            font-weight: 600;
        }

        .header-mode-dropdown {
            height: 34px;
            min-width: 165px;
            border-radius: 8px;
            border: 1px solid rgba(255, 255, 255, 0.45);
            background: rgba(9, 46, 84, 0.48);
            color: #ffffff;
            padding: 0 10px;
        }

        .header-mode-dropdown option {
            color: #1f2d3d;
        }

        .portal-layout-shell {
            display: flex;
            align-items: flex-start;
            gap: 14px;
            margin-top: 0;
            min-height: calc(100vh - 150px);
        }

        .portal-layout-shell.main-menu-collapsed {
            gap: 14px;
        }

        .portal-left-nav {
            width: 164px;
            flex: 0 0 164px;
            position: sticky;
            top: 88px;
            background: linear-gradient(180deg, #061947 0%, #071e59 48%, #03102e 100%);
            border-radius: 0 0 12px 12px;
            padding: 10px 8px 14px;
            box-shadow: 0 12px 28px rgba(3, 16, 46, 0.30);
            min-height: calc(100vh - 88px);
            max-height: calc(100vh - 88px);
            align-self: flex-start;
            transition: width 0.22s ease, flex-basis 0.22s ease, padding 0.22s ease, opacity 0.2s ease;
            overflow-y: auto;
            overflow-x: hidden;
            z-index: 90;
        }

        .portal-left-nav.is-collapsed {
            width: 72px;
            flex: 0 0 72px;
            min-height: calc(100vh - 88px);
            padding: 8px 6px 12px;
            opacity: 1;
            box-shadow: 0 10px 22px rgba(3, 16, 46, 0.24);
            border: 1px solid rgba(123, 165, 227, 0.26);
            pointer-events: auto;
        }

        .portal-left-nav-title {
            color: #ffffff;
            font-size: 17px;
            font-weight: 800;
            margin: 0 6px 18px;
            letter-spacing: 0.5px;
            text-transform: uppercase;
            padding: 8px 6px;
            border-bottom: 2px solid rgba(15, 110, 184, 0.4);
            display: flex;
            align-items: center;
        }

        .portal-section-header {
            display: flex;
            align-items: center;
            justify-content: flex-end;
            gap: 10px;
            margin: 0 0 8px;
            padding: 0;
            border-bottom: none;
        }

        .portal-section-header .portal-left-nav-title {
            margin: 0;
            padding: 0;
            border-bottom: none;
        }

        .portal-collapse-btn {
            border: 1px solid rgba(159, 195, 231, 0.55);
            background: rgba(255, 255, 255, 0.12);
            color: #eaf4ff;
            border-radius: 999px;
            font-size: 13px;
            font-weight: 900;
            min-height: 28px;
            min-width: 38px;
            padding: 4px 8px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
        }

        #mainMenuToggleBtn {
            min-width: 30px;
            min-height: 30px;
            width: 30px;
            height: 30px;
            padding: 0;
            border: 1px solid rgba(173, 206, 240, 0.72);
            background: rgba(255, 255, 255, 0.16);
            font-size: 12px;
            box-shadow: 0 5px 12px rgba(4, 23, 54, 0.28);
        }

        .portal-collapse-btn:hover {
            background: rgba(15, 110, 184, 0.28);
            border-color: rgba(159, 195, 231, 0.9);
        }

        .portal-collapse-btn .toggle-chevron { line-height: 1; }

        .collapsible-body.is-collapsed {
            display: none !important;
        }

        .portal-left-nav .portal-nav-link {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 9px;
            margin: 0 0 7px;
            border-radius: 9px;
            color: #f4f8fc;
            text-decoration: none;
            font-size: 12px;
            font-weight: 750;
            background: rgba(255, 255, 255, 0.06);
            border: 1px solid rgba(137, 173, 255, 0.15);
            text-align: left;
            transition: all 0.2s ease-in-out;
            position: relative;
            overflow: hidden;
        }

        .portal-left-nav .portal-nav-link:before {
            content: '';
            position: absolute;
            left: 0;
            top: 0;
            width: 3px;
            height: 100%;
            background: linear-gradient(180deg, #0f6eb8 0%, #0a4a7a 100%);
            transform: scaleY(0);
            transform-origin: top;
            transition: transform 0.2s ease-in-out;
        }

        .portal-left-nav .portal-nav-link:hover {
            background: rgba(59, 130, 246, 0.34);
            color: #ffffff;
            text-decoration: none;
            border-color: rgba(147, 197, 253, 0.42);
            box-shadow: 0 6px 14px rgba(8, 47, 118, 0.25), inset 0 0 10px rgba(255, 255, 255, 0.06);
            transform: translateX(2px);
        }

        .portal-left-nav .portal-nav-link:hover:before {
            transform: scaleY(1);
        }

        .portal-left-nav .portal-nav-link:active {
            background: rgba(15, 110, 184, 0.5);
            color: #ffffff;
        }

        .portal-left-nav .portal-nav-link.is-active {
            background: linear-gradient(90deg, rgba(88, 166, 255, 0.28), rgba(255, 255, 255, 0.10));
            color: #ffffff;
            border-color: rgba(162, 207, 255, 0.72);
            box-shadow: inset 4px 0 0 #73c7ff, 0 8px 18px rgba(8, 47, 118, 0.22);
        }

        .portal-left-nav .portal-nav-link.is-active:before {
            transform: scaleY(1);
        }

        .portal-left-nav.is-collapsed .portal-section-header {
            justify-content: center;
            margin: 0 0 10px;
        }

        .portal-left-nav.is-collapsed .collapsible-body.is-collapsed {
            display: block !important;
        }

        .portal-left-nav.is-collapsed .portal-nav-link {
            justify-content: center;
            gap: 0;
            padding: 10px 0;
            min-height: 40px;
        }

        .portal-left-nav.is-collapsed .portal-nav-link span {
            display: none;
        }

        .portal-left-nav.is-collapsed .portal-nav-link i {
            margin: 0;
            font-size: 15px;
        }

        .portal-nav-active-title {
            margin: 0 4px 12px;
            text-align: left;
            color: #b8dcff;
            font-size: 10px;
            font-weight: 800;
            letter-spacing: 0.08em;
            text-transform: uppercase;
            padding: 0 2px 8px;
            border-bottom: 1px solid rgba(143, 185, 242, 0.25);
            background: transparent;
        }

        .portal-left-nav.is-collapsed .portal-nav-active-title {
            display: block;
            margin: 2px 2px 8px;
            padding: 5px 4px;
            font-size: 10px;
            letter-spacing: 0.02em;
            line-height: 1.2;
        }

        .portal-main-workspace {
            flex: 1;
            min-width: 0;
        }

        .portal-layout-shell.main-menu-collapsed {
            gap: 14px;
        }

        .portal-layout-shell.main-menu-collapsed .portal-main-workspace {
            margin-left: 0;
            padding-left: 0;
        }

        .portal-menu-reopen {
            display: none;
            align-items: center;
            justify-content: center;
            position: sticky;
            top: 96px;
            min-height: 34px;
            min-width: 42px;
            margin-top: 8px;
            border: 1px solid #b7cce0;
            border-radius: 10px;
            background: #f1f7ff;
            color: #1c4e78;
            font-size: 12px;
            font-weight: 700;
            padding: 0 10px;
            cursor: pointer;
            box-shadow: 0 2px 6px rgba(16, 45, 74, 0.12);
            z-index: 95;
        }

        .portal-menu-reopen:hover {
            background: #e4f1ff;
            border-color: #8eb3d7;
        }

        .portal-layout-shell.main-menu-collapsed .portal-menu-reopen {
            display: inline-flex;
        }

        .portal-layout-shell.main-menu-collapsed .portal-menu-reopen {
            display: none !important;
        }

        .welcome-home-panel {
            background: #ffffff;
            border: 1px solid #d7e2ed;
            border-radius: 12px;
            padding: 22px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
            margin-bottom: 14px;
        }

        .welcome-home-title {
            margin: 0 0 8px;
            color: #0f4f8a;
            font-size: 28px;
            font-weight: 700;
        }

        .welcome-home-desc {
            margin: 0;
            color: #495e73;
            font-size: 15px;
            line-height: 1.5;
        }

        .portal-tab-shell {
            display: flex;
            flex-direction: column;
            gap: 18px;
            padding: 4px 0 24px;
            color: #1f3b57;
        }

        .portal-tab-hero {
            border: 1px solid #d9e4ef;
            border-radius: 12px;
            background: linear-gradient(180deg, #ffffff 0%, #f7fafd 100%);
            padding: 18px 20px;
            box-shadow: 0 8px 22px rgba(16, 41, 67, 0.06);
        }

        .portal-tab-eyebrow {
            margin: 0 0 6px;
            color: #0f5ea8;
            font-size: 11px;
            font-weight: 800;
            letter-spacing: 0.06em;
            text-transform: uppercase;
        }

        .portal-tab-title {
            margin: 0;
            color: #103a5c;
            font-size: 24px;
            line-height: 1.2;
            font-weight: 800;
        }

        .portal-tab-desc {
            max-width: 860px;
            margin: 8px 0 0;
            color: #5c7087;
            font-size: 14px;
            line-height: 1.5;
        }

        .portal-surface-card {
            border: 1px solid #d9e4ef;
            border-radius: 12px;
            background: #ffffff;
            padding: 16px;
            box-shadow: 0 8px 22px rgba(16, 41, 67, 0.06);
        }

        .cmf-rules-editor-shell {
            display: grid;
            grid-template-columns: minmax(0, 1.4fr) minmax(280px, .8fr);
            gap: 18px;
        }

        .cmf-rules-card {
            min-width: 0;
        }

        .cmf-rules-title {
            margin: 0 0 6px;
            color: #143a5a;
            font-size: 16px;
            font-weight: 700;
        }

        .cmf-rules-editor {
            width: 100%;
            min-height: 360px;
            border: 1px solid #c8d9ea;
            border-radius: 10px;
            padding: 12px;
            font-family: Consolas, 'Courier New', monospace;
            font-size: 13px;
            line-height: 1.45;
            color: #18324a;
            background: #fbfdff;
        }

        .cmf-rules-actions {
            display: flex;
            gap: 10px;
            align-items: center;
            flex-wrap: wrap;
            margin-top: 12px;
        }

        .cmf-rules-status {
            display: block;
            margin-top: 10px;
            font-size: 13px;
            font-weight: 600;
        }

        .cmf-rules-note {
            margin: 10px 0 0;
            color: #4c6378;
            font-size: 13px;
            line-height: 1.5;
        }

        .cmf-rules-list {
            margin: 10px 0 0;
            padding-left: 18px;
            color: #30495f;
            font-size: 13px;
            line-height: 1.6;
        }

        @media (max-width: 900px) {
            .cmf-rules-editor-shell {
                grid-template-columns: 1fr;
            }
        }

        .home-dashboard-shell {
            display: flex;
            flex-direction: column;
            gap: 18px;
        }

        .home-dashboard-hero {
            display: flex;
            justify-content: space-between;
            gap: 18px;
            align-items: flex-start;
            flex-wrap: wrap;
        }

        .home-dashboard-hero-copy {
            flex: 1 1 460px;
        }

        .home-dashboard-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 6px 12px;
            border-radius: 999px;
            background: rgba(15, 94, 168, 0.10);
            color: #0f5ea8;
            font-size: 12px;
            font-weight: 700;
            letter-spacing: 0.04em;
            text-transform: uppercase;
            margin-bottom: 12px;
        }

        .home-dashboard-hero-actions {
            display: flex;
            gap: 10px;
            align-items: center;
            flex-wrap: wrap;
        }

        .home-dashboard-pill-link {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 10px 14px;
            border-radius: 999px;
            text-decoration: none;
            border: 1px solid #bfd3e7;
            background: #f7fbff;
            color: #0f4f8a;
            font-size: 13px;
            font-weight: 600;
        }

        .home-dashboard-pill-link:hover {
            text-decoration: none;
            background: #eef6fd;
            color: #083d74;
        }

        .home-dashboard-generated {
            font-size: 12px;
            color: #5c7087;
        }

        .home-dashboard-metrics {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
            gap: 14px;
        }

        .home-dashboard-metric-card {
            position: relative;
            overflow: hidden;
            background: linear-gradient(180deg, #ffffff 0%, #f8fbfe 100%);
            border: 1px solid #d7e2ed;
            border-radius: 16px;
            padding: 16px 18px;
            box-shadow: 0 10px 22px rgba(16, 41, 67, 0.08);
            transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
        }

        .home-dashboard-metric-card:hover {
            transform: translateY(-2px);
            border-color: #b8d4ee;
            box-shadow: 0 16px 30px rgba(16, 41, 67, 0.12);
        }

        .home-dashboard-metric-top {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 10px;
            margin-bottom: 8px;
        }

        .home-dashboard-metric-icon {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 34px;
            height: 34px;
            border-radius: 12px;
            color: #ffffff;
            box-shadow: 0 10px 18px rgba(16, 41, 67, 0.12);
        }

        .home-dashboard-metric-card:nth-child(1) .home-dashboard-metric-icon { background: #ef4444; }
        .home-dashboard-metric-card:nth-child(2) .home-dashboard-metric-icon { background: #f59e0b; }
        .home-dashboard-metric-card:nth-child(3) .home-dashboard-metric-icon { background: #10b981; }
        .home-dashboard-metric-card:nth-child(4) .home-dashboard-metric-icon { background: #3b82f6; }
        .home-dashboard-metric-card:nth-child(5) .home-dashboard-metric-icon { background: #8b5cf6; }

        .home-dashboard-metric-spark {
            display: flex;
            align-items: flex-end;
            gap: 3px;
            height: 24px;
            margin-top: 10px;
        }

        .home-dashboard-metric-spark span {
            display: block;
            width: 12px;
            border-radius: 999px;
            background: #d7e6f5;
        }

        .home-dashboard-metric-spark span:nth-child(1) { height: 8px; }
        .home-dashboard-metric-spark span:nth-child(2) { height: 13px; }
        .home-dashboard-metric-spark span:nth-child(3) { height: 10px; }
        .home-dashboard-metric-spark span:nth-child(4) { height: 18px; }
        .home-dashboard-metric-spark span:nth-child(5) { height: 14px; }

        .home-dashboard-metric-card:nth-child(1) .home-dashboard-metric-spark span { background: #fecaca; }
        .home-dashboard-metric-card:nth-child(2) .home-dashboard-metric-spark span { background: #fed7aa; }
        .home-dashboard-metric-card:nth-child(3) .home-dashboard-metric-spark span { background: #bbf7d0; }
        .home-dashboard-metric-card:nth-child(4) .home-dashboard-metric-spark span { background: #bfdbfe; }
        .home-dashboard-metric-card:nth-child(5) .home-dashboard-metric-spark span { background: #ddd6fe; }

        .home-dashboard-metric-label {
            font-size: 12px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: #698096;
            margin-bottom: 8px;
        }

        .home-dashboard-metric-value {
            font-size: 30px;
            line-height: 1;
            font-weight: 800;
            color: #15395b;
            margin-bottom: 6px;
        }

        .home-dashboard-metric-note {
            font-size: 12px;
            color: #5c7087;
        }

        .home-ai-panels {
            display: grid;
            grid-template-columns: minmax(0, 0.95fr) minmax(0, 1.05fr);
            gap: 14px;
        }

        .home-ai-card {
            position: relative;
            overflow: hidden;
            background: linear-gradient(135deg, #ffffff 0%, #f3f8fc 100%);
            border: 1px solid #d4e2ef;
            border-radius: 14px;
            padding: 16px;
            box-shadow: 0 10px 22px rgba(16, 41, 67, 0.08);
        }

        .home-ai-card:before {
            content: "";
            position: absolute;
            inset: 0 auto 0 0;
            width: 4px;
            background: linear-gradient(180deg, #0068b5, #00a6b2);
        }

        .home-ai-card-head {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
            margin-bottom: 12px;
        }

        .home-ai-card-title {
            margin: 0;
            color: #103a5c;
            font-size: 16px;
            font-weight: 800;
        }

        .home-ai-card-subtitle {
            margin: 4px 0 0;
            color: #5c7087;
            font-size: 12px;
        }

        .home-ai-card-icon {
            width: 36px;
            height: 36px;
            border-radius: 12px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            color: #ffffff;
            background: #0f5ea8;
            box-shadow: 0 10px 18px rgba(15, 94, 168, 0.18);
        }

        .home-ai-tracker-grid {
            display: grid;
            grid-template-columns: repeat(3, minmax(0, 1fr));
            gap: 10px;
        }

        .home-ai-tracker-item {
            border: 1px solid #dfebf5;
            border-radius: 10px;
            background: #fbfdff;
            padding: 10px;
        }

        .home-ai-tracker-label {
            color: #647990;
            font-size: 11px;
            font-weight: 800;
            text-transform: uppercase;
            letter-spacing: 0.04em;
        }

        .home-ai-tracker-value {
            margin-top: 6px;
            color: #0f4f8a;
            font-size: 24px;
            font-weight: 900;
            line-height: 1;
        }

        .home-ai-summary-list {
            margin: 0;
            padding: 0;
            list-style: none;
            display: grid;
            gap: 8px;
            color: #263d54;
            font-size: 13px;
            line-height: 1.4;
        }

        .home-ai-summary-list li {
            display: flex;
            gap: 8px;
            align-items: flex-start;
        }

        .home-ai-summary-list i {
            color: #0f5ea8;
            margin-top: 2px;
        }

        .home-dashboard-grid {
            display: grid;
            grid-template-columns: minmax(0, 1.55fr) minmax(320px, 1fr);
            gap: 18px;
        }

        .home-dashboard-panel {
            background: #ffffff;
            border: 1px solid #d7e2ed;
            border-radius: 16px;
            box-shadow: 0 10px 22px rgba(16, 41, 67, 0.08);
            padding: 18px;
        }

        .home-dashboard-panel-head {
            display: flex;
            justify-content: space-between;
            gap: 12px;
            align-items: flex-start;
            margin-bottom: 12px;
        }

        .home-dashboard-panel-title {
            margin: 0;
            color: #0f4f8a;
            font-size: 20px;
            font-weight: 700;
        }

        .home-dashboard-panel-subtitle {
            margin: 4px 0 0;
            color: #5c7087;
            font-size: 13px;
        }

        .home-dashboard-chart {
            width: 100%;
            height: 320px;
        }

        .home-dashboard-chart-compact {
            width: 100%;
            height: 260px;
        }

        .home-dashboard-side-stack {
            display: flex;
            flex-direction: column;
            gap: 18px;
        }

        .home-dashboard-list {
            list-style: none;
            margin: 0;
            padding: 0;
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        .home-dashboard-list-item {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
            border: 1px solid #e1eaf2;
            border-radius: 12px;
            padding: 12px 14px;
            background: #fbfdff;
        }

        .home-dashboard-list-label {
            font-size: 13px;
            font-weight: 600;
            color: #1f3b57;
        }

        .home-dashboard-list-value {
            min-width: 46px;
            text-align: right;
            font-size: 18px;
            font-weight: 800;
            color: #0f5ea8;
        }

        @media (max-width: 1120px) {
            .home-dashboard-grid {
                grid-template-columns: 1fr;
            }

            .home-ai-panels {
                grid-template-columns: 1fr;
            }
        }

        .sidebar-toggle {
            background: rgba(255, 255, 255, 0.14);
            color: white;
            border: 1px solid rgba(255, 255, 255, 0.24);
            font-size: 16px;
            cursor: pointer;
            z-index: 10;
            border-radius: 999px;
            width: 42px;
            height: 42px;
            backdrop-filter: blur(8px);
        }

        .sidebar-toggle-div {
            position: absolute;
            right: 1.2rem;
            display: none !important;
        }

        .sidebar {
            font-size: 12px;
            position: fixed;
            top: 0;
            right: -280px;
            height: 100%;
            width: 250px;
            background: linear-gradient(180deg, #0b417b 0%, #0f5ea8 100%);
            padding-top: 68px;
            transition: right 0.3s ease-in-out;
            z-index: 120;
            overflow-y: auto;
            box-shadow: -16px 0 32px rgba(5, 25, 48, 0.22);
        }

        .close-btn {
            position: absolute;
            top: 12px;
            right: 14px;
            background-color: transparent;
            color: white;
            font-size: 20px;
            border: none;
            cursor: pointer;
        }

        .sidebar a {
            color: #eef7ff;
            padding: 12px 20px;
            text-decoration: none;
            display: block;
            font-size: 12px;
        }

        .sidebar a:hover {
            background-color: rgba(255, 255, 255, 0.12);
        }

        .content-wrapper {
            margin: 18px 18px 28px;
            padding: 24px;
            background: rgba(255, 255, 255, 0.74);
            border: 1px solid rgba(205, 217, 229, 0.9);
            border-radius: 24px;
            box-shadow: var(--portal-shadow);
            min-height: calc(100vh - 160px);
            backdrop-filter: blur(10px);
        }

        .table-container {
            margin-top: 0;
            position: relative;
            width: 100%;
            max-height: 75vh;
            overflow-y: auto;
            border-radius: var(--portal-radius);
            box-shadow: 0 12px 30px rgba(21, 49, 79, 0.10);
            background-color: var(--portal-surface);
            border: 1px solid var(--portal-border);
            padding: 10px;
        }

            /* Make the header sticky */
            .table-container thead {
                position: sticky;
                top: 0;
                background-color: #fff; /* Set a background color to cover any content scrolling beneath */
                z-index: 1; /* Keep the header above the rows */
            }

        
        .table-primary {
            font-weight: 600;
            font-family: "Segoe UI", sans-serif;
            border-collapse: separate;
            border-spacing: 0;
            font-size: .82em;
            border-radius: 14px;
            overflow: hidden;
            transition: box-shadow 0.3s ease;
            border: 1px solid var(--portal-border);
            margin-bottom: 10px;
            background: var(--portal-surface);
        }

        .table-primary th, .table-primary td {
            padding: 12px 14px;
            text-align: left;
            border: 1px solid #d7e0ea;
        }

        .table-primary thead {
            position: sticky;
            top: 0; /* Stick the header to the top */
            background-color: #fff; /* Ensure header has a solid background */
            z-index: 2; /* Make sure header stays on top of rows */
            box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1); /* Optional, to add a shadow */
        }

        .table-primary th {
            background: linear-gradient(180deg, #1568b3 0%, #0f5ea8 100%);
            color: white;
            font-weight: 700;
        }

        .table-primary tr {
            border-bottom: 1px solid #d7e0ea;
        }

                .component-with-group,
                .owner-pair,
                .status-cell-wrap-compact {
                    display: flex;
                    flex-direction: column;
                    gap: 14px;
                }

                .component-with-group,
                .owner-pair {
                    gap: 16px;
                }

                .component-detail {
                    font-size: 12px;
                    color: #374151;
                    word-break: break-word;
                    overflow-wrap: anywhere;
                    line-height: 1.5;
                }

                .component-group-pill {
                    display: inline-flex;
                    align-self: flex-start;
                    align-items: center;
                    justify-content: center;
                    min-height: 20px;
                    padding: 2px 10px;
                    border-radius: 999px;
                    font-size: 10px;
                    font-weight: 700;
                    letter-spacing: 0.04em;
                    text-transform: uppercase;
                    background: #eef4fb;
                    color: #0f5ea8;
                    border: 1px solid #cfe0f1;
                }

                .owner-inline-row {
                    display: flex;
                    align-items: baseline;
                    gap: 5px;
                    flex-wrap: wrap;
                }

                .owner-inline-label {
                    font-size: 11px;
                    font-weight: 600;
                    color: #6b7280;
                    white-space: nowrap;
                    flex-shrink: 0;
                }

                .owner-inline-value {
                    font-size: 13px;
                    color: #1b2a3a;
                    font-weight: 500;
                    word-break: break-word;
                }

                .owner-inline-muted {
                    color: #9ca3af;
                    font-weight: 400;
                }

                .status-cell-wrap-compact {
                    display: flex;
                    flex-direction: column;
                    align-items: flex-start;
                    gap: 6px;
                    min-width: 0;
                }

                .status-main {
                    display: block;
                    padding: 0;
                    margin: 0;
                    border: 0;
                    background: transparent;
                    color: #23364b;
                    font-size: 12px;
                    font-weight: 600;
                    line-height: 1.4;
                    white-space: normal;
                    overflow-wrap: anywhere;
                    word-break: break-word;
                }

                .ai-summary-btn-compact {
                    height: 24px;
                    padding: 0 8px;
                    border-radius: 999px;
                    font-size: 10px;
                    margin-top: 2px;
                }

                /* New status cell layout */
                .status-row {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    width: 100%;
                    min-width: 0;
                }

                .status-row-primary {
                    justify-content: flex-start;
                    flex-wrap: wrap;
                }

                .status-row-confidence {
                    margin-top: 0;
                }

                .status-label-group {
                    display: inline-flex;
                    align-items: center;
                    gap: 8px;
                    flex-wrap: wrap;
                    min-width: 0;
                }

                .status-pill {
                    display: inline-flex;
                    align-items: center;
                    padding: 6px 10px;
                    border-radius: 999px;
                    font-size: 12px;
                    font-weight: 700;
                    background: #eef4fb;
                    color: #0f5ea8;
                    border: 1px solid #cfe0f1;
                    max-width: 100%;
                    white-space: normal;
                    overflow-wrap: anywhere;
                }

                .status-confidence-pill {
                    display: inline-flex;
                    align-items: center;
                    min-height: 22px;
                    padding: 4px 8px;
                    border-radius: 999px;
                    font-size: 10px;
                    font-weight: 800;
                    color: #5b21b6;
                    background: #f4efff;
                    border: 1px solid #dfd2ff;
                    white-space: normal;
                    overflow-wrap: anywhere;
                }

                .ai-summary-btn-inline {
                    height: 22px;
                    min-width: 26px;
                    padding: 0 8px;
                    border-radius: 999px;
                    font-size: 11px;
                    background: transparent;
                    border: none;
                    color: #6b7280;
                    cursor: pointer;
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                }

                .ai-summary-btn-inline:hover {
                    background: rgba(15,94,168,0.06);
                    color: #0f5ea8;
                }

                .status-one-line {
                    display: block;
                    font-size: 11px;
                    line-height: 1.35;
                    color: #35516d;
                    margin-top: 2px;
                    width: 100%;
                    max-width: 100%;
                    white-space: normal;
                    overflow: hidden;
                    overflow-wrap: break-word;
                    word-break: normal;
                }

                .issue-grid-toolbar {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    padding: 4px 2px 6px;
                }

                .show-cols-btn {
                    font-size: 12px;
                    padding: 3px 10px;
                    border: 1px solid #aab;
                    border-radius: 4px;
                    background: #f4f6fa;
                    cursor: pointer;
                    color: #2a4a7f;
                }

                .show-cols-btn:hover {
                    background: #e2e8f4;
                }

                .col-hide-btn {
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    width: 18px;
                    height: 18px;
                    margin-left: 5px;
                    padding: 0;
                    font-size: 11px;
                    line-height: 1;
                    border: 1px solid rgba(255,255,255,0.6);
                    border-radius: 3px;
                    background: rgba(255,255,255,0.18);
                    color: #fff;
                    cursor: pointer;
                    opacity: 0.85;
                    vertical-align: middle;
                    flex-shrink: 0;
                }

                .col-hide-btn:hover {
                    opacity: 1;
                    background: rgba(220,50,50,0.55);
                    border-color: #fff;
                }

                .issue-sno-cell {
                    display: flex;
                    flex-direction: column;
                    align-items: flex-start;
                    gap: 6px;
                }

                .issue-sno-index {
                    font-weight: 700;
                    color: #1f3b57;
                }

                .issue-inline-edit-btn,
                .issue-inline-edit-action {
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    width: 28px;
                    height: 28px;
                    border-radius: 999px;
                    border: 1px solid #c7d7e7;
                    background: #ffffff;
                    color: #0f5ea8;
                    text-decoration: none;
                    cursor: pointer;
                }

                .issue-inline-edit-btn:hover,
                .issue-inline-edit-action:hover {
                    background: #eef6fd;
                    color: #083d74;
                    text-decoration: none;
                }

                .issue-inline-edit-actions {
                    display: flex;
                    gap: 6px;
                }
/* Add these new styles */
.tab-content-wrapper {
    position: relative;
    width: 100%;
    padding-top: 0;
    margin-top: 0;
}

.pane-heading {
    position: relative;
    z-index: 10;
    background: linear-gradient(90deg, rgba(15, 94, 168, 0.10), rgba(0, 166, 178, 0.06));
    padding: 14px 18px;
    margin: 8px 0 18px;
    border: 1px solid rgba(15, 94, 168, 0.18);
    border-radius: 16px;
    font-weight: 700;
    font-size: 15px;
    color: var(--portal-primary-deep);
    letter-spacing: 0.08em;
    text-transform: uppercase;
    text-align: center;
}

.modal-body.popup {
    position: relative;
    top: 0;
    margin-top: 0;
    padding-top: 0; /* Changed from 10px to 0 */
    padding-bottom: 10px;
}

.filter-header-container {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    width: 100%;
    min-height: 60px;
}

.filter-header-text {
    display: flex;
    align-items: center;
    gap: 4px;
    font-weight: bold;
    margin-bottom: 5px;
    align-self: center;
}

.filter-container {
    width: 100%;
    display: flex;
    justify-content: flex-end;
}

.header-dropdown {
    font-size: 11px;
    padding: 4px 8px;
    width: 132px;
    height: 30px;
    border: 1px solid #bfd0df;
    border-radius: 999px;
    background: #fff;
    color: var(--portal-ink);
}

/* RESTORE ORIGINAL TABLE STYLING FOR OTHER TABS */
.table-primary th {
    vertical-align: top;
    padding: 8px;
}

.gridview-container {
    position: relative;
    margin-top: 0;
    padding-top: 0;
    overflow-x: auto;
    width: 100%;
}

.empty-data-container {
    width: 100%;
    overflow-x: auto;
    margin-top: 0;
}

.empty-data-container table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 0;
}

.empty-data-container th {
    background-color: #f8f9fa;
    border: 1px solid #dee2e6;
    padding: 8px;
    text-align: left;
}

.empty-data-container td {
    border: 1px solid #dee2e6;
    padding: 8px;
}

/* Modern compact Issue List table */
#overall_request_details {
    margin-top: 0 !important;
    position: relative !important;
    z-index: 100 !important;
    border-collapse: separate !important;
    border-spacing: 0 8px !important;
    background: transparent !important;
    border: 0 !important;
}

#overall_request_details thead {
    display: table-header-group !important;
    background: transparent !important;
    position: sticky !important;
    top: 0 !important;
    z-index: 101 !important;
}

#overall_request_details th {
    background: #f6f8fb !important;
    color: #475569 !important;
    font-size: 10px !important;
    padding: 10px 10px !important;
    height: auto !important;
    min-height: 34px !important;
    border-top: 1px solid #e5ebf3 !important;
    border-bottom: 1px solid #e5ebf3 !important;
    border-left: 0 !important;
    border-right: 0 !important;
    display: table-cell !important;
    visibility: visible !important;
    opacity: 1 !important;
    position: relative !important;
    z-index: 102 !important;
    text-align: left !important;
    font-weight: 800 !important;
    letter-spacing: 0.05em !important;
    text-transform: uppercase !important;
    vertical-align: middle !important;
}

#overall_request_details td {
    background: #ffffff !important;
    border-top: 1px solid #e6edf5 !important;
    border-bottom: 1px solid #e6edf5 !important;
    border-left: 0 !important;
    border-right: 0 !important;
    padding: 12px 10px !important;
    box-shadow: 0 8px 22px rgba(20, 46, 77, 0.06) !important;
}

#overall_request_details tr,
#overall_request_details tr:first-child,
#overall_request_details tr:nth-child(even),
#overall_request_details tr:nth-child(odd) {
    background: transparent !important;
    border-bottom: 0 !important;
}

#overall_request_details tr td:first-child {
    border-left: 1px solid #e6edf5 !important;
    border-radius: 8px 0 0 8px !important;
}

#overall_request_details tr td:last-child {
    border-right: 1px solid #e6edf5 !important;
    border-radius: 0 8px 8px 0 !important;
}

/* Keep table headers clean; filters are shown above the table */
#overall_request_details th .filter-header-container {
    display: flex !important;
    flex-direction: column !important;
    align-items: flex-start !important;
    width: 100% !important;
    min-height: auto !important;
}

#overall_request_details th .filter-header-text {
    font-weight: bold !important;
    margin-bottom: 0 !important;
    align-self: flex-start !important;
    color: #475569 !important;
}

#overall_request_details th .filter-container {
    display: none !important;
}

.empty-data-container .filter-container {
    display: none !important;
}

.issue-top-filter-panel {
    padding: 12px;
    margin: 8px 0 10px !important;
    border-radius: 12px !important;
    background: #ffffff !important;
    border: 1px solid #e4ebf3 !important;
    box-shadow: 0 8px 22px rgba(20, 46, 77, 0.05) !important;
}

.issue-filter-section-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 10px;
    padding-bottom: 8px;
    border-bottom: 1px solid #d7e2ed;
}

.issue-filter-title {
    font-size: 13px;
    font-weight: 700;
    letter-spacing: 0.03em;
    text-transform: uppercase;
    color: #34506a;
}

.issue-collapse-btn {
    border: 1px solid #b8ccdf;
    background: #f2f8ff;
    color: #184a72;
    border-radius: 999px;
    font-size: 11px;
    font-weight: 700;
    min-height: 26px;
    min-width: 70px;
    padding: 4px 10px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
    cursor: pointer;
}

.issue-collapse-btn:hover {
    background: #e4f1ff;
    border-color: #8fb3d6;
}

.issue-collapse-btn .toggle-chevron {
    transition: transform 0.2s ease;
}

.issue-collapse-btn[aria-expanded="false"] .toggle-chevron {
    transform: rotate(-90deg);
}

.issue-top-filter-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(128px, 1fr));
    gap: 8px;
    margin-bottom: 10px;
}

.issue-top-filter-item {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.issue-top-filter-item span {
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: #616161;
    font-weight: 700;
}

.issue-top-filter-item .header-dropdown {
    width: 100% !important;
    height: 30px;
    font-size: 11px !important;
    border-radius: 8px !important;
}

.issue-top-filter-buttons {
    display: flex;
    gap: 8px;
    justify-content: flex-start;
}

.filter-btn {
    padding: 7px 16px;
    border: none;
    border-radius: 8px;
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    text-transform: capitalize;
    transition: all 0.2s ease;
}

#overall_request_details .col-hide-btn {
    display: none !important;
}

.filter-btn-primary {
    background: #0068b5;
    color: #ffffff;
}

.filter-btn-primary:hover {
    background: #005a9e;
    box-shadow: 0 2px 8px rgba(0, 104, 181, 0.3);
}

.filter-btn-secondary {
    background: #e8e8e8;
    color: #2a2a2a;
    border: 1px solid #d0d0d0;
}

.filter-btn-secondary:hover {
    background: #d8d8d8;
}

.status-cell-wrap {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.status-main {
    font-weight: 700;
    color: #23364b;
}

.sysdebug-preview {
    font-size: 11px;
    color: #5a6d80;
    max-height: 54px;
    overflow: hidden;
}

.ai-summary-btn {
    align-self: flex-start;
    height: 28px;
    padding: 0 10px;
    border-radius: 14px;
    font-size: 11px;
    border: 1px solid #0f5ea8;
    color: #0f5ea8;
    background: #ffffff;
}

.ai-summary-btn:hover {
    background: #eff6ff;
}

.ai-recommendation-btn {
    align-self: flex-start;
    height: 28px;
    padding: 0 10px;
    border-radius: 14px;
    font-size: 11px;
    border: 1px solid #107c10;
    color: #107c10;
    background: #ffffff;
}

.ai-recommendation-btn:hover {
    background: #f0f9f5;
}

.ai-summary-body {
    background: linear-gradient(180deg, #ffffff 0%, #f7fbff 100%);
    border: 1px solid #d8e3ee;
    border-radius: 14px;
    padding: 14px;
    white-space: normal;
    line-height: 1.45;
    font-size: 13px;
    color: #21384d;
    box-shadow: 0 12px 28px rgba(16, 41, 67, 0.08);
}

.ai-summary-body ul,
.ai-summary-body ol {
    margin: 0;
    padding-left: 0;
    list-style: none;
    display: grid;
    gap: 10px;
}

.ai-summary-body li {
    position: relative;
    padding: 10px 12px 10px 34px;
    border: 1px solid #e3edf7;
    border-radius: 12px;
    background: #ffffff;
    box-shadow: 0 6px 16px rgba(16, 41, 67, 0.05);
}

.ai-summary-body li::before {
    content: "\2726";
    position: absolute;
    left: 12px;
    top: 10px;
    color: #7c3aed;
    font-size: 12px;
    line-height: 1;
}

.issue-details-brief {
    background: linear-gradient(135deg, #f7fbff 0%, #effaf8 100%) !important;
    border-color: #cfe3f1 !important;
}

.issue-details-brief h2,
.issue-details-brief h3 {
    margin: 12px 0 8px !important;
    color: #123b5a !important;
    font-size: 15px !important;
    font-weight: 900 !important;
}

.issue-details-brief ul {
    grid-template-columns: 1fr !important;
}

.issue-details-brief li {
    border-color: #d7eaf3 !important;
    background: rgba(255, 255, 255, 0.92) !important;
    box-shadow: 0 8px 18px rgba(14, 63, 89, 0.07) !important;
}

.issue-details-brief li::before {
    content: "i";
    width: 16px;
    height: 16px;
    border-radius: 50%;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: #1b7f8f;
    color: #ffffff;
    font-size: 10px;
    font-weight: 900;
}

.cmf-rec-quality-card {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 12px 14px;
    background: #f8fbff;
    border: 1px solid #dbe7f4;
    border-radius: 10px;
    margin-bottom: 16px;
}

.cmf-rec-hero {
    display: grid;
    gap: 12px;
    margin-top: 16px;
}

.cmf-rec-section {
    margin-top: 16px;
}

.cmf-rec-section h3 {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 12px !important;
    font-weight: 850 !important;
    color: #344761 !important;
    margin: 0 0 8px !important;
    text-transform: uppercase;
    letter-spacing: 0.06em;
}

.cmf-rec-decision-card {
    border-left: 4px solid #7c3aed !important;
    background: linear-gradient(135deg, #fbf8ff 0%, #eef7ff 100%) !important;
}

.cmf-rec-title-row {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    flex-wrap: wrap;
}

.cmf-rec-title-text {
    flex: 1 1 180px;
    min-width: 0;
}

.cmf-rec-decision-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 28px;
    padding: 5px 12px;
    border-radius: 999px;
    border: 1px solid #f2c94c;
    background: linear-gradient(180deg, #fff7d6 0%, #ffe8a3 100%);
    color: #684600;
    font-size: 12px;
    font-weight: 650;
    box-shadow: 0 10px 22px rgba(245, 158, 11, 0.18);
    max-width: 100%;
    overflow-wrap: anywhere;
}

.pending-ai-rec-ready {
    border-color: #9dd7df !important;
    background: #f0fbfc !important;
}

.cmf-rec-placeholder-actions {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
    margin-top: 12px;
}

.cmf-rec-placeholder-btn {
    min-height: 38px;
    border-radius: 10px;
    border: 1px solid #d8c9ff;
    background: #ffffff;
    color: #5b21b6;
    font-size: 12px;
    font-weight: 800;
    cursor: pointer;
    box-shadow: 0 8px 20px rgba(124, 58, 237, 0.10);
}

.cmf-rec-placeholder-btn.primary {
    background: #7c3aed;
    border-color: #7c3aed;
    color: #ffffff;
}

.cmf-rec-placeholder-btn:hover {
    transform: translateY(-1px);
    box-shadow: 0 12px 24px rgba(124, 58, 237, 0.16);
}

.cmf-rec-quality-label {
    font-size: 11px;
    font-weight: 800;
    color: #5d7289;
    text-transform: uppercase;
    letter-spacing: 0.03em;
}

.cmf-rec-quality-value {
    margin-top: 4px;
    font-size: 14px;
    font-weight: 850;
    color: #0f5ea8;
    line-height: 1.35;
}

.cmf-rec-quality-line {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 14px;
    min-width: 260px;
}

.cmf-rec-quality-line span { color: #5d7289; }
.cmf-rec-quality-line strong { color: #0f5ea8; font-size: 18px; }

.reports-format-textarea.is-readonly {
    background: #f8fbff;
    color: #263b50;
    cursor: default;
}

.cmf-rec-score-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 16px;
}

.cmf-rec-score-item {
    display: grid;
    grid-template-columns: 24px minmax(0, 1fr) auto;
    gap: 9px;
    align-items: flex-start;
    padding: 10px 11px;
    border: 1px solid #e1e8f0;
    border-radius: 9px;
    background: #ffffff;
}

.cmf-rec-score-icon {
    width: 20px;
    height: 20px;
    border-radius: 50%;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    font-weight: 900;
}

.cmf-rec-score-icon.pass { background: #dcfce7; color: #15803d; }
.cmf-rec-score-icon.partial { background: #fef3c7; color: #b45309; }
.cmf-rec-score-icon.fail { background: #fee2e2; color: #b91c1c; }

.cmf-rec-score-title {
    font-size: 12px;
    font-weight: 800;
    color: #20354a;
    margin-bottom: 2px;
}

.cmf-rec-score-detail {
    font-size: 11px;
    color: #5d7289;
    line-height: 1.35;
}

.cmf-rec-score-value {
    font-size: 12px;
    font-weight: 850;
    color: #0f5ea8;
    white-space: nowrap;
}

.cmf-rec-impact-list {
    display: grid;
    gap: 8px;
    margin: 0;
    padding: 0;
    list-style: none;
    width: 100%;
    max-width: none;
    box-sizing: border-box;
}

.cmf-rec-impact-item {
    display: grid;
    grid-template-columns: 8px minmax(0, 1fr);
    gap: 10px;
    align-items: flex-start;
    padding: 10px 12px;
    border-radius: 12px;
    border: 1px solid #dbeafe;
    background: #f8fbff;
    color: #263b50;
    font-size: 12px;
    line-height: 1.45;
    width: 100%;
    max-width: none;
    box-sizing: border-box;
}

.cmf-rec-impact-item::before {
    content: "";
    width: 8px;
    height: 8px;
    border-radius: 50%;
    margin-top: 5px;
    background: #2563eb;
}

.cmf-rec-impact-item:nth-child(2)::before { background: #16a34a; }
.cmf-rec-impact-item:nth-child(3)::before { background: #f59e0b; }
.cmf-rec-impact-item:nth-child(4)::before { background: #dc2626; }

.cmf-rec-reasoning-card {
    display: grid;
    gap: 8px;
    padding: 12px 14px;
    border: 1px solid #dbeafe;
    border-radius: 12px;
    background: #f8fbff;
    color: #24364a;
    font-size: 12px;
    line-height: 1.45;
}

.cmf-rec-reasoning-card strong {
    color: #143f6b;
    font-weight: 800;
}

.cmf-rec-section .ai-summary-body {
    width: 100%;
    box-sizing: border-box;
}

.cmf-rec-section #cmfRecEvidence.ai-summary-body {
    padding: 0 !important;
    background: transparent !important;
    border: 0 !important;
}

.cmf-rec-section .ai-summary-body .cmf-rec-impact-list {
    width: 100% !important;
    max-width: none !important;
    padding: 0 !important;
}

.cmf-rec-section .ai-summary-body .cmf-rec-impact-item {
    width: 100% !important;
    max-width: none !important;
    padding: 10px 12px !important;
    box-shadow: none !important;
}

.cmf-rec-section .ai-summary-body .cmf-rec-impact-item::before {
    content: "" !important;
    position: static !important;
    left: auto !important;
    top: auto !important;
}

.cmf-rec-hidden {
    display: none !important;
}

#cmfRecDrawer {
    width: min(980px, 50vw) !important;
    height: 100% !important;
    max-height: 100vh !important;
    top: 0 !important;
    right: 0 !important;
    border: 0 !important;
    border-left: 1px solid rgba(172, 199, 231, 0.92) !important;
    border-radius: 0 !important;
    overflow-y: auto !important;
    padding: 18px 20px 20px !important;
    background:
        radial-gradient(circle at 12% 0%, rgba(40, 118, 240, 0.14), transparent 28%),
        linear-gradient(180deg, #ffffff 0%, #f4f9ff 100%) !important;
    background-color: #f7fbff !important;
    box-shadow: -18px 0 42px rgba(8, 25, 49, 0.24) !important;
    transform: translateX(100%) !important;
    visibility: hidden !important;
    opacity: 0 !important;
    pointer-events: none !important;
}

#cmfRecDrawer.show {
    transform: translateX(0) !important;
    visibility: visible !important;
    opacity: 1 !important;
    pointer-events: auto !important;
}

#cmfRecDrawer .ai-summary-drawer-title {
    margin-bottom: 7px;
    padding-right: 42px;
    color: #09204a;
    font-size: 16px;
    letter-spacing: 0;
    line-height: 1.3;
}

.cmf-assistant-title-icon {
    display: inline;
    margin-right: 8px;
    background: transparent;
    border: 0;
    color: inherit;
    font-size: 16px;
    vertical-align: middle;
}

.cmf-assistant-context {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 4px 12px;
    margin: 0 0 8px;
    padding: 8px 10px;
    border: 1px solid #d8e7f5;
    border-radius: 10px;
    background: rgba(255, 255, 255, 0.78);
    box-shadow: 0 6px 16px rgba(30, 84, 144, 0.05);
}

.cmf-assistant-context span {
    color: #64748b;
    font-size: 11px;
    font-weight: 700;
    text-transform: none;
    letter-spacing: 0.03em;
}

.cmf-assistant-context strong {
    min-width: 0;
    color: #102f55;
    font-size: 11px;
    font-weight: 400;
    overflow: hidden;
    text-overflow: clip;
    white-space: normal;
    overflow-wrap: anywhere;
    line-height: 1.35;
}

.cmf-assistant-context-line {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    min-width: 0;
    color: #111827;
    font-size: 12px;
    font-weight: 700;
    line-height: 1.35;
}

.cmf-assistant-hsd-link {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border: 1px solid #b9d8ff;
    border-radius: 7px;
    background: #ffffff;
    color: #0f5ea8;
    font-size: 11px;
    text-decoration: none;
    box-shadow: 0 4px 10px rgba(15, 94, 168, 0.08);
}

.cmf-assistant-hsd-link:hover,
.cmf-assistant-hsd-link:focus {
    background: #edf5ff;
    color: #0b4d8b;
    outline: none;
}

.cmf-assistant-context-title-line {
    grid-column: 1 / -1;
}

.cmf-assistant-context-line strong {
    color: #111827;
    font-size: 12px;
    font-weight: 500;
}

.cmf-assistant-context-title {
    align-self: start;
}

#cmfAssistantRecTag {
    margin-left: 8px;
    align-self: center;
    justify-self: end;
    color: #1f2937;
    font-size: 11px;
    line-height: 1;
}

.cmf-assistant-heading-id {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 5px 10px;
    border-radius: 999px;
    background: #edf5ff;
    border: 1px solid #c9ddfb;
    color: #17457c;
    font-size: 12px;
    font-weight: 850;
}

.cmf-assistant-heading-id span:first-child {
    color: #5f789b;
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.04em;
}

.cmf-assistant-heading-meta {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    margin-left: 8px;
    vertical-align: middle;
}

.cmf-assistant-heading-meta-item {
    display: inline-flex;
    align-items: baseline;
    gap: 5px;
    max-width: 210px;
    padding: 4px 8px;
    border: 1px solid rgba(92, 137, 190, 0.20);
    border-radius: 999px;
    background: rgba(255, 255, 255, 0.70);
    color: #17395f;
    font-size: 11px;
    font-weight: 700;
}

.cmf-assistant-heading-meta-item span {
    color: #64748b;
    font-size: 9px;
    font-weight: 800;
    text-transform: uppercase;
    white-space: nowrap;
}

.cmf-assistant-heading-meta-item strong {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: #102f55;
    font-size: 11px;
    font-weight: 800;
}

.cmf-rec-heading-sighting {
    display: inline-flex;
    align-items: center;
    margin-left: 12px;
    color: #5d7289;
    font-size: 13px;
    font-weight: 750;
    vertical-align: middle;
}

.cmf-decision-details-shell {
    display: grid;
    gap: 14px;
}

.cmf-decision-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 14px 16px;
    border: 1px solid #d7e7f5;
    border-radius: 14px;
    background: linear-gradient(135deg, #f7fbff 0%, #ecfdf5 100%);
}

.cmf-decision-header h3 {
    margin: 0 !important;
    font-size: 16px !important;
    font-weight: 900 !important;
    color: #17324d !important;
    text-transform: none !important;
    letter-spacing: 0 !important;
}

.cmf-decision-sighting {
    display: inline-flex;
    align-items: center;
    min-height: 28px;
    padding: 4px 10px;
    border-radius: 999px;
    background: #e0f2fe;
    color: #075985;
    font-size: 12px;
    font-weight: 850;
    white-space: nowrap;
}

.cmf-decision-card {
    border: 1px solid #e2eaf2;
    border-radius: 14px;
    background: #ffffff;
    padding: 14px;
    box-shadow: 0 10px 22px rgba(20, 48, 82, 0.06);
}

.cmf-decision-card h4 {
    margin: 0 0 8px !important;
    color: #213a55 !important;
    font-size: 13px !important;
    font-weight: 900 !important;
    text-transform: none !important;
    letter-spacing: 0 !important;
}

.cmf-decision-card p,
.cmf-decision-card li {
    color: #304761;
    font-size: 12px;
    line-height: 1.48;
}

.cmf-decision-card ul {
    display: grid;
    gap: 6px;
    margin: 0;
    padding-left: 18px;
}

.cmf-quality-meter-row {
    display: grid;
    grid-template-columns: 86px minmax(0, 1fr);
    gap: 12px;
    align-items: center;
    margin-bottom: 12px;
}

.cmf-quality-score-ring {
    --score: 0;
    width: 76px;
    height: 76px;
    border-radius: 50%;
    display: grid;
    place-items: center;
    background: conic-gradient(#16a34a calc(var(--score) * 1%), #e6eef6 0);
    color: #14532d;
    font-size: 18px;
    font-weight: 950;
    position: relative;
}

.cmf-quality-score-ring::after {
    content: "";
    position: absolute;
    inset: 8px;
    border-radius: 50%;
    background: #ffffff;
}

.cmf-quality-score-ring span {
    position: relative;
    z-index: 1;
}

.cmf-quality-caption {
    color: #5d7289;
    font-size: 12px;
    line-height: 1.4;
}

.cmf-quality-columns {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
}

.cmf-quality-column {
    border-radius: 12px;
    padding: 11px 12px;
    border: 1px solid #e7edf4;
    background: #f8fbff;
}

.cmf-quality-column.missing {
    background: #fff7ed;
    border-color: #fed7aa;
}

.cmf-quality-column.strengths {
    background: #f0fdf4;
    border-color: #bbf7d0;
}

.cmf-quality-column-title {
    margin-bottom: 6px;
    font-size: 11px;
    font-weight: 900;
    color: #334155;
}

@media (max-width: 720px) {
    .cmf-decision-header,
    .cmf-quality-meter-row,
    .cmf-quality-columns {
        grid-template-columns: 1fr;
    }
}

.ai-summary-drawer-bg {
    position: fixed;
    inset: 0;
    background: rgba(16, 31, 52, 0.48);
    display: none;
    z-index: 1060;
    backdrop-filter: blur(5px);
}

.ai-summary-drawer-bg.show {
    display: block;
}

.ai-summary-drawer {
    position: fixed;
    top: 0;
    right: 0;
    width: 560px;
    max-width: 94vw;
    height: 100%;
    background:
        radial-gradient(circle at top right, rgba(124, 58, 237, 0.12), transparent 34%),
        linear-gradient(180deg, #ffffff 0%, #f6f9fd 100%);
    box-shadow: -18px 0 42px rgba(8, 25, 49, 0.24);
    transform: translateX(100%);
    transition: transform 0.28s ease;
    z-index: 1061;
    overflow: hidden;
    padding: 24px;
    border-left: 1px solid rgba(205, 217, 229, 0.9);
    display: flex;
    flex-direction: column;
}

.ai-summary-drawer.show {
    transform: translateX(0);
}

.ai-summary-chat {
    border-top: 1px solid rgba(205,217,229,0.8);
    background: #fff;
    padding: 12px 24px;
    margin: 0 -24px -24px -24px;
    flex: 1 1 auto;
    min-height: 0;
    display: flex;
    flex-direction: column;
}
.ai-chat-messages {
    /* make the chat area take most of the drawer height to feel like a chatbot */
    flex: 1 1 auto;
    min-height: calc(100vh - 330px);
    max-height: none;
    overflow-y: auto;
    padding: 8px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 24px;
}
.ai-chat-message { display:flex; flex-direction:column; margin-bottom:12px; font-size:14px; line-height:1.5; }
.ai-chat-message.ai-chat-assistant .ai-chat-message-body { background:#f1f5f9; color:#1a202c; padding:14px 16px; border-radius:12px; max-width:96%; border-left: 4px solid #005A9E; box-shadow: 0 4px 6px rgba(0,0,0,0.05); align-self: flex-start; }
.ai-chat-message.ai-chat-user { align-self:flex-end; max-width: 90%; }
.ai-chat-message.ai-chat-user .ai-chat-message-body { background:#005A9E; color:#fff; padding:12px 14px; border-radius:12px; max-width:100%; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }
.ai-chat-input { display:flex; gap:10px; align-items:flex-end; margin-top:10px; }
.ai-chat-input-box { flex:1; min-height:45px; max-height:140px; padding:10px 14px; border-radius:12px; border:1px solid #cbd5e0; resize:vertical; font-size: 14px; box-shadow: inset 0 1px 2px rgba(0,0,0,0.05); transition: border-color 0.2s; }
.ai-chat-input-box:focus { border-color: #005A9E; outline: none; }
.ai-chat-send { background:transparent; border:none; padding:10px 10px; border-radius:50%; cursor:pointer; min-height: 45px; transition: transform 0.1s ease, color 0.2s; color: #0071c5; display: flex; align-items: center; justify-content: center; }
.ai-chat-send:active { transform: scale(0.95); }
.ai-chat-send:hover { color: #005A9E; background: rgba(0, 113, 197, 0.1); }
.ai-chat-send:disabled { color: #a0aec0; cursor: not-allowed; background: transparent; box-shadow: none; }

.ai-summary-drawer-close {
    position: absolute;
    top: 14px;
    right: 16px;
    border: none;
    background: transparent;
    color: #6b7a90;
    font-size: 24px;
    line-height: 1;
    cursor: pointer;
}

.ai-summary-drawer-title {
    margin: 0 0 12px 0;
    padding-right: 28px;
    font-size: 22px;
    color: #1b2a3a;
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
}

.ai-summary-confidence-inline {
    display: inline-flex;
    align-items: center;
    min-height: 24px;
    padding: 3px 9px;
    border-radius: 999px;
    background: #eef6ff;
    border: 1px solid #c8def6;
    color: #0f5ea8;
    font-size: 12px;
    font-weight: 800;
}

.ai-summary-meta-row {
    margin-bottom: 10px;
    color: #23364b;
    font-size: 13px;
    padding: 9px 11px;
    border: 1px solid #e2ebf4;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.72);
}

.ai-summary-meta-combined {
    display: flex;
    align-items: center;
    gap: 7px;
    flex-wrap: wrap;
}

.ai-summary-meta-row strong {
    color: #1b2a3a;
}

.ai-summary-title-badges {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    flex-wrap: wrap;
}

.ai-summary-facts {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
    margin: 12px 0 14px;
}

.ai-summary-fact {
    min-height: 58px;
    padding: 10px 12px;
    border: 1px solid #e2ebf4;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.76);
    box-shadow: 0 8px 18px rgba(16, 41, 67, 0.05);
}

.ai-summary-fact-label {
    display: block;
    color: #6a7d92;
    font-size: 10px;
    font-weight: 850;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    margin-bottom: 5px;
}

.ai-summary-fact-value {
    color: #19324a;
    font-size: 13px;
    font-weight: 750;
    overflow-wrap: anywhere;
}

.ai-summary-status-value {
    display: inline-flex;
    align-items: center;
    min-height: 24px;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 850;
    border: 1px solid #cfdcea;
    background: #f4f7fb;
    color: #344761;
}

.ai-summary-status-value.status-open,
.ai-summary-status-value.status-active,
.ai-summary-status-value.status-progress {
    background: #eef6ff;
    border-color: #c8def6;
    color: #0f5ea8;
}

.ai-summary-status-value.status-closed,
.ai-summary-status-value.status-complete,
.ai-summary-status-value.status-implemented {
    background: #ecfdf3;
    border-color: #bbf7d0;
    color: #15803d;
}

.ai-summary-status-value.status-rejected {
    background: #fff1f2;
    border-color: #fecdd3;
    color: #be123c;
}

.ai-summary-status-value.status-pending,
.ai-summary-status-value.status-review {
    background: #fffbeb;
    border-color: #fde68a;
    color: #b45309;
}

.ai-summary-section-title {
    margin: 14px 0 8px;
    color: #344761;
    font-size: 12px;
    font-weight: 850;
    text-transform: uppercase;
    letter-spacing: 0.06em;
}

.ai-tag-label {
    display: inline-block;
    font-size: 10px;
    background: #7c3aed;
    color: #fff;
    padding: 2px 7px;
    border-radius: 10px;
    font-weight: 700;
    letter-spacing: .03em;
    vertical-align: middle;
    margin-left: 6px;
}

.ai-summary-actions {
    display: flex;
    gap: 10px;
    margin-top: 18px;
    padding-top: 14px;
    border-top: 1px solid #e8edf3;
    flex-wrap: wrap;
}

.ai-action-btn {
    flex: 1;
    min-width: 130px;
    padding: 10px 16px;
    border-radius: 10px;
    font-size: 13px;
    font-weight: 600;
    cursor: pointer;
    border: 1px solid #d1d9e3;
    background: #f4f6fa;
    color: #23364b;
    transition: background 0.15s, border-color 0.15s;
}

body {
    background:
        radial-gradient(circle at 8% 6%, rgba(0, 199, 253, 0.18), transparent 26%),
        radial-gradient(circle at 92% 10%, rgba(124, 58, 237, 0.13), transparent 30%),
        linear-gradient(180deg, #eef2f7 0%, #e7eef6 100%) !important;
}

.content-wrapper {
    background: rgba(255, 255, 255, 0.78) !important;
    border: 1px solid rgba(220, 229, 240, 0.95) !important;
    box-shadow: 0 1px 3px rgba(16,31,52,.08), 0 18px 44px rgba(16,31,52,.12) !important;
    backdrop-filter: blur(12px) !important;
}

.pane-heading {
    background: linear-gradient(120deg, #00377c 0%, #0068b5 72%, #00c7fd 100%) !important;
    color: #ffffff !important;
    border: 0 !important;
    box-shadow: 0 14px 32px rgba(0, 104, 181, 0.22) !important;
}

#overall_request_details {
    border-spacing: 0 10px !important;
}

#overall_request_details th {
    background: #ffffff !important;
    color: #5d6f84 !important;
    border-top: 1px solid #e6edf5 !important;
    border-bottom: 1px solid #e6edf5 !important;
}

#overall_request_details td {
    background: linear-gradient(180deg, #ffffff 0%, #fbfdff 100%) !important;
    border-top: 1px solid #e4edf6 !important;
    border-bottom: 1px solid #e4edf6 !important;
    box-shadow: 0 10px 24px rgba(16, 41, 67, 0.07) !important;
    transition: transform 0.16s ease, box-shadow 0.16s ease, border-color 0.16s ease;
}

#overall_request_details tr:hover td {
    transform: translateY(-2px);
    border-color: #cfdff0 !important;
    box-shadow: 0 16px 34px rgba(16, 41, 67, 0.12) !important;
}

.ai-summary-btn,
.ai-recommendation-btn,
.ai-summary-btn-compact,
.ai-summary-btn-inline {
    box-shadow: 0 8px 18px rgba(15, 94, 168, 0.12);
}

.ai-action-btn:hover {
    background: #e8edf3;
    border-color: #b0bcc8;
}

.ai-action-copy {
    background: #1b2a3a;
    color: #fff;
    border-color: #1b2a3a;
}

.ai-action-copy:hover {
    background: #2d4460;
    border-color: #2d4460;
}

.ai-action-regen {
    background: #7c3aed;
    color: #fff;
    border-color: #7c3aed;
}

.ai-action-regen:hover {
    background: #6d28d9;
    border-color: #6d28d9;
}


#overall_request_details th .header-dropdown {
    font-size: 11px !important;
    padding: 2px 4px !important;
    width: 120px !important;
    height: 25px !important;
    border: 1px solid #ccc !important;
    border-radius: 3px !important;
    background-color: white !important;
    color: black !important;
}

/* Fix for empty data template */
.empty-data-container #overall_request_details th {
    background-color: #4472C4 !important;
    color: white !important;
    padding: 12px 8px !important;
    border-top: 1px solid #000 !important;
    border-bottom: 1px solid #000 !important;
    border-left: 2px solid #000 !important;
    border-right: 2px solid #000 !important;
}

.empty-data-container #overall_request_details td {
    border-top: 1px solid #000 !important;
    border-bottom: 1px solid #000 !important;
    border-left: 2px solid #000 !important;
    border-right: 2px solid #000 !important;
}

/* Ensure the gridview container doesn't hide content */
#overall_request_details + .gridview-container,
.gridview-container:has(#overall_request_details) {
    overflow: visible !important;
    height: auto !important;
    max-height: none !important;
}

/* Dashboard tab visual match to reference image (scoped to Home panel only) */
.ccip-dashboard-host {
    padding: 0 !important;
    border: 0 !important;
    border-radius: 0 !important;
    background: transparent !important;
    box-shadow: none !important;
}

.ccip-dashboard {
    display: flex;
    flex-direction: column;
    gap: 14px;
}

.ccip-dash-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    padding: 6px 2px 2px;
    flex-wrap: wrap;
}

.ccip-dash-greeting {
    margin: 0;
    font-size: 34px;
    line-height: 1.05;
    font-weight: 800;
    color: #101828;
}

.ccip-dash-sub {
    margin: 6px 0 0;
    color: #475467;
    font-size: 14px;
}

.ccip-dash-updated {
    display: inline-flex;
    align-items: center;
    min-height: 36px;
    border: 1px solid #d9e1ec;
    background: #ffffff;
    color: #4b5563;
    border-radius: 12px;
    padding: 8px 12px;
    font-size: 12px;
    font-weight: 700;
    box-shadow: 0 4px 14px rgba(16, 24, 40, 0.06);
}

.ccip-link-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 36px;
    border-radius: 10px;
    padding: 0 12px;
    border: 1px solid #c9d9f2;
    background: #f6f9ff;
    color: #315bba;
    text-decoration: none;
    font-size: 12px;
    font-weight: 700;
}


.ccip-main-grid {
    display: grid;
    grid-template-columns: minmax(0, 1.45fr) minmax(300px, 0.9fr);
    gap: 12px;
    align-items: stretch;
}

.ccip-main-grid .ccip-design-summary-card {
    grid-column: 1 / -1;
}

.ccip-main-grid .ccip-milestone-summary-card,
.ccip-main-grid .ccip-component-summary-card {
    align-self: flex-start;
    width: 502px;
    min-width: 502px;
    max-width: 502px;
    height: 312px;
    min-height: 312px;
    max-height: 312px;
    display: flex;
    flex-direction: column;
}

.ccip-main-grid .ccip-component-summary-card {
    width: 600px;
    min-width: 600px;
    max-width: 600px;
}

.ccip-main-grid .ccip-milestone-summary-card .ccip-live-table-wrap,
.ccip-main-grid .ccip-component-summary-card .ccip-live-table-wrap {
    width: max-content;
    max-width: 100%;
    max-height: none;
    min-height: 0;
    flex: 1 1 auto;
    overflow-y: auto;
}

.ccip-main-grid .ccip-component-summary-card .ccip-live-table-wrap {
    width: 100%;
    min-width: 100%;
    max-width: 100%;
    overflow-x: auto;
    overflow-y: auto;
}

.ccip-dashboard-overview {
    display: grid;
    grid-template-columns: minmax(0, 7fr) minmax(300px, 3fr);
    gap: 12px;
    align-items: start;
}

.ccip-dashboard-left {
    display: flex;
    flex-direction: column;
    gap: 12px;
    min-width: 0;
}

.ccip-dashboard-overview .ccip-dashboard-left {
    display: grid;
    grid-template-columns: minmax(0, 1fr) minmax(220px, 0.28fr);
    gap: 12px;
    align-items: stretch;
}

.ccip-dashboard-overview .ccip-trend-card-inline {
    grid-column: 1 / -1;
}

.ccip-dashboard-right-stack {
    display: flex;
    flex-direction: column;
    gap: 12px;
    min-width: 0;
}

.ccip-specific-top-layout {
    width: 1840px;
    min-width: 1840px;
    max-width: 1840px;
    display: grid;
    grid-template-columns: 1262px 568px;
    grid-template-rows: auto auto;
    gap: 10px;
    align-items: start;
}

#homeLegacyTopLayout #homeLegacySummarySection {
    grid-column: 1;
    grid-row: 1;
    width: 1130px;
    min-width: 1130px;
    max-width: 1130px;
    display: flex !important;
    flex-direction: row;
    gap: 10px;
    align-items: stretch;
}

#homeLegacyTopLayout #homeLegacySummarySection .overall-summary-side-card {
    flex: 0 0 var(--cmf-summary-side-card-width);
    width: var(--cmf-summary-side-card-width) !important;
    min-width: var(--cmf-summary-side-card-width) !important;
    max-width: var(--cmf-summary-side-card-width) !important;
    min-height: 162px;
    display: flex;
    flex-direction: column;
}

#homeLegacyTopLayout #homeLegacySummarySection .overall-summary-side-card .overall-card-head {
    margin-bottom: 0;
}

#homeLegacyTopLayout #homeLegacySummarySection .overall-summary-side-body {
    flex: 1 1 auto;
    min-height: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #667085;
    font-size: 12px;
    font-weight: 700;
}

#homeLegacyTopLayout #homeLegacySummarySection .overall-summary-card {
    flex: 1 1 auto;
    width: auto !important;
    min-width: 0;
}

#homeLegacyTopLayout #homeSpecificSummaryFactsDashboard.overall-summary-facts,
#homeLegacyTopLayout #homeSpecificSummaryFactsCmf.overall-summary-facts {
    display: flex;
    flex-wrap: nowrap;
    gap: 6px;
    justify-content: center;
    align-items: stretch;
    overflow-x: hidden;
    overflow-y: hidden;
}

#homeLegacyTopLayout #homeLegacyOverview {
    grid-column: 1;
    grid-row: 2;
    display: block;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-dashboard-left {
    display: grid;
    grid-template-columns: 660px var(--ccip-component-slot-width, 437.62px);
    grid-template-rows: 312px;
    gap: 10px;
    align-items: start;
    width: 1130px;
    min-width: 1130px;
    max-width: 1130px;
    border: 1px solid #e4e9f2;
    border-radius: 10px;
    background: #ffffff;
    box-shadow: 0 8px 20px rgba(16, 24, 40, 0.04);
    padding: 10px;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-trend-card-inline.overall-trend-card {
    grid-column: 1;
    grid-row: 1;
    order: 0;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-trend-card-inline.overall-trend-card,
#homeLegacyTopLayout #homeLegacyOverview .ccip-inline-summary-card {
    border: 1px solid #d9dde5 !important;
    border-radius: 10px !important;
    background: transparent;
    box-shadow: none;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot {
    grid-column: 2;
    grid-row: 1;
    width: var(--ccip-component-slot-width, 437.62px);
    min-width: var(--ccip-component-slot-width, 437.62px);
    max-width: var(--ccip-component-slot-width, 437.62px);
    display: flex;
    flex-direction: column;
    min-height: 0;
    height: 312px;
    min-height: 312px;
    max-height: 312px;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap {
    width: max-content;
    min-width: 0;
    max-width: 100%;
    flex: 0 1 auto;
    min-height: 0;
    overflow-x: auto;
    overflow-y: auto;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-inline-summary-card {
    display: flex;
    flex-direction: column;
    min-height: 0;
    height: 312px;
    max-height: 312px;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-inline-summary-card .ccip-card-head {
    margin-bottom: 10px;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-inline-summary-card .ccip-card-mini-title {
    font-size: 16px !important;
    font-weight: 800 !important;
    line-height: 1.2 !important;
    letter-spacing: -0.01em !important;
    text-transform: uppercase;
    color: #1e3a60 !important;
}

#homeLegacyTopLayout #homeLegacyOverview .ccip-inline-summary-card .ccip-live-table-wrap {
    width: 100%;
    min-width: 100%;
    max-width: 100%;
    flex: 1 1 auto;
    min-height: 0;
    overflow-x: auto;
    overflow-y: auto;
}

#homeLegacyTablesGrid .ccip-inline-side-stack {
    display: contents;
}

#homeLegacyTablesGrid .ccip-summary-canvas {
    --ccip-design-lock-width: 871.7px;
    --ccip-third-grid-gap: 10px;
    --ccip-third-side-width: calc((100% - var(--ccip-design-lock-width) - (2 * var(--ccip-third-grid-gap))) / 2);
    --ccip-third-design-width: var(--ccip-design-lock-width);
    --ccip-third-milestone-width: 502px;
    --ccip-third-pending-width: calc(100% - var(--ccip-third-design-width) - var(--ccip-third-milestone-width) - (2 * var(--ccip-third-grid-gap)));
    display: grid;
    grid-template-columns: var(--ccip-third-milestone-width) var(--ccip-third-pending-width) var(--ccip-third-design-width);
    gap: var(--ccip-third-grid-gap);
    width: 100%;
    min-width: 0;
    border: 1px solid #e4e9f2;
    border-radius: 10px;
    background: #ffffff;
    box-shadow: 0 8px 20px rgba(16, 24, 40, 0.04);
    padding: 10px;
}

#homeLegacyTablesGrid {
    width: 1840px;
    min-width: 1840px;
    max-width: 1840px;
}

/* Match platform-specific row-2 to row-3 spacing with overall tab rhythm. */
#homeWelcomePanel .ccip-dashboard > #homeLegacyTablesGrid {
    margin-top: -4px;
}

#homeLegacyTablesGrid .ccip-col.ccip-col-center {
    grid-column: 1 / -1;
}

#homeLegacyTablesGrid .ccip-col.ccip-col-right {
    display: none;
}

#homeLegacyTablesGrid .ccip-summary-canvas .ccip-design-summary-card {
    grid-column: 3;
    grid-row: 1;
    align-self: start;
    display: flex;
    flex-direction: column;
    width: var(--ccip-design-lock-width);
    min-width: var(--ccip-design-lock-width);
    max-width: var(--ccip-design-lock-width);
    height: 312px;
    min-height: 312px;
    max-height: 312px;
}

#homeLegacyTablesGrid .ccip-summary-canvas .ccip-design-summary-card .ccip-live-table-wrap {
    flex: 1 1 auto;
    min-height: 0;
    max-height: none;
    overflow-x: auto;
    overflow-y: auto;
}

#homeLegacyTablesGrid .ccip-inline-side-stack .ccip-inline-summary-card {
    grid-column: 2;
    grid-row: 1;
    display: flex;
    flex-direction: column;
    align-self: start;
    width: 100%;
    min-width: 0;
    max-width: none;
    justify-self: stretch;
    min-height: 0;
    height: 312px;
    max-height: 312px;
}

#homeLegacyTablesGrid .ccip-inline-side-stack .ccip-inline-summary-card.ccip-milestone-summary-card {
    grid-column: 1;
    grid-row: 1;
    width: var(--ccip-third-milestone-width);
    min-width: var(--ccip-third-milestone-width);
    max-width: var(--ccip-third-milestone-width);
}

#homeLegacyTablesGrid .ccip-inline-side-stack .ccip-inline-summary-card.ccip-pending-summary-card {
    width: 420px;
    min-width: 420px;
    max-width: 420px;
    justify-self: start;
}

#homeLegacyTablesGrid .ccip-inline-side-stack .ccip-inline-summary-card .ccip-card-head {
    margin-bottom: 10px;
}

#homeLegacyTablesGrid .ccip-inline-side-stack .ccip-inline-summary-card .ccip-card-mini-title {
    font-size: 16px !important;
    font-weight: 800 !important;
    line-height: 1.2 !important;
    letter-spacing: -0.01em !important;
    text-transform: uppercase;
    color: #1e3a60 !important;
}

#homeLegacyTablesGrid .ccip-inline-side-stack .ccip-inline-summary-card .ccip-live-table-wrap {
    width: max-content;
    min-width: 0;
    max-width: 100%;
    flex: 1 1 auto;
    min-height: 0;
    overflow-x: auto;
    overflow-y: auto;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable.ccip-live-table-wrap {
    width: 100%;
    min-width: 100%;
    max-width: 100%;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table {
    width: max-content;
    min-width: max-content;
    table-layout: auto;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table th:first-child,
#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table td:first-child {
    position: sticky;
    left: 0;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table th:first-child {
    z-index: 5;
    background: #f2f4f7;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table td:first-child {
    z-index: 2;
    background: #ffffff;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table tr.ccip-live-total td:first-child {
    background: #f8fafc !important;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table th:nth-child(2),
#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table td:nth-child(2) {
    white-space: nowrap;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table th:nth-child(3),
#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table td:nth-child(3) {
    white-space: nowrap;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-pending-summary-cell {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    white-space: nowrap;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-pending-summary-count {
    font-weight: 800;
}

#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-pending-summary-link {
    font-weight: 800;
}

#homeLegacyAiHealthCard {
    --ccip-ai-left-extend: 132px;
    --ccip-ai-fixed-height: 506px;
    grid-column: 2;
    grid-row: 1 / span 2;
    margin-left: calc(-1 * var(--ccip-ai-left-extend));
    width: calc(100% + var(--ccip-ai-left-extend));
    min-width: calc(100% + var(--ccip-ai-left-extend));
    max-width: calc(100% + var(--ccip-ai-left-extend));
    height: var(--ccip-ai-fixed-height);
    min-height: var(--ccip-ai-fixed-height);
    max-height: var(--ccip-ai-fixed-height);
    overflow-y: auto;
    overflow-x: hidden;
}

.ccip-trend-card-inline {
    min-height: 312px;
    max-height: 312px;
}

.ccip-ai-health-card {
    background: #ffffff;
    border: 1px solid #e4e9f2;
    border-radius: 8px;
    box-shadow: 0 12px 28px rgba(16, 24, 40, 0.08);
    padding: 12px;
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.ccip-ai-health-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    color: #1e3a60;
    font-family: Outfit, Manrope, "Segoe UI", sans-serif;
    font-size: 16px;
    font-weight: 800;
    line-height: 1.2;
    letter-spacing: -0.01em;
    text-transform: uppercase;
    margin-bottom: 10px;
}

.ccip-ai-beta {
    border-radius: 999px;
    background: #f3e8ff;
    color: #7e22ce;
    padding: 2px 8px;
    font-size: 10px;
    font-weight: 900;
}

.overall-ai-card .ccip-ai-beta {
    font-family: Outfit, Manrope, "Segoe UI", sans-serif;
    line-height: 12px;
    letter-spacing: -0.16px;
    text-transform: uppercase;
}

.ccip-ai-health-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
}

.ccip-ai-health-grid > div {
    border: 1px solid #e4e7ec;
    border-radius: 8px;
    background: #fbfcff;
    padding: 10px;
    min-height: 92px;
}

.ccip-health-score-value {
    width: 74px;
    height: 74px;
    border-radius: 999px;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 6px;
    background: conic-gradient(#22c55e 0 70%, #f59e0b 70% 88%, #ef4444 88% 100%);
    color: #111827;
    font-size: 25px;
    font-weight: 900;
    box-shadow: inset 0 0 0 9px #ffffff;
}

.ccip-health-score-label,
.ccip-health-label {
    color: #475467;
    font-size: 11px;
    font-weight: 800;
}

.ccip-health-risk {
    color: #f97316;
    font-size: 11px;
    font-weight: 900;
    text-align: center;
}

.ccip-health-value {
    margin-top: 6px;
    color: #111827;
    font-size: 14px;
    font-weight: 900;
}

.ccip-health-value.large {
    color: #4f46e5;
    font-size: 28px;
}

.ccip-health-list {
    margin: 6px 0 0;
    padding-left: 16px;
    color: #111827;
    font-size: 12px;
    font-weight: 700;
}

.ccip-ai-health-summary {
    border-top: 1px solid #e4e7ec;
    padding-top: 12px;
}

.ccip-summary-facts {
    display: grid;
    grid-template-columns: repeat(8, minmax(96px, 1fr));
    gap: 8px;
}

.ccip-summary-fact {
    background: #ffffff;
    border: 1px solid #e4e7ec;
    border-radius: 8px;
    padding: 10px;
    text-align: center;
}

.ccip-cmf-summary-section {
    margin-top: 12px;
    border: 1px solid #d0e4ff;
    border-radius: 8px;
    background: #f8fbff;
    padding: 12px;
}

.ccip-specific-summary-row {
    margin-top: 12px;
}

#homeLegacySummarySection.ccip-specific-summary-row {
    margin-top: 0;
}

/* Platform-specific: remove outer section canvas around CMF Summary block. */
#homeLegacySummarySection.ccip-specific-summary-row,
#homeCmfSummaryPanel .overall-row-summary.ccip-specific-summary-row {
    border: 0;
    background: transparent;
    box-shadow: none;
    padding: 0;
}

.ccip-cmf-summary-title {
    color: #12315f;
    font-size: 14px;
    font-weight: 900;
    letter-spacing: 0;
    margin: 0 0 10px;
    display: flex;
    align-items: center;
    gap: 8px;
}

.ccip-executive-summary-card {
    margin-top: 12px;
}

.ccip-executive-summary-body {
    color: #344054;
    font-size: 13px;
    font-weight: 600;
    line-height: 1.55;
}

.ccip-executive-summary-body strong {
    color: #111827;
    font-weight: 900;
}

.ccip-cmf-summary-workspace {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.ccip-summary-fact-label {
    color: #344054;
    font-size: 11px;
    font-weight: 800;
    white-space: nowrap;
}

.ccip-summary-fact-value {
    color: #111827;
    font-size: 28px;
    font-weight: 900;
    line-height: 1.05;
    margin-top: 4px;
    display: inline-flex;
    align-items: baseline;
    justify-content: center;
    gap: 4px;
    white-space: nowrap;
}

.ccip-days-suffix {
    font-size: 16px;
    line-height: 1;
    font-weight: 800;
    display: inline-block;
    white-space: nowrap;
}

.ccip-summary-fact-note {
    color: #667085;
    font-size: 10px;
    font-weight: 800;
    margin-top: 3px;
    white-space: nowrap;
}

.ccip-dashboard-tables-grid {
    grid-template-columns: minmax(0, 1.45fr) minmax(300px, 0.9fr);
}

.ccip-dashboard-charts-grid {
    grid-template-columns: minmax(0, 1.45fr) minmax(300px, 0.9fr);
}

.ccip-live-table-wrap {
    overflow: auto;
    max-height: 360px;
}

#homeLegacyTablesGrid .ccip-live-table-wrap,
#homeCmfSummaryPanel .ccip-live-table-wrap,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap {
    border-radius: 0;
    background: #ffffff;
    scrollbar-width: none;
    scrollbar-color: transparent transparent;
}

#homeLegacyTablesGrid .ccip-live-table-wrap:hover,
#homeCmfSummaryPanel .ccip-live-table-wrap:hover,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap:hover,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap:hover {
    scrollbar-width: thin;
    scrollbar-color: #cbd3dd transparent;
}

#homeLegacyTablesGrid .ccip-live-table-wrap::-webkit-scrollbar,
#homeCmfSummaryPanel .ccip-live-table-wrap::-webkit-scrollbar,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap::-webkit-scrollbar,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap::-webkit-scrollbar {
    width: 0;
    height: 0;
}

#homeLegacyTablesGrid .ccip-live-table-wrap:hover::-webkit-scrollbar,
#homeCmfSummaryPanel .ccip-live-table-wrap:hover::-webkit-scrollbar,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap:hover::-webkit-scrollbar,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap:hover::-webkit-scrollbar {
    width: 8px;
    height: 8px;
}

#homeLegacyTablesGrid .ccip-live-table-wrap::-webkit-scrollbar-track,
#homeCmfSummaryPanel .ccip-live-table-wrap::-webkit-scrollbar-track,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap::-webkit-scrollbar-track,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap::-webkit-scrollbar-track {
    background: transparent;
}

#homeLegacyTablesGrid .ccip-live-table-wrap::-webkit-scrollbar-thumb,
#homeCmfSummaryPanel .ccip-live-table-wrap::-webkit-scrollbar-thumb,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap::-webkit-scrollbar-thumb,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap::-webkit-scrollbar-thumb {
    background: transparent;
    border-radius: 999px;
}

#homeLegacyTablesGrid .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb,
#homeCmfSummaryPanel .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb {
    background: #cbd3dd;
}

#homeLegacyTablesGrid .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb:hover,
#homeCmfSummaryPanel .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb:hover,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb:hover,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table-wrap:hover::-webkit-scrollbar-thumb:hover {
    background: #aeb8c5;
}

#homeLegacyTablesGrid .ccip-summary-canvas .ccip-design-summary-card #homeDesignSummaryTable.ccip-hover-width-lock {
    overflow-x: hidden;
}

#homeLegacyTablesGrid .ccip-summary-canvas .ccip-design-summary-card #homeDesignSummaryTable.ccip-hover-width-lock .ccip-live-table {
    width: calc(100% + var(--ccip-hover-comp, 0px));
    min-width: calc(100% + var(--ccip-hover-comp, 0px));
}

.ccip-live-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 12px;
}

.ccip-live-table.ccip-component-summary-table {
    width: auto;
    table-layout: fixed;
    border-collapse: separate;
    border-spacing: 0;
}

.ccip-live-table.ccip-component-summary-table td {
    white-space: nowrap;
}

.ccip-live-table.ccip-component-summary-table tr > *:first-child {
    width: 92px;
    min-width: 92px;
    max-width: 92px;
}

.ccip-live-table.ccip-milestone-summary-table {
    width: 100%;
    max-width: 100%;
    table-layout: fixed;
}

.ccip-live-table.ccip-milestone-summary-table th,
.ccip-live-table.ccip-milestone-summary-table td {
    white-space: normal;
    overflow-wrap: anywhere;
    word-break: break-word;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-design-summary-table td:first-child,
#homeCmfSummaryPanel .ccip-live-table.ccip-design-summary-table td:first-child,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table.ccip-design-summary-table td:first-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-design-summary-table td:first-child {
    white-space: normal;
    overflow-wrap: anywhere;
    word-break: break-word;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-design-summary-table th:not(:first-child),
#homeLegacyTablesGrid .ccip-live-table.ccip-design-summary-table td:not(:first-child),
#homeCmfSummaryPanel .ccip-live-table.ccip-design-summary-table th:not(:first-child),
#homeCmfSummaryPanel .ccip-live-table.ccip-design-summary-table td:not(:first-child),
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table.ccip-design-summary-table th:not(:first-child),
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table.ccip-design-summary-table td:not(:first-child),
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-design-summary-table th:not(:first-child),
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-design-summary-table td:not(:first-child) {
    text-align: center;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-design-summary-table td,
#homeCmfSummaryPanel .ccip-live-table.ccip-design-summary-table td,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table.ccip-design-summary-table td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-design-summary-table td {
    white-space: normal;
    overflow-wrap: anywhere;
    word-break: break-word;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-design-summary-table th.ccip-design-edit-col,
#homeLegacyTablesGrid .ccip-live-table.ccip-design-summary-table td.ccip-design-edit-col,
#homeCmfSummaryPanel .ccip-live-table.ccip-design-summary-table th.ccip-design-edit-col,
#homeCmfSummaryPanel .ccip-live-table.ccip-design-summary-table td.ccip-design-edit-col,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table.ccip-design-summary-table th.ccip-design-edit-col,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table.ccip-design-summary-table td.ccip-design-edit-col,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-design-summary-table th.ccip-design-edit-col,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-design-summary-table td.ccip-design-edit-col {
    width: 72px;
    min-width: 72px;
    max-width: 72px;
    text-align: center;
    white-space: nowrap;
}

.ccip-design-edit-link {
    display: inline-block;
    padding: 2px 8px;
    border-radius: 999px;
    border: 1px solid #cbd5e1;
    background: #ffffff;
    color: #1d4ed8;
    font-size: 11px;
    font-weight: 700;
    text-decoration: none;
    line-height: 1.4;
}

.ccip-design-edit-link:hover {
    background: #eff6ff;
    border-color: #93c5fd;
}

.ccip-design-metric-link {
    color: #1d4ed8;
    font-weight: 700;
    text-decoration: none;
    cursor: pointer;
}

.ccip-design-metric-link:hover {
    color: #1e40af;
    text-decoration: underline;
}

.ccip-design-metric-link.is-total {
    color: #1d4ed8;
}

.ccip-design-edit-actions {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
}

.ccip-design-edit-actions .ccip-design-edit-link {
    padding: 1px 6px;
}

.ccip-design-inline-field {
    width: 100%;
    min-width: 0;
    min-height: 28px;
    max-height: 86px;
    resize: vertical;
    border: 1px solid #cbd5e1;
    border-radius: 8px;
    background: #ffffff;
    color: #0f172a;
    font-size: 11px;
    line-height: 1.35;
    padding: 4px 6px;
    box-sizing: border-box;
}

.ccip-design-row-editing td {
    background: #f8fbff;
}

.design-summary-name-cell {
    display: flex;
    align-items: center;
    gap: 6px;
    min-width: 0;
}

.design-summary-logo {
    width: 14px;
    height: 14px;
    border-radius: 3px;
    object-fit: contain;
    flex: 0 0 auto;
    background: #ffffff;
}

.design-summary-name-text {
    min-width: 0;
    white-space: normal;
    overflow-wrap: anywhere;
    word-break: break-word;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-milestone-summary-table thead th,
#homeLegacyTablesGrid .ccip-inline-side-stack #homePendingSummaryTable .ccip-live-table thead th {
    white-space: nowrap;
    overflow-wrap: normal;
    word-break: normal;
}

.ccip-live-table th {
    position: sticky;
    top: 0;
    z-index: 1;
    background: #244aaf;
    color: #ffffff;
    padding: 8px;
    text-align: left;
    white-space: nowrap;
}

.ccip-live-table.ccip-component-summary-table thead th {
    border-left: 0 !important;
    border-right: 1px solid #1f2937 !important;
}

.ccip-live-table.ccip-component-summary-table thead tr:first-child th:first-child {
    left: 0;
    z-index: 6;
    border-left: 1px solid #1f2937 !important;
    border-right: 2px solid #1f2937 !important;
}

.ccip-live-table.ccip-component-summary-table thead th:last-child {
    border-right: 0 !important;
}

.ccip-live-table .ccip-component-group-header {
    text-align: left;
}

.ccip-live-table .ccip-component-sub-header {
    border-top: 0;
    text-align: left;
}

.ccip-live-table.ccip-component-summary-table thead tr:first-child th {
    top: 0;
    z-index: 3;
    box-sizing: border-box;
    height: 36px;
    min-height: 36px;
    border-bottom: 2px solid #1f2937;
}

.ccip-live-table.ccip-component-summary-table .ccip-component-sub-header {
    top: 36px;
    z-index: 2;
    box-sizing: border-box;
    height: 50px;
    min-height: 50px;
}

.ccip-live-table.ccip-component-summary-table tbody td:first-child {
    position: sticky;
    left: 0;
    z-index: 1;
    border-left: 1px solid #e4e7ec !important;
    border-right: 2px solid #1f2937 !important;
}

.ccip-live-table.ccip-component-summary-table tbody td {
    border-left: 0 !important;
    border-right: 1px solid #e4e7ec !important;
}

.ccip-live-table.ccip-component-summary-table tbody td:last-child {
    border-right: 0 !important;
}

.ccip-live-table .ccip-header-format {
    display: block;
    font-size: 11px;
    font-weight: 600;
    opacity: 0.92;
    margin-top: 2px;
    line-height: 1.2;
}

.ccip-live-table td {
    border: 1px solid #e4e7ec;
    padding: 8px;
    color: #111827;
    background: #ffffff;
}

.ccip-live-table tbody tr:nth-child(even) td {
    background: #f8fbff;
}

.ccip-live-table .ccip-live-total td {
    background: #ffb340 !important;
    color: #111827;
    font-weight: 900;
}

/* Match platform-specific table visuals to Overall OEM table style. */
#homeLegacyTablesGrid .ccip-live-table,
#homeCmfSummaryPanel .ccip-live-table,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table {
    border-collapse: separate;
    border-spacing: 0;
    border-radius: 0;
}

#homeLegacyTablesGrid .ccip-live-table th,
#homeCmfSummaryPanel .ccip-live-table th,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table th,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table th {
    background: #f2f4f7;
    color: #344054;
    padding: 7px;
    border-top: 1px solid #d9dde5;
    border-right: 1px solid #d9dde5;
    border-bottom: 1px solid #d9dde5;
    border-left: 0;
    font-size: 10px;
    letter-spacing: 0.02em;
    text-transform: uppercase;
    background-clip: padding-box;
}

#homeLegacyTablesGrid .ccip-live-table td,
#homeCmfSummaryPanel .ccip-live-table td,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table td {
    background: #ffffff;
    color: #1f2937;
    border-top: 0;
    border-right: 1px solid #d9dde5;
    border-bottom: 1px solid #d9dde5;
    border-left: 0;
    padding: 7px;
}

#homeLegacyTablesGrid .ccip-live-table tr > *:first-child,
#homeCmfSummaryPanel .ccip-live-table tr > *:first-child,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table tr > *:first-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table tr > *:first-child {
    border-left: 0;
}

#homeLegacyTablesGrid .ccip-live-table tr > *:last-child,
#homeCmfSummaryPanel .ccip-live-table tr > *:last-child,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table tr > *:last-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table tr > *:last-child {
    border-right: 0;
}

#homeLegacyTablesGrid .ccip-live-table tbody tr:last-child td,
#homeCmfSummaryPanel .ccip-live-table tbody tr:last-child td,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table tbody tr:last-child td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table tbody tr:last-child td {
    border-bottom: 0;
}

#homeLegacyTablesGrid .ccip-live-table tbody tr:nth-child(even) td,
#homeCmfSummaryPanel .ccip-live-table tbody tr:nth-child(even) td,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table tbody tr:nth-child(even) td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table tbody tr:nth-child(even) td {
    background: #ffffff;
}

#homeLegacyTablesGrid .ccip-live-table .ccip-live-total td,
#homeCmfSummaryPanel .ccip-live-table .ccip-live-total td,
#homeLegacyOverview .ccip-inline-side-stack .ccip-live-table .ccip-live-total td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table .ccip-live-total td {
    background: #f8fafc !important;
    color: #1f2937;
    font-weight: 800;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table thead th,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table thead th,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table thead th {
    border-right: 1px solid #d9dde5 !important;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table thead tr:first-child th:first-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table thead tr:first-child th:first-child,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table thead tr:first-child th:first-child {
    border-left: 0 !important;
    border-right: 1px solid #d9dde5 !important;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table thead tr:first-child th,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table thead tr:first-child th,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table thead tr:first-child th {
    border-bottom: 1px solid #d9dde5;
}

#homeLegacyTablesGrid .ccip-live-table .ccip-component-sub-header,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table .ccip-component-sub-header,
#homeCmfSummaryPanel .ccip-live-table .ccip-component-sub-header {
    border-top: 0;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table tbody td:first-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table tbody td:first-child,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table tbody td:first-child {
    border-left: 0 !important;
    border-right: 1px solid #d9dde5 !important;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table tbody td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table tbody td,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table tbody td {
    border-right: 1px solid #d9dde5 !important;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table thead th:last-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table thead th:last-child,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table thead th:last-child,
#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table tbody td:last-child,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table tbody td:last-child,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table tbody td:last-child {
    border-right: 0 !important;
}

#homeLegacyTablesGrid .ccip-live-table.ccip-component-summary-table tbody tr:last-child td,
#homeLegacyTopLayout #homeLegacyOverview .ccip-component-slot .ccip-live-table.ccip-component-summary-table tbody tr:last-child td,
#homeCmfSummaryPanel .ccip-live-table.ccip-component-summary-table tbody tr:last-child td {
    border-bottom: 0 !important;
}

.ccip-empty-state {
    color: #667085;
    font-size: 12px;
    font-weight: 700;
    padding: 14px;
    border: 1px dashed #cbd5e1;
    border-radius: 8px;
}

.ccip-col {
    display: flex;
    flex-direction: column;
    gap: 12px;
    min-width: 0;
}

.ccip-card {
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    box-shadow: 0 8px 20px rgba(16, 24, 40, 0.04);
    padding: 14px;
    min-width: 0;
}

.ccip-card-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 10px;
}

.ccip-card-title {
    margin: 0;
    font-size: 26px;
    color: #101828;
    font-weight: 800;
}

.ccip-card-subtitle {
    margin: 0;
    font-size: 12px;
    color: #667085;
    font-weight: 700;
}

.ccip-card-mini-title {
    margin: 0;
    color: #111827;
    font-size: 14px;
    font-weight: 800;
}

.ccip-card-link {
    color: #315bba;
    text-decoration: none;
    font-size: 12px;
    font-weight: 700;
}

.ccip-card-link:hover {
    text-decoration: underline;
}

.ccip-daily-list {
    margin: 0;
    padding-left: 0;
    list-style: none;
    display: grid;
    gap: 8px;
}

.ccip-daily-list li {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    font-size: 13px;
    color: #1f2937;
}

.ccip-daily-list li:before {
    content: '\\f05a';
    font-family: 'Font Awesome 6 Free';
    font-weight: 900;
    color: #3b82f6;
    font-size: 11px;
    margin-top: 2px;
}

.ccip-daily-actions {
    margin-top: 12px;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
}

.ccip-button {
    border-radius: 8px;
    min-height: 34px;
    font-size: 12px;
    font-weight: 700;
    border: 1px solid #c8d7f3;
    background: #f5f8ff;
    color: #315bba;
}

.ccip-button.primary {
    background: #3b5bdb;
    color: #ffffff;
    border-color: #3b5bdb;
}

.ccip-rec-list {
    display: grid;
    gap: 8px;
}

.ccip-rec-row {
    display: grid;
    grid-template-columns: 98px 1fr 46px;
    gap: 8px;
    align-items: center;
}

.ccip-rec-label {
    color: #344054;
    font-size: 12px;
    font-weight: 700;
}

.ccip-rec-track {
    width: 100%;
    height: 8px;
    border-radius: 999px;
    background: #eef2f7;
    overflow: hidden;
}

.ccip-rec-fill {
    height: 100%;
    border-radius: 999px;
}

.ccip-rec-fill.good { background: #4ade80; }
.ccip-rec-fill.medium { background: #fb923c; }
.ccip-rec-fill.low { background: #ef4444; }

.ccip-rec-percent {
    text-align: right;
    color: #667085;
    font-size: 11px;
    font-weight: 800;
}

.ccip-trend-chart {
    height: 218px;
}

.ccip-priority-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 12px;
}

.ccip-priority-table th,
.ccip-priority-table td {
    padding: 8px 6px;
    border-bottom: 1px solid #eef2f7;
    text-align: left;
}

.ccip-priority-table th {
    font-size: 11px;
    color: #667085;
    font-weight: 800;
}

.ccip-severity {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 20px;
    min-width: 54px;
    border-radius: 999px;
    font-size: 10px;
    font-weight: 800;
    padding: 0 8px;
}

.ccip-severity.critical { background: #fee2e2; color: #dc2626; }
.ccip-severity.high { background: #ffedd5; color: #f97316; }
.ccip-severity.medium { background: #fef3c7; color: #ca8a04; }

.ccip-confidence {
    color: #16a34a;
    font-weight: 800;
}

.ccip-status-pill {
    display: inline-flex;
    align-items: center;
    min-height: 20px;
    border-radius: 999px;
    padding: 0 8px;
    background: #eef2ff;
    color: #4f46e5;
    font-size: 10px;
    font-weight: 800;
}

.ccip-region-wrap {
    display: grid;
    grid-template-columns: 1.25fr 0.95fr;
    gap: 10px;
    align-items: center;
}

.ccip-region-map {
    min-height: 138px;
    border-radius: 10px;
    background:
        radial-gradient(circle at 20% 36%, #9bb2e9 0 10px, transparent 11px),
        radial-gradient(circle at 55% 32%, #5ecf7a 0 10px, transparent 11px),
        radial-gradient(circle at 72% 47%, #f5a53a 0 10px, transparent 11px),
        radial-gradient(circle at 34% 76%, #a78bfa 0 10px, transparent 11px),
        linear-gradient(180deg, #f8fbff 0%, #f3f7fd 100%);
    border: 1px solid #e5ecf7;
    position: relative;
    overflow: hidden;
}

.ccip-region-map:before {
    content: '';
    position: absolute;
    inset: 8px;
    border-radius: 8px;
    background:
        radial-gradient(ellipse at 22% 38%, rgba(153, 167, 187, 0.35) 0 17%, transparent 18%),
        radial-gradient(ellipse at 50% 45%, rgba(153, 167, 187, 0.35) 0 16%, transparent 17%),
        radial-gradient(ellipse at 72% 45%, rgba(153, 167, 187, 0.35) 0 14%, transparent 15%),
        radial-gradient(ellipse at 33% 72%, rgba(153, 167, 187, 0.35) 0 11%, transparent 12%);
}

.ccip-region-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: grid;
    gap: 8px;
}

.ccip-region-list li {
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-size: 12px;
    color: #344054;
}

.ccip-dot {
    display: inline-block;
    width: 9px;
    height: 9px;
    border-radius: 999px;
    margin-right: 8px;
}

.ccip-dot.blue { background: #4f79ff; }
.ccip-dot.green { background: #4caf64; }
.ccip-dot.orange { background: #f5a63b; }
.ccip-dot.purple { background: #8b5cf6; }
.ccip-dot.red { background: #ef4444; }

.ccip-product-wrap {
    display: grid;
    grid-template-columns: 120px 1fr;
    gap: 10px;
    align-items: center;
}

.ccip-product-chart {
    width: 120px;
    height: 120px;
}

.ccip-product-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: grid;
    gap: 8px;
}

.ccip-product-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    color: #344054;
    font-size: 12px;
}

.ccip-product-name {
    display: inline-flex;
    align-items: center;
    min-width: 0;
}

.ccip-product-value {
    color: #475467;
    font-weight: 700;
    white-space: nowrap;
}

.ccip-quick-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px;
}

.ccip-quick-btn {
    min-height: 40px;
    border-radius: 10px;
    border: 1px solid #d9e1ec;
    background: #ffffff;
    color: #344054;
    font-size: 12px;
    font-weight: 700;
}

.overall-layout-v2 {
    display: grid;
    grid-template-columns: minmax(0, 7fr) minmax(300px, 3fr);
    grid-template-rows: auto auto auto;
    gap: 10px;
    align-items: stretch;
}

.overall-row {
    border: 1px solid #d8e2f0;
    border-radius: 12px;
    background: #ffffff;
    box-shadow: 0 8px 20px rgba(16, 24, 40, 0.05);
    padding: 10px;
}

.overall-row-title {
    margin: 0 0 10px;
    color: #143563;
    font-size: 15px;
    font-weight: 900;
    text-transform: uppercase;
    display: flex;
    align-items: center;
    gap: 6px;
}

.overall-row-summary {
    display: grid;
    grid-template-columns: minmax(0, 2.55fr) minmax(0, 1.45fr);
    gap: 10px;
    align-items: stretch;
    grid-column: 1;
    grid-row: 1;
}

#homeOverallLayoutV2 .overall-row-summary {
    grid-template-columns: 1fr !important;
}

#homeLegacySummarySection.overall-row-summary,
#homeCmfSummaryPanel .overall-row-summary {
    grid-template-columns: 1fr !important;
}

/* Overall tab: remove outer section/card chrome around CMF Summary cards. */
#homeOverallLayoutV2 > .overall-row-summary {
    border: 0;
    background: transparent;
    box-shadow: none;
    padding: 0;
    display: flex !important;
    flex-direction: row;
    align-items: stretch;
    gap: 10px;
    width: var(--cmf-summary-main-width);
    min-width: var(--cmf-summary-main-width);
    max-width: var(--cmf-summary-main-width);
}

#homeOverallLayoutV2 .overall-row-summary .overall-summary-card {
    border: 1px solid #e4e9f2;
    background: #ffffff;
    box-shadow: 0 8px 20px rgba(16, 24, 40, 0.04);
    border-radius: 10px;
    padding: 9px;
    flex: 1 1 auto;
    width: auto;
    min-width: 0;
    justify-self: start;
}

#homeOverallLayoutV2 .overall-row-summary .overall-summary-side-card {
    flex: 0 0 var(--cmf-summary-side-card-width);
    width: var(--cmf-summary-side-card-width);
    min-width: var(--cmf-summary-side-card-width);
    max-width: var(--cmf-summary-side-card-width);
    min-height: 162px;
    display: flex;
    flex-direction: column;
}

#homeOverallLayoutV2 .overall-row-summary .overall-summary-side-card .overall-card-head {
    margin-bottom: 0;
}

#homeOverallLayoutV2 .overall-row-summary .overall-summary-side-body {
    flex: 1 1 auto;
    min-height: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    padding-bottom: 0;
    color: #667085;
    font-size: 12px;
    font-weight: 700;
}

.overall-summary-card {
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.overall-summary-card .overall-summary-facts {
    flex: 1 1 auto;
}

.overall-summary-facts {
    display: flex;
    flex-wrap: nowrap;
    gap: 8px;
    overflow-x: hidden;
    overflow-y: hidden;
    padding-bottom: 0;
    align-items: stretch;
    height: 100%;
}

#homeOverallLayoutV2,
#homeLegacyTopLayout,
#homeCmfSummaryPanel {
    --cmf-summary-inner-card-width: 115px;
    --cmf-summary-side-card-width: calc(var(--cmf-summary-inner-card-width) + 20px);
    --cmf-summary-main-width: calc(1130px - var(--cmf-summary-side-card-width) - 10px);
}

#homeOverallSummaryFacts.overall-summary-facts,
#homeSpecificSummaryFactsDashboard.overall-summary-facts,
#homeSpecificSummaryFactsCmf.overall-summary-facts {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(var(--cmf-summary-inner-card-width), 1fr));
    gap: 6px;
    align-items: stretch;
    justify-content: stretch;
    overflow-x: auto;
    overflow-y: hidden;
}

#homeOverallLayoutV2 #homeOverallSummaryFacts.overall-summary-facts {
    display: flex;
    flex-wrap: nowrap;
    gap: 6px;
    justify-content: center;
    align-items: stretch;
    overflow-x: hidden;
    overflow-y: hidden;
}

#homeLegacyTopLayout #homeSpecificSummaryFactsDashboard.overall-summary-facts,
#homeLegacyTopLayout #homeSpecificSummaryFactsCmf.overall-summary-facts,
#homeCmfSummaryPanel #homeSpecificSummaryFactsCmf.overall-summary-facts {
    display: flex;
    flex-wrap: nowrap;
    gap: 6px;
    justify-content: center;
    align-items: stretch;
    overflow-x: hidden;
    overflow-y: hidden;
}

#homeOverallSummaryFacts .overall-summary-facts-core,
#homeSpecificSummaryFactsDashboard .overall-summary-facts-core,
#homeSpecificSummaryFactsCmf .overall-summary-facts-core {
    display: contents;
}

#homeOverallSummaryFacts .overall-summary-tpt-inline,
#homeSpecificSummaryFactsDashboard .overall-summary-tpt-inline,
#homeSpecificSummaryFactsCmf .overall-summary-tpt-inline {
    display: contents;
}

#homeOverallSummaryFacts .overall-summary-fact,
#homeSpecificSummaryFactsDashboard .overall-summary-fact,
#homeSpecificSummaryFactsCmf .overall-summary-fact {
    flex: 0 0 var(--cmf-summary-inner-card-width);
    width: var(--cmf-summary-inner-card-width);
    min-width: var(--cmf-summary-inner-card-width);
    max-width: var(--cmf-summary-inner-card-width);
    height: 117px;
    min-height: 117px;
    max-height: 117px;
}

#homeOverallSummaryFacts .overall-tpt-box,
#homeSpecificSummaryFactsDashboard .overall-tpt-box,
#homeSpecificSummaryFactsCmf .overall-tpt-box {
    flex: 0 0 var(--cmf-summary-inner-card-width);
    width: var(--cmf-summary-inner-card-width);
    min-width: var(--cmf-summary-inner-card-width);
    max-width: var(--cmf-summary-inner-card-width);
    height: 117px;
    min-height: 117px;
    max-height: 117px;
}

.overall-summary-facts .overall-summary-fact {
    flex: 0 0 var(--cmf-summary-inner-card-width);
    width: var(--cmf-summary-inner-card-width);
    min-width: var(--cmf-summary-inner-card-width);
    max-width: var(--cmf-summary-inner-card-width);
    height: 100%;
    min-height: 100%;
    max-height: 100%;
    text-align: center;
    border-width: 1px;
    box-sizing: border-box;
    cursor: help;
    display: grid;
    grid-template-rows: auto 1fr auto;
    align-items: center;
    padding: 10px 8px;
}

.overall-summary-facts .overall-summary-fact.ccip-summary-clickable {
    cursor: pointer;
    transition: border-color 140ms ease;
}

.overall-summary-facts .overall-summary-fact.ccip-summary-clickable:hover,
.overall-summary-facts .overall-summary-fact.ccip-summary-clickable:focus-visible {
    border-color: #2563eb;
}

.overall-summary-fact .ccip-summary-fact-label {
    color: #475467;
    font-size: 10px;
    text-transform: uppercase;
    letter-spacing: 0.03em;
    text-align: center;
    white-space: normal;
    line-height: 1.2;
    align-self: start;
}

.overall-summary-fact .ccip-summary-fact-value {
    font-size: 30px;
    margin-top: 0;
    align-self: center;
}

.overall-summary-fact .ccip-summary-fact-note {
    font-size: 10px;
    text-align: center;
    white-space: normal;
    line-height: 1.2;
    margin-top: 0;
    align-self: end;
}

.overall-summary-side-facts {
    display: flex;
    flex-wrap: nowrap;
    justify-content: center;
    align-items: stretch;
    width: 100%;
    overflow: hidden;
    margin-top: 12px;
}

.overall-summary-side-facts .overall-summary-fact {
    flex: 0 0 var(--cmf-summary-inner-card-width) !important;
    width: var(--cmf-summary-inner-card-width) !important;
    min-width: var(--cmf-summary-inner-card-width) !important;
    max-width: var(--cmf-summary-inner-card-width) !important;
    height: 117px !important;
    min-height: 117px !important;
    max-height: 117px !important;
}

.overall-summary-fact.tone-total {
    background: #eff6ff;
    border-color: #bfdbfe;
}

.overall-summary-fact.tone-total .ccip-summary-fact-value {
    color: #1d4ed8;
}

.overall-summary-fact.tone-closed {
    background: #ecfdf3;
    border-color: #abefc6;
}

.overall-summary-fact.tone-closed .ccip-summary-fact-value {
    color: #067647;
}

.overall-summary-fact.tone-implemented {
    background: #f0f9ff;
    border-color: #bae6fd;
}

.overall-summary-fact.tone-implemented .ccip-summary-fact-value {
    color: #0369a1;
}

.overall-summary-fact.tone-open {
    background: #fff4ed;
    border-color: #ffccba;
}

.overall-summary-fact.tone-open .ccip-summary-fact-value {
    color: #c2410c;
}

.overall-summary-fact.tone-pending {
    background: #fffbeb;
    border-color: #fde68a;
}

.overall-summary-fact.tone-pending .ccip-summary-fact-value {
    color: #b45309;
}

.overall-row-second {
    display: grid;
    grid-template-columns: 660px minmax(0, 1fr);
    gap: 10px;
    grid-column: 1;
    grid-row: 2;
}

.overall-row-third {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 810px;
    gap: 10px;
    grid-column: 1 / -1;
    grid-row: 3;
}

.overall-ai-span-card {
    grid-column: 2;
    grid-row: 1 / span 2;
    display: flex;
    flex-direction: column;
}

.overall-card {
    border: 1px solid #e4e9f2;
    border-radius: 10px;
    background: #ffffff;
    padding: 9px;
    min-width: 0;
}

.overall-card-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 6px;
}

.overall-card-title {
    margin: 0;
    color: #1e3a60;
    font-size: 13px;
    font-weight: 900;
    text-transform: uppercase;
}

.overall-trend-range {
    display: inline-flex;
    align-items: center;
    gap: 6px;
}

.overall-trend-range-select {
    min-width: 138px;
    height: 28px;
    border: 1px solid #cdd9e5;
    border-radius: 8px;
    background: #ffffff;
    color: #1f2937;
    font-size: 11px;
    font-weight: 700;
    padding: 0 8px;
}

.overall-ai-blank {
    min-height: 226px;
    border: 1px dashed #cfd9e8;
    border-radius: 8px;
    background: #f8fbff;
}

.overall-oem-table-wrap {
    overflow-y: auto;
    overflow-x: hidden;
    max-height: 226px;
}

/* Overall tab: remove outer section chrome around OEM table. */
.overall-row-second .overall-oem-card {
    border: 0;
    background: transparent;
    padding: 0;
    box-shadow: none;
}

.overall-row-second .overall-oem-card .overall-card-head {
    display: flex;
}

.overall-row-second .overall-trend-card {
    width: 660px;
    min-width: 660px;
    max-width: 660px;
    grid-column: 1 !important;
}

.overall-row-second .overall-pending-card {
    grid-column: 2 !important;
    width: 445px;
    min-width: 445px;
    max-width: 445px;
    justify-self: start;
}

.overall-oem-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 12px;
}

.overall-oem-table th {
    position: sticky;
    top: 0;
    z-index: 1;
    background: #f2f4f7;
    color: #344054;
    padding: 7px;
    text-align: left;
    white-space: nowrap;
    font-size: 10px;
    letter-spacing: 0.02em;
    text-transform: uppercase;
    border: 1px solid #d9dde5;
}

.overall-oem-table th:nth-child(1),
.overall-oem-table td:nth-child(1) { width: 22%; min-width: 190px; }

.overall-oem-table th:nth-child(2),
.overall-oem-table td:nth-child(2) { width: 11%; }

.overall-oem-table th:nth-child(3),
.overall-oem-table td:nth-child(3) { width: 9%; }

.overall-oem-table th:nth-child(4),
.overall-oem-table td:nth-child(4) { width: 10%; }

.overall-oem-table th:nth-child(5),
.overall-oem-table td:nth-child(5) { width: 10%; }

.overall-oem-table th:nth-child(6),
.overall-oem-table td:nth-child(6) { width: 11%; }

.overall-oem-table th:nth-child(7),
.overall-oem-table td:nth-child(7) { width: 15%; }

.overall-oem-table th:nth-child(8),
.overall-oem-table td:nth-child(8) { width: 12%; }

.overall-oem-table td {
    border: 1px solid #d9dde5;
    padding: 7px;
    color: #1f2937;
    background: #ffffff;
    white-space: nowrap;
}

.overall-oem-table th:nth-child(n+2),
.overall-oem-table td:nth-child(n+2) {
    text-align: center;
}

.overall-oem-metric-total {
    color: #1d4ed8;
    font-weight: 800;
}

.overall-oem-metric-open {
    color: #c2410c;
    font-weight: 800;
}

.overall-oem-metric-pending {
    color: #b45309;
    font-weight: 800;
}

.overall-oem-metric-implemented {
    color: #0369a1;
    font-weight: 800;
}

.overall-oem-metric-closed {
    color: #067647;
    font-weight: 800;
}

.overall-oem-metric-link {
    color: #1d4ed8 !important;
    font-weight: 700 !important;
    text-decoration: none;
    cursor: pointer;
}

.overall-oem-metric-link:hover {
    color: #1e40af !important;
    text-decoration: underline;
}

.overall-oem-name-cell {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    min-width: 0;

.overall-oem-name-link {
    color: inherit;
    text-decoration: none;
}

.overall-oem-name-link:hover {
    text-decoration: underline;
}
}

.overall-oem-logo {
    width: 16px;
    height: 16px;
    border-radius: 3px;
    object-fit: contain;
    flex: 0 0 auto;
    background: #ffffff;
}

.overall-oem-name-text {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.overall-oem-trend {
    display: inline-grid;
    grid-template-columns: 12px auto;
    align-items: center;
    column-gap: 6px;
    justify-content: center;
    min-width: 64px;
}

.overall-oem-trend-arrow {
    width: 14px;
    text-align: center;
    line-height: 1;
    font-size: 16px;
    font-weight: 900;
}

.overall-oem-trend-value {
    min-width: 44px;
    text-align: center;
    font-variant-numeric: tabular-nums;
}

.overall-risk-pill {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-height: 20px;
    border-radius: 999px;
    padding: 0 8px;
    font-size: 10px;
    font-weight: 800;
}

.overall-risk-pill.high { background: #fee2e2; color: #b42318; }
.overall-risk-pill.medium { background: #fef3c7; color: #b54708; }
.overall-risk-pill.low { background: #dcfce7; color: #067647; }

.overall-trend-up { color: #067647; font-weight: 800; }
.overall-trend-down { color: #b42318; font-weight: 800; }
.overall-trend-flat { color: #475467; font-weight: 800; }

.overall-trend-chart {
    height: 226px;
}

.overall-tpt-boxes {
    display: grid;
    grid-template-columns: repeat(3, 117px);
    gap: 6px;
    align-items: stretch;
    justify-content: center;
}

.overall-tpt-card {
    display: flex;
    flex-direction: column;
    aspect-ratio: 1 / 1;
    box-sizing: border-box;
}

.overall-tpt-card .overall-tpt-boxes {
    flex: 1;
}

.overall-tpt-box {
    border: 1px solid #d9e1ee;
    border-radius: 9px;
    background: #f8fbff;
    width: 117px;
    min-width: 117px;
    max-width: 117px;
    min-height: 117px;
    box-sizing: border-box;
    padding: 10px 8px;
    display: grid;
    grid-template-rows: auto 1fr auto;
    align-items: center;
    text-align: center;
    cursor: help;
}

.overall-tpt-label {
    color: #475467;
    font-size: 10px;
    font-weight: 800;
    line-height: 1.2;
    text-align: center;
    align-self: start;
}

.overall-tpt-value {
    margin-top: 0;
    color: #101828;
    font-size: 28px;
    line-height: 1;
    font-weight: 900;
    align-self: center;
    display: inline-flex;
    align-items: baseline;
    justify-content: center;
    gap: 4px;
    white-space: nowrap;
}

.overall-tpt-value.overall-tpt-value-good {
    color: #067647;
}

.overall-tpt-value.overall-tpt-value-bad {
    color: #b42318;
}

.overall-tpt-value-unit {
    font-size: 16px;
    line-height: 1;
    font-weight: 800;
}

.overall-tpt-note {
    margin-top: 0;
    font-size: 10px;
    font-weight: 800;
    line-height: 1.25;
    text-align: center;
    align-self: end;
    border-radius: 999px;
    padding: 2px 8px;
}

.overall-tpt-note.overall-tpt-trend-better {
    color: #067647;
    background: #ecfdf3;
    border: 1px solid #abefc6;
}

.overall-tpt-note.overall-tpt-trend-neutral {
    color: #b54708;
    background: #fffaeb;
    border: 1px solid #fedf89;
}

.overall-tpt-note.overall-tpt-trend-worse {
    color: #b42318;
    background: #fef3f2;
    border: 1px solid #fecdca;
}

.overall-tpt-legend {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
    flex-wrap: nowrap;
    white-space: nowrap;
    font-size: 9px;
    font-weight: 800;
    line-height: 1.2;
    color: #475467;
}

.overall-tpt-legend-threshold {
    display: inline-flex;
    align-items: center;
    white-space: nowrap;
}

.overall-tpt-legend-item {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    white-space: nowrap;
}

.overall-tpt-legend-dot {
    width: 8px;
    height: 8px;
    border-radius: 999px;
    border: 1px solid rgba(15, 23, 42, 0.25);
    flex: 0 0 auto;
}

.overall-tpt-legend-dot-good {
    background: #067647;
}

.overall-tpt-legend-dot-bad {
    background: #b42318;
}

.overall-heatmap-wrap {
    overflow: hidden;
    max-height: 226px;
    background: #ffffff;
    border-radius: 10px;
    padding: 6px;
    position: relative;
}

.overall-heatmap-board {
    display: grid;
    gap: 6px;
    align-content: start;
    min-width: max-content;
    grid-auto-rows: 46px;
    background: #ffffff;
}

.overall-heatmap-corner,
.overall-heatmap-platform,
.overall-heatmap-ingredient,
.overall-heatmap-tile {
    border-radius: 9px;
    border: 1px solid #dbe3ee;
    box-sizing: border-box;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 10px;
    font-weight: 900;
}

.overall-heatmap-corner {
    background: #f2f4f7;
    color: #344054;
    border: none;
    font-size: 9px;
    letter-spacing: 0.02em;
}

.overall-heatmap-platform {
    background: #f2f4f7;
    color: #344054;
    border: none;
    font-size: 9px;
    letter-spacing: 0.02em;
}

.overall-heatmap-ingredient {
    justify-content: flex-start;
    padding-left: 8px;
    background: #f2f4f7;
    color: #344054;
    border: none;
    font-size: 9px;
}

.overall-heatmap-tile {
    width: 46px;
    height: 46px;
    font-size: 11px;
    border: none;
    cursor: help;
}

.overall-heatmap-tile.overall-heatmap-clickable {
    cursor: pointer;
    border: 1px solid rgba(15, 23, 42, 0.12);
    transition: border-color 0.12s ease, box-shadow 0.12s ease;
}

.overall-heatmap-tile.overall-heatmap-clickable:hover,
.overall-heatmap-tile.overall-heatmap-clickable:focus {
    border-color: rgba(30, 64, 175, 0.72);
    box-shadow: 0 0 0 2px rgba(30, 64, 175, 0.14);
    outline: none;
}

.overall-heatmap-tile.overall-heat-low { background: #bbf7d0; color: #000000; }
.overall-heatmap-tile.overall-heat-med { background: #fde68a; color: #000000; }
.overall-heatmap-tile.overall-heat-high { background: #fdba74; color: #000000; }
.overall-heatmap-tile.overall-heat-critical { background: #fca5a5; color: #000000; }

.component-summary-heatmap-board {
    grid-auto-rows: 44px;
}

.component-summary-heatmap-board .overall-heatmap-corner,
.component-summary-heatmap-board .overall-heatmap-platform,
.component-summary-heatmap-board .overall-heatmap-ingredient,
.component-summary-heatmap-board .overall-heatmap-tile {
    min-height: 44px;
}

.component-summary-heatmap-board .overall-heatmap-platform {
    width: 100%;
    min-width: 0;
    padding: 0 8px;
    white-space: nowrap;
}

.component-summary-heatmap-board .overall-heatmap-tile {
    width: 100%;
    min-width: 0;
    height: 44px;
    font-size: 11px;
    font-variant-numeric: tabular-nums;
}

.component-summary-heatmap-board .overall-heatmap-corner,
.component-summary-heatmap-board .overall-heatmap-ingredient {
    position: sticky;
    left: 0;
    z-index: 2;
    background: #f2f4f7;
}

.component-summary-heatmap-board .overall-heatmap-corner {
    z-index: 3;
}

.component-summary-heatmap-board .overall-heatmap-ingredient.ccip-component-heat-total-label {
    font-weight: 900;
    color: #101828;
}

.component-summary-heatmap-board .overall-heatmap-tile.ccip-component-heat-total-tile {
    background: #e5e7eb;
    color: #111827;
    box-shadow: inset 0 0 0 1px rgba(15, 23, 42, 0.25);
}

.component-summary-legend {
    margin-top: 6px;
    padding: 0 4px 0 0;
    border: none;
    border-radius: 0;
    background: transparent;
    display: flex;
    align-items: center;
    gap: 6px;
    flex-wrap: nowrap;
    white-space: nowrap;
    font-size: 9px;
    color: #344054;
    overflow-x: auto;
    overflow-y: hidden;
}

.component-summary-legend-title {
    font-weight: 800;
    color: #1f2937;
}

.component-summary-legend-row {
    display: inline-flex;
    align-items: center;
    gap: 2px;
    line-height: 1.2;
}

.component-summary-legend-toggle {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    margin: 0;
    color: #1f2937;
    font-weight: 600;
    cursor: pointer;
    white-space: nowrap;
    line-height: 1.2;
}

.component-summary-legend-toggle-input {
    width: 11px;
    height: 11px;
    margin: 0;
    cursor: pointer;
    accent-color: #2563eb;
}

.component-summary-legend-row-toggle {
    min-height: 11px;
    height: auto;
    margin-left: -1px;
    padding-left: 4px;
    border-left: 1px solid #d0d5dd;
}

.component-summary-legend-label {
    font-weight: 600;
}

.component-summary-legend-value {
    min-width: 22px;
    height: 15px;
    padding: 0 3px;
    border: 1px solid #d0d5dd;
    border-radius: 0;
    background: #ffffff;
    color: #111827;
    font-size: 9px;
    line-height: 1;
    display: inline-flex;
    align-items: center;
    justify-content: center;
}

.component-summary-legend-derived {
    color: #111827;
    font-size: 9px;
    line-height: 1;
    font-weight: 600;
}

.component-summary-legend-range-row {
    gap: 2px;
}

.component-summary-legend-sep {
    font-weight: 700;
    color: #475467;
}

.component-summary-legend-input {
    width: 22px;
    min-width: 22px;
    height: 15px;
    padding: 0 2px;
    border: 1px solid #d0d5dd;
    border-radius: 0;
    background: #ffffff;
    color: #111827;
    font-size: 9px;
    line-height: 1;
    text-align: center;
}

.component-summary-legend-input:focus {
    outline: 1px solid #9ec5ff;
    border-color: #6ea8fe;
}

.component-summary-legend-input::-webkit-outer-spin-button,
.component-summary-legend-input::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}

.component-summary-legend-input[type=number] {
    -moz-appearance: textfield;
    appearance: textfield;
}

.component-summary-legend-input.component-summary-legend-input-invalid {
    border-color: #d92d20 !important;
    box-shadow: 0 0 0 1px rgba(217, 45, 32, 0.2) !important;
}

.component-summary-warning-popup {
    position: absolute;
    left: 50%;
    bottom: 8px;
    transform: translate(-50%, 8px);
    opacity: 0;
    pointer-events: none;
    z-index: 3;
    max-width: calc(100% - 16px);
    padding: 6px 10px;
    border-radius: 8px;
    background: #b42318;
    color: #ffffff;
    font-size: 11px;
    font-weight: 700;
    line-height: 1.25;
    box-shadow: 0 6px 16px rgba(16, 24, 40, 0.2);
    white-space: nowrap;
    transition: opacity 0.18s ease, transform 0.18s ease;
}

.component-summary-warning-popup.component-summary-warning-popup-visible {
    opacity: 1;
    transform: translate(-50%, 0);
}

/* Keep editable threshold inputs visually identical to value boxes. */
.component-summary-legend-input.component-summary-legend-value {
    padding: 0 3px;
    border: 1px solid #d0d5dd;
    border-radius: 0;
    background: #ffffff;
    -webkit-appearance: none;
    appearance: none;
}

#homeComponentSummaryTable .component-summary-legend .component-summary-legend-input,
#homeComponentSummaryTable .component-summary-legend .component-summary-legend-input.component-summary-legend-value,
#homeOverallHeatmap .component-summary-legend .component-summary-legend-input,
#homeOverallHeatmap .component-summary-legend .component-summary-legend-input.component-summary-legend-value {
    border: 1px solid #d0d5dd !important;
    border-radius: 0 !important;
    box-shadow: none !important;
}

.component-summary-legend-swatch {
    width: 10px;
    height: 10px;
    border-radius: 3px;
    border: 1px solid rgba(15, 23, 42, 0.16);
    flex: 0 0 auto;
}

.component-summary-legend-swatch.low { background: #bbf7d0; }
.component-summary-legend-swatch.med { background: #fde68a; }
.component-summary-legend-swatch.high { background: #fdba74; }
.component-summary-legend-swatch.critical { background: #fca5a5; }

#homeComponentSummaryTable.ccip-component-riskmap-wrap {
    overflow: hidden;
    position: relative;
}

.component-summary-heatmap-layout {
    display: grid;
    grid-template-rows: minmax(0, 1fr) auto;
    gap: 8px;
    height: 100%;
    min-height: 0;
}

.component-summary-heatmap-scroll {
    overflow-x: auto;
    overflow-y: auto;
    min-height: 0;
}

.overall-pending-body {
    display: grid;
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
    gap: 8px;
    min-height: 0;
    height: 100%;
    box-sizing: border-box;
    padding-bottom: 0;
}

.overall-pending-top {
    display: grid;
    grid-template-columns: minmax(96px, 40%) minmax(0, 1fr);
    align-items: start;
    gap: 6px;
    min-width: 0;
    max-width: 100%;
}

.overall-pending-bottom {
    display: grid;
    gap: 4px;
    min-width: 0;
}

.overall-pending-pie {
    width: 100%;
    max-width: 132px;
    height: auto;
    aspect-ratio: 1 / 1;
    min-width: 0;
    justify-self: center;
}

#homeMilestoneSummaryTable.ccip-milestone-pie-wrap {
    overflow: hidden;
    max-height: none;
    height: 100%;
    min-height: 0;
    display: flex;
    flex-direction: column;
}

.milestone-pie-layout {
    display: grid;
    grid-template-rows: auto auto minmax(0, 1fr);
    gap: 8px;
    min-height: 0;
    height: 100%;
    box-sizing: border-box;
    padding-bottom: 0;
}

.milestone-stacked-total {
    font-size: 11px;
    font-weight: 800;
    color: #344054;
    text-align: left;
}

.milestone-pie-canvas {
    width: 100%;
    max-width: 100%;
    height: 72px;
    min-height: 72px;
    min-width: 0;
    justify-self: stretch;
}

.milestone-pie-breakdown {
    border: 1px solid #d8e3f5;
    border-radius: 10px;
    background: #f8fbff;
    margin-top: 8px;
    padding: 8px;
    overflow-y: auto;
    overflow-x: hidden;
    align-content: start;
    min-width: 0;
    max-width: 100%;
    display: grid;
    gap: 5px;
}

.overall-pending-breakdown,
.overall-age-buckets {
    display: grid;
    gap: 5px;
}

.overall-pending-breakdown {
    border: 1px solid #d8e3f5;
    border-radius: 10px;
    background: #f8fbff;
    padding: 8px;
    height: 132px;
    max-height: 132px;
    overflow-y: auto;
    overflow-x: hidden;
    align-content: start;
    min-width: 0;
    max-width: 100%;
}

.overall-age-buckets {
    border: 1px solid #d8e3f5;
    border-radius: 10px;
    background: #ffffff;
    padding: 8px;
    align-content: start;
    max-height: none;
    overflow-y: visible;
    min-height: 0;
}

.overall-break-row,
.overall-age-row {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 6px;
    width: 100%;
    font-size: 11px;
    color: #344054;
}

.overall-break-left {
    display: inline-flex;
    align-items: flex-start;
    gap: 6px;
    min-width: 0;
    flex: 1 1 auto;
}

.overall-break-name {
    display: block;
    overflow: visible;
    text-overflow: clip;
    white-space: normal;
    overflow-wrap: anywhere;
    word-break: break-word;
    line-height: 1.3;
}

.overall-break-dot {
    width: 9px;
    height: 9px;
    border-radius: 999px;
    border: 1px solid rgba(15, 23, 42, 0.12);
    flex: 0 0 auto;
}

.overall-break-metrics {
    display: inline-flex;
    align-items: baseline;
    gap: 5px;
    white-space: nowrap;
    justify-content: flex-end;
    flex: 0 0 auto;
}

.overall-age-row {
    display: grid;
    grid-template-columns: minmax(0, 1fr) 86px minmax(0, 1fr);
    align-items: center;
    gap: 10px;
}

.overall-age-label {
    display: inline-grid;
    grid-template-columns: 38px auto;
    column-gap: 2px;
    align-items: center;
    min-width: 0;
    white-space: nowrap;
    justify-self: start;
    text-align: left;
}

.overall-age-label-main {
    text-align: left;
    font-variant-numeric: tabular-nums;
}

.overall-age-label-unit {
    text-align: left;
}

.overall-age-bar-track {
    width: 86px;
    height: 6px;
    border-radius: 999px;
    background: #e3ebf7;
    overflow: hidden;
    box-shadow: inset 0 0 0 1px #d7e2f2;
    justify-self: center;
}

.overall-age-bar-fill {
    display: block;
    height: 100%;
    border-radius: 999px;
    min-width: 2px;
    background: linear-gradient(90deg, #93c5fd 0%, #2563eb 100%);
}

.overall-age-metrics {
    white-space: nowrap;
    text-align: right;
    color: #1f2937;
    font-weight: 800;
    justify-self: end;
    min-width: max-content;
}

.overall-break-count {
    color: #111827;
    font-weight: 900;
}

.overall-break-count-link {
    color: #1d4ed8;
    text-decoration: none;
    font-weight: 800;
}

.overall-break-name-link {
    color: #1d4ed8;
    text-decoration: none;
    font-weight: 800;
}

.overall-break-count-link:hover,
.overall-break-count-link:focus,
.overall-break-name-link:hover,
.overall-break-name-link:focus {
    color: #1e40af;
    text-decoration: underline;
}

.ccip-component-summary-link {
    color: #1d4ed8;
    text-decoration: none;
    font-weight: 800;
}

.ccip-component-summary-link:hover,
.ccip-component-summary-link:focus {
    color: #1e40af;
    text-decoration: underline;
}

.ccip-pending-summary-count-link {
    color: #1d4ed8;
    font-weight: 800;
    text-decoration: none;
}

.ccip-pending-summary-count-link:hover,
.ccip-pending-summary-count-link:focus {
    color: #1e40af;
    text-decoration: underline;
}

.overall-break-pct {
    color: #667085;
    font-weight: 700;
}

.overall-age-title {
    margin-top: 0;
    margin-bottom: 4px;
    color: #334155;
    font-size: 11px;
    font-weight: 900;
    text-transform: uppercase;
}

.ccip-component-spaces {
    margin-top: 12px;
    border: 1px solid #d0ddf3;
    border-radius: 14px;
    background: linear-gradient(180deg, #f9fcff 0%, #f4f8ff 100%);
    padding: 14px;
}

.ccip-component-spaces-head {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 10px;
    margin-bottom: 12px;
    flex-wrap: wrap;
}

.ccip-component-spaces-title {
    margin: 0;
    color: #0f2f62;
    font-size: 16px;
    font-weight: 900;
    letter-spacing: 0.01em;
}

.ccip-component-spaces-note {
    margin: 2px 0 0;
    color: #52627a;
    font-size: 12px;
    font-weight: 700;
}

.ccip-space-grid {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 12px;
}

.ccip-space-card {
    border: 1px solid #d8e3f5;
    border-radius: 12px;
    background: #ffffff;
    box-shadow: 0 6px 16px rgba(15, 23, 42, 0.06);
    padding: 12px;
    min-height: 180px;
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.ccip-space-card.space-performance {
    grid-column: span 2;
}

.ccip-space-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
}

.ccip-space-title {
    margin: 0;
    color: #1f2f4f;
    font-size: 13px;
    font-weight: 900;
    letter-spacing: 0.01em;
}

.ccip-space-subtitle {
    margin: 2px 0 0;
    color: #667085;
    font-size: 11px;
    font-weight: 700;
}

.ccip-perf-tiles {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 8px;
}

.ccip-perf-tile {
    border: 1px solid #dbe4f2;
    border-radius: 10px;
    padding: 10px;
    background: #fbfdff;
}

.ccip-perf-label {
    color: #56637a;
    font-size: 11px;
    font-weight: 800;
}

.ccip-perf-value {
    margin-top: 5px;
    color: #10243f;
    font-size: 30px;
    line-height: 1;
    font-weight: 900;
}

.ccip-perf-note {
    margin-top: 4px;
    color: #7b8798;
    font-size: 10px;
    font-weight: 700;
}

.ccip-space-list {
    margin: 0;
    padding: 0;
    list-style: none;
    display: grid;
    gap: 8px;
}

.ccip-space-list li {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 8px;
    align-items: center;
    border: 1px solid #ecf0f7;
    border-radius: 8px;
    padding: 7px 9px;
    background: #ffffff;
}

.ccip-space-item-name {
    color: #334155;
    font-size: 12px;
    font-weight: 700;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.ccip-space-item-value {
    color: #0f4ea8;
    font-size: 12px;
    font-weight: 900;
    white-space: nowrap;
}

.ccip-space-empty {
    border: 1px dashed #c8d4e6;
    border-radius: 8px;
    background: #fbfdff;
    color: #6b7280;
    font-size: 12px;
    font-weight: 700;
    padding: 12px;
}

.ccip-activity-list,
.ccip-insights-list {
    margin: 0;
    padding: 0;
    list-style: none;
    display: grid;
    gap: 8px;
}

.ccip-activity-list li,
.ccip-insights-list li {
    font-size: 12px;
    color: #344054;
    line-height: 1.4;
    display: flex;
    gap: 8px;
    align-items: flex-start;
}

.ccip-time {
    color: #98a2b3;
    font-weight: 700;
    min-width: 40px;
}

.ccip-insights-list li:before {
    content: '\\f201';
    font-family: 'Font Awesome 6 Free';
    font-weight: 900;
    color: #6366f1;
    font-size: 11px;
    margin-top: 1px;
}

@media (max-width: 1500px) {
    .ccip-main-grid {
        grid-template-columns: minmax(0, 1.35fr) minmax(280px, 0.9fr);
    }

    .ccip-dashboard-overview {
        grid-template-columns: 1fr;
    }

    .ccip-main-grid .ccip-col-right {
        grid-column: auto;
        display: flex;
        flex-direction: column;
    }

    .overall-layout-v2 {
        grid-template-columns: 1fr;
        grid-template-rows: auto;
    }

    .overall-row-summary,
    .overall-row-second,
    .overall-row-third,
    .overall-ai-span-card {
        grid-column: auto;
        grid-row: auto;
    }

    .overall-row-summary {
        grid-template-columns: 1fr;
    }

    .overall-summary-facts .overall-summary-fact {
        flex: 1 1 0;
        width: auto;
        min-width: 0;
        max-width: none;
        height: 100%;
        min-height: 100%;
        max-height: 100%;
    }

    .overall-row-third {
        grid-template-columns: minmax(0, 1.1fr) minmax(220px, 0.65fr);
    }

    .overall-trend-card {
        grid-column: 1 / -1;
    }

    .ccip-space-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .ccip-space-card.space-performance {
        grid-column: span 2;
    }
}

@media (max-width: 1180px) {
    .ccip-dashboard-overview,
    .ccip-dashboard-tables-grid {
        grid-template-columns: 1fr;
    }

    .ccip-dashboard-tables-grid .ccip-col-center {
        display: flex;
    }

    .ccip-ai-health-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .ccip-health-score,
    .ccip-health-risk-compact,
    .ccip-health-weekly,
    .ccip-health-blockers,
    .ccip-ai-health-grid .ccip-ai-health-summary {
        grid-column: auto;
        grid-row: auto;
    }

    .ccip-dashboard-overview {
        min-height: 0;
    }
}

@media (max-width: 900px) {
    .ccip-main-grid {
        grid-template-columns: 1fr;
    }

    #homeLegacyTablesGrid .ccip-inline-side-stack {
        grid-template-columns: 1fr;
        grid-template-rows: auto;
    }

    .ccip-main-grid .ccip-col-right {
        grid-template-columns: 1fr;
    }

    .overall-layout-v2 {
        grid-template-columns: 1fr;
        grid-template-rows: auto;
    }

    .overall-row-summary,
    .overall-row-second,
    .overall-row-third,
    .overall-ai-span-card {
        grid-column: auto;
        grid-row: auto;
    }

    .overall-summary-facts .overall-summary-fact {
        flex: 1 1 0;
        width: auto;
        min-width: 0;
        max-width: none;
        height: 100%;
        min-height: 100%;
        max-height: 100%;
    }

    .overall-row-summary,
    .overall-row-second,
    .overall-row-third {
        grid-template-columns: 1fr;
    }

    .overall-pending-top {
        grid-template-columns: 1fr;
    }

    .overall-pending-pie {
        width: 100%;
        max-width: none;
        height: 170px;
        aspect-ratio: auto;
        justify-self: stretch;
    }

    .ccip-space-grid {
        grid-template-columns: 1fr;
    }

    .ccip-space-card.space-performance {
        grid-column: auto;
    }

    .ccip-perf-tiles {
        grid-template-columns: 1fr;
    }
}

@media (max-width: 740px) {
    .ccip-dash-greeting {
        font-size: 26px;
    }

    .ccip-product-wrap,
    .ccip-region-wrap {
        grid-template-columns: 1fr;
    }
}

/* Fixed geometry mode: keep positions/sizes rigid and scroll the whole page */
html,
body {
    overflow-x: auto !important;
    overflow-y: auto !important;
}

form,
.content-wrapper,
.main-content,
.portal-content,
.page-content {
    min-width: 1880px;
}

.overall-layout-v2 {
    width: 1840px;
    min-width: 1840px;
    max-width: 1840px;
    display: grid;
    grid-template-columns: var(--cmf-summary-side-card-width) var(--cmf-summary-main-width) 700px;
    grid-template-rows: auto auto auto;
    gap: 10px;
}

.overall-row {
    width: 100%;
    min-width: 0;
    box-sizing: border-box;
}

.overall-row-second {
    grid-template-columns: 660px minmax(0, 1fr) !important;
    width: var(--cmf-summary-main-width);
    min-width: var(--cmf-summary-main-width);
    max-width: var(--cmf-summary-main-width);
    justify-self: start;
}

.overall-row-summary {
    grid-template-columns: 840px 390px !important;
    width: var(--cmf-summary-main-width);
    min-width: var(--cmf-summary-main-width);
    max-width: var(--cmf-summary-main-width);
    justify-self: start;
}

#homeOverallLayoutV2 .overall-row-summary {
    grid-template-columns: 1fr !important;
}

#homeOverallLayoutV2 > .overall-row-summary {
    display: flex !important;
    flex-direction: row;
    align-items: stretch;
    gap: 10px;
}

#homeOverallLayoutV2 > .overall-row-summary > .overall-summary-side-card {
    display: none !important;
}

#homeOverallLayoutV2 > .overall-summary-side-card.overall-summary-side-span {
    flex: 0 0 var(--cmf-summary-side-card-width) !important;
    width: var(--cmf-summary-side-card-width) !important;
    min-width: var(--cmf-summary-side-card-width) !important;
    max-width: var(--cmf-summary-side-card-width) !important;
    min-height: 0;
    display: flex;
    flex-direction: column;
    align-self: stretch;
    grid-column: 1 !important;
    grid-row: 1 / span 2 !important;
}

#homeOverallLayoutV2 > .overall-row-summary > .overall-summary-card {
    flex: 1 1 auto;
    width: auto !important;
    min-width: 0;
}

#homeOverallLayoutV2 > .overall-row-summary .overall-summary-side-card .overall-card-head {
    margin-bottom: 0;
}

#homeOverallLayoutV2 > .overall-summary-side-card.overall-summary-side-span .overall-card-head {
    margin-bottom: 0;
}

#homeOverallLayoutV2 > .overall-row-summary .overall-summary-side-body {
    flex: 1 1 auto;
    min-height: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    padding-bottom: 0;
    color: #667085;
    font-size: 12px;
    font-weight: 700;
}

#homeOverallLayoutV2 > .overall-summary-side-card.overall-summary-side-span .overall-summary-side-body {
    flex: 1 1 auto;
    min-height: 0;
    display: flex;
    align-items: flex-start;
    justify-content: center;
    padding-bottom: 0;
    color: #667085;
    font-size: 12px;
    font-weight: 700;
}

#homeOverallLayoutV2 #homeOverallSummaryFacts.overall-summary-facts {
    display: flex !important;
    flex-wrap: nowrap;
    gap: 6px;
    justify-content: center;
    align-items: stretch;
    overflow-x: hidden;
    overflow-y: hidden;
}

#homeLegacyTopLayout #homeSpecificSummaryFactsDashboard.overall-summary-facts,
#homeLegacyTopLayout #homeSpecificSummaryFactsCmf.overall-summary-facts,
#homeCmfSummaryPanel #homeSpecificSummaryFactsCmf.overall-summary-facts {
    display: flex !important;
    flex-wrap: nowrap;
    gap: 6px;
    justify-content: center;
    align-items: stretch;
    overflow-x: hidden;
    overflow-y: hidden;
}

.overall-row-summary > .overall-card {
    width: 100%;
    min-width: 0;
    box-sizing: border-box;
}

.overall-row-second > .overall-card {
    width: 100%;
    min-width: 0;
    box-sizing: border-box;
}

.overall-row-third {
    grid-template-columns: minmax(0, 1fr) 810px !important;
    padding-left: 10px;
    padding-right: 10px;
}

.overall-row-third > .overall-card {
    grid-column: auto !important;
}

.overall-layout-v2 > .overall-row-summary {
    grid-column: 2 !important;
    grid-row: 1 !important;
}

.overall-layout-v2 > .overall-row-second {
    grid-column: 2 !important;
    grid-row: 2 !important;
}

.overall-layout-v2 > .overall-ai-span-card {
    grid-column: 3 !important;
    grid-row: 1 / span 2 !important;
    min-height: 0 !important;
    max-height: none !important;
}

.overall-layout-v2 > .overall-row-third {
    grid-column: 1 / -1 !important;
    grid-row: 3 !important;
}

.overall-trend-card { order: 1; }
.overall-heatmap-card { order: 2; }
.overall-pending-card { order: 3; }

.overall-heatmap-card {
    display: flex;
    flex-direction: column;
}

.overall-heatmap-card .overall-card-head {
    flex: 0 0 auto;
}

.overall-heatmap-card .overall-heatmap-wrap {
    flex: 1 1 auto;
    min-height: 0;
    max-height: none;
}

.overall-pending-card {
    display: flex;
    flex-direction: column;
    width: 100%;
    max-width: 100%;
    min-width: 0;
    box-sizing: border-box;
    overflow: hidden;
}

.overall-pending-card .overall-card-head {
    flex: 0 0 auto;
}

.overall-pending-card .overall-pending-body {
    flex: 1 1 auto;
}

.overall-summary-card,
.overall-tpt-card {
    min-height: 0;
    max-height: none;
    height: auto;
}

.overall-oem-card,
.overall-ai-card {
    min-height: 312px;
    max-height: 312px;
}

.overall-ai-span-card.overall-ai-card {
    min-height: 0 !important;
    max-height: none !important;
}

.overall-oem-card,
.overall-ai-card {
    display: flex;
    flex-direction: column;
}

.overall-summary-card .overall-card-head,
.overall-tpt-card .overall-card-head,
.overall-oem-card .overall-card-head,
.overall-ai-card .overall-card-head {
    flex: 0 0 auto;
}

.overall-summary-card .overall-summary-facts,
.overall-tpt-card .overall-tpt-boxes,
.overall-oem-card .overall-oem-table-wrap,
.overall-ai-card .overall-ai-blank {
    flex: 1 1 auto;
    min-height: 0;
}

.overall-ai-span-card .overall-ai-blank {
    flex: 1 1 auto;
    min-height: 0;
    height: 100%;
}

.overall-trend-card,
.overall-heatmap-card,
.overall-pending-card {
    min-height: 312px;
    max-height: 312px;
}

.overall-tpt-card {
    width: 390px;
    min-width: 390px;
    max-width: 390px;
    aspect-ratio: auto;
}

.overall-trend-chart {
    height: 249px;
}

/* Match platform-specific issue trend card geometry to overall trend card. */
#homeLegacyOverview .ccip-trend-card-inline.overall-trend-card {
    width: 660px;
    min-width: 660px;
    max-width: 660px;
    min-height: 312px;
    max-height: 312px;
    justify-self: start;
}

.overall-oem-table-wrap {
    max-height: none;
}

.overall-ai-blank {
    min-height: 0;
    height: 100%;
}

/* Issue List + CMF Pending interactive modernization */
.issue-tab-shell,
.pending-tab-shell {
    width: 100% !important;
    max-width: none !important;
}

#issueTabShell[style*="display:none"],
#issueTabShell[style*="display: none"],
#pendingTabShell[style*="display:none"],
#pendingTabShell[style*="display: none"] {
    display: none !important;
}

.issue-tab-main,
.pending-tab-main {
    min-width: 0;
    width: 100% !important;
    max-width: none !important;
}

.issue-tab-shell.side-hidden,
.pending-tab-shell.side-hidden {
    grid-template-columns: minmax(0, 1fr);
}

.issue-side-panel,
.pending-side-panel {
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    box-shadow: 0 10px 24px rgba(16, 24, 40, 0.06);
    padding: 12px;
    display: grid;
    gap: 10px;
    position: sticky;
    top: 92px;
}

.interactive-side-card {
    border: 1px solid #e9edf3;
    border-radius: 12px;
    background: linear-gradient(180deg, #ffffff 0%, #fafcff 100%);
    padding: 10px;
}

.interactive-side-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    margin-bottom: 8px;
}

.interactive-side-title {
    margin: 0;
    color: #0f172a;
    font-size: 14px;
    font-weight: 800;
}

.interactive-side-sub {
    margin: 0;
    color: #64748b;
    font-size: 11px;
}

.interactive-side-btn {
    width: 100%;
    min-height: 34px;
    border-radius: 10px;
    border: 1px solid #c8d8f7;
    background: #f4f8ff;
    color: #2452b8;
    font-size: 12px;
    font-weight: 700;
    cursor: pointer;
}

.interactive-side-btn:hover {
    background: #ecf3ff;
}

.interactive-chip-list {
    list-style: none;
    margin: 0;
    padding: 0;
    display: grid;
    gap: 8px;
}

.interactive-chip-list li {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    font-size: 12px;
    color: #334155;
}

.interactive-chip {
    display: inline-flex;
    align-items: center;
    min-height: 24px;
    padding: 0 10px;
    border-radius: 999px;
    border: 1px solid #dbe7f4;
    background: #f8fbff;
    color: #1e3a8a;
    font-size: 11px;
    font-weight: 800;
}

.interactive-shortcuts {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 8px;
}

.interactive-shortcut-btn {
    min-height: 36px;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    background: #ffffff;
    color: #334155;
    font-size: 11px;
    font-weight: 700;
    cursor: pointer;
}

.interactive-shortcut-btn:hover {
    background: #f8fbff;
}

.issue-page-hd,
.cmf-pending-page-hd {
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    box-shadow: 0 8px 18px rgba(16, 24, 40, 0.05);
    padding: 12px 14px;
    margin-bottom: 10px;
}

.issue-kpi-row,
.cmf-pending-kpi-row {
    gap: 10px;
    margin-bottom: 12px;
}

.issue-kpi,
.cmf-pending-kpi {
    border-radius: 12px;
    box-shadow: 0 8px 18px rgba(16, 24, 40, 0.05);
}

.issue-top-filter-panel,
#sharedFilterPanel.overview-platform-actions {
    border-radius: 12px;
    box-shadow: 0 8px 18px rgba(16, 24, 40, 0.04);
}

.issue-grid-scroll,
.cmf-pending-grid-wrap {
    border: 1px solid #e5e7eb;
    border-radius: 14px;
    padding: 8px;
    background: #ffffff;
    box-shadow: 0 10px 24px rgba(16, 24, 40, 0.05);
    width: 100% !important;
    max-width: none !important;
}

.issue-grid-inner #overall_request_details {
    border-radius: 12px !important;
    overflow: hidden !important;
}

.issue-grid-inner #overall_request_details th,
.cmf-pending-grid-wrap #GridView_cmf_pending th {
    position: sticky;
    top: 0;
    z-index: 2;
}

.issue-grid-inner #overall_request_details td,
.cmf-pending-grid-wrap #GridView_cmf_pending td {
    transition: background-color 0.18s ease, transform 0.18s ease;
}

.issue-grid-inner #overall_request_details tr:hover td,
.cmf-pending-grid-wrap #GridView_cmf_pending tr:hover td {
    background: #f8fbff !important;
}

.issue-pager-controls {
    margin-top: 12px;
}

@media (max-width: 1300px) {
    .issue-tab-shell,
    .pending-tab-shell {
        grid-template-columns: minmax(0, 1fr) 270px;
    }
}

@media (max-width: 760px) {
    .issue-tab-shell,
    .pending-tab-shell {
        grid-template-columns: 1fr;
    }

    .issue-side-panel,
    .pending-side-panel {
        position: static;
    }
}

        /* Footer */
        footer {
            background-color: #2D3748;
            color: white;
            padding-top: 2px;
            font-size: 10px;
            position: fixed;
            text-align: center;
            width: 100%;
            height: 3vh;
            box-shadow: 0 -2px 4px rgba(0, 0, 0, 0.1);
            z-index: 10;
        }

        /* Modal */
        .modal .modal-body {
            max-height: 80%;
            overflow-y: auto;
        }

        .searchable-dropdown {
            width: 200px;
            font-family: Arial, sans-serif;
        }

        /* Modern Dropdown */
        .dropdown {
            font-size: 12px;
            padding: 10px 14px;
            border-radius: 999px;
            border: 1px solid #8fb7db;
            background-color: rgba(255, 255, 255, 0.98);
            color: var(--portal-ink);
            width: auto;
            min-width: 110px;
            max-height: 42px;
            transition: all 0.3s ease;
            margin-left: 0;
            margin-top: 0;
            margin-bottom: 0;
            box-shadow: 0 8px 18px rgba(15, 94, 168, 0.10);
        }

            .dropdown:focus {
                outline: none;
                border-color: #005d99; /* Darker shade of blue */
                box-shadow: 0 0 8px rgba(0, 93, 153, 0.5);
            }

        /* Modern Buttons */
        .modern-button {
            background: linear-gradient(180deg, #1672be 0%, #0f5ea8 100%);
            color: white;
            border: 1px solid rgba(8, 61, 116, 0.15);
            padding: 9px 16px;
            font-size: 11px;
            border-radius: 999px;
            cursor: pointer;
            margin: 0;
            transition: all 0.3s ease;
            box-shadow: 0 10px 18px rgba(15, 94, 168, 0.18);
            font-weight: 600;
            letter-spacing: 0.02em;
        }

            .modern-button:hover {
                background: linear-gradient(180deg, #0f5ea8 0%, #0b4c87 100%);
                transform: translateY(-1px);
            }

            .modern-button:focus {
                outline: none;
                box-shadow: 0 0 8px rgba(0, 93, 153, 0.6);
            }

        .modern-button1 {
            background-color: #1d4f91; /* Intel's blue */
            color: white;
            border: none;
            padding: 8px 16px; /* Smaller padding */
            font-size: 10px; /* Smaller font */
            border-radius: 6px; /* Slightly smaller rounded corners */
            cursor: pointer;
            margin: 0 8px; /* Horizontal space between buttons */
            transition: all 0.3s ease;
        }

             .modern-button1:hover {
                 background-color: #1172b3; /* Darker Intel Blue on hover */
             }

             .modern-button1:focus {
                 outline: none;
                 box-shadow: 0 0 8px rgba(0, 93, 153, 0.6);
             }

             .modern-title-container {
                 display: flex;
                 justify-content: space-between;
                 align-items: center;
                 width: 100%;
             }

             .export-btn {
                 margin-left: auto;
             }

             .modal-header {
                 position: relative;
             }

#btnExportDesignToExcel {
    position: absolute;
    top: 10px;
    right: 50px;
    z-index: 1050;
}

#btnExportOEMToExcel {
    position: absolute;
    top: 10px;
    right: 50px;
    z-index: 1050;
}

#btnExportIngredientToExcel {
    position: absolute;
    top: 10px;
    right: 50px;
    z-index: 1050;
}

#btnExportDesignDetailsModal0 {
    position: absolute;
    top: 10px;
    right: 50px;
    z-index: 1050;
    background-color: #138a3d !important;
    color: #ffffff !important;
    border: none !important;
}

#btnExportDesignDetailsModal0:hover {
    background-color: #0e6f31 !important;
}

/* Unified popup export button layout + style */
.modal-title-container {
    display: flex;
    align-items: center;
    gap: 10px;
    width: 100%;
    min-width: 0;
}

.modal-title-container .modal-title {
    margin: 0;
    flex: 1 1 auto;
    min-width: 0;
}

#detailsModal .modal-header .export-btn,
#detailsModal0 .modal-header .export-btn,
#detailsModal2 .modal-header .export-btn,
#detailsModal3 .modal-header .export-btn,
#detailsModalDrivers .modal-header .export-btn,
#detailsModalDrivers1 .modal-header .export-btn {
    position: static !important;
    top: auto !important;
    right: auto !important;
    z-index: auto !important;
    margin-left: auto !important;
    margin-right: 8px !important;
    background: #16a34a !important;
    border: 1px solid #16a34a !important;
    color: #ffffff !important;
    box-shadow: 0 2px 8px rgba(22, 163, 74, 0.26) !important;
}

#detailsModal .modal-header .export-btn:hover,
#detailsModal0 .modal-header .export-btn:hover,
#detailsModal2 .modal-header .export-btn:hover,
#detailsModal3 .modal-header .export-btn:hover,
#detailsModalDrivers .modal-header .export-btn:hover,
#detailsModalDrivers1 .modal-header .export-btn:hover {
    background: #15803d !important;
    border-color: #15803d !important;
}

#detailsModal .modal-header .export-btn:focus,
#detailsModal0 .modal-header .export-btn:focus,
#detailsModal2 .modal-header .export-btn:focus,
#detailsModal3 .modal-header .export-btn:focus,
#detailsModalDrivers .modal-header .export-btn:focus,
#detailsModalDrivers1 .modal-header .export-btn:focus {
    outline: none !important;
    box-shadow: 0 0 0 2px rgba(187, 247, 208, 0.95), 0 0 0 4px rgba(22, 163, 74, 0.35) !important;
}

            /* Modern Buttons */
.modern-button2 {
    background-color: green; /* Intel's blue */
    color: white;
    border: none;
    padding: 8px 16px; /* Smaller padding */
    font-size: 10px; /* Smaller font */
    border-radius: 6px; /* Slightly smaller rounded corners */
    cursor: pointer;
    margin: 0 8px; /* Horizontal space between buttons */
    transition: all 0.3s ease;
}

    .modern-button2:hover {
        background-color: greenyellow; /* Darker Intel Blue on hover */
    }

    .modern-button2:focus {
        outline: none;
        box-shadow: 0 0 8px rgba(0, 93, 153, 0.6);
    }

        /* Button container for layout */
        .button-container {
                    display: flex;
                    justify-content: center;
                    gap: 10px;
                    margin-top: 18px;
                    margin-bottom: 16px;
                    padding: 14px;
                    flex-wrap: wrap;
                    background: rgba(255, 255, 255, 0.68);
                    border: 1px solid rgba(205, 217, 229, 0.9);
                    border-radius: 18px;
                    box-shadow: 0 10px 28px rgba(21, 49, 79, 0.08);
        }

         /* Button container for layout */
.button-container2 {
    display: flex;
justify-content: center; /* Center the buttons horizontally */
gap: 10px; /* Space between buttons */
margin-top: -150px;

}

.link-container {
            display: flex;
            justify-content: flex-end;
            gap: 14px;
            padding: 12px 14px;
            margin-top: 16px;
            background: rgba(255, 255, 255, 0.68);
            border: 1px solid rgba(205, 217, 229, 0.9);
            border-radius: 18px;
            box-shadow: 0 10px 24px rgba(18, 42, 66, 0.08);
            flex-wrap: wrap;
        }

        /* Style for individual links */
        .link-container a {
            text-decoration: none;
            color: var(--portal-primary);
            font-weight: 700;
            padding: 8px 12px;
            border-radius: 999px;
            background: rgba(15, 94, 168, 0.08);
        }

        /* Hover effect for links */
        .link-container a:hover {
            color: var(--portal-primary-deep);
            background: rgba(15, 94, 168, 0.14);
        }

        /* Responsive design for smaller screens */
        @media screen and (max-width: 600px) {
            .dropdown {
                width: 100%;
            }

            .button-container {
                flex-wrap: wrap; /* Allow buttons to wrap in small screens */
                justify-content: center; /* Center buttons in smaller screen */
            }

             .button-container2 {
     flex-wrap: wrap; /* Allow buttons to wrap in small screens */
     justify-content: center; /* Center buttons in smaller screen */
 }

            .modern-button {
                margin: 5px; /* Adjust margin for smaller screen */
                font-size: 12px; /* Slightly smaller font size */
                padding: 6px 12px; /* Adjust padding for smaller screen */
            }
        }

        .metricsPanel {
            margin-top: 10px;
            display: flex;
            justify-content: center; /* Aligns the content horizontally */
            align-items: center; /* Optionally centers the content vertically */
        }

        .super-header {
            background-color: #f1f1f1;
            font-weight: bold;
            text-align: center;
            padding: 10px;
        }

        /*.main-header-cell {
            border: 2px solid black !important;
            padding: 10px !important;
            text-align: center !important;
            vertical-align: middle !important;
            font-weight: bold !important;
        }

        .table-primary th {
            background-color: #2a62c2;
            color: white;
            font-weight: 500;
            vertical-align: middle;
            text-align: center;
            line-height: 1.2;
            padding: 8px 4px;
            border: 2px solid black !important;
        }*/

        /* Make sure the sub-headers are properly styled */
        /*.table-primary thead tr:nth-child(2) th {
            background-color: #4a7bc8;*/ /* Slightly lighter blue for sub-headers */
            /*font-size: 0.9em;
        }

        .table-primary-header {
            border: 2px solid black !important;
            padding: 10px !important;
            text-align: center !important;
            vertical-align: middle !important;
            font-weight: bold !important;
            background-color: #2a62c2 !important;
            color: white !important;
        }*/
 .table-primary {
     height: 100%; /* Make the table fill the container's height */
     width: 100%; /* Ensure the table takes up the full width of the container */
     border-collapse: collapse; /* Ensures borders are merged between cells */
 }

     .table-primary th {
         color: white; /* White text color */
         font-weight: bold; /* Bold text for headers */
         padding: 10px; /* Padding for better readability */
         text-align: left; /* Align text to the left */
         border: 1px solid #ddd; /* Border around headers */
     }

     .table-primary td {
         padding: 8px; /* Padding for cells */
         border: 1px solid #ddd; /* Border around table cells */
     }

 td:nth-child(even), th:nth-child(even) {
     border-left: solid 2px black;
 }

 td:nth-child(odd), th:nth-child(odd) {
     border-left: solid 2px black;
 }

 /* First row within .table-primary */
 .table-primary tr:first-child {
     background-color: #ffebcc; /* Mild yellow background for the first row */
     border-bottom: solid 2px black;
 }

 /* Even rows within .table-primary */
 .table-primary tr:nth-child(even) {
     background-color: #c2e0f7; /* Mild blue background for even rows */
     border-bottom: solid 2px black;
 }

 /* Odd rows within .table-primary */
 .table-primary tr:nth-child(odd) {
     background-color: #e0f1fb; /* Very light blue background for odd rows */
     border-bottom: solid 2px black;
 }



.gridview-container3 {
    table-layout: fixed;
    display: inline-block;
    width: 100%;
    vertical-align: top;
    height: 400px;
    margin-bottom: 2vh;
}

.table-primary7 {
    height: 100%; /* Make the table fill the container's height */
    width: 100%; /* Ensure the table takes up the full width of the container */
    border-collapse: collapse; /* Ensures borders are merged between cells */
    /*table-layout: fixed;*/
}

.table-primary7 th {
    color: white; /* White text color */
    font-weight: bold; /* Bold text for headers */
    padding: 10px; /* Padding for better readability */
    text-align: left; /* Align text to the left */
    border: 1px solid #ddd; /* Border around headers */
    /*vertical-align: middle;*/
}

.table-primary7 td {
    padding: 8px; /* Padding for cells */
    border: 1px solid #ddd; /* Border around table cells */
    /*text-align: left;  Changed from center to left to match css1 */
}

/* Driver group borders - every 3 columns (matching the nth-child pattern from css1) */
.table-primary7 td:nth-child(3n+1), 
.table-primary7 th:nth-child(3n+1) {
    border-left: solid 2px black;
}

/* Apply the same border pattern as css1 for consistency */
.table-primary7 td:nth-child(even), 
.table-primary7 th:nth-child(even) {
    border-left: solid 2px black;
}

.table-primary7 td:nth-child(odd), 
.table-primary7 th:nth-child(odd) {
    border-left: solid 2px black;
}


/* First row within .table-primary7 - matching css1 */
.table-primary7 tr:first-child {
    background-color: #ffebcc; /* Mild yellow background for the first row */
    border-bottom: solid 2px black;
}

/* Second header row - slightly different yellow to distinguish */
.table-primary7 tr:nth-child(2) {
    background-color: #e0f1fb !important; /* Same mild yellow as first row */
    border-bottom: solid 2px black;
}

/* Even rows within .table-primary7 - starting from 3rd row (after headers) */
.table-primary7 tr:nth-child(n+3):nth-child(even) {
    background-color: #c2e0f7; /* Mild blue background for even rows */
    border-bottom: solid 2px black;
}

/* Odd rows within .table-primary7 - starting from 3rd row (after headers) */
.table-primary7 tr:nth-child(n+3):nth-child(odd) {
    background-color: #e0f1fb; /* Very light blue background for odd rows */
    border-bottom: solid 2px black;
}

/* Total row styling - keeping orange but with consistent borders */
.total-row {
    background-color: orange !important;
    color: black !important;
    font-weight: bold !important;
    border-bottom: solid 2px black !important;
}

/* Merged cells in total row */
.table-primary7 .total-row td[colspan="2"] {
    background-color: orange !important;
    border: 1px solid #ddd !important; /* Changed to match standard cell borders */
    border-left: solid 2px black !important; /* Maintain the left border pattern */
    font-weight: bold !important;
    padding: 8px; /* Match standard cell padding */
}

/* Ensure total row cells follow the same border pattern */
.table-primary7 .total-row td {
    border: 1px solid #ddd !important;
    border-bottom: solid 2px black !important;
}

       
.table-primary2 {
    height: 100%; /* Make the table fill the container's height */
    width: 100%; /* Ensure the table takes up the full width of the container */
    border-collapse: collapse; /* Ensures borders are merged between cells */
}

.table-primary2 th {
    color: white; /* White text color */
    font-weight: bold; /* Bold text for headers */
    padding: 10px; /* Padding for better readability */
    text-align: left; /* Align text to the left */
    border: 1px solid #ddd; /* Border around headers */
}

.table-primary2 td {
    padding: 8px; /* Padding for cells */
    border: 1px solid #ddd; /* Border around table cells */
}

td:nth-child(even), th:nth-child(even) {
    border-left: solid 2px black;
}

td:nth-child(odd), th:nth-child(odd) {
    border-left: solid 2px black;
}

/* Keep Overall OEM table borders thin/open despite global nth-child table border rules above. */
.overall-oem-table th {
    border-left: 1px solid #d9dde5 !important;
}

.overall-oem-table td {
    border-left: 1px solid #d9dde5 !important;
}

.overall-oem-table th:first-child,
.overall-oem-table td:first-child {
    border-left: none !important;
}

.overall-oem-table th:last-child,
.overall-oem-table td:last-child {
    border-right: none !important;
}

.overall-oem-table tbody tr:last-child td {
    border-bottom: none !important;
}

.overall-oem-table tbody tr.overall-oem-total-row td {
    background: #f8fafc;
    font-weight: 800;
    border-top: 2px solid #cbd5e1;
    border-bottom: 1px solid #d9dde5 !important;
}

.overall-oem-total-label {
    color: #1f2937;
    font-weight: 900;
    letter-spacing: 0.01em;
}

.overall-oem-total-trend {
    color: #475467;
    font-weight: 700;
}

/* First row within .table-primary2 */
.table-primary2 tr:first-child {
    background-color: #ffebcc; /* Mild yellow background for the first row */
    border-bottom: solid 2px black;
}

/* Even rows within .table-primary2 */
.table-primary2 tr:nth-child(even) {
    background-color: #c2e0f7; /* Mild blue background for even rows */
    border-bottom: solid 2px black;
}

/* Odd rows within .table-primary2 */
.table-primary2 tr:nth-child(odd) {
    background-color: #e0f1fb; /* Very light blue background for odd rows */
    border-bottom: solid 2px black;
}

/* Last row should have an orange background */
.table-primary2 tr:last-child {
    background-color: orange; /* Orange background for the last row */
    border-bottom: solid 2px black; /* Keep the border on the last row */
}




        .modal-import {
            display: none;
            position: fixed;
            z-index: 1060;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
        }

        .modal-content-import {
            background-color: #fff;
            margin: 10% auto;
            padding: 20px;
            border: 1px solid #888;
            width: 30%;
            text-align: center;
        }

        .close-import {
            color: red;
            float: right;
            font-size: 20px;
            cursor: pointer;
        }

        .gridview-container-wrapper {
            display: flex;
            gap: 20px; /* Adjust space between the two grid views */
        }

        .gridview-container3 {
            flex: 1; /* Makes the two grid views take equal width */
            padding: 10px; /* Optional, to give some space inside the grid containers */
        }

        /* Define a CSS class for the column */
        .status-column {
            width: 25vw !important; /* Force the width using !important */
            display: inline-block; /* Prevent overflow issues */
        }
        .impact-column {
            width: 25vw !important; /* Force the width using !important */
            display: inline-block; /* Prevent overflow issues */
        }

        .los-column {
    width: 5vw !important; /* Force the width using !important */
    display: inline-block; /* Prevent overflow issues */
}

        .component-column {
            width: 10vw !important; /* Force the width using !important */ /* Prevent overflow issues */
            word-wrap: break-word !important; /* Ensure long words break and wrap */
            word-wrap: break-word; /* Ensures long words will wrap */
            white-space: normal; /* Allows wrapping within the cell */
            overflow: visible;
            max-width: none;
        }

        .owner-column {
            width: 10vw !important; /* Force the width using !important */ /* Prevent overflow issues */
            word-wrap: break-word !important; /* Ensure long words break and wrap */
            word-wrap: break-word; /* Ensures long words will wrap */
            white-space: normal; /* Allows wrapping within the cell */
            overflow: visible;
            max-width: none;
        }

        .idst-column {
            width: 8vw !important; /* Force the width using !important */
            display: inline-block; /* Prevent overflow issues */
        }

        .notes-box {
            background-color: #e0f1fb;
            border-radius: 8px;
            padding: 16px;
            width: 42.3vw;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            display: flex;
            align-items: center;
            justify-content: space-between;
        }

        .notes-label {
            font-size: 14px;
            color: #555;
            font-weight: 500;
        }

        .notes-value {
            font-size: 16px;
            color: #333;
            font-weight: 600;
        }

        .form-control {
            font-size: 12px;
        }

        .form-control-impact {
    font-size: 12px;
}


        .link {
            position: relative;
            top: -30px;
            left: 92%;
        }

        .orange-background {
    background-color: orange !important;
    color: black !important;
}

        /* Update info styling */
#updateinfo {
    display: none !important;
    position: absolute;
    left: 1vw;
    font-size: 10px; /* smaller to fit nicely */
    font-family: sans-serif;
    margin: 0;
    line-height: 1.2;
    white-space: nowrap;
}

.tptdefinitions {
      padding: 15px;
      border: 1px solid #ccc;
      max-width: 550px;
      font-family: Arial, sans-serif;
      background-color: #f9f9f9;
      font-size: 10px;
      max-height: 100px;
    }
    .tptdefinitions h3 {
      margin-top: 0;
    }
    .tptdefinitions p {
      margin: 10px 0;
    }

    .button-wrapper {
        float: right;
        padding-right:35px;
}

    /* Custom modal styles */
.custom-modal-dialog {
  max-width: 85%; /* Set the width of the modal to 85% of the page */
  width: 85%; /* You can adjust this value to get the desired width */
}

/* Optional: Adjust modal body height if content overflows */
.custom-modal-body {
  max-height: 80vh; /* Set max-height of the modal body */
  overflow-y: auto; /* Enable scrolling if the content is too long */
}

/* Custom GridView scale */
.custom-gridview {
  transform: scale(0.8); /* Scale the entire GridView to 80% of its original size */
  transform-origin: top left; /* Optional: This makes sure the scaling starts from the top-left corner */
  margin: 0 auto; /* Center the GridView */
  width: 120%; /* Ensure GridView still takes up the full width of its container */
}

/* Optional: Adjust for text overflow or padding inside the GridView */
.custom-gridview td, .custom-gridview th {
  padding: 0.5rem; /* Reduce padding for smaller text in scaled grid */
}

/* Design Summary details popup: fit dialog to table width and center table */
#detailsModal0 .modal-dialog.custom-modal-dialog {
    width: max-content;
    max-width: calc(100vw - 40px);
    margin-left: auto;
    margin-right: auto;
}

#detailsModal0 .modal-content {
    width: max-content;
    max-width: 100%;
}

#detailsModal0 .modal-body.custom-modal-body {
    overflow-x: auto;
    overflow-y: auto;
}

#detailsModal0 .gridview-container {
    display: flex;
    justify-content: center;
    align-items: flex-start;
    width: 100%;
}

#detailsModal0 .custom-gridview {
    transform: none;
    transform-origin: top center;
    width: max-content;
    min-width: 0;
    margin: 0 auto;
}

/* OEM details popup: fit dialog to table width and center table */
#detailsModal3 .modal-dialog.custom-modal-dialog {
    width: max-content;
    max-width: calc(100vw - 40px);
    margin-left: auto;
    margin-right: auto;
}

#detailsModal3 .modal-content {
    width: max-content;
    max-width: 100%;
}

#detailsModal3 .modal-body.custom-modal-body {
    overflow-x: auto;
    overflow-y: auto;
}

#detailsModal3 .gridview-container {
    display: flex;
    justify-content: center;
    align-items: flex-start;
    width: 100%;
}

#detailsModal3 .gridview-container > div {
    width: max-content;
    margin-left: auto;
    margin-right: auto;
}

#detailsModal3 .custom-gridview {
    transform: none;
    transform-origin: top center;
    width: max-content;
    min-width: 0;
    margin: 0 auto;
}

/* Milestone issue popups: fit dialog to table width and center table */
#detailsModalDrivers .modal-dialog.custom-modal-dialog,
#detailsModalDrivers1 .modal-dialog.custom-modal-dialog {
    width: max-content;
    max-width: calc(100vw - 40px);
    margin-left: auto;
    margin-right: auto;
}

#detailsModalDrivers .modal-content,
#detailsModalDrivers1 .modal-content {
    width: max-content;
    max-width: 100%;
}

#detailsModalDrivers .modal-body.custom-modal-body,
#detailsModalDrivers1 .modal-body.custom-modal-body {
    overflow-x: auto;
    overflow-y: auto;
}

#detailsModalDrivers .gridview-container,
#detailsModalDrivers1 .gridview-container {
    display: flex;
    justify-content: center;
    align-items: flex-start;
    width: 100%;
}

#detailsModalDrivers .gridview-container > div,
#detailsModalDrivers1 .gridview-container > div {
    width: max-content;
    margin-left: auto;
    margin-right: auto;
}

#detailsModalDrivers .custom-gridview,
#detailsModalDrivers1 .custom-gridview {
    transform: none;
    transform-origin: top center;
    width: max-content;
    min-width: 0;
    margin: 0 auto;
}

/* Summary metric issue popup (Total/Open/Closed/Implemented): fit dialog to table width */
#detailsModalSummaryMetrics .modal-dialog.custom-modal-dialog {
    width: max-content;
    max-width: calc(100vw - 40px);
    margin-left: auto;
    margin-right: auto;
}

#detailsModalSummaryMetrics .modal-content {
    width: max-content;
    max-width: 100%;
}

#detailsModalSummaryMetrics .modal-body.custom-modal-body {
    overflow-x: auto;
    overflow-y: auto;
}

#detailsModalSummaryMetrics .gridview-container {
    display: flex;
    justify-content: center;
    align-items: flex-start;
    width: 100%;
}

#detailsModalSummaryMetrics .gridview-container > div {
    width: max-content;
    margin-left: auto;
    margin-right: auto;
}

#detailsModalSummaryMetrics .custom-gridview {
    transform: none;
    transform-origin: top center;
    width: max-content;
    min-width: 0;
    margin: 0 auto;
}

/* Keep clickable numeric fields centered in summary metric detail table. */
#detailsModalSummaryMetrics #GridView_summaryMetricIssues th:nth-child(3),
#detailsModalSummaryMetrics #GridView_summaryMetricIssues td:nth-child(3),
#detailsModalSummaryMetrics #GridView_summaryMetricIssues th:nth-child(4),
#detailsModalSummaryMetrics #GridView_summaryMetricIssues td:nth-child(4),
#detailsModalSummaryMetrics #GridView_summaryMetricIssues th:nth-child(20),
#detailsModalSummaryMetrics #GridView_summaryMetricIssues td:nth-child(20) {
    text-align: center;
}

#detailsModalSummaryMetrics #GridView_summaryMetricIssues td:nth-child(3) a {
    display: inline-block;
    text-align: center;
}

.summary-issues-popup-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.5);
    z-index: 1060;
    display: none;
}

.summary-issues-popup-backdrop.visible {
    display: block;
}

.summary-issues-popup {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: min(980px, calc(100vw - 44px));
    max-height: calc(100vh - 64px);
    border-radius: 12px;
    border: 1px solid #d0d9e8;
    background: #ffffff;
    box-shadow: 0 18px 44px rgba(15, 23, 42, 0.24);
    z-index: 1061;
    display: none;
    overflow: hidden;
}

.summary-issues-popup.visible {
    display: flex;
    flex-direction: column;
}

.summary-issues-popup-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 14px;
    border-bottom: 1px solid #e4e9f2;
    background: #f8fbff;
}

.summary-issues-popup-title {
    margin: 0;
    color: #1e3a60;
    font-size: 14px;
    font-weight: 900;
    text-transform: uppercase;
}

.summary-issues-popup-close {
    border: 0;
    background: transparent;
    color: #475467;
    font-size: 24px;
    line-height: 1;
    cursor: pointer;
}

.summary-issues-popup-controls {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 10px 14px;
    border-bottom: 1px solid #e4e9f2;
    background: #ffffff;
}

.summary-issues-popup-filter-stack {
    display: grid;
    grid-template-columns: auto minmax(172px, 1fr);
    column-gap: 8px;
    row-gap: 8px;
    align-items: center;
}

.summary-issues-popup-controls label {
    margin: 0;
    color: #344054;
    font-size: 11px;
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.02em;
}

.summary-issues-popup-controls select {
    min-width: 172px;
    height: 30px;
    border: 1px solid #cdd9e5;
    border-radius: 8px;
    background: #ffffff;
    color: #1f2937;
    font-size: 12px;
    font-weight: 700;
    padding: 0 8px;
}

.summary-issues-popup-checkers {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
}

.summary-issues-popup-checkers label {
    margin: 0;
    display: inline-flex;
    align-items: center;
    gap: 6px;
    color: #344054;
    font-size: 12px;
    font-weight: 700;
    text-transform: none;
    letter-spacing: 0;
}

.summary-issues-popup-checkers input[type="checkbox"] {
    width: 14px;
    height: 14px;
    margin: 0;
}

.summary-issues-popup-meta {
    color: #667085;
    font-size: 12px;
    font-weight: 700;
    margin-left: auto;
    text-align: right;
}

.summary-issues-popup-meta-link {
    color: #1d4ed8;
    text-decoration: none;
    font-weight: 800;
}

.summary-issues-popup-meta-link:hover,
.summary-issues-popup-meta-link:focus {
    color: #1e40af;
    text-decoration: underline;
}

.summary-issues-popup-meta-idst {
    display: block;
    margin-top: 2px;
}

.summary-issues-popup-body {
    padding: 12px 14px;
    overflow: auto;
    background: #f9fbff;
}

.summary-issues-popup-empty {
    padding: 20px 14px;
    color: #667085;
    font-size: 13px;
    font-weight: 700;
}

.summary-issues-group {
    border: 1px solid #deE6f2;
    border-radius: 10px;
    background: #ffffff;
    padding: 10px;
    margin-bottom: 10px;
}

.summary-issues-group-title {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    color: #1f3b61;
    font-size: 12px;
    font-weight: 900;
    text-transform: uppercase;
    letter-spacing: 0.02em;
    margin-bottom: 0;
}

.summary-issues-group-count {
    margin-left: auto;
    text-transform: none;
    letter-spacing: 0;
    text-align: center;
    color: #475467;
    font-size: 12px;
    font-weight: 800;
    white-space: nowrap;
    min-width: 96px;
}

.summary-issues-group-count-link {
    color: #1d4ed8;
    text-decoration: none;
    font-weight: 800;
}

.summary-issues-group-count-link:hover,
.summary-issues-group-count-link:focus {
    color: #1e40af;
    text-decoration: underline;
}

.summary-issues-group-links {
    color: #334155;
    font-size: 12px;
    line-height: 1.45;
}

.summary-issues-group-links a {
    color: #1d4ed8;
    text-decoration: none;
    font-weight: 700;
}

.summary-issues-group-links a:hover {
    text-decoration: underline;
}

#selectallcheckbox {
    visibility: hidden;
}


    /* Field visibility controls */

    /* COMPLETE HIDING OF TABLE STRUCTURE FOR UNSELECTED FIELDS */
    #overall_request_details .field-sno.hidden,
    #overall_request_details .field-progress.hidden,
    #overall_request_details .field-sightingid.hidden,
    #overall_request_details .field-issue_details.hidden,
    #overall_request_details .field-promotedid.hidden,
    #overall_request_details .field-title.hidden,
    #overall_request_details .field-component.hidden,
    #overall_request_details .field-owner.hidden,
    #overall_request_details .field-promoted_owner.hidden,
    #overall_request_details .field-rvp_repro.hidden,
    #overall_request_details .field-processor.hidden,
    #overall_request_details .field-status.hidden,
    #overall_request_details .field-idst.hidden,
    #overall_request_details .field-los.hidden,
    #overall_request_details .field-customer_company.hidden,
    #overall_request_details .field-customer_detail.hidden,
    #overall_request_details .field-imagefreeze.hidden,
    #overall_request_details .field-component_group.hidden,
    #overall_request_details .field-duplicatedetails.hidden,
    #overall_request_details .field-impact.hidden,
    #overall_request_details .field-impact_processor.hidden,
    #overall_request_details .field-milestone.hidden,
    #overall_request_details .field-days_open.hidden,
    #overall_request_details .field-cmf_request.hidden,
    #overall_request_details .field-cmf_status.hidden,
    #overall_request_details .field-closed_reason.hidden,
    #overall_request_details .field-edit_column.hidden {
        display: none !important;
        visibility: hidden !important;
        width: 0 !important;
        min-width: 0 !important;
        max-width: 0 !important;
        padding: 0 !important;
        margin: 0 !important;
        border: none !important;
        border-width: 0 !important;
        overflow: hidden !important;
    }

    /* Hide empty data template columns as well */
    #overall_request_details .empty-data-container .field-sno.hidden,
    #overall_request_details .empty-data-container .field-progress.hidden,
    #overall_request_details .empty-data-container .field-sightingid.hidden,
    #overall_request_details .empty-data-container .field-issue_details.hidden,
    #overall_request_details .empty-data-container .field-promotedid.hidden,
    #overall_request_details .empty-data-container .field-title.hidden,
    #overall_request_details .empty-data-container .field-component.hidden,
    #overall_request_details .empty-data-container .field-owner.hidden,
    #overall_request_details .empty-data-container .field-promoted_owner.hidden,
    #overall_request_details .empty-data-container .field-rvp_repro.hidden,
    #overall_request_details .empty-data-container .field-processor.hidden,
    #overall_request_details .empty-data-container .field-status.hidden,
    #overall_request_details .empty-data-container .field-idst.hidden,
    #overall_request_details .empty-data-container .field-los.hidden,
    #overall_request_details .empty-data-container .field-customer_company.hidden,
    #overall_request_details .empty-data-container .field-customer_detail.hidden,
    #overall_request_details .empty-data-container .field-duplicatedetails.hidden,
    #overall_request_details .empty-data-container .field-component_group.hidden,
    #overall_request_details .empty-data-container .field-imagefreeze.hidden,
    #overall_request_details .empty-data-container .field-impact.hidden,
    #overall_request_details .empty-data-container .field-impact_processor.hidden,
    #overall_request_details .empty-data-container .field-milestone.hidden,
    #overall_request_details .empty-data-container .field-days_open.hidden,
    #overall_request_details .empty-data-container .field-cmf_request.hidden,
    #overall_request_details .empty-data-container .field-closed_reason.hidden,
    #overall_request_details .empty-data-container .field-edit_column.hidden {
        display: none !important;
        visibility: hidden !important;
        width: 0 !important;
        min-width: 0 !important;
        max-width: 0 !important;
        padding: 0 !important;
        margin: 0 !important;
        border: none !important;
        border-width: 0 !important;
        overflow: hidden !important;
    }

    /* Force table layout to recalculate when columns are hidden */
    #overall_request_details {
        border-collapse: separate !important;
        border-spacing: 0 10px !important;
        border: 0 !important;
        background: transparent !important;
        overflow: visible !important;
    }

    #overall_request_details table {
        table-layout: auto !important;
        width: 100% !important;
    }

    /* Responsive design */
    #<%= overall_request_details.ClientID %> .field-sno.hidden,
    #<%= overall_request_details.ClientID %> .field-progress.hidden,
    #<%= overall_request_details.ClientID %> .field-sightingid.hidden,
    #<%= overall_request_details.ClientID %> .field-issue_details.hidden,
    #<%= overall_request_details.ClientID %> .field-promotedid.hidden,
    #<%= overall_request_details.ClientID %> .field-title.hidden,
    #<%= overall_request_details.ClientID %> .field-component.hidden,
    #<%= overall_request_details.ClientID %> .field-owner.hidden,
    #<%= overall_request_details.ClientID %> .field-promoted_owner.hidden,
    #<%= overall_request_details.ClientID %> .field-rvp_repro.hidden,
    #<%= overall_request_details.ClientID %> .field-processor.hidden,
    #<%= overall_request_details.ClientID %> .field-status.hidden,
    #<%= overall_request_details.ClientID %> .field-idst.hidden,
    #<%= overall_request_details.ClientID %> .field-los.hidden,
    #<%= overall_request_details.ClientID %> .field-customer_company.hidden,
    #<%= overall_request_details.ClientID %> .field-customer_detail.hidden,
    #<%= overall_request_details.ClientID %> .field-imagefreeze.hidden,
    #<%= overall_request_details.ClientID %> .field-component_group.hidden,
    #<%= overall_request_details.ClientID %> .field-duplicatedetails.hidden,
    #<%= overall_request_details.ClientID %> .field-impact.hidden,
    #<%= overall_request_details.ClientID %> .field-impact_processor.hidden,
    #<%= overall_request_details.ClientID %> .field-milestone.hidden,
    #<%= overall_request_details.ClientID %> .field-days_open.hidden,
    #<%= overall_request_details.ClientID %> .field-cmf_request.hidden,
    #<%= overall_request_details.ClientID %> .field-cmf_status.hidden,
    #<%= overall_request_details.ClientID %> .field-closed_reason.hidden,
    #<%= overall_request_details.ClientID %> .field-edit_column.hidden {
        display: none !important;
        visibility: hidden !important;
        width: 0 !important;
        min-width: 0 !important;
        max-width: 0 !important;
        padding: 0 !important;
        margin: 0 !important;
        border: none !important;
        border-width: 0 !important;
        overflow: hidden !important;
    }
    #<%= overall_request_details.ClientID %> .empty-data-container .field-sno.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-progress.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-sightingid.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-issue_details.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-promotedid.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-title.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-component.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-owner.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-promoted_owner.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-rvp_repro.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-processor.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-status.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-idst.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-los.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-customer_company.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-customer_detail.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-duplicatedetails.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-component_group.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-imagefreeze.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-impact.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-impact_processor.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-milestone.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-days_open.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-cmf_request.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-closed_reason.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-edit_column.hidden {
        display: none !important;
        visibility: hidden !important;
        width: 0 !important;
        min-width: 0 !important;
        max-width: 0 !important;
        padding: 0 !important;
        margin: 0 !important;
        border: none !important;
        border-width: 0 !important;
        overflow: hidden !important;
    }
    #<%= overall_request_details.ClientID %> {
        table-layout: auto !important;
    }

    #<%= overall_request_details.ClientID %> table {
        table-layout: auto !important;
        width: 100% !important;
    }
    @media (max-width: 768px) {
        .checkbox-grid {
            grid-template-columns: 1fr;
        }

        .panel-actions {
            justify-content: center;
        }
    }

    /* Second-pass dashboard polish */
    .field-selector-panel {
        background: #ffffff;
        border: 1px solid #e0e0e0;
        border-radius: 10px;
        margin: 8px 0 12px;
        padding: 10px 12px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
    }

    .field-selector-toggle {
        background: #f5f5f5;
        color: #242424;
        border: 1px solid #d1d1d1;
        padding: 8px 12px;
        border-radius: 8px;
        cursor: pointer;
        font-size: 12px;
        font-weight: 600;
        transition: all 0.2s ease;
    }

    .field-selector-toggle:hover {
        background: #eef6ff;
        border-color: #9fc3e7;
    }

    .field-checkboxes {
        display: none;
        margin-top: 10px;
        padding-top: 10px;
        border-top: 1px solid #efefef;
    }

    .field-checkboxes.show {
        display: block;
    }

    .checkbox-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(190px, 1fr));
        gap: 6px 10px;
        max-height: 240px;
        overflow-y: auto;
        padding-right: 4px;
    }

    .checkbox-grid label {
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 0;
        padding: 6px 8px;
        border: 1px solid #efefef;
        border-radius: 8px;
        background: #fcfcfc;
        font-size: 12px;
        color: #3a3a3a;
    }

    .checkbox-grid input[type="checkbox"] {
        accent-color: #0068b5;
        width: 14px;
        height: 14px;
    }

    .panel-actions {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
        margin-top: 10px;
    }

    .panel-actions .btn {
        font-size: 12px;
        padding: 6px 10px;
        border-radius: 6px;
        min-height: 30px;
    }

    .button-container-focus {
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
        margin: 12px 0 10px;
        padding: 10px;
        border-radius: 14px;
        background: linear-gradient(180deg, rgba(255, 255, 255, 0.94) 0%, rgba(244, 248, 252, 0.98) 100%);
        border: 1px solid rgba(184, 201, 218, 0.8);
        box-shadow: 0 6px 20px rgba(16, 45, 74, 0.08);
    }

    .button-container-focus .tab-pill {
        border-radius: 999px;
        border: 1px solid #aac3db;
        background: #f6fbff;
        color: #164068;
        font-weight: 700;
        letter-spacing: 0.02em;
        min-height: 38px;
        padding: 8px 18px;
    }

    .button-container-focus .tab-pill:hover {
        background: #e9f4ff;
        border-color: #7ea4c8;
    }

    .button-container-focus .tab-pill.is-active {
        color: #ffffff;
        background: linear-gradient(120deg, #0d4f90 0%, #0f66b7 65%, #00a6b2 100%);
        border-color: #0f5ea8;
        box-shadow: 0 8px 16px rgba(15, 94, 168, 0.28);
    }

    .platform-switcher-modern {
        display: flex;
        align-items: center;
        justify-content: flex-start;
        gap: 12px;
        padding: 10px 12px;
        margin: 8px 0 10px;
        border: 1px solid #e0e0e0;
        border-radius: 10px;
        background: #ffffff;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
        flex-wrap: wrap;
    }

    .platform-chip-rail {
        display: flex;
        align-items: center;
        gap: 6px;
        overflow-x: auto;
        padding-bottom: 2px;
        flex: 1;
    }

    .platform-chip {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        border: 1px solid #d1d1d1;
        background: #fafafa;
        color: #424242;
        border-radius: 999px;
        padding: 6px 12px;
        min-height: 29px;
        font-size: 12px;
        font-weight: 600;
        line-height: 1;
        cursor: pointer;
        white-space: nowrap;
        box-sizing: border-box;
    }

    .platform-chip-label {
        line-height: 1;
    }

    .platform-chip-state {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        visibility: hidden;
        width: 16px;
        min-width: 16px;
        height: 16px;
        border-radius: 999px;
        font-size: 10px;
        font-weight: 900;
        line-height: 14px;
        text-align: center;
        border: 1px solid transparent;
        cursor: pointer;
        user-select: none;
    }

    .platform-chip-rail:not(.is-overall-selection) .platform-chip {
        gap: 0;
    }

    .platform-chip-rail:not(.is-overall-selection) .platform-chip-state {
        width: 0;
        min-width: 0;
        border-width: 0;
        margin: 0;
        padding: 0;
        overflow: hidden;
        opacity: 0;
        pointer-events: none;
    }

    .platform-chip-rail.is-overall-selection .platform-chip[data-platform="CMF_OVERALL_ALL_COMPONENTS_TABLE"] {
        gap: 0;
    }

    .platform-chip-rail.is-overall-selection .platform-chip[data-platform="CMF_OVERALL_ALL_COMPONENTS_TABLE"] .platform-chip-state {
        width: 0;
        min-width: 0;
        border-width: 0;
        margin: 0;
        padding: 0;
        overflow: hidden;
        opacity: 0;
        pointer-events: none;
    }

    .platform-chip.is-platform-included .platform-chip-state {
        visibility: visible;
        background: #dcfce7;
        border-color: #86efac;
        color: #166534;
    }

    .platform-chip.is-platform-excluded .platform-chip-state {
        visibility: visible;
        background: #fee2e2;
        border-color: #fca5a5;
        color: #991b1b;
    }

    .platform-chip:hover {
        border-color: #9fc3e7;
        background: #f2f8ff;
    }

    .platform-chip.is-active {
        background: #eff6fc;
        border-color: #0068b5;
        color: #0068b5;
    }

    .issue-page-hd {
        margin: 0 0 8px;
        display: flex;
        align-items: flex-end;
        justify-content: space-between;
        gap: 12px;
        flex-wrap: wrap;
    }

    .issue-page-hd-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        flex-wrap: wrap;
    }

    .issue-page-title {
        font-size: 18px;
        font-weight: 800;
        color: #10243a;
        margin: 0 0 2px;
    }

    .issue-page-desc {
        margin: 0;
        color: #607086;
        font-size: 12px;
        line-height: 1.5;
    }

    .issue-kpi-row {
        display: grid;
        grid-template-columns: repeat(4, minmax(140px, 1fr));
        gap: 8px;
        margin-bottom: 10px;
    }

    .issue-kpi {
        background: linear-gradient(180deg, #ffffff 0%, #fbfdff 100%);
        border: 1px solid #e3ebf4;
        border-radius: 8px;
        padding: 10px 12px;
        box-shadow: 0 8px 20px rgba(20, 46, 77, 0.06);
    }

    .issue-kpi-label {
        font-size: 11px;
        color: #62748a;
        margin-bottom: 5px;
        font-weight: 800;
        letter-spacing: 0.03em;
        text-transform: uppercase;
    }

    .issue-kpi-value {
        font-size: 25px;
        line-height: 1;
        font-weight: 800;
        color: #14283d;
    }

    .issue-kpi-sub {
        margin-top: 4px;
        font-size: 10px;
        color: #7c8da0;
    }

    /* CMF Pending List Styles */
    .cmf-pending-page-hd {
        margin: 0 0 12px;
    }

    .cmf-pending-page-hd-top {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        flex-wrap: wrap;
    }

    .cmf-pending-page-title {
        font-size: 18px;
        font-weight: 800;
        color: #111827;
        margin: 0;
    }

    .overview-platform-actions {
        background: #f5f8fb;
        border: 1px solid #d8e3ee;
        border-radius: 8px;
        padding: 8px 10px;
        margin-bottom: 10px;
        display: flex;
        gap: 12px;
        align-items: center;
        justify-content: space-between;
        flex-wrap: wrap;
        font-size: 13px;
    }

    .overview-platform-filter {
        display: flex;
        align-items: center;
        gap: 6px;
        min-width: 260px;
    }

    .overview-platform-filter label {
        font-weight: 600;
        color: #0f3554;
        white-space: nowrap;
    }

    .overview-platform-filter .form-control {
        max-width: 210px;
        padding: 5px 8px;
        border: 1px solid #c8d9ea;
        border-radius: 4px;
        font-size: 12px;
    }

    .cmf-pending-external-links {
        display: inline-flex;
        align-items: center;
        gap: 10px;
        flex-wrap: wrap;
    }

    .cmf-pending-external-links a {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-height: 34px;
        padding: 8px 12px;
        border-radius: 999px;
        border: 1px solid #d7e1ec;
        background: #f8fbff;
        color: #1b4e7f;
        font-size: 12px;
        font-weight: 700;
        text-decoration: none;
        transition: all 0.2s ease;
    }

    .cmf-pending-external-links a:hover {
        background: #e6f0fb;
        color: #0a3a66;
        border-color: #bfd4e5;
    }

    .cmf-pending-page-desc {
        margin: 4px 0 0;
        color: #64748b;
        font-size: 12px;
        line-height: 1.45;
    }

    .cmf-pending-kpi-row {
        display: grid;
        grid-template-columns: repeat(4, minmax(150px, 1fr));
        gap: 12px;
        margin: 10px 0 14px;
    }

    .cmf-pending-kpi {
        position: relative;
        min-height: 92px;
        background: #ffffff;
        border: 1px solid #e8edf5;
        border-radius: 10px;
        padding: 14px 16px;
        box-shadow: 0 8px 20px rgba(28, 48, 78, 0.05);
        overflow: hidden;
    }

    .cmf-pending-kpi::before {
        content: "";
        position: absolute;
        inset: 0 auto 0 0;
        width: 4px;
        background: #ef4444;
        opacity: 0.85;
    }

    .cmf-pending-kpi:nth-child(2)::before { background: #22c55e; }
    .cmf-pending-kpi:nth-child(3)::before { background: #f59e0b; }
    .cmf-pending-kpi:nth-child(4)::before { background: #6366f1; }

    .cmf-pending-kpi-label {
        font-size: 11px;
        color: #334155 !important;
        font-weight: 800;
        letter-spacing: 0;
        text-transform: none;
        margin-bottom: 8px;
    }

    .cmf-pending-kpi-value {
        font-size: 27px;
        line-height: 1;
        font-weight: 800;
        color: #ef4444;
        margin-bottom: 6px;
    }

    .cmf-pending-kpi-sub {
        margin-top: 0;
        font-size: 11px;
        color: #64748b;
        font-weight: 600;
    }

    .reports-workspace {
        padding: 12px 0 24px;
        display: flex;
        flex-direction: column;
        gap: 20px;
        min-height: 0;
    }

    .reports-workspace .portal-tab-hero,
    .config-rules-workspace .portal-tab-hero {
        padding: 0;
        border: 0;
        background: transparent;
        box-shadow: none;
    }

    .reports-workspace .portal-surface-card,
    .config-rules-workspace .portal-surface-card {
        border: 0;
        border-radius: 0;
        background: transparent;
        padding: 0;
        box-shadow: none;
    }

    .reports-assistant-card {
        display: flex;
        flex-direction: column;
        min-height: 420px;
        border: 1px solid #cfe2f3 !important;
        border-radius: 14px !important;
        background: linear-gradient(180deg, #ffffff 0%, #f4fbff 100%) !important;
        padding: 18px !important;
        box-shadow: 0 16px 36px rgba(15, 63, 89, 0.10) !important;
    }

    .reports-assistant-head {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
        margin-bottom: 12px;
    }

    .reports-card-title {
        margin: 0;
        color: #103a5c;
        font-size: 19px;
        font-weight: 900;
    }

    .reports-card-subtitle {
        margin-top: 4px;
        color: #5c7087;
        font-size: 12px;
    }

    .reports-chat-example {
        max-width: 80%;
        padding: 10px 12px;
        border-radius: 8px;
        white-space: pre-wrap;
        font-size: 13px;
        line-height: 1.4;
        background: #eef4fb;
        color: #12344d;
        border: 1px solid #d6e2ef;
    }

    .reports-chat-log {
        flex: 1 1 auto;
        overflow: auto;
        border: 1px solid #d6e8f5;
        border-radius: 12px;
        background: linear-gradient(180deg, #fbfdff 0%, #f4faff 100%);
        padding: 14px;
        margin-bottom: 12px;
        min-height: 260px;
        max-height: 420px;
    }

    .reports-prompt-row {
        display: flex;
        gap: 10px;
        align-items: center;
        flex-shrink: 0;
    }

    .reports-prompt-input {
        flex: 1;
        min-width: 220px;
        border: 1px solid #c8d9ea;
        border-radius: 10px;
        padding: 10px 12px;
        font-size: 13px;
        color: #18324a;
        background: #ffffff;
    }

    .cmf-pending-grid-wrap {
        overflow-x: auto !important;
        overflow-y: visible !important;
        width: 100%;
        max-width: 100%;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending {
        table-layout: fixed !important;
        width: 100% !important;
        min-width: 1260px !important;
        border-collapse: separate !important;
        border-spacing: 0 !important;
        border: 1px solid #e8edf5 !important;
        border-radius: 10px !important;
        overflow: hidden !important;
        background: #ffffff !important;
        box-shadow: 0 10px 24px rgba(28, 48, 78, 0.05) !important;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending th,
    .cmf-pending-grid-wrap #GridView_cmf_pending td {
        padding: 12px !important;
        white-space: normal !important;
        overflow-wrap: anywhere;
        word-break: break-word;
        vertical-align: top !important;
        max-width: 100%;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending td *,
    .cmf-pending-grid-wrap #GridView_cmf_pending th * {
        max-width: 100%;
        min-width: 0;
        overflow-wrap: anywhere;
        word-break: break-word;
        white-space: normal;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending th {
        background: #f7f9fc !important;
        color: #243042 !important;
        border: 0 !important;
        border-bottom: 1px solid #e5ebf3 !important;
        font-size: 10px !important;
        font-weight: 800 !important;
        letter-spacing: 0 !important;
        text-transform: none !important;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending td {
        background: #ffffff !important;
        border: 0 !important;
        border-bottom: 1px solid #eef2f7 !important;
        box-shadow: none !important;
        color: #1f2937 !important;
        font-size: 12px !important;
        line-height: 1.45;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending tr:hover td {
        background: #fbfdff !important;
    }

    .pending-select-cell {
        display: flex;
        align-items: flex-start;
        justify-content: center;
        min-height: 42px;
        position: relative;
    }

    .pending-row-check {
        width: 14px;
        height: 14px;
        border: 1px solid #b9c8d8;
        border-radius: 3px;
        background: #ffffff;
        box-shadow: 0 1px 2px rgba(31, 59, 87, 0.08);
        margin-top: 2px;
        display: inline-flex;
    }

    .pending-select-header .pending-row-check {
        margin-top: 0;
        border-color: #aebfd1;
        background: #f8fbff;
    }

    .pending-issue-cell,
    .pending-issue-with-action,
    .pending-customer-cell,
    .pending-component-cell,
    .pending-status-cell,
    .pending-signals-cell {
        display: flex;
        flex-direction: column;
        gap: 6px;
        align-items: flex-start;
        max-width: 100%;
        min-width: 0;
    }

    .pending-issue-with-action {
        gap: 8px;
    }

    .pending-issue-action-row {
        display: inline-flex;
        flex-wrap: wrap;
        gap: 6px;
        align-items: center;
    }

    .pending-recommendation-btn {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        align-self: flex-start;
        min-height: 28px;
        padding: 4px 9px;
        border-radius: 999px;
        border: 1px solid #cdddf2;
        background: #f5f9ff;
        color: #1f5f9e;
        font-size: 11px;
        font-weight: 800;
        cursor: pointer;
    }

    .pending-decision-details-btn {
        width: 28px;
        min-width: 28px;
        justify-content: center;
        padding: 4px 0;
        color: #5b21b6;
        background: #f7f2ff;
        border-color: #ddd0fe;
    }

    .pending-impact-details-btn {
        color: #9a3412;
        background: #fff7ed;
        border-color: #fed7aa;
    }

    .pending-recommendation-btn:hover {
        background: #eaf3ff;
        border-color: #9ec5ee;
    }

    .pending-decision-details-btn:hover {
        background: #efe7ff;
        border-color: #c4b5fd;
    }

    .pending-impact-details-btn:hover {
        background: #ffedd5;
        border-color: #fdba74;
    }

    .pending-ai-rec-cell {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 100%;
    }

    .pending-ai-rec-btn {
        flex-direction: row;
        align-items: center;
        justify-content: center;
        gap: 6px;
        min-width: 112px;
        min-height: 34px;
        border-radius: 10px;
        background: #f0fdf4;
        border-color: #bbf7d0;
        color: #15803d;
        text-align: center;
    }

    .pending-ai-rec-icon {
        font-size: 13px;
        line-height: 1;
    }

    .pending-ai-rec-label {
        font-size: 11px;
        font-weight: 900;
        line-height: 1.1;
    }

    .pending-ai-rec-confidence {
        font-size: 9px;
        font-weight: 800;
        color: #64748b;
        line-height: 1.1;
    }

    .pending-id-link {
        font-weight: 850;
        color: #075ea8;
        text-decoration: none;
    }

    .pending-title-text {
        font-size: 11px;
        font-weight: 750;
        color: #172b3f;
        line-height: 1.3;
        max-width: 100%;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .pending-mini-label {
        font-size: 9px;
        font-weight: 850;
        color: #6b7b8f;
        text-transform: uppercase;
        letter-spacing: 0.04em;
    }

    .pending-chip-row {
        display: flex;
        flex-wrap: wrap;
        gap: 5px;
    }

    .pending-chip {
        display: inline-flex;
        align-items: center;
        min-height: 18px;
        padding: 2px 6px;
        border-radius: 999px;
        background: #eef5fb;
        color: #214967;
        font-size: 10px;
        font-weight: 800;
    }

    .pane-heading {
        margin-top: 10px;
    }

    /* Fluent-inspired focused UI overrides for Issue/Pending/Reports.
       These are intentionally last to safely override legacy styles. */
    header {
        min-height: 48px;
        padding: 0 18px;
        justify-content: center;
        background: linear-gradient(90deg, #00377c 0%, #0068b5 100%);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.18);
    }

    .header-title {
        font-size: 15px;
        letter-spacing: 0.04em;
        text-transform: none;
        font-weight: 600;
    }

    #updateinfo {
        left: 16px;
        font-size: 11px;
        color: rgba(255, 255, 255, 0.92);
        line-height: 1.35;
        display: none !important;
    }

    .sidebar-toggle-div {
        right: 16px;
    }

    .sidebar-toggle {
        width: 30px;
        height: 30px;
        font-size: 14px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.18);
    }

    .dropdown {
        height: 34px;
        border-radius: 18px;
        border: 1px solid #d1d1d1;
        box-shadow: none;
        font-size: 12px;
        background: #ffffff;
    }

    .button-container.button-container-focus {
        margin: 10px 0 8px;
        padding: 0;
        gap: 0;
        background: #ffffff;
        border: 1px solid #e0e0e0;
        border-left: none;
        border-right: none;
        border-radius: 0;
        box-shadow: none;
        justify-content: flex-start;
        overflow-x: auto;
        flex-wrap: nowrap;
    }

    .button-container.button-container-focus .link-container {
        margin: 0 0 0 auto;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        padding: 6px 8px;
        border: none;
        background: transparent;
        box-shadow: none;
        flex-wrap: nowrap;
    }

    .button-container.button-container-focus .link-container a,
    .button-container.button-container-focus .link-container .aspNetDisabled {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-height: 34px;
        padding: 8px 12px;
        font-size: 12px;
        border-radius: 999px;
        white-space: nowrap;
    }

    .button-container-focus .tab-pill {
        border: none;
        border-bottom: 2px solid transparent;
        border-radius: 0;
        min-height: 40px;
        padding: 10px 18px;
        background: transparent;
        color: #616161;
        box-shadow: none;
        font-size: 13px;
        font-weight: 500;
    }

    .button-container-focus .tab-pill:hover {
        background: #f5f5f5;
        color: #242424;
        border-color: transparent;
    }

    .button-container-focus .tab-pill.is-active {
        background: transparent;
        color: #0068b5;
        border-bottom-color: #0068b5;
        box-shadow: none;
    }

    .link-container {
        margin-top: 10px;
        justify-content: flex-start;
        background: #ffffff;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        box-shadow: none;
        padding: 8px 10px;
    }

    .link-container a {
        background: #f5f5f5;
        color: #242424;
        font-weight: 600;
        font-size: 12px;
        border-radius: 999px;
    }

    .container-fluid.page-body-wrapper {
        padding-left: 4px !important;
        padding-right: 4px !important;
        margin-left: 0 !important;
        margin-right: 0 !important;
        max-width: 100% !important;
    }

    .portal-layout-shell {
        gap: 8px;
        margin-top: 0;
        flex-wrap: wrap;
    }

    .portal-layout-shell > .portal-main-workspace {
        flex: 1 1 calc(100% - 228px);
    }

    .portal-layout-shell > .gridview-container-wrapper {
        flex: 0 0 100%;
        width: 100%;
        margin-top: 8px;
    }

    .portal-left-nav {
        padding: 10px 8px;
        top: 48px;
        min-height: calc(100vh - 48px);
        max-height: calc(100vh - 48px);
    }

    .portal-section-header {
        margin: 0 2px 10px;
        padding: 6px 0;
    }

    .portal-left-nav .portal-nav-link {
        margin: 0 0 8px;
        padding: 10px 10px;
    }

    .content-wrapper {
        margin: 6px 4px 10px;
        padding: 8px;
        background: #ffffff;
        border: 1px solid #e0e0e0;
        border-radius: 12px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
        min-height: 0;
    }

    .table-container {
        max-height: none;
        overflow: visible;
        border: none;
        box-shadow: none;
        background: transparent;
        padding: 0;
    }

    .pane-heading {
        margin: 8px 0 10px;
        padding: 10px 14px;
        border-radius: 8px;
        border: 1px solid #e0e0e0;
        background: #fafafa;
        color: #242424;
        font-size: 14px;
        letter-spacing: 0.03em;
        text-transform: uppercase;
        text-align: left;
        display: none !important;
    }

    .portal-view-row {
        display: flex;
        justify-content: flex-start;
        margin: 10px 0 8px;
    }

    .portal-view-dropdown {
        min-width: 240px;
        height: 40px;
        border-radius: 10px;
        border: 1px solid #c6d4e1;
        background: #ffffff;
        color: #23364b;
        font-weight: 600;
        padding: 0 10px;
    }

    .issue-platform-links-row {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 12px;
        margin: 8px 0 10px;
        flex-wrap: wrap;
    }

    .issue-platform-switcher {
        flex: 1;
        min-width: 420px;
        margin: 0;
    }

    .issue-external-links {
        display: inline-flex;
        align-items: center;
        gap: 10px;
        flex-wrap: wrap;
    }

    .issue-external-links a,
    .issue-external-links .aspNetDisabled {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-height: 34px;
        padding: 8px 12px;
        border-radius: 999px;
        border: 1px solid #d7e1ec;
        background: #f8fbff;
        color: #1b4e7f;
        font-size: 12px;
        font-weight: 700;
        text-decoration: none;
        transition: all 0.2s ease;
    }

    .issue-external-links a:hover {
        background: #e6f0fb;
        color: #0a3a66;
        border-color: #bfd4e5;
    }

    .field-selector-panel {
        margin: 8px 0 12px;
        border-radius: 10px;
    }

    .gridview-container {
        margin-top: 0;
        padding-top: 0;
        overflow-x: auto;
    }

    .gridview-container .table-primary,
    .gridview-container #overall_request_details {
        min-width: 100% !important;
        width: 100% !important;
        table-layout: fixed;
    }

    .gridview-container .table-primary th,
    .gridview-container .table-primary td {
        white-space: normal !important;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .empty-data-container table.table-primary {
        min-width: 100% !important;
        width: 100% !important;
    }

    .table-primary td,
    #overall_request_details td {
        white-space: normal;
        word-break: break-word;
        padding: 16px !important;
        vertical-align: top !important;
    }

    .table-primary .field-title,
    .table-primary .field-customer_detail,
    .table-primary .field-duplicatedetails {
        min-width: 180px;
        max-width: 300px;
    }

    .table-primary .field-milestone {
        min-width: 120px;
        max-width: 200px;
    }

    /* Dynamic-width columns: size to content, wrap long lines */
    .table-primary th,
    .table-primary td {
        width: auto;
        word-break: break-word;
        overflow-wrap: anywhere;
        white-space: normal;
        max-width: 100%;
    }

    .table-primary td *,
    #overall_request_details td * {
        max-width: 100%;
        min-width: 0;
        overflow-wrap: anywhere;
        word-break: break-word;
        white-space: normal;
    }

    .table-primary .field-component,
    .table-primary .field-owner,
    .table-primary .field-promoted_owner {
        min-width: 160px;
        max-width: 260px;
    }
    .table-primary .field-sno { min-width: 42px; max-width: 56px; }
    .table-primary .field-sightingid { min-width: 140px; max-width: 200px; }
    .table-primary .field-status { min-width: 180px; max-width: 240px; }
    .table-primary .field-cmf_status { min-width: 130px; max-width: 240px; }
    .table-primary .field-imagefreeze { min-width: 80px; max-width: 110px; }
    .table-primary .field-days_open { min-width: 60px; max-width: 80px; }
    .table-primary .field-impact { min-width: 130px; max-width: 220px; }
    .table-primary .field-rvp_repro { min-width: 90px; max-width: 140px; }
    .table-primary .field-idst { min-width: 110px; max-width: 200px; }
    .table-primary .field-los { min-width: 60px; max-width: 80px; }

    /* Customer Detail combined cell */
    .issue-details-cell {
        max-width: 100%; /* This line is now correctly placed */
        display: flex;
        flex-direction: column;
        gap: 12px;
        align-items: flex-start;
    }

    .issue-title-text {
        color: #172b3f;
        word-break: break-word;
        max-width: 100%;
        line-height: 1.35;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending td *,
    .cmf-pending-grid-wrap #GridView_cmf_pending th * {
        max-width: 100%;
        min-width: 0;
        overflow-wrap: anywhere;
        word-break: break-word;
        white-space: normal;
    }

    .issue-meta-row {
        display: flex;
        flex-wrap: wrap;
        gap: 6px;
        align-items: center;
        max-width: 100%;
        min-width: 0;
    }

    .issue-id-link,
    .issue-promoted-id,
    .cmf-request-chip {
        display: inline-flex;
        max-width: 100%;
        overflow-wrap: anywhere;
        word-break: break-word;
        align-items: center;
        min-height: 20px;
        padding: 2px 7px;
        border-radius: 999px;
        font-size: 10px;
        font-weight: 800;
        line-height: 1;
        text-decoration: none;
        white-space: nowrap;
    }

    .issue-id-link {
        color: #075ea8;
        background: #eaf4ff;
        border: 1px solid #c9dff3;
    }

    .issue-promoted-id {
        color: #4f5f73;
        background: #f3f6fa;
        border: 1px solid #e0e7ef;
    }

    .cmf-request-chip {
        color: #0f766e;
        background: #e9fbf7;
        border: 1px solid #c8efe6;
    }

    .days-open-pill {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 46px;
        min-height: 30px;
        padding: 4px 8px;
        border-radius: 999px;
        background: #fff5f1;
        color: #c2410c;
        border: 1px solid #fed7c7;
        font-weight: 800;
        font-size: 12px;
    }

    .impact-processor-cell,
    .milestone-progress-cell {
        display: flex;
        flex-direction: column;
        gap: 7px;
        align-items: flex-start;
    }

    .impact-processor-cell {
        gap: 16px;
    }

    .impact-text {
        font-size: 12px;
        color: #26384d;
        font-weight: 700;
        line-height: 1.35;
        overflow-wrap: anywhere;
    }

    .progress-track {
        display: inline-flex;
        align-items: center;
        gap: 5px;
    }

    .progress-dot {
        width: 7px;
        height: 7px;
        border-radius: 50%;
        background: #d5deea;
    }

    .progress-dot.is-active { background: #0f6db5; }

    .progress-badge {
        display: inline-flex;
        padding: 2px 8px;
        border-radius: 999px;
        background: #eef6ff;
        color: #095c9f;
        border: 1px solid #cfe4f8;
        font-size: 10px;
        font-weight: 800;
        text-transform: uppercase;
    }

    .milestone-progress-fill {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 54px;
        width: 100%;
        border-radius: 10px;
        padding: 10px 8px;
        text-align: center;
        color: #ffffff;
        font-weight: 900;
        font-size: 12px;
        line-height: 1.25;
        box-shadow: inset 0 -12px 22px rgba(0, 0, 0, 0.08);
        overflow-wrap: anywhere;
    }

    .milestone-progress-red { background: linear-gradient(135deg, #ef4444 0%, #b91c1c 100%); }
    .milestone-progress-orange { background: linear-gradient(135deg, #f97316 0%, #c2410c 100%); }
    .milestone-progress-yellow { background: linear-gradient(135deg, #facc15 0%, #ca8a04 100%); color: #3f2f05; }
    .milestone-progress-green { background: linear-gradient(135deg, #22c55e 0%, #15803d 100%); }
    .milestone-progress-neutral { background: linear-gradient(135deg, #64748b 0%, #334155 100%); }

    .reports-format-card {
        flex-shrink: 0;
        padding-top: 4px !important;
    }

    .reports-format-title {
        margin: 0 0 4px;
        color: #0f3554;
        font-size: 17px;
        font-weight: 800;
    }

    .reports-format-subtitle {
        color: #45627d;
        font-size: 12px;
        margin-bottom: 10px;
    }

    .reports-format-textarea {
        width: 100%;
        min-height: 172px;
        border: 1px solid #d8e3ee;
        border-radius: 8px;
        padding: 12px;
        color: #18324a;
        background: #ffffff;
        font-family: Consolas, 'Courier New', monospace;
        font-size: 12px;
        line-height: 1.45;
        resize: vertical;
    }

    .reports-format-actions {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
        align-items: center;
        margin-top: 10px;
    }

    .issue-row-actions {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
    }

    .issue-sno-cell {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 6px;
        width: 100%;
    }

    .milestone-text-only {
        display: inline-flex;
        align-items: center;
        max-width: 100%;
        min-height: 24px;
        padding: 3px 8px;
        border-radius: 7px;
        background: #eef5fb;
        color: #143a5a;
        font-size: 11px;
        font-weight: 800;
        line-height: 1.25;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .company-badge-full {
        font-size: 11px !important;
        padding: 3px 10px !important;
        height: auto !important;
        min-width: auto !important;
        line-height: 1.5 !important;
        border-radius: 999px !important;
    }

    .customer-detail-text {
        display: block;
        padding-bottom: 8px;
        font-size: 12px;
        color: #374151;
        line-height: 1.35;
        word-break: break-word;
        overflow-wrap: anywhere;
    }

    .processor-chip {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        font-size: 11px;
        color: #4b5563;
        background: #f3f4f6;
        border: 1px solid #e5e7eb;
        border-radius: 4px;
        padding: 3px 8px;
        word-break: break-word;
        overflow-wrap: anywhere;
        max-width: 100%;
    }

    .processor-chip::before {
        content: "⚙";
        font-size: 10px;
        opacity: 0.6;
        flex-shrink: 0;
    }

    .milestone-with-company {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        flex-wrap: wrap;
    }

    .company-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 28px;
        height: 18px;
        padding: 0 6px;
        border-radius: 999px;
        color: #fff;
        font-size: 10px;
        font-weight: 700;
        letter-spacing: 0.04em;
        line-height: 1;
        flex-shrink: 0;
        box-shadow: inset 0 -1px 0 rgba(255, 255, 255, 0.18);
    }

    .company-badge-tone-0 { background: #0068b5; }
    .company-badge-tone-1 { background: #0f7c67; }
    .company-badge-tone-2 { background: #8f5cc6; }
    .company-badge-tone-3 { background: #b05d2e; }
    .company-badge-tone-4 { background: #6b7280; }
    .company-badge-tone-5 { background: #d13438; }

    .milestone-label {
        white-space: normal;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    /* SightingID + Promoted ID inline label:value layout */
    .id-pair-stack {
        display: flex;
        flex-direction: column;
        gap: 10px;
    }

    .id-inline-row {
        display: flex;
        align-items: baseline;
        gap: 5px;
        flex-wrap: wrap;
    }

    .id-inline-label {
        font-size: 11px;
        font-weight: 600;
        color: #6b7280;
        white-space: nowrap;
        flex-shrink: 0;
    }

    .id-inline-link {
        font-size: 13px;
        font-weight: 600;
        color: #0068b5;
        text-decoration: none;
        word-break: break-all;
    }

    .id-inline-link:hover { text-decoration: underline; }

    .id-inline-value {
        font-size: 13px;
        color: #4b5563;
        word-break: break-all;
    }

    .table-primary .pager,
    .table-primary .pager table {
        display: none;
    }

    .table-primary .pager a,
    .table-primary .pager span {
        display: inline-block;
        min-width: 36px;
        text-align: center;
        padding: 6px 10px;
        margin: 0 3px;
        border: 1px solid #d4d4d4;
        border-radius: 6px;
        font-size: 12px;
        color: #2e2e2e;
        background: #fafafa;
        text-decoration: none;
    }

    .table-primary .pager span {
        border-color: #0068b5;
        color: #0068b5;
        background: #eff6fc;
        font-weight: 700;
    }

    .issue-page-status {
        margin-top: 8px;
        text-align: right;
        font-size: 12px;
        color: #616161;
    }

    .issue-pager-controls {
        display: grid;
        grid-template-columns: 1fr auto 1fr;
        justify-content: stretch;
        align-items: center;
        gap: 6px;
        margin-top: 10px;
        padding: 8px 10px;
        border: 1px solid #e1e8f0;
        border-radius: 10px;
        background: #ffffff;
        box-shadow: 0 8px 18px rgba(30, 58, 90, 0.06);
        position: relative;
    }

    .issue-pager-btn,
    .issue-pager-current {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 40px;
        min-height: 32px;
        padding: 0 12px;
        border: 1px solid #d0d0d0;
        border-radius: 6px;
        font-size: 12px;
        text-decoration: none;
        background: #ffffff;
        color: #2a2a2a;
    }

    .issue-page-nav {
        grid-column: 2;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 6px;
        flex-wrap: wrap;
    }

    .issue-pager-group-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 40px;
        min-height: 32px;
        padding: 0 10px;
        border: 1px solid #d0d0d0;
        border-radius: 6px;
        font-size: 14px;
        font-weight: bold;
        text-decoration: none;
        background: #f5f5f5;
        color: #666;
        cursor: pointer;
    }

    .issue-pager-group-btn:hover:not(.disabled) {
        background: #e0e0e0;
    }

    .issue-pager-current {
        background: #0068b5;
        border-color: #0068b5;
        color: #ffffff;
        font-weight: 700;
        min-width: 34px;
    }

    .issue-page-size-control {
        grid-column: 3;
        justify-self: end;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        color: #58677a;
        font-size: 11px;
        font-weight: 700;
        white-space: nowrap;
    }

    .issue-page-size-select {
        height: 30px;
        min-width: 62px;
        border: 1px solid #cfdae8;
        border-radius: 8px;
        background: #f8fbff;
        color: #1f3b57;
        font-size: 12px;
        font-weight: 700;
        padding: 0 6px;
    }

    .issue-pager-btn.disabled,
    .issue-pager-btn.aspNetDisabled,
    .issue-pager-group-btn.disabled,
    .issue-pager-group-btn.aspNetDisabled {
        opacity: 0.45;
        cursor: not-allowed;
        pointer-events: none;
    }

    .table-primary,
    #overall_request_details,
    #GridView_cmf_pending,
    #GridView_analytics_summary {
        width: 100%;
        border-collapse: collapse !important;
        border-spacing: 0;
        border: 1px solid #e0e0e0;
        border-radius: 6px;
        overflow: hidden;
        margin-top: 0 !important;
        margin-bottom: 0;
        background: #ffffff;
    }

    .table-primary th,
    #overall_request_details th,
    #GridView_cmf_pending th,
    #GridView_analytics_summary th {
        background: #fafafa !important;
        color: #424242 !important;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        font-size: 11px !important;
        font-weight: 600 !important;
        border: 1px solid #e0e0e0 !important;
        padding: 10px 10px !important;
        vertical-align: top;
        text-align: left !important;
    }

    .table-primary td,
    #overall_request_details td,
    #GridView_cmf_pending td,
    #GridView_analytics_summary td {
        border: 1px solid #f0f0f0 !important;
        padding: 10px !important;
        font-size: 12px;
        color: #242424;
        background: #ffffff;
    }

    /* Issue List gets more breathing room than the other grids */
    #overall_request_details td {
        padding: 16px !important;
        vertical-align: top !important;
    }

    #overall_request_details tr:hover td,
    #GridView_cmf_pending tr:hover td,
    #GridView_analytics_summary tr:hover td {
        background: #f8fbff;
    }

    /* Final visual layer: keep Issue List/Pending away from flat Excel styling. */

    #overall_request_details {
        width: 100% !important;

        min-width: 1050px !important;

        margin: 0 !important;

        border-collapse: separate !important;

        border-spacing: 0 !important;

        table-layout: auto !important;

        background: #ffffff !important;

        border: none !important;

        border-radius: 12px !important;

        box-shadow: none !important;

        font-size: 12px !important;
    }

    #overall_request_details th {
        background: #ffffff !important;
        color: #5d6f84 !important;
        border-top: 1px solid #e6edf5 !important;
        border-bottom: 1px solid #e6edf5 !important;
        border-left: 0 !important;
        border-right: 0 !important;
    }

    #overall_request_details td {
        background: linear-gradient(180deg, #ffffff 0%, #fbfdff 100%) !important;
        border-top: 1px solid #e4edf6 !important;
        border-bottom: 1px solid #e4edf6 !important;
        border-left: 0 !important;
        border-right: 0 !important;
        box-shadow: 0 10px 24px rgba(16, 41, 67, 0.07) !important;
        transition: transform 0.16s ease, box-shadow 0.16s ease, border-color 0.16s ease;
    }

    #overall_request_details tr td:first-child {
        border-left: 1px solid #e4edf6 !important;
        border-radius: 10px 0 0 10px !important;
    }

    #overall_request_details tr td:last-child {
        border-right: 1px solid #e4edf6 !important;
        border-radius: 0 10px 10px 0 !important;
    }

    #overall_request_details tr:hover td {
        background: linear-gradient(180deg, #ffffff 0%, #f4f9ff 100%) !important;
        transform: translateY(-2px);
        border-color: #cfdff0 !important;
        box-shadow: 0 16px 34px rgba(16, 41, 67, 0.12) !important;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending {
        min-width: 1260px !important;
        border-collapse: separate !important;
        border-spacing: 0 !important;
        border: 1px solid #e8edf5 !important;
        border-radius: 10px !important;
        overflow: hidden !important;
        background: #ffffff !important;
        box-shadow: 0 10px 24px rgba(28, 48, 78, 0.05) !important;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending th {
        background: #f7f9fc !important;
        color: #243042 !important;
        border: 0 !important;
        border-bottom: 1px solid #e5ebf3 !important;
        padding: 10px 12px !important;
        font-size: 10px !important;
        font-weight: 800 !important;
        letter-spacing: 0 !important;
        text-transform: none !important;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending td {
        background: #ffffff !important;
        border: 0 !important;
        border-bottom: 1px solid #eef2f7 !important;
        box-shadow: none !important;
        padding: 12px !important;
        color: #1f2937 !important;
        vertical-align: top !important;
    }

    .cmf-pending-grid-wrap #GridView_cmf_pending tr:hover td {
        background: #fbfdff !important;
    }

    .cmf-pending-grid-wrap .pending-chip {
        border-radius: 999px;
        background: #eef5fb;
        color: #214967;
        font-size: 10px;
        font-weight: 800;
        line-height: 1.2;
    }

    .cmf-pending-grid-wrap .ai-summary-btn {
        width: 100%;
        min-height: 32px;
        border-radius: 8px;
        font-size: 11px;
        font-weight: 800;
        white-space: normal;
    }

    .cmf-pending-grid-wrap .pending-recommendation-btn {
        width: auto;
        max-width: 100%;
        white-space: nowrap;
    }

    .header-dropdown {
        height: 28px;
        width: 126px;
        border-radius: 4px !important;
        border: 1px solid #d1d1d1 !important;
        font-size: 11px !important;
        padding: 2px 6px !important;
        background: #ffffff !important;
        color: #242424 !important;
    }

    #searchfilters {
        margin: 0 0 10px;
        padding: 8px;
        border: 1px solid #e0e0e0;
        border-radius: 6px;
        background: #fafafa;
    }

    #searchfilters input[type="text"] {
        height: 30px;
        border: 1px solid #d1d1d1;
        border-radius: 4px;
        padding: 0 8px;
        font-size: 12px;
    }
    .sighting-details-panel {
        display: flex;
        flex-direction: column;
        gap: 8px;
        width: 100%;
        padding: 8px 0;
        min-width: 0;
    }

    .sighting-id-section {
        display: flex;
        align-items: center;
        gap: 6px;
        min-width: 0;
    }

    .sighting-id-label {
        font-size: 11px;
        font-weight: 700;
        color: #475569;
        white-space: nowrap;
    }

    .sighting-id-value {
        font-size: 12px;
        font-weight: 600;
        color: #0f5ea8;
        word-break: break-word;
        text-decoration: none;
    }

    .sighting-id-value:hover {
        text-decoration: underline;
    }

    .sighting-description-section {
        display: flex;
        flex-direction: column;
        gap: 4px;
        min-width: 0;
    }

    .sighting-description-label {
        font-size: 11px;
        font-weight: 700;
        color: #475569;
    }

    .sighting-description-content {
        font-size: 12px;
        line-height: 1.4;
        color: #333;
        min-height: 24px;
        word-wrap: break-word;
        white-space: normal;
        padding: 4px;
        border-radius: 4px;
        background: #f8fafc;
    }

    .sighting-description-loading {
        font-size: 11px;
        color: #999;
        font-style: italic;
    }

    .sighting-description-error {
        font-size: 11px;
        color: #dc2626;
        font-style: italic;
    }

    .sighting-data-container {
        display: none;
    }

    .cmf-assistant-sighting-card {
        border: 0;
        border-radius: 12px;
        background: transparent;
        padding: 0;
        margin: 4px 0 0;
        display: grid;
        grid-template-columns: 1fr;
        gap: 10px;
    }

    .cmf-assistant-sighting-head {
        display: none;
        align-items: flex-start;
        justify-content: space-between;
        gap: 12px;
        grid-column: 1 / -1;
        padding: 10px 12px;
        border: 1px solid rgba(194, 213, 239, 0.75);
        border-radius: 10px;
        background: linear-gradient(90deg, rgba(229, 241, 255, 0.88), rgba(236, 253, 245, 0.55));
    }

    .cmf-assistant-sighting-head i {
        color: #1d67c1;
        font-size: 18px;
        margin-top: 2px;
    }

    .cmf-assistant-eyebrow {
        font-size: 10px;
        font-weight: 900;
        color: #5d7290;
        text-transform: uppercase;
        letter-spacing: 0.08em;
    }

    .cmf-assistant-sighting-id {
        margin-top: 2px;
        font-size: 13px;
        font-weight: 900;
        color: #315f93;
    }

    .cmf-assistant-description {
        color: #28425c;
        font-size: 11px;
        line-height: 1.35;
        background: #ffffff;
        border: 1px solid #e1edf7;
        border-radius: 8px;
        padding: 6px 8px;
        min-height: 0;
        text-align: left;
    }

    .cmf-assistant-description p {
        margin: 0;
    }

    .cmf-assistant-description ul {
        margin: 6px 0 0;
        padding-left: 18px;
    }

    .cmf-assistant-description li {
        margin: 3px 0;
        padding-left: 2px;
        text-align: left;
    }

    .cmf-assistant-text-section {
        border: 1px solid #d8e7f5;
        background: #ffffff;
        border-radius: 8px;
        padding: 8px 9px;
        box-shadow: 0 6px 16px rgba(30, 84, 144, 0.05);
    }

    .cmf-assistant-description-section {
        border-color: #b9d8ff;
        background: linear-gradient(180deg, #f7fbff 0%, #ffffff 100%);
    }

    .cmf-assistant-impact-section {
        border-color: #fed7aa;
        background: #ffffff;
    }

    .cmf-assistant-text-section h3 {
        margin: 0 0 5px;
        color: #102a43;
        font-size: 11px;
        font-weight: 900;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .cmf-assistant-text-section h3::before {
        content: '\f0eb';
        font-family: 'Font Awesome 5 Free';
        font-weight: 900;
        width: 18px;
        height: 18px;
        border-radius: 6px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: #edf5ff;
        color: #246bfe;
        font-size: 9px;
        box-shadow: none;
    }

    .cmf-assistant-impact-section h3::before {
        content: '\f071';
        background: #fff3e0;
        color: #f97316;
    }

    .cmf-assistant-metrics-section h3::before {
        content: '\f201';
        background: #ecfdf5;
        color: #16a34a;
    }

    .cmf-assistant-text-section .cmf-assistant-description,
    .cmf-assistant-text-section .cmf-assistant-impact-block {
        border: 0;
        border-radius: 0;
        padding: 0;
        min-height: 0;
        background: transparent;
    }

    .cmf-assistant-recommendation-section {
        border-color: #c7d2fe;
        background: #ffffff;
    }

    .cmf-assistant-recommendation-section h3::before {
        content: '\f0ae';
        background: #eef2ff;
        color: #4f46e5;
    }

    .cmf-assistant-rec-shell {
        display: grid;
        gap: 8px;
    }

    .cmf-assistant-rec-top {
        display: grid;
        grid-template-columns: minmax(132px, 0.32fr) minmax(0, 1fr);
        gap: 8px;
        align-items: stretch;
    }

    .cmf-assistant-overall-score {
        border: 1px solid #d8e2ee;
        border-radius: 8px;
        padding: 10px 8px;
        background: linear-gradient(180deg, #f8fbff, #ffffff);
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 7px;
        min-height: 112px;
        overflow: hidden;
    }

    .cmf-assistant-score-ring {
        width: 72px;
        height: 72px;
        border-radius: 50%;
        display: grid;
        place-items: center;
        background: conic-gradient(#0f7cc2 calc(var(--score-percent, 0) * 1%), #e8eef6 0);
        box-shadow: inset 0 0 0 1px #d6e2ef;
    }

    .cmf-assistant-score-ring::before {
        content: '';
        position: absolute;
    }

    .cmf-assistant-score-ring-value {
        width: 54px;
        height: 54px;
        border-radius: 50%;
        background: #fff;
        display: grid;
        place-items: center;
        color: #12385f;
        font-size: 16px;
        font-weight: 950;
        line-height: 1;
    }

    .cmf-assistant-overall-score span,
    .cmf-assistant-metric-label {
        color: #64748b;
        font-size: 9px;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0;
    }

    .cmf-assistant-overall-score strong {
        color: #12385f;
        font-size: 13px;
        font-weight: 950;
        line-height: 1;
    }

    .cmf-assistant-overall-score small {
        color: #66788d;
        font-size: 10px;
        font-weight: 750;
        line-height: 1.25;
    }

    .cmf-assistant-metric-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 7px;
    }

    .cmf-assistant-metric-chip {
        border: 1px solid #d8e2ee;
        border-radius: 8px;
        padding: 7px 8px;
        background: #ffffff;
        min-width: 0;
    }

    .cmf-assistant-metric-chip.is-high {
        border-color: #d8e2ee;
        background: #ffffff;
    }

    .cmf-assistant-metric-chip.is-medium {
        border-color: #d8e2ee;
        background: #ffffff;
    }

    .cmf-assistant-metric-chip.is-low {
        border-color: #d8e2ee;
        background: #ffffff;
    }

    .cmf-assistant-metric-chip.is-empty {
        border-color: #d8e2ee;
        background: #ffffff;
    }

    .cmf-assistant-metric-main {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 6px;
        margin-bottom: 3px;
    }

    .cmf-assistant-metric-name {
        color: #172033;
        font-size: 11px;
        font-weight: 900;
        min-width: 0;
        overflow: visible;
        text-overflow: clip;
        white-space: normal;
        overflow-wrap: anywhere;
        line-height: 1.2;
    }

    .cmf-assistant-metric-value {
        color: #0f355c;
        font-size: 12px;
        font-weight: 950;
        white-space: nowrap;
    }

    .cmf-assistant-metric-factor {
        color: #52677f;
        font-size: 10.5px;
        font-weight: 650;
        line-height: 1.25;
        overflow-wrap: anywhere;
    }

    .cmf-assistant-reasoning-box {
        border: 1px solid #d8e2ee;
        border-radius: 8px;
        padding: 9px 10px;
        background: #fbfdff;
    }

    .cmf-assistant-reasoning-box h4 {
        margin: 0 0 5px;
        color: #172033;
        font-size: 11px;
        font-weight: 900;
    }

    .cmf-assistant-reasoning-box ul {
        margin: 0;
        padding-left: 16px;
        color: #40566e;
        font-size: 11px;
        line-height: 1.35;
    }

    .cmf-assistant-reasoning-copy ul {
        margin: 0;
        padding-left: 16px;
        color: #40566e;
        font-size: 11px;
        line-height: 1.4;
        text-align: left;
    }

    .cmf-assistant-reasoning-copy li {
        margin: 0 0 5px;
        color: #40566e;
        font-size: 11px;
        line-height: 1.4;
    }

    .cmf-assistant-reasoning-copy li:last-child {
        margin-bottom: 0;
    }

    .cmf-assistant-rec-tag {
        margin-left: 8px;
        padding: 5px 10px;
        border-radius: 8px;
        border: 1px solid #cbd5e1;
        background: #f8fafc;
        color: #1f2937;
        font-size: 11px;
        font-weight: 950;
        letter-spacing: 0;
        white-space: nowrap;
    }

    .cmf-assistant-rec-tag.is-ok {
        border-color: #86efac;
        background: #dcfce7;
        color: #166534;
    }

    .cmf-assistant-rec-tag.is-review {
        border-color: #fde68a;
        background: #fef3c7;
        color: #92400e;
    }

    .cmf-assistant-rec-tag.is-incomplete {
        border-color: #bfdbfe;
        background: #dbeafe;
        color: #1e40af;
    }

    .cmf-assistant-rec-tag.is-reject {
        border-color: #fecaca;
        background: #fee2e2;
        color: #991b1b;
    }

    .cmf-assistant-meta-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 8px;
    }

    .cmf-assistant-meta-grid div {
        border: 1px solid #d8e7f5;
        background: linear-gradient(135deg, #ffffff, #f5fbff);
        border-radius: 9px;
        padding: 7px 10px;
        min-width: 0;
        box-shadow: 0 8px 20px rgba(30, 84, 144, 0.05);
        display: flex;
        align-items: baseline;
        gap: 8px;
    }

    .cmf-assistant-meta-grid span {
        display: inline-block;
        color: #64748b;
        font-size: 10px;
        font-weight: 800;
        text-transform: uppercase;
        white-space: nowrap;
    }

    .cmf-assistant-meta-grid strong {
        display: inline-block;
        color: #132f4c;
        font-size: 12px;
        margin-top: 0;
        white-space: normal;
        overflow-wrap: anywhere;
        min-width: 0;
    }

    .cmf-assistant-impact-block {
        border: 1px solid #d8e7f5;
        background: #ffffff;
        border-radius: 8px;
        padding: 10px 12px;
    }

    .cmf-assistant-impact-block span {
        display: block;
        color: #64748b;
        font-size: 10px;
        font-weight: 800;
        text-transform: uppercase;
    }

    .cmf-assistant-impact-block strong {
        display: inline;
        color: #28425c;
        font-size: 13px;
        font-weight: 800;
        line-height: 1.45;
        white-space: normal;
    }

    .cmf-assistant-impact-summary {
        color: #2f3640;
        font-size: 11px;
        font-weight: 400;
        line-height: 1.35;
        margin-top: 0;
        white-space: normal;
        text-align: justify;
    }

    .cmf-assistant-impact-summary ul {
        margin: 0;
        padding-left: 18px;
    }

    .cmf-assistant-impact-summary li {
        margin: 0 0 7px;
        text-align: justify;
    }

    .cmf-assistant-fix-line {
        margin-top: 8px;
        text-align: justify;
    }

    .cmf-assistant-description em,
    .cmf-assistant-impact-summary em {
        font-style: italic;
    }

    .cmf-assistant-description u,
    .cmf-assistant-impact-summary u {
        text-decoration-thickness: 1.5px;
        text-underline-offset: 2px;
    }

    .cmf-impact-skeleton {
        gap: 9px;
        padding: 2px 0;
    }

    @media (max-width: 700px) {
        #cmfRecDrawer {
            top: 10px !important;
            right: 10px !important;
            width: calc(100vw - 20px) !important;
            max-height: calc(100vh - 20px) !important;
        }

        .cmf-assistant-sighting-card {
            grid-template-columns: 1fr;
        }

        .cmf-assistant-meta-grid {
            grid-template-columns: 1fr;
        }

        .cmf-assistant-rec-top,
        .cmf-assistant-metric-grid {
            grid-template-columns: 1fr;
        }
    }

    .cmf-assistant-feature-card {
        border: 1px solid #d8e7f5;
        border-radius: 12px;
        background: #fff;
        padding: 14px 16px;
        margin-bottom: 14px;
    }

    .cmf-assistant-quality-slot .cmf-assistant-feature-card {
        margin-top: 12px;
    }

    .cmf-quality-assessment-card {
        border: 1px solid #b9d8ff;
        border-radius: 8px;
        padding: 8px 9px;
        margin-bottom: 12px;
        box-shadow: 0 6px 16px rgba(30, 84, 144, 0.05);
        background: #ffffff;
    }

    .cmf-quality-assessment-card.is-quality-high {
        border-color: #bbf7d0;
    }

    .cmf-quality-assessment-card.is-quality-medium {
        border-color: #fed7aa;
    }

    .cmf-quality-assessment-card.is-quality-low {
        border-color: #fecaca;
    }

    .cmf-quality-assessment-card .cmf-assistant-feature-head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        margin-bottom: 8px;
    }

    .cmf-quality-assessment-card .cmf-assistant-feature-head h3 {
        margin: 0;
        font-size: 11px;
        line-height: 1.25;
        letter-spacing: 0;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .cmf-quality-assessment-card .cmf-assistant-feature-head h3::before {
        content: '\f46c';
        font-family: 'Font Awesome 5 Free';
        font-weight: 900;
        width: 18px;
        height: 18px;
        border-radius: 6px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: #edf5ff;
        color: #246bfe;
        font-size: 9px;
    }

    .cmf-sqa-category-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 10px;
        align-items: stretch;
    }

    .cmf-sqa-score-pill {
        border: 2px solid #19324a;
        border-radius: 8px;
        background: #ffffff;
        color: #102a43;
        padding: 6px 12px;
        font-size: 12px;
        font-weight: 950;
        white-space: nowrap;
    }

    .cmf-sqa-score-pill.is-quality-high { border-color: #16a34a; color: #166534; }
    .cmf-sqa-score-pill.is-quality-medium { border-color: #f59e0b; color: #92400e; }
    .cmf-sqa-score-pill.is-quality-low { border-color: #dc2626; color: #991b1b; }

    .cmf-sqa-category-card {
        border: 2px solid #d8e7f5;
        border-radius: 14px;
        background: #ffffff;
        min-height: 116px;
        padding: 10px 11px;
        min-width: 0;
    }

    .cmf-sqa-category-card.missing { border-color: #fecaca; }
    .cmf-sqa-category-card.partial { border-color: #fed7aa; }
    .cmf-sqa-category-card.conflicting { border-color: #bfdbfe; }

    .cmf-sqa-category-title {
        margin-bottom: 8px;
        color: #102a43;
        font-size: 12px;
        font-weight: 900;
    }

    .cmf-sqa-category-list {
        display: grid;
        gap: 6px;
        margin: 0;
        padding: 0;
        list-style: none;
    }

    .cmf-sqa-category-list li {
        color: #304761;
        font-size: 11px;
        line-height: 1.35;
        overflow-wrap: anywhere;
    }

    .cmf-sqa-category-list li strong,
    .cmf-sqa-category-list li span,
    .cmf-sqa-category-list li small {
        display: block;
    }

    .cmf-sqa-category-list li strong {
        color: #102a43;
        font-size: 11px;
        font-weight: 950;
        margin-bottom: 2px;
    }

    .cmf-sqa-category-list li span {
        color: #304761;
        font-weight: 800;
    }

    .cmf-sqa-category-list li small {
        color: #64748b;
        font-size: 10px;
        line-height: 1.35;
        margin-top: 2px;
    }

    .cmf-sqa-category-list li.is-empty {
        color: #64748b;
        font-style: italic;
    }

    .cmf-sqa-skeleton {
        border: 1px solid #d8e7f5;
        border-radius: 8px;
        background: #ffffff;
        padding: 10px 11px;
        margin: 12px 0;
    }

    .cmf-sqa-skeleton-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 10px;
        margin-top: 10px;
    }

    .cmf-sqa-skeleton-box {
        border: 1px solid #e1edf7;
        border-radius: 8px;
        padding: 10px;
        min-height: 72px;
    }

    .cmf-quality-score-panel,
    .cmf-quality-fields-panel {
        border: 1px solid #e1edf7;
        border-radius: 8px;
        background: #f8fbff;
        padding: 9px 10px;
        min-width: 0;
    }

    .cmf-quality-score-panel.is-quality-high {
        border-color: #bbf7d0;
        background: #f0fdf4;
    }

    .cmf-quality-score-panel.is-quality-medium {
        border-color: #fed7aa;
        background: #fff7ed;
    }

    .cmf-quality-score-panel.is-quality-low {
        border-color: #fecaca;
        background: #fff1f2;
    }

    .cmf-quality-score-value {
        color: #0f4f83;
        font-size: 26px;
        line-height: 1;
        font-weight: 950;
    }

    .cmf-quality-score-panel.is-quality-high .cmf-quality-score-value { color: #166534; }
    .cmf-quality-score-panel.is-quality-medium .cmf-quality-score-value { color: #b45309; }
    .cmf-quality-score-panel.is-quality-low .cmf-quality-score-value { color: #b91c1c; }

    .cmf-quality-score-value span {
        color: #64748b;
        font-size: 12px;
        font-weight: 850;
    }

    .cmf-quality-panel-label {
        margin-bottom: 6px;
        color: #64748b;
        font-size: 10px;
        font-weight: 900;
        text-transform: uppercase;
    }

    .cmf-quality-field-list {
        display: grid;
        gap: 5px;
        margin: 0;
        padding: 0;
        list-style: none;
    }

    .cmf-quality-field-list li {
        color: #304761;
        font-size: 11px;
        line-height: 1.35;
        overflow-wrap: anywhere;
    }

    .cmf-quality-action-row {
        display: flex;
        justify-content: flex-end;
        margin-top: 8px;
    }

    .cmf-quality-details-btn,
    .cmf-quality-back-btn,
    .cmf-quality-refresh-btn {
        border: 1px solid #b9d8ff;
        border-radius: 7px;
        background: #ffffff;
        color: #0f5ea8;
        font-size: 11px;
        font-weight: 900;
        padding: 6px 10px;
        cursor: pointer;
    }

    .cmf-quality-details-btn:hover,
    .cmf-quality-back-btn:hover,
    .cmf-quality-refresh-btn:hover {
        background: #edf5ff;
    }

    .cmf-quality-refresh-btn {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        border-color: #246bfe;
        background: #246bfe;
        color: #ffffff;
        box-shadow: 0 8px 18px rgba(36, 107, 254, 0.18);
        white-space: nowrap;
    }

    .cmf-quality-refresh-btn:hover {
        background: #1857d7;
        color: #ffffff;
    }

    .cmf-quality-detail-shell {
        display: grid;
        gap: 12px;
    }

    .cmf-quality-detail-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 10px;
    }

    .cmf-quality-detail-actions {
        display: inline-flex;
        align-items: center;
        gap: 8px;
    }

    .cmf-quality-detail-header h3 {
        margin: 0;
        color: #102a43;
        font-size: 16px;
        font-weight: 900;
    }

    .cmf-quality-detail-card {
        border: 1px solid #d8e7f5;
        border-radius: 8px;
        background: #ffffff;
        padding: 11px 12px;
    }

    .cmf-quality-detail-card h4 {
        margin: 0 0 8px;
        color: #19324a;
        font-size: 12px;
        font-weight: 900;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .cmf-sqa-card-icon,
    .cmf-sqa-sparkle {
        width: 22px;
        height: 22px;
        border-radius: 7px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: #eaf3ff;
        color: #246bfe;
        font-family: 'Font Awesome 5 Free';
        font-weight: 900;
        font-size: 11px;
        flex: 0 0 auto;
    }

    .cmf-sqa-sparkle {
        font-family: inherit;
        background: #fff7d6;
        color: #d97706;
        font-size: 14px;
    }

    .cmf-sqa-info-grid {
        display: grid;
        grid-template-columns: repeat(5, minmax(0, 1fr));
        border: 1px solid #e1edf7;
        border-radius: 8px;
        overflow: hidden;
        background: #f8fbff;
    }

    .cmf-sqa-info-cell {
        display: grid;
        gap: 4px;
        padding: 9px 10px;
        border-right: 1px solid #e1edf7;
        border-bottom: 1px solid #e1edf7;
        min-width: 0;
    }

    .cmf-sqa-info-cell.is-wide {
        grid-column: span 2;
    }

    .cmf-sqa-info-cell span {
        color: #4b6077;
        font-size: 10px;
        font-weight: 900;
    }

    .cmf-sqa-info-cell strong {
        color: #102a43;
        font-size: 12px;
        line-height: 1.35;
        overflow-wrap: anywhere;
    }

    .cmf-sqa-overview-template {
        display: grid;
        gap: 10px;
    }

    .cmf-sqa-overview-grid {
        display: grid;
        grid-template-columns: minmax(0, 1.4fr) minmax(190px, .75fr);
        gap: 16px;
        align-items: center;
    }

    .cmf-sqa-score-block {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 18px;
        min-width: 0;
    }

    .cmf-sqa-score-message {
        display: grid;
        gap: 5px;
        border-radius: 8px;
        padding: 14px 16px;
        background: #ecfdf5;
        color: #14532d;
        max-width: 230px;
    }

    .cmf-sqa-score-message.is-quality-medium { background: #fff7ed; color: #92400e; }
    .cmf-sqa-score-message.is-quality-low { background: #fff1f2; color: #991b1b; }

    .cmf-sqa-score-message strong {
        font-size: 16px;
        font-weight: 950;
    }

    .cmf-sqa-score-message span {
        font-size: 12px;
        line-height: 1.4;
        font-weight: 750;
    }

    .cmf-sqa-status-summary {
        display: grid;
        gap: 8px;
    }

    .cmf-sqa-status-summary-row {
        display: grid;
        grid-template-columns: 20px 1fr auto;
        align-items: center;
        gap: 8px;
        color: #304761;
        font-size: 12px;
        font-weight: 900;
    }

    .cmf-sqa-status-summary-row strong {
        color: #102a43;
        font-weight: 950;
    }

    .cmf-sqa-status-dot {
        width: 18px;
        height: 18px;
        border-radius: 50%;
        display: inline-flex;
        background: #22c55e;
    }

    .cmf-sqa-status-summary-row.partial .cmf-sqa-status-dot { background: #f59e0b; }
    .cmf-sqa-status-summary-row.missing .cmf-sqa-status-dot { background: #ef4444; }
    .cmf-sqa-status-summary-row.conflicting .cmf-sqa-status-dot { background: #8b5cf6; }

    .cmf-sqa-evidence-table {
        display: grid;
        border: 1px solid #e1edf7;
        border-radius: 8px;
        overflow: hidden;
    }

    .cmf-sqa-evidence-row {
        display: grid;
        grid-template-columns: 44px minmax(145px, .9fr) minmax(210px, 1.4fr) minmax(112px, .55fr);
        align-items: center;
        min-width: 0;
        border-bottom: 1px solid #edf2f7;
        background: #ffffff;
    }

    .cmf-sqa-evidence-row > div {
        padding: 9px 10px;
        color: #24364a;
        font-size: 11px;
        line-height: 1.35;
        overflow-wrap: anywhere;
        min-width: 0;
    }

    .cmf-sqa-evidence-row.is-header {
        background: #edf5ff;
    }

    .cmf-sqa-evidence-row.is-header > div {
        color: #183b66;
        font-size: 10px;
        font-weight: 950;
        text-transform: uppercase;
    }

    .cmf-sqa-evidence-row:last-child {
        border-bottom: 0;
    }

    .cmf-sqa-assessment-summary p {
        margin: 0;
        color: #304761;
        font-size: 12px;
        line-height: 1.55;
    }

    .cmf-quality-overview-card {
        display: grid;
        gap: 14px;
    }

    .cmf-quality-score-topline {
        display: grid;
        grid-template-columns: 1fr auto 1fr;
        align-items: center;
        gap: 14px;
        min-width: 0;
    }

    .cmf-quality-score-chart-wrap {
        display: grid;
        gap: 8px;
        justify-items: center;
        grid-column: 2;
    }

    .cmf-quality-score-topline .cmf-quality-refresh-btn {
        justify-self: end;
        grid-column: 3;
    }

    .cmf-quality-donut {
        width: 132px;
        height: 132px;
        border-radius: 50%;
        display: grid;
        place-items: center;
        background: conic-gradient(#22a65a 0 calc(var(--score) * 1%), #e8eef5 calc(var(--score) * 1%) 100%);
        position: relative;
        box-shadow: inset 0 0 0 1px rgba(16, 42, 67, 0.08);
    }

    .cmf-quality-donut::after {
        content: '';
        position: absolute;
        width: 76px;
        height: 76px;
        border-radius: 50%;
        background: #ffffff;
    }

    .cmf-quality-donut-value {
        position: relative;
        z-index: 1;
        color: #102a43;
        font-size: 24px;
        font-weight: 950;
    }

    .cmf-quality-score-list {
        display: grid;
        gap: 9px;
    }

    .cmf-quality-score-row {
        display: grid;
        grid-template-columns: minmax(220px, 1.4fr) minmax(90px, 0.45fr) minmax(130px, 0.65fr);
        align-items: center;
        gap: 12px;
        padding: 8px 0;
        border-bottom: 1px solid #edf2f7;
        min-width: 0;
    }

    .cmf-quality-score-row.is-header {
        padding-top: 0;
        color: #64748b;
        font-size: 10px;
        font-weight: 950;
        letter-spacing: 0;
        text-transform: uppercase;
    }

    .cmf-quality-score-row:last-child {
        border-bottom: 0;
    }

    .cmf-quality-score-name {
        color: #24364a;
        font-size: 12px;
        font-weight: 900;
        line-height: 1.35;
        overflow-wrap: anywhere;
        white-space: normal;
    }

    .cmf-quality-score-status {
        color: #4b6077;
        font-size: 12px;
        line-height: 1.3;
        overflow-wrap: anywhere;
        white-space: normal;
    }

    .cmf-quality-score-field-score {
        color: #102a43;
        font-size: 12px;
        font-weight: 950;
        line-height: 1.3;
    }

    @media (max-width: 640px) {
        .cmf-quality-score-topline {
            align-items: flex-start;
            grid-template-columns: 1fr auto;
        }

        .cmf-quality-score-chart-wrap {
            grid-column: 1 / -1;
            justify-self: center;
        }

        .cmf-quality-score-topline .cmf-quality-refresh-btn {
            grid-column: 2;
            grid-row: 1;
        }

        .cmf-quality-score-row {
            grid-template-columns: minmax(130px, 1fr) minmax(64px, auto) minmax(92px, auto);
            gap: 4px;
        }

        .cmf-quality-score-status,
        .cmf-quality-score-name {
            font-size: 11px;
        }
    }

    .cmf-quality-suggestions {
        margin: 0;
        padding-left: 18px;
        color: #304761;
        font-size: 12px;
        line-height: 1.45;
    }

    .cmf-quality-score-notes {
        display: grid;
        gap: 7px;
        margin: 0;
        padding: 0;
        list-style: none;
    }

    .cmf-quality-score-notes li {
        color: #304761;
        font-size: 12px;
        line-height: 1.42;
    }

    .cmf-quality-detail-list {
        display: grid;
        gap: 10px;
    }

    .cmf-quality-section-subtitle {
        margin: -2px 0 9px;
        color: #5b6f84;
        font-size: 11px;
        line-height: 1.4;
    }

    .cmf-quality-field-card {
        border: 1px solid #e1edf7;
        border-radius: 8px;
        background: #f8fbff;
        padding: 10px 11px;
    }

    .cmf-quality-field-card.present { border-color: #bbf7d0; background: #f0fdf4; }
    .cmf-quality-field-card.partial { border-color: #fed7aa; background: #fff7ed; }
    .cmf-quality-field-card.missing { border-color: #fecaca; background: #fff1f2; }
    .cmf-quality-field-card.conflicting { border-color: #fecaca; background: #fff1f2; }

    .cmf-quality-field-card-head {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 10px;
        margin-bottom: 7px;
    }

    .cmf-quality-field-card-title {
        color: #102a43;
        font-size: 12px;
        font-weight: 900;
        line-height: 1.35;
    }

    .cmf-quality-field-card p {
        margin: 5px 0 0;
        color: #304761;
        font-size: 11px;
        line-height: 1.45;
        text-align: justify;
    }

    .cmf-quality-field-card strong {
        color: #143f6b;
        font-weight: 900;
    }

    .cmf-assistant-feature-head {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 12px;
        margin-bottom: 10px;
    }

    .cmf-assistant-feature-head h3 {
        margin: 3px 0 0;
        color: #102a43;
        font-size: 16px;
        font-weight: 900;
    }

    .cmf-quality-tier {
        border-radius: 999px;
        padding: 7px 10px;
        font-size: 12px;
        font-weight: 900;
        white-space: nowrap;
        display: inline-flex;
        align-items: center;
        gap: 8px;
    }

    .cmf-quality-tier.high { background: #dcfce7; color: #166534; }
    .cmf-quality-tier.medium { background: #fef3c7; color: #92400e; }
    .cmf-quality-tier.low { background: #fee2e2; color: #991b1b; }

    .cmf-quality-tier span {
        opacity: 0.85;
        font-weight: 800;
    }

    .cmf-quality-meter {
        height: 8px;
        border-radius: 999px;
        background: #e5edf5;
        overflow: hidden;
        margin-bottom: 12px;
    }

    .cmf-quality-meter span {
        display: block;
        height: 100%;
        border-radius: 999px;
        background: linear-gradient(90deg, #16a34a 0%, #0f72c4 100%);
    }

    .cmf-quality-meter.is-quality-high span { background: #16a34a; }
    .cmf-quality-meter.is-quality-medium span { background: #f59e0b; }
    .cmf-quality-meter.is-quality-low span { background: #dc2626; }

    .cmf-quality-status-badge {
        display: inline-flex;
        align-items: center;
        border-radius: 999px;
        padding: 3px 8px;
        font-size: 10px;
        font-weight: 900;
        white-space: nowrap;
    }

    .cmf-quality-status-badge.present { background: #dcfce7; color: #166534; }
    .cmf-quality-status-badge.partial { background: #fef3c7; color: #92400e; }
    .cmf-quality-status-badge.missing { background: #fee2e2; color: #991b1b; }

    .cmf-quality-two-col {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 10px;
    }

    .cmf-quality-two-col > div {
        border: 1px solid #e1edf7;
        border-radius: 8px;
        background: #f8fbff;
        padding: 10px;
    }

    .cmf-quality-two-col h4 {
        margin: 0 0 8px;
        font-size: 12px;
        font-weight: 900;
        color: #19324a;
    }

    .cmf-quality-note {
        margin-top: 10px;
        color: #5d7088;
        font-size: 12px;
        font-weight: 700;
    }

    @media (max-width: 700px) {
        .cmf-quality-two-col {
            grid-template-columns: 1fr;
        }

        .cmf-sqa-category-grid {
            grid-template-columns: 1fr;
        }
    }

    .cmf-rec-assessment-shell {
        display: grid;
        gap: 14px;
        max-width: 100%;
        min-width: 0;
        overflow-x: hidden;
    }

    .cmf-rec-summary-band {
        display: grid;
        grid-template-columns: minmax(0, 1fr) auto;
        gap: 12px;
        align-items: center;
        padding: 14px 16px;
        border: 1px solid #d8e7f5;
        border-radius: 12px;
        background: linear-gradient(135deg, #f8fbff 0%, #eef8ff 100%);
        min-width: 0;
    }

    .cmf-rec-summary-title {
        color: #102a43;
        font-size: 13px;
        font-weight: 900;
        margin-bottom: 4px;
    }

    .cmf-rec-summary-copy {
        color: #52677f;
        font-size: 12px;
        line-height: 1.45;
        overflow-wrap: anywhere;
    }

    .cmf-rec-score-panel {
        min-width: 230px;
        padding: 12px 14px;
        border: 1px solid rgba(37, 99, 235, 0.18);
        border-radius: 12px;
        background: #ffffff;
        box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06);
    }

    .cmf-rec-score-top {
        display: flex;
        align-items: baseline;
        justify-content: space-between;
        gap: 12px;
        margin-bottom: 8px;
    }

    .cmf-rec-score-label {
        color: #52677c;
        font-size: 11px;
        font-weight: 850;
        text-transform: uppercase;
        letter-spacing: 0;
    }

    .cmf-rec-score-value {
        color: #143f6b;
        font-size: 18px;
        font-weight: 900;
        white-space: nowrap;
    }

    .cmf-rec-score-track {
        position: relative;
        height: 10px;
        overflow: hidden;
        border-radius: 999px;
        background: #e7edf5;
    }

    .cmf-rec-score-fill {
        width: var(--score-percent, 0%);
        height: 100%;
        border-radius: inherit;
        background: linear-gradient(90deg, #0ea5e9, #2563eb 55%, #16a34a);
    }

    .cmf-rec-score-caption {
        margin-top: 7px;
        color: #60758a;
        font-size: 11px;
        font-weight: 700;
        line-height: 1.35;
    }

    .cmf-rec-stage-grid {
        display: grid;
        grid-template-columns: minmax(0, 1fr);
        gap: 8px;
    }

    .cmf-rec-stage-card,
    .cmf-rec-dimension-card {
        min-width: 0;
        border: 1px solid #e0e8f2;
        border-radius: 10px;
        background: #ffffff;
        padding: 10px 12px;
        box-shadow: 0 8px 18px rgba(15, 35, 55, 0.04);
    }

    .cmf-rec-stage-card h4,
    .cmf-rec-dimension-card h4 {
        margin: 0 0 6px;
        color: #172033;
        font-size: 12px;
        font-weight: 900;
    }

    .cmf-rec-stage-card ul,
    .cmf-rec-dimension-card ul {
        margin: 0;
        padding-left: 17px;
        color: #4b5f75;
        font-size: 11px;
        line-height: 1.38;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .cmf-rec-stage-card li {
        margin-bottom: 4px;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .cmf-rec-dimension-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 8px;
    }

    .cmf-rec-dimension-card {
        padding: 10px;
    }

    .cmf-rec-dimension-head {
        display: flex;
        justify-content: space-between;
        gap: 8px;
        align-items: center;
        margin-bottom: 6px;
    }

    .cmf-rec-dimension-name {
        color: #172033;
        font-size: 11px;
        font-weight: 900;
    }

    .cmf-rec-dimension-score {
        color: #075ea8;
        font-size: 12px;
        font-weight: 900;
        white-space: nowrap;
    }

    .cmf-rec-dimension-eval {
        color: #5c7087;
        font-size: 11px;
        line-height: 1.35;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    .cmf-rec-metric-table-wrap {
        width: 100%;
        overflow-x: auto;
        border: 1px solid #dce6f1;
        border-radius: 8px;
    }

    .cmf-rec-metric-table {
        width: 100%;
        min-width: 560px;
        border-collapse: collapse;
        background: #ffffff;
        font-size: 11px;
        line-height: 1.35;
    }

    .cmf-rec-metric-table th,
    .cmf-rec-metric-table td {
        padding: 8px 10px;
        border-bottom: 1px solid #e7eef6;
        color: #40566e;
        text-align: left;
        vertical-align: top;
    }

    .cmf-rec-metric-table th {
        background: #f5f8fc;
        color: #172033;
        font-size: 10px;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0;
    }

    .cmf-rec-metric-table tr:last-child td {
        border-bottom: 0;
    }

    .cmf-rec-metric-score {
        display: inline-flex;
        min-width: 38px;
        justify-content: center;
        padding: 2px 6px;
        border-radius: 999px;
        background: #eaf4ff;
        color: #075ea8;
        font-weight: 900;
        white-space: nowrap;
    }

    .cmf-rec-missing-list li.is-empty {
        color: #0f8f61;
        font-weight: 800;
    }

    @media (max-width: 900px) {
        #updateinfo {
            display: none;
        }

        .button-container.button-container-focus {
            margin-top: 8px;
        }

        .content-wrapper {
            margin: 10px;
            padding: 10px;
        }

        .issue-kpi-row {
            grid-template-columns: repeat(2, minmax(120px, 1fr));
        }

        .portal-layout-shell {
            flex-direction: column;
            min-height: 0;
        }

        .portal-left-nav {
            width: 100%;
            flex: 1 1 auto;
            position: static;
            max-height: none;
            min-height: 0;
        }

        .issue-page-hd-top {
            align-items: flex-start;
        }
        .cmf-rec-stage-grid,
        .cmf-rec-dimension-grid,
        .cmf-rec-summary-band {
            grid-template-columns: 1fr;
        }
    }

    /* Final override: enforce horizontal scrolling for Issue List columns */

    @media (max-width: 900px) {
        #updateinfo {
            display: none;
        }

        .button-container.button-container-focus {
            margin-top: 8px;
        }

        .content-wrapper {
            margin: 10px;
            padding: 10px;
        }

        .issue-kpi-row {
            grid-template-columns: repeat(2, minmax(120px, 1fr));
        }

        .portal-layout-shell {
            flex-direction: column;
            min-height: 0;
        }

        .portal-left-nav {
            width: 100%;
            flex: 1 1 auto;
            position: static;
            max-height: none;
            min-height: 0;
        }

        .issue-page-hd-top {
            align-items: flex-start;
        }
    }
    .issue-grid-scroll {
        overflow: hidden !important;
        width: 100%;
        max-width: 100%;
        box-sizing: border-box;
        -webkit-overflow-scrolling: touch;
    }

    .issue-grid-inner {
        width: 100%;
        max-width: 100%;
        overflow-x: auto;
        overflow-y: visible;
        box-sizing: border-box;
    }

    .issue-grid-inner #overall_request_details {
        table-layout: fixed !important;
        width: 100% !important;
        min-width: 1620px !important;
        max-width: none !important;
        display: table !important;
        overflow: visible !important;
    }

    .issue-grid-inner #overall_request_details th,
    .issue-grid-inner #overall_request_details td {
        width: auto !important;
        min-width: 78px !important;
        max-width: none !important;
        white-space: normal !important;
        overflow: visible !important;
        text-overflow: clip !important;
        overflow-wrap: anywhere !important;
        word-break: break-word !important;
        vertical-align: top !important;
    }

    .issue-grid-inner #overall_request_details td > span,
    .issue-grid-inner #overall_request_details td .status-main,
    .issue-grid-inner #overall_request_details td .sysdebug-preview,
    .issue-grid-inner #overall_request_details td .component-column,
    .issue-grid-inner #overall_request_details td .owner-column,
    .issue-grid-inner #overall_request_details td .impact-column {
        display: block !important;
        width: 100% !important;
        max-width: 100% !important;
        white-space: normal !important;
        overflow-wrap: anywhere !important;
        word-break: break-word !important;
    }

    .issue-grid-inner #overall_request_details,
    .issue-grid-inner > table,
    .issue-grid-inner > .table-primary,
    .issue-grid-inner .table-primary {
        table-layout: fixed !important;
        width: 100% !important;
        min-width: 1560px !important;
        max-width: none !important;
        display: table !important;
    }

    .issue-grid-inner #<%= overall_request_details.ClientID %>,
    .issue-grid-inner > table,
    .issue-grid-inner > .table-primary,
    .issue-grid-inner .table-primary {
        table-layout: fixed !important;
        width: 100% !important;
        min-width: 1560px !important;
        max-width: none !important;
        display: table !important;
    }

    .issue-grid-inner #overall_request_details .field-sno { width: 62px !important; min-width: 62px !important; }
    .issue-grid-inner #overall_request_details .field-issue_details { width: 175px !important; min-width: 175px !important; }
    .issue-grid-inner #overall_request_details .field-customer_detail { width: 116px !important; min-width: 116px !important; }
    .issue-grid-inner #overall_request_details .field-duplicatedetails { width: 126px !important; min-width: 126px !important; max-width: 126px !important; }
    .issue-grid-inner #overall_request_details .field-component { width: 116px !important; min-width: 116px !important; }
    .issue-grid-inner #overall_request_details .field-owner { width: 116px !important; min-width: 116px !important; }
    .issue-grid-inner #overall_request_details .field-status { width: 176px !important; min-width: 176px !important; }
    .issue-grid-inner #overall_request_details .field-cmf_status { width: 96px !important; min-width: 96px !important; }
    .issue-grid-inner #overall_request_details .field-idst { width: 92px !important; min-width: 92px !important; }
    .issue-grid-inner #overall_request_details .field-imagefreeze { width: 92px !important; min-width: 92px !important; }
    .issue-grid-inner #overall_request_details .field-los { width: 58px !important; min-width: 58px !important; text-align: center !important; }
    .issue-grid-inner #overall_request_details .field-rvp_repro { width: 78px !important; min-width: 78px !important; }
    .issue-grid-inner #overall_request_details .field-impact_processor { width: 110px !important; min-width: 110px !important; }
    .issue-grid-inner #overall_request_details .field-days_open { width: 58px !important; min-width: 58px !important; text-align: center !important; }
    .issue-grid-inner #overall_request_details .field-milestone { width: 112px !important; min-width: 112px !important; }
    .issue-grid-inner #overall_request_details .field-closed_reason { width: 128px !important; min-width: 128px !important; }

    .issue-grid-inner #overall_request_details th.field-sno,
    .issue-grid-inner #overall_request_details td.field-sno,
    .issue-grid-inner #overall_request_details th.field-days_open,
    .issue-grid-inner #overall_request_details td.field-days_open {
        text-align: center !important;
    }

    /* Reference-style Issue List polish */
    #mainDataWrapper {
        background: #f8fbff;
        border: 1px solid #e6edf6;
        border-radius: 14px;
        box-shadow: 0 10px 28px rgba(15, 35, 70, 0.06);
    }

    .issue-page-hd {
        margin: 0 0 12px;
    }

    .issue-page-title {
        color: #111827;
        font-size: 18px;
        line-height: 1.2;
        font-weight: 800;
        margin: 0;
    }

    .issue-page-desc {
        color: #64748b;
        font-size: 12px;
        margin: 4px 0 0;
    }

    .issue-kpi-row {
        grid-template-columns: repeat(4, minmax(150px, 1fr));
        gap: 12px;
        margin: 10px 0 14px;
    }

    .issue-kpi {
        position: relative;
        min-height: 92px;
        padding: 14px 16px;
        background: #ffffff;
        border: 1px solid #e8edf5;
        border-radius: 10px;
        box-shadow: 0 8px 20px rgba(28, 48, 78, 0.05);
        overflow: hidden;
    }

    .issue-kpi::before {
        content: "";
        position: absolute;
        inset: 0 auto 0 0;
        width: 4px;
        background: #ef4444;
        opacity: 0.85;
    }

    .issue-kpi:nth-child(2)::before { background: #f59e0b; }
    .issue-kpi:nth-child(3)::before { background: #22c55e; }
    .issue-kpi:nth-child(4)::before { background: #6366f1; }

    .issue-kpi-label {
        color: #334155;
        font-size: 11px;
        font-weight: 800;
        letter-spacing: 0;
        text-transform: none;
        margin-bottom: 8px;
    }

    .issue-kpi-value {
        color: #ef4444;
        font-size: 27px;
        line-height: 1;
        font-weight: 800;
        margin-bottom: 6px;
    }

    .issue-kpi-sub {
        color: #64748b;
        font-size: 11px;
        font-weight: 600;
    }

    #sharedFilterPanel.overview-platform-actions,
    .issue-top-filter-panel {
        background: #ffffff;
        border: 1px solid #e6edf6;
        border-radius: 10px;
        box-shadow: 0 8px 20px rgba(28, 48, 78, 0.04);
    }

    .issue-grid-inner #overall_request_details {
        min-width: 1668px !important;
        border-collapse: separate !important;
        border-spacing: 0 !important;
        border: 1px solid #e8edf5 !important;
        border-radius: 10px !important;
        overflow: hidden !important;
        background: #ffffff !important;
        box-shadow: 0 10px 24px rgba(28, 48, 78, 0.05) !important;
    }

    .issue-grid-inner #overall_request_details th {
        background: #f7f9fc !important;
        color: #243042 !important;
        border: 0 !important;
        border-bottom: 1px solid #e5ebf3 !important;
        padding: 10px 12px !important;
        font-size: 10px !important;
        font-weight: 800 !important;
        letter-spacing: 0 !important;
        text-transform: none !important;
    }

    .issue-grid-inner #overall_request_details td {
        background: #ffffff !important;
        border: 0 !important;
        border-bottom: 1px solid #eef2f7 !important;
        box-shadow: none !important;
        padding: 12px !important;
        color: #1f2937 !important;
    }

    .issue-grid-inner #overall_request_details tr:hover td {
        background: #fbfdff !important;
    }

    .issue-grid-inner #overall_request_details .field-status,
    .issue-grid-inner #overall_request_details td.field-status,
    .issue-grid-inner #overall_request_details th.field-status {
        overflow: hidden !important;
        word-break: normal !important;
        overflow-wrap: break-word !important;
    }

    .issue-grid-inner #overall_request_details td.field-status > span,
    .issue-grid-inner #overall_request_details td.field-status .status-cell-wrap-compact,
    .issue-grid-inner #overall_request_details td.field-status .status-one-line {
        width: 100% !important;
        max-width: 100% !important;
        min-width: 0 !important;
    }

    .issue-grid-inner #overall_request_details td.field-status .status-one-line {
        max-height: 64px !important;
        overflow-y: auto !important;
        overflow-x: hidden !important;
    }

    .issue-grid-inner #overall_request_details td.field-impact_processor .impact-text {
        max-height: 76px !important;
        overflow-y: auto !important;
        overflow-x: hidden !important;
    }

    .issue-grid-inner .empty-data-container,
    .issue-grid-inner .empty-data-container table,
    .issue-grid-inner .empty-data-container table.table-primary {
        width: 100% !important;
        min-width: 100% !important;
        max-width: 100% !important;
    }

    .issue-scroll-proxy {
        display: none !important;
        width: 100%;
        overflow-x: scroll;
        overflow-y: hidden;
        height: 18px;
        margin-top: 6px;
        border: 1px solid #d7d7d7;
        border-radius: 2px;
        background: #f5f5f5;
    }

    .issue-scroll-proxy-inner {
        height: 1px;
    }

    header #updateinfo,
    #updateinfo {
        display: none !important;
        visibility: hidden !important;
        height: 0 !important;
        width: 0 !important;
        overflow: hidden !important;
    }

    .portal-layout-shell {
        margin-top: 0 !important;
    }

    .portal-left-nav {
        top: 48px !important;
        min-height: calc(100vh - 48px) !important;
        max-height: calc(100vh - 48px) !important;
        border-radius: 0 0 12px 12px !important;
    }

    /* Final layout corrections for side menu alignment and scroll whitespace */
    .container-scroller,
    .container-fluid.page-body-wrapper {
        padding-left: 0 !important;
        margin-left: 0 !important;
    }

    .portal-layout-shell {
        min-height: 0 !important;
        margin-left: 0 !important;
    }

    .portal-left-nav {
        margin-left: 0 !important;
        left: 0 !important;
        border-radius: 0 12px 12px 0 !important;
    }

    .gridview-container-wrapper {
        align-items: flex-start;
    }

    .gridview-container3 {
        height: auto !important;
        min-height: 0 !important;
        margin-bottom: 0 !important;
    }

    /* Remove Issue List hover lift/shadow effect */
    #overall_request_details tr:hover td,
    .issue-grid-inner #overall_request_details tr:hover td {
        transform: none !important;
        box-shadow: none !important;
        background: #ffffff !important;
        border-color: #eef2f7 !important;
    }

    /* ============================================================
    CMF PORTAL — PHASE 1 USER-CENTRIC UI
    ============================================================ */

    /* ------------------------------------------------------------
    APPLICATION SURFACE
    ------------------------------------------------------------ */

    html,
    body {
        background: #f5f7fb !important;
        color: #172033 !important;
    }

    body {
        font-family: var(--portal-font-body) !important;
        font-size: 14px !important;
        line-height: 1.45 !important;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-rendering: optimizeLegibility;
    }

    h1,
    h2,
    h3,
    h4,
    h5,
    h6,
    .header-title,
    .ccip-dash-greeting,
    .ccip-cmf-summary-title,
    .ccip-card-title,
    .ccip-card-mini-title,
    .overall-card-title {
        font-family: var(--portal-font-display) !important;
        letter-spacing: -0.01em !important;
    }

    .ccip-dash-greeting {
        font-size: 22px !important;
        font-weight: 800 !important;
        line-height: 1.15 !important;
        font-family: var(--portal-font-display) !important;
    }

    .overall-card-title,
    .ccip-card-title {
        font-size: 16px !important;
        font-weight: 800 !important;
        line-height: 1.2 !important;
    }

    .ccip-card-mini-title {
        font-size: 14px !important;
        font-weight: 780 !important;
        line-height: 1.2 !important;
    }

    #homeLegacyTablesGrid .ccip-card-mini-title,
    #homeCmfSummaryPanel .ccip-card-mini-title {
        font-size: 16px !important;
        font-weight: 800 !important;
        line-height: 1.2 !important;
        letter-spacing: -0.01em !important;
        text-transform: uppercase;
        color: #1e3a60 !important;
    }

    .ccip-summary-fact-label,
    .overall-tpt-label {
        font-family: var(--portal-font-card) !important;
        font-size: 11px !important;
        font-weight: 760 !important;
        letter-spacing: 0.02em !important;
        text-transform: uppercase;
    }

    .ccip-summary-fact-value,
    .overall-tpt-value {
        font-family: var(--portal-font-card) !important;
        font-variant-numeric: tabular-nums;
        font-feature-settings: "tnum" 1, "lnum" 1;
        letter-spacing: -0.01em;
    }

    .overall-tpt-note,
    .ccip-summary-fact-note {
        font-family: var(--portal-font-card) !important;
    }

    .ccip-summary-fact-value,
    .overall-tpt-value {
        font-weight: 900 !important;
    }


    /* ------------------------------------------------------------
    TOP HEADER
    ------------------------------------------------------------ */

    header {
        min-height: 76px !important;
        height: 76px !important;

        padding: 0 28px !important;

        display: flex !important;
        align-items: center !important;
        justify-content: flex-start !important;

        background: linear-gradient(90deg, #eef6fb 0%, #f7fafc 52%, #edf8f8 100%) !important;

        color: #172033 !important;

        border-bottom: 1px solid #d7e2ec !important;

        box-shadow: 0 2px 8px rgba(16, 24, 40, 0.05) !important;

        position: sticky !important;
        top: 0 !important;

        z-index: 1000 !important;
    }

    .header-title {
        color: #172033 !important;

        font-size: 17px !important;
        font-weight: 750 !important;

        letter-spacing: 0 !important;
        text-transform: none !important;

        text-align: left !important;
    }

    .header-user-mode {
        right: 24px !important;

        color: #475467 !important;
        margin-left: auto !important;
        flex: 0 0 auto !important;
    }

    .header-mode-dropdown {
        height: 36px !important;

        min-width: 155px !important;

        background: #ffffff !important;

        color: #344054 !important;

        border: 1px solid #d0d5dd !important;

        border-radius: 8px !important;
    }

    .sidebar-toggle-div {
        display: none !important;
    }


    /* ------------------------------------------------------------
    LEFT NAVIGATION
    ------------------------------------------------------------ */

    .portal-layout-shell {
        gap: 10px !important;

        min-height: calc(100vh - 76px) !important;
        align-items: stretch !important;
        padding-top: 12px !important;
    }

    .portal-left-nav {
        width: 176px !important;
        flex: 0 0 176px !important;

        top: 76px !important;

        min-height: 100% !important;
        max-height: none !important;
        height: auto !important;
        align-self: stretch !important;

        padding: 16px 10px !important;

        background: linear-gradient(180deg, #0f3f59 0%, #0b3148 54%, #09263b 100%) !important;

        border-radius: 0 !important;

        box-shadow: none !important;

        border-right: 1px solid rgba(142, 211, 215, 0.18) !important;
    }

    .portal-left-nav.is-collapsed {
        width: 60px !important;
        flex: 0 0 60px !important;
        padding-left: 5px !important;
        padding-right: 5px !important;
    }

    .portal-left-nav .portal-nav-link {
        min-height: 42px !important;

        padding: 10px 12px !important;

        margin-bottom: 5px !important;

        border: none !important;

        background: transparent !important;

        border-radius: 8px !important;

        color: #dcecf2 !important;

        font-size: 13px !important;
        font-weight: 600 !important;
    }

    .portal-left-nav .portal-nav-link:hover {
        background: rgba(0, 166, 178, 0.18) !important;

        transform: none !important;

        box-shadow: none !important;
    }

    .portal-left-nav .portal-nav-link.is-active {
        background: #1b7f8f !important;

        border: none !important;

        box-shadow: none !important;

        color: #ffffff !important;
    }

    .portal-left-nav .portal-nav-link.is-active:before {
        display: none !important;
    }

    .portal-left-nav-title {
        color: #ffffff !important;
    }


    /* ------------------------------------------------------------
    MAIN CONTENT
    ------------------------------------------------------------ */

    .portal-main-workspace {
        min-width: 0 !important;

        width: 100% !important;
    }

    .content-wrapper {
        width: 100% !important;

        box-sizing: border-box !important;

        margin: 0 !important;

        padding: 28px 30px 42px !important;

        background: #f5f7fb !important;

        border: none !important;

        box-shadow: none !important;

        border-radius: 0 !important;
    }


    /* Remove old banner-style tab headings */

    .pane-heading {
        background: transparent !important;

        border: none !important;

        box-shadow: none !important;

        padding: 0 !important;

        margin: 0 0 16px !important;

        color: #172033 !important;

        font-size: 24px !important;

        font-weight: 750 !important;

        letter-spacing: -0.02em !important;

        text-transform: none !important;

        text-align: left !important;
    }


    /* ------------------------------------------------------------
    ISSUE LIST LAYOUT
    ------------------------------------------------------------ */

    .issue-tab-shell {
        display: grid !important;

        grid-template-columns:
            minmax(0, 1fr)
            240px !important;

        gap: 16px !important;

        align-items: start !important;
    }

    .issue-tab-main {
        min-width: 0 !important;
    }


    /* ------------------------------------------------------------
    ISSUE PAGE HEADER
    ------------------------------------------------------------ */

    .issue-page-hd {
        background: transparent !important;

        border: none !important;

        box-shadow: none !important;

        padding: 0 0 4px !important;

        margin-bottom: 14px !important;
    }

    .issue-page-title {
        margin: 0 !important;

        color: #172033 !important;

        font-size: 24px !important;

        font-weight: 750 !important;

        letter-spacing: -0.02em !important;
    }

    .issue-page-desc {
        margin: 5px 0 0 !important;

        color: #667085 !important;

        font-size: 13px !important;

        line-height: 1.45 !important;
    }


    /* ------------------------------------------------------------
    ISSUE KPI CARDS
    ------------------------------------------------------------ */

    .issue-kpi-row {
        display: grid !important;

        grid-template-columns:
            repeat(4, minmax(0, 1fr)) !important;

        gap: 12px !important;

        margin-bottom: 14px !important;
    }

    .issue-kpi {
        min-height: 92px !important;

        padding: 14px 16px !important;

        background: #ffffff !important;

        border: 1px solid #e4e7ec !important;

        border-radius: 12px !important;

        box-shadow:
            0 2px 7px rgba(16,24,40,0.04) !important;
    }

    .issue-kpi-label {
        color: #667085 !important;

        font-size: 11px !important;

        font-weight: 700 !important;
    }

    .issue-kpi-value {
        margin-top: 5px !important;

        color: #172033 !important;

        font-size: 28px !important;

        font-weight: 800 !important;
    }

    .issue-kpi-sub {
        margin-top: 5px !important;

        color: #98a2b3 !important;

        font-size: 10px !important;
    }


    /* ------------------------------------------------------------
    ISSUE FILTER PANEL
    ------------------------------------------------------------ */

    .issue-top-filter-panel,
    #sharedFilterPanel.overview-platform-actions {
        background: #ffffff !important;

        border: 1px solid #e4e7ec !important;

        border-radius: 12px !important;

        box-shadow:
            0 2px 7px rgba(16,24,40,0.04) !important;

        padding: 14px !important;

        margin-bottom: 12px !important;
    }

    .issue-filter-section-header {
        margin-bottom: 10px !important;
    }

    .issue-filter-title {
        color: #172033 !important;

        font-size: 13px !important;

        font-weight: 750 !important;
    }

    .issue-top-filter-grid {
        display: grid !important;

        grid-template-columns:
            repeat(4, minmax(150px, 1fr)) !important;

        gap: 10px !important;
    }

    .issue-top-filter-item {
        display: flex !important;

        flex-direction: column !important;

        gap: 5px !important;
    }

    .issue-top-filter-item > span {
        color: #667085 !important;

        font-size: 10px !important;

        font-weight: 700 !important;
    }

    .issue-top-filter-item .header-dropdown {
        width: 100% !important;

        max-width: none !important;

        height: 34px !important;

        background: #ffffff !important;

        border: 1px solid #d0d5dd !important;

        border-radius: 7px !important;

        color: #344054 !important;

        font-size: 11px !important;
    }

    .issue-top-filter-buttons {
        margin-top: 10px !important;
    }


    /* ------------------------------------------------------------
    ISSUE SEARCH TOOLBAR
    ------------------------------------------------------------ */

    .issue-user-toolbar {
        display: flex !important;

        align-items: center !important;

        justify-content: space-between !important;

        gap: 12px !important;

        margin-bottom: 10px !important;
    }

    .issue-search-box {
        position: relative !important;

        flex: 1 1 360px !important;

        max-width: 520px !important;
    }

    .issue-search-box > i {
        position: absolute !important;

        left: 12px !important;

        top: 50% !important;

        transform: translateY(-50%) !important;

        color: #98a2b3 !important;

        font-size: 12px !important;
    }

    #issueQuickSearch {
        width: 100% !important;

        height: 38px !important;

        box-sizing: border-box !important;

        padding: 0 38px 0 34px !important;

        border: 1px solid #d0d5dd !important;

        border-radius: 8px !important;

        background: #ffffff !important;

        color: #172033 !important;

        font-size: 12px !important;

        outline: none !important;
    }

    #issueQuickSearch:focus {
        border-color: #4f6bed !important;

        box-shadow:
            0 0 0 3px rgba(79,107,237,0.10) !important;
    }

    .issue-search-clear {
        display: none;

        position: absolute !important;

        right: 8px !important;

        top: 50% !important;

        transform: translateY(-50%) !important;

        width: 24px !important;

        height: 24px !important;

        border: none !important;

        background: transparent !important;

        color: #98a2b3 !important;

        cursor: pointer !important;
    }

    .issue-user-actions {
        display: flex !important;

        gap: 8px !important;

        align-items: center !important;
    }

    .issue-toolbar-btn {
        min-height: 36px !important;

        padding: 0 12px !important;

        border: 1px solid #d0d5dd !important;

        border-radius: 8px !important;

        background: #ffffff !important;

        color: #344054 !important;

        font-size: 11px !important;

        font-weight: 700 !important;

        cursor: pointer !important;
    }

    .issue-toolbar-btn:hover {
        background: #f8fafc !important;

        border-color: #b8c0cc !important;
    }


    /* ------------------------------------------------------------
    ISSUE TABLE CONTAINER
    ------------------------------------------------------------ */

    .issue-grid-scroll {
        width: 100% !important;

        max-width: 100% !important;

        box-sizing: border-box !important;

        padding: 0 !important;

        border: 1px solid #e1e5eb !important;

        border-radius: 12px !important;

        background: #ffffff !important;

        box-shadow:
            0 2px 8px rgba(16,24,40,0.04) !important;

        overflow-x: auto !important;
    }

    .issue-grid-inner {
        min-width: 0 !important;
    }


    /* ------------------------------------------------------------
    ISSUE TABLE
    IMPORTANT: use field-* classes instead of nth-child()
    ------------------------------------------------------------ */

    #overall_request_details {
        width: 100% !important;

        min-width: 1050px !important;

        margin: 0 !important;

        border-collapse: separate !important;

        border-spacing: 0 !important;

        table-layout: auto !important;

        background: #ffffff !important;

        border: none !important;

        border-radius: 12px !important;

        box-shadow: none !important;

        font-size: 12px !important;
    }


    /* Header */

    #overall_request_details thead,
    #overall_request_details thead tr {
        background: #f8fafc !important;
    }

    #overall_request_details thead th {
        position: sticky !important;

        top: 0 !important;

        z-index: 10 !important;

        background: #f8fafc !important;

        color: #667085 !important;

        border: none !important;

        border-bottom: 1px solid #e4e7ec !important;

        padding: 11px 12px !important;

        font-size: 10px !important;

        font-weight: 750 !important;

        text-align: left !important;

        white-space: nowrap !important;
    }


    /* Hide filter dropdowns INSIDE table headers.
    Filters now live in the dedicated filter panel above. */

    #overall_request_details thead .filter-container {
        display: none !important;
    }

    #overall_request_details thead .filter-header-container {
        min-height: auto !important;

        align-items: flex-start !important;

        gap: 0 !important;
    }

    #overall_request_details thead .filter-header-text {
        margin: 0 !important;

        align-self: flex-start !important;

        text-align: left !important;
    }


    /* Body */

    #overall_request_details tbody tr {
        background: #ffffff !important;

        transition: background-color 0.12s ease !important;
    }

    #overall_request_details tbody tr:nth-child(even) {
        background: #fbfcfe !important;
    }

    #overall_request_details tbody tr:hover {
        background: #f5f9ff !important;
    }

    #overall_request_details tbody td {
        background: transparent !important;

        border: none !important;

        border-bottom: 1px solid #edf0f4 !important;

        padding: 13px 12px !important;

        color: #475467 !important;

        font-size: 12px !important;

        line-height: 1.45 !important;

        vertical-align: middle !important;

        overflow-wrap: anywhere !important;
    }

    #overall_request_details tbody tr:last-child td {
        border-bottom: none !important;
    }


    /* ------------------------------------------------------------
    ISSUE DETAILS — MOST IMPORTANT
    ------------------------------------------------------------ */

    #overall_request_details .field-issue_details {
        min-width: 280px !important;

        max-width: 360px !important;

        color: #172033 !important;

        font-size: 12px !important;

        font-weight: 500 !important;
    }

    #overall_request_details .field-issue_details a {
        color: #0068b5 !important;

        font-weight: 750 !important;

        text-decoration: none !important;
    }

    #overall_request_details .field-issue_details a:hover {
        color: #004f86 !important;

        text-decoration: underline !important;
    }

    .issue-meta-row-primary {
        align-items: center !important;
        justify-content: space-between !important;
        gap: 8px !important;
    }

    .issue-details-ai-btn {
        min-height: 24px !important;
        height: 24px !important;
        padding: 0 8px !important;
        font-size: 10px !important;
        white-space: nowrap !important;
    }


    /* ------------------------------------------------------------
    CUSTOMER
    ------------------------------------------------------------ */

    #overall_request_details .field-customer_detail {
        min-width: 112px !important;

        max-width: 138px !important;

        color: #344054 !important;

        font-weight: 600 !important;
    }

    #overall_request_details .field-duplicatedetails {
        min-width: 118px !important;
        max-width: 132px !important;
        width: 126px !important;
        white-space: normal !important;
        overflow-wrap: anywhere !important;
        word-break: break-word !important;
        line-height: 1.25 !important;
    }

    #overall_request_details th.field-duplicatedetails .filter-header-text,
    #overall_request_details th.field-duplicatedetails {
        white-space: normal !important;
        line-height: 1.2 !important;
    }


    /* ------------------------------------------------------------
    COMPONENT
    ------------------------------------------------------------ */

    #overall_request_details .field-component {
        min-width: 135px !important;

        max-width: 175px !important;

        color: #475467 !important;
    }


    /* ------------------------------------------------------------
    OWNER
    ------------------------------------------------------------ */

    #overall_request_details .field-owner {
        min-width: 120px !important;

        max-width: 160px !important;

        color: #475467 !important;
    }


    /* ------------------------------------------------------------
    MILESTONE
    ------------------------------------------------------------ */

    #overall_request_details .field-milestone {
        min-width: 105px !important;

        max-width: 150px !important;

        color: #344054 !important;

        font-weight: 600 !important;
    }


    /* ------------------------------------------------------------
    STATUS
    ------------------------------------------------------------ */

    #overall_request_details .field-status {
        min-width: 125px !important;

        max-width: 170px !important;

        color: #344054 !important;
    }


    /* ------------------------------------------------------------
    IMPACT
    ------------------------------------------------------------ */

    #overall_request_details .field-impact_processor {
        min-width: 190px !important;

        max-width: 250px !important;

        color: #475467 !important;
    }


    /* ------------------------------------------------------------
    DAYS OPEN
    ------------------------------------------------------------ */

    #overall_request_details .field-days_open {
        min-width: 75px !important;

        width: 75px !important;

        text-align: center !important;

        font-weight: 750 !important;
    }

    #overall_request_details .field-days_open .days-open,
    #overall_request_details .field-days_open span {
        display: inline-flex !important;

        align-items: center !important;

        justify-content: center !important;

        min-width: 34px !important;

        min-height: 25px !important;

        padding: 3px 8px !important;

        border-radius: 999px !important;

        background: #fff7e6 !important;

        border: 1px solid #f1d59b !important;

        color: #9a6700 !important;

        font-size: 11px !important;

        font-weight: 750 !important;
    }


    /* ------------------------------------------------------------
    STATUS / GENERAL BADGES
    ------------------------------------------------------------ */

    #overall_request_details .badge,
    #overall_request_details .tag,
    #overall_request_details [class*="badge"],
    #overall_request_details [class*="tag"] {
        display: inline-flex !important;

        align-items: center !important;

        justify-content: center !important;

        min-height: 23px !important;

        padding: 3px 9px !important;

        border-radius: 999px !important;

        font-size: 10px !important;

        font-weight: 700 !important;

        white-space: nowrap !important;
    }


    /* ------------------------------------------------------------
    EDIT COLUMN
    ------------------------------------------------------------ */

    #overall_request_details tbody td:first-child {
        width: 46px !important;

        min-width: 46px !important;

        text-align: center !important;

        padding: 10px 5px !important;
    }


    /* ------------------------------------------------------------
    AI SUMMARY BUTTON
    ------------------------------------------------------------ */

    #overall_request_details .ai-summary-btn,
    #overall_request_details .ai-summary-trigger {
        border-radius: 7px !important;
    }


    /* ------------------------------------------------------------
    SIDE ASSISTANT
    ------------------------------------------------------------ */

    .issue-side-panel {
        width: auto !important;

        box-sizing: border-box !important;

        padding: 10px !important;

        background: transparent !important;

        border: none !important;

        box-shadow: none !important;

        border-radius: 0 !important;

        gap: 10px !important;

        position: sticky !important;

        top: 90px !important;
    }

    .interactive-side-card {
        background: #ffffff !important;

        border: 1px solid #e4e7ec !important;

        border-radius: 12px !important;

        box-shadow:
            0 2px 7px rgba(16,24,40,0.04) !important;

        padding: 12px !important;
    }

    .interactive-side-title {
        font-size: 13px !important;

        color: #172033 !important;
    }

    .interactive-side-sub {
        font-size: 11px !important;

        line-height: 1.45 !important;

        color: #667085 !important;
    }


    /* ------------------------------------------------------------
    PAGINATION
    ------------------------------------------------------------ */

    .issue-pager-controls {
        display: grid !important;

        grid-template-columns: 1fr auto 1fr !important;

        align-items: center !important;

        justify-content: stretch !important;

        gap: 5px !important;

        margin-top: 12px !important;

        padding: 8px 0 !important;
    }


    /* ------------------------------------------------------------
    RESPONSIVE
    ------------------------------------------------------------ */

    @media (max-width: 1200px) {

        .issue-tab-shell {
            grid-template-columns: 1fr !important;
        }

        .issue-side-panel {
            position: static !important;

            display: grid !important;

            grid-template-columns: repeat(3, minmax(0, 1fr)) !important;
        }

        .issue-kpi-row {
            grid-template-columns: repeat(2, 1fr) !important;
        }

        .issue-top-filter-grid {
            grid-template-columns: repeat(3, minmax(150px, 1fr)) !important;
        }
    }

    @media (max-width: 760px) {

        .content-wrapper {
            padding: 16px !important;
        }

        .issue-user-toolbar {
            align-items: stretch !important;

            flex-direction: column !important;
        }

        .issue-search-box {
            max-width: none !important;
        }

        .issue-user-actions {
            justify-content: flex-start !important;
        }

        .issue-kpi-row {
            grid-template-columns: 1fr !important;
        }

        .issue-top-filter-grid {
            grid-template-columns: 1fr !important;
        }

        .issue-side-panel {
            grid-template-columns: 1fr !important;
        }
    }

    /* ============================================================
   CMF PENDING LIST
   ============================================================ */

    .pending-tab-shell {
        display: grid !important;

        grid-template-columns:
            minmax(0, 1fr)
            240px !important;

        gap: 16px !important;

        align-items: start !important;
    }

    .pending-tab-main {
        min-width: 0 !important;
    }

    .cmf-pending-page-hd {
        background: transparent !important;

        border: none !important;

        box-shadow: none !important;

        padding: 0 0 6px !important;

        margin-bottom: 14px !important;
    }

    .cmf-pending-page-title {
        margin: 0 !important;

        color: #172033 !important;

        font-size: 24px !important;

        font-weight: 750 !important;
    }

    .cmf-pending-page-desc {
        margin: 5px 0 0 !important;

        color: #667085 !important;

        font-size: 13px !important;
    }

    .cmf-pending-grid-wrap {
        width: 100% !important;

        box-sizing: border-box !important;

        padding: 0 !important;

        background: #ffffff !important;

        border: 1px solid #e1e5eb !important;

        border-radius: 12px !important;

        box-shadow:
            0 2px 8px rgba(16,24,40,0.04) !important;

        overflow-x: auto !important;
    }

    #GridView_cmf_pending {
        width: 100% !important;

        min-width: 900px !important;

        border-collapse: separate !important;

        border-spacing: 0 !important;

        background: #ffffff !important;
    }

    #GridView_cmf_pending th {
        background: #f8fafc !important;

        color: #667085 !important;

        border: none !important;

        border-bottom: 1px solid #e4e7ec !important;

        padding: 11px 12px !important;

        font-size: 10px !important;

        font-weight: 750 !important;

        text-align: left !important;
    }

    #GridView_cmf_pending td {
        background: #ffffff !important;

        color: #475467 !important;

        border: none !important;

        border-bottom: 1px solid #edf0f4 !important;

        padding: 13px 12px !important;

        font-size: 12px !important;

        line-height: 1.45 !important;

        vertical-align: top !important;
    }

    #GridView_cmf_pending tr:hover td {
        background: #f5f9ff !important;
    }

    .pending-side-panel {
        position: sticky !important;

        top: 90px !important;

        background: transparent !important;

        border: none !important;

        box-shadow: none !important;

        padding: 10px !important;
    }

    /* Reference-style top bar, global search, and compact work tables */
    header {
        height: 76px !important;
        min-height: 76px !important;
        padding: 0 28px !important;
        background: linear-gradient(90deg, #eef6fb 0%, #f7fafc 52%, #edf8f8 100%) !important;
        border-bottom: 1px solid #d7e2ec !important;
        box-shadow: 0 6px 18px rgba(15, 45, 65, 0.08) !important;
        gap: 22px !important;
    }

    .header-title {
        min-width: 220px !important;
        color: #16213a !important;
        font-size: 16px !important;
        font-weight: 800 !important;
    }

    .global-search-shell {
        position: relative;
        width: min(480px, 42vw);
        min-width: 320px;
        margin: 0 auto;
        flex: 0 1 480px;
        display: flex;
        align-items: center;
        color: #73839a;
    }

    .global-search-shell > i {
        position: absolute;
        left: 14px;
        font-size: 13px;
        color: #75839a;
        pointer-events: none;
    }

    #globalPortalSearch {
        width: 100%;
        height: 38px;
        padding: 0 84px 0 40px;
        border: 1px solid #dce3ee;
        border-radius: 10px;
        background: #ffffff;
        color: #1d2939;
        font-size: 12px;
        font-weight: 600;
        outline: none;
        box-shadow: 0 4px 16px rgba(15, 23, 42, 0.04);
    }

    #globalPortalSearch:focus {
        border-color: #6b8cff;
        box-shadow: 0 0 0 3px rgba(49, 91, 234, 0.12), 0 6px 18px rgba(15, 23, 42, 0.07);
    }

    .global-search-shortcut {
        position: absolute;
        right: 12px;
        display: inline-flex;
        align-items: center;
        height: 20px;
        padding: 0 7px;
        border: 1px solid #dce3ee;
        border-radius: 6px;
        background: #f8fafc;
        color: #667085;
        font-size: 10px;
        font-weight: 800;
    }

    .global-search-clear {
        display: none;
        position: absolute;
        right: 70px;
        width: 22px;
        height: 22px;
        border: none;
        border-radius: 999px;
        background: #eef2f7;
        color: #667085;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        font-size: 15px;
        line-height: 1;
    }

    .header-user-mode {
        position: static !important;
        margin-left: auto !important;
        flex: 0 0 auto !important;
        color: #475467 !important;
    }

    .header-mode-dropdown {
        height: 34px !important;
        min-width: 148px !important;
        border-radius: 9px !important;
        font-size: 12px !important;
        font-weight: 700 !important;
    }

    .portal-left-nav {
        top: 76px !important;
        min-height: 100% !important;
        max-height: none !important;
        height: auto !important;
        align-self: stretch !important;
        background: linear-gradient(180deg, #0f3f59 0%, #0b3148 54%, #09263b 100%) !important;
    }

    .content-wrapper {
        background: #f6f8fc !important;
        padding: 28px 30px 42px !important;
    }

    .portal-layout-shell.main-menu-collapsed .content-wrapper {
        padding-left: 28px !important;
    }

    #mainDataWrapper.content-wrapper {
        padding-left: 0 !important;
    }

    #mainDataWrapper > .modal-body.popup {
        padding-left: 0 !important;
        padding-right: 0 !important;
    }

    .issue-page-hd,
    .cmf-pending-page-hd {
        margin-bottom: 10px !important;
    }

    .issue-page-title,
    .cmf-pending-page-title {
        font-size: 21px !important;
        letter-spacing: 0 !important;
    }

    .issue-page-desc,
    .cmf-pending-page-desc {
        font-size: 12px !important;
    }

    .issue-kpi-row {
        grid-template-columns: repeat(4, minmax(140px, 1fr)) !important;
        gap: 9px !important;
        margin-bottom: 10px !important;
    }

    .issue-kpi {
        min-height: 78px !important;
        padding: 11px 13px !important;
        border-radius: 10px !important;
        border-color: #e8edf5 !important;
        box-shadow: 0 3px 14px rgba(15, 23, 42, 0.035) !important;
    }

    .issue-kpi-value {
        font-size: 24px !important;
    }

    .issue-top-filter-panel {
        display: flex !important;
        align-items: flex-end !important;
        gap: 8px !important;
        margin: 0 0 10px !important;
        padding: 10px !important;
        border: 1px solid #e3e9f2 !important;
        border-radius: 12px !important;
        background: #ffffff !important;
        box-shadow: 0 4px 16px rgba(15, 23, 42, 0.04) !important;
    }

    .issue-filter-section-header {
        display: flex !important;
        order: 2 !important;
        align-items: flex-end !important;
        justify-content: flex-end !important;
        margin: 0 !important;
        padding: 0 !important;
        border: 0 !important;
    }

    .issue-filter-title {
        display: none !important;
    }

    #issueTopFilterBody {
        display: flex !important;
        order: 1 !important;
        flex: 1 1 auto !important;
        align-items: flex-end !important;
        gap: 8px !important;
        padding: 0 !important;
        margin: 0 !important;
        background: transparent !important;
        border: 0 !important;
        box-shadow: none !important;
    }

    #issueTopFilterBody.is-collapsed {
        display: none !important;
    }

    .issue-top-filter-grid {
        display: flex !important;
        align-items: end !important;
        gap: 8px !important;
        flex: 1 1 auto !important;
        overflow-x: auto !important;
        padding-bottom: 2px !important;
    }

    .issue-top-filter-item {
        min-width: 132px !important;
        flex: 0 0 132px !important;
    }

    .issue-top-filter-item > span {
        font-size: 9px !important;
        color: #697586 !important;
        text-transform: uppercase !important;
    }

    .issue-top-filter-item .header-dropdown {
        height: 30px !important;
        border-radius: 8px !important;
        font-size: 10px !important;
    }

    .issue-top-filter-buttons {
        flex: 0 0 auto !important;
        margin: 0 !important;
    }

    .issue-user-toolbar {
        display: none !important;
    }

    .issue-view-note {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        color: #667085;
        font-size: 12px;
        font-weight: 700;
    }

    .issue-toolbar-btn.filter-toggle {
        border-color: #dbe5ff !important;
        background: #eef4ff !important;
        color: #315bea !important;
    }

    .issue-toolbar-btn.filter-toggle[aria-expanded="true"],
    .issue-toolbar-btn.filter-toggle:hover {
        background: #315bea !important;
        color: #ffffff !important;
        border-color: #315bea !important;
    }

    #sharedFilterPanel.overview-platform-actions {
        padding: 10px 12px !important;
        margin-bottom: 10px !important;
        border-radius: 12px !important;
        box-shadow: 0 4px 16px rgba(15, 23, 42, 0.035) !important;
    }

    .issue-grid-scroll,
    .cmf-pending-grid-wrap {
        border-radius: 12px !important;
        border-color: #e3e9f2 !important;
        box-shadow: 0 6px 20px rgba(15, 23, 42, 0.045) !important;
    }

    #overall_request_details,
    #GridView_cmf_pending {
        font-size: 11px !important;
    }

    #overall_request_details thead th,
    #GridView_cmf_pending th {
        height: 38px !important;
        padding: 8px 10px !important;
        background: #fbfcff !important;
        color: #344054 !important;
        font-size: 9px !important;
        font-weight: 800 !important;
        text-transform: none !important;
        border-bottom: 1px solid #e5eaf2 !important;
    }

    #overall_request_details tbody td,
    #GridView_cmf_pending td {
        padding: 9px 10px !important;
        font-size: 11px !important;
        line-height: 1.35 !important;
        border-bottom: 1px solid #eef2f7 !important;
    }

    #overall_request_details tbody tr:nth-child(even),
    #GridView_cmf_pending tr:nth-child(even) td {
        background: #fcfdff !important;
    }

    #overall_request_details tbody tr:hover td,
    #GridView_cmf_pending tr:hover td {
        background: #f3f7ff !important;
    }

    #overall_request_details .field-issue_details {
        min-width: 250px !important;
        max-width: 330px !important;
    }

    #overall_request_details .field-days_open .days-open,
    #overall_request_details .field-days_open span {
        min-height: 21px !important;
        padding: 2px 7px !important;
        font-size: 10px !important;
    }

    .pending-tab-shell {
        grid-template-columns: minmax(0, 1fr) 250px !important;
    }

    #issueTabShell.issue-tab-shell,
    #pendingTabShell.pending-tab-shell,
    #issueTabShell.issue-tab-shell.side-hidden,
    #pendingTabShell.pending-tab-shell.side-hidden {
        grid-template-columns: minmax(0, 1fr) !important;
        width: 100% !important;
        max-width: none !important;
    }

    #issueTabShell .issue-tab-main,
    #pendingTabShell .pending-tab-main {
        width: 100% !important;
        max-width: none !important;
    }

    #issueTabShell .issue-side-panel,
    #pendingTabShell .pending-side-panel {
        display: none !important;
    }

    .pending-tab-main .issue-top-filter-panel,
    .pending-tab-main .issue-user-toolbar {
        display: none !important;
    }

    .portal-view-hidden {
        display: none !important;
    }

    /* Final cleanup from PM feedback */
    .portal-left-nav.is-collapsed {
        width: 58px !important;
        flex: 0 0 58px !important;
        min-width: 58px !important;
        max-width: 58px !important;
        padding: 10px 7px !important;
    }

    .portal-left-nav.is-collapsed .portal-nav-active-title {
        display: none !important;
    }

    .portal-left-nav.is-collapsed .portal-nav-link {
        width: 42px !important;
        min-width: 42px !important;
        height: 42px !important;
        padding: 0 !important;
        margin: 0 auto 8px !important;
        justify-content: center !important;
        border-radius: 12px !important;
    }

    .portal-left-nav.is-collapsed .portal-nav-link span {
        display: none !important;
    }

    .portal-left-nav.is-collapsed .portal-nav-link i {
        margin: 0 !important;
        font-size: 16px !important;
    }

    .portal-layout-shell,
    .portal-layout-shell.main-menu-collapsed {
        gap: 14px !important;
    }

    .issue-grid-scroll {
        padding: 0 !important;
        border: none !important;
        border-radius: 0 !important;
        background: transparent !important;
        box-shadow: none !important;
        max-height: min(62vh, 620px) !important;
        min-height: 0 !important;
        overflow-x: auto !important;
        overflow-y: scroll !important;
        scrollbar-gutter: stable !important;
    }

    .issue-grid-scroll:has(#overall_request_details[style*="display: none"]),
    .issue-grid-scroll:has(#overall_request_details[style*="display:none"]) {
        display: none !important;
    }

    .issue-grid-inner #overall_request_details,
    #overall_request_details {
        border: 1px solid #e4eaf3 !important;
        border-radius: 12px !important;
        box-shadow: 0 8px 22px rgba(15, 23, 42, 0.045) !important;
    }

    .cmf-pending-page-hd {
        margin-top: 0 !important;
        margin-bottom: 12px !important;
    }

    .cmf-pending-grid-wrap {
        padding: 0 !important;
        border: none !important;
        border-radius: 0 !important;
        background: transparent !important;
        box-shadow: none !important;
        max-height: min(64vh, 640px) !important;
        min-height: 0 !important;
        overflow: auto !important;
        scrollbar-gutter: stable !important;
    }

    .cmf-pending-grid-wrap:has(#GridView_cmf_pending[style*="display: none"]),
    .cmf-pending-grid-wrap:has(#GridView_cmf_pending[style*="display:none"]) {
        display: none !important;
    }

    .issue-grid-scroll #overall_request_details thead th,
    .cmf-pending-grid-wrap #GridView_cmf_pending th {
        position: sticky !important;
        top: 0 !important;
        z-index: 20 !important;
    }

    .issue-pager-controls {
        position: static !important;
        background: rgba(246, 248, 252, 0.94) !important;
        border: 1px solid #e3eaf3 !important;
        border-radius: 12px !important;
        padding: 8px 10px !important;
        margin-top: 10px !important;
        box-shadow: 0 6px 18px rgba(15, 23, 42, 0.05) !important;
    }

    .issue-pager-controls .issue-page-size-control {
        position: static !important;
        grid-column: 3 !important;
        justify-self: end !important;
    }

    .issue-pager-controls .issue-page-nav {
        grid-column: 2 !important;
    }

    @media (max-width: 860px) {
        .issue-pager-controls {
            grid-template-columns: 1fr !important;
            justify-items: center !important;
        }

        .issue-pager-controls .issue-page-size-control {
            grid-column: 1 !important;
            justify-self: center !important;
        }

        .issue-pager-controls .issue-page-nav {
            grid-column: 1 !important;
        }
    }

    #GridView_cmf_pending {
        min-width: 1040px !important;
        border: 1px solid #e4eaf3 !important;
        border-radius: 12px !important;
        overflow: hidden !important;
        box-shadow: 0 8px 22px rgba(15, 23, 42, 0.045) !important;
        background: #ffffff !important;
    }

    #GridView_cmf_pending th {
        height: 40px !important;
        padding: 9px 12px !important;
        background: #f8fbff !important;
        color: #233044 !important;
        border-bottom: 1px solid #e3eaf3 !important;
        font-size: 10px !important;
        font-weight: 850 !important;
        letter-spacing: 0 !important;
    }

    #GridView_cmf_pending td {
        padding: 10px 12px !important;
        color: #344054 !important;
        font-size: 11px !important;
        line-height: 1.35 !important;
        vertical-align: middle !important;
        border-bottom: 1px solid #eef2f7 !important;
    }

    #GridView_cmf_pending tr:nth-child(even) td {
        background: #fcfdff !important;
    }

    #GridView_cmf_pending tr:hover td {
        background: #f2f7ff !important;
    }

    #GridView_cmf_pending a {
        color: #2456d6 !important;
        font-weight: 750 !important;
        text-decoration: none !important;
    }

    #GridView_cmf_pending a:hover {
        text-decoration: underline !important;
    }

    @media (max-width: 980px) {
        header {
            height: auto !important;
            min-height: 66px !important;
            flex-wrap: wrap !important;
            padding: 10px 14px !important;
        }

        .header-title {
            min-width: 160px !important;
        }

        .global-search-shell {
            order: 3;
            width: 100%;
            min-width: 0;
        }

        .issue-kpi-row {
            grid-template-columns: repeat(2, minmax(140px, 1fr)) !important;
        }
    }

    /* Must stay last: enforce table-content scrolling after legacy overrides. */
    .issue-grid-scroll {
        max-height: none !important;
        min-height: 0 !important;
        overflow: hidden !important;
        scrollbar-gutter: stable !important;
    }

    .issue-grid-inner {
        width: 100% !important;
        max-width: 100% !important;
        max-height: min(62vh, 620px) !important;
        min-height: 0 !important;
        overflow-x: auto !important;
        overflow-y: auto !important;
        scrollbar-gutter: stable both-edges !important;
        -webkit-overflow-scrolling: touch;
    }

    .issue-grid-inner #overall_request_details,
    .issue-grid-inner > table,
    .issue-grid-inner .table-primary {
        width: max-content !important;
        min-width: 2200px !important;
        max-width: none !important;
    }

    .cmf-pending-grid-wrap {
        max-height: min(64vh, 640px) !important;
        min-height: 0 !important;
        overflow-x: auto !important;
        overflow-y: auto !important;
        scrollbar-gutter: stable !important;
    }

    .issue-grid-inner #overall_request_details thead th,
    .cmf-pending-grid-wrap #GridView_cmf_pending th {
        position: sticky !important;
        top: 0 !important;
        z-index: 20 !important;
    }
    #pendingTabShell.pending-tab-shell,
    #pendingTabShell.pending-tab-shell.side-hidden {
        width: 100% !important;
        max-width: none !important;
    }

    #pendingTabShell.pending-tab-shell.portal-view-hidden,
    #issueTabShell.issue-tab-shell.portal-view-hidden {
        display: none !important;
    }

    #pendingTabShell .cmf-pending-page-hd {
        margin-bottom: 16px !important;
    }

    .cmf-pending-breadcrumb {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-bottom: 8px;
        color: #2563eb;
        font-size: 12px;
        font-weight: 800;
    }

    .cmf-pending-breadcrumb span {
        color: #94a3b8;
    }

    .cmf-pending-actions {
        display: inline-flex;
        align-items: center;
        gap: 10px;
        flex-wrap: wrap;
    }

    .cmf-pending-action-btn,
    .cmf-pending-filter-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        min-height: 34px;
        padding: 0 14px;
        border-radius: 7px;
        border: 1px solid #b9d2ff;
        background: #ffffff;
        color: #0b5bd3;
        font-size: 12px;
        font-weight: 850;
        cursor: pointer;
        box-shadow: 0 5px 14px rgba(15, 63, 130, 0.08);
    }

    .cmf-pending-action-btn.primary,
    .cmf-pending-filter-btn.primary {
        background: #0054d1;
        border-color: #0054d1;
        color: #ffffff;
    }

    #pendingTabShell .template-kpi-row {
        display: none !important;

        grid-template-columns: repeat(4, minmax(160px, 1fr)) !important;
        gap: 14px !important;
        margin: 0 0 16px !important;
    }

    #pendingTabShell .template-kpi-card {
        display: grid;
        grid-template-columns: 42px minmax(0, 1fr);
        gap: 12px;
        align-items: center;
        min-height: 94px !important;
        padding: 16px !important;
        border: 1px solid #e5eaf2 !important;
        border-radius: 8px !important;
        background: #ffffff !important;
        box-shadow: 0 8px 24px rgba(15, 23, 42, 0.055) !important;
    }

    #pendingTabShell .template-kpi-card::before {
        display: none !important;
    }

    .template-kpi-icon {
        width: 42px;
        height: 42px;
        border-radius: 50%;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-size: 18px;
        background: #eef5ff;
        color: #1262dc;
    }

    .template-kpi-card.kpi-amber .template-kpi-icon { background: #fff4e5; color: #f28c18; }
    .template-kpi-card.kpi-violet .template-kpi-icon { background: #f3ecff; color: #7c3aed; }
    .template-kpi-card.kpi-green .template-kpi-icon { background: #e9f9f0; color: #059669; }

    #pendingTabShell .cmf-pending-kpi-label {
        margin: 0 0 2px !important;
        color: #1f2a44 !important;
        font-size: 12px !important;
        font-weight: 850 !important;
    }

    #pendingTabShell .cmf-pending-kpi-value {
        margin: 0 !important;
        color: #0f172a !important;
        font-size: 25px !important;
        line-height: 1 !important;
    }

    #pendingTabShell .template-kpi-card.kpi-amber .cmf-pending-kpi-value { color: #f28c18 !important; }
    #pendingTabShell .template-kpi-card.kpi-violet .cmf-pending-kpi-value { color: #5b21d5 !important; }
    #pendingTabShell .template-kpi-card.kpi-green .cmf-pending-kpi-value { color: #059669 !important; }

    #pendingTabShell .cmf-pending-kpi-sub {
        margin: 5px 0 0 !important;
        color: #64748b !important;
        font-size: 10px !important;
        font-weight: 750 !important;
    }

    .cmf-pending-filter-card {
        display: grid;
        grid-template-columns: minmax(220px, 1fr) repeat(4, minmax(130px, 0.55fr)) auto auto;
        gap: 10px;
        align-items: center;
        margin-bottom: 0;
        padding: 14px;
        border: 1px solid #e2e8f0;
        border-bottom: 0;
        border-radius: 10px 10px 0 0;
        background: #ffffff;
        box-shadow: 0 8px 24px rgba(15, 23, 42, 0.045);
    }

    .cmf-pending-filter-select {
        width: 100%;
        min-width: 0;
        height: 36px;
        border: 1px solid #d7dfeb;
        border-radius: 7px;
        background: #ffffff;
        color: #334155;
        font-size: 12px;
        font-weight: 750;
        padding: 0 10px;
    }

    .cmf-pending-searchbox {
        position: relative;
        display: flex;
        align-items: center;
        color: #64748b;
    }

    .cmf-pending-searchbox i {
        position: absolute;
        left: 12px;
        font-size: 12px;
        pointer-events: none;
    }

    .cmf-pending-searchbox input {
        width: 100%;
        height: 34px;
        padding: 0 12px 0 34px;
        border: 1px solid #dbe3ee;
        border-radius: 7px;
        background: #ffffff;
        color: #172033;
        font-size: 12px;
        font-weight: 650;
        outline: none;
    }

    .cmf-pending-searchbox input:focus {
        border-color: #6a9df8;
        box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.12);
    }

    #pendingTabShell .cmf-pending-grid-wrap {
        max-height: min(64vh, 650px) !important;
        border: 1px solid #e2e8f0 !important;
        border-radius: 0 0 10px 10px !important;
        background: #ffffff !important;
        box-shadow: 0 12px 28px rgba(15, 23, 42, 0.055) !important;
    }

    #pendingTabShell #GridView_cmf_pending {
        min-width: 1090px !important;
        border: 0 !important;
        border-radius: 0 !important;
        box-shadow: none !important;
        table-layout: fixed !important;
    }

    #pendingTabShell #GridView_cmf_pending th {
        height: 42px !important;
        padding: 10px 14px !important;
        background: #f8fafc !important;
        color: #1f2a44 !important;
        font-size: 11px !important;
        font-weight: 900 !important;
        border-bottom: 1px solid #e5edf6 !important;
    }

    #pendingTabShell #GridView_cmf_pending td {
        padding: 12px 14px !important;
        color: #26364a !important;
        font-size: 11px !important;
        line-height: 1.38 !important;
        vertical-align: middle !important;
        border-bottom: 1px solid #eef2f7 !important;
    }

    #pendingTabShell #GridView_cmf_pending tr:hover td {
        background: #f6faff !important;
    }

    #pendingTabShell .pending-title-text {
        color: #162033 !important;
        font-size: 11px !important;
        font-weight: 800 !important;
    }

    #pendingTabShell .pending-chip {
        min-height: 21px !important;
        padding: 3px 8px !important;
        border-radius: 7px !important;
        background: #eef4ff !important;
        color: #2556a3 !important;
    }

    #pendingTabShell .pending-ai-rec-btn {
        min-width: 126px !important;
        min-height: 38px !important;
        border-radius: 7px !important;
        background: #ecfdf3 !important;
        color: #087647 !important;
        border-color: #bcf0d0 !important;
    }

    @media (max-width: 980px) {
        #pendingTabShell .template-kpi-row {
            grid-template-columns: repeat(2, minmax(150px, 1fr)) !important;
        }

        .cmf-pending-filter-card {
            grid-template-columns: 1fr;
        }
    }

    </style>

    <script type="text/javascript">
        window.CMF_PORTAL = window.CMF_PORTAL || {};
        window.CMF_PORTAL.ids = window.CMF_PORTAL.ids || {};
        window.CMF_PORTAL.ids.hfSelectedValues = '<%= hfSelectedValues.ClientID %>';
        window.CMF_PORTAL.ids.exportBtn = '<%= exportBtn.ClientID %>';
        window.CMF_PORTAL.ids.driverCollectorhf = '<%= driverCollectorhf.ClientID %>';
        window.CMF_PORTAL.ids.hiddenComponentName = '<%= HiddenComponentName.ClientID %>';
        window.CMF_PORTAL.ids.hiddenDriverName = '<%= HiddenDriverName.ClientID %>';
        window.CMF_PORTAL.ids.hiddenIssueType = '<%= HiddenIssueType.ClientID %>';
        window.CMF_PORTAL.ids.hiddenModalDesign = '<%= HiddenModalDesign.ClientID %>';
        window.CMF_PORTAL.ids.hiddenModalDriver = '<%= HiddenModalDriver.ClientID %>';
        window.CMF_PORTAL.ids.hiddenInlineDesignName = '<%= HiddenInlineDesignName.ClientID %>';
        window.CMF_PORTAL.ids.hiddenInlineSwImageFreeze = '<%= HiddenInlineSwImageFreeze.ClientID %>';
        window.CMF_PORTAL.ids.hiddenInlineSupportModel = '<%= HiddenInlineSupportModel.ClientID %>';
        window.CMF_PORTAL.ids.ddlTables = '<%= ddlTables.ClientID %>';
        window.CMF_PORTAL.ids.ddlSharedPlatform = '<%= ddlSharedPlatform.ClientID %>';
        window.CMF_PORTAL.ids.hfQuickPlatform = '<%= hfQuickPlatform.ClientID %>';
        window.CMF_PORTAL.ids.hfQuickPlatformSourceTab = '<%= hfQuickPlatformSourceTab.ClientID %>';
        window.CMF_PORTAL.ids.hfOverallExcludedPlatforms = '<%= hfOverallExcludedPlatforms.ClientID %>';
        window.CMF_PORTAL.ids.btnQuickPlatformApply = '<%= btnQuickPlatformApply.UniqueID %>';
        window.CMF_PORTAL.ids.btnShowGridView2 = '<%= btnShowGridView2.UniqueID %>';
        window.CMF_PORTAL.ids.btnExportToExcelDesignSummary = '<%= btnExportToExcel_des_summary.UniqueID %>';
        window.CMF_PORTAL.ids.overallRequestDetails = '<%= overall_request_details.ClientID %>';
        window.CMF_PORTAL.ids.gridViewDesignSummaryModal0 = '<%= GridView_design_summary_modal0.ClientID %>';
        window.CMF_PORTAL.ids.gridViewSummaryMetricIssues = '<%= GridView_summaryMetricIssues.ClientID %>';
        window.CMF_PORTAL.ids.gridViewDesignSummary = '<%= GridView_design_summary.ClientID %>';
        window.CMF_PORTAL.ids.gridViewComponentSummary = '<%= GridView_component_summary.ClientID %>';
        window.CMF_PORTAL.ids.gridViewCmfSummary = '<%= GridView_cmf_summary.ClientID %>';
        window.CMF_PORTAL.ids.gridViewComp = '<%= GridView_comp.ClientID %>';
        window.CMF_PORTAL.ids.gridViewCmfPending = '<%= GridView_cmf_pending.ClientID %>';
        window.CMF_PORTAL.ids.homeWelcomePanel = '<%= homeWelcomePanel.ClientID %>';
        window.CMF_PORTAL.ids.homeCmfSummaryPanel = '<%= homeCmfSummaryPanel.ClientID %>';
        window.CMF_PORTAL.ids.paneIssueList = '<%= pane3.ClientID %>';
        window.CMF_PORTAL.ids.paneCmfPending = '<%= pane4.ClientID %>';
        window.CMF_PORTAL.ids.activeViewTitle = '<%= lblActiveViewTitle.ClientID %>';
        window.CMF_PORTAL.ids.issueListHeaderPanel = '<%= issueListHeaderPanel.ClientID %>';
        window.CMF_PORTAL.ids.cmfPendingHeaderPanel = '<%= cmf_pending_header_panel.ClientID %>';
        window.CMF_PORTAL.ids.lblIssueTotal = '<%= lblIssueTotal.ClientID %>';
        window.CMF_PORTAL.ids.lblIssueInProgress = '<%= lblIssueInProgress.ClientID %>';
        window.CMF_PORTAL.ids.lblIssueStale = '<%= lblIssueStale.ClientID %>';
        window.CMF_PORTAL.ids.lblPendingSightings = '<%= lblPendingSightings.ClientID %>';
        window.CMF_PORTAL.ids.lblQualifyForCmf = '<%= lblQualifyForCmf.ClientID %>';
        window.CMF_PORTAL.ids.lblLikelyDuplicate = '<%= lblLikelyDuplicate.ClientID %>';
        window.CMF_PORTAL.ids.lblIncompleteSysScope = '<%= lblIncompleteSysScope.ClientID %>';
        window.CMF_PORTAL.currentPlatform = '<%= ResolvePlatformTable(Session["selectedPlatform"] as string ?? ddlTables.SelectedValue) %>';
        window.CMF_PORTAL.currentDashboardPlatform = '<%= ResolveDashboardPlatformSelection() %>';
        window.CMF_PORTAL.overallPlatformTable = '<%= CmfPlatformCatalog.OverallPlatformTableName %>';
        window.CMF_PORTAL.availablePlatforms = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(AvailablePlatformTablesJson) %>');
        window.CMF_PORTAL.platformChipOptions = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(AvailablePlatformChipOptionsJson) %>');
        window.CMF_PORTAL.overallExcludedPlatforms = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(OverallExcludedPlatformsJson) %>');

        window.CMF_PORTAL.defaults = {
            componentName: '',
            issueType: 'Open',
            driverName: ''
        };

        window.CMF_PORTAL.driversJson = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(DriversJson) %>');
        window.CMF_PORTAL.isInitialLoad = '<%= IsPostBack ? "false" : "true" %>' === 'true';
        window.CMF_PORTAL.activeFocusedTab = '<%= GetActiveFocusedTab() %>';

        function isOverallDashboardSelected() {
            return String(window.CMF_PORTAL.currentDashboardPlatform || '') === String(window.CMF_PORTAL.overallPlatformTable || '');
        }

        function getOverallSelectablePlatforms() {
            var overallValue = String(window.CMF_PORTAL.overallPlatformTable || '');
            var available = window.CMF_PORTAL.availablePlatforms || [];
            var result = [];
            for (var i = 0; i < available.length; i++) {
                var value = String(available[i] || '');
                if (!value || value === overallValue) {
                    continue;
                }
                result.push(value);
            }
            return result;
        }

        function getOverallExcludedPlatformSet() {
            var source = window.CMF_PORTAL.overallExcludedPlatforms || [];
            var set = {};
            for (var i = 0; i < source.length; i++) {
                var value = String(source[i] || '');
                if (value) {
                    set[value] = true;
                }
            }
            return set;
        }

        function syncOverallExcludedPlatformsHidden() {
            var hidden = document.getElementById(window.CMF_PORTAL.ids.hfOverallExcludedPlatforms);
            if (!hidden) {
                return;
            }

            var values = window.CMF_PORTAL.overallExcludedPlatforms || [];
            hidden.value = values.join(',');
        }

        function setOverallExcludedPlatformsFromSet(set) {
            var values = [];
            for (var key in set) {
                if (Object.prototype.hasOwnProperty.call(set, key) && set[key]) {
                    values.push(key);
                }
            }
            values.sort();
            window.CMF_PORTAL.overallExcludedPlatforms = values;
            syncOverallExcludedPlatformsHidden();
        }

        function updateOverallPlatformChipStates() {
            var overallValue = String(window.CMF_PORTAL.overallPlatformTable || '');
            var overallSelected = isOverallDashboardSelected();
            var excludedSet = getOverallExcludedPlatformSet();
            var chips = document.querySelectorAll('.platform-chip');
            var rails = document.querySelectorAll('.platform-chip-rail[data-chip-source="server"]');

            for (var railIndex = 0; railIndex < rails.length; railIndex++) {
                rails[railIndex].classList.toggle('is-overall-selection', !!overallSelected);
            }

            for (var i = 0; i < chips.length; i++) {
                var chip = chips[i];
                var value = String(chip.getAttribute('data-platform') || '');
                var state = chip.querySelector('.platform-chip-state');
                chip.classList.remove('is-platform-included');
                chip.classList.remove('is-platform-excluded');

                if (!state || !overallSelected || value === overallValue || chip.style.display === 'none') {
                    if (state) {
                        state.textContent = '';
                        state.style.visibility = 'hidden';
                    }
                    continue;
                }

                var excluded = !!excludedSet[value];
                chip.classList.add(excluded ? 'is-platform-excluded' : 'is-platform-included');
                state.textContent = excluded ? 'x' : '✓';
                state.title = excluded
                    ? 'Excluded from Overall metrics. Click to include.'
                    : 'Included in Overall metrics. Click to exclude.';
                state.setAttribute('aria-label', state.title);
                state.style.visibility = 'visible';
            }
        }

        function toggleOverallPlatformInclusion(platformValue) {
            var overallValue = String(window.CMF_PORTAL.overallPlatformTable || '');
            var target = String(platformValue || '');

            if (!target || target === overallValue || !isOverallDashboardSelected()) {
                return;
            }

            var selectable = getOverallSelectablePlatforms();
            var selectableSet = {};
            for (var i = 0; i < selectable.length; i++) {
                selectableSet[selectable[i]] = true;
            }

            if (!selectableSet[target]) {
                return;
            }

            var excludedSet = getOverallExcludedPlatformSet();
            if (excludedSet[target]) {
                delete excludedSet[target];
            } else {
                var includedCount = 0;
                for (var key in selectableSet) {
                    if (Object.prototype.hasOwnProperty.call(selectableSet, key) && !excludedSet[key] && key !== target) {
                        includedCount++;
                    }
                }

                if (includedCount <= 0) {
                    alert('At least one platform must remain included in Overall metrics.');
                    return;
                }

                excludedSet[target] = true;
            }

            for (var pruneKey in excludedSet) {
                if (Object.prototype.hasOwnProperty.call(excludedSet, pruneKey) && !selectableSet[pruneKey]) {
                    delete excludedSet[pruneKey];
                }
            }

            setOverallExcludedPlatformsFromSet(excludedSet);
            updateOverallPlatformChipStates();
            applyQuickPlatform(overallValue);
        }

        function renderPlatformChipRails() {
            var options = window.CMF_PORTAL.platformChipOptions || [];
            var overallValue = String(window.CMF_PORTAL.overallPlatformTable || '');
            var rails = document.querySelectorAll('.platform-chip-rail[data-chip-source="server"]');

            for (var railIndex = 0; railIndex < rails.length; railIndex++) {
                var rail = rails[railIndex];
                while (rail.firstChild) {
                    rail.removeChild(rail.firstChild);
                }

                if (!options.length) {
                    rail.style.display = 'none';
                    continue;
                }

                rail.style.display = '';
                for (var optionIndex = 0; optionIndex < options.length; optionIndex++) {
                    var option = options[optionIndex];
                    if (!option || !option.value) {
                        continue;
                    }

                    var button = document.createElement('button');
                    button.type = 'button';
                    button.className = 'platform-chip';
                    button.setAttribute('data-platform', option.value);
                    button.setAttribute('aria-label', 'Switch to ' + (option.label || option.value));

                    var label = document.createElement('span');
                    label.className = 'platform-chip-label';
                    label.textContent = option.label || option.value;
                    button.appendChild(label);

                    var state = document.createElement('span');
                    state.className = 'platform-chip-state';
                    state.setAttribute('aria-hidden', 'true');
                    state.onclick = function (event) {
                        event.preventDefault();
                        event.stopPropagation();
                        toggleOverallPlatformInclusion(this.parentNode.getAttribute('data-platform'));
                    };
                    button.appendChild(state);

                    button.onclick = function (event) {
                        if (event && event.target && event.target.classList && event.target.classList.contains('platform-chip-state')) {
                            return;
                        }
                        applyQuickPlatform(this.getAttribute('data-platform'));
                    };

                    rail.appendChild(button);
                }
            }
        }

        function applyPlatformChipVisibility() {
            var available = window.CMF_PORTAL.availablePlatforms || [];
            if (!available.length) {
                return;
            }

            var availableSet = {};
            for (var i = 0; i < available.length; i++) {
                availableSet[available[i]] = true;
            }

            var chips = document.querySelectorAll('.platform-chip');
            for (var chipIndex = 0; chipIndex < chips.length; chipIndex++) {
                var value = chips[chipIndex].getAttribute('data-platform');
                chips[chipIndex].style.display = availableSet[value] ? '' : 'none';
            }

            updateOverallPlatformChipStates();
        }

        function syncActivePlatformChip() {
            var current = window.CMF_PORTAL.currentDashboardPlatform || window.CMF_PORTAL.currentPlatform || '';
            if (!current) {
                var ddl = document.getElementById(window.CMF_PORTAL.ids.ddlTables);
                current = ddl ? ddl.value : '';
            }
            var chips = document.querySelectorAll('.platform-chip');
            for (var i = 0; i < chips.length; i++) {
                chips[i].classList.toggle('is-active', chips[i].getAttribute('data-platform') === current);
            }

            updateOverallPlatformChipStates();
        }

        function applyQuickPlatform(platformValue) {
            if (!platformValue) return;
            var hidden = document.getElementById(window.CMF_PORTAL.ids.hfQuickPlatform);
            if (!hidden) return;

            var sourceHidden = document.getElementById(window.CMF_PORTAL.ids.hfQuickPlatformSourceTab);
            if (sourceHidden) {
                var sourceTab = (window.CMF_PORTAL.activeFocusedTab || '').toLowerCase();
                var homePanel = document.getElementById(window.CMF_PORTAL.ids.homeWelcomePanel);
                var cmfPanel = document.getElementById(window.CMF_PORTAL.ids.homeCmfSummaryPanel);
                var issuePane = document.getElementById(window.CMF_PORTAL.ids.paneIssueList);
                var pendingPane = document.getElementById(window.CMF_PORTAL.ids.paneCmfPending);

                if (isElementVisible(homePanel)) {
                    sourceTab = 'home';
                } else if (isElementVisible(cmfPanel)) {
                    sourceTab = 'cmf';
                } else if (isElementVisible(pendingPane)) {
                    sourceTab = 'pending';
                } else if (isElementVisible(issuePane)) {
                    sourceTab = 'issue';
                }

                sourceHidden.value = sourceTab || 'home';
            }

            hidden.value = platformValue;
            syncOverallExcludedPlatformsHidden();
            __doPostBack(window.CMF_PORTAL.ids.btnQuickPlatformApply, '');
        }

        function forcePortalPostBack(target) {
            var form = document.forms && document.forms.length > 0 ? document.forms[0] : null;
            if (!form || !target) {
                return false;
            }

            if (form.__EVENTTARGET) {
                form.__EVENTTARGET.value = target;
            }
            if (form.__EVENTARGUMENT) {
                form.__EVENTARGUMENT.value = '';
            }

            form.submit();
            return true;
        }

        function initPortalNavPostbackFallback() {
            var links = document.querySelectorAll('#mainMenuSidebar .portal-nav-link');
            for (var i = 0; i < links.length; i++) {
                var link = links[i];
                if (link.getAttribute('data-nav-fallback-bound') === '1') {
                    continue;
                }

                link.addEventListener('click', function (event) {
                    var target = this.name || this.id;
                    if (!target) {
                        return;
                    }
                    event.preventDefault();
                    forcePortalPostBack(target);
                });

                link.setAttribute('data-nav-fallback-bound', '1');
            }
        }

        function getCurrentPlatformValue() {
            var shared = document.getElementById(window.CMF_PORTAL.ids.ddlSharedPlatform);
            if (shared && shared.value) {
                return shared.value;
            }

            var ddl = document.getElementById(window.CMF_PORTAL.ids.ddlTables);
            if (ddl && ddl.value) {
                return ddl.value;
            }

            return window.CMF_PORTAL.currentPlatform || '';
        }

        function getIssuePendingPlatformValue() {
            var shared = document.getElementById(window.CMF_PORTAL.ids.ddlSharedPlatform);
            if (shared && shared.value) {
                return shared.value;
            }

            return getCurrentPlatformValue();
        }

        function renderMarkdown(text) {
            try {
                var htmlContent = marked.parse(text);
                var cleanHtml = DOMPurify.sanitize(htmlContent, {
                    ALLOWED_TAGS: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'p', 'br', 'strong', 'b', 'em', 'i',
                                   'ul', 'ol', 'li', 'code', 'pre', 'blockquote', 'a', 'hr', 'span', 'div'],
                    ALLOWED_ATTR: ['href', 'target', 'class']
                });
                return cleanHtml;
            } catch (error) {
                console.error('Error rendering markdown:', error);
                return DOMPurify.sanitize(text, { ALLOWED_TAGS: [] });
            }
        }

        function prepareAiSummaryForDrawer(summary) {
            var rawText = summary || '';
            var text = normalizeAiSummaryText(rawText);
            var confidence = '';
            var confidenceMatch = rawText.match(/\*\*\s*AI\s+summary\s*\(\s*Confidence\s*:\s*([0-9]{1,3}%?)\s*\)\s*\*\*/i)
                || rawText.match(/AI\s+summary\s*\(\s*Confidence\s*:\s*([0-9]{1,3}%?)\s*\)/i);

            if (confidenceMatch && confidenceMatch[1]) {
                confidence = confidenceMatch[1];
                if (confidence.indexOf('%') < 0) confidence += '%';
            }

            var impact = extractAiSummaryLabel(text, 'Impact');
            var status = extractAiSummaryLabel(text, 'Status');
            var reproducibility = extractAiSummaryLabel(text, 'Reproducibility');
            var logs = extractAiSummaryLabel(text, 'Logs\\(sysdebug(?:/debug details)?\\)');
            var rvpDebug = extractAiSummaryLabel(text, 'RVP platform debug details');
            var bodyParts = extractAiSummaryBodyParts(text);

            return {
                confidence: confidence,
                status: status,
                impact: impact,
                reproducibility: reproducibility,
                logs: logs,
                rvpDebug: rvpDebug,
                body: bodyParts.summary,
                followUp: bodyParts.followUp,
                hasDecisionSections: bodyParts.hasDecisionSections
            };
        }

        function normalizeAiSummaryText(text) {
            return String(text || '')
                .replace(/\r/g, '\n')
                .replace(/^\s*\*\*\s*(Summary|Key points|Issue Summary|Risk Assessment|Next Action|Escalation Warning|Decision Impact|Follow up)\s*:?\s*\*\*\s*$/gim, '$1:')
                .replace(/^\s*\*\*\s*AI\s+summary\s*\(\s*Confidence\s*:\s*[0-9]{1,3}%?\s*\)\s*\*\*\s*$/gim, '')
                .replace(/^\s*AI\s+summary\s*\(\s*Confidence\s*:\s*[0-9]{1,3}%?\s*\)\s*$/gim, '')
                .replace(/^\s*Sighting\s+ID\s*:\s*.*?CMF\s+Ask\s+date\s*:\s*.*$/gim, '');
        }

        function extractAiSummaryLabel(text, labelPattern) {
            var match = String(text || '').match(new RegExp('^\\s*' + labelPattern + '\\s*:\\s*(.+?)\\s*$', 'im'));
            return match && match[1] ? match[1].replace(/\*\*/g, '').trim() : 'N/A';
        }

        function extractAiSummaryBodyParts(text) {
            var source = normalizeAiSummaryText(text);
            source = source.replace(/^\s*Status\s*:\s*.*$/gim, '');
            source = source.replace(/^\s*Impact\s*:\s*.*$/gim, '');
            source = source.replace(/^\s*Reproducibility\s*:\s*.*$/gim, '');
            source = source.replace(/^\s*Logs\(sysdebug(?:\/debug details)?\)\s*:\s*.*$/gim, '');
            source = source.replace(/^\s*RVP platform debug details\s*:\s*.*$/gim, '');

            // For the new prompt format with headings ### Debug Summary and ### Next Steps
            var debugSummaryMatch = source.match(/(?:###\s*)?Debug Summary(?:\*\*)?\s*:?([\s\S]*?)(?=(?:###\s*)?Next Steps(?:\*\*)?\s*:?|$)/i);
            var nextStepsMatch = source.match(/(?:###\s*)?Next Steps(?:\*\*)?\s*:?([\s\S]*)$/i);

            if (debugSummaryMatch || nextStepsMatch) {
                 return {
                     summary: debugSummaryMatch && debugSummaryMatch[1] ? debugSummaryMatch[1].trim() : '',
                     followUp: nextStepsMatch && nextStepsMatch[1] ? nextStepsMatch[1].trim() : '- No further action identified from available details.',
                     hasDecisionSections: false
                 }
            }

            var decisionMatch = source.match(/(?:\*\*)?Issue Summary(?:\*\*)?\s*:?([\s\S]*)$/i);
            if (decisionMatch && decisionMatch[1]) {
                return {
                    summary: compactDecisionSummaryText('Issue Summary:' + decisionMatch[1]),
                    followUp: '',
                    hasDecisionSections: true
                };
            }
            var summaryMatch = source.match(/(?:\*\*)?(?:Summary|Key points)(?:\*\*)?\s*:?([\s\S]*?)(?=(?:\*\*)?(?:Follow up|Next action)(?:\*\*)?\s*:?|$)/i);
            var followMatch = source.match(/(?:\*\*)?(?:Follow up|Next action)(?:\*\*)?\s*:?([\s\S]*)$/i);
            return {
                summary: compactAiBulletText(summaryMatch && summaryMatch[1] ? summaryMatch[1] : source, 10, 0), // Allow up to 10 bullets instead of 3
                followUp: compactAiBulletText(followMatch && followMatch[1] ? followMatch[1] : '', 5, 0),
                hasDecisionSections: false
            };
        }

        function compactDecisionSummaryText(text) {
            var source = String(text || '').replace(/\r/g, '\n');
            var sectionNames = ['Issue Summary', 'Risk Assessment', 'Next Action', 'Escalation Warning', 'Decision Impact'];
            var output = [];
            for (var i = 0; i < sectionNames.length; i++) {
                var name = sectionNames[i];
                var nextNames = sectionNames.slice(i + 1).map(function (item) { return item.replace(/\s+/g, '\\s+'); }).join('|');
                var pattern = nextNames
                    ? new RegExp('(?:\\*\\*)?' + name.replace(/\s+/g, '\\s+') + '(?:\\*\\*)?\\s*:?([\\s\\S]*?)(?=(?:\\*\\*)?(?:' + nextNames + ')(?:\\*\\*)?\\s*:?|$)', 'i')
                    : new RegExp('(?:\\*\\*)?' + name.replace(/\s+/g, '\\s+') + '(?:\\*\\*)?\\s*:?([\\s\\S]*)$', 'i');
                var match = source.match(pattern);
                var value = match && match[1] ? match[1] : '';
                value = value.replace(/^\s*(?:[-*•]|\d+[\.)])\s*/gm, '').replace(/\*\*/g, '').replace(/\s+/g, ' ').trim();
                if (!value) value = 'Not identified from available details.';
                output.push('**' + name + '**\n- ' + value);
            }
            return output.join('\n\n');
        }

        function compactAiBulletText(text, maxBullets, maxLength) {
            var source = String(text || '').replace(/\r/g, '\n');
            source = source.replace(/^#{1,6}\s+.*$/gm, '');
            source = source.replace(/^\s*\*\*?(Summary|Key points|Follow up|Next action|Recommendation|Reasoning)\*\*?\s*:?\s*$/gim, '');
            var lines = source.split('\n');
            var bullets = [];
            for (var i = 0; i < lines.length && bullets.length < maxBullets; i++) {
                var line = lines[i].replace(/^\s*(?:[-*•]|\d+[\.)])\s*/, '').trim();
                line = line.replace(/\*\*/g, '').replace(/\s+/g, ' ');
                line = line.replace(/^\*+|\*+$/g, '').trim();
                if (/^(summary|key points|follow up|next action|recommendation|reasoning)\s*:?$/i.test(line)) continue;
                if (!line || /^(impact|reproducibility|logs|rvp platform debug details)\s*:/i.test(line)) continue;
                if (/^sighting id\s*:/i.test(line) || /^issue title\s*:/i.test(line)) continue;
                if (maxLength && line.length > maxLength) line = line.substring(0, maxLength).replace(/[\s,;:.]+$/g, '');
                if (bullets.indexOf(line) < 0) bullets.push(line);
            }
            if (bullets.length === 0 && source.trim()) bullets.push(maxLength ? source.trim().substring(0, maxLength) : source.trim());
            return bullets.map(function (line) { return '- ' + line; }).join('\n');
        }

        function compactRecommendationText(text) {
            return compactAiBulletText(text, 3, 0);
        }

        function openAiSummaryModal(issueId, title, submittedDate, status, sysdebug, mode) {
            var currentMode = mode || 'AI Summary';
            var isDetails = currentMode === 'AI issue details';

            var headingNode = document.getElementById('aiSummaryHeadingText');
            var badgesNode = document.getElementById('aiSummaryTitleBadges');
            var metaNode = document.getElementById('aiSummaryMeta');
            var issueIdNode = document.getElementById('aiSummaryIssueId');
            var titleNode = document.getElementById('aiSummaryTitle');
            var titleRow = document.getElementById('aiSummaryTitleRow');
            var dateNode = document.getElementById('aiSummarySubmittedDate');
            var bodyNode = document.getElementById('aiSummaryBody');
            var factsNode = document.getElementById('aiSummaryFacts');
            var actionsNode = document.getElementById('aiSummaryActions');
            var confidenceNode = document.getElementById('aiSummaryConfidence');
            var statusBadgeNode = document.getElementById('aiSummaryStatusBadge');
            var drawerBg = document.getElementById('aiSummaryDrawerBg');
            var drawer = document.getElementById('aiSummaryDrawer');

            if (!issueIdNode || !titleNode || !dateNode || !bodyNode || !drawerBg || !drawer) {
                return;
            }

            if (headingNode) {
                if (isDetails) {
                    // Inject sighting ID directly into the heading as requested, and reset original issueId node to not replicate it
                    headingNode.innerHTML = 'Issue Details <span style="font-size: 14px; font-weight: 500; color: #5c7087; margin-left: 20px;">Sighting ID: ' + (issueId || 'N/A') + '</span>';
                } else if (currentMode === 'AI Debug Summary') {
                    headingNode.textContent = 'AI Debug Summary';
                } else {
                    headingNode.textContent = currentMode;
                }
            }
            if (badgesNode) badgesNode.style.display = isDetails ? 'none' : '';
            if (metaNode) metaNode.style.display = isDetails ? 'none' : '';
            if (factsNode) factsNode.style.display = 'none';
            // Clear or format the meta string depending on mode
            issueIdNode.textContent = isDetails ? '' : (issueId || 'N/A');

            // Revert stylize override since it's now in the header itself
            issueIdNode.style.cssText = '';
            if (titleNode) titleNode.textContent = '';
            if (titleRow) titleRow.style.display = 'none';
            dateNode.textContent = submittedDate || 'N/A';
            if (confidenceNode) confidenceNode.textContent = 'Confidence: --';
            if (statusBadgeNode) {
                statusBadgeNode.textContent = 'Status: --';
                statusBadgeNode.className = 'ai-summary-status-value';
            }
            bodyNode.className = isDetails ? 'ai-summary-body issue-details-brief' : 'ai-summary-body markdown-content';

            if (isDetails || currentMode === 'AI Summary') {
                bodyNode.innerHTML = `
                    <div class="ai-skeleton-loader">
                        <div style="font-size: 13px; font-weight: 600; color: #5c7087; margin-bottom: 6px; display: flex; align-items: center; gap: 8px;">
                            &#10024; Generating ` + (isDetails ? 'AI issue details' : 'Debug Summary') + `...
                        </div>
                        <div class="ai-skeleton-line ai-skeleton-title" style="width: 50%;"></div>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-top: 10px;">
                            <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                            <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                            <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                            <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                        </div>
                        <div style="margin-top: 20px;">
                            <div class="ai-skeleton-line ai-skeleton-title" style="width: 30%; height: 16px;"></div>
                            <div class="ai-skeleton-line" style="margin-top: 10px; width: 100%;"></div>
                            <div class="ai-skeleton-line" style="margin-top: 8px; width: 95%;"></div>
                            <div class="ai-skeleton-line" style="margin-top: 8px; width: 90%;"></div>
                        </div>
                    </div>`;
            } else {
                bodyNode.textContent = 'Generating AI summary...';
            }
            if (actionsNode) actionsNode.style.cssText = 'display: none !important;';

            drawerBg.classList.add('show');
            drawer.classList.add('show');

            var payload = {
                issueId: issueId || '',
                title: title || '',
                submittedDate: submittedDate || '',
                status: status || '',
                sysdebug: sysdebug || '',
                platform: getIssuePendingPlatformValue(),
                mode: currentMode
            };

            // Store for regenerate / copy
            window._aiSummaryLastPayload = payload;
            window._aiSummaryLastText = '';

            // Ensure any previous chat UI is hidden/cleared immediately so chats don't leak
            try {
                var chatEl = document.getElementById('aiSummaryChat');
                var chatMsgs = document.getElementById('aiSummaryChatMessages');
                if (chatEl) chatEl.style.display = 'none';
                if (chatMsgs) chatMsgs.innerHTML = '';
                window._aiCurrentIssueId = null;
            } catch (e) {}

            fetchAiSummary(payload, bodyNode, actionsNode);
        }

        function fetchAiSummary(payload, bodyNode, actionsNode) {
            if (!bodyNode) bodyNode = document.getElementById('aiSummaryBody');
            if (!actionsNode) actionsNode = document.getElementById('aiSummaryActions');

            bodyNode.className = 'ai-summary-body';

            // Only inject the banner if it's the chat/summary mode; details mode already injected a shimmer.
            if (payload.mode !== 'AI issue details' || bodyNode.innerHTML.indexOf('ai-skeleton-loader') === -1) {
                if (payload.mode === 'AI issue details' || payload.mode === 'AI Summary') {
                    bodyNode.innerHTML = `
                        <div class="ai-skeleton-loader">
                            <div style="font-size: 13px; font-weight: 600; color: #5c7087; margin-bottom: 6px; display: flex; align-items: center; gap: 8px;">
                                &#10024; Generating ` + (payload.mode === 'AI issue details' ? 'AI issue details' : 'Debug Summary') + `...
                            </div>
                            <div class="ai-skeleton-line ai-skeleton-title" style="width: 50%;"></div>
                            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-top: 10px;">
                                <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                                <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                                <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                                <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                            </div>
                            <div style="margin-top: 20px;">
                                <div class="ai-skeleton-line ai-skeleton-title" style="width: 30%; height: 16px;"></div>
                                <div class="ai-skeleton-line" style="margin-top: 10px; width: 100%;"></div>
                                <div class="ai-skeleton-line" style="margin-top: 8px; width: 95%;"></div>
                                <div class="ai-skeleton-line" style="margin-top: 8px; width: 90%;"></div>
                            </div>
                        </div>`;
                } else {
                    bodyNode.innerHTML = '<div class="ai-loading-banner">&#10024; Generating AI summary, please wait...</div>';
                }
            }

            fetch('CMF_Web_portal.aspx/GetIssueAiSummary', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    var errorMsg = (result && result.Message) ? result.Message : 'Unable to generate summary at this time.';
                    bodyNode.innerHTML = escapeHtml(errorMsg) + ' <a href="#" onclick="regenerateAiSummary(); return false;" style="color:var(--portal-primary); text-decoration:underline;">↻ Retry</a>';
                    bodyNode.className = 'ai-summary-body';
                    return;
                }
                var summary = result.Summary || 'No summary content returned.';
                var preparedSummary = prepareAiSummaryForDrawer(summary);
                if (result.Confidence && !preparedSummary.confidence) {
                    preparedSummary.confidence = result.Confidence + '%';
                }
                var confidenceNode = document.getElementById('aiSummaryConfidence');
                if (confidenceNode && preparedSummary.confidence) {
                    confidenceNode.textContent = 'Confidence: ' + preparedSummary.confidence;
                }
                var statusBadgeNode = document.getElementById('aiSummaryStatusBadge');
                if (statusBadgeNode) {
                    var summaryStatus = preparedSummary.status || 'N/A';
                    statusBadgeNode.textContent = 'Status: ' + summaryStatus;
                    statusBadgeNode.className = 'ai-summary-status-value ' + getAiStatusClass(summaryStatus);
                }
                updateIssueListConfidenceBadge(payload.issueId, preparedSummary.confidence);
                // Build the assistant message that represents the summary (chat-first layout)
                var assistantSummary = '';
                if (payload.mode === 'AI issue details') {
                    assistantSummary = prepareIssueDetailsForDrawer(summary);
                } else if (preparedSummary.hasDecisionSections) {
                    assistantSummary = preparedSummary.body;
                } else {
                    assistantSummary = '### Debug Summary\n\n' + preparedSummary.body + '\n\n### Next Steps\n' + (preparedSummary.followUp || '- No further action identified from available details.');
                }

                window._aiSummaryLastText = payload.mode === 'AI issue details' ? summary : assistantSummary;

                // Hide heavy fixed copy/regen buttons permanently when in chat
                if (actionsNode) {
                    actionsNode.style.cssText = 'display: none !important;';
                }

                // Chat-first UI: hide the standalone body and render the summary as the first assistant message
                if (!window._aiChatStore) window._aiChatStore = {}; // issueId => [{role,text}, ...]
                window._aiCurrentIssueId = (payload.issueId || '') + '_' + (payload.mode || 'AI Summary');

                var chat = document.getElementById('aiSummaryChat');
                var messages = document.getElementById('aiSummaryChatMessages');
                if (chat && messages) {
                    chat.style.display = '';
                    // Hide the separate body node so the drawer looks like a chat window
                    if (bodyNode) bodyNode.style.display = 'none';

                    // If we have stored conversation for this issue, render it; otherwise seed with summary
                    var hist = window._aiChatStore[window._aiCurrentIssueId];
                    messages.innerHTML = '';
                    if (hist && hist.length) {
                        for (var i = 0; i < hist.length; i++) {
                            var m = hist[i];
                            appendAiChatMessage(m.role, m.text, /*store*/ false);
                        }
                    } else {
                        // seed with assistant summary message
                        window._aiChatStore[window._aiCurrentIssueId] = [{ role: 'assistant', text: assistantSummary }];
                        appendAiChatMessage('assistant', assistantSummary, /*store*/ false, /*autoScroll*/ false);
                    }

                    var inputBox = document.getElementById('aiSummaryChatInput');
                    if (inputBox) {
                        inputBox.onkeydown = function (evt) {
                            if ((evt.key === 'Enter' || evt.keyCode === 13) && !evt.shiftKey) {
                                evt.preventDefault();
                                sendAiFollowUp();
                            }
                        };
                    }
                }
            })
            .catch(function () {
                bodyNode.innerHTML = 'Error while calling summary service. <a href="#" onclick="regenerateAiSummary(); return false;" style="color:var(--portal-primary); text-decoration:underline;">↻ Retry</a>';
                bodyNode.className = 'ai-summary-body';
            });
        }

        function updateIssueListConfidenceBadge(issueId, confidence) {
            if (!issueId || !confidence) return;
            var selector = '.status-confidence-pill[data-ai-confidence-issue="' + cssEscapeValue(issueId) + '"]';
            var nodes = document.querySelectorAll(selector);
            for (var i = 0; i < nodes.length; i++) {
                nodes[i].textContent = 'Confidence: ' + confidence;
                nodes[i].classList.remove('status-confidence-empty');
            }
        }

        function prepareIssueDetailsForDrawer(summary) {
            var text = String(summary || '').replace(/\r/g, '\n').replace(/\*\*/g, '');
            var getField = function(label) {
                var m = text.match(new RegExp('^(?:-\\s*)?' + label + '\\s*[:\\-]\\s*(.*)$', 'im'));
                return m ? escapeHtml(m[1].trim()) : 'N/A';
            };

            var html = '<div class="issue-details-grid">';

            // Build badges based on values to colorize them semantically
            var colorBadge = function(val, type) {
                var v = String(val).toLowerCase();
                if (v === 'n/a' || v === 'none' || v === 'no' || v === 'unknown' || v === '' || v === 'tbd') return `<span class="ai-semantic-badge badge-neutral">${val}</span>`;
                if (v.includes('not actioned') || v.includes('no promoted ticket yet')) return `<span class="ai-semantic-badge badge-neutral">${val}</span>`;
                if (v.includes('must fix') || v.includes('must_fix') || v.includes('blocker') || v.includes('critical')) return `<span class="ai-semantic-badge badge-red">${val}</span>`;

                if (type === 'repro') {
                    if (v.includes('100%') || v.includes('always')) return `<span class="ai-semantic-badge badge-red">${val}</span>`;
                    return `<span class="ai-semantic-badge badge-amber">${val}</span>`;
                }
                if (type === 'state') {
                    if (v.includes('rejected') || v.includes('won\'t fix')) return `<span class="ai-semantic-badge badge-neutral">${val}</span>`;
                    if (v.includes('approved') || v.includes('fixed')) return `<span class="ai-semantic-badge badge-green">${val}</span>`;
                }
                if (type === 'workaround') return `<span class="ai-semantic-badge badge-green">${val}</span>`;
                return val;
            };

            html += `<div class="ai-sk-block"><div class="idg-label">Problem / Issue</div><div class="idg-val">${colorBadge(getField('(?:Problem|Issue).*?'), 'problem')}</div></div>`;
            html += `<div class="ai-sk-block"><div class="idg-label">Current State</div><div class="idg-val">${colorBadge(getField('Current State'), 'state')}</div></div>`;
            html += `<div class="ai-sk-block"><div class="idg-label">Scenario / Trigger</div><div class="idg-val">${getField('Scenario.*?')}</div></div>`;
            html += `<div class="ai-sk-block"><div class="idg-label">Reproduction</div><div class="idg-val">${colorBadge(getField('Reproduction'), 'repro')}</div></div>`;

            html += `<div class="ai-sk-block issue-details-grid-full"><div class="idg-label">Affected Platform / System</div><div class="idg-val">${getField('Affected Platform.*?')}</div></div>`;
            html += `<div class="ai-sk-block issue-details-grid-full"><div class="idg-label">Environment</div><div class="idg-val">${getField('Environment.*?')}</div></div>`;
            html += `<div class="ai-sk-block issue-details-grid-full"><div class="idg-label">Current Investigation</div><div class="idg-val">${getField('Current Investigation')}</div></div>`;

            html += `<div class="ai-sk-block"><div class="idg-label">Suspected Root Cause</div><div class="idg-val">${getField('Suspected Root Cause')}</div></div>`;
            html += `<div class="ai-sk-block"><div class="idg-label">Workaround</div><div class="idg-val">${colorBadge(getField('Workaround'), 'workaround')}</div></div>`;
            html += `<div class="ai-sk-block issue-details-grid-full"><div class="idg-label">Evidence / Logs</div><div class="idg-val">${getField('Evidence.*?')}</div></div>`;

            html +=`</div>`; // end grid

            // Check for promoted ticket presence before blindly yielding N/A
            var promotedId = getField('Promoted ID');
            var isPromoted = promotedId !== 'N/A' && promotedId.toLowerCase() !== 'missing' && promotedId.toLowerCase() !== 'none' && promotedId !== '';

            var owner = isPromoted ? getField('Owner') : 'No promoted ticket yet';
            var fixedVer = isPromoted ? getField('(?:Fixed Version|Closure).*?') : 'No promoted ticket yet';
            var lastAct = isPromoted ? getField('Latest Activity') : 'No promoted ticket yet';
            if (!isPromoted) promotedId = 'No promoted ticket yet';

            html += `<div style="margin-top: 15px; padding-top: 15px; border-top: 1px solid #e0e2e5;">`;
            html += `<h4 style="font-size:12px; margin-bottom: 10px; color:#5c7087;">LINKED HSD / CMF DATA</h4>`;
            html += `<div class="issue-details-grid">`;
            html += `<div class="ai-sk-block"><div class="idg-label">Promoted ID</div><div class="idg-val">${promotedId}</div></div>`;
            html += `<div class="ai-sk-block"><div class="idg-label">Owner</div><div class="idg-val">${owner}</div></div>`;
            html += `<div class="ai-sk-block issue-details-grid-full"><div class="idg-label">Fixed Version / Closure</div><div class="idg-val">${fixedVer}</div></div>`;
            html += `<div class="ai-sk-block issue-details-grid-full"><div class="idg-label">Latest Activity</div><div class="idg-val">${lastAct}</div></div>`;
            html += `</div></div>`;

            return html;
        }

        function cssEscapeValue(value) {
            if (window.CSS && typeof window.CSS.escape === 'function') {
                return window.CSS.escape(String(value));
            }
            return String(value).replace(/\\/g, '\\\\').replace(/"/g, '\\"');
        }

        function updateAiSummaryFacts(container, summary, isLoading) {
            if (!container) return;
            var value = function (name) {
                if (isLoading) return 'Loading...';
                return summary && summary[name] ? summary[name] : 'N/A';
            };
            container.innerHTML =
                '<div class="ai-summary-fact"><span class="ai-summary-fact-label">Impact</span><span class="ai-summary-fact-value">' + escapeHtml(value('impact')) + '</span></div>' +
                '<div class="ai-summary-fact"><span class="ai-summary-fact-label">Reproducibility</span><span class="ai-summary-fact-value">' + escapeHtml(value('reproducibility')) + '</span></div>' +
                '<div class="ai-summary-fact"><span class="ai-summary-fact-label">Logs(sysdebug)</span><span class="ai-summary-fact-value">' + escapeHtml(value('logs')) + '</span></div>' +
                '<div class="ai-summary-fact"><span class="ai-summary-fact-label">RVP platform debug details</span><span class="ai-summary-fact-value">' + escapeHtml(value('rvpDebug')) + '</span></div>';
        }

        function getAiStatusClass(statusValue) {
            var text = String(statusValue || '').toLowerCase();
            if (/reject/.test(text)) return 'status-rejected';
            if (/closed|complete|implemented|fixed|resolved/.test(text)) return 'status-closed';
            if (/pending|review|ask|triage/.test(text)) return 'status-pending';
            if (/open|active/.test(text)) return 'status-open';
            if (/progress|debug|validation/.test(text)) return 'status-progress';
            return '';
        }

        function copyAiSummaryForReport() {
            var text = window._aiSummaryLastText || '';
            if (!text) return;
            var issueId = (document.getElementById('aiSummaryIssueId') || {}).textContent || '';
            var title = (document.getElementById('aiSummaryTitle') || {}).textContent || '';
            var date = (document.getElementById('aiSummarySubmittedDate') || {}).textContent || '';
            var full = issueId + ' — ' + title + '\nCMF Ask Date: ' + date + '\n\n' + text;
            if (navigator.clipboard) {
                navigator.clipboard.writeText(full).then(function () {
                    showPortalToast('Summary copied — ready to paste into your report');
                }).catch(function () {
                    showPortalToast('Copy failed — try selecting text manually');
                });
            } else {
                showPortalToast('Clipboard not available in this browser');
            }
        }

        function regenerateAiSummary() {
            var bodyNode = document.getElementById('aiSummaryBody');
            var actionsNode = document.getElementById('aiSummaryActions');
            if (!bodyNode || !window._aiSummaryLastPayload) return;
            bodyNode.textContent = 'Regenerating AI summary...';
            bodyNode.className = 'ai-summary-body markdown-content';
            if (actionsNode) actionsNode.style.display = 'none';
            fetchAiSummary(window._aiSummaryLastPayload, bodyNode, actionsNode);
        }

        function appendAiChatMessage(role, text, storeMessage, autoScroll = true) {
            if (typeof storeMessage === 'undefined') storeMessage = true;
            var container = document.getElementById('aiSummaryChatMessages');
            if (!container) return;
            var msg = document.createElement('div');
            msg.className = 'ai-chat-message ai-chat-' + (role === 'user' ? 'user' : 'assistant');
            var inner = document.createElement('div');
            inner.className = 'ai-chat-message-body';

            // Check if text already contains our injected structured HTML wrappers (e.g. from prepareIssueDetailsForDrawer)
            if (text.indexOf('class="issue-details-grid"') > -1) {
                inner.innerHTML = text; // Inject raw HTML
            } else {
                inner.innerHTML = renderMarkdown(escapeHtml(text).replace(/\n/g, '\n'));
            }

            if (role === 'assistant' && window._aiSummaryLastPayload) {
                var isAI_IssueDetails = window._aiSummaryLastPayload.mode === 'AI issue details';
                var isAI_Summary = window._aiSummaryLastPayload.mode === 'AI Summary';

                if (isAI_IssueDetails || isAI_Summary) {
                    var actionRibbon = document.createElement('div');
                    actionRibbon.className = 'ai-chat-msg-actions';
                    actionRibbon.style.cssText = 'display: flex; gap: 12px; margin-top: 8px; justify-content: flex-end; align-items: center; color: #5c7087; font-size: 13px;';

                    var retryLink = document.createElement('a');
                    retryLink.href = '#';
                    retryLink.onclick = function(e) {
                         e.preventDefault();
                         if (storeMessage === false && window._aiSummaryLastText === text) {
                            regenerateAiSummary();
                         } else {
                             // Just regenerating follow-up if applicable, or fallback to main
                             regenerateAiSummary();
                         }
                    };
                    retryLink.textContent = '↻ Regenerate';
                    retryLink.style.cssText = 'color: #0071c5; text-decoration: none; font-weight: 500; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: color 0.1s;';
                    retryLink.title = 'Regenerate this response';
                    retryLink.onmouseover = function() { this.style.color = '#005A9E'; };
                    retryLink.onmouseout = function() { this.style.color = '#0071c5'; };

                    var copyIcon = document.createElement('a');
                    copyIcon.href = '#';
                    copyIcon.onclick = function(e) {
                        e.preventDefault();
                        if (!navigator.clipboard) return;
                        // strip html tags for clean copy
                        var tmp = document.createElement('div');
                        tmp.innerHTML = inner.innerHTML;
                        // Remove action ribbon text itself during copy
                        var ribbonCopy = tmp.querySelector('.ai-chat-msg-actions');
                        if(ribbonCopy) tmp.removeChild(ribbonCopy);

                        navigator.clipboard.writeText(tmp.textContent || tmp.innerText).then(function() {
                            var o = copyIcon.innerHTML;
                            copyIcon.innerHTML = '&#10003;';
                            setTimeout(function(){ copyIcon.innerHTML = o; }, 2000);
                        });
                    };
                    copyIcon.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align: text-bottom;"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg>';
                    copyIcon.style.cssText = 'color: #5c7087; margin-left: 5px; text-decoration: none; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: color 0.1s;';
                    copyIcon.title = 'Copy response text';
                    copyIcon.onmouseover = function() { this.style.color = '#0071c5'; };
                    copyIcon.onmouseout = function() { this.style.color = '#5c7087'; };

                    actionRibbon.appendChild(retryLink);
                    actionRibbon.appendChild(copyIcon);
                    inner.appendChild(actionRibbon);
                }
            }

            msg.appendChild(inner);
            container.appendChild(msg);

            if (autoScroll) {
                container.scrollTop = container.scrollHeight;
            } else {
                container.scrollTop = 0;
            }

            // store in per-issue conversation if requested
            try {
                var id = window._aiCurrentIssueId || '';
                if (storeMessage && id) {
                    if (!window._aiChatStore) window._aiChatStore = {};
                    if (!window._aiChatStore[id]) window._aiChatStore[id] = [];
                    window._aiChatStore[id].push({ role: role, text: text });
                }
            } catch (e) {
                // ignore storage errors
            }
        }

        function sendAiFollowUp() {
            var input = document.getElementById('aiSummaryChatInput');
            if (!input) return;
            var question = input.value || '';
            if (!question.trim()) return;

            // show user's question and store it
            appendAiChatMessage('user', question.trim(), true);
            input.value = '';
            input.disabled = true;
            var sendBtn = document.getElementById('aiSummaryChatSend');
            if (sendBtn) sendBtn.disabled = true;

            // build payload from last payload
            var payload = window._aiSummaryLastPayload || {};
            payload.question = question.trim();

            var loadingId = 'loading-' + new Date().getTime();
            var container = document.getElementById('aiSummaryChatMessages');
            if (container) {
                var loadMsg = document.createElement('div');
                loadMsg.id = loadingId;
                loadMsg.className = 'ai-chat-message ai-chat-assistant';
                loadMsg.innerHTML = '<div class="ai-chat-message-body" style="font-style: italic;">Thinking...</div>';
                container.appendChild(loadMsg);
                container.scrollTop = container.scrollHeight;
            }

            fetch('CMF_Web_portal.aspx/AskIssueAiFollowUp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var loadingEl = document.getElementById(loadingId);
                if (loadingEl) loadingEl.parentNode.removeChild(loadingEl);

                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    appendAiChatMessage('assistant', (result && result.Message) ? result.Message : 'Unable to answer follow-up at this time.', true);
                } else {
                    appendAiChatMessage('assistant', result.Summary || 'No answer returned.', true);
                }
            })
            .catch(function () {
                appendAiChatMessage('assistant', 'Error while calling follow-up service.');
            })
            .finally(function () {
                if (input) input.disabled = false;
                if (sendBtn) sendBtn.disabled = false;
            });
        }

        function showPortalToast(msg) {
            var t = document.getElementById('aiSummaryToast');
            if (!t) {
                t = document.createElement('div');
                t.id = 'aiSummaryToast';
                t.style.cssText = 'position:fixed;bottom:22px;left:50%;transform:translateX(-50%);background:#1b2a3a;color:#fff;padding:10px 18px;border-radius:8px;font-size:13px;z-index:2000;opacity:0;transition:opacity .25s;pointer-events:none';
                document.body.appendChild(t);
            }
            t.textContent = msg;
            t.style.opacity = '1';
            clearTimeout(t._tid);
            t._tid = setTimeout(function () { t.style.opacity = '0'; }, 2500);
        }

        function closeAiSummaryDrawer() {
            var drawerBg = document.getElementById('aiSummaryDrawerBg');
            var drawer = document.getElementById('aiSummaryDrawer');
            if (!drawerBg || !drawer) {
                return;
            }

            drawerBg.classList.remove('show');
            drawer.classList.remove('show');

            // Reset chat/body UI and current issue context so conversations don't leak between issues
            try {
                window._aiCurrentIssueId = null;
                var chat = document.getElementById('aiSummaryChat');
                var messages = document.getElementById('aiSummaryChatMessages');
                var bodyNode = document.getElementById('aiSummaryBody');
                if (chat) chat.style.display = 'none';
                if (messages) messages.innerHTML = '';
                if (bodyNode) {
                    bodyNode.style.display = '';
                    bodyNode.textContent = '';
                }
            } catch (e) {
                // ignore
            }
        }

        document.addEventListener('keydown', function (event) {
            if (event.key !== 'Escape') {
                return;
            }

            var drawer = document.getElementById('aiSummaryDrawer');
            if (drawer && drawer.classList.contains('show')) {
                closeAiSummaryDrawer();
            }

            var recommendationDrawer = document.getElementById('cmfRecDrawer');
            if (recommendationDrawer && recommendationDrawer.classList.contains('show')) {
                closeCmfRecDrawer();
            }
        });

        function escapeHtml(value) {
            return String(value == null ? '' : value)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#39;');
        }

        function openCmfPendingRecommendationModal(cpId, title, component, componentGroup, cmfRequest, impact, idst, reproOnRvp, reproducibility, customerDetail, customerOwner) {
            var cpIdNode = document.getElementById('cmfRecCpId');
            var titleNode = document.getElementById('cmfRecTitle');
            var componentNode = document.getElementById('cmfRecComponent');
            var sightingIdNode = document.getElementById('cmfAssistantSightingId');
            var sightingHsdLinkNode = document.getElementById('cmfAssistantHsdLink');
            var sightingDescriptionNode = document.getElementById('cmfAssistantSightingDescription');
            var platformNode = document.getElementById('cmfAssistantPlatform');
            var visibleComponentNode = document.getElementById('cmfAssistantComponent');
            var impactSummaryNode = document.getElementById('cmfAssistantImpactSummary');
            var qualityAssessmentNode = document.getElementById('cmfAssistantQualityAssessment');
            var recommendationNode = document.getElementById('cmfAssistantRecommendationBody');
            var recTagNode = document.getElementById('cmfAssistantRecTag');
            var bodyNode = document.getElementById('cmfRecBody');
            var drawerBg = document.getElementById('cmfRecDrawerBg');
            var drawer = document.getElementById('cmfRecDrawer');

            if (!cpIdNode || !titleNode || !componentNode || !drawerBg || !drawer) {
                console.error('CMF Recommendation modal elements not found');
                return;
            }

            var summaryDrawer = document.getElementById('aiSummaryDrawer');
            var summaryDrawerBg = document.getElementById('aiSummaryDrawerBg');
            if (summaryDrawer || summaryDrawerBg) {
                closeAiSummaryDrawer();
            }

            cpIdNode.textContent = cpId || 'N/A';
            titleNode.textContent = title || 'N/A';
            componentNode.textContent = component || 'N/A';
            if (sightingDescriptionNode) sightingDescriptionNode.innerHTML = renderIssueDescriptionSkeleton();
            if (impactSummaryNode) impactSummaryNode.innerHTML = renderImpactSummarySkeleton();
            if (recommendationNode) recommendationNode.innerHTML = renderCmfRecommendationSkeleton();
            if (recTagNode) {
                recTagNode.textContent = 'CMF_Tag';
                recTagNode.className = 'cmf-assistant-rec-tag';
            }

            var recNode = document.getElementById('cmfRecRecommendation');
            var evidenceNode = document.getElementById('cmfRecEvidence');
            var qualityNode = document.getElementById('cmfRecQuality');
            setCmfDrawerMode('recommendation');
            var headingNode = document.getElementById('cmfRecHeading');
            if (headingNode) headingNode.innerHTML = '<span class="cmf-assistant-title-icon" aria-hidden="true">✨</span>AI CMF Assistant';
            sightingIdNode = document.getElementById('cmfAssistantSightingId');
            sightingHsdLinkNode = document.getElementById('cmfAssistantHsdLink');
            platformNode = document.getElementById('cmfAssistantPlatform');
            visibleComponentNode = document.getElementById('cmfAssistantComponent');
            if (sightingIdNode) sightingIdNode.textContent = cpId || 'N/A';
            if (sightingHsdLinkNode) {
                if (cpId) {
                    sightingHsdLinkNode.href = 'https://hsdes.intel.com/appstore/article/#/' + encodeURIComponent(cpId);
                    sightingHsdLinkNode.style.display = 'inline-flex';
                    sightingHsdLinkNode.setAttribute('aria-label', 'Open sighting ' + cpId + ' in HSD');
                    sightingHsdLinkNode.setAttribute('title', 'Open sighting ' + cpId + ' in HSD');
                } else {
                    sightingHsdLinkNode.removeAttribute('href');
                    sightingHsdLinkNode.style.display = 'none';
                }
            }
            if (platformNode) platformNode.textContent = getPlatformDisplayLabel(getIssuePendingPlatformValue());
            if (visibleComponentNode) visibleComponentNode.textContent = component || 'N/A';

            if (recNode) recNode.style.display = 'none';
            if (evidenceNode) evidenceNode.innerHTML = '';
            if (qualityNode) qualityNode.textContent = 'Evidence Quality: --';

            drawerBg.classList.add('show');
            drawer.classList.add('show');

            var payload = {
                cpId: cpId || '',
                title: title || '',
                component: component || '',
                componentGroup: componentGroup || '',
                cmfRequest: cmfRequest || '',
                impact: impact || '',
                idst: idst || '',
                reproOnRvp: reproOnRvp || '',
                reproducibility: reproducibility || '',
                customerDetail: customerDetail || '',
                customerOwner: customerOwner || '',
                platform: getIssuePendingPlatformValue()
            };

            if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessmentSkeleton();

            fetch('CMF_Web_portal.aspx/GetSightingDescription', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ cpId: payload.cpId, title: payload.title, platform: payload.platform })
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!sightingDescriptionNode) return;
                if (!result || result.Success !== true) {
                    sightingDescriptionNode.innerHTML = '<span class="sighting-description-error">Description unavailable.</span>';
                    return;
                }
                sightingDescriptionNode.innerHTML = renderIssueDescriptionText(result.Summary || 'No description available.');
            })
            .catch(function () {
                if (sightingDescriptionNode) sightingDescriptionNode.innerHTML = '<span class="sighting-description-error">Description unavailable.</span>';
            });

            fetch('CMF_Web_portal.aspx/GetSightingImpactSummary', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ cpId: payload.cpId, title: payload.title, impact: payload.impact, platform: payload.platform })
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!impactSummaryNode) return;
                if (!result || result.Success !== true) {
                    impactSummaryNode.innerHTML = '<span class="sighting-description-error">Impact summary unavailable.</span>';
                    return;
                }
                impactSummaryNode.innerHTML = renderImpactSummaryText(result.Summary || 'No impact summary available.');
            })
            .catch(function () {
                if (impactSummaryNode) impactSummaryNode.innerHTML = '<span class="sighting-description-error">Impact summary unavailable.</span>';
            });

            fetch('CMF_Web_portal.aspx/GetSightingQualityAssessment', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessmentUnavailable();
                    return;
                }
                if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessment(result, payload);
            })
            .catch(function () {
                if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessmentUnavailable();
            });

            if (evidenceNode) {
                evidenceNode.innerHTML = '<div class="ai-skeleton-loader"><div style="font-size:13px;font-weight:600;color:#5c7087;">&#10024; Assessing sighting quality and CMF readiness...</div><div class="ai-skeleton-line ai-skeleton-title" style="width:55%;"></div><div class="ai-skeleton-line" style="width:95%;"></div><div class="ai-skeleton-line" style="width:88%;"></div></div>';
            }

            fetch('CMF_Web_portal.aspx/GetCmfPendingRecommendation', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    if (evidenceNode) evidenceNode.textContent = (result && result.Message) ? result.Message : 'Unable to generate CMF recommendation.';
                    if (recommendationNode) recommendationNode.innerHTML = '<span class="sighting-description-error">CMF recommendation unavailable.</span>';
                    return;
                }
                if (qualityNode) qualityNode.textContent = 'Sighting quality and CMF readiness';
                updatePendingRecommendationBadge(payload.cpId, result.Recommendation, result.OverallQualityScore);
                updateCmfAssistantTag(result.Recommendation);
                if (recommendationNode) recommendationNode.innerHTML = renderCmfAssistantRecommendation(result, payload);
                if (evidenceNode) evidenceNode.innerHTML = renderMergedCmfRecommendation(result, payload);
            })
            .catch(function () {
                if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = '<span class="sighting-description-error">Sighting quality assessment unavailable.</span>';
                if (recommendationNode) recommendationNode.innerHTML = '<span class="sighting-description-error">CMF recommendation unavailable.</span>';
                if (evidenceNode) evidenceNode.textContent = 'Error while calling CMF recommendation service.';
            });

        }

        function openCmfPendingDetailsModal(cpId, title, component, cmfRequest, impact, idst, reproOnRvp, reproducibility, customerDetail, customerOwner) {
            var cpIdNode = document.getElementById('cmfRecCpId');
            var titleNode = document.getElementById('cmfRecTitle');
            var componentNode = document.getElementById('cmfRecComponent');
            var drawerBg = document.getElementById('cmfRecDrawerBg');
            var drawer = document.getElementById('cmfRecDrawer');
            var headingNode = document.getElementById('cmfRecHeading');
            var recNode = document.getElementById('cmfRecRecommendation');
            var evidenceNode = document.getElementById('cmfRecEvidence');
            var qualityNode = document.getElementById('cmfRecQuality');
            var detailsBodyNode = document.getElementById('cmfDecisionDetailsBody');

            if (!cpIdNode || !titleNode || !componentNode || !drawerBg || !drawer) {
                console.error('CMF details modal elements not found');
                return;
            }

            setCmfDrawerMode('details');
            if (headingNode) headingNode.innerHTML = 'CMF Decision Details <span class="cmf-rec-heading-sighting">Sighting ID: ' + escapeHtml(cpId || 'N/A') + '</span>';
            cpIdNode.textContent = cpId || 'N/A';
            if (titleNode) titleNode.textContent = title || 'N/A';
            componentNode.textContent = component || 'N/A';
            if (recNode) recNode.style.display = 'none';
            if (qualityNode) qualityNode.textContent = 'CMF-focused issue brief';
            if (detailsBodyNode) detailsBodyNode.innerHTML = `
                <div class="ai-skeleton-loader">
                    <div style="font-size: 13px; font-weight: 600; color: #5c7087; margin-bottom: 6px; display: flex; align-items: center; gap: 8px;">
                        &#10024; Generating CMF Decision Details...
                    </div>
                    <div class="ai-skeleton-line ai-skeleton-title" style="width: 50%;"></div>
                    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-top: 10px;">
                        <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                        <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                        <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                        <div class="ai-sk-block"><div class="ai-skeleton-line ai-skeleton-label"></div><div class="ai-skeleton-line ai-skeleton-value"></div></div>
                    </div>
                    <div style="margin-top: 20px;">
                        <div class="ai-skeleton-line ai-skeleton-title" style="width: 30%; height: 16px;"></div>
                        <div class="ai-skeleton-line" style="margin-top: 10px; width: 100%;"></div>
                        <div class="ai-skeleton-line" style="margin-top: 8px; width: 95%;"></div>
                        <div class="ai-skeleton-line" style="margin-top: 8px; width: 90%;"></div>
                    </div>
                </div>`;

            drawerBg.classList.add('show');
            drawer.classList.add('show');

            var payload = {
                cpId: cpId || '',
                title: title || '',
                component: component || '',
                cmfRequest: cmfRequest || '',
                impact: impact || '',
                idst: idst || '',
                reproOnRvp: reproOnRvp || '',
                reproducibility: reproducibility || '',
                customerDetail: customerDetail || '',
                customerOwner: customerOwner || '',
                platform: getIssuePendingPlatformValue()
            };

            fetch('CMF_Web_portal.aspx/GetCmfPendingDecisionDetails', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    if (evidenceNode) evidenceNode.textContent = (result && result.Message) ? result.Message : 'Unable to generate CMF decision details.';
                    return;
                }
                if (detailsBodyNode) detailsBodyNode.innerHTML = renderCmfDecisionDetails(result.Evidence || 'No CMF decision details returned.', cpId);
            })
            .catch(function () {
                if (detailsBodyNode) detailsBodyNode.textContent = 'Error while calling CMF decision details service.';
            });
        }

        function openCmfPendingImpactModal(cpId, title, component, cmfRequest, impact, idst, reproOnRvp, reproducibility, customerDetail, customerOwner) {
            var cpIdNode = document.getElementById('cmfRecCpId');
            var titleNode = document.getElementById('cmfRecTitle');
            var componentNode = document.getElementById('cmfRecComponent');
            var drawerBg = document.getElementById('cmfRecDrawerBg');
            var drawer = document.getElementById('cmfRecDrawer');
            var headingNode = document.getElementById('cmfRecHeading');
            var recNode = document.getElementById('cmfRecRecommendation');
            var evidenceNode = document.getElementById('cmfDecisionDetailsBody');
            var qualityNode = document.getElementById('cmfRecQuality');
            var actionsNode = document.getElementById('cmfRecActions');

            if (!cpIdNode || !titleNode || !componentNode || !drawerBg || !drawer) {
                console.error('CMF impact modal elements not found');
                return;
            }

            setCmfDrawerMode('details');
            if (headingNode) headingNode.textContent = 'AI Impact Details';
            cpIdNode.textContent = cpId || 'N/A';
            titleNode.textContent = title || 'N/A';
            componentNode.textContent = component || 'N/A';
            if (recNode) recNode.textContent = 'Impact perspective';
            if (qualityNode) qualityNode.textContent = 'Issue-specific impact assessment';
            if (evidenceNode) evidenceNode.textContent = 'Generating AI impact details...';
            if (actionsNode) actionsNode.classList.add('cmf-rec-hidden');

            drawerBg.classList.add('show');
            drawer.classList.add('show');

            var payload = {
                cpId: cpId || '',
                title: title || '',
                component: component || '',
                cmfRequest: cmfRequest || '',
                impact: impact || '',
                idst: idst || '',
                reproOnRvp: reproOnRvp || '',
                reproducibility: reproducibility || '',
                customerDetail: customerDetail || '',
                customerOwner: customerOwner || '',
                platform: getIssuePendingPlatformValue()
            };

            fetch('CMF_Web_portal.aspx/GetCmfPendingImpactDetails', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    if (evidenceNode) evidenceNode.textContent = (result && result.Message) ? result.Message : 'Unable to generate AI impact details.';
                    return;
                }
                if (evidenceNode) evidenceNode.innerHTML = renderCmfRecommendationHighlights(result.Evidence || 'No AI impact details returned.');
            })
            .catch(function () {
                if (evidenceNode) evidenceNode.textContent = 'Error while calling AI impact details service.';
            });
        }

        function setCmfDrawerMode(mode) {
            var isDetails = mode === 'details';
            var isInsights = mode === 'insights';
            var titleRow = document.getElementById('cmfRecTitleRow');
            var sightingRow = document.getElementById('cmfRecSightingRow');
            var assistantSightingDetails = document.getElementById('cmfAssistantSightingDetails');
            var assistantQualityAssessment = document.getElementById('cmfAssistantQualityAssessment');
            var assistantRecommendation = document.getElementById('cmfAssistantRecommendationSection');
            var assistantImpact = document.getElementById('cmfAssistantImpactSection');
            var qualityNode = document.getElementById('cmfRecQuality');
            var reasoningSection = document.getElementById('cmfRecReasoningSection');
            var actionsNode = document.getElementById('cmfRecActions');
            var detailsBody = document.getElementById('cmfDecisionDetailsBody');
            if (titleRow) titleRow.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (sightingRow) sightingRow.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (assistantSightingDetails) assistantSightingDetails.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (assistantQualityAssessment) assistantQualityAssessment.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (assistantRecommendation) assistantRecommendation.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (assistantImpact) assistantImpact.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (qualityNode) qualityNode.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (reasoningSection) reasoningSection.classList.toggle('cmf-rec-hidden', isDetails);
            if (actionsNode) actionsNode.classList.toggle('cmf-rec-hidden', isDetails || isInsights);
            if (detailsBody) detailsBody.classList.toggle('cmf-rec-hidden', !isDetails && !isInsights);
            if (!isDetails && !isInsights && detailsBody) detailsBody.innerHTML = '';
            if (!isDetails && !isInsights) {
                var headingNode = document.getElementById('cmfRecHeading');
                if (headingNode) headingNode.textContent = 'AI CMF Assistant';
            }
        }

        function normalizeCmfRecommendationLabel(value) {
            var text = String(value || '').trim();
            if (!text) return 'AI result';
            if (/tag\s+as\s+cmf|cmf_ok|approve/i.test(text)) return 'CMF_OK';
            if (/incomplete|hold|missing|more\s+evidence/i.test(text)) return 'CMF_INCOMPLETE';
            if (/do\s+not\s+tag|reject|cmf_reject/i.test(text)) return 'CMF_REJECT';
            return text;
        }

        function updatePendingRecommendationBadge(cpId, recommendation, score) {
            if (!cpId) return;
            var selector = '.pending-ai-rec-btn[data-cmf-rec-id="' + cssEscapeValue(cpId) + '"]';
            var nodes = document.querySelectorAll(selector);
            var rec = normalizeCmfRecommendationLabel(recommendation || 'CMF_REVIEW');
            for (var i = 0; i < nodes.length; i++) {
                var labelNode = nodes[i].querySelector('.pending-ai-rec-label');
                if (labelNode) labelNode.textContent = rec;
                var confidenceNode = nodes[i].querySelector('.pending-ai-rec-confidence');
                if (confidenceNode) {
                    confidenceNode.textContent = '';
                    confidenceNode.style.display = 'none';
                }
                nodes[i].classList.add('pending-ai-rec-ready');
                nodes[i].setAttribute('aria-label', 'Open CMF Analysis: ' + rec);
                nodes[i].setAttribute('title', 'Open CMF Analysis: ' + rec);
            }
        }

        function renderCmfRecommendationHighlights(reasoningText, ruleScores) {
            var text = String(reasoningText || '').trim();
            if (!text) return '<div class="cmf-rec-reasoning-card">No AI reasoning provided.</div>';

            var normalized = text.replace(/\r/g, '\n').replace(/\n{2,}/g, '\n').trim();
            var impactMatch = normalized.match(/(?:^|\n)\s*(?:ISSUE\s+IMPACT|DECISION\s+IMPACT)\s*:\s*([\s\S]*)$/i);
            var impactText = impactMatch ? impactMatch[1].replace(/^[-*]\s*/gm, '').trim() : '';
            var reasonText = impactMatch ? normalized.substring(0, impactMatch.index).trim() : normalized;
            reasonText = reasonText.replace(/^\s*(?:REASONING|AI\s+REASONING)\s*:\s*/i, '').replace(/^[-*]\s*/gm, '').trim();

            var bullets = reasonText.split(/\n+|(?:^|\s)[-•]\s+/).map(function (item) {
                return item.replace(/^\d+[\.)]\s*/, '').trim();
            }).filter(function (item) { return item.length > 0; }).slice(0, 4);
            if (bullets.length === 0 && reasonText) bullets = [firstCompleteSentence(reasonText, 420)];
            var firstImpact = firstCompleteSentence(impactText, 260);

            var html = '<div class="cmf-rec-reasoning-card">';
            html += '<div><strong>Reason:</strong></div>';
            html += '<ul class="cmf-rec-reason-list">';
            for (var i = 0; i < bullets.length; i++) {
                html += '<li>' + escapeHtml(trimToCompleteSentence(bullets[i], 240)) + '</li>';
            }
            html += '</ul>';
            if (firstImpact) html += '<div><strong>Impact:</strong> ' + escapeHtml(firstImpact) + '</div>';
            html += renderCmfCriterionScores(ruleScores || []);
            return html + '</div>';
        }

        function trimToCompleteSentence(text, maxLength) {
            var value = String(text || '').trim();
            if (!value || value.length <= maxLength) return value;
            var clipped = value.substring(0, maxLength).trim();
            var boundary = Math.max(clipped.lastIndexOf('.'), clipped.lastIndexOf('!'), clipped.lastIndexOf('?'));
            if (boundary >= 80) return clipped.substring(0, boundary + 1).trim();
            return clipped.replace(/\s+\S*$/, '').replace(/[,:;\-\s]+$/, '').trim() + '.';
        }

        function renderCmfDecisionDetailsLoading(cpId) {
            return '<div class="cmf-decision-details-shell"><div class="cmf-decision-card"><h4>Context</h4><div class="ai-loading-banner">&#10024; Generating CMF decision details, please wait...</div></div></div>';
        }

        function renderCmfDecisionDetails(rawText, cpId) {
            var parsed = parseCmfDecisionDetails(rawText || '');
            var score = parsed.score;
            var scoreText = typeof score === 'number' ? score + '%' : '--';
            return '<div class="cmf-decision-details-shell">' +
                '<section class="cmf-decision-card"><h4>Context</h4>' + renderCmfBriefList(parsed.context) + '</section>' +
                '<section class="cmf-decision-card"><h4>HSD Information Quality</h4>' +
                    '<div class="cmf-quality-meter-row"><div class="cmf-quality-score-ring" style="--score:' + escapeHtml(typeof score === 'number' ? score : 0) + '"><span>' + escapeHtml(scoreText) + '</span></div><div class="cmf-quality-caption">Quality score reflects how complete the HSD and row details are for a CMF decision.</div></div>' +
                    '<div class="cmf-quality-columns"><div class="cmf-quality-column missing"><div class="cmf-quality-column-title">Missing/Incomplete Details</div>' + renderCmfBriefList(parsed.missing) + '</div><div class="cmf-quality-column strengths"><div class="cmf-quality-column-title">Key Strengths</div>' + renderCmfBriefList(parsed.strengths) + '</div></div>' +
                '</section>' +
                '<section class="cmf-decision-card"><h4>Evidence Signals</h4>' + renderCmfBriefList(parsed.evidence) + '</section>' +
                '<section class="cmf-decision-card"><h4>Reviewer Attention</h4>' + renderCmfBriefList(parsed.attention) + '</section>' +
            '</div>';
        }

        function parseCmfDecisionDetails(rawText) {
            var text = String(rawText || '').replace(/\r/g, '\n');
            var lines = text.split('\n');
            var parsed = { context: [], missing: [], strengths: [], evidence: [], attention: [], score: null };
            var section = 'context';
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i].trim();
                if (!line) continue;
                var heading = line.replace(/^#+\s*/, '').replace(/:$/, '').trim().toLowerCase();
                if (heading === 'context' || heading === 'cmf decision context') { section = 'context'; continue; }
                if (heading === 'hsd information quality') { section = 'quality'; continue; }
                if (heading === 'evidence signals') { section = 'evidence'; continue; }
                if (heading === 'reviewer attention') { section = 'attention'; continue; }
                var scoreMatch = line.match(/quality\s*score\s*:\s*(\d{1,3})|score\s*:\s*(\d{1,3})/i);
                if (scoreMatch) {
                    parsed.score = Math.max(0, Math.min(100, parseInt(scoreMatch[1] || scoreMatch[2], 10)));
                    continue;
                }
                if (/^missing\/incomplete details\s*:/i.test(line)) {
                    section = 'missing';
                    addCmfDetailItems(parsed.missing, line.replace(/^missing\/incomplete details\s*:/i, ''));
                    continue;
                }
                if (/^key strengths\s*:/i.test(line)) {
                    section = 'strengths';
                    addCmfDetailItems(parsed.strengths, line.replace(/^key strengths\s*:/i, ''));
                    continue;
                }
                line = line.replace(/^[-*]\s*/, '').trim();
                if (section === 'quality') {
                    parsed.context.push(line);
                } else if (section === 'missing') {
                    addCmfDetailItems(parsed.missing, line);
                } else if (section === 'strengths') {
                    addCmfDetailItems(parsed.strengths, line);
                } else if (section === 'evidence') {
                    parsed.evidence.push(line);
                } else if (section === 'attention') {
                    parsed.attention.push(line);
                } else {
                    parsed.context.push(line);
                }
            }
            if (typeof parsed.score !== 'number') parsed.score = estimateCmfQualityScore(parsed);
            if (!parsed.context.length) parsed.context.push('No context returned by the model.');
            if (!parsed.missing.length) parsed.missing.push('None called out by the model.');
            if (!parsed.strengths.length) parsed.strengths.push('Limited strengths available.');
            if (!parsed.evidence.length) parsed.evidence.push('No evidence signals returned by the model.');
            if (!parsed.attention.length) parsed.attention.push('Review the CMF recommendation after checking HSD quality.');
            return parsed;
        }

        function addCmfDetailItems(target, text) {
            var clean = String(text || '').replace(/^[-*]\s*/, '').trim();
            if (!clean) return;
            var parts = clean.split(/;|,(?=\s*[A-Za-z])/);
            for (var i = 0; i < parts.length; i++) {
                var item = parts[i].trim();
                if (item) target.push(item);
            }
        }

        function estimateCmfQualityScore(parsed) {
            var score = 50 + Math.min(25, (parsed.strengths || []).length * 10) - Math.min(25, (parsed.missing || []).length * 8);
            return Math.max(20, Math.min(90, score));
        }

        function renderCmfBriefList(items) {
            var list = items && items.length ? items : ['No detail available.'];
            var html = '<ul>';
            for (var i = 0; i < list.length && i < 4; i++) {
                html += '<li>' + escapeHtml(sanitizeCmfDisplaySentence(list[i], 320)) + '</li>';
            }
            return html + '</ul>';
        }

        function closeCmfRecDrawer() {
            var drawerBg = document.getElementById('cmfRecDrawerBg');
            var drawer = document.getElementById('cmfRecDrawer');
            if (!drawerBg || !drawer) {
                return;
            }

            drawerBg.classList.remove('show');
            drawer.classList.remove('show');
        }

        function showCmfPlaceholderAction(actionName) {
            if (/approve/i.test(actionName)) {
                showPortalToast('Review the AI recommendation and quality score before approving CMF tagging.');
                return;
            }
            if (/commit/i.test(actionName)) {
                showPortalToast('Commit is gated until a generated recommendation is reviewed in this drawer.');
                return;
            }
            showPortalToast(actionName + ' is routed through the active portal workflow.');
        }

        function focusReportsAssistant(promptText) {
            var reportsLink = document.getElementById('lnkNavReports');
            var input = document.getElementById('reportsPromptInput');
            if (input) {
                input.value = promptText || input.value || '';
                input.focus();
                return false;
            }
            if (reportsLink) reportsLink.click();
            return false;
        }

        function focusFirstRowAiSummary() {
            var btn = document.querySelector('.status-cell-wrap .ai-summary-btn');
            if (btn) {
                btn.focus();
                btn.click();
                return false;
            }
            showPortalToast('No Issue List rows are available for AI summary.');
            return false;
        }

        function focusFirstPendingRecommendation() {
            var btn = document.querySelector('.pending-ai-rec-btn');
            if (btn) {
                btn.focus();
                btn.click();
                return false;
            }
            showPortalToast('No CMF Pending rows are available for recommendation.');
            return false;
        }

        function appendReportsChatMessage(role, text, imageUrl, reportUrl) {
            var chatLog = document.getElementById('reportsChatLog');
            if (!chatLog) {
                return;
            }

            var messageRow = document.createElement('div');
            messageRow.style.display = 'flex';
            messageRow.style.justifyContent = role === 'user' ? 'flex-end' : 'flex-start';
            messageRow.style.marginBottom = '10px';

            var bubble = document.createElement('div');
            bubble.style.maxWidth = '80%';
            bubble.style.padding = '10px 12px';
            bubble.style.borderRadius = '10px';
            bubble.style.whiteSpace = 'pre-wrap';
            bubble.style.fontSize = '13px';
            bubble.style.lineHeight = '1.4';

            if (role === 'user') {
                bubble.style.background = '#0068b5';
                bubble.style.color = '#fff';
                bubble.textContent = text || '';
            } else {
                bubble.style.background = '#eef4fb';
                bubble.style.color = '#12344d';
                bubble.style.border = '1px solid #d6e2ef';
                bubble.style.whiteSpace = 'normal';
                var markdownHtml = renderMarkdown(text || '');
                bubble.innerHTML = markdownHtml;
                bubble.className = 'markdown-content';
            }
            messageRow.appendChild(bubble);
            chatLog.appendChild(messageRow);

            if (role !== 'user' && imageUrl) {
                var imageWrap = document.createElement('div');
                imageWrap.style.margin = '6px 0 12px 0';
                var image = document.createElement('img');
                image.src = imageUrl;
                image.alt = 'Generated analytics chart';
                image.style.maxWidth = '100%';
                image.style.border = '1px solid #d6e2ef';
                image.style.borderRadius = '8px';
                image.style.background = '#fff';
                imageWrap.appendChild(image);
                chatLog.appendChild(imageWrap);
            }

            if (role !== 'user' && reportUrl) {
                var linkWrap = document.createElement('div');
                linkWrap.style.margin = '0 0 12px 0';
                var link = document.createElement('a');
                link.href = reportUrl;
                link.target = '_blank';
                link.textContent = 'Download generated report';
                link.style.fontSize = '13px';
                linkWrap.appendChild(link);
                chatLog.appendChild(linkWrap);
            }

            chatLog.scrollTop = chatLog.scrollHeight;
        }

        function sendReportsPrompt() {
            var input = document.getElementById('reportsPromptInput');
            var sendButton = document.getElementById('reportsPromptSend');
            if (!input || !sendButton) {
                return;
            }

            var prompt = (input.value || '').trim();
            if (!prompt) {
                return;
            }

            appendReportsChatMessage('user', prompt);
            input.value = '';
            sendButton.disabled = true;
            sendButton.textContent = 'Running...';

            var payload = {
                prompt: prompt,
                platform: getCurrentPlatformValue()
            };

            fetch('CMF_Web_portal.aspx/GetReportsAssistantResponse', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify(payload)
            })
            .then(function (response) {
                return response.json();
            })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    appendReportsChatMessage('assistant', (result && result.Message) ? result.Message : 'Unable to process this analytics request right now.');
                    return;
                }

                appendReportsChatMessage('assistant', result.Message || 'Completed.', result.ImageUrl || '', result.ReportUrl || '');
            })
            .catch(function () {
                appendReportsChatMessage('assistant', 'Error while calling reports assistant service.');
            })
            .finally(function () {
                sendButton.disabled = false;
                sendButton.textContent = 'Send';
            });
        }

        function sendReportsQuickPrompt(prompt) {
            var input = document.getElementById('reportsPromptInput');
            if (!input) {
                return;
            }

            input.value = prompt;
            sendReportsPrompt();
        }

        function saveReportsFormatTemplate() {
            try {
                var textarea = document.getElementById('reportsFormatTemplate');
                if (textarea && textarea.readOnly) {
                    showPortalToast('Click Edit format before saving changes.');
                    return;
                }
                var payload = { template: textarea ? (textarea.value || '') : '', platform: '', fileName: 'manual-template.md' };
                fetch('CMF_Web_portal.aspx/SaveReportsTemplate', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify(payload)
                })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var res = data && data.d ? data.d : data;
                    if (res && res.Success) {
                        setReportsTemplateEditMode(false);
                        showPortalToast('Report format saved to server.');
                    } else {
                        showPortalToast('Failed to save template: ' + (res && res.Message ? res.Message : 'unknown'));
                    }
                })
                .catch(function () { showPortalToast('Failed to save report template.'); });
            } catch (ignore) {
                showPortalToast('Failed to save report template.');
            }
        }

        function generateReportsFromTemplate() {
            // Request server to generate using the saved global template across all CMF data.
            var payload = { platform: '' };
            var btn = document.querySelector('.reports-format-actions button[onclick*="generateReportsFromTemplate"]');
            if (btn) { btn.disabled = true; btn.textContent = 'Generating...'; }
            fetch('CMF_Web_portal.aspx/GenerateReportFromTemplate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                var res = data && data.d ? data.d : data;
                if (!res || res.Success !== true) {
                    appendReportsChatMessage('assistant', (res && res.Message) ? res.Message : 'Unable to generate report from template.');
                } else {
                    appendReportsChatMessage('assistant', res.Message || 'Report generated.', res.ImageUrl || '', res.ReportUrl || '');
                }
            })
            .catch(function () { appendReportsChatMessage('assistant', 'Error while generating report from template.'); })
            .finally(function () { if (btn) { btn.disabled = false; btn.textContent = 'Generate from format'; } });
        }

        function uploadReportsFormat(event) {
            var file = event && event.target && event.target.files && event.target.files[0];
            if (!file) return;
            var label = document.getElementById('reportsFormatName');
            var reader = new FileReader();
            reader.onload = function () {
                var content = reader.result || '';
                var payload = { template: content, platform: '', fileName: file.name };
                fetch('CMF_Web_portal.aspx/SaveReportsTemplate', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify(payload)
                })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var res = data && data.d ? data.d : data;
                    if (res && res.Success) {
                        var textarea = document.getElementById('reportsFormatTemplate');
                        if (textarea) { textarea.value = content; }
                        setReportsTemplateEditMode(false);
                        if (label) { label.style.display = 'inline'; label.textContent = file.name + ' uploaded'; }
                        showPortalToast('Report template uploaded to server.');
                    } else {
                        showPortalToast('Failed to upload template: ' + (res && res.Message ? res.Message : 'unknown'));
                    }
                })
                .catch(function () { showPortalToast('Failed to upload report template.'); });
            };
            reader.readAsText(file);
        }

        function initReportsFormatTemplate() {
            try {
                var textarea = document.getElementById('reportsFormatTemplate');
                setReportsTemplateEditMode(false);
                // Try load server-saved template first
                fetch('CMF_Web_portal.aspx/GetSavedTemplate', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json; charset=utf-8' },
                    body: JSON.stringify({ platform: getCurrentPlatformValue() })
                })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var res = data && data.d ? data.d : data;
                    if (res && res.Success && textarea) {
                        textarea.value = res.Message || textarea.value;
                        setReportsTemplateEditMode(false);
                        return;
                    }
                    var saved = localStorage.getItem('cmf.reports.formatTemplate');
                    if (textarea && saved) { textarea.value = saved; }
                })
                .catch(function () {
                    var saved = localStorage.getItem('cmf.reports.formatTemplate');
                    if (textarea && saved) { textarea.value = saved; }
                });
            } catch (ignore) {
            }
        }

        function setReportsTemplateEditMode(isEditing) {
            var textarea = document.getElementById('reportsFormatTemplate');
            var editBtn = document.getElementById('reportsFormatEditBtn');
            var saveBtn = document.getElementById('reportsFormatSaveBtn');
            if (textarea) {
                textarea.readOnly = !isEditing;
                textarea.classList.toggle('is-readonly', !isEditing);
            }
            if (editBtn) editBtn.style.display = isEditing ? 'none' : '';
            if (saveBtn) saveBtn.disabled = !isEditing;
        }

        function bindReportsPromptEnterHandler() {
            var input = document.getElementById('reportsPromptInput');
            if (!input || input.dataset.boundEnter === '1') {
                return;
            }

            input.addEventListener('keydown', function (event) {
                if (event.key === 'Enter' && !event.shiftKey) {
                    event.preventDefault();
                    sendReportsPrompt();
                }
            });
            input.dataset.boundEnter = '1';
        }

        function getCollapsibleConfig() {
            return {
                mainMenu: {
                    bodyId: 'mainMenuSectionBody',
                    buttonId: 'mainMenuToggleBtn',
                    storageKey: 'cmf.collapsible.mainMenu',
                    containerId: 'mainMenuSidebar',
                    shellId: 'portalLayoutShell',
                    reopenButtonId: 'mainMenuReopenBtn'
                },
                issueFilters: {
                    bodyId: 'issueTopFilterBody',
                    buttonId: 'issueFiltersToggleBtn',
                    storageKey: 'cmf.collapsible.issueFilters.inline',
                    defaultCollapsed: false
                }
            };
        }

        function setCollapsibleSection(sectionName, collapsed, persistState) {
            var config = getCollapsibleConfig()[sectionName];
            if (!config) {
                return;
            }

            var body = document.getElementById(config.bodyId);
            var button = document.getElementById(config.buttonId);
            var container = config.containerId ? document.getElementById(config.containerId) : null;
            var shell = config.shellId ? document.getElementById(config.shellId) : null;
            var reopenButton = config.reopenButtonId ? document.getElementById(config.reopenButtonId) : null;

            if (body) {
                if (collapsed) {
                    body.classList.add('is-collapsed');
                } else {
                    body.classList.remove('is-collapsed');
                }
            }

            if (button) {
                button.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
                var label = button.querySelector('.toggle-label');
                if (label) {
                    label.textContent = sectionName === 'mainMenu' ? (collapsed ? '>>' : '<<') : (collapsed ? 'Show' : 'Hide');
                }
                if (sectionName === 'mainMenu') {
                    button.title = collapsed ? 'Expand menu labels' : 'Collapse menu labels';
                    button.setAttribute('aria-label', collapsed ? 'Expand menu labels' : 'Collapse menu labels');
                }
            }

            if (container) {
                if (collapsed) {
                    container.classList.add('is-collapsed');
                } else {
                    container.classList.remove('is-collapsed');
                }
            }

            if (shell) {
                if (collapsed) {
                    shell.classList.add('main-menu-collapsed');
                } else {
                    shell.classList.remove('main-menu-collapsed');
                }
            }

            if (reopenButton) {
                reopenButton.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
            }

            if (sectionName === 'mainMenu' && window.CMF_PORTAL && window.CMF_PORTAL.homeTrendChart) {
                setTimeout(function () {
                    try { window.CMF_PORTAL.homeTrendChart.resize(); } catch (ignore) { }
                }, 220);
            }

            if (persistState) {
                try {
                    localStorage.setItem(config.storageKey, collapsed ? '1' : '0');
                } catch (ignore) {
                }
            }
        }

        function toggleCollapsibleSection(sectionName) {
            var config = getCollapsibleConfig()[sectionName];
            if (!config) {
                return;
            }

            var body = document.getElementById(config.bodyId);
            var collapsed = body ? !body.classList.contains('is-collapsed') : false;
            setCollapsibleSection(sectionName, collapsed, true);
        }

        function initCollapsibleSections() {
            var all = getCollapsibleConfig();
            for (var sectionName in all) {
                if (!Object.prototype.hasOwnProperty.call(all, sectionName)) {
                    continue;
                }

                var cfg = all[sectionName];
                var initialCollapsed = !!cfg.defaultCollapsed;
                try {
                    var storedState = localStorage.getItem(cfg.storageKey);
                    if (storedState !== null) {
                        initialCollapsed = storedState === '1';
                    }
                } catch (ignore) {
                }
                setCollapsibleSection(sectionName, initialCollapsed, false);
            }
        }

        function initMainMenuTooltips() {
            var links = document.querySelectorAll('#mainMenuSidebar .portal-nav-link');
            for (var i = 0; i < links.length; i++) {
                var textNode = links[i].querySelector('span');
                var label = textNode ? (textNode.textContent || '').trim() : '';
                if (label) {
                    links[i].setAttribute('title', label);
                    links[i].setAttribute('aria-label', label);
                }
            }
        }

        function syncIssueHorizontalScroll(attempt) {
            var host = document.getElementById('issue-grid-inner');
            var proxy = document.getElementById('issue-scroll-proxy');
            var proxyInner = document.getElementById('issue-scroll-proxy-inner');
            var table = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);

            if (!host || !table) {
                return;
            }

            host.style.overflowX = 'auto';
            host.style.overflowY = 'auto';
            host.style.maxHeight = 'min(62vh, 620px)';
            host.style.width = '100%';
            host.style.maxWidth = '100%';

            // Enforce width at runtime in case later CSS rules collapse the table width.
            table.style.width = 'max-content';
            table.style.minWidth = '2200px';
            table.style.maxWidth = 'none';

            var targetWidth = Math.max(table.scrollWidth || 0, table.offsetWidth || 0, 2200);
            if (proxyInner) {
                proxyInner.style.width = targetWidth + 'px';
            }

            // If layout is not ready yet (e.g., after async update while hidden), retry a few times.
            if (host.clientWidth > 0 && targetWidth <= host.clientWidth && (attempt || 0) < 6) {
                setTimeout(function () { syncIssueHorizontalScroll((attempt || 0) + 1); }, 80);
            }

            if (!proxy) {
                return;
            }

            var syncLock = false;
            host.onscroll = function () {
                if (syncLock) return;
                syncLock = true;
                proxy.scrollLeft = host.scrollLeft;
                syncLock = false;
            };

            proxy.onscroll = function () {
                if (syncLock) return;
                syncLock = true;
                host.scrollLeft = proxy.scrollLeft;
                syncLock = false;
            };

            proxy.scrollLeft = host.scrollLeft;
            proxy.style.display = 'none';
        }

        function isOverallDashboardSelection() {
            var overallPlatform = (window.CMF_PORTAL.overallPlatformTable || '').toLowerCase();
            var currentPlatform = (window.CMF_PORTAL.currentDashboardPlatform || window.CMF_PORTAL.currentPlatform || '').toLowerCase();
            return !!overallPlatform && currentPlatform === overallPlatform;
        }

        function getOverallTrendRangeWeeks() {
            var selector = document.getElementById('homeOverallTrendRange');
            if (!selector) {
                return 6;
            }

            var parsed = parseInt(selector.value || '6', 10);
            return isNaN(parsed) || parsed <= 0 ? 6 : parsed;
        }

        function getSpecificTrendRangeWeeks() {
            var selector = document.getElementById('homeSpecificTrendRange');
            if (!selector) {
                return 6;
            }

            var parsed = parseInt(selector.value || '6', 10);
            return isNaN(parsed) || parsed <= 0 ? 6 : parsed;
        }

        function bindOverallTrendRangeSelector() {
            var selector = document.getElementById('homeOverallTrendRange');
            if (!selector || selector.dataset.boundChange === '1') {
                return;
            }

            selector.addEventListener('change', function () {
                if (!isOverallDashboardSelection()) {
                    return;
                }

                var snapshot = window.CMF_PORTAL.homeDashboardSnapshot;
                if (!snapshot) {
                    return;
                }

                renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'homeOverallTrendChartRef', getOverallTrendRangeWeeks());
            });

            selector.dataset.boundChange = '1';
        }

        function bindSpecificTrendRangeSelector() {
            var selector = document.getElementById('homeSpecificTrendRange');
            if (!selector || selector.dataset.boundChange === '1') {
                return;
            }

            selector.addEventListener('change', function () {
                if (isOverallDashboardSelection()) {
                    return;
                }

                var snapshot = window.CMF_PORTAL.homeDashboardSnapshot;
                if (!snapshot) {
                    return;
                }

                renderHomeTrendChart(snapshot, 'homeTrendChart', 'homeTrendChartRef', getSpecificTrendRangeWeeks());
            });

            selector.dataset.boundChange = '1';
        }

        function renderHomeTrendChart(snapshot, hostId, chartRefKey, rangeWeeks) {
            var trendHost = document.getElementById(hostId);
            if (!trendHost || typeof echarts === 'undefined') {
                return;
            }

            var fullTrend = snapshot.Trend || [];
            var selectedRangeWeeks = parseInt(rangeWeeks || 0, 10);
            if (isNaN(selectedRangeWeeks) || selectedRangeWeeks <= 0) {
                selectedRangeWeeks = fullTrend.length;
            }

            var trend = fullTrend;
            if (fullTrend.length > selectedRangeWeeks) {
                trend = fullTrend.slice(fullTrend.length - selectedRangeWeeks);
            }
            var trendStartIndex = Math.max(0, fullTrend.length - trend.length);
            var visibleTrendPoints = [];
            for (var visibleIndex = 0; visibleIndex < trend.length; visibleIndex++) {
                var visibleItem = trend[visibleIndex] || {};
                visibleTrendPoints.push({
                    weekLabel: String(visibleItem.WeekLabel || ''),
                    weekOffset: Math.max(0, (fullTrend.length - 1) - (trendStartIndex + visibleIndex)),
                    openValue: Number(visibleItem.NewIssues || 0),
                    closedValue: Number(visibleItem.ResolvedIssues || 0)
                });
            }

            function toTrendSeriesData(items, valueSelector) {
                var points = [];
                for (var pointIndex = 0; pointIndex < items.length; pointIndex++) {
                    var item = items[pointIndex] || {};
                    var fullIndex = trendStartIndex + pointIndex;
                    points.push({
                        value: Number(valueSelector(item) || 0),
                        weekLabel: item.WeekLabel || '',
                        weekOffset: Math.max(0, (fullTrend.length - 1) - fullIndex)
                    });
                }
                return points;
            }

            var cacheKey = 'lastHomeDashboardKey_' + hostId;
            var snapshotKey = [
                window.CMF_PORTAL.currentPlatform || '',
                hostId,
                selectedRangeWeeks,
                fullTrend.length,
                trend.length,
                getTextById(window.CMF_PORTAL.ids.lblIssueTotal),
                getTextById(window.CMF_PORTAL.ids.lblPendingSightings)
            ].join('|');

            if (window.CMF_PORTAL[cacheKey] === snapshotKey && window.CMF_PORTAL[chartRefKey]) {
                return;
            }

            if (!window.CMF_PORTAL[chartRefKey]) {
                window.CMF_PORTAL[chartRefKey] = echarts.init(trendHost);
            }

            window.CMF_PORTAL[chartRefKey].setOption({
                animation: false,
                color: ['#ef4444', '#16a34a'],
                tooltip: { trigger: 'axis' },
                legend: {
                    top: 0,
                    left: 0,
                    icon: 'circle',
                    itemWidth: 8,
                    itemHeight: 8,
                    textStyle: { color: '#475467', fontSize: 11 }
                },
                grid: { left: 36, right: 14, top: 30, bottom: 26 },
                xAxis: {
                    type: 'category',
                    data: trend.map(function (item) { return item.WeekLabel; }),
                    boundaryGap: false,
                    axisLine: { lineStyle: { color: '#e4e7ec' } },
                    axisLabel: { color: '#667085', fontSize: 11 }
                },
                yAxis: {
                    type: 'value',
                    axisLine: { show: false },
                    splitLine: { lineStyle: { color: '#eef2f7' } },
                    axisLabel: { color: '#667085', fontSize: 11 }
                },
                series: [
                    {
                        name: 'Open',
                        type: 'line',
                        smooth: true,
                        data: toTrendSeriesData(trend, function (item) { return item.NewIssues; }),
                        lineStyle: { width: 3 },
                        symbol: 'circle',
                        symbolSize: 6
                    },
                    {
                        name: 'Closed',
                        type: 'line',
                        smooth: true,
                        data: toTrendSeriesData(trend, function (item) { return item.ResolvedIssues; }),
                        lineStyle: { width: 3 },
                        symbol: 'circle',
                        symbolSize: 6,
                        areaStyle: { color: 'rgba(22,163,74,0.12)' }
                    }
                ]
            }, true);

            var trendChart = window.CMF_PORTAL[chartRefKey];
            if (trendChart) {
                if (trendChart.__cmfTrendClickHandler) {
                    trendChart.off('click', trendChart.__cmfTrendClickHandler);
                }

                if (trendChart.__cmfTrendAxisPointerHandler) {
                    trendChart.off('updateAxisPointer', trendChart.__cmfTrendAxisPointerHandler);
                }

                if (trendChart.__cmfTrendZrClickHandler && trendChart.getZr) {
                    trendChart.getZr().off('click', trendChart.__cmfTrendZrClickHandler);
                }
                trendChart.__cmfTrendClickHandler = null;

                var trendAxisPointerHandler = function (params) {
                    if (!params || !params.axesInfo || !params.axesInfo.length) {
                        return;
                    }

                    var axisInfo = params.axesInfo[0] || {};
                    var rawIndex = axisInfo.value;
                    var axisIndex = parseInt(rawIndex, 10);
                    if (isNaN(axisIndex) || axisIndex < 0 || axisIndex >= visibleTrendPoints.length) {
                        return;
                    }

                    trendChart.__cmfTrendLastAxisIndex = axisIndex;
                };

                trendChart.__cmfTrendAxisPointerHandler = trendAxisPointerHandler;
                trendChart.on('updateAxisPointer', trendAxisPointerHandler);

                trendChart.__cmfTrendVisiblePoints = visibleTrendPoints;

                var trendZrClickHandler = function (event) {
                    if (!event || !trendChart || !trendChart.containPixel || !trendChart.convertFromPixel) {
                        return;
                    }

                    var offsetX = event.offsetX;
                    var offsetY = event.offsetY;
                    if (!trendChart.containPixel({ gridIndex: 0 }, [offsetX, offsetY])) {
                        return;
                    }

                    var axisIndex = parseInt(trendChart.__cmfTrendLastAxisIndex, 10);
                    if (isNaN(axisIndex) || axisIndex < 0 || axisIndex >= visibleTrendPoints.length) {
                        var xValue = trendChart.convertFromPixel({ xAxisIndex: 0 }, [offsetX, offsetY])[0];
                        axisIndex = Math.round(Number(xValue));
                    }

                    if (isNaN(axisIndex) || axisIndex < 0 || axisIndex >= visibleTrendPoints.length) {
                        return;
                    }

                    var point = visibleTrendPoints[axisIndex] || null;
                    if (!point) {
                        return;
                    }

                    var totalIssues = Number(point.openValue || 0) + Number(point.closedValue || 0);
                    if (isNaN(totalIssues) || totalIssues <= 0) {
                        return;
                    }

                    openTrendWeekIssueLinksPopup('All', point.weekLabel, point.weekOffset);
                };

                trendChart.__cmfTrendZrClickHandler = trendZrClickHandler;
                if (trendChart.getZr) {
                    trendChart.getZr().on('click', trendZrClickHandler);
                }
            }

            window.CMF_PORTAL[cacheKey] = snapshotKey;
            window.CMF_PORTAL[chartRefKey].resize();
        }

        function renderHomeDashboard() {
            var homePanel = document.getElementById(window.CMF_PORTAL.ids.homeWelcomePanel);
            var cmfPanel = document.getElementById(window.CMF_PORTAL.ids.homeCmfSummaryPanel);
            var homeVisible = homePanel && isElementVisible(homePanel);
            var cmfVisible = cmfPanel && isElementVisible(cmfPanel);
            if (!homeVisible && !cmfVisible) {
                return;
            }

            var snapshot = window.CMF_PORTAL.homeDashboardSnapshot;
            if (!snapshot) {
                return;
            }

            var isOverall = isOverallDashboardSelection();
            var overallLayout = document.getElementById('homeOverallLayoutV2');
            var legacyTopLayout = document.getElementById('homeLegacyTopLayout');
            var legacyOverview = document.getElementById('homeLegacyOverview');
            var legacySummary = document.getElementById('homeLegacySummarySection');
            var legacyAiHealth = document.getElementById('homeLegacyAiHealthCard');
            var legacyTables = document.getElementById('homeLegacyTablesGrid');

            if (overallLayout) overallLayout.style.display = isOverall ? '' : 'none';
            if (legacyTopLayout) legacyTopLayout.style.display = isOverall ? 'none' : '';
            if (legacyOverview) legacyOverview.style.display = isOverall ? 'none' : '';
            if (legacySummary) legacySummary.style.display = isOverall ? 'none' : '';
            if (legacyAiHealth) legacyAiHealth.style.display = isOverall ? 'none' : '';
            if (legacyTables) legacyTables.style.display = isOverall ? 'none' : '';

            if (isOverall) {
                renderOverallCombinedSummaryFacts(snapshot);
                renderOverallOemTable(snapshot.OemSummary || []);
                renderOverallHeatmap(snapshot);
                renderOverallPendingSummary(snapshot);
                renderHomeTrendChart(snapshot, 'homeOverallTrendChart', 'homeOverallTrendChartRef', getOverallTrendRangeWeeks());
                return;
            }

            renderHomePortalHealth(snapshot);
            renderHomePredictedBlockers(snapshot);
            renderHomeExecutiveSummary(snapshot);
            renderSpecificSummarySection(snapshot);
            renderDashboardLiveTable('homeMilestoneSummaryTable', snapshot.MilestoneSummary);
            renderDashboardLiveTable('homeComponentSummaryTable', snapshot.ComponentSummary);
            renderDashboardLiveTable('homePendingSummaryTable', snapshot.PendingSummary);
            renderDashboardLiveTable('homeDesignSummaryTable', snapshot.DesignSummary);
            renderHomeTrendChart(snapshot, 'homeTrendChart', 'homeTrendChartRef', getSpecificTrendRangeWeeks());
        }

        function renderSpecificSummarySection(snapshot) {
            renderCombinedSummaryFacts(snapshot, 'homeSpecificSummaryFactsDashboard');
            renderCombinedSummaryFacts(snapshot, 'homeSpecificSummaryFactsCmf');
        }

        function renderHomeExecutiveSummary(snapshot) {
            var host = document.getElementById('homeExecutiveSummary');
            if (!host) return;
            var platform = snapshot.PlatformLabel || 'selected platform';
            var summaryKey = [
                window.CMF_PORTAL.currentPlatform || '',
                snapshot.ActiveIssues || 0,
                snapshot.NeedsAttention || 0,
                snapshot.ClosedIssues || 0,
                snapshot.StaleIssues || 0,
                snapshot.ProgramReadinessScore || 0,
                snapshot.TopRisk || '',
                (snapshot.PredictedBlockers || []).join('|')
            ].join('|');

            if (window.CMF_PORTAL.lastExecutiveSummaryKey === summaryKey) {
                return;
            }

            window.CMF_PORTAL.lastExecutiveSummaryKey = summaryKey;
            host.textContent = 'Generating AI executive summary...';

            fetch('CMF_Web_portal.aspx/GetHomeExecutiveSummary', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({
                    platformLabel: platform,
                    snapshotContext: buildHomeExecutiveSummaryContext(snapshot)
                })
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true || !result.Summary) {
                    host.textContent = (result && result.Message) ? result.Message : 'Unable to generate AI executive summary at this time.';
                    return;
                }
                if (typeof renderMarkdown === 'function') {
                    host.innerHTML = renderMarkdown(result.Summary);
                } else {
                    host.textContent = result.Summary;
                }
            })
            .catch(function () {
                host.textContent = 'Unable to generate AI executive summary at this time.';
            });
        }

        function buildHomeExecutiveSummaryContext(snapshot) {
            var lines = [];
            lines.push('Platform: ' + (snapshot.PlatformLabel || 'selected platform'));
            lines.push('Current workload only. Issue buckets: total=' + String(snapshot.ActiveIssues || 0) + ', open=' + String(snapshot.NeedsAttention || 0) + ', implemented=' + String(snapshot.StaleIssues || 0) + ', closed=' + String(snapshot.ClosedIssues || 0));
            lines.push('Operational metrics: resolved_last_7_days=' + String(snapshot.ResolvedThisWeek || 0) + ', average_resolution_days=' + String(snapshot.AverageResolutionDays || 0) + ', customer_impacted_rows=' + String(snapshot.CustomersAffected || 0));
            lines.push('AI portal health: readiness_score=' + String(snapshot.ProgramReadinessScore || 0) + ', risk_level=' + (snapshot.ProgramRiskLevel || 'unknown') + ', top_risk=' + (snapshot.TopRisk || 'none') + ', risk_concentration=' + (snapshot.RiskConcentration || '0%'));
            lines.push('Predicted blockers: ' + ((snapshot.PredictedBlockers || []).join('; ') || 'none'));
            lines.push('CMF summary facts: ' + summarizeFactsForAi(snapshot.SummaryFacts));
            lines.push('CMF TPT facts: ' + summarizeFactsForAi(snapshot.TptFacts));
            lines.push('Milestone CMF summary: ' + summarizeTableForAi(snapshot.MilestoneSummary, 5));
            lines.push('Component Open CMF summary: ' + summarizeTableForAi(snapshot.ComponentSummary, 6));
            lines.push('Pending CMF summary: ' + summarizeTableForAi(snapshot.PendingSummary, 4));
            lines.push('Design summary: ' + summarizeTableForAi(snapshot.DesignSummary, 4));
            return lines.join('\n');
        }

        function summarizeFactsForAi(facts) {
            facts = facts || [];
            var parts = [];
            for (var i = 0; i < facts.length; i++) {
                parts.push((facts[i].Label || '-') + '=' + (facts[i].Value || '0') + (facts[i].Note ? ' (' + facts[i].Note + ')' : ''));
            }
            return parts.join(', ') || 'none';
        }

        function summarizeTableForAi(tableData, maxRows) {
            if (!tableData || !tableData.Columns || !tableData.Rows) return 'none';
            var rows = [];
            for (var r = 0; r < tableData.Rows.length && r < maxRows; r++) {
                var cells = [];
                for (var c = 0; c < tableData.Columns.length; c++) {
                    var rawValue = tableData.Rows[r][c] == null ? '' : String(tableData.Rows[r][c]);
                    if (rawValue.indexOf(':::OPEN_HSDS:::') >= 0) {
                        rawValue = rawValue.split(':::OPEN_HSDS:::')[0];
                    }
                    cells.push(tableData.Columns[c] + '=' + rawValue);
                }
                rows.push(cells.join(', '));
            }
            return rows.join(' | ') || 'none';
        }

        function renderHomePortalHealth(snapshot) {
            var scoreNode = document.getElementById('homePortalHealthScore');
            var riskNode = document.getElementById('homePortalHealthRisk');
            var weeklyNode = document.getElementById('homeWeeklyChanges');
            var concentrationNode = document.getElementById('homePortalRiskConcentration');
            var blockersNode = document.getElementById('homePortalBlockers');
            if (scoreNode) scoreNode.textContent = String(snapshot.ProgramReadinessScore || 0);
            if (riskNode) riskNode.textContent = snapshot.ProgramRiskLevel || 'No risk data';
            if (weeklyNode) {
                weeklyNode.innerHTML = '';
                var weeklyChanges = snapshot.WeeklyChanges || [];
                if (weeklyChanges.length === 0) weeklyChanges = ['No weekly changes are available yet.'];
                for (var weeklyIndex = 0; weeklyIndex < weeklyChanges.length && weeklyIndex < 4; weeklyIndex++) {
                    var changeItem = document.createElement('li');
                    changeItem.textContent = weeklyChanges[weeklyIndex];
                    weeklyNode.appendChild(changeItem);
                }
            }
            if (concentrationNode) concentrationNode.textContent = snapshot.RiskConcentration || '0%';
            if (blockersNode) {
                blockersNode.innerHTML = '';
                var blockers = snapshot.PredictedBlockers || [];
                if (blockers.length === 0) blockers = ['Predicting blockers from current workload...'];
                for (var i = 0; i < blockers.length && i < 4; i++) {
                    var item = document.createElement('li');
                    item.textContent = blockers[i];
                    blockersNode.appendChild(item);
                }
            }
        }

        function renderHomePredictedBlockers(snapshot) {
            var blockersNode = document.getElementById('homePortalBlockers');
            if (!blockersNode) return;

            var blockerKey = [
                window.CMF_PORTAL.currentPlatform || '',
                snapshot.ActiveIssues || 0,
                snapshot.NeedsAttention || 0,
                snapshot.StaleIssues || 0,
                snapshot.ClosedIssues || 0,
                snapshot.TopRisk || '',
                snapshot.RiskConcentration || '',
                (snapshot.PredictedBlockers || []).join('|')
            ].join('|');

            if (window.CMF_PORTAL.lastPredictedBlockersKey === blockerKey) {
                return;
            }

            window.CMF_PORTAL.lastPredictedBlockersKey = blockerKey;

            fetch('CMF_Web_portal.aspx/GetHomePredictedBlockers', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({
                    platformLabel: snapshot.PlatformLabel || 'selected platform',
                    snapshotContext: buildHomeExecutiveSummaryContext(snapshot)
                })
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true || !result.Summary) {
                    return;
                }

                var lines = result.Summary
                    .replace(/<[^>]+>/g, '')
                    .split(/\r?\n|\s*[-•]\s+/)
                    .map(function (line) { return line.replace(/^\d+[\.)]\s*/, '').trim(); })
                    .filter(function (line) { return line.length > 0; })
                    .slice(0, 3);

                if (lines.length === 0) {
                    lines = [result.Summary.trim()];
                }

                blockersNode.innerHTML = '';
                for (var i = 0; i < lines.length; i++) {
                    var item = document.createElement('li');
                    item.textContent = lines[i];
                    blockersNode.appendChild(item);
                }
            })
            .catch(function () { });
        }

        function formatSummaryFactValueAndNote(value, note) {
            var valueText = String(value == null ? '0' : value);
            var noteText = String(note == null ? '' : note);

            var daysPrefixMatch = noteText.match(/^\s*days?\s*\|\s*(.*)$/i);
            if (!daysPrefixMatch) {
                return {
                    valueText: valueText,
                    noteText: noteText
                };
            }

            if (!/\bdays?\b/i.test(valueText)) {
                valueText = valueText + ' days';
            }

            return {
                valueText: valueText,
                noteText: daysPrefixMatch[1] || ''
            };
        }

        function renderValueWithDaysSuffix(valueText, suffixClassName) {
            var rawText = String(valueText == null ? '0' : valueText).trim();
            var dayMatch = rawText.match(/^(.+?)\s+(day|days)$/i);
            if (!dayMatch) {
                return escapeHtml(rawText);
            }

            var numericCandidate = String(dayMatch[1] || '').replace(/,/g, '').trim();
            var parsed = parseFloat(numericCandidate);
            var displayValue = isNaN(parsed) ? dayMatch[1] : String(parseInt(parsed, 10));
            var mainValue = escapeHtml(displayValue);
            var dayUnit = escapeHtml(dayMatch[2].toLowerCase());
            var cls = suffixClassName || 'ccip-days-suffix';
            return mainValue + ' <span class="' + cls + '">' + dayUnit + '</span>';
        }

        function renderHomeSummaryFacts(snapshot) {
            var host = document.getElementById('homeDashboardSummaryFacts');
            if (!host) return;
            var facts = [].concat(snapshot.SummaryFacts || [], snapshot.TptFacts || []);
            host.innerHTML = '';
            for (var i = 0; i < facts.length; i++) {
                var fact = facts[i] || {};
                if (!shouldRenderOverallSummaryFact(fact)) {
                    continue;
                }
                var formattedFact = formatSummaryFactValueAndNote(fact.Value, fact.Note);
                var card = document.createElement('div');
                card.className = 'ccip-summary-fact';
                card.innerHTML = '<div class="ccip-summary-fact-label">' + escapeHtml(fact.Label || '-') + '</div>' +
                    '<div class="ccip-summary-fact-value">' + renderValueWithDaysSuffix(formattedFact.valueText, 'ccip-days-suffix') + '</div>' +
                    '<div class="ccip-summary-fact-note">' + escapeHtml(formattedFact.noteText) + '</div>';
                host.appendChild(card);
            }
        }

        function getOverallSummaryToneClass(label) {
            var normalized = String(label || '').toLowerCase();
            if (normalized.indexOf('pending') >= 0) {
                return 'tone-pending';
            }
            if (normalized.indexOf('open') >= 0) {
                return 'tone-open';
            }
            if (normalized.indexOf('closed') >= 0) {
                return 'tone-closed';
            }
            if (normalized.indexOf('implemented') >= 0 || normalized.indexOf('verified') >= 0) {
                return 'tone-implemented';
            }
            if (normalized.indexOf('total') >= 0) {
                return 'tone-total';
            }

            return 'tone-total';
        }

        function shouldRenderOverallSummaryFact(fact) {
            var normalized = String((fact && fact.Label) || '').toLowerCase();
            return normalized.indexOf('avg resolution') < 0;
        }

        function buildOverallSummaryFactTooltip(fact) {
            var label = String((fact && fact.Label) || '').toLowerCase();
            var constituentTooltip = fact && fact.ConstituentTooltip ? String(fact.ConstituentTooltip).trim() : '';
            var lines = [];

            if (constituentTooltip && (label === 'implemented' || label === 'open' || label === 'pending')) {
                return constituentTooltip;
            }

            if (label === 'total cmfs') {
                lines.push('Total customer_must_fix CMFs in the current summary scope.');
                lines.push('Calculated as count of cmf_ok + cmf_duplicate rows.');
            } else if (label === 'closed') {
                lines.push('CMFs that are finalized.');
                lines.push('Calculated as count where status is complete or rejected.');
            } else if (label === 'implemented') {
                lines.push('CMFs that are implemented in the current summary scope.');
                lines.push('Calculated as count where status is implemented.');
            } else if (label === 'open') {
                lines.push('CMFs that are not complete, rejected, or implemented.');
                lines.push('Calculated directly from cmf_ok + cmf_duplicate rows in the summary scope.');
            } else if (label === 'pending') {
                lines.push('CMFs still unresolved in the pending pipeline.');
                lines.push('Calculated as count where status is not complete or rejected.');
            } else if (label.indexOf('avg resolution') >= 0) {
                lines.push('Mean time to resolve CMFs.');
                lines.push('Calculated as average days between date_cmf_decided and implemented_date over resolved CMFs with valid dates.');
            } else {
                lines.push('Summary KPI for the selected scope.');
            }

            return lines.join('\n');
        }

        function normalizeSummaryIssueMetricKey(label) {
            var normalized = String(label || '').trim().toLowerCase();
            if (normalized === 'total cmfs' || normalized === 'total') {
                return 'total';
            }
            if (normalized === 'closed') {
                return 'closed';
            }
            if (normalized === 'implemented') {
                return 'implemented';
            }
            if (normalized === 'open') {
                return 'open';
            }
            if (normalized === 'pending') {
                return 'pending';
            }
            return '';
        }

        function getSummaryIssueGroupByLabel(groupBy) {
            var key = String(groupBy || '').toLowerCase();
            if (key === 'issue') return 'Issue';
            if (key === 'platform') return 'Platform';
            if (key === 'milestone') return 'Milestone';
            if (key === 'design') return 'Design';
            if (key === 'component') return 'Component';
            if (key === 'owner') return 'Owner';
            if (key === 'idst') return 'iDST Name';
            return 'Ingredient';
        }

        function normalizeSummaryIssueSortBy(sortBy) {
            var key = String(sortBy || '').toLowerCase();
            return key === 'hsdcount' ? 'hsdcount' : 'alphabetical';
        }

        function getSummaryIssueSortByLabel(sortBy) {
            var key = normalizeSummaryIssueSortBy(sortBy);
            if (key === 'hsdcount') {
                return 'HSD Count';
            }
            return 'Alphabetical';
        }

        function syncSummaryIssuePopupControls(sourceType) {
            var normalizedSource = String(sourceType || 'summary').toLowerCase();
            var groupBySelect = document.getElementById('summaryIssuesGroupBy');
            var groupByLabel = document.querySelector('label[for="summaryIssuesGroupBy"]');
            var sortBySelect = document.getElementById('summaryIssuesSortBy');
            var sortByLabel = document.querySelector('label[for="summaryIssuesSortBy"]');
            var trendStatusLabel = document.getElementById('summaryIssuesTrendStatusLabel');
            var trendStatusChecks = document.getElementById('summaryIssuesTrendStatusChecks');
            if (!groupBySelect || !groupByLabel || !sortBySelect || !sortByLabel || !trendStatusLabel || !trendStatusChecks) {
                return;
            }

            var showGrouping = normalizedSource === 'summary' || normalizedSource === 'overall-heatmap' || normalizedSource === 'trend-week';
            var showTrendStatus = normalizedSource === 'trend-week';
            groupBySelect.style.display = showGrouping ? '' : 'none';
            groupByLabel.style.display = showGrouping ? '' : 'none';
            sortBySelect.style.display = showGrouping ? '' : 'none';
            sortByLabel.style.display = showGrouping ? '' : 'none';
            trendStatusLabel.style.display = showTrendStatus ? '' : 'none';
            trendStatusChecks.style.display = showTrendStatus ? 'flex' : 'none';
        }

        function syncSummaryTrendIssueStatusControls(showOpen, showClosed) {
            var openCheck = document.getElementById('summaryIssuesShowOpen');
            var closedCheck = document.getElementById('summaryIssuesShowClosed');

            var normalizedOpen = showOpen !== false;
            var normalizedClosed = showClosed !== false;

            if (openCheck) {
                openCheck.checked = normalizedOpen;
            }

            if (closedCheck) {
                closedCheck.checked = normalizedClosed;
            }

            return {
                showOpen: normalizedOpen,
                showClosed: normalizedClosed
            };
        }

        function syncSummaryIssueSortByOptions(preferredSortBy) {
            var select = document.getElementById('summaryIssuesSortBy');
            var selected = normalizeSummaryIssueSortBy(preferredSortBy);
            if (!select) {
                return selected;
            }

            var options = [
                { value: 'alphabetical', text: getSummaryIssueSortByLabel('alphabetical') },
                { value: 'hsdcount', text: getSummaryIssueSortByLabel('hsdcount') }
            ];

            select.innerHTML = '';
            for (var index = 0; index < options.length; index++) {
                var option = document.createElement('option');
                option.value = options[index].value;
                option.textContent = options[index].text;
                select.appendChild(option);
            }

            select.value = selected;
            return selected;
        }

        function syncSummaryIssueGroupByOptions(availableGroupBy, preferredGroupBy) {
            var select = document.getElementById('summaryIssuesGroupBy');
            if (!select) {
                return preferredGroupBy || 'ingredient';
            }

            var options = Array.isArray(availableGroupBy) ? availableGroupBy.slice() : [];
            if (!options.length) {
                options = ['ingredient'];
            }

            var normalized = [];
            var seen = {};
            for (var i = 0; i < options.length; i++) {
                var candidate = String(options[i] || '').toLowerCase().trim();
                if (!candidate || seen[candidate]) {
                    continue;
                }
                seen[candidate] = true;
                normalized.push(candidate);
            }

            if (!normalized.length) {
                normalized.push('ingredient');
            }

            var requested = String(preferredGroupBy || '').toLowerCase().trim();
            var selected = normalized.indexOf(requested) >= 0 ? requested : normalized[0];

            select.innerHTML = '';
            for (var index = 0; index < normalized.length; index++) {
                var value = normalized[index];
                var option = document.createElement('option');
                option.value = value;
                option.textContent = getSummaryIssueGroupByLabel(value);
                select.appendChild(option);
            }

            select.value = selected;
            return selected;
        }

        function buildSummaryIssueGroupByOptions(isOverallScope) {
            var options = ['ingredient', 'milestone', 'design', 'component', 'owner', 'idst'];
            if (isOverallScope) {
                options.splice(1, 0, 'platform');
            }
            return options;
        }

        function buildTrendIssueGroupByOptions(isOverallScope) {
            var options = ['issue', 'ingredient', 'milestone', 'design', 'component', 'owner', 'idst'];
            if (isOverallScope) {
                options.splice(2, 0, 'platform');
            }
            return options;
        }

        function closeSummaryIssueLinksPopup() {
            var popup = document.getElementById('summaryIssuesPopup');
            var backdrop = document.getElementById('summaryIssuesPopupBackdrop');
            if (popup) popup.classList.remove('visible');
            if (backdrop) backdrop.classList.remove('visible');
        }

        function openPendingSummaryDetailsFromSummaryPopup(groupName, idsCsv) {
            closeSummaryIssueLinksPopup();
            return showPendingSummaryIssues(groupName, idsCsv);
        }

        function openSummaryMetricDetailsFromSummaryPopup(metricLabel, groupName, idsCsv) {
            closeSummaryIssueLinksPopup();

            var payload =
                encodeURIComponent(String(metricLabel || 'Summary Metric')) + '||' +
                encodeURIComponent(String(groupName || 'Unassigned')) + '||' +
                encodeURIComponent(String(idsCsv || ''));

            __doPostBack('ShowSummaryMetricIssues', payload);
            return false;
        }

        function renderSummaryIssueLinks(result) {
            var body = document.getElementById('summaryIssuesPopupBody');
            var meta = document.getElementById('summaryIssuesPopupMeta');
            if (!body || !meta) {
                return;
            }

            var popupState = window.CMF_PORTAL.summaryIssuesPopupState || {};
            window.CMF_PORTAL.summaryIssuesPopupLastResult = result || null;
            syncSummaryIssuePopupControls(popupState.sourceType || 'summary');
            var syncedGroupBy = syncSummaryIssueGroupByOptions(
                result && result.AvailableGroupBy ? result.AvailableGroupBy : null,
                result && result.GroupBy ? result.GroupBy : popupState.groupBy
            );
            popupState.groupBy = syncedGroupBy;
            popupState.sortBy = syncSummaryIssueSortByOptions(popupState.sortBy || 'alphabetical');
            var trendStatus = syncSummaryTrendIssueStatusControls(popupState.showOpen, popupState.showClosed);
            popupState.showOpen = trendStatus.showOpen;
            popupState.showClosed = trendStatus.showClosed;
            window.CMF_PORTAL.summaryIssuesPopupState = popupState;

            var totalIssues = result && result.TotalIssues ? parseInt(result.TotalIssues, 10) : 0;
            if (isNaN(totalIssues)) {
                totalIssues = 0;
            }

            var metaParts = [String(totalIssues) + ' issues'];
            var idstMetaLine = '';
            if (popupState.sourceType === 'trend-week') {
                var weekLabel = String(popupState.weekLabel || '').trim();
                if (weekLabel) {
                    metaParts.push(weekLabel);
                }
            }

            if (popupState.sourceType === 'overall-heatmap') {
                var platformLabel = String(popupState.platformLabel || '').trim();
                var ingredientLabel = String(popupState.ingredient || '').trim();
                var scopeTokens = [];
                if (platformLabel) {
                    scopeTokens.push(platformLabel);
                }
                if (ingredientLabel) {
                    scopeTokens.push(ingredientLabel);
                }
                if (scopeTokens.length) {
                    metaParts.push(scopeTokens.join(' / '));
                }

                var idstNames = result && Array.isArray(result.IdstNames) ? result.IdstNames.slice() : [];
                if (idstNames.length) {
                    var maxVisibleIdst = 3;
                    var preview = idstNames.slice(0, maxVisibleIdst).join(', ');
                    if (idstNames.length > maxVisibleIdst) {
                        preview += ' +' + String(idstNames.length - maxVisibleIdst) + ' more';
                    }
                    idstMetaLine = 'iDST: ' + preview;
                }
            }

            var metaMainLine = metaParts.join(' • ');
            if (idstMetaLine) {
                meta.innerHTML =
                    '<span class="summary-issues-popup-meta-main">' + escapeHtml(metaMainLine) + '</span>' +
                    '<span class="summary-issues-popup-meta-idst">' + escapeHtml(idstMetaLine) + '</span>';
            } else {
                meta.textContent = metaMainLine;
            }

            if (!result || result.Success !== true) {
                body.innerHTML = '<div class="summary-issues-popup-empty">' + escapeHtml((result && result.Message) ? result.Message : 'Unable to load issues.') + '</div>';
                return;
            }

            var groups = result.Groups || [];
            if (!groups.length) {
                body.innerHTML = '<div class="summary-issues-popup-empty">' + escapeHtml(result.Message || 'No matching CMF records found.') + '</div>';
                return;
            }

            var sortBy = normalizeSummaryIssueSortBy(popupState.sortBy || 'alphabetical');
            var sortedGroups = groups.slice();
            if (sortBy === 'hsdcount') {
                sortedGroups.sort(function (left, right) {
                    var leftCount = left && left.HsdIds ? left.HsdIds.length : 0;
                    var rightCount = right && right.HsdIds ? right.HsdIds.length : 0;
                    if (leftCount !== rightCount) {
                        return rightCount - leftCount;
                    }

                    var leftName = String((left && left.GroupName) || '').toLowerCase();
                    var rightName = String((right && right.GroupName) || '').toLowerCase();
                    if (leftName < rightName) return -1;
                    if (leftName > rightName) return 1;
                    return 0;
                });
            } else {
                sortedGroups.sort(function (left, right) {
                    var leftName = String((left && left.GroupName) || '').toLowerCase();
                    var rightName = String((right && right.GroupName) || '').toLowerCase();
                    if (leftName < rightName) return -1;
                    if (leftName > rightName) return 1;
                    return 0;
                });
            }

            var html = '';
            var isPendingSummaryMetric = String(popupState.metricKey || '').toLowerCase() === 'pending';
            var canOpenSummaryMetricDetails =
                !isPendingSummaryMetric &&
                String(popupState.sourceType || 'summary').toLowerCase() === 'summary';
            var metricLabelForDetails = String(popupState.metricLabel || popupState.metricKey || 'Summary Metric');
            var pendingMetricIds = [];
            var pendingMetricSeen = {};
            var summaryMetricIds = [];
            var summaryMetricSeen = {};
            for (var i = 0; i < sortedGroups.length; i++) {
                var group = sortedGroups[i] || {};
                var ids = group.HsdIds || [];
                var issueCount = 0;
                var normalizedIds = [];
                var seenIds = {};
                for (var j = 0; j < ids.length; j++) {
                    var rawId = String(ids[j] || '').trim();
                    if (!rawId) {
                        continue;
                    }

                    var safeId = rawId.replace(/[^0-9]/g, '');
                    if (!safeId) {
                        continue;
                    }

                    if (seenIds[safeId]) {
                        continue;
                    }

                    seenIds[safeId] = true;
                    normalizedIds.push(safeId);

                    if (isPendingSummaryMetric && !pendingMetricSeen[safeId]) {
                        pendingMetricSeen[safeId] = true;
                        pendingMetricIds.push(safeId);
                    }

                    if (canOpenSummaryMetricDetails && !summaryMetricSeen[safeId]) {
                        summaryMetricSeen[safeId] = true;
                        summaryMetricIds.push(safeId);
                    }

                    issueCount += 1;
                }

                var countText = String(issueCount) + ' issues';
                var countHtml = '<span class="summary-issues-group-count">' + escapeHtml(countText) + '</span>';
                if (isPendingSummaryMetric && normalizedIds.length > 0) {
                    var encodedGroupName = encodeURIComponent(String(group.GroupName || 'Pending Summary'));
                    var encodedIds = encodeURIComponent(normalizedIds.join(','));
                    countHtml = '<span class="summary-issues-group-count"><a href="javascript:void(0);" class="summary-issues-group-count-link" onclick="return openPendingSummaryDetailsFromSummaryPopup(decodeURIComponent(\'' + encodedGroupName + '\'), decodeURIComponent(\'' + encodedIds + '\'))">' + escapeHtml(countText) + '</a></span>';
                } else if (canOpenSummaryMetricDetails && normalizedIds.length > 0) {
                    var encodedMetricLabel = encodeURIComponent(metricLabelForDetails);
                    var encodedSummaryGroup = encodeURIComponent(String(group.GroupName || 'Unassigned'));
                    var encodedSummaryIds = encodeURIComponent(normalizedIds.join(','));
                    countHtml = '<span class="summary-issues-group-count"><a href="javascript:void(0);" class="summary-issues-group-count-link" onclick="return openSummaryMetricDetailsFromSummaryPopup(decodeURIComponent(\'' + encodedMetricLabel + '\'), decodeURIComponent(\'' + encodedSummaryGroup + '\'), decodeURIComponent(\'' + encodedSummaryIds + '\'))">' + escapeHtml(countText) + '</a></span>';
                }

                html += '<section class="summary-issues-group">';
                html += '<div class="summary-issues-group-title"><span class="summary-issues-group-name">' + escapeHtml(group.GroupName || 'Unassigned') + '</span>' + countHtml + '</div>';
                html += '</section>';
            }

            body.innerHTML = html;

            if (isPendingSummaryMetric && pendingMetricIds.length > 0) {
                var encodedMetaIds = encodeURIComponent(pendingMetricIds.join(','));
                var metaLinkHtml = '<a href="javascript:void(0);" class="summary-issues-popup-meta-link" onclick="return openPendingSummaryDetailsFromSummaryPopup(\'Total Pending\', decodeURIComponent(\'' + encodedMetaIds + '\'))">' + escapeHtml(String(totalIssues) + ' issues') + '</a>';
                var metaExtraText = metaParts.length > 1 ? (' • ' + escapeHtml(metaParts.slice(1).join(' • '))) : '';
                meta.innerHTML = metaLinkHtml + metaExtraText;
            } else if (canOpenSummaryMetricDetails && summaryMetricIds.length > 0) {
                var encodedSummaryMetaMetric = encodeURIComponent(metricLabelForDetails);
                var encodedSummaryMetaGroup = encodeURIComponent('All Groups');
                var encodedSummaryMetaIds = encodeURIComponent(summaryMetricIds.join(','));
                var summaryMetaLinkHtml = '<a href="javascript:void(0);" class="summary-issues-popup-meta-link" onclick="return openSummaryMetricDetailsFromSummaryPopup(decodeURIComponent(\'' + encodedSummaryMetaMetric + '\'), decodeURIComponent(\'' + encodedSummaryMetaGroup + '\'), decodeURIComponent(\'' + encodedSummaryMetaIds + '\'))">' + escapeHtml(String(totalIssues) + ' issues') + '</a>';
                var summaryMetaExtraText = metaParts.length > 1 ? (' • ' + escapeHtml(metaParts.slice(1).join(' • '))) : '';
                meta.innerHTML = summaryMetaLinkHtml + summaryMetaExtraText;
            }
        }

        function loadSummaryIssueLinks(metricKey, metricLabel, groupBy) {
            var body = document.getElementById('summaryIssuesPopupBody');
            var title = document.getElementById('summaryIssuesPopupTitle');
            if (!body || !title) {
                return;
            }

            var popupState = window.CMF_PORTAL.summaryIssuesPopupState || {};
            var sourceType = String(popupState.sourceType || 'summary');

            if (sourceType === 'overall-heatmap') {
                title.textContent = 'CMF Issues • OVERALL HEATMAP';
            } else if (sourceType === 'trend-week') {
                var trendWeek = String(popupState.weekLabel || '').toUpperCase();
                title.textContent = 'CMF Issues' + (trendWeek ? (' • ' + trendWeek) : '');
            } else {
                title.textContent = 'CMF Issues • ' + String(metricLabel || metricKey || 'Summary Metric').toUpperCase();
            }

            body.innerHTML = '<div class="summary-issues-popup-empty">Loading issues...</div>';

            var endpoint = 'CMF_Web_portal.aspx/GetSummaryCardIssueLinks';
            var payload = {
                metricKey: metricKey,
                groupBy: groupBy,
                dashboardPlatform: window.CMF_PORTAL.currentDashboardPlatform || ''
            };

            if (sourceType === 'overall-heatmap') {
                endpoint = 'CMF_Web_portal.aspx/GetOverallHeatmapIssueLinks';
                payload = {
                    ingredient: String(popupState.ingredient || ''),
                    platformLabel: String(popupState.platformLabel || ''),
                    groupBy: groupBy
                };
            } else if (sourceType === 'trend-week') {
                endpoint = 'CMF_Web_portal.aspx/GetTrendWeekIssueLinks';
                payload = {
                    seriesName: String(popupState.seriesName || ''),
                    weekLabel: String(popupState.weekLabel || ''),
                    weekOffset: parseInt(popupState.weekOffset, 10) || 0,
                    dashboardPlatform: window.CMF_PORTAL.currentDashboardPlatform || '',
                    groupBy: groupBy,
                    includeOpen: popupState.showOpen !== false,
                    includeClosed: popupState.showClosed !== false
                };
            }

            fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                renderSummaryIssueLinks(result);
            })
            .catch(function () {
                renderSummaryIssueLinks({ Success: false, Message: 'Unable to load issue links right now.' });
            });
        }

        function openSummaryIssueLinksPopup(metricKey, metricLabel, popupOptions) {
            var popup = document.getElementById('summaryIssuesPopup');
            var backdrop = document.getElementById('summaryIssuesPopupBackdrop');
            var groupBySelect = document.getElementById('summaryIssuesGroupBy');
            var sortBySelect = document.getElementById('summaryIssuesSortBy');
            var showOpenCheck = document.getElementById('summaryIssuesShowOpen');
            var showClosedCheck = document.getElementById('summaryIssuesShowClosed');

            if (!popup || !backdrop || !groupBySelect || !sortBySelect || !showOpenCheck || !showClosedCheck) {
                return;
            }

            var sourceType = popupOptions && popupOptions.sourceType
                ? String(popupOptions.sourceType || '').toLowerCase()
                : 'summary';
            var options = sourceType === 'overall-heatmap'
                ? ['milestone', 'design', 'component', 'owner', 'idst']
                : (sourceType === 'trend-week'
                    ? buildTrendIssueGroupByOptions(isOverallDashboardSelection())
                    : buildSummaryIssueGroupByOptions(isOverallDashboardSelection()));
            var initialGroupBy = syncSummaryIssueGroupByOptions(options, options.length > 0 ? options[0] : 'ingredient');
            var initialSortBy = syncSummaryIssueSortByOptions('alphabetical');
            var initialTrendStatus = syncSummaryTrendIssueStatusControls(true, true);
            syncSummaryIssuePopupControls(sourceType);

            window.CMF_PORTAL.summaryIssuesPopupState = {
                metricKey: String(metricKey || ''),
                metricLabel: String(metricLabel || ''),
                groupBy: initialGroupBy,
                sortBy: initialSortBy,
                showOpen: initialTrendStatus.showOpen,
                showClosed: initialTrendStatus.showClosed,
                sourceType: sourceType,
                ingredient: popupOptions && popupOptions.ingredient ? String(popupOptions.ingredient || '').trim() : '',
                platformLabel: popupOptions && popupOptions.platformLabel ? String(popupOptions.platformLabel || '').trim() : '',
                seriesName: popupOptions && popupOptions.seriesName ? String(popupOptions.seriesName || '').trim() : '',
                weekLabel: popupOptions && popupOptions.weekLabel ? String(popupOptions.weekLabel || '').trim() : '',
                weekOffset: popupOptions && popupOptions.weekOffset != null ? parseInt(popupOptions.weekOffset, 10) : 0
            };
            window.CMF_PORTAL.summaryIssuesPopupLastResult = null;

            groupBySelect.onchange = function () {
                var state = window.CMF_PORTAL.summaryIssuesPopupState || {};
                state.groupBy = String(groupBySelect.value || 'ingredient');
                window.CMF_PORTAL.summaryIssuesPopupState = state;
                loadSummaryIssueLinks(state.metricKey, state.metricLabel, state.groupBy);
            };

            sortBySelect.onchange = function () {
                var state = window.CMF_PORTAL.summaryIssuesPopupState || {};
                state.sortBy = normalizeSummaryIssueSortBy(sortBySelect.value || 'alphabetical');
                window.CMF_PORTAL.summaryIssuesPopupState = state;
                renderSummaryIssueLinks(window.CMF_PORTAL.summaryIssuesPopupLastResult || null);
            };

            showOpenCheck.onchange = function () {
                var state = window.CMF_PORTAL.summaryIssuesPopupState || {};
                state.showOpen = !!showOpenCheck.checked;
                state.showClosed = !!showClosedCheck.checked;
                window.CMF_PORTAL.summaryIssuesPopupState = state;
                loadSummaryIssueLinks(state.metricKey, state.metricLabel, state.groupBy);
            };

            showClosedCheck.onchange = function () {
                var state = window.CMF_PORTAL.summaryIssuesPopupState || {};
                state.showOpen = !!showOpenCheck.checked;
                state.showClosed = !!showClosedCheck.checked;
                window.CMF_PORTAL.summaryIssuesPopupState = state;
                loadSummaryIssueLinks(state.metricKey, state.metricLabel, state.groupBy);
            };

            popup.classList.add('visible');
            backdrop.classList.add('visible');

            if (!window.CMF_PORTAL.summaryIssuesPopupEscBound) {
                document.addEventListener('keydown', function (event) {
                    if (event && event.key === 'Escape') {
                        closeSummaryIssueLinksPopup();
                    }
                });
                window.CMF_PORTAL.summaryIssuesPopupEscBound = true;
            }

            loadSummaryIssueLinks(
                window.CMF_PORTAL.summaryIssuesPopupState.metricKey,
                window.CMF_PORTAL.summaryIssuesPopupState.metricLabel,
                window.CMF_PORTAL.summaryIssuesPopupState.groupBy
            );
        }

        function openOverallHeatmapIssueLinksPopup(ingredient, platformLabel) {
            var normalizedIngredient = String(ingredient || '').trim() || 'Unassigned';
            var normalizedPlatform = String(platformLabel || '').trim();
            var labelParts = ['Overall Heatmap'];
            if (normalizedPlatform) {
                labelParts.push(normalizedPlatform);
            }
            if (normalizedIngredient) {
                labelParts.push(normalizedIngredient);
            }

            openSummaryIssueLinksPopup('open', labelParts.join(' • '), {
                sourceType: 'overall-heatmap',
                ingredient: normalizedIngredient,
                platformLabel: normalizedPlatform
            });
        }

        function openTrendWeekIssueLinksPopup(seriesName, weekLabel, weekOffset) {
            var normalizedSeries = String(seriesName || '').trim();
            var normalizedWeek = String(weekLabel || '').trim();
            var safeWeekOffset = parseInt(weekOffset, 10);
            if (isNaN(safeWeekOffset) || safeWeekOffset < 0) {
                safeWeekOffset = 0;
            }

            var popupLabel = normalizedSeries + (normalizedWeek ? (' • ' + normalizedWeek) : '');
            openSummaryIssueLinksPopup('open', popupLabel, {
                sourceType: 'trend-week',
                seriesName: normalizedSeries,
                weekLabel: normalizedWeek,
                weekOffset: safeWeekOffset
            });
        }

        function renderOverallSummaryFacts(snapshot, hostId) {
            var host = document.getElementById(hostId || 'homeOverallSummaryFacts');
            if (!host) {
                return;
            }

            var facts = snapshot && snapshot.SummaryFacts ? snapshot.SummaryFacts : [];
            var visibleFacts = [];
            for (var factIndex = 0; factIndex < facts.length; factIndex++) {
                var candidateFact = facts[factIndex] || {};
                if (shouldRenderOverallSummaryFact(candidateFact)) {
                    visibleFacts.push(candidateFact);
                }
            }

            if (!visibleFacts.length) {
                host.innerHTML = '<div class="ccip-empty-state">No summary data available.</div>';
                return;
            }

            host.innerHTML = '';
            for (var i = 0; i < visibleFacts.length; i++) {
                var fact = visibleFacts[i] || {};
                var formattedFact = formatSummaryFactValueAndNote(fact.Value, fact.Note);
                var card = document.createElement('div');
                card.className = 'ccip-summary-fact overall-summary-fact ' + getOverallSummaryToneClass(fact.Label);
                var tooltip = buildOverallSummaryFactTooltip(fact);
                var metricKey = normalizeSummaryIssueMetricKey(fact.Label);
                card.setAttribute('title', tooltip);
                card.setAttribute('aria-label', tooltip);

                if (metricKey) {
                    card.className += ' ccip-summary-clickable';
                    card.setAttribute('role', 'button');
                    card.setAttribute('tabindex', '0');
                    card.setAttribute('title', tooltip + '\nClick to view issue counts by group.');
                    card.setAttribute('aria-label', (fact.Label || 'Summary metric') + '. ' + tooltip + '. Click to view issue counts grouped by selection.');
                }

                card.innerHTML = '<div class="ccip-summary-fact-label">' + escapeHtml(fact.Label || '-') + '</div>' +
                    '<div class="ccip-summary-fact-value">' + renderValueWithDaysSuffix(formattedFact.valueText, 'ccip-days-suffix') + '</div>' +
                    '<div class="ccip-summary-fact-note">' + escapeHtml(formattedFact.noteText) + '</div>';

                if (metricKey) {
                    (function (boundMetricKey, boundLabel, boundCard) {
                        boundCard.addEventListener('click', function (event) {
                            if (event) {
                                event.preventDefault();
                            }
                            openSummaryIssueLinksPopup(boundMetricKey, boundLabel);
                        });

                        boundCard.addEventListener('keydown', function (event) {
                            var key = event && event.key ? String(event.key) : '';
                            if (key === 'Enter' || key === ' ' || key === 'Spacebar') {
                                event.preventDefault();
                                openSummaryIssueLinksPopup(boundMetricKey, boundLabel);
                            }
                        });
                    })(metricKey, fact.Label || 'Summary Metric', card);
                }

                host.appendChild(card);
            }
        }

        function renderCombinedSummaryFacts(snapshot, hostId, summaryCoreId, tptInlineId) {
            var host = document.getElementById(hostId || 'homeOverallSummaryFacts');
            if (!host) {
                return;
            }

            host.innerHTML = '';

            var summaryCore = document.createElement('div');
            summaryCore.id = summaryCoreId || (host.id + 'Core');
            summaryCore.className = 'overall-summary-facts-core';
            host.appendChild(summaryCore);

            var tptInline = document.createElement('div');
            tptInline.id = tptInlineId || (host.id + 'TptInline');
            tptInline.className = 'overall-summary-tpt-inline';
            host.appendChild(tptInline);

            renderOverallSummaryFacts(snapshot, summaryCore.id);
            renderOverallTptBoxes((snapshot && snapshot.TptFacts) ? snapshot.TptFacts : [], tptInline.id);
        }

        function renderOverallCombinedSummaryFacts(snapshot) {
            renderCombinedSummaryFacts(snapshot, 'homeOverallSummaryFacts', 'homeOverallSummaryFactsCore', 'homeOverallSummaryTptInline');
        }

        var OEM_DOMAIN_BY_CANONICAL = {
            lenovo: 'lenovo.com',
            dell: 'dell.com',
            hp: 'hp.com',
            samsung: 'samsung.com',
            asus: 'asus.com',
            honor: 'honor.com',
            microsoft: 'microsoft.com',
            nec: 'nec.com',
            acer: 'acer.com',
            panasonic: 'panasonic.com',
            msi: 'msi.com',
            dynabook: 'dynabook.com',
            fujitsu: 'fujitsu.com',
            lg: 'lg.com',
            gigabyte: 'gigabyte.com',
            intel: 'intel.com',
            xiaomi: 'xiaomi.com',
            asrock: 'asrock.com',
            compal: 'compal.com',
            mechrevo: 'mechrevo.com',
            razer: 'razer.com',
            byd: 'byd.com',
            emdoor: 'emdoor.com',
            wiko: 'wikomobile.com',
            wistron: 'wistron.com'
        };

        var OEM_ALIAS_TO_CANONICAL = {
            lenovo: 'lenovo',
            lenovo_japan: 'lenovo',
            lenovojapan: 'lenovo',
            dell: 'dell',
            hp: 'hp',
            hewlett_packard: 'hp',
            hp_hewlett_packard: 'hp',
            hp_hewlett_packard_: 'hp',
            samsung: 'samsung',
            asustek: 'asus',
            asus: 'asus',
            asustek_computer: 'asus',
            honor: 'honor',
            microsoft_surface: 'microsoft',
            microsoft: 'microsoft',
            surface: 'microsoft',
            nec: 'nec',
            acer: 'acer',
            panasonic: 'panasonic',
            msi_micro_star_international: 'msi',
            micro_star_international: 'msi',
            msi: 'msi',
            dynabook: 'dynabook',
            fujitsu: 'fujitsu',
            lg: 'lg',
            gigabyte: 'gigabyte',
            intel_internal: 'intel',
            intel___internal: 'intel',
            intel: 'intel',
            xiaomi: 'xiaomi',
            asrock: 'asrock',
            compal: 'compal',
            mechrevo: 'mechrevo',
            razer: 'razer',
            byd_company_ltd: 'byd',
            byd: 'byd',
            emdoor: 'emdoor',
            wiko: 'wiko',
            wistron: 'wistron',
            lengda: '',
            unknown: '',
            unassigned: ''
        };

        function normalizeOemNameKey(rawName) {
            var raw = String(rawName || '').toLowerCase().trim();
            if (!raw) {
                return '';
            }

            raw = raw.replace(/&/g, ' and ');
            raw = raw.replace(/[^a-z0-9]+/g, '_');
            raw = raw.replace(/^_+|_+$/g, '');
            raw = raw.replace(/_+/g, '_');
            return raw;
        }

        function resolveOemLogoUrl(oemName) {
            var normalized = normalizeOemNameKey(oemName);
            if (!normalized) {
                return '';
            }

            var canonical = OEM_ALIAS_TO_CANONICAL.hasOwnProperty(normalized)
                ? OEM_ALIAS_TO_CANONICAL[normalized]
                : normalized;

            if (!canonical) {
                return '';
            }

            var domain = OEM_DOMAIN_BY_CANONICAL[canonical] || '';
            if (!domain) {
                return '';
            }

            return 'https://www.google.com/s2/favicons?domain=' + encodeURIComponent(domain) + '&sz=64';
        }

        function renderOverallOemNameCell(oemName) {
            var displayName = String(oemName || 'Unassigned');
            var safeName = escapeHtml(displayName);
            var logoUrl = resolveOemLogoUrl(displayName);

            if (!logoUrl) {
                return '<span class="overall-oem-name-cell"><span class="overall-oem-name-text">' + safeName + '</span></span>';
            }

            return '<span class="overall-oem-name-cell">' +
                '<img class="overall-oem-logo" src="' + escapeHtml(logoUrl) + '" alt="" loading="lazy" referrerpolicy="no-referrer" onerror="this.style.display=\'none\';" />' +
                '<span class="overall-oem-name-text">' + safeName + '</span>' +
                '</span>';
        }

        var OEM_ALIAS_KEYS_SORTED = Object.keys(OEM_ALIAS_TO_CANONICAL).sort(function (left, right) {
            return String(right || '').length - String(left || '').length;
        });

        function resolveDesignNameLogoUrl(designName) {
            var displayName = String(designName || '').trim();
            if (!displayName) {
                return '';
            }

            var directLogoUrl = resolveOemLogoUrl(displayName);
            if (directLogoUrl) {
                return directLogoUrl;
            }

            var normalized = normalizeOemNameKey(displayName);
            if (!normalized) {
                return '';
            }

            var bounded = '_' + normalized + '_';
            for (var i = 0; i < OEM_ALIAS_KEYS_SORTED.length; i++) {
                var alias = OEM_ALIAS_KEYS_SORTED[i];
                var canonical = OEM_ALIAS_TO_CANONICAL[alias];
                if (!canonical) {
                    continue;
                }

                if (bounded.indexOf('_' + alias + '_') < 0) {
                    continue;
                }

                var domain = OEM_DOMAIN_BY_CANONICAL[canonical] || '';
                if (!domain) {
                    continue;
                }

                return 'https://www.google.com/s2/favicons?domain=' + encodeURIComponent(domain) + '&sz=64';
            }

            return '';
        }

        function renderDesignSummaryNameCell(designName) {
            var displayName = String(designName == null ? '' : designName);
            var safeName = escapeHtml(displayName);
            var logoUrl = resolveDesignNameLogoUrl(displayName);

            if (!logoUrl) {
                return '<span class="design-summary-name-cell"><span class="design-summary-name-text">' + safeName + '</span></span>';
            }

            return '<span class="design-summary-name-cell">' +
                '<img class="design-summary-logo" src="' + escapeHtml(logoUrl) + '" alt="" loading="lazy" referrerpolicy="no-referrer" onerror="this.style.display=\'none\';" />' +
                '<span class="design-summary-name-text">' + safeName + '</span>' +
                '</span>';
        }

        function buildDesignSummaryMetricLinkHtml(designName, columnName, valueText) {
            var normalizedDesign = String(designName == null ? '' : designName).trim();
            var normalizedColumn = String(columnName == null ? '' : columnName).trim();
            var normalizedValue = String(valueText == null ? '' : valueText).trim();

            if (!normalizedDesign || !normalizedColumn || !normalizedValue) {
                return escapeHtml(normalizedValue);
            }

            var encodedDesign = encodeURIComponent(normalizedDesign);
            var encodedColumn = encodeURIComponent(normalizedColumn);
            var totalClass = /^total$/i.test(normalizedDesign) ? ' is-total' : '';
            var baseClass = 'ccip-design-metric-link' + totalClass;

            if (/^impl\s*\/\s*verified$/i.test(normalizedColumn)) {
                return '<a href="javascript:void(0);" class="' + baseClass + '" onclick="return showImplementedVerifiedDetailsModal(decodeURIComponent(\'' + encodedDesign + '\'));">' + escapeHtml(normalizedValue) + '</a>';
            }

            return '<a href="javascript:void(0);" class="' + baseClass + '" onclick="return showDriverDetailsModal(decodeURIComponent(\'' + encodedDesign + '\'), decodeURIComponent(\'' + encodedColumn + '\'));">' + escapeHtml(normalizedValue) + '</a>';
        }

        function renderOverallOemTable(rows) {
            var host = document.getElementById('homeOverallOemTable');
            if (!host) {
                return;
            }

            var source = rows || [];
            if (!source.length) {
                host.innerHTML = '<div class="ccip-empty-state">No OEM/customer summary data available.</div>';
                return;
            }

            source.sort(function (left, right) {
                return (right.OpenIssues || 0) - (left.OpenIssues || 0);
            });

            var html = '<table class="overall-oem-table"><thead><tr>' +
                '<th>OEM/Customer</th>' +
                '<th>Total Issues</th>' +
                '<th>Closed</th>' +
                '<th>Implemented</th>' +
                '<th>Open Issues</th>' +
                '<th>CMF Pending</th>' +
                '<th>Average Resolution Time (Days)</th>' +
                '<th>Trend<br><span style="display:block;font-size:9px;font-weight:700;letter-spacing:0.01em;opacity:0.95;">(vs Last Week)</span></th>' +
                '</tr></thead><tbody>';

            function buildOverallOemMetricLinkHtml(oemName, metricKey, metricClass, value) {
                var safeOem = String(oemName || 'Unassigned');
                var safeMetric = String(metricKey || 'total');
                var safeValue = String(value == null ? '' : value);
                var encodedToken = encodeURIComponent(safeOem + '||' + safeMetric);
                return '<a href="javascript:void(0);" class="overall-oem-metric-link ' + metricClass + '" onclick="return showDetailsModal3(decodeURIComponent(\'' + encodedToken + '\'));">' + escapeHtml(safeValue) + '</a>';
            }

            function buildOverallOemNameLinkHtml(oemName) {
                var safeOem = String(oemName || 'Unassigned');
                var encodedToken = encodeURIComponent(safeOem + '||total');
                return '<a href="javascript:void(0);" class="overall-oem-name-link" onclick="return showDetailsModal3(decodeURIComponent(\'' + encodedToken + '\'));">' + renderOverallOemNameCell(safeOem) + '</a>';
            }

            var totals = {
                totalIssues: 0,
                openIssues: 0,
                cmfPending: 0,
                implemented: 0,
                closed: 0,
                weightedResolutionNumerator: 0
            };

            for (var i = 0; i < source.length; i++) {
                var row = source[i] || {};
                var oemName = String(row.OemName || 'Unassigned');
                var totalIssues = parseInt(row.TotalIssues || 0, 10);
                var openIssues = parseInt(row.OpenIssues || 0, 10);
                var cmfPending = parseInt(row.CmfPending || 0, 10);
                var implemented = parseInt(row.Implemented || 0, 10);
                var closed = parseInt(row.Closed || 0, 10);
                var avgResolution = parseFloat(row.AvgResolutionDays || 0);
                var trendPercent = parseInt(row.TrendPercent || 0, 10);
                var trendSafe = isNaN(trendPercent) ? 0 : trendPercent;
                var trendClass = trendSafe > 0 ? 'overall-trend-up' : (trendSafe < 0 ? 'overall-trend-down' : 'overall-trend-flat');
                var trendAbs = Math.abs(trendSafe);
                var trendArrow = trendSafe > 0 ? '↑' : (trendSafe < 0 ? '↓' : '');
                var trendValue = trendSafe > 0
                    ? ('+' + trendAbs + '%')
                    : (trendSafe < 0 ? ('-' + trendAbs + '%') : '0%');

                var safeTotalIssues = isNaN(totalIssues) ? 0 : totalIssues;
                var safeOpenIssues = isNaN(openIssues) ? 0 : openIssues;
                var safeCmfPending = isNaN(cmfPending) ? 0 : cmfPending;
                var safeImplemented = isNaN(implemented) ? 0 : implemented;
                var safeClosed = isNaN(closed) ? 0 : closed;
                var safeAvgResolution = isNaN(avgResolution) ? 0 : avgResolution;

                totals.totalIssues += safeTotalIssues;
                totals.openIssues += safeOpenIssues;
                totals.cmfPending += safeCmfPending;
                totals.implemented += safeImplemented;
                totals.closed += safeClosed;
                totals.weightedResolutionNumerator += (safeAvgResolution * safeTotalIssues);

                html += '<tr>' +
                    '<td>' + buildOverallOemNameLinkHtml(oemName) + '</td>' +
                    '<td>' + buildOverallOemMetricLinkHtml(oemName, 'total', 'overall-oem-metric-total', safeTotalIssues) + '</td>' +
                    '<td>' + buildOverallOemMetricLinkHtml(oemName, 'closed', 'overall-oem-metric-closed', safeClosed) + '</td>' +
                    '<td>' + buildOverallOemMetricLinkHtml(oemName, 'implemented', 'overall-oem-metric-implemented', safeImplemented) + '</td>' +
                    '<td>' + buildOverallOemMetricLinkHtml(oemName, 'open', 'overall-oem-metric-open', safeOpenIssues) + '</td>' +
                    '<td>' + buildOverallOemMetricLinkHtml(oemName, 'pending', 'overall-oem-metric-pending', safeCmfPending) + '</td>' +
                    '<td>' + escapeHtml(safeAvgResolution.toFixed(1)) + '</td>' +
                        '<td><span class="overall-oem-trend ' + trendClass + '"><span class="overall-oem-trend-arrow">' + escapeHtml(trendArrow) + '</span><span class="overall-oem-trend-value">' + escapeHtml(trendValue) + '</span></span></td>' +
                    '</tr>';
            }

            var totalAvgResolution = totals.totalIssues > 0
                ? (totals.weightedResolutionNumerator / totals.totalIssues)
                : 0;

            var overallOemTotalToken = '__all_oems__';

            html += '<tr class="overall-oem-total-row">' +
                '<td><span class="overall-oem-total-label">Total</span></td>' +
                '<td>' + buildOverallOemMetricLinkHtml(overallOemTotalToken, 'total', 'overall-oem-metric-total', totals.totalIssues) + '</td>' +
                '<td>' + buildOverallOemMetricLinkHtml(overallOemTotalToken, 'closed', 'overall-oem-metric-closed', totals.closed) + '</td>' +
                '<td>' + buildOverallOemMetricLinkHtml(overallOemTotalToken, 'implemented', 'overall-oem-metric-implemented', totals.implemented) + '</td>' +
                '<td>' + buildOverallOemMetricLinkHtml(overallOemTotalToken, 'open', 'overall-oem-metric-open', totals.openIssues) + '</td>' +
                '<td>' + buildOverallOemMetricLinkHtml(overallOemTotalToken, 'pending', 'overall-oem-metric-pending', totals.cmfPending) + '</td>' +
                '<td>' + escapeHtml(totalAvgResolution.toFixed(1)) + '</td>' +
                '<td><span class="overall-oem-total-trend">-</span></td>' +
                '</tr>';

            html += '</tbody></table>';
            host.innerHTML = html;
        }

        function readOverallFactNumber(facts, pattern) {
            var source = facts || [];
            for (var i = 0; i < source.length; i++) {
                var fact = source[i] || {};
                var label = String(fact.Label || '');
                if (!pattern.test(label)) {
                    continue;
                }

                var match = String(fact.Value || '').match(/-?\d+(?:\.\d+)?/);
                return match ? parseFloat(match[0]) : 0;
            }
            return 0;
        }

        function findTptFactByLabel(facts, pattern) {
            var source = facts || [];
            for (var i = 0; i < source.length; i++) {
                var fact = source[i] || {};
                if (pattern.test(String(fact.Label || ''))) {
                    return fact;
                }
            }
            return null;
        }

        function renderOverallAgeBucketLabel(rawLabel) {
            var text = String(rawLabel || '-').trim();
            var match = text.match(/^(.*)\s+(Day|Days)$/i);
            if (!match) {
                return '<span class="overall-age-label-main">' + escapeHtml(text) + '</span>';
            }

            return '<span class="overall-age-label-main">' + escapeHtml(match[1]) + '</span>' +
                '<span class="overall-age-label-unit">' + escapeHtml(match[2]) + '</span>';
        }

        function getTptTrendToneClass(text) {
            var value = String(text || '').trim();
            if (!value || value.indexOf('0 ') === 0) {
                return 'overall-tpt-trend-neutral';
            }

            if (value.charAt(0) === '-') {
                return 'overall-tpt-trend-better';
            }

            if (value.charAt(0) === '+') {
                return 'overall-tpt-trend-worse';
            }

            return 'overall-tpt-trend-neutral';
        }

        function getLegacyTptThreshold(kind) {
            if (kind === 'disposition') {
                return 2;
            }

            if (kind === 'resolution') {
                return 18;
            }

            return 21;
        }

        function getLegacyTptValueToneClass(kind, value) {
            var numeric = parseInt(value, 10);
            if (isNaN(numeric)) {
                numeric = 0;
            }

            return numeric > getLegacyTptThreshold(kind)
                ? 'overall-tpt-value-bad'
                : 'overall-tpt-value-good';
        }

        function buildTptColorLegend(kind) {
            var threshold = getLegacyTptThreshold(kind);
            var thresholdText = String(threshold);
            return '<div class="overall-tpt-legend" aria-label="Color legend: green icon, less than or equal to ' + thresholdText + ' days, greater than ' + thresholdText + ' days, red icon">' +
                '<span class="overall-tpt-legend-dot overall-tpt-legend-dot-good" aria-hidden="true"></span>' +
                '<span class="overall-tpt-legend-threshold">&lt;= ' + thresholdText + ' days &gt;</span>' +
                '<span class="overall-tpt-legend-dot overall-tpt-legend-dot-bad" aria-hidden="true"></span>' +
                '</div>';
        }

        function buildOverallTptTooltip(kind, useSpecificPlatformCopy) {
            var lines = [];
            if (useSpecificPlatformCopy === true) {
                if (kind === 'disposition') {
                    lines.push('CMF disposition TPT: CMF decided date - CMF ask date');
                    return lines.join('\n');
                }
                if (kind === 'resolution') {
                    lines.push('CMF resolution TPT: CMF decided date - Implemented date');
                    return lines.join('\n');
                }
            }

            if (kind === 'disposition') {
                lines.push('Average of time taken to disposition a CMF after it is asked.');
                lines.push('Calculated as average days between date_cmf_ask (fallback date_cmf_decided) and date_cmf_decided.');
            } else if (kind === 'resolution') {
                lines.push('Average of time taken to resolve a CMF after decision.');
                lines.push('Calculated as average days between date_cmf_decided (fallback implemented_date) and implemented_date; negative values are treated as 0.');
            } else if (kind === 'overall') {
                lines.push('Average of overall cycle time for CMF handling.');
                lines.push('Calculated as average days_active across scoped CMF records.');
            } else {
                lines.push('Turnaround metric in days for the selected scope.');
            }
            return lines.join('\n');
        }

        function renderOverallTptBoxes(tptFacts, hostId) {
            var host = document.getElementById(hostId || 'homeOverallTptBoxes');
            if (!host) {
                return;
            }

            var renderHostId = String(host.id || hostId || '');
            var isSpecificPlatformTpt = renderHostId === 'homeSpecificSummaryFactsDashboardTptInline'
                || renderHostId === 'homeSpecificSummaryFactsCmfTptInline';

            var facts = tptFacts || [];
            if (!facts.length) {
                host.innerHTML = '<div class="ccip-empty-state">No TPT metrics available.</div>';
                return;
            }

            var dispositionFact = findTptFactByLabel(facts, /cmf\s+disposition\s+tpt/i);
            var resolutionFact = findTptFactByLabel(facts, /cmf\s+resolution\s+tpt/i);
            var overallFact = findTptFactByLabel(facts, /cmf\s+overall\s+tpt/i);

            function toDayValue(fact) {
                var parsed = parseInt(fact && fact.Value ? fact.Value : 0, 10);
                return isNaN(parsed) ? 0 : parsed;
            }

            function trendText(fact) {
                var trend = fact && fact.TrendNote ? String(fact.TrendNote).trim() : '';
                if (trend) return trend;
                return '0 days vs last week';
            }

            var dispositionDays = toDayValue(dispositionFact);
            var resolutionDays = toDayValue(resolutionFact);
            var overallDays = toDayValue(overallFact);

            var html = '';
            html += '<div class="overall-tpt-box" data-tpt-kind="disposition">' +
                '<div class="overall-tpt-label">DISPOSITION TPT</div>' +
                '<div class="overall-tpt-value ' + getLegacyTptValueToneClass('disposition', dispositionDays) + '">' + escapeHtml(String(dispositionDays)) + ' <span class="overall-tpt-value-unit ccip-days-suffix">days</span></div>' +
                buildTptColorLegend('disposition') +
                '</div>';

            html += '<div class="overall-tpt-box" data-tpt-kind="resolution">' +
                '<div class="overall-tpt-label">RESOLUTION TPT</div>' +
                '<div class="overall-tpt-value ' + getLegacyTptValueToneClass('resolution', resolutionDays) + '">' + escapeHtml(String(resolutionDays)) + ' <span class="overall-tpt-value-unit ccip-days-suffix">days</span></div>' +
                buildTptColorLegend('resolution') +
                '</div>';

            html += '<div class="overall-tpt-box" data-tpt-kind="overall">' +
                '<div class="overall-tpt-label">OVERALL TPT</div>' +
                '<div class="overall-tpt-value ' + getLegacyTptValueToneClass('overall', overallDays) + '">' + escapeHtml(String(overallDays)) + ' <span class="overall-tpt-value-unit ccip-days-suffix">days</span></div>' +
                buildTptColorLegend('overall') +
                '</div>';

            host.innerHTML = html;

            var tptBoxes = host.querySelectorAll('.overall-tpt-box[data-tpt-kind]');
            for (var boxIndex = 0; boxIndex < tptBoxes.length; boxIndex++) {
                var box = tptBoxes[boxIndex];
                var kind = box.getAttribute('data-tpt-kind') || '';
                var tooltip = buildOverallTptTooltip(kind, isSpecificPlatformTpt);
                box.setAttribute('title', tooltip);
                box.setAttribute('aria-label', tooltip);
            }
        }

        function renderOverallHeatmap(snapshot) {
            var host = document.getElementById('homeOverallHeatmap');
            if (!host) {
                return;
            }

            function parseOverallHeatmapLosCount(statusTooltipText) {
                var tooltip = String(statusTooltipText || '');
                var losMatch = tooltip.match(/LOS:\s*(\d+)/i);
                if (!losMatch || losMatch.length < 2) {
                    return 0;
                }

                var losValue = parseInt(losMatch[1], 10);
                return isNaN(losValue) ? 0 : Math.max(0, losValue);
            }

            var platforms = snapshot && snapshot.HeatmapPlatforms ? snapshot.HeatmapPlatforms : [];
            var rows = snapshot && snapshot.ComponentRiskHeatmap ? snapshot.ComponentRiskHeatmap : [];

            if (!platforms.length || !rows.length) {
                host.innerHTML = '<div class="ccip-empty-state">No heatmap data available.</div>';
                return;
            }

            var overallRiskThresholds = normalizeComponentRiskThresholds(
                window.CMF_PORTAL && window.CMF_PORTAL.overallRiskThresholds
                    ? window.CMF_PORTAL.overallRiskThresholds
                    : null
            );
            var applyLosSubtraction = !!(window.CMF_PORTAL && window.CMF_PORTAL.overallRiskSubtractLos === true);
            if (window.CMF_PORTAL) {
                window.CMF_PORTAL.overallRiskThresholds = overallRiskThresholds;
                window.CMF_PORTAL.overallRiskSubtractLos = applyLosSubtraction;
            }

            var html = '<div class="component-summary-heatmap-layout"><div class="component-summary-heatmap-scroll"><div class="overall-heatmap-board" style="grid-template-columns: 112px repeat(' + platforms.length + ', 46px);">';
            html += '<div class="overall-heatmap-corner">INGREDIENT</div>';
            for (var c = 0; c < platforms.length; c++) {
                html += '<div class="overall-heatmap-platform">' + escapeHtml(platforms[c]) + '</div>';
            }

            for (var r = 0; r < rows.length; r++) {
                var row = rows[r] || {};
                var values = row.Values || [];
                var statusBreakdowns = row.StatusBreakdowns || [];
                var ingredientLabel = String(row.Ingredient || 'Unassigned');
                html += '<div class="overall-heatmap-ingredient">' + escapeHtml(ingredientLabel) + '</div>';

                for (var col = 0; col < platforms.length; col++) {
                    var platformLabel = String(platforms[col] || '');
                    var value = parseInt(values[col] || 0, 10);
                    var normalizedValue = isNaN(value) ? 0 : value;
                    var statusTooltip = statusBreakdowns.length > col ? String(statusBreakdowns[col] || '') : '';
                    var losCount = parseOverallHeatmapLosCount(statusTooltip);
                    var colorBasisValue = applyLosSubtraction ? (normalizedValue - losCount) : normalizedValue;
                    if (isNaN(colorBasisValue) || colorBasisValue < 0) {
                        colorBasisValue = 0;
                    }
                    var css = getOverallHeatClassFromOpenUniqueByThreshold(colorBasisValue, overallRiskThresholds);
                    var displayValue = String(normalizedValue);
                    if (applyLosSubtraction && losCount > 0) {
                        displayValue += ' (' + String(losCount) + ')';
                    }
                    var isClickable = normalizedValue > 0 && !!platformLabel;
                    var tooltipText = statusTooltip || ('Open Unique: ' + String(normalizedValue));
                    if (isClickable) {
                        tooltipText += '\nClick to view issue counts by group.';
                    }

                    var extraAttrs = ' title="' + escapeHtml(tooltipText) + '" aria-label="' + escapeHtml(tooltipText) + '"';
                    if (isClickable) {
                        extraAttrs += ' role="button" tabindex="0" data-heatmap-clickable="1" data-ingredient="' + escapeHtml(ingredientLabel) + '" data-platform-label="' + escapeHtml(platformLabel) + '"';
                    }

                    html += '<div class="overall-heatmap-tile ' + css + (isClickable ? ' overall-heatmap-clickable' : '') + '"' + extraAttrs + '>' + escapeHtml(displayValue) + '</div>';
                }
            }

            html += '</div></div>';
            var criticalStart = overallRiskThresholds.high;
            html += '<div class="component-summary-legend">'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch low"></span>'
                    + '<span class="component-summary-legend-label">Low :</span>'
                    + '<span class="component-summary-legend-derived">' + String(overallRiskThresholds.low) + '</span>'
                + '</div>'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch med"></span>'
                    + '<span class="component-summary-legend-label">Medium &lt;</span>'
                    + '<input type="number" min="2" step="1" class="component-summary-legend-value component-summary-legend-input" data-threshold="overall-med" value="' + String(overallRiskThresholds.med) + '">'
                + '</div>'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch high"></span>'
                    + '<span class="component-summary-legend-label">High &lt;</span>'
                    + '<input type="number" min="' + String(overallRiskThresholds.med + 1) + '" step="1" class="component-summary-legend-value component-summary-legend-input" data-threshold="overall-high" value="' + String(overallRiskThresholds.high) + '">'
                + '</div>'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch critical"></span>'
                    + '<span class="component-summary-legend-label">Critical :</span>'
                    + '<span class="component-summary-legend-derived">' + String(criticalStart) + '+</span>'
                + '</div>'
                + '<div class="component-summary-legend-row component-summary-legend-row-toggle">'
                    + '<label class="component-summary-legend-toggle">'
                        + '<span>Subtract LOS</span>'
                        + '<input type="checkbox" class="component-summary-legend-toggle-input" data-threshold="overall-los-subtract-toggle"' + (applyLosSubtraction ? ' checked' : '') + '>'
                    + '</label>'
                + '</div>'
                + '</div>'
                + '<div class="component-summary-warning-popup" role="alert" aria-live="assertive"></div>'
                + '</div>';
            host.innerHTML = html;

            var clickableTiles = host.querySelectorAll('.overall-heatmap-tile[data-heatmap-clickable="1"]');
            for (var tileIndex = 0; tileIndex < clickableTiles.length; tileIndex++) {
                (function (tile) {
                    if (!tile) {
                        return;
                    }

                    tile.addEventListener('click', function (event) {
                        if (event) {
                            event.preventDefault();
                        }
                        openOverallHeatmapIssueLinksPopup(
                            tile.getAttribute('data-ingredient') || 'Unassigned',
                            tile.getAttribute('data-platform-label') || ''
                        );
                    });

                    tile.addEventListener('keydown', function (event) {
                        var key = event && event.key ? String(event.key) : '';
                        if (key === 'Enter' || key === ' ' || key === 'Spacebar') {
                            event.preventDefault();
                            openOverallHeatmapIssueLinksPopup(
                                tile.getAttribute('data-ingredient') || 'Unassigned',
                                tile.getAttribute('data-platform-label') || ''
                            );
                        }
                    });
                })(clickableTiles[tileIndex]);
            }

            var medInput = host.querySelector('[data-threshold="overall-med"]');
            var highInput = host.querySelector('[data-threshold="overall-high"]');
            var losToggleInput = host.querySelector('[data-threshold="overall-los-subtract-toggle"]');
            var warningPopup = host.querySelector('.component-summary-warning-popup');
            var warningPopupTimer = null;
            var lastValidThresholds = {
                med: overallRiskThresholds.med,
                high: overallRiskThresholds.high
            };

            if (losToggleInput) {
                losToggleInput.addEventListener('change', function () {
                    if (window.CMF_PORTAL) {
                        window.CMF_PORTAL.overallRiskSubtractLos = !!losToggleInput.checked;
                    }
                    window.setTimeout(function () {
                        if (!host || !host.isConnected) {
                            return;
                        }
                        renderOverallHeatmap(snapshot);
                    }, 0);
                });
            }

            function showLegendError(message) {
                if (!warningPopup) {
                    return;
                }

                if (warningPopupTimer) {
                    window.clearTimeout(warningPopupTimer);
                    warningPopupTimer = null;
                }

                var text = String(message || '').trim();
                if (!text) {
                    warningPopup.textContent = '';
                    warningPopup.classList.remove('component-summary-warning-popup-visible');
                    return;
                }

                warningPopup.textContent = text;
                warningPopup.classList.add('component-summary-warning-popup-visible');
                warningPopupTimer = window.setTimeout(function () {
                    if (!warningPopup) {
                        return;
                    }
                    warningPopup.textContent = '';
                    warningPopup.classList.remove('component-summary-warning-popup-visible');
                    warningPopupTimer = null;
                }, 2400);
            }

            function setInputInvalidState(input, isInvalid) {
                if (!input) {
                    return;
                }
                if (isInvalid) {
                    input.classList.add('component-summary-legend-input-invalid');
                    return;
                }
                input.classList.remove('component-summary-legend-input-invalid');
            }

            function syncLegendInputMins() {
                if (medInput) {
                    medInput.min = '2';
                }
                if (highInput) {
                    var medValue = parseInt(medInput ? medInput.value : lastValidThresholds.med, 10);
                    var highMin = isNaN(medValue) ? (lastValidThresholds.med + 1) : (medValue + 1);
                    highInput.min = String(Math.max(3, highMin));
                }
            }

            function resetLegendInputsToLastValid() {
                if (medInput) {
                    medInput.value = String(lastValidThresholds.med);
                }
                if (highInput) {
                    highInput.value = String(lastValidThresholds.high);
                }
                syncLegendInputMins();
            }

            function validateLegendThresholdInputs() {
                var medValue = parseInt(medInput ? medInput.value : '', 10);
                var highValue = parseInt(highInput ? highInput.value : '', 10);

                if (isNaN(medValue) || medValue < 2) {
                    return {
                        valid: false,
                        message: 'Medium must be 2 or greater.',
                        invalidField: 'med'
                    };
                }

                if (isNaN(highValue) || highValue <= medValue) {
                    return {
                        valid: false,
                        message: 'High must be greater than Medium.',
                        invalidField: 'high'
                    };
                }

                return {
                    valid: true,
                    med: medValue,
                    high: highValue
                };
            }

            function rerenderOverallHeatmapFromLegendInputs() {
                setInputInvalidState(medInput, false);
                setInputInvalidState(highInput, false);

                var validation = validateLegendThresholdInputs();
                if (!validation.valid) {
                    showLegendError(validation.message);
                    if (validation.invalidField === 'med') {
                        setInputInvalidState(medInput, true);
                    } else if (validation.invalidField === 'high') {
                        setInputInvalidState(highInput, true);
                    }
                    resetLegendInputsToLastValid();
                    return;
                }

                showLegendError('');

                var nextThresholds = normalizeComponentRiskThresholds({
                    med: validation.med,
                    high: validation.high
                });

                lastValidThresholds.med = nextThresholds.med;
                lastValidThresholds.high = nextThresholds.high;
                syncLegendInputMins();

                if (window.CMF_PORTAL) {
                    window.CMF_PORTAL.overallRiskThresholds = nextThresholds;
                }
                window.setTimeout(function () {
                    try {
                        if (!host || !host.isConnected) {
                            return;
                        }
                        renderOverallHeatmap(snapshot);
                    } catch (rerenderError) {
                        console.error('Overall heatmap rerender failed:', rerenderError);
                        resetLegendInputsToLastValid();
                        showLegendError('Invalid threshold input.');
                    }
                }, 0);
            }

            var thresholdInputs = [medInput, highInput];
            for (var inputIndex = 0; inputIndex < thresholdInputs.length; inputIndex++) {
                var thresholdInput = thresholdInputs[inputIndex];
                if (!thresholdInput) {
                    continue;
                }
                thresholdInput.addEventListener('input', function () {
                    showLegendError('');
                    setInputInvalidState(medInput, false);
                    setInputInvalidState(highInput, false);
                    syncLegendInputMins();
                });
                thresholdInput.addEventListener('keydown', function (event) {
                    if (event && event.key === 'Enter') {
                        event.preventDefault();
                        event.stopPropagation();
                        if (event.target && typeof event.target.dispatchEvent === 'function') {
                            event.target.dispatchEvent(new Event('change', { bubbles: true }));
                        }
                        if (event.target && typeof event.target.blur === 'function') {
                            event.target.blur();
                        }
                    }
                });
                thresholdInput.addEventListener('change', rerenderOverallHeatmapFromLegendInputs);
            }

            syncLegendInputMins();
        }

        function renderOverallPendingSummary(snapshot) {
            var pieHost = document.getElementById('homeOverallPendingPie');
            var breakdownHost = document.getElementById('homeOverallPendingBreakdown');
            var ageHost = document.getElementById('homeOverallPendingAgeBuckets');
            if (!pieHost || !breakdownHost || !ageHost) {
                return;
            }

            function normalizePendingSummaryIdsCsv(rawCsv) {
                var tokens = String(rawCsv == null ? '' : rawCsv).split(',');
                var seen = {};
                var normalized = [];

                for (var index = 0; index < tokens.length; index++) {
                    var safeId = String(tokens[index] || '').replace(/[^0-9]/g, '').trim();
                    if (!safeId || seen[safeId]) {
                        continue;
                    }

                    seen[safeId] = true;
                    normalized.push(safeId);
                }

                return normalized.join(',');
            }

            function parsePendingSummaryRowCell(rawValue) {
                var valueText = String(rawValue == null ? '' : rawValue);
                var count = parseMetricInt(valueText);
                var idsCsv = '';
                var delimiter = (typeof PENDING_SUMMARY_LINKS_DELIMITER === 'string' && PENDING_SUMMARY_LINKS_DELIMITER)
                    ? PENDING_SUMMARY_LINKS_DELIMITER
                    : ':::OPEN_HSDS:::';

                if (valueText.indexOf(delimiter) >= 0) {
                    var parts = valueText.split(delimiter);
                    idsCsv = normalizePendingSummaryIdsCsv(parts.length > 1 ? parts[1] : '');
                }

                return {
                    count: count,
                    idsCsv: idsCsv
                };
            }

            function buildPendingSummaryBreakdownLinkHtml(ingredientName, idsCsv, text, cssClass) {
                var safeText = String(text == null ? '' : text);
                var normalizedIdsCsv = normalizePendingSummaryIdsCsv(idsCsv);
                if (!normalizedIdsCsv) {
                    return escapeHtml(safeText);
                }

                var encodedIngredient = encodeURIComponent(String(ingredientName == null ? '' : ingredientName));
                var encodedIds = encodeURIComponent(normalizedIdsCsv);
                return '<a href="javascript:void(0);" class="' + cssClass + '" onclick="return showPendingSummaryIssues(decodeURIComponent(\'' + encodedIngredient + '\'), decodeURIComponent(\'' + encodedIds + '\'));">' + escapeHtml(safeText) + '</a>';
            }

            function parseMetricInt(value) {
                var raw = String(value == null ? '' : value);
                if (raw.indexOf(':::OPEN_HSDS:::') >= 0) {
                    raw = raw.split(':::OPEN_HSDS:::')[0];
                }
                var cleaned = raw.replace(/[^\d\-]/g, '');
                var parsed = parseInt(cleaned, 10);
                return isNaN(parsed) ? 0 : parsed;
            }

            var pendingSummary = snapshot && snapshot.PendingSummary ? snapshot.PendingSummary : null;
            var rows = pendingSummary && pendingSummary.Rows ? pendingSummary.Rows : [];
            var columns = pendingSummary && pendingSummary.Columns ? pendingSummary.Columns : [];
            var pendingValueColumnIndex = -1;

            for (var c = 0; c < columns.length; c++) {
                var header = String(columns[c] || '').toLowerCase();
                if (header.indexOf('count') >= 0) {
                    pendingValueColumnIndex = c;
                    break;
                }
            }

            if (pendingValueColumnIndex < 0) {
                pendingValueColumnIndex = Math.max(1, columns.length - 1);
            }

            var breakdown = [];
            var pendingSummaryTotalIds = [];
            var pendingSummaryTotalIdSeen = {};
            for (var i = 0; i < rows.length; i++) {
                var source = rows[i] || [];
                var label = source.length > 0 ? String(source[0] || '').trim() : '';
                if (!label || /^total$/i.test(label)) {
                    continue;
                }

                var pendingCell = source.length > pendingValueColumnIndex ? source[pendingValueColumnIndex] : 0;
                var pendingCellData = parsePendingSummaryRowCell(pendingCell);
                if (pendingCellData.idsCsv) {
                    var totalTokens = String(pendingCellData.idsCsv).split(',');
                    for (var totalTokenIndex = 0; totalTokenIndex < totalTokens.length; totalTokenIndex++) {
                        var totalSafeId = String(totalTokens[totalTokenIndex] || '').replace(/[^0-9]/g, '').trim();
                        if (!totalSafeId || pendingSummaryTotalIdSeen[totalSafeId]) {
                            continue;
                        }

                        pendingSummaryTotalIdSeen[totalSafeId] = true;
                        pendingSummaryTotalIds.push(totalSafeId);
                    }
                }
                breakdown.push({
                    name: label,
                    value: pendingCellData.count,
                    idsCsv: pendingCellData.idsCsv
                });
            }

            var pendingSummaryTotalIdsCsv = pendingSummaryTotalIds.join(',');

            breakdown.sort(function (left, right) {
                return right.value - left.value;
            });

            var total = parseMetricInt(snapshot && snapshot.PendingIssues ? snapshot.PendingIssues : 0);
            if (!total || isNaN(total)) {
                total = 0;
                for (var b = 0; b < breakdown.length; b++) {
                    total += breakdown[b].value;
                }
            }

            var bucketRows = snapshot && snapshot.PendingAgeBuckets ? snapshot.PendingAgeBuckets : [];
            var bucketTotal = 0;
            for (var bt = 0; bt < bucketRows.length; bt++) {
                var bucketValue = parseMetricInt(bucketRows[bt] && bucketRows[bt].Value);
                bucketTotal += bucketValue;
            }
            if (total <= 0 && bucketTotal > 0) {
                total = bucketTotal;
            }

            var pieColors = ['#2563eb', '#f59e0b', '#22c55e', '#ef4444', '#8b5cf6', '#0ea5e9', '#14b8a6', '#f97316'];
            var topBreakdown = breakdown;

            if (!topBreakdown.length) {
                breakdownHost.innerHTML = '<div class="ccip-empty-state">No pending ingredient data.</div>';
            } else {
                var breakdownHtml = '';
                for (var k = 0; k < topBreakdown.length; k++) {
                    var item = topBreakdown[k];
                    var pct = total > 0 ? (item.value * 100 / total) : 0;
                    var dot = pieColors[k % pieColors.length];
                    var countHtml = buildPendingSummaryBreakdownLinkHtml(item.name, item.idsCsv, String(item.value), 'overall-break-count-link');
                    breakdownHtml += '<div class="overall-break-row">' +
                        '<span class="overall-break-left">' +
                            '<span class="overall-break-dot" style="background:' + dot + ';"></span>' +
                            '<span class="overall-break-name">' + escapeHtml(item.name) + '</span>' +
                        '</span>' +
                        '<span class="overall-break-metrics">' +
                            '<strong class="overall-break-count">' + countHtml + '</strong>' +
                            '<span class="overall-break-pct">(' + escapeHtml(pct.toFixed(1)) + '%)</span>' +
                        '</span>' +
                        '</div>';
                }
                breakdownHost.innerHTML = breakdownHtml;
            }

            if (!bucketRows.length) {
                ageHost.innerHTML = '<div class="ccip-empty-state">No pending age data.</div>';
            } else {
                var ageHtml = '';
                for (var a = 0; a < bucketRows.length; a++) {
                    var bucket = bucketRows[a] || {};
                    var count = parseMetricInt(bucket.Value || 0);
                    var safeCount = isNaN(count) ? 0 : count;
                    var agePct = total > 0 ? ((isNaN(count) ? 0 : count) * 100 / total) : 0;
                    var agePctClamped = Math.max(0, Math.min(100, agePct));
                    var bucketLabel = String(bucket.Name || '-');
                    var encodedBucketLabel = encodeURIComponent(bucketLabel);
                    var countLinkHtml = '<a href="javascript:void(0);" class="ccip-pending-summary-count-link" onclick="return showPendingAgeBucketIssues(decodeURIComponent(\'' + encodedBucketLabel + '\'))">' + escapeHtml(String(safeCount)) + '</a>';
                    ageHtml += '<div class="overall-age-row">' +
                        '<span class="overall-age-label">' + renderOverallAgeBucketLabel(bucketLabel) + '</span>' +
                        '<span class="overall-age-bar-track"><span class="overall-age-bar-fill" style="width:' + escapeHtml(agePctClamped.toFixed(1)) + '%;"></span></span>' +
                        '<strong class="overall-age-metrics">' + countLinkHtml + ' (' + escapeHtml(agePct.toFixed(1)) + '%)</strong>' +
                        '</div>';
                }
                ageHost.innerHTML = ageHtml;
            }

            if (!topBreakdown.length || total <= 0) {
                if (window.CMF_PORTAL.homeOverallPendingPieChart) {
                    window.CMF_PORTAL.homeOverallPendingPieChart.dispose();
                    window.CMF_PORTAL.homeOverallPendingPieChart = null;
                }
                pieHost.innerHTML = '<div class="ccip-empty-state">No pending chart data.</div>';
                return;
            }

            if (typeof echarts === 'undefined') {
                return;
            }

            pieHost.innerHTML = '';
            if (!window.CMF_PORTAL.homeOverallPendingPieChart) {
                window.CMF_PORTAL.homeOverallPendingPieChart = echarts.init(pieHost);
            }

            window.CMF_PORTAL.homeOverallPendingPieChart.setOption({
                animation: false,
                tooltip: {
                    trigger: 'item',
                    confine: false,
                    appendToBody: true,
                    extraCssText: 'z-index: 9999;'
                },
                color: pieColors,
                graphic: [
                    {
                        type: 'text',
                        left: 'center',
                        top: '36%',
                        silent: !pendingSummaryTotalIdsCsv,
                        cursor: pendingSummaryTotalIdsCsv ? 'pointer' : 'default',
                        onclick: pendingSummaryTotalIdsCsv
                            ? function () {
                                showPendingSummaryIssues('Total Pending', pendingSummaryTotalIdsCsv);
                            }
                            : null,
                        z: 10,
                        style: {
                            text: String(total),
                            fill: '#1d4ed8',
                            font: '900 14px Segoe UI',
                            textAlign: 'center',
                            textVerticalAlign: 'middle'
                        }
                    },
                    {
                        type: 'text',
                        left: 'center',
                        top: '52.5%',
                        silent: true,
                        z: 10,
                        style: {
                            text: 'Total\nPending',
                            fill: '#64748b',
                            font: '700 8px Segoe UI',
                            lineHeight: 9,
                            textAlign: 'center',
                            textVerticalAlign: 'middle'
                        }
                    }
                ],
                series: [{
                    type: 'pie',
                    cursor: 'pointer',
                    radius: ['52%', '84%'],
                    center: ['50%', '50%'],
                    itemStyle: {
                        borderColor: '#ffffff',
                        borderWidth: 1
                    },
                    label: { show: false },
                    data: topBreakdown.map(function (item) {
                        return { name: item.name, value: item.value, idsCsv: item.idsCsv || '' };
                    })
                }]
            }, true);

            window.CMF_PORTAL.homeOverallPendingPieChart.off('click');
            window.CMF_PORTAL.homeOverallPendingPieChart.on('click', function (params) {
                if (!params || params.componentType !== 'series') {
                    return;
                }

                var payload = params.data || {};
                var idsCsv = normalizePendingSummaryIdsCsv(payload.idsCsv || '');
                if (!idsCsv) {
                    return;
                }

                var ingredientName = String(payload.name || params.name || 'Pending Summary').trim();
                showPendingSummaryIssues(ingredientName, idsCsv);
            });

            if (window.CMF_PORTAL.homeOverallPendingPieCenterClickHandler) {
                window.CMF_PORTAL.homeOverallPendingPieChart.getZr().off('click', window.CMF_PORTAL.homeOverallPendingPieCenterClickHandler);
                window.CMF_PORTAL.homeOverallPendingPieCenterClickHandler = null;
            }

            if (pendingSummaryTotalIdsCsv) {
                var centerClickHandler = function (event) {
                    if (!event) {
                        return;
                    }

                    var x = typeof event.offsetX === 'number' ? event.offsetX : 0;
                    var y = typeof event.offsetY === 'number' ? event.offsetY : 0;
                    var centerX = pieHost.clientWidth / 2;
                    var centerY = pieHost.clientHeight / 2;
                    var innerRadius = Math.min(pieHost.clientWidth, pieHost.clientHeight) * 0.52 / 2;
                    var dx = x - centerX;
                    var dy = y - centerY;
                    var distance = Math.sqrt((dx * dx) + (dy * dy));

                    // Clicks inside the donut hole correspond to the center total label area.
                    if (distance <= innerRadius) {
                        showPendingSummaryIssues('Total Pending', pendingSummaryTotalIdsCsv);
                    }
                };

                window.CMF_PORTAL.homeOverallPendingPieChart.getZr().on('click', centerClickHandler);
                window.CMF_PORTAL.homeOverallPendingPieCenterClickHandler = centerClickHandler;
            }

            window.CMF_PORTAL.homeOverallPendingPieChart.resize();
        }

        function setHoverWidthLockState(host, enabled) {
            if (!host) return;
            if (enabled) {
                host.classList.add('ccip-hover-width-lock');
                var comp = Math.max(0, host.offsetWidth - host.clientWidth);
                host.style.setProperty('--ccip-hover-comp', comp + 'px');
                if (comp === 0) {
                    window.requestAnimationFrame(function () {
                        if (!host.classList.contains('ccip-hover-width-lock')) {
                            return;
                        }
                        var liveComp = Math.max(0, host.offsetWidth - host.clientWidth);
                        host.style.setProperty('--ccip-hover-comp', liveComp + 'px');
                    });
                }
                return;
            }
            host.classList.remove('ccip-hover-width-lock');
            host.style.removeProperty('--ccip-hover-comp');
        }

        function ensureHoverWidthLock(host) {
            if (!host || host.dataset.hoverWidthLockInit === '1') {
                return;
            }
            host.dataset.hoverWidthLockInit = '1';
            host.addEventListener('mouseenter', function () {
                setHoverWidthLockState(host, true);
            });
            host.addEventListener('mouseleave', function () {
                setHoverWidthLockState(host, false);
            });
            host.addEventListener('focusin', function () {
                setHoverWidthLockState(host, true);
            });
            host.addEventListener('focusout', function () {
                if (!host.contains(document.activeElement)) {
                    setHoverWidthLockState(host, false);
                }
            });
        }

        function parseComponentOpenUniqueMetrics(value) {
            var text = String(value == null ? '' : value).trim();
            var fullMatch = text.match(/^(\d+)\s*\(\s*(\d+)\s*\)/);
            if (fullMatch) {
                return {
                    openUnique: parseInt(fullMatch[1], 10) || 0,
                    openUniqueLos: parseInt(fullMatch[2], 10) || 0
                };
            }

            var looseMatch = text.match(/^(\d+)/);
            var openOnly = looseMatch ? parseInt(looseMatch[1], 10) : 0;
            return {
                openUnique: isNaN(openOnly) ? 0 : openOnly,
                openUniqueLos: 0
            };
        }

        function getOverallHeatClassFromOpenUnique(value) {
            return getOverallHeatClassFromOpenUniqueByThreshold(value, null);
        }

        function getDefaultComponentRiskThresholds() {
            return {
                low: 0,
                med: 2,
                high: 4
            };
        }

        function normalizeComponentRiskThresholds(config) {
            var defaults = getDefaultComponentRiskThresholds();
            var source = config || {};

            var med = parseInt(source.med, 10);
            var high = parseInt(source.high, 10);

            // Backward compatibility for previously saved range-style settings.
            if (isNaN(med)) {
                var legacyMedUpper = parseInt(source.medUpper, 10);
                var legacyMedLower = parseInt(source.medLower, 10);
                med = !isNaN(legacyMedUpper) ? legacyMedUpper : legacyMedLower;
            }
            if (isNaN(high)) {
                var legacyHighUpper = parseInt(source.highUpper, 10);
                var legacyHighLower = parseInt(source.highLower, 10);
                high = !isNaN(legacyHighUpper) ? legacyHighUpper : legacyHighLower;
            }

            var low = defaults.low;
            med = isNaN(med) ? defaults.med : med;
            high = isNaN(high) ? defaults.high : high;

            low = 0;
            med = Math.max(low + 2, med);
            high = Math.max(med + 1, high);

            return {
                low: low,
                med: med,
                high: high
            };
        }

        function getOverallHeatClassFromOpenUniqueByThreshold(value, thresholdConfig) {
            var normalized = parseInt(value, 10);
            if (isNaN(normalized)) {
                return 'overall-heat-low';
            }

            var thresholds = normalizeComponentRiskThresholds(thresholdConfig);
            if (normalized <= thresholds.low) {
                return 'overall-heat-low';
            }

            // Medium range: 1 .. (med - 1)
            if (normalized >= 1 && normalized <= (thresholds.med - 1)) {
                return 'overall-heat-med';
            }

            // High range: med .. (high - 1)
            if (normalized >= thresholds.med && normalized <= (thresholds.high - 1)) {
                return 'overall-heat-high';
            }

            // Critical range: >= high
            return 'overall-heat-critical';
        }

        function getComponentHeatmapPlatformColumnWidth(platformLabels) {
            var labels = Array.isArray(platformLabels) ? platformLabels : [];
            var longestWidth = 0;

            var measureCanvas = document.createElement('canvas');
            var measureContext = measureCanvas.getContext('2d');
            if (measureContext) {
                measureContext.font = '900 9px "Plus Jakarta Sans", "Segoe UI", sans-serif';
                for (var i = 0; i < labels.length; i++) {
                    var label = String(labels[i] == null ? '' : labels[i]).trim();
                    if (!label) {
                        continue;
                    }
                    var textWidth = measureContext.measureText(label).width;
                    var letterSpacing = Math.max(0, label.length - 1) * 0.18;
                    longestWidth = Math.max(longestWidth, textWidth + letterSpacing);
                }
            } else {
                for (var j = 0; j < labels.length; j++) {
                    var fallbackLabel = String(labels[j] == null ? '' : labels[j]).trim();
                    longestWidth = Math.max(longestWidth, fallbackLabel.length * 6.2);
                }
            }

            var padded = Math.ceil(longestWidth + 16);
            return Math.max(74, Math.min(180, padded));
        }

        function renderComponentSummaryHeatmap(host, columns, rows, hasMilestoneColumns) {
            host.classList.add('ccip-component-riskmap-wrap');
            var componentRiskThresholds = normalizeComponentRiskThresholds(
                window.CMF_PORTAL && window.CMF_PORTAL.componentRiskThresholds
                    ? window.CMF_PORTAL.componentRiskThresholds
                    : null
            );
            var applyLosSubtraction = !!(window.CMF_PORTAL && window.CMF_PORTAL.componentRiskSubtractLos === true);
            if (window.CMF_PORTAL) {
                window.CMF_PORTAL.componentRiskThresholds = componentRiskThresholds;
                window.CMF_PORTAL.componentRiskSubtractLos = applyLosSubtraction;
            }

            var platformLabels = [];
            var openColumnIndexes = [];

            if (hasMilestoneColumns) {
                for (var m = 1; m < columns.length; m += 2) {
                    var openMeta = String(columns[m] || '').split(':::');
                    var platformLabel = String((openMeta.length > 1 ? openMeta[0] : openMeta[0]) || '').trim();
                    if (!platformLabel) {
                        platformLabel = 'Milestone ' + String(platformLabels.length + 1);
                    }
                    platformLabels.push(platformLabel);
                    openColumnIndexes.push(m);
                }
            } else if (columns.length >= 2) {
                platformLabels.push('Open Unique (LOS)');
                openColumnIndexes.push(1);
            }

            if (!platformLabels.length || !openColumnIndexes.length) {
                host.innerHTML = '<div class="ccip-empty-state">No component heatmap data available.</div>';
                return;
            }

            var hasRows = false;
            for (var r = 0; r < rows.length; r++) {
                var row = rows[r] || [];
                var rowLabel = row.length > 0 ? String(row[0] || '').trim() : '';
                if (rowLabel) {
                    hasRows = true;
                    break;
                }
            }

            if (!hasRows) {
                host.innerHTML = '<div class="ccip-empty-state">No matching CMF data for this platform.</div>';
                return;
            }

            var platformColumnWidthPx = getComponentHeatmapPlatformColumnWidth(platformLabels);
            var html = '<div class="component-summary-heatmap-layout"><div class="component-summary-heatmap-scroll"><div class="overall-heatmap-board component-summary-heatmap-board" style="grid-template-columns: 112px repeat(' + platformLabels.length + ', ' + platformColumnWidthPx + 'px);">';
            html += '<div class="overall-heatmap-corner">COMPONENT</div>';
            for (var c = 0; c < platformLabels.length; c++) {
                html += '<div class="overall-heatmap-platform">' + escapeHtml(platformLabels[c]) + '</div>';
            }

            for (var rowIndex = 0; rowIndex < rows.length; rowIndex++) {
                var currentRow = rows[rowIndex] || [];
                var componentName = currentRow.length > 0 ? String(currentRow[0] || '').trim() : '';
                if (!componentName) {
                    continue;
                }

                if (/^total$/i.test(componentName)) {
                    continue;
                }

                html += '<div class="overall-heatmap-ingredient">' + escapeHtml(componentName) + '</div>';

                for (var platformIndex = 0; platformIndex < openColumnIndexes.length; platformIndex++) {
                    var openCellIndex = openColumnIndexes[platformIndex];
                    var openCellValue = currentRow.length > openCellIndex ? currentRow[openCellIndex] : '';
                    var metrics = parseComponentOpenUniqueMetrics(openCellValue);
                    var openUnique = isNaN(metrics.openUnique) ? 0 : metrics.openUnique;
                    var losCount = isNaN(metrics.openUniqueLos) ? 0 : metrics.openUniqueLos;
                    var colorBasisValue = applyLosSubtraction ? (openUnique - losCount) : openUnique;
                    if (isNaN(colorBasisValue) || colorBasisValue < 0) {
                        colorBasisValue = 0;
                    }
                    var cssClass = getOverallHeatClassFromOpenUniqueByThreshold(colorBasisValue, componentRiskThresholds);
                    var displayValue = String(openUnique);
                    if (applyLosSubtraction && losCount > 0) {
                        displayValue += ' (' + String(losCount) + ')';
                    }
                    var tooltip = 'Open Unique: ' + String(openUnique) + '\nLOS: ' + String(losCount);
                    html += '<div class="overall-heatmap-tile component-summary-heatmap-tile' + (cssClass ? ' ' + cssClass : '') + '" title="' + escapeHtml(tooltip) + '" aria-label="' + escapeHtml(tooltip) + '">' + escapeHtml(displayValue) + '</div>';
                }
            }

            html += '</div></div>';
            var criticalStart = componentRiskThresholds.high;
            html += '<div class="component-summary-legend">'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch low"></span>'
                    + '<span class="component-summary-legend-label">Low :</span>'
                    + '<span class="component-summary-legend-derived">' + String(componentRiskThresholds.low) + '</span>'
                + '</div>'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch med"></span>'
                    + '<span class="component-summary-legend-label">Medium &lt;</span>'
                    + '<input type="number" min="2" step="1" class="component-summary-legend-value component-summary-legend-input" data-threshold="med" value="' + String(componentRiskThresholds.med) + '">'
                + '</div>'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch high"></span>'
                    + '<span class="component-summary-legend-label">High &lt;</span>'
                    + '<input type="number" min="' + String(componentRiskThresholds.med + 1) + '" step="1" class="component-summary-legend-value component-summary-legend-input" data-threshold="high" value="' + String(componentRiskThresholds.high) + '">'
                + '</div>'
                + '<div class="component-summary-legend-row">'
                    + '<span class="component-summary-legend-swatch critical"></span>'
                    + '<span class="component-summary-legend-label">Critical :</span>'
                    + '<span class="component-summary-legend-derived">' + String(criticalStart) + '+</span>'
                + '</div>'
                + '<div class="component-summary-legend-row component-summary-legend-row-toggle">'
                    + '<label class="component-summary-legend-toggle">'
                        + '<span>Subtract LOS</span>'
                        + '<input type="checkbox" class="component-summary-legend-toggle-input" data-threshold="component-los-subtract-toggle"' + (applyLosSubtraction ? ' checked' : '') + '>'
                    + '</label>'
                + '</div>'
                + '</div>'
                + '<div class="component-summary-warning-popup" role="alert" aria-live="assertive"></div>'
                + '</div>';
            host.innerHTML = html;

            var medInput = host.querySelector('[data-threshold="med"]');
            var highInput = host.querySelector('[data-threshold="high"]');
            var losToggleInput = host.querySelector('[data-threshold="component-los-subtract-toggle"]');
            var warningPopup = host.querySelector('.component-summary-warning-popup');
            var warningPopupTimer = null;
            var lastValidThresholds = {
                med: componentRiskThresholds.med,
                high: componentRiskThresholds.high
            };

            if (losToggleInput) {
                losToggleInput.addEventListener('change', function () {
                    if (window.CMF_PORTAL) {
                        window.CMF_PORTAL.componentRiskSubtractLos = !!losToggleInput.checked;
                    }
                    window.setTimeout(function () {
                        try {
                            if (!host || !host.isConnected) {
                                return;
                            }
                            renderComponentSummaryHeatmap(host, columns, rows, hasMilestoneColumns);
                        } catch (toggleRenderError) {
                            console.error('Component riskmap LOS toggle rerender failed:', toggleRenderError);
                        }
                    }, 0);
                });
            }

            function showLegendError(message) {
                if (warningPopup) {
                    if (warningPopupTimer) {
                        window.clearTimeout(warningPopupTimer);
                        warningPopupTimer = null;
                    }

                    var text = String(message || '').trim();
                    if (!text) {
                        warningPopup.textContent = '';
                        warningPopup.classList.remove('component-summary-warning-popup-visible');
                        return;
                    }

                    warningPopup.textContent = text;
                    warningPopup.classList.add('component-summary-warning-popup-visible');
                    warningPopupTimer = window.setTimeout(function () {
                        if (!warningPopup) {
                            return;
                        }
                        warningPopup.textContent = '';
                        warningPopup.classList.remove('component-summary-warning-popup-visible');
                        warningPopupTimer = null;
                    }, 2400);
                }
            }

            function setInputInvalidState(input, isInvalid) {
                if (!input) {
                    return;
                }
                if (isInvalid) {
                    input.classList.add('component-summary-legend-input-invalid');
                    return;
                }
                input.classList.remove('component-summary-legend-input-invalid');
            }

            function syncLegendInputMins() {
                if (medInput) {
                    medInput.min = '2';
                }
                if (highInput) {
                    var medValue = parseInt(medInput ? medInput.value : lastValidThresholds.med, 10);
                    var highMin = isNaN(medValue) ? (lastValidThresholds.med + 1) : (medValue + 1);
                    highInput.min = String(Math.max(3, highMin));
                }
            }

            function resetLegendInputsToLastValid() {
                if (medInput) {
                    medInput.value = String(lastValidThresholds.med);
                }
                if (highInput) {
                    highInput.value = String(lastValidThresholds.high);
                }
                syncLegendInputMins();
            }

            function validateLegendThresholdInputs() {
                var medValue = parseInt(medInput ? medInput.value : '', 10);
                var highValue = parseInt(highInput ? highInput.value : '', 10);

                if (isNaN(medValue) || medValue < 2) {
                    return {
                        valid: false,
                        message: 'Medium must be 2 or greater.',
                        invalidField: 'med'
                    };
                }

                if (isNaN(highValue) || highValue <= medValue) {
                    return {
                        valid: false,
                        message: 'High must be greater than Medium.',
                        invalidField: 'high'
                    };
                }

                return {
                    valid: true,
                    med: medValue,
                    high: highValue
                };
            }

            function rerenderComponentRiskmapFromLegendInputs() {
                setInputInvalidState(medInput, false);
                setInputInvalidState(highInput, false);

                var validation = validateLegendThresholdInputs();
                if (!validation.valid) {
                    showLegendError(validation.message);
                    if (validation.invalidField === 'med') {
                        setInputInvalidState(medInput, true);
                    } else if (validation.invalidField === 'high') {
                        setInputInvalidState(highInput, true);
                    }
                    resetLegendInputsToLastValid();
                    return;
                }

                showLegendError('');

                var nextThresholds = normalizeComponentRiskThresholds({
                    med: validation.med,
                    high: validation.high
                });

                lastValidThresholds.med = nextThresholds.med;
                lastValidThresholds.high = nextThresholds.high;
                syncLegendInputMins();

                if (window.CMF_PORTAL) {
                    window.CMF_PORTAL.componentRiskThresholds = nextThresholds;
                }
                window.setTimeout(function () {
                    try {
                        if (!host || !host.isConnected) {
                            return;
                        }
                        renderComponentSummaryHeatmap(host, columns, rows, hasMilestoneColumns);
                    } catch (rerenderError) {
                        console.error('Component riskmap rerender failed:', rerenderError);
                        resetLegendInputsToLastValid();
                        showLegendError('Invalid threshold input.');
                    }
                }, 0);
            }

            var thresholdInputs = [medInput, highInput];
            for (var inputIndex = 0; inputIndex < thresholdInputs.length; inputIndex++) {
                var thresholdInput = thresholdInputs[inputIndex];
                if (!thresholdInput) {
                    continue;
                }
                thresholdInput.addEventListener('input', function () {
                    showLegendError('');
                    setInputInvalidState(medInput, false);
                    setInputInvalidState(highInput, false);
                    syncLegendInputMins();
                });
                thresholdInput.addEventListener('keydown', function (event) {
                    if (event && event.key === 'Enter') {
                        event.preventDefault();
                        event.stopPropagation();
                        if (event.target && typeof event.target.dispatchEvent === 'function') {
                            event.target.dispatchEvent(new Event('change', { bubbles: true }));
                        }
                        if (event.target && typeof event.target.blur === 'function') {
                            event.target.blur();
                        }
                    }
                });
                thresholdInput.addEventListener('change', rerenderComponentRiskmapFromLegendInputs);
            }

            syncLegendInputMins();
        }

        function renderMilestoneSummaryPie(host, tableData) {
            if (!host) {
                return;
            }

            host.classList.add('ccip-milestone-pie-wrap');

            function parseMetricInt(value) {
                var cleaned = String(value == null ? '' : value).replace(/[^\d\-]/g, '');
                var parsed = parseInt(cleaned, 10);
                return isNaN(parsed) ? 0 : parsed;
            }

            function buildMilestoneBreakCountLinkHtml(milestoneName, countValue) {
                var name = String(milestoneName == null ? '' : milestoneName).trim();
                var count = String(countValue == null ? '' : countValue);
                if (!name) {
                    return escapeHtml(count);
                }

                var encodedName = encodeURIComponent(name);
                return '<a href="javascript:void(0);" class="overall-break-count-link" onclick="return showDriverIssues(decodeURIComponent(\'' + encodedName + '\'));">' + escapeHtml(count) + '</a>';
            }

            var rows = tableData && tableData.Rows ? tableData.Rows : [];
            var breakdown = [];
            for (var i = 0; i < rows.length; i++) {
                var source = rows[i] || [];
                var label = source.length > 0 ? String(source[0] || '').trim() : '';
                if (!label || /^total$/i.test(label)) {
                    continue;
                }

                var value = source.length > 1 ? parseMetricInt(source[1]) : 0;
                breakdown.push({ name: label, value: value });
            }

            breakdown.sort(function (left, right) {
                return right.value - left.value;
            });

            var total = 0;
            for (var t = 0; t < breakdown.length; t++) {
                total += breakdown[t].value;
            }

            if (window.CMF_PORTAL.homeMilestoneSummaryPieChart) {
                window.CMF_PORTAL.homeMilestoneSummaryPieChart.dispose();
                window.CMF_PORTAL.homeMilestoneSummaryPieChart = null;
            }

            if (!breakdown.length || total <= 0) {
                host.innerHTML = '<div class="ccip-empty-state">No milestone chart data.</div>';
                return;
            }

            var pieColors = ['#2563eb', '#f59e0b', '#22c55e', '#ef4444', '#8b5cf6', '#0ea5e9', '#14b8a6', '#f97316'];
            var breakdownHtml = '';
            for (var b = 0; b < breakdown.length; b++) {
                var item = breakdown[b];
                var pct = total > 0 ? (item.value * 100 / total) : 0;
                var dot = pieColors[b % pieColors.length];
                breakdownHtml += '<div class="overall-break-row">' +
                    '<span class="overall-break-left">' +
                        '<span class="overall-break-dot" style="background:' + dot + ';"></span>' +
                        '<span class="overall-break-name">' + escapeHtml(item.name) + '</span>' +
                    '</span>' +
                    '<span class="overall-break-metrics">' +
                        '<strong class="overall-break-count">' + buildMilestoneBreakCountLinkHtml(item.name, item.value) + '</strong>' +
                        '<span class="overall-break-pct">(' + escapeHtml(pct.toFixed(1)) + '%)</span>' +
                    '</span>' +
                    '</div>';
            }

            host.innerHTML =
                '<div class="milestone-pie-layout">' +
                    '<div class="milestone-stacked-total">Total CMFs: ' + escapeHtml(String(total)) + '</div>' +
                    '<div class="milestone-pie-canvas"></div>' +
                    '<div class="milestone-pie-breakdown">' + breakdownHtml + '</div>' +
                '</div>';

            var pieHost = host.querySelector('.milestone-pie-canvas');
            if (!pieHost) {
                return;
            }

            if (typeof echarts === 'undefined') {
                pieHost.innerHTML = '<div class="ccip-empty-state">Chart unavailable.</div>';
                return;
            }

            window.CMF_PORTAL.homeMilestoneSummaryPieChart = echarts.init(pieHost);
            var stackedSeries = breakdown.map(function (item, index) {
                var pctValue = total > 0 ? (item.value * 100 / total) : 0;
                return {
                    name: item.name,
                    type: 'bar',
                    stack: 'milestone-total',
                    cursor: 'pointer',
                    data: [pctValue],
                    barWidth: 24,
                    itemStyle: {
                        color: pieColors[index % pieColors.length],
                        borderColor: '#ffffff',
                        borderWidth: 1
                    },
                    emphasis: { focus: 'series' }
                };
            });

            window.CMF_PORTAL.homeMilestoneSummaryPieChart.setOption({
                animation: false,
                tooltip: {
                    trigger: 'item',
                    confine: false,
                    appendToBody: true,
                    extraCssText: 'z-index: 9999;',
                    formatter: function (params) {
                        var item = breakdown[params.seriesIndex] || {};
                        var value = item.value || 0;
                        var pct = total > 0 ? (value * 100 / total) : 0;
                        return escapeHtml(String(item.name || '-')) + ': ' + escapeHtml(String(value)) + ' (' + escapeHtml(pct.toFixed(1)) + '%)';
                    }
                },
                color: pieColors,
                grid: {
                    left: 0,
                    right: 0,
                    top: 8,
                    bottom: 8,
                    containLabel: false
                },
                xAxis: {
                    type: 'value',
                    min: 0,
                    max: 100,
                    show: false
                },
                yAxis: {
                    type: 'category',
                    data: ['Milestones'],
                    show: false
                },
                series: stackedSeries
            }, true);

            window.CMF_PORTAL.homeMilestoneSummaryPieChart.off('click');
            window.CMF_PORTAL.homeMilestoneSummaryPieChart.on('click', function (params) {
                var milestoneName = params && params.seriesName
                    ? String(params.seriesName || '').trim()
                    : '';
                if (!milestoneName || typeof showDriverIssues !== 'function') {
                    return;
                }

                showDriverIssues(milestoneName);
            });

            window.CMF_PORTAL.homeMilestoneSummaryPieChart.resize();
        }

        function applyComponentGroupHeaderSticky(host) {
            if (!host) {
                return;
            }

            var table = host.querySelector('table.ccip-component-summary-table');
            if (!table) {
                return;
            }

            var headerRows = table.querySelectorAll('thead tr');
            if (!headerRows || headerRows.length < 2) {
                return;
            }

            var topCells = headerRows[0].querySelectorAll('th');
            if (!topCells || topCells.length === 0) {
                return;
            }

            var firstCell = topCells[0];
            var firstWidth = firstCell.getBoundingClientRect().width;
            if (!firstWidth || !isFinite(firstWidth) || firstWidth <= 0) {
                firstWidth = 92;
            }

            firstCell.style.left = '0px';
            firstCell.style.zIndex = '8';

            for (var i = 1; i < topCells.length; i++) {
                var groupCell = topCells[i];
                if (!groupCell.classList.contains('ccip-component-group-header')) {
                    continue;
                }

                // Keep vertical sticky behavior from base table header CSS.
                // Do not pin group headers horizontally; they must slide with their two sub-columns.
                groupCell.style.position = 'sticky';
                groupCell.style.left = '';
                groupCell.style.zIndex = '3';
                groupCell.style.background = '#f2f4f7';
            }
        }

        function getDesignSummaryColumnIndexes(table) {
            var result = { swIndex: -1, supportIndex: -1, editIndex: -1 };
            if (!table || !table.tHead || !table.tHead.rows || !table.tHead.rows.length) {
                return result;
            }

            var headerCells = table.tHead.rows[0].cells || [];
            for (var i = 0; i < headerCells.length; i++) {
                var rawHeader = String((headerCells[i].textContent || headerCells[i].innerText || '')).trim().toLowerCase();
                if (rawHeader === 'sw image freeze') {
                    result.swIndex = i;
                } else if (rawHeader === 'support model') {
                    result.supportIndex = i;
                } else if (rawHeader === 'edit') {
                    result.editIndex = i;
                }
            }

            return result;
        }

        function buildDesignSummaryEditLinkHtml(designName) {
            var encodedDesignName = encodeURIComponent(String(designName == null ? '' : designName));
            return '<a href="javascript:void(0);" class="ccip-design-edit-link" onclick="return openDesignSummaryEdit(this, decodeURIComponent(\'' + encodedDesignName + '\'));">Edit</a>';
        }

        function restoreDesignSummaryInlineRow(row) {
            if (!row) {
                return;
            }

            var table = row.closest('table');
            var indexes = getDesignSummaryColumnIndexes(table);
            if (indexes.swIndex < 0 || indexes.supportIndex < 0 || indexes.editIndex < 0) {
                return;
            }

            var swCell = row.cells[indexes.swIndex];
            var supportCell = row.cells[indexes.supportIndex];
            var editCell = row.cells[indexes.editIndex];
            if (!swCell || !supportCell || !editCell) {
                return;
            }

            var originalSw = row.getAttribute('data-inline-sw-original') || '';
            var originalSupport = row.getAttribute('data-inline-support-original') || '';
            var designName = row.getAttribute('data-design-name') || '';

            swCell.textContent = originalSw;
            supportCell.textContent = originalSupport;
            editCell.innerHTML = buildDesignSummaryEditLinkHtml(designName);

            row.classList.remove('ccip-design-row-editing');
            row.removeAttribute('data-inline-sw-original');
            row.removeAttribute('data-inline-support-original');

            if (table && table.__cmfInlineEditingRow === row) {
                table.__cmfInlineEditingRow = null;
            }
        }

        function cancelDesignSummaryInlineEdit(triggerElement) {
            var row = triggerElement && triggerElement.closest ? triggerElement.closest('tr') : null;
            restoreDesignSummaryInlineRow(row);
            return false;
        }

        function saveDesignSummaryInlineEdit(triggerElement, designName) {
            var row = triggerElement && triggerElement.closest ? triggerElement.closest('tr') : null;
            if (!row) {
                return false;
            }

            var table = row.closest('table');
            var indexes = getDesignSummaryColumnIndexes(table);
            if (indexes.swIndex < 0 || indexes.supportIndex < 0) {
                return false;
            }

            var design = designName == null ? '' : String(designName);
            if (!design) {
                design = row.getAttribute('data-design-name') || '';
            }
            design = String(design || '').trim();
            if (!design || /^total$/i.test(design)) {
                return false;
            }

            var swCell = row.cells[indexes.swIndex];
            var supportCell = row.cells[indexes.supportIndex];
            var swInput = swCell ? swCell.querySelector('.ccip-design-inline-sw') : null;
            var supportInput = supportCell ? supportCell.querySelector('.ccip-design-inline-support') : null;

            var hiddenDesign = document.getElementById(window.CMF_PORTAL.ids.hiddenInlineDesignName);
            var hiddenSw = document.getElementById(window.CMF_PORTAL.ids.hiddenInlineSwImageFreeze);
            var hiddenSupport = document.getElementById(window.CMF_PORTAL.ids.hiddenInlineSupportModel);

            if (!hiddenDesign || !hiddenSw || !hiddenSupport || typeof __doPostBack !== 'function') {
                return false;
            }

            hiddenDesign.value = design;
            hiddenSw.value = swInput ? swInput.value : '';
            hiddenSupport.value = supportInput ? supportInput.value : '';
            __doPostBack('SaveDesignSummaryInline', '');
            return false;
        }

        function openDesignSummaryEdit(triggerOrDesignName, designName) {
            var triggerElement = triggerOrDesignName && triggerOrDesignName.nodeType === 1 ? triggerOrDesignName : null;
            var design = designName;

            if (!triggerElement && typeof triggerOrDesignName === 'string' && typeof designName === 'undefined') {
                design = triggerOrDesignName;
            }

            design = design == null ? '' : String(design);
            if (!design || /^total$/i.test(design)) {
                return false;
            }

            var row = triggerElement && triggerElement.closest ? triggerElement.closest('tr') : null;
            if (!row) {
                var host = document.getElementById('homeDesignSummaryTable');
                var table = host ? host.querySelector('table') : null;
                var bodyRows = table && table.tBodies && table.tBodies.length ? table.tBodies[0].rows : [];
                for (var i = 0; i < bodyRows.length; i++) {
                    var firstCellText = bodyRows[i].cells && bodyRows[i].cells.length
                        ? String(bodyRows[i].cells[0].textContent || bodyRows[i].cells[0].innerText || '').trim()
                        : '';
                    if (firstCellText.toLowerCase() === design.trim().toLowerCase()) {
                        row = bodyRows[i];
                        break;
                    }
                }
            }

            if (!row) {
                return false;
            }

            var tableRef = row.closest('table');
            if (!tableRef) {
                return false;
            }

            if (tableRef.__cmfInlineEditingRow && tableRef.__cmfInlineEditingRow !== row) {
                restoreDesignSummaryInlineRow(tableRef.__cmfInlineEditingRow);
            }

            if (row.classList.contains('ccip-design-row-editing')) {
                return false;
            }

            var indexes = getDesignSummaryColumnIndexes(tableRef);
            if (indexes.swIndex < 0 || indexes.supportIndex < 0 || indexes.editIndex < 0) {
                return false;
            }

            var swCell = row.cells[indexes.swIndex];
            var supportCell = row.cells[indexes.supportIndex];
            var editCell = row.cells[indexes.editIndex];
            if (!swCell || !supportCell || !editCell) {
                return false;
            }

            var swValue = String(swCell.textContent || '').trim();
            var supportValue = String(supportCell.textContent || '').trim();
            var encodedDesignName = encodeURIComponent(design);

            row.setAttribute('data-design-name', design);
            row.setAttribute('data-inline-sw-original', swValue);
            row.setAttribute('data-inline-support-original', supportValue);

            swCell.innerHTML = '<textarea class="ccip-design-inline-field ccip-design-inline-sw" rows="2">' + escapeHtml(swValue) + '</textarea>';
            supportCell.innerHTML = '<textarea class="ccip-design-inline-field ccip-design-inline-support" rows="2">' + escapeHtml(supportValue) + '</textarea>';
            editCell.innerHTML = '<span class="ccip-design-edit-actions">'
                + '<a href="javascript:void(0);" class="ccip-design-edit-link" onclick="return saveDesignSummaryInlineEdit(this, decodeURIComponent(\'' + encodedDesignName + '\'));">Save</a>'
                + '<a href="javascript:void(0);" class="ccip-design-edit-link" onclick="return cancelDesignSummaryInlineEdit(this);">Cancel</a>'
                + '</span>';

            row.classList.add('ccip-design-row-editing');
            tableRef.__cmfInlineEditingRow = row;

            var firstInput = swCell.querySelector('.ccip-design-inline-sw');
            if (firstInput && firstInput.focus) {
                firstInput.focus();
                if (firstInput.select) {
                    firstInput.select();
                }
            }

            return false;
        }

        function exportHomeDesignSummaryToExcel() {
            if (window.CMF_PORTAL && window.CMF_PORTAL.ids && window.CMF_PORTAL.ids.btnExportToExcelDesignSummary) {
                __doPostBack(window.CMF_PORTAL.ids.btnExportToExcelDesignSummary, '');
                return false;
            }

            var host = document.getElementById('homeDesignSummaryTable');
            var table = host ? host.querySelector('table') : null;
            if (!table) {
                return false;
            }

            var html = '<html><head><meta charset="utf-8"></head><body>' + table.outerHTML + '</body></html>';
            var blob = new Blob(['\ufeff', html], { type: 'application/vnd.ms-excel' });
            var url = URL.createObjectURL(blob);
            var link = document.createElement('a');
            link.href = url;
            link.download = 'DesignSummary.xls';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
            return false;
        }

        function renderDashboardLiveTable(hostId, tableData) {
            var host = document.getElementById(hostId);
            if (!host) return;
            var columns = tableData && tableData.Columns ? tableData.Columns : [];
            var rows = tableData && tableData.Rows ? tableData.Rows : [];
            var isMilestoneSummaryHost = hostId === 'homeMilestoneSummaryTable';

            function getComponentSummaryMilestoneFromColumn(columnLabel) {
                var text = String(columnLabel == null ? '' : columnLabel);
                if (!text) {
                    return '';
                }

                var parts = text.split(':::');
                if (parts.length > 1) {
                    return String(parts[0] || '').trim();
                }

                var milestoneSelect = document.querySelector("[id$='ddlMilestoneTop'], [id$='ddlMilestoneHeader']");
                if (milestoneSelect && milestoneSelect.options && milestoneSelect.selectedIndex >= 0) {
                    var selectedText = String(milestoneSelect.options[milestoneSelect.selectedIndex].text || '').trim();
                    if (selectedText && !/^all$/i.test(selectedText)) {
                        return selectedText;
                    }
                }

                return '';
            }

            function hasPositiveComponentMetricValue(rawValue) {
                var text = String(rawValue == null ? '' : rawValue);
                var matches = text.match(/\d+/g);
                if (!matches || !matches.length) {
                    return false;
                }

                var total = 0;
                for (var idx = 0; idx < matches.length; idx++) {
                    total += parseInt(matches[idx], 10) || 0;
                }
                return total > 0;
            }

            function buildComponentSummaryMetricLinkHtml(componentName, milestoneName, issueType, value) {
                var safeComponent = String(componentName == null ? '' : componentName).trim();
                var safeMilestone = String(milestoneName == null ? '' : milestoneName).trim();
                var safeIssueType = String(issueType == null ? '' : issueType).trim();
                var valueText = String(value == null ? '' : value);

                if (!safeComponent || !safeMilestone || !safeIssueType || !hasPositiveComponentMetricValue(valueText)) {
                    return escapeHtml(valueText);
                }

                var encodedComponent = encodeURIComponent(safeComponent);
                var encodedMilestone = encodeURIComponent(safeMilestone);
                var encodedIssueType = encodeURIComponent(safeIssueType);

                return '<a href="javascript:void(0);" class="ccip-component-summary-link" onclick="return showCMFIssues(decodeURIComponent(\'' + encodedComponent + '\'), decodeURIComponent(\'' + encodedMilestone + '\'), decodeURIComponent(\'' + encodedIssueType + '\'));">' + escapeHtml(valueText) + '</a>';
            }

            if (isMilestoneSummaryHost) {
                renderMilestoneSummaryPie(host, tableData);
                return;
            }

            if (columns.length === 0) {
                host.innerHTML = '<div class="ccip-empty-state">No dashboard data available.</div>';
                return;
            }
            var isMilestoneSummary = hostId === 'homeMilestoneSummaryTable' && columns.length >= 2;
            var isPendingSummary = hostId === 'homePendingSummaryTable' && columns.length >= 2;
            var isDesignSummary = hostId === 'homeDesignSummaryTable' && columns.length >= 2;
            var hasMilestoneColumns = hostId === 'homeComponentSummaryTable' && columns.length >= 3 && String(columns[1] || '').indexOf(':::') > -1;
            var isComponentSummary = hostId === 'homeComponentSummaryTable' && columns.length >= 3;
            var pendingValueColumnIndex = isPendingSummary ? Math.max(1, columns.length - 1) : -1;
            var pendingSummaryAggregateIdsCsv = '';

            if (isComponentSummary && window.CMF_PORTAL && window.CMF_PORTAL.componentSummaryViewMode === 'heatmap') {
                renderComponentSummaryHeatmap(host, columns, rows, hasMilestoneColumns);
                return;
            }

            if (isPendingSummary && pendingValueColumnIndex >= 0) {
                var pendingIdSeen = {};
                var pendingIdList = [];

                for (var pendingRowIndex = 0; pendingRowIndex < rows.length; pendingRowIndex++) {
                    var pendingRow = rows[pendingRowIndex] || [];
                    var pendingLabel = pendingRow.length > 0 ? String(pendingRow[0] || '') : '';
                    if (/^total$/i.test(pendingLabel)) {
                        continue;
                    }

                    var pendingRawValue = pendingRow.length > pendingValueColumnIndex
                        ? String(pendingRow[pendingValueColumnIndex] == null ? '' : pendingRow[pendingValueColumnIndex])
                        : '';

                    if (pendingRawValue.indexOf(PENDING_SUMMARY_LINKS_DELIMITER) < 0) {
                        continue;
                    }

                    var pendingParts = pendingRawValue.split(PENDING_SUMMARY_LINKS_DELIMITER);
                    var pendingCsv = pendingParts.length > 1 ? String(pendingParts[1] || '') : '';
                    var pendingIds = pendingCsv.split(',');
                    for (var pendingIdIndex = 0; pendingIdIndex < pendingIds.length; pendingIdIndex++) {
                        var pendingSafeId = String(pendingIds[pendingIdIndex] || '').replace(/[^0-9]/g, '').trim();
                        if (!pendingSafeId || pendingIdSeen[pendingSafeId]) {
                            continue;
                        }
                        pendingIdSeen[pendingSafeId] = true;
                        pendingIdList.push(pendingSafeId);
                    }
                }

                pendingSummaryAggregateIdsCsv = pendingIdList.join(',');
            }

            if (isComponentSummary) {
                host.classList.remove('ccip-component-riskmap-wrap');
            }

            var renderColumns = columns.slice();
            var tableClasses = 'ccip-live-table'
                + (isComponentSummary ? ' ccip-component-summary-table' : '')
                + (isDesignSummary ? ' ccip-design-summary-table' : '')
                + (isMilestoneSummary ? ' ccip-milestone-summary-table' : '');
            var html = '<table class="' + tableClasses + '">';

            if (isComponentSummary) {
                var firstHeader = String(columns[0] || 'Component');
                var firstColPx = 92;
                var maxDataHeadingChars = 0;

                if (hasMilestoneColumns) {
                    for (var h = 1; h < columns.length; h += 2) {
                        var openMeta = String(columns[h] || '').split(':::');
                        var implMeta = String(columns[h + 1] || '').split(':::');
                        var milestoneName = String((openMeta.length > 1 ? openMeta[0] : openMeta[0]) || '');
                        var openTitleLen = String(openMeta.length > 1 ? openMeta[1] : 'Open').length;
                        var implTitleLen = String(implMeta.length > 1 ? implMeta[1] : 'Impl/Verified').length;
                        maxDataHeadingChars = Math.max(
                            maxDataHeadingChars,
                            Math.ceil(milestoneName.length / 2),
                            openTitleLen,
                            implTitleLen,
                            'Unique(LOS)+Duplicate(LOS)'.length,
                            'Unique+Duplicate'.length
                        );
                    }
                } else {
                    maxDataHeadingChars = Math.max(
                        'Open'.length,
                        'Impl/Verified'.length,
                        'Unique(LOS)+Duplicate(LOS)'.length,
                        'Unique+Duplicate'.length
                    );
                }

                var openColCh = Math.max(14, Math.min(21.6, Math.ceil(maxDataHeadingChars * 0.78) + 0.6));
                var implColCh = openColCh;
                var componentDataWidthCh = 0;
                for (var cw = 1; cw < columns.length; cw++) {
                    componentDataWidthCh += (cw % 2 === 1) ? openColCh : implColCh;
                }

                var componentTableDataCh = componentDataWidthCh.toFixed(1);
                html = '<table class="' + tableClasses + '" style="width:calc(' + firstColPx + 'px + ' + componentTableDataCh + 'ch); min-width:calc(' + firstColPx + 'px + ' + componentTableDataCh + 'ch);">';
                html += '<colgroup>';
                html += '<col style="width:' + firstColPx + 'px; min-width:' + firstColPx + 'px; max-width:' + firstColPx + 'px;">';
                for (var cg = 1; cg < columns.length; cg++) {
                    var colWidthCh = (cg % 2 === 1) ? openColCh : implColCh;
                    html += '<col style="width:' + colWidthCh + 'ch; min-width:' + colWidthCh + 'ch;">';
                }
                html += '</colgroup>';
            } else if (isMilestoneSummary) {
                var milestoneMaxHeadingChars = 0;

                for (var mc = 0; mc < columns.length; mc++) {
                    milestoneMaxHeadingChars = Math.max(milestoneMaxHeadingChars, String(renderColumns[mc] || '').length);
                }

                html += '<colgroup>';
                if (columns.length === 2) {
                    // Keep first column fixed and let the Count column expand to fill remaining width.
                    var milestoneFirstColPx = 150;
                    var uniqueCountHeader = String(renderColumns[1] || 'Unique CMF');
                    var uniqueCountMinCh = Math.max(12.8, Math.min(15.4, Math.ceil(uniqueCountHeader.length * 0.72) + 0.2));
                    html = '<table class="' + tableClasses + '" style="width:100%; min-width:100%;">';
                    html += '<col style="width:' + milestoneFirstColPx + 'px; min-width:' + milestoneFirstColPx + 'px;">';
                    html += '<col style="width:auto; min-width:' + uniqueCountMinCh + 'ch;">';
                } else {
                    var milestoneColPercent = (100 / columns.length).toFixed(4);
                    for (var mg = 0; mg < columns.length; mg++) {
                        html += '<col style="width:' + milestoneColPercent + '%;">';
                    }
                }
                html += '</colgroup>';
            } else if (isPendingSummary && columns.length >= 2) {
                var pendingFirstColPx = 94;
                var pendingHeader = String(renderColumns[pendingValueColumnIndex] || 'CMF Pending');
                var pendingColWidthCh = Math.max(10, Math.min(16, Math.ceil(pendingHeader.length * 0.72) + 0.4));
                var pendingIdstColCh = 0;
                if (columns.length > 2) {
                    var idstHeader = String(renderColumns[1] || 'iDST Name');
                    pendingIdstColCh = Math.max(11.2, Math.min(16.5, Math.ceil(idstHeader.length * 0.72) + 0.4));
                }

                var pendingTableWidth = 'calc(' + pendingFirstColPx + 'px + ' + pendingIdstColCh + 'ch + ' + pendingColWidthCh + 'ch)';
                html = '<table class="' + tableClasses + '" style="width:' + pendingTableWidth + '; min-width:' + pendingTableWidth + '; table-layout:auto;">';
                html += '<colgroup>';
                html += '<col style="width:' + pendingFirstColPx + 'px; min-width:' + pendingFirstColPx + 'px;">';
                if (columns.length > 2) {
                    html += '<col style="width:' + pendingIdstColCh + 'ch; min-width:' + pendingIdstColCh + 'ch;">';
                }
                html += '<col style="width:' + pendingColWidthCh + 'ch; min-width:' + pendingColWidthCh + 'ch;">';
                html += '</colgroup>';
            } else if (isDesignSummary) {
                var designOtherColCount = Math.max(1, columns.length - 1);
                var designBaseWidthPx = host && host.clientWidth > 0 ? host.clientWidth : 0;
                var designEditColPx = 72;
                var designMeasureCanvas = document.createElement('canvas');
                var designMeasureCtx = designMeasureCanvas.getContext('2d');
                var designHeadingWidthsPx = [];
                var designHeadingsTotalPx = 0;
                if (designMeasureCtx) {
                    designMeasureCtx.font = '700 10px "Plus Jakarta Sans", "Segoe UI", sans-serif';
                    for (var dh = 1; dh < columns.length; dh++) {
                        var headerLabel = String(renderColumns[dh] == null ? '' : renderColumns[dh]).trim();
                        var headerLabelUpper = headerLabel.toUpperCase();
                        var baseHeaderPx = designMeasureCtx.measureText(headerLabelUpper).width;
                        // Match table-header styling: 0.02em letter-spacing at 10px => 0.2px between glyphs.
                        var letterSpacingPx = Math.max(0, headerLabelUpper.length - 1) * 0.2;
                        var headerWidthPx = Math.max(64, Math.ceil(baseHeaderPx + letterSpacingPx + 16));
                        designHeadingWidthsPx.push(headerWidthPx);
                        designHeadingsTotalPx += headerWidthPx;
                    }
                } else {
                    for (var dc = 1; dc < columns.length; dc++) {
                        var fallbackLabel = String(renderColumns[dc] == null ? '' : renderColumns[dc]).trim();
                        var fallbackWidthPx = Math.max(64, (fallbackLabel.length * 7) + 16);
                        designHeadingWidthsPx.push(fallbackWidthPx);
                        designHeadingsTotalPx += fallbackWidthPx;
                    }
                }

                if (designHeadingWidthsPx.length !== designOtherColCount) {
                    designHeadingWidthsPx = [];
                    designHeadingsTotalPx = 0;
                    for (var de = 0; de < designOtherColCount; de++) {
                        designHeadingWidthsPx.push(72);
                        designHeadingsTotalPx += 72;
                    }
                }

                var designFirstColRatio = 0.28; // 20% reduction from the previous 0.35 split.
                var designFirstColPx = designBaseWidthPx > 0
                    ? Math.max(180, Math.floor(designBaseWidthPx * designFirstColRatio))
                    : 220;
                var designTableWidthPx = designFirstColPx + designHeadingsTotalPx + designEditColPx;

                html = '<table class="' + tableClasses + '" style="width:' + designTableWidthPx + 'px; min-width:' + designTableWidthPx + 'px; table-layout:fixed;">';
                html += '<colgroup>';
                html += '<col style="width:' + designFirstColPx.toFixed(3) + 'px; min-width:' + designFirstColPx.toFixed(3) + 'px;">';
                for (var dg = 0; dg < designOtherColCount; dg++) {
                    var colPx = Math.max(64, designHeadingWidthsPx[dg]);
                    html += '<col style="width:' + colPx.toFixed(3) + 'px; min-width:' + colPx.toFixed(3) + 'px;">';
                }
                html += '<col style="width:' + designEditColPx.toFixed(3) + 'px; min-width:' + designEditColPx.toFixed(3) + 'px; max-width:' + designEditColPx.toFixed(3) + 'px;">';
                html += '</colgroup>';
            }

            html += '<thead>';

            if (hasMilestoneColumns) {
                html += '<tr>' +
                    '<th rowspan="2">' + escapeHtml(columns[0] || 'Ingredient') + '</th>';

                for (var m = 1; m < columns.length; m += 2) {
                    var openMeta = String(columns[m] || '').split(':::');
                    var milestoneName = (openMeta.length > 1 ? openMeta[0] : openMeta[0]) || ('Milestone ' + Math.floor((m + 1) / 2));
                    html += '<th class="ccip-component-group-header" colspan="2">' + escapeHtml(milestoneName) + '</th>';
                }

                html += '</tr><tr>';
                for (var m2 = 1; m2 < columns.length; m2 += 2) {
                    var openParts = String(columns[m2] || '').split(':::');
                    var implParts = String(columns[m2 + 1] || '').split(':::');
                    var openTitle = openParts.length > 1 ? openParts[1] : 'Open';
                    var implTitle = implParts.length > 1 ? implParts[1] : 'Impl/Verified';
                    html += '<th class="ccip-component-sub-header">' +
                        escapeHtml(openTitle) +
                        '<span class="ccip-header-format">Unique(LOS)+Duplicate(LOS)</span></th>';
                    html += '<th class="ccip-component-sub-header">' +
                        escapeHtml(implTitle) +
                        '<span class="ccip-header-format">Unique+Duplicate</span></th>';
                }
                html += '</tr>';
            } else if (isComponentSummary) {
                html += '<tr>' +
                    '<th>' + escapeHtml(columns[0] || 'Ingredient') + '</th>' +
                    '<th>Open<span class="ccip-header-format">Unique(LOS)+Duplicate(LOS)</span></th>' +
                    '<th>Impl/Verified<span class="ccip-header-format">Unique+Duplicate</span></th>' +
                    '</tr>';
            } else if (isDesignSummary) {
                html += '<tr>';
                for (var c = 0; c < columns.length; c++) {
                    html += '<th>' + escapeHtml(renderColumns[c]) + '</th>';
                }
                html += '<th class="ccip-design-edit-col">Edit</th>';
                html += '</tr>';
            } else {
                html += '<tr>';
                for (var c = 0; c < columns.length; c++) {
                    html += '<th>' + escapeHtml(renderColumns[c]) + '</th>';
                }
                html += '</tr>';
            }

            html += '</thead><tbody>';
            if (rows.length === 0) {
                html += '<tr><td colspan="' + columns.length + '">No matching CMF data for this platform.</td></tr>';
            } else {
                for (var r = 0; r < rows.length; r++) {
                    var row = rows[r] || [];
                    var rowLabel = row.length > 0 ? String(row[0] || '') : '';
                    var isTotalRow = /^total/i.test(rowLabel);
                    var designName = isDesignSummary && row.length > 0 ? String(row[0] || '').trim() : '';
                    html += '<tr' + (isTotalRow ? ' class="ccip-live-total"' : '') + '>';
                    for (var i = 0; i < columns.length; i++) {
                        if (isPendingSummary && i === pendingValueColumnIndex) {
                            var totalRowIds = isTotalRow ? pendingSummaryAggregateIdsCsv : '';
                            html += '<td class="ccip-pending-summary-links-col">' + buildPendingSummaryCellHtml(row[i], isTotalRow, row[0], totalRowIds) + '</td>';
                        } else if (isDesignSummary && i === 0) {
                            html += '<td>' + renderDesignSummaryNameCell(row[i]) + '</td>';
                        } else if (isDesignSummary && i >= 3) {
                            html += '<td>' + buildDesignSummaryMetricLinkHtml(designName, renderColumns[i], row[i]) + '</td>';
                        } else if (isComponentSummary && i > 0) {
                            var issueType = (i % 2 === 1) ? 'OpenCombined' : 'ImplementedCombined';
                            var milestoneName = getComponentSummaryMilestoneFromColumn(columns[i]);
                            var componentName = String(rowLabel || '').trim();
                            html += '<td>' + buildComponentSummaryMetricLinkHtml(componentName, milestoneName, issueType, row[i]) + '</td>';
                        } else {
                            html += '<td>' + escapeHtml(row[i] == null ? '' : row[i]) + '</td>';
                        }
                    }

                    if (isDesignSummary) {
                        if (isTotalRow) {
                            html += '<td class="ccip-design-edit-col"></td>';
                        } else {
                            var editDesignName = row.length > 0 ? String(row[0] || '') : '';
                            var encodedDesignName = encodeURIComponent(editDesignName);
                            html += '<td class="ccip-design-edit-col">'
                                + '<a href="javascript:void(0);" class="ccip-design-edit-link" '
                                + 'onclick="return openDesignSummaryEdit(this, decodeURIComponent(\'' + encodedDesignName + '\'));">Edit</a>'
                                + '</td>';
                        }
                    }

                    html += '</tr>';
                }
            }
            html += '</tbody></table>';
            host.innerHTML = html;

            if (isComponentSummary && hasMilestoneColumns) {
                applyComponentGroupHeaderSticky(host);
            }

            if (hostId === 'homeDesignSummaryTable') {
                ensureHoverWidthLock(host);
            }

        }

        var PENDING_SUMMARY_LINKS_DELIMITER = ':::OPEN_HSDS:::';

        function getPendingIssueModalContextFromUi() {
            var groupingLabel = 'Component';
            var sortingLabel = 'by Count';

            var state = window.CMF_PORTAL && window.CMF_PORTAL.summaryIssuesPopupState
                ? window.CMF_PORTAL.summaryIssuesPopupState
                : null;

            if (state && String(state.metricKey || '').toLowerCase() === 'pending') {
                var groupByKey = String(state.groupBy || 'ingredient').toLowerCase();
                var sortByKey = normalizeSummaryIssueSortBy(state.sortBy || 'alphabetical');

                groupingLabel = getSummaryIssueGroupByLabel(groupByKey) || 'Ingredient';
                sortingLabel = sortByKey === 'hsdcount' ? 'by Count' : 'Alphabetically';
            }

            return {
                grouping: String(groupingLabel || 'Component'),
                sorting: String(sortingLabel || 'by Count')
            };
        }

        function showPendingSummaryIssues(ingredientName, idsCsv) {
            var ingredient = String(ingredientName == null ? '' : ingredientName).trim();
            var ids = String(idsCsv == null ? '' : idsCsv)
                .split(',')
                .map(function (token) {
                    return String(token || '').replace(/[^\d]/g, '');
                })
                .filter(function (token) {
                    return token.length > 0;
                });

            if (!ids.length) {
                return false;
            }

            var modalContext = getPendingIssueModalContextFromUi();
            var payload = encodeURIComponent(ingredient)
                + '||' + encodeURIComponent(ids.join(','))
                + '||' + encodeURIComponent(modalContext.grouping)
                + '||' + encodeURIComponent(modalContext.sorting);
            __doPostBack('ShowPendingSummaryIssues', payload);
            return false;
        }

        function showPendingAgeBucketIssues(bucketLabel) {
            var bucket = String(bucketLabel == null ? '' : bucketLabel).trim();
            if (!bucket) {
                return false;
            }

            __doPostBack('ShowPendingAgeBucketIssues', encodeURIComponent(bucket));
            return false;
        }

        function buildPendingSummaryCellHtml(rawValue, isTotalRow, ingredientName, totalRowIdsCsv) {
            var value = rawValue == null ? '' : String(rawValue);
            var countText = value;
            var normalizedIds = [];

            if (isTotalRow) {
                var totalCsv = String(totalRowIdsCsv == null ? '' : totalRowIdsCsv);
                var totalIds = totalCsv.split(',');
                var totalSeen = {};
                for (var totalIndex = 0; totalIndex < totalIds.length; totalIndex++) {
                    var totalSafeId = String(totalIds[totalIndex] || '').replace(/[^0-9]/g, '').trim();
                    if (!totalSafeId || totalSeen[totalSafeId]) {
                        continue;
                    }
                    totalSeen[totalSafeId] = true;
                    normalizedIds.push(totalSafeId);
                }
            } else if (value.indexOf(PENDING_SUMMARY_LINKS_DELIMITER) >= 0) {
                var parts = value.split(PENDING_SUMMARY_LINKS_DELIMITER);
                countText = parts[0] || '0';
                var csv = parts.length > 1 ? String(parts[1] || '') : '';
                var ids = csv.split(',');
                var seen = {};

                for (var i = 0; i < ids.length; i++) {
                    var token = String(ids[i] || '').trim();
                    if (!token) continue;
                    var safeId = token.replace(/[^0-9]/g, '');
                    if (!safeId || seen[safeId]) continue;
                    seen[safeId] = true;
                    normalizedIds.push(safeId);
                }
            }

            if (!normalizedIds.length) {
                return escapeHtml(value);
            }

            var encodedIngredient = encodeURIComponent(String(ingredientName == null ? '' : ingredientName));
            var encodedIds = encodeURIComponent(normalizedIds.join(','));
            return '<a href="javascript:void(0);" class="ccip-pending-summary-count-link" onclick="return showPendingSummaryIssues(decodeURIComponent(\'' + encodedIngredient + '\'), decodeURIComponent(\'' + encodedIds + '\'));">' + escapeHtml(countText) + '</a>';
        }

        function getTextById(id) {
            var node = id ? document.getElementById(id) : null;
            if (!node) return '0';
            return (node.textContent || node.innerText || '0').trim();
        }

        function renderInteractiveIssuePendingSidebars() {
            var issueTotal = getTextById(window.CMF_PORTAL.ids.lblIssueTotal);
            var issueProgress = getTextById(window.CMF_PORTAL.ids.lblIssueInProgress);
            var issueStale = getTextById(window.CMF_PORTAL.ids.lblIssueStale);
            var pendingTotal = getTextById(window.CMF_PORTAL.ids.lblPendingSightings);
            var pendingQualify = getTextById(window.CMF_PORTAL.ids.lblQualifyForCmf);
            var pendingDup = getTextById(window.CMF_PORTAL.ids.lblLikelyDuplicate);
            var pendingMissing = getTextById(window.CMF_PORTAL.ids.lblIncompleteSysScope);

            var issueTotalNode = document.getElementById('issueSideTotal');
            var issueProgressNode = document.getElementById('issueSideProgress');
            var issueStaleNode = document.getElementById('issueSideStale');
            if (issueTotalNode) issueTotalNode.textContent = issueTotal;
            if (issueProgressNode) issueProgressNode.textContent = issueProgress;
            if (issueStaleNode) issueStaleNode.textContent = issueStale;

            var pendingTotalNode = document.getElementById('pendingSideTotal');
            var pendingQualifyNode = document.getElementById('pendingSideQualify');
            var pendingDupNode = document.getElementById('pendingSideDup');
            var pendingMissNode = document.getElementById('pendingSideMissing');
            if (pendingTotalNode) pendingTotalNode.textContent = pendingTotal;
            if (pendingQualifyNode) pendingQualifyNode.textContent = pendingQualify;
            if (pendingDupNode) pendingDupNode.textContent = pendingDup;
            if (pendingMissNode) pendingMissNode.textContent = pendingMissing;
        }

        function isElementVisible(element) {
            if (!element) return false;
            var style = window.getComputedStyle ? window.getComputedStyle(element) : null;
            if (style && (style.display === 'none' || style.visibility === 'hidden')) {
                return false;
            }
            return element.offsetParent !== null || (style && style.position === 'fixed');
        }

        function normalizeIssuePendingShellDom() {
            var issueShell = document.getElementById('issueTabShell');
            var pendingShell = document.getElementById('pendingTabShell');
            if (!issueShell) return;

            var issueMain = issueShell.querySelector('.issue-tab-main');
            var issueSide = issueShell.querySelector('.issue-side-panel')
                || document.querySelector('#mainDataWrapper .issue-side-panel')
                || document.querySelector('.issue-side-panel');
            var pendingMain = pendingShell ? pendingShell.querySelector('.pending-tab-main') : null;
            var pendingSide = pendingShell
                ? (pendingShell.querySelector('.pending-side-panel')
                    || document.querySelector('#mainDataWrapper .pending-side-panel')
                    || document.querySelector('.pending-side-panel'))
                : null;

            if (issueMain && issueSide && issueSide.parentElement !== issueShell) {
                issueShell.insertBefore(issueSide, issueMain.nextSibling);
            }

            if (pendingShell && pendingMain && pendingSide && pendingSide.parentElement !== pendingShell) {
                pendingShell.insertBefore(pendingSide, pendingMain.nextSibling);
            }

            if (pendingShell && issueShell.parentElement && pendingShell.parentElement !== issueShell.parentElement) {
                issueShell.parentElement.insertBefore(pendingShell, issueShell.nextSibling);
            }
        }

        function syncIssuePendingSidePanelVisibility() {
            normalizeIssuePendingShellDom();
            var issueShell = document.getElementById('issueTabShell');
            var pendingShell = document.getElementById('pendingTabShell');
            var issueSide = issueShell ? issueShell.querySelector('.issue-side-panel') : null;
            var pendingSide = pendingShell ? pendingShell.querySelector('.pending-side-panel') : null;

            var issuePane = document.getElementById(window.CMF_PORTAL.ids.paneIssueList);
            var pendingPane = document.getElementById(window.CMF_PORTAL.ids.paneCmfPending);
            var activeViewTitle = document.getElementById(window.CMF_PORTAL.ids.activeViewTitle);
            var issueHeader = document.getElementById(window.CMF_PORTAL.ids.issueListHeaderPanel);
            var pendingHeader = document.getElementById(window.CMF_PORTAL.ids.cmfPendingHeaderPanel);
            var issueGrid = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
            var pendingGrid = document.getElementById(window.CMF_PORTAL.ids.gridViewCmfPending);
            var issueMain = issueShell ? issueShell.querySelector('.issue-tab-main') : null;
            var pendingMain = pendingShell ? pendingShell.querySelector('.pending-tab-main') : null;
            var issueGridWrap = issueGrid ? issueGrid.closest('.issue-grid-scroll') : null;
            var pendingGridWrap = pendingGrid ? pendingGrid.closest('.cmf-pending-grid-wrap') : null;

            var issueActive = isElementVisible(issuePane) || isElementVisible(issueHeader) || isElementVisible(issueGrid) || isElementVisible(issueMain);
            var pendingActive = isElementVisible(pendingPane) || isElementVisible(pendingHeader) || isElementVisible(pendingGrid) || isElementVisible(pendingMain);

            var activeText = activeViewTitle ? (activeViewTitle.textContent || activeViewTitle.innerText || '').toLowerCase() : '';
            var activeTab = (window.CMF_PORTAL.activeFocusedTab || '').toLowerCase();
            if (activeTab === 'pending' || activeText.indexOf('pending') >= 0) {
                pendingActive = true;
                issueActive = false;
            } else if (activeTab === 'issue' || activeText.indexOf('issue') >= 0) {
                issueActive = true;
                pendingActive = false;
            } else if (activeTab && activeTab !== 'pending' && activeTab !== 'issue') {
                issueActive = false;
                pendingActive = false;
            }

            if (!issueActive && !pendingActive && (!activeTab || activeTab === 'pending' || activeTab === 'issue')) {
                var issueWasVisible = !!(issueShell && issueShell.style.display === 'grid');
                var pendingWasVisible = !!(pendingShell && pendingShell.style.display === 'grid');
                if (pendingWasVisible) {
                    pendingActive = true;
                } else if (issueWasVisible) {
                    issueActive = true;
                } else {
                    issueActive = true;
                }
            }

            if (issueActive && pendingActive) {
                if (activeTab === 'pending' || activeText.indexOf('pending') >= 0) {
                    issueActive = false;
                } else {
                    pendingActive = false;
                }
            }

            if (issueShell) {
                issueShell.style.display = issueActive ? 'grid' : 'none';
                issueShell.classList.toggle('portal-view-hidden', !issueActive);
                issueShell.classList.toggle('side-hidden', !issueActive);
            }
            if (pendingShell) {
                pendingShell.style.display = pendingActive ? 'grid' : 'none';
                pendingShell.classList.toggle('portal-view-hidden', !pendingActive);
                pendingShell.classList.toggle('side-hidden', !pendingActive);
            }

            if (issueSide) {
                issueSide.style.display = issueActive ? 'grid' : 'none';
                issueSide.classList.toggle('portal-view-hidden', !issueActive);
            }

            if (pendingSide) {
                pendingSide.style.display = pendingActive ? 'grid' : 'none';
                pendingSide.classList.toggle('portal-view-hidden', !pendingActive);
            }

            if (issueGridWrap) {
                issueGridWrap.style.display = issueActive ? '' : 'none';
                issueGridWrap.classList.toggle('portal-view-hidden', !issueActive);
            }

            if (pendingGridWrap) {
                pendingGridWrap.style.display = pendingActive ? '' : 'none';
                pendingGridWrap.classList.toggle('portal-view-hidden', !pendingActive);
            }

            populateCmfPendingFilterOptions();
            filterPortalVisibleTables();
        }

        function isReportsViewActive() {
            var activeViewTitle = document.getElementById(window.CMF_PORTAL.ids.activeViewTitle);
            var text = activeViewTitle ? (activeViewTitle.textContent || activeViewTitle.innerText || '') : '';
            return text.toLowerCase().indexOf('report') >= 0;
        }

        document.addEventListener('DOMContentLoaded', function () {
            window.CMF_PORTAL.currentPlatform = getCurrentPlatformValue();
            window.CMF_PORTAL.currentDashboardPlatform = '<%= ResolveDashboardPlatformSelection() %>';
            window.CMF_PORTAL.availablePlatforms = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(AvailablePlatformTablesJson) %>');
            window.CMF_PORTAL.platformChipOptions = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(AvailablePlatformChipOptionsJson) %>');
            window.CMF_PORTAL.overallExcludedPlatforms = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(OverallExcludedPlatformsJson) %>');
            window.CMF_PORTAL.homeDashboardSnapshot = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(string.IsNullOrWhiteSpace(HomeDashboardSnapshotJson) ? "null" : HomeDashboardSnapshotJson) %>');
            renderPlatformChipRails();
            applyPlatformChipVisibility();
            syncActivePlatformChip();
            bindOverallTrendRangeSelector();
            bindSpecificTrendRangeSelector();
            var issueGrid = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
            if (isElementVisible(issueGrid)) {
                setTimeout(function () { syncIssueHorizontalScroll(0); }, 60);
            }
            if (isReportsViewActive()) {
                bindReportsPromptEnterHandler();
                initReportsFormatTemplate();
            }
            initCollapsibleSections();
            initMainMenuTooltips();
            initPortalNavPostbackFallback();
            renderHomeDashboard();
            renderInteractiveIssuePendingSidebars();
            syncIssuePendingSidePanelVisibility();
            syncGlobalSearchVisibility();
            bindGlobalSearchShortcut();
            applyTableOnlyScroll();
            scheduleInitColumnHideButtons(80);
        });

        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                window.CMF_PORTAL.currentPlatform = getCurrentPlatformValue();
                window.CMF_PORTAL.currentDashboardPlatform = '<%= ResolveDashboardPlatformSelection() %>';
                window.CMF_PORTAL.availablePlatforms = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(AvailablePlatformTablesJson) %>');
                window.CMF_PORTAL.platformChipOptions = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(AvailablePlatformChipOptionsJson) %>');
                window.CMF_PORTAL.overallExcludedPlatforms = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(OverallExcludedPlatformsJson) %>');
                window.CMF_PORTAL.homeDashboardSnapshot = JSON.parse('<%= HttpUtility.JavaScriptStringEncode(string.IsNullOrWhiteSpace(HomeDashboardSnapshotJson) ? "null" : HomeDashboardSnapshotJson) %>');
                renderPlatformChipRails();
                applyPlatformChipVisibility();
                syncActivePlatformChip();
                bindOverallTrendRangeSelector();
                bindSpecificTrendRangeSelector();
                var issueGrid = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
                if (isElementVisible(issueGrid)) {
                    setTimeout(function () { syncIssueHorizontalScroll(0); }, 60);
                }
                if (isReportsViewActive()) {
                    bindReportsPromptEnterHandler();
                    initReportsFormatTemplate();
                }
                initCollapsibleSections();
                initMainMenuTooltips();
                initPortalNavPostbackFallback();
                renderHomeDashboard();
                renderInteractiveIssuePendingSidebars();
                syncIssuePendingSidePanelVisibility();
                syncGlobalSearchVisibility();
                bindGlobalSearchShortcut();
                applyTableOnlyScroll();
                scheduleInitColumnHideButtons(0);
            });
        }

        // ── Column hide buttons ──────────────────────────────────────────────
        var CMF_HIDDEN_COLS_KEY = 'cmf_hidden_cols';
        var CMF_COLHIDE_TIMER = null;

        function scheduleInitColumnHideButtons(delay) {
            if (CMF_COLHIDE_TIMER) {
                clearTimeout(CMF_COLHIDE_TIMER);
            }
            CMF_COLHIDE_TIMER = setTimeout(function () {
                CMF_COLHIDE_TIMER = null;
                initColumnHideButtons();
            }, typeof delay === 'number' ? delay : 0);
        }

        function initColumnHideButtons() {
            var grid = document.getElementById('<%= overall_request_details.ClientID %>');
            if (!grid) return;
            if (!isElementVisible(grid)) return;
            // Select every header cell that carries a field-* class, regardless of thead/tbody structure
            var headers = grid.querySelectorAll('th[class*="field-"]');
            if (!headers || headers.length === 0) return;
            var stamp = headers.length + ':' + (grid.rows ? grid.rows.length : 0);
            var existingButtons = grid.querySelectorAll('.col-hide-btn').length;
            if (grid.getAttribute('data-colhide-stamp') === stamp && existingButtons >= headers.length) {
                restoreHiddenColumns();
                return;
            }
            headers.forEach(function (th) {
                if (th.querySelector('.col-hide-btn')) return; // already injected
                var fieldClass = [].slice.call(th.classList).find(function (c) { return c.indexOf('field-') === 0; });
                if (!fieldClass) return;
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'col-hide-btn';
                btn.innerHTML = '&#x2715;';
                btn.title = 'Hide column';
                btn.setAttribute('data-field', fieldClass);
                btn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    hideColumnByClass(fieldClass);
                });
                // Place beside the label text, inside filter container if present
                var target = th.querySelector('.filter-header-text') || th;
                target.appendChild(btn);
            });
            grid.setAttribute('data-colhide-stamp', stamp);
            restoreHiddenColumns();
        }

        function hideColumnByClass(fieldClass) {
            document.querySelectorAll('.' + fieldClass).forEach(function (el) {
                el.classList.add('hidden');
            });
            var hidden = JSON.parse(localStorage.getItem(CMF_HIDDEN_COLS_KEY) || '[]');
            if (hidden.indexOf(fieldClass) === -1) {
                hidden.push(fieldClass);
                localStorage.setItem(CMF_HIDDEN_COLS_KEY, JSON.stringify(hidden));
            }
            var btn = document.getElementById('btnShowAllColumns');
            if (btn) btn.style.display = '';
        }

        function restoreHiddenColumns() {
            var hidden = JSON.parse(localStorage.getItem(CMF_HIDDEN_COLS_KEY) || '[]');
            hidden.forEach(function (fieldClass) {
                document.querySelectorAll('.' + fieldClass).forEach(function (el) {
                    el.classList.add('hidden');
                });
            });
            var btn = document.getElementById('btnShowAllColumns');
            if (btn) btn.style.display = hidden.length > 0 ? '' : 'none';
        }

        function showAllColumns() {
            var hidden = JSON.parse(localStorage.getItem(CMF_HIDDEN_COLS_KEY) || '[]');
            hidden.forEach(function (fieldClass) {
                document.querySelectorAll('.' + fieldClass).forEach(function (el) {
                    el.classList.remove('hidden');
                });
            });
            localStorage.removeItem(CMF_HIDDEN_COLS_KEY);
            var btn = document.getElementById('btnShowAllColumns');
            if (btn) btn.style.display = 'none';
        }

        function filterGridRowsByQuery(grid, query) {
            if (!grid) return;
            var rows = grid.querySelectorAll('tbody tr');
            rows.forEach(function (row) {
                var text = (row.textContent || row.innerText || '').toLowerCase();
                row.style.display = (!query || text.indexOf(query) !== -1) ? '' : 'none';
            });
        }

        function filterPortalVisibleTables() {
            var input = document.getElementById('globalPortalSearch');
            var query = input ? (input.value || '').trim().toLowerCase() : '';
            var clearBtn = document.getElementById('globalSearchClear');
            var activeTab = (window.CMF_PORTAL.activeFocusedTab || '').toLowerCase();

            if (activeTab !== 'issue') {
                if (clearBtn) clearBtn.style.display = 'none';
                return;
            }

            if (clearBtn) {
                clearBtn.style.display = query ? 'inline-flex' : 'none';
            }
        }

        var issueGlobalSearchTimer = null;

        function scheduleIssueGlobalSearch() {
            var input = document.getElementById('globalPortalSearch');
            var query = input ? (input.value || '').trim() : '';
            var clearBtn = document.getElementById('globalSearchClear');
            var activeTab = (window.CMF_PORTAL.activeFocusedTab || '').toLowerCase();

            if (clearBtn) {
                clearBtn.style.display = query ? 'inline-flex' : 'none';
            }

            if (activeTab !== 'issue') {
                return;
            }

            if (issueGlobalSearchTimer) {
                clearTimeout(issueGlobalSearchTimer);
            }

            issueGlobalSearchTimer = setTimeout(function () {
                var hidden = document.getElementById('<%= hfIssueGlobalSearch.ClientID %>');
                if (hidden) {
                    hidden.value = query;
                }
                __doPostBack('<%= btnIssueGlobalSearchApply.UniqueID %>', '');
            }, 450);
        }

        function clearGlobalPortalSearch() {
            var input = document.getElementById('globalPortalSearch');
            if (!input) return;
            input.value = '';
            scheduleIssueGlobalSearch();
            input.focus();
        }

        function filterIssueTable() {
            filterPortalVisibleTables();
        }

        function clearIssueSearch() {
            clearGlobalPortalSearch();
        }

        function bindGlobalSearchShortcut() {
            if (window.CMF_PORTAL.globalSearchShortcutBound) return;
            document.addEventListener('keydown', function (event) {
                if ((event.ctrlKey || event.metaKey) && String(event.key || '').toLowerCase() === 'k') {
                    var input = document.getElementById('globalPortalSearch');
                    if (input) {
                        event.preventDefault();
                        input.focus();
                        input.select();
                    }
                }
            });
            window.CMF_PORTAL.globalSearchShortcutBound = true;
        }

        function syncGlobalSearchVisibility() {
            var shell = document.querySelector('.global-search-shell');
            var input = document.getElementById('globalPortalSearch');
            var clearBtn = document.getElementById('globalSearchClear');
            var activeTab = (window.CMF_PORTAL.activeFocusedTab || '').toLowerCase();
            var showSearch = activeTab === 'issue';
            if (shell) shell.style.display = showSearch ? 'flex' : 'none';
            if (!showSearch) {
                if (input) input.value = '';
                if (clearBtn) clearBtn.style.display = 'none';
            }
        }

        function applyTableOnlyScroll() {
            var issueWrap = document.querySelector('.issue-grid-scroll');
            var issueInner = document.getElementById('issue-grid-inner');
            var issueTable = document.getElementById(window.CMF_PORTAL.ids.overallRequestDetails);
            var pendingWrap = document.querySelector('.cmf-pending-grid-wrap');

            if (issueWrap) {
                issueWrap.style.setProperty('max-height', 'none', 'important');
                issueWrap.style.setProperty('min-height', '0', 'important');
                issueWrap.style.setProperty('overflow', 'hidden', 'important');
                issueWrap.style.setProperty('scrollbar-gutter', 'stable', 'important');
            }

            if (issueInner) {
                issueInner.style.setProperty('width', '100%', 'important');
                issueInner.style.setProperty('max-width', '100%', 'important');
                issueInner.style.setProperty('max-height', 'min(62vh, 620px)', 'important');
                issueInner.style.setProperty('min-height', '0', 'important');
                issueInner.style.setProperty('overflow-x', 'auto', 'important');
                issueInner.style.setProperty('overflow-y', 'auto', 'important');
                issueInner.style.setProperty('scrollbar-gutter', 'stable both-edges', 'important');
            }

            if (issueTable) {
                issueTable.style.setProperty('width', 'max-content', 'important');
                issueTable.style.setProperty('min-width', '2200px', 'important');
                issueTable.style.setProperty('max-width', 'none', 'important');
            }

            if (pendingWrap) {
                pendingWrap.style.setProperty('max-height', 'min(64vh, 640px)', 'important');
                pendingWrap.style.setProperty('min-height', '0', 'important');
                pendingWrap.style.setProperty('overflow-x', 'auto', 'important');
                pendingWrap.style.setProperty('overflow-y', 'auto', 'important');
                pendingWrap.style.setProperty('scrollbar-gutter', 'stable', 'important');
            }
        }

        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(function () {
                applyTableOnlyScroll();
                syncGlobalSearchVisibility();
                populateCmfPendingFilterOptions();
                syncIssueHorizontalScroll(0);
                scheduleInitColumnHideButtons(0);
            });
        }
        // ────────────────────────────────────────────────────────────────────

        function getPlatformDisplayLabel(platformValue) {
            var value = String(platformValue || '').trim();
            if (!value) return 'Current platform';

            var aliases = window.CMF_PORTAL && window.CMF_PORTAL.platformAliasMap ? window.CMF_PORTAL.platformAliasMap : {};
            for (var label in aliases) {
                if (Object.prototype.hasOwnProperty.call(aliases, label) && aliases[label] === value) {
                    return label.replace(/-/g, '_').toUpperCase();
                }
            }

            return value
                .replace(/^CMF_/i, '')
                .replace(/_ALL_COMPONENTS_TABLE$/i, '')
                .replace(/-/g, '_')
                .toUpperCase();
        }

        function renderImpactSummarySkeleton() {
            return '<div class="ai-skeleton-loader cmf-impact-skeleton" aria-label="Generating impact summary">' +
                '<div class="ai-skeleton-line" style="width: 94%;"></div>' +
                '<div class="ai-skeleton-line" style="width: 88%;"></div>' +
                '<div class="ai-skeleton-line" style="width: 72%;"></div>' +
                '<div class="ai-skeleton-line" style="width: 90%; margin-top: 8px;"></div>' +
                '<div class="ai-skeleton-line" style="width: 84%;"></div>' +
                '<div class="ai-skeleton-line" style="width: 66%;"></div>' +
                '</div>';
        }

        function renderIssueDescriptionSkeleton() {
                return '<div class="ai-skeleton-loader cmf-impact-skeleton" aria-label="Generating issue description">' +
                '<div class="ai-skeleton-line" style="width: 92%;"></div>' +
                '<div class="ai-skeleton-line" style="width: 76%; margin-top: 8px;"></div>' +
                '</div>';
            }

        function renderIssueDescriptionText(text) {
            var source = normalizeIssueDescriptionBullets(text || '');
            var lines = source.replace(/\r\n/g, '\n').replace(/\r/g, '\n')
                .split('\n')
                .map(function (line) { return line.trim(); })
                .filter(Boolean);
            if (!lines.length) return 'No description available.';

            var parsedLines = lines.map(function (line) {
                var isBullet = /^\s*(?:[-*•]|\d+[.)])\s+/.test(line);
                return {
                    isBullet: isBullet,
                    text: line.replace(/^\s*(?:[-*•]|\d+[.)])\s+/, '').trim()
                };
            }).filter(function (item) { return item.text; });

            var bullets = [];
            var markedBullets = parsedLines.filter(function (item) { return item.isBullet; });
            if (markedBullets.length >= 2) {
                bullets = markedBullets.map(function (item) { return item.text; }).slice(0, 3);
            } else if (parsedLines.length >= 4) {
                bullets = parsedLines.slice(1, 4).map(function (item) { return item.text; });
            } else {
                bullets = parsedLines.map(function (item) { return item.text; }).slice(0, 3);
            }

            if (bullets.length < 3) {
                var sentenceParts = parsedLines.map(function (item) { return item.text; }).join(' ').match(/[^.!?]+[.!?]+/g) || [];
                for (var i = 0; i < sentenceParts.length && bullets.length < 3; i++) {
                    var sentence = sentenceParts[i].trim();
                    if (sentence && bullets.indexOf(sentence) < 0) bullets.push(sentence);
                }
            }

            var html = '<ul>';
            for (var j = 0; j < bullets.length && j < 3; j++) {
                html += '<li>' + escapeHtml(bullets[j]) + '</li>';
            }
            return html + '</ul>';
        }

        function normalizeIssueDescriptionBullets(text) {
            return String(text || '')
                .replace(/\s+([-*•])\s+/g, '\n$1 ')
                .replace(/\s+(\d+[.)])\s+/g, '\n$1 ');
        }

        function renderAssistantRichText(text) {
            var html = escapeHtml(text || '');
            html = html.replace(/__(.+?)__/g, '<strong>$1</strong>');
            html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
            html = html.replace(/_(.+?)_/g, '<em>$1</em>');
            html = html.replace(/\*(.+?)\*/g, '<em>$1</em>');
            html = html.replace(/~~(.+?)~~/g, '<u>$1</u>');
            return html;
        }

        function normalizeImpactFixLine(line) {
            var match = String(line || '').match(/^(?:Fix\s*(?:requested\s*)?by|Must\s*fix\s*for|Requested\s*fix(?:\s*(?:timing|by))?|Needed\s*by)\s*:\s*(.+)$/i);
            if (!match) return '';
            var timing = match[1].replace(/[.;]\s*$/, '').trim();
            return timing ? 'Fix By: ' + timing : '';
        }

        function renderImpactSummaryText(text) {
            var lines = String(text || '').replace(/\r\n/g, '\n').replace(/\r/g, '\n')
                .replace(/\s+([-*•])\s+/g, '\n$1 ')
                .replace(/\s+(\d+[.)])\s+/g, '\n$1 ')
                .replace(/\s+((?:Fix\s*(?:requested\s*)?by|Must\s*fix\s*for|Requested\s*fix(?:\s*(?:timing|by))?|Needed\s*by)\s*:)\s*/ig, '\n$1 ')
                .split('\n')
                .map(function (line) { return line.trim(); })
                .filter(Boolean);
            if (!lines.length) return 'No impact summary available.';

            var fixLine = '';
            var bullets = [];
            lines.forEach(function (line) {
                var cleanLine = line.replace(/^\s*(?:[-*•]|\d+[.)])\s+/, '').trim();
                var normalizedFixLine = normalizeImpactFixLine(cleanLine);
                if (normalizedFixLine) {
                    fixLine = normalizedFixLine;
                } else if (cleanLine) {
                    bullets.push(cleanLine);
                }
            });

            if (!bullets.length && !fixLine) bullets = lines;

            var html = bullets.length ? '<ul class="cmf-assistant-impact-list">' + bullets.map(function (line) {
                return '<li>' + renderAssistantRichText(line) + '</li>';
            }).join('') + '</ul>' : '';

            if (fixLine) {
                html += '<div class="cmf-assistant-fix-line"><strong>' + renderAssistantRichText(fixLine) + '</strong></div>';
            }

            return html;
        }

        function renderMergedCmfRecommendation(result, payload) {
            result = result || {};
            payload = payload || {};
            var recommendation = normalizeCmfRecommendationLabel(result.Recommendation || 'CMF_REVIEW');
            var score = normalizeWeightedCmfScore(result.OverallQualityScore || 0);
            var scorePercent = Math.max(0, Math.min(100, score * 20));
            var rules = normalizeCmfDimensionScores(result.RuleScores || []);
            var missing = extractCmfMissingInformation(result.Evidence || '', rules, payload);
            var reasons = buildCmfRecommendationReasoning(recommendation, score, rules, result.Evidence || '', payload);
            var impact = extractCmfImpactSentence(result.Evidence || '', payload);
            var symptom = extractCmfSymptomEvidence(rules, reasons);
            var reviewerActions = buildCmfReviewerActions(recommendation, missing, rules, impact);
            var scoreDisplay = formatWeightedCmfScore(score) || '0.0';

            var html = '<div class="cmf-rec-assessment-shell">';
            html += '<section class="cmf-rec-summary-band">';
            html += '<div><div class="cmf-rec-summary-title">' + escapeHtml(recommendation) + '</div>';
            html += renderCmfScorePanel(scoreDisplay, scorePercent, recommendation);
            html += '</section>';

            html += '<section class="cmf-rec-stage-card"><h4>1. Issue Evidence</h4><ul>';
            html += '<li><strong>Issue symptom / failure mode:</strong> ' + escapeHtml(symptom) + '</li>';
            html += '<li><strong>Customer impact:</strong> ' + escapeHtml(impact) + '</li>';
            html += '<li><strong>Occurrence / repro:</strong> ' + escapeHtml(firstUsefulText(payload.reproducibility || payload.reproOnRvp, 'Missing occurrence or repro evidence.')) + '</li>';
            html += '<li><strong>Design:</strong> ' + escapeHtml(firstUsefulText(payload.customerDetail, 'Usage scenario not identified.')) + '</li>';
            html += '<li><strong>Workaround / recovery:</strong> ' + escapeHtml(findRecoverySignal(result.Evidence || '', payload, rules)) + '</li>';
            html += '<li><strong>Milestone / business exposure:</strong> ' + escapeHtml(findBusinessSignal(result.Evidence || '', payload, rules)) + '</li>';
            html += '</ul></section>';

            html += '<section class="cmf-rec-stage-card"><h4>2. Individual Metric Scores</h4>' + renderCmfMetricScoreTable(rules) + '</section>';

            html += '<div class="cmf-rec-stage-grid">';
            html += '<section class="cmf-rec-stage-card"><h4>3. Reasoning</h4>' + renderCmfBriefList(reasons) + '</section>';
            html += '<section class="cmf-rec-stage-card"><h4>4. Missing Information</h4>' + renderCmfMissingList(missing) + '</section>';
            html += '<section class="cmf-rec-stage-card"><h4>5. Reviewer Action</h4>' + renderCmfBriefList(reviewerActions) + '</section></div>';
            html += '</div>';
            return html;
        }

        function renderSightingQualityAssessment(result, payload) {
            result = result || {};
            payload = payload || {};
            var assessment = normalizeSightingQualityAssessment(result, payload);
            var score = assessment.score;
            var qualityStateClass = getQualityStateClass(score);
            window.CMF_PORTAL = window.CMF_PORTAL || {};
            window.CMF_PORTAL.currentSightingQualityAssessment = { assessment: assessment, payload: payload };
            var categoryItems = buildSqaCategoryItems(assessment);
            var scoreOutOfTen = formatSqaScoreOutOfTen(score);

            var html = '<section class="cmf-assistant-feature-card cmf-quality-assessment-card ' + qualityStateClass + '">';
            html += '<div class="cmf-assistant-feature-head"><div><h3>Sighting Quality Assessment</h3></div><div class="cmf-sqa-score-pill ' + qualityStateClass + '">Quality Score: ' + escapeHtml(scoreOutOfTen) + '/10</div></div>';
            html += '<div class="cmf-sqa-category-grid">';
            html += renderSqaCategoryCard('Missing Information', 'missing', categoryItems.missing);
            html += renderSqaCategoryCard('Partial Information', 'partial', categoryItems.partial);
            html += renderSqaCategoryCard('Conflicting Information', 'conflicting', categoryItems.conflicting);
            html += '</div>';
            html += '<div class="cmf-quality-action-row"><button type="button" class="cmf-quality-details-btn" onclick="showSightingQualityDetails()">View details</button></div>';
            html += '</section>';
            return html;
        }

        function renderSightingQualityAssessmentSkeleton() {
            return '<section class="cmf-assistant-feature-card cmf-quality-assessment-card cmf-sqa-skeleton" aria-label="Generating sighting quality assessment">' +
                '<div class="cmf-assistant-feature-head"><div><h3>Sighting Quality Assessment</h3></div></div>' +
                '<div class="ai-skeleton-loader">' +
                    '<div class="ai-skeleton-line" style="width:62%; height:13px;"></div>' +
                    '<div class="ai-skeleton-line" style="width:36%; height:13px; margin-top:8px;"></div>' +
                    '<div class="cmf-sqa-skeleton-grid">' +
                        '<div class="cmf-sqa-skeleton-box"><div class="ai-skeleton-line" style="width:72%;"></div><div class="ai-skeleton-line" style="width:54%; margin-top:10px;"></div></div>' +
                        '<div class="cmf-sqa-skeleton-box"><div class="ai-skeleton-line" style="width:70%;"></div><div class="ai-skeleton-line" style="width:62%; margin-top:10px;"></div></div>' +
                        '<div class="cmf-sqa-skeleton-box"><div class="ai-skeleton-line" style="width:78%;"></div><div class="ai-skeleton-line" style="width:50%; margin-top:10px;"></div></div>' +
                    '</div>' +
                '</div>' +
            '</section>';
        }

        function renderSightingQualityAssessmentUnavailable() {
            return '<section class="cmf-assistant-feature-card cmf-quality-assessment-card"><div class="cmf-assistant-feature-head"><div><h3>Sighting Quality Assessment</h3></div></div><div class="sighting-description-error">Sighting quality assessment unavailable.</div></section>';
        }

        function refreshSightingQualityAssessment() {
            var state = window.CMF_PORTAL && window.CMF_PORTAL.currentSightingQualityAssessment;
            if (!state || !state.payload) return;
            var qualityAssessmentNode = document.getElementById('cmfAssistantQualityAssessment');
            var detailsBody = document.getElementById('cmfDecisionDetailsBody');
            var payload = state.payload || {};
            if (detailsBody && !detailsBody.classList.contains('cmf-rec-hidden')) {
                detailsBody.innerHTML = '<div class="cmf-quality-detail-shell">' + renderSightingQualityAssessmentSkeleton() + '</div>';
            }
            if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessmentSkeleton();

            fetch('CMF_Web_portal.aspx/GetSightingQualityAssessment', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify(payload)
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessmentUnavailable();
                    if (detailsBody && !detailsBody.classList.contains('cmf-rec-hidden')) detailsBody.innerHTML = '<div class="sighting-description-error">Sighting quality assessment unavailable.</div>';
                    return;
                }
                var assessment = normalizeSightingQualityAssessment(result, payload);
                window.CMF_PORTAL.currentSightingQualityAssessment = { assessment: assessment, payload: payload };
                if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessment(result, payload);
                if (detailsBody && !detailsBody.classList.contains('cmf-rec-hidden')) detailsBody.innerHTML = renderSightingQualityDetails(assessment, payload);
            })
            .catch(function () {
                if (qualityAssessmentNode) qualityAssessmentNode.innerHTML = renderSightingQualityAssessmentUnavailable();
                if (detailsBody && !detailsBody.classList.contains('cmf-rec-hidden')) detailsBody.innerHTML = '<div class="sighting-description-error">Sighting quality assessment unavailable.</div>';
            });
        }

        function formatSqaScoreOutOfTen(score) {
            var value = Math.max(0, Math.min(10, (parseFloat(score) || 0) / 10));
            return value % 1 === 0 ? String(value.toFixed(0)) : value.toFixed(1);
        }

        function buildSqaCategoryItems(assessment) {
            var grouped = { missing: [], partial: [], conflicting: [] };
            var fields = assessment.fields || [];
            for (var i = 0; i < fields.length; i++) {
                var field = fields[i] || {};
                if (!isSqaCoreField(field.fieldName) && String(field.status || '').toLowerCase().indexOf('conflict') < 0) continue;
                var status = String(field.status || '').toLowerCase();
                if (status.indexOf('missing') >= 0) grouped.missing.push(field);
                else if (status.indexOf('conflict') >= 0) grouped.conflicting.push(field);
                else if (status.indexOf('partial') >= 0 || status.indexOf('review') >= 0) grouped.partial.push(field);
            }
            return grouped;
        }

        function isSqaCoreField(fieldName) {
            var text = String(fieldName || '').toLowerCase();
            return text === 'title'
                || text === 'reproducibility / failure rate'
                || text === 'processor'
                || text === 'soc stepping'
                || text === 'impact'
                || text === 'steps to reproduce'
                || text === 'exposure / priority'
                || text === 'me sku';
        }

        function renderSqaCategoryCard(title, type, items) {
            var list = (items || []).slice(0, 3);
            var html = '<div class="cmf-sqa-category-card ' + escapeHtml(type) + '"><div class="cmf-sqa-category-title">' + escapeHtml(title) + '</div><ul class="cmf-sqa-category-list">';
            if (!list.length) {
                html += '<li class="is-empty">None identified</li>';
            } else {
                for (var i = 0; i < list.length; i++) {
                    var field = list[i] || {};
                    html += '<li>' + escapeHtml(field.fieldName || 'Field') + '</li>';
                }
            }
            return html + '</ul></div>';
        }

        function getQualityStateClass(score) {
            if (score >= 80) return 'is-quality-high';
            if (score >= 50) return 'is-quality-medium';
            return 'is-quality-low';
        }

        function normalizeSightingQualityAssessment(result, payload) {
            var rawFields = result.Fields || result.fields || [];
            if (rawFields && rawFields.length) {
                var score = parseInt(result.QualityScore || result.qualityScore || 0, 10);
                var baseline = parseInt(result.BaselineScore || result.baselineScore || 0, 10);
                if (isNaN(score)) score = 0;
                if (isNaN(baseline)) baseline = score;
                var fields = [];
                for (var i = 0; i < rawFields.length; i++) {
                    var item = rawFields[i] || {};
                    fields.push({
                        fieldName: item.FieldName || item.fieldName || 'Field',
                        status: item.Status || item.status || 'Present',
                        characteristic: item.Characteristic || item.characteristic || 'Usable',
                        evidence: item.Evidence || item.evidence || '',
                        recommendation: item.Recommendation || item.recommendation || '',
                        decisionUse: item.DecisionUse || item.decisionUse || item.WhyItMatters || item.whyItMatters || ''
                    });
                }
                return {
                    score: Math.max(0, Math.min(100, score)),
                    baselineScore: Math.max(0, Math.min(100, baseline)),
                    dimensionScores: normalizeSqaDimensionScores(result.DimensionScores || result.dimensionScores || []),
                    tier: result.Tier || result.tier || '',
                    summary: [result.Summary || result.summary || 'Sighting completeness was assessed from database and HSD context.'],
                    issues: buildIssuesFromQualityFields(fields),
                    fields: fields
                };
            }
            var fallback = buildSightingQualityAssessment(result, payload);
            fallback.baselineScore = fallback.score;
            fallback.fields = buildFallbackQualityFields(fallback);
            fallback.dimensionScores = [];
            return fallback;
        }

        function normalizeSqaDimensionScores(rawDimensions) {
            var dimensions = [];
            for (var i = 0; i < (rawDimensions || []).length; i++) {
                var item = rawDimensions[i] || {};
                dimensions.push({
                    name: item.Name || item.name || item.DimensionName || item.dimensionName || 'Dimension',
                    score: Math.max(0, Math.min(100, parseInt(item.Score || item.score || 0, 10) || 0)),
                    weight: Math.max(0, parseFloat(item.Weight || item.weight || 0) || 0),
                    summary: item.Summary || item.summary || ''
                });
            }
            return dimensions;
        }

        function buildIssuesFromQualityFields(fields) {
            var issues = [];
            for (var i = 0; i < (fields || []).length; i++) {
                var field = fields[i] || {};
                if (!/^present$/i.test(field.status || '')) {
                    issues.push((field.fieldName || 'Field') + ': ' + (field.status || 'Needs review') + (field.characteristic ? ' - ' + field.characteristic : ''));
                }
            }
            return issues;
        }

        function buildQualityAttentionItems(assessment) {
            if (assessment.fields && assessment.fields.length) {
                var items = [];
                for (var i = 0; i < assessment.fields.length; i++) {
                    var field = assessment.fields[i] || {};
                    if (!/^present$/i.test(field.status || '')) {
                        items.push((field.fieldName || 'Field') + ': ' + (field.status || 'Needs review') + (field.characteristic ? ' - ' + field.characteristic : ''));
                    }
                }
                return items;
            }
            return assessment.issues || [];
        }

        function buildFallbackQualityFields(assessment) {
            var fields = [];
            var issues = assessment.issues || [];
            for (var i = 0; i < issues.length; i++) {
                var parts = String(issues[i]).split(':');
                var rawCharacteristic = parts.slice(1).join(':').replace(/^\s+/, '') || 'Needs review';
                var characteristic = /partial or vague/i.test(rawCharacteristic) ? 'Needs more specific evidence' : rawCharacteristic;
                fields.push({
                    fieldName: parts[0] || 'Field',
                    status: /missing/i.test(issues[i]) ? 'Missing' : (/conflict|disagree/i.test(issues[i]) ? 'Partial' : 'Partial'),
                    characteristic: characteristic,
                    evidence: 'Preliminary row-data check flagged this field before the full database and HSD assessment completed.',
                    recommendation: 'Add concrete sighting details or wait for the backend HSD assessment to refine this field.'
                });
            }
            return fields;
        }

        function renderQualityFieldList(items) {
            var html = '<ul class="cmf-quality-field-list">';
            for (var i = 0; i < items.length; i++) {
                html += '<li>' + escapeHtml(items[i]) + '</li>';
            }
            return html + '</ul>';
        }

        function showSightingQualityDetails() {
            var state = window.CMF_PORTAL && window.CMF_PORTAL.currentSightingQualityAssessment;
            if (!state || !state.assessment) return;

            var headingNode = document.getElementById('cmfRecHeading');
            var sightingDetails = document.getElementById('cmfAssistantSightingDetails');
            var qualitySlot = document.getElementById('cmfAssistantQualityAssessment');
            var recommendationSection = document.getElementById('cmfAssistantRecommendationSection');
            var impactSection = document.getElementById('cmfAssistantImpactSection');
            var reasoningSection = document.getElementById('cmfRecReasoningSection');
            var detailsBody = document.getElementById('cmfDecisionDetailsBody');
            if (headingNode) headingNode.innerHTML = '<button type="button" class="cmf-quality-back-btn" onclick="returnToCmfAssistantFromQualityDetails()">Back</button><span style="margin-left:10px;">Sighting Quality Details</span>';
            if (sightingDetails) sightingDetails.classList.add('cmf-rec-hidden');
            if (qualitySlot) qualitySlot.classList.add('cmf-rec-hidden');
            if (recommendationSection) recommendationSection.classList.add('cmf-rec-hidden');
            if (impactSection) impactSection.classList.add('cmf-rec-hidden');
            if (reasoningSection) reasoningSection.classList.add('cmf-rec-hidden');
            if (detailsBody) {
                detailsBody.classList.remove('cmf-rec-hidden');
                detailsBody.innerHTML = renderSightingQualityDetails(state.assessment, state.payload || {});
            }
        }

        function returnToCmfAssistantFromQualityDetails() {
            var headingNode = document.getElementById('cmfRecHeading');
            var sightingDetails = document.getElementById('cmfAssistantSightingDetails');
            var qualitySlot = document.getElementById('cmfAssistantQualityAssessment');
            var recommendationSection = document.getElementById('cmfAssistantRecommendationSection');
            var impactSection = document.getElementById('cmfAssistantImpactSection');
            var reasoningSection = document.getElementById('cmfRecReasoningSection');
            var detailsBody = document.getElementById('cmfDecisionDetailsBody');
            if (headingNode) headingNode.innerHTML = '<span class="cmf-assistant-title-icon" aria-hidden="true">✨</span>AI CMF Assistant';
            if (sightingDetails) sightingDetails.classList.remove('cmf-rec-hidden');
            if (qualitySlot) qualitySlot.classList.remove('cmf-rec-hidden');
            if (recommendationSection) recommendationSection.classList.remove('cmf-rec-hidden');
            if (impactSection) impactSection.classList.remove('cmf-rec-hidden');
            if (reasoningSection) reasoningSection.classList.remove('cmf-rec-hidden');
            if (detailsBody) {
                detailsBody.classList.add('cmf-rec-hidden');
                detailsBody.innerHTML = '';
            }
        }

        function renderSightingQualityDetails(assessment, payload) {
            var html = '<div class="cmf-quality-detail-shell">';
            html += renderSqaTemplateQualityOverview(assessment);
            html += renderSqaFieldEvidenceTable(assessment);
            html += renderSqaAssessmentSummary(assessment);
            html += renderSightingQualitySuggestions(assessment);
            html += '</div>';
            return html;
        }

        function renderSqaSightingInformationCard(payload) {
            payload = payload || {};
            var html = '<section class="cmf-quality-detail-card cmf-sqa-info-card"><h4><span class="cmf-sqa-card-icon" aria-hidden="true">&#xf0fe;</span>Sighting Information</h4>';
            html += '<div class="cmf-sqa-info-grid">';
            html += renderSqaInfoCell('Sighting ID', payload.cpId || 'N/A', true);
            html += renderSqaInfoCell('Title', payload.title || 'N/A', true);
            html += renderSqaInfoCell('Platform', getPlatformDisplayLabel(payload.platform || getIssuePendingPlatformValue()));
            html += renderSqaInfoCell('Component', payload.component || 'N/A');
            html += renderSqaInfoCell('Component Group', payload.componentGroup || payload.component || 'N/A');
            html += renderSqaInfoCell('Customer', extractSqaCustomer(payload.customerDetail));
            html += renderSqaInfoCell('Owner', payload.customerOwner || 'N/A');
            html += '</div></section>';
            return html;
        }

        function renderSqaInfoCell(label, value, wide) {
            return '<div class="cmf-sqa-info-cell' + (wide ? ' is-wide' : '') + '"><span>' + escapeHtml(label) + '</span><strong>' + escapeHtml(firstUsefulText(value, 'N/A')) + '</strong></div>';
        }

        function extractSqaCustomer(value) {
            var text = String(value || '').trim();
            if (!text) return 'N/A';
            var first = text.split(/[_\s-]+/).filter(Boolean)[0] || text;
            return first.charAt(0).toUpperCase() + first.slice(1);
        }

        function renderSqaTemplateQualityOverview(assessment) {
            var score = Math.max(0, Math.min(100, parseFloat(assessment.score) || 0));
            var counts = getSqaStatusCounts(assessment.fields || []);
            var quality = getSqaQualitySummary(score, counts);
            var html = '<section class="cmf-quality-detail-card cmf-sqa-overview-template"><h4><span class="cmf-sqa-card-icon" aria-hidden="true">&#xf201;</span>Quality Overview</h4>';
            html += '<div class="cmf-sqa-overview-grid">';
            html += '<div class="cmf-sqa-score-block"><div class="cmf-quality-donut" style="--score:' + escapeHtml(score) + '"><div class="cmf-quality-donut-value">' + escapeHtml(Math.round(score)) + '<span>/100</span></div></div><div class="cmf-sqa-score-message ' + escapeHtml(getQualityStateClass(score)) + '"><strong>' + escapeHtml(quality.title) + '</strong><span>' + escapeHtml(quality.message) + '</span></div></div>';
            html += '<div class="cmf-sqa-status-summary"><div class="cmf-quality-panel-label">Field Status Summary</div>';
            html += renderSqaStatusSummaryRow('Complete', counts.complete, 'present');
            html += renderSqaStatusSummaryRow('Partial', counts.partial, 'partial');
            html += renderSqaStatusSummaryRow('Missing', counts.missing, 'missing');
            html += renderSqaStatusSummaryRow('Conflicting', counts.conflicting, 'conflicting');
            html += '</div></div></section>';
            return html;
        }

        function getSqaQualitySummary(score, counts) {
            if (score >= 80) return { title: 'Good', message: 'Sufficient information for CMF review with minor gaps.' };
            if (score >= 50) return { title: 'Needs Review', message: 'Important information exists, but reviewers need a few clearer fields.' };
            return { title: 'Poor', message: 'Critical sighting information is missing or unclear for CMF review.' };
        }

        function getSqaStatusCounts(fields) {
            var counts = { complete: 0, partial: 0, missing: 0, conflicting: 0 };
            for (var i = 0; i < (fields || []).length; i++) {
                if (!isSqaCoreField((fields[i] || {}).fieldName)) continue;
                var status = normalizeSqaFieldStatusLabel((fields[i] || {}).status);
                if (status === 'Complete') counts.complete++;
                else if (status === 'Partial') counts.partial++;
                else if (status === 'Missing') counts.missing++;
                else if (status === 'Conflicting') counts.conflicting++;
            }
            return counts;
        }

        function renderSqaStatusSummaryRow(label, count, type) {
            return '<div class="cmf-sqa-status-summary-row ' + escapeHtml(type) + '"><span class="cmf-sqa-status-dot" aria-hidden="true"></span><span>' + escapeHtml(label) + '</span><strong>' + escapeHtml(count) + '</strong></div>';
        }

        function renderSqaFieldEvidenceTable(assessment) {
            var fields = (assessment.fields || []).filter(function (field) { return isSqaCoreField(field.fieldName); });
            var html = '<section class="cmf-quality-detail-card cmf-sqa-evidence-card"><h4><span class="cmf-sqa-card-icon" aria-hidden="true">&#xf15c;</span>Field Evidence</h4>';
            html += '<div class="cmf-sqa-evidence-table"><div class="cmf-sqa-evidence-row is-header"><div>#</div><div>Field Name</div><div>Detected Value</div><div>Status</div></div>';
            for (var i = 0; i < fields.length; i++) {
                var field = fields[i] || {};
                var statusClass = getQualityStatusClass(field.status);
                html += '<div class="cmf-sqa-evidence-row ' + escapeHtml(statusClass) + '"><div>' + escapeHtml(i + 1) + '</div><div>' + escapeHtml(field.fieldName || 'Field') + '</div><div>' + escapeHtml(getSqaDetectedValue(field)) + '</div><div>' + renderQualityStatusBadge(normalizeSqaFieldStatusLabel(field.status)) + '</div></div>';
            }
            html += '</div></section>';
            return html;
        }

        function getSqaDetectedValue(field) {
            var evidence = summarizeSqaEvidenceForReason(field && field.evidence);
            if (!evidence || /^no usable value found/i.test(evidence)) return 'Not found in HSD';
            return evidence.length > 92 ? evidence.slice(0, 89).replace(/\s+\S*$/, '') + '...' : evidence;
        }

        function renderSqaAssessmentSummary(assessment) {
            var fields = assessment.fields || [];
            var gaps = [];
            for (var i = 0; i < fields.length; i++) {
                var field = fields[i] || {};
                if (!isSqaCoreField(field.fieldName) || /^present$/i.test(field.status || '')) continue;
                gaps.push((field.fieldName || 'Field') + ' is ' + normalizeSqaFieldStatusLabel(field.status).toLowerCase());
            }
            var summary = (assessment.summary && assessment.summary.length ? assessment.summary[0] : '') || 'The sighting quality was assessed from the requested HSD checklist fields.';
            if (gaps.length) summary += ' Main gaps: ' + gaps.slice(0, 3).join('; ') + '.';
            var html = '<section class="cmf-quality-detail-card cmf-sqa-assessment-summary"><h4><span class="cmf-sqa-sparkle" aria-hidden="true">&#10024;</span>Assessment Summary</h4>';
            html += '<p>' + escapeHtml(summary) + '</p></section>';
            return html;
        }

        function renderSightingQualityScoreOverview(assessment) {
            var fields = (assessment.fields || []).filter(function (field) { return isSqaCoreField(field.fieldName); });
            var score = Math.max(0, Math.min(100, parseFloat(assessment.score) || 0));
            var html = '<section class="cmf-quality-detail-card cmf-quality-overview-card">';
            html += '<div class="cmf-quality-score-topline"><span aria-hidden="true"></span><div class="cmf-quality-score-chart-wrap"><div class="cmf-quality-panel-label">Quality Score</div><div class="cmf-quality-donut" style="--score:' + escapeHtml(score) + '"><div class="cmf-quality-donut-value">' + escapeHtml(formatSqaScoreOutOfTen(score)) + '/10</div></div></div><button type="button" class="cmf-quality-refresh-btn" onclick="refreshSightingQualityAssessment()"><span aria-hidden="true">&#8635;</span><span>Refresh</span></button></div>';
            html += '<div class="cmf-quality-score-list">';
            html += '<div class="cmf-quality-score-row is-header"><div>Field Name</div><div>Field Score</div><div>Status</div></div>';
            for (var i = 0; i < fields.length; i++) html += renderSightingQualityScoreRow(fields[i]);
            html += '</div></section>';
            return html;
        }

        function renderSightingQualityScoreRow(field) {
            field = field || {};
            var statusClass = getQualityStatusClass(field.status);
            var statusText = normalizeSqaFieldStatusLabel(field.status);
            var fieldScore = formatSqaFieldScore(getSqaFieldScore(field.status));
            return '<div class="cmf-quality-score-row ' + escapeHtml(statusClass) + '">' +
                '<div class="cmf-quality-score-name">' + escapeHtml(field.fieldName || 'Field') + '</div>' +
                '<div class="cmf-quality-score-field-score">' + escapeHtml(fieldScore) + '</div>' +
                '<div class="cmf-quality-score-status">' + escapeHtml(statusText) + '</div>' +
            '</div>';
        }

        function getSqaBriefFactor(field) {
            field = field || {};
            var status = normalizeSqaFieldStatusLabel(field.status);
            var fieldName = String(field.fieldName || '').toLowerCase();
            var text = String(field.characteristic || '').replace(/\s+/g, ' ').trim();
            var lower = text.toLowerCase();
            if (status === 'Complete') {
                if (fieldName.indexOf('title') >= 0 || fieldName.indexOf('subject') >= 0) return 'Clear title provided';
                if (fieldName.indexOf('description') >= 0 || fieldName.indexOf('happened') >= 0) return 'Problem details provided';
                if (fieldName.indexOf('impact') >= 0) return 'Impact context provided';
                if (fieldName.indexOf('repro') >= 0 || fieldName.indexOf('failure rate') >= 0) return 'Repro details provided';
                if (fieldName.indexOf('trigger') >= 0) return 'Trigger details provided';
                if (fieldName.indexOf('platform') >= 0 || fieldName.indexOf('product') >= 0) return 'Platform context provided';
                if (fieldName.indexOf('configuration') >= 0) return 'Configuration details provided';
                if (fieldName.indexOf('debug') >= 0 || fieldName.indexOf('attachment') >= 0) return 'Debug evidence available';
                return 'Sufficient evidence provided';
            }
            if (status === 'Missing') return 'No usable evidence found';
            if (status === 'Conflicting') return 'Evidence does not agree';
            if (!text) return 'Needs more detail';
            if (lower.indexOf('empty') >= 0 || lower.indexOf('unavailable') >= 0) return 'No usable value';
            if (lower.indexOf('idst') >= 0 || lower.indexOf('routing') >= 0) return 'Routing incomplete';
            if (lower.indexOf('numeric') >= 0 || lower.indexOf('repro') >= 0) return 'Rate unclear';
            if (lower.indexOf('trigger') >= 0 || lower.indexOf('scenario') >= 0) return 'Scenario unclear';
            if (lower.indexOf('impact') >= 0) return 'Impact unclear';
            if (lower.indexOf('configuration') >= 0 || lower.indexOf('os') >= 0 || lower.indexOf('driver') >= 0) return 'Config incomplete';
            if (lower.indexOf('description') >= 0 || lower.indexOf('behavior') >= 0) return 'Description weak';
            if (lower.indexOf('title') >= 0 || lower.indexOf('subject') >= 0) return 'Title weak';
            var words = text.split(' ').filter(Boolean).slice(0, 8);
            return words.join(' ');
        }

        function getSqaFieldScore(status) {
            var text = String(status || '').toLowerCase();
            if (text.indexOf('missing') >= 0) return 0;
            if (text.indexOf('conflict') >= 0) return 0;
            if (text.indexOf('partial') >= 0 || text.indexOf('review') >= 0) return 5;
            return 10;
        }

        function formatSqaFieldScore(score) {
            var value = Math.max(0, Math.min(10, parseFloat(score) || 0));
            return (value % 1 === 0 ? value.toFixed(0) : value.toFixed(1)) + '/10';
        }

        function normalizeSqaFieldStatusLabel(status) {
            var text = String(status || '').toLowerCase();
            if (text.indexOf('missing') >= 0) return 'Missing';
            if (text.indexOf('conflict') >= 0) return 'Conflicting';
            if (text.indexOf('partial') >= 0 || text.indexOf('review') >= 0) return 'Partial';
            return 'Complete';
        }

        function renderSightingQualitySuggestions(assessment) {
            var fields = assessment.fields || [];
            var suggestions = [];
            for (var i = 0; i < fields.length; i++) {
                var field = fields[i] || {};
                if (/^present$/i.test(field.status || '')) continue;
                var suggestion = field.recommendation || ('Improve ' + (field.fieldName || 'this field') + '.');
                if (suggestions.indexOf(suggestion) < 0) suggestions.push(suggestion);
            }
            if (!suggestions.length) suggestions.push('No immediate quality improvements were identified for the core SQA fields.');
            var html = '<section class="cmf-quality-detail-card cmf-sqa-next-steps"><h4><span class="cmf-sqa-card-icon" aria-hidden="true">&#xf0eb;</span>Suggested Next Steps</h4><ol class="cmf-quality-suggestions">';
            for (var j = 0; j < suggestions.length; j++) html += '<li>' + escapeHtml(suggestions[j]) + '</li>';
            return html + '</ol></section>';
        }

        function renderQualityScoreNotes(items) {
            var html = '<ul class="cmf-quality-score-notes">';
            for (var i = 0; i < (items || []).length; i++) {
                if (String(items[i] || '').trim()) html += '<li>' + escapeHtml(items[i]) + '</li>';
            }
            return html + '</ul>';
        }

        function renderQualityDetailSection(title, items) {
            if (!items || !items.length) return '';
            return '<section class="cmf-quality-detail-card"><h4>' + escapeHtml(title) + '</h4>' + renderQualityDetailCards(items) + '</section>';
        }

        function renderQualityDetailCards(items) {
            return '<div class="cmf-quality-detail-list">' + items.join('') + '</div>';
        }

        function renderSightingQualityFieldCard(field) {
            field = field || {};
            var statusClass = getQualityStatusClass(field.status);
            var statusText = field.status || 'Needs review';
            var html = '<article class="cmf-quality-field-card ' + statusClass + '">';
            html += '<div class="cmf-quality-field-card-head"><div class="cmf-quality-field-card-title">' + escapeHtml(field.fieldName || 'Field') + '</div>' + renderQualityStatusBadge(statusText) + '</div>';
            html += '<p><strong>Reason:</strong> ' + escapeHtml(getSqaStatusReason(field)) + '</p>';
            html += '</article>';
            return html;
        }

        function getSqaStatusReason(field) {
            field = field || {};
            var status = normalizeSqaFieldStatusLabel(field.status);
            var reason = getSqaDetailedReasonText(field);
            if (reason) return reason;
            var factor = getSqaBriefFactor(field);
            if (status === 'Missing') return factor + ' for this field.';
            if (status === 'Conflicting') return factor + ' for this field.';
            if (status === 'Partial') return factor + ' for this field.';
            return factor + '.';
        }

        function getSqaDetailedReasonText(field) {
            field = field || {};
            var status = normalizeSqaFieldStatusLabel(field.status);
            var insight = getSqaReasonInsight(field);
            var evidence = summarizeSqaEvidenceMeaning(field, status);
            var evidenceValue = summarizeSqaEvidenceValue(field);
            var hasMissingEvidence = status === 'Missing' || isGenericMissingSqaReason(cleanSqaReasonText(field.characteristic));
            if (status === 'Missing') {
                return buildSqaNaturalSqaReason('missing', insight, evidence, evidenceValue, hasMissingEvidence);
            }
            if (status === 'Partial') {
                return buildSqaNaturalSqaReason('partial', insight, evidence, evidenceValue, hasMissingEvidence);
            }
            if (status === 'Conflicting') {
                return buildSqaNaturalSqaReason('conflicting', insight, evidence, evidenceValue, hasMissingEvidence);
            }
            return '';
        }

        function buildSqaNaturalSqaReason(status, insight, evidence, evidenceValue, hasMissingEvidence) {
            insight = insight || {};
            var evidenceText = evidence ? lowerFirstSqaReason(evidence) : '';
            var valueText = evidenceValue ? lowerFirstSqaReason(evidenceValue) : '';
            var valueClause = valueText ? ' The available information says ' + valueText.replace(/[.!?]+$/, '') + '.' : '';
            if (status === 'missing') {
                if (hasMissingEvidence || !evidenceText) {
                    return sentenceCaseSqa('This field is marked missing because the sighting does not clearly provide ' + insight.focus + '.' + valueClause + ' Reviewers need ' + insight.reviewNeed + ' before this information can support a CMF decision.');
                }
                return sentenceCaseSqa('This field is marked missing because the information only suggests ' + evidenceText + ', but it does not clearly confirm ' + insight.focus + '.' + valueClause + ' Reviewers need ' + insight.reviewNeed + ' before this information can support a CMF decision.');
            }
            if (status === 'partial') {
                if (evidenceText) {
                    return sentenceCaseSqa('This field is marked partial because ' + evidenceText + ', but the detail is not complete enough for confident CMF review.' + valueClause + ' This creates reviewer uncertainty because ' + insight.partial + '. A complete rating would need ' + insight.reviewNeed + '.');
                }
                return sentenceCaseSqa('This field is marked partial because some useful context is present, but it is not strong enough for confident CMF review.' + valueClause + ' This creates reviewer uncertainty because ' + insight.partial + '. A complete rating would need ' + insight.reviewNeed + '.');
            }
            if (evidenceText) {
                return sentenceCaseSqa('This field is marked conflicting because the available information points in more than one direction: ' + evidenceText + '.' + valueClause + ' This can confuse reviewers because ' + insight.conflicting + '. It should be clarified with ' + insight.reviewNeed + '.');
            }
            return sentenceCaseSqa('This field is marked conflicting because the available information does not align cleanly.' + valueClause + ' This can confuse reviewers because ' + insight.conflicting + '. It should be clarified with ' + insight.reviewNeed + '.');
        }

        function summarizeSqaEvidenceValue(field) {
            var text = summarizeSqaEvidenceForReason(field && field.evidence);
            if (!text) return '';
            text = cleanSqaEvidenceValueForSentence(text);
            text = text.replace(/\s*;\s*/g, '; ');
            if (/^no usable value found/i.test(text)) return '';
            if (text.length > 150) text = text.slice(0, 147).replace(/\s+\S*$/, '') + '...';
            return text;
        }

        function cleanSqaEvidenceValueForSentence(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            text = text.replace(/\[[^\]]+\]/g, ' ').replace(/\s+/g, ' ').trim();
            text = text.replace(/\b(blocking|customer|stress\s+test|fvt)\b/gi, ' ').replace(/\s+/g, ' ').trim();
            text = text.replace(/\bthe units occur hang when run\b/i, 'the units hang during the run');
            text = text.replace(/\boccur hang\b/i, 'hangs');
            text = text.replace(/\brun\b$/i, 'run');
            if (text && !/[.!?]$/.test(text)) text += '.';
            return text;
        }

        function summarizeSqaEvidenceMeaning(field, status) {
            var raw = String((field && field.evidence) || '').toLowerCase();
            var fieldName = String((field && field.fieldName) || '').toLowerCase();
            if (!raw || /^no usable value found/i.test(raw)) return '';
            if (fieldName.indexOf('description') >= 0 || fieldName.indexOf('happened') >= 0) {
                if (/display|video|monitor|screen|blank/.test(raw)) return 'a visible symptom is mentioned';
                return 'a symptom is mentioned without enough observed behavior detail';
            }
            if (fieldName.indexOf('debug') >= 0 || fieldName.indexOf('attachment') >= 0) {
                if (/no\s+idst|not\s+assigned|unassigned/.test(raw) && /attach|log|scope|zip|yes/.test(raw)) return 'some debug material is referenced while ownership or routing remains unclear';
                if (/attach|log|scope|zip|yes/.test(raw)) return 'some debug material is referenced';
                return 'debug context is mentioned without enough supporting material';
            }
            if (fieldName.indexOf('failure rate') >= 0 || fieldName.indexOf('reproducibility') >= 0) {
                if (/\d+\s*%/.test(raw)) return status === 'Conflicting' ? 'different occurrence values are present' : 'an occurrence value is mentioned';
                return 'repeatability is implied without a clear rate or sample size';
            }
            if (fieldName.indexOf('trigger') >= 0 || fieldName.indexOf('steps to reproduce') >= 0) {
                if (/(^|\s)\d+[\.)]/.test(raw) || /step\s*\d+/i.test(raw)) return 'some reproduction steps are written, but they do not fully explain a clear ordered path to reproduce the issue';
                return 'a scenario may be implied, but clear reproduction steps are not available';
            }
            if (fieldName.indexOf('processor') >= 0) return 'processor information is mentioned but the exact processor context may still be unclear';
            if (fieldName.indexOf('stepping') >= 0) return 'a stepping-related value may be present, but the SoC stepping is not clearly called out';
            if (fieldName.indexOf('exposure') >= 0 || fieldName.indexOf('priority') >= 0) return 'priority or exposure information is present but the severity is not clearly called out';
            if (fieldName.indexOf('me sku') >= 0 || fieldName.indexOf('me_sku') >= 0) return 'ME SKU information is present but it does not clearly say Consumer or Corp';
            if (fieldName.indexOf('impact') >= 0) return 'some effect is implied without enough consequence or severity detail';
            if (fieldName.indexOf('configuration') >= 0) return 'some setup details are present while important configuration context is still missing';
            if (fieldName.indexOf('platform') >= 0 || fieldName.indexOf('product') >= 0) return 'some platform context is present without enough specificity';
            if (fieldName.indexOf('ownership') >= 0 || fieldName.indexOf('routing') >= 0) return 'some routing context is present without a clear responsible owner';
            if (fieldName.indexOf('status') >= 0 || fieldName.indexOf('resolution') >= 0) return 'some status context is present without a clear resolution state';
            if (fieldName.indexOf('title') >= 0 || fieldName.indexOf('subject') >= 0) return 'a short issue summary is present but not fully descriptive';
            return 'some related information is present';
        }

        function getSqaReasonInsight(field) {
            var text = String((field && field.fieldName) || '').toLowerCase();
            if (text === 'title' || text.indexOf('title') >= 0 || text.indexOf('subject') >= 0) {
                return {
                    missing: 'the sighting does not provide a usable short description of the issue',
                    partial: 'the title gives some context, but it does not clearly capture the issue or affected area',
                    conflicting: 'the title and the rest of the sighting point to different issue descriptions',
                    focus: 'what issue is being reported',
                    reviewNeed: 'a concise title that names the issue and affected area'
                };
            }
            if (text.indexOf('description') >= 0 || text.indexOf('happened') >= 0) {
                return {
                    missing: 'there is no clear explanation of what actually happened',
                    partial: 'the sighting describes the problem only at a high level and leaves the observed behavior or scope unclear',
                    conflicting: 'the problem description does not match the other issue details',
                    focus: 'what actually happened and where it was observed',
                    reviewNeed: 'a clear statement of the observed behavior and affected scope'
                };
            }
            if (text.indexOf('impact') >= 0) {
                return {
                    missing: 'the customer or validation impact is not stated',
                    partial: 'some impact is implied, but the practical consequence or severity is not clear enough',
                    conflicting: 'the impact statements suggest different levels of severity or affected users',
                    focus: 'the customer or validation impact',
                    reviewNeed: 'the practical consequence, severity, and affected users or validation area'
                };
            }
            if (text.indexOf('failure rate') >= 0 || text.indexOf('reproducibility') >= 0) {
                return {
                    missing: 'the sighting does not say how often the issue occurs',
                    partial: 'the issue appears reproducible, but the exact rate, sample size, or repeatability is not clear',
                    conflicting: 'the occurrence rate is reported differently across the available details',
                    focus: 'how often the issue occurs',
                    reviewNeed: 'a specific occurrence rate, sample size, or repeatability statement'
                };
            }
            if (text.indexOf('trigger') >= 0 || text.indexOf('steps to reproduce') >= 0) {
                return {
                    missing: 'the steps or condition that trigger the issue are not described',
                    partial: 'some steps or scenario details are present, but the reviewer cannot clearly follow them to reproduce the issue',
                    conflicting: 'the stated trigger does not line up with the rest of the sighting details',
                    focus: 'what steps or condition reproduce the issue',
                    reviewNeed: 'clear ordered reproduction steps or the exact trigger condition'
                };
            }
            if (text.indexOf('processor') >= 0) {
                return {
                    missing: 'the processor is not called out',
                    partial: 'processor context is hinted at, but the exact processor or processor family is not clear enough',
                    conflicting: 'different processor details appear in the available information',
                    focus: 'which processor the sighting applies to',
                    reviewNeed: 'the exact processor or processor family'
                };
            }
            if (text.indexOf('stepping') >= 0) {
                return {
                    missing: 'the SoC stepping is not called out',
                    partial: 'there may be stepping-related context, but the actual SoC stepping value is not explicit',
                    conflicting: 'different stepping values appear in the available information',
                    focus: 'which SoC stepping the sighting applies to',
                    reviewNeed: 'the exact SoC stepping value'
                };
            }
            if (text.indexOf('exposure') >= 0 || text.indexOf('priority') >= 0) {
                return {
                    missing: 'the exposure or priority is not called out',
                    partial: 'some priority context exists, but it does not clearly say whether the sighting is Showstopper or Critical',
                    conflicting: 'the exposure or priority values do not agree with each other',
                    focus: 'the exposure or priority severity',
                    reviewNeed: 'a clear exposure or priority value such as Showstopper or Critical'
                };
            }
            if (text.indexOf('me sku') >= 0 || text.indexOf('me_sku') >= 0) {
                return {
                    missing: 'the ME SKU is not called out',
                    partial: 'ME SKU context may be present, but it does not clearly say Consumer or Corp',
                    conflicting: 'different ME SKU values appear in the available information',
                    focus: 'whether the ME SKU is Consumer or Corp',
                    reviewNeed: 'a clear ME SKU value of Consumer or Corp'
                };
            }
            if (text.indexOf('platform') >= 0 || text.indexOf('product') >= 0) {
                return {
                    missing: 'the affected platform, product, or component context is not clear',
                    partial: 'some platform context is present, but it is not specific enough to identify the affected configuration confidently',
                    conflicting: 'different platform or product details appear to refer to different targets',
                    focus: 'which platform or product is affected',
                    reviewNeed: 'specific platform, product, component, board, or family details'
                };
            }
            if (text.indexOf('configuration') >= 0) {
                return {
                    missing: 'the system setup needed to understand the issue is not provided',
                    partial: 'some configuration details are present, but important setup information such as OS, BIOS, driver, board, or build is still incomplete',
                    conflicting: 'the configuration details do not agree with each other',
                    focus: 'the setup in which the issue occurs',
                    reviewNeed: 'the missing setup details such as OS, BIOS, driver, board, or build'
                };
            }
            if (text.indexOf('debug') >= 0 || text.indexOf('attachment') >= 0) {
                return {
                    missing: 'there is no usable debug evidence, attachment, or log reference for review',
                    partial: 'debug evidence is mentioned, but it is not complete enough to support confident triage',
                    conflicting: 'the debug evidence points in different directions and needs reviewer confirmation',
                    focus: 'whether there is enough debug material for triage',
                    reviewNeed: 'clear logs, attachments, debug ownership, or a usable evidence reference'
                };
            }
            if (text.indexOf('ownership') >= 0 || text.indexOf('routing') >= 0) {
                return {
                    missing: 'the sighting does not identify who owns or should review the issue',
                    partial: 'some routing information exists, but ownership or debug responsibility is not clear enough',
                    conflicting: 'the available routing details point to different owners or forums',
                    focus: 'who owns or should review the issue',
                    reviewNeed: 'a clear owner, submitter, debug forum, or routing path'
                };
            }
            if (text.indexOf('status') >= 0 || text.indexOf('resolution') >= 0) {
                return {
                    missing: 'there is no useful status, fix, closure, or resolution context',
                    partial: 'some status context exists, but it does not clearly explain the current resolution state',
                    conflicting: 'the status or resolution details do not agree with each other',
                    focus: 'the current status or resolution state',
                    reviewNeed: 'the current status, fix, closure reason, or resolution state'
                };
            }
            return {
                missing: 'the information needed for this field is not available',
                partial: 'some information is available, but it is not complete enough for confident review',
                conflicting: 'the available information does not agree and needs reviewer confirmation',
                focus: 'the information needed for this field',
                reviewNeed: 'clearer evidence for this field'
            };
        }

        function cleanSqaReasonText(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!text || /^(usable|complete|present|needs review)$/i.test(text)) return '';
            return stripSqaSourceLabels(text).replace(/[.;:,\s]+$/g, '');
        }

        function summarizeSqaEvidenceForReason(value) {
            var text = cleanSqaReasonText(value);
            if (!text || /^no usable value found/i.test(text)) return '';
            text = text.replace(/\b(DB|HSD)\b\s*/gi, '');
            text = text.replace(/\s*\|\s*/g, '; ');
            if (text.length > 120) text = text.slice(0, 117).replace(/\s+\S*$/, '') + '...';
            return text;
        }

        function isGenericMissingSqaReason(value) {
            var text = String(value || '').toLowerCase();
            return !text || text.indexOf('empty') >= 0 || text.indexOf('unavailable') >= 0 || text.indexOf('no usable') >= 0;
        }

        function stripSqaSourceLabels(value) {
            return String(value || '')
                .replace(/\b(?:DB|HSD)\s+([A-Za-z][A-Za-z\s/]+):\s*/g, '')
                .replace(/\b(?:Problem Description|Repro Steps|What Happened|Description|Actual Behavior|Customer Impact|Customer Summary|Customer Detail|Title|Subject|Component|Family|Board|Platform|Operating System|Processor|SoC Stepping|SOC Stepping|Stepping|Exposure|ME SKU|ME_SKU|System Scope Filename|Debug Log Attached|Sysdebug Category|Sysdebug Forum|Owner|Submitter|Status|Fix Description|Closed Reason|Fixed Version|CMF Justification|RVP Repro|Reproducibility|Steps to Reproduce|What Should Have Happened|Impact|Priority|iDST):\s*/g, '')
                .replace(/\bHSD\b\s*/g, '')
                .replace(/\bDB\b\s*/g, '');
        }

        function getSqaReadableFieldName(fieldName) {
            var text = String(fieldName || 'this field').toLowerCase();
            if (text.indexOf('title') >= 0 || text.indexOf('subject') >= 0) return 'the issue title';
            if (text.indexOf('description') >= 0 || text.indexOf('happened') >= 0) return 'the problem description';
            if (text.indexOf('impact') >= 0) return 'customer impact';
            if (text.indexOf('failure rate') >= 0 || text.indexOf('reproducibility') >= 0) return 'reproducibility';
            if (text.indexOf('trigger') >= 0 || text.indexOf('steps to reproduce') >= 0) return 'steps to reproduce';
            if (text.indexOf('processor') >= 0) return 'processor';
            if (text.indexOf('stepping') >= 0) return 'SoC stepping';
            if (text.indexOf('exposure') >= 0 || text.indexOf('priority') >= 0) return 'exposure or priority';
            if (text.indexOf('me sku') >= 0 || text.indexOf('me_sku') >= 0) return 'ME SKU';
            if (text.indexOf('platform') >= 0 || text.indexOf('product') >= 0) return 'platform context';
            if (text.indexOf('configuration') >= 0) return 'system configuration';
            if (text.indexOf('debug') >= 0 || text.indexOf('attachment') >= 0) return 'debug evidence';
            if (text.indexOf('ownership') >= 0 || text.indexOf('routing') >= 0) return 'ownership or routing';
            if (text.indexOf('status') >= 0 || text.indexOf('resolution') >= 0) return 'status or resolution context';
            return 'this field';
        }

        function sentenceCaseSqa(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!text) return '';
            text = text.charAt(0).toUpperCase() + text.slice(1);
            if (!/[.!?]$/.test(text)) text += '.';
            return text;
        }

        function lowerFirstSqaReason(value) {
            var text = cleanSqaReasonText(value);
            if (!text) return '';
            if (/^[A-Z]{2}/.test(text)) return text;
            return text.charAt(0).toLowerCase() + text.slice(1);
        }

        function renderQualityStatusBadge(status) {
            return '<span class="cmf-quality-status-badge ' + getQualityStatusClass(status) + '">' + escapeHtml(status || 'Needs review') + '</span>';
        }

        function getQualityStatusClass(status) {
            var text = String(status || '').toLowerCase();
            if (text.indexOf('missing') >= 0) return 'missing';
            if (text.indexOf('conflict') >= 0) return 'conflicting';
            if (text.indexOf('partial') >= 0 || text.indexOf('review') >= 0) return 'partial';
            return 'present';
        }

        function buildSightingQualityAssessment(result, payload) {
            var rules = result.RuleScores || [];
            var mandatoryFields = [
                { key: 'symptom', label: 'Symptom / failure mode', value: payload.title, ruleId: 'S', partialPattern: /^(issue|failure|problem|error|fails?|not working|observed|seen)$/i },
                { key: 'occurrence', label: 'Occurrence / reproducibility', value: payload.reproducibility || payload.reproOnRvp, ruleId: 'O', partialPattern: /^(sometimes|intermittent|frequent|rare|random|occasionally|reproduced|not sure)$/i },
                { key: 'usage', label: 'Usage trigger / decision context', value: payload.customerDetail || payload.component, ruleId: 'D', partialPattern: /^(customer|platform|component|validation|test|scenario)$/i },
                { key: 'impact', label: 'Customer impact', value: payload.impact, ruleId: 'B', partialPattern: /^(impact|blocked|blocks|issue observed|issues observed|customer impact|affects customer)$/i },
                { key: 'owner', label: 'Owner / debug routing', value: payload.idst || payload.customerOwner, ruleId: '', partialPattern: /^(owner|debug|idst|tbd|pending)$/i }
            ];
            var present = 0;
            var partial = [];
            var missing = [];
            var conflicting = findOccurrenceConflicts(payload, rules);

            for (var i = 0; i < mandatoryFields.length; i++) {
                var field = mandatoryFields[i];
                var value = String(field.value || '').replace(/\s+/g, ' ').trim();
                var rule = field.ruleId ? findCmfRuleById(rules, field.ruleId) : null;
                var ruleScore = rule ? parseInt(normalizeDimensionScore(rule.Score || '1'), 10) : 3;
                if (!isUsefulQualityText(value)) {
                    missing.push(field.label + ': missing');
                } else if (isPartialQualityText(value, field.partialPattern) || (rule && ruleScore <= 2)) {
                    partial.push(field.label + ': partial or vague');
                    present += 0.5;
                } else {
                    present += 1;
                }
            }

            var rawScore = Math.round((present / mandatoryFields.length) * 100);
            var penalty = (conflicting.length * 15) + (partial.length * 5);
            var score = Math.max(0, Math.min(100, rawScore - penalty));
            var issues = missing.concat(partial).concat(conflicting);
            var summary = [
                'Mandatory CMF fields assessed: symptom, occurrence, usage context, customer impact, and owner/debug routing.',
                'Completeness score before conflict penalties: ' + rawScore + '/100.',
                issues.length ? 'Confidence reduced by ' + issues.length + ' missing, partial, or conflicting signal' + (issues.length === 1 ? '.' : 's.') : 'Current sighting has enough decision context for initial CMF review.'
            ];
            return { score: score, summary: summary, issues: issues };
        }

        function isUsefulQualityText(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            return !!text && !/^n\/?a$|^none$|^unknown$|^missing$|^null$|^tbd$|^na$/i.test(text);
        }

        function isPartialQualityText(value, fieldPattern) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!isUsefulQualityText(text)) return false;
            if (text.length < 18 || text.split(/\s+/).length < 4) return true;
            if (fieldPattern && fieldPattern.test(text)) return true;
            return /^(it should be reproduced|need to check|under debug|to be updated|will update|issue observed|issues observed|blocks fvt exit)$/i.test(text);
        }

        function findOccurrenceConflicts(payload, rules) {
            var conflicts = [];
            var titleBand = classifyOccurrenceSignal(payload.title);
            var reproBand = classifyOccurrenceSignal(payload.reproducibility || payload.reproOnRvp);
            var impactBand = classifyOccurrenceSignal(payload.impact);
            var occurrenceRule = findCmfRuleById(rules, 'O');
            var ruleBand = occurrenceRule ? classifyOccurrenceSignal(occurrenceRule.Evaluation) : '';
            var referenceBand = titleBand || reproBand || ruleBand;
            if (referenceBand && impactBand && referenceBand !== impactBand) {
                conflicts.push('Occurrence: title/repro says ' + referenceBand + ', but impact says ' + impactBand + '.');
            }
            if (titleBand && reproBand && titleBand !== reproBand) {
                conflicts.push('Occurrence: title and reproducibility fields disagree.');
            }
            return conflicts;
        }

        function classifyOccurrenceSignal(value) {
            var text = String(value || '').toLowerCase();
            if (!text) return '';
            if (/>?=\s*1\s*\/\s*10|1\s*in\s*10|hourly|frequent|frequently|always|every\s+time|100%/i.test(text)) return 'frequent';
            if (/1\s*\/\s*1000|1\s*in\s*1000|<\s*1\s*\/\s*1000|rare|sporadic|occasionally|intermittent/i.test(text)) return 'rare';
            if (/1\s*\/\s*5000|1\s*in\s*5000|very\s+rare/i.test(text)) return 'very rare';
            return '';
        }

        function renderCmfScorePanel(scoreDisplay, scorePercent, recommendation) {
            return '<div class="cmf-rec-score-panel" style="--score-percent:' + escapeHtml(scorePercent) + '%">' +
                '<div class="cmf-rec-score-top"><span class="cmf-rec-score-label">Weighted evidence score</span><span class="cmf-rec-score-value">' + escapeHtml(scoreDisplay) + '/5</span></div>' +
                '<div class="cmf-rec-score-track" aria-hidden="true"><div class="cmf-rec-score-fill"></div></div>' +
                '<div class="cmf-rec-score-caption">' + escapeHtml(buildCmfScoreCaption(recommendation)) + '</div>' +
            '</div>';
        }

        function buildCmfScoreCaption(recommendation) {
            var rec = String(recommendation || '').toUpperCase();
            if (rec === 'CMF_OK') return 'Meets the CMF threshold; verify no blocking evidence is missing.';
            if (rec === 'CMF_REVIEW') return 'Near or above threshold, but reviewer judgment is needed.';
            if (rec === 'CMF_INCOMPLETE') return 'Evidence is not complete enough for a final CMF decision.';
            return 'Below the CMF threshold based on current evidence.';
        }

        function extractCmfSymptomEvidence(rules, reasons) {
            for (var i = 0; i < (rules || []).length; i++) {
                var rule = rules[i] || {};
                if (String(rule.RuleId || '').toUpperCase() === 'S') {
                    var severityText = cleanCmfEvidenceSnippet(String(rule.Evaluation || '').replace(/^S\d\s*[:\-]?\s*/i, '').replace(/^SCORE\s*\d\s*[-:]?\s*/i, ''));
                    if (severityText && !/^missing|no symptom/i.test(severityText)) return trimToCompleteSentence(severityText, 220);
                }
            }
            for (var j = 0; j < (reasons || []).length; j++) {
                if (/fail|black screen|hang|bsod|boot|crash|loss|error|warning|corrupt|damage/i.test(reasons[j])) {
                    return trimToCompleteSentence(cleanCmfEvidenceSnippet(reasons[j]), 220);
                }
            }
            return 'Not identified from available evidence.';
        }

        function normalizeCmfDimensionScores(ruleScores) {
            var defaults = [
                { RuleId: 'S', RuleName: 'Severity', Score: '0', Evaluation: 'Missing symptom severity evidence.' },
                { RuleId: 'O', RuleName: 'Occurrence', Score: '0', Evaluation: 'Missing occurrence or reproduction frequency.' },
                { RuleId: 'D', RuleName: 'Detection', Score: '0', Evaluation: 'Missing customer encounter or usage visibility.' },
                { RuleId: 'R', RuleName: 'Recovery', Score: '0', Evaluation: 'Missing workaround or recovery evidence.' },
                { RuleId: 'B', RuleName: 'Business Impact', Score: '0', Evaluation: 'Missing milestone or business exposure evidence.' },
                { RuleId: 'CX', RuleName: 'Customer Experience', Score: '0', Evaluation: 'Missing customer-experience evidence.' }
            ];
            var map = {};
            for (var i = 0; i < ruleScores.length; i++) {
                var rule = ruleScores[i] || {};
                var rawKey = String(rule.RuleId || '').toUpperCase();
                var key = rawKey === 'CX' ? 'CX' : rawKey.charAt(0);
                if (!map[key]) map[key] = rule;
            }
            for (var j = 0; j < defaults.length; j++) {
                var existing = map[defaults[j].RuleId];
                if (existing) defaults[j] = existing;
            }
            return defaults;
        }

        function renderCmfDimensionCards(rules) {
            var html = '<div class="cmf-rec-dimension-grid">';
            for (var i = 0; i < rules.length; i++) {
                var rule = rules[i] || {};
                var score = normalizeDimensionScore(rule.Score || '1');
                html += '<div class="cmf-rec-dimension-card"><div class="cmf-rec-dimension-head">';
                html += '<span class="cmf-rec-dimension-name">' + escapeHtml(rule.RuleId || '') + ' ' + escapeHtml(rule.RuleName || 'Score') + '</span>';
                html += '<span class="cmf-rec-dimension-score">' + escapeHtml(score) + '/5</span></div>';
                html += '<div class="cmf-rec-dimension-eval">' + escapeHtml(buildConciseCmfRuleEvaluation(rule, parseInt(score, 10))) + '</div></div>';
            }
            return html + '</div>';
        }

        function renderCmfMetricScoreTable(rules) {
            var html = '<div class="cmf-rec-metric-table-wrap"><table class="cmf-rec-metric-table"><thead><tr>';
            html += '<th>Metric Name</th><th>Score</th><th>Contributing Factor</th></tr></thead><tbody>';
            for (var i = 0; i < rules.length; i++) {
                var rule = rules[i] || {};
                var score = normalizeDimensionScore(rule.Score || '1');
                html += '<tr>';
                html += '<td><strong>' + escapeHtml(buildCmfMetricDisplayName(rule)) + '</strong></td>';
                html += '<td><span class="cmf-rec-metric-score">' + escapeHtml(score) + '/5</span></td>';
                html += '<td>' + escapeHtml(extractCmfContributingFactor(rule.Evaluation || '')) + '</td>';
                html += '</tr>';
            }
            return html + '</tbody></table></div>';
        }

        function renderCmfAssistantRecommendation(result, payload) {
            result = result || {};
            payload = payload || {};
            var recommendation = normalizeCmfRecommendationLabel(result.Recommendation || 'CMF_REVIEW');
            var rules = normalizeCmfDimensionScores(result.RuleScores || []);
            var score = calculateDisplayedCmfScore(rules);
            if (!score) score = normalizeWeightedCmfScore(result.OverallQualityScore || 0);
            var scoreDisplay = formatWeightedCmfScore(score) || '0.0';
            var scorePercent = Math.max(0, Math.min(100, (score / 5) * 100));
            var reasons = buildCmfAssistantRecommendationReasoning(recommendation, score, rules, result.Evidence || '', payload);
            var html = '<div class="cmf-assistant-rec-shell">';
            html += '<div class="cmf-assistant-rec-top">';
            html += '<div class="cmf-assistant-overall-score"><span>Overall Score</span><div class="cmf-assistant-score-ring" style="--score-percent:' + escapeHtml(scorePercent.toFixed(2)) + '"><div class="cmf-assistant-score-ring-value">' + escapeHtml(scoreDisplay) + '</div></div><strong>/5</strong></div>';
            html += renderCmfMetricChips(rules);
            html += '</div>';
            html += '<div class="cmf-assistant-reasoning-box"><h4>Reasoning</h4>' + renderCmfReasoningBullets(reasons) + '</div>';
            html += '</div>';
            return html;
        }

        function renderCmfReasoningBullets(items) {
            var html = '<div class="cmf-assistant-reasoning-copy"><ul>';
            var list = (items || []).filter(function (item) { return item && String(item).trim().length; });
            if (!list.length) list = ['No reasoning returned; review the score details and issue evidence.'];
            for (var i = 0; i < list.length; i++) {
                html += '<li>' + escapeHtml(list[i]) + '</li>';
            }
            return html + '</ul></div>';
        }

        function renderCmfMetricChips(rules) {
            var html = '<div class="cmf-assistant-metric-grid" aria-label="Individual metric scores">';
            for (var i = 0; i < rules.length; i++) {
                var rule = rules[i] || {};
                var score = parseInt(normalizeDimensionScore(rule.Score || '1'), 10);
                html += '<div class="cmf-assistant-metric-chip ' + escapeHtml(getCmfMetricScoreClass(score)) + '">';
                html += '<div class="cmf-assistant-metric-main"><span class="cmf-assistant-metric-name">' + escapeHtml(buildCmfMetricDisplayName(rule)) + '</span><span class="cmf-assistant-metric-value">' + escapeHtml(score) + '/5</span></div>';
                html += '<div class="cmf-assistant-metric-factor">' + escapeHtml(extractBriefCmfContributingFactor(rule)) + '</div>';
                html += '</div>';
            }
            return html + '</div>';
        }

        function calculateDisplayedCmfScore(rules) {
            var weights = { S: 0.25, O: 0.15, D: 0.20, R: 0.10, B: 0.15, CX: 0.15 };
            var total = 0;
            var hasMetric = false;
            for (var i = 0; i < (rules || []).length; i++) {
                var rule = rules[i] || {};
                var id = String(rule.RuleId || '').toUpperCase();
                if (!weights.hasOwnProperty(id)) continue;
                var score = parseInt(normalizeDimensionScore(rule.Score || '1'), 10);
                total += score * weights[id];
                hasMetric = true;
            }
            return hasMetric ? total : 0;
        }

        function buildCmfAssistantRecommendationReasoning(recommendation, score, rules, evidenceText, payload) {
            var rec = normalizeCmfRecommendationLabel(recommendation || 'CMF_REVIEW');
            var modelReasoning = buildCmfModelReasoningBullets(evidenceText, payload);
            if (modelReasoning.length) return modelReasoning;
            var scored = [];
            var factorMap = {};
            for (var i = 0; i < (rules || []).length; i++) {
                var rule = rules[i] || {};
                var ruleScore = parseInt(normalizeDimensionScore(rule.Score || '1'), 10);
                var id = String(rule.RuleId || '').toUpperCase();
                factorMap[String(rule.RuleId || '').toUpperCase()] = extractBriefCmfContributingFactor(rule);
                scored.push({ id: id || String(rule.RuleName || 'Metric'), score: ruleScore });
            }

            var issueFact = buildCmfReasoningIssueBullet(evidenceText, payload, factorMap);
            var impactFact = buildCmfReasoningImpactBullet(evidenceText, payload, scored);
            var recoveryFact = buildCmfReasoningRecoveryBullet(factorMap, scored);
            var scoreJudgment = buildCmfReasoningScoreJudgment(rec, score, scored);
            var result = [];
            if (rec === 'CMF_OK') {
                result.push(issueFact);
                result.push(impactFact);
                result.push(recoveryFact + ' ' + scoreJudgment);
            } else if (rec === 'CMF_REVIEW') {
                result.push(issueFact);
                result.push(impactFact);
                result.push(scoreJudgment + ' Confirm the weaker evidence before approving or rejecting the tag.');
            } else if (rec === 'CMF_INCOMPLETE') {
                result.push(issueFact);
                result.push(impactFact);
                result.push(scoreJudgment + ' Add missing impact, repro, recovery, or customer evidence before final disposition.');
            } else {
                result.push(issueFact);
                result.push(impactFact);
                result.push(scoreJudgment + ' Current evidence supports CMF_REJECT unless stronger customer-impact or business-gating proof is added.');
            }
            return limitCmfReasoningWords(result, 140);
        }

        function buildCmfModelReasoningBullets(evidenceText, payload) {
            var text = extractCmfReasoningText(evidenceText || '');
            if (!text) return [];
            var bullets = splitCmfReasoningItems(text).map(function (item) {
                return cleanCmfReasoningSentence(item, payload);
            }).filter(isUsefulCmfReasoningBullet);
            if (!bullets.length) {
                bullets = (text.match(/[^.!?]+[.!?]+|[^.!?]+$/g) || [text]).map(function (item) {
                    return cleanCmfReasoningSentence(item, payload);
                }).filter(isUsefulCmfReasoningBullet);
            }
            return limitCmfReasoningWords(bullets.slice(0, 4), 180);
        }

        function splitCmfReasoningItems(text) {
            var normalized = String(text || '').replace(/\r/g, '\n').trim();
            var raw = normalized.split(/\n+|(?:^|\s)[-•]\s+/).filter(function (item) { return item && item.trim(); });
            if (raw.length <= 1) {
                raw = normalized.split(/;\s+(?=[A-Z])/).filter(function (item) { return item && item.trim(); });
            }
            return raw;
        }

        function extractCmfReasoningText(value) {
            var text = String(value || '').replace(/\r/g, '\n').trim();
            if (!text) return '';
            var match = text.match(/(?:^|\n)\s*(?:REASONING|AI\s+REASONING)\s*:\s*([\s\S]*?)(?=\n\s*(?:ISSUE\s+IMPACT|RULE\s+SCORES|MISSING\s+INFO|NEXT\s+STEPS)\s*:|$)/i);
            if (match && match[1]) text = match[1];
            text = text.replace(/^\s*(?:REASONING|AI\s+REASONING)\s*:\s*/i, '').trim();
            return text;
        }

        function cleanCmfReasoningSentence(value, payload) {
            var text = cleanCmfEvidenceSnippet(value || '');
            text = text.replace(/^[-•\d.)\s]+/g, '').trim();
            text = text.replace(/\b(?:request(?:ed)?\s+the\s+)?fix\s+by\s+WW\d+\b/gi, 'customer milestone risk');
            text = text.replace(/"Reproducibility\s*:[^"]+"/gi, 'consistent reproduction evidence');
            text = text.replace(/"System stuck in PC2 during MS stress test will cause a high power consumption"/gi, 'a Modern Standby power-state failure');
            text = text.replace(/\bReproducibility\s*:\s*\S+/gi, 'consistent reproduction evidence');
            text = text.replace(/with measurable PHM traces and a reproduced fail pattern\s*:/gi, 'with diagnostic traces and repeated reproduction evidence');
            text = text.replace(/with diagnostic traces and repeated reproduction evidence\s+a Modern Standby power-state failure\s+and\s+consistent reproduction evidence/gi, 'with diagnostic traces and repeated reproduction evidence');
            text = text.replace(/\bWW\d+\b/g, '').replace(/;\s*/g, '. ').replace(/\s{2,}/g, ' ').trim();
            if (isCmfReasoningRepeatingDisplayedDetails(text, payload)) return '';
            text = removeAbruptCmfTail(trimToCompleteSentence(text, 260));
            return ensureCmfSentence(text);
        }

        function isUsefulCmfReasoningBullet(item) {
            var text = String(item || '').trim();
            if (!text) return false;
            if (/^not identified from available evidence[.!?]?$/i.test(text)) return false;
            return true;
        }

        function isCmfReasoningRepeatingDisplayedDetails(text, payload) {
            payload = payload || {};
            var lower = String(text || '').toLowerCase();
            var title = String(payload.title || '').toLowerCase();
            var component = String(payload.component || '').toLowerCase();
            var platform = getPlatformDisplayLabel(payload.platform || getIssuePendingPlatformValue()).toLowerCase();
            var repro = String(payload.reproducibility || payload.reproOnRvp || '').toLowerCase();
            if (title && lower.length > 40 && (title.indexOf(lower) >= 0 || lower.indexOf(title.substring(0, Math.min(70, title.length))) >= 0)) return true;
            if (component && lower === component) return true;
            if (platform && lower === platform) return true;
            if (repro && lower === repro) return true;
            return false;
        }

        function buildCmfReasoningIssueBullet(evidenceText, payload, factorMap) {
            payload = payload || {};
            factorMap = factorMap || {};
            var customer = extractSqaCustomer(payload.customerDetail || '').replace(/^N\/A$/i, '').trim();
            var platform = getPlatformDisplayLabel(payload.platform || getIssuePendingPlatformValue());
            var component = formatCmfReasoningComponent(firstUsefulText(payload.component, 'the affected component'));
            var titleSignal = summarizeCmfTitleSignal(payload.title || payload.cmfRequest || 'Issue evidence is limited');
            var reproSignal = firstUsefulText(payload.reproducibility || payload.reproOnRvp, '').replace(/[.!?]$/g, '');
            var ownerText = customer ? ' for ' + customer : '';
            var reproText = reproSignal ? ', with repro evidence: ' + reproSignal : '';
            return ensureCmfSentence(trimToCompleteSentence('The sighting describes ' + titleSignal + ownerText + ' on ' + platform + '/' + component + reproText, 185));
        }

        function buildCmfReasoningImpactBullet(evidenceText, payload, scored) {
            var cxScore = getCmfReasoningScore(scored, 'CX');
            var businessScore = getCmfReasoningScore(scored, 'B');
            var detectionScore = getCmfReasoningScore(scored, 'D');
            if (cxScore >= 4) return 'Customer experience impact is strong because the symptom affects a visible workflow rather than only internal debug tracking.';
            if (businessScore >= 4) return 'Business impact is material because the available evidence points to milestone, validation, or customer-delivery risk.';
            if (detectionScore >= 4) return 'The detection signal is meaningful because the failure appears in a normal or frequently used workflow.';
            return 'Customer impact is not yet strong enough in the available evidence, so reviewer judgment is still needed.';
        }

        function buildCmfReasoningRecoveryBullet(factorMap, scored) {
            factorMap = factorMap || {};
            var recoveryScore = getCmfReasoningScore(scored, 'R');
            if (recoveryScore >= 4) return 'Recovery evidence indicates the issue is difficult to mitigate, increasing CMF concern.';
            if (recoveryScore >= 2) return 'Any available workaround reduces immediate risk but does not remove the underlying issue.';
            return 'Recovery/workaround evidence is limited, which lowers confidence in final disposition.';
        }

        function summarizeCmfTitleSignal(value) {
            var text = cleanCmfEvidenceSnippet(value || '');
            text = text.replace(/^(?:\[[^\]]+\]\s*)+/, '').trim();
            return trimToCompleteSentence(text || 'Issue evidence is limited', 90).replace(/[.!?]$/g, '');
        }

        function formatCmfReasoningComponent(value) {
            var text = cleanCmfEvidenceSnippet(value || '');
            if (text.indexOf('.') >= 0) {
                var parts = text.split('.').filter(function (part) { return part && part.trim(); });
                return parts.length ? parts[parts.length - 1].trim() : text;
            }
            return text;
        }

        function buildCmfReasoningScoreJudgment(recommendation, score, scored) {
            var strengthText = describeCmfReasoningStrengths(scored);
            if (recommendation === 'CMF_OK') return 'Together, ' + strengthText + ' place the weighted result above the CMF_OK threshold.';
            if (recommendation === 'CMF_REVIEW') return 'Together, ' + strengthText + ' indicate the sighting is important but still borderline, so CMF_REVIEW is the right disposition.';
            if (recommendation === 'CMF_INCOMPLETE') return 'The evaluated signals are not complete enough to support a final CMF decision.';
            return 'The evaluated signals do not clear the CMF tagging threshold.';
        }

        function describeCmfReasoningStrengths(scored) {
            var phrases = [];
            if (getCmfReasoningScore(scored, 'O') >= 4) phrases.push('high reproducibility');
            if (getCmfReasoningScore(scored, 'D') >= 4) phrases.push('clear detection evidence');
            if (getCmfReasoningScore(scored, 'CX') >= 4) phrases.push('customer-experience degradation');
            if (getCmfReasoningScore(scored, 'B') >= 4) phrases.push('business exposure');
            if (getCmfReasoningScore(scored, 'S') >= 4) phrases.push('severity');
            if (phrases.length >= 2) return phrases.slice(0, 3).join(', ');
            if (phrases.length === 1) return phrases[0] + ' with remaining evidence gaps';
            return 'the available metric evidence and remaining gaps';
        }

        function getCmfReasoningScore(scored, id) {
            id = String(id || '').toUpperCase();
            for (var i = 0; i < (scored || []).length; i++) {
                if (String((scored[i] || {}).id || '').toUpperCase() === id) return scored[i].score || 0;
            }
            return 0;
        }

        function limitCmfReasoningWords(items, maxWords) {
            var remaining = maxWords || 80;
            var result = [];
            for (var i = 0; i < (items || []).length; i++) {
                var text = finalizeCmfReasoningBullet(items[i]);
                var words = text.split(/\s+/).filter(Boolean);
                if (!words.length || remaining <= 0) continue;
                if (words.length > remaining) {
                    text = trimToCompleteSentence(text, Math.max(90, remaining * 9));
                    text = finalizeCmfReasoningBullet(text);
                    words = text.split(/\s+/).filter(Boolean);
                    if (words.length > remaining) break;
                }
                result.push(text);
                remaining -= words.length;
            }
            return result;
        }

        function finalizeCmfReasoningBullet(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!text) return '';
            text = text.replace(/confidence in a single\s*[.!?]?$/i, 'confidence in a single recovery path.');
            text = text.replace(/with consistent repro\s*[.!?]?$/i, 'with consistent reproduction evidence before approval.');
            text = text.replace(/and consistent repro\s*[.!?]?$/i, 'and consistent reproduction evidence before approval.');
            var parts = text.match(/[^.!?]+[.!?]+/g);
            if (parts && parts.length) text = parts.join(' ').trim();
            text = removeAbruptCmfTail(text);
            if (/\b(single|consistent|repro|triage|owner|ownership|classification|decision|evidence|root cause)\.$/i.test(text) && parts && parts.length > 1) {
                text = parts.slice(0, -1).join(' ').trim();
            }
            return ensureCmfSentence(text);
        }

        function ensureCmfSentence(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!text) return '';
            return /[.!?]$/.test(text) ? text : text + '.';
        }

        function lowerFirstCmfPhrase(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!text || /^[A-Z]{2,}\b/.test(text)) return text;
            return text.charAt(0).toLowerCase() + text.slice(1);
        }

        function getCmfMetricScoreClass(score) {
            if (score >= 4) return 'is-high';
            if (score >= 3) return 'is-medium';
            if (score >= 1) return 'is-low';
            return 'is-empty';
        }

        function updateCmfAssistantTag(recommendation) {
            var node = document.getElementById('cmfAssistantRecTag');
            if (!node) return;
            var rec = normalizeCmfRecommendationLabel(recommendation || 'CMF_REVIEW');
            var className = 'cmf-assistant-rec-tag';
            if (rec === 'CMF_OK') className += ' is-ok';
            else if (rec === 'CMF_REVIEW') className += ' is-review';
            else if (rec === 'CMF_INCOMPLETE') className += ' is-incomplete';
            else if (rec === 'CMF_REJECT') className += ' is-reject';
            node.className = className;
            node.textContent = rec;
        }

        function renderCmfRecommendationSkeleton() {
            return '<div class="ai-skeleton-loader"><div class="ai-skeleton-line ai-skeleton-title" style="width:45%;"></div><div class="ai-skeleton-line" style="width:92%;"></div><div class="ai-skeleton-line" style="width:86%;"></div></div>';
        }

        function renderCmfMetricScoreSkeleton() {
            return '<div class="ai-skeleton-loader"><div style="font-size:13px;font-weight:600;color:#5c7087;">Calculating individual metric scores...</div><div class="ai-skeleton-line ai-skeleton-title" style="width:45%;"></div><div class="ai-skeleton-line" style="width:92%;"></div><div class="ai-skeleton-line" style="width:86%;"></div></div>';
        }

        function buildCmfMetricDisplayName(rule) {
            var id = String((rule || {}).RuleId || '').toUpperCase();
            var name = String((rule || {}).RuleName || '').trim();
            if (!name) name = id === 'CX' ? 'Customer Experience' : 'Metric';
            return id ? name + ' (' + id + ')' : name;
        }

        function extractCmfContributingFactor(evaluation) {
            var text = cleanCmfEvidenceSnippet(evaluation || '');
            var match = text.match(/contributing factor\s*:\s*([\s\S]*?)(?:\.\s*Evidence\s*:|\.\s|$)/i);
            if (match && match[1]) return trimToCompleteSentence(match[1], 260);
            return trimToCompleteSentence(text.replace(/^SCORE\s*\d\s*[-:]?\s*/i, ''), 260);
        }

        function extractBriefCmfContributingFactor(rule) {
            var id = String((rule || {}).RuleId || '').toUpperCase();
            var text = extractCmfContributingFactor((rule || {}).Evaluation || '');
            var lower = text.toLowerCase();
            if (!text || /missing evidence|missing .* evidence|not identified/i.test(text)) return getCmfMissingEvidenceFactorLabel(id);
            if (isConcreteCmfFactor(text)) return formatBriefCmfFactor(id, text);
            if (id === 'S') {
                if (/gpgpu|burnin|gpu/i.test(lower)) return 'GPGPU BurnIn failure';
                if (/bsod|black screen|hang|crash|boot/i.test(lower)) return 'Major system failure';
                if (/performance|degradation/i.test(lower)) return 'Performance degradation';
                return 'Functional impact evidence';
            }
            if (id === 'O') {
                if (/very_sporadic|sporadic|6\/90/i.test(lower)) return 'Sporadic repro rate';
                if (/frequent|hourly|1\/10/i.test(lower)) return 'Frequent occurrence';
                if (/rare|1\/1000/i.test(lower)) return 'Rare occurrence';
                return 'Repro frequency';
            }
            if (id === 'D') {
                if (/burnin|stress/i.test(lower)) return 'Stress workflow detection';
                if (/daily|login|restart/i.test(lower)) return 'Common workflow detection';
                return 'Usage trigger evidence';
            }
            if (id === 'R') {
                if (/revert|rollback|pcode|dcode/i.test(lower)) return 'Firmware rollback workaround';
                if (/reboot|restart|reconnect|reflash/i.test(lower)) return 'Manual recovery needed';
                return 'Workaround clarity';
            }
            if (id === 'B') {
                if (/blocked|blocks|validation|performance test/i.test(lower)) return 'Validation blocked';
                if (/launch|milestone|gate/i.test(lower)) return 'Milestone risk';
                return 'Business exposure';
            }
            if (id === 'CX') {
                if (/display|audio|camera|network|touch|unusable/i.test(lower)) return 'Visible feature impact';
                if (/missing/i.test(lower)) return 'Missing CX evidence';
                return 'Customer visibility';
            }
            return trimToWordCount(text, 6);
        }

        function getCmfMissingEvidenceFactorLabel(id) {
            if (id === 'S') return 'Low - Negligible Functional Impact';
            if (id === 'O') return 'Low - Almost Never or Cannot Reliably Reproduce';
            if (id === 'D') return 'Low - Special Debug, Lab, or Manufacturing Setup';
            if (id === 'R') return 'Low - Fully Effective Workaround';
            if (id === 'B') return 'Low - Internal Only with No Business Impact';
            if (id === 'CX') return 'Low - Not Visible to End User';
            return 'Missing evidence';
        }

        function formatBriefCmfFactor(id, value) {
            var text = String(value || '').replace(/&nbsp;/gi, ' ').replace(/\s+/g, ' ').trim();
            text = text.replace(/^"+|"+$/g, '').replace(/^>+/, '').trim();
            var lower = text.toLowerCase();
            if (/^(low|medium|moderate|high|frequent|very frequent|very high|critical)\s+-\s+/i.test(text)) return text;
            if (id === 'O') {
                var repro = text.match(/(?:frequently|sporadic|rare|always|occasionally)[\w_\-]*\s*\d*%?(?:-\d+%)?|\d+\s*%\s*-\s*\d+\s*%|\d+\/\d+/i);
                if (repro) return repro[0].replace(/\s+/g, '');
            }
            if (id === 'R') {
                if (/uma/i.test(text)) return 'UMA BIOS workaround';
                if (/rollback|roll back|revert/i.test(text)) return 'Driver rollback workaround';
                if (/reboot|restart/i.test(text)) return 'Restart recovery';
            }
            if (id === 'D') {
                if (/modern standby|s0ix|idle/i.test(lower)) return 'Modern Standby / idle path';
                if (/stress/i.test(lower)) return 'Stress test path';
            }
            if (id === 'S' || id === 'B') {
                if (/fvt exit/i.test(lower)) return 'FVT exit gating impact';
                if (/high power/i.test(lower)) return 'High power in PC2';
            }
            if (id === 'CX' && /missing/i.test(lower)) return 'Missing evidence';
            return trimToWordCount(text, 8);
        }

        function isConcreteCmfFactor(value) {
            var text = String(value || '').trim();
            if (!text) return false;
            if (/[:=]|\d|%|_|\b(WW|SKU|S0ix|S0i|S3|S4|S5|PC\d|BSOD|PHM|GFX|BIOS|driver|display|repro|stress|standby|resume|hang|timeout)\b/i.test(text)) return true;
            return text.split(/\s+/).filter(Boolean).length > 3;
        }

        function trimToWordCount(value, maxWords) {
            var words = String(value || '').replace(/[.!?]+$/g, '').split(/\s+/).filter(Boolean);
            return words.slice(0, maxWords || 6).join(' ') || 'Missing evidence';
        }

        function buildConciseCmfRuleEvaluation(rule, score) {
            var ruleName = String((rule || {}).RuleName || (rule || {}).RuleId || 'Criterion');
            var evidence = sanitizeCmfDisplaySentence((rule || {}).Evaluation || '', 240);
            evidence = evidence.replace(/^SCORE\s*\d\s*[-:]?\s*/i, '').replace(/^(?:CX|[SODRB])\d\s*[-:]?\s*/i, '').trim();
            var meaning = score >= 4 ? 'strong support' : (score >= 3 ? 'partial support' : 'missing or weak support');
            return ruleName + ' shows ' + meaning + ': ' + evidence;
        }

        function normalizeDimensionScore(value) {
            var raw = String(value || '').replace('/100', '').replace('/5', '').replace('%', '').trim();
            var num = parseFloat(raw);
            if (isNaN(num)) return '1';
            if (num > 5) num = Math.round(num / 20);
            num = Math.max(1, Math.min(5, Math.round(num)));
            return String(num);
        }

        function normalizeWeightedCmfScore(value) {
            var num = parseFloat(String(value || '').replace('/100', '').replace('/5', '').replace('%', '').trim());
            if (isNaN(num)) return 0;
            if (num > 5) num = num / 20;
            return Math.max(0, Math.min(5, num));
        }

        function formatWeightedCmfScore(value) {
            var score = normalizeWeightedCmfScore(value);
            if (!score) return '';
            return score.toFixed(2).replace(/\.00$/, '.0');
        }

        function extractCmfReasonBullets(text) {
            var raw = String(text || '').replace(/\r/g, '\n');
            var reasonMatch = raw.match(/(?:^|\n)\s*(?:REASONING|AI REASONING)\s*:\s*([\s\S]*?)(?=(?:\n\s*ISSUE IMPACT\s*:|\n\s*RULE SCORES\s*:|$))/i);
            var source = reasonMatch && reasonMatch[1] ? reasonMatch[1] : raw;
            var items = [];
            var current = '';
            var lines = source.split('\n');
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i].replace(/^\s+|\s+$/g, '');
                if (!line || /^issue impact\s*:/i.test(line)) continue;
                var bulletMatch = line.match(/^(?:[-*•]\s*|\d+[\.)]\s*)(.+)$/);
                if (bulletMatch) {
                    if (current) items.push(current);
                    current = bulletMatch[1].trim();
                } else {
                    current = current ? current + ' ' + line : line;
                }
            }
            if (current) items.push(current);
            items = items.map(function (item) {
                return sanitizeCmfDisplaySentence(item, 320);
            }).filter(function (item) { return item.length > 0 && !/^issue impact\s*:/i.test(item); }).slice(0, 4);
            return items.length ? items : ['No reasoning returned; review the score details and issue evidence.'];
        }

        function buildCmfRecommendationReasoning(recommendation, score, rules, evidenceText, payload) {
            var rec = String(recommendation || '').toUpperCase();
            var strengths = [];
            var gaps = [];
            var conflicts = /conflict|conflicting|borderline/i.test(evidenceText || '');

            for (var i = 0; i < (rules || []).length; i++) {
                var rule = rules[i] || {};
                var ruleName = String(rule.RuleName || rule.RuleId || 'criterion').toLowerCase();
                var ruleScore = parseInt(normalizeDimensionScore(rule.Score || '1'), 10);
                var evaluation = sanitizeCmfDisplaySentence(rule.Evaluation || '', 240);
                if (ruleScore >= 4) {
                    strengths.push(ruleName + ' is strong: ' + evaluation);
                } else if (ruleScore <= 2) {
                    gaps.push(ruleName + ' is weak: ' + evaluation);
                } else {
                    gaps.push(ruleName + ' needs reviewer judgment: ' + evaluation);
                }
            }

            var issueFact = extractCmfSymptomEvidence(rules, []) || firstUsefulText(payload.title, 'The issue has limited symptom detail.');
            var scoreText = formatWeightedCmfScore(score) || '0';
            var reasoning = [];
            reasoning.push('The recommendation is based on the failure evidence and the weighted S/O/D/R/B/CX score, not just the row fields: ' + sanitizeCmfDisplaySentence(issueFact, 260));

            if (rec === 'CMF_OK') {
                reasoning.push('CMF_OK is supported because the weighted evidence score is ' + scoreText + '/5 and the strongest criteria are ' + summarizeCmfReasonList(strengths, 'severity and business impact') + '.');
                reasoning.push(gaps.length ? 'Before approval, the reviewer should still confirm ' + summarizeCmfReasonList(gaps, 'any lower-scored criteria') + '.' : 'No decision-critical scoring gaps are visible from the supplied evidence.');
            } else if (rec === 'CMF_REVIEW') {
                reasoning.push('CMF_REVIEW is appropriate because the score is near or above the decision band but at least one criterion still needs judgment: ' + summarizeCmfReasonList(gaps, 'the borderline criteria') + '.');
                reasoning.push(conflicts ? 'The explanation flags conflicting evidence, so the reviewer should resolve that conflict before approving or rejecting the CMF tag.' : 'The reviewer should confirm whether the weaker criteria are real gaps or simply missing details in the current record.');
            } else if (rec === 'CMF_INCOMPLETE') {
                reasoning.push('CMF_INCOMPLETE is appropriate because the current record does not provide enough decision-critical evidence across ' + summarizeCmfReasonList(gaps, 'the required criteria') + '.');
                reasoning.push('The next action is to update the missing evidence, then rerun the recommendation so the score reflects the actual customer risk.');
            } else {
                reasoning.push('CMF_REJECT is supported only if the weak criteria remain low after review: ' + summarizeCmfReasonList(gaps, 'the low-scored criteria') + '.');
                reasoning.push('The reviewer should confirm that no customer, launch, recovery, or business-impact evidence is missing before rejecting the CMF request.');
            }
            return reasoning.slice(0, 4);
        }

        function summarizeCmfReasonList(items, fallback) {
            var clean = (items || []).filter(function (item) { return item && item.length; }).slice(0, 2);
            return clean.length ? clean.join('; ') : fallback;
        }

        function extractCmfImpactSentence(text, payload) {
            var raw = String(text || '').replace(/\r/g, '\n');
            var match = raw.match(/(?:^|\n)\s*ISSUE IMPACT\s*:\s*([\s\S]*?)(?=(?:\n\s*RULE SCORES\s*:|$))/i);
            if (match && match[1]) return firstCompleteSentence(match[1], 360);
            return firstUsefulText(payload.impact, 'Customer/user impact and urgency are not sufficiently described.');
        }

        function extractCmfMissingInformation(text, rules, payload) {
            var missing = [];
            if (!firstUsefulText(payload.title, '')) missing.push('symptom / failure mode');
            if (!firstUsefulText(payload.impact, '')) missing.push('customer impact');
            if (!firstUsefulText(payload.reproducibility || payload.reproOnRvp, '')) missing.push('occurrence / repro evidence');
            if (!firstUsefulText(payload.customerDetail, '')) missing.push('usage trigger / customer scenario');
            if (/missing workaround|missing recovery|workaround or recovery impact is not described/i.test(text)) missing.push('workaround / recovery');
            if (/business exposure is not established|missing milestone|customer or business exposure is not established/i.test(text)) missing.push('milestone / business impact');
            for (var i = 0; i < rules.length; i++) {
                var evalText = String((rules[i] || {}).Evaluation || '');
                var ruleScore = parseInt(normalizeDimensionScore((rules[i] || {}).Score || '1'), 10);
                if (/^FAIL|missing|not found|not described/i.test(evalText) || ruleScore <= 2) {
                    var name = buildMissingInfoFromRule(rules[i]);
                    if (missing.indexOf(name) < 0) missing.push(name);
                }
            }
            return missing.filter(function (item, index) { return item && missing.indexOf(item) === index; }).slice(0, 6);
        }

        function buildMissingInfoFromRule(rule) {
            var id = String((rule || {}).RuleId || '').toUpperCase();
            if (id === 'S') return 'clear symptom severity and customer-visible failure impact';
            if (id === 'O') return 'occurrence rate, cycle count, or reproducibility proof';
            if (id === 'D') return 'usage trigger showing when a customer would hit the issue';
            if (id === 'R') return 'workaround or recovery status and whether it is acceptable';
            if (id === 'B') return 'milestone, launch, escalation, or business-impact evidence';
            if (id === 'CX') return 'customer-experience visibility and feature degradation evidence';
            return ((rule || {}).RuleName || 'rule evidence').toLowerCase();
        }

        function renderCmfMissingList(items) {
            if (!items || !items.length) return '<ul class="cmf-rec-missing-list"><li class="is-empty">No decision-critical gaps identified from available evidence.</li></ul>';
            return renderCmfBriefList(items);
        }

        function buildCmfReviewerActions(recommendation, missing, rules, impact) {
            var actions = [];
            var rec = String(recommendation || '').toUpperCase();
            if (missing && missing.length) {
                actions.push('Ask the owner to update ' + missing.slice(0, 2).join(' and ') + ' before final CMF approval.');
            }
            if (rec === 'CMF_OK') {
                actions.push('Confirm the high S/O/D/R/B/CX evidence and approve only if the customer impact remains valid.');
            } else if (rec === 'CMF_REVIEW') {
                actions.push('Review the borderline or conflicting score evidence with the iDST owner before changing status.');
            } else if (rec === 'CMF_REJECT') {
                actions.push('Reject only if the current evidence still shows low CMF-level risk after checking customer impact.');
            } else {
                actions.push('Keep this as incomplete until the missing decision-critical evidence is added.');
            }
            var weakest = findWeakestCmfRule(rules);
            if (weakest) actions.push('Focus review on ' + weakest.RuleName.toLowerCase() + ', currently scored ' + normalizeDimensionScore(weakest.Score) + '/5.');
            if (impact && !/^missing|customer\/user impact/i.test(impact)) actions.push('Use the stated impact as the reviewer rationale if the disposition is confirmed.');
            return actions.slice(0, 3);
        }

        function findWeakestCmfRule(rules) {
            var weakest = null;
            for (var i = 0; i < (rules || []).length; i++) {
                var score = parseInt(normalizeDimensionScore(rules[i].Score || '1'), 10);
                if (!weakest || score < weakest._score) {
                    weakest = rules[i];
                    weakest._score = score;
                }
            }
            return weakest;
        }

        function firstUsefulText(value, fallback) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            if (!text || /^n\/?a$|^none$|^unknown$|^missing$/i.test(text)) return fallback || '';
            return cleanCmfEvidenceSnippet(trimToCompleteSentence(text, 260));
        }

        function findRecoverySignal(text, payload, rules) {
            var recoveryRule = findCmfRuleById(rules, 'R');
            if (recoveryRule && recoveryRule.Evaluation && !/missing|not described|no recovery/i.test(recoveryRule.Evaluation)) {
                return sanitizeCmfDisplaySentence(recoveryRule.Evaluation, 220);
            }
            var combined = String(text || '') + ' ' + String(payload.impact || '');
            var match = combined.match(/(?:no workaround|workaround[^.]*|reboot[^.]*|restart[^.]*|reflash[^.]*|reconnect[^.]*|recovery[^.]*)/i);
            return match ? sanitizeCmfDisplaySentence(match[0], 220) : 'Missing workaround or recovery detail.';
        }

        function findBusinessSignal(text, payload, rules) {
            var businessRule = findCmfRuleById(rules, 'B');
            if (businessRule && businessRule.Evaluation && !/missing|not established|not described/i.test(businessRule.Evaluation)) {
                return sanitizeCmfDisplaySentence(businessRule.Evaluation, 220);
            }
            var impact = firstUsefulText(payload.impact, '');
            if (/gate|gating|validation|milestone|launch|customer|ww\d+/i.test(impact)) return impact;
            var request = firstUsefulText(payload.cmfRequest, '');
            if (/gate|gating|validation|milestone|launch|customer|cmf/i.test(request)) return request;
            var combined = String(text || '') + ' ' + String(payload.impact || '') + ' ' + String(payload.cmfRequest || '');
            var match = combined.match(/(?:launch[^.]*|validation[^.]*|milestone[^.]*|escalation[^.]*|financial[^.]*|warranty[^.]*|returns[^.]*|customer[^.]*)/i);
            return match ? sanitizeCmfDisplaySentence(match[0], 220) : 'Missing milestone or business exposure detail.';
        }

        function findCmfRuleById(rules, ruleId) {
            for (var i = 0; i < (rules || []).length; i++) {
                if (String((rules[i] || {}).RuleId || '').toUpperCase() === ruleId) return rules[i];
            }
            return null;
        }

        function sanitizeCmfDisplaySentence(value, maxLength) {
            var text = cleanCmfEvidenceSnippet(value);
            text = removeAbruptCmfTail(text);
            if (!/[.!?]$/.test(text)) text += '.';
            return removeAbruptCmfTail(trimToCompleteSentence(text, maxLength || 240));
        }

        function removeAbruptCmfTail(value) {
            var text = String(value || '').trim();
            text = text.replace(/[,:;\-\s]+$/g, '').trim();
            text = text.replace(/\b(indicating|including|because|with|and|or|that|the|a|an|to|of|for|by|as|which|rather than)\s*[.!?]?$/i, '').trim();
            text = text.replace(/[,:;\-\s]+$/g, '').trim();
            if (text && !/[.!?]$/.test(text)) text += '.';
            return text || 'Not identified from available evidence.';
        }

        function cleanCmfEvidenceSnippet(value) {
            var text = String(value || '').replace(/\s+/g, ' ').trim();
            text = text.replace(/\bneeds confirm\b/gi, 'needs to confirm')
                .replace(/\bneed confirm\b/gi, 'need to confirm')
                .replace(/\bneeds talk to\b/gi, 'needs to talk to')
                .replace(/\bneed talk to\b/gi, 'need to talk to');
            text = text.replace(/["'“”‘’]+$/g, '').replace(/\s*\([^)]*$/g, '').trim();
            if ((text.match(/"/g) || []).length % 2 === 1) text = text.replace(/"[^".]{0,120}$/g, '').trim();
            text = text.replace(/^["“”]+/g, '').replace(/["“”]+$/g, '').trim();
            text = text.replace(/\b(indicating|including|because|with|and|or|that|the|a|an|to|of|for|by|as|which|rather than)[\.]?$/i, '').trim();
            return text.replace(/[,:;\-\s]+$/g, '').trim() || 'Not identified from available evidence.';
        }

        function renderCmfCriterionScores(ruleScores) {
            if (!ruleScores || !ruleScores.length) return '';
            var html = '<div style="margin-top:14px;"><strong>Scoring Criteria:</strong><div style="display:grid; gap:8px; margin-top:8px;">';
            for (var i = 0; i < ruleScores.length; i++) {
                var rule = ruleScores[i] || {};
                var score = parseInt(String(rule.Score || '0').replace('%', ''), 10);
                if (isNaN(score)) score = 0;
                var status = /^PASS/i.test(rule.Evaluation || '') ? 'PASS' : (/^PARTIAL/i.test(rule.Evaluation || '') ? 'PARTIAL' : 'FAIL');
                var color = status === 'PASS' ? '#047857' : (status === 'PARTIAL' ? '#b45309' : '#b91c1c');
                html += '<div style="border:1px solid #d8e2ee; border-radius:8px; padding:9px 10px; background:#fff;">';
                html += '<div style="display:flex; justify-content:space-between; gap:12px; align-items:center; font-weight:700; color:#1f344a;">';
                html += '<span>' + escapeHtml(rule.RuleId || '') + ' - ' + escapeHtml(rule.RuleName || 'Criterion') + '</span>';
                html += '<span style="color:' + color + '; white-space:nowrap;">' + escapeHtml(status) + ' · ' + score + '%</span>';
                html += '</div>';
                html += '<div style="margin-top:4px; color:#5d7289; font-size:12px; line-height:1.45;">' + escapeHtml(rule.Evaluation || '') + '</div>';
                html += '</div>';
            }
            return html + '</div></div>';
        }

        function firstCompleteSentence(text, maxLength) {
            var value = String(text || '').replace(/\s+/g, ' ').trim();
            if (!value) return '';
            var sentences = value.match(/[^.!?]+[.!?]+|[^.!?]+$/g) || [value];
            var sentence = sentences[0].trim();
            if (sentence.length <= maxLength) return sentence;
            return trimToCompleteSentence(sentence, maxLength);
        }

        function filterCmfPendingRows() {
            var input = document.getElementById('cmfPendingClientSearch');
            var table = document.getElementById('GridView_cmf_pending');
            if (!input || !table) return;
            var query = (input.value || '').toLowerCase().trim();
            var component = getPendingFilterValue('cmfPendingComponentFilter');
            var owner = getPendingFilterValue('cmfPendingOwnerFilter');
            var customer = getPendingFilterValue('cmfPendingCustomerFilter');
            var idst = getPendingFilterValue('cmfPendingIdstFilter');
            var rows = table.querySelectorAll('tbody tr');
            for (var i = 0; i < rows.length; i++) {
                var rowText = (rows[i].innerText || '').toLowerCase();
                var rowComponent = extractPendingFieldValue(rows[i].innerText || '', 'component').toLowerCase();
                var rowOwner = extractPendingFieldValue(rows[i].innerText || '', 'owner').toLowerCase();
                var rowCustomer = extractPendingFieldValue(rows[i].innerText || '', 'customer').toLowerCase();
                var rowIdst = extractPendingFieldValue(rows[i].innerText || '', 'idst').toLowerCase();
                var matchesQuery = !query || rowText.indexOf(query) >= 0;
                var matchesComponent = !component || rowComponent === component;
                var matchesOwner = !owner || rowOwner === owner;
                var matchesCustomer = !customer || rowCustomer === customer;
                var matchesIdst = !idst || rowIdst === idst;
                rows[i].style.display = matchesQuery && matchesComponent && matchesOwner && matchesCustomer && matchesIdst ? '' : 'none';
            }

            // Load sighting descriptions for visible rows
            loadSightingDescriptions();
        }

        function loadSightingDescriptions() {
            var table = document.getElementById('GridView_cmf_pending');
            if (!table) return;

            var rows = table.querySelectorAll('tbody tr');
            for (var i = 0; i < rows.length; i++) {
                var row = rows[i];
                if (getComputedStyle(row).display === 'none') continue;

                var descriptionContainers = row.querySelectorAll('.sighting-description-content');
                for (var j = 0; j < descriptionContainers.length; j++) {
                    var container = descriptionContainers[j];
                    var dataContainer = container.closest('.sighting-details-panel').querySelector('.sighting-data-container');

                    if (dataContainer) {
                        var cpId = dataContainer.getAttribute('data-sighting-id');
                        var title = dataContainer.getAttribute('data-title');
                        var component = dataContainer.getAttribute('data-component');

                        // Only load if not already loaded
                        if (container.getAttribute('data-loaded') !== 'true') {
                            loadSingleSightingDescription(cpId, title, component, container);
                        }
                    }
                }
            }
        }

        function loadSingleSightingDescription(cpId, title, component, containerElement) {
            if (!containerElement || !cpId) return;

            // Mark as loading
            containerElement.setAttribute('data-loaded', 'true');
            containerElement.setAttribute('data-loading', 'true');

            var platformValue = getIssuePendingPlatformValue();

            fetch('CMF_Web_portal.aspx/GetSightingDescription', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({
                    cpId: cpId,
                    title: title,
                    platform: platformValue
                })
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                containerElement.setAttribute('data-loading', 'false');
                var result = data && data.d ? data.d : data;

                if (!result || result.Success !== true) {
                    var errorMsg = result && result.Message ? result.Message : 'Could not generate description.';
                    containerElement.innerHTML = '<span class="sighting-description-error">⚠ ' + escapeHtml(errorMsg) + '</span>';
                    return;
                }

                var description = result.Summary || 'No description available.';
                containerElement.innerHTML = escapeHtml(description);
            })
            .catch(function (error) {
                containerElement.setAttribute('data-loading', 'false');
                containerElement.innerHTML = '<span class="sighting-description-error">⚠ Error loading description</span>';
                console.error('Error loading sighting description:', error);
            });
        }

        function openCmfPendingInsights() {
            var drawerBg = document.getElementById('cmfRecDrawerBg');
            var drawer = document.getElementById('cmfRecDrawer');
            var headingNode = document.getElementById('cmfRecHeading');
            var recNode = document.getElementById('cmfRecRecommendation');
            var evidenceNode = document.getElementById('cmfDecisionDetailsBody');
            var qualityNode = document.getElementById('cmfRecQuality');
            if (!drawerBg || !drawer || !evidenceNode) return;

            setCmfDrawerMode('insights');
            if (headingNode) headingNode.textContent = 'CMF Pending List - Insights';
            if (recNode) recNode.style.display = 'none';
            if (qualityNode) qualityNode.textContent = 'Visible sightings overview';
            evidenceNode.innerHTML = '<div class="ai-skeleton-loader"><div style="font-size:13px;font-weight:600;color:#5c7087;">&#10024; Generating CMF Pending insights...</div><div class="ai-skeleton-line ai-skeleton-title" style="width:55%;"></div><div class="ai-skeleton-line" style="width:95%;"></div><div class="ai-skeleton-line" style="width:88%;"></div></div>';
            drawerBg.classList.add('show');
            drawer.classList.add('show');

            fetch('CMF_Web_portal.aspx/GetCmfPendingListInsights', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json; charset=utf-8' },
                body: JSON.stringify({ platform: getIssuePendingPlatformValue(), rowsContext: buildCmfPendingRowsContext() })
            })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                var result = data && data.d ? data.d : data;
                if (!result || result.Success !== true) {
                    evidenceNode.textContent = result && result.Message ? result.Message : 'Unable to generate CMF Pending insights.';
                    return;
                }
                evidenceNode.innerHTML = renderCmfBriefList(String(result.Summary || '').split(/\r?\n/).map(function (line) { return line.replace(/^[-*]\s*/, '').trim(); }).filter(Boolean));
            })
            .catch(function () { evidenceNode.textContent = 'Error while calling CMF Pending insights service.'; });
        }

        function buildCmfPendingRowsContext() {
            var rows = Array.from(document.querySelectorAll('#GridView_cmf_pending tbody tr')).filter(function (row) { return getComputedStyle(row).display !== 'none'; }).slice(0, 20);
            return rows.map(function (row, index) {
                var text = row.innerText || '';
                return 'Sighting ' + (index + 1) + ': component=' + extractPendingFieldValue(text, 'component') + '; customer=' + extractPendingFieldValue(text, 'customer') + '; owner=' + extractPendingFieldValue(text, 'owner') + '; idst=' + extractPendingFieldValue(text, 'idst') + '; repro=' + extractPendingFieldValue(text, 'reproducibility') + '; impact=' + extractPendingFieldValue(text, 'impact') + '.';
            }).join('\n');
        }

        function getPendingFilterValue(id) {
            var element = document.getElementById(id);
            return element ? (element.value || '').toLowerCase().trim() : '';
        }

        function populateCmfPendingFilterOptions() {
            var table = document.getElementById('GridView_cmf_pending');
            if (!table) return;

            var filters = [
                { id: 'cmfPendingComponentFilter', label: 'Components', field: 'component' },
                { id: 'cmfPendingOwnerFilter', label: 'Owners', field: 'owner' },
                { id: 'cmfPendingCustomerFilter', label: 'Customers', field: 'customer' },
                { id: 'cmfPendingIdstFilter', label: 'iDST', field: 'idst' }
            ];

            for (var filterIndex = 0; filterIndex < filters.length; filterIndex++) {
                var filter = filters[filterIndex];
                var select = document.getElementById(filter.id);
                if (!select) continue;

                var currentValue = select.value;
                var values = [];
                var rows = table.querySelectorAll('tbody tr');
                for (var rowIndex = 0; rowIndex < rows.length; rowIndex++) {
                    var text = rows[rowIndex].innerText || '';
                    var value = extractPendingFieldValue(text, filter.field);
                    if (value && values.indexOf(value) === -1) {
                        values.push(value);
                    }
                }

                values.sort(function (left, right) { return left.localeCompare(right); });
                select.innerHTML = '<option value="">All ' + filter.label + '</option>';
                for (var valueIndex = 0; valueIndex < values.length; valueIndex++) {
                    var option = document.createElement('option');
                    option.value = values[valueIndex];
                    option.textContent = values[valueIndex];
                    select.appendChild(option);
                }
                if (currentValue && values.indexOf(currentValue) >= 0) {
                    select.value = currentValue;
                }
            }
        }

        function extractPendingFieldValue(text, field) {
            var lines = (text || '').split(/\r?\n/).map(function (line) { return line.trim(); }).filter(Boolean);
            for (var i = 0; i < lines.length; i++) {
                var line = lines[i];
                var lower = line.toLowerCase();
                if (field === 'idst' && lower.indexOf('idst:') === 0) {
                    return line.substring(line.indexOf(':') + 1).trim();
                }
                if (lower === field && lines[i + 1]) {
                    return lines[i + 1].trim();
                }
            }
            return '';
        }

        function resetCmfPendingClientFilters() {
            var input = document.getElementById('cmfPendingClientSearch');
            if (input) input.value = '';
            var selects = ['cmfPendingComponentFilter', 'cmfPendingOwnerFilter', 'cmfPendingCustomerFilter', 'cmfPendingIdstFilter'];
            for (var i = 0; i < selects.length; i++) {
                var select = document.getElementById(selects[i]);
                if (select) select.value = '';
            }
            filterCmfPendingRows();
        }
</script>
    <script src="Scripts/cmf-portal-main.js?v=20261023-popup-export-all"></script>


</head>

<body>
    <form id="form1" runat="server">
    <asp:HiddenField ID="HiddenModalDesign" runat="server" />
    <asp:HiddenField ID="HiddenInlineDesignName" runat="server" />
    <asp:HiddenField ID="HiddenInlineSwImageFreeze" runat="server" />
    <asp:HiddenField ID="HiddenInlineSupportModel" runat="server" />
    <asp:HiddenField ID="HiddenDriverName" runat="server" />
    <asp:HiddenField ID="HiddenComponentName" runat="server" />
    <asp:HiddenField ID="HiddenIssueType" runat="server" />
    <asp:HiddenField ID="hfIssueGlobalSearch" runat="server" />
    <asp:LinkButton ID="btnIssueGlobalSearchApply" runat="server" OnClick="btnIssueGlobalSearchApply_Click" Style="display:none;" />
<!-- You already have HiddenDriverName -->
    <asp:HiddenField ID="dw_trigger" runat="server" />

    <!-- Header -->
    <header>
        <!-- Convert to ASP.NET DropDownList -->


        <div class="header-title" runat="server" id="headerTitle">CMF Live Dashboard</div>
        <div class="global-search-shell" role="search">
            <i class="fas fa-search" aria-hidden="true"></i>
            <input type="text" id="globalPortalSearch" placeholder="Search issues, customers, components..." autocomplete="off" value="<%= HttpUtility.HtmlAttributeEncode(GetIssueGlobalSearchQuery()) %>" oninput="scheduleIssueGlobalSearch()" />
            <button type="button" id="globalSearchClear" class="global-search-clear" onclick="clearGlobalPortalSearch()" aria-label="Clear search">&times;</button>
            <span class="global-search-shortcut">Ctrl + K</span>
        </div>
        <div class="header-user-mode">
            <label for="ddlUserMode">Mode</label>
            <asp:DropDownList ID="ddlUserMode" runat="server" CssClass="header-mode-dropdown" AutoPostBack="true" OnSelectedIndexChanged="ddlUserMode_SelectedIndexChanged">
                <asp:ListItem Text="Program Manager" Value="program_manager" />
                <asp:ListItem Text="Admin" Value="admin" />
            </asp:DropDownList>
        </div>
        <div class="sidebar-toggle-div">
            <button class="sidebar-toggle" onclick="toggleSidebar()">☰</button>
        </div>

              <div id="updateinfo" runat="server">
  <div id="last-update">Last Updated: </div>
  <div id="next-update">Next Update: </div>
   <asp:Label ID="session_count" runat="server" Text="Total Visits: " />
</div>
    </header>
    <div class="container-scroller">



        <div id="sidebar" class="sidebar">
    <button class="close-btn" onclick="closeSidebar()">X</button>
    <!-- Hardcoded 'All Drivers' link -->
    <div style="margin-left: 20px">
        <asp:LinkButton ID="lnkAllDrivers" runat="server" CommandArgument="AllDrivers" OnCommand="lnkValue_Command">
    All Milestones
        </asp:LinkButton><br />
    </div>

    <!-- Select All Checkbox - Updated onclick -->
    <div id="selectallcheckbox" style="margin-left: 20px" runat="server">
        <input type="checkbox" id="chkSelectAll" class="milestone-checkbox" onclick="toggleSelectAll()"/>
        <label style="color: white; font-family: 'Arial', sans-serif;" for="chkSelectAll">Select All</label><br />
    </div>

    <!-- List of distinct values as checkboxes - Updated onclick -->
    <div style="margin-left: 20px">
        <asp:Repeater ID="rptDistinctValues" runat="server">
            <ItemTemplate>
                <input type="checkbox" class="chkValue milestone-checkbox"
                    id="chkValue_<%# Eval("drivers") %>"
                    value='<%# Eval("drivers") %>'
                    onclick="toggleIndividualCheckbox()" />
                <label style="color: white; font-family: 'Arial', sans-serif;"><%# Eval("drivers") %></label><br />
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <asp:LinkButton ID="submitdrivers" runat="server" CommandArgument="" OnCommand="lnkValue_Command">
Submit
    </asp:LinkButton>
</div>



        <!-- Main Content -->
        <div class="container-fluid page-body-wrapper">
                <asp:HiddenField ID="HiddenField1" runat="server" />
                <asp:HiddenField ID="HiddenModalDriver" runat="server" />
                <asp:HiddenField ID="HiddenField2" runat="server" />

                <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>

                <div class="portal-layout-shell" id="portalLayoutShell">
                    <aside class="portal-left-nav" id="mainMenuSidebar">
                        <div class="portal-section-header">
                            <button type="button" id="mainMenuToggleBtn" class="portal-collapse-btn" aria-expanded="true" onclick="toggleCollapsibleSection('mainMenu')" title="Collapse menu" aria-label="Collapse menu">
                                <span class="toggle-label">&lt;&lt;</span>
                            </button>
                        </div>
                        <div class="portal-nav-active-title" aria-live="polite">
                            <asp:Label ID="lblActiveViewTitle" runat="server" Text="Issue List" />
                        </div>
                        <div id="mainMenuSectionBody" class="collapsible-body">
                            <asp:LinkButton ID="lnkNavHome" runat="server" CssClass="portal-nav-link" OnClick="btnShowHomeDashboard_Click"><i class="fas fa-chart-pie" aria-hidden="true"></i><span>Dashboard</span></asp:LinkButton>
                            <asp:LinkButton ID="lnkNavCmfSummary" runat="server" CssClass="portal-nav-link" OnClick="btnShowGridView3_Click" Visible="false"><i class="fas fa-table-list" aria-hidden="true"></i><span>CMF Summary</span></asp:LinkButton>
                            <asp:LinkButton ID="lnkNavIssueList" runat="server" CssClass="portal-nav-link" OnClick="btnShowGridView1_Click"><i class="fas fa-fire" aria-hidden="true"></i><span>Issue List</span></asp:LinkButton>
                            <asp:LinkButton ID="lnkNavPendingList" runat="server" CssClass="portal-nav-link" OnClick="btnShowGridView4_Click"><i class="fas fa-clipboard-check" aria-hidden="true"></i><span>CMF Pending</span></asp:LinkButton>
                            <asp:LinkButton ID="lnkNavReports" runat="server" CssClass="portal-nav-link" OnClick="btnShowGridView8_Click"><i class="fas fa-chart-line" aria-hidden="true"></i><span>Reports &amp; Analytics</span></asp:LinkButton>
                            <asp:LinkButton ID="lnkNavConfigRules" runat="server" CssClass="portal-nav-link" OnClick="btnShowGridView9_Click" Visible="false"><i class="fas fa-sliders-h" aria-hidden="true"></i><span>Rules</span></asp:LinkButton>
                        </div>
                    </aside>

                    <button type="button" id="mainMenuReopenBtn" class="portal-menu-reopen" aria-expanded="false" onclick="toggleCollapsibleSection('mainMenu')" title="Open menu" aria-label="Open menu">
                        <span class="toggle-chevron">&gt;&gt;</span>
                    </button>

                    <div class="portal-main-workspace">
                        <asp:Panel ID="homeWelcomePanel" runat="server" Visible="false" CssClass="ccip-dashboard-host">
                            <div class="ccip-dashboard">
                                <div class="ccip-dash-top">
                                    <div>
                                        <asp:Label ID="lblHomeGreeting" runat="server" CssClass="ccip-dash-greeting" Text="Good Morning" />
                                        <p class="ccip-dash-sub">Here is what is happening with your critical issues today on <%: HomeDashboardPlatformLabel %> in <asp:Label ID="lblWelcomeMode" runat="server" /> mode.</p>
                                    </div>
                                    <div style="display:flex; gap:8px; align-items:center; flex-wrap:wrap;">
                                        <div class="ccip-dash-updated">Updated  <asp:Label ID="lblHomeDashboardGeneratedAt" runat="server" /></div>
                                        <asp:HyperLink ID="lnkPlatformDashboardHome" runat="server" Target="_blank" CssClass="ccip-link-btn" Visible="false">Open platform dashboard</asp:HyperLink>
                                    </div>
                                </div>

                                <div class="platform-chip-rail" data-chip-source="server" role="tablist" aria-label="Dashboard platform switch" style="margin-bottom:14px;"></div>

                                <asp:Label ID="lblHomeAiNewTodayValue" runat="server" style="display:none;" />
                                <asp:Label ID="lblHomeAiClosedTodayValue" runat="server" style="display:none;" />

                                <section id="homeOverallLayoutV2" class="overall-layout-v2" aria-label="Overall dashboard layout" style="display:none;">
                                    <article class="overall-card overall-summary-side-card overall-summary-side-span" aria-label="Q.Links section">
                                        <div class="overall-card-head">
                                            <h4 class="overall-card-title"><i class="fas fa-layer-group" aria-hidden="true"></i> Q.Links</h4>
                                        </div>
                                        <div class="overall-summary-side-body">
                                            <div class="overall-summary-facts overall-summary-side-facts" aria-hidden="true">
                                                <div class="ccip-summary-fact overall-summary-fact tone-total overall-summary-fact-blank" aria-hidden="true"></div>
                                            </div>
                                        </div>
                                    </article>

                                    <section class="overall-row overall-row-summary">
                                        <article class="overall-card overall-summary-side-card" aria-label="Reserved summary section">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-layer-group" aria-hidden="true"></i> Q.Links</h4>
                                            </div>
                                            <div class="overall-summary-side-body">
                                                <div class="overall-summary-facts overall-summary-side-facts" aria-hidden="true">
                                                    <div class="ccip-summary-fact overall-summary-fact tone-total overall-summary-fact-blank" aria-hidden="true"></div>
                                                </div>
                                            </div>
                                        </article>

                                        <article class="overall-card overall-summary-card">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-table-list" aria-hidden="true"></i> CMF Summary</h4>
                                            </div>
                                            <div id="homeOverallSummaryFacts" class="overall-summary-facts"></div>
                                        </article>
                                    </section>

                                    <section class="overall-row overall-row-second">
                                        <article class="overall-card overall-trend-card">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-chart-line" aria-hidden="true" style="color:#3b82f6; margin-right:6px;"></i>Issue Trend by Work Week</h4>
                                                <div class="overall-trend-range">
                                                    <select id="homeOverallTrendRange" class="overall-trend-range-select" aria-label="Issue trend time range">
                                                        <option value="6" selected="selected">Last 6 Weeks</option>
                                                        <option value="12">Last 12 Weeks</option>
                                                    </select>
                                                </div>
                                            </div>
                                            <div id="homeOverallTrendChart" class="overall-trend-chart"></div>
                                        </article>

                                        <article class="overall-card overall-pending-card">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-hourglass-half" aria-hidden="true" style="margin-right:6px;"></i>CMF Pending Summary</h4>
                                            </div>
                                            <div class="overall-pending-body">
                                                <div class="overall-pending-top">
                                                    <div id="homeOverallPendingPie" class="overall-pending-pie"></div>
                                                    <div id="homeOverallPendingBreakdown" class="overall-pending-breakdown"></div>
                                                </div>
                                                <div class="overall-pending-bottom">
                                                    <div class="overall-age-title">Pending Age Bucket</div>
                                                    <div id="homeOverallPendingAgeBuckets" class="overall-age-buckets"></div>
                                                </div>
                                            </div>
                                        </article>
                                    </section>

                                    <article class="overall-card overall-ai-card overall-ai-span-card">
                                        <div class="overall-card-head">
                                            <h4 class="overall-card-title"><i class="fas fa-wand-magic-sparkles" aria-hidden="true" style="margin-right:6px;"></i>AI Insights</h4>
                                            <span class="ccip-ai-beta">Beta</span>
                                        </div>
                                        <div class="overall-ai-blank"></div>
                                    </article>

                                    <section class="overall-row overall-row-third">
                                        <article class="overall-card overall-oem-card">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-building" aria-hidden="true" style="margin-right:6px;"></i>Overall OEM Specific Data</h4>
                                            </div>
                                            <div id="homeOverallOemTable" class="overall-oem-table-wrap"></div>
                                        </article>

                                        <article class="overall-card overall-heatmap-card">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-th-large" aria-hidden="true" style="margin-right:6px;"></i>Ingredient Risk Heatmap</h4>
                                            </div>
                                            <div id="homeOverallHeatmap" class="overall-heatmap-wrap"></div>
                                        </article>
                                    </section>
                                </section>

                                <div id="homeLegacyTopLayout" class="ccip-specific-top-layout">
                                    <section id="homeLegacySummarySection" class="overall-row overall-row-summary ccip-specific-summary-row" aria-label="CMF Summary KPI Cards">
                                        <article class="overall-card overall-summary-side-card" aria-label="Reserved summary section">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-layer-group" aria-hidden="true"></i> Q.Links</h4>
                                            </div>
                                            <div class="overall-summary-side-body">
                                                <div class="overall-summary-facts overall-summary-side-facts" aria-hidden="true">
                                                    <div class="ccip-summary-fact overall-summary-fact tone-total overall-summary-fact-blank" aria-hidden="true"></div>
                                                </div>
                                            </div>
                                        </article>

                                        <article class="overall-card overall-summary-card">
                                            <div class="overall-card-head">
                                                <h4 class="overall-card-title"><i class="fas fa-table-list" aria-hidden="true"></i> CMF Summary</h4>
                                            </div>
                                            <div id="homeSpecificSummaryFactsDashboard" class="overall-summary-facts"></div>
                                        </article>
                                    </section>

                                    <div id="homeLegacyOverview" class="ccip-dashboard-overview">
                                        <div class="ccip-dashboard-left">
                                            <div style="display:none;" aria-hidden="true">
                                                <asp:Label ID="lblHomeActiveIssuesValue" runat="server" />
                                                <asp:Label ID="lblHomeNeedsAttentionValue" runat="server" />
                                                <asp:Label ID="lblHomeResolvedThisWeekValue" runat="server" />
                                                <asp:Label ID="lblHomeResolutionDaysValue" runat="server" />
                                                <asp:Label ID="lblHomeCustomersAffectedValue" runat="server" />
                                            </div>
                                            <section class="ccip-card ccip-trend-card-inline overall-card overall-trend-card">
                                                <div class="overall-card-head">
                                                    <h4 class="overall-card-title"><i class="fas fa-chart-line" aria-hidden="true" style="color:#3b82f6; margin-right:6px;"></i>Issue Trend by Work Week</h4>
                                                    <div class="overall-trend-range">
                                                        <select id="homeSpecificTrendRange" class="overall-trend-range-select" aria-label="Issue trend time range">
                                                            <option value="6" selected="selected">Last 6 Weeks</option>
                                                            <option value="12">Last 12 Weeks</option>
                                                        </select>
                                                    </div>
                                                </div>
                                                <div id="homeTrendChart" class="overall-trend-chart"></div>
                                            </section>

                                            <section class="ccip-card ccip-component-summary-card ccip-inline-summary-card ccip-component-slot">
                                                <div class="ccip-card-head">
                                                    <h3 class="ccip-card-mini-title"><i class="fas fa-table" aria-hidden="true" style="color:#1d4ed8; margin-right:6px;"></i>Milestone CMF Summary</h3>
                                                </div>
                                                <div id="homeMilestoneSummaryTable" class="ccip-live-table-wrap"></div>
                                            </section>

                                        </div>
                                    </div>

                                    <aside id="homeLegacyAiHealthCard" class="overall-card overall-ai-card overall-ai-span-card">
                                        <div class="overall-card-head ccip-ai-health-head">
                                            <span><i class="fas fa-wand-magic-sparkles" aria-hidden="true" style="margin-right:6px;"></i>AI Program Health</span>
                                            <span class="ccip-ai-beta">Beta</span>
                                        </div>
                                        <div class="overall-ai-blank"></div>
                                    </aside>
                                </div>

                                <div id="homeLegacyTablesGrid" class="ccip-main-grid ccip-dashboard-tables-grid">
                                    <div class="ccip-col ccip-col-center">
                                        <div class="ccip-summary-canvas">
                                            <div class="ccip-inline-side-stack">
                                                <section class="ccip-card ccip-milestone-summary-card ccip-inline-summary-card">
                                                    <div class="ccip-card-head">
                                                        <h3 class="ccip-card-mini-title"><i class="fas fa-layer-group" aria-hidden="true" style="color:#f97316; margin-right:6px;"></i>COMPONENT OPEN CMF</h3>
                                                    </div>
                                                    <div id="homeComponentSummaryTable" class="ccip-live-table-wrap"></div>
                                                </section>

                                                <section class="ccip-card ccip-inline-summary-card ccip-pending-summary-card">
                                                    <div class="ccip-card-head">
                                                        <h3 class="ccip-card-mini-title"><i class="fas fa-clipboard-list" aria-hidden="true" style="color:#0f766e; margin-right:6px;"></i>CMF Pending Count</h3>
                                                    </div>
                                                    <div id="homePendingSummaryTable" class="ccip-live-table-wrap"></div>
                                                </section>
                                            </div>

                                            <section class="ccip-card ccip-design-summary-card">
                                                <div class="ccip-card-head">
                                                    <h3 class="ccip-card-mini-title"><i class="fas fa-diagram-project" aria-hidden="true" style="color:#0e7490; margin-right:6px;"></i>DESIGN SUMMARY</h3>
                                                    <a href="javascript:void(0);" class="ccip-card-link" onclick="openPopup(); return false;">Import Excel</a>
                                                </div>
                                                <div id="homeDesignSummaryTable" class="ccip-live-table-wrap"></div>
                                            </section>
                                        </div>

                                    </div>

                                    <div class="ccip-col ccip-col-right">
                                    </div>
                                </div>

                            </div>
                        </asp:Panel>

                        <asp:Panel ID="homeCmfSummaryPanel" runat="server" Visible="false" CssClass="ccip-dashboard-host ccip-cmf-summary-workspace">
                            <div class="ccip-dashboard">
                                <div class="ccip-dash-top">
                                    <div>
                                        <h2 class="ccip-dash-greeting" style="margin:0;">CMF Summary</h2>
                                        <p class="ccip-dash-sub">Platform-specific CMF process metrics, milestone counts, component breakdown, and pending CMF count for <%: HomeDashboardPlatformLabel %>.</p>
                                    </div>
                                    <div class="ccip-dash-updated">Updated <asp:Label ID="lblCmfSummaryGeneratedAt" runat="server" /></div>
                                </div>

                                <div class="platform-chip-rail" data-chip-source="server" role="tablist" aria-label="CMF Summary platform switch" style="margin-bottom:14px;"></div>

                                <section class="overall-row overall-row-summary ccip-specific-summary-row" aria-label="CMF Summary KPI Cards">
                                    <article class="overall-card overall-summary-card">
                                        <div class="overall-card-head">
                                            <h4 class="overall-card-title"><i class="fas fa-table-list" aria-hidden="true"></i> CMF Summary</h4>
                                        </div>
                                        <div id="homeSpecificSummaryFactsCmf" class="overall-summary-facts"></div>
                                    </article>
                                </section>

                                <div class="ccip-main-grid ccip-dashboard-tables-grid">
                                    <div class="ccip-col ccip-col-center">
                                        <section class="ccip-card ccip-milestone-summary-card">
                                            <div class="ccip-card-head">
                                                <h3 class="ccip-card-mini-title"><i class="fas fa-layer-group" aria-hidden="true" style="color:#f97316; margin-right:6px;"></i>COMPONENT OPEN CMF</h3>
                                            </div>
                                            <div id="homeComponentSummaryTable" class="ccip-live-table-wrap"></div>
                                        </section>

                                        <section class="ccip-card ccip-component-summary-card">
                                            <div class="ccip-card-head">
                                                <h3 class="ccip-card-mini-title"><i class="fas fa-table" aria-hidden="true" style="color:#1d4ed8; margin-right:6px;"></i>Milestone CMF Summary</h3>
                                            </div>
                                            <div id="homeMilestoneSummaryTable" class="ccip-live-table-wrap"></div>
                                        </section>
                                    </div>

                                    <div class="ccip-col ccip-col-right">
                                        <section class="ccip-card">
                                            <div class="ccip-card-head">
                                                <h3 class="ccip-card-mini-title"><i class="fas fa-clipboard-list" aria-hidden="true" style="color:#0f766e; margin-right:6px;"></i>CMF Pending Count</h3>
                                            </div>
                                            <div id="homePendingSummaryTable" class="ccip-live-table-wrap"></div>
                                        </section>
                                    </div>

                                    <section class="ccip-card ccip-design-summary-card">
                                        <div class="ccip-card-head">
                                            <h3 class="ccip-card-mini-title"><i class="fas fa-diagram-project" aria-hidden="true" style="color:#0e7490; margin-right:6px;"></i>DESIGN SUMMARY</h3>
                                            <a href="javascript:void(0);" class="ccip-card-link" onclick="openPopup(); return false;">Import Excel</a>
                                        </div>
                                        <div id="homeDesignSummaryTable" class="ccip-live-table-wrap"></div>
                                    </section>
                                </div>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="configRulesPanel" runat="server" Visible="false" CssClass="portal-tab-shell config-rules-workspace">
                            <div class="portal-tab-hero">
                                <div class="portal-tab-eyebrow">Administration</div>
                                <h2 class="portal-tab-title">Config/CMF Rules</h2>
                                <p class="portal-tab-desc">
                                    Admin-defined CMF qualification rules. AI recommendations in the CMF Pending List use these rules for tag / no-tag decisions.
                                </p>
                            </div>
                            <div class="cmf-rules-editor-shell">
                                <div class="portal-surface-card cmf-rules-card">
                                    <h3 class="cmf-rules-title">CMF Qualification Rules</h3>
                                    <asp:TextBox ID="txtCmfRules" runat="server" TextMode="MultiLine" CssClass="cmf-rules-editor" />
                                    <div class="cmf-rules-actions">
                                        <asp:Button ID="btnSaveCmfRules" runat="server" Text="Save Rules" CssClass="modern-button" OnClick="btnSaveCmfRules_Click" />
                                        <asp:Button ID="btnResetCmfRules" runat="server" Text="Reset Defaults" CssClass="modern-button" OnClick="btnResetCmfRules_Click" />
                                    </div>
                                    <asp:Label ID="lblCmfRulesStatus" runat="server" CssClass="cmf-rules-status" />
                                </div>
                                <div class="portal-surface-card cmf-rules-card">
                                    <h3 class="cmf-rules-title">How AI Uses These Rules</h3>
                                    <p class="cmf-rules-note">When a Program Manager opens AI Recommendation on a pending item, the service loads the latest saved admin rules and evaluates the issue fields against them.</p>
                                    <ul class="cmf-rules-list">
                                        <li>Rules are hidden from Program Manager navigation.</li>
                                        <li>Changing rules clears cached recommendations.</li>
                                        <li>The AI response must explain which rules support or block tagging.</li>
                                    </ul>
                                </div>
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="reportsPlaceholderPanel" runat="server" Visible="false" CssClass="portal-tab-shell">
                            <div class="portal-tab-hero">
                                <div class="portal-tab-eyebrow">Reports</div>
                                <h2 class="portal-tab-title">Reports &amp; Analytics</h2>
                                <p class="portal-tab-desc">
                                    Statistics view is currently disabled. This tab is reserved for finalized report content.
                                </p>
                            </div>
                        </asp:Panel>

                        <!-- ================================================================
                             REPORTS & ANALYTICS PANEL
                        ================================================================ -->
                        <div id="analyticsPanel" runat="server" visible="false" class="portal-tab-shell reports-workspace">

                            <div class="portal-tab-hero">
                                <div class="portal-tab-eyebrow">Reports</div>
                                <h2 class="portal-tab-title">Reports &amp; Analytics</h2>
                                <p class="portal-tab-desc">Ask data questions, generate charts, and create repeatable CMF reports from the live portal data.</p>
                            </div>

                            <div class="portal-surface-card reports-assistant-card">
                                <div class="reports-assistant-head">
                                    <div>
                                        <div class="reports-card-title">AI Reports Assistant</div>
                                        <div class="reports-card-subtitle">Ask data questions, generate Plotly charts, and create reports from live CMF data.</div>
                                    </div>
                                    <div style="display:flex; gap:8px; flex-wrap:wrap;">
                                        <!-- Quick chart/report actions removed per request -->
                                    </div>
                                </div>

                                <div id="reportsChatLog" class="reports-chat-log">
                                    <div style="display:flex; justify-content:flex-start; margin-bottom:10px;">
                                        <div class="reports-chat-example">
                                            Ask me things like:\n- "Show status distribution chart"\n- "Give open versus closed trend"\n- "Generate issue report csv"
                                        </div>
                                    </div>
                                </div>

                                <div class="reports-prompt-row">
                                    <input id="reportsPromptInput" type="text" placeholder="Type your analytics prompt..." class="reports-prompt-input" />
                                    <button id="reportsPromptSend" type="button" class="modern-button" onclick="sendReportsPrompt()">Send</button>
                                </div>
                            </div>

                            <div class="portal-surface-card reports-format-card">
                                <h3 class="reports-format-title">Report Format Template</h3>
                                <div class="reports-format-subtitle">Define the structure once; the assistant can populate it from live CMF data when you generate a report.</div>
                                <textarea id="reportsFormatTemplate" class="reports-format-textarea" readonly>## Weekly CMF Report - {{platform}} - {{week}}

1. Executive Summary (AI, 3 bullets)
2. New CMF-tagged sightings (table: id, issue, replication rate, impact)
3. Pending sightings likely to qualify next
4. Closed this week (root cause + fixed SW version)
5. Weekly changes / current workload
6. Chart: CMF tag rate trend</textarea>
                                <input type="file" id="reportsFormatFile" accept=".txt,.md,.csv,.json,.html" style="display:none" onchange="uploadReportsFormat(event)" />
                                <div class="reports-format-actions">
                                    <button type="button" class="modern-button" style="padding:6px 10px;" onclick="document.getElementById('reportsFormatFile').click()">Upload sample format</button>
                                    <button type="button" id="reportsFormatEditBtn" class="modern-button" style="padding:6px 10px;" onclick="setReportsTemplateEditMode(true)">Edit format</button>
                                    <button type="button" id="reportsFormatSaveBtn" class="modern-button" style="padding:6px 10px;" onclick="saveReportsFormatTemplate()" disabled>Save format</button>
                                    <button type="button" class="modern-button" style="padding:6px 10px;" onclick="generateReportsFromTemplate()">Generate from format</button>
                                    <span id="reportsFormatName" class="reports-format-subtitle" style="margin:0; display:none;"></span>
                                </div>
                            </div>

                        </div>

                <asp:DropDownList
                    ID="ddlTables"
                    runat="server"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlTables_SelectedIndexChanged"
                    CssClass="dropdown"
                    Style="display:none;">
                </asp:DropDownList>

                <div class="platform-switcher-modern" style="display:none;">
                    <div class="platform-chip-rail" data-chip-source="server" role="tablist" aria-label="Quick platform switch"></div>
                </div>

                <asp:HiddenField ID="hfQuickPlatform" runat="server" />
                <asp:HiddenField ID="hfQuickPlatformSourceTab" runat="server" />
                <asp:HiddenField ID="hfOverallExcludedPlatforms" runat="server" />
                <asp:LinkButton ID="btnQuickPlatformApply" runat="server" OnClick="btnQuickPlatformApply_Click" Style="display:none;" />

                <div class="portal-view-row" style="display:none;">
                    <div class="portal-view-left">
                        <asp:DropDownList ID="ddlFocusedView" runat="server" CssClass="portal-view-dropdown" AutoPostBack="true" OnSelectedIndexChanged="ddlFocusedView_SelectedIndexChanged">
                            <asp:ListItem Text="Main Dashboard" Value="home" />
                            <asp:ListItem Text="CMF Summary" Value="cmf" />
                            <asp:ListItem Text="Issue List" Value="issue" />
                            <asp:ListItem Text="CMF Pending List" Value="pending" />
                            <asp:ListItem Text="Reports &amp; Analytics" Value="reports" />
                            <asp:ListItem Text="Config/CMF Rules" Value="config" />
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="button-container button-container-focus" style="display:none;">

                    <asp:Button ID="btnShowGridView3" runat="server" Text="CMF Summary" OnClick="btnShowGridView3_Click" CssClass="modern-button" Visible="false" />
                    <asp:Button ID="btnShowGridView2" runat="server" Text="Design Summary" OnClick="btnShowGridView2_Click" CssClass="modern-button" Visible="false" />
                    <asp:Button ID="btnShowGridView1" runat="server" Text="Issue List" OnClick="btnShowGridView1_Click" CssClass="modern-button tab-pill" />
                    <asp:Button ID="btnShowGridView4" runat="server" Text="CMF Pending List" OnClick="btnShowGridView4_Click" CssClass="modern-button tab-pill" />
                    <asp:Button ID="btnShowGridView5" runat="server" Text="Design Indicator" OnClick="btnShowGridView5_Click" CssClass="modern-button" Visible="false" />
                    <asp:Button ID="btnShowGridView6" runat="server" Text="Ingredient Indicator" OnClick="btnShowGridView6_Click" CssClass="modern-button" Visible="false" />
                    <asp:Button ID="btnShowGridView7" runat="server" Text="OEM Indicator" OnClick="btnShowGridView7_Click" CssClass="modern-button" Visible="false" /> 

<asp:Button ID="btnExportToExcel" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_ig" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_ingred" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_des" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_design" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_des_summary" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_designsummary" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_cmf_pending" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_cmf_pending" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_oem" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_oem" CssClass="modern-button" Visible="false" />
                    <asp:Button ID="btnShowGridView8" runat="server" Text="Reports &amp; Analytics" OnClick="btnShowGridView8_Click" CssClass="modern-button tab-pill" />
                    <asp:Button ID="btnShowGridView9" runat="server" Text="Config/CMF Rules" OnClick="btnShowGridView9_Click" CssClass="modern-button tab-pill" Visible="false" />
        
                </div>

                <div class="pane-heading" runat="server" id="pane1" visible="false">CMF Summary</div>
                <div class="pane-heading" runat="server" id="pane2" visible="false">Design Summary</div>
                <div class="pane-heading" runat="server" id="pane3" visible="true">Issue List</div>
                <div class="pane-heading" runat="server" id="pane4" visible="false">CMF Pending List</div>

                <div class="pane-heading" runat="server" id="pane5" visible="false">Design Indicator</div>
                <div class="pane-heading" runat="server" id="pane6" visible="false">Ingredient Indicator</div>
                <div class="pane-heading" runat="server" id="pane7" visible="false">OEM Indicator</div>
                <div class="pane-heading" runat="server" id="pane8" visible="false">Reports &amp; Analytics</div>
                <div class="pane-heading" runat="server" id="pane9" visible="false">Config/CMF Rules</div>

                <div id="aiSummaryDrawerBg" class="ai-summary-drawer-bg" onclick="closeAiSummaryDrawer()"></div>
                <aside id="aiSummaryDrawer" class="ai-summary-drawer" aria-hidden="true" role="dialog" aria-label="AI Issue Summary">
                    <button type="button" class="ai-summary-drawer-close" onclick="closeAiSummaryDrawer()" aria-label="Close">&times;</button>
                    <h2 class="ai-summary-drawer-title"><span id="aiSummaryHeadingText">AI Summary</span><span id="aiSummaryTitleBadges" class="ai-summary-title-badges"><span id="aiSummaryConfidence" class="ai-summary-confidence-inline">Confidence: --</span><span id="aiSummaryStatusBadge" class="ai-summary-status-value">Status: --</span></span></h2>
                    <div id="aiSummaryMeta" class="ai-summary-meta-row ai-summary-meta-combined"><strong>Sighting ID:</strong> <span id="aiSummaryIssueId">-</span><strong>CMF Ask Date:</strong> <span id="aiSummarySubmittedDate">-</span></div>
                    <div class="ai-summary-meta-row" id="aiSummaryTitleRow" style="display:none"><strong>Title:</strong> <span id="aiSummaryTitle">-</span></div>
                    <div id="aiSummaryFacts" class="ai-summary-facts"></div>
                    <div class="ai-summary-body" id="aiSummaryBody">Generating AI summary...</div>
                    <div class="ai-summary-actions" id="aiSummaryActions" style="display:none; display: flex; gap: 12px; width: 100%; margin-top: 15px;">
                        <button type="button" style="flex: 1; padding: 8px; border-radius: 6px; border: 1px solid #ccc; background-color: #f7f7f7; font-weight: bold; cursor: pointer; transition: background-color 0.2s;" onmouseover="this.style.backgroundColor='#e2e2e2'" onmouseout="this.style.backgroundColor='#f7f7f7'" onclick="copyAiSummaryForReport()" title="Copy summary as plain text for pasting into reports">
                            &#x2398; Copy for Report
                        </button>
                        <button type="button" style="flex: 1; padding: 8px; border-radius: 6px; border: 1px solid #ccc; background-color: #f7f7f7; font-weight: bold; cursor: pointer; transition: background-color 0.2s;" onmouseover="this.style.backgroundColor='#e2e2e2'" onmouseout="this.style.backgroundColor='#f7f7f7'" onclick="regenerateAiSummary()" title="Regenerate summary from AI">
                            &#x21BA; Regenerate
                        </button>
                    </div>
                    <div id="aiSummaryChat" class="ai-summary-chat" style="display:none">
                        <div id="aiSummaryChatMessages" class="ai-chat-messages" aria-live="polite"></div>
                        <div class="ai-chat-input">
                            <textarea id="aiSummaryChatInput" class="ai-chat-input-box" placeholder="Ask about this defect... (Enter to send, Shift+Enter for newline)"></textarea>
                            <button type="button" id="aiSummaryChatSend" class="ai-chat-send" onclick="sendAiFollowUp();" aria-label="Send" title="Send" style="background: transparent; border: none; padding: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center; color: #0071c5; transition: transform 0.1s ease;">
                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <line x1="22" y1="2" x2="11" y2="13"></line>
                                    <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
                                </svg>
                            </button>
                        </div>
                    </div>
                </aside>

                <div id="cmfRecDrawerBg" class="ai-summary-drawer-bg" onclick="closeCmfRecDrawer()"></div>
                <aside id="cmfRecDrawer" class="ai-summary-drawer" aria-hidden="true" role="dialog" aria-label="AI CMF Pending Recommendation">
                    <button type="button" class="ai-summary-drawer-close" onclick="closeCmfRecDrawer()" aria-label="Close">&times;</button>
                    <h2 id="cmfRecHeading" class="ai-summary-drawer-title"><span class="cmf-assistant-title-icon" aria-hidden="true">✨</span>AI CMF Assistant</h2>
                    <div class="cmf-assistant-context" aria-label="Sighting context">
                        <div class="cmf-assistant-context-line">Sighting ID: <strong id="cmfAssistantSightingId">-</strong><a id="cmfAssistantHsdLink" class="cmf-assistant-hsd-link" href="#" target="_blank" rel="noopener noreferrer" title="Open sighting in HSD" aria-label="Open sighting in HSD"><i class="fas fa-external-link-alt" aria-hidden="true"></i></a></div>
                        <span id="cmfAssistantRecTag" class="cmf-assistant-rec-tag">CMF_Tag</span>
                        <div class="cmf-assistant-context-line cmf-assistant-context-title-line">Title: <strong id="cmfRecTitle" class="cmf-rec-title-text">-</strong></div>
                    </div>
                    <section id="cmfAssistantSightingDetails" class="cmf-assistant-sighting-card" aria-label="Sighting details">
                        <div class="cmf-assistant-text-section cmf-assistant-description-section">
                            <h3>Problem Description</h3>
                            <div id="cmfAssistantSightingDescription" class="cmf-assistant-description">-</div>
                        </div>
                    </section>
                    <section id="cmfAssistantRecommendationSection" class="cmf-assistant-sighting-card" aria-label="CMF recommendation">
                        <div class="cmf-assistant-text-section cmf-assistant-recommendation-section">
                            <h3>CMF Recommendation</h3>
                            <div id="cmfAssistantRecommendationBody">-</div>
                        </div>
                    </section>
                    <div id="cmfAssistantQualityAssessment" class="cmf-assistant-quality-slot" aria-label="Sighting quality assessment"></div>
                    <section id="cmfAssistantImpactSection" class="cmf-assistant-sighting-card" aria-label="Impact summary">
                        <div class="cmf-assistant-text-section cmf-assistant-impact-section">
                            <h3>Impact Summary</h3>
                            <div class="cmf-assistant-impact-block">
                                <div id="cmfAssistantImpactSummary" class="cmf-assistant-impact-summary">-</div>
                            </div>
                        </div>
                    </section>
                    <div id="cmfRecTitleRow" class="ai-summary-meta-row cmf-rec-title-row" style="display:none"><strong>Issue Title:</strong> <span class="cmf-rec-title-text">-</span><span id="cmfRecRecommendation" class="cmf-rec-decision-badge">Generating...</span></div>
                    <div id="cmfRecSightingRow" class="ai-summary-meta-row" style="display:none"><strong>Sighting ID:</strong> <span id="cmfRecCpId">-</span></div>
                    <span id="cmfRecComponent" style="display:none">-</span>
                    <div id="cmfRecQuality" class="ai-summary-meta-row" style="display:none">Evidence quality: --</div>

                    <div id="cmfRecReasoningSection" class="cmf-rec-section" style="display:none">
                        <div id="cmfRecEvidence" class="ai-summary-body">-</div>
                    </div>
                    <div id="cmfDecisionDetailsBody" class="cmf-rec-hidden"></div>

                    <div id="cmfRecActions" class="cmf-rec-placeholder-actions" style="display:none">
                        <button type="button" class="cmf-rec-placeholder-btn primary" onclick="showCmfPlaceholderAction('Approve CMF Tag')">Approve CMF Tag</button>
                        <button type="button" class="cmf-rec-placeholder-btn" onclick="showCmfPlaceholderAction('Commit Auto-fill')">Commit Auto-fill</button>
                    </div>
                </aside>

                <div id="summaryIssuesPopupBackdrop" class="summary-issues-popup-backdrop" onclick="closeSummaryIssueLinksPopup()"></div>
                <section id="summaryIssuesPopup" class="summary-issues-popup" role="dialog" aria-modal="true" aria-label="CMF issues by group">
                    <div class="summary-issues-popup-head">
                        <h3 id="summaryIssuesPopupTitle" class="summary-issues-popup-title">CMF Issues</h3>
                        <button type="button" class="summary-issues-popup-close" onclick="closeSummaryIssueLinksPopup()" aria-label="Close">&times;</button>
                    </div>
                    <div class="summary-issues-popup-controls">
                        <div class="summary-issues-popup-filter-stack">
                            <label for="summaryIssuesGroupBy">Group by</label>
                            <select id="summaryIssuesGroupBy"></select>
                            <label for="summaryIssuesSortBy">Sort by</label>
                            <select id="summaryIssuesSortBy"></select>
                            <label for="summaryIssuesShowOpen" id="summaryIssuesTrendStatusLabel">Show issues</label>
                            <div id="summaryIssuesTrendStatusChecks" class="summary-issues-popup-checkers">
                                <label><input type="checkbox" id="summaryIssuesShowOpen" checked="checked" />Open</label>
                                <label><input type="checkbox" id="summaryIssuesShowClosed" checked="checked" />Closed</label>
                            </div>
                        </div>
                        <span id="summaryIssuesPopupMeta" class="summary-issues-popup-meta"></span>
                    </div>
                    <div id="summaryIssuesPopupBody" class="summary-issues-popup-body"></div>
                </section>



                    



                <!-- Popup Modal -->
                <div id="importPopup" class="modal-import">
                    <div class="modal-content-import">
                        <span class="close-import" onclick="closePopup()">&times;</span>
                        <h3>Upload Excel File</h3>

                        <!-- File Upload Input -->
                        <asp:FileUpload ID="fileUploadExcel" runat="server" CssClass="form-control mb-2" />

                        <!-- Import Button -->
                        <asp:Button ID="btnImportExcel" runat="server" Text="Import" CssClass="btn btn-primary" OnClick="btnImportExcel_Click" />

                        <!-- Message Label -->
                        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="mt-2 d-block"></asp:Label>
                    </div>
                </div>

                <!-- Toast Container -->
<div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1055;">
    <div id="errorToast" class="toast align-items-center text-bg-danger border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
            <div class="toast-body" id="toastMessage">
                <!-- Error message will be injected here -->
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    </div>
</div>



                <!-- Hidden Field to store selected values -->
                <asp:HiddenField ID="hfSelectedValues" runat="server" />
                <asp:HiddenField ID="driverCollectorhf" runat="server" />

                <!-- Invisible HTML Button -->
                <asp:Button ID="exportBtn" runat="server" Text="Export to PPT" OnClick="ExportGridViewToPPT" Style="display: none;" />
                <!-- Modal for selecting multiple values -->
                <div class="modal fade" id="exportModal" tabindex="-1" role="dialog" aria-labelledby="exportModalLabel" aria-hidden="true">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title" id="exportModalLabel">Select Values to Export</h5>
                                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                    <span aria-hidden="true">&times;</span>
                                </button>
                            </div>
                            <div class="modal-body">
                                <asp:Repeater ID="rptDistinctFilters" runat="server">
                                    <ItemTemplate>
                                        <input type="checkbox" class="chkValue" value='<%# Eval("drivers") %>' />
                                        <%# Eval("drivers") %>


                                        <br />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                                <asp:Button
                                    ID="btnExportInModal"
                                    runat="server"
                                    CssClass="btn btn-primary"
                                    Text="Export"
                                    OnClientClick="submitExport(); return false;"
                                    Visible="true" />


                            </div>
                        </div>
                    </div>
                </div>

                

<div id="mainDataWrapper" runat="server" class="content-wrapper">
    <div class="modal-body popup">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="table-container">

                    <!-- SHARED FILTER PANEL FOR ISSUE LIST & CMF PENDING LIST -->
                    <div id="sharedFilterPanel" runat="server" visible="false" class="overview-platform-actions">
                        <div class="overview-platform-filter">
                            <label>Platform:</label>
                            <asp:DropDownList ID="ddlSharedPlatform" runat="server" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlSharedPlatform_SelectedIndexChanged"
                                CssClass="form-control">
                                <asp:ListItem Text="-- Select Platform --" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="issue-external-links">
                            <a href="https://crtt.intel.com/Administration/SetupIDST.aspx?DNo=5343&PNo=135&Ing=92&CNo=1225" target="_blank">CRTT</a>
                            <asp:HyperLink ID="lnkPlatformDashboard" runat="server" Target="_blank" />
                            <asp:HyperLink ID="lnkPlatformDashboardPending" runat="server" Target="_blank" />
                            <a href="https://pbirs01.intel.com/reports/powerbi/CCE-GCE%20DASHBOARDS/CMF_Portal" target="_blank">Infographic</a>
                        </div>
                    </div>

                    <div class="issue-tab-shell" id="issueTabShell" style="display:none;">
                    <div class="issue-tab-main">

                    <asp:Panel ID="issueListHeaderPanel" runat="server" Visible="true">
                        <div class="issue-page-hd">
                            <div class="issue-page-hd-top">
                                <h1 class="issue-page-title">Issue List</h1>
                            </div>
                            <p class="issue-page-desc">
                                Review active issues, understand their current status, and identify what needs attention next.
                            </p>
                        </div>
                        <div class="issue-kpi-row">
                            <div class="issue-kpi">
                                <div class="issue-kpi-label">Total Issues</div>
                                <div class="issue-kpi-value"><asp:Label ID="lblIssueTotal" runat="server" Text="0" /></div>
                                <div class="issue-kpi-sub"><i class="fas fa-arrow-up" aria-hidden="true"></i> current platform workload</div>
                            </div>
                            <div class="issue-kpi">
                                <div class="issue-kpi-label">Open Issues</div>
                                <div class="issue-kpi-value" style="color:#0068b5;"><asp:Label ID="lblIssueInProgress" runat="server" Text="0" /></div>
                                <div class="issue-kpi-sub"><i class="fas fa-arrow-up" aria-hidden="true"></i> open status</div>
                            </div>
                            <div class="issue-kpi">
                                <div class="issue-kpi-label">Closed Issues</div>
                                <div class="issue-kpi-value" style="color:#107c10;"><asp:Label ID="lblIssueClosed" runat="server" Text="0" /></div>
                                <div class="issue-kpi-sub"><i class="fas fa-arrow-up" aria-hidden="true"></i> complete or rejected</div>
                            </div>
                            <div class="issue-kpi">
                                <div class="issue-kpi-label">Implemented Issues</div>
                                <div class="issue-kpi-value" style="color:#bc2f32;"><asp:Label ID="lblIssueStale" runat="server" Text="0" /></div>
                                <div class="issue-kpi-sub"><i class="fas fa-clock" aria-hidden="true"></i> implemented or verified</div>
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- TOP FILTER PANEL - FOR ISSUE LIST -->
                    <asp:Panel ID="fieldSelectorPanel" runat="server" Visible="true">
                        <div class="field-selector-panel issue-top-filter-panel">
                            <div class="issue-filter-section-header">
                                <div class="issue-filter-title"><i class="fas fa-filter" aria-hidden="true"></i> Filters</div>
                                <button type="button" id="issueFiltersToggleBtn" class="issue-collapse-btn" aria-expanded="true" onclick="toggleCollapsibleSection('issueFilters')">
                                    <span class="toggle-label">Hide</span>
                                    <span class="toggle-chevron">▾</span>
                                </button>
                            </div>
                            <div id="issueTopFilterBody" class="collapsible-body">
                                <div class="issue-top-filter-grid">
                                    <div class="issue-top-filter-item"><span>Milestone</span><asp:DropDownList ID="ddlMilestoneTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlMilestoneHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>Owner</span><asp:DropDownList ID="ddlOwnerTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlOwnerHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>RVP Repro</span><asp:DropDownList ID="ddlRvpReproTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRvpReproHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>iDST</span><asp:DropDownList ID="ddlIdstTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlIdstHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>LOS</span><asp:DropDownList ID="ddlLosTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLosHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>Company</span><asp:DropDownList ID="ddlCompanyTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlCompanyHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>Customer Detail</span><asp:DropDownList ID="ddlDetailTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDetailHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                    <div class="issue-top-filter-item"><span>Component Group</span><asp:DropDownList ID="ddlComponentTop" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlComponentHeader_SelectedIndexChanged" CssClass="header-dropdown" /></div>
                                </div>
                                <div class="issue-top-filter-buttons">
                                    <asp:Button ID="btnClearFilters" runat="server" CssClass="filter-btn filter-btn-secondary" Text="Clear Filters" OnClick="btnClearFilters_Click" />
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                    <!-- END TOP FILTER PANEL -->

                    <div id="searchfilters" class="header-overlay" runat="server" visible="false">
                        <!-- Your existing search filters -->
                        <input type="text" id="searchColumn0" placeholder="Search Progress" oninput="filterGrid()" />
                        <input type="text" id="searchColumn5" placeholder="Search Owner" oninput="filterGrid()" />
                        <input type="text" id="searchColumn10" placeholder="Search Component" oninput="filterGrid()" />
                        <input type="text" id="searchColumn4" placeholder="Search Component" oninput="filterGrid()" />
                        <input type="text" id="searchColumn6" placeholder="Search RVP Repro" oninput="filterGrid()" />
                    </div>

                    <div class="issue-user-toolbar">
                        <div class="issue-view-note"><i class="fas fa-table-list" aria-hidden="true"></i> Compact issue workspace</div>
                        <div class="issue-user-actions">
                            <button
                                type="button"
                                id="issueFilterToolbarBtn"
                                class="issue-toolbar-btn filter-toggle"
                                onclick="toggleCollapsibleSection('issueFilters')">
                                <i class="fas fa-filter" aria-hidden="true"></i>
                                Filters
                            </button>
                            <button
                                type="button"
                                id="btnShowAllColumns"
                                class="issue-toolbar-btn secondary"
                                onclick="showAllColumns()"
                                style="display:none">
                                <i class="fas fa-table-columns" aria-hidden="true"></i>
                                Show Columns
                            </button>
                        </div>
                    </div>


                    <div class="gridview-container issue-grid-scroll">
                        <div class="issue-grid-toolbar">
                            <button type="button" id="btnShowAllColumns" class="show-cols-btn" onclick="showAllColumns()" style="display:none" title="Restore all hidden columns">&#x21BA; Show All Columns</button>
                        </div>
                        <!-- MODIFY YOUR GRIDVIEW TO ADD DATA-FIELD ATTRIBUTES -->
                        <div id="issue-grid-inner" class="issue-grid-inner">

<asp:GridView ID="overall_request_details" CssClass="table-primary" runat="server" AutoGenerateColumns="False"
    EmptyDataText="No Open CMFs" OnRowEditing="overall_request_details_RowEditing"
    OnRowUpdating="overall_request_details_RowUpdating" OnRowCancelingEdit="overall_request_details_RowCancelingEdit"
    OnRowDataBound="overall_request_details_RowDataBound" OnPageIndexChanging="overall_request_details_PageIndexChanging"
    AllowPaging="true" PageSize="12" PagerStyle-CssClass="pager" Visible="false">

    <PagerSettings Visible="false" />

    <Columns>

        <asp:TemplateField HeaderText="#" ItemStyle-Width="50px" HeaderStyle-Width="50px"
            ItemStyle-CssClass="field-sno" HeaderStyle-CssClass="field-sno">
            <ItemTemplate>
                <div class="issue-sno-cell">
                    <span class="issue-sno-index"><%# Container.DataItemIndex + 1 %></span>
                    <asp:LinkButton ID="btnRowEdit" runat="server" CommandName="Edit" CausesValidation="false" CssClass="issue-inline-edit-btn" ToolTip="Edit row" aria-label="Edit row">
                        <i class="fas fa-pencil-alt" aria-hidden="true"></i>
                    </asp:LinkButton>
                </div>
            </ItemTemplate>
            <EditItemTemplate>
                <div class="issue-sno-cell">
                    <span class="issue-sno-index"><%# Container.DataItemIndex + 1 %></span>
                    <span class="issue-inline-edit-actions">
                        <asp:LinkButton ID="btnRowUpdate" runat="server" CommandName="Update" CssClass="issue-inline-edit-action" ToolTip="Save changes" aria-label="Save changes">
                            <i class="fas fa-check" aria-hidden="true"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnRowCancel" runat="server" CommandName="Cancel" CausesValidation="false" CssClass="issue-inline-edit-action" ToolTip="Cancel edit" aria-label="Cancel edit">
                            <i class="fas fa-times" aria-hidden="true"></i>
                        </asp:LinkButton>
                    </span>
                </div>
            </EditItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Issue Details" ItemStyle-CssClass="field-issue_details" HeaderStyle-CssClass="field-issue_details">
            <ItemTemplate>
                <%# RenderIssueDetails(Eval("SightingID"), Eval("Merged_PromotedID"), Eval("title"), Eval("cmf_request"), Eval("SubmittedDate"), Eval("IssueStatus"), Eval("sysdebug")) %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Customer" ItemStyle-CssClass="field-customer_detail" HeaderStyle-CssClass="field-customer_detail">
            <ItemTemplate><%# RenderCustomerDetailWithCompanyAndProcessor(Eval("customer_detail"), Eval("customer_company"), Eval("processor")) %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Component" ItemStyle-CssClass="component-column field-component" HeaderStyle-CssClass="field-component">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">Component</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlComponentHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlComponentHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><%# RenderComponentWithGroupBadge(Eval("component"), Eval("component_group")) %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Owner" ItemStyle-CssClass="field-owner" HeaderStyle-CssClass="field-owner">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">Owner</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlOwnerHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlOwnerHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><%# RenderOwnerWithPromotedOwner(Eval("Owner"), Eval("owners_name")) %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtOwner" runat="server" TextMode="MultiLine" Text='<%# Bind("Owner") %>'
                    CssClass="form-control"></asp:TextBox>
            </EditItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Milestone" ItemStyle-CssClass="field-milestone" HeaderStyle-CssClass="field-milestone">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">Milestone</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlMilestoneHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlMilestoneHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate>
                <%# RenderMilestoneText(Eval("Driver"), Eval("MustFixFor")) %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Status" ItemStyle-CssClass="field-status" HeaderStyle-CssClass="field-status">
            <ItemTemplate>
                <%# RenderStatusWithAiSummaryButton(Eval("IssueStatus"), Eval("Status"), Eval("SightingID"), Eval("title"), Eval("SubmittedDate"), Eval("sysdebug"), Container.DataItem) %>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="Status" HeaderText="CMF Status" ReadOnly="True" Visible="false"
            ItemStyle-CssClass="field-cmf_status" HeaderStyle-CssClass="field-cmf_status" />

        <asp:BoundField DataField="ImageFreeze" HeaderText="Image Freeze" ReadOnly="True"
    ItemStyle-CssClass="field-imagefreeze" HeaderStyle-CssClass="field-imagefreeze" />

        <asp:TemplateField HeaderText="Duplicate Details"
            ItemStyle-CssClass="field-duplicatedetails" HeaderStyle-CssClass="field-duplicatedetails">
            <ItemTemplate><%# CreateDuplicateLinks(Eval("DuplicateDetails")) %></ItemTemplate>
        </asp:TemplateField>


        <asp:TemplateField HeaderText="RVP Repro" ItemStyle-CssClass="field-rvp_repro" HeaderStyle-CssClass="field-rvp_repro">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">RVP Repro</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlRvpReproHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlRvpReproHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><span><%# Eval("repro_on_rvp") %></span></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="iDST" ItemStyle-CssClass="field-idst" HeaderStyle-CssClass="field-idst">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">iDST</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlIdstHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlIdstHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><span class="idst-column"><%# Eval("idst") %></span></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtidst" runat="server" TextMode="MultiLine" Text='<%# Bind("idst") %>'
                    CssClass="form-control"></asp:TextBox>
            </EditItemTemplate>
        </asp:TemplateField>


        <asp:TemplateField HeaderText="LOS" ItemStyle-CssClass="field-los" HeaderStyle-CssClass="field-los">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">LOS</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlLosHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlLosHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemStyle Width="100px" />
            <HeaderStyle Width="100px" />
            <ItemTemplate><span class="los-column"><%# Eval("los") %></span></ItemTemplate>
            <EditItemTemplate>
                <asp:DropDownList ID="ddllos" runat="server" CssClass="form-control"></asp:DropDownList>
            </EditItemTemplate>
        </asp:TemplateField>


        <asp:BoundField DataField="processor" HeaderText="Processor" ReadOnly="True"
            ItemStyle-CssClass="field-processor" HeaderStyle-CssClass="field-processor" Visible="false" />


    <asp:TemplateField HeaderText="Impact" ItemStyle-CssClass="field-impact_processor" HeaderStyle-CssClass="field-impact_processor">
        <ItemTemplate><%# RenderImpactWithProcessor(Eval("impact"), Eval("processor")) %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtimpact" runat="server" TextMode="MultiLine" Text='<%# Bind("impact") %>'
                    CssClass="form-control"></asp:TextBox>
            </EditItemTemplate>
        </asp:TemplateField>


        <asp:TemplateField HeaderText="Days Open" ItemStyle-CssClass="daysopen-column field-days_open" HeaderStyle-CssClass="field-days_open">
            <ItemTemplate><%# RenderDaysOpen(Eval("days_active")) %></ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="ClosedDetails" HeaderText="Closed Reason / Fixed Version" ReadOnly="True"
            ItemStyle-CssClass="cmfrequest-column field-closed_reason" HeaderStyle-CssClass="field-closed_reason" />

        <asp:BoundField DataField="progress" HeaderText="Progress" ReadOnly="True" Visible="false"
            ItemStyle-CssClass="field-progress" HeaderStyle-CssClass="field-progress" />

    </Columns>

<EmptyDataTemplate>
    <div class="empty-data-container">
        <table class="table-primary" style="width:100%">
            <thead>
                <tr>

                    <th style="width:50px;" class="field-sno">S.No</th>

                    <th class="field-sightingid">SightingID | Promoted ID</th>

                    <th class="field-milestone">
                        <div class="filter-header-container">
                            <div class="filter-header-text">Milestone</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlMilestoneHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlMilestoneHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>

                    <th class="field-title">Title</th>

                    <th class="field-owner">
                        <div class="filter-header-container">
                            <div class="filter-header-text">Owner</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlOwnerHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlOwnerHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>

                    <th class="field-status">Status</th>

                    <th class="field-component">Component</th>


                    <th class="field-customer_detail">
                        <div class="filter-header-container">
                            <div class="filter-header-text">Customer Detail</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlDetailHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlDetailHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>

                    <th class="field-imagefreeze">ImageFreeze</th>

                    <th class="field-duplicatedetails">Duplicate Details</th>

                    <th class="field-rvp_repro">
                        <div class="filter-header-container">
                            <div class="filter-header-text">RVP Repro</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlRvpReproHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlRvpReproHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>

                    <th class="field-idst">
                        <div class="filter-header-container">
                            <div class="filter-header-text">iDST</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlIdstHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlIdstHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>


                    <th class="field-los" style="width:100px;">
                        <div class="filter-header-container">
                            <div class="filter-header-text">LOS</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlLosHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlLosHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>


                    <th class="field-processor">Processor</th>


                    <th class="field-impact">Impact</th>


                    <th class="field-days_open">Days_Open</th>


                    <th class="field-closed_reason">Closed Reason / Fixed Version</th>

                    <th class="field-progress">Progress</th>


                </tr>
            </thead>
            <tbody>
                <tr>
                    <td colspan="23" style="text-align:center; padding:20px;">
                        No Open CMFs found for the selected filters. Please adjust your filter criteria.
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</EmptyDataTemplate>


</asp:GridView>
                        </div>
                    </div>
                        <asp:Panel ID="issuePagerPanel" runat="server" Visible="false">
                            <div id="issue-scroll-proxy" class="issue-scroll-proxy" aria-label="Issue list horizontal scroll">
                                <div id="issue-scroll-proxy-inner" class="issue-scroll-proxy-inner"></div>
                            </div>
                            <div class="issue-pager-controls">
                                <div class="issue-page-nav" aria-label="Issue pages">
                                    <asp:LinkButton ID="btnPageGroupPrev" runat="server" CssClass="issue-pager-group-btn" OnClick="btnPageGroupPrev_Click" ToolTip="Previous 10 pages">&laquo;</asp:LinkButton>
                                    <asp:Repeater ID="rptPageNumbers" runat="server" OnItemCommand="rptPageNumbers_ItemCommand">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnPageNumber" runat="server" CommandName="SelectPage" CommandArgument='<%# Eval("PageNumber") %>' CssClass='<%# "issue-pager-btn " + (Eval("IsCurrentPage").ToString() == "True" ? "issue-pager-current" : "") %>' Text='<%# Eval("PageNumber") %>' />
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <asp:LinkButton ID="btnPageGroupNext" runat="server" CssClass="issue-pager-group-btn" OnClick="btnPageGroupNext_Click" ToolTip="Next 10 pages">&raquo;</asp:LinkButton>
                                </div>
                                <span class="issue-page-size-control">
                                    <span>Rows per page</span>
                                    <asp:DropDownList ID="ddlIssuePageSize" runat="server" AutoPostBack="true" CssClass="issue-page-size-select" OnSelectedIndexChanged="ddlIssuePageSize_SelectedIndexChanged">
                                        <asp:ListItem Text="10" Value="10" />
                                        <asp:ListItem Text="12" Value="12" Selected="True" />
                                        <asp:ListItem Text="25" Value="25" />
                                        <asp:ListItem Text="50" Value="50" />
                                        <asp:ListItem Text="100" Value="100" />
                                    </asp:DropDownList>
                                </span>
                            </div>
                            <div class="issue-page-status">
                                <asp:Label ID="lblIssuePageStatus" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>

                    <!-- CMF PENDING LIST HEADER PANEL - INSIDE mainDataWrapper -->
                    <div class="pending-tab-shell" id="pendingTabShell" style="display:none;">
                        <div class="pending-tab-main">
                            <div class="cmf-pending-page-hd">
                                <div>
                                    <div class="cmf-pending-breadcrumb"><asp:LinkButton ID="lnkPendingBreadcrumbHome" runat="server" OnClick="btnShowHomeDashboard_Click">Home</asp:LinkButton> <span>›</span> CMF Pending List</div>
                                    <div class="cmf-pending-page-hd-top">
                                        <div>
                                            <h1 class="cmf-pending-page-title">CMF Pending List</h1>
                                            <p class="cmf-pending-page-desc">Track and manage all pending CMFs and recommended dispositions.</p>
                                        </div>
                                        <div class="cmf-pending-actions">
                                            <button type="button" class="cmf-pending-action-btn" onclick="openCmfPendingInsights();"><i class="fas fa-wand-magic-sparkles" aria-hidden="true"></i><span>AI Insights</span></button>
                                            <button type="button" class="cmf-pending-action-btn primary" onclick="showPortalToast('Use Export to Excel from the left navigation for the active portal export workflow.');"><i class="fas fa-download" aria-hidden="true"></i><span>Export</span></button>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <asp:Panel ID="cmf_pending_header_panel" runat="server" Visible="false"></asp:Panel>

                            <div class="cmf-pending-kpi-row template-kpi-row">
                                <div class="cmf-pending-kpi template-kpi-card kpi-blue">
                                    <div class="template-kpi-icon"><i class="fas fa-file-lines" aria-hidden="true"></i></div>
                                    <div><div class="cmf-pending-kpi-label">Pending CMFs</div><div class="cmf-pending-kpi-value"><asp:Label ID="lblPendingSightings" runat="server" Text="0" /></div><div class="cmf-pending-kpi-sub">Current platform workload</div></div>
                                </div>
                                <div class="cmf-pending-kpi template-kpi-card kpi-amber">
                                    <div class="template-kpi-icon"><i class="fas fa-triangle-exclamation" aria-hidden="true"></i></div>
                                    <div><div class="cmf-pending-kpi-label">Needs Attention</div><div class="cmf-pending-kpi-value"><asp:Label ID="lblIncompleteSysScope" runat="server" Text="0" /></div><div class="cmf-pending-kpi-sub">Missing decision evidence</div></div>
                                </div>
                                <div class="cmf-pending-kpi template-kpi-card kpi-violet">
                                    <div class="template-kpi-icon"><i class="fas fa-robot" aria-hidden="true"></i></div>
                                    <div><div class="cmf-pending-kpi-label">AI Recommended</div><div class="cmf-pending-kpi-value"><asp:Label ID="lblQualifyForCmf" runat="server" Text="0" /></div><div class="cmf-pending-kpi-sub">Likely CMF candidates</div></div>
                                </div>
                                <div class="cmf-pending-kpi template-kpi-card kpi-green">
                                    <div class="template-kpi-icon"><i class="fas fa-copy" aria-hidden="true"></i></div>
                                    <div><div class="cmf-pending-kpi-label">Potential Duplicates</div><div class="cmf-pending-kpi-value"><asp:Label ID="lblLikelyDuplicate" runat="server" Text="0" /></div><div class="cmf-pending-kpi-sub">Review linked sightings</div></div>
                                </div>
                            </div>

                            <div class="cmf-pending-filter-card" aria-label="CMF pending list filters">
                                <div class="cmf-pending-searchbox"><i class="fas fa-search" aria-hidden="true"></i><input type="text" id="cmfPendingClientSearch" placeholder="Search by ID, title, customer, component..." oninput="filterCmfPendingRows()" /></div>
                                <select id="cmfPendingComponentFilter" class="cmf-pending-filter-select" onchange="filterCmfPendingRows()" aria-label="Filter CMF Pending by component"><option value="">All Components</option></select>
                                <select id="cmfPendingOwnerFilter" class="cmf-pending-filter-select" onchange="filterCmfPendingRows()" aria-label="Filter CMF Pending by owner"><option value="">All Owners</option></select>
                                <select id="cmfPendingCustomerFilter" class="cmf-pending-filter-select" onchange="filterCmfPendingRows()" aria-label="Filter CMF Pending by customer"><option value="">All Customers</option></select>
                                <select id="cmfPendingIdstFilter" class="cmf-pending-filter-select" onchange="filterCmfPendingRows()" aria-label="Filter CMF Pending by iDST"><option value="">All iDST</option></select>
                                <button type="button" class="cmf-pending-filter-btn" onclick="resetCmfPendingClientFilters()"><i class="fas fa-rotate-left" aria-hidden="true"></i><span>Reset</span></button>
                            </div>

                            <!-- CMF PENDING LIST GRIDVIEW - INSIDE mainDataWrapper -->
                            <div class="gridview-container cmf-pending-grid-wrap" id="cmfPendingGridContainer" runat="server">
                                <asp:GridView ID="GridView_cmf_pending" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Issue Details" ItemStyle-Width="285px" HeaderStyle-Width="285px">
                                            <ItemTemplate><%# RenderPendingIssueDetailsWithRecommendation(Eval("cp_id"), Eval("title"), Eval("cmf_request"), Eval("component"), Eval("impact"), Eval("idst"), Eval("repro_on_rvp"), Eval("reproducibility"), Eval("customer_detail"), Eval("customer_owner")) %></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Customer / Owner" ItemStyle-Width="160px" HeaderStyle-Width="160px">
                                            <ItemTemplate><%# RenderPendingCustomer(Eval("customer_detail"), Eval("customer_owner")) %></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Evidence" ItemStyle-Width="190px" HeaderStyle-Width="190px">
                                            <ItemTemplate><%# RenderPendingEvidence(Eval("idst"), Eval("repro_on_rvp"), Eval("reproducibility")) %></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Impact" ItemStyle-Width="210px" HeaderStyle-Width="210px">
                                            <ItemTemplate><%# RenderPendingAskImpact(Eval("cp_id"), Eval("title"), Eval("component"), Eval("date_cmf_ask"), Eval("cmf_request"), Eval("impact"), Eval("idst"), Eval("repro_on_rvp"), Eval("reproducibility"), Eval("customer_detail"), Eval("customer_owner")) %></ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="AI CMF Assistant" ItemStyle-Width="150px" HeaderStyle-Width="150px">
                                            <ItemTemplate><%# RenderPendingRecommendationCell(Eval("cp_id"), Eval("title"), Eval("component"), Eval("component_group"), Eval("cmf_request"), Eval("impact"), Eval("idst"), Eval("repro_on_rvp"), Eval("reproducibility"), Eval("customer_detail"), Eval("customer_owner")) %></ItemTemplate>
                                        </asp:TemplateField>

                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                        
                        <aside class="pending-side-panel" aria-label="CMF pending assistant panel">
                            <section class="interactive-side-card">
                                <div class="interactive-side-head">
                                    <h3 class="interactive-side-title"><i class="fas fa-brain" aria-hidden="true" style="color:#7c3aed; margin-right:6px;"></i>Pending AI Queue</h3>
                                </div>
                                <p class="interactive-side-sub">Recommendation engine snapshots for current platform selection.</p>
                                <button type="button" class="interactive-side-btn" onclick="focusFirstPendingRecommendation(); return false;">Open Recommendation Flow</button>
                            </section>

                            <section class="interactive-side-card">
                                <div class="interactive-side-head"><h3 class="interactive-side-title">Quick Insights</h3></div>
                                <ul class="interactive-chip-list">
                                    <li><span>Qualification candidates</span><span class="interactive-chip" id="pendingSideQualify">0</span></li>
                                    <li><span>Potential duplicates</span><span class="interactive-chip" id="pendingSideDup">0</span></li>
                                    <li><span>Missing debug evidence</span><span class="interactive-chip" id="pendingSideMissing">0</span></li>
                                </ul>
                            </section>

                            <section class="interactive-side-card">
                                <div class="interactive-side-head"><h3 class="interactive-side-title">Shortcuts</h3></div>
                                <div class="interactive-shortcuts">
                                    <button type="button" class="interactive-shortcut-btn" onclick="focusFirstPendingRecommendation(); return false;">Top Priority First</button>
                                    <button type="button" class="interactive-shortcut-btn" onclick="showPortalToast('Use shared platform dropdown above to switch context.')">Switch Platform</button>
                                    <button type="button" class="interactive-shortcut-btn" onclick="focusFirstPendingRecommendation(); return false;">Review Scores</button>
                                    <button type="button" class="interactive-shortcut-btn" onclick="focusReportsAssistant('Generate a CMF pending backlog report for all platforms'); return false;">Export Pending</button>
                                </div>
                            </section>

                            <section class="interactive-side-card">
                                <div class="interactive-side-head"><h3 class="interactive-side-title">Actions</h3></div>
                                <div class="interactive-shortcuts">
                                    <button type="button" class="interactive-shortcut-btn" onclick="focusFirstPendingRecommendation(); return false;">Prioritize High Score</button>
                                    <button type="button" class="interactive-shortcut-btn" onclick="focusFirstPendingRecommendation(); return false;">Commit Auto-fill</button>
                                    <button type="button" class="interactive-shortcut-btn" onclick="focusReportsAssistant('Generate a CMF backlog sharing report for all platforms'); return false;">Export Backlog</button>
                                    <button type="button" class="interactive-shortcut-btn" onclick="showPortalToast('Switch platform in the shared filter bar.')">Switch Platform</button>
                                </div>
                            </section>
                        </aside>
                    </div>

                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>

                 

                                    <div class="gridview-container">
                                        <asp:GridView ID="GridView_design_open" CssClass="table-primary2" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false" OnRowEditing="GridView_design_open_RowEditing" OnRowDataBound="GridView_design_open_RowDataBound" DataKeyNames="Design">
                                        </asp:GridView>
                                    </div>

                                    <div class="gridview-container">
                                        <asp:GridView ID="GridView2_edit" CssClass="table-primary2" runat="server" AutoGenerateColumns="false" EmptyDataText="No Open CMFs" Visible="false" OnRowUpdating="GridView_design_open_RowUpdating" OnRowCancelingEdit="GridView_design_open_RowCancelingEdit" OnRowDataBound="GridView_design_open_RowDataBound" DataKeyNames="Design">
                                            <Columns>
                                                <asp:BoundField DataField="Design" HeaderText="Design Name" ReadOnly="True" />

                                                <asp:TemplateField HeaderText="SW Image Freeze">
                                                    <ItemTemplate>
                                                        <span><%# Eval("SWImageFreeze") %></span>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtSWImageFreeze" runat="server" TextMode="MultiLine" Text='<%# Bind("SWImageFreeze") %>' CssClass="form-control"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Support Model">
                                                    <ItemTemplate>
                                                        <span><%# Eval("SupportModel") %></span>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtSupportModel" runat="server" TextMode="MultiLine" Text='<%# Bind("SupportModel") %>' CssClass="form-control"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateField>
                                                <asp:CommandField ShowEditButton="True" />
                                            </Columns>
                                        </asp:GridView>
                                    </div>

                                    <asp:Panel ID="panelExtraGridViews" runat="server" Visible="false">
                                    <div class="gridview-container">
                                        <asp:GridView ID="GridView_design_summary" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false" OnRowDataBound="GridView5_RowDataBound">
                                            <Columns>
                                                
                                                <asp:BoundField DataField="Design" HeaderText="Design" />
                                                <asp:BoundField DataField="SWImageFreeze" HeaderText="SW Image Freeze" />
                                                <asp:BoundField DataField="Issues_in_CMF_ASK" HeaderText="Issues in CMF_ASK" />
                                                <asp:BoundField DataField="Total_CMF_REJECT" HeaderText="Total CMF_REJECT" />
                                                <asp:BoundField DataField="Total_CMF_Approved" HeaderText="Total CMF_Approved" />
                                               <asp:BoundField DataField="crit_tpt" HeaderText="CMF Overall TPT"  />
                                                <asp:BoundField DataField="disp_tpt" HeaderText="CMF Disposition TPT"  />
                                                <asp:BoundField DataField="resolve_tpt" HeaderText="CMF Resolution TPT"  />

                                                <asp:BoundField DataField="CMFOpenPercentage" HeaderText="CMF Open %"  />
                                                <asp:BoundField DataField="Noise" HeaderText="Noise%"  />
                                                <asp:BoundField DataField="IntelIssuePercentage" HeaderText="Intel Issue %"/>  
                                                <asp:BoundField DataField="ThirdPartyPercentage" HeaderText="3rd Party %" />
                                                <asp:BoundField DataField="CustomerIssuePercentage" HeaderText="Customer Issue %" />
                                                
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                        
                                    
                                <div class="gridview-container">
    <asp:GridView ID="GridView_oem_summary" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false" OnRowDataBound="GridView7_RowDataBound">
        <Columns>
            <asp:BoundField DataField="OEM" HeaderText="OEM" />
            <asp:BoundField DataField="Issues_in_CMF_ASK" HeaderText="Issues in CMF_ASK" />
            <asp:BoundField DataField="Total_CMF_REJECT" HeaderText="Total CMF_REJECT" />
            <asp:BoundField DataField="Total_CMF_Approved" HeaderText="Total CMF_Approved" />
            <asp:BoundField DataField="crit_tpt" HeaderText="CMF Overall TPT" />
            <asp:BoundField DataField="disp_tpt" HeaderText="CMF Disposition TPT" />
            <asp:BoundField DataField="resolve_tpt" HeaderText="CMF Resolution TPT" />
            <asp:BoundField DataField="CMFOpenPercentage" HeaderText="CMF Open %" />
            <asp:BoundField DataField="Noise" HeaderText="Noise%" />
            <asp:BoundField DataField="IntelIssuePercentage" HeaderText="Intel Issue %" />
            <asp:BoundField DataField="ThirdPartyPercentage" HeaderText="3rd Party %" />
            <asp:BoundField DataField="CustomerIssuePercentage" HeaderText="Customer Issue %" />
        </Columns>
    </asp:GridView>
</div>
                    </asp:Panel>
                <!-- Bootstrap Modal for OEM Summary -->
<div class="modal fade" id="detailsModal3" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog custom-modal-dialog" role="document"> <!-- Custom class for modal -->
    <div class="modal-content">
      <div class="modal-header">
        <div class="modal-title-container">
            <h5 class="modal-title">OEM Summary Details</h5>
            <asp:Button ID="btnExportOEMToExcel" runat="server" Text="Export to Excel" CssClass="modern-button1 export-btn" OnClientClick="exportToExcel3('<%= HiddenModalDesign.Value %>'); return false;" />
        </div>
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body custom-modal-body" id="modalContent7">
        <div class="gridview-container">
          <asp:GridView ID="GridView_oem_summary_modal7" CssClass="table-primary custom-gridview" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true" >
            <Columns>
               <asp:HyperLinkField
                 DataTextField="cp_id"
                 HeaderText="Sighting ID"
                 DataNavigateUrlFields="cp_id"
                 DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
                 Target="_blank" />
              <asp:BoundField DataField="title" HeaderText="title" />
              <asp:BoundField DataField="status" HeaderText="status" />
              <asp:BoundField DataField="component" HeaderText="component" />
              <asp:BoundField DataField="cmf_request" HeaderText="cmf_request" />
              <asp:BoundField DataField="customer_owner" HeaderText="customer_owner" />
              <asp:BoundField DataField="customer_company" HeaderText="customer_company" />
              <asp:BoundField DataField="promoted_id" HeaderText="promoted_id" />
              <asp:BoundField DataField="closed_reason" HeaderText="closed_reason" />
              <asp:BoundField DataField="days_active" HeaderText="days_active" />
              <asp:BoundField DataField="idst" HeaderText="idst" />
              <asp:BoundField DataField="drivers" HeaderText="Milestones" />
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </div>
</div>
<!-- Bootstrap Modal -->
<div class="modal fade" id="detailsModal0" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog custom-modal-dialog" role="document"> <!-- Custom class for modal -->
    <div class="modal-content">
      <div class="modal-header">
                <div class="modal-title-container">
                    <h5 class="modal-title">Issue List</h5>
                    <button type="button" id="btnExportDesignDetailsModal0" class="modern-button2 export-btn" onclick="return exportDesignDetailsModal0ToExcel();">Export to Excel</button>
                </div>
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span>&times;</span>
        </button>
      </div>
      <div class="modal-body custom-modal-body" id="modalContent0">
        <div class="gridview-container">
          <asp:GridView ID="GridView_design_summary_modal0" CssClass="table-primary custom-gridview" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true" OnRowDataBound="GridViewdesign_RowDataBound">
            <Columns>
               <asp:HyperLinkField
     DataTextField="cp_id"
     HeaderText="Sighting ID"
     DataNavigateUrlFields="cp_id"
     DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
     Target="_blank" />
              <asp:BoundField DataField="title" HeaderText="title" />
              <asp:BoundField DataField="status" HeaderText="status" />
              <asp:BoundField DataField="component" HeaderText="component" />
              <asp:BoundField DataField="cmf_request" HeaderText="cmf_request" />
              <asp:BoundField DataField="customer_owner" HeaderText="customer_owner" />
              <asp:BoundField DataField="promoted_id" HeaderText="promoted_id" />
              <asp:BoundField DataField="closed_reason" HeaderText="closed_reason" />
              <asp:BoundField DataField="days_active" HeaderText="days_active" />
              <asp:BoundField DataField="idst" HeaderText="idst" />
              <asp:BoundField DataField="drivers" HeaderText="Milestones" />
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </div>
</div>

<!-- Bootstrap Modal -->
<div class="modal fade" id="detailsModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog custom-modal-dialog" role="document"> <!-- Custom class for modal -->
    <div class="modal-content">
      <div class="modal-header">
        <div class="modal-title-container">
            <h5 class="modal-title">Issue List</h5>
            <asp:Button ID="btnExportDesignToExcel" runat="server" Text="Export to Excel" CssClass="modern-button1 export-btn" OnClientClick="exportToExcel('<%= HiddenModalDesign.Value %>'); return false;" />
        </div>        
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body custom-modal-body" id="modalContent">
        <div class="gridview-container">
          <asp:GridView ID="GridView_design_summary_modal" CssClass="table-primary custom-gridview" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true">
            <Columns>
                         <asp:HyperLinkField
DataTextField="cp_id"
HeaderText="Sighting ID"
DataNavigateUrlFields="cp_id"
DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
Target="_blank" />

              <asp:BoundField DataField="title" HeaderText="title" />
              <asp:BoundField DataField="status" HeaderText="status" />
              <asp:BoundField DataField="component" HeaderText="component" />
              <asp:BoundField DataField="cmf_request" HeaderText="cmf_request" />
              <asp:BoundField DataField="customer_owner" HeaderText="customer_owner" />
              <asp:BoundField DataField="promoted_id" HeaderText="promoted_id" />
              <asp:BoundField DataField="closed_reason" HeaderText="closed_reason" />
              <asp:BoundField DataField="days_active" HeaderText="days_active" />
              <asp:BoundField DataField="idst" HeaderText="idst" />
              <asp:BoundField DataField="drivers" HeaderText="Milestones" />
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </div>
</div>


                                    <div class="gridview-container">
                                            <div class="button-wrapper">
 <asp:Button ID="btnImportPopup" runat="server" Text="Import Excel" CssClass="modern-button2" OnClientClick="openPopup(); return false;" />
</div>
                                        <asp:GridView ID="GridView_component_summary" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false" OnRowDataBound="GridView6_RowDataBound">
                                            <Columns>

                                                <asp:BoundField DataField="Component" HeaderText="Ingredient" />
                                                <asp:BoundField DataField="Total_CMF_Approved" HeaderText="Total CMF_Approved" />
                                                <asp:BoundField DataField="Total_CMF_REJECT" HeaderText="Total CMF_REJECT" />
                                                <asp:BoundField DataField="Issues_in_CMF_ASK" HeaderText="Issues in CMF_ASK" />
                                                <asp:BoundField DataField="CMFOpenPercentage" HeaderText="CMF Open %" />
                                                <asp:BoundField DataField="disp_tpt" HeaderText="CMF disposition TPT" />
                                                <asp:BoundField DataField="crit_tpt" HeaderText="CMF Overall TPT" />
                                                                                                
                                                <asp:BoundField DataField="resolve_tpt" HeaderText="CMF Resolution TPT"  />

                                                <asp:BoundField DataField="Noise" HeaderText="Noise%" />
                                                <asp:BoundField DataField="IntelIssuePercentage" HeaderText="Intel Issue %" />       
                                                <asp:BoundField DataField="ThirdPartyPercentage" HeaderText="3rd Party %" />
                                                <asp:BoundField DataField="CustomerIssuePercentage" HeaderText="Customer Issue %" />                  
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                </div>

<div class="modal fade" id="detailsModalDrivers" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog custom-modal-dialog" role="document">
    <div class="modal-content">

      <div class="modal-header">
        <div class="modal-title-container">
          <h5 class="modal-title">Issue List (by Milestone) — <asp:Label ID="lblDriverName" runat="server" /></h5>
          <asp:Button ID="btnExportDriverIssues" runat="server"
                      Text="Export to Excel"
                      CssClass="modern-button1 export-btn"
                      OnClientClick="exportDriverIssues('<%= HiddenDriverName.Value %>'); return false;" />
        </div>
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <div class="modal-body custom-modal-body">
        <div class="gridview-container">
          <asp:GridView ID="GridView_driver_issues"
                        runat="server"
                        CssClass="table-primary custom-gridview"
                        AutoGenerateColumns="False"
                        EmptyDataText="No issues for this driver">
            <Columns>
            
              <asp:HyperLinkField DataTextField="cp_id"
                                  HeaderText="Sighting ID"
                                  DataNavigateUrlFields="cp_id"
                                  DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
                                  Target="_blank" />
                
              <asp:BoundField DataField="title" HeaderText="Title" />
              <asp:BoundField DataField="component" HeaderText="Component" />
              <%-- Add more columns if you want, e.g. status/component --%>
            </Columns>
          </asp:GridView>
        </div>
      </div>

    </div>
  </div>
</div>

        <div class="modal fade" id="detailsModalDrivers1" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog custom-modal-dialog" role="document">
    <div class="modal-content">

      <div class="modal-header">
        <div class="modal-title-container">
                    <h5 class="modal-title"><asp:Label ID="lblDriverName1" runat="server" /></h5>
        <asp:Button ID="btnExportDriverIssues1" runat="server"
            Text="Export to Excel"
            CssClass="modern-button1 export-btn"
            OnClientClick="exportCMFIssues(getComponentValue(), getDriverValue(), getIssueTypeValue()); return false;" />

        </div>
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>

      <div class="modal-body custom-modal-body">
        <div class="gridview-container">
          <asp:GridView ID="GridView_cmf_issues"
                        runat="server"
                        CssClass="table-primary custom-gridview"
                        AutoGenerateColumns="False"
                        EmptyDataText="No issues for this driver">
            <Columns>
            
                            <asp:HyperLinkField DataTextField="id"
                                                                    HeaderText="ID"
                                                                    DataNavigateUrlFields="id"
                                  DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
                                  Target="_blank" />
                
              <asp:BoundField DataField="title" HeaderText="Title" />
                            <asp:BoundField DataField="customer_detail" HeaderText="Customer Detail" />
              <asp:BoundField DataField="component" HeaderText="Component" />
                            <asp:BoundField DataField="component_group" HeaderText="Component Group" />
                            <asp:BoundField DataField="idst" HeaderText="iDST" />
                            <asp:BoundField DataField="customer_owner" HeaderText="Customer Owner" />
                            <asp:BoundField DataField="repro_on_rvp" HeaderText="RVP Repro" />
                            <asp:BoundField DataField="reproducibility" HeaderText="Reproducibility" />
                            <asp:BoundField DataField="date_cmf_ask" HeaderText="Date CMF Ask" />
                <asp:BoundField DataField="cmf_request" HeaderText="CMF Request" />
                            <asp:BoundField DataField="why_cmf_change" HeaderText="Why CMF Change" />
                            <asp:BoundField DataField="pending_reason" HeaderText="Pending Reason" />
                            <asp:BoundField DataField="justification" HeaderText="Justification" />
              <%-- Add more columns if you want, e.g. status/component --%>
            </Columns>
          </asp:GridView>
        </div>
      </div>

    </div>
  </div>
</div>

<div class="modal fade" id="detailsModalSummaryMetrics" tabindex="-1" role="dialog" aria-hidden="true">
    <div class="modal-dialog custom-modal-dialog" role="document">
        <div class="modal-content">

            <div class="modal-header">
                <div class="modal-title-container">
                    <h5 class="modal-title"><asp:Label ID="lblSummaryMetricIssuesTitle" runat="server" /></h5>
                    <button type="button" id="btnExportSummaryMetricIssues" class="modern-button2 export-btn" onclick="return exportSummaryMetricIssuesToExcel();">Export to Excel</button>
                </div>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body custom-modal-body">
                <div class="gridview-container">
                    <asp:GridView ID="GridView_summaryMetricIssues"
                                                runat="server"
                                                CssClass="table-primary custom-gridview"
                                                AutoGenerateColumns="False"
                                                EmptyDataText="No issues found for this group.">
                        <Columns>
                            <asp:BoundField DataField="Milestone" HeaderText="Milestone" />
                            <asp:BoundField DataField="Progress" HeaderText="Progress" />
                            <asp:HyperLinkField DataTextField="SightingID"
                                                                    HeaderText="SightingID"
                                                                    DataNavigateUrlFields="SightingID"
                                                                    DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
                                                                    Target="_blank"
                                                                    HeaderStyle-HorizontalAlign="Center"
                                                                    ItemStyle-HorizontalAlign="Center" />
                            <asp:BoundField DataField="PromotedID" HeaderText="Promoted ID" />
                            <asp:BoundField DataField="CustomerDetail" HeaderText="Customer Detail" />
                            <asp:BoundField DataField="ImageFreeze" HeaderText="ImageFreeze" />
                            <asp:BoundField DataField="DuplicateDetails" HeaderText="Duplicate ID | Customer Detail | ImageFreezeDate" />
                            <asp:BoundField DataField="CustomerCompany" HeaderText="Customer Company" />
                            <asp:BoundField DataField="Title" HeaderText="Title" />
                            <asp:BoundField DataField="Component" HeaderText="Component" />
                            <asp:BoundField DataField="Ingredient" HeaderText="Ingredient" />
                            <asp:BoundField DataField="Owner" HeaderText="Owner" />
                            <asp:BoundField DataField="PromotedOwner" HeaderText="Promoted_Owner" />
                            <asp:BoundField DataField="RvpRepro" HeaderText="RVP Repro" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:BoundField DataField="Idst" HeaderText="iDST" />
                            <asp:BoundField DataField="Los" HeaderText="LOS" />
                            <asp:BoundField DataField="Processor" HeaderText="Processor" />
                            <asp:BoundField DataField="Impact" HeaderText="Impact" />
                            <asp:BoundField DataField="DaysOpen" HeaderText="Days_Open" />
                            <asp:BoundField DataField="ClosedDetails" HeaderText="Closed Reason / Fixed Version" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </div>
    </div>
</div>

<!-- Bootstrap Modal -->
<div class="modal fade" id="detailsModal2" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog custom-modal-dialog" role="document"> <!-- Custom class for modal -->
    <div class="modal-content">
      <div class="modal-header">
        <div class="modal-title-container">
          <h5 class="modal-title">Issue List</h5>
          <asp:Button ID="btnExportIngredientToExcel" runat="server" Text="Export to Excel" CssClass="modern-button1 export-btn" OnClientClick="exportToExcel2('<%= HiddenModalDesign.Value %>'); return false;" />
        </div>        
        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
          <span aria-hidden="true">&times;</span>
        </button>
      </div>
      <div class="modal-body custom-modal-body" id="modalContent2">
        <div class="gridview-container">
          <asp:GridView ID="GridView_component_summary_modal" CssClass="table-primary custom-gridview" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true">
            <Columns>
                        <asp:HyperLinkField
DataTextField="cp_id"
HeaderText="Sighting ID"
DataNavigateUrlFields="cp_id"
DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
Target="_blank" />
              <asp:BoundField DataField="title" HeaderText="title" />
              <asp:BoundField DataField="status" HeaderText="status" />
              <asp:BoundField DataField="component" HeaderText="component" />
              <asp:BoundField DataField="cmf_request" HeaderText="cmf_request" />
              <asp:BoundField DataField="customer_owner" HeaderText="customer_owner" />
              <asp:BoundField DataField="promoted_id" HeaderText="promoted_id" />
              <asp:BoundField DataField="closed_reason" HeaderText="closed_reason" />
              <asp:BoundField DataField="days_active" HeaderText="days_active" />
              <asp:BoundField DataField="idst" HeaderText="idst" />
              <asp:BoundField DataField="drivers" HeaderText="Milestones" />
            </Columns>
          </asp:GridView>
        </div>
      </div>
    </div>
  </div>
</div>




                                <%--<!-- Display Metrics -->
                                    <div class="gridview-container3">
                                        <asp:GridView ID="GridView_cmf_summary1" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true">
                                            <Columns>
                                                <asp:TemplateField HeaderText="Total">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblTotal" runat="server" Text='<%# Eval("Total") %>'></asp:Label><br />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Closed">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblClosed" runat="server" Text='<%# Eval("Closed") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Implemented">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblImplementedCount" runat="server" Text='<%# Eval("ImplementedCount") %>'></asp:Label><br />
                                                        <asp:Label ID="lblImplementedDetails" runat="server" Text='<%# Eval("ImplementedDetails") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>


                                        </asp:GridView>
                                    </div>
                                    <div class="gridview-container3">
                                        <asp:GridView ID="GridView_cmf_summary" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true" DataKeyNames="Component">
                                            <Columns>
                                                <asp:BoundField DataField="Component" HeaderText="Component" SortExpression="Component" />
                                                <asp:BoundField DataField="Driver_issues" HeaderText="Driver_issues" SortExpression="Driver_issues" ReadOnly="True" />

                                            </Columns>
                                        </asp:GridView>--%>

                                    <!-- Display Metrics -->
                                    <div class="gridview-container-wrapper">
                                        <div class="gridview-container3">
                                            <asp:GridView ID="GridView_cmf_summary1" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true">
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Total">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblTotal" runat="server" Text='<%# Eval("Total") %>'></asp:Label><br />
                                                        </ItemTemplate>
                                                        <ItemStyle Width="20%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Open">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblOpen" runat="server" Text='<%# Eval("Open") %>'></asp:Label>
                                                        </ItemTemplate>
                                                        <ItemStyle Width="20%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Closed">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblClosed" runat="server" Text='<%# Eval("Closed") %>'></asp:Label>
                                                        </ItemTemplate>
                                                        <ItemStyle Width="20%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Implemented">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblImplementedCountAndDup" runat="server" Text='<%# Eval("ImplementedCountAndDup") %>'></asp:Label><br />
                                                            <asp:Label ID="lblImplementedComponents" runat="server" Text='<%# Eval("ImplementedComponentsOnly") %>'></asp:Label>
                                                        </ItemTemplate>
                                                        <ItemStyle Width="70%" />
                                                    </asp:TemplateField>
                                                </Columns>
                                            </asp:GridView>

                                            <asp:GridView ID="GridView_notes" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true" OnRowDataBound="GridView_notes_RowDataBound">
                                                <Columns>
                                                  
                                                    <asp:BoundField DataField="disp_tpt" HeaderText="CMF Disposition TPT" ReadOnly="True" ItemStyle-Width="33%" />
                                                    <asp:BoundField DataField="resolve_tpt" HeaderText="CMF Resolution TPT" ReadOnly="True" ItemStyle-Width="33%" />
                                                    <asp:BoundField DataField="crit_tpt" HeaderText="CMF Overall TPT" ReadOnly="True" ItemStyle-Width="33%" />
                                                </Columns>
                                            </asp:GridView>

                                                                                        

                                          
                                            <!-- Milestone Mapping: Drivers and CMF counts -->
                                            
                                            <asp:GridView ID="GridView_milestone_map"
              runat="server"
              AutoGenerateColumns="False"
              CssClass="table-primary"
              EmptyDataText="No CMF data for milestones"
              Visible="true">
    <Columns>

        <asp:BoundField DataField="Driver" HeaderText="Milestone" ReadOnly="True" ItemStyle-Width="50%" />

       <asp:TemplateField HeaderText="Unique CMF Count">
          <ItemTemplate>
            <a href="javascript:void(0)"
                    data-driver='<%# HttpUtility.HtmlAttributeEncode(Convert.ToString(Eval("Driver") ?? "")) %>'
                    onclick='showDriverIssues(this.getAttribute("data-driver")); return false;'
               style="font-weight: bold; text-decoration: none;"
               class="btn btn-link p-0">
              <%# Eval("CMFCount") %>
            </a>
          </ItemTemplate>
          <ItemStyle Width="50%" />
        </asp:TemplateField>
        
    </Columns>
</asp:GridView>

                                        </div>






                                        <div class="gridview-container3">
                                            <asp:GridView ID="GridView_cmf_summary" CssClass="table-primary7" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true" DataKeyNames="Component" OnRowCreated="GridView_cmf_summary_RowCreated">
                                                <Columns>
                                                    <asp:BoundField DataField="Component" HeaderText="Component_Open_CMFs" SortExpression="Component" ReadOnly="True" ItemStyle-Width="25%"/>
                                                    <asp:BoundField DataField="Driver_issues" HeaderText="Driver_issues" SortExpression="Driver_issues" ReadOnly="True" ItemStyle-Width="25%"/>
                                                </Columns>
                                            </asp:GridView>

                                                                                        <asp:GridView ID="GridView_comp" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No CMFs Pending" Visible="true">
    <Columns>
      
        <asp:BoundField DataField="component_group" HeaderText="Component" ReadOnly="True" ItemStyle-Width="50%" />
        <asp:BoundField DataField="CMF Pending Count" HeaderText="CMF Pending Count" ReadOnly="True" ItemStyle-Width="50%" />
    </Columns>
</asp:GridView>

                                                                                                                                    <div class="tptdefinitions" id="tptdefdiv" runat="server"> 
                                                  <p><strong>CMF resolution TPT:</strong> CMF decided date - Implemented date</p>
                                                  <p><strong>CMF disposition TPT:</strong> CMF decided date - CMF ask date</p>
                                                </div>
                                           
                                        </div>
                                                                                                   
                                    </div>


                                                                                                                                

                                </div>


                   

                                </div>
                                                                                                                                                              
                            
                            </ContentTemplate>
                        </asp:UpdatePanel>

                    </div>

                </div>
                    </div>
                </div>
            </form>

        </div>
                                                                                                                                                                 
    </div>
   

</body>
</html>
