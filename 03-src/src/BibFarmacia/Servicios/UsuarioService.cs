using BibFarmacia.Dominio;

namespace BibFarmacia.Servicios
{
    public class UsuarioService
    {
        private List<Usuario> usuarios;

        public UsuarioService()
        {
            usuarios = new List<Usuario>();
        }

        // Réplica de ServicioUsuario.Cargar (AS-IS): mismas columnas, mismo mensaje de retorno.
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

                    Usuario usuario = new Usuario(
                        datos[0],
                        datos[1],
                        datos[2],
                        datos[3],
                        datos[4],
                        datos[5]);

                    usuarios.Add(usuario);
                }

                return "Usuarios cargados";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<Usuario> ObtenerUsuarios()
        {
            return usuarios;
        }

        // Réplica de ServicioUsuario.Login + AspectoAutenticacion.Login (AS-IS): misma comparación
        // exacta. No se introduce una abstracción IAutenticador -- ninguna SC exige un segundo
        // mecanismo de autenticación en esta entrega (H-09, deuda técnica consciente, ver ADR).
        public bool Login(string user, string password)
        {
            return usuarios.Any(u => u.NombreUsuario == user && u.Contrasena == password);
        }
    }
}
