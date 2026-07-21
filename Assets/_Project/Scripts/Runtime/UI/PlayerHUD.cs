using BurnOut.Player;
using UnityEngine;
using UnityEngine.UI;

namespace BurnOut.UI
{
    public sealed class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerSanity playerSanity;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider sanityBar;
        [SerializeField] private GameObject keyIcon;
        [SerializeField] private GameObject lowSanityOverlay;

        private void Start()
        {
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerSanity == null && playerHealth != null) playerSanity = playerHealth.GetComponent<PlayerSanity>();
            if (inventory == null && playerHealth != null) inventory = playerHealth.GetComponent<PlayerInventory>();
            if (playerHealth != null) playerHealth.HealthChanged += SetHealth;
            if (playerSanity != null) { playerSanity.SanityChanged += SetSanity; playerSanity.LowSanityChanged += SetLowSanity; }
            if (inventory != null) inventory.KeyStateChanged += SetKey;
            SetKey(false); SetLowSanity(false);
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.HealthChanged -= SetHealth;
            if (playerSanity != null) { playerSanity.SanityChanged -= SetSanity; playerSanity.LowSanityChanged -= SetLowSanity; }
            if (inventory != null) inventory.KeyStateChanged -= SetKey;
        }

        private void SetHealth(int current, int maximum) { if (healthBar != null) healthBar.value = maximum == 0 ? 0f : (float)current / maximum; }
        private void SetSanity(float current, float maximum) { if (sanityBar != null) sanityBar.value = maximum == 0f ? 0f : current / maximum; }
        private void SetKey(bool hasKey) { if (keyIcon != null) keyIcon.SetActive(hasKey); }
        private void SetLowSanity(bool low) { if (lowSanityOverlay != null) lowSanityOverlay.SetActive(low); }
    }
}
