using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace FormularioPago
{
    public class Auxiliares
    {
        private string QueryBBDD;
        private string ServicioID;
        public string[] BBDDValues = new string[] { };
        private static string ConnectionString = ConfigurationManager.ConnectionStrings["SKNETgestrafico"].ConnectionString;
        public static string pagina = ConfigurationManager.AppSettings["Pagina"];

        public Auxiliares(string servicioID)
        {
            ServicioID = servicioID;
        }

        public void UpdateCampoBBD(string[,] keyvalues, string tabla)
        {
            foreach (var keyVal in keyvalues)
            {
                // TODO: ACTUALIZAR CAMPOS EN BBDD
            }
        }

        public string[] SelectSolicitudCompleta(string condicionSelect, string tabla)
        {
            QueryBBDD = "SELECT TOP (1) * FROM [dbo].[" + tabla + "] WHERE " + condicionSelect + " order by fechaCreacion desc";
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
                        BBDDValues = new string[dataReader.FieldCount];
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            BBDDValues[i] = dataReader[i].ToString();
                        }
                    }
                }
                connection.Close();
                return BBDDValues;
            }
            catch (Exception ex)
            {
                var errH = new ErrorHandling("0000", "Error SelectSolicitudCompletaDG(). <br/>" + QueryBBDD + "<br/>" + "<br/><br/>" + ex);
                errH.SetError();
                return BBDDValues;
            }
        }

        public bool InsertarSolicitud(string[] camposBBDD, string tabla)
        {
            QueryBBDD = "INSERT INTO [dbo].[" + tabla + "] (" + camposBBDD[0] + ") VALUES(" + camposBBDD[1] + ")";
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
                var errH = new ErrorHandling("0000", "Error InsertarSolicitud(). <br/>" + QueryBBDD + "<br/><br/>" + ex);
                errH.SetError();
                return false;

            }
        }

        public string SelectCampo(string nombreColumna)
        {
            string value = "";
            QueryBBDD = "";
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

            }
            return value;
        }
    }
}