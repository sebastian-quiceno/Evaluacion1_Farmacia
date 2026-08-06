namespace BibFarmacia.Domain.Interfaces
{
    public interface IEstrategiaDescuento
    {
        string NombreConvenio { get; }
        decimal CalcularDescuento(decimal subtotal);
    }
}
