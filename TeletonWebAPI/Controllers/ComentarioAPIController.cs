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
    public class ComentarioAPIController : ControllerBase
    {
        private readonly ILogger<ComentarioAPIController> _logger;
        private readonly MiAppContext dbContext;

        public ComentarioAPIController(ILogger<ComentarioAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [Route("api/ComentarioAPI/AltaComentario")]
        [HttpGet]
        public async Task<IActionResult> AltaComentario(int? idSol, string comentario)
        {

            Comentario c = new Comentario
            {
                Fecha = DateTime.Today,
                SolicitudId = (int)idSol,
                TextoComentario = comentario

            };
            await dbContext.Comentarios.AddAsync(c);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/ComentarioAPI/GetComentarios")]
        [HttpGet]
        public async Task<IActionResult> GetComentarios(int Id)
        {
            List<Comentario> lstCom = await dbContext.Comentarios.Where(x => x.SolicitudId == Id).ToListAsync();
            if (lstCom == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(lstCom);
        }

        [Route("api/ComentarioAPI/GetComentario")]
        [HttpGet]
        public async Task<IActionResult> GetComentario(int Id)
        {
            Comentario c = await dbContext.Comentarios.Where(x => x.IdComentario == Id).SingleOrDefaultAsync();
            if (c == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(c);
        }

        [Route("api/ComentarioAPI/EliminarComentario")]
        [HttpGet]
        public async Task<IActionResult> EliminarComentario(int? idCom)
        {
            Comentario c = await dbContext.Comentarios.FindAsync((int)idCom);
            if (c == null)
            {
                return BadRequest(ModelState);
            }
            dbContext.Comentarios.Remove(c);
            await dbContext.SaveChangesAsync();
            return Ok();
        }


        [Route("api/ComentarioAPI/GetListSemaforos")]
        [HttpGet]
        public async Task<IActionResult> GetListSemaforos()
        {
            List<Semaforo> lstSem = await dbContext.Semaforos.Include("Colaborador").ToListAsync();
            if (lstSem == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(lstSem);
        }

        [Route("api/ComentarioAPI/BuscarSemaforo")]
        [HttpGet]
        public async Task<IActionResult> BuscarSemaforo(int Id)
        {
            Semaforo s = await dbContext.Semaforos.Include("Colaborador").Where(x=>x.Id == Id).SingleOrDefaultAsync();
            if (s == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(s);
        }
        [Route("api/ComentarioAPI/EditSemaforo")]
        [HttpPut]
        public async Task<IActionResult> EditSemaforo(Semaforo s)
        {
            Semaforo semBuscado = await dbContext.Semaforos.Include("Colaborador").Where(x => x.Id == s.Id).SingleOrDefaultAsync();
            if (semBuscado == null)
            {
                return BadRequest(ModelState);
            }
            semBuscado.CantAlcanciasSinDevolver = s.CantAlcanciasSinDevolver;

            await dbContext.SaveChangesAsync();
            return Ok(s);
        }


        

    }
}
