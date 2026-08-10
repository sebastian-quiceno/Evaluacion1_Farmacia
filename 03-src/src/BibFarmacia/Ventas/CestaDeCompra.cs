using BibFarmacia.Interfaces;

namespace BibFarmacia.Ventas
{
    public class CestaDeCompra
    {
        private List<LineaDeVenta> lineas;

        public CestaDeCompra()
        {
            lineas = new List<LineaDeVenta>();
        }

        public void AgregarArticulo(IVendible articulo, int cantidad)
        {
            lineas.Add(new LineaDeVenta(articulo, cantidad));
        }

        public List<LineaDeVenta> ObtenerLineas()
        {
            return lineas;
        }

        public decimal CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (var linea in lineas)
            {
                subtotal += linea.CalcularSubtotal();
            }

            return subtotal;
        }

        public void Vaciar()
        {
            lineas.Clear();
        }
    }
}
