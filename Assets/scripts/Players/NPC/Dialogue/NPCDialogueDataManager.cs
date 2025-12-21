using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueDataManager : MonoBehaviour
{
    [Header("Configuracin del NPC")]
    [SerializeField] private NPCDialogueData npcDialogueData;

    private Dictionary<string, int> interactionCounts = new Dictionary<string, int>();
    private Dictionary<string, HashSet<string>> flags = new Dictionary<string, HashSet<string>>();

    
    private CharacterDialogueSet currentDialogueSet;

    
    private static NPCDialogueDataManager instance;
    public static NPCDialogueDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NPCDialogueDataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DialogueDataManager");
                    instance = go.AddComponent<NPCDialogueDataManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        
        if (npcDialogueData == null)
        {

        }
    }

    #region Mtodos Pblicos - Usados por NPCEnhancedDialogueSystem

    
    
    
    public bool HasDialogueAvailable(string playerTag)
    {
        if (npcDialogueData == null) return false;

        
        CharacterDialogueSet dialogueSet = npcDialogueData.GetDialogueForPlayer(playerTag, this);
        if (dialogueSet != null) return true;

        
        return npcDialogueData.followUpDialogue != null && npcDialogueData.followUpDialogue.Count > 0;
    }

    
    
    
    public List<DialogueNode> GetDialogueForPlayer(string playerTag)
    {
        if (npcDialogueData == null) return new List<DialogueNode>();

        
        currentDialogueSet = npcDialogueData.GetDialogueForPlayer(playerTag, this);

        if (currentDialogueSet != null)
        {

            return currentDialogueSet.dialogueNodes;
        }

        
        if (npcDialogueData.followUpDialogue != null && npcDialogueData.followUpDialogue.Count > 0)
        {

            currentDialogueSet = null; 
            return npcDialogueData.followUpDialogue;
        }


        return new List<DialogueNode>();
    }

    
    
    
    public void CompleteCurrentDialogue(string playerTag)
    {
        if (npcDialogueData == null) return;

        
        if (currentDialogueSet != null)
        {
            npcDialogueData.CompleteDialogue(playerTag, currentDialogueSet);

        }

        
        IncrementInteractionCount(playerTag);


        currentDialogueSet = null;
    }

    #endregion

    #region Contadores de Interaccin

    public int GetInteractionCount(string playerTag)
    {
        return interactionCounts.ContainsKey(playerTag) ? interactionCounts[playerTag] : 0;
    }

    public void IncrementInteractionCount(string playerTag)
    {
        if (!interactionCounts.ContainsKey(playerTag))
            interactionCounts[playerTag] = 0;

        interactionCounts[playerTag]++;
    }

    #endregion

    #region Gestin de Flags

    public bool HasFlag(string playerTag, string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return false;

        if (!flags.ContainsKey(playerTag))
            return false;

        return flags[playerTag].Contains(flag);
    }

    public void SetFlag(string playerTag, string flag)
    {
        if (string.IsNullOrEmpty(flag))
            return;

        if (!flags.ContainsKey(playerTag))
            flags[playerTag] = new HashSet<string>();

        flags[playerTag].Add(flag);

    }

    #endregion

    #region Reset y Utilidades

    
    
    
    public void ResetAllData()
    {
        interactionCounts.Clear();
        flags.Clear();
        currentDialogueSet = null;

    }

    
    
    
    public void SetDialogueData(NPCDialogueData data)
    {
        npcDialogueData = data;
    }

    
    
    
    public NPCDialogueData GetDialogueData()
    {
        return npcDialogueData;
    }

    #endregion
}
