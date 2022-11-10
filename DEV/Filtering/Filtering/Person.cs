using System.ComponentModel.DataAnnotations;

namespace Filtering
{
    public class Person
    {
        [Required]
        public int Id { get; set; }
        public String Name { get; set; }
        public DateTime DOB { get; set; }
        public String Email { get; set; }

        public int? MyHomeID { get; set; }
        public Home? MyHome { get; set; }
    }
}
