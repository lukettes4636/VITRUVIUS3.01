using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class DisableStaticBatching : EditorWindow
{
    [MenuItem("Tools/Disable Static Batching for Piso and Pared")]
    public static void ShowWindow()
    {
        GetWindow<DisableStaticBatching>("Disable Static Batching");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Disable Static Batching for Piso and Pared"))
        {
            DisableStaticBatchingForKeywords();
        }
    }

    public static void DisableStaticBatchingForKeywords()
    {
        // Find all GameObjects in the current scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<GameObject> objectsToModify = new List<GameObject>();

        foreach (GameObject go in allObjects)
        {
            string lowerName = go.name.ToLower();
            if (lowerName.Contains("piso") || lowerName.Contains("pared"))
            {
                objectsToModify.Add(go);
            }
        }

        // Find all prefabs in the project
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        List<GameObject> prefabsToModify = new List<GameObject>();

        foreach (string guid in allPrefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                string lowerName = prefab.name.ToLower();
                if (lowerName.Contains("piso") || lowerName.Contains("pared"))
                {
                    prefabsToModify.Add(prefab);
                }
            }
        }

        // Disable static batching for found GameObjects in scene
        foreach (GameObject go in objectsToModify)
        {
            if (GameObjectUtility.GetStaticEditorFlags(go).HasFlag(StaticEditorFlags.BatchingStatic))
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
                flags &= ~StaticEditorFlags.BatchingStatic; // Remove BatchingStatic flag
                GameObjectUtility.SetStaticEditorFlags(go, flags);
                Debug.Log($"Disabled static batching for scene object: {go.name}");
            }
        }

        // Disable static batching for found prefabs
        foreach (GameObject prefab in prefabsToModify)
        {
            // For prefabs, we need to modify the prefab asset directly
            // This requires checking if the prefab is already open for editing
            // or if it needs to be loaded and saved.
            // For simplicity, we'll assume direct modification is sufficient for now.
            // In a more robust solution, one might need to open the prefab in isolation.

            if (GameObjectUtility.GetStaticEditorFlags(prefab).HasFlag(StaticEditorFlags.BatchingStatic))
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(prefab);
                flags &= ~StaticEditorFlags.BatchingStatic; // Remove BatchingStatic flag
                GameObjectUtility.SetStaticEditorFlags(prefab, flags);
                EditorUtility.SetDirty(prefab); // Mark prefab as dirty to save changes
                Debug.Log($"Disabled static batching for prefab: {prefab.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Static batching disablement process completed.");
    }
}
