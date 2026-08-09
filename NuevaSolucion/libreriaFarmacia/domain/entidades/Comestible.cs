using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class Comestible : Producto
    {
        //Atributos
        private int pesoGramos;

        //Constructores
        public Comestible(string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias, int pesoGramos)
        : base(nombre, empresa, precio, stock, stockMinimo, plazoVencimientoDias)
        {
            PesoGramos = pesoGramos;
        }

        //Getters Setters
        public int PesoGramos { get => pesoGramos;
            set {
                if (value < 0)
                    throw new ArgumentException("El peso del alimento no puede ser negativo");
                pesoGramos = value;
            }
        }
    }
}
