using FlujoCompletoCompra.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlujoCompletoCompra
{
    public partial class Checkout : Page
    {
        Paycomet Paycomet;
        //PASAR ID MAESTRO AQUI
        public static string ServicioID = "9e4bcdca2ca544479da82690170d5ef4";

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