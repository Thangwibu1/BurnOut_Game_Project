using BurnOut.Player;
using UnityEngine;

namespace BurnOut.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LockedDoor : MonoBehaviour
    {
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Color openColor = new(.35f, 1f, .7f, .25f);
        private void Awake()
        {
            blockingCollider ??= GetComponent<Collider2D>();
            blockingCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerInventory>(out var inventory) || !inventory.ConsumeKey()) return;
            blockingCollider.enabled = false;
            if (visual != null) visual.color = openColor;
            Destroy(gameObject, .35f);
        }
    }
}
