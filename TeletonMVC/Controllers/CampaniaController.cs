using Dominio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dominio.EntidadesDeNegocio;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace TeletonMVC.Controllers
{
    public class CampaniaController : Controller
    {
        private readonly ILogger<CampaniaController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;
        public CampaniaController(ILogger<CampaniaController> logger, MiAppContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }


        // GET: CampaniaController
        public ActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                return RedirectToAction("ListadoCampanias");
            }
            return RedirectToAction("Login", "Usuario");
        }


        // GET: CampaniaController/Create
        public ActionResult AltaCampania()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: CampaniaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AltaCampania(string Nombre, DateTime? FechaInicio, DateTime? FechaFinMVD, DateTime? FechaFinFB)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                try
                {
                    //Si los datos son correctos, voy a dar de alta el cliente
                    if (Campania.ValidarDatos(Nombre, FechaInicio, FechaFinMVD, FechaFinFB))
                    {
                        string parameters = "CampaniaAPI/AltaCampania";

                        string fechaIni = FechaInicio.ToString();
                        string finMVD = FechaFinMVD.ToString();
                        string finFB = FechaFinFB.ToString();

                        Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Nombre=" + Nombre + "&FechaInicio=" + fechaIni + "&FechaFinMVD=" + finMVD + "&FechaFinFB=" + finFB);

                        respuesta.Wait();

                        if (respuesta.Result.IsSuccessStatusCode)
                        {
                            Campania validar = new Campania();
                            Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            string json = taskContent.Result;
                            validar = JsonConvert.DeserializeObject<Campania>(json);
                            if (validar != null)
                            {
                                TempData["Status"] = false;
                                TempData["Mensaje"] = "No se pudo agregar una campaña debido a que existe una actualmente.";
                                return View();
                            }
                            TempData["Status"] = true;
                            TempData["Mensaje"] = "Se agrego correctamente la campaña";
                            return RedirectToAction("ListadoCampanias");

                        }
                        return View();
                    }
                    else
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Verifique que los datos ingresados sean correctos.";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = ex.Message.ToString();
                    return View();
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: SolicitudController/Edit/5
        public ActionResult Edit(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                Campania camp = TraerCampania(id); 
                return View(camp);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: SolicitudController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Campania camp)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                if(camp.FechaFinMVD<camp.FechaInicio || camp.FechaFinFB < camp.FechaInicio)
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Las fechas finales no pueden ser antes del inicio.";
                    return View(camp);
                }


                string parameters = "CampaniaAPI/EditCampania";

                string json = JsonConvert.SerializeObject(camp, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = ApiClient.PutAsync(parameters, content);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    TempData["Status"] = true;
                    TempData["Mensaje"] = "Se ha editado correctamente";
                    return RedirectToAction("ListadoCampanias");
                }
                TempData["Status"] = false;
                TempData["Mensaje"] = "Algo ha salido mal, inténtelo más tarde.";
                return View(camp);

            }
            return RedirectToAction("Login", "Usuario");
        }



        public List<Campania> TraerCampaniasAlc(string Id)
        {
            List<Campania> retorno = null;
         
            try
            {
                string parameters = "CampaniaAPI/AlcanciaCampanias";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + Id);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    retorno = JsonConvert.DeserializeObject<List<Campania>>(json);
                    return retorno;
                }
                return retorno;
            }
            catch (Exception ex)
            {

                ViewBag.Mensaje = ex.Message.ToString();
                return retorno;
            }
        }
            
        
        public async Task<ActionResult> VerCampanias(string Id)
        {
            List<Campania> alcCamp = TraerCampaniasAlc(Id);
            if (alcCamp != null)
            {
                return View(alcCamp);
            }
            //return View(alcCamp);
            TempData["Mensaje"] = "Para asociar una alcancía a una campaña se debe realizar una solicitud primero.";
            TempData["Status"] = false;
            return RedirectToAction("MostrarAlcancias", "Alcancia");

        }

        public ActionResult ListadoCampanias()
        {
            List<Campania> lstCamp = TraerCampanias();
            return View(lstCamp);
        }

        public List<Campania> TraerCampanias()
        {
            List<Campania> camps = null;

            try
            {
                string parameters = "CampaniaAPI/TodasCampanias";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    camps = JsonConvert.DeserializeObject<List<Campania>>(json);
                    return camps;
                }
                return camps;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return camps;
            }
        }

        public Campania TraerCampania(int id)
        {
            Campania camp = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "CampaniaAPI/GetCampania";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        camp = JsonConvert.DeserializeObject<Campania>(json);
                        return camp;
                    }
                    return camp;
                }
                catch (Exception ex)
                {

                    ViewBag.Mensaje = ex.Message.ToString();
                    return camp;
                }
            }
            return camp;
        }
    }
}
