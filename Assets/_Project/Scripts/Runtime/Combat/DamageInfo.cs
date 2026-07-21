using UnityEngine;

namespace BurnOut.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(int amount, Vector2 sourcePosition, float knockback)
        {
            Amount = Mathf.Max(0, amount);
            SourcePosition = sourcePosition;
            Knockback = Mathf.Max(0f, knockback);
        }

        public int Amount { get; }
        public Vector2 SourcePosition { get; }
        public float Knockback { get; }
    }
}
