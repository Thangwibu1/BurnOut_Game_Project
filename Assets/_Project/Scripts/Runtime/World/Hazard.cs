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
        private void Awake() => GetComponent<Collider2D>().isTrigger = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

        private void TryDamage(Collider2D other)
        {
            if (Time.time < nextDamageTime || !other.TryGetComponent<PlayerHealth>(out var player)) return;
            nextDamageTime = Time.time + damageInterval;
            player.TakeDamage(new DamageInfo(damage, transform.position, 6f));
        }
    }
}
