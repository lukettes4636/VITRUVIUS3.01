using UnityEngine;

public static class SoundHelper
{
    public static void PlayRandomSound(AudioClip[] clips, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && clips != null && clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            AudioManager.Instance.PlaySFX(clip, position, volume, pitch);
        }
    }
    
    public static void PlayInteractionSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().interactionSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayDoorOpenSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().doorOpenSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayDoorCloseSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().doorCloseSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayDoorLockedSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().doorLockedSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayDoorUnlockSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().doorUnlockSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayPuzzleSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().puzzleSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayFlashlightSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().flashlightSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
    
    public static void PlayPickupSound(Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig() != null)
        {
            AudioClip[] sounds = AudioManager.Instance.GetAudioConfig().pickUpSounds;
            PlayRandomSound(sounds, position, volume, pitch);
        }
    }
}