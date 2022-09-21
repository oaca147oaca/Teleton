using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeletonMVC.Models
{
    public class AlcExternaModel
    {
        [Required]
        public String Nombre { get; set; }
        [Required]
        public String Telefono { get; set; }
        [Required]
        public int Cantidad { get; set; }
    }
}
