using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface IMovimientoRepository
    {
        Task<Movimiento?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Movimiento>> ObtenerTodosAsync();
        Task GuardarAsync(Movimiento movimiento);
        Task ActualizarAsync(Movimiento movimiento);
        Task BorrarAsync(int id);
    }
}
