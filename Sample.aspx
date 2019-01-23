<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Sample.aspx.cs" Inherits="OnlineEditorsExample.Sample" Title="Sample" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="OnlineEditorsExample" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <script type="text/javascript" language="javascript">window.document.domain = "techsollabs.net";</script>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Sample</title>

    <link rel="icon" href="~/favicon.ico" type="image/x-icon" />

    <link rel="stylesheet" type="text/css" href="https://fonts.googleapis.com/css?family=Open+Sans:900,800,700,600,500,400,300&subset=latin,cyrillic-ext,cyrillic,latin-ext" />
    <link href="App_Themes/stylesheet.css" rel="stylesheet" />
    <link href="App_Themes/jquery-ui.css" rel="stylesheet" />

    <script language="javascript" type="text/javascript" src="script/jquery-1.9.0.min.js"></script>
    <script language="javascript" type="text/javascript" src="script/jquery-ui.min.js"></script>
    <script language="javascript" type="text/javascript" src="script/jquery.blockUI.js"></script>

    <script language="javascript" type="text/javascript">
        var objDialog = null;
        var objCW = null;
        var intTimer = 0;
        var jq = null;

        function CallSync(strDocId, strActType) {
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "TempForm.aspx?DocId=" + strDocId + "&AT=" + strActType, false);
            xhttp.send();
            var strRes = xhttp.responseText;

            console.log('CallSync.Result: ' + strRes);

            if (strRes == 'OK') {
                return true;
            }
            else {
                return false;
            }
        }

        function GetStatusSync(strDocId) {
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", "GetStatus.aspx?DocId=" + strDocId, false);
            xhttp.send();
            var strRes = xhttp.responseText;

            console.log('GetStatusSync.Result: ' + strRes);

            return strRes;
        }

        function openPop(strDocId, strDocName, blnRFlag) {
            if (typeof jQuery != "undefined") {
                jq = jQuery.noConflict();

                CallSync(strDocId, 'POPUP_OPEN');

                var intSH = screen.availHeight - 150;
                intSH = (intSH * 95) / 100;

                objDialog = jq("#mainProgress").dialog({
                    resizable: false,
                    height: intSH,
                    width: "95%",
                    modal: true,
                    title: "Edit Mode"
                });

                objDialog.siblings('div.ui-dialog-titlebar').remove();

                jq('#hidDocId').val(strDocId);
                jq('#hidDocName').val(strDocName);
                var url = "doceditor.aspx?DocId=" + encodeURIComponent(strDocId) + "&RM=" + blnRFlag;

                jq("#mainProgress").addClass("embedded");
                jq("#embeddedView").attr("src", url);

                objDialog.dialog("option", "height", intSH);

                objCW = document.getElementById('embeddedView').contentWindow;
            }
        }

        function CloseOK()
        {
            setTimeout('ShowBlock()', 1);

            if (CallSync(jq('#hidDocId').val(), 'OkClicked'))
            {
                objCW.OkClicked();
                jq('#mess').show();

                setTimeout('CheckDocumentSavedStatus()', 500);
            }
        }

        function CheckDocumentSavedStatus()
        {
            console.log('CheckDocumentSavedStatus: ' + objCW.IsDocumentSaved());

            if (objCW.IsDocumentSaved())
            {
                CallSync(jq('#hidDocId').val(), 'EDITOR_CLOSED');
                objCW.GetDocEditor().destroyEditor();
                jq('#hiddenFileName').val("");
                jq("#embeddedView").attr("src", '');
                jq('#mess').hide();
                objDialog.dialog("destroy");

                setTimeout('CheckStatus1()', 500);
            }
            else
            {
                console.log("Else: " + intTimer);
                setTimeout('CheckDocumentSavedStatus()', 1000);
                intTimer++;
            }
        }

        function CheckStatus1()
        {
            if (GetStatusSync(jq('#hidDocId').val()) == 'MustSave')
            {
                HideBlock();
            }
            else
            {
                console.log("Else: " + intTimer);
                setTimeout('CheckStatus1()', 1000);
                intTimer++;
            }
        }

        function ClosePopup() {
            if (CallSync(jq('#hidDocId').val(), 'CancelClicked')) {
                objCW.GetDocEditor().destroyEditor();
                jq('#hiddenFileName').val("");
                jq("#embeddedView").attr("src", '');
                jq('#mess').hide();
                objDialog.dialog("destroy");
            }

            return false;
        }

        function ShowBlock() { jq.blockUI({ css: { backgroundColor: '', border: '0px' }, overlayCSS: { backgroundColor: '#000', opacity: 0.7, cursor: 'wait' }, message: 'Wait Processing.....' }); }
        function HideBlock() { jq.unblockUI(); }
    </script>
</head>
<body>
    <form id="form2" runat="server">
        <div class="main-panel">
            <br />
            <% var storedFiles = GetStoredFiles();
               if (storedFiles.Any())
               { %>
            <div class="help-block">
                <span>Your documents</span>
                <br /><br />
                <ul>
                    <% foreach (var storedFile in storedFiles)
                       { %>
                    <li class="clearFix">
                        <div class="stored-edit text" onclick="openPop('<%= storedFile.DocId %>', '<%= storedFile.DocName %>', false)">
                            <span title="<%= storedFile.DocId %>"><%= storedFile.DocName %></span>
                        </div>
                        <div class="stored-edit" style="cursor: pointer;" onclick="openPop('<%= storedFile.DocId %>', '<%= storedFile.DocName %>', true)">
                            <span title="<%= storedFile.DocId %>">Review</span>
                        </div>
                        <span>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a class="stored-download" href="files/<%=storedFile.DocName%>">Download</a></span>
                    </li>
                    <% } %>
                </ul>
            </div>
            <% } %>
        </div>
        <div>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnClearData" Text="  Clear Data  " OnClick="btnClearData_Click" runat="server" />
            <span>Clearing data will clear data stored for all users.</span>
        </div>

        <div id="mainProgress">
            <input type="hidden" name="hidDocId" id="hidDocId" />
            <input type="hidden" name="hidDocName" id="hidDocName" />

            <iframe id="embeddedView" src="" height="94%" width="99%" frameborder="0" scrolling="no" allowtransparency></iframe>
            <div id="divCloseOk" onclick="CloseOK();" class="button green">Ok</div>
            <div onclick="ClosePopup();"  class="button green" >Cancel</div>
            &nbsp;&nbsp;&nbsp;&nbsp;<span id="mess" style="display:none;">Wait Processing</span>
        </div>
    </form>
</body>
</html>