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

namespace TeletonWebAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class TipoColaboradorAPI : ControllerBase
    {
        private readonly ILogger<TipoColaboradorAPI> _logger;
        private readonly MiAppContext dbContext;

        public TipoColaboradorAPI(ILogger<TipoColaboradorAPI> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [Route("api/TipoColaboradorAPI/AltaTipoColaborador")]
        [HttpGet]
        public async Task<IActionResult> AltaTipoColaborador(string Nombre)
        {
            var tipoColBuscado = dbContext.TipoColaboradores.Where(x => x.Nombre == Nombre).SingleOrDefault();
            if (tipoColBuscado!=null)
            {
                return BadRequest();
            }
            TipoColaborador tipoColab = new TipoColaborador
            {
                Nombre = Nombre
            };

            if (tipoColab == null)
            {
                return BadRequest(ModelState);
            }
           

            await dbContext.TipoColaboradores.AddAsync(tipoColab);
            await dbContext.SaveChangesAsync();
            return Ok(tipoColab);
        }

        [Route("api/TipoColaboradorAPI/UpdateTipoColaborador")]
        [HttpGet]
        public async Task<IActionResult> UpdateTipoColaborador(int? Id, string Nombre)
        {
            //await dbContext.Usuarios.Where(u => u.Nombre == usr.Nombre).SingleOrDefaultAsync();   
            var tipoValidar = await dbContext.TipoColaboradores.Where(u => u.Nombre == Nombre).SingleOrDefaultAsync(); //Esta llamada se hace para verificar si ya existe un tipo colaborador con ese nombre, en caso de ser asi no se deberia dejar hacer la modificacion debido a que ya existe en la bd
            var tipoColab = await dbContext.TipoColaboradores.Where(u => u.Id == Id).SingleOrDefaultAsync();//Busco al que me pidieron modificar

            if (tipoValidar != null || tipoColab == null)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            tipoColab.Nombre = Nombre;
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/TipoColaboradorAPI/EliminarTipoColaborador")]
        [HttpDelete]
        public async Task<IActionResult> EliminarTipoColaborador(int? Id)
        {
            var tipoColab = await dbContext.TipoColaboradores.Where(u => u.Id == Id).SingleOrDefaultAsync();
            if (tipoColab == null)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dbContext.TipoColaboradores.Remove(tipoColab);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/TipoColaboradorAPI/GetListTipoColaborador")]
        [HttpGet]
        public async Task<IActionResult> GetListTipoColaborador()
        {
            var listaTc = await dbContext.TipoColaboradores.OrderBy(c => c.Nombre).ToListAsync();

            if (listaTc == null)
            {
                return BadRequest();
            }
            return Ok(listaTc);
        }


        [Route("api/TipoColaboradorAPI/GetTipoColaborador")]
        [HttpGet]
        public async Task<IActionResult> GetTipoColaborador(int Id)
        {
            var tipoCol = await dbContext.TipoColaboradores.Where(u => u.Id == Id).SingleOrDefaultAsync();
            if (tipoCol == null)
            {
                return BadRequest();
            }
            return Ok(tipoCol);
        }














    }
}
