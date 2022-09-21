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
using System.Text;
using System.Threading.Tasks;

namespace TeletonMVC.Controllers
{
    public class SemaforoController : Controller
    {
        private readonly ILogger<SemaforoController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;



        public SemaforoController(ILogger<SemaforoController> logger, MiAppContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }

        // GET: SemaforoController
        public ActionResult Index()
        {
            var lista = TraerTodosSemaforos();
            return View(lista);
        }


        // GET: SemaforoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SemaforoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, int CantAlcanciasSinDevolver)
        {
            try
            {
                if (CantAlcanciasSinDevolver >= 0)
                {
                    var semaforo = BuscarSemaforoId(id);
                    if (semaforo != null)
                    {
                        semaforo.CantAlcanciasSinDevolver = CantAlcanciasSinDevolver;

                        string parameters = "ComentarioAPI/EditSemaforo";
                        string json = JsonConvert.SerializeObject(semaforo, Formatting.Indented);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await ApiClient.PutAsync(parameters, content);

                        if (response.IsSuccessStatusCode)
                        {
                            TempData["Mensaje"] = "Se realizó el cambio correctamente.";
                            TempData["Status"] = true;
                            return RedirectToAction("Index");
                        }
                        ViewBag.Mensaje = "Ocurrió un error con el Semáforo seleccionado.";
                        ViewBag.Status = false;
                        return View();
                    }
                    else
                    {
                        ViewBag.Mensaje = "Ocurrió un error con el Semáforo seleccionado.";
                        ViewBag.Status = false;
                        return View();
                    }
                }
                else
                {
                    ViewBag.Mensaje = "La cantidad no puede ser menor que 0.";
                    ViewBag.Status = false;
                    return View();
                }
            }
            catch
            {
                ViewBag.Mensaje = "Ocurrió un error.";
                ViewBag.Status = false;
                return View();
            }
        }

        public Semaforo BuscarSemaforoId(int? Id)
        {
            try
            {
                Semaforo retorno = new Semaforo();
                string parameters = "ComentarioAPI/BuscarSemaforo";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Id=" + Id);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    retorno = JsonConvert.DeserializeObject<Semaforo>(json);
                    return retorno;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public List<Semaforo> TraerTodosSemaforos()
        {
            var listSemaforos = new List<Semaforo>();
            try
            {
                string parameters = "ComentarioAPI/GetListSemaforos";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listSemaforos = JsonConvert.DeserializeObject<List<Semaforo>>(json);
                    ViewBag.Status = true;
                    return listSemaforos;
                }
                return listSemaforos;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return listSemaforos;
            }
        }

    }
}
