using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum PromptMode
{
    Smooth,
    Fast,
    Instant,
    Bounce,
    Elastic
}

public class SimpleTriggerPromptPro : MonoBehaviour
{
    [Header("TextMeshPro Settings")]
    [Tooltip("Referencia al componente TextMeshPro")]
    public TMP_Text promptText;
    public CanvasGroup promptGroup;
    
    [Tooltip("Texto que se mostrara")]
    public string displayText = "Presiona E para interactuar";

    [Header("Image Settings")]
    [Tooltip("Imagen de fondo del prompt")]
    public Image backgroundImage;
    [Tooltip("Color normal de la imagen")]
    public Color normalImageColor = Color.white;
    [Tooltip("Color cuando el jugador esta cerca")]
    public Color hoverImageColor = Color.yellow;
    [Tooltip("Opacidad de la imagen")]
    [Range(0f, 1f)]
    public float imageOpacity = 0.8f;
    [Tooltip("Animar el color de la imagen")]
    public bool animateImageColor = true;
    [Tooltip("Velocidad de animacion del color")]
    [Range(1f, 10f)]
    public float imageColorSpeed = 5f;
    
    [Header("Trigger Settings")]
    [Tooltip("Tags de jugadores para detectar (ej: Player1, Player2)")]
    public string[] playerTags = new string[] { "Player1", "Player2" };
    [Tooltip("Origen de deteccion (dejar vacio para usar este objeto)")]
    public Transform detectionOrigin;
    
    [Tooltip("Distancia donde el prompt esta completamente visible")]
    [Range(0.5f, 20f)]
    public float fullVisibilityDistance = 5f;
    
    [Tooltip("Distancia donde el prompt comienza a aparecer")]
    [Range(1f, 30f)]
    public float fadeStartDistance = 12f;
    
    [Header("Fade Settings")]
    [Tooltip("Velocidad de fade in/out")]
    [Range(0.1f, 5f)]
    public float fadeSpeed = 2f;
    
    [Tooltip("Suavizado de la transicion")]
    [Range(0.1f, 3f)]
    public float smoothness = 1f;
    
    [Tooltip("Opacidad maxima del texto")]
    [Range(0f, 1f)]
    public float maxOpacity = 1f;
    
    [Header("Distance-based Opacity")]
    [Tooltip("Habilitar opacidad basada en distancia")]
    public bool useDistanceOpacity = true;
    
    [Tooltip("Factor de atenuacion por distancia")]
    [Range(0.5f, 3f)]
    public float distanceAttenuation = 1.5f;
    
    [Header("Animation Settings")]
    [Tooltip("Modo de animacion del prompt")]
    public PromptMode animationMode = PromptMode.Smooth;
    [Tooltip("Habilitar animacion de escala")]
    public bool useScaleAnimation = false;
    [Tooltip("Ocultar objeto cuando esta invisible")]
    public bool hideObjectWhenInvisible = true;
    
    [Tooltip("Factor de escala cuando esta oculto")]
    [Range(0.1f, 1f)]
    public float hiddenScale = 0.8f;
    
    [Tooltip("Velocidad de animacion de escala")]
    [Range(1f, 10f)]
    public float scaleSpeed = 5f;
    
    [Header("Advanced Settings")]
    [Tooltip("Capa de deteccion del jugador")]
    public LayerMask playerLayerMask = -1;
    
    [Tooltip("Usar trigger en lugar de distancia")]
    public bool useTrigger = false;
    
    [Tooltip("Tiempo de actualizacion de busqueda de jugadores (segundos)")]
    [Range(0.1f, 2f)]
    public float playerSearchInterval = 0.5f;

    private Transform[] playerTransforms;
    private float currentOpacity = 0f;
    private float targetOpacity = 0f;
    private float opacityVelocity = 0f; 
    private Vector3 originalScale;
    private Vector3 currentScale;
    private bool isPlayerNear = false;
    private Color currentImageColor;
    private Color targetImageColor;
    private float lastPlayerSearchTime;
    private int playersInTrigger = 0;
    private float hysteresisOffset = 0.2f; // Slight buffer to prevent distance flickering

    void Start()
    {
        InitializeComponents();
        FindAllPlayers();
        CacheOriginalValues();
        lastPlayerSearchTime = Time.time;
    }

    void Update()
    {
        
        if (Time.time - lastPlayerSearchTime > playerSearchInterval)
        {
            FindAllPlayers();
            lastPlayerSearchTime = Time.time;
        }

        if (!useTrigger)
        {
            UpdatePlayerDistance();
        }
        
        UpdateOpacity();
        UpdateScale();
        UpdateImageColor();
        ApplyVisualChanges();
    }

    void InitializeComponents()
    {
        
        if (promptText != null)
        {
            promptText.text = displayText;
            promptText.alpha = 0f; 
            currentOpacity = 0f; 
            targetOpacity = 0f; 
        }

        if (promptGroup != null)
        {
            promptGroup.alpha = 0f;
        }

        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = GetComponentInChildren<Image>();
            }
        }

        if (backgroundImage != null)
        {
            currentImageColor = normalImageColor;
            targetImageColor = normalImageColor;
            Color imgColor = normalImageColor;
            imgColor.a = 0f;
            backgroundImage.color = imgColor;
        }
    }

    void FindAllPlayers()
    {
        System.Collections.Generic.List<Transform> allPlayers = new System.Collections.Generic.List<Transform>();
        
        foreach (string tag in playerTags)
        {
            // Using FindGameObjectsWithTag is fine here since it only runs every 'playerSearchInterval'
            GameObject[] playersWithTag = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject playerObj in playersWithTag)
            {
                if (playerObj != null && playerObj.transform != null)
                {
                    allPlayers.Add(playerObj.transform);
                }
            }
        }
        
        // If no players found by tag, try to find them by name as a fallback for Player1/Player2
        if (allPlayers.Count == 0)
        {
            GameObject p1 = GameObject.Find("Player1");
            if (p1 != null) allPlayers.Add(p1.transform);
            GameObject p2 = GameObject.Find("Player2");
            if (p2 != null) allPlayers.Add(p2.transform);
        }

        playerTransforms = allPlayers.Count > 0 ? allPlayers.ToArray() : null;
    }

    void UpdatePlayerDistance()
    {
        if (playerTransforms == null || playerTransforms.Length == 0)
        {
            isPlayerNear = false;
            targetOpacity = 0f;
            return;
        }

        float nearestDistance = float.MaxValue;
        Vector3 origin = GetOriginPosition();
        
        
        foreach (Transform player in playerTransforms)
        {
            if (player == null) continue;
            
            float distance = Vector3.Distance(origin, player.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
            }
        }

        UpdateTargetOpacity(nearestDistance);
    }

    Vector3 GetOriginPosition()
    {
        return detectionOrigin != null ? detectionOrigin.position : transform.position;
    }

    void UpdateTargetOpacity(float distance)
    {
        
        if (distance <= fullVisibilityDistance)
        {
            isPlayerNear = true;
            targetOpacity = maxOpacity;
        }
        
        else if (distance >= fadeStartDistance)
        {
            isPlayerNear = false;
            targetOpacity = 0f;
        }
        
        else
        {
            isPlayerNear = true;
            
            if (useDistanceOpacity)
            {
                float distanceRange = fadeStartDistance - fullVisibilityDistance;
                float distanceFactor = 1f - ((distance - fullVisibilityDistance) / distanceRange);
                
                // Smoother curve for distance attenuation
                float attenuatedOpacity = maxOpacity * Mathf.SmoothStep(0f, 1f, distanceFactor);
                targetOpacity = Mathf.Clamp(attenuatedOpacity, 0f, maxOpacity);
            }
            else
            {
                float distanceRange = fadeStartDistance - fullVisibilityDistance;
                float distanceFactor = 1f - ((distance - fullVisibilityDistance) / distanceRange);
                // Use SmoothStep for a much smoother transition
                targetOpacity = Mathf.SmoothStep(0f, maxOpacity, distanceFactor);
            }
        }
    }

    void UpdateOpacity()
    {
        float smoothTime;
        
        switch (animationMode)
        {
            case PromptMode.Instant:
                currentOpacity = targetOpacity;
                opacityVelocity = 0f;
                return;
                
            case PromptMode.Fast:
                smoothTime = 0.1f;
                break;
                
            case PromptMode.Bounce:
                smoothTime = 0.3f;
                break;
                
            case PromptMode.Elastic:
                smoothTime = 0.5f;
                break;
                
            case PromptMode.Smooth:
            default:
                smoothTime = 1f / Mathf.Max(0.1f, fadeSpeed);
                smoothTime *= smoothness;
                break;
        }

        currentOpacity = Mathf.SmoothDamp(currentOpacity, targetOpacity, ref opacityVelocity, smoothTime);

        // Clamp currentOpacity to targetOpacity if they are very close to avoid micro-updates
        if (Mathf.Abs(currentOpacity - targetOpacity) < 0.001f)
        {
            currentOpacity = targetOpacity;
            opacityVelocity = 0f;
        }
        
        currentOpacity = Mathf.Clamp(currentOpacity, 0f, maxOpacity); 
    }

    void UpdateScale()
    {
        if (!useScaleAnimation) return;

        Vector3 targetScale = isPlayerNear ? originalScale : originalScale * hiddenScale;
        currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    void UpdateImageColor()
    {
        if (backgroundImage == null || !animateImageColor) return;

        targetImageColor = isPlayerNear ? hoverImageColor : normalImageColor;
        currentImageColor = Color.Lerp(currentImageColor, targetImageColor, Time.deltaTime * imageColorSpeed);
        
        Color imgColor = currentImageColor;
        imgColor.a = imageOpacity * currentOpacity;
        backgroundImage.color = imgColor;
    }

    void ApplyVisualChanges()
    {
        bool shouldBeActive = currentOpacity > 0.01f;
        
        if (promptGroup != null)
        {
            promptGroup.alpha = currentOpacity;
            
            // Optimization: Disable raycasts and interaction when mostly invisible
            promptGroup.blocksRaycasts = currentOpacity > 0.1f;
            promptGroup.interactable = currentOpacity > 0.1f;
            
            if (hideObjectWhenInvisible && promptGroup.gameObject.activeSelf != shouldBeActive)
            {
                promptGroup.gameObject.SetActive(shouldBeActive);
            }
            
            if (useScaleAnimation && promptGroup.transform != null)
            {
                promptGroup.transform.localScale = currentScale;
            }
        }
        else if (promptText != null)
        {
            promptText.alpha = currentOpacity;
            
            if (hideObjectWhenInvisible && promptText.gameObject.activeSelf != shouldBeActive)
            {
                promptText.gameObject.SetActive(shouldBeActive);
            }
            
            if (useScaleAnimation)
            {
                promptText.transform.localScale = currentScale;
            }
        }
    }

    void CacheOriginalValues()
    {
        if (promptText != null)
        {
            originalScale = promptText.transform.localScale;
            currentScale = originalScale;
        }
        else if (promptGroup != null)
        {
            originalScale = promptGroup.transform.localScale;
            currentScale = originalScale;
        }
    }

    bool IsValidPlayerTag(string tag)
    {
        foreach (string validTag in playerTags)
        {
            if (tag == validTag)
                return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTrigger || !IsValidPlayerTag(other.tag)) return;
        
        playersInTrigger++;
        isPlayerNear = true;
        targetOpacity = maxOpacity;
    }

    void OnTriggerExit(Collider other)
    {
        if (!useTrigger || !IsValidPlayerTag(other.tag)) return;
        
        playersInTrigger--;
        
        
        if (playersInTrigger <= 0)
        {
            playersInTrigger = 0;
            isPlayerNear = false;
            targetOpacity = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = GetOriginPosition();
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, fullVisibilityDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, fadeStartDistance);
        
        if (Application.isPlaying && playerTransforms != null)
        {
            foreach (Transform player in playerTransforms)
            {
                if (player != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(origin, player.position);
                }
            }
        }
    }

    
    public void SetText(string newText)
    {
        displayText = newText;
        if (promptText != null)
        {
            promptText.text = displayText;
        }
    }

    public void ForceShow()
    {
        targetOpacity = maxOpacity;
        isPlayerNear = true;
    }

    public void ForceHide()
    {
        targetOpacity = 0f;
        isPlayerNear = false;
    }

    public void ResetSettings()
    {
        currentOpacity = 0f;
        targetOpacity = 0f;
        isPlayerNear = false;
        playersInTrigger = 0;
        CacheOriginalValues();
    }

    public void SetAnimationMode(PromptMode mode)
    {
        animationMode = mode;
        switch (mode)
        {
            case PromptMode.Smooth:
                fadeSpeed = 2f;
                smoothness = 1f;
                break;
            case PromptMode.Fast:
                fadeSpeed = 5f;
                smoothness = 0.5f;
                break;
            case PromptMode.Instant:
                break;
            case PromptMode.Bounce:
                fadeSpeed = 3f;
                smoothness = 1.5f;
                break;
            case PromptMode.Elastic:
                fadeSpeed = 1f;
                smoothness = 2f;
                break;
        }
    }
}