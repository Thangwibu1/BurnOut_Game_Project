using BurnOut.Player;

namespace BurnOut.Items
{
    public sealed class KeyPickup : PickupBase
    {
        protected override bool Apply(PlayerHealth player)
        {
            player.GetComponent<PlayerInventory>()?.AddKey();
            return true;
        }
    }
}
