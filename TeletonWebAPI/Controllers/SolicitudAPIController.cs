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
    // [Route("api/[controller]")]
    [ApiController]
    public class SolicitudAPIController : ControllerBase
    {
        private readonly ILogger<SolicitudAPIController> _logger;
        private readonly MiAppContext dbContext;

        public SolicitudAPIController(ILogger<SolicitudAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }


        [Route("api/SolicitudAPI/EditSolicitud")]
        [HttpGet]
        public async Task<IActionResult> EditSolicitud(int Id, int CantSol)
        {
            Solicitud s = await dbContext.Solicitudes.Where(x => x.Id == Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            s.CantSolicitadas = CantSol;
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/SolicitudAPI/EditAlcSol")]
        [HttpPost]
        public async Task<IActionResult> EditAlcSol(AlcanciaSolicitud alcSol)
        {
            AlcanciaSolicitud alcSolBuscada = await dbContext.AlcanciaSolicitudes.Where(x => x.IdAlcancia == alcSol.IdAlcancia && x.IdSolicitud == alcSol.IdSolicitud).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            alcSolBuscada.Impreso = alcSol.Impreso;
            alcSolBuscada.NumeroTicket = alcSol.NumeroTicket;
            await dbContext.SaveChangesAsync();
            return Ok();
        }


        [Route("api/SolicitudAPI/DeleteSolicitud")]
        [HttpGet]
        public async Task<IActionResult> DeleteSolicitud(int? Id)
        {
            Solicitud sol = await dbContext.Solicitudes.FindAsync(Id);
            if (sol == null)
            {
                return BadRequest(ModelState);
            }

            dbContext.Solicitudes.Remove(sol);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/SolicitudAPI/ObtenerSolEsp")]
        [HttpGet]
        public async Task<IActionResult> ObtenerSolEsp(int Id)
        {
            Solicitud sol = await dbContext.Solicitudes.Where(x => x.Id == Id).Include(x => x.Responsable).Include(x => x.Retira).Include(x => x.Colaborador).Include(x => x.Colaborador.TipoCol).Include(x => x.Alcancias).Include(x => x.Campania).Include(x => x.AlcanciasSolicitudes).SingleOrDefaultAsync();
            if (sol == null)
            {
                return BadRequest();
            }
            return Ok(sol);
        }

        [Route("api/SolicitudAPI/GetSolicitud")]
        [HttpGet]
        public async Task<IActionResult> GetSolicitud(int Id)
        {
            Solicitud sol = await dbContext.Solicitudes.Include("Responsable").Include("Retira").Include("Colaborador").Include("Campania").Include("Alcancias").Include("AlcanciasSolicitudes").Where(x => x.Id == Id).SingleOrDefaultAsync();
            if (sol == null)
            {
                return BadRequest();
            }
            return Ok(sol);
        }

        [Route("api/SolicitudAPI/GetPreSolicitud")]
        [HttpGet]
        public async Task<IActionResult> GetPreSolicitud(int Id)
        {
            Solicitud sol = await dbContext.Solicitudes.Include("Responsable").Include("Retira").Include("Colaborador").Include("Campania").Where(x => x.Id == Id).SingleOrDefaultAsync();
            if (sol == null)
            {
                return BadRequest();
            }
            return Ok(sol);
        }

        [Route("api/SolicitudAPI/GetAlcanciaSolicitud")]
        [HttpGet]
        public async Task<IActionResult> GetAlcanciaSolicitud(string IdAlc, int IdSol)
        {
            AlcanciaSolicitud alcSol = await dbContext.AlcanciaSolicitudes.Where(x => x.IdAlcancia == IdAlc && x.IdSolicitud == IdSol).SingleOrDefaultAsync();
            //if (alcSol == null)
            //{
            //    return BadRequest();
            //}
            return Ok(alcSol);
        }

        [Route("api/SolicitudAPI/GetListPreSolicitud")]
        [HttpGet]
        public async Task<IActionResult> GetListPreSolicitud()
        {
            List<Solicitud> sols = await dbContext.Solicitudes.Include("Colaborador").Include("Responsable").Include("Retira").Where(u => u.CantSolicitadas > 0 && u.CantEntregadas == 0).ToListAsync();
            if (sols == null)
            {
                return BadRequest();
            }
            return Ok(sols);
        }

        [Route("api/SolicitudAPI/GetListPosSolicitud")]
        [HttpGet]
        public async Task<IActionResult> GetListPosSolicitud()
        {
            List<Solicitud> sols = await dbContext.Solicitudes.Include("Colaborador").Include("Responsable").Include("Retira").Where(u => u.CantEntregadas > 0 && u.CantDevueltas > 0).ToListAsync();

            if (sols == null)
            {
                return BadRequest();
            }
            return Ok(sols);
        }

        [Route("api/SolicitudAPI/GetListSolicitud")]
        [HttpGet]
        public async Task<IActionResult> GetListSolicitud()
        {
            List<Solicitud> sols = await dbContext.Solicitudes.Include("Colaborador").Include("Responsable").Include("Retira").ToListAsync();


            if (sols == null)
            {
                return BadRequest();
            }
            return Ok(sols);
        }

        [Route("api/SolicitudAPI/GetListSolsComentario")]
        [HttpGet]
        public async Task<IActionResult> GetListSolsComentario()
        {
            List<Solicitud> sols = await dbContext.Solicitudes.Include("Colaborador").Include("Responsable").Where(u => u.CantEntregadas > 0 && u.CantDevueltas < u.CantEntregadas).ToListAsync();
            if (sols == null)
            {
                return BadRequest();
            }
            return Ok(sols);
        }

        [Route("api/SolicitudAPI/AltaAlcanciaSolicitud")]
        [HttpPost]
        public async Task<IActionResult> AltaAlcanciaSolicitud(AlcanciaSolicitud alcSol)
        {
            //Buscar la alcancía por id y cambiarle el estado a entregada y NO habilitada
            Alcancia a = await dbContext.Alcancias.Where(x=>x.IdAlcancia == alcSol.IdAlcancia).FirstOrDefaultAsync();
            Solicitud sol = await dbContext.Solicitudes.Where(x=>x.Id == alcSol.IdSolicitud).FirstOrDefaultAsync();

            a.Habilitada = "NO";
            a.Estado = "ENTREGADA";
            sol.CantEntregadas++;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.AlcanciaSolicitudes.AddAsync(alcSol);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/SolicitudAPI/AltaSolicitud")]
        [HttpPost]
        public async Task<IActionResult> AltaSolicitud(Solicitud solicitud)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Agrego cantidad al semáforo
            Semaforo semaforo = dbContext.Semaforos.Where(x => x.ColaboradorId == solicitud.ColaboradorId).FirstOrDefault();

            if (semaforo != null)
            {
                List<Solicitud> sol = dbContext.Solicitudes.Include("Campania").Where(x => x.ColaboradorId == solicitud.ColaboradorId).ToList();
                List<Campania> cam = new List<Campania>();
                foreach (var s in sol)
                {
                    if (sol != null && !cam.Contains(s.Campania))
                    {
                        cam.Add(s.Campania);
                    }
                }
                //Cuento la cantidad de campañas donde participo el colaborador
                int cantidad = cam.Count();
                semaforo.TotalCampanias = cantidad;


                //semaforo.TotalCampanias = TraerCampañas(solicitud.ColaboradorId);
                semaforo.CantAlcanciasSinDevolver = semaforo.CantAlcanciasSinDevolver + solicitud.CantEntregadas;
            }
            else
            {
                Semaforo sem = new Semaforo()
                {
                    ColaboradorId = solicitud.ColaboradorId,
                    CantAlcanciasDevueltas = 0,
                    CantAlcanciasSinDevolver = 1,
                    TotalCampanias = 1
                };
                await dbContext.Semaforos.AddAsync(sem);
            }

            await dbContext.Solicitudes.AddAsync(solicitud);
            await dbContext.SaveChangesAsync();
            return Ok(solicitud);
        }

        [Route("api/SolicitudAPI/GetListSolsExcel")]
        [HttpGet]
        public async Task<IActionResult> GetListSolsExcel()
        {
            List<AlcanciaSolicitud> aSols = await dbContext.AlcanciaSolicitudes.Include("Alcancia").Include("Solicitud").Where(u => u.Impreso == "NO" && u.Solicitud.CantDevueltas > 0 && u.Solicitud.CantEntregadas > 0).ToListAsync();
            if (aSols == null)
            {
                return BadRequest();
            }
            return Ok(aSols);
        }

        [Route("api/SolicitudAPI/CambiarEstadoImpreso")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoImpreso(List<AlcanciaSolicitud> lstAS)
        {
            foreach (AlcanciaSolicitud item in lstAS)
            {
                AlcanciaSolicitud a = await dbContext.AlcanciaSolicitudes.Where(x => x.IdAlcancia == item.IdAlcancia && x.IdSolicitud == item.IdSolicitud).SingleOrDefaultAsync();
                a.Impreso = "SI";
                await dbContext.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
