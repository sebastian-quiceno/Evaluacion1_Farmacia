using BibFarmacia.Enums;

namespace BibFarmacia.Dominio
{
    public class MedicamentoLiquido : Medicamento
    {
        private MaterialEnvase materialEnvase;
        private int mililitros;

        public MedicamentoLiquido(string codigo, string nombre, decimal precio, int stock, int stockMinimo,
            DateTime fechaVencimiento, Laboratorio laboratorio, MaterialEnvase materialEnvase, int mililitros)
            : base(codigo, nombre, precio, stock, stockMinimo, laboratorio, fechaVencimiento)
        {
            this.materialEnvase = materialEnvase;
            this.mililitros = mililitros;
        }

        public MaterialEnvase MaterialEnvase => materialEnvase;
        public int Mililitros => mililitros;
    }
}
