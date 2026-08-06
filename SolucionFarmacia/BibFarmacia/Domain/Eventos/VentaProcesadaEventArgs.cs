using System;
using BibFarmacia.Domain.Interfaces;
using System.Collections.Generic;

namespace BibFarmacia.Domain.Eventos
{
    public class VentaProcesadaEventArgs : EventArgs
    {
        public List<IVendible> ArticulosVendidos { get; }
        public DateTime FechaVenta { get; }

        public VentaProcesadaEventArgs(List<IVendible> articulosVendidos)
        {
            ArticulosVendidos = articulosVendidos;
            FechaVenta = DateTime.Now;
        }
    }
}
