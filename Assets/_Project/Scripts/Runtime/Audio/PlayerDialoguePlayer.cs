using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BurnOut.Audio
{
    /// <summary>
    /// Plays the player's spoken lines during gameplay: 5s after the level loads it plays
    /// dialogue 1, then alternates to dialogue 2 and back every 15s, repeating for the whole level.
    /// A single persistent listener spawns a fresh player each time the gameplay scene loads.
    /// </summary>
    public sealed class PlayerDialoguePlayer : MonoBehaviour
    {
        private const string LevelScene = "SC_Level01";
        private const float InitialDelay = 5f;
        private const float Interval = 15f;

        // Runs once at startup and registers a scene-load hook. We can't spawn the player directly
        // here: this fires while the menu is the active scene, and it never fires again on later
        // SceneManager.LoadScene calls — so we listen for the level scene loading instead.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Handle the case where the game is launched straight into the level (e.g. from the editor).
            if (SceneManager.GetActiveScene().name == LevelScene) Spawn();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == LevelScene) Spawn();
        }

        private static void Spawn()
        {
            var go = new GameObject("~PlayerDialoguePlayer") { hideFlags = HideFlags.HideInHierarchy };
            go.AddComponent<PlayerDialoguePlayer>();
        }

        private AudioSource source;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f; // 2D, non-positional
            source.playOnAwake = false;
            source.volume = 1f;
        }

        private void Start() => StartCoroutine(PlayLoop());

        private IEnumerator PlayLoop()
        {
            var clip1 = RuntimeSfx.LoadClip("SFX/player_dialogue-1");
            var clip2 = RuntimeSfx.LoadClip("SFX/player_dialogue-2");
            if (clip1 == null && clip2 == null) yield break;

            // Real-time waits so the schedule holds even if the game pauses (Time.timeScale == 0).
            yield return new WaitForSecondsRealtime(InitialDelay);

            bool first = true;
            while (true)
            {
                var clip = first ? clip1 : clip2;
                if (clip != null) source.PlayOneShot(clip);
                first = !first;
                yield return new WaitForSecondsRealtime(Interval);
            }
        }
    }
}
