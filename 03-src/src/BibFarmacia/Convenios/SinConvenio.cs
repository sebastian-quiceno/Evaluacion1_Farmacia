namespace BibFarmacia.Convenios
{
    // Es el convenio que se asigna por defecto a todo cliente que ya existía antes de SC-3
    // (clientes.txt sin columna de convenio). Su contrato es un requisito de comportamiento,
    // no una opción de diseño: debe devolver el subtotal exactamente igual, sin descuento,
    // para que ningún cliente existente vea un comportamiento nuevo (restricción dura A.1.1).
    public class SinConvenio : Convenio
    {
        public SinConvenio() : base("Sin convenio")
        {
        }

        public override decimal CalcularDescuento(decimal subtotal)
        {
            return subtotal;
        }
    }
}
