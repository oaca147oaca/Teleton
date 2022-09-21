using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.EntidadesDeNegocio
{
    [Table("AlcanciasExternas")]
    public class AlcanciaExterna
    {

        [Display(Name = "Id Alcancía")]
        [Column("AlcExtId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int IdAlcExt { get; set; }

        [ForeignKey("Campania")]
        public int CampaniaId { get; set; }
        public Campania Campania { get; set; }

        [Column("Nombre")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Teléfono")]
        [Column("Telefono")]
        [StringLength(50)]
        public string Telefono { get; set; }

        [Display(Name = "Fecha devolución")]
        [Column("FechaDevolucion")]
        [Required]
        public DateTime FechaDevolucion { get; set; }


        [Display(Name = "Monto en pesos")]
        public int MontoPesos { get; set; }

        [Display(Name = "Monto en dólares")]
        public int MontoDolares { get; set; }

        [Display(Name = "Recibo")]
        public int NumeroTicket { get; set; }

        public string Impreso { get; set; }


    }
}
