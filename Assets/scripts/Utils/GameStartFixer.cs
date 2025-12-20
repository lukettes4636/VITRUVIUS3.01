using UnityEngine;

public class GameStartFixer : MonoBehaviour
{
    [Header("Start Settings")]
    [SerializeField] private bool forceUnpauseOnStart = true;
    [SerializeField] private bool logCurrentState = true;
    
    void Awake()
    {
        
        if (forceUnpauseOnStart)
        {
            ForceUnpause();
        }
    }
    
    void Start()
    {
        if (logCurrentState)
        {
            LogCurrentPauseState();
        }
        
        
        Invoke("DoubleCheckUnpause", 0.1f);
    }
    
    void ForceUnpause()
    {
        if (Mathf.Approximately(Time.timeScale, 0f))
        {

            Time.timeScale = 1f;
        }
    }
    
    void DoubleCheckUnpause()
    {
        if (Mathf.Approximately(Time.timeScale, 0f))
        {

            Time.timeScale = 1f;
        }
        
        LogCurrentPauseState();
    }
    
    void LogCurrentPauseState()
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
    }
    
    [ContextMenu("Force Unpause Now")]
    public void ForceUnpauseNow()
    {
        Time.timeScale = 1f;

    }
    
    [ContextMenu("Check Current State")]
    public void CheckCurrentState()
    {
        LogCurrentPauseState();
    }
}
