using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Dominio.EntidadesDeNegocio;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dominio
{
    [Table("Campanias")]
    public class Campania
    {
        [Column("CampaniaId")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int IdCampania { get; set; }
        [Column("Nombre")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [Column("FechaInicio")]
        [Required]
        public DateTime FechaInicio { get; set; }

        [Column("FechaFinMVD")]
        [Required]
        public DateTime FechaFinMVD { get; set; }

        [Column("FechaFinFB")]
        [Required]
        public DateTime FechaFinFB { get; set; }

        public List<Solicitud> SolicitudesCampania { get; set; }
        public static bool ValidarDatos(string Nombre, DateTime? FechaInicio, DateTime? FechaFinMVD, DateTime? FechaFinFB)
        {
        
            if((Nombre!= "" && FechaInicio!= null && FechaFinMVD != null && FechaFinFB != null) && FechaFinMVD > FechaInicio && FechaFinFB > FechaInicio)
            {
                return true;
            }

            return false;   
        }

        public static Campania TraerCampaniaActual(string key, Task<HttpResponseMessage> respuesta)
        {
            Campania c = new Campania();
            try
            {
                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    c = JsonConvert.DeserializeObject<Campania>(json);
                    return c;
                }
                //Por este lado es que no existe una campania actual
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

    }
}
