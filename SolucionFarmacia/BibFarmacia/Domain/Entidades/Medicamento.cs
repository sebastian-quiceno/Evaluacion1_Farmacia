using System;

namespace BibFarmacia.Domain.Entidades
{
    public abstract class Medicamento : ProductoBase
    {
        public Laboratorio Laboratorio { get; set; }
        public DateTime FechaVencimiento { get; set; }

        protected Medicamento(string codigo, string nombre, decimal precio, int stock, int stockMinimo, bool aplicaImpuesto, Laboratorio laboratorio, DateTime fechaVencimiento)
            : base(codigo, nombre, precio, stock, stockMinimo, aplicaImpuesto)
        {
            Laboratorio = laboratorio;
            FechaVencimiento = fechaVencimiento;
        }

        public override void MostrarInformacion()
        {
            base.MostrarInformacion();
            Console.WriteLine($"Vencimiento: {FechaVencimiento.ToShortDateString()} - Lab: {Laboratorio.Nombre}");
        }
    }
}
