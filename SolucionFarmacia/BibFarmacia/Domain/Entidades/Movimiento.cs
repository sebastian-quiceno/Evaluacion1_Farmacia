using System;
using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Domain.Entidades
{
    public class Movimiento
    {
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public string Tipo { get; set; } // "Venta", "Ingreso", etc.
        public IVendible Articulo { get; set; }

        public Movimiento(DateTime fecha, int cantidad, string tipo, IVendible articulo)
        {
            Fecha = fecha;
            Cantidad = cantidad;
            Tipo = tipo;
            Articulo = articulo;
        }
    }
}
