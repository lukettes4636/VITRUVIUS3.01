using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class NPCPopupBillboard : MonoBehaviour
{
    [Header("UI (World Space)")]
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [Tooltip("Tamano de fuente del texto. Ajusta este valor si el texto no entra bien en el canvas.")]
    public float fontSize = 30f;

    [Header("Tamano del Panel")]
    [Tooltip("Referencias a los paneles de ambos jugadores. Se ajustara al panel del jugador activo durante el dialogo.")]
    [SerializeField] private GameObject player1PanelReference;
    [SerializeField] private GameObject player2PanelReference;
    [Tooltip("Si esta activado, ajustara el tamano del panel del NPC al mismo tamano que el del jugador activo.")]
    [SerializeField] private bool matchPlayerPanelSize = true;

    private GameObject currentActivePlayerPanel;

    [Header("Posicion")]
    [SerializeField] private float verticalOffset = 2.3f;
    [Tooltip("Offset lateral del canvas. Se controla automaticamente durante el dialogo.")]
    [SerializeField] private float lateralOffset = 0f;

    [Header("Tamano del Canvas (para calculo automatico)")]
    [Tooltip("Ancho estimado del canvas en unidades del mundo. Si es 0, se calcula automaticamente.")]
    [SerializeField] private float canvasWidth = 0f;
    [Tooltip("Alto estimado del canvas en unidades del mundo. Si es 0, se calcula automaticamente.")]
    [SerializeField] private float canvasHeight = 0f;

    [Header("Aparicion del texto")]
    [SerializeField] private float charRevealSpeed = 0.02f;
    [SerializeField] private float visibleDuration = 2f;

    [Header("Transicion de Posicion")]
    [Tooltip("Velocidad de transicion del offset lateral (mayor = mas rapido).")]
    [SerializeField] private float offsetTransitionSpeed = 5f;

    private Coroutine revealCoroutine;
    private Coroutine offsetTransitionCoroutine;
    private float timer;
    private bool isVisible;
    private float currentLateralOffset = 0f;
    private float targetLateralOffset = 0f;

    void Start()
    {
        if (popupCanvas != null)
            popupCanvas.enabled = false;

        if (matchPlayerPanelSize)
        {
            if (player1PanelReference == null || player2PanelReference == null)
            {
                PlayerPopupBillboard[] playerPopups = FindObjectsOfType<PlayerPopupBillboard>();
                foreach (PlayerPopupBillboard popup in playerPopups)
                {
                    GameObject panel = popup.GetPopupPanel();
                    if (panel != null)
                    {
                        GameObject parent = popup.gameObject;
                        while (parent != null && !parent.CompareTag("Player1") && !parent.CompareTag("Player2"))
                        {
                            parent = parent.transform.parent != null ? parent.transform.parent.gameObject : null;
                        }

                        if (parent != null)
                        {
                            if (parent.CompareTag("Player1") && player1PanelReference == null)
                                player1PanelReference = panel;
                            else if (parent.CompareTag("Player2") && player2PanelReference == null)
                                player2PanelReference = panel;
                        }
                        else
                        {
                            if (player1PanelReference == null)
                                player1PanelReference = panel;
                            else if (player2PanelReference == null)
                                player2PanelReference = panel;
                        }
                    }
                }
            }
        }
    }

    public void SetActivePlayerPanel(GameObject playerPanel)
    {
        if (!matchPlayerPanelSize || popupPanel == null || playerPanel == null) return;

        currentActivePlayerPanel = playerPanel;

        RectTransform npcPanelRect = popupPanel.GetComponent<RectTransform>();
        RectTransform playerPanelRect = playerPanel.GetComponent<RectTransform>();

        if (npcPanelRect != null && playerPanelRect != null)
        {
            npcPanelRect.sizeDelta = playerPanelRect.sizeDelta;
        }
    }

    public void SetActivePlayer(string playerTag)
    {
        if (!matchPlayerPanelSize || popupPanel == null) return;

        GameObject targetPanel = null;

        if (playerTag == "Player1" && player1PanelReference != null)
            targetPanel = player1PanelReference;
        else if (playerTag == "Player2" && player2PanelReference != null)
            targetPanel = player2PanelReference;

        if (targetPanel != null)
        {
            SetActivePlayerPanel(targetPanel);
        }
    }

    void Update()
    {
        if (popupCanvas == null) return;

        lateralOffset = currentLateralOffset;
        UpdateCanvasPosition();

        if (isVisible && popupText != null)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                Hide();
        }
    }

    public void SetLateralOffset(float offset)
    {
        targetLateralOffset = offset;

        if (offsetTransitionCoroutine != null)
            StopCoroutine(offsetTransitionCoroutine);

        offsetTransitionCoroutine = StartCoroutine(SmoothLateralOffsetTransition());
    }

    private IEnumerator SmoothLateralOffsetTransition()
    {
        float startOffset = currentLateralOffset;
        float elapsedTime = 0f;
        float duration = 1f / offsetTransitionSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            currentLateralOffset = Mathf.Lerp(startOffset, targetLateralOffset, t);
            yield return null;
        }

        currentLateralOffset = targetLateralOffset;
        offsetTransitionCoroutine = null;
    }

    public Vector2 GetCanvasWorldSize()
    {
        Vector2 size = Vector2.zero;

        if (popupCanvas != null)
        {
            RectTransform canvasRect = popupCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                Vector3 localScale = popupCanvas.transform.localScale;
                size = new Vector2(
                    canvasRect.rect.width * localScale.x,
                    canvasRect.rect.height * localScale.y
                );
            }
        }

        if (size.x == 0f) size.x = canvasWidth > 0f ? canvasWidth : 2f;
        if (size.y == 0f) size.y = canvasHeight > 0f ? canvasHeight : 1f;

        return size;
    }

    public Vector3 GetCanvasWorldPosition()
    {
        if (popupCanvas != null)
            return popupCanvas.transform.position;
        return transform.position + Vector3.up * verticalOffset;
    }

    private void UpdateCanvasPosition()
    {
        if (popupCanvas == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 basePosition = transform.position + Vector3.up * verticalOffset;

        if (lateralOffset != 0f)
        {
            Vector3 cameraRight = cam.transform.right;
            basePosition += cameraRight * lateralOffset;
        }

        popupCanvas.transform.position = basePosition;

        Vector3 lookPos = popupCanvas.transform.position + cam.transform.rotation * Vector3.forward;
        popupCanvas.transform.LookAt(lookPos, cam.transform.rotation * Vector3.up);
    }

    
    
    
    
    public void UpdateTextInstant(string message)
    {
        if (popupText == null || popupCanvas == null) return;

        
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        
        if (!popupCanvas.enabled)
            popupCanvas.enabled = true;

        
        popupText.fontSize = fontSize;
        popupText.text = message;

        
        timer = 999f;
        isVisible = true;
    }

    
    
    
    public void ShowMessage(string message, float duration)
    {
        if (popupText == null || popupCanvas == null) return;

        if (popupText != null)
            popupText.fontSize = fontSize;

        popupCanvas.enabled = true;
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);
        revealCoroutine = StartCoroutine(RevealText(message));

        timer = duration > 0f ? duration : visibleDuration;
        isVisible = true;
    }

    private IEnumerator RevealText(string message)
    {
        popupText.text = "";
        foreach (char c in message)
        {
            popupText.text += c;
            yield return new WaitForSeconds(charRevealSpeed);
        }
        revealCoroutine = null;
    }

    public void Hide()
    {
        if (popupCanvas == null) return;
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);
        isVisible = false;
        timer = 0f;
        popupCanvas.enabled = false;
        popupText.text = "";
    }
}