using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Audio/Proximity Audio Zone (Co-op Curve)")]
public class ProximityAudioZone_Coop_Advanced : MonoBehaviour
{
    [Header("🎮 Player Settings")]
    [Tooltip("Primer jugador (Transform).")]
    public Transform player1;

    [Tooltip("Segundo jugador (Transform).")]
    public Transform player2;

    [Header("🔊 Audio Settings")]
    [Tooltip("Distancia maxima a la que se puede escuchar el audio")]
    public float maxDistance = 20f;

    [Tooltip("Distancia donde el sonido alcanza su volumen maximo.")]
    public float minDistance = 1f;

    [Tooltip("Volumen maximo del sonido.")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Tooltip("Velocidad de cambio de volumen (fade in/out).")]

    public float volumeChangeSpeed = 2f;

    [Tooltip("Spatial Blend (0 = 2D, 1 = 3D).")]
    [Range(0f, 1f)]
    public float spatialBlend = 0f;

    [Header("📈 Volumen por distancia (editable)")]
    [Tooltip("Curve that defines how volume changes with distance (0 = near, 1 = far).")]
    public AnimationCurve distanceVolumeCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, -0.5f),     
        new Keyframe(0.3f, 0.8f, -1.2f, -1.2f), 
        new Keyframe(0.7f, 0.3f, -0.8f, -0.8f), 
        new Keyframe(1f, 0f, -0.2f, 0f)         
    );

    [Header("🧱 Occlusion Settings")]
    [Tooltip("Layers that can block the audio")]
    public LayerMask occlusionLayers;

    [Tooltip("Volume factor when there is an obstruction.")]
    [Range(0f, 1f)] public float occlusionVolumeMultiplier = 0.25f;

    [Tooltip("Transition speed when applying or removing occlusion.")]
    [Range(0.1f, 10f)] public float occlusionFadeSpeed = 3f;

    [Header("🎚 Low-Pass Filter (Opcional)")]
    [Tooltip("Enable muffled sound filter when there is occlusion.")]
    public bool useLowPassFilter = true;

    [Tooltip("Normal frequency (without occlusion). Typical values: 22000 Hz.")]
    public float normalCutoffFrequency = 22000f;

    [Tooltip("Frequency when sound is blocked. Typical values: 800-2000 Hz.")]
    public float occludedCutoffFrequency = 800f;

    [Tooltip("Filter transition speed (Hz per second).")]
    public float filterTransitionSpeed = 1500f;

    [Header("🚪 Activation Settings")]
    [Tooltip("If enabled, sound doesn't start until a player enters range.")]
    public bool requireEntryToStart = true;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;
    private bool hasActivated = false;
    private float currentOcclusionP1 = 1f;
    private float currentOcclusionP2 = 1f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend; 
        audioSource.volume = 0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.maxDistance = maxDistance;
        audioSource.minDistance = minDistance;

        if (!requireEntryToStart)
        {
            audioSource.Play();
            hasActivated = true;
        }

        if (useLowPassFilter)
        {
            lowPassFilter = GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
                lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = normalCutoffFrequency;
        }
    }

    void Update()
    {
        if (player1 == null && player2 == null) return;

        
        float finalVol1 = 0f;
        if (player1 != null)
        {
            float dist1 = Vector3.Distance(player1.position, transform.position);
            UpdatePlayerOcclusion(player1, dist1, ref currentOcclusionP1);
            if (dist1 <= maxDistance)
            {
                float baseVol1 = CalculateBaseVolume(dist1);
                finalVol1 = baseVol1 * currentOcclusionP1;
            }
        }

        
        float finalVol2 = 0f;
        if (player2 != null)
        {
            float dist2 = Vector3.Distance(player2.position, transform.position);
            UpdatePlayerOcclusion(player2, dist2, ref currentOcclusionP2);
            if (dist2 <= maxDistance)
            {
                float baseVol2 = CalculateBaseVolume(dist2);
                finalVol2 = baseVol2 * currentOcclusionP2;
            }
        }

        
        float targetVolume = Mathf.Max(finalVol1, finalVol2);

        
        
        float dominantOcclusion = (finalVol1 >= finalVol2) ? currentOcclusionP1 : currentOcclusionP2;

        
        if (requireEntryToStart && !hasActivated && targetVolume > 0.01f)
        {
            audioSource.Play();
            hasActivated = true;
        }

        
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeChangeSpeed);

        if (useLowPassFilter && lowPassFilter != null)
        {
            float targetCutoff = Mathf.Lerp(normalCutoffFrequency, occludedCutoffFrequency, 1f - dominantOcclusion);
            lowPassFilter.cutoffFrequency = Mathf.MoveTowards(
                lowPassFilter.cutoffFrequency,
                targetCutoff,
                Time.deltaTime * filterTransitionSpeed
            );
        }

        
        if (requireEntryToStart && hasActivated && targetVolume <= 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
            hasActivated = false;
        }
    }

    private void UpdatePlayerOcclusion(Transform player, float distance, ref float currentOcclusion)
    {
        float target = 1f;
        float checkDist = Mathf.Min(distance, maxDistance);

        Vector3 direction = (player.position - transform.position).normalized;
        
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, checkDist, occlusionLayers))
        {
            if (hit.transform != player)
                target = occlusionVolumeMultiplier;
        }

        currentOcclusion = Mathf.Lerp(currentOcclusion, target, Time.deltaTime * occlusionFadeSpeed);
    }

    private float CalculateBaseVolume(float distance)
    {
        float normalizedDist = Mathf.Clamp01(distance / maxDistance);
        return maxVolume * distanceVolumeCurve.Evaluate(normalizedDist);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
#endif
}