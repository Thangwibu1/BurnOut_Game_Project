using BurnOut.Combat;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Enemies
{
    [RequireComponent(typeof(Projectile))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float hitRadius = .42f;
        private bool fired;
        private PlayerHealth player;

        public void FireAt(Vector3 target)
        {
            fired = true;
            GetComponent<Projectile>().Launch((target - transform.position).normalized);
        }

        private void Update()
        {
            if (!fired) return;
            player ??= FindAnyObjectByType<PlayerHealth>();
            if (player == null || Vector2.Distance(transform.position, player.transform.position) > hitRadius) return;
            player.TakeDamage(new DamageInfo(damage, transform.position, 6f));
            Destroy(gameObject);
        }
    }
}
