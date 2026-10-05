using System;

namespace SuenaAMueble.Audio
{
    /// <summary>
    /// Sonidos provisionales fabricados por código, para poder jugar sin ningún asset.
    /// En cuanto haya un .wav de verdad en Resources/Sonidos con el mismo id, se usa ese.
    /// Suenan "a videojuego antiguo", pero tienen el ritmo y la forma de cada objeto.
    /// </summary>
    public static class SoundSynth
    {
        public const int DefaultRate = 44100;

        public static float[] Generate(string id, int sr = DefaultRate)
        {
            Builder b;
            switch (id)
            {
                case "reloj":
                    b = new Builder(3.1f, sr, 1);
                    for (int i = 0; i < 6; i++)
                    {
                        float f = i % 2 == 0 ? 2600f : 1900f;
                        b.Click(0.1f + 0.5f * i, f, 0.6f);
                    }
                    break;

                case "nevera":
                    b = new Builder(2.6f, sr, 2);
                    b.Tone(0.0f, 2.6f, t => 110f + 1.5f * (float)Math.Sin(t * 3.0), Wave.Saw, 0.35f, 0.25f, 0.3f, 900f);
                    b.Noise(0.0f, 2.6f, 0.03f, 0f, 1500f, 0.25f, 0.3f, null);
                    break;

                case "grifo":
                    b = new Builder(2.0f, sr, 3);
                    for (int i = 0; i < 3; i++) b.Drop(0.1f + 0.7f * i, 700f, 1500f, 0.06f, 0.6f);
                    break;

                case "pato":
                    b = new Builder(1.1f, sr, 4);
                    for (int i = 0; i < 2; i++)
                    {
                        float s = 0.1f + 0.45f * i;
                        b.Voice(s, 0.28f, t => 520f - 140f * t / 0.28f, 900f, 1500f, 0.5f, 0.01f, 0.06f, 0.15f);
                    }
                    break;

                case "despertador":
                    b = new Builder(1.9f, sr, 5);
                    for (int g = 0; g < 2; g++)
                        for (int i = 0; i < 4; i++)
                            b.Tone(0.1f + 0.9f * g + 0.16f * i, 0.09f, t => 2000f, Wave.Square, 0.35f, 0.005f, 0.01f, 0f);
                    break;

                case "gato":
                    b = new Builder(1.15f, sr, 6);
                    b.Voice(0.1f, 0.95f, t =>
                    {
                        float u = t / 0.95f;
                        return u < 0.35f ? 450f + 300f * (u / 0.35f) : 750f - 370f * ((u - 0.35f) / 0.65f);
                    }, 650f, 1700f, 0.5f, 0.05f, 0.2f, 0.05f);
                    break;

                case "microondas":
                    b = new Builder(2.5f, sr, 7);
                    b.Tone(0.05f, 1.8f, t => 100f, Wave.Saw, 0.3f, 0.15f, 0.1f, 700f);
                    b.Noise(0.05f, 1.8f, 0.03f, 0f, 1200f, 0.15f, 0.1f, null);
                    b.Tone(2.0f, 0.35f, t => 1000f, Wave.Sine, 0.6f, 0.005f, 0.03f, 0f);
                    break;

                case "lavadora":
                    b = new Builder(3.0f, sr, 8);
                    b.Noise(0.0f, 3.0f, 0.55f, 0f, 600f, 0.3f, 0.2f, t => (0.45f + 0.55f * t / 3f) * (1f - 0.25f * (float)Math.Sin(t * 2.0 * Math.PI * 8.0)));
                    b.Tone(0.0f, 3.0f, t => 55f + 10f * t, Wave.Saw, 0.2f, 0.3f, 0.2f, 500f);
                    break;

                case "telefono":
                    b = new Builder(2.3f, sr, 9);
                    for (int i = 0; i < 2; i++)
                    {
                        float s = 0.1f + 1.2f * i;
                        Func<float, float> trem = t => 0.35f + 0.65f * (0.5f + 0.5f * (float)Math.Sin(t * 2.0 * Math.PI * 18.0));
                        b.Tone(s, 0.8f, t => 1100f, Wave.Sine, 0.3f, 0.01f, 0.05f, 0f, trem);
                        b.Tone(s, 0.8f, t => 1350f, Wave.Sine, 0.25f, 0.01f, 0.05f, 0f, trem);
                    }
                    break;

                case "olla":
                    b = new Builder(2.6f, sr, 10);
                    b.Noise(0.05f, 2.5f, 0.6f, 3000f, 7000f, 0.15f, 0.2f, t => 1f - 0.2f * (float)Math.Sin(t * 2.0 * Math.PI * 6.0));
                    break;

                case "aspiradora":
                    b = new Builder(3.0f, sr, 11);
                    b.Noise(0.0f, 3.0f, 0.4f, 600f, 3000f, 0.3f, 0.2f, null);
                    b.Tone(0.0f, 3.0f, t => 780f, Wave.Saw, 0.12f, 0.3f, 0.2f, 2500f);
                    break;

                case "perro":
                    b = new Builder(1.0f, sr, 12);
                    for (int i = 0; i < 2; i++)
                    {
                        float s = 0.1f + 0.45f * i;
                        b.Voice(s, 0.22f, t => 380f - 130f * t / 0.22f, 700f, 1200f, 0.6f, 0.01f, 0.08f, 0.3f);
                    }
                    break;

                case "sarten":
                    b = new Builder(2.5f, sr, 13);
                    b.Noise(0.0f, 2.5f, 0.3f, 2500f, 8000f, 0.1f, 0.2f, null);
                    b.Crackles(0.0f, 2.5f, 40f, 0.35f);
                    break;

                case "gallo":
                    b = new Builder(1.5f, sr, 14);
                    b.Voice(0.10f, 0.12f, t => 600f, 400f, 2300f, 0.5f, 0.01f, 0.03f, 0.2f);
                    b.Voice(0.27f, 0.12f, t => 700f, 400f, 2300f, 0.5f, 0.01f, 0.03f, 0.2f);
                    b.Voice(0.44f, 0.15f, t => 800f, 500f, 2000f, 0.5f, 0.01f, 0.03f, 0.2f);
                    b.Voice(0.64f, 0.75f, t =>
                    {
                        float u = t / 0.75f;
                        return u < 0.3f ? 850f + 80f * (u / 0.3f) : 930f - 380f * ((u - 0.3f) / 0.7f);
                    }, 400f, 2300f, 0.55f, 0.02f, 0.25f, 0.15f);
                    break;

                case "motosierra":
                    b = new Builder(3.1f, sr, 15);
                    for (int i = 0; i < 2; i++)
                    {
                        float s = 0.1f + 0.45f * i;
                        b.Tone(s, 0.25f, t => 40f + 160f * t, Wave.Saw, 0.4f, 0.02f, 0.06f, 2000f);
                        b.Noise(s, 0.25f, 0.15f, 0f, 2000f, 0.02f, 0.06f, null);
                    }
                    b.Tone(1.0f, 2.0f, t => 70f + 45f * t, Wave.Saw, 0.5f, 0.15f, 0.15f, 2500f);
                    b.Noise(1.0f, 2.0f, 0.2f, 0f, 2500f, 0.15f, 0.15f, null);
                    break;

                case "vater":
                    b = new Builder(2.6f, sr, 16);
                    b.Noise(0.1f, 1.3f, 0.45f, 1500f, 5000f, 0.15f, 0.25f, null);
                    for (int i = 0; i < 4; i++) b.Drop(1.6f + 0.22f * i, 250f, 500f, 0.1f, 0.6f);
                    break;

                case "coche":
                    b = new Builder(2.4f, sr, 17);
                    for (int i = 0; i < 3; i++)
                    {
                        float s = 0.1f + 0.55f * i;
                        Func<float, float> chug = t => 0.5f + 0.5f * (float)Math.Sin(t * 2.0 * Math.PI * 12.0);
                        b.Tone(s, 0.4f, t => 35f, Wave.Saw, 0.45f, 0.02f, 0.08f, 1200f, chug);
                        b.Noise(s, 0.4f, 0.2f, 0f, 1200f, 0.02f, 0.08f, chug);
                    }
                    b.Noise(2.0f, 0.18f, 0.7f, 0f, 800f, 0.005f, 0.1f, null);
                    break;

                case "cortacesped":
                    b = new Builder(3.0f, sr, 18);
                    b.Tone(0.0f, 3.0f, t => 95f + 4f * (float)Math.Sin(t * 2.0 * Math.PI * 4.0), Wave.Saw, 0.4f, 0.3f, 0.3f, 2500f);
                    b.Noise(0.0f, 3.0f, 0.15f, 0f, 2500f, 0.3f, 0.3f, null);
                    break;

                case "licuadora":
                    b = new Builder(2.8f, sr, 19);
                    b.Tone(0.0f, 2.8f, t => 550f + 100f * t / 2.8f, Wave.Saw, 0.3f, 0.2f, 0.2f, 3500f, t => 0.5f + 0.5f * t / 2.8f);
                    b.Noise(0.0f, 2.8f, 0.15f, 1000f, 4000f, 0.2f, 0.2f, t => 0.5f + 0.5f * t / 2.8f);
                    break;

                default:
                    b = new Builder(1.0f, sr, 0);
                    b.Tone(0.1f, 0.5f, t => 440f, Wave.Sine, 0.4f, 0.02f, 0.1f, 0f);
                    break;
            }
            return b.Finish();
        }

        /// <summary>Pitido corto (cuenta atrás, avisos).</summary>
        public static float[] Beep(float freq, float seconds, float amp = 0.4f, int sr = DefaultRate)
        {
            Builder b = new Builder(seconds + 0.02f, sr, 0);
            b.Tone(0f, seconds, t => freq, Wave.Sine, amp, 0.005f, 0.03f, 0f);
            return b.Raw();
        }

        /// <summary>"¡Ding!" de cuando te quedas invisible.</summary>
        public static float[] Ding(int sr = DefaultRate)
        {
            Builder b = new Builder(0.7f, sr, 0);
            b.Tone(0f, 0.6f, t => 1320f, Wave.Sine, 0.25f, 0.005f, 0.5f, 0f);
            b.Tone(0.08f, 0.55f, t => 1760f, Wave.Sine, 0.2f, 0.005f, 0.45f, 0f);
            return b.Raw();
        }

        /// <summary>Zumbido de "¡te han pillado!".</summary>
        public static float[] Buzz(int sr = DefaultRate)
        {
            Builder b = new Builder(0.55f, sr, 0);
            b.Tone(0f, 0.5f, t => 110f, Wave.Square, 0.2f, 0.01f, 0.1f, 1500f);
            b.Tone(0f, 0.5f, t => 116f, Wave.Square, 0.2f, 0.01f, 0.1f, 1500f);
            return b.Raw();
        }

        // ====================================================================
        private enum Wave { Sine, Square, Saw }

        private sealed class Builder
        {
            private readonly float[] _d;
            private readonly int _sr;
            private readonly Random _rng;

            public Builder(float seconds, int sr, int seed)
            {
                _sr = sr;
                _d = new float[(int)(seconds * sr)];
                _rng = new Random(1234 + seed);
            }

            private float Env(float t, float dur, float attack, float release)
            {
                float a = attack > 0f ? Math.Min(1f, t / attack) : 1f;
                float r = release > 0f ? Math.Min(1f, (dur - t) / release) : 1f;
                return Math.Max(0f, Math.Min(a, r));
            }

            public void Tone(float start, float dur, Func<float, float> freq, Wave wave, float amp,
                             float attack, float release, float lowPassHz, Func<float, float> ampMod = null)
            {
                int s0 = (int)(start * _sr), n = (int)(dur * _sr);
                double phase = 0;
                float lp = 0f;
                float a = lowPassHz > 0f ? 1f - (float)Math.Exp(-2.0 * Math.PI * lowPassHz / _sr) : 1f;
                for (int i = 0; i < n && s0 + i < _d.Length; i++)
                {
                    float t = (float)i / _sr;
                    phase += freq(t) / _sr;
                    phase -= Math.Floor(phase);
                    float v;
                    switch (wave)
                    {
                        case Wave.Square: v = phase < 0.5 ? 1f : -1f; break;
                        case Wave.Saw: v = (float)(2.0 * phase - 1.0); break;
                        default: v = (float)Math.Sin(2.0 * Math.PI * phase); break;
                    }
                    lp += a * (v - lp);
                    float m = ampMod != null ? ampMod(t) : 1f;
                    _d[s0 + i] += lp * amp * m * Env(t, dur, attack, release);
                }
            }

            /// <summary>Ruido filtrado. highPassHz 0 = sin paso alto; lowPassHz 0 = sin paso bajo.</summary>
            public void Noise(float start, float dur, float amp, float highPassHz, float lowPassHz,
                              float attack, float release, Func<float, float> ampMod)
            {
                int s0 = (int)(start * _sr), n = (int)(dur * _sr);
                float aLp = lowPassHz > 0f ? 1f - (float)Math.Exp(-2.0 * Math.PI * lowPassHz / _sr) : 1f;
                float aHp = highPassHz > 0f ? 1f - (float)Math.Exp(-2.0 * Math.PI * highPassHz / _sr) : 0f;
                float lp1 = 0f, lp2 = 0f, hpState = 0f;
                for (int i = 0; i < n && s0 + i < _d.Length; i++)
                {
                    float t = (float)i / _sr;
                    float w = (float)(_rng.NextDouble() * 2.0 - 1.0);
                    if (aHp > 0f)
                    {
                        hpState += aHp * (w - hpState);
                        w -= hpState;
                    }
                    lp1 += aLp * (w - lp1);
                    lp2 += aLp * (lp1 - lp2);
                    float m = ampMod != null ? ampMod(t) : 1f;
                    // El ruido filtrado pierde mucha energía: se compensa a ojo.
                    float gain = highPassHz > 0f ? 2.5f : 1.5f;
                    _d[s0 + i] += lp2 * amp * gain * m * Env(t, dur, attack, release);
                }
            }

            /// <summary>Voz sintética: armónicos con dos formantes (suena a "animal de dibujos").</summary>
            public void Voice(float start, float dur, Func<float, float> pitch, float f1, float f2,
                              float amp, float attack, float release, float breath)
            {
                int s0 = (int)(start * _sr), n = (int)(dur * _sr);
                double phase = 0;
                for (int i = 0; i < n && s0 + i < _d.Length; i++)
                {
                    float t = (float)i / _sr;
                    float f0 = pitch(t);
                    phase += f0 / _sr;
                    phase -= Math.Floor(phase);
                    int harmonics = Math.Max(1, (int)(5000f / f0));
                    float v = 0f;
                    for (int k = 1; k <= harmonics; k++)
                    {
                        float fk = f0 * k;
                        float g1 = (fk - f1) / 150f, g2 = (fk - f2) / 220f;
                        float gain = 0.12f + (float)Math.Exp(-g1 * g1) + 0.7f * (float)Math.Exp(-g2 * g2);
                        v += gain / (float)Math.Pow(k, 0.6) * (float)Math.Sin(2.0 * Math.PI * k * phase);
                    }
                    v *= 0.35f;
                    v += breath * (float)(_rng.NextDouble() * 2.0 - 1.0);
                    _d[s0 + i] += v * amp * Env(t, dur, attack, release);
                }
            }

            public void Click(float start, float freq, float amp)
            {
                int s0 = (int)(start * _sr), n = (int)(0.03f * _sr);
                for (int i = 0; i < n && s0 + i < _d.Length; i++)
                {
                    float t = (float)i / _sr;
                    float decay = (float)Math.Exp(-t / 0.006);
                    float v = 0.7f * (float)Math.Sin(2.0 * Math.PI * freq * t) + 0.3f * (float)(_rng.NextDouble() * 2.0 - 1.0);
                    _d[s0 + i] += v * amp * decay;
                }
            }

            /// <summary>Gota: un pitido que sube de tono y se apaga.</summary>
            public void Drop(float start, float fFrom, float fTo, float dur, float amp)
            {
                int s0 = (int)(start * _sr), n = (int)((dur + 0.1f) * _sr);
                double phase = 0;
                for (int i = 0; i < n && s0 + i < _d.Length; i++)
                {
                    float t = (float)i / _sr;
                    float u = Math.Min(1f, t / dur);
                    phase += (fFrom + (fTo - fFrom) * u) / _sr;
                    float decay = (float)Math.Exp(-t / (dur * 0.9f));
                    float attack = Math.Min(1f, t / 0.003f);
                    _d[s0 + i] += (float)Math.Sin(2.0 * Math.PI * phase) * amp * decay * attack;
                }
            }

            public void Crackles(float start, float dur, float perSecond, float amp)
            {
                int count = (int)(dur * perSecond);
                for (int c = 0; c < count; c++)
                {
                    float at = start + (float)_rng.NextDouble() * dur;
                    int s0 = (int)(at * _sr), n = (int)(0.004f * _sr);
                    float a = amp * (0.4f + 0.6f * (float)_rng.NextDouble());
                    for (int i = 0; i < n && s0 + i < _d.Length; i++)
                    {
                        _d[s0 + i] += a * (float)(_rng.NextDouble() * 2.0 - 1.0) * (1f - (float)i / n);
                    }
                }
            }

            public float[] Raw()
            {
                return _d;
            }

            /// <summary>Normaliza a un pico de 0.8 para que todos suenen igual de fuerte.</summary>
            public float[] Finish()
            {
                float peak = 1e-6f;
                for (int i = 0; i < _d.Length; i++) peak = Math.Max(peak, Math.Abs(_d[i]));
                float g = 0.8f / peak;
                for (int i = 0; i < _d.Length; i++) _d[i] *= g;
                return _d;
            }
        }
    }
}
