/*
 *
 * (c) Copyright Ascensio System Limited 2010-2017
 *
 * The MIT License (MIT)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using ASC.Api.DocumentConverter;
using System.Collections;

namespace OnlineEditorsExample
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class WebEditor : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string strDocId = context.Request["DocId"];
            DBStore.UpdateDocStatus(strDocId, "", "WebEditor.ProcessRequest: " + context.Request["type"] + " : " + context.Request.UserHostName);

            switch (context.Request["type"])
            {
                case "track":
                    Track(context);
                    break;
            }
        }

        private static void Track(HttpContext context)
        {
            string strDocId = context.Request["DocId"];
            string strFileName = DBStore.GetDocName(strDocId);
            string strBody = "";
            string strStatus = "0";

            try
            {
                using (var receiveStream = context.Request.InputStream)
                {
                    using (var readStream = new StreamReader(receiveStream))
                    {
                        strBody = readStream.ReadToEnd();
                    }
                }

                DBStore.AddStatus(strDocId, "WebEditor.Track", strBody);
            }
            catch (Exception e)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, e.Message);
            }

            if (string.IsNullOrEmpty(strBody))
            {
                return;
            }

            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dictionary<string, object> dicResponse = jss.Deserialize<Dictionary<string, object>>(strBody);

            string strDocKey = "";
            int intStatusVal = 0;
            TrackerStatus enumStatus = TrackerStatus.NotFound;
            string strFileDownloadUrl = "";
            string strUser = "";

            if (dicResponse.ContainsKey("key"))
            {
                strDocKey = dicResponse["key"].ToString();
            }

            if (dicResponse.ContainsKey("status"))
            {
                intStatusVal = Convert.ToInt32(dicResponse["status"]);
                enumStatus = (TrackerStatus)intStatusVal;
            }

            if (dicResponse.ContainsKey("url"))
            {
                strFileDownloadUrl = dicResponse["url"].ToString();
            }

            if (dicResponse.ContainsKey("users"))
            {
                strUser = (dicResponse["users"] as ArrayList)[0].ToString();
            }

            DBStore.UpdateDocStatus(strDocId, "", "WebEditor.Track: " + enumStatus + " : " + intStatusVal.ToString());
            DBStore.AddStatus(strDocId, "strDocKey", strDocKey);
            DBStore.AddStatus(strDocId, "strUser", strUser);
            DBStore.AddStatus(strDocId, "strFileDownloadUrl", strFileDownloadUrl);

            switch (enumStatus)
            {
                case TrackerStatus.MustSave:
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strFileDownloadUrl);

                    try
                    {
                        var storagePath = Sample.StoragePath(strFileName);

                        if (DBStore.GetDocStatus(strDocId) == "OkClicked" ||
                            DBStore.GetDocStatus(strDocId) == "EDITOR_CLOSED")
                        {
                            //--- This is the document stream sent from Document Service
                            using (var stream = req.GetResponse().GetResponseStream())
                            {
                                if (stream == null) throw new Exception("stream is null");
                                const int bufferSize = 4096;

                                using (var fs = File.Open(storagePath, FileMode.Create))
                                {
                                    var buffer = new byte[bufferSize];
                                    int readed;
                                    while ((readed = stream.Read(buffer, 0, bufferSize)) != 0)
                                    {
                                        fs.Write(buffer, 0, readed);
                                    }
                                }
                            }

                            DBStore.UpdateDocStatus(strDocId, enumStatus.ToString(), "WebEditor.Track: Saved Path: " + storagePath);

                            DBStore.AddStatus(strDocId, "Command Service", "Start");
                            string strRes = ServiceConverter.GetCommandService("drop", strDocKey, "", (new List<string>() { strUser }));
                            DBStore.AddStatus(strDocId, "Command Service", strRes);
                        }
                        else
                        {
                            DBStore.UpdateDocStatus(strDocId, enumStatus.ToString(), "WebEditor.Track: NOT SAVED");
                        }
                    }
                    catch (Exception ex)
                    {
                        DBStore.AddStatus(strDocId, "Exception", ex.ToString());
                    }

                    break;
                case TrackerStatus.Corrupted:
                case TrackerStatus.ForceSave:
                case TrackerStatus.Closed:
                    break;
                case TrackerStatus.Editing:
                    //--- Check User and Key combination and set the status of document 
                    strStatus = "0";
                    break;
            }

            context.Response.Write("{\"error\":" + strStatus + "}");
        }

        private enum TrackerStatus
        {
            NotFound = 0,
            Editing = 1,
            MustSave = 2,
            Corrupted = 3,
            Closed = 4,
            ForceSave = 6,
        }

        private static void Save(HttpContext context)
        {
            ////---var downloadUri = context.Request["fileuri"];
            //var fileName = HttpUtility.UrlDecode(context.Request["filename"]);
            ////---var hsCode = (_Default.CurUserHostAddress(null) + "/" + Path.GetFileName(downloadUri) + "/" + File.GetLastWriteTime(_Default.StoragePath(fileName, null))).GetHashCode().ToString();
            //var hsCode = (_Default.CurUserHostAddress(null) + "/" + fileName + "/" + File.GetLastWriteTime(_Default.StoragePath(fileName, null))).GetHashCode().ToString();
            //var key = ServiceConverter.GenerateRevisionId(hsCode);

            //var result = ServiceConverter.GetCommandService("forcesave", key, "Force Save", null);
            //_Default.ForceSaveResult = result;

            //context.Response.Write("{ \"error\": \"" + result + "\"}");
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}