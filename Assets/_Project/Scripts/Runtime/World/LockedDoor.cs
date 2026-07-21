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
        [SerializeField] private Color openColor = new(.65f, 1f, .9f, 1f);
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
            Destroy(gameObject, .7f);
        }
    }
}
