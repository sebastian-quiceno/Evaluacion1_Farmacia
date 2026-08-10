using BibFarmacia.Dominio;
using BibFarmacia.Enums;
using BibFarmacia.Interfaces;
using BibFarmacia.Ventas;

namespace BibFarmacia.Servicios
{
    public class GestorInventario
    {
        private List<ProductoBase> productos;
        private INotificador notificador;

        public GestorInventario(INotificador notificador)
        {
            productos = new List<ProductoBase>();
            this.notificador = notificador;
        }

        // Réplica exacta de ServicioProducto.CargarDesdeArchivo (AS-IS): mismas columnas, mismo
        // mensaje de retorno, mismo hallazgo H-12 (siempre construye una cápsula con relleno Gel y
        // laboratorio hardcodeado a "Medellin"/"4444444") -- productos.txt no cambió de formato en
        // esta entrega (SC-1 no es la solicitud elegida para implementarse, ver evidencia/metrica-SC2.md),
        // así que "fijarlo de verdad" exigiría una columna de tipo que el archivo no tiene todavía.
        // Se declara como deuda técnica consciente, igual que en ADR-003.
        public string CargarDesdeArchivo(string ruta)
        {
            try
            {
                if (!File.Exists(ruta))
                {
                    return "Archivo no encontrado";
                }

                string[] lineas = File.ReadAllLines(ruta);

                foreach (string linea in lineas)
                {
                    string[] datos = linea.Split(';');

                    Laboratorio laboratorio = new Laboratorio(datos[5], "Medellin", "4444444");

                    MedicamentoCapsula medicamento = new MedicamentoCapsula(
                        codigo: datos[0],
                        nombre: datos[0],
                        precio: decimal.Parse(datos[1]),
                        stock: int.Parse(datos[2]),
                        stockMinimo: int.Parse(datos[3]),
                        fechaVencimiento: DateTime.Parse(datos[4]),
                        laboratorio: laboratorio,
                        tipoRelleno: TipoRelleno.Gel);

                    productos.Add(medicamento);
                }

                return "Productos cargados";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<ProductoBase> ObtenerProductos()
        {
            return productos;
        }

        // Misma condición que ServicioProducto.VerificarStock (AS-IS): producto.Stock <= producto.StockMinimo.
        public void VerificarStock()
        {
            foreach (var producto in productos)
            {
                if (producto.EstaEnStockMinimo())
                {
                    notificador.Notificar(
                        $"ALERTA: stock mínimo de {producto.Nombre}",
                        TipoNotificacion.StockMinimo);
                }
            }
        }

        // Misma condición que ServicioProducto.VerificarVencimiento (AS-IS): dias <= 30.
        public void VerificarVencimiento()
        {
            foreach (var producto in productos)
            {
                if (producto is IPerecedero perecedero && perecedero.EstaProximoAVencer(30))
                {
                    notificador.Notificar(
                        $"ALERTA: {producto.Nombre} próximo a vencer",
                        TipoNotificacion.Vencimiento);
                }
            }
        }

        // Reacciona a la venta: descuenta stock únicamente de los artículos que sí son
        // inventariables (un ServicioMedico, por ejemplo, no implementa IControlableEnInventario).
        public void AlProcesarVenta(EventoVentaProcesada evento)
        {
            foreach (var linea in evento.Lineas)
            {
                if (linea.Articulo is IControlableEnInventario inventariable)
                {
                    inventariable.DeducirStock(linea.Cantidad);
                }
            }
        }
    }
}
