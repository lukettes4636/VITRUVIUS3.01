using System.Collections.Generic;
using UnityEngine;




public class NPCDialogueDataManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NPCDialogueData dialogueData;

    
    private Dictionary<string, int> interactionCount = new Dictionary<string, int>();

    
    private Dictionary<string, HashSet<string>> playerFlags = new Dictionary<string, HashSet<string>>();

    
    private Dictionary<string, CharacterDialogueSet> currentDialogueSet = new Dictionary<string, CharacterDialogueSet>();

    #region Public API

    
    
    
    public List<DialogueNode> GetDialogueForPlayer(string playerTag)
    {
        if (dialogueData == null)
        {

            return new List<DialogueNode>();
        }

        
        CharacterDialogueSet dialogueSet = dialogueData.GetDialogueForPlayer(playerTag, this);

        if (dialogueSet != null)
        {
            currentDialogueSet[playerTag] = dialogueSet;
            return dialogueSet.dialogueNodes;
        }

        
        if (dialogueData.followUpDialogue != null && dialogueData.followUpDialogue.Count > 0)
        {
            return dialogueData.followUpDialogue;
        }

        return new List<DialogueNode>();
    }

    
    
    
    public void CompleteCurrentDialogue(string playerTag)
    {
        
        IncrementInteractionCount(playerTag);

        
        if (currentDialogueSet.ContainsKey(playerTag) && currentDialogueSet[playerTag] != null)
        {
            dialogueData.CompleteDialogue(playerTag, currentDialogueSet[playerTag]);
        }
    }

    
    
    
    public int GetInteractionCount(string playerTag)
    {
        if (!interactionCount.ContainsKey(playerTag))
            return 0;

        return interactionCount[playerTag];
    }

    
    
    
    private void IncrementInteractionCount(string playerTag)
    {
        if (!interactionCount.ContainsKey(playerTag))
            interactionCount[playerTag] = 0;

        interactionCount[playerTag]++;
    }

    
    
    
    public void SetFlag(string playerTag, string flagName)
    {
        if (!playerFlags.ContainsKey(playerTag))
            playerFlags[playerTag] = new HashSet<string>();

        playerFlags[playerTag].Add(flagName);
    }

    
    
    
    public bool HasFlag(string playerTag, string flagName)
    {
        if (!playerFlags.ContainsKey(playerTag))
            return false;

        return playerFlags[playerTag].Contains(flagName);
    }

    
    
    
    public void RemoveFlag(string playerTag, string flagName)
    {
        if (playerFlags.ContainsKey(playerTag))
        {
            playerFlags[playerTag].Remove(flagName);
        }
    }

    
    
    
    public void ResetAllProgress()
    {
        interactionCount.Clear();
        playerFlags.Clear();
        currentDialogueSet.Clear();

        if (dialogueData != null)
            dialogueData.ResetAllDialogues();
    }

    
    
    
    public void ResetPlayerProgress(string playerTag)
    {
        if (interactionCount.ContainsKey(playerTag))
            interactionCount.Remove(playerTag);

        if (playerFlags.ContainsKey(playerTag))
            playerFlags.Remove(playerTag);

        if (currentDialogueSet.ContainsKey(playerTag))
            currentDialogueSet.Remove(playerTag);
    }

    
    
    
    public bool HasDialogueAvailable(string playerTag)
    {
        if (dialogueData == null)
            return false;

        CharacterDialogueSet dialogueSet = dialogueData.GetDialogueForPlayer(playerTag, this);

        if (dialogueSet != null)
            return true;

        
        return dialogueData.followUpDialogue != null && dialogueData.followUpDialogue.Count > 0;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (dialogueData == null)
        {

        }
    }

    #endregion

    #region Editor Helpers

    [ContextMenu("Reset All Progress")]
    private void ResetProgressMenuItem()
    {
        ResetAllProgress();
    }

    [ContextMenu("Show Current State")]
    private void ShowCurrentState()
    {


        foreach (var kvp in interactionCount)
        {

        }

        foreach (var kvp in playerFlags)
        {

        }
    }

    #endregion
}
