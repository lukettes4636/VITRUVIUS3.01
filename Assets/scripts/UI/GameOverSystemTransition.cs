using UnityEngine;

/// <summary>
/// Manager to handle transition from old GameOver system to new one
/// Deactivates the old system and activates the new system
/// </summary>
public class GameOverSystemTransition : MonoBehaviour
{
    [Header("System Management")]
    public bool disableOldSystemOnStart = true;
    public bool enableNewSystemOnStart = true;
    
    private NewGameOverManager newGameOverManager;
    private GameOverManager oldGameOverManager;
    private bool oldSystemActive = false;
    
    private void Awake()
    {
        // Find both systems
        newGameOverManager = FindObjectOfType<NewGameOverManager>();
        oldGameOverManager = FindObjectOfType<GameOverManager>();
        
        // Handle transition
        HandleSystemTransition();
    }
    
    private void HandleSystemTransition()
    {
        // Disable old system if requested
        if (disableOldSystemOnStart && oldGameOverManager != null)
        {
            DisableOldSystem();
        }
        
        // Enable new system if requested and not already active
        if (enableNewSystemOnStart)
        {
            if (newGameOverManager == null)
            {
                // Create new system if it doesn't exist
                GameObject go = new GameObject("NewGameOverManager");
                newGameOverManager = go.AddComponent<NewGameOverManager>();
            }
            
            // Ensure new system is properly initialized
            newGameOverManager.gameObject.SetActive(true);
        }
        
        string oldStatus = oldSystemActive ? "active" : "disabled";
        string newStatus = (newGameOverManager != null) ? "active" : "missing";
        Debug.Log("GameOver System Transition: Old system " + oldStatus + ", New system " + newStatus);
    }
    
    /// <summary>
    /// Disables the old GameOver system completely
    /// </summary>
    private void DisableOldSystem()
    {
        if (oldGameOverManager != null)
        {
            // Disable the GameOverManager script
            oldGameOverManager.enabled = false;
            
            // Hide any existing UI it might have created
            var fadeScreen = GameObject.Find("FadeScreen");
            if (fadeScreen != null)
            {
                fadeScreen.SetActive(false);
            }
            
            // Disable any child UI objects
            Transform[] children = oldGameOverManager.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != oldGameOverManager.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            
            oldSystemActive = false;
            Debug.Log("Old GameOver system disabled");
        }
    }
    
    /// <summary>
    /// Enables the old GameOver system (for rollback if needed)
    /// </summary>
    public void EnableOldSystem()
    {
        if (oldGameOverManager != null)
        {
            oldGameOverManager.enabled = true;
            oldSystemActive = true;
            Debug.Log("Old GameOver system enabled");
        }
    }
    
    /// <summary>
    /// Disables the new GameOver system
    /// </summary>
    public void DisableNewSystem()
    {
        if (newGameOverManager != null)
        {
            newGameOverManager.gameObject.SetActive(false);
            Debug.Log("New GameOver system disabled");
        }
    }
    
    /// <summary>
    /// Enables the new GameOver system
    /// </summary>
    public void EnableNewSystem()
    {
        if (newGameOverManager != null)
        {
            newGameOverManager.gameObject.SetActive(true);
            Debug.Log("New GameOver system enabled");
        }
    }
    
    /// <summary>
    /// Gets the current active system
    /// </summary>
    public string GetActiveSystem()
    {
        if (newGameOverManager != null && newGameOverManager.gameObject.activeInHierarchy)
            return "New System";
        else if (oldGameOverManager != null && oldGameOverManager.enabled)
            return "Old System";
        else
            return "No Active System";
    }
    
    /// <summary>
    /// Shows GameOver using the new system
    /// </summary>
    public void ShowGameOverNewSystem()
    {
        if (newGameOverManager != null)
        {
            newGameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("New GameOver system not available");
        }
    }
    
    /// <summary>
    /// Shows GameOver using the old system (for comparison/rollback)
    /// </summary>
    public void ShowGameOverOldSystem()
    {
        if (oldGameOverManager != null && oldGameOverManager.enabled)
        {
            // Trigger the old system's GameOver
            oldGameOverManager.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("Old GameOver system not available");
        }
    }
}