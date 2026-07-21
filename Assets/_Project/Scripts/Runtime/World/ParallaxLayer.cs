using UnityEngine;

namespace BurnOut.World
{
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float factor = .2f;
        private Vector3 initialCameraPosition;
        private Vector3 initialPosition;
        private void Start()
        {
            cameraTransform ??= Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform == null) { enabled = false; return; }
            initialCameraPosition = cameraTransform.position;
            initialPosition = transform.position;
        }
        private void LateUpdate() => transform.position = initialPosition + (cameraTransform.position - initialCameraPosition) * factor;
    }
}
