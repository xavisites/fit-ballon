using System;
using System.Collections.Generic;

namespace SuenaAMueble.Audio
{
    /// <summary>
    /// Todos los números que deciden lo exigente que es la puntuación.
    /// Si en las pruebas con amigos sale demasiado fácil o difícil, se toca aquí.
    /// </summary>
    public sealed class ScoringConfig
    {
        /// <summary>Por debajo de este volumen (dBFS) se considera que no has hecho nada.</summary>
        public float SilenceDb = -50f;
        /// <summary>Lo mínimo que tienes que sobresalir del ruido de fondo para que cuente.</summary>
        public float MinAboveNoiseDb = 8f;
        /// <summary>Margen bajo el pico que cuenta como "sonando".</summary>
        public float ActiveRangeDb = 30f;

        // Golpes (sílabas): se enciende al acercarse al pico y se apaga al caer bastante.
        public float BurstOnDb = 12f;
        public float BurstOffDb = 20f;
        public float BurstMergeGapSeconds = 0.06f;
        public float BurstMinSeconds = 0.03f;

        // Pesos cuando el original tiene melodía (gato, gallo, perro...).
        public float WeightRhythm = 0.40f;
        public float WeightPitch = 0.25f;
        public float WeightBrightness = 0.15f;
        public float WeightNoise = 0.20f;

        // Pesos cuando el original es ruido o golpes (lavadora, reloj...).
        public float WeightRhythmNoPitch = 0.50f;
        public float WeightBrightnessNoPitch = 0.25f;
        public float WeightNoiseNoPitch = 0.25f;

        /// <summary>Generosidad: la nota cruda por debajo de esto vale 0 y por encima de High vale 100.</summary>
        public float RawLow = 0.25f;
        public float RawHigh = 0.85f;
        /// <summary>Si lo has intentado de verdad, nunca sacas menos que esto.</summary>
        public int MinScoreIfTried = 40;

        public static readonly ScoringConfig Default = new ScoringConfig();
    }

    /// <summary>Resumen de un sonido listo para comparar.</summary>
    public sealed class SoundProfile
    {
        public FeatureTrack Track;
        public bool Silent;
        public float PeakDb;
        public float NoiseFloorDb;
        public int Start;
        public int End;
        public float DurationSeconds;
        /// <summary>Volumen 0..1 a lo largo del tramo que suena.</summary>
        public float[] Envelope;
        public int Bursts;
        public float VoicedRatio;
        /// <summary>Melodía en semitonos respecto a la nota media (solo fotogramas con nota).</summary>
        public float[] Contour;
        public float BrightnessOctaves;
        public float Noisiness;
        /// <summary>
        /// Si la melodía cuenta para la nota. Se detecta sola, pero el catálogo la fuerza
        /// (un tic-tac no tiene melodía aunque el detector encuentre algo).
        /// </summary>
        public bool HasMelody;
    }

    public sealed class ScoreResult
    {
        public int Total;
        public bool Silent;
        /// <summary>Subnotas 0..1. Pitch vale -1 si el original no tiene melodía.</summary>
        public float Rhythm, Pitch, Brightness, Noise;
        public float Raw;
        public int RefBursts, ImpBursts;
        public float RefDuration, ImpDuration;
        public float[] RefEnvelope;
        public float[] ImpEnvelope;
        /// <summary>Consejo para la próxima vez.</summary>
        public string Tip;
    }

    /// <summary>
    /// Compara una imitación con el sonido original y da una nota de 0 a 100.
    /// No usa IA: mide ritmo, melodía, brillo y ruido, y las compara con DTW
    /// (que perdona que lo hagas algo más rápido o más lento).
    /// </summary>
    public static class ImitationScorer
    {
        private const int EnvelopePoints = 60;
        private const int ContourPoints = 40;

        /// <summary>
        /// noiseFloorDb: ruido de fondo del micro medido antes de grabar. Si no se sabe (NaN) se
        /// estima con lo más bajo de la propia grabación. Para los sonidos originales se pasa -90:
        /// no tienen ruido de fondo y muchos suenan de principio a fin.
        /// </summary>
        public static SoundProfile Analyze(float[] monoSamples, int sampleRate, float noiseFloorDb = float.NaN, ScoringConfig cfg = null)
        {
            if (cfg == null) cfg = ScoringConfig.Default;
            FeatureTrack t = AudioFeatures.Extract(monoSamples, sampleRate);
            SoundProfile p = new SoundProfile();
            p.Track = t;

            int n = t.Length;
            float peak = -120f;
            for (int i = 0; i < n; i++) if (t.Db[i] > peak) peak = t.Db[i];
            p.PeakDb = peak;
            p.NoiseFloorDb = float.IsNaN(noiseFloorDb) ? Percentile(t.Db, 0.10f) : noiseFloorDb;

            if (peak < cfg.SilenceDb || peak - p.NoiseFloorDb < cfg.MinAboveNoiseDb)
            {
                p.Silent = true;
                p.Envelope = new float[EnvelopePoints];
                p.Contour = new float[0];
                return p;
            }

            float threshold = Math.Max(peak - cfg.ActiveRangeDb, Math.Max(p.NoiseFloorDb + 6f, -60f));
            int start = -1, end = -1;
            for (int i = 0; i < n; i++)
            {
                if (t.Db[i] >= threshold)
                {
                    if (start < 0) start = i;
                    end = i;
                }
            }
            p.Start = start;
            p.End = end;
            int len = end - start + 1;
            p.DurationSeconds = len * t.HopSeconds;

            // Envolvente 0..1 (40 dB por debajo del pico = 0).
            float[] env = new float[len];
            for (int i = 0; i < len; i++)
            {
                env[i] = Clamp01((t.Db[start + i] - (peak - 40f)) / 40f);
            }
            p.Envelope = Smooth(env, 1);

            p.Bursts = CountBursts(t, start, end, peak, cfg);

            // Melodía, brillo y ruido sobre los fotogramas activos.
            List<float> pitches = new List<float>();
            List<float> brightness = new List<float>();
            double noiseSum = 0;
            int active = 0;
            for (int i = start; i <= end; i++)
            {
                if (t.Db[i] < threshold) continue;
                active++;
                if (t.PitchHz[i] > 0f && t.Voicing[i] > 0.75f) pitches.Add(t.PitchHz[i]);
                float c = Math.Max(250f, Math.Min(5000f, t.CentroidHz[i]));
                brightness.Add((float)(Math.Log(c) / Math.Log(2.0)));
                noiseSum += Clamp01((t.Flatness[i] - 0.05f) / 0.45f);
            }
            p.VoicedRatio = active > 0 ? (float)pitches.Count / active : 0f;
            p.Noisiness = active > 0 ? (float)(noiseSum / active) : 0f;
            p.BrightnessOctaves = brightness.Count > 0 ? Median(brightness) : 8f;

            p.HasMelody = p.VoicedRatio > 0.3f && pitches.Count >= 5;
            if (pitches.Count > 0)
            {
                float median = Median(pitches);
                float[] contour = new float[pitches.Count];
                for (int i = 0; i < contour.Length; i++)
                {
                    float st = 12f * (float)(Math.Log(pitches[i] / median) / Math.Log(2.0));
                    contour[i] = Math.Max(-12f, Math.Min(12f, st));
                }
                p.Contour = Smooth(contour, 2);
            }
            else
            {
                p.Contour = new float[0];
            }
            return p;
        }

        public static ScoreResult Compare(SoundProfile reference, SoundProfile imitation, ScoringConfig cfg = null)
        {
            if (cfg == null) cfg = ScoringConfig.Default;
            ScoreResult r = new ScoreResult();
            r.RefEnvelope = ResampleArray(reference.Envelope, EnvelopePoints);
            r.ImpEnvelope = ResampleArray(imitation.Envelope, EnvelopePoints);
            r.RefBursts = reference.Bursts;
            r.ImpBursts = imitation.Bursts;
            r.RefDuration = reference.DurationSeconds;
            r.ImpDuration = imitation.DurationSeconds;

            if (imitation.Silent)
            {
                r.Silent = true;
                r.Total = 0;
                r.Pitch = -1f;
                r.Tip = "No te he oído. Acércate al micro y suéltalo sin vergüenza.";
                return r;
            }

            // ---- ritmo: golpes + forma del volumen + duración
            float burstDiff = Math.Abs(imitation.Bursts - reference.Bursts);
            float burstSim = 1f / (1f + burstDiff / Math.Max(1f, 0.5f * reference.Bursts));
            float envDist = Dtw(r.RefEnvelope, r.ImpEnvelope, EnvelopePoints / 3);
            float envSim = Clamp01(1f - envDist * 2.5f);
            float durRatio = imitation.DurationSeconds / Math.Max(0.05f, reference.DurationSeconds);
            float durSim = (float)Math.Exp(-Math.Abs(Math.Log(Math.Max(0.01f, durRatio))) * 1.2);
            r.Rhythm = 0.45f * burstSim + 0.35f * envSim + 0.20f * durSim;

            // ---- brillo (octavas de diferencia del centro del espectro)
            float octaves = Math.Abs(imitation.BrightnessOctaves - reference.BrightnessOctaves);
            r.Brightness = (float)Math.Exp(-(octaves / 1.3f) * (octaves / 1.3f));

            // ---- ruido contra nota limpia
            r.Noise = Clamp01(1f - Math.Abs(imitation.Noisiness - reference.Noisiness) * 1.6f);

            if (reference.HasMelody && reference.Contour.Length >= 5)
            {
                if (imitation.Contour.Length < 5)
                {
                    r.Pitch = 0.15f;
                }
                else
                {
                    float[] a = ResampleArray(reference.Contour, ContourPoints);
                    float[] b = ResampleArray(imitation.Contour, ContourPoints);
                    float semis = Dtw(a, b, ContourPoints / 3);
                    r.Pitch = (float)Math.Exp(-semis / 3.5f);
                    float voicedPenalty = Math.Min(1f, imitation.VoicedRatio / Math.Max(0.05f, reference.VoicedRatio) * 1.5f);
                    r.Pitch *= voicedPenalty;
                }
                r.Raw = cfg.WeightRhythm * r.Rhythm + cfg.WeightPitch * r.Pitch
                      + cfg.WeightBrightness * r.Brightness + cfg.WeightNoise * r.Noise;
            }
            else
            {
                r.Pitch = -1f;
                r.Raw = cfg.WeightRhythmNoPitch * r.Rhythm + cfg.WeightBrightnessNoPitch * r.Brightness
                      + cfg.WeightNoiseNoPitch * r.Noise;
            }

            float g = Clamp01((r.Raw - cfg.RawLow) / (cfg.RawHigh - cfg.RawLow));
            int total = (int)Math.Round(100.0 * Math.Pow(g, 0.85));
            bool tried = imitation.DurationSeconds >= 0.3f * reference.DurationSeconds;
            if (tried) total = Math.Max(total, cfg.MinScoreIfTried);
            r.Total = Math.Max(0, Math.Min(100, total));
            r.Tip = BuildTip(r, reference, imitation, burstSim, durRatio);
            return r;
        }

        // ------------------------------------------------------------------ consejos
        private static string BuildTip(ScoreResult r, SoundProfile reference, SoundProfile imitation, float burstSim, float durRatio)
        {
            if (r.Total >= 90) return "Perfecto. Ni el cazador más fino te pillaría.";

            float pitchGap = r.Pitch < 0f ? 1f : r.Pitch;
            float worst = Math.Min(Math.Min(r.Rhythm, pitchGap), Math.Min(r.Brightness, r.Noise));

            if (worst == r.Rhythm)
            {
                if (burstSim < 0.75f && reference.Bursts != imitation.Bursts)
                {
                    return "Cuenta los golpes: el original tiene " + reference.Bursts +
                           " y tú hiciste " + imitation.Bursts + ".";
                }
                if (durRatio < 0.7f) return "Te quedas corto: alárgalo más.";
                if (durRatio > 1.4f) return "Te alargas demasiado: córtalo antes.";
                return "Fíjate en cuándo sube y baja el volumen del original.";
            }
            if (worst == pitchGap)
            {
                return "Sigue la melodía: sube y baja la voz como el original.";
            }
            if (worst == r.Brightness)
            {
                return imitation.BrightnessOctaves < reference.BrightnessOctaves
                    ? "Más agudo y brillante: prueba con \"iii\" o \"sss\"."
                    : "Más grave y oscuro: prueba con \"ooo\" o \"mmm\".";
            }
            return imitation.Noisiness < reference.Noisiness
                ? "Más soplido y menos nota: \"shhh\", \"fff\", \"rrr\"."
                : "Menos soplido y más nota: \"uuu\", \"mmm\", \"iii\".";
        }

        // ------------------------------------------------------------------ golpes
        private static int CountBursts(FeatureTrack t, int start, int end, float peak, ScoringConfig cfg)
        {
            float[] db = new float[end - start + 1];
            Array.Copy(t.Db, start, db, 0, db.Length);
            db = Smooth(db, 2);

            float on = peak - cfg.BurstOnDb;
            float off = peak - cfg.BurstOffDb;
            int minLen = Math.Max(1, (int)(cfg.BurstMinSeconds / t.HopSeconds));
            int mergeGap = (int)(cfg.BurstMergeGapSeconds / t.HopSeconds);

            int bursts = 0;
            bool inBurst = false;
            int burstStart = 0, lastEnd = -1000;
            for (int i = 0; i < db.Length; i++)
            {
                if (!inBurst && db[i] >= on)
                {
                    inBurst = true;
                    burstStart = i;
                }
                else if (inBurst && db[i] < off)
                {
                    inBurst = false;
                    RegisterBurst(burstStart, i, minLen, mergeGap, ref bursts, ref lastEnd);
                }
            }
            if (inBurst) RegisterBurst(burstStart, db.Length, minLen, mergeGap, ref bursts, ref lastEnd);
            return Math.Max(1, bursts);
        }

        private static void RegisterBurst(int s, int e, int minLen, int mergeGap, ref int bursts, ref int lastEnd)
        {
            if (e - s < minLen) return;
            if (s - lastEnd > mergeGap) bursts++;
            lastEnd = e;
        }

        // ------------------------------------------------------------------ utilidades
        /// <summary>Distancia DTW media por paso (con banda Sakoe-Chiba).</summary>
        public static float Dtw(float[] a, float[] b, int band)
        {
            int n = a.Length, m = b.Length;
            if (n == 0 || m == 0) return 1f;
            band = Math.Max(band, Math.Abs(n - m));
            float[,] cost = new float[n + 1, m + 1];
            int[,] steps = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++)
                for (int j = 0; j <= m; j++) cost[i, j] = float.PositiveInfinity;
            cost[0, 0] = 0f;

            for (int i = 1; i <= n; i++)
            {
                int jFrom = Math.Max(1, i - band), jTo = Math.Min(m, i + band);
                for (int j = jFrom; j <= jTo; j++)
                {
                    float d = Math.Abs(a[i - 1] - b[j - 1]);
                    float best = cost[i - 1, j - 1];
                    int bestSteps = steps[i - 1, j - 1];
                    if (cost[i - 1, j] < best) { best = cost[i - 1, j]; bestSteps = steps[i - 1, j]; }
                    if (cost[i, j - 1] < best) { best = cost[i, j - 1]; bestSteps = steps[i, j - 1]; }
                    cost[i, j] = best + d;
                    steps[i, j] = bestSteps + 1;
                }
            }
            return steps[n, m] > 0 ? cost[n, m] / steps[n, m] : 1f;
        }

        public static float[] ResampleArray(float[] src, int count)
        {
            float[] dst = new float[count];
            if (src == null || src.Length == 0) return dst;
            if (src.Length == 1)
            {
                for (int i = 0; i < count; i++) dst[i] = src[0];
                return dst;
            }
            for (int i = 0; i < count; i++)
            {
                float pos = (float)i * (src.Length - 1) / (count - 1);
                int i0 = (int)pos;
                int i1 = Math.Min(i0 + 1, src.Length - 1);
                dst[i] = src[i0] + (src[i1] - src[i0]) * (pos - i0);
            }
            return dst;
        }

        private static float[] Smooth(float[] x, int radius)
        {
            if (radius <= 0 || x.Length == 0) return x;
            float[] y = new float[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                float s = 0f;
                int c = 0;
                for (int k = -radius; k <= radius; k++)
                {
                    int j = i + k;
                    if (j < 0 || j >= x.Length) continue;
                    s += x[j];
                    c++;
                }
                y[i] = s / c;
            }
            return y;
        }

        private static float Percentile(float[] x, float q)
        {
            if (x.Length == 0) return -120f;
            float[] s = (float[])x.Clone();
            Array.Sort(s);
            return s[Math.Min(s.Length - 1, (int)(q * s.Length))];
        }

        private static float Median(List<float> x)
        {
            float[] s = x.ToArray();
            Array.Sort(s);
            return s[s.Length / 2];
        }

        private static float Clamp01(float v)
        {
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }
    }
}
