using UnityEngine;

public class PlayerFlashlightHandler : MonoBehaviour
{
    [Header("Player Settings")]
    private bool isPlayer1 = false;
    private bool isPlayer2 = false;

    [Header("Flashlight Objects")]
    
    public GameObject player1FlashlightRoot;
    public GameObject player2FlashlightRoot;

    private PlayerInventory inventory;
    private bool hasFlashlight = false;

    
    private GameObject _flashlightRoot;
    private FlashlightController _controller;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();

        
        string playerName = gameObject.name.ToLower();
        isPlayer1 = playerName.Contains("player1") || playerName.Contains("1");
        isPlayer2 = playerName.Contains("player2") || playerName.Contains("2");

        
        if (isPlayer1 && player1FlashlightRoot != null)
        {
            _flashlightRoot = player1FlashlightRoot;
        }
        else if (isPlayer2 && player2FlashlightRoot != null)
        {
            _flashlightRoot = player2FlashlightRoot;
        }

        
        if (_flashlightRoot != null)
        {
            _controller = _flashlightRoot.GetComponent<FlashlightController>();
            _flashlightRoot.SetActive(false); 
        }

        
        if (_flashlightRoot != null && _controller == null)
            _controller = _flashlightRoot.GetComponent<FlashlightController>();
    }

    void Update()
    {
        CheckFlashlightStatus();
    }

    private void CheckFlashlightStatus()
    {
        bool shouldHaveFlashlight = inventory != null && inventory.HasItem("Flashlight");

        if (shouldHaveFlashlight != hasFlashlight)
        {
            hasFlashlight = shouldHaveFlashlight;
            UpdateFlashlightVisibility();
        }
    }

    private void UpdateFlashlightVisibility()
    {
        
        if (_flashlightRoot != null)
        {
            _flashlightRoot.SetActive(hasFlashlight);

            
            if (hasFlashlight && _controller != null)
            {
                
                _controller.SetFlashlightState(true, true);
            }
        }
    }
}
