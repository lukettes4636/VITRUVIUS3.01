using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Key Card System")]
    [SerializeField] private List<string> collectedKeyCards = new List<string>();

    [Header("Item Tracking System")]
    [SerializeField] private List<string> collectedItems = new List<string>();

    [Header("Essential Items Configuration")]
    [Tooltip("Lista de objetos o tarjetas que no se pierden al morir.")]
    [SerializeField] private List<string> essentialItems = new List<string> { "Card", "Lever", "Key" };

    [Header("UI References")]
    [SerializeField] private Text keyCardStatusText;
    [SerializeField] private Text itemsCollectedText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip keyCardCollectSound;
    [SerializeField] private AudioClip itemCollectSound;

    private void Start()
    {
        UpdateUI();
    }

    
    
    

    public bool AddKeyCard(string keyCardID)
    {
        if (HasKeyCard(keyCardID))
        {

            return false;
        }

        collectedKeyCards.Add(keyCardID);
        if (keyCardCollectSound != null && audioSource != null)
            audioSource.PlayOneShot(keyCardCollectSound);


        UpdateUI();
        return true;
    }

    public bool AddItem(string itemID)
    {
        if (collectedItems.Contains(itemID))
        {

            return false;
        }

        collectedItems.Add(itemID);

        if (itemCollectSound != null && audioSource != null)
            audioSource.PlayOneShot(itemCollectSound);


        UpdateUI();

        return true;
    }

    
    
    

    public bool HasKeyCard(string keyCardID) => collectedKeyCards.Contains(keyCardID);
    public bool HasItem(string itemID) => collectedItems.Contains(itemID);

    public bool UseItem(string itemID)
    {
        if (HasItem(itemID))
        {
            collectedItems.Remove(itemID);
            UpdateUI();

            return true;
        }
        return false;
    }

    public bool UseKeyCard(string keyCardID)
    {
        if (HasKeyCard(keyCardID))
        {
            collectedKeyCards.Remove(keyCardID);
            UpdateUI();
            return true;
        }
        return false;
    }

    
    
    

    public List<string> GetCollectedKeyCards() => new List<string>(collectedKeyCards);
    public List<string> GetCollectedItems() => new List<string>(collectedItems);

    public void RestoreInventory(List<string> keyCards, List<string> items)
    {
        collectedKeyCards.Clear();
        collectedItems.Clear();
        collectedKeyCards.AddRange(keyCards);
        collectedItems.AddRange(items);
        UpdateUI();

    }

    
    
    
    public void RemoveNonEssentialItems()
    {
        List<string> itemsToKeep = new List<string>();
        List<string> keyCardsToKeep = new List<string>();

        foreach (string item in collectedItems)
        {
            if (essentialItems.Contains(item))
                itemsToKeep.Add(item);
        }

        foreach (string keyCard in collectedKeyCards)
        {
            if (essentialItems.Contains(keyCard))
                keyCardsToKeep.Add(keyCard);
        }

        collectedItems = itemsToKeep;
        collectedKeyCards = keyCardsToKeep;

        UpdateUI();

    }

    
    
    

    public int GetTotalItemsCollected()
    {
        return collectedItems.Count + collectedKeyCards.Count;
    }

    private void UpdateUI()
    {
        if (keyCardStatusText != null)
            keyCardStatusText.text = collectedKeyCards.Count > 0 ? $"Tarjetas: {collectedKeyCards.Count}" : "Sin tarjetas";

        if (itemsCollectedText != null)
            itemsCollectedText.text = $"Items: {GetTotalItemsCollected()}";

        
        
        
        var hotbarUI = GetComponentInChildren<InventoryHotbarUI>();
        if (hotbarUI != null)
            hotbarUI.RefreshHotbar();
    }
}
