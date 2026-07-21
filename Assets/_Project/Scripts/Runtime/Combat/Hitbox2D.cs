using System.Collections.Generic;
using UnityEngine;

namespace BurnOut.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hitbox2D : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float knockback = 6f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private Collider2D hitboxCollider;

        private readonly HashSet<IDamageable> hitTargets = new();
        private bool activeHitbox;

        private void Awake()
        {
            hitboxCollider ??= GetComponent<Collider2D>();
            if (hitboxCollider != null)
            {
                hitboxCollider.isTrigger = true;
                hitboxCollider.enabled = false;
            }
        }

        public void BeginHit()
        {
            hitTargets.Clear();
            activeHitbox = true;
            if (hitboxCollider != null) hitboxCollider.enabled = true;
        }

        public void EndHit()
        {
            activeHitbox = false;
            if (hitboxCollider != null) hitboxCollider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!activeHitbox || (targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !hitTargets.Add(damageable)) return;
            damageable.TakeDamage(new DamageInfo(damage, transform.position, knockback));
        }
    }
}
