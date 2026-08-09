using libreriaFarmacia.domain.entidades;
using libreriaFarmacia.domain.repositorios;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.aplication
{
    public class ProductoService
    {
        //Atributos
        private IProductoRepository repositorioProducto;

        //Constructor
        public ProductoService(IProductoRepository repositorioProducto)
        {
            this.repositorioProducto = repositorioProducto;
        }

        //Metodos
        public async Task<Producto?> obtenerCompaniaPorIdAsync(int id)
        {
            return await repositorioProducto.ObtenerPorIdAsync(id);
        }

        public async Task<IReadOnlyList<Producto?>> obtenerTodasLasCompanias()
        {
            return await repositorioProducto.ObtenerTodosAsync();
        }

        public async Task crearCompaniaAsync(Producto producto)
        {
            await repositorioProducto.GuardarAsync(producto);
        }

        public async Task actualizarCompaniaAsync(Producto producto)
        {
            await repositorioProducto.ActualizarAsync(producto);
        }

        public async Task eliminarCompaniaPorIdAsync(int id)
        {
            await repositorioProducto.BorrarAsync(id);
        }
    }
}
