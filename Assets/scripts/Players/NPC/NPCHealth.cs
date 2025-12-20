using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;





[RequireComponent(typeof(NPCIdentifier))]
public class NPCHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    [Header("UI World Space Settings")]
    [Tooltip("El CanvasGroup del World Space UI (barra de vida).")]
    [SerializeField] private CanvasGroup healthCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [Tooltip("Tiempo que la barra de vida permanece visible despues de recibir dano.")]
    [SerializeField] private float displayTimeAfterDamage = 3.0f;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [Tooltip("Sonido de dolor critico cuando la vida esta baja")]
    [SerializeField] private AudioClip criticalPainSound;

    [Header("Efectos Visuales")]
    [SerializeField] private UnityEngine.VFX.VisualEffect bloodParticlesPrefab;
    [SerializeField] private float bloodEffectProbability = 0.7f;
    [Tooltip("El Transform que marca el punto de origen de los efectos de sangre (debe estar en el pecho del NPC).")]
    [SerializeField] private Transform chestImpactPoint;

    [Header("Animation References")]
    [SerializeField] private Animator npcAnimator;

    private int currentHealth;
    private bool isInvulnerable = false;
    public bool IsDead { get; private set; } = false;
    private bool criticalHealthTriggered = false;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private Transform mainCameraTransform;
    private NPCIdentifier npcIdentifier;

    public Action<int, int> OnHealthChanged;
    public Action<int> OnNPCDied;

    void Awake()
    {
        npcIdentifier = GetComponent<NPCIdentifier>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
        }

        if (npcAnimator == null)
            npcAnimator = GetComponentInChildren<Animator>();

        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;

        
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

        if (healthCanvasGroup != null)
        {
            healthCanvasGroup.alpha = 0f;
            healthCanvasGroup.interactable = false;
            healthCanvasGroup.blocksRaycasts = false;
        }

        UpdateUI();
    }

    void Update()
    {
        
        if (healthCanvasGroup != null && mainCameraTransform != null && !IsDead)
        {
            healthCanvasGroup.transform.LookAt(
                healthCanvasGroup.transform.position + mainCameraTransform.rotation * Vector3.forward,
                mainCameraTransform.rotation * Vector3.up
            );
            healthCanvasGroup.transform.rotation *= Quaternion.Euler(0, 180f, 0);
        }
    }

    
    
    
    public void TakeDamage(int damage)
    {
        if (IsDead || isInvulnerable)
            return;

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

    private void ApplyDamageEffects()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        
        PlayBloodEffect();

        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger("Hit");
        }
    }

    
    
    
    private void PlayBloodEffect()
    {
        if (bloodParticlesPrefab == null || chestImpactPoint == null)
            return;

        if (UnityEngine.Random.value > bloodEffectProbability)
            return;

        Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
        Vector3 spawnPos = chestImpactPoint.position;
        UnityEngine.VFX.VisualEffect bloodVFX = Instantiate(bloodParticlesPrefab, spawnPos, randomRotation);
        bloodVFX.Play();

        StartCoroutine(AttachAndDetachVFX(bloodVFX, chestImpactPoint, 0.05f, 2.5f));
    }

    private IEnumerator AttachAndDetachVFX(UnityEngine.VFX.VisualEffect vfx, Transform parent, float attachDelay, float lifetime)
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
        ApplyDeathEffects();

        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        if (healthCanvasGroup != null) healthCanvasGroup.alpha = 0f;

        if (npcAnimator != null)
        {
            npcAnimator.enabled = true;
            npcAnimator.SetBool("IsDead", true);
        }

        
        if (npcIdentifier != null)
            OnNPCDied?.Invoke(npcIdentifier.npcID);

        
        NPCBehaviorManager behavior = GetComponent<NPCBehaviorManager>();
        if (behavior != null)
            behavior.enabled = false;

        
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;
    }

    private void ApplyDeathEffects()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
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
}

