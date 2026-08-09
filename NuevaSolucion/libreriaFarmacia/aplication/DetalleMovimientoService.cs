using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class DetalleMovimientoService
    {
        //Atributos
        private IDetalleMovimientoRepository repositorioDetalleMovimiento;

        //Constructor
        public DetalleMovimientoService(IDetalleMovimientoRepository repositorioDetalleMovimiento)
        {
            this.repositorioDetalleMovimiento = repositorioDetalleMovimiento;
        }

        //Metodos
        public async Task<DetalleMovimiento?> obtenerCompaniaPorIdAsync(int id)
        {
            return await repositorioDetalleMovimiento.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<DetalleMovimiento?>> obtenerTodasLasCompanias()
        {
            return await repositorioDetalleMovimiento.ObtenerTodosAsync();
        }

        public async Task crearCompaniaAsync(DetalleMovimiento detalleMovimiento)
        {
            await repositorioDetalleMovimiento.GuardarAsync(detalleMovimiento);
        }

        public async Task actualizarCompaniaAsync(DetalleMovimiento detalleMovimiento)
        {
            await repositorioDetalleMovimiento.ActualizarAsync(detalleMovimiento);
        }

        public async Task eliminarCompaniaPorIdAsync(int id)
        {
            await repositorioDetalleMovimiento.BorrarAsync(id);
        }
    }
}
