using UnityEngine;

namespace FitBallon
{
    /// <summary>Parroquiano borracho: da tumbos por la sala, estorba y desvía el globo.</summary>
    public class DrunkNpc : MonoBehaviour
    {
        public float speed = 1.6f;
        public float roamRadius = 7f;
        public Vector3 roamCenter = Vector3.zero;

        private static readonly string[] Names =
            { "Grombak", "Eldrin", "Marta la Roja", "Bofur", "Tía Nerys", "El Tuerto" };

        private Vector3 _dir;
        private float _stumble;
        private float _turnTimer;
        private float _hiccup;
        private float _phase;
        private string _name;

        public string DisplayName { get { return _name; } }

        private void Awake()
        {
            _name = Names[Random.Range(0, Names.Length)];
            _phase = Random.Range(0f, 10f);
            _hiccup = Random.Range(2f, 6f);
            PickDirection();

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void PickDirection()
        {
            float a = Random.Range(0f, Mathf.PI * 2f);
            _dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            _turnTimer = Random.Range(1.5f, 4f);
        }

        public void Stumble()
        {
            _stumble = Random.Range(0.8f, 1.6f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _phase += dt * 2.4f;

            GameManager gm = GameManager.Instance;
            if (gm != null && gm.State != GameState.Playing) return;

            if (_stumble > 0f)
            {
                _stumble -= dt;
                // Se tambalea en el sitio.
                transform.rotation = Quaternion.Euler(Mathf.Sin(_phase * 3f) * 18f, transform.eulerAngles.y,
                                                      Mathf.Cos(_phase * 2.4f) * 18f);
                return;
            }

            _turnTimer -= dt;
            if (_turnTimer <= 0f) PickDirection();

            Vector3 pos = transform.position + _dir * speed * dt;
            Vector3 offset = pos - roamCenter;
            offset.y = 0f;
            if (offset.magnitude > roamRadius)
            {
                _dir = -_dir;
                PickDirectionKeepFacing();
                pos = transform.position;
            }
            transform.position = pos;

            Vector3 look = _dir;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
            {
                Quaternion want = Quaternion.LookRotation(look.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, want, 4f * dt);
            }
            // Tumbo constante.
            transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, Mathf.Sin(_phase) * 9f);

            _hiccup -= dt;
            if (_hiccup <= 0f)
            {
                _hiccup = Random.Range(3f, 9f);
                if (gm != null) gm.ShowMessage(transform.position + Vector3.up * 2.4f, "¡hip!",
                    new Color(1f, 0.83f, 0.47f));
            }
        }

        private void PickDirectionKeepFacing()
        {
            Vector3 toCenter = roamCenter - transform.position;
            toCenter.y = 0f;
            if (toCenter.sqrMagnitude > 0.01f) _dir = toCenter.normalized;
            _turnTimer = Random.Range(1.5f, 3f);
        }
    }
}
