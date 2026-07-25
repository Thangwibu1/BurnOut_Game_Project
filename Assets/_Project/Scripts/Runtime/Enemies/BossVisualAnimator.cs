using UnityEngine;

namespace BurnOut.Enemies
{
    /// <summary>
    /// Plays the shadow's authored frames for the mini-boss according to its live fight state.
    /// Separate from <see cref="EnemyVisualAnimator"/> because the boss is driven by
    /// <see cref="MiniBossController"/> rather than an <see cref="EnemyBrain"/>.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(MiniBossController))]
    public sealed class BossVisualAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] moveFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private float frameRate = 9f;

        private MiniBossController boss;
        private Sprite[] activeFrames;
        private int frameIndex;
        private float timer;

        private void Awake()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            boss = GetComponent<MiniBossController>();
        }

        private void Update()
        {
            if (boss == null || spriteRenderer == null) return;
            Sprite[] frames;
            switch (boss.State)
            {
                case MiniBossController.BossState.Dead: frames = deathFrames; break;
                case MiniBossController.BossState.Telegraph:
                case MiniBossController.BossState.Slam:
                case MiniBossController.BossState.Shoot: frames = attackFrames; break;
                case MiniBossController.BossState.Chase: frames = moveFrames; break;
                default: frames = idleFrames; break;
            }

            if (frames == null || frames.Length == 0) return;
            if (activeFrames != frames) { activeFrames = frames; frameIndex = 0; timer = 0f; spriteRenderer.sprite = frames[0]; }
            timer += Time.deltaTime;
            if (timer < 1f / frameRate) return;
            timer = 0f;
            // Death plays once then freezes on the last frame; other states loop.
            if (boss.State == MiniBossController.BossState.Dead)
            {
                if (frameIndex < frames.Length - 1) { frameIndex++; spriteRenderer.sprite = frames[frameIndex]; }
            }
            else
            {
                frameIndex = (frameIndex + 1) % frames.Length;
                spriteRenderer.sprite = frames[frameIndex];
            }
        }
    }
}
