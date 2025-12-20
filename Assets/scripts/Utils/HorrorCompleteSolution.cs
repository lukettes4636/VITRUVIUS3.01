using UnityEngine;
using UnityEngine.Animations.Rigging;





public class HorrorCompleteSolution : MonoBehaviour
{
    [Header("=== HORROR VISIBILITY SOLUTION ===")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool monitorVisibility = true;
    [SerializeField] private bool fixDuringRoar = true;
    [SerializeField] private bool fixDuringAttack = true;
    
    [Header("Target Model")]
    [SerializeField] private GameObject horrorModel;
    [SerializeField] private string horrorModelName = "Horror1_LP";
    
    [Header("Debug Options")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool showGizmos = true;
    
    
    private Renderer[] allRenderers;
    private SkinnedMeshRenderer[] skinnedRenderers;
    private GameObject[] allChildObjects;
    
    
    private bool wasVisibleBeforeRoar = true;
    private bool wasVisibleBeforeAttack = true;
    private int visibilityCheckFrame = 0;
    private const int VISIBILITY_CHECK_INTERVAL = 30; 
    
    
    public bool AutoFixOnStart { get => autoFixOnStart; set => autoFixOnStart = value; }
    public bool MonitorVisibility { get => monitorVisibility; set => monitorVisibility = value; }
    public bool FixDuringRoar { get => fixDuringRoar; set => fixDuringRoar = value; }
    public bool FixDuringAttack { get => fixDuringAttack; set => fixDuringAttack = value; }
    public bool DebugLogs { get => debugLogs; set => debugLogs = value; }
    public bool ShowGizmos { get => showGizmos; set => showGizmos = value; }
    
    void Start()
    {
        InitializeSolution();
        
        if (autoFixOnStart)
        {
            FixAllVisibilityIssues();
        }
    }
    
    void Update()
    {
        if (monitorVisibility)
        {
            visibilityCheckFrame++;
            if (visibilityCheckFrame >= VISIBILITY_CHECK_INTERVAL)
            {
                visibilityCheckFrame = 0;
                CheckAndFixVisibilityIssues();
            }
        }
    }
    
    void InitializeSolution()
    {

        
        FindHorrorModel();
        CacheComponents();
        
        if (debugLogs)
        {

        }
    }
    
    void FindHorrorModel()
    {
        if (horrorModel == null)
        {
            horrorModel = GameObject.Find(horrorModelName);
        }
        
        if (horrorModel == null)
        {
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name.Contains("Horror") && t.name.Contains("LP"))
                {
                    horrorModel = t.gameObject;

                    break;
                }
            }
        }
        
        if (horrorModel == null)
        {
            GameObject enemyRoot = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyRoot != null)
            {
                horrorModel = enemyRoot;

            }
        }
        
        if (horrorModel == null)
        {

        }
    }
    
    void CacheComponents()
    {
        if (horrorModel != null)
        {
            allRenderers = horrorModel.GetComponentsInChildren<Renderer>();
            skinnedRenderers = horrorModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            allChildObjects = new GameObject[horrorModel.transform.childCount];
            for (int i = 0; i < horrorModel.transform.childCount; i++)
            {
                allChildObjects[i] = horrorModel.transform.GetChild(i).gameObject;
            }
        }
        else
        {
            allRenderers = new Renderer[0];
            skinnedRenderers = new SkinnedMeshRenderer[0];
            allChildObjects = new GameObject[0];
        }
    }
    
    public void FixAllVisibilityIssues()
    {

        
        FixGameObjectActiveState();
        FixRenderers();
        FixSkinnedMeshRenderers();
        ValidateBones();
        

    }
    
    void CheckAndFixVisibilityIssues()
    {
        bool needsFix = false;
        
        
        if (horrorModel != null && !horrorModel.activeSelf)
        {
            needsFix = true;
        }
        
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && !renderer.enabled)
            {
                needsFix = true;
                break;
            }
        }
        
        if (needsFix)
        {

            FixAllVisibilityIssues();
        }
    }
    
    void FixGameObjectActiveState()
    {
        if (horrorModel != null && !horrorModel.activeSelf)
        {
            horrorModel.SetActive(true);

        }
        
        
        foreach (GameObject child in allChildObjects)
        {
            if (child != null && !child.activeSelf)
            {
                child.SetActive(true);

            }
        }
    }
    
    void FixRenderers()
    {
        int fixedCount = 0;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && !renderer.enabled)
            {
                renderer.enabled = true;
                fixedCount++;

            }
        }
        

    }
    
    void FixSkinnedMeshRenderers()
    {
        int fixedCount = 0;
        
        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
        {
            if (skinnedRenderer != null && !skinnedRenderer.enabled)
            {
                skinnedRenderer.enabled = true;
                fixedCount++;

            }
        }
        

    }
    
    void ValidateBones()
    {
        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
        {
            if (skinnedRenderer != null)
            {
                if (skinnedRenderer.bones == null || skinnedRenderer.bones.Length == 0)
                {

                }
                
                if (skinnedRenderer.rootBone == null)
                {

                }
            }
        }
    }
    
    
    public void OnRoarStart()
    {
        if (fixDuringRoar)
        {
            wasVisibleBeforeRoar = IsAnyRendererVisible();

        }
    }
    
    public void OnRoarEnd()
    {
        if (fixDuringRoar)
        {
            FixAllVisibilityIssues();

        }
    }
    
    public void OnAttackStart()
    {
        if (fixDuringAttack)
        {
            wasVisibleBeforeAttack = IsAnyRendererVisible();

        }
    }
    
    public void OnAttackEnd()
    {
        if (fixDuringAttack)
        {
            FixAllVisibilityIssues();

        }
    }
    
    bool IsAnyRendererVisible()
    {
        if (horrorModel == null || !horrorModel.activeSelf) return false;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer.enabled)
                return true;
        }
        return false;
    }
    
    
    public void SetAutoFixOnStart(bool value) { autoFixOnStart = value; }
    public void SetMonitorVisibility(bool value) { monitorVisibility = value; }
    public void SetFixDuringRoar(bool value) { fixDuringRoar = value; }
    public void SetFixDuringAttack(bool value) { fixDuringAttack = value; }
    public void SetDebugLogs(bool value) { debugLogs = value; }
    public void SetShowGizmos(bool value) { showGizmos = value; }
    
    
    [ContextMenu("Force Fix Visibility")]
    public void ForceFixVisibility()
    {
        FixAllVisibilityIssues();
    }
    
    [ContextMenu("Check Visibility Status")]
    public void CheckVisibilityStatus()
    {
        bool isVisible = IsAnyRendererVisible();


    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (horrorModel != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(horrorModel.transform.position, 2f);
            
            if (IsAnyRendererVisible())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawSphere(horrorModel.transform.position + Vector3.up * 3f, 0.5f);
        }
    }
}
