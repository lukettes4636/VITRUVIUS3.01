using UnityEngine;
using UnityEngine.SceneManagement;




public class NemesisSceneReset : MonoBehaviour
{
    [Header("Scene Reset Configuration")]
    [Tooltip("Should this object persist between scene reloads?")]
    public bool persistBetweenScenes = false;
    
    [Tooltip("Should the Nemesis AI be reset when the scene loads?")]
    public bool resetOnSceneLoad = true;
    
    [Tooltip("Should chase music be reset when the scene loads?")]
    public bool resetChaseMusic = true;
    
    [Tooltip("Should the enemy be respawned at original position?")]
    public bool respawnAtOriginalPosition = true;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private NemesisAI_Enhanced nemesisAI;
    private ChaseMusicController chaseMusicController;
    private bool hasBeenInitialized = false;
    
    void Awake()
    {
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        
        nemesisAI = GetComponent<NemesisAI_Enhanced>();
        chaseMusicController = GetComponent<ChaseMusicController>();
        
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        if (persistBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
        

    }
    
    void OnDestroy()
    {
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    
    
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (resetOnSceneLoad && scene.name == "DaVinciP1")
        {
            ResetNemesis();
        }
    }
    
    
    
    
    public void ResetNemesis()
    {

        
        
        if (respawnAtOriginalPosition)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;

        }
        
        
        if (nemesisAI != null)
        {
            
            nemesisAI.ResetAIState();
            
            
            if (resetChaseMusic)
            {
                nemesisAI.ResetChaseMusic();
            }
            

        }
        
        
        if (chaseMusicController != null && resetChaseMusic)
        {
            chaseMusicController.ResetChaseMusic();

        }
        
        
        var navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.enabled = true;

        }
        
        
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

        }
        
        
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;

        }
        
        
        var detectionHelper = GetComponent<NemesisDetectionHelper>();
        if (detectionHelper != null)
        {
            detectionHelper.ResetDetectionState();

        }
        
        hasBeenInitialized = true;
    }
    
    
    
    
    public Vector3 GetOriginalPosition()
    {
        return originalPosition;
    }
    
    
    
    
    public Quaternion GetOriginalRotation()
    {
        return originalRotation;
    }
    
    
    
    
#if UNITY_EDITOR
    [ContextMenu("Reset Nemesis Now")]
    public void ResetNemesisManual()
    {
        ResetNemesis();
    }
    
    
    
    
    [ContextMenu("Set Current Position as Original")]
    public void SetCurrentAsOriginalPosition()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

    }
    
    void OnDrawGizmosSelected()
    {
        if (Application.isEditor)
        {
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originalPosition, 1f);
            Gizmos.DrawLine(originalPosition, originalPosition + Vector3.up * 2f);
            
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.8f);
            
            
            if (Vector3.Distance(originalPosition, transform.position) > 0.1f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(originalPosition, transform.position);
            }
            
            UnityEditor.Handles.Label(originalPosition + Vector3.up * 2.5f, $"Original Position: {originalPosition}");
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Current Position: {transform.position}");
        }
    }
#endif
}
