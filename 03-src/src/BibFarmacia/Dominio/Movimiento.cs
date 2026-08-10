using BibFarmacia.Interfaces;

namespace BibFarmacia.Dominio
{
    public class Movimiento
    {
        private DateTime fecha;
        private int cantidad;
        private string tipo;
        private IVendible articulo;

        public Movimiento(DateTime fecha, int cantidad, string tipo, IVendible articulo)
        {
            this.fecha = fecha;
            this.cantidad = cantidad;
            this.tipo = tipo;
            this.articulo = articulo;
        }

        public DateTime Fecha => fecha;
        public int Cantidad => cantidad;
        public string Tipo => tipo;
        public IVendible Articulo => articulo;
    }
}
