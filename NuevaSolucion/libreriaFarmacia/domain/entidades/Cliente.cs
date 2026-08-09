using libreriaFarmacia.domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class Cliente: Persona
    {
        //Atributos
        private int puntos;
        private Asociacion asociacion;

        //Constructores
        public Cliente(int id, string nombre, string cedula, string telefono, string correo, int puntos, Asociacion asociacion)
        : base(id, nombre, cedula, telefono, correo)
        {
            Puntos = puntos;
            Asociacion = asociacion;
        }

        public Cliente(
        string nombre,
        string cedula,
        string telefono,
        string correo)
        : base(nombre, cedula, telefono, correo)
        {
            Puntos = 0;
        }

        //Getters Setters
        public int Puntos { get => puntos;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Los puntos no pueden ser negativos");
                }
                puntos = value;
            }
        }

        internal Asociacion Asociacion { get => asociacion; set => asociacion = value; }

        //Metodos
        public void aumentarPuntos(int cantidad) {
            Puntos += cantidad;
        }

        public void disminuirPuntos(int cantidad) {
            if (puntos - cantidad < 0)
                throw new Exception("No tiene suficientes puntos");
            Puntos -= cantidad;
        }
    }
}
