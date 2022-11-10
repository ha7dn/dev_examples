using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace GuidFormatGenerator
{
    public class DBFunctions
    {
        static string ConnectionString = ConfigurationManager.ConnectionStrings["SKNETgestraficoS"].ConnectionString;
        string ErrorMsg;

        /// <summary>
        /// Metodo de SQL para invocar a un procedimiento almacenado de BBDD
        /// que se encarga del formateo, inserción y devolucion de un GUID
        /// </summary>
        /// <returns>UniqueIdentifier en mayusculas sin guiones</returns>
        public string GetGuid()
        {
            string result = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand("InsertFormatGuid", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        while (reader.Read())
                        {
                            result = reader.GetString(0);
                        }
                    }
                    else
                    {
                        ErrorMsg = "no se encontraron datos en la BBDD";
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Hubo un error conectando la BBDD");
                sb.AppendLine(ex.ToString());
                sb.AppendLine(ex.Message);

                ErrorMsg = sb.ToString();   
                result = ErrorMsg; 
            }

            return result;
        }
    }
}