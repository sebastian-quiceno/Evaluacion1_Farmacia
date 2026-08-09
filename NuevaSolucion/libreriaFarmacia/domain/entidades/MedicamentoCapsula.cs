using libreriaFarmacia.domain.enums;
using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class MedicamentoCapsula: Producto
    {
        //Atributos
        private TiposRelleno tipoRelleno;

        //Constructores
        public MedicamentoCapsula(string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias, TiposRelleno tipoRelleno)
        : base(nombre, empresa, precio, stock, stockMinimo, plazoVencimientoDias)
        {
            TipoRelleno = tipoRelleno;
        }

        //Getters Setters
        public TiposRelleno TipoRelleno { get; private set; } // El tipo de relleno debe ser inmutable
    }
}
