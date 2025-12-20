using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events; 

public class PuertaDobleConLlave : MonoBehaviour
{
    [Header("Door Configuration")]
    [Tooltip("El Transform de la primera hoja de la puerta que rotara.")]
    [SerializeField] private Transform puertaA;
    [Tooltip("El Transform de la segunda hoja de la puerta que rotara.")]
    [SerializeField] private Transform puertaB;

    [SerializeField] private float anguloApertura = 90f;
    [SerializeField] private float velocidadApertura = 3f;
    [Tooltip("El eje sobre el que rotaran las puertas. Usualmente Vector3.up (0, 1, 0).")]
    [SerializeField] private Vector3 ejeRotacion = Vector3.up;

    [Header("Key Configuration")]
    [Tooltip("El ID de la Key Card requerida en el inventario del jugador.")]
    [SerializeField] private string keyCardIDRequerida = "Llave";
    [SerializeField] private bool consumirLlave = true;

    [Header("UI Feedback")]
    [SerializeField] private string mensajeExito = "Puerta abierta. La llave se ha roto.";
    [SerializeField] private string mensajeFallo = "Necesitamos la llave ";

    [Header("Audio")]
    [SerializeField] private bool playDoorSounds = true;

    [Header("Outline Multiplayer")]
    [Tooltip("El color usado cuando dos o mas jugadores estan en el trigger.")]
    [SerializeField] private Color cooperativeOutlineColor = Color.yellow;

    [Tooltip("El nombre de la propiedad 'Color' en el Shader Graph.")]
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [Tooltip("El nombre de la propiedad 'Scale' en el Shader Graph.")]
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;

    [Header("Eventos de Escena")]
    [Tooltip("Arrastra aqui el NPC y selecciona la funcion NPCBehaviorManager.RunToDoorAndVanish")]
    public UnityEvent OnDoorOpened; 

    private List<PlayerIdentifier> activePlayers = new List<PlayerIdentifier>();
    private List<Renderer> doorRenderers = new List<Renderer>();
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;

    private Collider puertaCollider;

    private bool estaAbierta = false;
    private float anguloObjetivo = 0f;
    private float anguloActual = 0f;

    void Start()
    {
        puertaCollider = GetComponent<Collider>();

        if (puertaA != null)
        {
            Renderer rendererA = puertaA.GetComponent<Renderer>();
            if (rendererA != null) doorRenderers.Add(rendererA);
        }
        if (puertaB != null)
        {
            Renderer rendererB = puertaB.GetComponent<Renderer>();
            if (rendererB != null) doorRenderers.Add(rendererB);
        }

        if (doorRenderers.Count > 0)
        {
            propertyBlock = new MaterialPropertyBlock();

            outlineColorID = Shader.PropertyToID(outlineColorProperty);
            outlineScaleID = Shader.PropertyToID(outlineScaleProperty);

            SetOutlineState(Color.black, 0.0f);
        }
    }

    void Update()
    {
        if (estaAbierta)
        {
            anguloActual = Mathf.MoveTowards(anguloActual, anguloObjetivo, velocidadApertura * Time.deltaTime);

            float currentEulerY = puertaA.localRotation.eulerAngles.y;
            float normalizedCurrentY = (currentEulerY > 180f) ? currentEulerY - 360f : currentEulerY;
            float deltaAngulo = anguloActual - normalizedCurrentY;

            puertaA.Rotate(ejeRotacion, deltaAngulo, Space.Self);
            puertaB.Rotate(ejeRotacion, -deltaAngulo, Space.Self);
        }
    }

    private void SetOutlineState(Color color, float scale)
    {
        if (propertyBlock == null) return;

        foreach (Renderer rend in doorRenderers)
        {
            if (rend != null)
            {
                if (rend.sharedMaterials.Length < 2)
                    continue;

                rend.GetPropertyBlock(propertyBlock, 1);
                propertyBlock.SetColor(outlineColorID, color);
                propertyBlock.SetFloat(outlineScaleID, scale);
                rend.SetPropertyBlock(propertyBlock, 1);
            }
        }
    }

    private void UpdateOutlineVisuals()
    {
        if (activePlayers.Count == 0)
        {
            SetOutlineState(originalOutlineColor, 0.0f);
        }
        else if (activePlayers.Count == 1)
        {
            PlayerIdentifier singlePlayer = activePlayers[0];
            SetOutlineState(singlePlayer.PlayerOutlineColor, activeOutlineScale);
        }
        else
        {
            SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            if (!activePlayers.Contains(playerIdentifier))
            {
                activePlayers.Add(playerIdentifier);
            }
            UpdateOutlineVisuals();

            PlayerInventory inv = other.GetComponent<PlayerInventory>();
            bool hasKey = inv != null && inv.HasKeyCard(keyCardIDRequerida);
            DialogueManager.ShowKeyDoorEnterDialogue(other.gameObject, hasKey);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            if (activePlayers.Contains(playerIdentifier))
            {
                activePlayers.Remove(playerIdentifier);
            }
            UpdateOutlineVisuals();
        }
    }

    public void IntentoAbrirPuerta(MonoBehaviour playerScript)
    {
        if (estaAbierta) return;

        PlayerInventory inventory = playerScript.GetComponent<PlayerInventory>();
        PlayerUIController uiController = GetPlayerUIController(playerScript.gameObject);

        if (inventory != null && inventory.HasKeyCard(keyCardIDRequerida))
        {
            if (consumirLlave)
            {
                inventory.UseKeyCard(keyCardIDRequerida);
            }

            AbrirPuerta();

            if (uiController != null)
            {
                uiController.ShowNotification(mensajeExito);
            }
        }
        else
        {
            if (playDoorSounds)
            {
                if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig().doorOpenSounds.Length > 0)
                {
                    AudioClip doorClip = AudioManager.Instance.GetAudioConfig().doorOpenSounds[0];
                    AudioManager.Instance.PlaySFX(doorClip, transform.position, 0.7f, 0.9f);
                }
            }

            if (uiController != null)
            {
                uiController.ShowNotification(mensajeFallo + keyCardIDRequerida);
            }
        }
    }

    private void AbrirPuerta()
    {
        estaAbierta = true;
        anguloObjetivo = anguloApertura;

        if (playDoorSounds)
        {
            if (AudioManager.Instance != null && AudioManager.Instance.GetAudioConfig().doorOpenSounds.Length > 0)
            {
                AudioClip doorOpenClip = AudioManager.Instance.GetAudioConfig().doorOpenSounds[0];
                AudioManager.Instance.PlaySFX(doorOpenClip, transform.position, 0.8f, 1f);
            }
        }

        if (puertaCollider != null)
        {
            puertaCollider.enabled = false;
        }

        SetOutlineState(Color.black, 0.0f);

        
        
        if (OnDoorOpened != null)
        {
            OnDoorOpened.Invoke();
        }
    }

    private PlayerUIController GetPlayerUIController(GameObject playerObject)
    {
        if (playerObject == null) return null;
        return playerObject.GetComponent<PlayerUIController>();
    }
}