using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    [Table("Roles")]
    public class Rol
    {
        [Column("RolId")]
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
