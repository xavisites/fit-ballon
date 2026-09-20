using System.Collections.Generic;
using UnityEngine;

namespace FitBallon
{
    public enum GameState { Menu, Playing, LevelUp, GameOver }

    /// <summary>Texto que sale flotando en el mundo (puntos, avisos, hipidos).</summary>
    public class FloatingMessage
    {
        public Vector3 WorldPos;
        public string Text;
        public Color Color;
        public float Life;
        public float MaxLife;
    }

    /// <summary>Cerebro de la partida: niveles, desbloqueos, puntuación, vidas y d20.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Referencias de la sala")]
        public PlayerController player;
        public BalloonController balloon;
        public Transform roomCenter;
        public GameObject drunkTemplate;

        [Header("Ajustes")]
        public float startDrunk = 0f;
        public bool startInMenu = true;

        public GameState State { get; private set; }
        public int Level { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }
        public int Touches { get; private set; }
        public int LevelTouches { get; private set; }
        public int Combo { get; private set; }
        public float PenaltyTimer { get; private set; }
        public string LastReason { get; private set; }
        public int LastRoll { get; private set; }
        public string LastRollText { get; private set; }
        public BodyPartData LastUnlocked { get; private set; }
        public float BannerTimer { get; private set; }

        public PlayerController Player { get { return player; } }
        public BalloonController Balloon { get { return balloon; } }
        public List<FloatingMessage> Messages { get { return _messages; } }

        private readonly List<FloatingMessage> _messages = new List<FloatingMessage>();
        private readonly List<GameObject> _drunks = new List<GameObject>();
        private float _comboTimer;
        private float _freeze;
        private float _chosenDrunk;

        private void Awake()
        {
            Instance = this;
            State = startInMenu ? GameState.Menu : GameState.Playing;
            _chosenDrunk = startDrunk;
            Level = 1;
            Lives = FBConfig.StartLives;
        }

        private void Start()
        {
            // El cuerpo del jugador no debe empujar el globo: sólo las zonas de golpeo.
            if (player != null && balloon != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null && balloon.Col != null) Physics.IgnoreCollision(balloon.Col, cc, true);
            }
            if (!startInMenu) StartGame(_chosenDrunk);
            else if (balloon != null) balloon.Freeze(true);
        }

        public Vector3 RoomCenterPos()
        {
            return roomCenter != null ? roomCenter.position : Vector3.zero;
        }

        public void StartGame(float drunkLevel)
        {
            _chosenDrunk = drunkLevel;
            Level = 1;
            Lives = FBConfig.StartLives;
            Score = 0;
            Touches = 0;
            LevelTouches = 0;
            Combo = 0;
            PenaltyTimer = 0f;
            LastReason = null;
            LastUnlocked = null;
            _messages.Clear();
            ClearDrunks();

            if (player != null) player.ResetPlayer(RoomCenterPos() + Vector3.back * 3f, drunkLevel);
            if (balloon != null)
            {
                balloon.Freeze(false);
                balloon.Respawn(RoomCenterPos());
            }
            _freeze = 1.2f;
            State = GameState.Playing;
        }

        public void BackToMenu()
        {
            State = GameState.Menu;
            if (balloon != null) balloon.Freeze(true);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                _messages[i].Life -= dt;
                _messages[i].WorldPos += Vector3.up * dt * 0.8f;
                if (_messages[i].Life <= 0f) _messages.RemoveAt(i);
            }

            if (State == GameState.LevelUp)
            {
                BannerTimer -= dt;
                if (BannerTimer <= 0f || FBInput.Pressed(KeyCode.Space))
                {
                    State = GameState.Playing;
                    _freeze = 0.8f;
                }
                return;
            }

            if (State != GameState.Playing) return;

            if (PenaltyTimer > 0f) PenaltyTimer -= dt;
            if (_comboTimer > 0f)
            {
                _comboTimer -= dt;
                if (_comboTimer <= 0f) Combo = 0;
            }

            if (_freeze > 0f)
            {
                _freeze -= dt;
                if (balloon != null) balloon.Freeze(true);
                return;
            }
            if (balloon != null) balloon.Freeze(false);

            // ¿Ha tocado el suelo?
            if (balloon != null && balloon.transform.position.y <= FBConfig.FloorDeathY)
            {
                LoseLife("¡El globo ha tocado el suelo!");
            }
        }

        public float Multiplier()
        {
            return Mathf.Min(4f, 1f + Mathf.Floor(Combo / 4f) * 0.5f);
        }

        public void OnTouch(BodyPartData part, Vector3 worldPos)
        {
            Combo++;
            _comboTimer = 3.2f;
            Touches++;
            LevelTouches++;

            float mult = Multiplier();
            int pts = Mathf.RoundToInt(part.Points * mult);
            string label = "+" + pts + (mult > 1f ? "  x" + mult.ToString("0.#") : "");
            AddScore(pts, worldPos, label, part.Color);

            if (LevelTouches >= FBConfig.GoalFor(Level)) LevelUp();
        }

        public void AddScore(int points, Vector3 worldPos, string label)
        {
            AddScore(points, worldPos, label, new Color(1f, 0.91f, 0.69f));
        }

        public void AddScore(int points, Vector3 worldPos, string label, Color color)
        {
            Score += points;
            if (!string.IsNullOrEmpty(label)) ShowMessage(worldPos, label, color);
        }

        public void ShowMessage(Vector3 worldPos, string text, Color color)
        {
            FloatingMessage m = new FloatingMessage();
            m.WorldPos = worldPos;
            m.Text = text;
            m.Color = color;
            m.Life = 1.4f;
            m.MaxLife = 1.4f;
            _messages.Add(m);
        }

        public void LoseLife(string reason)
        {
            if (State != GameState.Playing) return;

            Lives--;
            Combo = 0;
            LastReason = reason;
            ShowMessage(balloon != null ? balloon.transform.position : RoomCenterPos(),
                reason, new Color(1f, 0.56f, 0.48f));

            if (Lives <= 0)
            {
                State = GameState.GameOver;
                if (balloon != null) balloon.Freeze(true);
                int best = PlayerPrefs.GetInt("fitballon.best", 0);
                if (Score > best) PlayerPrefs.SetInt("fitballon.best", Score);
                return;
            }

            if (balloon != null) balloon.Respawn(RoomCenterPos());
            _freeze = 1.1f;
        }

        private void LevelUp()
        {
            Level++;
            LevelTouches = 0;

            LastUnlocked = null;
            List<BodyPartData> parts = FBConfig.Parts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].UnlockLevel == Level) LastUnlocked = parts[i];
            }

            if (Level == 2 || Level == 4 || Level == 6) AddDrunk();
            RollD20();

            BannerTimer = 3.6f;
            State = GameState.LevelUp;
            if (balloon != null) balloon.Freeze(true);
        }

        private void RollD20()
        {
            int n = Random.Range(1, 21);
            string text;
            if (n == 20)
            {
                Lives = Mathf.Min(FBConfig.MaxLives, Lives + 1);
                text = "¡CRÍTICO! El tabernero te regala una vida.";
            }
            else if (n >= 15)
            {
                Score += 200;
                text = "Buena tirada: +200 puntos.";
            }
            else if (n >= 10)
            {
                text = "Nada digno de mención.";
            }
            else if (n >= 6)
            {
                AddDrunk();
                text = "Entra otro parroquiano dando tumbos.";
            }
            else if (n >= 2)
            {
                if (player != null) player.drunk = Mathf.Min(1f, player.drunk + 0.25f);
                text = "Te invitan a una ronda. No puedes decir que no.";
            }
            else
            {
                PenaltyTimer = 15f;
                text = "¡PIFIA! El globo pesa más durante 15 s.";
            }
            LastRoll = n;
            LastRollText = text;
        }

        public void AddDrunk()
        {
            if (drunkTemplate == null) return;
            Vector3 c = RoomCenterPos();
            Vector3 pos = c + new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-6f, 6f));
            pos.y = drunkTemplate.transform.position.y;

            GameObject go = Instantiate(drunkTemplate, pos, Quaternion.identity);
            go.SetActive(true);
            DrunkNpc npc = go.GetComponent<DrunkNpc>();
            if (npc != null) npc.roamCenter = c;
            _drunks.Add(go);
        }

        private void ClearDrunks()
        {
            for (int i = 0; i < _drunks.Count; i++)
            {
                if (_drunks[i] != null) Destroy(_drunks[i]);
            }
            _drunks.Clear();
        }
    }
}
