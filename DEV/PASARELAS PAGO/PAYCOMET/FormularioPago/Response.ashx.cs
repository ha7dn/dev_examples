using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FormularioPago
{
    /// <summary>
    /// Summary description for Response
    /// </summary>
    public class Response : IHttpHandler
    {
        Paycomet Paycomet;
        //public static string ServicioID = "";
        public static string ServicioID = "0bc8b61606a34ae4b74cb548937e2bd5";
        string RedirectURL = "";

        public void ProcessRequest(HttpContext context)
        {
            //ServicioID = GetServicioIDFromCookie(context.Request);
            Dictionary<string, string> strFormData = new Dictionary<string, string>();


            var formData = context.Request.Form; // Get the form object from the current HTTP request.
            if (formData != null || formData["paytpvToken"] != "" || formData["paytpvToken"] != null)
            {
                if (ServicioID != null || ServicioID != "")
                {
                    Paycomet = new Paycomet(context.Request, ServicioID);
                    RedirectURL = Paycomet.getBankAuthUrl();
                }
                else
                {
                    RedirectURL = "/checkout.aspx";
                    var errH = new ErrorHandling("0000", Environment.StackTrace, "ServicioID no pasado");
                    errH.SetError();
                }
            }
            else
            {
                RedirectURL = "/checkout.aspx";
                var errH = new ErrorHandling("0000", Environment.StackTrace, "paytpvToken no pasado");
                errH.SetError();
            }
            context.Response.Redirect(RedirectURL);
        }

        private string GetServicioIDFromCookie(HttpRequest request)
        {
            string id = "";
            try
            {
                HttpCookie cookienumerosolicitudrecuperada = request.Cookies["cookienumerosolicitud"];
                id = cookienumerosolicitudrecuperada["gs"].ToString();
                return id;
            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", Environment.StackTrace, ex.Message);
                errH.SetError();
                return null;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}