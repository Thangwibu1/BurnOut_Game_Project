using BurnOut.Combat;
using UnityEngine;

namespace BurnOut.Enemies
{
    [RequireComponent(typeof(Projectile))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        public void FireAt(Vector3 target) => GetComponent<Projectile>().Launch((target - transform.position).normalized);
    }
}
