using BurnOut.Audio;
using BurnOut.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BurnOut.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject levelCompletePanel;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            // Damage between Player and Enemy is distance-based (EnemyBrain/MiniBossController),
            // so the physical push from their solid colliders is unwanted friction, not gameplay.
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
            SetPanel(pausePanel, false);
            SetPanel(gameOverPanel, false);
            SetPanel(levelCompletePanel, false);
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            SetPanel(pausePanel, IsPaused);
        }

        public void ShowGameOver()
        {
            SetPanel(gameOverPanel, true);
        }

        public void HideGameOver() => SetPanel(gameOverPanel, false);

        public void CompleteLevel()
        {
            RuntimeSfx.Play(RuntimeSfx.Sound.Complete);
            Time.timeScale = 0f;
            SetPanel(levelCompletePanel, true);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("SC_MainMenu");
        }

        private static void SetPanel(GameObject panel, bool visible)
        {
            if (panel != null) panel.SetActive(visible);
        }
    }
}
