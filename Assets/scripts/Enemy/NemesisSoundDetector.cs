using UnityEngine;
using System.Collections.Generic;

public class NemesisSoundDetector : MonoBehaviour
{
    [Header("Sound Detection Settings")]
    public float maxHearingDistance = 35f;
    public float soundAttenuationPerWall = 0.7f;
    public LayerMask soundBlockerLayer;
    
    [Header("Target Tags")]
    public string player1Tag = "Player1";
    public string player2Tag = "Player2";
    public string npcTag = "NPC";
    
    
    private Dictionary<Transform, SoundSource> detectedSounds;
    
    private class SoundSource
    {
        public Transform source;
        public float intensity;
        public Vector3 position;
        public SoundType type;
        public float timestamp;
    }
    
    public enum SoundType { PlayerMovement, NPCMovement, PlayerAttack, NPCAttack, ObjectNoise }
    
    void Awake()
    {
        detectedSounds = new Dictionary<Transform, SoundSource>();
    }
    
    void Update()
    {
        DetectCharacterSounds();
        CleanUpOldSounds();
    }
    
    void DetectCharacterSounds()
    {
        detectedSounds.Clear();
        
        
        GameObject player1 = GameObject.FindGameObjectWithTag(player1Tag);
        if (player1 != null)
        {
            DetectPlayerSounds(player1.transform, SoundType.PlayerMovement);
        }
        
        
        GameObject player2 = GameObject.FindGameObjectWithTag(player2Tag);
        if (player2 != null)
        {
            DetectPlayerSounds(player2.transform, SoundType.PlayerMovement);
        }
        
        
        GameObject npc = GameObject.FindGameObjectWithTag(npcTag);
        if (npc != null)
        {
            DetectNPCSounds(npc.transform);
        }
    }
    
    void DetectPlayerSounds(Transform player, SoundType soundType)
    {
        if (player == null) return;
        
        
        bool isMoving = IsPlayerMoving(player);
        if (!isMoving) return;
        
        
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > maxHearingDistance) return;
        
        
        int wallCount = CountWallsBetween(transform.position, player.position);
        float soundIntensity = CalculateSoundIntensity(distance, wallCount);
        
        if (soundIntensity > 0.1f)
        {
            var soundSource = new SoundSource
            {
                source = player,
                intensity = soundIntensity,
                position = player.position,
                type = soundType,
                timestamp = Time.time
            };
            
            detectedSounds[player] = soundSource;
        }
    }
    
    void DetectNPCSounds(Transform npc)
    {
        if (npc == null) return;
        
        
        var noiseEmitter = npc.GetComponent<NPCNoiseEmitter>();
        if (noiseEmitter != null && noiseEmitter.currentNoiseRadius > 0.1f)
        {
            float distance = Vector3.Distance(transform.position, npc.position);
            if (distance <= maxHearingDistance)
            {
                int wallCount = CountWallsBetween(transform.position, npc.position);
                float soundIntensity = CalculateSoundIntensity(distance, wallCount);
                
                if (soundIntensity > 0.1f)
                {
                    var soundSource = new SoundSource
                    {
                        source = npc,
                        intensity = soundIntensity,
                        position = npc.position,
                        type = SoundType.NPCMovement,
                        timestamp = Time.time
                    };
                    
                    detectedSounds[npc] = soundSource;
                }
            }
        }
    }
    
    int CountWallsBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        
        RaycastHit[] hits = Physics.RaycastAll(start + Vector3.up, direction, distance, soundBlockerLayer);
        return hits.Length;
    }
    
    float CalculateSoundIntensity(float distance, int wallCount)
    {
        if (distance > maxHearingDistance) return 0f;
        
        
        float baseIntensity = 1f - (distance / maxHearingDistance);
        
        
        float wallAttenuation = Mathf.Pow(soundAttenuationPerWall, wallCount);
        float finalIntensity = baseIntensity * wallAttenuation;
        
        return Mathf.Clamp01(finalIntensity);
    }
    
    void CleanUpOldSounds()
    {
        
        List<Transform> keysToRemove = new List<Transform>();
        foreach (var kvp in detectedSounds)
        {
            if (Time.time - kvp.Value.timestamp > 3f)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            detectedSounds.Remove(key);
        }
    }
    
    
    public bool HasDetectedSounds()
    {
        return detectedSounds.Count > 0;
    }
    
    public Transform GetLoudestSoundSource(out float intensity, out Vector3 position)
    {
        intensity = 0f;
        position = Vector3.zero;
        
        if (detectedSounds.Count == 0) return null;
        
        Transform loudestSource = null;
        float maxIntensity = 0f;
        Vector3 sourcePosition = Vector3.zero;
        
        foreach (var soundSource in detectedSounds.Values)
        {
            if (soundSource.intensity > maxIntensity)
            {
                maxIntensity = soundSource.intensity;
                loudestSource = soundSource.source;
                sourcePosition = soundSource.position;
            }
        }
        
        intensity = maxIntensity;
        position = sourcePosition;
        return loudestSource;
    }
    
    public Transform GetNearestSoundSource(out float intensity, out Vector3 position)
    {
        intensity = 0f;
        position = Vector3.zero;
        
        if (detectedSounds.Count == 0) return null;
        
        Transform nearestSource = null;
        float minDistance = Mathf.Infinity;
        float sourceIntensity = 0f;
        Vector3 sourcePosition = Vector3.zero;
        
        foreach (var soundSource in detectedSounds.Values)
        {
            float distance = Vector3.Distance(transform.position, soundSource.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestSource = soundSource.source;
                sourceIntensity = soundSource.intensity;
                sourcePosition = soundSource.position;
            }
        }
        
        intensity = sourceIntensity;
        position = sourcePosition;
        return nearestSource;
    }
    
    public List<Transform> GetAllDetectedSources()
    {
        return new List<Transform>(detectedSounds.Keys);
    }
    
    bool IsPlayerMoving(Transform player)
    {
        if (player == null) return false;
        
        
        Vector3 currentPosition = player.position;
        
        
        if (previousPositions.ContainsKey(player))
        {
            float distanceMoved = Vector3.Distance(currentPosition, previousPositions[player]);
            previousPositions[player] = currentPosition;
            
            
            return distanceMoved > 0.01f;
        }
        else
        {
            
            previousPositions[player] = currentPosition;
            return false; 
        }
    }
    
    
    private Dictionary<Transform, Vector3> previousPositions = new Dictionary<Transform, Vector3>();
}