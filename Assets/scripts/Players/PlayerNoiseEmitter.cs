using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("Configuración Visual (Shader)")]
    [Tooltip("Arrastrá aquí el objeto Quad hijo que tiene el material del Sonar")]
    public GameObject sonarQuad;

    [Tooltip("Color de las ondas para ESTE jugador específico")]
    public Color playerSonarColor = Color.cyan; // <--- ACÁ CAMBIAS EL COLOR (Rojo o Azul)

    private Material sonarMat;
    private Transform sonarTransform;

    [Header("Nombres en el Shader Graph")]
    // Asegurate que en el Blackboard del shader se llamen así (Reference Name)
    public string shaderSpeedProperty = "_Speed";
    public string shaderColorProperty = "_SonarColor";

    [Header("Radios de Ruido (Metros)")]
    public float idleNoiseRadius = 2.5f;
    public float walkNoiseRadius = 3f;
    public float crouchNoiseRadius = 2f;
    public float runNoiseRadius = 6f;

    [Header("Suavizado Visual")]
    public float visualLerpSpeed = 5f;

    [Header("Velocidad de Onda (Pulsación)")]
    public float idlePulseSpeed = 1f;
    public float walkPulseSpeed = 3f;
    public float runPulseSpeed = 6f;

    [Header("Debug")]
    public bool showNoiseGizmo = true;

    [HideInInspector] public float currentNoiseRadius = 0f;

    private CharacterController controller;
    private float visualRadius = 0f;
    public bool isRingVisible = false;

    // --- VARIABLES DE REFLECTION (Tu lógica original) ---
    private object activeMovementScript;
    private FieldInfo isMovingField;
    private FieldInfo isRunningField;
    private FieldInfo isCrouchingField;
    private bool reflectionInitialized = false;

    [Header("Input")]
    [SerializeField] private InputActionReference toggleNoiseRingAction;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ringOnClip;
    public AudioClip ringOffClip;
    [Range(0f, 1f)] public float audioVolume = 0.5f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        InitializeReflection();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // --- CONFIGURACIÓN DEL SHADER Y COLOR ---
        if (sonarQuad != null)
        {
            sonarTransform = sonarQuad.transform;
            Renderer rend = sonarQuad.GetComponent<Renderer>();

            if (rend != null)
            {
                // .material crea una COPIA única para este objeto. 
                // Así el color de uno no afecta al otro.
                sonarMat = rend.material;

                // Asignamos el color del jugador UNA sola vez al inicio
                sonarMat.SetColor(shaderColorProperty, playerSonarColor);
            }

            // Sincronizamos estado inicial (Apagado/Prendido)
            sonarQuad.SetActive(isRingVisible);
        }
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
    }

    void Update()
    {
        CalculateLogicRadius();
        UpdateShaderVisuals();
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
            catch { reflectionInitialized = false; }
        }
        else
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

        // Prendemos/Apagamos el objeto directamente
        if (sonarQuad != null) sonarQuad.SetActive(isRingVisible);

        if (audioSource != null)
        {
            AudioClip clipToPlay = isRingVisible ? ringOnClip : ringOffClip;
            if (clipToPlay != null) audioSource.PlayOneShot(clipToPlay, audioVolume);
        }
    }

    void UpdateShaderVisuals()
    {
        if (sonarQuad == null || sonarMat == null) return;

        // 1. Interpolación suave del tamaño
        visualRadius = Mathf.Lerp(visualRadius, currentNoiseRadius, Time.deltaTime * visualLerpSpeed);

        // 2. Escalamos el objeto real (Radio * 2 = Diámetro)
        // Mantenemos Y en 1, escalamos X y Z.
        float diameter = visualRadius * 2f;
        Vector3 newScale = new Vector3(diameter, diameter, diameter);
        sonarTransform.localScale = newScale;

        // 3. Calculamos velocidad de pulso
        float targetPulse = idlePulseSpeed;
        if (currentNoiseRadius >= runNoiseRadius - 0.1f) targetPulse = runPulseSpeed;
        else if (currentNoiseRadius >= walkNoiseRadius - 0.1f) targetPulse = walkPulseSpeed;

        // 4. Enviamos al shader
        sonarMat.SetFloat(shaderSpeedProperty, targetPulse);
    }

    void OnDrawGizmosSelected()
    {
        if (!showNoiseGizmo) return;
        // Usamos el color del jugador también para el Gizmo para que coincida
        Gizmos.color = new Color(playerSonarColor.r, playerSonarColor.g, playerSonarColor.b, 0.4f);
        Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
    }
}