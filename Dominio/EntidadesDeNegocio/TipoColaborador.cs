using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    [Table("TipoColaboradores")]
    [Display(Name = "Tipo donante")]
    public class TipoColaborador
    {
        [Column("TipoId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [Column("Nombre")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}
