using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Checkpoint : MonoBehaviour
{
    public delegate void CheckpointReached(int playerID, Vector3 position);
    public static event CheckpointReached OnCheckpointReached;

    [Header("Checkpoint Settings")]
    public int checkpointID;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject activationVisual;
    [SerializeField] private bool disableColliderWhenBothUsed = true;

    private Dictionary<int, bool> playerActivationStatus = new Dictionary<int, bool>();
    private Collider checkpointCollider;

    private void Awake()
    {
        if (activationVisual != null)
            activationVisual.SetActive(false);

        playerActivationStatus.Add(1, false);
        playerActivationStatus.Add(2, false);

        checkpointCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0)
            return;

        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();
        if (playerIdentifier == null)
            return;

        int playerID = playerIdentifier.playerID;

        if (playerActivationStatus.ContainsKey(playerID) && playerActivationStatus[playerID])
            return;

        OnCheckpointReached?.Invoke(playerID, transform.position);

        playerActivationStatus[playerID] = true;

        bool bothActivated = playerActivationStatus.Values.All(activated => activated);

        if (bothActivated)
        {
            if (activationVisual != null)
                activationVisual.SetActive(true);

            if (disableColliderWhenBothUsed && checkpointCollider != null)
                checkpointCollider.enabled = false;
        }

        PlayerUIController uiController = other.GetComponent<PlayerUIController>();
        if (uiController != null)
            uiController.ShowNotification("Checkpoint saved");
    }
}