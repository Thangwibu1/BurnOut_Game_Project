using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        public enum State { Idle, Patrol, Chase, Attack, Hurt, Dead }
        [SerializeField] private Transform patrolLeft;
        [SerializeField] private Transform patrolRight;
        [SerializeField] private float patrolSpeed = 1.6f;
        [SerializeField] private float chaseSpeed = 2.7f;
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float attackRange = 1.05f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private GameObject energyProjectilePrefab;
        [SerializeField] private float projectileRange = 5.5f;
        // Keep ranged pressure readable: one deliberate shot every three seconds,
        // instead of a stream that makes dodging feel random.
        [SerializeField] private float projectileCooldown = 3f;
        // The shadow enemy is a flat silhouette that reads well with a cool/warm colour wash. Detailed
        // sheet-art monsters look wrong tinted, so they switch this off and keep only speed/size variety.
        [SerializeField] private bool tintVariety = true;
        private Transform player;
        private EnemyHealth health;
        private float direction = 1f;
        private float nextAttackTime;
        private float nextProjectileTime;

        public State CurrentState { get; private set; } = State.Patrol;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            player = FindAnyObjectByType<PlayerHealth>()?.transform;
            health.Died += () => CurrentState = State.Dead;
            ApplySpawnVariety();
        }

        // Light per-spawn variation so repeated encounters never feel like clones:
        // some shadows are small and quick, others larger and slower, with a cool/warm tint.
        private void ApplySpawnVariety()
        {
            float v = Random.value;
            patrolSpeed *= Mathf.Lerp(.85f, 1.25f, v);
            chaseSpeed *= Mathf.Lerp(.9f, 1.4f, v);
            attackCooldown *= Mathf.Lerp(1.15f, .8f, v);
            var scale = transform.localScale;
            float size = Mathf.Lerp(1.15f, .88f, v);
            transform.localScale = new Vector3(Mathf.Abs(scale.x) * size, Mathf.Abs(scale.y) * size, scale.z);
            if (!tintVariety) return;
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = Color.Lerp(new Color(1f, .88f, .88f), new Color(.82f, .86f, 1f), v);
        }

        private void Update()
        {
            if (player == null) player = FindAnyObjectByType<PlayerHealth>()?.transform;
            if (!health.IsAlive || player == null) return;
            var distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange) { CurrentState = State.Attack; TryAttack(); return; }
            if (distance <= projectileRange) { CurrentState = State.Attack; FireEnergy(); MoveTowards(player.position.x, patrolSpeed * .45f); return; }
            if (distance <= detectionRange) { CurrentState = State.Chase; MoveTowards(player.position.x, chaseSpeed); return; }
            CurrentState = State.Patrol;
            Patrol();
        }

        private void Patrol()
        {
            if (patrolLeft == null || patrolRight == null) { transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime); return; }
            if (transform.position.x <= patrolLeft.position.x) direction = 1f;
            if (transform.position.x >= patrolRight.position.x) direction = -1f;
            transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime);
        }

        private void MoveTowards(float targetX, float speed)
        {
            direction = Mathf.Sign(targetX - transform.position.x);
            transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction >= 0f ? 1f : -1f);
            transform.localScale = scale;
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + attackCooldown;
            // A small lunge so the strike reads as a deliberate lunge, not a passive touch.
            direction = Mathf.Sign(player.position.x - transform.position.x);
            transform.Translate(Vector2.right * direction * .35f);
            RuntimeSfx.Play(RuntimeSfx.Sound.Attack, .5f);
            player.GetComponent<PlayerHealth>()?.TakeDamage(new DamageInfo(contactDamage, transform.position, 5f));
        }

        private void FireEnergy()
        {
            if (energyProjectilePrefab == null || Time.time < nextProjectileTime) return;
            nextProjectileTime = Time.time + projectileCooldown;
            direction = Mathf.Sign(player.position.x - transform.position.x);
            var projectile = Instantiate(energyProjectilePrefab, transform.position + new Vector3(direction * .55f, .15f, 0f), Quaternion.identity);
            projectile.GetComponent<EnemyProjectile>()?.FireAt(player.position + Vector3.up * .25f);
        }
    }
}
