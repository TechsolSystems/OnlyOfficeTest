<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DocEditor.aspx.cs" Inherits="OnlineEditorsExample.DocEditor" Title="ONLYOFFICE" %>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="OnlineEditorsExample" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript" language="javascript">window.document.domain = "techsollabs.net";</script>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="icon" href="~/favicon.ico" type="image/x-icon" />
    <title>ONLYOFFICE</title>
    <link href="App_Themes/stylesheet.css" rel="stylesheet" />
    <style>
        html {
            height: 100%;
            width: 100%;
        }

        body {
            background: #fff;
            color: #333;
            font-family: Arial, Tahoma,sans-serif;
            font-size: 12px;
            font-weight: normal;
            height: 100%;
            margin: 0;
            overflow-y: hidden;
            padding: 0;
            text-decoration: none;
        }

        form {
            height: 100%;
        }

        div {
            margin: 0;
            padding: 0;
        }
    </style>

    <script language="javascript" type="text/javascript" src="<%= DocServiceApiUri %>"></script>
    <script language="javascript" type="text/javascript" src="script/jquery-1.9.0.min.js"></script>
    <script language="javascript" type="text/javascript" src="script/jquery.blockUI.js"></script>

    <script type="text/javascript" language="javascript">
        function HideBlock() { $.unblockUI(); }
        function ShowBlock() { $.blockUI({ css: { backgroundColor: '', border: '0px' }, overlayCSS: { backgroundColor: '#777', opacity: 0.9, cursor: 'wait' }, message: '' }); }
    </script>

    <script type="text/javascript" language="javascript">
        var docEditor;
        var strFileName = "<%= FileName %>";
        var strDocId = "<%= Key %>"; //"<%= DocId %>";
        var strFileUri = "<%= FileUri %>";
        var strCallbackUrl = "<%= CallbackUrl %>";
        var strFileType = "docx";
        var blnReviewMode = <%= RM %>;

        var blnOkFlag = false;
        var blnDocSaved = false;

        function GetDocEditor()
        {
            return docEditor;
        }

        function GetDocEditorObject()
        {
            var objFrame = document.getElementsByTagName("iframe")[0];
            var objDoc = objFrame.contentDocument || objFrame.contentWindow.document;

            return objDoc;
        }

        function OkClicked()
        {
            blnOkFlag = true;

            // innerAlert('Clicked: ' + GetDocEditorObject().getElementById("id-toolbar-btn-save"));
            GetDocEditorObject().getElementById("id-toolbar-btn-save").click();
        }

        function IsDocumentSaved() {
            return blnDocSaved;
        }

        var innerAlert = function (message) {
            if (console && console.log)
                console.log(message);
        };

        function CleanUp()
        {
            //--- Cleanup
            var objDI = $(GetDocEditorObject()); 

            objDI.contents().find(".box-tabs section.tabs ul li").each(function (i) {
                var a = $(this).children('a');

                if (a != null && ($(a).attr('data-tab') == 'file' || $(a).attr('data-tab') == 'plugins')) {
                    $(a).parent().remove();
                }
            });

            objDI.contents().find("#id-toolbar-btn-settings").remove();  //--- Advanced Settings
            objDI.contents().find("#left-btn-chat").remove();   //--- Chat
            objDI.contents().find("#left-btn-about").remove();  //--- About

            objDI.contents().find("#slot-btn-coauthmode").remove();  //--- Co Auth Settings
            objDI.contents().find("#slot-btn-chat").remove();        //--- Chat from menu
            // objDI.contents().find("#btn-review-view").remove();  //--- Display Mode

            if (!blnReviewMode)
            {
                objDI.contents().find("#left-btn-comments").remove();    //--- left bar connents button
                objDI.contents().find("#tlbtn-addcomment-1").remove();   //--- tool bar connents button
            }

            var objBtnTrackChanges = objDI.contents().find("#btn-review-on");
            var strClass = objBtnTrackChanges.children().attr("class");

            //--- Enable Track changes and hide the buttons
            if (blnReviewMode && strClass.toLowerCase().indexOf("active") < 0)
            {
                  objBtnTrackChanges.children().click();
            }

            objBtnTrackChanges.remove();
            objDI.contents().find("#btn-doc-review").remove();

            $.unblockUI();
        }

        var onReady = function () {
            innerAlert("Document editor ready");
            //  docEditor.showMessage('This is the test message ....  this message comes from onReady event.<BR>');
        };

        var onDocumentReady = function () {
            console.log("Document is loaded");
            CleanUp();
        };

        var onDocumentStateChange = function (event) {
            blnDocSaved = !event.data;
            innerAlert("blnDocSaved: " + blnDocSaved);
        };

        var onError = function (event) {
            if (event)
                innerAlert("OnError: " + event.data);
        };

        var onOutdatedVersion = function (event) {
            innerAlert("onOutdatedVersion: " + event.data);
        };

        var сonnectEditor = function () {
            ShowBlock();

            docEditor = new DocsAPI.DocEditor("iframeEditor",
                {
                    width: "100%",
                    height: "98%",
                    type: "desktop", 
                    documentType: "text",
                    document: {
                        title: 'Title can be anything: ' + strFileName,
                        url: strFileUri,
                        fileType: strFileType,
                        key: strDocId,
                        permissions: {
                            edit: true,
                            download: false,
                            comment: blnReviewMode,
                            print: false,
                            review: blnReviewMode,
                        }
                    },
                    editorConfig: {
                        mode: 'edit',
                        lang: "en",
                        callbackUrl: strCallbackUrl,
                        user: {
                            id: "UserID" + "<%= Session.SessionID %>",
                            name: "Name " + "<%= Session.SessionID.Substring(0, 10) %>",
                        },
                        embedded: {
                            toolbarDocked: "top",
                        },
                        customization: {
                            about: false,
                            feedback: false,
                            autosave: true,
                            compactToolbar: true,
                            forcesave: false,
                            showReviewChanges: true,
                        },
                    },
                    events: {
                        'onAppReady': onReady,
                        'onDocumentReady': onDocumentReady,
                        'onDocumentStateChange': onDocumentStateChange,
                        'onError': onError,
                        'onOutdatedVersion': onOutdatedVersion,
                    }
                });
        };

        if (window.addEventListener) {
            window.addEventListener("load", сonnectEditor);
        } else if (window.attachEvent) {
            window.attachEvent("load", сonnectEditor);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="iframeEditor">
        </div>
    </form>
</body>
</html>