using UnityEngine;

public class CanvasScreenClamper : MonoBehaviour
{
    
    [Tooltip("Margen que el Canvas debe mantener desde los bordes de la pantalla (ej: 0.05 es 5%).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float marginPercentage = 0.05f;

    private Transform mainCameraTransform;

    void Start()
    {
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null)
            return;

        
        if (!gameObject.activeSelf)
            return;

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        Camera cam = Camera.main;
        Vector3 currentPosition = transform.position;
        Vector3 viewportPosition = cam.WorldToViewportPoint(currentPosition);
        if (viewportPosition.z < 0) return;
        float minX = marginPercentage;
        float maxX = 1f - marginPercentage;
        float minY = marginPercentage;
        float maxY = 1f - marginPercentage;
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, minX, maxX);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, minY, maxY);
        Vector3 clampedWorldPosition = cam.ViewportToWorldPoint(viewportPosition);
        transform.position = clampedWorldPosition;
    }
}