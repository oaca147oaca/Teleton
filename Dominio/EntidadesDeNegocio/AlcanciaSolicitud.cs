using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.EntidadesDeNegocio
{
    public class AlcanciaSolicitud
    {

        public string IdAlcancia { get; set; }
        public Alcancia Alcancia { get; set; }

        public int IdSolicitud { get; set; }
        public Solicitud Solicitud { get; set; }
        
        [Required(AllowEmptyStrings = true)]
        public DateTime FechaSolicitud { get; set; }

        [Required(AllowEmptyStrings = true)]
        public int NumeroTicket { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Impreso { get; set; }

        [Required(AllowEmptyStrings = true)]
        public int MontoPesos { get; set; }

        [Required(AllowEmptyStrings = true)]
        public int MontoDolares { get; set; }

        [Required(AllowEmptyStrings = true)]
        public bool EsVacia{ get; set; }

    }
}
