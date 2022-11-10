using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AuthGateway.Models
{
    public class Login
    {
        public Login()
        {
            ID = Guid.NewGuid();
        }

        [Key]
        public Guid ID { get; set; }
        public string Dominio { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public DateTime FechaEntrada { get; set; }
        public IList<Token> Tokens { get; set; }
    }
}