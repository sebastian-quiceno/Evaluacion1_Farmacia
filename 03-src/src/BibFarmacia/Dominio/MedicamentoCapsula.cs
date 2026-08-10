using BibFarmacia.Enums;

namespace BibFarmacia.Dominio
{
    public class MedicamentoCapsula : Medicamento
    {
        private TipoRelleno tipoRelleno;

        public MedicamentoCapsula(string codigo, string nombre, decimal precio, int stock, int stockMinimo,
            DateTime fechaVencimiento, Laboratorio laboratorio, TipoRelleno tipoRelleno)
            : base(codigo, nombre, precio, stock, stockMinimo, laboratorio, fechaVencimiento)
        {
            this.tipoRelleno = tipoRelleno;
        }

        public TipoRelleno TipoRelleno => tipoRelleno;
    }
}
