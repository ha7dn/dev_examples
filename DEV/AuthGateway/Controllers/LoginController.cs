using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;
using AuthGateway.Data;
using AuthGateway.Models;
using AuthGateway.Recursos;

namespace AuthGateway.Controllers
{
    public class LoginController : ApiController
    {
        private AuthGatewayContext context = new AuthGatewayContext();

        [HttpGet]
        public IHttpActionResult Echo()
        {
            return Ok(true);
        }

        [HttpPost]
        public IHttpActionResult New(Login login)
        {
            if (login == null || !ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                login.FechaEntrada = DateTime.Now;
                login.FechaUltimaModificacion = DateTime.Now;
                context.Logins.Add(login);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                //Auxiliares.EnviarMail(Auxiliares.emailDeError, "API Error al guardar credenciales de autenticacion", ex.ToString(), false);
                return BadRequest();
            }

            return Ok("Usuario registrado");
        }


        [HttpGet]
        public IHttpActionResult EchoUser(string username)
        {
            if (context.Logins.FirstOrDefault(l => l.User == username) != null)
            {
                var identity = Thread.CurrentPrincipal.Identity;
                return Ok($" IPrincipal-user: {username} - IsAuthenticated: {identity.IsAuthenticated}");

            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPost]
        public IHttpActionResult Authenticate(string password)
        {
            if (password == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var thisLogin = context.Logins.FirstOrDefault(l => l.Password == password);
            bool isCredentialValid = (thisLogin != null);
            if (isCredentialValid)
            {

                thisLogin.Dominio = Request.RequestUri.Host;
                var token = TokenGenerator.GenerateTokenJwt(thisLogin.User);
                var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

                context.Tokens.Add(new Token()
                {
                    LoginID = thisLogin.ID,
                    Self = tokenStr,
                    ExpiryTime = ((DateTimeOffset)token.ValidTo).ToUnixTimeSeconds(),
                    FechaEntrada = DateTime.Now,
                    FechaUltimaModificacion = DateTime.Now
                });
                context.SaveChanges();


                return Ok(tokenStr);
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}