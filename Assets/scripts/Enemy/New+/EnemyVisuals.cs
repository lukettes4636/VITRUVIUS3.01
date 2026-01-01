using System.Collections;
using UnityEngine;

/// <summary>
/// Controla animaciones, audio y efectos visuales del enemigo.
/// SIMPLIFICADO: Sin sistema de scanning de cabeza.
/// </summary>
public class EnemyVisuals : MonoBehaviour
{
    [Header("Referencias")]
    private Animator anim;
    private AudioSource audioSource;
    private EnemyMotor motor;

    [Header("Audio")]
    public AudioClip roarClip;
    public AudioClip attackClip;
    public AudioClip secondaryAttackClip;
    public AudioClip wallBreakSound;
    public AudioClip eatingSound;

    [Header("Sistema de Pisadas")]
    public AudioClip crawlFootstepClip;
    public AudioClip walkFootstepClip;
    public float crawlFootstepInterval = 0.5f;
    public float walkFootstepInterval = 0.35f;
    [Range(0f, 0.3f)]
    public float pitchVariance = 0.1f;

    [Header("Hitboxes de Combate")]
    public GameObject rightHandCollider;
    public GameObject leftHandCollider;

    [Header("VFX - Shader Rugido")]
    public Material roarMaterial;
    public float maxRoarDistortion = 0.03f;

    [Header("Configuración Animator")]
    [Tooltip("Suavizado de transición de Speed")]
    public float animationDampTime = 5f;

    // FLAGS DE SINCRONIZACIÓN
    public bool AnimImpactReceived { get; private set; }
    public bool AnimFinishedReceived { get; private set; }

    // SISTEMA DE PISADAS
    private Coroutine footstepCoroutine;
    private AudioClip currentStepClip;
    private float currentStepInterval;

    // VFX ROAR
    private int _roarIntensityID;
    private int _isActiveID;
    private Coroutine roarVisualCoroutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
        motor = GetComponent<EnemyMotor>();

        if (anim != null)
            anim.applyRootMotion = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Setup hitboxes
        DisableAllHitboxes();
        EnsureHitboxSetup(rightHandCollider);
        EnsureHitboxSetup(leftHandCollider);

        // Setup shader roar
        if (roarMaterial != null)
        {
            _roarIntensityID = Shader.PropertyToID("_RoarIntensity");
            _isActiveID = Shader.PropertyToID("_IsActive");
            roarMaterial.SetFloat(_isActiveID, 0f);
        }
    }

    // ========================================
    // ANIMACIONES PRINCIPALES
    // ========================================

    /// <summary>
    /// Actualiza estado de locomoción (crawl vs stand).
    /// </summary>
    public void UpdateAnimationState(bool isCrawling)
    {
        anim.SetBool("isCrawling", isCrawling);

        // Speed para blend tree
        float targetSpeed = motor.IsMoving ? 1f : 0f;

        if (!motor.IsMoving)
        {
            anim.SetFloat("Speed", 0f);
        }
        else
        {
            float currentSpeed = anim.GetFloat("Speed");
            float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * animationDampTime);
            anim.SetFloat("Speed", newSpeed);
        }

        // Sistema de pisadas
        if (motor.IsMoving)
        {
            AudioClip targetClip = isCrawling ? crawlFootstepClip : walkFootstepClip;
            float targetInterval = isCrawling ? crawlFootstepInterval : walkFootstepInterval;
            UpdateFootsteps(targetClip, targetInterval);
        }
        else
        {
            StopFootsteps();
        }
    }

    /// <summary>
    /// Estado pasivo (0=Sleep, 1=Eat).
    /// </summary>
    public void SetPassiveState(int stateIndex)
    {
        anim.SetFloat("PassiveType", (float)stateIndex);

        // Audio de comer
        if (stateIndex == 1 && eatingSound != null)
        {
            if (!audioSource.isPlaying || audioSource.clip != eatingSound)
            {
                audioSource.clip = eatingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (audioSource.clip == eatingSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    // ========================================
    // TRIGGERS DE TRANSICIÓN
    // ========================================

    public void TriggerGetUp()
    {
        ResetSyncFlags();
        anim.ResetTrigger("ToCrawl");
        anim.SetTrigger("GetUp");
    }

    public void TriggerToCrawl()
    {
        ResetSyncFlags();
        anim.ResetTrigger("GetUp");
        anim.SetTrigger("ToCrawl");
    }

    public void TriggerRoar()
    {
        ResetSyncFlags();
        anim.SetTrigger("Roar");
    }

    public void TriggerAttack(int attackIndex)
    {
        ResetSyncFlags();
        anim.ResetTrigger("Attack");
        anim.SetInteger("AttackIndex", attackIndex);
        anim.SetTrigger("Attack");
    }

    public void StopAttack()
    {
        DisableAllHitboxes();
    }

    // ========================================
    // CALLBACKS DE ANIMACIÓN (Animation Events)
    // ========================================

    public void AE_ActionImpact()
    {
        AnimImpactReceived = true;
        EnableRightHand();
        EnableLeftHand();
    }

    public void AE_ActionFinish()
    {
        AnimFinishedReceived = true;
        DisableAllHitboxes();
    }

    public void ResetSyncFlags()
    {
        AnimImpactReceived = false;
        AnimFinishedReceived = false;
    }

    // ========================================
    // AUDIO
    // ========================================

    public void PlayRoarSound() => PlayOneShot(roarClip, 0.7f);
    public void PlayAttackSound()
    {
        PlayOneShot(attackClip, 0.7f);
        PlayOneShot(secondaryAttackClip, 0.7f);
    }
    public void PlayWallBreakSound() => PlayOneShot(wallBreakSound);

    private void PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }

    // ========================================
    // HITBOXES
    // ========================================

    public void EnableRightHand()
    {
        if (rightHandCollider) rightHandCollider.SetActive(true);
    }

    public void DisableRightHand()
    {
        if (rightHandCollider) rightHandCollider.SetActive(false);
    }

    public void EnableLeftHand()
    {
        if (leftHandCollider) leftHandCollider.SetActive(true);
    }

    public void DisableLeftHand()
    {
        if (leftHandCollider) leftHandCollider.SetActive(false);
    }

    private void DisableAllHitboxes()
    {
        DisableRightHand();
        DisableLeftHand();
    }

    private void EnsureHitboxSetup(GameObject go)
    {
        if (!go) return;

        var col = go.GetComponent<Collider>();
        if (col) col.isTrigger = true;

        var rb = go.GetComponent<Rigidbody>();
        if (!rb) rb = go.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    // ========================================
    // SHADER ROAR
    // ========================================

    public void AE_StartRoarEffect()
    {
        if (roarMaterial)
        {
            roarMaterial.SetFloat(_isActiveID, 1f);
            if (roarVisualCoroutine != null) StopCoroutine(roarVisualCoroutine);
            roarVisualCoroutine = StartCoroutine(RoarRoutine());
        }
    }

    public void AE_StopRoarEffect()
    {
        if (roarMaterial)
        {
            roarMaterial.SetFloat(_isActiveID, 0f);
            if (roarVisualCoroutine != null) StopCoroutine(roarVisualCoroutine);
        }
    }

    private IEnumerator RoarRoutine()
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime;
            float pulse = Mathf.Abs(Mathf.Sin(t * 10f)) * maxRoarDistortion;
            roarMaterial.SetFloat(_roarIntensityID, pulse);
            yield return null;
        }
    }

    // ========================================
    // SISTEMA DE PISADAS
    // ========================================

    private void UpdateFootsteps(AudioClip clip, float interval)
    {
        if (footstepCoroutine != null && currentStepClip == clip && currentStepInterval == interval)
            return;

        StopFootsteps();
        currentStepClip = clip;
        currentStepInterval = interval;
        footstepCoroutine = StartCoroutine(FootstepRoutine(clip, interval));
    }

    private void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
        currentStepClip = null;
    }

    private IEnumerator FootstepRoutine(AudioClip clip, float interval)
    {
        while (true)
        {
            if (clip != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
                audioSource.PlayOneShot(clip, 0.7f);
            }
            yield return new WaitForSeconds(interval);
        }
    }
}