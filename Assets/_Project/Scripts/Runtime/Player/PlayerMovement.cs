using BurnOut.Input;
using UnityEngine;

namespace BurnOut.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerMovementConfig config;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = .16f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform visual;

        private Rigidbody2D body;
        private PlayerInputReader input;
        private Vector2 moveInput;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private float dashTimer;
        private float dashCooldownTimer;
        private bool usedDoubleJump;

        public bool IsGrounded { get; private set; }
        public bool IsDashing => dashTimer > 0f;
        public bool FacingRight { get; private set; } = true;
        public float HorizontalSpeed => body == null ? 0f : body.linearVelocity.x;
        public event System.Action Dashed;
        public event System.Action Jumped;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
            body.freezeRotation = true;
            if (visual == null) visual = transform;
            if (config == null || groundCheck == null) { Debug.LogError($"[{nameof(PlayerMovement)}] Config or Ground Check is missing on {name}.", this); enabled = false; }
        }

        private void OnEnable()
        {
            input.MoveChanged += SetMove; input.JumpPressed += BufferJump; input.DashPressed += TryDash;
        }

        private void OnDisable()
        {
            input.MoveChanged -= SetMove; input.JumpPressed -= BufferJump; input.DashPressed -= TryDash;
        }

        private void Update()
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            coyoteCounter = IsGrounded ? config.CoyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime);
            if (IsGrounded) usedDoubleJump = false;
            jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
            if (jumpBufferCounter > 0f && (coyoteCounter > 0f || !usedDoubleJump)) PerformJump();
            if (moveInput.x > .01f && !FacingRight || moveInput.x < -.01f && FacingRight) Flip();
        }

        private void FixedUpdate()
        {
            if (IsDashing)
            {
                dashTimer -= Time.fixedDeltaTime;
                body.linearVelocity = new Vector2((FacingRight ? 1f : -1f) * config.DashSpeed, 0f);
                return;
            }
            var target = moveInput.x * config.MoveSpeed;
            var rate = IsGrounded ? (Mathf.Abs(target) > .01f ? config.Acceleration : config.Deceleration) : config.AirAcceleration;
            body.linearVelocity = new Vector2(Mathf.MoveTowards(body.linearVelocity.x, target, rate * Time.fixedDeltaTime), body.linearVelocity.y);
        }

        public void Teleport(Vector3 position)
        {
            body.position = position;
            body.linearVelocity = Vector2.zero;
        }

        private void SetMove(Vector2 value) => moveInput = value;
        private void BufferJump() => jumpBufferCounter = config.JumpBufferTime;

        private void PerformJump()
        {
            var groundedJump = coyoteCounter > 0f;
            body.linearVelocity = new Vector2(body.linearVelocity.x, groundedJump ? config.JumpForce : config.DoubleJumpForce);
            if (!groundedJump) usedDoubleJump = true;
            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
            Jumped?.Invoke();
        }

        private void TryDash()
        {
            if (dashCooldownTimer > 0f || IsDashing) return;
            dashTimer = config.DashDuration;
            dashCooldownTimer = config.DashCooldown;
            Dashed?.Invoke();
        }

        private void Flip()
        {
            FacingRight = !FacingRight;
            var targetVisual = visual != null ? visual : transform;
            var scale = targetVisual.localScale;
            scale.x = Mathf.Abs(scale.x) * (FacingRight ? 1f : -1f);
            targetVisual.localScale = scale;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
