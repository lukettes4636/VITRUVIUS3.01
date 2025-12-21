using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SimpleEndDemo : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup uiCanvasGroup;
    public GameObject uiRoot; 
    public TextMeshProUGUI messageText;
    public Button continueButton;
    public string menuSceneName = "Main Menu";

    private bool triggered = false;

    private void Start()
    {
        
        if (uiCanvasGroup == null) uiCanvasGroup = GetComponentInChildren<CanvasGroup>();
        if (uiCanvasGroup == null && transform.parent != null) uiCanvasGroup = transform.parent.GetComponentInChildren<CanvasGroup>();
        
        if (uiRoot == null) uiRoot = uiCanvasGroup ? uiCanvasGroup.gameObject : (transform.parent != null ? transform.parent.Find("Canvas")?.gameObject : transform.Find("Canvas")?.gameObject);
        
        if (messageText == null) messageText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (messageText == null && transform.parent != null) messageText = transform.parent.GetComponentInChildren<TextMeshProUGUI>(true);

        if (continueButton == null) continueButton = GetComponentInChildren<Button>(true);
        if (continueButton == null && transform.parent != null) continueButton = transform.parent.GetComponentInChildren<Button>(true);

        
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.blocksRaycasts = false;
        }
        else if (uiRoot != null)
        {
            uiRoot.SetActive(false);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        
        if (other.CompareTag("Player") || other.GetComponent<PlayerIdentifier>() != null || other.GetComponentInParent<PlayerIdentifier>() != null)
        {
            triggered = true;
            ShowEndScreen();
        }
    }

    private void ShowEndScreen()
    {
        
        if (messageText != null)
        {
            messageText.text = "END OF THE DEMO\n\nTHANKS FOR WATCHING";
        }

        
        if (uiRoot != null) uiRoot.SetActive(true);

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 1f;
            uiCanvasGroup.blocksRaycasts = true;
        }

        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnContinueClicked()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
