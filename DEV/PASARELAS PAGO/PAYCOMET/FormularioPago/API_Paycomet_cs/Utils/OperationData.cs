using System;
namespace API_Paycomet_cs.Models
{
    public class OperationData
    {
        public int Type { get; set; }
        public string Language { get; set; }
        public string Hash { get; set; }
        public string UrlOk { get; set; }
        public string UrlKo { get; set; }
        public string Reference { get; set; }
        public string Secure3D { get; set; }
        public string Amount { get; set; }
        public string Concept { get; set; }
        public string Currency { get; set; }
        public string Scoring { get; set; }
        public string IdUser { get; set; }
        public string TokenUser { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Periodicity { get; set; }
    }
}
