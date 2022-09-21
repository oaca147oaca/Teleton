using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    public class Comentario
    {
        [Column("ComentarioId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int IdComentario { get; set; }

        [Column("SolicitudId")]
        [ForeignKey("SolicitudId")]
        [Required]
        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; }

        [Column("Fecha")]
        [Required]
        public DateTime Fecha { get; set; }
        [Column("Texto")]
        [Required]
        [StringLength(250)]
        public string TextoComentario { get; set; }

    }
}
