using UnityEngine;

namespace FitBallon
{
    /// <summary>Fuego: si el globo entra, explota.</summary>
    public class HazardZone : MonoBehaviour
    {
        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            BalloonController balloon = other.GetComponentInParent<BalloonController>();
            if (balloon != null) balloon.Pop();
        }
    }
}
