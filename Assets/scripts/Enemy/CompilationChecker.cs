using UnityEngine;

public class CompilationChecker : MonoBehaviour
{
    [ContextMenu("Check All Nemesis Scripts")]
    void CheckAllNemesisScripts()
    {

        
        
        try
        {
            var nemesisAI = GetComponent<NemesisAI>();
            if (nemesisAI != null)
            {

            }
            else
            {

            }
        }
        catch (System.Exception e)
        {

        }
        
        
        try
        {
            var sceneSetup = GetComponent<NemesisSceneSetup>();
            if (sceneSetup != null)
            {

            }
            else
            {

            }
        }
        catch (System.Exception e)
        {

        }
        
        
        try
        {
            var enhancedAI = GetComponent<NemesisAI_Enhanced>();
            if (enhancedAI != null)
            {

            }
            else
            {

            }
        }
        catch (System.Exception e)
        {

        }
        

    }
}
