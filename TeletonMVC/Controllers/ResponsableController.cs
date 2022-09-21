using Dominio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TeletonMVC.Controllers
{
    public class ResponsableController : Controller
    {
        private readonly ILogger<ResponsableController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;

        public ResponsableController(ILogger<ResponsableController> logger, MiAppContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }


        // GET: ResponsableController
        public ActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return RedirectToAction("MostrarTodosResponsables");
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: ResponsableController/Details/5
        public ActionResult Details(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var resp = TraerResponsables();
                foreach (var item in resp)
                {
                    if (item.Id == id)
                    {
                        return View(item);

                    }
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        public List<Responsable> TraerResponsables()
        {
            List<Responsable> r = new List<Responsable>();
            try
            {
                string parameters = "ResponsableAPI/TodosResp";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    r = JsonConvert.DeserializeObject<List<Responsable>>(json);
                    ViewBag.Status = true;
                }
                return r;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return null;
            }
        }


        public ActionResult AltaResponsable()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                //Traigo al colaborador en caso que vuelva para atrás, mostrarle el formulario completo, enviándolo a edit del colaborador 
                var valueTraido = HttpContext.Session.Get("Colaborador");
                ViewBag.Colaborador = JsonSerializer.Deserialize<Colaborador>(valueTraido)!;
                ViewBag.Resp = TraerResponsables();
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AltaResponsable(int SelResp) //Si ya vino por aca, ya existe en la bd y simplemente lo guardo y paso
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {

                if (SelResp != 0)
                {
                    try
                    {
                        Responsable resp = TraerResponsable(SelResp);

                        if (resp != null)
                        {
                            TempData["Status"] = true;
                            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resp));
                            HttpContext.Session.Set("Responsable", data);
                            HttpContext.Session.SetInt32("EsRetira", resp.Retira ? 1 : 0);

                            var estaEnProceso = HttpContext.Session.GetInt32("estaEnProceso");
                            if (estaEnProceso == 1)//Si es 1, es true, por lo que está en proceso de una solicitud y se mandará a la creación del Retira
                            {
                                TempData["Mensaje"] = "Se agregó correctamente al Responsable " + resp.Nombre + ".";
                                return RedirectToAction("AltaRetira", "Retira");
                            }

                            TempData["Mensaje"] = "Se agregó correctamente al Responsable.";
                            return RedirectToAction("AltaRetira", "Retira");
                        }
                        TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                        TempData["Status"] = false;
                        return RedirectToAction("AltaResponsable");
                    }
                    catch (Exception ex)
                    {
                        TempData["Mensaje"] = ex.Message.ToString();
                        TempData["Status"] = false;
                        return RedirectToAction("AltaResponsable");
                    }
                }
                else
                {
                    TempData["Mensaje"] = "No se ha seleccionado ningún Responsable";
                    TempData["Status"] = false;
                    return RedirectToAction("AltaResponsable");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        // GET: ResponsableController/Create
        public ActionResult AltaResponsablePaso(string msj)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                //Traigo al colaborador en caso que vuelva para atrás, mostrarle el formulario completo, enviándolo a edit del colaborador 
                var valueTraido = HttpContext.Session.Get("Colaborador");
                ViewBag.Colaborador = JsonSerializer.Deserialize<Colaborador>(valueTraido)!;
                ViewBag.Mensaje = msj;
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: ResponsableController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AltaResponsablePaso(Responsable resp)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                if (resp.Nombre != null && resp.Cedula != null && resp.Correo != null && resp.Telefono != null)
                {
                    if (resp.Cedula.Length >= 7 && resp.Cedula.Length <= 8 && resp.Telefono.Length >= 8 && resp.Telefono.Length <= 9)
                    {
                        try
                        {
                            //Acá genero una consulta a la API para chequear que no exista el Responsable ingresado
                            var respEncontrado = TraerResponsableCedula(resp.Cedula);

                            if (respEncontrado != null)
                            {
                                TempData["Status"] = false;
                                TempData["Mensaje"] = "El Responsable ingresado ya existe en el sistema (Cédula).";
                                return RedirectToAction("AltaResponsable");
                            }


                            string parameters = "ResponsableAPI/AltaResponsable";

                            string json = JsonConvert.SerializeObject(resp, Formatting.Indented);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var response = await ApiClient.PostAsync(parameters, content);

                            if (response.IsSuccessStatusCode)
                            {
                                Task<string> taskContent = response.Content.ReadAsStringAsync();
                                taskContent.Wait();
                                json = taskContent.Result;
                                resp = JsonConvert.DeserializeObject<Responsable>(json);
                                TempData["Status"] = true;

                                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resp));
                                HttpContext.Session.Set("Responsable", data);
                                HttpContext.Session.SetInt32("ResponsableId", resp.Id);
                                HttpContext.Session.SetInt32("EsRetira", resp.Retira ? 1 : 0);

                                if (resp.Retira)
                                {

                                    TempData["Status"] = true;
                                    TempData["Mensaje"] = "Se agregó correctamente al Responsable.";
                                    return RedirectToAction("AltaRetiraPasos", "Retira");
                                }

                                TempData["Mensaje"] = "Se agregó correctamente al Responsable.";
                                return RedirectToAction("AltaRetira", "Retira");
                            }
                            TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                            TempData["Status"] = false;
                            return View(resp);
                        }
                        catch (Exception ex)
                        {
                            TempData["Mensaje"] = ex.Message.ToString();
                            TempData["Status"] = false;
                            string mensaje = ex.Message.ToString();
                            return View(resp);
                        }
                    }
                    else
                    {
                        TempData["Mensaje"] = "La cédula es completa, sin puntos ni guiones, de 7 u 8 dígitos. El teléfono es 8 ó 9.";
                        TempData["Status"] = false;
                        return View(resp);
                    }
                }
                else
                {
                    TempData["Mensaje"] = "Debe completar los datos.";
                    TempData["Status"] = false;
                    return View(resp);
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: ResponsableController/Edit/5
        public ActionResult Edit(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var resp = TraerResponsable(id);
                return View(resp);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: ResponsableController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Responsable resp)
        {//Hay que agregar que si se viene a editar desde una creación de solicitud, que continúe el proceso
         // var estaEnProceso = HttpContext.Session.GetInt32("estaEnProceso");
            var estaEnProceso = 1;
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {

                try
                {
                    string parameters = "ResponsableAPI/EditResponsable";

                    string json = JsonConvert.SerializeObject(resp, Formatting.Indented);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = ApiClient.PutAsync(parameters, content);

                    response.Wait();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        json = taskContent.Result;
                        resp = JsonConvert.DeserializeObject<Responsable>(json);
                        ViewBag.Status = true;

                        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resp));
                        HttpContext.Session.Set("Colaborador", data);
                        if (estaEnProceso == 1)
                        {
                            TempData["Status"] = true;
                            TempData["Mensaje"] = "Se modificó correctamente al Responsable " + resp.Nombre + ".";
                            return RedirectToAction("AltaRetira", "Retira");
                        }
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se editó correctamente el Colaborador.";
                        return RedirectToAction("MostrarTodosCols");

                    }
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        // POST: ResponsableController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    string parameters = "ResponsableAPI/EliminarResponsable";

                    Task<HttpResponseMessage> respuesta = ApiClient.DeleteAsync(parameters + "?Id=" + id);
                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        Responsable resp = JsonConvert.DeserializeObject<Responsable>(json);
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se eliminó correctamente al Responsable " + resp.Nombre;
                        return RedirectToAction("MostrarTodosResponsables");
                    }
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                    return RedirectToAction("MostrarTodosCols");
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


        [HttpGet]
        public ActionResult MostrarTodosResponsables()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var responsables = TraerResponsables();
                return View(responsables);
            }
            return RedirectToAction("Login", "Usuario");
        }
        public async Task<ActionResult> VerResponsablesDeColAsync(int Id)
        {
            List<Responsable> responsables = new List<Responsable>();
            try
            {
                string parameters = "ResponsableAPI/GetListResponsablesDeCol";
                var response = ApiClient.GetAsync(parameters + "?Id=" + Id);

                response.Wait();
                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    responsables = JsonConvert.DeserializeObject<List<Responsable>>(json);
                    return View(responsables);
                }
                return View(responsables);
            }
            catch (Exception ex)
            {
                return View(responsables);
            }
        }

        public Responsable TraerResponsable(int id)
        {
            Responsable resp = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "ResponsableAPI/BuscarResp";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        resp = JsonConvert.DeserializeObject<Responsable>(json);
                        return resp;
                    }
                    return resp;
                }
                catch (Exception ex)
                {

                    ViewBag.Mensaje = ex.Message.ToString();
                    return resp;
                }
            }
            return resp;
        }

        public Responsable TraerResponsableCedula(string cedula)
        {
            Responsable resp = null;
            if (cedula != "")
            {
                try
                {
                    string parameters = "ResponsableAPI/BuscarResponsableCedula";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Cedula=" + cedula);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        resp = JsonConvert.DeserializeObject<Responsable>(json);
                        return resp;
                    }
                    return resp;
                }
                catch (Exception ex)
                {

                    ViewBag.Mensaje = ex.Message.ToString();
                    return resp;
                }
            }
            return resp;
        }


    }
}
