using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Michsky.UI.Dark
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneFadeController : MonoBehaviour
    {
        public static SceneFadeController Instance { get; private set; }

        [Header("FADE SETTINGS")]
        [Tooltip("Tiempo que tarda en ponerse la pantalla negra al salir.")]
        public float fadeOutDuration = 1f;
        [Tooltip("Tiempo que tarda en aclararse la pantalla en la nueva escena (Más lento = más suave).")]
        public float fadeInDuration = 2.5f;
        public bool fadeInOnStart = true;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (fadeInOnStart && canvasGroup != null)
            {
                // Nos aseguramos de que empiece totalmente negra
                canvasGroup.alpha = 1f;
                StartCoroutine(FadeIn());
            }
        }

        public IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.blocksRaycasts = true;

            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        public IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                // Usamos fadeInDuration para que esta transición sea mucho más lenta
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}