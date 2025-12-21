using System;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public static class SanitizeAscii
{
    [InitializeOnLoadMethod]
    private static void AutoRun()
    {
        Run();
    }

    [MenuItem("Tools/Cleanup/Sanitize ASCII in Scripts")]
    private static void RunMenu()
    {
        Run();
    }

    private static void Run()
    {
        string root = Path.Combine(Application.dataPath, "scripts");
        if (!Directory.Exists(root)) return;
        string[] files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            try
            {
                string text = File.ReadAllText(f, Encoding.UTF8);
                string cleaned = Clean(text);
                if (!string.Equals(text, cleaned, StringComparison.Ordinal))
                {
                    File.WriteAllText(f, cleaned, new UTF8Encoding(false));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SanitizeAscii failed for {f}: {e.Message}");
            }
        }
        AssetDatabase.Refresh();
    }

    private static string Clean(string s)
    {
        string x = s.Replace("\uFFFD", "");
        x = x.Replace("�", "");
        x = x.Replace("“", "\"").Replace("”", "\"");
        x = x.Replace("‘", "\'").Replace("’", "\'");
        x = x.Replace("–", "-").Replace("—", "-");
        x = x.Replace("¡", "").Replace("¿", "");
        string nfd = x.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(nfd.Length);
        for (int i = 0; i < nfd.Length; i++)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(nfd[i]);
            if (uc != UnicodeCategory.NonSpacingMark && uc != UnicodeCategory.SpacingCombiningMark && uc != UnicodeCategory.EnclosingMark)
            {
                sb.Append(nfd[i]);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
