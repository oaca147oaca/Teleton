using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositorio;
using Repositorios;
using Microsoft.Extensions.Logging;
using Dominio;
using System.Net.Http;
using Newtonsoft.Json;
using Dominio.EntidadesDeNegocio;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using SelectPdf;
using System.Text;
using System.Net.Http.Json;

namespace TeletonMVC.Controllers
{
    public class AlcanciaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlcanciaController> _logger;
        private readonly MiAppContext dbContext;
        ICompositeViewEngine _compositeViewEngine;
        private readonly HttpClient ApiClient;

        public AlcanciaController(ILogger<AlcanciaController> logger, MiAppContext dbContext, IConfiguration configuration, ICompositeViewEngine compositeViewEngine)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            _compositeViewEngine = compositeViewEngine;

            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }

        public ActionResult AltaAlcancia()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        [HttpPost]
        public ActionResult AltaAlcancia(string IdAlcancia)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                int idValidacion = Int32.Parse(IdAlcancia);
                if (IdAlcancia != " " && idValidacion>0)
                {
                    try
                    {
                        //Genero la configuración para la api //aca lo busco para que aparezca un mensaje adecuado, aunque no deberia me parece 
                        Alcancia alc = new Alcancia();

                        string parameters = "AlcanciaAPI/AltaAlcancia";
                        var response = ApiClient.GetAsync(parameters + "?Codigo=" + IdAlcancia);

                        response.Wait();

                        if (response.Result.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            string json = taskContent.Result;
                            alc = JsonConvert.DeserializeObject<Alcancia>(json);//Si la api me devuelve un objeto es que ya existe la alcancia
                            if (alc != null)
                            {
                                ViewBag.Mensaje = "La alcancía ingresada ya existe";
                                ViewBag.Status = false;
                                return View();
                            }
                            ViewBag.Mensaje = "Se agregó correctamente la alcancia";
                            ViewBag.Status = true;
                            return View();
                        }
                        ViewBag.Mensaje = "Hubo un error, intente nuevamente";
                        ViewBag.Status = false;
                        return View();
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Mensaje = ex.Message.ToString();
                        return View();
                    }
                }
                else
                {
                    ViewBag.Mensaje = "Debe rellenar el campo y no se deben ingresar alcancías con código negativo.";
                    ViewBag.Status = false;
                    return View();
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        public ActionResult DevolucionAlcancia(string id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                ViewBag.AlcanciaId = id;
                int cantEntregadas = HttpContext.Session.GetInt32("CantEntregadas").GetValueOrDefault();
                ViewBag.CantEntregadas = cantEntregadas != 0 ? cantEntregadas : 0;
                List<string> alcanciasList = retornarListaAlcancias();
                ViewBag.ListaAlcancias = alcanciasList;
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }


        // [HttpPost]
        public async Task<IActionResult> DevolucionAlcanciaApiAsync(string IdAlcancia)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                if (IdAlcancia != " ")
                {
                    List<string> alcanciasList = retornarListaAlcancias();
                    bool devolvioTodas = true;
                    int solId = 0;
                    foreach (var item in alcanciasList)
                    {
                        try
                        {
                            string parameters = "AlcanciaAPI/Devolucion";
                            Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?idAlcancia=" + item);

                            respuesta.Wait();

                            if (respuesta.Result.IsSuccessStatusCode)
                            {
                                Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                                taskContent.Wait();
                                string json = taskContent.Result;
                                solId = JsonConvert.DeserializeObject<int>(json);

                            }
                            //poner esto en un else de if
                            //TempData["Mensaje"] = "Algo salió mal";
                            //TempData["Status"] = false;
                            //return RedirectToAction("DevolucionAlcancia");
                            //no tomo en cuenta los mensajes de error porque vienen de la api
                        }
                        catch (Exception ex)
                        {
                            devolvioTodas = false;
                            TempData["Mensaje"] = ex.Message.ToString();
                            TempData["Status"] = false;
                            return RedirectToAction("DevolucionAlcancia");
                        }
                    }
                    if (devolvioTodas)
                    {

                        //Seteo todos los parámetros del Session a "0"
                        alcanciasList = new List<string>();
                        byte[] alcanciasInBytes = alcanciasList.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                        HttpContext.Session.Set("ListaAlcancias", alcanciasInBytes);

                        //Imprimir(solId).GetAwaiter();

                        TempData["Mensaje"] = "Se hizo la devolución correctamente";
                        TempData["Status"] = true;

                        return await Imprimir(solId);

                    }
                }
                else
                {
                    TempData["Mensaje"] = "Debe rellenar el campo";
                    TempData["Status"] = false;
                    return RedirectToAction("DevolucionAlcancia");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        [HttpPost]
        public ActionResult AgregarAlcancia(int idAlcancia, bool isActive)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                string idAlc = idAlcancia.ToString();
                var alcanciasInByte = HttpContext.Session.Get("ListaAlcancias");
                List<string> alcanciasList = new List<string>();
                //string strByte = alcanciasInByte.ToString();

                if (isActive && idAlcancia == 0)
                {
                    if (alcanciasInByte == null)
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "No hay ninguna alcancía agregada.";
                        return RedirectToAction("DevolucionAlcancia");
                    }
                    else
                    {
                        List<string> alcanList = retornarListaAlcancias();
                        if (alcanList.Count == 0)
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = "Se eliminaron las alcancías agregadas, vuelva a ingresar.";
                            return RedirectToAction("DevolucionAlcancia");
                        }
                    }
                    return RedirectToAction("DevolucionAlcanciaApi", "Alcancia");
                }


                Alcancia alcEncontrada = TraerAlcancia(idAlc);
                if (alcEncontrada != null)
                {
                    //Acá chequeo que no haya sido devuelta ya
                    if (alcEncontrada.Habilitada.ToUpper() == "NO")
                    {
                        if (alcEncontrada.Estado.ToUpper() != "ENTREGADA")
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = "Esta Alcancía no fue entregada o ya fue devuelta.";
                            return RedirectToAction("DevolucionAlcancia", new { idAlc = idAlc });
                        }
                    }
                    else
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Esta Alcancía no fue entregada o ya fue devuelta.";
                        return RedirectToAction("DevolucionAlcancia", new { idAlc = idAlc });
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
                            return RedirectToAction("DevolucionAlcancia", new { msj = "Alcancía ya agregada." });
                        }
                    }

                    alcanciasList.Add(idAlc);

                    byte[] alcanciasInBytes = alcanciasList.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                    HttpContext.Session.Set("ListaAlcancias", alcanciasInBytes);
                    HttpContext.Session.SetInt32("CantEntregadas", alcanciasList.Count());

                    //ViewBag.ListaAlcancias = alcanciasList;

                    if (isActive)
                    {

                        TempData["Mensaje"] = "Alcancía agregada correctamente";
                        TempData["Status"] = true;
                        return RedirectToAction("DevolucionAlcanciaApi", "Alcancia");

                    }
                    TempData["Mensaje"] = "Alcancía agregada correctamente";
                    TempData["Status"] = true;
                    return RedirectToAction("DevolucionAlcancia");
                }
                else
                {
                    TempData["Mensaje"] = "Alcancía no encontrada (Id) o campo vacío.";
                    TempData["Status"] = false;
                    return RedirectToAction("DevolucionAlcancia");
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
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB")
            {
                List<string> alcancias = retornarListaAlcancias();

                alcancias.Remove(id);

                byte[] alcanciasInByte = alcancias.SelectMany(s => System.Text.Encoding.ASCII.GetBytes(s + "\0")).ToArray();
                HttpContext.Session.Set("ListaAlcancias", alcanciasInByte);
                HttpContext.Session.SetInt32("CantEntregadas", alcancias.Count());
                TempData["Status"] = true;
                return RedirectToAction("DevolucionAlcancia", new { msj = "Alcancía borrada correctamente." });
            }
            return RedirectToAction("Login", "Usuario");
        }


        public ActionResult Contabilizar()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Contabilidad" || rol == "Administrador")
            {
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        [HttpPost]
        public async Task<ActionResult> ContabilizarAsync(string IdAlcancia, int? MontoPesos, int? MontoDolares, int? NumeroTicket)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Contabilidad" || rol == "Administrador")
            {
                if (IdAlcancia== null)
                {
                    TempData["Mensaje"] = "Verifique los datos ingresados.";
                    TempData["Status"] = false;
                    return View();
                }
                bool alcVacia = false;
                if (MontoPesos == 0 && MontoDolares == 0 && NumeroTicket == 0)
                {
                    alcVacia = true;
                }

                if ((IdAlcancia.Trim() != "" && MontoPesos >= 0 && MontoDolares >= 0 && NumeroTicket > 0) || (alcVacia)) //el ticket hay que agregarlo a la vista
                {
                    try
                    {
                        Alcancia a = TraerAlcancia(IdAlcancia);
                        if (a != null && a.Estado != "RECIBIDA")
                        {

                            TempData["Mensaje"] = "La alcancía ingresada no fue recibida aún.";
                            TempData["Status"] = false;
                            return View();
                        }
                        AlcanciaSolicitud alcSol = new AlcanciaSolicitud()
                        {
                            IdAlcancia = IdAlcancia,
                            MontoPesos = (int)MontoPesos,
                            MontoDolares = (int)MontoDolares,
                            NumeroTicket = (int)NumeroTicket,
                            Impreso = "NO",
                            EsVacia = alcVacia
                        };



                        string parameters = "AlcanciaAPI/Contabilizar";
                        string json = JsonConvert.SerializeObject(alcSol, Formatting.Indented);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await ApiClient.PostAsync(parameters, content);

                        //string parameters = "AlcanciaAPI/Contabilizar";
                        //Task<HttpResponseMessage> response = ApiClient.PostAsJsonAsync(parameters, alcSol);

                        //+ "?IdAlcancia=" + IdAlcancia + "&MontoPesos=" + MontoPesos + "&MontoDolares=" + MontoDolares + "&NumeroTicket=" + NumeroTicket);

                        //response.Wait();

                        if (response.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            json = taskContent.Result;
                            alcSol = JsonConvert.DeserializeObject<AlcanciaSolicitud>(json);

                            TempData["Mensaje"] = "Se ingresaron los montos al sistema de forma correcta.";
                            TempData["Status"] = true;
                            return View();
                        }
                        TempData["Mensaje"] = "Algo salió mal.";
                        TempData["Status"] = false;
                        return View();
                    }
                    catch (Exception ex)
                    {
                        TempData["Mensaje"] = ex.Message.ToString();
                        TempData["Status"] = false;
                        return View();
                    }
                }
                TempData["Mensaje"] = "Verifique los datos ingresados.";
                TempData["Status"] = false;
                return View();
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
        public async Task<IActionResult> Imprimir(int Id)
        {
            if (Id == 0)
            {
                TempData["Mensaje"] = "Ocurrió un error, inténtelo más tarde.";
                TempData["Status"] = false;
                return RedirectToAction("DevolucionAlcancia");
            }
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioMvd" || rol == "VoluntarioFB" || rol == "Contabilidad")
            {

                Solicitud solicitud = TraerSolicitudEsp(Id);
                using (var stringWriter = new StringWriter())
                {
                    //esta linea sirve para cargar el modelo en la vista para que tenga todos los datos
                    var viewResult = _compositeViewEngine.FindView(ControllerContext, "_DevolucionPDF", false); //aca traigo la vista

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
                    string nombre = "Alcancia " + solicitud.Colaborador.Nombre + " " + DateTime.Now.ToString("dd/MM/yyyy") + ".pdf";

                    return File(pdfByte, "application/pdf", nombre);
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        public Solicitud TraerSolicitud(int id)
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

        public Solicitud ObtenerSolicitudEsp(int Id)
        {
            Solicitud sol = new Solicitud();
            try
            {
                string parameters = "SolicitudAPI/ObtenerSolEsp";
                var response = ApiClient.GetAsync(parameters + "?Id=" + Id);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {

                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    sol = JsonConvert.DeserializeObject<Solicitud>(json);
                    return sol;
                }
                return sol;
            }
            catch
            {
                return sol;
            }
        }



        public ActionResult Details(string Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                Alcancia alc = TraerAlcancia(Id);

                //var alc = dbContext.Alcancias.Where(x => x.IdAlcancia == Id).SingleOrDefault();
                return View(alc);
            }
            return RedirectToAction("Login", "Usuario");
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


        [HttpGet]
        public ActionResult MostrarAlcancias()
        {
            List<Alcancia> alcancias = TraerAlcancias();///PASAR A API
            return View(alcancias);
        }
        public List<Alcancia> TraerAlcancias()
        {
            List<Alcancia> alc = null;
            try
            {
                string parameters = "AlcanciaAPI/ListaAlcancia";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {

                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    alc = JsonConvert.DeserializeObject<List<Alcancia>>(json);
                    return alc;
                }
                return alc;
            }
            catch
            {
                return alc;
            }
        }

        public ActionResult Edit(string Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol.Length > 0 && rol != "VoluntarioMvd" && rol != "VoluntarioFB")
            {
                Alcancia alcancia = TraerAlcancia(Id);
                return View(alcancia);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: TipoColaboradorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Alcancia alc, int SelEst, int SelHab)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol.Length > 0 && rol != "VoluntarioMvd" && rol != "VoluntarioFB")
            {
                Alcancia alcancia = TraerAlcancia(alc.IdAlcancia);
                try
                {
                    if (SelEst != 0)
                    {
                        string estado = "";
                        switch (SelEst)
                        {
                            case 1:
                                estado = "Disponible";
                                break;
                            case 2:
                                estado = "Entregada";
                                break;
                            case 3:
                                estado = "Recibida";
                                break;
                            case 4:
                                estado = "Contabilizada";
                                break;
                            case 5:
                                estado = "Desvinculada";
                                break;
                        }
                        alcancia.Estado = estado.ToUpper();
                    }
                    if (SelHab != 0)
                    {
                        string habilitada = "";
                        switch (SelHab)
                        {
                            case 1:
                                habilitada = "SI";
                                break;
                            case 2:
                                habilitada = "NO";
                                break;
                        }
                        alcancia.Habilitada = habilitada;
                    }


                    string parameters = "AlcanciaAPI/Editar";
                    string json = JsonConvert.SerializeObject(alcancia, Formatting.Indented);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = ApiClient.PostAsync(parameters, content);
                    response.Wait();


                    if (response.Result.IsSuccessStatusCode)
                    {
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se realizó el cambio correctamente.";

                        return RedirectToAction("MostrarAlcancias");
                    }
                    else
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Ocurrió un error, no se encontró la alcancía";
                        return View(alc);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = "Error para ingresar los datos, verifique por favor otra vez.";
                    TempData["Status"] = false;
                    return View();
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        public ActionResult MostrarAlcanciasDeSol(int id)
        {

            List<AlcanciaSolicitud> alcancias = TraerListaAlcSol(id);
            HttpContext.Session.SetInt32("MostrarAlcanciasDeSol", id);
            return View(alcancias);
        }

        public List<AlcanciaSolicitud> TraerListaAlcSol(int id)
        {
            List<AlcanciaSolicitud> asol = new List<AlcanciaSolicitud>();

            try
            {
                string parameters = "AlcanciaAPI/TraerListaAlcSol";
                var response = ApiClient.GetAsync(parameters + "?Id=" + id);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {

                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    asol = JsonConvert.DeserializeObject<List<AlcanciaSolicitud>>(json);
                    return asol;
                }
                return asol;
            }
            catch
            {
                return asol;
            }
        }
    }
}
