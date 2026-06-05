using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Windows.Media.Effects;

namespace GridFiller
{
    /// <summary>
    /// Lógica principal de la ventana del juego.
    /// Comentarios incluidos para explicar campos, constantes, flujo de inicio/fin de partida y puntos sensibles.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Temporizador del juego que controla los "ticks" (movimientos periódicos)
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();

        // Generador de números aleatorios (puede usarse para elementos tipo comida si se añade)
        private Random rnd = new Random();

        // Tamaño en píxeles de cada casilla (cuadrado) de la serpiente
        const int SnakeSquareSize = 20;

        // Longitud inicial (no usada para "rellenar" aquí; se crea manualmente en StartNewGame)
        const int SnakeStartLength = 3;

        // Intervalo inicial en ms entre ticks (velocidad)
        const int SnakeStartSpeed = 400;

        // Umbral de velocidad (no usado actualmente pero definido para futura lógica)
        const int SnakeSpeedThreshold = 100;

        // Nuevas constantes: cada cuántos bloques aplicar el incremento y cuánto (ms)
        const int BlocksPerSpeedIncrease = 5;   // cada 5 bloques
        const int SpeedIncreaseMs = 5;          // disminuir el intervalo en 5 ms (hacerlo más rápido)

        // Pinceles para cuerpo y cabeza de la serpiente.
        // Actualmente ambos son negro. Mantener separados por si se desea diferenciarlos.
        private SolidColorBrush snakeBodyBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)); // negro
        private SolidColorBrush snakeHeadBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)); // cabeza también negro

        // Lista de partes de la serpiente (ordenadas de cola -> cabeza)
        private List<SnakePart> snakeParts = new List<SnakePart>();

        // Máximo número de entradas en el ranking
        const int MaxHighscoreListEntryCount = 5;

        // Dirección actual de la serpiente
        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;

        // Puntuación actual (incrementa en cada movimiento en este diseño)
        private int currentScore = 0;

        public MainWindow()
        {
            InitializeComponent();

            // Suscribimos el tick del temporizador al handler
            gameTickTimer.Tick += GameTickTimer_Tick;

            // Cargamos la lista de récords desde disco al iniciar
            LoadHighscoreList();
        }

        // Handler del temporizador: mueve la serpiente cada tick
        private void GameTickTimer_Tick(object? sender, EventArgs e)
        {
            MoveSnake();
        }

        // Después de que la ventana se ha renderizado por primera vez, dibujamos el área
        private void Window_ContentRendered(object? sender, EventArgs e)
        {
            DrawGameArea();
            // StartNewGame(); // opcional: si quieres iniciar automáticamente
        }

        // Limpia y prepara el área de juego (Canvas)
        private void DrawGameArea()
        {
            // Fondo blanco (ya declarado en XAML) — se reafirma aquí por si se cambia en runtime.
            GameArea.Background = Brushes.White;

            // Eliminamos rectángulos que no tengan Name ni Tag (supone que son fondos obsoletos creados anteriormente).
            // Esto preserva elementos UI identificables y elimina basura visual.
            for (int i = GameArea.Children.Count - 1; i >= 0; i--)
            {
                if (GameArea.Children[i] is Rectangle rect)
                {
                    // Si el rectángulo tiene Tag o Name, lo consideramos persistente y no lo eliminamos.
                    if (string.IsNullOrEmpty(rect.Name) && rect.Tag == null)
                        GameArea.Children.RemoveAt(i);
                }
            }
        }

        // Dibuja cada parte de la serpiente en el Canvas.
        // Si la parte no tiene UiElement, crea un Rectangle, lo configura y lo añade.
        // Si ya existe, actualiza su posición y efectos.
        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                var rect = snakePart.UiElement as Rectangle;
                if (rect == null)
                {
                    // Crear efecto "glow" (sombra) para emular neon negro.
                    DropShadowEffect glow = new DropShadowEffect
                    {
                        Color = Color.FromArgb(220, 0, 0, 0), // negro con alpha
                        BlurRadius = snakePart.IsHead ? 16 : 10,
                        ShadowDepth = 0,
                        Opacity = snakePart.IsHead ? 0.95 : 0.85
                    };

                    rect = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush),
                        Effect = glow
                    };

                    // Marcamos los elementos de la serpiente para identificarlos (evitar borrarlos en DrawGameArea)
                    rect.Tag = "SnakePart";

                    snakePart.UiElement = rect;
                    GameArea.Children.Add(rect);
                    Canvas.SetTop(rect, snakePart.Position.Y);
                    Canvas.SetLeft(rect, snakePart.Position.X);
                }
                else
                {
                    // Actualizar posición y aspecto del rectángulo ya existente
                    Canvas.SetTop(rect, snakePart.Position.Y);
                    Canvas.SetLeft(rect, snakePart.Position.X);
                    rect.Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush);

                    // Ajustar el efecto si ha cambiado el estado (cabeza/cuerpo)
                    if (rect.Effect is DropShadowEffect existingGlow)
                    {
                        existingGlow.Color = Color.FromArgb(220, 0, 0, 0);
                        existingGlow.BlurRadius = snakePart.IsHead ? 16 : 10;
                        existingGlow.Opacity = snakePart.IsHead ? 0.95 : 0.85;
                    }
                    else
                    {
                        rect.Effect = new DropShadowEffect
                        {
                            Color = Color.FromArgb(220, 0, 0, 0),
                            BlurRadius = snakePart.IsHead ? 16 : 10,
                            ShadowDepth = 0,
                            Opacity = snakePart.IsHead ? 0.95 : 0.85
                        };
                    }
                }
            }
        }

        // Lógica de movimiento de la serpiente por tick.
        // NOTA: en este diseño la serpiente "crece" en cada movimiento (no se elimina la cola).
        private void MoveSnake()
        {
            // Marcar todas las partes actuales como cuerpo (no cabeza)
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement is Rectangle rect)
                    rect.Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            // Coger la cabeza actual y calcular la nueva posición según la dirección
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            // Añadir una nueva cabeza (crecimiento)
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            // Dibujar la serpiente: se crearán rectángulos para las nuevas partes si procede
            DrawSnake();

            // Incrementar la puntuación en cada movimiento (comportamiento intencional de "cubrir" el tablero)
            currentScore++;

            // Ajustar la velocidad según la puntuación: cada BlocksPerSpeedIncrease bloques, disminuir el intervalo en SpeedIncreaseMs
            UpdateSpeedBasedOnScore();

            // Asegurarse de no mostrar el diálogo de nuevo récord durante la partida
            bdrNewHighscore.Visibility = Visibility.Collapsed;

            // Actualizar UI de estado (barra superior)
            UpdateGameStatus();

            // Comprobar colisiones (paredes y auto-colisión)
            DoCollisionCheck();
        }

        // Ajusta el intervalo del temporizador en función de la puntuación.
        // Cada BlocksPerSpeedIncrease bloques se reduce el intervalo en SpeedIncreaseMs,
        // hasta el límite mínimo definido por SnakeSpeedThreshold.
        private void UpdateSpeedBasedOnScore()
        {
            if (BlocksPerSpeedIncrease <= 0) return;

            // Cuántos "pasos" de incremento hemos superado
            int increments = currentScore / BlocksPerSpeedIncrease;

            // Nuevo intervalo calculado a partir del inicio
            int newIntervalMs = SnakeStartSpeed - (increments * SpeedIncreaseMs);

            // Respetar el umbral mínimo (no reducir por debajo de éste)
            if (newIntervalMs < SnakeSpeedThreshold)
                newIntervalMs = SnakeSpeedThreshold;

            // Aplicar sólo si ha cambiado para evitar reasignaciones innecesarias
            int currentIntervalMs = (int)gameTickTimer.Interval.TotalMilliseconds;
            if (currentIntervalMs != newIntervalMs)
            {
                gameTickTimer.Interval = TimeSpan.FromMilliseconds(newIntervalMs);
            }
        }

        // Inicializa y reinicia el estado para una nueva partida
        private void StartNewGame()
        {
            // Oculta diálogos y menús
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;
            bdrNewHighscore.Visibility = Visibility.Collapsed;

            // Eliminar rectángulos antiguos de la serpiente del Canvas
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();

            // Reset de estado
            currentScore = 0;
            snakeDirection = SnakeDirection.Right;

            // Añadimos la cabeza inicial en una posición fija (5,5) en multiplicador de casillas
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });

            // Establecer intervalo inicial del temporizador
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            // Dibujar la serpiente inicial
            DrawSnake();

            // Actualizar indicadores en UI
            UpdateGameStatus();

            // Arrancar el temporizador (comienza la partida)
            gameTickTimer.IsEnabled = true;
        }

        // Handler de teclas: cambia dirección o inicia partida con ESPACIO.
        // Se usa KeyUp actualmente; podrías considerar KeyDown para mayor reactividad.
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            // Si la dirección cambió por la tecla, avanzamos la serpiente inmediatamente.
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }

        // Comprueba colisiones de la cabeza con paredes o con cualquier parte del cuerpo
        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            // Colisión con las fronteras del Canvas (fuera del área)
            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
            (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            // Auto-colisión: comparar la cabeza con cada parte excepto la propia cabeza.
            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }

        // Actualiza los textos de estado y el título de la ventana
        private void UpdateGameStatus()
        {
            // Calcula el número total de casillas (cols * rows)
            int cols = Math.Max(1, (int)(GameArea.ActualWidth / SnakeSquareSize));
            int rows = Math.Max(1, (int)(GameArea.ActualHeight / SnakeSquareSize));
            int totalSquares = cols * rows;

            // Muestra "actual/total" en la barra de estado
            this.tbStatusScore.Text = $"{currentScore}/{totalSquares}";

            // Muestra el intervalo del temporizador en ms
            this.tbStatusSpeed.Text = ((int)gameTickTimer.Interval.TotalMilliseconds).ToString();

            // Actualiza el título (opcional)
            this.Title = $"SnakeWPF - Puntuación: {currentScore}/{totalSquares}";
        }

        // Finaliza la partida y muestra el diálogo correspondiente (nuevo récord o fin de juego)
        private void EndGame()
        {
            bool isNewHighscore = false;
            if (currentScore > 0)
            {
                int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
                // Si la puntuación es mejor que el mínimo del ranking o hay espacio, mostrar diálogo para añadir récord.
                if ((currentScore > lowestHighscore) || (this.HighscoreList.Count < MaxHighscoreListEntryCount))
                {
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }
            if (!isNewHighscore)
            {
                tbFinalScore.Text = currentScore.ToString();
                bdrEndOfGame.Visibility = Visibility.Visible;
            }
            // Parar el temporizador (detener la partida)
            gameTickTimer.IsEnabled = false;
        }

        // Permite arrastrar la ventana personalizada (WindowStyle=None)
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Cierra la aplicación (botón X)
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Botón "Jugar" en el menú principal: oculta el menú y empieza la partida
        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            bdrMainMenu.Visibility = Visibility.Collapsed;
            StartNewGame();
        }

        // Mostrar lista de récords desde menú principal
        private void BtnViewHighscores_Click(object sender, RoutedEventArgs e)
        {
            bdrMainMenu.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        // Volver al menú principal desde la lista de récords
        private void BtnBackToMenuFromHighscores_Click(object sender, RoutedEventArgs e)
        {
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrMainMenu.Visibility = Visibility.Visible;
        }

        // Mostrar la lista de récords desde la pantalla de bienvenida
        private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        // Colección observable que guarda los récords. Enlazada desde XAML con CollectionViewSource.
        public ObservableCollection<SnakeHighscore> HighscoreList
        {
            get; set;
        } = new ObservableCollection<SnakeHighscore>();

        // Reordena la ObservableCollection por Score descendente.
        // Esto asegura que la vista siempre muestre los mejores por encima.
        private void ReorderHighscores()
        {
            var sorted = this.HighscoreList.OrderByDescending(h => h.Score).ToList();
            this.HighscoreList.Clear();
            foreach (var h in sorted)
                this.HighscoreList.Add(h);
        }

        // Carga la lista de récords desde el archivo XML.
        // Observación importante: aquí se deserializa a List<SnakeHighscore>.
        private void LoadHighscoreList()
        {
            if (File.Exists("snake_highscorelist.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream("snake_highscorelist.xml", FileMode.Open))
                {
                    var tempList = serializer.Deserialize(reader) as List<SnakeHighscore>;
                    if (tempList == null)
                        return;

                    this.HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                        this.HighscoreList.Add(item);
                }

                // Asegurar orden correcto en la colección enlazada
                ReorderHighscores();
            }
        }

        // Guarda la lista de récords en disco.
        // Atención: actualmente serializa ObservableCollection<SnakeHighscore> mientras LoadHighscoreList deserializa List<SnakeHighscore>.
        // Esto puede provocar incompatibilidades. Recomiendo usar el mismo tipo en ambos métodos (por ejemplo List<SnakeHighscore>).
        private void SaveHighscoreList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream("snake_highscorelist.xml", FileMode.Create))
            {
                serializer.Serialize(writer, this.HighscoreList);
            }
        }

        // Añade la nueva entrada de récord desde el diálogo "Nuevo récord"
        private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = 0;
            // Calcula posición de inserción (intento de insertar en el lugar correcto)
            if ((this.HighscoreList.Count > 0) && (currentScore < this.HighscoreList.Max(x => x.Score)))
            {
                SnakeHighscore justAbove = this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentScore);
                if (justAbove != null)
                    newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
            }
            // Crear e insertar la nueva entrada
            this.HighscoreList.Insert(newIndex, new SnakeHighscore()
            {
                PlayerName = txtPlayerName.Text,
                Score = currentScore
            });
            // Asegurar que la lista no exceda el máximo
            while (this.HighscoreList.Count > MaxHighscoreListEntryCount)
                this.HighscoreList.RemoveAt(MaxHighscoreListEntryCount);

            // Reordenar y guardar
            ReorderHighscores();
            SaveHighscoreList();

            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }
    }
}