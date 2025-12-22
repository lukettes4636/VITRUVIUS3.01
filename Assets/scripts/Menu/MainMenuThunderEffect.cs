using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuThunderEffect : MonoBehaviour
{
    
    
    
    [Header("Visual Settings")]
    [Tooltip("La luz que simulara el relampago.")]
    [SerializeField] private Light lightningLight;
    
    [Tooltip("Intensidad maxima del destello.")]
    [SerializeField] private float maxIntensity = 0.5f;
    
    [Tooltip("Color del relampago (generalmente blanco azulado).")]
    [SerializeField] private Color lightningColor = new Color(0.9f, 0.95f, 1f);
    
    [Tooltip("Colores secundarios para variacion del relampago.")]
    [SerializeField] private Color[] lightningColorVariations = new Color[]
    {
        new Color(0.8f, 0.9f, 1f),
        new Color(1f, 1f, 1f),
        new Color(0.7f, 0.85f, 1f)
    };

    [Header("Audio Settings")]
    [Tooltip("Fuente de audio para emitir el sonido del trueno.")]
    [SerializeField] private AudioSource thunderAudioSource;
    
    [Tooltip("Lista de sonidos de truenos para variar.")]
    [SerializeField] private List<AudioClip> thunderClips;
    
    [Tooltip("Volumen del trueno (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float thunderVolume = 1f;
    
    [Tooltip("Pitch del trueno para variacion adicional.")]
    [Range(0.5f, 2f)]
    [SerializeField] private float thunderPitch = 1f;

    [Header("Timing Settings")]
    [Tooltip("Tiempo minimo y maximo entre truenos (en segundos).")]
    [SerializeField] private Vector2 intervalRange = new Vector2(8f, 20f);
    
    [Tooltip("Duracion del efecto visual de parpadeo.")]
    [SerializeField] private float flashDuration = 0.3f;
    
    [Tooltip("Activar retraso realista entre relampago y trueno.")]
    [SerializeField] private bool useRealisticThunderDelay = false;
    
    [Tooltip("Retraso entre el relampago y el trueno (simula distancia realista).")]
    [SerializeField] private Vector2 thunderDelayRange = new Vector2(0.5f, 3f);

    [Header("Advanced Settings")]
    [Tooltip("Numero de destellos rapidos por relampago.")]
    [SerializeField] private int flashCount = 3;
    
    [Tooltip("Duracion de cada destello individual.")]
    [SerializeField] private float individualFlashDuration = 0.05f;

    private float originalIntensity;
    private Color originalColor;
    private bool isFlashing = false;

    private void Start()
    {
        if (lightningLight != null)
        {
            originalIntensity = lightningLight.intensity;
            originalColor = lightningLight.color;
            
            lightningLight.intensity = 0; 
        }
        
        
        if (thunderClips == null || thunderClips.Count == 0)
        {
            #if UNITY_EDITOR
            AutoAssignThunderClips();
            #endif
        }
        
        
        if (thunderAudioSource == null)
        {

            thunderAudioSource = GetComponent<AudioSource>();
            if (thunderAudioSource == null)
            {

                thunderAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        
        if (thunderAudioSource != null)
        {
            thunderAudioSource.spatialBlend = 0f;  
            thunderAudioSource.volume = 1f;      
            thunderAudioSource.priority = 0;     
            thunderAudioSource.playOnAwake = false;
        }

        StartCoroutine(ThunderLoop());
    }

    
#if UNITY_EDITOR
    [ContextMenu("Test Thunder Effect")]
    private void TestThunderEffect()
    {
        if (!isFlashing)
        {
            StartCoroutine(RealisticLightningFlash());
        }
    }
    
    [ContextMenu("Test Thunder Sound Only")]
    private void TestThunderSoundOnly()
    {

        PlayThunderSoundImmediate();
    }
    
    [ContextMenu("Auto Assign Thunder Clips")]
    private void AutoAssignThunderClips()
    {

        
        thunderClips = new List<AudioClip>();
        
        
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip thunder");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            
            if (clip != null)
            {
                thunderClips.Add(clip);

            }
        }
        
        
        string[] specificNames = { "loud-thunder", "thunder", "storm" };
        foreach (string name in specificNames)
        {
            string[] nameGuids = UnityEditor.AssetDatabase.FindAssets($"t:AudioClip {name}");
            foreach (string guid in nameGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                
                if (clip != null && !thunderClips.Contains(clip))
                {
                    thunderClips.Add(clip);

                }
            }
        }
        
        if (thunderClips.Count == 0)
        {

        }
    }
#endif

    private IEnumerator ThunderLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(waitTime);

            if (!isFlashing)
            {
                StartCoroutine(RealisticLightningFlash());
            }
        }
    }

    private IEnumerator RealisticLightningFlash()
    {
        if (lightningLight == null) yield break;

        isFlashing = true;

        

        
        
        Color selectedColor = lightningColorVariations[Random.Range(0, lightningColorVariations.Length)];
        lightningLight.color = selectedColor;
        
        
        if (useRealisticThunderDelay)
        {
            float thunderDelay = Random.Range(thunderDelayRange.x, thunderDelayRange.y);
            StartCoroutine(PlayThunderSoundDelayed(thunderDelay));
        }
        else
        {
            
            PlayThunderSoundImmediate();
        }
        
        
        for (int i = 0; i < flashCount; i++)
        {
            
            float currentIntensity = Mathf.Clamp(maxIntensity * Random.Range(0.8f, 1.0f), 0f, 0.5f);
            lightningLight.intensity = currentIntensity;
            
            yield return new WaitForSeconds(individualFlashDuration);
            
            
            lightningLight.intensity = currentIntensity * Random.Range(0.3f, 0.6f); 
            yield return new WaitForSeconds(individualFlashDuration * 0.5f);
            
            
            lightningLight.intensity = currentIntensity * Random.Range(0.5f, 0.8f); 
            yield return new WaitForSeconds(individualFlashDuration * 0.7f);
            
            
            if (i < flashCount - 1)
            {
                lightningLight.intensity = 0;
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }
        }
        
        
        float fadeTime = 0;
        float fadeDuration = flashDuration * 0.5f;
        float startFadeIntensity = lightningLight.intensity;
        
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float fadeRatio = fadeTime / fadeDuration;
            lightningLight.intensity = Mathf.Lerp(startFadeIntensity, 0, fadeRatio * fadeRatio); 
            yield return null;
        }
        
        
        lightningLight.intensity = 0;
        lightningLight.color = originalColor;
        isFlashing = false;
    }

    private void PlayThunderSoundImmediate()
    {
        

        
        if (thunderAudioSource != null && thunderClips != null && thunderClips.Count > 0)
        {
            AudioClip clip = thunderClips[Random.Range(0, thunderClips.Count)];
            
            
            if (clip == null)
            {

                return;
            }
            
            
            float originalVolume = thunderAudioSource.volume;
            float originalPitch = thunderAudioSource.pitch;
            
            
            float boostedVolume = 1f; 
            thunderAudioSource.volume = boostedVolume;
            thunderAudioSource.pitch = thunderPitch;
            
            
            thunderAudioSource.enabled = true;
            thunderAudioSource.playOnAwake = false;
            
            
            thunderAudioSource.spatialBlend = 0f;  
            thunderAudioSource.volume = 1f;      
            
            
            thunderAudioSource.priority = 0;     
            thunderAudioSource.panStereo = 0f;   
            thunderAudioSource.reverbZoneMix = 1f; 
            thunderAudioSource.dopplerLevel = 0f; 
            

            
            thunderAudioSource.PlayOneShot(clip);
            
            
            StartCoroutine(RestoreAudioSettings(originalVolume, originalPitch, clip.length));
        }
        else
        {

            
            
            if (thunderAudioSource == null)
            {

                thunderAudioSource = gameObject.AddComponent<AudioSource>();
                thunderAudioSource.volume = thunderVolume;
                thunderAudioSource.pitch = thunderPitch;
            }
        }
    }
    
    private IEnumerator RestoreAudioSettings(float originalVolume, float originalPitch, float clipDuration)
    {
        yield return new WaitForSeconds(clipDuration + 0.1f);
        if (thunderAudioSource != null)
        {
            thunderAudioSource.volume = originalVolume;
            thunderAudioSource.pitch = originalPitch;
        }
    }

    private IEnumerator PlayThunderSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayThunderSoundImmediate();
    }
}
