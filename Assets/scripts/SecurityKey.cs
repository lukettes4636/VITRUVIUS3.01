using UnityEngine;

public class SecurityKey : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private bool useAudioConfig = true;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    private void Start()
    {
        startPosition = transform.position;
        
        if (gameObject.tag != "Collectable")
        {
            gameObject.tag = "Collectable";
        }
    }
    
    private void Update()
    {
        if (isCollected) return;
        
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    public void Collect(GameObject collector)
    {
        if (isCollected) return;
        
        isCollected = true;
        
        PlayerInventory inventory = collector.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            if (inventory.AddKeyCard("SecurityKey"))
            {
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, transform.rotation);
                }
                
                if (useAudioConfig)
                {
                    SoundHelper.PlayPickupSound(transform.position);
                }
                else if (collectSound != null)
                {
                    AudioManager.Instance.PlaySFX(collectSound, transform.position, 0.7f, Random.Range(0.9f, 1.1f));
                }
                

                Destroy(gameObject);
            }
        }
    }
}
