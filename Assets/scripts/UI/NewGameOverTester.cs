using UnityEngine;

/// <summary>
/// Test script to demonstrate the new GameOver system
/// </summary>
public class NewGameOverTester : MonoBehaviour
{
    [Header("Test Settings")]
    public KeyCode showGameOverKey = KeyCode.G;
    public KeyCode hideGameOverKey = KeyCode.H;
    public KeyCode triggerDeathKey = KeyCode.D;
    
    private NewGameOverManager gameOverManager;
    private CheckpointSystem checkpointSystem;
    
    private void Start()
    {
        // Get references to the systems
        gameOverManager = NewGameOverManager.Instance;
        checkpointSystem = CheckpointSystem.Instance;
        
        if (gameOverManager == null)
        {
            Debug.LogError("NewGameOverManager not found! Creating one now...");
            GameObject go = new GameObject("NewGameOverManager");
            gameOverManager = go.AddComponent<NewGameOverManager>();
        }
        
        if (checkpointSystem == null)
        {
            Debug.LogError("CheckpointSystem not found! Creating one now...");
            GameObject go = new GameObject("CheckpointSystem");
            checkpointSystem = go.AddComponent<CheckpointSystem>();
        }
        
        // Save initial checkpoint
        checkpointSystem.SaveCheckpoint();
        
        Debug.Log($"New GameOver System Test Ready!");
        Debug.Log($"Press {showGameOverKey} to show GameOver screen");
        Debug.Log($"Press {hideGameOverKey} to hide GameOver screen");
        Debug.Log($"Press {triggerDeathKey} to simulate player death");
    }
    
    private void Update()
    {
        // Test showing GameOver screen
        if (Input.GetKeyDown(showGameOverKey))
        {
            Debug.Log("Showing GameOver screen...");
            gameOverManager.ShowGameOver();
        }
        
        // Test hiding GameOver screen
        if (Input.GetKeyDown(hideGameOverKey))
        {
            Debug.Log("Hiding GameOver screen...");
            gameOverManager.HideGameOver();
        }
        
        // Test simulating player death
        if (Input.GetKeyDown(triggerDeathKey))
        {
            SimulatePlayerDeath();
        }
    }
    
    /// <summary>
    /// Simulates player death and shows GameOver screen
    /// </summary>
    private void SimulatePlayerDeath()
    {
        Debug.Log("Player died! Showing GameOver screen...");
        
        // Save current state before showing GameOver
        checkpointSystem.SaveCheckpoint();
        
        // Show GameOver screen
        gameOverManager.ShowGameOver();
        
        // You can customize the GameOver text here
        gameOverManager.SetGameOverText("YOU DIED");
    }
    
    /// <summary>
    /// Test method to save a checkpoint manually
    /// </summary>
    [ContextMenu("Save Checkpoint")]
    public void SaveCheckpointManually()
    {
        if (checkpointSystem != null)
        {
            checkpointSystem.SaveCheckpoint();
            Debug.Log("Checkpoint saved manually");
        }
    }
    
    /// <summary>
    /// Test method to clear all checkpoints
    /// </summary>
    [ContextMenu("Clear Checkpoints")]
    public void ClearCheckpointsManually()
    {
        if (checkpointSystem != null)
        {
            checkpointSystem.ClearCheckpoints();
            Debug.Log("All checkpoints cleared");
        }
    }
    
    /// <summary>
    /// Test method to customize GameOver appearance
    /// </summary>
    [ContextMenu("Customize GameOver")]
    public void CustomizeGameOver()
    {
        if (gameOverManager != null)
        {
            // Change button texts
            gameOverManager.SetRetryButtonText("TRY AGAIN");
            gameOverManager.SetQuitButtonText("EXIT GAME");
            
            // Change colors
            gameOverManager.SetButtonColors(
                new Color(0.3f, 0.1f, 0.1f, 1f),     // Normal
                new Color(0.5f, 0.2f, 0.2f, 1f),     // Highlighted  
                new Color(0.1f, 0.05f, 0.05f, 1f)    // Pressed
            );
            
            // Change panel color
            gameOverManager.SetPanelColor(new Color(0.1f, 0, 0, 0.9f));
            
            Debug.Log("GameOver screen customized");
        }
    }
}