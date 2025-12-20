using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup pausePanelCanvasGroup; 
    [SerializeField] private TMPro.TextMeshProUGUI pauseText; 
    [SerializeField] private UnityEngine.UI.Button continueButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private RectTransform pauseTitleRectTransform;

    [Header("Auto-Setup")]
    [SerializeField] private bool findPlayersAutomatically = true;
    [SerializeField] private bool createPauseUIIfNeeded = false;

    [Header("Joystick Navigation")]
    [SerializeField] private float navigationThreshold = 0.5f;
    [SerializeField] private float navigationCooldown = 0.2f;

    [Header("Button Visual Effects")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.8f); 
    [SerializeField] private Color highlightedColor = new Color(1f, 0.2f, 0.2f, 0.3f); 
    [SerializeField] private float fadeTransitionSpeed = 3f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float highlightedScale = 1.1f;

    [Header("Typography")]
    [SerializeField] private TMPro.TMP_FontAsset mainMenuFont;

    [Header("Button Background")]
    [SerializeField] private Image continueButtonBackground;
    [SerializeField] private Image quitButtonBackground;

    [Header("Blur Effect")]
    [SerializeField] private Volume blurVolume; 
    [SerializeField] private float blurIntensity = 5f; 
    [SerializeField] private float blurTransitionSpeed = 3f; 
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

    void Start()
    {
        
        EnsureGameNotPausedAtStart();
        
        SetupPauseUI();
        FindPlayers();
        SetupButtonListeners();
        HidePausePanel();
        SetupAudio();
    }
    
    void EnsureGameNotPausedAtStart()
    {
        
        if (Mathf.Approximately(Time.timeScale, 0f))
        {

            Time.timeScale = 1f;
        }
        
        
        isPaused = false;
    }

    void SetupPauseUI()
    {

        
        
        if (pausePanelCanvasGroup == null)
        {
            GameObject pausePanel = GameObject.Find("PausePanel");
            if (pausePanel != null)
            {
                pausePanelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();

            }
            else
            {

            }
        }
        else
        {

        }
        
        
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 0f;
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;

        }
        else
        {

        }

        
        if (pauseText == null)
        {
            GameObject pauseTitle = GameObject.Find("PauseTitle");
            if (pauseTitle != null)
            {
                pauseText = pauseTitle.GetComponent<TMPro.TextMeshProUGUI>();
                pauseTitleRectTransform = pauseTitle.GetComponent<RectTransform>();

            }
            else
            {

            }
        }
        
        if (pauseTitleRectTransform == null && pauseText != null)
        {
            pauseTitleRectTransform = pauseText.GetComponent<RectTransform>();
        }

        
        if (mainMenuFont != null)
        {
            if (pauseText != null)
            {
                pauseText.font = mainMenuFont;
            }
        }

        
        if (continueButton == null)
        {
            GameObject continueBtn = GameObject.Find("ContinueButton");
            if (continueBtn != null)
            {
                continueButton = continueBtn.GetComponent<UnityEngine.UI.Button>();

            }
            else
            {

            }
        }

        
        if (quitButton == null)
        {
            GameObject quitBtn = GameObject.Find("QuitButton");
            if (quitBtn != null)
            {
                quitButton = quitBtn.GetComponent<UnityEngine.UI.Button>();

            }
            else
            {

            }
        }

        
        if (continueButtonBackground == null && continueButton != null)
        {
            continueButtonBackground = continueButton.GetComponent<Image>();
        }
        if (quitButtonBackground == null && quitButton != null)
        {
            quitButtonBackground = quitButton.GetComponent<Image>();
        }
        
        
        if (mainMenuFont != null)
        {
            if (continueButton != null)
            {
                var txt = continueButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                if (txt != null) txt.font = mainMenuFont;
            }

            if (quitButton != null)
            {
                var txt = quitButton.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                if (txt != null) txt.font = mainMenuFont;
            }
        }

        
        Texture2D buttonGrungeTexture = Resources.Load<Texture2D>("Dark UI/Textures/Grunge/Background 3");
        if (buttonGrungeTexture != null)
        {
            Sprite grungeSprite = Sprite.Create(
                buttonGrungeTexture,
                new Rect(0, 0, buttonGrungeTexture.width, buttonGrungeTexture.height),
                new Vector2(0.5f, 0.5f));

            if (continueButtonBackground != null)
            {
                continueButtonBackground.sprite = grungeSprite;
                continueButtonBackground.type = Image.Type.Sliced;
                continueButtonBackground.color = Color.white;
            }

            if (quitButtonBackground != null)
            {
                quitButtonBackground.sprite = grungeSprite;
                quitButtonBackground.type = Image.Type.Sliced;
                quitButtonBackground.color = Color.white;
            }
        }


        SetupBlurEffect();

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
            else
            {

            }
        }
        else
        {

        }

        if (player2 != null)
        {
            player2Input = player2.GetComponent<PlayerInput>();
            if (player2Input != null)
            {

                ConnectPlayerPauseAction(player2Input, "Player2");
            }
            else
            {

            }
        }
        else
        {

        }
        

    }

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

                pauseAction.performed += OnPausePressed;


            }
            else
            {

                
                
                string availableActions = "";
                foreach (var action in playerInput.actions)
                {
                    availableActions += action.name + ", ";
                }
                


            }
        }
        else
        {

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
        else
        {

        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            
            
            AddButtonHoverEvents(quitButton, 1);

        }
        else
        {

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
        pauseButtons = new Button[2];
        pauseButtons[0] = continueButton;
        pauseButtons[1] = quitButton;
        
        
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
        
        
        int oldIndex = currentButtonIndex;
        previousButtonIndex = currentButtonIndex;
        currentButtonIndex = index;
        
        
        pauseButtons[index].Select();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(pauseButtons[index].gameObject);
        }
        
        
        ApplyButtonHighlight(pauseButtons[index], true);

        
        if (oldIndex != currentButtonIndex)
        {
            PlayHoverSound();
        }
        
        
        if (previousButtonIndex >= 0 && previousButtonIndex < pauseButtons.Length && previousButtonIndex != index)
        {
            ApplyButtonHighlight(pauseButtons[previousButtonIndex], false);
        }
    }
    
    void ApplyButtonHighlight(Button button, bool isHighlighted)
    {
        if (button == null) return;
        
        
        ColorBlock colors = button.colors;
        if (isHighlighted)
        {
            colors.normalColor = highlightedColor;
            colors.highlightedColor = highlightedColor;
        }
        else
        {
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
        }
        button.colors = colors;

        
        RectTransform rt = button.transform as RectTransform;
        if (rt != null)
        {
            rt.localScale = isHighlighted ? Vector3.one * highlightedScale : Vector3.one * normalScale;
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

                    
                    RectTransform rt = pauseButtons[i].transform as RectTransform;
                    if (rt != null)
                    {
                        rt.localScale = Vector3.one * normalScale;
                    }
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
    
    
    public bool IsPaused()
    {
        return isPaused;
    }

    void OnPausePressed(InputAction.CallbackContext context)
    {

        TogglePause();
    }

    public void TogglePause()
    {

        
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

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

        
        





        
        
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 1f;
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;

        }
        else
        {

        }
        
        
        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(true);

        }
        else
        {

        }

        
        
        canNavigate = true;
        currentButtonIndex = 0;
        if (pauseButtons != null && pauseButtons.Length > 0)
        {
            HighlightButton(currentButtonIndex);
        }
        
        
        EnableBlur();
        

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
        else
        {

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
        else
        {

        }
        
        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(false);

        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        canNavigate = false;
        RestoreAllButtons();
        
        
        DisableBlur();
        
    }
    
    System.Collections.IEnumerator FadeOutAndHidePanel()
    {
        
        yield return StartCoroutine(FadeCanvasGroup(pausePanelCanvasGroup, pausePanelCanvasGroup.alpha, 0f, fadeTransitionSpeed));
        
        
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
        }
        
        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(false);
        }
    }

    void OnContinueButtonClicked()
    {
        PlayClickSound();
        ResumeGame();
    }

    void OnQuitButtonClicked()
    {
        PlayClickSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

void Update()
    {
        
        if (Application.isEditor || Debug.isDebugBuild)
        {
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
            {
                TogglePause();
            }
        }

        if (!isPaused || !canNavigate || pauseButtons == null || pauseButtons.Length == 0) return;
        
        
        float verticalInput = 0f;
        
        
        
        if (player1Input != null && player1Input.actions != null)
        {
            var moveAction = FindMoveAction(player1Input);
            if (moveAction != null && moveAction.enabled)
            {
                Vector2 moveInput = moveAction.ReadValue<Vector2>();
                if (Mathf.Abs(moveInput.y) > navigationThreshold) verticalInput = moveInput.y;
            }
        }
        
        if (verticalInput == 0f && player2Input != null && player2Input.actions != null)
        {
            var moveAction = FindMoveAction(player2Input);
            if (moveAction != null && moveAction.enabled)
            {
                Vector2 moveInput = moveAction.ReadValue<Vector2>();
                if (Mathf.Abs(moveInput.y) > navigationThreshold) verticalInput = moveInput.y;
            }
        }

        
        if (verticalInput == 0f)
        {
            foreach (var gamepad in Gamepad.all)
            {
                float y = gamepad.leftStick.y.ReadValue();
                if (Mathf.Abs(y) > navigationThreshold) { verticalInput = y; break; }
                
                float dpadY = gamepad.dpad.y.ReadValue();
                if (Mathf.Abs(dpadY) > navigationThreshold) { verticalInput = dpadY; break; }
            }
        }
        
        
        if (Mathf.Abs(verticalInput) > navigationThreshold && Time.unscaledTime - lastNavigationTime > navigationCooldown)
        {
            
            if (verticalInput > 0f) 
            {
                NavigateUp();
            }
            
            else if (verticalInput < 0f) 
            {
                NavigateDown();
            }
            
            lastNavigationTime = Time.unscaledTime;
        }
        
        
        HandleButtonSelection();
    }
    
    void NavigateUp()
    {
        if (pauseButtons == null || pauseButtons.Length == 0) return;
        
        currentButtonIndex--;
        if (currentButtonIndex < 0)
        {
            currentButtonIndex = pauseButtons.Length - 1; 
        }
        
        HighlightButton(currentButtonIndex);
    }
    
    void NavigateDown()
    {
        if (pauseButtons == null || pauseButtons.Length == 0) return;
        
        currentButtonIndex++;
        if (currentButtonIndex >= pauseButtons.Length)
        {
            currentButtonIndex = 0; 
        }
        
        HighlightButton(currentButtonIndex);
    }
    
    void HandleButtonSelection()
    {
        bool submitPressed = false;
        
        
        if (player1Input != null && player1Input.actions != null)
        {
            var submitAction = player1Input.actions["Submit"];
            if (submitAction != null && submitAction.WasPressedThisFrame())
            {
                submitPressed = true;
            }
        }
        
        
        if (!submitPressed && player2Input != null && player2Input.actions != null)
        {
            var submitAction = player2Input.actions["Submit"];
            if (submitAction != null && submitAction.WasPressedThisFrame())
            {
                submitPressed = true;
            }
        }

        
        if (!submitPressed)
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame) { submitPressed = true; break; }
            }
        }
        
        if (submitPressed && pauseButtons != null && currentButtonIndex >= 0 && currentButtonIndex < pauseButtons.Length)
        {
            
            PlayClickSound();
            pauseButtons[currentButtonIndex].onClick.Invoke();
        }
    }

    
    InputAction FindMoveAction(PlayerInput input)
    {
        if (input == null || input.actions == null) return null;

        
        var move = input.actions["Move"];
        if (move != null) return move;

        
        foreach (var action in input.actions)
        {
            if (action == null) continue;
            if (action.expectedControlType != null && !action.expectedControlType.Contains("Vector2")) continue;

            string nameLower = action.name.ToLowerInvariant();
            if (nameLower.Contains("move") || nameLower.Contains("navigation") || nameLower.Contains("navigate"))
            {
                return action;
            }
        }

        return null;
    }

    void OnDisable()
    {
        if (player1Input != null && player1Input.actions != null)
        {
            var pauseAction = player1Input.actions["PauseToggle"];
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePressed;
            }
        }

        if (player2Input != null && player2Input.actions != null)
        {
            var pauseAction = player2Input.actions["PauseToggle"];
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePressed;
            }
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }

    void SetupBlurEffect()
    {

        
        if (blurVolume == null)
        {
            
            GameObject volumeObj = GameObject.Find("Global Volume");
            if (volumeObj != null)
            {
                blurVolume = volumeObj.GetComponent<Volume>();

            }
            else
            {

                return;
            }
        }

        if (blurVolume != null && blurVolume.profile != null)
        {
            
            if (blurVolume.profile.TryGet(out depthOfField))
            {

                
                
                depthOfField.active = false; 
                depthOfField.mode.overrideState = true;
                depthOfField.mode.value = DepthOfFieldMode.Bokeh;
                
                depthOfField.focusDistance.overrideState = true;
                depthOfField.focusDistance.value = blurIntensity;
                
                depthOfField.aperture.overrideState = true;
                depthOfField.aperture.value = blurIntensity / 10f;
                
                depthOfField.focalLength.overrideState = true;
                depthOfField.focalLength.value = 50f;
                

            }
            else
            {
                
                if (blurVolume.profile != null)
                {
                    depthOfField = blurVolume.profile.Add<DepthOfField>();
                    if (depthOfField != null)
                    {
                        depthOfField.active = false;
                        depthOfField.mode.overrideState = true;
                        depthOfField.mode.value = DepthOfFieldMode.Bokeh;
                        depthOfField.focusDistance.overrideState = true;
                        depthOfField.focusDistance.value = blurIntensity;
                        depthOfField.aperture.overrideState = true;
                        depthOfField.aperture.value = blurIntensity / 10f;
                        depthOfField.focalLength.overrideState = true;
                        depthOfField.focalLength.value = 50f;
                    }
                }
            }
        }
        else
        {

        }
        

    }

    public void EnableBlur()
    {

        
        if (depthOfField != null)
        {
            depthOfField.active = true;
        }
        else
        {
            
            if (blurVolume != null && blurVolume.profile != null)
            {
                if (blurVolume.profile.TryGet(out depthOfField))
                {
                    depthOfField.active = true;
                }
            }
        }
        

    }

    public void DisableBlur()
    {

        
        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
        else
        {
            
            if (blurVolume != null && blurVolume.profile != null)
            {
                if (blurVolume.profile.TryGet(out depthOfField))
                {
                    depthOfField.active = false;
                }
            }
        }
        

    }
}


public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private PauseController pauseController;
    private int buttonIndex;
    private bool isHovered = false;
    
    public void SetupHover(Button btn, PauseController controller, int index)
    {
        button = btn;
        pauseController = controller;
        buttonIndex = index;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pauseController != null && button != null)
        {
            
            if (pauseController.IsPaused())
            {
                isHovered = true;
                pauseController.SetHoveredButton(buttonIndex);
                pauseController.PlayHoverSound();
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (pauseController != null && button != null)
        {
            isHovered = false;
            
        }
    }
    
    public bool IsHovered()
    {
        return isHovered;
    }
}
