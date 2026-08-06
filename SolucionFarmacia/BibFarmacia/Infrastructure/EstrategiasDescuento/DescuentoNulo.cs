using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Infrastructure.EstrategiasDescuento
{
    public class DescuentoNulo : IEstrategiaDescuento
    {
        public string NombreConvenio => "Sin Convenio";

        public decimal CalcularDescuento(decimal subtotal)
        {
            return 0;
        }
    }
}
