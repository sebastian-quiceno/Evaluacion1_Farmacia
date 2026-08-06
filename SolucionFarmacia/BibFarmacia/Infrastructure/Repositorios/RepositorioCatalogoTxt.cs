using System;
using System.Collections.Generic;
using System.IO;
using BibFarmacia.Application.Interfaces;
using BibFarmacia.Domain.Entidades;
using BibFarmacia.Domain.Interfaces;
using BibFarmacia.Domain.Enums;
using System.Linq;

namespace BibFarmacia.Infrastructure.Repositorios
{
    public class RepositorioCatalogoTxt : IRepositorioCatalogo
    {
        private readonly string rutaArchivo;
        private List<IVendible> cacheCatalogo;
        private readonly Dictionary<string, Func<string[], IVendible>> factoryProduct;

        public RepositorioCatalogoTxt(string rutaArchivo)
        {
            this.rutaArchivo = rutaArchivo;
            
            // OCP: Registro de delegados de creación. Si se añaden nuevos tipos, 
            // no hay que modificar el método ObtenerCatalogo, solo registrar la nueva función.
            factoryProduct = new Dictionary<string, Func<string[], IVendible>>
            {
                { "Servicio", datos => new ServicioMedico(datos[1], datos[2], decimal.Parse(datos[3])) },
                { "Retail", datos => new ArticuloRetail(datos[1], datos[2], decimal.Parse(datos[3]), int.Parse(datos[4]), int.Parse(datos[5])) },
                { "Capsula", datos => new MedicamentoCapsula(datos[1], datos[2], decimal.Parse(datos[3]), int.Parse(datos[4]), int.Parse(datos[5]), true, new Laboratorio(datos[7], "N/A", "N/A"), DateTime.Parse(datos[6]), Enum.Parse<TipoRelleno>(datos[8])) },
                { "Liquido", datos => new MedicamentoLiquido(datos[1], datos[2], decimal.Parse(datos[3]), int.Parse(datos[4]), int.Parse(datos[5]), true, new Laboratorio(datos[7], "N/A", "N/A"), DateTime.Parse(datos[6]), Enum.Parse<MaterialEnvase>(datos[8]), int.Parse(datos[9])) }
            };
        }

        public List<IVendible> ObtenerCatalogo()
        {
            if (cacheCatalogo != null) return cacheCatalogo;
            
            cacheCatalogo = new List<IVendible>();
            
            if (!File.Exists(rutaArchivo)) return cacheCatalogo;

            string[] lineas = File.ReadAllLines(rutaArchivo);
            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                string[] datos = linea.Split(';');
                string tipo = datos[0];
                
                if (factoryProduct.TryGetValue(tipo, out var creador))
                {
                    cacheCatalogo.Add(creador(datos));
                }
            }
            return cacheCatalogo;
        }

        public void ActualizarStock(IVendible articulo, int cantidadRestante)
        {
            if (cacheCatalogo == null) ObtenerCatalogo();

            // Guardar cambios al disco (Simulado para simplificar, en la vida real reescribe el archivo o usa DB)
            List<string> lineas = new List<string>();
            foreach (var item in cacheCatalogo)
            {
                if (item is MedicamentoCapsula cap)
                    lineas.Add($"Capsula;{cap.Codigo};{cap.Nombre};{cap.Precio};{cap.Stock};{cap.StockMinimo};{cap.FechaVencimiento};{cap.Laboratorio.Nombre};{cap.TipoRelleno}");
                else if (item is MedicamentoLiquido liq)
                    lineas.Add($"Liquido;{liq.Codigo};{liq.Nombre};{liq.Precio};{liq.Stock};{liq.StockMinimo};{liq.FechaVencimiento};{liq.Laboratorio.Nombre};{liq.MaterialEnvase};{liq.Mililitros}");
                else if (item is ArticuloRetail ret)
                    lineas.Add($"Retail;{ret.Codigo};{ret.Nombre};{ret.Precio};{ret.Stock};{ret.StockMinimo}");
                else if (item is ServicioMedico serv)
                    lineas.Add($"Servicio;{serv.Codigo};{serv.Nombre};{serv.Precio}");
            }
            File.WriteAllLines(rutaArchivo, lineas);
        }
    }
}
