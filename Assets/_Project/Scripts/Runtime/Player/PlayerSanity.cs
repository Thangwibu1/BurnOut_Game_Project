using System;
using System.Collections.Generic;
using BurnOut.Combat;
using BurnOut.Core;
using UnityEngine;

namespace BurnOut.Player
{
    public sealed class PlayerSanity : MonoBehaviour
    {
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float passiveDrainPerSecond = .2f;
        [SerializeField] private float nearbyEnemyDrainPerSecond = 1.1f;
        [SerializeField] private float enemyDetectionRadius = 4.5f;
        [SerializeField] private LayerMask enemyLayer;
        private readonly Collider2D[] nearbyEnemies = new Collider2D[12];
        private ContactFilter2D enemyFilter;
        private bool lowSanity;

        public event Action<float, float> SanityChanged;
        public event Action<bool> LowSanityChanged;
        public float CurrentSanity { get; private set; }
        public float MaximumSanity => maxSanity;
        public bool IsLow => lowSanity;

        private void Awake()
        {
            CurrentSanity = maxSanity;
            enemyFilter = ContactFilter2D.noFilter;
            enemyFilter.SetLayerMask(enemyLayer);
        }

        private void Update()
        {
            var enemyCount = Physics2D.OverlapCircle(transform.position, enemyDetectionRadius, enemyFilter, nearbyEnemies);
            ChangeSanity(-(passiveDrainPerSecond + enemyCount * nearbyEnemyDrainPerSecond) * Time.deltaTime);
            if (CurrentSanity <= 0f) GetComponent<PlayerHealth>()?.TakeDamage(new DamageInfo(999, transform.position, 0f));
        }

        public void Restore(float amount) => ChangeSanity(Mathf.Max(0f, amount));
        public void RestoreFull() { CurrentSanity = maxSanity; Notify(); }
        public void ApplyDamagePenalty(int damage) => ChangeSanity(-damage * 6f);

        private void ChangeSanity(float delta)
        {
            var old = CurrentSanity;
            CurrentSanity = Mathf.Clamp(CurrentSanity + delta, 0f, maxSanity);
            if (!Mathf.Approximately(old, CurrentSanity)) Notify();
        }

        private void Notify()
        {
            SanityChanged?.Invoke(CurrentSanity, maxSanity);
            var nextLow = CurrentSanity / maxSanity <= .3f;
            if (nextLow == lowSanity) return;
            lowSanity = nextLow;
            LowSanityChanged?.Invoke(lowSanity);
        }
    }
}
