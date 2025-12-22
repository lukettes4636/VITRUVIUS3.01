using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class NewGameOverManager : MonoBehaviour
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
    
    [Header("Visual Settings")]
    public Color panelColor = new Color(0, 0, 0, 0.95f);
    public Color gameOverTextColor = new Color(0.8f, 0.05f, 0.05f, 1f);
    public float gameOverTextFontSize = 96f;
    public string gameOverTextString = "GAME OVER";
    
    [Header("Button Settings")]
    public string retryButtonText = "RETRY";
    public string quitButtonText = "QUIT";
    public Color buttonNormalColor = new Color(0.15f, 0.05f, 0.05f, 0.9f);
    public Color buttonHighlightedColor = new Color(0.3f, 0.1f, 0.1f, 1f);
    public Color buttonPressedColor = new Color(0.05f, 0.02f, 0.02f, 1f);
    public Color buttonTextColor = new Color(0.9f, 0.8f, 0.8f, 1f);
    public float buttonFontSize = 48f;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 3.5f;
    public float textFadeInDuration = 1.5f;
    public float buttonFadeInDuration = 1f;
    public float textPulseSpeed = 2f;
    public float textPulseIntensity = 0.15f;
    
    [Header("Audio Settings")]
    public AudioClip horrorAmbientSound;
    public float ambientVolume = 0.4f;
    private AudioSource ambientAudioSource;
    
    [Header("Player Detection")]
    private List<PlayerHealth> playerHealthComponents = new List<PlayerHealth>();
    private int deadPlayersCount = 0;
    private bool gameOverTriggered = false;
    
    [Header("Confirmation Dialog")]
    private GameObject confirmationDialog;
    private bool showingConfirmation = false;
    
    private static NewGameOverManager instance;
    public static NewGameOverManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NewGameOverManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("NewGameOverManager");
                    instance = go.AddComponent<NewGameOverManager>();
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
        
        CreateGameOverUI();
        HideGameOver();
    }
    
    private void Start()
    {
        FindAndSubscribeToPlayers();
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
                Debug.Log($"[NewGameOverManager] Subscribed to player: {player.gameObject.name}");
            }
        }
        
        Debug.Log($"[NewGameOverManager] Monitoring {playerHealthComponents.Count} players");
    }
    
    private void OnPlayerDied(int playerID)
    {
        if (gameOverTriggered) return;
        
        deadPlayersCount++;
        Debug.Log($"[NewGameOverManager] Player {playerID} died. Total dead: {deadPlayersCount}/{playerHealthComponents.Count}");
        
        if (deadPlayersCount >= playerHealthComponents.Count && playerHealthComponents.Count > 0)
        {
            Debug.Log("[NewGameOverManager] All players dead - triggering Game Over");
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
        gameOverText.text = gameOverTextString;
        gameOverText.color = gameOverTextColor;
        gameOverText.fontSize = gameOverTextFontSize;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontStyle = FontStyles.Bold;
        
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
        
        retryButton = CreateButton("RetryButton", retryButtonText, buttonContainer.transform);
        retryButton.onClick.AddListener(OnRetryClicked);
        
        quitButton = CreateButton("QuitButton", quitButtonText, buttonContainer.transform);
        quitButton.onClick.AddListener(OnQuitClicked);
        
        CreateConfirmationDialog();
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
        colors.selectedColor = buttonHighlightedColor;
        colors.fadeDuration = 0.2f;
        button.colors = colors;
        
        GameObject textObj = new GameObject("ButtonText", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.color = buttonTextColor;
        text.fontSize = buttonFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return button;
    }
    
    private void CreateConfirmationDialog()
    {
        confirmationDialog = new GameObject("ConfirmationDialog", typeof(RectTransform));
        confirmationDialog.transform.SetParent(gameOverPanel.transform, false);
        confirmationDialog.SetActive(false);
        
        CanvasGroup dialogGroup = confirmationDialog.AddComponent<CanvasGroup>();
        
        Image dialogBg = confirmationDialog.AddComponent<Image>();
        dialogBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        
        RectTransform dialogRect = confirmationDialog.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = new Vector2(600, 300);
        dialogRect.anchoredPosition = Vector2.zero;
        
        GameObject confirmText = new GameObject("ConfirmText", typeof(RectTransform));
        confirmText.transform.SetParent(confirmationDialog.transform, false);
        
        TextMeshProUGUI confirmTMP = confirmText.AddComponent<TextMeshProUGUI>();
        confirmTMP.text = "QUIT GAME?";
        confirmTMP.fontSize = 48f;
        confirmTMP.color = new Color(0.9f, 0.8f, 0.8f, 1f);
        confirmTMP.alignment = TextAlignmentOptions.Center;
        confirmTMP.fontStyle = FontStyles.Bold;
        
        RectTransform confirmTextRect = confirmText.GetComponent<RectTransform>();
        confirmTextRect.anchorMin = new Vector2(0.1f, 0.6f);
        confirmTextRect.anchorMax = new Vector2(0.9f, 0.9f);
        confirmTextRect.sizeDelta = Vector2.zero;
        
        GameObject btnContainer = new GameObject("ConfirmButtons", typeof(RectTransform));
        btnContainer.transform.SetParent(confirmationDialog.transform, false);
        
        RectTransform btnContainerRect = btnContainer.GetComponent<RectTransform>();
        btnContainerRect.anchorMin = new Vector2(0.1f, 0.1f);
        btnContainerRect.anchorMax = new Vector2(0.9f, 0.5f);
        btnContainerRect.sizeDelta = Vector2.zero;
        
        HorizontalLayoutGroup hlg = btnContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        
        Button yesBtn = CreateSmallButton("YesButton", "YES", btnContainer.transform);
        yesBtn.onClick.AddListener(OnConfirmQuit);
        
        Button noBtn = CreateSmallButton("NoButton", "NO", btnContainer.transform);
        noBtn.onClick.AddListener(OnCancelQuit);
    }
    
    private Button CreateSmallButton(string name, string text, Transform parent)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        
        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = buttonNormalColor;
        
        ColorBlock colors = btn.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHighlightedColor;
        colors.pressedColor = buttonPressedColor;
        btn.colors = colors;
        
        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36f;
        tmp.color = buttonTextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        
        return btn;
    }
    
    public void ShowGameOver()
    {
        if (gameOverTriggered) return;
        
        gameOverTriggered = true;
        gameOverCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
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
        
        Debug.Log("[NewGameOverManager] Starting fade-in sequence");
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            panelCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        panelCanvasGroup.alpha = 1f;
        
        Debug.Log("[NewGameOverManager] Panel faded in, showing text");
        
        elapsed = 0f;
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            textCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / textFadeInDuration);
            yield return null;
        }
        textCanvasGroup.alpha = 1f;
        
        StartCoroutine(PulseText());
        
        Debug.Log("[NewGameOverManager] Text faded in, showing buttons");
        
        elapsed = 0f;
        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            buttonsCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / buttonFadeInDuration);
            yield return null;
        }
        buttonsCanvasGroup.alpha = 1f;
        
        if (ambientAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutAmbient(1.5f));
        }
        
        Debug.Log("[NewGameOverManager] Fade-in sequence complete");
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
        Debug.Log("[NewGameOverManager] RETRY clicked");
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
            Debug.LogWarning("[NewGameOverManager] No CheckpointSystem found, reloading current scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    private void OnQuitClicked()
    {
        Debug.Log("[NewGameOverManager] QUIT clicked, showing confirmation");
        confirmationDialog.SetActive(true);
        showingConfirmation = true;
    }
    
    private void OnConfirmQuit()
    {
        Debug.Log("[NewGameOverManager] Quit confirmed");
        StopAllCoroutines();
        
        if (ambientAudioSource.isPlaying)
        {
            ambientAudioSource.Stop();
        }
        
        #if UNITY_EDITOR
        Debug.Log("[NewGameOverManager] Would quit application in build");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void OnCancelQuit()
    {
        Debug.Log("[NewGameOverManager] Quit cancelled");
        confirmationDialog.SetActive(false);
        showingConfirmation = false;
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
    
    #region Public Configuration Methods
    
    public void SetGameOverText(string text)
    {
        gameOverTextString = text;
        if (gameOverText != null)
            gameOverText.text = text;
    }
    
    public void SetRetryButtonText(string text)
    {
        retryButtonText = text;
        TextMeshProUGUI retryText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
        if (retryText != null)
            retryText.text = text;
    }
    
    public void SetQuitButtonText(string text)
    {
        quitButtonText = text;
        TextMeshProUGUI quitText = quitButton.GetComponentInChildren<TextMeshProUGUI>();
        if (quitText != null)
            quitText.text = text;
    }
    
    public void SetButtonColors(Color normal, Color highlighted, Color pressed)
    {
        buttonNormalColor = normal;
        buttonHighlightedColor = highlighted;
        buttonPressedColor = pressed;
        
        UpdateButtonColors(retryButton);
        UpdateButtonColors(quitButton);
    }
    
    public void SetPanelColor(Color color)
    {
        panelColor = color;
        if (gameOverPanel != null)
        {
            Image panelImage = gameOverPanel.GetComponent<Image>();
            if (panelImage != null)
                panelImage.color = color;
        }
    }
    
    private void UpdateButtonColors(Button button)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = buttonNormalColor;
            colors.highlightedColor = buttonHighlightedColor;
            colors.pressedColor = buttonPressedColor;
            colors.selectedColor = buttonHighlightedColor;
            button.colors = colors;
        }
    }
    
    #endregion
}
