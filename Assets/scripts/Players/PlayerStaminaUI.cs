using UnityEngine;
using UnityEngine.UI;
using System.Collections;





public class PlayerStaminaUI : MonoBehaviour
{
    [Header("UI World Space Settings")]
    [Tooltip("El CanvasGroup del World Space UI (barra de estamina).")]
    [SerializeField] private CanvasGroup staminaCanvasGroup;

    [Tooltip("El Slider de la barra de estamina.")]
    [SerializeField] private Slider staminaSlider;

    [Tooltip("Duracion del efecto de fade in/out.")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Tooltip("Tiempo que la barra permanece visible despues de llenarse completamente.")]
    [SerializeField] private float displayTimeAfterFull = 1.0f;

    [Header("Billboard Settings")]
    [Tooltip("Si esta activado, la barra siempre mirara hacia la camara principal.")]
    [SerializeField] private bool enableBillboard = true;

    
    private Transform mainCameraTransform;
    private Coroutine fadeCoroutine;

    
    private bool isVisible = false;
    private float maxStamina = 100f;

    #region Unity Lifecycle

    void Awake()
    {
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Start()
    {
        
        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 0f;
            staminaCanvasGroup.interactable = false;
            staminaCanvasGroup.blocksRaycasts = false;
        }

        
        if (staminaSlider != null)
        {
            staminaSlider.value = maxStamina;
        }
    }

    void Update()
    {
        
        if (enableBillboard && staminaCanvasGroup != null && mainCameraTransform != null)
        {
            
            staminaCanvasGroup.transform.LookAt(
                staminaCanvasGroup.transform.position + mainCameraTransform.rotation * Vector3.forward,
                mainCameraTransform.rotation * Vector3.up
            );

            
            staminaCanvasGroup.transform.rotation *= Quaternion.Euler(0, 180f, 0);
        }
    }

    #endregion

    #region Public Methods - Control de Visibilidad

    
    
    
    public void ShowStaminaBar()
    {
        if (staminaCanvasGroup == null) return;

        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInRoutine());
        isVisible = true;
    }

    
    
    
    public void HideStaminaBar()
    {
        if (staminaCanvasGroup == null) return;

        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutRoutine());
        isVisible = false;
    }

    
    
    
    public void OnStaminaFullyRecharged()
    {
        if (staminaCanvasGroup == null) return;

        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(ShowBrieflyOnFullRoutine());
    }

    
    
    
    public void HideImmediate()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (staminaCanvasGroup != null)
        {
            staminaCanvasGroup.alpha = 0f;
        }

        isVisible = false;
    }

    #endregion

    #region Public Methods - Actualizacion de Valores

    
    
    
    
    
    public void UpdateStaminaValue(float currentStamina, float maxStamina)
    {
        this.maxStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    
    
    
    public void InitializeMaxStamina(float max)
    {
        maxStamina = max;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = max;
        }
    }

    #endregion

    #region Coroutines - Efectos de Fade

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        float startAlpha = staminaCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            staminaCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer / fadeDuration);
            yield return null;
        }

        staminaCanvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }

    private IEnumerator FadeOutRoutine()
    {
        float timer = 0f;
        float startAlpha = staminaCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            staminaCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }

        staminaCanvasGroup.alpha = 0f;
        fadeCoroutine = null;
    }

    private IEnumerator ShowBrieflyOnFullRoutine()
    {
        
        float timer = 0f;
        float startAlpha = staminaCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            staminaCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer / fadeDuration);
            yield return null;
        }
        staminaCanvasGroup.alpha = 1f;

        
        yield return new WaitForSeconds(displayTimeAfterFull);

        
        timer = 0f;
        startAlpha = staminaCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            staminaCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }

        staminaCanvasGroup.alpha = 0f;
        fadeCoroutine = null;
    }

    #endregion

    #region Getters

    
    
    
    public bool IsVisible => isVisible;

    #endregion
}