using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// Cámara de sala tipo diorama, como la referencia: posición fija y ancha,
    /// mirando al punto medio entre el jugador y el globo.
    /// </summary>
    public class RoomCamera : MonoBehaviour
    {
        public Transform player;
        public Transform balloon;
        public Vector3 roomCenter = Vector3.zero;
        public float followStrength = 0.35f;
        public float lookSmooth = 3.5f;
        public float drunkRoll = 2.5f;

        private Vector3 _lookTarget;
        private float _phase;

        private void Start()
        {
            _lookTarget = roomCenter + Vector3.up * 2.5f;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            _phase += dt;

            Vector3 focus = roomCenter + Vector3.up * 2.5f;
            if (player != null && balloon != null)
            {
                Vector3 mid = (player.position + balloon.position) * 0.5f;
                focus = Vector3.Lerp(roomCenter + Vector3.up * 2.5f, mid, followStrength);
            }
            _lookTarget = Vector3.Lerp(_lookTarget, focus, Mathf.Clamp01(lookSmooth * dt));

            Quaternion look = Quaternion.LookRotation((_lookTarget - transform.position).normalized, Vector3.up);

            float drunk = 0f;
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.Player != null) drunk = gm.Player.drunk;
            if (drunk > 0f)
            {
                look = look * Quaternion.Euler(0f, 0f, Mathf.Sin(_phase * 0.8f) * drunkRoll * drunk);
            }
            transform.rotation = look;
        }
    }
}
