using UnityEngine;
using UnityEditor;
using TMPro;

public class SetupEndGameSplash : MonoBehaviour
{
    public static void SetupSplash()
    {
        EndLevelTrigger trigger = FindObjectOfType<EndLevelTrigger>();
        if (trigger == null)
        {
            Debug.LogError("No EndLevelTrigger found in the scene!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(trigger, "Setup Splash References");

        // Find Animator Controller
        string animPath = "Assets/Dark UI/Animations/Transitions/Splash Title Transition.controller";
        RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animPath);
        
        if (animController != null)
        {
            trigger.splashAnimatorController = animController;
            Debug.Log("Assigned Splash Animator Controller.");
        }
        else
        {
            Debug.LogError($"Could not find Animator Controller at {animPath}");
        }

        // Find Font Asset
        string fontPath = "Assets/Dark UI/Fonts/Larke Sans Light SDF.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);

        if (fontAsset != null)
        {
            trigger.splashFont = fontAsset;
            Debug.Log("Assigned Splash Font Asset.");
        }
        else
        {
            Debug.LogError($"Could not find Font Asset at {fontPath}");
        }
        
        EditorUtility.SetDirty(trigger);
        Debug.Log("End Game Splash setup complete.");
    }
}
