using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using VITRUVIUS.Menu;
using UnityEngine.Rendering.Universal;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image controlsDisplayImage; 
[Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pausePanelCanvasGroup; 
    [SerializeField] private UnityEngine.UI.Button continueButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private UnityEngine.UI.Button controlsButton;
    [SerializeField] private UnityEngine.UI.Button backButton;
    [SerializeField] private GameObject controlsCanvas;
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private RectTransform pauseTitleRectTransform;

    [Header("Auto-Setup")]
    [SerializeField] private bool findPlayersAutomatically = true;

    [Header("Joystick Navigation")]
    [SerializeField] private float navigationThreshold = 0.5f;
    [SerializeField] private float navigationCooldown = 0.2f;

    [Header("Button Visual Effects")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.8f); 
    [SerializeField] private Color highlightedColor = new Color(1f, 0.2f, 0.2f, 0.3f); 
    [SerializeField] private float fadeTransitionSpeed = 3f;

    [Header("Typography")]

    [Header("Button Background")]
    [SerializeField] private Image continueButtonBackground;
    [SerializeField] private Image quitButtonBackground;
    [SerializeField] private Image controlsButtonBackground;

    [Header("Blur Effect")]
    [SerializeField] private Volume blurVolume; 
    [SerializeField] private float blurIntensity = 5f; 
    private DepthOfField depthOfField;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float soundVolume = 0.7f; 

    private bool isPaused = false;
    private PlayerInput player1Input;
    private PlayerInput player2Input;
    private Button[] pauseButtons;
    private int currentButtonIndex = 0;
    private float lastNavigationTime = 0f;
    private bool canNavigate = false;
    private int previousButtonIndex = -1;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private bool blockInputDuringTransition = true;
    private bool isTransitioning = false;

    [Header("Validation Settings")]
    [SerializeField] private bool preventOverlap = true;

void Start()
    {
        InitializePauseSystem();
        EnsureGameNotPausedAtStart();
        SetupPauseUI();
        FindPlayers();
        SetupButtonListeners();
        HidePausePanel();
        SetupAudio();
        ValidateInitialState();
        
        
        EnsureControlsImageSetup();
    }
    
    void InitializePauseSystem()
    {
        isTransitioning = false;
        
        if (preventOverlap)
        {
            canNavigate = false;
        }
        
        ValidateUIReferences();
    }
    
    void ValidateInitialState()
    {
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 0f;
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
            if (controlsCanvas != null)
            {
                controlsCanvas.SetActive(false);
            }
        }
    }
    
    void ValidateUIReferences()
    {
        if (pausePanelCanvasGroup == null)
        {

        }
        if (controlsCanvasGroup == null)
        {

        }
        if (continueButton == null)
        {

        }
        if (controlsButton == null)
        {

        }
        if (quitButton == null)
        {

        }
    }
    
    void EnsureGameNotPausedAtStart()
    {
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = 1f;
        }
        
        isPaused = false;
    }

    void CreatePauseUI()
    {
    }

    void FindPlayers()
    {
        if (!findPlayersAutomatically) 
        {
            return;
        }

        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        
        if (player1 != null)
        {
            player1Input = player1.GetComponent<PlayerInput>();
            if (player1Input != null)
            {
                ConnectPlayerPauseAction(player1Input, "Player1");
            }
        }

        if (player2 != null)
        {
            player2Input = player2.GetComponent<PlayerInput>();
            if (player2Input != null)
            {
                ConnectPlayerPauseAction(player2Input, "Player2");
            }
        }
    }

    private InputAction p1PauseAction;
    private InputAction p2PauseAction;

    void ConnectPlayerPauseAction(PlayerInput playerInput, string playerName)
    {
        if (playerInput.actions != null)
        {
            InputAction pauseAction = playerInput.actions["PauseToggle"];
            
            if (pauseAction == null) pauseAction = playerInput.actions["Pause"];
            if (pauseAction == null) pauseAction = playerInput.actions["Start"];
            if (pauseAction == null) pauseAction = playerInput.actions["Menu"];
            if (pauseAction == null) pauseAction = playerInput.actions["Options"];
            
            if (pauseAction != null)
            {
                if (playerName == "Player1") p1PauseAction = pauseAction;
                else if (playerName == "Player2") p2PauseAction = pauseAction;

                pauseAction.performed += OnPausePressed;
            }
        }
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (audioSource != null)
        {
            audioSource.volume = soundVolume;
            audioSource.playOnAwake = false;
        }
    }
    
    public void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
    
    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    void SetupButtonListeners()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            AddButtonHoverEvents(continueButton, 0);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            AddButtonHoverEvents(quitButton, 2);
        }
        
        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(OnControlsButtonClicked);
            AddButtonHoverEvents(controlsButton, 1);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        SetupPauseButtonsArray();
    }
    
    void AddButtonHoverEvents(Button button, int buttonIndex)
    {
        if (button == null) return;
        
        ButtonHoverHandler hoverHandler = button.gameObject.GetComponent<ButtonHoverHandler>();
        if (hoverHandler == null)
        {
            hoverHandler = button.gameObject.AddComponent<ButtonHoverHandler>();
        }
        
        hoverHandler.SetupHover(button, this, buttonIndex);
    }

    void SetupPauseButtonsArray()
    {
        pauseButtons = new Button[3];
        pauseButtons[0] = continueButton;
        pauseButtons[1] = controlsButton;
        pauseButtons[2] = quitButton;
        
        var validButtons = new System.Collections.Generic.List<Button>();
        foreach (Button button in pauseButtons)
        {
            if (button != null)
            {
                validButtons.Add(button);
            }
        }
        pauseButtons = validButtons.ToArray();

        if (pauseButtons.Length > 0)
        {
            currentButtonIndex = 0;
            previousButtonIndex = -1;
            HighlightButton(currentButtonIndex);
        }
    }
    
void HighlightButton(int index)
    {
        if (pauseButtons == null || pauseButtons.Length == 0) return;
        
        if (index < 0 || index >= pauseButtons.Length) return;
        
        if (pauseButtons[index] == null)
        {
            return;
        }

        int oldIndex = currentButtonIndex;
        previousButtonIndex = currentButtonIndex;
        currentButtonIndex = index;
        
        try 
        {
            pauseButtons[index].Select();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(pauseButtons[index].gameObject);
            }
        }
        catch (System.Exception)
        {

        }
        
        
        for (int i = 0; i < pauseButtons.Length; i++)
        {
            if (pauseButtons[i] != null)
            {
                
                ApplyButtonHighlight(pauseButtons[i], i == index);
            }
        }

        
        if (oldIndex != currentButtonIndex)
        {
            PlayHoverSound();
        }
    }
    
    void ApplyButtonHighlight(Button button, bool isHighlighted)
    {
        if (button == null) return;
        
        Image[] images = button.GetComponentsInChildren<Image>();
        Text[] texts = button.GetComponentsInChildren<Text>();
        
        System.Collections.Generic.List<Graphic> allGraphics = new System.Collections.Generic.List<Graphic>();
        allGraphics.AddRange(images);
        allGraphics.AddRange(texts);
        
        StartCoroutine(SmoothOpacityTransition(allGraphics.ToArray(), isHighlighted ? 1f : 0.5f, 0.3f));
    }

    System.Collections.IEnumerator SmoothOpacityTransition(Graphic[] graphics, float targetOpacity, float duration = 0.3f)
    {
        if (graphics == null || graphics.Length == 0) yield break;
        
        float startTime = Time.unscaledTime;
        float startOpacity = graphics[0].color.a;
        
        while (Time.unscaledTime - startTime < duration)
        {
            float elapsed = Time.unscaledTime - startTime;
            float newOpacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
            
            foreach (Graphic graphic in graphics)
            {
                if (graphic != null)
                {
                    Color color = graphic.color;
                    color.a = newOpacity;
                    graphic.color = color;
                }
            }
            
            yield return null;
        }
        
        foreach (Graphic graphic in graphics)
        {
            if (graphic != null)
            {
                Color color = graphic.color;
                color.a = targetOpacity;
                graphic.color = color;
            }
        }
    }

    void SetInputBlocked(bool blocked)
    {
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.interactable = !blocked;
            pausePanelCanvasGroup.blocksRaycasts = !blocked;
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.interactable = !blocked;
            controlsCanvasGroup.blocksRaycasts = !blocked;
        }
    }

    public void OnContinueButtonPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        if (blockInputDuringTransition && isTransitioning)
        {
            return;
        }
        
        if (isPaused && pausePanel != null && pausePanel.activeSelf)
        {
            TogglePause();
        }
    }
    
    System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        if (canvasGroup == null) 
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
    
    void RestoreAllButtons()
    {
        if (pauseButtons == null) return;
        
        for (int i = 0; i < pauseButtons.Length; i++)
        {
            if (pauseButtons[i] != null)
            {
                ColorBlock colors = pauseButtons[i].colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = normalColor;
                pauseButtons[i].colors = colors;
            }
        }
        
        currentButtonIndex = 0;
        previousButtonIndex = -1;
    }
    
    public void SetHoveredButton(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < pauseButtons.Length && pauseButtons[buttonIndex] != null)
        {
            currentButtonIndex = buttonIndex;
            HighlightButton(buttonIndex);
        }
    }

public void OnPausePressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        
        if (blockInputDuringTransition && isTransitioning)
        {
            return;
        }
        
        
        if (controlsCanvas != null && controlsCanvas.activeSelf)
        {
            return;
        }
        
        
        if (isPaused && pausePanel != null && pausePanelCanvasGroup != null && pausePanelCanvasGroup.alpha > 0)
        {
            return;
        }
        
        
        if (preventOverlap && IsAnyMenuActive() && !isPaused)
        {
            return;
        }
        
        
        TogglePause();
    }
    
    bool IsAnyMenuActive()
    {
        return (controlsCanvasGroup != null && controlsCanvasGroup.alpha > 0) ||
               (pausePanelCanvasGroup != null && pausePanelCanvasGroup.alpha > 0 && isPaused);
    }
    
    bool CanPauseGame()
    {
        if (Time.timeScale == 0f && !isPaused)
        {
            return false;
        }
        
        if (IsAnyMenuActive())
        {
            return false;
        }
        
        return true;
    }

    public void TogglePause()
    {
        if (isTransitioning)
        {
            return;
        }
        
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

public bool IsPaused() { return isPaused; }

public void ShowPauseMenuFromControls() { StartCoroutine(TransitionToPause()); }


    
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        ShowPausePanel();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        HidePausePanel();
    }

void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 1f;
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;
        }
        
        
        canNavigate = true;
        currentButtonIndex = 0;
        
        
        if (pauseButtons != null && pauseButtons.Length > 0)
        {
            for (int i = 0; i < pauseButtons.Length; i++)
            {
                if (pauseButtons[i] != null)
                {
                    ApplyButtonHighlight(pauseButtons[i], i == currentButtonIndex);
                }
            }
            HighlightButton(currentButtonIndex);
        }
        
        EnableBlur();
        
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, soundVolume);
        }
    }
    
    System.Collections.IEnumerator FadeInPanel()
    {
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 0f;
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
            
            yield return StartCoroutine(FadeCanvasGroup(pausePanelCanvasGroup, 0f, 1f, fadeTransitionSpeed));
            
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;
        }
    }

    void HidePausePanel()
    {
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 0f;
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
        }
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        canNavigate = false;
        RestoreAllButtons();
        
        DisableBlur();
    }

    void SetupPauseUI()
    {
        SetupCanvasGroups();
        SetupButtonReferences();
        ApplyConsistentButtonStyling();
        SetupBlurEffect();
    }

void EnsureControlsImageSetup()
    {
        
        if (controlsDisplayImage != null)
        {
            
            RectTransform rectTransform = controlsDisplayImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                
                rectTransform.anchoredPosition = Vector2.zero;
                
                
                rectTransform.sizeDelta = new Vector2(1200f, 800f);
                
                
                rectTransform.localScale = new Vector3(0.65f, 0.65f, 1f);
            }
            
            
            controlsDisplayImage.gameObject.SetActive(false);
        }
    }


    void SetupCanvasGroups()
    {
        if (pausePanel != null && pausePanelCanvasGroup == null)
        {
            pausePanelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (pausePanelCanvasGroup == null)
            {
                pausePanelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
        }
        
        if (controlsCanvas != null && controlsCanvasGroup == null)
        {
            controlsCanvasGroup = controlsCanvas.GetComponent<CanvasGroup>();
            if (controlsCanvasGroup == null)
            {
                controlsCanvasGroup = controlsCanvas.AddComponent<CanvasGroup>();
            }
        }
    }

    void SetupButtonReferences()
    {
        if (pausePanel != null)
        {
            if (continueButton == null)
            {
                Transform continueTransform = pausePanel.transform.Find("CONTINUE");
                if (continueTransform != null)
                {
                    continueButton = continueTransform.GetComponent<Button>();
                }
            }
            
            if (controlsButton == null)
            {
                Transform controlsTransform = pausePanel.transform.Find("CONTROLS");
                if (controlsTransform != null)
                {
                    controlsButton = controlsTransform.GetComponent<Button>();
                }
            }
            
            if (quitButton == null)
            {
                Transform quitTransform = pausePanel.transform.Find("QUIT");
                if (quitTransform != null)
                {
                    quitButton = quitTransform.GetComponent<Button>();
                }
            }
        }
        
        if (controlsCanvas != null && backButton == null)
        {
            Transform backTransform = controlsCanvas.transform.Find("BACK");
            if (backTransform != null)
            {
                backButton = backTransform.GetComponent<Button>();
            }
        }
    }

    void ApplyConsistentButtonStyling()
    {
        if (continueButton != null && continueButtonBackground != null)
        {
            SetupButtonVisuals(continueButton, continueButtonBackground, "CONTINUE");
        }
        
        if (controlsButton != null && controlsButtonBackground != null)
        {
            SetupButtonVisuals(controlsButton, controlsButtonBackground, "CONTROLS");
        }
        
        if (quitButton != null && quitButtonBackground != null)
        {
            SetupButtonVisuals(quitButton, quitButtonBackground, "QUIT");
        }
    }
    
    void SetupButtonVisuals(Button button, Image background, string buttonText)
    {
        if (button == null) return;
        
        var buttonTextComponent = button.GetComponentInChildren<Text>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = buttonText;
            buttonTextComponent.color = normalColor;
            buttonTextComponent.fontSize = 24;
        }
        
        if (background != null)
        {
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            background.raycastTarget = true;
        }
        
        var buttonColors = button.colors;
        buttonColors.normalColor = new Color(1f, 1f, 1f, 0.8f);
        buttonColors.highlightedColor = new Color(1f, 0.2f, 0.2f, 0.3f);
        buttonColors.pressedColor = new Color(0.8f, 0.1f, 0.1f, 0.5f);
        buttonColors.selectedColor = new Color(1f, 0.3f, 0.3f, 0.4f);
        button.colors = buttonColors;
        
        ApplyButtonHighlight(button, false);
    }

    void SetupBlurEffect()
    {
        if (blurVolume != null)
        {
            blurVolume.profile.TryGet(out depthOfField);
            if (depthOfField != null)
            {
                depthOfField.active = false;
            }
        }
    }

    void EnableBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.active = true;
            depthOfField.focalLength.value = blurIntensity;
        }
    }

    void DisableBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
    }

    public void OnContinueButtonClicked()
    {
        PlayClickSound();
        ResumeGame();
    }

    public void OnQuitButtonClicked()
    {
        PlayClickSound();
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnControlsButtonClicked()
    {
        PlayClickSound();
        StartCoroutine(TransitionToControls());
    }

    public void OnBackButtonClicked()
    {
        PlayClickSound();
        BeginTransitionToPause();
    }

    public void BeginTransitionToPause()
    {
        if (!gameObject.activeInHierarchy || !isActiveAndEnabled)
        {
            PauseController[] controllers = FindObjectsOfType<PauseController>(true);
            foreach (var c in controllers)
            {
                if (c != null && c.gameObject.activeInHierarchy && c.isActiveAndEnabled)
                {
                    c.BeginTransitionToPause();
                    return;
                }
            }
        }
        StartCoroutine(TransitionToPause());
    }

System.Collections.IEnumerator TransitionToControls()
    {
        isTransitioning = true;
        if (blockInputDuringTransition) SetInputBlocked(true);
        
        float duration = Mathf.Min(transitionDuration, 0.5f);
        
        if (pausePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(pausePanelCanvasGroup, 1f, 0f, duration));
        }
        
        if (pausePanel != null) pausePanel.SetActive(false);
        
        
        if (controlsDisplayImage != null)
        {
            controlsDisplayImage.gameObject.SetActive(true);
        }
        
        if (controlsCanvas != null) controlsCanvas.SetActive(true);
        if (controlsCanvasGroup == null && controlsCanvas != null)
        {
            controlsCanvasGroup = controlsCanvas.GetComponent<CanvasGroup>();
        }
        if (controlsCanvasGroup != null) 
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = true;
            controlsCanvasGroup.blocksRaycasts = true;
        }
        
        if (controlsCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(controlsCanvasGroup, 0f, 1f, duration));
            controlsCanvasGroup.alpha = 1f;
            controlsCanvasGroup.interactable = true;
            controlsCanvasGroup.blocksRaycasts = true;
        }
        
        SelectControlsFirstButton();
        isTransitioning = false;
        if (blockInputDuringTransition) SetInputBlocked(false);
    }

System.Collections.IEnumerator TransitionToPause()
    {
        isTransitioning = true;
        if (blockInputDuringTransition) SetInputBlocked(true);
        
        float duration = Mathf.Min(transitionDuration, 0.5f);
        
        if (controlsCanvasGroup == null && controlsCanvas != null)
        {
            controlsCanvasGroup = controlsCanvas.GetComponent<CanvasGroup>();
        }
        if (controlsCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(controlsCanvasGroup, 1f, 0f, duration));
        }
        
        
        if (controlsDisplayImage != null)
        {
            controlsDisplayImage.gameObject.SetActive(false);
        }
        
        if (controlsCanvas != null) controlsCanvas.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pausePanelCanvasGroup != null) 
        {
            pausePanelCanvasGroup.alpha = 0f;
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;
        }
        
        if (pausePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(pausePanelCanvasGroup, 0f, 1f, duration));
            pausePanelCanvasGroup.alpha = 1f;
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;
        }
        
        SelectPauseFirstButton();
        isTransitioning = false;
        if (blockInputDuringTransition) SetInputBlocked(false);
    }

    void SelectControlsFirstButton()
    {
        if (backButton != null)
        {
            backButton.Select();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(backButton.gameObject);
            }
        }
    }

    void SelectPauseFirstButton()
    {
        if (continueButton != null)
        {
            continueButton.Select();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
            }
            currentButtonIndex = 0;
            HighlightButton(currentButtonIndex);
        }
    }

    void Update()
    {
        if (canNavigate && pauseButtons != null && pauseButtons.Length > 0)
        {
            HandleGamepadNavigation();
        }
        
        if (controlsCanvas != null && controlsCanvas.activeSelf)
        {
            HandleControlsCanvasInput();
        }
    }

void HandleGamepadNavigation()
    {
        if (Time.unscaledTime - lastNavigationTime < navigationCooldown) return;
        if (isTransitioning) return;
        
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;
        
        
        float verticalInput = gamepad.leftStick.y.ReadValue();
        
        
        bool dpadUp = gamepad.dpad.up.isPressed;
        bool dpadDown = gamepad.dpad.down.isPressed;
        
        if (Mathf.Abs(verticalInput) > navigationThreshold || dpadUp || dpadDown)
        {
            if (verticalInput > navigationThreshold || dpadUp)
            {
                NavigateUp();
            }
            else if (verticalInput < -navigationThreshold || dpadDown)
            {
                NavigateDown();
            }
            lastNavigationTime = Time.unscaledTime;
        }
        
        
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            if (pauseButtons != null && currentButtonIndex >= 0 && currentButtonIndex < pauseButtons.Length)
            {
                Button selectedButton = pauseButtons[currentButtonIndex];
                if (selectedButton != null)
                {
                    selectedButton.onClick.Invoke();
                }
            }
        }
    }

    void NavigateUp()
    {
        if (pauseButtons == null || pauseButtons.Length == 0) return;
        
        int newIndex = currentButtonIndex - 1;
        if (newIndex < 0) newIndex = pauseButtons.Length - 1;
        
        while (newIndex != currentButtonIndex && pauseButtons[newIndex] == null)
        {
            newIndex--;
            if (newIndex < 0) newIndex = pauseButtons.Length - 1;
        }
        
        if (pauseButtons[newIndex] != null)
        {
            HighlightButton(newIndex);
        }
    }

    void NavigateDown()
    {
        if (pauseButtons == null || pauseButtons.Length == 0) return;
        
        int newIndex = currentButtonIndex + 1;
        if (newIndex >= pauseButtons.Length) newIndex = 0;
        
        while (newIndex != currentButtonIndex && pauseButtons[newIndex] == null)
        {
            newIndex++;
            if (newIndex >= pauseButtons.Length) newIndex = 0;
        }
        
        if (pauseButtons[newIndex] != null)
        {
            HighlightButton(newIndex);
        }
    }

void HandleControlsCanvasInput()
    {
        if (isTransitioning) return;
        
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;
        
        bool shouldReturnToPause = false;
        
        
        if (gamepad != null)
        {
            
            if (gamepad.buttonSouth.wasPressedThisFrame || 
                gamepad.buttonEast.wasPressedThisFrame ||  
                gamepad.buttonWest.wasPressedThisFrame ||  
                gamepad.buttonNorth.wasPressedThisFrame || 
                
                gamepad.leftShoulder.wasPressedThisFrame ||
                gamepad.rightShoulder.wasPressedThisFrame ||
                gamepad.leftTrigger.wasPressedThisFrame ||
                gamepad.rightTrigger.wasPressedThisFrame ||
                
                gamepad.leftStickButton.wasPressedThisFrame ||
                gamepad.rightStickButton.wasPressedThisFrame ||
                
                gamepad.dpad.up.wasPressedThisFrame ||
                gamepad.dpad.down.wasPressedThisFrame ||
                gamepad.dpad.left.wasPressedThisFrame ||
                gamepad.dpad.right.wasPressedThisFrame ||
                
                gamepad.startButton.wasPressedThisFrame ||
                gamepad.selectButton.wasPressedThisFrame)
            {
                shouldReturnToPause = true;
            }
        }
        
        
        if (keyboard != null)
        {
            if (keyboard.escapeKey.wasPressedThisFrame || 
                keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame)
            {
                shouldReturnToPause = true;
            }
        }
        
        if (shouldReturnToPause)
        {
            PlayClickSound();
            OnBackButtonClicked();
        }
    }
}
