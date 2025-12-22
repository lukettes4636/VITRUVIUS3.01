using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// New independent GameOver system with canvas-based UI
/// Provides retry and quit functionality with visual effects
/// </summary>
public class NewGameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas gameOverCanvas;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button retryButton;
    public Button quitButton;
    
    [Header("Visual Settings")]
    public Color panelColor = new Color(0, 0, 0, 0.8f);
    public Color gameOverTextColor = Color.red;
    public float gameOverTextFontSize = 72f;
    public string gameOverTextString = "GAME OVER";
    
    [Header("Button Settings")]
    public string retryButtonText = "RETRY";
    public string quitButtonText = "QUIT";
    public Color buttonNormalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color buttonHighlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color buttonPressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color buttonTextColor = Color.white;
    public float buttonFontSize = 36f;
    
    [Header("Font Settings")]
    public TMP_FontAsset gameOverFont;
    public TMP_FontAsset buttonFont;
    
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
        
        CreateGameOverUI();
        HideGameOver();
    }
    
    /// <summary>
    /// Creates the complete GameOver UI system
    /// </summary>
    private void CreateGameOverUI()
    {
        // Create Canvas
        gameOverCanvas = gameObject.AddComponent<Canvas>();
        gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameOverCanvas.sortingOrder = 1000;
        
        // Add Canvas Scaler
        CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add Graphic Raycaster
        gameObject.AddComponent<GraphicRaycaster>();
        
        // Create Panel
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(gameOverCanvas.transform, false);
        
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = panelColor;
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        // Create GAME OVER text
        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverPanel.transform, false);
        
        gameOverText = textObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = gameOverTextString;
        gameOverText.color = gameOverTextColor;
        gameOverText.fontSize = gameOverTextFontSize;
        gameOverText.alignment = TextAlignmentOptions.Center;
        
        if (gameOverFont != null)
            gameOverText.font = gameOverFont;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.7f);
        textRect.anchorMax = new Vector2(0.5f, 0.7f);
        textRect.sizeDelta = new Vector2(400, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        // Create Button Container
        GameObject buttonContainer = new GameObject("ButtonContainer", typeof(RectTransform));
        buttonContainer.transform.SetParent(gameOverPanel.transform, false);
        
        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.3f);
        containerRect.anchorMax = new Vector2(0.5f, 0.3f);
        containerRect.sizeDelta = new Vector2(600, 200);
        containerRect.anchoredPosition = Vector2.zero;
        
        // Add Vertical Layout Group
        VerticalLayoutGroup layoutGroup = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 20f;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        
        // Create RETRY Button
        retryButton = CreateButton("RetryButton", retryButtonText, buttonContainer.transform);
        retryButton.onClick.AddListener(OnRetryClicked);
        
        // Create QUIT Button
        quitButton = CreateButton("QuitButton", quitButtonText, buttonContainer.transform);
        quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    /// <summary>
    /// Creates a styled button with text
    /// </summary>
    private Button CreateButton(string name, string buttonText, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Button Image
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonNormalColor;
        
        // Button RectTransform
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(250, 60);
        
        // Button Colors
        ColorBlock colors = button.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHighlightedColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonHighlightedColor;
        button.colors = colors;
        
        // Button Text
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.color = buttonTextColor;
        text.fontSize = buttonFontSize;
        text.alignment = TextAlignmentOptions.Center;
        
        if (buttonFont != null)
            text.font = buttonFont;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return button;
    }
    
    /// <summary>
    /// Shows the GameOver screen
    /// </summary>
    public void ShowGameOver()
    {
        gameOverCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Hides the GameOver screen
    /// </summary>
    public void HideGameOver()
    {
        gameOverCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Handles retry button click - loads last checkpoint
    /// </summary>
    private void OnRetryClicked()
    {
        HideGameOver();
        LoadLastCheckpoint();
    }
    
    /// <summary>
    /// Handles quit button click
    /// </summary>
    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
        Debug.Log("Quit button pressed - would quit application in build");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Loads the last checkpoint using the checkpoint system
    /// </summary>
    private void LoadLastCheckpoint()
    {
        CheckpointSystem.Instance?.LoadLastCheckpoint();
    }
    
    #region Public Configuration Methods
    
    /// <summary>
    /// Sets the Game Over text
    /// </summary>
    public void SetGameOverText(string text)
    {
        gameOverTextString = text;
        if (gameOverText != null)
            gameOverText.text = text;
    }
    
    /// <summary>
    /// Sets the retry button text
    /// </summary>
    public void SetRetryButtonText(string text)
    {
        retryButtonText = text;
        TextMeshProUGUI retryText = retryButton.GetComponentInChildren<TextMeshProUGUI>();
        if (retryText != null)
            retryText.text = text;
    }
    
    /// <summary>
    /// Sets the quit button text
    /// </summary>
    public void SetQuitButtonText(string text)
    {
        quitButtonText = text;
        TextMeshProUGUI quitText = quitButton.GetComponentInChildren<TextMeshProUGUI>();
        if (quitText != null)
            quitText.text = text;
    }
    
    /// <summary>
    /// Sets button colors
    /// </summary>
    public void SetButtonColors(Color normal, Color highlighted, Color pressed)
    {
        buttonNormalColor = normal;
        buttonHighlightedColor = highlighted;
        buttonPressedColor = pressed;
        
        UpdateButtonColors(retryButton);
        UpdateButtonColors(quitButton);
    }
    
    /// <summary>
    /// Sets the panel color
    /// </summary>
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
    
    /// <summary>
    /// Updates button colors
    /// </summary>
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