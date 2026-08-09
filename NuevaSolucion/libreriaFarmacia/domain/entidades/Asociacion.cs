using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class Asociacion
    {
        private int id;
        private string nombre;

        public Asociacion(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public int Id { get => id; set => id = value; }
        public string Nombre { get => nombre; set => nombre = value; }
    }
}
