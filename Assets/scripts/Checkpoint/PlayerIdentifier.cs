using UnityEngine;

public class PlayerIdentifier : MonoBehaviour
{
    
    [Tooltip("Asigna 1 para el Jugador 1, 2 para el Jugador 2.")]
    public int playerID = 1;

    [Header("Outline Color")]
    [Tooltip("The color to be used for the Outline effect of interactable objects.")]
    [SerializeField] private Color playerOutlineColor = Color.blue; 

    
    public Color PlayerOutlineColor => playerOutlineColor; 

    
    public PlayerHealth playerHealth;
    public PlayerInventory playerInventory;

    
    

    private void Awake()
    {
        
        playerHealth = GetComponent<PlayerHealth>();
        playerInventory = GetComponent<PlayerInventory>();
    }
}