using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public static class PromptVisualHelper
{
    public static Color ComputeColor(IList<PlayerIdentifier> players, Color cooperativeColor)
    {
        if (players == null || players.Count == 0) return Color.white;
        if (players.Count >= 2) return cooperativeColor;
        return players[0] != null ? players[0].PlayerOutlineColor : Color.white;
    }

    public static Color ComputeColor(HashSet<GameObject> players, Color cooperativeColor)
    {
        if (players == null || players.Count == 0) return Color.white;
        if (players.Count >= 2) return cooperativeColor;
        foreach (var obj in players)
        {
            var id = obj != null ? obj.GetComponent<PlayerIdentifier>() : null;
            if (id != null) return id.PlayerOutlineColor;
        }
        return Color.white;
    }

    public static void ApplyToPrompt(GameObject promptCanvas, Color color)
    {
        if (promptCanvas == null) return;
        var texts = promptCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            var t = texts[i];
            var m = t.fontMaterial != null ? t.fontMaterial : t.material;
            if (m != null)
            {
                if (m.HasProperty("_TextColor")) m.SetColor("_TextColor", color);
                else if (m.HasProperty("TextColor")) m.SetColor("TextColor", color);
                else if (m.HasProperty("_FaceColor")) m.SetColor("_FaceColor", color);
                else if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
                else if (m.HasProperty("_MainTex")) m.SetColor("_MainTex", color);
            }
            t.SetVerticesDirty();
        }
        var images = promptCanvas.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = color;
        }
    }
}

