using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.interfaces
{
    public interface IVendible
    {
        int Id { get; set; }
        string Nombre { get; set; }
        float Precio { get; set; }

    }
}
