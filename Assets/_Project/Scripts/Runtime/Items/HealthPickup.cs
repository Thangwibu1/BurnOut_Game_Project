using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Items
{
    public sealed class HealthPickup : PickupBase
    {
        [SerializeField] private int restoreAmount = 2;
        protected override bool Apply(PlayerHealth player) { player.Heal(restoreAmount); return true; }
    }
}
