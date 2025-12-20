using UnityEngine;
using System.Reflection;

public class PauseStateChecker : MonoBehaviour
{
    void Start()
    {



        
        PauseController pauseController = FindObjectOfType<PauseController>();
        if (pauseController != null)
        {
            var type = pauseController.GetType();
            var isPausedField = type.GetField("isPaused", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (isPausedField != null)
            {
                bool isPaused = (bool)isPausedField.GetValue(pauseController);

            }
        }
        
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {

        }
        
        
        if (Mathf.Approximately(Time.timeScale, 0f))
        {

            Time.timeScale = 1f;
        }
    }
}
