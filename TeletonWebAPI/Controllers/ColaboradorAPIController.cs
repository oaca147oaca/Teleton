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
    public class ColaboradorAPIController : ControllerBase
    {
        private readonly ILogger<ColaboradorAPIController> _logger;
        private readonly MiAppContext dbContext;

        public ColaboradorAPIController(ILogger<ColaboradorAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [Route("api/ColaboradorAPI/AltaColaborador")]
        [HttpPost]
        public async Task<IActionResult> AltaColaborador(Colaborador c)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Colaboradores.AddAsync(c);
            await dbContext.SaveChangesAsync();

            return Ok(c);
        }

        [Route("api/ColaboradorAPI/EditColaborador")]
        [HttpGet]
        public async Task<IActionResult> EditColaborador(int Id)
        {
            var colBuscado = await dbContext.Colaboradores.Where(u => u.Id == Id).SingleOrDefaultAsync();//Busco al que me pidieron modificar

            //var colValidar = await dbContext.Colaboradores.Where(u => u.Nombre == col.Nombre).SingleOrDefaultAsync(); //Esta llamada se hace para verificar si ya existe un colaborador con ese nombre, en caso de ser asi no se deberia dejar hacer la modificacion debido a que ya existe en la bd

            if (colBuscado == null)//Esto creo que es innecesario
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //colBuscado.Nombre = col.Nombre;
            //colBuscado.RazonSocial = col.RazonSocial;
            //colBuscado.Direccion = col.Direccion;
            //colBuscado.Departamento = col.Departamento;
            //colBuscado.Localidad = col.Localidad;
            colBuscado.TipoColId = Id;
            //dbContext.Entry(col).State = EntityState.Modified;
            // colBuscado = col;
            await dbContext.SaveChangesAsync();

            return Ok(colBuscado);
        }

        [Route("api/ColaboradorAPI/EliminarColaborador")]
        [HttpDelete]
        public async Task<IActionResult> EliminarColaborador(int Id)
        {
            var colab = dbContext.Colaboradores.Find(Id);
            if (colab == null)
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dbContext.Colaboradores.Remove(colab);
            await dbContext.SaveChangesAsync();
            return Ok(colab);
        }

        [Route("api/ColaboradorAPI/GetColaborador")]
        [HttpGet]
        public async Task<IActionResult> GetColaborador(int Id)
        {
            var col = await dbContext.Colaboradores.Include("TipoCol").Where(u => u.Id == Id).SingleOrDefaultAsync();
            if (col == null)
            {
                return BadRequest();
            }
            return Ok(col);
        }

        [Route("api/ColaboradorAPI/GetListColaborador")]
        [HttpGet]
        public async Task<IActionResult> GetListColaborador()
        {
            var lista = await dbContext.Colaboradores.Include("TipoCol").OrderBy(c => c.Nombre).ToListAsync();
            //var lista = await dbContext.Usuarios.OrderBy(c => c.Nombre).ToListAsync();

            if (lista == null)
            {
                return BadRequest();
            }
            return Ok(lista);
        }
    }
}
