using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Items
{
    public sealed class SanityPickup : PickupBase
    {
        [SerializeField] private float restoreAmount = 25f;
        protected override bool Apply(PlayerHealth player)
        {
            player.GetComponent<PlayerSanity>()?.Restore(restoreAmount);
            return true;
        }
    }
}
