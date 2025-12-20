using UnityEngine;

public class NemesisValidator : MonoBehaviour
{
    [Header("Validation Configuration")]
    public bool runValidationOnStart = true;
    public bool continuousValidation = false;
    
    [Header("Test Results")]
    [SerializeField] private bool hasNemesisAI = false;
    [SerializeField] private bool hasDetectionHelper = false;
    [SerializeField] private bool hasTester = false;
    [SerializeField] private bool hasRigidbody = false;
    [SerializeField] private bool hasCollider = false;
    [SerializeField] private bool hasNavMeshAgent = false;
    [SerializeField] private bool hasAnimator = false;
    [SerializeField] private bool hasAudioSource = false;
    
    void Start()
    {
        if (runValidationOnStart)
        {
            RunFullValidation();
        }
    }
    
    void Update()
    {
        if (continuousValidation)
        {
            UpdateComponentStatus();
        }
    }
    
    [ContextMenu("Run Full Validation")]
    public void RunFullValidation()
    {

        
        bool allTestsPassed = true;
        
        
        allTestsPassed &= ValidateCoreComponents();
        
        
        allTestsPassed &= ValidateDetectionSystem();
        
        
        allTestsPassed &= ValidateMovementSystem();
        
        
        allTestsPassed &= ValidateAudioSystem();
        
        
        allTestsPassed &= ValidateIntegration();
        

        
        if (allTestsPassed)
        {



        }
    }
    
    bool ValidateCoreComponents()
    {

        
        UpdateComponentStatus();
        
        bool passed = true;
        
        if (!hasNemesisAI)
        {

            passed = false;
        }
        else
        {

        }
        
        if (!hasDetectionHelper)
        {

            passed = false;
        }
        else
        {

        }
        
        if (!hasTester)
        {

        }
        else
        {

        }
        
        return passed;
    }
    
    bool ValidateDetectionSystem()
    {

        
        var detectionHelper = GetComponent<NemesisDetectionHelper>();
        if (detectionHelper == null)
        {

            return false;
        }
        
        bool passed = true;
        
        
        if (!detectionHelper.enableCollisions)
        {

        }
        else
        {

        }
        
        
        if (detectionHelper.showDebugRays)
        {

        }
        else
        {

        }
        
        
        if (detectionHelper.obstacleLayerMask == 0)
        {

        }
        else
        {

        }
        
        if (detectionHelper.targetLayerMask == 0)
        {

        }
        else
        {

        }
        
        return passed;
    }
    
    bool ValidateMovementSystem()
    {

        
        bool passed = true;
        
        if (!hasNavMeshAgent)
        {

            passed = false;
        }
        else
        {

            
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent.enabled)
            {

            }
            else
            {

            }
        }
        
        if (!hasRigidbody)
        {

        }
        else
        {

        }
        
        if (!hasCollider)
        {

        }
        else
        {

        }
        
        return passed;
    }
    
    bool ValidateAudioSystem()
    {

        
        bool passed = true;
        
        if (!hasAudioSource)
        {

        }
        else
        {

        }
        
        if (!hasAnimator)
        {

            passed = false;
        }
        else
        {

        }
        
        return passed;
    }
    
    bool ValidateIntegration()
    {

        
        var basicAI = GetComponent<NemesisAI>();
        var enhancedAI = GetComponent<NemesisAI_Enhanced>();
        
        if (basicAI == null && enhancedAI == null)
        {

            return false;
        }
        
        bool passed = true;
        
        float radius = enhancedAI != null ? enhancedAI.detectionRadius : basicAI.detectionRadius;
        float attackRange = enhancedAI != null ? enhancedAI.attackRange : basicAI.attackRange;
        float walkSpeed = enhancedAI != null ? enhancedAI.walkSpeed : basicAI.walkSpeed;
        float chaseSpeed = enhancedAI != null ? enhancedAI.chaseSpeed : basicAI.chaseSpeed;
        
        if (radius <= 0)
        {

            passed = false;
        }
        else
        {

        }
        
        if (attackRange <= 0)
        {

            passed = false;
        }
        else
        {

        }
        
        if (walkSpeed <= 0)
        {

            passed = false;
        }
        else
        {

        }
        
        if (chaseSpeed <= 0)
        {

            passed = false;
        }
        else
        {

        }
        
        return passed;
    }
    
    void UpdateComponentStatus()
    {
        hasNemesisAI = GetComponent<NemesisAI>() != null || GetComponent<NemesisAI_Enhanced>() != null;
        hasDetectionHelper = GetComponent<NemesisDetectionHelper>() != null;
        hasTester = GetComponent<NemesisTester>() != null;
        hasRigidbody = GetComponent<Rigidbody>() != null;
        hasCollider = GetComponent<Collider>() != null;
        hasNavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>() != null;
        hasAnimator = GetComponent<Animator>() != null;
        hasAudioSource = GetComponent<AudioSource>() != null;
    }
    
    [ContextMenu("Auto-Fix Missing Components")]
    public void AutoFixMissingComponents()
    {

        
        if (GetComponent<NemesisAI>() == null)
        {

        }
        
        if (GetComponent<NemesisDetectionHelper>() == null)
        {
            gameObject.AddComponent<NemesisDetectionHelper>();

        }
        
        if (GetComponent<NemesisTester>() == null)
        {
            gameObject.AddComponent<NemesisTester>();

        }
        
        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

        }
        
        if (GetComponent<Collider>() == null)
        {
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 2f;
            col.center = new Vector3(0f, 1f, 0f);

        }
        
        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();

        }
        

    }
}
