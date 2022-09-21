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
    public class CampaniaAPIController : ControllerBase
    {
        private readonly ILogger<CampaniaAPIController> _logger;
        private readonly MiAppContext dbContext;

        public CampaniaAPIController(ILogger<CampaniaAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        [Route("api/CampaniaAPI/AltaCampania")]
        [HttpGet]
        public async Task<IActionResult> AltaCampania(string Nombre, string FechaInicio, string FechaFinMVD, string FechaFinFB)
        {
            Campania existeCampaania = await dbContext.Campanias.Where(x => x.FechaInicio.Year == DateTime.Today.Year).SingleOrDefaultAsync();
            if (existeCampaania != null)
            {
                return Ok(existeCampaania);
            }
            DateTime inicio = DateTime.Parse(FechaInicio);
            DateTime finMVD = DateTime.Parse(FechaFinMVD);
            DateTime finFB = DateTime.Parse(FechaFinFB);

            Campania campania = new Campania
            {
                Nombre = Nombre,
                FechaInicio = inicio,
                FechaFinMVD = finMVD,
                FechaFinFB = finFB
            };
            await dbContext.Campanias.AddAsync(campania);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/CampaniaAPI/EditCampania")]
        [HttpPut]
        public async Task<IActionResult> EditCampania(Campania cam)
        {

            Campania c = await dbContext.Campanias.Where(x => x.IdCampania == cam.IdCampania).SingleOrDefaultAsync();
            if (c == null)
            {
                return BadRequest(ModelState);
            }
            c.Nombre = cam.Nombre;
            c.FechaInicio = cam.FechaInicio;
            c.FechaFinMVD = cam.FechaFinMVD;
            c.FechaFinFB = cam.FechaFinFB;
            await dbContext.SaveChangesAsync();

            return Ok();
        }



        [Route("api/CampaniaAPI/GetCampaniaActual")]
        [HttpGet]
        public async Task<IActionResult> GetCampaniaActual()
        {
            //Campania c = await dbContext.Campanias.Where(x => x.FechaInicio.Day <= DateTime.Today.Day && x.FechaFinFB >= DateTime.Today).SingleOrDefaultAsync(); //Obtengo la campaña actual buscando por la fecha
                Campania c = await dbContext.Campanias.Where(x => x.FechaInicio.Year == DateTime.Today.Year).FirstOrDefaultAsync(); //Obtengo la campaña actual buscando por la fecha
            if (c == null)
            {
              return BadRequest(ModelState);
            }

            return Ok(c);
        }

        [Route("api/CampaniaAPI/TodasCampanias")]
        [HttpGet]
        public async Task<IActionResult> TodasCampanias()
        {
            List<Campania> lstCamp = await dbContext.Campanias.ToListAsync(); //Obtengo la campaña actual buscando por la fecha
            if (lstCamp == null)
            {
                return BadRequest(ModelState);
            }

            return Ok(lstCamp);
        }

        [Route("api/CampaniaAPI/AlcanciaCampanias")]
        [HttpGet]
        public async Task<IActionResult> AlcanciaCampanias(string Id)
        {
            List<AlcanciaSolicitud> alcSols = await dbContext.AlcanciaSolicitudes.Include("Solicitud").Where(x => x.IdAlcancia == Id).ToListAsync();
            List<Campania> campanias = new List<Campania>();
            Solicitud sol = new Solicitud();
            foreach (var als in alcSols)
            {
                sol = dbContext.Solicitudes.Include("Campania").Where(x => x.Id == als.IdSolicitud).SingleOrDefault();


                if (sol != null)
                {
                    campanias.Add(sol.Campania);
                }
            }

            return Ok(campanias);
        }

        [Route("api/CampaniaAPI/GetCampania")]
        [HttpGet]
        public async Task<IActionResult> GetCampania(int Id)
        {
            Campania camp = await dbContext.Campanias.Where(x => x.IdCampania == Id).SingleOrDefaultAsync();

            if (camp == null)
            {
                return BadRequest();
            }
            return Ok(camp);
        }
    }
}
