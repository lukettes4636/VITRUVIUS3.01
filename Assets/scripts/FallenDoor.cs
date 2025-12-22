using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Collections.Generic;

public class FallenDoor : InteractiveObject
{
    [Header("Door Identification")]
    [SerializeField] private string doorID = "";
    [Header("Door Settings")]
    [SerializeField] private float maxLiftHeight = 2f;
    [SerializeField] private float liftSpeed = 1f;
    [SerializeField] private float dropSpeed = 4f;

    [Header("Fail Settings (Player 2)")]
    [Tooltip("Altura mxima a la que sube cuando falla el P2")]
    [SerializeField] private float failLiftHeight = 0.35f;

    [Tooltip("Intensidad de la vibracin visual (Shake)")]
    [SerializeField] private float shakeAmount = 0.03f;

    [Header("Crush Damage Settings")]
    [SerializeField] private int crushDamage = 50;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float crushHeightThreshold = 0.6f;

    [Header("Audio (Mechanical)")]
    [Tooltip("Sonido continuo de la puerta subiendo (Loop)")]
    [SerializeField] private AudioClip liftSound;
    [Tooltip("Sonido de golpe seco al caer")]
    [SerializeField] private AudioClip dropSound;
    [Tooltip("Sonido de tope metalico al llegar arriba")]
    [SerializeField] private AudioClip maxHeightSound;

    [Header("Voice Audio (Effort)")]
    [Tooltip("Sonido de esfuerzo (Loop/Grito) para PLAYER 1 (Hombre)")]
    [SerializeField] private AudioClip effortVoiceP1;
    [Tooltip("Sonido de esfuerzo (Loop/Grito) para PLAYER 2 (Mujer)")]
    [SerializeField] private AudioClip effortVoiceP2;

    private AudioSource audioSource;      
    private AudioSource voiceAudioSource; 

    [Header("Haptic Feedback - Drop IMPACT")]
    [SerializeField] private float dropRumbleDuration = 0.3f;
    [SerializeField] private float dropLowFrequency = 1.0f;
    [SerializeField] private float dropHighFrequency = 1.0f;

    private MovJugador1 playerLifter = null;
    private MovJugador2 playerFailler = null;

    [Header("Outline Multiplayer")]
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;
    [SerializeField] private Color mixedColor = new Color(1f, 0f, 1f, 1f);

    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;

    private List<PlayerIdentifier> playersInTrigger = new List<PlayerIdentifier>();

    private Vector3 initialPosition;
    private bool isBeingLifted = false;
    private bool isFailLifting = false;
    private bool isAtMaxHeight = false;
    private bool hasDropped = true;
    private bool isDropping = false;
    private bool failDialogueShown = false;

    [Header("UI Prompt Settings")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;

    private void Start()
    {
        initialPosition = transform.position;

        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            outlineColorID = Shader.PropertyToID(outlineColorProperty);
            outlineScaleID = Shader.PropertyToID(outlineScaleProperty);
            SetOutlineState(Color.black, 0.0f);
        }

        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        
        voiceAudioSource = gameObject.AddComponent<AudioSource>();
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.spatialBlend = 1f;
        voiceAudioSource.rolloffMode = AudioRolloffMode.Linear; 

        if (promptCanvas != null) promptCanvas.enabled = false;
    }

    private void Update()
    {
        HandleDoorMovement();
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
        PlayerIdentifier pid = other.GetComponent<PlayerIdentifier>();

        if (pid != null && !playersInTrigger.Contains(pid))
        {
            playersInTrigger.Add(pid);
            UpdateOutlineColor();
            UpdatePrompt();
            DialogueManager.ShowFallenDoorEnter(string.IsNullOrEmpty(doorID) ? gameObject.name : doorID, pid.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentifier pid = other.GetComponent<PlayerIdentifier>();

        if (pid != null && playersInTrigger.Contains(pid))
        {
            playersInTrigger.Remove(pid);
            UpdateOutlineColor();
            UpdatePrompt();
        }
    }

    private void UpdateOutlineColor()
    {
        if (playersInTrigger.Count == 0) SetOutlineState(Color.black, 0.0f);
        else if (playersInTrigger.Count == 1) SetOutlineState(playersInTrigger[0].PlayerOutlineColor, activeOutlineScale);
        else SetOutlineState(mixedColor, activeOutlineScale);
    }

    private void UpdatePrompt()
    {
        if (promptCanvas == null || promptText == null) return;
        if (playersInTrigger.Count > 0) {
            promptCanvas.enabled = true;
            promptText.text = "HOLD    TO LIFT";
            UpdatePromptVisuals();
        }
        else {
            promptCanvas.enabled = false;
            if (promptButtonImage != null) promptButtonImage.gameObject.SetActive(false);
        }
    }

    private void UpdatePromptVisuals()
    {
        if (promptCanvas != null)
        {
            Color c = PromptVisualHelper.ComputeColor(playersInTrigger, mixedColor);
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

    

    
    public void StartLifting(MovJugador1 lifter)
    {
        isBeingLifted = true;
        isFailLifting = false;
        playerLifter = lifter;
        playerFailler = null;
        hasDropped = false;
        isDropping = false;

        
        if (liftSound != null && audioSource != null)
        {
            audioSource.clip = liftSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        if (effortVoiceP1 != null && voiceAudioSource != null)
        {
            voiceAudioSource.clip = effortVoiceP1;
            voiceAudioSource.loop = true;
            voiceAudioSource.Play();
        }
    }

    
    public void StartFailLifting(MovJugador2 failler)
    {
        if (isBeingLifted) return;

        isFailLifting = true;
        playerFailler = failler;
        playerLifter = null;
        hasDropped = false;
        isDropping = false;
        failDialogueShown = false;

        
        if (liftSound != null && audioSource != null)
        {
            audioSource.clip = liftSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        if (effortVoiceP2 != null && voiceAudioSource != null)
        {
            voiceAudioSource.clip = effortVoiceP2;
            voiceAudioSource.loop = true;
            voiceAudioSource.Play();
        }

    }

    
    public void StopLifting()
    {
        if (isDropping) return;
        float currentHeight = transform.position.y - initialPosition.y;
        bool wasMidAction = (isBeingLifted || isFailLifting) && currentHeight > 0f && !isAtMaxHeight;
        GameObject actorGo = null;
        if (playerLifter != null) actorGo = playerLifter.gameObject;
        else if (playerFailler != null) actorGo = playerFailler.gameObject;
        isBeingLifted = false;
        isFailLifting = false;
        playerLifter = null;
        playerFailler = null;
        isDropping = true;

        if (actorGo != null && wasMidAction)
        {
            DialogueManager.ShowFallenDoorEarlyRelease(string.IsNullOrEmpty(doorID) ? gameObject.name : doorID, actorGo);
        }

        
        if (audioSource != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.loop = false;
            if (dropSound != null) audioSource.PlayOneShot(dropSound);
        }

        
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
    }

    private void HandleDoorMovement()
    {
        float currentHeight = transform.position.y - initialPosition.y;

        
        if (isBeingLifted && !isFailLifting && currentHeight < maxLiftHeight)
        {
            float shake = Random.Range(-shakeAmount, shakeAmount);
            float nextHeight = transform.position.y + (liftSpeed * Time.deltaTime);

            transform.position = new Vector3(initialPosition.x + shake, nextHeight, initialPosition.z);
            isDropping = false;

            if (playerLifter != null) playerLifter.StartCooperativeEffects(0.1f, 0.02f, 0.4f, 0.2f, 0.1f);

            
            if (transform.position.y >= initialPosition.y + maxLiftHeight)
            {
                transform.position = new Vector3(initialPosition.x, initialPosition.y + maxLiftHeight, initialPosition.z);

                if (!isAtMaxHeight)
                {
                    
                    if (audioSource != null)
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                        if (maxHeightSound != null) audioSource.PlayOneShot(maxHeightSound);
                    }
                    
                    if (voiceAudioSource != null) voiceAudioSource.Stop();

                    if (playerLifter != null) playerLifter.StartCooperativeEffects(0.2f, 0.1f, 0.8f, 0.8f, 0.2f);
                    isAtMaxHeight = true;
                    DialogueManager.ShowFallenDoorLiftSuccess(string.IsNullOrEmpty(doorID) ? gameObject.name : doorID);
                }
            }
        }

        
        else if (isFailLifting && !isBeingLifted)
        {
            if (currentHeight < failLiftHeight)
            {
                float shake = Random.Range(-shakeAmount, shakeAmount);
                Vector3 nextPos = Vector3.MoveTowards(transform.position, initialPosition + Vector3.up * failLiftHeight, liftSpeed * 0.5f * Time.deltaTime);
                nextPos.x = initialPosition.x + shake;
                transform.position = nextPos;
                isDropping = false;
            }
            else
            {
                float shake = Random.Range(-shakeAmount, shakeAmount);
                transform.position = new Vector3(initialPosition.x + shake, initialPosition.y + failLiftHeight, initialPosition.z);
                if (playerFailler != null) playerFailler.StartCooperativeEffects(0.1f, 0.05f, 0.3f, 0.3f, 0.1f);
                if (!failDialogueShown && playerFailler != null)
                {
                    failDialogueShown = true;
                    isFailLifting = false;
                    isDropping = true;
                    if (audioSource != null)
                    {
                        if (audioSource.isPlaying) audioSource.Stop();
                        audioSource.loop = false;
                        if (dropSound != null) audioSource.PlayOneShot(dropSound);
                    }
                    if (voiceAudioSource != null && voiceAudioSource.isPlaying)
                    {
                        voiceAudioSource.Stop();
                    }
                    DialogueManager.ShowFallenDoorLiftFail(string.IsNullOrEmpty(doorID) ? gameObject.name : doorID, playerFailler.gameObject);
                }
            }
        }

        
        else if (!isBeingLifted && !isFailLifting && currentHeight > 0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, dropSpeed * Time.deltaTime);

            Vector3 fixedX = transform.position;
            fixedX.x = initialPosition.x;
            transform.position = fixedX;

            isAtMaxHeight = false;

            if (isDropping && currentHeight <= crushHeightThreshold)
            {
                CheckForCrush();
            }

            if (transform.position.y <= initialPosition.y)
            {
                transform.position = initialPosition;
                isDropping = false;

                if (!hasDropped)
                {
                    if (audioSource != null) audioSource.loop = false;
                    TriggerDropImpactEffects();
                    hasDropped = true;
                }
            }
        }
        else if (currentHeight <= 0f)
        {
            hasDropped = true;
            isDropping = false;
            if (transform.position != initialPosition) transform.position = initialPosition;
        }
    }

    private void CheckForCrush()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.up * 0.1f, 0.8f, playerLayer);
        foreach (Collider hit in hits)
        {
            PlayerIdentifier hitID = hit.GetComponent<PlayerIdentifier>();
            if (hitID != null)
            {
                PlayerHealth victimHealth = hit.GetComponent<PlayerHealth>();
                if (victimHealth != null && !victimHealth.IsDead)
                {
                    victimHealth.SetLastDamageSource("FallenDoorCrush");
                    victimHealth.TakeDamage(crushDamage);

                    MovJugador1 p1 = hit.GetComponent<MovJugador1>();
                    if (p1) p1.StartCooperativeEffects(0.3f, 0.25f, 0.8f, 0.9f, 0.3f);
                    MovJugador2 p2 = hit.GetComponent<MovJugador2>();
                    if (p2) p2.StartCooperativeEffects(0.3f, 0.25f, 0.8f, 0.9f, 0.3f);
                }
            }
        }
    }

    private void TriggerDropImpactEffects()
    {
        foreach (var player in playersInTrigger)
        {
            if (player == null) continue;

            MovJugador1 p1 = player.GetComponent<MovJugador1>();
            if (p1) p1.StartCooperativeEffects(0f, 0f, dropLowFrequency, dropHighFrequency, dropRumbleDuration);

            MovJugador2 p2 = player.GetComponent<MovJugador2>();
            if (p2) p2.StartCooperativeEffects(0f, 0f, dropLowFrequency, dropHighFrequency, dropRumbleDuration);
        }
    }

    protected override void OnInteract() { }
}
