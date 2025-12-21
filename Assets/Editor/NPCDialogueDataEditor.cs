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
        EditorGUILayout.LabelField("Sistema de Diálogos Individual por Personaje", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Configura los diálogos para cada jugador. Los diálogos se resetean automáticamente al salir del Play Mode si tienes el componente DialogueAutoReset en la escena.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // Información del NPC
        EditorGUILayout.LabelField("Información del NPC", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        dialogueData.npcName = EditorGUILayout.TextField("Nombre", dialogueData.npcName);
        dialogueData.npcDescription = EditorGUILayout.TextField("Descripción", dialogueData.npcDescription);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(10);

        // Diálogos Player 1
        showPlayer1Dialogues = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayer1Dialogues, "Diálogos de Player 1");
        if (showPlayer1Dialogues)
        {
            EditorGUI.indentLevel++;
            DrawCharacterDialogueList(dialogueData.player1Dialogues, "Player1");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Diálogos Player 2
        showPlayer2Dialogues = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayer2Dialogues, "Diálogos de Player 2");
        if (showPlayer2Dialogues)
        {
            EditorGUI.indentLevel++;
            DrawCharacterDialogueList(dialogueData.player2Dialogues, "Player2");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // Diálogo de Seguimiento
        showFollowUp = EditorGUILayout.BeginFoldoutHeaderGroup(showFollowUp, "Diálogo de Seguimiento (Default)");
        if (showFollowUp)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Este diálogo se muestra cuando un jugador ya completó todas sus conversaciones específicas.",
                MessageType.Info
            );
            DrawDialogueNodeList(dialogueData.followUpDialogue);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // Botones de utilidad
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Resetear Estado de Este Diálogo"))
        {
            if (EditorUtility.DisplayDialog(
                "Resetear Diálogo",
                "¿Estás seguro de que quieres resetear el estado (visto/no visto) de este diálogo?",
                "Sí", "Cancelar"))
            {
                dialogueData.ResetAllDialogues();
                EditorUtility.SetDirty(dialogueData);
                AssetDatabase.SaveAssets();
                Debug.Log($"Diálogo '{dialogueData.npcName}' reseteado");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Botón para buscar DialogueAutoReset
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Reseteo Automático", EditorStyles.boldLabel);

        DialogueAutoReset autoReset = FindObjectOfType<DialogueAutoReset>();

        if (autoReset == null)
        {
            EditorGUILayout.HelpBox(
                "No se encontró DialogueAutoReset en la escena. Este componente resetea automáticamente los diálogos al salir del Play Mode.",
                MessageType.Warning
            );

            if (GUILayout.Button("Crear DialogueAutoReset en la Escena"))
            {
                CreateDialogueAutoReset();
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "✓ DialogueAutoReset encontrado. Los diálogos se resetearán automáticamente al salir del Play Mode.",
                MessageType.Info
            );

            if (GUILayout.Button("Seleccionar DialogueAutoReset"))
            {
                Selection.activeGameObject = autoReset.gameObject;
            }
        }

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dialogueData);
        }
    }

    private void CreateDialogueAutoReset()
    {
        // Busca si ya existe un DialogueManager
        GameObject managerObj = GameObject.Find("DialogueManager");

        if (managerObj == null)
        {
            // Crea un nuevo GameObject
            managerObj = new GameObject("DialogueManager");
            Undo.RegisterCreatedObjectUndo(managerObj, "Create DialogueManager");
        }

        // Añade el componente si no lo tiene
        if (managerObj.GetComponent<DialogueAutoReset>() == null)
        {
            DialogueAutoReset autoReset = managerObj.AddComponent<DialogueAutoReset>();
            Undo.RegisterCreatedObjectUndo(autoReset, "Add DialogueAutoReset");
        }

        // Añade el NPCDialogueDataManager si no lo tiene
        if (managerObj.GetComponent<NPCDialogueDataManager>() == null)
        {
            NPCDialogueDataManager dataManager = managerObj.AddComponent<NPCDialogueDataManager>();
            Undo.RegisterCreatedObjectUndo(dataManager, "Add NPCDialogueDataManager");
        }

        Selection.activeGameObject = managerObj;
        EditorGUIUtility.PingObject(managerObj);

        Debug.Log("DialogueManager creado con DialogueAutoReset y NPCDialogueDataManager");
    }

    private void DrawCharacterDialogueList(List<CharacterDialogueSet> dialogues, string playerTag)
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button($"+ Añadir Nuevo Diálogo para {playerTag}"))
        {
            CharacterDialogueSet newSet = new CharacterDialogueSet
            {
                characterTag = playerTag,
                setName = $"Diálogo {dialogues.Count + 1}",
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
                "Eliminar Diálogo",
                $"¿Eliminar '{dialogueSet.setName}'?",
                "Sí", "Cancelar"))
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
            EditorGUILayout.HelpBox("Este diálogo ya ha sido mostrado (Estado: Visto)", MessageType.Info);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Condiciones de Desbloqueo", EditorStyles.boldLabel);
        DrawConditionsList(dialogueSet.conditions);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Contenido del Diálogo", EditorStyles.boldLabel);
        DrawDialogueNodeList(dialogueSet.dialogueNodes);

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawConditionsList(List<DialogueCondition> conditions)
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("+ Añadir Condición"))
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
                condition.minimumCount = EditorGUILayout.IntField("Mínimo Interacciones", condition.minimumCount);
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

        if (GUILayout.Button("+ Añadir Nodo de Texto"))
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

        if (GUILayout.Button("+ Añadir Opción"))
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

        option.optionText = EditorGUILayout.TextField($"Opción {index + 1}", option.optionText);
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