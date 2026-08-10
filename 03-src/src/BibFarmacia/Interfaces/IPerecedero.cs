namespace BibFarmacia.Interfaces
{
    public interface IPerecedero
    {
        DateTime FechaVencimiento { get; }

        bool EstaProximoAVencer(int dias);
    }
}
