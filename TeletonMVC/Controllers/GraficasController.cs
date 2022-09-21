using Dominio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TeletonMVC.Models;

namespace TeletonMVC.Controllers
{
    public class GraficasController : Controller
    {
        private readonly ILogger<GraficasController> _logger;
        private readonly IConfiguration _configuration;
        private string key="";
        public HttpClient ApiClient { get; }

        public GraficasController(ILogger<GraficasController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            this.key = _configuration.GetSection("ApiKey")?.Value;
            this.ApiClient = new HttpClient();

            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);

            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }
        public IActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                List<Solicitud> sols = TraerSols();

                int cantMVD = 0;
                int cantAlcMVD = 0;
                int cantFB = 0;
                int cantAlcFB = 0;

                for(int i = 0; i < sols.Count; i++)
                {
                    if (sols[i].LugarEntrega == "MVD")
                    {
                        cantMVD ++;
                        cantAlcMVD += sols[i].CantEntregadas;
                    }
                    else
                    {
                        cantFB++;
                        cantAlcFB += sols[i].CantEntregadas;
                    }
                }
                ViewBag.CantSolMVD = cantMVD;
                ViewBag.CantSolFB = cantFB;
                ViewBag.CantAlcMVD = cantAlcMVD;
                ViewBag.CantAlcFB = cantAlcFB;


                string parameters = "CampaniaAPI/GetCampaniaActual";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);
                Campania camp = Campania.TraerCampaniaActual(key,respuesta);

               
                if (camp == null && (rol == "Voluntariado" || rol == "Administrador"))
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Debe registrarse una campaña para poder ingresar cualquier tipo de solicitud.";
                    return RedirectToAction("AltaCampania", "Campania");
                }
                else if (camp == null && (rol == "VoluntarioMvd" || rol == "VoluntarioFB" || rol == "Contabilidad"))
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Para tener acceso al sistema debe existir una campaña actualmente, llame a un supervisor para continuar.";
                    return RedirectToAction("MostrarTodasSols", "Solicitud");
                }

                ViewBag.CampAct = camp;
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        public JsonResult DataPastel()
        {
            SeriePastel serie = new SeriePastel();

            string parameters = "AlcanciaAPI/ListaAlcancia";
            Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);


            return Json(serie.GetDataDummy(key,respuesta));
        }

        public JsonResult DataPastel2()
        {

            string key = _configuration.GetSection("ApiKey")?.Value;

            string parameters = "AlcanciaAPI/ListaAlcancia";
            Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);

            SeriePastel2 serie = new SeriePastel2();
            return Json(serie.GetDataDummy(key,respuesta));
        }

        public List<Solicitud> TraerSols()
        {
            List<Solicitud> sols = new List<Solicitud>();
            try
            {
               

                string parameters = "SolicitudAPI/GetListSolicitud";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    sols = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                    return sols; 
                }
            }
            catch(Exception ex)
            {

            }
            return sols;
        }

    }
}
