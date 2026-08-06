using System;
using BibFarmacia.Domain.Interfaces;
using BibFarmacia.Domain.Eventos;
using BibFarmacia.Application.Interfaces;

namespace BibFarmacia.Application.Servicios
{
    public class GestorInventario
    {
        private readonly IRepositorioCatalogo repositorioCatalogo;

        public GestorInventario(IRepositorioCatalogo repositorioCatalogo)
        {
            this.repositorioCatalogo = repositorioCatalogo;
        }

        public void AlProcesarVenta(object sender, VentaProcesadaEventArgs e)
        {
            foreach (var articulo in e.ArticulosVendidos)
            {
                // Solo reducimos inventario de aquellos artículos físicos, los servicios se ignoran
                if (articulo is IControlableEnInventario controlable)
                {
                    controlable.DeducirStock(1);
                    repositorioCatalogo.ActualizarStock(articulo, controlable.Stock);
                    
                    // Alertas de stock mínimo
                    if (controlable.Stock <= controlable.StockMinimo)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n[ALERTA INVENTARIO] El artículo {articulo.Nombre} ha alcanzado su stock mínimo ({controlable.StockMinimo}). Stock actual: {controlable.Stock}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
