using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class was used to replace speed tree assets with prefabs.
/// It probably has no more use and should be removed before releasing
/// TODO: Remove
/// </summary>
[ExecuteInEditMode]
public class PrefabReplacer : MonoBehaviour
{
    public Transform root;
    public bool button;
    public List<GameObject> prefabs;
    
#if UNITY_EDITOR
    void Start()
    {
        button = false;
    }

    void Update()
    {
        if (button)
        {
            button = false;
            Replace();
        }
    }

    private void Replace()
    {
        for (int i = 0, childCount = root.childCount; i < childCount; i++)
        {
            Transform tree = root.GetChild(i);
            GameObject treeGameObject = tree.gameObject;

            if (GetPrefab(treeGameObject, out GameObject prefab))
            {
                tree.GetLocalPositionAndRotation(out var pos, out var rot);
                Transform newTree = ((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent: root)).transform;
                newTree.SetLocalPositionAndRotation(pos, rot);
                newTree.localScale = tree.localScale;

                DestroyImmediate(treeGameObject);
            }
        }
    }

    private bool GetPrefab(GameObject tree, out GameObject replacingPrefab)
    {
        string name = tree.name.Split('(')[0].Trim();
        replacingPrefab = prefabs.Find(go => string.Equals(name, go.name));

        if (replacingPrefab != null)
        {
            Debug.Log("Found prefab for " + name);
            return true;
        }
        
        return false;
    }
#endif // UNITY_EDITOR
}
