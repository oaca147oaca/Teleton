using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    [Display(Name = "Donante")]
    public class Colaborador
    {
        [Column("ColaboradorId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column("Nombre")]
        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [Column("RazonSocial")]
        [Required]
        [StringLength(60)]
        public string RazonSocial { get; set; }
        [Column("Direccion")]
        [Required]
        [StringLength(60)]
        public string Direccion { get; set; }
        [Column("Departamento")]
        [Required]
        [StringLength(60)]
        public string Departamento { get; set; }
        [Column("Localidad")]
        [Required]
        [StringLength(60)]
        public string Localidad { get; set; }

        [ForeignKey("TipoColaborador")]
        public int TipoColId { get; set; }
        public TipoColaborador TipoCol { get; set; }


    }

}
