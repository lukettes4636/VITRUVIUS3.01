using System.Collections;
using UnityEngine;
using TMPro;
using System; 

public class PlayerUIController : MonoBehaviour
{
    
    [Header("Game Manager")]
    [Tooltip("Asignar el script GameManager aqu. (No usado directamente para respawn, pero se mantiene).")]
    [SerializeField] private MonoBehaviour gameManager;

    
    [Header("UI Notification (solo compatibilidad, no visible)")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayTime = 3.0f;
    private Coroutine hideCoroutine;

    
    [Header("UI Respawn References")]
    [Tooltip("Panel del respawn del jugador.")]
    [SerializeField] private GameObject respawnPanel;
    [Tooltip("Texto que muestra la cuenta regresiva o el mensaje de respawn.")]
    [SerializeField] private TextMeshProUGUI respawnTimerText;
    [SerializeField] private float respawnTime = 5.0f;

    private Coroutine respawnCoroutine;
    private Action onRespawnReadyCallback;

    
    [Header("Popup Flotante sobre la cabeza del jugador")]
    [Tooltip("Referencia al PlayerPopupBillboard del mismo jugador.")]
    [SerializeField] private PlayerPopupBillboard popupBillboard;

    [SerializeField] private float popupDisplayTime = 2.5f;

    void Start()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (respawnPanel != null)
            respawnPanel.SetActive(false);

        if (popupBillboard == null)
            popupBillboard = GetComponent<PlayerPopupBillboard>();
    }

    
    
    

    public void StartRespawnTimer(Action readyCallback)
    {
        if (respawnPanel == null || respawnTimerText == null)
        {

            return;
        }

        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        onRespawnReadyCallback = readyCallback;
        respawnPanel.SetActive(true);

        respawnCoroutine = StartCoroutine(RespawnCountdown());
    }

    private IEnumerator RespawnCountdown()
    {
        float timer = respawnTime;

        while (timer > 0)
        {
            respawnTimerText.text = $"Reaparicion en: {Mathf.Ceil(timer):0}";
            timer -= Time.deltaTime;
            yield return null;
        }

        onRespawnReadyCallback?.Invoke();

        respawnTimerText.text = "Press Submit [A] to respawn";
        respawnCoroutine = null;
    }

    public void HideRespawnPanel()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
        if (respawnPanel != null)
            respawnPanel.SetActive(false);
    }

    
    
    

    
    
    
    public void ShowNotification(string message)
    {
        if (popupBillboard != null)
        {
            popupBillboard.ShowMessage(message, popupDisplayTime);
            return;
        }

        
        if (notificationText != null)
        {
            notificationText.text = message;
            if (notificationPanel != null)
                notificationPanel.SetActive(true);

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideNotificationAfterDelay());
        }
        else
        {

        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
        hideCoroutine = null;
    }
}
