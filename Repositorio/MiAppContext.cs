using Dominio;
using Dominio.EntidadesDeNegocio;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Repositorio
{
    public class MiAppContext : DbContext
    {
        public MiAppContext()
        {
        }

        public MiAppContext(DbContextOptions<MiAppContext> options) : base(options)
        {

        }
        public DbSet<Alcancia> Alcancias { get; set; }
        public DbSet<Campania> Campanias { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Responsable> Responsables { get; set; }
        public DbSet<Retira> Retiran { get; set; }
        public DbSet<Semaforo> Semaforos { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<TipoColaborador> TipoColaboradores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Voluntario> Voluntarios { get; set; }
        public DbSet<AlcanciaSolicitud> AlcanciaSolicitudes { get; set; }
        public DbSet<AlcanciaExterna> AlcanciasExternas { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        {


            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer(@"data source = DESKTOP-7FLROKS\SQLEXPRESS; initial catalog = TeletonBD; integrated security = true");
                //optionsBuilder.UseSqlServer(@"data source = DESKTOP-B997NFV\SQLEXPRESS; initial catalog = TeletonBD; integrated security = true");
                //optionsBuilder.UseSqlServer(@"Data Source=tcp:teletonmvcdbserver.database.windows.net,1433;Initial Catalog=TeletonMVC_db;User Id=TeletonMVCBD@teletonmvcdbserver;Password=Teleton_2022");
                optionsBuilder.UseSqlServer(@"data source = localhost\\MSSQLSERVER01; initial catalog = TeletonBD; integrated security = true");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Alcancia>(a => a.Property(pt => pt.EsHabilitada));
            modelBuilder.Entity<Alcancia>()
            .HasMany(p => p.Solicitudes)
            .WithMany(p => p.Alcancias)
            .UsingEntity<AlcanciaSolicitud>(
                j => j
                    .HasOne(pt => pt.Solicitud)
                    .WithMany(t => t.AlcanciasSolicitudes)
                    .HasForeignKey(pt => pt.IdSolicitud),
                j => j
                    .HasOne(pt => pt.Alcancia)
                    .WithMany(p => p.AlcanciasSolicitudes)
                    .HasForeignKey(pt => pt.IdAlcancia),
                j =>
                {
                    j.Property(pt => pt.FechaSolicitud);
                    j.Property(pt => pt.NumeroTicket);
                    j.Property(pt => pt.MontoDolares);
                    j.Property(pt => pt.MontoPesos);
                    j.Property(pt => pt.Impreso);
                    j.Property(pt => pt.EsVacia);
                    j.HasKey(t => new { t.IdAlcancia, t.IdSolicitud });
                });

        }
    }
}

