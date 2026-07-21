using System.Collections;
using BurnOut.World;
using UnityEngine;

namespace BurnOut.Core
{
    /// <summary>
    /// Global "game feel" helpers: screen shake and hit-stop (brief freeze frame).
    /// Auto-creates its own runner so nothing in the scene needs to be wired.
    /// Safe to call from anywhere; no art or audio assets required.
    /// </summary>
    public static class Juice
    {
        private static JuiceRunner runner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (runner != null) return;
            var go = new GameObject("~JuiceRunner") { hideFlags = HideFlags.HideInHierarchy };
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<JuiceRunner>();
        }

        /// <summary>Shakes the active gameplay camera. amount is in world units.</summary>
        public static void Shake(float amount, float duration = .25f) => CameraFollow2D.Shake(amount, duration);

        /// <summary>Freezes time briefly for punch on impactful hits. Uses real time so it resolves even at low time scale.</summary>
        public static void HitStop(float seconds)
        {
            if (runner == null) Bootstrap();
            if (runner != null) runner.HitStop(seconds);
        }
    }

    public sealed class JuiceRunner : MonoBehaviour
    {
        private Coroutine hitStopRoutine;

        public void HitStop(float seconds)
        {
            if (seconds <= 0f) return;
            // Never fight the pause menu.
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(HitStopRoutine(seconds));
        }

        private IEnumerator HitStopRoutine(float seconds)
        {
            Time.timeScale = 0.02f;
            yield return new WaitForSecondsRealtime(seconds);
            // Only hand time back if nobody else paused the game meanwhile.
            if (GameManager.Instance == null || !GameManager.Instance.IsPaused) Time.timeScale = 1f;
            hitStopRoutine = null;
        }
    }
}
