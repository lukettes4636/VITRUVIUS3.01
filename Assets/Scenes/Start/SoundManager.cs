using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] sounds;
    public AudioClip[] music;

    [HideInInspector] public AudioSource[] sfxChannel;
    [HideInInspector] public AudioSource[] musicChannel;

    [Range(0,1)] public float volumeSFX;
    [Range(0,1)] public float volumeMusic;

    public static SoundManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        sfxChannel = new AudioSource[sounds.Length];
        for (int i = 0; i < sfxChannel.Length; i++)
        {
            sfxChannel[i] = gameObject.AddComponent<AudioSource>();
            sfxChannel[i].clip = sounds[i];
        }

        musicChannel = new AudioSource[music.Length];
        for (int i = 0; i < musicChannel.Length; i++)
        {
            musicChannel[i] = gameObject.AddComponent<AudioSource>();
            musicChannel[i].clip = music[i];
        }

        volumeSFX = PlayerPrefs.GetFloat("volumeSFX", 1f);
        volumeMusic = PlayerPrefs.GetFloat("volumeMusic", 1f);
    }

    #region SFX

    public bool isSoundPlaying(SoundID id)
    {
        return sfxChannel[(int)id].isPlaying;
    }

    public void PlaySound(SoundID id, bool loop = false, float pitch = 1)
    {
        sfxChannel[(int)id].Play();
        sfxChannel[(int)id].loop = loop;
        sfxChannel[(int)id].volume = volumeSFX;
        sfxChannel[(int)id].pitch = pitch;
        
    }

    public void StopAllSounds()
    {
        for (int i = 0; i < sfxChannel.Length; i++)
        {
            sfxChannel[i].Stop();
        }
    }

    public void PauseAllSounds()
    {
        for (int i = 0; i < sfxChannel.Length; i++)
        {
            sfxChannel[i].Pause();
        }
    }

    public void ResumeAllSounds()
    {
        for (int i = 0; i < sfxChannel.Length; i++)
        {
            sfxChannel[i].UnPause();
        }
    }

    public void StopSound(SoundID id)
    {
        sfxChannel[(int)id].Stop();
    }

    public void PauseSound(SoundID id)
    {
        sfxChannel[(int)id].Pause();
    }

    public void ResumeSound(SoundID id)
    {
        sfxChannel[(int)id].UnPause();
    }

    public void ToggleMuteSound(SoundID id)
    {
        sfxChannel[(int)id].mute = !sfxChannel[(int)id].mute;
    }

    public void ChangeVolumeSound(float volume)
    {
        volumeSFX = volume;
        for (int i = 0; i < sfxChannel.Length; i++)
        {
            sfxChannel[i].volume = volumeSFX;
        }
        
        PlayerPrefs.SetFloat("volumeSFX", volumeSFX);
    }

    #endregion

    #region MUSIC

    public bool isMusicPlaying(MusicID id)
    {
        return musicChannel[(int)id].isPlaying;
    }

    public void PlayMusic(MusicID id, bool loop = false, float pitch = 1)
    {
        musicChannel[(int)id].Play();
        musicChannel[(int)id].loop = loop;
        musicChannel[(int)id].volume = volumeMusic;
        musicChannel[(int)id].pitch = pitch;
    }

    public void StopAllMusic()
    {
        for (int i = 0; i < musicChannel.Length; i++)
        {
            musicChannel[i].Stop();
        }
    }

    public void PauseAllMusic()
    {
        for (int i = 0; i < musicChannel.Length; i++)
        {
            musicChannel[i].Pause();
        }
    }

    public void ResumeAllMusic()
    {
        for (int i = 0; i < musicChannel.Length; i++)
        {
            musicChannel[i].UnPause();
        }
    }

    public void StopMusic(MusicID id)
    {
        musicChannel[(int)id].Stop();
    }

    public void PauseMusic(MusicID id)
    {
        musicChannel[(int)id].Pause();
    }

    public void ResumeMusic(MusicID id)
    {
        musicChannel[(int)id].UnPause();
    }

    public void ToggleMuteMusic(MusicID id)
    {
        musicChannel[(int)id].mute = !musicChannel[(int)id].mute;
    }

    public void ChangeVolumeMusic(float volume)
    {
        volumeMusic = volume;
        for (int i = 0; i < musicChannel.Length; i++)
        {
            musicChannel[i].volume = volumeMusic;
        }
        
        PlayerPrefs.SetFloat("volumeMusic", volumeMusic);
    }

    #endregion
}

public enum SoundID
{
    MSDos,
    BuySucess,
    BuyFailure,
    ButtonSound,
    MoneyCollect,
    DamageSnd,
    DodgeSnd,
    WeaponShot,
    WeaponReload,
    WeaponCycle
}

public enum MusicID
{
    Music1,
    Music2,
    Music3
}