using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dominio.EntidadesDeNegocio;
using Repositorio;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Dominio;
using System.Text;
using TeletonMVC.Models;
using System.IO;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SelectPdf;

namespace TeletonMVC.Controllers
{
    public class AlcanciaExternasController : Controller
    {
        private readonly MiAppContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;
        ICompositeViewEngine _compositeViewEngine;
        private string key;

        public AlcanciaExternasController(MiAppContext context, IConfiguration configuration, ICompositeViewEngine compositeViewEngine)
        {
            _context = context;
            _configuration = configuration;
            _compositeViewEngine = compositeViewEngine;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }

        // GET: AlcanciaExternas
        public async Task<IActionResult> Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            {
                List<AlcanciaExterna> lstAlcExt = new List<AlcanciaExterna>();
                try
                {
                    string parameters = "AlcanciasExtAPI/GetListAlc";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        lstAlcExt = JsonConvert.DeserializeObject<List<AlcanciaExterna>>(json);

                        return View(lstAlcExt);
                    }

                    return View(lstAlcExt);
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = ex.Message.ToString();
                    TempData["Status"] = false;
                    return View(lstAlcExt);
                }
            }
            else
            {
                return RedirectToAction("Login", "Usuario");
            }
        }

        // GET: AlcanciaExternas/Details/5
        public ActionResult Details(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            { 
                AlcanciaExterna ae = GetAlcanciaExt(id);
                if (ae == null)
                {
                    TempData["Mensaje"] = "Ha ocurrido un error, intentelo mas tarde";
                    TempData["Status"] = false;
                    return RedirectToAction("Index");
                }
                return View(ae); 
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: AlcanciaExternas/Create
        public IActionResult Create()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd" || rol == "Contabilidad")
            {
                ViewBag.Rol = rol;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Usuario");
            }
        }

        // POST: AlcanciaExternas/Create
        [HttpPost]
        public async Task<ActionResult> CreateAsync(AlcExternaModel alcanciaExterna)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd" || rol == "Contabilidad")
            {
                if (alcanciaExterna.Nombre == null && alcanciaExterna.Telefono == null && alcanciaExterna.Cantidad <= 0)
                {
                    TempData["Mensaje"] = "Revise los datos ingresados, uno de los campos esta vacío o es menor a 0";
                    TempData["Status"] = false;

                    return View(alcanciaExterna);
                }

                string parametros = "CampaniaAPI/GetCampaniaActual";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parametros);
                Campania camp = Campania.TraerCampaniaActual(key,respuesta);

                if (camp == null)
                {
                    if (rol == "Administrador")
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Debe registrarse una campaña primero.";
                        return RedirectToAction("AltaCampania", "Campania");
                    }
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Para tener acceso debe existir una campaña actualmente, llame a un supervisor para continuar.";
                    return RedirectToAction("Index");
                }

                try
                {

                    string parameters = "AlcanciasExtAPI/AltaAlcExt";
                    string json = JsonConvert.SerializeObject(alcanciaExterna, Formatting.Indented);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await ApiClient.PostAsync(parameters, content);


                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Se agregó correctamente la alcancía externa.";
                        TempData["Status"] = true;
                        //await Imprimir(alcanciaExterna.Nombre);
                        return await Imprimir(alcanciaExterna.Nombre);
                    }
                    else
                    {
                        TempData["Mensaje"] = "Ha ocurrido un error, inténtelo más tarde.";
                        TempData["Status"] = false;
                        return View(alcanciaExterna);
                    }
                }
                catch (Exception ex)
                {

                    TempData["Mensaje"] = ex.Message.ToString();
                    TempData["Status"] = false;

                    return View(alcanciaExterna);
                }
            }
            else
            {
                return RedirectToAction("Login", "Usuario");
            }
        }



        // GET: AlcanciaExternas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            { 
                if (id <= 0)
                {
                    return NotFound();
                }

                AlcanciaExterna ae = GetAlcanciaExt(id);
                if (ae == null)
                {
                    TempData["Mensaje"] = "Ha ocurrido un error, intentelo mas tarde";
                    TempData["Status"] = false;
                    return RedirectToAction("Index");
                }
                //ViewBag.CampaniaId = ae.Campania.IdCampania;
                return View(ae);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: AlcanciaExternas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AlcanciaExterna alcanciaExterna, int SelImp)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            { 
            
            
                if (alcanciaExterna.MontoDolares>=0 && alcanciaExterna.MontoDolares>=0 && alcanciaExterna.Telefono!=null )
                {
                    if (SelImp == 0)
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Debe seleccionar una opción.";
                        return RedirectToAction("Edit",new { id = alcanciaExterna.IdAlcExt });
                    }
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
                    alcanciaExterna.Impreso = impreso;

                    try
                    {
                        alcanciaExterna.Nombre = "Relleno";
                        alcanciaExterna.CampaniaId = 1;

                        string parameters = "AlcanciasExtAPI/EditarAlc";

                        string json = JsonConvert.SerializeObject(alcanciaExterna, Formatting.Indented);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = ApiClient.PostAsync(parameters, content);
                        response.Wait();


                        if (response.Result.IsSuccessStatusCode)
                        {
                            TempData["Status"] = true;
                            TempData["Mensaje"] = "Se realizó el cambio correctamente.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            TempData["Status"] = false;
                            TempData["Mensaje"] = "Ocurrió un error, inténtelo mas tarde.";
                            return RedirectToAction("Edit", new { id = alcanciaExterna.IdAlcExt });
                        }
                    }
                    catch (Exception ex)
                    {

                        TempData["Mensaje"] = "Error para ingresar los datos, verifique por favor otra vez.";
                        TempData["Status"] = false;
                        return RedirectToAction("Edit", new { id = alcanciaExterna.IdAlcExt });
                    }
                }
                else
                {
                    TempData["Mensaje"] = "Debe completar los campos";
                    TempData["Status"] = false;
                    return RedirectToAction("Edit", new { id = alcanciaExterna.IdAlcExt });
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        // GET: AlcanciaExternas/Delete/5
        public async Task<IActionResult> Delete(int id)
        {

            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "Contabilidad")
            { 
                try
                {
                    string parameters = "AlcanciasExtAPI/EliminarAlc";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + id);
                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        TempData["Status"] = true;
                        TempData["Mensaje"] = "Se eliminó correctamente la alcancía " + id + ".";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["Status"] = false;
                        TempData["Mensaje"] = "Ocurrió un error, inténtelo más tarde.";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "Ocurrió un error, inténtelo más tarde.";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        public AlcanciaExterna GetAlcanciaExt(int Id)
        {
            AlcanciaExterna ae = new AlcanciaExterna();
            try
            {
                string parameters = "AlcanciasExtAPI/GetAlcExt";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + Id);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    ae = JsonConvert.DeserializeObject<AlcanciaExterna>(json);
                    return ae;
                }
                return ae;
            }
            catch (Exception ex)
            {
                return ae;
            }
        }

        public async Task<ActionResult> Imprimir(string Nombre)
        {
            //URL de donde saque la docu https://selectpdf.com/html-to-pdf/docs/html/Index.htm
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador" || rol == "VoluntarioFB" || rol == "VoluntarioMvd" || rol == "Contabilidad")
            {
                ViewBag.alc = "";
                List<AlcanciaExterna> alcanciaExt = ObtenerAlcancias(Nombre);

                //apartir de ahora todo en otro metodo async
                using (var stringWriter = new StringWriter())
                {
                    //esta linea sirve para cargar el modelo en la vista para que tenga todos los datos
                    var viewResult = _compositeViewEngine.FindView(ControllerContext, "_DevolucionAlcExterna", false); //aca traigo la vista
                                                                                                                       // await this.RenderView("_SolicitudPDF", model:solicitud);
                    if (viewResult.View == null) //verifico que no sea nula
                    {
                        throw new ArgumentNullException("View cannot be found");
                    }

                    var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

                    TempData["alcanciaExt"] = alcanciaExt;


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

                    string nombre = "Alcancia " + alcanciaExt[0].Nombre + " " + DateTime.Now.ToString("dd/MM/yyyy") + ".pdf";
                    return File(pdfByte, "application/pdf",nombre);
                }
            }
            return RedirectToAction("Login", "Usuario");
        }


        public List<AlcanciaExterna> ObtenerAlcancias(string Nombre)
        {
            List<AlcanciaExterna> ae = new List<AlcanciaExterna>();
            try
            {
                string parameters = "AlcanciasExtAPI/ObtenerAlcancias";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Nombre=" + Nombre);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    ae = JsonConvert.DeserializeObject<List<AlcanciaExterna>>(json);
                    return ae;
                }
                return ae;
            }
            catch (Exception ex)
            {
                return ae;
            }
        }

    }
}
