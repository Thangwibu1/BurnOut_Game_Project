using System;
using BurnOut.Combat;
using UnityEngine;

namespace BurnOut.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private GameObject deathDropPrefab;
        [SerializeField] private float deathAnimationDuration = .42f;
        public event Action<int, int> HealthChanged;
        public event Action Died;
        public int CurrentHealth { get; private set; }
        public bool IsAlive { get; private set; } = true;

        protected virtual void Awake() => CurrentHealth = maxHealth;

        public virtual void TakeDamage(DamageInfo damage)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage.Amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            StartCoroutine(FlashHit());
            if (CurrentHealth == 0) Die();
        }

        protected virtual void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            Died?.Invoke();
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
