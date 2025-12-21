using UnityEngine;
using UnityEngine.VFX;

public class ButtonIndicatorController : MonoBehaviour
{
    [Header("Animator Settings")]
    private Animator animator;
    private static readonly int PressHash = Animator.StringToHash("Press");

    [Header("VFX - SOLO ONDA DE IMPACTO")]
    [SerializeField] private VisualEffect shockwaveVFX;

    [Header("VFX Scale")]
    [Tooltip("Tamao de la onda de impacto")]
    [SerializeField] private float shockwaveBaseScale = 1f;
    [SerializeField] private float shockwaveFinalScale = 5f;

    [Header("Color Settings")]
    private SpriteRenderer spriteRenderer;
    private UnityEngine.UI.Image imageComponent;

    [Header("Pulse Effect")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;

    private Color currentPlayerColor = Color.white;
    private Vector3 originalScale;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            imageComponent = GetComponentInChildren<UnityEngine.UI.Image>();
        }

        originalScale = transform.localScale;
    }

    void Start()
    {
        
        if (shockwaveVFX != null)
        {
            shockwaveVFX.Stop();
        }
    }

    void Update()
    {
        if (enablePulse)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }

    
    
    
    public void TriggerPress()
    {
        if (animator != null)
        {
            animator.SetTrigger(PressHash);
        }
    }

    
    
    
    public void TriggerShockwave(bool isFinalHit = false)
    {
        if (shockwaveVFX == null)
        {
            return;
        }

        
        shockwaveVFX.Stop();

        
        float targetScale = isFinalHit ? shockwaveFinalScale * 1.5f : shockwaveFinalScale;
        shockwaveVFX.transform.localScale = Vector3.one * targetScale;

        
        if (shockwaveVFX.HasVector4("PlayerColor"))
        {
            shockwaveVFX.SetVector4("PlayerColor", currentPlayerColor);
        }

        
        shockwaveVFX.Reinit();
        shockwaveVFX.Play();
    }

    
    
    
    public void SetPlayerColor(Color playerColor)
    {
        currentPlayerColor = playerColor;

        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = playerColor;
        }
        else if (imageComponent != null)
        {
            imageComponent.color = playerColor;
        }

        
        if (shockwaveVFX != null && shockwaveVFX.HasVector4("PlayerColor"))
        {
            shockwaveVFX.SetVector4("PlayerColor", playerColor);
        }
    }

    
    
    
    public void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            spriteRenderer.color = c;
        }
        else if (imageComponent != null)
        {
            Color c = imageComponent.color;
            c.a = Mathf.Clamp01(alpha);
            imageComponent.color = c;
        }
    }

    void OnDisable()
    {
        
        if (shockwaveVFX != null)
        {
            shockwaveVFX.Stop();
        }
    }
}
