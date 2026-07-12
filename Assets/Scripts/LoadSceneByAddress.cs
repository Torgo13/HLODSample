using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace TCGE
{
    internal sealed class LoadSceneByAddress : MonoBehaviour
    {
#if USING_ADDRESSABLES
        public string key = string.Empty; // address string
        private AsyncOperationHandle<SceneInstance> loadHandle;

        void Start()
        {
            loadHandle = Addressables.LoadSceneAsync(key);
        }

#if ZERO
        void OnDestroy()
        {
            Addressables.UnloadSceneAsync(loadHandle);
        }
#endif // ZERO

        void OnGUI()
        {
            if (!loadHandle.IsDone)
                GUI.Label(new Rect(100, 100, 200, 100), $"{loadHandle.PercentComplete * 0.01f:P0}");
        }
#endif // USING_ADDRESSABLES
    }
}
