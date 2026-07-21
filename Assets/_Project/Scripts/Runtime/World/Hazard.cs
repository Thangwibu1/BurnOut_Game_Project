using BurnOut.Combat;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float damageInterval = .7f;
        private float nextDamageTime;
        private Collider2D hazardCollider;
        private PlayerHealth player;

        private void Awake()
        {
            hazardCollider = GetComponent<Collider2D>();
            hazardCollider.isTrigger = true;
        }

        private void Update()
        {
            player ??= FindAnyObjectByType<PlayerHealth>();
            // Fallback damage check: project layer settings can never silence spike damage.
            if (player != null && hazardCollider != null && hazardCollider.bounds.Contains(player.transform.position)) TryDamage(player);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

        private void TryDamage(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerHealth>(out var hitPlayer)) return;
            TryDamage(hitPlayer);
        }

        private void TryDamage(PlayerHealth hitPlayer)
        {
            if (Time.time < nextDamageTime) return;
            nextDamageTime = Time.time + damageInterval;
            hitPlayer.TakeDamage(new DamageInfo(damage, transform.position, 6f));
        }
    }
}
