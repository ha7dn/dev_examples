using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AuthGateway.Models
{
    public class Token
    {
        public Token()
        {
            ID = Guid.NewGuid();
        }

        [Key]
        public Guid ID { get; set; }
        public string Self { get; set; }
        public string RefreshToken { get; set; }
        public long ExpiryTime { get; set; }
        public Guid LoginID { get; set; }
        public Login Login { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public DateTime FechaEntrada { get; set; }
    }
}