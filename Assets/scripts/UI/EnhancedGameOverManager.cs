using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class EnhancedGameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas gameOverCanvas;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button retryButton;
    public Button quitButton;
    private CanvasGroup panelCanvasGroup;
    private CanvasGroup textCanvasGroup;
    private CanvasGroup buttonsCanvasGroup;
    
    [Header("Font Settings")]
    public TMP_FontAsset larkeSansFont;
    public float gameOverTextFontSize = 96f;
    public float buttonFontSize = 48f;
    
    [Header("Texture Settings")]
    public Texture2D textFaceTexture;
    public bool useTextTexture = true;
    public Vector2 textureScale = Vector2.one;
    
    [Header("Visual Settings")]
    public Color panelColor = new Color(0, 0, 0, 0.95f);
    public Color gameOverTextColor = new Color(0.8f, 0.05f, 0.05f, 1f);
    public Color buttonNormalColor = new Color(0.15f, 0.05f, 0.05f, 0.9f);
    public Color buttonHighlightedColor = new Color(0.3f, 0.1f, 0.1f, 1f);
    public Color buttonPressedColor = new Color(0.05f, 0.02f, 0.02f, 1f);
    public Color buttonSelectedColor = new Color(0.5f, 0.15f, 0.15f, 1f);
    public Color buttonTextColor = new Color(0.9f, 0.8f, 0.8f, 1f);
    
    [Header("Animation Settings")]
    public float fadeInDuration = 3.5f;
    public float textFadeInDuration = 1.5f;
    public float buttonFadeInDuration = 0.2f;
    public float buttonTransitionSpeed = 0.2f;
    public float buttonHoverScale = 1.05f;
    public float textPulseSpeed = 2f;
    public float textPulseIntensity = 0.15f;
    
    [Header("Audio Settings")]
    public AudioClip horrorAmbientSound;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f, 1f)]
    public float ambientVolume = 0.4f;
    [Range(0f, 1f)]
    public float hoverVolume = 0.5f;
    [Range(0f, 1f)]
    public float clickVolume = 0.7f;
    private AudioSource ambientAudioSource;
    private AudioSource uiAudioSource;
    
    [Header("Haptic Settings")]
    public float navigationHapticDuration = 0.1f;
    public float selectionHapticDuration = 0.2f;
    public float navigationHapticIntensity = 0.3f;
    public float selectionHapticIntensity = 0.6f;
    
    [Header("Input System")]
    public InputActionAsset menuInputActions;
    private InputAction navigateAction;
    private InputAction submitAction;
    
    [Header("Text Movement Settings")]
    public float textMoveSpeed = 500f;
    public float textMovementSmoothing = 0.1f;
    public float textMovementEasing = 0.05f;
    public float screenEdgePadding = 100f;
    private Vector2 textMovementInput;
    private Vector2 currentTextVelocity;
    private Vector3 originalTextPosition;
    private RectTransform textRectTransform;
    private bool textMovementEnabled = false;
    
    [Header("Player Detection")]
    private List<PlayerHealth> playerHealthComponents = new List<PlayerHealth>();
    private int deadPlayersCount = 0;
    private bool gameOverTriggered = false;
    
    private int selectedButtonIndex = 0;
    private Button[] buttons;
    private Vector3[] buttonOriginalScales;
    private float navigationCooldown = 0f;
    private const float NAVIGATION_DELAY = 0.2f;
    
    private EventSystem eventSystem;
    private Gamepad currentGamepad;
    
    private static EnhancedGameOverManager instance;
    public static EnhancedGameOverManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EnhancedGameOverManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EnhancedGameOverManager");
                    instance = go.AddComponent<EnhancedGameOverManager>();
                }
            }
            return instance;
        }
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Reset and Find Resources")]
    public void ResetAndFindResources()
    {
        if (larkeSansFont == null)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("Larke Sans Bold SDF t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                larkeSansFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                Debug.Log($"[EnhancedGameOverManager] Found font at: {path}");
            }
        }
        
        // Find a default texture if none assigned
        if (textFaceTexture == null)
        {
            // Optional: find a default noise or grunge texture
        }
    }
    #endif

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        if (!ValidateResources())
        {
            Debug.LogError("[EnhancedGameOverManager] Resource validation failed. Some UI elements may not display correctly.");
        }
        
        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = ambientVolume;
        ambientAudioSource.ignoreListenerPause = true;
        
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;
        uiAudioSource.ignoreListenerPause = true;
        
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            eventSystem = esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
        
        SetupInputActions();
        CreateGameOverUI();
        HideGameOver();
    }
    
    private bool ValidateResources()
    {
        bool allValid = true;
        
        if (larkeSansFont == null)
        {
            // Try to find it in the project
            Debug.LogWarning("[EnhancedGameOverManager] Larke Sans font not assigned. Attempting to locate...");
            // In a real scenario, we might use Resources.Load if it was there, 
            // but since it's not, we'll just log and return false.
            allValid = false;
        }
        
        if (useTextTexture && textFaceTexture == null)
        {
            Debug.LogWarning("[EnhancedGameOverManager] Text Face Texture is enabled but no texture is assigned.");
            // Not strictly fatal, but good to know
        }
        
        return allValid;
    }
    
    private void Start()
    {
        FindAndSubscribeToPlayers();
    }
    
    private void SetupInputActions()
    {
        if (menuInputActions == null)
        {
            menuInputActions = Resources.Load<InputActionAsset>("Input/MenuNavigation");
            if (menuInputActions == null)
            {
                Debug.LogWarning("[EnhancedGameOverManager] MenuNavigation InputActionAsset not found in Resources/Input/");
                return;
            }
        }
        
        var menuMap = menuInputActions.FindActionMap("Menu");
        if (menuMap != null)
        {
            navigateAction = menuMap.FindAction("Navigate");
            submitAction = menuMap.FindAction("Submit");
        }
    }
    
    private void OnEnable()
    {
        if (navigateAction != null)
        {
            navigateAction.Enable();
            navigateAction.performed += OnNavigate;
        }
        if (submitAction != null)
        {
            submitAction.Enable();
            submitAction.performed += OnSubmit;
        }
    }
    
    private void OnDisable()
    {
        if (navigateAction != null)
        {
            navigateAction.performed -= OnNavigate;
            navigateAction.Disable();
        }
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmit;
            submitAction.Disable();
        }
    }
    
    private void Update()
    {
        if (!gameOverTriggered) return;
        
        if (navigationCooldown > 0f)
        {
            navigationCooldown -= Time.unscaledDeltaTime;
        }
        
        if (Gamepad.current != null && currentGamepad != Gamepad.current)
        {
            currentGamepad = Gamepad.current;
        }
        
        // Handle text movement with joystick
        if (textMovementEnabled && textRectTransform != null && currentGamepad != null)
        {
            HandleTextMovement();
        }
        
        if (currentGamepad != null && navigationCooldown <= 0f)
        {
            Vector2 leftStick = currentGamepad.leftStick.ReadValue();
            
            // Check if stick is being used for text movement (vertical input)
            if (Mathf.Abs(leftStick.y) > 0.3f && textMovementEnabled)
            {
                // Text movement takes priority over button navigation
                return;
            }
            
            if (Mathf.Abs(leftStick.y) > 0.5f)
            {
                int previousIndex = selectedButtonIndex;
                
                if (leftStick.y > 0.5f)
                {
                    selectedButtonIndex--;
                    if (selectedButtonIndex < 0) selectedButtonIndex = buttons.Length - 1;
                }
                else if (leftStick.y < -0.5f)
                {
                    selectedButtonIndex++;
                    if (selectedButtonIndex >= buttons.Length) selectedButtonIndex = 0;
                }
                
                if (previousIndex != selectedButtonIndex)
                {
                    UpdateButtonSelection();
                    PlayHoverSound();
                    TriggerHapticFeedback(navigationHapticIntensity, 0f, navigationHapticDuration);
                    navigationCooldown = NAVIGATION_DELAY;
                }
            }
            
            if (currentGamepad.buttonSouth.wasPressedThisFrame)
            {
                ExecuteSelection();
            }
        }
    }
    
    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!gameOverTriggered || navigationCooldown > 0f) return;
        
        Vector2 input = context.ReadValue<Vector2>();
        
        if (Mathf.Abs(input.y) > 0.5f)
        {
            int previousIndex = selectedButtonIndex;
            
            if (input.y > 0.5f)
            {
                selectedButtonIndex--;
                if (selectedButtonIndex < 0) selectedButtonIndex = buttons.Length - 1;
            }
            else if (input.y < -0.5f)
            {
                selectedButtonIndex++;
                if (selectedButtonIndex >= buttons.Length) selectedButtonIndex = 0;
            }
            
            if (previousIndex != selectedButtonIndex)
            {
                UpdateButtonSelection();
                PlayHoverSound();
                TriggerHapticFeedback(navigationHapticIntensity, 0f, navigationHapticDuration);
                navigationCooldown = NAVIGATION_DELAY;
            }
        }
    }
    
    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!gameOverTriggered) return;
        
        // Only allow selection via Gamepad buttonSouth (A button)
        if (context.control.device is Gamepad && context.control.name.Contains("buttonSouth"))
        {
            ExecuteSelection();
        }
    }
    
    private void ExecuteSelection()
    {
        PlayClickSound();
        TriggerHapticFeedback(selectionHapticIntensity, selectionHapticIntensity, selectionHapticDuration);
        
        if (buttons != null && selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Length)
        {
            buttons[selectedButtonIndex].onClick.Invoke();
        }
    }
    
    private void UpdateButtonSelection()
    {
        if (buttons == null) return;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == selectedButtonIndex)
            {
                StartCoroutine(AnimateButtonHover(buttons[i], true));
                eventSystem.SetSelectedGameObject(buttons[i].gameObject);
            }
            else
            {
                StartCoroutine(AnimateButtonHover(buttons[i], false));
            }
        }
    }
    
    private IEnumerator AnimateButtonHover(Button button, bool hovered)
    {
        if (button == null) yield break;
        
        int buttonIndex = System.Array.IndexOf(buttons, button);
        if (buttonIndex < 0) yield break;
        
        CanvasGroup buttonGroup = button.GetComponent<CanvasGroup>();
        if (buttonGroup == null)
        {
            buttonGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        
        float targetAlpha = hovered ? 1f : 0.7f;
        
        float elapsed = 0f;
        float startAlpha = buttonGroup.alpha;
        
        while (elapsed < buttonTransitionSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / buttonTransitionSpeed;
            
            buttonGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            yield return null;
        }
        
        buttonGroup.alpha = targetAlpha;
    }
    
    private void TriggerHapticFeedback(float lowFrequency, float highFrequency, float duration)
    {
        if (currentGamepad == null)
            return;
        
        try
        {
            currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            StartCoroutine(StopHapticFeedback(duration));
        }
        catch
        {
        }
    }
    
    private IEnumerator StopHapticFeedback(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        if (currentGamepad != null)
        {
            try
            {
                currentGamepad.SetMotorSpeeds(0f, 0f);
            }
            catch
            {
            }
        }
    }
    
    private void PlayHoverSound()
    {
        if (hoverSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }
    
    private void PlayClickSound()
    {
        if (clickSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clickSound, clickVolume);
        }
    }
    
    private void FindAndSubscribeToPlayers()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();
        foreach (PlayerHealth player in players)
        {
            if (player != null && !playerHealthComponents.Contains(player))
            {
                playerHealthComponents.Add(player);
                player.OnPlayerDied += OnPlayerDied;
                Debug.Log($"[EnhancedGameOverManager] Subscribed to player: {player.gameObject.name}");
            }
        }
        
        Debug.Log($"[EnhancedGameOverManager] Monitoring {playerHealthComponents.Count} players");
    }
    
    private void OnPlayerDied(int playerID)
    {
        if (gameOverTriggered) return;
        
        deadPlayersCount++;
        Debug.Log($"[EnhancedGameOverManager] Player {playerID} died. Total dead: {deadPlayersCount}/{playerHealthComponents.Count}");
        
        if (deadPlayersCount >= playerHealthComponents.Count && playerHealthComponents.Count > 0)
        {
            Debug.Log("[EnhancedGameOverManager] All players dead - triggering Game Over");
            ShowGameOver();
        }
    }
    
    private void CreateGameOverUI()
    {
        gameOverCanvas = gameObject.AddComponent<Canvas>();
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameOverCanvas.sortingOrder = 9999;
        
        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        // Disabled GraphicRaycaster to prevent mouse/touch activation
        // Navigation and selection will be handled via Gamepad input
        GraphicRaycaster raycaster = gameObject.AddComponent<GraphicRaycaster>();
        raycaster.enabled = false;
        
        // Create panel without background image (removed panelImage component)
        gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        gameOverPanel.transform.SetParent(gameOverCanvas.transform, false);
        
        // No Image component - transparent panel
        panelCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.alpha = 0f;
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        GameObject textObj = new GameObject("GameOverText", typeof(RectTransform));
        textObj.transform.SetParent(gameOverPanel.transform, false);
        textObj.transform.SetAsFirstSibling(); // Ensure text is rendered above other elements
        
        textCanvasGroup = textObj.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        
        gameOverText = textObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.color = Color.white; // Pure white color (#FFFFFF)
        gameOverText.fontSize = gameOverTextFontSize;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontStyle = FontStyles.Bold;
        
        // Configure black outline (2px thickness)
        gameOverText.outlineColor = Color.black;
        gameOverText.outlineWidth = 0.2f; // 2px equivalent in TMP units
        gameOverText.enableVertexGradient = false;
        
        if (larkeSansFont != null)
        {
            gameOverText.font = larkeSansFont;
        }
        
        ApplyTextureToTMP(gameOverText);
        
        textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center vertically
        textRectTransform.anchorMax = new Vector2(0.5f, 0.5f); // Center vertically
        textRectTransform.sizeDelta = new Vector2(800, 150);
        textRectTransform.anchoredPosition = Vector2.zero;
        
        // Store original position for reset functionality
        originalTextPosition = textRectTransform.anchoredPosition;
        
        GameObject buttonContainer = new GameObject("ButtonContainer", typeof(RectTransform));
        buttonContainer.transform.SetParent(gameOverPanel.transform, false);
        
        buttonsCanvasGroup = buttonContainer.AddComponent<CanvasGroup>();
        buttonsCanvasGroup.alpha = 0f;
        
        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.35f);
        containerRect.anchorMax = new Vector2(0.5f, 0.35f);
        containerRect.sizeDelta = new Vector2(450, 250);
        containerRect.anchoredPosition = Vector2.zero;
        
        VerticalLayoutGroup layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 30f;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        
        retryButton = CreateButton("RetryButton", "RETRY", buttonContainer.transform);
        retryButton.onClick.AddListener(OnRetryClicked);
        
        quitButton = CreateButton("QuitButton", "QUIT", buttonContainer.transform);
        quitButton.onClick.AddListener(OnQuitClicked);
        
        buttons = new Button[] { retryButton, quitButton };
        buttonOriginalScales = new Vector3[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttonOriginalScales[i] = buttons[i].GetComponent<RectTransform>().localScale;
            
            CanvasGroup btnGroup = buttons[i].gameObject.AddComponent<CanvasGroup>();
            btnGroup.alpha = (i == 0) ? 1f : 0.7f;
            
            AddButtonHoverEvents(buttons[i], i);
        }
    }
    
    private void ApplyTextureToTMP(TextMeshProUGUI tmpText)
    {
        if (tmpText == null || !useTextTexture || textFaceTexture == null) return;
        
        // Create a material instance to avoid affecting other text elements
        Material mat = tmpText.fontMaterial;
        mat.SetTexture(ShaderUtilities.ID_FaceTex, textFaceTexture);
        mat.SetTextureScale(ShaderUtilities.ID_FaceTex, textureScale);
        tmpText.fontMaterial = mat;
    }
    
    private Button CreateButton(string name, string buttonText, Transform parent)
    {
        GameObject buttonObj = new GameObject(name, typeof(RectTransform));
        buttonObj.transform.SetParent(parent, false);
        
        Button button = buttonObj.AddComponent<Button>();
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonNormalColor;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400, 100);
        
        ColorBlock colors = button.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHighlightedColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonSelectedColor;
        colors.fadeDuration = buttonTransitionSpeed;
        button.colors = colors;
        
        GameObject textObj = new GameObject("ButtonText", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.color = buttonTextColor;
        text.fontSize = buttonFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        if (larkeSansFont != null)
        {
            text.font = larkeSansFont;
        }
        
        ApplyTextureToTMP(text);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return button;
    }
    
    private void AddButtonHoverEvents(Button button, int index)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => {
            if (selectedButtonIndex != index)
            {
                selectedButtonIndex = index;
                UpdateButtonSelection();
                PlayHoverSound();
            }
        });
        trigger.triggers.Add(pointerEnter);
        
        // Note: PointerClick is intentionally omitted to prevent mouse/touch activation
        // selection is only handled via Gamepad Button South (A)
    }
    
    public void ShowGameOver()
    {
        if (gameOverTriggered) return;
        
        gameOverTriggered = true;
        gameOverCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        selectedButtonIndex = 0;
        
        // Reset text position to center
        ResetTextPosition();
        
        if (horrorAmbientSound != null)
        {
            ambientAudioSource.clip = horrorAmbientSound;
            ambientAudioSource.Play();
        }
        
        StartCoroutine(FadeInSequence());
    }
    
    private IEnumerator FadeInSequence()
    {
        panelCanvasGroup.alpha = 0f;
        textCanvasGroup.alpha = 0f;
        buttonsCanvasGroup.alpha = 0f;
        
        Debug.Log("[EnhancedGameOverManager] Starting fade-in sequence");
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        panelCanvasGroup.alpha = 1f;
        
        elapsed = 0f;
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            textCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / textFadeInDuration);
            yield return null;
        }
        textCanvasGroup.alpha = 1f;
        
        StartCoroutine(PulseText());
        
        // Enable text movement after text is fully visible
        EnableTextMovement();
        
        elapsed = 0f;
        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            buttonsCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / buttonFadeInDuration);
            yield return null;
        }
        buttonsCanvasGroup.alpha = 1f;
        
        UpdateButtonSelection();
        
        // Removed FadeOutAmbient call to keep music playing
        // if (ambientAudioSource.isPlaying)
        // {
        //     StartCoroutine(FadeOutAmbient(1.5f));
        // }
        
        Debug.Log("[EnhancedGameOverManager] Fade-in sequence complete");
    }
    
    private IEnumerator PulseText()
    {
        while (gameOverTriggered && gameOverText != null)
        {
            float pulse = Mathf.Sin(Time.unscaledTime * textPulseSpeed) * textPulseIntensity;
            float alpha = 1f + pulse;
            Color col = gameOverText.color;
            col.a = Mathf.Clamp01(alpha);
            gameOverText.color = col;
            yield return null;
        }
    }
    
    private IEnumerator FadeOutAmbient(float duration)
    {
        float startVolume = ambientAudioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            ambientAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        ambientAudioSource.Stop();
        ambientAudioSource.volume = ambientVolume;
    }
    
    public void HideGameOver()
    {
        // Disable text movement and reset position
        DisableTextMovement();
        ResetTextPosition();
        
        gameOverCanvas.gameObject.SetActive(false);
        gameOverTriggered = false;
        deadPlayersCount = 0;
        Time.timeScale = 1f;
    }
    
    private void OnRetryClicked()
    {
        Debug.Log("[EnhancedGameOverManager] RETRY clicked");
        StopAllCoroutines();
        
        if (ambientAudioSource.isPlaying)
        {
            ambientAudioSource.Stop();
        }
        
        HideGameOver();
        
        if (CheckpointSystem.Instance != null)
        {
            CheckpointSystem.Instance.LoadLastCheckpoint();
        }
        else
        {
            Debug.LogWarning("[EnhancedGameOverManager] No CheckpointSystem found, reloading current scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    private void OnQuitClicked()
    {
        Debug.Log("[EnhancedGameOverManager] QUIT clicked - Exiting game");
        StopAllCoroutines();
        
        if (ambientAudioSource.isPlaying)
        {
            ambientAudioSource.Stop();
        }
        
        #if UNITY_EDITOR
        Debug.Log("[EnhancedGameOverManager] Stopping editor playmode");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void HandleTextMovement()
    {
        if (textRectTransform == null) return;
        
        Vector2 leftStick = currentGamepad.leftStick.ReadValue();
        textMovementInput = Vector2.Lerp(textMovementInput, leftStick, textMovementEasing);
        
        if (Mathf.Abs(textMovementInput.y) > 0.1f)
        {
            Vector2 currentPosition = textRectTransform.anchoredPosition;
            float movementDelta = textMovementInput.y * textMoveSpeed * Time.unscaledDeltaTime;
            
            Vector2 newPosition = currentPosition + Vector2.up * movementDelta;
            
            // Apply screen boundaries (cached calculation for performance)
            newPosition = ClampTextPositionToScreen(newPosition);
            
            // Smooth movement with easing - optimized for 60fps
            Vector2 smoothedPosition = Vector2.SmoothDamp(currentPosition, newPosition, ref currentTextVelocity, textMovementSmoothing);
            textRectTransform.anchoredPosition = smoothedPosition;
        }
    }
    
    private Vector2 ClampTextPositionToScreen(Vector2 position)
    {
        if (textRectTransform == null) return position;
        
        // Get screen dimensions in canvas space
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 canvasSize = gameOverCanvas.GetComponent<RectTransform>().sizeDelta;
        
        // Calculate text bounds
        float textHeight = textRectTransform.sizeDelta.y;
        float halfTextHeight = textHeight * 0.5f;
        
        // Calculate safe movement area with padding
        float minY = -canvasSize.y * 0.5f + halfTextHeight + screenEdgePadding;
        float maxY = canvasSize.y * 0.5f - halfTextHeight - screenEdgePadding;
        
        // Clamp position
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    private void ResetTextPosition()
    {
        if (textRectTransform != null)
        {
            textRectTransform.anchoredPosition = originalTextPosition;
            currentTextVelocity = Vector2.zero;
            textMovementInput = Vector2.zero;
        }
    }
    
    private void EnableTextMovement()
    {
        textMovementEnabled = true;
    }

    private void OnValidate()
    {
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = ambientVolume;
        }
    }
    
    private void DisableTextMovement()
    {
        textMovementEnabled = false;
    }
    
    private void OnDestroy()
    {
        foreach (PlayerHealth player in playerHealthComponents)
        {
            if (player != null)
            {
                player.OnPlayerDied -= OnPlayerDied;
            }
        }
    }
}
