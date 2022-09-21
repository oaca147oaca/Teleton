using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositorio;
using Microsoft.EntityFrameworkCore;
using Dominio.Interface;
using Repositorios;
using Newtonsoft.Json;

namespace TeletonMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }




        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(4);
            });

            services.AddControllersWithViews();
            services.AddScoped<IRepositorioSolicitud, RepositorioSolicitud>(); //esta linea sirve para la impresion de datos pdf
            string apiKey = Configuration.GetSection("AppSettings:API_KEY")?.Value;
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
           

            services.AddDbContext<MiAppContext>
           (o => o.UseSqlServer(Configuration.
            GetConnectionString("DeveloperDatabase")));

            services.AddDbContext<MiAppContext>(
             options => options.UseSqlServer("name=ConnectionStrings:DeveloperDatabase"));
            
            services.AddControllers();

            services.AddMvc();
            services.AddSession();
            services.AddHttpContextAccessor();

            services.AddDistributedMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            
            app.UseSession();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Usuario}/{action=Login}/{id?}");
            });
        }
    }
}
