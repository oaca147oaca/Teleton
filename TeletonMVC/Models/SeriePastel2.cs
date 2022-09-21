using Dominio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TeletonMVC.Models
{
    public class SeriePastel2
    {
        public string name { get; set; }
        public double y { get; set; }
        public bool sliced { get; set; }
        public bool selected { get; set; }

        public SeriePastel2()
        {

        }
        public SeriePastel2(string name, double y, bool sliced = false, bool selected = false)
        {
            this.name = name;
            this.y = y;
            this.sliced = sliced;
            this.selected = selected;

        }

        public List<SeriePastel2> GetDataDummy(string Key, Task<HttpResponseMessage> respuesta)
        {
            try
            {
                //Genero la configuración para la api
                List<SeriePastel2> retorno = new List<SeriePastel2>();
                List<Alcancia> lstAlc = new List<Alcancia>();


                respuesta.Wait();

                if (respuesta.Result.IsSuccessStatusCode)
                {
                    Task<string> taskContent = respuesta.Result.Content.ReadAsStringAsync();
                    taskContent.Wait();
                    string json = taskContent.Result;
                    lstAlc = JsonConvert.DeserializeObject<List<Alcancia>>(json);


                    int lstDisp = 0;
                    int lstEntregadas = 0;
                    int lstRecibida = 0;
                    int lstCont = 0;
                    int lstDesv = 0;

                    for (int i = 0; i < lstAlc.Count; i++)
                    {
                        if (lstAlc[i].Estado == "DISPONIBLE")
                        {
                            lstDisp++;
                        }
                        else if(lstAlc[i].Estado == "ENTREGADA")
                        {
                            lstEntregadas++;
                        }
                        else if(lstAlc[i].Estado == "RECIBIDA")
                        {
                            lstRecibida++;
                        }
                        else if(lstAlc[i].Estado == "CONTABILIZADA")
                        {
                            lstCont++;
                        }else if(lstAlc[i].Estado == "DESVINCULADA")
                        {
                            lstDesv++;
                        }
                    }
                    retorno.Add(new SeriePastel2("Disponibles", lstDisp));
                    retorno.Add(new SeriePastel2("Entregadas", lstEntregadas));
                    retorno.Add(new SeriePastel2("Recibidas", lstRecibida));
                    retorno.Add(new SeriePastel2("Contabilizadas", lstCont));
                    retorno.Add(new SeriePastel2("Desvinculadas", lstDesv));
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
