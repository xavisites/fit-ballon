using System.Collections.Generic;
using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// El aventurero. Se mueve en el plano de la sala con la cámara como referencia,
    /// salta, y golpea el globo con la parte del cuerpo que tenga desbloqueada.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Referencias")]
        public Transform cameraPivot;          // para mover en función de la cámara
        public BodyPartHitbox[] hitboxes;

        [Header("Borrachera (0 = sobrio, 1 = barril y medio)")]
        [Range(0f, 1f)] public float drunk = 0f;

        [Header("Ayuda de puntería")]
        public bool faceBalloonWhenIdle = true;

        private CharacterController _cc;
        private Vector3 _velocity;             // sólo la componente vertical se acumula
        private Vector3 _planarVel;
        private float _stun;
        private float _swayPhase;
        private float _invertTimer;
        private float _invertCooldown = 3f;
        private float _lockMsgTimer;

        private readonly Dictionary<BodyPartId, float> _cooldowns = new Dictionary<BodyPartId, float>();
        private BodyPartData _currentAction;
        private float _actionTime;
        private bool _actionUsed;

        public bool IsStunned { get { return _stun > 0f; } }
        public BodyPartData CurrentAction { get { return _currentAction; } }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _swayPhase = Random.Range(0f, 10f);
            if (hitboxes == null || hitboxes.Length == 0)
            {
                hitboxes = GetComponentsInChildren<BodyPartHitbox>(true);
            }
        }

        public float CooldownLeft(BodyPartId id)
        {
            float v;
            if (_cooldowns.TryGetValue(id, out v)) return Mathf.Max(0f, v);
            return 0f;
        }

        public bool CanUse(BodyPartData part)
        {
            if (part == null || _stun > 0f || _currentAction != null) return false;
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Level < part.UnlockLevel) return false;
            return CooldownLeft(part.Id) <= 0f;
        }

        public bool TryAction(BodyPartId id)
        {
            BodyPartData part = FBConfig.Get(id);
            if (part == null) return false;

            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Level < part.UnlockLevel)
            {
                if (_lockMsgTimer <= 0f)
                {
                    _lockMsgTimer = 0.9f;
                    gm.ShowMessage(transform.position + Vector3.up * 2.6f,
                        part.Name.ToUpper() + " — nivel " + part.UnlockLevel, new Color(0.79f, 0.42f, 0.42f));
                }
                return false;
            }
            if (!CanUse(part)) return false;

            _currentAction = part;
            _actionTime = 0f;
            _actionUsed = false;
            _cooldowns[part.Id] = part.Cooldown + part.Duration;
            return true;
        }

        /// <summary>Lo llama la zona de golpeo cuando alcanza al globo.</summary>
        public void RegisterHit(BodyPartId id, BalloonController balloon, Vector3 contactPoint)
        {
            if (_currentAction == null || _currentAction.Id != id || _actionUsed) return;
            _actionUsed = true;
            SetHitbox(id, false);

            BodyPartData part = _currentAction;
            Vector3 fwd = transform.forward;
            Vector3 vel = Vector3.up * part.Power + fwd * part.Forward + _planarVel * 0.25f;

            if (part.Id == BodyPartId.Chest)
            {
                // El pecho amortigua: casi sin componente horizontal.
                vel = Vector3.up * part.Power + fwd * part.Forward * 0.4f;
            }
            if (drunk > 0f)
            {
                vel += new Vector3(Random.Range(-1.6f, 1.6f), Random.Range(-0.4f, 0.4f),
                                   Random.Range(-1.6f, 1.6f)) * drunk;
            }

            balloon.Kick(vel);
            if (GameManager.Instance != null) GameManager.Instance.OnTouch(part, contactPoint);
        }

        public void Knock(Vector3 fromPosition)
        {
            if (_stun > 0f) return;
            _stun = 0.8f;
            Vector3 away = transform.position - fromPosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = -transform.forward;
            _planarVel = away.normalized * 4.5f;
            _velocity.y = 3.5f;
            CancelAction();
        }

        private void CancelAction()
        {
            if (_currentAction != null) SetHitbox(_currentAction.Id, false);
            _currentAction = null;
        }

        private void SetHitbox(BodyPartId id, bool on)
        {
            if (hitboxes == null) return;
            for (int i = 0; i < hitboxes.Length; i++)
            {
                if (hitboxes[i] != null && hitboxes[i].part == id) hitboxes[i].SetActive(on);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            GameManager gm = GameManager.Instance;
            bool playing = gm == null || gm.State == GameState.Playing;

            // Recargas.
            List<BodyPartId> keys = new List<BodyPartId>(_cooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                if (_cooldowns[keys[i]] > 0f) _cooldowns[keys[i]] -= dt;
            }
            if (_lockMsgTimer > 0f) _lockMsgTimer -= dt;
            if (_stun > 0f) _stun -= dt;
            _swayPhase += dt * 2.1f;

            // Controles cambiados por el alcohol.
            if (drunk > 0f && playing)
            {
                if (_invertTimer > 0f)
                {
                    _invertTimer -= dt;
                }
                else
                {
                    _invertCooldown -= dt;
                    if (_invertCooldown <= 0f)
                    {
                        _invertCooldown = Random.Range(3.5f, 8f) / (0.4f + drunk);
                        if (Random.value < drunk * 0.75f)
                        {
                            _invertTimer = Random.Range(1f, 2f);
                            if (gm != null)
                            {
                                gm.ShowMessage(transform.position + Vector3.up * 2.8f, "¡hip!",
                                    new Color(1f, 0.83f, 0.47f));
                            }
                        }
                    }
                }
            }

            if (playing) HandleActionKeys();
            UpdateActionWindow(dt);
            Move(dt, playing);
        }

        private void HandleActionKeys()
        {
            List<BodyPartData> parts = FBConfig.Parts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (FBInput.Pressed(parts[i].Key)) TryAction(parts[i].Id);
            }
        }

        private void UpdateActionWindow(float dt)
        {
            if (_currentAction == null) return;
            _actionTime += dt;
            bool inWindow = !_actionUsed &&
                            _actionTime >= _currentAction.ActiveFrom &&
                            _actionTime <= _currentAction.ActiveTo;
            SetHitbox(_currentAction.Id, inWindow);

            if (_actionTime >= _currentAction.Duration)
            {
                SetHitbox(_currentAction.Id, false);
                _currentAction = null;
            }
        }

        private void Move(float dt, bool playing)
        {
            Vector3 wish = Vector3.zero;

            if (playing && _stun <= 0f)
            {
                float h = FBInput.Horizontal();
                float v = FBInput.Vertical();
                if (_invertTimer > 0f) { h = -h; v = -v; }

                Vector3 fwd = Vector3.forward;
                Vector3 right = Vector3.right;
                if (cameraPivot != null)
                {
                    fwd = cameraPivot.forward; fwd.y = 0f; fwd.Normalize();
                    right = cameraPivot.right; right.y = 0f; right.Normalize();
                }
                wish = (fwd * v + right * h);
                if (wish.sqrMagnitude > 1f) wish.Normalize();

                if (_cc.isGrounded && FBInput.JumpPressed()) _velocity.y = FBConfig.PlayerJump;
            }

            // Vaivén de borracho: te desplaza aunque no toques nada.
            if (drunk > 0f)
            {
                wish += new Vector3(Mathf.Sin(_swayPhase * 1.3f), 0f, Mathf.Cos(_swayPhase * 0.9f)) * drunk * 0.45f;
            }

            Vector3 target = wish * FBConfig.PlayerSpeed;
            _planarVel = Vector3.MoveTowards(_planarVel, target, FBConfig.PlayerAccel * dt);

            if (_cc.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
            _velocity.y += FBConfig.PlayerGravity * dt;

            Vector3 motion = _planarVel + Vector3.up * _velocity.y;
            _cc.Move(motion * dt);

            // Orientación: hacia donde corres; si estás quieto, mirando al globo.
            Vector3 look = _planarVel;
            look.y = 0f;
            if (look.sqrMagnitude < 0.6f && faceBalloonWhenIdle && GameManager.Instance != null &&
                GameManager.Instance.Balloon != null)
            {
                look = GameManager.Instance.Balloon.transform.position - transform.position;
                look.y = 0f;
            }
            if (look.sqrMagnitude > 0.05f)
            {
                Quaternion want = Quaternion.LookRotation(look.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, want, FBConfig.PlayerTurnSpeed * dt);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            DrunkNpc drunkNpc = hit.collider.GetComponentInParent<DrunkNpc>();
            if (drunkNpc != null && _stun <= 0f)
            {
                Knock(drunkNpc.transform.position);
                drunkNpc.Stumble();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowMessage(drunkNpc.transform.position + Vector3.up * 2.4f,
                        "¡mira por dónde vas!", new Color(1f, 0.61f, 0.42f));
                }
            }
        }

        public void ResetPlayer(Vector3 pos, float drunkLevel)
        {
            CancelAction();
            _cooldowns.Clear();
            _stun = 0f;
            _invertTimer = 0f;
            _planarVel = Vector3.zero;
            _velocity = Vector3.zero;
            drunk = drunkLevel;
            _cc.enabled = false;
            transform.position = pos;
            _cc.enabled = true;
        }
    }
}
