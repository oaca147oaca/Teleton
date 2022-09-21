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
    public class RetiraAPIController : ControllerBase
    {

        private readonly ILogger<RetiraAPIController> _logger;
        private readonly MiAppContext dbContext;

        public RetiraAPIController(ILogger<RetiraAPIController> logger, MiAppContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }


        [Route("api/RetiraAPI/AltaRetira")]
        [HttpPost]
        public async Task<IActionResult> AltaRetira(Retira r) //[FromForm] Colaborador colaborador
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Retiran.AddAsync(r);
            await dbContext.SaveChangesAsync();

            return Ok(r);
        }

        [Route("api/RetiraAPI/EditRetira")]
        [HttpPost]
        public async Task<IActionResult> EditRetira(Retira r)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await dbContext.Retiran.AddAsync(r);
            await dbContext.SaveChangesAsync();

            return Ok();
        } 
        
        [Route("api/RetiraAPI/BuscarRetira")]
        [HttpGet]
        public async Task<IActionResult> BuscarRetira(int ? Id)
        {
            Retira r = await dbContext.Retiran.FindAsync(Id);
            if (r == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(r);
        }

        [Route("api/RetiraAPI/BuscarRetiraCedula")]
        public async Task<IActionResult> BuscarRetiraCedula(string Cedula)
        {
            Retira r = dbContext.Retiran.Where(x => x.Cedula == Cedula).SingleOrDefault();
            if (r == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(r);
        }

        [Route("api/RetiraAPI/DeleteRetira")]
        [HttpGet]
        public async Task<IActionResult> DeleteRetira(int? Id)
        {
            Retira r = await dbContext.Retiran.FindAsync(Id);
            if (r == null)
            {
                return BadRequest(ModelState);
            }

            dbContext.Retiran.Remove(r);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("api/RetiraAPI/GetListRetira")]
        [HttpGet]
        public async Task<IActionResult> GetListRetira()
        {
            List<Retira> lstRet= await dbContext.Retiran.ToListAsync();
            if (lstRet == null)
            {
                return BadRequest(ModelState);
            }
            return Ok(lstRet);
        }

        [Route("api/RetiraAPI/GetListRetiranDeCol")]
        [HttpGet]
        public async Task<IActionResult> GetListRetiranDeCol(int Id)
        {
            List<Solicitud> sols = await dbContext.Solicitudes.Where(x => x.ColaboradorId == Id).Include("Retira").ToListAsync();
            if (sols == null)
            {
                return BadRequest();
            }
            List<Retira> retiran = new List<Retira>();
            foreach (Solicitud sol in sols)
            {
                if (!retiran.Contains(sol.Retira))
                {
                    retiran.Add(sol.Retira);
                }
            }

            return Ok(retiran);
        }

    }
}
