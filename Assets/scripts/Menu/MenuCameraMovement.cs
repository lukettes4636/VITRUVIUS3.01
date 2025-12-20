using UnityEngine;

public class MenuCameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of camera movement (higher = faster shake/sway)")]
    [Range(0.01f, 2f)]
    public float movementSpeed = 0.25f;
    
    [Tooltip("Maximum distance the camera can move from its starting position")]
    [Range(0.01f, 2f)]
    public float movementRadius = 0.2f;
    
    [Header("Rotation Settings")]
    [Tooltip("Enable subtle rotation")]
    public bool enableRotation = true;
    
    [Tooltip("Speed of camera rotation")]
    [Range(0.01f, 2f)]
    public float rotationSpeed = 0.2f;
    
    [Tooltip("Maximum rotation angle in degrees")]
    [Range(0.01f, 5f)]
    public float maxRotationAngle = 0.5f;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timeOffset;
    
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        timeOffset = Random.Range(0f, 1000f);
    }
    
    void Update()
    {
        float time = Time.time * movementSpeed + timeOffset;
        
        float xOffset = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * movementRadius;
        float yOffset = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * movementRadius;
        float zOffset = (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * movementRadius * 0.5f; 
        
        transform.position = startPosition + new Vector3(xOffset, yOffset, zOffset);
        
        if (enableRotation)
        {
            float rotTime = Time.time * rotationSpeed + timeOffset;
            
            float xRot = (Mathf.PerlinNoise(rotTime * 0.3f, 100f) - 0.5f) * 2f * maxRotationAngle;
            float yRot = (Mathf.PerlinNoise(rotTime * 0.3f, 200f) - 0.5f) * 2f * maxRotationAngle;
            float zRot = (Mathf.PerlinNoise(rotTime * 0.3f, 300f) - 0.5f) * 2f * maxRotationAngle * 0.5f;
            
            Quaternion targetRotation = startRotation * Quaternion.Euler(xRot, yRot, zRot);
            transform.rotation = targetRotation;
        }
    }
}
