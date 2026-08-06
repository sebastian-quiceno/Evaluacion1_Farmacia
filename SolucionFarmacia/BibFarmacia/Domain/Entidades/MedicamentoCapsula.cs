using System;
using BibFarmacia.Domain.Enums;

namespace BibFarmacia.Domain.Entidades
{
    public class MedicamentoCapsula : Medicamento
    {
        public TipoRelleno TipoRelleno { get; set; }

        public MedicamentoCapsula(string codigo, string nombre, decimal precio, int stock, int stockMinimo, bool aplicaImpuesto, Laboratorio laboratorio, DateTime fechaVencimiento, TipoRelleno tipoRelleno)
            : base(codigo, nombre, precio, stock, stockMinimo, aplicaImpuesto, laboratorio, fechaVencimiento)
        {
            TipoRelleno = tipoRelleno;
        }

        public override void MostrarInformacion()
        {
            base.MostrarInformacion();
            Console.WriteLine($"Tipo Relleno: {TipoRelleno}");
        }
    }
}
