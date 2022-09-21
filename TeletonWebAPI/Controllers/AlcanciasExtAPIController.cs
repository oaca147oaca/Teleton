using Dominio;
using Dominio.EntidadesDeNegocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeletonMVC.Models;

namespace TeletonWebAPI.Controllers
{
    [ApiController]
    public class AlcanciasExtAPIController : ControllerBase
    {
        private readonly ILogger<AlcanciaAPIController> _logger;
        private readonly MiAppContext dbContext;

        public AlcanciasExtAPIController(ILogger<AlcanciaAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [Route("api/AlcanciasExtAPI/GetListAlc")]
        [HttpGet]
        public async Task<IActionResult> GetListAlc()
        {

            List<AlcanciaExterna> lstALcs = await dbContext.AlcanciasExternas.Include(a => a.Campania).ToListAsync();

            if (lstALcs == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(lstALcs);
        }


        [Route("api/AlcanciasExtAPI/AltaAlcExt")]
        [HttpPost]
        public async Task<IActionResult> AltaAlcExt(AlcExternaModel a)
        {

            Campania camp = await dbContext.Campanias.Where(x => x.FechaInicio.Year == DateTime.Today.Year).FirstOrDefaultAsync();

            

            for (int i = 0; i < a.Cantidad; i++)
            {
                AlcanciaExterna alcEx = new AlcanciaExterna();
                alcEx.Nombre = a.Nombre;
                alcEx.Telefono = a.Telefono;
                alcEx.MontoDolares = 0;
                alcEx.MontoPesos = 0;
                alcEx.NumeroTicket = 0;
                alcEx.Impreso = "N";
                alcEx.CampaniaId = camp.IdCampania;
                alcEx.FechaDevolucion = DateTime.Today;

                await dbContext.AlcanciasExternas.AddAsync(alcEx);
                await dbContext.SaveChangesAsync();
            }
            return Ok();
        }

        [Route("api/AlcanciasExtAPI/GetAlcExt")]
        [HttpGet]
        public async Task<IActionResult> GetAlcExt(int Id)
        {
            var ae = await dbContext.AlcanciasExternas.Include(a => a.Campania).FirstOrDefaultAsync(m => m.IdAlcExt == Id);

            if (ae != null)
            {
                return Ok(ae);
            }
            return BadRequest(ModelState);
        }

        [Route("api/AlcanciasExtAPI/ObtenerAlcancias")]
        [HttpGet]
        public async Task<IActionResult> ObtenerAlcancias(string Nombre)
        {
            List<AlcanciaExterna> ae = await dbContext.AlcanciasExternas.Include(a => a.Campania).Where(m => m.Nombre == Nombre && m.FechaDevolucion.Date == DateTime.Today.Date).ToListAsync();

            if (ae != null)
            {
                return Ok(ae);
            }
            return BadRequest(ModelState);
        }
      

        [Route("api/AlcanciasExtAPI/EditarAlc")]
        [HttpPost]
        public async Task<IActionResult> EditarAlc(AlcanciaExterna a)
        {
            var ae = await dbContext.AlcanciasExternas.Include(a => a.Campania).FirstOrDefaultAsync(m => m.IdAlcExt == a.IdAlcExt);
            if (ae == null)
            {
                return BadRequest(ModelState);
            }
            ae.MontoDolares = a.MontoDolares;
            ae.MontoPesos = a.MontoPesos;
            ae.NumeroTicket = a.NumeroTicket;
            ae.Telefono = a.Telefono;
            ae.Impreso = a.Impreso;
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/AlcanciasExtAPI/EliminarAlc")]
        [HttpGet]
        public async Task<IActionResult> EliminarAlc(int Id)
        {
            var ae = await dbContext.AlcanciasExternas.Where(x=>x.IdAlcExt ==Id).SingleOrDefaultAsync();
            if (ae == null)
            {
                return BadRequest(ModelState);
            }
            dbContext.AlcanciasExternas.Remove(ae); 
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/AlcanciasExtAPI/GetListAlcExcel")]
        [HttpGet]
        public async Task<IActionResult> GetListAlcExcel()
        {
            List<AlcanciaExterna> lstALcs = await dbContext.AlcanciasExternas.Include(a => a.Campania).Where(u => u.Impreso == "NO").ToListAsync();

            if (lstALcs == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(lstALcs);
        }

        [Route("api/AlcanciasExtAPI/CambiarEstadoImpreso")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstadoImpreso(List<AlcanciaExterna> lstAE)
        {
            foreach (AlcanciaExterna item in lstAE)
            {
                AlcanciaExterna a = await dbContext.AlcanciasExternas.FindAsync(item.IdAlcExt);
                a.Impreso = "SI";
                await dbContext.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
