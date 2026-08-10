using BibFarmacia.Dominio;
using BibFarmacia.Enums;
using BibFarmacia.Interfaces;
using BibFarmacia.Ventas;

namespace BibFarmacia.Servicios
{
    public class MovimientoService
    {
        private List<Movimiento> movimientos;
        private INotificador notificador;

        public MovimientoService(INotificador notificador)
        {
            movimientos = new List<Movimiento>();
            this.notificador = notificador;
        }

        // Réplica de ServicioMovimiento.RegistrarMovimiento (AS-IS): mismo mensaje
        // "Movimiento registrado: {tipo}", ahora vía INotificador en vez de un EventoMovimiento concreto.
        public void RegistrarMovimiento(Movimiento movimiento)
        {
            movimientos.Add(movimiento);

            notificador.Notificar(
                $"Movimiento registrado: {movimiento.Tipo}",
                TipoNotificacion.MovimientoRegistrado);
        }

        public List<Movimiento> ObtenerMovimientos()
        {
            return movimientos;
        }

        // Reacciona a la venta: un Movimiento tipo "Venta" por cada línea (en el AS-IS solo existía
        // una línea por venta, así que para el escenario preservado este bucle genera exactamente un
        // Movimiento, igual que Program.cs hacía en línea).
        public void AlProcesarVenta(EventoVentaProcesada evento)
        {
            foreach (var linea in evento.Lineas)
            {
                Movimiento movimiento = new Movimiento(
                    evento.FechaVenta,
                    linea.Cantidad,
                    "Venta",
                    linea.Articulo);

                RegistrarMovimiento(movimiento);
            }
        }
    }
}
