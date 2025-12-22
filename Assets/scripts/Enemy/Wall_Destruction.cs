using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class Wall_Destruction : MonoBehaviour
{
    public GameObject fracturedWallPrefab;

    [Header("Efectos Visuales")]
    public GameObject dustExplosionVFXPrefab;
    private const float VfxCleanupTime = 2.0f;

    [Header("Parametros de Destruccion")]
    public float baseExplosionForce = 800f;
    public float maxAngle = 45.0f;
    public ForceMode forceMode = ForceMode.Impulse;

    [Header("Control de Fisica Post-Destruccion")]
    public float physicsSimulationTime = 3.0f;
    public float cleanupTime = 0.0f;

    [Header("Navegacion del Enemigo")]
    public string enemyLayerName = "Enemy";
    public string fragmentLayerName = "Debris";
    public bool disableCollisionWithEnemy = true;
    public bool removeFromNavMesh = true;

    [Header("NavMesh Update")]
    public bool updateNavMeshOnDestroy = true;
    public float navMeshUpdateRadius = 10f;

    private Collider[] originalColliders;

    private void Awake()
    {
        originalColliders = GetComponentsInChildren<Collider>();
    }

    public void Explode(Vector3 impactPoint, Vector3 impactDirection)
    {
        if (fracturedWallPrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 wallPosition = transform.position;

        GameObject brokenWall = Instantiate(fracturedWallPrefab, transform.position, transform.rotation);

        if (dustExplosionVFXPrefab != null)
        {
            GameObject dustVFXInstance = Instantiate(dustExplosionVFXPrefab, impactPoint, Quaternion.identity);
            Destroy(dustVFXInstance, VfxCleanupTime);
        }

        if (!brokenWall.activeSelf)
        {
            brokenWall.SetActive(true);
        }

        SetupFragments(brokenWall);
        RemoveFromNavMesh();
        
        // Start UpdateNavMeshAfterDestruction on the brokenWall since this object is about to be disabled
        if (updateNavMeshOnDestroy)
        {
            // Use a temporary MonoBehaviour on the new object or a global manager to run the coroutine
            // For simplicity, we'll try to run it on the brokenWall if it has a MonoBehaviour, 
            // otherwise we'll create a temporary helper.
            MonoBehaviour runner = brokenWall.GetComponent<MonoBehaviour>();
            if (runner == null) runner = brokenWall.AddComponent<CoroutineRunner>();
            
            runner.StartCoroutine(UpdateNavMeshAfterDestruction(wallPosition));
        }

        // Start SimulateAndFreeze on the brokenWall since this object is about to be disabled
        MonoBehaviour simRunner = brokenWall.GetComponent<MonoBehaviour>();
        if (simRunner == null) simRunner = brokenWall.AddComponent<CoroutineRunner>();
        simRunner.StartCoroutine(SimulateAndFreeze(brokenWall.transform, impactPoint, impactDirection));

        if (gameObject.scene.IsValid())
        {
            gameObject.SetActive(false);
        }
    }
    
    // Helper class to run coroutines on the broken wall object
    public class CoroutineRunner : MonoBehaviour {}

    private void RemoveFromNavMesh()
    {
        if (originalColliders != null)
        {
            foreach (Collider col in originalColliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        NavMeshObstacle[] obstacles = GetComponentsInChildren<NavMeshObstacle>();
        foreach (var obstacle in obstacles)
        {
            if (obstacle != null)
            {
                obstacle.enabled = false;
            }
        }
    }

    private IEnumerator UpdateNavMeshAfterDestruction(Vector3 position)
    {
        yield return null;
        yield return null;

        NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();

        if (surfaces.Length > 0)
        {
            foreach (NavMeshSurface surface in surfaces)
            {
                // Set to use physics colliders to avoid Read/Write errors on meshes in standalone builds
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                
                // Force update to ensure all colliders are properly recognized
                surface.defaultArea = 0;
                surface.BuildNavMesh();
            }
        }

        // Force a frame delay to ensure physics updates
        yield return null;

        Collider[] nearbyColliders = Physics.OverlapSphere(position, navMeshUpdateRadius);
        foreach (Collider col in nearbyColliders)
        {
            NavMeshAgent agent = col.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh && agent.hasPath)
            {
                Vector3 currentDestination = agent.destination;
                agent.ResetPath();
                yield return null;
                agent.SetDestination(currentDestination);
            }
        }
    }

    private void SetupFragments(GameObject brokenWall)
    {
        Collider[] fragmentColliders = brokenWall.GetComponentsInChildren<Collider>();
        
        int debrisLayer = LayerMask.NameToLayer(fragmentLayerName);
        if (debrisLayer != -1)
        {
            brokenWall.layer = debrisLayer;
            foreach (Transform t in brokenWall.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = debrisLayer;
                var modifier = t.gameObject.GetComponent<Unity.AI.Navigation.NavMeshModifier>();
                if (modifier == null) modifier = t.gameObject.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
                modifier.ignoreFromBuild = true;
            }
        }

        if (removeFromNavMesh)
        {
            NavMeshObstacle[] obstacles = brokenWall.GetComponentsInChildren<NavMeshObstacle>();
            foreach (var obstacle in obstacles)
            {
                obstacle.enabled = false;
            }
        }

        if (disableCollisionWithEnemy)
        {
            int enemyLayer = LayerMask.NameToLayer(enemyLayerName);

            if (enemyLayer != -1)
            {
                foreach (Collider fragCol in fragmentColliders)
                {
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    foreach (GameObject enemy in enemies)
                    {
                        Collider[] enemyCols = enemy.GetComponentsInChildren<Collider>();
                        foreach (Collider enemyCol in enemyCols)
                        {
                            Physics.IgnoreCollision(fragCol, enemyCol, true);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator SimulateAndFreeze(Transform parent, Vector3 impactPoint, Vector3 impactDirection)
    {
        Vector3 directionCentral = impactDirection.normalized;
        Rigidbody[] fragments = parent.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in fragments)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            float angleDeviation = maxAngle / 2f;
            float angleX = Random.Range(-angleDeviation, angleDeviation);
            float angleY = Random.Range(-angleDeviation, angleDeviation);

            Quaternion rotation = Quaternion.AngleAxis(angleX, Vector3.right) * Quaternion.AngleAxis(angleY, Vector3.up);
            Vector3 directionPush = rotation * directionCentral;

            float forceMagnitude = baseExplosionForce * rb.mass;
            Vector3 forceVector = directionPush.normalized * forceMagnitude;

            rb.AddForceAtPosition(forceVector, impactPoint, forceMode);
            rb.AddTorque(Random.insideUnitSphere * forceMagnitude * 0.01f, forceMode);
        }

        yield return new WaitForSeconds(physicsSimulationTime);

        foreach (Rigidbody rb in fragments)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (cleanupTime > 0)
        {
            yield return new WaitForSeconds(cleanupTime);
            Destroy(parent.gameObject);
        }
    }
}
