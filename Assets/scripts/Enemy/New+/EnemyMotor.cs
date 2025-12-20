using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMotor : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Configuracion Base")]
    public float rotationSpeed = 8f;
    public float movementThreshold = 0.05f;

    
    public bool IsMoving { get; private set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        agent.updateRotation = false;
        agent.acceleration = 12f;
        agent.angularSpeed = 120f;
    }

    void Update()
    {
        
        float actualSpeed = agent.velocity.magnitude;
        bool hasIntent = agent.desiredVelocity.magnitude > 0.5f && !agent.isStopped;
        bool calculatingPath = agent.pathPending;

        IsMoving = actualSpeed > movementThreshold || hasIntent || calculatingPath;

        HandleRotation();
    }

    public void MoveTo(Vector3 position, float speed, float stoppingDistance)
    {
        if (agent.isStopped) agent.isStopped = false;

        agent.speed = speed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(position);
    }

    public void Stop()
    {
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
        IsMoving = false;
    }

    public void RotateTowards(Vector3 targetPosition)
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandleRotation()
    {
        if (IsMoving || agent.hasPath)
        {
            Vector3 nextPoint = agent.steeringTarget;
            RotateTowards(nextPoint);
        }
    }

    public void SetAutoRotation(bool enabled)
    {
        agent.updateRotation = enabled;
    }

    public float GetRemainingDistance()
    {
        if (agent.pathPending) return Mathf.Infinity;
        return agent.remainingDistance;
    }
}