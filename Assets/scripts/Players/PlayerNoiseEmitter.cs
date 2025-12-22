using UnityEngine;
using UnityEngine.VFX;
using System.Reflection;
using System.Linq; 
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("Radios de ruido (metros)")]
    [Tooltip("Radio de ruido constante en idle (SIEMPRE activo para deteccion cercana)")]
    public float idleNoiseRadius = 2.5f; 
    public float walkNoiseRadius = 3f;
    public float crouchNoiseRadius = 2f;
    public float runNoiseRadius = 6f;

    [Header("Visual Feedback (VFX)")]
    public VisualEffect noiseVFX;
    public string vfxRadiusProperty = "Radius";
    public string vfxPulseProperty = "PulseSpeed";
    public float visualLerpSpeed = 5f;

    [Header("Configuracin de Pulsacin")]
    public float idlePulseSpeed = 2f;
    public float walkPulseSpeed = 8f;
    public float runPulseSpeed = 18f;

    [Header("Debug")]
    public bool showNoiseGizmo = true;
    public Color noiseColor = new Color(1f, 0.6f, 0f, 0.25f);
    public float debugLogInterval = 1f; 
    private float lastLogTime;

    [HideInInspector] public float currentNoiseRadius = 0f;

    private CharacterController controller;
    private float visualRadius = 0f;
    public bool isRingVisible = false;
    
    public bool IsRingVisible
    {
        get { return isRingVisible; }
        set { isRingVisible = value; }
    }
    
    private object activeMovementScript;
    private FieldInfo isMovingField;
    private FieldInfo isRunningField;
    private FieldInfo isCrouchingField;
    private bool reflectionInitialized = false;

    [Header("Input para Toggle de Noise Ring")]
    [SerializeField] private InputActionReference toggleNoiseRingAction; 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ringOnClip;
    public AudioClip ringOffClip;
    [Range(0f, 1f)] public float audioVolume = 0.5f;
    public float ringOnPitch = 1.2f;
    public float ringOffPitch = 0.8f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        InitializeReflection();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (noiseVFX != null)
        {
            noiseVFX.Play();
            noiseVFX.enabled = false;
        }
        lastLogTime = Time.time;
    }

    void OnEnable()
    {
        if (toggleNoiseRingAction != null && toggleNoiseRingAction.action != null)
        {
            toggleNoiseRingAction.action.Enable();
            toggleNoiseRingAction.action.performed += ctx => ToggleRingVisibility();
        }
    }

    void OnDisable()
    {
        if (toggleNoiseRingAction != null && toggleNoiseRingAction.action != null)
        {
            toggleNoiseRingAction.action.performed -= ctx => ToggleRingVisibility();
            toggleNoiseRingAction.action.Disable();
        }
    }

    void InitializeReflection()
    {
        
        Component[] components = GetComponents<Component>();

        
        activeMovementScript = components.FirstOrDefault(c =>
            c != null && (c.GetType().Name == "MovJugador1" || c.GetType().Name == "MovJugador2"));

        if (activeMovementScript != null)
        {
            var type = activeMovementScript.GetType();


            
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

            isMovingField = type.GetField("isMoving", flags);
            isRunningField = type.GetField("isRunningInput", flags); 
            isCrouchingField = type.GetField("isCrouching", flags);





            reflectionInitialized = isMovingField != null && isRunningField != null && isCrouchingField != null;
        }
        else
        {

            reflectionInitialized = false;
        }
    }

    void Update()
    {
        CalculateLogicRadius();
        UpdateVFX();

        if (Time.time > lastLogTime + debugLogInterval)
        {
            LogCurrentState();
            lastLogTime = Time.time;
        }
    }

    void LogCurrentState()
    {
        if (reflectionInitialized)
        {
            bool isMoving = (bool)isMovingField.GetValue(activeMovementScript);
            bool isRunning = (bool)isRunningField.GetValue(activeMovementScript);
            bool isCrouching = (bool)isCrouchingField.GetValue(activeMovementScript);


        }
        else
        {

        }
    }

    
    
    
void CalculateLogicRadius()
    {
        bool isMoving = false;
        bool isRunning = false;
        bool isCrouching = false;

        if (reflectionInitialized)
        {
            try
            {
                isMoving = (bool)isMovingField.GetValue(activeMovementScript);
                isRunning = (bool)isRunningField.GetValue(activeMovementScript);
                isCrouching = (bool)isCrouchingField.GetValue(activeMovementScript);
            }
            catch (System.Exception ex)
            {
                reflectionInitialized = false;
            }
        }

        
        if (!reflectionInitialized)
        {
            isMoving = controller.velocity.magnitude > 0.1f;
        }

        
        float targetRadius = idleNoiseRadius; 

        if (isMoving)
        {
            if (isRunning) targetRadius = runNoiseRadius;
            else if (isCrouching) targetRadius = crouchNoiseRadius;
            else targetRadius = walkNoiseRadius;
        }

        
        
        currentNoiseRadius = Mathf.Max(targetRadius, idleNoiseRadius);
    }

    
    
    
    public void ToggleRingVisibility()
    {
        isRingVisible = !isRingVisible;
        
        if (audioSource != null)
        {
            AudioClip clipToPlay = isRingVisible ? ringOnClip : ringOffClip;
            if (clipToPlay != null)
            {
                audioSource.pitch = isRingVisible ? ringOnPitch : ringOffPitch;
                audioSource.PlayOneShot(clipToPlay, audioVolume);
            }
        }
    }

    void UpdateVFX()
    {
        if (noiseVFX == null) return;

        
        visualRadius = Mathf.Lerp(visualRadius, currentNoiseRadius, Time.deltaTime * visualLerpSpeed);

        
        
        if (noiseVFX.HasFloat(vfxRadiusProperty))
            noiseVFX.SetFloat(vfxRadiusProperty, visualRadius);

        
        float targetPulse = idlePulseSpeed;

        if (currentNoiseRadius >= runNoiseRadius - 0.1f)
            targetPulse = runPulseSpeed;
        else if (currentNoiseRadius >= walkNoiseRadius - 0.1f)
            targetPulse = walkPulseSpeed;

        
        if (noiseVFX.HasFloat(vfxPulseProperty))
            noiseVFX.SetFloat(vfxPulseProperty, targetPulse);

        
        noiseVFX.enabled = isRingVisible && (visualRadius > 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        if (!showNoiseGizmo) return;
        Gizmos.color = noiseColor;
        Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
    }
}

