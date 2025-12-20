using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryHotbarUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInventory playerInventory;

    // CAMBIO 1: Ya no usamos hotbarSlots para poner el ítem, 
    // sino un array dedicado a los íconos hijos.
    [Tooltip("Arrastra aquí las Imágenes HIJAS (ItemIcon) de cada slot")]
    public Image[] slotIcons;

    [Header("Sprites de ítems")]
    public Sprite cardIcon;
    public Sprite leverIcon;
    public Sprite keyIcon;
    // public Sprite emptyIcon; // Ya no es necesario si ocultamos el ícono

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
            // Caso A: Hay un ítem en este slot
            if (i < allItems.Count)
            {
                string itemID = allItems[i];

                if (itemSprites.ContainsKey(itemID))
                {
                    slotIcons[i].sprite = itemSprites[itemID];

                    // IMPORTANTE: Aseguramos que el color sea visible (Alpha 1)
                    slotIcons[i].color = Color.white;

                    // IMPORTANTE: Activamos la imagen para que se vea
                    slotIcons[i].enabled = true;
                }
                else
                {
                    // Si hay ítem pero no tenemos su icono, ocultamos para evitar cuadro blanco
                    slotIcons[i].enabled = false;
                }
            }
            // Caso B: El slot está vacío
            else
            {
                slotIcons[i].sprite = null; // Quitamos la referencia (opcional)

                // ESTA ES LA CLAVE: Desactivamos el componente Image
                // Al hacer esto, Unity deja de renderizar el cuadrado blanco.
                slotIcons[i].enabled = false;
            }
        }
    }
}