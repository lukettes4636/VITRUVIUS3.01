using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    
    
    
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    
    
    [Header("Player Management")]
    [Tooltip("Asigna los GameObjects raz de los jugadores aqu.")]
    [SerializeField] private GameObject[] playerGameObjects;

    [Tooltip("Puntos de spawn iniciales. Deben coincidir por ndice con playerGameObjects.")]
    [SerializeField] private Transform[] spawnPoints;

    
    private Dictionary<int, PlayerHealth> playerHealthMap = new Dictionary<int, PlayerHealth>();
    private Dictionary<int, Transform> playerSpawnMap = new Dictionary<int, Transform>();
    private Dictionary<int, Vector3> playerSpawnPositions = new Dictionary<int, Vector3>();
    [SerializeField] private bool logRespawnDebug = false;

    
    
    
    void OnEnable()
    {
        Checkpoint.OnCheckpointReached += UpdateSpawnPoint;
    }

    void OnDisable()
    {
        Checkpoint.OnCheckpointReached -= UpdateSpawnPoint;
    }

    
    
    
    void Start()
    {
        InitializePlayers();
    }

    private void InitializePlayers()
    {
        if (playerGameObjects == null || spawnPoints == null || playerGameObjects.Length != spawnPoints.Length)
        {

            return;
        }

        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            GameObject playerObj = playerGameObjects[i];
            Transform spawnPoint = spawnPoints[i];

            if (playerObj == null || spawnPoint == null)
            {

                continue;
            }

            PlayerIdentifier identifier = playerObj.GetComponent<PlayerIdentifier>();
            PlayerHealth health = playerObj.GetComponent<PlayerHealth>();

            if (identifier == null)
            {

                continue;
            }

            int playerID = identifier.playerID;

            playerHealthMap[playerID] = health;
            playerSpawnMap[playerID] = spawnPoint;
            playerSpawnPositions[playerID] = spawnPoint.position; 

            if (health != null)
                health.OnPlayerDied += HandlePlayerDeath;
        }
    }

    
    
    
    private void UpdateSpawnPoint(int playerID, Vector3 position)
    {
        playerSpawnPositions[playerID] = position;

    }

    
    
    
    private void HandlePlayerDeath(int playerID)
    {

    }

    public void RespawnPlayer(int playerID)
    {
        if (!playerHealthMap.TryGetValue(playerID, out PlayerHealth healthComponent))
        {

            return;
        }

        if (!playerSpawnPositions.TryGetValue(playerID, out Vector3 respawnPos))
        {

            return;
        }

        if (logRespawnDebug)
        {
            var st = Environment.StackTrace;

        }
        GameObject playerObj = healthComponent.gameObject;
        CharacterController cc = playerObj.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        playerObj.transform.position = respawnPos;

        if (cc != null)
            cc.enabled = true;

        MovJugador1 mov1 = playerObj.GetComponent<MovJugador1>();
        MovJugador2 mov2 = playerObj.GetComponent<MovJugador2>();

        if (mov1 != null) mov1.ResetMovementState();
        if (mov2 != null) mov2.ResetMovementState();

        healthComponent.RestoreState();

        PlayerUIController uiController = playerObj.GetComponent<PlayerUIController>();
        if (uiController != null)
            uiController.HideRespawnPanel();


    }
}
