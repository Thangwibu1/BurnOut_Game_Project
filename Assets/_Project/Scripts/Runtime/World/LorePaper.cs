using BurnOut.Audio;
using BurnOut.Player;
using BurnOut.UI;
using UnityEngine;

namespace BurnOut.World
{
    // A readable paper scattered in the level. Walking into it shows its line in the bottom
    // message box. The paper is not consumed — it stays in the world so the player can re-read it.
    [RequireComponent(typeof(Collider2D))]
    public sealed class LorePaper : MonoBehaviour
    {
        [SerializeField, TextArea] private string message;

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;

        public void SetMessage(string value) => message = value;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerHealth>() == null) return;
            if (PaperMessageBox.Instance == null || string.IsNullOrEmpty(message)) return;
            PaperMessageBox.Instance.Show(message);
            RuntimeSfx.Play(RuntimeSfx.Sound.Pickup, .6f);
        }
    }
}
