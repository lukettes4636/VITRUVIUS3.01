using UnityEditor;
using UnityEditor.Animations;

public static class FixAnimatorControllers
{
    [MenuItem("Tools/Cleanup/Fix Animator Controllers")]
    public static void Run()
    {
        var guids = AssetDatabase.FindAssets("t:AnimatorController");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (ctrl == null) continue;

            if (ctrl.layers == null || ctrl.layers.Length == 0)
            {
                var layer = new AnimatorControllerLayer { name = "Base Layer", defaultWeight = 1f, blendingMode = AnimatorLayerBlendingMode.Override, syncedLayerIndex = -1, iKPass = false };
                var sm = new AnimatorStateMachine { name = "Base" };
                layer.stateMachine = sm;
                ctrl.AddLayer(layer);
            }

            for (int i = 0; i < ctrl.layers.Length; i++)
            {
                var layer = ctrl.layers[i];
                if (layer.stateMachine == null)
                {
                    layer.stateMachine = new AnimatorStateMachine { name = "Base" };
                    ctrl.layers[i] = layer;
                }

                var sm = layer.stateMachine;
                if (sm.states == null || sm.states.Length == 0)
                {
                    var idle = sm.AddState("Idle");
                    sm.defaultState = idle;
                }
                else if (sm.defaultState == null)
                {
                    sm.defaultState = sm.states[0].state;
                }
            }

            EditorUtility.SetDirty(ctrl);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
