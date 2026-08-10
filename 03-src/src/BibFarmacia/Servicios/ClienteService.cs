using BibFarmacia.Convenios;
using BibFarmacia.Dominio;
using BibFarmacia.Enums;
using BibFarmacia.Interfaces;

namespace BibFarmacia.Servicios
{
    public class ClienteService
    {
        private List<Cliente> clientes;
        private INotificador notificador;

        public ClienteService(INotificador notificador)
        {
            clientes = new List<Cliente>();
            this.notificador = notificador;
        }

        // Réplica de ServicioCliente.Cargar (AS-IS): mismas columnas, mismo mensaje de retorno.
        // clientes.txt no tiene columna de convenio -- todo cliente cargado desde archivo hoy recibe
        // SinConvenio, que garantiza el invariante "0 clientes existentes ven un comportamiento nuevo"
        // (ver docs/Herencias y Verificacion LSP.md, 3.2). Es el único lugar del sistema que menciona
        // SinConvenio/ConvenioUniversidad/ConvenioEmpresa por su nombre concreto.
        public string Cargar(string ruta)
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

                    Cliente cliente = new Cliente(
                        datos[0],
                        datos[1],
                        datos[2],
                        datos[3],
                        new SinConvenio());

                    clientes.Add(cliente);
                }

                return "Clientes cargados";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<Cliente> ObtenerClientes()
        {
            return clientes;
        }

        // Réplica de ServicioCliente.AcumularPuntos (AS-IS), pero delegando en Cliente.AcumularPuntos()
        // en vez de mutar cliente.Puntos directamente -- cierra la duplicación de regla que era H-0x
        // (Cliente.AcumularPuntos era código muerto en el AS-IS; aquí es el único punto de mutación).
        public void AcumularPuntos(Cliente cliente, int puntos)
        {
            cliente.AcumularPuntos(puntos);

            notificador.Notificar(
                $"Cliente {cliente.Nombre} acumuló {puntos} puntos",
                TipoNotificacion.PuntosAcumulados);
        }
    }
}
