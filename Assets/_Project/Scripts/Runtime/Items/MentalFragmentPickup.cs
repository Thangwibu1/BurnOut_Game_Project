using BurnOut.Player;

namespace BurnOut.Items
{
    public sealed class MentalFragmentPickup : PickupBase
    {
        protected override bool Apply(PlayerHealth player)
        {
            player.GetComponent<PlayerInventory>()?.AddMentalFragment();
            return true;
        }
    }
}
