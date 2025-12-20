using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeypadDoorController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject keypadCanvas;
    [SerializeField] private GameObject interactPromptCanvas;
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;
    private KeypadUIManager uiManager;

    [Header("Door Settings")]
    [SerializeField] private GameObject doorRoot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Feedback Light")]
    [SerializeField] private Light feedbackLight;
    [SerializeField] private Color idleColor = Color.red;
    [SerializeField] private Color correctColor = Color.green;

    [Header("Outline Multiplayer")]
    [Tooltip("Color usado cuando dos o ms jugadores estn en el trigger.")]
    [SerializeField] private Color cooperativeOutlineColor = Color.yellow;
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;

    private List<PlayerIdentifier> activePlayers = new List<PlayerIdentifier>();
    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;

    private bool isActive = false;
    private bool isOpen = false;

    private MovJugador1 currentP1;
    private MovJugador2 currentP2;
    [Header("Dialogue Config")]
    [SerializeField] private string doorID = "KeypadDoorA";

    private MovJugador1 nearbyP1;
    private MovJugador2 nearbyP2;

    private Collider doorBlockingCollider;

    void Start()
    {
        if (keypadCanvas != null)
        {
            uiManager = keypadCanvas.GetComponent<KeypadUIManager>();

            keypadCanvas.SetActive(false);
            if (keypadCanvas.GetComponent<BillboardCanvas>() == null)
                keypadCanvas.AddComponent<BillboardCanvas>();
            if (keypadCanvas.GetComponent<CanvasScreenClamper>() == null)
                keypadCanvas.AddComponent<CanvasScreenClamper>();
        }

        if (interactPromptCanvas != null)
        {
            interactPromptCanvas.SetActive(false);
            if (interactPromptCanvas.GetComponent<BillboardCanvas>() == null)
                interactPromptCanvas.AddComponent<BillboardCanvas>();
            if (interactPromptCanvas.GetComponent<CanvasScreenClamper>() == null)
                interactPromptCanvas.AddComponent<CanvasScreenClamper>();
        }

        if (feedbackLight != null)
            feedbackLight.color = idleColor;

        if (doorRoot != null)
            doorBlockingCollider = doorRoot.GetComponent<Collider>();

        meshRenderer = transform.parent?.GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            outlineColorID = Shader.PropertyToID(outlineColorProperty);
            outlineScaleID = Shader.PropertyToID(outlineScaleProperty);
            SetOutlineState(Color.black, 0.0f);
        }
    }


    private void SetOutlineState(Color color, float scale)
    {
        if (meshRenderer == null) return;
        meshRenderer.GetPropertyBlock(propertyBlock, 1);
        propertyBlock.SetColor(outlineColorID, color);
        propertyBlock.SetFloat(outlineScaleID, scale);
        meshRenderer.SetPropertyBlock(propertyBlock, 1);
    }

    private void UpdateOutlineVisuals()
    {
        if (activePlayers.Count == 0)
            SetOutlineState(originalOutlineColor, 0.0f);
        else if (activePlayers.Count == 1)
            SetOutlineState(activePlayers[0].PlayerOutlineColor, activeOutlineScale);
        else
            SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
        UpdatePromptVisuals();
    }


    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentifier id = other.GetComponent<PlayerIdentifier>();
        if (id == null) return;

        if (!activePlayers.Contains(id))
            activePlayers.Add(id);

        UpdateOutlineVisuals();
        ShowPrompt(true);

        DialogueManager.ShowKeypadEnterDialogue(doorID, other.gameObject);

        if (id.playerID == 1)
        {
            MovJugador1 p1 = other.GetComponent<MovJugador1>();
            if (p1 != null)
            {
                nearbyP1 = p1;
                PlayerInput input = p1.GetComponent<PlayerInput>();
                if (input != null)
                    input.actions["Action Button"].performed += OnInteractP1;
            }
        }
        else if (id.playerID == 2)
        {
            MovJugador2 p2 = other.GetComponent<MovJugador2>();
            if (p2 != null)
            {
                nearbyP2 = p2;
                PlayerInput input = p2.GetComponent<PlayerInput>();
                if (input != null)
                    input.actions["Action Button"].performed += OnInteractP2;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier id = other.GetComponent<PlayerIdentifier>();
        if (id == null) return;

        activePlayers.Remove(id);
        UpdateOutlineVisuals();

        if (activePlayers.Count == 0)
            ShowPrompt(false);

        if (id.playerID == 1 && nearbyP1 != null)
        {
            PlayerInput input = nearbyP1.GetComponent<PlayerInput>();
            if (input != null)
                input.actions["Action Button"].performed -= OnInteractP1;
            nearbyP1 = null;
        }
        else if (id.playerID == 2 && nearbyP2 != null)
        {
            PlayerInput input = nearbyP2.GetComponent<PlayerInput>();
            if (input != null)
                input.actions["Action Button"].performed -= OnInteractP2;
            nearbyP2 = null;
        }
    }


    private void OnInteractP1(InputAction.CallbackContext ctx)
    {
        if (!isActive && !isOpen && nearbyP1 != null)
            ActivateForPlayer(nearbyP1);
    }

    private void OnInteractP2(InputAction.CallbackContext ctx)
    {
        if (!isActive && !isOpen && nearbyP2 != null)
            ActivateForPlayer(nearbyP2);
    }

    private void ActivateForPlayer(MovJugador1 player)
    {
        if (player == null) return;

        
        isActive = true;
        currentP1 = player;

        
        ShowPrompt(false);

        
        keypadCanvas.SetActive(true);
        PositionCanvasOnScreen(keypadCanvas);

        
        uiManager.Open(this, player.GetComponent<PlayerInput>());

        
        player.EnterLockMode(uiManager);
    }

    private void ActivateForPlayer(MovJugador2 player)
    {
        if (player == null) return;

        
        isActive = true;
        currentP2 = player;

        
        ShowPrompt(false);

        
        keypadCanvas.SetActive(true);
        PositionCanvasOnScreen(keypadCanvas);

        
        uiManager.Open(this, player.GetComponent<PlayerInput>());

        
        player.EnterLockMode(uiManager);
    }


    public void ReleasePlayer()
    {
        if (currentP1 != null)
        {
            currentP1.ExitLockMode();
            currentP1 = null;
        }
        if (currentP2 != null)
        {
            currentP2.ExitLockMode();
            currentP2 = null;
        }

        isActive = false;
        keypadCanvas.SetActive(false);

        
        if (!isOpen && activePlayers.Count > 0)
            ShowPrompt(true);
    }


    public void OpenDoor()
    {
        if (isOpen) return;
        StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        isOpen = true;

        if (doorBlockingCollider != null)
            doorBlockingCollider.enabled = false;

        if (feedbackLight != null)
            feedbackLight.color = correctColor;

        float rotated = 0f;
        while (rotated < openAngle)
        {
            float delta = rotationSpeed * Time.deltaTime;
            doorRoot.transform.Rotate(Vector3.up, delta);
            rotated += delta;
            yield return null;
        }

        ReleasePlayer();
        ShowPrompt(false);
    }


    private void ShowPrompt(bool state)
    {
        if (interactPromptCanvas != null)
        {
            bool active = state && !isActive && !isOpen;
            interactPromptCanvas.SetActive(active);
            if (active) UpdatePromptVisuals();
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
        if (interactPromptCanvas == null) return;
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

    private void PositionCanvasOnScreen(GameObject canvas)
    {
        if (canvas == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        Transform t = canvas.transform;
        Vector3 vp = cam.WorldToViewportPoint(t.position);
        if (vp.z < 0f) vp.z = Mathf.Abs(vp.z);
        float m = 0.05f;
        vp.x = Mathf.Clamp(vp.x, m, 1f - m);
        vp.y = Mathf.Clamp(vp.y, m, 1f - m);
        Vector3 world = cam.ViewportToWorldPoint(vp);
        t.position = world;
    }
}
