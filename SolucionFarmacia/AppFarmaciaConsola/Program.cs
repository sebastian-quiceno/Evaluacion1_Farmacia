using System;
using System.Linq;
using BibFarmacia.Application.CasosDeUso;
using BibFarmacia.Application.Interfaces;
using BibFarmacia.Application.Servicios;
using BibFarmacia.Domain.Entidades;
using BibFarmacia.Domain.Interfaces;
using BibFarmacia.Infrastructure.Repositorios;

namespace AppFarmaciaConsola
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Sistema Farmacia (Clean Architecture)";

            // ================= COMPOSITION ROOT (Pure DI) =================
            IRepositorioCatalogo repoCatalogo = new RepositorioCatalogoTxt("productos.txt");
            IRepositorioCliente repoCliente = new RepositorioClienteTxt("clientes.txt");
            IRepositorioUsuario repoUsuario = new RepositorioUsuarioTxt("usuarios.txt");
            IRepositorioMovimiento repoMovimiento = new RepositorioMovimientoMemoria();

            GestorInventario gestorInventario = new GestorInventario(repoCatalogo);
            ProcesarVentaUseCase casoUsoVenta = new ProcesarVentaUseCase();
            
            // Suscribir el GestorInventario al evento de venta
            casoUsoVenta.VentaProcesadaEvent += gestorInventario.AlProcesarVenta;
            // También suscribiremos el log de movimientos
            casoUsoVenta.VentaProcesadaEvent += (sender, e) =>
            {
                foreach (var art in e.ArticulosVendidos)
                {
                    repoMovimiento.RegistrarMovimiento(new Movimiento(e.FechaVenta, 1, "Venta", art));
                }
            };

            // ================= CARGA INICIAL =================
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Cargando información del sistema...\n");
            Console.ResetColor();

            var catalogo = repoCatalogo.ObtenerCatalogo();
            var clientes = repoCliente.ObtenerClientes();
            var usuarios = repoUsuario.ObtenerUsuarios();

            Console.WriteLine($"{catalogo.Count} productos/servicios cargados.");
            Console.WriteLine($"{clientes.Count} clientes cargados.");
            Console.WriteLine($"{usuarios.Count} usuarios cargados.\n");

            // ================= LOGIN =================
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("=========== LOGIN ===========");
            Console.ResetColor();

            Console.Write("Usuario: ");
            string user = Console.ReadLine();
            Console.Write("Contraseña: ");
            string password = Console.ReadLine();

            bool loginCorrecto = usuarios.Any(u => u.UserName == user && u.Password == password);

            if (!loginCorrecto)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAcceso denegado");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nLogin correcto");
            Console.ResetColor();

            // ================= MENÚ =================
            int opcion = 0;
            while (opcion != 6)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n==============================");
                Console.WriteLine("      SISTEMA FARMACIA");
                Console.WriteLine("==============================");
                Console.ResetColor();

                Console.WriteLine("1. Ver catálogo (Productos y Servicios)");
                Console.WriteLine("2. Ver clientes");
                Console.WriteLine("3. Buscar en catálogo");
                Console.WriteLine("4. Registrar nueva venta (SC-1, SC-2, SC-3)");
                Console.WriteLine("5. Acumular puntos");
                Console.WriteLine("6. Salir");

                Console.Write("\nSeleccione opción: ");
                if (!int.TryParse(Console.ReadLine(), out opcion)) opcion = 0;

                switch (opcion)
                {
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n===== CATÁLOGO =====");
                        Console.ResetColor();
                        
                        foreach (var articulo in repoCatalogo.ObtenerCatalogo())
                        {
                            articulo.MostrarInformacion();
                            Console.WriteLine("---");
                        }
                        break;

                    case 2:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n===== CLIENTES =====");
                        Console.ResetColor();

                        foreach (var cliente in repoCliente.ObtenerClientes())
                        {
                            Console.WriteLine($"{cliente.Nombre} - Puntos: {cliente.Puntos} - Convenio: {cliente.Convenio?.NombreConvenio}");
                        }
                        break;

                    case 3:
                        Console.Write("\nIngrese nombre del artículo: ");
                        string nombre = Console.ReadLine();

                        var buscado = repoCatalogo.ObtenerCatalogo()
                            .FirstOrDefault(p => p.Nombre.ToLower().Contains(nombre.ToLower()));

                        if (buscado != null)
                        {
                            Console.WriteLine("\nEncontrado:");
                            buscado.MostrarInformacion();
                        }
                        else
                        {
                            Console.WriteLine("\nArtículo no encontrado");
                        }
                        break;

                    case 4:
                        // Lógica de Venta
                        CestaDeCompra cesta = new CestaDeCompra();
                        IEstrategiaDescuento convenioVenta = null;

                        Console.Write("\n¿La venta está asociada a un cliente registrado? (s/n): ");
                        if (Console.ReadLine().ToLower() == "s")
                        {
                            Console.Write("Nombre del cliente: ");
                            string nomCliente = Console.ReadLine();
                            var cliente = repoCliente.ObtenerClientes()
                                .FirstOrDefault(c => c.Nombre.ToLower().Contains(nomCliente.ToLower()));
                                
                            if (cliente != null)
                            {
                                convenioVenta = cliente.Convenio;
                                Console.WriteLine($"Cliente asociado: {cliente.Nombre} ({convenioVenta.NombreConvenio})");
                            }
                            else
                            {
                                Console.WriteLine("Cliente no encontrado, venta se realizará sin convenio.");
                            }
                        }

                        bool agregarMas = true;
                        while (agregarMas)
                        {
                            Console.Write("\nCódigo o Nombre del artículo: ");
                            string term = Console.ReadLine();
                            var articulo = repoCatalogo.ObtenerCatalogo()
                                .FirstOrDefault(p => p.Codigo.ToLower() == term.ToLower() || p.Nombre.ToLower().Contains(term.ToLower()));

                            if (articulo != null)
                            {
                                Console.Write("Cantidad: ");
                                if (int.TryParse(Console.ReadLine(), out int cant))
                                {
                                    try
                                    {
                                        cesta.AgregarArticulo(articulo, cant);
                                        Console.WriteLine($"{cant}x {articulo.Nombre} agregados a la cesta.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"Error: {ex.Message}");
                                        Console.ResetColor();
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Artículo no encontrado.");
                            }

                            Console.Write("¿Agregar otro artículo? (s/n): ");
                            agregarMas = Console.ReadLine().ToLower() == "s";
                        }

                        if (cesta.ObtenerArticulos().Count > 0)
                        {
                            try
                            {
                                decimal subtotal = cesta.CalcularSubtotal();
                                decimal descuento = convenioVenta != null ? convenioVenta.CalcularDescuento(subtotal) : 0;
                                decimal total = casoUsoVenta.EjecutarVenta(cesta, convenioVenta);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\n===== TICKET DE VENTA =====");
                                Console.WriteLine($"Subtotal:  ${subtotal}");
                                Console.WriteLine($"Descuento: ${descuento} ({(convenioVenta?.NombreConvenio ?? "Sin Convenio")})");
                                Console.WriteLine($"Total:     ${total}");
                                Console.WriteLine("===========================");
                                Console.ResetColor();
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\nError al procesar la venta: {ex.Message}");
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Cesta vacía. Venta cancelada.");
                        }
                        break;

                    case 5:
                        Console.Write("\nNombre del cliente: ");
                        string nc = Console.ReadLine();
                        var cli = repoCliente.ObtenerClientes()
                            .FirstOrDefault(c => c.Nombre.ToLower().Contains(nc.ToLower()));

                        if (cli != null)
                        {
                            Console.Write("Puntos a acumular: ");
                            if (int.TryParse(Console.ReadLine(), out int puntos))
                            {
                                cli.AcumularPuntos(puntos);
                                repoCliente.GuardarCliente(cli);
                                Console.WriteLine($"Puntos acumulados. Nuevo total: {cli.Puntos}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nCliente no encontrado");
                        }
                        break;

                    case 6:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nSaliendo del sistema...");
                        Console.ResetColor();
                        break;

                    default:
                        Console.WriteLine("\nOpción inválida");
                        break;
                }
            }
            Console.WriteLine("\nFIN DEL SISTEMA");
        }
    }
}