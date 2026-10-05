using System;

namespace SuenaAMueble.Audio
{
    /// <summary>
    /// Rasgos de un sonido fotograma a fotograma (uno cada 10 ms).
    /// Es lo que se compara entre el sonido original y la imitación.
    /// </summary>
    public sealed class FeatureTrack
    {
        public float HopSeconds;
        /// <summary>Volumen de cada fotograma en dBFS (0 = máximo, -60 = casi silencio).</summary>
        public float[] Db;
        /// <summary>Tono en Hz, o 0 si el fotograma no tiene nota clara.</summary>
        public float[] PitchHz;
        /// <summary>0..1: cuánto se parece a una nota limpia (1) o a un ruido (0).</summary>
        public float[] Voicing;
        /// <summary>Centro de gravedad del espectro: alto = brillante ("sss"), bajo = oscuro ("mmm").</summary>
        public float[] CentroidHz;
        /// <summary>0..1: planitud espectral. Alto = ruido ("shhh"), bajo = tono ("uuu").</summary>
        public float[] Flatness;

        public int Length { get { return Db.Length; } }
    }

    /// <summary>
    /// Análisis de audio sin dependencias de Unity (se puede probar fuera del editor).
    /// Todo se pasa primero a 16 kHz mono: sobra para la voz y es rápido.
    /// </summary>
    public static class AudioFeatures
    {
        public const int AnalysisRate = 16000;
        public const float HopSeconds = 0.01f;

        private const int Hop = 160;          // 10 ms
        private const int Window = 640;       // 40 ms
        private const int FftSize = 1024;
        private const float MinPitchHz = 70f;
        private const float MaxPitchHz = 1000f;
        private const float YinThreshold = 0.2f;
        private const float SilenceDb = -65f;

        public static float[] ToMono(float[] interleaved, int channels)
        {
            if (channels <= 1) return interleaved;
            int n = interleaved.Length / channels;
            float[] mono = new float[n];
            for (int i = 0; i < n; i++)
            {
                float s = 0f;
                for (int c = 0; c < channels; c++) s += interleaved[i * channels + c];
                mono[i] = s / channels;
            }
            return mono;
        }

        /// <summary>Cambia la frecuencia de muestreo con un filtro paso bajo previo para no meter alias.</summary>
        public static float[] Resample(float[] input, int fromRate, int toRate)
        {
            if (input == null || input.Length == 0) return new float[0];
            if (fromRate == toRate) return (float[])input.Clone();

            float[] src = input;
            if (toRate < fromRate)
            {
                src = LowPassFir(input, 0.45f * toRate / fromRate, 63);
            }

            double ratio = (double)fromRate / toRate;
            int outLen = (int)(src.Length / ratio);
            float[] output = new float[outLen];
            for (int i = 0; i < outLen; i++)
            {
                double pos = i * ratio;
                int i0 = (int)pos;
                int i1 = Math.Min(i0 + 1, src.Length - 1);
                float frac = (float)(pos - i0);
                output[i] = src[i0] + (src[i1] - src[i0]) * frac;
            }
            return output;
        }

        /// <summary>FIR paso bajo con ventana de Hamming. cutoff en fracción de la frecuencia de muestreo (0..0.5).</summary>
        private static float[] LowPassFir(float[] x, float cutoff, int taps)
        {
            float[] h = new float[taps];
            int mid = taps / 2;
            float sum = 0f;
            for (int i = 0; i < taps; i++)
            {
                int k = i - mid;
                double sinc = k == 0 ? 2.0 * cutoff : Math.Sin(2.0 * Math.PI * cutoff * k) / (Math.PI * k);
                double win = 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * i / (taps - 1));
                h[i] = (float)(sinc * win);
                sum += h[i];
            }
            for (int i = 0; i < taps; i++) h[i] /= sum;

            float[] y = new float[x.Length];
            for (int n = 0; n < x.Length; n++)
            {
                float acc = 0f;
                for (int i = 0; i < taps; i++)
                {
                    int idx = n + i - mid;
                    if (idx >= 0 && idx < x.Length) acc += x[idx] * h[i];
                }
                y[n] = acc;
            }
            return y;
        }

        public static FeatureTrack Extract(float[] samples, int sampleRate)
        {
            float[] x = Resample(samples, sampleRate, AnalysisRate);
            if (x.Length < Window)
            {
                float[] padded = new float[Window];
                Array.Copy(x, padded, x.Length);
                x = padded;
            }

            int frames = (x.Length - Window) / Hop + 1;
            FeatureTrack t = new FeatureTrack();
            t.HopSeconds = HopSeconds;
            t.Db = new float[frames];
            t.PitchHz = new float[frames];
            t.Voicing = new float[frames];
            t.CentroidHz = new float[frames];
            t.Flatness = new float[frames];

            float[] hann = new float[Window];
            for (int i = 0; i < Window; i++) hann[i] = 0.5f - 0.5f * (float)Math.Cos(2.0 * Math.PI * i / (Window - 1));

            float[] re = new float[FftSize];
            float[] im = new float[FftSize];
            int tauMax = (int)(AnalysisRate / MinPitchHz);
            float[] yin = new float[tauMax + 1];

            int binLo = (int)(100f * FftSize / AnalysisRate);
            int binHi = (int)(6000f * FftSize / AnalysisRate);

            for (int f = 0; f < frames; f++)
            {
                int off = f * Hop;

                double energy = 0;
                for (int i = 0; i < Window; i++) energy += x[off + i] * x[off + i];
                float rms = (float)Math.Sqrt(energy / Window);
                float db = 20f * (float)Math.Log10(rms + 1e-9);
                t.Db[f] = db;

                if (db < SilenceDb)
                {
                    t.CentroidHz[f] = 0f;
                    t.Flatness[f] = 1f;
                    continue;
                }

                // ---- espectro
                for (int i = 0; i < FftSize; i++)
                {
                    re[i] = i < Window ? x[off + i] * hann[i] : 0f;
                    im[i] = 0f;
                }
                Fft(re, im);

                double num = 0, den = 0, logSum = 0;
                int count = 0;
                for (int b = binLo; b <= binHi; b++)
                {
                    double p = re[b] * re[b] + im[b] * im[b] + 1e-12;
                    double freq = (double)b * AnalysisRate / FftSize;
                    num += freq * p;
                    den += p;
                    logSum += Math.Log(p);
                    count++;
                }
                t.CentroidHz[f] = (float)(num / den);
                double geo = Math.Exp(logSum / count);
                double arith = den / count;
                t.Flatness[f] = (float)Math.Min(1.0, geo / arith);

                // ---- tono (YIN)
                float conf;
                t.PitchHz[f] = Yin(x, off, Window - tauMax, tauMax, yin, out conf);
                t.Voicing[f] = conf;
            }
            return t;
        }

        /// <summary>Detector de tono YIN (de Cheveigné y Kawahara, 2002). Devuelve 0 si no hay nota.</summary>
        private static float Yin(float[] x, int off, int w, int tauMax, float[] d, out float confidence)
        {
            int tauMin = (int)(AnalysisRate / MaxPitchHz);
            d[0] = 1f;
            float running = 0f;
            for (int tau = 1; tau <= tauMax; tau++)
            {
                double s = 0;
                for (int j = 0; j < w; j++)
                {
                    float diff = x[off + j] - x[off + j + tau];
                    s += diff * diff;
                }
                running += (float)s;
                d[tau] = running > 0f ? (float)s * tau / running : 1f;
            }

            int best = -1;
            for (int tau = tauMin; tau < tauMax; tau++)
            {
                if (d[tau] < YinThreshold)
                {
                    while (tau + 1 < tauMax && d[tau + 1] < d[tau]) tau++;
                    best = tau;
                    break;
                }
            }
            if (best < 0)
            {
                best = tauMin;
                for (int tau = tauMin; tau < tauMax; tau++) if (d[tau] < d[best]) best = tau;
            }

            confidence = Math.Max(0f, Math.Min(1f, 1f - d[best]));

            float refined = best;
            if (best > 1 && best < tauMax)
            {
                float a = d[best - 1], b = d[best], c = d[best + 1];
                float denom = a - 2f * b + c;
                if (Math.Abs(denom) > 1e-6f) refined = best + 0.5f * (a - c) / denom;
            }
            return confidence > 0.5f ? AnalysisRate / refined : 0f;
        }

        /// <summary>FFT radix-2 en el sitio. La longitud tiene que ser potencia de 2.</summary>
        private static void Fft(float[] re, float[] im)
        {
            int n = re.Length;
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1) j ^= bit;
                j ^= bit;
                if (i < j)
                {
                    float tr = re[i]; re[i] = re[j]; re[j] = tr;
                    float ti = im[i]; im[i] = im[j]; im[j] = ti;
                }
            }
            for (int len = 2; len <= n; len <<= 1)
            {
                double ang = -2.0 * Math.PI / len;
                float wRe = (float)Math.Cos(ang), wIm = (float)Math.Sin(ang);
                for (int i = 0; i < n; i += len)
                {
                    float curRe = 1f, curIm = 0f;
                    int half = len >> 1;
                    for (int k = 0; k < half; k++)
                    {
                        int a = i + k, b = i + k + half;
                        float vRe = re[b] * curRe - im[b] * curIm;
                        float vIm = re[b] * curIm + im[b] * curRe;
                        re[b] = re[a] - vRe; im[b] = im[a] - vIm;
                        re[a] += vRe; im[a] += vIm;
                        float nRe = curRe * wRe - curIm * wIm;
                        curIm = curRe * wIm + curIm * wRe;
                        curRe = nRe;
                    }
                }
            }
        }
    }
}
