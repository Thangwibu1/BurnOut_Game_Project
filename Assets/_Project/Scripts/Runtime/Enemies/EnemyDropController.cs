using UnityEngine;

namespace BurnOut.Enemies
{
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemyDropController : MonoBehaviour
    {
        [SerializeField] private GameObject dropPrefab;
        private void Awake() => GetComponent<EnemyHealth>().Died += Drop;
        private void OnDestroy() { var health = GetComponent<EnemyHealth>(); if (health != null) health.Died -= Drop; }
        private void Drop() { if (dropPrefab != null) Instantiate(dropPrefab, transform.position, Quaternion.identity); }
    }
}
