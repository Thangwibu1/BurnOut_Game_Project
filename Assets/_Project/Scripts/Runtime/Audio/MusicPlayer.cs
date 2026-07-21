using UnityEngine;

namespace BurnOut.Audio
{
    /// <summary>
    /// Persistent looping background music. Bootstraps itself before the first scene loads, survives
    /// scene changes (menu → gameplay) via DontDestroyOnLoad, and keeps a single AudioSource so the
    /// track plays continuously without restarting on load. Volume follows the AudioManager music slider.
    /// </summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer Instance { get; private set; }

        private AudioSource source;
        // Matches the MUSIC settings slider's default value so the audible level agrees with the UI on first load.
        private float baseVolume = 0.7f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("~MusicPlayer");
            DontDestroyOnLoad(go);
            go.AddComponent<MusicPlayer>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // The scenes are built in code and their cameras carry no AudioListener, so nothing is
            // audible. Guarantee exactly one listener on this persistent object so music (and SFX)
            // play in every scene. Only added if the scene doesn't already provide one.
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();

            var clip = Resources.Load<AudioClip>("Music/background_music");
            if (clip == null) { Debug.LogWarning("[MusicPlayer] Resources/Music/background_music not found."); return; }

            source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D, non-positional
            source.volume = baseVolume;
            source.Play();
        }

        /// <summary>Sets the music volume (0..1); called by the settings slider through AudioManager.</summary>
        public void SetVolume(float value)
        {
            baseVolume = Mathf.Clamp01(value);
            if (source != null) source.volume = baseVolume;
        }
    }
}
