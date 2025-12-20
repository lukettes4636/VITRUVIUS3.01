using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DaVinciP1PauseFix : EditorWindow
{
    private static bool verboseLogging = true;
    
    [MenuItem("Tools/Fix DaVinciP1 Pause System")]
    public static void ShowWindow()
    {
        GetWindow<DaVinciP1PauseFix>("Fix DaVinciP1 Pause");
    }
    
    void OnGUI()
    {
        GUILayout.Label("DaVinciP1 Pause System Fix", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will:");
        GUILayout.Label("• Replace PauseManager with PauseController");
        GUILayout.Label("• Create all necessary UI elements");
        GUILayout.Label("• Set up proper references");
        GUILayout.Label("• Fix GameOverManager auto-pause");
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Fix Pause System", GUILayout.Height(40)))
        {
            FixPauseSystem();
        }
        
        GUILayout.Space(10);
        
        verboseLogging = EditorGUILayout.Toggle("Verbose Logging", verboseLogging);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Test Pause Input"))
        {
            TestPauseInput();
        }
        
        if (GUILayout.Button("Check Pause Status"))
        {
            CheckPauseStatus();
        }
    }
    
    static void FixPauseSystem()
    {
        Debug.Log("=== Starting DaVinciP1 Pause System Fix ===");
        
        // Find and replace PauseManager
        GameObject pauseManagerObj = GameObject.Find("PauseManager");
        if (pauseManagerObj != null)
        {
            Debug.Log($"Found PauseManager GameObject: {pauseManagerObj.name}");
            
            // Remove old PauseManager
            var oldPauseManager = pauseManagerObj.GetComponent<PauseManager>();
            if (oldPauseManager != null)
            {
                Debug.Log("Removing old PauseManager component...");
                DestroyImmediate(oldPauseManager);
            }
            
            // Add PauseController
            PauseController pauseController = pauseManagerObj.GetComponent<PauseController>();
            if (pauseController == null)
            {
                Debug.Log("Adding PauseController component...");
                pauseController = pauseManagerObj.AddComponent<PauseController>();
            }
            
            // Setup UI elements
            SetupUIElements(pauseController, pauseManagerObj);
            
            // Fix GameOverManager
            FixGameOverManager();
            
            // Ensure game doesn't start paused
            EnsureGameDoesntStartPaused();
            
            Debug.Log("=== Pause System Fix Complete ===");
            EditorUtility.DisplayDialog("Success", "Pause system has been fixed!\n\nTest the Start button on your joystick.", "OK");
        }
        else
        {
            Debug.LogError("Could not find PauseManager GameObject. Please ensure you have a PauseManager in your scene.");
            EditorUtility.DisplayDialog("Error", "Could not find PauseManager GameObject in the scene.", "OK");
        }
    }
    
    static void SetupUIElements(PauseController pauseController, GameObject pauseManagerObj)
    {
        Debug.Log("Setting up UI elements...");
        
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Created new Canvas");
        }
        
        // Create pause panel
        GameObject pausePanel = GameObject.Find("PausePanel");
        if (pausePanel == null)
        {
            pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvas.transform);
            pausePanel.transform.localPosition = Vector3.zero;
            pausePanel.AddComponent<RectTransform>();
        }
        
        // Add CanvasGroup to panel
        CanvasGroup panelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
        }
        
        // Set CanvasGroup properties
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        
        // Setup RectTransform for pause panel
        RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        // Create pause text
        GameObject pauseTextObj = GameObject.Find("PauseText");
        if (pauseTextObj == null)
        {
            pauseTextObj = new GameObject("PauseText");
            pauseTextObj.transform.SetParent(pausePanel.transform);
        }
        
        TextMeshProUGUI pauseText = pauseTextObj.GetComponent<TextMeshProUGUI>();
        if (pauseText == null)
        {
            pauseText = pauseTextObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Configure pause text
        pauseText.text = "PAUSE";
        pauseText.fontSize = 72;
        pauseText.alignment = TextAlignmentOptions.Center;
        pauseText.color = Color.white;
        
        // Setup RectTransform for pause text
        RectTransform pauseTextRect = pauseText.GetComponent<RectTransform>();
        pauseTextRect.anchorMin = new Vector2(0.5f, 0.8f);
        pauseTextRect.anchorMax = new Vector2(0.5f, 0.8f);
        pauseTextRect.sizeDelta = new Vector2(200, 100);
        pauseTextRect.anchoredPosition = Vector2.zero;
        
        // Create continue button
        GameObject continueButtonObj = GameObject.Find("ContinueButton");
        if (continueButtonObj == null)
        {
            continueButtonObj = new GameObject("ContinueButton");
            continueButtonObj.transform.SetParent(pausePanel.transform);
        }
        
        Button continueButton = continueButtonObj.GetComponent<Button>();
        if (continueButton == null)
        {
            continueButton = continueButtonObj.AddComponent<Button>();
        }
        
        // Create continue button text
        GameObject continueTextObj = continueButtonObj.transform.Find("Text")?.gameObject;
        if (continueTextObj == null)
        {
            continueTextObj = new GameObject("Text");
            continueTextObj.transform.SetParent(continueButtonObj.transform);
        }
        
        TextMeshProUGUI continueText = continueTextObj.GetComponent<TextMeshProUGUI>();
        if (continueText == null)
        {
            continueText = continueTextObj.AddComponent<TextMeshProUGUI>();
        }
        
        continueText.text = "CONTINUE";
        continueText.fontSize = 36;
        continueText.alignment = TextAlignmentOptions.Center;
        continueText.color = Color.white;
        
        // Setup RectTransform for continue button
        RectTransform continueButtonRect = continueButton.GetComponent<RectTransform>();
        if (continueButtonRect == null)
        {
            continueButtonRect = continueButtonObj.AddComponent<RectTransform>();
        }
        continueButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        continueButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        continueButtonRect.sizeDelta = new Vector2(200, 60);
        continueButtonRect.anchoredPosition = new Vector2(0, 40);
        
        // Setup RectTransform for continue text
        RectTransform continueTextRect = continueText.GetComponent<RectTransform>();
        continueTextRect.anchorMin = Vector2.zero;
        continueTextRect.anchorMax = Vector2.one;
        continueTextRect.sizeDelta = Vector2.zero;
        continueTextRect.anchoredPosition = Vector2.zero;
        
        // Create quit button
        GameObject quitButtonObj = GameObject.Find("QuitButton");
        if (quitButtonObj == null)
        {
            quitButtonObj = new GameObject("QuitButton");
            quitButtonObj.transform.SetParent(pausePanel.transform);
        }
        
        Button quitButton = quitButtonObj.GetComponent<Button>();
        if (quitButton == null)
        {
            quitButton = quitButtonObj.AddComponent<Button>();
        }
        
        // Create quit button text
        GameObject quitTextObj = quitButtonObj.transform.Find("Text")?.gameObject;
        if (quitTextObj == null)
        {
            quitTextObj = new GameObject("Text");
            quitTextObj.transform.SetParent(quitButtonObj.transform);
        }
        
        TextMeshProUGUI quitText = quitTextObj.GetComponent<TextMeshProUGUI>();
        if (quitText == null)
        {
            quitText = quitTextObj.AddComponent<TextMeshProUGUI>();
        }
        
        quitText.text = "QUIT";
        quitText.fontSize = 36;
        quitText.alignment = TextAlignmentOptions.Center;
        quitText.color = Color.white;
        
        // Setup RectTransform for quit button
        RectTransform quitButtonRect = quitButton.GetComponent<RectTransform>();
        if (quitButtonRect == null)
        {
            quitButtonRect = quitButtonObj.AddComponent<RectTransform>();
        }
        quitButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        quitButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        quitButtonRect.sizeDelta = new Vector2(200, 60);
        quitButtonRect.anchoredPosition = new Vector2(0, -40);
        
        // Setup RectTransform for quit text
        RectTransform quitTextRect = quitText.GetComponent<RectTransform>();
        quitTextRect.anchorMin = Vector2.zero;
        quitTextRect.anchorMax = Vector2.one;
        quitTextRect.sizeDelta = Vector2.zero;
        quitTextRect.anchoredPosition = Vector2.zero;
        
        // Setup audio source
        AudioSource audioSource = pauseManagerObj.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = pauseManagerObj.AddComponent<AudioSource>();
        }
        
        // Setup blur volume
        var blurVolume = FindObjectOfType<UnityEngine.Rendering.Volume>();
        if (blurVolume != null)
        {
            Debug.Log("Found blur volume in scene");
        }
        
        // Now use reflection to set the private fields
        SetupPauseControllerReferences(pauseController, pausePanel, pauseText, continueButton, quitButton);
        
        Debug.Log("UI elements setup complete");
    }
    
    static void SetupPauseControllerReferences(PauseController pauseController, GameObject pausePanel, TextMeshProUGUI pauseText, Button continueButton, Button quitButton)
    {
        Debug.Log("Setting up PauseController references...");
        
        // Use reflection to set private fields
        var type = pauseController.GetType();
        
        // Set pausePanelCanvasGroup
        var canvasGroupField = type.GetField("pausePanelCanvasGroup", BindingFlags.NonPublic | BindingFlags.Instance);
        if (canvasGroupField != null)
        {
            canvasGroupField.SetValue(pauseController, pausePanel.GetComponent<CanvasGroup>());
            Debug.Log("Set pausePanelCanvasGroup reference");
        }
        
        // Set pauseText
        var pauseTextField = type.GetField("pauseText", BindingFlags.NonPublic | BindingFlags.Instance);
        if (pauseTextField != null)
        {
            pauseTextField.SetValue(pauseController, pauseText);
            Debug.Log("Set pauseText reference");
        }
        
        // Set pauseTitleRectTransform
        var pauseTitleField = type.GetField("pauseTitleRectTransform", BindingFlags.NonPublic | BindingFlags.Instance);
        if (pauseTitleField != null)
        {
            pauseTitleField.SetValue(pauseController, pauseText.GetComponent<RectTransform>());
            Debug.Log("Set pauseTitleRectTransform reference");
        }
        
        // Set continueButton
        var continueButtonField = type.GetField("continueButton", BindingFlags.NonPublic | BindingFlags.Instance);
        if (continueButtonField != null)
        {
            continueButtonField.SetValue(pauseController, continueButton);
            Debug.Log("Set continueButton reference");
        }
        
        // Set quitButton
        var quitButtonField = type.GetField("quitButton", BindingFlags.NonPublic | BindingFlags.Instance);
        if (quitButtonField != null)
        {
            quitButtonField.SetValue(pauseController, quitButton);
            Debug.Log("Set quitButton reference");
        }
        
        // Set audioSource
        var audioSourceField = type.GetField("audioSource", BindingFlags.NonPublic | BindingFlags.Instance);
        if (audioSourceField != null)
        {
            var audioSource = pauseController.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSourceField.SetValue(pauseController, audioSource);
                Debug.Log("Set audioSource reference");
            }
        }
        
        // Set blurVolume
        var blurVolumeField = type.GetField("blurVolume", BindingFlags.NonPublic | BindingFlags.Instance);
        if (blurVolumeField != null)
        {
            var blurVolume = FindObjectOfType<UnityEngine.Rendering.Volume>();
            if (blurVolume != null)
            {
                blurVolumeField.SetValue(pauseController, blurVolume);
                Debug.Log("Set blurVolume reference");
            }
        }
    }
    
    static void FixGameOverManager()
    {
        Debug.Log("Fixing GameOverManager...");
        
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            // Use reflection to set the autoPauseOnGameOver field
            var type = gameOverManager.GetType();
            var autoPauseField = type.GetField("autoPauseOnGameOver", BindingFlags.NonPublic | BindingFlags.Instance);
            if (autoPauseField != null)
            {
                autoPauseField.SetValue(gameOverManager, false);
                Debug.Log("Disabled auto-pause in GameOverManager");
            }
            else
            {
                Debug.LogWarning("Could not find autoPauseOnGameOver field in GameOverManager");
            }
        }
        else
        {
            Debug.LogWarning("Could not find GameOverManager in the scene");
        }
    }
    
    static void TestPauseInput()
    {
        Debug.Log("=== Testing Pause Input ===");
        
        PauseController pauseController = FindObjectOfType<PauseController>();
        if (pauseController != null)
        {
            Debug.Log("Found PauseController, attempting to toggle pause...");
            pauseController.TogglePause();
        }
        else
        {
            Debug.LogError("No PauseController found in scene");
        }
    }
    
    static void CheckPauseStatus()
    {
        Debug.Log("=== Checking Pause Status ===");
        
        PauseController pauseController = FindObjectOfType<PauseController>();
        if (pauseController != null)
        {
            var type = pauseController.GetType();
            var isPausedField = type.GetField("isPaused", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isPausedField != null)
            {
                bool isPaused = (bool)isPausedField.GetValue(pauseController);
                Debug.Log($"PauseController isPaused: {isPaused}");
            }
            
            var canvasGroupField = type.GetField("pausePanelCanvasGroup", BindingFlags.NonPublic | BindingFlags.Instance);
            if (canvasGroupField != null)
            {
                var canvasGroup = canvasGroupField.GetValue(pauseController) as CanvasGroup;
                if (canvasGroup != null)
                {
                    Debug.Log($"PausePanel alpha: {canvasGroup.alpha}, interactable: {canvasGroup.interactable}");
                }
                else
                {
                    Debug.LogWarning("pausePanelCanvasGroup is null");
                }
            }
        }
        
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            var type = gameOverManager.GetType();
            var autoPauseField = type.GetField("autoPauseOnGameOver", BindingFlags.NonPublic | BindingFlags.Instance);
            if (autoPauseField != null)
            {
                bool autoPause = (bool)autoPauseField.GetValue(gameOverManager);
                Debug.Log($"GameOverManager autoPauseOnGameOver: {autoPause}");
            }
        }
        
        Debug.Log($"Time.timeScale: {Time.timeScale}");
    }
    
    static void EnsureGameDoesntStartPaused()
    {
        Debug.Log("Ensuring game doesn't start paused...");
        
        // Check current time scale
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Debug.LogWarning("Time.timeScale is 0, forcing it to 1 to prevent game starting paused");
            Time.timeScale = 1f;
        }
        
        // Check if any PauseController is paused
        PauseController pauseController = FindObjectOfType<PauseController>();
        if (pauseController != null)
        {
            var type = pauseController.GetType();
            var isPausedField = type.GetField("isPaused", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isPausedField != null)
            {
                bool isPaused = (bool)isPausedField.GetValue(pauseController);
                if (isPaused)
                {
                    Debug.LogWarning("PauseController isPaused is true, forcing it to false");
                    isPausedField.SetValue(pauseController, false);
                }
            }
        }
        
        Debug.Log($"Final Time.timeScale: {Time.timeScale}");
    }
}