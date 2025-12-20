using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ZoneNarrativeEntry
{
    public string zoneID = "ZoneA";
    public string firstEnter_Player1 = "I'll scout ahead.";
    public string firstEnter_Player2 = "I'll check this area.";
    public string secondEnter_Player1WhenP2First = "You got here first. What's the status?";
    public string secondEnter_Player2WhenP1First = "I'll back you up.";
    public string bothPresentLine = "We're here. Stay sharp.";
}

[CreateAssetMenu(fileName = "ZoneNarrativeData", menuName = "Dialogue System/Zone Narrative Data")]
public class ZoneNarrativeData : ScriptableObject
{
    public List<ZoneNarrativeEntry> entries = new List<ZoneNarrativeEntry>();

    public ZoneNarrativeEntry GetEntry(string id)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] != null && entries[i].zoneID == id)
                return entries[i];
        }
        return null;
    }
}
