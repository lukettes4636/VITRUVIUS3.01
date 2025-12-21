using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryHotbarUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;

    [Tooltip("Child icons for each slot.")]
    public Image[] slotIcons;

    [Header("Item Sprites")]
    public Sprite cardIcon;
    public Sprite leverIcon;
    public Sprite keyIcon;

    private Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();

    private void Awake()
    {
        itemSprites["Card"] = cardIcon;
        itemSprites["Lever"] = leverIcon;
        itemSprites["Key"] = keyIcon;
    }

    private void Start()
    {
        RefreshHotbar();
    }

    public void RefreshHotbar()
    {
        if (playerInventory == null) return;
        if (slotIcons == null || slotIcons.Length == 0) return;

        var items = playerInventory.GetCollectedItems();
        var keyCards = playerInventory.GetCollectedKeyCards();

        List<string> allItems = new List<string>();
        allItems.AddRange(items);
        allItems.AddRange(keyCards);

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i < allItems.Count)
            {
                string itemID = allItems[i];

                if (itemSprites.ContainsKey(itemID))
                {
                    slotIcons[i].sprite = itemSprites[itemID];
                    slotIcons[i].color = Color.white;
                    slotIcons[i].enabled = true;
                }
                else
                {
                    slotIcons[i].enabled = false;
                }
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false;
            }
        }
    }
}