using UnityEngine;

namespace BurnOut.Combat
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 9f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private Hitbox2D hitbox;
        private Vector2 direction = Vector2.right;
        public void Launch(Vector2 direction) { this.direction = direction.normalized; hitbox?.BeginHit(); }
        private void Update() { transform.Translate(direction * speed * Time.deltaTime); lifetime -= Time.deltaTime; if (lifetime <= 0f) Destroy(gameObject); }
        private void OnDestroy() => hitbox?.EndHit();
    }
}
