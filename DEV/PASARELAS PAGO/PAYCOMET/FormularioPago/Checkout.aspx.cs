using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FormularioPago
{
    public partial class Checkout : System.Web.UI.Page
    {
        Paycomet Paycomet;
        //PASAR ID MAESTRO AQUI
        public static string ServicioID = "0bc8b61606a34ae4b74cb548937e2bd5";

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btncheckoutpagar_Click(object sender, EventArgs e)
        {
            Paycomet = new Paycomet(Request, ServicioID);
            Response.Redirect(Paycomet.getPaycometUrl());
        }
    }
}