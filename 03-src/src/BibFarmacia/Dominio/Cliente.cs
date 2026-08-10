using BibFarmacia.Convenios;

namespace BibFarmacia.Dominio
{
    public class Cliente : Persona
    {
        private int puntos;
        private Convenio convenio;

        public Cliente(string nombre, string cedula, string telefono, string correo, Convenio convenio)
            : base(nombre, cedula, telefono, correo)
        {
            puntos = 0;
            this.convenio = convenio;
        }

        public int Puntos => puntos;
        public Convenio Convenio => convenio;

        public void AcumularPuntos(int puntos)
        {
            this.puntos += puntos;
        }
    }
}
