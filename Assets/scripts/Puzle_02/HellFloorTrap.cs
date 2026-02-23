using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class HellFloorTrap : MonoBehaviour
{
    [Header("Componentes Visuales")]
    [SerializeField] private VisualEffect trapVFX;
    [SerializeField] private Renderer floorRenderer;

    [Header("Bloqueador Físico")]
    [Tooltip("El muro invisible que bloquea el paso una vez que el suelo se rompe.")]
    [SerializeField] private GameObject blockerCollider;

    [Header("Configuración del Evento")]
    [Tooltip("Velocidad a la que se rompe el suelo")]
    [SerializeField] private float destructionSpeed = 2.0f;

    [Tooltip("Hasta qué punto se rompe el suelo. 0 = Totalmente roto, 1 = Intacto.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float destructionLimit = 0.2f;

    [Header("Detección")]
    [SerializeField] private string player1Tag = "Player1";
    [SerializeField] private string player2Tag = "Player2";

    // Variables internas
    private Material floorMaterial;
    private bool isTriggered = false;
    private int openAmountID;

    // Memoria de quién cruzó
    private bool p1HasEntered = false;
    private bool p2HasEntered = false;

    // Saber si están parados encima AHORA MISMO
    private bool isPlayer1Inside = false;
    private bool isPlayer2Inside = false;

    private void Awake()
    {
        openAmountID = Shader.PropertyToID("_OpenAmount");

        if (floorRenderer != null)
        {
            floorMaterial = floorRenderer.material;
            floorMaterial.SetFloat(openAmountID, 1f); // 1 = Intacto
        }

        if (trapVFX != null) trapVFX.Stop();

        // Apagamos el muro invisible al principio
        if (blockerCollider != null) blockerCollider.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        // Registramos que entraron y que actualmente están pisando
        if (other.CompareTag(player1Tag))
        {
            isPlayer1Inside = true;
            p1HasEntered = true; // Memoria: el P1 ya tocó esto
        }
        else if (other.CompareTag(player2Tag))
        {
            isPlayer2Inside = true;
            p2HasEntered = true; // Memoria: el P2 ya tocó esto
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isTriggered) return;

        // Actualizamos que ya no están pisando
        if (other.CompareTag(player1Tag)) isPlayer1Inside = false;
        else if (other.CompareTag(player2Tag)) isPlayer2Inside = false;

        // LA MAGIA ESTÁ AQUÍ:
        // Si ambos jugadores YA la pisaron en algún momento (p1HasEntered && p2HasEntered)
        // Y ahora mismo NINGUNO de los dos la está pisando (!isPlayer1Inside && !isPlayer2Inside)
        // Significa que ambos ya cruzaron al otro lado. ¡Activamos la trampa!
        if (p1HasEntered && p2HasEntered && !isPlayer1Inside && !isPlayer2Inside)
        {
            ActivateTrap();
        }
    }

    private void ActivateTrap()
    {
        isTriggered = true;

        // 1. Encendemos el muro invisible para impedir el regreso permanentemente
        if (blockerCollider != null) blockerCollider.SetActive(true);

        // 2. Disparamos las partículas
        if (trapVFX != null)
        {
            trapVFX.Play();
            trapVFX.SendEvent("OnTrapTrigger");
        }

        // 3. Destruimos el material del suelo
        if (floorMaterial != null)
        {
            StartCoroutine(AnimateDestruction());
        }
    }

    private IEnumerator AnimateDestruction()
    {
        float currentIntegrity = 1f;

        while (currentIntegrity > destructionLimit)
        {
            currentIntegrity -= Time.deltaTime * destructionSpeed;
            floorMaterial.SetFloat(openAmountID, currentIntegrity);
            yield return null;
        }

        // Se queda en este estado para siempre
        floorMaterial.SetFloat(openAmountID, destructionLimit);
    }
}