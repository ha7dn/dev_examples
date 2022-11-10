namespace RelationshipRevision.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public StudentAddress Address { get; set; }
    }

}
