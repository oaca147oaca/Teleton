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
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using Syncfusion.XlsIO;
using Dominio.EntidadesDeNegocio;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata;
using SelectPdf;
using JsonSerializer = System.Text.Json.JsonSerializer;



namespace TeletonMVC.Controllers
{
    public class SolicitudController : Controller
    {

        private readonly ILogger<SolicitudController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        ICompositeViewEngine _compositeViewEngine;
        private Campania campaniaActual;
        private readonly HttpClient ApiClient;


        public SolicitudController(ILogger<SolicitudController> logger, MiAppContext dbContext, IConfiguration configuration, ICompositeViewEngine compositeViewEngine)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            _compositeViewEngine = compositeViewEngine;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);


            string parameters = "CampaniaAPI/GetCampaniaActual";
            Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);
            this.campaniaActual = Campania.TraerCampaniaActual(key, respuesta);
        }
        // GET: SolicitudController
        public ActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return RedirectToAction("MostrarTodasSols");
            }
            return RedirectToAction("Login", "Usuario");
        }


        /////////////////////////////////////////////////////////////////////////////////-----Mostrar Solicitudes-----//////////////////////////////////////////////////////////////////////////////
        public async Task<ActionResult> MostrarTodasSols()
        {

            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                string parameters = "SolicitudAPI/GetListSolicitud";
                var response = await ApiClient.GetAsync(parameters);

                if (response.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    var listSols = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                    return View(listSols);
                }
                return View();

            }
            return RedirectToAction("Login", "Usuario");
        }

        public async Task<ActionResult> MostrarPreSols()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                string parameters = "SolicitudAPI/GetListPreSolicitud";
                var response = await ApiClient.GetAsync(parameters);

                if (response.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    var listSols = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                    return View(listSols);
                }
                TempData["Mensaje"] = "Ocurrió un error, intente nuevamente.";
                TempData["Status"] = false;
                return View();

            }
            return RedirectToAction("Login", "Usuario");
        }

        public async Task<ActionResult> MostrarPosSols()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol== "Contabilidad")
            {
                string parameters = "SolicitudAPI/GetListPosSolicitud";
                var response = await ApiClient.GetAsync(parameters);

                if (response.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    var listSols = JsonConvert.DeserializeObject<List<Solicitud>>(json);
                    return View(listSols);
                }
                TempData["Mensaje"] = "Ocurrió un error, intente nuevamente.";
                TempData["Status"] = false;
                return View();

            }
            return RedirectToAction("Login", "Usuario"); ;
        }

        /////////////////////////////////////////////////////////////////////////////////-----Hasta aca mostrar Solicitudes-----//////////////////////////////////////////////////////////////////////////////

        // GET: SolicitudController/Details/5
        public ActionResult Details(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var sol = TraerSolicitud(id);
                return View(sol);
            }
            return RedirectToAction("Login", "Usuario");
        }



        /////////////////////////////////////////////////////////////////////////////////-----Crear Solicitudes-----//////////////////////////////////////////////////////////////////////////////

        // GET: SolicitudController/Create
        public ActionResult Create(int id, string idAlc)
        {
            string rol = HttpContext.Session.GetString("Rol");

            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {

                List<string> alcanciasList = retornarListaAlcancias();

                int cantEntregadas = HttpContext.Session.GetInt32("CantEntregadas").GetValueOrDefault();
                int CantSolicitadas = HttpContext.Session.GetInt32("CantSolicitadas").GetValueOrDefault();
                //int EsPreNueva = HttpContext.Session.GetInt32("esPreNueva").GetValueOrDefault();

                ViewBag.CantEntregadas = cantEntregadas != 0 ? cantEntregadas : 0;
                ViewBag.CantSolicitadas = CantSolicitadas;
                ViewBag.ListaAlcancias = alcanciasList;
                ViewBag.AlcanciaId = idAlc;
                ViewBag.EsPreNueva = false;
                if (id != 0)
                {
                    HttpContext.Session.SetInt32("SolicitudId", (int)id);
                }

                int esPre = HttpContext.Session.GetInt32("esPre").GetValueOrDefault();
                id = HttpContext.Session.GetInt32("SolicitudId").GetValueOrDefault();

                if (esPre == 1 && id != 0)//1 sería true, por lo que se quiere continuar una pre-solicitud
                {
                    Solicitud sol = TraerPreSolicitud(id);

                    byte[] dataCol = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sol.Colaborador));
                    HttpContext.Session.Set("Colaborador", dataCol);

                    byte[] dataResp = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sol.Responsable));
                    HttpContext.Session.Set("Responsable", dataResp);

                    byte[] dataRet = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sol.Retira));
                    HttpContext.Session.Set("Retira", dataRet);

                    ViewBag.CantSolicitadas = sol.CantSolicitadas;
                    ViewBag.Solicitud = sol;

                    ViewBag.EsPre = true;
                    return View();
                }

                if (esPre != 1)
                {
                    ViewBag.EsPre = false;
                }
                else //Si entra por acá, es porque está realizando una NUEVA pre-solicitud
                {
                    ViewBag.EsPreNueva = true;
                    ViewBag.EsPre = true;
                }

                Solicitud solNew = new Solicitud();

                //Traigo a los 3 desde el Session, deserealizándolos
                Colaborador colTraido = JsonSerializer.Deserialize<Colaborador>(HttpContext.Session.Get("Colaborador"))!;
                solNew.Colaborador = colTraido;


                Responsable respTraido = JsonSerializer.Deserialize<Responsable>(HttpContext.Session.Get("Responsable"))!;
                solNew.Responsable = respTraido;

                Retira retTraido = JsonSerializer.Deserialize<Retira>(HttpContext.Session.Get("Retira"))!;
                solNew.Retira = retTraido;

                solNew.CantSolicitadas = CantSolicitadas;
                ViewBag.Solicitud = solNew;
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }


        public async Task<ActionResult> CreateSolAsync(Solicitud solRecibida)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                //Traigo a los 3 desde el Session, deserealizándolos
                Colaborador colEncontrado = JsonSerializer.Deserialize<Colaborador>(HttpContext.Session.Get("Colaborador"))!;

                Responsable respEncontrado = JsonSerializer.Deserialize<Responsable>(HttpContext.Session.Get("Responsable"))!;

                Retira retEncontrado = JsonSerializer.Deserialize<Retira>(HttpContext.Session.Get("Retira"))!;

                int cantSolicitadas = HttpContext.Session.GetInt32("CantSolicitadas").GetValueOrDefault();
                int id = HttpContext.Session.GetInt32("SolicitudId").GetValueOrDefault();

                if (colEncontrado != null && respEncontrado != null && retEncontrado != null)
                {

                    List<Alcancia> alcancias = TraerAlcancias();

                    Solicitud sol = new Solicitud()
                    {
                        CampaniaId = campaniaActual.IdCampania,
                        ColaboradorId = colEncontrado.Id,
                        ResponsableId = respEncontrado.Id,
                        RetiraId = retEncontrado.Id,
                        CantSolicitadas = cantSolicitadas,
                        CantEntregadas = 0,
                        CantDevueltas = 0,
                        LugarEntrega = rol == "VoluntarioFB" ? "FB" : "MVD"
                    };

                    int esPre = HttpContext.Session.GetInt32("esPre").GetValueOrDefault();
                    int esPreNueva = HttpContext.Session.GetInt32("esPreNueva").GetValueOrDefault();

                    if (esPre == 1 && esPreNueva != 1)
                    {
                        sol.Id = id;
                        AgregarAlcanciasAsolicitud(sol);

                        //////Seteo todos los parámetros del Session a "0"
                        List<string> alcanciasList = new List<string>();
                        byte[] alcanciasInBytes = alcanciasList.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                        HttpContext.Session.Set("ListaAlcancias", alcanciasInBytes);
                        HttpContext.Session.SetInt32("CantSolicitadas", 0);
                        TempData["Mensaje"] = "Se agregó correctamente la Solicitud";

                        return RedirectToAction("Imprimir", new { Id = sol.Id });

                        //return RedirectToAction("Index");
                    }

                    try
                    {
                        string parameters = "SolicitudAPI/AltaSolicitud";
                        string json = JsonConvert.SerializeObject(sol, Formatting.Indented);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await ApiClient.PostAsync(parameters, content);

                        if (response.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            json = taskContent.Result;
                            sol = JsonConvert.DeserializeObject<Solicitud>(json);
                            TempData["Status"] = true;

                            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sol));
                            HttpContext.Session.Set("Solicitud", data);

                            if (esPreNueva != 1)
                            {
                                AgregarAlcanciasAsolicitud(sol);

                                ////Seteo todos los parámetros del Session a "0"
                                List<string> alcanciasList = new List<string>();
                                byte[] alcanciasInBytes = alcanciasList.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                                HttpContext.Session.Set("ListaAlcancias", alcanciasInBytes);
                                HttpContext.Session.SetInt32("CantSolicitadas", 0);

                                TempData["Mensaje"] = "Se agregó correctamente la Solicitud.";
                                return RedirectToAction("Imprimir", new { Id = sol.Id });

                            }



                            TempData["Mensaje"] = "Se agregó correctamente la Solicitud.";
                            return RedirectToAction("MostrarPreSols");
                            
                            //return RedirectToAction("Imprimir", new { Id = sol.Id });
                        }

                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Hubo un error, intente nuevamente.";

                        return View("Create");
                    }
                    catch (Exception ex)
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = ex.Message.ToString();
                        return View("Create");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No se encontró alguna de las referencias.");
                }
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        public void AgregarAlcanciasAsolicitud(Solicitud sol)
        {
            List<Alcancia> alcancias = TraerAlcancias();

            foreach (var item in alcancias)
            {
                try
                {
                    var alcSolBuscada = TraerAlcSol(item.IdAlcancia, sol.Id);

                    if (alcSolBuscada == null)
                    {
                        AlcanciaSolicitud alcSol = new AlcanciaSolicitud()
                        {
                            IdAlcancia = item.IdAlcancia,
                            IdSolicitud = sol.Id,
                            FechaSolicitud = DateTime.Today,
                            Impreso = "NO",
                            MontoDolares = 0,
                            MontoPesos = 0,
                            NumeroTicket = 0,
                            EsVacia = false
                        };

                        string parameters = "SolicitudAPI/AltaAlcanciaSolicitud";
                        Task<HttpResponseMessage> respuesta = ApiClient.PostAsJsonAsync(parameters, alcSol);

                        if (respuesta.Result.IsSuccessStatusCode)
                        {
                            TempData["Mensaje"] = "Se agregó correctamente la Solicitud.";
                            TempData["Status"] = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = ex.Message.ToString();
                    TempData["Status"] = false;
                    RedirectToAction("Create");

                }
            }
            //


        }


        public ActionResult ElegirSolicitud(int valor)
        {
            string rol = HttpContext.Session.GetString("Rol");

            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB" || rol == "Contabilidad")
            {
                //Seteo todos los parámetros del Session a "0"
                List<string> alcanciasList = new List<string>();
                byte[] alcanciasInBytes = alcanciasList.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                HttpContext.Session.Set("ListaAlcancias", alcanciasInBytes);
                HttpContext.Session.SetInt32("CantSolicitadas", 0);

                if (campaniaActual == null && (rol == "Voluntariado" || rol == "Administrador"))
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Debe registrarse una campaña para poder ingresar cualquier tipo de solicitud.";
                    return RedirectToAction("AltaCampania", "Campania");
                }
                else if (campaniaActual == null && (rol == "VoluntarioMvd" || rol == "VoluntarioFB" || rol == "Contabilidad"))
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Para tener acceso a las solicitudes debe existir una campaña actualmente, llame a un supervisor para continuar.";
                    return RedirectToAction("MostrarTodasSols", "Solicitud");
                }

                HttpContext.Session.SetInt32("estaEnProceso", 1);

                if (valor == 1)
                {
                    HttpContext.Session.SetInt32("esPre", 0);
                    HttpContext.Session.SetInt32("esPreNueva", 0);
                }
                else if (valor == 3)
                {
                    HttpContext.Session.SetInt32("esPreNueva", 1);
                    HttpContext.Session.SetInt32("esPre", 1);
                }
                else if (valor == 2)
                {
                    HttpContext.Session.SetInt32("esPreNueva", 0);
                    HttpContext.Session.SetInt32("esPre", 1);
                    return RedirectToAction("MostrarPreSols");
                }
                return RedirectToAction("AltaColaborador", "Colaborador");
            }
            return RedirectToAction("Login", "Usuario");
        }
        /////////////////////////////////////////////////////////////////////////////////-----Hasta aca crear Solicitudes-----//////////////////////////////////////////////////////////////////////////////
        public ActionResult IrAnewSol()
        {
            return RedirectToAction("ElegirSolicitud", "Solicitud", new { valor = 1 });
        }
        public ActionResult IrApreSol()
        {
            return RedirectToAction("ElegirSolicitud", "Solicitud", new { valor = 2 });
        }
        public ActionResult IrAcrearPreSol()
        {
            return RedirectToAction("ElegirSolicitud", "Solicitud", new { valor = 3 });
        }

        private List<Alcancia> TraerAlcancias()
        {
            List<Alcancia> alcancias = new List<Alcancia>();
            List<string> alcanciasStrings = retornarListaAlcancias();

            foreach (var item in alcanciasStrings)
            {
                Alcancia alcEncontada = TraerAlcancia(item); 
                alcancias.Add(alcEncontada);
            }

            return alcancias;
        }
        //FALTA EL CONTINUAR PRE SOLICITUD
        public ActionResult ContinuarPreSolicitud(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");

            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")

            {
                HttpContext.Session.SetInt32("esPre", 1);

                HttpContext.Session.SetInt32("SolicitudId", id);

                return RedirectToAction("Create", new { id = id });
            }

            return RedirectToAction("Login", "Usuario");
        }


        // GET: SolicitudController/Edit/5
        public ActionResult Edit(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                var sol = TraerSolicitud(id);
                return View(sol);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: SolicitudController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Solicitud sol)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                if (sol.CantSolicitadas == 0)
                {
                    TempData["Mensaje"] = "Se debe ingresar cantidad solicitadas.";
                    TempData["Status"] = false;
                    return RedirectToAction("Edit",new {id=id });
                }
                string parameters = "SolicitudAPI/EditSolicitud";
                Task<HttpResponseMessage> response = ApiClient.GetAsync(parameters + "?Id=" + id + "&CantSol=" + sol.CantSolicitadas);
                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    TempData["Status"] = true;
                    TempData["Mensaje"] = "Se ha editado correctamente";
                    return RedirectToAction("Index");
                }
                TempData["Status"] = false;
                TempData["Mensaje"] = "Algo ha salido mal, inténtelo más tarde.";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Login", "Usuario");
        }

        public ActionResult EditAlcSol(string Id)//
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                var IdSol = HttpContext.Session.GetInt32("MostrarAlcanciasDeSol");

                //var sol = TraerSolicitud(IdSol);
                AlcanciaSolicitud alcSol = TraerAlcSol(Id, (int)IdSol);
                return View(alcSol);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: SolicitudController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAlcSol(AlcanciaSolicitud alcs, int SelImp)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                if (SelImp == 0)
                {
                    TempData["Mensaje"] = "Debe seleccionar una opción.";
                    TempData["Status"] = false;
                    return RedirectToAction("EditAlcSol", new { Id = alcs.IdAlcancia });
                }
                if (alcs.NumeroTicket > 0)
                {
                    string impreso = "";
                    switch (SelImp)
                    {
                        case 1:
                            impreso = "SI";
                            break;
                        case 2:
                            impreso = "NO";
                            break;
                    }

                    try
                    {
                        if (alcs != null)
                        {
                            var alcSol = TraerAlcSol(alcs.IdAlcancia, alcs.IdSolicitud);

                            alcSol.Impreso = impreso;
                            alcSol.NumeroTicket = alcs.NumeroTicket;

                            try
                            {
                                string parameters = "SolicitudAPI/EditAlcSol";
                                Task<HttpResponseMessage> respuesta = ApiClient.PostAsJsonAsync(parameters, alcSol);

                                if (respuesta.Result.IsSuccessStatusCode)
                                {
                                    TempData["Mensaje"] = "Se editó correctamente la Alcancía Solicitud.";
                                    TempData["Status"] = true;
                                    return RedirectToAction("MostrarAlcanciasDeSol","Alcancia",new { id = alcs.IdSolicitud });
                                }
                                TempData["Mensaje"] = "Ocurrió un error.";
                                TempData["Status"] = false;
                                return RedirectToAction("EditAlcSol", new { Id = alcs.IdAlcancia });
                            }
                            catch (Exception ex)
                            {
                                TempData["Mensaje"] = "Ocurrió un error.";
                                TempData["Status"] = false;
                                return RedirectToAction("EditAlcSol", new { Id = alcs.IdAlcancia });
                            }

                        }
                        else
                        {
                            TempData["Mensaje"] = "Ocurrió un error.";
                            TempData["Status"] = false;
                            return RedirectToAction("EditAlcSol",new {Id=alcs.IdAlcancia });
                        }
                    }
                    catch
                    {
                        return RedirectToAction("EditAlcSol", new { Id = alcs.IdAlcancia }); 
                    }
                }
                else
                {
                    TempData["Mensaje"] = "Debe completar los campos y verificar que sean válidos.";
                    TempData["Status"] = false;
                    return RedirectToAction("EditAlcSol", new { Id = alcs.IdAlcancia });

                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        public ActionResult Delete(int id)//falta
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    string parameters = "SolicitudAPI/DeleteSolicitud";
                    Task<HttpResponseMessage> response = ApiClient.GetAsync(parameters + "?Id=" + id);

                    response.Wait();

                    if (response.Result.IsSuccessStatusCode)
                    {
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se ha borrado correctamente.";
                        return RedirectToAction("MostrarTodasSols");
                    }
                    ViewBag.Status = false;
                    ViewBag.Mensaje = "Algo ha salido mal, inténtelo más tarde.";
                    return RedirectToAction("MostrarTodosUsus");
                }
                catch
                {
                    return View();
                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        [HttpPost]
        public ActionResult AgregarAlcancia(Solicitud solRecibida, bool isActive)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd")
            {
                if (solRecibida.CantSolicitadas == 0) {
                    TempData["Mensaje"] = "Se debe ingresar cantidad solicitadas.";
                    TempData["Status"] = false;
                    return RedirectToAction("Create");
                }
                HttpContext.Session.SetInt32("CantSolicitadas", solRecibida.CantSolicitadas);
                int esPreNueva = HttpContext.Session.GetInt32("esPreNueva").GetValueOrDefault();
                if (esPreNueva == 1)
                {
                    return RedirectToAction("CreateSol");
                }

                HttpContext.Session.SetInt32("CantSolicitadas", solRecibida.CantSolicitadas);
                string idAlc = solRecibida.Id.ToString();
                var alcanciasInByte = HttpContext.Session.Get("ListaAlcancias");
                List<string> alcanciasList = new List<string>();
                //string strByte = alcanciasInByte.ToString();

                if (isActive && solRecibida.Id == 0)
                {
                    if (alcanciasInByte == null)
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "No hay ninguna alcancía agregada.";
                        return RedirectToAction("Create");
                    }
                    else
                    {
                        List<string> alcanList = retornarListaAlcancias();
                        if (alcanList.Count == 0)
                        {
                            TempData["Mensaje"] = "Se eliminaron las alcancías agregadas, vuelva a ingresar.";
                            TempData["Status"] = false;
                            return RedirectToAction("Create");
                        }
                    }
                    return RedirectToAction("CreateSol", "Solicitud");
                }



                Alcancia alcEncontrada = TraerAlcancia(idAlc);

                if (alcEncontrada != null)
                {
                    if (alcEncontrada.Habilitada.ToUpper() == "NO" || alcEncontrada.Estado.ToUpper() != "DISPONIBLE")
                    {
                        TempData["Mensaje"] = "Esta Alcancía no está habilitada para ser entregada.";
                        TempData["Status"] = false;

                        return RedirectToAction("Create", new { idAlc = idAlc });
                    }

                    if (alcanciasInByte != null)
                    {
                        string alcancia = string.Empty;

                        for (int i = 0; i < alcanciasInByte.Length; i++)
                        {
                            var item = alcanciasInByte[i];
                            if (item == 0)
                            {
                                alcanciasList.Add(alcancia);
                                alcancia = string.Empty;
                            }
                            else
                            {
                                alcancia += System.Text.Encoding.ASCII.GetString(new[] { item });
                            }
                        }

                    }
                    foreach (var item in alcanciasList)
                    {
                        if (item == idAlc)
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = "Alcancía ya agregada.";

                            return RedirectToAction("Create");
                        }
                    }

                    alcanciasList.Add(idAlc);

                    byte[] alcanciasInBytes = alcanciasList.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                    HttpContext.Session.Set("ListaAlcancias", alcanciasInBytes);
                    HttpContext.Session.SetInt32("CantEntregadas", alcanciasList.Count());

                    if (isActive)
                    {
                        return RedirectToAction("CreateSol", "Solicitud");
                    }

                    TempData["Status"] = true;
                    TempData["Mensaje"] = "Alcancía agregada correctamente";
                    return RedirectToAction("Create");
                }
                else
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Alcancía no encontrada (Id) o campo vacío.";

                    return RedirectToAction("Create");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        public List<string> retornarListaAlcancias()
        {

            var alcanciasInBytes = HttpContext.Session.Get("ListaAlcancias");

            List<string> alcancias = new List<string>();
            string alcancia = string.Empty;
            if (alcanciasInBytes != null)
            {
                for (int i = 0; i < alcanciasInBytes.Length; i++)
                {
                    var item = alcanciasInBytes[i];
                    if (item == 0)
                    {
                        alcancias.Add(alcancia);
                        alcancia = string.Empty;
                    }
                    else
                    {
                        alcancia += System.Text.Encoding.ASCII.GetString(new[] { item });
                    }
                }

            }
            return alcancias;
        }

        public ActionResult DeleteAlc(string id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                List<string> alcancias = retornarListaAlcancias();

                alcancias.Remove(id);

                byte[] alcanciasInByte = alcancias.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                HttpContext.Session.Set("ListaAlcancias", alcanciasInByte);
                HttpContext.Session.SetInt32("CantEntregadas", alcancias.Count());
                TempData["Status"] = true;
                TempData["Mensaje"] = "Alcancía borrada correctamente.";

                return RedirectToAction("Create");
            }
            return RedirectToAction("Login", "Usuario");
        }


        public Solicitud TraerSolicitudEsp(int Id)
        {
            Solicitud sol = null;
            if (Id != 0)
            {
                try
                {
                    string parameters = "SolicitudAPI/ObtenerSolEsp";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + Id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        sol = JsonConvert.DeserializeObject<Solicitud>(json);
                        return sol;
                    }
                    return sol;
                }
                catch (Exception ex)
                {
                    return sol;
                }
            }
            return sol;
        }

        public async Task<IActionResult> Imprimir(int? Id)
        {
            //URL de donde saque la docu https://selectpdf.com/html-to-pdf/docs/html/Index.htm
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" ||  rol == "VoluntarioMvd" || rol == "Contabilidad")
            {
                ViewBag.alc = "";
                Solicitud solicitud = TraerSolicitudEsp((int)Id);
                //apartir de ahora todo en otro metodo async
                using (var stringWriter = new StringWriter())
                {
                    //esta linea sirve para cargar el modelo en la vista para que tenga todos los datos
                    var viewResult = _compositeViewEngine.FindView(ControllerContext, "_SolicitudPDF", false);

                    if (viewResult.View == null) //verifico que no sea nula
                    {
                        throw new ArgumentNullException("View cannot be found");
                    }

                    var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

                    TempData["Solicitud"] = solicitud;


                    var viewContext = new ViewContext(
                        ControllerContext,
                        viewResult.View,
                        viewDictionary,
                        TempData,
                        stringWriter,
                        new HtmlHelperOptions()
                        );

                    await viewResult.View.RenderAsync(viewContext);

                    var htmlToPdf = new HtmlToPdf(842, 595);
                    htmlToPdf.Options.DrawBackground = true;
                    htmlToPdf.Options.DisplayHeader = true;
                    htmlToPdf.Header.DisplayOnFirstPage = true;
                    htmlToPdf.Header.Height = 100;

                    PdfImageSection imagen = new PdfImageSection(12, 25, 90, 45, "FotoTeletonPDF.jpg");

                    htmlToPdf.Header.Add(imagen);


                    var pdf = htmlToPdf.ConvertHtmlString(stringWriter.ToString());

                    var pdfByte = pdf.Save();
                    string nombre = "Entrega " + solicitud.Colaborador.Nombre + " " +DateTime.Now.ToString("dd/MM/yyyy") + ".pdf";
                    return File(pdfByte, "application/pdf", nombre);
                   

                    //Este codigo sirve para descargarlo, cambiar el stringBase segun tu compu
                    //string stringBase = @"C:\Desktop\Entrega_" + solicitud.Colaborador.Nombre + ".pdf";

                    //using (var streamWriter = new StreamWriter(stringBase))
                    //{
                    //    await streamWriter.BaseStream.WriteAsync(pdfByte, 0, pdfByte.Length); 
                    //}

                    // return File(pdfByte, "application/pdf");
                    //return RedirectToAction("MostrarTodasSols");
                }
            }
            else
            {
                return RedirectToAction("Login", "Usuario");
            }
           
        }

        public string BuscoCuenta(int moneda, string lugar, string db)
        {
            //Moneda nacional = 1
            //Moneda dolares  = 2
            string cuenta;
            if (moneda == 1)
            {
                if (lugar == "MVD")
                {
                    if (db == "D")
                        cuenta = "1.1.03.07.02";
                    else
                        cuenta = "5.1.02.08.01";
                }
                else
                {
                    if (db == "D")
                        cuenta = "1.1.03.07.07";
                    else
                        cuenta = "5.1.02.08.02";
                }

            }
            else //Moneda Extranjera
            {
                if (lugar == "MVD")
                {
                    if (db == "D")
                        cuenta = "1.1.03.08.02";
                    else
                        cuenta = "5.1.02.08.01";
                }
                else
                {
                    if (db == "D")
                        cuenta = "1.1.03.08.07";
                    else
                        cuenta = "5.1.02.08.02";
                }
            }
            return cuenta;
        }
        public string BuscoConcepto(int moneda, string lugar)
        {
            string concepto;
            if (moneda == 1)
            {
                if (lugar == "MVD")
                {
                    concepto = "ALCANCIA MONTEVIDEO";

                }
                else
                {
                    concepto = "ALCANCIA FB";
                }

            }
            else //Moneda Extranjera
            {
                if (lugar == "MVD")
                {
                    concepto = "ALCANCIA MONTEVIDEO USD";
                }
                else
                {
                    concepto = "ALCANCIA FB USD ";
                }
            }

            return concepto;

        }

        public ActionResult ExcelDocument()
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;

                //Creamos el Workbook
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];
                //Obtengo los datos de las alcancias 
                List<AlcanciaSolicitud> alcsol = TraerSolsExcel();
                List<AlcanciaExterna> alcExt = TraerAlcanciasExt();
                int contador = 0;
                string rango = "";
                string cuenta = "";
                string concepto = "";
                string dc = "";

                foreach (AlcanciaSolicitud item in alcsol)
                {
                    if (item.MontoPesos != 0)
                    {
                        for (int x = 1; x <= 2; x++)
                        {
                            if (x == 1)
                                dc = "D";
                            else
                                dc = "H";

                            contador += 1;
                            rango = "A" + contador;
                            worksheet.Range[rango].Text = item.FechaSolicitud.ToString("dd/MM/yyyy");

                            rango = "B" + contador;
                            cuenta = BuscoCuenta(1, item.Solicitud.LugarEntrega, dc);
                            worksheet.Range[rango].Text = cuenta;

                            rango = "C" + contador;
                            worksheet.Range[rango].Text = "";

                            rango = "D" + contador;
                            concepto = BuscoConcepto(1, item.Solicitud.LugarEntrega) + " " + item.IdAlcancia + " " + item.NumeroTicket;
                            worksheet.Range[rango].Text = concepto;

                            rango = "E" + contador;
                            worksheet.Range[rango].Text = dc;

                            rango = "F" + contador;
                            worksheet.Range[rango].Number = item.MontoPesos;
                        }
                    }

                    if (item.MontoDolares != 0)
                    {
                        for (int x = 1; x <= 2; x++)
                        {
                            contador += 1;
                            rango = "A" + contador;
                            worksheet.Range[rango].Text = item.FechaSolicitud.ToString("dd/MM/yyyy");

                            rango = "B" + contador;
                            cuenta = BuscoCuenta(2, item.Solicitud.LugarEntrega, dc);//Falta ver como obtener donde se realizo 
                            worksheet.Range[rango].Text = cuenta;

                            rango = "C" + contador;
                            worksheet.Range[rango].Text = "";

                            rango = "D" + contador;
                            concepto = BuscoConcepto(2, item.Solicitud.LugarEntrega) + " " + item.IdAlcancia + " " + item.NumeroTicket;
                            worksheet.Range[rango].Text = concepto;

                            rango = "E" + contador;
                            if (x == 1)
                                worksheet.Range[rango].Text = "D";
                            else
                                worksheet.Range[rango].Text = "H";

                            rango = "F" + contador;
                            worksheet.Range[rango].Number = item.MontoDolares;
                        }
                    }
                    contador += 1;
                }
                foreach (AlcanciaExterna item in alcExt)
                {
                    if (item.MontoPesos != 0)
                    {
                        for (int x = 1; x <= 2; x++)
                        {
                            if (x == 1)
                                dc = "D";
                            else
                                dc = "H";

                            contador += 1;
                            rango = "A" + contador;
                            worksheet.Range[rango].Text = item.FechaDevolucion.ToString("dd/MM/yyyy");

                            rango = "B" + contador;
                            cuenta = BuscoCuenta(1, "MVD", dc);
                            worksheet.Range[rango].Text = cuenta;

                            rango = "C" + contador;
                            worksheet.Range[rango].Text = "";

                            rango = "D" + contador;
                            concepto = BuscoConcepto(1, "MVD") + " " + item.IdAlcExt + " " + item.NumeroTicket;
                            worksheet.Range[rango].Text = concepto;

                            rango = "E" + contador;
                            worksheet.Range[rango].Text = dc;

                            rango = "F" + contador;
                            worksheet.Range[rango].Number = item.MontoPesos;
                        }
                    }

                    if (item.MontoDolares != 0)
                    {
                        for (int x = 1; x <= 2; x++)
                        {
                            contador += 1;
                            rango = "A" + contador;
                            worksheet.Range[rango].Text = item.FechaDevolucion.ToString("dd/MM/yyyy");

                            rango = "B" + contador;
                            cuenta = BuscoCuenta(2, "MVD", dc);
                            worksheet.Range[rango].Text = cuenta;

                            rango = "C" + contador;
                            worksheet.Range[rango].Text = "";

                            rango = "D" + contador;
                            concepto = BuscoConcepto(2, "MVD") + " " + item.IdAlcExt + " " + item.NumeroTicket;
                            worksheet.Range[rango].Text = concepto;

                            rango = "E" + contador;
                            if (x == 1)
                                worksheet.Range[rango].Text = "D";
                            else
                                worksheet.Range[rango].Text = "H";

                            rango = "F" + contador;
                            worksheet.Range[rango].Number = item.MontoDolares;
                        }
                    }
                    contador += 1;

                }
                CambiarEstadoImpreso(alcExt, alcsol);

                //Guardamos el excel en MemoryStream
                MemoryStream stream = new MemoryStream();

                workbook.SaveAs(stream);

                //Grabamos la posicion como 0
                stream.Position = 0;

                //Descarga del Excel en el navegador
                FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/excel");
                fileStreamResult.FileDownloadName = "Alcancias.xlsx";
                return fileStreamResult;
            }
        }

        public List<AlcanciaSolicitud> TraerSolsExcel()
        {
            List<AlcanciaSolicitud> listSols = new List<AlcanciaSolicitud>();

            try
            {
                string parameters = "SolicitudAPI/GetListSolsExcel";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listSols = JsonConvert.DeserializeObject<List<AlcanciaSolicitud>>(json);
                    return listSols;
                }
                return listSols;
            }
            catch (Exception ex)
            {
                return listSols;
            }
        }

        public List<AlcanciaExterna> TraerAlcanciasExt()
        {
            List<AlcanciaExterna> lstAlc = new List<AlcanciaExterna>();

            try
            {
                string parameters = "AlcanciasExtAPI/GetListAlcExcel";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    lstAlc = JsonConvert.DeserializeObject<List<AlcanciaExterna>>(json);
                    return lstAlc;
                }
                return lstAlc;
            }
            catch (Exception ex)
            {
                return lstAlc;
            }
        }


        public void CambiarEstadoImpreso(List<AlcanciaExterna> lstAE, List<AlcanciaSolicitud> lstAS)
        {
            try
            {
                //Llamada 1
                string parameters = "AlcanciasExtAPI/CambiarEstadoImpreso";
                string json = JsonConvert.SerializeObject(lstAE, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = ApiClient.PostAsync(parameters, content);

                response.Wait();


                //Llamada 2
                parameters = "SolicitudAPI/CambiarEstadoImpreso";
                json = JsonConvert.SerializeObject(lstAS, Formatting.Indented);
                content = new StringContent(json, Encoding.UTF8, "application/json");
                response = ApiClient.PostAsync(parameters, content);

                response.Wait();
            }
            catch (Exception ex)
            {


            }
        }

        public Solicitud TraerPreSolicitud(int? id)
        {
            Solicitud sol = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "SolicitudAPI/GetPreSolicitud";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        sol = JsonConvert.DeserializeObject<Solicitud>(json);
                        return sol;
                    }
                    return sol;
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = ex.Message.ToString();
                    TempData["Status"] = false;
                    return sol;
                }
            }
            return sol;
        }

        public Solicitud TraerSolicitud(int? id)
        {
            Solicitud sol = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "SolicitudAPI/GetSolicitud";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        sol = JsonConvert.DeserializeObject<Solicitud>(json);
                        return sol;
                    }
                    return sol;
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = ex.Message.ToString();
                    TempData["Status"] = false;
                    return sol;
                }
            }
            return sol;
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


        public AlcanciaSolicitud TraerAlcSol(string idAlcancia, int id)
        {
            AlcanciaSolicitud alcSol = null;
            if (id != 0)
            {
                try
                {
                    string parameters = "SolicitudAPI/GetAlcanciaSolicitud";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?IdAlc=" + idAlcancia + "&IdSol=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        alcSol = JsonConvert.DeserializeObject<AlcanciaSolicitud>(json);
                        return alcSol;
                    }
                    return alcSol;
                }
                catch (Exception ex)
                {
                    return alcSol;
                }
            }
            return alcSol;
        }

        public Alcancia TraerAlcancia(string Id)
        {
            Alcancia alc = null;
            try
            {
                string parameters = "AlcanciaAPI/GetAlcancia";
                var response = ApiClient.GetAsync(parameters + "?Id=" + Id);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    alc = JsonConvert.DeserializeObject<Alcancia>(json);
                    return alc;
                }
                return alc;
            }
            catch
            {
                return alc;
            }

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
    }
}

