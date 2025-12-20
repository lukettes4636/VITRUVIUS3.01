using UnityEngine;
using System.Collections;

public class AmbientSoundManager : MonoBehaviour
{
    public static AmbientSoundManager Instance { get; private set; }

    [Header("Ambient Sound Configuration")]
    public AudioSource ambientSource;
    public AudioClip[] ambientClips;
    public float minDelayBetweenAmbient = 10f;
    public float maxDelayBetweenAmbient = 30f;

    [Header("Music Configuration")]
    public AudioSource musicSource;
    public AudioClip[] musicClips;
    public bool playMusic = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playMusic && musicClips.Length > 0)
        {
            PlayRandomMusic();
        }

        if (ambientClips.Length > 0)
        {
            StartCoroutine(AmbientSoundRoutine());
        }
    }

    private IEnumerator AmbientSoundRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelayBetweenAmbient, maxDelayBetweenAmbient));
            
            if (ambientClips.Length > 0 && ambientSource != null)
            {
                AudioClip clip = ambientClips[Random.Range(0, ambientClips.Length)];
                ambientSource.PlayOneShot(clip);
            }
        }
    }

    public void PlayRandomMusic()
    {
        if (musicClips.Length > 0 && musicSource != null)
        {
            AudioClip clip = musicClips[Random.Range(0, musicClips.Length)];
            AudioManager.Instance.PlayMusic(clip, 2f);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic());
        }
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / 2f;
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    public void PlayAmbientSound(AudioClip clip, float volume = 1f)
    {
        if (ambientSource != null && clip != null)
        {
            ambientSource.PlayOneShot(clip, volume);
        }
    }
}