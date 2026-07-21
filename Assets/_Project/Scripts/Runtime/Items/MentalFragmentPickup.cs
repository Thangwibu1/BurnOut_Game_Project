using BurnOut.Core;
using BurnOut.Player;

namespace BurnOut.Items
{
    public sealed class MentalFragmentPickup : PickupBase
    {
        protected override bool Apply(PlayerHealth player) { GameManager.Instance?.CompleteLevel(); return true; }
    }
}
