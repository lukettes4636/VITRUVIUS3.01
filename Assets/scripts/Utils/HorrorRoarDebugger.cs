using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HorrorRoarDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool logRoarEvents = true;
    [SerializeField] private bool logAnimationEvents = true;
    [SerializeField] private bool logRendererChanges = true;
    [SerializeField] private bool autoFixRoarIssues = true;
    
    [Header("Target Components")]
    [SerializeField] private EnemyVisuals enemyVisuals;
    [SerializeField] private Animator animator;
    
    private Renderer[] cachedRenderers;
    private Rig[] cachedRigs;
    private bool roarActive = false;
    private bool wasVisibleBeforeRoar = true;
    
    void Start()
    {
        FindComponents();
        CacheInitialState();
    }
    
    void FindComponents()
    {
        if (enemyVisuals == null)
            enemyVisuals = GetComponentInParent<EnemyVisuals>();
        
        if (animator == null)
            animator = GetComponentInParent<Animator>();
        
        if (enemyVisuals != null)
        {
            cachedRenderers = enemyVisuals.GetComponentsInChildren<Renderer>();
            cachedRigs = enemyVisuals.GetComponentsInChildren<Rig>();
            
            if (logRoarEvents)
            {

            }
        }
    }
    
    void CacheInitialState()
    {
        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            wasVisibleBeforeRoar = cachedRenderers[0].enabled;
        }
    }
    
    
    void OnRoarStart()
    {
        if (logRoarEvents)
        {

        }
        SetVisibility(false);
    }

    void OnRoarEnd()
    {
        if (logRoarEvents)
        {

        }
        SetVisibility(true);
    }
    
    void CacheVisibilityBeforeRoar()
    {
        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            wasVisibleBeforeRoar = cachedRenderers[0].enabled;
            
            if (logRendererChanges)
            {

                for (int i = 0; i < cachedRenderers.Length; i++)
                {
                    if (cachedRenderers[i] != null)
                    {

                    }
                }
            }
        }
    }
    
    void RestoreVisibilityAfterRoar()
    {
        if (cachedRenderers != null)
        {
            bool anyRendererHidden = false;
            
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null && !cachedRenderers[i].enabled)
                {
                    anyRendererHidden = true;
                    cachedRenderers[i].enabled = true;
                    
                    if (logRendererChanges)
                    {

                    }
                }
            }
            
            if (anyRendererHidden)
            {

            }
        }
    }
    
    void LogCurrentVisibilityState(string context)
    {
        if (cachedRenderers != null && logRendererChanges)
        {

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {

                }
            }

        }
    }

    private void SetVisibility(bool visible)
    {
        if (cachedRenderers != null)
        {
            foreach (Renderer r in cachedRenderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
        if (cachedRigs != null)
        {
            foreach (Rig r in cachedRigs)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }

    [ContextMenu("Test Roar Start")]
    public void TestRoarStart()
    {
        OnRoarStart();
    }
    
    [ContextMenu("Test Roar End")]
    public void TestRoarEnd()
    {
        OnRoarEnd();
    }
}
