using UnityEngine;

namespace SuenaAMueble.Audio
{
    /// <summary>
    /// Mantiene el micrófono grabando en bucle todo el rato (así no hay retraso al empezar)
    /// y permite leer los últimos N segundos. También da el nivel para el medidor y una
    /// estimación del ruido de fondo de la habitación.
    /// </summary>
    public class MicCapture : MonoBehaviour
    {
        private const int BufferSeconds = 12;
        private const int PreferredRate = 44100;
        private const int MeterSamples = 1024;

        public AudioClip Clip { get; private set; }
        public int SampleRate { get; private set; }
        /// <summary>null = micrófono predeterminado del sistema.</summary>
        public string DeviceName { get; private set; }
        public string Error { get; private set; }

        /// <summary>Nivel actual en dBFS y normalizado 0..1 para el medidor.</summary>
        public float LevelDb { get; private set; }
        public float Level01 { get; private set; }
        /// <summary>Ruido de fondo estimado (dBFS) con los momentos más tranquilos de los últimos segundos.</summary>
        public float NoiseFloorDb { get; private set; }

        public bool Available { get { return Clip != null && string.IsNullOrEmpty(Error); } }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Error)) return Error;
                return string.IsNullOrEmpty(DeviceName) ? "Micrófono predeterminado" : DeviceName;
            }
        }

        private readonly float[] _meter = new float[MeterSamples];
        private readonly float[] _history = new float[150];   // ~5 s de niveles a 30 fps
        private int _historyIdx;
        private float _historyTimer;
        private int _deviceIndex = -1;

        private void Start()
        {
            NoiseFloorDb = -60f;
            for (int i = 0; i < _history.Length; i++) _history[i] = -60f;
            Begin(null);
        }

        public void Begin(string device)
        {
            Stop();
            Error = null;
            if (Microphone.devices.Length == 0)
            {
                Error = "No se encuentra ningún micrófono";
                return;
            }

            int minF, maxF;
            Microphone.GetDeviceCaps(device, out minF, out maxF);
            int rate = PreferredRate;
            if (minF != 0 || maxF != 0) rate = Mathf.Clamp(PreferredRate, minF, maxF);

            DeviceName = device;
            Clip = Microphone.Start(device, true, BufferSeconds, rate);
            if (Clip == null)
            {
                Error = "No se ha podido abrir el micrófono";
                return;
            }
            SampleRate = Clip.frequency;
        }

        /// <summary>Pasa al siguiente micrófono de la lista (y vuelve al predeterminado al final).</summary>
        public void NextDevice()
        {
            string[] devices = Microphone.devices;
            if (devices.Length == 0)
            {
                Begin(null);
                return;
            }
            _deviceIndex++;
            if (_deviceIndex >= devices.Length) _deviceIndex = -1;
            Begin(_deviceIndex < 0 ? null : devices[_deviceIndex]);
        }

        public void Stop()
        {
            if (Clip != null && Microphone.IsRecording(DeviceName)) Microphone.End(DeviceName);
            Clip = null;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Update()
        {
            if (!Available)
            {
                LevelDb = -90f;
                Level01 = 0f;
                return;
            }

            int pos = Microphone.GetPosition(DeviceName);
            int start = pos - MeterSamples;
            if (start < 0) start += Clip.samples;
            Clip.GetData(_meter, start);

            double sum = 0;
            for (int i = 0; i < _meter.Length; i++) sum += _meter[i] * _meter[i];
            float rms = Mathf.Sqrt((float)(sum / _meter.Length));
            LevelDb = 20f * Mathf.Log10(rms + 1e-9f);
            Level01 = Mathf.Clamp01((LevelDb + 60f) / 55f);

            _historyTimer += Time.unscaledDeltaTime;
            if (_historyTimer >= 1f / 30f)
            {
                _historyTimer = 0f;
                _history[_historyIdx] = LevelDb;
                _historyIdx = (_historyIdx + 1) % _history.Length;

                float[] sorted = (float[])_history.Clone();
                System.Array.Sort(sorted);
                NoiseFloorDb = Mathf.Max(-70f, sorted[sorted.Length / 10]);
            }
        }

        /// <summary>Los últimos <paramref name="seconds"/> segundos captados, en mono.</summary>
        public float[] ReadLast(float seconds)
        {
            if (!Available) return new float[0];
            int n = Mathf.Min(Mathf.RoundToInt(seconds * SampleRate), Clip.samples - 1);
            int pos = Microphone.GetPosition(DeviceName);
            int start = pos - n;
            if (start < 0) start += Clip.samples;

            float[] data = new float[n * Clip.channels];
            Clip.GetData(data, start);   // GetData da la vuelta sola al final del búfer circular
            return AudioFeatures.ToMono(data, Clip.channels);
        }
    }
}
