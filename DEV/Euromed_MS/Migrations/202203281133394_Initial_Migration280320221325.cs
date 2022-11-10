namespace Euromed_MS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial_Migration280320221325 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ErrorLogs",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Clase = c.String(),
                        Error = c.String(),
                        Mensaje = c.String(),
                        Fecha = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Pacientes",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        NombreAplicacion = c.String(),
                        NombreFormulario = c.String(),
                        AppId = c.Long(nullable: false),
                        ZohoId = c.String(),
                        FechaCreacion = c.String(),
                        Nombre = c.String(),
                        Apellido1 = c.String(),
                        Apellido2 = c.String(),
                        Genero = c.String(),
                        Email = c.String(),
                        TelefonoPaciente = c.String(),
                        DocId = c.String(),
                        FechaNacimiento = c.String(),
                        Localidad = c.String(),
                        Firma = c.String(),
                        TipoPrueba = c.String(),
                        VelocidadPrueba = c.String(),
                        CodigoMuestra = c.String(),
                        Resultado = c.String(),
                        FechaRecepcionMuestra = c.String(),
                        HoraRecepcionMuestra = c.String(),
                        CargaViral = c.String(),
                        IGG = c.String(),
                        IGM = c.String(),
                        EstadoEnvio = c.String(),
                        EstadoDocumento = c.String(),
                        EstadoNotificacion = c.String(),
                        NombreComprador = c.String(),
                        Direccion = c.String(),
                        Ciudad = c.String(),
                        Provincia = c.String(),
                        CodigoPostal = c.String(),
                        Pais = c.String(),
                        TelefonoComprador = c.String(),
                        EmailComprador = c.String(),
                        CodigoReserva = c.String(),
                        CodigoTicket = c.String(),
                        Precio = c.String(),
                        FechaEmisionCertificado = c.String(),
                        PresentaSintomas = c.String(),
                        VolarViajar = c.String(),
                        Contacto_Positivo = c.String(),
                        FechaReserva = c.DateTime(),
                        HoraReserva = c.DateTime(),
                        EstadoPago = c.String(),
                        IdDocumento = c.String(),
                        Notas = c.String(),
                        IDAfiliado = c.String(),
                        Ubicacion = c.String(),
                        FechaEntrada = c.DateTime(),
                        FechaUltimaModificacion = c.DateTime(),
                        FechaRecepcionLaboratorio = c.DateTime(),
                        HoraRecepcionLaboratorio = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Pacientes");
            DropTable("dbo.ErrorLogs");
        }
    }
}
