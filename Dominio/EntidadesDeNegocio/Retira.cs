using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    public class Retira
    {
        [Column("RetiraId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [Column("Nombre")]
        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [Column("Cedula")]
        [Required]
        [StringLength(20)]
        public string Cedula { get; set; }
        [Column("Telefono")]
        [Required]
        [StringLength(30)]
        public string Telefono { get; set; }

    }
}
