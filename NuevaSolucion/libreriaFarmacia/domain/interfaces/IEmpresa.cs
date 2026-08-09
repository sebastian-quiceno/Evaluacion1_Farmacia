using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.interfaces
{
    public interface IEmpresa
    {
        string Nombre { get; set; }
        string Direccion { get; set; }
        string Telefono { get; set; }
    }
}
