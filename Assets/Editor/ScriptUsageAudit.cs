using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ScriptUsageAudit
{
    public static void Report()
    {
        var scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/scripts" });
        var allTypes = new HashSet<Type>();
        foreach (var guid in scriptGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            var t = ms != null ? ms.GetClass() : null;
            if (t != null && typeof(MonoBehaviour).IsAssignableFrom(t)) allTypes.Add(t);
        }
        var usedTypes = new HashSet<Type>();
        int missingCount = 0;
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var comps = go.GetComponentsInChildren<Component>(true);
            foreach (var c in comps)
            {
                if (c == null) { missingCount++; continue; }
                var t = c.GetType();
                if (typeof(MonoBehaviour).IsAssignableFrom(t)) usedTypes.Add(t);
            }
        }
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        var opened = new List<string>();
        foreach (var guid in sceneGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            opened.Add(path);
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                var comps = go.GetComponentsInChildren<Component>(true);
                foreach (var c in comps)
                {
                    if (c == null) { missingCount++; continue; }
                    var t = c.GetType();
                    if (typeof(MonoBehaviour).IsAssignableFrom(t)) usedTypes.Add(t);
                }
            }
            EditorSceneManager.CloseScene(scene, true);
        }
        var unused = allTypes.Except(usedTypes).Select(x => x.FullName).OrderBy(x => x).ToList();
        Debug.Log($"ScriptUsageAudit: total={allTypes.Count} used={usedTypes.Count} unused={unused.Count} missing={missingCount}");
        foreach (var u in unused) Debug.Log($"Unused: {u}");
    }
}
