using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// Zona de golpeo de una parte del cuerpo. Solo existe para el globo durante
    /// la ventana activa de la animación: si no está activa, el globo la atraviesa.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class BodyPartHitbox : MonoBehaviour
    {
        public BodyPartId part = BodyPartId.Foot;

        private SphereCollider _col;
        private PlayerController _player;
        private bool _active;

        private void Awake()
        {
            _col = GetComponent<SphereCollider>();
            _col.isTrigger = true;
            _col.enabled = false;
            _player = GetComponentInParent<PlayerController>();
        }

        public void SetActive(bool on)
        {
            _active = on;
            if (_col != null) _col.enabled = on;
        }

        private void OnTriggerEnter(Collider other) { Check(other); }
        private void OnTriggerStay(Collider other) { Check(other); }

        private void Check(Collider other)
        {
            if (!_active || _player == null) return;
            BalloonController balloon = other.GetComponentInParent<BalloonController>();
            if (balloon == null) return;
            _player.RegisterHit(part, balloon, transform.position);
        }

        private void OnDrawGizmosSelected()
        {
            BodyPartData data = FBConfig.Get(part);
            SphereCollider c = GetComponent<SphereCollider>();
            if (c == null) return;
            Gizmos.color = data != null ? data.Color : Color.white;
            Gizmos.DrawWireSphere(transform.position, c.radius * transform.lossyScale.x);
        }
    }
}
