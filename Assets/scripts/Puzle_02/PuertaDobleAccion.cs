using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PuertaDobleAccion : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private Transform puertaA;
    [SerializeField] private Transform puertaB;

    [Header("Opening Configuration")]
    [SerializeField] private int golpesNecesarios = 3;
    [SerializeField] private float velocidadApertura = 90f;
    [SerializeField] private Vector3 ejeRotacion = Vector3.up;

    [Header("Door Shake Effects")]
    [SerializeField] private float intensidadTemblor = 0.05f;
    [SerializeField] private float duracionTemblor = 0.3f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    [Header("Joystick Vibration")]
    [Range(0f, 1f)][SerializeField] private float lowFrequency = 0.8f;
    [Range(0f, 1f)][SerializeField] private float highFrequency = 0.3f;
    [SerializeField] private float rumbleDuration = 0.25f;

    [Header("Cooperation Time Window")]
    [SerializeField] private float ventanaDeTiempo = 0.3f;
    [SerializeField] private bool usarSistemaDeBuffer = true;
    [SerializeField] private float tiempoDeBuffer = 0.5f;

    [Header("Opening Mode")]
    [SerializeField] private bool modoIndividual = false;

    [Header("Notification Feedback")]
    [SerializeField] private string needHelpMessage = "I need help with this door!";
    [SerializeField] private string readyToActMessage = "Let's go, together now!";
    [SerializeField] private string waitingForPartnerMessage = "Waiting for partner...";

    [Header("Audio")]
    [SerializeField] private bool playDoorSounds = true;
    [SerializeField] private AudioClip hitSuccessSound;
    [SerializeField] private AudioClip hitErrorSound;
    [SerializeField] private AudioClip buttonPressSound;
    [SerializeField] private AudioClip buttonReadySound;
    [SerializeField] private float hitSoundVolume = 0.8f;
    [SerializeField] private float errorSoundVolume = 0.6f;
    [SerializeField] private float buttonPressSoundVolume = 0.5f;

    [Header("Button Indicators")]
    [SerializeField] private GameObject buttonIndicatorPrefab;
    [SerializeField] private Vector3 buttonOffsetFromPlayer = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private bool buttonFollowsPlayer = true;
    [SerializeField] private Vector3 player1ButtonOffset = new Vector3(-1f, 2f, 0f);
    [SerializeField] private Vector3 player2ButtonOffset = new Vector3(1f, 2f, 0f);
    [SerializeField] private float buttonAnimDuration = 0.3f;
    [SerializeField] private bool useBillboardEffect = true;

    [Header("Visual Feedback")]
    [SerializeField] private float buttonReadyPulseSpeed = 4f;
    [SerializeField] private Color buttonReadyColor = Color.yellow;

    [Header("Outline Multiplayer")]
    [SerializeField] private Color cooperativeOutlineColor = Color.yellow;
    [SerializeField] private string outlineColorProperty = "_Outline_Color";
    [SerializeField] private string outlineScaleProperty = "_Outline_Scale";
    [SerializeField] private float activeOutlineScale = 0.0125f;

    [Header("Shader FX")]
    [SerializeField] private Material puzzleFullscreenMat;
    [SerializeField] private float stressDecaySpeed = 0.8f;
    [SerializeField] private float stressAddedPerHit = 0.4f;
    [SerializeField] private float baseTension = 0.15f;

    [Header("UI Prompt Settings")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image promptButtonImage;
    [SerializeField] private RectTransform buttonAnchor;
    [SerializeField] private Vector2 buttonImageOffset = Vector2.zero;
    [SerializeField] private RectTransform singleButtonAnchor;
    [SerializeField] private RectTransform coopButtonAnchor;
    [SerializeField] private Vector2 singleButtonOffset = Vector2.zero;
    [SerializeField] private Vector2 coopButtonOffset = Vector2.zero;

    private List<PlayerIdentifier> activePlayers;
    private List<Renderer> doorRenderers;
    private MaterialPropertyBlock propertyBlock;
    private int outlineColorID;
    private int outlineScaleID;
    private Color originalOutlineColor = Color.black;

    private int golpesActuales = 0;
    private bool estaAbierta = false;
    private float anguloObjetivo = 0f;
    private float anguloActual = 0f;
    private bool isCoopMessageShown = false;

    private float currentStress = 0f;
    private int stressLevelID;

    private HashSet<GameObject> jugadoresEnTrigger;

    private class PlayerHitData
    {
        public float timestamp;
        public bool isWaitingForPartner;
        public Coroutine bufferCoroutine;

        public PlayerHitData(float time)
        {
            timestamp = time;
            isWaitingForPartner = false;
            bufferCoroutine = null;
        }
    }

    private Dictionary<GameObject, PlayerHitData> playerHitData;
    private Vector3 posicionOriginal;

    private GameObject buttonIndicator1;
    private GameObject buttonIndicator2;
    private Dictionary<GameObject, GameObject> playerButtonMap;

    private void Awake()
    {
        activePlayers = new List<PlayerIdentifier>();
        doorRenderers = new List<Renderer>();
        jugadoresEnTrigger = new HashSet<GameObject>();
        playerHitData = new Dictionary<GameObject, PlayerHitData>();
        playerButtonMap = new Dictionary<GameObject, GameObject>();
    }

    private void Start()
    {
        if (puertaA == null || puertaB == null)
        {

            enabled = false;
            return;
        }

        posicionOriginal = transform.position;
        anguloObjetivo = anguloActual;

        if (puzzleFullscreenMat != null)
        {
            stressLevelID = Shader.PropertyToID("_StressLevel");
            puzzleFullscreenMat.SetFloat(stressLevelID, 0f);
        }

        if (puertaA != null)
        {
            Renderer rendererA = puertaA.GetComponent<Renderer>();
            if (rendererA != null)
            {
                doorRenderers.Add(rendererA);
            }
        }

        if (puertaB != null)
        {
            Renderer rendererB = puertaB.GetComponent<Renderer>();
            if (rendererB != null)
            {
                doorRenderers.Add(rendererB);
            }
        }

        if (doorRenderers.Count > 0)
        {
            propertyBlock = new MaterialPropertyBlock();
            outlineColorID = Shader.PropertyToID(outlineColorProperty);
            outlineScaleID = Shader.PropertyToID(outlineScaleProperty);
            SetOutlineState(Color.black, 0.0f);
        }

        if (promptCanvas != null)
        {
            promptCanvas.enabled = false;
        }

        if (buttonIndicatorPrefab != null && !buttonFollowsPlayer)
        {
            buttonIndicator1 = Instantiate(buttonIndicatorPrefab, transform.position + player1ButtonOffset, Quaternion.identity, transform);
            buttonIndicator1.SetActive(false);

            if (useBillboardEffect)
            {
                AddBillboardEffect(buttonIndicator1);
            }

            buttonIndicator2 = Instantiate(buttonIndicatorPrefab, transform.position + player2ButtonOffset, Quaternion.identity, transform);
            buttonIndicator2.SetActive(false);

            if (useBillboardEffect)
            {
                AddBillboardEffect(buttonIndicator2);
            }
        }
    }

    private void Update()
    {
        if (estaAbierta)
        {
            anguloActual = Mathf.MoveTowards(anguloActual, anguloObjetivo, velocidadApertura * Time.deltaTime);

            puertaA.localRotation = Quaternion.AngleAxis(anguloActual, ejeRotacion);
            puertaB.localRotation = Quaternion.AngleAxis(-anguloActual, ejeRotacion);

            if (puzzleFullscreenMat != null && currentStress > 0f)
            {
                currentStress = Mathf.MoveTowards(currentStress, 0f, Time.deltaTime * 2f);
                puzzleFullscreenMat.SetFloat(stressLevelID, currentStress);
            }

            if (Mathf.Approximately(anguloActual, anguloObjetivo) && currentStress <= 0f)
            {
                enabled = false;
            }
        }
        else
        {
            HandleShaderUpdate();

            if (buttonFollowsPlayer)
            {
                UpdateButtonPositions();
            }
        }
    }

    private void HandleShaderUpdate()
    {
        if (puzzleFullscreenMat == null || jugadoresEnTrigger == null)
        {
            return;
        }

        float targetBase = jugadoresEnTrigger.Count >= 2 ? baseTension : 0f;

        if (currentStress > targetBase)
        {
            currentStress -= Time.deltaTime * stressDecaySpeed;
        }
        else if (currentStress < targetBase)
        {
            currentStress = Mathf.MoveTowards(currentStress, targetBase, Time.deltaTime);
        }

        puzzleFullscreenMat.SetFloat(stressLevelID, currentStress);
    }

    private void UpdateButtonPositions()
    {
        foreach (KeyValuePair<GameObject, GameObject> kvp in playerButtonMap)
        {
            GameObject player = kvp.Key;
            GameObject button = kvp.Value;

            if (player != null && button != null && button.activeSelf)
            {
                button.transform.position = player.transform.position + buttonOffsetFromPlayer;
            }
        }
    }

    private void SetOutlineState(Color color, float scale)
    {
        if (propertyBlock == null)
        {
            return;
        }

        foreach (Renderer rend in doorRenderers)
        {
            if (rend == null || rend.sharedMaterials.Length < 2)
            {
                continue;
            }

            rend.GetPropertyBlock(propertyBlock, 1);
            propertyBlock.SetColor(outlineColorID, color);
            propertyBlock.SetFloat(outlineScaleID, scale);
            rend.SetPropertyBlock(propertyBlock, 1);
        }
    }

    private void UpdateOutlineVisuals()
    {
        if (estaAbierta)
        {
            return;
        }

        int count = jugadoresEnTrigger != null ? jugadoresEnTrigger.Count : 0;
        if (count == 0)
        {
            SetOutlineState(originalOutlineColor, 0.0f);
        }
        else if (count == 1)
        {
            GameObject only = null;
            foreach (var p in jugadoresEnTrigger) { only = p; break; }
            var id = only != null ? only.GetComponent<PlayerIdentifier>() : null;
            Color c = id != null ? id.PlayerOutlineColor : originalOutlineColor;
            SetOutlineState(c, activeOutlineScale);
        }
        else
        {
            SetOutlineState(cooperativeOutlineColor, activeOutlineScale);
        }
    }

    private void AddBillboardEffect(GameObject buttonObject)
    {
        if (buttonObject == null)
        {
            return;
        }

        BillboardEffect billboard = buttonObject.GetComponent<BillboardEffect>();
        if (billboard == null)
        {
            buttonObject.AddComponent<BillboardEffect>();
        }
    }

    private IEnumerator ShakeDoor()
    {
        float timer = 0f;
        while (timer < duracionTemblor)
        {
            float x = Random.Range(-1f, 1f) * intensidadTemblor;
            float z = Random.Range(-1f, 1f) * intensidadTemblor;

            transform.position = posicionOriginal + new Vector3(x, 0f, z);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = posicionOriginal;
    }

    private IEnumerator AnimateButtonPress(GameObject buttonIndicator)
    {
        if (buttonIndicator == null)
        {
            yield break;
        }

        ButtonIndicatorController controller = buttonIndicator.GetComponent<ButtonIndicatorController>();
        if (controller != null)
        {
            controller.TriggerPress();
        }

        yield return new WaitForSeconds(buttonAnimDuration);
    }

    private IEnumerator WaitForPartnerBuffer(GameObject jugador)
    {
        if (!playerHitData.ContainsKey(jugador))
        {
            yield break;
        }

        PlayerHitData hitData = playerHitData[jugador];
        hitData.isWaitingForPartner = true;

        if (playerButtonMap.ContainsKey(jugador))
        {
            ButtonIndicatorController controller = playerButtonMap[jugador].GetComponent<ButtonIndicatorController>();
            if (controller != null)
            {
                StartCoroutine(PulseButtonWhileWaiting(controller, tiempoDeBuffer));
            }
        }

        PlayerUIController uiController = GetPlayerUIController(jugador);
        if (uiController != null)
        {
            uiController.ShowNotification(waitingForPartnerMessage);
        }

        yield return new WaitForSeconds(tiempoDeBuffer);

        if (hitData.isWaitingForPartner)
        {
            hitData.isWaitingForPartner = false;
            hitData.timestamp = float.MinValue;
        }
    }

    private IEnumerator PulseButtonWhileWaiting(ButtonIndicatorController controller, float duration)
    {
        float elapsed = 0f;
        Color originalColor = Color.white;

        while (elapsed < duration)
        {
            float pulse = Mathf.Sin(elapsed * buttonReadyPulseSpeed * Mathf.PI) * 0.5f + 0.5f;
            Color pulseColor = Color.Lerp(originalColor, buttonReadyColor, pulse * 0.5f);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void PlayButtonPressAudio()
    {
        if (buttonPressSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonPressSound, transform.position, buttonPressSoundVolume, 1f);
        }
    }

    private void PlayButtonReadyAudio()
    {
        if (buttonReadySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonReadySound, transform.position, buttonPressSoundVolume * 0.8f, 1.1f);
        }
    }

    private void PlayHitSuccessAudio()
    {
        if (hitSuccessSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitSuccessSound, transform.position, hitSoundVolume, 1f);
        }
    }

    private void PlayErrorAudio()
    {
        if (hitErrorSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitErrorSound, transform.position, errorSoundVolume, 1f);
        }
    }

    public void IntentoDeAccion(GameObject jugador)
    {
        if (estaAbierta || !jugadoresEnTrigger.Contains(jugador))
        {
            return;
        }

        float tiempoActual = Time.time;

        if (!playerHitData.ContainsKey(jugador))
        {
            playerHitData[jugador] = new PlayerHitData(tiempoActual);
        }

        PlayerHitData currentHitData = playerHitData[jugador];
        currentHitData.timestamp = tiempoActual;

        if (playerButtonMap.ContainsKey(jugador))
        {
            ButtonIndicatorController controller = playerButtonMap[jugador].GetComponent<ButtonIndicatorController>();
            if (controller != null)
            {
                controller.TriggerPress();
            }
            StartCoroutine(AnimateButtonPress(playerButtonMap[jugador]));
            PlayButtonPressAudio();
        }

        if (modoIndividual)
        {
            GolpeExitosoIndividual(jugador);
            return;
        }

        if (jugadoresEnTrigger.Count < 2)
        {
            PlayErrorAudio();
            return;
        }

        GameObject otroJugador = null;
        foreach (GameObject p in jugadoresEnTrigger)
        {
            if (p != jugador)
            {
                otroJugador = p;
                break;
            }
        }

        if (otroJugador == null)
        {
            return;
        }

        if (!playerHitData.ContainsKey(otroJugador))
        {
            playerHitData[otroJugador] = new PlayerHitData(float.MinValue);
        }

        PlayerHitData otherHitData = playerHitData[otroJugador];
        float diferenciaTiempo = Mathf.Abs(tiempoActual - otherHitData.timestamp);

        if (usarSistemaDeBuffer && otherHitData.isWaitingForPartner)
        {
            if (otherHitData.bufferCoroutine != null)
            {
                StopCoroutine(otherHitData.bufferCoroutine);
                otherHitData.bufferCoroutine = null;
            }

            otherHitData.isWaitingForPartner = false;
            GolpeExitoso(jugador, otroJugador);

            currentHitData.timestamp = float.MinValue;
            otherHitData.timestamp = float.MinValue;
            return;
        }

        if (diferenciaTiempo <= ventanaDeTiempo && otherHitData.timestamp > float.MinValue)
        {
            GolpeExitoso(jugador, otroJugador);

            currentHitData.timestamp = float.MinValue;
            otherHitData.timestamp = float.MinValue;
            return;
        }

        if (usarSistemaDeBuffer && !currentHitData.isWaitingForPartner)
        {
            if (currentHitData.bufferCoroutine != null)
            {
                StopCoroutine(currentHitData.bufferCoroutine);
            }

            currentHitData.bufferCoroutine = StartCoroutine(WaitForPartnerBuffer(jugador));
            PlayButtonReadyAudio();
            return;
        }

        PlayErrorAudio();
    }

    private void GolpeExitoso(GameObject playerA, GameObject playerB)
    {
        golpesActuales++;
        bool isFinalHit = golpesActuales >= golpesNecesarios;

        currentStress = Mathf.Clamp01(currentStress + stressAddedPerHit);

        StartCoroutine(ShakeDoor());
        RequestCooperativeEffects(playerA);
        RequestCooperativeEffects(playerB);

        PlayHitSuccessAudio();

        if (playerButtonMap.ContainsKey(playerA))
        {
            ButtonIndicatorController controllerA = playerButtonMap[playerA].GetComponent<ButtonIndicatorController>();
            if (controllerA != null)
            {
                controllerA.TriggerShockwave(isFinalHit);
            }
        }

        if (playerButtonMap.ContainsKey(playerB))
        {
            ButtonIndicatorController controllerB = playerButtonMap[playerB].GetComponent<ButtonIndicatorController>();
            if (controllerB != null)
            {
                controllerB.TriggerShockwave(isFinalHit);
            }
        }

        if (playDoorSounds && AudioManager.Instance != null)
        {
            var cfg = AudioManager.Instance.GetAudioConfig();
            if (cfg.doorOpenSounds != null && cfg.doorOpenSounds.Length > 0)
            {
                AudioManager.Instance.PlaySFX(cfg.doorOpenSounds[0], transform.position, 0.7f, 0.9f);
            }
        }

        if (golpesActuales == golpesNecesarios - 1)
        {
            DialogueManager.ShowDoubleDoorFinalHitApproach();
        }

        if (golpesActuales >= golpesNecesarios)
        {
            AbrirPuerta();
        }
    }

    private void GolpeExitosoIndividual(GameObject jugador)
    {
        golpesActuales++;
        bool isFinalHit = golpesActuales >= golpesNecesarios;

        currentStress = Mathf.Clamp01(currentStress + stressAddedPerHit);

        StartCoroutine(ShakeDoor());
        RequestCooperativeEffects(jugador);

        PlayHitSuccessAudio();

        if (playerButtonMap.ContainsKey(jugador))
        {
            ButtonIndicatorController controller = playerButtonMap[jugador].GetComponent<ButtonIndicatorController>();
            if (controller != null)
            {
                controller.TriggerShockwave(isFinalHit);
            }
        }

        if (playDoorSounds && AudioManager.Instance != null)
        {
            var cfg = AudioManager.Instance.GetAudioConfig();
            if (cfg.doorOpenSounds != null && cfg.doorOpenSounds.Length > 0)
            {
                AudioManager.Instance.PlaySFX(cfg.doorOpenSounds[0], transform.position, 0.7f, 0.9f);
            }
        }

        if (golpesActuales >= golpesNecesarios)
        {
            AbrirPuerta();
        }
    }

    private void RequestCooperativeEffects(GameObject player)
    {
        MovJugador1 mov1 = player.GetComponent<MovJugador1>();
        if (mov1 != null)
        {
            mov1.StartCooperativeEffects(shakeDuration, shakeMagnitude, lowFrequency, highFrequency, rumbleDuration);
            return;
        }

        MovJugador2 mov2 = player.GetComponent<MovJugador2>();
        if (mov2 != null)
        {
            mov2.StartCooperativeEffects(shakeDuration, shakeMagnitude, lowFrequency, highFrequency, rumbleDuration);
        }
    }

    private void AbrirPuerta()
    {
        estaAbierta = true;
        anguloObjetivo += 90f;
        SetOutlineState(Color.black, 0.0f);

        if (playDoorSounds && AudioManager.Instance != null)
        {
            var cfg = AudioManager.Instance.GetAudioConfig();
            if (cfg.doorOpenSounds != null && cfg.doorOpenSounds.Length > 0)
            {
                AudioManager.Instance.PlaySFX(cfg.doorOpenSounds[0], transform.position, 0.8f, 1f);
            }
        }

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        DialogueManager.ShowDoubleDoorOpened();

        foreach (GameObject player in jugadoresEnTrigger)
        {
            if (player == null)
            {
                continue;
            }

            MovJugador1 mov1 = player.GetComponent<MovJugador1>();
            MovJugador2 mov2 = player.GetComponent<MovJugador2>();

            if (mov1 != null)
            {
                mov1.ClearCurrentDoor(this);
            }

            if (mov2 != null)
            {
                mov2.ClearCurrentDoor(this);
            }
        }

        
        if (buttonFollowsPlayer)
        {
            foreach (KeyValuePair<GameObject, GameObject> kvp in playerButtonMap)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
        }
        else
        {
            if (buttonIndicator1 != null)
            {
                buttonIndicator1.SetActive(false);
            }

            if (buttonIndicator2 != null)
            {
                buttonIndicator2.SetActive(false);
            }
        }

        jugadoresEnTrigger.Clear();
        playerHitData.Clear();
        playerButtonMap.Clear();

        if (promptCanvas != null)
        {
            promptCanvas.enabled = false;
        }
    }

    private PlayerUIController GetPlayerUIController(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return null;
        }

        PlayerUIController ui = playerObject.GetComponent<PlayerUIController>();
        if (ui != null)
        {
            return ui;
        }

        return playerObject.GetComponentInParent<PlayerUIController>();
    }

    public void AddPlayer(GameObject player)
    {
        PlayerIdentifier playerIdentifier = player.GetComponent<PlayerIdentifier>();
        if (playerIdentifier != null)
        {
            if (!activePlayers.Contains(playerIdentifier))
            {
                activePlayers.Add(playerIdentifier);
            }
        }

        if (estaAbierta || jugadoresEnTrigger.Contains(player))
        {
            return;
        }

        jugadoresEnTrigger.Add(player);
        UpdateOutlineVisuals();

        if (!playerHitData.ContainsKey(player))
        {
            playerHitData[player] = new PlayerHitData(float.MinValue);
        }

        if (buttonIndicatorPrefab != null)
        {
            GameObject assignedButton = null;

            if (buttonFollowsPlayer)
            {
                assignedButton = Instantiate(buttonIndicatorPrefab, player.transform.position + buttonOffsetFromPlayer, Quaternion.identity);
                assignedButton.name = string.Format("ButtonIndicator_{0}", player.name);

                if (useBillboardEffect)
                {
                    AddBillboardEffect(assignedButton);
                }
            }
            else
            {
                if (jugadoresEnTrigger.Count == 1)
                {
                    assignedButton = buttonIndicator1;
                }
                else if (jugadoresEnTrigger.Count == 2)
                {
                    bool button1Assigned = false;
                    foreach (KeyValuePair<GameObject, GameObject> kvp in playerButtonMap)
                    {
                        if (kvp.Value == buttonIndicator1)
                        {
                            button1Assigned = true;
                            break;
                        }
                    }
                    assignedButton = button1Assigned ? buttonIndicator2 : buttonIndicator1;
                }
            }

            if (assignedButton != null)
            {
                assignedButton.SetActive(true);
                playerButtonMap[player] = assignedButton;

                ButtonIndicatorController controller = assignedButton.GetComponent<ButtonIndicatorController>();
                if (controller != null && playerIdentifier != null)
                {
                    controller.SetPlayerColor(playerIdentifier.PlayerOutlineColor);
                }
            }
        }

        PlayerUIController uiController = GetPlayerUIController(player);

        if (jugadoresEnTrigger.Count == 1)
        {
            if (uiController != null)
            {
                uiController.ShowNotification(needHelpMessage);
            }

            if (promptCanvas != null && promptText != null)
            {
                promptCanvas.enabled = true;
                promptText.text = "PRESS AT THE SAME TIME TO HIT THE DOOR";
                UpdatePromptVisuals();
            }

            isCoopMessageShown = false;
            DialogueManager.ShowDoubleDoorSingleEnter(player);
        }
        else if (jugadoresEnTrigger.Count >= 2 && !isCoopMessageShown)
        {
            if (uiController != null)
            {
                uiController.ShowNotification(readyToActMessage);
            }

            foreach (GameObject otherPlayer in jugadoresEnTrigger)
            {
                if (otherPlayer != player)
                {
                    PlayerUIController otherUI = GetPlayerUIController(otherPlayer);
                    if (otherUI != null)
                    {
                        otherUI.ShowNotification(readyToActMessage);
                    }
                }
            }
            isCoopMessageShown = true;

            if (promptCanvas != null && promptText != null)
            {
                promptCanvas.enabled = true;
                promptText.text = "READY - PRESS BOTH";
                UpdatePromptVisuals();
            }
            DialogueManager.ShowDoubleDoorBothReady();
        }

        if (promptCanvas != null && promptText != null)
        {
            promptCanvas.enabled = true;
            promptText.text = jugadoresEnTrigger.Count >= 2 ? "READY - PRESS BOTH" : "PRESS AT THE SAME TIME TO HIT THE DOOR";
            UpdatePromptVisuals();
        }
    }

    public void RemovePlayer(GameObject player)
    {
        PlayerIdentifier playerIdentifier = player.GetComponent<PlayerIdentifier>();
        if (playerIdentifier != null)
        {
            if (activePlayers.Contains(playerIdentifier))
            {
                activePlayers.Remove(playerIdentifier);
            }
        }

        if (estaAbierta || !jugadoresEnTrigger.Contains(player))
        {
            return;
        }

        if (playerHitData.ContainsKey(player))
        {
            PlayerHitData hitData = playerHitData[player];
            if (hitData.bufferCoroutine != null)
            {
                StopCoroutine(hitData.bufferCoroutine);
                hitData.bufferCoroutine = null;
            }
            playerHitData.Remove(player);
        }

        if (playerButtonMap.ContainsKey(player))
        {
            GameObject buttonToRemove = playerButtonMap[player];

            if (buttonFollowsPlayer)
            {
                if (buttonToRemove != null)
                {
                    Destroy(buttonToRemove);
                }
            }
            else
            {
                if (buttonToRemove != null)
                {
                    buttonToRemove.SetActive(false);
                }
            }

            playerButtonMap.Remove(player);
        }

        jugadoresEnTrigger.Remove(player);
        UpdateOutlineVisuals();

        if (jugadoresEnTrigger.Count < 2)
        {
            isCoopMessageShown = false;
        }

        if (jugadoresEnTrigger.Count == 1)
        {
            GameObject remainingPlayer = null;
            foreach (GameObject p in jugadoresEnTrigger)
            {
                remainingPlayer = p;
                break;
            }

            if (remainingPlayer != null)
            {
                PlayerUIController remainingUI = GetPlayerUIController(remainingPlayer);
                if (remainingUI != null)
                {
                    remainingUI.ShowNotification(needHelpMessage);
                }

                if (promptCanvas != null && promptText != null)
                {
                    promptCanvas.enabled = true;
                    promptText.text = "PRESS AT THE SAME TIME TO HIT THE DOOR";
                    UpdatePromptVisuals();
                }
            }
        }
        else if (jugadoresEnTrigger.Count == 0 && promptCanvas != null)
        {
            promptCanvas.enabled = false;
        }
    }

    private void UpdatePromptVisuals()
    {
        if (promptCanvas != null)
        {
            Color c = PromptVisualHelper.ComputeColor(jugadoresEnTrigger, cooperativeOutlineColor);
            PromptVisualHelper.ApplyToPrompt(promptCanvas.gameObject, c);

            if (promptButtonImage != null)
            {
                bool active = promptCanvas.enabled;
                promptButtonImage.gameObject.SetActive(active);
                if (active)
                {
                    bool isCoop = jugadoresEnTrigger != null && jugadoresEnTrigger.Count >= 2;
                    RectTransform anchor = isCoop ? (coopButtonAnchor != null ? coopButtonAnchor : buttonAnchor)
                                                  : (singleButtonAnchor != null ? singleButtonAnchor : buttonAnchor);
                    Vector2 offset = isCoop ? coopButtonOffset : singleButtonOffset;

                    if (anchor != null)
                    {
                        promptButtonImage.rectTransform.SetParent(anchor, false);
                        promptButtonImage.rectTransform.anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        promptButtonImage.rectTransform.SetParent(promptCanvas.transform, false);
                        promptButtonImage.rectTransform.anchoredPosition = offset;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<GameObject, PlayerHitData> kvp in playerHitData)
        {
            if (kvp.Value.bufferCoroutine != null)
            {
                StopCoroutine(kvp.Value.bufferCoroutine);
            }
        }

        if (buttonFollowsPlayer)
        {
            foreach (KeyValuePair<GameObject, GameObject> kvp in playerButtonMap)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
        }
        else
        {
            if (buttonIndicator1 != null)
            {
                Destroy(buttonIndicator1);
            }

            if (buttonIndicator2 != null)
            {
                Destroy(buttonIndicator2);
            }
        }
    }
}
