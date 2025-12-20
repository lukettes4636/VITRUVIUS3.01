using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; 
using UnityEngine.UI;

public class MonitorPuzzleController : InteractiveObject
{
    public enum MonitorObjectKind { Computer, Note }
    [Header("Individual UI Configuration")]
    
    [SerializeField] private Canvas player1Canvas;
    [SerializeField] private Canvas player2Canvas;

    [Header("Dialogue Config")]
    [SerializeField] private MonitorObjectKind objectKind = MonitorObjectKind.Computer;
    [SerializeField] private string monitorID = "MonitorA";

    
    
    
    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactPromptCanvas; 
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;
    
    
    

    [Header("Audio Configuration")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float audioVolume = 0.8f;
    [SerializeField] private float audioPitch = 1f;

    [Header("Inputs de Activacion")]
    
    [SerializeField] private InputActionReference actionButtonPlayer1;
    [SerializeField] private InputActionReference actionButtonPlayer2;

    
    [Header("Outline Multiplayer")]
    [Tooltip("The color used when two or more players are in the trigger.")]
    [SerializeField] private Color cooperativeOutlineColor = Color.yellow;

    [Tooltip("The name of the 'Color' property in the Shader Graph.")]
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [Tooltip("The name of the 'Scale' property in the Shader Graph.")]
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;

    
    private List<PlayerIdentifier> activePlayers = new List<PlayerIdentifier>();
    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;
    

    
    private bool isPlayer1InRange = false;
    private bool isPlayer2InRange = false;

    private void Start()
    {
        
        if (player1Canvas != null)
        {
            player1Canvas.gameObject.SetActive(false);
            if (player1Canvas.gameObject.GetComponent<BillboardCanvas>() == null)
                player1Canvas.gameObject.AddComponent<BillboardCanvas>();
            if (player1Canvas.gameObject.GetComponent<CanvasScreenClamper>() == null)
                player1Canvas.gameObject.AddComponent<CanvasScreenClamper>();
        }
        if (player2Canvas != null)
        {
            player2Canvas.gameObject.SetActive(false);
            if (player2Canvas.gameObject.GetComponent<BillboardCanvas>() == null)
                player2Canvas.gameObject.AddComponent<BillboardCanvas>();
            if (player2Canvas.gameObject.GetComponent<CanvasScreenClamper>() == null)
                player2Canvas.gameObject.AddComponent<CanvasScreenClamper>();
        }

        
        
        
        if (interactPromptCanvas != null)
            interactPromptCanvas.gameObject.SetActive(false);
        
        
        


        
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

    protected override void OnEnable()
    {
        base.OnEnable();
        if (actionButtonPlayer1 != null)
            actionButtonPlayer1.action.performed += OnButtonPressed;
        if (actionButtonPlayer2 != null)
            actionButtonPlayer2.action.performed += OnButtonPressed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (actionButtonPlayer1 != null)
            actionButtonPlayer1.action.performed -= OnButtonPressed;
        if (actionButtonPlayer2 != null)
            actionButtonPlayer2.action.performed -= OnButtonPressed;
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        bool isPlayer1Action = context.action == actionButtonPlayer1.action;
        bool isPlayer2Action = context.action == actionButtonPlayer2.action;

        if (isPlayer1Action && isPlayer1InRange && player1Canvas != null)
        {
            ToggleCanvas(player1Canvas);
            
            if (player1Canvas.gameObject.activeSelf)
                ShowPrompt(false);
        }
        else if (isPlayer2Action && isPlayer2InRange && player2Canvas != null)
        {
            ToggleCanvas(player2Canvas);
            
            if (player2Canvas.gameObject.activeSelf)
                ShowPrompt(false);
        }
    }

    private void ToggleCanvas(Canvas canvas)
    {
        bool newState = !canvas.gameObject.activeSelf;
        canvas.gameObject.SetActive(newState);

        if (newState && openSound != null)
        {
            
            
             AudioManager.Instance.PlaySFX(openSound, transform.position, audioVolume, audioPitch);
        }
        else if (!newState && closeSound != null)
        {
             AudioManager.Instance.PlaySFX(closeSound, transform.position, audioVolume, audioPitch);
        }



        
        if (!newState)
        {
            
            bool isAnyCanvasActive = (player1Canvas != null && player1Canvas.gameObject.activeSelf) ||
                                    (player2Canvas != null && player2Canvas.gameObject.activeSelf);

            if (!isAnyCanvasActive)
                ShowPrompt(true);
        }
    }

    
    private void SetOutlineState(Color color, float scale)
    {
        if (meshRenderer != null && propertyBlock != null)
        {
            
            meshRenderer.GetPropertyBlock(propertyBlock, 1);

            propertyBlock.SetColor(outlineColorID, color);
            propertyBlock.SetFloat(outlineScaleID, scale);

            meshRenderer.SetPropertyBlock(propertyBlock, 1);
        }
    }

    private void UpdateOutlineVisuals()
    {
        if (activePlayers.Count == 0)
        {
            SetOutlineState(originalOutlineColor, 0.0f);
        }
        else if (activePlayers.Count == 1)
        {
            
            PlayerIdentifier singlePlayer = activePlayers[0];
            
            SetOutlineState(singlePlayer.PlayerOutlineColor, activeOutlineScale);
        }
        else
        {
            
            SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            
            if (playerIdentifier.playerID == 1)
            {
                isPlayer1InRange = true;
            }
            else if (playerIdentifier.playerID == 2)
            {
                isPlayer2InRange = true;
            }

            
            if (!activePlayers.Contains(playerIdentifier))
            {
                activePlayers.Add(playerIdentifier);
            }

            UpdateOutlineVisuals();

            
            
            
            
            if (!(player1Canvas != null && player1Canvas.gameObject.activeSelf) &&
                !(player2Canvas != null && player2Canvas.gameObject.activeSelf))
            {
                UpdatePromptVisuals();
                ShowPrompt(true);
            }

            DialogueManager.ShowMonitorEnterDialogue(monitorID, objectKind.ToString(), playerIdentifier.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier playerIdentifier = other.GetComponent<PlayerIdentifier>();

        if (playerIdentifier != null)
        {
            
            if (playerIdentifier.playerID == 1)
            {
                isPlayer1InRange = false;
                if (player1Canvas != null) player1Canvas.gameObject.SetActive(false);
            }
            else if (playerIdentifier.playerID == 2)
            {
                isPlayer2InRange = false;
                if (player2Canvas != null) player2Canvas.gameObject.SetActive(false);
            }

            
            if (activePlayers.Contains(playerIdentifier))
            {
                activePlayers.Remove(playerIdentifier);
            }

            UpdateOutlineVisuals();

            
            
            
            
            if (activePlayers.Count == 0)
            {
                ShowPrompt(false);
            }
            
            
            
        }
    }

    
    
    
    private void ShowPrompt(bool state)
    {
        if (interactPromptCanvas != null)
        {
            if (state)
            {
                UpdatePromptVisuals();
            }
            bool active = state && activePlayers.Count > 0;
            interactPromptCanvas.SetActive(active);
            if (promptButtonImage != null)
            {
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

    private void UpdatePromptVisuals()
    {
        if (interactPromptCanvas != null)
        {
            Color c = PromptVisualHelper.ComputeColor(activePlayers, cooperativeOutlineColor);
            PromptVisualHelper.ApplyToPrompt(interactPromptCanvas, c);
            if (promptButtonImage != null)
            {
                bool active = interactPromptCanvas.activeSelf;
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
    
    
    
}
