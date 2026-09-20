using System.Collections.Generic;
using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// HUD de prototipo dibujado con IMGUI: no necesita Canvas, prefabs ni TextMeshPro,
    /// así que funciona en cualquier versión de Unity nada más darle a Play.
    /// Cuando el juego esté cerrado, esto se sustituye por un Canvas de verdad.
    /// </summary>
    public class HudOnGui : MonoBehaviour
    {
        public GameManager game;
        public Camera cam;

        private GUIStyle _label, _big, _huge, _small, _button, _box, _msgStyle, _partName, _partSub;
        private Texture2D _panelTex, _barBgTex, _barFgTex;
        private readonly Dictionary<BodyPartId, Texture2D> _partFill = new Dictionary<BodyPartId, Texture2D>();
        private bool _ready;

        private void Awake()
        {
            if (game == null) game = GetComponent<GameManager>();
            if (cam == null) cam = Camera.main;
        }

        private Texture2D SolidTexture(Color c)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void BuildStyles()
        {
            _panelTex = SolidTexture(new Color(0.04f, 0.03f, 0.06f, 0.85f));
            _barBgTex = SolidTexture(new Color(1f, 1f, 1f, 0.15f));
            _barFgTex = SolidTexture(new Color(0.55f, 0.91f, 0.66f, 0.95f));

            _label = new GUIStyle(GUI.skin.label);
            _label.fontSize = 16;
            _label.normal.textColor = new Color(0.95f, 0.90f, 0.82f);

            _small = new GUIStyle(_label);
            _small.fontSize = 12;
            _small.normal.textColor = new Color(0.65f, 0.58f, 0.48f);

            _big = new GUIStyle(_label);
            _big.fontSize = 24;
            _big.fontStyle = FontStyle.Bold;
            _big.alignment = TextAnchor.MiddleCenter;
            _big.normal.textColor = new Color(0.91f, 0.76f, 0.42f);

            _huge = new GUIStyle(_big);
            _huge.fontSize = 44;

            _button = new GUIStyle(GUI.skin.button);
            _button.fontSize = 18;

            _box = new GUIStyle(GUI.skin.box);
            _box.normal.background = _panelTex;

            _msgStyle = new GUIStyle(_big);
            _msgStyle.fontSize = 18;

            _partName = new GUIStyle(_label);
            _partName.fontSize = 15;
            _partSub = new GUIStyle(_small);

            // Una textura por parte, creada una sola vez.
            List<BodyPartData> parts = FBConfig.Parts;
            for (int i = 0; i < parts.Count; i++)
            {
                Color c = parts[i].Color;
                c.a = 0.30f;
                _partFill[parts[i].Id] = SolidTexture(c);
            }

            _ready = true;
        }

        private void OnGUI()
        {
            if (game == null) return;
            if (!_ready) BuildStyles();

            switch (game.State)
            {
                case GameState.Menu: DrawMenu(); break;
                case GameState.Playing: DrawPlaying(); break;
                case GameState.LevelUp: DrawPlaying(); DrawBanner(); break;
                case GameState.GameOver: DrawPlaying(); DrawGameOver(); break;
            }
        }

        private void DrawMenu()
        {
            float w = 560f, h = 340f;
            Rect r = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(r, GUIContent.none, _box);

            GUI.Label(new Rect(r.x, r.y + 18, w, 50), "EL GLOBO DE LA TABERNA", _huge);
            GUI.Label(new Rect(r.x, r.y + 74, w, 26), "No dejes que toque el suelo. Y solo con los pies.", _big);

            GUI.Label(new Rect(r.x + 40, r.y + 120, w - 80, 70),
                "Moverte: W A S D / flechas     Saltar: ESPACIO\n" +
                "Pie: Z     Rodilla: X     Cabeza: C     Pecho: V     Mano: B\n" +
                "La rodilla y las demás se desbloquean subiendo de nivel.", _label);

            GUI.Label(new Rect(r.x, r.y + 196, w, 24), "¿Cómo vas de jarras?", _big);

            float bw = 150f, bh = 44f, gap = 14f;
            float x0 = r.x + (w - (bw * 3 + gap * 2)) * 0.5f;
            float by = r.y + 228;
            if (GUI.Button(new Rect(x0, by, bw, bh), "Sobrio", _button)) game.StartGame(0f);
            if (GUI.Button(new Rect(x0 + bw + gap, by, bw, bh), "Alegre", _button)) game.StartGame(0.45f);
            if (GUI.Button(new Rect(x0 + (bw + gap) * 2, by, bw, bh), "Borracho", _button)) game.StartGame(0.9f);

            GUI.Label(new Rect(r.x, r.y + 288, w, 22),
                "Récord: " + PlayerPrefs.GetInt("fitballon.best", 0), _big);
        }

        private void DrawPlaying()
        {
            // Barra superior.
            GUI.Box(new Rect(0, 0, Screen.width, 42), GUIContent.none, _box);

            string hearts = "";
            for (int i = 0; i < game.Lives; i++) hearts += "♥ ";
            GUIStyle heartStyle = new GUIStyle(_label);
            heartStyle.normal.textColor = new Color(0.79f, 0.29f, 0.35f);
            heartStyle.fontSize = 20;
            GUI.Label(new Rect(16, 8, 200, 28), hearts, heartStyle);

            GUI.Label(new Rect(Screen.width * 0.5f - 150, 6, 300, 24), "NIVEL " + game.Level, _big);

            float goal = FBConfig.GoalFor(game.Level);
            float frac = Mathf.Clamp01(game.LevelTouches / goal);
            Rect bar = new Rect(Screen.width * 0.5f - 95, 30, 190, 6);
            GUI.DrawTexture(bar, _barBgTex);
            GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * frac, bar.height), _barFgTex);

            GUIStyle scoreStyle = new GUIStyle(_label);
            scoreStyle.fontSize = 22;
            scoreStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(Screen.width - 216, 8, 200, 26), game.Score.ToString(), scoreStyle);

            if (game.Combo >= 4)
            {
                GUIStyle comboStyle = new GUIStyle(_big);
                comboStyle.normal.textColor = new Color(1f, 0.83f, 0.47f);
                GUI.Label(new Rect(Screen.width * 0.5f - 150, 46, 300, 24),
                    "COMBO " + game.Combo + "  x" + game.Multiplier().ToString("0.#"), comboStyle);
            }

            if (game.PenaltyTimer > 0f)
            {
                GUIStyle p = new GUIStyle(_small);
                p.normal.textColor = new Color(1f, 0.56f, 0.48f);
                GUI.Label(new Rect(16, 46, 260, 20),
                    "globo hechizado " + Mathf.CeilToInt(game.PenaltyTimer) + "s", p);
            }

            DrawPartStrip();
            DrawFloatingMessages();
        }

        private void DrawPartStrip()
        {
            List<BodyPartData> parts = FBConfig.Parts;
            float cw = 120f, gap = 8f, ch = 46f;
            float total = parts.Count * cw + (parts.Count - 1) * gap;
            float x0 = (Screen.width - total) * 0.5f;
            float y = Screen.height - ch - 16f;

            for (int i = 0; i < parts.Count; i++)
            {
                BodyPartData p = parts[i];
                bool unlocked = game.Level >= p.UnlockLevel;
                float cd = game.Player != null ? game.Player.CooldownLeft(p.Id) : 0f;
                bool ready = unlocked && cd <= 0f;
                Rect cell = new Rect(x0 + i * (cw + gap), y, cw, ch);

                GUI.Box(cell, GUIContent.none, _box);

                if (unlocked && !ready)
                {
                    float f = Mathf.Clamp01(1f - cd / (p.Cooldown + p.Duration));
                    Texture2D fill;
                    if (_partFill.TryGetValue(p.Id, out fill) && fill != null)
                    {
                        GUI.DrawTexture(new Rect(cell.x, cell.y, cell.width * f, cell.height), fill);
                    }
                }

                GUIStyle name = _partName;
                name.normal.textColor = unlocked
                    ? (ready ? p.Color : new Color(0.75f, 0.72f, 0.68f))
                    : new Color(0.35f, 0.31f, 0.38f);
                GUI.Label(new Rect(cell.x + 10, cell.y + 5, cw, 20), "[" + p.KeyLabel + "] " + p.Name, name);

                GUIStyle sub = _partSub;
                sub.normal.textColor = unlocked
                    ? new Color(0.65f, 0.58f, 0.48f)
                    : new Color(0.32f, 0.29f, 0.35f);
                GUI.Label(new Rect(cell.x + 10, cell.y + 24, cw, 18),
                    unlocked ? p.Points + " pts" : "nivel " + p.UnlockLevel, sub);
            }
        }

        private void DrawFloatingMessages()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            List<FloatingMessage> list = game.Messages;
            for (int i = 0; i < list.Count; i++)
            {
                FloatingMessage m = list[i];
                Vector3 sp = cam.WorldToScreenPoint(m.WorldPos);
                if (sp.z <= 0f) continue;

                Color c = m.Color;
                c.a = Mathf.Clamp01(m.Life / m.MaxLife);
                _msgStyle.normal.textColor = c;
                GUI.Label(new Rect(sp.x - 120f, Screen.height - sp.y - 20f, 240f, 24f), m.Text, _msgStyle);
            }
        }

        private void DrawBanner()
        {
            float w = 620f, h = 240f;
            Rect r = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(r, GUIContent.none, _box);

            GUI.Label(new Rect(r.x, r.y + 16, w, 50), "NIVEL " + game.Level, _huge);

            if (game.LastUnlocked != null)
            {
                GUIStyle un = new GUIStyle(_big);
                un.normal.textColor = game.LastUnlocked.Color;
                GUI.Label(new Rect(r.x, r.y + 72, w, 28),
                    "¡" + game.LastUnlocked.Name.ToUpper() + " DESBLOQUEADA!  [" +
                    game.LastUnlocked.KeyLabel + "]", un);
            }
            else
            {
                GUI.Label(new Rect(r.x, r.y + 72, w, 28), "La sala se complica...", _big);
            }

            GUIStyle d20 = new GUIStyle(_huge);
            d20.fontSize = 36;
            d20.normal.textColor = game.LastRoll == 20 ? new Color(0.55f, 0.91f, 0.66f)
                : (game.LastRoll == 1 ? new Color(1f, 0.48f, 0.48f) : new Color(0.95f, 0.90f, 0.82f));
            GUI.Label(new Rect(r.x, r.y + 112, w, 44), "d20: " + game.LastRoll, d20);
            GUI.Label(new Rect(r.x, r.y + 160, w, 24), game.LastRollText, _big);
            GUI.Label(new Rect(r.x, r.y + 200, w, 20), "ESPACIO para seguir", _big);
        }

        private void DrawGameOver()
        {
            float w = 520f, h = 250f;
            Rect r = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(r, GUIContent.none, _box);

            GUI.Label(new Rect(r.x, r.y + 18, w, 50), "EL GLOBO HA CAÍDO", _huge);
            GUI.Label(new Rect(r.x, r.y + 76, w, 24), game.LastReason, _big);
            GUI.Label(new Rect(r.x, r.y + 110, w, 24),
                game.Score + " puntos  ·  nivel " + game.Level + "  ·  " + game.Touches + " toques", _big);
            GUI.Label(new Rect(r.x, r.y + 140, w, 24),
                "Récord: " + PlayerPrefs.GetInt("fitballon.best", 0), _big);

            float bw = 180f, bh = 44f;
            if (GUI.Button(new Rect(r.x + w * 0.5f - bw - 8, r.y + 180, bw, bh), "Otra ronda", _button))
            {
                game.StartGame(game.Player != null ? game.Player.drunk : 0f);
            }
            if (GUI.Button(new Rect(r.x + w * 0.5f + 8, r.y + 180, bw, bh), "Menú", _button))
            {
                game.BackToMenu();
            }
        }
    }
}
