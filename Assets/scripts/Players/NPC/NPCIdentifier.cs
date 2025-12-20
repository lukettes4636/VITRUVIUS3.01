using UnityEngine;





[RequireComponent(typeof(NPCHealth))]
public class NPCIdentifier : MonoBehaviour
{
    [Header("NPC Identification")]
    [Tooltip("ID unico del NPC. Usalo para diferenciar multiples NPCs si es necesario.")]
    public int npcID = 1;

    [Header("Outline Color")]
    [Tooltip("El color usado para el efecto Outline de objetos interactuables cuando este NPC esta cerca.")]
    [SerializeField] private Color npcOutlineColor = Color.green;

    
    
    
    public Color NPCOutlineColor => npcOutlineColor;

    [Header("Component References")]
    [SerializeField] private NPCHealth npcHealth;
    [SerializeField] private NPCNoiseEmitter npcNoiseEmitter;

    
    
    
    public NPCHealth NPCHealth => npcHealth;

    
    
    
    public NPCNoiseEmitter NPCNoiseEmitter => npcNoiseEmitter;

    private void Awake()
    {
        
        if (npcHealth == null)
            npcHealth = GetComponent<NPCHealth>();

        if (npcNoiseEmitter == null)
            npcNoiseEmitter = GetComponent<NPCNoiseEmitter>();
    }
}

