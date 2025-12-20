using UnityEngine;
using Gameplay;





[RequireComponent(typeof(Rigidbody))]
public class ObjectNoiseEmitter : MonoBehaviour
{
    [Header("Radios de ruido (metros)")]
    [Tooltip("Radio de ruido cuando el objeto esta quieto")]
    public float idleNoiseRadius = 0f;
    [Tooltip("Radio de ruido cuando el objeto se mueve lentamente")]
    public float movingNoiseRadius = 4f;
    [Tooltip("Radio de ruido cuando el objeto se mueve rapido (caida/lanzamiento)")]
    public float fastMovingNoiseRadius = 8f;
    [Tooltip("Radio de ruido temporal cuando el objeto choca")]
    public float collisionNoiseRadius = 10f;
    [Tooltip("Duracion del ruido de colision")]
    public float collisionNoiseDuration = 2f;

    [Header("Thresholds de Velocidad")]
    [Tooltip("Velocidad minima para considerar que esta moviendose")]
    [SerializeField] private float movingSpeedThreshold = 0.5f;
    [Tooltip("Velocidad minima para considerar movimiento rapido")]
    [SerializeField] private float fastMovingSpeedThreshold = 3.0f;

    [Header("Debug")]
    [Tooltip("Mostrar gizmo de radio de ruido en el editor")]
    public bool showNoiseGizmo = true;
    [Tooltip("Color del gizmo de ruido")]
    public Color noiseColor = new Color(1f, 0.4f, 0.8f, 0.25f);

    
    
    
    [HideInInspector] public float currentNoiseRadius = 0f;

    private Rigidbody rb;
    private float collisionNoiseTimer = 0f;
    private GrabbableObjectController grabbableController;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabbableController = GetComponent<GrabbableObjectController>();
    }

    void Update()
    {
        
        if (grabbableController != null && grabbableController.IsHeld)
        {
            currentNoiseRadius = idleNoiseRadius;
            return;
        }

        CalculateLogicRadius();
        
        
        if (collisionNoiseTimer > 0f)
        {
            collisionNoiseTimer -= Time.deltaTime;
            if (collisionNoiseTimer <= 0f)
            {
                collisionNoiseTimer = 0f;
            }
        }
    }

    
    
    
    void CalculateLogicRadius()
    {
        if (rb == null)
        {
            currentNoiseRadius = idleNoiseRadius;
            return;
        }

        float currentSpeed = rb.velocity.magnitude;
        float baseNoiseRadius = idleNoiseRadius;

        
        if (collisionNoiseTimer > 0f)
        {
            float collisionNoiseAmount = collisionNoiseTimer / collisionNoiseDuration;
            baseNoiseRadius = Mathf.Lerp(idleNoiseRadius, collisionNoiseRadius, collisionNoiseAmount);
        }
        
        else if (currentSpeed > movingSpeedThreshold)
        {
            if (currentSpeed >= fastMovingSpeedThreshold)
            {
                baseNoiseRadius = fastMovingNoiseRadius;
            }
            else
            {
                baseNoiseRadius = movingNoiseRadius;
            }
        }

        currentNoiseRadius = baseNoiseRadius;
    }

    
    
    
    public void TriggerCollisionNoise(float impactMagnitude = 1f)
    {
        collisionNoiseTimer = collisionNoiseDuration;
        
        float adjustedRadius = collisionNoiseRadius * Mathf.Clamp(impactMagnitude / 10f, 0.5f, 2f);
        currentNoiseRadius = Mathf.Max(currentNoiseRadius, adjustedRadius);
    }

    void OnDrawGizmosSelected()
    {
        if (!showNoiseGizmo) return;
        Gizmos.color = noiseColor;
        Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
    }
}
