using BibFarmacia.Interfaces;

namespace BibFarmacia.Dominio
{
    // SC-2: nuevo tipo de ítem vendible que NO es inventariable (una inyectología no tiene stock).
    // Implementa únicamente IVendible -- deliberadamente no implementa IControlableEnInventario ni
    // IPerecedero (ver docs/Herencias y Verificacion LSP.md, 4.2).
    public class ServicioMedico : IVendible
    {
        private string codigo;
        private string nombre;
        private decimal precio;

        public ServicioMedico(string codigo, string nombre, decimal precio)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.precio = precio;
        }

        public string Codigo => codigo;
        public string Nombre => nombre;
        public decimal Precio => precio;

        public void MostrarInformacion()
        {
            Console.WriteLine($"Servicio: {nombre}");
            Console.WriteLine($"Precio: {precio}");
        }
    }
}
