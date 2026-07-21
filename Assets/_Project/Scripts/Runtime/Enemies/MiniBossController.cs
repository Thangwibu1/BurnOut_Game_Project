using System.Collections;
using BurnOut.Audio;
using BurnOut.Combat;
using BurnOut.Core;
using BurnOut.Player;
using UnityEngine;

namespace BurnOut.Enemies
{
    /// <summary>
    /// A readable multi-phase mini-boss: it chases, then either telegraphs a heavy lunging slam
    /// or fires an arc of energy bolts. Below 40% health it enrages — faster, angrier, bigger volleys.
    /// State is exposed so <see cref="BossVisualAnimator"/> can drive its frames.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class MiniBossController : MonoBehaviour
    {
        public enum BossState { Idle, Chase, Telegraph, Slam, Shoot, Dead }

        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float enrageMoveSpeed = 3.6f;
        [SerializeField] private float actionInterval = 2.6f;
        [SerializeField] private float enrageActionInterval = 1.5f;
        [SerializeField] private float slamRange = 6.5f;
        [SerializeField] private float slamSpeed = 22f;
        [SerializeField] private int slamDamage = 2;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float contactRange = 1.15f;
        [SerializeField] private float contactVerticalRange = 1.7f;
        [SerializeField] private float contactCooldown = .9f;
        [SerializeField] private GameObject energyProjectilePrefab;
        [SerializeField] private GameObject mentalFragmentPrefab;
        [SerializeField] private Color enrageTint = new(1f, .38f, .38f);

        private EnemyHealth health;
        private SpriteRenderer sprite;
        private Transform player;
        private PlayerHealth playerHealth;
        private BossState state = BossState.Idle;
        private float nextAction;
        private float nextContact;
        private float groundY;
        private bool enraged;
        private bool acting;

        public BossState State => state;
        public bool FacingRight { get; private set; } = true;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            sprite = GetComponent<SpriteRenderer>();
            groundY = transform.position.y;
            health.Died += OnDied;
        }

        private void OnEnable() => nextAction = Time.time + 1.2f;

        private void Update()
        {
            if (state == BossState.Dead) return;
            if (player == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
                player = playerHealth != null ? playerHealth.transform : null;
                if (player == null) return;
            }
            if (!health.IsAlive) return;

            if (!enraged && health.CurrentHealth <= Mathf.CeilToInt(health.MaxHealth * .4f)) Enrage();

            FacePlayer();
            TryContact();

            if (acting) return;

            float dist = Mathf.Abs(player.position.x - transform.position.x);
            if (Time.time >= nextAction)
            {
                StartCoroutine(dist <= slamRange ? SlamRoutine() : ShootRoutine());
                return;
            }

            // Default behaviour: close the distance.
            state = BossState.Chase;
            float speed = enraged ? enrageMoveSpeed : moveSpeed;
            var pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, player.position.x, speed * Time.deltaTime);
            pos.y = Mathf.MoveTowards(pos.y, groundY, speed * Time.deltaTime);
            transform.position = pos;
        }

        private IEnumerator SlamRoutine()
        {
            acting = true;
            nextAction = Time.time + (enraged ? enrageActionInterval : actionInterval);

            // Telegraph: pulse bright so the player can read and dodge the incoming lunge.
            state = BossState.Telegraph;
            RuntimeSfx.Play(RuntimeSfx.Sound.BossTelegraph);
            ImpactFX.Expand(transform.position, new Color(1f, .5f, .3f, .8f), 2.2f);
            float telegraph = enraged ? .45f : .7f;
            float e = 0f;
            var restColor = enraged ? enrageTint : Color.white;
            while (e < telegraph)
            {
                e += Time.deltaTime;
                if (sprite != null) sprite.color = Color.Lerp(restColor, new Color(1f, .92f, .45f), Mathf.PingPong(e * 8f, 1f));
                yield return null;
            }
            if (sprite != null) sprite.color = restColor;

            // Lunge toward the player's ground position.
            state = BossState.Slam;
            float targetX = player.position.x;
            var target = new Vector3(targetX, groundY, transform.position.z);
            float guard = 0f;
            while (Mathf.Abs(transform.position.x - targetX) > .12f && guard < 1.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, slamSpeed * Time.deltaTime);
                TryContact();
                guard += Time.deltaTime;
                if (state == BossState.Dead) yield break;
                yield return null;
            }

            // Landing impact.
            Juice.Shake(.5f, .35f);
            Juice.HitStop(.05f);
            RuntimeSfx.Play(RuntimeSfx.Sound.BossSlam);
            ImpactFX.Expand(transform.position, new Color(1f, .4f, .25f), 3.2f);
            ImpactFX.Burst(transform.position, new Color(1f, .6f, .3f), 14, 7f, .4f);

            state = BossState.Idle;
            acting = false;
        }

        private IEnumerator ShootRoutine()
        {
            acting = true;
            nextAction = Time.time + (enraged ? enrageActionInterval : actionInterval);
            state = BossState.Shoot;
            RuntimeSfx.Play(RuntimeSfx.Sound.BossTelegraph, .8f);
            yield return new WaitForSeconds(.35f);

            if (energyProjectilePrefab != null && player != null && state != BossState.Dead)
            {
                int shots = enraged ? 5 : 3;
                var origin = transform.position + Vector3.up * .3f;
                var toPlayer = ((Vector2)(player.position - origin)).normalized;
                float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x);
                float spread = 28f * Mathf.Deg2Rad;
                for (int i = 0; i < shots; i++)
                {
                    float t = shots == 1 ? .5f : i / (float)(shots - 1);
                    float ang = baseAngle + Mathf.Lerp(-spread, spread, t);
                    var dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
                    var proj = Instantiate(energyProjectilePrefab, origin, Quaternion.identity);
                    proj.GetComponent<EnemyProjectile>()?.FireAt(origin + dir);
                }
                RuntimeSfx.Play(RuntimeSfx.Sound.Skill, .7f);
            }

            yield return new WaitForSeconds(.25f);
            state = BossState.Idle;
            acting = false;
        }

        private void TryContact()
        {
            if (player == null || Time.time < nextContact) return;
            if (Mathf.Abs(player.position.x - transform.position.x) > contactRange) return;
            if (Mathf.Abs(player.position.y - transform.position.y) > contactVerticalRange) return;
            nextContact = Time.time + contactCooldown;
            bool slamming = state == BossState.Slam;
            playerHealth?.TakeDamage(new DamageInfo(slamming ? slamDamage : contactDamage, transform.position, slamming ? 9f : 6f));
        }

        private void Enrage()
        {
            enraged = true;
            if (sprite != null) sprite.color = enrageTint;
            var s = transform.localScale;
            transform.localScale = new Vector3(s.x * 1.12f, Mathf.Abs(s.y) * 1.12f, s.z);
            Juice.Shake(.4f, .4f);
            RuntimeSfx.Play(RuntimeSfx.Sound.BossTelegraph);
            ImpactFX.Expand(transform.position, enrageTint, 3.5f);
        }

        private void FacePlayer()
        {
            bool right = player.position.x >= transform.position.x;
            if (right == FacingRight) return;
            FacingRight = right;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (right ? 1f : -1f);
            transform.localScale = s;
        }

        private void OnDied()
        {
            state = BossState.Dead;
            StopAllCoroutines();
            Juice.Shake(.7f, .6f);
            Juice.HitStop(.12f);
            RuntimeSfx.Play(RuntimeSfx.Sound.BossSlam);
            ImpactFX.Expand(transform.position, new Color(1f, .5f, .95f), 4f);
            ImpactFX.Burst(transform.position, new Color(.9f, .4f, 1f), 22, 8f, .5f);
            if (mentalFragmentPrefab != null) Instantiate(mentalFragmentPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }
}
