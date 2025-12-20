using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("The unique ID that will be saved in the inventory (e.g.: PalancaParte, LlaveRoja).")]
    [SerializeField] private string itemID = "NuevoItem";
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

    
    [Header("Player Restriction")]
    [Tooltip("El ID del jugador que PUEDE activar el outline y reclamar este objeto. 0 = Ambos pueden reclamar. 1 = Solo P1. 2 = Solo P2.")]
    [SerializeField] private int requiredPlayerID = 0; 

    [Header("Outline Multiplayer")]
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;

    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;

    
    private GameObject currentHoveringPlayer = null;

    private Vector3 startPosition;
    private bool isCollected = false;

    public string ItemID => itemID;

    
    
    
    [Header("UI Prompt Settings")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;
    

    private void Start()
    {
        startPosition = transform.position;

        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            outlineColorID = Shader.PropertyToID(outlineColorProperty);
            outlineScaleID = Shader.PropertyToID(outlineScaleProperty);
            SetOutlineState(Color.black, 0.0f);
        }

        if (GetComponent<Collider>() == null || !GetComponent<Collider>().isTrigger)
        {

        }

        
        if (promptCanvas != null)
        {
            promptCanvas.enabled = false;
        }
    }

    private void Update()
    {
        if (isCollected) return;
        
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void SetOutlineState(Color color, float scale)
    {
        if (meshRenderer != null && propertyBlock != null)
        {
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(outlineColorID, color);
            propertyBlock.SetFloat(outlineScaleID, scale);
            meshRenderer.SetPropertyBlock(propertyBlock, 1);
        }
    }

    
    private bool IsPlayerAllowed(int playerID)
    {
        if (requiredPlayerID == 0) return true;
        return requiredPlayerID == playerID;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponentInParent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            
            if (!IsPlayerAllowed(playerIdentifier.playerID)) return;

            
            if (currentHoveringPlayer != null) return;

            
            currentHoveringPlayer = playerIdentifier.gameObject;

            
            SetOutlineState(playerIdentifier.PlayerOutlineColor, activeOutlineScale);

            
            if (promptCanvas != null && promptText != null)
            {
                promptCanvas.enabled = true;
                promptText.text = "PRESS TO COLLECT";
                UpdatePromptVisualsInternal();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponentInParent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            if (playerIdentifier.gameObject == currentHoveringPlayer)
            {
                
                SetOutlineState(originalOutlineColor, 0.0f);
                currentHoveringPlayer = null;

                
                if (promptCanvas != null)
                {
                    promptCanvas.enabled = false;
                    if (promptButtonImage != null) promptButtonImage.gameObject.SetActive(false);
                }
            }
        }
    }

    public void Collect(GameObject collector)
    {
        if (isCollected) return;

        if (collector != currentHoveringPlayer)
        {
            return;
        }

        PlayerInventory inventory = collector.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            if (inventory.AddItem(itemID))
            {
                isCollected = true;

                PlayerUIController uiController = collector.GetComponent<PlayerUIController>();
                if (uiController != null)
                {
                    string message = $"I found the {itemID}!";
                    uiController.ShowNotification(message);
                }

                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, transform.rotation);
                }
                if (collectSound != null)
                {
                    AudioManager.Instance.PlaySFX(collectSound, transform.position, 0.7f, Random.Range(0.9f, 1.1f));
                }

                
                SetOutlineState(originalOutlineColor, 0.0f);
                currentHoveringPlayer = null;

                Destroy(gameObject);
            }
        }
    }

    private void UpdatePromptVisualsInternal()
    {
        if (promptCanvas == null) return;
        Color c = Color.white;
        if (currentHoveringPlayer != null)
        {
            var id = currentHoveringPlayer.GetComponent<PlayerIdentifier>();
            if (id != null) c = id.PlayerOutlineColor;
        }
        PromptVisualHelper.ApplyToPrompt(promptCanvas.gameObject, c);
        if (promptButtonImage != null)
        {
            bool active = promptCanvas.enabled;
            promptButtonImage.gameObject.SetActive(active);
            if (active)
            {
                if (buttonAnchor != null)
                    promptButtonImage.rectTransform.position = buttonAnchor.position;
                else
                    promptButtonImage.rectTransform.anchoredPosition = buttonImageOffset;
            }
        }
    }
}
