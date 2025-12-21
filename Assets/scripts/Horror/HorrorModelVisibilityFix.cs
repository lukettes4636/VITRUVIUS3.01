using UnityEngine;

public class HorrorModelVisibilityFix : MonoBehaviour
{
    [Header("Visibility Fix Settings")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool monitorVisibility = true;
    [SerializeField] private bool fixDuringRoar = true;
    [SerializeField] private bool fixDuringAttack = true;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showGizmos = false;
    
    private Renderer[] renderers;
    private bool wasVisible = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            CacheRenderers();
            FixVisibilityIssues();
        }
    }
    
    void Update()
    {
        if (!monitorVisibility) return;
        
        if (renderers == null || renderers.Length == 0)
        {
            CacheRenderers();
        }
        
        bool currentlyVisible = IsAnyRendererVisible();
        
        if (wasVisible && !currentlyVisible)
        {
            if (enableDebugLogs)

            
            FixVisibilityIssues();
        }
        
        wasVisible = currentlyVisible;
    }
    
    void CacheRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        if (enableDebugLogs)
        {

        }
    }
    
    bool IsAnyRendererVisible()
    {
        if (renderers == null) return false;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.enabled && renderer.isVisible)
                return true;
        }
        return false;
    }
    
    public void FixVisibilityIssues()
    {
        if (renderers == null)
            CacheRenderers();
        
        int fixedCount = 0;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            
            if (!renderer.enabled)
            {
                renderer.enabled = true;
                fixedCount++;
                if (enableDebugLogs)
                {

                }
            }
            
            if (!renderer.gameObject.activeSelf)
            {
                renderer.gameObject.SetActive(true);
                fixedCount++;
                if (enableDebugLogs)
                {

                }
            }
        }
        
        if (enableDebugLogs && fixedCount > 0)
        {

        }
    }
    
    public void SetAutoFixOnStart(bool value) => autoFixOnStart = value;
    public void SetMonitorVisibility(bool value) => monitorVisibility = value;
    public void SetFixDuringRoar(bool value) => fixDuringRoar = value;
    public void SetFixDuringAttack(bool value) => fixDuringAttack = value;
    public void SetDebugLogs(bool value) => enableDebugLogs = value;
    public void SetShowGizmos(bool value) => showGizmos = value;
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = IsAnyRendererVisible() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
