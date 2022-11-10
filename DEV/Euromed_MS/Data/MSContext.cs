using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using static Euromed_MS.Models.Paciente;

namespace Euromed_MS.Data
{
    public class MSContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public MSContext() : base("name=MSContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<Euromed_MS.Models.Paciente>().ToTable("Pacientes");
            //modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<SqlDefaultValueAttribute, string>("SqlDefaultValue", (p, attributes) => attributes.Single().DefaultValue));
        }

        public System.Data.Entity.DbSet<Euromed_MS.Models.Paciente> Pacientes { get; set; }
        public System.Data.Entity.DbSet<Euromed_MS.Models.ErrorLog> ErrorLogs { get; set; }
    }
}
