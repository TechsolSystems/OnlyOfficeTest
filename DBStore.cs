using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace OnlineEditorsExample
{
    public class DBStore
    {
        private static List<DocInfo> lsDocs = new List<DocInfo>();
        private static List<DocInfoStatus> lsDocStatus = new List<DocInfoStatus>();

        public static void Clear()
        {
            lsDocs.Clear();
            lsDocStatus.Clear();
        }

        public static DocInfo CreateNewDoc(string strDocName)
        {
            DocInfo objDoc = new DocInfo()
            {
                DocId = Guid.NewGuid().ToString(),
                DocName = strDocName,
                Status = "CREATED"
            };

            lsDocs.Add(objDoc);

            DBStore.AddStatus(objDoc.DocId, "CREATED", "New Document Created.");

            return objDoc;
        }

        public static void UpdateDocStatus(string strDocId, string strStatus, string strMessage)
        {
            foreach (DocInfo objDI in lsDocs)
            {
                if (objDI.DocId == strDocId)
                {
                    if (strStatus == "")
                    {
                        strStatus = objDI.Status;
                    }

                    objDI.Status = strStatus;

                    DBStore.AddStatus(strDocId, strStatus, strMessage);
                }
            }
        }

        public static void AddStatus(string strDocId, string strStatus, string strMessage)
        {
            DocInfoStatus objDIS = new DocInfoStatus()
            {
                LoggedTime = DateTime.Now,
                DocId = strDocId,
                Status = strStatus,
                Message = strMessage
            };

            lsDocStatus.Add(objDIS);
        }

        public static List<DocInfo> GetDocList()
        {
            return lsDocs;
        }

        public static string GetDocName(string strDocId)
        {
            string strName = "";

            foreach (DocInfo objDI in lsDocs)
            {
                if (objDI.DocId == strDocId)
                {
                    strName = objDI.DocName;
                }
            }
            
            return strName;
        }

        public static string GetDocStatus(string strDocId)
        {
            string strStatus = "";

            foreach (DocInfo objDI in lsDocs)
            {
                if (objDI.DocId == strDocId)
                {
                    strStatus = objDI.Status;
                }
            }

            return strStatus;
        }

        public static string GetDocLogs()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div style='font-family:monospace;'><OL>");

            foreach (DocInfo objDI in lsDocs)
            {
                sb.AppendFormat("<LI>{0} :: {1} :: {2}<BR>", objDI.DocId, objDI.DocName, objDI.Status);
                sb.AppendFormat("{0}</LI>", DBStore.GetDocStatusLogs(objDI.DocId));
            }

            sb.Append("</OL></DIV>");

            return sb.ToString();
        }

        public static string GetDocStatusLogs(string strDocId)
        {
            StringBuilder sbStatus = new StringBuilder();

            sbStatus.Append("<OL>");

            foreach (DocInfoStatus objStatus in lsDocStatus)
            {
                if (objStatus.DocId == strDocId)
                {
                    sbStatus.AppendFormat("<LI>{0} :: {1} :: {2}</LI>", objStatus.LoggedTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), objStatus.Status, objStatus.Message);
                }
            }

            sbStatus.Append("</OL><BR>");

            return sbStatus.ToString();
        }
    }

    public class DocInfo
    {
        public string DocId { get; set; }
        public string DocName { get; set; }
        public string Status { get; set; }
    }

    public class DocInfoStatus
    {
        public DateTime LoggedTime { get; set; }
        public string DocId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}