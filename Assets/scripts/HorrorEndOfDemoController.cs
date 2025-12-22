using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;

public class HorrorEndOfDemoController : MonoBehaviour
{
    private static HorrorEndOfDemoController instance;
    public static HorrorEndOfDemoController Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("HorrorEndOfDemoController");
                instance = go.AddComponent<HorrorEndOfDemoController>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Font Settings")]
    public TMP_FontAsset larkeSansFont;

    [Header("Horror Visual Theme")]
    public Color backgroundColor = new Color(0.05f, 0, 0, 0.98f);
    public Color bloodRedText = new Color(0.7f, 0.05f, 0.05f, 1f);
    public Color crackedWhiteText = new Color(0.9f, 0.85f, 0.8f, 1f);
    public Color buttonNormalColor = new Color(0.2f, 0.02f, 0.02f, 0.95f);
    public Color buttonHoverColor = new Color(0.4f, 0.05f, 0.05f, 1f);
    public Color buttonPressedColor = new Color(0.1f, 0.01f, 0.01f, 1f);
    public Color buttonSelectedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
    public Color buttonTextColor = new Color(0.95f, 0.9f, 0.85f, 1f);

    [Header("Text Content")]
    public string mainText = "END OF THE DEMO";
    public string secondaryText = "THE NIGHTMARE CONTINUES...";
    public string buttonText = "ESCAPE";
    public float mainTextSize = 84f;
    public float secondaryTextSize = 42f;
    public float buttonFontSize = 44f;

    [Header("Animation Settings")]
    public float fadeInDuration = 3f;
    public float textFadeDelay = 0.5f;
    public float buttonFadeDelay = 1.5f;
    public float buttonTransitionSpeed = 0.2f;
    public float buttonHoverScale = 1.08f;
    public float textGlitchSpeed = 3f;
    public float textGlitchIntensity = 0.2f;

    [Header("Audio Settings")]
    public AudioClip horrorAmbientSound;
    public AudioClip staticNoiseSound;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public float ambientVolume = 0.5f;
    public float staticVolume = 0.3f;
    public float hoverVolume = 0.6f;
    public float clickVolume = 0.8f;

    [Header("Haptic Settings")]
    public float buttonHoverHapticDuration = 0.15f;
    public float buttonHoverHapticIntensity = 0.4f;
    public float buttonClickHapticDuration = 0.25f;
    public float buttonClickHapticIntensity = 0.7f;

    [Header("Input System")]
    public InputActionAsset menuInputActions;
    private InputAction submitAction;

    private GameObject endScreenRoot;
    private CanvasGroup canvasGroup;
    private CanvasGroup textCanvasGroup;
    private CanvasGroup buttonCanvasGroup;
    private Button quitButton;
    private TextMeshProUGUI mainTMPText;
    private TextMeshProUGUI secondaryTMPText;
    private Vector3 buttonOriginalScale;
    private bool isShowing = false;
    private AudioSource ambientAudioSource;
    private AudioSource staticAudioSource;
    private AudioSource uiAudioSource;
    private EventSystem eventSystem;
    private Gamepad currentGamepad;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = ambientVolume;

        staticAudioSource = gameObject.AddComponent<AudioSource>();
        staticAudioSource.playOnAwake = false;
        staticAudioSource.loop = true;
        staticAudioSource.volume = staticVolume;

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
    }

    private void SetupInputActions()
    {
        if (menuInputActions == null)
        {
            menuInputActions = Resources.Load<InputActionAsset>("Input/MenuNavigation");
            if (menuInputActions == null)
            {
                Debug.LogWarning("[HorrorEndOfDemo] MenuNavigation InputActionAsset not found");
                return;
            }
        }

        var menuMap = menuInputActions.FindActionMap("Menu");
        if (menuMap != null)
        {
            submitAction = menuMap.FindAction("Submit");
        }
    }

    private void OnEnable()
    {
        if (submitAction != null)
        {
            submitAction.Enable();
            submitAction.performed += OnSubmit;
        }
    }

    private void OnDisable()
    {
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmit;
            submitAction.Disable();
        }
    }

    private void Update()
    {
        if (isShowing && Gamepad.current != null && currentGamepad != Gamepad.current)
        {
            currentGamepad = Gamepad.current;
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!isShowing || quitButton == null) return;

        PlayClickSound();
        TriggerHapticFeedback(buttonClickHapticIntensity, buttonClickHapticIntensity, buttonClickHapticDuration);
        quitButton.onClick.Invoke();
    }

    public void ShowEndScreen()
    {
        if (isShowing)
        {
            Debug.LogWarning("[HorrorEndOfDemo] End screen already showing");
            return;
        }

        Debug.Log("[HorrorEndOfDemo] Showing horror end screen");
        isShowing = true;

        if (endScreenRoot == null)
        {
            CreateEndScreenUI();
        }

        endScreenRoot.SetActive(true);
        StartCoroutine(FadeInSequence());

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;

        if (horrorAmbientSound != null)
        {
            ambientAudioSource.clip = horrorAmbientSound;
            ambientAudioSource.Play();
        }

        if (staticNoiseSound != null)
        {
            staticAudioSource.clip = staticNoiseSound;
            staticAudioSource.Play();
            StartCoroutine(FadeOutStatic(2f));
        }
    }

    private void CreateEndScreenUI()
    {
        Debug.Log("[HorrorEndOfDemo] Creating horror end screen UI");

        endScreenRoot = new GameObject("HorrorEndScreenCanvas");
        endScreenRoot.transform.SetParent(transform, false);

        Canvas canvas = endScreenRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = endScreenRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        endScreenRoot.AddComponent<GraphicRaycaster>();

        canvasGroup = endScreenRoot.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(endScreenRoot.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundColor;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(endScreenRoot.transform, false);
        textCanvasGroup = textContainer.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        RectTransform containerRect = textContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.6f);
        containerRect.anchorMax = new Vector2(0.5f, 0.6f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(1400, 300);

        GameObject mainTextObj = new GameObject("MainText");
        mainTextObj.transform.SetParent(textContainer.transform, false);
        mainTMPText = mainTextObj.AddComponent<TextMeshProUGUI>();
        mainTMPText.text = mainText;
        mainTMPText.fontSize = mainTextSize;
        mainTMPText.color = bloodRedText;
        mainTMPText.alignment = TextAlignmentOptions.Center;
        mainTMPText.fontStyle = FontStyles.Bold;
        
        if (larkeSansFont != null)
        {
            mainTMPText.font = larkeSansFont;
        }
        else
        {
            Debug.LogWarning("[HorrorEndOfDemo] Larke Sans font not assigned");
        }
        
        RectTransform mainRect = mainTextObj.GetComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0, 0.55f);
        mainRect.anchorMax = new Vector2(1, 1);
        mainRect.sizeDelta = Vector2.zero;

        GameObject secondaryTextObj = new GameObject("SecondaryText");
        secondaryTextObj.transform.SetParent(textContainer.transform, false);
        secondaryTMPText = secondaryTextObj.AddComponent<TextMeshProUGUI>();
        secondaryTMPText.text = secondaryText;
        secondaryTMPText.fontSize = secondaryTextSize;
        secondaryTMPText.color = crackedWhiteText;
        secondaryTMPText.alignment = TextAlignmentOptions.Center;
        secondaryTMPText.fontStyle = FontStyles.Italic;
        
        if (larkeSansFont != null)
        {
            secondaryTMPText.font = larkeSansFont;
        }
        
        RectTransform secondaryRect = secondaryTextObj.GetComponent<RectTransform>();
        secondaryRect.anchorMin = new Vector2(0, 0);
        secondaryRect.anchorMax = new Vector2(1, 0.45f);
        secondaryRect.sizeDelta = Vector2.zero;

        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(endScreenRoot.transform, false);
        buttonCanvasGroup = buttonContainer.AddComponent<CanvasGroup>();
        buttonCanvasGroup.alpha = 0f;
        RectTransform btnContainerRect = buttonContainer.GetComponent<RectTransform>();
        btnContainerRect.anchorMin = new Vector2(0.5f, 0.35f);
        btnContainerRect.anchorMax = new Vector2(0.5f, 0.35f);
        btnContainerRect.pivot = new Vector2(0.5f, 0.5f);
        btnContainerRect.anchoredPosition = Vector2.zero;
        btnContainerRect.sizeDelta = new Vector2(400, 100);

        GameObject buttonObj = new GameObject("QuitButton");
        buttonObj.transform.SetParent(buttonContainer.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.sizeDelta = Vector2.zero;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonNormalColor;

        quitButton = buttonObj.AddComponent<Button>();
        quitButton.targetGraphic = buttonImage;

        ColorBlock colors = quitButton.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonSelectedColor;
        colors.fadeDuration = buttonTransitionSpeed;
        quitButton.colors = colors;

        quitButton.onClick.AddListener(OnQuitButtonPressed);

        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonTMP = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonTMP.text = buttonText;
        buttonTMP.fontSize = buttonFontSize;
        buttonTMP.color = buttonTextColor;
        buttonTMP.alignment = TextAlignmentOptions.Center;
        buttonTMP.fontStyle = FontStyles.Bold;
        
        if (larkeSansFont != null)
        {
            buttonTMP.font = larkeSansFont;
        }
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        buttonOriginalScale = buttonRect.localScale;

        AddButtonHoverEvents();

        endScreenRoot.SetActive(false);
        Debug.Log("[HorrorEndOfDemo] Horror end screen UI created");
    }

    private void AddButtonHoverEvents()
    {
        EventTrigger trigger = quitButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => {
            StartCoroutine(AnimateButtonHover(true));
            PlayHoverSound();
            TriggerHapticFeedback(buttonHoverHapticIntensity, 0f, buttonHoverHapticDuration);
            eventSystem.SetSelectedGameObject(quitButton.gameObject);
        });
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            StartCoroutine(AnimateButtonHover(false));
        });
        trigger.triggers.Add(pointerExit);

        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener((data) => {
            PlayClickSound();
            TriggerHapticFeedback(buttonClickHapticIntensity, buttonClickHapticIntensity, buttonClickHapticDuration);
        });
        trigger.triggers.Add(pointerClick);
    }

    private IEnumerator AnimateButtonHover(bool hovered)
    {
        if (quitButton == null) yield break;

        RectTransform rect = quitButton.GetComponent<RectTransform>();
        Image img = quitButton.GetComponent<Image>();

        Vector3 targetScale = hovered ? buttonOriginalScale * buttonHoverScale : buttonOriginalScale;
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

    private IEnumerator FadeInSequence()
    {
        canvasGroup.alpha = 0f;
        textCanvasGroup.alpha = 0f;
        buttonCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(textFadeDelay);

        elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            textCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / 1.5f);
            yield return null;
        }
        textCanvasGroup.alpha = 1f;

        StartCoroutine(GlitchText());

        yield return new WaitForSecondsRealtime(buttonFadeDelay);

        elapsed = 0f;
        while (elapsed < buttonTransitionSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            buttonCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / buttonTransitionSpeed);
            yield return null;
        }
        buttonCanvasGroup.alpha = 1f;

        eventSystem.SetSelectedGameObject(quitButton.gameObject);
        StartCoroutine(AnimateButtonHover(true));
    }

    private IEnumerator GlitchText()
    {
        while (isShowing && mainTMPText != null)
        {
            float glitch = Mathf.Sin(Time.unscaledTime * textGlitchSpeed) * textGlitchIntensity;
            float alpha = 1f + glitch;
            
            Color mainCol = mainTMPText.color;
            mainCol.a = Mathf.Clamp01(alpha);
            mainTMPText.color = mainCol;

            float flickerChance = Random.value;
            if (flickerChance > 0.97f)
            {
                mainTMPText.color = crackedWhiteText;
                yield return new WaitForSecondsRealtime(0.05f);
                mainTMPText.color = bloodRedText;
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutStatic(float duration)
    {
        float startVolume = staticVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            staticAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        staticAudioSource.Stop();
        staticAudioSource.volume = staticVolume;
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

    private void OnQuitButtonPressed()
    {
        Debug.Log("[HorrorEndOfDemo] ESCAPE button pressed - Exiting");
        StopAllCoroutines();

        if (ambientAudioSource.isPlaying)
            ambientAudioSource.Stop();
        if (staticAudioSource.isPlaying)
            staticAudioSource.Stop();

        #if UNITY_EDITOR
        Debug.Log("[HorrorEndOfDemo] Stopping editor playmode");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ResetEndScreen()
    {
        Debug.Log("[HorrorEndOfDemo] Resetting end screen");
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
