using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnhancedGameOverTests : MonoBehaviour
{
    [Header("Target Manager")]
    public EnhancedGameOverManager manager;

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("--- Starting Enhanced Game Over Tests ---");
        TestTypography();
        TestTextures();
        TestInputLogic();
        Debug.Log("--- Tests Complete ---");
    }

    public void TestTypography()
    {
        if (manager == null) manager = FindObjectOfType<EnhancedGameOverManager>();
        
        if (manager.larkeSansFont != null)
        {
            Debug.Log("[Test] Typography: Larke Sans Bold is assigned correctly.");
            if (manager.gameOverText != null && manager.gameOverText.font == manager.larkeSansFont)
            {
                Debug.Log("[Test] Typography: Font applied correctly to GameOverText.");
            }
            else
            {
                Debug.LogWarning("[Test] Typography: Font not applied to GameOverText yet (UI might not be created).");
            }
        }
        else
        {
            Debug.LogError("[Test] Typography: Larke Sans Bold is NOT assigned.");
        }
    }

    public void TestTextures()
    {
        if (manager == null) manager = FindObjectOfType<EnhancedGameOverManager>();

        if (manager.useTextTexture)
        {
            if (manager.textFaceTexture != null)
            {
                Debug.Log($"[Test] Textures: Background texture '{manager.textFaceTexture.name}' is assigned.");
                if (manager.gameOverText != null && manager.gameOverText.fontMaterial.GetTexture(ShaderUtilities.ID_FaceTex) == manager.textFaceTexture)
                {
                    Debug.Log("[Test] Textures: Texture applied correctly to GameOverText material.");
                }
            }
            else
            {
                Debug.LogWarning("[Test] Textures: Texture support enabled but no texture assigned.");
            }
        }
        else
        {
            Debug.Log("[Test] Textures: Texture support is disabled.");
        }
    }

    public void TestInputLogic()
    {
        if (manager == null) manager = FindObjectOfType<EnhancedGameOverManager>();

        // Check if GraphicRaycaster is disabled
        Canvas canvas = manager.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null && !raycaster.enabled)
            {
                Debug.Log("[Test] Input: GraphicRaycaster is disabled (Mouse/Touch activation blocked).");
            }
            else if (raycaster == null)
            {
                Debug.Log("[Test] Input: No GraphicRaycaster found (Mouse/Touch activation blocked).");
            }
            else
            {
                Debug.LogWarning("[Test] Input: GraphicRaycaster is still enabled!");
            }
        }

        Debug.Log("[Test] Input: Selection restricted to Gamepad Button South (A) in code. Manual verification required for physical input.");
    }
}
