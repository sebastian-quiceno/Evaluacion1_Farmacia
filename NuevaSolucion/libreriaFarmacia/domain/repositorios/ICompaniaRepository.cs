using libreriaFarmacia.domain.entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.repositorios
{
    public interface ICompaniaRepository
    {
        Task<Compania?> ObtenerPorIdAsync(int id);
        Task<IReadOnlyList<Compania>> ObtenerTodosAsync();
        Task GuardarAsync(Compania compania);
        Task ActualizarAsync(Compania compania);
        Task BorrarAsync(int id);
    }
}
