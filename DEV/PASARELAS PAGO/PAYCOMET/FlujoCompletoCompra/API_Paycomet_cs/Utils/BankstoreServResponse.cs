using System;
using System.Collections.Generic;

namespace API_Paycomet_cs.Models
{
    public class BankstoreServResponse
    {
        public string Result { get; set; }
        public string DsErrorId { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public BankstoreServResponse()
        {
            DsErrorId = "-1";//Default Not Error Value
        }
    }
}
