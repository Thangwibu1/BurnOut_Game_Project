using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace BurnOut.UI
{
    [RequireComponent(typeof(EventSystem), typeof(InputSystemUIInputModule))]
    public sealed class InputSystemUiSetup : MonoBehaviour
    {
        private InputSystemUIInputModule inputModule;

        private void Awake()
        {
            inputModule = GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
    }
}
