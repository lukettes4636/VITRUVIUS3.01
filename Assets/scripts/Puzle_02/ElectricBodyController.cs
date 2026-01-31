using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class ElectricBodyController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Arrastrá acá el PREFAB de tu efecto (VFX_ElectricDamage)")]
    [SerializeField] private GameObject electricVFXPrefab;

    // Lista para guardar los efectos de cada parte del cuerpo
    private List<VisualEffect> vfxInstances = new List<VisualEffect>();

    void Start()
    {
        if (electricVFXPrefab == null)
        {
            Debug.LogError("?? ¡Falta asignar el Prefab Eléctrico en el ElectricBodyController!");
            return;
        }

        SetupBodyParts();
    }

    void SetupBodyParts()
    {
        // 1. Buscamos TODOS los SkinnedMeshRenderers en los hijos (remera, piel, pantalón...)
        SkinnedMeshRenderer[] allMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var meshPart in allMeshes)
        {
            // Filtro opcional: ignoramos cosas muy chicas como pestañas si querés
            if (meshPart.name.Contains("Eye")) continue;

            // 2. Creamos una copia del efecto para ESTA parte del cuerpo
            GameObject vfxObj = Instantiate(electricVFXPrefab, transform.position, Quaternion.identity, transform);
            vfxObj.name = "VFX_Electric_" + meshPart.name;

            VisualEffect vfx = vfxObj.GetComponent<VisualEffect>();

            if (vfx != null)
            {
                // "Cuerpo" es el nombre exacto que pusiste en el Blackboard del Grafo
                vfx.SetSkinnedMeshRenderer("Cuerpo", meshPart);

                // Guardamos la referencia para activarlo después
                vfxInstances.Add(vfx);
            }
        }
    }

    // --- ESTE ES EL MÉTODO QUE LLAMARÁ EL EVENTO DE ANIMACIÓN ---
    public void TriggerElectricVisuals()
    {
        // Recorremos todos los efectos creados y les damos Play
        foreach (var vfx in vfxInstances)
        {
            if (vfx != null) vfx.Play();
        }
    }
}