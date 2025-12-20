using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using System.Collections; // <<<< NECESARIO PARA LAS COROUTINES

public class HotbarController : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInventory playerInventory;
    public PlayerInput playerInput;
    // Asigna el CanvasGroup del objeto raíz de la Hotbar aquí en el Inspector
    public CanvasGroup hotBarCanvasGroup;

    [Header("UI Slots")]
    public Image[] slots;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    [Header("Control de Fade (Opacidad)")]
    public float fadeDuration = 0.5f;       // Duración de la transición (ej. 0.5 segundos)
    public float fadedAlpha = 0.1f;         // Opacidad cuando está inactivo (ej. 0.1)
    public float normalAlpha = 1f;          // Opacidad cuando está activo (1.0)
    public float fadeDelay = 3f;            // Segundos antes de que regrese al fade

    [Header("Input Actions (New Input System)")]
    public InputActionReference moveRightAction;
    public InputActionReference moveLeftAction;
    public InputActionReference analyzeAction;

    [Header("World Space Canvas References")]
    [Tooltip("Canvas que se muestra al analizar la tarjeta.")]
    public GameObject cardCanvas;

    [Tooltip("Campo opcional: asigna aquí el texto del jugador (TextMeshProUGUI o Text) en el canvas world space.")]
    public TextMeshProUGUI playerDialogueText;

    private int selectedIndex = 0;
    private bool isCardCanvasOpen = false;
    private float lastInteractionTime; // Temporizador de inactividad

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        // Intentar encontrar automáticamente el texto del jugador si no está asignado
        if (playerDialogueText == null)
            playerDialogueText = GetComponentInChildren<TextMeshProUGUI>(true);

        // Inicializar el temporizador y establecer el estado inicial de la Hotbar en fade.
        lastInteractionTime = Time.time;
        SetAlpha(fadedAlpha);
    }

    private void OnEnable()
    {
        if (moveRightAction != null)
            moveRightAction.action.performed += OnMoveRight;
        if (moveLeftAction != null)
            moveLeftAction.action.performed += OnMoveLeft;
        if (analyzeAction != null)
            analyzeAction.action.performed += OnAnalyzeItem;

        if (moveRightAction != null) moveRightAction.action.Enable();
        if (moveLeftAction != null) moveLeftAction.action.Enable();
        if (analyzeAction != null) analyzeAction.action.Enable();

        UpdateSlotSelection();
    }

    private void OnDisable()
    {
        if (moveRightAction != null)
            moveRightAction.action.performed -= OnMoveRight;
        if (moveLeftAction != null)
            moveLeftAction.action.performed -= OnMoveLeft;
        if (analyzeAction != null)
            analyzeAction.action.performed -= OnAnalyzeItem;

        if (moveRightAction != null) moveRightAction.action.Disable();
        if (moveLeftAction != null) moveLeftAction.action.Disable();
        if (analyzeAction != null) analyzeAction.action.Disable();
    }

    // ======================================================================
    // Lógica de Fade y Temporizador
    // ======================================================================

    private void Update()
    {
        // Vuelve al fade si ha pasado el tiempo sin interacción y actualmente está visible.
        if (hotBarCanvasGroup != null && hotBarCanvasGroup.alpha == normalAlpha && Time.time > lastInteractionTime + fadeDelay)
        {
            StartFade(fadedAlpha);
        }
    }

    // Método para ser llamado en cada acción que queremos que muestre la Hotbar.
    public void RegisterInteraction()
    {
        // 1. Asegura que la hotbar se haga visible (FadeIn)
        StartFade(normalAlpha);

        // 2. Reinicia el temporizador de inactividad
        lastInteractionTime = Time.time;
    }

    private void StartFade(float targetAlpha)
    {
        if (hotBarCanvasGroup == null) return;

        // Detiene cualquier fade anterior para iniciar el nuevo inmediatamente
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(targetAlpha));
    }

    // Coroutine para el desvanecimiento suave (fade)
    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        float startAlpha = hotBarCanvasGroup.alpha;
        float elapsedTime = 0f;

        // Permite la interacción cuando va a ser visible
        if (targetAlpha == normalAlpha)
        {
            hotBarCanvasGroup.interactable = true;
            hotBarCanvasGroup.blocksRaycasts = true;
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            hotBarCanvasGroup.alpha = newAlpha;
            yield return null;
        }

        hotBarCanvasGroup.alpha = targetAlpha;

        // Bloquear la interacción cuando vuelve al fade
        if (targetAlpha == fadedAlpha)
        {
            hotBarCanvasGroup.interactable = false;
            hotBarCanvasGroup.blocksRaycasts = false;
        }
    }

    // Establece la opacidad instantáneamente (útil para el Awake)
    private void SetAlpha(float alpha)
    {
        if (hotBarCanvasGroup == null) return;
        hotBarCanvasGroup.alpha = alpha;
        hotBarCanvasGroup.interactable = alpha >= normalAlpha;
        hotBarCanvasGroup.blocksRaycasts = alpha >= normalAlpha;
    }

    // ======================================================================
    // Lógica de Input
    // ======================================================================

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        // REGISTRA LA INTERACCIÓN
        RegisterInteraction();

        selectedIndex = (selectedIndex + 1) % slots.Length;
        UpdateSlotSelection();
        CloseCardCanvas();
    }

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        // REGISTRA LA INTERACCIÓN
        RegisterInteraction();

        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = slots.Length - 1;
        UpdateSlotSelection();
        CloseCardCanvas();
    }

    private void OnAnalyzeItem(InputAction.CallbackContext ctx)
    {
        // REGISTRA LA INTERACCIÓN
        RegisterInteraction();

        if (playerInventory == null) return;

        var allItems = new List<string>();
        allItems.AddRange(playerInventory.GetCollectedItems());
        allItems.AddRange(playerInventory.GetCollectedKeyCards());

        // --- MODIFICACIÓN: Chequeo de slot vacío ---
        // Si el índice seleccionado es mayor o igual a la cantidad de ítems, el slot está vacío.
        if (selectedIndex >= allItems.Count)
        {
            ShowPlayerNotification("Nothing to analyze here.");
            return; // Detenemos la ejecución aquí.
        }

        string selectedItem = allItems[selectedIndex];

        switch (selectedItem)
        {
            case "Card":
                ToggleCardCanvas();
                break;

            case "Lever":
                ShowPlayerNotification("This might help me cut the electricity.");
                break;

            case "Key":
                ShowPlayerNotification("A key... I wonder what it opens.");
                break;

            default:
                // Caso por defecto para ítems recogidos que no tienen un análisis específico.
                ShowPlayerNotification("Cannot analyze this item right now.");
                break;
        }
    }

    private void ToggleCardCanvas()
    {
        if (cardCanvas == null)
        {
            Debug.LogWarning("No se asignó el canvas de la tarjeta en el inspector.");
            return;
        }

        isCardCanvasOpen = !isCardCanvasOpen;
        cardCanvas.SetActive(isCardCanvasOpen);

        Debug.Log(isCardCanvasOpen ? "Canvas de tarjeta abierto" : "Canvas de tarjeta cerrado");
    }

    private void CloseCardCanvas()
    {
        if (cardCanvas != null && isCardCanvasOpen)
        {
            cardCanvas.SetActive(false);
            isCardCanvasOpen = false;
        }
    }

    private void ShowPlayerNotification(string message)
    {
        if (playerDialogueText != null)
        {
            playerDialogueText.text = message;
            return;
        }

        PlayerUIController uiController = GetComponentInParent<PlayerUIController>();
        if (uiController != null)
        {
            uiController.ShowNotification(message);
        }
        else
        {
            Debug.Log($"[Notification]: {message}");
        }
    }

    private void UpdateSlotSelection()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    public int GetSelectedIndex() => selectedIndex;

}