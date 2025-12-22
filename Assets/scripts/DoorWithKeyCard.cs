using UnityEngine;
using UnityEngine.InputSystem;

public class DoorWithKeyCard : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyCardID = "Tarjeta";
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredCode = "1234";
    
    [Header("Door Movement")]
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closedPosition;
    [SerializeField] private float moveSpeed = 2f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject lockedEffect;
    [SerializeField] private GameObject unlockedEffect;
    
    [Header("Audio")]
    [SerializeField] private bool useAudioConfig = true;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;
    
    [Header("Code System")]
    [SerializeField] private Canvas codePanelCanvas;
    [SerializeField] private InputActionReference actionButtonPlayer1;
    [SerializeField] private InputActionReference actionButtonPlayer2;
    
    private bool isOpen = false;
    private bool isMoving = false;
    private bool playerInRange = false;
    private bool codeEnteredCorrectly = false;
    
    public string RequiredKeyCardID => requiredKeyCardID;
    public bool IsLocked => isLocked;
    
    private void Start()
    {
        closedPosition = transform.position;
        
        if (gameObject.tag != "Puerta")
        {
            gameObject.tag = "Puerta";
        }
        
        if (codePanelCanvas != null)
        {
            codePanelCanvas.gameObject.SetActive(false);
        }
        
        EnableInputActions();
    }
    
    private void OnEnable()
    {
        EnableInputActions();
    }
    
    private void OnDisable()
    {
        DisableInputActions();
    }
    
    private void EnableInputActions()
    {
        if (actionButtonPlayer1 != null) actionButtonPlayer1.action.Enable();
        if (actionButtonPlayer2 != null) actionButtonPlayer2.action.Enable();
    }
    
    private void DisableInputActions()
    {
        if (actionButtonPlayer1 != null) actionButtonPlayer1.action.Disable();
        if (actionButtonPlayer2 != null) actionButtonPlayer2.action.Disable();
    }
    
    private void Update()
    {
        if (isMoving)
        {
            Vector3 targetPosition = isOpen ? openPosition : closedPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
        }
        
        CheckActionButtonInput();
    }
    
    private void CheckActionButtonInput()
    {
        if (playerInRange)
        {
            bool player1Pressed = actionButtonPlayer1 != null && actionButtonPlayer1.action.WasPerformedThisFrame();
            bool player2Pressed = actionButtonPlayer2 != null && actionButtonPlayer2.action.WasPerformedThisFrame();
            
            if (player1Pressed || player2Pressed)
            {
                ToggleCodePanel();
            }
        }
    }
    
    private void ToggleCodePanel()
    {
        if (codePanelCanvas != null)
        {
            bool newState = !codePanelCanvas.gameObject.activeSelf;
            codePanelCanvas.gameObject.SetActive(newState);
            
            if (newState)
            {

            }
            else
            {

            }
        }
    }
    
    public bool TryOpen(GameObject player)
    {
        if (!isLocked && codeEnteredCorrectly)
        {
            OpenDoor();
            return true;
        }
        
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null && inventory.HasKeyCard(requiredKeyCardID))
        {
            if (codeEnteredCorrectly)
            {
                UnlockDoor();
                OpenDoor();
                return true;
            }
            else
            {

                return false;
            }
        }
        
        PlayLockedSound();
        return false;
    }
    
    public void SetCodeEntered(bool correct)
    {
        codeEnteredCorrectly = correct;
        if (correct)
        {

            if (codePanelCanvas != null)
            {
                codePanelCanvas.gameObject.SetActive(false);
            }
        }
        else
        {

        }
    }
    
    public bool ValidateCode(string inputCode)
    {
        bool isValid = inputCode == requiredCode;
        SetCodeEntered(isValid);
        return isValid;
    }
    
    public void UnlockDoor()
    {
        isLocked = false;
        
        if (unlockedEffect != null)
        {
            Instantiate(unlockedEffect, transform.position, transform.rotation);
        }
        
        if (useAudioConfig)
        {
            SoundHelper.PlayDoorUnlockSound(transform.position);
        }
        else if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }
        

    }
    
    public void OpenDoor()
    {
        if (isLocked) return;
        
        isOpen = true;
        isMoving = true;
        
        if (useAudioConfig)
        {
            SoundHelper.PlayDoorOpenSound(transform.position);
        }
        else if (openSound != null)
        {
            AudioManager.Instance.PlaySFX(openSound, transform.position, 0.8f, 1f);
        }
        

    }
    
    public void CloseDoor()
    {
        isOpen = false;
        isMoving = true;

    }
    
    public void LockDoor()
    {
        isLocked = true;
        codeEnteredCorrectly = false;
        CloseDoor();
        
        if (lockedEffect != null)
        {
            Instantiate(lockedEffect, transform.position, transform.rotation);
        }
        

    }
    
    private void PlayLockedSound()
    {
        if (useAudioConfig)
        {
            SoundHelper.PlayDoorLockedSound(transform.position);
        }
        else if (lockedSound != null)
        {
            AudioManager.Instance.PlaySFX(lockedSound, transform.position, 0.8f, 1f);
        }
        

    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = true;

        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            playerInRange = false;
            
            if (codePanelCanvas != null)
            {
                codePanelCanvas.gameObject.SetActive(false);
            }
            
            CloseDoor();
        }
    }
}
