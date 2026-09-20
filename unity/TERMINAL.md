# Unity desde la terminal

Unity tiene modo consola (`-batchmode`), así que puedes montar la escena o
compilar el juego **sin abrir el editor**. Útil si prefieres trabajar a base de
comandos en vez de clics.

> **Antes de nada: cierra Unity.** Un proyecto solo lo puede tener abierto un
> proceso a la vez; si el editor está abierto, el comando falla con
> *"Multiple Unity instances cannot open the same project"*.

## Dónde está el ejecutable

| Sistema | Ruta |
|---|---|
| Windows | `C:\Program Files\Unity\Hub\Editor\<VERSIÓN>\Editor\Unity.exe` |
| macOS | `/Applications/Unity/Hub/Editor/<VERSIÓN>/Unity.app/Contents/MacOS/Unity` |
| Linux | `~/Unity/Hub/Editor/<VERSIÓN>/Editor/Unity` |

Sustituye `<VERSIÓN>` por la que tengas (por ejemplo `6000.0.30f1` o `2022.3.45f1`).

## Montar la sala

**Windows (PowerShell):**

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.30f1\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "C:\ruta\a\tu\ProyectoFitBallon" `
  -executeMethod FitBallon.EditorTools.FitBallonCli.Scene `
  -logFile -
```

**macOS / Linux:**

```bash
/Applications/Unity/Hub/Editor/6000.0.30f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath ~/ProyectoFitBallon \
  -executeMethod FitBallon.EditorTools.FitBallonCli.Scene \
  -logFile -
```

`-logFile -` manda el registro a la propia terminal: ahí ves si algo ha fallado.
Al terminar tienes `Assets/FitBallon/Scenes/Sala.unity` lista.

## Compilar un ejecutable

Lo mismo cambiando el método a `Build`:

```bash
... -executeMethod FitBallon.EditorTools.FitBallonCli.Build ...
```

Compila para la plataforma que tenga activa el proyecto y deja el resultado en
la carpeta `Builds/` del proyecto. Si la escena no existe, la crea antes.

## Lo que la terminal NO puede hacer

- **Jugar.** No hay modo consola para el bucle de juego: para darle a Play hace
  falta el editor abierto (o el ejecutable ya compilado).
- **Ver la escena.** Para mirar si algo se ve raro, hay que abrir Unity.

Es decir: la terminal sirve para generar y compilar, pero **probar el juego es
siempre a mano**, en tu máquina.
