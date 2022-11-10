namespace Euromed_MS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DEV_280320221325 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.ErrorLog");
        }
        
        public override void Down()
        {
        }
    }
}
