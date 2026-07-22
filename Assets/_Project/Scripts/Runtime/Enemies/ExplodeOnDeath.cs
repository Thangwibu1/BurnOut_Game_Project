using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Enemies
{
    /// <summary>
    /// Map-2 monster behaviour: on death it detonates. Any player caught inside the blast radius
    /// takes a heavy fixed hit (half the player's max HP). The damage routes through PlayerHealth,
    /// so a dashing or shielded player passes through unharmed — the blast is dodgeable.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class ExplodeOnDeath : MonoBehaviour
    {
        [SerializeField] private int explosionDamage = 4;   // 50% of the player's 8 max HP
        [SerializeField] private float explosionRadius = 2.4f;
        [SerializeField] private float knockback = 10f;
        [SerializeField] private Color blastColor = new(1f, .55f, .2f);
        [SerializeField] private GameObject explosionVisualPrefab;

        private EnemyHealth health;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            health.Died += Detonate;
        }

        private void OnDestroy() { if (health != null) health.Died -= Detonate; }

        private void Detonate()
        {
            var origin = transform.position;

            // Damage: hit the player if they're within the blast. Find by component so it works
            // regardless of collider layers, and let PlayerHealth honour i-frames (dash/shield).
            var player = FindAnyObjectByType<PlayerHealth>();
            if (player != null && Vector2.Distance(player.transform.position, origin) <= explosionRadius)
                player.TakeDamage(new DamageInfo(explosionDamage, origin, knockback));

            // Feedback that needs no art asset.
            ImpactFX.Expand(origin, new Color(blastColor.r, blastColor.g, blastColor.b, .85f), explosionRadius * 1.6f);
            ImpactFX.Burst(origin, blastColor, 20, 8f, .45f);
            RuntimeSfx.Play(RuntimeSfx.Sound.BossSlam);
            Juice.Shake(.4f, .3f);

            // Optional authored explosion animation.
            if (explosionVisualPrefab != null) Instantiate(explosionVisualPrefab, origin, Quaternion.identity);
        }
    }
}
