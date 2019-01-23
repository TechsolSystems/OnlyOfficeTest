using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineEditorsExample
{
    public partial class GetStatus : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string strDocId = Request.QueryString["DocId"];

            string strStatus = DBStore.GetDocStatus(strDocId);

            Response.Clear();
            Response.Write(strStatus);
        }
    }
}