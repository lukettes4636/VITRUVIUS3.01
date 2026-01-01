using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Sistema de detección de ruido simplificado.
/// Solo detecta por SONIDO en rango fijo de 10m.
/// </summary>
public class EnemySenses : MonoBehaviour
{
    [Header("Referencias de Targets")]
    public Transform[] playerTargets;
    public Transform[] npcTargets;
    public ObjectNoiseDetection objectNoiseDetection;

    [Header("Configuración de Detección")]
    [Tooltip("Rango máximo de audición del enemigo")]
    public float detectionRange = 10f;

    [Tooltip("Rango ultra cercano para detectar idle")]
    public float ultraCloseRange = 2f;

    [Tooltip("Ruido mínimo para detectar (ignora idle estático)")]
    public float minNoiseToDetect = 0.1f;

    [Tooltip("Threshold de ruido para objetos")]
    public float objectNoiseThreshold = 2f;

    // PROPIEDADES PÚBLICAS
    public Transform CurrentTarget { get; private set; }
    public Vector3 LastKnownPosition { get; private set; }
    public bool HasTarget { get; private set; }
    public float CurrentTargetNoiseLevel { get; private set; }

    // CACHE DE COMPONENTES
    private struct TargetCache
    {
        public Transform transform;
        public PlayerHealth playerHealth;
        public NPCHealth npcHealth;
        public PlayerNoiseEmitter playerNoise;
        public NPCNoiseEmitter npcNoise;
    }

    private List<TargetCache> playerCache = new List<TargetCache>();
    private List<TargetCache> npcCache = new List<TargetCache>();

    void Start()
    {
        InitializeCaches();
    }

    private void InitializeCaches()
    {
        // Cache jugadores
        playerCache.Clear();
        if (playerTargets != null)
        {
            foreach (var p in playerTargets)
            {
                if (p == null) continue;
                playerCache.Add(new TargetCache
                {
                    transform = p,
                    playerHealth = p.GetComponent<PlayerHealth>(),
                    playerNoise = p.GetComponent<PlayerNoiseEmitter>()
                });
            }
        }

        // Cache NPCs
        npcCache.Clear();
        if (npcTargets != null)
        {
            foreach (var n in npcTargets)
            {
                if (n == null) continue;
                npcCache.Add(new TargetCache
                {
                    transform = n,
                    npcHealth = n.GetComponent<NPCHealth>(),
                    npcNoise = n.GetComponent<NPCNoiseEmitter>()
                });
            }
        }
    }

    /// <summary>
    /// Llamar cada frame desde EnemyBrain.
    /// Busca el mejor target disponible.
    /// </summary>
    public void Tick(bool canDetectObjects = true)
    {
        List<DetectedTarget> candidates = new List<DetectedTarget>();

        // 1. DETECTAR JUGADORES
        foreach (var cache in playerCache)
        {
            if (!IsTargetValid(cache.transform, cache.playerHealth, null)) continue;

            float noise = cache.playerNoise != null ? cache.playerNoise.currentNoiseRadius : 0f;
            float distance = Vector3.Distance(transform.position, cache.transform.position);

            if (IsAudible(distance, noise))
            {
                candidates.Add(new DetectedTarget
                {
                    transform = cache.transform,
                    noiseLevel = noise,
                    distance = distance,
                    type = TargetType.Player
                });
            }
        }

        // 2. DETECTAR NPCs
        foreach (var cache in npcCache)
        {
            if (!IsTargetValid(cache.transform, null, cache.npcHealth)) continue;

            float noise = cache.npcNoise != null ? cache.npcNoise.currentNoiseRadius : 0f;
            float distance = Vector3.Distance(transform.position, cache.transform.position);

            if (IsAudible(distance, noise))
            {
                candidates.Add(new DetectedTarget
                {
                    transform = cache.transform,
                    noiseLevel = noise,
                    distance = distance,
                    type = TargetType.NPC
                });
            }
        }

        // 3. DETECTAR OBJETOS (solo si está permitido)
        if (canDetectObjects && objectNoiseDetection != null)
        {
            if (objectNoiseDetection.GetLoudestObject(out Transform objTransform, out Vector3 objPosition))
            {
                ObjectNoiseEmitter objNoise = objTransform.GetComponent<ObjectNoiseEmitter>();
                if (objNoise != null)
                {
                    float noise = objNoise.currentNoiseRadius;
                    float distance = Vector3.Distance(transform.position, objPosition);

                    // Threshold más alto para objetos
                    if (distance <= detectionRange && noise >= objectNoiseThreshold)
                    {
                        candidates.Add(new DetectedTarget
                        {
                            transform = objTransform,
                            noiseLevel = noise,
                            distance = distance,
                            type = TargetType.Object
                        });
                    }
                }
            }
        }

        // 4. SELECCIONAR MEJOR TARGET
        if (candidates.Count > 0)
        {
            // Ordenar por: 1) Mayor ruido, 2) Más cercano
            var bestTarget = candidates
                .OrderByDescending(t => t.noiseLevel)
                .ThenBy(t => t.distance)
                .First();

            CurrentTarget = bestTarget.transform;
            LastKnownPosition = bestTarget.transform.position;
            CurrentTargetNoiseLevel = bestTarget.noiseLevel;
            HasTarget = true;
        }
        else
        {
            // No hay targets audibles
            CurrentTarget = null;
            HasTarget = false;
            // LastKnownPosition se mantiene para investigación
        }
    }

    /// <summary>
    /// Verifica si un target es audible según distancia y ruido.
    /// </summary>
    private bool IsAudible(float distance, float noiseRadius)
    {
        // Detección ultra cercana (idle)
        if (distance <= ultraCloseRange && noiseRadius >= minNoiseToDetect)
            return true;

        // Detección normal
        if (distance <= detectionRange && noiseRadius > minNoiseToDetect)
            return true;

        return false;
    }

    /// <summary>
    /// Valida que el target siga vivo y disponible.
    /// </summary>
    private bool IsTargetValid(Transform target, PlayerHealth playerHealth, NPCHealth npcHealth)
    {
        if (target == null) return false;
        if (playerHealth != null && playerHealth.IsDead) return false;
        if (npcHealth != null && npcHealth.IsDead) return false;
        return true;
    }

    /// <summary>
    /// Verifica si el target actual sigue haciendo ruido.
    /// </summary>
    public bool IsCurrentTargetStillAudible()
    {
        if (CurrentTarget == null) return false;

        float distance = Vector3.Distance(transform.position, CurrentTarget.position);
        float noise = GetTargetNoiseRadius(CurrentTarget);

        return IsAudible(distance, noise);
    }

    /// <summary>
    /// Obtiene el radio de ruido de un target específico.
    /// </summary>
    private float GetTargetNoiseRadius(Transform target)
    {
        var playerNoise = target.GetComponent<PlayerNoiseEmitter>();
        if (playerNoise != null) return playerNoise.currentNoiseRadius;

        var npcNoise = target.GetComponent<NPCNoiseEmitter>();
        if (npcNoise != null) return npcNoise.currentNoiseRadius;

        var objNoise = target.GetComponent<ObjectNoiseEmitter>();
        if (objNoise != null) return objNoise.currentNoiseRadius;

        return 0f;
    }

    /// <summary>
    /// Limpia el target actual.
    /// </summary>
    public void ClearTarget()
    {
        CurrentTarget = null;
        HasTarget = false;
        CurrentTargetNoiseLevel = 0f;
    }

    /// <summary>
    /// Verifica si hay una pared destructible en el camino.
    /// </summary>
    public bool CheckWallInPath(out GameObject wallTarget, LayerMask wallLayer, float checkDistance = 5f)
    {
        wallTarget = null;

        if (!HasTarget) return false;

        Vector3 start = transform.position + Vector3.up;
        Vector3 direction = (LastKnownPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, LastKnownPosition);
        float checkDist = Mathf.Min(distance, checkDistance);

        // Raycast simple
        if (Physics.Raycast(start, direction, out RaycastHit hit, checkDist, wallLayer))
        {
            if (hit.collider.gameObject.activeInHierarchy)
            {
                wallTarget = hit.collider.gameObject;
                return true;
            }
        }

        return false;
    }

    // STRUCT AUXILIAR
    private struct DetectedTarget
    {
        public Transform transform;
        public float noiseLevel;
        public float distance;
        public TargetType type;
    }

    private enum TargetType { Player, NPC, Object }

    // DEBUG
    void OnDrawGizmosSelected()
    {
        // Rango de detección normal
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango ultra cercano
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, ultraCloseRange);

        // Target actual
        if (HasTarget && CurrentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, CurrentTarget.position);
            Gizmos.DrawWireSphere(CurrentTarget.position, 0.5f);
        }

        // Última posición conocida
        if (LastKnownPosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastKnownPosition, 0.3f);
        }
    }
}