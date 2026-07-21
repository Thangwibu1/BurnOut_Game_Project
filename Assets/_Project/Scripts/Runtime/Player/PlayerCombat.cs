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
        public enum SkillId { None, Aura, Rush, Shockwave }

        [Header("Normal attack")]
        [SerializeField] private Hitbox2D normalAttackHitbox;
        [SerializeField] private float attackActiveTime = .13f;
        [SerializeField] private float attackCooldown = .27f;

        [Header("Skill 1 - Aura (heal + shield)")]
        [SerializeField] private float auraCooldown = 9f;
        [SerializeField] private int auraHealAmount = 3;
        [SerializeField] private float auraSanityRestore = 30f;
        [SerializeField] private float auraShieldTime = 2.5f;

        [Header("Skill 2 - Rush (lunge strike)")]
        [SerializeField] private Hitbox2D rushHitbox;
        [SerializeField] private float rushCooldown = 2.5f;
        [SerializeField] private float rushLungeSpeed = 13f;
        [SerializeField] private float rushActiveTime = .32f; // stays live for the whole lunge so it always connects

        [Header("Skill 3 - Shockwave (ground wave)")]
        [SerializeField] private GameObject shockwavePrefab;
        [SerializeField] private float shockwaveCooldown = 4f;

        [SerializeField] private float lowSanityDamageBonus = 2f;

        private PlayerInputReader input;
        private PlayerMovement movement;
        private PlayerSanity sanity;
        private PlayerHealth health;
        private Rigidbody2D body;
        private float attackCooldownTimer;
        private float auraCooldownTimer;
        private float rushCooldownTimer;
        private float shockwaveCooldownTimer;
        private float attackVisualTimer;
        private float skillVisualTimer;
        private SkillId activeSkill = SkillId.None;

        public bool IsAttacking => attackVisualTimer > 0f;
        public bool IsUsingSkill => skillVisualTimer > 0f;
        public SkillId ActiveSkill => activeSkill;
        public float AuraCooldownRemaining => auraCooldownTimer;
        public float RushCooldownRemaining => rushCooldownTimer;
        public float ShockwaveCooldownRemaining => shockwaveCooldownTimer;
        // Full cooldown durations, so the HUD can show a 0..1 fill for each skill.
        public float AuraCooldown => auraCooldown;
        public float RushCooldown => rushCooldown;
        public float ShockwaveCooldown => shockwaveCooldown;
        public event System.Action AttackPerformed;
        public event System.Action SkillPerformed;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement>();
            sanity = GetComponent<PlayerSanity>();
            health = GetComponent<PlayerHealth>();
            body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            input.AttackPressed += Attack;
            input.Skill1Pressed += Shockwave; // Z — ground slash wave (art: skill 1)
            input.Skill2Pressed += Aura;      // X — focus aura (art: skill 2)
            input.Skill3Pressed += Rush;      // C — lunging rush (art: skill 3)
        }

        private void OnDisable()
        {
            input.AttackPressed -= Attack;
            input.Skill1Pressed -= Shockwave;
            input.Skill2Pressed -= Aura;
            input.Skill3Pressed -= Rush;
        }

        private void Update()
        {
            attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
            auraCooldownTimer = Mathf.Max(0f, auraCooldownTimer - Time.deltaTime);
            rushCooldownTimer = Mathf.Max(0f, rushCooldownTimer - Time.deltaTime);
            shockwaveCooldownTimer = Mathf.Max(0f, shockwaveCooldownTimer - Time.deltaTime);
            attackVisualTimer = Mathf.Max(0f, attackVisualTimer - Time.deltaTime);
            skillVisualTimer = Mathf.Max(0f, skillVisualTimer - Time.deltaTime);
            if (skillVisualTimer <= 0f) activeSkill = SkillId.None;

            // Focus: as sanity collapses, desperation makes Lily's strikes hit far harder.
            float focus = sanity != null && sanity.IsLow ? lowSanityDamageBonus : 1f;
            if (normalAttackHitbox != null) normalAttackHitbox.DamageMultiplier = focus;
            if (rushHitbox != null) rushHitbox.DamageMultiplier = focus * 1.5f;
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

        // Skill 1 — Aura: a defensive focus. Restores health + sanity and grants a brief shield.
        private void Aura()
        {
            if (auraCooldownTimer > 0f) return;
            auraCooldownTimer = auraCooldown;
            skillVisualTimer = .6f;
            activeSkill = SkillId.Aura;
            SkillPerformed?.Invoke();
            health?.Heal(auraHealAmount);
            sanity?.Restore(auraSanityRestore);
            health?.GrantShield(auraShieldTime);
            ImpactFX.Expand(transform.position, new Color(1f, .95f, .5f), 2.6f);
            ImpactFX.Burst(transform.position, new Color(1f, .92f, .55f), 16, 5f, .35f);
            RuntimeSfx.Play(RuntimeSfx.Sound.Checkpoint, .9f);
            Juice.Shake(.1f, .12f);
        }

        // Skill 2 — Rush: a committed lunging strike with a shockwave ring.
        private void Rush()
        {
            if (rushCooldownTimer > 0f || rushHitbox == null) return;
            rushCooldownTimer = rushCooldown;
            skillVisualTimer = attackActiveTime + .3f;
            activeSkill = SkillId.Rush;
            SkillPerformed?.Invoke();
            float dir = movement.FacingRight ? 1f : -1f;
            if (body != null) body.linearVelocity = new Vector2(dir * rushLungeSpeed, Mathf.Max(body.linearVelocity.y, 1.5f));
            ImpactFX.Expand(transform.position + Vector3.right * dir * .6f, new Color(.4f, 1f, .85f), 1.9f);
            RuntimeSfx.Play(RuntimeSfx.Sound.Skill);
            Juice.Shake(.24f, .2f);
            StartCoroutine(ActivateHitbox(rushHitbox, rushActiveTime));
        }

        // Skill 3 — Shockwave: slam the ground and send a travelling wave that hits everything ahead.
        private void Shockwave()
        {
            if (shockwaveCooldownTimer > 0f || shockwavePrefab == null) return;
            shockwaveCooldownTimer = shockwaveCooldown;
            skillVisualTimer = .5f;
            activeSkill = SkillId.Shockwave;
            SkillPerformed?.Invoke();
            float dir = movement.FacingRight ? 1f : -1f;
            var origin = transform.position + new Vector3(dir * .7f, -.55f, 0f);
            var wave = Instantiate(shockwavePrefab, origin, Quaternion.identity);
            var scale = wave.transform.localScale; scale.x = Mathf.Abs(scale.x) * dir; wave.transform.localScale = scale;
            float focus = sanity != null && sanity.IsLow ? lowSanityDamageBonus : 1f;
            foreach (var hb in wave.GetComponentsInChildren<Hitbox2D>()) hb.DamageMultiplier = focus;
            wave.GetComponent<Projectile>()?.Launch(new Vector2(dir, 0f));
            ImpactFX.Expand(transform.position, new Color(1f, .6f, .35f), 2.2f);
            RuntimeSfx.Play(RuntimeSfx.Sound.BossSlam, .8f);
            Juice.Shake(.3f, .25f);
            Juice.HitStop(.04f);
        }

        private IEnumerator ActivateHitbox(Hitbox2D hitbox, float activeTime = -1f)
        {
            hitbox.BeginHit();
            yield return new WaitForSeconds(activeTime > 0f ? activeTime : attackActiveTime);
            hitbox.EndHit();
        }
    }
}

