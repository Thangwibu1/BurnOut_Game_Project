using System.Collections;
using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
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
        [SerializeField] private float skillLungeSpeed = 11f;
        [SerializeField] private float lowSanityDamageBonus = 2f;

        private PlayerInputReader input;
        private PlayerMovement movement;
        private PlayerSanity sanity;
        private Rigidbody2D body;
        private float attackCooldownTimer;
        private float skillCooldownTimer;
        private float attackVisualTimer;
        private float skillVisualTimer;

        public float SkillCooldownRemaining => skillCooldownTimer;
        public bool IsAttacking => attackVisualTimer > 0f;
        public bool IsUsingSkill => skillVisualTimer > 0f;
        public event System.Action AttackPerformed;
        public event System.Action SkillPerformed;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement>();
            sanity = GetComponent<PlayerSanity>();
            body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable() { input.AttackPressed += Attack; input.SkillPressed += Skill; }
        private void OnDisable() { input.AttackPressed -= Attack; input.SkillPressed -= Skill; }

        private void Update()
        {
            attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
            skillCooldownTimer = Mathf.Max(0f, skillCooldownTimer - Time.deltaTime);
            attackVisualTimer = Mathf.Max(0f, attackVisualTimer - Time.deltaTime);
            skillVisualTimer = Mathf.Max(0f, skillVisualTimer - Time.deltaTime);

            // Focus: as sanity collapses, desperation makes Lily's strikes hit far harder.
            float focus = sanity != null && sanity.IsLow ? lowSanityDamageBonus : 1f;
            if (normalAttackHitbox != null) normalAttackHitbox.DamageMultiplier = focus;
            if (skillHitbox != null) skillHitbox.DamageMultiplier = focus * 1.5f;
        }

        private void Attack()
        {
            if (attackCooldownTimer > 0f || normalAttackHitbox == null) return;
            attackCooldownTimer = attackCooldown;
            attackVisualTimer = attackActiveTime + .12f;
            AttackPerformed?.Invoke();
            RuntimeSfx.Play(RuntimeSfx.Sound.Attack, .8f);
            Juice.Shake(.06f, .08f);
            StartCoroutine(ActivateHitbox(normalAttackHitbox));
        }

        private void Skill()
        {
            if (skillCooldownTimer > 0f || skillHitbox == null) return;
            skillCooldownTimer = skillCooldown;
            skillVisualTimer = attackActiveTime + .28f;
            SkillPerformed?.Invoke();

            // Skill is now a committed lunging strike with a shockwave — heavier, riskier, more satisfying.
            float dir = movement.FacingRight ? 1f : -1f;
            if (body != null) body.linearVelocity = new Vector2(dir * skillLungeSpeed, Mathf.Max(body.linearVelocity.y, 1.5f));
            ImpactFX.Expand(transform.position + Vector3.right * dir * .6f, new Color(.4f, 1f, .85f), 1.9f);
            RuntimeSfx.Play(RuntimeSfx.Sound.Skill);
            Juice.Shake(.24f, .2f);
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
