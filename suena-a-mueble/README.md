# Suena a Mueble · prototipo 1: laboratorio de imitación

> Te escondes convertido en un objeto. Cuando suena la alarma tienes que hacer su ruido con
> la voz. Si lo haces mal, el cazador oye **tu voz** saliendo de la nevera.

Este primer prototipo prueba lo más arriesgado del juego, la **nota de la imitación**,
antes de meter el multijugador. Es para un jugador y no necesita ningún asset: los sonidos
y los objetos son provisionales y están hechos por código.

## Cómo abrirlo (la primera vez)

1. Instala **GitHub Desktop** e inicia sesión con tu cuenta de GitHub.
2. *File → Clone repository →* `xavisites/fit-ballon` → elige una carpeta → *Clone*.
3. Arriba, en *Current branch*, elige **`claude/great-ride-svi27x`**.
4. Abre **Unity Hub → Projects → Add → Add project from disk** y selecciona la carpeta
   **`fit-ballon/suena-a-mueble`** (esta carpeta, no la de fuera).
5. Si Unity Hub dice que no tienes esa versión, elige la **Unity 6** que tengas instalada.
6. La primera vez tarda unos minutos. Cuando termine, la escena `Laboratorio` se crea y se
   abre sola. Si no se abre, ve al menú **Suena a Mueble → Abrir laboratorio**.
7. Dale a **Play**. Mejor con auriculares.

## Cómo recibir los cambios nuevos

En GitHub Desktop: **Fetch origin → Pull origin**. Vuelve a Unity, que recompila solo, y
dale a Play.

Si GitHub Desktop te enseña archivos cambiados que no has tocado tú (por ejemplo,
`ProjectVersion.txt`), descártalos con clic derecho → *Discard changes*.

## Controles

| Tecla | Acción |
|---|---|
| `ESPACIO` | imitar / siguiente |
| `R` | oír el sonido original |
| `V` | oír tu imitación |
| `H` | **así te oye el cazador** |
| `ESC` | menú |
| `F1` | datos de depuración (nota cruda, golpes, duración) |

## Cómo puntúa

Se comparan cuatro cosas entre el original y tu imitación, sin IA:

- **Ritmo:** cuántos golpes tiene, cómo sube y baja el volumen y cuánto dura.
- **Melodía:** solo en los animales (pato, gato, perro y gallo). Mide si subes y bajas el tono como el original.
- **Brillo:** si es agudo como "sss" o "iii", o grave como "ooo" o "mmm".
- **Ruido o nota:** si es un soplido como "shhh" o una nota limpia como "uuu".

| Nota | Resultado |
|---|---|
| 80–100 | invisible: el cazador oye el objeto de verdad |
| 50–79 | sospechoso: el objeto tiembla |
| 0–49 | ¡pillado!: se pone rojo, le salen ojos y se oye tu voz |

Si lo intentas de verdad, la nota mínima es 40. Todos los números para hacerlo más fácil o
más difícil están en `ScoringConfig`, dentro de `Scripts/Audio/ImitationScorer.cs`.

## Archivos

```
Scripts/Audio/AudioFeatures.cs     análisis: volumen, tono (YIN), brillo y ruido cada 10 ms
Scripts/Audio/ImitationScorer.cs   comparación y nota de 0 a 100 (+ ScoringConfig)
Scripts/Audio/SoundSynth.cs        sonidos provisionales generados por código
Scripts/Audio/MicCapture.cs        micrófono siempre grabando en bucle, nivel y ruido de fondo
Scripts/Audio/ReferenceLibrary.cs  usa Resources/Sonidos/<id>.wav si existe; si no, el sintetizado
Scripts/Core/PropCatalog.cs        los 19 objetos: nombre, sonido y dificultad
Scripts/Core/LabBootstrap.cs       monta la escena al darle a Play
Scripts/Gameplay/PropShapes.cs     modelos provisionales hechos con cubos y cilindros
Scripts/Gameplay/PropDisplay.cs    el objeto: salta, tiembla, se pone rojo y le salen ojos
Scripts/Gameplay/ImitationLab.cs   flujo: escuchar, cuenta atrás, grabar, nota y cazador
Scripts/UI/LabHud.cs               interfaz provisional (IMGUI)
Editor/SuenaAMuebleSetup.cs        crea y abre la escena sola
Resources/Sonidos/                 aquí van los sonidos de verdad (ver LEEME.txt)
```

## Aviso

El código compila contra las librerías de Unity y la puntuación se ha probado con sonidos
generados. Lo que no se ha podido probar aquí es el editor con un micrófono de verdad. Si
algo falla al darle a Play, copia el error rojo de la consola de Unity y pásamelo.
