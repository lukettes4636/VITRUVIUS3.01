using UnityEngine;





public class HorrorEnemyIntegration : MonoBehaviour
{
    [Header("=== HORROR ENEMY INTEGRATION ===")]
    [SerializeField] private bool autoApplyFixes = true;
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private bool showGizmos = true;
    
    [Header("=== VISIBILITY FIXES ===")]
    [SerializeField] private bool fixVisibilityIssues = true;
    [SerializeField] private bool monitorVisibility = true;
    [SerializeField] private bool fixDuringRoar = true;
    [SerializeField] private bool fixDuringAttack = true;
    
    [Header("=== DETECTION IMPROVEMENTS ===")]
    [SerializeField] private bool improveDetection = true;
    [SerializeField] private bool enableSoundDetection = true;
    [SerializeField] private bool prioritizeTargets = true;
    
    [Header("=== ATTACK FIXES ===")]
    [SerializeField] private bool fixAttackIssues = true;
    [SerializeField] private bool ensureAttackAnimation = true;
    [SerializeField] private bool enableDamageDealing = true;
    
    
    private NemesisAI nemesisAI;
    private HorrorModelVisibilityFix visibilityFix;
    
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    
    void Start()
    {
        if (autoApplyFixes)
        {
            ApplyAllFixes();
        }
    }
    
    void ApplyAllFixes()
    {

        
        
        CacheComponents();
        
        
        if (fixVisibilityIssues)
        {
            ApplyVisibilityFixes();
        }
        
        
        if (improveDetection)
        {
            ApplyDetectionImprovements();
        }
        
        
        if (fixAttackIssues)
        {
            ApplyAttackFixes();
        }
        
        
        ValidateSetup();
        

    }
    
    void CacheComponents()
    {
        
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        visibilityFix = GetComponent<HorrorModelVisibilityFix>();
        
        
        if (enableDebugMode)
        {





        }
    }
    
    void ApplyVisibilityFixes()
    {
        if (visibilityFix == null)
        {
            return;
        }
        
        visibilityFix.SetAutoFixOnStart(autoApplyFixes);
        visibilityFix.SetMonitorVisibility(monitorVisibility);
        visibilityFix.SetDebugLogs(enableDebugMode);
        visibilityFix.SetShowGizmos(showGizmos);
        
        if (enableDebugMode)
        {

        }
    }
    
    void ApplyDetectionImprovements()
    {

        
        if (nemesisAI == null)
        {

            return;
        }
        
        
        if (enableSoundDetection)
        {
            
            if (nemesisAI.soundDetectionRadius <= 0)
            {
                nemesisAI.soundDetectionRadius = 15f; 
            }
        }
        
        
        if (nemesisAI.detectionLayerMask == 0)
        {
            nemesisAI.detectionLayerMask = LayerMask.GetMask("Player", "NPC");
        }
        
        if (nemesisAI.soundBlockerLayer == 0)
        {
            nemesisAI.soundBlockerLayer = LayerMask.GetMask("Default", "Environment");
        }
    }
    
    void ApplyAttackFixes()
    {
        if (nemesisAI == null)
        {
            return;
        }
        
        if (nemesisAI.attackDamage <= 0)
        {
            nemesisAI.attackDamage = 25; 
        }
        
        if (nemesisAI.attackRange <= 0)
        {
            nemesisAI.attackRange = 2f; 
        }
        
        if (nemesisAI.attackCooldown <= 0)
        {
            nemesisAI.attackCooldown = 2f; 
        }
        
        if (animator != null && ensureAttackAnimation)
        {
            bool hasAttackParam = false;
            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.GetParameter(i).name == "Attack")
                {
                    hasAttackParam = true;
                    break;
                }
            }
            
            if (!hasAttackParam)
            {

            }
        }
    }
    
    void ValidateSetup()
    {

        
        
        bool hasCriticalIssues = false;
        
        if (nemesisAI == null)
        {

            hasCriticalIssues = true;
        }
        
        if (animator == null)
        {

        }
        
        if (agent == null)
        {

        }
        
        if (!hasCriticalIssues)
        {

        }
        else
        {

        }
    }
    
    
    [ContextMenu("Apply Fixes Now")]
    public void ApplyFixesNow()
    {
        ApplyAllFixes();
    }
    
    [ContextMenu("Check Status")]
    public void CheckStatus()
    {
        if (enableDebugMode)
        {






        }
    }
    
    [ContextMenu("Test Detection")]
    public void TestDetection()
    {
        if (nemesisAI == null)
        {

            return;
        }
        

        
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        GameObject npc = GameObject.FindGameObjectWithTag("NPC");
        
        if (enableDebugMode)
        {




        }
    }
    
    [ContextMenu("Test Attack")]
    public void TestAttack()
    {
        if (animator == null)
        {

            return;
        }
        

        
        
        animator.SetTrigger("Attack");
        

    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (nemesisAI != null)
        {
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, nemesisAI.detectionRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, nemesisAI.attackRange);
            
            if (enableSoundDetection)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, nemesisAI.soundDetectionRadius);
            }
            
            
            Transform currentTarget = GetCurrentTarget();
            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
        }
    }
    
    
    private Transform GetCurrentTarget()
    {
        if (nemesisAI != null)
        {
            
            var targetField = nemesisAI.GetType().GetField("currentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null)
            {
                return targetField.GetValue(nemesisAI) as Transform;
            }
        }
        return null;
    }
}
