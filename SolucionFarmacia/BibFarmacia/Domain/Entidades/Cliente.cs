using System;
using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Domain.Entidades
{
    public class Cliente : Persona
    {
        public int Puntos { get; set; }
        public IEstrategiaDescuento Convenio { get; set; }

        public Cliente(string nombre, string cedula, string telefono, string correo, IEstrategiaDescuento convenio = null)
            : base(nombre, cedula, telefono, correo)
        {
            Puntos = 0;
            Convenio = convenio;
        }

        public void AcumularPuntos(int puntos)
        {
            if (puntos < 0) throw new ArgumentException("Los puntos no pueden ser negativos");
            Puntos += puntos;
        }
    }
}
