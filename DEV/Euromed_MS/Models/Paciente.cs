using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Euromed_MS.Models
{
    public class Paciente
    {

        public Paciente()
        {
            ID = Guid.NewGuid();
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class SqlDefaultValueAttribute : Attribute
        {
            public string DefaultValue { get; set; }
        }


        [Key]
        [DefaultValue("newid()")]
        public Guid ID { get; set; }
        public string NombreAplicacion { get; set; }
        public string NombreFormulario { get; set; }
        public long AppId { get; set; }
        public string ZohoId { get; set; }
        public string FechaCreacion { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string Genero { get; set; }
        public string Email { get; set; }
        public string TelefonoPaciente { get; set; }
        public string DocId { get; set; }
        public string FechaNacimiento { get; set; }
        public string Localidad { get; set; }
        public string Firma { get; set; }
        public string TipoPrueba { get; set; }
        public string VelocidadPrueba { get; set; }
        public string CodigoMuestra { get; set; }
        public string Resultado { get; set; }
        public string FechaRecepcionMuestra { get; set; }
        public string HoraRecepcionMuestra { get; set; }
        public string CargaViral { get; set; }
        public string IGG { get; set; }
        public string IGM { get; set; }
        public string EstadoEnvio { get; set; }
        public string EstadoDocumento { get; set; }
        public string EstadoNotificacion { get; set; }
        public string NombreComprador { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }
        public string TelefonoComprador { get; set; }
        public string EmailComprador { get; set; }
        public string CodigoReserva { get; set; }
        public string CodigoTicket { get; set; }
        public string Precio { get; set; }
        public string FechaEmisionCertificado { get; set; }
        public string PresentaSintomas { get; set; }
        public string VolarViajar { get; set; }
        public string Contacto_Positivo { get; set; }
        public DateTime? FechaReserva { get; set; }
        public DateTime? HoraReserva { get; set; }
        public string EstadoPago { get; set; }
        public string IdDocumento { get; set; }
        public string Notas { get; set; }
        public string IDAfiliado { get; set; }
        public string Ubicacion { get; set; }
        public DateTime? FechaEntrada { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public DateTime? FechaRecepcionLaboratorio { get; set; }
        public DateTime? HoraRecepcionLaboratorio { get; set; }
    }
}