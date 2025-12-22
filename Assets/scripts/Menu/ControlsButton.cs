using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace VITRUVIUS.Menu
{
    public class ControlsButton : MonoBehaviour
    {
        [Header("Controls Canvas References")]
        [SerializeField] private GameObject controlsCanvas;
        [SerializeField] private CanvasGroup controlsCanvasGroup;
        
        [Header("Sprite References")]
        [SerializeField] private Sprite mainControlsSprite;
        [SerializeField] private Sprite secondaryBackgroundSprite;
        [SerializeField] private float secondarySpriteOpacity = 0.7f;
        
        [Header("UI Components")]
        [SerializeField] private Image mainControlsImage;
        [SerializeField] private Image secondaryBackgroundImage;
        [SerializeField] private Button backButton;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onControlsShown;
        [SerializeField] private UnityEvent onControlsHidden;
        
        private Button button;
        private PauseController pauseController;
        private bool isShowingControls = false;
        private bool isSelected = false;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            pauseController = FindObjectOfType<PauseController>();
            
            SetupControlsCanvas();
            SetupBackButton();
        }
        

        
        private void SetupBackButton()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HideControls);
            }
        }
        
        private void Update()
        {
            if (isSelected && !isShowingControls)
            {
                if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return))
                {
                    ShowControls();
                }
            }
        }
        
    public void ShowControls()
    {
        if (blockInputDuringTransition && isTransitioning)
        {
            return;
        }
        
        if (preventOverlap && IsAnyMenuActive())
        {
            return;
        }
        
        if (useSmoothTransitions)
        {
            StartCoroutine(TransitionToControls());
        }
        else
        {
            ShowControlsPanel();
        }
    }
    
    void ShowControlsPanel()
    {
        isShowingControls = true;
        
        if (controlsCanvas != null)
        {
            controlsCanvas.SetActive(true);
        }

        if (mainControlsImage != null)
        {
            mainControlsImage.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 1f;
            controlsCanvasGroup.interactable = true;
            controlsCanvasGroup.blocksRaycasts = true;
        }
        
        SelectFirstControlsButton();
    }
    
    System.Collections.IEnumerator TransitionToControls()
    {
        isTransitioning = true;
        isShowingControls = true;
        
        if (controlsCanvas != null)
        {
            controlsCanvas.SetActive(true);
        }

        if (mainControlsImage != null)
        {
            mainControlsImage.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(normalizedTime);
            
            if (controlsCanvasGroup != null)
            {
                controlsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, curveValue);
            }
            
            yield return null;
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 1f;
            controlsCanvasGroup.interactable = true;
            controlsCanvasGroup.blocksRaycasts = true;
        }
        
        SelectFirstControlsButton();
        isTransitioning = false;
    }
    
    bool IsAnyMenuActive()
    {
        return (controlsCanvasGroup != null && controlsCanvasGroup.alpha > 0) ||
               (pauseController?.IsPaused() ?? false);
    }
        
    public void HideControls()
    {
        if (blockInputDuringTransition && isTransitioning)
        {
            return;
        }
        
        if (useSmoothTransitions)
        {
            StartCoroutine(TransitionFromControls());
        }
        else
        {
            HideControlsPanel();
        }
    }
    
    void HideControlsPanel()
    {
        isShowingControls = false;
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
        }
        
        if (controlsCanvas != null)
        {
            controlsCanvas.SetActive(false);
        }
        
        if (pauseController != null)
        {
            pauseController.ShowPauseMenuFromControls();
        }
        else
        {
            if (button != null)
            {
                button.Select();
            }
        }
        
        onControlsHidden?.Invoke();
    }
    
    System.Collections.IEnumerator TransitionFromControls()
    {
        isTransitioning = true;
        
        float elapsedTime = 0f;
        float startAlpha = 1f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(normalizedTime);
            
            if (controlsCanvasGroup != null)
            {
                controlsCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, curveValue);
            }
            
            yield return null;
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
        }
        
        if (controlsCanvas != null)
        {
            controlsCanvas.SetActive(false);
        }
        
        isShowingControls = false;
        isTransitioning = false;
        
        if (pauseController != null)
        {
            pauseController.ShowPauseMenuFromControls();
        }
        else
        {
            if (button != null)
            {
                button.Select();
            }
        }
        
        onControlsHidden?.Invoke();
    }
        
        private void SetupSprites()
        {
            if (mainControlsImage != null && mainControlsSprite != null)
            {
                mainControlsImage.sprite = mainControlsSprite;
                mainControlsImage.gameObject.SetActive(true);
            }
            
            if (secondaryBackgroundImage != null && secondaryBackgroundSprite != null)
            {
                secondaryBackgroundImage.sprite = secondaryBackgroundSprite;
                Color tempColor = secondaryBackgroundImage.color;
                tempColor.a = secondarySpriteOpacity;
                secondaryBackgroundImage.color = tempColor;
                secondaryBackgroundImage.gameObject.SetActive(true);
            }
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }
        
        public bool IsShowingControls()
        {
            return isShowingControls;
        }
        
        private void OnValidate()
        {
            if (controlsCanvas != null && controlsCanvasGroup == null)
            {
                controlsCanvasGroup = controlsCanvas.GetComponent<CanvasGroup>();
            }
            
            if (mainControlsImage == null && controlsCanvas != null)
            {
                Transform mainImageTransform = controlsCanvas.transform.Find("MainControlsImage");
                if (mainImageTransform != null)
                {
                    mainControlsImage = mainImageTransform.GetComponent<Image>();
                }
            }
            
            if (secondaryBackgroundImage == null && controlsCanvas != null)
            {
                Transform secondaryTransform = controlsCanvas.transform.Find("SecondaryBackgroundImage");
                if (secondaryTransform != null)
                {
                    secondaryBackgroundImage = secondaryTransform.GetComponent<Image>();
                }
            }
            
            if (backButton == null && controlsCanvas != null)
            {
                Transform backButtonTransform = controlsCanvas.transform.Find("BackButton");
                if (backButtonTransform != null)
                {
                    backButton = backButtonTransform.GetComponent<Button>();
                }
            }
            
            if (secondarySpriteOpacity < 0f) secondarySpriteOpacity = 0f;
            if (secondarySpriteOpacity > 1f) secondarySpriteOpacity = 1f;
        }
        
        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HideControls);
            }
        }
    

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useSmoothTransitions = true;
    
    [Header("State Management")]
    [SerializeField] private bool preventOverlap = true;
    [SerializeField] private bool isTransitioning = false;
    private Coroutine currentTransition;
    
    [Header("Input Validation")]
    [SerializeField] private bool blockInputDuringTransition = true;



    
    void SetupControlsCanvas()
    {
        if (controlsCanvas == null)
        {
            return;
        }
        
        if (controlsCanvasGroup == null)
        {
            controlsCanvasGroup = controlsCanvas.GetComponent<CanvasGroup>();
        }
        
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
        }
        
        controlsCanvas.SetActive(false);
    }
    
    void SelectFirstControlsButton()
    {
        if (backButton != null)
        {
            backButton.Select();
        }
        else if (controlsCanvas != null)
        {
            Button firstButton = controlsCanvas.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                firstButton.Select();
            }
        }
    }
}
}
