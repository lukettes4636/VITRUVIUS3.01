using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _mainCamera;

    [Header("Targets")]
    [SerializeField] private List<Transform> targets = new List<Transform>();

    [Header("Zoom Settings")]
    [SerializeField] private float edgeBuffer = 4.0f;
    [SerializeField] private float minSize = 6.0f;
    [SerializeField] private float maxSize = 18.0f;
    [SerializeField] private float smoothTime = 0.2f;

    private Vector3 _velocity;
    private float _zoomSpeed;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        transform.position = GetAveragePosition();
        _mainCamera.orthographicSize = GetDesiredSize();
    }

    private void LateUpdate()
    {
        SetPosition();
        SetSize();
    }

    private void SetPosition()
    {
        transform.position = Vector3.SmoothDamp(transform.position, GetAveragePosition(), ref _velocity, smoothTime);
    }

    private void SetSize()
    {
        _mainCamera.orthographicSize = Mathf.SmoothDamp(
            _mainCamera.orthographicSize,
            GetDesiredSize(),
            ref _zoomSpeed,
            smoothTime
        );
    }

    private Vector3 GetAveragePosition()
    {
        Vector3 avg = Vector3.zero;
        int activeCount = 0;

        foreach (var target in targets)
        {
            if (target == null) continue;

            var health = target.GetComponent<PlayerHealth>();
            
            if (health != null && health.IsIgnoredByCamera) continue; 

            if (!target.gameObject.activeInHierarchy) continue;

            avg += target.position;
            activeCount++;
        }

        if (activeCount == 0) return transform.position;
        return avg / activeCount;
    }

    private float GetDesiredSize()
    {
        float size = 0f;
        Vector3 desiredLocalPos = transform.InverseTransformPoint(GetAveragePosition());

        foreach (var target in targets)
        {
            if (target == null) continue;

            var health = target.GetComponent<PlayerHealth>();
            
            if (health != null && health.IsIgnoredByCamera) continue; 

            if (!target.gameObject.activeInHierarchy) continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(target.position);
            Vector3 delta = targetLocalPos - desiredLocalPos;

            size = Mathf.Max(size, Mathf.Abs(delta.y), Mathf.Abs(delta.x) / _mainCamera.aspect);
        }

        return Mathf.Clamp(size + edgeBuffer, minSize, maxSize);
    }

    
    
    
    public void AddTarget(Transform newTarget)
    {
        if (!targets.Contains(newTarget))
            targets.Add(newTarget);
    }

    public void RemoveTarget(Transform target)
    {
        if (targets.Contains(target))
            targets.Remove(target);
    }
}