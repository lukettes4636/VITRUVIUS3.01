using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class SymbioteNest : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("El MeshRenderer de esta pared (NO el material)")]
    public Renderer wallRenderer; // <-- CAMBIO 1: Ahora pedimos el Renderer
    [Tooltip("El componente VFX Graph hijo")]
    public VisualEffect tentacleVFX;

    [Header("Objetivos (Víctimas)")]
    [Tooltip("Arrastra aquí a Jugador 1, Jugador 2 y el NPC")]
    public List<Transform> possibleTargets = new List<Transform>();

    [Header("Sensibilidad")]
    public float activationDistance = 5.0f; // Empieza a latir
    public float attackDistance = 2.0f;     // Máxima intensidad

    // Nombres internos del Shader y VFX (Reference Names)
    private int shaderThreatParamID; // <-- CAMBIO 2: Usamos ID para mejor rendimiento
    private string vfxSpawnParam = "SpawnRate";
    private string vfxTargetParam = "TargetPosition";

    private MaterialPropertyBlock propBlock; // <-- CAMBIO 3: Para independizar cada pared

    void Start()
    {
        // Inicializamos el bloque de propiedades y el ID del shader
        propBlock = new MaterialPropertyBlock();
        shaderThreatParamID = Shader.PropertyToID("_ThreatLevel");

        // Si se te olvidó asignarlo en el Inspector, lo busca automáticamente
        if (wallRenderer == null)
        {
            wallRenderer = GetComponent<Renderer>();
        }
    }

    void Update()
    {
        Transform closestTarget = GetClosestTarget(out float minDistance);

        if (closestTarget == null)
        {
            ResetEffects();
            return;
        }

        float threat = Mathf.InverseLerp(activationDistance, attackDistance, minDistance);

        // 3. Controlar SHADER de forma independiente usando PropertyBlock
        if (wallRenderer != null)
        {
            wallRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat(shaderThreatParamID, threat);
            wallRenderer.SetPropertyBlock(propBlock);
        }

        // 4. Controlar VFX (Tentáculos)
        if (tentacleVFX != null)
        {
            tentacleVFX.SetVector3(vfxTargetParam, closestTarget.position);
            tentacleVFX.SetFloat(vfxSpawnParam, threat > 0.05f ? threat * 50f : 0f);
        }
    }

    private Transform GetClosestTarget(out float distance)
    {
        Transform bestTarget = null;
        float closestDistSqr = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (Transform potentialTarget in possibleTargets)
        {
            if (potentialTarget == null) continue;

            Vector3 directionToTarget = potentialTarget.position - currentPos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistSqr)
            {
                closestDistSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        distance = Mathf.Sqrt(closestDistSqr);
        return bestTarget;
    }

    private void ResetEffects()
    {
        if (wallRenderer != null)
        {
            wallRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat(shaderThreatParamID, 0);
            wallRenderer.SetPropertyBlock(propBlock);
        }
        if (tentacleVFX != null) tentacleVFX.SetFloat(vfxSpawnParam, 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}