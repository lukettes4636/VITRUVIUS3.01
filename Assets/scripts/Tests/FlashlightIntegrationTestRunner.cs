using UnityEngine;

public class FlashlightIntegrationTestRunner : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = true;
    public bool enableDetailedLogging = true;
    
    [Header("Component References")]
    public FlashlightController_Enhanced flashlightController;
    public FlashlightAnimationController animationController;
    public PlayerInventory playerInventory;
    
    private bool testsCompleted = false;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunIntegrationTests();
        }
    }
    
    [ContextMenu("Run Integration Tests")]
    public void RunIntegrationTests()
    {
        if (testsCompleted)
        {
            Debug.Log("[FlashlightIntegrationTestRunner] Tests already completed. Use Run Tests Again to re-run.");
            return;
        }
        
        Debug.Log("=== FLASHLIGHT ANIMATION INTEGRATION TESTS STARTING ===");
        
        // Test 1: Component Validation
        TestComponentValidation();
        
        // Test 2: Inventory Integration
        TestInventoryIntegration();
        
        // Test 3: State Synchronization
        TestStateSynchronization();
        
        // Test 4: Animation Triggering
        TestAnimationTriggering();
        
        Debug.Log("=== FLASHLIGHT ANIMATION INTEGRATION TESTS COMPLETED ===");
        testsCompleted = true;
    }
    
    [ContextMenu("Run Tests Again")]
    public void RunTestsAgain()
    {
        testsCompleted = false;
        RunIntegrationTests();
    }
    
    void TestComponentValidation()
    {
        Debug.Log("\n--- TEST 1: COMPONENT VALIDATION ---");
        
        bool allComponentsValid = true;
        
        // Validate FlashlightController_Enhanced
        if (flashlightController == null)
        {
            Debug.LogError("❌ FlashlightController_Enhanced is null!");
            allComponentsValid = false;
        }
        else
        {
            Debug.Log("✅ FlashlightController_Enhanced found");
            
            // Check if it has the animation controller reference
            if (flashlightController.animationController == null)
            {
                Debug.LogWarning("⚠️  FlashlightController_Enhanced.animationController is null - integration may not work");
            }
            else
            {
                Debug.Log("✅ FlashlightController_Enhanced has animation controller reference");
            }
        }
        
        // Validate FlashlightAnimationController
        if (animationController == null)
        {
            Debug.LogError("❌ FlashlightAnimationController is null!");
            allComponentsValid = false;
        }
        else
        {
            Debug.Log("✅ FlashlightAnimationController found");
        }
        
        // Validate PlayerInventory
        if (playerInventory == null)
        {
            Debug.LogError("❌ PlayerInventory is null!");
            allComponentsValid = false;
        }
        else
        {
            Debug.Log("✅ PlayerInventory found");
        }
        
        if (allComponentsValid)
        {
            Debug.Log("✅ All components validated successfully");
        }
        else
        {
            Debug.LogError("❌ Component validation failed - fix references before continuing");
        }
    }
    
    void TestInventoryIntegration()
    {
        Debug.Log("\n--- TEST 2: INVENTORY INTEGRATION ---");
        
        if (playerInventory == null)
        {
            Debug.LogError("❌ Cannot test inventory integration - PlayerInventory is null");
            return;
        }
        
        // Test HasItem method
        bool hasFlashlightInitially = playerInventory.HasItem("Flashlight");
        Debug.Log($"Initial flashlight status: {(hasFlashlightInitially ? "HAS FLASHLIGHT" : "NO FLASHLIGHT")}");
        
        // Test adding flashlight
        playerInventory.AddItem("Flashlight");
        bool hasFlashlightAfterAdd = playerInventory.HasItem("Flashlight");
        Debug.Log($"After adding flashlight: {(hasFlashlightAfterAdd ? "HAS FLASHLIGHT" : "NO FLASHLIGHT")}");
        
        if (hasFlashlightAfterAdd)
        {
            Debug.Log("✅ Inventory integration working correctly");
        }
        else
        {
            Debug.LogError("❌ Inventory integration failed - flashlight not added");
        }
    }
    
    void TestStateSynchronization()
    {
        Debug.Log("\n--- TEST 3: STATE SYNCHRONIZATION ---");
        
        if (flashlightController == null || animationController == null)
        {
            Debug.LogError("❌ Cannot test state synchronization - missing components");
            return;
        }
        
        // Get initial states
        bool initialFlashlightState = flashlightController.isFlashlightOn;
        
        Debug.Log($"Initial flashlight state: {initialFlashlightState}");
        Debug.Log($"Animation controller status: {(animationController != null ? "ACTIVE" : "INACTIVE")}");
        
        // Test state change
        if (flashlightController.isFlashlightOn == false)
        {
            Debug.Log("Testing state change: OFF -> ON");
            flashlightController.ForceFlashlightState(true);
            
            if (flashlightController.isFlashlightOn == true)
            {
                Debug.Log("✅ Flashlight state changed successfully");
            }
            else
            {
                Debug.LogError("❌ Flashlight state change failed");
            }
        }
        else
        {
            Debug.Log("Testing state change: ON -> OFF");
            flashlightController.ForceFlashlightState(false);
            
            if (flashlightController.isFlashlightOn == false)
            {
                Debug.Log("✅ Flashlight state changed successfully");
            }
            else
            {
                Debug.LogError("❌ Flashlight state change failed");
            }
        }
        
        Debug.Log("✅ State synchronization test completed");
    }
    
    void TestAnimationTriggering()
    {
        Debug.Log("\n--- TEST 4: ANIMATION TRIGGERING ---");
        
        if (flashlightController == null || animationController == null)
        {
            Debug.LogError("❌ Cannot test animation triggering - missing components");
            return;
        }
        
        // Ensure player has flashlight
        if (playerInventory != null && !playerInventory.HasItem("Flashlight"))
        {
            playerInventory.AddItem("Flashlight");
            Debug.Log("Added flashlight to inventory for animation test");
        }
        
        // Test animation triggering
        Debug.Log("Testing animation trigger via StartFlashlightAnimation()");
        
        // Get current state before trigger
        bool stateBefore = flashlightController.isFlashlightOn;
        
        // Trigger animation
        flashlightController.StartFlashlightAnimation();
        
        // Check if state changed
        bool stateAfter = flashlightController.isFlashlightOn;
        
        if (stateBefore != stateAfter)
        {
            Debug.Log($"✅ Animation triggered successfully - state changed from {stateBefore} to {stateAfter}");
        }
        else
        {
            Debug.LogWarning("⚠️  Animation triggered but state didn't change (may be normal)");
        }
        
        // Test with animation controller reference
        if (flashlightController.animationController != null)
        {
            Debug.Log("✅ Animation controller is properly linked to flashlight controller");
        }
        else
        {
            Debug.LogWarning("⚠️  Animation controller reference may need to be assigned");
        }
        
        Debug.Log("✅ Animation triggering test completed");
    }
    
    void OnGUI()
    {
        if (!enableDetailedLogging) return;
        
        // Display current test status
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("Flashlight Animation Integration Test", GUI.skin.box);
        
        if (testsCompleted)
        {
            GUILayout.Label("✅ Tests Completed");
        }
        else
        {
            GUILayout.Label("🔄 Tests Ready to Run");
        }
        
        // Display current states
        if (flashlightController != null)
        {
            GUILayout.Label($"Flashlight State: {(flashlightController.isFlashlightOn ? "ON" : "OFF")}");
        }
        
        if (playerInventory != null)
        {
            GUILayout.Label($"Has Flashlight: {(playerInventory.HasItem("Flashlight") ? "YES" : "NO")}");
        }
        
        if (animationController != null)
        {
            GUILayout.Label("Animation Controller: ACTIVE");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}