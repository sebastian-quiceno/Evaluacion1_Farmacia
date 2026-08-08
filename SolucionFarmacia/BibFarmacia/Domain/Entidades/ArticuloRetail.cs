using System;
using BibFarmacia.Domain.Interfaces;

namespace BibFarmacia.Domain.Entidades
{
    public class ArticuloRetail : ProductoBase
    {
        public ArticuloRetail(string codigo, string nombre, decimal precio, int stock, int stockMinimo)
            : base(codigo, nombre, precio, stock, stockMinimo, aplicaImpuesto: true)
        {
        }

        //Se deberia sobrescribir toString(), esta quemando el retorno
        public override void MostrarInformacion()
        {
            base.MostrarInformacion();
            Console.WriteLine("Tipo: Articulo Retail (Cosmético/Comestible)");
        }
    }
}
