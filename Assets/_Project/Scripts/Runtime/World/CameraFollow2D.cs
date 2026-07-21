using UnityEngine;

namespace BurnOut.World
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        public static CameraFollow2D Instance { get; private set; }

        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 2f, -10f);
        [SerializeField] private float smoothTime = .12f;

        private Vector3 velocity;
        private Vector3 basePosition;
        private float shakeAmount;
        private float shakeDecay;

        /// <summary>Adds a decaying positional shake to whichever gameplay camera is active.</summary>
        public static void Shake(float amount, float duration = .25f)
        {
            if (Instance != null) Instance.AddShake(amount, duration);
        }

        private void Awake()
        {
            Instance = this;
            basePosition = transform.position;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void AddShake(float amount, float duration)
        {
            shakeAmount = Mathf.Max(shakeAmount, amount);
            shakeDecay = Mathf.Max(shakeDecay, duration <= 0f ? 12f : amount / duration);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            basePosition = Vector3.SmoothDamp(basePosition, target.position + offset, ref velocity, smoothTime);

            Vector3 shake = Vector3.zero;
            if (shakeAmount > 0f)
            {
                shake = new Vector3(Random.value * 2f - 1f, Random.value * 2f - 1f, 0f) * shakeAmount;
                // Real time so shake resolves smoothly even during hit-stop.
                shakeAmount = Mathf.Max(0f, shakeAmount - shakeDecay * Time.unscaledDeltaTime);
            }

            transform.position = basePosition + shake;
        }
    }
}
