using UnityEngine;

namespace BurnOut.Player
{
    [CreateAssetMenu(menuName = "Burn Out/Player Movement Config", fileName = "PlayerMovementConfig")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [field: SerializeField] public float MoveSpeed { get; private set; } = 7f;
        [field: SerializeField] public float Acceleration { get; private set; } = 55f;
        [field: SerializeField] public float Deceleration { get; private set; } = 65f;
        [field: SerializeField] public float AirAcceleration { get; private set; } = 32f;
        [field: SerializeField] public float JumpForce { get; private set; } = 13f;
        [field: SerializeField] public float DoubleJumpForce { get; private set; } = 12f;
        [field: SerializeField] public float CoyoteTime { get; private set; } = .12f;
        [field: SerializeField] public float JumpBufferTime { get; private set; } = .12f;
        [field: SerializeField] public float DashSpeed { get; private set; } = 16f;
        [field: SerializeField] public float DashDuration { get; private set; } = .14f;
        [field: SerializeField] public float DashCooldown { get; private set; } = .55f;
    }
}
