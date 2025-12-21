using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif





public class DialogueAutoReset : MonoBehaviour
{
    [Header("Configuracion de Reseteo")]
    [Tooltip("Arrastra aqui todos los ScriptableObjects de dialogo que quieras resetear automaticamente")]
    [SerializeField] private NPCDialogueData[] allDialogueData;

    [Header("Opciones")]
    [SerializeField] private bool resetOnPlayModeExit = true;
    [SerializeField] private bool showDebugMessages = true;

    private void Awake()
    {
        
        if (allDialogueData == null || allDialogueData.Length == 0)
        {
            allDialogueData = Resources.FindObjectsOfTypeAll<NPCDialogueData>();
            if (showDebugMessages && allDialogueData.Length > 0)
            {

            }
        }

#if UNITY_EDITOR
        
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        
        if (state == PlayModeStateChange.ExitingPlayMode && resetOnPlayModeExit)
        {
            ResetAllDialogues();
            if (showDebugMessages)
            {

            }
        }
    }
#endif

    
    
    
    public void ResetAllDialogues()
    {
        if (allDialogueData == null) return;

        foreach (var dialogue in allDialogueData)
        {
            if (dialogue != null)
            {
                dialogue.ResetAllDialogues();

#if UNITY_EDITOR
                
                EditorUtility.SetDirty(dialogue);
#endif
            }
        }

        
        if (NPCDialogueDataManager.Instance != null)
        {
            NPCDialogueDataManager.Instance.ResetAllData();
        }

#if UNITY_EDITOR
        
        AssetDatabase.SaveAssets();
#endif
    }

    
    
    
    [ContextMenu("Buscar Todos los Dialogos")]
    public void FindAllDialogues()
    {
#if UNITY_EDITOR
        allDialogueData = Resources.FindObjectsOfTypeAll<NPCDialogueData>();
        EditorUtility.SetDirty(this);

#endif
    }

    
    
    
    [ContextMenu("Resetear Todos los Dialogos Ahora")]
    public void ManualReset()
    {
        ResetAllDialogues();

    }
}
