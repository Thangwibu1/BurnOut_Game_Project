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

        private Camera cam;
        private float baseOrthoSize;
        private float targetOrthoSize;
        private float zoomSpeed;

        /// <summary>Adds a decaying positional shake to whichever gameplay camera is active.</summary>
        public static void Shake(float amount, float duration = .25f)
        {
            if (Instance != null) Instance.AddShake(amount, duration);
        }

        /// <summary>Eases the camera to a tighter orthographic size (cinematic zoom-in).</summary>
        public static void Zoom(float orthoSize, float duration)
        {
            if (Instance != null) Instance.SetZoom(orthoSize, duration);
        }

        /// <summary>Eases the camera back to its default framing.</summary>
        public static void ResetZoom(float duration)
        {
            if (Instance != null) Instance.SetZoom(Instance.baseOrthoSize, duration);
        }

        private void Awake()
        {
            Instance = this;
            basePosition = transform.position;
            cam = GetComponent<Camera>();
            baseOrthoSize = cam != null ? cam.orthographicSize : 5.5f;
            targetOrthoSize = baseOrthoSize;
        }

        private void SetZoom(float orthoSize, float duration)
        {
            if (cam == null) return;
            targetOrthoSize = orthoSize;
            zoomSpeed = duration <= 0f ? 999f : Mathf.Abs(cam.orthographicSize - orthoSize) / duration;
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

            // Cinematic zoom runs on unscaled time so it stays smooth during slow-mo.
            if (cam != null && !Mathf.Approximately(cam.orthographicSize, targetOrthoSize))
                cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, targetOrthoSize, zoomSpeed * Time.unscaledDeltaTime);
        }
    }
}
