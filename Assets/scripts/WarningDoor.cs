using UnityEngine;

public class WarningDoor : MonoBehaviour
{
    [Header("Warning Message")]
    [Tooltip("The text that will appear when the player collides with the Collider.")]
    [SerializeField] private string warningMessage = "I need to turn off the electricity!";

    
    [Header("Objeto de Bloqueo")]
    [Tooltip("Arrastra aqu el componente Collider que quieres desactivar para permitir el paso.")]
    [SerializeField] private Collider obstacleColliderToDisable;

    
    private Collider triggerCollider;

    void Start()
    {
        
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || !triggerCollider.isTrigger)
        {

        }

        
        if (obstacleColliderToDisable == null)
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (obstacleColliderToDisable != null && !obstacleColliderToDisable.enabled) return;

        
        PlayerUIController uiController = other.GetComponent<PlayerUIController>();
        if (uiController == null)
        {
            uiController = other.GetComponentInParent<PlayerUIController>();
        }

        if (uiController != null)
        {
            
            uiController.ShowNotification(warningMessage);
        }

        DialogueManager.ShowWarningDoorEnterDialogue(other.gameObject);
    }

    
    public void DeactivateBarrier()
    {
        
        if (obstacleColliderToDisable != null)
        {
            obstacleColliderToDisable.enabled = false;
        }

        
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        
        this.enabled = false;


    }
}
