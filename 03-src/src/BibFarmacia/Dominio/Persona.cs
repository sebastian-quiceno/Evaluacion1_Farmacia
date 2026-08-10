namespace BibFarmacia.Dominio
{
    public abstract class Persona
    {
        protected string nombre;
        protected string cedula;
        protected string telefono;
        protected string correo;

        protected Persona(string nombre, string cedula, string telefono, string correo)
        {
            this.nombre = nombre;
            this.cedula = cedula;
            this.telefono = telefono;
            this.correo = correo;
        }

        public string Nombre => nombre;
        public string Cedula => cedula;
        public string Telefono => telefono;
        public string Correo => correo;
    }
}
