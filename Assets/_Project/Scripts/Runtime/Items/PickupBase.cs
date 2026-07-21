using UnityEngine;

namespace BurnOut.Items
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class PickupBase : MonoBehaviour
    {
        protected virtual void Awake()
        {
            var collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<BurnOut.Player.PlayerHealth>(out var player)) return;
            if (Apply(player)) Destroy(gameObject);
        }

        protected abstract bool Apply(BurnOut.Player.PlayerHealth player);
    }
}
