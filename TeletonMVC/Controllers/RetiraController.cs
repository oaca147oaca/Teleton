using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repositorios;
using Dominio;
using Repositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TeletonMVC.Controllers
{
    public class RetiraController : Controller
    {

        private readonly ILogger<RetiraController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;

        public RetiraController(ILogger<RetiraController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }
        // GET: RetiraController
        public ActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return RedirectToAction("MostrarTodosRetiran");

            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: RetiraController/Details/5
        public ActionResult Details(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    string parameters = "RetiraAPI/BuscarRetira";
                    Task<HttpResponseMessage> response = ApiClient.GetAsync(parameters + "?Id=" + id);

                    response.Wait();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        Retira r = new Retira();
                        r = JsonConvert.DeserializeObject<Retira>(json);
                        return View(r);
                    }
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Algo salio mal, intentelo mas tarde";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = ex.Message.ToString();
                    return RedirectToAction("Index");
                }

            }
            return RedirectToAction("Login", "Usuario");
        }

        public List<Retira> TraerRetiran()
        {
            List<Retira> lstRet = new List<Retira>();

            try
            {
                string parameters = "RetiraAPI/GetListRetira";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    lstRet = JsonConvert.DeserializeObject<List<Retira>>(json);
                    return lstRet;
                }
                return lstRet;
            }
            catch (Exception ex)
            {
                TempData["Status"] = false;
                TempData["Mensaje"] = ex.Message.ToString();
                return lstRet;
            }
        }


        // GET: RetiraController/Create
        public ActionResult AltaRetira()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                var esRetira = HttpContext.Session.GetInt32("EsRetira").GetValueOrDefault();
                //if (esRetira == 1)
                //{
                //    ColaboradorEsRetira();
                //}
                ViewBag.EsRetira = HttpContext.Session.GetInt32("EsRetira") == 1 ? true : false;
                //Traigo al responsable en caso que vuelva para atrás, mostrarle el formulario completo, enviándolo a edit del responsable 
                var valueTraido = HttpContext.Session.Get("Responsable");
                ViewBag.Responsable = JsonSerializer.Deserialize<Responsable>(valueTraido)!;

                ViewBag.Retiran = TraerRetiran();

                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AltaRetira(int SelRet)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                if (SelRet != 0)
                {
                    try
                    {
                        Retira r = new Retira();
                        string parameters = "RetiraAPI/BuscarRetira";
                        Task<HttpResponseMessage> response = ApiClient.GetAsync(parameters + "?Id=" + SelRet);

                        response.Wait();

                        if (response.Result.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            string json = taskContent.Result;
                            r = JsonConvert.DeserializeObject<Retira>(json);
                            TempData["Status"] = true;

                            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(r));
                            HttpContext.Session.Set("Retira", data);
                            HttpContext.Session.SetInt32("RetiraId", r.Id);

                            var estaEnProceso = HttpContext.Session.GetInt32("estaEnProceso");
                            if (estaEnProceso == 1)//Si es 1, es true, por lo que está en proceso de una solicitud y se mandará a la creación de la solicitud
                            {
                                TempData["Mensaje"] = "Se agregó correctamente el que Retira " + r.Nombre + ".";
                                HttpContext.Session.SetInt32("SolicitudId", 0);
                                return RedirectToAction("Create", "Solicitud");

                            }

                            TempData["Mensaje"] = "Se agregó el que Retira correctamente";
                            return RedirectToAction("Create", "Solicitud");
                        }
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                        return RedirectToAction("AltaRetira");
                    }
                    catch (Exception ex)
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = ex.Message.ToString();
                        return RedirectToAction("AltaRetira");
                    }
                }
                else
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "No se ha seleccionado ningún Retira.";
                    return RedirectToAction("AltaRetira");
                }
              
            }
            return RedirectToAction("Login", "Usuario");
        }

        [HttpGet]
        public ActionResult AltaRetiraPasos()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                bool esRetira = HttpContext.Session.GetInt32("EsRetira") == 1 ? true : false;
                ViewBag.EsRetira = esRetira;
                if (esRetira)
                {
                    var valueTraido = HttpContext.Session.Get("Responsable");
                    var resp = JsonSerializer.Deserialize<Responsable>(valueTraido)!;
                    ViewBag.Responsable = resp;
                    Retira ret = new Retira
                    {
                        Nombre = resp.Nombre,
                        Cedula = resp.Cedula,
                        Telefono = resp.Telefono
                    };
                    return View(ret);
                }
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }


        // POST: RetiraController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AltaRetiraPasos(Retira ret)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                if (ret.Nombre != null && ret.Cedula != null && ret.Telefono != null)
                {
                    try
                    {
                        //Acá genero una consulta a la API para chequear que no exista el que Retira que ingresaron
                        var retEncontrado = TraerRetiraCedula(ret.Cedula);

                        if (retEncontrado != null)
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = "El que Retira ingresado ya existe en el sistema (Cédula).";
                            return RedirectToAction("AltaRetira");
                        }

                        string parameters = "RetiraAPI/AltaRetira";


                        string json = JsonConvert.SerializeObject(ret, Formatting.Indented);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await ApiClient.PostAsync(parameters, content);

                        if (response.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            json = taskContent.Result;
                            ret = JsonConvert.DeserializeObject<Retira>(json);
                            TempData["Status"] = true;

                            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ret));
                            HttpContext.Session.Set("Retira", data);
                            HttpContext.Session.SetInt32("RetiraId", ret.Id);

                            var estaEnProceso = HttpContext.Session.GetInt32("estaEnProceso");
                            if (estaEnProceso == 1)//Si es 1, es true, por lo que está en proceso de una solicitud y se mandará a la creación de la solicitud
                            {
                                TempData["Mensaje"] = "Se agregó correctamente el que Retira: " + ret.Nombre + ".";
                                HttpContext.Session.SetInt32("SolicitudId", 0);
                                return RedirectToAction("Create", "Solicitud");

                            }

                            TempData["Mensaje"] = "Se agregó el que Retira correctamente";
                            return RedirectToAction("Create", "Solicitud");
                        }
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                        return RedirectToAction("AltaRetira");
                    }
                    catch (Exception ex)
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = ex.Message.ToString();
                        return RedirectToAction("AltaRetira");
                    }
                }
                else
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Debe completar los datos.";

                    return RedirectToAction("AltaRetiraPasos");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: RetiraController/Edit/5
        public ActionResult Edit(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var ret = TraerRetira((int)Id);
                return View(ret);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: RetiraController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Retira r)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                string parameters = "RetiraAPI/EditRetira";

                string json = JsonConvert.SerializeObject(r, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = ApiClient.PutAsync(parameters, content);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    TempData["Status"] = true;
                    TempData["Mensaje"] = "Se ha editado correctamente";
                    return RedirectToAction("Index");
                }
                TempData["Status"] = false;
                TempData["Mensaje"] = "Algo ha salido mal, intentelo mas tarde";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login", "Usuario");
        }
        public ActionResult VerRetirosDeCol(int Id)
        {
            List<Retira> retiran = new List<Retira>();
            try
            {
                string parameters = "RetiraAPI/GetListRetiranDeCol";
                var response = ApiClient.GetAsync(parameters + "?Id=" + Id);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    retiran = JsonConvert.DeserializeObject<List<Retira>>(json);
                    return View(retiran);
                }
                TempData["Status"] = false;
                TempData["Mensaje"] = "Algo ha salido mal, inténtelo más tarde.";
                return View();
            }
            catch (Exception ex)
            {
                TempData["Status"] = false;
                TempData["Mensaje"] = "Algo ha salido mal, inténtelo más tarde.";
                return View();
            }
        }
        // GET: RetiraController/Delete/5
        public ActionResult Delete(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                string parameters = "RetiraAPI/DeleteRetira";
                Task<HttpResponseMessage> response = ApiClient.GetAsync(parameters + "?Id=" + Id);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    TempData["Status"] = true;
                    TempData["Mensaje"] = "Se ha elimnado correctamente";
                    return RedirectToAction("VerRetirosDeCol");
                }
                TempData["Status"] = false;
                TempData["Mensaje"] = "Algo ha salido mal, intentelo más tarde.";
                return RedirectToAction("VerRetirosDeCol");
            }
            return RedirectToAction("Login", "Usuario");
        }


        [HttpGet]
        public ActionResult MostrarTodosRetiran()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var ret = TraerRetiran();
                return View("MostrarTodosRetiran", ret);
            }
            return RedirectToAction("Login", "Usuario");
        }

        public Retira TraerRetira(int id)
        {
            Retira ret = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "RetiraAPI/BuscarRetira";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        ret = JsonConvert.DeserializeObject<Retira>(json);
                        return ret;
                    }
                    return ret;
                }
                catch (Exception ex)
                {

                    ViewBag.Mensaje = ex.Message.ToString();
                    return ret;
                }
            }
            return ret;
        }

        public Retira TraerRetiraCedula(string cedula)
        {
            Retira ret = null;
            if (cedula != "")
            {
                try
                {
                    string parameters = "RetiraAPI/BuscarRetiraCedula";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Cedula=" + cedula);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        ret = JsonConvert.DeserializeObject<Retira>(json);
                        return ret;
                    }
                    return ret;
                }
                catch (Exception ex)
                {

                    ViewBag.Mensaje = ex.Message.ToString();
                    return ret;
                }
            }
            return ret;
        }

    }
}
