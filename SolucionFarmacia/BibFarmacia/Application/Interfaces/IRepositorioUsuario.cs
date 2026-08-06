using BibFarmacia.Domain.Entidades;
using System.Collections.Generic;

namespace BibFarmacia.Application.Interfaces
{
    public interface IRepositorioUsuario
    {
        List<Usuario> ObtenerUsuarios();
    }
}
