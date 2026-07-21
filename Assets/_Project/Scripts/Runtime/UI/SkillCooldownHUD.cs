using BurnOut.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BurnOut.UI
{
    /// <summary>
    /// Bottom-centre skill bar: one pip per skill (Z Shockwave, X Aura, C Rush).
    /// A radial overlay wipes away as the skill comes off cooldown, with a seconds
    /// countdown, so the player can read availability at a glance. Builds itself at runtime.
    /// </summary>
    public sealed class SkillCooldownHUD : MonoBehaviour
    {
        private PlayerCombat combat;
        private readonly Image[] overlays = new Image[3];
        private readonly TextMeshProUGUI[] timers = new TextMeshProUGUI[3];
        private readonly Image[] frames = new Image[3];

        private static readonly string[] Keys = { "Z", "X", "C" };
        private static readonly string[] Names = { "WAVE", "AURA", "RUSH" };
        private static readonly Color[] Tints =
        {
            new(1f, .6f, .3f),   // Shockwave
            new(1f, .9f, .5f),   // Aura
            new(.45f, 1f, .85f)  // Rush
        };

        private void Start()
        {
            combat = FindAnyObjectByType<PlayerCombat>();
            Build();
        }

        private void Build()
        {
            const float size = 76f, gap = 14f;
            float totalWidth = size * 3 + gap * 2;
            for (int i = 0; i < 3; i++)
            {
                float x = -totalWidth / 2f + size / 2f + i * (size + gap);
                var pip = NewRect($"Skill{i}", transform, new Vector2(x, 58f), new Vector2(size, size));
                pip.anchorMin = pip.anchorMax = new Vector2(.5f, 0f); pip.pivot = new Vector2(.5f, 0f);

                var frame = AddImage(pip, new Color(.08f, .07f, .12f, .82f)); frames[i] = frame;
                var icon = NewRect("icon", pip, Vector2.zero, new Vector2(size - 10, size - 10));
                var iconImg = icon.gameObject.AddComponent<Image>(); iconImg.color = Tints[i]; iconImg.sprite = WhiteSprite();

                var overlayRect = NewRect("cd", pip, Vector2.zero, new Vector2(size - 10, size - 10));
                var overlay = overlayRect.gameObject.AddComponent<Image>();
                overlay.sprite = WhiteSprite();
                overlay.color = new Color(0f, 0f, 0f, .72f);
                overlay.type = Image.Type.Filled; overlay.fillMethod = Image.FillMethod.Radial360;
                overlay.fillOrigin = (int)Image.Origin360.Top; overlay.fillClockwise = false;
                overlay.fillAmount = 0f; overlays[i] = overlay;

                AddLabel(pip, Keys[i], 30, new Vector2(0, 4), TextAlignmentOptions.Center, Color.white);
                AddLabel(pip, Names[i], 13, new Vector2(0, -size / 2f + 12f), TextAlignmentOptions.Center, new Color(.85f, .85f, .95f));
                timers[i] = AddLabel(pip, "", 26, Vector2.zero, TextAlignmentOptions.Center, new Color(1f, .95f, .7f));
            }
        }

        private void Update()
        {
            if (combat == null) return;
            Set(0, combat.ShockwaveCooldownRemaining, combat.ShockwaveCooldown);
            Set(1, combat.AuraCooldownRemaining, combat.AuraCooldown);
            Set(2, combat.RushCooldownRemaining, combat.RushCooldown);
        }

        private void Set(int i, float remaining, float full)
        {
            float pct = full <= 0f ? 0f : Mathf.Clamp01(remaining / full);
            overlays[i].fillAmount = pct;
            bool ready = remaining <= .05f;
            timers[i].text = ready ? "" : Mathf.CeilToInt(remaining).ToString();
            // Ready pips glow; cooling pips dim.
            frames[i].color = ready ? new Color(.12f, .11f, .18f, .9f) : new Color(.06f, .05f, .09f, .82f);
        }

        private RectTransform NewRect(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false); rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        private Image AddImage(RectTransform rt, Color color)
        {
            var img = rt.gameObject.AddComponent<Image>(); img.color = color; img.sprite = WhiteSprite(); return img;
        }

        private static Sprite whiteSprite;
        private static Sprite WhiteSprite()
        {
            if (whiteSprite != null) return whiteSprite;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            var px = new Color[16]; for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px); tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(.5f, .5f), 100f);
            return whiteSprite;
        }

        private TextMeshProUGUI AddLabel(Transform parent, string text, float fontSize, Vector2 pos, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject("label", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(120, 40);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text; label.fontSize = fontSize; label.alignment = align; label.color = color; label.fontStyle = FontStyles.Bold;
            return label;
        }
    }
}
