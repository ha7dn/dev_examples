using Microsoft.EntityFrameworkCore;
using RelationshipRevision.Models;

namespace RelationshipRevision
{
    public class SchoolContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=192.168.1.180;Database=RelationshipRevision;User Id=sa;Password=%gOn%hAy%vAs%iTz%lAu%;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasOne<StudentAddress>(s => s.Address)
                .WithOne(ad => ad.Student)
                .HasForeignKey<StudentAddress>(ad => ad.AddressOfStudentId);
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAddress> StudentAddresses { get; set; }
    }
}
