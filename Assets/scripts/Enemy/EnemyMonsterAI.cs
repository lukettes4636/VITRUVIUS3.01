using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMonsterAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    private Coroutine footstepCoroutine = null;
    private AudioClip currentFootstepClip = null;
    private float currentFootstepInterval = 0f;

    [Header("Deteccion de jugadores")]
    public Transform[] playerTargets;

    [Header("Deteccion de NPCs")]
    [Tooltip("NPCs que el enemigo puede detectar y atacar")]
    public Transform[] npcTargets;
    private Transform currentPlayer;

    [Header("Parametros de movimiento")]
    public float lookRadius = 15f;

    public float audioDetectionRadius = 10f;
    public float viewAngle = 90f;
    public float attackRange = 2.2f;
    public float crawlSpeed = 1.2f;
    public float walkSpeed = 2.5f;
    public float rotationSpeed = 8f;

    public float attackStopDistanceOffset = 0.5f;
    public float patrolStopDistance = 0.1f;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    private int patrolIndex = 0;
    private bool isWaiting = false;

    [Header("Animaciones")]
    public float crawlToStateDuration = 1.1f;
    public float stateToCrawlDuration = 1.0f;
    public float roarDuration = 1.2f;
    public float attackDuration = 1f;
    public float attackCooldown = 0.7f;
    public string sleepAnimBool = "isSleeping";

    [Header("Vision")]
    public LayerMask visionBlockerLayer;
    public float raycastHeightOffset = 1.2f;
    public float targetRaycastHeightOffset = 1.0f;
    public Color debugRayColor = Color.cyan;
    public Color blockedRayColor = Color.red;

    [Header("Audio")]
    public AudioClip roarClip;
    public AudioClip attackClip;
    public AudioClip secondaryAttackClip;
    public AudioClip crawlFootstepClip;
    public AudioClip walkFootstepClip;

    public float pitchVariance = 0.1f;

    [Header("Pisadas Programaticas")]
    public float crawlFootstepInterval = 0.5f;
    public float walkFootstepInterval = 0.35f;

    [Header("Damage Settings")]
    public GameObject rightHandCollider;
    public GameObject leftHandCollider;

    [Header("Destruccion de Pared")]
    public LayerMask destructibleWallLayer;
    public float wallDetectionDistance = 3.0f;
    public AudioClip wallBreakSound;
    private GameObject currentWallTarget = null;

    [Header("Optimizacion de Transiciones")]

    public float transitionDelay = 0.15f;
    [Tooltip("Velocidad minima para considerar que esta en movimiento")]
    public float movementThreshold = 0.05f;

    [Header("Camera System")]
    public EnemyCameraController enemyCameraController;

    
    [Header("Shader FX - Roar")]
    [Tooltip("Arrastra aqu el Material creado con SG_EnemyRoar (Mat_Roar)")]
    public Material roarMaterial;

    [Tooltip("Intensidad mxima de la distorsin.")]
    [Range(0, 0.1f)]
    public float maxRoarDistortion = 0.03f;
    

    private float halfViewAngle;
    private bool isAttacking = false;
    private bool isRising = false;
    private bool isRoaring = false;
    private bool playerVisible = false;
    private bool lastPlayerVisible = false;
    private bool returningToCrawl = false;
    private bool hasAwakened = false;
    private bool isTransitioning = false;
    private bool hasShownFirstDetection = false;
    public float reDetectionCooldown = 2f;
    private float lastReDetectionTime = -999f;

    
    private int _roarIntensityID;
    private int _isActiveID;
    private Coroutine roarVisualCoroutine = null;

    private enum State { Sleeping, Patrol, Chasing, Attacking, Rising, Roaring, Dead, ReturningToCrawl, Investigating }
    private State currentState = State.Sleeping;
    private State previousState = State.Sleeping;

    private Vector3 lastKnownPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.playOnAwake = false;
        }

        
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 30f; 
        audioSource.dopplerLevel = 0f; 

        halfViewAngle = viewAngle / 2f;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            agent.stoppingDistance = patrolStopDistance;
            agent.speed = crawlSpeed;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
        }


        anim.SetBool(sleepAnimBool, true);
        anim.SetBool("isCrawling", false);
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);

        DisableAllHitboxes();


        if (enemyCameraController == null)
            enemyCameraController = FindObjectOfType<EnemyCameraController>();

        
        if (roarMaterial != null)
        {
            _roarIntensityID = Shader.PropertyToID("_RoarIntensity");
            _isActiveID = Shader.PropertyToID("_IsActive");

            
            roarMaterial.SetFloat(_isActiveID, 0f);
            roarMaterial.SetFloat(_roarIntensityID, 0f);
        }
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        DetectPlayers();

        if (currentState == State.Sleeping)
        {
            HandleSleepingState();
            return;
        }


        if (currentState == State.Rising ||
            currentState == State.Roaring ||
            currentState == State.ReturningToCrawl)
        {
            return;
        }


        if (currentState == State.Attacking)
        {
            if (currentPlayer != null)
            {
                RotateToTarget(currentPlayer.position);
            }
            return;
        }


        if (currentState == State.Investigating)
        {
            Investigate();
            return;
        }


        if (!playerVisible || currentPlayer == null)
        {
            if (hasAwakened)
            {

                if (currentState == State.Chasing && !returningToCrawl && !isTransitioning)
                {
                    StartCoroutine(ReturnToCrawl());
                    return;
                }

                Patrol();
            }
            return;
        }


        float distance = Vector3.Distance(transform.position, currentPlayer.position);
        RotateToTarget(currentPlayer.position);


        if (currentState == State.Patrol)
        {
            StartCoroutine(RiseAndRoar());
            return;
        }


        if (currentState != State.Chasing)
        {
            return;
        }

        agent.stoppingDistance = attackRange - attackStopDistanceOffset;


        if (IsWallBetweenPlayerAndMe(currentPlayer))
        {
            StartCoroutine(AttackWallCycle());
            return;
        }


        if (distance > agent.stoppingDistance + 0.1f)
        {
            ChasePlayer();
        }
        else
        {
            StartCoroutine(AttackCycle());
        }
    }





    void HandleSleepingState()
    {
        if (currentPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, currentPlayer.position);

            if (distance <= audioDetectionRadius)
            {
                hasAwakened = true;
                anim.SetBool(sleepAnimBool, false);

                agent.updateRotation = true;

                if (patrolPoints.Length > 0)
                {
                    SetNextPatrol();
                }

                StartCoroutine(RiseAndRoar());
            }
        }
    }

    void Patrol()
    {
        if (currentState != State.Patrol)
        {
            ChangeState(State.Patrol);
            agent.stoppingDistance = patrolStopDistance;
            agent.speed = crawlSpeed;

            if (patrolPoints.Length > 0 && !agent.hasPath)
            {
                SetNextPatrol();
            }
        }

        if (patrolPoints.Length == 0)
        {
            StopMovementCompletely();
            return;
        }


        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            if (!isWaiting)
            {
                StopMovementCompletely();
                StartCoroutine(WaitAtPatrol());
            }
        }
        else
        {

            if (agent.isStopped)
            {
                agent.isStopped = false;
            }


            bool isMoving = agent.velocity.sqrMagnitude > movementThreshold;
            anim.SetBool("isCrawling", isMoving);
            anim.SetBool("isWalking", false);

            if (isMoving)
            {
                UpdateFootsteps(crawlFootstepClip, crawlFootstepInterval);
            }
        }
    }

    IEnumerator WaitAtPatrol()
    {
        isWaiting = true;
        anim.SetBool("isCrawling", false);

        yield return new WaitForSeconds(patrolWaitTime);

        SetNextPatrol();


        yield return new WaitForSeconds(transitionDelay);

        agent.isStopped = false;
        isWaiting = false;
    }

    void SetNextPatrol()
    {
        if (patrolPoints.Length == 0) return;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[patrolIndex].position);
    }





    void DetectPlayers()
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        
        foreach (Transform player in playerTargets)
        {
            if (player == null) continue;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && health.IsDead)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, player.position);

            bool canSeePlayer = dist <= lookRadius && IsPlayerInViewCone(player) && HasLineOfSight(player);
            bool canHearPlayer = dist <= audioDetectionRadius;

            
            PlayerNoiseEmitter noiseEmitter = player.GetComponent<PlayerNoiseEmitter>();
            if (noiseEmitter != null && canHearPlayer && dist <= noiseEmitter.currentNoiseRadius)
            {
                canHearPlayer = true;
            }

            if (currentState == State.Sleeping)
            {
                if (!canHearPlayer) continue;
            }
            else
            {
                if (!canSeePlayer && !canHearPlayer) continue;
            }

            if (dist < minDist)
            {
                minDist = dist;
                nearest = player;
            }
        }

        
        foreach (Transform npc in npcTargets)
        {
            if (npc == null) continue;

            NPCHealth npcHealth = npc.GetComponent<NPCHealth>();
            if (npcHealth != null && npcHealth.IsDead)
            {
                continue;
            }

            float dist = Vector3.Distance(transform.position, npc.position);

            bool canSeeNPC = dist <= lookRadius && IsPlayerInViewCone(npc) && HasLineOfSight(npc);
            bool canHearNPC = dist <= audioDetectionRadius;

            
            NPCNoiseEmitter npcNoiseEmitter = npc.GetComponent<NPCNoiseEmitter>();
            if (npcNoiseEmitter != null && canHearNPC && dist <= npcNoiseEmitter.currentNoiseRadius)
            {
                canHearNPC = true;
            }

            if (currentState == State.Sleeping)
            {
                if (!canHearNPC) continue;
            }
            else
            {
                if (!canSeeNPC && !canHearNPC) continue;
            }

            if (dist < minDist)
            {
                minDist = dist;
                nearest = npc;
            }
        }

        if (nearest != null)
        {
            lastKnownPosition = nearest.position;
        }

        bool hadPlayer = playerVisible;
        currentPlayer = nearest;
        playerVisible = nearest != null;

        if (playerVisible)
        {
            if (currentState == State.Investigating)
            {
                ChangeState(State.Chasing);
            }
        }

        if (playerVisible && !hadPlayer)
        {
            if (!hasShownFirstDetection)
            {
                hasShownFirstDetection = true;
                lastReDetectionTime = Time.time;
            }
            else if (Time.time - lastReDetectionTime > reDetectionCooldown && currentPlayer != null)
            {
                DialogueManager.ShowEnemyDetectedAgainDialogue(currentPlayer.gameObject);
                lastReDetectionTime = Time.time;
            }
        }


        if (!playerVisible && hadPlayer && hasAwakened &&
            !returningToCrawl && !isTransitioning &&
            (currentState == State.Chasing || currentState == State.Attacking))
        {
            ChangeState(State.Investigating);
        }
    }

    bool IsPlayerInViewCone(Transform player)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        return Vector3.Angle(transform.forward, dir) < halfViewAngle;
    }

    bool HasLineOfSight(Transform player)
    {
        Vector3 origin = transform.position + Vector3.up * raycastHeightOffset;
        Vector3 target = player.position + Vector3.up * targetRaycastHeightOffset;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, visionBlockerLayer))
        {
#if UNITY_EDITOR
            Debug.DrawLine(origin, hit.point, blockedRayColor, Time.deltaTime);
#endif
            return false;
        }

#if UNITY_EDITOR
        Debug.DrawLine(origin, target, debugRayColor, Time.deltaTime);
#endif
        return true;
    }

    bool IsWallBetweenPlayerAndMe(Transform targetPlayer)
    {
        if (targetPlayer == null) return false;

        Vector3 start = transform.position + Vector3.up * raycastHeightOffset;
        Vector3 end = targetPlayer.position + Vector3.up * targetRaycastHeightOffset;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        float effectiveDistance = Mathf.Min(distance, wallDetectionDistance);

        if (Physics.Raycast(start, direction, out RaycastHit hit, effectiveDistance, destructibleWallLayer))
        {
            if ((destructibleWallLayer & (1 << hit.collider.gameObject.layer)) != 0)
            {
                currentWallTarget = hit.collider.gameObject;
                return true;
            }
        }

        currentWallTarget = null;
        return false;
    }





    IEnumerator RiseAndRoar()
    {
        if (isRising || isRoaring || isTransitioning) yield break;
        if (currentState == State.Rising || currentState == State.Roaring) yield break;

        isTransitioning = true;
        ChangeState(State.Rising);
        isRising = true;


        if (enemyCameraController != null)
            enemyCameraController.StartTrackingEnemy(transform);

        StopMovementCompletely();
        ResetAllAnimations();

        anim.SetTrigger("GetUp");

        yield return new WaitForSeconds(crawlToStateDuration);

        isRising = false;
        isRoaring = true;
        ChangeState(State.Roaring);

        PlayRoarSound();
        anim.SetTrigger("Roar");

        
        

        yield return new WaitForSeconds(roarDuration);

        isRoaring = false;
        isTransitioning = false;
        ChangeState(State.Chasing);


        yield return new WaitForSeconds(transitionDelay);

        agent.speed = walkSpeed;
        agent.isStopped = false;
        anim.SetBool("isWalking", true);

        UpdateFootsteps(walkFootstepClip, walkFootstepInterval);
        if (!hasShownFirstDetection)
        {
            hasShownFirstDetection = true;
            lastReDetectionTime = Time.time;
        }
    }

    void ChasePlayer()
    {
        if (currentPlayer == null) return;
        if (currentState != State.Chasing) return;

        agent.speed = walkSpeed;
        agent.SetDestination(currentPlayer.position);

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }


        bool isMoving = agent.velocity.sqrMagnitude > movementThreshold;
        anim.SetBool("isWalking", isMoving);
        anim.SetBool("isCrawling", false);

        if (isMoving)
        {
            UpdateFootsteps(walkFootstepClip, walkFootstepInterval);
        }
    }

    void Investigate()
    {
        if (currentState != State.Investigating) return;

        agent.speed = walkSpeed;
        agent.SetDestination(lastKnownPosition);

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        bool isMoving = agent.velocity.sqrMagnitude > movementThreshold;
        anim.SetBool("isWalking", isMoving);
        anim.SetBool("isCrawling", false);

        if (isMoving)
        {
            UpdateFootsteps(walkFootstepClip, walkFootstepInterval);
        }

        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            DialogueManager.ShowEnemyChaseEndedDialogue();
            StartCoroutine(ReturnToCrawl());
        }
    }

    IEnumerator AttackCycle()
    {
        if (isAttacking || isTransitioning) yield break;
        if (currentState == State.Attacking) yield break;

        isTransitioning = true;
        ChangeState(State.Attacking);
        isAttacking = true;

        StopMovementCompletely();

        anim.SetBool("isWalking", false);
        anim.SetBool("isCrawling", false);
        anim.SetBool("isAttacking", true);

        int rand = Random.Range(1, 4);
        anim.SetTrigger("Attack" + rand);

        yield return new WaitForSeconds(attackDuration);

        DisableAllHitboxes();

        anim.SetBool("isAttacking", false);
        isAttacking = false;
        isTransitioning = false;


        yield return null;

        if (playerVisible && currentPlayer != null)
        {
            ChangeState(State.Chasing);

            yield return new WaitForSeconds(transitionDelay);

            agent.speed = walkSpeed;
            agent.isStopped = false;
            anim.SetBool("isWalking", true);

            UpdateFootsteps(walkFootstepClip, walkFootstepInterval);
        }
        else
        {
            if (!returningToCrawl && hasAwakened)
            {
                ChangeState(State.Investigating);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
    }

    IEnumerator AttackWallCycle()
    {
        if (currentWallTarget == null || isAttacking || isTransitioning) yield break;
        if (currentState == State.Attacking) yield break;

        GameObject wallToAttack = currentWallTarget;


        agent.speed = walkSpeed;
        agent.isStopped = false;

        Vector3 targetPos = wallToAttack.transform.position;
        Vector3 approachPos = targetPos - (targetPos - transform.position).normalized * 0.5f;

        agent.SetDestination(approachPos);

        anim.SetBool("isWalking", true);
        anim.SetBool("isCrawling", false);
        UpdateFootsteps(walkFootstepClip, walkFootstepInterval);

        float timeout = 10f;
        float elapsed = 0f;

        while (Vector3.Distance(transform.position, approachPos) > agent.stoppingDistance + 0.3f)
        {
            elapsed += Time.deltaTime;

            if (elapsed > timeout || currentPlayer == null || !IsWallBetweenPlayerAndMe(currentPlayer))
            {
                yield break;
            }

            yield return null;
        }


        isTransitioning = true;
        ChangeState(State.Attacking);
        isAttacking = true;

        StopMovementCompletely();
        RotateToTarget(wallToAttack.transform.position);

        anim.SetBool("isWalking", false);
        anim.SetBool("isCrawling", false);
        anim.SetBool("isAttacking", true);

        anim.SetTrigger("Attack3");

        yield return new WaitForSeconds(attackDuration);

        DisableAllHitboxes();

        anim.SetBool("isAttacking", false);
        isAttacking = false;
        isTransitioning = false;

        yield return null;

        if (playerVisible && currentPlayer != null)
        {
            ChangeState(State.Chasing);

            yield return new WaitForSeconds(transitionDelay);

            agent.speed = walkSpeed;
            agent.isStopped = false;
            anim.SetBool("isWalking", true);
            UpdateFootsteps(walkFootstepClip, walkFootstepInterval);
        }

        yield return new WaitForSeconds(attackCooldown);
    }

    IEnumerator ReturnToCrawl()
    {
        if (returningToCrawl || isTransitioning) yield break;

        isTransitioning = true;
        returningToCrawl = true;
        ChangeState(State.ReturningToCrawl);


        if (enemyCameraController != null)
            enemyCameraController.StopTrackingEnemy();

        StopMovementCompletely();
        ResetAllAnimations();

        anim.SetTrigger("ToCrawl");

        yield return new WaitForSeconds(stateToCrawlDuration);

        agent.stoppingDistance = patrolStopDistance;
        agent.speed = crawlSpeed;

        ChangeState(State.Patrol);

        SetNextPatrol();


        yield return new WaitForSeconds(transitionDelay);

        agent.isStopped = false;
        anim.SetBool("isCrawling", true);

        returningToCrawl = false;
        isTransitioning = false;

        UpdateFootsteps(crawlFootstepClip, crawlFootstepInterval);
    }





    void ChangeState(State newState)
    {
        if (currentState != newState)
        {
            previousState = currentState;
            currentState = newState;
        }
    }

    void StopMovementCompletely()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        StopFootsteps();
    }

    void ResetAllAnimations()
    {
        anim.SetBool("isCrawling", false);
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);
    }

    void RotateToTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }
    }





    private void DisableAllHitboxes()
    {
        DisableRightHand();
        DisableLeftHand();
    }

    public void EnableRightHand()
    {
        if (rightHandCollider != null) rightHandCollider.SetActive(true);
    }

    public void DisableRightHand()
    {
        if (rightHandCollider != null) rightHandCollider.SetActive(false);
    }

    public void EnableLeftHand()
    {
        if (leftHandCollider != null) leftHandCollider.SetActive(true);
    }

    public void DisableLeftHand()
    {
        if (leftHandCollider != null) leftHandCollider.SetActive(false);
    }

    public void PlayAttackSound()
    {
        if (audioSource != null)
        {
            audioSource.pitch = 1f;

            if (attackClip != null)
                audioSource.PlayOneShot(attackClip, 0.7f);

            if (secondaryAttackClip != null)
                audioSource.PlayOneShot(secondaryAttackClip, 0.7f);
        }
    }

    public void TryToDestroyWall()
    {
        if (currentWallTarget != null)
        {
            Wall_Destruction wallScript = currentWallTarget.GetComponent<Wall_Destruction>();

            if (wallScript != null)
            {
                Vector3 impactPoint = currentWallTarget.transform.position;
                Vector3 impactDirection = transform.forward;

                wallScript.Explode(impactPoint, impactDirection);
                DialogueManager.ShowEnemyWallBreakDialogue();

                if (audioSource != null && wallBreakSound != null)
                {
                    audioSource.PlayOneShot(wallBreakSound);
                }

                currentWallTarget = null;
            }
        }
    }

    public void OnEnemyDeath()
    {
        if (enemyCameraController != null)
            enemyCameraController.StopTrackingEnemy();

        ChangeState(State.Dead);
    }





    private IEnumerator PlayFootsteps(AudioClip clip, float interval)
    {

        while (true)
        {
            if (currentState == State.Dead)
            {

                footstepCoroutine = null;
                yield break;
            }

            if (agent.velocity.sqrMagnitude <= movementThreshold)
            {
                 
                 
                 footstepCoroutine = null;
                 yield break;
            }

            if (currentFootstepClip != clip)
            {

                footstepCoroutine = null;
                yield break;
            }

            if (audioSource != null && clip != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
                audioSource.PlayOneShot(clip, 0.7f);

            }
            else
            {

            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void UpdateFootsteps(AudioClip clip, float interval)
    {
        if (footstepCoroutine != null && currentFootstepClip == clip && currentFootstepInterval == interval)
        {
            return;
        }

        
        if (IsInvoking(nameof(StartFootstepsDelayed)) && currentFootstepClip == clip && currentFootstepInterval == interval)
        {
            return;
        }


        StopFootsteps();

        currentFootstepClip = clip;
        currentFootstepInterval = interval;


        Invoke(nameof(StartFootstepsDelayed), 0.15f);
    }

    private void StartFootstepsDelayed()
    {

        if (agent.velocity.sqrMagnitude > movementThreshold && currentFootstepClip != null)
        {
            footstepCoroutine = StartCoroutine(PlayFootsteps(currentFootstepClip, currentFootstepInterval));
        }
    }

    private void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }

        currentFootstepClip = null;
        CancelInvoke(nameof(StartFootstepsDelayed));
    }

    public void PlayRoarSound()
    {
        if (audioSource != null && roarClip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(roarClip, 0.7f);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        halfViewAngle = viewAngle / 2f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, audioDetectionRadius);
        Gizmos.color = Color.red;

        if (agent != null)
        {
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, attackRange * 0.7f);
        }

        Vector3 origin = transform.position + Vector3.up * 0.05f;
        Vector3 left = Quaternion.Euler(0, -halfViewAngle, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, halfViewAngle, 0) * transform.forward;
        Gizmos.color = debugRayColor;
        Gizmos.DrawRay(origin, left * lookRadius);
        Gizmos.DrawRay(origin, right * lookRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, wallDetectionDistance);
    }
#endif

    
    
    
    

    
    public void AE_StartRoarEffect()
    {
        if (roarMaterial == null) return;

        
        roarMaterial.SetFloat(_isActiveID, 1f);

        
        if (roarVisualCoroutine != null) StopCoroutine(roarVisualCoroutine);
        roarVisualCoroutine = StartCoroutine(RoarIntensityRoutine());
    }

    
    public void AE_StopRoarEffect()
    {
        if (roarMaterial == null) return;

        
        roarMaterial.SetFloat(_isActiveID, 0f);
        roarMaterial.SetFloat(_roarIntensityID, 0f);

        
        if (roarVisualCoroutine != null)
        {
            StopCoroutine(roarVisualCoroutine);
            roarVisualCoroutine = null;
        }
    }

    
    private IEnumerator RoarIntensityRoutine()
    {
        float timer = 0f;

        while (true) 
        {
            timer += Time.deltaTime;

            
            
            
            float pulse = Mathf.Abs(Mathf.Sin(timer * 10f));

            
            float currentIntensity = pulse * maxRoarDistortion;

            roarMaterial.SetFloat(_roarIntensityID, currentIntensity);

            yield return null;
        }
    }
}
