using System;

namespace BurnOut.Player
{
    public sealed class PlayerInventory : UnityEngine.MonoBehaviour
    {
        public event Action<bool> KeyStateChanged;
        public bool HasKey { get; private set; }
        public int MentalFragments { get; private set; }
        public bool HasMentalFragment => MentalFragments > 0;

        public void AddKey()
        {
            HasKey = true;
            KeyStateChanged?.Invoke(true);
        }

        public bool ConsumeKey()
        {
            if (!HasKey) return false;
            HasKey = false;
            KeyStateChanged?.Invoke(false);
            return true;
        }

        public void AddMentalFragment() => MentalFragments++;
    }
}
