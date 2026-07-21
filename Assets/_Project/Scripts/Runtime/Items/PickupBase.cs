using BurnOut.Audio;
using BurnOut.Combat;
using UnityEngine;

namespace BurnOut.Items
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class PickupBase : MonoBehaviour
    {
        [SerializeField] private RuntimeSfx.Sound collectSound = RuntimeSfx.Sound.Pickup;
        [SerializeField] private Color collectColor = new(.6f, 1f, .9f);

        protected virtual void Awake()
        {
            var collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<BurnOut.Player.PlayerHealth>(out var player)) return;
            if (!Apply(player)) return;
            RuntimeSfx.Play(collectSound);
            ImpactFX.Burst(transform.position, collectColor, 9, 4.5f, .26f);
            Destroy(gameObject);
        }

        protected abstract bool Apply(BurnOut.Player.PlayerHealth player);
    }
}
