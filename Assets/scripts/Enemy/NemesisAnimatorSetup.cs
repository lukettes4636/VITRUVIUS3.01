using UnityEngine;

public class NemesisAnimatorSetup : MonoBehaviour
{
    [Header("Animator Controller Setup")]
    [Tooltip("The Animator Controller asset (if not in Resources/Horror)")]
    public RuntimeAnimatorController animatorController;
    
    [Header("Required Parameters")]
    [Tooltip("These parameters MUST exist in your Animator Controller")]
    public string walkParameter = "Walk";
    public string attackParameter = "Attack";
    public string detectedParameter = "Detected";
    
    [Header("Animation States")]
    [Tooltip("These states should exist in your Animator Controller")]
    public string walkStateName = "Walk";
    public string attackStateName = "Attack";
    public string idleStateName = "Idle";
    
    private Animator animator;
    private NemesisAI_Enhanced nemesisAI;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        nemesisAI = GetComponent<NemesisAI_Enhanced>();
        
        if (animator == null)
        {

            return;
        }
        
        
        if (animator.runtimeAnimatorController == null && animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }
        
        ValidateAnimatorSetup();
    }
    
    [ContextMenu("Validate Animator Setup")]
    public void ValidateAnimatorSetup()
    {
        if (animator == null)
        {

            return;
        }
        

        
        
        if (animator.runtimeAnimatorController == null)
        {





            return;
        }
        

        
        
        bool hasWalk = HasParameter(walkParameter, AnimatorControllerParameterType.Bool);
        bool hasAttack = HasParameter(attackParameter, AnimatorControllerParameterType.Bool);
        bool hasDetected = HasParameter(detectedParameter, AnimatorControllerParameterType.Trigger);
        



        
        if (!hasWalk || !hasAttack || !hasDetected)
        {





        }
        else
        {

        }
        
        



        
        
        TestParameterAccess();
    }
    
    bool HasParameter(string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || animator.parameters == null)
            return false;
            
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName && param.type == parameterType)
            {
                return true;
            }
        }
        return false;
    }
    
    void TestParameterAccess()
    {

        
        try
        {
            
            
            if (HasParameter(walkParameter, AnimatorControllerParameterType.Bool))
            {
                int walkHash = Animator.StringToHash(walkParameter);
                animator.SetBool(walkHash, true);
                bool walkValue = animator.GetBool(walkHash);
                animator.SetBool(walkHash, false);
            }
            
            
            if (HasParameter(attackParameter, AnimatorControllerParameterType.Bool))
            {
                int attackHash = Animator.StringToHash(attackParameter);
                animator.SetBool(attackHash, true);
                bool attackValue = animator.GetBool(attackHash);
                animator.SetBool(attackHash, false);
            }
            
            
            if (HasParameter(detectedParameter, AnimatorControllerParameterType.Trigger))
            {
                int detectedHash = Animator.StringToHash(detectedParameter);
                animator.SetTrigger(detectedHash);
            }

            

        }
        catch (System.Exception e)
        {

        }
    }
    
    [ContextMenu("Fix Common Issues")]
    public void FixCommonIssues()
    {
        if (animator == null)
        {

            return;
        }
        

        
        
        if (animator.runtimeAnimatorController == null)
        {

            RuntimeAnimatorController horrorController = Resources.Load<RuntimeAnimatorController>("Horror");
            if (horrorController != null)
            {
                animator.runtimeAnimatorController = horrorController;

            }
            else
            {



            }
        }
        
        
        if (nemesisAI != null)
        {

            
            
        }
        
        ValidateAnimatorSetup();
    }
    
    void Update()
    {
        
        if (Application.isEditor && Input.GetKeyDown(KeyCode.F1))
        {
            ValidateAnimatorSetup();
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(NemesisAnimatorSetup))]
public class NemesisAnimatorSetupEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        NemesisAnimatorSetup setup = (NemesisAnimatorSetup)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Animator Debugging Tools", UnityEditor.EditorStyles.boldLabel);
        
        if (GUILayout.Button("Validate Animator Setup"))
        {
            setup.ValidateAnimatorSetup();
        }
        
        if (GUILayout.Button("Fix Common Issues"))
        {
            setup.FixCommonIssues();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Press F1 in Play Mode to re-validate", UnityEditor.EditorStyles.miniLabel);
    }
}
#endif
