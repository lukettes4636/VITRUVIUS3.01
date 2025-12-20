using UnityEngine;

public class SecurityDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closedPosition;
    [SerializeField] private float moveSpeed = 2f;
    
    [Header("Key Configuration")]
    [SerializeField] private string requiredKeyID = "Tarjeta";
    
    [Header("Audio")]
    [SerializeField] private bool useAudioConfig = true;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject openEffect;
    [SerializeField] private GameObject lockedEffect;
    
    [Header("Light Control")]
    [SerializeField] private Light pcRoomLight;
    [SerializeField] private Color closedLightColor = Color.red;
    [SerializeField] private Color openLightColor = Color.green;
    
    private bool isOpen = false;
    private bool isMoving = false;
    private bool player1InRange = false;
    private bool player2InRange = false;
    
    private void Start()
    {
        if (doorTransform == null)
            doorTransform = transform;
            
        closedPosition = doorTransform.position;
        
        
        if (pcRoomLight != null)
        {
            pcRoomLight.color = closedLightColor;
        }
    }
    
    private void Update()
    {
        if (isMoving)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            doorTransform.position = Vector3.MoveTowards(doorTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(doorTransform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
        }
        
        CheckForKeyAndPlayers();
    }
    
    private void CheckForKeyAndPlayers()
    {
        if (isOpen || (!player1InRange && !player2InRange)) return;
        
        bool hasRequiredKey = false;
        
        if (player1InRange)
        {
            GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
            if (player1 != null)
            {
                PlayerInventory inventory = player1.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.HasKeyCard(requiredKeyID))
                {
                    hasRequiredKey = true;

                }
            }
        }
        
        if (!hasRequiredKey && player2InRange)
        {
            GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
            if (player2 != null)
            {
                PlayerInventory inventory = player2.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.HasKeyCard(requiredKeyID))
                {
                    hasRequiredKey = true;

                }
            }
        }
        
        if (hasRequiredKey)
        {
            OpenDoor();
        }
        else
        {
            PlayLockedSound();
        }
    }
    
    private void OpenDoor()
    {
        if (isOpen) return;
        
        isOpen = true;
        isMoving = true;
        
        if (useAudioConfig)
        {
            SoundHelper.PlayDoorOpenSound(transform.position);
        }
        else if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
        
        if (openEffect != null)
        {
            Instantiate(openEffect, transform.position, transform.rotation);
        }
        

        
        
        ChangeLightColor(openLightColor);
    }
    
    private void PlayLockedSound()
    {
        if (useAudioConfig)
        {
            SoundHelper.PlayDoorLockedSound(transform.position);
        }
        else if (lockedSound != null)
        {
            AudioSource.PlayClipAtPoint(lockedSound, transform.position);
        }
        
        if (lockedEffect != null)
        {
            Instantiate(lockedEffect, transform.position, transform.rotation);
        }
        

    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            player1InRange = true;
        }
        else if (other.CompareTag("Player2"))
        {
            player2InRange = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            player1InRange = false;
        }
        else if (other.CompareTag("Player2"))
        {
            player2InRange = false;
        }
    }
    
    
    private void ChangeLightColor(Color newColor)
    {
        if (pcRoomLight != null)
        {
            pcRoomLight.color = newColor;

        }
    }
}
