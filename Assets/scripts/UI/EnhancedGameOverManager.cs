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
    public float ambientVolume = 0.4f;
    public float hoverVolume = 0.5f;
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
        
        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = ambientVolume;
        
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;
        
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
        
        RectTransform rect = button.GetComponent<RectTransform>();
        Image img = button.GetComponent<Image>();
        
        Vector3 targetScale = hovered ? buttonOriginalScales[buttonIndex] * buttonHoverScale : buttonOriginalScales[buttonIndex];
        Color targetColor = hovered ? buttonSelectedColor : buttonNormalColor;
        
        float elapsed = 0f;
        Vector3 startScale = rect.localScale;
        Color startColor = img.color;
        
        while (elapsed < buttonTransitionSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / buttonTransitionSpeed;
            
            rect.localScale = Vector3.Lerp(startScale, targetScale, t);
            img.color = Color.Lerp(startColor, targetColor, t);
            
            yield return null;
        }
        
        rect.localScale = targetScale;
        img.color = targetColor;
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
        
        gameObject.AddComponent<GraphicRaycaster>();
        
        gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        gameOverPanel.transform.SetParent(gameOverCanvas.transform, false);
        
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = panelColor;
        
        panelCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.alpha = 0f;
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        GameObject textObj = new GameObject("GameOverText", typeof(RectTransform));
        textObj.transform.SetParent(gameOverPanel.transform, false);
        
        textCanvasGroup = textObj.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        
        gameOverText = textObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.color = gameOverTextColor;
        gameOverText.fontSize = gameOverTextFontSize;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontStyle = FontStyles.Bold;
        
        if (larkeSansFont != null)
        {
            gameOverText.font = larkeSansFont;
        }
        else
        {
            Debug.LogWarning("[EnhancedGameOverManager] Larke Sans font not assigned, using default");
        }
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.65f);
        textRect.anchorMax = new Vector2(0.5f, 0.65f);
        textRect.sizeDelta = new Vector2(800, 150);
        textRect.anchoredPosition = Vector2.zero;
        
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
            AddButtonHoverEvents(buttons[i], i);
        }
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
        
        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener((data) => {
            PlayClickSound();
        });
        trigger.triggers.Add(pointerClick);
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
        
        elapsed = 0f;
        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            buttonsCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / buttonFadeInDuration);
            yield return null;
        }
        buttonsCanvasGroup.alpha = 1f;
        
        UpdateButtonSelection();
        
        if (ambientAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutAmbient(1.5f));
        }
        
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
