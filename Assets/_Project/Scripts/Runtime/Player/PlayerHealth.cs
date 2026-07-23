using System;
using System.Collections;
using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using UnityEngine;

namespace BurnOut.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(Rigidbody2D))]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable, IHealable
    {
        [SerializeField] private int maxHealth = 8;
        [SerializeField] private float invincibilityDuration = 1.05f;
        [SerializeField] private PlayerSanity sanity;
        private PlayerMovement movement;
        private Rigidbody2D body;
        private bool invincible;

        public event Action<int, int> HealthChanged;
        public event Action Died;
        public int CurrentHealth { get; private set; }
        public int MaximumHealth => maxHealth;
        public bool IsAlive { get; private set; } = true;
        public Vector3 InitialSpawnPosition { get; private set; }

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>(); body = GetComponent<Rigidbody2D>();
            sanity ??= GetComponent<PlayerSanity>();
            CurrentHealth = maxHealth;
            InitialSpawnPosition = transform.position;
        }

        private void Update()
        {
            if (IsAlive && transform.position.y < -22f) CheckpointManager.Instance?.Respawn(this);
        }

        public void TakeDamage(DamageInfo damage)
        {
            // Dash is the player's committed dodge: projectiles and traps pass through during its short window.
            if (!IsAlive || invincible || movement.IsDashing || damage.Amount == 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            sanity?.ApplyDamagePenalty(damage.Amount);
            var direction = ((Vector2)transform.position - damage.SourcePosition).normalized;
            body.AddForce(direction * damage.Knockback, ForceMode2D.Impulse);
            StartCoroutine(FlashDamage());
            ImpactFX.Spark(transform.position, new Color(1f, .3f, .38f));
            RuntimeSfx.Play(RuntimeSfx.Sound.Hurt);
            Juice.Shake(.3f, .24f);
            Juice.HitStop(.04f);
            if (CurrentHealth == 0) StartCoroutine(DieRoutine()); else StartCoroutine(InvincibilityRoutine());
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>Grants a temporary invulnerability window — used by the Aura focus skill.</summary>
        public void GrantShield(float seconds)
        {
            if (!IsAlive || seconds <= 0f) return;
            StartCoroutine(ShieldRoutine(seconds));
        }

        private static Sprite bubbleSprite;

        private IEnumerator ShieldRoutine(float seconds)
        {
            invincible = true;
            var bubble = CreateShieldBubble();
            var r = bubble.GetComponent<SpriteRenderer>();
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / seconds;
                // Gentle pulse for the whole duration, then fade out over the final 0.6s.
                float pulse = 1f + Mathf.Sin(elapsed * 9f) * .05f;
                bubble.transform.localScale = Vector3.one * (2.15f * pulse);
                float fade = t > .88f ? Mathf.InverseLerp(1f, .88f, t) : 1f;
                if (r != null) r.color = new Color(.45f, .85f, 1f, .38f * fade + .12f * Mathf.Abs(Mathf.Sin(elapsed * 6f)) * fade);
                yield return null;
            }
            invincible = false;
            if (bubble != null) Destroy(bubble);
        }

        private GameObject CreateShieldBubble()
        {
            if (bubbleSprite == null) bubbleSprite = MakeBubbleSprite();
            var go = new GameObject("~shield");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * 2.15f;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = bubbleSprite;
            r.sortingOrder = 25; // above the player, below the HUD
            r.color = new Color(.45f, .85f, 1f, .4f);
            return go;
        }

        private static Sprite MakeBubbleSprite()
        {
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float c = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                // Bright thin rim near the edge, faint fill inside — a soap-bubble shield.
                float rim = Mathf.Exp(-Mathf.Pow((d - .92f) * 9f, 2f));
                float fill = d < 1f ? .12f : 0f;
                float a = Mathf.Clamp01(rim + fill) * (d <= 1f ? 1f : 0f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }

        public void RestoreFull()
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void RespawnAt(Vector3 position)
        {
            transform.position = position;
            movement.Teleport(position);
            IsAlive = true;
            RestoreFull();
            sanity?.RestoreFull();
            GameManager.Instance?.HideGameOver();
            gameObject.SetActive(true);
        }

        private IEnumerator InvincibilityRoutine()
        {
            invincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            invincible = false;
        }

        private IEnumerator DieRoutine()
        {
            IsAlive = false;   // PlayerVisualAnimator switches to deathFrames immediately
            Died?.Invoke();
            ImpactFX.Burst(transform.position, new Color(1f, .35f, .4f), 16, 6.5f, .4f);
            RuntimeSfx.PlayClip(RuntimeSfx.LoadClip("SFX/player_die"), 1f);
            RuntimeSfx.Play(RuntimeSfx.Sound.GameOver);
            Juice.Shake(.45f, .4f);
            GameManager.Instance?.ShowGameOver();
            // Hold for 2 s so the death animation (and voice clip) plays out fully before respawning.
            yield return new WaitForSeconds(2f);
            CheckpointManager.Instance?.Respawn(this);
        }

        private IEnumerator FlashDamage()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) yield break;
            var original = renderer.color;
            renderer.color = new Color(1f, .25f, .35f, 1f);
            yield return new WaitForSeconds(.12f);
            if (renderer != null) renderer.color = original;
        }
    }
}
