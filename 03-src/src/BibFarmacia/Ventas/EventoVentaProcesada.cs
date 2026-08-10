namespace BibFarmacia.Ventas
{
    // Dato que viaja entre CasodeUsoProcesarVenta (quien vende) y GestorInventario /
    // MovimientoService (quienes reaccionan a la venta), sin que ninguno de los dos conozca al otro.
    public class EventoVentaProcesada
    {
        private List<LineaDeVenta> lineas;
        private DateTime fechaVenta;

        public EventoVentaProcesada(List<LineaDeVenta> lineas)
        {
            this.lineas = lineas;
            fechaVenta = DateTime.Now;
        }

        public List<LineaDeVenta> Lineas => lineas;
        public DateTime FechaVenta => fechaVenta;
    }
}
