using System;

namespace BurnOut.Player
{
    public sealed class PlayerInventory : UnityEngine.MonoBehaviour
    {
        public event Action<bool> KeyStateChanged;
        public bool HasKey { get; private set; }

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
    }
}
