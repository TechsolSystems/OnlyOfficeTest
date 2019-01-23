using System;
using System.IO;
using System.Web;

namespace OnlineEditorsExample
{
    public partial class FileHandler : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string strDocId = Request.QueryString["DocId"];
            string strFileName = DBStore.GetDocName(strDocId);
            string strPhysicalPath = Server.MapPath("~/Files/") + strFileName;

            DBStore.UpdateDocStatus(strDocId, "EDIT_INPROGRESS", "FileHandler.Page_Load: Document " + strFileName + ", sent to OO");

            using (FileStream fs = new FileStream(strPhysicalPath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4096];
                int count = 0;

                while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Response.ContentType = MimeMapping.GetMimeMapping(strPhysicalPath);
                    Response.OutputStream.Write(buffer, 0, count);
                    Response.Flush();
                }
            }
        }
    }
}