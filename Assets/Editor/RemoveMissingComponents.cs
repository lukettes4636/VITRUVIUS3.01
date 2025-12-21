using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RemoveMissingComponents
{
    [MenuItem("Tools/Cleanup/Remove Missing Components")]
    public static void Run()
    {
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            foreach (var go in scene.GetRootGameObjects())
                ScanGO(go);
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
        }

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            ScanGO(go);
            EditorUtility.SetDirty(go);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ScanGO(GameObject go)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform t in go.transform)
            ScanGO(t.gameObject);
    }
}
