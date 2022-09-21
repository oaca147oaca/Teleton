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
using Microsoft.Extensions.Configuration;
using System.Text;
using Dominio.EntidadesDeNegocio;

namespace GestionAlcancias.Controllers
{
    
    public class UsuarioController : Controller
    {

        private readonly ILogger<UsuarioController> _logger;
        private readonly MiAppContext dbContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient ApiClient;

//Hola se actualizar 55555
        public UsuarioController(ILogger<UsuarioController> logger, MiAppContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            this.dbContext = dbContext;
            _configuration = configuration;
            this.ApiClient = new HttpClient();
            ApiClient.BaseAddress = new Uri(_configuration.GetSection("ApiUri")?.Value);
            string key = _configuration.GetSection("ApiKey")?.Value;
            ApiClient.DefaultRequestHeaders.Add("ApiKey", key);
        }
        public ActionResult Login()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol != null)
            {
                CierreSesion();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Nombre, string Contrasenia)
        {
            //Si el usuario ingresó los campos vacios 
            if (Nombre != null && Contrasenia != null)
            {
                try
                {
                    Usuario usr = new Usuario();

                    string parameters = "UsuarioAPI/Login";
                    Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Nombre=" + Nombre + "&Contrasenia=" + Contrasenia);//Intentar mandarlo por json en el body!!!!

                    respuesta.Wait();

                    if (respuesta.Result.IsSuccessStatusCode)
                    {
                        Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                        taskContent.Wait();
                        string json = taskContent.Result;
                        usr = JsonConvert.DeserializeObject<Usuario>(json);

                        HttpContext.Session.SetString("Name", Nombre);
                        HttpContext.Session.SetInt32("UserId", usr.Id);
                        HttpContext.Session.SetString("Rol", usr.Rol.Nombre);
                        TempData["Mensaje"] = null;
                        TempData["Status"] = null;
                        TempData["NombreUsuario"] = Nombre;
                        TempData["UserId"] = usr.Id;
                        if (usr.Rol.Nombre == "VoluntarioMvd" || usr.Rol.Nombre == "VoluntarioFB")
                        {
                            return RedirectToAction("IngresarDatosVoluntario", "Usuario");
                        }

                        return RedirectToAction("Index", "Graficas");
                    }
                    TempData["Status"] = false;
                    TempData["Mensaje"] = "El nombre de usuario y/o la contraseña son incorrectos";
                    return View();
                }
                catch (Exception ex)
                {
                    TempData["Mensaje"] = ex.Message.ToString();
                    TempData["Status"] = false;
                    return View();
                }
            }
            else
            {
                TempData["Status"] = false;
                TempData["Mensaje"] = "Debe completar todos los campos.";
                return View();
            }
        }

        public ActionResult Details(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                var usr = BuscarUsuarioId(Id);
                return View(usr);
            }
            return RedirectToAction("Login", "Usuario");
        }
        public ActionResult AuditoriaVoluntarios()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador" || rol == "Voluntariado")
            {
                var usr = TraerTodosVols();
                return View(usr);
            }
            return RedirectToAction("Login", "Usuario");
        }

        public async Task<ActionResult> MostrarTodosUsus()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                List<Usuario> usuarios = TraerTodosUsus();
                return View(usuarios);
            }
            return RedirectToAction("Login", "Usuario");
        }

        public ActionResult CambioContrasenia()
        {
            //Cambio de contraseña, todos los usuarios deben poder hacerlo menos los voluntarios
            string rol = HttpContext.Session.GetString("Rol");
            if (rol.Length > 0 && rol != "VoluntarioMvd" && rol != "VoluntarioFB")
            {
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }





        // POST: TipoColaboradorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambioContrasenia(string Contrasenia, string ContraseniaNueva, string ContraseniaConfir)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol.Length > 0 && rol != "VoluntarioMvd" && rol != "VoluntarioFB")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if(Contrasenia == ContraseniaNueva)
                {
                    ViewBag.Mensaje = "No se puede cambiar a la misma contraseña.";
                    ViewBag.Status = false;
                    return View();
                }

                Usuario usuBuscado = BuscarUsuarioId(userId);
                if (Contrasenia != null && ContraseniaNueva != null && ContraseniaConfir != null)
                {
                    if (usuBuscado.Contrasenia == Contrasenia)
                    {
                        try
                        {
                            if (ContraseniaNueva == ContraseniaConfir)
                            {
                                usuBuscado.Contrasenia = ContraseniaNueva;

                                string parameters = "UsuarioAPI/ActualizarContra";
                                string json = JsonConvert.SerializeObject(usuBuscado, Formatting.Indented);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");
                                var response = await ApiClient.PostAsync(parameters, content);

                                if (response.IsSuccessStatusCode)
                                {
                                    ViewBag.Mensaje = "Se realizó correctamente.";
                                    ViewBag.Status = true;
                                    return RedirectToAction("Index", "Graficas");
                                }
                                ViewBag.Mensaje = "Ocurrió un error, inténtelo más tarde.";
                                ViewBag.Status = false;
                                return View();
                            }
                            else
                            {
                                ViewBag.Mensaje = "Las contraseñas nuevas no coinciden.";
                                ViewBag.Status = false;
                                return View();
                            }

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
                        ViewBag.Mensaje = "Contraseña equivocada";
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
            return RedirectToAction("Login", "Usuario");
        }
        public ActionResult Edit(int? Id)
        {
            //Cambio de contraseña, todos los usuarios deben poder hacerlo menos los voluntarios
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                var usr = BuscarUsuarioId(Id);
                return View(usr);
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: TipoColaboradorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? Id, string contrasenia)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                if (contrasenia != "" && contrasenia!=null)
                {
                    try
                    {
                        var usr = BuscarUsuarioId(Id);
                        if (usr.Contrasenia != contrasenia)
                        {
                            usr.Contrasenia = contrasenia;
                            string parameters = "UsuarioAPI/ActualizarContra";
                            string json = JsonConvert.SerializeObject(usr, Formatting.Indented);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var respuesta = ApiClient.PostAsync(parameters, content);

                            respuesta.Wait();

                            if (respuesta.Result.IsSuccessStatusCode)
                            {

                                TempData["Mensaje"] = "Se realizó correctamente la edición.";
                                TempData["Status"] = true;
                                return RedirectToAction("MostrarTodosUsus");
                            }
                        }
                        else
                        {
                            ViewBag.Mensaje = "Es la misma contraseña";
                            ViewBag.Status = false;
                            return View();
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Mensaje"] = "Error para ingresar los datos, verifique por favor otra vez.";
                        TempData["Status"] = false;
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
            return RedirectToAction("Login", "Usuario");
        }
        public ActionResult Create()
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                ViewBag.Roles = TraerTodosRoles();
                return View();
            }
            return RedirectToAction("Login", "Usuario");
        }

        // POST: TipoColaboradorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string Nombre, int? SelRol)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                ViewBag.Roles = TraerTodosRoles();
                if ((Nombre != "" || Nombre!=null) && SelRol != null)
                {
                    try
                    {
                        var usuario = BuscarUsuarioNombre(Nombre);
                        if (usuario == null)
                        {
                            Usuario usr = new Usuario
                            {
                                Nombre = Nombre,
                                RolId = (int)SelRol,
                                Contrasenia = Nombre
                            };

                            string parameters = "UsuarioAPI/AltaUsuario";
                            string json = JsonConvert.SerializeObject(usr, Formatting.Indented);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var respuesta = ApiClient.PostAsync(parameters, content);

                            respuesta.Wait();

                            if (respuesta.Result.IsSuccessStatusCode)
                            {
                                TempData["Mensaje"] = "El usuario ha sido creado exitosamente.";
                                TempData["Status"] = true;
                                return RedirectToAction("MostrarTodosUsus");
                            }
                            TempData["Mensaje"] = "Ha ocurrido un error, inténtelo más tarde.";
                            TempData["Status"] = false;
                            return View();
                        }
                        else
                        {
                            TempData["Mensaje"] = "El usuario ya existe.";
                            TempData["Status"] = false;
                            return View();
                        }

                    }
                    catch (Exception ex)
                    {
                        TempData["Mensaje"] = ex.Message.ToString();
                        TempData["Status"] = false;
                        return View();
                    }
                }
                else
                {
                    TempData["Mensaje"] = "Debe completar los campos.";
                    TempData["Status"] = false;
                    return View();
                }
            }
            return RedirectToAction("Login", "Usuario");
        }

        public ActionResult CierreSesion()
        {
            var v = HttpContext.Session.GetInt32("VoluntarioId");
            var rol = HttpContext.Session.GetString("Rol");
            if (rol != null)
            {
                if (v != null)
                {
                    try
                    {
                        string parameters = "UsuarioAPI/CerrarSesion";
                        Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?vId=" + v);

                        respuesta.Wait();

                        if (respuesta.Result.IsSuccessStatusCode)
                        {
                            HttpContext.Session.Remove("Name");
                            HttpContext.Session.Remove("Rol");
                            HttpContext.Session.Remove("VoluntarioId");
                            return RedirectToAction("Login", "Usuario");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Mensaje"] = ex.Message.ToString();
                        TempData["Status"] = false;
                        return View();
                    }
                }
                HttpContext.Session.Remove("Name");
                HttpContext.Session.Remove("Rol");
            }
            return RedirectToAction("Login", "Usuario");
        }

        //Aca empieza lo de voluntario
        public ActionResult IngresarDatosVoluntario(string msj)
        {
            ViewBag.Mensaje = msj;
            return View();
        }

        public ActionResult Delete(int? Id)
        {
            string rol = HttpContext.Session.GetString("Rol");
            if (rol == "Administrador")
            {
                string parameters = "UsuarioAPI/DeleteUsuario";
                Task<HttpResponseMessage> response = ApiClient.GetAsync(parameters + "?Id=" + Id);


                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    ViewBag.Status = true;
                    ViewBag.Mensaje = "Se ha editado correctamente.";
                    return RedirectToAction("MostrarTodosUsus");
                }
                ViewBag.Status = false;
                ViewBag.Mensaje = "Algo ha salido mal, inténtelo más tarde.";
                return RedirectToAction("MostrarTodosUsus");
            }
            return RedirectToAction("Login", "Usuario");
        }


        [HttpPost]
        public async Task<ActionResult> IngresarDatosVoluntario(Voluntario v)
        {

            if (v.Nombre != "" && v.Cedula != "")
            {
                if (v.Cedula.Length <= 8 && v.Cedula.Length >= 7)
                {

                    try
                    {
                        v.Inicio = DateTime.Now;
                        string parameters = "UsuarioAPI/AltaVoluntario";
                        string json = JsonConvert.SerializeObject(v, Formatting.Indented);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await ApiClient.PostAsync(parameters, content);

                        if (response.IsSuccessStatusCode)
                        {
                            Task<string> taskContent = response.Content.ReadAsStringAsync();
                            taskContent.Wait();
                            json = taskContent.Result;
                            v = JsonConvert.DeserializeObject<Voluntario>(json);

                            HttpContext.Session.SetInt32("VoluntarioId", v.Id);//Este Id lo guardo para que al cerrar sesión, se guarde DateTime de la acción
                            return RedirectToAction("MostrarTodasSols", "Solicitud");
                        }
                        string mensaje = "Hubo un error, intente nuevamente.";
                        ViewBag.Status = false;
                        return RedirectToAction("IngresarDatosVoluntario", new { mensaje });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Status = false;
                        ViewBag.Mensaje = ex.Message.ToString();
                        return View();
                    }
                }
                else
                {
                    ViewBag.Status = false;
                    ViewBag.Mensaje = "La cédula deben ser solo números y 7 u 8 dígitos.";
                    return View();

                }
            }
            else
            {
                ViewBag.Status = false;
                ViewBag.Mensaje = "Debe completar todos los campos.";
                return View();
            }
        }



        public Usuario BuscarUsuarioNombre(string Nombre)
        {
            try
            {
                Usuario retorno = new Usuario();
                string parameters = "UsuarioAPI/BuscarUsuarioNombre";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?Nombre=" + Nombre);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    retorno = JsonConvert.DeserializeObject<Usuario>(json);
                    return retorno;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public Usuario BuscarUsuarioId(int? userId)
        {
            try
            {
                Usuario retorno = new Usuario();
                string parameters = "UsuarioAPI/BuscarUsuario";
                Task<HttpResponseMessage> respuesta = ApiClient.GetAsync(parameters + "?UserId=" + userId);

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    retorno = JsonConvert.DeserializeObject<Usuario>(json);
                    return retorno;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public List<Rol> TraerTodosRoles()
        {
            var listRols = new List<Rol>();
            try
            {
                string parameters = "UsuarioAPI/GetListRoles";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listRols = JsonConvert.DeserializeObject<List<Rol>>(json);
                    ViewBag.Status = true;
                    return listRols;
                }
                return listRols;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return listRols;
            }
        }

        public List<Usuario> TraerTodosUsus()
        {
            var listUsus = new List<Usuario>();
            try
            {
                string parameters = "UsuarioAPI/GetListUsus";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listUsus = JsonConvert.DeserializeObject<List<Usuario>>(json);
                    ViewBag.Status = true;
                    return listUsus;
                }
                return listUsus;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return listUsus;
            }
        }


        public List<Voluntario> TraerTodosVols()
        {
            var listUsus = new List<Voluntario>();
            try
            {
                string parameters = "UsuarioAPI/GetListVols";
                var response = ApiClient.GetAsync(parameters);

                response.Wait();

                if (response.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = response.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    listUsus = JsonConvert.DeserializeObject<List<Voluntario>>(json);
                    ViewBag.Status = true;
                    return listUsus;
                }
                return listUsus;
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message.ToString();
                return listUsus;
            }
        }

    }
}