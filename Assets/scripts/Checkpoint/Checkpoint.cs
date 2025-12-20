using UnityEngine;
using System.Collections.Generic; 
using System.Linq; 

public class Checkpoint : MonoBehaviour
{
    
    
    
    
    public delegate void CheckpointReached(int playerID, Vector3 position);
    public static event CheckpointReached OnCheckpointReached;


    
    
    
    [Header("Checkpoint Settings")]
    public int checkpointID;

    [Tooltip("La capa que identifica a los jugadores (Layer: Player).")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("El objeto visual (ej: una luz) que se activa al guardar el punto.")]
    [SerializeField] private GameObject activationVisual;

    [Tooltip("Si se debe desactivar el Collider cuando AMBOS jugadores lo han activado.")]
    [SerializeField] private bool disableColliderWhenBothUsed = true;


    
    
    
    
    private Dictionary<int, bool> playerActivationStatus = new Dictionary<int, bool>();


    private void Awake()
    {
        
        if (activationVisual != null)
        {
            activationVisual.SetActive(false);
        }

        
        playerActivationStatus.Add(1, false);
        playerActivationStatus.Add(2, false);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();

            if (playerIdentifier != null)
            {
                int playerID = playerIdentifier.playerID;

                
                
                if (playerActivationStatus.ContainsKey(playerID) && playerActivationStatus[playerID] == true)
                {
                    
                    
                    return;
                }

                
                OnCheckpointReached?.Invoke(playerID, transform.position);

                

                
                playerActivationStatus[playerID] = true;

                
                
                bool bothActivated = playerActivationStatus.Values.All(activated => activated);

                if (bothActivated)
                {
                    
                    if (activationVisual != null)
                    {
                        activationVisual.SetActive(true);
                    }

                    
                    if (disableColliderWhenBothUsed)
                    {
                        Collider col = GetComponent<Collider>();
                        if (col != null) col.enabled = false;
                    }


                }
                

                
                
                PlayerUIController uiController = other.GetComponent<PlayerUIController>();
                if (uiController != null)
                {
                    uiController.ShowNotification($"Checkpoint saved");
                }
            }
            else
            {
                

            }
        }
    }
}
