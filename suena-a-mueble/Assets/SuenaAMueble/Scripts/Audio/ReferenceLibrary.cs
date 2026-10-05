using System.Collections.Generic;
using UnityEngine;

namespace SuenaAMueble.Audio
{
    public sealed class ReferenceSound
    {
        public PropDef Def;
        public AudioClip Clip;
        public float[] Mono;
        public int SampleRate;
        public SoundProfile Profile;
        /// <summary>true si viene de un .wav de Resources/Sonidos; false si es el sintetizado.</summary>
        public bool FromFile;
        public float Duration { get { return (float)Mono.Length / SampleRate; } }
    }

    /// <summary>
    /// Da el sonido original de cada objeto. Si existe Resources/Sonidos/&lt;id&gt; (wav, ogg o mp3)
    /// se usa ese; si no, el sintetizado. Se analiza una vez y se guarda.
    /// </summary>
    public static class ReferenceLibrary
    {
        private const float MaxSeconds = 6f;
        private static readonly Dictionary<string, ReferenceSound> Cache = new Dictionary<string, ReferenceSound>();

        public static ReferenceSound Get(PropDef def)
        {
            ReferenceSound r;
            if (Cache.TryGetValue(def.Id, out r)) return r;

            r = new ReferenceSound();
            r.Def = def;

            AudioClip file = Resources.Load<AudioClip>("Sonidos/" + def.Id);
            if (file != null && TryReadClip(file, r))
            {
                r.FromFile = true;
                r.Clip = file;
            }
            else
            {
                if (file != null)
                {
                    Debug.LogWarning("Suena a Mueble: no se pueden leer las muestras de '" + def.Id +
                                     "'. En el inspector del audio pon Load Type = Decompress On Load. Uso el sintetizado.");
                }
                r.SampleRate = SoundSynth.DefaultRate;
                r.Mono = SoundSynth.Generate(def.Id, r.SampleRate);
                r.Clip = MakeClip("ref_" + def.Id, r.Mono, r.SampleRate);
            }

            r.Profile = ImitationScorer.Analyze(r.Mono, r.SampleRate, -90f);
            r.Profile.HasMelody = def.Melodia;
            Cache[def.Id] = r;
            return r;
        }

        private static bool TryReadClip(AudioClip clip, ReferenceSound r)
        {
            if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
            float[] data = new float[clip.samples * clip.channels];
            if (!clip.GetData(data, 0)) return false;

            float[] mono = AudioFeatures.ToMono(data, clip.channels);
            int max = (int)(MaxSeconds * clip.frequency);
            if (mono.Length > max) System.Array.Resize(ref mono, max);
            r.Mono = mono;
            r.SampleRate = clip.frequency;
            return true;
        }

        public static AudioClip MakeClip(string name, float[] mono, int sampleRate)
        {
            AudioClip c = AudioClip.Create(name, Mathf.Max(1, mono.Length), 1, sampleRate, false);
            if (mono.Length > 0) c.SetData(mono, 0);
            return c;
        }
    }
}
