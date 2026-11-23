using System.Collections.Generic;
using UnityEngine;

namespace CrimsonCampus
{
    public class RetroAudioManager : MonoBehaviour
    {
        private AudioSource _source;
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        public bool SoundEnabled = true;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.volume = 0.25f;

            _clips["woo"] = CreateArpeggio("woo", new float[] { 659.25f, 830.61f, 987.77f }, 0.06f, true);
            _clips["pulse"] = CreateChord("pulse", new float[] { 523.25f, 659.25f, 783.99f }, 0.14f);
            _clips["buy"] = CreateArpeggio("buy", new float[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.05f, true);
            _clips["moon"] = CreateChord("moon", new float[] { 440f, 554.37f, 659.25f, 880f }, 0.35f);
            _clips["sacrifice_start"] = CreateSweep("sacrifice_start", 160f, 520f, 0.55f);
            _clips["sacrifice_boom"] = CreateChord("sacrifice_boom", new float[] { 146.83f, 220f, 349.23f, 587.33f }, 0.38f);
            _clips["hunter_alert"] = CreateArpeggio("hunter_alert", new float[] { 440f, 311.13f, 440f, 311.13f }, 0.09f, true);
            _clips["wolf_howl"] = CreateSweep("wolf_howl", 220f, 620f, 0.45f);
            _clips["bat_dash"] = CreateBatDashClip("bat_dash");
            _clips["death"] = CreateSweep("death", 320f, 55f, 0.6f);
        }

        public void Play(string key)
        {
            if (!SoundEnabled || _source == null) return;
            if (_clips.TryGetValue(key, out AudioClip clip))
            {
                _source.PlayOneShot(clip);
            }
        }

        private AudioClip CreateArpeggio(string name, float[] freqs, float noteLen, bool square)
        {
            int sr = 22050;
            int samplesPerNote = Mathf.RoundToInt(sr * noteLen);
            int total = samplesPerNote * freqs.Length;
            float[] data = new float[total];

            for (int n = 0; n < freqs.Length; n++)
            {
                float f = freqs[n];
                for (int i = 0; i < samplesPerNote; i++)
                {
                    float t = i / (float)sr;
                    float env = 1f - (i / (float)samplesPerNote);
                    float wave = Mathf.Sin(2f * Mathf.PI * f * t);
                    if (square) wave = wave >= 0f ? 0.55f : -0.55f;
                    data[n * samplesPerNote + i] = wave * env * 0.4f;
                }
            }

            AudioClip clip = AudioClip.Create(name, total, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChord(string name, float[] freqs, float duration)
        {
            int sr = 22050;
            int total = Mathf.RoundToInt(sr * duration);
            float[] data = new float[total];

            for (int i = 0; i < total; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Pow(1f - (i / (float)total), 1.4f);
                float sum = 0f;
                for (int k = 0; k < freqs.Length; k++)
                    sum += Mathf.Sin(2f * Mathf.PI * freqs[k] * t);
                data[i] = (sum / freqs.Length) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create(name, total, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSweep(string name, float startFreq, float endFreq, float duration)
        {
            int sr = 22050;
            int total = Mathf.RoundToInt(sr * duration);
            float[] data = new float[total];
            float phase = 0f;

            for (int i = 0; i < total; i++)
            {
                float p = i / (float)total;
                float freq = Mathf.Lerp(startFreq, endFreq, p);
                phase += 2f * Mathf.PI * freq / sr;
                float env = Mathf.Sin(p * Mathf.PI);
                float wave = Mathf.Sin(phase) >= 0f ? 0.5f : -0.5f;
                data[i] = wave * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create(name, total, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBatDashClip(string name)
        {
            int sr = 22050;
            float duration = 0.24f;
            int total = Mathf.RoundToInt(sr * duration);
            float[] data = new float[total];
            float phase = 0f;
            System.Random rng = new System.Random(42);

            for (int i = 0; i < total; i++)
            {
                float p = i / (float)total;
                // Cri aigu rapide + battement d'ailes souffle
                float freq = Mathf.Lerp(1150f, 420f, p) + Mathf.Sin(p * 42f) * 90f;
                phase += 2f * Mathf.PI * freq / sr;
                float env = (1f - p) * Mathf.Sin(Mathf.Min(1f, p * 6f) * Mathf.PI * 0.5f);
                float screech = Mathf.Sin(phase) * 0.32f;
                float whoosh = ((float)rng.NextDouble() * 2f - 1f) * 0.22f * (0.5f + 0.5f * Mathf.Sin(p * 30f));
                data[i] = (screech + whoosh) * env;
            }

            AudioClip clip = AudioClip.Create(name, total, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
