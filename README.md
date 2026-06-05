# SnakeWPF

SnakeWPF es una aplicación de estudio desarrollada en C# y WPF que reimagina el clásico juego Snake como un desafío de tipo **Grid Filler**.

En lugar de comer objetos para crecer, la serpiente aumenta su tamaño en cada movimiento. El objetivo consiste en cubrir la mayor cantidad posible del tablero mientras se evita colisionar con los bordes o con el propio cuerpo.

## Características

* Desarrollado con WPF (.NET)
* Renderizado mediante Canvas y Rectangles
* Crecimiento continuo de la serpiente
* Sistema de puntuación basado en casillas ocupadas
* Incremento progresivo de velocidad
* Tabla de récords persistente mediante XML
* Interfaz personalizada sin barra de ventana estándar
* Efectos visuales mediante DropShadowEffect

## Mecánica de juego

Cada movimiento añade una nueva sección a la serpiente.

La puntuación aumenta automáticamente conforme se cubren nuevas posiciones del tablero.

La velocidad se incrementa progresivamente según la cantidad de bloques ocupados.

La partida termina cuando:

* La cabeza sale del área de juego.
* La cabeza colisiona con cualquier parte del cuerpo.

## Controles

| Tecla   | Acción                |
| ------- | --------------------- |
| ↑       | Mover arriba          |
| ↓       | Mover abajo           |
| ←       | Mover izquierda       |
| →       | Mover derecha         |
| Espacio | Iniciar nueva partida |

## Arquitectura

### SnakePart

Representa un segmento de la serpiente.

Propiedades:

* Position
* UiElement
* IsHead

### SnakeHighscore

Representa una entrada de la clasificación.

Propiedades:

* PlayerName
* Score

### MainWindow

Gestiona:

* Bucle principal del juego
* Renderizado
* Entrada de usuario
* Colisiones
* Sistema de puntuación
* Persistencia de récords

## Sistema de puntuación

La puntuación aumenta en cada movimiento realizado.

La interfaz muestra:

Puntuación actual / Total de casillas disponibles

Ejemplo:

120 / 400

## Almacenamiento de récords

Los récords se almacenan localmente en:

snake_highscorelist.xml

La aplicación conserva los cinco mejores resultados.

## Posibles mejoras futuras

* Algoritmo de resolución automática
* Generación procedural de niveles
* Modos de dificultad
* Estadísticas avanzadas
* Exportación de partidas
* Temas visuales
* Soporte para tamaños de tablero personalizados
* Animaciones de movimiento

## Compilación

Requisitos:

* Visual Studio 2022 o superior
* .NET Desktop Development
* Windows 10/11

Compilar:

1. Clonar el repositorio.
2. Abrir la solución en Visual Studio.
3. Ejecutar en modo Debug o Release.

## Licencia

Proyecto educativo y experimental desarrollado para el estudio de:

* WPF
* Programación orientada a objetos
* Gestión de eventos
* Persistencia XML
* Renderizado de interfaces gráficas

---

Desarrollado como proyecto de aprendizaje en C# y WPF.
