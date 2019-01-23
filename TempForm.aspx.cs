using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineEditorsExample
{
    public partial class TempForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string strDocId = Request.QueryString["DocId"];
            string strActionType = Request.QueryString["AT"];

            DBStore.UpdateDocStatus(strDocId, strActionType, "TempForm.Page_Load");

            Response.Clear();
            Response.Write("OK");
        }
    }
}