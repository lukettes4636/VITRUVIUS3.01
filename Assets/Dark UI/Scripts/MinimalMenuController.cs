using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Michsky.UI.Dark
{
    public class MinimalMenuController : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════════
        //  MENU PRINCIPAL
        // ═══════════════════════════════════════════════════════════════
        [Header("MENU")]
        public Transform mainMenuButtonsRoot;
        public MainPanelManager mainPanelManager;
        public GameObject optionsPanelRoot;
        public string startSceneName = "DaVinciPB";
        public GameObject creditsPanelRoot;
        public int fallbackOptionsPanelIndex = 1;
        public int fallbackCreditsPanelIndex = 2;

        // ═══════════════════════════════════════════════════════════════
        //  SETTINGS — CANVAS GROUP (fade + bloqueo de raycasts)
        // ═══════════════════════════════════════════════════════════════
        [Header("SETTINGS PANEL")]
        [Tooltip("CanvasGroup del panel de Settings. Se gestiona con fade.")]
        public CanvasGroup settingsCanvasGroup;
        [Tooltip("Duración en segundos del fade de entrada y salida.")]
        public float fadeDuration = 0.3f;

        // ═══════════════════════════════════════════════════════════════
        //  JOYSTICK / GAMEPAD — FOCO AUTOMÁTICO
        // ═══════════════════════════════════════════════════════════════
        [Header("JOYSTICK — FOCO")]
        [Tooltip("Primer elemento seleccionable al abrir Settings (ej. Slider Master Volume).")]
        public GameObject primerElementoSettings;
        [Tooltip("Primer elemento seleccionable al cerrar Settings (ej. Botón START).")]
        public GameObject primerElementoMainMenu;

        // ═══════════════════════════════════════════════════════════════
        //  AUDIO
        // ═══════════════════════════════════════════════════════════════
        [Header("AUDIO")]
        public AudioMixer audioMixer;
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;

        // ═══════════════════════════════════════════════════════════════
        //  BRILLO — POST PROCESSING (URP Volume)
        // ═══════════════════════════════════════════════════════════════
        [Header("BRILLO")]
        [Tooltip("Volume global de URP que contiene el perfil con ColorAdjustments.")]
        public Volume globalVolume;
        public Slider brightnessSlider;

        // ═══════════════════════════════════════════════════════════════
        //  CALIDAD — CustomDropdown de Michsky
        // ═══════════════════════════════════════════════════════════════
        [Header("CALIDAD")]
        [Tooltip("Dropdown de Dark UI. Cada ítem del dropdown corresponde a un nivel de calidad de Unity.")]
        public CustomDropdown qualityDropdown;

        // ═══════════════════════════════════════════════════════════════
        //  RESOLUCIÓN
        // ═══════════════════════════════════════════════════════════════
        [Header("RESOLUCIÓN")]
        public CustomDropdown resolutionDropdown;

        // ═══════════════════════════════════════════════════════════════
        //  PRIVADOS
        // ═══════════════════════════════════════════════════════════════
        readonly HashSet<string> allowedMainButtons = new HashSet<string>(new[] { "START", "OPTIONS", "CREDITS" });
        readonly HashSet<string> allowedOptionGroups = new HashSet<string>(new[] { "MouseSensitivity", "Brightness", "Resolution" });

        Resolution[] resolutions;
        List<string> resOptions = new List<string>();
        Coroutine fadeCoroutine;
        ColorAdjustments colorAdjustments; // cacheado para no buscarlo cada frame

        // ═══════════════════════════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ═══════════════════════════════════════════════════════════════
        void Awake()
        {
            // Settings panel comienza cerrado y sin interacción
            if (settingsCanvasGroup != null)
            {
                settingsCanvasGroup.alpha = 0f;
                settingsCanvasGroup.interactable = false;
                settingsCanvasGroup.blocksRaycasts = false;
            }

            // Cachear ColorAdjustments para evitar búsquedas repetidas
            if (globalVolume != null && globalVolume.profile != null)
                globalVolume.profile.TryGet(out colorAdjustments);

            FilterMainMenuButtons();
            FilterOptionGroups();
            SetupQualityDropdown();
            SetupResolutionDropdown();
            BindAudioSliders();
            
            WireMenuButtons();
        }
        void Start()
        {
            LoadPrefs();
        }
        // ═══════════════════════════════════════════════════════════════
        //  INPUT UPDATE — CERRAR CON BOTÓN B (CANCEL)
        // ═══════════════════════════════════════════════════════════════
        void Update()
        {
            // Verificamos si el Dropdown de calidad existe y si está abierto
            bool isQualityDropdownOpen = qualityDropdown != null && qualityDropdown.isOn;

            // También verificamos el de resolución por si acaso
            bool isResDropdownOpen = resolutionDropdown != null && resolutionDropdown.isOn;

            // Si se presiona el botón "Cancel" (B en Xbox / Círculo en PS)
            if (Input.GetButtonDown("Cancel"))
            {
                // Solo cerramos el panel de Settings SI:
                // 1. El panel es visible (Alpha > 0)
                // 2. Y NINGUNO de los dropdowns está abierto (porque si están abiertos, ellos se cierran solos)
                if (settingsCanvasGroup != null && settingsCanvasGroup.alpha > 0.1f && !isQualityDropdownOpen && !isResDropdownOpen)
                {
                    CloseSettingsMenu();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  FILTROS DE UI
        // ═══════════════════════════════════════════════════════════════
        void FilterMainMenuButtons()
        {
            if (mainMenuButtonsRoot == null) return;

            for (int i = 0; i < mainMenuButtonsRoot.childCount; i++)
            {
                var child = mainMenuButtonsRoot.GetChild(i).gameObject;
                var label = child.GetComponentInChildren<TextMeshProUGUI>();
                var name = label != null
                    ? label.text.Trim().ToUpperInvariant()
                    : child.name.Trim().ToUpperInvariant();

                if (!allowedMainButtons.Contains(name))
                    child.SetActive(false);
            }
        }

        void FilterOptionGroups()
        {
            if (optionsPanelRoot == null) return;

            for (int i = 0; i < optionsPanelRoot.transform.childCount; i++)
            {
                var group = optionsPanelRoot.transform.GetChild(i).gameObject;
                if (!allowedOptionGroups.Contains(group.name.Trim()))
                    group.SetActive(false);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  BIND SLIDERS DE AUDIO
        //  Los listeners se registran aquí una sola vez. Así, cuando
        //  LoadPrefs asigne .value, los setters se disparan automáticamente
        //  y el AudioMixer/PostProcess se actualiza sin duplicar lógica.
        // ═══════════════════════════════════════════════════════════════
        void BindAudioSliders()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        // ═══════════════════════════════════════════════════════════════
        //  QUALITY DROPDOWN
        // ═══════════════════════════════════════════════════════════════
        void SetupQualityDropdown()
        {
            if (qualityDropdown == null) return;

            for (int i = 0; i < qualityDropdown.dropdownItems.Count; i++)
            {
                int capturedIndex = i; // captura local imprescindible para el closure de lambda

                if (qualityDropdown.dropdownItems[capturedIndex].OnItemSelection == null)
                    qualityDropdown.dropdownItems[capturedIndex].OnItemSelection =
                        new UnityEngine.Events.UnityEvent();

                qualityDropdown.dropdownItems[capturedIndex].OnItemSelection.AddListener(
                    () => SetQuality(capturedIndex)
                );
            }

            qualityDropdown.SetupDropdown();
        }

        // ═══════════════════════════════════════════════════════════════
        //  RESOLUTION DROPDOWN
        // ═══════════════════════════════════════════════════════════════
        void SetupResolutionDropdown()
        {
            if (resolutionDropdown == null) return;

            resOptions.Clear();
            resolutionDropdown.dropdownItems.RemoveRange(0, resolutionDropdown.dropdownItems.Count);

            resolutions = Screen.resolutions;
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                resOptions.Add(option);
                resolutionDropdown.CreateNewOption(resOptions[i]);

                var item = resolutionDropdown.dropdownItems[i];
                item.OnItemSelection = new UnityEngine.Events.UnityEvent();
                item.OnItemSelection.AddListener(UpdateResolution);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    resolutionDropdown.selectedItemIndex = currentResolutionIndex;
                    resolutionDropdown.index = currentResolutionIndex;
                }
            }

            resolutionDropdown.SetupDropdown();
        }

        // ═══════════════════════════════════════════════════════════════
        //  WIRE MENU BUTTONS
        // ═══════════════════════════════════════════════════════════════
        void WireMenuButtons()
        {
            if (mainMenuButtonsRoot == null) return;

            for (int i = 0; i < mainMenuButtonsRoot.childCount; i++)
            {
                var child = mainMenuButtonsRoot.GetChild(i).gameObject;
                var label = child.GetComponentInChildren<TextMeshProUGUI>();
                var btnName = label != null
                    ? label.text.Trim().ToUpperInvariant()
                    : child.name.Trim().ToUpperInvariant();

                var btn = child.GetComponent<Button>();
                if (btn == null) continue;

                switch (btnName)
                {
                    case "START":
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(StartGame);
                        break;

                    case "OPTIONS":
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(OpenOptions);
                        break;

                    case "CREDITS":
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(OpenCredits);
                        break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  LOAD PREFS
        //  Carga todos los valores guardados, los asigna visualmente a los
        //  controles de UI y los aplica al motor (AudioMixer, PostProcess,
        //  QualitySettings). Los listeners registrados en BindAudioSliders
        //  se encargan de propagar el valor al motor cuando se asigna .value.
        // ═══════════════════════════════════════════════════════════════
        void LoadPrefs()
        {
            // ── Volumen Master ───────────────────────────────────────────
            float masterVal = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (masterSlider != null)
                masterSlider.value = masterVal; // dispara listener → SetMasterVolume
            else
                SetMasterVolume(masterVal);

            // ── Volumen Music ────────────────────────────────────────────
            float musicVal = PlayerPrefs.GetFloat("MusicVolume", 1f);
            if (musicSlider != null)
                musicSlider.value = musicVal;
            else
                SetMusicVolume(musicVal);

            // ── Volumen SFX ──────────────────────────────────────────────
            float sfxVal = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (sfxSlider != null)
                sfxSlider.value = sfxVal;
            else
                SetSFXVolume(sfxVal);

            // ── Brillo ───────────────────────────────────────────────────
            // Valor por defecto 0 (sin exposición extra), rango típico -2 a 2
            float brightnessVal = PlayerPrefs.GetFloat("Brightness", 0f);
            if (brightnessSlider != null)
                brightnessSlider.value = brightnessVal;
            else
                SetBrightness(brightnessVal);

            // ── Calidad ──────────────────────────────────────────────────
            if (qualityDropdown != null)
            {
                int savedQualityIndex = PlayerPrefs.GetInt("QualityIndex", 0);
                savedQualityIndex = Mathf.Clamp(savedQualityIndex, 0, qualityDropdown.dropdownItems.Count - 1);

                qualityDropdown.selectedItemIndex = savedQualityIndex;
                qualityDropdown.index = savedQualityIndex;

                // Refrescar la UI del dropdown para que muestre el ítem cargado
                qualityDropdown.SetupDropdown();

                // Aplicar nivel de calidad al motor
                QualitySettings.SetQualityLevel(savedQualityIndex, true);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  AUDIO SETTERS — Con Corte a Silencio Total, Clamp y Guardado Forzado
        // ═══════════════════════════════════════════════════════════════
        public void SetMasterVolume(float v)
        {
            PlayerPrefs.SetFloat("MasterVolume", v);
            PlayerPrefs.Save(); // Obliga a Unity a guardar en disco YA MISMO

            if (audioMixer != null)
            {
                // El Clamp asegura que el valor jamás se salga del rango matemático
                float dB = v <= 0.001f ? -80f : Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
                audioMixer.SetFloat("Master", dB);
            }
            else
            {
                AudioListener.volume = v <= 0.001f ? 0f : v;
            }
        }

        public void SetMusicVolume(float v)
        {
            PlayerPrefs.SetFloat("MusicVolume", v);
            PlayerPrefs.Save();

            if (audioMixer != null)
            {
                float dB = v <= 0.001f ? -80f : Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
                audioMixer.SetFloat("Music", dB);
            }
        }

        public void SetSFXVolume(float v)
        {
            PlayerPrefs.SetFloat("SFXVolume", v);
            PlayerPrefs.Save();

            if (audioMixer != null)
            {
                float dB = v <= 0.001f ? -80f : Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
                audioMixer.SetFloat("SFX", dB);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  BRILLO — URP Post-Processing con Override Forzado
        // ═══════════════════════════════════════════════════════════════
        public void SetBrightness(float v)
        {
            PlayerPrefs.SetFloat("Brightness", v);
            PlayerPrefs.Save(); // Guardado instantáneo

            // Si el componente no estaba cacheado, buscarlo ahora
            if (colorAdjustments == null && globalVolume != null && globalVolume.profile != null)
                globalVolume.profile.TryGet(out colorAdjustments);

            if (colorAdjustments != null)
            {
                // ¡ESTA ES LA LÍNEA MÁGICA! Obliga a URP a sobreescribir el brillo en tiempo real
                colorAdjustments.postExposure.overrideState = true;
                colorAdjustments.postExposure.value = v;
            }
            else
            {
                Debug.LogWarning("[MinimalMenuController] ColorAdjustments no encontrado en el globalVolume.");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  CALIDAD
        //  index: posición del ítem en el dropdown (0, 1, 2...).
        //  El orden de los ítems en el dropdown debe coincidir con los
        //  niveles de calidad definidos en Project Settings → Quality.
        // ═══════════════════════════════════════════════════════════════
        public void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt("QualityIndex", index);
        }

        // ═══════════════════════════════════════════════════════════════
        //  RESOLUCIÓN
        // ═══════════════════════════════════════════════════════════════
        public void UpdateResolution()
        {
            if (resolutionDropdown == null || resolutions == null || resolutions.Length == 0) return;
            SetResolution(resolutionDropdown.index);
            resolutionDropdown.UpdateValues();
        }

        public void SetResolution(int resolutionIndex)
        {
            if (resolutions == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length) return;
            Screen.SetResolution(
                resolutions[resolutionIndex].width,
                resolutions[resolutionIndex].height,
                Screen.fullScreen
            );
        }

        // ═══════════════════════════════════════════════════════════════
        //  SETTINGS PANEL — FADE IN / OUT + FOCO DE JOYSTICK
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Abre el panel de Settings con fade-in. Habilita la interacción
        /// antes del fade y transfiere el foco del EventSystem al primer
        /// elemento navegable con joystick al inicio de la animación.
        /// </summary>
        public void OpenSettingsMenu()
        {
            SetMainMenuInteractable(false);
            if (settingsCanvasGroup == null) return;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            // Habilitar interacción antes del fade para que el joystick
            // pueda navegar desde el primer frame visible.
            settingsCanvasGroup.interactable = true;
            settingsCanvasGroup.blocksRaycasts = true;

            // Transferir el foco al primer elemento del panel de Settings
            if (primerElementoSettings != null)
                EventSystem.current.SetSelectedGameObject(primerElementoSettings);

            fadeCoroutine = StartCoroutine(
                FadeCanvasGroup(settingsCanvasGroup, settingsCanvasGroup.alpha, 1f, fadeDuration)
            );
        }

        /// <summary>
        /// Cierra el panel de Settings con fade-out. Al completar el fade,
        /// deshabilita la interacción (para que el joystick no seleccione
        /// elementos invisibles) y devuelve el foco al menú principal.
        /// </summary>
        public void CloseSettingsMenu()
        {
            if (settingsCanvasGroup == null) return;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(
                FadeCanvasGroup(
                    cg: settingsCanvasGroup,
                    from: settingsCanvasGroup.alpha,
                    to: 0f,
                    duration: fadeDuration,
                    onComplete: () =>
                    {
                        SetMainMenuInteractable(true);
                        // Desactivar interacción sólo cuando el panel ya es invisible
                        settingsCanvasGroup.interactable = false;
                        settingsCanvasGroup.blocksRaycasts = false;

                        // Devolver el foco al menú principal
                        if (primerElementoMainMenu != null)
                            EventSystem.current.SetSelectedGameObject(primerElementoMainMenu);
                    }
                )
            );
        }

        /// <summary>
        /// Corutina genérica de fade para un CanvasGroup.
        /// Usa Time.unscaledDeltaTime para funcionar aunque timeScale sea 0
        /// (útil si el menú de pausa pausa el tiempo del juego).
        /// </summary>
        IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to,
                                    float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;
            cg.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));

                // Mientras el alpha sea mayor que 0, mantener el bloqueo activo
                bool visible = cg.alpha > 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;

                yield return null;
            }

            // Garantizar valor exacto al final de la animación
            cg.alpha = to;
            bool finalVisible = to > 0f;
            cg.interactable = finalVisible;
            cg.blocksRaycasts = finalVisible;

            fadeCoroutine = null;
            onComplete?.Invoke();
        }

        // ═══════════════════════════════════════════════════════════════
        //  PANEL MANAGER — HELPERS
        // ═══════════════════════════════════════════════════════════════
        int GetOptionsIndex()
        {
            if (mainPanelManager != null && optionsPanelRoot != null)
            {
                int idx = mainPanelManager.panels.IndexOf(optionsPanelRoot);
                if (idx >= 0) return idx;
            }
            return fallbackOptionsPanelIndex;
        }

        int GetCreditsIndex()
        {
            if (mainPanelManager != null && creditsPanelRoot != null)
            {
                int idx = mainPanelManager.panels.IndexOf(creditsPanelRoot);
                if (idx >= 0) return idx;
            }
            return fallbackCreditsPanelIndex;
        }

        public void OpenOptions()
        {
            if (mainPanelManager == null) return;
            mainPanelManager.PanelAnim(GetOptionsIndex());
            OpenSettingsMenu();
        }

        public void OpenCredits()
        {
            if (mainPanelManager == null) return;
            mainPanelManager.PanelAnim(GetCreditsIndex());
        }
        // --- NUEVO MÉTODO PARA BLOQUEAR EL MENÚ TRASERO ---
        void SetMainMenuInteractable(bool state)
        {
            if (mainMenuButtonsRoot != null)
            {
                CanvasGroup cg = mainMenuButtonsRoot.GetComponent<CanvasGroup>();
                if (cg == null) cg = mainMenuButtonsRoot.gameObject.AddComponent<CanvasGroup>();

                cg.interactable = state;
                cg.blocksRaycasts = state;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  CARGA DE ESCENA
        // ═══════════════════════════════════════════════════════════════
        public void StartGame()
        {
            if (string.IsNullOrEmpty(startSceneName)) return;

            // Iniciamos la corutina de carga
            StartCoroutine(FadeAndLoadRoutine());
        }

        private IEnumerator FadeAndLoadRoutine()
        {
            // 1. Bloqueamos los botones para que el jugador no haga doble clic
            SetMainMenuInteractable(false);

            // 2. Llamamos a tu SceneFadeController y esperamos a que termine
            if (SceneFadeController.Instance != null)
            {
                yield return StartCoroutine(SceneFadeController.Instance.FadeOut());
            }

            // 3. Cargamos la escena cuando la pantalla ya está negra
            SceneManager.LoadScene(startSceneName);
        }

        // ═══════════════════════════════════════════════════════════════
        //  SALIR DEL JUEGO
        // ═══════════════════════════════════════════════════════════════
        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}