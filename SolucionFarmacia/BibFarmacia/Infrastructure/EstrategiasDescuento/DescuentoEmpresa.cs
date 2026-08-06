using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Infrastructure.EstrategiasDescuento
{
    public class DescuentoEmpresa : IEstrategiaDescuento
    {
        public string NombreConvenio => "Convenio Empresarial";

        public decimal CalcularDescuento(decimal subtotal)
        {
            return subtotal * 0.20m; // 20% de descuento
        }
    }
}
