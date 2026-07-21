using System;
using System.Collections.Generic;
using UnityEngine;

namespace BurnOut.Audio
{
    /// <summary>
    /// Generates simple procedural sound effects and a looping ambient drone at runtime,
    /// so the prototype is not silent without shipping any audio files.
    /// Everything is synthesised from sine/saw/noise with short envelopes and cached.
    /// Failures are swallowed — worst case a sound just does not play.
    /// </summary>
    public static class RuntimeSfx
    {
        public enum Sound
        {
            Jump, Dash, Attack, Skill, Hit, EnemyDeath, Hurt,
            Pickup, KeyGet, BossTelegraph, BossSlam, Complete, GameOver, DoorOpen, Checkpoint
        }

        private const int SampleRate = 44100;
        private static readonly Dictionary<Sound, AudioClip> cache = new();
        private static AudioClip ambienceClip;
        private static SfxPlayer player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (player != null) return;
            var go = new GameObject("~SfxPlayer") { hideFlags = HideFlags.HideInHierarchy };
            UnityEngine.Object.DontDestroyOnLoad(go);
            player = go.AddComponent<SfxPlayer>();
        }

        public static void Play(Sound sound, float volume = 1f)
        {
            if (player == null) Bootstrap();
            if (player == null) return;
            var clip = Get(sound);
            if (clip != null) player.Play(clip, volume);
        }

        public static AudioClip Ambience()
        {
            if (ambienceClip != null) return ambienceClip;
            try { ambienceClip = BuildDrone(); } catch { ambienceClip = null; }
            return ambienceClip;
        }

        private static AudioClip Get(Sound sound)
        {
            if (cache.TryGetValue(sound, out var c) && c != null) return c;
            AudioClip clip;
            try { clip = Build(sound); } catch { clip = null; }
            cache[sound] = clip;
            return clip;
        }

        // ---- sound design -------------------------------------------------

        private static AudioClip Build(Sound sound)
        {
            switch (sound)
            {
                case Sound.Jump:
                    return FromFunc("Jump", .18f, t => Env(t, .18f, .004f, .10f) * Sine(t, Lerp(360f, 720f, t, .18f)) * .5f);
                case Sound.Dash:
                    return FromFunc("Dash", .22f, t => Env(t, .22f, .005f, .16f) * (Noise() * .35f + Sine(t, Lerp(680f, 150f, t, .22f)) * .6f) * .5f);
                case Sound.Attack:
                    return FromFunc("Attack", .13f, t => Env(t, .13f, .003f, .10f) * (Noise() * (1f - t / .13f)) * .55f);
                case Sound.Skill:
                    return FromFunc("Skill", .34f, t => Env(t, .34f, .006f, .22f) * (Sine(t, Lerp(220f, 540f, t, .34f)) * .5f + Sine(t, 330f) * .3f + Noise() * .15f) * .55f);
                case Sound.Hit:
                    return FromFunc("Hit", .12f, t => Env(t, .12f, .002f, .09f) * (Sine(t, 150f) * .6f + Noise() * .5f) * .6f);
                case Sound.EnemyDeath:
                    return FromFunc("EnemyDeath", .38f, t => Env(t, .38f, .004f, .28f) * (Sine(t, Lerp(320f, 70f, t, .38f)) * .55f + Noise() * .3f) * .55f);
                case Sound.Hurt:
                    return FromFunc("Hurt", .26f, t => Env(t, .26f, .003f, .2f) * (Sine(t, 180f) * .4f + Sine(t, 190f) * .4f + Noise() * .3f) * .55f);
                case Sound.Pickup:
                    return FromFunc("Pickup", .3f, t => Env(t, .3f, .004f, .18f) * (Blip(t, 0f, .12f, 660f) + Blip(t, .1f, .2f, 990f)) * .5f);
                case Sound.KeyGet:
                    return FromFunc("KeyGet", .42f, t => Env(t, .42f, .004f, .28f) * (Blip(t, 0f, .14f, 784f) + Blip(t, .12f, .28f, 1175f) + Blip(t, .24f, .4f, 1568f)) * .45f);
                case Sound.BossTelegraph:
                    return FromFunc("BossTelegraph", .5f, t => Env(t, .5f, .05f, .1f) * (Sine(t, Lerp(90f, 210f, t, .5f)) * .5f + Saw(t, 55f) * .25f) * .5f);
                case Sound.BossSlam:
                    return FromFunc("BossSlam", .55f, t => Env(t, .55f, .002f, .4f) * (Sine(t, Lerp(120f, 42f, t, .55f)) * .7f + Noise() * .45f * (1f - t / .55f)) * .7f);
                case Sound.Complete:
                    return FromFunc("Complete", .9f, t => Env(t, .9f, .02f, .5f) * (Sine(t, 523f) * .3f + Sine(t, 659f) * .28f + Sine(t, 784f) * .26f) * .5f);
                case Sound.GameOver:
                    return FromFunc("GameOver", .8f, t => Env(t, .8f, .01f, .5f) * (Sine(t, Lerp(330f, 110f, t, .8f)) * .45f + Sine(t, Lerp(247f, 82f, t, .8f)) * .35f) * .5f);
                case Sound.DoorOpen:
                    return FromFunc("DoorOpen", .5f, t => Env(t, .5f, .01f, .3f) * (Noise() * .35f * (1f - t / .5f) + Sine(t, Lerp(70f, 130f, t, .5f)) * .4f) * .5f);
                case Sound.Checkpoint:
                    return FromFunc("Checkpoint", .6f, t => Env(t, .6f, .02f, .4f) * (Sine(t, 587f) * .3f + Sine(t, 880f) * .25f + Sine(t, 1174f) * .2f) * .45f);
                default:
                    return null;
            }
        }

        private static AudioClip BuildDrone()
        {
            // 4s buffer with frequencies + LFO chosen as exact multiples of 0.25 Hz so the loop is seamless.
            return FromFunc("Ambience", 4f, t =>
            {
                float lfo = 0.5f + 0.5f * Sine(t, 0.25f);
                float s = Sine(t, 55f) * 0.5f + Sine(t, 82.5f) * 0.3f + Sine(t, 110f) * 0.15f * lfo;
                return s * 0.2f * (0.7f + 0.3f * lfo);
            });
        }

        // ---- synthesis helpers -------------------------------------------

        private static AudioClip FromFunc(string name, float duration, Func<float, float> fn)
        {
            int n = Mathf.Max(1, (int)(duration * SampleRate));
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(fn(i / (float)SampleRate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Sine(float t, float freq) => Mathf.Sin(2f * Mathf.PI * freq * t);
        private static float Saw(float t, float freq) { float p = t * freq; return 2f * (p - Mathf.Floor(p + 0.5f)); }
        private static float Noise() => UnityEngine.Random.value * 2f - 1f;
        private static float Lerp(float a, float b, float t, float dur) => Mathf.Lerp(a, b, Mathf.Clamp01(t / dur));

        private static float Env(float t, float dur, float attack, float release)
        {
            if (t < 0f || t > dur) return 0f;
            float a = attack <= 0f ? 1f : Mathf.Clamp01(t / attack);
            float r = release <= 0f ? 1f : Mathf.Clamp01((dur - t) / release);
            return a * r;
        }

        // A note that begins at 'start' and rings for 'len', used to build arpeggios inside one clip.
        private static float Blip(float t, float start, float len, float freq)
        {
            float local = t - start;
            if (local < 0f || local > len) return 0f;
            return Env(local, len, .004f, len * .6f) * Sine(local, freq);
        }
    }

    /// <summary>Small pooled one-shot player plus a looping ambient bed. Lives for the whole session.</summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        private readonly List<AudioSource> pool = new();
        private int index;
        private AudioSource ambience;

        private void Awake()
        {
            for (int i = 0; i < 6; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                pool.Add(s);
            }

            try
            {
                var drone = RuntimeSfx.Ambience();
                if (drone != null)
                {
                    ambience = gameObject.AddComponent<AudioSource>();
                    ambience.clip = drone;
                    ambience.loop = true;
                    ambience.playOnAwake = false;
                    ambience.spatialBlend = 0f;
                    ambience.volume = 0.32f;
                    ambience.Play();
                }
            }
            catch { /* ambience is optional */ }
        }

        public void Play(AudioClip clip, float volume)
        {
            if (clip == null || pool.Count == 0) return;
            var s = pool[index];
            index = (index + 1) % pool.Count;
            s.pitch = 1f;
            s.PlayOneShot(clip, Mathf.Clamp01(volume) * 0.8f);
        }
    }
}
