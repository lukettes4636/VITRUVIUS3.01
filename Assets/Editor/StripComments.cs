using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class StripComments
{
    [InitializeOnLoadMethod]
    private static void AutoRun()
    {
        TryStripAll();
    }

    public static void StripAllMenu()
    {
        TryStripAll();
    }

    private static void TryStripAll()
    {
        string root = Path.Combine(Application.dataPath, "scripts");
        if (!Directory.Exists(root)) return;
        string[] files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            try
            {
                string text = File.ReadAllText(f, Encoding.UTF8);
                string stripped = Strip(text);
                if (!string.Equals(text, stripped, StringComparison.Ordinal))
                {
                    File.WriteAllText(f, stripped, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"StripComments failed for {f}: {e.Message}");
            }
        }
        AssetDatabase.Refresh();
    }

    private static string Strip(string s)
    {
        var sb = new StringBuilder(s.Length);
        bool inLine = false;
        bool inBlock = false;
        bool inStr = false;
        bool inVerbatim = false;
        bool inChar = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            char n = i + 1 < s.Length ? s[i + 1] : '\0';
            if (inLine)
            {
                if (c == '\n')
                {
                    inLine = false;
                    sb.Append(c);
                }
                else if (c == '\r')
                {
                    inLine = false;
                    sb.Append(c);
                }
                continue;
            }
            if (inBlock)
            {
                if (c == '*' && n == '/')
                {
                    inBlock = false;
                    i++;
                }
                continue;
            }
            if (inStr)
            {
                if (c == '\\')
                {
                    sb.Append(c);
                    if (i + 1 < s.Length) { sb.Append(s[i + 1]); i++; }
                    continue;
                }
                if (c == '"')
                {
                    inStr = false;
                    sb.Append(c);
                    continue;
                }
                sb.Append(c);
                continue;
            }
            if (inVerbatim)
            {
                if (c == '"')
                {
                    if (n == '"') { sb.Append('"'); sb.Append('"'); i++; continue; }
                    inVerbatim = false; sb.Append(c); continue;
                }
                sb.Append(c);
                continue;
            }
            if (inChar)
            {
                if (c == '\\')
                {
                    sb.Append(c);
                    if (i + 1 < s.Length) { sb.Append(s[i + 1]); i++; }
                    continue;
                }
                if (c == '\'') { inChar = false; sb.Append(c); continue; }
                sb.Append(c);
                continue;
            }
            if (c == '/' && n == '/') { inLine = true; i++; continue; }
            if (c == '/' && n == '*') { inBlock = true; i++; continue; }
            if (c == '@' && n == '"') { inVerbatim = true; sb.Append('@'); sb.Append('"'); i++; continue; }
            if (c == '"') { inStr = true; sb.Append(c); continue; }
            if (c == '\'') { inChar = true; sb.Append(c); continue; }
            sb.Append(c);
        }
        return sb.ToString();
    }
}
