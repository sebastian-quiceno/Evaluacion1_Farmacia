namespace BibFarmacia.Interfaces
{
    public interface IVendible
    {
        string Codigo { get; }
        string Nombre { get; }
        decimal Precio { get; }

        void MostrarInformacion();
    }
}
