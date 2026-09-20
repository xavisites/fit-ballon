using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace FitBallon
{
    /// <summary>
    /// Capa de teclado que funciona con el Input Manager clásico y con el Input System nuevo,
    /// para que el proyecto no reviente según cómo esté configurado Unity.
    /// </summary>
    public static class FBInput
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private static Key ToKey(KeyCode code)
        {
            switch (code)
            {
                case KeyCode.Z: return Key.Z;
                case KeyCode.X: return Key.X;
                case KeyCode.C: return Key.C;
                case KeyCode.V: return Key.V;
                case KeyCode.B: return Key.B;
                case KeyCode.P: return Key.P;
                case KeyCode.R: return Key.R;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.W: return Key.W;
                case KeyCode.A: return Key.A;
                case KeyCode.S: return Key.S;
                case KeyCode.D: return Key.D;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                default: return Key.None;
            }
        }
#endif

        public static bool Held(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard kb = Keyboard.current;
            Key k = ToKey(code);
            if (kb == null || k == Key.None) return false;
            return kb[k].isPressed;
#else
            return Input.GetKey(code);
#endif
        }

        public static bool Pressed(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard kb = Keyboard.current;
            Key k = ToKey(code);
            if (kb == null || k == Key.None) return false;
            return kb[k].wasPressedThisFrame;
#else
            return Input.GetKeyDown(code);
#endif
        }

        /// <summary>Eje horizontal (A/D o flechas).</summary>
        public static float Horizontal()
        {
            float h = 0f;
            if (Held(KeyCode.A) || Held(KeyCode.LeftArrow)) h -= 1f;
            if (Held(KeyCode.D) || Held(KeyCode.RightArrow)) h += 1f;
            return h;
        }

        /// <summary>Eje de profundidad (W/S o flechas).</summary>
        public static float Vertical()
        {
            float v = 0f;
            if (Held(KeyCode.S) || Held(KeyCode.DownArrow)) v -= 1f;
            if (Held(KeyCode.W) || Held(KeyCode.UpArrow)) v += 1f;
            return v;
        }

        public static bool JumpPressed() { return Pressed(KeyCode.Space); }
    }
}
