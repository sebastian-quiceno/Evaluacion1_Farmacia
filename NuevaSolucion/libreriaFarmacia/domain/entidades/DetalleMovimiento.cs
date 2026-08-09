using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace libreriaFarmacia.domain.entidades
{
    public class DetalleMovimiento
    {
        //Atributos
        private int id;
        private IVendible vendible;
        private int cantidad;
        private float subtotal;

        //Constructores
        public DetalleMovimiento(IVendible vendible, int cantidad, int id = 0)
        {
            Vendible = vendible;
            Cantidad = cantidad;

            calcularSubtotal();
            this.id = id;
        }

        //Getters Setters
        public IVendible Vendible { get => vendible;
            set {
                if (value is null)
                    throw new ArgumentException("El Vendible no puede ser nulo en Detalle Movimiento");
                vendible = value;
            }
        }
        public int Cantidad { get => cantidad;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("La cantidad en Detalle Movimiento no puede ser negativa o cero");
                cantidad = value;
            } 
        }
        public float Subtotal { get => subtotal;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("El subtotal de Detalle Movimiento no puede ser negativo");
                subtotal = value;
            }
        }

        public int Id { get => id; set => id = value; }

        //Metodos
        public void calcularSubtotal() {
            Subtotal = vendible.Precio * cantidad;
        }

        public void aumentarCantidad(int cantidad) {
            Cantidad += cantidad;
        }

        public void disminuirCantidad(int cantidad)
        {
            if (Cantidad - cantidad <= 0)
                throw new ArgumentOutOfRangeException("No se puede quitar esa cantidad en DetalleMovimiento");
            Cantidad -= cantidad;
        }

    }
}
