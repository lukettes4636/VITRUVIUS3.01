using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class ObjectCarryingSystem : MonoBehaviour
{
    [Header("Rigging Setup")]
    public Rig carryingRig;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("Referencias de Codos (Hints)")]
    [Tooltip("Arrastra aqui el objeto Hint_L")]
    public Transform leftElbowHint;
    [Tooltip("Arrastra aqui el objeto Hint_R")]
    public Transform rightElbowHint;

    [Header("Configuracion General")]
    public float transitionSpeed = 8f;

    [Header("Polishing - Inercia y Respiracion")]
    [Tooltip("Cantidad de movimiento vertical al respirar/caminar")]
    public float bobAmount = 0.05f;
    [Tooltip("Velocidad del movimiento vertical")]
    public float bobSpeed = 2f;
    [Tooltip("Que tanto se abren los codos al cargar algo")]
    public float elbowOutOffset = 0.3f;

    [Header("Configuracion de Lanzamiento")]
    [Tooltip("Dibuja la curva en el inspector: Eje Y(0 a 1) es el progreso. Haz que empiece lento y suba rapido.")]
    public AnimationCurve throwVelocityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    
    private float _currentThrowForward;
    private float _currentThrowUpward;
    private float _currentThrowSpeed;

    
    private float defaultThrowForward = 1.2f;
    private float defaultThrowUpward = 0.1f;
    private float defaultThrowSpeed = 15f;

    
    private Transform _gripL;
    private Transform _gripR;

    private bool _isCarrying = false;
    private bool _isThrowing = false;
    private float _targetWeight = 0f;

    private Vector3 _throwTargetPosL;
    private Vector3 _throwTargetPosR;
    private Quaternion _throwTargetRotL;
    private Quaternion _throwTargetRotR;

    
    private Vector3 _defaultHintPosL;
    private Vector3 _defaultHintPosR;

    private void Start()
    {
        if (carryingRig != null) carryingRig.weight = 0f;

        _currentThrowForward = defaultThrowForward;
        _currentThrowUpward = defaultThrowUpward;
        _currentThrowSpeed = defaultThrowSpeed;

        
        if (leftElbowHint) _defaultHintPosL = leftElbowHint.localPosition;
        if (rightElbowHint) _defaultHintPosR = rightElbowHint.localPosition;
    }

    private void LateUpdate()
    {
        if (carryingRig == null) return;

        
        carryingRig.weight = Mathf.Lerp(carryingRig.weight, _targetWeight, Time.deltaTime * transitionSpeed);

        
        
        if (_isCarrying || _isThrowing)
        {
            if (leftElbowHint)
                leftElbowHint.localPosition = Vector3.Lerp(leftElbowHint.localPosition, _defaultHintPosL + (Vector3.left * elbowOutOffset), Time.deltaTime * 5f);

            if (rightElbowHint)
                rightElbowHint.localPosition = Vector3.Lerp(rightElbowHint.localPosition, _defaultHintPosR + (Vector3.right * elbowOutOffset), Time.deltaTime * 5f);
        }
        else
        {
            
            if (leftElbowHint) leftElbowHint.localPosition = Vector3.Lerp(leftElbowHint.localPosition, _defaultHintPosL, Time.deltaTime * 5f);
            if (rightElbowHint) rightElbowHint.localPosition = Vector3.Lerp(rightElbowHint.localPosition, _defaultHintPosR, Time.deltaTime * 5f);
        }

        
        if (_isThrowing)
        {
            
            
        }
        else if (_isCarrying && _gripL != null && _gripR != null)
        {
            
            
            float breathingEffect = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            Vector3 bobVector = new Vector3(0, breathingEffect, 0);

            
            
            
            float handFollowSpeed = 20f; 

            leftHandTarget.position = Vector3.Lerp(leftHandTarget.position, _gripL.position + bobVector, Time.deltaTime * handFollowSpeed);
            leftHandTarget.rotation = Quaternion.Slerp(leftHandTarget.rotation, _gripL.rotation, Time.deltaTime * handFollowSpeed);

            rightHandTarget.position = Vector3.Lerp(rightHandTarget.position, _gripR.position + bobVector, Time.deltaTime * handFollowSpeed);
            rightHandTarget.rotation = Quaternion.Slerp(rightHandTarget.rotation, _gripR.rotation, Time.deltaTime * handFollowSpeed);
        }
    }

    public void ApplyProfile(PlayerGrabProfile profile)
    {
        if (profile != null)
        {
            _currentThrowForward = profile.ThrowArmReach;
            _currentThrowUpward = profile.ThrowArmHeight;
            _currentThrowSpeed = profile.ThrowAnimationSpeed;
        }
        else
        {
            _currentThrowForward = defaultThrowForward;
            _currentThrowUpward = defaultThrowUpward;
            _currentThrowSpeed = defaultThrowSpeed;
        }
    }

    public void StartCarrying(GameObject heldObject)
    {
        Transform foundGripL = heldObject.transform.Find("Grip_L");
        Transform foundGripR = heldObject.transform.Find("Grip_R");

        if (foundGripL != null && foundGripR != null)
        {
            _gripL = foundGripL;
            _gripR = foundGripR;
            _isCarrying = true;
            _isThrowing = false;
            _targetWeight = 1f;
        }
        else
        {
            _isCarrying = false;
            _targetWeight = 0f;
        }
    }

    public void StopCarrying()
    {
        if (!_isThrowing)
        {
            _isCarrying = false;
            _targetWeight = 0f;
            _gripL = null;
            _gripR = null;
        }
    }

    public void PlayThrowAnimation()
    {
        _isThrowing = true;
        _targetWeight = 1f;

        Vector3 forwardDir = transform.forward;
        Vector3 upDir = transform.up;
        Vector3 displacement = (forwardDir * _currentThrowForward) + (upDir * _currentThrowUpward);

        
        Vector3 startPosL = leftHandTarget.position;
        Vector3 startPosR = rightHandTarget.position;

        _throwTargetPosL = startPosL + displacement;
        _throwTargetPosR = startPosR + displacement;

        _throwTargetRotL = leftHandTarget.rotation;
        _throwTargetRotR = rightHandTarget.rotation;

        
        StartCoroutine(AnimateThrowCurve(startPosL, startPosR));
    }

    
    private IEnumerator AnimateThrowCurve(Vector3 startL, Vector3 startR)
    {
        float timer = 0f;
        
        
        float duration = 0.25f; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            
            float curveValue = throwVelocityCurve.Evaluate(progress);

            
            leftHandTarget.position = Vector3.Lerp(startL, _throwTargetPosL, curveValue);
            rightHandTarget.position = Vector3.Lerp(startR, _throwTargetPosR, curveValue);

            
            leftHandTarget.rotation = _throwTargetRotL;
            rightHandTarget.rotation = _throwTargetRotR;

            yield return null;
        }

        
        yield return new WaitForSeconds(0.15f);

        _isThrowing = false;
        _isCarrying = false;
        _targetWeight = 0f;
        _gripL = null;
        _gripR = null;
    }
}