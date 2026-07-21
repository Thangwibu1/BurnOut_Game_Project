using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    /// <summary>Introduces enemies only when Lily reaches a designed encounter space.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class EncounterSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Vector3[] spawnOffsets;
        [SerializeField] private bool triggerOnce = true;
        private bool triggered;

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((triggerOnce && triggered) || !other.TryGetComponent<PlayerHealth>(out _)) return;
            triggered = true;
            if (enemyPrefab == null) return;
            foreach (var offset in spawnOffsets) Instantiate(enemyPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}
