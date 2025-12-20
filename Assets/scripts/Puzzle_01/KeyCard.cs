using UnityEngine;
using UnityEngine.UI;

public class KeyCard : MonoBehaviour
{
    [Header("Key Card Settings")]
    [SerializeField] private string keyCardID = "Tarjeta";
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

   
    [Header("Player Restriction")]
    [Tooltip("El ID del jugador que PUEDE activar el outline y recolectar este objeto. 0 = Cualquier jugador.")]
    [SerializeField] private int requiredPlayerID = 0; 

    [Header("Outline Multiplayer")]
    [Tooltip("The name of the 'Color' property in the Shader Graph.")]
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [Tooltip("The name of the 'Scale' property in the Shader Graph.")]
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;

    
    
    [Tooltip("El color usado cuando dos o mas jugadores estan en el trigger.")]
    public Color cooperativeOutlineColor = Color.yellow;
   
   
    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactPromptCanvas; 
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;
    

    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;
    private GameObject currentHoveringPlayer = null;
   

    private Vector3 startPosition;
    private bool isCollected = false;

    public string KeyCardID => keyCardID;

    private void Start()
    {
        startPosition = transform.position;

        if (interactPromptCanvas != null)
            interactPromptCanvas.SetActive(false);
        if (promptButtonImage != null)
            promptButtonImage.gameObject.SetActive(false);
       


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

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || currentHoveringPlayer != null) return;

        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            if (requiredPlayerID != 0 && playerIdentifier.playerID != requiredPlayerID)
            {
                return;
            }

            currentHoveringPlayer = other.gameObject;
            SetOutlineState(playerIdentifier.PlayerOutlineColor, activeOutlineScale);

            ShowPrompt(true);
            UpdatePromptButtonVisuals();
         
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentHoveringPlayer)
        {
            SetOutlineState(originalOutlineColor, 0.0f);
            currentHoveringPlayer = null;

            
            ShowPrompt(false);
            UpdatePromptButtonVisuals();
          
        }
    }

    public void Collect(GameObject collector)
    {
        if (isCollected) return;

        PlayerIdentifier collectorIdentifier = collector.GetComponent<PlayerIdentifier>();
        if (collectorIdentifier != null)
        {
            if (requiredPlayerID != 0 && collectorIdentifier.playerID != requiredPlayerID)
            {
                return;
            }
        }

        PlayerInventory inventory = collector.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            if (inventory.AddKeyCard(keyCardID))
            {
                isCollected = true;

                PlayerUIController uiController = collector.GetComponent<PlayerUIController>();
                if (uiController != null)
                {
                    string message = $"I found the {keyCardID}!";
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

              
                ShowPrompt(false);
             

                SetOutlineState(originalOutlineColor, 0.0f);

                Destroy(gameObject);
            }
        }
    }

    
    private void ShowPrompt(bool state)
    {
       
        if (interactPromptCanvas != null)
            interactPromptCanvas.SetActive(state && !isCollected);
        UpdatePromptButtonVisuals();
    }

    private void UpdatePromptButtonVisuals()
    {
        if (promptButtonImage == null) return;
        bool active = interactPromptCanvas != null && interactPromptCanvas.activeSelf && !isCollected;
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
