using UnityEngine;

public class DynamicAtmosphere : MonoBehaviour
{
    [Header("Configuración del Material")]
    [Tooltip("Arrastrá aquí el Material MAT_TerrorLens que creaste")]
    public Material terrorLensMat;

    [Tooltip("El nombre exacto de la referencia en el Shader Graph")]
    public string dangerProperty = "_DangerLevel";

    [Header("Ajustes de la Zona")]
    [Tooltip("Qué tan rápido cambia el color (Más alto = más rápido)")]
    public float transitionSpeed = 2f;

    [Header("Detección")]
    public string player1Tag = "Player1";
    public string player2Tag = "Player2";

    // Variables internas para la animación
    private int playersInside = 0; // Cuenta cuántos jugadores están pisando la zona
    private float currentLevel = 0f;

    void Update()
    {
        if (terrorLensMat == null) return;

        // Si hay al menos 1 jugador adentro, el objetivo es 1 (Terror). Si no, es 0 (Normal).
        float targetLevel = (playersInside > 0) ? 1f : 0f;

        // Interpolación suave
        currentLevel = Mathf.Lerp(currentLevel, targetLevel, Time.deltaTime * transitionSpeed);
        terrorLensMat.SetFloat(dangerProperty, currentLevel);
    }

    // --- DETECCIÓN DE ZONAS ---

    void OnTriggerEnter(Collider other)
    {
        // Si entra cualquier jugador, sumamos 1 a la cuenta
        if (other.CompareTag(player1Tag) || other.CompareTag(player2Tag))
        {
            playersInside++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Si sale un jugador, restamos 1 de la cuenta
        if (other.CompareTag(player1Tag) || other.CompareTag(player2Tag))
        {
            playersInside--;

            // Por seguridad, evitamos que el contador baje de cero
            if (playersInside < 0) playersInside = 0;
        }
    }

    void OnApplicationQuit()
    {
        // Reseteo de seguridad al cerrar el juego
        if (terrorLensMat != null)
        {
            terrorLensMat.SetFloat(dangerProperty, 0f);
        }
    }
}