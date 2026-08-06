using System;
using System.Collections.Generic;
using System.IO;
using BibFarmacia.Application.Interfaces;
using BibFarmacia.Domain.Entidades;
using BibFarmacia.Domain.Interfaces;
using BibFarmacia.Infrastructure.EstrategiasDescuento;

namespace BibFarmacia.Infrastructure.Repositorios
{
    public class RepositorioClienteTxt : IRepositorioCliente
    {
        private readonly string rutaArchivo;
        private List<Cliente> cacheClientes;
        private readonly Dictionary<string, Func<IEstrategiaDescuento>> factoryConvenios;

        public RepositorioClienteTxt(string rutaArchivo)
        {
            this.rutaArchivo = rutaArchivo;
            
            // OCP: Registro de estrategias mediante delegados
            factoryConvenios = new Dictionary<string, Func<IEstrategiaDescuento>>
            {
                { "Universidad", () => new DescuentoUniversidad() },
                { "Empresa", () => new DescuentoEmpresa() },
                { "Ninguno", () => new DescuentoNulo() }
            };
        }
        
        public void RegistrarNuevaEstrategia(string nombre, Func<IEstrategiaDescuento> creador)
        {
            factoryConvenios[nombre] = creador;
        }

        public List<Cliente> ObtenerClientes()
        {
            if (cacheClientes != null) return cacheClientes;

            cacheClientes = new List<Cliente>();

            if (!File.Exists(rutaArchivo)) return cacheClientes;

            string[] lineas = File.ReadAllLines(rutaArchivo);
            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                string[] datos = linea.Split(';');
                string nombre = datos[0];
                string cedula = datos[1];
                string telefono = datos[2];
                string correo = datos[3];
                string tipoConvenio = datos.Length > 4 ? datos[4] : "Ninguno";

                IEstrategiaDescuento convenio = factoryConvenios.ContainsKey(tipoConvenio) 
                    ? factoryConvenios[tipoConvenio]() 
                    : new DescuentoNulo();

                cacheClientes.Add(new Cliente(nombre, cedula, telefono, correo, convenio));
            }
            return cacheClientes;
        }

        public void GuardarCliente(Cliente cliente)
        {
            if (cacheClientes == null) ObtenerClientes();
            if (!cacheClientes.Contains(cliente)) cacheClientes.Add(cliente);

            List<string> lineas = new List<string>();
            foreach (var c in cacheClientes)
            {
                string tipoConvenio = "Ninguno";
                if (c.Convenio is DescuentoUniversidad) tipoConvenio = "Universidad";
                else if (c.Convenio is DescuentoEmpresa) tipoConvenio = "Empresa";
                
                lineas.Add($"{c.Nombre};{c.Cedula};{c.Telefono};{c.Correo};{tipoConvenio}");
            }
            File.WriteAllLines(rutaArchivo, lineas);
        }
    }
}
