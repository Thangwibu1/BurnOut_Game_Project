using UnityEngine;

namespace BurnOut.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = .7f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = .8f;
        [SerializeField] private AudioSource musicSource;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ApplyVolumes();
        }

        public void SetMaster(float value) { masterVolume = value; ApplyVolumes(); }
        public void SetMusic(float value) { musicVolume = value; ApplyVolumes(); }
        public void SetSfx(float value) => sfxVolume = value;
        public void PlaySfx(AudioClip clip, Vector3 position)
        {
            if (clip != null) AudioSource.PlayClipAtPoint(clip, position, masterVolume * sfxVolume);
        }
        private void ApplyVolumes() { AudioListener.volume = masterVolume; if (musicSource != null) musicSource.volume = musicVolume; }
    }
}
