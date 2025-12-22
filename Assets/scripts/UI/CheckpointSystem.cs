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
            [System.Serializable]
            public class PlayerSnapshot
            {
                public int playerID;
                public Vector3 position;
                public Quaternion rotation;
            }
            public List<PlayerSnapshot> players = new List<PlayerSnapshot>();
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
            
            // Save all players by PlayerIdentifier
            var identifiers = FindObjectsOfType<PlayerIdentifier>();
            foreach (var id in identifiers)
            {
                var snap = new CheckpointData.PlayerSnapshot
                {
                    playerID = id.playerID,
                    position = id.transform.position,
                    rotation = id.transform.rotation
                };
                checkpoint.players.Add(snap);
            }
            
            checkpoint.gameTime = Time.time;
            
            lastCheckpoint = checkpoint;
            checkpointHistory.Add(checkpoint);
        
        // Keep only the last MAX_CHECKPOINTS
        if (checkpointHistory.Count > MAX_CHECKPOINTS)
        {
            checkpointHistory.RemoveAt(0);
        }
        
            var pos = checkpoint.players.Count > 0 ? checkpoint.players[0].position : Vector3.zero;
            Debug.Log($"Checkpoint saved: {checkpoint.sceneName} at position {pos}");
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
        
        public System.Collections.IEnumerator LoadLastCheckpointRoutine()
        {
            if (lastCheckpoint == null)
            {
                Debug.LogWarning("No checkpoint available to load");
                yield break;
            }
            yield return LoadCheckpointCoroutine(lastCheckpoint);
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
            var identifiers = FindObjectsOfType<PlayerIdentifier>();
            foreach (var id in identifiers)
            {
                var snap = checkpoint.players.Find(s => s.playerID == id.playerID);
                if (snap != null)
                {
                    var ph = id.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.ForceReviveAt(snap.position);
                        id.transform.rotation = snap.rotation;
                    }
                    else
                    {
                        id.transform.position = snap.position;
                        id.transform.rotation = snap.rotation;
                    }
                }
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
