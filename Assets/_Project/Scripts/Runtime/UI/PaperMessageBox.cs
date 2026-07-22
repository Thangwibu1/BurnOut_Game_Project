using System.Collections;
using TMPro;
using UnityEngine;

namespace BurnOut.UI
{
    // Bottom-of-screen reader box for lore papers. A paper the player touches pushes its line here,
    // shown in the Chiller display font. The box holds for a few seconds then hides so it never
    // blocks the view; touching a paper again brings it straight back.
    public sealed class PaperMessageBox : MonoBehaviour
    {
        public static PaperMessageBox Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float holdSeconds = 6f;

        private Coroutine hideRoutine;

        private void Awake()
        {
            Instance = this;
            // Match the rest of the UI: the Chiller font is created at runtime, so assign it here
            // rather than baking a runtime-only asset reference into the scene.
            if (label != null) { var font = MenuFont.Chiller; if (font != null) label.font = font; }
            if (panel != null) panel.SetActive(false);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Show(string message)
        {
            if (label != null) label.text = message;
            if (panel != null) panel.SetActive(true);
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            // Unscaled so the line stays readable even if the game is paused mid-read.
            yield return new WaitForSecondsRealtime(holdSeconds);
            if (panel != null) panel.SetActive(false);
            hideRoutine = null;
        }
    }
}
