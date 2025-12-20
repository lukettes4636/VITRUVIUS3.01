using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Michsky.UI.Dark
{
    public class SceneFadeController : MonoBehaviour
    {
        public static SceneFadeController Instance { get; private set; }

        [Header("FADE SETTINGS")]
        public Image fadeImage;
        public float fadeDuration = 1f;
        public bool fadeInOnStart = true;

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

            
            if (fadeImage == null)
            {

            }
        }

        private void Start()
        {
            if (fadeInOnStart && fadeImage != null)
            {
                
                fadeImage.color = new Color(0, 0, 0, 1);
                StartCoroutine(FadeIn());
            }
        }

        public IEnumerator FadeOut()
        {
            if (fadeImage == null) yield break;

            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }

            
            color.a = 1f;
            fadeImage.color = color;
        }

        public IEnumerator FadeIn()
        {
            if (fadeImage == null) yield break;

            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                fadeImage.color = color;
                yield return null;
            }

            
            color.a = 0f;
            fadeImage.color = color;
        }
    }
}
