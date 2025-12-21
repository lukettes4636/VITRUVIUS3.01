using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponWallAvoidance : MonoBehaviour
{
    [Header("Referencias")]
    public Rig armDownRig;
    public Animator animator; 

    [Header("Configuracin de Deteccin")]
    public Transform rayOrigin;
    public float detectionDistance = 1.2f;
    public LayerMask obstacleMask;

    [Header("Suavizado")]
    public float smoothSpeed = 10f;

    
    private const string BOOL_FLASHLIGHT = "FlashlightOn";
    private const string BOOL_DEAD = "IsDeadAnimator";

    private float _targetWeight;
    private float _currentWeight;

    void Start()
    {
        if (armDownRig != null) armDownRig.weight = 0f;

        
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (rayOrigin == null || armDownRig == null || animator == null) return;

        
        
        bool isFlashlightOn = animator.GetBool(BOOL_FLASHLIGHT);
        bool isDead = animator.GetBool(BOOL_DEAD);

        
        if (isFlashlightOn && !isDead)
        {
            DetectWall();
        }
        else
        {
            
            _targetWeight = 0f;
        }

        
        
        _currentWeight = Mathf.Lerp(_currentWeight, _targetWeight, Time.deltaTime * smoothSpeed);
        armDownRig.weight = _currentWeight;
    }

    private void DetectWall()
    {
        Ray ray = new Ray(rayOrigin.position, transform.forward);

#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction * detectionDistance, _targetWeight > 0.5f ? Color.red : Color.green);
#endif

        if (Physics.Raycast(ray, detectionDistance, obstacleMask))
        {
            _targetWeight = 1f; 
        }
        else
        {
            _targetWeight = 0f; 
        }
    }
}