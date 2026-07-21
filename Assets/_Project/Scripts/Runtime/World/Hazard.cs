using BurnOut.Combat;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Hazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        private void Awake() => GetComponent<Collider2D>().isTrigger = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerHealth>(out var player)) player.TakeDamage(new DamageInfo(damage, transform.position, 4f));
        }
    }
}
