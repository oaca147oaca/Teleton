
using Dominio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TeletonMVC.Models
{
    public class SeriePastel
    {

        public string name { get; set; }
        public double y { get; set; }
        public bool sliced { get; set; }
        public bool selected { get; set; }
       
        public SeriePastel()
        {
         
        }
        public SeriePastel(string name, double y, bool sliced=false, bool selected=false)
        {
            this.name = name;
            this.y = y;
            this.sliced = sliced;
            this.selected = selected;
          
  
        }

        public List<SeriePastel> GetDataDummy(string Key, Task<HttpResponseMessage> respuesta)
        {
            try
            {
                //Genero la configuración para la api
                List<SeriePastel> retorno = new List<SeriePastel>();
                List<Alcancia> lstAlc = new List<Alcancia>();

                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    lstAlc = JsonConvert.DeserializeObject<List<Alcancia>>(json);

                    int lstDisp = 0;
                    int lstNoDisp = 0;
                    for (int i=0;i<lstAlc.Count;i++)
                    {
                        if (lstAlc[i].Habilitada == "SI")
                        {
                            lstDisp++;
                        }
                        else
                        {
                            lstNoDisp++;
                        }
                    }
                    retorno.Add(new SeriePastel("Habilitada", lstDisp));
                    retorno.Add(new SeriePastel("No habilitada", lstNoDisp));
                    return retorno;
                }
                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
