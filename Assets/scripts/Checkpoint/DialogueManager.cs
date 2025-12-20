using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    private bool dialogueActive = false;
    private int activeMessageCount = 0;
    [SerializeField] private PlayerDialogueData playerDialogueData;
    [SerializeField] private ZoneNarrativeData zoneNarrativeData;
    private HashSet<string> shownEnterFlags = new HashSet<string>();
    private Dictionary<string, int> firstEntrant = new Dictionary<string, int>();
    private HashSet<string> shownZoneBoth = new HashSet<string>();
    private PlayerHealth player1HealthRef;
    private PlayerHealth player2HealthRef;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void OnEnable()
    {
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        player1HealthRef = p1 != null ? p1.GetComponent<PlayerHealth>() : null;
        player2HealthRef = p2 != null ? p2.GetComponent<PlayerHealth>() : null;
        if (player1HealthRef != null) player1HealthRef.OnPlayerDied += OnPlayerDiedHandler;
        if (player2HealthRef != null) player2HealthRef.OnPlayerDied += OnPlayerDiedHandler;
    }

    void OnDisable()
    {
        if (player1HealthRef != null) player1HealthRef.OnPlayerDied -= OnPlayerDiedHandler;
        if (player2HealthRef != null) player2HealthRef.OnPlayerDied -= OnPlayerDiedHandler;
    }

    public static bool IsDialogueActive()
    {
        return instance != null && instance.dialogueActive;
    }

    public static void SetDialogueActive(bool active)
    {
        if (instance == null) return;
        instance.dialogueActive = active;
    }

    public static void ShowPlayerMessage(GameObject player, string message, float duration = 2f)
    {
        if (player == null) return;
        if (instance == null) return;

        PlayerPopupBillboard billboard = player.GetComponent<PlayerPopupBillboard>();
        if (billboard == null) billboard = player.GetComponentInChildren<PlayerPopupBillboard>();

        if (billboard != null)
        {
            instance.activeMessageCount++;
            instance.dialogueActive = true;
            billboard.ShowMessage(message, duration);
            instance.StartCoroutine(instance.DeactivateAfter(duration));
            return;
        }

        NPCPopupBillboard npcBillboard = player.GetComponent<NPCPopupBillboard>();
        if (npcBillboard == null) npcBillboard = player.GetComponentInChildren<NPCPopupBillboard>();

        if (npcBillboard != null)
        {
            instance.activeMessageCount++;
            instance.dialogueActive = true;
            npcBillboard.ShowMessage(message, duration);
            instance.StartCoroutine(instance.DeactivateAfter(duration));
            return;
        }
    }

    private IEnumerator DeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        activeMessageCount = Mathf.Max(0, activeMessageCount - 1);
        if (activeMessageCount == 0) dialogueActive = false;
    }

    public static GameObject FindPlayerByTag(string tag)
    {
        return GameObject.FindGameObjectWithTag(tag);
    }

    public static void ShowMessageForTag(string tag, string message, float duration = 2f)
    {
        GameObject player = FindPlayerByTag(tag);
        ShowPlayerMessage(player, message, duration);
    }

    private void OnPlayerDiedHandler(int deadId)
    {
        string survivorTag = deadId == 1 ? "Player2" : "Player1";
        GameObject survivor = FindPlayerByTag(survivorTag);
        if (survivor == null) return;
        if (playerDialogueData == null) return;
        if (survivorTag == "Player1")
            ShowPlayerMessage(survivor, playerDialogueData.survivorAfterDeath_Player1, 2.5f);
        else
            ShowPlayerMessage(survivor, playerDialogueData.survivorAfterDeath_Player2, 2.5f);
    }

    public static void ShowItemCollectionDialogue(GameObject collector, string itemID, bool isKeyCard)
    {
        if (collector == null) return;
        if (instance == null) return;

        PlayerIdentifier collectorId = collector.GetComponent<PlayerIdentifier>();
        PlayerInventory collectorInv = collector.GetComponent<PlayerInventory>();
        if (collectorId == null || collectorInv == null) return;

        PlayerIdentifier[] players = GameObject.FindObjectsOfType<PlayerIdentifier>();
        GameObject other = null;
        foreach (var p in players)
        {
            if (p.playerID != collectorId.playerID)
            {
                other = p.gameObject;
                break;
            }
        }

        PlayerInventory otherInv = other != null ? other.GetComponent<PlayerInventory>() : null;

        if (itemID == "Flashlight")
        {
            bool otherHasFlashlight = otherInv != null && otherInv.HasItem("Flashlight");

            if (!otherHasFlashlight && other != null)
            {
                if (instance.playerDialogueData != null)
                {
                    if (collectorId.playerID == 1)
                        ShowPlayerMessage(other, instance.playerDialogueData.flashlightOtherReact_Player2, 2.5f);
                    else
                        ShowPlayerMessage(other, instance.playerDialogueData.flashlightOtherReact_Player1, 2.5f);
                }
                else
                {
                    ShowPlayerMessage(other, "I still need a flashlight.", 2.5f);
                }
            }
            else if (otherHasFlashlight && other != null)
            {
                if (instance.playerDialogueData != null)
                {
                    if (collectorId.playerID == 2)
                        ShowPlayerMessage(other, instance.playerDialogueData.flashlightBothHave_Player2WasLast_Player1Teasing, 2.5f);
                    else
                        ShowPlayerMessage(other, instance.playerDialogueData.flashlightBothHave_Player1WasLast_Player2Encouraging, 2.5f);
                }
                else
                {
                    ShowPlayerMessage(other, "Now we both have light.", 2.5f);
                }
            }

            return;
        }

        if (isKeyCard)
        {
            if (instance.playerDialogueData != null)
            {
                if (collectorId.playerID == 1)
                {
                    ShowPlayerMessage(collector, instance.playerDialogueData.keyCardCollector_Player1, 2.5f);
                    if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.keyCardOtherReact_Player2OnP1, 2.5f);
                }
                else
                {
                    ShowPlayerMessage(collector, instance.playerDialogueData.keyCardCollector_Player2, 2.5f);
                    if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.keyCardOtherReact_Player1OnP2, 2.5f);
                }
            }
            else
            {
                ShowPlayerMessage(collector, "Keycard acquired.", 2.5f);
                if (other != null) ShowPlayerMessage(other, "Copy that.", 2.5f);
            }

            return;
        }

        if (itemID == "Lever")
        {
            if (instance.playerDialogueData != null)
            {
                if (collectorId.playerID == 1)
                {
                    ShowPlayerMessage(collector, instance.playerDialogueData.leverCollector_Player1, 2.5f);
                    if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.leverOtherReact_Player2OnP1, 2.5f);
                }
                else
                {
                    ShowPlayerMessage(collector, instance.playerDialogueData.leverCollector_Player2, 2.5f);
                    if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.leverOtherReact_Player1OnP2, 2.5f);
                }
            }
            else
            {
                ShowPlayerMessage(collector, "Lever picked.", 2.5f);
                if (other != null) ShowPlayerMessage(other, "Use it when ready.", 2.5f);
            }

            return;
        }

        if (itemID == "Key")
        {
            if (instance.playerDialogueData != null)
            {
                if (collectorId.playerID == 1)
                {
                    ShowPlayerMessage(collector, instance.playerDialogueData.keyCollector_Player1, 2.5f);
                    if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.keyOtherReact_Player2OnP1, 2.5f);
                }
                else
                {
                    ShowPlayerMessage(collector, instance.playerDialogueData.keyCollector_Player2, 2.5f);
                    if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.keyOtherReact_Player1OnP2, 2.5f);
                }
            }
            else
            {
                ShowPlayerMessage(collector, "Key found.", 2.5f);
                if (other != null) ShowPlayerMessage(other, "That will help.", 2.5f);
            }

            return;
        }

        ShowPlayerMessage(collector, "Item collected.", 2.5f);
        if (other != null) ShowPlayerMessage(other, "Roger.", 2.5f);
    }

public static void ShowMonitorEnterDialogue(string monitorID, string monitorKind, GameObject entrant)
{
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        string keyBase = "monitor:" + monitorKind + ":" + monitorID;

        int first;
        if (!instance.firstEntrant.TryGetValue(keyBase, out first))
        {
            instance.firstEntrant[keyBase] = id.playerID;
            if (instance.playerDialogueData != null)
            {
                if (monitorKind == "Computer")
                    ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.monitorComputer_First_Player1 : instance.playerDialogueData.monitorComputer_First_Player2, 2.5f);
                else
                    ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.monitorNote_First_Player1 : instance.playerDialogueData.monitorNote_First_Player2, 2.5f);
            }
            else
            {
                ShowPlayerMessage(entrant, "Let's check this.", 2.5f);
            }
        }
        else
        {
            int otherWasFirst = first;
            if (instance.playerDialogueData != null)
            {
                if (monitorKind == "Computer")
                {
                    if (id.playerID == otherWasFirst)
                        ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.monitorComputer_First_Player1 : instance.playerDialogueData.monitorComputer_First_Player2, 2.5f);
                    else if (id.playerID == 1)
                        ShowPlayerMessage(entrant, instance.playerDialogueData.monitorComputer_Second_Player1WhenP2First, 2.5f);
                    else
                        ShowPlayerMessage(entrant, instance.playerDialogueData.monitorComputer_Second_Player2WhenP1First, 2.5f);
                }
                else
                {
                    if (id.playerID == otherWasFirst)
                        ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.monitorNote_First_Player1 : instance.playerDialogueData.monitorNote_First_Player2, 2.5f);
                    else if (id.playerID == 1)
                        ShowPlayerMessage(entrant, instance.playerDialogueData.monitorNote_Second_Player1WhenP2First, 2.5f);
                    else
                        ShowPlayerMessage(entrant, instance.playerDialogueData.monitorNote_Second_Player2WhenP1First, 2.5f);
                }
            }
        }
}

public static void ShowKeypadEnterDialogue(string doorID, GameObject entrant)
{
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        string keyBase = "keypad:" + doorID;

        int first;
        if (!instance.firstEntrant.TryGetValue(keyBase, out first))
        {
            instance.firstEntrant[keyBase] = id.playerID;
            if (instance.playerDialogueData != null)
                ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.keypadEnter_First_Player1 : instance.playerDialogueData.keypadEnter_First_Player2, 2.5f);
            else
                ShowPlayerMessage(entrant, "Approaching keypad.", 2.5f);
        }
        else
        {
            int otherWasFirst = first;
            if (instance.playerDialogueData != null)
            {
                if (id.playerID == otherWasFirst)
                    ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.keypadEnter_First_Player1 : instance.playerDialogueData.keypadEnter_First_Player2, 2.5f);
                else if (id.playerID == 1)
                    ShowPlayerMessage(entrant, instance.playerDialogueData.keypadEnter_Second_Player1WhenP2First, 2.5f);
                else
                    ShowPlayerMessage(entrant, instance.playerDialogueData.keypadEnter_Second_Player2WhenP1First, 2.5f);
            }
        }
}

    public static void ShowKeypadCodeResultDialogue(GameObject actor, bool success)
    {
        if (instance == null || actor == null) return;
        PlayerIdentifier actorId = actor.GetComponent<PlayerIdentifier>();
        if (actorId == null) return;
        string otherTag = actorId.playerID == 1 ? "Player2" : "Player1";
        GameObject other = FindPlayerByTag(otherTag);
        if (other == null) return;
        if (instance.playerDialogueData != null)
        {
            if (!success)
            {
                if (actorId.playerID == 1)
                    ShowPlayerMessage(other, instance.playerDialogueData.keypadCode_Incorrect_OtherReact_Player2OnP1Attempt, 2.5f);
                else
                    ShowPlayerMessage(other, instance.playerDialogueData.keypadCode_Incorrect_OtherReact_Player1OnP2Attempt, 2.5f);
            }
            else
            {
                if (actorId.playerID == 1)
                    ShowPlayerMessage(other, instance.playerDialogueData.keypadCode_Correct_OtherReact_Player2OnP1Attempt, 2.5f);
                else
                    ShowPlayerMessage(other, instance.playerDialogueData.keypadCode_Correct_OtherReact_Player1OnP2Attempt, 2.5f);
            }
        }
    }

    public static void ShowWarningDoorEnterDialogue(GameObject entrant)
    {
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (instance.playerDialogueData != null)
        {
            ShowPlayerMessage(entrant, id.playerID == 1 ? instance.playerDialogueData.warningDoor_Enter_Player1 : instance.playerDialogueData.warningDoor_Enter_Player2, 2.5f);
        }
        else
        {
            ShowPlayerMessage(entrant, "Barrier ahead.", 2.5f);
        }
    }

    public static void ShowElectricBoxEnterDialogue(GameObject entrant, bool hasLever)
    {
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (instance.playerDialogueData != null)
        {
            if (id.playerID == 1)
                ShowPlayerMessage(entrant, hasLever ? instance.playerDialogueData.electricBox_Enter_HasLever_Player1 : instance.playerDialogueData.electricBox_Enter_NoLever_Player1, 2.5f);
            else
                ShowPlayerMessage(entrant, hasLever ? instance.playerDialogueData.electricBox_Enter_HasLever_Player2 : instance.playerDialogueData.electricBox_Enter_NoLever_Player2, 2.5f);
        }
        else
        {
            ShowPlayerMessage(entrant, hasLever ? "I have the lever part." : "Missing lever part.", 2.5f);
        }
    }

    public static void ShowKeyDoorEnterDialogue(GameObject entrant, bool hasKey)
    {
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (instance.playerDialogueData != null)
        {
            if (id.playerID == 1)
                ShowPlayerMessage(entrant, hasKey ? instance.playerDialogueData.keyDoor_Enter_HasKey_Player1 : instance.playerDialogueData.keyDoor_Enter_NoKey_Player1, 2.5f);
            else
                ShowPlayerMessage(entrant, hasKey ? instance.playerDialogueData.keyDoor_Enter_HasKey_Player2 : instance.playerDialogueData.keyDoor_Enter_NoKey_Player2, 2.5f);
        }
        else
        {
            ShowPlayerMessage(entrant, hasKey ? "I have the key." : "Need the key.", 2.5f);
        }
    }

    public static void ShowDoubleDoorSingleEnter(GameObject entrant)
    {
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        GameObject other = FindPlayerByTag(id.playerID == 1 ? "Player2" : "Player1");
        if (instance.playerDialogueData != null)
        {
            if (id.playerID == 1)
            {
                ShowPlayerMessage(entrant, instance.playerDialogueData.doubleDoor_SingleEnter_Player1, 2.5f);
                if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.doubleDoor_SingleEnter_Response_Player2OnP1, 2.5f);
            }
            else
            {
                ShowPlayerMessage(entrant, instance.playerDialogueData.doubleDoor_SingleEnter_Player2, 2.5f);
                if (other != null) ShowPlayerMessage(other, instance.playerDialogueData.doubleDoor_SingleEnter_Response_Player1OnP2, 2.5f);
            }
        }
        else
        {
            ShowPlayerMessage(entrant, "I need help with this door!", 2.5f);
            if (other != null) ShowPlayerMessage(other, "On my way.", 2.5f);
        }
    }

    public static void ShowDoubleDoorBothReady()
    {
        if (instance == null) return;
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        if (instance.playerDialogueData != null)
        {
            if (p1 != null) ShowPlayerMessage(p1, instance.playerDialogueData.doubleDoor_BothReady_Player1, 2.5f);
            if (p2 != null) ShowPlayerMessage(p2, instance.playerDialogueData.doubleDoor_BothReady_Player2, 2.5f);
        }
        else
        {
            if (p1 != null) ShowPlayerMessage(p1, "Ready. Hit together.", 2.5f);
            if (p2 != null) ShowPlayerMessage(p2, "Ready here. Together.", 2.5f);
        }
    }

    public static void ShowDoubleDoorFinalHitApproach()
    {
        if (instance == null) return;
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        if (instance.playerDialogueData != null)
        {
            if (p1 != null) ShowPlayerMessage(p1, instance.playerDialogueData.doubleDoor_FinalHitApproach_Player1, 2.5f);
            if (p2 != null) ShowPlayerMessage(p2, instance.playerDialogueData.doubleDoor_FinalHitApproach_Player2, 2.5f);
        }
        else
        {
            if (p1 != null) ShowPlayerMessage(p1, "One more. Together.", 2.5f);
            if (p2 != null) ShowPlayerMessage(p2, "Last hit. Go.", 2.5f);
        }
    }

    public static void ShowDoubleDoorOpened()
    {
        if (instance == null) return;
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        if (instance.playerDialogueData != null)
        {
            if (p1 != null) ShowPlayerMessage(p1, instance.playerDialogueData.doubleDoor_Opened_Player1, 2.5f);
            if (p2 != null) ShowPlayerMessage(p2, instance.playerDialogueData.doubleDoor_Opened_Player2, 2.5f);
        }
        else
        {
            if (p1 != null) ShowPlayerMessage(p1, "Door opened!", 2.5f);
            if (p2 != null) ShowPlayerMessage(p2, "It's open!", 2.5f);
        }
    }

public static void ShowZoneNarrativeEnter(string zoneID, GameObject entrant)
{
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        string keyBase = "zone:" + zoneID;

        int first;
        ZoneNarrativeEntry entry = instance.zoneNarrativeData != null ? instance.zoneNarrativeData.GetEntry(zoneID) : null;
        if (!instance.firstEntrant.TryGetValue(keyBase, out first))
        {
            instance.firstEntrant[keyBase] = id.playerID;
            if (entry != null)
                ShowPlayerMessage(entrant, id.playerID == 1 ? entry.firstEnter_Player1 : entry.firstEnter_Player2, 2.5f);
            else
                ShowPlayerMessage(entrant, "Entering area.", 2.5f);
        }
        else
        {
            int otherWasFirst = first;
            if (entry != null)
            {
                if (id.playerID == otherWasFirst)
                    ShowPlayerMessage(entrant, id.playerID == 1 ? entry.firstEnter_Player1 : entry.firstEnter_Player2, 2.5f);
                else if (id.playerID == 1)
                    ShowPlayerMessage(entrant, entry.secondEnter_Player1WhenP2First, 2.5f);
                else
                    ShowPlayerMessage(entrant, entry.secondEnter_Player2WhenP1First, 2.5f);
            }
            else
            {
                ShowPlayerMessage(entrant, "Entering area.", 2.5f);
            }
        }
}

    public static void ShowZoneNarrativeBoth(string zoneID)
    {
        if (instance == null) return;
        string key = "zone:" + zoneID + ":both";
        if (instance.shownZoneBoth.Contains(key)) return;
        instance.shownZoneBoth.Add(key);
        ZoneNarrativeEntry entry = instance.zoneNarrativeData != null ? instance.zoneNarrativeData.GetEntry(zoneID) : null;
        if (entry == null) return;
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        if (p1 != null) ShowPlayerMessage(p1, entry.bothPresentLine, 2.5f);
        if (p2 != null) ShowPlayerMessage(p2, entry.bothPresentLine, 2.5f);
    }

    public static void ShowEnemyWallBreakDialogue()
    {
        if (instance == null) return;
        if (instance.playerDialogueData == null) return;
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        if (p1 != null) ShowPlayerMessage(p1, instance.playerDialogueData.enemyBreakWall_Player1, 2.5f);
        if (p2 != null) ShowPlayerMessage(p2, instance.playerDialogueData.enemyBreakWall_Player2, 2.5f);
    }

    public static void ShowEnemyChaseEndedDialogue()
    {
        if (instance == null) return;
        if (instance.playerDialogueData == null) return;
        GameObject p1 = FindPlayerByTag("Player1");
        GameObject p2 = FindPlayerByTag("Player2");
        if (p1 != null) ShowPlayerMessage(p1, instance.playerDialogueData.enemyChaseEnded_Player1, 2.5f);
        if (p2 != null) ShowPlayerMessage(p2, instance.playerDialogueData.enemyChaseEnded_Player2, 2.5f);
    }

    public static void ShowEnemyDetectedAgainDialogue(GameObject detectedPlayer)
    {
        if (instance == null || detectedPlayer == null) return;
        if (instance.playerDialogueData == null) return;
        PlayerIdentifier id = detectedPlayer.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (id.playerID == 1)
            ShowPlayerMessage(detectedPlayer, instance.playerDialogueData.enemyDetectedAgain_Player1, 2.5f);
        else
            ShowPlayerMessage(detectedPlayer, instance.playerDialogueData.enemyDetectedAgain_Player2, 2.5f);
    }

public static void ShowFallenDoorEnter(string doorID, GameObject entrant)
{
        if (instance == null || entrant == null) return;
        PlayerIdentifier id = entrant.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (instance.playerDialogueData == null) return;
        if (id.playerID == 1)
            ShowPlayerMessage(entrant, instance.playerDialogueData.fallenDoor_Enter_Player1_Confident, 2.5f);
        else
            ShowPlayerMessage(entrant, instance.playerDialogueData.fallenDoor_Enter_Player2_Attempt, 2.5f);
}

    public static void ShowFallenDoorLiftFail(string doorID, GameObject actor)
    {
        if (instance == null || actor == null) return;
        if (instance.playerDialogueData == null) return;
        PlayerIdentifier id = actor.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (id.playerID == 2)
        {
            ShowPlayerMessage(actor, instance.playerDialogueData.fallenDoor_LiftFail_Player2, 2.5f);
            GameObject p1 = FindPlayerByTag("Player1");
            if (p1 != null) ShowPlayerMessage(p1, instance.playerDialogueData.fallenDoor_LiftFail_Response_Player1, 2.5f);
        }
    }

    public static void ShowFallenDoorLiftSuccess(string doorID)
    {
        if (instance == null) return;
        if (instance.playerDialogueData == null) return;
        GameObject p2 = FindPlayerByTag("Player2");
        if (p2 != null) ShowPlayerMessage(p2, instance.playerDialogueData.fallenDoor_LiftSuccess_Player2, 2.5f);
    }

    public static void ShowFallenDoorEarlyRelease(string doorID, GameObject actor)
    {
        if (instance == null || actor == null) return;
        if (instance.playerDialogueData == null) return;
        PlayerIdentifier id = actor.GetComponent<PlayerIdentifier>();
        if (id == null) return;
        if (id.playerID == 1)
            ShowPlayerMessage(actor, instance.playerDialogueData.fallenDoor_EarlyRelease_Player1, 2.5f);
        else
            ShowPlayerMessage(actor, instance.playerDialogueData.fallenDoor_EarlyRelease_Player2, 2.5f);
    }
}
