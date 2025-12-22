using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class EndLevelTrigger : MonoBehaviour
{
    [Serializable]
    private class PlayerInventorySnapshot
    {
        public List<string> keyCards;
        public List<string> items;
        public bool flashlightState;
    }

    private static Dictionary<int, PlayerInventorySnapshot> savedInventories = new Dictionary<int, PlayerInventorySnapshot>();
    private static bool hasPendingRestore;
    private static string pendingSceneName;
    private static bool sceneLoadedHookRegistered;

    [Header("Referencias")]
    public CanvasGroup fadePanelCanvasGroup;
    public GameObject endDemoTextObject;
    public GameObject continueButtonObject;
    public Button continueButton;
    public GameObject proximityIndicator; 

    [Header("Splash Settings")]
    public RuntimeAnimatorController splashAnimatorController;
    public TMP_FontAsset splashFont;

    [Header("Player Input")]
    public PlayerInput playerOneInput;
    public PlayerInput playerTwoInput;

    [Header("Configuracin")]
    public string mainMenuScene = "Main Menu";
    public float fadeDuration = 1.5f;
    public string nextSceneName = "DaVinciP1";
    public bool requireBothPlayers = true;
    public float proximityRange = 5f; 

    private bool transitionStarted = false;
    private HashSet<int> playersInTrigger = new HashSet<int>();
    private Dictionary<int, PlayerIdentifier> playersById = new Dictionary<int, PlayerIdentifier>();

    private void Update()
    {
        UpdateProximityIndicator();
    }

    private void UpdateProximityIndicator()
    {
        if (proximityIndicator == null || transitionStarted) return;

        bool anyPlayerNear = false;
        PlayerIdentifier[] allPlayers = GameObject.FindObjectsOfType<PlayerIdentifier>();
        
        foreach (var player in allPlayers)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= proximityRange)
            {
                anyPlayerNear = true;
                break;
            }
        }

        if (proximityIndicator.activeSelf != anyPlayerNear)
        {
            proximityIndicator.SetActive(anyPlayerNear);

        }
    }

    private static void EnsureSceneLoadedHook()
    {
        if (sceneLoadedHookRegistered) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneLoadedHookRegistered = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingRestore) return;
        if (!string.IsNullOrEmpty(pendingSceneName) && !string.Equals(scene.name, pendingSceneName, StringComparison.Ordinal))
        {
            return;
        }



        try
        {
            PlayerIdentifier[] identifiers = GameObject.FindObjectsOfType<PlayerIdentifier>();
            for (int i = 0; i < identifiers.Length; i++)
            {
                PlayerIdentifier identifier = identifiers[i];
                if (identifier == null) continue;

                PlayerInventorySnapshot snapshot;
                if (!savedInventories.TryGetValue(identifier.playerID, out snapshot))
                {
                    continue;
                }

                PlayerInventory inventory = identifier.playerInventory != null
                    ? identifier.playerInventory
                    : identifier.GetComponent<PlayerInventory>();

                if (inventory != null && snapshot != null)
                {
                    
                    if (!snapshot.items.Contains("Flashlight"))
                    {
                        snapshot.items.Add("Flashlight");
                    }

                    inventory.RestoreInventory(snapshot.keyCards, snapshot.items);


                    
                    FlashlightController flashlight = identifier.GetComponentInChildren<FlashlightController>(true);
                    if (flashlight != null)
                    {
                        flashlight.SetFlashlightState(snapshot.flashlightState, true);

                    }
                }
            }
        }
        catch (Exception)
        {

        }
        finally
        {
            hasPendingRestore = false;
            pendingSceneName = null;
            savedInventories.Clear();
        }
    }

    private void Start()
    {
        EnsureSceneLoadedHook();
        
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
        if (proximityIndicator != null)
        {
            proximityIndicator.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transitionStarted) return;

        PlayerIdentifier identifier = other.GetComponent<PlayerIdentifier>() ?? other.GetComponentInParent<PlayerIdentifier>();
        if (identifier == null) return;

        int playerID = identifier.playerID;
        if (!playersInTrigger.Contains(playerID))
        {
            playersInTrigger.Add(playerID);
            playersById[playerID] = identifier;

        }

        if (requireBothPlayers)
        {
            if (playersInTrigger.Contains(1) && playersInTrigger.Contains(2))
            {

                StartCoroutine(BeginTransition());
            }
        }
        else
        {
            StartCoroutine(BeginTransition());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (transitionStarted) return;

        PlayerIdentifier identifier = other.GetComponent<PlayerIdentifier>() ?? other.GetComponentInParent<PlayerIdentifier>();
        if (identifier == null) return;

        int playerID = identifier.playerID;
        if (playersInTrigger.Contains(playerID))
        {
            playersInTrigger.Remove(playerID);

        }
        if (playersById.ContainsKey(playerID))
        {
            playersById.Remove(playerID);
        }
    }

    private void SaveInventories()
    {
        savedInventories.Clear();

        foreach (KeyValuePair<int, PlayerIdentifier> kvp in playersById)
        {
            PlayerIdentifier identifier = kvp.Value;
            if (identifier == null) continue;

            PlayerInventory inventory = identifier.playerInventory != null
                ? identifier.playerInventory
                : identifier.GetComponent<PlayerInventory>();

            if (inventory == null) continue;

            FlashlightController flashlight = identifier.GetComponentInChildren<FlashlightController>(true);
            bool fState = flashlight != null ? flashlight.isFlashlightOn : false;

            List<string> keyCards = inventory.GetCollectedKeyCards();
            List<string> items = inventory.GetCollectedItems();

            PlayerInventorySnapshot snapshot = new PlayerInventorySnapshot
            {
                keyCards = keyCards != null ? new List<string>(keyCards) : new List<string>(),
                items = items != null ? new List<string>(items) : new List<string>(),
                flashlightState = fState
            };

            savedInventories[identifier.playerID] = snapshot;

        }
    }

    private void DisablePlayerControls()
    {
        foreach (KeyValuePair<int, PlayerIdentifier> kvp in playersById)
        {
            PlayerIdentifier identifier = kvp.Value;
            if (identifier == null) continue;

            CharacterController cc = identifier.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            PlayerInput input = identifier.GetComponent<PlayerInput>();
            if (input != null)
            {
                input.enabled = false;
            }
        }
    }

    private IEnumerator BeginTransition()
    {
        if (transitionStarted)
        {
            yield break;
        }

        transitionStarted = true;

        SaveInventories();
        hasPendingRestore = savedInventories.Count > 0;

        string targetScene = string.IsNullOrEmpty(nextSceneName) ? "DaVinciP1" : nextSceneName;
        pendingSceneName = targetScene;

        DisablePlayerControls();

        if (fadePanelCanvasGroup != null)
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
        }

        yield return StartCoroutine(LoadSceneAsyncInternal(targetScene));
    }

    private IEnumerator LoadSceneAsyncInternal(string sceneName)
    {
        bool usedManager = false;

        if (SceneLoadManager.Instance != null)
        {
            try
            {
                SceneLoadManager.Instance.LoadScene(sceneName);
                usedManager = true;
            }
            catch (Exception)
            {

            }
        }

        if (usedManager)
        {
            yield break;
        }

        AsyncOperation asyncOp = null;
        try
        {
            asyncOp = SceneManager.LoadSceneAsync(sceneName);
        }
        catch (Exception)
        {

            yield break;
        }

        if (asyncOp == null)
        {

            yield break;
        }

        asyncOp.allowSceneActivation = true;
        while (!asyncOp.isDone)
        {
            yield return null;
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

            // Ensure button visuals
            Image btnImg = continueButtonObject.GetComponent<Image>();
            if (btnImg != null)
            {
                 // Ensure it's visible even if sprite is missing
                 if (btnImg.sprite == null) 
                 {
                     btnImg.sprite = null; // Clear potential broken reference
                     btnImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                 }
            }

            // Ensure button text
            TextMeshProUGUI btnText = continueButtonObject.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText == null)
            {
                 GameObject textObj = new GameObject("Text");
                 textObj.transform.SetParent(continueButtonObject.transform, false);
                 btnText = textObj.AddComponent<TextMeshProUGUI>();
                 RectTransform textRect = textObj.GetComponent<RectTransform>();
                 textRect.anchorMin = Vector2.zero;
                 textRect.anchorMax = Vector2.one;
                 textRect.sizeDelta = Vector2.zero;
            }
            
            if (string.IsNullOrEmpty(btnText.text)) btnText.text = "QUIT";
            if (splashFont != null) btnText.font = splashFont;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 32;
            btnText.color = Color.black;

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
