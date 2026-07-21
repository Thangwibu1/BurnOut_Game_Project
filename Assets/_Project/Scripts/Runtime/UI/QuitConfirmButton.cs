using BurnOut.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BurnOut.UI
{
    // Adds a power-icon quit button to the pause panel. Clicking it opens a YES/NO confirmation;
    // confirming returns to the main menu. Built entirely at runtime and styled with the Chiller
    // menu font so it matches the rest of the UI without needing a scene rebuild.
    public sealed class QuitConfirmButton : MonoBehaviour
    {
        private GameObject dialog;

        private void Start()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            // Live on the always-visible canvas (top-right corner) so the quit button is reachable
            // at any time during play, not hidden inside the pause panel.
            BuildPowerButton(canvas.transform);
            BuildDialog(canvas.transform);
        }

        private void BuildPowerButton(Transform parent)
        {
            var go = new GameObject("QuitPowerButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            // Pin to the top-right corner of the screen.
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(58f, 58f);
            go.GetComponent<Image>().sprite = PowerSprite();
            go.GetComponent<Button>().onClick.AddListener(ShowDialog);
        }

        // Freeze the game while the confirmation is up so the world doesn't keep moving behind it.
        // Remember the prior timeScale so closing with NO restores it (handles the case where the
        // player was already paused via ESC — we must not silently un-pause them).
        private float previousTimeScale = 1f;
        private void ShowDialog()
        {
            if (dialog == null) return;
            dialog.SetActive(true);
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        private void HideDialog()
        {
            if (dialog == null) return;
            dialog.SetActive(false);
            Time.timeScale = previousTimeScale;
        }

        private void BuildDialog(Transform canvas)
        {
            // Full-screen dim so the confirmation reads as a modal over the paused game.
            dialog = new GameObject("QuitConfirmDialog", typeof(RectTransform), typeof(Image));
            var dimRect = (RectTransform)dialog.transform;
            dimRect.SetParent(canvas, false);
            dimRect.anchorMin = Vector2.zero; dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero; dimRect.offsetMax = Vector2.zero;
            dialog.GetComponent<Image>().color = new Color(0f, 0f, 0f, .72f);

            var box = NewRect("Box", dimRect, Vector2.zero, new Vector2(520f, 260f));
            box.gameObject.AddComponent<Image>().color = new Color(.1f, .07f, .12f, .96f);

            AddLabel(box, "QUIT TO MAIN MENU?", 40, new Vector2(0f, 62f), new Vector2(480f, 70f));
            var yes = AddButton(box, "YES", new Vector2(-115f, -55f));
            yes.onClick.AddListener(QuitToMenu);
            var no = AddButton(box, "NO", new Vector2(115f, -55f));
            no.onClick.AddListener(HideDialog);

            dialog.SetActive(false);
        }

        private void QuitToMenu()
        {
            HideDialog();
            GameManager.Instance?.GoToMainMenu();
        }

        private RectTransform NewRect(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false); rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        private void AddLabel(Transform parent, string text, float fontSize, Vector2 pos, Vector2 size)
        {
            var rt = NewRect("Label", parent, pos, size);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            var font = MenuFont.Chiller;
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.86f, .74f, .5f);
        }

        private Button AddButton(Transform parent, string caption, Vector2 pos)
        {
            var go = new GameObject($"{caption}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(190f, 70f);
            go.GetComponent<Image>().color = new Color(.22f, .13f, .35f, 1f);
            AddLabel(rt, caption, 46, Vector2.zero, new Vector2(190f, 70f));
            return go.GetComponent<Button>();
        }

        // Draws a classic power symbol (ring with a top gap + vertical bar) into a small texture,
        // so the button needs no imported art asset.
        private static Sprite powerSprite;
        private static Sprite PowerSprite()
        {
            if (powerSprite != null) return powerSprite;
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { hideFlags = HideFlags.DontUnloadUnusedAsset };
            var clear = new Color(0f, 0f, 0f, 0f);
            var glyph = new Color(.9f, .82f, .5f, 1f);
            float cx = (n - 1) / 2f, cy = (n - 1) / 2f;
            float outer = 22f, inner = 16f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg; // 0 at top, widening symmetrically
                    bool onRing = dist <= outer && dist >= inner && Mathf.Abs(ang) > 34f;
                    bool onBar = Mathf.Abs(dx) <= 3f && dy >= 0f && dy <= 26f;
                    tex.SetPixel(x, y, onRing || onBar ? glyph : clear);
                }
            tex.Apply();
            powerSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100f);
            return powerSprite;
        }
    }
}
