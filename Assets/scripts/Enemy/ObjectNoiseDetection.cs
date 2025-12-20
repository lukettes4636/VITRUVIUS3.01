using UnityEngine;
using System.Collections.Generic;
using Gameplay;





public class ObjectNoiseDetection : MonoBehaviour
{
    [Header("Deteccion de Objetos")]
    [Tooltip("Objetos que pueden emitir ruido (se pueden detectar automaticamente o asignar manualmente)")]
    public ObjectNoiseEmitter[] objectNoiseTargets;

    [Tooltip("Buscar automaticamente todos los ObjectNoiseEmitter en la escena")]
    public bool autoFindObjects = true;

    [Header("Configuracion")]
    [Tooltip("Radio maximo de deteccion de objetos")]
    public float maxDetectionDistance = 20f;
    [Tooltip("Nivel minimo de ruido para detectar")]
    public float detectionThreshold = 2f;

    private List<ObjectNoiseEmitter> activeNoiseObjects = new List<ObjectNoiseEmitter>();

    void Start()
    {
        if (autoFindObjects)
        {
            FindAllNoiseObjects();
        }
        else
        {
            activeNoiseObjects.AddRange(objectNoiseTargets);
        }
    }

    void FindAllNoiseObjects()
    {
        ObjectNoiseEmitter[] allObjects = FindObjectsOfType<ObjectNoiseEmitter>();
        activeNoiseObjects.Clear();
        activeNoiseObjects.AddRange(allObjects);
        
        if (objectNoiseTargets != null && objectNoiseTargets.Length > 0)
        {
            foreach (var obj in objectNoiseTargets)
            {
                if (obj != null && !activeNoiseObjects.Contains(obj))
                    activeNoiseObjects.Add(obj);
            }
        }
    }

    
    
    
    public bool GetLoudestObject(out Transform objectTransform, out Vector3 position)
    {
        objectTransform = null;
        position = Vector3.zero;
        float maxNoise = 0f;
        Transform loudest = null;

        foreach (var noiseObj in activeNoiseObjects)
        {
            if (noiseObj == null || noiseObj.currentNoiseRadius < detectionThreshold) continue;

            float dist = Vector3.Distance(transform.position, noiseObj.transform.position);
            if (dist > maxDetectionDistance) continue;

            
            float effectiveNoise = noiseObj.currentNoiseRadius * (1f - (dist / maxDetectionDistance));
            if (effectiveNoise > maxNoise)
            {
                maxNoise = effectiveNoise;
                loudest = noiseObj.transform;
            }
        }

        if (loudest != null)
        {
            objectTransform = loudest;
            position = loudest.position;
            return true;
        }

        return false;
    }

    
    
    
    public bool HasNoisyObjectNearby()
    {
        foreach (var noiseObj in activeNoiseObjects)
        {
            if (noiseObj == null || noiseObj.currentNoiseRadius < detectionThreshold) continue;

            float dist = Vector3.Distance(transform.position, noiseObj.transform.position);
            if (dist <= maxDetectionDistance && dist <= noiseObj.currentNoiseRadius)
            {
                return true;
            }
        }
        return false;
    }
}

