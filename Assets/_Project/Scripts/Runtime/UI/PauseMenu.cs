using BurnOut.Core;
using BurnOut.Input;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.UI
{
    public sealed class PauseMenu : MonoBehaviour
    {
        private PlayerInputReader input;
        private void Start()
        {
            input = FindAnyObjectByType<PlayerInputReader>();
            if (input != null) input.PausePressed += Toggle;
        }
        private void OnDestroy() { if (input != null) input.PausePressed -= Toggle; }
        public void Toggle() => GameManager.Instance?.TogglePause();
        public void Resume() { if (GameManager.Instance != null && GameManager.Instance.IsPaused) GameManager.Instance.TogglePause(); }
        public void Restart() => GameManager.Instance?.RestartLevel();
        public void MainMenu() => GameManager.Instance?.GoToMainMenu();
    }
}
