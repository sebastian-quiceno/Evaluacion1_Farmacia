using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class AsociacionService
    {
        //Atributos
        private IAsociacionRepository repositorioAsociacion;

        //Constructor
        public AsociacionService(IAsociacionRepository repositorioAsociacion)
        {
            this.repositorioAsociacion = repositorioAsociacion;
        }

        //Metodos
        public async Task<Asociacion?> obtenerCompaniaPorIdAsync(int id)
        {
            return await repositorioAsociacion.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<Asociacion?>> obtenerTodasLasCompanias()
        {
            return await repositorioAsociacion.ObtenerTodosAsync();
        }

        public async Task crearCompaniaAsync(Asociacion asociacion)
        {
            await repositorioAsociacion.GuardarAsync(asociacion);
        }

        public async Task actualizarCompaniaAsync(Asociacion asociacion)
        {
            await repositorioAsociacion.ActualizarAsync(asociacion);
        }

        public async Task eliminarCompaniaPorIdAsync(int id)
        {
            await repositorioAsociacion.BorrarAsync(id);
        }
    }
}
