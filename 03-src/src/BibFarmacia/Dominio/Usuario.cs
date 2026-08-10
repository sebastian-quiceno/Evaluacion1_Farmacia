namespace BibFarmacia.Dominio
{
    public class Usuario : Persona
    {
        private string usuario;
        private string contrasena;

        public Usuario(string nombre, string cedula, string telefono, string correo, string usuario, string contrasena)
            : base(nombre, cedula, telefono, correo)
        {
            this.usuario = usuario;
            this.contrasena = contrasena;
        }

        public string NombreUsuario => usuario;
        public string Contrasena => contrasena;
    }
}
