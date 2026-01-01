using System.Collections;
using UnityEngine;

/// <summary>
/// Cerebro del enemigo - Máquina de estados simplificada.
/// Estados: PATROL → ALERT → CHASE → ATTACK → INVESTIGATE → RETURN
/// </summary>

public class EnemyBrain : MonoBehaviour
{
    // COMPONENTES
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

    [Header("Investigación")]
    [Tooltip("Tiempo que espera en el último punto conocido")]
    public float investigationDuration = 2f;

    [Header("Debug")]
    [SerializeField] private State currentState;

    // ESTADOS
    private enum State
    {
        Sleeping,   // Dormido (pasivo)
        Eating,     // Comiendo (pasivo)
        Patrol,     // Patrullando en crawl
        Alert,      // GetUp → Roar (transición)
        Chase,      // Persiguiendo de pie
        Attack,     // Atacando
        Investigate,// Escuchando en punto
        Return      // ToCrawl → vuelve a patrol
    }

    void Start()
    {
        // Get componentes
        senses = GetComponent<EnemySenses>();
        motor = GetComponent<EnemyMotor>();
        visuals = GetComponent<EnemyVisuals>();
        combat = GetComponent<EnemyCombat>();
        cameraController = FindObjectOfType<EnemyCameraController>();

        // Setup inicial
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
        // Detectar targets cada frame
        bool canDetectObjects = (currentState != State.Attack);
        senses.Tick(canDetectObjects);

        // Lógica según estado
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
                break;

            case State.Investigate:
                // Manejado por coroutine
                break;

            case State.Alert:
            case State.Attack:
            case State.Return:
                // Manejados por coroutines
                break;
        }
    }

    // ========================================
    // ESTADOS PASIVOS (SLEEPING / EATING)
    // ========================================

    void HandlePassiveStates()
    {
        // Si detecta ruido, despertar
        if (senses.HasTarget)
        {
            StartCoroutine(WakeUpSequence());
        }
    }

    IEnumerator WakeUpSequence()
    {
        currentState = State.Alert;
        motor.Stop();

        // GetUp animation
        visuals.TriggerGetUp();
        yield return new WaitForSeconds(2.5f); // Duración GetUp

        // Confirmar que salió de crawl
        float confirmTime = 0f;
        while (confirmTime < 1f)
        {
            confirmTime += Time.deltaTime;
            yield return null;
        }

        // Roar + Shader + Zoom
        if (cameraController)
            cameraController.StartTrackingEnemy(transform);

        visuals.TriggerRoar();
        visuals.PlayRoarSound();
        yield return new WaitForSeconds(3f); // Duración Roar

        // Transición a Chase
        currentState = State.Chase;
    }

    // ========================================
    // PATRULLA
    // ========================================

    void HandlePatrol()
    {
        // Si detecta ruido, despertar
        if (senses.HasTarget)
        {
            StartCoroutine(WakeUpSequence());
            return;
        }

        // Lógica de patrulla
        if (patrolPoints.Length == 0) return;

        if (motor.GetRemainingDistance() <= 0.2f)
        {
            StartCoroutine(PatrolWaitRoutine());
        }
        else
        {
            visuals.UpdateAnimationState(true); // Crawl walk
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

    // ========================================
    // PERSECUCIÓN
    // ========================================

    void HandleChase()
    {
        // VERIFICAR TARGET
        if (!senses.HasTarget)
        {
            // Perdió el target, investigar último punto
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        Transform target = senses.CurrentTarget;
        if (target == null)
        {
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        // VERIFICAR SI MURIÓ
        if (IsTargetDead(target))
        {
            StartCoroutine(InvestigateLastKnown());
            return;
        }

        //  VERIFICAR PAREDES
        if (senses.CheckWallInPath(out GameObject wall, combat.destructibleWallLayer, combat.wallCheckDistance))
        {
            StartCoroutine(combat.AttackWall(wall, senses));
            currentState = State.Attack;
            return;
        }

        //  VERIFICAR RANGO DE ATAQUE
        if (combat.CanAttackTarget(target))
        {
            StartCoroutine(AttackSequence(target));
            return;
        }

        //  PERSEGUIR
        motor.MoveTo(target.position, walkSpeed, 0.5f);
        visuals.UpdateAnimationState(false); // Stand walk
    }

    bool IsTargetDead(Transform target)
    {
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead) return true;

        NPCHealth npcHealth = target.GetComponent<NPCHealth>();
        if (npcHealth != null && npcHealth.IsDead) return true;

        return false;
    }

    // ========================================
    // ATAQUE
    // ========================================

    IEnumerator AttackSequence(Transform target)
    {
        currentState = State.Attack;

        // Ejecutar ataque
        yield return StartCoroutine(combat.AttackTarget(target));

        // Después del ataque
        if (senses.HasTarget && !IsTargetDead(target))
        {
            // Sigue vivo y audible, volver a chase
            currentState = State.Chase;
        }
        else
        {
            // Perdió el target, investigar
            yield return StartCoroutine(InvestigateLastKnown());
        }
    }

    // ========================================
    // INVESTIGACIÓN
    // ========================================

    IEnumerator InvestigateLastKnown()
    {
        currentState = State.Investigate;
        Vector3 investigatePos = senses.LastKnownPosition;

        //  Ir al último punto conocido
        motor.MoveTo(investigatePos, walkSpeed, 0.2f);

        float timeout = 0f;
        while (motor.GetRemainingDistance() > 0.3f && timeout < 7f)
        {
            timeout += Time.deltaTime;
            visuals.UpdateAnimationState(false); // Stand walk

            // Si detecta algo nuevo, cancelar investigación
            if (senses.HasTarget)
            {
                currentState = State.Chase;
                yield break;
            }

            yield return null;
        }

        //  Detenerse y escuchar
        motor.Stop();
        visuals.UpdateAnimationState(false);

        float investigateTime = 0f;
        while (investigateTime < investigationDuration)
        {
            investigateTime += Time.deltaTime;

            // Si detecta algo, cancelar
            if (senses.HasTarget)
            {
                currentState = State.Chase;
                yield break;
            }

            yield return null;
        }

        // No encontró nada, volver a patrol
        yield return StartCoroutine(ReturnToPatrol());
    }

    // ========================================
    // VOLVER A PATRULLA
    // ========================================

    IEnumerator ReturnToPatrol()
    {
        currentState = State.Return;

        // Zoom in cámara
        if (cameraController)
            cameraController.StopTrackingEnemy();

        motor.Stop();

        // ToCrawl animation
        visuals.TriggerToCrawl();
        yield return new WaitForSeconds(2.5f); // Duración ToCrawl

        // Volver a patrol
        currentState = State.Patrol;
        motor.SetAutoRotation(true);
        GoToNextPatrolPoint();
    }

    // ========================================
    // MUERTE
    // ========================================

    public void OnEnemyDeath()
    {
        senses.ClearTarget();
        combat.CancelAttack();
        visuals.StopAttack();
        motor.Stop();
        StopAllCoroutines();

        if (cameraController)
            cameraController.StopTrackingEnemy();

        currentState = State.Patrol; // Estado dummy
        enabled = false; // Deshabilitar script
    }
}