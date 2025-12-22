using UnityEngine;
using System.Collections;

public class FlashlightAnimationIntegrationTest : MonoBehaviour
{
    [Header("Test Components")]
    public FlashlightController_Enhanced flashlightController;
    public FlashlightAnimationController animationController;
    public PlayerInventory playerInventory;
    
    [Header("Test Settings")]
    public bool enableDebugMode = true;
    public float testDelay = 2f;
    
    private bool testInProgress = false;
    
    void Start()
    {
        if (enableDebugMode)
        {
            Debug.Log("[FlashlightAnimationIntegrationTest] Starting integration test");
        }
        
        StartCoroutine(RunIntegrationTest());
    }
    
    IEnumerator RunIntegrationTest()
    {
        if (testInProgress) yield break;
        testInProgress = true;
        
        // Wait for initialization
        yield return new WaitForSeconds(1f);
        
        // Test 1: Verify components are connected
        if (enableDebugMode)
        {
            Debug.Log("[FlashlightAnimationIntegrationTest] Test 1: Component connectivity");
            Debug.Log($"  - Flashlight Controller: {flashlightController != null}");
            Debug.Log($"  - Animation Controller: {animationController != null}");
            Debug.Log($"  - Player Inventory: {playerInventory != null}");
        }
        
        // Test 2: Test without flashlight in inventory
        if (enableDebugMode)
        {
            Debug.Log("[FlashlightAnimationIntegrationTest] Test 2: No flashlight in inventory");
        }
        
        yield return new WaitForSeconds(testDelay);
        
        // Test 3: Add flashlight to inventory
        if (playerInventory != null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[FlashlightAnimationIntegrationTest] Test 3: Adding flashlight to inventory");
            }
            
            playerInventory.AddItem("Flashlight");
        }
        
        yield return new WaitForSeconds(testDelay);
        
        // Test 4: Toggle flashlight on
        if (flashlightController != null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[FlashlightAnimationIntegrationTest] Test 4: Turning flashlight on");
            }
            
            flashlightController.StartFlashlightAnimation();
        }
        
        yield return new WaitForSeconds(testDelay);
        
        // Test 5: Toggle flashlight off
        if (flashlightController != null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[FlashlightAnimationIntegrationTest] Test 5: Turning flashlight off");
            }
            
            flashlightController.StartFlashlightAnimation();
        }
        
        yield return new WaitForSeconds(testDelay);
        
        // Test 6: Force state update
        if (flashlightController != null)
        {
            if (enableDebugMode)
            {
                Debug.Log("[FlashlightAnimationIntegrationTest] Test 6: Force state update");
            }
            
            flashlightController.ForceFlashlightState(true);
        }
        
        yield return new WaitForSeconds(testDelay);
        
        if (enableDebugMode)
        {
            Debug.Log("[FlashlightAnimationIntegrationTest] Integration test completed");
        }
        
        testInProgress = false;
    }
    
    void Update()
    {
        // Manual test controls
        if (Input.GetKeyDown(KeyCode.T) && !testInProgress)
        {
            StartCoroutine(RunIntegrationTest());
        }
        
        if (Input.GetKeyDown(KeyCode.F) && flashlightController != null)
        {
            flashlightController.StartFlashlightAnimation();
        }
        
        if (Input.GetKeyDown(KeyCode.R) && flashlightController != null)
        {
            flashlightController.ForceFlashlightState(!flashlightController.isFlashlightOn);
        }
    }
}