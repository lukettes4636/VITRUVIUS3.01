using UnityEngine;
using System.Collections;

public class PauseDiagnosticTool : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool runDiagnosticOnStart = true;
    [SerializeField] private bool fixIssuesAutomatically = true;
    
    void Start()
    {
        if (runDiagnosticOnStart)
        {
            StartCoroutine(RunDiagnosticAfterDelay());
        }
    }
    
    IEnumerator RunDiagnosticAfterDelay()
    {
        
        yield return null;
        



        
        
        CheckPauseController();
        CheckGameOverManager();
        CheckOtherScripts();
        
        
        if (fixIssuesAutomatically && Mathf.Approximately(Time.timeScale, 0f))
        {

            Time.timeScale = 1f;

        }
    }
    
    void CheckPauseController()
    {
        PauseController pauseController = FindObjectOfType<PauseController>();
        if (pauseController != null)
        {
            var type = pauseController.GetType();
            var isPausedField = type.GetField("isPaused", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isPausedField != null)
            {
                bool isPaused = (bool)isPausedField.GetValue(pauseController);

                
                if (isPaused)
                {

                }
            }
        }
    }
    
    void CheckGameOverManager()
    {
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            var type = gameOverManager.GetType();
            var gameOverTriggeredField = type.GetField("gameOverTriggered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameOverTriggeredField != null)
            {
                bool gameOverTriggered = (bool)gameOverTriggeredField.GetValue(gameOverManager);

                
                if (gameOverTriggered)
                {

                }
            }
        }
    }
    
    void CheckOtherScripts()
    {
        
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script == null) continue;
            
            var type = script.GetType();
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            foreach (var method in methods)
            {
                if (method.Name.Contains("Pause") || method.Name.Contains("Start"))
                {

                }
            }
        }
    }
    
    [ContextMenu("Manual Diagnostic")]
    public void ManualDiagnostic()
    {
        StartCoroutine(RunDiagnosticAfterDelay());
    }
    
    [ContextMenu("Force Unpause")]
    public void ForceUnpause()
    {
        Time.timeScale = 1f;

    }
}
