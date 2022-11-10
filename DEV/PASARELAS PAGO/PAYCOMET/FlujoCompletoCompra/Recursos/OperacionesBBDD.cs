using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace FlujoCompletoCompra.Recursos
{
    public class OperacionesBBDD
    {
        private string QueryBBDD;
        private string ServicioID;
        public string[] BBDDValues = new string[] { };
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["SKNETgestrafico"].ConnectionString;
        public static string pagina = ConfigurationManager.AppSettings["Pagina"];

        public OperacionesBBDD(string servicioID)
        {
            ServicioID = servicioID;
        }

        public void UpdateCamposPaycomet(Dictionary<string,string> keyvalues)
        {
            QueryBBDD = "UPDATE [dbo].[DatosTransaccionPaycomet] SET ";
            string campos = "";
            foreach(var keyVal in keyvalues)
            {
                string valor = keyVal.Value;
                if (valor == "NEWID()" || valor == "GETDATE()")
                {
                    QueryBBDD += "["+ keyVal.Key +"] = " + valor + ",";
                }
                else if (valor == "")
                {
                    QueryBBDD += "[" + keyVal.Key + "] = " + "NULL,";
                }
                else
                {
                    QueryBBDD += "[" + keyVal.Key + "] = '" + valor + "',";
                }
            }
            QueryBBDD = QueryBBDD.Substring(0, QueryBBDD.LastIndexOf(',')) + " WHERE [Order] = '" + ServicioID + "'";
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                SqlCommand command = new SqlCommand(QueryBBDD, connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error UpdateCamposPaycomet() - " + QueryBBDD);
                sb.AppendLine(ex.Message);
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();
            }
        }

        public void UpdateEstadoPago(string estadoPaycomet, string codigoError = "")
        {
            QueryBBDD = "UPDATE [dbo].[LogTransacciones] SET EstadoPago = ";
            string estadoPago = "";
            string error = "";
            if (estadoPaycomet == "OK")
            {
                estadoPago = "2";
            }
            else if (estadoPaycomet == "KO")
            {
                estadoPago = "4";
                error = " ,[CodigoError] = '" + codigoError + "', [fechaError] = GETDATE()";
            }
            else
            {
                estadoPago = "3";
                error = " ,[CodigoError] = '" + codigoError + "', [fechaError] = GETDATE()";
            }

            QueryBBDD += estadoPago + error + " ,fechaModificacion = GETDATE() WHERE [ServicioID_id] = '" + ServicioID + "'";
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                SqlCommand command = new SqlCommand(QueryBBDD, connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error UpdateEstadoPago() - " + QueryBBDD);
                sb.AppendLine(ex.Message);
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();
            }

            UpdateEstadoPagoMaestra(estadoPaycomet);
        }

        public void UpdateEstadoPagoMaestra(string estadoPaycomet)
        {
            QueryBBDD = "UPDATE [dbo].[datosCompleto_tablamaestra] SET EstadoPago = ";
            string estadoPago = "";
            if (estadoPaycomet == "OK")
            {
                estadoPago = "2";
            }
            else if (estadoPaycomet == "KO")
            {
                estadoPago = "4";
            }
            else
            {
                estadoPago = "3";
            }

            QueryBBDD += estadoPago + " ,fechaModificacion = GETDATE() WHERE [GUID_maestro] = '" + ServicioID + "'";
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                SqlCommand command = new SqlCommand(QueryBBDD, connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error UpdateEstadoPagoMaestra() - " + QueryBBDD);
                sb.AppendLine(ex.Message);
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();
            }

        }

        public bool InsertarSolicitud(string[] camposBBDD, string tabla)
        {
            QueryBBDD = "INSERT INTO [dbo].[" + tabla + "] VALUES(";
            foreach (var valor in camposBBDD)
            {
                if (valor == "NEWID()" || valor == "GETDATE()")
                {
                    QueryBBDD += valor + ",";
                }
                else if (valor == "")
                {
                    QueryBBDD += "NULL,";
                }
                else
                {
                    QueryBBDD += "'" + valor + "',";
                }
            }
            QueryBBDD = QueryBBDD.Substring(0,QueryBBDD.LastIndexOf(',')) + ")";
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                SqlCommand command = new SqlCommand(QueryBBDD, connection);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return true;

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error InsertarSolicitud() - " + QueryBBDD);
                sb.AppendLine(ex.Message);
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();
                return false;

            }
        }

        public string SelectNombreServicio()
        {
            string value = "";
            QueryBBDD = "SELECT [nombreBonito] FROM [dbo].[recursos_nombresservicios] WHERE [GUID_nombreServicio] =" +
                        "(SELECT[nombreServicio_id]  FROM [dbo].[datosCompleto_tablamaestra] WHERE [GUID_maestro] = '"+ServicioID+"')";
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                SqlCommand command = new SqlCommand(QueryBBDD, connection);
                connection.Open();
                SqlDataReader dataReader = command.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        value = dataReader[0].ToString();
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error SelectNombreServicio() - " + QueryBBDD);
                sb.AppendLine(ex.Message);
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();
                return "";
            }
            return value;
        }

        public void UpsertErrorCheckout(string errorID, string errorDesc)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand("UpsertCodigoError", connection))
                {
                    cmd.Parameters.Add("@CodigoError", errorID);
                    cmd.Parameters.Add("@ErrorDesc", errorDesc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error UpsertErrorCheckout() - " + QueryBBDD);
                sb.AppendLine(ex.Message);
                var errH = new ErrorHandling("0000", Environment.StackTrace, sb.ToString());
                errH.SetError();
            }
        }

    }
}