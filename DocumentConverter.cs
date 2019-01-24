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
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace ASC.Api.DocumentConverter
{
    /// <summary>
    /// Class service api conversion
    /// </summary>
    public static class ServiceConverter
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static ServiceConverter()
        {
            DocumentConverterUrl = WebConfigurationManager.AppSettings["files.docservice.url.converter"] ?? "";
            DocumentCommandServiceUrl = WebConfigurationManager.AppSettings["files.docservice.url.coauthoring.commandservice"] ?? "";

            Int32.TryParse(WebConfigurationManager.AppSettings["files.docservice.timeout"], out ConvertTimeout);
            ConvertTimeout = ConvertTimeout > 0 ? ConvertTimeout : 120000;
        }

        #region private fields

        /// <summary>
        /// Timeout to request conversion
        /// </summary>
        private static readonly int ConvertTimeout;

        /// <summary>
        /// Url to the service of conversion
        /// </summary>
        private static readonly string DocumentConverterUrl;
        /// <summary>
        /// Url to the Command Service
        /// </summary>
        private static readonly string DocumentCommandServiceUrl;

        #endregion

        #region public method

        public static string GetCommandService(string c, string key, string userdata, List<string> users)
        {
            var request = (HttpWebRequest)WebRequest.Create(DocumentCommandServiceUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Timeout = ConvertTimeout;
            var bodyString = string.Empty;
            switch (c)
            {
                case "drop":
                    bodyString = string.Format("{{\"c\":\"{0}\", \"key\":\"{1}\", \"users\":{2} }}", c, key, Newtonsoft.Json.JsonConvert.SerializeObject(users));
                    break;
                case "forcesave":
                    bodyString = string.Format("{{\"c\":\"{0}\", \"key\":\"{1}\", \"userdata\":\"{2}\" }}", c, key, userdata);
                    break;
                case "info":
                    bodyString = string.Format("{{ \"c\":\"{0}\", \"key\":\"{1}\" }}", c, key);
                    break;
                case "version":
                    bodyString = string.Format("{{ \"c\":\"{0}\" }}", c);
                    break;
            }


            var bytes = Encoding.UTF8.GetBytes(bodyString);
            request.ContentLength = bytes.Length;//setting the content length of the request
            using (var requestStream = request.GetRequestStream())//open the request stream to write out data in request
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            string dataResponse;
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) throw new Exception("Response is null");

                using (var reader = new StreamReader(stream))
                {
                    dataResponse = reader.ReadToEnd();
                }
            }
            string output = string.Empty;

            ////Response from the Document Server for the Command service requests:
            //0	No errors.
            //1	Document key is missing or no document with such key could be found.
            //2	Callback url not correct.
            //3	Internal server error.
            //4	No changes were applied to the document before the forcesave command was received.
            //5	Command not correсt.
            //6	Invalid token.
            var res = GetCommandServiceResponse(dataResponse, out output);

            return dataResponse; // output.Replace("Error occurred in the coauthoring/CommandService.ashx: ","");
        }

        /// <summary>
        ///     The method is to convert the file to the required format
        /// </summary>
        /// <param name="documentUri">Uri for the document to convert</param>
        /// <param name="fromExtension">Document extension</param>
        /// <param name="toExtension">Extension to which to convert</param>
        /// <param name="documentRevisionId">Key for caching on service</param>
        /// <param name="isAsync">Perform conversions asynchronously</param>
        /// <param name="convertedDocumentUri">Uri to the converted document</param>
        /// <returns>The percentage of completion of conversion</returns>
        /// <example>
        /// string convertedDocumentUri;
        /// GetConvertedUri("http://helpcenter.onlyoffice.com/content/GettingStarted.pdf", ".pdf", ".docx", "http://helpcenter.onlyoffice.com/content/GettingStarted.pdf", false, out convertedDocumentUri);
        /// </example>
        /// <exception>
        /// </exception>
        public static int GetConvertedUri(string documentUri,
                                          string fromExtension,
                                          string toExtension,
                                          string documentRevisionId,
                                          bool isAsync,
                                          out string convertedDocumentUri)
        {
            convertedDocumentUri = string.Empty;

            fromExtension = string.IsNullOrEmpty(fromExtension) ? Path.GetExtension(documentUri) : fromExtension;

            var title = Path.GetFileName(documentUri);
            title = string.IsNullOrEmpty(title) ? Guid.NewGuid().ToString() : title;

            documentRevisionId = string.IsNullOrEmpty(documentRevisionId)
                                     ? documentUri
                                     : documentRevisionId;
            documentRevisionId = GenerateRevisionId(documentRevisionId);

            var request = (HttpWebRequest)WebRequest.Create(DocumentConverterUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Timeout = ConvertTimeout;

            var bodyString = string.Format("{{\"async\": {0},\"filetype\": \"{1}\",\"key\": \"{2}\",\"outputtype\": \"{3}\",\"title\": \"{4}\",\"url\": \"{5}\"}}",
                                           isAsync.ToString().ToLower(),
                                           fromExtension.Trim('.'),
                                           documentRevisionId,
                                           toExtension.Trim('.'),
                                           title,
                                           documentUri);

            var bytes = Encoding.UTF8.GetBytes(bodyString);
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            //// hack. http://ubuntuforums.org/showthread.php?t=1841740
            //if (_Default.IsMono)
            //{
            //    ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;
            //}

            string dataResponse;
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) throw new Exception("Response is null");

                using (var reader = new StreamReader(stream))
                {
                    dataResponse = reader.ReadToEnd();
                }
            }

            return GetResponseUri(dataResponse, out convertedDocumentUri);
        }

        /// <summary>
        /// Translation key to a supported form.
        /// </summary>
        /// <param name="expectedKey">Expected key</param>
        /// <returns>Supported key</returns>
        public static string GenerateRevisionId(string expectedKey)
        {
            if (expectedKey.Length > 20) expectedKey = expectedKey.GetHashCode().ToString();
            var key = Regex.Replace(expectedKey, "[^0-9-.a-zA-Z_=]", "_");
            return key.Substring(key.Length - Math.Min(key.Length, 20));
        }

        #endregion

        #region private method

        /// <summary>
        /// Processing document received from the editing service
        /// </summary>
        /// <param name="jsonDocumentResponse">The resulting json from editing service</param>
        /// <param name="responseUri">Uri to the converted document</param>
        /// <returns>The percentage of completion of conversion</returns>
        private static int GetResponseUri(string jsonDocumentResponse, out string responseUri)
        {
            if (string.IsNullOrEmpty(jsonDocumentResponse)) throw new ArgumentException("Invalid param", "jsonDocumentResponse");

            var responseFromService = System.Web.Helpers.Json.Decode(jsonDocumentResponse);
            if (jsonDocumentResponse == null) throw new WebException("Invalid answer format");

            var errorElement = responseFromService.error;
            if (errorElement != null) ProcessResponseUriError(Convert.ToInt32(errorElement));

            var isEndConvert = responseFromService.endConvert;

            int resultPercent;
            responseUri = string.Empty;
            if (isEndConvert)
            {
                responseUri = responseFromService.fileUrl;
                resultPercent = 100;
            }
            else
            {
                resultPercent = responseFromService.percent;
                if (resultPercent >= 100) resultPercent = 99;
            }

            return resultPercent;
        }
        private static string GetCommandServiceResponse(string jsonDocumentResponse, out string output)
        {
            if (string.IsNullOrEmpty(jsonDocumentResponse)) throw new ArgumentException("Invalid param", "jsonDocumentResponse");

            var responseFromService = System.Web.Helpers.Json.Decode(jsonDocumentResponse);
            if (jsonDocumentResponse == null)
            {
                //throw new WebException("Invalid answer format");
            }

            string errorMsg = string.Empty;
            int errorElement = -1;
            errorElement = Convert.ToInt32(responseFromService.error);
            if (errorElement != null)
                ProcessCommandServiceError(errorElement, out errorMsg);
            else
                errorElement = -1;

            ////Response from the Document Server for the Command service requests:
            //0	No errors.
            //1	Document key is missing or no document with such key could be found.
            //2	Callback url not correct.
            //3	Internal server error.
            //4	No changes were applied to the document before the forcesave command was received.
            //5	Command not correсt.
            //6	Invalid token.
            output = errorMsg;

            return errorElement.ToString();
        }

        /// <summary>
        /// Generate an error code table
        /// </summary>
        /// <param name="errorCode">Error code</param>
        private static void ProcessCommandServiceError(int errorCode, out string errorMsg)
        {
            var errorMessage = string.Empty;
            const string errorMessageTemplate = "Error occurred in the coauthoring/CommandService.ashx: {0}";

            switch (errorCode)
            {
                case 0:
                    //No errors.
                    break;
                case 1:
                    errorMessage = String.Format(errorMessageTemplate, "Document key is missing or no document with such key could be found.");
                    break;
                case 2:
                    errorMessage = String.Format(errorMessageTemplate, "Callback url not correct.");
                    break;
                case 3:
                    errorMessage = String.Format(errorMessageTemplate, "Internal server error.");
                    break;
                case 4:
                    errorMessage = String.Format(errorMessageTemplate, "No changes were applied to the document before the forcesave command was received.");
                    break;
                case 5:
                    errorMessage = String.Format(errorMessageTemplate, "Command not correсt.");
                    break;
                case 6:
                    errorMessage = String.Format(errorMessageTemplate, "Invalid token.");
                    break;
            }
            errorMsg = errorMessage;
            //throw new Exception(errorMessage);
        }

        /// <summary>
        /// Generate an error code table
        /// </summary>
        /// <param name="errorCode">Error code</param>
        private static void ProcessResponseUriError(int errorCode)
        {
            var errorMessage = string.Empty;
            const string errorMessageTemplate = "Error occurred in the ConvertService.ashx: {0}";

            switch (errorCode)
            {
                case -8:
                    // public const int c_nErrorFileVKey = -8;
                    errorMessage = String.Format(errorMessageTemplate, "Error document VKey");
                    break;
                case -7:
                    // public const int c_nErrorFileRequest = -7;
                    errorMessage = String.Format(errorMessageTemplate, "Error document request");
                    break;
                case -6:
                    // public const int c_nErrorDatabase = -6;
                    errorMessage = String.Format(errorMessageTemplate, "Error database");
                    break;
                case -5:
                    // public const int c_nErrorUnexpectedGuid = -5;
                    errorMessage = String.Format(errorMessageTemplate, "Error unexpected guid");
                    break;
                case -4:
                    // public const int c_nErrorDownloadError = -4;
                    errorMessage = String.Format(errorMessageTemplate, "Error download error");
                    break;
                case -3:
                    // public const int c_nErrorConvertationError = -3;
                    errorMessage = String.Format(errorMessageTemplate, "Error convertation error");
                    break;
                case -2:
                    // public const int c_nErrorConvertationTimeout = -2;
                    errorMessage = String.Format(errorMessageTemplate, "Error convertation timeout");
                    break;
                case -1:
                    // public const int c_nErrorUnknown = -1;
                    errorMessage = String.Format(errorMessageTemplate, "Error convertation unknown");
                    break;
                case 0:
                    // public const int c_nErrorNo = 0;
                    break;
                default:
                    errorMessage = "ErrorCode = " + errorCode;
                    break;
            }

            throw new Exception(errorMessage);
        }

        #endregion
    }
}