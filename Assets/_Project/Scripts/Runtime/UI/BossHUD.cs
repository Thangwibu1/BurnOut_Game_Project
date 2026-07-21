using BurnOut.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace BurnOut.UI
{
    public sealed class BossHUD : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider healthBar;
        private EnemyHealth boss;
        public void Show(EnemyHealth target)
        {
            boss = target;
            if (panel != null) panel.SetActive(boss != null);
            if (boss != null) { boss.HealthChanged += Refresh; boss.Died += Hide; }
        }
        private void OnDestroy()
        {
            if (boss != null) { boss.HealthChanged -= Refresh; boss.Died -= Hide; }
        }
        private void Refresh(int current, int maximum) { if (healthBar != null) healthBar.value = maximum == 0 ? 0f : (float)current / maximum; }
        private void Hide() { if (panel != null) panel.SetActive(false); }
    }
}
