using UnityEngine;
using UnityEngine.AI;
// using UnityEngine.VFX; // YA NO LO USAMOS

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCHealth))]
public class NPCNoiseEmitter : MonoBehaviour
{
    [Header("Configuración Visual (Shader)")]
    [Tooltip("Arrastrá aquí el objeto Quad hijo que tiene el material del Sonar")]
    public GameObject sonarQuad;

    [Tooltip("Color del sonar para este NPC")]
    public Color npcSonarColor = new Color(1f, 0.8f, 0.2f, 1f); // Amarillo por defecto

    private Material sonarMat;
    private Transform sonarTransform;

    [Header("Nombres en el Shader Graph")]
    public string shaderSpeedProperty = "_Speed";
    public string shaderColorProperty = "_SonarColor";

    [Header("Radios de ruido (metros)")]
    public float idleNoiseRadius = 2.5f;
    public float walkNoiseRadius = 3f;
    public float runNoiseRadius = 5f;
    public float crouchNoiseRadius = 2f;

    [Header("Thresholds de Velocidad")]
    [SerializeField] private float walkSpeedThreshold = 0.5f;
    [SerializeField] private float runSpeedThreshold = 4.0f;

    [Header("Visual Feedback")]
    public float visualLerpSpeed = 5f;

    [Header("Configuración de Pulsación")]
    public float idlePulseSpeed = 1f;
    public float walkPulseSpeed = 3f;
    public float runPulseSpeed = 6f;

    [Header("Debug")]
    public bool showNoiseGizmo = true;
    public Color gizmoColor = new Color(1f, 0.8f, 0.4f, 0.25f);

    [HideInInspector] public float currentNoiseRadius = 0f;

    private NavMeshAgent agent;
    private NPCHealth npcHealth;
    private NPCBehaviorManager behaviorManager;
    private float visualRadius = 0f;

    // Estado de visibilidad
    private bool isRingVisible = true;
    private PlayerNoiseEmitter leaderNoiseEmitter = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        npcHealth = GetComponent<NPCHealth>();
        behaviorManager = GetComponent<NPCBehaviorManager>();

        // CONFIGURACIÓN DEL SHADER
        if (sonarQuad != null)
        {
            sonarTransform = sonarQuad.transform;
            Renderer rend = sonarQuad.GetComponent<Renderer>();

            if (rend != null)
            {
                // Instancia única del material para este NPC
                sonarMat = rend.material;

                // Asignamos el color del NPC
                sonarMat.SetColor(shaderColorProperty, npcSonarColor);
            }
        }
    }

    void Update()
    {
        // Si está muerto, apagamos todo
        if (npcHealth != null && npcHealth.IsDead)
        {
            currentNoiseRadius = 0f;
            if (sonarQuad != null) sonarQuad.SetActive(false);
            return;
        }

        SyncRingWithLeader();
        CalculateLogicRadius();
        UpdateShaderVisuals();
    }

    void CalculateLogicRadius()
    {
        if (agent == null)
        {
            currentNoiseRadius = idleNoiseRadius;
            return;
        }

        float currentSpeed = agent.velocity.magnitude;
        bool isMoving = currentSpeed > 0.1f;
        bool isRunning = currentSpeed >= runSpeedThreshold;
        bool isWalking = currentSpeed >= walkSpeedThreshold && !isRunning;

        // Lógica de agachado (depende de tu BehaviorManager)
        bool isCrouching = false;
        if (behaviorManager != null && behaviorManager.crouchSpeed > 0)
        {
            // Chequeo simple: si se mueve lento y no es porque apenas arrancó
            if (agent.speed <= behaviorManager.crouchSpeed + 0.1f && isMoving)
            {
                isCrouching = true;
            }
        }

        float targetRadius = idleNoiseRadius;

        if (isMoving)
        {
            if (isRunning) targetRadius = runNoiseRadius;
            else if (isCrouching) targetRadius = crouchNoiseRadius;
            else if (isWalking) targetRadius = walkNoiseRadius;
            else targetRadius = idleNoiseRadius;
        }

        currentNoiseRadius = Mathf.Max(targetRadius, idleNoiseRadius);
    }

    private void SyncRingWithLeader()
    {
        // Si no sigue a nadie, el anillo es visible por defecto (o según tu lógica propia)
        if (behaviorManager == null || !behaviorManager.IsFollowing)
        {
            leaderNoiseEmitter = null;
            // Opcional: isRingVisible = true; 
            return;
        }

        Transform leaderTransform = behaviorManager.CurrentLeaderTransform;
        if (leaderTransform != null)
        {
            // ACÁ ESTABA EL ERROR: Buscamos el componente PlayerNoiseEmitter
            PlayerNoiseEmitter leaderNoise = leaderTransform.GetComponent<PlayerNoiseEmitter>();

            if (leaderNoise != null)
            {
                leaderNoiseEmitter = leaderNoise;

                // CORRECCIÓN: Usamos 'isRingVisible' (minúscula), que es la variable pública
                isRingVisible = leaderNoise.isRingVisible;
            }
            else
            {
                leaderNoiseEmitter = null;
            }
        }
    }

    public void ToggleRingVisibility()
    {
        isRingVisible = !isRingVisible;
    }

    void UpdateShaderVisuals()
    {
        if (sonarQuad == null || sonarMat == null) return;

        // 1. Controlar Activación (Si el líder apagó el sonar, el NPC también)
        // Solo mostramos si isRingVisible es true Y el radio es relevante
        bool shouldShow = isRingVisible && (currentNoiseRadius > 0.1f);

        if (sonarQuad.activeSelf != shouldShow)
        {
            sonarQuad.SetActive(shouldShow);
        }

        if (!shouldShow) return;

        // 2. Interpolación Visual
        visualRadius = Mathf.Lerp(visualRadius, currentNoiseRadius, Time.deltaTime * visualLerpSpeed);

        // 3. Escalar el Quad (Diametro = Radio * 2)
        float diameter = visualRadius * 2f;
        Vector3 newScale = new Vector3(diameter, diameter, diameter);
        sonarTransform.localScale = newScale;

        // 4. Calcular velocidad de pulso
        float targetPulse = idlePulseSpeed;
        if (currentNoiseRadius >= runNoiseRadius - 0.1f) targetPulse = runPulseSpeed;
        else if (currentNoiseRadius >= walkNoiseRadius - 0.1f) targetPulse = walkPulseSpeed;

        // 5. Enviar al Shader
        sonarMat.SetFloat(shaderSpeedProperty, targetPulse);
    }

    void OnDrawGizmosSelected()
    {
        if (!showNoiseGizmo) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
    }
}