using UnityEditor;
using UnityEngine;

public class RenameToPrefabName : MonoBehaviour
{
    [MenuItem("GameObject/To Prefab Name", false, 1)]
    public static void RenameSelected()
    {
        Object obj = Selection.activeObject;
        GameObject go = (GameObject) obj;

        Rename(go.transform);
    }

    private static void Rename(Transform transform)
    {
        GameObject gameObject = transform.gameObject;
        GameObject prefabGO = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

        //If prefab, rename, else search among children
        if (prefabGO != null)
        {
            string prefabGOName = prefabGO.name;
            Debug.Log("Renaming " + gameObject.name + " to " + prefabGOName);
            gameObject.name = prefabGOName;
        }
        else
        {
            for (int i = 0, childCount = transform.childCount; i < childCount; i++)
            {
                Rename(transform.GetChild(i));
            }
        }
    }
}
