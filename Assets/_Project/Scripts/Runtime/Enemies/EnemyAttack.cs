using BurnOut.Combat;
using UnityEngine;

namespace BurnOut.Enemies
{
    public sealed class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private Hitbox2D hitbox;
        [SerializeField] private float cooldown = 1f;
        private float nextAttackTime;
        public bool TryAttack()
        {
            if (hitbox == null || Time.time < nextAttackTime) return false;
            nextAttackTime = Time.time + cooldown; hitbox.BeginHit(); Invoke(nameof(EndAttack), .15f); return true;
        }
        private void EndAttack() => hitbox?.EndHit();
    }
}
