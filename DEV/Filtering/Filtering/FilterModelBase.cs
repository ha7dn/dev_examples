namespace Filtering
{
    public abstract class FilterModelBase : ICloneable
    {
        public int Page { get; set; }
        public int Limit { get; set; }

        public FilterModelBase()
        {
            this.Page = 1;
            this.Limit = 100;
        }

        public abstract object Clone();
    }
}
