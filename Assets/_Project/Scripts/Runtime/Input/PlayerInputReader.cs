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
        public event Action Skill1Pressed;
        public event Action Skill2Pressed;
        public event Action Skill3Pressed;
        public event Action InteractPressed;
        public event Action PausePressed;

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;
        private InputAction attackAction;
        private InputAction skill1Action;
        private InputAction skill2Action;
        private InputAction skill3Action;
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
            skill1Action = actions.FindAction("Skill1", true);
            skill2Action = actions.FindAction("Skill2", true);
            skill3Action = actions.FindAction("Skill3", true);
            interactAction = actions.FindAction("Interact", true);
            pauseAction = actions.FindAction("Pause", true);
        }

        private void OnEnable()
        {
            moveAction.performed += OnMove; moveAction.canceled += OnMove;
            jumpAction.performed += OnJump; dashAction.performed += OnDash; attackAction.performed += OnAttack;
            skill1Action.performed += OnSkill1; skill2Action.performed += OnSkill2; skill3Action.performed += OnSkill3;
            interactAction.performed += OnInteract; pauseAction.performed += OnPause;
        }

        private void OnDisable()
        {
            if (moveAction == null) return;
            moveAction.performed -= OnMove; moveAction.canceled -= OnMove;
            jumpAction.performed -= OnJump; dashAction.performed -= OnDash; attackAction.performed -= OnAttack;
            skill1Action.performed -= OnSkill1; skill2Action.performed -= OnSkill2; skill3Action.performed -= OnSkill3;
            interactAction.performed -= OnInteract; pauseAction.performed -= OnPause;
        }

        private void OnMove(InputAction.CallbackContext context) => MoveChanged?.Invoke(context.ReadValue<Vector2>());
        private void OnJump(InputAction.CallbackContext _) => JumpPressed?.Invoke();
        private void OnDash(InputAction.CallbackContext _) => DashPressed?.Invoke();
        private void OnAttack(InputAction.CallbackContext _) => AttackPressed?.Invoke();
        private void OnSkill1(InputAction.CallbackContext _) => Skill1Pressed?.Invoke();
        private void OnSkill2(InputAction.CallbackContext _) => Skill2Pressed?.Invoke();
        private void OnSkill3(InputAction.CallbackContext _) => Skill3Pressed?.Invoke();
        private void OnInteract(InputAction.CallbackContext _) => InteractPressed?.Invoke();
        private void OnPause(InputAction.CallbackContext _) => PausePressed?.Invoke();
    }
}
