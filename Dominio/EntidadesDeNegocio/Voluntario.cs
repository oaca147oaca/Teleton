using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio.EntidadesDeNegocio
{
    [Table("Voluntarios")]
    public class Voluntario
    {
        [Column("VoluntarioId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column("Nombre")]
        [StringLength(20)]
        [Required]
        public string Nombre { get; set; }



        [Column("Cedula")]
        [Required]
        [StringLength(20)]
        public string Cedula { get; set; }


        [Required]
        public DateTime Inicio { get; set; }
        [Required]
        public DateTime Fin { get; set; }

    }
}
