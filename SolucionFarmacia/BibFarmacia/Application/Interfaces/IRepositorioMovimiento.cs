using BibFarmacia.Domain.Entidades;
using System.Collections.Generic;

namespace BibFarmacia.Application.Interfaces
{
    public interface IRepositorioMovimiento
    {
        void RegistrarMovimiento(Movimiento movimiento);
        List<Movimiento> ObtenerMovimientos();
    }
}
