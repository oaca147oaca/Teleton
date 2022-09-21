using Dominio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class ComentarioController : Controller
    {

        private readonly ILogger<ComentarioController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;


        public ComentarioController(ILogger<ComentarioController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }
        // GET: ComentarioController //Muestro las solicitudes de alcancias 
        public ActionResult Index()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                ViewBag.Sols = TraerSolicitudesComentario();
                return View();
            }
            return RedirectToAction("Login","Usuario");

        }
        public List<Solicitud> TraerSolicitudesComentario()
        {
            List<Solicitud> sols = new List<Solicitud>();
            try
            {
                string parameters = "SolicitudAPI/GetListSolsComentario";
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
                return sols;
            }
            catch (Exception ex)
            {
                return sols;
            }
        }

        // GET: ComentarioController/Details/5
        public ActionResult Comentarios(int id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                List<Comentario> comentario = TraerListComentarios(id);
                HttpContext.Session.SetInt32("SolicitudId", id);
                return View(comentario);
            }
            return RedirectToAction("Login","Usuario");
        }

        public List<Comentario> TraerListComentarios(int id)
        {
            List<Comentario> lstCom = new List<Comentario>();
            try
            {
                string parameters = "ComentarioAPI/GetComentarios";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters+ "?Id=" + id);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    lstCom = JsonConvert.DeserializeObject<List<Comentario>>(json);
                    return lstCom;
                }
                return lstCom;
            }
            catch (Exception ex)
            {
                return lstCom;
            }
        }





        // GET: ComentarioController/Create
        public ActionResult Create()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                return View();
            }
            return RedirectToAction("Login","Usuario");
        }

        // POST: ComentarioController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string TextoComentario)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    if(TextoComentario != " ")
                    {
                        int id = HttpContext.Session.GetInt32("SolicitudId").GetValueOrDefault();
                        if(id != 0)
                        {
                            string parameters = "ComentarioAPI/AltaComentario";
                            Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?idSol="+id + "&comentario="+ TextoComentario);//

                            respuesta.Wait();

                            if (respuesta.Result.IsSuccessStatusCode)
                            {
                                //HttpContext.Session.SetInt32("SolicitudId", 0);
                                TempData["Mensaje"] = "Se ha agregado el mensaje correctamente.";
                                TempData["Status"] = true;
                                return RedirectToAction("Comentarios", new { id = id});
                            }
                            TempData["Mensaje"] = "Ha ocurrido un error.";
                            TempData["Status"] = false;
                            return View();
                        }
                        else
                        {
                            TempData["Mensaje"] = "Ha ocurrido un error.";
                            TempData["Status"] = false;
                            return View();
                        }
                    }
                    else
                    {
                        TempData["Mensaje"] = "Debe ingresar un comentario.";
                        TempData["Status"] = false;
                        return View();
                    }
                }
                catch
                {
                    TempData["Mensaje"] = "Ha ocurrido un error.";
                    TempData["Status"] = false;
                    return View();
                }
            }
            return RedirectToAction("Login","Usuario");
        }

        // GET: ComentarioController/Delete/5
        public ActionResult Delete(int? id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Voluntariado" || rol == "Administrador")
            {
                try
                {
                    string parameters = "ComentarioAPI/EliminarComentario";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?idCom=" + id);

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Se ha eliminado correctamente el comentario.";
                        TempData["Status"] = true;
                        int idSol = HttpContext.Session.GetInt32("SolicitudId").GetValueOrDefault();
                        return RedirectToAction("Comentarios", new { id= idSol });
                    }
                    TempData["Mensaje"] = "Ha ocurrido un error.";
                    TempData["Status"] = false;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = "Ha ocurrido un error.";
                    TempData["Status"] = false;
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Login","Usuario");
        }

    }
}
