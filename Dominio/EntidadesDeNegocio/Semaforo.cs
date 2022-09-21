using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    public class Semaforo
    {
        [Column("SemaforoId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [Column("ColaboradorId")]
        [ForeignKey("ColaboradorId")]
        [Required]
        public int ColaboradorId { get; set; }
        public Colaborador Colaborador { get; set; }
        [Column("CantAlcanciasDevueltas")]
        [Required]
        public int CantAlcanciasDevueltas { get; set; }
        [Column("CantAlcanciasSinDevolver")]
        [Required]
        public int CantAlcanciasSinDevolver { get; set; }
        [Column("TotalCampanias")]
        [Required]
        public int TotalCampanias { get; set; }




    }
}
