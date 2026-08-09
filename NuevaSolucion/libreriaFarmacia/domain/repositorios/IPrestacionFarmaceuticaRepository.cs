using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface IPrestacionFarmaceuticaRepository
    {
        Task<PrestacionFarmaceutica?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<PrestacionFarmaceutica>> ObtenerTodosAsync();
        Task GuardarAsync(PrestacionFarmaceutica prestacionFarmaceutica);
        Task ActualizarAsync(PrestacionFarmaceutica prestacionFarmaceutica);
        Task BorrarAsync(int id);
    }
}
