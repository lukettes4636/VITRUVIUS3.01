using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneOrganizer
{
    [MenuItem("Tools/Organize DaVinciPB Scene")]
    public static void OrganizeDaVinciPB()
    {
        string scenePath = "Assets/Scenes/DaVinciPB.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        // Crear padres para categorías
        GameObject entornoParent = new GameObject("Entorno");
        GameObject uiParent = new GameObject("UI");
        GameObject entidadesParent = new GameObject("Entidades");

        // Reparentar basados en nombres y layers
        foreach (GameObject root in roots)
        {
            if (root.name.Contains("piso_modulo") || root.name.Contains("Piso_Buffet") || root.layer == 9)
            {
                root.transform.SetParent(entornoParent.transform, true);
            }
            else if (root.name.Contains("Pared_Exte") || root.name.Contains("silla") || root.layer == 11)
            {
                root.transform.SetParent(entornoParent.transform, true);
            }
            else if (root.name.Contains("prompt") || root.GetComponent<Canvas>() != null)
            {
                root.transform.SetParent(uiParent.transform, true);
            }
            // Añadir lógica para Entidades si se detectan (ej. por tag o componente)
            else if (root.tag == "Player" || root.tag == "Enemy" || root.tag == "NPC")
            {
                root.transform.SetParent(entidadesParent.transform, true);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Escena DaVinciPB organizada en categorías: Entorno, UI, Entidades.");
    }
}
