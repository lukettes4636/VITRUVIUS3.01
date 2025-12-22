using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemySenses), typeof(EnemyMotor), typeof(EnemyVisuals))]
public class EnemyBrain : MonoBehaviour
{
    private EnemySenses senses;
    private EnemyMotor motor;
    private EnemyVisuals visuals;
    private EnemyCameraController cameraController;

    [Header("Configuracion de Estado Inicial")]
    public InitialState initialState = InitialState.Sleeping;
    public enum InitialState { Sleeping, Eating, Patrol }

    [Header("Velocidades")]
    public float crawlSpeed = 1.2f;
    public float walkSpeed = 2.5f;
    public float investigationSpeed = 1.8f;

    [Header("Combate")]
    [Tooltip("Rango de ataque - distancia a la que puede atacar")]
    public float attackRange = 2.2f;
    [Tooltip("Tiempo de espera despues de terminar un ataque")]
    public float attackCooldown = 0.5f;
    public int attackDamage = 25;
    [Tooltip("Rango de deteccion para verificar si hay objetivo cerca antes de atacar")]
    public float detectionRange = 2.5f;
    [Tooltip("Tiempo que mantiene el foco en el objetivo tras golpear")]
    public float postAttackFocusTime = 1.5f;
    [Header("Acercamiento al Ataque")]
    [Tooltip("Distancia de parada al acercarse para atacar")]
    public float attackApproachStoppingDistance = 0.75f;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1f;
    private int patrolIndex = 0;

    [Header("Investigacion")]
    [Tooltip("Tiempo que el enemigo investiga en un punto sin recibir sonido")]
    public float investigationDuration = 3.0f;

    private float lastWallCheckTime = 0f;
    private float wallCheckInterval = 0.3f;

    private enum State { Sleeping, Eating, Patrol, Investigating, Chasing, Attacking, Transitioning, Dead }
    [SerializeField] private State currentState;

    private bool isWaitingAtPatrol = false;
    private bool hasShownFirstDetection = false;
    private float lastReDetectionTime = -999f;
    private Vector3 preInvestigatePosition;
    private bool isInvestigatingObjectNoise = false;
    private Coroutine investigationCoroutine;
    private float investigationTimer = 0f;
    private bool hasOtherTargetsInRange = false;
    private bool isInvestigatingStanding = false; 

    public Transform GetCurrentTarget()
    {
        if (senses.CurrentPlayer != null) return senses.CurrentPlayer;
        if (senses.CurrentNPCTarget != null) return senses.CurrentNPCTarget;
        return senses.CurrentNoisyObject;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.05f, attackApproachStoppingDistance));
    }
    void Start()
    {
        senses = GetComponent<EnemySenses>();
        motor = GetComponent<EnemyMotor>();
        visuals = GetComponent<EnemyVisuals>();
        cameraController = FindObjectOfType<EnemyCameraController>();

        SetupInitialState();
    }

    void SetupInitialState()
    {
        visuals.UpdateAnimationState(false);

        switch (initialState)
        {
            case InitialState.Sleeping:
                currentState = State.Sleeping;
                visuals.SetPassiveState(0);
                break;
            case InitialState.Eating:
                currentState = State.Eating;
                visuals.SetPassiveState(1);
                break;
            case InitialState.Patrol:
                currentState = State.Patrol;
                visuals.SetPassiveState(0);
                
                GoToNextPatrolPoint();
                break;
        }
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        senses.Tick();

        
        if (currentState == State.Sleeping || currentState == State.Eating)
        {
            if (senses.HasTargetOfInterest)
            {
                WakeUp();
            }
            return;
        }

        if (currentState == State.Transitioning || currentState == State.Attacking) return;

        switch (currentState)
        {
            case State.Chasing:
                HandleChasing();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Investigating:
                HandleInvestigating();
                break;
        }

        HandleDialogueFeedback();
    }

    void HandleChasing()
    {
        Transform currentTarget = GetCurrentTarget();

        
        if (currentTarget != null)
        {
            PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
            NPCHealth npcHealth = currentTarget.GetComponent<NPCHealth>();

            
            float distCT = Vector3.Distance(transform.position, currentTarget.position);
            if (distCT <= detectionRange)
            {
                senses.HasTargetOfInterest = true;
                senses.TargetPositionOfInterest = currentTarget.position;
            }

            if (senses.CurrentNoisyObject != null && currentTarget == senses.CurrentNoisyObject)
            {
                
                if (senses.CurrentNoisyObject == null || !senses.objectNoiseDetection.HasNoisyObjectNearby())
                {
                    
                    senses.ForgetTarget();
                    if (currentState != State.Investigating)
                    {
                        StartCoroutine(InvestigateRoutine(senses.TargetPositionOfInterest));
                    }
                    return;
                }

                
                senses.TargetPositionOfInterest = senses.CurrentNoisyObject.position;
                
                motor.MoveTo(senses.TargetPositionOfInterest, crawlSpeed, 0.2f);
                visuals.UpdateAnimationState(true);

                
                if (motor.GetRemainingDistance() <= 0.3f)
                {
                    if (currentState != State.Investigating)
                    {
                        StartCoroutine(InvestigateObjectRoutine());
                    }
                }
                return;
            }

            bool targetIsDead = (playerHealth != null && playerHealth.IsDead) || (npcHealth != null && npcHealth.IsDead);

            if (targetIsDead)
            {
                
                CheckForOtherTargetsInRange();
                
                if (hasOtherTargetsInRange)
                {
                    

                    senses.ForgetTarget();
                    SelectClosestAliveTarget();
                    if (senses.HasTargetOfInterest)
                    {
                        
                        if (senses.CurrentNoisyObject != null)
                        {
                            StartCoroutine(InvestigateObjectRoutine());
                        }
                        else
                        {
                            
                            currentState = State.Chasing;
                        }
                    }
                }
                else
                {
                    

                    senses.ForgetTarget();
                    StartCoroutine(InvestigateAfterKillRoutine());
                }
                return;
            }
        }

        
        if (Time.time - lastWallCheckTime > wallCheckInterval)
        {
            lastWallCheckTime = Time.time;
            if (senses.CheckForWallInFront() || (senses.HasTargetOfInterest && senses.CheckWallInPathToTarget()))
            {
                StartCoroutine(AttackWallRoutine(senses.CurrentWallTarget));
                return;
            }
        }

        
        if (senses.HasTargetOfInterest)
        {
            SelectClosestAliveTarget();
            
            
            if (senses.CurrentNoisyObject != null && senses.CurrentPlayer == null && senses.CurrentNPCTarget == null)
            {
                
                if (senses.CurrentNoisyObject == null || !senses.objectNoiseDetection.HasNoisyObjectNearby())
                {
                    
                    senses.ForgetTarget();
                    if (currentState != State.Investigating)
                    {
                        StartCoroutine(InvestigateRoutine(senses.TargetPositionOfInterest));
                    }
                    return;
                }

                
                senses.TargetPositionOfInterest = senses.CurrentNoisyObject.position;
                
                motor.MoveTo(senses.TargetPositionOfInterest, crawlSpeed, 0.2f);
                visuals.UpdateAnimationState(true);

                
                if (motor.GetRemainingDistance() <= 0.3f)
                {
                    if (currentState != State.Investigating)
                    {
                        StartCoroutine(InvestigateObjectRoutine());
                    }
                }
                return;
            }

            
            Transform liveTarget = GetCurrentTarget();
            if (liveTarget != null)
            {
                motor.MoveTo(liveTarget.position, walkSpeed, Mathf.Max(0.05f, attackApproachStoppingDistance));
            }
            else
            {
                motor.MoveTo(senses.TargetPositionOfInterest, walkSpeed, Mathf.Max(0.05f, attackApproachStoppingDistance));
            }
            visuals.UpdateAnimationState(false);

            
            bool isCharacter = senses.CurrentPlayer != null || senses.CurrentNPCTarget != null;
            if (isCharacter)
            {
                Transform actualTarget = GetCurrentTarget();
                if (actualTarget != null)
                {
                    
                    float distanceToTarget = Vector3.Distance(transform.position, actualTarget.position);
                    
                    
                    bool isTargetMakingNoise = false;
                    PlayerNoiseEmitter playerNoise = actualTarget.GetComponent<PlayerNoiseEmitter>();
                    NPCNoiseEmitter npcNoise = actualTarget.GetComponent<NPCNoiseEmitter>();
                    
                    if (playerNoise != null)
                    {
                        isTargetMakingNoise = playerNoise.currentNoiseRadius > playerNoise.idleNoiseRadius + 0.1f;
                    }
                    else if (npcNoise != null)
                    {
                        isTargetMakingNoise = npcNoise.currentNoiseRadius > npcNoise.idleNoiseRadius + 0.1f;
                    }
                    
                    
                    if (distanceToTarget <= attackRange && !senses.CheckForWallInFront())
                    {
                        StartCoroutine(AttackTargetRoutine());
                    }
                }
            }
        }
        else
        {
            
            
            Transform liveTarget = GetCurrentTarget();
            if (liveTarget != null)
            {
                senses.HasTargetOfInterest = true;
                senses.TargetPositionOfInterest = liveTarget.position;
                currentState = State.Chasing;
            }
            else if (currentState == State.Chasing)
            {
                
                senses.ForgetTarget();
                StartCoroutine(ReturnToPatrolRoutine());
            }
            else if (currentState != State.Investigating && senses.TargetPositionOfInterest != Vector3.zero)
            {
                StartCoroutine(InvestigateRoutine(senses.TargetPositionOfInterest));
            }
        }
    }

    void HandlePatrol()
    {
        
        if (senses.HasTargetOfInterest)
        {
            currentState = State.Transitioning;

            
            if (senses.CurrentNoisyObject != null && senses.CurrentPlayer == null && senses.CurrentNPCTarget == null)
            {
                StartCoroutine(WakeUpQuietRoutine());
            }
            else
            {
                
                StartCoroutine(WakeUpAndRoarRoutine());
            }
            return;
        }

        
        if (patrolPoints.Length == 0) return;

        if (motor.GetRemainingDistance() <= 0.2f)
        {
            if (!isWaitingAtPatrol)
            {
                StartCoroutine(PatrolWaitRoutine());
            }
        }
        else
        {
            visuals.UpdateAnimationState(true);
        }
    }

    void HandleInvestigating()
    {
        
        if (senses.HasTargetOfInterest)
        {
            if (investigationCoroutine != null)
            {
                StopCoroutine(investigationCoroutine);
                investigationCoroutine = null;
            }
            visuals.SetInvestigatingMode(false);
            
            
            if (senses.CurrentPlayer != null || senses.CurrentNPCTarget != null)
            {
                
                if (!isInvestigatingStanding)
                {
                    StartCoroutine(ReactToPlayerNoiseWhileInvestigating());
                }
                else
                {
                    
                    currentState = State.Chasing;
                }
            }
            
            else if (senses.CurrentNoisyObject != null)
            {
                
                if (isInvestigatingStanding)
                {
                    
                    currentState = State.Chasing;
                }
                else
                {
                    
                    currentState = State.Chasing;
                }
            }
        }
    }

    IEnumerator ReactToPlayerNoiseWhileInvestigating()
    {
        currentState = State.Transitioning;
        motor.Stop();
        visuals.SetInvestigatingMode(false);
        
        
        if (!isInvestigatingStanding)
        {
        visuals.TriggerGetUp();
        {
            float t = 0f;
            while (!visuals.AnimFinishedReceived && t < 2.5f) { t += Time.deltaTime; yield return null; }
        }
        }
        
        
        visuals.TriggerRoar();
        visuals.PlayRoarSound();
        {
            float t = 0f;
            while (!visuals.AnimFinishedReceived && t < 3.0f) { t += Time.deltaTime; yield return null; }
        }
        
        
        isInvestigatingStanding = true;
        currentState = State.Chasing;
    }

    void CheckForOtherTargetsInRange()
    {
        hasOtherTargetsInRange = false;

        
        foreach (Transform player in senses.playerTargets)
        {
            if (player == null) continue;
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsDead)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= senses.maxHearingDistance)
                {
                    hasOtherTargetsInRange = true;
                    return;
                }
            }
        }

        
        foreach (Transform npc in senses.npcTargets)
        {
            if (npc == null) continue;
            NPCHealth health = npc.GetComponent<NPCHealth>();
            if (health != null && !health.IsDead)
            {
                float distance = Vector3.Distance(transform.position, npc.position);
                if (distance <= senses.maxHearingDistance)
                {
                    hasOtherTargetsInRange = true;
                    return;
                }
            }
        }

        
        if (senses.objectNoiseDetection != null && senses.objectNoiseDetection.HasNoisyObjectNearby())
        {
            hasOtherTargetsInRange = true;
        }
    }

    IEnumerator WakeUpAndRoarRoutine()
    {
        currentState = State.Transitioning;
        motor.Stop();
        motor.SetAutoRotation(false);
        if (cameraController) cameraController.StartTrackingEnemy(transform);

        visuals.TriggerGetUp();
        {
            float t = 0f;
            while (!visuals.AnimFinishedReceived && t < 2.5f) { t += Time.deltaTime; yield return null; }
        }

        
        float confirmT = 0f;
        while (visuals.IsCrawlingAnim() && confirmT < 1.0f) { confirmT += Time.deltaTime; yield return null; }
        visuals.TriggerRoar();
        visuals.PlayRoarSound();
        {
            float t = 0f;
            while (!visuals.AnimFinishedReceived && t < 3.0f) { t += Time.deltaTime; yield return null; }
        }

        currentState = State.Chasing;
    }

    IEnumerator WakeUpQuietRoutine()
    {
        currentState = State.Transitioning;
        motor.Stop();
        motor.SetAutoRotation(false);
        visuals.UpdateAnimationState(true);

        preInvestigatePosition = transform.position;
        isInvestigatingObjectNoise = true;

        
        yield return StartCoroutine(InvestigateObjectRoutine());
    }

    IEnumerator InvestigateObjectRoutine()
    {
        currentState = State.Investigating;
        isInvestigatingStanding = false; 
        
        investigationCoroutine = StartCoroutine(InvestigateObjectRoutineInternal());
        yield return investigationCoroutine;
    }

    IEnumerator InvestigateObjectRoutineInternal()
    {
        Vector3 investigationPosition = senses.TargetPositionOfInterest;
        if (senses.CurrentNoisyObject != null)
        {
            investigationPosition = senses.CurrentNoisyObject.position;
        }
        
        motor.MoveTo(investigationPosition, crawlSpeed, 0.2f);

        float timeoutTimer = 0f;
        while (motor.GetRemainingDistance() > 0.3f && timeoutTimer < 7.0f)
        {
            timeoutTimer += Time.deltaTime;
            visuals.UpdateAnimationState(true);
            
            
            if (senses.CurrentNoisyObject == null || !senses.objectNoiseDetection.HasNoisyObjectNearby())
            {
                senses.ForgetTarget();
            }
            
            
            if (senses.HasTargetOfInterest)
            {
                if (senses.CurrentPlayer != null || senses.CurrentNPCTarget != null)
                {
                    
                    yield break;
                }
                else if (senses.CurrentNoisyObject != null)
                {
                    
                    yield break;
                }
            }
            
            yield return null;
        }

        
        motor.Stop();
        visuals.UpdateAnimationState(true);
        visuals.SetInvestigatingMode(true);
        
        investigationTimer = 0f;
        while (investigationTimer < investigationDuration)
        {
            investigationTimer += Time.deltaTime;
            
            
            if (senses.HasTargetOfInterest)
            {
                visuals.SetInvestigatingMode(false);
                yield break;
            }
            
            yield return null;
        }

        
        visuals.SetInvestigatingMode(false);
        senses.ForgetTarget();
        
        if (isInvestigatingObjectNoise)
        {
            senses.IgnoreCurrentNoisyObjectFor(8f);
            yield return StartCoroutine(ReturnToPreviousSpotRoutine());
        }
        else
        {
            yield return StartCoroutine(ReturnToPatrolRoutine());
        }

        isInvestigatingObjectNoise = false;
        isInvestigatingStanding = false;
    }

    IEnumerator InvestigateRoutine(Vector3 targetPosition)
    {
        currentState = State.Investigating;
        
        
        if (!isInvestigatingStanding)
        {
            isInvestigatingStanding = false; 
        }
        
        investigationCoroutine = StartCoroutine(InvestigateRoutineInternal(targetPosition));
        yield return investigationCoroutine;
    }

    IEnumerator InvestigateRoutineInternal(Vector3 targetPosition)
    {
        
        float moveSpeed = isInvestigatingStanding ? walkSpeed : crawlSpeed;
        motor.MoveTo(targetPosition, moveSpeed, 0.2f);

        float timeoutTimer = 0f;
        while (motor.GetRemainingDistance() > 0.3f && timeoutTimer < 7.0f)
        {
            timeoutTimer += Time.deltaTime;
            
            visuals.UpdateAnimationState(!isInvestigatingStanding);
            
            
            if (senses.HasTargetOfInterest)
            {
                yield break;
            }
            
            yield return null;
        }

        
        motor.Stop();
        
        visuals.UpdateAnimationState(!isInvestigatingStanding);
        visuals.SetInvestigatingMode(true);
        
        investigationTimer = 0f;
        while (investigationTimer < investigationDuration)
        {
            investigationTimer += Time.deltaTime;
            
            
            if (senses.HasTargetOfInterest)
            {
                visuals.SetInvestigatingMode(false);
                yield break;
            }
            
            yield return null;
        }

        
        visuals.SetInvestigatingMode(false);
        senses.ForgetTarget();
        yield return StartCoroutine(ReturnToPatrolRoutine());
        
        isInvestigatingStanding = false;
    }

    IEnumerator InvestigateAfterKillRoutine()
    {
        currentState = State.Investigating;
        isInvestigatingStanding = false; 
        
        investigationCoroutine = StartCoroutine(InvestigateAfterKillRoutineInternal());
        yield return investigationCoroutine;
    }

    IEnumerator InvestigateAfterKillRoutineInternal()
    {
        motor.Stop();
        visuals.UpdateAnimationState(true);
        visuals.SetInvestigatingMode(true);
        
        investigationTimer = 0f;
        while (investigationTimer < investigationDuration)
        {
            investigationTimer += Time.deltaTime;
            
            
            if (senses.HasTargetOfInterest)
            {
                visuals.SetInvestigatingMode(false);
                yield break;
            }
            
            yield return null;
        }

        
        visuals.SetInvestigatingMode(false);
        yield return StartCoroutine(ReturnToPatrolRoutine());
        
        isInvestigatingStanding = false;
    }

    IEnumerator ReturnToPreviousSpotRoutine()
    {
        currentState = State.Transitioning;
        visuals.UpdateAnimationState(true);
        motor.MoveTo(preInvestigatePosition, crawlSpeed, 0.2f);

        float timer = 0f;
        while (motor.GetRemainingDistance() > 0.3f && timer < 6f)
        {
            timer += Time.deltaTime;
            visuals.UpdateAnimationState(true);
            yield return null;
        }

        currentState = State.Patrol;
        motor.SetAutoRotation(true);
        GoToNextPatrolPoint();
    }

    IEnumerator ReturnToPatrolRoutine()
    {
        currentState = State.Transitioning;
        if (cameraController) cameraController.StopTrackingEnemy();
        motor.Stop();

        visuals.TriggerToCrawl();
        {
            float t = 0f;
            while (!visuals.AnimFinishedReceived && t < 2.5f) { t += Time.deltaTime; yield return null; }
        }

        currentState = State.Patrol;
        motor.SetAutoRotation(true);
        GoToNextPatrolPoint();
    }

    IEnumerator AttackTargetRoutine()
    {
        currentState = State.Attacking;
        motor.Stop();
        motor.RotateTowards(senses.TargetPositionOfInterest);

        visuals.TriggerAttack(Random.Range(1, 4));

        yield return new WaitUntil(() => visuals.AnimImpactReceived || visuals.AnimFinishedReceived);

        
        Transform target = GetCurrentTarget();
        if (target != null)
        {
            PlayerHealth pHealth = target.GetComponent<PlayerHealth>();
            NPCHealth nHealth = target.GetComponent<NPCHealth>();
            if (pHealth != null) pHealth.TakeDamage(attackDamage);
            if (nHealth != null) nHealth.TakeDamage(attackDamage);
            
            Vector3 targetPos = target.position;
            senses.TargetPositionOfInterest = targetPos;
            senses.HasTargetOfInterest = true;
            if (pHealth != null) senses.SetPlayerTarget(target);
            else if (nHealth != null) senses.SetNPCTarget(target);
        }

        {
            float waitFinishT = 0f;
            while (!visuals.AnimFinishedReceived && waitFinishT < 3.0f) { waitFinishT += Time.deltaTime; yield return null; }
        }
        visuals.StopAttack();

        currentState = State.Chasing;
        float focusT = 0f;
        while (focusT < postAttackFocusTime)
        {
            focusT += Time.deltaTime;
            Transform cur = GetCurrentTarget();
            if (cur != null)
            {
                float dist = Vector3.Distance(transform.position, cur.position);
                if (dist <= detectionRange)
                {
                    senses.TargetPositionOfInterest = cur.position;
                    senses.HasTargetOfInterest = true;
                }
            }
            yield return null;
        }
        yield return new WaitForSeconds(attackCooldown);
    }

    IEnumerator AttackWallRoutine(GameObject wall)
    {
        if (wall == null)
        {
            currentState = State.Chasing;
            yield break;
        }

        currentState = State.Attacking;
        motor.MoveTo(wall.transform.position, walkSpeed, 1.5f);

        float waitTimer = 0f;
        while (wall != null && Vector3.Distance(transform.position, wall.transform.position) > 1.8f && waitTimer < 3f)
        {
            waitTimer += Time.deltaTime;
            visuals.UpdateAnimationState(false);
            yield return null;
        }

        motor.Stop();
        if (wall != null)
        {
            motor.RotateTowards(wall.transform.position);
        visuals.TriggerAttack(3);
        {
            float t = 0f;
            while (!visuals.AnimImpactReceived && t < 2.0f) { t += Time.deltaTime; yield return null; }
        }
            TryDestroyWall(wall);
        }

        yield return new WaitUntil(() => visuals.AnimFinishedReceived);
        visuals.StopAttack();
        senses.CurrentWallTarget = null;
        currentState = State.Chasing;
    }

    void WakeUp()
    {

        currentState = State.Transitioning;
        if (senses.CurrentNoisyObject != null && senses.CurrentPlayer == null && senses.CurrentNPCTarget == null)
        {
            StartCoroutine(WakeUpQuietRoutine());
        }
        else
        {
            StartCoroutine(WakeUpAndRoarRoutine());
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        motor.MoveTo(patrolPoints[patrolIndex].position, crawlSpeed, 0.2f);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    IEnumerator PatrolWaitRoutine()
    {
        isWaitingAtPatrol = true;
        motor.Stop();
        visuals.UpdateAnimationState(true);
        yield return new WaitForSeconds(patrolWaitTime);
        GoToNextPatrolPoint();
        isWaitingAtPatrol = false;
    }

    void SelectClosestAliveTarget()
    {
        Transform bestTarget = null;
        float closestDistance = float.MaxValue;
        bool isNPCFound = false;

        
        foreach (Transform npc in senses.npcTargets)
        {
            if (npc == null) continue;
            NPCHealth health = npc.GetComponent<NPCHealth>();
            if (health != null && !health.IsDead)
            {
                float distance = Vector3.Distance(transform.position, npc.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = npc;
                    isNPCFound = true;
                }
            }
        }

        
        foreach (Transform player in senses.playerTargets)
        {
            if (player == null) continue;
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && !health.IsDead)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = player;
                    isNPCFound = false;
                }
            }
        }

        if (bestTarget != null)
        {
            if (isNPCFound) senses.SetNPCTarget(bestTarget);
            else senses.SetPlayerTarget(bestTarget);
        }
    }

    void TryDestroyWall(GameObject wall)
    {
        if (wall != null)
        {
            Wall_Destruction destructionScript = wall.GetComponent<Wall_Destruction>();
            if (destructionScript != null)
            {
                destructionScript.Explode(wall.transform.position, transform.forward);
                visuals.PlayWallBreakSound();
                DialogueManager.ShowEnemyWallBreakDialogue();
            }
        }
    }

    void HandleDialogueFeedback()
    {
        Transform target = GetCurrentTarget();
        if (target == null) return;

        if (!hasShownFirstDetection)
        {
            hasShownFirstDetection = true;
            lastReDetectionTime = Time.time;
        }
        else if (Time.time - lastReDetectionTime > 2.0f && currentState == State.Patrol)
        {
            DialogueManager.ShowEnemyDetectedAgainDialogue(target.gameObject);
            lastReDetectionTime = Time.time;
        }
    }

    public void OnEnemyDeath()
    {
        senses.ForgetTarget();
        visuals.StopAttack();
        visuals.SetInvestigatingMode(false);
        motor.Stop();
        StopAllCoroutines();
        if (cameraController) cameraController.StopTrackingEnemy();
        currentState = State.Dead;
    }
}
