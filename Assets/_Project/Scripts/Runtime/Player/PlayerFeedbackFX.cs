using BurnOut.Audio;
using BurnOut.Combat;
using UnityEngine;

namespace BurnOut.Player
{
    /// <summary>Lightweight in-game feedback for jumps, dashes, impacts and skill casts.</summary>
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat), typeof(SpriteRenderer))]
    public sealed class PlayerFeedbackFX : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sourceRenderer;
        private PlayerMovement movement;
        private PlayerCombat combat;
        private float nextAfterimage;

        // Dedicated looping AudioSource for footsteps — separate from the one-shot pool so
        // it starts/stops cleanly without interrupting other sounds.
        private AudioSource footstepSource;

        private void Awake()
        {
            sourceRenderer ??= GetComponent<SpriteRenderer>();
            movement = GetComponent<PlayerMovement>();
            combat = GetComponent<PlayerCombat>();
            BuildFootstepSource();
        }

        private void BuildFootstepSource()
        {
            var clip = RuntimeSfx.LoadClip("SFX/footsteps");
            if (clip == null) return;
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.clip = clip;
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f;
            footstepSource.volume = 0.55f;
        }
        private void OnEnable()  { combat.AttackPerformed += ShowAttack; combat.SkillPerformed += ShowSkill; movement.Jumped += ShowJump; movement.Dashed += ShowDash; }
        private void OnDisable() { combat.AttackPerformed -= ShowAttack; combat.SkillPerformed -= ShowSkill; movement.Jumped -= ShowJump; movement.Dashed -= ShowDash; StopFootsteps(); }

        private void ShowJump() => RuntimeSfx.Play(RuntimeSfx.Sound.Jump, .6f);
        private void ShowDash()
        {
            RuntimeSfx.Play(RuntimeSfx.Sound.Dash, .8f);
            ImpactFX.Expand(transform.position, new Color(.3f, .9f, 1f, .7f), 1.4f);
        }

        private void Update()
        {
            // Footsteps: loop while grounded + moving; stop when airborne, dashing, or still.
            if (footstepSource != null)
            {
                bool shouldStep = movement.IsGrounded && !movement.IsDashing
                                  && Mathf.Abs(movement.HorizontalSpeed) > .15f;
                if (shouldStep  && !footstepSource.isPlaying) footstepSource.Play();
                if (!shouldStep &&  footstepSource.isPlaying) StopFootsteps();
            }

            if (!movement.IsDashing || Time.time < nextAfterimage) return;
            nextAfterimage = Time.time + .045f;
            CreateEcho(transform.position, new Color(.28f, .9f, 1f, .42f), .3f, 1.15f);
        }

        private void StopFootsteps() { if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop(); }

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
            if (target != null) { var c = target.color; c.a *= Mathf.Clamp01(1f - Time.deltaTime / lifetime); target.color = c; transform.localScale = Vector3.Lerp(transform.localScale, Vector3.Scale(transform.localScale, growth), Time.deltaTime * 8f); }
            if (elapsed >= lifetime) Destroy(gameObject);
        }
    }
}
