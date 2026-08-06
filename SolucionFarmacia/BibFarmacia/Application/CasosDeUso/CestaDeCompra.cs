using System;
using System.Collections.Generic;
using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Application.CasosDeUso
{
    public class CestaDeCompra
    {
        private List<IVendible> articulos;

        public CestaDeCompra()
        {
            articulos = new List<IVendible>();
        }

        public void AgregarArticulo(IVendible articulo, int cantidad)
        {
            if (articulo is IControlableEnInventario controlable)
            {
                if (!controlable.TieneStockSuficiente(cantidad))
                {
                    throw new InvalidOperationException($"No hay stock suficiente para {articulo.Nombre}.");
                }
            }

            for (int i = 0; i < cantidad; i++)
            {
                articulos.Add(articulo);
            }
        }

        public List<IVendible> ObtenerArticulos()
        {
            return articulos;
        }

        public decimal CalcularSubtotal()
        {
            decimal total = 0;
            foreach (var art in articulos)
            {
                total += art.Precio;
            }
            return total;
        }
        
        public void Vaciar()
        {
            articulos.Clear();
        }
    }
}
