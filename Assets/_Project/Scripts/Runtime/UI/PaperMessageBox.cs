using TMPro;
using UnityEngine;

namespace BurnOut.UI
{
    // Bottom-of-screen reader box for lore papers. A paper shows its line here while the player stands
    // on it, in the Chiller display font, and clears the moment the player steps off. Tracks which
    // paper owns the current line so leaving one paper never wipes a line another just put up.
    public sealed class PaperMessageBox : MonoBehaviour
    {
        public static PaperMessageBox Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI label;

        private Object currentOwner;

        private void Awake()
        {
            Instance = this;
            // Match the rest of the UI: the Chiller font is created at runtime, so assign it here
            // rather than baking a runtime-only asset reference into the scene.
            if (label != null) { var font = MenuFont.Chiller; if (font != null) label.font = font; }
            if (panel != null) panel.SetActive(false);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        // Shows a line and remembers the paper that requested it.
        public void Show(Object owner, string message)
        {
            currentOwner = owner;
            if (label != null) label.text = message;
            if (panel != null) panel.SetActive(true);
        }

        // Hides only if the given paper is the one currently displayed.
        public void Hide(Object owner)
        {
            if (owner != currentOwner) return;
            currentOwner = null;
            if (panel != null) panel.SetActive(false);
        }
    }
}
