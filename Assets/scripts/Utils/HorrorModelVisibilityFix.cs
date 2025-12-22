using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HorrorModelVisibilityFix : MonoBehaviour
{
    
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool monitorVisibility = true;
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showGizmos = true;

    private Renderer[] renderers;
    private Rig[] rigs;

    public void SetAutoFixOnStart(bool value) => autoFixOnStart = value;
    public void SetMonitorVisibility(bool value) => monitorVisibility = value;
    public void SetDebugLogs(bool value) => enableDebugLogs = value;
    public void SetShowGizmos(bool value) => showGizmos = value;

    void Start()
    {
        CacheComponents();
        if (autoFixOnStart)
        {
            ApplyFix();
        }
    }

    void Update()
    {
        if (monitorVisibility)
        {
            ApplyFix();
        }
    }

    private void CacheComponents()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        rigs = GetComponentsInChildren<Rig>(true);
    }

    public void ApplyFix()
    {
        if (renderers == null) CacheComponents();

        #if UNITY_EDITOR
        bool fixedAny = false;
        #endif

        foreach (var renderer in renderers)
        {
            if (renderer != null && !renderer.enabled)
            {
                renderer.enabled = true;
                #if UNITY_EDITOR
                fixedAny = true;
                #endif
                #if UNITY_EDITOR

                #endif
            }
        }

        foreach (var rig in rigs)
        {
            if (rig != null && rig.weight < 0.99f)
            {
                rig.weight = 1f;
                #if UNITY_EDITOR
                fixedAny = true;
                #endif
                #if UNITY_EDITOR

                #endif
            }
        }

        #if UNITY_EDITOR
        if (fixedAny && enableDebugLogs)
        {
            
        }
        #endif
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(0.5f, 2f, 0.5f));
    }
    #endif
}
