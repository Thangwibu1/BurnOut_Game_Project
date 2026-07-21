using UnityEngine;

namespace BurnOut.Player
{
    /// <summary>Lightweight in-game feedback for dashes, pillow impacts and skill casts.</summary>
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat), typeof(SpriteRenderer))]
    public sealed class PlayerFeedbackFX : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sourceRenderer;
        private PlayerMovement movement;
        private PlayerCombat combat;
        private float nextAfterimage;

        private void Awake() { sourceRenderer ??= GetComponent<SpriteRenderer>(); movement = GetComponent<PlayerMovement>(); combat = GetComponent<PlayerCombat>(); }
        private void OnEnable() { combat.AttackPerformed += ShowAttack; combat.SkillPerformed += ShowSkill; }
        private void OnDisable() { combat.AttackPerformed -= ShowAttack; combat.SkillPerformed -= ShowSkill; }
        private void Update()
        {
            if (!movement.IsDashing || Time.time < nextAfterimage) return;
            nextAfterimage = Time.time + .045f;
            CreateEcho(transform.position, new Color(.28f, .9f, 1f, .42f), .3f, 1.15f);
        }

        private void ShowAttack() => CreateEcho(transform.position + Vector3.right * (movement.FacingRight ? .55f : -.55f), new Color(.92f, .82f, 1f, .68f), .18f, 1.3f);
        private void ShowSkill() => CreateEcho(transform.position + Vector3.right * (movement.FacingRight ? .7f : -.7f), new Color(.35f, 1f, .85f, .8f), .35f, 1.8f);

        private void CreateEcho(Vector3 position, Color color, float lifetime, float scale)
        {
            if (sourceRenderer == null || sourceRenderer.sprite == null) return;
            var effect = new GameObject("LilyFeedbackFX");
            effect.transform.position = position;
            effect.transform.localScale = transform.localScale * scale;
            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = sourceRenderer.sprite; renderer.color = color; renderer.sortingOrder = sourceRenderer.sortingOrder + 2;
            effect.AddComponent<SpriteFadeOut>().Configure(renderer, lifetime, Vector3.one * 1.15f);
        }
    }

    public sealed class SpriteFadeOut : MonoBehaviour
    {
        private SpriteRenderer target;
        private float lifetime;
        private Vector3 growth;
        private float elapsed;
        public void Configure(SpriteRenderer renderer, float seconds, Vector3 scaleGrowth) { target = renderer; lifetime = seconds; growth = scaleGrowth; }
        private void Update()
        {
            elapsed += Time.deltaTime;
            if (target != null) { var c = target.color; c.a *= Mathf.Clamp01(1f - Time.deltaTime / lifetime); target.color = c; transform.localScale = Vector3.Lerp(transform.localScale, transform.localScale * growth, Time.deltaTime * 8f); }
            if (elapsed >= lifetime) Destroy(gameObject);
        }
    }
}
