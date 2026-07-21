using UnityEngine;

namespace BurnOut.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerHealth))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private PlayerMovement movement;
        private PlayerHealth health;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int IsDashing = Animator.StringToHash("IsDashing");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private void Awake() { movement = GetComponent<PlayerMovement>(); health = GetComponent<PlayerHealth>(); }
        private void Update()
        {
            if (animator == null) return;
            var velocity = GetComponent<Rigidbody2D>().linearVelocity;
            animator.SetFloat(Speed, Mathf.Abs(velocity.x)); animator.SetFloat(VerticalSpeed, velocity.y); animator.SetBool(IsGrounded, movement.IsGrounded); animator.SetBool(IsDashing, movement.IsDashing); animator.SetBool(IsDead, !health.IsAlive);
        }
    }
}
