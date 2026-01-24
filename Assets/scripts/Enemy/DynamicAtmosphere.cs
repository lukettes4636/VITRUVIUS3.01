using UnityEngine;

public class DynamicAtmosphere : MonoBehaviour
{
    [Header("Configuración del Material")]
    [Tooltip("Arrastrá aquí el Material MAT_TerrorLens que creaste")]
    public Material terrorLensMat;

    [Tooltip("El nombre exacto de la referencia en el Shader Graph (suele ser _DangerLevel)")]
    public string dangerProperty = "_DangerLevel";

    [Header("Ajustes de la Zona")]
    [Tooltip("Nombre del Tag que tendrá la caja invisible")]
    public string zoneTag = "DangerZone";

    [Tooltip("Qué tan rápido cambia el color (Más alto = más rápido)")]
    public float transitionSpeed = 2f;

    // Variables internas para la animación
    private float targetLevel = 0f; // 0 = Normal, 1 = Terror
    private float currentLevel = 0f;

    void Update()
    {
        // Si no asignaste el material, no hacemos nada para evitar errores
        if (terrorLensMat == null) return;

        // Interpolación suave (Lerp): Movemos el valor actual hacia el objetivo
        currentLevel = Mathf.Lerp(currentLevel, targetLevel, Time.deltaTime * transitionSpeed);

        // Le mandamos el número al Shader
        terrorLensMat.SetFloat(dangerProperty, currentLevel);
    }

    // --- DETECCIÓN DE ZONAS ---

    // Cuando entramos a la caja
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(zoneTag))
        {
            targetLevel = 1f; // Objetivo: Activar modo terror (100%)
        }
    }

    // Cuando salimos de la caja
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(zoneTag))
        {
            targetLevel = 0f; // Objetivo: Volver a modo normal (0%)
        }
    }

    // IMPORTANTE: Esto resetea el efecto al cerrar el juego.
    // Si no ponés esto, el editor de Unity se queda rojo para siempre.
    void OnApplicationQuit()
    {
        if (terrorLensMat != null)
        {
            terrorLensMat.SetFloat(dangerProperty, 0f);
        }
    }
}