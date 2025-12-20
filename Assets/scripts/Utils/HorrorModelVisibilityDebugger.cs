using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HorrorModelVisibilityDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool logVisibilityChanges = true;
    [SerializeField] private bool logRendererStates = true;
    [SerializeField] private bool logRigStates = true;
    [SerializeField] private bool autoFixVisibilityIssues = true;
    
    [Header("Target Objects")]
    [SerializeField] private GameObject horrorModelRoot;
    [SerializeField] private string modelName = "Horror1_LP";
    
    private Renderer[] modelRenderers;
    private Rig[] rigs;
    private Animator animator;
    private EnemyVisuals enemyVisuals;
    
    private bool[] initialRendererStates;
    private float[] initialRigWeights;
    
    void Start()
    {
        FindHorrorModel();
        if (horrorModelRoot != null)
        {
            CacheComponents();
            CacheInitialStates();
            LogInitialState();
        }
        else
        {

        }
    }
    
    void Update()
    {
        if (horrorModelRoot != null && logVisibilityChanges)
        {
            CheckForVisibilityChanges();
        }
    }
    
    void FindHorrorModel()
    {
        if (horrorModelRoot == null)
        {
            horrorModelRoot = GameObject.Find(modelName);
        }
        
        if (horrorModelRoot == null)
        {
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains("Horror") && t.name.Contains("LP"))
                {
                    horrorModelRoot = t.gameObject;

                    break;
                }
            }
        }
        
        if (horrorModelRoot == null)
        {
            GameObject enemyRoot = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyRoot != null)
            {
                horrorModelRoot = enemyRoot;

            }
        }
    }
    
    void CacheComponents()
    {
        modelRenderers = horrorModelRoot.GetComponentsInChildren<Renderer>();
        rigs = horrorModelRoot.GetComponentsInChildren<Rig>();
        animator = horrorModelRoot.GetComponentInChildren<Animator>();
        enemyVisuals = horrorModelRoot.GetComponentInParent<EnemyVisuals>();
        

    }
    
    void CacheInitialStates()
    {
        initialRendererStates = new bool[modelRenderers.Length];
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i] != null)
            {
                initialRendererStates[i] = modelRenderers[i].enabled;
            }
        }
        
        initialRigWeights = new float[rigs.Length];
        for (int i = 0; i < rigs.Length; i++)
        {
            if (rigs[i] != null)
            {
                initialRigWeights[i] = rigs[i].weight;
            }
        }
    }
    
    void LogInitialState()
    {




        
        if (logRendererStates)
        {
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                if (modelRenderers[i] != null)
                {

                }
            }
        }
        
        if (logRigStates)
        {
            for (int i = 0; i < rigs.Length; i++)
            {
                if (rigs[i] != null)
                {

                }
            }
        }
        
        if (animator != null)
        {

        }
        
        if (enemyVisuals != null)
        {

        }
    }
    
    void CheckForVisibilityChanges()
    {
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i] != null && modelRenderers[i].enabled != initialRendererStates[i])
            {

                
                if (autoFixVisibilityIssues)
                {
                    modelRenderers[i].enabled = initialRendererStates[i];

                }
            }
        }
        
        for (int i = 0; i < rigs.Length; i++)
        {
            if (rigs[i] != null && Mathf.Abs(rigs[i].weight - initialRigWeights[i]) > 0.01f)
            {

            }
        }
        
        if (horrorModelRoot != null && !horrorModelRoot.activeSelf)
        {

            
            if (autoFixVisibilityIssues)
            {
                horrorModelRoot.SetActive(true);

            }
        }
    }
    
    [ContextMenu("Force Check Visibility")]
    public void ForceCheckVisibility()
    {
        LogCurrentState();
    }
    
    [ContextMenu("Force Fix Visibility")]
    public void ForceFixVisibility()
    {
        if (horrorModelRoot != null)
        {
            horrorModelRoot.SetActive(true);
            
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                if (modelRenderers[i] != null)
                {
                    modelRenderers[i].enabled = true;
                }
            }
            

        }
    }
    
    void LogCurrentState()
    {



        
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            if (modelRenderers[i] != null)
            {

            }
        }
        
        for (int i = 0; i < rigs.Length; i++)
        {
            if (rigs[i] != null)
            {

            }
        }
    }
}
