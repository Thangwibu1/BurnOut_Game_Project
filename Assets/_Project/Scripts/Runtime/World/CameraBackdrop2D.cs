using UnityEngine;

namespace BurnOut.World
{
    /// <summary>Centers a full-screen painted backdrop on the active camera so no uncovered void appears while travelling.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CameraBackdrop2D : MonoBehaviour
    {
        [SerializeField] private Vector2 offset;
        [SerializeField] private float cameraInfluence = 1f;
        private Camera sceneCamera;

        private void LateUpdate()
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            if (sceneCamera == null) return;
            var cameraPosition = sceneCamera.transform.position;
            transform.position = new Vector3(cameraPosition.x * cameraInfluence + offset.x, cameraPosition.y * cameraInfluence + offset.y, transform.position.z);
        }
    }
}
