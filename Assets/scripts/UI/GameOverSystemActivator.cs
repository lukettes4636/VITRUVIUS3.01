using UnityEngine;

/// <summary>
/// Script to deactivate the old GameOver system and activate the new one
/// Attach this to a GameObject in your scene to handle the transition
/// </summary>
public class GameOverSystemActivator : MonoBehaviour
{
    [Header("System Configuration")]
    public bool autoDisableOldSystem = true;
    public bool autoEnableNewSystem = true;
    public GameObject newGameOverPrefab;
    
    private void Start()
    {
        HandleSystemTransition();
    }
    
    /// <summary>
    /// Handles the transition from old to new GameOver system
    /// </summary>
    private void HandleSystemTransition()
    {
        // Disable old system
        if (autoDisableOldSystem)
        {
            DisableOldGameOverSystem();
        }
        
        // Enable new system
        if (autoEnableNewSystem)
        {
            EnableNewGameOverSystem();
        }
    }
    
    /// <summary>
    /// Finds and disables the old GameOver system
    /// </summary>
    private void DisableOldGameOverSystem()
    {
        // Find the old GameOverManager in the scene
        GameOverManager oldManager = FindObjectOfType<GameOverManager>();
        
        if (oldManager != null)
        {
            // Disable the script
            oldManager.enabled = false;
            
            // Hide any UI it might have created
            GameObject fadeScreen = GameObject.Find("FadeScreen");
            if (fadeScreen != null)
            {
                fadeScreen.SetActive(false);
            }
            
            // Look for any GameOver UI elements and disable them
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("GameOver") || obj.name.Contains("FadeScreen"))
                {
                    if (obj.transform.parent == null) // Only disable root objects
                    {
                        obj.SetActive(false);
                    }
                }
            }
            
            Debug.Log("Old GameOver system disabled");
        }
        else
        {
            Debug.Log("No old GameOverManager found in scene");
        }
        
        // Also check for any prefab instances that might have GameOverManager
        GameOverManager[] allOldManagers = FindObjectsOfType<GameOverManager>();
        foreach (GameOverManager manager in allOldManagers)
        {
            if (manager != null && manager.enabled)
            {
                manager.enabled = false;
                Debug.Log($"Disabled GameOverManager on {manager.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Creates and enables the new GameOver system
    /// </summary>
    private void EnableNewGameOverSystem()
    {
        // Check if new system already exists
        NewGameOverManager existingManager = FindObjectOfType<NewGameOverManager>();
        
        if (existingManager == null)
        {
            // Create new system
            if (newGameOverPrefab != null)
            {
                // Instantiate from prefab if provided
                GameObject newSystem = Instantiate(newGameOverPrefab);
                newSystem.name = "NewGameOverSystem";
                Debug.Log("New GameOver system created from prefab");
            }
            else
            {
                // Create from scratch
                GameObject go = new GameObject("NewGameOverSystem");
                go.AddComponent<NewGameOverManager>();
                go.AddComponent<CheckpointSystem>();
                Debug.Log("New GameOver system created from scratch");
            }
        }
        else
        {
            Debug.Log("New GameOver system already exists");
        }
        
        // Initialize the new system
        NewGameOverManager newManager = FindObjectOfType<NewGameOverManager>();
        if (newManager != null)
        {
            // Ensure it's active
            newManager.gameObject.SetActive(true);
            
            // Hide it initially (it will be shown when needed)
            newManager.HideGameOver();
            
            Debug.Log("New GameOver system initialized and ready");
        }
    }
    
    /// <summary>
    /// Manually trigger the transition
    /// </summary>
    [ContextMenu("Trigger Transition")]
    public void TriggerTransition()
    {
        HandleSystemTransition();
    }
    
    /// <summary>
    /// Test the new GameOver system
    /// </summary>
    [ContextMenu("Test New System")]
    public void TestNewSystem()
    {
        NewGameOverManager newManager = FindObjectOfType<NewGameOverManager>();
        if (newManager != null)
        {
            newManager.ShowGameOver();
            Debug.Log("New GameOver system test triggered");
        }
        else
        {
            Debug.LogError("New GameOver system not found!");
        }
    }
    
    /// <summary>
    /// Check which systems are active
    /// </summary>
    [ContextMenu("Check System Status")]
    /// <summary>
    /// Check which systems are active
    /// </summary>
    [ContextMenu("Check System Status")]
    /// <summary>
    /// Check which systems are active
    /// </summary>
    [ContextMenu("Check System Status")]
    public void CheckSystemStatus()
    {
        GameOverManager oldManager = FindObjectOfType<GameOverManager>();
        NewGameOverManager newManager = FindObjectOfType<NewGameOverManager>();
        
        Debug.Log("=== GameOver System Status ===");
        
        string oldStatus = (oldManager != null && oldManager.enabled) ? "ACTIVE" : "INACTIVE";
        Debug.Log("Old System: " + oldStatus);
        
        string newStatus = (newManager != null && newManager.gameObject.activeInHierarchy) ? "ACTIVE" : "INACTIVE";
        Debug.Log("New System: " + newStatus);
        
        if (oldManager != null)
        {
            Debug.Log("Old Manager GameObject: " + oldManager.gameObject.name);
        }
        
        if (newManager != null)
        {
            Debug.Log("New Manager GameObject: " + newManager.gameObject.name);
        }
    }
}