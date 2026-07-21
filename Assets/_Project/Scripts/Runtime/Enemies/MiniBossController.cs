using BurnOut.Combat;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class MiniBossController : MonoBehaviour
    {
        [SerializeField] private float dashSpeed = 9f;
        [SerializeField] private float dashCooldown = 3f;
        [SerializeField] private GameObject mentalFragmentPrefab;
        private EnemyHealth health;
        private Transform player;
        private float nextDash;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            player = FindAnyObjectByType<PlayerHealth>()?.transform;
            health.Died += SpawnFragment;
        }

        private void Update()
        {
            if (!health.IsAlive || player == null || Time.time < nextDash) return;
            nextDash = Time.time + dashCooldown;
            var target = player.position;
            transform.position = Vector3.MoveTowards(transform.position, target, dashSpeed * .35f);
            player.GetComponent<PlayerHealth>()?.TakeDamage(new DamageInfo(2, transform.position, 8f));
        }

        private void SpawnFragment()
        {
            if (mentalFragmentPrefab != null) Instantiate(mentalFragmentPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }
}
