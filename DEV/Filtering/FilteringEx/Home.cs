namespace Filtering
{
    public class Home
    {

        public int Id { get; set; }
        public string Address { get; set; }
        public bool Rental { get; set; }
        public float Cost { get; set; }
        public int Inhabitants { get; set; }

        public IEnumerable<Person>? Persons { get; set; }

    }
}
