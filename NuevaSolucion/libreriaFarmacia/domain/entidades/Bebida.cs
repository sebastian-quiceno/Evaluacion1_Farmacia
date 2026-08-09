using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class Bebida: Producto
    {
        //Atributos
        private int mililitros;

        //Constructores
        public Bebida(string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias, int mililitros)
        : base(nombre, empresa, precio, stock, stockMinimo, plazoVencimientoDias)
        {
            Mililitros = mililitros;
        }

        public Bebida(string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias, int mililitros)
        : base(nombre, empresa, precio, stock, stockMinimo, plazoVencimientoDias)
        {
            Mililitros = mililitros;
        }

        public int Mililitros { get => mililitros;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Los mililitros de la bebida no puede ser negativo");
                mililitros = value;
            }
        }
    }
}
