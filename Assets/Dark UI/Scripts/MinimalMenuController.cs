using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Michsky.UI.Dark
{
    public class MinimalMenuController : MonoBehaviour
    {
        [Header("MENU")]
        public Transform mainMenuButtonsRoot;
        public MainPanelManager mainPanelManager;
        public GameObject optionsPanelRoot;
        public string startSceneName = "DaVinciPB";
        public GameObject creditsPanelRoot;
        public int fallbackOptionsPanelIndex = 1;
        public int fallbackCreditsPanelIndex = 2;

        [Header("AUDIO")]
        public AudioMixer audioMixer;
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Slider voicesSlider;

        [Header("CONTROLS")]
        public Slider mouseSensitivitySlider;
        public Slider brightnessSlider;
        public SwitchManager fsrSwitch;
        public Toggle fsrToggle;

        [Header("QUALITY")]
        public Button qualityLowButton;
        public Button qualityMediumButton;
        public Button qualityHighButton;

        readonly HashSet<string> allowedMainButtons = new HashSet<string>(new[] { "START", "OPTIONS", "CREDITS" });
        readonly HashSet<string> allowedOptionGroups = new HashSet<string>(new[] { "MouseSensitivity", "Brightness", "Resolution" });

        [Header("RESOLUTION")]
        public CustomDropdown resolutionDropdown;
        Resolution[] resolutions;
        List<string> resOptions = new List<string>();

        void Awake()
        {
            FilterMainMenuButtons();
            FilterOptionGroups();
            BindUI();
            LoadPrefs();
            WireMenuButtons();
            SetupResolutionDropdown();
        }

        void FilterMainMenuButtons()
        {
            if (mainMenuButtonsRoot == null) return;
            for (int i = 0; i < mainMenuButtonsRoot.childCount; i++)
            {
                var child = mainMenuButtonsRoot.GetChild(i).gameObject;
                var label = child.GetComponentInChildren<TextMeshProUGUI>();
                var name = label != null ? label.text.Trim().ToUpperInvariant() : child.name.Trim().ToUpperInvariant();
                if (!allowedMainButtons.Contains(name)) child.SetActive(false);
            }
        }

        void FilterOptionGroups()
        {
            if (optionsPanelRoot == null) return;
            for (int i = 0; i < optionsPanelRoot.transform.childCount; i++)
            {
                var group = optionsPanelRoot.transform.GetChild(i).gameObject;
                var name = group.name.Trim();
                if (!allowedOptionGroups.Contains(name)) group.SetActive(false);
            }
        }

        void BindUI()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            if (voicesSlider != null) voicesSlider.onValueChanged.AddListener(SetVoicesVolume);
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
            if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(SetBrightness);
            if (fsrSwitch != null)
            {
                fsrSwitch.OnEvents.AddListener(() => SetFSR(true));
                fsrSwitch.OffEvents.AddListener(() => SetFSR(false));
            }
            if (fsrToggle != null)
            {
                fsrToggle.onValueChanged.AddListener(SetFSR);
            }
            if (qualityLowButton != null) qualityLowButton.onClick.AddListener(() => SetQuality(0));
            if (qualityMediumButton != null) qualityMediumButton.onClick.AddListener(() => SetQuality(2));
            if (qualityHighButton != null) qualityHighButton.onClick.AddListener(() => SetQuality(5));
        }

        void SetupResolutionDropdown()
        {
            if (resolutionDropdown == null) return;
            if (optionsPanelRoot != null && !optionsPanelRoot.activeInHierarchy) { /* still populate */ }

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

                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    resolutionDropdown.selectedItemIndex = currentResolutionIndex;
                    resolutionDropdown.index = currentResolutionIndex;
                }
            }

            resolutionDropdown.SetupDropdown();
        }

        void WireMenuButtons()
        {
            if (mainMenuButtonsRoot == null) return;
            for (int i = 0; i < mainMenuButtonsRoot.childCount; i++)
            {
                var child = mainMenuButtonsRoot.GetChild(i).gameObject;
                var label = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                var name = label != null ? label.text.Trim().ToUpperInvariant() : child.name.Trim().ToUpperInvariant();
                var btn = child.GetComponent<Button>();
                if (btn == null) continue;

                if (name == "START")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(StartGame);
                }

                else if (name == "OPTIONS")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OpenOptions);
                }

                else if (name == "CREDITS")
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OpenCredits);
                }
            }
        }

        void LoadPrefs()
        {
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivitySlider.value);
            if (brightnessSlider != null) brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", brightnessSlider.value);
            var fsr = PlayerPrefs.GetInt("FSR", 0) == 1;
            SetFSR(fsr);
        }

        public void SetMasterVolume(float v)
        {
            if (audioMixer != null) audioMixer.SetFloat("Master", Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f);
            else AudioListener.volume = Mathf.Clamp(v, 0f, 1f);
        }

        public void SetMusicVolume(float v)
        {
            if (audioMixer != null) audioMixer.SetFloat("Music", Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f);
        }

        public void SetSFXVolume(float v)
        {
            if (audioMixer != null) audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f);
        }

        public void SetVoicesVolume(float v)
        {
            if (audioMixer != null) audioMixer.SetFloat("Voices", Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f);
        }

        public void SetMouseSensitivity(float v)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", v);
        }

        public void SetBrightness(float v)
        {
            PlayerPrefs.SetFloat("Brightness", v);
            Shader.SetGlobalFloat("_GlobalBrightness", v);
        }

        public void SetFSR(bool on)
        {
            PlayerPrefs.SetInt("FSR", on ? 1 : 0);
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.renderScale = on ? 0.77f : 1.0f;
            }
        }

        public void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
        }

        public void UpdateResolution()
        {
            if (resolutionDropdown == null || resolutions == null || resolutions.Length == 0) return;
            SetResolution(resolutionDropdown.index);
            resolutionDropdown.UpdateValues();
        }

        public void SetResolution(int resolutionIndex)
        {
            if (resolutions == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length) return;
            Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, Screen.fullScreen);
        }

        public void StartGame()
        {
            if (string.IsNullOrEmpty(startSceneName)) return;

            bool sceneInBuild = false;
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == startSceneName)
                {
                    sceneInBuild = true;
                    break;
                }
            }

            if (sceneInBuild)
                SceneManager.LoadScene(startSceneName);
            else
                Debug.LogError($"Scene '{startSceneName}' not found in Build Settings.");
        }

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
        }

        public void OpenCredits()
        {
            if (mainPanelManager == null) return;
            mainPanelManager.PanelAnim(GetCreditsIndex());
        }

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

