using BibFarmacia.Interfaces;

namespace BibFarmacia.Dominio
{
    // SC-1: nuevo tipo de producto (cosméticos, comestibles). Hereda de ProductoBase, no de
    // Medicamento: no tiene laboratorio, y forzarlo bajo Medicamento habría violado LSP (ver
    // docs/Herencias y Verificacion LSP.md, 4.1). Implementa IPerecedero de forma independiente.
    public class ArticuloRetail : ProductoBase, IPerecedero
    {
        private DateTime fechaVencimiento;

        public ArticuloRetail(string codigo, string nombre, decimal precio, int stock, int stockMinimo,
            DateTime fechaVencimiento)
            : base(codigo, nombre, precio, stock, stockMinimo)
        {
            this.fechaVencimiento = fechaVencimiento;
        }

        public DateTime FechaVencimiento => fechaVencimiento;

        public override void MostrarInformacion()
        {
            Console.WriteLine($"Producto: {nombre}");
            Console.WriteLine($"Precio: {precio}");
            Console.WriteLine($"Stock: {stock}");
        }

        public bool EstaProximoAVencer(int dias)
        {
            int diasRestantes = (fechaVencimiento - DateTime.Now).Days;
            return diasRestantes <= dias;
        }
    }
}
