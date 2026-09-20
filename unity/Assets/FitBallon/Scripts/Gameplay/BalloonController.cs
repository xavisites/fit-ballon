using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// El globo. No usa la gravedad de Unity: cae con su propia aceleración (flota)
    /// y rebota resolviendo los choques a mano, para que el tacto sea el mismo
    /// contra muros, columnas, candelabros y borrachos.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class BalloonController : MonoBehaviour
    {
        [Header("Ajustes")]
        public float wobbleStrength = 0.35f;
        public float spawnHeight = 6.5f;

        private Rigidbody _rb;
        private SphereCollider _col;
        private Vector3 _lastVel;
        private float _wobblePhase;
        private bool _alive = true;

        public Rigidbody Body { get { return _rb; } }
        public Collider Col { get { return _col; } }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<SphereCollider>();
            _rb.useGravity = false;
            _rb.SetDrag(FBConfig.BalloonDrag);
            _rb.mass = 0.3f;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _wobblePhase = Random.Range(0f, 10f);
        }

        private float FallAccel()
        {
            GameManager gm = GameManager.Instance;
            float a = FBConfig.BalloonFallAccel;
            if (gm != null)
            {
                a += (gm.Level - 1) * FBConfig.BalloonFallPerLevel;
                if (gm.PenaltyTimer > 0f) a += FBConfig.BalloonPenaltyAccel;
            }
            return Mathf.Min(a, FBConfig.BalloonFallMax);
        }

        private void FixedUpdate()
        {
            if (!_alive) return;

            float dt = Time.fixedDeltaTime;
            _wobblePhase += dt * 1.3f;

            Vector3 v = _rb.GetVel();
            v += Vector3.down * FallAccel() * dt;

            // Corriente de aire de la sala: el globo nunca cae en línea recta.
            Vector3 wobble = new Vector3(Mathf.Sin(_wobblePhase * 1.7f), 0f, Mathf.Cos(_wobblePhase * 1.1f));
            v += wobble * wobbleStrength * dt;

            if (v.magnitude > FBConfig.BalloonMaxSpeed)
            {
                v = v.normalized * FBConfig.BalloonMaxSpeed;
            }
            _rb.SetVel(v);
            _lastVel = v;
        }

        private void OnCollisionEnter(Collision c)
        {
            if (!_alive) return;

            Vector3 n = c.contacts.Length > 0 ? c.contacts[0].normal : Vector3.up;
            Vector3 reflected = Vector3.Reflect(_lastVel, n) * FBConfig.BalloonBounce;

            DrunkNpc drunk = c.collider.GetComponentInParent<DrunkNpc>();
            if (drunk != null)
            {
                // Rebote de taberna: impredecible, pero da puntos.
                reflected = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(3f, 5f), Random.Range(-2.5f, 2.5f));
                drunk.Stumble();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(15, drunk.transform.position + Vector3.up * 2.2f,
                        "+15 " + drunk.DisplayName);
                }
            }

            _rb.SetVel(reflected);
            _lastVel = reflected;
        }

        /// <summary>Impulso de una parte del cuerpo.</summary>
        public void Kick(Vector3 velocity)
        {
            if (!_alive) return;
            _rb.SetVel(velocity);
            _lastVel = velocity;
        }

        public void Pop()
        {
            if (!_alive) return;
            if (GameManager.Instance != null) GameManager.Instance.LoseLife("¡El globo ha ardido!");
        }

        public void Respawn(Vector3 center)
        {
            _alive = true;
            transform.position = new Vector3(center.x + Random.Range(-2f, 2f), spawnHeight,
                                             center.z + Random.Range(-2f, 2f));
            _rb.SetVel(Vector3.zero);
            _lastVel = Vector3.zero;
            _wobblePhase = Random.Range(0f, 10f);
        }

        public void Freeze(bool frozen)
        {
            _alive = !frozen;
            if (frozen) _rb.SetVel(Vector3.zero);
        }
    }
}
