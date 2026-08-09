using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class PrestacionFarmaceutica : IVendible
    {
        //Atributos
        private int id; 
        private string nombre;
        private float precio;

        //Constructores
        public PrestacionFarmaceutica(int id, string nombre, float precio)
        {
            this.id = id;
            this.nombre = nombre;
            this.precio = precio;
        }

        //Getters Setters
        public int Id { get => id;
            set {
                if (value < 0)
                    throw new ArgumentException("El id de la prestacion farmaceutica no puede ser negativo");
                id = value;
            } 
        }
        public string Nombre
        {
            get => nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío");

                nombre = value;
            }
        }
        public float Precio { get => precio;
            set {
                if (value < 0)
                    throw new ArgumentException("El precio de la prestacion farmaceutica no puede ser negativo");
                precio = value;
            }
        }

        //Metodos  
    }
}
