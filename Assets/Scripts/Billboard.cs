using UnityEngine;

namespace TCGE
{
    public sealed class Billboard : MonoBehaviour
    {
        [SerializeField]
        Transform mainCamera;

        [SerializeField]
        bool invert;

        void LateUpdate()
        {
            TransformHandle handle = transformHandle;
            Vector3 lookDir = mainCamera.position - handle.position;
            lookDir.y = 0;
            handle.forward = invert ? -lookDir : lookDir;
        }
    }
}
