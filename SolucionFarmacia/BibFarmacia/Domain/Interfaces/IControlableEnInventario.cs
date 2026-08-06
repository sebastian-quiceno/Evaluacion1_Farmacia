namespace BibFarmacia.Domain.Interfaces
{
    public interface IControlableEnInventario
    {
        int Stock { get; set; }
        int StockMinimo { get; set; }
        
        void DeducirStock(int cantidad);
        bool TieneStockSuficiente(int cantidad);
    }
}
