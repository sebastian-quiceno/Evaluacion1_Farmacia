using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class ClienteService
    {
        //Atributos
        private IClienteRespository repositorioClientes;

        //Constructor
        public ClienteService(IClienteRespository repositorioClientes)
        {
            this.repositorioClientes = repositorioClientes;
        }

        //Metodos
        public async Task<Cliente?> obtenerClientePorIdAsync(int id)
        {
            return await repositorioClientes.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<Cliente>> obtenerTodosLosClientes() {
            return await repositorioClientes.ObtenerTodosAsync();
        }

        public async Task crearClienteAsync(Cliente cliente) {
            await repositorioClientes.GuardarAsync(cliente);
        }

        public async Task actualizarClienteAsync(Cliente cliente) {
            await repositorioClientes.ActualizarAsync(cliente);
        }

        public async Task eliminarClientePorIdAsync(int id) {
            await repositorioClientes.BorrarAsync(id);
        }
    }
}
