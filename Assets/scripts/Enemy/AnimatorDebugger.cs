using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorDebugger : MonoBehaviour
{
    [ContextMenu("Debug Animator Parameters")]
    public void DebugAnimatorParameters()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {

            return;
        }






        
        if (animator.runtimeAnimatorController != null)
        {

            
            var parameters = animator.parameters;

            
            foreach (var param in parameters)
            {

            }
            
            var layers = animator.layerCount;

            
            for (int i = 0; i < layers; i++)
            {

            }
        }
        else
        {

        }
        

        if (animator.runtimeAnimatorController != null)
        {
            foreach (var param in animator.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:

                        break;
                    case AnimatorControllerParameterType.Trigger:

                        break;
                    case AnimatorControllerParameterType.Float:

                        break;
                    case AnimatorControllerParameterType.Int:

                        break;
                }
            }
        }
    }
    
    [ContextMenu("Test Animation Parameters")]
    public void TestAnimationParameters()
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null) return;


        
        
        int walkHash = Animator.StringToHash("Walk");
        bool hasWalk = animator.parameterCount > 0 && System.Array.Exists(animator.parameters, p => p.nameHash == walkHash);
        if (hasWalk)
        {

            animator.SetBool(walkHash, true);

            
            
            Invoke("StopWalkAnimation", 2f);
        }
        else
        {

        }
        
        
        int attackHash = Animator.StringToHash("Attack");
        bool hasAttack = animator.parameterCount > 0 && System.Array.Exists(animator.parameters, p => p.nameHash == attackHash);
        if (hasAttack)
        {

            animator.SetBool(attackHash, true);

            
            
            Invoke("StopAttackAnimation", 1f);
        }
        else
        {

        }
        
        
        int detectedHash = Animator.StringToHash("Detected");
        bool hasDetected = animator.parameterCount > 0 && System.Array.Exists(animator.parameters, p => p.nameHash == detectedHash);
        if (hasDetected)
        {

            animator.SetTrigger(detectedHash);

        }
        else
        {

        }
    }
    
    private void StopWalkAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Walk", false);

        }
    }
    
    private void StopAttackAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Attack", false);

        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"State: {stateInfo.shortNameHash}\nTime: {stateInfo.normalizedTime:F2}");
            }
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(AnimatorDebugger))]
public class AnimatorDebuggerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        AnimatorDebugger debugger = (AnimatorDebugger)target;
        
        if (GUILayout.Button("Debug Animator Parameters"))
        {
            debugger.DebugAnimatorParameters();
        }
        
        if (GUILayout.Button("Test Animation Parameters"))
        {
            debugger.TestAnimationParameters();
        }
    }
}
#endif
