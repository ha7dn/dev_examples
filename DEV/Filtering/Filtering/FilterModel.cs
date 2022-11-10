using Newtonsoft.Json;

namespace Filtering
{
    public class FilterModel:FilterModelBase
    {
        public String? Term { get; set; }
        public DateTime? MinDate { get; set; }
        public Boolean? IncludeInactive { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }

        public FilterModel() : base()
        {
            this.Limit = 3;
        }


        public override object Clone()
        {
            var jsonString = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject(jsonString, this.GetType());
        }
    }
}
