using System.Collections;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    private EnemySenses senses;
    private EnemyMotor motor;
    private EnemyVisuals visuals;
    private EnemyCombat combat;
    private EnemyCameraController cameraController;

    [Header("Estado Inicial")]
    public InitialState initialState = InitialState.Sleeping;
    public enum InitialState { Sleeping, Eating, Patrol }

    [Header("Velocidades")]
    public float crawlSpeed = 1.2f;
    public float walkSpeed = 2.5f;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1f;
    private int patrolIndex = 0;

    [Header("Investigacion")]
    public float investigationDuration = 2f;

    [Header("Debug")]
    [SerializeField] private State currentState;

    private enum State
    {
        Sleeping,
        Eating,
        Patrol,
        Alert,
        Chase,
        Attack,
        Investigate,
        Return
    }

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float stuckCheckInterval = 0.5f;
    private const float stuckThreshold = 0.1f;

    void Start()
    {
        senses = GetComponent<EnemySenses>();
        motor = GetComponent<EnemyMotor>();
        visuals = GetComponent<EnemyVisuals>();
        combat = GetComponent<EnemyCombat>();
        cameraController = FindObjectOfType<EnemyCameraController>();

        lastPosition = transform.position;
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
        bool canDetectObjects = (currentState != State.Attack);
        senses.Tick(canDetectObjects);

        switch (currentState)
        {
            case State.Sleeping:
            case State.Eating:
                HandlePassiveStates();
                break;

            case State.Patrol:
                HandlePatrol();
                break;

            case State.Chase:
                HandleChase();
                CheckIfStuck();
                break;

            case State.Investigate:
            case State.Alert:
            case State.Attack:
            case State.Return:
                break;
        }
    }

    void HandlePassiveStates()
    {
        if (senses.HasTarget)
        {
            StartCoroutine(WakeUpSequence());
        }
    }

    IEnumerator WakeUpSequence()
    {
        currentState = State.Alert;
        motor.Stop();

        visuals.TriggerGetUp();
        yield return new WaitForSeconds(2.5f);

        float confirmTime = 0f;
        while (confirmTime < 1f)
        {
            confirmTime += Time.deltaTime;
            yield return null;
        }

        if (cameraController)
            cameraController.StartTrackingEnemy(transform);

        visuals.TriggerRoar();
        visuals.PlayRoarSound();
        yield return new WaitForSeconds(3f);

        currentState = State.Chase;
    }

    void HandlePatrol()
    {
        if (senses.HasTarget)
        {
            StartCoroutine(WakeUpSequence());
            return;
        }

        if (patrolPoints.Length == 0) return;

        if (motor.GetRemainingDistance() <= 0.2f)
        {
            StartCoroutine(PatrolWaitRoutine());
        }
        else
        {
            visuals.UpdateAnimationState(true);
        }
    }

    IEnumerator PatrolWaitRoutine()
    {
        motor.Stop();
        visuals.UpdateAnimationState(true);
        yield return new WaitForSeconds(patrolWaitTime);
        GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        motor.MoveTo(patrolPoints[patrolIndex].position, crawlSpeed, 0.2f);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    void HandleChase()
    {
        if (!senses.HasTarget)
        {
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        Transform target = senses.CurrentTarget;
        if (target == null)
        {
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        if (IsTargetDead(target))
        {
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        if (motor.GetRemainingDistance() == Mathf.Infinity)
        {
            Debug.LogWarning("[EnemyBrain] NavMesh path invalido, investigando ultima posicion");
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        if (senses.CheckWallInPath(out GameObject wall, combat.destructibleWallLayer, combat.wallCheckDistance))
        {
            currentState = State.Attack;
            StartCoroutine(AttackWallSequence(wall));
            return;
        }

        if (combat.CanAttackTarget(target))
        {
            StartCoroutine(AttackSequence(target));
            return;
        }

        motor.MoveTo(target.position, walkSpeed, 0.5f);
        visuals.UpdateAnimationState(false);
    }

    bool IsTargetDead(Transform target)
    {
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead) return true;

        NPCHealth npcHealth = target.GetComponent<NPCHealth>();
        if (npcHealth != null && npcHealth.IsDead) return true;

        return false;
    }

    IEnumerator AttackSequence(Transform target)
    {
        currentState = State.Attack;

        yield return StartCoroutine(combat.AttackTarget(target));

        if (senses.HasTarget && !IsTargetDead(target))
        {
            currentState = State.Chase;
        }
        else
        {
            yield return StartCoroutine(InvestigateLastKnown());
        }
    }

    IEnumerator AttackWallSequence(GameObject wall)
    {
        yield return StartCoroutine(combat.AttackWall(wall, senses));

        if (senses.HasTarget)
        {
            currentState = State.Chase;
        }
        else
        {
            yield return StartCoroutine(InvestigateLastKnown());
        }
    }

    void CheckIfStuck()
    {
        stuckTimer += Time.deltaTime;

        if (stuckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (motor.IsMoving && distanceMoved < stuckThreshold)
            {
                Debug.LogWarning("[EnemyBrain] Enemigo detectado atascado. Forzando investigacion.");

                motor.Stop();
                StartCoroutine(InvestigateLastKnown());
            }

            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }

    IEnumerator InvestigateLastKnown()
    {
        if (currentState == State.Investigate)
        {
            Debug.LogWarning("[EnemyBrain] Ya esta investigando, saliendo");
            yield break;
        }

        currentState = State.Investigate;
        Vector3 investigatePos = senses.LastKnownPosition;

        Debug.Log("[EnemyBrain] Iniciando investigacion en: " + investigatePos);

        motor.MoveTo(investigatePos, walkSpeed, 0.2f);

        float timeout = 0f;
        while (motor.GetRemainingDistance() > 0.3f && timeout < 7f)
        {
            timeout += Time.deltaTime;
            visuals.UpdateAnimationState(false);

            if (senses.HasTarget)
            {
                Debug.Log("[EnemyBrain] Target detectado durante investigacion, volviendo a Chase");
                currentState = State.Chase;
                yield break;
            }

            yield return null;
        }

        motor.Stop();
        visuals.UpdateAnimationState(false);

        Debug.Log("[EnemyBrain] Escuchando por " + investigationDuration + " segundos");

        float investigateTime = 0f;
        while (investigateTime < investigationDuration)
        {
            investigateTime += Time.deltaTime;

            if (senses.HasTarget)
            {
                Debug.Log("[EnemyBrain] Target detectado, cancelando investigacion");
                currentState = State.Chase;
                yield break;
            }

            yield return null;
        }

        Debug.Log("[EnemyBrain] Investigacion completada, volviendo a patrol");
        yield return StartCoroutine(ReturnToPatrol());
    }

    IEnumerator ReturnToPatrol()
    {
        currentState = State.Return;

        if (cameraController)
            cameraController.StopTrackingEnemy();

        motor.Stop();

        visuals.TriggerToCrawl();
        yield return new WaitForSeconds(2.5f);

        currentState = State.Patrol;
        motor.SetAutoRotation(true);
        GoToNextPatrolPoint();
    }

    public void OnEnemyDeath()
    {
        senses.ClearTarget();
        combat.CancelAttack();
        visuals.StopAttack();
        motor.Stop();
        StopAllCoroutines();

        if (cameraController)
            cameraController.StopTrackingEnemy();

        currentState = State.Patrol;
        enabled = false;
    }
}