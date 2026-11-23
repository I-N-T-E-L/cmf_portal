<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CMF_Web_portal_old.aspx.cs" Inherits="CMF_Web_portal_old" EnableEventValidation="false" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <!-- Required meta tags -->
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <!-- <meta http-equiv="refresh" content="600;" /> -->
    <title>CMF Web Portal</title>

    <!-- Stylesheets -->
    <link rel="stylesheet" href="template/vendors/feather/feather.css">
    <link rel="stylesheet" href="template/vendors/ti-icons/css/themify-icons.css">
    <link rel="stylesheet" href="template/vendors/css/vendor.bundle.base.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.2.1/css/all.min.css" type="text/css" />
    <link rel="stylesheet" href="template/css/style.css">
    <!-- Updated CSS link -->

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <!-- Bootstrap CSS -->
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" rel="stylesheet">

    <!-- jQuery -->
    <script src="https://code.jquery.com/jquery-3.3.1.slim.min.js"></script>

    <!-- Bootstrap JS -->
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>

    <style>
        /* General Styles */

        body {
            overflow-x: hidden;
            font-family: 'Arial', sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
            color: #333;
        }

        /* Header */
        header {
            z-index: 100;
            height: 7.0vh;
            display: flex;
            justify-content: center;
            align-items: center;
            background-color: #2a62c2;
            color: white;
            padding: 20px 30px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .header-title {
            justify-content: center;
            background-color: #2a62c2;
            font-size: 16px;
            font-weight: 600;
            position: center;
            z-index: 110;
        }

        .sidebar-toggle {
            background-color: #2a62c2;
            color: white;
            border: none;
            font-size: 16px;
            cursor: pointer;
            z-index: 10;
        }

        .sidebar-toggle-div {
            position: absolute;
            right: 1vw;
        }

        /* Sidebar */
        .sidebar {
            font-size: 12px;
            position: fixed;
            top: 0;
            right: -250px;
            height: 100%;
            width: 200px;
            background-color: #1172b3;
            padding-top: 60px;
            transition: right 0.3s ease-in-out;
            z-index: 120;
            overflow-y: auto;
        }

        .close-btn {
            position: absolute;
            top: 10px;
            right: 10px;
            background-color: transparent;
            color: white;
            font-size: 20px;
            border: none;
            cursor: pointer;
        }

        .sidebar a {
            color: #E2E8F0;
            padding: 12px 20px;
            text-decoration: none;
            display: block;
            font-size: 12px;
            z-index: 1;
        }

            .sidebar a:hover {
                background-color: #1887d1;
                z-index: 1;
            }

        /* Content */
        .content-wrapper {
            margin-left: 0;
            padding: 30px;
            background-color: #fff;
            min-height: calc(100vh - 140px);
        }


        /* Table Styles */
        .table-container {
            margin-top: -10px;
            position: relative;
            width: 100%;
            max-height: 75vh;
            overflow-y: auto;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            background-color: white;
        }

            /* Make the header sticky */
            .table-container thead {
                position: sticky;
                top: 0;
                background-color: #fff; /* Set a background color to cover any content scrolling beneath */
                z-index: 1; /* Keep the header above the rows */
            }

        
        .table-primary {
            font-weight: bold;
            font-family: Calibri;
            border-collapse: collapse;
            font-size: .8em;
            border-collapse: collapse;
            border-radius: 8px;
            overflow: auto;
            transition: box-shadow 0.3s ease;
            border: 2px solid black; /* Add border around the whole table */
            margin-bottom: 10px;
        }

        td:nth-child(2), th:nth-child(2) {
            border-left: solid 200px red;
        }

        .table-primary th, .table-primary td {
            padding: 15px;
            text-align: left;
            border: 10px solid black; /* Black border for cells (rows and columns) */
        }

        .table-primary thead {
            position: sticky;
            top: 0; /* Stick the header to the top */
            background-color: #fff; /* Ensure header has a solid background */
            z-index: 2; /* Make sure header stays on top of rows */
            box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1); /* Optional, to add a shadow */
        }

        .table-primary th {
            background-color: #2a62c2;
            color: white;
            font-weight: 500;
        }

        .table-primary tr {
            border-bottom: 1px solid black; /* Add a bottom border to each row */
        }

                /* Status: allow breaks and give it room */
        .table-primary .field-status           { min-width: 420px; }
        .table-primary .status-column {
            display: block;
            white-space: normal;
            overflow-wrap: anywhere;    /* modern, breaks long tokens */
            word-break: break-word;     /* fallback */
        }

        /* Optional: cap extremely long single cells to avoid super tall rows */
        .table-primary .status-column--clamp {
            display: -webkit-box;
            -webkit-line-clamp: 8;      /* show up to 8 lines */
            -webkit-box-orient: vertical;
            overflow: hidden;
            white-space: pre-wrap;
        }



        .table-primary2 {
    font-weight: bold;
    font-family: Calibri;
    border-collapse: collapse;
    font-size: .8em;
    border-collapse: collapse;
    border-radius: 8px;
    overflow: auto;
    transition: box-shadow 0.3s ease;
    border: 2px solid black; /* Add border around the whole table */
    margin-bottom: 10px;
}

td:nth-child(2), th:nth-child(2) {
    border-left: solid 200px red;
}

.table-primary2 th, .table-primary2 td {
    padding: 15px;
    text-align: left;
    border: 10px solid black; /* Black border for cells (rows and columns) */
}

.table-primary2 thead {
    position: sticky;
    top: 0; /* Stick the header to the top */
    background-color: #fff; /* Ensure header has a solid background */
    z-index: 2; /* Make sure header stays on top of rows */
    box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1); /* Optional, to add a shadow */
}

.table-primary2 th {
    background-color: #2a62c2;
    color: white;
    font-weight: 500;
}

.table-primary2 tr {
    border-bottom: 1px solid black; /* Add a bottom border to each row */
}

.table-primary7 {
    font-weight: bold;
    font-family: Calibri;
    border-collapse: collapse;
    font-size: .8em;
    border-collapse: collapse;
    border-radius: 8px;
    overflow: auto;
    transition: box-shadow 0.3s ease;
    border: 2px solid black; /* Add border around the whole table */
    margin-bottom: 10px;
}

td:nth-child(2), th:nth-child(2) {
    border-left: solid 200px red;
}

.table-primary7 th, .table-primary7 td {
    padding: 15px;
    text-align: left;
    border: 10px solid black; /* Black border for cells (rows and columns) */
}

.table-primary7 thead {
    position: sticky;
    top: 0; /* Stick the header to the top */
    background-color: #fff; /* Ensure header has a solid background */
    z-index: 2; /* Make sure header stays on top of rows */
    box-shadow: 0px 2px 5px rgba(0, 0, 0, 0.1); /* Optional, to add a shadow */
}

.table-primary7 th {
    background-color: #2a62c2;
    color: white;
    font-weight: 500;
}

.table-primary7 tr {
    border-bottom: 1px solid black; /* Add a bottom border to each row */
}

/* Add these new styles */
.tab-content-wrapper {
    position: relative;
    width: 100%;
    padding-top: 0;
    margin-top: 0;
}

.pane-heading {
    display: block !important;
    position: relative;
    z-index: 10;
    background-color: #f8f9fa;
    padding: 10px 15px;
    margin-bottom: 10px;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    font-weight: bold;
    font-size: 16px;
    color: #495057;
}

/* Modified existing styles */
.content-wrapper {
    position: relative;
    min-height: auto;
    margin-top: 0;
    padding-top: 0;
}

.modal-body.popup {
    position: relative;
    top: 0;
    margin-top: 0;
    padding-top: 0; /* Changed from 10px to 0 */
    padding-bottom: 10px;
}

.table-container {
    position: relative;
    margin-top: 0;
    padding-top: 0;
}

.filter-header-container {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    width: 100%;
    min-height: 60px;
}

.filter-header-text {
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
    padding: 2px 4px;
    width: 120px;
    height: 25px;
    border: 1px solid #ccc;
    border-radius: 3px;
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

/* ONLY MODIFY THE ISSUE LIST TABLE - DON'T BREAK FILTERS */
#overall_request_details {
    margin-top: 20px !important;
    position: relative !important;
    z-index: 100 !important;
    border-collapse: collapse !important;
}

#overall_request_details thead {
    display: table-header-group !important;
    background-color: #4472C4 !important;
    position: relative !important;
    z-index: 101 !important;
}

#overall_request_details th {
    background-color: #4472C4 !important;
    color: white !important;
    font-size: 12px !important;
    padding: 12px 8px !important;
    height: auto !important;
    min-height: 40px !important;
    border-top: 1px solid #000 !important;
    border-bottom: 1px solid #000 !important;
    border-left: 2px solid #000 !important;
    border-right: 2px solid #000 !important;
    display: table-cell !important;
    visibility: visible !important;
    opacity: 1 !important;
    position: relative !important;
    z-index: 102 !important;
    text-align: center !important;
    font-weight: bold !important;
    vertical-align: middle !important;
}

#overall_request_details td {
    border-top: 1px solid #000 !important;
    border-bottom: 1px solid #000 !important;
    border-left: 2px solid #000 !important;
    border-right: 2px solid #000 !important;
    padding: 8px !important;
}

/* PRESERVE FILTER DROPDOWN STYLING IN HEADERS */
#overall_request_details th .filter-header-container {
    display: flex !important;
    flex-direction: column !important;
    align-items: flex-end !important;
    width: 100% !important;
    min-height: 60px !important;
}

#overall_request_details th .filter-header-text {
    font-weight: bold !important;
    margin-bottom: 5px !important;
    align-self: center !important;
    color: white !important;
}

#overall_request_details th .filter-container {
    width: 100% !important;
    display: flex !important;
    justify-content: flex-end !important;
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
            padding: 10px;
            border-radius: 8px;
            border: 2px solid #1d4f91; /* Intel's blue */
            background-color: white;
            color: #333;
            width: 50%;
            max-width: 90px;
            max-height: 40px;
            transition: all 0.3s ease;
            margin-left: 30px;
            margin-top: 15px;
            margin-bottom: -10px;
        }

            .dropdown:focus {
                outline: none;
                border-color: #005d99; /* Darker shade of blue */
                box-shadow: 0 0 8px rgba(0, 93, 153, 0.5);
            }

        /* Modern Buttons */
        .modern-button {
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

            .modern-button:hover {
                background-color: #1172b3; /* Darker Intel Blue on hover */
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
            justify-content: center; /* Center the buttons horizontally */
            gap: 10px; /* Space between buttons */
            margin-top: -30px;
        }

         /* Button container for layout */
.button-container2 {
    display: flex;
justify-content: center; /* Center the buttons horizontally */
gap: 10px; /* Space between buttons */
margin-top: -150px;

}

.link-container {
            display: flex; /* Align links horizontally */
            justify-content: flex-end; /* Align links to the right */
            gap: 20px; /* Space between links */
            padding: 10px; /* Padding around the container */
            margin-top: 20px; /* Space above the container to avoid overlap */
            background-color: #f0f0f0; /* Background color for the container */
            border-radius: 5px; /* Rounded corners */
        }

        /* Style for individual links */
        .link-container a {
            text-decoration: none; /* Remove underline from links */
            color: #0073e6; /* Link color */
            font-weight: bold; /* Bold text */
        }

        /* Hover effect for links */
        .link-container a:hover {
            color: #005bb5; /* Darker color on hover */
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
            overflow: hidden; /* Prevents content overflow */
            max-width: 150px;
        }

        .owner-column {
            width: 10vw !important; /* Force the width using !important */ /* Prevent overflow issues */
            word-wrap: break-word !important; /* Ensure long words break and wrap */
            word-wrap: break-word; /* Ensures long words will wrap */
            white-space: normal; /* Allows wrapping within the cell */
            overflow: hidden; /* Prevents content overflow */
            max-width: 150px;
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


        .pane-heading {
            position: relative;
            font-size: 1.25rem; /* Slightly reduced font size */
            font-weight: 700; /* Bold font */
            color: #1E3A8A; /* Slightly darker blue shade */
            letter-spacing: 1px; /* Subtle letter spacing */
            text-transform: uppercase; /* Clean, sharp appearance */
            margin-top: -10px; /* Space from above */
            margin-bottom: -30px; /* Space from below */
            text-decoration: underline; /* Underline the heading */
            text-decoration-color: #1E3A8A; /* Matches the heading color for a cohesive look */
            text-align: center; /* Centers the text horizontally */
            width: 100%; /* Ensures it spans the full width of the container */
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

#selectallcheckbox {
    visibility: hidden;
}


<style type="text/css">
    /* Field Selector Panel Styles */
    .field-selector-panel {
        background: #f8f9fa;
        border: 1px solid #dee2e6;
        border-radius: 5px;
        margin-bottom: 15px;
        padding: 15px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .field-selector-toggle {
        background: linear-gradient(135deg, #007bff, #0056b3);
        color: white;
        border: none;
        padding: 12px 24px;
        border-radius: 6px;
        cursor: pointer;
        font-size: 14px;
        font-weight: 500;
        transition: all 0.3s ease;
        box-shadow: 0 2px 4px rgba(0,123,255,0.3);
        display: flex;
        align-items: center;
        gap: 8px;
        width: 100%;
        justify-content: center;
        margin-left: 30px;
        margin-top: 25px;
        margin-bottom: -10px;
    }

    .field-selector-toggle:hover {
        background: linear-gradient(135deg, #0056b3, #004085);
        transform: translateY(-1px);
        box-shadow: 0 4px 8px rgba(0,123,255,0.4);
    }

    .field-checkboxes {
        display: none;
        margin-top: 15px;
        padding: 15px;
        background: white;
        border: 1px solid #e9ecef;
        border-radius: 6px;
        box-shadow: inset 0 1px 3px rgba(0,0,0,0.1);
    }

    .field-checkboxes.show {
        display: block !important;
    }

    .checkbox-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 10px;
        margin-bottom: 15px;
        max-height: 300px;
        overflow-y: auto;
    }

    .checkbox-grid label {
        display: flex;
        align-items: center;
        padding: 8px 12px;
        cursor: pointer;
        font-size: 13px;
        border-radius: 4px;
        transition: all 0.2s ease;
        border: 1px solid transparent;
    }

    .checkbox-grid label:hover {
        background-color: #f8f9fa;
        border-color: #dee2e6;
    }

    .checkbox-grid input[type="checkbox"] {
        margin-right: 10px;
        transform: scale(1.1);
    }

    .panel-actions {
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
        justify-content: center;
        padding-top: 15px;
        border-top: 1px solid #e9ecef;
    }

    .btn {
        padding: 8px 16px;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 13px;
        font-weight: 500;
        transition: all 0.2s ease;
    }

    .btn-primary { background-color: #007bff; color: white; }
    .btn-secondary { background-color: #6c757d; color: white; }
    .btn-success { background-color: #28a745; color: white; }
    .btn-warning { background-color: #ffc107; color: #212529; }

    .btn:hover {
        opacity: 0.9;
        transform: translateY(-1px);
        box-shadow: 0 2px 4px rgba(0,0,0,0.2);
    }

    .selected-fields-info {
        margin-top: 10px;
        font-size: 12px;
        color: #6c757d;
        text-align: center;
        font-style: italic;
    }

    /* COMPLETE HIDING OF TABLE STRUCTURE FOR UNSELECTED FIELDS */
    #<%= overall_request_details.ClientID %> .field-sno.hidden,
    #<%= overall_request_details.ClientID %> .field-progress.hidden,
    #<%= overall_request_details.ClientID %> .field-sightingid.hidden,
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
    #<%= overall_request_details.ClientID %> .field-milestone.hidden,
    #<%= overall_request_details.ClientID %> .field-days_open.hidden,
    #<%= overall_request_details.ClientID %> .field-cmf_request.hidden,
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

    /* Hide empty data template columns as well */
    #<%= overall_request_details.ClientID %> .empty-data-container .field-sno.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-progress.hidden,
    #<%= overall_request_details.ClientID %> .empty-data-container .field-sightingid.hidden,
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

    /* Force table layout to recalculate when columns are hidden */
    #<%= overall_request_details.ClientID %> {
        table-layout: auto !important;
    }

    #<%= overall_request_details.ClientID %> table {
        table-layout: auto !important;
        width: 100% !important;
    }

    /* Responsive design */
    @media (max-width: 768px) {
        .checkbox-grid {
            grid-template-columns: 1fr;
        }
        
        .panel-actions {
            justify-content: center;
        }
    }
</style>

    <script>


        function toggleSidebar() {
            const sidebar = document.getElementById('sidebar');
            if (sidebar.style.right === "-250px") {
                sidebar.style.right = "0";
            } else {
                sidebar.style.right = "-250px";
            }
        }

        $(document).ready(function () {
            // Apply Select2 to your DropDownList
            $('.searchable-dropdown').select2({
                placeholder: "Select an owner",
                allowClear: true
            });
        });

        function closeSidebar() {
            document.querySelector('.sidebar').style.right = '-250px';
        }

        function openExportPopup() {
            // Show the modal when export button is clicked
            $('#exportModal').modal('show');
        }

        function submitExport() {
            // Create an array to hold the selected values
            var selectedValues = [];

            // Loop through each checkbox in the Repeater and check if it's selected
            $('input[type="checkbox"]:checked').each(function () {
                // Get the actual value of the checkbox (from the "Value" attribute, which matches Eval)
                selectedValues.push($(this).val());
            });

            // Check if any values were selected
            if (selectedValues.length == 0) {
                alert('Please select at least one value.');
                return; // Don't proceed if no values are selected
            }

            // Store selected values in a hidden field as a comma-separated string
            document.getElementById('<%= hfSelectedValues.ClientID %>').value = selectedValues.join(',');

            // Trigger postback to call a server-side function
            __doPostBack('<%= exportBtn.ClientID %>', '');
        }

        function driverCollector() {
            // Create an array to hold the selected values
            var selectedValues = [];

            // ONLY select checkboxes with the milestone-checkbox class (excluding Select All)
            $('.milestone-checkbox:checked').not('#chkSelectAll').each(function () {
                // Get the actual value of the checkbox
                selectedValues.push($(this).val());
            });

            // Check the Select All checkbox state
            var selectAllCheckbox = document.getElementById('chkSelectAll');

            if (selectAllCheckbox && selectAllCheckbox.checked) {
                // If Select All is checked, set to "AllDrivers"
                document.getElementById('<%= driverCollectorhf.ClientID %>').value = "AllDrivers";
            } else {
                // Check if any individual values were selected
                if (selectedValues.length == 0) {
                    alert('Please select at least one value.');
                    return; // Don't proceed if no values are selected
                }
                // Store selected individual values
                document.getElementById('<%= driverCollectorhf.ClientID %>').value = selectedValues.join(',');
            }
        }

        function toggleSelectAll() {
            var selectAllCheckbox = document.getElementById('chkSelectAll');
            var individualCheckboxes = $('.milestone-checkbox').not('#chkSelectAll');

            if (selectAllCheckbox.checked) {
                // If Select All is checked, uncheck all individual checkboxes
                individualCheckboxes.prop('checked', false);
            }

            // Call driverCollector to update the hidden field
            driverCollector();
        }

        function toggleIndividualCheckbox() {
            var selectAllCheckbox = document.getElementById('chkSelectAll');

            // If any individual checkbox is checked, uncheck Select All
            if ($('.milestone-checkbox:checked').not('#chkSelectAll').length > 0) {
                selectAllCheckbox.checked = false;
            }

            // Call driverCollector to update the hidden field
            driverCollector();
        }

        <%--function driverCollector() {
            // Create an array to hold the selected values
            var selectedValues = [];

            // Loop through each checkbox in the Repeater and check if it's selected
            $('input[type="checkbox"]:checked').each(function () {
                // Get the actual value of the checkbox (from the "Value" attribute, which matches Eval)
                selectedValues.push($(this).val());
            });

            // Check if any values were selected
            if (selectedValues.length == 0) {
                alert('Please select at least one value.');
                return; // Don't proceed if no values are selected
            }
            var selectAllCheckbox = document.getElementById('chkSelectAll');
            if (selectAllCheckbox.checked) {
                document.getElementById('<%= driverCollectorhf.ClientID %>').value = "AllDrivers";
            } else {
                document.getElementById('<%= driverCollectorhf.ClientID %>').value = selectedValues.join(',');
            }
            // Store selected values in a hidden field as a comma-separated string
        }--%>


        function openPopup() {
            document.getElementById("importPopup").style.display = "block";
        }

        function closePopup() {
            document.getElementById("importPopup").style.display = "none";
        }

        // Function to toggle "Select All" checkbox
        function toggleSelectAll(selectAllCheckbox) {
            var checkboxes = document.querySelectorAll('.chkValue');
            checkboxes.forEach(function (checkbox) {
                checkbox.checked = selectAllCheckbox.checked;
            });
        }

        // Function to update the "Select All" checkbox state
        function updateSelectAllCheckbox() {
            var checkboxes = document.querySelectorAll('.chkValue');
            var selectAllCheckbox = document.getElementById('chkSelectAll');
            var allChecked = true;

            checkboxes.forEach(function (checkbox) {
                if (!checkbox.checked) {
                    allChecked = false;
                }
            });

            // If not all checkboxes are checked, uncheck the "Select All" checkbox
            selectAllCheckbox.checked = allChecked;
        }

        function showCMFIssues(component, driver, issueType) {
            var hfComponent = document.getElementById('<%= HiddenComponentName.ClientID %>');
            var hfDriver = document.getElementById('<%= HiddenDriverName.ClientID %>');
            var hfIssueType = document.getElementById('<%= HiddenIssueType.ClientID %>');

            if (hfComponent) hfComponent.value = component || '';
            if (hfDriver) hfDriver.value = driver || '';
            if (hfIssueType) hfIssueType.value = issueType || '';

            __doPostBack('ShowCMFIssues', component + '|' + driver + '|' + issueType);
        }

        function getComponentValue() {
            // Get from hidden field if it exists
            var hfComponent = document.getElementById('<%= HiddenComponentName.ClientID %>');
            if (hfComponent && hfComponent.value) {
                return hfComponent.value;
            }

            // Or get from session/viewstate - you'll need to determine the source
            return '<%= Session["ComponentName"] != null ? Session["ComponentName"].ToString() : "" %>';
        }

        function getIssueTypeValue() {
            // Get from hidden field if it exists
            var hfIssueType = document.getElementById('<%= HiddenIssueType.ClientID %>');
            if (hfIssueType && hfIssueType.value) {
                return hfIssueType.value;
            }

            // Or determine based on your business logic
            return '<%= Session["IssueType"] != null ? Session["IssueType"].ToString() : "Open" %>';
        }

        function getDriverValue() {
            // Get from hidden field if it exists
            var hfDriver = document.getElementById('<%= HiddenDriverName.ClientID %>');
            if (hfDriver && hfDriver.value) {
                return hfDriver.value;
            }

            // Or get from session/viewstate - you'll need to determine the source
            return '<%= Session["DriverName"] != null ? Session["DriverName"].ToString() : "" %>';
        }

        function exportCMFIssues(component, driver, issueType) {
            var hfComponent = document.getElementById('<%= HiddenComponentName.ClientID %>');
    var hfDriver = document.getElementById('<%= HiddenDriverName.ClientID %>');
    var hfIssueType = document.getElementById('<%= HiddenIssueType.ClientID %>');

            if (hfComponent) hfComponent.value = component || '';
            if (hfDriver) hfDriver.value = driver || '';
            if (hfIssueType) hfIssueType.value = issueType || '';

            __doPostBack('ExportCMFIssues', component + '|' + driver + '|' + issueType);

            setTimeout(function () {
                var f = document.forms[0];
                if (f && f.__EVENTTARGET) f.__EVENTTARGET.value = '';
                if (f && f.__EVENTARGUMENT) f.__EVENTARGUMENT.value = '';
            }, 0);

            return false;
        }


        function adjustGridHeights() {
            // Get the height of the gridviews on the right side (GridView_cmf_summary and GridView_comp)
            var gridViewComp = document.getElementById('GridView_comp');
            var gridViewcmf_summary = document.getElementById('GridView_cmf_summary');

            // Get the height of the gridviews on the left side (GridView_cmf_summary1 and GridView_notes)
            var gridViewcmf_summary1 = document.getElementById('GridView_cmf_summary1');
            var gridViewNotes = document.getElementById('GridView_notes');

            // Get the total height of the left-side gridviews (without the gap first)
            var leftHeight = gridViewcmf_summary1.offsetHeight + gridViewNotes.offsetHeight;

            // Get the total height of the right-side gridviews (without the gap first)
            var rightHeight = gridViewcmf_summary.offsetHeight + gridViewComp.offsetHeight;

            // Calculate a dynamic gap as 2% of the total height (a visual balance)
            var gap = 0.02 * Math.max(leftHeight, rightHeight);  // 2% gap, depending on the larger height

            // Adjust total heights by adding the calculated gap
            leftHeight += gap;
            rightHeight += gap;

            // If the left and right heights are not equal, adjust accordingly
            if (leftHeight < rightHeight) {
                // If left side is shorter, distribute the remaining height equally between the two left-side gridviews
                var remainingHeight = rightHeight - leftHeight;
                var extraHeight = remainingHeight / 2;

                // Apply the extra height to both left gridviews (GridView_cmf_summary1 and GridView_notes)
                gridViewcmf_summary1.style.height = (gridViewcmf_summary1.offsetHeight + (extraHeight + 0.5)) + 'px';
                gridViewNotes.style.height = (gridViewNotes.offsetHeight + (extraHeight + 0.5)) + 'px';
            } else if (leftHeight > rightHeight) {
                // If left side is taller, adjust the right-side gridviews to match the total height of the left side
                var heightDifference = leftHeight - rightHeight;
                gridViewcmf_summary.style.height = (gridViewcmf_summary.offsetHeight + heightDifference) + 'px';
            }
            // If the heights are already equal, no adjustment is needed
            else {
                // No action needed if the heights are equal
                gridViewcmf_summary.style.height = gridViewcmf_summary.offsetHeight + 'px';
                gridViewComp.style.height = gridViewComp.offsetHeight + 'px';
                gridViewcmf_summary1.style.height = gridViewcmf_summary1.offsetHeight + 'px';
                gridViewNotes.style.height = gridViewNotes.offsetHeight + 'px';
            }
        }

        function changeFirstColumnColors() {
            // Get the gridview element
            var gridView = document.getElementById('<%= overall_request_details.ClientID %>');

            // Get all rows in the grid (skipping the header row if it exists)
            var rows = gridView.getElementsByTagName('tr');

            // Loop through each row (starting from the second row if header exists)
            for (var i = 1; i < rows.length; i++) {
                // Get the first cell (td) in the current row
                var firstCell = rows[i].cells[0];

                // Get the value of the first cell
                var cellValue = firstCell.textContent || firstCell.innerText;

                // Set the background color and text color to the value of the cell
                if (cellValue) {
                    firstCell.style.backgroundColor = cellValue;
                    firstCell.style.color = cellValue;
                }
            }
        }

        function moveUnassignedRowsToBottom() {
            // Get both GridViews
            const gridView1 = document.getElementById("GridView_design_summary");
            const gridView2 = document.getElementById("GridView_cmf_pending");

            // Function to move rows in any gridview
            function moveRows(gridView) {
                // Get all rows in the GridView (excluding header)
                const rows = Array.from(gridView.getElementsByTagName("tr")).slice(1);

                // Filter rows where the first column has the value "Unassigned"
                const unassignedRows = rows.filter(row => row.cells[0]?.innerText.trim() === "unassigned");
                const assignedRows = rows.filter(row => row.cells[0]?.innerText.trim() !== "unassigned");

                // Append unassigned rows to the bottom
                unassignedRows.forEach(row => gridView.appendChild(row));

                // Reorder rows in the GridView
                assignedRows.forEach(row => gridView.appendChild(row));
            }

            // Move rows in both GridViews
            moveRows(gridView1);
            moveRows(gridView2);
        }

        function moveUnassignedRowsToBottom() {
            const gridView1 = document.getElementById('<%= GridView_design_summary.ClientID %>');
            const gridView2 = document.getElementById('<%= GridView_component_summary.ClientID %>');

            if (!gridView1 && !gridView2) return;

            // Function to move unassigned rows to the bottom
            function processGridView(gridView) {
                const rows = Array.from(gridView.getElementsByTagName('tr'));
                const header = rows[0]; // Header row
                const dataRows = rows.slice(1); // Skip header

                const assignedRows = [];
                const unassignedRows = [];

                dataRows.forEach(row => {
                    const designCell = row.cells[0]; // Design is the first column
                    const designValue = designCell.textContent.trim().toLowerCase();
                    console.log("Design Value: ", designValue);

                    if (designValue === "unassigned") {
                        unassignedRows.push(row);
                    } else {
                        assignedRows.push(row);
                    }
                });

                // Clear existing rows (excluding header)
                while (gridView.rows.length > 1) {
                    gridView.deleteRow(1);
                }

                // Append rows in order (assigned first, then unassigned)
                assignedRows.forEach(row => gridView.appendChild(row));
                unassignedRows.forEach(row => gridView.appendChild(row));
            }

            // Process both GridViews
            if (gridView1) processGridView(gridView1);
            if (gridView2) processGridView(gridView2);
        }

        window.onload = function () {
            var gridView = document.getElementById('<%= GridView_cmf_summary.ClientID %>');
    var gridView_comp = document.getElementById('<%= GridView_comp.ClientID %>');

            if (gridView) {

                var rows = gridView.rows;
                var lastDataRowIndex = rows.length - 1;
                if (lastDataRowIndex >= 0) {
                    var lastRow = rows[lastDataRowIndex];
                    if (lastRow) {
                        lastRow.style.backgroundColor = "orange";
                        lastRow.style.color = "black";
                    }
                }
                //createMergedHeaders(gridView);
                //formatTotalRow(gridView);
            }

            if (gridView_comp) {
                var rows = gridView_comp.rows;
                var lastDataRowIndex = rows.length - 1;
                if (lastDataRowIndex >= 0) {
                    var lastRow = rows[lastDataRowIndex];
                    if (lastRow) {
                        lastRow.style.backgroundColor = "orange";
                        lastRow.style.color = "black";
                    }
                }
            }
        }

        function createMergedHeaders(gridView) {
            var headerRow = gridView.rows[0];
            if (!headerRow) return;

            // Use the public property instead
            var drivers = <%= DriversJson %>;

    // Create new header row for driver names
    var newHeaderRow = gridView.insertRow(0);
            newHeaderRow.style.backgroundColor = "#0056b3"; // Match css1 first row color
    newHeaderRow.style.color = "black"; // Changed to black to match css1
    newHeaderRow.style.fontWeight = "bold";
    newHeaderRow.style.borderBottom = "solid 2px black";

    // Component header cell
    var componentCell = newHeaderRow.insertCell(0);
    componentCell.innerHTML = "Component";
    componentCell.rowSpan = 2;
    componentCell.style.textAlign = "left"; // Changed to left to match css1
    componentCell.style.verticalAlign = "middle";
    componentCell.style.border = "1px solid #ddd"; // Match css1 border
    componentCell.style.borderLeft = "solid 2px black";
    componentCell.style.padding = "10px";
            componentCell.style.backgroundColor = "#0056b3";
    componentCell.style.color = "white";

    // Driver header cells (merged)
    for (var i = 0; i < drivers.length; i++) {
        var driverCell = newHeaderRow.insertCell(-1);
        driverCell.innerHTML = drivers[i];
        driverCell.colSpan = 2;
        driverCell.style.textAlign = "left"; // Changed to left to match css1
        driverCell.style.border = "1px solid #ddd"; // Match css1 border
        driverCell.style.borderLeft = "solid 2px black";
        driverCell.style.borderBottom = "solid 2px black";
        driverCell.style.padding = "10px";
        driverCell.style.backgroundColor = "#0056b3"; // Match css1 color
        driverCell.style.color = "white"; // Changed to black
        driverCell.style.fontWeight = "bold";
    }

    // Update original header row (now second row)
    headerRow.deleteCell(0); // Remove component cell since it's merged

    // Style the sub-headers to match css1
            headerRow.style.backgroundColor = "#0056b3";
    headerRow.style.borderBottom = "solid 2px black";

    for (var i = 0; i < headerRow.cells.length; i++) {
        headerRow.cells[i].style.backgroundColor = "#0056b3"; // Match css1 header color
        headerRow.cells[i].style.color = "white"; // Changed to black
        headerRow.cells[i].style.textAlign = "left"; // Changed to left to match css1
        headerRow.cells[i].style.padding = "10px"; // Match css1 padding
        headerRow.cells[i].style.border = "1px solid #ddd"; // Match css1 border
        headerRow.cells[i].style.borderLeft = "solid 2px black";
        headerRow.cells[i].style.fontWeight = "bold";
    }
}
<%--function createMergedHeaders(gridView) {
    var headerRow = gridView.rows[0];
    if (!headerRow) return;

    // Use the public property instead
    var drivers = <%= DriversJson %>;
    
    // Create new header row for driver names
    var newHeaderRow = gridView.insertRow(0);
    newHeaderRow.style.backgroundColor = "#0056b3"; // Primary blue
    newHeaderRow.style.color = "white";
    newHeaderRow.style.fontWeight = "bold";
    
    // Component header cell
    var componentCell = newHeaderRow.insertCell(0);
    componentCell.innerHTML = "Component";
    componentCell.rowSpan = 2;
    componentCell.style.textAlign = "center";
    componentCell.style.verticalAlign = "middle";
    componentCell.style.border = "2px solid black";
    componentCell.style.padding = "10px";
    
    // Driver header cells (merged)
    for (var i = 0; i < drivers.length; i++) {
        var driverCell = newHeaderRow.insertCell(-1);
        driverCell.innerHTML = drivers[i];
        driverCell.colSpan = 2;
        driverCell.style.textAlign = "center";
        driverCell.style.border = "2px solid black";
        driverCell.style.padding = "10px";
        driverCell.style.backgroundColor = "#007bff"; // Primary blue
        driverCell.style.color = "white";
    }
    
    // Update original header row (now second row)
    headerRow.deleteCell(0); // Remove component cell since it's merged
    
    // Style the sub-headers with darker blue
    for (var i = 0; i < headerRow.cells.length; i++) {
        headerRow.cells[i].style.backgroundColor = "#0056b3"; // Darker blue for sub-headers
        headerRow.cells[i].style.color = "white";
        headerRow.cells[i].style.textAlign = "center";
        headerRow.cells[i].style.padding = "8px";
        headerRow.cells[i].style.border = "2px solid black";
    }
}--%>

function formatTotalRow(gridView) {
    var drivers = <%= DriversJson %>;
    var rows = gridView.rows;
    var totalRowIndex = -1;

    // Find the total row
    for (var i = 0; i < rows.length; i++) {
        if (rows[i].cells[0] && (rows[i].cells[0].innerText.includes("Total") || rows[i].cells[0].textContent.includes("Total"))) {
            totalRowIndex = i;
            break;
        }
    }

    if (totalRowIndex >= 0) {
        var totalRow = rows[totalRowIndex];

        // Style the total row
        totalRow.style.backgroundColor = "orange";
        totalRow.style.color = "black";
        totalRow.style.fontWeight = "bold";

        // Update the first cell text
        totalRow.cells[0].innerText = "Total (LOS) + Duplicates + Implemented";

        // Merge the driver columns for total row
        for (var i = 0; i < drivers.length; i++) {
            var openCellIndex = 1 + (i * 2);
            var implCellIndex = 2 + (i * 2);

            if (openCellIndex < totalRow.cells.length && implCellIndex < totalRow.cells.length) {
                var openValue = totalRow.cells[openCellIndex].innerText || totalRow.cells[openCellIndex].textContent;
                var implValue = totalRow.cells[implCellIndex].innerText || totalRow.cells[implCellIndex].textContent;

                // Create merged content
                var mergedContent = openValue + " + " + implValue;

                // Update the first cell with merged content and hide the second
                totalRow.cells[openCellIndex].innerHTML = mergedContent;
                totalRow.cells[openCellIndex].colSpan = 2;
                totalRow.cells[openCellIndex].style.textAlign = "center";

                // Hide the second cell
                totalRow.cells[implCellIndex].style.display = "none";
            }
        }
    }
}

        function pad(num) {
            return num.toString().padStart(2, '0');
        }

        function formatIST(date) {
            const istOffset = 330;
            const utc = date.getTime() + (date.getTimezoneOffset() * 60000);
            const istTime = new Date(utc + (istOffset * 60000));

            const day = pad(istTime.getDate());
            const month = pad(istTime.getMonth() + 1);
            const year = istTime.getFullYear();
            const hours = pad(istTime.getHours());
            const minutes = pad(istTime.getMinutes());

            return `${day}/${month}/${year} ${hours}:${minutes} IST`;
        }

        function updateTimestamps() {
            const now = new Date();
            const lastUpdate = new Date(now);
            const minutes = lastUpdate.getMinutes();

            lastUpdate.setMinutes(minutes < 30 ? 0 : 30, 0, 0);
            const nextUpdate = new Date(lastUpdate);
            nextUpdate.setMinutes(lastUpdate.getMinutes() + 30);

            document.getElementById('last-update').innerText = `Last Updated: ${formatIST(lastUpdate)}`;
            document.getElementById('next-update').innerText = `Next Update: ${formatIST(nextUpdate)}`;
        }

        // Wait for full DOM load to avoid ASP.NET conflicts
        window.addEventListener('load', function () {
            updateTimestamps();
            setInterval(updateTimestamps, 60 * 1000);
        });

        function filterGrid() {
            const grid = document.getElementById('<%= overall_request_details.ClientID %>');
          const rows = grid.getElementsByTagName('tr');

            // Get filter values from the input fields
            const filter0 = document.getElementById('searchColumn0').value.toLowerCase();
          const filter1 = document.getElementById('searchColumn5').value.toLowerCase();
          const filter2 = document.getElementById('searchColumn10').value.toLowerCase();
          const filter3 = document.getElementById('searchColumn4').value.toLowerCase();
          const filter4 = document.getElementById('searchColumn6').value.toLowerCase();

          console.log("Filter values:", { filter1, filter2, filter3, filter4 });
          console.log("Total rows:", rows.length);

          // Loop through rows and apply the filter
          for (let i = 1; i < rows.length; i++) {
              const cells = rows[i].getElementsByTagName('td');

              if (cells.length === 0) continue;  // Skip header rows

              // Get column values (make sure these are the correct column indexes)
              const progressCell = cells[0];
              const driverCell = cells[5];  // Column for "Driver"
              const idstCell = cells[10];    // Column for "iDST"
              const componentCell = cells[4]; // Column for "Component"
              const rvpCell = cells[6];      // Column for "RVP Repro"

              // Get text content for each cell (trim and convert to lowercase)
              const progressText = progressCell.textContent.trim().toLowerCase();
              const driverText = driverCell.textContent.trim().toLowerCase();
              const idstText = idstCell.textContent.trim().toLowerCase();
              const componentText = componentCell.textContent.trim().toLowerCase();
              const rvpText = rvpCell.textContent.trim().toLowerCase();

              // Check if each cell matches its corresponding filter
              const matchProgress = progressText.includes(filter0);
              const matchDriver = driverText.includes(filter1);
              const matchIDST = idstText.includes(filter2);
              const matchComponent = componentText.includes(filter3);
              const matchRVP = rvpText.includes(filter4);

              // Show or hide row based on filter matches
              const showRow = matchProgress && matchDriver && matchIDST && matchComponent && matchRVP;
              rows[i].style.display = showRow ? '' : 'none';
          }
      }

        //function showDetailsModal(design) {

        //    // For now just show dummy content
        //    document.getElementById('modalContent').innerHTML = "Dummy modal content for Design: " + design;
        //    $('#detailsModal').modal('show');

        //}

        function showTotalDriverIssuesModal(design) {
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;
    __doPostBack('ShowTotalDriverIssuesModal', design);
}

function showTotalImplementedVerifiedModal(design) {
    document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;
    __doPostBack('ShowTotalImplementedVerifiedModal', design);
}

        function showDriverDetailsModal(design, driver) {
            // Store design and driver in hidden fields so server can access them
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;
                document.getElementById('<%= HiddenModalDriver.ClientID %>').value = driver;

                // Trigger postback manually
                __doPostBack('ShowDriverModal', design + ',' + driver);
        }

        function showImplementedVerifiedDetailsModal(design) {
            // Store design in a hidden field so server can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;

                // Trigger postback manually
                __doPostBack('ShowImplementedVerifiedModal', design);
            }

        <%--function showDetailsModal(design) {
            // Store design in a hidden field so seriver can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;

            // Trigger postback manually
            __doPostBack('ShowModal', design);
        }--%>

        function showDetailsModal(design) {
            // Store design in a hidden field so server can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;

            // Trigger postback manually
            __doPostBack('ShowModal', design);
        }


        function showDetailsModal2(ingred) {
            // Store design in a hidden field so seriver can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = ingred;

            // Trigger postback manually
            __doPostBack('ShowModal2', ingred);
        }
            function showDriverIssues(milestoneDriver) {
          var hf = document.getElementById('<%= HiddenDriverName.ClientID %>');
          if (hf) hf.value = milestoneDriver || '';
          __doPostBack('ShowDriverIssues', milestoneDriver || '');
      }

      function exportDriverIssues(milestoneDriver) {
          var hf = document.getElementById('<%= HiddenDriverName.ClientID %>');
                if (hf) hf.value = milestoneDriver || '';
          __doPostBack('ExportDriverIssues', milestoneDriver || '');


          // Clear sticky __EVENT* so next click doesn't repeat the export
          setTimeout(function () {
              var f = document.forms[0];
              if (f && f.__EVENTTARGET) f.__EVENTTARGET.value = '';
              if (f && f.__EVENTARGUMENT) f.__EVENTARGUMENT.value = '';
          }, 0);

          return false;

      }


        function showDetailsModal3(oem) {
            // Store design in a hidden field so seriver can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = oem;

            // Trigger postback manually
            __doPostBack('ShowModal3', oem);
        }

        function exportToExcel(design) {
            // Store design in a hidden field so server can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;

            // Trigger postback manually for exporting to Excel
            __doPostBack('ExportToExcel', design);
        }

        function exportToExcel2(design) {
            // Store design in a hidden field so server can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;

            // Trigger postback manually for exporting to Excel
            __doPostBack('ExportToExcel2', design);
        }

        function exportToExcel3(design) {
            // Store design in a hidden field so server can access it
            document.getElementById('<%= HiddenModalDesign.ClientID %>').value = design;

            // Trigger postback manually for exporting to Excel
            __doPostBack('ExportToExcel3', design);
        }

        function addSummaryRowToGridView(gridViewId) {
            const grid = document.getElementById(gridViewId);
            if (!grid) {
                console.error("GridView not found with ID:", gridViewId);
                return;
            }

            const rows = grid.getElementsByTagName("tr");
            if (rows.length < 2) return;

            const columnSums = [];
            const headerCells = rows[0].cells;
            const numColumns = headerCells.length;

            // Iterate through data rows only (skip header and footer if any)
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];

                // Skip footer or already-inserted summary rows
                if (row.className.includes("GridViewFooter") || row.getAttribute("data-summary")) {
                    continue;
                }

                const cells = row.cells;
                for (let j = 0; j < numColumns; j++) {
                    const text = cells[j]?.innerText?.trim() || '';
                    let value = parseFloat(text);

                    // Handle format like "3.8% (1/26)"
                    if (isNaN(value)) {
                        const match = text.match(/\((\d+(?:\.\d+)?)\/\d+\)/);
                        if (match) {
                            value = parseFloat(match[1]);
                        } else {
                            value = 0;
                        }
                    }

                    columnSums[j] = (columnSums[j] || 0) + value;
                }
            }

            // Create summary row
            const summaryRow = grid.insertRow(-1);
            summaryRow.setAttribute("data-summary", "true"); // Tag it so we can skip later if re-running
            for (let j = 0; j < numColumns; j++) {
                const cell = summaryRow.insertCell(j);
                if (j === 0) {
                    cell.innerText = "Total";
                    cell.style.fontWeight = "bold";
                } else {
                    const sum = columnSums[j] || 0;
                    cell.innerText = sum.toFixed(2);
                }
            }

            summaryRow.style.backgroundColor = "#eef0f4";
        }

        function showToast(message) {
            const toastEl = document.getElementById('errorToast');
            const toastBody = document.getElementById('toastMessage');

            toastBody.textContent = message;

            var toast = new bootstrap.Toast(toastEl, {
                delay: 7000 // Show toast for 8 seconds
            });
            toast.show();
        }



    </script>

    <div class="content-wrapper">
    <!-- Your existing content -->
</div>

<script type="text/javascript">
    // Simple, reliable functions that work with ASP.NET UpdatePanel
    function toggleFieldSelector() {
        var panel = document.getElementById('field-checkboxes');
        var button = document.getElementById('toggle-panel');

        if (panel && button) {
            if (panel.classList.contains('show')) {
                panel.classList.remove('show');
                button.innerHTML = '▼ Select Columns to Display';
            } else {
                panel.classList.add('show');
                button.innerHTML = '▲ Hide Column Selector';
            }
        }
    }

    function selectAllFields() {
        var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
        for (var i = 0; i < checkboxes.length; i++) {
            checkboxes[i].checked = true;
        }
        updateSelectedInfo();
    }

    function deselectAllFields() {
        var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
        for (var i = 0; i < checkboxes.length; i++) {
            checkboxes[i].checked = false;
        }
        updateSelectedInfo();
    }

    //function applyFieldFilter() {
    //    var selectedFields = getSelectedFields();
    //    updateGridViewDisplay(selectedFields);
    //    savePreferences(selectedFields);
    //    updateSelectedInfo();

    //    // Show success message
    //    showMessage('Filter applied! Showing ' + selectedFields.length + ' columns.', 'success');

    //    // Hide the panel
    //    var panel = document.getElementById('field-checkboxes');
    //    var button = document.getElementById('toggle-panel');
    //    if (panel && button) {
    //        panel.classList.remove('show');
    //        button.innerHTML = '▼ Select Columns to Display';
    //    }
    //}

    function applyFieldFilter() {
        var selectedFields = getSelectedFields();
        updateGridViewDisplay(selectedFields);
        savePreferences(selectedFields);
        updateSelectedInfo();

        // Save selected columns to server session
        saveSelectedColumnsToServer(selectedFields);

        // Show success message
        showMessage('Filter applied! Showing ' + selectedFields.length + ' columns.', 'success');

        // Hide the panel
        var panel = document.getElementById('field-checkboxes');
        var button = document.getElementById('toggle-panel');
        if (panel && button) {
            panel.classList.remove('show');
            button.innerHTML = '▼ Select Columns to Display';
        }
    }

    function saveSelectedColumnsToServer(selectedFields) {
        // Use PageMethods to call server-side method
        if (typeof PageMethods !== 'undefined' && PageMethods.SaveSelectedColumns) {
            PageMethods.SaveSelectedColumns(selectedFields);
        } else {
            // Alternative: use AJAX call
            var xhr = new XMLHttpRequest();
            xhr.open('POST', window.location.pathname + '/SaveSelectedColumns', true);
            xhr.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
            xhr.send(JSON.stringify({ selectedColumns: selectedFields }));
        }
    }

    function resetToDefault() {
        // Reset to show all columns (same as selectAllFields)
        selectAllFields();
        applyFieldFilter();
        showMessage('Reset to show all columns!', 'info');
    }

    function getSelectedFields() {
        var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]:checked');
        var selectedFields = [];
        for (var i = 0; i < checkboxes.length; i++) {
            selectedFields.push(checkboxes[i].value);
        }
        return selectedFields;
    }

    function updateGridViewDisplay(selectedFields) {

        var allFieldClasses = [
            'field-sno',
            'field-milestone',
            'field-progress',
            'field-sightingid',
            'field-promotedid',
            'field-customer_detail',
            'field-imagefreeze',
            'field-duplicatedetails',     // NEW
            'field-customer_company',
            'field-title',
            'field-component',
            'field-component_group',
            'field-owner',
            'field-promoted_owner',
            'field-rvp_repro',
            'field-status',
            'field-idst',
            'field-los',
            'field-processor',
            'field-impact',
            'field-days_open',
            'field-cmf_request',
            'field-closed_reason',
            'field-edit_column'
        ];


        var gridView = document.getElementById('<%= overall_request_details.ClientID %>');
        if (!gridView) {
            console.log('GridView not found');
            return;
        }

        allFieldClasses.forEach(function (fieldClass) {
            var fieldName = fieldClass.replace('field-', '');
            var elements = gridView.querySelectorAll('.' + fieldClass);

            for (var i = 0; i < elements.length; i++) {
                var element = elements[i];
                if (selectedFields.indexOf(fieldName) !== -1) {
                    element.classList.remove('hidden');
                } else {
                    element.classList.add('hidden');
                }
            }
        });

        // Force table reflow
        var tables = gridView.querySelectorAll('table');
        for (var i = 0; i < tables.length; i++) {
            var table = tables[i];
            var originalDisplay = table.style.display;
            table.style.display = 'none';
            table.offsetHeight; // Force reflow
            table.style.display = originalDisplay || 'table';
        }
    }

    function updateSelectedInfo() {
        var selectedFields = getSelectedFields();
        var info = document.getElementById('selected-info');
        if (info) {
            if (selectedFields.length > 0) {
                //info.textContent = 'Currently showing ' + selectedFields.length + ' columns: ' + selectedFields.join(', ');
            } else {
                info.textContent = 'No columns selected';
            }
        }
    }

    function savePreferences(selectedFields) {
        if (typeof (Storage) !== "undefined") {
            localStorage.setItem('gridViewSelectedFields', JSON.stringify(selectedFields));
        }
    }

    function loadSavedPreferences() {
        // Check if this is a fresh page load using server-side IsPostBack
        var isPageLoad = '<%= IsPostBack ? "false" : "true" %>' === 'true';
        
        if (isPageLoad) {
            // Fresh page load - show ALL columns by default

            var allFields = [
                'sno',
                'milestone',
                'progress',
                'sightingid',
                'promotedid',
                'customer_detail',
                'imagefreeze',
                'duplicatedetails',        // NEW
                'customer_company',
                'title',
                'component',
                'component_group',
                'owner',
                'promoted_owner',
                'rvp_repro',
                'status',
                'idst',
                'los',
                'processor',
                'impact',
                'days_open',
                'cmf_request',
                'closed_reason',
                'edit_column'
            ];
            

            
            // Check all checkboxes
            var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = true;
            }
            
            // Apply the filter to show all columns
            updateGridViewDisplay(allFields);
            updateSelectedInfo();
        } else {
            // UpdatePanel postback - load saved preferences or use current state
            if (typeof(Storage) !== "undefined") {
                var saved = localStorage.getItem('gridViewSelectedFields');
                if (saved) {
                    try {
                        var selectedFields = JSON.parse(saved);
                        
                        // Update checkboxes with saved preferences
                        var checkboxes = document.querySelectorAll('.field-checkboxes input[type="checkbox"]');
                        for (var i = 0; i < checkboxes.length; i++) {
                            var checkbox = checkboxes[i];
                            checkbox.checked = selectedFields.indexOf(checkbox.value) !== -1;
                        }
                        
                        // Apply the saved filter
                        updateGridViewDisplay(selectedFields);
                        updateSelectedInfo();
                    } catch (e) {
                        console.log('Error loading saved preferences:', e);
                        // Use current checkbox state
                        var selectedFields = getSelectedFields();
                        updateGridViewDisplay(selectedFields);
                        updateSelectedInfo();
                    }
                } else {
                    // No saved preferences, use current checkbox state
                    var selectedFields = getSelectedFields();
                    updateGridViewDisplay(selectedFields);
                    updateSelectedInfo();
                }
            } else {
                // localStorage not supported, use current checkbox state
                var selectedFields = getSelectedFields();
                updateGridViewDisplay(selectedFields);
                updateSelectedInfo();
            }
        }
    }

    function showMessage(message, type) {
        // Remove any existing messages
        var existingMessages = document.querySelectorAll('.field-filter-message');
        for (var i = 0; i < existingMessages.length; i++) {
            existingMessages[i].remove();
        }

        // Create a temporary message element
        var messageDiv = document.createElement('div');
        messageDiv.className = 'alert alert-' + type + ' field-filter-message';
        messageDiv.textContent = message;
        messageDiv.style.position = 'fixed';
        messageDiv.style.top = '20px';
        messageDiv.style.right = '20px';
        messageDiv.style.zIndex = '9999';
        messageDiv.style.padding = '12px 18px';
        messageDiv.style.borderRadius = '6px';
        messageDiv.style.fontSize = '14px';
        messageDiv.style.fontWeight = '500';
        messageDiv.style.backgroundColor = type === 'success' ? '#d4edda' : '#d1ecf1';
        messageDiv.style.color = type === 'success' ? '#155724' : '#0c5460';
        messageDiv.style.border = '1px solid ' + (type === 'success' ? '#c3e6cb' : '#bee5eb');
        messageDiv.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';

        document.body.appendChild(messageDiv);

        // Remove after 3 seconds
        setTimeout(function() {
            if (messageDiv.parentNode) {
                messageDiv.remove();
            }
        }, 3000);
    }

    // Initialize when page loads
    function initializeFieldFilter() {
        // Only initialize if the GridView exists
        if (document.getElementById('<%= overall_request_details.ClientID %>')) {
            loadSavedPreferences();
        }
    }

    // Initialize on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeFieldFilter);
    } else {
        initializeFieldFilter();
    }

    // Also initialize after UpdatePanel postbacks
    function pageLoad() {
        initializeFieldFilter();
    }
</script>


</head>

<body>
    <asp:HiddenField ID="HiddenModalDesign" runat="server" />
    <asp:HiddenField ID="HiddenDriverName" runat="server" />
    <asp:HiddenField ID="HiddenComponentName" runat="server" />
    <asp:HiddenField ID="HiddenIssueType" runat="server" />
<!-- You already have HiddenDriverName -->
    <asp:HiddenField ID="dw_trigger" runat="server" />

    <!-- Header -->
    <header>
        <!-- Convert to ASP.NET DropDownList -->


        <div class="header-title" runat="server" id="headerTitle">CMF Live Dashboard</div>
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



        <%--        <!-- Sidebar -->
        <div id="sidebar" class="sidebar">
            <button class="close-btn" onclick="closeSidebar()">X</button>
            <!-- Hardcoded 'All Drivers' link -->
    <div style="margin-left: 20px">
        <asp:LinkButton ID="lnkAllDrivers" runat="server" CommandArgument="AllDrivers" OnCommand="lnkValue_Command">
            All Drivers
        </asp:LinkButton><br />
    </div>
                       <!-- List of distinct values as checkboxes -->
    <div style="margin-left: 20px">
    <asp:Repeater ID="rptDistinctValues" runat="server">
        <ItemTemplate>
            <!-- Dynamically setting the ID and Value of the CheckBox -->
            <input type="checkbox" class="chkValue" 
                   id="chkValue_<%# Eval("drivers") %>" 
                   value='<%# Eval("drivers") %>' 
                   onclick="driverCollector()" />
            <label><%# Eval("drivers") %></label><br />
        </ItemTemplate>
    </asp:Repeater>
</div>--%>

        <!-- Sidebar -->
        <%--<div id="sidebar" class="sidebar">
            <button class="close-btn" onclick="closeSidebar()">X</button>
            <!-- Hardcoded 'All Drivers' link -->
            <div style="margin-left: 20px">
                <asp:LinkButton ID="lnkAllDrivers" runat="server" CommandArgument="AllDrivers" OnCommand="lnkValue_Command">
            All Milestones
                </asp:LinkButton><br />
            </div>

            <!-- Select All Checkbox -->
            <div id="selectallcheckbox" style="margin-left: 20px" runat="server">
                <input type="checkbox" id="chkSelectAll" onclick="driverCollector()"/>
                <label style="color: white; font-family: 'Arial', sans-serif;" for="chkSelectAll">Select All</label><br />
            </div>

            <!-- List of distinct values as checkboxes -->
            <div style="margin-left: 20px">
                <asp:Repeater ID="rptDistinctValues" runat="server">
                    <ItemTemplate>
                        <!-- Dynamically setting the ID and Value of the CheckBox -->
                        <input type="checkbox" class="chkValue"
                            id="chkValue_<%# Eval("drivers") %>"
                            value='<%# Eval("drivers") %>'
                            onclick="driverCollector()" />
                        <label style="color: white; font-family: 'Arial', sans-serif;"><%# Eval("drivers") %></label><br />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:LinkButton ID="submitdrivers" runat="server" CommandArgument="" OnCommand="lnkValue_Command">
    Submit
            </asp:LinkButton>
        </div>--%>

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
            <form id="form1" runat="server">
                <asp:HiddenField ID="HiddenField1" runat="server" />
                <asp:HiddenField ID="HiddenModalDriver" runat="server" />
                <asp:HiddenField ID="HiddenField2" runat="server" />

                <asp:ScriptManager ID="ToolkitScriptManager1" runat="server">
                </asp:ScriptManager>

                <asp:DropDownList
                    ID="ddlTables"
                    runat="server"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlTables_SelectedIndexChanged"
                    CssClass="dropdown">
                    <asp:ListItem Text="NVL-S" Value="CMF_NVL_S_ALL_COMPONENTS_TABLE" />                    
                    <asp:ListItem Text="NVL-H" Value="CMF_NVL_H_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="NVL-U" Value="CMF_NVL_U_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="NVL-Hx" Value="CMF_NVL_Hx_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="PTL" Value="CMF_PTL_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="LNL" Value="CMF_LNL_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="ARL-S" Value="CMF_ARL_S_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="ARL-H" Value="CMF_ARL_H_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="ARL-U" Value="CMF_ARL_U_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="ARL-Hx" Value="CMF_ARL_HX_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="GNR" Value="CMF_GNR_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="WCL" Value="CMF_WCL_ALL_COMPONENTS_TABLE" />
                    <asp:ListItem Text="ARL-Refresh" Value="CMF_ARL_Refresh_ALL_COMPONENTS_TABLE" />
                    
<%--                    <asp:ListItem Text="PTL" Value="CMF_PTL_ALL_COMPONENTS_TABLE" />--%>
                </asp:DropDownList>

                <div class="button-container">

                    <asp:Button ID="btnShowGridView3" runat="server" Text="CMF Summary" OnClick="btnShowGridView3_Click" CssClass="modern-button" />
                    <asp:Button ID="btnShowGridView2" runat="server" Text="Design Summary" OnClick="btnShowGridView2_Click" CssClass="modern-button" />
                    <asp:Button ID="btnShowGridView1" runat="server" Text="Issue List" OnClick="btnShowGridView1_Click" CssClass="modern-button" />
                    <asp:Button ID="btnShowGridView4" runat="server" Text="CMF Pending List" OnClick="btnShowGridView4_Click" CssClass="modern-button" />
                    <asp:Button ID="btnShowGridView5" runat="server" Text="Design Indicator" OnClick="btnShowGridView5_Click" CssClass="modern-button" />
                    <asp:Button ID="btnShowGridView6" runat="server" Text="Ingredient Indicator" OnClick="btnShowGridView6_Click" CssClass="modern-button" />
                    <asp:Button ID="btnShowGridView7" runat="server" Text="OEM Indicator" OnClick="btnShowGridView7_Click" CssClass="modern-button" /> 
                    
<%--                    <asp:Button ID="btnExportToPPT" runat="server" Text="Export to PPT" OnClientClick="openExportPopup(); return false;" CssClass="modern-button" />--%>

<asp:Button ID="btnExportToExcel" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_ig" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_ingred" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_des" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_design" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_des_summary" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_designsummary" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_cmf_pending" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_cmf_pending" CssClass="modern-button" />
                    <asp:Button ID="btnExportToExcel_oem" runat="server" Text="Export to Excel" OnClick="btnExportToExcel_Click_oem" CssClass="modern-button" Visible="false" />
        
                </div>

                <div class="link-container">
<%--                    <a href="https://pbirs01.intel.com/reports/powerbi/CCE-GCE%20DASHBOARDS/CMF_Portal" target="_blank">PowerBI</a>--%>
                    <a href="https://crtt.intel.com/Administration/SetupIDST.aspx?DNo=5343&PNo=135&Ing=92&CNo=1225" target="_blank">CRTT</a>
                    <asp:HyperLink ID="lnkPlatformDashboard" runat="server" Target="_blank" />
                    <asp:HyperLink ID="lnkNvlDashboardDownload" runat="server" Target="_blank" Text="Dashboards" />
                    <a href="https://pbirs01.intel.com/reports/powerbi/CCE-GCE%20DASHBOARDS/CMF_Portal" target="_blank">Infographic </a>

                </div>

                <div class="pane-heading" runat="server" id="pane1" visible="true">CMF Summary</div>
                <div class="pane-heading" runat="server" id="pane2" visible="false">Design Summary</div>
                <div class="pane-heading" runat="server" id="pane3" visible="false">Issue List</div>
                <div class="pane-heading" runat="server" id="pane4" visible="false">CMF Pending List</div>
                <div class="pane-heading" runat="server" id="pane5" visible="false">Design Indicator</div>
                <div class="pane-heading" runat="server" id="pane6" visible="false">Ingredient Indicator</div>
                <div class="pane-heading" runat="server" id="pane7" visible="false">OEM Indicator</div>



                    



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

                

<div class="content-wrapper">
    <div class="modal-body popup">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Timer ID="AutoRefreshTimer" runat="server" Interval="600000" OnTick="AutoRefreshTimer_Tick" />
                <div class="table-container">

                    <!-- FIELD SELECTOR PANEL - ONLY FOR ISSUE LIST -->
<asp:Panel ID="fieldSelectorPanel" runat="server" Visible="false">
    <div class="field-selector-panel">
        <button id="toggle-panel" class="field-selector-toggle" type="button" onclick="toggleFieldSelector()">
            ▼ Select Columns to Display
        </button>
        
        <%--<div class="selected-fields-info">
            <span id="selected-info">Currently showing all 23 columns</span>
        </div>--%>
        
        <div id="field-checkboxes" class="field-checkboxes">
            
<div class="checkbox-grid">
  <label><input type="checkbox" value="sno" checked onchange="updateSelectedInfo()"> S.No</label>
  <label><input type="checkbox" value="milestone" checked onchange="updateSelectedInfo()"> Milestone</label>
  <label><input type="checkbox" value="progress" checked onchange="updateSelectedInfo()"> Progress</label>
  <label><input type="checkbox" value="sightingid" checked onchange="updateSelectedInfo()"> Sighting ID</label>
  <label><input type="checkbox" value="promotedid" checked onchange="updateSelectedInfo()"> Promoted ID</label>
 <label><input type="checkbox" value="customer_detail" checked onchange="updateSelectedInfo()"> Customer Detail</label>
<label><input type="checkbox" value="imagefreeze" checked onchange="updateSelectedInfo()"> ImageFreeze</label> 
<label><input type="checkbox" value="duplicatedetails" checked onchange="updateSelectedInfo()"> Duplicate ID | Customer Detail | ImageFreezeDate</label>
  <label><input type="checkbox" value="customer_company" checked onchange="updateSelectedInfo()"> Customer Company</label>
  <label><input type="checkbox" value="title" checked onchange="updateSelectedInfo()"> Title</label>
  <label><input type="checkbox" value="component" checked onchange="updateSelectedInfo()"> Component</label>
  <label><input type="checkbox" value="component_group" checked onchange="updateSelectedInfo()"> Component Group</label>
  <label><input type="checkbox" value="owner" checked onchange="updateSelectedInfo()"> Owner</label>
  <label><input type="checkbox" value="promoted_owner" checked onchange="updateSelectedInfo()"> Promoted Owner</label>
  <label><input type="checkbox" value="rvp_repro" checked onchange="updateSelectedInfo()"> RVP Repro</label>
  <label><input type="checkbox" value="status" checked onchange="updateSelectedInfo()"> Status</label>
  <label><input type="checkbox" value="idst" checked onchange="updateSelectedInfo()"> iDST</label>
  <label><input type="checkbox" value="los" checked onchange="updateSelectedInfo()"> LOS</label>
  <label><input type="checkbox" value="processor" checked onchange="updateSelectedInfo()"> Processor</label>
  <label><input type="checkbox" value="impact" checked onchange="updateSelectedInfo()"> Impact</label>
  <label><input type="checkbox" value="days_open" checked onchange="updateSelectedInfo()"> Days Open</label>
<%--  <label><input type="checkbox" value="cmf_request" checked onchange="updateSelectedInfo()"> CMF Request</label>--%>
  <label><input type="checkbox" value="closed_reason" checked onchange="updateSelectedInfo()"> Closed Reason / Fixed Version</label>
  <label><input type="checkbox" value="edit_column" checked onchange="updateSelectedInfo()"> Edit</label>
</div>

            
            <div class="panel-actions">
                <button class="btn btn-primary" type="button" onclick="selectAllFields()">
                    ✓ Select All
                </button>
                <button class="btn btn-secondary" type="button" onclick="deselectAllFields()">
                    ✗ Deselect All
                </button>
                <button class="btn btn-success" type="button" onclick="applyFieldFilter()">
                    🔄 Apply Filter
                </button>
                <%--<button class="btn btn-warning" type="button" onclick="resetToDefault()">
                    🔄 Reset to All Columns
                </button>--%>
            </div>
        </div>
    </div>
</asp:Panel>
<!-- END OF FIELD SELECTOR PANEL -->

                    <div id="searchfilters" class="header-overlay" runat="server" visible="false">
                        <!-- Your existing search filters -->
                        <input type="text" id="searchColumn0" placeholder="Search Progress" oninput="filterGrid()" />
                        <input type="text" id="searchColumn5" placeholder="Search Owner" oninput="filterGrid()" />
                        <input type="text" id="searchColumn10" placeholder="Search Component" oninput="filterGrid()" />
                        <input type="text" id="searchColumn4" placeholder="Search Component" oninput="filterGrid()" />
                        <input type="text" id="searchColumn6" placeholder="Search RVP Repro" oninput="filterGrid()" />
                    </div>

                    <div class="gridview-container">
                        <!-- MODIFY YOUR GRIDVIEW TO ADD DATA-FIELD ATTRIBUTES -->
                        
<asp:GridView ID="overall_request_details" CssClass="table-primary" runat="server" AutoGenerateColumns="False"
    EmptyDataText="No Open CMFs" OnRowEditing="overall_request_details_RowEditing"
    OnRowUpdating="overall_request_details_RowUpdating" OnRowCancelingEdit="overall_request_details_RowCancelingEdit"
    OnRowDataBound="overall_request_details_RowDataBound" Visible="false">

    <Columns>
   
        <asp:TemplateField HeaderText="S.No" ItemStyle-Width="50px" HeaderStyle-Width="50px"
            ItemStyle-CssClass="field-sno" HeaderStyle-CssClass="field-sno">
            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
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
                <span><%# Eval("Driver") %></span>
            </ItemTemplate>
        </asp:TemplateField>

        
        <asp:BoundField DataField="progress" HeaderText="Progress" ReadOnly="True"
            ItemStyle-CssClass="field-progress" HeaderStyle-CssClass="field-progress" />

      
        <asp:HyperLinkField DataTextField="SightingID" HeaderText="SightingID"
            DataNavigateUrlFields="SightingID"
            DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}" Target="_blank"
            ItemStyle-CssClass="field-sightingid" HeaderStyle-CssClass="field-sightingid" />

   
        <asp:HyperLinkField DataTextField="Merged_PromotedID" HeaderText="Promoted ID"
            DataNavigateUrlFields="Merged_PromotedID"
            DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}" Target="_blank"
            ItemStyle-CssClass="field-promotedid" HeaderStyle-CssClass="field-promotedid" />

        <%--<asp:TemplateField HeaderText="Customer Detail" ItemStyle-CssClass="field-customer_detail" HeaderStyle-CssClass="field-customer_detail">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">Customer Detail</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlDetailHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlDetailHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><span><%# Eval("customer_detail") %></span></ItemTemplate>
        </asp:TemplateField>--%>

            <asp:BoundField DataField="customer_detail" HeaderText="Customer Detail" ReadOnly="True"
ItemStyle-CssClass="field-customer_detail" HeaderStyle-CssClass="field-customer_detail" />

        <asp:BoundField DataField="ImageFreeze" HeaderText="ImageFreeze" ReadOnly="True"
    ItemStyle-CssClass="field-imagefreeze" HeaderStyle-CssClass="field-imagefreeze" />

        <asp:TemplateField HeaderText="Duplicate ID | Customer Detail | ImageFreezeDate"
            ItemStyle-CssClass="field-duplicatedetails" HeaderStyle-CssClass="field-duplicatedetails">
            <ItemTemplate><%# CreateDuplicateLinks(Eval("DuplicateDetails")) %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="Customer Company" ItemStyle-CssClass="field-customer_company" HeaderStyle-CssClass="field-customer_company">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">Customer Company</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlCompanyHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlCompanyHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><span><%# Eval("customer_company") %></span></ItemTemplate>
        </asp:TemplateField>

      
        <asp:BoundField DataField="title" HeaderText="Title" ReadOnly="True"
            ItemStyle-CssClass="field-title" HeaderStyle-CssClass="field-title" />

       
        <asp:BoundField DataField="component" HeaderText="Component" ReadOnly="True"
            ItemStyle-CssClass="component-column field-component" HeaderStyle-CssClass="field-component" />

        <asp:TemplateField HeaderText="Component Group" ItemStyle-CssClass="field-component_group" HeaderStyle-CssClass="field-component_group">
            <HeaderTemplate>
                <div class="filter-header-container">
                    <div class="filter-header-text">Component Group</div>
                    <div class="filter-container">
                        <asp:DropDownList ID="ddlComponentHeader" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlComponentHeader_SelectedIndexChanged" CssClass="header-dropdown" />
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate><span><%# Eval("component_group") %></span></ItemTemplate>
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
            <ItemTemplate><span class="owner-column"><%# Eval("Owner") %></span></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtOwner" runat="server" TextMode="MultiLine" Text='<%# Bind("Owner") %>'
                    CssClass="form-control"></asp:TextBox>
            </EditItemTemplate>
        </asp:TemplateField>

    
        <asp:BoundField DataField="owners_name" HeaderText="Promoted_Owner" ReadOnly="True"
            ItemStyle-CssClass="field-promoted_owner" HeaderStyle-CssClass="field-promoted_owner" />

      
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

       
        <asp:TemplateField HeaderText="Status" SortExpression="Status" ItemStyle-CssClass="field-status" HeaderStyle-CssClass="field-status">
            <ItemTemplate><span class="status-column"><%# FormatStatusText(Eval("Status")) %></span></ItemTemplate>
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
            <ItemTemplate><span class="idst-column"><%# Eval("idst_display_name") %></span></ItemTemplate>
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
            ItemStyle-CssClass="field-processor" HeaderStyle-CssClass="field-processor" />

        
      <asp:TemplateField HeaderText="Impact" ItemStyle-CssClass="field-impact" HeaderStyle-CssClass="field-impact">
            <ItemTemplate><span class="impact-column"><%# Eval("impact") %></span></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtimpact" runat="server" TextMode="MultiLine" Text='<%# Bind("impact") %>'
                    CssClass="form-control"></asp:TextBox>
            </EditItemTemplate>
        </asp:TemplateField>

       
        <asp:BoundField DataField="days_active" HeaderText="Days_Open" ReadOnly="True"
            ItemStyle-CssClass="daysopen-column field-days_open" HeaderStyle-CssClass="field-days_open" />

       
        <%--<asp:BoundField DataField="cmf_request" HeaderText="CMF_Request" ReadOnly="True"
            ItemStyle-CssClass="field-cmf_request" HeaderStyle-CssClass="field-cmf_request" />--%>

        
        <asp:BoundField DataField="ClosedDetails" HeaderText="Closed Reason / Fixed Version" ReadOnly="True"
            ItemStyle-CssClass="cmfrequest-column field-closed_reason" HeaderStyle-CssClass="field-closed_reason" />

        
        <asp:CommandField ShowEditButton="True" ItemStyle-CssClass="field-edit_column" HeaderStyle-CssClass="field-edit_column" />
    </Columns>
    
<EmptyDataTemplate>
    <div class="empty-data-container">
        <table class="table-primary" style="width:100%">
            <thead>
                <tr>
                    
                    <th style="width:50px;" class="field-sno">S.No</th>

                   
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

                   
                    <th class="field-progress">Progress</th>

                   
                    <th class="field-sightingid">SightingID</th>

                    
                    <th class="field-promotedid">Promoted ID</th>


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
                    
                    <th class="field-duplicatedetails">Duplicate ID | Customer Detail | ImageFreezeDate</th>

                   
                    <th class="field-customer_company">
                        <div class="filter-header-container">
                            <div class="filter-header-text">Customer Company</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlCompanyHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlCompanyHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>

                   
                    <th class="field-title">Title</th>

                   
                    <th class="field-component">Component</th>

                    
                    <th class="field-component_group">
                        <div class="filter-header-container">
                            <div class="filter-header-text">Component Group</div>
                            <div class="filter-container">
                                <asp:DropDownList ID="ddlComponentHeaderEmpty" runat="server"
                                    AutoPostBack="true"
                                    OnSelectedIndexChanged="ddlComponentHeader_SelectedIndexChanged"
                                    CssClass="header-dropdown">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </th>

                    
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

                   
                    <th class="field-promoted_owner">Promoted_Owner</th>

                    
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

                    
                    <th class="field-status">Status</th>

                    
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

                   
                    <th class="field-edit_column"></th>
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
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>

                 

                                    <div class="gridview-container">
                                        <asp:GridView ID="GridView_design_open" CssClass="table-primary2" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false" OnRowEditing="GridView_design_open_RowEditing" OnRowCancelingEdit="GridView_design_open_RowCancelingEdit" OnRowDataBound="GridView_design_open_RowDataBound" DataKeyNames="Design">
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
                                                <%--<asp:BoundField DataField="Driver_issues" HeaderText="Driver_issues" ReadOnly="True" />
                                                <asp:BoundField DataField="Implemented_Verified" HeaderText="Implemented/Verified" ReadOnly="True" />--%>
                                                <asp:CommandField ShowEditButton="True" />
                                            </Columns>
                                        </asp:GridView>
                                    </div>



                                    <div class="gridview-container">
                                        <asp:GridView ID="GridView_cmf_pending" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false">
                                            <Columns>
                                                <asp:HyperLinkField
                                                    DataTextField="cp_id"
                                                    HeaderText="ID"
                                                    DataNavigateUrlFields="cp_id"
                                                    DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
                                                    Target="_blank" />
                                                <asp:BoundField DataField="title" HeaderText="Title" />
                                                <asp:BoundField DataField="customer_detail" HeaderText="Customer Detail" />
                                                <%--<asp:BoundField DataField="component" ItemStyle-Wrap="true"
                                                    ItemStyle-Width="7000px"
                                                    HeaderStyle-Width="7000px" HeaderText="Component" />--%>
                                                <asp:BoundField DataField="component" HeaderText="Component" />
                                                <asp:BoundField DataField="component_group" HeaderText="Component Group" />
                                            <asp:TemplateField HeaderText="iDST">
                                                <ItemTemplate>
                                                    <span class="idst-column"><%# Eval("idst_display_name") %></span>
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="txtidst" runat="server" TextMode="MultiLine" Text='<%# Bind("idst_display_name") %>' CssClass="form-control"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateField>

                                            <asp:BoundField DataField="customer_owner" HeaderText="Customer Owner" />
                                            <asp:BoundField DataField="repro_on_rvp" HeaderText="RVP Repro" ReadOnly="True" />
                                            <asp:BoundField DataField="reproducibility" HeaderText="Reproducibility" ReadOnly="True" />
<%--<asp:BoundField DataField="date_cmf_ask" HeaderText="Date CMF Ask" />--%>
                                                
                                                <asp:BoundField DataField="date_cmf_ask" HeaderText="Date CMF Ask" />
                                                <asp:BoundField DataField="cmf_request" HeaderText="CMF Request" />
                                                <asp:BoundField DataField="why_cmf_change" HeaderText="Why CMF Change" />
                                                <asp:BoundField DataField="pending_reason" HeaderText="Pending Reason" />
                                                <asp:BoundField DataField="impact" HeaderText="Justification" />

                                            </Columns> 
                                        </asp:GridView>
                                    </div>


                                    <div class="gridview-container">
                                        <asp:GridView ID="GridView_design_summary" CssClass="table-primary" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="false" OnRowDataBound="GridView5_RowDataBound">
                                            <Columns>
                                                
                                                <asp:BoundField DataField="Design" HeaderText="Design" />
<%--                                                <asp:BoundField DataField="ME_SKU" HeaderText="ME_SKU" />--%>
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
        <h5 class="modal-title">Issue List</h5>
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
          <h5 class="modal-title">Issue List (by Milestone) — <asp:Label ID="lblDriverName1" runat="server" /></h5>
         <%-- <asp:Button ID="btnExportDriverIssues1" runat="server"
                      Text="Export to Excel"
                      CssClass="modern-button1 export-btn"
                      OnClientClick="exportCMFIssues('component_value', '<%= HiddenDriverName.Value %>', 'issue_type_value'); return false;" />
                    --%>

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
            
              <asp:HyperLinkField DataTextField="cp_id"
                                  HeaderText="Sighting ID"
                                  DataNavigateUrlFields="cp_id"
                                  DataNavigateUrlFormatString="https://hsdes.intel.com/appstore/article/#/{0}"
                                  Target="_blank" />
                
              <asp:BoundField DataField="title" HeaderText="Title" />
              <asp:BoundField DataField="component" HeaderText="Component" />
                <asp:BoundField DataField="cmf_request" HeaderText="CMF Request" />
             <asp:BoundField DataField="los" HeaderText="LOS" />
              <%-- Add more columns if you want, e.g. status/component --%>
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
                                                   
                                                   <%-- <asp:TemplateField HeaderText="Implemented">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblImplementedCount" runat="server" Text='<%# Eval("ImplementedCount") %>'></asp:Label><br />
                                                            <asp:Label ID="lblImplementedDetails" runat="server" Text='<%# Eval("ImplementedDetails") %>'></asp:Label>
                                                        </ItemTemplate>
                                                        <ItemStyle Width="33%" />
                                                    </asp:TemplateField>--%>
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
                                            <%--<asp:GridView ID="GridView_milestone_map"
                                                          runat="server"
                                                          AutoGenerateColumns="False"
                                                          CssClass="table-primary"
                                                          EmptyDataText="No CMF data for milestones"
                                                          Visible="true">
                                                <Columns>
                                                    <asp:BoundField DataField="Driver" HeaderText="Driver" ReadOnly="True" ItemStyle-Width="50%" />
                                                    <asp:BoundField DataField="CMFCount" HeaderText="CMF Count" ReadOnly="True" ItemStyle-Width="50%" />
                                                </Columns>
                                            </asp:GridView>--%>

                                       <%-- <asp:GridView ID="GridView_milestone_map"
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
                                                       onclick='showDriverIssues("<%# HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("Driver") ?? "")) %>"); return false;'
                                                       class="btn btn-link p-0">
                                                      <%# Eval("CMFCount") %>
                                                    </a>
                                                  </ItemTemplate>
                                                  <ItemStyle Width="50%" />
                                                </asp:TemplateField>
                                                
                                            </Columns>
                                        </asp:GridView>


                                        --%>  
                                            
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
               onclick='showDriverIssues("<%# HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("Driver") ?? "")) %>"); return false;'
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
                                            <asp:GridView ID="GridView_cmf_summary" CssClass="table-primary7" runat="server" AutoGenerateColumns="False" EmptyDataText="No Open CMFs" Visible="true" DataKeyNames="Component">
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

            </form>

        </div>
                                                                                                                                                                 
    </div>
   

</body>
</html>
