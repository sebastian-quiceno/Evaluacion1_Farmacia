using System;
using BibFarmacia.Domain.Interfaces;
using BibFarmacia.Domain.Eventos;

namespace BibFarmacia.Application.CasosDeUso
{
    public class ProcesarVentaUseCase
    {
        public event EventHandler<VentaProcesadaEventArgs> VentaProcesadaEvent;

        public ProcesarVentaUseCase()
        {
        }

        public decimal EjecutarVenta(CestaDeCompra cesta, IEstrategiaDescuento estrategiaDescuento)
        {
            var articulos = cesta.ObtenerArticulos();
            if (articulos.Count == 0)
            {
                throw new InvalidOperationException("La cesta está vacía.");
            }

            decimal subtotal = cesta.CalcularSubtotal();
            
            // Aplicar descuento por convenio (SC-3)
            decimal descuento = 0;
            if (estrategiaDescuento != null)
            {
                descuento = estrategiaDescuento.CalcularDescuento(subtotal);
            }

            decimal totalFinal = subtotal - descuento;

            // Lanzar evento para que GestorInventario lo escuche
            OnVentaProcesada(new VentaProcesadaEventArgs(new System.Collections.Generic.List<IVendible>(articulos)));

            cesta.Vaciar();

            return totalFinal;
        }

        protected virtual void OnVentaProcesada(VentaProcesadaEventArgs e)
        {
            VentaProcesadaEvent?.Invoke(this, e);
        }
    }
}
