namespace AuthGateway.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration_290320221129 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logins",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Dominio = c.String(),
                        User = c.String(),
                        Password = c.String(),
                        FechaUltimaModificacion = c.DateTime(nullable: false),
                        FechaEntrada = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Self = c.String(),
                        RefreshToken = c.String(),
                        ExpiryTime = c.Long(nullable: false),
                        LoginID = c.Guid(nullable: false),
                        FechaUltimaModificacion = c.DateTime(nullable: false),
                        FechaEntrada = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Logins", t => t.LoginID, cascadeDelete: true)
                .Index(t => t.LoginID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tokens", "LoginID", "dbo.Logins");
            DropIndex("dbo.Tokens", new[] { "LoginID" });
            DropTable("dbo.Tokens");
            DropTable("dbo.Logins");
        }
    }
}
