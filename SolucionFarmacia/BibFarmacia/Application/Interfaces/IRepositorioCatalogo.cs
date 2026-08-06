using BibFarmacia.Domain.Interfaces;
using BibFarmacia.Domain.Entidades;
using System.Collections.Generic;

namespace BibFarmacia.Application.Interfaces
{
    public interface IRepositorioCatalogo
    {
        List<IVendible> ObtenerCatalogo();
        void ActualizarStock(IVendible articulo, int cantidad);
    }
}
