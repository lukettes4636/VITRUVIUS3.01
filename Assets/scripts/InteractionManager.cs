using UnityEngine;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;
    
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask interactionLayer;
    
    private List<InteractiveObject> interactiveObjects = new List<InteractiveObject>();
    private InteractiveObject currentInteractable;
    
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
    
    public void RegisterInteractable(InteractiveObject interactable)
    {
        if (!interactiveObjects.Contains(interactable))
        {
            interactiveObjects.Add(interactable);
        }
    }
    
    public void UnregisterInteractable(InteractiveObject interactable)
    {
        if (interactiveObjects.Contains(interactable))
        {
            interactiveObjects.Remove(interactable);
        }
    }
    
    public InteractiveObject GetNearestInteractable(Vector3 position)
    {
        InteractiveObject nearest = null;
        float minDistance = Mathf.Infinity;
        
        foreach (InteractiveObject interactable in interactiveObjects)
        {
            if (interactable.CanInteract())
            {
                float distance = Vector3.Distance(position, interactable.transform.position);
                if (distance < minDistance && distance <= interactionRange)
                {
                    minDistance = distance;
                    nearest = interactable;
                }
            }
        }
        
        return nearest;
    }
    
    public string GetInteractionMessage(Vector3 position)
    {
        InteractiveObject interactable = GetNearestInteractable(position);
        return interactable != null ? interactable.GetInteractionMessage() : "";
    }
    
    public bool TryInteract(Vector3 position)
    {
        InteractiveObject interactable = GetNearestInteractable(position);
        if (interactable != null)
        {
            float distance = Vector3.Distance(position, interactable.transform.position);
            if (distance <= interactionRange)
            {

                interactable.Interact();
                return true;
            }
        }
        return false;
    }
}
