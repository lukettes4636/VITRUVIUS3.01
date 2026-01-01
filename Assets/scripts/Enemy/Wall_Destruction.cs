using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Wall_Destruction : MonoBehaviour
{
    [Header("Prefab de Pared Rota")]
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
            Debug.LogWarning("No hay prefab de pared fracturada asignado");
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

        MonoBehaviour runner = brokenWall.GetComponent<MonoBehaviour>();
        if (runner == null)
            runner = brokenWall.AddComponent<CoroutineRunner>();

        runner.StartCoroutine(SimulateAndFreeze(brokenWall.transform, impactPoint, impactDirection));

        if (updateNavMeshOnDestroy)
        {
            runner.StartCoroutine(UpdateNavMeshAfterDestruction(wallPosition));
        }

        if (gameObject.scene.IsValid())
        {
            gameObject.SetActive(false);
        }
    }

    public class CoroutineRunner : MonoBehaviour { }

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
            Debug.Log("[Wall_Destruction] Actualizando NavMeshSurfaces");

            foreach (NavMeshSurface surface in surfaces)
            {
                surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
                surface.defaultArea = 0;
                surface.BuildNavMesh();
            }

            Debug.Log("[Wall_Destruction] NavMesh actualizado correctamente");
        }
        else
        {
            Debug.LogWarning("[Wall_Destruction] No se encontro NavMeshSurface en la escena");
        }

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

                Debug.Log("[Wall_Destruction] Ruta recalculada para: " + agent.gameObject.name);
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
            }

            Debug.Log("[Wall_Destruction] Fragmentos configurados en layer: " + fragmentLayerName);
        }
        else
        {
            Debug.LogWarning("[Wall_Destruction] Layer no encontrado: " + fragmentLayerName);
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

            Quaternion rotation = Quaternion.AngleAxis(angleX, Vector3.right) *
                                  Quaternion.AngleAxis(angleY, Vector3.up);
            Vector3 directionPush = rotation * directionCentral;

            float forceMagnitude = baseExplosionForce * rb.mass;
            Vector3 forceVector = directionPush.normalized * forceMagnitude;

            rb.AddForceAtPosition(forceVector, impactPoint, forceMode);
            rb.AddTorque(Random.insideUnitSphere * forceMagnitude * 0.01f, forceMode);
        }

        Debug.Log("[Wall_Destruction] Simulando fisica");
        yield return new WaitForSeconds(physicsSimulationTime);

        Debug.Log("[Wall_Destruction] Congelando fragmentos y deshabilitando colliders");

        int disabledCount = 0;
        foreach (Rigidbody rb in fragments)
        {
            Collider col = rb.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
                disabledCount++;
            }

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("[Wall_Destruction] Colliders deshabilitados: " + disabledCount);

        yield return new WaitForSeconds(0.1f);
        ForceNavMeshUpdate();

        if (cleanupTime > 0)
        {
            yield return new WaitForSeconds(cleanupTime);
            Destroy(parent.gameObject);
        }
    }

    private void ForceNavMeshUpdate()
    {
        NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();

        if (surfaces.Length > 0)
        {
            Debug.Log("[Wall_Destruction] Forzando rebuild de NavMesh");

            foreach (var surface in surfaces)
            {
                surface.BuildNavMesh();
            }
        }

        EnemyMotor[] enemies = FindObjectsOfType<EnemyMotor>();
        foreach (var enemy in enemies)
        {
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh && agent.hasPath)
            {
                Vector3 destination = agent.destination;
                agent.ResetPath();
                agent.SetDestination(destination);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, navMeshUpdateRadius);
    }
}