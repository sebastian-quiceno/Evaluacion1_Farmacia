using System;

namespace BibFarmacia.Domain.Entidades
{
    public abstract class Persona
    {
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }

        protected Persona(string nombre, string cedula, string telefono, string correo)
        {
            Nombre = nombre;
            Cedula = cedula;
            Telefono = telefono;
            Correo = correo;
        }
    }
}
