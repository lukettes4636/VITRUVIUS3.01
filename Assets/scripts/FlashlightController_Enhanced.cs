using UnityEngine;
using UnityEngine.InputSystem;
using VLB;

public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}

public class FlashlightController_Enhanced : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public float rotationSpeed = 100f;
    
    public Behaviour volumetricBeam;

    [Header("Input Settings")]
    public InputActionReference toggleAction;

    [Header("Arm Settings")]
    public Transform armTransform;
    public Vector3 armRotation = new Vector3(0, 0, 0);

    [Header("Audio")]
    [SerializeField] private bool playFlashlightSounds = true;
    public AudioClip flashlightClickSound;

    [Header("Animation Sync")]
    [Tooltip("Animator que controla la animacion de la linterna")]
    public Animator flashlightAnimator;
    [Tooltip("Buscar automaticamente animator basado en nombre del objeto")]
    public bool autoFindAnimator = true;
    [Tooltip("Nombre del parametro booleano para activar animacion")]
    public string animationBoolParameter = "FlashlightOn";
    [Tooltip("Tiempo de delay adicional despues de la animacion")]
    public float animationDelay = 0.1f;
    
    [Header("Flashlight Animation Controller")]
    [Tooltip("Controlador de animaciones de brazo para la linterna")]
    public FlashlightAnimationController animationController;

    [Header("Input Delay Settings")]
    [Tooltip("Delay entre el boton y el encendido de la luz (segundos)")]
    [Range(0.0f, 1.0f)] public float lightActivationDelay = 0.2f;
    [Tooltip("Delay entre el boton y el sonido (segundos)")]
    [Range(0.0f, 1.0f)] public float soundActivationDelay = 0.1f;

    [Header("Hand Collision Prevention")]
    [Tooltip("Habilitar deteccion de colision para evitar que mano traspase paredes")]
    public bool enableHandCollisionPrevention = true;
    [Tooltip("Capas que deben bloquear el movimiento de la mano")]
    public LayerMask handCollisionLayers = -1;
    [Tooltip("Radio de deteccion de colision alrededor de la mano")]
    [Range(0.1f, 1.0f)] public float handCollisionRadius = 0.3f;
    [Tooltip("Distancia maxima para deteccion de colision")]
    [Range(0.5f, 2.0f)] public float handCollisionDistance = 1.0f;

    [Header("Auto Lower Arm Near Walls")]
    [Tooltip("Bajar automaticamente el brazo cuando esta cerca de paredes")]
    public bool enableAutoLowerArm = true;
    [Tooltip("Distancia a la pared para activar el bajado automatico del brazo")]
    [Range(0.1f, 1.0f)] public float wallDetectionDistance = 0.5f;
    [Tooltip("Velocidad de transicion del brazo (0-10)")]
    [Range(1.0f, 10.0f)] public float armTransitionSpeed = 3.0f;
    [Tooltip("Nombre del parametro float en el Animator para controlar el brazo cerca de paredes")]
    public string wallArmParameter = "WallArm";
    [Tooltip("Valor del parametro cuando el brazo debe estar abajo (cerca de pared)")]
    [Range(0f, 1f)] public float wallArmDownValue = 1f;
    [Tooltip("Valor del parametro cuando el brazo debe estar normal (lejos de pared)")]
    [Range(0f, 1f)] public float wallArmNormalValue = 0f;

    [Header("VLB Optimization")]
    [Tooltip("Optimizar contacto volumetrico con objetos")]
    public bool enableVLBContactOptimization = true;
    [Tooltip("Segmentos geometricos para mejor precision (16-128)")]
    [Range(16, 128)] public int vlbTargetSegments = 128;
    [Tooltip("Distancia de blend para contactos definidos (0.1-2.0)")]
    [Range(0.1f, 2.0f)] public float vlbDepthBlend = 1.5f;
    [Tooltip("Intensidad de noise reducida para contactos limpios")]
    [Range(0.0f, 0.3f)] public float vlbNoiseIntensity = 0.15f;
    [Tooltip("Intensidad del haz volumetrico (1.0-10.0)")]
    [Range(1.0f, 10.0f)] public float vlbIntensity = 3.0f;
    [Tooltip("Brillo adicional del haz volumetrico (1.0-5.0)")]
    [Range(1.0f, 5.0f)] public float vlbBrightness = 2.0f;

    public bool isFlashlightOn = true;
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private bool pendingLightActivation = false;
    private bool pendingSoundActivation = false;
    private float lightDelayTimer = 0f;
    private float soundDelayTimer = 0f;
    
    [HideInInspector] public float originalIntensity;
    private VolumetricLightBeamSD vlbBeam;

    private AudioSource audioSource;
    private Vector3 lastSafeArmPosition;
    private Quaternion lastSafeArmRotation;
    
    private float currentWallArmValue = 0f;
    private float targetWallArmValue = 0f;

    void Start()
    {
        if (autoFindAnimator && flashlightAnimator == null)
        {
            FindAndSetupAnimator();
        }

        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();

        if (flashlight != null)
        {
            originalIntensity = flashlight.intensity;
            SetFlashlightState(isFlashlightOn, true);
        }

        SetupArm();

        InitializeVLBBeam();
        OptimizeVLBContact();

        if (toggleAction != null)
        {
            toggleAction.action.performed += OnToggleFlashlight;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.7f;
        }

        // Initialize animation controller
        if (animationController == null)
        {
            animationController = GetComponent<FlashlightAnimationController>();
        }
        
        if (animationController != null)
        {
            animationController.InitializeFromFlashlight(this, isFlashlightOn);
        }
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Disable();
        }
    }

    void Update()
    {
        ProcessDelayTimers();
        RotateFlashlight();
        
        if (isAnimating)
        {
            animationTimer -= Time.deltaTime;
            if (animationTimer <= 0f)
            {
                CompleteAnimation();
            }
        }
        
        PreventHandWallClipping();
        
        if (enableAutoLowerArm)
        {
            AutoLowerArmNearWalls();
        }
    }
    
    private void ProcessDelayTimers()
    {
        if (pendingLightActivation)
        {
            lightDelayTimer -= Time.deltaTime;
            if (lightDelayTimer <= 0f)
            {
                pendingLightActivation = false;
                
                if (flashlight != null)
                {
                    flashlight.intensity = isFlashlightOn ? originalIntensity : 0f;
                    flashlight.enabled = isFlashlightOn;
                }
                if (volumetricBeam != null)
                {
                    volumetricBeam.enabled = isFlashlightOn;
                }
            }
        }
        
        if (pendingSoundActivation)
        {
            soundDelayTimer -= Time.deltaTime;
            if (soundDelayTimer <= 0f)
            {
                pendingSoundActivation = false;
                PlayFlashlightSound();
            }
        }
    }

    void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed && !isAnimating)
        {
            pendingLightActivation = true;
            pendingSoundActivation = true;
            lightDelayTimer = lightActivationDelay;
            soundDelayTimer = soundActivationDelay;
            StartFlashlightAnimation();
        }
    }

    public void StartFlashlightAnimation()
    {
        if (isAnimating) return;
        
        isAnimating = true;
        PlayFlashlightSound();
        
        // Update animation controller state
        if (animationController != null)
        {
            // Flashlight animation controller integration
            if (!isFlashlightOn)
            {
                animationController.OnFlashlightTurnedOn();
            }
            else
            {
                animationController.OnFlashlightTurnedOff();
            }
        }
        
        if (flashlightAnimator != null && !string.IsNullOrEmpty(animationBoolParameter))
        {
            flashlightAnimator.SetBool(animationBoolParameter, !isFlashlightOn);
        }
        
        animationTimer = animationDelay;
    }

    public void OnFlashlightAnimationComplete()
    {
        CompleteAnimation();
    }

    private void CompleteAnimation()
    {
        if (!isAnimating) return;
        
        isFlashlightOn = !isFlashlightOn;
        SetFlashlightState(isFlashlightOn, true);
        isAnimating = false;
        animationTimer = 0f;
    }

    public void SetFlashlightState(bool state, bool immediate = false)
    {
        if (isAnimating && !immediate) return;
        
        isFlashlightOn = state;

        if (flashlight != null)
        {
            flashlight.intensity = state ? originalIntensity : 0f;
            flashlight.enabled = state;
        }

        if (volumetricBeam != null)
        {
            volumetricBeam.enabled = state;
        }

        if (immediate && flashlightAnimator != null && !string.IsNullOrEmpty(animationBoolParameter))
        {
            flashlightAnimator.SetBool(animationBoolParameter, state);
        }

        
        ToggleAllChildObjects(state);
    }

    void RotateFlashlight()
    {
        if (armTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            Vector3 rotation = armTransform.localEulerAngles;
            rotation.x += mouseY * rotationSpeed * Time.deltaTime;
            rotation.y += mouseX * rotationSpeed * Time.deltaTime;
            
            rotation.x = Mathf.Clamp(rotation.x, -80f, 80f);
            rotation.y = Mathf.Clamp(rotation.y, -80f, 80f);
            
            armTransform.localEulerAngles = rotation;
        }
    }

    void SetupArm()
    {
        if (armTransform != null)
        {
            armTransform.localEulerAngles = armRotation;
        }
    }

    void PlayFlashlightSound()
    {
        if (playFlashlightSounds && flashlightClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(flashlightClickSound);
        }
    }

    public void ForceFlashlightState(bool state)
    {
        isFlashlightOn = state;
        isAnimating = false;
        SetFlashlightState(state, true);
        
        // Force update animation controller state
        if (animationController != null)
        {
            animationController.ForceUpdateState(state);
        }
    }

    
private void ToggleAllChildObjects(bool state)
    {
        
        Light[] allLights = GetComponentsInChildren<Light>(true);
        foreach (Light light in allLights)
        {
            if (light != null && flashlight != null && light != flashlight) 
            {
                light.enabled = state;
            }
        }

        
        Behaviour[] allBehaviours = GetComponentsInChildren<Behaviour>(true);
        foreach (Behaviour behaviour in allBehaviours)
        {
            if (behaviour != null && behaviour != volumetricBeam && behaviour != this)
            {
                
                if (behaviour.GetType().Name.Contains("Spotlight") || 
                    behaviour.GetType().Name.Contains("Light") ||
                    behaviour.GetType().Name.Contains("VLB"))
                {
                    behaviour.enabled = state;
                }
            }
        }

        
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = state;
            }
        }
    }

    private void InitializeVLBBeam()
    {
        if (volumetricBeam != null)
        {
            vlbBeam = volumetricBeam as VolumetricLightBeamSD;
        }
        else
        {
            vlbBeam = GetComponentInChildren<VolumetricLightBeamSD>();
            if (vlbBeam != null)
            {
                volumetricBeam = vlbBeam;
            }
        }
    }

    private void OptimizeVLBContact()
    {
        if (!enableVLBContactOptimization || vlbBeam == null) return;

        vlbBeam.geomCustomSegments = vlbTargetSegments;
        vlbBeam.depthBlendDistance = vlbDepthBlend;
        vlbBeam.noiseIntensity = vlbNoiseIntensity;
        vlbBeam.intensityInside = vlbIntensity;
        vlbBeam.intensityOutside = vlbIntensity;
        vlbBeam.intensityFromLight = true; 
        vlbBeam.color = Color.white * vlbBrightness; 
    }

    private void PreventHandWallClipping()
    {
        if (!enableHandCollisionPrevention || armTransform == null) return;

        Vector3 handPosition = armTransform.position;
        Vector3 handForward = armTransform.forward;

        RaycastHit hit;
        if (Physics.SphereCast(handPosition, handCollisionRadius, handForward, out hit, 
                             handCollisionDistance, handCollisionLayers))
        {
            Vector3 safePosition = handPosition - handForward * (hit.distance - 0.05f);
            armTransform.position = safePosition;
            lastSafeArmPosition = safePosition;
            lastSafeArmRotation = armTransform.rotation;
        }
        else
        {
            lastSafeArmPosition = handPosition;
            lastSafeArmRotation = armTransform.rotation;
        }

        Collider[] overlappingColliders = Physics.OverlapSphere(handPosition, handCollisionRadius * 0.8f, handCollisionLayers);
        if (overlappingColliders.Length > 0)
        {
            armTransform.position = lastSafeArmPosition;
            armTransform.rotation = lastSafeArmRotation;
        }
    }

    [ContextMenu("Aplicar Optimizaciones VLB")]
    public void ApplyVBLOptimizations()
    {
        OptimizeVLBContact();
    }

    [ContextMenu("Resetear Optimizaciones VLB")]
    public void ResetVBLOptimizations()
    {
        if (vlbBeam == null) return;

        vlbBeam.geomCustomSegments = 16;
        vlbBeam.depthBlendDistance = 2.0f;
        vlbBeam.noiseIntensity = 0.5f;
        vlbBeam.intensityInside = 1.0f;
        vlbBeam.intensityOutside = 1.0f;
        vlbBeam.intensityFromLight = true; 
        vlbBeam.color = Color.white; 
    }

    private void FindAndSetupAnimator()
    {
        Animator[] allAnimators = FindObjectsOfType<Animator>(true);
        
        foreach (Animator animator in allAnimators)
        {
            if (animator.gameObject.name.Contains("Player1") || 
                animator.gameObject.name.Contains("player1") ||
                animator.gameObject.name.Equals("Player1"))
            {
                if (gameObject.name.Contains("Player1") || 
                    gameObject.name.Contains("player1") ||
                    gameObject.transform.IsChildOf(animator.transform))
                {
                    flashlightAnimator = animator;
                    break;
                }
            }
            else if (animator.gameObject.name.Contains("Player2") || 
                     animator.gameObject.name.Contains("player2") ||
                     animator.gameObject.name.Equals("Player2"))
            {
                if (gameObject.name.Contains("Player2") || 
                    gameObject.name.Contains("player2") ||
                    gameObject.transform.IsChildOf(animator.transform))
                {
                    flashlightAnimator = animator;
                    break;
                }
            }
        }

        if (flashlightAnimator != null)
        {
            if (!string.IsNullOrEmpty(wallArmParameter))
            {
                if (!flashlightAnimator.HasParameter(wallArmParameter))
                {
                    #if UNITY_EDITOR

                    #endif
                    StartCoroutine(AddParameterNextFrame());
                }
            }
            #if UNITY_EDITOR

            #endif
        }
        else
        {
            #if UNITY_EDITOR

            #endif
        }
    }

    private System.Collections.IEnumerator AddParameterNextFrame()
    {
        yield return null;
        
        #if UNITY_EDITOR
        UnityEditor.Animations.AnimatorController controller = flashlightAnimator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        if (controller != null)
        {
            bool parameterExists = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == wallArmParameter && param.type == AnimatorControllerParameterType.Float)
                {
                    parameterExists = true;
                    break;
                }
            }
            
            if (!parameterExists)
            {
                controller.AddParameter(wallArmParameter, AnimatorControllerParameterType.Float);

            }
        }
        #endif
    }

    private void AutoLowerArmNearWalls()
    {
        if (armTransform == null || flashlightAnimator == null) return;

        Vector3 handPosition = armTransform.position;
        Vector3 handForward = armTransform.forward;

        RaycastHit hit;
        bool wallDetected = Physics.Raycast(handPosition, handForward, out hit, 
                                          wallDetectionDistance, handCollisionLayers);

        if (wallDetected)
        {
            targetWallArmValue = wallArmDownValue;
        }
        else
        {
            targetWallArmValue = wallArmNormalValue;
        }

        currentWallArmValue = Mathf.Lerp(
            currentWallArmValue, 
            targetWallArmValue, 
            Time.deltaTime * armTransitionSpeed
        );

        if (!string.IsNullOrEmpty(wallArmParameter))
        {
            flashlightAnimator.SetFloat(wallArmParameter, currentWallArmValue);
        }

        #if UNITY_EDITOR
        Debug.DrawRay(handPosition, handForward * wallDetectionDistance, 
                     wallDetected ? Color.red : Color.green);
        #endif
    }
}
