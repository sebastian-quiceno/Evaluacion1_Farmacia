namespace BibFarmacia.Convenios
{
    public class ConvenioUniversidad : Convenio
    {
        private const decimal PorcentajeDescuento = 0.15m;

        public ConvenioUniversidad(string nombreConvenio) : base(nombreConvenio)
        {
        }

        public override decimal CalcularDescuento(decimal subtotal)
        {
            return subtotal * (1 - PorcentajeDescuento);
        }
    }
}
