using BibFarmacia.Dominio;

namespace BibFarmacia.Servicios
{
    // SC-2: 100% clase nueva, sin equivalente en el AS-IS (ver evidencia/metrica-SC2.md). Separada
    // de GestorInventario porque un ServicioMedico no es inventariable -- mezclarlo habría obligado a
    // GestorInventario a manejar dos colecciones con reglas distintas.
    public class GestorServiciosMedicos
    {
        private List<ServicioMedico> serviciosMedicos;

        public GestorServiciosMedicos()
        {
            serviciosMedicos = new List<ServicioMedico>();
        }

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

                    ServicioMedico servicio = new ServicioMedico(
                        codigo: datos[0],
                        nombre: datos[1],
                        precio: decimal.Parse(datos[2]));

                    serviciosMedicos.Add(servicio);
                }

                return "Servicios médicos cargados";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<ServicioMedico> ObtenerServiciosMedicos()
        {
            return serviciosMedicos;
        }
    }
}
