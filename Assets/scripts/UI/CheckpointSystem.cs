using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Checkpoint system for the cooperative 2-player game.
/// Saves and restores per-player position, rotation and health.
/// Automatically hooks into the Checkpoint.OnCheckpointReached event.
/// </summary>
public class CheckpointSystem : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Data structures
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>State snapshot for one player.</summary>
    [System.Serializable]
    public class PlayerCheckpointData
    {
        public int playerID;
        public Vector3 position;
        public Quaternion rotation;
        public int health;
    }

    /// <summary>Full checkpoint snapshot for the whole session.</summary>
    [System.Serializable]
    public class CheckpointData
    {
        public string sceneName;
        public float gameTime;

        // Keyed by playerID (1 or 2) so it works for any number of players.
        public Dictionary<int, PlayerCheckpointData> playerData =
            new Dictionary<int, PlayerCheckpointData>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Singleton
    // ─────────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────────
    //  Internal state
    // ─────────────────────────────────────────────────────────────────────────

    private CheckpointData lastCheckpoint;
    private List<CheckpointData> checkpointHistory = new List<CheckpointData>();
    private const int MAX_CHECKPOINTS = 10;

    // ─────────────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────────

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
    }

    // FIX #3: Subscribe / unsubscribe to the static Checkpoint event here so
    // that every time a checkpoint trigger is entered we capture the full
    // two-player state automatically.
    private void OnEnable()
    {
        Checkpoint.OnCheckpointReached += HandleCheckpointReached;
    }

    private void OnDisable()
    {
        Checkpoint.OnCheckpointReached -= HandleCheckpointReached;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Event handler
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by Checkpoint when any player touches a checkpoint trigger.
    /// We save the complete state of BOTH players at that moment.
    /// </summary>
    private void HandleCheckpointReached(int triggeringPlayerID, Vector3 checkpointPosition)
    {
        Debug.Log($"[CheckpointSystem] Checkpoint reached by player {triggeringPlayerID}. Saving full state.");
        SaveCheckpoint();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Save
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Captures position, rotation and current health for every player
    /// identified by a PlayerIdentifier component in the scene.
    /// </summary>
    public void SaveCheckpoint()
    {
        CheckpointData checkpoint = new CheckpointData
        {
            sceneName = SceneManager.GetActiveScene().name,
            gameTime = Time.time
        };

        // Find every player in the scene using PlayerIdentifier.
        PlayerIdentifier[] allPlayers = FindObjectsOfType<PlayerIdentifier>();

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("[CheckpointSystem] SaveCheckpoint: no PlayerIdentifier objects found in scene.");
        }

        foreach (PlayerIdentifier pid in allPlayers)
        {
            PlayerHealth health = pid.GetComponent<PlayerHealth>();

            var data = new PlayerCheckpointData
            {
                playerID = pid.playerID,
                position = pid.transform.position,
                rotation = pid.transform.rotation,
                health = health != null ? health.GetCurrentHealth() : 0
            };

            checkpoint.playerData[pid.playerID] = data;

            Debug.Log($"[CheckpointSystem] Saved player {pid.playerID} at {data.position} with {data.health} HP.");
        }

        lastCheckpoint = checkpoint;
        checkpointHistory.Add(checkpoint);

        if (checkpointHistory.Count > MAX_CHECKPOINTS)
            checkpointHistory.RemoveAt(0);

        Debug.Log($"[CheckpointSystem] Checkpoint saved: scene={checkpoint.sceneName}, players={checkpoint.playerData.Count}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Load
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Loads the last saved checkpoint.</summary>
    public void LoadLastCheckpoint()
    {
        if (lastCheckpoint == null)
        {
            Debug.LogWarning("[CheckpointSystem] LoadLastCheckpoint: no checkpoint saved yet.");
            return;
        }

        StartCoroutine(LoadCheckpointCoroutine(lastCheckpoint));
    }

    private IEnumerator LoadCheckpointCoroutine(CheckpointData checkpoint)
    {
        Debug.Log($"[CheckpointSystem] Loading checkpoint for scene: {checkpoint.sceneName}");

        // Only reload if the scene actually changed.
        if (SceneManager.GetActiveScene().name != checkpoint.sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(checkpoint.sceneName);
            yield return new WaitUntil(() => op.isDone);

            // Wait one extra frame so all Awake/Start calls have run after load.
            yield return null;
        }

        RestorePlayerState(checkpoint);
    }

    /// <summary>
    /// FIX #2: Restores BOTH players from the checkpoint data.
    /// Moves each player to their saved position and calls RestoreState()
    /// on their PlayerHealth so the death UI is dismissed and they come
    /// back to life correctly.
    /// </summary>
    private void RestorePlayerState(CheckpointData checkpoint)
    {
        // Find all PlayerIdentifier components in the (possibly freshly loaded) scene.
        PlayerIdentifier[] allPlayers = FindObjectsOfType<PlayerIdentifier>();

        if (allPlayers.Length == 0)
        {
            Debug.LogWarning("[CheckpointSystem] RestorePlayerState: no PlayerIdentifier objects found.");
            return;
        }

        foreach (PlayerIdentifier pid in allPlayers)
        {
            if (!checkpoint.playerData.TryGetValue(pid.playerID, out PlayerCheckpointData data))
            {
                Debug.LogWarning($"[CheckpointSystem] No saved data for player {pid.playerID}. Skipping.");
                continue;
            }

            // --- Teleport ---
            // Disable the CharacterController briefly so we can warp the transform
            // without the controller fighting the position change.
            CharacterController cc = pid.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            pid.transform.position = data.position;
            pid.transform.rotation = data.rotation;

            if (cc != null) cc.enabled = true;

            // --- Restore life / UI ---
            // RestoreState() handles: clearing IsDead, refilling health, re-enabling
            // movement, switching the Input Action Map back to "Player", and hiding
            // the respawn panel — everything the old GameManager.RespawnPlayer did.
            PlayerHealth health = pid.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.RestoreState();
            }

            Debug.Log($"[CheckpointSystem] Player {pid.playerID} restored to {data.position} with {data.health} HP.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Utilities
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns true if at least one checkpoint has been saved.</summary>
    public bool HasCheckpoint() => lastCheckpoint != null;

    /// <summary>Returns the raw last checkpoint data (read-only reference).</summary>
    public CheckpointData GetLastCheckpoint() => lastCheckpoint;

    /// <summary>Clears all saved checkpoints.</summary>
    public void ClearCheckpoints()
    {
        lastCheckpoint = null;
        checkpointHistory.Clear();
        Debug.Log("[CheckpointSystem] All checkpoints cleared.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Optional auto-save
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Starts auto-saving at the specified interval (in seconds).</summary>
    public void EnableAutoSave(float intervalSeconds = 60f)
    {
        CancelInvoke(nameof(SaveCheckpoint));
        InvokeRepeating(nameof(SaveCheckpoint), intervalSeconds, intervalSeconds);
    }

    /// <summary>Stops auto-saving.</summary>
    public void DisableAutoSave()
    {
        CancelInvoke(nameof(SaveCheckpoint));
    }
}