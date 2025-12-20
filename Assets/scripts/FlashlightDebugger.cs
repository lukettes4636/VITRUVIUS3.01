using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showChildObjects = true;
    
    [Header("Input Test")]
    [SerializeField] private bool testInput = false;
    
    private FlashlightController flashlightController;
    private FlashlightController_Enhanced flashlightControllerEnhanced;
    private PlayerInventory playerInventory;
    
    void Start()
    {
        flashlightController = GetComponent<FlashlightController>();
        flashlightControllerEnhanced = GetComponent<FlashlightController_Enhanced>();
        playerInventory = GetComponentInParent<PlayerInventory>();
        
        if (showDebugLogs)
        {

            LogFlashlightStatus();
        }
    }
    
    void Update()
    {
        if (testInput && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
        
        if (showDebugLogs && Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame)
        {

        }
    }
    
    public void ToggleFlashlight()
    {
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightState(!flashlightController.isFlashlightOn, true);
            if (showDebugLogs)
            {

                LogFlashlightStatus();
            }
        }
        else if (flashlightControllerEnhanced != null)
        {
            flashlightControllerEnhanced.SetFlashlightState(!flashlightControllerEnhanced.isFlashlightOn, true);
            if (showDebugLogs)
            {

                LogFlashlightStatus();
            }
        }
        else
        {

        }
    }
    
    public void LogFlashlightStatus()
    {

        
        if (playerInventory != null)
        {
            bool hasFlashlight = playerInventory.HasItem("Flashlight");

        }
        
        if (flashlightController != null)
        {

        }
        
        if (flashlightControllerEnhanced != null)
        {

        }
        
        if (showChildObjects)
        {
            LogAllChildObjects();
        }
        

    }
    
    private void LogAllChildObjects()
    {

        
        Light[] allLights = GetComponentsInChildren<Light>(true);

        foreach (Light light in allLights)
        {

        }
        
        Behaviour[] allBehaviours = GetComponentsInChildren<Behaviour>(true);

        foreach (Behaviour behaviour in allBehaviours)
        {
            if (behaviour.GetType().Name.Contains("Spotlight") || 
                behaviour.GetType().Name.Contains("Light") ||
                behaviour.GetType().Name.Contains("VLB"))
            {

            }
        }
        
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in allRenderers)
        {

        }
        
        GameObject[] allChildren = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            allChildren[i] = transform.GetChild(i).gameObject;
        }
        

        foreach (GameObject child in allChildren)
        {

        }
    }
    
    [ContextMenu("Force Toggle On")]
    private void ForceToggleOn()
    {
        if (flashlightController != null)
            flashlightController.SetFlashlightState(true, true);
        if (flashlightControllerEnhanced != null)
            flashlightControllerEnhanced.SetFlashlightState(true, true);
        LogFlashlightStatus();
    }
    
    [ContextMenu("Force Toggle Off")]
    private void ForceToggleOff()
    {
        if (flashlightController != null)
            flashlightController.SetFlashlightState(false, true);
        if (flashlightControllerEnhanced != null)
            flashlightControllerEnhanced.SetFlashlightState(false, true);
        LogFlashlightStatus();
    }
    
    [ContextMenu("Log Status")]
    private void DebugLogStatus()
    {
        LogFlashlightStatus();
    }
}
