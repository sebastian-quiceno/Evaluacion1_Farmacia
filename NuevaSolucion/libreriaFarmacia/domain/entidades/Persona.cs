using libreriaFarmacia.domain.Constants;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public abstract class Persona
    {
        //Atributos
        private int id; 
        private string nombre;
        private string cedula;
        private string telefono;
        private string correo;

        //Constructor
        protected Persona(int id, string nombre, string cedula, string telefono, string correo)
        {
            Id = id;
            Nombre = nombre;
            Cedula = cedula;
            Telefono = telefono;
            Correo = correo;
        }

        protected Persona(string nombre, string cedula, string telefono, string correo)
        {
            Nombre = nombre;
            Cedula = cedula;
            Telefono = telefono;
            Correo = correo;
        }

        //Setters y Getters
        public int Id { get; protected set; }
        public string Nombre { get => nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío");

                nombre = value;
            }
        }
        public string Cedula { get => cedula; 
            set {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La cédula no puede estar vacía");

                if (!value.All(char.IsDigit))
                    throw new ArgumentException("La cédula solo puede contener números");
                
                if(value.Length < ValidacionesPersona.CedulaLongitudMinima || value.Length > ValidacionesPersona.CedulaLongitudMaxima)
                    throw new ArgumentException("la cedula no tiene la longitud permitida");
                
                cedula = value;
            } 
        }
        public string Telefono { get => telefono;
            set {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El numero no puede estar vacío");

                if (!value.All(char.IsDigit))
                    throw new ArgumentException("El numero solo puede contener números");
                
                if (value.Length != ValidacionesPersona.TelefonoLongitudMaxima)
                    throw new ArgumentException("Numero de telefono no valido, no cumple con la longitud");
                
                telefono = value;
            }
        }
        public string Correo { get => correo;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El correo no puede estar vacío");

                try
                {
                    var mail = new MailAddress(value);

                    if (mail.Address != value)
                        throw new ArgumentException("El formato del correo no es válido");

                    correo = value;
                }
                catch (FormatException)
                {
                    throw new ArgumentException("El formato del correo no es válido");
                }
            }
        }
    }
}
