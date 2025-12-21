using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class StripDebugLogs
{
    [InitializeOnLoadMethod]
    private static void AutoRun()
    {
        Run();
    }

    [MenuItem("Tools/Cleanup/Strip Debug Logs")]
    private static void Run()
    {
        string root = Path.Combine(Application.dataPath, "scripts");
        if (!Directory.Exists(root)) return;
        string[] files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            try
            {
                string[] lines = File.ReadAllLines(f, Encoding.UTF8);
                bool changed = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.Contains("Debug.Log(") || line.Contains("Debug.LogWarning(") || line.Contains("Debug.LogError("))
                    {
                        lines[i] = string.Empty;
                        changed = true;
                    }
                }
                if (changed)
                {
                    File.WriteAllLines(f, lines, new UTF8Encoding(false));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"StripDebugLogs failed for {f}: {e.Message}");
            }
        }
        AssetDatabase.Refresh();
    }
}
