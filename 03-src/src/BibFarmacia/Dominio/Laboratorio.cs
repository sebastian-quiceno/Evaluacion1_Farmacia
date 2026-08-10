namespace BibFarmacia.Dominio
{
    public class Laboratorio
    {
        private string nombre;
        private string direccion;
        private string telefono;

        public Laboratorio(string nombre, string direccion, string telefono)
        {
            this.nombre = nombre;
            this.direccion = direccion;
            this.telefono = telefono;
        }

        public string Nombre => nombre;
        public string Direccion => direccion;
        public string Telefono => telefono;
    }
}
