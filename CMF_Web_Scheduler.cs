﻿//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Configuration;
//using System.Data.SqlClient;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Runtime.Serialization.Json;
//using System.Text;
//using System.Threading.Tasks;
//using System.Runtime.InteropServices.ComTypes;
//using System.Net.Http.Headers;
//using System.Net.Http;
//using System.Drawing.Imaging;
//using System.Collections;
//using System.Diagnostics;

//namespace SchedulerTest
//{
//    internal class Program
//    {
//        static DataTable dt4;
//        static SqlCommand checkexists;
//        static int ifexists;
//        static string strconnstring;
//        static List<string> cpIdsFromApi;
//        static string idsToKeep;
//        static string deleteQuery;
//        static string CreateQueryRequest(string eql)
//        {
//            System.Guid myTranId = new System.Guid();
//            string postData = "{\"requests\":[ {\"tran_id\": \"" + myTranId + "\"," +
//                "\"command\": \"get_records_by_eql\",\"command_args\": {\"eql\": \""
//                + eql + "\"," + "\"count\": \"999999\"},";
//            string varArgs = "\"var_args\": [] } ] }";
//            string jqryData = postData + varArgs;
//            System.Diagnostics.Debug.Write(jqryData);
//            //{"requests":[ {"tran_id": "00000000-0000-0000-0000-000000000000","command": "get_records_by_eql","command_args": {"eql": "select id, status, reason, title where tenant = 'ip_sw_graphics' and subject = 'bug'"},"var_args": [] } ] }
//            //string jqq = jqryData;
//            return jqryData;

//        }



//        static Responses SubmitRequest(string requestData)
//        {
//            string _uri = "https://hsdes-api.intel.com/ws/ESService"; // Your URI here
//            HttpClientHandler handler = new HttpClientHandler();
//            handler.UseDefaultCredentials = true;

//            var httpClient = new HttpClient(handler);
//            var requestUri = new Uri(_uri);

//            var content = new StringContent(requestData);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

//            HttpResponseMessage response = httpClient.PostAsync(requestUri, content).GetAwaiter().GetResult();

//            if (!response.IsSuccessStatusCode)
//            {
//                throw new HttpRequestException($"Server error (HTTP {(int)response.StatusCode}: {response.ReasonPhrase}).");
//            }

//            using (var responseStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
//            {
//                var jsonSerializer = new DataContractJsonSerializer(typeof(Responses));
//                return (Responses)jsonSerializer.ReadObject(responseStream);
//            }
//        }
//        static DataTable GetDatatableFromJson(IList<Result> list)
//        {
//            PropertyDescriptorCollection props =
//        TypeDescriptor.GetProperties(typeof(Result));
//            DataTable table = new DataTable();
//            for (int i = 0; i < props.Count; i++)
//            {
//                PropertyDescriptor prop = props[i];
//                table.Columns.Add(prop.Name, prop.PropertyType);
//            }
//            object[] values_get = new object[props.Count];
//            foreach (Result item in list)
//            {
//                for (int i = 0; i < values_get.Length; i++)
//                {
//                    values_get[i] = props[i].GetValue(item);
//                }
//                table.Rows.Add(values_get);
//            }
//            return table;
//        }

//        //    private static void TestAvailableSystems()
//        //    {
//        //        // Test with the same syntax pattern as your working main query
//        //        string[] testQueries = {
//        //    // Test client_platf.bug first (we know this should work)
//        //    "select client_platf.bug.promoted_id as 'promoted_id' from client_platf.bug",

//        //    // Test HSD systems with similar syntax
//        //    "select sighting_central.sighting.owner as 'owner' from sighting_central.sighting",
//        //    "select IP_SW_GRAPHICS.owner as 'owner' from IP_SW_GRAPHICS",
//        //    "select sfip.sighting.owner as 'owner' from sfip.sighting",
//        //    "select central_firmware.bug.owner as 'owner' from central_firmware.bug",
//        //    "select platf_win_os.bug.owner as 'owner' from platf_win_os.bug",
//        //    "select client_conn_ip.owner as 'owner' from client_conn_ip",
//        //    "select ip_fw_sw_sensing.bug.owner as 'owner' from ip_fw_sw_sensing.bug"
//        //};

//        //        foreach (string query in testQueries)
//        //        {
//        //            try
//        //            {
//        //                Console.WriteLine($"Testing: {query}");

//        //                var response = SubmitRequest(CreateQueryRequest(query));
//        //                string status = response?.responses?[0]?.status ?? "null";

//        //                Console.WriteLine($"Status: {status}");
//        //                if (status.ToLower() == "success")
//        //                {
//        //                    var results = response.responses[0].result_table;
//        //                    Console.WriteLine($"? SUCCESS - Query works! Result count: {results?.Count}");
//        //                }
//        //                else
//        //                {
//        //                    Console.WriteLine($"? FAILED - Query failed");
//        //                }
//        //                Console.WriteLine("---");
//        //            }
//        //            catch (Exception ex)
//        //            {
//        //                Console.WriteLine($"? EXCEPTION: {ex.Message}");
//        //                Console.WriteLine("---");
//        //            }
//        //        }
//        //    }

//        private static void TestBasicSystemAccess()
//        {
//            // Test the most basic queries possible
//            string[] basicQueries = {
//        "select count(*) from client_platf.bug",  // This works from your main query
//        "select count(*) from sighting_central.sighting",
//        "select count(*) from IP_SW_GRAPHICS",
//        "select count(*) from sfip.sighting",
//        "select count(*) from central_firmware.bug",
//        "select count(*) from platf_win_os.bug",
//        "select count(*) from client_conn_ip",
//        "select count(*) from ip_fw_sw_sensing.bug"
//    };

//            string[] systemNames = {
//        "client_platf.bug (WORKING)",
//        "sighting_central.sighting",
//        "IP_SW_GRAPHICS",
//        "sfip.sighting",
//        "central_firmware.bug",
//        "platf_win_os.bug",
//        "client_conn_ip",
//        "ip_fw_sw_sensing.bug"
//    };

//            for (int i = 0; i < basicQueries.Length; i++)
//            {
//                try
//                {
//                    Console.WriteLine($"\n=== Testing {systemNames[i]} ===");
//                    Console.WriteLine($"Query: {basicQueries[i]}");

//                    var response = SubmitRequest(CreateQueryRequest(basicQueries[i]));
//                    string status = response?.responses?[0]?.status ?? "null";

//                    Console.WriteLine($"Status: {status}");

//                    if (status.ToLower() == "success")
//                    {
//                        Console.WriteLine($"? SUCCESS - System accessible!");
//                    }
//                    else
//                    {
//                        Console.WriteLine($"? FAILED - Cannot access system");
//                        // Print more detailed error info
//                        if (response?.responses?.Count > 0)
//                        {
//                            Console.WriteLine($"Error details: {response.responses[0].status}");
//                            if (response.responses[0].result_table != null)
//                            {
//                                Console.WriteLine($"Result table count: {response.responses[0].result_table.Count}");
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"? EXCEPTION: {ex.Message}");
//                }
//                Console.WriteLine("---");
//            }
//        }

//        //private static string GetOwnerFromPromotedHSD(string promotedId, string componentGroup)
//        //{
//        //    if (string.IsNullOrEmpty(promotedId))
//        //        return null;

//        //    string cleanPromotedId = ExtractNumericId(promotedId);
//        //    if (string.IsNullOrEmpty(cleanPromotedId))
//        //        return null;

//        //    // Add specific debugging for the problematic ID
//        //    bool isProblematicId = cleanPromotedId == "15018185865";
//        //    if (isProblematicId)
//        //    {
//        //        Console.WriteLine($"=== DEBUGGING ID 15018185865 ===");
//        //        Console.WriteLine($"Original promotedId: {promotedId}");
//        //        Console.WriteLine($"Cleaned promotedId: {cleanPromotedId}");
//        //        Console.WriteLine($"Component Group: {componentGroup}");
//        //    }

//        //    string hsdQuery = "";

//        //    try
//        //    {
//        //        // Determine which HSD system to query based on component group
//        //        //switch (componentGroup?.ToUpper())
//        //        //{
//        //        //    case "HW":
//        //        //        hsdQuery = $"select sighting_central.sighting.owner as 'owner', sighting_central.sighting.id as 'id' where sighting_central.sighting.id = '{cleanPromotedId}'";
//        //        //        break;
//        //        //        // ... other cases remain the same
//        //        //}

//        //        switch (componentGroup?.ToUpper())
//        //        {
//        //            case "HW":
//        //                hsdQuery = $"select sighting_central.sighting.owner as 'owner', sighting_central.sighting.id as 'id' where sighting_central.sighting.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "audio":
//        //                hsdQuery = $"select sfip.sighting.owner as 'owner', sfip.sighting.id as 'id' where sfip.sighting.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "GFX":
//        //                hsdQuery = $"select ip_sw_graphics.bug.owner as 'owner', ip_sw_graphics.bug.id as 'id' where ip_sw_graphics.bug.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "iGFX":
//        //                hsdQuery = $"select ip_sw_graphics.bug.owner as 'owner', ip_sw_graphics.bug.id as 'id' where ip_sw_graphics.bug.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "sensor":
//        //                hsdQuery = $"select ip_fw_sw_sensing.bug.owner as 'owner', ip_fw_sw_sensing.bug.id as 'id' where ip_fw_sw_sensing.bug.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "BIOS":
//        //                hsdQuery = $"select central_firmware.bug.owner as 'owner', central_firmware.bug.id as 'id' where central_firmware.bug.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "OS":
//        //                hsdQuery = $"select platf_win_os.bug.owner as 'owner', platf_win_os.bug.id as 'id' where platf_win_os.bug.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "TBT":
//        //                hsdQuery = $"select client_conn_ip.owner as 'owner', client_conn_ip.id as 'id' where client_conn_ip.id = '{cleanPromotedId}'";
//        //                break;
//        //            case "TCSS":
//        //                hsdQuery = $"select client_conn_ip.owner as 'owner', client_conn_ip.id as 'id' where client_conn_ip.id = '{cleanPromotedId}'";
//        //                break;
//        //            default:
//        //                return null;
//        //        }

//        //        if (isProblematicId)
//        //        {
//        //            Console.WriteLine($"Generated Query: {hsdQuery}");
//        //        }

//        //        if (!string.IsNullOrEmpty(hsdQuery))
//        //        {
//        //            // Make API call to get owner from HSD
//        //            var hsdApiResponse = SubmitRequest(CreateQueryRequest(hsdQuery));

//        //            if (isProblematicId)
//        //            {
//        //                Console.WriteLine($"API Response received: {hsdApiResponse != null}");
//        //                Console.WriteLine($"Response count: {hsdApiResponse?.responses?.Count ?? 0}");
//        //                if (hsdApiResponse?.responses?.Count > 0)
//        //                {
//        //                    Console.WriteLine($"Response status: {hsdApiResponse.responses[0].status}");
//        //                    Console.WriteLine($"Result table count: {hsdApiResponse.responses[0].result_table?.Count ?? 0}");
//        //                }
//        //            }

//        //            if (hsdApiResponse?.responses?.Count > 0 &&
//        //                hsdApiResponse.responses[0].status.ToLower() == "success")
//        //            {
//        //                IList<Result> hsdResults = hsdApiResponse.responses[0].result_table;

//        //                if (hsdResults?.Count > 0)
//        //                {
//        //                    // Convert to DataTable like your main code does
//        //                    DataTable hsdDataTable = GetDatatableFromJson(hsdResults);

//        //                    if (isProblematicId)
//        //                    {
//        //                        Console.WriteLine($"DataTable created with {hsdDataTable.Rows.Count} rows");
//        //                        Console.WriteLine($"Columns: {string.Join(", ", hsdDataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");

//        //                        if (hsdDataTable.Rows.Count > 0)
//        //                        {
//        //                            Console.WriteLine("First row data:");
//        //                            foreach (DataColumn col in hsdDataTable.Columns)
//        //                            {
//        //                                var value = hsdDataTable.Rows[0][col.ColumnName];
//        //                                Console.WriteLine($"  {col.ColumnName}: '{value}' (Type: {value?.GetType()?.Name ?? "null"})");
//        //                            }
//        //                        }
//        //                    }

//        //                    // Since we're querying by specific ID, we should get the owner from the first result
//        //                    if (hsdDataTable.Rows.Count > 0 && hsdDataTable.Columns.Contains("owner"))
//        //                    {
//        //                        string owner = hsdDataTable.Rows[0]["owner"].ToString();

//        //                        if (isProblematicId)
//        //                        {
//        //                            Console.WriteLine($"Raw owner value: '{owner}'");
//        //                            Console.WriteLine($"Owner is null or empty: {string.IsNullOrEmpty(owner)}");
//        //                            Console.WriteLine($"Owner after trim: '{owner?.Trim()}'");
//        //                        }

//        //                        return string.IsNullOrEmpty(owner) ? "no owner assigned" : owner;
//        //                    }
//        //                    else if (isProblematicId)
//        //                    {
//        //                        Console.WriteLine("Either no rows found or 'owner' column missing");
//        //                    }
//        //                }
//        //                else if (isProblematicId)
//        //                {
//        //                    Console.WriteLine("No results returned from HSD query");
//        //                }
//        //            }
//        //            else if (isProblematicId)
//        //            {
//        //                Console.WriteLine("API call was not successful");
//        //                if (hsdApiResponse?.responses?.Count > 0)
//        //                {
//        //                    Console.WriteLine($"Error status: {hsdApiResponse.responses[0].status}");
//        //                    // If there's an error message field, log it too
//        //                    Console.WriteLine($"Full response: {hsdApiResponse.responses[0].ToString()}");
//        //                    //Console.WriteLine($"Full response: {System.Text.Json.JsonSerializer.Serialize(hsdApiResponse.responses[0])}");
//        //                }
//        //            }
//        //        }
//        //        else if (isProblematicId)
//        //        {
//        //            Console.WriteLine("HSD Query is null or empty");
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        if (isProblematicId)
//        //        {
//        //            Console.WriteLine($"Exception for ID 15018185865: {ex.Message}");
//        //            Console.WriteLine($"Stack trace: {ex.StackTrace}");
//        //        }
//        //        return "no owner assigned";
//        //    }

//        //    if (isProblematicId)
//        //    {
//        //        Console.WriteLine("=== END DEBUG FOR 15018185865 ===");
//        //    }

//        //    return "no owner assigned";
//        //}

//        private static string GetOwnerFromPromotedHSD(string promotedId, string componentGroup)
//        {
//            if (promotedId == "16029019981" || promotedId == "15018596196")
//            {
//                int a = 1;
//                //check;
//            }
//            if (string.IsNullOrEmpty(promotedId))
//                return null;

//            string cleanPromotedId = ExtractNumericId(promotedId);
//            if (string.IsNullOrEmpty(cleanPromotedId))
//                return null;

//            string hsdQuery = "";

//            try
//            {
//                switch (componentGroup?.ToUpper())
//                {
//                    //case "HW":
//                    //    hsdQuery = $"select sighting_central.sighting.owner as 'owner', sighting_central.sighting.id as 'id' where sighting_central.sighting.id = '{cleanPromotedId}'";
//                    //    break;
//                    case "HW":
//                        {
//                            // 1) Primary tenant
//                            string q1 =
//                                $"select sighting_central.sighting.owner as 'owner', sighting_central.sighting.id as 'id' " +
//                                $"where sighting_central.sighting.id = '{cleanPromotedId}'";

//                            var r1 = SubmitRequest(CreateQueryRequest(q1));
//                            bool hasPrimary = r1?.responses?[0]?.result_table?.Count > 0;

//                            if (hasPrimary)
//                            {
//                                hsdQuery = q1;
//                                break;
//                            }

//                            // 2) Fallback tenant
//                            hsdQuery =
//                                $"select heia_soc.bugeco.owner as 'owner', heia_soc.bugeco.id as 'id' " +
//                                $"where heia_soc.bugeco.id = '{cleanPromotedId}'";

//                            break;
//                        }
//                    case "AUDIO":
//                        hsdQuery = $"select sfip.sighting.owner as 'owner', sfip.sighting.id as 'id' where sfip.sighting.id = '{cleanPromotedId}'";
//                        break;
//                    case "GFX":
//                        hsdQuery = $"select ip_sw_graphics.bug.owner as 'owner', ip_sw_graphics.bug.id as 'id' where ip_sw_graphics.bug.id = '{cleanPromotedId}'";
//                        break;
//                    case "IGFX":
//                        hsdQuery = $"select ip_sw_graphics.bug.owner as 'owner', ip_sw_graphics.bug.id as 'id' where ip_sw_graphics.bug.id = '{cleanPromotedId}'";
//                        break;
//                    case "IPF":
//                        hsdQuery = $"select ip_sw_graphics.bug.owner as 'owner', ip_sw_graphics.bug.id as 'id' where ip_sw_graphics.bug.id = '{cleanPromotedId}'";
//                        break;
//                    case "SENSOR":
//                        hsdQuery = $"select ip_fw_sw_sensing.bug.owner as 'owner', ip_fw_sw_sensing.bug.id as 'id' where ip_fw_sw_sensing.bug.id = '{cleanPromotedId}'";
//                        break;
//                    case "BIOS":
//                        {
//                            // Try primary tenant first
//                            string q1 = $"select central_firmware.bug.owner as 'owner', central_firmware.bug.id as 'id' where central_firmware.bug.id = '{cleanPromotedId}'";
//                            var r1 = SubmitRequest(CreateQueryRequest(q1));

//                            bool hasPrimary = r1?.responses?[0]?.result_table?.Count > 0;

//                            if (hasPrimary)
//                            {
//                                hsdQuery = q1;
//                                break;
//                            }

//                            // Fallback tenant
//                            hsdQuery =
//                               $"select platf_win_os.bug.owner as 'owner', platf_win_os.bug.id as 'id' where platf_win_os.bug.id = '{cleanPromotedId}'";
//                            break;
//                        }
//                    //hsdQuery = $"select central_firmware.bug.owner as 'owner', central_firmware.bug.id as 'id' where central_firmware.bug.id = '{cleanPromotedId}'";
//                    //break;
//                    //case "OS":
//                    //    hsdQuery = $"select platf_win_os.bug.owner as 'owner', platf_win_os.bug.id as 'id' where platf_win_os.bug.id = '{cleanPromotedId}'";
//                    //    break;
//                    case "OS":
//                        {
//                            // Try primary tenant first
//                            string q1 = $"select platf_win_os.bug.owner as 'owner', platf_win_os.bug.id as 'id' where platf_win_os.bug.id = '{cleanPromotedId}'";
//                            var r1 = SubmitRequest(CreateQueryRequest(q1));

//                            bool hasPrimary = r1?.responses?[0]?.result_table?.Count > 0;

//                            if (hasPrimary)
//                            {
//                                hsdQuery = q1;
//                                break;
//                            }

//                            // Fallback tenant
//                            hsdQuery =
//                                $"select sighting_central.sighting.owner as 'owner', sighting_central.sighting.id as 'id' where sighting_central.sighting.id = '{cleanPromotedId}'";
//                            break;
//                        }
//                    case "TBT":
//                        hsdQuery = $"select client_conn_ip.owner as 'owner', client_conn_ip.id as 'id' where client_conn_ip.id = '{cleanPromotedId}'";
//                        break;
//                    case "TCSS (excl TBT)":
//                        hsdQuery = $"select client_conn_ip.owner as 'owner', client_conn_ip.id as 'id' where client_conn_ip.id = '{cleanPromotedId}'";
//                        break;
//                    case "LAN":
//                        hsdQuery = $"select client_conn_ip.sighting.owner as 'owner', client_conn_ip.sighting.id as 'id' where client_conn_ip.sighting.id = '{cleanPromotedId}'";
//                        break;
//                    case "MEMORY":
//                        hsdQuery = $"select sighting_central.sighting.owner as 'owner', sighting_central.sighting.id as 'id' where sighting_central.sighting.id = '{cleanPromotedId}'";
//                        break;
//                    default:
//                        return null;
//                }

//                if (!string.IsNullOrEmpty(hsdQuery))
//                {

//                    var hsdApiResponse = SubmitRequest(CreateQueryRequest(hsdQuery));
//                    if (promotedId == "16029019981" || promotedId == "15018596196")
//                    {
//                        Console.WriteLine($"API Response Status: {hsdApiResponse?.responses?[0]?.status}");
//                        Console.WriteLine($"Result Count: {hsdApiResponse?.responses?[0]?.result_table?.Count ?? 0}");
//                    }



//                    if (hsdApiResponse?.responses?.Count > 0 &&
//                        hsdApiResponse.responses[0].status.ToLower() == "success")
//                    {
//                        IList<Result> hsdResults = hsdApiResponse.responses[0].result_table;

//                        if (hsdResults?.Count > 0)
//                        {
//                            DataTable hsdDataTable = GetDatatableFromJson(hsdResults);

//                            if (hsdDataTable.Rows.Count > 0 && hsdDataTable.Columns.Contains("owner"))
//                            {
//                                string owner = hsdDataTable.Rows[0]["owner"].ToString();
//                                return string.IsNullOrEmpty(owner) ? "no owner assigned" : owner;
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return "no owner assigned";
//            }

//            return "no owner assigned";
//        }

//        // Helper method to extract numeric ID from complex promoted_id strings
//        private static string ExtractNumericId(string promotedId)
//        {
//            if (string.IsNullOrEmpty(promotedId))
//                return "";

//            // Handle special cases
//            if (promotedId.ToUpper() == "NA" || promotedId.ToUpper() == "N/A")
//                return "";

//            // If it contains URLs, try to extract ID from URL
//            if (promotedId.Contains("hsdes.intel.com") || promotedId.Contains("jira.idoc.intel.com"))
//            {
//                var match = System.Text.RegularExpressions.Regex.Match(promotedId, @"\d{10,}");
//                return match.Success ? match.Value : "";
//            }

//            // If it contains multiple IDs or descriptive text, extract the first long number
//            var numericMatch = System.Text.RegularExpressions.Regex.Match(promotedId, @"\d{10,}");
//            if (numericMatch.Success)
//                return numericMatch.Value;

//            // If it's already a clean numeric ID
//            if (System.Text.RegularExpressions.Regex.IsMatch(promotedId, @"^\d+$"))
//                return promotedId;

//            return "";
//        }

//        static void Main(string[] args)
//        {

//            string cs = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;
//            SqlConnection con1;
//            con1 = new SqlConnection(cs);
//            con1.Open();

//            Console.WriteLine("=== TESTING AVAILABLE SYSTEMS ===");
//            TestBasicSystemAccess();
//            Console.WriteLine("=== END OF TESTS ===\n");

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            int ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max), sysdebug_forum nvarchar(max))";
//                //,third_party_classification nvarchar(max)

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            int columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                // Table doesn't exist, create it with the new column
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max),customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime, third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max), sysdebug_forum nvarchar(max), owners_name nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max), sysdebug_forum nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max),customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max), sysdebug_forum nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            checkexists.CommandType = CommandType.Text;
//            checkexists.Connection = con1;
//            ifexists = (int)checkexists.ExecuteScalar();

//            if (ifexists == 0)
//            {
//                createTable += "customer_company nvarchar(max), submitted_date datetime, implemented_date datetime,";
//                createTable += "updated_date datetime, closed_date datetime, component nvarchar(max), release nvarchar(max),";
//                createTable += "reason nvarchar(max), repro_on_rvp nvarchar(max), resolution_type nvarchar(max), cmf_request nvarchar(max), must_fix_for nvarchar(max),report_type nvarchar(max), closed_reason nvarchar(max),";
//                createTable += "ext_additional_info nvarchar(max), customer_detail nvarchar(max), customer_owner nvarchar(max), submitter_org nvarchar(max), days_open nvarchar(max), ww_submitted nvarchar(max),";
//                createTable += "customer_affected nvarchar(max), customer_impact nvarchar(max), priority nvarchar(max),submitted_by nvarchar(max), regression nvarchar(max), location nvarchar(max), date_debug datetime,  third_party_classification nvarchar(max),family_affected nvarchar(max), promoted_status nvarchar(max), promoted_id nvarchar(max), component_group varchar(50), days_active nvarchar(20), progress nvarchar(max), cmf_status nvarchar(max), status_updated_by nvarchar(max), status_updated_on datetime, idst nvarchar(max), los nvarchar(max), drivers nvarchar(max), merge_id nvarchar(max), date_cmf_ask datetime, date_cmf_decided datetime, impact nvarchar(max), fixed_in_version nvarchar(max), me_sku nvarchar(max), sysdebug nvarchar(max))";

//                SqlCommand create = new SqlCommand(createTable);
//                create.CommandType = CommandType.Text;
//                create.Connection = con1;
//                create.ExecuteScalar();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }

//            // Always check for the column and add it if it doesn't exist (regardless of whether table was just created or already existed)
//            checkColumn.CommandType = CommandType.Text;
//            checkColumn.Connection = con1;
//            columnExists = (int)checkColumn.ExecuteScalar();

//            if (columnExists == 0)
//            {
//                SqlCommand alter = new SqlCommand(alterTable);
//                alter.CommandType = CommandType.Text;
//                alter.Connection = con1;
//                alter.ExecuteNonQuery();
//            }


//            con1.Close();









//            //    client_platf.bug.promoted_id as 'promoted_id', 
//            //    client_platf.bug.promoted_status as 'promoted_status', 
//            //    client_platf.bug.customer_company as 'customer_company',
//            //    client_platf.bug.family as 'family', 
//            //    client_platf.bug.how_found_category as 'how_found_category', 
//            //    client_platf.bug.report_type AS 'report_type', 
//            //    client_platf.bug.closed_reason AS 'closed_reason', 
//            //    client_platf.bug.release AS 'release', 
//            //    client_platf.bug.component AS 'component', 
//            //    client_platf.bug.submitter_org AS 'submitter_org', 
//            //    client_platf.bug.platform AS 'platform', 
//            //    client_platf.bug.resolution_type AS 'resolution_type',
//            //    bug.operating_system,
//            //    client_platf.bug.customer_detail as 'customer_detail',
//            //    client_platf.bug.status as 'status',
//            //    client_platf.bug.regression as 'regression', 
//            //    client_platf.bug.submitted_date as 'submitted_date',
//            //    client_platf.bug.implemented_date as 'implemented_date',
//            //    client_platf.bug.updated_date as 'updated_date',
//            //    client_platf.bug.closed_date as 'closed_date',
//            //    client_platf.bug.reason as 'reason',
//            //    client_platf.bug.repro_on_rvp as 'repro_on_rvp', 
//            //    client_platf.bug.cmf_request as 'cmf_request', 
//            //    client_platf.bug.must_fix_for as 'must_fix_for',
//            //    client_platf.bug.title as 'title',
//            //    client_platf.bug.customer_affected as 'customer_affected', 
//            //    client_platf.bug.customer_impact as 'customer_impact', 
//            //    client_platf.bug.priority as 'priority',
//            //    client_platf.bug.location as 'location',
//            //    client_platf.bug.id as 'id',
//            //    client_platf.bug.date_debug as 'date_debug',
//            //    client_platf.bug.3rd_party_classification AS '3rd_party_classification',
//            //    client_platf.bug.odm as 'odm',
//            //    client_platf.bug.owner as 'owner',
//            //    client_platf.bug.family_affected as 'family_affected', 
//            //    client_platf.bug.merge_id as 'merge_id', 
//            //    client_platf.bug.progress as 'progress', 
//            //    client_platf.bug.date_cmf_ask as 'date_cmf_ask', 
//            //    client_platf.bug.date_cmf_decided as 'date_cmf_decided', 
//            //    client_platf.bug.impact as 'impact', 
//            //    client_platf.bug.fixed_in_version as 'fixed_in_version', 
//            //    client_platf.bug.me_sku as 'me_sku', 
//            //    client_platf.bug.sysdebug as 'sysdebug', 
//            //    client_platf.bug.sysdebug_forum as 'sysdebug_forum', 
//            //    client_platf.bug.component_group as 'component_group',
//            //    case 
//            //        when client_platf.bug.component_group = 'HW' then (select owner from sighting_central.sighting where id = client_platf.bug.promoted_id)
//            //        when client_platf.bug.component_group = 'Audio' then (select owner from sfip.sighting where id = client_platf.bug.promoted_id)
//            //        when client_platf.bug.component_group = 'GFX' then (select owner from IP_SW_GRAPHICS where id = client_platf.bug.promoted_id)
//            //        when client_platf.bug.component_group = 'Sensor' then (select owner from ip_fw_sw_sensing.bug where id = client_platf.bug.promoted_id)
//            //        when client_platf.bug.component_group = 'BIOS' then (select owner from central_firmware.bug where id = client_platf.bug.promoted_id)
//            //        when client_platf.bug.component_group = 'OS' then (select owner from platf_win_os.bug where id = client_platf.bug.promoted_id)
//            //        when client_platf.bug.component_group = 'TBT/TCSS' then (select owner from client_conn_ip where id = client_platf.bug.promoted_id)
//            //        else null 
//            //    end as 'owner_name' 
//            //from client_platf.bug
//            //    AND (client_platf.bug.tag contains ('cmf_ask') 
//            //        OR (client_platf.bug.cmf_request in ('cmf_ok', 'cmf_duplicate', 'cmf_ask', 'cmf_ack', 'cmf_incomplete', 'cmf_reject')) 
//            //        OR client_platf.bug.cmf_request in ('cmf_ok', 'cmf_duplicate', 'cmf_ask', 'cmf_ack', 'cmf_incomplete', 'cmf_reject')) 
//            //    AND (client_platf.bug.submitter_org contains ('ccg_cce') 
//            //        OR client_platf.bug.submitter_org contains ('windows_platform_validation')))";









//            string strconnstring = ConfigurationManager.ConnectionStrings["gfxitt"].ConnectionString;

//            //apiresponse_rpl = SubmitRequest(CreateQueryRequest(query2));


//            DataTable dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        if (index + 2 <= mustFixFor.Length)
//                                        {
//                                            if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                            {
//                                                mustFixFor = "Pre-PV";
//                                            }
//                                        }

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    //}
//                                    //else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.
//                                    //    Rows[i]["progress"].ToString());
//                                    //}
//                                    //else
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());


//                                    //}
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Client_Win11_SV3 - 2024#")
//                                    {
//                                        comp_group = "OS";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "iGFX";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "security";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "camera";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "Modern Standby";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }


//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }
//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        if (index + 2 <= mustFixFor.Length)
//                                        {
//                                            if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                            {
//                                                mustFixFor = "Pre-PV";
//                                            }
//                                        }

//                                        cmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());


//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());


//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);

//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());

//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Client_Win11_SV3 - 2024#")
//                                    {
//                                        comp_group = "OS";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "iGFX";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "security";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "camera";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "Modern Standby";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    // Set color code based on status

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }

//                        }
//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            List<string> cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            string idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }

//            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//            ///


//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();

//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "IOMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.DTT")
//                                    {
//                                        comp_group = "DTT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "Kevin, Manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok and Bao, Xulie and Lee, jony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());



//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "IOMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "Kevin, Manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok Bao, Xulie and Lee, jony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            ///////////////////////////
//            ///
//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }


//            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();

//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "IOMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.DTT")
//                                    {
//                                        comp_group = "DTT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "Kevin, Manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok and Bao, Xulie and Lee, jony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());



//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "IOMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "Kevin, Manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok Bao, Xulie and Lee, jony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            ///////////////////////////
//            ///
//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }

//            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    //if (cp_id == "14025924127")
//                                    //{
//                                    //    Console.WriteLine("Found the specific CP ID!");
//                                    //}
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    //}
//                                    //else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //}
//                                    //else
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    //}
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);


//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;

//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    //if(cp_id == "14025924127")
//                                    //{
//                                    //    Console.WriteLine("Found the specific CP ID!");
//                                    //}
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);

//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());


//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    else
//                                        cmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // If there's a promoted_id, fetch owner from HSD via API
//                                    if (!string.IsNullOrEmpty(promotedId))
//                                    {
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    }

//                                    // If no owner found from promoted HSD, use CP owner as fallback
//                                    if (string.IsNullOrEmpty(ownersName))
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    }
//                                    else
//                                    {
//                                        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }



//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    if (dt_temp.Rows.Count > 0)
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //14

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());


//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);

//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());

//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);
//                                    if (dt_temp.Rows.Count > 0)
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    else
//                                        cmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }

//                        }
//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }



//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    if (dt_temp.Rows.Count > 0)
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@component_group", DBNull.Value);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST a   ";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);

//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());

//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);
//                                    if (dt_temp.Rows.Count > 0)
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    else
//                                        cmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST a   ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }





//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    //}
//                                    //else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //}
//                                    //else
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    //}
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    if (dt_temp.Rows.Count > 0)
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);



//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "yakir gurievsky";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "jithin valappilekandy";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "harshal b vyas";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);

//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());

//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);
//                                    if (dt_temp.Rows.Count > 0)
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    else
//                                        cmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "yakir gurievsky";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "jithin valappilekandy";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "harshal b vyas";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }



//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();

//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.DTT")
//                                    {
//                                        comp_group = "DTT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok and Bao, Xulie and Lee, jony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());



//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok Bao, Xulie and Lee, jony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }


//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();

//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    //if (dt4.Columns.Contains("sysdebug_forum"))
//                                    //    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"]?.ToString() ?? DBNull.Value.ToString());
//                                    //else
//                                    //    updateCmd.Parameters.AddWithValue("@cmf_status", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    //SqlDataAdapter adapt;


//                                    dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);
//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    //{
//                                    //    comp_group = "HW";
//                                    //    updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    //}
//                                    //else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    //{
//                                    //    comp_group = "HW";
//                                    //    updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    //}
//                                    //else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    //{
//                                    //    comp_group = "OS";
//                                    //    updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    //}
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //Check if promoted_id is null(from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //    {
//                                    //        ownersName = null;
//                                    //        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //    }
//                                    //    else if (string.IsNullOrEmpty(promotedId))
//                                    //    {
//                                    //        promoted_id is empty string, set owners_name to empty
//                                    //       ownersName = "";
//                                    //        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //        If HSD query returns null / empty, set owners_name to empty
//                                    //        if (string.IsNullOrEmpty(ownersName))
//                                    //        {
//                                    //            ownersName = "";
//                                    //            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //        }
//                                    //        else
//                                    //        {
//                                    //            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //        }
//                                    //    }

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    //if (comp_group == "3rd party")
//                                    //{
//                                    //    idstName = "mark j christopherson";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    if (comp_group == "audio")
//                                    {
//                                        idstName = "vijetha ramu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "john e lofgren";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "BT/WIFI")
//                                    //{
//                                    //    idstName = "ingrid shih";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "camera")
//                                    //{
//                                    //    idstName = "michael feng";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "terry c huang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "DG")
//                                    //{
//                                    //    idstName = "subhojit saha";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "EC")
//                                    //{
//                                    //    idstName = "navtej singh";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "HID Event Filter")
//                                    //{
//                                    //    idstName = "corinna tsou";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "HW")
//                                    //{
//                                    //    idstName = "arun ok";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "ICSS/Computer Vision")
//                                    //{
//                                    //    idstName = "joseph chan";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "iGFX")
//                                    //{
//                                    //    idstName = "joby thomas kurian";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "INF")
//                                    //{
//                                    //    idstName = "Joshi, Megha";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vijetha ramu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "IPF")
//                                    //{
//                                    //    idstName = "james y chen";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "eshak b";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "mark paperno";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "Modern Standby")
//                                    //{
//                                    //    idstName = "tony teng";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "OS")
//                                    //{
//                                    //    idstName = "ritika gupta";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "chidananda murthy k";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "sensor")
//                                    //{
//                                    //    idstName = "jingwu lin";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "SIV")
//                                    //{
//                                    //    idstName = "SIV";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "ron a starbuck jr";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "TBT")
//                                    //{
//                                    //    idstName = "jason ch liu";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TCSS (excl TBT)")
//                                    //{
//                                    //    idstName = "jabeer hussain s a";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TnP")
//                                    //{
//                                    //    idstName = "steven wang";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "tools")
//                                    //{
//                                    //    idstName = "pdst";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "Unison")
//                                    //{
//                                    //    idstName = "";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "VPU")
//                                    //{
//                                    //    idstName = "james lee";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "WWAN")
//                                    //{
//                                    //    idstName = "";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    //string idstName;
//                                    //if (comp_group == "3rd party")
//                                    //{
//                                    //    idstName = "mark j christopherson";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "audio")
//                                    //{
//                                    //    idstName = "ramu, vijetha";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "BIOS")
//                                    //{
//                                    //    idstName = "robert rauch";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "BT/WIFI")
//                                    //{
//                                    //    idstName = "ingrid shih";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "camera")
//                                    //{
//                                    //    idstName = "michael feng";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "CSME")
//                                    //{
//                                    //    idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "DG")
//                                    //{
//                                    //    idstName = "subhojit saha";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "EC")
//                                    //{
//                                    //    idstName = "navtej singh";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "HID Event Filter")
//                                    //{
//                                    //    idstName = "corinna tsou";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "HW")
//                                    //{
//                                    //    idstName = "arun ok";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "ICSS/Computer Vision")
//                                    //{
//                                    //    idstName = "joseph chan";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "iGFX")
//                                    //{
//                                    //    idstName = "joby thomas kurian";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "INF")
//                                    //{
//                                    //    idstName = "Joshi, Megha";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "IOMT")
//                                    //{
//                                    //    idstName = "ramu, vijetha";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "IPF")
//                                    //{
//                                    //    idstName = "james y chen";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "LAN")
//                                    //{
//                                    //    idstName = "allen lan";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "memory")
//                                    //{
//                                    //    idstName = "evgeni volfson";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "Modern Standby")
//                                    //{
//                                    //    idstName = "tony teng";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "no iDST assigned")
//                                    //{
//                                    //    idstName = "robert w bryan";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "OS")
//                                    //{
//                                    //    idstName = "ritika gupta";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "PnP")
//                                    //{
//                                    //    idstName = "gayathri prabhu";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "Power Delivery")
//                                    //{
//                                    //    idstName = "";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "security")
//                                    //{
//                                    //    idstName = "supreeti sehra";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "sensor")
//                                    //{
//                                    //    idstName = "jingwu lin";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "Serial I/O")
//                                    //{
//                                    //    idstName = "corinna tsou";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "SIV")
//                                    //{
//                                    //    idstName = "SIV";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "storage")
//                                    //{
//                                    //    idstName = "freeman lo";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TBT")
//                                    //{
//                                    //    idstName = "jason ch liu";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TCSS (excl TBT)")
//                                    //{
//                                    //    idstName = "jabeer hussain s a";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TnP")
//                                    //{
//                                    //    idstName = "steven wang";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "tools")
//                                    //{
//                                    //    idstName = "pdst";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "Unison")
//                                    //{
//                                    //    idstName = "";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "VPU")
//                                    //{
//                                    //    idstName = "james lee";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "WWAN")
//                                    //{
//                                    //    idstName = "";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else
//                                    //{
//                                    //    idstName = "no iDST assigned";
//                                    //    updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    //if (dt4.Columns.Contains("sysdebug_forum"))
//                                    //    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"]?.ToString() ?? DBNull.Value.ToString());
//                                    //else
//                                    //    cmd.Parameters.AddWithValue("@cmf_status", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());

//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    //                                    {
//                                    //                                        comp_group = "HW";
//                                    //                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    //                                    }
//                                    //                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    //                                    {
//                                    //                                        comp_group = "HW";
//                                    //                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    //                                    }
//                                    //                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    //                                    {
//                                    //                                        comp_group = "OS";
//                                    //                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    //                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    //if (comp_group == "3rd party")
//                                    //{
//                                    //    idstName = "mark j christopherson";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    if (comp_group == "audio")
//                                    {
//                                        idstName = "vijetha ramu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "john e lofgren";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "BT/WIFI")
//                                    //{
//                                    //    idstName = "ingrid shih";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "camera")
//                                    //{
//                                    //    idstName = "michael feng";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "terry c huang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "DG")
//                                    //{
//                                    //    idstName = "subhojit saha";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "EC")
//                                    //{
//                                    //    idstName = "navtej singh";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "HID Event Filter")
//                                    //{
//                                    //    idstName = "corinna tsou";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "HW")
//                                    //{
//                                    //    idstName = "arun ok";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "ICSS/Computer Vision")
//                                    //{
//                                    //    idstName = "joseph chan";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "iGFX")
//                                    //{
//                                    //    idstName = "joby thomas kurian";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "INF")
//                                    //{
//                                    //    idstName = "Joshi, Megha";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "vijetha ramu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "IPF")
//                                    //{
//                                    //    idstName = "james y chen";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "eshak b";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "mark paperno";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "Modern Standby")
//                                    //{
//                                    //    idstName = "tony teng";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "OS")
//                                    //{
//                                    //    idstName = "ritika gupta";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "chidananda murthy k";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "sensor")
//                                    //{
//                                    //    idstName = "jingwu lin";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "vineeth a hegde";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "SIV")
//                                    //{
//                                    //    idstName = "SIV";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "ron a starbuck jr";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    //else if (comp_group == "TBT")
//                                    //{
//                                    //    idstName = "jason ch liu";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TCSS (excl TBT)")
//                                    //{
//                                    //    idstName = "jabeer hussain s a";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "TnP")
//                                    //{
//                                    //    idstName = "steven wang";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "tools")
//                                    //{
//                                    //    idstName = "pdst";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "Unison")
//                                    //{
//                                    //    idstName = "";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "VPU")
//                                    //{
//                                    //    idstName = "james lee";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    //else if (comp_group == "WWAN")
//                                    //{
//                                    //    idstName = "";
//                                    //    cmd.Parameters.AddWithValue("@idst", idstName);
//                                    //}
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }


//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    //}
//                                    //else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //}
//                                    //else
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    //}
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok and Bao, Xulie and Lee, jony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());

//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok and Bao, Xulie and Lee, jony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }


//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();


//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    //if (cp_id == "14025924127")
//                                    //{
//                                    //    Console.WriteLine("Found the specific CP ID!");
//                                    //}
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    //}
//                                    //else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    //}
//                                    //else
//                                    //{
//                                    //    updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    //}
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);


//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;

//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    //if(cp_id == "14025924127")
//                                    //{
//                                    //    Console.WriteLine("Found the specific CP ID!");
//                                    //}
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);

//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());


//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    else
//                                        cmd.Parameters.AddWithValue("@component_group", DBNull.Value);

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string comp_group;
//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                    }
//                                    else
//                                        comp_group = DBNull.Value.ToString();

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // If there's a promoted_id, fetch owner from HSD via API
//                                    if (!string.IsNullOrEmpty(promotedId))
//                                    {
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    }

//                                    // If no owner found from promoted HSD, use CP owner as fallback
//                                    if (string.IsNullOrEmpty(ownersName))
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    }
//                                    else
//                                    {
//                                        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "kenny hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "kevin manalo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "sata yeh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools")
//                                    {
//                                        idstName = "ramu vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "georgia wang";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "sam hsu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }


//            dt4 = new DataTable();
//            {
//                dt4 = GetDatatableFromJson(webresults_cp);
//            }

//            if (dt4.Rows.Count > 0)
//            {

//                for (int i = 0; dt4.Rows.Count > i; i++)
//                {
//                    //datarow row1 = dt4.rows[0];
//                    // Check if the record already exists
//                    string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//                    using (SqlConnection con = new SqlConnection(strconnstring))
//                    {
//                        con.Open();
//                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//                        {
//                            checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//                            int count = (int)checkCmd.ExecuteScalar();

//                            if (count > 0)
//                            {

//                                SqlDataAdapter adapt;
//                                DataTable dt_temp = new DataTable();
//                                adapt = new SqlDataAdapter(checkProgress, con);
//                                adapt.Fill(dt_temp);

//                                string progress = dt_temp.Rows[0][0].ToString();

//                                using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//                                {
//                                    updateCmd.Connection = con;
//                                    updateCmd.CommandType = CommandType.Text;
//                                    updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//                                    {
//                                        string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//                                        int index = mustFixFor.IndexOf("WW") + 2;

//                                        //if (index + 2 <= mustFixFor.Length)
//                                        //{
//                                        //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//                                        //    {
//                                        //        mustFixFor = "Pre-PV";
//                                        //    }
//                                        //}

//                                        updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", "orange");
//                                    }
//                                    else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }
//                                    else
//                                    {
//                                        updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//                                    }

//                                    // Add original_progress parameter - keep existing value or set to null for new records
//                                    updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//                                    updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//                                    updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//                                    dt_temp.Clear();
//                                    adapt.Fill(dt_temp);
//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "IOMT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.DTT")
//                                    {
//                                        comp_group = "DTT";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    updateCmd.Parameters.AddWithValue("@days_active", days);

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// Check if promoted_id is null (from database)
//                                    //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    //}
//                                    //else if (string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    // promoted_id is empty string, set owners_name to empty
//                                    //    ownersName = "";
//                                    //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    //}
//                                    //else
//                                    //{
//                                    //    // promoted_id exists and is not empty, fetch owner from HSD via API
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                    //    // If HSD query returns null/empty, set owners_name to empty
//                                    //    if (string.IsNullOrEmpty(ownersName))
//                                    //    {
//                                    //        ownersName = "";
//                                    //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                    //    }
//                                    //    else
//                                    //    {
//                                    //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //    }
//                                    //}

//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                        }
//                                    }

//                                    updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//                                    //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok and Bao, Xulie and Lee, jony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        updateCmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    updateCmd.ExecuteNonQuery();
//                                }
//                            }

//                            else
//                            {
//                                using (SqlCommand cmd = new SqlCommand(insert))
//                                {
//                                    cmd.Connection = con;
//                                    cmd.CommandType = CommandType.Text;
//                                    cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//                                    cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//                                    cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//                                    cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//                                    cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//                                    cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//                                    cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//                                    if (dt4.Rows[i]["submitted_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//                                    if (dt4.Rows[i]["implemented_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//                                    if (dt4.Rows[i]["updated_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//                                    if (dt4.Rows[i]["closed_date"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//                                    cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//                                    cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//                                    cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//                                    cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//                                    cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//                                    cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//                                    cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//                                    cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//                                    cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//                                    cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//                                    cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//                                    cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//                                    cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//                                    cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//                                    cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//                                    if (dt4.Rows[i]["date_debug"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//                                    if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//                                    if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//                                    else
//                                        cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//                                    cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//                                    cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//                                    cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//                                    cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//                                    cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//                                    //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@los", DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//                                    cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//                                    cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//                                    cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());



//                                    SqlDataAdapter adapt;


//                                    DataTable dt_temp = new DataTable();
//                                    adapt.Fill(dt_temp);


//                                    int days;
//                                    if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    else
//                                    {
//                                        DateTime startDate, endDate;
//                                        startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//                                        startDate = startDate.Date;
//                                        endDate = DateTime.Now;
//                                        endDate = endDate.Date;
//                                        var span = endDate.Subtract(startDate);
//                                        days = span.Days;
//                                    }
//                                    cmd.Parameters.AddWithValue("@days_active", days);

//                                    string component = dt4.Rows[i]["found_in_component"].ToString();
//                                    string comp_group;

//                                    if (dt_temp.Rows.Count > 0)
//                                    {
//                                        comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//                                        cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//                                    }
//                                    {
//                                        comp_group = "PMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//                                    {
//                                        comp_group = "Security";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "IOMT";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.hw.uncategorized.unknown")
//                                    {
//                                        comp_group = "HW";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else if (component == "platform.Common.sw.os.Win11_2025_H2")
//                                    {
//                                        comp_group = "OS";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }
//                                    else
//                                    {
//                                        comp_group = "Other";
//                                        cmd.Parameters.AddWithValue("@component_group", comp_group);
//                                    }

//                                    //// Get owner from promoted HSD or fallback to CP owner
//                                    //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    //string ownersName = "";

//                                    //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    //// If there's a promoted_id, fetch owner from HSD via API
//                                    //if (!string.IsNullOrEmpty(promotedId))
//                                    //{
//                                    //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                    //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                    //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//                                    //}

//                                    //// If no owner found from promoted HSD, use CP owner as fallback
//                                    //if (string.IsNullOrEmpty(ownersName))
//                                    //{
//                                    //    ownersName = null;
//                                    //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//                                    //}
//                                    //else
//                                    //{
//                                    //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                    //}

//                                    //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//                                    // Get owner from promoted HSD or fallback to CP owner
//                                    string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//                                    string ownersName = "";

//                                    Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//                                    // Check if promoted_id is null (from database)
//                                    if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//                                    {
//                                        ownersName = null;
//                                        Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//                                    }
//                                    else if (string.IsNullOrEmpty(promotedId))
//                                    {
//                                        // promoted_id is empty string, set owners_name to empty
//                                        ownersName = "";
//                                        Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//                                    }
//                                    else
//                                    {
//                                        // promoted_id exists and is not empty, fetch owner from HSD via API
//                                        Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//                                        ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//                                        Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//                                        // If HSD query returns null/empty, set owners_name to empty
//                                        if (string.IsNullOrEmpty(ownersName))
//                                        {
//                                            ownersName = "";
//                                            Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//                                        }
//                                        else
//                                        {
//                                            Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//                                        }
//                                    }

//                                    cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//                                    cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//                                    string idstName;
//                                    if (comp_group == "3rd party")
//                                    {
//                                        idstName = "mark j christopherson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PMT")
//                                    {
//                                        idstName = "Teng Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "audio")
//                                    {
//                                        idstName = "kuan han";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BIOS")
//                                    {
//                                        idstName = "robert rauch";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "BT/WIFI")
//                                    {
//                                        idstName = "ingrid shih";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "camera")
//                                    {
//                                        idstName = "Chang Neil Y";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "CSME")
//                                    {
//                                        idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DG")
//                                    {
//                                        idstName = "subhojit saha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "EC")
//                                    {
//                                        idstName = "navtej singh";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HID Event Filter")
//                                    {
//                                        idstName = "corinna tsou";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "HW")
//                                    {
//                                        idstName = "arun ok Bao, Xulie and Lee, jony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "ICSS/Computer Vision")
//                                    {
//                                        idstName = "joseph chan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "iGFX" || comp_group == "GFX")
//                                    {
//                                        idstName = "joby thomas kurian";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "INF")
//                                    {
//                                        idstName = "Joshi, Megha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IOMT")
//                                    {
//                                        idstName = "ramu, vijetha";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "IPF")
//                                    {
//                                        idstName = "james y chen";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "LAN")
//                                    {
//                                        idstName = "allen lan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "memory")
//                                    {
//                                        idstName = "evgeni volfson";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Modern Standby")
//                                    {
//                                        idstName = "tony teng";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "no iDST assigned")
//                                    {
//                                        idstName = "robert w bryan";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "OS")
//                                    {
//                                        idstName = "ritika gupta";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "PnP")
//                                    {
//                                        idstName = "gayathri prabhu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Power Delivery")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "security" || comp_group == "Security")
//                                    {
//                                        idstName = "Rautch Robert ";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "DTT")
//                                    {
//                                        idstName = "Wu, Steven";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "sensor")
//                                    {
//                                        idstName = "jingwu lin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "tools" || comp_group == "Tools")
//                                    {
//                                        idstName = "Teng, Tony";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "SIV")
//                                    {
//                                        idstName = "SIV";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "storage")
//                                    {
//                                        idstName = "freeman lo";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TBT")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TCSS (excl TBT)")
//                                    {
//                                        idstName = "jason ch liu";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "TnP")
//                                    {
//                                        idstName = "Chaing Merlin";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Serial I/O")
//                                    {
//                                        idstName = "lu hank";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "Unison")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "VPU")
//                                    {
//                                        idstName = "james lee";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else if (comp_group == "WWAN")
//                                    {
//                                        idstName = "";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }
//                                    else
//                                    {
//                                        idstName = "no iDST assigned";
//                                        cmd.Parameters.AddWithValue("@idst", idstName);
//                                    }

//                                    cmd.ExecuteScalar();

//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            // Step 1: Extract all cp_ids from dt4
//            cpIdsFromApi = new List<string>();
//            foreach (DataRow row in dt4.Rows)
//            {
//                cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            }

//            idsToKeep = string.Join(",", cpIdsFromApi);

//            ///////////////////////////
//            ///
//            // Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            if (!string.IsNullOrEmpty(idsToKeep))
//            {

//                using (SqlConnection con = new SqlConnection(strconnstring))
//                {
//                    con.Open();
//                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//                    {
//                        deleteCmd.ExecuteNonQuery();
//                    }
//                }
//            }

//            using (SqlConnection con = new SqlConnection(strconnstring))
//            {
//                con.Open();

//                SqlCommand cmd = new SqlCommand(update_cp);
//                cmd.Connection = con;
//                cmd.CommandType = CommandType.Text;

//                cmd.ExecuteScalar();
//            }

//            //////////////////////////////////////////////////////////////////////////
//            ///


//            //dt4 = new DataTable();
//            //{
//            //    dt4 = GetDatatableFromJson(webresults_cp);
//            //}

//            //if (dt4.Rows.Count > 0)
//            //{

//            //    for (int i = 0; dt4.Rows.Count > i; i++)
//            //    {
//            //        //datarow row1 = dt4.rows[0];
//            //        // Check if the record already exists
//            //        string cp_id = dt4.Rows[i]["ip_sw_id"].ToString();



//            //        using (SqlConnection con = new SqlConnection(strconnstring))
//            //        {
//            //            con.Open();
//            //            using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
//            //            {
//            //                checkCmd.Parameters.AddWithValue("@cp_id", cp_id);
//            //                int count = (int)checkCmd.ExecuteScalar();

//            //                if (count > 0)
//            //                {

//            //                    SqlDataAdapter adapt;
//            //                    DataTable dt_temp = new DataTable();
//            //                    adapt = new SqlDataAdapter(checkProgress, con);
//            //                    adapt.Fill(dt_temp);

//            //                    string progress = dt_temp.Rows[0][0].ToString();

//            //                    using (SqlCommand updateCmd = new SqlCommand(updateQuery))
//            //                    {
//            //                        updateCmd.Connection = con;
//            //                        updateCmd.CommandType = CommandType.Text;
//            //                        updateCmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());

//            //                        updateCmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//            //                        if (dt4.Rows[i]["submitted_date"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//            //                        if (dt4.Rows[i]["implemented_date"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//            //                        if (dt4.Rows[i]["updated_date"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//            //                        if (dt4.Rows[i]["closed_date"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//            //                        if (dt4.Rows[i]["must_fix_for"].ToString().Contains("WW"))
//            //                        {
//            //                            string mustFixFor = dt4.Rows[i]["must_fix_for"].ToString();
//            //                            int index = mustFixFor.IndexOf("WW") + 2;

//            //                            //if (index + 2 <= mustFixFor.Length)
//            //                            //{
//            //                            //    if (int.TryParse(mustFixFor.Substring(index, 2), out int weekNumber) && weekNumber >= 1 && weekNumber <= 31)
//            //                            //    {
//            //                            //        mustFixFor = "Pre-PV";
//            //                            //    }
//            //                            //}

//            //                            updateCmd.Parameters.AddWithValue("@drivers", mustFixFor);
//            //                        }
//            //                        else
//            //                        {
//            //                            updateCmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//            //                        }
//            //                        updateCmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//            //                        if (dt4.Rows[i]["date_debug"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//            //                        if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//            //                        if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//            //                            updateCmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//            //                        else
//            //                            updateCmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//            //                        updateCmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//            //                        //updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//            //                        if (progress == "orange" && dt4.Rows[i]["state"].ToString() == "open")
//            //                        {
//            //                            updateCmd.Parameters.AddWithValue("@progress", "orange");
//            //                        }
//            //                        else if (progress == "orange" && (dt4.Rows[i]["state"].ToString() == "rejected" || dt4.Rows[i]["state"].ToString() == "complete"))
//            //                        {
//            //                            updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//            //                        }
//            //                        else
//            //                        {
//            //                            updateCmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());
//            //                        }

//            //                        // Add original_progress parameter - keep existing value or set to null for new records
//            //                        updateCmd.Parameters.AddWithValue("@original_progress", DBNull.Value);
//            //                        updateCmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());
//            //                        updateCmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());

//            //                        dt_temp.Clear();
//            //                        adapt.Fill(dt_temp);
//            //                        string component = dt4.Rows[i]["found_in_component"].ToString();
//            //                        string comp_group;

//            //                        if (dt_temp.Rows.Count > 0)
//            //                        {
//            //                            comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//            //                            updateCmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//            //                        }
//            //                        {
//            //                            comp_group = "PMT";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//            //                        {
//            //                            comp_group = "Security";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        {
//            //                            comp_group = "HW";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else if (component == "platform.Common.hw.uncategorized.unknown")
//            //                        {
//            //                            comp_group = "HW";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        {
//            //                            comp_group = "IOMT";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }

//            //                        else if (component == "platform.Common.sw.os.Win11_2025_H2")
//            //                        {
//            //                            comp_group = "OS";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else if (component == "platform.Common.sw.win-driver.DTT")
//            //                        {
//            //                            comp_group = "DTT";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else
//            //                        {
//            //                            comp_group = "Other";
//            //                            updateCmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }

//            //                        int days;
//            //                        if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//            //                        {
//            //                            DateTime startDate, endDate;
//            //                            startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//            //                            startDate = startDate.Date;
//            //                            endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//            //                            endDate = endDate.Date;
//            //                            var span = endDate.Subtract(startDate);
//            //                            days = span.Days;
//            //                        }
//            //                        else
//            //                        {
//            //                            DateTime startDate, endDate;
//            //                            startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//            //                            startDate = startDate.Date;
//            //                            endDate = DateTime.Now;
//            //                            endDate = endDate.Date;
//            //                            var span = endDate.Subtract(startDate);
//            //                            days = span.Days;
//            //                        }
//            //                        updateCmd.Parameters.AddWithValue("@days_active", days);

//            //                        //// Get owner from promoted HSD or fallback to CP owner
//            //                        //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//            //                        //string ownersName = "";

//            //                        //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//            //                        //// If there's a promoted_id, fetch owner from HSD via API
//            //                        //if (!string.IsNullOrEmpty(promotedId))
//            //                        //{
//            //                        //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//            //                        //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group); // Removed 'con' parameter
//            //                        //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//            //                        //}

//            //                        //// If no owner found from promoted HSD, use CP owner as fallback
//            //                        //if (string.IsNullOrEmpty(ownersName))
//            //                        //{
//            //                        //    ownersName = null;
//            //                        //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//            //                        //}
//            //                        //else
//            //                        //{
//            //                        //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//            //                        //}

//            //                        //updateCmd.Parameters.AddWithValue("@owners_name", ownersName);

//            //                        // Get owner from promoted HSD or fallback to CP owner
//            //                        //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//            //                        //string ownersName = "";

//            //                        //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//            //                        //// Check if promoted_id is null (from database)
//            //                        //if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//            //                        //{
//            //                        //    ownersName = null;
//            //                        //    Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//            //                        //}
//            //                        //else if (string.IsNullOrEmpty(promotedId))
//            //                        //{
//            //                        //    // promoted_id is empty string, set owners_name to empty
//            //                        //    ownersName = "";
//            //                        //    Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//            //                        //}
//            //                        //else
//            //                        //{
//            //                        //    // promoted_id exists and is not empty, fetch owner from HSD via API
//            //                        //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//            //                        //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//            //                        //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//            //                        //    // If HSD query returns null/empty, set owners_name to empty
//            //                        //    if (string.IsNullOrEmpty(ownersName))
//            //                        //    {
//            //                        //        ownersName = "";
//            //                        //        Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//            //                        //    }
//            //                        //    else
//            //                        //    {
//            //                        //        Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//            //                        //    }
//            //                        //}

//            //                        //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//            //                        string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//            //                        string ownersName = "";

//            //                        // Check if promoted_id is null (from database)
//            //                        if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//            //                        {
//            //                            ownersName = null;
//            //                        }
//            //                        else if (string.IsNullOrEmpty(promotedId))
//            //                        {
//            //                            // promoted_id is empty string, set owners_name to empty
//            //                            ownersName = "";
//            //                        }
//            //                        else
//            //                        {
//            //                            // promoted_id exists and is not empty, fetch owner from HSD via API
//            //                            ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);

//            //                            // If HSD query returns null/empty, set owners_name to empty
//            //                            if (string.IsNullOrEmpty(ownersName))
//            //                            {
//            //                                ownersName = "";
//            //                            }
//            //                        }

//            //                        updateCmd.Parameters.AddWithValue("@owners_name", string.IsNullOrEmpty(ownersName) ? (object)DBNull.Value : ownersName);
//            //                        //updateCmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);

//            //                        string idstName;
//            //                        if (comp_group == "3rd party")
//            //                        {
//            //                            idstName = "mark j christopherson";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "PMT")
//            //                        {
//            //                            idstName = "Teng Tony";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "audio")
//            //                        {
//            //                            idstName = "kuan han";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "BIOS")
//            //                        {
//            //                            idstName = "robert rauch";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "BT/WIFI")
//            //                        {
//            //                            idstName = "ingrid shih";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "camera")
//            //                        {
//            //                            idstName = "Chang Neil Y";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "DTT")
//            //                        {
//            //                            idstName = "Wu, Steven";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "CSME")
//            //                        {
//            //                            idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "DG")
//            //                        {
//            //                            idstName = "subhojit saha";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "EC")
//            //                        {
//            //                            idstName = "navtej singh";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "HID Event Filter")
//            //                        {
//            //                            idstName = "corinna tsou";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "HW")
//            //                        {
//            //                            idstName = "arun ok and Bao, Xulie and Lee, jony";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "ICSS/Computer Vision")
//            //                        {
//            //                            idstName = "joseph chan";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "iGFX" || comp_group == "GFX")
//            //                        {
//            //                            idstName = "joby thomas kurian";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "INF")
//            //                        {
//            //                            idstName = "Joshi, Megha";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "IOMT")
//            //                        {
//            //                            idstName = "ramu, vijetha";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "IPF")
//            //                        {
//            //                            idstName = "james y chen";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "LAN")
//            //                        {
//            //                            idstName = "allen lan";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "memory")
//            //                        {
//            //                            idstName = "evgeni volfson";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Modern Standby")
//            //                        {
//            //                            idstName = "tony teng";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "no iDST assigned")
//            //                        {
//            //                            idstName = "robert w bryan";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "OS")
//            //                        {
//            //                            idstName = "ritika gupta";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "PnP")
//            //                        {
//            //                            idstName = "gayathri prabhu";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Power Delivery")
//            //                        {
//            //                            idstName = "";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "security" || comp_group == "Security")
//            //                        {
//            //                            idstName = "Rautch Robert";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "DTT")
//            //                        {
//            //                            idstName = "Wu, Steven";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "sensor")
//            //                        {
//            //                            idstName = "jingwu lin";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "tools" || comp_group == "Tools")
//            //                        {
//            //                            idstName = "Teng, Tony";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "SIV")
//            //                        {
//            //                            idstName = "SIV";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "storage")
//            //                        {
//            //                            idstName = "freeman lo";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "TBT")
//            //                        {
//            //                            idstName = "jason ch liu";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "TCSS (excl TBT)")
//            //                        {
//            //                            idstName = "jason ch liu";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "TnP")
//            //                        {
//            //                            idstName = "Chaing Merlin";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Serial I/O")
//            //                        {
//            //                            idstName = "lu hank";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Unison")
//            //                        {
//            //                            idstName = "";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "VPU")
//            //                        {
//            //                            idstName = "james lee";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "WWAN")
//            //                        {
//            //                            idstName = "";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else
//            //                        {
//            //                            idstName = "no iDST assigned";
//            //                            updateCmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }

//            //                        updateCmd.ExecuteNonQuery();
//            //                    }
//            //                }

//            //                else
//            //                {
//            //                    using (SqlCommand cmd = new SqlCommand(insert))
//            //                    {
//            //                        cmd.Connection = con;
//            //                        cmd.CommandType = CommandType.Text;
//            //                        cmd.Parameters.AddWithValue("@cp_id", dt4.Rows[i]["ip_sw_id"].ToString());
//            //                        cmd.Parameters.AddWithValue("@title", dt4.Rows[i]["title"].ToString());
//            //                        cmd.Parameters.AddWithValue("@reproducibility", dt4.Rows[i]["reproducibility"].ToString());
//            //                        cmd.Parameters.AddWithValue("@status", dt4.Rows[i]["state"].ToString());
//            //                        cmd.Parameters.AddWithValue("@how_found_category", dt4.Rows[i]["how_found_category"].ToString());
//            //                        cmd.Parameters.AddWithValue("@family", dt4.Rows[i]["family"].ToString());
//            //                        cmd.Parameters.AddWithValue("@operating_system", dt4.Rows[i]["operating_system"].ToString());
//            //                        cmd.Parameters.AddWithValue("@platform", dt4.Rows[i]["platform"].ToString());
//            //                        cmd.Parameters.AddWithValue("@customer_company", dt4.Rows[i]["customers"].ToString());
//            //                        if (dt4.Rows[i]["submitted_date"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@submitted_date", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@submitted_date", dt4.Rows[i]["submitted_date"].ToString());
//            //                        if (dt4.Rows[i]["implemented_date"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@implemented_date", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@implemented_date", dt4.Rows[i]["implemented_date"].ToString());
//            //                        if (dt4.Rows[i]["updated_date"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@updated_date", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@updated_date", dt4.Rows[i]["updated_date"].ToString());
//            //                        if (dt4.Rows[i]["closed_date"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@closed_date", dt4.Rows[i]["closed_date"].ToString());
//            //                        cmd.Parameters.AddWithValue("@component", dt4.Rows[i]["found_in_component"].ToString());
//            //                        cmd.Parameters.AddWithValue("@release", dt4.Rows[i]["release"].ToString());
//            //                        cmd.Parameters.AddWithValue("@reason", dt4.Rows[i]["pending_reason"].ToString());
//            //                        cmd.Parameters.AddWithValue("@repro_on_rvp", dt4.Rows[i]["repro_on_rvp"].ToString());
//            //                        cmd.Parameters.AddWithValue("@impact", dt4.Rows[i]["impact"].ToString());
//            //                        cmd.Parameters.AddWithValue("@resolution_type", dt4.Rows[i]["resolution_type"].ToString());
//            //                        cmd.Parameters.AddWithValue("@cmf_request", dt4.Rows[i]["cmf_request"].ToString());
//            //                        cmd.Parameters.AddWithValue("@must_fix_for", dt4.Rows[i]["must_fix_for"].ToString());
//            //                        cmd.Parameters.AddWithValue("@drivers", dt4.Rows[i]["must_fix_for"].ToString());
//            //                        cmd.Parameters.AddWithValue("@report_type", dt4.Rows[i]["report_type"].ToString());
//            //                        cmd.Parameters.AddWithValue("@closed_reason", dt4.Rows[i]["closed_reason"].ToString());
//            //                        cmd.Parameters.AddWithValue("@ext_additional_info", dt4.Rows[i]["ext_additional_info"].ToString());
//            //                        cmd.Parameters.AddWithValue("@customer_detail", dt4.Rows[i]["customer_detail"].ToString());
//            //                        cmd.Parameters.AddWithValue("@customer_owner", dt4.Rows[i]["owner"].ToString());
//            //                        cmd.Parameters.AddWithValue("@submitter_org", dt4.Rows[i]["submitter_org"].ToString());
//            //                        cmd.Parameters.AddWithValue("@days_open", dt4.Rows[i]["days_open"].ToString());
//            //                        cmd.Parameters.AddWithValue("@ww_submitted", dt4.Rows[i]["ww_submitted"].ToString());
//            //                        cmd.Parameters.AddWithValue("@submitted_by", dt4.Rows[i]["submitted_by"].ToString());
//            //                        cmd.Parameters.AddWithValue("@regression", dt4.Rows[i]["regression"].ToString());
//            //                        cmd.Parameters.AddWithValue("@customer_affected", dt4.Rows[i]["customer_affected"].ToString());
//            //                        cmd.Parameters.AddWithValue("@processor", dt4.Rows[i]["processor"].ToString());
//            //                        cmd.Parameters.AddWithValue("@customer_impact", dt4.Rows[i]["customer_impact"].ToString());
//            //                        cmd.Parameters.AddWithValue("@priority", dt4.Rows[i]["priority"].ToString());
//            //                        cmd.Parameters.AddWithValue("@location", dt4.Rows[i]["location"].ToString());
//            //                        if (dt4.Rows[i]["date_debug"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@date_debug", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@date_debug", dt4.Rows[i]["date_debug"].ToString());

//            //                        if (dt4.Rows[i]["date_cmf_decided"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@date_cmf_decided", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@date_cmf_decided", dt4.Rows[i]["date_cmf_decided"].ToString());
//            //                        if (dt4.Rows[i]["date_cmf_ask"].ToString() == "")
//            //                            cmd.Parameters.AddWithValue("@date_cmf_ask", DBNull.Value);
//            //                        else
//            //                            cmd.Parameters.AddWithValue("@date_cmf_ask", dt4.Rows[i]["date_cmf_ask"].ToString());

//            //                        cmd.Parameters.AddWithValue("@3rd_party_classification", dt4.Rows[i]["third_party_classification"].ToString());
//            //                        cmd.Parameters.AddWithValue("@family_affected", dt4.Rows[i]["family_affected"].ToString());
//            //                        cmd.Parameters.AddWithValue("@promoted_status", dt4.Rows[i]["promoted_status"].ToString());
//            //                        cmd.Parameters.AddWithValue("@promoted_id", dt4.Rows[i]["promoted_id"].ToString());
//            //                        cmd.Parameters.AddWithValue("@progress", dt4.Rows[i]["progress"].ToString());

//            //                        cmd.Parameters.AddWithValue("@cmf_status", dt4.Rows[i]["sysdebug_forum"].ToString());
//            //                        cmd.Parameters.AddWithValue("@status_updated_by", DBNull.Value);
//            //                        cmd.Parameters.AddWithValue("@status_updated_on", DBNull.Value);
//            //                        //cmd.Parameters.AddWithValue("@idst", DBNull.Value);
//            //                        cmd.Parameters.AddWithValue("@los", DBNull.Value);
//            //                        cmd.Parameters.AddWithValue("@merge_id", dt4.Rows[i]["merge_id"].ToString());
//            //                        cmd.Parameters.AddWithValue("@fixed_in_version", dt4.Rows[i]["fixed_in_version"].ToString());
//            //                        cmd.Parameters.AddWithValue("@me_sku", dt4.Rows[i]["me_sku"].ToString());
//            //                        cmd.Parameters.AddWithValue("@sysdebug", dt4.Rows[i]["sysdebug"].ToString());



//            //                        SqlDataAdapter adapt;


//            //                        DataTable dt_temp = new DataTable();
//            //                        adapt.Fill(dt_temp);


//            //                        int days;
//            //                        if (dt4.Rows[i]["state"].ToString() == "complete" || dt4.Rows[i]["state"].ToString() == "rejected")
//            //                        {
//            //                            DateTime startDate, endDate;
//            //                            startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//            //                            startDate = startDate.Date;
//            //                            endDate = Convert.ToDateTime(dt4.Rows[i]["closed_date"]);
//            //                            endDate = endDate.Date;
//            //                            var span = endDate.Subtract(startDate);
//            //                            days = span.Days;
//            //                        }
//            //                        else
//            //                        {
//            //                            DateTime startDate, endDate;
//            //                            startDate = Convert.ToDateTime(dt4.Rows[i]["submitted_date"]);
//            //                            startDate = startDate.Date;
//            //                            endDate = DateTime.Now;
//            //                            endDate = endDate.Date;
//            //                            var span = endDate.Subtract(startDate);
//            //                            days = span.Days;
//            //                        }
//            //                        cmd.Parameters.AddWithValue("@days_active", days);

//            //                        string component = dt4.Rows[i]["found_in_component"].ToString();
//            //                        string comp_group;

//            //                        if (dt_temp.Rows.Count > 0)
//            //                        {
//            //                            comp_group = dt_temp.Rows[0]["CRTT_value"].ToString();
//            //                            cmd.Parameters.AddWithValue("@component_group", dt_temp.Rows[0]["CRTT_value"].ToString());
//            //                        }
//            //                        {
//            //                            comp_group = "PMT";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else if (component == "platform.Common.sw.win-driver.Intel TSE Driver#")
//            //                        {
//            //                            comp_group = "Security";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        {
//            //                            comp_group = "IOMT";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        {
//            //                            comp_group = "HW";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else if (component == "platform.Common.hw.uncategorized.unknown")
//            //                        {
//            //                            comp_group = "HW";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else if (component == "platform.Common.sw.os.Win11_2025_H2")
//            //                        {
//            //                            comp_group = "OS";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }
//            //                        else
//            //                        {
//            //                            comp_group = "Other";
//            //                            cmd.Parameters.AddWithValue("@component_group", comp_group);
//            //                        }

//            //                        //// Get owner from promoted HSD or fallback to CP owner
//            //                        //string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//            //                        //string ownersName = "";

//            //                        //Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//            //                        //// If there's a promoted_id, fetch owner from HSD via API
//            //                        //if (!string.IsNullOrEmpty(promotedId))
//            //                        //{
//            //                        //    Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//            //                        //    ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//            //                        //    Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");
//            //                        //}

//            //                        //// If no owner found from promoted HSD, use CP owner as fallback
//            //                        //if (string.IsNullOrEmpty(ownersName))
//            //                        //{
//            //                        //    ownersName = null;
//            //                        //    Console.WriteLine($"Debug - Using CP owner as fallback: '{ownersName}'");
//            //                        //}
//            //                        //else
//            //                        //{
//            //                        //    Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//            //                        //}

//            //                        //cmd.Parameters.AddWithValue("@owners_name", ownersName);

//            //                        // Get owner from promoted HSD or fallback to CP owner
//            //                        string promotedId = dt4.Rows[i]["promoted_id"].ToString();
//            //                        string ownersName = "";

//            //                        Console.WriteLine($"Debug - Row {i}: promoted_id = '{promotedId}', comp_group = '{comp_group}'");

//            //                        // Check if promoted_id is null (from database)
//            //                        if (dt4.Rows[i]["promoted_id"] == DBNull.Value || dt4.Rows[i]["promoted_id"] == null)
//            //                        {
//            //                            ownersName = null;
//            //                            Console.WriteLine($"Debug - promoted_id is null, setting owner to null");
//            //                        }
//            //                        else if (string.IsNullOrEmpty(promotedId))
//            //                        {
//            //                            // promoted_id is empty string, set owners_name to empty
//            //                            ownersName = "";
//            //                            Console.WriteLine($"Debug - promoted_id is empty, setting owner to empty");
//            //                        }
//            //                        else
//            //                        {
//            //                            // promoted_id exists and is not empty, fetch owner from HSD via API
//            //                            Console.WriteLine($"Debug - Attempting to fetch owner from HSD API for promoted_id: {promotedId}");
//            //                            ownersName = GetOwnerFromPromotedHSD(promotedId, comp_group);
//            //                            Console.WriteLine($"Debug - Owner from HSD API: '{ownersName}'");

//            //                            // If HSD query returns null/empty, set owners_name to empty
//            //                            if (string.IsNullOrEmpty(ownersName))
//            //                            {
//            //                                ownersName = "";
//            //                                Console.WriteLine($"Debug - HSD returned null/empty, setting owner to empty");
//            //                            }
//            //                            else
//            //                            {
//            //                                Console.WriteLine($"Debug - Using HSD owner: '{ownersName}'");
//            //                            }
//            //                        }

//            //                        cmd.Parameters.AddWithValue("@owners_name", (object)ownersName ?? DBNull.Value);
//            //                        cmd.Parameters.AddWithValue("@original_progress", DBNull.Value);

//            //                        string idstName;
//            //                        if (comp_group == "3rd party")
//            //                        {
//            //                            idstName = "mark j christopherson";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "PMT")
//            //                        {
//            //                            idstName = "Teng Tony";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "audio")
//            //                        {
//            //                            idstName = "kuan han";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "BIOS")
//            //                        {
//            //                            idstName = "robert rauch";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "BT/WIFI")
//            //                        {
//            //                            idstName = "ingrid shih";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "camera")
//            //                        {
//            //                            idstName = "Chang Neil Y";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "CSME")
//            //                        {
//            //                            idstName = "shanmugasundaram, jayaregha ; sellam, esther";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "DG")
//            //                        {
//            //                            idstName = "subhojit saha";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "DTT")
//            //                        {
//            //                            idstName = "Wu, Steven";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "EC")
//            //                        {
//            //                            idstName = "navtej singh";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "HID Event Filter")
//            //                        {
//            //                            idstName = "corinna tsou";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "HW")
//            //                        {
//            //                            idstName = "arun ok Bao, Xulie and Lee, jony";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "ICSS/Computer Vision")
//            //                        {
//            //                            idstName = "joseph chan";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "iGFX" || comp_group == "GFX")
//            //                        {
//            //                            idstName = "joby thomas kurian";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "INF")
//            //                        {
//            //                            idstName = "Joshi, Megha";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "IOMT")
//            //                        {
//            //                            idstName = "ramu, vijetha";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "IPF")
//            //                        {
//            //                            idstName = "james y chen";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "LAN")
//            //                        {
//            //                            idstName = "allen lan";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "memory")
//            //                        {
//            //                            idstName = "evgeni volfson";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Modern Standby")
//            //                        {
//            //                            idstName = "tony teng";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "no iDST assigned")
//            //                        {
//            //                            idstName = "robert w bryan";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "OS")
//            //                        {
//            //                            idstName = "ritika gupta";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "PnP")
//            //                        {
//            //                            idstName = "gayathri prabhu";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Power Delivery")
//            //                        {
//            //                            idstName = "";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "security" || comp_group == "Security")
//            //                        {
//            //                            idstName = "Rautch Robert ";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "DTT")
//            //                        {
//            //                            idstName = "Wu, Steven";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "sensor")
//            //                        {
//            //                            idstName = "jingwu lin";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "tools" || comp_group == "Tools")
//            //                        {
//            //                            idstName = "Teng, Tony";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "SIV")
//            //                        {
//            //                            idstName = "SIV";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "storage")
//            //                        {
//            //                            idstName = "freeman lo";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "TBT")
//            //                        {
//            //                            idstName = "jason ch liu";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "TCSS (excl TBT)")
//            //                        {
//            //                            idstName = "jason ch liu";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "TnP")
//            //                        {
//            //                            idstName = "Chaing Merlin";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Serial I/O")
//            //                        {
//            //                            idstName = "lu hank";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "Unison")
//            //                        {
//            //                            idstName = "";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "VPU")
//            //                        {
//            //                            idstName = "james lee";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else if (comp_group == "WWAN")
//            //                        {
//            //                            idstName = "";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }
//            //                        else
//            //                        {
//            //                            idstName = "no iDST assigned";
//            //                            cmd.Parameters.AddWithValue("@idst", idstName);
//            //                        }

//            //                        cmd.ExecuteScalar();

//            //                    }
//            //                }
//            //            }

//            //        }
//            //    }
//            //}

//            //// Step 1: Extract all cp_ids from dt4
//            //cpIdsFromApi = new List<string>();
//            //foreach (DataRow row in dt4.Rows)
//            //{
//            //    cpIdsFromApi.Add("'" + row["ip_sw_id"].ToString().Replace("'", "''") + "'"); // Add quotes and escape
//            //}

//            //idsToKeep = string.Join(",", cpIdsFromApi);

//            /////////////////////////////
//            /////
//            //// Step 2: Build and execute DELETE query (only if there are IDs to keep)
//            //if (!string.IsNullOrEmpty(idsToKeep))
//            //{

//            //    using (SqlConnection con = new SqlConnection(strconnstring))
//            //    {
//            //        con.Open();
//            //        using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
//            //        {
//            //            deleteCmd.ExecuteNonQuery();
//            //        }
//            //    }
//            //}

//            //using (SqlConnection con = new SqlConnection(strconnstring))
//            //{
//            //    con.Open();

//            //    SqlCommand cmd = new SqlCommand(update_cp);
//            //    cmd.Connection = con;
//            //    cmd.CommandType = CommandType.Text;

//            //    cmd.ExecuteScalar();
//            //}




//            Environment.Exit(0);

//        }

//    }


//}