using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

public sealed class CmfExecutiveKpiItem
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string Subtext { get; set; }
    public string AccentHex { get; set; }
}

public sealed class CmfExecutiveChartSeriesItem
{
    public string Category { get; set; }
    public int PrimaryValue { get; set; }
    public int SecondaryValue { get; set; }
    public int TertiaryValue { get; set; }
    public string PrimaryLabel { get; set; }
    public string SecondaryLabel { get; set; }
    public string TertiaryLabel { get; set; }
}

public sealed class CmfExecutiveDeckSpec
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string ScopeSummary { get; set; }
    public string ExecutiveTakeaway { get; set; }
    public List<CmfExecutiveKpiItem> Kpis { get; set; }
    public List<string> ExecutiveBullets { get; set; }
    public List<string> HighlightBoxes { get; set; }
    public string ChartTitle { get; set; }
    public string PrimarySeriesName { get; set; }
    public string SecondarySeriesName { get; set; }
    public string TertiarySeriesName { get; set; }
    public List<CmfExecutiveChartSeriesItem> ChartItems { get; set; }
    public List<string> ComparisonTableHeaders { get; set; }
    public List<string[]> ComparisonTableRows { get; set; }
    public string Slide4Title { get; set; }
    public List<CmfExecutiveChartSeriesItem> ComponentChartItems { get; set; }
    public List<string> TopIssuesTableHeaders { get; set; }
    public List<string[]> TopIssuesTableRows { get; set; }
}

public static class CmfExecutivePptxService
{
    // Widescreen 16:9 in EMUs (13.333 in x 7.5 in)
    private const long SlideWidth = 12192000L;
    private const long SlideHeight = 6858000L;

    public static string GenerateExecutiveDeck(CmfExecutiveDeckSpec spec, string outputDirectory, string filePrefix = "CMF_Pulse_Executive_Deck")
    {
        if (spec == null) throw new ArgumentNullException("spec");
        if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentException("Output directory is required.", "outputDirectory");

        Directory.CreateDirectory(outputDirectory);
        string stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string safePrefix = System.Text.RegularExpressions.Regex.Replace(string.IsNullOrWhiteSpace(filePrefix) ? "CMF_Pulse_Deck" : filePrefix, @"[^A-Za-z0-9_-]+", "_");
        string fileName = safePrefix + "_" + stamp + ".pptx";
        string fullPath = Path.Combine(outputDirectory, fileName);

        using (PresentationDocument doc = PresentationDocument.Create(fullPath, PresentationDocumentType.Presentation))
        {
            PresentationPart presentationPart = doc.AddPresentationPart();
            presentationPart.Presentation = new P.Presentation();

            SlideMasterPart slideMasterPart = CreateSlideMasterPart(presentationPart);
            SlideLayoutPart slideLayoutPart = slideMasterPart.SlideLayoutParts.GetEnumerator().Current;
            foreach (SlideLayoutPart lp in slideMasterPart.SlideLayoutParts)
            {
                slideLayoutPart = lp;
                break;
            }

            P.SlideMasterIdList slideMasterIdList = new P.SlideMasterIdList(
                new P.SlideMasterId { Id = 2147483648U, RelationshipId = presentationPart.GetIdOfPart(slideMasterPart) });

            P.SlideIdList slideIdList = new P.SlideIdList();
            uint slideId = 256U;

            // Slide 1: Cover & Executive KPI Scorecard
            SlidePart slide1 = CreateBlankSlidePart(presentationPart, slideLayoutPart);
            BuildSlide1CoverAndKpis(slide1.Slide.CommonSlideData.ShapeTree, spec);
            slide1.Slide.Save();
            slideIdList.Append(new P.SlideId { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(slide1) });

            // Slide 2: Executive AI Insights, Trend Narrative & Highlight Callouts
            SlidePart slide2 = CreateBlankSlidePart(presentationPart, slideLayoutPart);
            BuildSlide2ExecutiveHighlights(slide2.Slide.CommonSlideData.ShapeTree, spec);
            slide2.Slide.Save();
            slideIdList.Append(new P.SlideId { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(slide2) });

            // Slide 3: Platform / Status Trend Chart & Comparison Matrix
            SlidePart slide3 = CreateBlankSlidePart(presentationPart, slideLayoutPart);
            BuildSlide3ChartAndComparisonTable(slide3.Slide.CommonSlideData.ShapeTree, spec);
            slide3.Slide.Save();
            slideIdList.Append(new P.SlideId { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(slide3) });

            // Slide 4: Component Distribution & Top Priority Sightings
            SlidePart slide4 = CreateBlankSlidePart(presentationPart, slideLayoutPart);
            BuildSlide4ComponentsAndTopIssues(slide4.Slide.CommonSlideData.ShapeTree, spec);
            slide4.Slide.Save();
            slideIdList.Append(new P.SlideId { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(slide4) });

            P.SlideSize slideSize = new P.SlideSize { Cx = (int)SlideWidth, Cy = (int)SlideHeight, Type = P.SlideSizeValues.Screen16x9 };
            P.NotesSize notesSize = new P.NotesSize { Cx = 6858000, Cy = 9144000 };

            presentationPart.Presentation.Append(slideMasterIdList, slideIdList, slideSize, notesSize);
            presentationPart.Presentation.Save();
        }

        return fullPath;
    }

    private static void BuildSlide1CoverAndKpis(P.ShapeTree tree, CmfExecutiveDeckSpec spec)
    {
        uint id = 2U;
        // Soft background canvas
        AddRectangle(tree, ref id, "BgCanvas", 0, 0, SlideWidth, SlideHeight, "F4F8FB", null);
        // Deep Navy Header Banner
        AddRectangle(tree, ref id, "HeroBanner", 0, 0, SlideWidth, 1850000L, "0A2540", null);
        // Cyan Accent Stripe
        AddRectangle(tree, ref id, "CyanStripe", 0, 1850000L, SlideWidth, 55000L, "00A6B2", null);

        // Badge pill in top-left
        AddRectangle(tree, ref id, "PulseBadge", 550000L, 260000L, 2350000L, 340000L, "0F5EA8", "38BDF8", "CMF PULSE AI  |  EXECUTIVE BRIEF", 1100, true, "E0F2FE", A.TextAlignmentTypeValues.Center);

        // Main Title & Subtitle
        string title = string.IsNullOrWhiteSpace(spec.Title) ? "CMF Executive Platform & Trend Analysis" : spec.Title;
        string subtitle = string.IsNullOrWhiteSpace(spec.Subtitle)
            ? "Live HSDES & CMF Portal Database Intelligence  |  Generated " + DateTime.UtcNow.ToString("dd MMM yyyy HH:mm") + " UTC"
            : spec.Subtitle;
        AddTextBox(tree, ref id, "DeckTitle", 550000L, 670000L, 11000000L, 620000L, title, 2800, true, "FFFFFF");
        AddTextBox(tree, ref id, "DeckSubtitle", 550000L, 1300000L, 11000000L, 420000L, subtitle, 1300, false, "BAE6FD");

        // Section Heading
        AddTextBox(tree, ref id, "KpiSectionHeading", 550000L, 2120000L, 11000000L, 360000L, "EXECUTIVE KPI SCORECARD (" + (string.IsNullOrWhiteSpace(spec.ScopeSummary) ? "SELECTED SCOPE" : spec.ScopeSummary.ToUpperInvariant()) + ")", 1300, true, "0F355C");

        List<CmfExecutiveKpiItem> kpis = spec.Kpis ?? new List<CmfExecutiveKpiItem>();
        if (kpis.Count == 0)
        {
            kpis.Add(new CmfExecutiveKpiItem { Label = "TOTAL ISSUES", Value = "0", Subtext = "In selected scope", AccentHex = "0F5EA8" });
        }

        int count = Math.Min(5, kpis.Count);
        long gap = 220000L;
        long totalWidth = SlideWidth - 1100000L;
        long cardWidth = (totalWidth - (gap * (count - 1))) / count;
        long cardY = 2550000L;
        long cardH = 1950000L;

        for (int i = 0; i < count; i++)
        {
            CmfExecutiveKpiItem item = kpis[i];
            long cardX = 550000L + i * (cardWidth + gap);
            string accent = string.IsNullOrWhiteSpace(item.AccentHex) ? "0F5EA8" : item.AccentHex;

            AddRectangle(tree, ref id, "KpiCard_" + i, cardX, cardY, cardWidth, cardH, "FFFFFF", "CBD5E1");
            AddRectangle(tree, ref id, "KpiTopBar_" + i, cardX, cardY, cardWidth, 75000L, accent, null);
            AddTextBox(tree, ref id, "KpiLabel_" + i, cardX + 140000L, cardY + 180000L, cardWidth - 280000L, 340000L, (item.Label ?? "METRIC").ToUpperInvariant(), 1050, true, "475569");
            AddTextBox(tree, ref id, "KpiVal_" + i, cardX + 140000L, cardY + 560000L, cardWidth - 280000L, 760000L, item.Value ?? "0", 3200, true, accent);
            AddTextBox(tree, ref id, "KpiSub_" + i, cardX + 140000L, cardY + 1400000L, cardWidth - 280000L, 420000L, item.Subtext ?? string.Empty, 1050, false, "64748B");
        }

        // Bottom Executive Takeaway Banner
        AddRectangle(tree, ref id, "TakeawayBox", 550000L, 4820000L, SlideWidth - 1100000L, 1520000L, "FFFFFF", "93C5FD");
        AddRectangle(tree, ref id, "TakeawayLeftStripe", 550000L, 4820000L, 95000L, 1520000L, "0F5EA8", null);
        AddTextBox(tree, ref id, "TakeawayHeader", 760000L, 4960000L, 10600000L, 320000L, "EXECUTIVE PULSE SUMMARY & KEY TAKEAWAY", 1200, true, "0F5EA8");
        string takeaway = string.IsNullOrWhiteSpace(spec.ExecutiveTakeaway)
            ? "This executive deck synthesizes live CMF disposition metrics, pending bottlenecks, and component trends across the selected platforms."
            : spec.ExecutiveTakeaway;
        AddTextBox(tree, ref id, "TakeawayBody", 760000L, 5320000L, 10600000L, 920000L, takeaway, 1350, false, "1E293B");
    }

    private static void BuildSlide2ExecutiveHighlights(P.ShapeTree tree, CmfExecutiveDeckSpec spec)
    {
        uint id = 2U;
        AddStandardSlideHeader(tree, ref id, "Executive AI Analysis & Strategic Highlights", spec.ScopeSummary);

        // Left Card: Executive Bullet Points
        long leftX = 550000L;
        long topY = 1220000L;
        long leftW = 6500000L;
        long cardH = 5150000L;

        AddRectangle(tree, ref id, "LeftNarrativeCard", leftX, topY, leftW, cardH, "FFFFFF", "CBD5E1");
        AddRectangle(tree, ref id, "LeftCardHeaderBar", leftX, topY, leftW, 440000L, "0F355C", null);
        AddTextBox(tree, ref id, "LeftCardHeaderTitle", leftX + 180000L, topY + 70000L, leftW - 360000L, 300000L, "KEY ANALYTICAL FINDINGS & TRENDS", 1250, true, "FFFFFF");

        List<string> bullets = spec.ExecutiveBullets ?? new List<string>();
        long bulletY = topY + 600000L;
        long bulletStep = bullets.Count > 4 ? 850000L : 1020000L;
        for (int i = 0; i < bullets.Count && i < 5; i++)
        {
            string bText = CleanMarkdownMarkers(bullets[i]);
            AddRectangle(tree, ref id, "BulletDot_" + i, leftX + 200000L, bulletY + 80000L, 120000L, 120000L, "00A6B2", null);
            AddTextBox(tree, ref id, "BulletText_" + i, leftX + 400000L, bulletY, leftW - 580000L, bulletStep - 60000L, bText, 1250, false, "1E293B");
            bulletY += bulletStep;
        }

        // Right Column: 3 Highlight Callout Cards
        long rightX = 7300000L;
        long rightW = SlideWidth - rightX - 550000L;
        List<string> callouts = spec.HighlightBoxes ?? new List<string>();
        string[] calloutTitles = new[] { "PLATFORM & TREND HIGHLIGHT", "COMPONENT / DUPLICATE SIGNAL", "RECOMMENDED FOCUS AREA" };
        string[] calloutColors = new[] { "0F5EA8", "D97706", "059669" };
        string[] calloutBgs = new[] { "F0F9FF", "FFFBEB", "ECFDF5" };

        long boxH = 1580000L;
        long boxGap = 200000L;
        for (int i = 0; i < 3; i++)
        {
            long boxY = topY + i * (boxH + boxGap);
            string body = i < callouts.Count ? CleanMarkdownMarkers(callouts[i]) : "Monitor CMF pending velocity and align component owners on top showstopper sightings.";
            AddRectangle(tree, ref id, "CalloutCard_" + i, rightX, boxY, rightW, boxH, calloutBgs[i], calloutColors[i]);
            AddRectangle(tree, ref id, "CalloutStripe_" + i, rightX, boxY, 85000L, boxH, calloutColors[i], null);
            AddTextBox(tree, ref id, "CalloutTitle_" + i, rightX + 180000L, boxY + 120000L, rightW - 300000L, 280000L, calloutTitles[i], 1100, true, calloutColors[i]);
            AddTextBox(tree, ref id, "CalloutBody_" + i, rightX + 180000L, boxY + 420000L, rightW - 300000L, boxH - 500000L, body, 1200, false, "1E293B");
        }
    }

    private static void BuildSlide3ChartAndComparisonTable(P.ShapeTree tree, CmfExecutiveDeckSpec spec)
    {
        uint id = 2U;
        string header = string.IsNullOrWhiteSpace(spec.ChartTitle) ? "Platform & CMF Disposition Trend Comparison" : spec.ChartTitle;
        AddStandardSlideHeader(tree, ref id, header, spec.ScopeSummary);

        // Left side: Native Vector Grouped Bar Chart Card
        long chartX = 550000L;
        long chartY = 1220000L;
        long chartW = 5650000L;
        long chartH = 5150000L;
        AddRectangle(tree, ref id, "ChartCard", chartX, chartY, chartW, chartH, "FFFFFF", "CBD5E1");
        AddTextBox(tree, ref id, "ChartBoxTitle", chartX + 160000L, chartY + 110000L, chartW - 320000L, 300000L, "VISUAL COMPARISON BY CATEGORY", 1150, true, "0F355C");

        DrawVectorBarChart(
            tree,
            ref id,
            chartX + 220000L,
            chartY + 520000L,
            chartW - 440000L,
            chartH - 740000L,
            spec.ChartItems ?? new List<CmfExecutiveChartSeriesItem>(),
            spec.PrimarySeriesName ?? "CMF Pending (Ask)",
            spec.SecondarySeriesName ?? "CMF Approved (OK)",
            spec.TertiarySeriesName ?? "Incomplete / Duplicate");

        // Right side: Styled Comparison Matrix Table
        long tableX = 6420000L;
        long tableY = 1220000L;
        long tableW = SlideWidth - tableX - 550000L;
        long tableH = 5150000L;

        AddRectangle(tree, ref id, "TableCard", tableX, tableY, tableW, tableH, "FFFFFF", "CBD5E1");
        AddTextBox(tree, ref id, "TableBoxTitle", tableX + 160000L, tableY + 110000L, tableW - 320000L, 300000L, "DISPOSITION & STATUS BREAKDOWN MATRIX", 1150, true, "0F355C");

        List<string> headers = spec.ComparisonTableHeaders ?? new List<string> { "Group", "Total", "Pending", "CMF_OK", "Incomp", "Dup" };
        List<string[]> rows = spec.ComparisonTableRows ?? new List<string[]>();
        AddStyledTable(tree, ref id, "ComparisonMatrixTable", tableX + 140000L, tableY + 480000L, tableW - 280000L, tableH - 640000L, headers, rows, 10);
    }

    private static void BuildSlide4ComponentsAndTopIssues(P.ShapeTree tree, CmfExecutiveDeckSpec spec)
    {
        uint id = 2U;
        string header = string.IsNullOrWhiteSpace(spec.Slide4Title) ? "Component Distribution & Top Priority Sightings" : spec.Slide4Title;
        AddStandardSlideHeader(tree, ref id, header, spec.ScopeSummary);

        // Top section: Horizontal / Grouped Component Bars
        long topX = 550000L;
        long topY = 1220000L;
        long fullW = SlideWidth - 1100000L;
        long compH = 2250000L;

        AddRectangle(tree, ref id, "CompCard", topX, topY, fullW, compH, "FFFFFF", "CBD5E1");
        AddTextBox(tree, ref id, "CompTitle", topX + 160000L, topY + 90000L, fullW - 320000L, 280000L, "TOP COMPONENTS BY CMF VOLUME (APPROVED / PENDING / DUPLICATE)", 1100, true, "0F355C");

        List<CmfExecutiveChartSeriesItem> compItems = spec.ComponentChartItems ?? new List<CmfExecutiveChartSeriesItem>();
        DrawHorizontalComponentBars(tree, ref id, topX + 200000L, topY + 420000L, fullW - 400000L, compH - 520000L, compItems);

        // Bottom section: Top Actionable Sightings Table
        long botY = 3650000L;
        long botH = 2720000L;
        AddRectangle(tree, ref id, "IssuesCard", topX, botY, fullW, botH, "FFFFFF", "CBD5E1");
        AddTextBox(tree, ref id, "IssuesTitle", topX + 160000L, botY + 90000L, fullW - 320000L, 280000L, "KEY HIGH-PRIORITY / PENDING CMF SIGHTINGS", 1100, true, "0F355C");

        List<string> headers = spec.TopIssuesTableHeaders ?? new List<string> { "Platform", "Sighting ID", "Component", "CMF Tag", "Status", "Title" };
        List<string[]> rows = spec.TopIssuesTableRows ?? new List<string[]>();
        AddStyledTable(tree, ref id, "TopSightingsTable", topX + 140000L, botY + 400000L, fullW - 280000L, botH - 520000L, headers, rows, 6);
    }

    private static void DrawVectorBarChart(
        P.ShapeTree tree,
        ref uint id,
        long x,
        long y,
        long w,
        long h,
        List<CmfExecutiveChartSeriesItem> items,
        string s1Name,
        string s2Name,
        string s3Name)
    {
        // Legend row
        AddRectangle(tree, ref id, "Leg1", x, y, 160000L, 140000L, "F59E0B", null);
        AddTextBox(tree, ref id, "Leg1Txt", x + 190000L, y - 30000L, 1500000L, 220000L, s1Name, 950, true, "334155");
        AddRectangle(tree, ref id, "Leg2", x + 1750000L, y, 160000L, 140000L, "10B981", null);
        AddTextBox(tree, ref id, "Leg2Txt", x + 1940000L, y - 30000L, 1500000L, 220000L, s2Name, 950, true, "334155");
        AddRectangle(tree, ref id, "Leg3", x + 3500000L, y, 160000L, 140000L, "6366F1", null);
        AddTextBox(tree, ref id, "Leg3Txt", x + 3690000L, y - 30000L, 1500000L, 220000L, s3Name, 950, true, "334155");

        long plotY = y + 340000L;
        long plotH = h - 820000L;
        long baselineY = plotY + plotH;

        // Baseline axis
        AddRectangle(tree, ref id, "AxisLine", x, baselineY, w, 18000L, "94A3B8", null);

        int count = Math.Min(6, items.Count);
        if (count == 0)
        {
            AddTextBox(tree, ref id, "NoChartData", x + 300000L, plotY + 600000L, w - 600000L, 400000L, "No comparative series data in selected scope.", 1100, false, "64748B");
            return;
        }

        int maxVal = 1;
        for (int i = 0; i < count; i++)
        {
            if (items[i].PrimaryValue > maxVal) maxVal = items[i].PrimaryValue;
            if (items[i].SecondaryValue > maxVal) maxVal = items[i].SecondaryValue;
            if (items[i].TertiaryValue > maxVal) maxVal = items[i].TertiaryValue;
        }

        long groupW = w / count;
        long barW = Math.Min(260000L, (groupW - 240000L) / 3);
        string[] colors = new[] { "F59E0B", "10B981", "6366F1" };

        for (int i = 0; i < count; i++)
        {
            CmfExecutiveChartSeriesItem item = items[i];
            int[] vals = new[] { item.PrimaryValue, item.SecondaryValue, item.TertiaryValue };
            long groupStartX = x + (i * groupW) + ((groupW - (barW * 3 + 60000L)) / 2);

            for (int b = 0; b < 3; b++)
            {
                long barH = vals[b] <= 0 ? 25000L : Math.Max(60000L, (long)((double)vals[b] / maxVal * (plotH - 260000L)));
                long bx = groupStartX + b * (barW + 30000L);
                long by = baselineY - barH;
                AddRectangle(tree, ref id, "Bar_" + i + "_" + b, bx, by, barW, barH, colors[b], null);
                AddTextBox(tree, ref id, "BarVal_" + i + "_" + b, bx - 40000L, by - 230000L, barW + 80000L, 210000L, vals[b].ToString(), 850, true, "1E293B", A.TextAlignmentTypeValues.Center);
            }

            AddTextBox(tree, ref id, "CatLbl_" + i, x + (i * groupW), baselineY + 40000L, groupW, 360000L, item.Category ?? ("Group " + (i + 1)), 950, true, "0F355C", A.TextAlignmentTypeValues.Center);
        }
    }

    private static void DrawHorizontalComponentBars(P.ShapeTree tree, ref uint id, long x, long y, long w, long h, List<CmfExecutiveChartSeriesItem> items)
    {
        int count = Math.Min(5, items != null ? items.Count : 0);
        if (count == 0)
        {
            AddTextBox(tree, ref id, "NoCompBars", x, y + 300000L, w, 300000L, "No component distribution data available.", 1100, false, "64748B");
            return;
        }

        int maxTotal = 1;
        for (int i = 0; i < count; i++)
        {
            int sum = Math.Max(0, items[i].PrimaryValue) + Math.Max(0, items[i].SecondaryValue) + Math.Max(0, items[i].TertiaryValue);
            if (sum > maxTotal) maxTotal = sum;
        }

        long rowStep = h / count;
        long labelW = 2100000L;
        long maxBarW = w - labelW - 1600000L;

        for (int i = 0; i < count; i++)
        {
            CmfExecutiveChartSeriesItem item = items[i];
            long ry = y + i * rowStep;
            int total = Math.Max(0, item.PrimaryValue) + Math.Max(0, item.SecondaryValue) + Math.Max(0, item.TertiaryValue);
            long barW = total <= 0 ? 60000L : Math.Max(140000L, (long)((double)total / maxTotal * maxBarW));

            AddTextBox(tree, ref id, "CompRowLbl_" + i, x, ry, labelW - 80000L, rowStep - 20000L, item.Category ?? "Component", 1000, true, "1E293B");
            AddRectangle(tree, ref id, "CompRowBar_" + i, x + labelW, ry + 55000L, barW, Math.Min(185000L, rowStep - 90000L), "0F5EA8", null);
            string metricDetail = string.Format("Total: {0}  (OK: {1} | Ask: {2} | Dup: {3})", total, item.SecondaryValue, item.PrimaryValue, item.TertiaryValue);
            AddTextBox(tree, ref id, "CompRowVal_" + i, x + labelW + barW + 100000L, ry, 2500000L, rowStep - 20000L, metricDetail, 950, false, "334155");
        }
    }

    private static void AddStandardSlideHeader(P.ShapeTree tree, ref uint id, string title, string scope)
    {
        AddRectangle(tree, ref id, "BgCanvas_" + id, 0, 0, SlideWidth, SlideHeight, "F4F8FB", null);
        AddRectangle(tree, ref id, "TopHeader_" + id, 0, 0, SlideWidth, 960000L, "0A2540", null);
        AddRectangle(tree, ref id, "TopStripe_" + id, 0, 960000L, SlideWidth, 40000L, "00A6B2", null);
        AddTextBox(tree, ref id, "SlideTitle_" + id, 550000L, 160000L, 8600000L, 580000L, title ?? "CMF Pulse Executive View", 2000, true, "FFFFFF");
        string badge = string.IsNullOrWhiteSpace(scope) ? "CMF PULSE AI" : "SCOPE: " + scope.ToUpperInvariant();
        AddRectangle(tree, ref id, "ScopePill_" + id, SlideWidth - 3150000L, 240000L, 2600000L, 440000L, "0F5EA8", "38BDF8", badge, 1000, true, "E0F2FE", A.TextAlignmentTypeValues.Center);
    }

    private static void AddStyledTable(
        P.ShapeTree tree,
        ref uint id,
        string name,
        long x,
        long y,
        long w,
        long h,
        List<string> headers,
        List<string[]> rows,
        int maxRows)
    {
        int colCount = Math.Max(1, headers.Count);
        A.Table table = new A.Table(
            new A.TableProperties { FirstRow = true, BandRow = true });

        A.TableGrid grid = new A.TableGrid();
        for (int c = 0; c < colCount; c++)
        {
            long colW = (c == colCount - 1 && colCount >= 5) ? (long)(w * 0.34) : (w / colCount);
            grid.AppendChild(new A.GridColumn { Width = colW });
        }
        table.AppendChild(grid);

        // Header Row
        A.TableRow headerRow = new A.TableRow { Height = 340000L };
        for (int c = 0; c < colCount; c++)
        {
            headerRow.AppendChild(CreateStyledCell(headers[c], "0F355C", "FFFFFF", true, 950));
        }
        table.AppendChild(headerRow);

        int count = Math.Min(maxRows, rows != null ? rows.Count : 0);
        if (count == 0)
        {
            A.TableRow emptyRow = new A.TableRow { Height = 340000L };
            for (int c = 0; c < colCount; c++)
            {
                emptyRow.AppendChild(CreateStyledCell(c == 0 ? "No records in selected filter" : "-", "FFFFFF", "64748B", false, 900));
            }
            table.AppendChild(emptyRow);
        }
        else
        {
            for (int r = 0; r < count; r++)
            {
                string[] vals = rows[r] ?? new string[0];
                string bg = (r % 2 == 0) ? "FFFFFF" : "F1F5F9";
                A.TableRow tr = new A.TableRow { Height = 330000L };
                for (int c = 0; c < colCount; c++)
                {
                    string cellText = c < vals.Length ? (vals[c] ?? "-") : "-";
                    if (cellText.Length > 72) cellText = cellText.Substring(0, 69) + "...";
                    tr.AppendChild(CreateStyledCell(cellText, bg, "1E293B", c == 0, 900));
                }
                table.AppendChild(tr);
            }
        }

        P.GraphicFrame frame = new P.GraphicFrame(
            new P.NonVisualGraphicFrameProperties(
                new P.NonVisualDrawingProperties { Id = id++, Name = name },
                new P.NonVisualGraphicFrameDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.Transform(
                new A.Offset { X = x, Y = y },
                new A.Extents { Cx = w, Cy = h }),
            new A.Graphic(
                new A.GraphicData(table) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/table" }));

        tree.AppendChild(frame);
    }

    private static A.TableCell CreateStyledCell(string text, string fillHex, string fontHex, bool bold, int fontSizeHundredths)
    {
        A.RunProperties runProps = new A.RunProperties { Language = "en-US", FontSize = fontSizeHundredths, Bold = bold };
        runProps.Append(new A.SolidFill(new A.RgbColorModelHex { Val = fontHex }));
        runProps.Append(new A.LatinFont { Typeface = "Calibri" });

        A.Paragraph para = new A.Paragraph(
            new A.ParagraphProperties { Alignment = A.TextAlignmentTypeValues.Left },
            new A.Run(runProps, new A.Text(text ?? string.Empty)));

        A.TableCellProperties cellProps = new A.TableCellProperties(
            new A.SolidFill(new A.RgbColorModelHex { Val = fillHex }))
        {
            Anchor = A.TextAnchoringTypeValues.Center,
            LeftMargin = 65000,
            RightMargin = 65000,
            TopMargin = 35000,
            BottomMargin = 35000
        };

        return new A.TableCell(
            new A.TextBody(new A.BodyProperties(), new A.ListStyle(), para),
            cellProps);
    }

    private static void AddRectangle(
        P.ShapeTree tree,
        ref uint id,
        string name,
        long x,
        long y,
        long w,
        long h,
        string fillHex,
        string borderHex,
        string text = null,
        int fontSize = 1200,
        bool bold = false,
        string fontHex = "1E293B",
        A.TextAlignmentTypeValues align = A.TextAlignmentTypeValues.Left)
    {
        P.ShapeProperties spPr = new P.ShapeProperties(
            new A.Transform2D(
                new A.Offset { X = x, Y = y },
                new A.Extents { Cx = w, Cy = h }),
            new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle });

        if (!string.IsNullOrWhiteSpace(fillHex))
        {
            spPr.Append(new A.SolidFill(new A.RgbColorModelHex { Val = fillHex }));
        }
        else
        {
            spPr.Append(new A.NoFill());
        }

        if (!string.IsNullOrWhiteSpace(borderHex))
        {
            spPr.Append(new A.Outline(new A.SolidFill(new A.RgbColorModelHex { Val = borderHex })) { Width = 12700 });
        }
        else
        {
            spPr.Append(new A.Outline(new A.NoFill()));
        }

        A.Paragraph para = new A.Paragraph(new A.ParagraphProperties { Alignment = align });
        if (!string.IsNullOrEmpty(text))
        {
            A.RunProperties rp = new A.RunProperties { Language = "en-US", FontSize = fontSize, Bold = bold };
            rp.Append(new A.SolidFill(new A.RgbColorModelHex { Val = fontHex }));
            rp.Append(new A.LatinFont { Typeface = "Calibri" });
            para.Append(new A.Run(rp, new A.Text(text)));
        }

        P.Shape shape = new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id++, Name = name },
                new P.NonVisualShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()),
            spPr,
            new P.TextBody(
                new A.BodyProperties { Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Center },
                new A.ListStyle(),
                para));

        tree.AppendChild(shape);
    }

    private static void AddTextBox(
        P.ShapeTree tree,
        ref uint id,
        string name,
        long x,
        long y,
        long w,
        long h,
        string text,
        int fontSize,
        bool bold,
        string fontHex,
        A.TextAlignmentTypeValues align = A.TextAlignmentTypeValues.Left)
    {
        P.ShapeProperties spPr = new P.ShapeProperties(
            new A.Transform2D(
                new A.Offset { X = x, Y = y },
                new A.Extents { Cx = w, Cy = h }),
            new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle },
            new A.NoFill(),
            new A.Outline(new A.NoFill()));

        A.RunProperties rp = new A.RunProperties { Language = "en-US", FontSize = fontSize, Bold = bold };
        rp.Append(new A.SolidFill(new A.RgbColorModelHex { Val = fontHex }));
        rp.Append(new A.LatinFont { Typeface = "Calibri" });

        A.Paragraph para = new A.Paragraph(
            new A.ParagraphProperties { Alignment = align },
            new A.Run(rp, new A.Text(text ?? string.Empty)));

        P.Shape shape = new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id++, Name = name },
                new P.NonVisualShapeDrawingProperties { TextBox = true },
                new P.ApplicationNonVisualDrawingProperties()),
            spPr,
            new P.TextBody(
                new A.BodyProperties { Wrap = A.TextWrappingValues.Square, LeftInset = 45000, RightInset = 45000, TopInset = 25000, BottomInset = 25000 },
                new A.ListStyle(),
                para));

        tree.AppendChild(shape);
    }

    private static string CleanMarkdownMarkers(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        string text = value.Replace("**", string.Empty).Replace("__", string.Empty).Trim();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*[-*•]\s+", string.Empty);
        return text.Trim();
    }

    private static SlidePart CreateBlankSlidePart(PresentationPart presentationPart, SlideLayoutPart slideLayoutPart)
    {
        SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
        slidePart.Slide = new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()))),
            new P.ColorMapOverride(new A.MasterColorMapping()));
        slidePart.AddPart(slideLayoutPart);
        return slidePart;
    }

    private static SlideMasterPart CreateSlideMasterPart(PresentationPart presentationPart)
    {
        SlideMasterPart slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
        SlideLayoutPart slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
        ThemePart themePart = presentationPart.AddNewPart<ThemePart>();
        slideMasterPart.AddPart(themePart);

        themePart.Theme = new A.Theme(
            new A.ThemeElements(
                new A.ColorScheme(
                    new A.Dark1Color(new A.SystemColor { Val = A.SystemColorValues.WindowText, LastColor = "000000" }),
                    new A.Light1Color(new A.SystemColor { Val = A.SystemColorValues.Window, LastColor = "FFFFFF" }),
                    new A.Dark2Color(new A.RgbColorModelHex { Val = "0A2540" }),
                    new A.Light2Color(new A.RgbColorModelHex { Val = "F4F8FB" }),
                    new A.Accent1Color(new A.RgbColorModelHex { Val = "0F5EA8" }),
                    new A.Accent2Color(new A.RgbColorModelHex { Val = "00A6B2" }),
                    new A.Accent3Color(new A.RgbColorModelHex { Val = "10B981" }),
                    new A.Accent4Color(new A.RgbColorModelHex { Val = "F59E0B" }),
                    new A.Accent5Color(new A.RgbColorModelHex { Val = "6366F1" }),
                    new A.Accent6Color(new A.RgbColorModelHex { Val = "EF4444" }),
                    new A.Hyperlink(new A.RgbColorModelHex { Val = "0F5EA8" }),
                    new A.FollowedHyperlinkColor(new A.RgbColorModelHex { Val = "475569" }))
                { Name = "CMF Pulse Theme" },
                new A.FontScheme(
                    new A.MajorFont(new A.LatinFont { Typeface = "Calibri" }, new A.EastAsianFont { Typeface = "" }, new A.ComplexScriptFont { Typeface = "" }),
                    new A.MinorFont(new A.LatinFont { Typeface = "Calibri" }, new A.EastAsianFont { Typeface = "" }, new A.ComplexScriptFont { Typeface = "" }))
                { Name = "CMF Pulse Fonts" },
                new A.FormatScheme(
                    new A.FillStyleList(new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.PhColor }), new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.PhColor }), new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.PhColor })),
                    new A.LineStyleList(new A.Outline { Width = 12700 }, new A.Outline { Width = 12700 }, new A.Outline { Width = 12700 }),
                    new A.EffectStyleList(new A.EffectStyle(new A.EffectList()), new A.EffectStyle(new A.EffectList()), new A.EffectStyle(new A.EffectList())),
                    new A.BackgroundFillStyleList(new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.PhColor }), new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.PhColor }), new A.SolidFill(new A.SchemeColor { Val = A.SchemeColorValues.PhColor })))
                { Name = "CMF Pulse Format" }))
        { Name = "CMF Pulse Office Theme" };

        slideLayoutPart.SlideLayout = new P.SlideLayout(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()))),
            new P.ColorMapOverride(new A.MasterColorMapping()))
        { Type = P.SlideLayoutValues.Blank };
        slideLayoutPart.AddPart(slideMasterPart);

        string layoutRelId = slideMasterPart.GetIdOfPart(slideLayoutPart);
        slideMasterPart.SlideMaster = new P.SlideMaster(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()),
                    new P.GroupShapeProperties(new A.TransformGroup()))),
            new P.ColorMap
            {
                Background1 = A.ColorSchemeIndexValues.Light1,
                Text1 = A.ColorSchemeIndexValues.Dark1,
                Background2 = A.ColorSchemeIndexValues.Light2,
                Text2 = A.ColorSchemeIndexValues.Dark2,
                Accent1 = A.ColorSchemeIndexValues.Accent1,
                Accent2 = A.ColorSchemeIndexValues.Accent2,
                Accent3 = A.ColorSchemeIndexValues.Accent3,
                Accent4 = A.ColorSchemeIndexValues.Accent4,
                Accent5 = A.ColorSchemeIndexValues.Accent5,
                Accent6 = A.ColorSchemeIndexValues.Accent6,
                Hyperlink = A.ColorSchemeIndexValues.Hyperlink,
                FollowedHyperlink = A.ColorSchemeIndexValues.FollowedHyperlink
            },
            new P.SlideLayoutIdList(new P.SlideLayoutId { Id = 2147483649U, RelationshipId = layoutRelId }),
            new P.TextStyles(new P.TitleStyle(), new P.BodyStyle(), new P.OtherStyle()));

        return slideMasterPart;
    }
}
