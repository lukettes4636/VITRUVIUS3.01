using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public float rotationSpeed = 100f;

    
    public Behaviour volumetricBeam;

    [Header("Input Settings")]
    public InputActionReference toggleAction;

    [Header("Arm Settings")]
    public Transform armTransform;
    public Vector3 armRotation = new Vector3(0, 0, 0);

    [Header("Audio")]
    [SerializeField] private bool playFlashlightSounds = true;

    [Header("Animation")]
    [Tooltip("Animator que contiene el parametro booleano 'FlashlightOn'.")]
    public Animator flashlightAnimator;

    public bool isFlashlightOn = true;

    
    [HideInInspector] public float originalIntensity;

    void Start()
    {
        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();

        if (flashlight != null)
        {
            if (volumetricBeam == null)
            {
                
            }

            
            originalIntensity = flashlight.intensity;

            
            SetFlashlightState(isFlashlightOn, true);
        }

        SetupArm();

        if (toggleAction != null)
        {
            toggleAction.action.performed += OnToggleFlashlight;
        }
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Disable();
        }
    }

    void Update()
    {
        RotateFlashlight();
    }

    void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleFlashlight();
        }
    }

    
    
    
    public void SetFlashlightState(bool state, bool immediate = false)
    {
        isFlashlightOn = state;

        
        if (flashlight != null)
        {
            flashlight.intensity = state ? originalIntensity : 0f;
            flashlight.enabled = state; 
        }

        
        if (volumetricBeam != null)
        {
            volumetricBeam.enabled = state;
        }

        
        if (flashlightAnimator != null)
        {
            
            flashlightAnimator.SetBool("FlashlightOn", state);
        }

        
        ToggleAllChildObjects(state);
    }

    
    private void ToggleAllChildObjects(bool state)
    {
        
        Light[] allLights = GetComponentsInChildren<Light>(true);
        foreach (Light light in allLights)
        {
            if (light != flashlight) 
            {
                light.enabled = state;
            }
        }

        
        Behaviour[] allBehaviours = GetComponentsInChildren<Behaviour>(true);
        foreach (Behaviour behaviour in allBehaviours)
        {
            if (behaviour != volumetricBeam && behaviour != this)
            {
                
                if (behaviour.GetType().Name.Contains("Spotlight") || 
                    behaviour.GetType().Name.Contains("Light") ||
                    behaviour.GetType().Name.Contains("VLB"))
                {
                    behaviour.enabled = state;
                }
            }
        }

        
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            renderer.enabled = state;
        }
    }

    void ToggleFlashlight()
    {
        
        PlayerInventory inventory = GetComponentInParent<PlayerInventory>();
        if (inventory == null || !inventory.HasItem("Flashlight"))
        {

            return;
        }

        
        if (playFlashlightSounds)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig().doorOpenSounds.Length > 0)
            {
                AudioClip doorClip = AudioManager.Instance.GetAudioConfig().doorOpenSounds[0];
                float pitchValue = isFlashlightOn ? 1.8f : 2.0f;
                AudioManager.Instance.PlaySFX(doorClip, transform.position, 0.6f, 1f, pitchValue);
            }
        }

        
        SetFlashlightState(!isFlashlightOn);
    }

    void RotateFlashlight()
    {
        if (armTransform != null)
        {
            armTransform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void SetupArm()
    {
        if (armTransform != null)
        {
            armTransform.localRotation = Quaternion.Euler(armRotation);
        }
    }
}
