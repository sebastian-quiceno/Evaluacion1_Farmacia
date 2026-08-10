using BibFarmacia.Interfaces;

namespace BibFarmacia.Dominio
{
    public abstract class ProductoBase : IVendible, IControlableEnInventario
    {
        protected string codigo;
        protected string nombre;
        protected decimal precio;
        protected int stock;
        protected int stockMinimo;

        protected ProductoBase(string codigo, string nombre, decimal precio, int stock, int stockMinimo)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.precio = precio;
            this.stock = stock;
            this.stockMinimo = stockMinimo;
        }

        public string Codigo => codigo;
        public string Nombre => nombre;
        public decimal Precio => precio;
        public int Stock => stock;
        public int StockMinimo => stockMinimo;

        public abstract void MostrarInformacion();

        // Réplica exacta del AS-IS (Program.cs:280, "productoVenta.Stock -= cantidad"): resta sin
        // validar, sin lanzar excepción. No se agrega una guarda que AS-IS no tenía — eso sería un
        // cambio de comportamiento no autorizado (A.1.1). Lo único que cambia es QUIÉN hace la resta:
        // antes cualquier código externo tocaba el campo directo; ahora solo el propio producto (H-07).
        public void DeducirStock(int cantidad)
        {
            stock -= cantidad;
        }

        public bool TieneStockSuficiente(int cantidad)
        {
            return stock >= cantidad;
        }

        // Misma condición que ServicioProducto.VerificarStock (AS-IS): producto.Stock <= producto.StockMinimo.
        public bool EstaEnStockMinimo()
        {
            return stock <= stockMinimo;
        }
    }
}
