using BibFarmacia.Enums;
using BibFarmacia.Interfaces;

namespace BibFarmacia.Notificacion
{
    // Réplica exacta de los 4 colores que Program.cs (AS-IS) asignaba a cada Evento*.Disparar,
    // ahora centralizados aquí en vez de repetidos como 4 suscripciones de lambda en Program.cs.
    public class NotificadorConsola : INotificador
    {
        public void Notificar(string mensaje, TipoNotificacion tipo)
        {
            Console.ForegroundColor = ColorPara(tipo);

            Console.WriteLine(mensaje);

            Console.ResetColor();
        }

        private static ConsoleColor ColorPara(TipoNotificacion tipo)
        {
            return tipo switch
            {
                TipoNotificacion.StockMinimo => ConsoleColor.Red,
                TipoNotificacion.Vencimiento => ConsoleColor.Yellow,
                TipoNotificacion.PuntosAcumulados => ConsoleColor.Green,
                TipoNotificacion.MovimientoRegistrado => ConsoleColor.Cyan,
                _ => ConsoleColor.White
            };
        }
    }
}
