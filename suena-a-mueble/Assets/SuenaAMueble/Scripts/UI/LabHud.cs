using SuenaAMueble.Audio;
using UnityEngine;

namespace SuenaAMueble
{
    /// <summary>
    /// Interfaz provisional con IMGUI: cero configuración, funciona en cualquier Unity.
    /// Teclas: ESPACIO = acción principal, R = oír el original, H = así te oye el cazador,
    /// V = tu voz, ESC = menú, F1 = datos de depuración.
    /// </summary>
    public class LabHud : MonoBehaviour
    {
        public ImitationLab lab;
        public MicCapture mic;

        private const float RefHeight = 720f;
        private float _w;
        private bool _debug;
        private Vector2 _scroll;

        private GUIStyle _title, _label, _small, _big, _huge, _button, _bigButton, _panel, _center;
        private Texture2D _panelTex, _barBg, _white;
        private Texture2D _graph;
        private ScoreResult _graphFor;
        private bool _ready;

        private static readonly Color Green = new Color(0.45f, 0.9f, 0.5f);
        private static readonly Color Yellow = new Color(1f, 0.82f, 0.3f);
        private static readonly Color Red = new Color(1f, 0.38f, 0.32f);

        private Texture2D Solid(Color c)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void BuildStyles()
        {
            _panelTex = Solid(new Color(0.06f, 0.06f, 0.1f, 0.86f));
            _barBg = Solid(new Color(1f, 1f, 1f, 0.15f));
            _white = Solid(Color.white);

            _label = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
            _label.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            _small = new GUIStyle(_label) { fontSize = 13 };
            _small.normal.textColor = new Color(0.75f, 0.75f, 0.8f);
            _title = new GUIStyle(_label) { fontSize = 26, fontStyle = FontStyle.Bold };
            _title.normal.textColor = Yellow;
            _center = new GUIStyle(_label) { alignment = TextAnchor.MiddleCenter };
            _big = new GUIStyle(_center) { fontSize = 30, fontStyle = FontStyle.Bold };
            _huge = new GUIStyle(_center) { fontSize = 90, fontStyle = FontStyle.Bold };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 16, wordWrap = true };
            _bigButton = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };
            _panel = new GUIStyle(GUI.skin.box);
            _panel.normal.background = _panelTex;
            _ready = true;
        }

        private void OnGUI()
        {
            if (lab == null) return;
            if (!_ready) BuildStyles();

            float scale = Screen.height / RefHeight;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            _w = Screen.width / scale;

            // Si una tecla ha cambiado de pantalla, no se dibuja este evento (evita errores de IMGUI).
            if (HandleKeys()) return;

            GUI.Label(new Rect(20, 12, 700, 40), "SUENA A MUEBLE  ·  laboratorio de imitación", _title);

            switch (lab.State)
            {
                case LabState.Menu: DrawMenu(); break;
                case LabState.Elegir: DrawPicker(); break;
                case LabState.Listo: DrawReady(); break;
                case LabState.Escuchando: DrawBanner("Escucha bien...", lab.Current.Onomatopeya); break;
                case LabState.CuentaAtras: DrawCountdown(); break;
                case LabState.Grabando: DrawRecording(); break;
                case LabState.Analizando: DrawBanner("Analizando...", ""); break;
                case LabState.Resultado: DrawResult(); break;
                case LabState.RetoFin: DrawChallengeEnd(); break;
            }

            if (lab.Mode == LabMode.Reto && lab.State != LabState.Menu && lab.State != LabState.RetoFin)
            {
                GUI.Label(new Rect(_w - 300, 16, 280, 30),
                    "RETO  " + (lab.RetoIndex + 1) + "/" + ImitationLab.RetoLength + "   ·   " + lab.RetoPoints + " pts", _label);
            }

            DrawMicBar();

            if (!string.IsNullOrEmpty(lab.Message))
            {
                GUI.Label(new Rect(20, RefHeight - 110, _w - 40, 30), lab.Message, _label);
            }
        }

        // ------------------------------------------------------------------ teclas
        private bool HandleKeys()
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return false;
            switch (e.keyCode)
            {
                case KeyCode.Space:
                    if (lab.State == LabState.Listo) lab.BeginAttempt();
                    else if (lab.State == LabState.Resultado) lab.Next();
                    else if (lab.State == LabState.Menu) lab.StartFree();
                    break;
                case KeyCode.R:
                    if (lab.State == LabState.Listo || lab.State == LabState.Resultado) lab.PlayReference();
                    break;
                case KeyCode.H:
                    if (lab.State == LabState.Resultado) lab.PlayHunterMix();
                    break;
                case KeyCode.V:
                    if (lab.State == LabState.Resultado) lab.PlayMyVoice(lab.LastAttempt);
                    break;
                case KeyCode.Escape:
                    lab.GoToMenu();
                    break;
                case KeyCode.F1:
                    _debug = !_debug;
                    break;
                default:
                    return false;
            }
            e.Use();
            return true;
        }

        // ------------------------------------------------------------------ pantallas
        private void DrawMenu()
        {
            Rect r = new Rect(20, 70, 520, 470);
            GUI.Box(r, GUIContent.none, _panel);
            GUILayout.BeginArea(new Rect(r.x + 20, r.y + 16, r.width - 40, r.height - 32));
            GUILayout.Label("Te escondes convertido en un objeto. Cuando suena la alarma, tienes que " +
                            "hacer su ruido con la voz. Si lo haces mal, el cazador oye TU voz saliendo del mueble.", _label);
            GUILayout.Space(10);
            GUILayout.Label("80-100  invisible   ·   50-79  sospechoso   ·   0-49  ¡pillado!", _small);
            GUILayout.Space(16);
            if (GUILayout.Button("Modo libre  (elige objeto y practica)", _bigButton, GUILayout.Height(60))) lab.StartFree();
            GUILayout.Space(8);
            if (GUILayout.Button("Reto: " + ImitationLab.RetoLength + " objetos al azar, una sola oportunidad", _bigButton, GUILayout.Height(60))) lab.StartChallenge();
            GUILayout.Space(16);
            GUILayout.Label("Prueba el micro: habla y mira que se mueva la barra de abajo. " +
                            "Mejor con auriculares, para que el micro no capte el sonido original.", _small);
            GUILayout.EndArea();
        }

        private void DrawPicker()
        {
            Rect r = new Rect(20, 70, 560, RefHeight - 200);
            GUI.Box(r, GUIContent.none, _panel);
            GUILayout.BeginArea(new Rect(r.x + 14, r.y + 12, r.width - 28, r.height - 24));
            GUILayout.Label("Elige en qué te escondes", _label);
            _scroll = GUILayout.BeginScrollView(_scroll);
            int dif = 0;
            foreach (PropDef def in PropCatalog.All)
            {
                if (def.Dificultad != dif)
                {
                    dif = def.Dificultad;
                    GUILayout.Space(6);
                    GUILayout.Label(PropCatalog.Estrellas(dif), _small);
                }
                if (GUILayout.Button(def.Nombre + "     \"" + def.Onomatopeya + "\"", _button, GUILayout.Height(34)))
                {
                    lab.Choose(def);
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Volver", _button, GUILayout.Height(30))) lab.GoToMenu();
            GUILayout.EndArea();
        }

        private void DrawReady()
        {
            PropDef d = lab.Current;
            Rect r = new Rect(20, 70, 430, 330);
            GUI.Box(r, GUIContent.none, _panel);
            GUILayout.BeginArea(new Rect(r.x + 20, r.y + 14, r.width - 40, r.height - 28));
            GUILayout.Label("Eres " + d.ConArticulo.ToUpper(), _title);
            GUILayout.Label("\"" + d.Onomatopeya + "\"", _label);
            GUILayout.Label(PropCatalog.Estrellas(d.Dificultad) + (lab.CurrentRef.FromFile ? "   ·   sonido real" : "   ·   sonido provisional"), _small);
            GUILayout.Space(14);
            if (GUILayout.Button("Escuchar el original  (R)", _button, GUILayout.Height(40))) lab.PlayReference();
            GUILayout.Space(6);
            if (GUILayout.Button("¡IMITAR!  (ESPACIO)", _bigButton, GUILayout.Height(64))) lab.BeginAttempt();
            GUILayout.Space(6);
            if (GUILayout.Button("Otro objeto", _button, GUILayout.Height(32))) lab.StartFree();
            GUILayout.EndArea();
        }

        private void DrawBanner(string title, string sub)
        {
            Rect r = new Rect(_w * 0.5f - 300, 80, 600, 110);
            GUI.Box(r, GUIContent.none, _panel);
            GUI.Label(new Rect(r.x, r.y + 12, r.width, 46), title, _big);
            GUI.Label(new Rect(r.x, r.y + 60, r.width, 36), sub, _center);
        }

        private void DrawCountdown()
        {
            GUI.Label(new Rect(_w * 0.5f - 200, 90, 400, 130), lab.CountdownText, _huge);
        }

        private void DrawRecording()
        {
            Rect r = new Rect(_w * 0.5f - 300, 80, 600, 130);
            GUI.Box(r, GUIContent.none, _panel);
            GUI.color = Red;
            GUI.Label(new Rect(r.x, r.y + 10, r.width, 46), "● ¡HAZ DE " + lab.Current.Nombre.ToUpper() + "!", _big);
            GUI.color = Color.white;
            GUI.Label(new Rect(r.x, r.y + 56, r.width, 30), "\"" + lab.Current.Onomatopeya + "\"", _center);
            float k = Mathf.Clamp01(lab.RecordElapsed / lab.RecordSeconds);
            Bar(new Rect(r.x + 30, r.y + 96, r.width - 60, 16), 1f - k, Yellow);
        }

        private void DrawResult()
        {
            Attempt a = lab.LastAttempt;
            if (a == null) return;
            ScoreResult s = a.Result;
            Color c = ColorFor(s.Total);

            Rect r = new Rect(20, 70, 500, RefHeight - 190);
            GUI.Box(r, GUIContent.none, _panel);
            float x = r.x + 20, y = r.y + 10, w = r.width - 40;

            GUI.color = c;
            GUI.Label(new Rect(x, y, w, 100), s.Total.ToString(), _huge);
            GUI.color = Color.white;
            y += 96;
            GUI.Label(new Rect(x, y, w, 34), Verdict(s, a.Def), _big);
            y += 38;
            GUI.Label(new Rect(x, y, w, 24), "+" + a.Points + " puntos  (" + PropCatalog.Estrellas(a.Def.Dificultad) + ")", _center);
            y += 30;

            SubBar(ref y, x, w, "Ritmo", s.Rhythm);
            SubBar(ref y, x, w, "Melodía", s.Pitch);
            SubBar(ref y, x, w, "Brillo", s.Brightness);
            SubBar(ref y, x, w, "Ruido / nota", s.Noise);

            GUI.Label(new Rect(x, y + 2, w, 44), s.Tip, _small);
            y += 46;

            DrawGraph(new Rect(x, y, w, 70), s);
            y += 74;

            if (lab.HunterMixPlaying) GUI.Label(new Rect(x, y - 2, w, 20), "Así te oye el cazador...", _small);
            y += 18;

            float bw = (w - 10) / 3f;
            if (GUI.Button(new Rect(x, y, bw, 36), "Original (R)", _button)) lab.PlayReference();
            if (GUI.Button(new Rect(x + bw + 5, y, bw, 36), "Tu voz (V)", _button)) lab.PlayMyVoice(a);
            if (GUI.Button(new Rect(x + 2 * (bw + 5), y, bw, 36), "Cazador (H)", _button)) lab.PlayHunterMix();
            y += 42;

            if (lab.Mode == LabMode.Libre)
            {
                float hw = (w - 5) / 2f;
                if (GUI.Button(new Rect(x, y, hw, 44), "Repetir", _bigButton)) lab.Retry();
                if (GUI.Button(new Rect(x + hw + 5, y, hw, 44), "Otro (ESPACIO)", _bigButton)) lab.Next();
            }
            else
            {
                string next = lab.RetoIndex + 1 >= ImitationLab.RetoLength ? "Ver resultado (ESPACIO)" : "Siguiente (ESPACIO)";
                if (GUI.Button(new Rect(x, y, w, 44), next, _bigButton)) lab.Next();
            }

            if (_debug)
            {
                GUI.Label(new Rect(r.xMax + 10, r.y, 360, 120),
                    "raw " + s.Raw.ToString("F2") + "\ngolpes ref " + s.RefBursts + " / tú " + s.ImpBursts +
                    "\nduración ref " + s.RefDuration.ToString("F2") + "s / tú " + s.ImpDuration.ToString("F2") + "s" +
                    "\nruido de fondo " + mic.NoiseFloorDb.ToString("F0") + " dB", _small);
            }
        }

        private void DrawChallengeEnd()
        {
            Rect r = new Rect(_w * 0.5f - 300, 70, 600, 470);
            GUI.Box(r, GUIContent.none, _panel);
            GUILayout.BeginArea(new Rect(r.x + 24, r.y + 14, r.width - 48, r.height - 28));
            GUILayout.Label("FIN DEL RETO", _big);
            GUILayout.Label(lab.RetoPoints + " puntos", _big);
            GUILayout.Space(8);
            foreach (Attempt a in lab.RetoAttempts)
            {
                GUILayout.Label(a.Def.Nombre + ":  " + a.Result.Total + "  (+" + a.Points + ")", _label);
            }
            GUILayout.Space(12);
            Attempt best = lab.BestAttempt();
            Attempt worst = lab.WorstAttempt();
            if (best != null && GUILayout.Button("MEJOR imitación: " + best.Def.Nombre + " (" + best.Result.Total + ")", _button, GUILayout.Height(40)))
            {
                lab.PlayMyVoice(best);
            }
            if (worst != null && GUILayout.Button("PEOR imitación: " + worst.Def.Nombre + " (" + worst.Result.Total + ")", _button, GUILayout.Height(40)))
            {
                lab.PlayMyVoice(worst);
            }
            GUILayout.Space(12);
            if (GUILayout.Button("Otra vez", _bigButton, GUILayout.Height(50))) lab.StartChallenge();
            if (GUILayout.Button("Menú", _button, GUILayout.Height(32))) lab.GoToMenu();
            GUILayout.EndArea();
        }

        // ------------------------------------------------------------------ piezas
        private void DrawMicBar()
        {
            Rect r = new Rect(20, RefHeight - 70, 520, 50);
            GUI.Box(r, GUIContent.none, _panel);
            GUI.Label(new Rect(r.x + 12, r.y + 4, 330, 22), "Micro: " + mic.DisplayName, _small);
            Color c = mic.Level01 > 0.92f ? Red : Green;
            Bar(new Rect(r.x + 12, r.y + 28, 330, 12), mic.Level01, c);
            if (GUI.Button(new Rect(r.x + 360, r.y + 10, 145, 30), "Cambiar micro", _button)) mic.NextDevice();
        }

        private void SubBar(ref float y, float x, float w, string name, float value)
        {
            GUI.Label(new Rect(x, y, 120, 22), name, _small);
            if (value < 0f)
            {
                GUI.Label(new Rect(x + 120, y, w - 120, 22), "no cuenta en este objeto", _small);
            }
            else
            {
                Bar(new Rect(x + 120, y + 5, w - 170, 12), value, ColorFor(value * 100f));
                GUI.Label(new Rect(x + w - 44, y, 44, 22), Mathf.RoundToInt(value * 100f).ToString(), _small);
            }
            y += 24;
        }

        private void Bar(Rect r, float value, Color c)
        {
            GUI.DrawTexture(r, _barBg);
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width * Mathf.Clamp01(value), r.height), _white);
            GUI.color = old;
        }

        /// <summary>Volumen del original (blanco) contra el tuyo (amarillo).</summary>
        private void DrawGraph(Rect r, ScoreResult s)
        {
            if (_graph == null || _graphFor != s)
            {
                _graphFor = s;
                const int W = 240, H = 60;
                if (_graph == null) _graph = new Texture2D(W, H) { filterMode = FilterMode.Point };
                Color bg = new Color(0f, 0f, 0f, 0.35f);
                Color[] px = new Color[W * H];
                for (int i = 0; i < px.Length; i++) px[i] = bg;
                Plot(px, W, H, s.RefEnvelope, new Color(1f, 1f, 1f, 0.9f));
                Plot(px, W, H, s.ImpEnvelope, Yellow);
                _graph.SetPixels(px);
                _graph.Apply();
            }
            GUI.DrawTexture(r, _graph);
            GUI.Label(new Rect(r.x + 6, r.y + 2, 300, 20), "volumen: blanco = original, amarillo = tú", _small);
        }

        private static void Plot(Color[] px, int w, int h, float[] values, Color c)
        {
            if (values == null || values.Length < 2) return;
            int prevY = -1;
            for (int x = 0; x < w; x++)
            {
                float pos = (float)x / (w - 1) * (values.Length - 1);
                int i0 = Mathf.FloorToInt(pos);
                int i1 = Mathf.Min(i0 + 1, values.Length - 1);
                float v = Mathf.Lerp(values[i0], values[i1], pos - i0);
                int y = Mathf.Clamp(Mathf.RoundToInt(v * (h - 4)) + 2, 0, h - 1);
                int from = prevY < 0 ? y : Mathf.Min(prevY, y);
                int to = prevY < 0 ? y : Mathf.Max(prevY, y);
                for (int yy = from; yy <= to; yy++) px[yy * w + x] = c;
                prevY = y;
            }
        }

        private static Color ColorFor(float score)
        {
            if (score >= 80f) return Green;
            if (score >= 50f) return Yellow;
            return Red;
        }

        private static string Verdict(ScoreResult s, PropDef def)
        {
            if (s.Silent) return "SILENCIO = ¡PILLADO!";
            if (s.Total >= 80) return "¡INVISIBLE! Eres " + def.ConArticulo;
            if (s.Total >= 50) return "Sospechoso... el cazador duda";
            return "¡PILLADO! Se oye tu voz";
        }
    }
}
