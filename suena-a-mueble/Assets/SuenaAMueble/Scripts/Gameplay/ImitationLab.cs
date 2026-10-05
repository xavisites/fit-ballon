using System.Collections;
using System.Collections.Generic;
using SuenaAMueble.Audio;
using UnityEngine;

namespace SuenaAMueble
{
    public enum LabState { Menu, Elegir, Listo, Escuchando, CuentaAtras, Grabando, Analizando, Resultado, RetoFin }
    public enum LabMode { Libre, Reto }

    /// <summary>Un intento: lo que hiciste y la nota que sacaste.</summary>
    public sealed class Attempt
    {
        public PropDef Def;
        public ScoreResult Result;
        public AudioClip Voice;
        public int Points;
    }

    /// <summary>
    /// Laboratorio de imitación (semanas 1-2 del plan): suena el objeto, lo imitas
    /// con el micro y te dice si el cazador te pillaría. Sin multijugador todavía.
    /// </summary>
    public class ImitationLab : MonoBehaviour
    {
        public const int RetoLength = 5;

        public PropDisplay prop;
        public MicCapture mic;

        public LabState State { get; private set; }
        public LabMode Mode { get; private set; }
        public PropDef Current { get; private set; }
        public ReferenceSound CurrentRef { get; private set; }
        public Attempt LastAttempt { get; private set; }
        public string CountdownText { get; private set; }
        public float RecordSeconds { get; private set; }
        public float RecordElapsed { get; private set; }
        public string Message { get; set; }
        public bool HunterMixPlaying { get { return _voiceSource.isPlaying && _voiceFilter.enabled; } }

        public int RetoIndex { get; private set; }
        public int RetoPoints { get; private set; }
        public readonly List<Attempt> RetoAttempts = new List<Attempt>();

        private AudioSource _refSource;
        private AudioSource _voiceSource;
        private AudioSource _sfxSource;
        private AudioLowPassFilter _voiceFilter;
        private AudioClip _tick, _go, _ding, _buzz;
        private readonly List<PropDef> _retoQueue = new List<PropDef>();
        private Coroutine _flow;

        private void Awake()
        {
            _refSource = gameObject.AddComponent<AudioSource>();
            _refSource.playOnAwake = false;

            GameObject voiceGo = new GameObject("VozDelEscondido");
            voiceGo.transform.SetParent(transform, false);
            _voiceSource = voiceGo.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceFilter = voiceGo.AddComponent<AudioLowPassFilter>();
            _voiceFilter.cutoffFrequency = 2200f;   // "voz saliendo de dentro de la nevera"
            _voiceFilter.enabled = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            int sr = SoundSynth.DefaultRate;
            _tick = ReferenceLibrary.MakeClip("tick", SoundSynth.Beep(880f, 0.1f), sr);
            _go = ReferenceLibrary.MakeClip("go", SoundSynth.Beep(1320f, 0.18f, 0.5f), sr);
            _ding = ReferenceLibrary.MakeClip("ding", SoundSynth.Ding(), sr);
            _buzz = ReferenceLibrary.MakeClip("buzz", SoundSynth.Buzz(), sr);

            State = LabState.Menu;
        }

        // ================================================================ menú
        public void GoToMenu()
        {
            StopFlow();
            StopAudio();
            State = LabState.Menu;
            Message = null;
        }

        public void StartFree()
        {
            Mode = LabMode.Libre;
            StopAudio();
            State = LabState.Elegir;
            Message = null;
        }

        public void StartChallenge()
        {
            Mode = LabMode.Reto;
            RetoAttempts.Clear();
            RetoPoints = 0;
            RetoIndex = 0;
            _retoQueue.Clear();
            List<PropDef> pool = new List<PropDef>(PropCatalog.All);
            for (int i = 0; i < RetoLength && pool.Count > 0; i++)
            {
                int k = Random.Range(0, pool.Count);
                _retoQueue.Add(pool[k]);
                pool.RemoveAt(k);
            }
            Choose(_retoQueue[0]);
            BeginAttempt();
        }

        public void Choose(PropDef def)
        {
            StopFlow();
            StopAudio();
            Current = def;
            CurrentRef = ReferenceLibrary.Get(def);
            LastAttempt = null;
            prop.Show(def);
            State = LabState.Listo;
            Message = null;
        }

        // ================================================================ intento
        public void PlayReference()
        {
            if (CurrentRef == null) return;
            StopAudio();
            _refSource.clip = CurrentRef.Clip;
            _refSource.volume = 1f;
            _refSource.Play();
        }

        public void BeginAttempt()
        {
            if (Current == null) return;
            if (!mic.Available)
            {
                Message = "Sin micrófono no se puede jugar: " + mic.DisplayName;
                return;
            }
            StopFlow();
            _flow = StartCoroutine(AttemptFlow());
        }

        private IEnumerator AttemptFlow()
        {
            prop.ResetPose();
            Message = null;

            // 1. Escuchas el original una vez (como antes de cada alarma).
            State = LabState.Escuchando;
            PlayReference();
            yield return new WaitForSeconds(CurrentRef.Duration + 0.35f);

            // 2. Cuenta atrás.
            State = LabState.CuentaAtras;
            for (int i = 3; i >= 1; i--)
            {
                CountdownText = i.ToString();
                _sfxSource.PlayOneShot(_tick);
                yield return new WaitForSeconds(0.55f);
            }
            CountdownText = "¡YA!";
            _sfxSource.PlayOneShot(_go);
            yield return new WaitForSeconds(0.2f);

            // 3. Grabas. El micro ya está grabando en bucle: solo hay que esperar y leer.
            float noiseFloor = mic.NoiseFloorDb;
            State = LabState.Grabando;
            RecordSeconds = Mathf.Clamp(CurrentRef.Duration + 1.5f, 3f, 6.5f);
            RecordElapsed = 0f;
            while (RecordElapsed < RecordSeconds)
            {
                RecordElapsed += Time.deltaTime;
                prop.SetVoiceLevel(mic.Level01);
                yield return null;
            }
            prop.SetVoiceLevel(0f);

            // 4. Nota.
            State = LabState.Analizando;
            float[] voice = mic.ReadLast(RecordSeconds);
            yield return null;   // que se vea "Analizando..." un fotograma

            SoundProfile imitation = ImitationScorer.Analyze(voice, mic.SampleRate, noiseFloor);
            ScoreResult result = ImitationScorer.Compare(CurrentRef.Profile, imitation);

            Attempt a = new Attempt();
            a.Def = Current;
            a.Result = result;
            a.Voice = ReferenceLibrary.MakeClip("voz_" + Current.Id, Normalize(voice), mic.SampleRate);
            a.Points = result.Total * Current.Dificultad;
            LastAttempt = a;

            if (Mode == LabMode.Reto)
            {
                RetoAttempts.Add(a);
                RetoPoints += a.Points;
            }

            State = LabState.Resultado;
            PropReaction reaction = ReactionFor(result.Total);
            prop.React(reaction);
            _sfxSource.PlayOneShot(reaction == PropReaction.Pillado ? _buzz : (reaction == PropReaction.Invisible ? _ding : _tick));

            // 5. Lo mejor del juego: oír lo que oiría el cazador.
            yield return new WaitForSeconds(0.7f);
            PlayHunterMix();
            _flow = null;
        }

        public static PropReaction ReactionFor(int total)
        {
            if (total >= 80) return PropReaction.Invisible;
            if (total >= 50) return PropReaction.Sospechoso;
            return PropReaction.Pillado;
        }

        public void PlayMyVoice(Attempt a)
        {
            if (a == null) return;
            StopAudio();
            _voiceFilter.enabled = false;
            _voiceSource.clip = a.Voice;
            _voiceSource.volume = 1f;
            _voiceSource.Play();
        }

        public void PlayHunterMix()
        {
            PlayHunterMix(LastAttempt);
        }

        /// <summary>
        /// Lo que oye el cazador: con buena nota, el objeto de verdad; con mala, tu voz
        /// (amortiguada, como si saliera de dentro del mueble).
        /// </summary>
        public void PlayHunterMix(Attempt a)
        {
            if (a == null) return;
            StopAudio();
            ReferenceSound r = ReferenceLibrary.Get(a.Def);
            float score = a.Result.Total;
            _refSource.clip = r.Clip;
            _refSource.volume = Mathf.Clamp01((score - 30f) / 50f);
            _voiceFilter.enabled = true;
            _voiceSource.clip = a.Voice;
            _voiceSource.volume = Mathf.Clamp01((80f - score) / 40f);
            if (_refSource.volume > 0f) _refSource.Play();
            if (_voiceSource.volume > 0f) _voiceSource.Play();
        }

        public void Retry()
        {
            if (Mode == LabMode.Reto) return;   // en el reto no hay segunda oportunidad
            BeginAttempt();
        }

        public void Next()
        {
            if (Mode == LabMode.Libre)
            {
                StartFree();
                return;
            }
            RetoIndex++;
            if (RetoIndex >= _retoQueue.Count)
            {
                StopAudio();
                State = LabState.RetoFin;
                return;
            }
            Choose(_retoQueue[RetoIndex]);
            BeginAttempt();
        }

        public Attempt BestAttempt()
        {
            Attempt best = null;
            foreach (Attempt a in RetoAttempts) if (best == null || a.Result.Total > best.Result.Total) best = a;
            return best;
        }

        public Attempt WorstAttempt()
        {
            Attempt worst = null;
            foreach (Attempt a in RetoAttempts) if (worst == null || a.Result.Total < worst.Result.Total) worst = a;
            return worst;
        }

        // ================================================================ utilidades
        private void StopFlow()
        {
            if (_flow != null) StopCoroutine(_flow);
            _flow = null;
            if (prop != null) prop.SetVoiceLevel(0f);
        }

        private void StopAudio()
        {
            _refSource.Stop();
            _voiceSource.Stop();
        }

        /// <summary>Sube el volumen de la grabación para que se oiga bien al reproducirla.</summary>
        private static float[] Normalize(float[] x)
        {
            float peak = 1e-4f;
            for (int i = 0; i < x.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(x[i]));
            float g = Mathf.Min(0.9f / peak, 20f);
            float[] y = new float[x.Length];
            for (int i = 0; i < x.Length; i++) y[i] = x[i] * g;
            return y;
        }
    }
}
