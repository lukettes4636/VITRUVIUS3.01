using UnityEngine;

public class NemesisTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Enable debug logging for detection events")]
    public bool enableDebugLogging = true;
    
    [Tooltip("Show detection gizmos in Scene view")]
    public bool showDetectionGizmos = true;
    
    [Header("Test Objects")]
    public GameObject testPlayer1;
    public GameObject testPlayer2;
    public GameObject testNPC;
    
    private NemesisAI nemesisAI;
    private NemesisDetectionHelper detectionHelper;
    
    void Start()
    {
        nemesisAI = GetComponent<NemesisAI>();
        detectionHelper = GetComponent<NemesisDetectionHelper>();
        
        if (nemesisAI == null)
        {

            return;
        }
        
        if (detectionHelper == null)
        {

            return;
        }
        

        
        if (enableDebugLogging)
        {



        }
    }
    
    void Update()
    {
        if (!enableDebugLogging) return;
        
        if (nemesisAI != null)
        {
            Transform currentTarget = nemesisAI.GetCurrentTarget();
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);

            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDetectionGizmos) return;
        
        if (nemesisAI != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, nemesisAI.detectionRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, nemesisAI.soundDetectionRadius);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, nemesisAI.attackRange);
        }
        
        if (detectionHelper != null && detectionHelper.showDebugRays)
        {
            if (testPlayer1 != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, testPlayer1.transform.position);
            }
            
            if (testPlayer2 != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, testPlayer2.transform.position);
            }
            
            if (testNPC != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, testNPC.transform.position);
            }
        }
    }
    
    [ContextMenu("Test Player1 Detection")]
    void TestPlayer1Detection()
    {
        if (testPlayer1 == null)
        {

            return;
        }
        
        if (detectionHelper != null)
        {
            bool canDetect = detectionHelper.CheckTargetDetection(testPlayer1.transform, nemesisAI.detectionRadius, "Player1");

            
            if (canDetect)
            {
                nemesisAI.SetAlerted(true);
            }
        }
    }
    
    [ContextMenu("Test Player2 Detection")]
    void TestPlayer2Detection()
    {
        if (testPlayer2 == null)
        {

            return;
        }
        
        if (detectionHelper != null)
        {
            bool canDetect = detectionHelper.CheckTargetDetection(testPlayer2.transform, nemesisAI.detectionRadius, "Player2");

            
            if (canDetect)
            {
                nemesisAI.SetAlerted(true);
            }
        }
    }
    
    [ContextMenu("Test NPC Detection")]
    void TestNPCDetection()
    {
        if (testNPC == null)
        {

            return;
        }
        
        if (detectionHelper != null)
        {
            bool canDetect = detectionHelper.CheckTargetDetection(testNPC.transform, nemesisAI.detectionRadius, "NPC");

            
            if (canDetect)
            {
                nemesisAI.SetAlerted(true);
            }
        }
    }
    
    [ContextMenu("Reset Nemesis State")]
    void ResetNemesisState()
    {
        if (nemesisAI != null)
        {
            nemesisAI.SetAlerted(false);

        }
    }
}
