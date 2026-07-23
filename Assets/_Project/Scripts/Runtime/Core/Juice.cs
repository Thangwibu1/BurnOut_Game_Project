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

        /// <summary>Cinematic beat: slows time and zooms the camera onto the player, holds, then eases back.
        /// Used to show off big skill casts (e.g. the Z shockwave).</summary>
        public static void Cinematic(float timeScale, float zoomSize, float holdSeconds)
        {
            if (runner == null) Bootstrap();
            if (runner != null) runner.Cinematic(timeScale, zoomSize, holdSeconds);
        }
    }

    public sealed class JuiceRunner : MonoBehaviour
    {
        private Coroutine hitStopRoutine;
        private Coroutine cinematicRoutine;

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

        public void Cinematic(float timeScale, float zoomSize, float holdSeconds)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (cinematicRoutine != null) StopCoroutine(cinematicRoutine);
            cinematicRoutine = StartCoroutine(CinematicRoutine(timeScale, zoomSize, holdSeconds));
        }

        private IEnumerator CinematicRoutine(float timeScale, float zoomSize, float holdSeconds)
        {
            // Slow time so the cast animation reads clearly, and punch the camera in.
            Time.timeScale = Mathf.Clamp(timeScale, 0.05f, 1f);
            CameraFollow2D.Zoom(zoomSize, .1f);
            yield return new WaitForSecondsRealtime(holdSeconds);
            // Ease time and framing back to normal.
            CameraFollow2D.ResetZoom(.22f);
            float t = 0f;
            while (t < .18f)
            {
                t += Time.unscaledDeltaTime;
                if (GameManager.Instance != null && GameManager.Instance.IsPaused) { cinematicRoutine = null; yield break; }
                Time.timeScale = Mathf.Lerp(timeScale, 1f, t / .18f);
                yield return null;
            }
            if (GameManager.Instance == null || !GameManager.Instance.IsPaused) Time.timeScale = 1f;
            cinematicRoutine = null;
        }
    }
}
