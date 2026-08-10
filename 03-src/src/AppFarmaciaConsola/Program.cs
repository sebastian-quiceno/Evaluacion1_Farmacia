using BibFarmacia.Interfaces;
using BibFarmacia.Notificacion;
using BibFarmacia.Servicios;
using BibFarmacia.Ventas;

namespace AppFarmaciaConsola
{
    // Único punto del programa donde se construyen e inyectan los objetos (composition root).
    // Ya no conoce el menú: eso vive en MenuConsola. Cierra el Punto de Dolor #1 (antes, un único
    // Program.cs hacía tanto el cableado de dependencias como toda la interacción de consola).
    public class Program
    {
        private INotificador notificador = null!;
        private GestorInventario gestorInventario = null!;
        private GestorServiciosMedicos gestorServiciosMedicos = null!;
        private ClienteService clienteService = null!;
        private UsuarioService usuarioService = null!;
        private MovimientoService movimientoService = null!;
        private CasodeUsoProcesarVenta casoDeUsoProcesarVenta = null!;
        private MenuConsola menuConsola = null!;

        public static void Main(string[] args)
        {
            Console.Title = "Sistema Farmacia";

            var program = new Program();

            program.ConstruirDependencias();
            program.SuscribirManejadoresDeEventos();

            program.menuConsola.Ejecutar();
        }

        private void ConstruirDependencias()
        {
            notificador = new NotificadorConsola();

            gestorInventario = new GestorInventario(notificador);
            gestorServiciosMedicos = new GestorServiciosMedicos();
            clienteService = new ClienteService(notificador);
            usuarioService = new UsuarioService();
            movimientoService = new MovimientoService(notificador);
            casoDeUsoProcesarVenta = new CasodeUsoProcesarVenta();

            menuConsola = new MenuConsola(
                usuarioService,
                gestorInventario,
                gestorServiciosMedicos,
                clienteService,
                casoDeUsoProcesarVenta);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Cargando información del sistema...\n");
            Console.ResetColor();

            Console.WriteLine(gestorInventario.CargarDesdeArchivo("productos.txt"));
            Console.WriteLine(clienteService.Cargar("clientes.txt"));
            Console.WriteLine(usuarioService.Cargar("usuarios.txt"));
            Console.WriteLine(gestorServiciosMedicos.CargarDesdeArchivo("serviciosmedicos.txt"));

            Console.WriteLine();
        }

        // CasodeUsoProcesarVenta no conoce a GestorInventario ni a MovimientoService: solo dispara
        // EventoVentaProcesada. Es este método -- y solo este -- el que conecta quién reacciona a la
        // venta (ver docs/Inversion de Dependencias (DIP).md).
        private void SuscribirManejadoresDeEventos()
        {
            casoDeUsoProcesarVenta.VentaProcesada += gestorInventario.AlProcesarVenta;
            casoDeUsoProcesarVenta.VentaProcesada += movimientoService.AlProcesarVenta;
        }
    }
}
