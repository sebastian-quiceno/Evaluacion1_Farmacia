namespace BibFarmacia.Convenios
{
    public class ConvenioEmpresa : Convenio
    {
        private const decimal PorcentajeDescuento = 0.10m;

        public ConvenioEmpresa(string nombreConvenio) : base(nombreConvenio)
        {
        }

        public override decimal CalcularDescuento(decimal subtotal)
        {
            return subtotal * (1 - PorcentajeDescuento);
        }
    }
}
