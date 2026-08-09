using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface IAsociacionRepository
    {
        Task<Asociacion?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Asociacion>> ObtenerTodosAsync();
        Task GuardarAsync(Asociacion asociacion);
        Task ActualizarAsync(Asociacion asociacion);
        Task BorrarAsync(int id);
    }
}
