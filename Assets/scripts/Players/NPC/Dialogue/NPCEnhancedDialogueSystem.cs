using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(NPCDialogueDataManager))]
public class NPCEnhancedDialogueSystem : MonoBehaviour
{
    #region Inspector Fields

    [Header("References")]
    [SerializeField] private NPCPopupBillboard npcPopup;
    [SerializeField] private PlayerPopupBillboard player1Popup;
    [SerializeField] private PlayerPopupBillboard player2Popup;
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private NPCDialogueDataManager dialogueDataManager;
    [SerializeField] private DialogueCameraController cameraController;

    [Header("Animation Settings")]
    [SerializeField] private string talkParameter = "IsTalking";

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private GameObject interactPromptCanvas;
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;

    [Header("Prompt Visuals")]
    [SerializeField] private Color cooperativeOutlineColor = Color.yellow;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference player1ActionButton;
    [SerializeField] private InputActionReference player2ActionButton;
    [SerializeField] private InputActionReference player1Navigate;
    [SerializeField] private InputActionReference player2Navigate;
    [SerializeField] private InputActionReference player1Submit;
    [SerializeField] private InputActionReference player2Submit;

    [Header("Visual Feedback")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color unselectedColor = Color.white;
    [SerializeField] private bool enableSelectedPulse = true;
    [SerializeField] private float selectedPulseSpeed = 4f;
    [SerializeField] private float selectedPulseAmount = 0.15f;

    [Header("Canvas Arrangement")]
    [SerializeField] private bool autoArrangeCanvases = true;
    [SerializeField] private float minCanvasSeparation = 0.5f;
    [SerializeField] private float separationMultiplier = 1.2f;
    [SerializeField] private float canvasArrangementUpdateRate = 0.1f;

    [Header("Input Settings")]
    [SerializeField] private float navigateCooldown = 0.2f;

    #endregion

    #region Private Fields

    private List<DialogueNode> dialogueNodes = new List<DialogueNode>();
    private int currentNodeIndex = 0;
    private int selectedOptionIndex = 0;
    private bool isDialogueActive = false;
    private bool isAwaitingChoice = false;
    private bool isWaitingForInput = false;

    private string currentPlayerTag;
    private PlayerPopupBillboard currentPlayerPopup;
    private PlayerInput currentPlayerInput;

    private GameObject player1Object;
    private GameObject player2Object;
    private Transform player1Transform;
    private Transform player2Transform;
    private PlayerInput player1Input;
    private PlayerInput player2Input;

    private readonly List<PlayerIdentifier> playersInRange = new List<PlayerIdentifier>();

    private System.Action<InputAction.CallbackContext> onPlayer1StartDialogue;
    private System.Action<InputAction.CallbackContext> onPlayer2StartDialogue;
    private System.Action<InputAction.CallbackContext> onPlayer1Submit;
    private System.Action<InputAction.CallbackContext> onPlayer2Submit;

    private Coroutine canvasArrangementCoroutine;
    private float lastNavigateTime = 0f;

    
    private NPCBehaviorManager npcBehavior;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        CachePlayerReferences();

        
        npcBehavior = GetComponent<NPCBehaviorManager>();
    }

    private void Start()
    {
        HideInteractionPrompt();
    }

    private void OnEnable()
    {
        RegisterInputCallbacks();
    }

    private void OnDisable()
    {
        UnregisterInputCallbacks();
    }

    private void Update()
    {
        if (isAwaitingChoice && isDialogueActive)
        {
            ProcessOptionNavigation();
            RefreshOptionsPulse();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerEnterRange(other);
    }

    private void OnTriggerExit(Collider other)
    {
        HandlePlayerExitRange(other);
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        if (dialogueDataManager == null)
            dialogueDataManager = GetComponent<NPCDialogueDataManager>();

        if (cameraController == null)
            cameraController = FindObjectOfType<DialogueCameraController>();
    }

    private void CachePlayerReferences()
    {
        player1Object = GameObject.FindGameObjectWithTag("Player1");
        player2Object = GameObject.FindGameObjectWithTag("Player2");

        if (player1Object != null)
        {
            player1Transform = player1Object.transform;
            player1Input = player1Object.GetComponent<PlayerInput>();
        }

        if (player2Object != null)
        {
            player2Transform = player2Object.transform;
            player2Input = player2Object.GetComponent<PlayerInput>();
        }
    }

    #endregion

    #region Input Management

    private void RegisterInputCallbacks()
    {
        onPlayer1StartDialogue = ctx => TryStartDialogue("Player1");
        onPlayer2StartDialogue = ctx => TryStartDialogue("Player2");
        onPlayer1Submit = ctx => TryProgressDialogue();
        onPlayer2Submit = ctx => TryProgressDialogue();

        if (player1ActionButton?.action != null)
            player1ActionButton.action.performed += onPlayer1StartDialogue;

        if (player2ActionButton?.action != null)
            player2ActionButton.action.performed += onPlayer2StartDialogue;

        if (player1Submit?.action != null)
            player1Submit.action.performed += onPlayer1Submit;

        if (player2Submit?.action != null)
            player2Submit.action.performed += onPlayer2Submit;
    }

    private void UnregisterInputCallbacks()
    {
        if (player1ActionButton?.action != null && onPlayer1StartDialogue != null)
            player1ActionButton.action.performed -= onPlayer1StartDialogue;

        if (player2ActionButton?.action != null && onPlayer2StartDialogue != null)
            player2ActionButton.action.performed -= onPlayer2StartDialogue;

        if (player1Submit?.action != null && onPlayer1Submit != null)
            player1Submit.action.performed -= onPlayer1Submit;

        if (player2Submit?.action != null && onPlayer2Submit != null)
            player2Submit.action.performed -= onPlayer2Submit;
    }

    #endregion

    #region Player Range Detection

    private void HandlePlayerEnterRange(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();
        if (playerIdentifier == null) return;

        if (!playersInRange.Contains(playerIdentifier))
        {
            playersInRange.Add(playerIdentifier);
            UpdateInteractionPrompt();
        }
    }

    private void HandlePlayerExitRange(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();
        if (playerIdentifier == null) return;

        if (playersInRange.Contains(playerIdentifier))
        {
            playersInRange.Remove(playerIdentifier);
            UpdateInteractionPrompt();
        }
    }

    private void UpdateInteractionPrompt()
    {
        
        bool shouldShowPrompt = playersInRange.Count > 0 && !DialogueManager.IsDialogueActive();

        
        if (npcBehavior != null && npcBehavior.IsFollowing)
        {
            shouldShowPrompt = false;
        }

        ShowInteractionPrompt(shouldShowPrompt);

        if (shouldShowPrompt && interactPromptCanvas != null)
        {
            Color c = PromptVisualHelper.ComputeColor(playersInRange, cooperativeOutlineColor);
            PromptVisualHelper.ApplyToPrompt(interactPromptCanvas, c);
            UpdatePromptButtonVisuals(shouldShowPrompt);
        }
    }

    #endregion

    #region Dialogue Control

    private void TryStartDialogue(string playerTag)
    {
        
        if (npcBehavior != null && npcBehavior.IsFollowing)
            return;

        if (isDialogueActive) return;

        if (!TryGetPlayerContext(playerTag, out Transform playerTransform, out PlayerInput playerInput))
            return;

        if (!IsPlayerInRange(playerTransform))
            return;

        if (DialogueManager.IsDialogueActive())
        {
            ShowPlayerBusyMessage(playerTag);
            return;
        }

        if (dialogueDataManager != null && !dialogueDataManager.HasDialogueAvailable(playerTag))
        {
            return;
        }

        StartDialogue(playerTag, playerTransform, playerInput);
    }

    private void StartDialogue(string playerTag, Transform playerTransform, PlayerInput playerInput)
    {
        DialogueManager.SetDialogueActive(true);
        isDialogueActive = true;

        currentPlayerTag = playerTag;
        currentPlayerPopup = GetPlayerPopup(playerTag);
        currentPlayerInput = playerInput;
        currentPlayerInput.SwitchCurrentActionMap("UI");
        ApplySelectedColorForPlayer(playerTag);

        HideInteractionPrompt();

        npcPopup?.SetActivePlayer(playerTag);

        cameraController?.StartDialogueCamera(transform, playerTransform);

        if (autoArrangeCanvases)
        {
            StartCanvasArrangement();
        }

        LoadDialogueForPlayer(playerTag);
        currentNodeIndex = 0;

        SetNPCTalkingState(true);

        DisplayCurrentNode();
    }

    private void LoadDialogueForPlayer(string playerTag)
    {
        if (dialogueDataManager != null)
        {
            dialogueNodes = dialogueDataManager.GetDialogueForPlayer(playerTag);
        }
        else
        {
            dialogueNodes = new List<DialogueNode>();
        }

        if (dialogueNodes == null)
        {
            dialogueNodes = new List<DialogueNode>();
        }
    }

    private void TryProgressDialogue()
    {
        if (!isDialogueActive) return;

        if (isWaitingForInput && !isAwaitingChoice)
        {
            isWaitingForInput = false;
            AdvanceToNextNode();
            return;
        }

        if (isAwaitingChoice)
        {
            SelectOption(selectedOptionIndex);
            return;
        }
    }

    public void EndDialogue()
    {
        StopCanvasArrangement();
        HideAllPopups();
        RestoreCanvasPositions();

        DialogueManager.SetDialogueActive(false);
        isDialogueActive = false;
        isAwaitingChoice = false;
        isWaitingForInput = false;
        currentNodeIndex = 0;

        cameraController?.EndDialogueCamera();

        SetNPCTalkingState(false);

        if (currentPlayerInput != null)
            currentPlayerInput.SwitchCurrentActionMap("Player");

        string playerTagToEnd = currentPlayerTag;

        currentPlayerInput = null;
        currentPlayerTag = "";
        currentPlayerPopup = null;

        if (dialogueDataManager != null && !string.IsNullOrEmpty(playerTagToEnd))
        {
            dialogueDataManager.CompleteCurrentDialogue(playerTagToEnd);
        }

        UpdateInteractionPrompt();
    }

    #endregion

    #region Node Display

    private void DisplayCurrentNode()
    {
        if (!IsValidNodeIndex(currentNodeIndex))
        {
            EndDialogue();
            return;
        }

        DialogueNode node = dialogueNodes[currentNodeIndex];
        isAwaitingChoice = false;
        isWaitingForInput = false;

        if (node.isNPC)
        {
            ShowNPCDialogue(node.line);
        }
        else
        {
            ShowPlayerDialogue(node.line);
        }

        isWaitingForInput = true;
    }

    private void ShowNPCDialogue(string message)
    {
        currentPlayerPopup?.Hide();
        npcPopup?.ShowMessage(message, 999f);
        SetNPCTalkingState(true);
    }

    private void ShowPlayerDialogue(string message)
    {
        npcPopup?.Hide();
        currentPlayerPopup?.ShowMessage(message, 999f);
        SetNPCTalkingState(false);
    }

    #endregion

    #region Option Management

    private void ProcessOptionNavigation()
    {
        if (Time.time - lastNavigateTime < navigateCooldown) return;

        Vector2 navigationInput = GetNavigationInput();

        if (Mathf.Abs(navigationInput.y) > 0.3f)
        {
            int direction = navigationInput.y > 0 ? -1 : 1;
            MoveOptionSelection(direction);
            lastNavigateTime = Time.time;
        }
    }

    private Vector2 GetNavigationInput()
    {
        Vector2 input = Vector2.zero;

        if (currentPlayerTag == "Player1" && player1Navigate != null && player1Navigate.action != null)
        {
            input = player1Navigate.action.ReadValue<Vector2>();
        }
        else if (currentPlayerTag == "Player2" && player2Navigate != null && player2Navigate.action != null)
        {
            input = player2Navigate.action.ReadValue<Vector2>();
        }

        return input;
    }

    private void MoveOptionSelection(int direction)
    {
        if (!IsValidNodeIndex(currentNodeIndex)) return;

        DialogueNode node = dialogueNodes[currentNodeIndex];
        if (node.options == null || node.options.Count == 0) return;

        selectedOptionIndex = Mathf.Clamp(
            selectedOptionIndex + direction,
            0,
            node.options.Count - 1
        );

        DisplayOptions(node);
    }

    private void DisplayOptions(DialogueNode node)
    {
        if (node.options == null || node.options.Count == 0) return;

        string optionsText = BuildOptionsText(node.options);
        currentPlayerPopup?.UpdateTextInstant(optionsText);
    }

    private string BuildOptionsText(List<DialogueOption> options)
    {
        Color selColor = GetPulsedSelectedColor();
        string selectedHex = ColorUtility.ToHtmlStringRGB(selColor);
        string unselectedHex = ColorUtility.ToHtmlStringRGB(unselectedColor);
        string result = "";

        for (int i = 0; i < options.Count; i++)
        {
            bool isSelected = i == selectedOptionIndex;
            string colorHex = isSelected ? selectedHex : unselectedHex;
            string prefix = isSelected ? "> " : "  ";

            result += $"<color=#{colorHex}>{prefix}{options[i].optionText}</color>\n";
        }

        return result;
    }

    private void RefreshOptionsPulse()
    {
        if (!IsValidNodeIndex(currentNodeIndex)) return;
        DialogueNode node = dialogueNodes[currentNodeIndex];
        if (node.options == null || node.options.Count == 0) return;
        string optionsText = BuildOptionsText(node.options);
        currentPlayerPopup?.UpdateTextInstant(optionsText);
    }

    private void ApplySelectedColorForPlayer(string playerTag)
    {
        if (playerTag == "Player1" && player1Object != null)
        {
            var id = player1Object.GetComponent<PlayerIdentifier>();
            if (id != null) selectedColor = id.PlayerOutlineColor;
        }
        else if (playerTag == "Player2" && player2Object != null)
        {
            var id = player2Object.GetComponent<PlayerIdentifier>();
            if (id != null) selectedColor = id.PlayerOutlineColor;
        }
        else
        {
            GameObject obj = GameObject.FindGameObjectWithTag(playerTag);
            var id = obj != null ? obj.GetComponent<PlayerIdentifier>() : null;
            if (id != null) selectedColor = id.PlayerOutlineColor;
        }
    }

    private Color GetPulsedSelectedColor()
    {
        if (!enableSelectedPulse) return selectedColor;
        float p = (Mathf.Sin(Time.time * selectedPulseSpeed) * 0.5f + 0.5f) * selectedPulseAmount;
        Color.RGBToHSV(selectedColor, out float h, out float s, out float v);
        float nv = Mathf.Clamp01(v + p);
        return Color.HSVToRGB(h, s, nv);
    }

    private void SelectOption(int optionIndex)
    {
        if (!IsValidNodeIndex(currentNodeIndex))
        {
            EndDialogue();
            return;
        }

        DialogueNode node = dialogueNodes[currentNodeIndex];

        if (node.options == null || optionIndex < 0 || optionIndex >= node.options.Count)
        {
            EndDialogue();
            return;
        }

        
        string flagName = node.options[optionIndex].flagToTrigger;
        if (!string.IsNullOrEmpty(flagName) && dialogueDataManager != null)
        {
            dialogueDataManager.SetFlag(currentPlayerTag, flagName);

        }

        int nextNodeIndex = node.options[optionIndex].nextNodeIndex;

        
        if (nextNodeIndex < 0 || nextNodeIndex >= dialogueNodes.Count)
        {
            EndDialogue();
        }
        else
        {
            currentNodeIndex = nextNodeIndex;
            isAwaitingChoice = false;
            DisplayCurrentNode();
        }
    }

    #endregion

    #region Dialogue Advancement

    private void AdvanceToNextNode()
    {
        if (dialogueNodes == null)
        {
            EndDialogue();
            return;
        }

        DialogueNode node = dialogueNodes[currentNodeIndex];

        if (HasOptions(node))
        {
            isAwaitingChoice = true;
            selectedOptionIndex = 0;
            DisplayOptions(node);
            return;
        }

        currentNodeIndex++;

        if (currentNodeIndex >= dialogueNodes.Count)
        {
            EndDialogue();
        }
        else
        {
            DisplayCurrentNode();
        }
    }

    private bool HasOptions(DialogueNode node)
    {
        return node.options != null && node.options.Count > 0;
    }

    #endregion

    #region Canvas Arrangement

    private void StartCanvasArrangement()
    {
        if (npcPopup == null || currentPlayerPopup == null) return;

        StopCanvasArrangement();
        canvasArrangementCoroutine = StartCoroutine(ArrangeCanvasesRoutine());
    }

    private void StopCanvasArrangement()
    {
        if (canvasArrangementCoroutine != null)
        {
            StopCoroutine(canvasArrangementCoroutine);
            canvasArrangementCoroutine = null;
        }
    }

    private IEnumerator ArrangeCanvasesRoutine()
    {
        yield return null;

        WaitForSeconds wait = new WaitForSeconds(canvasArrangementUpdateRate);

        while (isDialogueActive)
        {
            if (npcPopup == null || currentPlayerPopup == null) yield break;

            Camera mainCamera = Camera.main;
            if (mainCamera == null) yield break;

            GameObject playerObject = GameObject.FindGameObjectWithTag(currentPlayerTag);
            if (playerObject == null) yield break;

            CalculateAndApplyCanvasOffsets(mainCamera, playerObject.transform);

            yield return wait;
        }
    }

    private void CalculateAndApplyCanvasOffsets(Camera mainCamera, Transform playerTransform)
    {
        Vector3 npcPosition = transform.position;
        Vector3 playerPosition = playerTransform.position;

        Vector2 npcCanvasSize = npcPopup.GetCanvasWorldSize();
        Vector2 playerCanvasSize = currentPlayerPopup.GetCanvasWorldSize();

        Vector3 npcToPlayer = playerPosition - npcPosition;
        npcToPlayer.y = 0f;

        if (npcToPlayer.magnitude < 0.01f) return;

        npcToPlayer.Normalize();

        Vector3 cameraRight = mainCamera.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        float dotProduct = Vector3.Dot(cameraRight, npcToPlayer);

        float requiredSeparation = CalculateRequiredSeparation(npcCanvasSize, playerCanvasSize);

        if (dotProduct > 0f)
        {
            npcPopup.SetLateralOffset(-requiredSeparation);
            currentPlayerPopup.SetLateralOffset(requiredSeparation);
        }
        else
        {
            npcPopup.SetLateralOffset(requiredSeparation);
            currentPlayerPopup.SetLateralOffset(-requiredSeparation);
        }
    }

    private float CalculateRequiredSeparation(Vector2 npcSize, Vector2 playerSize)
    {
        float baseSeparation = (npcSize.x * 0.5f) + (playerSize.x * 0.5f) + minCanvasSeparation;
        return baseSeparation * separationMultiplier;
    }

    private void RestoreCanvasPositions()
    {
        if (autoArrangeCanvases)
        {
            npcPopup?.SetLateralOffset(0f);
            player1Popup?.SetLateralOffset(0f);
            player2Popup?.SetLateralOffset(0f);
        }
    }

    #endregion

    #region Helper Methods

    private bool TryGetPlayerContext(string playerTag, out Transform playerTransform, out PlayerInput playerInput)
    {
        if (playerTag == "Player1")
        {
            playerTransform = player1Transform;
            playerInput = player1Input;
            return playerTransform != null && playerInput != null;
        }

        if (playerTag == "Player2")
        {
            playerTransform = player2Transform;
            playerInput = player2Input;
            return playerTransform != null && playerInput != null;
        }

        playerTransform = null;
        playerInput = null;
        return false;
    }

    private bool IsPlayerInRange(Transform playerTransform)
    {
        if (playerTransform == null) return false;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionRange;
    }

    private PlayerPopupBillboard GetPlayerPopup(string playerTag)
    {
        return playerTag == "Player1" ? player1Popup : player2Popup;
    }

    private void ShowPlayerBusyMessage(string playerTag)
    {
        PlayerPopupBillboard popup = GetPlayerPopup(playerTag);
        popup?.ShowMessage("The other player is talking...", 1.5f);
    }

    private bool IsValidNodeIndex(int index)
    {
        return dialogueNodes != null && index >= 0 && index < dialogueNodes.Count;
    }

    private void SetNPCTalkingState(bool isTalking)
    {
        if (npcAnimator != null && AnimatorHasParameter(npcAnimator, talkParameter))
        {
            npcAnimator.SetBool(talkParameter, isTalking);
        }
    }

    private bool AnimatorHasParameter(Animator animator, string parameterName)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
            return false;

        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
    }

    private void HideAllPopups()
    {
        currentPlayerPopup?.Hide();
        npcPopup?.Hide();
        player1Popup?.Hide();
        player2Popup?.Hide();
    }

    private void ShowInteractionPrompt(bool show)
    {
        if (interactPromptCanvas != null)
        {
            interactPromptCanvas.SetActive(show);
            UpdatePromptButtonVisuals(show);
        }
    }

    private void HideInteractionPrompt()
    {
        ShowInteractionPrompt(false);
    }

    private void UpdatePromptButtonVisuals(bool show)
    {
        if (promptButtonImage == null) return;
        bool active = show && interactPromptCanvas != null && interactPromptCanvas.activeSelf;
        promptButtonImage.gameObject.SetActive(active);
        if (active)
        {
            if (buttonAnchor != null)
                promptButtonImage.rectTransform.position = buttonAnchor.position;
            else
                promptButtonImage.rectTransform.anchoredPosition = buttonImageOffset;
        }
    }

    #endregion
}
