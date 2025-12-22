using System.Collections;
using UnityEngine;

using UnityEngine.Animations.Rigging;

public class EnemyVisuals : MonoBehaviour
{
    
    private Animator anim;
    private AudioSource audioSource;
    private EnemyMotor motor;

    
    
    

    [Header("--- AUDIO ---")]
    public AudioClip roarClip;
    public AudioClip attackClip;
    public AudioClip secondaryAttackClip;
    public AudioClip wallBreakSound;
    public AudioClip eatingSound;

    [Header("--- SISTEMA DE PISADAS ---")]
    public AudioClip crawlFootstepClip;
    public AudioClip walkFootstepClip;
    public float crawlFootstepInterval = 0.5f;
    public float walkFootstepInterval = 0.35f;
    [Tooltip("Variacion aleatoria del tono del audio")]
    public float pitchVariance = 0.1f;

    [Header("--- HITBOXES (COMBATE) ---")]
    public GameObject rightHandCollider;
    public GameObject leftHandCollider;

    [Header("--- VFX (SHADER RUGIDO) ---")]
    public Material roarMaterial;
    public float maxRoarDistortion = 0.03f;

    [Header("--- CONFIGURACION ANIMATOR ---")]
    [Tooltip("Suavizado de la transicion de caminar (Blend Tree)")]
    public float animationDampTime = 5f;

    [Header("--- SISTEMA DE MIRADA (IK RIGGING) ---")]
    [Tooltip("Arrastra aqui el objeto 'IK_Rig' que tiene el componente Rig")]
    public Rig headAimRig;
    [Tooltip("Arrastra aqui el objeto 'LookTarget' (Esfera vacia)")]
    public Transform lookTarget;
    [Tooltip("Hueso de la cabeza para anclar el objetivo de mirada")]
    public Transform headBone;
    [Tooltip("Distancia lateral del barrido de cabeza")]
    public float scanWidth = 1.5f;
    [Tooltip("Velocidad del movimiento de cabeza")]
    public float scanSpeed = 2.0f;
    [Tooltip("Ancho del barrido cuando esta agachado/crawling")]
    public float crawlScanWidth = 0.8f;
    [Tooltip("Velocidad del barrido cuando esta agachado/crawling")]
    public float crawlScanSpeed = 1.5f;
    [Tooltip("Amplitud vertical del barrido en modo crawling")]
    public float verticalScanAmplitude = 0.2f;
    [Tooltip("Distancia frontal del LookTarget desde la cabeza")]
    public float lookTargetDistance = 1.2f;
    [Tooltip("Velocidad de mezcla del peso del rig de mirada")]
    public float investigateRigBlendSpeed = 2f;
    private bool overrideScan = false;
    private float overrideT = 0f;
    public bool IsScanning { get; private set; }

    
    
    

    
    public bool AnimImpactReceived { get; private set; }
    public bool AnimFinishedReceived { get; private set; }

    
    private Coroutine footstepCoroutine;
    private AudioClip currentStepClip;
    private float currentStepInterval;

    
    private int _roarIntensityID;
    private int _isActiveID;
    private Coroutine roarVisualCoroutine;

    
    private bool isInvestigating = false;
    private Vector3 initialTargetLocalPos; 

    
    
    

    void Awake()
    {
        anim = GetComponent<Animator>();
        motor = GetComponent<EnemyMotor>();
        if (anim != null) anim.applyRootMotion = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        
        DisableAllHitboxes();
        EnsureHitboxSetup(rightHandCollider);
        EnsureHitboxSetup(leftHandCollider);

        
        if (roarMaterial != null)
        {
            _roarIntensityID = Shader.PropertyToID("_RoarIntensity");
            _isActiveID = Shader.PropertyToID("_IsActive");
            roarMaterial.SetFloat(_isActiveID, 0f);
        }

        
        if (lookTarget != null)
        {
            
            initialTargetLocalPos = lookTarget.localPosition;
        }

        
        if (headAimRig != null)
        {
            headAimRig.weight = 0f;
        }
    }

    void Update()
    {
        HandleHeadScanningLogic();
    }

    
    
    

    private void HandleHeadScanningLogic()
    {
        if (lookTarget == null || headAimRig == null) return;

        
        
        float targetWeight = isInvestigating ? 1f : 0f;
        headAimRig.weight = Mathf.MoveTowards(headAimRig.weight, targetWeight, Time.deltaTime * investigateRigBlendSpeed);

        
        if (headAimRig.weight > 0.01f)
        {
            
            bool isCrawlingAnim = anim != null && anim.GetBool("isCrawling");
            float useSpeed = isCrawlingAnim ? crawlScanSpeed : scanSpeed;
            float useWidth = isCrawlingAnim ? crawlScanWidth : scanWidth;
            float t = overrideScan ? overrideT : Time.time * useSpeed;
            float oscX = Mathf.Sin(t) * useWidth;
            float oscY = isCrawlingAnim ? Mathf.Sin(t * 0.5f) * verticalScanAmplitude : 0f;

            
            if (headBone != null)
            {
                Vector3 basePos = headBone.position + headBone.forward * lookTargetDistance;
                Vector3 targetPosWorld = basePos + headBone.right * oscX + headBone.up * oscY;
                lookTarget.position = targetPosWorld;
            }
            else
            {
                Vector3 targetPosLocal = initialTargetLocalPos;
                targetPosLocal.x += oscX;
                targetPosLocal.y += oscY;
                lookTarget.localPosition = targetPosLocal;
            }

            
            Vector3 targetPos = initialTargetLocalPos;
            targetPos.x += oscX;
            
        }
        else
        {
            
            lookTarget.localPosition = Vector3.Lerp(lookTarget.localPosition, initialTargetLocalPos, Time.deltaTime * 5f);
        }
    }

    
    public void SetInvestigatingMode(bool state)
    {
        isInvestigating = state;
    }

    public bool IsCrawlingAnim()
    {
        return anim != null && anim.GetBool("isCrawling");
    }
    
    
    

    public void UpdateAnimationState(bool isCrawling)
    {
        
        anim.SetBool("isCrawling", isCrawling);

        
        float targetSpeed = motor.IsMoving ? 1f : 0f;
        float currentSpeed = anim.GetFloat("Speed");

        
        if (!motor.IsMoving)
        {
            anim.SetFloat("Speed", 0f);
        }
        else
        {
            float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * animationDampTime);
            anim.SetFloat("Speed", newSpeed);
        }

        
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

    
    public void SetPassiveState(int stateIndex)
    {
        anim.SetFloat("PassiveType", (float)stateIndex);

        
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

    
    public void SetSleep(bool state) { if (state) SetPassiveState(0); }
    public void SetEat(bool state) { if (state) SetPassiveState(1); }

    
    
    

    public void TriggerAttack(int attackIndex)
    {
        ResetSyncFlags();
        anim.ResetTrigger("Attack");
        anim.SetInteger("AttackIndex", attackIndex);
        anim.SetTrigger("Attack");
    }

    public void TriggerRoar()
    {
        ResetSyncFlags();
        anim.SetTrigger("Roar");
    }

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

    public void StopAttack() => DisableAllHitboxes();

    

    public void ResetSyncFlags()
    {
        AnimImpactReceived = false;
        AnimFinishedReceived = false;
    }

    
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

    
    public void EnableRightHand() { if (rightHandCollider) rightHandCollider.SetActive(true); }
    public void DisableRightHand() { if (rightHandCollider) rightHandCollider.SetActive(false); }
    public void EnableLeftHand() { if (leftHandCollider) leftHandCollider.SetActive(true); }
    public void DisableLeftHand() { if (leftHandCollider) leftHandCollider.SetActive(false); }

    private void DisableAllHitboxes() { DisableRightHand(); DisableLeftHand(); }

    
    
    

    private void EnsureHitboxSetup(GameObject go)
    {
        if (!go) return;
        var col = go.GetComponent<Collider>();
        if (col) col.isTrigger = true;
        var rb = go.GetComponent<Rigidbody>();
        if (!rb)
        {
            rb = go.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

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

    
    
    

    private void UpdateFootsteps(AudioClip clip, float interval)
    {
        if (footstepCoroutine != null && currentStepClip == clip && currentStepInterval == interval) return;

        StopFootsteps();
        currentStepClip = clip;
        currentStepInterval = interval;
        footstepCoroutine = StartCoroutine(FootstepRoutine(clip, interval));
    }

    private void StopFootsteps()
    {
        if (footstepCoroutine != null) StopCoroutine(footstepCoroutine);
        footstepCoroutine = null;
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

    public IEnumerator RunScanCycles(int cycles)
    {
        SetInvestigatingMode(true);
        overrideScan = true;
        IsScanning = true;
        int completed = 0;
        float lastSign = 0f;
        bool isCrawlingAnim = anim != null && anim.GetBool("isCrawling");
        float useSpeed = isCrawlingAnim ? crawlScanSpeed : scanSpeed;
        overrideT = 0f;
        while (completed < cycles)
        {
            overrideT += Time.deltaTime * useSpeed;
            float s = Mathf.Sin(overrideT);
            float sign = Mathf.Sign(s);
            if (lastSign <= 0f && sign > 0f)
            {
                completed++;
            }
            lastSign = sign;
            yield return null;
        }
        overrideScan = false;
        IsScanning = false;
        SetInvestigatingMode(false);
    }
}
