using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Checkpoint system for the new GameOver manager
/// Handles saving and loading of game state
/// </summary>
public class CheckpointSystem : MonoBehaviour
{
    [System.Serializable]
    public class CheckpointData
    {
        public string sceneName;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public int playerHealth;
        public int playerScore;
        public float gameTime;
        public Dictionary<string, object> customData;
        
        public CheckpointData()
        {
            customData = new Dictionary<string, object>();
        }
    }
    
    private static CheckpointSystem instance;
    public static CheckpointSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CheckpointSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CheckpointSystem");
                    instance = go.AddComponent<CheckpointSystem>();
                }
            }
            return instance;
        }
    }
    
    private CheckpointData lastCheckpoint;
    private List<CheckpointData> checkpointHistory;
    private const int MAX_CHECKPOINTS = 10;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        checkpointHistory = new List<CheckpointData>();
    }
    
    /// <summary>
    /// Saves the current game state as a checkpoint
    /// </summary>
    public void SaveCheckpoint()
    {
        CheckpointData checkpoint = new CheckpointData();
        
        // Save current scene
        checkpoint.sceneName = SceneManager.GetActiveScene().name;
        
        // Save player data (you'll need to adapt this to your player system)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            checkpoint.playerPosition = player.transform.position;
            checkpoint.playerRotation = player.transform.rotation;
            
            // Add your player health/score components here
            // Example: checkpoint.playerHealth = player.GetComponent<PlayerHealth>().currentHealth;
            // Example: checkpoint.playerScore = player.GetComponent<PlayerScore>().currentScore;
        }
        
        checkpoint.gameTime = Time.time;
        
        lastCheckpoint = checkpoint;
        checkpointHistory.Add(checkpoint);
        
        // Keep only the last MAX_CHECKPOINTS
        if (checkpointHistory.Count > MAX_CHECKPOINTS)
        {
            checkpointHistory.RemoveAt(0);
        }
        
        Debug.Log($"Checkpoint saved: {checkpoint.sceneName} at position {checkpoint.playerPosition}");
    }
    
    /// <summary>
    /// Loads the last checkpoint
    /// </summary>
    public void LoadLastCheckpoint()
    {
        if (lastCheckpoint == null)
        {
            Debug.LogWarning("No checkpoint available to load");
            return;
        }
        
        StartCoroutine(LoadCheckpointCoroutine(lastCheckpoint));
    }
    
    /// <summary>
    /// Loads a specific checkpoint
    /// </summary>
    private System.Collections.IEnumerator LoadCheckpointCoroutine(CheckpointData checkpoint)
    {
        Debug.Log($"Loading checkpoint: {checkpoint.sceneName}");
        
        // Load the scene if it's different from current
        if (SceneManager.GetActiveScene().name != checkpoint.sceneName)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(checkpoint.sceneName);
            yield return new WaitUntil(() => loadOperation.isDone);
        }
        
        // Restore player state
        RestorePlayerState(checkpoint);
        
        Debug.Log("Checkpoint loaded successfully");
    }
    
    /// <summary>
    /// Restores player state from checkpoint
    /// </summary>
    private void RestorePlayerState(CheckpointData checkpoint)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Restore position and rotation
            player.transform.position = checkpoint.playerPosition;
            player.transform.rotation = checkpoint.playerRotation;
            
            // Restore health and score (adapt to your player system)
            // Example: player.GetComponent<PlayerHealth>().currentHealth = checkpoint.playerHealth;
            // Example: player.GetComponent<PlayerScore>().currentScore = checkpoint.playerScore;
        }
        else
        {
            Debug.LogWarning("Player not found when restoring checkpoint");
        }
    }
    
    /// <summary>
    /// Checks if a checkpoint is available
    /// </summary>
    public bool HasCheckpoint()
    {
        return lastCheckpoint != null;
    }
    
    /// <summary>
    /// Gets the last checkpoint data (for reference)
    /// </summary>
    public CheckpointData GetLastCheckpoint()
    {
        return lastCheckpoint;
    }
    
    /// <summary>
    /// Clears all checkpoints
    /// </summary>
    public void ClearCheckpoints()
    {
        lastCheckpoint = null;
        checkpointHistory.Clear();
        Debug.Log("All checkpoints cleared");
    }
    
    /// <summary>
    /// Auto-save checkpoint at regular intervals (optional)
    /// </summary>
    public void EnableAutoSave(float intervalSeconds = 60f)
    {
        CancelInvoke(nameof(SaveCheckpoint));
        InvokeRepeating(nameof(SaveCheckpoint), intervalSeconds, intervalSeconds);
    }
    
    /// <summary>
    /// Disable auto-save
    /// </summary>
    public void DisableAutoSave()
    {
        CancelInvoke(nameof(SaveCheckpoint));
    }
}