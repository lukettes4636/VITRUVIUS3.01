using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public abstract class PlayerControllerBase : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float runSpeed = 8f;
    [SerializeField] protected float crouchSpeed = 17.92f;
    [SerializeField] protected float crouchRunSpeed = 25f;
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected float gravity = -9.81f;

    [Header("Altura del Collider")]
    [SerializeField] protected float standHeight = 2f;
    [SerializeField] protected float crouchHeight = 1f;
    [SerializeField] protected Vector3 standCenter = new Vector3(0, 1f, 0);
    [SerializeField] protected Vector3 crouchCenter = new Vector3(0, 0.5f, 0);

    [Header("Estamina")]
    [SerializeField] protected float maxStamina = 100f;
    [SerializeField] protected float runCooldown = 4f;
    [SerializeField] protected float staminaDepletionRate = 12f;
    [SerializeField] protected float staminaRechargeRate = 12f;

    [Header("Procedural Animation (Fatiga)")]
    [Tooltip("Referencia al script que controla el Rigging de cansancio.")]
    [SerializeField] protected StaminaFatigueFeedback fatigueFeedback;

    [Header("Aceleracion")]
    [SerializeField] protected float acceleration = 12f;
    [SerializeField] protected float deceleration = 16f;
    protected float currentSpeedScalar;

    [Header("Collection Settings")]
    [SerializeField] protected float collectionRange = 2f;
    [SerializeField] protected LayerMask collectableLayer;
    [SerializeField] protected InputActionReference collectAction;

    [Header("Door Lift Settings")]
    [SerializeField] protected InputActionReference liftDoorAction;
    [SerializeField] protected float minHoldTimeToStartLift = 0.15f;

    [Header("Ring Toggle Settings")]
    [SerializeField] protected InputActionReference toggleRingAction;

    [Header("Visual Effect - Fog Sphere")]
    [Tooltip("Referencia al Visual Effect del jugador")]
    [SerializeField] protected VisualEffect fogSphereVFX;
    [Tooltip("Nombre del parametro Vector3 en el VFX Graph")]
    [SerializeField] protected string vfxCenterParameterName = "SpherePosition";
    [Tooltip("Offset vertical de la esfera respecto al jugador")]
    [SerializeField] protected Vector3 sphereOffset = new Vector3(0, 1f, 0);
    [Tooltip("Radio de despeje de niebla (aumentar para despejar desde mas lejos)")]
    [SerializeField] protected float fogClearingRadius = 8f;
    [SerializeField] protected string vfxRadiusParameterName = "Radius";
    [Tooltip("Tiempo de suavizado para el efecto de niebla")]
    [SerializeField] protected float vfxSmoothTime = 0.3f;

    [Header("Inventory Settings")]
    [SerializeField] protected GameObject inventoryCanvas;
    [SerializeField] protected InputActionReference inventoryAction;

    [Header("Audio (Animation Events)")]
    public AudioClip walkFootstepClip;
    public AudioClip runFootstepClip;
    public AudioClip crouchFootstepClip;

    [Header("Tired State")]
    [SerializeField] protected string tiredAnimationBool = "IsTired";
    [SerializeField] protected AudioClip pantingSound;
    [SerializeField] protected AudioSource audioSource;

    [Header("Popup Flotante sobre cabeza")]
    [SerializeField] protected PlayerPopupBillboard popupBillboard;

    
    protected CharacterController controller;
    protected Animator animator;
    protected PlayerInventory playerInventory;
    protected PlayerStaminaUI staminaUI;
    protected PlayerNoiseEmitter noiseEmitter;
    protected PlayerInput playerInput;
    protected Gamepad gamepad;

    
    protected Vector2 moveInput;
    protected bool isRunningInput = false;
    protected bool isCrouching = false;
    protected bool isMoving = false;
    protected float currentStamina;
    protected float cooldownTimer = 0f;
    protected bool canRun = true;
    protected Vector3 verticalVelocity;
    protected bool wasRunning = false;
    protected bool staminaWasEmpty = false;
    
    
    protected bool isInUI = false;
    protected KeypadUIManager currentLockUI = null;

    
    protected FallenDoor currentDoorToLift = null;
    protected bool isHoldingDoor = false;
    protected bool isAnimationInLiftState = false;
    protected float liftButtonHoldTime = 0f;
    protected bool liftButtonPressed = false;

    
    protected PuertaDobleAccion currentDoor = null;
    protected ElectricBox currentElectricBox = null;
    protected PuertaDobleConLlave currentKeyDoor = null;

    
    protected Vector3 currentVfxPosition;
    protected Vector3 vfxVelocity;

    
    protected Transform cameraTransform;
    protected Vector3 originalCameraPosition;
    protected bool isShaking = false;

    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeInput();
        InitializeState();
    }

    protected virtual void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInventory = GetComponent<PlayerInventory>();
        staminaUI = GetComponent<PlayerStaminaUI>();
        noiseEmitter = GetComponent<PlayerNoiseEmitter>();
        playerInput = GetComponent<PlayerInput>();
        
        if (fatigueFeedback == null) fatigueFeedback = GetComponent<StaminaFatigueFeedback>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        cameraTransform = Camera.main?.transform;
        if (cameraTransform != null) originalCameraPosition = cameraTransform.localPosition;

        if (playerInput != null && playerInput.devices.Count > 0)
            gamepad = playerInput.devices[0] as Gamepad;
    }

    protected virtual void InitializeInput()
    {
        if (collectAction != null) collectAction.action.performed += ctx => TryInteract();
        if (toggleRingAction != null) toggleRingAction.action.performed += ctx => OnToggleRing();
        if (inventoryAction != null) inventoryAction.action.performed += ctx => ToggleInventory();
        
        if (liftDoorAction != null)
        {
            liftDoorAction.action.performed += ctx => OnLiftDoorPressed();
            liftDoorAction.action.canceled += ctx => OnLiftDoorReleased();
        }
    }

    protected virtual void InitializeState()
    {
        currentStamina = maxStamina;
        currentSpeedScalar = moveSpeed;
        
        if (controller != null)
        {
            controller.height = standHeight;
            controller.center = standCenter;
        }

        if (staminaUI != null) staminaUI.InitializeMaxStamina(maxStamina);
    }

    protected virtual void OnEnable()
    {
        collectAction?.action.Enable();
        liftDoorAction?.action.Enable();
        inventoryAction?.action.Enable();
        toggleRingAction?.action.Enable();
        gamepad?.SetMotorSpeeds(0f, 0f);
    }

    protected virtual void OnDisable()
    {
        collectAction?.action.Disable();
        liftDoorAction?.action.Disable();
        inventoryAction?.action.Disable();
        toggleRingAction?.action.Disable();
        gamepad?.SetMotorSpeeds(0f, 0f);
    }

    protected virtual void Update()
    {
        UpdateVFX();

        if (isInUI || !enabled || controller == null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsRunning", false);
            return;
        }

        HandleStamina();
        HandleMovement();
        HandleDoorLifting();
    }

    protected void UpdateVFX()
    {
        if (fogSphereVFX != null)
        {
            Vector3 targetPosition = transform.position + sphereOffset;
            if (currentVfxPosition == Vector3.zero) currentVfxPosition = targetPosition;
            currentVfxPosition = Vector3.SmoothDamp(currentVfxPosition, targetPosition, ref vfxVelocity, vfxSmoothTime);
            fogSphereVFX.SetVector3(vfxCenterParameterName, currentVfxPosition);
        }
    }

    protected void HandleMovement()
    {
        bool moving = moveInput.magnitude > 0.1f;
        float desiredSpeed = GetDesiredSpeed();

        
        float accel = currentSpeedScalar < desiredSpeed ? acceleration : deceleration;
        currentSpeedScalar = Mathf.MoveTowards(currentSpeedScalar, desiredSpeed, accel * Time.deltaTime);

        
        Vector3 movement = CalculateMovementDirection();

        
        if (controller.isGrounded)
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;

        
        Vector3 finalMovement = (movement * currentSpeedScalar) + new Vector3(0, verticalVelocity.y, 0);
        controller.Move(finalMovement * Time.deltaTime);

        
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsRunning", isRunningInput && moving && canRun);
        animator.SetFloat("Speed", moving ? (isRunningInput && canRun ? (isCrouching ? 1.5f : 2f) : (isCrouching ? 1.0f : 1f)) : 0f);
    }

    protected virtual Vector3 CalculateMovementDirection()
    {
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0; camForward.Normalize();
            Vector3 camRight = cameraTransform.right;
            camRight.y = 0; camRight.Normalize();
            return (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }
        return new Vector3(moveInput.x, 0, moveInput.y);
    }

    protected virtual float GetDesiredSpeed()
    {
        if (!canRun) return 0f;
        if (isCrouching)
        {
            if (isRunningInput && moveInput.magnitude > 0.1f && canRun) return crouchRunSpeed;
            return crouchSpeed;
        }
        if (isRunningInput && moveInput.magnitude > 0.1f && canRun) return runSpeed;
        return moveInput.magnitude > 0.1f ? moveSpeed : 0f;
    }

    protected virtual void HandleStamina()
    {
        bool moving = moveInput.magnitude > 0.1f;
        
        
        if (currentStamina < maxStamina && !isRunningInput)
        {
            float prev = currentStamina;
            currentStamina = Mathf.Clamp(currentStamina + staminaRechargeRate * Time.deltaTime, 0, maxStamina);
            if (staminaUI != null) staminaUI.UpdateStaminaValue(currentStamina, maxStamina);
            
            if (prev < maxStamina && currentStamina >= maxStamina && staminaWasEmpty)
            {
                staminaUI?.OnStaminaFullyRecharged();
                staminaWasEmpty = false;
            }
        }

        
        if (isRunningInput && moving && canRun)
        {
            currentStamina = Mathf.Clamp(currentStamina - staminaDepletionRate * Time.deltaTime, 0, maxStamina);
            staminaUI?.UpdateStaminaValue(currentStamina, maxStamina);
        }

        
        if (currentStamina <= 0 && canRun)
        {
            SetExhaustedState(true);
        }

        
        if (!canRun)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                SetExhaustedState(false);
            }
        }
    }

    protected void SetExhaustedState(bool exhausted)
    {
        canRun = !exhausted;
        fatigueFeedback?.SetExhausted(exhausted);
        animator.SetBool(tiredAnimationBool, exhausted);

        if (exhausted)
        {
            cooldownTimer = runCooldown;
            staminaWasEmpty = true;
            staminaUI?.HideStaminaBar();
            
            if (audioSource != null && pantingSound != null)
            {
                audioSource.clip = pantingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            currentStamina = maxStamina;
             if (audioSource != null && audioSource.clip == pantingSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
            staminaUI?.OnStaminaFullyRecharged();
        }
    }

    protected virtual void HandleDoorLifting()
    {
        if (liftButtonPressed && currentDoorToLift != null && !isAnimationInLiftState && !isInUI)
        {
            liftButtonHoldTime += Time.deltaTime;
            if (liftButtonHoldTime >= minHoldTimeToStartLift && !animator.GetBool("IsStartingLift"))
            {
                isHoldingDoor = true;
                animator.SetBool("IsStartingLift", true);
            }
        }
    }

    
    public void OnMove(InputValue value) { if (!isInUI) moveInput = value.Get<Vector2>(); }
    public void OnRun(InputValue value) { if (!isInUI) isRunningInput = value.isPressed; }
    public void OnCrouch(InputValue value) { if (!isInUI && value.isPressed) isCrouching = !isCrouching; }

    protected virtual void TryInteract()
    {
        if (isInUI) return;

        
        if (currentDoor != null) { currentDoor.IntentoDeAccion(this.gameObject); return; }
        if (currentKeyDoor != null) { currentKeyDoor.IntentoAbrirPuerta(this); return; } 
        if (currentElectricBox != null) { currentElectricBox.TryDeactivatePower(this); return; } 

        
        TryCollectItems();
    }

    protected virtual void TryCollectItems()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, collectionRange, collectableLayer);
        foreach (Collider itemCollider in items)
        {
            GameObject item = itemCollider.gameObject;
            if (item.GetComponent<PuertaDobleAccion>() || item.GetComponent<ElectricBox>()) continue;

            PickableItem pickable = item.GetComponent<PickableItem>();
            if (pickable != null)
            {
                pickable.Collect(gameObject);
                popupBillboard?.ShowMessage($"I found the {pickable.DisplayName}!", 2f);
                return;
            }
             CollectableItem collectable = item.GetComponent<CollectableItem>();
            if (collectable != null)
            {
                collectable.Collect(gameObject);
                 popupBillboard?.ShowMessage($"I found the {collectable.ItemID}!", 2f);
                return;
            }
        }
    }

    protected void OnToggleRing() { if (!isInUI) noiseEmitter?.ToggleRingVisibility(); }
    
    protected void ToggleInventory()
    {
        if (playerInventory != null && playerInventory.HasKeyCard("Card") && !isInUI) 
            inventoryCanvas.SetActive(!inventoryCanvas.activeSelf);
    }
    
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        
        FallenDoor doorScript = other.GetComponent<FallenDoor>();
        if (doorScript != null) currentDoorToLift = doorScript;

         ElectricBox box = other.GetComponentInParent<ElectricBox>() ?? other.GetComponent<ElectricBox>();
        if (box != null) currentElectricBox = box;
        
        
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        FallenDoor doorScript = other.GetComponent<FallenDoor>();
        if (doorScript != null && doorScript == currentDoorToLift)
        {
            currentDoorToLift = null;
            if (isHoldingDoor || isAnimationInLiftState) OnLiftDoorReleased();
        }
        
    }
    
    
    public void OnLiftDoorPressed() { liftButtonPressed = true; liftButtonHoldTime = 0f; }
    public void OnLiftDoorReleased() 
    { 
        liftButtonPressed = false; 
        if (isAnimationInLiftState) animator.SetBool("ShouldCancelLift", true); 
    }

    public void OnDoorLiftAnimationStart() { if (currentDoorToLift == null || !isHoldingDoor) OnLiftDoorReleased(); else isAnimationInLiftState = true; }
    public void OnDoorLiftAnimationComplete() { isAnimationInLiftState = false; isHoldingDoor = false; animator.SetBool("IsLifting", false); }

}
