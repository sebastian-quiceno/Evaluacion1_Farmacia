using libreriaFarmacia.domain.enums;
using libreriaFarmacia.domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace libreriaFarmacia.domain.entidades
{
    public class Movimiento
    {
        //Atributos
        private int id;
        private Cliente cliente;
        private List<DetalleMovimiento> listaDetallesMovimientos;
        private EstadosCompra estadoCompra;
        private float descuento;
        private float subtotal;
        private float total;
        private DateTime fecha;

        //Constructores
        public Movimiento(int id, Cliente cliente, List<DetalleMovimiento> listaDetallesMovimientos, EstadosCompra estadoCompra, float descuento, float subtotal, float total, DateTime fecha)
        {
            this.id = id;
            this.cliente = cliente;
            this.listaDetallesMovimientos = listaDetallesMovimientos;
            this.estadoCompra = estadoCompra;
            this.descuento = descuento;
            this.subtotal = subtotal;
            this.total = total;
            this.fecha = fecha;
        }

        public Movimiento(Cliente cliente)
        {
            this.cliente = cliente;
            listaDetallesMovimientos = new List<DetalleMovimiento>();
            estadoCompra = EstadosCompra.Pendiente;
            fecha = DateTime.Today;
        }

        //Getters Setters
        public int Id { get => id;
            set {
                if (value < 0)
                    throw new ArgumentException("El id de Movimiento no puede ser negativo");
                id = value;
            } 
        }
        public Cliente Cliente { get => cliente;
            set {
                if (value is null)
                    throw new ArgumentException("El cliente de Movimiento no puede ser null");
                cliente = value;
            } 
        
        }
        public List<DetalleMovimiento> ListaDetallesMovimientos { get => listaDetallesMovimientos; set => listaDetallesMovimientos = value; }
        public EstadosCompra EstadoCompra { get => estadoCompra; set => estadoCompra = value; }
        public float Descuento { get => descuento; set => descuento = value; }
        public float Subtotal { get => subtotal; set => subtotal = value; }
        public float Total { get => total; set => total = value; }
        public DateTime Fecha { get => fecha; set => fecha = value; }

        //Metodos
        public void agregarVendible(IVendible vendible, int cantidad) {
            if (estadoCompra != EstadosCompra.Pendiente)
                throw new InvalidOperationException("No se puede modificar un movimiento que se haya confirmado o cancelado");
            
            if (cantidad <= 0)
                throw new InvalidOperationException("La cantidad no puede ser negativa");

            if (vendible is null)
                throw new InvalidOperationException("El vendible no puede ser null");

            var detalle = listaDetallesMovimientos.FirstOrDefault(d => d.Vendible.Id == vendible.Id);

            if (detalle != null)
            {
                detalle.aumentarCantidad(cantidad);
                return;
            }

            listaDetallesMovimientos.Add(new DetalleMovimiento(vendible, cantidad));
        }

        public void agregarDescuento(float descuento) {

            if (estadoCompra != EstadosCompra.Pendiente)
                throw new InvalidOperationException("No se puede modificar un movimiento que se haya confirmado o cancelado");

            //if (descuento > 1 || descuento <= 0)
            //    throw new ArgumentException("El descuento no puede ser negativo o mayor al 100%");

            if (descuento < 0)
                throw new ArgumentException(
                    "El descuento no puede ser negativo.");

            if (descuento > Subtotal)
                throw new ArgumentException(
                    "El descuento no puede superar el subtotal.");

            Descuento = descuento;
        }

        public void confirmarMovimiento() {
            if (listaDetallesMovimientos.Any())
                throw new InvalidOperationException(
                    "No se puede confirmar un movimiento vacío.");

            if (EstadoCompra != EstadosCompra.Pendiente)
                throw new InvalidOperationException(
                    "El movimiento ya fue procesado.");

            EstadoCompra = EstadosCompra.Aprovada;
        }




    }
}
