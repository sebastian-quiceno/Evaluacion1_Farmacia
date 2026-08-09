using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface IDetalleMovimientoRepository
    {
        Task<DetalleMovimiento?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<DetalleMovimiento>> ObtenerTodosAsync();
        Task GuardarAsync(DetalleMovimiento detalleMovimiento);
        Task ActualizarAsync(DetalleMovimiento detalleMovimiento);
        Task BorrarAsync(int id);
    }
}
