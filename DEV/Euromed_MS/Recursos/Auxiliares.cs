using Euromed_MS.Data;
using Euromed_MS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
namespace Euromed_MS.Recursos
{
    public class Auxiliares
    {
        public static string emailDeError = "logs.error@hachedos.com";
        public static bool modoPruebas = Convert.ToBoolean(ConfigurationManager.AppSettings["modoPruebas"]);
        private MSContext context = new MSContext();
        static string thisClassName = "Auxiliares";


        public DateTime StringToDateTime(string fecha)
        {
            DateTime fechaFinal = new DateTime();
            try
            {
                fechaFinal = DateTime.Parse(fecha);
            }
            catch (Exception ex)
            {
                context.ErrorLogs.Add(new ErrorLog()
                {
                    Error = "Error Parseando Fecha: " + fecha,
                    Clase = thisClassName,
                    Mensaje = ex.ToString()
                });
                context.SaveChangesAsync();
            }

            return fechaFinal;
        }



    }
}