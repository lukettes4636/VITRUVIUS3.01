using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class PickableItem : MonoBehaviour
{
    public enum ItemType { KeyCard, Item }

    [Header("Item Settings")]
    [SerializeField] private ItemType itemType = ItemType.Item;
    [SerializeField] private string itemID = "NuevoItem";
    [SerializeField] private string displayName = "";
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Player Restriction")]
    [SerializeField] private int requiredPlayerID = 0;

    [Header("Outline Settings")]
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;
    [SerializeField] private Color cooperativeOutlineColor = Color.yellow;

    [Header("UI Prompt Settings")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;

    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;
    private HashSet<GameObject> hoveringPlayers = new HashSet<GameObject>();
    private Vector3 startPosition;
    private bool isCollected = false;

    public string ItemID => itemID;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? itemID : displayName;

    void Start()
    {
        startPosition = transform.position;

        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            outlineColorID = Shader.PropertyToID(outlineColorProperty);
            outlineScaleID = Shader.PropertyToID(outlineScaleProperty);
            SetOutlineState(Color.black, 0f);
        }

        if (promptCanvas != null)
            promptCanvas.enabled = false;
    }

    void Update()
    {
        if (isCollected) return;
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void SetOutlineState(Color color, float scale)
    {
        if (meshRenderer == null || propertyBlock == null) return;
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(outlineColorID, color);
        propertyBlock.SetFloat(outlineScaleID, scale);
        meshRenderer.SetPropertyBlock(propertyBlock, 1);
    }

    private bool IsPlayerAllowed(int playerID)
    {
        if (requiredPlayerID == 0) return true;
        return requiredPlayerID == playerID;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponentInParent<PlayerIdentifier>();
        if (playerIdentifier == null) return;
        if (isCollected) return;
        if (!IsPlayerAllowed(playerIdentifier.playerID)) return;
        hoveringPlayers.Add(playerIdentifier.gameObject);
        if (requiredPlayerID == 0 && hoveringPlayers.Count >= 2)
            SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
        else
            SetOutlineState(playerIdentifier.PlayerOutlineColor, activeOutlineScale);
        if (promptCanvas != null && promptText != null)
        {
            promptCanvas.enabled = true;
            UpdatePromptVisualsInternal();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponentInParent<PlayerIdentifier>();
        if (playerIdentifier == null) return;
        hoveringPlayers.Remove(playerIdentifier.gameObject);
        if (hoveringPlayers.Count == 0)
        {
            SetOutlineState(originalOutlineColor, 0f);
            if (promptCanvas != null)
            {
                promptCanvas.enabled = false;
                if (promptButtonImage != null) promptButtonImage.gameObject.SetActive(false);
            }
        }
        else
        {
            if (requiredPlayerID == 0 && hoveringPlayers.Count >= 2)
            {
                SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
            }
            else
            {
                foreach (var obj in hoveringPlayers)
                {
                    var id = obj.GetComponent<PlayerIdentifier>();
                    if (id != null)
                    {
                        SetOutlineState(id.PlayerOutlineColor, activeOutlineScale);
                        break;
                    }
                }
            }
            if (promptCanvas != null)
            {
                UpdatePromptVisualsInternal();
            }
        }
    }

    public void Collect(GameObject collector)
    {
        if (isCollected) return;
        PlayerIdentifier collectorIdentifier = collector.GetComponent<PlayerIdentifier>();
        if (collectorIdentifier != null)
        {
            if (!IsPlayerAllowed(collectorIdentifier.playerID)) return;
        }

        PlayerInventory inventory = collector.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        bool added = false;
        if (itemType == ItemType.KeyCard)
            added = inventory.AddKeyCard(itemID);
        else
            added = inventory.AddItem(itemID);

        if (!added) return;
        isCollected = true;

        PlayerUIController uiController = collector.GetComponent<PlayerUIController>();
        if (uiController != null)
        {
            string message = $"I found the {DisplayName}!";
            uiController.ShowNotification(message);
        }

        DialogueManager.ShowItemCollectionDialogue(collector, itemID, itemType == ItemType.KeyCard);

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, transform.rotation);
        if (collectSound != null)
            AudioManager.Instance.PlaySFX(collectSound, transform.position, 0.7f, Random.Range(0.9f, 1.1f));

        SetOutlineState(originalOutlineColor, 0f);
        hoveringPlayers.Clear();
        if (promptCanvas != null)
        {
            promptCanvas.enabled = false;
            if (promptButtonImage != null) promptButtonImage.gameObject.SetActive(false);
        }
        Destroy(gameObject);
    }

    private void UpdatePromptVisualsInternal()
    {
        if (promptCanvas == null) return;
        Color c = PromptVisualHelper.ComputeColor(hoveringPlayers, cooperativeOutlineColor);
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
