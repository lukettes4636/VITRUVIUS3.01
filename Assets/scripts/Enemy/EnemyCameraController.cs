using UnityEngine;
using System.Collections;

public class EnemyCameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CameraController cameraController;

    [Header("Configuracion de Zoom")]
    [SerializeField] private float combatMinSize = 10f;
    [SerializeField] private float combatMaxSize = 22f;
    [Tooltip("Buffer extra alrededor de los personajes durante combate")]
    [SerializeField] private float combatEdgeBuffer = 6f;

    [Header("Transiciones Suaves")]
    [SerializeField] private float zoomOutDuration = 2.0f;
    [SerializeField] private float zoomInDuration = 1.5f;
    [Tooltip("Curva de animacion para el zoom. Ajusta para controlar la suavidad")]
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Transform enemyTransform;
    private bool isTrackingEnemy = false;
    private Coroutine transitionCoroutine = null;

    
    private float originalMinSize;
    private float originalMaxSize;
    private float originalEdgeBuffer;

    private void Awake()
    {
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        if (cameraController == null)
        {

        }
    }

    
    
    
    public void StartTrackingEnemy(Transform enemy)
    {
        if (isTrackingEnemy || enemy == null || cameraController == null) return;

        enemyTransform = enemy;

        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        
        transitionCoroutine = StartCoroutine(SmoothStartTracking());
    }

    
    
    
    public void StopTrackingEnemy()
    {
        if (!isTrackingEnemy || cameraController == null) return;

        
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        
        transitionCoroutine = StartCoroutine(SmoothStopTracking());
    }

    private IEnumerator SmoothStartTracking()
    {
        if (cameraController == null || enemyTransform == null) yield break;

        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic) yield break;

        
        originalMinSize = GetCameraMinSize();
        originalMaxSize = GetCameraMaxSize();
        originalEdgeBuffer = GetCameraEdgeBuffer();

        float startSize = mainCamera.orthographicSize;

        
        cameraController.AddTarget(enemyTransform);
        isTrackingEnemy = true;

        
        float elapsedTime = 0f;

        while (elapsedTime < zoomOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = zoomCurve.Evaluate(elapsedTime / zoomOutDuration);

            
            float newMinSize = Mathf.Lerp(originalMinSize, combatMinSize, t);
            float newMaxSize = Mathf.Lerp(originalMaxSize, combatMaxSize, t);
            float newEdgeBuffer = Mathf.Lerp(originalEdgeBuffer, combatEdgeBuffer, t);

            SetCameraParameters(newMinSize, newMaxSize, newEdgeBuffer);

            yield return null;
        }

        
        SetCameraParameters(combatMinSize, combatMaxSize, combatEdgeBuffer);

        transitionCoroutine = null;
    }

    private IEnumerator SmoothStopTracking()
    {
        if (cameraController == null || enemyTransform == null) yield break;

        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic) yield break;

        float startSize = mainCamera.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < zoomInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = zoomCurve.Evaluate(elapsedTime / zoomInDuration);

            
            float newMinSize = Mathf.Lerp(combatMinSize, originalMinSize, t);
            float newMaxSize = Mathf.Lerp(combatMaxSize, originalMaxSize, t);
            float newEdgeBuffer = Mathf.Lerp(combatEdgeBuffer, originalEdgeBuffer, t);

            SetCameraParameters(newMinSize, newMaxSize, newEdgeBuffer);

            yield return null;
        }

        
        SetCameraParameters(originalMinSize, originalMaxSize, originalEdgeBuffer);

        
        cameraController.RemoveTarget(enemyTransform);

        
        isTrackingEnemy = false;
        enemyTransform = null;
        transitionCoroutine = null;
    }

    
    private float GetCameraMinSize()
    {
        var field = typeof(CameraController).GetField("minSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(cameraController);
        return 6f; 
    }

    private float GetCameraMaxSize()
    {
        var field = typeof(CameraController).GetField("maxSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(cameraController);
        return 18f; 
    }

    private float GetCameraEdgeBuffer()
    {
        var field = typeof(CameraController).GetField("edgeBuffer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(cameraController);
        return 4f; 
    }

    private void SetCameraParameters(float minSize, float maxSize, float edgeBuffer)
    {
        var minField = typeof(CameraController).GetField("minSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var maxField = typeof(CameraController).GetField("maxSize",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var bufferField = typeof(CameraController).GetField("edgeBuffer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (minField != null) minField.SetValue(cameraController, minSize);
        if (maxField != null) maxField.SetValue(cameraController, maxSize);
        if (bufferField != null) bufferField.SetValue(cameraController, edgeBuffer);
    }

    
    
    
    public bool IsTrackingEnemy()
    {
        return isTrackingEnemy;
    }

    private void OnDestroy()
    {
        
        if (isTrackingEnemy && transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        if (isTrackingEnemy && cameraController != null)
        {
            if (enemyTransform != null)
                cameraController.RemoveTarget(enemyTransform);

            
            SetCameraParameters(originalMinSize, originalMaxSize, originalEdgeBuffer);
        }
    }
}
