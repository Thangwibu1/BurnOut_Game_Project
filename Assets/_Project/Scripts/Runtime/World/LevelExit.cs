using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelExit : MonoBehaviour
    {
        private void Awake() => GetComponent<Collider2D>().isTrigger = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() == null) return;
            // The gate opens for the key dropped by the defeated boss.
            if (other.GetComponent<PlayerInventory>()?.HasKey == true) GameManager.Instance?.CompleteLevel();
        }
    }
}
