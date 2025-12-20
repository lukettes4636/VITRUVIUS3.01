using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    [Header("Music Configuration")]
    public AudioClip musicClip;
    public bool playOnStart = true;
    public float fadeDuration = 2f;
    public bool loop = true;

    void Start()
    {
        if (playOnStart && musicClip != null)
        {
            PlayBackgroundMusic();
        }
    }

    public void PlayBackgroundMusic()
    {
        if (musicClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicClip, fadeDuration);
        }
    }

    public void StopBackgroundMusic()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
        {
            AudioManager.Instance.musicSource.Stop();
        }
    }

    public void ChangeMusic(AudioClip newClip, float newFadeDuration = 2f)
    {
        if (newClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(newClip, newFadeDuration);
        }
    }
}