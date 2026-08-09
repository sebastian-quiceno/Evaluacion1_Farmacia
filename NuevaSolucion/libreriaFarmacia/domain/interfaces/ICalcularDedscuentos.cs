using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.interfaces
{
    public interface ICalcularDedscuentos
    {
        float CalcularDescuento(Cliente cliente, decimal subtotal);
    }
}
