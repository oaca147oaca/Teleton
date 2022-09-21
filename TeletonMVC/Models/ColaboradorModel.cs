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
    public class ColaboradorModel
    {
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; }
        
        [Display(Name = "Dirección")]
        public string Dirección { get; set; }

        [Display(Name = "Departamento")]
        public SelectList Departamento { get; set; }

        [Display(Name = "Localidad")]
        public string Localidad { get; set; }

        [Display(Name = "Tipo de Colaborador")]
        public SelectList TipoCol { get; set; }
        
        public int TipoColId { get; set; }

        public ColaboradorModel()
        {
            using (MiAppContext dbContext = new MiAppContext())
            {

                List<TipoColaborador> tiposCols = dbContext.TipoColaboradores.ToList();
                TipoColaborador tipoDefault = new TipoColaborador()
                {
                    Id = -1,
                    Nombre = "Seleccione un tipo"
                };

                tiposCols.Insert(0, tipoDefault);

                TipoCol = new SelectList(tiposCols, "Id", "Nombre");


                var listaDepartamentos = new[]
             {
                new{id = "-1", fase = "Seleccione un Departamento"},
                new{id = "1", fase ="Artigas"},
                new{id = "2", fase ="Canelones"},
                new{id = "3", fase ="Cerro largo"},
                new{id = "4", fase ="Colonia"},
                new{id = "5", fase ="Durazno"},
                new{id = "6", fase ="Flores"},
                new{id = "7", fase ="Florida"},
                new{id = "8", fase ="Lavalleja"},
                new{id = "9", fase ="Maldonado"},
                new{id = "10", fase ="Montevideo"},
                new{id = "11", fase ="Paysandú"},
                new{id = "12", fase ="Rio Negro"},
                new{id = "13", fase ="Rivera"},
                new{id = "14", fase ="Rocha"},
                new{id = "15", fase ="Salto"},
                new{id = "16", fase ="San José"},
                new{id = "17", fase ="Soriano"},
                new{id = "18", fase ="Tacuarembó"},
                new{id = "19", fase ="Treinta y Tres"},

            }.ToList();

                Departamento = new SelectList(listaDepartamentos);
            }

        }







    }
}
