using System.Collections;
using UnityEngine;

/// <summary>
/// Maneja toda la lógica de combate del enemigo.
/// </summary>
public class EnemyCombat : MonoBehaviour
{
    [Header("Referencias")]
    private EnemyVisuals visuals;
    private EnemyMotor motor;

    [Header("Configuración de Combate")]
    [Tooltip("Rango para iniciar ataque")]
    public float attackRange = 2.2f;

    [Tooltip("Tiempo de espera después de atacar")]
    public float attackCooldown = 0.5f;

    [Tooltip("Daño a jugadores")]
    public int playerDamage = 25;

    [Tooltip("Daño a NPCs")]
    public int npcDamage = 15;

    [Header("Paredes Destructibles")]
    [Tooltip("Layer de paredes que se pueden romper")]
    public LayerMask destructibleWallLayer;

    [Tooltip("Distancia de detección de paredes")]
    public float wallCheckDistance = 5f;

    // ESTADO
    public bool IsAttacking { get; private set; }
    private bool canAttack = true;

    void Awake()
    {
        visuals = GetComponent<EnemyVisuals>();
        motor = GetComponent<EnemyMotor>();
    }

    /// <summary>
    /// Verifica si puede atacar al target actual.
    /// </summary>
    public bool CanAttackTarget(Transform target)
    {
        if (target == null) return false;
        if (!canAttack) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= attackRange;
    }

    /// <summary>
    /// Ejecuta un ataque contra el target.
    /// </summary>
    public IEnumerator AttackTarget(Transform target)
    {
        if (target == null || IsAttacking) yield break;

        IsAttacking = true;
        canAttack = false;

        // 1. Detener movimiento y rotar
        motor.Stop();
        motor.RotateTowards(target.position);
        yield return new WaitForSeconds(0.1f); // Frame para orientación

        // 2. Trigger animación ataque (1-3 random)
        int attackIndex = Random.Range(1, 4);
        visuals.TriggerAttack(attackIndex);

        // 3. Esperar impacto
        yield return new WaitUntil(() => visuals.AnimImpactReceived || visuals.AnimFinishedReceived);

        // 4. Aplicar daño si target sigue vivo
        if (target != null)
        {
            ApplyDamage(target);
        }

        // 5. Esperar fin de animación
        float waitTime = 0f;
        while (!visuals.AnimFinishedReceived && waitTime < 3f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        // 6. Limpiar hitboxes
        visuals.StopAttack();

        // 7. Cooldown
        yield return new WaitForSeconds(attackCooldown);

        IsAttacking = false;
        canAttack = true;
    }

    /// <summary>
    /// Aplica daño según el tipo de target.
    /// </summary>
    private void ApplyDamage(Transform target)
    {
        // Daño a jugador
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(playerDamage);
            return;
        }

        // Daño a NPC
        NPCHealth npcHealth = target.GetComponent<NPCHealth>();
        if (npcHealth != null && !npcHealth.IsDead)
        {
            npcHealth.TakeDamage(npcDamage);
        }
    }

    /// <summary>
    /// Ataca y destruye una pared.
    /// </summary>
    public IEnumerator AttackWall(GameObject wall, EnemySenses senses)
    {
        if (wall == null) yield break;

        IsAttacking = true;
        canAttack = false;

        // 1. Acercarse a la pared
        motor.MoveTo(wall.transform.position, 2.5f, 1.5f);

        float timeout = 0f;
        while (Vector3.Distance(transform.position, wall.transform.position) > 1.8f && timeout < 3f)
        {
            timeout += Time.deltaTime;
            visuals.UpdateAnimationState(false); // Stand walk
            yield return null;
        }

        // 2. Detener y rotar hacia pared
        motor.Stop();
        if (wall != null)
        {
            motor.RotateTowards(wall.transform.position);
            yield return new WaitForSeconds(0.2f);

            // 3. Trigger Attack 3 (rompe paredes)
            visuals.TriggerAttack(3);

            // 4. Esperar impacto
            float waitImpact = 0f;
            while (!visuals.AnimImpactReceived && waitImpact < 2f)
            {
                waitImpact += Time.deltaTime;
                yield return null;
            }

            // 5. Destruir pared
            DestroyWall(wall);

            // 6. Esperar fin animación
            yield return new WaitUntil(() => visuals.AnimFinishedReceived);
            visuals.StopAttack();
        }

        IsAttacking = false;
        canAttack = true;
    }

    /// <summary>
    /// Destruye la pared usando su script Wall_Destruction.
    /// </summary>
    private void DestroyWall(GameObject wall)
    {
        if (wall == null) return;

        Wall_Destruction destructionScript = wall.GetComponent<Wall_Destruction>();
        if (destructionScript != null)
        {
            destructionScript.Explode(wall.transform.position, transform.forward);
            visuals.PlayWallBreakSound();

            // Diálogo opcional
            DialogueManager.ShowEnemyWallBreakDialogue();
        }
    }

    /// <summary>
    /// Fuerza el fin del ataque (para transiciones de estado).
    /// </summary>
    public void CancelAttack()
    {
        IsAttacking = false;
        canAttack = true;
        visuals.StopAttack();
    }

    // DEBUG
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}