using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerPopupBillboard : MonoBehaviour
{
    [Header("Panel del popup (World Space)")]
    [SerializeField] private GameObject popupPanel;

    public GameObject GetPopupPanel() => popupPanel;

    [Header("Texto dentro del popup")]
    [SerializeField] private TextMeshProUGUI popupText;
    [Tooltip("Tamano de fuente del texto. Ajusta este valor si el texto no entra bien en el canvas.")]
    public float fontSize = 30f;

    [Header("Duracin de la animacin")]
    [SerializeField] private float appearDuration = 0.2f;
    [SerializeField] private float disappearDuration = 0.2f;

    [Header("Posicion flotante")]
    [SerializeField] private float verticalOffset = 2f;
    [Tooltip("Offset lateral del canvas. Se controla automaticamente durante el dialogo.")]
    [SerializeField] private float lateralOffset = 0f;
    [Tooltip("Velocidad de transicion del offset lateral (mayor = mas rapido).")]
    [SerializeField] private float offsetTransitionSpeed = 5f;

    [Header("Tamano del Canvas (para calculo automatico)")]
    [Tooltip("Ancho estimado del canvas en unidades del mundo. Si es 0, se calcula automaticamente.")]
    [SerializeField] private float canvasWidth = 0f;
    [Tooltip("Alto estimado del canvas en unidades del mundo. Si es 0, se calcula automaticamente.")]
    [SerializeField] private float canvasHeight = 0f;

    

    private Coroutine currentRoutine;
    private Coroutine offsetTransitionCoroutine;
    private Vector3 originalScale;
    private Camera mainCam;
    private float currentLateralOffset = 0f;
    private float targetLateralOffset = 0f;

    void Start()
    {
        mainCam = Camera.main;

        if (popupPanel != null)
        {
            originalScale = popupPanel.transform.localScale;
            popupPanel.SetActive(false);
        }

        if (popupText == null)
            popupText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void LateUpdate()
    {
        if (popupPanel != null && popupPanel.activeSelf && mainCam != null)
        {
            lateralOffset = currentLateralOffset;

            Vector3 basePosition = transform.position + Vector3.up * verticalOffset;

            if (lateralOffset != 0f)
            {
                Vector3 cameraRight = mainCam.transform.right;
                basePosition += cameraRight * lateralOffset;
            }

            popupPanel.transform.position = basePosition;

            Vector3 lookDirection = mainCam.transform.position - popupPanel.transform.position;
            lookDirection.y = 0;
            popupPanel.transform.rotation = Quaternion.LookRotation(-lookDirection);
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

        if (popupPanel != null)
        {
            RectTransform panelRect = popupPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                Vector3 localScale = popupPanel.transform.lossyScale;
                size = new Vector2(
                    panelRect.rect.width * localScale.x,
                    panelRect.rect.height * localScale.y
                );
            }
        }

        if (size.x == 0f) size.x = canvasWidth > 0f ? canvasWidth : 2f;
        if (size.y == 0f) size.y = canvasHeight > 0f ? canvasHeight : 1f;

        return size;
    }

    public Vector3 GetCanvasWorldPosition()
    {
        if (popupPanel != null)
            return popupPanel.transform.position;
        return transform.position + Vector3.up * verticalOffset;
    }

    
    
    
    
    public void UpdateTextInstant(string message)
    {
        if (popupPanel == null || popupText == null) return;

        
        popupText.fontSize = fontSize;
        popupText.text = message;

        
        if (popupPanel.activeSelf && popupPanel.transform.localScale == originalScale)
        {
            
            return;
        }

        
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        popupPanel.SetActive(true);
        popupPanel.transform.localScale = originalScale; 

        
        currentRoutine = null;
    }

    
    
    
    public void ShowMessage(string message, float time = 2f)
    {
        if (popupPanel == null || popupText == null) return;

        popupText.fontSize = fontSize;
        popupText.text = message;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        popupPanel.SetActive(true);
        popupPanel.transform.localScale = Vector3.zero;
        StartCoroutine(AnimatePopup(popupPanel.transform, Vector3.zero, originalScale, appearDuration));

        currentRoutine = StartCoroutine(HideAfterDelay(time));
    }

    private IEnumerator HideAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        yield return HidePopup();
    }

    private IEnumerator AnimatePopup(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / duration);
            target.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        target.localScale = to;
    }

    private IEnumerator HidePopup()
    {
        if (popupPanel == null) yield break;

        Transform t = popupPanel.transform;
        float time = 0;
        Vector3 startScale = t.localScale;

        while (time < disappearDuration)
        {
            time += Time.deltaTime;
            float tLerp = Mathf.SmoothStep(0, 1, time / disappearDuration);
            t.localScale = Vector3.Lerp(startScale, Vector3.zero, tLerp);
            yield return null;
        }

        popupPanel.SetActive(false);
    }

    public void Hide()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        StartCoroutine(HidePopup());
    }

    
}
