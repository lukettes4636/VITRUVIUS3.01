using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HorrorModelVisibilityFix : MonoBehaviour
{
    [Header("Fix Configuration")]
    [SerializeField] private bool preventModelHiding = true;
    [SerializeField] private bool logFixes = true;
    [SerializeField] private bool restoreVisibilityOnRoarEnd = true;
    [SerializeField] private bool forceRenderersEnabled = true;
    
    [Header("Target Objects")]
    [SerializeField] private GameObject horrorModelRoot;
    [SerializeField] private string modelName = "Horror1_LP";
    
    private Renderer[] modelRenderers;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private bool visibilityFixed = false;
    
    void Start()
    {
        FindHorrorModel();
        if (horrorModelRoot != null)
        {
            CacheRenderers();
            ApplyVisibilityFix();
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
                    if (logFixes)

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
                if (logFixes)
                {

                }
            }
        }
        
        if (horrorModelRoot == null)
        {

        }
    }
    
    void CacheRenderers()
    {
        if (horrorModelRoot != null)
        {
            modelRenderers = horrorModelRoot.GetComponentsInChildren<Renderer>();
            skinnedMeshRenderers = horrorModelRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            if (logFixes)
            {

            }
        }
    }
    
    void ApplyVisibilityFix()
    {
        if (horrorModelRoot == null || modelRenderers == null) return;
        
        
        if (!horrorModelRoot.activeSelf)
        {
            horrorModelRoot.SetActive(true);
            if (logFixes)
            {

            }
        }
        
        
        if (forceRenderersEnabled)
        {
            foreach (Renderer renderer in modelRenderers)
            {
                if (renderer != null && !renderer.enabled)
                {
                    renderer.enabled = true;
                    if (logFixes)
                    {

                    }
                }
            }
        }
        
        
        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedMeshRenderers)
        {
            if (skinnedRenderer != null)
            {
                
                skinnedRenderer.enabled = true;
                
                
                if (skinnedRenderer.bones == null || skinnedRenderer.bones.Length == 0)
                {

                }
                
                if (logFixes)
                {

                }
            }
        }
        
        visibilityFixed = true;
        
        if (logFixes)
        {

        }
    }
    
    
    public void OnRoarStart()
    {
        if (preventModelHiding && logFixes)
        {

        }
    }
    
    
    public void OnRoarEnd()
    {
        if (restoreVisibilityOnRoarEnd)
        {
            ApplyVisibilityFix();
            
            if (logFixes)
            {

            }
        }
    }
    
    
    public void ForceFixVisibility()
    {
        ApplyVisibilityFix();
    }
    
    
    public bool IsModelVisible()
    {
        if (horrorModelRoot == null) return false;
        if (!horrorModelRoot.activeSelf) return false;
        
        foreach (Renderer renderer in modelRenderers)
        {
            if (renderer != null && renderer.enabled)
                return true;
        }
        return false;
    }
}
