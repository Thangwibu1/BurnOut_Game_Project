using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Color activeColor = new(0.35f, 1f, .85f);
        private bool activated;

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.TryGetComponent<PlayerHealth>(out var player)) return;
            activated = true;
            CheckpointManager.Instance?.SetCheckpoint(transform.position + Vector3.up);
            player.RestoreFull();
            player.GetComponent<PlayerSanity>()?.RestoreFull();
            if (visual != null) visual.color = activeColor;
        }
    }
}
