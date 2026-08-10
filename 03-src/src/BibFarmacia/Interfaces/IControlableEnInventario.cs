namespace BibFarmacia.Interfaces
{
    public interface IControlableEnInventario
    {
        int Stock { get; }
        int StockMinimo { get; }

        void DeducirStock(int cantidad);
        bool TieneStockSuficiente(int cantidad);
        bool EstaEnStockMinimo();
    }
}
