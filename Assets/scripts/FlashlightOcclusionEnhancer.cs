using UnityEngine;
using VLB; 

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class FlashlightOcclusionEnhancer : MonoBehaviour
{
    [Header("Dynamic Occlusion Settings")]
    [Tooltip("Habilitar oclusion dinamica para evitar que la luz atraviese objetos")]
    public bool enableDynamicOcclusion = true;
    
    [Tooltip("Capas que pueden ocluir la luz (selecciona paredes, muebles, etc.)")]
    public LayerMask occlusionLayers = -1;
    
    [Tooltip("Distancia de fade para transiciones suaves en bordes")]
    [Range(0.01f, 0.5f)]
    public float occlusionFadeDistance = 0.1f;
    
    [Tooltip("Calidad de la oclusion (mayor = mas preciso pero mas costoso)")]
    [Range(0.1f, 2f)]
    public float occlusionQuality = 1.0f;

    [Header("Contact Shadows Enhancement")]
    [Tooltip("Intensidad de sombras de contacto para detalles finos")]
    [Range(0f, 1f)]
    public float contactShadowIntensity = 0.3f;
    
    [Tooltip("Suavizado de bordes de sombras de contacto")]
    [Range(0.1f, 2f)]
    public float contactShadowSoftness = 0.8f;
    
    [Tooltip("Distancia maxima para sombras de contacto")]
    [Range(0.1f, 2f)]
    public float contactShadowDistance = 0.5f;

    [Header("Beam Optimization")]
    [Tooltip("Intensidad del haz volumetrico cuando esta ocluido")]
    [Range(0f, 1f)]
    public float occludedBeamIntensity = 0.2f;
    
    [Tooltip("Atenuacion progresiva del haz al acercarse a superficies")]
    [Range(0.1f, 5f)]
    public float surfaceAttenuation = 1.5f;
    
    [Tooltip("Habilitar efectos de polvo/particulas en el haz")]
    public bool enableDustParticles = true;

    [Header("Proximity Occlusion")]
    [Tooltip("Habilitar oclusion por proximidad para evitar que la luz atraviese paredes cercanas")]
    public bool enableProximityOcclusion = true;
    
    [Tooltip("Distancia a la que se activa la oclusion por proximidad")]
    [Range(0.05f, 0.5f)]
    public float proximityOcclusionDistance = 0.12f;
    
    [Tooltip("Radio de deteccion de proximidad")]
    [Range(0.1f, 1f)]
    public float proximityDetectionRadius = 0.35f;
    
    [Tooltip("Capas para deteccion de proximidad")]
    public LayerMask proximityLayers = -1;

    [Header("Performance Settings")]
    [Tooltip("Frecuencia de actualizacion de la oclusion (frames)")]
    [Range(1, 10)]
    public int occlusionUpdateRate = 2;
    
    [Tooltip("Maxima distancia para deteccion de oclusion")]
    [Range(5f, 50f)]
    public float maxOcclusionDistance = 20f;

    [Header("Advanced Occlusion Settings")]
    [Tooltip("Usar deteccion multiple de rayos para mejor precision")]
    public bool useMultiRayDetection = true;
    
    [Tooltip("Cantidad de rayos para deteccion multiple")]
    [Range(3, 16)]
    public int multiRayCount = 9;
    
    [Tooltip("Radio de dispersion para rayos multiples")]
    [Range(0.01f, 0.5f)]
    public float multiRaySpread = 0.12f;
    
    [Tooltip("Habilitar oclusion volumetrica avanzada")]
    public bool enableVolumetricOcclusion = true;
    
    [Tooltip("Intensidad de oclusion volumetrica")]
    [Range(0.1f, 3f)]
    public float volumetricOcclusionIntensity = 1.2f;
    
    [Header("Beam Smoothing & Contact Enhancement")]
    [Tooltip("Suavizado de transiciones de oclusion (mayor = mas suave)")]
    [Range(0.1f, 5f)]
    public float occlusionSmoothness = 0.8f;
    
    [Tooltip("Habilitar deteccion de bordes para mejor contacto visual")]
    public bool enableEdgeDetection = true;
    
    [Tooltip("Sensibilidad de deteccion de bordes")]
    [Range(0.01f, 0.3f)]
    public float edgeDetectionSensitivity = 0.08f;
    
    [Tooltip("Suavizado de bordes detectados")]
    [Range(0.1f, 2f)]
    public float edgeSmoothing = 0.6f;
    
    [Tooltip("Habilitar micro-variaciones para efecto mas organico")]
    public bool enableMicroVariations = true;
    
    [Tooltip("Intensidad de variaciones microscopicas")]
    [Range(0.01f, 0.2f)]
    public float microVariationIntensity = 0.05f;
    
    [Tooltip("Frecuencia de variaciones microscopicas")]
    [Range(0.1f, 5f)]
    public float microVariationFrequency = 1.5f;

    
    private Light flashlightLight;
    private VLB.VolumetricLightBeamSD volumetricBeam;
    private VLB.DynamicOcclusionRaycasting dynamicOcclusion;
    private VLB.VolumetricDustParticles dustParticles;

    
    private bool isOccluded = false;
    private float currentBeamIntensity;
    private float originalBeamIntensity;
    private int frameCount = 0;
    
    private Vector3[] multiRayDirections;
    private float[] rayOcclusionFactors;
    private float averageOcclusionFactor = 0f;
    private float smoothedOcclusionFactor = 0f;
    private float edgeDetectionFactor = 0f;
    private float microVariationOffset = 0f;
    private bool isProximityOccluded = false;
    private float proximityOcclusionFactor = 0f;

    private Transform flashlightModel;
    private Renderer[] modelRenderers;
    private bool isModelDisabled = false;

    void OnEnable()
    {
        InitializeComponents();
        SetupDynamicOcclusion();
        CacheOriginalValues();
        InitializeMultiRaySystem();
    }

    void Start()
    {
        InitializeComponents();
        SetupDynamicOcclusion();
        CacheOriginalValues();

    if (volumetricBeam != null) {
        volumetricBeam.intensityModeAdvanced = true;
    }

    string modelName = transform.root.name.Contains("Player1") ? "flashlightfbx - Copy" : "flashlightfbx";
        flashlightModel = transform.root.Find(modelName);
        if (flashlightModel == null) {

        } else {
            modelRenderers = flashlightModel.GetComponentsInChildren<Renderer>();
        }
    }

    void Update()
    {
        frameCount++;
        
        
        if (enableDynamicOcclusion && frameCount % occlusionUpdateRate == 0)
        {
            UpdateOcclusion();
        }

        
        UpdateSmoothFactors();
        
        
        if (enableProximityOcclusion)
        {
            UpdateProximityOcclusion();
        }
        
        ApplyVisualEffects();
    }

    private void UpdateEdgeDetection()
    {
        
        float occlusionVariation = Mathf.Abs(smoothedOcclusionFactor - averageOcclusionFactor);
        
        
        if (occlusionVariation > edgeDetectionSensitivity)
        {
            edgeDetectionFactor = Mathf.Lerp(edgeDetectionFactor, 1.0f, Time.deltaTime * edgeSmoothing);
        }
        else
        {
            edgeDetectionFactor = Mathf.Lerp(edgeDetectionFactor, 0.0f, Time.deltaTime * edgeSmoothing * 2f);
        }
        
        
        edgeDetectionFactor = Mathf.Clamp01(edgeDetectionFactor);
    }

    private float CalculateSurfaceFactor(RaycastHit hit)
    {
        
        float surfaceFactor = 1.0f;
        
        
        if (hit.collider.material != null)
        {
            
            surfaceFactor = Mathf.Clamp(1.0f - hit.collider.material.dynamicFriction, 0.3f, 1.0f);
        }
        
        
        if (hit.collider.sharedMaterial != null)
        {
            
            surfaceFactor *= Mathf.Clamp(1.0f - hit.collider.sharedMaterial.dynamicFriction, 0.5f, 1.5f);
        }
        
        return Mathf.Clamp01(surfaceFactor);
    }

    private void UpdateSmoothFactors()
    {
        
        if (!enableEdgeDetection)
        {
            edgeDetectionFactor = 0f;
        }
        
        
        if (!enableMicroVariations)
        {
            microVariationOffset = 0f;
        }
    }

    void InitializeComponents()
    {
        if (flashlightLight == null)
        {
            flashlightLight = GetComponent<Light>();
            if (flashlightLight == null)
            {
                return;
            }
        }

        
        if (volumetricBeam == null)
        {
            volumetricBeam = GetComponent<VLB.VolumetricLightBeamSD>();
            if (volumetricBeam == null)
            {
                volumetricBeam = GetComponentInChildren<VLB.VolumetricLightBeamSD>();
                if (volumetricBeam == null)
                {
                    return;
                }
            }
        }

        
        if (dustParticles == null && enableDustParticles)
        {
            dustParticles = GetComponent<VolumetricDustParticles>();
            if (dustParticles == null)
            {
                dustParticles = GetComponentInChildren<VolumetricDustParticles>();
            }
        }
    }

    void SetupDynamicOcclusion()
    {
        if (!enableDynamicOcclusion || volumetricBeam == null) return;

        
        if (dynamicOcclusion == null)
        {
            dynamicOcclusion = volumetricBeam.gameObject.GetComponent<DynamicOcclusionRaycasting>();
            if (dynamicOcclusion == null)
            {
                dynamicOcclusion = volumetricBeam.gameObject.AddComponent<DynamicOcclusionRaycasting>();
            }
        }

        
        if (dynamicOcclusion != null)
        {
            dynamicOcclusion.enabled = true;
            dynamicOcclusion.layerMask = occlusionLayers;
            dynamicOcclusion.fadeDistanceToSurface = occlusionFadeDistance;
            dynamicOcclusion.minSurfaceRatio = 0.1f * occlusionQuality;
            dynamicOcclusion.planeAlignment = VLB.PlaneAlignment.Beam;
            dynamicOcclusion.planeOffset = 0.01f;
        }
    }

    void InitializeMultiRaySystem()
    {
        if (useMultiRayDetection && multiRayCount > 0)
        {
            multiRayDirections = new Vector3[multiRayCount];
            rayOcclusionFactors = new float[multiRayCount];
            
            
            for (int i = 0; i < multiRayCount; i++)
            {
                float angle = (i / (float)multiRayCount) * 360f;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * multiRaySpread,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * multiRaySpread,
                    0f
                );
                multiRayDirections[i] = offset;
                rayOcclusionFactors[i] = 0f;
            }
        }
    }

    void CacheOriginalValues()
    {
        if (volumetricBeam != null)
        {
            originalBeamIntensity = volumetricBeam.intensityInside;
            currentBeamIntensity = originalBeamIntensity;
        }
    }

    void UpdateOcclusion()
    {
        if (dynamicOcclusion == null || !enableDynamicOcclusion) return;

        Vector3 beamDirection = transform.forward;
        float maxDistance = Mathf.Min(flashlightLight.range, maxOcclusionDistance);

        if (useMultiRayDetection && multiRayDirections != null)
        {
            
            int occludedRays = 0;
            averageOcclusionFactor = 0f;

            for (int i = 0; i < multiRayCount; i++)
            {
                Vector3 rayDirection = transform.TransformDirection(multiRayDirections[i]) + beamDirection;
                rayDirection.Normalize();

                RaycastHit hit;
                if (Physics.Raycast(transform.position, rayDirection, out hit, maxDistance, occlusionLayers))
                {
                    
                    float distanceFactor = 1.0f - Mathf.Clamp01(hit.distance / maxDistance);
                    float angleFactor = Mathf.Clamp01(Vector3.Dot(-rayDirection, hit.normal));
                    float surfaceFactor = CalculateSurfaceFactor(hit);
                    
                    rayOcclusionFactors[i] = Mathf.Clamp01(distanceFactor * angleFactor * surfaceFactor * volumetricOcclusionIntensity);
                    occludedRays++;
                }
                else
                {
                    rayOcclusionFactors[i] = 0f;
                }

                averageOcclusionFactor += rayOcclusionFactors[i];
            }

            averageOcclusionFactor /= multiRayCount;
        }
    
    
        if (occlusionSmoothness > 0)
        {
            smoothedOcclusionFactor = Mathf.Lerp(smoothedOcclusionFactor, averageOcclusionFactor, Time.deltaTime * occlusionSmoothness);
        }
        else
        {
            smoothedOcclusionFactor = averageOcclusionFactor;
        }
        
        
        isOccluded = smoothedOcclusionFactor > 0.05f;
        
        
        if (enableEdgeDetection)
        {
            UpdateEdgeDetection();
        }
        
        
        if (enableMicroVariations)
        {
            microVariationOffset = Mathf.PerlinNoise(Time.time * microVariationFrequency, 0) * microVariationIntensity;
        }
        
        
        if (isOccluded)
        {
            float finalOcclusion = smoothedOcclusionFactor - (edgeDetectionFactor * 0.3f) + microVariationOffset;
            finalOcclusion = Mathf.Clamp01(finalOcclusion);
            
            currentBeamIntensity = Mathf.Lerp(originalBeamIntensity, occludedBeamIntensity, finalOcclusion);
            
            
            if (volumetricBeam != null)
            {
                volumetricBeam.intensityInside = currentBeamIntensity;
                volumetricBeam.intensityOutside = currentBeamIntensity;
            }
        }
        else
        {
            currentBeamIntensity = originalBeamIntensity;
            if (volumetricBeam != null)
            {
                volumetricBeam.intensityInside = originalBeamIntensity;
                volumetricBeam.intensityOutside = originalBeamIntensity;
            }
        }
    }

    void ApplyVisualEffects()
    {
        
        if (flashlightLight != null)
        {
            flashlightLight.shadowStrength = contactShadowIntensity * smoothedOcclusionFactor;
            
            
            if (isOccluded)
            {
                flashlightLight.shadowBias = 0.05f;
                flashlightLight.shadowNormalBias = 0.4f;
            }
            else
            {
                flashlightLight.shadowBias = 0.0f;
                flashlightLight.shadowNormalBias = 0.0f;
            }
        }
    }

    
    public void SetOcclusionEnabled(bool enabled)
    {
        enableDynamicOcclusion = enabled;
        if (dynamicOcclusion != null)
        {
            dynamicOcclusion.enabled = enabled;
        }
    }

    public void SetContactShadowIntensity(float intensity)
    {
        contactShadowIntensity = Mathf.Clamp01(intensity);
    }

    public void SetBeamIntensity(float intensity)
    {
        if (volumetricBeam != null)
        {
            volumetricBeam.intensityInside = intensity;
            volumetricBeam.intensityOutside = intensity;
            originalBeamIntensity = intensity;
        }
    }

    [ContextMenu("Optimizar Oclusion Ahora")]
    public void OptimizeOcclusion()
    {
        InitializeComponents();
        SetupDynamicOcclusion();
        UpdateOcclusion();
        ApplyVisualEffects();
    }

    [ContextMenu("Restablecer Configuracion")]
    public void ResetSettings()
    {
        if (volumetricBeam != null)
        {
            volumetricBeam.intensityInside = originalBeamIntensity;
            volumetricBeam.intensityOutside = originalBeamIntensity;
        }
        
        smoothedOcclusionFactor = 0f;
        edgeDetectionFactor = 0f;
        microVariationOffset = 0f;
        
        InitializeComponents();
        SetupDynamicOcclusion();
    }

    private void UpdateProximityOcclusion()
    {
        if (!enableProximityOcclusion || volumetricBeam == null) return;

        
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position, 
            proximityDetectionRadius, 
            proximityLayers
        );

        isProximityOccluded = nearbyColliders.Length > 0;

        
        if (isProximityOccluded)
        {
            
            float closestDistance = float.MaxValue;
            foreach (Collider collider in nearbyColliders)
            {
                float distance = Vector3.Distance(transform.position, collider.ClosestPoint(transform.position));
                closestDistance = Mathf.Min(closestDistance, distance);
            }

            
            proximityOcclusionFactor = Mathf.Clamp01(1f - (closestDistance / proximityOcclusionDistance));
            
            
            if (proximityOcclusionFactor > 0.7f)
            {
                
                float beamIntensityMultiplier = Mathf.Lerp(1f, 0.1f, (proximityOcclusionFactor - 0.7f) / 0.3f);
                float newIntensity = volumetricBeam.intensityOutside * beamIntensityMultiplier;
                volumetricBeam.intensityInside = newIntensity;
                volumetricBeam.intensityOutside = newIntensity;
                
                
                if (proximityOcclusionFactor > 0.85f)
                {
                    volumetricBeam.enabled = false;
                }
            }
        }
        else
        {
            
            proximityOcclusionFactor = Mathf.Lerp(proximityOcclusionFactor, 0f, Time.deltaTime * 5f);
            
            
            if (proximityOcclusionFactor < 0.1f)
            {
                volumetricBeam.enabled = true;
                
                
                if (volumetricBeam.intensityInside < originalBeamIntensity)
                {
                    volumetricBeam.intensityInside = Mathf.Lerp(
                        volumetricBeam.intensityInside, 
                        originalBeamIntensity, 
                        Time.deltaTime * 3f
                    );
                    volumetricBeam.intensityOutside = volumetricBeam.intensityInside;
                }

                if (isModelDisabled && modelRenderers != null) {
                    foreach (Renderer r in modelRenderers) {
                        r.enabled = true;
                    }
                    isModelDisabled = false;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        
        if (useMultiRayDetection && multiRayDirections != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 beamDirection = transform.forward;
            float maxDistance = Mathf.Min(flashlightLight.range, maxOcclusionDistance);

            for (int i = 0; i < multiRayCount; i++)
            {
                Vector3 rayDirection = transform.TransformDirection(multiRayDirections[i]) + beamDirection;
                rayDirection.Normalize();
                
                Gizmos.DrawRay(transform.position, rayDirection * maxDistance * rayOcclusionFactors[i]);
            }
        }

        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
