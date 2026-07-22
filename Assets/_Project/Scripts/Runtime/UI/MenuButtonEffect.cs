using BurnOut.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BurnOut.UI
{
    // Hover-scale + press-punch feedback for the main menu's Start/Settings/Exit hit targets.
    // Scales the hit target (and its child label) on hover, punches down on press,
    // and tints the label from a resting tan to a bright gold while hovered.
    public sealed class MenuButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float hoverScale = 1.12f;
        [SerializeField] private float pressScale = .95f;
        [SerializeField] private float lerpSpeed = 14f;
        [SerializeField] private Color normalColor = new(.86f, .74f, .5f);
        [SerializeField] private Color hoverColor = new(1f, .82f, .3f);

        private RectTransform target;
        private float targetScale = 1f;
        private bool hovering;

        private void Awake()
        {
            target = (RectTransform)transform;
            if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();
            ApplyColor(normalColor);
        }

        private void Update()
        {
            // Unscaled so it still animates even when the game is paused (Time.timeScale == 0).
            target.localScale = Vector3.Lerp(target.localScale, Vector3.one * targetScale, Time.unscaledDeltaTime * lerpSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            targetScale = hoverScale;
            ApplyColor(hoverColor);
            RuntimeSfx.PlayUiHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            targetScale = 1f;
            ApplyColor(normalColor);
        }

        public void OnPointerDown(PointerEventData eventData) { targetScale = pressScale; RuntimeSfx.PlayUiClick(); }
        public void OnPointerUp(PointerEventData eventData) => targetScale = hovering ? hoverScale : 1f;

        private void ApplyColor(Color color) { if (label != null) label.color = color; }
    }
}
