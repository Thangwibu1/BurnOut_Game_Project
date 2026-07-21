using System;
using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private GameObject deathDropPrefab;
        [SerializeField] private float deathAnimationDuration = .42f;
        [SerializeField] private float sanityRewardOnKill = 10f;
        public event Action<int, int> HealthChanged;
        public event Action Died;
        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsAlive { get; private set; } = true;

        protected virtual void Awake() => CurrentHealth = maxHealth;

        public virtual void TakeDamage(DamageInfo damage)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            StartCoroutine(FlashHit());
            // Impact feedback: spark, sound and a tiny punch of screen shake + freeze.
            ImpactFX.Spark(transform.position, new Color(1f, .72f, .86f));
            RuntimeSfx.Play(RuntimeSfx.Sound.Hit, .7f);
            Juice.Shake(.12f, .12f);
            Juice.HitStop(.03f);
            if (CurrentHealth == 0) Die();
        }

        protected virtual void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            Died?.Invoke();
            ImpactFX.Burst(transform.position, new Color(1f, .5f, .72f), 12, 6f, .35f);
            ImpactFX.Expand(transform.position, new Color(1f, .6f, .8f, .8f), 1.4f);
            RuntimeSfx.Play(RuntimeSfx.Sound.EnemyDeath);
            Juice.Shake(.2f, .18f);
            // Striking back restores a sliver of sanity — aggression is the psychological reward.
            if (sanityRewardOnKill > 0f) FindAnyObjectByType<PlayerSanity>()?.Restore(sanityRewardOnKill);
            if (deathDropPrefab != null) Instantiate(deathDropPrefab, transform.position, Quaternion.identity);
            foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
            Destroy(gameObject, deathAnimationDuration);
        }

        private System.Collections.IEnumerator FlashHit()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null) yield break;
            var original = renderer.color; renderer.color = new Color(1f, .55f, .8f, 1f);
            yield return new WaitForSeconds(.08f);
            if (renderer != null && IsAlive) renderer.color = original;
        }
    }
}
