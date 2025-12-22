using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndOfDemoController : MonoBehaviour
{
    private static EndOfDemoController instance;
    public static EndOfDemoController Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EndOfDemoController");
                instance = go.AddComponent<EndOfDemoController>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Text Settings")]
    [SerializeField] private string mainText = "END OF THE DEMO";
    [SerializeField] private string secondaryText = "THANKS FOR PLAYING";
    [SerializeField] private int mainTextSize = 72;
    [SerializeField] private int secondaryTextSize = 36;
    [SerializeField] private Color textColor = Color.white;

    [Header("Button Settings")]
    [SerializeField] private Color buttonNormalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color buttonHoverColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color buttonPressColor = new Color(0.1f, 0.1f, 0.1f, 1f);

    private GameObject endScreenRoot;
    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowEndScreen()
    {
        if (isShowing)
        {
            Debug.LogWarning("[EndOfDemo] End screen already showing, ignoring duplicate call.");
            return;
        }

        Debug.Log("[EndOfDemo] Showing end screen...");
        isShowing = true;

        if (endScreenRoot == null)
        {
            CreateEndScreenUI();
        }

        endScreenRoot.SetActive(true);
        StartCoroutine(FadeInCoroutine());

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Time.timeScale = 1f;
    }

    private void CreateEndScreenUI()
    {
        Debug.Log("[EndOfDemo] Creating end screen UI...");

        endScreenRoot = new GameObject("EndScreenCanvas");
        endScreenRoot.transform.SetParent(transform);

        Canvas canvas = endScreenRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = endScreenRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        endScreenRoot.AddComponent<GraphicRaycaster>();

        canvasGroup = endScreenRoot.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(endScreenRoot.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.95f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(endScreenRoot.transform, false);
        RectTransform containerRect = textContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0, 100);
        containerRect.sizeDelta = new Vector2(1200, 400);

        GameObject mainTextObj = new GameObject("MainText");
        mainTextObj.transform.SetParent(textContainer.transform, false);
        TextMeshProUGUI mainTMP = mainTextObj.AddComponent<TextMeshProUGUI>();
        mainTMP.text = mainText;
        mainTMP.fontSize = mainTextSize;
        mainTMP.color = textColor;
        mainTMP.alignment = TextAlignmentOptions.Center;
        mainTMP.fontStyle = FontStyles.Bold;
        RectTransform mainRect = mainTextObj.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0, 0.5f);
        mainRect.anchorMax = new Vector2(1, 1);
        mainRect.sizeDelta = Vector2.zero;
        mainRect.anchoredPosition = Vector2.zero;

        GameObject secondaryTextObj = new GameObject("SecondaryText");
        secondaryTextObj.transform.SetParent(textContainer.transform, false);
        TextMeshProUGUI secondaryTMP = secondaryTextObj.AddComponent<TextMeshProUGUI>();
        secondaryTMP.text = secondaryText;
        secondaryTMP.fontSize = secondaryTextSize;
        secondaryTMP.color = textColor;
        secondaryTMP.alignment = TextAlignmentOptions.Center;
        RectTransform secondaryRect = secondaryTextObj.GetComponent<RectTransform>();
        secondaryRect.anchorMin = new Vector2(0, 0);
        secondaryRect.anchorMax = new Vector2(1, 0.5f);
        secondaryRect.sizeDelta = Vector2.zero;
        secondaryRect.anchoredPosition = Vector2.zero;

        GameObject buttonObj = new GameObject("QuitButton");
        buttonObj.transform.SetParent(endScreenRoot.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, -200);
        buttonRect.sizeDelta = new Vector2(300, 80);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonNormalColor;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        ColorBlock colors = button.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = buttonPressColor;
        colors.selectedColor = buttonNormalColor;
        colors.fadeDuration = 0.15f;
        button.colors = colors;

        button.onClick.AddListener(OnQuitButtonPressed);

        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "QUIT";
        buttonText.fontSize = 36;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        endScreenRoot.SetActive(false);

        Debug.Log("[EndOfDemo] End screen UI created successfully.");
    }

    private IEnumerator FadeInCoroutine()
    {
        Debug.Log("[EndOfDemo] Starting fade-in animation...");
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            float curveValue = fadeCurve.Evaluate(t);
            canvasGroup.alpha = curveValue;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        Debug.Log("[EndOfDemo] Fade-in complete.");
    }

    private void OnQuitButtonPressed()
    {
        Debug.Log("[EndOfDemo] Quit button pressed. Exiting application...");

        #if UNITY_EDITOR
        Debug.Log("[EndOfDemo] Running in editor - would quit application in build.");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ResetEndScreen()
    {
        Debug.Log("[EndOfDemo] Resetting end screen state.");
        isShowing = false;
        if (endScreenRoot != null)
        {
            endScreenRoot.SetActive(false);
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}
