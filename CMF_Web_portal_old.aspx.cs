using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;


public class CMFIssueCountTemplate : ITemplate
{
    private string _dataField;
    private string _driver;
    private string _issueType;

    public CMFIssueCountTemplate(string dataField, string driver, string issueType)
    {
        _dataField = dataField;
        _driver = driver;
        _issueType = issueType;
    }

    public void InstantiateIn(System.Web.UI.Control container)
    {
        System.Web.UI.WebControls.Literal literal = new System.Web.UI.WebControls.Literal();
        literal.DataBinding += new EventHandler(literal_DataBinding);
        container.Controls.Add(literal);
    }

    void literal_DataBinding(object sender, EventArgs e)
    {
        System.Web.UI.WebControls.Literal literal = (System.Web.UI.WebControls.Literal)sender;
        GridViewRow row = (GridViewRow)literal.NamingContainer;

        // Add null checks for DataBinder.Eval
        object valueObj = DataBinder.Eval(row.DataItem, _dataField);
        object componentObj = DataBinder.Eval(row.DataItem, "Component");

        string value = valueObj != null ? valueObj.ToString() : "0";
        string component = componentObj != null ? componentObj.ToString() : "";

        // Extract numeric value from the formatted string (e.g., "5(2)" -> "5")
        string numericValue = value;
        if (value.Contains("("))
        {
            numericValue = value.Substring(0, value.IndexOf("("));
        }

        // Remove any non-numeric characters for the count check
        string cleanValue = System.Text.RegularExpressions.Regex.Replace(numericValue, @"[^\d]", "");

        if (!string.IsNullOrEmpty(cleanValue) && cleanValue != "0")
        {
            // Determine link color based on whether it's the Total row
            string linkColor = component == "Total (LOS) + Duplicates + Implemented" ? "black" : "";

            // Create clickable link with dynamic color and bold font
            literal.Text = string.Format(
                @"<a href=""javascript:void(0)"" onclick='showCMFIssues(""{0}"", ""{1}"", ""{2}""); return false;' style=""color:{3}; text-decoration:none; font-weight:bold;"" class=""btn btn-link p-0"">{4}</a>",
                HttpUtility.JavaScriptStringEncode(component),
                HttpUtility.JavaScriptStringEncode(_driver),
                HttpUtility.JavaScriptStringEncode(_issueType),
                linkColor,
                value
            );
        }
        else
        {
            // Make non-clickable values bold as well for consistency
            literal.Text = string.Format("<span style=\"font-weight:bold;\">{0}</span>", value);
        }
    }
}

public partial class CMF_Web_portal_old : System.Web.UI.Page
{
    private List<string> drivers = new List<string>();

    // Add this public property
    public string DriversJson
    {
        get
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(drivers.ToArray());
        }
    }

    // Or add this public property for simple array
    public string[] DriversArray
    {
        get
        {
            return drivers.ToArray();
        }
    }

    protected string FormatStatusText(object statusValue)
    {
        string statusText = statusValue == null ? string.Empty : statusValue.ToString();

        if (string.IsNullOrEmpty(statusText))
        {
            return string.Empty;
        }

        string normalizedStatus = statusText
            .Replace("\\r\\n", "\n")
            .Replace("\\n", "\n")
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        return HttpUtility.HtmlEncode(normalizedStatus).Replace("\n", "<br />");
    }

    public static string WorkWeek;
    //private List<string> drivers;
    private Dictionary<string, string> driverColumns;

    private void SetWorkWeek()
    {
        // Get the current date
        DateTime currentDate = DateTime.Now;

        // Get the CultureInfo for the current culture (en-US in this case)
        CultureInfo cultureInfo = CultureInfo.CurrentCulture;

        // Get the current work week number (WW)
        int workWeekNumber = cultureInfo.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        // Format the work week as WW'09, WW'10, etc.
        WorkWeek = string.Format("WW'{0:D2}", workWeekNumber);

    }


    private string headerTemplate = WorkWeek + " | CMF Live Dashboard - ";
    private string selectedPlatform;
    private string selectedValue; // Default table
    private string driver = " | CMF Live Dashboard - All Milestones";
    private int totalCount = 0;
    private int duplicates = 0;
    private int closedCount = 0;
    private int implementedCount = 0;
    private string implementedComponents = "";

    private void ShowModal0()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "$('#detailsModal0').modal('show');", true);
    }

    private void ShowModal()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "$('#detailsModal').modal('show');", true);
    }

    private void ShowModal2()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "$('#detailsModal2').modal('show');", true);
    }

    private void ShowModal3()
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "$('#detailsModal3').modal('show');", true);
    }

    private void BindImplementedVerifiedDetailsModal(string design)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string modalquery;

        // Get the current filter value to determine which drivers are selected
        string filterValue = Session["filterValue"] as string;
        string driverFilter = "";

        // Build driver filter clause if specific drivers are selected
        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
        {
            string[] rawDrivers = filterValue.Split(new char[] { ',' });
            List<string> driversList = new List<string>();

            foreach (string driver in rawDrivers)
            {
                string trimmedDriver = driver.Trim();
                if (!string.IsNullOrEmpty(trimmedDriver))
                {
                    driversList.Add("'" + trimmedDriver + "'");
                }
            }

            if (driversList.Count > 0)
            {
                driverFilter = " AND drivers IN (" + string.Join(", ", driversList) + ") ";
            }
        }

        if (design == "Total")
        {
            modalquery =
                "SELECT cp_id, title, status, component, cmf_request, customer_owner, " +
                "promoted_id, closed_reason, days_active, idst, drivers " +
                "FROM " + selectedPlatform + " " +
                "WHERE sysdebug LIKE('%customer_must_fix%') " +
                "AND status IN ('implemented', 'verified') " +
                driverFilter; // Apply driver filter
        }
        else if (design == "RVP")
        {
            modalquery =
                "SELECT cp_id, title, status, component, cmf_request, customer_owner, " +
                "promoted_id, closed_reason, days_active, idst, drivers " +
                "FROM " + selectedPlatform + " " +
                "WHERE (customer_detail = '' OR customer_detail IS NULL) " +
                "AND sysdebug LIKE('%customer_must_fix%') " +
                "AND status IN ('implemented', 'verified') " +
                driverFilter; // Apply driver filter
        }
        else
        {
            modalquery =
                "SELECT cp_id, title, status, component, cmf_request, customer_owner, " +
                "promoted_id, closed_reason, days_active, idst, drivers " +
                "FROM " + selectedPlatform + " " +
                "WHERE customer_detail = '" + design + "' " +
                "AND sysdebug LIKE('%customer_must_fix%') " +
                "AND status IN ('implemented', 'verified') " +
                driverFilter; // Apply driver filter
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlDataAdapter da = new SqlDataAdapter(modalquery, con))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView_design_summary_modal0.DataSource = dt;
            GridView_design_summary_modal0.DataBind();
        }
    }

    //private void BindImplementedVerifiedDetailsModal(string design)
    //{
    //    string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
    //    string modalquery;
    //    if (design == "Total")
    //    {
    //        modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE sysdebug LIKE('%customer_must_fix%') AND status IN ('implemented', 'verified')";
    //    }
    //    else if (design == "RVP")
    //    {
    //        modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] " +
    //                "FROM " + selectedPlatform + " " +
    //                "WHERE drivers = '" + driver + "' " +
    //                "AND (customer_detail = '' OR customer_detail IS NULL) " + // Check for empty or NULL customer_detail
    //                "AND sysdebug LIKE('%customer_must_fix%') " +
    //                "AND status IN ('implemented', 'verified')";
    //    }
    //    else
    //    {
    //        modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE customer_detail = '" + design + "' AND sysdebug LIKE('%customer_must_fix%') AND status IN ('implemented', 'verified')";
    //    }


    //    using (SqlConnection con = new SqlConnection(connectionString))
    //    using (SqlCommand cmd = new SqlCommand(modalquery, con))
    //    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
    //    {
    //        DataTable dt = new DataTable();
    //        da.Fill(dt);
    //        GridView_design_summary_modal0.DataSource = dt;
    //        GridView_design_summary_modal0.DataBind();
    //    }
    //}

    private void BindDriverDetailsModal(string design, string driver)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string modalquery;
        if (design == "Total")
        {
            modalquery = "SELECT[cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE drivers = '" + driver + "' AND sysdebug LIKE('%customer_must_fix%') AND status IN('open')";
        }
        else if (design == "RVP")
        {
            modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] " +
                    "FROM " + selectedPlatform + " " +
                    "WHERE drivers = '" + driver + "' " +
                    "AND (customer_detail = '' OR customer_detail IS NULL) " + // Check for empty or NULL customer_detail
                    "AND sysdebug LIKE('%customer_must_fix%') " +
                    "AND status IN ('open')";
        }
        else
        {
            modalquery = "SELECT [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE drivers = '" + driver + "' AND customer_detail = '" + design + "' AND sysdebug LIKE('%customer_must_fix%') AND status IN ('open')";
        }


        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView_design_summary_modal0.DataSource = dt;
            GridView_design_summary_modal0.DataBind();
        }
    }

    private void RegisterFieldSelectorScript()
    {
        string script = @"
            // Ensure field selector is initialized after postback
            if (typeof(resetToDefault) === 'function') {
                resetToDefault();
            }
        ";

        ClientScript.RegisterStartupScript(this.GetType(), "FieldSelectorInit", script, true);
    }
    protected void btnShowGridView1_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        searchfilters.Visible = false;
        // Show GridView1 and hide GridView2
        overall_request_details.Visible = true;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        btnExportToExcel.Visible = true;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = true;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        fieldSelectorPanel.Visible = true;
        // Show or hide the div based on your condition
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        //RegisterFieldSelectorScript();
    }

    protected void btnShowGridView2_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        GridView2_edit.EditIndex = -1;
        GridView_design_open.EditIndex = -1;
        searchfilters.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = true;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = true;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = true;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = true;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        fieldSelectorPanel.Visible = false;
    }

    protected void btnShowGridView3_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        searchfilters.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = true;
        GridView_cmf_summary1.Visible = true;
        GridView_milestone_map.Visible = true;
        GridView_notes.Visible = true;
        GridView_comp.Visible = true;
        tptdefdiv.Visible = true;
        GridView_comp.Visible = true;
        //tptdefdiv.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        btnImportPopup.Visible = false;
        pane1.Visible = true;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        fieldSelectorPanel.Visible = false;
    }

    protected void btnShowGridView4_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        searchfilters.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = true;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        GridView_cmf_pending.Visible = true;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        btnImportPopup.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = true;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = false;
        fieldSelectorPanel.Visible = false;
    }
    protected void btnShowGridView5_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        searchfilters.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;
        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = true;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = true;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = true;
        pane6.Visible = false;
        pane7.Visible = false;
        fieldSelectorPanel.Visible = false;
    }
    protected void btnShowGridView6_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        searchfilters.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = true;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = false;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = true;
        GridView_oem_summary.Visible = false;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = true;
        pane7.Visible = false;
        fieldSelectorPanel.Visible = false;
    }

    protected void btnShowGridView7_Click(object sender, EventArgs e)
    {
        GridView2_edit.Visible = false;
        searchfilters.Visible = false;
        // Show GridView2 and hide GridView1
        btnExportToExcel.Visible = false;

        btnExportToExcel_ig.Visible = false;
        btnExportToExcel_des.Visible = false;
        btnExportToExcel_des_summary.Visible = false;
        btnExportToExcel_cmf_pending.Visible = false;
        btnExportToExcel_oem.Visible = true;
        overall_request_details.Visible = false;
        GridView_design_open.Visible = false;
        GridView_cmf_summary.Visible = false;
        GridView_cmf_summary1.Visible = false;
        GridView_milestone_map.Visible = false;
        btnImportPopup.Visible = false;
        GridView_cmf_pending.Visible = false;
        GridView_design_summary.Visible = false;
        GridView_component_summary.Visible = false;
        GridView_oem_summary.Visible = true;
        GridView_notes.Visible = false;
        GridView_comp.Visible = false;
        tptdefdiv.Visible = false;
        pane1.Visible = false;
        pane2.Visible = false;
        pane3.Visible = false;
        pane4.Visible = false;
        pane5.Visible = false;
        pane6.Visible = false;
        pane7.Visible = true;
        fieldSelectorPanel.Visible = false;
        BindGridView_oem_summary();
    }
    private void BindDistinctValues()

    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        // Query to select distinct values from a column
        //string query = "SELECT DISTINCT \r\n    CASE\r\n        WHEN \r\n            -- Check if 'must_fix_for' contains 'WW' followed by a number (including floats, ignoring the decimal part)\r\n            (PATINDEX('%WW[0-3][0-9]%', must_fix_for) > 0 \r\n             AND \r\n                -- Extract the number after 'WW', remove non-numeric characters (including decimals), and convert to integer\r\n                TRY_CAST(FLOOR(CAST(SUBSTRING(must_fix_for, PATINDEX('%WW[0-3][0-9]%', must_fix_for) + 2, 2) AS FLOAT)) AS INT) BETWEEN 1 AND 31)\r\n        THEN 'Pre-PV'\r\n        ELSE must_fix_for\r\n    END AS must_fix_for\r\nFROM \r\n    " + selectedPlatform;
        string query = "SELECT DISTINCT drivers FROM " + selectedPlatform + " WHERE drivers not in (' ') and sysdebug like ('%customer_must_fix%')";

        // Create and configure the SqlConnection and SqlDataAdapter
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
            System.Data.DataTable dt = new System.Data.DataTable();

            // Fill the DataTable with distinct values
            dataAdapter.Fill(dt);

            // Bind the distinct values to the Repeater control
            rptDistinctValues.DataSource = dt;
            rptDistinctValues.DataBind();
        }
    }



    private void BindDistinctFilters()

    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        // Query to select distinct values from a column
        //string query = "SELECT DISTINCT \r\n    CASE\r\n        WHEN \r\n            -- Check if 'must_fix_for' contains 'WW' followed by a number (including floats, ignoring the decimal part)\r\n            (PATINDEX('%WW[0-3][0-9]%', must_fix_for) > 0 \r\n             AND \r\n                -- Extract the number after 'WW', remove non-numeric characters (including decimals), and convert to integer\r\n                TRY_CAST(FLOOR(CAST(SUBSTRING(must_fix_for, PATINDEX('%WW[0-3][0-9]%', must_fix_for) + 2, 2) AS FLOAT)) AS INT) BETWEEN 1 AND 31)\r\n        THEN 'Pre-PV'\r\n        ELSE must_fix_for\r\n    END AS must_fix_for\r\nFROM \r\n    " + selectedPlatform;
        string query = "SELECT DISTINCT drivers FROM " + selectedPlatform + " WHERE drivers not in (' ')";

        // Create and configure the SqlConnection and SqlDataAdapter
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
            System.Data.DataTable dt = new System.Data.DataTable();

            // Fill the DataTable with distinct values
            dataAdapter.Fill(dt);

            // Bind the distinct values to the Repeater control
            rptDistinctFilters.DataSource = dt;
            rptDistinctFilters.DataBind();
        }
    }




    protected void ddlTables_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Capture the selected value from the dropdown
        selectedPlatform = ddlTables.SelectedValue;

        // Store the selected table value in the session to persist it across postbacks
        Session["selectedPlatform"] = selectedPlatform;
        headerTitle.InnerText = WorkWeek + " | CMF Live Dashboard - All Milestones";

        // Update the platform-specific dashboard link
        UpdatePlatformDashboardLink();

        // Rebind the grid and distinct values for the selected table
        BindGridView();
        BindDistinctValues();
        BindDistinctFilters();
    }

    // Helper method to update platform-specific dashboard link
    private void UpdatePlatformDashboardLink()
    {
        // Dictionary mapping platform values to their CDR links
        Dictionary<string, string> platformLinks = new Dictionary<string, string>()
        {
            { "CMF_PTL_ALL_COMPONENTS_TABLE", "https://cdrdv2.intel.com/v1/dl/getContent/833017" },
            { "CMF_GNR_ALL_COMPONENTS_TABLE", "https://cdrdv2.intel.com/v1/dl/getContent/824182" },
            { "CMF_WCL_ALL_COMPONENTS_TABLE", "https://cdrdv2.intel.com/v1/dl/getContent/858122" }
        };

        // Dictionary mapping platform values to their display names
        Dictionary<string, string> platformNames = new Dictionary<string, string>()
        {
            { "CMF_PTL_ALL_COMPONENTS_TABLE", "PTL Dashboard" },
            { "CMF_GNR_ALL_COMPONENTS_TABLE", "GNR Dashboard" },
            { "CMF_WCL_ALL_COMPONENTS_TABLE", "WCL Dashboard" }
        };

        HashSet<string> nvlPlatforms = new HashSet<string>
        {
            "CMF_NVL_S_ALL_COMPONENTS_TABLE",
            "CMF_NVL_H_ALL_COMPONENTS_TABLE",
            "CMF_NVL_U_ALL_COMPONENTS_TABLE",
            "CMF_NVL_Hx_ALL_COMPONENTS_TABLE"
        };

        string currentPlatform = Session["selectedPlatform"] as string ?? ddlTables.SelectedValue;

        // Check if there's a link for this platform
        if (platformLinks.ContainsKey(currentPlatform))
        {
            lnkPlatformDashboard.NavigateUrl = platformLinks[currentPlatform];
            lnkPlatformDashboard.Text = platformNames[currentPlatform];
            lnkPlatformDashboard.Visible = true;
        }
        else
        {
            // Hide the link if no dashboard exists for this platform
            lnkPlatformDashboard.Visible = false;
        }

        if (nvlPlatforms.Contains(currentPlatform))
        {
            lnkNvlDashboardDownload.NavigateUrl = "https://cdrdv2.intel.com/v1/dl/getContent/862842";
            lnkNvlDashboardDownload.Visible = true;
        }
        else
        {
            lnkNvlDashboardDownload.Visible = false;
        }
    }

    [Serializable]
    public class FilterItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public FilterItem() { }

        public FilterItem(string text, string value)
        {
            Text = text;
            Value = value;
        }
    }

    // Remove ddlProgressHeader_SelectedIndexChanged and ddlComponentHeader_SelectedIndexChanged

    protected void ddlOwnerHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["ownerFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlRvpReproHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["rvpReproFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlIdstHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["idstFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlCompanyHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["companyFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlDetailHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["detailFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlComponentHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["componentFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlLosHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["losFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlMilestoneHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["milestoneFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    protected void ddlCmfRequestHeader_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = sender as DropDownList;
        if (ddl != null)
        {
            Session["cmfRequestFilter"] = ddl.SelectedValue;
            ApplyAllFilters();
        }
    }

    private void ApplyAllFilters()
    {
        string filterValue = Session["filterValue"] as string;
        Dictionary<string, string> filters = GetAllFilterValues();
        BindGridView(filterValue, filters);
    }

    // Updated to remove progress and component filters
    private Dictionary<string, string> GetAllFilterValues()
    {
        return new Dictionary<string, string>
    {
        {"owner", Session["ownerFilter"] as string ?? "All"},
        {"rvpRepro", Session["rvpReproFilter"] as string ?? "All"},
        {"idst", Session["idstFilter"] as string ?? "All"},
        {"los", Session["losFilter"] as string ?? "All"},
        {"milestone", Session["milestoneFilter"] as string ?? "All"},
        {"cmfRequest", Session["cmfRequestFilter"] as string ?? "All"},
        {"Company", Session["companyFilter"] as string ?? "All"},      // Fixed: use "companyFilter"
        {"Detail", Session["detailFilter"] as string ?? "All"},        // Fixed: use "detailFilter"
        {"Component", Session["componentFilter"] as string ?? "All"},  // Fixed: use "componentFilter"
    };
    }

    // Updated to remove progress and component filters
    private void BindAllFilters()
    {
        // Get the current platform
        string currentPlatform = Session["selectedPlatform"] as string ?? ddlTables.SelectedValue;

        string cs = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (var con = new SqlConnection(cs))
        {
            con.Open();

            BindOwnerFilter(con, currentPlatform);
            BindFilterData(con, "repro_on_rvp", "RvpReproItems", currentPlatform);
            BindFilterData(con, "idst", "IdstItems", currentPlatform);
            BindFilterData(con, "los", "LosItems", currentPlatform);
            BindFilterData(con, "drivers", "MilestoneItems", currentPlatform);
            BindFilterData(con, "cmf_request", "CmfRequestItems", currentPlatform);
            BindFilterData(con, "customer_company", "CompanyItems", currentPlatform);
            BindFilterData(con, "customer_detail", "DetailItems", currentPlatform);
            BindFilterData(con, "component_group", "ComponentItems", currentPlatform);
        }
    }

    private void BindFilterData(SqlConnection con, string columnName, string viewStateKey, string platform)
    {
        string query;

        string mappingTable = platform.IndexOf("NVL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_NVL"
            : platform.IndexOf("PTL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_PTL"
            : platform.IndexOf("WCL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_WCL"
            : "idst_mapping_table";

        if (columnName == "los")
        {
            // For LOS, convert NULL/empty to "No" to match GridView processing
            query = "SELECT DISTINCT " +
                    "CASE " +
                    "    WHEN los IS NULL OR LTRIM(RTRIM(los)) = '' THEN 'No' " +
                    "    ELSE LTRIM(RTRIM(los)) " +
                    "END AS filterValue " +
                    "FROM " + platform + " " +
                    "WHERE 1=1 ";
        }
        else if (columnName == "idst")
        {
            query = "SELECT DISTINCT LTRIM(RTRIM(ISNULL(M.idst_display_name, A.idst))) AS filterValue " +
                    "FROM " + platform + " A " +
                    "LEFT JOIN [gfxitt_db].[dbo].[" + mappingTable + "] M ON A.component_group = M.component " +
                    "WHERE 1=1 ";
        }
        else
        {
            query = "SELECT DISTINCT LTRIM(RTRIM(" + columnName + ")) AS filterValue " +
                    "FROM " + platform + " " +
                    "WHERE " + columnName + " IS NOT NULL AND LTRIM(RTRIM(" + columnName + ")) <> '' ";
        }

        // Get the current filter value from session
        string filterValue = Session["filterValue"] as string;

        // Add the same WHERE conditions as the main GridView query
        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
        {
            if (filterValue.Contains(","))
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR ( " +
                        "       @FilterValue LIKE '%,' + drivers + ',%' " +
                        "       OR @FilterValue LIKE drivers + ',%' " +
                        "       OR @FilterValue LIKE '%,' + drivers) ) " +
                        "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
            }
            else
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR drivers = @FilterValue ) AND status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
            }
        }
        else
        {
            // Handle AllDrivers case or when filterValue is empty
            query += " AND status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
        }

        query += " ORDER BY filterValue;";

        using (var cmd = new SqlCommand(query, con))
        {
            // Add parameter if filterValue exists and is not AllDrivers
            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                cmd.Parameters.AddWithValue("@FilterValue", filterValue);
            }

            using (var rdr = cmd.ExecuteReader())
            {
                List<FilterItem> items = new List<FilterItem>();
                items.Add(new FilterItem("All", "All"));

                while (rdr.Read())
                {
                    var v = rdr["filterValue"].ToString();
                    if (!string.IsNullOrEmpty(v)) // Skip empty values
                    {
                        items.Add(new FilterItem(v, v));
                    }
                }

                ViewState[viewStateKey] = items.Select(item => new[] { item.Text, item.Value }).ToArray();
            }
        }
    }

    private void BindOwnerFilter(SqlConnection con, string platform)
    {
        // Get the current filter value from session
        string filterValue = Session["filterValue"] as string;

        string query = "SELECT DISTINCT LTRIM(RTRIM(customer_owner)) AS owner " +
                       "FROM " + platform + " " +
                       "WHERE customer_owner IS NOT NULL AND LTRIM(RTRIM(customer_owner)) <> '' ";

        // Add the same WHERE conditions as the main GridView query
        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
        {
            if (filterValue.Contains(","))
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR ( " +
                        "       @FilterValue LIKE '%,' + drivers + ',%' " +
                        "       OR @FilterValue LIKE drivers + ',%' " +
                        "       OR @FilterValue LIKE '%,' + drivers) ) " +
                        "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
            }
            else
            {
                query += " AND ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' " +
                        " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) " +
                        "    OR drivers = @FilterValue ) AND status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
            }
        }
        else
        {
            // Handle AllDrivers case or when filterValue is empty
            query += " AND status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
        }

        query += " ORDER BY owner;";

        using (var cmd = new SqlCommand(query, con))
        {
            // Add parameter if filterValue exists and is not AllDrivers
            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                cmd.Parameters.AddWithValue("@FilterValue", filterValue);
            }

            using (var rdr = cmd.ExecuteReader())
            {
                List<string> ownerAliases = new List<string>();
                while (rdr.Read())
                {
                    ownerAliases.Add(rdr["owner"].ToString());
                }

                List<FilterItem> items = new List<FilterItem>();
                items.Add(new FilterItem("All", "All"));

                // ... rest of the Active Directory lookup code remains the same ...
                PrincipalContext context_gar = null;
                PrincipalContext context_amr = null;
                PrincipalContext context_ccr = null;
                PrincipalContext context_ger = null;

                try { context_gar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
                try { context_amr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
                try { context_ccr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
                try { context_ger = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

                foreach (string rawOwner in ownerAliases)
                {
                    string processedAlias = ProcessOwnerAlias(rawOwner);
                    string displayName = GetOwnerDisplayName(processedAlias, context_gar, context_amr, context_ccr, context_ger);

                    items.Add(new FilterItem(displayName, rawOwner));
                }

                ViewState["OwnerItems"] = items.Select(item => new[] { item.Text, item.Value }).ToArray();
            }
        }
    }

    private string ProcessOwnerAlias(string rawOwner)
    {
        if (string.IsNullOrEmpty(rawOwner)) return string.Empty;

        string processed = rawOwner.Trim();
        if (processed.Contains("\\")) processed = processed.Substring(processed.LastIndexOf('\\') + 1);
        if (processed.Contains("@")) processed = processed.Split('@')[0];

        return processed.Trim();
    }

    private string GetOwnerDisplayName(string alias, PrincipalContext context_gar, PrincipalContext context_amr, PrincipalContext context_ccr, PrincipalContext context_ger)
    {
        if (string.IsNullOrEmpty(alias)) return "Unable to fetch";

        UserPrincipal up = null;

        if (context_gar != null) up = UserPrincipal.FindByIdentity(context_gar, IdentityType.SamAccountName, alias);
        if (up == null && context_amr != null) up = UserPrincipal.FindByIdentity(context_amr, IdentityType.SamAccountName, alias);
        if (up == null && context_ccr != null) up = UserPrincipal.FindByIdentity(context_ccr, IdentityType.SamAccountName, alias);
        if (up == null && context_ger != null) up = UserPrincipal.FindByIdentity(context_ger, IdentityType.SamAccountName, alias);

        return up != null ? up.DisplayName : alias;
    }

    private string ProcessOwnerName(string rawOwner)
    {
        if (string.IsNullOrEmpty(rawOwner)) return "Unable to fetch";

        string processed = rawOwner.Trim();
        if (processed.Contains("\\")) processed = processed.Substring(processed.LastIndexOf('\\') + 1);
        if (processed.Contains("@")) processed = processed.Split('@')[0];

        return processed;
    }

    // Updated DataBound method to handle both regular and empty data templates
    protected void overall_request_details_DataBound(object sender, EventArgs e)
    {
        // Handle regular header row
        if (overall_request_details.HeaderRow != null)
        {
            PopulateHeaderDropdown("ddlOwnerHeader", "OwnerItems", "ownerFilter");
            PopulateHeaderDropdown("ddlRvpReproHeader", "RvpReproItems", "rvpReproFilter");
            PopulateHeaderDropdown("ddlIdstHeader", "IdstItems", "idstFilter");
            PopulateHeaderDropdown("ddlLosHeader", "LosItems", "losFilter");
            PopulateHeaderDropdown("ddlCompanyHeader", "CompanyItems", "companyFilter");      // Fixed: "companyFilter"
            PopulateHeaderDropdown("ddlDetailHeader", "DetailItems", "detailFilter");        // Fixed: "detailFilter"
            PopulateHeaderDropdown("ddlComponentHeader", "ComponentItems", "componentFilter"); // Fixed: "componentFilter"
            PopulateHeaderDropdown("ddlMilestoneHeader", "MilestoneItems", "milestoneFilter");
            PopulateHeaderDropdown("ddlCmfRequestHeader", "CmfRequestItems", "cmfRequestFilter");
        }

        // Handle empty data template
        if (overall_request_details.Rows.Count == 0)
        {
            PopulateEmptyTemplateDropdowns();
        }
    }

    private void PopulateEmptyTemplateDropdowns()
    {
        System.Web.UI.Control emptyDataTemplate = overall_request_details.Controls[0].Controls[0];

        PopulateEmptyDropdown(emptyDataTemplate, "ddlOwnerHeaderEmpty", "OwnerItems", "ownerFilter");
        PopulateEmptyDropdown(emptyDataTemplate, "ddlRvpReproHeaderEmpty", "RvpReproItems", "rvpReproFilter");
        PopulateEmptyDropdown(emptyDataTemplate, "ddlIdstHeaderEmpty", "IdstItems", "idstFilter");
        PopulateEmptyDropdown(emptyDataTemplate, "ddlLosHeaderEmpty", "LosItems", "losFilter");
        PopulateEmptyDropdown(emptyDataTemplate, "ddlCompanyHeaderEmpty", "CompanyItems", "companyFilter");      // Fixed
        PopulateEmptyDropdown(emptyDataTemplate, "ddlDetailHeaderEmpty", "DetailItems", "detailFilter");        // Fixed
        PopulateEmptyDropdown(emptyDataTemplate, "ddlComponentHeaderEmpty", "ComponentItems", "componentFilter"); // Fixed
        PopulateEmptyDropdown(emptyDataTemplate, "ddlMilestoneHeaderEmpty", "MilestoneItems", "milestoneFilter");
        PopulateEmptyDropdown(emptyDataTemplate, "ddlCmfRequestHeaderEmpty", "CmfRequestItems", "cmfRequestFilter");
    }

    private void PopulateEmptyDropdown(System.Web.UI.Control container, string dropdownId, string viewStateKey, string sessionKey)
    {
        DropDownList ddl = container.FindControl(dropdownId) as DropDownList;
        PopulateFilterDropdown(ddl, viewStateKey, sessionKey);
    }

    private void PopulateHeaderDropdown(string dropdownId, string viewStateKey, string sessionKey)
    {
        DropDownList ddl = overall_request_details.HeaderRow.FindControl(dropdownId) as DropDownList;
        PopulateFilterDropdown(ddl, viewStateKey, sessionKey);
    }

    private List<FilterItem> GetFilterItems(string viewStateKey)
    {
        object state = ViewState[viewStateKey];
        if (state == null)
            return null;

        if (!(state is string[][]) && !(state is List<FilterItem>))
        {
            System.Diagnostics.Trace.TraceWarning(
                "Reloading incompatible CMF filter ViewState '{0}' of type '{1}'.",
                viewStateKey, state.GetType().AssemblyQualifiedName);
            BindAllFilters();
            state = ViewState[viewStateKey];
        }

        List<FilterItem> legacyItems = state as List<FilterItem>;
        if (legacyItems != null)
            return legacyItems;

        string[][] storedItems = state as string[][];
        if (storedItems == null || storedItems.Any(item => item == null || item.Length != 2))
            throw new InvalidOperationException("Invalid CMF filter ViewState: " + viewStateKey);

        return storedItems.Select(item => new FilterItem(item[0], item[1])).ToList();
    }

    private void PopulateFilterDropdown(DropDownList ddl, string viewStateKey, string sessionKey)
    {
        if (ddl == null)
            return;

        List<FilterItem> items = GetFilterItems(viewStateKey);
        if (items != null)
        {
            ddl.Items.Clear();

            foreach (FilterItem item in items)
            {
                ddl.Items.Add(new ListItem(item.Text, item.Value));
            }

            if (Session[sessionKey] != null)
            {
                string value = Session[sessionKey].ToString();
                var item = ddl.Items.FindByValue(value);
                if (item != null)
                    ddl.SelectedValue = value;
            }
        }
    }

    // Updated BuildFilterClauses (remove progress and component)
    private static string BuildFilterClauses(Dictionary<string, string> filters)
    {
        List<string> clauses = new List<string>();

        foreach (var filter in filters)
        {
            if (!string.IsNullOrWhiteSpace(filter.Value) && !filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                switch (filter.Key)
                {
                    case "owner":
                        clauses.Add(" AND LTRIM(RTRIM(customer_owner)) = @OwnerFilter ");
                        break;
                    case "rvpRepro":
                        clauses.Add(" AND LTRIM(RTRIM(repro_on_rvp)) = @RvpReproFilter ");
                        break;
                    case "idst":
                        clauses.Add(" AND LTRIM(RTRIM(ISNULL(M.idst_display_name, main.idst))) = @IdstFilter ");
                        break;
                    case "Company":
                        clauses.Add(" AND LTRIM(RTRIM(customer_company)) = @CompanyFilter ");
                        break;
                    case "Detail":
                        clauses.Add(" AND LTRIM(RTRIM(customer_detail)) = @DetailFilter ");
                        break;
                    case "Component":
                        clauses.Add(" AND LTRIM(RTRIM(component_group)) = @ComponentFilter ");
                        break;
                    //case "los":
                    //    clauses.Add(" AND LTRIM(RTRIM(los)) = @LosFilter ");
                    //    break;

                    case "los":
                        // Special handling: 'No' in the UI means NULL or empty in DB
                        clauses.Add(
                            " AND ( " +
                            "     (@LosFilter = 'No' AND (los IS NULL OR LTRIM(RTRIM(los)) = '')) " +
                            "  OR (@LosFilter <> 'No' AND LTRIM(RTRIM(los)) = @LosFilter) " +
                            ") "
                        );
                        break;

                    case "milestone":
                        clauses.Add(" AND LTRIM(RTRIM(drivers)) = @MilestoneFilter ");
                        break;
                    case "cmfRequest":
                        clauses.Add(" AND LTRIM(RTRIM(cmf_request)) = @CmfRequestFilter ");
                        break;
                }
            }
        }

        return string.Join("", clauses);
    }

    private void InitializeFilterValue()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            // Get the current platform
            string currentPlatform = Session["selectedPlatform"] as string ?? ddlTables.SelectedValue;

            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + currentPlatform +
                                    " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";

            using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
            {
                SqlDataReader reader = cmd.ExecuteReader();
                List<string> driversList = new List<string>();

                while (reader.Read())
                {
                    driversList.Add(reader["drivers"].ToString());

                }
                reader.Close();

                if (driversList.Count > 0)
                {
                    Session["filterValue"] = string.Join(",", driversList);
                }
                else
                {
                    Session["filterValue"] = null;
                }
            }
        }
    }

    private static PrincipalContext TryCreateContext(string domain)
    {
        try { return new PrincipalContext(ContextType.Domain, domain); }
        catch { return null; }
    }


    private static string ResolveDisplayName(
    string alias,
    IEnumerable<PrincipalContext> contexts)
    {
        foreach (PrincipalContext ctx in contexts)
        {
            if (ctx == null)
                continue;

            UserPrincipal up = UserPrincipal.FindByIdentity(
                ctx, IdentityType.SamAccountName, alias);

            if (up != null)
                return up.DisplayName ?? alias;
        }

        // ✅ REQUIRED return to satisfy compiler
        return "Unable to fetch";
    }
    private void EnrichNewUsers()
    {
        // ✅ Old‑style instantiation
        List<string> users = new List<string>();

        using (SqlConnection con = new SqlConnection(
            ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
        {
            con.Open();

            using (SqlCommand cmd = new SqlCommand(@"
         SELECT Username
         FROM dbo.CMFUserVisitCounts_Final
         WHERE DisplayName IS NULL;", con))
            {
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        users.Add(rdr.GetString(0));
                    }
                }
            }
        }

        // ✅ Nothing to resolve
        if (users.Count == 0)
            return;

        // ✅ Explicit type (no target-typed new)
        PrincipalContext[] contexts = new PrincipalContext[]
        {
     TryCreateContext("gar.corp.intel.com"),
     TryCreateContext("amr.corp.intel.com"),
     TryCreateContext("ccr.corp.intel.com"),
     TryCreateContext("ger.corp.intel.com")
        };

        using (SqlConnection updateCon = new SqlConnection(
            ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
        {
            updateCon.Open();

            foreach (string alias in users)
            {
                string displayName = ResolveDisplayName(alias, contexts);

                using (SqlCommand updateCmd = new SqlCommand(@"
             UPDATE dbo.CMFUserVisitCounts_Final
             SET DisplayName = @name,
                 LastUpdated = SYSDATETIME()
             WHERE Username = @user;", updateCon))
                {
                    updateCmd.Parameters.AddWithValue("@user", alias);
                    updateCmd.Parameters.AddWithValue("@name", displayName);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        overall_request_details.DataBound += overall_request_details_DataBound;
        //GridView_cmf_summary.RowCreated += GridView_cmf_summary_RowCreated;

        lnkAllDrivers.Visible = true;

        if (!IsPostBack)
        {
            //EnrichNewUsers(); --to trigger enrichment of new users in the visit count table, can be commented out when not required

            try
            {
                string crttOutputDirectory = GetSafeCrttOutputDirectory();
                ExecuteCrttApiCalls(crttOutputDirectory);
            }
            catch (Exception ex)
            {
                Session["CrttApiError"] = ex.Message;
                System.Diagnostics.Debug.WriteLine("CRTT ERROR: " + ex.ToString());
            }

            DateTime currentDate = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            int workWeekNumber = cultureInfo.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            WorkWeek = string.Format("WW'{0:D2}", workWeekNumber);

            // Set initial platform correctly
            Session["selectedPlatform"] = ddlTables.SelectedValue;
            selectedPlatform = ddlTables.SelectedValue;
            Session["isFirstLoad"] = null;

            btnExportToExcel.Visible = false;
            btnExportToExcel_ig.Visible = false;
            btnExportToExcel_des.Visible = false;
            btnExportToExcel_cmf_pending.Visible = false;
            btnExportToExcel_des_summary.Visible = false;
            overall_request_details.Visible = false;
            GridView_design_open.Visible = false;
            GridView_cmf_summary.Visible = true;
            GridView_cmf_summary1.Visible = true;
            GridView_milestone_map.Visible = true;
            GridView_notes.Visible = true;
            GridView_comp.Visible = true;
            tptdefdiv.Visible = true;
            GridView_cmf_pending.Visible = false;
            btnImportPopup.Visible = false;

            headerTitle.InnerText = WorkWeek + driver;

            // Updated filter initialization (removed progress and component):
            if (Session["ownerFilter"] == null) Session["ownerFilter"] = "All";
            if (Session["rvpReproFilter"] == null) Session["rvpReproFilter"] = "All";
            if (Session["idstFilter"] == null) Session["idstFilter"] = "All";
            if (Session["losFilter"] == null) Session["losFilter"] = "All";
            if (Session["milestoneFilter"] == null) Session["milestoneFilter"] = "All";
            if (Session["cmfRequestFilter"] == null) Session["cmfRequestFilter"] = "All";
            if (Session["CompanyFilter"] == null) Session["CompanyFilter"] = "All";
            if (Session["DetailFilter"] == null) Session["DetailFilter"] = "All";
            if (Session["ComponentFilter"] == null) Session["ComponentFilter"] = "All";

            // Initialize filterValue for first load - this is important for BindAllFilters
            InitializeFilterValue();

            BindAllFilters();
            BindGridView();
            BindGridView_cmf_summary();
            BindGridView_cmf_pending();
            BindGridView_design_summary();
            BindGridView_component_summary();
            BindGridView_oem_summary();
            string currentPlatformForCrtt = Session["selectedPlatform"] as string ?? ddlTables.SelectedValue;
            GenerateBasePlatformDesignFile(AppDomain.CurrentDomain.BaseDirectory, currentPlatformForCrtt);
            BindDistinctValues();
            BindDistinctFilters();

            // Set the platform-specific dashboard link
            UpdatePlatformDashboardLink();

            // Total Visits Logic
            string session_count_table = "CMFUserVisitCounts_Final";
            string identity = HttpContext.Current.User.Identity.Name;
            int checkidentity = identity.LastIndexOf('\\');
            string var_username = identity.Substring(checkidentity + 1);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM " + session_count_table + " WHERE Username = @username";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@username", var_username);
                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    query = "UPDATE " + session_count_table + " SET SessionCount = SessionCount + 1 WHERE Username = @username";
                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@username", var_username);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    query = "INSERT INTO " + session_count_table + " (Username, SessionCount) VALUES (@username, 1)";
                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@username", var_username);
                    cmd.ExecuteNonQuery();
                }

                query = "SELECT ISNULL(SUM(SessionCount), 0) FROM " + session_count_table;
                cmd = new SqlCommand(query, con);
                count = (int)cmd.ExecuteScalar();
                session_count.Text = "Total Visits: " + count.ToString();
            }
        }
        else
        {
            Session["isFirstLoad"] = false;
            SetWorkWeek();

            // Check if platform has changed
            string newPlatform = ddlTables.SelectedValue;
            string currentPlatform = Session["selectedPlatform"] as string;

            if (newPlatform != currentPlatform)
            {
                // Platform changed - update and rebind filters
                Session["selectedPlatform"] = newPlatform;
                selectedPlatform = newPlatform;

                // Clear all filters when platform changes
                Session["ownerFilter"] = "All";
                Session["rvpReproFilter"] = "All";
                Session["idstFilter"] = "All";
                Session["losFilter"] = "All";
                Session["milestoneFilter"] = "All";
                Session["cmfRequestFilter"] = "All";
                Session["CompanyFilter"] = "All";
                Session["DetailFilter"] = "All";
                Session["ComponentFilter"] = "All";

                // IMPORTANT: Initialize filterValue for the new platform before binding filters
                InitializeFilterValue();

                // Rebind filters for new platform
                BindAllFilters();

                // Rebind ALL grids when platform changes
                BindGridView();
                BindGridView_cmf_summary();  // ADD THIS LINE
                BindGridView_cmf_pending();
                BindGridView_design_summary();
                BindGridView_component_summary();
                BindGridView_oem_summary();

                try
                {
                    string crttOutputDirectory = GetSafeCrttOutputDirectory();
                    ExecuteCrttApiCalls(crttOutputDirectory);
                    GenerateBasePlatformDesignFile(crttOutputDirectory, newPlatform);
                }
                catch (Exception ex)
                {
                    Session["CrttApiError"] = ex.Message;
                    System.Diagnostics.Debug.WriteLine("CRTT ERROR ON PLATFORM CHANGE: " + ex.ToString());
                }

                BindDistinctValues();
                BindDistinctFilters();

                // Update the platform-specific dashboard link
                UpdatePlatformDashboardLink();
            }
            else
            {
                selectedPlatform = Session["selectedPlatform"] as string ?? ddlTables.SelectedValue;

                // IMPORTANT: Rebind CMF summary on every postback to maintain the grid
                // This ensures the grid doesn't get cleared when clicking on issue counts
                BindGridView_cmf_summary();  // ADD THIS LINE
            }

            string eventTarget = Request["__EVENTTARGET"];
            string eventArgument = Request["__EVENTARGUMENT"];

            if (eventTarget == "ShowModal" && !string.IsNullOrEmpty(eventArgument))
            {
                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;
                BindModalGrid(eventArgument);
                ShowModal();
            }
            else if (eventTarget == "ShowModal2" && !string.IsNullOrEmpty(eventArgument))
            {
                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;
                BindModalGrid2(eventArgument);
                ShowModal2();
            }
            else if (eventTarget == "ShowDriverIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                HiddenDriverName.Value = eventArgument;
                Session["DriverName"] = eventArgument; // persist for export
                BindDriverIssues(eventArgument);
                ShowDriverIssues(); // opens the drivers modal
            }
            else if (eventTarget == "ExportDriverIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                string driver = Session["DriverName"] as string ?? eventArgument;
                if (!string.IsNullOrWhiteSpace(driver))
                {
                    HiddenDriverName.Value = driver;
                    ExportDriverIssues(driver);
                    return;
                }
            }
            else if (eventTarget == "ShowCMFIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                string[] parts = eventArgument.Split('|');
                if (parts.Length == 3)
                {
                    string component = parts[0];
                    string driver = parts[1];
                    string issueType = parts[2];

                    HiddenComponentName.Value = component;
                    HiddenDriverName.Value = driver;
                    HiddenIssueType.Value = issueType;

                    Session["ComponentName"] = component;
                    Session["DriverName"] = driver;
                    Session["IssueType"] = issueType;

                    BindCMFIssues(component, driver, issueType);
                    ShowCMFIssues();
                }
            }
            else if (eventTarget == "ExportCMFIssues" && !string.IsNullOrEmpty(eventArgument))
            {
                string[] parts = eventArgument.Split('|');
                if (parts.Length == 3)
                {
                    string component = parts[0];
                    string driver = parts[1];
                    string issueType = parts[2];

                    HiddenComponentName.Value = component;
                    HiddenDriverName.Value = driver;
                    HiddenIssueType.Value = issueType;

                    ExportCMFIssues(component, driver, issueType);
                    return;
                }
            }
            else if (eventTarget == "ShowModal3" && !string.IsNullOrEmpty(eventArgument))
            {
                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;
                BindModalGrid3(eventArgument);
                ShowModal3();
            }
            else if (eventTarget == "ShowDriverModal" && !string.IsNullOrEmpty(eventArgument))
            {
                string[] args = eventArgument.Split(',');
                if (args.Length == 2)
                {
                    string design = args[0];
                    string driver = args[1];
                    Session["DesignValue"] = design;
                    BindDriverDetailsModal(design, driver);
                    ShowModal0();
                }
            }
            else if (eventTarget == "ShowImplementedVerifiedModal" && !string.IsNullOrEmpty(eventArgument))
            {
                HiddenModalDesign.Value = eventArgument;
                Session["DesignValue"] = eventArgument;
                BindImplementedVerifiedDetailsModal(eventArgument);
                ShowModal0();
            }
            else if (eventTarget == "ExportToExcel" && !string.IsNullOrEmpty(eventArgument))
            {
                string designValue = Session["DesignValue"] as string;
                if (!string.IsNullOrEmpty(designValue))
                {
                    HiddenModalDesign.Value = designValue;
                    ExportToExcel(designValue);
                }
            }
            else if (eventTarget == "ExportToExcel2" && !string.IsNullOrEmpty(eventArgument))
            {
                string designValue = Session["DesignValue"] as string;
                if (!string.IsNullOrEmpty(designValue))
                {
                    HiddenModalDesign.Value = designValue;
                    ExportToExcel2(designValue);
                }
            }
        }
    }

    // Keep your existing BuildIdstClause and BindGridView methods unchanged
    private static string BuildIdstClause(string idstFilter)
    {
        if (string.IsNullOrWhiteSpace(idstFilter) || idstFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return " AND LTRIM(RTRIM(idst)) = @IdstFilter ";
    }

    //private void BindGridView(string filterValue = null, Dictionary<string, string> columnFilters = null)
    //{
    //    if (columnFilters == null)
    //        columnFilters = GetAllFilterValues();

    //    string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
    //    using (SqlConnection con = new SqlConnection(connectionString))
    //    {
    //        con.Open();

    //        // First Load Condition
    //        if (Session["isFirstLoad"] == null)
    //        {
    //            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";
    //            using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
    //            {
    //                SqlDataReader reader = cmd.ExecuteReader();
    //                List<string> driversList = new List<string>();

    //                while (reader.Read())
    //                {
    //                    driversList.Add(reader["drivers"].ToString());
    //                }
    //                reader.Close();

    //                if (driversList.Count > 0)
    //                {
    //                    Session["filterValue"] = string.Join(",", driversList);
    //                    filterValue = Session["filterValue"].ToString();
    //                }
    //            }
    //        }

    //        // Build base query
    //        string base_master_query = "SELECT progress, cp_id As SightingID, title , component , customer_owner As Owner, owners_name, repro_on_rvp, processor, cmf_status As Status, idst, los, drivers AS Driver, days_active, CASE WHEN cmf_request = 'cmf_duplicate' THEN merge_id ELSE promoted_id END AS Merged_PromotedID , cmf_request, customer_company, customer_detail, component_group,customer_affected, impact, closed_reason + CHAR(13) + CHAR(10) +  CASE WHEN status = '/*rejected*/' THEN 'NA' ELSE fixed_in_version END AS ClosedDetails FROM " + selectedPlatform;

    //        // Collect DISTINCT aliases for owner processing
    //        SqlDataAdapter masterAdapt = new SqlDataAdapter(base_master_query, con);
    //        DataTable dt_master = new DataTable();
    //        masterAdapt.Fill(dt_master);

    //        HashSet<string> aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    //        foreach (DataRow r in dt_master.Rows)
    //        {
    //            if (r["Owner"] == DBNull.Value) continue;
    //            string raw = r["Owner"].ToString().Trim();
    //            if (string.IsNullOrEmpty(raw)) continue;

    //            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //            if (raw.Contains("@")) raw = raw.Split('@')[0];
    //            raw = raw.Trim();

    //            if (!string.IsNullOrEmpty(raw))
    //                aliasSet.Add(raw);

    //            if (r["Owners_name"] == DBNull.Value) continue;
    //            raw = r["owners_name"].ToString().Trim();
    //            if (string.IsNullOrEmpty(raw)) continue;

    //            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //            if (raw.Contains("@")) raw = raw.Split('@')[0];
    //            raw = raw.Trim();

    //            if (!string.IsNullOrEmpty(raw))
    //                aliasSet.Add(raw);
    //        }

    //        // Create AD contexts
    //        PrincipalContext context_gar = null;
    //        PrincipalContext context_amr = null;
    //        PrincipalContext context_ccr = null;
    //        PrincipalContext context_ger = null;

    //        try { context_gar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
    //        try { context_amr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
    //        try { context_ccr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
    //        try { context_ger = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

    //        Dictionary<string, string> ownerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    //        foreach (string alias in aliasSet)
    //        {
    //            string displayName = "Unable to fetch";
    //            UserPrincipal up = null;

    //            if (context_gar != null) up = UserPrincipal.FindByIdentity(context_gar, IdentityType.SamAccountName, alias);
    //            if (up == null && context_amr != null) up = UserPrincipal.FindByIdentity(context_amr, IdentityType.SamAccountName, alias);
    //            if (up == null && context_ccr != null) up = UserPrincipal.FindByIdentity(context_ccr, IdentityType.SamAccountName, alias);
    //            if (up == null && context_ger != null) up = UserPrincipal.FindByIdentity(context_ger, IdentityType.SamAccountName, alias);

    //            if (up != null) displayName = up.DisplayName;

    //            ownerMap[alias] = displayName;
    //        }

    //        // Build dynamic master_query
    //        string master_query = base_master_query;
    //        string order_clause = "ORDER BY \r\n    CASE \r\n        WHEN progress = 'Green' THEN 1  -- Give 'green' a lower priority, so it appears later\r\n        ELSE 0  -- Non-green rows get a higher priority (appear first)\r\n    END,\r\n    Driver, \r\n    CASE \r\n        WHEN progress = 'Red' THEN 1\r\n        WHEN progress = 'Orange' THEN 2\r\n        WHEN progress = 'Yellow' THEN 3\r\n        WHEN progress = 'Green' THEN 4  -- Green comes last, after all other colors\r\n    END,\r\n    component;\r\n ";

    //        // Build WHERE clause - FIXED: Use consistent status filtering
    //        string whereClause = "";

    //        if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
    //        {
    //            if (filterValue.Contains(","))
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n" +
    //                "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR ( " +
    //                "       @FilterValue LIKE '%,' + drivers + ',%' " +
    //                "       OR @FilterValue LIKE drivers + ',%' " +
    //                "       OR @FilterValue LIKE '%,' + drivers) ) " +
    //                "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
    //            }
    //            else
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR drivers = @FilterValue ) AND status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
    //            }
    //        }
    //        else
    //        {
    //            // Handle AllDrivers case
    //            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";
    //            using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
    //            {
    //                SqlDataReader reader = cmd.ExecuteReader();
    //                List<string> driversList = new List<string>();

    //                while (reader.Read())
    //                {
    //                    driversList.Add(reader["drivers"].ToString());
    //                }
    //                reader.Close();

    //                if (driversList.Count > 0)
    //                {
    //                    Session["filterValue"] = string.Join(",", driversList);
    //                    filterValue = Session["filterValue"].ToString();
    //                }
    //            }

    //            if (!string.IsNullOrEmpty(filterValue) && filterValue.Contains(","))
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n" +
    //                "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR ( " +
    //                "       @FilterValue LIKE '%,' + drivers + ',%' " +
    //                "       OR @FilterValue LIKE drivers + ',%' " +
    //                "       OR @FilterValue LIKE '%,' + drivers) ) " +
    //                "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
    //            }
    //            else if (string.IsNullOrEmpty(filterValue))
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE status IN ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
    //            }
    //            else
    //            {
    //                // FIXED: Use consistent status filtering - always use status IN ('open', 'implemented')
    //                whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
    //                "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
    //                "    OR drivers = @FilterValue ) AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
    //            }
    //        }

    //        // Add column filters
    //        whereClause += BuildFilterClauses(columnFilters);

    //        master_query += whereClause + order_clause;

    //        // Execute final query
    //        using (SqlCommand cmd = new SqlCommand(master_query, con))
    //        {
    //            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
    //            {
    //                if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
    //                    sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

    //                // Add column filter parameters
    //                foreach (var filter in columnFilters)
    //                {
    //                    if (!string.IsNullOrWhiteSpace(filter.Value) && !filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        switch (filter.Key)
    //                        {
    //                            case "owner":
    //                                sda.SelectCommand.Parameters.AddWithValue("@OwnerFilter", filter.Value);
    //                                break;
    //                            case "rvpRepro":
    //                                sda.SelectCommand.Parameters.AddWithValue("@RvpReproFilter", filter.Value);
    //                                break;
    //                            case "idst":
    //                                sda.SelectCommand.Parameters.AddWithValue("@IdstFilter", filter.Value);
    //                                break;
    //                            case "los":
    //                                sda.SelectCommand.Parameters.AddWithValue("@LosFilter", filter.Value);
    //                                break;
    //                            case "milestone":
    //                                sda.SelectCommand.Parameters.AddWithValue("@MilestoneFilter", filter.Value);
    //                                break;
    //                            case "Company":
    //                                sda.SelectCommand.Parameters.AddWithValue("@CompanyFilter", filter.Value);
    //                                break;
    //                            case "Detail":
    //                                sda.SelectCommand.Parameters.AddWithValue("@DetailFilter", filter.Value);
    //                                break;
    //                            case "Component":
    //                                sda.SelectCommand.Parameters.AddWithValue("@ComponentFilter", filter.Value);
    //                                break;
    //                            case "cmfRequest":
    //                                sda.SelectCommand.Parameters.AddWithValue("@CmfRequestFilter", filter.Value);
    //                                break;
    //                        }
    //                    }
    //                }

    //                DataTable dt = new DataTable();
    //                sda.Fill(dt);

    //                // Process data rows
    //                foreach (DataRow row in dt.Rows)
    //                {
    //                    // Replace Owner with actual names
    //                    string raw = (row["Owner"] == DBNull.Value) ? string.Empty : row["Owner"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(raw))
    //                    {
    //                        row["Owner"] = "Unable to fetch";
    //                    }
    //                    else
    //                    {
    //                        if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //                        if (raw.Contains("@")) raw = raw.Split('@')[0];
    //                        raw = raw.Trim();

    //                        string actualName;
    //                        if (ownerMap.TryGetValue(raw, out actualName))
    //                            row["Owner"] = actualName;
    //                        else
    //                            row["Owner"] = "Unable to fetch";
    //                    }

    //                    raw = (row["owners_name"] == DBNull.Value) ? string.Empty : row["owners_name"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(raw))
    //                    {
    //                        row["owners_name"] = "Unable to fetch";
    //                    }
    //                    else
    //                    {
    //                        if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
    //                        if (raw.Contains("@")) raw = raw.Split('@')[0];
    //                        raw = raw.Trim();

    //                        string actualName;
    //                        if (ownerMap.TryGetValue(raw, out actualName))
    //                            row["Owners_name"] = actualName;
    //                        else
    //                            row["owners_name"] = "Unable to fetch";
    //                    }

    //                    // Handle LOS - Set to "No" if blank/null
    //                    string losValue = (row["los"] == DBNull.Value) ? string.Empty : row["los"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(losValue))
    //                    {
    //                        row["los"] = "No";
    //                    }

    //                    // Handle ClosedDetails - Set to "NA" if blank/null
    //                    string closedDetailsValue = (row["ClosedDetails"] == DBNull.Value) ? string.Empty : row["ClosedDetails"].ToString().Trim();
    //                    if (string.IsNullOrEmpty(closedDetailsValue))
    //                    {
    //                        row["ClosedDetails"] = "NA";
    //                    }
    //                }

    //                // Bind
    //                overall_request_details.DataSource = dt;
    //                overall_request_details.DataKeyNames = new string[] { "SightingID" };
    //                overall_request_details.DataBind();

    //                BindGridView_design_open();
    //                BindGridView_cmf_summary();
    //                BindGridView_cmf_pending();
    //                BindGridView_design_summary();
    //                BindGridView_component_summary();
    //                BindGridView_oem_summary();
    //            }
    //        }
    //    }
    //}

    protected void AutoRefreshTimer_Tick(object sender, EventArgs e)
    {
        SetWorkWeek();
        selectedPlatform = Session["selectedPlatform"] as string ?? ddlTables.SelectedValue;

        BindGridView();
        BindGridView_cmf_summary();
        BindGridView_cmf_pending();
        BindGridView_design_summary();
        BindGridView_component_summary();
        BindGridView_oem_summary();
        BindDistinctValues();
        BindDistinctFilters();
    }

    private void BindGridView(string filterValue = null, Dictionary<string, string> columnFilters = null)
    {
        if (columnFilters == null)
            columnFilters = GetAllFilterValues();

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            // First Load Condition
            if (Session["isFirstLoad"] == null)
            {
                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";
                using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<string> driversList = new List<string>();

                    while (reader.Read())
                    {
                        driversList.Add(reader["drivers"].ToString());
                    }
                    reader.Close();

                    if (driversList.Count > 0)
                    {
                        Session["filterValue"] = string.Join(",", driversList);
                        filterValue = Session["filterValue"].ToString();
                    }
                }
            }

            string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
            string designTable = basePlatform + "_DESIGN_TABLE";

            //            string base_master_query = @"
            //SELECT 
            //    progress, 
            //    cp_id As SightingID, 
            //    title, 
            //    component, 
            //    customer_owner As Owner, 
            //    owners_name, 
            //    repro_on_rvp, 
            //    processor, 
            //    cmf_status As Status, 
            //    idst, 
            //    los, 
            //    drivers AS Driver, 
            //    days_active, 
            //    CASE WHEN cmf_request = 'cmf_duplicate' THEN merge_id ELSE promoted_id END AS Merged_PromotedID,
            //    cmf_request, 
            //    customer_company, 
            //    customer_detail, 
            //    component_group,
            //    customer_affected, 
            //    impact, 
            //    closed_reason + CHAR(13) + CHAR(10) + CASE WHEN status = 'rejected' THEN 'NA' ELSE fixed_in_version END AS ClosedDetails,
            //    -- NEW: Duplicate Details column with HSD ID and Customer Company
            //    STUFF((
            //        SELECT ', '
            //       + CAST(dup.cp_id AS VARCHAR)
            //       + '|' + ISNULL(dup.customer_detail, 'Unknown')
            //       + '|' + ISNULL(CONVERT(VARCHAR(10), d2.sw_image_freeze, 120), 'Unknown')
            //FROM " + selectedPlatform + @" dup
            //LEFT JOIN " + designTable + @" AS d2
            //       ON d2.customer_detail = dup.customer_detail
            //WHERE dup.cmf_request = 'cmf_duplicate'
            //  AND dup.status = 'open'
            //  AND dup.merge_id = main.cp_id
            //FOR XML PATH('')
            //    ), 1, 2, '') AS DuplicateDetails
            //FROM " + selectedPlatform + @" main";

            //string base_master_query = @"
            //SELECT 
            //    main.progress, 
            //    main.cp_id As SightingID, 
            //    main.title, 
            //    main.component, 
            //    main.customer_owner As Owner, 
            //    main.owners_name, 
            //    main.repro_on_rvp, 
            //    main.processor, 
            //    main.cmf_status As Status, 
            //    main.idst, 
            //    main.los, 
            //    main.drivers AS Driver, 
            //    main.days_active, 
            //    CASE WHEN main.cmf_request = 'cmf_duplicate' THEN main.merge_id ELSE main.promoted_id END AS Merged_PromotedID,
            //    main.cmf_request, 
            //    main.customer_company, 
            //    main.customer_detail, 
            //    main.component_group,
            //    main.customer_affected, 
            //    main.impact, 
            //    main.closed_reason + CHAR(13) + CHAR(10) + CASE WHEN main.status = 'rejected' THEN 'NA' ELSE main.fixed_in_version END AS ClosedDetails,

            //    -- NEW: bring sw_image_freeze from design table
            //    d.sw_image_freeze AS ImageFreeze,

            //    -- Duplicate Details column with HSD ID and Customer Company
            //    STUFF((
            //                        SELECT ', '
            //           + CAST(dup.cp_id AS VARCHAR)
            //           + '|' + ISNULL(dup.customer_detail, 'Unknown')
            //           + '|' + ISNULL(CONVERT(VARCHAR(10), d2.ImageFreeze, 120), 'Unknown')
            //    FROM " + selectedPlatform + @" dup
            //    LEFT JOIN " + designTable + @" AS d2
            //           ON d2.customer_detail = dup.customer_detail
            //    WHERE dup.cmf_request = 'cmf_duplicate'
            //      AND dup.status = 'open'
            //      AND dup.merge_id = main.cp_id
            //    FOR XML PATH('')
            //    ), 1, 2, '') AS DuplicateDetails
            //FROM " + selectedPlatform + @" AS main
            //LEFT JOIN " + designTable + @" AS d
            //    ON d.customer_detail = main.customer_detail";

            string mappingTable = selectedPlatform.IndexOf("NVL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_NVL"
                : selectedPlatform.IndexOf("PTL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_PTL"
                : selectedPlatform.IndexOf("WCL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_WCL"
                : "idst_mapping_table";

            string base_master_query = @"
SELECT 
    main.progress, 
    main.cp_id As SightingID, 
    main.title, 
    main.component, 
    main.customer_owner As Owner, 
    main.owners_name, 
    main.repro_on_rvp, 
    main.processor, 
    main.cmf_status As Status, 
    main.idst,
    ISNULL(M.idst_display_name, main.idst) AS idst_display_name, 
    main.los, 
    main.drivers AS Driver, 
    main.days_active, 
    CASE WHEN main.cmf_request = 'cmf_duplicate' THEN main.merge_id ELSE main.promoted_id END AS Merged_PromotedID,
    main.cmf_request, 
    main.customer_company, 
    main.customer_detail, 
    main.component_group,
    main.customer_affected, 
    main.impact, 
    main.closed_reason + CHAR(13) + CHAR(10) + CASE WHEN main.status = 'rejected' THEN 'NA' ELSE main.fixed_in_version END AS ClosedDetails,

    -- Expose a single field name to the UI
    d.sw_image_freeze AS ImageFreeze,

    -- Duplicate Details: HSD|CustomerDetail|ImageFreezeDate
    STUFF((
        SELECT ', '
             + CAST(dup.cp_id AS VARCHAR)
             + '|' + ISNULL(dup.customer_detail, 'Unknown')
             + '|' + ISNULL(CONVERT(VARCHAR(10), d2.sw_image_freeze, 120), 'Unknown')  -- <-- FIXED HERE
        FROM " + selectedPlatform + @" dup
        LEFT JOIN " + designTable + @" AS d2
               ON d2.customer_detail = dup.customer_detail
        WHERE dup.cmf_request = 'cmf_duplicate'
          --AND dup.status = 'open'
          AND dup.merge_id = main.cp_id
        FOR XML PATH('')
    ), 1, 2, '') AS DuplicateDetails
FROM " + selectedPlatform + @" AS main
LEFT JOIN " + designTable + @" AS d
    ON d.customer_detail = main.customer_detail
LEFT JOIN [gfxitt_db].[dbo].[" + mappingTable + @"] M
    ON main.component_group = M.component";

            // Collect DISTINCT aliases for owner processing
            SqlDataAdapter masterAdapt = new SqlDataAdapter(base_master_query, con);
            DataTable dt_master = new DataTable();
            masterAdapt.Fill(dt_master);

            HashSet<string> aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in dt_master.Rows)
            {
                if (r["Owner"] == DBNull.Value) continue;
                string raw = r["Owner"].ToString().Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                if (raw.Contains("@")) raw = raw.Split('@')[0];
                raw = raw.Trim();

                if (!string.IsNullOrEmpty(raw))
                    aliasSet.Add(raw);

                if (r["Owners_name"] == DBNull.Value) continue;
                raw = r["owners_name"].ToString().Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                if (raw.Contains("@")) raw = raw.Split('@')[0];
                raw = raw.Trim();

                if (!string.IsNullOrEmpty(raw))
                    aliasSet.Add(raw);
            }

            // Create AD contexts
            PrincipalContext context_gar = null;
            PrincipalContext context_amr = null;
            PrincipalContext context_ccr = null;
            PrincipalContext context_ger = null;

            try { context_gar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
            try { context_amr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
            try { context_ccr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
            try { context_ger = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

            Dictionary<string, string> ownerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string alias in aliasSet)
            {
                string displayName = "Unable to fetch";
                UserPrincipal up = null;

                if (context_gar != null) up = UserPrincipal.FindByIdentity(context_gar, IdentityType.SamAccountName, alias);
                if (up == null && context_amr != null) up = UserPrincipal.FindByIdentity(context_amr, IdentityType.SamAccountName, alias);
                if (up == null && context_ccr != null) up = UserPrincipal.FindByIdentity(context_ccr, IdentityType.SamAccountName, alias);
                if (up == null && context_ger != null) up = UserPrincipal.FindByIdentity(context_ger, IdentityType.SamAccountName, alias);

                if (up != null) displayName = up.DisplayName;

                ownerMap[alias] = displayName;
            }

            // Build dynamic master_query
            string master_query = base_master_query;
            string order_clause = "ORDER BY \r\n    CASE \r\n        WHEN progress = 'Green' THEN 1  -- Give 'green' a lower priority, so it appears later\r\n        ELSE 0  -- Non-green rows get a higher priority (appear first)\r\n    END,\r\n    Driver, \r\n    CASE \r\n        WHEN progress = 'Red' THEN 1\r\n        WHEN progress = 'Orange' THEN 2\r\n        WHEN progress = 'Yellow' THEN 3\r\n        WHEN progress = 'Green' THEN 4  -- Green comes last, after all other colors\r\n    END,\r\n    component;\r\n ";

            // Build WHERE clause - MODIFIED TO MATCH OLD CODE LOGIC
            string whereClause = "";

            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                if (filterValue.Contains(","))
                {
                    // Use old code logic: first load vs subsequent loads
                    if (Session["isFirstLoad"] == null)
                    {
                        whereClause = " WHERE \r\n" +
                        "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                        "    OR ( " +
                        "       @FilterValue LIKE '%,' + drivers + ',%' " +
                        "       OR @FilterValue LIKE drivers + ',%' " +
                        "       OR @FilterValue LIKE '%,' + drivers) ) " +
                        "AND sysdebug Like ('%customer_must_fix%') AND status IN ('open', 'implemented') AND cmf_request in ('cmf_ok') ";
                    }
                    else
                    {
                        whereClause = " WHERE \r\n" +
                        "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                        "    OR ( " +
                        "       @FilterValue LIKE '%,' + drivers + ',%' " +
                        "       OR @FilterValue LIKE drivers + ',%' " +
                        "       OR @FilterValue LIKE '%,' + drivers) ) " +
                        "AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
                    }
                }
                else
                {
                    // Use old code logic: first load vs subsequent loads
                    if (Session["isFirstLoad"] == null)
                    {
                        whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                        " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                        "    OR drivers = @FilterValue ) AND status in ('open', 'implemented') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
                    }
                    else
                    {
                        whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                        " AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                        "    OR drivers = @FilterValue ) AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
                    }
                }
            }
            else
            {
                // Handle AllDrivers case
                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";
                using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<string> driversList = new List<string>();

                    while (reader.Read())
                    {
                        driversList.Add(reader["drivers"].ToString());
                    }
                    reader.Close();

                    if (driversList.Count > 0)
                    {
                        Session["filterValue"] = string.Join(",", driversList);
                        filterValue = Session["filterValue"].ToString();
                    }
                }

                if (!string.IsNullOrEmpty(filterValue) && filterValue.Contains(","))
                {
                    whereClause = " WHERE \r\n" +
                    "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR ( " +
                    "       @FilterValue LIKE '%,' + drivers + ',%' " +
                    "       OR @FilterValue LIKE drivers + ',%' " +
                    "       OR @FilterValue LIKE '%,' + drivers) ) " +
                    "AND sysdebug Like ('%customer_must_fix%') AND status NOT IN ('rejected') AND cmf_request in ('cmf_ok') ";
                }
                else if (string.IsNullOrEmpty(filterValue))
                {
                    whereClause = " WHERE status NOT IN ('rejected') AND sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok') ";
                }
                else
                {
                    whereClause = " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR drivers = @FilterValue ) AND sysdebug Like ('%customer_must_fix%') AND status not in ( 'rejected') AND cmf_request in ('cmf_ok') ";
                }
            }

            // Add column filters
            whereClause += BuildFilterClauses(columnFilters);

            master_query += whereClause + order_clause;

            // Execute final query
            using (SqlCommand cmd = new SqlCommand(master_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
                        sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

                    // Add column filter parameters
                    foreach (var filter in columnFilters)
                    {
                        if (!string.IsNullOrWhiteSpace(filter.Value) && !filter.Value.Equals("All", StringComparison.OrdinalIgnoreCase))
                        {
                            switch (filter.Key)
                            {
                                case "owner":
                                    sda.SelectCommand.Parameters.AddWithValue("@OwnerFilter", filter.Value);
                                    break;
                                case "rvpRepro":
                                    sda.SelectCommand.Parameters.AddWithValue("@RvpReproFilter", filter.Value);
                                    break;
                                case "idst":
                                    sda.SelectCommand.Parameters.AddWithValue("@IdstFilter", filter.Value);
                                    break;
                                case "los":
                                    sda.SelectCommand.Parameters.AddWithValue("@LosFilter", filter.Value);
                                    break;
                                case "milestone":
                                    sda.SelectCommand.Parameters.AddWithValue("@MilestoneFilter", filter.Value);
                                    break;
                                case "Company":
                                    sda.SelectCommand.Parameters.AddWithValue("@CompanyFilter", filter.Value);
                                    break;
                                case "Detail":
                                    sda.SelectCommand.Parameters.AddWithValue("@DetailFilter", filter.Value);
                                    break;
                                case "Component":
                                    sda.SelectCommand.Parameters.AddWithValue("@ComponentFilter", filter.Value);
                                    break;
                                case "cmfRequest":
                                    sda.SelectCommand.Parameters.AddWithValue("@CmfRequestFilter", filter.Value);
                                    break;
                            }
                        }
                    }

                    DataTable dt = new DataTable();
                    sda.Fill(dt);

                    // Add DuplicateDetails column if it doesn't exist
                    if (!dt.Columns.Contains("DuplicateDetails"))
                    {
                        dt.Columns.Add("DuplicateDetails", typeof(string));
                    }

                    // Process data rows
                    foreach (DataRow row in dt.Rows)
                    {
                        // Replace Owner with actual names
                        string raw = (row["Owner"] == DBNull.Value) ? string.Empty : row["Owner"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw))
                        {
                            row["Owner"] = "Unable to fetch";
                        }
                        else
                        {
                            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                            if (raw.Contains("@")) raw = raw.Split('@')[0];
                            raw = raw.Trim();

                            string actualName;
                            if (ownerMap.TryGetValue(raw, out actualName))
                                row["Owner"] = actualName;
                            else
                                row["Owner"] = "Unable to fetch";
                        }

                        raw = (row["owners_name"] == DBNull.Value) ? string.Empty : row["owners_name"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw))
                        {
                            row["owners_name"] = "Unable to fetch";
                        }
                        else
                        {
                            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                            if (raw.Contains("@")) raw = raw.Split('@')[0];
                            raw = raw.Trim();

                            string actualName;
                            if (ownerMap.TryGetValue(raw, out actualName))
                                row["Owners_name"] = actualName;
                            else
                                row["owners_name"] = "Unable to fetch";
                        }

                        // Handle LOS - Set to "No" if blank/null
                        string losValue = (row["los"] == DBNull.Value) ? string.Empty : row["los"].ToString().Trim();
                        if (string.IsNullOrEmpty(losValue))
                        {
                            row["los"] = "No";
                        }

                        // Handle ClosedDetails - Set to "NA" if blank/null
                        string closedDetailsValue = (row["ClosedDetails"] == DBNull.Value) ? string.Empty : row["ClosedDetails"].ToString().Trim();
                        if (string.IsNullOrEmpty(closedDetailsValue))
                        {
                            row["ClosedDetails"] = "NA";
                        }
                    }

                    // Bind
                    overall_request_details.DataSource = dt;
                    overall_request_details.DataKeyNames = new string[] { "SightingID" };
                    overall_request_details.DataBind();

                    BindGridView_design_open();
                    BindGridView_cmf_summary();
                    BindGridView_cmf_pending();
                    BindGridView_design_summary();
                    BindGridView_component_summary();
                    BindGridView_oem_summary();
                }
            }
        }
    }

    protected string CreateDuplicateLinks(object duplicateDetails)
    {
        if (duplicateDetails == null || duplicateDetails == DBNull.Value)
            return "NA";

        string details = duplicateDetails.ToString().Trim();
        if (string.IsNullOrEmpty(details))
            return "NA";

        // Split by comma and create hyperlinks for each duplicate with customer detail + image freeze date
        string[] duplicateEntries = details.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        List<string> links = new List<string>();

        foreach (string entry in duplicateEntries)
        {
            string trimmedEntry = entry.Trim();
            if (!string.IsNullOrEmpty(trimmedEntry))
            {
                string[] parts = trimmedEntry.Split('|');

                string hsdId = parts.Length > 0 ? parts[0].Trim() : "";
                string customerDetail = parts.Length > 1 ? (string.IsNullOrWhiteSpace(parts[1]) ? "Unknown" : parts[1].Trim()) : "Unknown";
                string freezeDate = parts.Length > 2 ? (string.IsNullOrWhiteSpace(parts[2]) ? "Unknown" : parts[2].Trim()) : "Unknown";

                if (!string.IsNullOrEmpty(hsdId))
                {
                    string link = string.Format(
                        "<a href='https://hsdes.intel.com/appstore/article/#{0}' target='_blank' title='Customer: {1} | ImageFreeze: {2}'>{0} | {1} | {2}</a>",
                        hsdId,
                        System.Web.HttpUtility.HtmlEncode(customerDetail),
                        System.Web.HttpUtility.HtmlEncode(freezeDate)
                    );
                    links.Add(link);
                }
            }
        }

        return links.Count > 0 ? string.Join(", ", links) : "NA";
    }

    //protected string CreateDuplicateLinks(object duplicateDetails)
    //{
    //    if (duplicateDetails == null || duplicateDetails == DBNull.Value)
    //        return "NA";

    //    string details = duplicateDetails.ToString().Trim();
    //    if (string.IsNullOrEmpty(details))
    //        return "NA";

    //    // Split by comma and create hyperlinks for each duplicate ID with customer company
    //    string[] duplicateEntries = details.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    //    List<string> links = new List<string>();

    //    foreach (string entry in duplicateEntries)
    //    {
    //        string trimmedEntry = entry.Trim();
    //        if (!string.IsNullOrEmpty(trimmedEntry))
    //        {
    //            // Split by pipe to get HSD ID and Customer Company
    //            string[] parts = trimmedEntry.Split('|');
    //            string hsdId = parts[0].Trim();
    //            string customerCompany = parts.Length > 1 ? parts[1].Trim() : "Unknown";

    //            // Create hyperlink with HSD ID and show customer company - removed curly braces
    //            string link = string.Format(
    //                "<a href='https://hsdes.intel.com/appstore/article/#{0}' target='_blank' title='Customer: {2}'>{0} | {2}</a>",
    //                hsdId,
    //                hsdId,
    //                customerCompany
    //            );
    //            links.Add(link);
    //        }
    //    }

    //    // Return "NA" if no valid links were created
    //    return links.Count > 0 ? string.Join(", ", links) : "NA";
    //}


    public List<string> SelectedColumns
    {
        get
        {
            if (ViewState["SelectedColumns"] == null)
            {
                // Default to all columns

                ViewState["SelectedColumns"] = new List<string>
{
    "sno",
    "milestone",
    "progress",
    "sightingid",
    "promotedid",
    "customer_detail",
    "duplicatedetails",          // NEW
    "customer_company",          // Keeping this (see note below)
    "title",
    "component",
    "component_group",           // comp_grp
    "owner",
    "promoted_owner",
    "rvp_repro",
    "status",
    "idst",
    "los",
    "processor",                 // keep before customer_affected
    "customer_affected",
    "impact",
    "days_open",
    //"cmf_request",
    "closed_reason",
    "edit_column"
};

            }
            return (List<string>)ViewState["SelectedColumns"];
        }
        set
        {
            ViewState["SelectedColumns"] = value;
        }
    }

    // Add a web method to receive selected columns from JavaScript
    [WebMethod]
    public static void SaveSelectedColumns(List<string> selectedColumns)
    {
        HttpContext.Current.Session["SelectedColumns"] = selectedColumns;
    }

    protected void btnExportToExcel_Click(object sender, EventArgs e)
    {
        // Get selected columns from session or use all columns as default
        List<string> selectedColumns = Session["SelectedColumns"] as List<string>;

        if (selectedColumns == null || selectedColumns.Count == 0)
        {
            selectedColumns = new List<string>
    {
        "sno",
        "milestone",
        "progress",
        "sightingid",
        "promotedid",
        "customer_detail",
        "imagefreeze",
        "duplicatedetails",
        "customer_company",
        "title",
        "component",
        "component_group",
        "owner",
        "promoted_owner",
        "rvp_repro",
        "status",
        "idst",
        "los",
        "processor",
        "impact",
        "days_open",
        //"cmf_request",
        "closed_reason",
        "edit_column"
    };
        }



        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=GridViewExport.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        overall_request_details.AllowPaging = false;

        // Store original visibility states
        Dictionary<int, bool> originalVisibility = new Dictionary<int, bool>();

        // Map field names to column indices (you'll need to adjust this based on your actual column order)

        //var columnMapping = new Dictionary<string, int>
        //{
        //    {"sno", 0},
        //    {"milestone", 1},
        //    {"progress", 2},
        //    {"sightingid", 3},
        //    {"promotedid", 4},
        //    {"customer_detail", 5},
        //    {"duplicatedetails", 6},   // NEW index
        //    {"customer_company", 7},
        //    {"title", 8},
        //    {"component", 9},
        //    {"component_group", 10},
        //    {"owner", 11},
        //    {"promoted_owner", 12},
        //    {"rvp_repro", 13},
        //    {"status", 14},
        //    {"idst", 15},
        //    {"los", 16},
        //    {"processor", 17},
        //    {"customer_affected", 18},
        //    {"impact", 19},
        //    {"days_open", 20},
        //    //{"cmf_request", 21},
        //    {"closed_reason", 21},
        //    {"edit_column", 22}
        //};
        var columnMapping = new Dictionary<string, int>
{
    {"sno", 0},
    {"milestone", 1},
    {"progress", 2},
    {"sightingid", 3},
    {"promotedid", 4},
    {"customer_detail", 5},
    {"imagefreeze", 6},           // NEW
    {"duplicatedetails", 7},
    {"customer_company", 8},
    {"title", 9},
    {"component", 10},
    {"component_group", 11},
    {"owner", 12},
    {"promoted_owner", 13},
    {"rvp_repro", 14},
    {"status", 15},
    {"idst", 16},
    {"los", 17},
    {"processor", 18},
    {"impact", 19},
    {"days_open", 20},
    {"closed_reason", 21},
    {"edit_column", 22}
};


        // Hide columns that are not selected
        for (int i = 0; i < overall_request_details.Columns.Count; i++)
        {
            originalVisibility[i] = overall_request_details.Columns[i].Visible;

            // Find the field name for this column index
            string fieldName = columnMapping.FirstOrDefault(x => x.Value == i).Key;

            if (!string.IsNullOrEmpty(fieldName))
            {
                overall_request_details.Columns[i].Visible = selectedColumns.Contains(fieldName);
            }
        }

        // Always hide the edit column for export
        if (overall_request_details.Columns.Count > 0)
        {
            overall_request_details.Columns[overall_request_details.Columns.Count - 1].Visible = false;
        }

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        overall_request_details.RenderControl(hw);

        // Write the StringWriter content to the Response output stream
        Response.Output.Write(sw.ToString());

        // Restore original visibility states
        foreach (var kvp in originalVisibility)
        {
            if (kvp.Key < overall_request_details.Columns.Count)
            {
                overall_request_details.Columns[kvp.Key].Visible = kvp.Value;
            }
        }

        // Re-enable paging
        overall_request_details.AllowPaging = true;

        // End the response to download the Excel file
        Response.End();
    }

    //protected void btnExportToExcel_Click(object sender, EventArgs e)
    //{
    //    // Set the response properties for the Excel download
    //    Response.ContentType = "application/vnd.ms-excel";
    //    Response.AddHeader("Content-Disposition", "attachment;filename=GridViewExport.xls");
    //    Response.Charset = "";
    //    Response.Buffer = true;
    //    Response.Clear();

    //    // Temporarily disable paging for the export
    //    overall_request_details.AllowPaging = false;

    //    // Hide the last column for export
    //    if (overall_request_details.Columns.Count > 0)
    //    {
    //        overall_request_details.Columns[overall_request_details.Columns.Count - 1].Visible = false;
    //    }

    //    // Create a StringWriter to capture the HTML output
    //    StringWriter sw = new StringWriter();
    //    HtmlTextWriter hw = new HtmlTextWriter(sw);

    //    // Render the GridView to the StringWriter
    //    overall_request_details.RenderControl(hw);

    //    // Write the StringWriter content to the Response output stream
    //    Response.Output.Write(sw.ToString());

    //    // End the response to download the Excel file
    //    Response.End();

    //    // Re-enable the visibility of the last column after the export
    //    overall_request_details.Columns[overall_request_details.Columns.Count - 1].Visible = true;
    //}

    protected void btnExportToExcel_Click_ingred(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=Ingredient.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_component_summary.AllowPaging = false;

        // Hide the last column for export
        //if (GridView_component_summary.Columns.Count > 0)
        //{
        //    GridView_component_summary.Columns[GridView_component_summary.Columns.Count - 1].Visible = false;
        //}

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_component_summary.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_component_summary.Columns[GridView_component_summary.Columns.Count - 1].Visible = true;
    }

    private string RemoveHyperlinksFromHtml(string html)
    {
        // Remove <a> tags but keep the inner text
        string pattern = @"<a[^>]*>(.*?)</a>";
        string result = System.Text.RegularExpressions.Regex.Replace(html, pattern, "$1",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return result;
    }
    protected void btnExportToExcel_Click_design(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=Design.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_design_summary.AllowPaging = false;

        // Hide the last column for export
        //if (GridView_design_summary.Columns.Count > 0)
        //{
        //    GridView_design_summary.Columns[GridView_design_summary.Columns.Count - 1].Visible = false;
        //}

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_design_summary.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_design_summary.Columns[GridView_design_summary.Columns.Count - 1].Visible = true;
    }

    // Export to Excel for Design Summary
    protected void btnExportToExcel_Click_designsummary(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=DesignSummary.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_design_open.AllowPaging = false;

        // Hide the last column for export
        if (GridView_design_open.Columns.Count > 0)
        {
            GridView_design_open.Columns[GridView_design_open.Columns.Count - 1].Visible = false;
        }

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_design_open.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_design_open.Columns[GridView_design_open.Columns.Count - 1].Visible = true;
    }

    // Export to Excel for CMF Pending List
    protected void btnExportToExcel_Click_cmf_pending(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=CMFPendingList.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_cmf_pending.AllowPaging = false;

        // Hide the last column for export
        //if (GridView_cmf_pending.Columns.Count > 0)
        //{
        //    GridView_cmf_pending.Columns[GridView_cmf_pending.Columns.Count - 1].Visible = false;
        //}

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_cmf_pending.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();

        // Re-enable the visibility of the last column after the export
        GridView_cmf_pending.Columns[GridView_cmf_pending.Columns.Count - 1].Visible = true;
    }

    // Export to Excel for OEM Summary
    protected void btnExportToExcel_Click_oem(object sender, EventArgs e)
    {
        // Set the response properties for the Excel download
        Response.ContentType = "application/vnd.ms-excel";
        Response.AddHeader("Content-Disposition", "attachment;filename=OEMSummary.xls");
        Response.Charset = "";
        Response.Buffer = true;
        Response.Clear();

        // Temporarily disable paging for the export
        GridView_oem_summary.AllowPaging = false;

        // Create a StringWriter to capture the HTML output
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        // Render the GridView to the StringWriter
        GridView_oem_summary.RenderControl(hw);

        // Get the HTML string and remove hyperlinks
        string htmlOutput = sw.ToString();
        string cleanedOutput = RemoveHyperlinksFromHtml(htmlOutput);

        // Write the cleaned content to the Response output stream
        Response.Output.Write(cleanedOutput);

        // End the response to download the Excel file
        Response.End();
    }

    //private string RemoveHyperlinksFromHtml(string html)
    //{
    //    // Remove <a> tags but keep the inner text
    //    string pattern = @"<a[^>]*>(.*?)</a>";
    //    string result = System.Text.RegularExpressions.Regex.Replace(html, pattern, "$1",
    //        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    //    return result;
    //}

    private void BindGridView_design_open(string filtervalue = null)
    {
        // Initialize the fields
        drivers = new List<string>();
        driverColumns = new Dictionary<string, string>();

        int dl;
        int dc = 0;
        string driverList = "(";
        string main_query = "";
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
            {
                Session["filterValue"] = filtervalue;
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
                dl = drivers.Count;
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    dc += 1;
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_"); // Ensure column name is safe
                    driverColumns[driver] = safeColumnName;
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    if (dc != dl)
                    {
                        driverList += "'" + driver + "', ";
                    }
                    else
                    {
                        driverList += "'" + driver + "'";
                    }
                }
                driverList += ")";
                main_query = "SELECT A.customer_detail AS Design, B.sw_image_freeze AS SWImageFreeze, B.support_model AS SupportModel " +
                             driverCaseStatements +
                             ", SUM(CASE WHEN A.status IN ('implemented', 'verified') AND A.drivers in " + driverList + " THEN 1 ELSE 0 END) AS Implemented_Verified " +
                             "FROM " + selectedPlatform + " A INNER JOIN " + designTable + " B ON A.customer_detail = B.customer_detail " +
                             "Where sysdebug Like ('%customer_must_fix%') AND status in ('open','implemented','verified') and cmf_request not in ('cmf_reject') GROUP BY A.customer_detail, B.sw_image_freeze, B.support_model " +
                             "ORDER BY Implemented_Verified DESC  ";
            }
            else
            {
                string ddrivers;
                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";

                using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder driversList = new StringBuilder();

                        while (reader.Read())
                        {
                            string driver = reader["drivers"].ToString();

                            if (driversList.Length > 0)
                            {
                                driversList.Append(", ");
                            }

                            driversList.Append(driver);
                        }

                        ddrivers = driversList.ToString();
                    }
                }

                filtervalue = ddrivers;

                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
                dl = drivers.Count;
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    dc += 1;
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                    driverColumns[driver] = safeColumnName;
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    if (dc != dl)
                    {
                        driverList += "'" + driver + "', ";
                    }
                    else
                    {
                        driverList += "'" + driver + "'";
                    }
                }
                driverList += ")";

                string implementedVerifiedCondition = drivers.Count > 0 ? "A.status IN ('implemented', 'verified') " : "A.status IN ('implemented', 'verified')";

                main_query = "SELECT A.customer_detail AS Design, B.sw_image_freeze AS SWImageFreeze, B.support_model AS SupportModel " +
                             driverCaseStatements +
                             ", SUM(CASE WHEN " + implementedVerifiedCondition + " THEN 1 ELSE 0 END) AS Implemented_Verified " +
                             "FROM " + selectedPlatform + " A INNER JOIN " + designTable + " B ON A.customer_detail = B.customer_detail " +
                             "Where sysdebug Like ('%customer_must_fix%') AND status in ('open','implemented','verified') and cmf_request not in ('cmf_reject') GROUP BY A.customer_detail, B.sw_image_freeze, B.support_model " +
                             "ORDER BY Implemented_Verified DESC ";
            }

            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        DataRow totalRow = dt.NewRow();
                        totalRow["Design"] = "Total";
                        totalRow["SWImageFreeze"] = DBNull.Value;
                        totalRow["SupportModel"] = DBNull.Value;

                        if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                        {
                            foreach (var driver in drivers)
                            {
                                string colName = driverColumns[driver] + "_Issues";
                                totalRow[colName] = dt.AsEnumerable().Sum(row => row[colName] != DBNull.Value ? Convert.ToInt32(row[colName]) : 0);
                            }
                        }
                        else
                        {
                            if (dt.Columns.Contains("Driver_issues"))
                            {
                                totalRow["Driver_issues"] = dt.AsEnumerable().Sum(row => row["Driver_issues"] != DBNull.Value ? Convert.ToInt32(row["Driver_issues"]) : 0);
                            }
                        }

                        if (dt.Columns.Contains("Implemented_Verified"))
                        {
                            totalRow["Implemented_Verified"] = dt.AsEnumerable().Sum(row => row["Implemented_Verified"] != DBNull.Value ? Convert.ToInt32(row["Implemented_Verified"]) : 0);
                        }
                        dt.Rows.Add(totalRow);
                    }

                    // Update rows with empty Design
                    //foreach (DataRow row in dt.Rows)
                    //{
                    //    if (string.IsNullOrEmpty(row["Design"].ToString()))
                    //    {
                    //        row["Design"] = "RVP";
                    //        row["SWImageFreeze"] = "NA";
                    //        row["SupportModel"] = "NA";
                    //    }
                    //}

                    GridView_design_open.Columns.Clear();
                    if (dt.Columns.Contains("Design"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "Design", HeaderText = "Design Name", ReadOnly = true });
                    if (dt.Columns.Contains("SWImageFreeze"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "SWImageFreeze", HeaderText = "SW Image Freeze" });
                    if (dt.Columns.Contains("SupportModel"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "SupportModel", HeaderText = "Support Model" });

                    if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                    {
                        foreach (var driver in drivers)
                        {
                            string colName = driverColumns[driver] + "_Issues";
                            if (dt.Columns.Contains(colName))
                            {
                                GridView_design_open.Columns.Add(new BoundField { DataField = colName, HeaderText = driver, ReadOnly = true });
                            }
                        }
                    }
                    else
                    {
                        if (dt.Columns.Contains("Driver_issues"))
                        {
                            GridView_design_open.Columns.Add(new BoundField { DataField = "Driver_issues", HeaderText = "Driver Issues", ReadOnly = true });
                        }
                    }

                    if (dt.Columns.Contains("Implemented_Verified"))
                        GridView_design_open.Columns.Add(new BoundField { DataField = "Implemented_Verified", HeaderText = "Impl/Verified", ReadOnly = true });
                    GridView_design_open.Columns.Add(new CommandField { ShowEditButton = true });

                    GridView_design_open.RowDataBound += new GridViewRowEventHandler(GridView_design_open_RowDataBound);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (string.IsNullOrEmpty(row["Design"].ToString()))
                        {
                            row["Design"] = "RVP";
                            row["SWImageFreeze"] = "NA";
                            row["SupportModel"] = "NA";
                        }
                    }

                    GridView_design_open.DataSource = dt;
                    GridView_design_open.DataBind();

                    GridView2_edit.DataSource = dt;
                    GridView2_edit.DataBind();
                }
            }
        }
    }

    private void BindMilestoneMapping()
    {
        // Uses the exact table/query you provided.
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        // If you want to make the table dynamic by platform, swap the table name below with `selectedPlatform`
        // or a platform-specific name. For now, we use the exact table as requested.
        string milestoneQuery = @"
    SELECT 
        LTRIM(RTRIM(drivers)) AS Driver, 
        COUNT(*) AS CMFCount
    FROM " + selectedPlatform + @"
    WHERE 
        ISNULL(LTRIM(RTRIM(drivers)), '') <> '' 
        AND cmf_request IN ('cmf_ok') 
        AND sysdebug LIKE ('%customer_must_fix%')
    GROUP BY LTRIM(RTRIM(drivers))
    ORDER BY LTRIM(RTRIM(drivers));
";

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(milestoneQuery, con))
        {
            System.Data.DataTable mapTable = new System.Data.DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(mapTable);

            GridView_milestone_map.DataSource = mapTable;
            GridView_milestone_map.DataBind();
        }
    }
    private static string NormalizeSummaryDriver(string driver)
    {
        return driver.Trim().Replace(" ", "_").Replace("-", "_").Replace("/", "_").ToUpperInvariant();
    }

    private static string SummaryDriverSql(string column)
    {
        return "UPPER(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(" + column +
            ")), ' ', '_'), '-', '_'), '/', '_'))";
    }

    private void BindGridView_cmf_summary(string filtervalue = null)
    {
        string full_query = "";
        string main_query = "";
        string los_query = "";
        string total_query = "";
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";
        string firstPatternCaseStatements = "";
        string secondPatternNulls = "";
        //string driver_filter = "";

        //CMF Ask Count
        string designTable = basePlatform + "_CMF_ASK";
        string cmf_pending_count = "SELECT COUNT(cp_id) FROM " + designTable + " WHERE status  IN ('open')";

        //(SELECT COUNT(cp_id) FROM " + designTable + " WHERE status NOT IN ('complete','rejected')) AS cmf_pending_count,

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string notesquery = @"SELECT (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) FROM " + selectedPlatform + " WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS disp_tpt,(SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + " WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS resolve_tpt, (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0) FROM " + selectedPlatform + " WHERE sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok','cmf_duplicate')) AS crit_tpt";

            System.Data.DataTable notestable = new System.Data.DataTable();
            using (SqlCommand cmd = new SqlCommand(notesquery, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(notestable);
                GridView_notes.DataSource = notestable;
                GridView_notes.DataBind();
            }
        }

        using (SqlConnection con2 = new SqlConnection(connectionString))
        {
            //string compquery = @"SELECT component_group, COUNT(cp_id) AS [CMF Pending Count] FROM " + designTable + " WHERE status NOT IN ('complete', 'rejected') GROUP BY component_group";

            string compquery = @"
        SELECT 
            CASE 
                WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
                THEN 'Unassigned' 
                ELSE component_group 
            END AS component_group, 
            COUNT(cp_id) AS [CMF Pending Count] 
        FROM " + designTable + @"  
        WHERE status NOT IN ('complete', 'rejected')
        GROUP BY 
            CASE 
                WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
                THEN 'Unassigned' 
                ELSE component_group 
            END

        UNION ALL

        SELECT 'Total' AS component_group, COUNT(cp_id) AS [CMF Pending Count] 
        FROM " + designTable + @" 
        WHERE status NOT IN ('complete', 'rejected');

        ";

            System.Data.DataTable comptable = new System.Data.DataTable();

            using (SqlCommand cmd2 = new SqlCommand(compquery, con2))
            {
                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                da2.Fill(comptable);
                GridView_comp.DataSource = comptable;
                GridView_comp.DataBind();
            }
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            List<string> drivers = new List<string>();
            Dictionary<string, string> driverColumns = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
            {
                Session["filterValue"] = filtervalue;
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = NormalizeSummaryDriver(driver);
                    if (!string.IsNullOrEmpty(trimmedDriver) && !drivers.Contains(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }

                string Driver_name = "";
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace("]", "]]");
                    driverColumns[driver] = safeColumnName;
                    // Issues count (open records only)
                    driverCaseStatements += ", SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    // Implemented count (implemented records only)
                    driverCaseStatements += ", SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified','future') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented]";
                    Driver_name = safeColumnName + "_Issues";
                }

                string losCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driverColumns[driver].Replace("]]", "]");

                    losCaseStatements += ", SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
                        "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

                    Driver_name = safeColumnName + "_LOS";
                }

                firstPatternCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driverColumns[driver];

                    // For Total row: show individual driver totals in the format Open(LOS) + Duplicates + Implemented
                    // For regular rows: show just Open(LOS) format
                    firstPatternCaseStatements += ", CASE " +
                    "WHEN Component = 'Total' THEN " +
                    "CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' + " +
                    "' + ' + CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) + ' Duplicates'" +  // <-- REMOVED THE IMPLEMENTED PART
                    " ELSE CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')'" +
                    " END AS [" + safeColumnName + "_Issues]";

                    // Implemented column - show individual counts for each driver
                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_Implemented]";
                }

                // Build the main query
                full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
                " FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
                "  FROM " + selectedPlatform + " A     LEFT JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE A.status IN ('open','implemented','verified','future') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
                "UNION ALL   SELECT COALESCE(B.component_group, A.component_group) AS Component " + driverCaseStatements + "       " + losCaseStatements +
                "  FROM " + selectedPlatform + " A   LEFT JOIN " + componentTable + " B         ON A.component_group = B.component_group  WHERE A.status IN ('open','implemented','verified','future') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject') GROUP BY COALESCE(B.component_group, A.component_group)";

                // Add HAVING clause to filter out component rows where open, los and implemented are all zero across every driver
                if (drivers.Count > 0)
                {
                    List<string> driverConditions = new List<string>();
                    foreach (string driver in drivers)
                    {
                        string safeDriver = driver.Replace("'", "''");
                        driverConditions.Add(
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified','future') THEN 1 ELSE 0 END) > 0");
                    }

                    full_query += " HAVING (" + string.Join(" OR ", driverConditions) + ")";
                }

                full_query += ") AS FinalResult ORDER BY CASE WHEN FinalResult.Component = 'Total' THEN 1 ELSE 0 END, Component";


                main_query = full_query;
            }
            else
            {
                string ddrivers;
                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open','implemented','verified','future') and cmf_request not in ('cmf_reject') and sysdebug Like ('%customer_must_fix%') ";

                using (SqlCommand cmd = new SqlCommand(alldriver_query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder driversList = new StringBuilder();

                        while (reader.Read())
                        {
                            string driver = reader["drivers"].ToString();

                            if (driversList.Length > 0)
                            {
                                driversList.Append(", ");
                            }

                            driversList.Append(driver);
                        }

                        ddrivers = driversList.ToString();
                    }
                }

                filtervalue = ddrivers;
                System.Diagnostics.Debug.WriteLine(filtervalue + "\n");

                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = NormalizeSummaryDriver(driver);
                    if (!string.IsNullOrEmpty(trimmedDriver) && !drivers.Contains(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }

                string Driver_name = "";
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace("]", "]]");
                    driverColumns[driver] = safeColumnName;
                    // Issues count (open records only)
                    driverCaseStatements += ", SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    // Implemented count (implemented records only)
                    driverCaseStatements += ", SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified','future') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented], " +
                        "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request = 'cmf_duplicate' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified','future') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_ImplementedDup]";
                    Driver_name = safeColumnName + "_Issues";
                }

                string losCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driverColumns[driver];

                    losCaseStatements += ", SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
                        "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + driver.Replace("'", "''") + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

                    Driver_name = safeColumnName + "_LOS";
                }

                firstPatternCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driverColumns[driver];

                    firstPatternCaseStatements += ", CASE " +
                        "WHEN Component = 'Total' THEN " +
                        "CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' + " +
                        "' + ' + CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) + ' Duplicates'" +
                        " ELSE CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')'" +
                        " END AS [" + safeColumnName + "_Issues], " +
                        "CASE WHEN Component = 'Total' THEN " +
                        "CAST([" + safeColumnName + "_Implemented] AS VARCHAR) + ' + ' + CAST([" + safeColumnName + "_ImplementedDup] AS VARCHAR) + ' Duplicates'" +
                        " ELSE CAST([" + safeColumnName + "_Implemented] AS VARCHAR) " +
                        "END AS [" + safeColumnName + "_Implemented]";
                }

                //// Build the main query
                //full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
                //" FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
                //"  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
                //"UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
                //"  FROM " + selectedPlatform + " A INNER JOIN " + componentTable + " B ON A.component_group = B.component_group WHERE (A.status IN ('open','implemented','verified'))  AND A.sysdebug LIKE '%customer_must_fix%'  AND A.cmf_request NOT IN ('cmf_reject')  AND A.drivers IN ( SELECT DISTINCT drivers FROM " + selectedPlatform + @" WHERE status IN ('open') AND cmf_request NOT IN ('cmf_reject') AND sysdebug LIKE '%customer_must_fix%') GROUP BY B.component_group";



                //// Add HAVING clause to filter out components with all zeros
                //if (drivers.Count > 0)
                //{
                //    full_query += " HAVING (";

                //    List<string> driverConditions = new List<string>();
                //    foreach (string driver in drivers)
                //    {
                //        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
                //        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                //                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
                //    }

                //    full_query += string.Join(" OR ", driverConditions) + ")";
                //}

                //// Add HAVING clause to filter out components with no CMFs at all (platform-based)
                ////full_query += @"
                ////    HAVING
                ////        SUM(CASE
                ////                WHEN A.sysdebug LIKE '%customer_must_fix%'
                ////                 AND A.cmf_request NOT IN ('cmf_reject')
                ////               THEN 1
                ////            END) > 0
                ////    ";

                ////full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";
                //full_query += ") AS FinalResult ORDER BY CASE WHEN FinalResult.Component = 'Total' THEN 1 ELSE 0 END, Component";

                full_query =
"SELECT CASE " +
"  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented' " +
"  ELSE Component END AS Component " +
firstPatternCaseStatements +

" FROM ( " +

"   SELECT 'Total' AS Component " +
        driverCaseStatements +
        losCaseStatements +
"   FROM " + selectedPlatform + " A " +
"   LEFT JOIN " + componentTable + " B " +
"       ON A.component_group = B.component_group " +
"   WHERE A.status IN ('open','implemented','verified','future') " +
"     AND A.sysdebug LIKE '%customer_must_fix%' " +
"     AND A.cmf_request NOT IN ('cmf_reject') " +

"   UNION ALL " +

"   SELECT COALESCE(B.component_group, A.component_group) AS Component " +
        driverCaseStatements +
        losCaseStatements +
"   FROM " + selectedPlatform + " A " +
"   LEFT JOIN " + componentTable + " B " +
"       ON A.component_group = B.component_group " +
"   WHERE A.status IN ('open','implemented','verified','future') " +
"     AND A.sysdebug LIKE '%customer_must_fix%' " +
"     AND A.cmf_request NOT IN ('cmf_reject') " +
"   GROUP BY COALESCE(B.component_group, A.component_group) ";

                // Add HAVING clause to filter out component rows where open, los and implemented are all zero across every driver
                if (drivers.Count > 0)
                {
                    List<string> driverConditions = new List<string>();
                    foreach (string driver in drivers)
                    {
                        string safeDriver = driver.Replace("'", "''");
                        driverConditions.Add(
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
                            "SUM(CASE WHEN " + SummaryDriverSql("A.drivers") + " = N'" + safeDriver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified','future') THEN 1 ELSE 0 END) > 0");
                    }

                    full_query += " HAVING (" + string.Join(" OR ", driverConditions) + ")";
                }

                full_query +=
") AS FinalResult " +
"ORDER BY CASE WHEN Component = 'Total' THEN 1 ELSE 0 END, Component";

                main_query = full_query;


            }


            // Summary Query to fetch metrics
            string summaryQuery = @"
        SELECT 
            SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS TotalCount,
            SUM(CASE WHEN [cmf_request] = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS Duplicates,
            SUM(CASE WHEN [cmf_request] in ('cmf_ok', 'cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] IN ('open')) THEN 1 ELSE 0 END) AS OpenCount,
            SUM(CASE WHEN [cmf_request] in ('cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] IN ('open')) THEN 1 ELSE 0 END ) AS OpenDup,
            SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedCount,
            SUM(CASE WHEN cmf_request in ('cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedDup,
            SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] IN ('implemented', 'future') THEN 1 ELSE 0 END) AS ImplementedCount,
            SUM(CASE WHEN cmf_request in ('cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] IN ('implemented', 'future') THEN 1 ELSE 0 END) AS ImplementedDup,
            STUFF((
                SELECT ', ' + [component_group]
                FROM " + selectedPlatform + @"
                WHERE cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] IN ('implemented', 'future') 
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ImplementedComponents
        FROM " + selectedPlatform;

            // Fetch summary data
            System.Data.DataTable summaryTable = new System.Data.DataTable();
            using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                {
                    da.SelectCommand.Parameters.AddWithValue("@FilterValue", filtervalue);
                }
                da.Fill(summaryTable);
                //Add custom columns to the summary table
                summaryTable.Columns.Add("Total", typeof(string));
                summaryTable.Columns.Add("Closed", typeof(string));
                summaryTable.Columns.Add("Open", typeof(string));
                //summaryTable.Columns.Add("Implemented", typeof(string));
                //summaryTable.Columns.Add("ImplementedDetails", typeof(string));
                // Add these columns to your DataTable
                summaryTable.Columns.Add("ImplementedCountAndDup", typeof(string));
                summaryTable.Columns.Add("ImplementedComponentsOnly", typeof(string));

                // Then in your data processing:


                if (summaryTable.Rows.Count > 0)
                {
                    DataRow row = summaryTable.Rows[0];
                    // Safely access columns and handle DBNull values
                    int totalCount = row["TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["TotalCount"]);
                    int duplicates = row["Duplicates"] is DBNull ? 0 : Convert.ToInt32(row["Duplicates"]);
                    int openCount = row["OpenCount"] is DBNull ? 0 : Convert.ToInt32(row["OpenCount"]);
                    int openDup = row["OpenDup"] is DBNull ? 0 : Convert.ToInt32(row["OpenDup"]);
                    int closedCount = row["ClosedCount"] is DBNull ? 0 : Convert.ToInt32(row["ClosedCount"]);
                    int closedDup = row["ClosedDup"] is DBNull ? 0 : Convert.ToInt32(row["ClosedDup"]);
                    int implementedCount = row["ImplementedCount"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedCount"]);
                    int implementedDup = row["ImplementedDup"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedDup"]);
                    //string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();
                    string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();

                    // Split the string into a list of components
                    string[] componentsList = implementedComponents.Split(',');

                    // Create a dictionary to count occurrences of each component
                    Dictionary<string, int> componentCounts = new Dictionary<string, int>();

                    // Initialize a variable to store the total count
                    int totalCountComp = 0;

                    foreach (string component in componentsList)
                    {
                        string trimmedComponent = component.Trim(); // Trim spaces
                        if (componentCounts.ContainsKey(trimmedComponent))
                        {
                            componentCounts[trimmedComponent]++; // Increment count
                        }
                        else
                        {
                            componentCounts[trimmedComponent] = 1; // Initialize count
                        }

                        // Increment the total count for each component
                        totalCountComp++;
                    }

                    // Get the original number of components (from the split list)
                    int originalCount = implementedCount;

                    // Build the formatted result using string.Format
                    List<string> resultList = new List<string>();

                    foreach (KeyValuePair<string, int> kvp in componentCounts)
                    {
                        resultList.Add(string.Format("{0} - {1}", kvp.Key, kvp.Value));
                    }

                    // Check if total count does not match the original count
                    if (totalCountComp != originalCount)
                    {
                        int difference = Math.Abs(originalCount - totalCountComp);
                        resultList.Add(string.Format("Other - {0}", difference));
                    }

                    // Join the result into a single string
                    string resultComp = string.Join(", ", resultList);

                    row["Total"] = String.Format("{0} ({1} CMF Duplicates)", totalCount, duplicates);
                    row["Closed"] = String.Format("{0} ({1} CMF Duplicates)", closedCount, closedDup);
                    row["Open"] = String.Format("{0} ({1} CMF Duplicates)", openCount, openDup);
                    //if (implementedCount != 0)
                    //{
                    //    //row["ImplementedDetails"] = String.Format("{0} ({1} CMF Duplicates) - ({2})",
                    //    //    implementedCount, implementedDup, resultComp);
                    //    row["ImplementedDetails"] = String.Format("({0} CMF Duplicates) ({1})",
                    //        implementedDup, resultComp);
                    //}
                    //else
                    //{
                    //    row["ImplementedDetails"] = "";
                    //}

                    if (implementedCount != 0)
                    {
                        row["ImplementedCountAndDup"] = String.Format("{0} ({1} CMF Duplicates)",
                            implementedCount, implementedDup);
                        row["ImplementedComponentsOnly"] = String.Format("({0})", resultComp);
                    }
                    else
                    {
                        row["ImplementedCountAndDup"] = "0 (0 CMF Duplicates)";
                        row["ImplementedComponentsOnly"] = "";
                    }

                }
            }

            using (SqlCommand cmd = new SqlCommand(full_query, conn))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    sda.Fill(dt);

                    // Clear existing columns
                    GridView_cmf_summary.Columns.Clear();
                    GridView_cmf_summary.Width = Unit.Percentage(100);

                    double componentWidth = 20.0;
                    double driverGroupWidth = (80.0) / drivers.Count;

                    // Add Component column
                    BoundField componentField = new BoundField
                    {
                        DataField = "Component",
                        HeaderText = "Component_Open_CMFs",
                        ReadOnly = true
                    };
                    componentField.ItemStyle.Width = Unit.Percentage(componentWidth);
                    componentField.HeaderStyle.Width = Unit.Percentage(componentWidth);
                    componentField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                    componentField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                    componentField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                    componentField.HeaderStyle.ForeColor = System.Drawing.Color.White;
                    GridView_cmf_summary.Columns.Add(componentField);

                    // Add columns for each driver dynamically
                    foreach (var driver in drivers)
                    {
                        string safeColumnName = driverColumns[driver];

                        // Open (LOS) Column - using TemplateField for clickable functionality
                        TemplateField openField = new TemplateField();
                        openField.HeaderText = HttpUtility.HtmlEncode(driver) + "<br/>Open (LOS)";
                        openField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 2);
                        openField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 2);
                        openField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                        openField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        openField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        openField.HeaderStyle.ForeColor = System.Drawing.Color.White;

                        openField.ItemTemplate = new CMFIssueCountTemplate(safeColumnName + "_Issues", driver, "Open");
                        GridView_cmf_summary.Columns.Add(openField);

                        // Implemented/Verified Column - using TemplateField for clickable functionality
                        TemplateField implField = new TemplateField();
                        implField.HeaderText = HttpUtility.HtmlEncode(driver) + "<br/>Impl/Verified (Duplicates)";
                        implField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 2);
                        implField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 2);
                        implField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                        implField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                        implField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        implField.HeaderStyle.ForeColor = System.Drawing.Color.White;

                        implField.ItemTemplate = new CMFIssueCountTemplate(safeColumnName + "_Implemented", driver, "Implemented");
                        GridView_cmf_summary.Columns.Add(implField);
                    }

                    GridView_cmf_summary.DataSource = dt;
                    GridView_cmf_summary.DataBind();
                }
            }

            GridView_cmf_summary1.DataSource = summaryTable;
            GridView_cmf_summary1.DataBind();
            BindMilestoneMapping();
        }
    }

    //    private void BindGridView_cmf_summary(string filtervalue = null)
    //    {
    //        string full_query = "";
    //        string main_query = "";
    //        string los_query = "";
    //        string total_query = "";
    //        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
    //        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";
    //        string firstPatternCaseStatements = "";
    //        string secondPatternNulls = "";
    //        //string driver_filter = "";

    //        //CMF Ask Count
    //        string designTable = basePlatform + "_CMF_ASK";
    //        string cmf_pending_count = "SELECT COUNT(cp_id) FROM " + designTable + " WHERE status  IN ('open')";

    //        //(SELECT COUNT(cp_id) FROM " + designTable + " WHERE status NOT IN ('complete','rejected')) AS cmf_pending_count,

    //        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

    //        using (SqlConnection con = new SqlConnection(connectionString))
    //        {
    //            string notesquery = @"SELECT 
    //(SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) 
    // FROM " + selectedPlatform + @" 
    // WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS disp_tpt,
    //(SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) AS INT)), 0) 
    // FROM " + selectedPlatform + @" 
    // WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%')) AS resolve_tpt, 
    //(SELECT ISNULL(AVG(CAST(days_active AS INT)), 0) 
    // FROM " + selectedPlatform + @" 
    // WHERE sysdebug Like ('%customer_must_fix%') AND cmf_request in ('cmf_ok','cmf_duplicate')) AS crit_tpt";

    //            System.Data.DataTable notestable = new System.Data.DataTable();
    //            using (SqlCommand cmd = new SqlCommand(notesquery, con))
    //            {
    //                SqlDataAdapter da = new SqlDataAdapter(cmd);
    //                da.Fill(notestable);
    //                GridView_notes.DataSource = notestable;
    //                GridView_notes.DataBind();
    //            }
    //        }

    //        using (SqlConnection con2 = new SqlConnection(connectionString))
    //        {
    //            //string compquery = @"SELECT component_group, COUNT(cp_id) AS [CMF Pending Count] FROM " + designTable + " WHERE status NOT IN ('complete', 'rejected') GROUP BY component_group";

    //            string compquery = @"
    //SELECT 
    //    CASE 
    //        WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
    //        THEN 'Unassigned' 
    //        ELSE component_group 
    //    END AS component_group, 
    //    COUNT(cp_id) AS [CMF Pending Count] 
    //FROM " + designTable + @"  
    //WHERE status NOT IN ('complete', 'rejected')
    //GROUP BY 
    //    CASE 
    //        WHEN component_group IS NULL OR component_group = '' OR component_group = 'no iDST assigned' 
    //        THEN 'Unassigned' 
    //        ELSE component_group 
    //    END

    //UNION ALL

    //SELECT 'Total' AS component_group, COUNT(cp_id) AS [CMF Pending Count] 
    //FROM " + designTable + @" 
    //WHERE status NOT IN ('complete', 'rejected');
    //";

    //            System.Data.DataTable comptable = new System.Data.DataTable();

    //            using (SqlCommand cmd2 = new SqlCommand(compquery, con2))
    //            {
    //                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
    //                da2.Fill(comptable);
    //                GridView_comp.DataSource = comptable;
    //                GridView_comp.DataBind();
    //            }
    //        }

    //        using (SqlConnection conn = new SqlConnection(connectionString))
    //        {
    //            List<string> drivers = new List<string>();
    //            Dictionary<string, string> driverColumns = new Dictionary<string, string>();

    //            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
    //            {
    //                Session["filterValue"] = filtervalue;
    //                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

    //                foreach (string driver in rawDrivers)
    //                {
    //                    string trimmedDriver = driver.Trim();
    //                    if (!string.IsNullOrEmpty(trimmedDriver))
    //                    {
    //                        drivers.Add(trimmedDriver);
    //                    }
    //                }

    //                string Driver_name = "";
    //                string driverCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;
    //                    // Issues count (open records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
    //                    // Implemented count (implemented records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented]";
    //                    Driver_name = safeColumnName + "_Issues";
    //                }

    //                string losCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    losCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
    //                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

    //                    Driver_name = safeColumnName + "_LOS";
    //                }

    //                firstPatternCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    // For Total row: show individual driver totals in the format Open(LOS) + Duplicates
    //                    // For regular rows: show just Open(LOS) format
    //                    firstPatternCaseStatements += ", CASE " +
    //                    "WHEN Component = 'Total' THEN " +
    //                    "CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' + " +
    //                    "' + ' + CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) + ' Duplicates'" +
    //                    " ELSE CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')'" +
    //                    " END AS [" + safeColumnName + "_Issues]";

    //                    // Implemented column - show individual counts for each driver
    //                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_Implemented]";
    //                }

    //                // Build the main query
    //                full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
    //                " FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
    //                "UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A   INNER JOIN " + componentTable + " B         ON A.component_group = B.component_group  WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject') GROUP BY B.component_group";

    //                // Add HAVING clause to filter out components with all zeros
    //                if (drivers.Count > 0)
    //                {
    //                    full_query += " HAVING (";

    //                    List<string> driverConditions = new List<string>();
    //                    foreach (string driver in drivers)
    //                    {
    //                        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
    //                                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
    //                    }

    //                    full_query += string.Join(" OR ", driverConditions) + ")";
    //                }

    //                full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";

    //                main_query = full_query;
    //            }
    //            else
    //            {
    //                string ddrivers;
    //                string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open') and cmf_request not in ('cmf_reject') and sysdebug Like ('%customer_must_fix%') ";

    //                using (SqlCommand cmd = new SqlCommand(alldriver_query, conn))
    //                {
    //                    conn.Open();
    //                    using (SqlDataReader reader = cmd.ExecuteReader())
    //                    {
    //                        StringBuilder driversList = new StringBuilder();

    //                        while (reader.Read())
    //                        {
    //                            string driver = reader["drivers"].ToString();

    //                            if (driversList.Length > 0)
    //                            {
    //                                driversList.Append(", ");
    //                            }

    //                            driversList.Append(driver);
    //                        }

    //                        ddrivers = driversList.ToString();
    //                    }
    //                    conn.Close();
    //                }

    //                filtervalue = ddrivers;

    //                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

    //                foreach (string driver in rawDrivers)
    //                {
    //                    string trimmedDriver = driver.Trim();
    //                    if (!string.IsNullOrEmpty(trimmedDriver))
    //                    {
    //                        drivers.Add(trimmedDriver);
    //                    }
    //                }

    //                string Driver_name = "";
    //                string driverCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;
    //                    // Issues count (open records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
    //                    // Implemented count (implemented records only)
    //                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Implemented]";
    //                    Driver_name = safeColumnName + "_Issues";
    //                }

    //                string losCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    losCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' AND A.los = 'Yes' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_LOS], " +
    //                        "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_duplicate' AND A.status IN ('open') THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Duplicates]";

    //                    Driver_name = safeColumnName + "_LOS";
    //                }

    //                firstPatternCaseStatements = "";
    //                foreach (string driver in drivers)
    //                {
    //                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                    driverColumns[driver] = safeColumnName;

    //                    firstPatternCaseStatements += ", CASE " +
    //                    "WHEN Component = 'Total' THEN " +
    //                    "CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')' + " +
    //                    "' + ' + CAST([" + safeColumnName + "_Duplicates] AS VARCHAR) + ' Duplicates'" +
    //                    " ELSE CAST([" + safeColumnName + "_Issues] AS VARCHAR) + '(' + CAST([" + safeColumnName + "_LOS] AS VARCHAR) + ')'" +
    //                    " END AS [" + safeColumnName + "_Issues]";

    //                    // Implemented column - show individual counts for each driver
    //                    firstPatternCaseStatements += ", CAST([" + safeColumnName + "_Implemented] AS VARCHAR) AS [" + safeColumnName + "_Implemented]";
    //                }

    //                // Build the main query
    //                full_query = "SELECT  CASE  WHEN Component = 'Total' THEN 'Total (LOS) + Duplicates + Implemented'  ELSE Component END AS Component " + firstPatternCaseStatements +
    //                " FROM (    SELECT 'Total' AS Component " + driverCaseStatements + "" + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A     INNER JOIN " + componentTable + " B       ON A.component_group = B.component_group    WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject')  " +
    //                "UNION ALL   SELECT B.component_group AS Component " + driverCaseStatements + "       " + losCaseStatements +
    //                "  FROM " + selectedPlatform + " A   INNER JOIN " + componentTable + " B         ON A.component_group = B.component_group  WHERE (A.status IN ('open') OR A.status = 'implemented') AND A.sysdebug Like ('%customer_must_fix%')  and cmf_request not in ('cmf_reject') GROUP BY B.component_group";

    //                // Add HAVING clause to filter out components with all zeros
    //                if (drivers.Count > 0)
    //                {
    //                    full_query += " HAVING (";

    //                    List<string> driverConditions = new List<string>();
    //                    foreach (string driver in drivers)
    //                    {
    //                        string safeColumnName = driver.Replace(" ", "_").Replace("-", "_");
    //                        driverConditions.Add("SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request != 'cmf_duplicate' AND A.cmf_request != 'cmf_reject' AND A.status IN ('open') THEN 1 ELSE 0 END) > 0 OR " +
    //                                            "SUM(CASE WHEN A.drivers = '" + driver + "' AND A.cmf_request = 'cmf_ok' AND A.sysdebug Like ('%customer_must_fix%') AND A.status in ('implemented','verified') THEN 1 ELSE 0 END) > 0");
    //                    }

    //                    full_query += string.Join(" OR ", driverConditions) + ")";
    //                }

    //                full_query += ") AS FinalResult ORDER BY CASE WHEN Component = 'Total (LOS) + Duplicates + Implemented' THEN 1 ELSE 0 END, Component";

    //                main_query = full_query;
    //            }

    //            // ===========================
    //            // Summary Query to fetch metrics (UPDATED to include DuplicatesClosed & DuplicatesImplemented)
    //            // ===========================
    //            string summaryQuery = @"
    //SELECT 
    //    -- Totals
    //    SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS TotalCount,
    //    SUM(CASE WHEN [cmf_request] = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS Duplicates,

    //    -- Closed metrics
    //    SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%')
    //             AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedCount,
    //    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%')
    //             AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS DuplicatesClosed,

    //    -- Implemented metrics
    //    SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%')
    //             AND [status] = 'implemented' THEN 1 ELSE 0 END) AS ImplementedCount,
    //    SUM(CASE WHEN cmf_request = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%')
    //             AND [status] = 'implemented' THEN 1 ELSE 0 END) AS DuplicatesImplemented,

    //    -- Implemented components list (for details)
    //    STUFF((
    //        SELECT ', ' + [component_group]
    //        FROM " + selectedPlatform + @"
    //        WHERE cmf_request in ('cmf_ok','cmf_duplicate') 
    //          AND sysdebug Like ('%customer_must_fix%') 
    //          AND [status] = 'implemented' 
    //        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ImplementedComponents
    //FROM " + selectedPlatform;

    //            // Fetch summary data
    //            System.Data.DataTable summaryTable = new System.Data.DataTable();
    //            using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
    //            {
    //                SqlDataAdapter da = new SqlDataAdapter(cmd);

    //                // (No parameter needed here since the query doesn't use @FilterValue)
    //                da.Fill(summaryTable);

    //                // Add custom columns to the summary table
    //                summaryTable.Columns.Add("Total", typeof(string));
    //                summaryTable.Columns.Add("Closed", typeof(string));
    //                summaryTable.Columns.Add("Implemented", typeof(string));         // NEW display column
    //                summaryTable.Columns.Add("ImplementedDetails", typeof(string));

    //                if (summaryTable.Rows.Count > 0)
    //                {
    //                    DataRow row = summaryTable.Rows[0];
    //                    // Safely access columns and handle DBNull values
    //                    int totalCount = row["TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["TotalCount"]);
    //                    int duplicates = row["Duplicates"] is DBNull ? 0 : Convert.ToInt32(row["Duplicates"]);

    //                    int closedCount = row["ClosedCount"] is DBNull ? 0 : Convert.ToInt32(row["ClosedCount"]);
    //                    int duplicatesClosed = row["DuplicatesClosed"] is DBNull ? 0 : Convert.ToInt32(row["DuplicatesClosed"]); // NEW

    //                    int implementedCount = row["ImplementedCount"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedCount"]);
    //                    int duplicatesImplemented = row["DuplicatesImplemented"] is DBNull ? 0 : Convert.ToInt32(row["DuplicatesImplemented"]); // NEW

    //                    string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();

    //                    // Split and count component occurrences for ImplementedDetails
    //                    string[] componentsList = implementedComponents.Split(',');
    //                    Dictionary<string, int> componentCounts = new Dictionary<string, int>();
    //                    int totalCountComp = 0;

    //                    foreach (string component in componentsList)
    //                    {
    //                        string trimmedComponent = component.Trim(); // Trim spaces
    //                        if (string.IsNullOrEmpty(trimmedComponent)) continue;

    //                        if (componentCounts.ContainsKey(trimmedComponent))
    //                        {
    //                            componentCounts[trimmedComponent]++; // Increment count
    //                        }
    //                        else
    //                        {
    //                            componentCounts[trimmedComponent] = 1; // Initialize count
    //                        }

    //                        // Increment the total count for each component
    //                        totalCountComp++;
    //                    }

    //                    // The original count based on ImplementedCount
    //                    int originalCount = implementedCount;

    //                    // Build the formatted result using string.Format
    //                    List<string> resultList = new List<string>();

    //                    foreach (KeyValuePair<string, int> kvp in componentCounts)
    //                    {
    //                        resultList.Add(string.Format("{0} - {1}", kvp.Key, kvp.Value));
    //                    }

    //                    // Check if total count does not match the original count
    //                    if (totalCountComp != originalCount)
    //                    {
    //                        int difference = Math.Abs(originalCount - totalCountComp);
    //                        resultList.Add(string.Format("Other - {0}", difference));
    //                    }

    //                    // Join the result into a single string
    //                    string resultComp = string.Join(", ", resultList);

    //                    // Display strings with duplicates in brackets
    //                    row["Total"] = string.Format("{0} ({1} CMF Duplicates)", totalCount, duplicates);
    //                    row["Closed"] = string.Format("{0} ({1} CMF Duplicates)", closedCount, duplicatesClosed);                   // UPDATED
    //                    row["Implemented"] = string.Format("{0} ({1} CMF Duplicates)", implementedCount, duplicatesImplemented);   // NEW

    //                    if (implementedCount != 0)
    //                    {
    //                        row["ImplementedDetails"] = "(" + resultComp + ")";
    //                    }
    //                    else
    //                    {
    //                        row["ImplementedDetails"] = "";
    //                    }
    //                }
    //            }

    //            using (SqlCommand cmd = new SqlCommand(full_query, conn))
    //            {
    //                System.Data.DataTable dt = new System.Data.DataTable();
    //                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
    //                {
    //                    sda.Fill(dt);

    //                    // Clear existing columns
    //                    GridView_cmf_summary.Columns.Clear();
    //                    GridView_cmf_summary.Width = Unit.Percentage(100);

    //                    double componentWidth = 20.0;
    //                    double driverGroupWidth = (drivers.Count > 0) ? (80.0 / drivers.Count) : 80.0; // guard against division by zero

    //                    // Add Component column
    //                    BoundField componentField = new BoundField
    //                    {
    //                        DataField = "Component",
    //                        HeaderText = "Component_Open_CMFs",
    //                        ReadOnly = true
    //                    };
    //                    componentField.ItemStyle.Width = Unit.Percentage(componentWidth);
    //                    componentField.HeaderStyle.Width = Unit.Percentage(componentWidth);
    //                    componentField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
    //                    componentField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
    //                    componentField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                    componentField.HeaderStyle.ForeColor = System.Drawing.Color.White;
    //                    GridView_cmf_summary.Columns.Add(componentField);

    //                    // Add columns for each driver dynamically
    //                    foreach (var driver in drivers)
    //                    {
    //                        string safeColumnName = driverColumns[driver];

    //                        // Open (LOS) Column
    //                        BoundField openField = new BoundField
    //                        {
    //                            DataField = safeColumnName + "_Issues",
    //                            HeaderText = safeColumnName + "<br/>Open (LOS)",
    //                            ReadOnly = true,
    //                            HtmlEncode = false
    //                        };
    //                        openField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        openField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        openField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        openField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        openField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                        openField.HeaderStyle.ForeColor = System.Drawing.Color.White;
    //                        GridView_cmf_summary.Columns.Add(openField);

    //                        // Implemented/Verified Column
    //                        BoundField implField = new BoundField
    //                        {
    //                            DataField = safeColumnName + "_Implemented",
    //                            HeaderText = safeColumnName + "<br/>Impl/Verified",
    //                            ReadOnly = true,
    //                            HtmlEncode = false
    //                        };
    //                        implField.ItemStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        implField.HeaderStyle.Width = Unit.Percentage(driverGroupWidth / 2);
    //                        implField.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        implField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
    //                        implField.HeaderStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                        implField.HeaderStyle.ForeColor = System.Drawing.Color.White;
    //                        GridView_cmf_summary.Columns.Add(implField);
    //                    }

    //                    GridView_cmf_summary.DataSource = dt;
    //                    GridView_cmf_summary.DataBind();
    //                }
    //            }

    //            GridView_cmf_summary1.DataSource = summaryTable;
    //            GridView_cmf_summary1.DataBind();
    //        }
    //    }
    protected void GridView_cmf_summary_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            // Get drivers from ViewState
            string[] drivers = (string[])ViewState["Drivers"];
            double driverGroupWidth = (double)ViewState["DriverGroupWidth"];

            if (drivers != null && drivers.Length > 0)
            {
                // Create a new header row for driver names
                GridViewRow driverHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);

                // Add empty cell for Component column
                TableHeaderCell componentHeaderCell = new TableHeaderCell();
                componentHeaderCell.Text = "";
                componentHeaderCell.Width = Unit.Percentage(20.0);
                componentHeaderCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                componentHeaderCell.ForeColor = System.Drawing.Color.White;
                componentHeaderCell.HorizontalAlign = HorizontalAlign.Center;
                driverHeaderRow.Cells.Add(componentHeaderCell);

                // Add driver name headers
                for (int i = 0; i < drivers.Length; i++)
                {
                    TableHeaderCell driverCell = new TableHeaderCell();
                    driverCell.Text = drivers[i];
                    driverCell.ColumnSpan = 2; // Span across both "Open (LOS)" and "Impl/Verified" columns
                    driverCell.Width = Unit.Percentage(driverGroupWidth);
                    driverCell.BackColor = System.Drawing.Color.Red;
                    driverCell.ForeColor = System.Drawing.Color.White;
                    driverCell.HorizontalAlign = HorizontalAlign.Center;
                    driverCell.Font.Bold = true;
                    driverHeaderRow.Cells.Add(driverCell);
                }

                // Insert the driver header row at the top - specify the WebControls.Table
                ((System.Web.UI.WebControls.Table)GridView_cmf_summary.Controls[0]).Rows.AddAt(0, driverHeaderRow);
            }
        }
    }


    private void AddDriverNamesRow()
    {
        try
        {
            List<string> drivers = Session["CurrentDrivers"] as List<string>;

            if (drivers != null && drivers.Count > 0)
            {
                System.Web.UI.WebControls.Table gridTable = GridView_cmf_summary.Controls[0] as System.Web.UI.WebControls.Table;
                if (gridTable != null)
                {
                    // Create a new row for driver names
                    TableRow driverRow = new TableRow();
                    driverRow.TableSection = TableRowSection.TableHeader;

                    // Add empty cell for Component column
                    TableHeaderCell componentCell = new TableHeaderCell();
                    componentCell.Text = "";
                    componentCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                    componentCell.ForeColor = System.Drawing.Color.White;
                    componentCell.HorizontalAlign = HorizontalAlign.Center;
                    componentCell.Font.Bold = true;
                    driverRow.Cells.Add(componentCell);

                    // Add driver names and empty cells
                    foreach (string driver in drivers)
                    {
                        // Driver name cell (above "Open (LOS)")
                        TableHeaderCell driverCell = new TableHeaderCell();
                        driverCell.Text = driver;
                        driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        driverCell.ForeColor = System.Drawing.Color.White;
                        driverCell.HorizontalAlign = HorizontalAlign.Center;
                        driverCell.Font.Bold = true;
                        driverRow.Cells.Add(driverCell);

                        // Empty cell (above "Impl/Verified")
                        TableHeaderCell emptyCell = new TableHeaderCell();
                        emptyCell.Text = "";
                        emptyCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
                        emptyCell.ForeColor = System.Drawing.Color.White;
                        emptyCell.HorizontalAlign = HorizontalAlign.Center;
                        driverRow.Cells.Add(emptyCell);
                    }

                    // Insert at the top (position 0)
                    gridTable.Rows.AddAt(0, driverRow);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle silently
            System.Diagnostics.Debug.WriteLine("Error adding driver row: " + ex.Message);
        }
    }
    private List<string> GetDriversList()
    {
        List<string> drivers = new List<string>();

        // Try to get from session first
        if (Session["CurrentDrivers"] != null)
        {
            return (List<string>)Session["CurrentDrivers"];
        }

        // If not in session, recreate from filtervalue
        string filtervalue = null;
        if (Session["filterValue"] != null)
        {
            filtervalue = Session["filterValue"].ToString();
        }

        if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
        {
            string[] rawDrivers = filtervalue.Split(new char[] { ',' });
            foreach (string driver in rawDrivers)
            {
                string trimmedDriver = driver.Trim();
                if (!string.IsNullOrEmpty(trimmedDriver))
                {
                    drivers.Add(trimmedDriver);
                }
            }
        }

        return drivers;
    }


    //protected void GridView_cmf_summary_RowCreated(object sender, GridViewRowEventArgs e)
    //{
    //    if (e.Row.RowType == DataControlRowType.Header)
    //    {
    //        // Get the drivers from session or recreate the list
    //        List<string> drivers = GetDriversList(); // You'll need to implement this method

    //        if (drivers != null && drivers.Count > 0)
    //        {
    //            // Create the top header row for driver names
    //            GridViewRow topHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);
    //            topHeaderRow.TableSection = TableRowSection.TableHeader;

    //            // Component column header (spans 2 rows)
    //            TableHeaderCell componentCell = new TableHeaderCell();
    //            componentCell.Text = "Component_Open_CMFs";
    //            componentCell.RowSpan = 2;
    //            componentCell.HorizontalAlign = HorizontalAlign.Center;
    //            componentCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //            componentCell.ForeColor = System.Drawing.Color.White;
    //            topHeaderRow.Cells.Add(componentCell);

    //            // Driver headers (each spans 2 columns)
    //            foreach (var driver in drivers)
    //            {
    //                TableHeaderCell driverCell = new TableHeaderCell();
    //                driverCell.Text = driver;
    //                driverCell.ColumnSpan = 2;
    //                driverCell.HorizontalAlign = HorizontalAlign.Center;
    //                driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0056b3");
    //                driverCell.ForeColor = System.Drawing.Color.White;
    //                topHeaderRow.Cells.Add(driverCell);
    //            }

    //            // Add the top header row
    //            ((Table)GridView_cmf_summary.Controls[0]).Rows.AddAt(0, topHeaderRow);

    //            // Modify the existing header row
    //            e.Row.Cells[0].Visible = false; // Hide the component cell since we're using rowspan
    //        }
    //    }
    //}

    private void AddDriverHeaderRow()
    {
        if (GridView_cmf_summary.Rows.Count > 0)
        {
            // Get the existing header row
            GridViewRow existingHeader = GridView_cmf_summary.HeaderRow;

            // Create new main header row
            GridViewRow mainHeaderRow = new GridViewRow(0, 0, DataControlRowType.Header, DataControlRowState.Normal);

            // Component header cell (spans 1 column, 2 rows)
            TableHeaderCell componentCell = new TableHeaderCell();
            componentCell.Text = "Component_Open_CMFs";
            componentCell.RowSpan = 2;
            componentCell.HorizontalAlign = HorizontalAlign.Center;
            componentCell.VerticalAlign = VerticalAlign.Middle;
            componentCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#2a62c2");
            componentCell.ForeColor = System.Drawing.Color.White;
            componentCell.Font.Bold = true;
            componentCell.CssClass = "table-primary-header";
            mainHeaderRow.Cells.Add(componentCell);

            // Get drivers list
            List<string> drivers = new List<string>();
            if (Session["filterValue"] != null)
            {
                string filtervalue = Session["filterValue"].ToString();
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });
                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
            }

            // Add driver header cells (each spans 2 columns)
            foreach (string driver in drivers)
            {
                TableHeaderCell driverCell = new TableHeaderCell();
                driverCell.Text = driver;
                driverCell.ColumnSpan = 2;
                driverCell.HorizontalAlign = HorizontalAlign.Center;
                driverCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#2a62c2");
                driverCell.ForeColor = System.Drawing.Color.White;
                driverCell.Font.Bold = true;
                driverCell.CssClass = "table-primary-header";
                mainHeaderRow.Cells.Add(driverCell);
            }

            // Remove the component cell from existing header (since it's now in main header with rowspan)
            existingHeader.Cells.RemoveAt(0);

            // Fix: Cast to System.Web.UI.WebControls.Table explicitly
            System.Web.UI.WebControls.Table table = (System.Web.UI.WebControls.Table)GridView_cmf_summary.Controls[0];
            table.Rows.AddAt(0, mainHeaderRow);
        }
    }
    private void BindGridView_cmf_pending()
    {
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_CMF_ASK";
        string allComponentsTable = basePlatform + "_ALL_COMPONENTS_TABLE"; // Define the ALL_COMPONENTS_TABLE
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            // Modify the query to join the designTable with the ALL_COMPONENTS_TABLE
            string mappingTable = selectedPlatform.IndexOf("NVL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_NVL"
                : selectedPlatform.IndexOf("PTL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_PTL"
                : selectedPlatform.IndexOf("WCL", StringComparison.OrdinalIgnoreCase) >= 0 ? "idst_mapping_table_WCL"
                : "idst_mapping_table";

            string main_query = @"
            SELECT 
                d.cp_id, 
                d.title, 
                d.component, 
                CASE 
                    WHEN d.component_group IS NULL OR d.component_group = '' OR d.component_group = 'no iDST assigned' 
                    THEN 'Unassigned' 
                    ELSE d.component_group 
                END AS component_group,
                d.reproducibility,
                a.repro_on_rvp, 
                d.customer_owner,
                a.idst,
                ISNULL(M.idst_display_name, a.idst) AS idst_display_name, 
                d.customer_detail, 
                d.date_cmf_ask, 
                d.cmf_request,
                CASE 
                    WHEN d.cmf_request = 'cmf_incomplete' THEN ISNULL(NULLIF(d.why_cmf_change, ''), 'NA')
                    ELSE 'NA'
                END AS why_cmf_change,
                CASE
                    WHEN d.cmf_request = 'cmf_ask' THEN 'iDST'
                    WHEN d.cmf_request = 'cmf_incomplete' THEN 'CE'
                    WHEN d.cmf_request = 'cmf_ok' AND ISNULL(a.sysdebug, '') NOT LIKE '%customer_must_fix%' THEN 'CMF CHAIR'
                    ELSE 'NA'
                END AS pending_reason,
                d.impact 
            FROM " + designTable + @" AS d
            LEFT JOIN (
                SELECT 
                    cp_id,
                    MAX(repro_on_rvp) AS repro_on_rvp,
                    MAX(idst) AS idst,
                    MAX(sysdebug) AS sysdebug,
                    MAX(component_group) AS component_group
                FROM " + allComponentsTable + @"
                GROUP BY cp_id
            ) AS a ON d.cp_id = a.cp_id
            LEFT JOIN [gfxitt_db].[dbo].[" + mappingTable + @"] M ON a.component_group = M.component
            WHERE d.status NOT IN ('complete', 'rejected')
            ORDER BY d.date_cmf_ask DESC";

            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);

                    // Collect DISTINCT aliases for owner processing
                    HashSet<string> aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DataRow r in dt.Rows)
                    {
                        if (r["customer_owner"] == DBNull.Value) continue;
                        string raw = r["customer_owner"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw)) continue;

                        if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                        if (raw.Contains("@")) raw = raw.Split('@')[0];
                        raw = raw.Trim();

                        if (!string.IsNullOrEmpty(raw))
                            aliasSet.Add(raw);
                    }

                    // Create AD contexts
                    PrincipalContext context_gar = null;
                    PrincipalContext context_amr = null;
                    PrincipalContext context_ccr = null;
                    PrincipalContext context_ger = null;

                    try { context_gar = new PrincipalContext(ContextType.Domain, "gar.corp.intel.com"); } catch { }
                    try { context_amr = new PrincipalContext(ContextType.Domain, "amr.corp.intel.com"); } catch { }
                    try { context_ccr = new PrincipalContext(ContextType.Domain, "ccr.corp.intel.com"); } catch { }
                    try { context_ger = new PrincipalContext(ContextType.Domain, "ger.corp.intel.com"); } catch { }

                    Dictionary<string, string> ownerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (string alias in aliasSet)
                    {
                        string displayName = "Unable to fetch";
                        UserPrincipal up = null;

                        if (context_gar != null) up = UserPrincipal.FindByIdentity(context_gar, IdentityType.SamAccountName, alias);
                        if (up == null && context_amr != null) up = UserPrincipal.FindByIdentity(context_amr, IdentityType.SamAccountName, alias);
                        if (up == null && context_ccr != null) up = UserPrincipal.FindByIdentity(context_ccr, IdentityType.SamAccountName, alias);
                        if (up == null && context_ger != null) up = UserPrincipal.FindByIdentity(context_ger, IdentityType.SamAccountName, alias);

                        if (up != null) displayName = up.DisplayName;

                        ownerMap[alias] = displayName;
                    }

                    // Process data rows to replace customer_owner with display names
                    foreach (DataRow row in dt.Rows)
                    {
                        string raw = (row["customer_owner"] == DBNull.Value) ? string.Empty : row["customer_owner"].ToString().Trim();
                        if (string.IsNullOrEmpty(raw))
                        {
                            row["customer_owner"] = "Unable to fetch";
                        }
                        else
                        {
                            if (raw.Contains("\\")) raw = raw.Substring(raw.LastIndexOf('\\') + 1);
                            if (raw.Contains("@")) raw = raw.Split('@')[0];
                            raw = raw.Trim();

                            string actualName;
                            if (ownerMap.TryGetValue(raw, out actualName))
                                row["customer_owner"] = actualName;
                            else
                                row["customer_owner"] = "Unable to fetch";
                        }
                    }

                    GridView_cmf_pending.DataSource = dt;
                    GridView_cmf_pending.DataBind();
                }
            }
        }
    }

    private void BindGridView_design_summary(string filtervalue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";

        string main_query = @"SELECT
                                CASE 
                                    WHEN A.customer_detail IS NULL OR A.customer_detail = '' THEN 'unassigned'
                                    ELSE A.customer_detail
                                END AS Design,  
                                B.sw_image_freeze AS SWImageFreeze, 
                                 
                                (SELECT COUNT(cp_id)
                                 FROM " + selectedPlatform + @"
                                 WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND customer_detail = A.customer_detail AND status not in ('complete','rejected') ) AS Issues_in_CMF_ASK,
                                (SELECT COUNT(cp_id)
                                 FROM " + selectedPlatform + @"
                                 WHERE cmf_request IN('cmf_reject') AND customer_detail = A.customer_detail) AS Total_CMF_REJECT,
                                (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0)
                                 FROM " + selectedPlatform + @"
                                 WHERE sysdebug LIKE('%customer_must_fix%') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail) AS crit_tpt,
                                (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_detail = A.customer_detail) AS disp_tpt, 
                                (SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_detail = A.customer_detail) AS resolve_tpt,
                                (CASE WHEN(SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail AND sysdebug LIKE('%customer_must_fix%')) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail AND sysdebug LIKE('%customer_must_fix%'))
                                          * 100.0, 1) AS VARCHAR) + '%' +' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_detail = A.customer_detail) AS VARCHAR) +'/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_detail = A.customer_detail AND sysdebug LIKE('%customer_must_fix%')) AS VARCHAR) +')'
                                END) AS CMFOpenPercentage,
                                (SELECT COUNT(cp_id)
                                 FROM " + selectedPlatform + @"
                                 WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND customer_detail = A.customer_detail) AS Total_CMF_Approved,
                                (CASE WHEN(SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_detail = A.customer_detail) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND(closed_reason NOT LIKE('%internal%')
                                           AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                           AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND customer_detail = A.customer_detail)
                                          * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status IN('complete', 'rejected')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND(closed_reason NOT LIKE('%internal%')
                                                AND sysdebug LIKE('%customer_must_fix%')
                                                AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +'/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +')'
                                END) AS Noise,
                                (CASE WHEN(SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_detail = A.customer_detail) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND(closed_reason LIKE('%internal%')
                                           OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                           AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND customer_detail = A.customer_detail)
                                          * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND(closed_reason LIKE('%internal%')
                                                OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +'/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND customer_detail = A.customer_detail) AS VARCHAR) +')'
                                END) AS IntelIssuePercentage,
                                (CASE WHEN (SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_detail = A.customer_detail) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                           AND customer_detail = A.customer_detail)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                           AND status IN('complete', 'rejected')
                                           AND customer_detail = A.customer_detail)
                                          * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status IN('complete', 'rejected')
                                                AND sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                                AND customer_detail = A.customer_detail) AS VARCHAR) + '/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                                AND status IN('complete', 'rejected')
                                                AND customer_detail = A.customer_detail) AS VARCHAR) + ')'
                                END) AS ThirdPartyPercentage,
                                (CASE WHEN (SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND customer_detail = A.customer_detail) = 0
                                THEN 'N/A'
                                ELSE CAST(ROUND(
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                    AND customer_detail = A.customer_detail)
                                    /
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND customer_detail = A.customer_detail)
                                    * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE status IN('complete', 'rejected')
                                        AND sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                        AND customer_detail = A.customer_detail) AS VARCHAR) + '/' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_detail = A.customer_detail) AS VARCHAR) + ')'
                        END) AS CustomerIssuePercentage
                            FROM
                                " + selectedPlatform + @" A
                            INNER JOIN
                                " + designTable + @" B ON A.customer_detail = B.customer_detail
                            GROUP BY
                                A.customer_detail, B.sw_image_freeze
                            ORDER BY
                                Total_CMF_Approved desc";

        using (SqlConnection con = new SqlConnection(connectionString))
        {

            using (SqlCommand cmd = new SqlCommand(main_query, con))

            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {

                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);

                    GridView_design_summary.DataSource = dt;
                    GridView_design_summary.DataBind();
                }
            }

        }


    }


    private void BindModalGrid(string design)
    {
        string raw_design = "";
        string trigger = "";
        string modalquery = "";
        string cmf_req = "";

        int lastUnderscoreIndex = design.LastIndexOf('_');

        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < design.Length - 1)
        {
            raw_design = design.Substring(0, lastUnderscoreIndex);
            trigger = design.Substring(lastUnderscoreIndex + 1);
        }

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        if (raw_design == "unassigned" || raw_design == "")
        {
            raw_design = "";
        }

        // Original logic for triggers 1, 2, and 3
        if (trigger == "trg1")
        {
            cmf_req = "'cmf_ok','cmf_duplicate'"; //trg1->Total_CMF_Approved
        }
        else if (trigger == "trg2")
        {
            cmf_req = "'cmf_ask','cmf_incomplete'"; //trg2 -> Issues_in_CMF_ASK
        }
        else
        {
            cmf_req = "'cmf_reject'"; //trg3 -> Total_CMF_REJECT
        }

        //if (trigger != "trg2")
        //{
        //    modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE ((CMF_REQUEST in (" + cmf_req + ") and [sysdebug] like ('%customer_must_fix%')) OR ((CMF_REQUEST in (" + cmf_req + ") AND CMF_REQUEST NOT IN ('cmf_ok','cmf_duplicate')))) and customer_detail in ('" + raw_design + "')";
        //}
        if (trigger != "trg2")
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id],[closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE CMF_REQUEST in (" + cmf_req + ") and customer_detail in ('" + raw_design + "')";
        }
        else
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id],[closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE CMF_REQUEST in (" + cmf_req + ") and status not in ('complete','rejected') and customer_detail in ('" + raw_design + "')";
        }

        // Additional logic for triggers 4 onwards
        switch (trigger)
        {
            case "trg4": // CMFOpenPercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected') AND customer_detail = '" + raw_design + "'";
                break;
            case "trg5": // Noise
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) AND customer_detail = '" + raw_design + "'";
                break;
            case "trg6": // IntelIssuePercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) AND customer_detail = '" + raw_design + "'";
                break;
            case "trg7": // ThirdPartyPercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')) AND customer_detail = '" + raw_design + "'";
                break;
            case "trg8": // CustomerIssuePercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')) AND customer_detail = '" + raw_design + "'";
                break;
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView_design_summary_modal.DataSource = dt;
            GridView_design_summary_modal.DataBind();
        }
    }

    private DataTable BindModalGridExcel(string design)
    {
        string raw_design = "";
        string trigger = "";
        string modalquery = "";
        string cmf_req = "";

        int lastUnderscoreIndex = design.LastIndexOf('_');

        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < design.Length - 1)
        {
            raw_design = design.Substring(0, lastUnderscoreIndex);
            trigger = design.Substring(lastUnderscoreIndex + 1);
        }

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        if (raw_design == "unassigned" || raw_design == "")
        {
            raw_design = "";
        }

        // Original logic for triggers 1, 2, and 3
        if (trigger == "trg1")
        {
            cmf_req = "'cmf_ok','cmf_duplicate'"; //trg1->Total_CMF_Approved
        }
        else if (trigger == "trg2")
        {
            cmf_req = "'cmf_ask','cmf_incomplete'"; //trg2 -> Issues_in_CMF_ASK
        }
        else
        {
            cmf_req = "'cmf_reject'"; //trg3 -> Total_CMF_REJECT
        }

        //if (trigger != "trg2")
        //{
        //    modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE ((CMF_REQUEST in (" + cmf_req + ") and [sysdebug] like ('%customer_must_fix%')) OR ((CMF_REQUEST in (" + cmf_req + ") AND CMF_REQUEST NOT IN ('cmf_ok','cmf_duplicate')))) and customer_detail in ('" + raw_design + "')";
        //}
        if (trigger != "trg2")
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id],[closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE CMF_REQUEST in (" + cmf_req + ") and customer_detail in ('" + raw_design + "')";
        }
        else
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id],[closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE CMF_REQUEST in (" + cmf_req + ") and status not in ('complete','rejected') and customer_detail in ('" + raw_design + "')";
        }

        // Additional logic for triggers 4 onwards
        switch (trigger)
        {
            case "trg4": // CMFOpenPercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected') AND customer_detail = '" + raw_design + "'";
                break;
            case "trg5": // Noise
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) AND customer_detail = '" + raw_design + "'";
                break;
            case "trg6": // IntelIssuePercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) AND customer_detail = '" + raw_design + "'";
                break;
            case "trg7": // ThirdPartyPercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')) AND customer_detail = '" + raw_design + "'";
                break;
            case "trg8": // CustomerIssuePercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')) AND customer_detail = '" + raw_design + "'";
                break;
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
    private void BindModalGrid2(string ingred)
    {
        string raw_ingred = "";
        string trigger = "";
        string modalquery = "";
        string cmf_req = "";

        int lastUnderscoreIndex = ingred.LastIndexOf('_');

        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < ingred.Length - 1)
        {
            raw_ingred = ingred.Substring(0, lastUnderscoreIndex);
            trigger = ingred.Substring(lastUnderscoreIndex + 1);


        }


        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        if (raw_ingred == "unassigned" || raw_ingred == "")
        {
            raw_ingred = "";
        }

        if (trigger == "trg1") // for Ingredient Indicator under Total CMF_Approved column kept only cmf_ok trg1 -> Total_CMF_Approved
        {
            cmf_req = "'cmf_ok'";
        }
        else if (trigger == "trg2") //trg2 -> Issues_in_CMF_ASK
        {
            cmf_req = "'cmf_ask','cmf_incomplete'";
        }
        else
        {
            cmf_req = "'cmf_reject'"; //trg3 -> Total_CMF_REJECT
        }
        //AND [status] not in ('complete','rejected')

        if (trigger != "trg2")
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE ((CMF_REQUEST in (" + cmf_req + ") and [sysdebug] like ('%customer_must_fix%')) OR ((CMF_REQUEST in (" + cmf_req + ") AND CMF_REQUEST NOT IN ('cmf_ok','cmf_duplicate')))) and component_group in ('" + raw_ingred + "')";
        }
        else
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE CMF_REQUEST in (" + cmf_req + ")  and component_group in ('" + raw_ingred + "')";
        }

        switch (trigger)
        {
            case "trg4": // CMFOpenPercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected') AND component_group = '" + raw_ingred + "'";
                break;
            case "trg5": // Noise
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) AND component_group = '" + raw_ingred + "'";
                break;
            case "trg6": // IntelIssuePercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) AND component_group = '" + raw_ingred + "'";
                break;
            case "trg7": // ThirdPartyPercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')) AND component_group = '" + raw_ingred + "'";
                break;
            case "trg8": // CustomerIssuePercentage
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')) AND component_group = '" + raw_ingred + "'";
                break;
        }


        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView_component_summary_modal.DataSource = dt;
            GridView_component_summary_modal.DataBind();
        }
    }

    private void BindCMFIssues(string component, string driver, string issueType)
    {
        lblDriverName1.Text = Server.HtmlEncode(component + " - " + driver + " (" + issueType + ")");

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string tableName = selectedPlatform;



        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";

        if (!IsSafeTableName(tableName))
        {
            GridView_driver_issues.DataSource = null;
            GridView_driver_issues.DataBind();
            return;
        }

        string sql = "";

        if (issueType == "Open")
        {
            if (component == "Total (LOS) + Duplicates + Implemented")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + tableName + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND " + SummaryDriverSql("drivers") + @" = @Driver
  AND status IN ('open')
  AND cmf_request != 'cmf_reject'
  AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, COALESCE(B.component_group, A.component_group) as component
FROM " + tableName + @" A
INNER JOIN " + componentTable + @" B ON A.component_group = B.component_group
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND " + SummaryDriverSql("A.drivers") + @" = @Driver
  AND COALESCE(B.component_group, A.component_group) = @Component
  AND A.status IN ('open')
  AND A.cmf_request != 'cmf_duplicate'
  AND A.cmf_request != 'cmf_reject'
  AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
            }
        }
        else if (issueType == "Implemented")
        {
            if (component == "Total (LOS) + Duplicates + Implemented")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + tableName + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND " + SummaryDriverSql("drivers") + @" = @Driver
  AND cmf_request = 'cmf_ok'
  AND sysdebug LIKE '%customer_must_fix%'
  AND status IN ('implemented','verified','future')
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, COALESCE(B.component_group, A.component_group) as component
FROM " + tableName + @" A
INNER JOIN " + componentTable + @" B ON A.component_group = B.component_group
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND " + SummaryDriverSql("A.drivers") + @" = @Driver
  AND COALESCE(B.component_group, A.component_group) = @Component
  AND A.cmf_request = 'cmf_ok'
  AND A.sysdebug LIKE '%customer_must_fix%'
  AND A.status IN ('implemented','verified','future')
ORDER BY A.cmf_request desc";
            }
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            object driverParam = (driver != null) ? (object)NormalizeSummaryDriver(driver) : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            if (component != "Total (LOS) + Duplicates + Implemented")
            {
                object componentParam = (component != null) ? (object)component.Trim() : (object)DBNull.Value;
                cmd.Parameters.Add("@Component", SqlDbType.NVarChar, 256).Value = componentParam;
            }

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                GridView_cmf_issues.DataSource = dt;
                GridView_cmf_issues.DataBind();
            }
        }

        // Show modal using your existing modal
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowCMFIssuesModal",
            "$('#detailsModalDrivers1').modal('show');", true);
    }

    private void ShowCMFIssues()
    {
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowCMFIssuesModal",
            "$('#detailsModalDrivers1').modal('show');", true);
    }

    private void ExportCMFIssues(string component, string driver, string issueType)
    {
        try
        {
            DataTable dt = GetCMFIssuesData(component, driver, issueType);

            Response.Clear();
            Response.Buffer = true;

            string safeComponent = (component ?? "").Replace("\"", "");
            string safeDriver = (driver ?? "").Replace("\"", "");
            string fileName = "CMF_Issues_" + safeComponent + "_" + safeDriver + "_" + issueType + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                GridView gv = new GridView();
                gv.AutoGenerateColumns = false;
                gv.EnableViewState = false;

                HyperLinkField linkField = new HyperLinkField();
                linkField.DataTextField = "cp_id";
                linkField.HeaderText = "Sighting ID";
                linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                linkField.Target = "_blank";
                gv.Columns.Add(linkField);

                BoundField bfTitle = new BoundField();
                bfTitle.DataField = "title";
                bfTitle.HeaderText = "Title";
                gv.Columns.Add(bfTitle);

                BoundField bfCmfReq = new BoundField();
                bfCmfReq.DataField = "cmf_request";
                bfCmfReq.HeaderText = "CMF Request";
                gv.Columns.Add(bfCmfReq);

                BoundField bfLOS = new BoundField();
                bfLOS.DataField = "los";
                bfLOS.HeaderText = "LOS";
                gv.Columns.Add(bfLOS);

                BoundField bfComponent = new BoundField();
                bfComponent.DataField = "component";
                bfComponent.HeaderText = "Component";
                gv.Columns.Add(bfComponent);

                gv.DataSource = dt;
                gv.DataBind();
                gv.RenderControl(hw);

                Response.ContentEncoding = System.Text.Encoding.Unicode;
                Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during CMF export: " + ex.Message);
        }
    }


    private DataTable GetCMFIssuesData(string component, string driver, string issueType)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string tableName = selectedPlatform;
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";

        // Use the same SQL logic as in BindCMFIssues
        string sql = "";

        if (issueType == "Open")
        {
            if (component == "Total (LOS) + Duplicates + Implemented")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + tableName + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND " + SummaryDriverSql("drivers") + @" = @Driver
  AND status IN ('open')
  AND cmf_request != 'cmf_reject'
  AND sysdebug LIKE '%customer_must_fix%'
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, COALESCE(B.component_group, A.component_group) as component
FROM " + tableName + @" A
INNER JOIN " + componentTable + @" B ON A.component_group = B.component_group
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND " + SummaryDriverSql("A.drivers") + @" = @Driver
  AND COALESCE(B.component_group, A.component_group) = @Component
  AND A.status IN ('open')
  AND A.cmf_request != 'cmf_duplicate'
  AND A.cmf_request != 'cmf_reject'
  AND A.sysdebug LIKE '%customer_must_fix%'
ORDER BY A.cmf_request desc";
            }
        }
        else if (issueType == "Implemented")
        {
            if (component == "Total (LOS) + Duplicates + Implemented")
            {
                sql = @"
SELECT cp_id, title, cmf_request, ISNULL(los, 'No') as los, component_group as component
FROM " + tableName + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND " + SummaryDriverSql("drivers") + @" = @Driver
  AND cmf_request = 'cmf_ok'
  AND sysdebug LIKE '%customer_must_fix%'
  AND status IN ('implemented','verified','future')
ORDER BY cmf_request desc";
            }
            else
            {
                sql = @"
SELECT A.cp_id, A.title, A.cmf_request, ISNULL(A.los, 'No') as los, COALESCE(B.component_group, A.component_group) as component
FROM " + tableName + @" A
INNER JOIN " + componentTable + @" B ON A.component_group = B.component_group
WHERE ISNULL(LTRIM(RTRIM(A.drivers)), '') <> ''
  AND " + SummaryDriverSql("A.drivers") + @" = @Driver
  AND COALESCE(B.component_group, A.component_group) = @Component
  AND A.cmf_request = 'cmf_ok'
  AND A.sysdebug LIKE '%customer_must_fix%'
  AND A.status IN ('implemented','verified','future')
ORDER BY A.cmf_request desc";
            }
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            object driverParam = (driver != null) ? (object)NormalizeSummaryDriver(driver) : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            if (component != "Total (LOS) + Duplicates + Implemented")
            {
                object componentParam = (component != null) ? (object)component.Trim() : (object)DBNull.Value;
                cmd.Parameters.Add("@Component", SqlDbType.NVarChar, 256).Value = componentParam;
            }

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
    private void BindDriverIssues(string driver)
    {
        lblDriverName.Text = Server.HtmlEncode(driver);

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string tableName = selectedPlatform; // e.g., "CMF_PTL_ALL_COMPONENTS_TABLE"

        if (!IsSafeTableName(tableName))
        {
            GridView_driver_issues.DataSource = null;
            GridView_driver_issues.DataBind();
            return;
        }

        string sql = @"
SELECT cp_id, title, component
FROM " + tableName + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND cmf_request IN ('cmf_ok')
  AND sysdebug LIKE '%customer_must_fix%'
  AND LTRIM(RTRIM(drivers)) = @Driver
ORDER BY cp_id";

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            object driverParam = (driver != null) ? (object)driver.Trim() : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                GridView_driver_issues.DataSource = dt;
                GridView_driver_issues.DataBind();
            }
        }

        // Show modal
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowDriverIssuesModal",
            "$('#detailsModalDrivers').modal('show');", true);
    }

    private static bool IsSafeTableName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z0-9_]+$");
    }

    // Show the modal (Bootstrap)
    private void ShowDriverIssues()
    {
        ScriptManager.RegisterStartupScript(this, GetType(),
            "ShowDriverIssuesModal",
            "$('#detailsModalDrivers').modal('show');", true);
    }

    private DataTable GetDriverIssuesData(string driver)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string tableName = selectedPlatform;

        if (!IsSafeTableName(tableName))
            return new DataTable();

        string sql = @"
SELECT cp_id, title, component
FROM " + tableName + @"
WHERE ISNULL(LTRIM(RTRIM(drivers)), '') <> ''
  AND cmf_request IN ('cmf_ok')
  AND sysdebug LIKE '%customer_must_fix%'
  AND LTRIM(RTRIM(drivers)) = @Driver
ORDER BY cp_id";

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            object driverParam = (driver != null) ? (object)driver.Trim() : (object)DBNull.Value;
            cmd.Parameters.Add("@Driver", SqlDbType.NVarChar, 256).Value = driverParam;

            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    private void ExportDriverIssues(string driver)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export Driver Issues started.");

            DataTable dt = GetDriverIssuesData(driver);

            Response.Clear();
            Response.Buffer = true;

            string safeDriver = (driver ?? "").Replace("\"", "");
            string fileName = "Driver_Issues_" + safeDriver + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                GridView gv = new GridView();
                gv.AutoGenerateColumns = false;
                gv.EnableViewState = false;

                HyperLinkField linkField = new HyperLinkField();
                linkField.DataTextField = "cp_id";
                linkField.HeaderText = "Sighting ID";
                linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                linkField.Target = "_blank";
                gv.Columns.Add(linkField);

                BoundField bfTitle = new BoundField();
                bfTitle.DataField = "title";
                bfTitle.HeaderText = "Title";
                gv.Columns.Add(bfTitle);

                // ADD THIS: Component column
                BoundField bfComponent = new BoundField();
                bfComponent.DataField = "component";
                bfComponent.HeaderText = "Component";
                gv.Columns.Add(bfComponent);

                gv.DataSource = dt;
                gv.DataBind();
                gv.RenderControl(hw);

                Response.ContentEncoding = System.Text.Encoding.Unicode;
                Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End(); // or use CompleteRequest() to avoid ThreadAbortException
            }

            System.Diagnostics.Debug.WriteLine("Export Driver Issues completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during driver export: " + ex.Message);
        }
    }

    private DataTable BindModalGridExcel2(string ingred)
    {
        string raw_ingred = "";
        string trigger = "";
        string modalquery = "";
        string cmf_req = "";

        int lastUnderscoreIndex = ingred.LastIndexOf('_');

        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < ingred.Length - 1)
        {
            raw_ingred = ingred.Substring(0, lastUnderscoreIndex);
            trigger = ingred.Substring(lastUnderscoreIndex + 1);
        }

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        if (raw_ingred == "unassigned" || raw_ingred == "")
        {
            raw_ingred = "";
        }

        if (trigger == "trg1")
        {
            cmf_req = "'cmf_ok'";
        }
        else if (trigger == "trg2")
        {
            cmf_req = "'cmf_ask','cmf_incomplete'";
        }
        else
        {
            cmf_req = "'cmf_reject'";
        }

        if (trigger != "trg2")
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE ((CMF_REQUEST in (" + cmf_req + ") and [sysdebug] like ('%customer_must_fix%')) OR ((CMF_REQUEST in (" + cmf_req + ") AND CMF_REQUEST NOT IN ('cmf_ok','cmf_duplicate')))) and component_group in ('" + raw_ingred + "')";
        }
        else
        {
            modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE CMF_REQUEST in (" + cmf_req + ")  and component_group in ('" + raw_ingred + "')";
        }

        switch (trigger)
        {
            case "trg4":
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected') AND component_group = '" + raw_ingred + "'";
                break;
            case "trg5":
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) AND component_group = '" + raw_ingred + "'";
                break;
            case "trg6":
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) AND component_group = '" + raw_ingred + "'";
                break;
            case "trg7":
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')) AND component_group = '" + raw_ingred + "'";
                break;
            case "trg8":
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform + " WHERE cmf_request IN ('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')) AND component_group = '" + raw_ingred + "'";
                break;
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    private void BindModalGrid3(string oem)
    {
        string raw_oem = "";
        string trigger = "";
        string modalquery = "";
        string cmf_req = "";

        int lastUnderscoreIndex = oem.LastIndexOf('_');

        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < oem.Length - 1)
        {
            raw_oem = oem.Substring(0, lastUnderscoreIndex);
            trigger = oem.Substring(lastUnderscoreIndex + 1);
        }

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        // Handle empty/unassigned OEM consistently
        string oemCondition = "";
        if (raw_oem == "unassigned" || raw_oem == "")
        {
            oemCondition = "(customer_company IS NULL OR customer_company = '')";
        }
        else
        {
            oemCondition = "customer_company = '" + raw_oem + "'";
        }

        switch (trigger)
        {
            case "trg1": // Total_CMF_Approved - FIXED to include both cmf_ok and cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND " + oemCondition;
                break;

            case "trg2": // Issues_in_CMF_ASK
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND status NOT IN ('complete','rejected') AND " + oemCondition;
                break;

            case "trg3": // Total_CMF_REJECT
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_reject') AND " + oemCondition;
                break;

            case "trg4": // CMFOpenPercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected') AND " + oemCondition;
                break;

            case "trg5": // Noise - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) " +
                            "AND " + oemCondition;
                break;

            case "trg6": // IntelIssuePercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) " +
                            "AND " + oemCondition;
                break;

            case "trg7": // ThirdPartyPercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')) " +
                            "AND " + oemCondition;
                break;

            case "trg8": // CustomerIssuePercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')) " +
                            "AND " + oemCondition;
                break;

            default:
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE 1=0"; // Return no results for unknown triggers
                break;
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView_oem_summary_modal7.DataSource = dt;
            GridView_oem_summary_modal7.DataBind();
        }
    }

    private DataTable BindModalGridExcel3(string oem)
    {
        string raw_oem = "";
        string trigger = "";
        string modalquery = "";

        int lastUnderscoreIndex = oem.LastIndexOf('_');

        if (lastUnderscoreIndex != -1 && lastUnderscoreIndex < oem.Length - 1)
        {
            raw_oem = oem.Substring(0, lastUnderscoreIndex);
            trigger = oem.Substring(lastUnderscoreIndex + 1);
        }

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        // Handle empty/unassigned OEM consistently
        string oemCondition = "";
        if (raw_oem == "unassigned" || raw_oem == "")
        {
            oemCondition = "(customer_company IS NULL OR customer_company = '')";
        }
        else
        {
            oemCondition = "customer_company = '" + raw_oem + "'";
        }

        switch (trigger)
        {
            case "trg1": // Total_CMF_Approved - FIXED to include both cmf_ok and cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND " + oemCondition;
                break;

            case "trg2": // Issues_in_CMF_ASK
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND status NOT IN ('complete','rejected') AND " + oemCondition;
                break;

            case "trg3": // Total_CMF_REJECT
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_reject') AND " + oemCondition;
                break;

            case "trg4": // CMFOpenPercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status NOT IN('complete', 'rejected') AND " + oemCondition;
                break;

            case "trg5": // Noise - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason NOT LIKE('%internal%') AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env')) " +
                            "AND " + oemCondition;
                break;

            case "trg6": // IntelIssuePercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason LIKE('%internal%') OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb')) " +
                            "AND " + oemCondition;
                break;

            case "trg7": // ThirdPartyPercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue')) " +
                            "AND " + oemCondition;
                break;

            case "trg8": // CustomerIssuePercentage - FIXED to include cmf_duplicate
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND status IN('complete', 'rejected') " +
                            "AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug')) " +
                            "AND " + oemCondition;
                break;

            default:
                modalquery = "SELECT TOP 1000 [cp_id], [title], [status], [component], [cmf_request], [customer_owner], [customer_company], [promoted_id], [closed_reason], [days_active], [idst], [drivers] FROM " + selectedPlatform +
                            " WHERE 1=0"; // Return no results for unknown triggers
                break;
        }

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(modalquery, con))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        {
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
    private void ExportToExcel(string design)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export to Excel started.");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=IssueList.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Get the data from BindModalGrid2 method
                    DataTable dt = BindModalGridExcel(design);

                    // Bind data to a temporary GridView
                    GridView gridView = new GridView();
                    gridView.DataSource = dt;
                    gridView.AutoGenerateColumns = false;

                    // Add HyperLinkField for cp_id
                    HyperLinkField linkField = new HyperLinkField();
                    linkField.DataTextField = "cp_id";
                    linkField.HeaderText = "Sighting ID";
                    linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                    linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                    linkField.Target = "_blank";
                    gridView.Columns.Add(linkField);

                    // Add remaining BoundFields
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "cp_id")
                        {
                            BoundField bf = new BoundField();
                            bf.DataField = col.ColumnName;
                            bf.HeaderText = col.ColumnName;
                            gridView.Columns.Add(bf);
                        }
                    }

                    gridView.DataBind();
                    gridView.EnableViewState = false;

                    // Render the GridView to HTML
                    gridView.RenderControl(hw);

                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            System.Diagnostics.Debug.WriteLine("Export to Excel completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during export: " + ex.Message);
        }
        finally
        {
            Session["DesignValue"] = null;
        }
    }

    private void ExportToExcel2(string design)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export to Excel started.");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=IssueList.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Get the data from BindModalGrid2 method
                    DataTable dt = BindModalGridExcel2(design);

                    // Bind data to a temporary GridView
                    GridView gridView = new GridView();
                    gridView.DataSource = dt;
                    gridView.AutoGenerateColumns = false;

                    // Add HyperLinkField for cp_id
                    HyperLinkField linkField = new HyperLinkField();
                    linkField.DataTextField = "cp_id";
                    linkField.HeaderText = "Sighting ID";
                    linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                    linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                    linkField.Target = "_blank";
                    gridView.Columns.Add(linkField);

                    // Add remaining BoundFields
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "cp_id")
                        {
                            BoundField bf = new BoundField();
                            bf.DataField = col.ColumnName;
                            bf.HeaderText = col.ColumnName;
                            gridView.Columns.Add(bf);
                        }
                    }

                    gridView.DataBind();
                    gridView.EnableViewState = false;

                    // Render the GridView to HTML
                    gridView.RenderControl(hw);

                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            System.Diagnostics.Debug.WriteLine("Export to Excel completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during export: " + ex.Message);
        }
        finally
        {
            Session["DesignValue"] = null;
        }
    }

    private void ExportToExcel3(string design)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Export to Excel started.");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=IssueList.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    // Get the data from BindModalGrid2 method
                    DataTable dt = BindModalGridExcel3(design);

                    // Bind data to a temporary GridView
                    GridView gridView = new GridView();
                    gridView.DataSource = dt;
                    gridView.AutoGenerateColumns = false;

                    // Add HyperLinkField for cp_id
                    HyperLinkField linkField = new HyperLinkField();
                    linkField.DataTextField = "cp_id";
                    linkField.HeaderText = "Sighting ID";
                    linkField.DataNavigateUrlFields = new string[] { "cp_id" };
                    linkField.DataNavigateUrlFormatString = "https://hsdes.intel.com/appstore/article/#/{0}";
                    linkField.Target = "_blank";
                    gridView.Columns.Add(linkField);

                    // Add remaining BoundFields
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "cp_id")
                        {
                            BoundField bf = new BoundField();
                            bf.DataField = col.ColumnName;
                            bf.HeaderText = col.ColumnName;
                            gridView.Columns.Add(bf);
                        }
                    }

                    gridView.DataBind();
                    gridView.EnableViewState = false;

                    // Render the GridView to HTML
                    gridView.RenderControl(hw);

                    Response.ContentEncoding = System.Text.Encoding.Unicode;
                    Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
            }

            System.Diagnostics.Debug.WriteLine("Export to Excel completed successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during export: " + ex.Message);
        }
        finally
        {
            Session["DesignValue"] = null;
        }
    }
    protected void GridViewdesign_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        string designfilter = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column index for "Total CMF_Approved"
            int colIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "Total CMF_Approved")
                {
                    colIndex = i;
                    break;
                }
            }

            if (colIndex != -1)
            {
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                designfilter = design;

                //string link = "<a href='Details.aspx?sku=" + Server.UrlEncode(meSku) + "'>" + val + "</a>";

                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";


                e.Row.Cells[colIndex].Text = link;
            }
        }

    }

    protected void GridView5_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string trigger = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column indices for both red and yellow highlighted columns
            int colIndexApproved = -1;
            int colIndexASK = -1;
            int colIndexReject = -1;
            int colIndexCMFOpen = -1;
            int colIndexNoise = -1;
            int colIndexIntelIssue = -1;
            int colIndex3rdParty = -1;
            int colIndexCustomerIssue = -1;
            int colIndexDispTPT = 6;
            int colIndexResolveTPT = 7;
            int colIndexTotalTPT = 5;

            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                string headerText = grid.HeaderRow.Cells[i].Text.Trim();

                if (headerText == "Total CMF_Approved")
                {
                    colIndexApproved = i;
                }
                else if (headerText == "Issues in CMF_ASK")
                {
                    colIndexASK = i;
                }
                else if (headerText == "Total CMF_REJECT")
                {
                    colIndexReject = i;
                }
                else if (headerText == "CMF Open %")
                {
                    colIndexCMFOpen = i;
                }
                else if (headerText == "Noise%")
                {
                    colIndexNoise = i;
                }
                else if (headerText == "Intel Issue %")
                {
                    colIndexIntelIssue = i;
                }
                else if (headerText == "3rd Party %")
                {
                    colIndex3rdParty = i;
                }
                else if (headerText == "Customer Issue %")
                {
                    colIndexCustomerIssue = i;
                }
            }

            // Modify the columns for red highlighted sections
            if (colIndexApproved != -1)
            {
                trigger = "_trg1";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";

                e.Row.Cells[colIndexApproved].Text = link;
            }

            if (colIndexASK != -1)
            {
                trigger = "_trg2";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Issues_in_CMF_ASK");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";

                e.Row.Cells[colIndexASK].Text = link;
            }

            if (colIndexReject != -1)
            {
                trigger = "_trg3";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_REJECT");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + val + "</a>";

                e.Row.Cells[colIndexReject].Text = link;
            }

            // Modify the columns for yellow highlighted sections
            if (colIndexCMFOpen != -1)
            {
                trigger = "_trg4";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CMFOpenPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCMFOpen].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexNoise != -1)
            {
                trigger = "_trg5";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Noise");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexNoise].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexIntelIssue != -1)
            {
                trigger = "_trg6";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "IntelIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexIntelIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndex3rdParty != -1)
            {
                trigger = "_trg7";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "ThirdPartyPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndex3rdParty].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexCustomerIssue != -1)
            {
                trigger = "_trg8";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CustomerIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                string val = rawVal != null ? rawVal.ToString() : "";
                string design = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                design += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + design + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCustomerIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            // Apply conditional styling for CMF Disposition TPT and CMF Resolution TPT
            if (colIndexDispTPT != -1)
            {
                int dispTPT = 0;
                int.TryParse(e.Row.Cells[colIndexDispTPT].Text, out dispTPT);

                if (dispTPT > 2)
                {
                    e.Row.Cells[colIndexDispTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexDispTPT].ForeColor = System.Drawing.Color.Green;
                }
            }

            if (colIndexResolveTPT != -1)
            {
                int resolveTPT = 0;
                int.TryParse(e.Row.Cells[colIndexResolveTPT].Text, out resolveTPT);

                if (resolveTPT > 18)
                {
                    e.Row.Cells[colIndexResolveTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexResolveTPT].ForeColor = System.Drawing.Color.Green;
                }
            }

            if (colIndexTotalTPT != -1)
            {
                int totalTPT = 0;
                int.TryParse(e.Row.Cells[colIndexTotalTPT].Text, out totalTPT);

                if (totalTPT > 21)
                {
                    e.Row.Cells[colIndexTotalTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexTotalTPT].ForeColor = System.Drawing.Color.Green;
                }
            }
        }
    }

    // Helper method to extract the numerator from the ratio string
    private string ExtractNumerator(string ratio)
    {
        if (string.IsNullOrEmpty(ratio))
        {
            return "0";
        }

        // Find the opening parenthesis
        int startIndex = ratio.IndexOf('(');
        if (startIndex == -1)
        {
            return "0";
        }

        // Find the slash
        int slashIndex = ratio.IndexOf('/', startIndex);
        if (slashIndex == -1)
        {
            return "0";
        }

        // Extract the numerator
        string numerator = ratio.Substring(startIndex + 1, slashIndex - startIndex - 1).Trim();
        return numerator;
    }


    private void BindGridView_component_summary(string filtervalue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_COMPONENT_GROUP_TABLE";



        string main_query = @"SELECT CASE
                                    WHEN A.component_group IS NULL OR A.component_group = '' THEN 'unassigned'
                                    ELSE A.component_group
                                END AS Component,                             
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND component_group = A.component_group AND status not in ('complete','rejected')) AS Issues_in_CMF_ASK,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_reject') AND component_group = A.component_group) AS Total_CMF_REJECT,
                            (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0)
                             FROM " + selectedPlatform + @"
                             WHERE sysdebug LIKE('%customer_must_fix%') AND cmf_request IN('cmf_ok') AND component_group = A.component_group) AS crit_tpt,
                            (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0)
                             FROM " + selectedPlatform + @"
                             WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug LIKE('%customer_must_fix%') AND component_group = A.component_group) AS disp_tpt,
                            (SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND component_group = A.component_group) AS resolve_tpt,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE cmf_request IN ('cmf_ok') AND component_group = A.component_group AND sysdebug LIKE('%customer_must_fix%')) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND component_group = A.component_group)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE cmf_request IN('cmf_ok') AND component_group = A.component_group AND sysdebug LIKE('%customer_must_fix%'))
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok') AND sysdebug LIKE('%customer_must_fix%') AND component_group = A.component_group) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE cmf_request IN ('cmf_ok') AND component_group = A.component_group AND sysdebug LIKE('%customer_must_fix%')) AS VARCHAR) +')'
                            END) AS CMFOpenPercentage,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_ok') AND sysdebug Like ('%customer_must_fix%') AND component_group = A.component_group) AS Total_CMF_Approved,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND status IN('complete', 'rejected')
                                        AND component_group = A.component_group) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                       AND status IN('complete', 'rejected')
                                       AND(closed_reason NOT LIKE('%internal%')
                                       AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                       AND component_group = A.component_group)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND status IN('complete', 'rejected')
                                       AND component_group = A.component_group)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status IN('complete', 'rejected')
                                            AND (closed_reason NOT LIKE('%internal%')
                                            AND sysdebug LIKE('%customer_must_fix%')
                                            AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                            AND component_group = A.component_group) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE ('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN ('complete', 'rejected')
                                            AND component_group = A.component_group) AS VARCHAR) +')'
                            END) AS Noise,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND status IN('complete', 'rejected')
                                        AND component_group = A.component_group) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE ('%customer_must_fix%')
                                       AND cmf_request IN ('cmf_ok')
                                       AND status IN ('complete', 'rejected')
                                       AND(closed_reason LIKE ('%internal%')
                                       OR closed_reason IN ('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                       AND component_group = A.component_group)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN ('cmf_ok')
                                       AND status IN('complete', 'rejected')
                                       AND component_group = A.component_group)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE ('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN ('complete', 'rejected')
                                            AND (closed_reason LIKE ('%internal%')
                                            OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                            AND component_group = A.component_group) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN('complete', 'rejected')
                                            AND component_group = A.component_group) AS VARCHAR) +')'
                            END) AS IntelIssuePercentage,
                            (CASE WHEN (SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN ('cmf_ok')
                                            AND status IN('complete', 'rejected')
                                            AND component_group = A.component_group) = 0
                                      THEN 'N/A'
                                      ELSE CAST(ROUND(
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN ('cmf_ok')
                                           AND status IN('complete', 'rejected')
                                           AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                           AND component_group = A.component_group)
                                          /
                                          (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                           FROM " + selectedPlatform + @"
                                           WHERE sysdebug LIKE('%customer_must_fix%')
                                           AND cmf_request IN ('cmf_ok')
                                           AND status IN('complete', 'rejected')
                                           AND component_group = A.component_group)
                                          * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE status IN('complete', 'rejected')
                                                AND cmf_request IN ('cmf_ok')
                                                AND sysdebug LIKE('%customer_must_fix%')
                                                AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                                AND component_group = A.component_group) AS VARCHAR) + '/' +
                                          CAST((SELECT COUNT(cp_id)
                                                FROM " + selectedPlatform + @"
                                                WHERE sysdebug LIKE('%customer_must_fix%')
                                                AND cmf_request IN ('cmf_ok')
                                                AND status IN('complete', 'rejected')
                                                AND component_group = A.component_group) AS VARCHAR) + ')'
                                END) AS ThirdPartyPercentage,
                                (CASE WHEN (SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN ('cmf_ok')
                                    AND status IN('complete', 'rejected')
                                    AND component_group = A.component_group) = 0
                                THEN 'N/A'
                                ELSE CAST(ROUND(
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN ('cmf_ok')
                                    AND status IN('complete', 'rejected')
                                    AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                    AND component_group = A.component_group)
                                    /
                                    (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN ('cmf_ok')
                                    AND status IN('complete', 'rejected')
                                    AND component_group = A.component_group)
                                    * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE status IN('complete', 'rejected')
                                        AND sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                        AND component_group = A.component_group) AS VARCHAR) + '/' +
                                    CAST((SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN ('cmf_ok')
                                        AND status IN('complete', 'rejected')
                                        AND component_group = A.component_group) AS VARCHAR) + ')'
                        END) AS CustomerIssuePercentage
                        FROM
                            " + selectedPlatform + @" A 
                        WHERE cmf_request Not IN ('cmf_duplicate')
                        GROUP BY
                            A.component_group
                        ORDER BY
                            Total_CMF_Approved desc";
        using (SqlConnection con = new SqlConnection(connectionString))
        {

            using (SqlCommand cmd = new SqlCommand(main_query, con))

            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {

                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);

                    GridView_component_summary.DataSource = dt;
                    GridView_component_summary.DataBind();
                }
            }

        }


    }

    // Bind data to OEM Summary GridView
    private void BindGridView_oem_summary(string filtervalue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");

        string main_query = @"SELECT
                            CASE 
                                WHEN A.customer_company IS NULL OR A.customer_company = '' THEN 'unassigned'
                                ELSE A.customer_company
                            END AS OEM,  
                             
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN ('cmf_ask','cmf_incomplete') AND customer_company = A.customer_company AND status not in ('complete','rejected') ) AS Issues_in_CMF_ASK,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_reject') AND customer_company = A.customer_company) AS Total_CMF_REJECT,
                            (SELECT ISNULL(AVG(CAST(days_active AS INT)), 0)
                             FROM " + selectedPlatform + @"
                             WHERE sysdebug LIKE('%customer_must_fix%') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company) AS crit_tpt,
                            (SELECT ISNULL(AVG(CAST(DATEDIFF(DAY, ISNULL(date_cmf_ask, date_cmf_decided), date_cmf_decided) AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(date_cmf_decided AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_company = A.customer_company) AS disp_tpt, 
                            (SELECT ISNULL(AVG(CAST(CASE WHEN TRY_CAST(implemented_date AS DATE) < TRY_CAST(date_cmf_decided AS DATE) THEN 0 ELSE DATEDIFF(DAY, ISNULL(date_cmf_decided, implemented_date), implemented_date) END AS INT)), 0) FROM " + selectedPlatform + @" WHERE TRY_CAST(implemented_date AS DATE) IS NOT NULL AND sysdebug Like ('%customer_must_fix%') AND customer_company = A.customer_company) AS resolve_tpt,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company AND sysdebug LIKE('%customer_must_fix%')) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE cmf_request IN ('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company AND sysdebug LIKE('%customer_must_fix%'))
                                      * 100.0, 1) AS VARCHAR) + '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status NOT IN('complete', 'rejected') AND cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug LIKE('%customer_must_fix%') AND customer_company = A.customer_company) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND customer_company = A.customer_company AND sysdebug LIKE('%customer_must_fix%')) AS VARCHAR) +')'
                            END) AS CMFOpenPercentage,
                            (SELECT COUNT(cp_id)
                             FROM " + selectedPlatform + @"
                             WHERE cmf_request IN('cmf_ok', 'cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND customer_company = A.customer_company) AS Total_CMF_Approved,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_company = A.customer_company) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND(closed_reason NOT LIKE('%internal%')
                                       AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                       AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND customer_company = A.customer_company)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status IN('complete', 'rejected')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND(closed_reason NOT LIKE('%internal%')
                                            AND sysdebug LIKE('%customer_must_fix%')
                                            AND closed_reason IN('below_zbb', 'cannot_reproduce', 'customer_disengaged', 'customer_enquiry_resolved', 'expected_behavior', 'feature_not_por', 'feature_por_not_ready', 'filed_by_mistake', 'inactive', 'known_errata', 'not_a_defect', 'poor_sighting_quality', 'proj_cancelled', 'test/test_env'))
                                            AND customer_company = A.customer_company) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_company = A.customer_company) AS VARCHAR) +')'
                            END) AS Noise,
                            (CASE WHEN(SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_company = A.customer_company) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND(closed_reason LIKE('%internal%')
                                       OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                       AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND customer_company = A.customer_company)
                                      * 100.0, 1) AS VARCHAR)+ '%' +' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND(closed_reason LIKE('%internal%')
                                            OR closed_reason IN('same_source_fix', 'unknown_fix', 'workaround', 'backout', 'bug_fix', 'documentation', 'duplicate', 'fw', 'high_risk', 'hw', 'intel_silicon_bug', 'internal_doc_bug', 'internal_fw_bug', 'internal_hw_bug', 'internal_mmanufacturing', 'internal_si_bug', 'internal_sw_bug', 'product_changed', 'promoted', 'pushed_to_other_database', 'rcr_created', 'regression_reverted', 'requirements_updated', 'transferred', 'user_verified', 'wont_fix', 'zbb'))
                                            AND customer_company = A.customer_company) AS VARCHAR) +'/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_company = A.customer_company) AS VARCHAR) +')'
                            END) AS IntelIssuePercentage,
                            (CASE WHEN (SELECT COUNT(cp_id)
                                        FROM " + selectedPlatform + @"
                                        WHERE sysdebug LIKE('%customer_must_fix%')
                                        AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                        AND status IN('complete', 'rejected')
                                        AND customer_company = A.customer_company) = 0
                                  THEN 'N/A'
                                  ELSE CAST(ROUND(
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                       AND customer_company = A.customer_company)
                                      /
                                      (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                       FROM " + selectedPlatform + @"
                                       WHERE sysdebug LIKE('%customer_must_fix%')
                                       AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                       AND status IN('complete', 'rejected')
                                       AND customer_company = A.customer_company)
                                      * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE status IN('complete', 'rejected')
                                            AND sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND (closed_reason IN('3rd_party', '3rd_party_hw_bug', '3rd_party_sw_bug', 'application_bug', 'os_issue'))
                                            AND customer_company = A.customer_company) AS VARCHAR) + '/' +
                                      CAST((SELECT COUNT(cp_id)
                                            FROM " + selectedPlatform + @"
                                            WHERE sysdebug LIKE('%customer_must_fix%')
                                            AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                            AND status IN('complete', 'rejected')
                                            AND customer_company = A.customer_company) AS VARCHAR) + ')'
                            END) AS ThirdPartyPercentage,
                            (CASE WHEN (SELECT COUNT(cp_id)
                                FROM " + selectedPlatform + @"
                                WHERE sysdebug LIKE('%customer_must_fix%')
                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                AND status IN('complete', 'rejected')
                                AND customer_company = A.customer_company) = 0
                            THEN 'N/A'
                            ELSE CAST(ROUND(
                                (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                FROM " + selectedPlatform + @"
                                WHERE sysdebug LIKE('%customer_must_fix%')
                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                AND status IN('complete', 'rejected')
                                AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                AND customer_company = A.customer_company)
                                /
                                (SELECT CAST(COUNT(cp_id) AS FLOAT)
                                FROM " + selectedPlatform + @"
                                WHERE sysdebug LIKE('%customer_must_fix%')
                                AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                AND status IN('complete', 'rejected')
                                AND customer_company = A.customer_company)
                                * 100.0, 1) AS VARCHAR)+ '%' + ' (' +
                                CAST((SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE status IN('complete', 'rejected')
                                    AND sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND (closed_reason IN('await_user_verify', 'board_issue', 'config_issue', 'customer_bios_issue', 'customer_hw_issue', 'customer_sw_issue', 'inquiry_closed', 'platform_out_of_spec', 'test_bug'))
                                    AND customer_company = A.customer_company) AS VARCHAR) + '/' +
                                CAST((SELECT COUNT(cp_id)
                                    FROM " + selectedPlatform + @"
                                    WHERE sysdebug LIKE('%customer_must_fix%')
                                    AND cmf_request IN('cmf_ok', 'cmf_duplicate')
                                    AND status IN('complete', 'rejected')
                                    AND customer_company = A.customer_company) AS VARCHAR) + ')'
                    END) AS CustomerIssuePercentage
                        FROM
                            " + selectedPlatform + @" A
                        GROUP BY
                            A.customer_company
                        ORDER BY
                            Total_CMF_Approved desc";

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);

                    GridView_oem_summary.DataSource = dt;
                    GridView_oem_summary.DataBind();
                }
            }
        }
    }

    protected void GridViewoem_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string oemfilter = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column index for "days_active"
            int daysActiveIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "days_active")
                {
                    daysActiveIndex = i;
                    break;
                }
            }

            // Apply color coding to days_active column
            if (daysActiveIndex != -1)
            {
                string daysActiveText = e.Row.Cells[daysActiveIndex].Text;
                int daysActive;

                if (int.TryParse(daysActiveText, out daysActive))
                {
                    if (daysActive > 21)
                    {
                        e.Row.Cells[daysActiveIndex].ForeColor = System.Drawing.Color.Red;
                        e.Row.Cells[daysActiveIndex].Font.Bold = true;
                    }
                    else if (daysActive > 14)
                    {
                        e.Row.Cells[daysActiveIndex].ForeColor = System.Drawing.Color.Orange;
                    }
                    else
                    {
                        e.Row.Cells[daysActiveIndex].ForeColor = System.Drawing.Color.Green;
                    }
                }
            }

            // If you want to add clickable links similar to the design version, uncomment below:
            /*
            // Find the column index for "Total CMF_Approved" (if you want clickable links)
            int colIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "Total CMF_Approved")
                {
                    colIndex = i;
                    break;
                }
            }

            if (colIndex != -1)
            {
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "customer_company");

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                oemfilter = oem;

                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndex].Text = link;
            }
            */
        }
    }
    protected void GridView6_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string trigger = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column indices for both red and yellow highlighted columns
            int colIndexApproved = -1;
            int colIndexASK = -1;
            int colIndexReject = -1;
            int colIndexCMFOpen = -1;
            int colIndexNoise = -1;
            int colIndexIntelIssue = -1;
            int colIndex3rdParty = -1;
            int colIndexCustomerIssue = -1;
            int colIndexDispTPT = 5;
            int colIndexResolveTPT = 7;
            int colIndexTotalTPT = 6;

            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                string headerText = grid.HeaderRow.Cells[i].Text.Trim();

                if (headerText == "Total CMF_Approved")
                {
                    colIndexApproved = i;
                }
                else if (headerText == "Issues in CMF_ASK")
                {
                    colIndexASK = i;
                }
                else if (headerText == "Total CMF_REJECT")
                {
                    colIndexReject = i;
                }
                else if (headerText == "CMF Open %")
                {
                    colIndexCMFOpen = i;
                }
                else if (headerText == "Noise%")
                {
                    colIndexNoise = i;
                }
                else if (headerText == "Intel Issue %")
                {
                    colIndexIntelIssue = i;
                }
                else if (headerText == "3rd Party %")
                {
                    colIndex3rdParty = i;
                }
                else if (headerText == "Customer Issue %")
                {
                    colIndexCustomerIssue = i;
                }
            }

            // Modify the columns for red highlighted sections
            if (colIndexApproved != -1)
            {
                trigger = "_trg1";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + val + "</a>";

                e.Row.Cells[colIndexApproved].Text = link;
            }

            if (colIndexASK != -1)
            {
                trigger = "_trg2";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Issues_in_CMF_ASK");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + val + "</a>";

                e.Row.Cells[colIndexASK].Text = link;
            }

            if (colIndexReject != -1)
            {
                trigger = "_trg3";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_REJECT");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + val + "</a>";

                e.Row.Cells[colIndexReject].Text = link;
            }

            // Modify the columns for yellow highlighted sections
            if (colIndexCMFOpen != -1)
            {
                trigger = "_trg4";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CMFOpenPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCMFOpen].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexNoise != -1)
            {
                trigger = "_trg5";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Noise");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexNoise].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexIntelIssue != -1)
            {
                trigger = "_trg6";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "IntelIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexIntelIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndex3rdParty != -1)
            {
                trigger = "_trg7";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "ThirdPartyPercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndex3rdParty].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexCustomerIssue != -1)
            {
                trigger = "_trg8";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CustomerIssuePercentage");
                object desVal = DataBinder.Eval(e.Row.DataItem, "Component");

                string val = rawVal != null ? rawVal.ToString() : "";
                string ingred = desVal != null ? desVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                ingred += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal2('" + ingred + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCustomerIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexDispTPT != -1)
            {
                int dispTPT = 0;
                int.TryParse(e.Row.Cells[colIndexDispTPT].Text, out dispTPT);

                if (dispTPT > 2)
                {
                    e.Row.Cells[colIndexDispTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexDispTPT].ForeColor = System.Drawing.Color.Green;
                }
            }

            if (colIndexResolveTPT != -1)
            {
                int resolveTPT = 0;
                int.TryParse(e.Row.Cells[colIndexResolveTPT].Text, out resolveTPT);

                if (resolveTPT > 18)
                {
                    e.Row.Cells[colIndexResolveTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexResolveTPT].ForeColor = System.Drawing.Color.Green;
                }
            }

            if (colIndexTotalTPT != -1)
            {
                int totalTPT = 0;
                int.TryParse(e.Row.Cells[colIndexTotalTPT].Text, out totalTPT);

                if (totalTPT > 21)
                {
                    e.Row.Cells[colIndexTotalTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexTotalTPT].ForeColor = System.Drawing.Color.Green;
                }
            }
        }


    }

    protected void GridView7_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        string trigger = "";

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            // Find the column indices for both red and yellow highlighted columns
            int colIndexApproved = -1;
            int colIndexASK = -1;
            int colIndexReject = -1;
            int colIndexCMFOpen = -1;
            int colIndexNoise = -1;
            int colIndexIntelIssue = -1;
            int colIndex3rdParty = -1;
            int colIndexCustomerIssue = -1;
            int colIndexDispTPT = 4; // Adjusted for OEM Summary (no SWImageFreeze column)
            int colIndexResolveTPT = 6; // Adjusted for OEM Summary
            int colIndexTotalTPT = 5;   // Adjusted for OEM Summary

            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                string headerText = grid.HeaderRow.Cells[i].Text.Trim();

                if (headerText == "Total CMF_Approved")
                {
                    colIndexApproved = i;
                }
                else if (headerText == "Issues in CMF_ASK")
                {
                    colIndexASK = i;
                }
                else if (headerText == "Total CMF_REJECT")
                {
                    colIndexReject = i;
                }
                else if (headerText == "CMF Open %")
                {
                    colIndexCMFOpen = i;
                }
                else if (headerText == "Noise%")
                {
                    colIndexNoise = i;
                }
                else if (headerText == "Intel Issue %")
                {
                    colIndexIntelIssue = i;
                }
                else if (headerText == "3rd Party %")
                {
                    colIndex3rdParty = i;
                }
                else if (headerText == "Customer Issue %")
                {
                    colIndexCustomerIssue = i;
                }
            }

            // Modify the columns for red highlighted sections
            if (colIndexApproved != -1)
            {
                trigger = "_trg1";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_Approved");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndexApproved].Text = link;
            }

            if (colIndexASK != -1)
            {
                trigger = "_trg2";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Issues_in_CMF_ASK");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndexASK].Text = link;
            }

            if (colIndexReject != -1)
            {
                trigger = "_trg3";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Total_CMF_REJECT");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + val + "</a>";

                e.Row.Cells[colIndexReject].Text = link;
            }

            // Modify the columns for yellow highlighted sections
            if (colIndexCMFOpen != -1)
            {
                trigger = "_trg4";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CMFOpenPercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCMFOpen].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexNoise != -1)
            {
                trigger = "_trg5";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "Noise");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexNoise].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexIntelIssue != -1)
            {
                trigger = "_trg6";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "IntelIssuePercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexIntelIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndex3rdParty != -1)
            {
                trigger = "_trg7";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "ThirdPartyPercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndex3rdParty].Text = val.Replace("(" + numerator, "(" + link);
            }

            if (colIndexCustomerIssue != -1)
            {
                trigger = "_trg8";
                object rawVal = DataBinder.Eval(e.Row.DataItem, "CustomerIssuePercentage");
                object oemVal = DataBinder.Eval(e.Row.DataItem, "OEM"); // Changed from Component to OEM

                string val = rawVal != null ? rawVal.ToString() : "";
                string oem = oemVal != null ? oemVal.ToString() : "";

                // Extract numerator from the ratio string
                string numerator = ExtractNumerator(val);
                oem += trigger;
                string link = "<a href='javascript:void(0);' onclick=\"showDetailsModal3('" + oem + "')\">" + numerator + "</a>";

                // Keep the original content intact and append the clickable numerator
                e.Row.Cells[colIndexCustomerIssue].Text = val.Replace("(" + numerator, "(" + link);
            }

            // TPT color coding
            if (colIndexDispTPT != -1)
            {
                int dispTPT = 0;
                int.TryParse(e.Row.Cells[colIndexDispTPT].Text, out dispTPT);

                if (dispTPT > 2)
                {
                    e.Row.Cells[colIndexDispTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexDispTPT].ForeColor = System.Drawing.Color.Green;
                }
            }

            if (colIndexResolveTPT != -1)
            {
                int resolveTPT = 0;
                int.TryParse(e.Row.Cells[colIndexResolveTPT].Text, out resolveTPT);

                if (resolveTPT > 18)
                {
                    e.Row.Cells[colIndexResolveTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexResolveTPT].ForeColor = System.Drawing.Color.Green;
                }
            }

            if (colIndexTotalTPT != -1)
            {
                int totalTPT = 0;
                int.TryParse(e.Row.Cells[colIndexTotalTPT].Text, out totalTPT);

                if (totalTPT > 21)
                {
                    e.Row.Cells[colIndexTotalTPT].ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    e.Row.Cells[colIndexTotalTPT].ForeColor = System.Drawing.Color.Green;
                }
            }
        }
    }

    protected void overall_request_details_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        // Cancel the edit mode
        overall_request_details.EditIndex = -1;
        selectedValue = Session["selectedValue"] as string ?? null;

        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);


        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void overall_request_details_RowEditing(object sender, GridViewEditEventArgs e)
    {
        // Set the row to be edited
        overall_request_details.EditIndex = e.NewEditIndex;
        selectedValue = Session["selectedValue"] as string ?? null;

        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void overall_request_details_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Get the row being updated
        GridViewRow row = overall_request_details.Rows[e.RowIndex];

        // Retrieve the new values with explicit null checks


        string los = null;
        DropDownList ddllos = row.FindControl("ddllos") as DropDownList;
        if (ddllos != null)
        {
            los = ddllos.SelectedValue;
        }


        string impact = null;
        TextBox txtimpact = row.FindControl("txtimpact") as TextBox;
        if (txtimpact != null)
        {
            impact = txtimpact.Text;
        }

        string ownerTextbox = null;
        TextBox txtOwner = row.FindControl("txtOwner") as TextBox;
        if (txtOwner != null)
        {
            ownerTextbox = txtOwner.Text;
        }



        string newidst = null;
        TextBox txtidst = row.FindControl("txtidst") as TextBox;
        if (txtidst != null)
        {
            newidst = txtidst.Text;
        }

        // Get the SightingID (unique key) for the row
        string sightingId = overall_request_details.DataKeys[e.RowIndex].Value.ToString();

        // Fetch the current value of the Status field from the database

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;


        // Update the database
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            //string query = "UPDATE " + selectedPlatform + " SET customer_owner = @Owner, idst=@idst, los=@los, impact=@impact, progress = CASE WHEN @los = 'Yes' THEN 'orange' ELSE progress END WHERE cp_id = @SightingID";
            string query = @"UPDATE " + selectedPlatform + @" 
                SET customer_owner = @Owner, 
                    idst = @idst, 
                    los = @los, 
                    impact = @impact,
                    original_progress = CASE 
                        WHEN @los = 'Yes' AND (original_progress IS NULL OR original_progress = '') 
                        THEN progress 
                        ELSE original_progress 
                    END,
                    progress = CASE 
                        WHEN @los = 'Yes' THEN 'orange'
                        WHEN @los = 'No' AND (original_progress IS NOT NULL AND original_progress != '') 
                        THEN original_progress
                        ELSE progress 
                    END
                WHERE cp_id = @SightingID";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Owner", ownerTextbox);

                cmd.Parameters.AddWithValue("@SightingID", sightingId);
                cmd.Parameters.AddWithValue("@idst", newidst);
                cmd.Parameters.AddWithValue("@los", los);
                cmd.Parameters.AddWithValue("@impact", impact);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        // Reset the edit index and rebind the data
        overall_request_details.EditIndex = -1;
        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void GridView_notes_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Check if the row is a data row (not header, footer, etc.)
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Get the values from the current row
            int dispTPT = 0;
            int resolveTPT = 0;
            int totalTPT = 0;

            // Try parsing the values to integers
            int.TryParse(e.Row.Cells[0].Text, out dispTPT);
            int.TryParse(e.Row.Cells[1].Text, out resolveTPT);
            int.TryParse(e.Row.Cells[2].Text, out totalTPT);

            // Add suffix to CMF_Disposition_TPT cell
            if (e.Row.Cells[0].Text != "")
            {
                e.Row.Cells[0].Text += " Days";
            }

            // Add suffix to CMF_Resolution_TPT cell
            if (e.Row.Cells[1].Text != "")
            {
                e.Row.Cells[1].Text += " Days";
            }

            // Add suffix to critical_tpt cell
            if (e.Row.Cells[2].Text != "")
            {
                e.Row.Cells[2].Text += " Days";
            }

            // Apply red color if conditions are met
            if (dispTPT > 2)
            {
                e.Row.Cells[0].ForeColor = System.Drawing.Color.Red; // Assuming disp_tpt is the first column
            }
            else
            {
                e.Row.Cells[0].ForeColor = System.Drawing.Color.Green; // Assuming disp_tpt is the first column
            }

            if (resolveTPT > 18)
            {
                e.Row.Cells[1].ForeColor = System.Drawing.Color.Red; // Assuming resolve_tpt is the second column
            }
            else
            {
                e.Row.Cells[1].ForeColor = System.Drawing.Color.Green; // Assuming resolve_tpt is the second column
            }

            if (totalTPT > 21)
            {
                e.Row.Cells[2].ForeColor = System.Drawing.Color.Red; // Assuming resolve_tpt is the second column
            }
            else
            {
                e.Row.Cells[2].ForeColor = System.Drawing.Color.Green; // Assuming resolve_tpt is the second column
            }
        }
    }


    protected void overall_request_details_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (e.Row.RowType == DataControlRowType.DataRow && e.Row.RowIndex == overall_request_details.EditIndex)
        {


            // Populate the Color DropDownList
            var ddllos = e.Row.FindControl("ddllos") as DropDownList;
            if (ddllos != null)
            {
                ddllos.Items.Add(new ListItem("No", "No"));
                ddllos.Items.Add(new ListItem("Yes", "Yes"));

                ddllos.SelectedValue = DataBinder.Eval(e.Row.DataItem, "los").ToString();
            }




        }
        else
        {



            string losValue = null;
            if (DataBinder.Eval(e.Row.DataItem, "los") != null)
            {
                losValue = DataBinder.Eval(e.Row.DataItem, "los").ToString();
            }

            // Check if the row is a data row (not a header or footer row)
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Get the first cell of the row
                TableCell firstCell = e.Row.Cells[2];

                // Get the value of the first cell (assuming it's a color name like "red", "green", etc.)
                string cellValue = firstCell.Text.Trim().ToLower();  // Ensure it's in a consistent format

                // Check if the cell has a valid color value
                if (!string.IsNullOrEmpty(cellValue))
                {
                    try
                    {
                        // Set the background color and text color based on the cell value
                        firstCell.BackColor = System.Drawing.Color.FromName(cellValue);
                        firstCell.ForeColor = System.Drawing.Color.FromName(cellValue);
                    }
                    catch (Exception ex)
                    {
                        // Handle invalid color names (e.g., if the cell contains a non-color value)
                        // Optionally log the exception or use default colors
                        firstCell.BackColor = System.Drawing.Color.Gray;  // Default background color
                        firstCell.ForeColor = System.Drawing.Color.White;  // Default text color
                    }
                }
            }


        }
    }

    protected void GridView_design_open_RowEditing(object sender, GridViewEditEventArgs e)
    {
        e.Cancel = true;
        GridView_design_open.EditIndex = -1;
        // Set the EditIndex for the edit GridView and make it visible
        GridView2_edit.EditIndex = e.NewEditIndex;
        GridView2_edit.Visible = true;
        GridView_design_open.Visible = false;
        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
    }

    protected void GridView_design_open_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Assuming selectedPlatform is a class-level variable or accessible in this method
        //string selectedPlatform = "CMF_ARL_HX_ALL_COMPONENTS_TABLE"; // Example value, replace with actual value
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";


        // Get the row being updated
        GridViewRow row = GridView2_edit.Rows[e.RowIndex];

        // Retrieve the new value for SWImageFreeze from the TextBox
        TextBox txtSWImageFreeze = row.FindControl("txtSWImageFreeze") as TextBox;
        string newSWImageFreeze = txtSWImageFreeze != null ? txtSWImageFreeze.Text : null;

        // Retrieve the new value for SWImageFreeze from the TextBox
        TextBox txtSupportModel = row.FindControl("txtSupportModel") as TextBox;
        string newSupportModel = txtSupportModel != null ? txtSupportModel.Text : null;

        // Get the design key for the row being updated
        string design = GridView2_edit.DataKeys[e.RowIndex].Value.ToString();

        // Update the database with the new value
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string query = "UPDATE " + designTable + " SET sw_image_freeze = @SWImageFreeze, support_model=@SupportModel WHERE customer_detail = @Design";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@SWImageFreeze", newSWImageFreeze);
                cmd.Parameters.AddWithValue("@SupportModel", newSupportModel);
                cmd.Parameters.AddWithValue("@Design", design);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Reset the EditIndex and rebind both GridViews
        GridView2_edit.EditIndex = -1;
        GridView2_edit.Visible = false;

        GridView_design_open.EditIndex = -1;
        GridView_design_open.Visible = true;


        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);
        string script = "window.onload = function () { adjustGridHeights(); };";  // Call your custom JS function
        ClientScript.RegisterStartupScript(this.GetType(), "TriggerJavaScriptFunction", script, true);

    }



    protected void GridView_design_open_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        // Reset the EditIndex and hide the edit GridView
        GridView2_edit.EditIndex = -1;
        GridView2_edit.Visible = false;
        GridView_design_open.EditIndex = -1;
        GridView_design_open.Visible = true;
        selectedValue = Session["selectedValue"] as string ?? null;
        BindGridView(selectedValue);
        BindGridView_design_open(selectedValue);
        BindGridView_cmf_summary(selectedValue);
        BindGridView_design_summary(selectedValue);
        BindGridView_component_summary(selectedValue);
        BindGridView_oem_summary(selectedValue);

    }

    protected void GridView_design_open_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GridView grid = (GridView)sender;

            foreach (var driver in drivers)
            {
                string colName = driverColumns[driver] + "_Issues";
                int colIndex = -1;

                for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
                {
                    if (grid.HeaderRow.Cells[i].Text.Trim() == driver)
                    {
                        colIndex = i;
                        break;
                    }
                }

                if (colIndex != -1)
                {
                    try
                    {
                        object rawVal = DataBinder.Eval(e.Row.DataItem, colName);
                        object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                        string val = rawVal != null ? rawVal.ToString() : "";
                        string design = desVal != null ? desVal.ToString() : "";

                        // Determine the link color based on the design value
                        string linkColor = design == "Total" ? "black" : "blue";

                        string link = "<a href='javascript:void(0);' style='color:" + linkColor + ";' onclick=\"showDriverDetailsModal('" + design + "','" + driver + "')\">" + val + "</a>";

                        e.Row.Cells[colIndex].Text = link;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error accessing column " + colName + ": " + ex.Message);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Column index not found for driver: " + driver);
                }
            }

            // Make Implemented/Verified clickable
            int implementedVerifiedIndex = -1;
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.Trim() == "Impl/Verified")
                {
                    implementedVerifiedIndex = i;
                    break;
                }
            }

            if (implementedVerifiedIndex != -1)
            {
                try
                {
                    object rawVal = DataBinder.Eval(e.Row.DataItem, "Implemented_Verified");
                    object desVal = DataBinder.Eval(e.Row.DataItem, "Design");

                    string val = rawVal != null ? rawVal.ToString() : "";
                    string design = desVal != null ? desVal.ToString() : "";

                    // Determine the link color based on the design value
                    string linkColor = design == "Total" ? "black" : "blue";

                    string link = "<a href='javascript:void(0);' style='color:" + linkColor + ";' onclick=\"showImplementedVerifiedDetailsModal('" + design + "')\">" + val + "</a>";

                    e.Row.Cells[implementedVerifiedIndex].Text = link;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error accessing Implemented_Verified: " + ex.Message);
                }
            }

            // Hide the edit button for the "Total" row
            object designValue = DataBinder.Eval(e.Row.DataItem, "Design");
            if (designValue != null && designValue.ToString() == "Total")
            {
                foreach (System.Web.UI.Control control in e.Row.Cells[e.Row.Cells.Count - 1].Controls)
                {
                    System.Diagnostics.Debug.WriteLine("Control Type: " + control.GetType().ToString());

                    // Check if the control is a button type
                    if (control is Button || control is LinkButton || control is ImageButton)
                    {
                        System.Diagnostics.Debug.WriteLine("Hiding control: " + control.GetType().ToString());
                        control.Visible = false;
                    }
                }
            }
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }

    protected void lnkValue_Command(object sender, System.Web.UI.WebControls.CommandEventArgs e)
    {

        string collectedDrivers = driverCollectorhf.Value;
        // Get the selected distinct value from the CommandArgument
        selectedValue = e.CommandArgument.ToString();

        if (selectedValue != "")
        {
            if (selectedValue == "AllDrivers")
            {
                collectedDrivers = "AllDrivers";
            }

        }


        if (collectedDrivers == "AllDrivers")
        {
            Session["selectedPlatform"] = selectedPlatform;
            Session["selectedValue"] = selectedValue;


            string ddrivers;
            string alldriver_query = "SELECT DISTINCT([drivers]) FROM " + selectedPlatform + " WHERE status in ('open') and cmf_request not in ('cmf_reject') AND sysdebug Like ('%customer_must_fix%') ";

            string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(alldriver_query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder driversList = new StringBuilder();

                        while (reader.Read())
                        {
                            // Assuming the column name is 'drivers' and contains the driver's name/ID
                            string driver = reader["drivers"].ToString();

                            if (driversList.Length > 0)
                            {
                                driversList.Append(","); // Append comma if not the first value
                            }

                            driversList.Append(driver);
                        }

                        // Now driversList contains the comma-separated string
                        ddrivers = driversList.ToString();

                        // Optionally, you can store this string in a session or variable

                    }
                }
            }

            selectedValue = ddrivers;


            headerTitle.InnerText = headerTemplate += "All Milestones";
            // Rebind the table with the filtered data
            BindGridView();
            BindGridView_design_open(selectedValue);
            BindGridView_cmf_summary(selectedValue);
            BindGridView_design_summary(selectedValue);
            BindGridView_component_summary(selectedValue);
            BindGridView_oem_summary(selectedValue);
            //GetGridViewData(selectedValue);
        }
        else
        {
            // Collect all the selected checkbox values from the Repeater

            // Step 1: Split the collectedDrivers string into a list of drivers
            var drivers = collectedDrivers.Split(',').ToList();

            // Step 2: Identify duplicates by grouping and filtering those with count > 1
            var duplicateDrivers = drivers.GroupBy(d => d)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

            // Step 3: If no duplicates, stop here
            if (duplicateDrivers.Count == 0)
            {

            }
            else
            {
                // Step 4: Keep only drivers with duplicates
                var driversWithDuplicatesOnly = drivers.Where(d => duplicateDrivers.Contains(d)).ToList();

                // Step 5: Remove duplicates and keep only one copy of each element
                var uniqueDrivers = driversWithDuplicatesOnly.Distinct().ToList();

                // Update collectedDrivers with the result as a comma-separated string
                collectedDrivers = string.Join(",", uniqueDrivers);

            }



            selectedValue = collectedDrivers;
            Session["selectedValue"] = collectedDrivers;
            headerTitle.InnerText = headerTemplate += selectedValue;
            // Rebind the table with the filtered data
            BindGridView(selectedValue);
            BindGridView_design_open(selectedValue);
            BindGridView_cmf_summary(selectedValue);
            BindGridView_design_summary(selectedValue);
            BindGridView_component_summary(selectedValue);
            BindGridView_oem_summary(selectedValue);

        }


    }

    protected void chkAllDrivers_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox chkAllDrivers = (CheckBox)sender;
        // Handle the event when the "All Drivers" checkbox is checked or unchecked
        // You can access the checked state using chkAllDrivers.Checked
    }

    protected void chkValue_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox chkValue = (CheckBox)sender;
        // Handle the event when a distinct value checkbox is checked or unchecked
        // You can access the checked state using chkValue.Checked
        // You can also access the text (which is the distinct value) using chkValue.Text
    }


    protected void btnExportInModal_Click(object sender, EventArgs e)
    {
        ExportGridViewToPPT(sender, e);
    }





    protected void ExportGridViewToPPT(object sender, EventArgs e)
    {
        //Get the selected values from the hidden field
        string selectedValuesString = hfSelectedValues.Value;
        if (string.IsNullOrEmpty(selectedValuesString))
        {
            // No values selected, handle accordingly
            return;
        }

        // Split the selected values into an array
        string[] selectedValues = selectedValuesString.Split(',');

        // Define the export path in the App_Data folder
        string exportPath = Server.MapPath("~/App_Data/ExportedGridView.pptx");

        using (PresentationDocument presentation = PresentationDocument.Create(exportPath, PresentationDocumentType.Presentation))
        {
            PresentationPart presentationPart = presentation.AddPresentationPart();
            presentationPart.Presentation = new DocumentFormat.OpenXml.Presentation.Presentation();

            SlideIdList slideIdList = new SlideIdList();
            uint slideId = 256;

            // Fetch data tables
            selectedValue = Session["selectedValue"] as string ?? null;
            System.Data.DataTable dt = GetGridViewData_maindriver(selectedValue);
            System.Data.DataTable dt2 = GetGridViewDataFromControl(GridView_design_open, true);
            System.Data.DataTable dt5 = GetGridViewDataFromControl_TotalTable();
            System.Data.DataTable dt6 = GetGridViewDataFromControl(GridView_notes, false);
            System.Data.DataTable dt3 = GetGridViewDataFromControl(GridView_cmf_summary, false);
            System.Data.DataTable dt4 = GetGridViewDataFromControl(GridView_cmf_pending, false);
            int rowsPerSlide = 10;

            // Add a slide for dt5 and dt3 together
            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
            slidePart.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

            // Add both tables to the same slide
            AddThreeTablesToSlide(slidePart, dt5, dt3, dt6, 0, dt5.Rows.Count, 0, dt3.Rows.Count, 0, dt6.Rows.Count);

            // Add the slide to the slide list
            slideIdList.Append(new SlideId
            {
                Id = slideId,
                RelationshipId = presentationPart.GetIdOfPart(slidePart)
            });

            slideId++;

            // Continue with other slides
            slideId = AddSlidesForGridView2_3(presentationPart, slideIdList, dt2, rowsPerSlide = 20, slideId);

            for (int i = 0; i < dt.Rows.Count; i += rowsPerSlide)
            {
                // Create a new slide for the first GridView data table (dt)
                SlidePart slidePartMain = presentationPart.AddNewPart<SlidePart>();
                slidePartMain.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

                // Add a table to the slide
                AddTableToSlide(slidePartMain, dt, i, rowsPerSlide);

                // Add the slide to the slide list
                slideIdList.Append(new SlideId
                {
                    Id = slideId,
                    RelationshipId = presentationPart.GetIdOfPart(slidePartMain)
                });

                slideId++;
            }

            slideId = AddSlidesForGridView2_3(presentationPart, slideIdList, dt4, rowsPerSlide = 20, slideId);

            foreach (string selectedValue1 in selectedValues)
            {
                System.Data.DataTable dt1 = GetGridViewData(selectedValue1);
                for (int i = 0; i < dt1.Rows.Count; i += rowsPerSlide)
                {
                    // Create a new slide for the first GridView data table (dt1)
                    SlidePart slidePartForDt1 = presentationPart.AddNewPart<SlidePart>();
                    slidePartForDt1.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

                    // Add a table to the slide
                    AddTableToSlide(slidePartForDt1, dt1, i, rowsPerSlide);

                    // Add the slide to the slide list
                    slideIdList.Append(new SlideId
                    {
                        Id = slideId,
                        RelationshipId = presentationPart.GetIdOfPart(slidePartForDt1)
                    });

                    slideId++;
                }
            }

            // Append slide ID list to the presentation and save it
            presentationPart.Presentation.Append(slideIdList);
            presentationPart.Presentation.Save();
        }

        // Provide the file as a download to the user
        Response.ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        Response.AddHeader("Content-Disposition", "attachment; filename=ExportedGridView.pptx");

        Response.TransmitFile(exportPath);
        Response.End();
    }

    private uint AddSlidesForGridView2_3(PresentationPart presentationPart, SlideIdList slideIdList, System.Data.DataTable dt, int rowsPerSlide, uint slideId)
    {
        for (int i = 0; i < dt.Rows.Count; i += rowsPerSlide)
        {
            // Create a new slide
            SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
            slidePart.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

            // Add a table to the slide
            AddTableToSlide_grid2_3(slidePart, dt, i, rowsPerSlide);

            // Add the slide to the slide list
            slideIdList.Append(new SlideId
            {
                Id = slideId,
                RelationshipId = presentationPart.GetIdOfPart(slidePart)
            });

            slideId++;
        }
        return slideId;
    }

    protected System.Data.DataTable GetGridViewDataFromControl(GridView gridView, bool excludeLastColumn = false, string filterValue = null)
    {

        System.Data.DataTable dt = new System.Data.DataTable();

        // Add columns to DataTable
        int columnCount = gridView.Columns.Count;
        if (excludeLastColumn)
        {
            columnCount -= 1; // Exclude the last column
        }

        for (int i = 0; i < columnCount; i++)
        {
            dt.Columns.Add(gridView.Columns[i].HeaderText);
        }

        // Add rows to DataTable
        foreach (GridViewRow row in gridView.Rows)
        {
            DataRow dr = dt.NewRow();
            for (int i = 0; i < columnCount; i++)
            {
                // Decode HTML-encoded content
                dr[i] = HttpUtility.HtmlDecode(row.Cells[i].Text);
            }

            // Ensure the first column is not empty
            if (string.IsNullOrWhiteSpace(dr[0].ToString()))
            {
                dr[0] = HttpUtility.HtmlDecode(row.Cells[0].Text);
            }

            dt.Rows.Add(dr);
        }

        return dt;
    }




    protected System.Data.DataTable GetGridViewData_maindriver(string filterValue = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string master_query = "SELECT progress As color, cp_id As SightingID, title , component_group , customer_owner As Owner, repro_on_rvp, cmf_status As Status, idst, los, drivers AS Driver FROM " + selectedPlatform;
            //!string.IsNullOrEmpty(filtervalue) && filtervalue!="AllDrivers"
            //!string.IsNullOrEmpty(filtervalue) && filtervalue!="AllDrivers" && filtervalue!="AllDrivers"
            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {


                if (filterValue.Contains(","))
                {
                    Session["filterValue"] = filterValue;
                    // Add a filter based on the selected distinct value
                    //master_query += " WHERE must_fix_for = @FilterValue";
                    master_query += " WHERE \r\n" +
                    "((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
                    "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
                    "    OR ( " +
                    "       @FilterValue LIKE '%,' + drivers + ',%' " +
                    "       OR @FilterValue LIKE drivers + ',%' " +
                    "       OR @FilterValue LIKE '%,' + drivers) ) " +
                    "AND status NOT IN ('complete', 'rejected') order by drivers";


                }
                else
                {
                    Session["filterValue"] = filterValue;
                    // Add a filter based on the selected distinct value
                    //master_query += " WHERE must_fix_for = @FilterValue";
                    master_query += " WHERE \r\n ((@FilterValue = 'Pre-PV' AND drivers LIKE '%WW%' \r\n" +
        "     AND FLOOR(CAST(SUBSTRING(drivers, CHARINDEX('WW', must_fix_for) + 2, 2) AS FLOAT)) BETWEEN 1 AND 31) \r\n" +
        "    OR drivers = @FilterValue )AND status not in ('complete', 'rejected') order by drivers";
                }


            }
            else
            {
                Session["filterValue"] = WorkWeek + " | CMF Live Dashboard - All";
                headerTitle.InnerText = WorkWeek + " | CMF Live Dashboard - All";
                master_query += " WHERE status not in ('complete', 'rejected') Order by drivers";
            }
            using (SqlCommand cmd = new SqlCommand(master_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    // Add the parameter if filtering
                    if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
                    {
                        if (filterValue.Contains(","))
                        {
                            sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

                        }
                        else
                        {
                            sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);

                        }
                    }
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    return dt;
                }
            }
        }
    }




    protected System.Data.DataTable GetGridViewData(string filterValue = null)
    {

        // Reuse the existing BindGridView method logic to fetch the data
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            string main_query = "SELECT progress, cp_id As SightingID, title , component_group , customer_owner As Owner, repro_on_rvp, cmf_status As Status, idst As iDST, los AS LOS, drivers AS Diver FROM " + selectedPlatform;

            if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
            {
                Session["filterValue"] = filterValue;
                // Add a filter based on the selected distinct value
                main_query += "  WHERE drivers = @FilterValue ";
            }

            using (SqlCommand cmd = new SqlCommand(main_query, con))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    // Add the parameter if filtering
                    if (!string.IsNullOrEmpty(filterValue) && filterValue != "AllDrivers")
                    {
                        sda.SelectCommand.Parameters.AddWithValue("@FilterValue", filterValue);
                    }
                    System.Data.DataTable dt = new System.Data.DataTable();
                    sda.Fill(dt);
                    return dt;
                }
            }
        }
    }

    protected System.Data.DataTable GetGridViewDataFromControl_TotalTable(string filtervalue = null)
    {

        string main_query = "";
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string componentTable = basePlatform + "_COMPONENT_GROUP_TABLE";
        //string driver_filter = "";
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            List<string> drivers = new List<string>();
            Dictionary<string, string> driverColumns = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
            {
                Session["filterValue"] = filtervalue;
                string[] rawDrivers = filtervalue.Split(new char[] { ',' });

                foreach (string driver in rawDrivers)
                {
                    string trimmedDriver = driver.Trim();
                    if (!string.IsNullOrEmpty(trimmedDriver))
                    {
                        drivers.Add(trimmedDriver);
                    }
                }
                string Driver_name = "";
                string driverCaseStatements = "";
                foreach (string driver in drivers)
                {
                    string safeColumnName = driver.Replace(" ", "_").Replace("-", "_"); // Ensure column name is safe
                    driverColumns[driver] = safeColumnName;
                    driverCaseStatements += ", SUM(CASE WHEN A.drivers = '" + driver + "' THEN 1 ELSE 0 END) AS [" + safeColumnName + "_Issues]";
                    Driver_name = safeColumnName + "_Issues";
                }



                main_query = "SELECT B.component_group AS Component " +
                             driverCaseStatements +
                             "FROM " + selectedPlatform + " A INNER JOIN " + componentTable + " B ON A.component_group = B.component_group " +
                             "Where status not in ('complete','rejected') GROUP BY B.component_group ";

            }
            else
            {


                main_query = "SELECT B.component_group AS Component, COUNT(A.cp_id) AS Driver_issues FROM " + selectedPlatform + " A INNER JOIN " + componentTable + " B ON A.component_group = B.component_group " +
                             "Where status not in ('complete','rejected') GROUP BY B.component_group order by Driver_issues desc";


            }


            // Summary Query to fetch metrics
            string summaryQuery = @"
            SELECT 
                SUM(CASE WHEN cmf_request = 'cmf_ok' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS TotalCount,
                SUM(CASE WHEN [cmf_request] = 'cmf_duplicate' AND sysdebug Like ('%customer_must_fix%') THEN 1 ELSE 0 END) AS Duplicates,
                SUM(CASE WHEN cmf_request = 'cmf_ok' AND sysdebug Like ('%customer_must_fix%') AND ([status] = 'complete' OR [status] = 'rejected') THEN 1 ELSE 0 END) AS ClosedCount,
                SUM(CASE WHEN cmf_request in ('cmf_ok','cmf_duplicate') AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' THEN 1 ELSE 0 END) AS ImplementedCount,
                STUFF((
                    SELECT ', ' + [component_group]
                    FROM " + selectedPlatform + @"
                    WHERE cmf_request = 'cmf_ok' AND sysdebug Like ('%customer_must_fix%') AND [status] = 'implemented' 
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ImplementedComponents
            FROM " + selectedPlatform;


            conn.Open();

            // Fetch summary data
            System.Data.DataTable summaryTable = new System.Data.DataTable();
            using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                if (!string.IsNullOrEmpty(filtervalue) && filtervalue != "AllDrivers")
                {
                    da.SelectCommand.Parameters.AddWithValue("@FilterValue", filtervalue);
                }
                da.Fill(summaryTable);
                //Add custom columns to the summary table
                summaryTable.Columns.Add("Total", typeof(string));
                summaryTable.Columns.Add("Closed", typeof(string));
                summaryTable.Columns.Add("ImplementedDetails", typeof(string));

                if (summaryTable.Rows.Count > 0)
                {
                    DataRow row = summaryTable.Rows[0];
                    // Safely access columns and handle DBNull values
                    int totalCount = row["TotalCount"] is DBNull ? 0 : Convert.ToInt32(row["TotalCount"]);
                    int duplicates = row["Duplicates"] is DBNull ? 0 : Convert.ToInt32(row["Duplicates"]);
                    int closedCount = row["ClosedCount"] is DBNull ? 0 : Convert.ToInt32(row["ClosedCount"]);
                    int implementedCount = row["ImplementedCount"] is DBNull ? 0 : Convert.ToInt32(row["ImplementedCount"]);
                    //string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();
                    string implementedComponents = row["ImplementedComponents"] is DBNull ? "" : row["ImplementedComponents"].ToString();




                    // Split the string into a list of components
                    string[] componentsList = implementedComponents.Split(',');

                    // Create a dictionary to count occurrences of each component
                    Dictionary<string, int> componentCounts = new Dictionary<string, int>();

                    // Initialize a variable to store the total count
                    int totalCountComp = 0;

                    foreach (string component in componentsList)
                    {
                        string trimmedComponent = component.Trim(); // Trim spaces
                        if (componentCounts.ContainsKey(trimmedComponent))
                        {
                            componentCounts[trimmedComponent]++; // Increment count
                        }
                        else
                        {
                            componentCounts[trimmedComponent] = 1; // Initialize count
                        }

                        // Increment the total count for each component
                        totalCountComp++;
                    }

                    // Get the original number of components (from the split list)
                    int originalCount = implementedCount;

                    // Build the formatted result using string.Format
                    List<string> resultList = new List<string>();

                    foreach (KeyValuePair<string, int> kvp in componentCounts)
                    {
                        resultList.Add(string.Format("{0} - {1}", kvp.Key, kvp.Value));
                    }

                    // Check if total count does not match the original count
                    if (totalCountComp != originalCount)
                    {
                        int difference = Math.Abs(originalCount - totalCountComp);
                        resultList.Add(string.Format("Other - {0}", difference));
                    }

                    // Join the result into a single string
                    string resultComp = string.Join(", ", resultList);

                    string implementedCount_final = originalCount.ToString();

                    int nondupes = totalCount - duplicates;

                    row["Total"] = String.Format("{0} ({1}) + Duplicates", totalCount, duplicates);
                    row["Closed"] = String.Format("{0}", closedCount);
                    row["ImplementedDetails"] = String.Format(resultComp);
                }


                // **Create a new DataTable with only the last 3 columns**
                System.Data.DataTable filteredTable = new System.Data.DataTable();
                filteredTable.Columns.Add("Total", typeof(string));
                filteredTable.Columns.Add("Closed", typeof(string));
                filteredTable.Columns.Add("ImplementedCount", typeof(string));
                filteredTable.Columns.Add("ImplementedDetails", typeof(string));


                // Copy rows from ,summaryTable,
                foreach (DataRow row in summaryTable.Rows)
                {
                    DataRow newRow = filteredTable.NewRow();
                    newRow["Total"] = row["Total"];
                    newRow["Closed"] = row["Closed"];
                    newRow["ImplementedCount"] = row["ImplementedCount"];
                    newRow["ImplementedDetails"] = row["ImplementedDetails"];

                    filteredTable.Rows.Add(newRow);
                }

                return filteredTable;
            }
        }
    }
    protected SlidePart AddSlide(PresentationPart presentationPart, System.Data.DataTable dt)
    {
        SlidePart slidePart = presentationPart.AddNewPart<SlidePart>();
        slidePart.Slide = new DocumentFormat.OpenXml.Presentation.Slide(new CommonSlideData(new ShapeTree()));

        // Add Title to the Slide
        AddSlideTitle(slidePart, "GridView Data Export");

        // Add Table to Slide
        AddTableToSlide(slidePart, dt, startRow: 0, rowCount: 10);

        return slidePart;
    }

    protected void AddSlideTitle(SlidePart slidePart, string titleText)
    {
        // Use Presentation namespace for slide components
        DocumentFormat.OpenXml.Presentation.Shape titleShape = new DocumentFormat.OpenXml.Presentation.Shape(
            new DocumentFormat.OpenXml.Presentation.NonVisualShapeProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Title" },
                new DocumentFormat.OpenXml.Presentation.NonVisualShapeDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()),
            new DocumentFormat.OpenXml.Presentation.ShapeProperties(),
            new DocumentFormat.OpenXml.Presentation.TextBody(
                new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                new DocumentFormat.OpenXml.Drawing.ListStyle(),
                new DocumentFormat.OpenXml.Drawing.Paragraph(
                    new DocumentFormat.OpenXml.Drawing.Run(
                        new DocumentFormat.OpenXml.Drawing.Text { Text = titleText })))
        );

        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(titleShape);
    }

    protected void AddTableToSlide(SlidePart slidePart, System.Data.DataTable dt, int startRow, int rowCount)
    {
        var table = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid(dt.Columns.Count),  // Use the updated CreateTableGrid with percentage-based widths
            CreateTableHeader(dt)
        );

        // Append rows
        foreach (var row in CreateTableRows(dt, startRow, rowCount))
        {
            table.AppendChild(row);
        }

        // Define the slide width and height in EMUs
        long slideWidth = 9144000;  // Full slide width in EMUs
        long slideHeight = 5144000; // Full slide height in EMUs

        // Set table position as a percentage of slide width and height
        double percentageX = 0.02; // 10% of slide width from the left
        double percentageY = 0.04; // 10% of slide height from the top

        // Calculate the position (Offset) based on percentages
        long offsetX = (long)(slideWidth * percentageX);
        long offsetY = (long)(slideHeight * percentageY);

        // Define the graphic frame and ensure the table fits within the slide dimensions
        var graphicFrame = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Table" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX, Y = offsetY },  // Position the table
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.5) }  // Set the table size
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Append the graphic frame to the slide's shape tree
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame);
    }

    protected void AddTableToSlide_grid2_3(SlidePart slidePart, System.Data.DataTable dt, int startRow, int rowCount)
    {
        var table = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt.Columns.Count),  // Use the updated CreateTableGrid with percentage-based widths
            CreateTableHeader(dt)
        );

        // Append rows
        foreach (var row in CreateTableRows_grid2_3(dt, startRow, rowCount))
        {
            table.AppendChild(row);
        }

        // Define the slide width and height in EMUs
        long slideWidth = 9144000;  // Full slide width in EMUs
        long slideHeight = 5144000; // Full slide height in EMUs

        // Set table position as a percentage of slide width and height
        double percentageX = 0.02; // 10% of slide width from the left
        double percentageY = 0.04; // 10% of slide height from the top

        // Calculate the position (Offset) based on percentages
        long offsetX = (long)(slideWidth * percentageX);
        long offsetY = (long)(slideHeight * percentageY);

        // Define the graphic frame and ensure the table fits within the slide dimensions
        var graphicFrame = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Table" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX, Y = offsetY },  // Position the table
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.5) }  // Set the table size
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Append the graphic frame to the slide's shape tree
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame);
    }
    private void AddThreeTablesToSlide(SlidePart slidePart, System.Data.DataTable dt1, System.Data.DataTable dt2, System.Data.DataTable dt3, int startRow1, int rowCount1, int startRow2, int rowCount2, int startRow3, int rowCount3)
    {
        // Create the first table
        var table1 = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt1.Columns.Count),
            CreateTableHeader(dt1)
        );

        foreach (var row in CreateTableRows_grid2_3(dt1, startRow1, rowCount1))
        {
            table1.AppendChild(row);
        }

        // Create the second table
        var table2 = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt2.Columns.Count),
            CreateTableHeader(dt2)
        );

        foreach (var row in CreateTableRows_grid2_3(dt2, startRow2, rowCount2))
        {
            table2.AppendChild(row);
        }

        // Create the third table
        var table3 = new DocumentFormat.OpenXml.Drawing.Table(
            new DocumentFormat.OpenXml.Drawing.TableProperties(
                new DocumentFormat.OpenXml.Drawing.TableStyleId("{5C22544A-7EE6-4342-B048-85BDC9FD1C3A}")
            ),
            CreateTableGrid2_3(dt3.Columns.Count),
            CreateTableHeader(dt3)
        );

        foreach (var row in CreateTableRows_grid2_3(dt3, startRow3, rowCount3))
        {
            table3.AppendChild(row);
        }

        // Define the slide width and height in EMUs
        long slideWidth = 9144000;  // Full slide width in EMUs
        long slideHeight = 5144000; // Full slide height in EMUs

        // Set table positions
        long offsetX1 = (long)(slideWidth * 0.02);
        long offsetY1 = (long)(slideHeight * 0.04);

        long offsetX2 = (long)(slideWidth * 0.02);
        long offsetY2 = (long)(slideHeight * 0.4);

        long offsetX3 = (long)(slideWidth * 0.02);
        long offsetY3 = (long)(slideHeight * 0.74); // Position the third table lower on the slide

        // Add the first table to the slide
        var graphicFrame1 = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 1, Name = "Table1" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX1, Y = offsetY1 },
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.3) }
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table1)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Add the second table to the slide
        var graphicFrame2 = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 2, Name = "Table2" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX2, Y = offsetY2 },
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.3) }
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table2)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Add the third table to the slide
        var graphicFrame3 = new DocumentFormat.OpenXml.Presentation.GraphicFrame(
            new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameProperties(
                new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties { Id = 3, Name = "Table3" },
                new DocumentFormat.OpenXml.Presentation.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Presentation.ApplicationNonVisualDrawingProperties()
            ),
            new DocumentFormat.OpenXml.Presentation.Transform(
                new DocumentFormat.OpenXml.Drawing.Offset { X = offsetX3, Y = offsetY3 },
                new DocumentFormat.OpenXml.Drawing.Extents { Cx = slideWidth, Cy = (long)(slideHeight * 0.3) }
            ),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(table3)
                {
                    Uri = "http://schemas.openxmlformats.org/drawingml/2006/table"
                }
            )
        );

        // Append the graphic frames to the slide's shape tree
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame1);
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame2);
        slidePart.Slide.CommonSlideData.ShapeTree.AppendChild(graphicFrame3);
    }




    private DocumentFormat.OpenXml.Drawing.TableGrid CreateTableGrid(int columnCount)
    {
        var grid = new DocumentFormat.OpenXml.Drawing.TableGrid();

        // Define column widths as percentages of the slide width (100% = 9144000 EMUs)
        long slideWidth = 9144000; // Full slide width in EMUs
        double[] columnPercentages = new double[] { 0.05, 0.075, 0.2, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075, 0.075 }; // Example for column widths

        // Calculate the EMU width for each column based on the percentage
        for (int i = 0; i < columnCount; i++)
        {
            // Calculate width based on percentage
            long columnWidth = (long)(slideWidth * columnPercentages[i]);

            grid.AppendChild(new DocumentFormat.OpenXml.Drawing.GridColumn() { Width = columnWidth });
        }

        return grid;
    }

    private DocumentFormat.OpenXml.Drawing.TableGrid CreateTableGrid2_3(int columnCount)
    {
        var grid = new DocumentFormat.OpenXml.Drawing.TableGrid();

        // Define column widths as percentages of the slide width (100% = 9144000 EMUs)
        long slideWidth = 9144000; // Full slide width in EMUs
        double[] columnPercentages = new double[] { 0.16, 0.16, 0.16, 0.16, 0.16, 0.16, 0.16 }; // Example for column widths

        // Calculate the EMU width for each column based on the percentage
        for (int i = 0; i < columnCount; i++)
        {
            // Calculate width based on percentage
            long columnWidth = (long)(slideWidth * columnPercentages[i]);

            grid.AppendChild(new DocumentFormat.OpenXml.Drawing.GridColumn() { Width = columnWidth });
        }

        return grid;
    }


    protected DocumentFormat.OpenXml.Drawing.TableRow CreateTableHeader(System.Data.DataTable dt)
    {
        var headerRow = new DocumentFormat.OpenXml.Drawing.TableRow();
        foreach (DataColumn column in dt.Columns)
        {
            var headerCell = new DocumentFormat.OpenXml.Drawing.TableCell(
                new DocumentFormat.OpenXml.Drawing.TextBody(
                    new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                    new DocumentFormat.OpenXml.Drawing.Paragraph(
                        new DocumentFormat.OpenXml.Drawing.Run(
                            new DocumentFormat.OpenXml.Drawing.RunProperties() { FontSize = 800 },  // Set font size (600 = 12 pt)
                            new DocumentFormat.OpenXml.Drawing.Text(column.ColumnName)))));
            headerRow.AppendChild(headerCell);
        }
        return headerRow;
    }

    protected IEnumerable<DocumentFormat.OpenXml.Drawing.TableRow> CreateTableRows(System.Data.DataTable dt, int startRow, int rowCount)
    {
        for (int i = startRow; i < Math.Min(startRow + rowCount, dt.Rows.Count); i++)
        {
            var dataRow = new DocumentFormat.OpenXml.Drawing.TableRow();
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                var cellValue = dt.Rows[i].ItemArray[j].ToString();
                var dataCell = new DocumentFormat.OpenXml.Drawing.TableCell();

                // If it's the first column, apply background color and no text
                if (j == 0)
                {
                    // Set the background color based on the value of the first column
                    string cellColor = GetCellColorBasedOnValue(cellValue);

                    // Apply the background color to the first column cell
                    dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TableCellProperties(
                        new DocumentFormat.OpenXml.Drawing.SolidFill(
                            new DocumentFormat.OpenXml.Drawing.RgbColorModelHex() { Val = cellColor }
                        )
                    ));

                    // Apply font formatting (keeping the font size)
                    dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TextBody(
                        new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                        new DocumentFormat.OpenXml.Drawing.ListStyle(),
                        new DocumentFormat.OpenXml.Drawing.Paragraph()
                    ));
                }
                else
                {
                    // For other columns, print the value with font formatting (e.g., size)
                    dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TextBody(
                        new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                        new DocumentFormat.OpenXml.Drawing.ListStyle(),
                        new DocumentFormat.OpenXml.Drawing.Paragraph(
                            new DocumentFormat.OpenXml.Drawing.Run(
                                new DocumentFormat.OpenXml.Drawing.RunProperties() { FontSize = 800 },  // Set font size (800 = 16 pt)
                                new DocumentFormat.OpenXml.Drawing.Text(cellValue)
                            )
                        )
                    ));
                }

                // Append the cell to the row
                dataRow.AppendChild(dataCell);
            }
            yield return dataRow;
        }
    }


    // Helper method to get the color based on the value in the first column
    private string GetCellColorBasedOnValue(string value)
    {
        // Example: Set color based on value
        if (value == "red")
        {
            return "ff0000";
        }
        else if (value == "green")
        {
            return "008000";
        }
        else if (value == "yellow")
        {
            return "ffff00";
        }
        else if (value == "orange")
        {
            return "ffa500";
        }
        else
        {
            return "FFFFFF"; // Default: White
        }
    }

    protected IEnumerable<DocumentFormat.OpenXml.Drawing.TableRow> CreateTableRows_grid2_3(System.Data.DataTable dt, int startRow, int rowCount)
    {
        for (int i = startRow; i < Math.Min(startRow + rowCount, dt.Rows.Count); i++)
        {
            var dataRow = new DocumentFormat.OpenXml.Drawing.TableRow();
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                var cellValue = dt.Rows[i].ItemArray[j].ToString();
                var dataCell = new DocumentFormat.OpenXml.Drawing.TableCell();


                // For other columns, print the value with font formatting (e.g., size)
                dataCell.AppendChild(new DocumentFormat.OpenXml.Drawing.TextBody(
                    new DocumentFormat.OpenXml.Drawing.BodyProperties(),
                    new DocumentFormat.OpenXml.Drawing.ListStyle(),
                    new DocumentFormat.OpenXml.Drawing.Paragraph(
                        new DocumentFormat.OpenXml.Drawing.Run(
                            new DocumentFormat.OpenXml.Drawing.RunProperties() { FontSize = 800 },  // Set font size (800 = 16 pt)
                            new DocumentFormat.OpenXml.Drawing.Text(cellValue)
                        )
                    )
                ));


                // Append the cell to the row
                dataRow.AppendChild(dataCell);
            }
            yield return dataRow;
        }
    }

    protected void btnImportExcel_Click(object sender, EventArgs e)
    {
        if (fileUploadExcel.HasFile)
        {
            try
            {
                string filePath = Path.Combine(Server.MapPath("~/Uploads"), fileUploadExcel.FileName);
                fileUploadExcel.SaveAs(filePath);

                System.Data.DataTable dt = ReadExcelFile(filePath);
                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowsUpdated = UpdateDatabase(dt);
                    lblMessage.Text = rowsUpdated + " rows updated successfully!";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "No matching data found in the uploaded file.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                // Escape quotes to avoid breaking JavaScript
                string errorMessage = ex.Message.Replace("'", "\\'").Replace(Environment.NewLine, " ");

                string script = string.Format(@"
            <script type='text/javascript'>
                window.onload = function() {{
                    showToast('{0}');
                }};
            </script>", errorMessage);

                ClientScript.RegisterStartupScript(this.GetType(), "showErrorToast", script);
            }
            //catch (Exception ex)
            //{
            //    lblMessage.Text = "Error: " + ex.Message;
            //    lblMessage.ForeColor = System.Drawing.Color.Red;
            //}
        }
        else
        {
            lblMessage.Text = "Please select an Excel file.";
            lblMessage.ForeColor = System.Drawing.Color.Red;
        }

        BindGridView_design_open();
    }

    private System.Data.DataTable ReadExcelFile(string filePath)
    {
        System.Data.DataTable dt = new System.Data.DataTable();

        using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets["Report"]; // Explicitly fetch "Report"

            if (worksheet == null)
            {
                throw new Exception("Please upload a Valid Excel File. Worksheet 'Report' not found in the uploaded file.");
            }

            if (worksheet.Dimension == null)
            {
                throw new Exception("The worksheet 'Report' is empty.");
            }// Read first sheet
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column; // Get total columns

            int customerDetailIndex = -1;
            int swImageIndex = -1;
            int supportModel = -1;

            // Read the header row (Row 1)
            for (int col = 1; col <= colCount; col++)
            {
                string header = worksheet.Cells[1, col].Text.Trim();
                if (header == "HSD-ES Customer Detail")
                    customerDetailIndex = col;
                else if (header == "Software Image Freeze (IF) Planned Date")
                    swImageIndex = col;
                else if (header == "Support Model")
                    supportModel = col;
            }

            // Ensure both columns exist
            if (customerDetailIndex == -1 || swImageIndex == -1 || supportModel == -1)
            {
                throw new Exception("Required columns not found in Excel file.");
            }

            dt.Columns.Add("HSD-ES Customer Detail");
            dt.Columns.Add("Software Image Freeze (IF) Planned Date");
            dt.Columns.Add("Support Model");

            // Read data rows
            for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip headers)
            {
                DataRow dr = dt.NewRow();
                dr["HSD-ES Customer Detail"] = worksheet.Cells[row, customerDetailIndex].Text.Trim();
                dr["Software Image Freeze (IF) Planned Date"] = worksheet.Cells[row, swImageIndex].Text.Trim();
                dr["Support Model"] = worksheet.Cells[row, supportModel].Text.Trim();
                dt.Rows.Add(dr);
            }
        }

        return dt;
    }


    private int UpdateDatabase(System.Data.DataTable dt)
    {
        string basePlatform = selectedPlatform.Replace("_ALL_COMPONENTS_TABLE", "");
        string designTable = basePlatform + "_DESIGN_TABLE";

        int rowsUpdated = 0;
        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            foreach (DataRow row in dt.Rows)
            {
                string customerDetails = row["HSD-ES Customer Detail"].ToString();
                string swValue = row["Software Image Freeze (IF) Planned Date"].ToString();
                string supportMod = row["Support Model"].ToString();

                string query = "UPDATE " + designTable +
                               " SET sw_image_freeze = @SWValue, support_model = @SupportMod WHERE customer_detail = @CustomerDetails";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SWValue", swValue);
                    cmd.Parameters.AddWithValue("@CustomerDetails", customerDetails);
                    cmd.Parameters.AddWithValue("@SupportMod", supportMod);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        rowsUpdated += result;
                }
            }
        }

        return rowsUpdated;
    }

    private static readonly HashSet<string> AllowedPlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CMF_NVL_S_ALL_COMPONENTS_TABLE",
        "CMF_NVL_H_ALL_COMPONENTS_TABLE",
        "CMF_NVL_U_ALL_COMPONENTS_TABLE",
        "CMF_NVL_Hx_ALL_COMPONENTS_TABLE",
        "CMF_PTL_ALL_COMPONENTS_TABLE",
        "CMF_LNL_ALL_COMPONENTS_TABLE",
        "CMF_ARL_S_ALL_COMPONENTS_TABLE",
        "CMF_ARL_H_ALL_COMPONENTS_TABLE",
        "CMF_ARL_U_ALL_COMPONENTS_TABLE",
        "CMF_ARL_HX_ALL_COMPONENTS_TABLE",
        "CMF_GNR_ALL_COMPONENTS_TABLE",
        "CMF_WCL_ALL_COMPONENTS_TABLE",
        "CMF_ARL_Refresh_ALL_COMPONENTS_TABLE"
    };

    private string ValidatePlatformOrThrow(string platformValue)
    {
        if (string.IsNullOrWhiteSpace(platformValue) || !AllowedPlatforms.Contains(platformValue))
        {
            throw new InvalidOperationException("Invalid platform selection.");
        }

        return platformValue;
    }

    private string GetSafeCrttOutputDirectory()
    {
        string outputDirectory = Server.MapPath("~/App_Data/Crtt");
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        return outputDirectory;
    }

    private int GetCrttApiTimeoutMs()
    {
        int timeoutMs;
        return int.TryParse(ConfigurationManager.AppSettings["CrttApiTimeoutMs"], out timeoutMs) ? timeoutMs : 30000;
    }

    private string GetBasePlatformCode(string platformValue)
    {
        return (platformValue ?? string.Empty)
            .Replace("_ALL_COMPONENTS_TABLE", string.Empty)
            .Replace("CMF_", string.Empty);
    }

    private string GetMappedBasePlatform(string platformValue)
    {
        string basePlatform = GetBasePlatformCode(platformValue);

        Dictionary<string, string> basePlatformMap = new Dictionary<string, string>
        {
            { "NVL_S", "Nova Lake S" },
            { "NVL_H", "Nova Lake H" },
            { "NVL_U", "Nova Lake U" },
            { "NVL_Hx", "Nova Lake Hx" }
        };

        return basePlatformMap.ContainsKey(basePlatform) ? basePlatformMap[basePlatform] : basePlatform;
    }

    private string GetComparableDesignSummaryName(string designSummaryRecord)
    {
        if (string.IsNullOrWhiteSpace(designSummaryRecord))
        {
            return string.Empty;
        }

        int firstUnderscore = designSummaryRecord.IndexOf('_');
        if (firstUnderscore >= 0)
        {
            int secondUnderscore = designSummaryRecord.IndexOf('_', firstUnderscore + 1);
            if (secondUnderscore >= 0 && secondUnderscore < designSummaryRecord.Length - 1)
            {
                return designSummaryRecord.Substring(secondUnderscore + 1);
            }
        }

        return designSummaryRecord;
    }

    private string NormalizeDesignName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }

    private string FormatCrttDateAsWorkWeek(string actualDate)
    {
        if (string.IsNullOrWhiteSpace(actualDate))
        {
            return string.Empty;
        }

        DateTime parsedDate;
        if (!DateTime.TryParse(actualDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out parsedDate))
        {
            return actualDate;
        }

        CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        int workWeekNumber = cultureInfo.Calendar.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        int workDayNumber = ((int)parsedDate.DayOfWeek + 6) % 7 + 1;

        return string.Format("{0:D2}.{1} '{2:D2}", workWeekNumber, workDayNumber, parsedDate.Year % 100);
    }

    private string GetSwImageFreezeActualDate(string token, string designId)
    {
        if (string.IsNullOrWhiteSpace(designId))
        {
            return string.Empty;
        }

        string apiUrl = "https://crtt-api.intel.com/designSchedule/?dID=" + designId;
        string scheduleResponse = DownloadCrttApiResponse(apiUrl, token);

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;
        List<Dictionary<string, object>> scheduleItems = serializer.Deserialize<List<Dictionary<string, object>>>(scheduleResponse);

        foreach (Dictionary<string, object> scheduleItem in scheduleItems)
        {
            string milestone = scheduleItem.ContainsKey("Milestone") && scheduleItem["Milestone"] != null ? scheduleItem["Milestone"].ToString() : string.Empty;
            if (milestone.Equals("Software Image Freeze (IF)", StringComparison.OrdinalIgnoreCase))
            {
                string actualDt = scheduleItem.ContainsKey("actual_dt") && scheduleItem["actual_dt"] != null ? scheduleItem["actual_dt"].ToString() : string.Empty;
                return FormatCrttDateAsWorkWeek(actualDt);
            }
        }

        return string.Empty;
    }

    private void SyncSwImageFreezeDate(string designTable, string designSummaryRecord, string swImageFreezeActualDate)
    {
        if (string.IsNullOrWhiteSpace(designSummaryRecord) || string.IsNullOrWhiteSpace(swImageFreezeActualDate))
        {
            return;
        }

        string connectionString = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            string selectQuery = "SELECT TOP 1 sw_image_freeze FROM " + designTable + " WHERE customer_detail = @Design";
            using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
            {
                selectCmd.Parameters.AddWithValue("@Design", designSummaryRecord);
                object existingValue = selectCmd.ExecuteScalar();
                string existingSwImageFreeze = existingValue == null || existingValue == DBNull.Value ? string.Empty : existingValue.ToString().Trim();

                if (!string.Equals(existingSwImageFreeze, swImageFreezeActualDate.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    string updateQuery = "UPDATE " + designTable + " SET sw_image_freeze = @SWImageFreeze WHERE customer_detail = @Design";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                    {
                        updateCmd.Parameters.AddWithValue("@SWImageFreeze", swImageFreezeActualDate.Trim());
                        updateCmd.Parameters.AddWithValue("@Design", designSummaryRecord);
                        updateCmd.ExecuteNonQuery();
                    }

                    System.Diagnostics.Debug.WriteLine("Updated SW IF: " + designSummaryRecord + " | old=" + existingSwImageFreeze + " | new=" + swImageFreezeActualDate);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SW IF already in sync: " + designSummaryRecord + " | value=" + swImageFreezeActualDate);
                }
            }
        }
    }

    private void ExecuteCrttApiCalls(string outputDirectory)
    {
        System.Diagnostics.Debug.WriteLine("Enter ExecuteCrttApiCalls");
        if (string.IsNullOrEmpty(outputDirectory))
        {
            throw new ArgumentException("Output directory is required.", "outputDirectory");
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string currentPlatform = ValidatePlatformOrThrow(Session["selectedPlatform"] as string ?? ddlTables.SelectedValue);
        Session["MappedBasePlatform"] = GetMappedBasePlatform(currentPlatform);

        string token = GetATokenForGraph();
        Session["CrttAccessToken"] = token;

        SaveCrttApiResponse("https://crtt-api.intel.com/designSchedule/?dID=6812", token, Path.Combine(outputDirectory, "response.txt"));
        SaveCrttApiResponse("https://crtt-api.intel.com/designNames/?DesignID=2328", token, Path.Combine(outputDirectory, "designInfo.txt"));
    }

    private void GenerateBasePlatformDesignFile(string outputDirectory, string currentPlatform)
    {
        currentPlatform = ValidatePlatformOrThrow(currentPlatform);
        string designInfoPath = Path.Combine(outputDirectory, "designInfo.txt");
        if (!File.Exists(designInfoPath))
        {
            return;
        }

        string mappedBasePlatform = GetMappedBasePlatform(currentPlatform);
        string basePlatformCode = GetBasePlatformCode(currentPlatform);
        string designTable = currentPlatform.Replace("_ALL_COMPONENTS_TABLE", "_DESIGN_TABLE");

        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;
        List<Dictionary<string, object>> apiDesigns = serializer.Deserialize<List<Dictionary<string, object>>>(File.ReadAllText(designInfoPath));

        List<Dictionary<string, string>> matchingApiDesigns = new List<Dictionary<string, string>>();
        foreach (Dictionary<string, object> apiDesign in apiDesigns)
        {
            string platformName = apiDesign.ContainsKey("PlatformName") && apiDesign["PlatformName"] != null ? apiDesign["PlatformName"].ToString() : string.Empty;
            if (platformName.IndexOf(mappedBasePlatform, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matchingApiDesigns.Add(new Dictionary<string, string>
                {
                    { "DesignID", apiDesign.ContainsKey("DesignID") && apiDesign["DesignID"] != null ? apiDesign["DesignID"].ToString() : string.Empty },
                    { "DesignName", apiDesign.ContainsKey("DesignName") && apiDesign["DesignName"] != null ? apiDesign["DesignName"].ToString() : string.Empty },
                    { "PlatformName", platformName }
                });
            }
        }

        List<string> outputLines = new List<string>();
        List<string> designSummaryRecords = new List<string>();

        foreach (GridViewRow row in GridView_design_summary.Rows)
        {
            if (row.RowType != DataControlRowType.DataRow || row.Cells.Count == 0)
            {
                continue;
            }

            string designSummaryRecord = HttpUtility.HtmlDecode(row.Cells[0].Text).Trim();
            if (!string.IsNullOrEmpty(designSummaryRecord) && !designSummaryRecord.Equals("unassigned", StringComparison.OrdinalIgnoreCase))
            {
                designSummaryRecords.Add(designSummaryRecord);
            }
        }

        System.Diagnostics.Debug.WriteLine("Design summary displayed record count: " + designSummaryRecords.Count);

        foreach (Dictionary<string, string> apiDesign in matchingApiDesigns)
        {
            string designName = apiDesign["DesignName"];
            if (string.IsNullOrEmpty(designName))
            {
                continue;
            }

            string normalizedApiDesignName = NormalizeDesignName(designName);

            foreach (string designSummaryRecord in designSummaryRecords)
            {
                string comparableDesignSummaryName = GetComparableDesignSummaryName(designSummaryRecord);
                string normalizedDesignSummaryName = NormalizeDesignName(comparableDesignSummaryName);

                if (normalizedDesignSummaryName == normalizedApiDesignName)
                {
                    string line = designName + " | " + designSummaryRecord + " | " + apiDesign["PlatformName"];
                    if (!outputLines.Contains(line))
                    {
                        outputLines.Add(line);

                        string token = Session["CrttAccessToken"] as string;
                        string swImageFreezeActualDate = GetSwImageFreezeActualDate(token, apiDesign["DesignID"]);
                        System.Diagnostics.Debug.WriteLine("Matched record: " + line + " | actual_dt=" + swImageFreezeActualDate);
                        SyncSwImageFreezeDate(designTable, designSummaryRecord, swImageFreezeActualDate);
                    }
                }
            }
        }

    }

    private string GetATokenForGraph()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

        string azureAppId = ConfigurationManager.AppSettings["CrttAzureAppId"];
        string azureTenantId = ConfigurationManager.AppSettings["CrttAzureTenantId"];
        string userEmail = ConfigurationManager.AppSettings["CrttUserEmail"];
        string password = ConfigurationManager.AppSettings["CrttPassword"];

        if (string.IsNullOrWhiteSpace(azureAppId) || string.IsNullOrWhiteSpace(azureTenantId) || string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("CRTT auth app settings are not configured.");
        }

        string tokenEndpoint = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", azureTenantId);
        string postData = string.Format(
            "client_id={0}&scope={1}&username={2}&password={3}&grant_type=password",
            Uri.EscapeDataString(azureAppId),
            Uri.EscapeDataString(azureAppId + "/.default"),
            Uri.EscapeDataString(userEmail),
            Uri.EscapeDataString(password));

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.Timeout = GetCrttApiTimeoutMs();
        request.ReadWriteTimeout = GetCrttApiTimeoutMs();

        byte[] payload = Encoding.UTF8.GetBytes(postData);
        request.ContentLength = payload.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(payload, 0, payload.Length);
        }

        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string tokenResponse = reader.ReadToEnd();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> tokenPayload = serializer.Deserialize<Dictionary<string, object>>(tokenResponse);

                if (tokenPayload != null && tokenPayload.ContainsKey("access_token") && tokenPayload["access_token"] != null)
                {
                    return tokenPayload["access_token"].ToString();
                }

                throw new InvalidOperationException("Access token was not returned by Azure AD.");
            }
        }
        catch (WebException ex)
        {
            throw new InvalidOperationException("Failed to acquire CRTT access token.", ex);
        }
    }

    private string DownloadCrttApiResponse(string apiUrl, string token)
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
        request.Method = "GET";
        request.Timeout = GetCrttApiTimeoutMs();
        request.ReadWriteTimeout = GetCrttApiTimeoutMs();
        request.Headers[HttpRequestHeader.Authorization] = string.Format("Bearer {0}", token);

        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
        catch (WebException ex)
        {
            throw new InvalidOperationException("CRTT API request failed for " + apiUrl, ex);
        }
    }

    private void SaveCrttApiResponse(string apiUrl, string token, string outputPath)
    {
        string response = DownloadCrttApiResponse(apiUrl, token);
        File.WriteAllText(outputPath, response);
    }

}
