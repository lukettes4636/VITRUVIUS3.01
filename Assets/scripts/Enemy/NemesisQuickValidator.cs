using UnityEngine;

public class NemesisQuickValidator : MonoBehaviour
{
    [ContextMenu("Quick Compile Check")]
    void QuickCompileCheck()
    {

        
        var nemesisAI = GetComponent<NemesisAI>();
        if (nemesisAI != null)
        {

            
            
            try
            {
                nemesisAI.SetAlerted(true);
                nemesisAI.SetAlerted(false);

                
                var target = nemesisAI.GetCurrentTarget();

                
                var isAttacking = nemesisAI.IsAttacking();

                

            }
            catch (System.Exception e)
            {

            }
        }
        else
        {

        }
    }
}
