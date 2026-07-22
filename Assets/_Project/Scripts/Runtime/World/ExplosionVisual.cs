using UnityEngine;

namespace BurnOut.World
{
    /// <summary>
    /// Plays a one-shot explosion sprite animation then destroys itself. Cosmetic only — the actual
    /// blast damage lives in ExplodeOnDeath, so this can be missing or mis-sliced without breaking the fight.
    /// </summary>
    public sealed class ExplosionVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameRate = 16f;
        [SerializeField] private int sortingOrder = 42;

        private int frameIndex;
        private float timer;

        private void Awake()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sortingOrder = sortingOrder;
            if (frames != null && frames.Length > 0 && spriteRenderer != null) spriteRenderer.sprite = frames[0];
            else if (frames == null || frames.Length == 0) Destroy(gameObject); // nothing to play
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            timer += Time.deltaTime;
            if (timer < 1f / frameRate) return;
            timer = 0f;
            frameIndex++;
            if (frameIndex >= frames.Length) { Destroy(gameObject); return; }
            if (spriteRenderer != null) spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
