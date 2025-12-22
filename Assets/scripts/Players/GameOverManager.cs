using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;

public class GameOverManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time to wait before reloading the scene after Game Over logic triggers.")]
    [SerializeField] private float reloadDelay = 3.0f;
    [Tooltip("Duration of the fade to black effect.")]
    [SerializeField] private float fadeDuration = 1.5f;
    [Tooltip("Intensity of the shake effect for the GAME OVER text.")]
    [SerializeField] private float shakeIntensity = 3f; 
    [Tooltip("Duration of the shake effect.")]
    [SerializeField] private float shakeDuration = 1.5f;
    [Tooltip("Whether to automatically pause the game when Game Over is triggered.")]
    [SerializeField] private bool autoPauseOnGameOver = false; 
    [Tooltip("Font size for the GAME OVER text. Default is 67 (approx 30% smaller than original 96).")]
    [SerializeField] private float gameOverTextSize = 67f; 

    [Header("References")]
    [Tooltip("The NPC that must survive. If null, will try to auto-find by tag.")]
    [SerializeField] private NPCHealth npcToProtect;
    [Tooltip("Tag to search for the NPC. Default: NPC")]
    [SerializeField] private string npcTag = "NPC";
    
    [Tooltip("The players in the scene. If empty, will try to auto-find.")]
    [SerializeField] private List<PlayerHealth> players;

    private int deadPlayersCount = 0;
    private bool gameOverTriggered = false;
    private Canvas fadeCanvas;
    private Image fadeImage;
    
    
    [Header("UI Assignment")]
    [Tooltip("The Retry button. If null, one will be created programmatically.")]
    [SerializeField] private Button retryButton;
    [Tooltip("The Quit (Respawn) button. If null, one will be created programmatically.")]
    [SerializeField] private Button quitButton;
    
    private Canvas gameOverCanvas;
    public Canvas gameOverButtonsCanvas;
    public GameObject gameOverCanvasPrefab;
    public bool autoCreateGameOverUI = false;
    private TextMeshProUGUI gameOverText;
    private Button internalRetryButton;
    private Button internalQuitButton;
    private CanvasGroup buttonCanvasGroup;
    
    
    [Header("UI Audio")]
    [Tooltip("Sound played when navigating between buttons.")]
    [SerializeField] private AudioClip hoverSound;
    [Tooltip("Sound played when a button is clicked.")]
    [SerializeField] private AudioClip clickSound;
    private AudioSource uiAudioSource;

    private float originalTimeScale = 1f;
    private bool wasGamePaused = false;

    public static GameOverManager Instance { get; private set; }

    void Update()
    {
        if (!gameOverTriggered || gameOverButtonsCanvas == null ? gameOverCanvas == null : false) return;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f; // 2D Sound
        uiAudioSource.ignoreListenerPause = true;
    }

    void Start()
    {
        InitializeReferences();
        SubscribeToEvents();
        CreateFadeScreen();
        CreateGameOverUI();
        SetupButtonListeners();
        StartCoroutine(InitialFadeIn());
        
        
        if (npcToProtect == null)
        {
            StartCoroutine(SearchForNPCLater());
        }
    }

    private void InitializeReferences()
    {
        
        if (npcToProtect == null)
        {
            npcToProtect = FindNPCByTag();
            
            if (npcToProtect == null)
            {
                npcToProtect = FindObjectOfType<NPCHealth>();
            }
            
            if (npcToProtect == null)
            {

            }
            else
            {

            }
        }

        
        if (players == null || players.Count == 0)
        {
            players = new List<PlayerHealth>(FindObjectsOfType<PlayerHealth>());
            if (players.Count == 0)
            {

            }
            else
            {

            }
        }
    }

    private NPCHealth FindNPCByTag()
    {
        
        GameObject[] npcsWithTag = GameObject.FindGameObjectsWithTag(npcTag);
        
        foreach (GameObject npcObj in npcsWithTag)
        {
            NPCHealth npcHealth = npcObj.GetComponent<NPCHealth>();
            if (npcHealth != null)
            {
                return npcHealth;
            }
        }
        
        
        if (npcTag != "NPC")
        {
            GameObject npcWithDefaultTag = GameObject.FindGameObjectWithTag("NPC");
            if (npcWithDefaultTag != null)
            {
                return npcWithDefaultTag.GetComponent<NPCHealth>();
            }
        }
        
        return null;
    }

    private void SubscribeToEvents()
    {
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied += HandleNPCDeath;
        }

        foreach (var player in players)
        {
            if (player != null)
            {
                player.OnPlayerDied += HandlePlayerDeath;
            }
        }
    }

    private void OnDestroy()
    {
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied -= HandleNPCDeath;
        }

        foreach (var player in players)
        {
            if (player != null)
            {
                player.OnPlayerDied -= HandlePlayerDeath;
            }
        }
    }

    private void HandleNPCDeath(int npcID)
    {
        if (gameOverTriggered) return;

        
        if (npcToProtect == null)
        {
            npcToProtect = FindNPCByTag();
            if (npcToProtect == null)
            {

                return;
            }
        }

        TriggerGameOver();
    }

    private void HandlePlayerDeath(int playerID)
    {
        if (gameOverTriggered) return;

        deadPlayersCount++;


        if (deadPlayersCount >= players.Count)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        gameOverTriggered = true;
        
        
        if (autoPauseOnGameOver)
        {
            PauseGame();
        }
        
        StartCoroutine(GameOverSequence());
    }
    
    
    
    
    private void PauseGame()
    {
        originalTimeScale = Time.timeScale;
        wasGamePaused = Mathf.Approximately(Time.timeScale, 0f);
        
        if (!wasGamePaused)
        {
            Time.timeScale = 0f;

        }
        
        
        DisableAllPlayerInput();
        
        
        DisableAllEnemyAI();
        
        
        PauseGameAudio();
    }
    
    
    
    
    public void ManualPause()
    {
        PauseGame();
    }
    
    public void ManualResume()
    {
        ResumeGame();
    }
    
    private void ResumeGame()
    {
        if (!wasGamePaused)
        {
            Time.timeScale = originalTimeScale;

        }
        
        
        EnableAllPlayerInput();
        
        
        EnableAllEnemyAI();
        
        
        ResumeGameAudio();
    }
    
    
    
    
    private void DisableAllPlayerInput()
    {
        var playerInputs = FindObjectsOfType<UnityEngine.InputSystem.PlayerInput>();
        foreach (var input in playerInputs)
        {
            input.enabled = true;
            if (input.actions != null)
            {
                input.SwitchCurrentActionMap("UI");
            }
        }
        
        var playerControllers = FindObjectsOfType<PlayerControllerBase>();
        foreach (var controller in playerControllers)
        {
            controller.enabled = false;
        }
        

    }
    
    
    
    
    private void EnableAllPlayerInput()
    {
        var playerInputs = FindObjectsOfType<UnityEngine.InputSystem.PlayerInput>();
        foreach (var input in playerInputs)
        {
            input.enabled = true;
            if (input.actions != null)
            {
                input.SwitchCurrentActionMap("Player");
            }
        }
        
        var playerControllers = FindObjectsOfType<PlayerControllerBase>();
        foreach (var controller in playerControllers)
        {
            controller.enabled = true;
        }
        

    }
    
    
    
    
    private void DisableAllEnemyAI()
    {
        var navMeshAgents = FindObjectsOfType<UnityEngine.AI.NavMeshAgent>();
        foreach (var agent in navMeshAgents)
        {
            if (agent.gameObject.CompareTag("Enemy"))
            {
                agent.enabled = false;
            }
        }
    }
    
    private void EnableAllEnemyAI()
    {
        var navMeshAgents = FindObjectsOfType<UnityEngine.AI.NavMeshAgent>();
        foreach (var agent in navMeshAgents)
        {
            if (agent.gameObject.CompareTag("Enemy"))
            {
                agent.enabled = true;
            }
        }
    }
    
    
    
    
    private void PauseGameAudio()
    {
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            
            if (audioSource.GetComponentInParent<Canvas>() == null)
            {
                audioSource.Pause();
            }
        }
        

    }
    
    
    
    
    private void ResumeGameAudio()
    {
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            if (audioSource.GetComponentInParent<Canvas>() == null)
            {
                audioSource.UnPause();
            }
        }
        

    }

    public void AssignNPCToProtect(NPCHealth npc)
    {
        
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied -= HandleNPCDeath;
        }

        npcToProtect = npc;

        
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied += HandleNPCDeath;
            
            
            if (npcToProtect.gameObject.tag != npcTag && !string.IsNullOrEmpty(npcTag))
            {
                npcToProtect.gameObject.tag = npcTag;

            }
            

        }
    }

    private IEnumerator GameOverSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        yield return new WaitForSecondsRealtime(reloadDelay);
        
        ShowGameOverUI();
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float fadeProgress = elapsed / fadeDuration;
                
                
                float smoothProgress = Mathf.SmoothStep(0f, 1f, fadeProgress);
                
                
                color.r = 0f;
                color.g = 0f;
                color.b = 0f;
                color.a = smoothProgress;
                
                fadeImage.color = color;
                yield return null;
            }
            
            
            color = Color.black;
            color.a = 1f;
            fadeImage.color = color;
        }
        yield return null;
    }

    private void CreateFadeScreen()
    {
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;
        fadeCanvas.gameObject.SetActive(false);

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = Color.black;

        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private IEnumerator InitialFadeIn()
    {
        if (fadeImage != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }
            
            fadeCanvas.gameObject.SetActive(false);
        }
        yield return null;
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }
            
            fadeCanvas.gameObject.SetActive(false);
        }
        yield return null;
    }

    private void RespawnAllPlayers()
    {
        
        if (GameManager.Instance != null)
        {
            
            foreach (var player in players)
            {
                if (player != null && player.GetComponent<PlayerIdentifier>() != null)
                {
                    int playerID = player.GetComponent<PlayerIdentifier>().playerID;
                    GameManager.Instance.RespawnPlayer(playerID);
                }
            }
        }
        else
        {
            
            foreach (var player in players)
            {
                if (player != null && player.IsDead)
                {
                    
                    player.RestoreState();
                }
            }
        }
    }

    private void ResetGameOverState()
    {
        
        deadPlayersCount = 0;
        gameOverTriggered = false;
        
        
        players = new List<PlayerHealth>(FindObjectsOfType<PlayerHealth>());
        
        
        foreach (var player in players)
        {
            if (player != null)
            {
                player.OnPlayerDied += HandlePlayerDeath;
            }
        }
    }

    private void SetupButtonListeners()
    {
        internalRetryButton = retryButton != null ? retryButton : internalRetryButton;
        internalQuitButton = quitButton != null ? quitButton : internalQuitButton;

        if (internalRetryButton != null)
        {
            internalRetryButton.onClick.RemoveAllListeners();
            internalRetryButton.onClick.AddListener(() => PlayClickSound());
            internalRetryButton.onClick.AddListener(OnContinueButtonClicked);
            AddButtonTriggers(internalRetryButton);
            
            internalRetryButton.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = internalRetryButton.colors;
            cb.normalColor = new Color(1, 1, 1, 1f);
            cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            internalRetryButton.colors = cb;
        }

        if (internalQuitButton != null)
        {
            internalQuitButton.onClick.RemoveAllListeners();
            internalQuitButton.onClick.AddListener(() => PlayClickSound());
            internalQuitButton.onClick.AddListener(OnQuitButtonClicked);
            AddButtonTriggers(internalQuitButton);
            
            internalQuitButton.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = internalQuitButton.colors;
            cb.normalColor = new Color(1, 1, 1, 1f);
            cb.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            internalQuitButton.colors = cb;
        }
        
        if (internalRetryButton != null && internalQuitButton != null)
        {
            Navigation retryNav = new Navigation();
            retryNav.mode = Navigation.Mode.Explicit;
            retryNav.selectOnDown = internalQuitButton;
            retryNav.selectOnUp = internalQuitButton;
            internalRetryButton.navigation = retryNav;

            Navigation quitNav = new Navigation();
            quitNav.mode = Navigation.Mode.Explicit;
            quitNav.selectOnUp = internalRetryButton;
            quitNav.selectOnDown = internalRetryButton;
            internalQuitButton.navigation = quitNav;
        }
    }

    private void AddButtonTriggers(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entrySelect = new EventTrigger.Entry();
        entrySelect.eventID = EventTriggerType.Select;
        entrySelect.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entrySelect);
    }

    private void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverSound != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    private void CreateGameOverUI()
    {
        // Ensure EventSystem exists for UI navigation
         if (EventSystem.current == null)
         {
             GameObject eventSystemGO = new GameObject("EventSystem");
             eventSystemGO.AddComponent<EventSystem>();
             var uiModule = eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            var playerInputs = FindObjectsOfType<UnityEngine.InputSystem.PlayerInput>();
            UnityEngine.InputSystem.PlayerInput provider = null;
            if (playerInputs != null && playerInputs.Length > 0) provider = playerInputs[0];
            if (provider != null && provider.actions != null)
            {
                var uiMap = provider.actions.FindActionMap("UI");
                if (uiMap != null)
                {
                    var navigate = uiMap.FindAction("Navigate");
                    var submit = uiMap.FindAction("Submit");
                    var cancel = uiMap.FindAction("Cancel");
                    if (navigate != null) uiModule.move = UnityEngine.InputSystem.InputActionReference.Create(navigate);
                    if (submit != null) uiModule.submit = UnityEngine.InputSystem.InputActionReference.Create(submit);
                    if (cancel != null) uiModule.cancel = UnityEngine.InputSystem.InputActionReference.Create(cancel);
                }
            }
         }
 
        // Use user-provided canvas or prefab; avoid auto-creation unless enabled
        if (gameOverButtonsCanvas != null)
        {
            gameOverCanvas = gameOverButtonsCanvas;
            if (gameOverCanvas != null) gameOverCanvas.gameObject.SetActive(false);
            return;
        }
        if (gameOverCanvasPrefab != null)
        {
            var go = Instantiate(gameOverCanvasPrefab);
            gameOverButtonsCanvas = go.GetComponentInChildren<Canvas>();
            if (gameOverButtonsCanvas == null) gameOverButtonsCanvas = go.GetComponent<Canvas>();
            gameOverCanvas = gameOverButtonsCanvas;
            if (gameOverCanvas != null) gameOverCanvas.gameObject.SetActive(false);
            return;
        }
        if (!autoCreateGameOverUI)
        {
            return;
        }
 
         GameObject canvasGO = new GameObject("GameOverCanvas");
        canvasGO.transform.SetParent(transform);
        gameOverCanvas = canvasGO.AddComponent<Canvas>();
        gameOverButtonsCanvas = gameOverCanvas;
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameOverCanvas.sortingOrder = 1001; 
        gameOverCanvas.gameObject.SetActive(false);

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();

        
        GameObject panelGO = new GameObject("BackgroundPanel", typeof(RectTransform));
        panelGO.transform.SetParent(canvasGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f); 

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        GameObject textGO = new GameObject("GameOverText", typeof(RectTransform));
        textGO.transform.SetParent(canvasGO.transform, false);
        gameOverText = textGO.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.fontSize = gameOverTextSize; 
        gameOverText.color = new Color(1f, 1f, 1f, 0f); 
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontStyle = FontStyles.Bold;
        
        TMP_FontAsset liberationFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (liberationFont != null)
        {
            gameOverText.font = liberationFont;
        }

        gameOverText.enableVertexGradient = true;
        gameOverText.colorGradient = new VertexGradient(
            new Color(0.9f, 0.9f, 0.9f, 1f),      
            new Color(0.7f, 0.7f, 0.7f, 1f),      
            new Color(0.5f, 0.5f, 0.5f, 1f),      
            new Color(0.3f, 0.3f, 0.3f, 1f)       
        );
        
        gameOverText.gameObject.AddComponent<Shadow>().effectColor = new Color(0.05f, 0.05f, 0.05f, 0.6f);
        gameOverText.gameObject.GetComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
        
        gameOverText.gameObject.AddComponent<Outline>().effectColor = new Color(0.15f, 0.15f, 0.15f, 0.4f);
        gameOverText.gameObject.GetComponent<Outline>().effectDistance = new Vector2(1f, -1f);

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.65f);
        textRect.anchorMax = new Vector2(0.5f, 0.65f);
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = Vector2.zero;

        
        Material textMaterial = new Material(Shader.Find("TextMeshPro/Distance Field"));
        
        
        Texture2D grungeTexture = Resources.Load<Texture2D>("Dark UI/Textures/Grunge/Background 3");
        if (grungeTexture != null)
        {
            textMaterial.SetTexture("_MainTex", grungeTexture);
            textMaterial.SetFloat("_OutlineWidth", 0.1f);
            textMaterial.SetFloat("_OutlineSoftness", 0.2f);
            gameOverText.fontMaterial = textMaterial;
        }

        
        gameOverText.color = new Color(0.85f, 0.85f, 0.85f, 1f); 

        
        GameObject buttonContainer = new GameObject("ButtonContainer", typeof(RectTransform));
        buttonContainer.transform.SetParent(canvasGO.transform, false);
        buttonCanvasGroup = buttonContainer.AddComponent<CanvasGroup>();
        buttonCanvasGroup.alpha = 0f;

        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.35f);
        containerRect.anchorMax = new Vector2(0.5f, 0.35f);
        containerRect.sizeDelta = new Vector2(600, 300);
        containerRect.anchoredPosition = Vector2.zero;

        
        if (retryButton == null)
        {
            GameObject continueButtonGO = new GameObject("ContinueButton", typeof(RectTransform));
            continueButtonGO.transform.SetParent(buttonContainer.transform, false);
            internalRetryButton = continueButtonGO.AddComponent<Button>();
            
            Image continueImage = continueButtonGO.AddComponent<Image>();

            Texture2D buttonGrungeTexture = Resources.Load<Texture2D>("Dark UI/Textures/Grunge/Background 3");
            if (buttonGrungeTexture != null)
            {
                Sprite grungeSprite = Sprite.Create(
                    buttonGrungeTexture,
                    new Rect(0, 0, buttonGrungeTexture.width, buttonGrungeTexture.height),
                    new Vector2(0.5f, 0.5f));
                continueImage.sprite = grungeSprite;
                continueImage.type = Image.Type.Sliced;
                continueImage.color = Color.white;
            }
            else
            {
                continueImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            internalRetryButton.targetGraphic = continueImage;

            RectTransform continueRect = continueButtonGO.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.1f, 0.55f);
            continueRect.anchorMax = new Vector2(0.9f, 0.85f);
            continueRect.sizeDelta = Vector2.zero;
            continueRect.anchoredPosition = Vector2.zero;

            GameObject continueTextGO = new GameObject("ContinueText", typeof(RectTransform));
            continueTextGO.transform.SetParent(continueButtonGO.transform, false);
            TextMeshProUGUI continueText = continueTextGO.AddComponent<TextMeshProUGUI>();
            continueText.text = "CONTINUE";
            continueText.fontSize = 32;
            continueText.color = Color.white;
            continueText.alignment = TextAlignmentOptions.Center;

            RectTransform continueTextRect = continueTextGO.GetComponent<RectTransform>();
            continueTextRect.anchorMin = Vector2.zero;
            continueTextRect.anchorMax = Vector2.one;
            continueTextRect.sizeDelta = Vector2.zero;
            continueTextRect.anchoredPosition = Vector2.zero;
        }

        
        if (quitButton == null)
        {
            GameObject quitButtonGO = new GameObject("QuitButton", typeof(RectTransform));
            quitButtonGO.transform.SetParent(buttonContainer.transform, false);
            internalQuitButton = quitButtonGO.AddComponent<Button>();
            
            Image quitImage = quitButtonGO.AddComponent<Image>();

            Texture2D buttonGrungeTexture = Resources.Load<Texture2D>("Dark UI/Textures/Grunge/Background 3");
            if (buttonGrungeTexture != null)
            {
                Sprite grungeSpriteQuit = Sprite.Create(
                    buttonGrungeTexture,
                    new Rect(0, 0, buttonGrungeTexture.width, buttonGrungeTexture.height),
                    new Vector2(0.5f, 0.5f));
                quitImage.sprite = grungeSpriteQuit;
                quitImage.type = Image.Type.Sliced;
                quitImage.color = Color.white;
            }
            else
            {
                quitImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            internalQuitButton.targetGraphic = quitImage;

            RectTransform quitRect = quitButtonGO.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0.1f, 0.15f);
            quitRect.anchorMax = new Vector2(0.9f, 0.45f);
            quitRect.sizeDelta = Vector2.zero;
            quitRect.anchoredPosition = Vector2.zero;

            GameObject quitTextGO = new GameObject("QuitText", typeof(RectTransform));
            quitTextGO.transform.SetParent(quitButtonGO.transform, false);
            TextMeshProUGUI quitText = quitTextGO.AddComponent<TextMeshProUGUI>();
            quitText.text = "QUIT TO MENU";
            if (liberationFont != null) quitText.font = liberationFont;
            quitText.fontSize = 32;
            quitText.color = Color.white;
            quitText.alignment = TextAlignmentOptions.Center;

            RectTransform quitTextRect = quitTextGO.GetComponent<RectTransform>();
            quitTextRect.anchorMin = Vector2.zero;
            quitTextRect.anchorMax = Vector2.one;
            quitTextRect.sizeDelta = Vector2.zero;
            quitTextRect.anchoredPosition = Vector2.zero;
        }
    }

    private void ShowGameOverUI()
    {
        var targetCanvas = gameOverButtonsCanvas != null ? gameOverButtonsCanvas : gameOverCanvas;
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
            
            // Focus on the retry button for joystick navigation
            if (internalRetryButton != null)
            {
                internalRetryButton.Select();
                internalRetryButton.OnSelect(null);
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(internalRetryButton.gameObject);
                }
            }
            else
            {
                var firstBtn = targetCanvas.GetComponentInChildren<Button>();
                if (firstBtn != null)
                {
                    firstBtn.Select();
                    if (EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(firstBtn.gameObject);
                    }
                }
            }
            
            if (gameOverText != null)
            {
                StartCoroutine(FadeInGameOverText());
            }
        }
    }
    
    private IEnumerator FadeInGameOverText()
    {
        if (gameOverText != null)
        {
            // Ensure buttons are active and set to 0 alpha
            if (buttonCanvasGroup != null) buttonCanvasGroup.alpha = 0f;
            
            Color textColor = gameOverText.color;
            textColor.a = 0f;
            gameOverText.color = textColor;
            
            float duration = 1.5f; // Slightly faster
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                
                textColor.a = alpha;
                gameOverText.color = textColor;

                if (buttonCanvasGroup != null)
                {
                    buttonCanvasGroup.alpha = alpha;
                }
                
                yield return null;
            }
            
            textColor.a = 1f;
            gameOverText.color = textColor;
            if (buttonCanvasGroup != null) buttonCanvasGroup.alpha = 1f;
            
            StartCoroutine(ShakeGameOverText());
        }
    }
    
    private IEnumerator ShakeGameOverText()
    {
        if (gameOverText != null)
        {
            RectTransform textRect = gameOverText.GetComponent<RectTransform>();
            Vector3 originalPosition = textRect.anchoredPosition;
            
            float elapsed = 0f;
            float smoothShakeSpeed = 8f; 
            
            while (elapsed < shakeDuration)
            {
                
                float shakeProgress = elapsed / shakeDuration;
                float intensityMultiplier = (1f - shakeProgress) * 0.5f; 
                
                
                float shakeX = Mathf.Sin(elapsed * smoothShakeSpeed) * shakeIntensity * intensityMultiplier;
                float shakeY = Mathf.Cos(elapsed * smoothShakeSpeed * 0.7f) * shakeIntensity * 0.5f * intensityMultiplier;
                
                textRect.anchoredPosition = originalPosition + new Vector3(shakeX, shakeY, 0f);
                
                
                float flicker = Mathf.Lerp(0.95f, 1f, Mathf.Sin(elapsed * 4f) * 0.5f + 0.5f);
                Color currentColor = gameOverText.color;
                currentColor.a = flicker;
                gameOverText.color = currentColor;
                
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            
            
            float returnDuration = 0.3f;
            float returnElapsed = 0f;
            Vector3 currentPos = textRect.anchoredPosition;
            
            while (returnElapsed < returnDuration)
            {
                returnElapsed += Time.unscaledDeltaTime;
                float t = returnElapsed / returnDuration;
                t = Mathf.SmoothStep(0f, 1f, t); 
                
                textRect.anchoredPosition = Vector3.Lerp(currentPos, originalPosition, t);
                yield return null;
            }
            
            
            textRect.anchoredPosition = originalPosition;
            gameOverText.color = Color.white;
        }
    }

    private void HideGameOverUI()
    {
        var targetCanvas = gameOverButtonsCanvas != null ? gameOverButtonsCanvas : gameOverCanvas;
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(false);
        }
    }

    private void OnContinueButtonClicked()
    {

        
        HideGameOverUI();
        
        
        ResumeGame();
        
        
        ResetGameOverState();
        
        
        StartCoroutine(RestartLevel());
    }
    
    
    
    
    private IEnumerator RestartLevel()
    {
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        

        
        
        yield return StartCoroutine(FadeToBlack());
        
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        
        SceneManager.LoadScene(currentSceneName);
        

    }

    private void OnQuitButtonClicked()
    {
        // Stop any pending logic
        StopAllCoroutines();
        
        // Ensure time is back to normal
        Time.timeScale = 1f;
        
        // Load the main menu scene
        SceneManager.LoadScene("Main Menu");
    }

    private IEnumerator FadeInAfterUI()
    {
        yield return StartCoroutine(FadeIn());
        
        ResetGameOverState();
    }

    private IEnumerator SearchForNPCLater()
    {
        yield return new WaitForSeconds(2f);
        
        while (npcToProtect == null)
        {
            npcToProtect = FindNPCByTag();
            
            if (npcToProtect != null)
            {

                
                if (npcToProtect != null)
                {
                    npcToProtect.OnNPCDied += HandleNPCDeath;
                }
                break;
            }
            
            yield return new WaitForSeconds(3f);
        }
    }

    
}

