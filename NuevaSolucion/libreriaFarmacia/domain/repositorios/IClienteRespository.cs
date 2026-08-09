using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface IClienteRespository
    {
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Cliente>> ObtenerTodosAsync();
        Task GuardarAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
        Task BorrarAsync(int id);
    }
}
