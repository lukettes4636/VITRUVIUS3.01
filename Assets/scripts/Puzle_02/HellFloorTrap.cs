using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class HellFloorTrap : MonoBehaviour
{
    [Header("Componentes Visuales")]
    [SerializeField] private VisualEffect trapVFX;
    [SerializeField] private Renderer floorRenderer;

    [Header("Configuración del Evento")]
    [Tooltip("Velocidad a la que se rompe el suelo")]
    [SerializeField] private float destructionSpeed = 2.0f;

    [Tooltip("Hasta qué punto se rompe el suelo. 0 = Totalmente roto, 1 = Intacto.")]
    [Range(0.0f, 1.0f)] // Crea un slider en el Inspector
    [SerializeField] private float destructionLimit = 0.2f;

    [Header("Detección")]
    [SerializeField] private string player1Tag = "Player1";
    [SerializeField] private string player2Tag = "Player2";

    // Variables internas
    private Material floorMaterial;
    private bool isTriggered = false;
    private int openAmountID;

    private void Awake()
    {
        openAmountID = Shader.PropertyToID("_OpenAmount");

        if (floorRenderer != null)
        {
            floorMaterial = floorRenderer.material;
            // IMPORTANTE: Empezamos en 1 (Suelo Sano/Intacto)
            floorMaterial.SetFloat(openAmountID, 1f);
        }

        if (trapVFX != null) trapVFX.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag(player1Tag) || other.CompareTag(player2Tag))
        {
            ActivateTrap();
        }
    }

    private void ActivateTrap()
    {
        isTriggered = true;

        // 1. Efectos visuales
        if (trapVFX != null)
        {
            trapVFX.Play();
            trapVFX.SendEvent("OnTrapTrigger");
        }

        // 2. Romper el suelo (de 1 hacia abajo)
        if (floorMaterial != null)
        {
            StartCoroutine(AnimateDestruction());
        }
    }

    private IEnumerator AnimateDestruction()
    {
        float currentIntegrity = 1f; // Empezamos al 100% de integridad

        // Mientras la integridad sea MAYOR que el límite que pusiste...
        while (currentIntegrity > destructionLimit)
        {
            // Restamos valor (rompemos)
            currentIntegrity -= Time.deltaTime * destructionSpeed;

            // Aplicamos al shader
            floorMaterial.SetFloat(openAmountID, currentIntegrity);

            yield return null;
        }

        // Aseguramos que quede clavado en el límite exacto al terminar
        floorMaterial.SetFloat(openAmountID, destructionLimit);
    }
}