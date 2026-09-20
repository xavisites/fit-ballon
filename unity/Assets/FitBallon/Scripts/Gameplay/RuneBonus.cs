using UnityEngine;

namespace FitBallon
{
    /// <summary>Runa flotante: si el globo la atraviesa, puntos extra.</summary>
    public class RuneBonus : MonoBehaviour
    {
        public int points = 60;
        public float respawnDelay = 7f;

        private Renderer[] _renderers;
        private Collider _col;
        private float _timer;
        private float _phase;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _col = GetComponent<Collider>();
            if (_col != null) _col.isTrigger = true;
            _phase = Random.Range(0f, 10f);
        }

        private void Update()
        {
            _phase += Time.deltaTime;
            transform.Rotate(Vector3.up, 45f * Time.deltaTime, Space.World);

            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f) SetVisible(true);
            }
        }

        private void SetVisible(bool on)
        {
            if (_col != null) _col.enabled = on;
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null) _renderers[i].enabled = on;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_timer > 0f) return;
            BalloonController balloon = other.GetComponentInParent<BalloonController>();
            if (balloon == null) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(points, transform.position, "+" + points + " RUNA");
            }
            SetVisible(false);
            _timer = respawnDelay;
        }
    }
}
