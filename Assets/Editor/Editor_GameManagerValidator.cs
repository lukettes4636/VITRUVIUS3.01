using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class Editor_GameManagerValidator : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GameManager gameManager = (GameManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("GameManager Status", EditorStyles.boldLabel);
        
        if (GameManager.Instance != null)
        {
            EditorGUILayout.HelpBox("✓ GameManager Instance is active", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("✗ GameManager Instance is null - this may cause issues", MessageType.Warning);
        }
        
        if (gameManager.gameObject.name == "GameManager")
        {
            EditorGUILayout.HelpBox("✓ GameObject name is correct", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ GameObject name is '" + gameManager.gameObject.name + "' - consider renaming to 'GameManager'", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Validate Setup"))
        {
            ValidateGameManagerSetup(gameManager);
        }
    }
    
    void ValidateGameManagerSetup(GameManager gameManager)
    {
        // Add validation logic here if needed
    }
}
