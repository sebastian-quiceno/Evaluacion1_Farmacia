using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class PrestacionFarmaceuticaService
    {
        //Atributos
        private IPrestacionFarmaceuticaRepository repositorioPrestacionFarmaceutica;

        //Constructor
        public PrestacionFarmaceuticaService(IPrestacionFarmaceuticaRepository repositorioPrestacionFarmaceutica)
        {
            this.repositorioPrestacionFarmaceutica = repositorioPrestacionFarmaceutica;
        }

        //Metodos
        public async Task<PrestacionFarmaceutica?> obtenerCompaniaPorIdAsync(int id)
        {
            return await repositorioPrestacionFarmaceutica.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<PrestacionFarmaceutica?>> obtenerTodasLasCompanias()
        {
            return await repositorioPrestacionFarmaceutica.ObtenerTodosAsync();
        }

        public async Task crearCompaniaAsync(PrestacionFarmaceutica prestacionFarmaceutica)
        {
            await repositorioPrestacionFarmaceutica.GuardarAsync(prestacionFarmaceutica);
        }

        public async Task actualizarCompaniaAsync(PrestacionFarmaceutica prestacionFarmaceutica)
        {
            await repositorioPrestacionFarmaceutica.ActualizarAsync(prestacionFarmaceutica);
        }

        public async Task eliminarCompaniaPorIdAsync(int id)
        {
            await repositorioPrestacionFarmaceutica.BorrarAsync(id);
        }
    }
}
