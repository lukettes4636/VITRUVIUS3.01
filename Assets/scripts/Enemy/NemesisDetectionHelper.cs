using UnityEngine;
using System.Collections.Generic;

public class NemesisDetectionHelper : MonoBehaviour
{
    [Header("Detection Configuration")]
    [Tooltip("Layers that block line of sight")]
    public LayerMask obstacleLayerMask = 1; 
    
    [Tooltip("Layers that contain detectable targets")]
    public LayerMask targetLayerMask = -1; 
    
    [Tooltip("Height offset for detection rays (to avoid ground collision)")]
    public float raycastHeightOffset = 1f;
    
    [Tooltip("Enable debug visualization in Scene view")]
    public bool showDebugRays = true;
    
    [Header("Collision Configuration")]
    [Tooltip("Should the nemesis collide with targets?")]
    public bool enableCollisions = true;
    
    [Tooltip("Collision radius for the nemesis")]
    public float collisionRadius = 0.5f;
    
    [Tooltip("Force applied when colliding with targets")]
    public float collisionForce = 10f;
    
    private NemesisAI parentAI;
    private NemesisAI_Enhanced parentAIEnhanced;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    
    
    private HashSet<Transform> lastDetectedTargets = new HashSet<Transform>();
    private float detectionTimer = 0f;
    
    void Awake()
    {
        parentAI = GetComponent<NemesisAI>();
        parentAIEnhanced = GetComponent<NemesisAI_Enhanced>();
        rb = GetComponent<Rigidbody>();
        
        SetupPhysics();
        SetupCollider();
    }
    
    void SetupPhysics()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        
        rb.mass = 100f;
        rb.drag = 5f;
        rb.angularDrag = 5f;
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    
    void SetupCollider()
    {
        
        Collider[] existingColliders = GetComponents<Collider>();
        foreach (Collider col in existingColliders)
        {
            if (!(col is CapsuleCollider))
            {
                Destroy(col);
            }
        }
        
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        }
        
        
        capsuleCollider.radius = collisionRadius;
        capsuleCollider.height = 2f;
        capsuleCollider.center = new Vector3(0f, 1f, 0f);
        capsuleCollider.isTrigger = false; 
    }
    
    
    
    
    public bool CanDetectTarget(Transform target, float maxDistance)
    {
        if (target == null) return false;
        
        Vector3 targetPosition = target.position + Vector3.up * raycastHeightOffset;
        Vector3 myPosition = transform.position + Vector3.up * raycastHeightOffset;
        Vector3 direction = (targetPosition - myPosition).normalized;
        float distance = Vector3.Distance(myPosition, targetPosition);
        
        if (distance > maxDistance) return false;
        
        
        RaycastHit hit;
        if (Physics.Raycast(myPosition, direction, out hit, distance, obstacleLayerMask))
        {
            
            if (hit.transform != target)
            {
                if (showDebugRays)
                {
                    Debug.DrawRay(myPosition, direction * distance, Color.red, 0.1f);
                }
                return false;
            }
        }
        
        
        if (Vector3.Dot(transform.forward, direction) < -0.5f) 
        {
            return false;
        }
        
        if (showDebugRays)
        {
            Debug.DrawRay(myPosition, direction * distance, Color.green, 0.1f);
        }
        
        return true;
    }
    
    
    
    
    public bool CheckTargetDetection(Transform target, float detectionRadius, string targetTag)
    {
        if (target == null) return false;
        
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > detectionRadius) return false;
        
        
        if (!target.CompareTag(targetTag)) return false;
        
        
        return CanDetectTarget(target, detectionRadius);
    }
    
    
    
    
    public Collider[] GetTargetsInRadius(float radius, string targetTag)
    {
        
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayerMask);
        
        
        System.Collections.Generic.List<Collider> validTargets = new System.Collections.Generic.List<Collider>();
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(targetTag))
            {
                validTargets.Add(hit);
            }
        }
        
        return validTargets.ToArray();
    }
    
    
    
    
    void OnCollisionEnter(Collision collision)
    {
        if (!enableCollisions) return;
        
        GameObject other = collision.gameObject;
        
        
        if (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("NPC"))
        {
            HandleTargetCollision(other, collision);
        }
    }
    
    void HandleTargetCollision(GameObject target, Collision collision)
    {
        
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 pushDirection = (target.transform.position - transform.position).normalized;
            pushDirection.y = 0.5f; 
            targetRb.AddForce(pushDirection * collisionForce, ForceMode.Impulse);
        }
        
        

        
        
        if (parentAI != null)
        {
            
        }
        else if (parentAIEnhanced != null)
        {
            
        }
    }
    
    
    
    
    void OnCollisionStay(Collision collision)
    {
        if (!enableCollisions) return;
        
        GameObject other = collision.gameObject;
        
        if (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("NPC"))
        {
            
            Vector3 separationDirection = (transform.position - other.transform.position).normalized;
            separationDirection.y = 0f; 
            
            
            if (rb != null)
            {
                rb.AddForce(separationDirection * collisionForce * 0.5f, ForceMode.Force);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugRays) return;
        
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
        
        
        if (parentAI != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, parentAI.detectionRadius);
        }
        else if (parentAIEnhanced != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, parentAIEnhanced.detectionRadius);
        }
    }
    
    
    
    
    public void ResetDetectionState()
    {
        
        lastDetectedTargets.Clear();
        detectionTimer = 0f;
        
        
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        
        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }
        

    }
}
