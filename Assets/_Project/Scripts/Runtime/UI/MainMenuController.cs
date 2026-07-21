using BurnOut.Core;
using TMPro;
using UnityEngine;

namespace BurnOut.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;

        // The new menu artwork (burn_out_menu.png) no longer bakes the START / SETTINGS / EXIT
        // words into the background, so we paint them onto the existing transparent hit targets
        // at runtime and give each one hover/click feedback. Runs once on load.
        private void Start() => DecorateMenuButtons();

        private void DecorateMenuButtons()
        {
            var menuFont = MenuFont.Chiller;
            DecorateButton("StartHitTarget", "START", menuFont);
            DecorateButton("SettingsHitTarget", "SETTINGS", menuFont);
            DecorateButton("ExitHitTarget", "EXIT", menuFont);
        }

        private void DecorateButton(string hitTargetName, string caption, TMP_FontAsset font)
        {
            var hit = transform.Find(hitTargetName);
            if (hit == null) return;

            // Only build the label once (in case Start runs again after a scene reload).
            var label = hit.GetComponentInChildren<TextMeshProUGUI>();
            if (label == null)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(hit, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                // Small left padding so the caption sits just inside the hit target, not jammed to its edge.
                rect.offsetMin = new Vector2(40f, 0f); rect.offsetMax = Vector2.zero;
                label = go.GetComponent<TextMeshProUGUI>();
                label.text = caption;
                if (font != null) label.font = font;
                // Chiller is a display face — bump the size and loosen tracking so it reads big and eerie.
                label.fontSize = 64;
                label.characterSpacing = 6f;
                label.alignment = TextAlignmentOptions.Left;
                label.raycastTarget = false;
                label.color = new Color(.86f, .74f, .5f);
            }

            if (hit.GetComponent<MenuButtonEffect>() == null)
                hit.gameObject.AddComponent<MenuButtonEffect>();
        }

        public void StartGame() => SceneLoader.LoadLevel01();
        public void OpenSettings() { if (settingsPanel != null) settingsPanel.SetActive(true); }
        public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }
        public void ExitGame() => Application.Quit();
    }
}
