using UnityEngine;

namespace BurnOut.Enemies
{
    public sealed class EnemyMovement : MonoBehaviour
    {
        public void MoveHorizontally(float direction, float speed) => transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }
}
