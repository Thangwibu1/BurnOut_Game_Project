using UnityEngine;

namespace BurnOut.Enemies
{
    [RequireComponent(typeof(SpriteRenderer), typeof(EnemyBrain), typeof(EnemyHealth))]
    public sealed class EnemyVisualAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] moveFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private float frameRate = 9f;
        private EnemyBrain brain;
        private EnemyHealth health;
        private Sprite[] activeFrames;
        private int frameIndex;
        private float timer;

        private void Awake() { spriteRenderer ??= GetComponent<SpriteRenderer>(); brain = GetComponent<EnemyBrain>(); health = GetComponent<EnemyHealth>(); }
        private void Update()
        {
            var frames = !health.IsAlive ? deathFrames : brain.CurrentState == EnemyBrain.State.Attack ? attackFrames : brain.CurrentState == EnemyBrain.State.Patrol || brain.CurrentState == EnemyBrain.State.Chase ? moveFrames : idleFrames;
            if (frames == null || frames.Length == 0) return;
            if (activeFrames != frames) { activeFrames = frames; frameIndex = 0; timer = 0f; spriteRenderer.sprite = frames[0]; }
            timer += Time.deltaTime;
            if (timer < 1f / frameRate) return;
            timer = 0f; frameIndex = (frameIndex + 1) % frames.Length; spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
