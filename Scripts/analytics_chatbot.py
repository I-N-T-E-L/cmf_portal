import argparse
import json
import os
import re
import time
import urllib.error
import urllib.request
from textwrap import wrap

# Try to import pandas and matplotlib at module level
try:
    import pandas as pd
    HAS_PANDAS = True
except ImportError:
    HAS_PANDAS = False
    pd = None

try:
    import matplotlib.pyplot as plt
except ImportError:
    plt = None

_PLATFORM_ALIAS_CACHE = None


def _fail(message):
    print(json.dumps({"success": False, "message": message, "image_file": "", "report_file": ""}))


def _safe_name(prefix, ext):
    return f"{prefix}_{int(time.time() * 1000)}.{ext}"


def _tokenize(text):
    return re.findall(r"[a-z0-9_]+", (text or "").lower())


def _normalize_intent(intent):
    value = (intent or "").strip().lower()
    if value in {"qa", "question", "query"}:
        return "rag_qa"
    return value


def _contains_any(text, candidates):
    value = (text or "").lower()
    return any(candidate in value for candidate in candidates)


def _contains_whole_alias(text, alias):
    if not text or not alias:
        return False
    escaped = re.escape((alias or "").strip().lower()).replace(r"\ ", r"[\s_-]+")
    if not escaped:
        return False
    return re.search(r"(?<![a-z0-9])" + escaped + r"(?![a-z0-9])", text, flags=re.IGNORECASE) is not None


def _normalize_codes(raw_codes):
    values = raw_codes if isinstance(raw_codes, list) else [raw_codes]
    normalized = []
    for value in values:
        code = _canonical_platform_code(value)
        if code and code not in normalized:
            normalized.append(code)
    return normalized


def _load_platform_alias_payload():
    global _PLATFORM_ALIAS_CACHE
    if _PLATFORM_ALIAS_CACHE is not None:
        return _PLATFORM_ALIAS_CACHE

    payload = {
        "alias_to_codes": {},
        "group_alias_to_codes": {},
    }

    raw = (os.environ.get("CMF_PLATFORM_ALIAS_MAP_JSON", "") or "").strip()
    if raw:
        try:
            parsed = json.loads(raw)
            if isinstance(parsed, dict):
                alias_map = parsed.get("alias_to_codes")
                if isinstance(alias_map, dict):
                    for alias, codes in alias_map.items():
                        normalized_alias = (alias or "").strip().lower()
                        normalized_codes = _normalize_codes(codes)
                        if normalized_alias and normalized_codes:
                            payload["alias_to_codes"][normalized_alias] = normalized_codes

                group_map = parsed.get("group_alias_to_codes")
                if isinstance(group_map, dict):
                    for alias, codes in group_map.items():
                        normalized_alias = (alias or "").strip().lower()
                        normalized_codes = _normalize_codes(codes)
                        if normalized_alias and normalized_codes:
                            payload["group_alias_to_codes"][normalized_alias] = normalized_codes
        except Exception:
            payload = {
                "alias_to_codes": {},
                "group_alias_to_codes": {},
            }

    _PLATFORM_ALIAS_CACHE = payload
    return _PLATFORM_ALIAS_CACHE


def _platform_keywords():
    payload = _load_platform_alias_payload()
    keywords = []

    for alias, codes in payload["group_alias_to_codes"].items():
        if alias and alias not in keywords:
            keywords.append(alias)
        for code in codes:
            if code and code not in keywords:
                keywords.append(code)

    for alias, codes in payload["alias_to_codes"].items():
        if alias and alias not in keywords:
            keywords.append(alias)
        for code in codes:
            if code and code not in keywords:
                keywords.append(code)

    return keywords


def _validate_cmf_prompt(prompt, context):
    prompt_lower = (prompt or "").strip().lower()
    if not prompt_lower:
        return False, "Please enter a CMF-related question."

    dynamic_platform_keywords = _platform_keywords()
    portal_keywords = [
        "cmf", "issue", "issues", "sighting", "defect", "status", "owner", "component", "driver",
        "platform", "milestone", "customer", "resolved", "implemented", "verified", "closed", "open",
        "pending", "chart", "report", "analytics", "trend", "summary", "stale", "days", "count",
    ]
    portal_keywords.extend(dynamic_platform_keywords)
    non_portal_keywords = [
        "song", "movie", "weather", "recipe", "travel", "joke", "poem", "celebrity",
        "football", "basketball", "cricket", "stock price", "bitcoin", "horoscope",
    ]

    # Allow clear follow-up prompts when prior context exists.
    has_context = bool((context or {}).get("conversation_history"))
    if has_context and _is_followup_prompt(prompt_lower):
        return True, ""

    if _contains_any(prompt_lower, non_portal_keywords):
        return False, (
            "I can only help with CMF-related issue analysis. "
            "Please ask about CMF issues, platforms, owners, components, trends, or reports."
        )

    if not _contains_any(prompt_lower, portal_keywords):
        return False, (
            "I can only help with CMF-related issue analysis. "
            "Please ask about CMF issues, platforms, owners, components, trends, or reports."
        )

    return True, ""


def _canonical_platform_code(value):
    token = (value or "").strip().lower()
    if not token:
        return ""
    token = token.replace(" ", "_").replace("-", "_")
    return token


def _extract_platform_tokens(text):
    value = (text or "").lower()
    if not value:
        return []

    payload = _load_platform_alias_payload()
    group_alias_to_codes = payload["group_alias_to_codes"]
    alias_to_codes = payload["alias_to_codes"]

    normalized = []
    for alias, codes in group_alias_to_codes.items():
        if _contains_whole_alias(value, alias):
            for code in codes:
                if code and code not in normalized:
                    normalized.append(code)

    for alias, codes in alias_to_codes.items():
        if _contains_whole_alias(value, alias):
            for code in codes:
                if code and code not in normalized:
                    normalized.append(code)

    return normalized


def _extract_status_tokens(text):
    prompt_lower = (text or "").lower()
    aliases = {
        "open": ["open", "opened"],
        "implemented": ["implemented", "implementation"],
        "verified": ["verified", "verification"],
        "complete": ["complete", "completed", "closed", "resolved"],
        "rejected": ["rejected", "reject"],
        "pending": ["pending", "in progress", "wip"],
    }

    matched = []
    for canonical, words in aliases.items():
        if any(word in prompt_lower for word in words):
            matched.append(canonical)
    return matched


def _looks_like_platform_breakdown_question(prompt):
    prompt_lower = (prompt or "").lower()
    platform_phrases = [
        "which platform",
        "what platform",
        "platform breakdown",
        "platform wise",
        "across platforms",
        "across platform",
        "by platform",
        "platform distribution",
        "platform split",
    ]
    return _contains_any(prompt_lower, platform_phrases)


def _is_top_customer_question(prompt):
    prompt_lower = (prompt or "").lower()
    return _contains_any(prompt_lower, ["top customer", "highest customer", "most issues customer", "largest customer"]) or (
        "customer" in prompt_lower and _contains_any(prompt_lower, ["top", "highest", "most"])
    )


def _is_top_owner_question(prompt):
    prompt_lower = (prompt or "").lower()
    return _contains_any(prompt_lower, ["top owner", "highest owner", "most issues owner"]) or (
        "owner" in prompt_lower and _contains_any(prompt_lower, ["top", "highest", "most"])
    )


def _is_count_question(prompt):
    prompt_lower = (prompt or "").lower()
    return _contains_any(
        prompt_lower,
        ["how many", "count", "total", "number of", "records", "rows", "issues"],
    )


def _is_graph_request(prompt):
    prompt_lower = (prompt or "").lower()
    return _contains_any(prompt_lower, ["chart", "graph", "plot", "visual", "bar", "line", "compare graph", "comparison graph"])


def _count_by_platform(df, requested_platforms, fallback_platform=""):
    platform_col = "Platform" if "Platform" in df.columns else "platform"
    if platform_col not in df.columns:
        total = len(df)
        if fallback_platform:
            return [(fallback_platform, total)]
        return [("current dataset", total)]

    platform_series = (
        df[platform_col]
        .fillna("UNKNOWN")
        .astype(str)
        .str.strip()
        .replace("", "UNKNOWN")
    )

    canonical_series = platform_series.str.lower().str.replace(" ", "_", regex=False).str.replace("-", "_", regex=False)

    if requested_platforms:
        result = []
        for token in requested_platforms:
            mask = canonical_series == token
            count = int(mask.sum())
            label = token.upper()
            result.append((label, count))
        return result

    if fallback_platform:
        token = _canonical_platform_code(fallback_platform)
        if token:
            count = int((canonical_series == token).sum())
            return [(token.upper(), count)]

    return [("current dataset", int(len(df)))]


def _build_platform_comparison_chart(df, requested_platforms, output_dir):
    platform_col = "Platform" if "Platform" in df.columns else "platform"
    if platform_col not in df.columns or not requested_platforms:
        return ""

    platform_series = (
        df[platform_col]
        .fillna("UNKNOWN")
        .astype(str)
        .str.strip()
    )
    canonical_series = platform_series.str.lower().str.replace(" ", "_", regex=False).str.replace("-", "_", regex=False)

    labels = []
    values = []
    for token in requested_platforms:
        labels.append(token.upper())
        values.append(int((canonical_series == token).sum()))

    if not labels:
        return ""

    image_file = _safe_name("platform_comparison_chart", "html")
    ok = _generate_html_bar_chart(
        "Platform Issue Count Comparison",
        labels,
        values,
        os.path.join(output_dir, image_file),
        "#1f77b4",
    )
    return image_file if ok else ""


def _parse_context_json(raw):
    if not raw:
        return {}

    try:
        parsed = json.loads(raw)
        return parsed if isinstance(parsed, dict) else {}
    except (TypeError, ValueError):
        return {}


def _sanitize_text(value):
    if value is None:
        return ""
    return str(value).replace("\n", " ").replace("\r", " ").strip()


def _read_csv_simple(csv_path, max_rows=2000):
    """Fallback CSV reader when pandas is not available"""
    import csv
    rows = []
    try:
        with open(csv_path, 'r', encoding='utf-8-sig') as f:
            reader = csv.DictReader(f)
            for i, row in enumerate(reader):
                if i >= max_rows:
                    break
                rows.append(row)
    except Exception:
        pass
    return rows


def _generate_html_bar_chart(title, labels, values, output_path, color="#0068b5"):
    """Generate an interactive Plotly HTML bar chart."""
    html_content = f"""
    <!DOCTYPE html>
    <html>
    <head>
        <title>{title}</title>
        <script src="https://cdn.plot.ly/plotly-2.35.2.min.js"></script>
        <style>
            body {{ font-family: Segoe UI, sans-serif; margin: 20px; color: #20354a; }}
            .container {{ max-width: 1000px; margin: auto; }}
            #chart {{ width: 100%; height: 560px; }}
        </style>
    </head>
    <body>
        <div class="container">
            <h1>{title}</h1>
            <div id="chart"></div>
        </div>
        <script>
            Plotly.newPlot('chart', [{{
                type: 'bar',
                x: {json.dumps(labels)},
                y: {json.dumps(values)},
                marker: {{ color: '{color}' }},
                hovertemplate: '%{{x}}<br>Issues: %{{y}}<extra></extra>'
            }}], {{
                margin: {{ t: 20, r: 20, b: 100, l: 60 }},
                yaxis: {{ rangemode: 'tozero', title: 'Issue Count' }},
                xaxis: {{ tickangle: -35 }},
                plot_bgcolor: '#ffffff',
                paper_bgcolor: '#ffffff'
            }}, {{ responsive: true }});
        </script>
    </body>
    </html>
    """
    try:
        with open(output_path, 'w') as f:
            f.write(html_content)
        return True
    except Exception:
        return False


def _generate_html_horizontal_bar_chart(title, labels, values, output_path, color="#00a6b2"):
    """Generate an interactive Plotly horizontal bar chart."""
    html_content = f"""
    <!DOCTYPE html>
    <html>
    <head>
        <title>{title}</title>
        <script src="https://cdn.plot.ly/plotly-2.35.2.min.js"></script>
        <style>
            body {{ font-family: Segoe UI, sans-serif; margin: 20px; color: #20354a; }}
            .container {{ max-width: 1000px; margin: auto; }}
            #chart {{ width: 100%; height: 560px; }}
        </style>
    </head>
    <body>
        <div class="container">
            <h1>{title}</h1>
            <div id="chart"></div>
        </div>
        <script>
            Plotly.newPlot('chart', [{{
                type: 'bar',
                orientation: 'h',
                y: {json.dumps(labels[::-1])},
                x: {json.dumps(values[::-1])},
                marker: {{ color: '{color}' }},
                hovertemplate: '%{{y}}<br>Issues: %{{x}}<extra></extra>'
            }}], {{
                margin: {{ t: 20, r: 20, b: 60, l: 180 }},
                xaxis: {{ rangemode: 'tozero', title: 'Issue Count' }},
                plot_bgcolor: '#ffffff',
                paper_bgcolor: '#ffffff'
            }}, {{ responsive: true }});
        </script>
    </body>
    </html>
    """
    try:
        with open(output_path, 'w') as f:
            f.write(html_content)
        return True
    except Exception:
        return False


def _pdf_escape(text):
    return (text or '').replace('\\', '\\\\').replace('(', '\\(').replace(')', '\\)')


def _write_simple_pdf(output_path, title, lines):
    safe_lines = []
    for line in lines:
        if not line:
            safe_lines.append('')
            continue
        safe_lines.extend(wrap(str(line), width=92) or [''])

    pages = [safe_lines[i:i + 44] for i in range(0, len(safe_lines), 44)] or [[]]
    objects = []
    page_refs = []

    def add_object(content):
        objects.append(content)
        return len(objects)

    font_ref = add_object('<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>')
    pages_ref_placeholder = 0
    for page_lines in pages:
        y = 780
        stream_lines = ['BT', '/F1 16 Tf', f'50 {y} Td', f'({_pdf_escape(title)}) Tj']
        stream_lines.extend(['/F1 10 Tf', '0 -24 Td'])
        first = True
        for line in page_lines:
            if first:
                first = False
            else:
                stream_lines.append('0 -14 Td')
            stream_lines.append(f'({_pdf_escape(line)}) Tj')
        stream_lines.append('ET')
        stream = '\n'.join(stream_lines)
        content_ref = add_object(f'<< /Length {len(stream.encode("latin-1", "replace"))} >>\nstream\n{stream}\nendstream')
        page_ref = add_object(f'<< /Type /Page /Parent PAGES_REF 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 {font_ref} 0 R >> >> /Contents {content_ref} 0 R >>')
        page_refs.append(page_ref)

    kids = ' '.join(f'{ref} 0 R' for ref in page_refs)
    pages_ref = add_object(f'<< /Type /Pages /Kids [{kids}] /Count {len(page_refs)} >>')
    catalog_ref = add_object(f'<< /Type /Catalog /Pages {pages_ref} 0 R >>')

    rendered = []
    offsets = [0]
    rendered.append('%PDF-1.4\n')
    for index, content in enumerate(objects, start=1):
        content = content.replace('PAGES_REF', str(pages_ref))
        offsets.append(sum(len(part.encode('latin-1', 'replace')) for part in rendered))
        rendered.append(f'{index} 0 obj\n{content}\nendobj\n')
    xref_offset = sum(len(part.encode('latin-1', 'replace')) for part in rendered)
    rendered.append(f'xref\n0 {len(objects) + 1}\n0000000000 65535 f \n')
    for offset in offsets[1:]:
        rendered.append(f'{offset:010d} 00000 n \n')
    rendered.append(f'trailer\n<< /Size {len(objects) + 1} /Root {catalog_ref} 0 R >>\nstartxref\n{xref_offset}\n%%EOF')

    with open(output_path, 'wb') as pdf_file:
        pdf_file.write(''.join(rendered).encode('latin-1', 'replace'))


def _template_report_lines(template_text, df, platform):
    week_label = time.strftime('WW%U')
    populated = (template_text or '').replace('{{platform}}', (platform or '').upper()).replace('{{week}}', week_label)
    template_sections = _extract_template_sections(populated)
    lines = []

    if template_sections:
        for heading, guidance in template_sections:
            if lines:
                lines.append('')
            lines.append(_strip_heading_guidance(heading))
            lines.extend(_content_for_template_heading(heading, guidance, df, platform))
    else:
        lines = [line.strip() for line in populated.splitlines() if line.strip()]

    lines.extend(['', 'Live CMF Data Snapshot'])
    if HAS_PANDAS:
        total = len(df)
        status_counts = df['status'].fillna('unknown').astype(str).value_counts().head(6).to_dict() if 'status' in df.columns else {}
        owner_col = 'Owner' if 'Owner' in df.columns else ('owner' if 'owner' in df.columns else '')
        top_owners = df[owner_col].fillna('Unassigned').astype(str).value_counts().head(5).to_dict() if owner_col else {}
        stale = int((pd.to_numeric(df['days_active'], errors='coerce').fillna(0) > 14).sum()) if 'days_active' in df.columns else 0
        lines.append(f'Total rows evaluated: {total}')
        lines.append('Status mix: ' + (', '.join(f'{k}: {v}' for k, v in status_counts.items()) or 'status data unavailable'))
        lines.append('Top owners: ' + (', '.join(f'{k}: {v}' for k, v in top_owners.items()) or 'owner data unavailable'))
        lines.append(f'Stale issues over 14 days: {stale}')
        lines.extend(['', 'Sample Issues'])
        for _, row in df.head(8).iterrows():
            issue_id = str(row.get('cp_id', row.get('SightingID', 'N/A')))
            title = str(row.get('title', 'Untitled')).replace('\n', ' ')
            status = str(row.get('status', row.get('Status', 'unknown')))
            lines.append(f'- {issue_id}: {title[:110]} [{status}]')
    else:
        lines.append(f'Total rows evaluated: {len(df)}')
        for row in df[:8]:
            lines.append(f"- {row.get('cp_id', 'N/A')}: {str(row.get('title', 'Untitled'))[:110]}")
    return lines


def _extract_template_sections(template_text):
    sections = []
    current_heading = ''
    current_body = []

    for raw_line in (template_text or '').splitlines():
        line = raw_line.strip()
        if not line:
            continue

        heading = _normalize_template_heading(line)
        if heading:
            if current_heading:
                sections.append((current_heading, ' '.join(current_body).strip()))
            current_heading = heading
            current_body = []
        elif current_heading:
            current_body.append(line)

    if current_heading:
        sections.append((current_heading, ' '.join(current_body).strip()))

    return sections


def _normalize_template_heading(line):
    cleaned = re.sub(r'^#{1,6}\s*', '', line or '').strip()
    cleaned = re.sub(r'^[\-\*]\s+', '', cleaned).strip()
    cleaned = re.sub(r'^\d+[\.)]\s+', '', cleaned).strip()
    cleaned = cleaned.rstrip(':').strip()
    if not cleaned:
        return ''

    word_count = len(cleaned.split())
    looks_like_heading = word_count <= 12 and (
        (line or '').lstrip().startswith('#')
        or (line or '').rstrip().endswith(':')
        or re.match(r'^\s*(?:\d+[\.)]|[-*])\s+', line or '')
    )
    return cleaned if looks_like_heading else ''


def _strip_heading_guidance(heading):
    return re.sub(r'\s*[\(\[][^\)\]]+[\)\]]\s*$', '', heading or '').strip() or heading


def _content_for_template_heading(heading, guidance, df, platform):
    heading_lower = (heading or '').lower()
    guidance_from_heading = ' '.join(re.findall(r'[\(\[]([^\)\]]+)[\)\]]', heading or ''))
    guidance_lower = ((guidance or '') + ' ' + guidance_from_heading).lower()
    combined = heading_lower + ' ' + guidance_lower
    total = len(df)

    if not HAS_PANDAS:
        return [f'- {total} live CMF rows were available for this section.']

    status_counts = df['status'].fillna('unknown').astype(str).value_counts().head(5).to_dict() if 'status' in df.columns else {}
    owner_col = 'Owner' if 'Owner' in df.columns else ('owner' if 'owner' in df.columns else '')
    top_owners = df[owner_col].fillna('Unassigned').astype(str).value_counts().head(3).to_dict() if owner_col else {}
    component_col = 'component' if 'component' in df.columns else ('Component' if 'Component' in df.columns else '')
    top_components = df[component_col].fillna('Unknown').astype(str).value_counts().head(3).to_dict() if component_col else {}
    stale = int((pd.to_numeric(df['days_active'], errors='coerce').fillna(0) > 14).sum()) if 'days_active' in df.columns else 0
    high_impact = 0
    if 'impact' in df.columns:
        impact_text = df['impact'].fillna('').astype(str).str.lower()
        high_impact = int(impact_text.str.contains('critical|high|blocker|showstopper|crash|hang', regex=True).sum())

    detail_lines = _template_detail_rows(combined, df)
    if detail_lines:
        return detail_lines

    if any(token in combined for token in ['summary', 'overview', 'executive']):
        return [
            f'- {platform.upper() or "Selected platform"} has {total} issues in the live dataset used for this report.',
            '- Status mix: ' + (', '.join(f'{k}: {v}' for k, v in status_counts.items()) or 'status data unavailable') + '.',
            f'- {stale} issues are older than 14 days and may need follow-up attention.'
        ]

    if any(token in combined for token in ['status', 'progress', 'state']):
        return ['- ' + (', '.join(f'{k}: {v}' for k, v in status_counts.items()) or 'Status data was not available.')]

    if any(token in combined for token in ['owner', 'action', 'responsible']):
        return ['- Top owners by issue volume: ' + (', '.join(f'{k}: {v}' for k, v in top_owners.items()) or 'owner data unavailable') + '.']

    if any(token in combined for token in ['customer', 'customers', 'account']):
        if 'customer_company' in df.columns:
            top_customers = df['customer_company'].fillna('Unassigned').astype(str).str.strip().replace('', 'Unassigned').value_counts().head(5).to_dict()
        else:
            top_customers = {}
        return ['- Top customers by issue volume: ' + (', '.join(f'{k}: {v}' for k, v in top_customers.items()) or 'customer data unavailable') + '.']

    if any(token in combined for token in ['component', 'area', 'module']):
        return ['- Most represented components: ' + (', '.join(f'{k}: {v}' for k, v in top_components.items()) or 'component data unavailable') + '.']

    if any(token in combined for token in ['risk', 'impact', 'priority', 'stale', 'aging']):
        return [
            f'- {high_impact} issues include high-impact wording such as critical, blocker, crash, hang, or showstopper.',
            f'- {stale} issues have been active for more than 14 days.'
        ]

    if any(token in combined for token in ['recommend', 'next', 'action', 'follow']):
        return [
            '- Prioritize high-impact and stale issues first, then confirm owners for unassigned or unclear records.',
            '- Re-run this report after filters or platform scope change so the section data stays aligned to the requested view.'
        ]

    return [
        f'- This section was populated from {total} live CMF records for {platform.upper() or "the selected platform"}.',
        '- Key signals: ' + (', '.join(f'{k}: {v}' for k, v in status_counts.items()) or 'status data unavailable') + '.'
    ]


def _template_detail_rows(combined, df):
    if not HAS_PANDAS or df is None or len(df) == 0:
        return []

    wants_issue_rows = any(token in combined for token in [
        'sighting', 'sightings', 'issue list', 'issues list', 'defect list', 'id,', 'ids', 'replication', 'repro', 'impact'
    ])
    wants_cmf_tagged = any(token in combined for token in [
        'new cmf tagged', 'newly cmf tagged', 'newly tagged', 'cmf tagged', 'tagged sightings', 'tagged issues'
    ])
    if not wants_issue_rows and not wants_cmf_tagged:
        return []

    subset = df.copy()
    if wants_cmf_tagged and 'cmf_request' in subset.columns:
        cmf_text = subset['cmf_request'].fillna('').astype(str).str.lower()
        subset = subset[cmf_text.str.contains('cmf_ok|cmf_duplicate|cmf ask|cmf_ask|cmf_incomplete', regex=True)]

    sort_col = _first_existing_column(subset, ['date_cmf_ask', 'SubmittedDate', 'submitted_date', 'created_date', 'days_active'])
    if sort_col:
        if sort_col == 'days_active':
            subset = subset.assign(_sort_days=pd.to_numeric(subset[sort_col], errors='coerce').fillna(-1)).sort_values('_sort_days', ascending=False)
        else:
            subset = subset.assign(_sort_date=pd.to_datetime(subset[sort_col], errors='coerce')).sort_values('_sort_date', ascending=False, na_position='last')

    fields = _requested_template_fields(combined)
    if not fields:
        fields = ['id', 'issue', 'replication rate', 'impact'] if wants_cmf_tagged else ['id', 'issue', 'status', 'impact']

    lines = []
    label = 'new/active CMF-tagged sightings' if wants_cmf_tagged else 'matching sightings'
    lines.append(f'- Showing {min(len(subset), 8)} {label} from {len(subset)} matching records.')

    if len(subset) == 0:
        lines.append('- No matching records were found for this section in the selected report data.')
        return lines

    for _, row in subset.head(8).iterrows():
        pieces = []
        for field in fields[:6]:
            value = _template_field_value(row, field)
            if value:
                pieces.append(f'{_template_field_label(field)}: {value}')
        if pieces:
            lines.append('- ' + '; '.join(pieces))

    return lines


def _requested_template_fields(text):
    fields = []
    mapping = [
        ('id', ['id', 'hsd', 'sighting']),
        ('issue', ['issue', 'title', 'description']),
        ('replication rate', ['replication rate', 'replication', 'repro rate', 'reproducibility', 'repro', 'rvp']),
        ('impact', ['impact', 'customer impact']),
        ('status', ['status', 'state']),
        ('owner', ['owner', 'responsible']),
        ('component', ['component', 'module']),
        ('platform', ['platform']),
        ('cmf request', ['cmf request', 'request state']),
        ('priority', ['priority']),
        ('date', ['date', 'recently tagged', 'tagged date']),
    ]
    for canonical, aliases in mapping:
        if any(alias in text for alias in aliases) and canonical not in fields:
            fields.append(canonical)
    return fields


def _template_field_label(field):
    labels = {
        'id': 'ID',
        'issue': 'Issue',
        'replication rate': 'Replication',
        'impact': 'Impact',
        'cmf request': 'CMF request',
    }
    return labels.get(field, field[:1].upper() + field[1:])


def _template_field_value(row, field):
    candidates = {
        'id': ['SightingID', 'cp_id', 'id'],
        'issue': ['title', 'Issue', 'issue'],
        'replication rate': ['reproducibility', 'repro_on_rvp', 'idst'],
        'impact': ['impact', 'customer_impact'],
        'status': ['status', 'Status'],
        'owner': ['Owner', 'owner', 'customer_owner'],
        'component': ['component', 'component_group'],
        'platform': ['Platform', 'platform'],
        'cmf request': ['cmf_request'],
        'priority': ['priority'],
        'date': ['date_cmf_ask', 'SubmittedDate', 'submitted_date', 'created_date'],
    }.get(field, [])

    for column in candidates:
        if column in row.index:
            value = _sanitize_text(row.get(column, ''))
            if value and value.lower() not in {'nan', 'none', 'null'}:
                return value[:140]
    return ''


def _first_existing_column(df, candidates):
    for column in candidates:
        if column in df.columns:
            return column
    return ''


def _generate_html_histogram(title, values, bins=18, output_path="", color="#bc2f32"):
    """Generate an interactive Plotly histogram."""
    if not values:
        return False
    
    try:
        # Simple binning logic
        values_list = [v for v in values if v is not None and v != ""]
        if not values_list:
            return False
        
        values_float = []
        for v in values_list:
            try:
                values_float.append(float(v))
            except:
                pass
        
        if not values_float:
            return False
        
        min_val = min(values_float)
        max_val = max(values_float)
        bin_width = (max_val - min_val + 1) / bins if max_val > min_val else 1
        
        bin_counts = [0] * bins
        bin_labels = []
        
        for i in range(bins):
            bin_start = min_val + i * bin_width
            bin_end = bin_start + bin_width
            bin_labels.append(f"{bin_start:.1f}-{bin_end:.1f}")
            
            for val in values_float:
                if i == bins - 1:
                    if bin_start <= val <= bin_end:
                        bin_counts[i] += 1
                else:
                    if bin_start <= val < bin_end:
                        bin_counts[i] += 1
        
        html_content = f"""
        <!DOCTYPE html>
        <html>
        <head>
            <title>{title}</title>
            <script src="https://cdn.plot.ly/plotly-2.35.2.min.js"></script>
            <style>
                body {{ font-family: Segoe UI, sans-serif; margin: 20px; color: #20354a; }}
                .container {{ max-width: 1000px; margin: auto; }}
                #chart {{ width: 100%; height: 560px; }}
            </style>
        </head>
        <body>
            <div class="container">
                <h1>{title}</h1>
                <div id="chart"></div>
            </div>
            <script>
                Plotly.newPlot('chart', [{{
                    type: 'bar',
                    x: {json.dumps(bin_labels)},
                    y: {json.dumps(bin_counts)},
                    marker: {{ color: '{color}' }},
                    hovertemplate: '%{{x}} days<br>Issues: %{{y}}<extra></extra>'
                }}], {{
                    margin: {{ t: 20, r: 20, b: 100, l: 60 }},
                    yaxis: {{ rangemode: 'tozero', title: 'Issue Count' }},
                    xaxis: {{ title: 'Days Active Bucket', tickangle: -35 }},
                    plot_bgcolor: '#ffffff',
                    paper_bgcolor: '#ffffff'
                }}, {{ responsive: true }});
            </script>
        </body>
        </html>
        """
        with open(output_path, 'w') as f:
            f.write(html_content)
        return True
    except Exception:
        return False


def _sanitize_text(value):
    if value is None:
        return ""
    return str(value).replace("\n", " ").replace("\r", " ").strip()


def _build_search_text(df):
    cols = [
        "SightingID",
        "title",
        "status",
        "sysdebug",
        "customer_company",
        "component_group",
        "component",
        "Owner",
        "drivers",
        "idst",
        "Platform",
        "platform",
    ]
    existing = [col for col in cols if col in df.columns]
    if not existing:
        return [""] * len(df)

    return (
        df[existing]
        .fillna("")
        .astype(str)
        .agg(" | ".join, axis=1)
        .str.lower()
        .tolist()
    )


def _retrieve_rows(df, prompt, top_k=25):
    prompt_tokens = _tokenize(prompt)
    if not prompt_tokens:
        return df.head(top_k).copy(), [""] * min(top_k, len(df))

    search_text = _build_search_text(df)
    token_set = set(prompt_tokens)

    scores = []
    for text in search_text:
        score = 0
        for token in token_set:
            if token in text:
                score += 2
        if (prompt or "").lower() in text:
            score += 5
        scores.append(score)

    ranked = sorted(enumerate(scores), key=lambda pair: pair[1], reverse=True)
    chosen_indices = [index for index, score in ranked if score > 0][:top_k]

    if not chosen_indices:
        chosen_indices = list(range(min(top_k, len(df))))

    retrieved = df.iloc[chosen_indices].copy()
    retrieved_scores = [scores[index] for index in chosen_indices]
    return retrieved, retrieved_scores


def _retrieve_rows_semantic(df, prompt, top_k=30):
    """Retrieve rows using improved semantic scoring (TF-IDF-like)"""
    if len(df) == 0 or not prompt:
        return df.head(top_k).copy()
    
    prompt_tokens = _tokenize(prompt)
    if not prompt_tokens:
        return df.head(top_k).copy()
    
    search_text = _build_search_text(df)
    
    # Calculate term frequencies for this prompt.
    token_freq = {}
    for token in prompt_tokens:
        token_freq[token] = token_freq.get(token, 0) + 1
    
    # Calculate scores with improved weighting
    scores = []
    for doc_text in search_text:
        doc_tokens = _tokenize(doc_text)
        score = 0.0
        
        # TF-like scoring: weight tokens by their frequency in prompt
        for token in prompt_tokens:
            if token in doc_tokens:
                # Weight by prompt frequency and doc occurrence
                tf_weight = min(3, token_freq.get(token, 1))  # Cap at 3
                doc_freq = doc_tokens.count(token)
                score += (tf_weight * (1 + doc_freq))
        
        # Bonus for exact phrase matches, capped to avoid runaway scores.
        if prompt.lower() in doc_text:
            score += min(20.0, len(prompt) * 0.1)
        
        scores.append(score)
    
    # Select top_k by score
    ranked = sorted(enumerate(scores), key=lambda x: x[1], reverse=True)
    chosen_indices = [i for i, s in ranked[:top_k]]
    
    if not chosen_indices:
        chosen_indices = list(range(min(top_k, len(df))))
    
    if HAS_PANDAS:
        return df.iloc[chosen_indices].copy()
    else:
        return [df[i] for i in chosen_indices]


def _format_issue_line(row):
    sighting = _sanitize_text(row.get("SightingID", "N/A"))
    company = _sanitize_text(row.get("customer_company", "Unassigned"))
    title = _sanitize_text(row.get("title", ""))
    status = _sanitize_text(row.get("status", ""))
    owner = _sanitize_text(row.get("Owner", ""))
    return f"- {sighting} | {company} | {status} | {owner} | {title}"


def _detect_companies(df, prompt):
    if "customer_company" not in df.columns:
        return []

    prompt_lower = (prompt or "").lower()
    companies = (
        df["customer_company"].fillna("").astype(str).str.strip().str.lower().unique().tolist()
    )
    companies = [name for name in companies if name]

    matched = sorted(set([name for name in companies if name in prompt_lower]), key=lambda name: len(name), reverse=True)

    # Common brand alias extraction; merge with exact matches to improve comparisons like hp vs lenovo.
    aliases = ["lenovo", "hp", "dell", "asus", "acer", "msi", "samsung"]
    alias_hits = [alias for alias in aliases if alias in prompt_lower]

    combined = []
    for name in matched + alias_hits:
        if name not in combined:
            combined.append(name)

    return combined[:5]


def _company_filter(df, company_name):
    if "customer_company" not in df.columns:
        return df.iloc[0:0]
    token = (company_name or "").strip().lower()
    return df[df["customer_company"].fillna("").astype(str).str.lower().str.contains(token, regex=False)]


def _detect_owners(df, prompt):
    owner_col = "Owner" if "Owner" in df.columns else ("owner" if "owner" in df.columns else "")
    if not owner_col:
        return []

    prompt_lower = (prompt or "").lower()
    owners = (
        df[owner_col].fillna("").astype(str).str.strip().str.lower().unique().tolist()
    )
    owners = [name for name in owners if name]
    matched = sorted(set([name for name in owners if name in prompt_lower]), key=lambda name: len(name), reverse=True)
    return matched[:5]


def _owner_filter(df, owner_name):
    owner_col = "Owner" if "Owner" in df.columns else ("owner" if "owner" in df.columns else "")
    if not owner_col:
        return df.iloc[0:0]
    token = (owner_name or "").strip().lower()
    return df[df[owner_col].fillna("").astype(str).str.lower().str.contains(token, regex=False)]


def _extract_primary_entity_from_message(message):
    match = re.search(r"-\s*([^:\n]+):\s*\d+\s+issues", message or "", flags=re.IGNORECASE)
    if not match:
        return ""
    return (match.group(1) or "").strip()


def _filter_by_issue_ids(df, issue_ids):
    if "SightingID" not in df.columns or not issue_ids:
        return df.iloc[0:0]

    normalized_ids = {
        str(value).strip().lower()
        for value in issue_ids
        if str(value).strip()
    }
    if not normalized_ids:
        return df.iloc[0:0]

    sighting_series = df["SightingID"].fillna("").astype(str).str.strip().str.lower()
    return df[sighting_series.isin(normalized_ids)]


def _is_followup_prompt(prompt):
    prompt_lower = (prompt or "").lower()
    followup_markers = [
        "these issues",
        "those issues",
        "mentioned above",
        "from above",
        "earlier",
        "previous",
        "same issues",
        "the above",
        "them",
        "what about",
        "how about",
        "and for",
        "same for",
        "this one",
        "that one",
        "also",
    ]
    return any(marker in prompt_lower for marker in followup_markers)


def _resolve_reference_rows(df, prompt, retrieved_df, context):
    if not context:
        return retrieved_df

    if not _is_followup_prompt(prompt):
        return retrieved_df

    by_ids = _filter_by_issue_ids(df, context.get("last_issue_ids") or [])
    if len(by_ids) > 0:
        return by_ids

    companies = context.get("last_companies") or []
    if companies and "customer_company" in df.columns:
        indices = set()
        for company in companies[:3]:
            filtered = _company_filter(df, company)
            if len(filtered) > 0:
                indices.update(filtered.index.tolist())

        if indices:
            return df.loc[sorted(indices)]

    owners = context.get("last_owners") or []
    if owners:
        indices = set()
        for owner in owners[:3]:
            filtered = _owner_filter(df, owner)
            if len(filtered) > 0:
                indices.update(filtered.index.tolist())

        if indices:
            return df.loc[sorted(indices)]

    return retrieved_df


def _apply_prompt_entity_focus(df, prompt, reference_df):
    prompt_lower = (prompt or "").lower()
    companies = _detect_companies(df, prompt)

    list_like = _contains_any(
        prompt_lower,
        ["list", "show", "issues related", "issues for", "which issues"],
    )

    if companies and list_like:
        focused = _company_filter(df, companies[0])
        if len(focused) > 0:
            return focused

    return reference_df


def _extract_issue_ids(df, limit=120):
    if "SightingID" not in df.columns or len(df) == 0:
        return []

    values = (
        df["SightingID"]
        .fillna("")
        .astype(str)
        .str.strip()
        .tolist()
    )
    values = [value for value in values if value]
    return values[:limit]


def _platform_breakdown_message(reference_df, fallback_platform=""):
    platform_column = "Platform" if "Platform" in reference_df.columns else "platform"
    if platform_column not in reference_df.columns or len(reference_df) == 0:
        if fallback_platform:
            return f"Platforms for the referenced issues:\n- {fallback_platform}: based on the current selected platform dataset"
        return "Platform information is unavailable for the referenced issues."

    counts = (
        reference_df[platform_column]
        .fillna("UNKNOWN")
        .astype(str)
        .str.strip()
        .replace("", "UNKNOWN")
        .value_counts()
    )

    if len(counts) == 0:
        return "Platform information is unavailable for the referenced issues."

    lines = ["Platforms for the referenced issues:"]
    for name, count in counts.items():
        lines.append(f"- {name}: {int(count)} issues")
    return "\n".join(lines)


def _build_next_context(prompt, response, source_df, previous_context=None):
    """Build enriched context with conversation history"""
    companies = _detect_companies(source_df, prompt)
    owners = _detect_owners(source_df, prompt)

    if _is_top_owner_question(prompt):
        inferred_owner = _extract_primary_entity_from_message(response).lower()
        if inferred_owner:
            owners = [inferred_owner]

    if _is_top_customer_question(prompt):
        inferred_company = _extract_primary_entity_from_message(response).lower()
        if inferred_company:
            companies = [inferred_company]

    platform_column = "Platform" if "Platform" in source_df.columns else "platform"
    platforms = []
    
    if HAS_PANDAS and platform_column in source_df.columns:
        platforms = (
            source_df[platform_column]
            .fillna("UNKNOWN")
            .astype(str)
            .str.strip()
            .replace("", "UNKNOWN")
            .value_counts()
            .head(8)
            .index
            .tolist()
        )
    
    # Maintain conversation history
    conversation_history = []
    if previous_context and previous_context.get("conversation_history"):
        conversation_history = previous_context.get("conversation_history", [])[-4:]  # Keep last 4 exchanges
    
    # Add current exchange
    conversation_history.append({
        "user": (prompt or "").strip()[:300],
        "assistant": (response or "").strip()[:300]
    })
    
    return {
        "last_prompt": (prompt or "").strip(),
        "last_issue_ids": _extract_issue_ids(source_df, limit=120),
        "last_companies": companies[:5],
        "last_owners": owners[:5],
        "last_platforms": platforms,
        "last_result_size": int(len(source_df)) if HAS_PANDAS else len(source_df),
        "conversation_history": conversation_history,
    }


def _monthly_counts(dataframe, pd):
    if "SubmittedDate" not in dataframe.columns or len(dataframe) == 0:
        return None

    frame = dataframe.copy()
    frame["SubmittedDate"] = pd.to_datetime(frame["SubmittedDate"], errors="coerce")
    frame = frame.dropna(subset=["SubmittedDate"])
    if len(frame) == 0:
        return None

    frame["Month"] = frame["SubmittedDate"].dt.to_period("M").astype(str)
    return frame.groupby("Month").size().reset_index(name="count")


def _create_comparison_chart(df_a, df_b, name_a, name_b, output_dir, pd, plt):
    import matplotlib.pyplot as plt
    
    trend_a = _monthly_counts(df_a, pd)
    trend_b = _monthly_counts(df_b, pd)

    if trend_a is None or trend_b is None:
        return ""

    trend_a["company"] = name_a
    trend_b["company"] = name_b
    trend = pd.concat([trend_a, trend_b], ignore_index=True)
    if len(trend) == 0:
        return ""

    fig, ax = plt.subplots(figsize=(12, 6))
    
    for company in [name_a, name_b]:
        company_data = trend[trend["company"] == company]
        ax.plot(company_data["Month"], company_data["count"], marker='o', linewidth=2, label=company)
    
    ax.set_title(f"Issue Trend: {name_a} vs {name_b}", fontsize=14)
    ax.set_xlabel("Month", fontsize=12)
    ax.set_ylabel("Issue Count", fontsize=12)
    ax.legend()
    ax.grid(True, alpha=0.3)
    plt.xticks(rotation=45)
    plt.tight_layout()
    
    png_file = _safe_name("company_trend", "png")
    fig.savefig(os.path.join(output_dir, png_file), dpi=100, bbox_inches='tight')
    plt.close(fig)
    return png_file


def _deterministic_rag_answer(df, prompt, retrieved_df, pd, go, output_dir, fallback_platform=""):
    prompt_lower = (prompt or "").lower()
    image_file = ""
    requested_platforms = _extract_platform_tokens(prompt)

    if _is_graph_request(prompt) and len(requested_platforms) >= 2:
        platform_counts = _count_by_platform(df, requested_platforms, fallback_platform)
        chart_file = _build_platform_comparison_chart(df, requested_platforms, output_dir)
        lines = ["Generated platform comparison graph from current CMF dataset:"]
        for name, count in platform_counts:
            lines.append(f"- {name}: {count} records")
        if chart_file:
            return "\n".join(lines), chart_file
        return "\n".join(lines), image_file

    if _is_count_question(prompt):
        status_tokens = _extract_status_tokens(prompt)
        if status_tokens and "status" in df.columns:
            status_series = df["status"].fillna("").astype(str).str.strip().str.lower()
            lines = ["Exact status count from current CMF dataset:"]
            for token in status_tokens:
                if token == "complete":
                    count = int(status_series.isin(["complete", "closed", "resolved"]).sum())
                elif token == "pending":
                    count = int((~status_series.isin(["complete", "closed", "resolved", "rejected", "implemented", "verified"])).sum())
                else:
                    count = int((status_series == token).sum())
                lines.append(f"- {token}: {count} records")
            lines.append(f"- total issues in scope: {len(df)}")
            return "\n".join(lines), image_file

    if _is_count_question(prompt) and (_contains_any(prompt_lower, ["platform", "platforms"]) or requested_platforms):
        platform_counts = _count_by_platform(df, requested_platforms, fallback_platform)
        lines = ["Exact count from current CMF dataset:"]
        for name, count in platform_counts:
            lines.append(f"- {name}: {count} records")
        if not requested_platforms and fallback_platform:
            lines.append("- Count is based on the currently selected platform table.")
        return "\n".join(lines), image_file

    if _is_top_customer_question(prompt) and "customer_company" in df.columns:
        customer_counts = (
            df["customer_company"]
            .fillna("Unassigned")
            .astype(str)
            .str.strip()
            .replace("", "Unassigned")
            .value_counts()
        )
        if len(customer_counts) > 0:
            top_name = str(customer_counts.index[0])
            top_count = int(customer_counts.iloc[0])
            return (
                "Top customer in current CMF dataset:\n"
                f"- {top_name}: {top_count} issues\n"
                f"- Total issues analyzed: {len(df)}"
            ), image_file

    if _is_top_owner_question(prompt):
        owner_col = "Owner" if "Owner" in df.columns else ("owner" if "owner" in df.columns else "")
        if owner_col:
            owner_counts = (
                df[owner_col]
                .fillna("Unassigned")
                .astype(str)
                .str.strip()
                .replace("", "Unassigned")
                .value_counts()
            )
            if len(owner_counts) > 0:
                top_name = str(owner_counts.index[0])
                top_count = int(owner_counts.iloc[0])
                return (
                    "Top owner in current CMF dataset:\n"
                    f"- {top_name}: {top_count} issues\n"
                    f"- Total issues analyzed: {len(df)}"
                ), image_file

    if _looks_like_platform_breakdown_question(prompt):
        return _platform_breakdown_message(df, fallback_platform), image_file

    # Company comparison questions
    companies = _detect_companies(df, prompt)
    compare_requested = _contains_any(prompt_lower, ["compare", "versus", "vs", "trend between"])
    if compare_requested and len(companies) >= 2:
        company_a = companies[0]
        company_b = companies[1]
        data_a = _company_filter(df, company_a)
        data_b = _company_filter(df, company_b)

        open_a = int(data_a["status"].fillna("").astype(str).str.lower().eq("open").sum()) if "status" in data_a.columns else 0
        open_b = int(data_b["status"].fillna("").astype(str).str.lower().eq("open").sum()) if "status" in data_b.columns else 0
        stale_a = int((data_a["days_active"] > 14).sum()) if "days_active" in data_a.columns else 0
        stale_b = int((data_b["days_active"] > 14).sum()) if "days_active" in data_b.columns else 0

        import matplotlib.pyplot as plt
        image_file = _create_comparison_chart(data_a, data_b, company_a, company_b, output_dir, pd, plt)

        message = (
            f"Comparison for {company_a} vs {company_b}:\n"
            f"- Total issues: {len(data_a)} vs {len(data_b)}\n"
            f"- Open issues: {open_a} vs {open_b}\n"
            f"- Stale issues (>14 days): {stale_a} vs {stale_b}\n"
            "- Trend chart generated when submitted-date data is available."
        )
        return message, image_file

    # List issues for requested company/entity
    if _contains_any(prompt_lower, ["list", "show", "which", "issues related", "issues for"]):
        if companies:
            company = companies[0]
            subset = _company_filter(df, company)
            if len(subset) > 0:
                lines = [
                    f"Top issues for {company} (showing up to 12):",
                ]
                for _, row in subset.head(12).iterrows():
                    lines.append(_format_issue_line(row))
                return "\n".join(lines), ""

        lines = ["Top relevant issues (showing up to 12):"]
        for _, row in retrieved_df.head(12).iterrows():
            lines.append(_format_issue_line(row))
        return "\n".join(lines), ""

    # Generic grounded answer with quick stats
    total = len(df)
    status_counts = (
        df["status"].fillna("unknown").astype(str).str.lower().value_counts().head(6)
        if "status" in df.columns
        else None
    )

    lines = [
        "Grounded answer from retrieved issue data:",
        f"- Total issues analyzed: {total}",
    ]

    if status_counts is not None and len(status_counts) > 0:
        status_text = ", ".join([f"{index}: {int(value)}" for index, value in status_counts.items()])
        lines.append(f"- Status mix: {status_text}")

    lines.append("- Most relevant issues:")
    for _, row in retrieved_df.head(8).iterrows():
        lines.append(_format_issue_line(row))

    return "\n".join(lines), ""


def _build_context_block(df, retrieved_df):
    total = len(df)
    status_counts = (
        df["status"].fillna("unknown").astype(str).str.lower().value_counts().head(8)
        if "status" in df.columns
        else None
    )

    company_counts = (
        df["customer_company"].fillna("Unassigned").astype(str).value_counts().head(8)
        if "customer_company" in df.columns
        else None
    )

    component_counts = (
        df["component"].fillna("Unknown").astype(str).value_counts().head(5)
        if "component" in df.columns
        else None
    )

    driver_counts = (
        df["drivers"].fillna("Unknown").astype(str).value_counts().head(5)
        if "drivers" in df.columns
        else None
    )

    lines = [
        "=== CMF PORTAL DATA CONTEXT ===",
        f"Total dataset rows: {total}",
        f"Retrieved relevant rows: {len(retrieved_df)}",
        "",
        "Available data fields: status, component, drivers, owner, platform, customer_company, title, date_submitted, days_active",
        "",
        "Status counts:",
    ]
    if status_counts is not None and len(status_counts) > 0:
        for index, value in status_counts.items():
            lines.append(f"- {index}: {int(value)}")
    else:
        lines.append("- unavailable")

    lines.append("\nTop companies:")
    if company_counts is not None and len(company_counts) > 0:
        for index, value in company_counts.items():
            lines.append(f"- {index}: {int(value)}")
    else:
        lines.append("- unavailable")

    lines.append("\nTop components:")
    if component_counts is not None and len(component_counts) > 0:
        for index, value in component_counts.items():
            lines.append(f"- {index}: {int(value)}")
    else:
        lines.append("- unavailable")

    lines.append("\nTop drivers:")
    if driver_counts is not None and len(driver_counts) > 0:
        for index, value in driver_counts.items():
            lines.append(f"- {index}: {int(value)}")
    else:
        lines.append("- unavailable")

    lines.append("\n=== RETRIEVED ISSUES (Matching Query) ===")
    if HAS_PANDAS:
        for _, row in retrieved_df.head(25).iterrows():
            lines.append(_format_issue_line(row))
    else:
        for i, row in enumerate(retrieved_df[:25]):
            lines.append(_format_issue_line(row))

    lines.append("\n=== NOTES ===")
    lines.append("- If asked about data not shown above, clearly state it's not in the current dataset")
    lines.append("- Provide analysis only based on the data shown")
    lines.append("- Use specific numbers and counts from the data")

    return "\n".join(lines)


def _build_context_with_history(df, retrieved_df, conversation_context):
    """Build RAG context block with conversation history"""
    base_context = _build_context_block(df, retrieved_df)
    
    if not conversation_context or not conversation_context.get("conversation_history"):
        return base_context
    
    history = conversation_context.get("conversation_history", [])
    if not history:
        return base_context
    
    # Include last 2-3 exchanges for context
    context_lines = [base_context, "\n=== Conversation History ==="]
    for exchange in history[-3:]:
        user_q = exchange.get("user", "").strip()
        assistant_a = exchange.get("assistant", "").strip()
        
        if user_q:
            context_lines.append(f"Q: {user_q[:200]}...")
        if assistant_a:
            context_lines.append(f"A: {assistant_a[:200]}...")
        context_lines.append("---")
    
    return "\n".join(context_lines)


def _truncate_context_block(context_block, max_chars=14000):
    text = context_block or ""
    if len(text) <= max_chars:
        return text

    head = text[:max_chars]
    cut = head.rfind("\n")
    if cut > int(max_chars * 0.7):
        head = head[:cut]
    return head + "\n[Context truncated for model token safety]"


def _classify_intent_with_model(prompt, context_block):
    """Use LLM to classify intent more accurately"""
    api_key = os.environ.get("CMF_GITHUB_API_KEY", "").strip()
    endpoint = os.environ.get("CMF_GITHUB_ENDPOINT", "").strip() or "https://models.inference.ai.azure.com/chat/completions"
    model = os.environ.get("CMF_GITHUB_MODEL", "").strip() or "gpt-4o-mini"
    
    if not api_key:
        return ""  # Fallback to keyword-based classification
    
    classification_prompt = (
        "Classify this request into ONE of these intents:\n"
        "- 'summary': user asks for overview/summary\n"
        "- 'status_chart': user wants chart/graph of status distribution\n"
        "- 'owner_chart': user wants to see issues by owner\n"
        "- 'stale_chart': user wants to see aging/stale issues\n"
        "- 'issue_report': user wants CSV/Excel report\n"
        "- 'comparison': user wants to compare two companies/owners\n"
        "- 'rag_qa': user asks a question requiring analysis\n\n"
        f"User request: {prompt}\n\n"
        "Respond with ONLY the intent name."
    )
    
    payload = {
        "model": model,
        "messages": [
            {"role": "system", "content": "You are an analytics classifier. Respond with only the intent name."},
            {"role": "user", "content": classification_prompt},
        ],
    }
    
    try:
        data = json.dumps(payload).encode("utf-8")
        headers = {
            "Content-Type": "application/json",
            "Authorization": f"Bearer {api_key}",
            "Accept": "application/json",
        }
        
        request = urllib.request.Request(endpoint, data=data, headers=headers, method="POST")
        proxy = os.environ.get("CMF_GITHUB_PROXY", "").strip()
        
        handlers = []
        if proxy:
            handlers.append(urllib.request.ProxyHandler({"http": proxy, "https": proxy}))
        
        opener = urllib.request.build_opener(*handlers)
        
        with opener.open(request, timeout=15) as response:
            body = response.read().decode("utf-8", errors="replace")
            parsed = json.loads(body)
            choices = parsed.get("choices") or []
            if not choices:
                return ""
            message = choices[0].get("message") or {}
            intent = (message.get("content") or "").strip().lower()
            
            # Validate intent
            valid_intents = ["summary", "status_chart", "owner_chart", "stale_chart", "issue_report", "comparison", "rag_qa"]
            return intent if intent in valid_intents else ""
    except Exception:
        return ""


def _call_claude_model(prompt, context_block):
    api_key = os.environ.get("ANTHROPIC_API_KEY", "").strip()
    
    if not api_key:
        return _call_github_model(prompt, context_block)
    
    try:
        import anthropic
    except ImportError:
        return _call_github_model(prompt, context_block)
    
    client = anthropic.Anthropic(api_key=api_key)
    
    system_msg = (
        "You are a CMF reports assistant specialized in analyzing issue data. "
        "Answer ONLY from provided context. If data is insufficient, say so clearly. "
        "Keep concise and numeric where possible. Use analytical insights to provide value."
    )
    user_msg = (
        "User question:\n"
        f"{prompt}\n\n"
        "Context data:\n"
        f"{context_block}\n\n"
        "Output rules:\n"
        "- 4-8 bullets max\n"
        "- include exact counts when available\n"
        "- provide actionable insights\n"
        "- do not fabricate issue IDs or trends"
    )
    
    try:
        message = client.messages.create(
            model="claude-3-5-sonnet-20241022",
            max_tokens=1024,
            system=system_msg,
            messages=[
                {"role": "user", "content": user_msg}
            ]
        )
        return (message.content[0].text if message.content else "").strip()
    except Exception:
        return _call_github_model(prompt, context_block)


def _call_github_model(prompt, context_block):
    api_key = os.environ.get("CMF_GITHUB_API_KEY", "").strip()
    endpoint = os.environ.get("CMF_GITHUB_ENDPOINT", "").strip() or "https://models.inference.ai.azure.com/chat/completions"
    model = os.environ.get("CMF_GITHUB_MODEL", "").strip() or "gpt-4o-mini"
    proxy = os.environ.get("CMF_GITHUB_PROXY", "").strip()

    if not api_key:
        return ""

    system_msg = (
        "You are a CMF (Continuous Manufacturing Framework) analytics assistant. Your role is to analyze issue data and provide actionable insights.\n"
        "\nCONTEXT:\n"
        "- You analyze issues, bugs, and defects from configured CMF platform datasets\n"
        "- Data includes: issue IDs, titles, status, components, drivers, owners, platforms, and dates\n"
        "\nREQUIRED RULES:\n"
        "1. RELEVANCE: Only answer questions about CMF issues, platforms, data analysis, and trends\n"
        "2. DATA-DRIVEN: Answer ONLY based on the provided data context. Never fabricate statistics\n"
        "3. MISSING DATA: If the user asks about something not in the dataset (e.g., specific customer group not found), clearly state: 'This specific information is not available in the current dataset. Based on available data: [provide what you can find]'\n"
        "4. ACCURACY: Always include specific numbers, counts, and percentages when available\n"
        "5. CONTEXT-AWARE: Treat Conversation History as working memory. Resolve pronouns like 'those', 'them', 'that owner', 'same customer', and 'previous chart' from prior turns before answering.\n"
        "6. CONCISE: Keep responses brief (4-8 bullet points or 2-4 paragraphs max)\n"
        "7. PROACTIVE: Anticipate likely follow-ups. After answering, suggest one useful next question, chart, or report action based on the result. Ask a clarifying question only when required to avoid a wrong answer.\n"
        "8. NO FABRICATION: Never invent issue IDs, dates, components, or statistics\n"
        "9. GRACEFUL DEGRADATION: If data is incomplete, explain what you found and what's missing\n"
        "10. SCOPE LIMITS: Reject non-portal questions. If asked something unrelated to CMF (e.g., 'sing a song'), say: 'I can only help with CMF-related data analysis. Please ask about issues, platforms, or analytics.'\n"
        "\nRESPONSE FORMAT:\n"
        "- Lead with key insights first\n"
        "- Use bullet points for multiple findings\n"
        "- Include data counts/percentages when relevant\n"
        "- End with 'Useful next step:' followed by one specific follow-up question, chart, or report action"
    )
    
    user_msg = (
        "Analyze the following data and respond to the user question:\n\n"
        f"{context_block}\n\n"
        f"User question: {prompt}\n\n"
        "Provide a clear, data-driven response. Use conversation history for follow-up questions. If the question asks about something specific not found in the data, "
        "state that clearly and provide what you can find instead. End with one useful next step."
    )

    payload = {
        "model": model,
        "messages": [
            {"role": "system", "content": system_msg},
            {"role": "user", "content": user_msg},
        ],
    }

    data = json.dumps(payload).encode("utf-8")
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {api_key}",
        "Accept": "application/json",
        "User-Agent": "CMF-Reports-RAG/1.0",
    }

    request = urllib.request.Request(endpoint, data=data, headers=headers, method="POST")

    handlers = []
    if proxy:
        handlers.append(urllib.request.ProxyHandler({"http": proxy, "https": proxy}))

    opener = urllib.request.build_opener(*handlers)

    try:
        with opener.open(request, timeout=35) as response:
            body = response.read().decode("utf-8", errors="replace")
            parsed = json.loads(body)
            choices = parsed.get("choices") or []
            if not choices:
                return ""
            message = choices[0].get("message") or {}
            return (message.get("content") or "").strip()
    except (urllib.error.URLError, urllib.error.HTTPError, TimeoutError, ValueError):
        return ""


def _summary_message(df):
    if HAS_PANDAS:
        total = len(df)
        status_counts = df["status"].fillna("unknown").astype(str).str.lower().value_counts() if "status" in df.columns else {}
        open_count = int(status_counts.get("open", 0)) if hasattr(status_counts, "get") else 0
        implemented_count = int(status_counts.get("implemented", 0)) if hasattr(status_counts, "get") else 0
        closed_count = int(status_counts.get("complete", 0) + status_counts.get("rejected", 0)) if hasattr(status_counts, "get") else 0

        stale_count = 0
        if "days_active" in df.columns:
            stale_count = int((df["days_active"].fillna(0).astype(float) > 14).sum())

        top_component = "N/A"
        if "component_group" in df.columns:
            cg = df["component_group"].fillna("Unassigned").astype(str).value_counts()
            if len(cg) > 0:
                top_component = f"{cg.index[0]} ({int(cg.iloc[0])})"
    else:
        total = len(df)
        status_counts = {}
        stale_count = 0
        component_counts = {}

        for row in df:
            status = str(row.get("status", "unknown") or "unknown").strip().lower()
            status_counts[status] = status_counts.get(status, 0) + 1

            try:
                if float(row.get("days_active", 0) or 0) > 14:
                    stale_count += 1
            except Exception:
                pass

            comp = str(row.get("component_group", "Unassigned") or "Unassigned").strip()
            component_counts[comp] = component_counts.get(comp, 0) + 1

        open_count = int(status_counts.get("open", 0))
        implemented_count = int(status_counts.get("implemented", 0))
        closed_count = int(status_counts.get("complete", 0) + status_counts.get("rejected", 0))
        top_component = "N/A"
        if component_counts:
            top_component = sorted(component_counts.items(), key=lambda item: item[1], reverse=True)[0]
            top_component = f"{top_component[0]} ({int(top_component[1])})"

    return (
        "Analytics summary:\n"
        f"- Total issues analyzed: {total}\n"
        f"- Open: {open_count}\n"
        f"- Implemented: {implemented_count}\n"
        f"- Closed (complete/rejected): {closed_count}\n"
        f"- Stale (days_active > 14): {stale_count}\n"
        f"- Top component group: {top_component}\n"
        "Ask for a chart or report for deeper analysis."
    )


def main():
    parser = argparse.ArgumentParser(description="CMF reports assistant analytics runner")
    parser.add_argument("--csv", required=True)
    parser.add_argument("--intent", required=True)
    parser.add_argument("--prompt", required=True)
    parser.add_argument("--context-json", default="{}")
    parser.add_argument("--platform", default="")
    parser.add_argument("--template", default="")
    parser.add_argument("--output-dir", required=True)
    parser.add_argument("--max-rows", type=int, default=2000)
    args = parser.parse_args()

    if not os.path.exists(args.csv):
        _fail("Input CSV file not found.")
        return

    os.makedirs(args.output_dir, exist_ok=True)

    # Read CSV using pandas or fallback method
    if HAS_PANDAS:
        try:
            df = pd.read_csv(args.csv)
        except Exception as ex:
            _fail("Failed to read CSV: " + str(ex))
            return
        
        if len(df) == 0:
            _fail("No data rows available for analytics.")
            return
        
        df = df.head(max(1, args.max_rows)).copy()
        df.columns = [str(c).strip() for c in df.columns]
        
        for col in ["status", "component_group", "Owner", "drivers", "days_active"]:
            if col in df.columns:
                if col == "days_active":
                    df[col] = pd.to_numeric(df[col], errors="coerce").fillna(0)
                else:
                    df[col] = df[col].fillna("Unassigned").astype(str)
    else:
        # Fallback: use simple CSV reader
        rows = _read_csv_simple(args.csv, args.max_rows)
        if not rows:
            _fail("Failed to read CSV or no data available.")
            return
        
        df = rows  # Keep as list of dicts for compatibility
        if len(df) == 0:
            _fail("No data rows available for analytics.")
            return

    intent = _normalize_intent(args.intent or "rag_qa")
    session_context = _parse_context_json(args.context_json)

    is_relevant, relevance_message = _validate_cmf_prompt(args.prompt, session_context)
    if not is_relevant:
        print(json.dumps({"success": True, "message": relevance_message, "image_file": "", "report_file": ""}))
        return

    image_file = ""
    report_file = ""

    # If a template path was supplied, generate a markdown report based on it
    if args.template:
        try:
            if not os.path.exists(args.template):
                _fail("Template file not found: " + args.template)
                return

            with open(args.template, 'r', encoding='utf-8') as tf:
                template_text = tf.read()

            report_lines = _template_report_lines(template_text, df, args.platform)
            out_name = _safe_name("report", "pdf")
            out_path = os.path.join(args.output_dir, out_name)
            _write_simple_pdf(out_path, "CMF Report", report_lines)

            print(json.dumps({"success": True, "message": "PDF report generated from the saved template and live CMF data.", "image_file": image_file, "report_file": out_name}))
            return
        except Exception as ex:
            _fail("Failed to generate report from template: " + str(ex))
            return

    if intent == "summary":
        message = _summary_message(df)
        print(json.dumps({"success": True, "message": message, "image_file": image_file, "report_file": report_file}))
        return

    if intent == "status_chart":
        if HAS_PANDAS:
            if "status" not in df.columns:
                _fail("status column is unavailable in data.")
                return

            if False and plt is not None:
                counts = df["status"].fillna("unknown").astype(str).value_counts().head(12)
                if len(counts) == 0:
                    _fail("No status values available for chart generation.")
                    return

                fig, ax = plt.subplots(figsize=(12, 6))
                ax.bar(counts.index.tolist(), counts.values.tolist(), color='#0068b5')
                ax.set_title("Issues by Status", fontsize=14)
                ax.set_xlabel("Status", fontsize=12)
                ax.set_ylabel("Issue Count", fontsize=12)
                ax.grid(True, alpha=0.3, axis='y')
                plt.xticks(rotation=45, ha='right')
                plt.tight_layout()

                image_file = _safe_name("status_chart", "png")
                fig.savefig(os.path.join(args.output_dir, image_file), dpi=100, bbox_inches='tight')
                plt.close(fig)
            else:
                # Fallback to HTML chart
                counts = df["status"].fillna("unknown").astype(str).value_counts().head(12)
                if len(counts) == 0:
                    _fail("No status values available for chart generation.")
                    return
                
                image_file = _safe_name("status_chart", "html")
                if _generate_html_bar_chart("Issues by Status", counts.index.tolist(), counts.values.tolist(), 
                                           os.path.join(args.output_dir, image_file), "#0068b5"):
                    pass
                else:
                    _fail("Failed to generate status chart.")
                    return
        else:
            # Fallback without pandas
            status_counts = {}
            for row in df:
                status = row.get("status", "unknown") or "unknown"
                status_counts[status] = status_counts.get(status, 0) + 1
            
            # Get top 12
            sorted_counts = sorted(status_counts.items(), key=lambda x: x[1], reverse=True)[:12]
            if not sorted_counts:
                _fail("No status values available for chart generation.")
                return
            
            labels = [item[0] for item in sorted_counts]
            values = [item[1] for item in sorted_counts]
            
            image_file = _safe_name("status_chart", "html")
            if not _generate_html_bar_chart("Issues by Status", labels, values, 
                                           os.path.join(args.output_dir, image_file), "#0068b5"):
                _fail("Failed to generate status chart.")
                return

        message = "Generated status distribution chart from current platform data."
        print(json.dumps({"success": True, "message": message, "image_file": image_file, "report_file": report_file}))
        return

    if intent == "owner_chart":
        if HAS_PANDAS:
            owner_col = "Owner" if "Owner" in df.columns else "owner"
            if owner_col not in df.columns:
                _fail("owner column is unavailable in data.")
                return

            if False and plt is not None:
                counts = df[owner_col].fillna("Unassigned").astype(str).value_counts().head(10)
                if len(counts) == 0:
                    _fail("No owner values available for chart generation.")
                    return

                fig, ax = plt.subplots(figsize=(12, 6))
                ax.barh(counts.index.tolist(), counts.values.tolist(), color='#00a6b2')
                ax.set_title("Top Owners by Issue Count", fontsize=14)
                ax.set_xlabel("Issue Count", fontsize=12)
                ax.set_ylabel("Owner", fontsize=12)
                ax.grid(True, alpha=0.3, axis='x')
                plt.tight_layout()

                image_file = _safe_name("owner_chart", "png")
                fig.savefig(os.path.join(args.output_dir, image_file), dpi=100, bbox_inches='tight')
                plt.close(fig)
            else:
                # Fallback to HTML chart
                counts = df[owner_col].fillna("Unassigned").astype(str).value_counts().head(10)
                if len(counts) == 0:
                    _fail("No owner values available for chart generation.")
                    return
                
                image_file = _safe_name("owner_chart", "html")
                if _generate_html_horizontal_bar_chart("Top Owners by Issue Count", counts.index.tolist(), 
                                                       counts.values.tolist(), os.path.join(args.output_dir, image_file), "#00a6b2"):
                    pass
                else:
                    _fail("Failed to generate owner chart.")
                    return
        else:
            # Fallback without pandas
            owner_col = "Owner" if any("Owner" in row for row in df) else "owner"
            owner_counts = {}
            for row in df:
                owner = row.get(owner_col, "Unassigned") or "Unassigned"
                owner_counts[owner] = owner_counts.get(owner, 0) + 1
            
            # Get top 10
            sorted_counts = sorted(owner_counts.items(), key=lambda x: x[1], reverse=True)[:10]
            if not sorted_counts:
                _fail("No owner values available for chart generation.")
                return
            
            labels = [item[0] for item in sorted_counts]
            values = [item[1] for item in sorted_counts]
            
            image_file = _safe_name("owner_chart", "html")
            if not _generate_html_horizontal_bar_chart("Top Owners by Issue Count", labels, values,
                                                       os.path.join(args.output_dir, image_file), "#00a6b2"):
                _fail("Failed to generate owner chart.")
                return

        message = "Generated top-owners chart."
        print(json.dumps({"success": True, "message": message, "image_file": image_file, "report_file": report_file}))
        return

    if intent == "stale_chart":
        if HAS_PANDAS:
            if "days_active" not in df.columns:
                _fail("days_active column is unavailable in data.")
                return

            if False and plt is not None:
                fig, ax = plt.subplots(figsize=(12, 6))
                ax.hist(df["days_active"], bins=18, color='#bc2f32', edgecolor='black')
                ax.set_title("Issue Aging Distribution (days_active)", fontsize=14)
                ax.set_xlabel("Days Active", fontsize=12)
                ax.set_ylabel("Issue Count", fontsize=12)
                ax.grid(True, alpha=0.3, axis='y')
                plt.tight_layout()

                image_file = _safe_name("stale_chart", "png")
                fig.savefig(os.path.join(args.output_dir, image_file), dpi=100, bbox_inches='tight')
                plt.close(fig)
            else:
                # Fallback to HTML chart
                days_active_values = df["days_active"].tolist()
                image_file = _safe_name("stale_chart", "html")
                if _generate_html_histogram("Issue Aging Distribution (days_active)", days_active_values, 18, 
                                           os.path.join(args.output_dir, image_file), "#bc2f32"):
                    pass
                else:
                    _fail("Failed to generate stale chart.")
                    return
        else:
            # Fallback without pandas
            days_active_values = []
            for row in df:
                try:
                    val = float(row.get("days_active", 0) or 0)
                    days_active_values.append(val)
                except:
                    pass
            
            if not days_active_values:
                _fail("days_active column is unavailable or has no numeric values.")
                return
            
            image_file = _safe_name("stale_chart", "html")
            if not _generate_html_histogram("Issue Aging Distribution (days_active)", days_active_values, 18,
                                           os.path.join(args.output_dir, image_file), "#bc2f32"):
                _fail("Failed to generate stale chart.")
                return

        if HAS_PANDAS:
            stale_count = int((df["days_active"] > 14).sum())
        else:
            stale_count = 0
            for row in df:
                try:
                    if float(row.get("days_active", 0) or 0) > 14:
                        stale_count += 1
                except Exception:
                    pass
        message = f"Generated interactive issue-aging chart. Stale issue count (>14 days): {stale_count}."
        print(json.dumps({"success": True, "message": message, "image_file": image_file, "report_file": report_file}))
        return

    if intent == "issue_report":
        report_file = _safe_name("issue_report", "csv")
        report_path = os.path.join(args.output_dir, report_file)
        if HAS_PANDAS:
            df.to_csv(report_path, index=False)
        else:
            import csv
            fieldnames = list(df[0].keys()) if df else []
            with open(report_path, "w", encoding="utf-8", newline="") as csv_file:
                writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
                writer.writeheader()
                writer.writerows(df)

        message = (
            "Generated downloadable issue report CSV for the current platform. "
            "You can open it in Excel for deeper analysis."
        )
        print(json.dumps({"success": True, "message": message, "image_file": image_file, "report_file": report_file}))
        return

    if intent in {"rag_qa", "comparison"}:
        # Use improved semantic retrieval
        if HAS_PANDAS:
            retrieved_df = _retrieve_rows_semantic(df, args.prompt, top_k=40)
        else:
            retrieved_df, _ = _retrieve_rows(df, args.prompt, top_k=40)
        
        reference_df = _resolve_reference_rows(df, args.prompt, retrieved_df, session_context)
        reference_df = _apply_prompt_entity_focus(df, args.prompt, reference_df)
        
        # Build deterministic answer if applicable
        deterministic_message, deterministic_image = _deterministic_rag_answer(
            df,
            args.prompt,
            reference_df,
            pd,
            None,  # No need for plotting in comparison
            args.output_dir,
            args.platform,
        )

        # Build context with conversation history for better RAG
        context_block = _truncate_context_block(_build_context_with_history(df, reference_df, session_context))
        prompt_lower = (args.prompt or "").lower()
        force_deterministic = (
            (_contains_any(prompt_lower, ["platform", "platforms"]) and _is_followup_prompt(args.prompt))
            or (_is_count_question(args.prompt) and _contains_any(prompt_lower, ["platform", "platforms"]))
        )
        
        # Get model answer with improved prompting
        model_answer = "" if force_deterministic else _call_github_model(args.prompt, context_block)
        
        # Determine final message
        message = model_answer if model_answer else deterministic_message
        image_file = deterministic_image

        context_source_df = reference_df
        primary_entity = _extract_primary_entity_from_message(message)
        if _is_top_owner_question(args.prompt) and primary_entity:
            focused = _owner_filter(df, primary_entity)
            if len(focused) > 0:
                context_source_df = focused
        elif _is_top_customer_question(args.prompt) and primary_entity:
            focused = _company_filter(df, primary_entity)
            if len(focused) > 0:
                context_source_df = focused
        
        # Build enriched context with conversation history
        next_context = _build_next_context(args.prompt, message, context_source_df, session_context)

        print(json.dumps({"success": True, "message": message, "image_file": image_file, "report_file": report_file, "context": next_context}))
        return

    _fail("Unsupported intent requested.")


if __name__ == "__main__":
    main()
