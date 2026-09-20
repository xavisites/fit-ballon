# El Globo de la Taberna — versión 3D (Unity)

Versión 3D del juego, con la estructura de sala del vídeo de referencia
(*Keep it Up!*): sala cerrada, cámara ancha tipo diorama, globo que no puede
tocar el suelo… pero **solo se toca con los pies** hasta que vas desbloqueando
rodilla, cabeza, pecho y mano.

La sala se monta con **un clic** y se juega con cubos y cápsulas, antes de
importar ningún modelo. Luego se cambian las cajas por los assets sin tocar
ni una línea de código.

---

## Puesta en marcha (5 minutos)

1. En **Unity Hub → New project → 3D** (sirve Built-in o URP; los materiales se
   crean con el shader que toque en cada caso). Unity 2021.3 o superior.
2. Cierra Unity Hub y abre el proyecto.
3. Copia la carpeta **`unity/Assets/FitBallon`** de este repositorio dentro de la
   carpeta `Assets` de tu proyecto.
4. Espera a que compile. En la barra de menús te sale un menú nuevo: **Fit Ballon**.
5. **Fit Ballon ▸ Crear escena jugable** → acepta → se crea y se guarda
   `Assets/FitBallon/Scenes/Sala.unity` con la sala entera montada.
6. Dale a **Play**. Sale el menú: elige Sobrio / Alegre / Borracho.

Si prefieres no crear una escena nueva, abre la tuya y usa
**Fit Ballon ▸ Añadir sala a la escena actual**.

## Controles

| Tecla | Acción |
|---|---|
| `W A S D` o flechas | moverte por la sala |
| `ESPACIO` | saltar |
| `Z` | **pie** — desde el nivel 1 |
| `X` | **rodilla** — nivel 2 |
| `C` | **cabeza** — nivel 3 |
| `V` | **pecho** — nivel 4 |
| `B` | **mano** — nivel 5 |

El personaje **mira solo hacia el globo** cuando estás quieto, así que basta con
colocarte debajo y pulsar la tecla en el momento justo. Cada parte tiene su
alcance, su impulso y su recarga: el pie llega bajo, la cabeza llega alto, el
pecho amortigua para colocarlo, y la mano es hacer trampa (5 puntos y recarga
larguísima).

## Qué hay montado

- **Sala** de 18×18 con muros, techo, 4 columnas y una viga.
- **Candelabro** colgante que se balancea como un péndulo y descoloca el globo.
- **2 braseros**: si el globo toca el fuego, **explota** y pierdes una vida.
- **2 runas** flotantes: +60 puntos si el globo las atraviesa.
- **Borrachos** que dan tumbos, hipan, te tiran al suelo si chocas y mandan el
  globo por los aires (pero dan puntos). Entran más al subir de nivel.
- **Tirada de d20** en cada subida de nivel: vida extra, puntos, otro borracho,
  otra ronda para ti, o pifia que hace pesar el globo 15 segundos.
- **Combo** con multiplicador hasta ×4, 3 vidas y récord guardado en PlayerPrefs.

## Los scripts

```
Scripts/Core/FBConfig.cs      tabla de partes del cuerpo y constantes de ajuste ← toca aquí para equilibrar
Scripts/Core/GameManager.cs   niveles, desbloqueos, puntuación, vidas, d20
Scripts/Core/FBInput.cs       teclado compatible con el Input Manager viejo y el Input System nuevo
Scripts/Core/RbExt.cs         compatibilidad Rigidbody.velocity / linearVelocity (Unity 6)
Scripts/Gameplay/BalloonController.cs   el globo: flotación propia y rebotes
Scripts/Gameplay/PlayerController.cs    movimiento, salto, golpeos y borrachera
Scripts/Gameplay/BodyPartHitbox.cs      zona de golpeo de cada parte del cuerpo
Scripts/Gameplay/DrunkNpc.cs            los parroquianos
Scripts/Gameplay/HazardZone.cs          el fuego
Scripts/Gameplay/RuneBonus.cs           las runas
Scripts/Gameplay/SwingingObstacle.cs    el candelabro
Scripts/Gameplay/RoomCamera.cs          cámara de sala
Scripts/UI/HudOnGui.cs                  HUD provisional (IMGUI, cero configuración)
Editor/FitBallonSceneBuilder.cs         el menú que monta la sala
```

El globo **no usa la gravedad de Unity**: cae con su propia aceleración y su
propio rozamiento (`BalloonFallAccel` 3.0 y `BalloonDrag` 1.5), lo que da una
caída de unos 2 m/s y un toque de pie que lo sube 2,6 m. Si lo quieres más
fácil, baja `BalloonFallAccel`; más difícil, súbelo.

---

## Meter tus assets

### La sala (KayKit Dungeon Pack)

Los objetos de la sala son cubos con un `BoxCollider`. Para cambiarlos:

1. Importa el pack a `Assets/KayKit/`.
2. Arrastra el modelo que quieras como **hijo** del objeto que ya existe
   (`Columna_1`, `Muro_N`…).
3. Borra el `MeshRenderer` y el `MeshFilter` del cubo, **pero deja el collider**:
   es lo que hace rebotar al globo.

Así los colisionadores siguen cuadrando con lo que ves y no hay que retocar nada.

### El globo (`RED_BALLOON.zip`)

Exporta el `.blend` a `.fbx` o `.glb`, mételo como hijo de `Globo`, quita el
`MeshRenderer` de la esfera y deja el `SphereCollider` y el
`BalloonController` donde están.

### Tu personaje (`Personaje.blend`)

Tu rig es **Rigify**, y Rigify mete un montón de huesos de control que Unity no
debe ver:

1. En Blender: selecciona el `rig`, y en el panel de Rigify usa
   *Generate/Bake* o el addon **Rigify to Unity**. Lo importante es exportar
   **solo los huesos de deformación** (los `DEF-`), no los controladores.
2. Exporta a FBX con *Armature* + *Mesh*, y las animaciones `Anim_Idle` y
   `Anim_Walk`.
3. En Unity, en el `.fbx` → pestaña **Rig** → *Animation Type: Humanoid* →
   Apply.
4. Arrastra el personaje como hijo de `Jugador` y borra
   `Cuerpo_placeholder` y `Frente`.
5. **Importante**: mueve cada objeto `Golpeo_Foot`, `Golpeo_Knee`, etc. para que
   cuelguen del hueso correspondiente del esqueleto (el pie, la rodilla, la
   cabeza…). Así la zona de golpeo sigue a la animación en vez de quedarse
   flotando delante del personaje.
6. Para que se vea el gesto, pon un `Animator` con un estado por parte y
   dispáralo desde `PlayerController.TryAction`. Está marcado en el código
   dónde engancharlo.

---

## Lo que falta (por orden de lo que más se nota)

1. **Animaciones** de golpeo: ahora el personaje pega sin mover la pierna.
2. **Sonido**: rebote, fuego, hipidos, ambiente de taberna.
3. **HUD de verdad**: sustituir `HudOnGui` por un Canvas con TextMeshPro.
4. **Partículas**: chispas al golpear y explosión del globo.
5. **Más salas**: la estructura ya lo admite, basta con otro método en
   `FitBallonSceneBuilder` o guardar varias escenas.

## Aviso honesto

Estos scripts se han escrito y **verificado compilando contra la API de Unity**,
pero no se han podido ejecutar dentro del editor (el entorno donde se
desarrollaron no tiene Unity instalado). La lógica y la física vienen del
prototipo 2D, que sí está probado. Si al darle a Play algo no cuadra —sobre todo
alturas y tiempos de recarga—, los números están todos juntos en
`Scripts/Core/FBConfig.cs`.
