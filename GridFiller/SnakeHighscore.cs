using System;

namespace GridFiller
{
    /// <summary>
    /// Representa una entrada del ranking de puntuaciones.
    /// - Debe tener propiedades públicas para que XmlSerializer pueda serializarla/deserializarla.
    /// </summary>
    public class SnakeHighscore 
    {
        // Nombre del jugador (ej.: "ANA")
        public string PlayerName { get; set; }

        // Puntuación asociada al jugador
        public int Score { get; set; }
    }

}
