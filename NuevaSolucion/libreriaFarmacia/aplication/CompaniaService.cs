using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class CompaniaService
    {
        //Atributos
        private ICompaniaRepository repositorioCompania;

        //Constructor
        public CompaniaService(ICompaniaRepository repositorioCompania)
        {
            this.repositorioCompania = repositorioCompania;
        }

        //Metodos
        public async Task<Compania?> obtenerCompaniaPorIdAsync(int id)
        {
            return await repositorioCompania.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<Compania?>> obtenerTodasLasCompanias()
        {
            return await repositorioCompania.ObtenerTodosAsync();
        }

        public async Task crearCompaniaAsync(Compania compania)
        {
            await repositorioCompania.GuardarAsync(compania);
        }

        public async Task actualizarCompaniaAsync(Compania compania)
        {
            await repositorioCompania.ActualizarAsync(compania);
        }

        public async Task eliminarCompaniaPorIdAsync(int id)
        {
            await repositorioCompania.BorrarAsync(id);
        }
    }
}
