using BurnOut.Enemies;
using BurnOut.Player;
using BurnOut.UI;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BossArenaTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject arenaGate;
        [SerializeField] private MiniBossController boss;
        [SerializeField] private BossHUD bossHud;
        private bool triggered;
        private void Awake() => GetComponent<Collider2D>().isTrigger = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || other.GetComponent<PlayerHealth>() == null) return;
            triggered = true;
            if (arenaGate != null) arenaGate.SetActive(true);
            if (boss != null) boss.gameObject.SetActive(true);
            bossHud?.Show(boss != null ? boss.GetComponent<EnemyHealth>() : null);
        }
    }
}
