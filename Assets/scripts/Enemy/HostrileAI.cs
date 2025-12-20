using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;


public class HostileAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private List<Transform> playerTransforms; 
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;


    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask playerLayerMask;


    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;


    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1f;
    private bool isOnAttackCooldown;
    [SerializeField] private float forwardShotForce = 10f;
    [SerializeField] private float verticalShotForce = 5f;


    [Header("Detection Ranges")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;
    [SerializeField] private float pursuitSpeed = 3.5f; 

    [Header("Advanced Detection")]
    [SerializeField] private float fieldOfView = 110f; 
    [SerializeField] private float hearingRange = 15f; 
    [SerializeField] private float memoryDuration = 5f; 

    private bool isPlayerVisible;
    private bool isPlayerInRange;
    private Transform currentTargetPlayer; 
    private Vector3 lastKnownPlayerPosition;
    private float lastSightingTime;
    private bool hasMemoryOfPlayer;
    private bool hasShownFirstDetection;
    private bool isInChase;
    private bool lastIsPlayerVisible;
    private bool previousHasMemoryOfPlayer;
    [SerializeField] private float reDetectionCooldown = 2f;
    private float lastReDetectionTime;
    private bool wallBreakDialogueFired;

    


    private void Awake()
    {
        
        playerTransforms = new List<Transform>();
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        if (player1 != null)
        {
            playerTransforms.Add(player1.transform);
        }
        
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        if (player2 != null)
        {
            playerTransforms.Add(player2.transform);
        }

        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        if (navAgent != null)
        {
            navAgent.autoRepath = true;
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navAgent.avoidancePriority = 50;
            navAgent.acceleration = 16f;
            navAgent.angularSpeed = 180f;
            navAgent.autoBraking = true;
        }

        hasShownFirstDetection = false;
        isInChase = false;
        lastIsPlayerVisible = false;
        previousHasMemoryOfPlayer = false;
        lastReDetectionTime = -999f;
        wallBreakDialogueFired = false;
    }


    private void Update()
    {
        DetectPlayer();
        previousHasMemoryOfPlayer = hasMemoryOfPlayer;
        UpdateMemory();
        HandleDialogueTransitions(previousHasMemoryOfPlayer);
        UpdateBehaviourState();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagementRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        
        Gizmos.color = Color.cyan;
        float halfFOV = fieldOfView / 2.0f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
        Vector3 leftDirection = leftRayRotation * transform.forward * visionRange;
        Vector3 rightDirection = rightRayRotation * transform.forward * visionRange;
        Gizmos.DrawRay(transform.position, leftDirection);
        Gizmos.DrawRay(transform.position, rightDirection);
        Gizmos.DrawLine(transform.position + rightDirection, transform.position + leftDirection);

        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        
        if (hasMemoryOfPlayer)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
        }
    }


    private void DetectPlayer()
    {
        isPlayerVisible = false;
        isPlayerInRange = false;
        currentTargetPlayer = null;
        float closestDistance = Mathf.Infinity;

        if (playerTransforms == null || playerTransforms.Count == 0) return;

        foreach (Transform player in playerTransforms)
        {
            if (player == null) continue;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            
            if (distanceToPlayer <= visionRange && angleToPlayer <= fieldOfView / 2f)
            {
                
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRange, playerLayerMask))
                {
                    if (hit.transform == player)
                    {
                        isPlayerVisible = true;
                        lastKnownPlayerPosition = player.position;
                        lastSightingTime = Time.time;
                        hasMemoryOfPlayer = true;

                        if (distanceToPlayer < closestDistance)
                        {
                            closestDistance = distanceToPlayer;
                            currentTargetPlayer = player;
                        }

                        
                        if (distanceToPlayer <= engagementRange)
                        {
                            isPlayerInRange = true;
                        }
                    }
                }
            }
            
            else if (distanceToPlayer <= hearingRange)
            {
                
                lastKnownPlayerPosition = player.position;
                lastSightingTime = Time.time;
                hasMemoryOfPlayer = true;
            }
        }
    }

    private void HandleDialogueTransitions(bool wasHasMemory)
    {
        if (isPlayerVisible && !lastIsPlayerVisible)
        {
            if (!hasShownFirstDetection)
            {
                hasShownFirstDetection = true;
                isInChase = true;
            }
            else if (Time.time - lastReDetectionTime > reDetectionCooldown)
            {
                if (currentTargetPlayer != null)
                    DialogueManager.ShowEnemyDetectedAgainDialogue(currentTargetPlayer.gameObject);
                lastReDetectionTime = Time.time;
                isInChase = true;
            }
        }

        if (wasHasMemory && !hasMemoryOfPlayer)
        {
            if (isInChase)
            {
                DialogueManager.ShowEnemyChaseEndedDialogue();
                isInChase = false;
            }
        }

        lastIsPlayerVisible = isPlayerVisible;
    }

    public void OnWallBroken()
    {
        if (wallBreakDialogueFired) return;
        DialogueManager.ShowEnemyWallBreakDialogue();
        wallBreakDialogueFired = true;
    }

    private void UpdateMemory()
    {
        
        if (hasMemoryOfPlayer && Time.time - lastSightingTime > memoryDuration)
        {
            hasMemoryOfPlayer = false;
        }
    }


    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;


        Rigidbody projectileRb = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        projectileRb.AddForce(transform.forward * forwardShotForce, ForceMode.Impulse);
        projectileRb.AddForce(transform.up * verticalShotForce, ForceMode.Impulse);


        Destroy(projectileRb.gameObject, 3f);
    }


    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);


        Vector3 potentialPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);


        if (Physics.Raycast(potentialPoint, -transform.up, 2f, terrainLayer))
        {
            currentPatrolPoint = potentialPoint;
            hasPatrolPoint = true;
        }
    }


    private IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCooldown = false;
    }




    private void PerformPatrol()
    {
        if (!hasPatrolPoint)
            FindPatrolPoint();


        if (hasPatrolPoint)
            navAgent.SetDestination(currentPatrolPoint);


        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
            hasPatrolPoint = false;
    }


    private void PerformChase()
    {
        if (currentTargetPlayer != null)
        {
            navAgent.speed = pursuitSpeed; 
            navAgent.SetDestination(currentTargetPlayer.position);
        }
    }

    

    private void PerformAttack()
    {
        navAgent.speed = 0f; 
        navAgent.SetDestination(transform.position);

        if (currentTargetPlayer != null)
        {
            transform.LookAt(currentTargetPlayer);
        }

        if (!isOnAttackCooldown)
        {
            FireProjectile();
            StartCoroutine(AttackCooldownRoutine());
        }
    }


    private void PerformInvestigation()
    {
        
        navAgent.speed = pursuitSpeed * 0.7f; 
        navAgent.SetDestination(lastKnownPlayerPosition);

        
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 2f || !hasMemoryOfPlayer)
        {
            hasMemoryOfPlayer = false;
        }
    }


    private void UpdateBehaviourState()
    {
        if (AreAllPlayersInactive())
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
            return;
        }
        if (isPlayerVisible && isPlayerInRange)
        {
            PerformAttack();
        }
        else if (isPlayerVisible && !isPlayerInRange)
        {
            PerformChase();
        }
        else if (hasMemoryOfPlayer)
        {
            PerformInvestigation();
        }
        else
        {
            PerformPatrol();
        }
    }

    private bool AreAllPlayersInactive()
    {
        if (playerTransforms == null || playerTransforms.Count == 0)
        {
            return true; 
        }

        foreach (Transform player in playerTransforms)
        {
            if (player != null)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    return false; 
                }
            }
        }
        return true; 
    }
}
