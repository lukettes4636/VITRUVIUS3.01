using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class NemesisAI_Enhanced : MonoBehaviour
{
    [Header("Nemesis Configuration - Simplified")]
    [Tooltip("Standard walking speed")]
    public float walkSpeed = 3.5f;
    [Tooltip("Chase speed (fast walk)")]
    public float chaseSpeed = 4.5f;
    public float rotationSpeed = 10f;
    
    [Header("Detection Settings")]
    public float detectionRadius = 40f; 
    public float attackRange = 2.5f;
    public LayerMask obstacleLayerMask; 
    
    [Header("AI Behavior")]
    public float memoryDuration = 10f;
    public float targetSwitchDelay = 2f;
    
    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    public int attackDamage = 30;
    public float attackDuration = 0.9f;
    public float attackRangeMultiplier = 1.2f;
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    public AudioClip[] detectionSounds;
    public AudioClip[] footstepSounds;
    public AudioClip chaseMusic;
    public AudioClip wallBreakSound;
    public ChaseMusicController chaseMusicController;
    
    [Header("Wall Breaking")]
    public LayerMask breakableWallLayer;
    public float wallBreakDistance = 2.0f;
    
    [Header("References")]
    public Animator animator;
    public NemesisDetectionHelper detectionHelper;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    public AudioSource musicAudioSource;
    
    public enum NemesisState { Idle, Chase, Attack, Search, BreakWall }
    private NemesisState currentState = NemesisState.Idle;
    
    private Transform currentTarget;
    private TargetInfo currentTargetInfo;
    private Vector3 lastKnownPosition;
    private float lastDetectionTime;
    private float lastTargetSwitchTime;
    private bool hasTriggeredChaseMusic = false;
    
    private List<Transform> potentialTargets = new List<Transform>();
    private float targetScanTimer;
    private float searchTimer;
    
    
    private readonly int walkHash = Animator.StringToHash("Walk");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int detectionHash = Animator.StringToHash("Detected");
    
    
    private HashSet<int> validParams = new HashSet<int>();
    
    private class TargetInfo
    {
        public Transform transform;
        public string tag;
        public float distance;
        
        public TargetInfo(Transform t, string tag, float d)
        {
            transform = t;
            this.tag = tag;
            distance = d;
        }
    }
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        detectionHelper = GetComponent<NemesisDetectionHelper>();
        chaseMusicController = GetComponent<ChaseMusicController>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        musicAudioSource.spatialBlend = 0f; 
        musicAudioSource.volume = 1f;
        musicAudioSource.playOnAwake = false;
        
        ConfigureNavMeshAgent();
        
        if (obstacleLayerMask == 0)
        {
            obstacleLayerMask = LayerMask.GetMask("Default", "Walls", "Obstacles");
        }
        
        CacheAnimatorParameters();
        
        
        if (chaseMusicController != null && chaseMusic != null)
        {
            chaseMusicController.chaseMusicClip = chaseMusic;
            chaseMusicController.musicAudioSource = musicAudioSource;
            chaseMusicController.loopMusic = true;
            chaseMusicController.musicVolume = 1f;
        }
    }
    
    void CacheAnimatorParameters()
    {
        if (animator == null) return;
        if (animator.runtimeAnimatorController == null) return;
        
        foreach (var param in animator.parameters)
        {
            validParams.Add(param.nameHash);
        }
    }
    
    void SetAnimBool(int hash, bool value)
    {
        if (animator != null && validParams.Contains(hash))
        {
            animator.SetBool(hash, value);
        }
    }
    
    void SetAnimTrigger(int hash)
    {
        if (animator != null && validParams.Contains(hash))
        {
            animator.SetTrigger(hash);
        }
    }
    
    void Start()
    {
        currentState = NemesisState.Idle;
        FindTargets();
    }
    
    void Update()
    {
        if (Time.time > targetScanTimer + 3f)
        {
            FindTargets();
            targetScanTimer = Time.time;
        }
        
        UpdateAIState();
        CheckForWalls();
        ExecuteCurrentState();
        UpdateAnimation();
    }
    
    void ConfigureNavMeshAgent()
    {
        agent.speed = walkSpeed;
        agent.angularSpeed = 360f;
        agent.acceleration = 15f;
        agent.stoppingDistance = attackRange - 0.5f;
        agent.autoBraking = true;
        agent.updateRotation = true;
    }
    
    void FindTargets()
    {
        potentialTargets.Clear();
        
        GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
        if (p1) potentialTargets.Add(p1.transform);
        
        GameObject p2 = GameObject.FindGameObjectWithTag("Player2");
        if (p2) potentialTargets.Add(p2.transform);
        
        GameObject npc = GameObject.FindGameObjectWithTag("NPC");
        if (npc) potentialTargets.Add(npc.transform);
    }
    
    void UpdateAIState()
    {
        if (currentState == NemesisState.Attack || currentState == NemesisState.BreakWall) return;
        
        TargetInfo bestTarget = ScanForTargets();
        
        if (bestTarget != null)
        {
            if (currentState != NemesisState.Chase)
            {
                
                bool isPlayerTarget = bestTarget.tag == "Player1" || bestTarget.tag == "Player2";
                if (!hasTriggeredChaseMusic && chaseMusic != null && isPlayerTarget)
                {
                    
                    if (chaseMusicController != null)
                    {
                        chaseMusicController.PlayChaseMusic();
                    }
                    else
                    {
                        musicAudioSource.clip = chaseMusic;
                        musicAudioSource.Play();
                    }
                    hasTriggeredChaseMusic = true;

                }
                
                PlayDetectionSound();
                SetAnimTrigger(detectionHash);
                currentState = NemesisState.Chase;
            }
            
            if (currentTarget != bestTarget.transform)
            {
                if (currentTarget == null || Time.time - lastTargetSwitchTime > targetSwitchDelay)
                {
                    currentTarget = bestTarget.transform;
                    currentTargetInfo = bestTarget;
                    lastTargetSwitchTime = Time.time;
                }
            }
            else
            {
                currentTargetInfo = bestTarget;
            }
            
            lastDetectionTime = Time.time;
            lastKnownPosition = currentTarget.position;
        }
        else
        {
            if (currentState == NemesisState.Chase)
            {
                if (Time.time - lastDetectionTime < memoryDuration)
                {
                    currentState = NemesisState.Search;
                }
                else
                {
                    currentState = NemesisState.Idle;
                }
            }
            else if (currentState == NemesisState.Search)
            {
                if (Vector3.Distance(transform.position, lastKnownPosition) < 1f || Time.time - lastDetectionTime > memoryDuration)
                {
                    currentState = NemesisState.Idle;
                }
            }
        }
    }
    
    TargetInfo ScanForTargets()
    {
        TargetInfo best = null;
        float minDistance = float.MaxValue;
        
        foreach (var t in potentialTargets)
        {
            if (t == null) continue;
            
            if (!IsTargetAlive(t)) continue;
            
            float dist = Vector3.Distance(transform.position, t.position);
            if (dist > detectionRadius) continue;
            
            if (!HasLineOfSight(t)) continue;
            
            
            if (dist < minDistance)
            {
                minDistance = dist;
                best = new TargetInfo(t, t.tag, dist);
            }
        }
        return best;
    }
    
    bool IsTargetAlive(Transform t)
    {
        if (t.CompareTag("Player1") || t.CompareTag("Player2"))
        {
            var health = t.GetComponent<PlayerHealth>();
            return health != null && !health.IsDead;
        }
        else if (t.CompareTag("NPC"))
        {
            var health = t.GetComponent<NPCHealth>();
            return health != null && !health.IsDead;
        }
        return true;
    }
    
    bool HasLineOfSight(Transform t)
    {
        Vector3 start = transform.position + Vector3.up * 1.5f; 
        Vector3 end = t.position + Vector3.up * 1.0f; 
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        
        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, obstacleLayerMask))
        {
            if (hit.transform != t && !hit.transform.IsChildOf(t))
            {
                return false;
            }
        }
        return true;
    }
    
    void CheckForWalls()
    {
        if (currentState == NemesisState.Attack || currentState == NemesisState.BreakWall) return;
        
        
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, wallBreakDistance, breakableWallLayer))
        {
            StartCoroutine(BreakWallSequence(hit.collider.gameObject));
        }
    }
    
    IEnumerator BreakWallSequence(GameObject wall)
    {
        currentState = NemesisState.BreakWall;
        agent.ResetPath();
        
        SetAnimBool(attackHash, true);
        
        yield return new WaitForSeconds(attackDuration * 0.4f);
        
        if (wall != null)
        {
            if (wallBreakSound != null) audioSource.PlayOneShot(wallBreakSound);
            
            
            Destroy(wall); 
        }
        
        yield return new WaitForSeconds(attackDuration * 0.6f);
        
        SetAnimBool(attackHash, false);
        yield return new WaitForSeconds(0.5f);
        
        currentState = NemesisState.Chase;
    }

    void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case NemesisState.Idle:
                agent.ResetPath();
                break;
                
            case NemesisState.Chase:
                if (currentTarget != null)
                {
                    agent.speed = chaseSpeed;
                    agent.SetDestination(currentTarget.position);
                    
                    float dist = Vector3.Distance(transform.position, currentTarget.position);
                    if (dist <= attackRange)
                    {
                        currentState = NemesisState.Attack;
                        StartCoroutine(AttackSequence());
                    }
                }
                else
                {
                    currentState = NemesisState.Search;
                }
                break;
                
            case NemesisState.Search:
                agent.speed = walkSpeed;
                agent.SetDestination(lastKnownPosition);
                break;
                
            case NemesisState.Attack:
            case NemesisState.BreakWall:
                break;
        }
    }
    
    System.Collections.IEnumerator AttackSequence()
    {
        agent.ResetPath();
        
        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }
        
        SetAnimBool(attackHash, true);
        PlayAttackSound();
        
        yield return new WaitForSeconds(attackDuration * 0.4f);
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange * attackRangeMultiplier)
        {
            DealDamageToTarget();
        }
        
        yield return new WaitForSeconds(attackDuration * 0.6f);
        
        SetAnimBool(attackHash, false);
        yield return new WaitForSeconds(attackCooldown);
        
        currentState = NemesisState.Chase;
    }
    
    void DealDamageToTarget()
    {
        if (currentTarget == null) return;
        
        if (currentTarget.CompareTag("Player1") || currentTarget.CompareTag("Player2"))
        {
            var health = currentTarget.GetComponent<PlayerHealth>();
            if (health) health.TakeDamage(attackDamage);
        }
        else if (currentTarget.CompareTag("NPC"))
        {
            var health = currentTarget.GetComponent<NPCHealth>();
            if (health) health.TakeDamage(attackDamage);
        }
    }
    
    void UpdateAnimation()
    {
        bool moving = agent.velocity.magnitude > 0.1f;
        SetAnimBool(walkHash, moving);
    }
    
    void PlayDetectionSound()
    {
        if (detectionSounds != null && detectionSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = detectionSounds[Random.Range(0, detectionSounds.Length)];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
    
    void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    public NemesisState GetCurrentState() => currentState;
    public Transform GetCurrentTarget() => currentTarget;
    public void ForceAlert(Vector3 pos)
    {
        currentState = NemesisState.Search;
        lastKnownPosition = pos;
        lastDetectionTime = Time.time;
    }
    
    
    
    
    
    
    
    
    
    public void ResetChaseMusic()
    {
        hasTriggeredChaseMusic = false;
        
        
        if (chaseMusicController != null)
        {
            chaseMusicController.ResetChaseMusic();
        }
        else if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }
        

    }
    
    
    
    
    public void ResetAIState()
    {
        
        currentState = NemesisState.Idle;
        currentTarget = null;
        lastKnownPosition = Vector3.zero;
        searchTimer = 0f;
        attackCooldown = 0f;
        
        
        hasTriggeredChaseMusic = false;
        
        
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = true;
        }
        
        
        if (detectionHelper != null)
        {
            detectionHelper.ResetDetectionState();
        }
        
        
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        
        ResetChaseMusic();
        

    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        if (currentTarget != null)
        {
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
