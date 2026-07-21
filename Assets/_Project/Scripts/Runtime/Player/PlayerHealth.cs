using System;
using System.Collections;
using BurnOut.Combat;
using BurnOut.Core;
using UnityEngine;

namespace BurnOut.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(Rigidbody2D))]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable, IHealable
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invincibilityDuration = .7f;
        [SerializeField] private PlayerSanity sanity;
        private PlayerMovement movement;
        private Rigidbody2D body;
        private bool invincible;

        public event Action<int, int> HealthChanged;
        public event Action Died;
        public int CurrentHealth { get; private set; }
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
            if (IsAlive && transform.position.y < -12f) CheckpointManager.Instance?.Respawn(this);
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
            if (CurrentHealth == 0) StartCoroutine(DieRoutine()); else StartCoroutine(InvincibilityRoutine());
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
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
            IsAlive = false;
            Died?.Invoke();
            GameManager.Instance?.ShowGameOver();
            yield return new WaitForSeconds(.65f);
            CheckpointManager.Instance?.Respawn(this);
        }
    }
}
