using UnityEngine;
using System.Collections;

public class FlashlightAnimationManualTest : MonoBehaviour
{
    [Header("Manual Test Controls")]
    [Tooltip("Press F to toggle flashlight animation")]
    public KeyCode toggleKey = KeyCode.F;
    
    [Tooltip("Press R to force flashlight state")]
    public KeyCode forceStateKey = KeyCode.R;
    
    [Tooltip("Press T to run integration test")]
    public KeyCode testKey = KeyCode.T;
    
    [Header("Test Components")]
    public FlashlightController_Enhanced flashlightController;
    public FlashlightAnimationController animationController;
    public PlayerInventory playerInventory;
    
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    
    private bool testRunning = false;
    
    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("[FlashlightAnimationManualTest] Manual test controls ready:");
            Debug.Log("  - Press F to toggle flashlight");
            Debug.Log("  - Press R to force flashlight state");
            Debug.Log("  - Press T to run automated test sequence");
            Debug.Log("  - Press I to add flashlight to inventory");
            Debug.Log("  - Press C to clear inventory");
        }
    }
    
    void Update()
    {
        // Manual controls
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
        
        if (Input.GetKeyDown(forceStateKey))
        {
            ForceFlashlightState();
        }
        
        if (Input.GetKeyDown(testKey) && !testRunning)
        {
            StartCoroutine(RunTestSequence());
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            AddFlashlightToInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearInventory();
        }
        
        // Display current state
        if (showDebugInfo && animationController != null)
        {
            DisplayCurrentState();
        }
    }
    
    void ToggleFlashlight()
    {
        if (flashlightController != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[FlashlightAnimationManualTest] Toggling flashlight (current state: {flashlightController.isFlashlightOn})");
            }
            
            flashlightController.StartFlashlightAnimation();
        }
        else
        {
            Debug.LogWarning("[FlashlightAnimationManualTest] Flashlight controller not assigned!");
        }
    }
    
    void ForceFlashlightState()
    {
        if (flashlightController != null)
        {
            bool newState = !flashlightController.isFlashlightOn;
            if (showDebugInfo)
            {
                Debug.Log($"[FlashlightAnimationManualTest] Forcing flashlight state to: {newState}");
            }
            
            flashlightController.ForceFlashlightState(newState);
        }
        else
        {
            Debug.LogWarning("[FlashlightAnimationManualTest] Flashlight controller not assigned!");
        }
    }
    
    void AddFlashlightToInventory()
    {
        if (playerInventory != null)
        {
            playerInventory.AddItem("Flashlight");
            if (showDebugInfo)
            {
                Debug.Log("[FlashlightAnimationManualTest] Added flashlight to inventory");
            }
        }
        else
        {
            Debug.LogWarning("[FlashlightAnimationManualTest] Player inventory not assigned!");
        }
    }
    
    void ClearInventory()
    {
        if (playerInventory != null)
        {
            // This would need a ClearAllItems method in PlayerInventory
            if (showDebugInfo)
            {
                Debug.Log("[FlashlightAnimationManualTest] Inventory clear requested (method may need implementation)");
            }
        }
    }
    
    IEnumerator RunTestSequence()
    {
        if (testRunning) yield break;
        testRunning = true;
        
        if (showDebugInfo)
        {
            Debug.Log("[FlashlightAnimationManualTest] Starting automated test sequence");
        }
        
        // Test 1: No flashlight in inventory
        if (showDebugInfo)
        {
            Debug.Log("Test 1: No flashlight in inventory - should not animate");
        }
        yield return new WaitForSeconds(1f);
        
        // Test 2: Add flashlight
        AddFlashlightToInventory();
        yield return new WaitForSeconds(1f);
        
        // Test 3: Toggle flashlight on
        if (showDebugInfo)
        {
            Debug.Log("Test 3: Toggling flashlight on - should start arm raise animation");
        }
        ToggleFlashlight();
        yield return new WaitForSeconds(2f);
        
        // Test 4: Toggle flashlight off
        if (showDebugInfo)
        {
            Debug.Log("Test 4: Toggling flashlight off - should start arm lower animation");
        }
        ToggleFlashlight();
        yield return new WaitForSeconds(2f);
        
        // Test 5: Force state
        if (showDebugInfo)
        {
            Debug.Log("Test 5: Forcing flashlight state - should update animation immediately");
        }
        ForceFlashlightState();
        yield return new WaitForSeconds(2f);
        
        if (showDebugInfo)
        {
            Debug.Log("[FlashlightAnimationManualTest] Test sequence completed");
        }
        
        testRunning = false;
    }
    
    void DisplayCurrentState()
    {
        if (Time.frameCount % 60 == 0) // Update every second at 60 FPS
        {
            string stateInfo = $"Flashlight State: {(flashlightController != null ? flashlightController.isFlashlightOn.ToString() : "N/A")}\n";
            stateInfo += $"Has Flashlight: {(playerInventory != null ? playerInventory.HasItem("Flashlight").ToString() : "N/A")}\n";
            
            if (animationController != null)
            {
                stateInfo += "Animation Controller: Active";
            }
            else
            {
                stateInfo += "Animation Controller: Not assigned";
            }
            
            Debug.Log($"[FlashlightAnimationManualTest] Current State:\n{stateInfo}");
        }
    }
}