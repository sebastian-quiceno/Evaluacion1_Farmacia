using libreriaFarmacia.domain.enums;
using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class MedicamentoLiquido : Producto
    {
        //Atributos
        private int mililitros;
        private MaterialesEnvase materialEnvase;

        //Constructores
        public MedicamentoLiquido(string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias, int mililitros, MaterialesEnvase materialEnvase)
        : base(nombre, empresa, precio, stock, stockMinimo, plazoVencimientoDias)
        {
            MaterialEnvase = materialEnvase;
        }

        //Getters Setters 
        public int Mililitros { get => mililitros;
            set {
                if (value < 0)
                    throw new ArgumentException("El volumen del Medicamento Liquido no puede ser negativo");
                mililitros = value;
            } 
        }
        public MaterialesEnvase MaterialEnvase { get; private set; }

    }
}
