using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.EventSystems;

public class GameOverManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time to wait before showing Game Over UI.")]
    [SerializeField] private float gameOverDelay = 2.0f;
    [Tooltip("Duration of the fade to black effect.")]
    [SerializeField] private float fadeDuration = 1.5f;
    [Tooltip("Canvas sorting order for Game Over (higher = on top of everything).")]
    [SerializeField] private int gameOverCanvasSortingOrder = 9999;

    [Header("UI References - ASIGNAR EN EL INSPECTOR")]
    [Tooltip("El Canvas principal del Game Over (desactivado por defecto).")]
    [SerializeField] private Canvas gameOverCanvas;
    [Tooltip("El Canvas o Image para el fade a negro.")]
    [SerializeField] private Image fadeImage;
    [Tooltip("El texto 'GAME OVER'.")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [Tooltip("Bot�n de Continuar/Reintentar.")]
    [SerializeField] private Button retryButton;
    [Tooltip("Bot�n de Salir al Men�.")]
    [SerializeField] private Button quitButton;
    [Tooltip("(Opcional) CanvasGroup para animar los botones.")]
    [SerializeField] private CanvasGroup buttonCanvasGroup;

    [Header("NPC & Players References")]
    [Tooltip("The NPC that must survive. If null, will try to auto-find by tag.")]
    [SerializeField] private NPCHealth npcToProtect;
    [Tooltip("Tag to search for the NPC. Default: NPC")]
    [SerializeField] private string npcTag = "NPC";
    [Tooltip("The players in the scene. If empty, will try to auto-find.")]
    [SerializeField] private List<PlayerHealth> players;

    [Header("UI Audio")]
    [Tooltip("Sound played when navigating between buttons.")]
    [SerializeField] private AudioClip hoverSound;
    [Tooltip("Sound played when a button is clicked.")]
    [SerializeField] private AudioClip clickSound;

    private AudioSource uiAudioSource;
    private int deadPlayersCount = 0;
    private bool gameOverTriggered = false;
    private float originalTimeScale = 1f;
    private bool wasGamePaused = false;
    private int selectedButtonIndex = 0;
    private float lastNavigateTime = 0f;
    private float navigateRepeatDelay = 0.2f;
    private List<UnityEngine.InputSystem.PlayerInput> uiPlayerInputs = new List<UnityEngine.InputSystem.PlayerInput>();
    private bool hasSceneSnapshot = false;
    private List<(Transform t, Vector3 pos, Quaternion rot)> enemySnapshot = new List<(Transform, Vector3, Quaternion)>();
    private List<(SecurityDoor door, bool open)> securityDoorSnapshot = new List<(SecurityDoor, bool)>();
    private List<(DoorWithKeyCard door, bool locked, bool open)> keyCardDoorSnapshot = new List<(DoorWithKeyCard, bool, bool)>();

    public static GameOverManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup audio
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;
        uiAudioSource.ignoreListenerPause = true;
    }

    void Start()
    {
        // Verificar referencias cr�ticas
        ValidateUIReferences();

        // Configurar el sorting order del canvas
        if (gameOverCanvas != null)
        {
            gameOverCanvas.sortingOrder = gameOverCanvasSortingOrder;
            gameOverCanvas.gameObject.SetActive(false);
        }

        InitializeReferences();
        SubscribeToEvents();
        SetupButtonListeners();

        // Fade in inicial si existe fadeImage
        if (fadeImage != null)
        {
            StartCoroutine(InitialFadeIn());
        }

        if (npcToProtect == null)
        {
            StartCoroutine(SearchForNPCLater());
        }

        Checkpoint.OnCheckpointReached += OnCheckpointReached;
    }

    void Update()
    {
        if (!gameOverTriggered || gameOverCanvas == null || !gameOverCanvas.gameObject.activeInHierarchy)
            return;
    }

    private void ValidateUIReferences()
    {
        if (gameOverCanvas == null)
        {
            Debug.LogError("[GameOverManager] �CANVAS DE GAME OVER NO ASIGNADO! Por favor asigna el Canvas en el Inspector.");
        }

        if (fadeImage == null)
        {
            Debug.LogWarning("[GameOverManager] Fade Image no asignado. No habr� efecto de fade.");
        }

        if (gameOverText == null)
        {
            Debug.LogWarning("[GameOverManager] Game Over Text no asignado.");
        }

        if (retryButton == null)
        {
            Debug.LogWarning("[GameOverManager] Retry Button no asignado.");
        }

        if (quitButton == null)
        {
            Debug.LogWarning("[GameOverManager] Quit Button no asignado.");
        }
    }

    private void InitializeReferences()
    {
        if (npcToProtect == null)
        {
            npcToProtect = FindNPCByTag();
            if (npcToProtect == null)
            {
                npcToProtect = FindObjectOfType<NPCHealth>();
            }
        }

        if (players == null || players.Count == 0)
        {
            players = new List<PlayerHealth>(FindObjectsOfType<PlayerHealth>());
        }
    }

    private NPCHealth FindNPCByTag()
    {
        GameObject[] npcsWithTag = GameObject.FindGameObjectsWithTag(npcTag);

        foreach (GameObject npcObj in npcsWithTag)
        {
            NPCHealth npcHealth = npcObj.GetComponent<NPCHealth>();
            if (npcHealth != null)
            {
                return npcHealth;
            }
        }

        if (npcTag != "NPC")
        {
            GameObject npcWithDefaultTag = GameObject.FindGameObjectWithTag("NPC");
            if (npcWithDefaultTag != null)
            {
                return npcWithDefaultTag.GetComponent<NPCHealth>();
            }
        }

        return null;
    }

    private void SubscribeToEvents()
    {
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied += HandleNPCDeath;
        }

        foreach (var player in players)
        {
            if (player != null)
            {
                player.OnPlayerDied += HandlePlayerDeath;
            }
        }
    }

    private void OnDestroy()
    {
        Checkpoint.OnCheckpointReached -= OnCheckpointReached;
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied -= HandleNPCDeath;
        }

        foreach (var player in players)
        {
            if (player != null)
            {
                player.OnPlayerDied -= HandlePlayerDeath;
            }
        }
    }

    private void HandleNPCDeath(int npcID)
    {
        if (gameOverTriggered) return;

        if (npcToProtect == null)
        {
            npcToProtect = FindNPCByTag();
            if (npcToProtect == null) return;
        }

        TriggerGameOver();
    }

    private void HandlePlayerDeath(int playerID)
    {
        if (gameOverTriggered) return;

        deadPlayersCount++;
        Debug.Log($"[GameOverManager] Player {playerID} died. Dead count: {deadPlayersCount}/{players.Count}");

        // Solo triggerea game over si AMBOS jugadores est�n muertos
        if (deadPlayersCount >= players.Count)
        {
            Debug.Log("[GameOverManager] All players dead. Triggering Game Over...");
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;

        gameOverTriggered = true;
        Debug.Log("[GameOverManager] Game Over triggered!");

        HideAllRespawnPanels();

        StartCoroutine(GameOverSequence());
    }

    private void PauseGame()
    {
        originalTimeScale = Time.timeScale;
        wasGamePaused = Mathf.Approximately(Time.timeScale, 0f);

        if (!wasGamePaused)
        {
            Time.timeScale = 0f;
        }

        DisableAllPlayerInput();
        DisableAllEnemyAI();
        PauseGameAudio();

        Debug.Log("[GameOverManager] Game paused.");
    }

    private void ResumeGame()
    {
        if (!wasGamePaused)
        {
            Time.timeScale = originalTimeScale;
        }

        EnableAllPlayerInput();
        EnableAllEnemyAI();
        ResumeGameAudio();

        Debug.Log("[GameOverManager] Game resumed.");
    }

    private void DisableAllPlayerInput()
    {
        var mov1Scripts = FindObjectsOfType<MovJugador1>();
        foreach (var mov in mov1Scripts)
        {
            mov.enabled = false;
        }

        var mov2Scripts = FindObjectsOfType<MovJugador2>();
        foreach (var mov in mov2Scripts)
        {
            mov.enabled = false;
        }
    }

    private void EnableAllPlayerInput()
    {
        var mov1Scripts = FindObjectsOfType<MovJugador1>();
        foreach (var mov in mov1Scripts)
        {
            mov.enabled = true;
        }

        var mov2Scripts = FindObjectsOfType<MovJugador2>();
        foreach (var mov in mov2Scripts)
        {
            mov.enabled = true;
        }
    }

    private void DisableAllEnemyAI()
    {
        var navMeshAgents = FindObjectsOfType<UnityEngine.AI.NavMeshAgent>();
        foreach (var agent in navMeshAgents)
        {
            if (agent.gameObject.CompareTag("Enemy"))
            {
                agent.enabled = false;
            }
        }
    }

    private void EnableAllEnemyAI()
    {
        var navMeshAgents = FindObjectsOfType<UnityEngine.AI.NavMeshAgent>();
        foreach (var agent in navMeshAgents)
        {
            if (agent.gameObject.CompareTag("Enemy"))
            {
                agent.enabled = true;
            }
        }
    }

    private void PauseGameAudio()
    {
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            if (audioSource.GetComponentInParent<Canvas>() == null)
            {
                audioSource.Pause();
            }
        }
    }

    private void ResumeGameAudio()
    {
        var audioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            if (audioSource.GetComponentInParent<Canvas>() == null)
            {
                audioSource.UnPause();
            }
        }
    }

    public void AssignNPCToProtect(NPCHealth npc)
    {
        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied -= HandleNPCDeath;
        }

        npcToProtect = npc;

        if (npcToProtect != null)
        {
            npcToProtect.OnNPCDied += HandleNPCDeath;

            if (npcToProtect.gameObject.tag != npcTag && !string.IsNullOrEmpty(npcTag))
            {
                npcToProtect.gameObject.tag = npcTag;
            }
        }
    }

    private IEnumerator GameOverSequence()
    {
        // Fade a negro
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeToBlack());
        }

        PauseGame();
        yield return new WaitForSecondsRealtime(gameOverDelay);

        // Mostrar el UI
        ShowGameOverUI();
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Usar unscaled porque el juego est� pausado
                float fadeProgress = elapsed / fadeDuration;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, fadeProgress);

                color = Color.black;
                color.a = smoothProgress;
                fadeImage.color = color;

                yield return null;
            }

            color = Color.black;
            color.a = 1f;
            fadeImage.color = color;
        }
    }

    private IEnumerator InitialFadeIn()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            fadeImage.gameObject.SetActive(false);
        }
    }

    private void SetupButtonListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() => PlayClickSound());
            retryButton.onClick.AddListener(OnContinueButtonClicked);
            AddButtonTriggers(retryButton);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => PlayClickSound());
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            AddButtonTriggers(quitButton);
        }

        // Configurar navegaci�n entre botones
        if (retryButton != null && quitButton != null)
        {
            Navigation retryNav = new Navigation();
            retryNav.mode = Navigation.Mode.Explicit;
            retryNav.selectOnDown = quitButton;
            retryNav.selectOnUp = quitButton;
            retryButton.navigation = retryNav;

            Navigation quitNav = new Navigation();
            quitNav.mode = Navigation.Mode.Explicit;
            quitNav.selectOnUp = retryButton;
            quitNav.selectOnDown = retryButton;
            quitButton.navigation = quitNav;
        }
    }

    private void AddButtonTriggers(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entrySelect = new EventTrigger.Entry();
        entrySelect.eventID = EventTriggerType.Select;
        entrySelect.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entrySelect);
    }

    private void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverSound != null)
        {
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    private void ShowGameOverUI()
    {
        if (gameOverCanvas == null)
        {
            Debug.LogError("[GameOverManager] No se puede mostrar Game Over: Canvas no asignado.");
            return;
        }

        gameOverCanvas.gameObject.SetActive(true);

        if (retryButton != null)
        {
            selectedButtonIndex = 0;
            retryButton.Select();
            retryButton.OnSelect(null);
            EventSystem.current?.SetSelectedGameObject(retryButton.gameObject);
        }

        if (gameOverText != null || buttonCanvasGroup != null)
        {
            StartCoroutine(FadeInGameOverElements());
        }

        EnterGameOverUINavigation();
    }

    private IEnumerator FadeInGameOverElements()
    {
        // Inicializar alphas a 0
        if (gameOverText != null)
        {
            Color textColor = gameOverText.color;
            textColor.a = 0f;
            gameOverText.color = textColor;
        }

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 0f;
        }

        float duration = 1.5f;
        float elapsed = 0f;

        // Fade in gradual usando unscaled time
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (gameOverText != null)
            {
                Color textColor = gameOverText.color;
                textColor.a = alpha;
                gameOverText.color = textColor;
            }

            if (buttonCanvasGroup != null)
            {
                buttonCanvasGroup.alpha = alpha;
            }

            yield return null;
        }

        // Asegurar alpha final
        if (gameOverText != null)
        {
            Color textColor = gameOverText.color;
            textColor.a = 1f;
            gameOverText.color = textColor;
        }

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 1f;
        }
    }

    private void HideGameOverUI()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("[GameOverManager] Continue button clicked. Restarting from checkpoint...");

        HideGameOverUI();
        ExitGameOverUINavigation();
        StartCoroutine(RestartFromCheckpoint());
    }

    private IEnumerator RestartFromCheckpoint()
    {
        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeToBlack());
        }

        yield return new WaitForSecondsRealtime(0.2f);

        ResetGameOverState();
        Time.timeScale = 1f;
        ResumeGame();

        RespawnAllPlayers();

        HideAllRespawnPanels();

        if (fadeImage != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        Debug.Log("[GameOverManager] Players respawned using RestoreState.");
    }

    private void RespawnAllPlayers()
    {
        // Encontrar todos los jugadores y respawnearlos desde sus checkpoints
        foreach (var player in players)
        {
            if (player != null)
            {
                // Forzar que el jugador no est� muerto
                if (player.IsDead)
                {
                    // Llamar al RestoreState de cada jugador
                    // Esto usa sus lastCheckpointPosition guardadas
                    player.RestoreState();
                }
                else
                {
                    var ui = player.GetComponent<PlayerUIController>();
                    if (ui != null) ui.HideRespawnPanel();
                }
            }
        }

        Debug.Log($"[GameOverManager] {players.Count} players respawned from their checkpoints.");
    }

    private void ResetEnemies()
    {
        RestoreSceneSnapshot();
        Debug.Log("[GameOverManager] Scene restored from snapshot.");
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime; // Ahora usamos deltaTime normal porque ya resumimos el juego
                color.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            fadeImage.gameObject.SetActive(false);
        }
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("[GameOverManager] Quit button clicked. Returning to Main Menu...");

        StopAllCoroutines();
        ExitGameOverUINavigation();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void ResetGameOverState()
    {
        deadPlayersCount = 0;
        gameOverTriggered = false;

        Debug.Log("[GameOverManager] Game Over state reset.");
    }

    private void EnterGameOverUINavigation()
    {
        uiPlayerInputs.Clear();
        foreach (var p in players)
        {
            if (p == null) continue;
            var pi = p.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pi == null) continue;
            uiPlayerInputs.Add(pi);
        }
        if (uiPlayerInputs.Count == 0)
        {
            var all = FindObjectsOfType<UnityEngine.InputSystem.PlayerInput>();
            uiPlayerInputs.AddRange(all);
        }
        foreach (var pi in uiPlayerInputs)
        {
            try { pi.SwitchCurrentActionMap("UI"); } catch {}
            var actions = pi.actions;
            if (actions == null) continue;
            var nav = actions.FindAction("Navigate", true);
            var sub = actions.FindAction("Submit", false);
            var can = actions.FindAction("Cancel", false);
            if (nav != null) nav.performed += OnUINavigate;
            if (sub != null) sub.performed += OnUISubmit;
            if (can != null) can.performed += OnUICancel;
        }
    }

    private void ExitGameOverUINavigation()
    {
        foreach (var pi in uiPlayerInputs)
        {
            var actions = pi.actions;
            if (actions != null)
            {
                var nav = actions.FindAction("Navigate", false);
                var sub = actions.FindAction("Submit", false);
                var can = actions.FindAction("Cancel", false);
                if (nav != null) nav.performed -= OnUINavigate;
                if (sub != null) sub.performed -= OnUISubmit;
                if (can != null) can.performed -= OnUICancel;
            }
            try { pi.SwitchCurrentActionMap("Player"); } catch {}
        }
        uiPlayerInputs.Clear();
    }

    private void OnUINavigate(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - lastNavigateTime < navigateRepeatDelay) return;
        Vector2 v = ctx.ReadValue<Vector2>();
        if (Mathf.Abs(v.y) < 0.5f) return;
        lastNavigateTime = Time.unscaledTime;
        selectedButtonIndex = 1 - selectedButtonIndex;
        var target = selectedButtonIndex == 0 ? retryButton : quitButton;
        if (target == null) return;
        EventSystem.current?.SetSelectedGameObject(target.gameObject);
        target.Select();
    }

    private void OnUISubmit(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selected != null)
        {
            var btn = selected.GetComponent<Button>();
            if (btn != null) btn.onClick.Invoke();
        }
        else
        {
            var fallback = selectedButtonIndex == 0 ? retryButton : quitButton;
            if (fallback != null) fallback.onClick.Invoke();
        }
    }

    private void OnUICancel(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (quitButton != null) quitButton.onClick.Invoke();
    }

    private void OnCheckpointReached(int playerID, Vector3 position)
    {
        // Intentionally left blank to avoid using CheckpointSystem in retry flow
    }

    private void CreateSceneSnapshot()
    {
        enemySnapshot.Clear();
        securityDoorSnapshot.Clear();
        keyCardDoorSnapshot.Clear();

        var enemies = FindObjectsOfType<EnemyMonsterAI>();
        foreach (var e in enemies)
        {
            enemySnapshot.Add((e.transform, e.transform.position, e.transform.rotation));
        }
        var enemyTagged = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var go in enemyTagged)
        {
            var t = go.transform;
            if (!enemies.Any(x => x.transform == t))
            {
                enemySnapshot.Add((t, t.position, t.rotation));
            }
        }

        var secDoors = FindObjectsOfType<SecurityDoor>();
        foreach (var d in secDoors)
        {
            bool open = false;
            try { open = d.IsOpen; } catch { }
            securityDoorSnapshot.Add((d, open));
        }

        var keyDoors = FindObjectsOfType<DoorWithKeyCard>();
        foreach (var d in keyDoors)
        {
            bool locked = d.IsLocked;
            bool open = Vector3.Distance(d.transform.position, GetFieldVector3(d, "openPosition")) < 0.05f;
            keyCardDoorSnapshot.Add((d, locked, open));
        }

        hasSceneSnapshot = true;
        Debug.Log("[GameOverManager] Scene snapshot created.");
    }

    private void RestoreSceneSnapshot()
    {
        if (!hasSceneSnapshot) return;

        foreach (var rec in enemySnapshot)
        {
            if (rec.t == null) continue;
            var nav = rec.t.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null)
            {
                nav.isStopped = true;
                nav.ResetPath();
            }
            rec.t.position = rec.pos;
            rec.t.rotation = rec.rot;
        }

        foreach (var rec in securityDoorSnapshot)
        {
            if (rec.door == null) continue;
            rec.door.ForceSetState(rec.open);
        }

        foreach (var rec in keyCardDoorSnapshot)
        {
            if (rec.door == null) continue;
            rec.door.ForceSetState(rec.locked, rec.open, false);
        }

        foreach (var player in players)
        {
            if (player == null) continue;
            var anim = player.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetBool("IsDeadAnimator", false);
            }
            var ui = player.GetComponent<PlayerUIController>();
            if (ui != null) ui.HideRespawnPanel();
        }
    }

    private Vector3 GetFieldVector3(object obj, string fieldName)
    {
        var fi = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (fi != null && fi.FieldType == typeof(Vector3))
        {
            return (Vector3)fi.GetValue(obj);
        }
        return Vector3.zero;
    }

    private void HideAllRespawnPanels()
    {
        foreach (var player in players)
        {
            if (player == null) continue;
            var ui = player.GetComponent<PlayerUIController>();
            if (ui != null) ui.HideRespawnPanel();
        }
    }

    private IEnumerator SearchForNPCLater()
    {
        yield return new WaitForSeconds(2f);

        while (npcToProtect == null)
        {
            npcToProtect = FindNPCByTag();

            if (npcToProtect != null)
            {
                npcToProtect.OnNPCDied += HandleNPCDeath;
                Debug.Log("[GameOverManager] NPC to protect found and subscribed.");
                break;
            }

            yield return new WaitForSeconds(3f);
        }
    }
}
