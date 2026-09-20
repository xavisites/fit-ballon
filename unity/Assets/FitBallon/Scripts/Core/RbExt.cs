using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// Unity 6 renombró Rigidbody.velocity a linearVelocity y drag a linearDamping.
    /// Estos atajos hacen que el mismo código valga en 2021, 2022 y 6.x.
    /// </summary>
    public static class RbExt
    {
        public static Vector3 GetVel(this Rigidbody rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        public static void SetVel(this Rigidbody rb, Vector3 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.velocity = v;
#endif
        }

        public static void SetDrag(this Rigidbody rb, float d)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = d;
#else
            rb.drag = d;
#endif
        }
    }
}
