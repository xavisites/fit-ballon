using System.Collections.Generic;
using UnityEngine;

namespace SuenaAMueble
{
    public enum PropReaction { Invisible, Sospechoso, Pillado }

    /// <summary>
    /// El objeto en el que estás escondido. Se monta con formas básicas y reacciona a la nota:
    /// salta de alegría, tiembla de nervios, o se pone rojo y le salen ojos (¡pillado!).
    /// Mientras grabas, late con tu voz.
    /// </summary>
    public class PropDisplay : MonoBehaviour
    {
        private Transform _model;
        private Transform _eyes;
        private readonly List<Material> _materials = new List<Material>();
        private readonly List<Color> _baseColors = new List<Color>();

        private float _voiceLevel;
        private float _wobbleTimer, _bounceTimer, _flashTimer, _eyesTimer;
        private bool _eyesOut;

        public void Show(PropDef def)
        {
            if (_model != null) Destroy(_model.gameObject);
            _materials.Clear();
            _baseColors.Clear();
            _wobbleTimer = _bounceTimer = _flashTimer = 0f;
            _eyesOut = false;

            _model = new GameObject("Modelo_" + def.Id).transform;
            _model.SetParent(transform, false);

            Bounds bounds = new Bounds();
            bool first = true;
            foreach (PropShapes.Part part in PropShapes.For(def.Id))
            {
                GameObject go = GameObject.CreatePrimitive(part.Type);
                go.name = part.Type.ToString();
                Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(_model, false);
                go.transform.localPosition = part.Pos;
                go.transform.localEulerAngles = part.Euler;
                go.transform.localScale = part.Scale;
                Material m = Paint(go, part.Color);
                _materials.Add(m);
                _baseColors.Add(part.Color);

                Bounds b = go.GetComponent<Renderer>().bounds;
                if (first) { bounds = b; first = false; }
                else bounds.Encapsulate(b);
            }

            BuildEyes(bounds);
        }

        public static Material Paint(GameObject go, Color color)
        {
            Renderer r = go.GetComponent<Renderer>();
            Material m = new Material(r.sharedMaterial);
            SetColor(m, color);
            r.sharedMaterial = m;
            return m;
        }

        private static void SetColor(Material m, Color c)
        {
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        }

        private void BuildEyes(Bounds worldBounds)
        {
            _eyes = new GameObject("Ojos").transform;
            _eyes.SetParent(_model, false);
            Vector3 local = transform.InverseTransformPoint(worldBounds.center);
            Vector3 size = worldBounds.size;
            float eyeSize = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.28f, 0.12f, 0.3f);
            float y = local.y + size.y * 0.5f - eyeSize * 0.9f;
            float z = local.z - size.z * 0.5f - eyeSize * 0.2f;
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject white = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(white.GetComponent<Collider>());
                white.transform.SetParent(_eyes, false);
                white.transform.localPosition = new Vector3(local.x + side * eyeSize * 0.6f, y, z);
                white.transform.localScale = Vector3.one * eyeSize;
                Paint(white, Color.white);

                GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(pupil.GetComponent<Collider>());
                pupil.transform.SetParent(white.transform, false);
                pupil.transform.localPosition = new Vector3(side * 0.12f, -0.1f, -0.38f);
                pupil.transform.localScale = Vector3.one * 0.45f;
                Paint(pupil, Color.black);
            }
            _eyes.localScale = Vector3.zero;
        }

        /// <summary>Nivel de la voz 0..1 mientras se graba (0 para parar).</summary>
        public void SetVoiceLevel(float level01)
        {
            _voiceLevel = level01;
        }

        public void React(PropReaction reaction)
        {
            switch (reaction)
            {
                case PropReaction.Invisible:
                    _bounceTimer = 0.8f;
                    break;
                case PropReaction.Sospechoso:
                    _wobbleTimer = 1.6f;
                    break;
                case PropReaction.Pillado:
                    _flashTimer = 1.4f;
                    _eyesOut = true;
                    _eyesTimer = 0f;
                    break;
            }
        }

        public void ResetPose()
        {
            _wobbleTimer = _bounceTimer = _flashTimer = 0f;
            _eyesOut = false;
            _voiceLevel = 0f;
            RestoreColors();
        }

        private void RestoreColors()
        {
            for (int i = 0; i < _materials.Count; i++) SetColor(_materials[i], _baseColors[i]);
        }

        private void Update()
        {
            if (_model == null) return;
            float dt = Time.deltaTime;
            float t = Time.time;

            float scale = 1f + 0.012f * Mathf.Sin(t * 2.2f) + _voiceLevel * 0.1f;
            Vector3 pos = Vector3.zero;
            Vector3 euler = Vector3.zero;

            if (_bounceTimer > 0f)
            {
                _bounceTimer -= dt;
                float u = 1f - _bounceTimer / 0.8f;
                pos.y = Mathf.Abs(Mathf.Sin(u * Mathf.PI * 2f)) * 0.25f * (1f - u);
            }
            if (_wobbleTimer > 0f)
            {
                _wobbleTimer -= dt;
                euler.z = Mathf.Sin(t * 38f) * 7f * Mathf.Clamp01(_wobbleTimer / 1.6f);
            }
            if (_voiceLevel > 0.05f)
            {
                euler.z += Mathf.Sin(t * 50f) * _voiceLevel * 2.5f;
            }
            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                bool red = Mathf.Repeat(t * 6f, 1f) < 0.5f && _flashTimer > 0f;
                for (int i = 0; i < _materials.Count; i++)
                {
                    SetColor(_materials[i], red ? Color.Lerp(_baseColors[i], new Color(1f, 0.15f, 0.1f), 0.75f) : _baseColors[i]);
                }
                if (_flashTimer <= 0f) RestoreColors();
            }

            _model.localPosition = pos;
            _model.localEulerAngles = euler;
            _model.localScale = Vector3.one * scale;

            if (_eyes != null)
            {
                if (_eyesOut)
                {
                    _eyesTimer += dt;
                    float k = _eyesTimer / 0.35f;
                    float s = k < 1f ? Mathf.Sin(k * Mathf.PI * 0.5f) * 1.25f : 1f + 0.25f * Mathf.Exp(-(k - 1f) * 3f) * Mathf.Cos((k - 1f) * 12f);
                    _eyes.localScale = Vector3.one * s;
                }
                else
                {
                    _eyes.localScale = Vector3.zero;
                }
            }
        }
    }
}
