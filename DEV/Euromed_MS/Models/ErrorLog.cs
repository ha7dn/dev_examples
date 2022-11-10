using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Euromed_MS.Models
{
    public class ErrorLog
    {
        static string filePath = ConfigurationManager.AppSettings["CarpetaLogs"];
        public ErrorLog()
        {
            ID = Guid.NewGuid();
            Fecha = DateTime.Now;
        }

        public Guid ID { get; set; }
        public string Clase { get; set; }
        public string Error { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }

        public void AddLinetoErrorTxt(string line)
        {
            File.WriteAllText( filePath + "ErroLog_ " + Guid.NewGuid().ToString() + ".txt", line);
        }

    }
}