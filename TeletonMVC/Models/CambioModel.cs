using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TeletonMVC.Models
{
    public class CambioModel
    {
        [Required]
        public String Contrasenia { get; set; }
        [Required]
        public String ContraseniaNueva { get; set; }
        [Required]
        public String ContraseniaConfir { get; set; }
    }
}
