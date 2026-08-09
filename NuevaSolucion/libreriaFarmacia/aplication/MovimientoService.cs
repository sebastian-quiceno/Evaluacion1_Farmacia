using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class MovimientoService
    {
        //Atributos
        private IMovimientoRepository repositorioMovimiento;

        //Constructor
        public MovimientoService(IMovimientoRepository repositorioMovimiento)
        {
            this.repositorioMovimiento = repositorioMovimiento;
        }

        //Metodos
        public async Task<Movimiento?> obtenerCompaniaPorIdAsync(int id)
        {
            return await repositorioMovimiento.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<Movimiento?>> obtenerTodasLasCompanias()
        {
            return await repositorioMovimiento.ObtenerTodosAsync();
        }

        public async Task crearCompaniaAsync(Movimiento movimiento)
        {
            await repositorioMovimiento.GuardarAsync(movimiento);
        }

        public async Task actualizarCompaniaAsync(Movimiento movimiento)
        {
            await repositorioMovimiento.ActualizarAsync(movimiento);
        }

        public async Task eliminarCompaniaPorIdAsync(int id)
        {
            await repositorioMovimiento.BorrarAsync(id);
        }
    }
}
