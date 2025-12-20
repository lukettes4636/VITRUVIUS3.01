using UnityEngine;
using System.Collections;





public class DialogueCameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("GameObject padre que contiene el CameraController. Si la camara es hija de un objeto con CameraController, asignalo aqui.")]
    [SerializeField] private Transform cameraParentTransform;
    [Tooltip("Referencia al CameraController. Se deshabilitara durante el dialogo.")]
    [SerializeField] private CameraController cameraController;
    
    [Header("Ajustes de Camara durante Dialogo")]
    [SerializeField] private float dialogueCameraTilt = 15f; 
    [SerializeField] private float smoothTime = 0.2f;
    
    [Header("Zoom Settings (similar a CameraController)")]
    [SerializeField] private float edgeBuffer = 4.0f;
    [SerializeField] private float minSize = 6.0f;
    [SerializeField] private float maxSize = 18.0f;
    
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private float originalOrthographicSize;
    private bool isDialogueActive = false;
    private Transform npcTransform;
    private Transform currentPlayerTransform;
    private Vector3 _velocity;
    private float _zoomSpeed;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            return;
        }
        
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();
        
        if (cameraParentTransform == null && mainCamera.transform.parent != null)
        {
            cameraParentTransform = mainCamera.transform.parent;
            
            if (cameraController == null && cameraParentTransform != null)
                cameraController = cameraParentTransform.GetComponent<CameraController>();
        }
        
        if (cameraParentTransform == null)
            cameraParentTransform = mainCamera.transform;
        
        originalRotation = cameraParentTransform.rotation;
        originalPosition = cameraParentTransform.position;
        
        if (mainCamera.orthographic)
            originalOrthographicSize = mainCamera.orthographicSize;
    }
    
    
    
    
    public void StartDialogueCamera(Transform npc, Transform player)
    {
        if (isDialogueActive) return;
        
        isDialogueActive = true;
        npcTransform = npc;
        currentPlayerTransform = player;
        
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
        
        if (cameraParentTransform != null)
        {
            originalRotation = cameraParentTransform.rotation;
            originalPosition = cameraParentTransform.position;
        }
        
        if (mainCamera.orthographic)
            originalOrthographicSize = mainCamera.orthographicSize;
    }
    
    public void EndDialogueCamera()
    {
        if (!isDialogueActive) return;
        
        StartCoroutine(SmoothEndDialogueCamera());
    }
    
    private IEnumerator SmoothEndDialogueCamera()
    {
        if (cameraParentTransform == null || mainCamera == null) yield break;
        
        Quaternion startRotation = cameraParentTransform.rotation;
        float startSize = mainCamera.orthographic ? mainCamera.orthographicSize : originalOrthographicSize;
        
        float elapsedTime = 0f;
        float duration = 0.5f; 
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            
            
            cameraParentTransform.rotation = Quaternion.Lerp(startRotation, originalRotation, t);
            
            
            if (mainCamera.orthographic)
            {
                float targetSize = originalOrthographicSize;
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            }
            
            yield return null;
        }
        
        
        cameraParentTransform.rotation = originalRotation;
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = originalOrthographicSize;
        }
        
        
        isDialogueActive = false;
        npcTransform = null;
        currentPlayerTransform = null;
        
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
    
    private void LateUpdate()
    {
        
        if (!isDialogueActive || cameraParentTransform == null || mainCamera == null) return;
        
        SetPosition();
        SetSize();
        SetRotation();
    }
    
    private void SetPosition()
    {
        Vector3 targetPosition = GetAveragePosition();
        cameraParentTransform.position = Vector3.SmoothDamp(
            cameraParentTransform.position, 
            targetPosition, 
            ref _velocity, 
            smoothTime
        );
    }
    
    private void SetSize()
    {
        if (!mainCamera.orthographic) return;
        
        float targetSize = GetDesiredSize();
        mainCamera.orthographicSize = Mathf.SmoothDamp(
            mainCamera.orthographicSize,
            targetSize,
            ref _zoomSpeed,
            smoothTime
        );
    }
    
    private void SetRotation()
    {
        Quaternion targetRotation = originalRotation * Quaternion.Euler(-dialogueCameraTilt, 0, 0);
        cameraParentTransform.rotation = Quaternion.Lerp(
            cameraParentTransform.rotation, 
            targetRotation, 
            Time.deltaTime * 2f
        );
    }
    
    private Vector3 GetAveragePosition()
    {
        Vector3 avg = Vector3.zero;
        int count = 0;
        
        
        if (npcTransform != null && npcTransform.gameObject.activeInHierarchy)
        {
            avg += npcTransform.position;
            count++;
        }
        
        
        if (currentPlayerTransform != null && currentPlayerTransform.gameObject.activeInHierarchy)
        {
            var health = currentPlayerTransform.GetComponent<PlayerHealth>();
            if (health == null || !health.IsIgnoredByCamera)
            {
                avg += currentPlayerTransform.position;
                count++;
            }
        }
        
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        
        if (player1 != null && player1.activeInHierarchy && player1.transform != currentPlayerTransform)
        {
            var health = player1.GetComponent<PlayerHealth>();
            if (health == null || !health.IsIgnoredByCamera)
            {
                avg += player1.transform.position;
                count++;
            }
        }
        
        if (player2 != null && player2.activeInHierarchy && player2.transform != currentPlayerTransform)
        {
            var health = player2.GetComponent<PlayerHealth>();
            if (health == null || !health.IsIgnoredByCamera)
            {
                avg += player2.transform.position;
                count++;
            }
        }
        
        if (count == 0) return cameraParentTransform.position;
        return avg / count;
    }
    
    private float GetDesiredSize()
    {
        if (!mainCamera.orthographic) return originalOrthographicSize;
        
        float size = 0f;
        Vector3 averagePos = GetAveragePosition();
        Vector3 desiredLocalPos = cameraParentTransform.InverseTransformPoint(averagePos);
        
        
        if (npcTransform != null && npcTransform.gameObject.activeInHierarchy)
        {
            Vector3 targetLocalPos = cameraParentTransform.InverseTransformPoint(npcTransform.position);
            Vector3 delta = targetLocalPos - desiredLocalPos;
            size = Mathf.Max(size, Mathf.Abs(delta.y), Mathf.Abs(delta.x) / mainCamera.aspect);
        }
        
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        
        if (player1 != null && player1.activeInHierarchy)
        {
            var health = player1.GetComponent<PlayerHealth>();
            if (health == null || !health.IsIgnoredByCamera)
            {
                Vector3 targetLocalPos = cameraParentTransform.InverseTransformPoint(player1.transform.position);
                Vector3 delta = targetLocalPos - desiredLocalPos;
                size = Mathf.Max(size, Mathf.Abs(delta.y), Mathf.Abs(delta.x) / mainCamera.aspect);
            }
        }
        
        if (player2 != null && player2.activeInHierarchy)
        {
            var health = player2.GetComponent<PlayerHealth>();
            if (health == null || !health.IsIgnoredByCamera)
            {
                Vector3 targetLocalPos = cameraParentTransform.InverseTransformPoint(player2.transform.position);
                Vector3 delta = targetLocalPos - desiredLocalPos;
                size = Mathf.Max(size, Mathf.Abs(delta.y), Mathf.Abs(delta.x) / mainCamera.aspect);
            }
        }
        
        return Mathf.Clamp(size + edgeBuffer, minSize, maxSize);
    }
    
    private void OnDestroy()
    {
        if (isDialogueActive)
        {
            if (cameraParentTransform != null)
            {
                cameraParentTransform.rotation = originalRotation;
            }
            
            if (mainCamera != null && mainCamera.orthographic)
            {
                mainCamera.orthographicSize = originalOrthographicSize;
            }
            
            if (cameraController != null)
            {
                cameraController.enabled = true;
            }
        }
    }
}


