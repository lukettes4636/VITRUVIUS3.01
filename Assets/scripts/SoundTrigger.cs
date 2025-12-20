using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundTrigger : MonoBehaviour
{
    [Header("Configuracion del Sonido")]
    [Tooltip("El sonido que se reproducira al entrar en la zona")]
    public AudioClip soundToPlay;
    
    [Tooltip("Volumen del sonido (0 a 1)")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("Si es verdadero, el sonido se reproducira solo la primera vez")]
    public bool playOnlyOnce = false;
    
    [Tooltip("Si es verdadero, el sonido se reproducira con PlayOneShot (permite superposicion)")]
    public bool useOneShot = true;

    [Header("Configuracion del Trigger")]
    [Tooltip("Tag del objeto que activara el sonido (ej: Player)")]
    public string targetTag = "Player";
    
    [Tooltip("Color del gizmo en el editor para identificar la zona")]
    public Color gizmoColor = new Color(0, 1, 0, 0.3f);

    private AudioSource audioSource;
    private bool hasPlayed = false;
    private Collider myCollider;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        myCollider = GetComponent<Collider>();
        
        
        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }
        else
        {
            
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            myCollider = box;
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; 
    }

    private void Reset()
    {
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }
        else
        {
            myCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnlyOnce && hasPlayed) return;

        if (other.CompareTag(targetTag))
        {
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (soundToPlay == null) return;

        if (useOneShot)
        {
            audioSource.PlayOneShot(soundToPlay, volume);
        }
        else
        {
            audioSource.clip = soundToPlay;
            audioSource.volume = volume;
            audioSource.Play();
        }

        if (playOnlyOnce)
        {
            hasPlayed = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        
        if (GetComponent<BoxCollider>() != null)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        
        else if (GetComponent<SphereCollider>() != null)
        {
            SphereCollider sphere = GetComponent<SphereCollider>();
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else
        {
            
            Gizmos.DrawIcon(transform.position, "Audio Source Gizmo", true);
        }
    }
}
