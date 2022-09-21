using Microsoft.AspNetCore.Mvc.Rendering;
using Repositorio;
using Dominio;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeletonMVC.Models
{
    public class SolicitudModel 
    {
        [Display(Name = "Colaborador")]
        public SelectList Colaboradores { get; set; }
        public int ColaboradorId { get; set; }


        [Display(Name = "Responsable")]
        public SelectList Responsables { get; set; }
        public int ResponsableId { get; set; }


        [Display(Name = "Retira")]
        public SelectList Retiran{ get; set; }
        public int RetiraId { get; set; }


        [Display(Name = "Cant. Solicitadas")]
        public int CantSol { get; set; }
        

        [Display(Name = "Cant. Entregadas")]
        public int CantEnt { get; set; }
        

        [Display(Name = "Cant. Devueltas")]
        public int CantDev { get; set; }


        public SolicitudModel()
        {
            using (MiAppContext dbContext = new MiAppContext())
            {

                List<Colaborador> cols = dbContext.Colaboradores.ToList();
                //Colaborador tipoDefault = new Colaborador()
                //{
                //    Id = -1,
                //    Nombre = "Seleccione un tipo"
                //};

                //tiposCols.Insert(0, tipoDefault);
                Colaboradores = new SelectList(cols);


                List<Responsable> resps= dbContext.Responsables.ToList();
                Responsables = new SelectList(resps);

                List<Retira> retiran= dbContext.Retiran.ToList();
                Retiran = new SelectList(retiran);







            //    var listaDepartamentos = new[]
            // {
            //    new{id = "-1", fase = "Seleccione un Departamento"},
            //    new{id = "1", fase ="Artigas"},
            //    new{id = "2", fase ="Canelones"},
            //    new{id = "3", fase ="Cerro largo"},
            //    new{id = "4", fase ="Colonia"},
            //    new{id = "5", fase ="Durazno"},
            //    new{id = "6", fase ="Flores"},
            //    new{id = "7", fase ="Florida"},
            //    new{id = "8", fase ="Lavalleja"},
            //    new{id = "9", fase ="Maldonado"},
            //    new{id = "10", fase ="Montevideo"},
            //    new{id = "11", fase ="Paysandú"},
            //    new{id = "12", fase ="Rio Negro"},
            //    new{id = "13", fase ="Rivera"},
            //    new{id = "14", fase ="Rocha"},
            //    new{id = "15", fase ="Salto"},
            //    new{id = "16", fase ="San José"},
            //    new{id = "17", fase ="Soriano"},
            //    new{id = "18", fase ="Tacuarembó"},
            //    new{id = "19", fase ="Treinta y Tres"},

            //}.ToList();

            //    Departamento = new SelectList(listaDepartamentos);
            }

        }


    }
}
