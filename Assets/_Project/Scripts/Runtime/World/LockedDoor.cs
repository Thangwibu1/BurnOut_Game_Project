using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LockedDoor : MonoBehaviour
    {
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Color openColor = Color.white;
        private bool opened;
        private void Awake()
        {
            blockingCollider ??= GetComponent<Collider2D>();
            blockingCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (opened || !other.TryGetComponent<PlayerInventory>(out var inventory) || !inventory.ConsumeKey()) return;
            opened = true;
            blockingCollider.enabled = false;
            if (visual != null) { if (openSprite != null) visual.sprite = openSprite; visual.color = openColor; }
            RuntimeSfx.Play(RuntimeSfx.Sound.DoorOpen);
            ImpactFX.Burst(transform.position, new Color(.9f, .8f, .5f), 10, 4f, .3f);
            Juice.Shake(.18f, .2f);
            Destroy(gameObject, .7f);
        }
    }
}
