using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip interactSound;
    public float volume = 1.0f;
    public float pitch = 1.0f;
    public bool useAudioConfig = true;
    
    [Header("Interaction Settings")]
    public bool canInteract = true;
    public string interactionMessage = "Interactuar";
    
    protected virtual void OnEnable()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.RegisterInteractable(this);
        }
    }
    
    protected virtual void OnDisable()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.UnregisterInteractable(this);
        }
    }
    
    public virtual void Interact()
    {
        if (!canInteract) return;
        
        
        PlayInteractionSound();
        
        
        OnInteract();
    }
    
    protected virtual void OnInteract()
    {
        

    }
    
    protected void PlayInteractionSound()
    {
        if (AudioManager.Instance != null)
        {
            if (useAudioConfig && AudioManager.Instance.GetAudioConfig() != null)
            {
                
                AudioClip[] interactionSounds = AudioManager.Instance.GetAudioConfig().interactionSounds;
                if (interactionSounds != null && interactionSounds.Length > 0)
                {
                    AudioClip clip = interactionSounds[Random.Range(0, interactionSounds.Length)];
                    AudioManager.Instance.PlaySFX(clip, transform.position, volume, pitch);
                    return;
                }
            }
            
            
            if (interactSound != null)
            {
                AudioManager.Instance.PlaySFX(interactSound, transform.position, volume, pitch);
            }
        }
    }
    
    protected void PlayDoorOpenSound()
    {
        PlaySoundFromCategory(AudioManager.Instance.GetAudioConfig()?.doorOpenSounds, "doorOpenSounds");
    }
    
    protected void PlayDoorCloseSound()
    {
        PlaySoundFromCategory(AudioManager.Instance.GetAudioConfig()?.doorCloseSounds, "doorCloseSounds");
    }
    
    protected void PlayDoorLockedSound()
    {
        PlaySoundFromCategory(AudioManager.Instance.GetAudioConfig()?.doorLockedSounds, "doorLockedSounds");
    }
    
    protected void PlayPuzzleSound()
    {
        PlaySoundFromCategory(AudioManager.Instance.GetAudioConfig()?.puzzleSounds, "puzzleSounds");
    }
    
    protected void PlayFlashlightSound()
    {
        PlaySoundFromCategory(AudioManager.Instance.GetAudioConfig()?.flashlightSounds, "flashlightSounds");
    }
    
    private void PlaySoundFromCategory(AudioClip[] clips, string categoryName)
    {
        if (AudioManager.Instance != null && clips != null && clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            AudioManager.Instance.PlaySFX(clip, transform.position, volume, pitch);
        }
        else if (AudioManager.Instance != null)
        {

        }
    }
    
    public string GetInteractionMessage()
    {
        return interactionMessage;
    }
    
    public bool CanInteract()
    {
        return canInteract;
    }
}
