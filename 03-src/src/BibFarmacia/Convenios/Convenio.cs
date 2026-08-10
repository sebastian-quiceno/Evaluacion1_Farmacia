namespace BibFarmacia.Convenios
{
    // Cada Convenio representa una relación comercial real (empresa, banco, universidad...).
    // CalcularDescuento debe respetar el invariante 0 <= resultado <= subtotal (ver
    // docs/Herencias y Verificacion LSP.md, sección 3.2): ninguna implementación puede
    // devolver más que el subtotal recibido.
    public abstract class Convenio
    {
        protected string nombreConvenio;

        protected Convenio(string nombreConvenio)
        {
            this.nombreConvenio = nombreConvenio;
        }

        public string NombreConvenio => nombreConvenio;

        public abstract decimal CalcularDescuento(decimal subtotal);
    }
}
