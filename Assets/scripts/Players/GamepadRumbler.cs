using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq; 

public class GamepadRumbler : MonoBehaviour
{
    
    private Gamepad specificGamepad;

    [Header("Rumble Settings")]
    [Range(0f, 1f)] public float lowFrequency = 0.25f;
    [Range(0f, 1f)] public float highFrequency = 0.75f;
    [SerializeField] private float rumbleDuration = 0.25f;

    private Coroutine rumbleCoroutine;

    void Awake()
    {
        
        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            
            
            specificGamepad = playerInput.devices.OfType<Gamepad>().FirstOrDefault();
        }

        if (specificGamepad == null)
        {

        }
    }

    private void OnDisable()
    {
        
        StopRumble();
    }

    
    
    
    public void Rumble()
    {
        if (specificGamepad == null) return;

        if (rumbleCoroutine != null)
            StopCoroutine(rumbleCoroutine);

        rumbleCoroutine = StartCoroutine(RumbleRoutine(lowFrequency, highFrequency, rumbleDuration));
    }

    
    
    
    public void RumbleStrong()
    {
        if (specificGamepad == null) return;

        if (rumbleCoroutine != null)
            StopCoroutine(rumbleCoroutine);

        
        float strongLow = Mathf.Clamp01(lowFrequency * 4f); 
        float strongHigh = Mathf.Clamp01(highFrequency * 4f); 
        float strongDuration = rumbleDuration * 2.5f; 

        rumbleCoroutine = StartCoroutine(RumbleRoutine(strongLow, strongHigh, strongDuration));
    }

    
    
    
    public void StopRumble()
    {
        if (rumbleCoroutine != null)
        {
            StopCoroutine(rumbleCoroutine);
            rumbleCoroutine = null;
        }

        if (specificGamepad != null)
        {
            
            specificGamepad.SetMotorSpeeds(0f, 0f);
        }
    }


    private IEnumerator RumbleRoutine(float low, float high, float duration)
    {
        
        specificGamepad.SetMotorSpeeds(low, high);
        yield return new WaitForSeconds(duration);
        specificGamepad.SetMotorSpeeds(0f, 0f); 
        rumbleCoroutine = null;
    }
}
