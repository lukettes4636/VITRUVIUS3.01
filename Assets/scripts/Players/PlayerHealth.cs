using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.VFX; 

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private GamepadRumbler rumbler;
    [SerializeField] private PlayerUIController uiController;

    private PlayerIdentifier playerIdentifier;
    private PlayerInventory playerInventory;

    public Action<int, int> OnHealthChanged;
    public Action<int> OnPlayerDied;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    
    [SerializeField] private int electricDamagePerSecond = 5;

    [Header("Animation Settings")]
    
    [SerializeField] private float deathAnimationDuration = 1.5f;

    
    
    
    [Header("Death Camera Settings")]
    [Tooltip("Tiempo en segundos despues del fin de la animacion de muerte que la camara debe seguir al jugador.")]
    [SerializeField] private float cameraFollowDelay = 2.0f;
    

    [Header("UI World Space Settings")]
    [Tooltip("El CanvasGroup del World Space UI (barra de vida).")]
    [SerializeField] private CanvasGroup healthCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [Tooltip("Tiempo que la barra de vida permanece visible despues de recibir dano.")]
    [SerializeField] private float displayTimeAfterDamage = 3.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip damageSound;
    [Tooltip("Death sound")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip electricDamageSound;
    [Tooltip("Critical pain sound")]
    [SerializeField] private AudioClip criticalPainSound;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Efectos Visuales")]
    
    [SerializeField] private VisualEffect bloodParticlesPrefab;
    [Tooltip("El Transform que marca el punto de origen de los efectos de sangre (debe estar en el pecho del jugador).")]
    [SerializeField] private Transform chestImpactPoint;

    [Header("Animation References")]
    [SerializeField] private Animator playerAnimator;

    [Header("Input (UI Map)")]
    [SerializeField] private InputActionReference submitAction;

    private int currentHealth;
    
    private bool isInvulnerable = false;
    public bool IsDead { get; private set; } = false;
    private bool canRespawn = false;
    private string lastDamageSource;

    
    
    
    public bool IsIgnoredByCamera { get; private set; } = false; 
    private Coroutine cameraDelayCoroutine;
    

    private Vector3 lastCheckpointPosition;
    private bool isInElectricTrap = false;
    private bool criticalHealthTriggered = false;

    private CharacterController characterController;
    private MonoBehaviour movementScript;
    private Coroutine electricDamageCoroutine;
    private PlayerInput playerInput;
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private Transform mainCameraTransform;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        playerIdentifier = GetComponent<PlayerIdentifier>();
        playerInventory = GetComponent<PlayerInventory>();

        audioSource = GetComponent<AudioSource>();
        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        
        movementScript = GetComponent("MovJugador1") as MonoBehaviour;
        if (movementScript == null)
            movementScript = GetComponent("MovJugador2") as MonoBehaviour;

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }


        chestImpactPoint = transform.Find("ChestImpactPoint");
        if (chestImpactPoint == null)
        {
            GameObject chestPointObj = new GameObject("ChestImpactPoint");
            chestPointObj.transform.SetParent(transform);
            chestPointObj.transform.localPosition = new Vector3(0, 1.4f, 0); 
            chestImpactPoint = chestPointObj.transform;
        }


    }

    void Start()
    {
        currentHealth = maxHealth;
        IsDead = false;
        
        IsIgnoredByCamera = false;
        lastCheckpointPosition = transform.position;

        if (healthCanvasGroup != null)
        {
            healthCanvasGroup.alpha = 0f;
            healthCanvasGroup.interactable = false;
            healthCanvasGroup.blocksRaycasts = false;
        }

        UpdateUI();

        if (submitAction != null && submitAction.action != null)
            submitAction.action.performed += OnRespawnInput;

        Checkpoint.OnCheckpointReached += HandleCheckpointReached;

        if (playerInput != null && !IsDead)
            playerInput.SwitchCurrentActionMap("Player");
    }

    private void OnDestroy()
    {
        if (submitAction != null && submitAction.action != null)
            submitAction.action.performed -= OnRespawnInput;

        Checkpoint.OnCheckpointReached -= HandleCheckpointReached;
    }

    
    
    
    void Update()
    {
        if (healthCanvasGroup != null && mainCameraTransform != null)
        {
            healthCanvasGroup.transform.LookAt(healthCanvasGroup.transform.position + mainCameraTransform.rotation * Vector3.forward,
                                               mainCameraTransform.rotation * Vector3.up);

            healthCanvasGroup.transform.rotation *= Quaternion.Euler(0, 180f, 0);
        }
    }

    
    
    
    private void HandleCheckpointReached(int checkpointPlayerID, Vector3 position)
    {
        if (playerIdentifier != null && playerIdentifier.playerID == checkpointPlayerID)
        {
            lastCheckpointPosition = position + Vector3.up * 0.1f;


            if (uiController != null)
                uiController.ShowNotification("Checkpoint saved!");
        }
    }

    private void OnRespawnInput(InputAction.CallbackContext context)
    {

        if (IsDead && canRespawn)
            RestoreState();
    }

    
    
    
    public void TakeDamage(int damage)
    {
        
        if (IsDead || isInvulnerable)
        {

            return;
        }


        StartCoroutine(BecomeInvulnerable());

        
        ApplyDamageEffects();

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);


        
        CheckCriticalHealthState();

        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        
        if (healthCanvasGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(ShowHealthBarRoutine());
        }

        if (currentHealth <= 0)
        {

            Die();
        }
    }

    public void EnterElectricTrap()
    {
        if (isInElectricTrap || IsDead) return;

        isInElectricTrap = true;
        if (electricDamageCoroutine != null) StopCoroutine(electricDamageCoroutine);
        electricDamageCoroutine = StartCoroutine(ElectricDamageRoutine());
    }

    public void ExitElectricTrap()
    {
        isInElectricTrap = false;
        if (electricDamageCoroutine != null)
        {
            StopCoroutine(electricDamageCoroutine);
            electricDamageCoroutine = null;
        }
    }

    private IEnumerator ElectricDamageRoutine()
    {
        while (isInElectricTrap && !IsDead)
        {
            TakeDamage(electricDamagePerSecond);
            if (IsDead) yield break;
            yield return new WaitForSeconds(1f);
        }
        electricDamageCoroutine = null;
    }

    private void ApplyDamageEffects()
    {
        
        if (cameraShake != null) cameraShake.Shake();

        if (rumbler != null)
        {
            rumbler.StopRumble();
            rumbler.Rumble();
        }

        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        
        PlayBloodEffect();

        if (playerAnimator != null)
        {
            
            
            
            playerAnimator.SetTrigger("Hit");
            
        }
    }

    
    public void TakeElectricDamage(int damage)
    {
        if (IsDead || isInvulnerable) return;

        StartCoroutine(BecomeInvulnerable());
        ApplyElectricDamageEffects(); 
        TakeDamage(damage);
    }

    private void ApplyElectricDamageEffects()
    {
        
        ApplyDamageEffects();

        
        if (electricDamageSound != null && audioSource != null)
            audioSource.PlayOneShot(electricDamageSound);

        
        if (cameraShake != null)
            cameraShake.Shake(); 
    }

    
    
    
    private void PlayBloodEffect()
    {
        if (bloodParticlesPrefab == null)
        {
            return;
        }

        if (chestImpactPoint == null)
        {
            return;
        }
        
        
        

        Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

        
        Vector3 spawnPos = chestImpactPoint.position;

        
        VisualEffect bloodVFX = Instantiate(bloodParticlesPrefab, spawnPos, randomRotation);
        
        
        bloodVFX.Play();

        
        StartCoroutine(AttachAndDetachVFX(bloodVFX, chestImpactPoint, 0.05f, 2.5f));
    }

    private IEnumerator AttachAndDetachVFX(VisualEffect vfx, Transform parent, float attachDelay, float lifetime)
    {
        
        yield return new WaitForSeconds(attachDelay);

        if (vfx != null && parent != null)
        {
            vfx.transform.SetParent(parent);
            vfx.transform.localPosition = Vector3.zero;
        }

        yield return new WaitForSeconds(lifetime);
        if (vfx != null) Destroy(vfx.gameObject);
    }

    private void CheckCriticalHealthState()
    {
        bool isCritical = currentHealth <= maxHealth * 0.5f;

        if (isCritical && !criticalHealthTriggered)
        {
            
            TriggerCriticalHealthEffects();
            criticalHealthTriggered = true;
        }
        else if (!isCritical)
        {
            criticalHealthTriggered = false;
        }
    }

    private void TriggerCriticalHealthEffects()
    {
        
        if (criticalPainSound != null && audioSource != null)
            audioSource.PlayOneShot(criticalPainSound);
    }

    
    private IEnumerator BecomeInvulnerable()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    
    
    
    private IEnumerator ShowHealthBarRoutine()
    {
        
        float timer = 0f;
        float startAlpha = healthCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            healthCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer / fadeDuration);
            yield return null;
        }
        healthCanvasGroup.alpha = 1f;

        
        yield return new WaitForSeconds(displayTimeAfterDamage);

        
        timer = 0f;
        startAlpha = healthCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            healthCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }
        healthCanvasGroup.alpha = 0f;

        fadeCoroutine = null;
    }

    
    
    
    private void Die()
    {
        if (IsDead) return;


        IsDead = true;
        canRespawn = false;

        
        IsIgnoredByCamera = false;

        ExitElectricTrap();
        ApplyDeathEffects(); 

        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (healthCanvasGroup != null) healthCanvasGroup.alpha = 0f;

        if (playerInventory != null)
            playerInventory.RemoveNonEssentialItems();

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI");

        
        if (movementScript is MovJugador1 mov1) mov1.StopMovement();
        else if (movementScript is MovJugador2 mov2) mov2.StopMovement();

        StartCoroutine(DeathSequenceRoutine());
    }

    
    
    
    private void ApplyDeathEffects()
    {
        
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.SetBool("IsDeadAnimator", true);
        }

        
        if (rumbler != null)
        {
            rumbler.StopRumble();
            rumbler.RumbleStrong(); 
        }

        
        if (cameraShake != null) cameraShake.StopShake();
    }
    

    private IEnumerator DeathSequenceRoutine()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        if (uiController != null)
            uiController.StartRespawnTimer(RespawnReady);


        if (playerIdentifier != null)
            OnPlayerDied?.Invoke(playerIdentifier.playerID);

        
        
        
        if (cameraDelayCoroutine != null) StopCoroutine(cameraDelayCoroutine);
        cameraDelayCoroutine = StartCoroutine(CameraDelayRoutine());
        
    }

    
    
    
    private IEnumerator CameraDelayRoutine()
    {
        
        yield return new WaitForSeconds(cameraFollowDelay);

        
        IsIgnoredByCamera = true;
        cameraDelayCoroutine = null;
    }
    

    public void RespawnReady()
    {
        canRespawn = true;

    }

    
    
    
    public void RestoreState()
    {
        if (!IsDead || !canRespawn) return;


        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (healthCanvasGroup != null) healthCanvasGroup.alpha = 0f;

        
        
        
        
        if (cameraDelayCoroutine != null)
        {
            StopCoroutine(cameraDelayCoroutine);
            cameraDelayCoroutine = null;
        }
        IsIgnoredByCamera = false; 
        

        
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        IsDead = false;
        canRespawn = false;
        currentHealth = maxHealth;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsDeadAnimator", false);
        }

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

        
        if (characterController != null)
            characterController.enabled = false;

        transform.position = lastCheckpointPosition; 

        
        if (characterController != null)
            characterController.enabled = true;

        if (movementScript != null)
            movementScript.enabled = true;

        
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = true;

        if (uiController != null)
            uiController.HideRespawnPanel();

        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

    }

    
    
    
    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";
    }

    
    
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public void SetLastDamageSource(string source) => lastDamageSource = source;
}
