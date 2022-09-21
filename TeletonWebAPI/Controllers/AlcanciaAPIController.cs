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
    public class AlcanciaAPIController : ControllerBase
    {
        private readonly ILogger<AlcanciaAPIController> _logger;
        private readonly MiAppContext dbContext;

        public AlcanciaAPIController(ILogger<AlcanciaAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [Route("api/AlcanciaAPI/AltaAlcancia")]
        [HttpGet]
        public async Task<IActionResult> AltaAlcancia(string Codigo)
        {

            Alcancia alc = await dbContext.Alcancias.Where(x=>x.IdAlcancia == Codigo).SingleOrDefaultAsync();
            if (alc != null)
            {
                return Ok(alc); //Si entra por aca, manda un Ok pero con el objeto, el cual en el controller deserealiza y si no esta null es porque ya existe
            }

            Alcancia alcancia = new Alcancia
            {
                IdAlcancia = Codigo,
                FechaAlta = DateTime.Today,
                Estado = "DISPONIBLE", //ENTREGADA, RECIBIDA, CONTABILIZADA, DISPONIBLE
                Habilitada = "SI"
            };  
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Alcancias.AddAsync(alcancia);
            await dbContext.SaveChangesAsync();

            return Ok();
        }


        [Route("api/AlcanciaAPI/GetAlcancia")]
        [HttpGet]
        public async Task<IActionResult> GetAlcancia(string Id)
        {

            Alcancia a = await dbContext.Alcancias.FindAsync(Id);
            if (a != null)
            {
                return Ok(a);
            }
            return BadRequest(ModelState);
        }

        [Route("api/AlcanciaAPI/Devolucion")]
        [HttpGet]
        public async Task<IActionResult> Devolucion(string idAlcancia)
        {
            if (idAlcancia != null)
            {
                var alcSol = await dbContext.AlcanciaSolicitudes.Include("Solicitud").Where(u => u.IdAlcancia == idAlcancia && u.Solicitud.CantDevueltas<u.Solicitud.CantEntregadas).SingleOrDefaultAsync();
                var alc = await dbContext.Alcancias.Where(x => x.IdAlcancia == idAlcancia).SingleOrDefaultAsync();
                if (alcSol != null && alc!=null)
                {
                    //Semáforo: sumo las alcancías devueltas y resto esa cantidad a las sin devolver
                    //Semaforo semaforo = dbContext.Semaforos.Where(x => x.ColaboradorId == alcSol.Solicitud.ColaboradorId).SingleOrDefault();
                    //semaforo.CantAlcanciasSinDevolver = semaforo.CantAlcanciasSinDevolver - alcSol.Solicitud.CantDevueltas;
                    //semaforo.CantAlcanciasDevueltas = semaforo.CantAlcanciasDevueltas + alcSol.Solicitud.CantDevueltas;
                    alc.Estado = "RECIBIDA";
                    alcSol.Solicitud.CantDevueltas = alcSol.Solicitud.CantDevueltas + 1;
                    await dbContext.SaveChangesAsync();
                    return Ok(alcSol.Solicitud.Id);
                }
            }
            return BadRequest(ModelState);
        }



        [Route("api/AlcanciaAPI/Contabilizar")]
        [HttpPost]
        public async Task<IActionResult> Contabilizar(AlcanciaSolicitud alcSolRecibida)
        {
         
            var alcSol = await dbContext.AlcanciaSolicitudes.Where(u => u.IdAlcancia == alcSolRecibida.IdAlcancia && u.MontoPesos == 0 && u.MontoDolares == 0 && !u.EsVacia).SingleOrDefaultAsync();
            var alcancia = await dbContext.Alcancias.Where(u => u.IdAlcancia == alcSolRecibida.IdAlcancia).SingleOrDefaultAsync();
          
            if (alcSol != null && alcancia!=null)
            {
                alcancia.Estado = "CONTABILIZADA";
                alcSol.MontoPesos = alcSolRecibida.MontoPesos;
                alcSol.MontoDolares = alcSolRecibida.MontoDolares;
                alcSol.NumeroTicket = alcSolRecibida.NumeroTicket;
                alcSol.EsVacia = alcSolRecibida.EsVacia;

                await dbContext.SaveChangesAsync();
                return Ok(alcSol);
                   
            }
            return BadRequest(ModelState);
        }

        [Route("api/AlcanciaAPI/ListaAlcancia")]
        [HttpGet]
        public async Task<IActionResult> ListaAlcancia()
        {
            var lstAlc = await dbContext.Alcancias.ToListAsync();
            if (lstAlc != null)
            {
                return Ok(lstAlc);
            }
            return BadRequest(ModelState);
        }

        [Route("api/AlcanciaAPI/TraerListaAlcSol")]
        [HttpGet]
        public async Task<IActionResult> TraerListaAlcSol(int Id)
        {
            List<AlcanciaSolicitud> lstAlc = await dbContext.AlcanciaSolicitudes.Where(x => x.IdSolicitud == Id).Include("Alcancia").Include("Solicitud").ToListAsync();
            if (lstAlc != null)
            {
                return Ok(lstAlc);
            }
            return BadRequest(ModelState);
        }


        [Route("api/AlcanciaAPI/Editar")]
        [HttpPost]
        public async Task<IActionResult> Editar(Alcancia alc)
        {
            Alcancia a = await dbContext.Alcancias.Where(x=>x.IdAlcancia==alc.IdAlcancia).SingleOrDefaultAsync();//Llamada a la api
            if (a == null)
            {
                return BadRequest(ModelState);
            }
            a.Habilitada = alc.Habilitada;
            a.Estado = alc.Estado;
            await dbContext.SaveChangesAsync();
            return Ok();
        }


    }
}
