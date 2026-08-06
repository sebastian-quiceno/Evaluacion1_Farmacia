using System;
using BibFarmacia.Domain.Enums;

namespace BibFarmacia.Domain.Entidades
{
    public class MedicamentoLiquido : Medicamento
    {
        public MaterialEnvase MaterialEnvase { get; set; }
        public int Mililitros { get; set; }

        public MedicamentoLiquido(string codigo, string nombre, decimal precio, int stock, int stockMinimo, bool aplicaImpuesto, Laboratorio laboratorio, DateTime fechaVencimiento, MaterialEnvase materialEnvase, int mililitros)
            : base(codigo, nombre, precio, stock, stockMinimo, aplicaImpuesto, laboratorio, fechaVencimiento)
        {
            MaterialEnvase = materialEnvase;
            Mililitros = mililitros;
        }

        public override void MostrarInformacion()
        {
            base.MostrarInformacion();
            Console.WriteLine($"Envase: {MaterialEnvase} - {Mililitros}ml");
        }
    }
}
