using BibFarmacia.Convenios;
using BibFarmacia.Dominio;
using BibFarmacia.Servicios;
using BibFarmacia.Ventas;

namespace AppFarmaciaConsola
{
    // Toda la interacción con el usuario vive aquí (login, menú, lectura de opciones). Program ya
    // no conoce el menú: separa la orquestación de arranque de la interacción, cerrando el
    // Punto de Dolor #1 (ver docs/Principios SOLID Argumentados.md, 1.1).
    //
    // Las opciones 1-7 preservan exactamente el mismo texto, orden y efecto que Program.cs (AS-IS).
    // La opción 8 es nueva -- SC-2, servicios médicos -- y es la única solicitud de cambio que se
    // mide formalmente en esta entrega (ver evidencia/metrica-SC2.md).
    public class MenuConsola
    {
        private readonly UsuarioService usuarioService;
        private readonly GestorInventario gestorInventario;
        private readonly GestorServiciosMedicos gestorServiciosMedicos;
        private readonly ClienteService clienteService;
        private readonly CasodeUsoProcesarVenta casoDeUsoProcesarVenta;

        public MenuConsola(
            UsuarioService usuarioService,
            GestorInventario gestorInventario,
            GestorServiciosMedicos gestorServiciosMedicos,
            ClienteService clienteService,
            CasodeUsoProcesarVenta casoDeUsoProcesarVenta)
        {
            this.usuarioService = usuarioService;
            this.gestorInventario = gestorInventario;
            this.gestorServiciosMedicos = gestorServiciosMedicos;
            this.clienteService = clienteService;
            this.casoDeUsoProcesarVenta = casoDeUsoProcesarVenta;
        }

        public void Ejecutar()
        {
            // ================= LOGIN =================

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("=========== LOGIN ===========");
            Console.ResetColor();

            Console.Write("Usuario: ");
            string user = Console.ReadLine()!;

            Console.Write("Contraseña: ");
            string password = Console.ReadLine()!;

            bool login = usuarioService.Login(user, password);

            if (!login)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAcceso denegado");
                Console.ResetColor();

                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nLogin correcto");
            Console.ResetColor();

            // ================= ALERTAS =================

            gestorInventario.VerificarStock();
            gestorInventario.VerificarVencimiento();

            // ================= MENÚ =================

            int opcion = 0;

            while (opcion != 7)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;

                Console.WriteLine("\n==============================");
                Console.WriteLine("      SISTEMA FARMACIA");
                Console.WriteLine("==============================");

                Console.ResetColor();

                Console.WriteLine("1. Ver productos");
                Console.WriteLine("2. Ver clientes");
                Console.WriteLine("3. Buscar producto");
                Console.WriteLine("4. Registrar venta");
                Console.WriteLine("5. Acumular puntos");
                Console.WriteLine("6. Ver alertas");
                Console.WriteLine("7. Salir");
                Console.WriteLine("8. Ver servicios médicos");

                Console.Write("\nSeleccione opción: ");

                opcion = int.Parse(Console.ReadLine()!);

                switch (opcion)
                {
                    case 1:
                        MostrarProductos();
                        break;

                    case 2:
                        MostrarClientes();
                        break;

                    case 3:
                        BuscarProducto();
                        break;

                    case 4:
                        RegistrarVenta();
                        break;

                    case 5:
                        AcumularPuntosCliente();
                        break;

                    case 6:
                        Console.WriteLine("\nVerificando alertas...");
                        gestorInventario.VerificarStock();
                        gestorInventario.VerificarVencimiento();
                        break;

                    case 7:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nSaliendo del sistema...");
                        Console.ResetColor();
                        break;

                    case 8:
                        MostrarServiciosMedicos();
                        break;

                    default:
                        Console.WriteLine("\nOpción inválida");
                        break;
                }
            }

            Console.WriteLine("\nFIN DEL SISTEMA");
        }

        private void MostrarProductos()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n===== PRODUCTOS =====");
            Console.ResetColor();

            Console.WriteLine("Nombre\t\tStock\tPrecio");
            Console.WriteLine("-----------------------------------");

            foreach (var producto in gestorInventario.ObtenerProductos())
            {
                Console.WriteLine($"{producto.Nombre}\t\t{producto.Stock}\t{producto.Precio}");
            }
        }

        private void MostrarClientes()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n===== CLIENTES =====");
            Console.ResetColor();

            foreach (var cliente in clienteService.ObtenerClientes())
            {
                Console.WriteLine($"{cliente.Nombre} - Puntos: {cliente.Puntos}");
            }
        }

        private void BuscarProducto()
        {
            Console.Write("\nIngrese nombre producto: ");
            string nombre = Console.ReadLine()!;

            var productoBuscado = gestorInventario.ObtenerProductos()
                .FirstOrDefault(p => p.Nombre.ToLower().Contains(nombre.ToLower()));

            if (productoBuscado != null)
            {
                Console.WriteLine($"\nProducto: {productoBuscado.Nombre}");
                Console.WriteLine($"Precio: {productoBuscado.Precio}");
                Console.WriteLine($"Stock: {productoBuscado.Stock}");
            }
            else
            {
                Console.WriteLine("\nProducto no encontrado");
            }
        }

        private void RegistrarVenta()
        {
            Console.Write("\nNombre producto: ");
            string nombreVenta = Console.ReadLine()!;

            var productoVenta = gestorInventario.ObtenerProductos()
                .FirstOrDefault(p => p.Nombre.ToLower().Contains(nombreVenta.ToLower()));

            if (productoVenta != null)
            {
                Console.Write("Cantidad: ");
                int cantidad = int.Parse(Console.ReadLine()!);

                var cesta = new CestaDeCompra();
                cesta.AgregarArticulo(productoVenta, cantidad);

                // Venta de mostrador, sin cliente asociado: igual que en el AS-IS, la opción 4 nunca
                // pedía identificar un cliente. SinConvenio garantiza que EjecutarVenta no aplique
                // ningún descuento aquí (0 % -- ver docs/Herencias y Verificacion LSP.md, 3.2), así
                // que el resultado observable es idéntico al de Program.cs (AS-IS).
                var clienteMostrador = new Cliente(string.Empty, string.Empty, string.Empty, string.Empty, new SinConvenio());

                casoDeUsoProcesarVenta.EjecutarVenta(cesta, clienteMostrador);

                Console.WriteLine("\nVenta registrada");
            }
            else
            {
                Console.WriteLine("\nProducto no encontrado");
            }
        }

        private void AcumularPuntosCliente()
        {
            Console.Write("\nNombre cliente: ");
            string nombreCliente = Console.ReadLine()!;

            var clientePuntos = clienteService.ObtenerClientes()
                .FirstOrDefault(c => c.Nombre.ToLower().Contains(nombreCliente.ToLower()));

            if (clientePuntos != null)
            {
                Console.Write("Puntos: ");
                int puntos = int.Parse(Console.ReadLine()!);

                clienteService.AcumularPuntos(clientePuntos, puntos);
            }
            else
            {
                Console.WriteLine("\nCliente no encontrado");
            }
        }

        private void MostrarServiciosMedicos()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n===== SERVICIOS MÉDICOS =====");
            Console.ResetColor();

            Console.WriteLine("Nombre\t\tPrecio");
            Console.WriteLine("-----------------------------------");

            foreach (var servicio in gestorServiciosMedicos.ObtenerServiciosMedicos())
            {
                Console.WriteLine($"{servicio.Nombre}\t\t{servicio.Precio}");
            }
        }
    }
}
