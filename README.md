# El Globo de la Taberna 🎈🐉

> **Dos versiones en este repositorio**
> - **3D en Unity** → [`unity/`](unity/README.md) — la versión buena, con sala
>   montada de un clic. Es la que se está llevando adelante.
> - **Prototipo 2D en el navegador** (lo que hay debajo) — sirvió para ajustar
>   el diseño: desbloqueo de partes del cuerpo, combos, d20 y borrachera.
>   Se juega abriendo `index.html`.

---

## Prototipo 2D (navegador)

Juego de navegador: estás en una sala de mazmorra (estilo D&D) llena de obstáculos
y hay un globo que **no puede tocar el suelo**. La gracia es que al principio
**solo puedes tocarlo con los pies**; según subes de nivel se te van desbloqueando
la rodilla, la cabeza, el pecho y la mano. Y sí: puedes jugar borracho.

## Cómo jugar

Abre `index.html` con doble clic (no hace falta ni servidor ni instalar nada).

Si prefieres servirlo:

```bash
npx http-server . -p 8080     # luego abre http://localhost:8080
```

## Controles

| Tecla | Acción |
|---|---|
| `←` `→` (o `A` `D`) | moverte por la sala |
| `↑` / `ESPACIO` (o `W`) | saltar |
| `Z` | **pie** — disponible desde el nivel 1 |
| `X` | **rodilla** — nivel 2 |
| `C` | **cabeza** — nivel 3 |
| `V` | **pecho** — nivel 4 |
| `B` | **mano** — nivel 5 |
| `P` / `ESC` | pausa |
| `H` | ver las zonas de golpeo (depuración) |

En móvil/tablet aparecen botones táctiles abajo.

## Reglas

- Tienes **3 vidas**. Pierdes una si el globo toca el suelo o si estalla contra el fuego.
- Cada parte del cuerpo da puntos distintos: cuanto más difícil, más puntos.
  El pie son 10, la rodilla 20, el pecho 25 y la cabeza 35. La mano solo da 5
  (es hacer trampa) y tarda mucho en recargarse.
- Encadenar toques sube el **combo**, y el combo multiplica los puntos hasta ×4.
- Las **runas azules** flotando dan 60 puntos si el globo las atraviesa.
- Cada parte tiene una recarga: no vale machacar la tecla.

## La sala

Según el nivel van apareciendo:

- **Columnas y vigas**: el globo rebota en ellas.
- **Candelabros colgantes**: se balancean y mandan el globo a donde les apetece.
- **Braseros y antorchas**: si el globo toca el fuego, **explota**.
- **Runas**: puntos extra.

## Los borrachos

En la taberna hay parroquianos dando tumbos. Estorban, hipan y, si el globo
les da, lo mandan por los aires en una dirección aleatoria (eso sí, te dan puntos).
Si chocas con uno, te tira al suelo unos instantes.

Además tú mismo puedes estar bebido. En el menú eliges tu estado:

- **Sobrio**: control limpio.
- **Alegre**: te vas de lado y de vez en cuando se te cambian los controles.
- **Borracho**: la sala da vueltas, los golpes se desvían y tus piernas van a su aire.

## La tirada de d20

Al subir de nivel se tira un d20 y la sala decide:

| Tirada | Efecto |
|---|---|
| 20 | ¡Crítico! +1 vida |
| 15-19 | +200 puntos |
| 10-14 | nada |
| 6-9 | entra otro borracho |
| 2-5 | te invitan a una ronda (subes de borrachera) |
| 1 | ¡Pifia! el globo pesa más durante 15 s |

## Estructura del proyecto

```
index.html        pantalla, menús y controles táctiles
css/style.css     estilo de menús y HUD en DOM
src/config.js     constantes: partes del cuerpo, física, niveles
src/utils.js      matemáticas y ayudas de dibujo
src/fx.js         partículas, textos flotantes y temblor de cámara
src/room.js       la sala: decorado, obstáculos y sus colisiones
src/balloon.js    el globo
src/player.js     el aventurero y sus zonas de golpeo por parte del cuerpo
src/drunks.js     los parroquianos borrachos
src/game.js       bucle, estados, puntuación, HUD y arranque
```

Todo es JavaScript sin dependencias ni compilación: se cargan los `<script>` en orden
y comparten el objeto global `FB`.

## Ideas para seguir

- Sonido: hipidos, rebotes, fuego, música de taberna.
- Modo dos jugadores en el mismo teclado (pases entre vosotros).
- Más salas: cripta helada (el globo resbala), fragua (corrientes de aire).
- Modo de verdad "fitness": leer la webcam con detección de pose para que los
  toques sean con tus pies de verdad.
