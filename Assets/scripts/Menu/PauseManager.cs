using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup pausePanelCanvasGroup; 
    [SerializeField] private TextMeshProUGUI pauseText; 
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    private bool isPaused = false;

    void OnEnable()
    {
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        
        if (pausePanelCanvasGroup != null) pausePanelCanvasGroup.alpha = 0f;
        if (pauseText != null) pauseText.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
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


        
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 1f; 
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;
        }
        if (pauseText != null) pauseText.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;


        
        if (pausePanelCanvasGroup != null)
        {
            pausePanelCanvasGroup.alpha = 0f; 
            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
        }
        if (pauseText != null) pauseText.gameObject.SetActive(false);
    }

    
    public void OnContinueButtonClicked()
    {
        ResumeGame();
    }

    public void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
