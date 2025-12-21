using UnityEngine;
using VLB;
using System.Collections.Generic;

namespace Vitruvius.Graphics
{
    [DefaultExecutionOrder(100)] 
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class VLB_ImproveIntersection : MonoBehaviour
    {
        [Header("Target Objects")]
        [Tooltip("Partial names of objects to fix")]
        public string[] targetNames = new string[] { 
            "SpotSmoothIntersectOn (1)", 
            "SpotSmoothIntersectOn (2)",
            "SpotSmoothIntersectOn" 
        };

        [Header("Intersection Settings")]
        [Range(0.1f, 5.0f)]
        public float depthBlendDistance = 2.0f; 

        [Range(12, 64)]
        public int geometrySides = 48;

        [Header("Raycasting / Dynamic Occlusion")]
        public bool enableDynamicOcclusion = true;
        public LayerMask occlusionLayers = -1; 
        
        [Tooltip("Alignment: Beam = Cut Straight, Surface = Align with Wall (More Realistic)")]
        public PlaneAlignment planeAlignment = PlaneAlignment.Surface; 
        
        [Range(0.01f, 2.0f)]
        public float fadeDistanceToSurface = 0.75f; 

        [Range(0.0f, 1.0f)]
        [Tooltip("Values > 0.5 enable Multi-Ray detection (Up/Down/Left/Right)")]
        public float minSurfaceRatio = 0.75f; 

        [Header("Advanced Enhancer")]
        [Tooltip("Attach FlashlightOcclusionEnhancer for premium quality (Intensity modulation + Shadows)")]
        public bool removeEnhancerScript = false; 

        void Start()
        {
            ApplyFixes();
        }

        void OnValidate()
        {
           
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Fixes Now")]
#endif
        public void ApplyFixes()
        {
            var beams = FindObjectsOfType<VolumetricLightBeamSD>();
            int count = 0;

            foreach (var beam in beams)
            {
                if (IsTarget(beam.gameObject.name))
                {
                    OptimizeBeam(beam);
                    count++;
                }
            }

        }

        bool IsTarget(string name)
        {
            foreach (var target in targetNames)
            {
                if (name.Contains(target)) return true;
            }
            return false;
        }

        void OptimizeBeam(VolumetricLightBeamSD beam)
        {
            
            beam.depthBlendDistance = depthBlendDistance;

            
            beam.geomMeshType = MeshType.Custom;
            beam.geomCustomSides = geometrySides;
            
            
            if (!beam.isNoiseEnabled)
            {
                beam.noiseMode = NoiseMode.WorldSpace;
                beam.noiseIntensity = 0.12f;
                beam.noiseScaleLocal = 0.5f;
            }

            
            beam.UpdateAfterManualPropertyChange();

            
            if (enableDynamicOcclusion)
            {
                
                if (removeEnhancerScript)
                {
                    var enhancer = beam.GetComponent<FlashlightOcclusionEnhancer>();
                    if (enhancer) DestroyImmediate(enhancer);
                }

                var occlusion = beam.GetComponent<DynamicOcclusionRaycasting>();
                if (occlusion == null)
                {
                    occlusion = beam.gameObject.AddComponent<DynamicOcclusionRaycasting>();
                }
                
                if (occlusion != null) {
                    occlusion.layerMask = occlusionLayers;
                    occlusion.fadeDistanceToSurface = fadeDistanceToSurface;
                    occlusion.planeAlignment = planeAlignment; 
                    occlusion.minSurfaceRatio = minSurfaceRatio; 
                    occlusion.planeOffset = 0.1f; 
                    occlusion.minOccluderArea = 0.0f; 
                    occlusion.enabled = true;
                }
            }
        }
    }
}
