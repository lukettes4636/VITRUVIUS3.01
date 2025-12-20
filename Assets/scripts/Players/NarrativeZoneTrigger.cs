using UnityEngine;
using System.Collections.Generic;

public class NarrativeZoneTrigger : MonoBehaviour
{
    [SerializeField] private string zoneID = "ZoneA";
    [SerializeField] private bool requireBothPlayers = false;

    private HashSet<int> presentPlayers = new HashSet<int>();
    private bool bothFired = false;

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentifier id = other.GetComponent<PlayerIdentifier>();
        if (id == null) id = other.GetComponentInParent<PlayerIdentifier>();
        if (id == null) return;

        presentPlayers.Add(id.playerID);
        DialogueManager.ShowZoneNarrativeEnter(zoneID, id.gameObject);

        if (requireBothPlayers && presentPlayers.Count >= 2 && !bothFired)
        {
            DialogueManager.ShowZoneNarrativeBoth(zoneID);
            bothFired = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier id = other.GetComponent<PlayerIdentifier>();
        if (id == null) id = other.GetComponentInParent<PlayerIdentifier>();
        if (id == null) return;
        presentPlayers.Remove(id.playerID);
    }
}
