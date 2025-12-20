#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HorrorModelVisibilityEditor : EditorWindow
{
    private GameObject horrorModel;
    private bool foundIssues = false;
    private string diagnosticResult = "";
    
    [MenuItem("Tools/Diagnose Horror Model Visibility Issue")]
    public static void ShowWindow()
    {
        GetWindow<HorrorModelVisibilityEditor>("Horror Model Diagnostic");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Horror Model Visibility Diagnostic", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Este herramienta diagnostica y arregla el problema del modelo Horror1_LP que se oculta durante el roar.");
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔍 Buscar y Diagnosticar Modelo Horror", GUILayout.Height(30)))
        {
            FindAndDiagnoseHorrorModel();
        }
        
        GUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(diagnosticResult))
        {
            EditorGUILayout.HelpBox(diagnosticResult, foundIssues ? MessageType.Warning : MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        if (foundIssues)
        {
            if (GUILayout.Button("🔧 Arreglar Visibilidad del Modelo", GUILayout.Height(30)))
            {
                FixHorrorModelVisibility();
            }
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🚀 Agregar Scripts de Protección", GUILayout.Height(30)))
        {
            AddProtectionScripts();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("📋 Ver Logs Detallados", GUILayout.Height(30)))
        {
            ShowDetailedLogs();
        }
    }
    
    void FindAndDiagnoseHorrorModel()
    {
        foundIssues = false;
        diagnosticResult = "";
        
        // Buscar el modelo Horror1_LP
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Horror") && obj.name.Contains("LP"))
            {
                horrorModel = obj;
                break;
            }
        }
        
        if (horrorModel == null)
        {
            diagnosticResult = "❌ No se encontró ningún modelo Horror1_LP en la escena.\n\nAsegúrate de que el enemigo esté instanciado en la escena.";
            return;
        }
        
        // Diagnosticar el modelo
        string result = $"✅ Modelo encontrado: {horrorModel.name}\n\n";
        
        // Verificar si está activo
        if (!horrorModel.activeSelf)
        {
            result += "⚠️ El modelo está DESACTIVADO.\n";
            foundIssues = true;
        }
        else
        {
            result += "✅ El modelo está activo.\n";
        }
        
        // Verificar renderers
        Renderer[] renderers = horrorModel.GetComponentsInChildren<Renderer>();
        int disabledRenderers = 0;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
            {
                disabledRenderers++;
            }
        }
        
        if (disabledRenderers > 0)
        {
            result += $"⚠️ {disabledRenderers} de {renderers.Length} renderers están desactivados.\n";
            foundIssues = true;
        }
        else
        {
            result += $"✅ Todos los renderers ({renderers.Length}) están activos.\n";
        }
        
        // Verificar rigs
        Rig[] rigs = horrorModel.GetComponentsInChildren<Rig>();
        int zeroWeightRigs = 0;
        foreach (Rig rig in rigs)
        {
            if (rig.weight <= 0.01f)
            {
                zeroWeightRigs++;
            }
        }
        
        if (zeroWeightRigs > 0)
        {
            result += $"⚠️ {zeroWeightRigs} de {rigs.Length} rigs tienen peso 0.\n";
        }
        
        // Verificar componentes de protección
        HorrorModelVisibilityFix existingFix = horrorModel.GetComponent<HorrorModelVisibilityFix>();
        HorrorRoarDebugger existingDebugger = horrorModel.GetComponent<HorrorRoarDebugger>();
        
        if (existingFix == null && existingDebugger == null)
        {
            result += "⚠️ No hay scripts de protección instalados.\n";
            foundIssues = true;
        }
        else
        {
            result += "✅ Scripts de protección detectados.\n";
        }
        
        diagnosticResult = result;
    }
    
    void FixHorrorModelVisibility()
    {
        if (horrorModel == null)
        {
            Debug.LogError("No hay modelo para arreglar");
            return;
        }
        
        // Activar el GameObject
        if (!horrorModel.activeSelf)
        {
            horrorModel.SetActive(true);
            Debug.Log($"✅ Activado GameObject: {horrorModel.name}");
        }
        
        // Activar todos los renderers
        Renderer[] renderers = horrorModel.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
            {
                renderer.enabled = true;
                Debug.Log($"✅ Activado renderer: {renderer.name}");
            }
        }
        
        // Ajustar pesos de rigs
        Rig[] rigs = horrorModel.GetComponentsInChildren<Rig>();
        foreach (Rig rig in rigs)
        {
            if (rig.weight <= 0.01f)
            {
                rig.weight = 1f;
                Debug.Log($"✅ Ajustado peso de rig: {rig.name}");
            }
        }
        
        diagnosticResult = "✅ Visibilidad del modelo arreglada exitosamente!\n\nEl modelo Horror1_LP debería ser visible ahora.";
        foundIssues = false;
    }
    
    void AddProtectionScripts()
    {
        if (horrorModel == null)
        {
            FindAndDiagnoseHorrorModel();
        }
        
        if (horrorModel == null)
        {
            Debug.LogError("No se puede agregar protección sin un modelo");
            return;
        }
        
        // Agregar HorrorModelVisibilityFix
        HorrorModelVisibilityFix fix = horrorModel.GetComponent<HorrorModelVisibilityFix>();
        if (fix == null)
        {
            fix = horrorModel.AddComponent<HorrorModelVisibilityFix>();
            Debug.Log("✅ Agregado HorrorModelVisibilityFix");
        }
        else
        {
            Debug.Log("⚠️ HorrorModelVisibilityFix ya existe");
        }
        
        // Agregar HorrorRoarDebugger
        HorrorRoarDebugger debugger = horrorModel.GetComponent<HorrorRoarDebugger>();
        if (debugger == null)
        {
            debugger = horrorModel.AddComponent<HorrorRoarDebugger>();
            Debug.Log("✅ Agregado HorrorRoarDebugger");
        }
        else
        {
            Debug.Log("⚠️ HorrorRoarDebugger ya existe");
        }
        
        diagnosticResult = "✅ Scripts de protección agregados exitosamente!\n\nEl modelo ahora está protegido contra problemas de visibilidad durante el roar.";
    }
    
    void ShowDetailedLogs()
    {
        if (horrorModel == null)
        {
            FindAndDiagnoseHorrorModel();
        }
        
        if (horrorModel == null)
        {
            Debug.LogError("No hay modelo para diagnosticar");
            return;
        }
        
        Debug.Log("=== DIAGNÓSTICO DETALLADO DEL MODELO HORROR ===");
        Debug.Log($"Modelo: {horrorModel.name}");
        Debug.Log($"Activo: {horrorModel.activeSelf}");
        Debug.Log($"Posición: {horrorModel.transform.position}");
        Debug.Log($"Rotación: {horrorModel.transform.rotation.eulerAngles}");
        
        Renderer[] renderers = horrorModel.GetComponentsInChildren<Renderer>();
        Debug.Log($"Total de renderers: {renderers.Length}");
        
        foreach (Renderer renderer in renderers)
        {
            Debug.Log($"  Renderer: {renderer.name} - Enabled: {renderer.enabled} - Visible: {renderer.isVisible}");
        }
        
        SkinnedMeshRenderer[] skinnedRenderers = horrorModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        Debug.Log($"Total de skinned mesh renderers: {skinnedRenderers.Length}");
        
        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
        {
            Debug.Log($"  SkinnedMeshRenderer: {skinnedRenderer.name} - Enabled: {skinnedRenderer.enabled} - Bones: {(skinnedRenderer.bones != null ? skinnedRenderer.bones.Length : 0)}");
        }
        
        Rig[] rigs = horrorModel.GetComponentsInChildren<Rig>();
        Debug.Log($"Total de rigs: {rigs.Length}");
        
        foreach (Rig rig in rigs)
        {
            Debug.Log($"  Rig: {rig.name} - Weight: {rig.weight} - Enabled: {rig.enabled}");
        }
        
        Animator animator = horrorModel.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log($"Animator: {animator.name} - Enabled: {animator.enabled} - Runtime: {animator.runtimeAnimatorController != null}");
        }
        
        Debug.Log("=== FIN DEL DIAGNÓSTICO ===");
    }
}
#endif