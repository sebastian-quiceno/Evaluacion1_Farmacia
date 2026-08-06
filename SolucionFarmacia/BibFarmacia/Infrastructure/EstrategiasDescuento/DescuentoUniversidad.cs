using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Infrastructure.EstrategiasDescuento
{
    public class DescuentoUniversidad : IEstrategiaDescuento
    {
        public string NombreConvenio => "Convenio Universidad";

        public decimal CalcularDescuento(decimal subtotal)
        {
            return subtotal * 0.15m; // 15% de descuento
        }
    }
}
