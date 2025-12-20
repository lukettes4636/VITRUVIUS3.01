using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using VLB;

public class AdjustSpotlightPosition : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow<AdjustSpotlightPosition>("Adjust Spotlight Positions");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Adjust Positions in DaVinciPB"))
        {
            AdjustPositions();
        }
    }

    private void AdjustPositions()
    {
        Scene davinciScene = SceneManager.GetSceneByName("DaVinciPB");
        if (!davinciScene.isLoaded)
        {
            Debug.LogError("Scene 'DaVinciPB' is not loaded. Please open the scene and try again.");
            return;
        }

        int adjustedCount = 0;
        GameObject[] rootObjects = davinciScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            Transform[] allChildren = rootObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name.StartsWith("BeamWithOcclusionRaycasting"))
                {
                    // Check if the object is a child of Player1 or Player2
                    bool isPlayerLight = false;
                    Transform parent = child.parent;
                    while (parent != null)
                    {
                        if (parent.name == "Player1" || parent.name == "Player2")
                        {
                            isPlayerLight = true;
                            break;
                        }
                        parent = parent.parent;
                    }

                    if (isPlayerLight)
                    {
                        continue;
                    }

                    Transform spotlight = child.Find("SpotSmoothIntersectOn (1) Variant");
                    if (spotlight != null)
                    {
                        Debug.Log($"Found spotlight '{spotlight.name}' for '{child.name}'.");
                        spotlight.position = child.position;
                        spotlight.rotation = child.rotation;

                        var parentBeam = child.GetComponent<VolumetricLightBeamSD>();
                        var spotlightBeam = spotlight.GetComponent<VolumetricLightBeamSD>();

                        if (parentBeam == null)
                        {
                            Debug.LogWarning($"Parent '{child.name}' does not have a VolumetricLightBeamSD component.");
                        }

                        if (spotlightBeam == null)
                        {
                            Debug.LogWarning($"Spotlight '{spotlight.name}' does not have a VolumetricLightBeamSD component.");
                        }

                        if (parentBeam != null && spotlightBeam != null)
                        {
                            Debug.Log($"Parent '{child.name}' color is {parentBeam.color}.");
                            Debug.Log($"Spotlight '{spotlight.name}' color before change is {spotlightBeam.color}.");
                            spotlightBeam.color = parentBeam.color;
                            Debug.Log($"Spotlight '{spotlight.name}' color after change is {spotlightBeam.color}.");
                            spotlightBeam.UpdateAfterManualPropertyChange();
                        }

                        adjustedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find 'SpotSmoothIntersectOn (1) Variant' child for '{child.name}'.");
                    }
                }
            }
        }

        Debug.Log($"Adjusted {adjustedCount} spotlight positions in the 'DaVinciPB' scene.");
    }
}
