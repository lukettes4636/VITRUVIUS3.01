using UnityEngine;
using UnityEngine.Animations.Rigging;

public class StaminaFatigueFeedback : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aqui el objeto 'Fatigue_Rig' nuevo")]
    public Rig fatigueRig;

    [Tooltip("Arrastra aqui el objeto 'Target_Slouch'")]
    public Transform breathingTarget;

    [Header("Configuracion Visual")]
    public float transitionSpeed = 5f; 
    public float breathSpeed = 14f;    
    public float breathAmount = 8f;   

    private bool _isExhausted = false;
    private Quaternion _initialRotation;

    void Start()
    {
        
        if (breathingTarget != null)
            _initialRotation = breathingTarget.localRotation;

        
        if (fatigueRig != null)
            fatigueRig.weight = 0f;
    }

    void Update()
    {
        if (fatigueRig == null) return;

        
        
        float targetW = _isExhausted ? 1f : 0f;
        fatigueRig.weight = Mathf.Lerp(fatigueRig.weight, targetW, Time.deltaTime * transitionSpeed);

        
        if (fatigueRig.weight > 0.1f && breathingTarget != null)
        {
            
            float breathAngle = Mathf.Sin(Time.time * breathSpeed) * breathAmount;

            
            
            Quaternion breathRot = Quaternion.Euler(breathAngle, 0, 0);

            breathingTarget.localRotation = _initialRotation * breathRot;
        }
    }

    
    public void SetExhausted(bool state)
    {
        _isExhausted = state;
    }
}