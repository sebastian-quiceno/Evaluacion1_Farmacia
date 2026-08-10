using BibFarmacia.Interfaces;

namespace BibFarmacia.Ventas
{
    public class LineaDeVenta
    {
        private IVendible articulo;
        private int cantidad;

        public LineaDeVenta(IVendible articulo, int cantidad)
        {
            this.articulo = articulo;
            this.cantidad = cantidad;
        }

        public IVendible Articulo => articulo;
        public int Cantidad => cantidad;

        public decimal CalcularSubtotal()
        {
            return articulo.Precio * cantidad;
        }
    }
}
