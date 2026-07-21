using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BurnOut.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public event Action<Vector2> MoveChanged;
        public event Action JumpPressed;
        public event Action DashPressed;
        public event Action AttackPressed;
        public event Action SkillPressed;
        public event Action InteractPressed;
        public event Action PausePressed;

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;
        private InputAction attackAction;
        private InputAction skillAction;
        private InputAction interactAction;
        private InputAction pauseAction;

        private void Awake()
        {
            var actions = GetComponent<PlayerInput>().actions;
            if (actions == null) { Debug.LogError($"[{nameof(PlayerInputReader)}] Input actions are missing on {name}.", this); enabled = false; return; }
            moveAction = actions.FindAction("Move", true);
            jumpAction = actions.FindAction("Jump", true);
            dashAction = actions.FindAction("Dash", true);
            attackAction = actions.FindAction("Attack", true);
            skillAction = actions.FindAction("Skill", true);
            interactAction = actions.FindAction("Interact", true);
            pauseAction = actions.FindAction("Pause", true);
        }

        private void OnEnable()
        {
            moveAction.performed += OnMove; moveAction.canceled += OnMove;
            jumpAction.performed += OnJump; dashAction.performed += OnDash; attackAction.performed += OnAttack;
            skillAction.performed += OnSkill; interactAction.performed += OnInteract; pauseAction.performed += OnPause;
        }

        private void OnDisable()
        {
            if (moveAction == null) return;
            moveAction.performed -= OnMove; moveAction.canceled -= OnMove;
            jumpAction.performed -= OnJump; dashAction.performed -= OnDash; attackAction.performed -= OnAttack;
            skillAction.performed -= OnSkill; interactAction.performed -= OnInteract; pauseAction.performed -= OnPause;
        }

        private void OnMove(InputAction.CallbackContext context) => MoveChanged?.Invoke(context.ReadValue<Vector2>());
        private void OnJump(InputAction.CallbackContext _) => JumpPressed?.Invoke();
        private void OnDash(InputAction.CallbackContext _) => DashPressed?.Invoke();
        private void OnAttack(InputAction.CallbackContext _) => AttackPressed?.Invoke();
        private void OnSkill(InputAction.CallbackContext _) => SkillPressed?.Invoke();
        private void OnInteract(InputAction.CallbackContext _) => InteractPressed?.Invoke();
        private void OnPause(InputAction.CallbackContext _) => PausePressed?.Invoke();
    }
}
