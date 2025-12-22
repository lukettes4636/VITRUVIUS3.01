using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class ParticleSoundController : MonoBehaviour
{
    [Header("Sound Configuration")]
    [SerializeField] private AudioClip particleSoundClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopSound = true;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    [Header("Hearing Range")]
    [SerializeField] private float maxHearingDistance = 12f;
    [SerializeField] private float minHearingDistance = 2f;
    [Tooltip("Si esta activado, el sonido se escuchara globalmente sin importar la distancia")]
    [SerializeField] private bool globalSound = false;
    
    private ParticleSystem _particleSystem;
    private AudioSource audioSource;
    private bool wasPlaying = false;
    private Transform playerTransform;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        
        audioSource.playOnAwake = false;
        audioSource.loop = loopSound;
        audioSource.volume = volume;
        
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Start()
    {
        if (playOnStart && particleSoundClip != null)
        {
            audioSource.clip = particleSoundClip;
            if (_particleSystem.isPlaying)
                audioSource.Play();
        }
    }

    void Update()
    {
        if (_particleSystem == null || audioSource == null) return;

        bool isPlaying = _particleSystem.isPlaying;
        
        if (isPlaying && !wasPlaying && particleSoundClip != null)
        {
            
            audioSource.clip = particleSoundClip;
            if (ShouldPlaySound())
                audioSource.Play();
        }
        else if (!isPlaying && wasPlaying)
        {
            
            if (!loopSound)
                audioSource.Stop();
        }
        
        
        if (audioSource.isPlaying && !globalSound && playerTransform != null)
        {
            UpdateSoundVolumeBasedOnDistance();
        }
        
        wasPlaying = isPlaying;
    }

    
    public void SetSoundClip(AudioClip newClip, bool playImmediately = false)
    {
        particleSoundClip = newClip;
        audioSource.clip = newClip;
        if (playImmediately && _particleSystem.isPlaying && ShouldPlaySound())
            audioSource.Play();
    }

    
    private bool ShouldPlaySound()
    {
        if (globalSound) return true;
        if (playerTransform == null) return false;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= maxHearingDistance;
    }

    
    private void UpdateSoundVolumeBasedOnDistance()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance > maxHearingDistance)
        {
            
            audioSource.Stop();
            return;
        }
        
        if (distance <= minHearingDistance)
        {
            
            audioSource.volume = volume;
        }
        else
        {
            
            float normalizedDistance = (distance - minHearingDistance) / (maxHearingDistance - minHearingDistance);
            audioSource.volume = volume * (1f - normalizedDistance);
        }
    }

    
    public void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }
}