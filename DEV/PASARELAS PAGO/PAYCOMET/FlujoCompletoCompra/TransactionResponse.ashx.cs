using FlujoCompletoCompra.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FlujoCompletoCompra
{
    /// <summary>
    /// Summary description for TransactionResponse
    /// </summary>
    public class TransactionResponse : IHttpHandler
    {

        Dictionary<string, string> PostData = new Dictionary<string, string>();
        Dictionary<string, string> camposUpdate = new Dictionary<string, string>();
        OperacionesBBDD Comunes;
        public void ProcessRequest(HttpContext context)
        {
            var formDataKeys = context.Request.Form.AllKeys;
            if (formDataKeys.Length > 0)
            {
                foreach (var key in formDataKeys)
                {
                    if (key != null)
                    {
                        PostData.Add(key, context.Request.Form.Get(key)); 
                    }
                }

                if (PostData.ContainsKey("TransactionName") && PostData.ContainsKey("Order"))
                {
                    string transactionType = PostData["TransactionType"];
                    string orderNum = PostData["Order"];
                    Comunes = new OperacionesBBDD(orderNum);
                    switch (transactionType)
                    {
                        case "1":
                            UpdatePayment();
                            break;
                        case "2":
                        case "3":
                        case "4":
                        case "107":
                        case "6":
                        case "9":
                        case "13":
                        case "14":
                        case "16":
                        case "106":
                            InsertNewTransaction(transactionType, orderNum);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Error Post Notificacion Banco: ServicioID no encontrado ");
                    var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                    errH.SetError();

                    context.Response.Write(sb.ToString());
                } 
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error Post Notificacion Banco: Post vacio ");
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();

                context.Response.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Actualiza el estado de pago de una operacion
        /// </summary>
        /// <param name="transactionType">tipo de transaccionm</param>
        /// <param name="order">num solicitud </param>
        private void UpdatePayment()
        {
            foreach (var keyVal in PostData)
            {
                camposUpdate.Add(keyVal.Key, keyVal.Value);
            }

            try
            {
                if (camposUpdate["ErrorID"] != "")
                {
                    Comunes.UpsertErrorCheckout(camposUpdate["ErrorID"], camposUpdate["ErrorDescription"]);
                }
                Comunes.UpdateCamposPaycomet(camposUpdate);
                Comunes.UpdateEstadoPago(PostData["Response"].ToString(), camposUpdate["ErrorID"]);
            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", Environment.StackTrace, ex.Message);
                errH.SetError();
            }
        }

        /// <summary>
        /// Inserta nuevos datos de otro tipo de transacciones distintos a PAGO OK/KO
        /// </summary>
        /// <param name="transactionType">Tipo transaccion</param>
        /// <param name="order">num solicitud</param>
        private void InsertNewTransaction(string transactionType, string order)
        {
            OperacionesBBDD Comunes = new OperacionesBBDD(order);

            try
            {

            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", Environment.StackTrace, ex.Message);
                errH.SetError();
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