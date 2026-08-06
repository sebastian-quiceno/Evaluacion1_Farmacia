using BibFarmacia.Domain.Entidades;
using System.Collections.Generic;

namespace BibFarmacia.Application.Interfaces
{
    public interface IRepositorioCliente
    {
        List<Cliente> ObtenerClientes();
        void GuardarCliente(Cliente cliente);
    }
}
