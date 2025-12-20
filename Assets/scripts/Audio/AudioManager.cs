using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum FootstepType
    {
        Player1,
        Player2
    }

    [Header("Audio Configuration")]
    [SerializeField]
    public AudioMixer masterMixer;
    public AudioConfig audioConfig;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int sfxPoolSize = 10; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (masterMixer == null)
        {

        }

        InitializeSFXPool();
        LoadVolumeSettings();
    }

    private void Start()
    {
        InitializeSFXPool();
        LoadVolumeSettings();
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SFX_AudioSource_" + i);
            obj.transform.SetParent(this.transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            if (masterMixer != null && audioConfig != null && audioConfig.sfxMixerGroup != null)
            {
                source.outputAudioMixerGroup = audioConfig.sfxMixerGroup;
            }
            else
            {

            }
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        
        GameObject obj = new GameObject("SFX_AudioSource_" + sfxPool.Count);
        obj.transform.SetParent(this.transform);
        AudioSource newSource = obj.AddComponent<AudioSource>();
        if (masterMixer != null && audioConfig != null && audioConfig.sfxMixerGroup != null)
        {
            newSource.outputAudioMixerGroup = audioConfig.sfxMixerGroup;
        }
        else
        {

        }
        newSource.playOnAwake = false;
        sfxPool.Add(newSource);
        return newSource;
    }

    
    
    
    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        if (musicSource.isPlaying && musicSource.clip == clip) return;

        StartCoroutine(FadeTrack(musicSource, clip, fadeDuration));
    }



    public void PlaySFX(AudioClip clip, Vector3 position, float spatialBlend = 1f, float volume = 1f)
    {
        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.transform.position = position;
        source.spatialBlend = spatialBlend;
        source.volume = volume;
        source.loop = false;
        source.Play();
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float spatialBlend = 1f, float volume = 1f, float pitch = 1f)
    {
        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.transform.position = position;
        source.spatialBlend = spatialBlend;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = false;
        source.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }



    public void PlayFootstep(FootstepType footstepType, Vector3 position = default, float volume = 1f)
    {
        AudioClip[] footstepClips = null;
        
        switch (footstepType)
        {
            case FootstepType.Player1:
                footstepClips = audioConfig.player1Footsteps;
                break;
            case FootstepType.Player2:
                footstepClips = audioConfig.player2Footsteps;
                break;
        }
        
        if (footstepClips != null && footstepClips.Length > 0)
        {
            AudioClip clipToPlay = footstepClips[Random.Range(0, footstepClips.Length)];
            if (position == default)
            {
                PlaySFX(clipToPlay, volume);
            }
            else
            {
                PlaySFX(clipToPlay, position, 1f, volume);
            }
        }
    }

    private IEnumerator FadeTrack(AudioSource source, AudioClip newClip, float duration)
    {
        float startVolume = source.volume;

        if (source.isPlaying)
        {
            while (source.volume > 0)
            {
                source.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }
            source.Stop();
        }

        source.clip = newClip;
        source.Play();

        while (source.volume < startVolume)
        {
            source.volume += startVolume * Time.deltaTime / duration;
            yield return null;
        }
        source.volume = startVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration, System.Action onComplete = null)
    {
        float startVolume = source.volume;
        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        source.Stop();
        source.volume = startVolume; 
        onComplete?.Invoke();
    }

    public void SetMasterVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("MasterVolume", dB);
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("MusicVolume", dB);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("SFXVolume", dB);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }
    }

    public void SetAmbientVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("AmbientVolume", dB);
            PlayerPrefs.SetFloat("AmbientVolume", volume);
        }
    }

    public void SetVoiceVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("VoiceVolume", dB);
            PlayerPrefs.SetFloat("VoiceVolume", volume);
        }
    }

    public AudioConfig GetAudioConfig()
    {
        return audioConfig;
    }
    private void LoadVolumeSettings()
    {
        if (masterMixer != null)
        {
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
            SetAmbientVolume(PlayerPrefs.GetFloat("AmbientVolume", 1f));
            SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume", 1f));
        }
    }
}
