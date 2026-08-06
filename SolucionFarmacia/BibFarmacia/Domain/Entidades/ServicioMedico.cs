using System;
using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Domain.Entidades
{
    public class ServicioMedico : IVendible
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public bool AplicaImpuesto { get; set; }

        public ServicioMedico(string codigo, string nombre, decimal precio)
        {
            Codigo = codigo;
            Nombre = nombre;
            Precio = precio;
            AplicaImpuesto = false; // Servicios medicos asumen no impuesto por defecto
        }

        public void MostrarInformacion()
        {
            Console.WriteLine($"[{Codigo}] SERVICIO: {Nombre} - Precio: {Precio}");
        }
    }
}
