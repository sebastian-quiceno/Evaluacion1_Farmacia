using BibFarmacia.Dominio;

namespace BibFarmacia.Ventas
{
    // <<CasoDeUso>>: orquesta la venta (aplica el descuento del convenio del cliente y dispara el
    // evento). NO sabe cómo se actualiza el inventario ni cómo se registra el movimiento -- eso lo
    // hacen GestorInventario y MovimientoService, reaccionando cada uno a EventoVentaProcesada.
    public class CasodeUsoProcesarVenta
    {
        public delegate void DelegadoVenta(EventoVentaProcesada evento);

        public event DelegadoVenta? VentaProcesada;

        public decimal EjecutarVenta(CestaDeCompra cesta, Cliente cliente)
        {
            decimal subtotal = cesta.CalcularSubtotal();
            decimal total = cliente.Convenio.CalcularDescuento(subtotal);

            var evento = new EventoVentaProcesada(cesta.ObtenerLineas());

            VentaProcesada?.Invoke(evento);

            return total;
        }
    }
}
