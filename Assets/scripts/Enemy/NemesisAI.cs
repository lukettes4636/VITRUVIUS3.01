using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class NemesisAI : MonoBehaviour
{
    [Header("Nemesis AI Configuration")]
    [Tooltip("Walking speed only - no crawling for this nemesis")]
    public float walkSpeed = 3.5f;
    public float chaseSpeed = 5f;
    public float rotationSpeed = 8f;
    
    [Header("Detection Settings")]
    public float detectionRadius = 25f;
    public float attackRange = 2.5f;
    public float soundDetectionRadius = 30f;
    public LayerMask detectionLayerMask;
    public LayerMask soundBlockerLayer;
    
    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 25;
    public float attackDuration = 0.8f;
    
    [Header("Targeting Priority")]
    [Tooltip("Higher priority targets will be focused first")]
    public float npcPriority = 2f;
    public float playerPriority = 1f;
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    public AudioClip[] detectionSounds;
    public AudioClip[] footstepSounds;
    
    [Header("References")]
    public Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    
    private Transform currentTarget;
    private Vector3 lastKnownPosition;
    private float lastDetectionTime;
    private bool isAlerted;
    private bool isAttacking;
    private bool canAttack = true;
    
    private enum TargetType { None, Player1, Player2, NPC }
    private TargetType currentTargetType;
    
    
    private readonly int walkHash = Animator.StringToHash("Walk");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int detectionHash = Animator.StringToHash("Detected");
    
    
    private List<Transform> detectedTargets = new List<Transform>();
    private float lastSoundCheckTime;
    private float soundCheckInterval = 0.2f; 
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        
        if (attackSounds == null) attackSounds = new AudioClip[0];
        if (detectionSounds == null) detectionSounds = new AudioClip[0];
        if (footstepSounds == null) footstepSounds = new AudioClip[0];
        
        ConfigureNavMeshAgent();
    }
    
    void Start()
    {
        isAlerted = false;
        currentTarget = null;
        currentTargetType = TargetType.None;
        
        if (animator != null)
        {
            animator.SetBool(walkHash, false);
            animator.SetBool(attackHash, false);
        }
    }
    
    void Update()
    {
        if (isAttacking) return;
        
        
        DetectTargetsImproved();
        
        if (currentTarget != null)
        {
            ChaseTarget();
        }
        else if (isAlerted)
        {
            SearchLastKnownPosition();
        }
        else
        {
            Patrol();
        }
        
        UpdateAnimation();
    }
    
    void ConfigureNavMeshAgent()
    {
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;
            agent.stoppingDistance = attackRange - 0.3f;
            agent.autoBraking = true;
            agent.updateRotation = true;
        }
    }
    
    void DetectTargetsImproved()
    {
        detectedTargets.Clear();
        Transform bestTarget = null;
        TargetType bestTargetType = TargetType.None;
        float bestPriorityScore = 0f;
        
        
        DetectPlayersImproved(ref bestTarget, ref bestTargetType, ref bestPriorityScore);
        
        
        DetectNPCsImproved(ref bestTarget, ref bestTargetType, ref bestPriorityScore);
        
        
        if (Time.time - lastSoundCheckTime > soundCheckInterval)
        {
            lastSoundCheckTime = Time.time;
            DetectSoundsImproved(ref bestTarget, ref bestTargetType, ref bestPriorityScore);
        }
        
        
        UpdateCurrentTarget(bestTarget, bestTargetType);
    }
    
    void DetectPlayersImproved(ref Transform bestTarget, ref TargetType bestTargetType, ref float bestPriorityScore)
    {
        
        List<GameObject> players = new List<GameObject>();
        
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        if (player1 != null) players.Add(player1);
        
        
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null) players.Add(player2);
        
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if ((obj.name.Contains("Player") || obj.tag.Contains("Player")) && !players.Contains(obj))
            {
                players.Add(obj);
            }
        }
        
        
        foreach (GameObject player in players)
        {
            if (player == null) continue;
            
            
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead) continue;
            
            
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > detectionRadius) continue;
            
            
            if (!HasLineOfSight(player.transform)) continue;
            
            
            float priorityScore = CalculateTargetPriority(player.transform, playerPriority, distance);
            
            if (priorityScore > bestPriorityScore)
            {
                bestPriorityScore = priorityScore;
                bestTarget = player.transform;
                bestTargetType = player.tag == "Player1" ? TargetType.Player1 : TargetType.Player2;
                detectedTargets.Add(player.transform);
            }
        }
    }
    
    void DetectNPCsImproved(ref Transform bestTarget, ref TargetType bestTargetType, ref float bestPriorityScore)
    {
        
        List<GameObject> npcs = new List<GameObject>();
        
        
        GameObject npc = GameObject.FindGameObjectWithTag("NPC");
        if (npc != null) npcs.Add(npc);
        
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if ((obj.name.Contains("NPC") || obj.tag.Contains("NPC")) && !npcs.Contains(obj))
            {
                npcs.Add(obj);
            }
        }
        
        
        foreach (GameObject npcObj in npcs)
        {
            if (npcObj == null) continue;
            
            
            var npcHealth = npcObj.GetComponent<NPCHealth>();
            if (npcHealth != null && npcHealth.IsDead) continue;
            
            
            float distance = Vector3.Distance(transform.position, npcObj.transform.position);
            if (distance > detectionRadius) continue;
            
            
            if (!HasLineOfSight(npcObj.transform)) continue;
            
            
            float priorityScore = CalculateTargetPriority(npcObj.transform, npcPriority, distance);
            
            if (priorityScore > bestPriorityScore)
            {
                bestPriorityScore = priorityScore;
                bestTarget = npcObj.transform;
                bestTargetType = TargetType.NPC;
                detectedTargets.Add(npcObj.transform);
            }
        }
    }
    
    void DetectSoundsImproved(ref Transform bestTarget, ref TargetType bestTargetType, ref float bestPriorityScore)
    {
        
        List<GameObject> soundSources = new List<GameObject>();
        
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        if (player1 != null) soundSources.Add(player1);
        
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null) soundSources.Add(player2);
        
        
        GameObject npc = GameObject.FindGameObjectWithTag("NPC");
        if (npc != null) soundSources.Add(npc);
        
        
        PlayerNoiseEmitter[] allNoiseEmitters = FindObjectsOfType<PlayerNoiseEmitter>();
        foreach (var emitter in allNoiseEmitters)
        {
            if (emitter != null && !soundSources.Contains(emitter.gameObject))
            {
                soundSources.Add(emitter.gameObject);
            }
        }
        
        NPCNoiseEmitter[] allNPCNoiseEmitters = FindObjectsOfType<NPCNoiseEmitter>();
        foreach (var emitter in allNPCNoiseEmitters)
        {
            if (emitter != null && !soundSources.Contains(emitter.gameObject))
            {
                soundSources.Add(emitter.gameObject);
            }
        }
        
        
        foreach (GameObject source in soundSources)
        {
            if (source == null) continue;
            
            
            var playerNoise = source.GetComponent<PlayerNoiseEmitter>();
            var npcNoise = source.GetComponent<NPCNoiseEmitter>();
            
            float noiseRadius = 0f;
            if (playerNoise != null) noiseRadius = playerNoise.currentNoiseRadius;
            if (npcNoise != null) noiseRadius = npcNoise.currentNoiseRadius;
            
            if (noiseRadius <= 0.1f) continue;
            
            
            float distance = Vector3.Distance(transform.position, source.transform.position);
            if (distance > soundDetectionRadius) continue;
            
            
            if (IsSoundObstructed(source.transform)) continue;
            
            
            float soundPriority = CalculateSoundPriority(noiseRadius, distance);
            
            
            TargetType soundTargetType = TargetType.None;
            if (source.CompareTag("Player1")) soundTargetType = TargetType.Player1;
            else if (source.CompareTag("Player2")) soundTargetType = TargetType.Player2;
            else if (source.CompareTag("NPC")) soundTargetType = TargetType.NPC;
            
            if (soundPriority > bestPriorityScore && soundTargetType != TargetType.None)
            {
                bestPriorityScore = soundPriority;
                bestTarget = source.transform;
                bestTargetType = soundTargetType;
            }
        }
    }
    
    bool HasLineOfSight(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        
        
        Vector3 rayOrigin = transform.position + Vector3.up * 1.7f;
        
        
        if (Physics.Raycast(rayOrigin, direction, distance, soundBlockerLayer))
        {
            return false; 
        }
        
        return true;
    }
    
    bool IsSoundObstructed(Transform soundSource)
    {
        Vector3 direction = (soundSource.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, soundSource.position);
        
        
        if (Physics.Raycast(transform.position + Vector3.up, direction, distance, soundBlockerLayer))
        {
            return true; 
        }
        
        return false;
    }
    
    float CalculateTargetPriority(Transform target, float basePriority, float distance)
    {
        
        float distanceFactor = 1f - (distance / detectionRadius);
        float angleFactor = CalculateAngleFactor(target);
        
        return basePriority * distanceFactor * angleFactor * 10f;
    }
    
    float CalculateAngleFactor(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 forward = transform.forward;
        
        float angle = Vector3.Angle(forward, directionToTarget);
        
        
        if (angle < 45f) return 1.5f;
        if (angle < 90f) return 1.2f;
        if (angle < 135f) return 0.8f;
        return 0.5f;
    }
    
    float CalculateSoundPriority(float noiseRadius, float distance)
    {
        if (distance > soundDetectionRadius) return 0f;
        
        
        float effectiveRadius = Mathf.Max(noiseRadius, 2f);
        float distanceFactor = 1f - (distance / soundDetectionRadius);
        float noiseFactor = Mathf.Clamp01(noiseRadius / 15f); 
        
        return (noiseFactor + distanceFactor) * 5f;
    }
    
    void UpdateCurrentTarget(Transform newTarget, TargetType newTargetType)
    {
        if (newTarget != null && newTarget != currentTarget)
        {
            
            currentTarget = newTarget;
            currentTargetType = newTargetType;
            lastKnownPosition = currentTarget.position;
            lastDetectionTime = Time.time;
            isAlerted = true;
            
            
            animator.SetTrigger(detectionHash);
            PlayDetectionSound();
            
            
            agent.speed = chaseSpeed;
            

        }
        else if (newTarget == null && currentTarget != null)
        {
            

            currentTarget = null;
            currentTargetType = TargetType.None;
        }
    }
    
    void ChaseTarget()
    {
        if (currentTarget == null) return;
        
        
        lastKnownPosition = currentTarget.position;
        lastDetectionTime = Time.time;
        
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        
        if (distanceToTarget <= attackRange && canAttack)
        {
            StartCoroutine(AttackTargetImproved());
        }
        else if (distanceToTarget > attackRange)
        {
            
            agent.SetDestination(currentTarget.position);
            
            
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
    
    System.Collections.IEnumerator AttackTargetImproved()
    {
        isAttacking = true;
        canAttack = false;
        
        
        agent.ResetPath();
        
        
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
            }
        }
        
        
        animator.SetBool(attackHash, true);
        
        
        PlayAttackSound();
        
        
        yield return new WaitForSeconds(attackDuration * 0.3f);
        
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange * 1.2f)
        {
            DealDamageToTargetImproved();
        }
        
        
        yield return new WaitForSeconds(attackDuration * 0.7f);
        
        
        animator.SetBool(attackHash, false);
        isAttacking = false;
        
        
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    void DealDamageToTargetImproved()
    {
        if (currentTarget == null) return;
        

        
        switch (currentTargetType)
        {
            case TargetType.Player1:
            case TargetType.Player2:
                var playerHealth = currentTarget.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(attackDamage);

                }
                else
                {

                }
                break;
                
            case TargetType.NPC:
                var npcHealth = currentTarget.GetComponent<NPCHealth>();
                if (npcHealth != null && !npcHealth.IsDead)
                {
                    npcHealth.TakeDamage(attackDamage);

                }
                else
                {

                }
                break;
                
            default:

                break;
        }
    }
    
    void SearchLastKnownPosition()
    {
        if (Vector3.Distance(transform.position, lastKnownPosition) > 2f)
        {
            agent.SetDestination(lastKnownPosition);
        }
        else
        {
            
            isAlerted = false;
            agent.speed = walkSpeed;

        }
    }
    
    void Patrol()
    {
        agent.speed = walkSpeed;
        
        
        if (agent.remainingDistance < 0.5f || !agent.hasPath)
        {
            
            Vector3 randomDirection = Random.insideUnitSphere * 15f;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
    
    void UpdateAnimation()
    {
        
        bool isMoving = agent.velocity.magnitude > 0.1f && !isAttacking;
        animator.SetBool(walkHash, isMoving);
        animator.SetBool(attackHash, isAttacking);
    }
    
    void PlayDetectionSound()
    {
        if (detectionSounds != null && detectionSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = detectionSounds[Random.Range(0, detectionSounds.Length)];
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
    
    void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
    
    
    public void SetAlerted(bool alerted)
    {
        isAlerted = alerted;
        if (alerted)
        {
            agent.speed = chaseSpeed;
        }
        else
        {
            agent.speed = walkSpeed;
        }
    }
    
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public bool IsAlerted()
    {
        return isAlerted;
    }
    
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundDetectionRadius);
        
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
#endif
}
