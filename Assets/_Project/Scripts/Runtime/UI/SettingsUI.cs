using BurnOut.Audio;
using UnityEngine;

namespace BurnOut.UI
{
    public sealed class SettingsUI : MonoBehaviour
    {
        public void SetMasterVolume(float value) => AudioManager.Instance?.SetMaster(value);
        public void SetMusicVolume(float value) => AudioManager.Instance?.SetMusic(value);
        public void SetSfxVolume(float value) => AudioManager.Instance?.SetSfx(value);
    }
}
