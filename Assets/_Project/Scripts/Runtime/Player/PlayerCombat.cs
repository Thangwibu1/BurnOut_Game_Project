using System.Collections;
using BurnOut.Combat;
using BurnOut.Input;
using UnityEngine;

namespace BurnOut.Player
{
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMovement))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private Hitbox2D normalAttackHitbox;
        [SerializeField] private Hitbox2D skillHitbox;
        [SerializeField] private float attackActiveTime = .13f;
        [SerializeField] private float attackCooldown = .27f;
        [SerializeField] private float skillCooldown = 2.5f;
        private PlayerInputReader input;
        private float attackCooldownTimer;
        private float skillCooldownTimer;
        private float attackVisualTimer;
        private float skillVisualTimer;

        public float SkillCooldownRemaining => skillCooldownTimer;
        public bool IsAttacking => attackVisualTimer > 0f;
        public bool IsUsingSkill => skillVisualTimer > 0f;
        public event System.Action AttackPerformed;
        public event System.Action SkillPerformed;

        private void Awake() => input = GetComponent<PlayerInputReader>();
        private void OnEnable() { input.AttackPressed += Attack; input.SkillPressed += Skill; }
        private void OnDisable() { input.AttackPressed -= Attack; input.SkillPressed -= Skill; }
        private void Update() { attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime); skillCooldownTimer = Mathf.Max(0f, skillCooldownTimer - Time.deltaTime); attackVisualTimer = Mathf.Max(0f, attackVisualTimer - Time.deltaTime); skillVisualTimer = Mathf.Max(0f, skillVisualTimer - Time.deltaTime); }

        private void Attack()
        {
            if (attackCooldownTimer > 0f || normalAttackHitbox == null) return;
            attackCooldownTimer = attackCooldown;
            attackVisualTimer = attackActiveTime + .12f;
            AttackPerformed?.Invoke();
            StartCoroutine(ActivateHitbox(normalAttackHitbox));
        }

        private void Skill()
        {
            if (skillCooldownTimer > 0f || skillHitbox == null) return;
            skillCooldownTimer = skillCooldown;
            skillVisualTimer = attackActiveTime + .28f;
            SkillPerformed?.Invoke();
            StartCoroutine(ActivateHitbox(skillHitbox));
        }

        private IEnumerator ActivateHitbox(Hitbox2D hitbox)
        {
            hitbox.BeginHit();
            yield return new WaitForSeconds(attackActiveTime);
            hitbox.EndHit();
        }
    }
}
