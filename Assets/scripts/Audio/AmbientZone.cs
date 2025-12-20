using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    public AudioClip ambientSound;
    [Range(0f, 1f)]
    public float volume = 1f;
    public float spatialBlend = 1f;
    public bool loop = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            AudioManager.Instance.PlaySFX(ambientSound, transform.position, spatialBlend, volume);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.size);
    }
}