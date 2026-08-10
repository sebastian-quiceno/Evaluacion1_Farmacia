using BibFarmacia.Enums;

namespace BibFarmacia.Interfaces
{
    public interface INotificador
    {
        void Notificar(string mensaje, TipoNotificacion tipo);
    }
}
