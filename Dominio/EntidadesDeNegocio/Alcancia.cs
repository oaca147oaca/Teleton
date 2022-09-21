using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Dominio.EntidadesDeNegocio;

namespace Dominio
{
    [Table("Alcancias")]
    public class Alcancia
    {
        [Column("AlcanciaId")]
        [Key]
        [Required]
        [StringLength(300)]
        public string IdAlcancia { get; set; }

        [Column("Estado")]
        [Required]
        [StringLength(50)]
        public string Estado { get; set; }

        [Column("FechaAlta")]
        [Required]
        public DateTime FechaAlta { get; set; }

        [Column("EsHabilitada")]
        [Required]
        [MaxLength(2)]
        public string Habilitada { get; set; }

        public ICollection<Solicitud> Solicitudes { get; set; }

        public List<AlcanciaSolicitud> AlcanciasSolicitudes { get; set; }
    }
}
