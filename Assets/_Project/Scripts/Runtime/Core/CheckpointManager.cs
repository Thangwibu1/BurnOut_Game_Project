using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Core
{
    public sealed class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }
        public Vector3 RespawnPosition { get; private set; }
        public bool HasCheckpoint { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SetCheckpoint(Vector3 position)
        {
            RespawnPosition = position;
            HasCheckpoint = true;
        }

        public void Respawn(PlayerHealth player)
        {
            if (player == null) return;
            var target = HasCheckpoint ? RespawnPosition : player.InitialSpawnPosition;
            player.RespawnAt(target);
        }
    }
}
