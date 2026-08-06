namespace BibFarmacia.Domain.Interfaces
{
    public interface IVendible
    {
        string Codigo { get; set; }
        string Nombre { get; set; }
        decimal Precio { get; set; }
        bool AplicaImpuesto { get; set; }

        void MostrarInformacion();
    }
}
