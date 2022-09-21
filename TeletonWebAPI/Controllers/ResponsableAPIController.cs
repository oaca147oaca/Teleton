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
    public class ResponsableAPIController : ControllerBase
    {
        private readonly ILogger<ResponsableAPIController> _logger;
        private readonly MiAppContext dbContext;

        public ResponsableAPIController(ILogger<ResponsableAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }


        [Route("api/ResponsableAPI/AltaResponsable")]
        [HttpPost]
        public async Task<IActionResult> AltaResponsable(Responsable r)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Responsables.AddAsync(r);
            await dbContext.SaveChangesAsync();

            return Ok(r);
        }

        [Route("api/ResponsableAPI/TodosResp")]
        [HttpGet]
        public async Task<IActionResult> TraerResponsables()
        {
            List<Responsable> r = await dbContext.Responsables.ToListAsync();
            if (r == null)
            {
                return BadRequest();
            }
            return Ok(r);
        }

        [Route("api/ResponsableAPI/BuscarResp")]
        [HttpGet]
        public async Task<IActionResult> BuscarResp(int Id)
        {
            Responsable r = await dbContext.Responsables.Where(x=>x.Id ==Id).SingleOrDefaultAsync();
            if (r == null)
            {
                return BadRequest();
            }
            return Ok(r);
        }


        [Route("api/ResponsableAPI/BuscarResponsableCedula")]
        public async Task<IActionResult> BuscarResponsableCedula(string Cedula)
        {
            Responsable r = dbContext.Responsables.Where(x => x.Cedula == Cedula).SingleOrDefault();
            if (r == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(r);
        }


        [Route("api/ResponsableAPI/EditResponsable")]
        [HttpPut]
        public async Task<IActionResult> EditColaborador(Responsable resp)
        {
            var respBuscado = await dbContext.Responsables.Where(u => u.Id == resp.Id).SingleOrDefaultAsync();//Busco al que me pidieron modificar


            if (resp == null)//Esto creo que es innecesario
            {
                return BadRequest(ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            respBuscado.Nombre = resp.Nombre;
            respBuscado.Cedula = resp.Cedula;
            respBuscado.Correo = resp.Correo;
            respBuscado.Telefono = resp.Telefono;
            respBuscado.Retira = resp.Retira;
            await dbContext.SaveChangesAsync();

            return Ok(respBuscado);
        }

        [Route("api/ResponsableAPI/EliminarResponsable")]
        [HttpGet]
        public async Task<IActionResult> DeleteResponsable(int? Id)
        {
            Responsable r = await dbContext.Responsables.FindAsync(Id);
            if (r == null)
            {
                return BadRequest(ModelState);
            }

            dbContext.Responsables.Remove(r);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/ResponsableAPI/GetListResponsablesDeCol")]
        [HttpGet]
        public async Task<IActionResult> GetListResponsablesDeCol(int Id)
        {
            List<Solicitud> sols = await dbContext.Solicitudes.Where(x => x.ColaboradorId == Id).Include("Responsable").ToListAsync();
            if (sols == null)
            {
                return BadRequest();
            }
            List<Responsable> responsables = new List<Responsable>();
            foreach (Solicitud sol in sols)
            {
                if (!responsables.Contains(sol.Responsable))
                {
                    responsables.Add(sol.Responsable);
                }
            }

            return Ok(responsables);
        }
    }
}
