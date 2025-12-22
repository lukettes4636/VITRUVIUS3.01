using UnityEngine;
using System.Collections.Generic;

public class EnemySenses : MonoBehaviour
{
    [Header("Deteccion de Jugadores")]
    public Transform[] playerTargets;

    [Header("Deteccion de NPCs")]
    public Transform[] npcTargets;

    [Header("Deteccion de Objetos")]
    public ObjectNoiseDetection objectNoiseDetection;

    [Header("Configuracion de Audio")]
    [Range(0.5f, 3.0f)] public float audioSensitivity = 1.0f;
    [Tooltip("Rango maximo de deteccion por sonido")]
    public float maxHearingDistance = 20f;
    public float minDetectionRadius = 1.0f;
    [Range(0.0f, 1.0f)] public float detectionThreshold = 0.15f;

    [Header("Deteccion de Proximidad")]
    [Tooltip("NOTA: El enemigo es CIEGO, solo detecta por SONIDO. Esta opcion esta deshabilitada.")]
    public float proximityDetectionRadius = 3.5f;
    [Range(1.0f, 3.0f)] public float closeRangeBoost = 2.0f;

    [Header("Obstaculos de Audio")]
    public LayerMask soundBlockerLayer;
    [Range(0.1f, 0.9f)] public float soundAttenuationPerWall = 0.7f;

    [Header("Memoria (Persistencia)")]
    public float memoryDuration = 3.0f;
    private float timeSinceLastHeard = 0f;

    [Header("Deteccion de Paredes")]
    public LayerMask destructibleWallLayer;
    public float wallDetectionDistance = 5.0f;
    public float wallDetectionRadius = 1.0f;
    public float frontWallCheckDistance = 2.5f;

    public Vector3 TargetPositionOfInterest { get; set; }
    public bool HasTargetOfInterest { get; set; }
    public Transform CurrentPlayer { get; set; }
    public Transform CurrentNPCTarget { get; set; }
    public Transform CurrentNoisyObject { get; private set; }
    public GameObject CurrentWallTarget { get; set; }
    public float CurrentAlertLevel { get; private set; }

    public Transform CurrentTarget => CurrentPlayer ?? CurrentNPCTarget ?? CurrentNoisyObject;

    private Dictionary<Transform, float> ignoredObjectsUntil = new Dictionary<Transform, float>();

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
        playerCache.Clear();
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

        npcCache.Clear();
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

    public void Tick()
    {
        PruneIgnoredObjects();
        ProcessAudioDetection();
        ProcessObjectNoiseDetection();
    }

    private void ProcessAudioDetection()
    {
        Transform bestTarget = null;
        float minDistance = float.MaxValue;
        float maxAudioStrength = 0f;
        bool isTargetNPC = false;

        void CheckTarget(Transform target, float noiseRadius, float idleNoiseRadius, bool isNPC)
        {
            if (target == null) return;

            float dist = Vector3.Distance(transform.position, target.position);
            
            bool isEmittingActiveNoise = noiseRadius > idleNoiseRadius + 0.1f;
            
            
            if (dist <= minDetectionRadius)
            {
                float strengthClose = 1f;
                if (strengthClose > maxAudioStrength) maxAudioStrength = strengthClose;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestTarget = target;
                    isTargetNPC = isNPC;
                }
                return;
            }
            
            bool shouldEvaluate = isEmittingActiveNoise;
            if (!shouldEvaluate) return;
            
            float strength = CalculateAudioStrength(target, noiseRadius, dist);
            
            bool shouldDetect = strength > detectionThreshold;

            if (shouldDetect)
            {
                if (strength > maxAudioStrength) maxAudioStrength = strength;

                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestTarget = target;
                    isTargetNPC = isNPC;
                }
            }
        }

        foreach (var cache in playerCache)
        {
            if (cache.transform == null) continue;
            if (cache.playerHealth != null && cache.playerHealth.IsDead) continue;

            if (cache.playerNoise != null)
            {
                float effectiveRadius = Mathf.Max(cache.playerNoise.currentNoiseRadius, minDetectionRadius);
                float idleRadius = cache.playerNoise.idleNoiseRadius;
                CheckTarget(cache.transform, effectiveRadius, idleRadius, false);
            }
            else
            {
                CheckTarget(cache.transform, minDetectionRadius, 0f, false);
            }
        }

        foreach (var cache in npcCache)
        {
            if (cache.transform == null) continue;
            if (cache.npcHealth != null && cache.npcHealth.IsDead) continue;

            if (cache.npcNoise != null)
            {
                float effectiveRadius = Mathf.Max(cache.npcNoise.currentNoiseRadius, minDetectionRadius);
                float idleRadius = cache.npcNoise.idleNoiseRadius;
                CheckTarget(cache.transform, effectiveRadius, idleRadius, true);
            }
            else
            {
                CheckTarget(cache.transform, minDetectionRadius, 0f, true);
            }
        }

        CurrentAlertLevel = maxAudioStrength;

        if (bestTarget != null)
        {
            bool stillMakingNoise = false;
            
            
            TargetCache bestCache = default;
            bool foundInCache = false;
            
            foreach(var c in playerCache) if(c.transform == bestTarget) { bestCache = c; foundInCache = true; break; }
            if(!foundInCache) foreach(var c in npcCache) if(c.transform == bestTarget) { bestCache = c; foundInCache = true; break; }

            if (foundInCache)
            {
                if (bestCache.playerNoise != null)
                {
                    stillMakingNoise = bestCache.playerNoise.currentNoiseRadius > bestCache.playerNoise.idleNoiseRadius + 0.1f;
                }
                else if (bestCache.npcNoise != null)
                {
                    stillMakingNoise = bestCache.npcNoise.currentNoiseRadius > bestCache.npcNoise.idleNoiseRadius + 0.1f;
                }
            }
            
            
            bool isUltraClose = minDistance <= Mathf.Max(minDetectionRadius, 0.5f);
            if (stillMakingNoise || isUltraClose)
            {
                if (isTargetNPC)
                {
                    CurrentNPCTarget = bestTarget;
                    CurrentPlayer = null;
                }
                else
                {
                    CurrentPlayer = bestTarget;
                    CurrentNPCTarget = null;
                }
                TargetPositionOfInterest = bestTarget.position;
                HasTargetOfInterest = true;
                timeSinceLastHeard = 0f;
            }
            else
            {
                if (CurrentPlayer == bestTarget) CurrentPlayer = null;
                if (CurrentNPCTarget == bestTarget) CurrentNPCTarget = null;
                
                if (CurrentPlayer == null && CurrentNPCTarget == null)
                {
                    HasTargetOfInterest = false;
                }
                
                timeSinceLastHeard += Time.deltaTime;
                if (timeSinceLastHeard > memoryDuration)
                {
                    HasTargetOfInterest = false;
                    CurrentPlayer = null;
                    CurrentNPCTarget = null;
                }
            }
        }
        else
        {
            timeSinceLastHeard += Time.deltaTime;
            if (timeSinceLastHeard > memoryDuration)
            {
                HasTargetOfInterest = false;
                CurrentPlayer = null;
                CurrentNPCTarget = null;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxHearingDistance);
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, minDetectionRadius);
    }
    private float CalculateAudioStrength(Transform target, float noiseRadius, float distance)
    {
        if (distance > maxHearingDistance) return 0f;

        float sensitivity = audioSensitivity;
        if (distance <= proximityDetectionRadius)
        {
            float proximityFactor = 1f - (distance / proximityDetectionRadius);
            sensitivity *= (1f + (closeRangeBoost - 1f) * Mathf.Pow(proximityFactor, 0.5f));
        }

        float rawRadius = noiseRadius * sensitivity;
        int walls = CountSoundBlockers(target);
        float attenuatedRadius = rawRadius * Mathf.Pow(soundAttenuationPerWall, walls);
        float effectiveRadius = Mathf.Max(attenuatedRadius, minDetectionRadius);

        if (distance <= effectiveRadius)
        {
            float baseStrength = 1f - (distance / effectiveRadius);

            if (distance < 2.0f)
            {
                float closeFactor = 1f - (distance / 2.0f);
                baseStrength = Mathf.Lerp(baseStrength, 1.0f, closeFactor * 0.5f);
            }

            return Mathf.Clamp01(baseStrength);
        }

        return 0f;
    }

    private int CountSoundBlockers(Transform target)
    {
        Vector3 start = transform.position + Vector3.up;
        Vector3 end = target.position + Vector3.up;
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        return Physics.RaycastAll(start, dir, dist, soundBlockerLayer).Length;
    }

    public bool CheckForWallInFront()
    {
        Vector3 origin = transform.position + Vector3.up;

        Vector3[] checkOffsets = new Vector3[]
        {
            transform.forward * 1.0f,
            (transform.forward + transform.right * 0.3f).normalized * 1.0f,
            (transform.forward - transform.right * 0.3f).normalized * 1.0f
        };

        foreach (Vector3 offset in checkOffsets)
        {
            Vector3 checkPos = origin + offset;
            Collider[] hits = Physics.OverlapSphere(checkPos, 0.8f, destructibleWallLayer);

            foreach (var col in hits)
            {
                if (col.gameObject != gameObject && col.gameObject.activeInHierarchy)
                {
                    CurrentWallTarget = col.gameObject;
                    return true;
                }
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(origin, transform.forward, out hit, frontWallCheckDistance, destructibleWallLayer))
        {
            if (hit.collider.gameObject != gameObject && hit.collider.gameObject.activeInHierarchy)
            {
                CurrentWallTarget = hit.collider.gameObject;
                return true;
            }
        }

        return false;
    }

    public bool CheckWallInPathToTarget()
    {
        if (!HasTargetOfInterest) return false;

        Vector3 start = transform.position + Vector3.up * 1.2f;
        Vector3 end = TargetPositionOfInterest + Vector3.up;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        float checkDist = Mathf.Min(distance, wallDetectionDistance);

        RaycastHit[] hits = Physics.SphereCastAll(start, wallDetectionRadius, direction, checkDist, destructibleWallLayer);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != gameObject && hit.collider.gameObject.activeInHierarchy)
            {
                CurrentWallTarget = hit.collider.gameObject;
                return true;
            }
        }

        RaycastHit simpleHit;
        if (Physics.Raycast(start, direction, out simpleHit, checkDist, destructibleWallLayer))
        {
            if (simpleHit.collider.gameObject != gameObject && simpleHit.collider.gameObject.activeInHierarchy)
            {
                CurrentWallTarget = simpleHit.collider.gameObject;
                return true;
            }
        }

        CurrentWallTarget = null;
        return false;
    }

    private void ProcessObjectNoiseDetection()
    {
        
        if (HasTargetOfInterest && (CurrentPlayer != null || CurrentNPCTarget != null))
        {
            CurrentNoisyObject = null;
            return;
        }

        
        if (objectNoiseDetection != null && objectNoiseDetection.HasNoisyObjectNearby())
        {
            if (objectNoiseDetection.GetLoudestObject(out Transform noisyObject, out Vector3 objectPosition))
            {
                if (noisyObject != null && !IsObjectIgnored(noisyObject))
                {
                    CurrentNoisyObject = noisyObject;
                    TargetPositionOfInterest = objectPosition;
                    HasTargetOfInterest = true;
                    timeSinceLastHeard = 0f;
                    return;
                }
            }
        }

        
        if (CurrentNoisyObject != null)
        {
            
            bool stillLoudest = false;
            if (objectNoiseDetection != null && objectNoiseDetection.HasNoisyObjectNearby())
            {
                if (objectNoiseDetection.GetLoudestObject(out Transform loudest, out Vector3 pos))
                {
                    if (loudest == CurrentNoisyObject)
                    {
                        stillLoudest = true;
                        TargetPositionOfInterest = pos; 
                    }
                }
            }
            
            if (!stillLoudest)
            {
                
                CurrentNoisyObject = null;
                if (CurrentPlayer == null && CurrentNPCTarget == null)
                {
                    HasTargetOfInterest = false;
                }
            }
        }
        else if (CurrentPlayer == null && CurrentNPCTarget == null && !HasTargetOfInterest)
        {
            CurrentNoisyObject = null;
        }
    }

    public void ForgetTarget()
    {
        HasTargetOfInterest = false;
        CurrentPlayer = null;
        CurrentNPCTarget = null;
        CurrentNoisyObject = null;
        CurrentWallTarget = null;
    }

    public void SetPlayerTarget(Transform player)
    {
        CurrentPlayer = player;
        CurrentNPCTarget = null;
        if (player != null)
        {
            TargetPositionOfInterest = player.position;
            HasTargetOfInterest = true;
        }
    }

    public void SetNPCTarget(Transform npc)
    {
        CurrentNPCTarget = npc;
        CurrentPlayer = null;
        if (npc != null)
        {
            TargetPositionOfInterest = npc.position;
            HasTargetOfInterest = true;
        }
    }

    public void IgnoreCurrentNoisyObjectFor(float seconds)
    {
        if (CurrentNoisyObject != null)
        {
            ignoredObjectsUntil[CurrentNoisyObject] = Time.time + seconds;
            CurrentNoisyObject = null;
            HasTargetOfInterest = false;
        }
    }

    private bool IsObjectIgnored(Transform obj)
    {
        if (obj == null) return false;
        if (ignoredObjectsUntil.TryGetValue(obj, out float until))
        {
            return Time.time < until;
        }
        return false;
    }

    private void PruneIgnoredObjects()
    {
        if (ignoredObjectsUntil.Count == 0) return;
        var keys = new List<Transform>(ignoredObjectsUntil.Keys);
        foreach (var k in keys)
        {
            if (ignoredObjectsUntil[k] <= Time.time)
                ignoredObjectsUntil.Remove(k);
        }
    }
}
