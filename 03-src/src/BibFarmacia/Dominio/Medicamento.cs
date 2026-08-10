using BibFarmacia.Interfaces;

namespace BibFarmacia.Dominio
{
    // MostrarInformacion() se implementa aquí (no en MedicamentoCapsula ni MedicamentoLiquido):
    // en el AS-IS ninguna de las dos subclases la sobreescribía, ambas heredaban literalmente la
    // misma salida de Producto.MostrarInformacion(). Repetirla en cada subclase habría sido una
    // redefinición nueva no autorizada por A.1.1 (ver docs/Herencias y Verificacion LSP.md, 2.2).
    public abstract class Medicamento : ProductoBase, IPerecedero
    {
        protected Laboratorio laboratorio;
        protected DateTime fechaVencimiento;

        protected Medicamento(string codigo, string nombre, decimal precio, int stock, int stockMinimo,
            Laboratorio laboratorio, DateTime fechaVencimiento)
            : base(codigo, nombre, precio, stock, stockMinimo)
        {
            this.laboratorio = laboratorio;
            this.fechaVencimiento = fechaVencimiento;
        }

        public Laboratorio Laboratorio => laboratorio;
        public DateTime FechaVencimiento => fechaVencimiento;

        // Misma salida que Producto.MostrarInformacion() en el AS-IS (Producto.cs:29-34).
        public override void MostrarInformacion()
        {
            Console.WriteLine($"Producto: {nombre}");
            Console.WriteLine($"Precio: {precio}");
            Console.WriteLine($"Stock: {stock}");
        }

        // Misma condición que ServicioProducto.VerificarVencimiento (AS-IS): (FechaVencimiento - DateTime.Now).Days <= dias.
        public bool EstaProximoAVencer(int dias)
        {
            int diasRestantes = (fechaVencimiento - DateTime.Now).Days;
            return diasRestantes <= dias;
        }
    }
}
