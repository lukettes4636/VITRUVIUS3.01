#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(NPCDialogueData))]
public class NPCDialogueDataEditor : Editor
{
    private NPCDialogueData dialogueData;
    private bool showPlayer1Dialogues = true;
    private bool showPlayer2Dialogues = true;
    private bool showFollowUp = true;

    private void OnEnable()
    {
        dialogueData = (NPCDialogueData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Sistema de Dialogos Individual por Personaje", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Configura los dialogos para cada jugador. Usa las condiciones para desbloquear conversaciones y las 'Flags' en las opciones para activar eventos.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        
        EditorGUILayout.LabelField("Informacion del NPC", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        dialogueData.npcName = EditorGUILayout.TextField("Nombre", dialogueData.npcName);
        dialogueData.npcDescription = EditorGUILayout.TextField("Descripcion", dialogueData.npcDescription);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(10);

        
        showPlayer1Dialogues = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayer1Dialogues, "Dialogos de Player 1");
        if (showPlayer1Dialogues)
        {
            EditorGUI.indentLevel++;
            DrawCharacterDialogueList(dialogueData.player1Dialogues, "Player1");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        
        showPlayer2Dialogues = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayer2Dialogues, "Dialogos de Player 2");
        if (showPlayer2Dialogues)
        {
            EditorGUI.indentLevel++;
            DrawCharacterDialogueList(dialogueData.player2Dialogues, "Player2");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        
        showFollowUp = EditorGUILayout.BeginFoldoutHeaderGroup(showFollowUp, "Dialogo de Seguimiento (Default)");
        if (showFollowUp)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Este dialogo se muestra cuando un jugador ya completo todas sus conversaciones especificas.",
                MessageType.Info
            );
            DrawDialogueNodeList(dialogueData.followUpDialogue);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Resetear Estado de Todos los Dialogos"))
        {
            if (EditorUtility.DisplayDialog(
                "Resetear Dialogos",
                "Estas seguro de que quieres resetear el estado (visto/no visto) de todos los dialogos?",
                "Si", "Cancelar"))
            {
                dialogueData.ResetAllDialogues();
                EditorUtility.SetDirty(dialogueData);
            }
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dialogueData);
        }
    }

    
    
    

    private void DrawCharacterDialogueList(List<CharacterDialogueSet> dialogues, string playerTag)
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button($"+ Anadir Nuevo Dialogo para {playerTag}"))
        {
            CharacterDialogueSet newSet = new CharacterDialogueSet
            {
                characterTag = playerTag,
                setName = $"Dialogo {dialogues.Count + 1}",
                dialogueNodes = new List<DialogueNode>()
            };
            dialogues.Add(newSet);
            EditorUtility.SetDirty(dialogueData);
        }

        EditorGUILayout.Space(5);

        for (int i = 0; i < dialogues.Count; i++)
        {
            DrawCharacterDialogueSet(dialogues, i);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCharacterDialogueSet(List<CharacterDialogueSet> dialogues, int index)
    {
        CharacterDialogueSet dialogueSet = dialogues[index];

        EditorGUILayout.BeginVertical("helpbox");

        
        EditorGUILayout.BeginHorizontal();
        dialogueSet.setName = EditorGUILayout.TextField("Nombre del Set", dialogueSet.setName);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            if (EditorUtility.DisplayDialog(
                "Eliminar Dialogo",
                $"Eliminar '{dialogueSet.setName}'?",
                "Si", "Cancelar"))
            {
                dialogues.RemoveAt(index);
                EditorUtility.SetDirty(dialogueData);
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        
        dialogueSet.oneTimeOnly = EditorGUILayout.Toggle("Solo una vez", dialogueSet.oneTimeOnly);
        dialogueSet.markAsCompleted = EditorGUILayout.Toggle("Marcar completado", dialogueSet.markAsCompleted);

        if (dialogueSet.hasBeenShown)
        {
            EditorGUILayout.HelpBox("Este dialogo ya ha sido mostrado (Estado: Visto)", MessageType.Info);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Condiciones de Desbloqueo", EditorStyles.boldLabel);
        DrawConditionsList(dialogueSet.conditions);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Contenido del Dialogo", EditorStyles.boldLabel);
        DrawDialogueNodeList(dialogueSet.dialogueNodes);

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    
    
    

    private void DrawConditionsList(List<DialogueCondition> conditions)
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("+ Anadir Condicion"))
        {
            conditions.Add(new DialogueCondition());
            EditorUtility.SetDirty(dialogueData);
        }

        for (int i = 0; i < conditions.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            DrawCondition(conditions[i]);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                conditions.RemoveAt(i);
                EditorUtility.SetDirty(dialogueData);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCondition(DialogueCondition condition)
    {
        EditorGUILayout.BeginVertical();

        condition.type = (DialogueCondition.ConditionType)EditorGUILayout.EnumPopup("Tipo", condition.type);

        switch (condition.type)
        {
            case DialogueCondition.ConditionType.PlayerSpecific:
                condition.specificPlayerTag = EditorGUILayout.TextField("Tag Jugador", condition.specificPlayerTag);
                break;

            case DialogueCondition.ConditionType.MinimumInteractions:
                condition.minimumCount = EditorGUILayout.IntField("Minimo Interacciones", condition.minimumCount);
                break;

            case DialogueCondition.ConditionType.HasCompletedQuest:
            case DialogueCondition.ConditionType.HasItem:
            case DialogueCondition.ConditionType.CustomFlag:
                condition.conditionValue = EditorGUILayout.TextField("Valor / Flag", condition.conditionValue);
                break;
        }

        EditorGUILayout.EndVertical();
    }

    
    
    

    private void DrawDialogueNodeList(List<DialogueNode> nodes)
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("+ Anadir Nodo de Texto"))
        {
            nodes.Add(new DialogueNode());
            EditorUtility.SetDirty(dialogueData);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            DrawDialogueNode(nodes, i);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDialogueNode(List<DialogueNode> nodes, int index)
    {
        DialogueNode node = nodes[index];

        EditorGUILayout.BeginVertical("helpbox");

        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Nodo {index}", EditorStyles.boldLabel);

        if (GUILayout.Button("↑", GUILayout.Width(25)) && index > 0)
        {
            DialogueNode temp = nodes[index];
            nodes[index] = nodes[index - 1];
            nodes[index - 1] = temp;
            EditorUtility.SetDirty(dialogueData);
            return;
        }

        if (GUILayout.Button("↓", GUILayout.Width(25)) && index < nodes.Count - 1)
        {
            DialogueNode temp = nodes[index];
            nodes[index] = nodes[index + 1];
            nodes[index + 1] = temp;
            EditorUtility.SetDirty(dialogueData);
            return;
        }

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            nodes.RemoveAt(index);
            EditorUtility.SetDirty(dialogueData);
            return;
        }
        EditorGUILayout.EndHorizontal();

        
        node.isNPC = EditorGUILayout.Toggle("Habla el NPC", node.isNPC);
        EditorGUILayout.LabelField("Texto:");
        node.line = EditorGUILayout.TextArea(node.line, GUILayout.MinHeight(40));

        
        if (node.options == null)
            node.options = new List<DialogueOption>();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Opciones de Respuesta", EditorStyles.boldLabel);

        if (GUILayout.Button("+ Anadir Opcion"))
        {
            node.options.Add(new DialogueOption());
            EditorUtility.SetDirty(dialogueData);
        }

        for (int i = 0; i < node.options.Count; i++)
        {
            DrawDialogueOption(node.options, i, nodes.Count);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);
    }

    private void DrawDialogueOption(List<DialogueOption> options, int index, int maxNodeIndex)
    {
        DialogueOption option = options[index];

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.BeginVertical();

        
        option.optionText = EditorGUILayout.TextField($"Opcion {index + 1}", option.optionText);

        
        option.flagToTrigger = EditorGUILayout.TextField("Flag Evento (Opcional)", option.flagToTrigger);

        
        
        option.nextNodeIndex = EditorGUILayout.IntSlider("Ir a Nodo (-1=Fin)", option.nextNodeIndex, -1, maxNodeIndex - 1);

        EditorGUILayout.EndVertical();

        
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            options.RemoveAt(index);
            EditorUtility.SetDirty(dialogueData);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif