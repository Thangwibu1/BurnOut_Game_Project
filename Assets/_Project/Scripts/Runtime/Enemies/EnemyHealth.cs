using System;
using BurnOut.Combat;
using UnityEngine;

namespace BurnOut.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private GameObject deathDropPrefab;
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
            if (CurrentHealth == 0) Die();
        }

        protected virtual void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            Died?.Invoke();
            if (deathDropPrefab != null) Instantiate(deathDropPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject, .08f);
        }
    }
}
