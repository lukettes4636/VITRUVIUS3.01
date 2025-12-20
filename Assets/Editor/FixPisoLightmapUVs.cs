using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FixPisoLightmapUVs : EditorWindow
{
    [MenuItem("Tools/Fix UVs Lightmap para Pisos")]
    public static void ShowWindow()
    {
        GetWindow<FixPisoLightmapUVs>("Fix UVs Lightmap Pisos");
    }

    void OnGUI()
    {
        GUILayout.Label("Reparar UVs de Lightmap para objetos con 'piso'", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Buscar y Reparar UVs"))
        {
            FixLightmapUVsForPisos();
        }
        
        if (GUILayout.Button("Buscar objetos con problemas"))
        {
            FindProblematicObjects();
        }
    }

    public static void FixLightmapUVsForPisos()
    {
        string[] allModelPaths = AssetDatabase.FindAssets("t:Model");
        List<string> pisoModels = new List<string>();
        int fixedCount = 0;
        
        foreach (string guid in allModelPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            
            if (fileName.Contains("piso"))
            {
                pisoModels.Add(path);
            }
        }
        
        if (pisoModels.Count == 0)
        {
            Debug.Log("No se encontraron modelos con 'piso' en el nombre.");
            return;
        }
        
        foreach (string modelPath in pisoModels)
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            
            if (modelImporter != null)
            {
                bool needsFix = false;
                
                if (!modelImporter.generateSecondaryUV)
                {
                    modelImporter.generateSecondaryUV = true;
                    needsFix = true;
                    Debug.Log("Activando Generate Lightmap UVs en: " + modelPath);
                }
                
                if (needsFix)
                {
                    AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
                    fixedCount++;
                }
            }
        }
        
        Debug.Log("Proceso completado. Se repararon " + fixedCount + " modelos con problemas de UVs.");
        
        if (fixedCount > 0)
        {
            EditorUtility.DisplayDialog("Proceso Completado", 
                "Se han reparado " + fixedCount + " modelos.\n\n" +
                "Ahora debes volver a hacer el baking de luces para aplicar los cambios.", "OK");
        }
    }
    
    public static void FindProblematicObjects()
    {
        string[] allModelPaths = AssetDatabase.FindAssets("t:Model");
        List<string> problematicModels = new List<string>();
        
        foreach (string guid in allModelPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            
            if (fileName.Contains("piso"))
            {
                ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
                
                if (modelImporter != null && !modelImporter.generateSecondaryUV)
                {
                    problematicModels.Add(path);
                }
            }
        }
        
        if (problematicModels.Count == 0)
        {
            Debug.Log("No se encontraron modelos con problemas de UVs.");
        }
        else
        {
            Debug.Log("Modelos que necesitan reparación de UVs:");
            foreach (string path in problematicModels)
            {
                Debug.Log(" - " + path);
            }
        }
    }
}