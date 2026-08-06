using System.Collections.Generic;
using BibFarmacia.Application.Interfaces;
using BibFarmacia.Domain.Entidades;

namespace BibFarmacia.Infrastructure.Repositorios
{
    public class RepositorioMovimientoMemoria : IRepositorioMovimiento
    {
        private readonly List<Movimiento> movimientos = new List<Movimiento>();

        public void RegistrarMovimiento(Movimiento movimiento)
        {
            movimientos.Add(movimiento);
        }

        public List<Movimiento> ObtenerMovimientos()
        {
            return movimientos;
        }
    }
}
