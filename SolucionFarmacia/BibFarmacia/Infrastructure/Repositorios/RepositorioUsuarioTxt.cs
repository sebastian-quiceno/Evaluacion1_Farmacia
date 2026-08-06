using System.Collections.Generic;
using System.IO;
using BibFarmacia.Application.Interfaces;
using BibFarmacia.Domain.Entidades;

namespace BibFarmacia.Infrastructure.Repositorios
{
    public class RepositorioUsuarioTxt : IRepositorioUsuario
    {
        private readonly string rutaArchivo;

        public RepositorioUsuarioTxt(string rutaArchivo)
        {
            this.rutaArchivo = rutaArchivo;
        }

        public List<Usuario> ObtenerUsuarios()
        {
            var usuarios = new List<Usuario>();

            if (!File.Exists(rutaArchivo)) return usuarios;

            string[] lineas = File.ReadAllLines(rutaArchivo);
            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                string[] datos = linea.Split(';');
                usuarios.Add(new Usuario(datos[0], datos[1], datos[2], datos[3], datos[4], datos[5]));
            }
            return usuarios;
        }
    }
}
