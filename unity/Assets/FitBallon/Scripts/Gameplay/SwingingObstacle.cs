using UnityEngine;

namespace FitBallon
{
    /// <summary>Candelabro colgante: se balancea como un péndulo y descoloca el globo.</summary>
    public class SwingingObstacle : MonoBehaviour
    {
        public float amplitude = 35f;      // grados
        public float period = 3.4f;        // segundos por ciclo
        public Vector3 axis = Vector3.forward;

        private float _t;
        private Quaternion _base;

        private void Awake()
        {
            _base = transform.localRotation;
            _t = Random.Range(0f, period);

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            _t += Time.fixedDeltaTime;
            float angle = Mathf.Sin((_t / period) * Mathf.PI * 2f) * amplitude;
            transform.localRotation = _base * Quaternion.AngleAxis(angle, axis.normalized);
        }
    }
}
