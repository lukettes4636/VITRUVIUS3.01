using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueCondition
{
    public enum ConditionType
    {
        None,
        HasCompletedQuest,
        HasItem,
        PlayerSpecific,
        MinimumInteractions,
        CustomFlag
    }

    public ConditionType type = ConditionType.None;
    public string conditionValue = "";
    public int minimumCount = 1;
    public string specificPlayerTag = "";

    public bool IsMet(string playerTag, NPCDialogueDataManager dataManager)
    {
        switch (type)
        {
            case ConditionType.None:
                return true;

            case ConditionType.PlayerSpecific:
                return playerTag == specificPlayerTag;

            case ConditionType.MinimumInteractions:
                return dataManager.GetInteractionCount(playerTag) >= minimumCount;

            case ConditionType.CustomFlag:
                return dataManager.HasFlag(playerTag, conditionValue);

            case ConditionType.HasCompletedQuest:
            case ConditionType.HasItem:
                return true;

            default:
                return true;
        }
    }
}

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public int nextNodeIndex;

    [Tooltip("Escribe aqui el nombre de la Flag que se activara al elegir esta opcion (ej: NPC_Follows_P1)")]
    public string flagToTrigger = "";
}

[System.Serializable]
public class DialogueNode
{
    public bool isNPC = true;
    [TextArea(3, 10)] public string line;
    public List<DialogueOption> options = new List<DialogueOption>();
}

[System.Serializable]
public class CharacterDialogueSet
{
    [Header("Identificacion")]
    public string characterTag = "Player1";
    public string setName = "Primera Conversacion";

    [Header("Condiciones")]
    public List<DialogueCondition> conditions = new List<DialogueCondition>();

    [Header("Dialogo")]
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();

    [Header("Configuracion")]
    public bool oneTimeOnly = false;
    public bool markAsCompleted = true;

    [HideInInspector]
    public bool hasBeenShown = false;

    public bool CanShow(string playerTag, NPCDialogueDataManager dataManager)
    {
        if (playerTag != characterTag) return false;
        if (oneTimeOnly && hasBeenShown) return false;

        foreach (var condition in conditions)
        {
            if (!condition.IsMet(playerTag, dataManager)) return false;
        }
        return true;
    }
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "Dialogue System/NPC Dialogue Data")]
public class NPCDialogueData : ScriptableObject
{
    [Header("Informacion del NPC")]
    public string npcName = "NPC";
    public string npcDescription = "";

    [Header("Dialogos por Personaje")]
    public List<CharacterDialogueSet> player1Dialogues = new List<CharacterDialogueSet>();
    public List<CharacterDialogueSet> player2Dialogues = new List<CharacterDialogueSet>();

    [Header("Dialogo de Seguimiento (Follow-up)")]
    public List<DialogueNode> followUpDialogue = new List<DialogueNode>();

    public CharacterDialogueSet GetDialogueForPlayer(string playerTag, NPCDialogueDataManager dataManager)
    {
        List<CharacterDialogueSet> relevantDialogues = playerTag == "Player1" ? player1Dialogues : player2Dialogues;
        foreach (var dialogueSet in relevantDialogues)
        {
            if (dialogueSet.CanShow(playerTag, dataManager))
            {
                return dialogueSet;
            }
        }
        return null;
    }

    public void CompleteDialogue(string playerTag, CharacterDialogueSet dialogueSet)
    {
        if (dialogueSet != null && dialogueSet.markAsCompleted)
        {
            dialogueSet.hasBeenShown = true;
        }
    }

    public void ResetAllDialogues()
    {
        foreach (var dialogue in player1Dialogues) dialogue.hasBeenShown = false;
        foreach (var dialogue in player2Dialogues) dialogue.hasBeenShown = false;
    }
}