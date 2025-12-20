using UnityEngine;

public class PlaneInteraction : InteractiveObject
{
        [Header("Audio Configuration")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float audioVolume = 0.8f;
    [SerializeField] private float audioPitch = 1f;
    
    private bool isNoteOpen = false;
[Header("Plane Specific Settings")]
    public string planeType = "Default";
    public bool isCollectible = false;
    
    protected override void OnInteract()
    {
        
        isNoteOpen = !isNoteOpen;
        
        
        if (isNoteOpen && openSound != null)
        {
            AudioManager.Instance.PlaySFX(openSound, transform.position, audioVolume, audioPitch);
        }
        else if (!isNoteOpen && closeSound != null)
        {
            AudioManager.Instance.PlaySFX(closeSound, transform.position, audioVolume, audioPitch);
        }
        
        

        
        if (isCollectible)
        {
            

            gameObject.SetActive(false);
        }
        else
        {
            

        }
    }
}
