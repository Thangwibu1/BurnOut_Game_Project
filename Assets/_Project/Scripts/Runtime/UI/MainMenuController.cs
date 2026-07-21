using BurnOut.Core;
using UnityEngine;

namespace BurnOut.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;
        public void StartGame() => SceneLoader.LoadLevel01();
        public void OpenSettings() { if (settingsPanel != null) settingsPanel.SetActive(true); }
        public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }
        public void ExitGame() => Application.Quit();
    }
}
