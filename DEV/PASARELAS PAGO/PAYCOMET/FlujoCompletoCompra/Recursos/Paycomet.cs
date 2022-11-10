using API_Paycomet_cs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;

namespace FlujoCompletoCompra.Recursos
{
    public class Paycomet
    {
        //----------------------------------------------------------------------------------------------
        // Datos del terminal de la cuenta Paycomet
        //----------------------------------------------------------------------------------------------
        private string account = WebConfigurationManager.AppSettings["MerchantCode"];
        private string terminalid = WebConfigurationManager.AppSettings["Terminal"];
        private string password = WebConfigurationManager.AppSettings["Password"];
        private string endpoint = WebConfigurationManager.AppSettings["endpoint"];
        private string endpointUrl = WebConfigurationManager.AppSettings["endpointUrl"];
        private string jetid = WebConfigurationManager.AppSettings["JetId"];

        private string amount;// Cantidad de una venta en EUR, a modo de ejemplo
        private Paycomet_Bankstore bs = null;// Objeto Paycomet_Bankstore para hacer la llamada a los métodos API
        string ipReal;// Almacenaremos la ip del cliente que ejecuta la compra.

        HttpRequest thisRequest;
        string ServicioID;
        string[,] datosOperacion;
        string[] camposBBDD;
        OperacionesBBDD comunes;
        public Paycomet(HttpRequest request, string servicioID)
        {
            thisRequest = request;
            ServicioID = servicioID;
            comunes = new OperacionesBBDD(ServicioID);
        }


        public string getPaycometUrl()
        {
            string redirectUrl = "checkout.aspx";
            try
            {
                bs = new Paycomet_Bankstore(account, terminalid, password, endpoint, endpointUrl);

            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", Environment.StackTrace, "Error en Inicializacion de pasarela: " + ex.Message);
                errH.SetError();
            }


            if (bs != null)
            {

                //amount = thisRequest.Form["amount"];
                amount = "50";
                string productDescription = comunes.SelectNombreServicio();
                string codigoError = "";
                string estadoPago = "0";
                BankstoreServResponse execute_purchase = bs.ExecutePurchaseUrl(ServicioID, amount, "EUR", "ES", productDescription, "1", "50");

                if (execute_purchase.Result == "OK")
                {
                    estadoPago = "1";
                    redirectUrl = execute_purchase.Data["URL_REDIRECT"];
                }
                else
                {
                    codigoError = execute_purchase.DsErrorId;
                    var errH = new ErrorHandling(execute_purchase.DsErrorId, Environment.StackTrace);
                    errH.SetError();
                }
                camposBBDD = new string[] { "NEWID()", estadoPago, redirectUrl, ServicioID, codigoError, "", "GETDATE()", "GETDATE()" };
                comunes.InsertarSolicitud(camposBBDD, "LogTransacciones");
                camposBBDD = new string[] { bs.OperationData.Type.ToString(), "","",bs.OperationData.StartDate, bs.OperationData.StartDate,"",
                                            ServicioID, execute_purchase.Result, codigoError, "", "", bs.OperationData.Currency, bs.OperationData.Amount,
                                            bs.OperationData.Amount, "es", account, terminalid, bs.OperationData.Concept, "", bs.OperationData.Hash,
                                            bs.OperationData.IdUser, bs.OperationData.TokenUser, bs.OperationData.Secure3D, "", "", "0", "", "", "", "", "" };
                comunes.InsertarSolicitud(camposBBDD, "DatosTransaccionPaycomet");
            }

            return redirectUrl;
        }



        /// <summary>
        /// Devuelve la Ip del cliente que ejecuta la compra
        /// </summary>
        /// <returns>Client IP</returns>
        private string GetIP()
        {
            try
            {
                string Str = "";
                Str = System.Net.Dns.GetHostName();
                IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(Str);
                IPAddress[] addr = ipEntry.AddressList;
                return addr[addr.Length - 2].ToString();
            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", Environment.StackTrace, ex.Message);
                errH.SetError();
                return "";
            }
        }



        //public string getBankAuthUrl()
        //{
        //    string token = thisRequest.Form["paytpvToken"];
        //    amount = thisRequest.Form["amount"];
        //    string jetID = jetid;
        //    string redirectUrl = "/checkout.aspx";
        //    string err = "";

        //    if (!String.IsNullOrEmpty(token) && token.Length == 64)
        //    {
        //        ipReal = GetIP();
        //        string idPayUser = "";// Este id se obtiene en la respuesta, satisfactoria, del método AddUser()
        //        string tokenPayUser = "";// Este token se obtiene en la respuesta, satisfactoria, del método AddUser()
        //        //----------------------------------------------------------------------------------------------
        //        // Creación del objeto Paycomet_Bankstore con los datos del terminal que ejecutara los pagos
        //        //----------------------------------------------------------------------------------------------
        //        try
        //        {
        //            bs = new Paycomet_Bankstore(account, terminalid, password, endpoint, endpointUrl);
        //        }
        //        catch (Exception ex)
        //        {
        //            var errH = new ErrorHandling("0000", Environment.StackTrace, ex.Message);
        //            errH.SetError();
        //        }

        //        //----------------------------------------------------------------------------------------------
        //        // Tokenización de usuario para el proceso de compra
        //        //----------------------------------------------------------------------------------------------
        //        if (bs != null)
        //        {
        //            BankstoreServResponse add_user_token = bs.AddUserToken(token, ipReal, jetID);

        //            if (add_user_token.Result == "OK")
        //            {
        //                //----------------------------------------------------------------------------------------------
        //                // Proceso de compra con datos de producto
        //                //----------------------------------------------------------------------------------------------
        //                idPayUser = add_user_token.Data["DS_IDUSER"];
        //                tokenPayUser = add_user_token.Data["DS_TOKEN_USER"];
        //                string transReference = ServicioID;// Referencia de la operación. No se puede repetir la orden del pedido, será siempre única
        //                string currency = "EUR";// Moneda de la transacción. Listado de monedas: https://docs.paycomet.com/es/documentacion/monedas
        //                string productDescription = new Comunes(ServicioID).SelectCampo("nombreServicio");// Descripción del producto
        //                string scoring = "50";// Valor de scoring de riesgo de la transacción. Entre 0 y 100.
        //                string merchant_data = "{'customer':{'id': '" + idPayUser + "','name': 'haydn','surname': 'pruebas','email': 'mail@mail.com'}}";// Ejemplo de innformación de autenticación del cliente (JSON)
        //                string merchant_description = "Texto en factura"; // Permite al comercio enviar un texto de hasta 25 caracteres que se imprimirá en la factura del cliente. Uso exclusivo de caracteres simples, sin acentos ni caracteres especiales.
        //                string owner = thisRequest.Form["username"];// Propietario de la tarjeta

        //                BankstoreServResponse execute_purchase = bs.ExecutePurchaseTokenUrl(transReference, amount, currency, idPayUser, tokenPayUser, "ES", merchant_description, "1", scoring);

        //                if (execute_purchase.Result == "OK")
        //                {
        //                    redirectUrl = execute_purchase.Data["URL_REDIRECT"];
        //                }
        //                else
        //                {
        //                    var errH = new ErrorHandling(execute_purchase.DsErrorId, Environment.StackTrace);
        //                    errH.SetError();
        //                }
        //            }
        //            else
        //            {
        //                var errH = new ErrorHandling(add_user_token.DsErrorId, Environment.StackTrace);
        //                errH.SetError();
        //            }
        //        }
        //        else
        //        {
        //            var errH = new ErrorHandling("0000", Environment.StackTrace, "paytpvToken no pasado");
        //            errH.SetError();
        //        }

        //    }
        //    return redirectUrl;
        //}
    }
}