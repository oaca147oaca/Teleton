using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dominio;
using Dominio.EntidadesDeNegocio;
using Dominio.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositorio;

namespace Repositorios
{
    public class RepositorioSolicitud : IRepositorioSolicitud
    {

        private readonly IConfiguration _configuration;


        public RepositorioSolicitud(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public List<Solicitud> GetSolicitudes()
        {
            //List<Solicitud> sols = dbContext.Solicitudes.Include(x => x.Responsable).Include(x => x.Retira).Include(x => x.Colaborador).Include(x => x.Alcancias).ToList(); 
           // return sols;
            return null;
        }

    }
}
