using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Color activeColor = Color.white;
        private bool activated;

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.TryGetComponent<PlayerHealth>(out var player)) return;
            activated = true;
            CheckpointManager.Instance?.SetCheckpoint(transform.position + Vector3.up);
            player.RestoreFull();
            player.GetComponent<PlayerSanity>()?.RestoreFull();
            if (visual != null)
            {
                if (activeSprite != null) visual.sprite = activeSprite;
                visual.color = activeColor;
            }
            RuntimeSfx.Play(RuntimeSfx.Sound.Checkpoint);
            ImpactFX.Expand(transform.position, new Color(.4f, 1f, .85f), 2.6f);
            Juice.Shake(.1f, .16f);
        }
    }
}
