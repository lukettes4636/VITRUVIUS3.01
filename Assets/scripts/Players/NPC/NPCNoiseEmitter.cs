using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.AI;





[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCHealth))]
public class NPCNoiseEmitter : MonoBehaviour
{
    [Header("Radios de ruido (metros)")]
    [Tooltip("Radio de ruido constante cuando el NPC esta quieto (SIEMPRE activo para deteccion cercana)")]
    public float idleNoiseRadius = 2.5f; 
    [Tooltip("Radio de ruido cuando el NPC esta caminando")]
    public float walkNoiseRadius = 3f;
    [Tooltip("Radio de ruido cuando el NPC esta corriendo")]
    public float runNoiseRadius = 5f;
    [Tooltip("Radio de ruido cuando el NPC esta agachado")]
    public float crouchNoiseRadius = 2f;

    [Header("Thresholds de Velocidad")]
    [Tooltip("Velocidad minima para considerar que esta caminando")]
    [SerializeField] private float walkSpeedThreshold = 0.5f;
    [Tooltip("Velocidad minima para considerar que esta corriendo")]
    [SerializeField] private float runSpeedThreshold = 4.0f;

    [Header("Visual Feedback (VFX)")]
    [Tooltip("Visual Effect que muestra el radio de ruido")]
    public VisualEffect noiseVFX;
    [Tooltip("Nombre del parametro de radio en el VFX")]
    public string vfxRadiusProperty = "Radius";
    [Tooltip("Nombre del parametro de pulso en el VFX")]
    public string vfxPulseProperty = "PulseSpeed";
    [Tooltip("Velocidad de interpolacion visual del radio")]
    public float visualLerpSpeed = 5f;

    [Header("Configuracion de Pulsacion")]
    [Tooltip("Velocidad de pulso cuando esta quieto")]
    public float idlePulseSpeed = 2f;
    [Tooltip("Velocidad de pulso cuando esta caminando")]
    public float walkPulseSpeed = 8f;
    [Tooltip("Velocidad de pulso cuando esta corriendo")]
    public float runPulseSpeed = 15f;

    [Header("Debug")]
    [Tooltip("Mostrar gizmo de radio de ruido en el editor")]
    public bool showNoiseGizmo = true;
    [Tooltip("Color del gizmo de ruido")]
    public Color noiseColor = new Color(1f, 0.8f, 0.4f, 0.25f);

    
    
    
    [HideInInspector] public float currentNoiseRadius = 0f;

    private NavMeshAgent agent;
    private NPCHealth npcHealth;
    private NPCBehaviorManager behaviorManager;
    private float visualRadius = 0f;
    private bool isRingVisible = true;
    private PlayerNoiseEmitter leaderNoiseEmitter = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        npcHealth = GetComponent<NPCHealth>();
        behaviorManager = GetComponent<NPCBehaviorManager>();

        if (noiseVFX != null)
        {
            noiseVFX.Play();
        }
    }

    void Update()
    {
        
        if (npcHealth != null && npcHealth.IsDead)
        {
            currentNoiseRadius = 0f;
            UpdateVFX();
            return;
        }

        SyncRingWithLeader();

        CalculateLogicRadius();
        UpdateVFX();
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

        
        bool isCrouching = false;
        if (behaviorManager != null && agent.speed <= behaviorManager.crouchSpeed + 0.1f && isMoving)
        {
            isCrouching = true;
        }

        
        float targetRadius = idleNoiseRadius; 

        if (isMoving)
        {
            if (isRunning)
                targetRadius = runNoiseRadius;
            else if (isCrouching)
                targetRadius = crouchNoiseRadius;
            else if (isWalking)
                targetRadius = walkNoiseRadius;
            else
                targetRadius = idleNoiseRadius;
        }

        
        
        currentNoiseRadius = Mathf.Max(targetRadius, idleNoiseRadius);
    }

    
    
    
    private void SyncRingWithLeader()
    {
        if (behaviorManager == null || !behaviorManager.IsFollowing)
        {
            leaderNoiseEmitter = null;
            return;
        }

        Transform leaderTransform = behaviorManager.CurrentLeaderTransform;
        if (leaderTransform != null)
        {
            PlayerNoiseEmitter leaderNoise = leaderTransform.GetComponent<PlayerNoiseEmitter>();
            if (leaderNoise != null)
            {
                leaderNoiseEmitter = leaderNoise;
                isRingVisible = leaderNoise.IsRingVisible;
            }
            else
            {
                leaderNoiseEmitter = null;
            }
        }
        else
        {
            leaderNoiseEmitter = null;
        }
    }

    
    public void ToggleRingVisibility()
    {
        isRingVisible = !isRingVisible;
    }

    
    
    
    void UpdateVFX()
    {
        if (noiseVFX == null) return;

        
        visualRadius = Mathf.Lerp(visualRadius, currentNoiseRadius, Time.deltaTime * visualLerpSpeed);

        
        bool shouldShowRing = isRingVisible && (visualRadius > 0.1f);
        
        if (shouldShowRing)
        {
            
            noiseVFX.SetFloat(vfxRadiusProperty, visualRadius);

            
            float targetPulse = idlePulseSpeed;

            if (currentNoiseRadius >= runNoiseRadius - 0.1f)
                targetPulse = runPulseSpeed;
            else if (currentNoiseRadius >= walkNoiseRadius - 0.1f)
                targetPulse = walkPulseSpeed;

            noiseVFX.SetFloat(vfxPulseProperty, targetPulse);
        }

        
        noiseVFX.enabled = shouldShowRing;
    }

    void OnDrawGizmosSelected()
    {
        if (!showNoiseGizmo) return;
        Gizmos.color = noiseColor;
        Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
    }
}

