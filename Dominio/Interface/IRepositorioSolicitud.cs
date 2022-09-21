using System;
using System.Collections.Generic;
using System.Text;

namespace Dominio.Interface
{
    public interface IRepositorioSolicitud
    {
        List<Solicitud> GetSolicitudes();
    }
}
