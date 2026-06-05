using System;
using System.Windows;

namespace GridFiller
{
    /// <summary>
    /// Representa una parte de la serpiente:
    /// - UiElement: referencia al Rectangle en el Canvas (puede ser nulo antes de crearlo).
    /// - Position: coordenadas en el Canvas.
    /// - IsHead: indica si esta parte es la cabeza (afecta apariencia).
    /// </summary>
    public class SnakePart
    {
        // Referencia a la UI (Rectangle) que representa la parte en el Canvas.
        // Nullable porque todavía puede no haberse creado el elemento visual.
        public UIElement? UiElement { get; set; }

        // Posición en el Canvas (X,Y)
        public Point Position { get; set; }

        // Indica si esta parte es la cabeza (afecta Shadow, tamaño, etc.)
        public bool IsHead { get; set; }

    }
}
