using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Euromed_MS.Data;
using Euromed_MS.Models;
using Euromed_MS.Recursos;
using Newtonsoft.Json.Linq;

namespace Euromed_MS.Controllers
{

    public class PacientesController : ApiController
    {
        private MSContext context = new MSContext();
        static string thisClassName = "PacientesController";
        DateTime FechaActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local);
        static string token = ConfigurationManager.AppSettings["JWT_TOKEN"].ToString();

        [HttpPost]
        public IHttpActionResult GestionarPaciente(Paciente newPaciente)
        {
            if (System.Web.HttpContext.Current.Request.Headers["Authorization"] != null
               && System.Web.HttpContext.Current.Request.Headers["Authorization"].Substring(6).Trim() == token)
            {
                if (!ModelState.IsValid)
                {
                    context.ErrorLogs.Add(new ErrorLog()
                    {
                        Error = "Error en Modelo de DATOS " + newPaciente.ToString(),
                        Clase = thisClassName,
                        Mensaje = JObject.FromObject(newPaciente).ToString()
                    });
                    context.SaveChangesAsync();
                    return BadRequest();
                }

                Paciente oldPaciente = null;

                newPaciente.FechaEntrada = FechaActual;
                newPaciente.FechaUltimaModificacion = FechaActual;

                try { oldPaciente = context.Pacientes.FirstOrDefault(p => p.AppId == newPaciente.AppId); }
                catch (Exception ex)
                {
                    context.ErrorLogs.Add(new ErrorLog()
                    {
                        Error = "Error Buscando el Paciente: " + newPaciente.AppId,
                        Clase = thisClassName,
                        Mensaje = ex.ToString()
                    });
                    context.SaveChangesAsync();
                }

                try
                {
                    if (newPaciente.FechaRecepcionMuestra == "" || newPaciente.FechaRecepcionMuestra == null || newPaciente.HoraRecepcionMuestra == "" || newPaciente.HoraRecepcionMuestra == null)
                    {
                        if ((newPaciente.CodigoMuestra != null || newPaciente.CodigoMuestra != ""))
                        {
                            newPaciente.FechaRecepcionMuestra = FechaActual.ToString("dd/MM/yyyy");
                            newPaciente.HoraRecepcionMuestra = FechaActual.ToString("HH:mm");
                        }
                    }

                    if (oldPaciente == null)
                    {
                        oldPaciente = context.Pacientes.Add(newPaciente);
                    }
                    else
                    {
                        newPaciente.ID = oldPaciente.ID;
                        context.Pacientes.AddOrUpdate(newPaciente);
                    }
                }
                catch (Exception ex)
                {
                    context.ErrorLogs.Add(new ErrorLog()
                    {
                        Error = "Error Guardando solicitud Paciente " + newPaciente.CodigoMuestra,
                        Clase = thisClassName,
                        Mensaje = ex.ToString()
                    });
                    context.SaveChangesAsync();
                }




                try { context.SaveChanges(); }
                catch (Exception ex)
                {
                    context.ErrorLogs.Add(new ErrorLog()
                    {
                        Error = "Error Guardando solicitud Paciente " + newPaciente.CodigoReserva,
                        Clase = thisClassName,
                        Mensaje = ex.ToString()
                    });
                    context.SaveChangesAsync();
                }

                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

    }
}