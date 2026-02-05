using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic; // Necesario para usar Listas

public class SymbioteNest : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("El material de la pared (Mesh Renderer)")]
    public Material wallMaterial;
    [Tooltip("El componente VFX Graph hijo")]
    public VisualEffect tentacleVFX;

    [Header("Objetivos (Víctimas)")]
    [Tooltip("Arrastra aquí a Jugador 1, Jugador 2 y el NPC")]
    public List<Transform> possibleTargets = new List<Transform>();

    [Header("Sensibilidad")]
    public float activationDistance = 5.0f; // Empieza a latir
    public float attackDistance = 2.0f;     // Máxima intensidad

    // Nombres internos del Shader y VFX (Reference Names)
    private string shaderThreatParam = "_ThreatLevel";
    private string vfxSpawnParam = "SpawnRate";
    private string vfxTargetParam = "TargetPosition";

    void Update()
    {
        // 1. Encontrar al objetivo más cercano
        Transform closestTarget = GetClosestTarget(out float minDistance);

        // Si no hay nadie vivo o asignado, apagamos todo
        if (closestTarget == null)
        {
            ResetEffects();
            return;
        }

        // 2. Calcular Nivel de Amenaza (0 a 1) basado en el más cercano
        // Mathf.InverseLerp devuelve 0 si estamos lejos y 1 si estamos cerca
        float threat = Mathf.InverseLerp(activationDistance, attackDistance, minDistance);

        // 3. Controlar SHADER (Pared)
        if (wallMaterial != null)
        {
            wallMaterial.SetFloat(shaderThreatParam, threat);
        }

        // 4. Controlar VFX (Tentáculos)
        if (tentacleVFX != null)
        {
            // Los tentáculos apuntan al que esté más cerca
            tentacleVFX.SetVector3(vfxTargetParam, closestTarget.position);

            if (threat > 0.05f)
            {
                // Multiplicamos por 150 para que salgan muchos gusanos
                tentacleVFX.SetFloat(vfxSpawnParam, threat * 50f);
            }
            else
            {
                tentacleVFX.SetFloat(vfxSpawnParam, 0f);
            }
        }
    }

    // Función auxiliar para buscar el más cercano
    private Transform GetClosestTarget(out float distance)
    {
        Transform bestTarget = null;
        float closestDistSqr = Mathf.Infinity; // Usamos distancia al cuadrado (es más rápido)
        Vector3 currentPos = transform.position;

        foreach (Transform potentialTarget in possibleTargets)
        {
            if (potentialTarget == null) continue; // Si el NPC murió o se destruyó, lo ignoramos

            Vector3 directionToTarget = potentialTarget.position - currentPos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistSqr)
            {
                closestDistSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        // Devolvemos la distancia real (raíz cuadrada) para los cálculos de amenaza
        distance = Mathf.Sqrt(closestDistSqr);
        return bestTarget;
    }

    private void ResetEffects()
    {
        if (wallMaterial != null) wallMaterial.SetFloat(shaderThreatParam, 0);
        if (tentacleVFX != null) tentacleVFX.SetFloat(vfxSpawnParam, 0);
    }

    // Dibujos de ayuda en el Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}