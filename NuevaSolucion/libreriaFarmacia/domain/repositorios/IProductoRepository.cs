using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface IProductoRepository
    {
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Producto>> ObtenerTodosAsync();
        Task GuardarAsync(Producto producto);
        Task ActualizarAsync(Producto producto);
        Task BorrarAsync(int id);
    }
}
