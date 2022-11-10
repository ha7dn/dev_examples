using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormularioPago
{
    /// <summary>
    /// Summary description for TransactionRes
    /// </summary>
    public class TransactionRes : IHttpHandler
    {
        Dictionary<string, string> PostData = new Dictionary<string, string>();
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
            var formDataKeys = context.Request.Form.AllKeys;
            foreach (var key in formDataKeys)
            {
                PostData.Add(context.Request.Form[key], context.Request.Form.Get(key));
            }

            if (PostData.ContainsKey("TransactionName"))
            {
                string transactionType = PostData["TransactionName"];
                string orderNum = PostData["Order"];
                switch (transactionType)
                {
                    case "Autorización":
                        UpdatePayment(transactionType, orderNum);
                        break;
                    case "Devolución":
                        InsertNewTransaction(transactionType, orderNum);
                        break;
                    case "Bankstore IFrame add_user":
                        InsertNewTransaction(transactionType, orderNum);
                        break;
                    default:
                        break;
                }
            }
        }

        private void InsertNewTransaction(string transactionType, string order)
        {
            Auxiliares Auxiliares = new Auxiliares(order);

            try
            {

            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", Environment.StackTrace, ex.Message);
                errH.SetError();
            }
        }

        private void UpdatePayment(string transactionType, string order)
        {
            Auxiliares Auxiliares = new Auxiliares(order);

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