using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class EndLevelTrigger : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup fadePanelCanvasGroup;
    public GameObject endDemoTextObject;
    public GameObject continueButtonObject;
    public Button continueButton;

    [Header("Splash Settings")]
    public RuntimeAnimatorController splashAnimatorController;
    public TMP_FontAsset splashFont;

    [Header("Player Input")]
    public PlayerInput playerOneInput;
    public PlayerInput playerTwoInput;

    [Header("Configuracin")]
    public string mainMenuScene = "Main Menu";
    public float fadeDuration = 1.5f;

    private bool triggered = false;

    private void Start()
    {
        
        if (fadePanelCanvasGroup != null)
        {
            fadePanelCanvasGroup.alpha = 0f;
        }
        if (continueButtonObject != null)
        {
            continueButtonObject.SetActive(false);
        }
        if (endDemoTextObject != null)
        {
            endDemoTextObject.SetActive(false); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && (other.GetComponent<CharacterController>() != null))
        {
            triggered = true;

            
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            StartCoroutine(FadeAndShowUI());
        }
    }

    private IEnumerator FadeAndShowUI()
    {
        
        float startTime = Time.time;
        float endAlpha = 1f; 

        while (fadePanelCanvasGroup.alpha < endAlpha)
        {
            float t = (Time.time - startTime) / fadeDuration;
            fadePanelCanvasGroup.alpha = Mathf.Lerp(0f, endAlpha, t);
            yield return null;
        }

        fadePanelCanvasGroup.alpha = endAlpha; 

        
        if (splashAnimatorController == null)
        {
            splashAnimatorController = Resources.Load<RuntimeAnimatorController>("EndGame/SplashTitleTransition");
        }
        if (splashFont == null)
        {
            splashFont = Resources.Load<TMP_FontAsset>("EndGame/LarkeSansLightSDF");
        }

        if (splashAnimatorController != null && splashFont != null)
        {
             
             GameObject splashObj = new GameObject("Splash_EndDemo");
             
             
             if (fadePanelCanvasGroup != null) 
             {
                splashObj.transform.SetParent(fadePanelCanvasGroup.transform, false);
             }
             else
             {
                  splashObj.transform.SetParent(this.transform, false);
             }

             
             splashObj.transform.localPosition = Vector3.zero;
             splashObj.transform.localRotation = Quaternion.identity;
             splashObj.transform.localScale = Vector3.one;
            
             
             CanvasGroup cg = splashObj.AddComponent<CanvasGroup>();
             cg.alpha = 0f; 

             
             RectTransform rt = splashObj.AddComponent<RectTransform>();
             
             rt.anchorMin = new Vector2(0.5f, 0.5f);
             rt.anchorMax = new Vector2(0.5f, 0.5f);
             rt.pivot = new Vector2(0.5f, 0.5f);
             rt.anchoredPosition = Vector2.zero; 
             rt.sizeDelta = new Vector2(1500, 200); 

             
             TextMeshProUGUI tmp = splashObj.AddComponent<TextMeshProUGUI>();
             if (splashFont != null) tmp.font = splashFont;
             tmp.text = "END OF THE DEMO";
             tmp.fontSize = 90; 
             tmp.alignment = TextAlignmentOptions.Center; 
             tmp.color = new Color(1f, 1f, 1f, 1f);
             
             
             Animator anim = splashObj.AddComponent<Animator>();
             anim.runtimeAnimatorController = splashAnimatorController;
             
          
             float textTimer = 0f;
             float textDuration = 4.0f;
             while (textTimer < textDuration)
             {
                 textTimer += Time.deltaTime;
                 cg.alpha = Mathf.Lerp(0f, 1f, textTimer / textDuration);
                 yield return null;
             }
             cg.alpha = 1f;
             



        }
        else if (endDemoTextObject != null)
        {
            endDemoTextObject.SetActive(true);
        }

        
        if (continueButtonObject != null)
        {
            
            if (fadePanelCanvasGroup != null)
            {
                continueButtonObject.transform.SetParent(fadePanelCanvasGroup.transform, false);
            }

            continueButtonObject.SetActive(true);

            

            
            RectTransform btnRect = continueButtonObject.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                
                btnRect.anchorMin = new Vector2(0.5f, 0.5f);
                btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                btnRect.pivot = new Vector2(0.5f, 0.5f);
                
                
                
                btnRect.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                
                
                
                btnRect.anchoredPosition = new Vector2(0f, -150f);
            }

            
            Button btn = continueButton;
            if (btn == null)
            {
                btn = continueButtonObject.GetComponent<Button>();
                if (btn == null) btn = continueButtonObject.GetComponentInChildren<Button>();
            }

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {

                    LoadMainMenuScene();
                });
            }
        }

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
             GameObject eventSystem = new GameObject("EventSystem");
             eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
             eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        if (continueButton != null)
        {
            continueButton.Select();
        }

        
        if (playerOneInput != null)
        {
            playerOneInput.SwitchCurrentActionMap("UI");
        }
        if (playerTwoInput != null)
        {
            playerTwoInput.SwitchCurrentActionMap("UI");
        }

        
    }

    
    public void LoadMainMenuScene()
    {


        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerOneInput != null)
        {
            playerOneInput.SwitchCurrentActionMap("Player");
        }
        if (playerTwoInput != null)
        {
            playerTwoInput.SwitchCurrentActionMap("Player");
        }

        
        SceneManager.LoadScene(mainMenuScene);
    }
}
