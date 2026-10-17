using UnityEngine;

namespace FitBallon
{
    /// <summary>
    /// Mueve el Animator del personaje a partir de lo que hace el jugador:
    /// quieto / andando / corriendo, si está en el suelo, y el golpeo.
    ///
    /// No falla si el Animator no tiene alguno de los parámetros: simplemente
    /// no lo manda. Así se puede usar con cualquier controlador.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Referencias (se buscan solas si las dejas vacías)")]
        public PlayerController player;
        public Animator animator;

        [Header("Nombres de los parámetros del Animator")]
        public string speedParam = "Speed";
        public string groundedParam = "Grounded";
        public string kickTrigger = "Kick";

        [Header("Suavizado del cambio andar/correr")]
        public float damping = 0.12f;

        private bool _hasSpeed, _hasGrounded, _hasKick;

        private void Awake()
        {
            if (player == null) player = GetComponentInParent<PlayerController>();
            if (player == null) player = GetComponent<PlayerController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (animator != null)
            {
                // El movimiento lo lleva el CharacterController: si dejamos que lo
                // lleve también la animación, el personaje se va solo.
                animator.applyRootMotion = false;

                _hasSpeed = HasParameter(speedParam, AnimatorControllerParameterType.Float);
                _hasGrounded = HasParameter(groundedParam, AnimatorControllerParameterType.Bool);
                _hasKick = HasParameter(kickTrigger, AnimatorControllerParameterType.Trigger);
            }
            else
            {
                Debug.LogWarning("PlayerAnimator: no encuentro ningún Animator. " +
                                 "Mete el personaje como hijo de Jugador y usa " +
                                 "Fit Ballon ▸ Preparar animaciones del personaje.");
            }
        }

        private bool HasParameter(string name, AnimatorControllerParameterType type)
        {
            if (animator == null || string.IsNullOrEmpty(name)) return false;
            AnimatorControllerParameter[] ps = animator.parameters;
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].name == name && ps[i].type == type) return true;
            }
            return false;
        }

        private void OnEnable()
        {
            if (player != null) player.OnActionStarted += HandleAction;
        }

        private void OnDisable()
        {
            if (player != null) player.OnActionStarted -= HandleAction;
        }

        private void HandleAction(BodyPartData part)
        {
            if (animator != null && _hasKick) animator.SetTrigger(kickTrigger);
        }

        private void Update()
        {
            if (animator == null || player == null) return;

            if (_hasSpeed)
            {
                animator.SetFloat(speedParam, player.LocomotionBlend, damping, Time.deltaTime);
            }
            if (_hasGrounded)
            {
                animator.SetBool(groundedParam, player.IsGrounded);
            }
        }
    }
}
