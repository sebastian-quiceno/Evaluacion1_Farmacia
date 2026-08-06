using System;
using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Domain.Entidades
{
    public abstract class ProductoBase : IVendible, IControlableEnInventario
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public bool AplicaImpuesto { get; set; }
        
        public int Stock { get; set; }
        public int StockMinimo { get; set; }

        protected ProductoBase(string codigo, string nombre, decimal precio, int stock, int stockMinimo, bool aplicaImpuesto)
        {
            Codigo = codigo;
            Nombre = nombre;
            Precio = precio;
            Stock = stock;
            StockMinimo = stockMinimo;
            AplicaImpuesto = aplicaImpuesto;
        }

        public virtual void MostrarInformacion()
        {
            Console.WriteLine($"[{Codigo}] {Nombre} - Precio: {Precio} - Stock: {Stock}");
        }

        public void DeducirStock(int cantidad)
        {
            if (TieneStockSuficiente(cantidad))
            {
                Stock -= cantidad;
            }
            else
            {
                throw new InvalidOperationException($"Stock insuficiente para {Nombre}");
            }
        }

        public bool TieneStockSuficiente(int cantidad)
        {
            return Stock >= cantidad;
        }
    }
}
