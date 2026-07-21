using UnityEngine;

namespace BurnOut.Player
{
    /// <summary>Plays Lily's authored sprite frames according to her live movement and combat state.</summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement), typeof(PlayerCombat), typeof(PlayerSanity), typeof(PlayerHealth))]
    public sealed class PlayerVisualAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] runFrames;
        [SerializeField] private Sprite[] jumpFrames;
        [SerializeField] private Sprite[] lowSanityFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] skillFrames;
        [SerializeField] private Sprite[] dashFrames;
        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private float frameRate = 11f;

        private PlayerMovement movement;
        private PlayerCombat combat;
        private PlayerSanity sanity;
        private PlayerHealth health;
        private Sprite[] activeFrames;
        private int frameIndex;
        private float frameTimer;

        private void Awake()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            movement = GetComponent<PlayerMovement>(); combat = GetComponent<PlayerCombat>();
            sanity = GetComponent<PlayerSanity>(); health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            var frames = !health.IsAlive ? deathFrames
                : movement.IsDashing ? dashFrames
                : combat.IsUsingSkill ? skillFrames
                : combat.IsAttacking ? attackFrames
                : !movement.IsGrounded ? jumpFrames
                : sanity.IsLow ? lowSanityFrames
                : Mathf.Abs(movement.HorizontalSpeed) > .15f ? runFrames : idleFrames;
            Play(frames);
        }

        private void Play(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0 || spriteRenderer == null) return;
            if (activeFrames != frames) { activeFrames = frames; frameIndex = 0; frameTimer = 0f; spriteRenderer.sprite = frames[0]; }
            frameTimer += Time.deltaTime;
            if (frameTimer < 1f / frameRate) return;
            frameTimer = 0f;
            frameIndex = (frameIndex + 1) % frames.Length;
            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
