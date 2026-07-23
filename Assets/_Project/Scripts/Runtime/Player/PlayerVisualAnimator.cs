using BurnOut.Audio;
using UnityEngine;

namespace BurnOut.Player
{
    /// <summary>Plays Lily's authored sprite frames according to her live movement and combat state.</summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement), typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerSanity), typeof(PlayerHealth))]
    public sealed class PlayerVisualAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] runFrames;
        [SerializeField] private Sprite[] jumpFrames;
        [SerializeField] private Sprite[] lowSanityFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] auraFrames;
        [SerializeField] private Sprite[] rushFrames;
        [SerializeField] private Sprite[] shockwaveFrames;
        [SerializeField] private Sprite[] dashFrames;
        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private Sprite[] yawnFrames;
        [SerializeField] private float frameRate = 11f;
        [Header("Idle yawn")]
        [Tooltip("Seconds of standing perfectly still before Lily yawns.")]
        [SerializeField] private float yawnAfterIdle = 5f;
        [Tooltip("Seconds per yawn animation frame (slower than normal so the yawn reads clearly).")]
        [SerializeField] private float yawnFrameDuration = .28f;
        [Tooltip("Resources path (no extension) of a horizontal yawn sheet, auto-sliced if yawnFrames is empty.")]
        [SerializeField] private string yawnSheetResource = "Sprites/Player_Yawn";
        [SerializeField] private int yawnSheetColumns = 5;

        private PlayerMovement movement;
        private PlayerCombat combat;
        private PlayerSanity sanity;
        private PlayerHealth health;
        private Sprite[] activeFrames;
        private int frameIndex;
        private float frameTimer;
        private float idleTime;
        private bool yawning;    // true = played yawn anim, holding on last frame until player moves
        private AudioSource yawnAudioSource;

        private void Awake()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            movement = GetComponent<PlayerMovement>(); combat = GetComponent<PlayerCombat>();
            sanity = GetComponent<PlayerSanity>(); health = GetComponent<PlayerHealth>();

            // yawnFrames is identical to lowSanityFrames (same sprite sheet).
            // Unity periodically re-serializes the prefab and clears yawnFrames, so we rebuild it
            // at runtime: first try the Resources sheet, then fall back to lowSanityFrames which is
            // always wired in the prefab — so the yawn animation is never silently missing.
            if (yawnFrames == null || yawnFrames.Length == 0)
            {
                if (!string.IsNullOrEmpty(yawnSheetResource))
                    yawnFrames = SliceSheet(yawnSheetResource, yawnSheetColumns);
            }
            if (yawnFrames == null || yawnFrames.Length == 0)
                yawnFrames = lowSanityFrames;
        }

        private void Update()
        {
            bool restingIdle = health.IsAlive && movement.IsGrounded && !movement.IsDashing
                && !combat.IsUsingSkill && !combat.IsAttacking && !sanity.IsLow
                && Mathf.Abs(movement.HorizontalSpeed) <= .15f;

            if (!restingIdle)
            {
                // Any action cancels the yawn — stop audio immediately.
                if (yawning) CancelYawn();
                idleTime = 0f;
            }
            else if (!yawning)
            {
                idleTime += Time.deltaTime;
                if (idleTime >= yawnAfterIdle && yawnFrames != null && yawnFrames.Length > 0)
                    StartYawn();
            }
            // yawning && restingIdle → stay on last frame, do nothing — player controls when it ends.

            // Fallback to idleFrames if yawnFrames is somehow null so the sprite never disappears.
            var hasYawn = yawnFrames != null && yawnFrames.Length > 0;
            var frames = !health.IsAlive ? deathFrames
                : movement.IsDashing ? dashFrames
                : combat.IsUsingSkill ? SkillFrames(combat.ActiveSkill)
                : combat.IsAttacking ? attackFrames
                : !movement.IsGrounded ? jumpFrames
                : sanity.IsLow ? lowSanityFrames
                : yawning ? (hasYawn ? yawnFrames : idleFrames)
                : Mathf.Abs(movement.HorizontalSpeed) > .15f ? runFrames : idleFrames;
            Play(frames);
        }

        private void StartYawn()
        {
            yawning = true;
            idleTime = 0f;
            // Loop the yawn audio continuously until the player moves — stopped in CancelYawn().
            var clip = RuntimeSfx.LoadClip("SFX/player_yawn");
            if (clip != null)
            {
                if (yawnAudioSource == null)
                {
                    yawnAudioSource = gameObject.AddComponent<AudioSource>();
                    yawnAudioSource.spatialBlend = 0f;
                    yawnAudioSource.playOnAwake = false;
                    yawnAudioSource.volume = .9f;
                    yawnAudioSource.loop = true;
                }
                yawnAudioSource.clip = clip;
                if (!yawnAudioSource.isPlaying) yawnAudioSource.Play();
            }
        }

        private void CancelYawn()
        {
            yawning = false;
            idleTime = 0f;
            if (yawnAudioSource != null && yawnAudioSource.isPlaying) yawnAudioSource.Stop();
        }

        private Sprite[] SkillFrames(PlayerCombat.SkillId skill)
        {
            return skill switch
            {
                // Aura casts with the quick attack swing (like Shift), not a separate long pose.
                PlayerCombat.SkillId.Aura => attackFrames,
                PlayerCombat.SkillId.Shockwave => shockwaveFrames != null && shockwaveFrames.Length > 0 ? shockwaveFrames : attackFrames,
                _ => rushFrames != null && rushFrames.Length > 0 ? rushFrames : attackFrames
            };
        }

        private void Play(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0 || spriteRenderer == null) return;
            if (activeFrames != frames) { activeFrames = frames; frameIndex = 0; frameTimer = 0f; spriteRenderer.sprite = frames[0]; }

            bool isYawnAnim = yawning && frames == yawnFrames;
            float interval = isYawnAnim ? yawnFrameDuration : 1f / frameRate;
            frameTimer += Time.deltaTime;
            if (frameTimer < interval) return;
            frameTimer = 0f;

            if (isYawnAnim)
            {
                // Advance until the last frame then freeze there — player must move to break out.
                if (frameIndex < frames.Length - 1)
                {
                    frameIndex++;
                    spriteRenderer.sprite = frames[frameIndex];
                }
                // already on last frame → do nothing, sprite stays frozen
            }
            else
            {
                frameIndex = (frameIndex + 1) % frames.Length;
                spriteRenderer.sprite = frames[frameIndex];
            }
        }

        // Slices a horizontal sprite sheet loaded from Resources into evenly-sized frames at runtime,
        // so a yawn sheet can be dropped into Resources without hand-configuring sprites in the editor.
        private static Sprite[] SliceSheet(string resourcePath, int columns)
        {
            if (columns < 1) columns = 1;
            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null) { Debug.LogWarning($"[PlayerVisualAnimator] Yawn sheet not found at Resources/{resourcePath}."); return null; }
            int fw = tex.width / columns, fh = tex.height;
            var frames = new Sprite[columns];
            for (int i = 0; i < columns; i++)
                frames[i] = Sprite.Create(tex, new Rect(i * fw, 0, fw, fh), new Vector2(.5f, .5f), 100f);
            return frames;
        }
    }
}
