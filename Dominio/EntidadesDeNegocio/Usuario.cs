using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    [Table("Usuarios")]
    public class Usuario
    {

        [Column("UsuarioId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Column("Nombre")]
        [StringLength(20)]
        [Required]
        public string Nombre { get; set; }

        [ForeignKey("Rol")]
        [Required]
        public int RolId{ get; set; }
        public Rol Rol { get; set; }
        
        
        [Column("Contrasenia")]
        [Required]
        [StringLength(20)]
        public string Contrasenia { get; set; }



    }

}
