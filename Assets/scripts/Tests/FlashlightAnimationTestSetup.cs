using UnityEngine;

[CreateAssetMenu(fileName = "FlashlightAnimationTestSetup", menuName = "Tests/Flashlight Animation Setup")]
public class FlashlightAnimationTestSetup : ScriptableObject
{
    [Header("Test Configuration")]
    [TextArea(3, 10)]
    public string setupInstructions = @"
SETUP INSTRUCTIONS FOR FLASHLIGHT ANIMATION TESTING:

1. Create a new GameObject in your scene called 'FlashlightTestManager'

2. Add the following components to the GameObject:
   - FlashlightAnimationIntegrationTest
   - FlashlightAnimationManualTest

3. Assign the following references in the inspector:
   - Flashlight Controller: Find your FlashlightController_Enhanced in the scene
   - Animation Controller: Find your FlashlightAnimationController in the scene  
   - Player Inventory: Find your PlayerInventory component

4. Configure the test settings:
   - Enable Debug Mode: Check this for detailed logging
   - Test Delay: Set to 2.0 for normal testing speed
   - Show Debug Info: Check this for real-time state display

5. Test Controls:
   - F: Toggle flashlight (should trigger animation)
   - R: Force flashlight state (immediate state change)
   - T: Run automated test sequence
   - I: Add flashlight to inventory
   - C: Clear inventory (if implemented)

6. Expected Test Results:
   - Without flashlight: No animation should play
   - With flashlight: Arm should raise/lower smoothly
   - State changes: Should be synchronized with animation
   - Collision detection: Should prevent arm from clipping through objects

7. Debug Information:
   - Check console for detailed logs
   - Look for '[FlashlightAnimationController]' messages
   - Verify state transitions are logged correctly

8. Troubleshooting:
   - If animations don't play: Check animator parameter names
   - If states don't sync: Verify component references
   - If inventory check fails: Ensure PlayerInventory has HasItem method
";

    [Header("Quick Setup Checklist")]
    public bool flashlightControllerAssigned;
    public bool animationControllerAssigned;
    public bool playerInventoryAssigned;
    public bool testComponentsAdded;
    public bool debugModeEnabled;
    
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== FLASHLIGHT ANIMATION TEST SETUP VALIDATION ===");
        
        // Find test objects in scene
        GameObject testManager = GameObject.Find("FlashlightTestManager");
        
        if (testManager == null)
        {
            Debug.LogError("❌ FlashlightTestManager GameObject not found! Create it first.");
            return;
        }
        
        Debug.Log($"✅ Found FlashlightTestManager: {testManager.name}");
        
        // Check for test components
        FlashlightAnimationIntegrationTest integrationTest = testManager.GetComponent<FlashlightAnimationIntegrationTest>();
        FlashlightAnimationManualTest manualTest = testManager.GetComponent<FlashlightAnimationManualTest>();
        
        if (integrationTest == null)
        {
            Debug.LogError("❌ FlashlightAnimationIntegrationTest component missing!");
        }
        else
        {
            Debug.Log("✅ FlashlightAnimationIntegrationTest component found");
            
            // Validate references
            if (integrationTest.flashlightController == null)
            {
                Debug.LogWarning("⚠️  FlashlightController_Enhanced reference not assigned in integration test");
            }
            else
            {
                Debug.Log("✅ FlashlightController_Enhanced reference assigned");
            }
            
            if (integrationTest.animationController == null)
            {
                Debug.LogWarning("⚠️  FlashlightAnimationController reference not assigned in integration test");
            }
            else
            {
                Debug.Log("✅ FlashlightAnimationController reference assigned");
            }
            
            if (integrationTest.playerInventory == null)
            {
                Debug.LogWarning("⚠️  PlayerInventory reference not assigned in integration test");
            }
            else
            {
                Debug.Log("✅ PlayerInventory reference assigned");
            }
        }
        
        if (manualTest == null)
        {
            Debug.LogError("❌ FlashlightAnimationManualTest component missing!");
        }
        else
        {
            Debug.Log("✅ FlashlightAnimationManualTest component found");
            
            // Validate manual test references
            if (manualTest.flashlightController == null)
            {
                Debug.LogWarning("⚠️  FlashlightController_Enhanced reference not assigned in manual test");
            }
            
            if (manualTest.animationController == null)
            {
                Debug.LogWarning("⚠️  FlashlightAnimationController reference not assigned in manual test");
            }
            
            if (manualTest.playerInventory == null)
            {
                Debug.LogWarning("⚠️  PlayerInventory reference not assigned in manual test");
            }
        }
        
        // Check for common issues
        Debug.Log("\n=== COMMON ISSUES CHECK ===");
        
        // Find all relevant components in scene
        FlashlightController_Enhanced[] flashlightControllers = FindObjectsOfType<FlashlightController_Enhanced>();
        FlashlightAnimationController[] animationControllers = FindObjectsOfType<FlashlightAnimationController>();
        PlayerInventory[] playerInventories = FindObjectsOfType<PlayerInventory>();
        
        Debug.Log($"Found {flashlightControllers.Length} FlashlightController_Enhanced in scene");
        Debug.Log($"Found {animationControllers.Length} FlashlightAnimationController in scene");
        Debug.Log($"Found {playerInventories.Length} PlayerInventory in scene");
        
        if (flashlightControllers.Length == 0)
        {
            Debug.LogError("❌ No FlashlightController_Enhanced found in scene!");
        }
        
        if (animationControllers.Length == 0)
        {
            Debug.LogError("❌ No FlashlightAnimationController found in scene!");
        }
        
        if (playerInventories.Length == 0)
        {
            Debug.LogError("❌ No PlayerInventory found in scene!");
        }
        
        Debug.Log("\n=== VALIDATION COMPLETE ===");
        Debug.Log("Check the warnings above and assign missing references in the inspector.");
    }
    
    [ContextMenu("Auto-Assign References")]
    public void AutoAssignReferences()
    {
        GameObject testManager = GameObject.Find("FlashlightTestManager");
        
        if (testManager == null)
        {
            Debug.LogError("FlashlightTestManager not found! Create it first.");
            return;
        }
        
        FlashlightAnimationIntegrationTest integrationTest = testManager.GetComponent<FlashlightAnimationIntegrationTest>();
        FlashlightAnimationManualTest manualTest = testManager.GetComponent<FlashlightAnimationManualTest>();
        
        // Find components in scene
        FlashlightController_Enhanced flashlightController = FindObjectOfType<FlashlightController_Enhanced>();
        FlashlightAnimationController animationController = FindObjectOfType<FlashlightAnimationController>();
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        
        if (flashlightController == null)
        {
            Debug.LogError("Could not find FlashlightController_Enhanced in scene!");
        }
        else
        {
            if (integrationTest != null) integrationTest.flashlightController = flashlightController;
            if (manualTest != null) manualTest.flashlightController = flashlightController;
            Debug.Log("✅ Auto-assigned FlashlightController_Enhanced");
        }
        
        if (animationController == null)
        {
            Debug.LogError("Could not find FlashlightAnimationController in scene!");
        }
        else
        {
            if (integrationTest != null) integrationTest.animationController = animationController;
            if (manualTest != null) manualTest.animationController = animationController;
            Debug.Log("✅ Auto-assigned FlashlightAnimationController");
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("Could not find PlayerInventory in scene!");
        }
        else
        {
            if (integrationTest != null) integrationTest.playerInventory = playerInventory;
            if (manualTest != null) manualTest.playerInventory = playerInventory;
            Debug.Log("✅ Auto-assigned PlayerInventory");
        }
        
        Debug.Log("Auto-assignment complete. Check inspector to verify references.");
    }
}