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
using TeletonMVC.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
//using System.Text.Json.JsonSerializer;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TeletonMVC.Controllers
{
    public class ColaboradorController : Controller
    {

        private readonly ILogger<ColaboradorController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;

        public ColaboradorController(ILogger<ColaboradorController> logger, MiAppContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }



        // GET: ColaboradorController
        public ActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return RedirectToAction("MostrarTodosCols");
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: ColaboradorController/Details/5
        public ActionResult Details(int Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var col = TraerColaborador(Id);
                return View(col);
            }
            return RedirectToAction("Login", "Usuario");
        }

        public ActionResult AltaColaboradorPasos()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                ViewBag.Departamentos = ObtenerDepartamentos();
                ViewBag.TiposCols = TraerTodosTipoCol();

                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: ColaboradorController/Create
        public ActionResult AltaColaborador() //Ete es el primer metodo que llama cuando empezamos la solicitud, a buscar un colaborador existente
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                ViewBag.Cols = TraerTodosCols();
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        public async Task<ActionResult> CrearColaborador(int SelCol)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                if (SelCol != 0)
                {
                    Colaborador col = TraerColaborador(SelCol);

                    //Lo guardo en Session
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(col));
                    HttpContext.Session.Set("Colaborador", data);

                    var estaEnProceso = HttpContext.Session.GetInt32("estaEnProceso");
                    if (estaEnProceso == 1)//Si es 1, es true, por lo que está en proceso de una solicitud y se mandará a la creación del Repsonsable
                    {
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se agregó correctamente al Donante " + col.Nombre + ".";
                        return RedirectToAction("AltaResponsable", "Responsable");
                    }
                }
                else
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Debe seleccionar una opción.";
                    return RedirectToAction("AltaColaborador");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        [HttpPost]
        public async Task<ActionResult> AltaColaboradorPasosAsync(Colaborador col, int SelTipoCol, string SelDept)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                if (col.Nombre != null && col.Direccion != null && SelDept != null && col.Localidad != null)
                {
                    //col.TipoCol = TraerTipoColaborador(SelTipoCol);
                    //
                    //Si los datos son correctos, voy a dar de alta el colaborador
                    if (SelTipoCol != 0)
                    {
                        col.Departamento = SelDept;
                        col.Localidad = col.Localidad.ToUpper();
                        TipoColaborador tipoCol = BuscarTipoCol(SelTipoCol);
                        if (tipoCol == null || (tipoCol.Nombre == "Empresa" && col.RazonSocial == null))
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = "Debe completar los datos.";
                            return RedirectToAction("AltaColaboradorPasos", col);
                        }

                        col.RazonSocial = "NoEmpresa";
                        col.TipoColId = SelTipoCol;
                        Solicitud solicitudNueva = new Solicitud();
                        try
                        {
                            //Genero una consulta a la API para chequear si existe el colaborador
                            var colabEncontrado = TraerColaborador(col.Id);

                            if (colabEncontrado != null)
                            {
                                TempData["Status"] = false;
                                TempData["Mensaje"] = "El donador ingresado ya existe en el sistema (Razón Social).";
                                return RedirectToAction("AltaColaborador");
                            }

                            string parameters = "ColaboradorAPI/AltaColaborador";
                            string json = JsonConvert.SerializeObject(col, Formatting.Indented);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var response = await ApiClient.PostAsync(parameters, content);

                            if (response.IsSuccessStatusCode)
                            {
                                Task<string> taskContent = response.Content.ReadAsStringAsync();
                                taskContent.Wait();
                                json = taskContent.Result;
                                col = JsonConvert.DeserializeObject<Colaborador>(json);

                                TempData["Status"] = true;

                                //Lo guardo en Session
                                solicitudNueva.Colaborador = col;
                                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(col));
                                HttpContext.Session.Set("Colaborador", data);

                                TempData["Status"] = true;
                                TempData["Mensaje"] = "Se agregó correctamente al donador " + col.Nombre + ".";
                                return RedirectToAction("AltaResponsable", "Responsable");
                            }

                            TempData["Status"] = false;
                            TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                            return RedirectToAction("AltaColaborador");
                        }
                        catch (Exception ex)
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = ex.Message.ToString();
                            return RedirectToAction("AltaColaborador");
                        }

                    }
                    else
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Debe seleccionar una opción.";
                        return RedirectToAction("AltaColaboradorPasos");
                    }
                }
                else
                {

                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Debe completar los datos.";
                    return RedirectToAction("AltaColaboradorPasos");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: ColaboradorController/Edit/5
        public ActionResult Edit(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                Colaborador col = TraerColaborador(id);
                ViewBag.TiposCols = TraerTodosTipoCol();
                //ViewBag.Cols = TraerTodosCols();
                return View(col);
            }
            return RedirectToAction("Login", "Usuario");
        }

        //Hay que pasarlo por el BOdy a la API
        // POST: ColaboradorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Colaborador col, int? SelTipoCol)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                
                if (SelTipoCol == null)
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Debe seleccionar una opcion";
                    return RedirectToAction("Edit");
                }
                try
                {
                    col.TipoColId = (int)SelTipoCol;
                    string parameters = "ColaboradorAPI/EditColaborador";

                   // string json = JsonConvert.SerializeObject(col, Formatting.Indented);
                   // var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = ApiClient.GetAsync(parameters + "?Id=" + SelTipoCol);

                    response.Wait();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        col = JsonConvert.DeserializeObject<Colaborador>(json);

                        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(col));
                        HttpContext.Session.Set("Colaborador", data);

                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se editó correctamente el Donante.";
                        return RedirectToAction("MostrarTodosCols");
                    }
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Hubo un error, intente nuevamente.";
                    return RedirectToAction("Edit");

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

        public ActionResult Delete(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    string parameters = "ColaboradorAPI/EliminarColaborador";
                    Task<HttpResponseMessage> respuesta = ApiClient.DeleteAsync(parameters + "?Id=" + id);
                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        Colaborador col = JsonConvert.DeserializeObject<Colaborador>(json);
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se eliminó correctamente al Donante " + col.Nombre;
                        return RedirectToAction("MostrarTodosCols");

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
        public async Task<IActionResult> MostrarTodosCols()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                List<Colaborador> listCols = TraerTodosCols();
                return View(listCols);
            }
            return RedirectToAction("Login", "Usuario");
        }



        public Colaborador TraerColaborador(int id)
        {
            Colaborador col = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "ColaboradorAPI/GetColaborador";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        col = JsonConvert.DeserializeObject<Colaborador>(json);
                        return col;
                    }
                    return col;
                }
                catch (Exception ex)
                {

                    ViewBag.Mensaje = ex.Message.ToString();
                    return col;
                }
            }
            return col;
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

        public List<Colaborador> TraerTodosCols()
        {
            var listCols = new List<Colaborador>();

            try
            {
                string parameters = "ColaboradorAPI/GetListColaborador";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listCols = JsonConvert.DeserializeObject<List<Colaborador>>(json);
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

        public TipoColaborador BuscarTipoCol(int SelTipoCol)
        {
            TipoColaborador tpc = new TipoColaborador();
            try
            {
                string parameters = "TipoColaboradorAPI/GetTipoColaborador";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + SelTipoCol);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    tpc = JsonConvert.DeserializeObject<TipoColaborador>(json);
                    return tpc;
                }
            }
            catch (Exception ex)
            {
                return tpc;
            }
            return tpc;
        }

        public JsonResult TraerTodosTipoColJSON()
        {
            var listCols = new List<TipoColaborador>();
            var retorno = "";
            try
            {


                string parameters = "TipoColaboradorAPI/GetListTipoColaborador";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);


                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listCols = JsonConvert.DeserializeObject<List<TipoColaborador>>(json);
                    retorno = JsonConvert.SerializeObject(listCols, Formatting.Indented);
                    return Json(retorno);
                }
                return Json(retorno);
            }
            catch (Exception ex)
            {
                return Json(retorno);
            }
        }

        public List<string> ObtenerDepartamentos()
        {
            List<string> departamentos = new List<string>();
            departamentos.Add("Montevideo");
            departamentos.Add("Río Negro");
            departamentos.Add("Artigas");
            departamentos.Add("Canelones");
            departamentos.Add("Cerro Largo");
            departamentos.Add("Colonia");
            departamentos.Add("Durazno");
            departamentos.Add("Flores");
            departamentos.Add("Florida");
            departamentos.Add("Lavalleja");
            departamentos.Add("Maldonado");
            departamentos.Add("Paysandú");
            departamentos.Add("Rivera");
            departamentos.Add("Rocha");
            departamentos.Add("Salto");
            departamentos.Add("San Jose");
            departamentos.Add("Soriano");
            departamentos.Add("Tacuaraembó");
            departamentos.Add("Treinta y Tres");

            return departamentos;
        }
    }
}
