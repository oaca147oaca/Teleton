using Dominio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Dominio.EntidadesDeNegocio;

namespace TeletonWebAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class UsuarioAPIController : ControllerBase
    {
        private readonly ILogger<UsuarioAPIController> _logger;
        private readonly MiAppContext dbContext;

        public UsuarioAPIController(ILogger<UsuarioAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("api/UsuarioAPI/Usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var lista = await dbContext.Usuarios.OrderBy(c => c.Nombre).ToListAsync();
            return Ok(lista);
        }

        [HttpGet]
        [Route("api/UsuarioAPI/Login")]
        public async Task<IActionResult> Login(string Nombre, string Contrasenia) //
        {
            Usuario usr = new Usuario
            {
                Nombre = Nombre,
                Contrasenia = Contrasenia
            };
            if (usr == null)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuAutorizado = await dbContext.Usuarios.Include("Rol").Where(u => u.Nombre == usr.Nombre).SingleOrDefaultAsync();

            if (usuAutorizado == null)
            {
                return NotFound();

            }
            else
            {
                if (usuAutorizado.Contrasenia == usr.Contrasenia)
                {

                    return Ok(usuAutorizado);
                }
                else
                {
                    return NotFound();
                }
            }
        }
        [Route("api/UsuarioAPI/AltaVoluntario")]
        [HttpPost]
        public async Task<IActionResult> AltaVoluntario(Voluntario v)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Voluntarios.AddAsync(v);
            await dbContext.SaveChangesAsync();

            return Ok(v);
        }

        [Route("api/UsuarioAPI/AltaUsuario")]
        [HttpPost]
        public async Task<IActionResult> AltaUsuario(Usuario u)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Usuarios.AddAsync(u);
            await dbContext.SaveChangesAsync();

            return Ok(u);
        }


        [Route("api/UsuarioAPI/DeleteUsuario")]
        [HttpGet]
        public async Task<IActionResult> DeleteUsuario(int? Id)
        {
            Usuario usu = await dbContext.Usuarios.FindAsync(Id);
            if (usu == null)
            {
                return BadRequest(ModelState);
            }

            dbContext.Usuarios.Remove(usu);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/UsuarioAPI/ActualizarContra")]
        [HttpPost]
        public async Task<IActionResult> ActualizarContra(Usuario u)
        {
            Usuario usu = await dbContext.Usuarios.Where(x => x.Id == u.Id).FirstOrDefaultAsync();
            if (usu == null)
            {
                return BadRequest(ModelState);
            }
            usu.Contrasenia = u.Contrasenia;
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/UsuarioAPI/CerrarSesion")]
        [HttpGet]
        public async Task<IActionResult> CerrarSesion(int vId)
        {
            var voluntario = dbContext.Voluntarios.Where(x => x.Id == vId).FirstOrDefault();
            voluntario.Fin = DateTime.Now; 

            if (voluntario == null)
            {
                return BadRequest(ModelState);
            }
            await dbContext.SaveChangesAsync();
            return Ok();
        }



        [Route("api/UsuarioAPI/BuscarUsuario")]
        [HttpGet]
        public async Task<IActionResult> BuscarUsuario(int UserId)
        {
            Usuario usu = await dbContext.Usuarios.Where(x => x.Id == UserId).Include("Rol").FirstOrDefaultAsync();
            if (usu == null)
            {
                return BadRequest(ModelState);
            }

            return Ok(usu);
        }

        [Route("api/UsuarioAPI/BuscarUsuarioNombre")]
        [HttpGet]
        public async Task<IActionResult> BuscarUsuarioNombre(string Nombre)
        {
            Usuario usu = await dbContext.Usuarios.Where(x => x.Nombre == Nombre).Include("Rol").FirstOrDefaultAsync();
            if (usu == null)
            {
                return BadRequest(ModelState);
            }

            return Ok(usu);
        }

        [Route("api/UsuarioAPI/GetListRoles")]
        [HttpGet]
        public async Task<IActionResult> GetListRoles()
        {
            var listaRoles = await dbContext.Roles.OrderBy(c => c.Nombre).ToListAsync();

            if (listaRoles == null)
            {
                return BadRequest();
            }
            return Ok(listaRoles);
        }

        [Route("api/UsuarioAPI/GetListUsus")]
        [HttpGet]
        public async Task<IActionResult> GetListUsus()
        {
            var listaUsus = await dbContext.Usuarios.Include("Rol").ToListAsync();//Hacer llamado por api

            if (listaUsus == null)
            {
                return BadRequest();
            }
            return Ok(listaUsus);
        }
        
        [Route("api/UsuarioAPI/GetListVols")]
        [HttpGet]
        public async Task<IActionResult> GetListVols()
        {
            var listaVols = await dbContext.Voluntarios.ToListAsync();//Hacer llamado por api

            if (listaVols == null)
            {
                return BadRequest();
            }
            return Ok(listaVols);
        }

    }
}
