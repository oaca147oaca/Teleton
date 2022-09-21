using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dominio.EntidadesDeNegocio;

namespace Dominio
{
    [Table("Solicitud")]
    public class Solicitud
    {
        [Column("SolicitudId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

         [ForeignKey("Campania")]
        public int CampaniaId { get; set; }
        public Campania Campania { get; set; }

        [ForeignKey("Colaborador")]
        public int ColaboradorId { get; set; }

        public Colaborador Colaborador { get; set; }

        [ForeignKey("Reponsable")]
        public int ResponsableId { get; set; }

        public Responsable Responsable { get; set; }

        [ForeignKey("Retira")]
        public int RetiraId { get; set; }

        public Retira Retira { get; set; }


        public int CantSolicitadas { get; set; }

        
        public int CantEntregadas { get; set; }

   
        public int CantDevueltas { get; set; }

        public string LugarEntrega { get; set; }



        public ICollection<Alcancia> Alcancias { get; set; }
        public List<AlcanciaSolicitud> AlcanciasSolicitudes { get; set; }

    }
}
