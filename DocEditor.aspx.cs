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
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using ASC.Api.DocumentConverter;

namespace OnlineEditorsExample
{
    public partial class DocEditor : Page
    {
        public static string FileName;
        public static string DocId;
        public static string RM;

        protected void Page_Load(object sender, EventArgs e)
        {
            DocId = Request["DocId"];
            FileName = DBStore.GetDocName(DocId);
            RM = Request["RM"].ToLower();
        }

        public static string FileUri
        {
            get
            {
                var uri = Sample.Host;
                uri.Path = HttpRuntime.AppDomainAppVirtualPath +
                                   (HttpRuntime.AppDomainAppVirtualPath.EndsWith("/") ? "" : "/") +
                                   "FileHandler.aspx";

                uri.Query = "DocId=" + DocId;

                return uri.ToString();

            }
        }

        public static UriBuilder Host
        {
            get
            {
                var uri = new UriBuilder(HttpContext.Current.Request.Url) { Query = "" };
                var requestHost = HttpContext.Current.Request.Headers["Host"];
                if (!string.IsNullOrEmpty(requestHost))
                    uri = new UriBuilder(uri.Scheme + "://" + requestHost);

                return uri;
            }
        }

        protected string Key
        {
            get
            {
                return DocId + DateTime.Now.ToString("-yyyyMMddHHmm");
            }
        }

        protected string DocServiceApiUri
        {
            get { return WebConfigurationManager.AppSettings["files.docservice.url.api"] ?? string.Empty; }
        }

        public static string CallbackUrl
        {
            get
            {
                var callbackUrl = Sample.Host;
                callbackUrl.Path = HttpRuntime.AppDomainAppVirtualPath + 
                                   (HttpRuntime.AppDomainAppVirtualPath.EndsWith("/") ? "" : "/") + 
                                   "webeditor.ashx";

                callbackUrl.Query = "type=track&DocId=" + DocId;

                return callbackUrl.ToString();
            }
        }
    }
}