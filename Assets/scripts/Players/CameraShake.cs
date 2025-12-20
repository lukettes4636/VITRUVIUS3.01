using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    private Vector3 initialPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        
        initialPosition = transform.localPosition;
    }

    private void OnDisable()
    {
        
        StopShake();
    }

    
    
    
    public void Shake()
    {
        
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    
    
    
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        transform.localPosition = initialPosition;
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
            transform.localPosition = initialPosition + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        
        transform.localPosition = initialPosition;
        shakeCoroutine = null;
    }
}