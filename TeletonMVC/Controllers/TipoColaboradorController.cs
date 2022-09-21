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
using System.Threading.Tasks;

namespace TeletonMVC.Controllers
{
    public class TipoColaboradorController : Controller
    {

        private readonly ILogger<TipoColaboradorController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;

        public TipoColaboradorController(ILogger<TipoColaboradorController> logger, MiAppContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }
         

        // GET: TipoColaboradorController
        public ActionResult Index(string msj, bool esStatus, int tcId)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                ViewBag.TipoColId = tcId;
                ViewBag.Mensaje = msj;  
                ViewBag.Status= esStatus;
                var tipoCols = TraerTodosTipoCol();
                return View(tipoCols);
            }
            return RedirectToAction("Login","Usuario");
        }

        // GET: TipoColaboradorController/Details/5
        public ActionResult Details(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var tpc = TraerTipoColaborador((int)Id);
                if (tpc == null)
                {
                    ViewBag.Mensaje = "Tipo de colaborador no encontrado";
                    return View();

                }
                return View(tpc);
            }
            return RedirectToAction("Login","Usuario");
        }

        // GET: TipoColaboradorController/Create
        public ActionResult Create()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return View();
            }
            return RedirectToAction("Login","Usuario");
        }

        // POST: TipoColaboradorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Nombre)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                if (Nombre != "")
                {
                    try
                    {

                        string parameters = "TipoColaboradorAPI/AltaTipoColaborador";
                        var response = ApiClient.GetAsync(parameters + "?Nombre=" + Nombre);


                        ////Genero la configuracion para la api
                        //Uri uri = new Uri("https://localhost:44334/api/TipoColaboradorAPI/AltaTipoColaborador"); 
                        //HttpClient clienteApi = new HttpClient();
                        //string key = _configuration.GetSection("ApiKey")?.Value;
                        //clienteApi.DefaultRequestHeaders.Add("ApiKey", key);

                        //Task<HttpResponseMessage> respuesta = clienteApi.GetAsync(uri + "?Nombre=" + Nombre);

                        response.Wait();
                        if (response.Result.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            var json = taskContent.Result;
                            var tipoCol= JsonConvert.DeserializeObject<TipoColaborador>(json);

                            ViewBag.Status = true;
                            return RedirectToAction("Index", new { msj = "Se agregó correctamente el tipo de colaborador " + tipoCol.Nombre, 
                                status = true, tcId = tipoCol.Id });
                        }
                        ViewBag.Mensaje = "Ha ocurrido un error, intente nuevamente.";
                        ViewBag.Status = false;
                        return View();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Mensaje = ex.Message.ToString();
                        ViewBag.Status = false;
                        return View();
                    }
                }
                else
                {
                    ViewBag.Mensaje = "Debe completar los campos.";
                    ViewBag.Status = false;
                    return View();
                }
            }
            return RedirectToAction("Login","Usuario");
        }

        // GET: TipoColaboradorController/Edit/5
        public ActionResult Edit(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var tpc = TraerTipoColaborador((int)Id);
                if (tpc == null)
                {
                    ViewBag.Mensaje = "Tipo de colaborador no encontrado";
                    return View();
                }
                return View(tpc);
            }
            return RedirectToAction("Login","Usuario");
        }

        // POST: TipoColaboradorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? Id, string Nombre)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                if (Nombre != "")
                {
                    try
                    {
                        
                        TipoColaborador tpc = TraerTipoColaborador((int)Id);

                        if(tpc.Nombre == Nombre)
                        {
                            ViewBag.Mensaje = "No se ha realizado ningún cambio";
                            ViewBag.Status = false;
                            return View();
                        }


                        string parameters = "TipoColaboradorAPI/UpdateTipoColaborador";
                        var response = ApiClient.GetAsync(parameters+"?Id=" + Id + "&Nombre=" + Nombre);

                        response.Wait();
                        if (response.Result.IsSuccessStatusCode)
                        {
                            string mensaje = "Se modificó correctamente el tipo de colaborador "+Nombre+".";
                            //ViewBag.Status = true;
                            //return View();
                            return RedirectToAction("Index", new { msj = mensaje, tcId = Id});

                        }
                        ViewBag.Mensaje = "Ha ocurrido un error, intente nuevamente.";
                        ViewBag.Status = false;
                        return View();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Mensaje = "Error para ingresar los datos, verifique porfavor otra vez";
                        ViewBag.Status = false;
                        return View();
                    }
                }
                else
                {
                    ViewBag.Mensaje = "Debe completar los campos.";
                    ViewBag.Status = false;
                    return View();
                }
            }
            return RedirectToAction("Login","Usuario");
        }

        public ActionResult Delete(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    //Genero la configuracion para la api
                    string parameters = "TipoColaboradorAPI/EliminarTipoColaborador";


                    Task<HttpResponseMessage> respuesta = ApiClient.DeleteAsync(parameters + "?Id=" + Id);

                    respuesta.Wait();
                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        string mensaje = "Se eliminó correctamente el tipo de colaborador.";
                        return RedirectToAction("Index", new { msj = mensaje});
                    }
                    ViewBag.Mensaje = "Ha ocurrido un error, intente nuevamente.";
                    ViewBag.Status = false;
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.Mensaje = "Ha ocurrido un error, intentelo nuevamente verificando los datos.";
                    ViewBag.Status = false;
                    return View();
                }
            }
            return RedirectToAction("Login","Usuario");
        }


        public TipoColaborador TraerTipoColaborador(int id)
        {

            TipoColaborador tipoCol = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "TipoColaboradorAPI/GetTipoColaborador";

                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        tipoCol = JsonConvert.DeserializeObject<TipoColaborador>(json);
                        return tipoCol;
                    }
                    return tipoCol;
                }
                catch (Exception ex)
                {
                    ViewBag.Mensaje = ex.Message.ToString();
                    return tipoCol;
                }
            }
            return tipoCol;

        }

        public List<TipoColaborador> TraerTodosTipoCol()
        {
            var listCols = new List<TipoColaborador>();
            try
            {
                string parameters = "TipoColaboradorAPI/GetListTipoColaborador";

                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listCols = JsonConvert.DeserializeObject<List<TipoColaborador>>(json);
                    ViewBag.Status = true;
                    return listCols;
                }
                return listCols;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return listCols;
            }
        }
    }
}
