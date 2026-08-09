using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public abstract class Producto : IVendible
    {
        //Atributos
        protected int id;
        protected string nombre;
        protected IEmpresa empresa;
        protected float precio;
        protected int stock;
        protected int stockMinimo;
        protected int plazoVencimientoDias;
        protected DateTime fechaVencimiento;

        public Producto(int id, string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias, DateTime fechaVencimiento)
        {
            Id = id;
            Nombre = nombre;
            Empresa = empresa;
            Precio = precio;
            Stock = stock;
            StockMinimo = stockMinimo;
            PlazoVencimientoDias = plazoVencimientoDias;
            FechaVencimiento = fechaVencimiento;
        }

        //Constructores
        public Producto(string nombre, IEmpresa empresa, float precio, int stock, int stockMinimo, int plazoVencimientoDias)
        {
            Nombre = nombre;    
            Empresa = empresa;
            Precio = precio;
            Stock = stock;
            StockMinimo = stockMinimo;
            PlazoVencimientoDias = plazoVencimientoDias;
            colocarFechaVencimiento();
        }



        //Getters Setters
        public int Id { get => id; 
            set {
                id = value < 0 ? throw new ArgumentException("Id invalido") : value;
            } 
        }
        public string Nombre { get => nombre;
            set {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío");

                nombre = value;
            }
        }
        public float Precio { get => precio;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El precio no puede ser negativo");
                precio = value;
            }
        }
        public int Stock { get => stock;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El stock no puede ser negativo");
                stock = value;
            }
        }
        public int StockMinimo { get => stockMinimo;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El stock minimo no puede ser negativo");
                stockMinimo = value;
            }
        }
        public DateTime FechaVencimiento { get => fechaVencimiento; 
            set {
                if (value < DateTime.Today)
                    throw new ArgumentException("Fecha invalida");
                fechaVencimiento = value;
            } 
        }
        public int PlazoVencimientoDias { get => plazoVencimientoDias;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El plazo de vencimiento en dias no puede ser negativo");
                plazoVencimientoDias = value;
            }
        }
        public IEmpresa Empresa
        {
            get => empresa;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                empresa = value;
            }
        }

        //Metodos
        public void colocarFechaVencimiento() {
            FechaVencimiento = DateTime.Today.AddDays(PlazoVencimientoDias);
        }

        public void aumentarStock(int cantidad) {
            Stock += cantidad;
        }

        public void disminuirStock(int cantidad) {
            if (stock < cantidad)
                throw new Exception("No hay suficiente Stock");
            Stock -= cantidad; 
         }
    }
}
