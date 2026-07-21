using BurnOut.Player;
using BurnOut.Enemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BurnOut.World
{
    /// <summary>Introduces enemies only when Lily reaches a designed encounter space.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class EncounterSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject rewardPrefab;
        [SerializeField] private Vector3[] spawnOffsets;
        [SerializeField] private int waveCount = 1;
        [SerializeField] private float waveDelay = 1.25f;
        [SerializeField] private bool triggerOnce = true;
        private bool triggered;
        private bool rewardGiven;
        private bool waitingForWave;
        private int completedWaves;
        private readonly List<EnemyHealth> activeEnemies = new();

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((triggerOnce && triggered) || !other.TryGetComponent<PlayerHealth>(out _)) return;
            triggered = true;
            SpawnWave();
        }

        private void Update()
        {
            if (!triggered || waitingForWave) return;
            activeEnemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
            if (activeEnemies.Count > 0) return;
            if (completedWaves < waveCount) StartCoroutine(SpawnNextWave());
            else if (!rewardGiven) { rewardGiven = true; if (rewardPrefab != null) Instantiate(rewardPrefab, transform.position + Vector3.up * .7f, Quaternion.identity); }
        }

        private IEnumerator SpawnNextWave()
        {
            waitingForWave = true;
            yield return new WaitForSeconds(waveDelay);
            SpawnWave();
            waitingForWave = false;
        }

        private void SpawnWave()
        {
            if (enemyPrefab == null) return;
            completedWaves++;
            foreach (var offset in spawnOffsets)
            {
                var enemy = Instantiate(enemyPrefab, transform.position + offset, Quaternion.identity);
                if (enemy.TryGetComponent<EnemyHealth>(out var health)) activeEnemies.Add(health);
            }
        }
    }
}
