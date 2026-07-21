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
            if (other.GetComponent<PlayerHealth>() != null) GameManager.Instance?.CompleteLevel();
        }
    }
}
