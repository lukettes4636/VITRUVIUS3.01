using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Michsky.UI.Dark
{
    public class SceneController : MonoBehaviour
    {
        [Header("SCENE SETTINGS")]
        public string gameSceneName = "Game";

        [Header("TRANSITION SETTINGS")]
        public float transitionDelay = 0.5f;
        public bool useFadeTransition = true;

        private bool isTransitioning = false;

        public void LoadGameScene()
        {
            if (isTransitioning == false)
            {
                isTransitioning = true;

                if (useFadeTransition && SceneFadeController.Instance != null)
                {
                    StartCoroutine(LoadGameWithFade());
                }
                else
                {
                    Invoke("LoadGame", transitionDelay);
                }
            }
        }

        public void LoadGameSceneImmediate()
        {
            LoadGame();
        }

        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) == false)
            {
                if (useFadeTransition && SceneFadeController.Instance != null)
                {
                    StartCoroutine(LoadSceneWithFade(sceneName));
                }
                else
                {
                    SceneManager.LoadScene(sceneName);
                }
            }
        }

        public void LoadSceneByIndex(int sceneIndex)
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                if (useFadeTransition && SceneFadeController.Instance != null)
                {
                    StartCoroutine(LoadSceneWithFade(sceneIndex));
                }
                else
                {
                    SceneManager.LoadScene(sceneIndex);
                }
            }
        }

        public void RestartCurrentScene()
        {
            if (useFadeTransition && SceneFadeController.Instance != null)
            {
                StartCoroutine(LoadSceneWithFade(SceneManager.GetActiveScene().name));
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void QuitGameWithDelay()
        {
            if (isTransitioning == false)
            {
                isTransitioning = true;

                if (useFadeTransition && SceneFadeController.Instance != null)
                {
                    StartCoroutine(QuitGameWithFade());
                }
                else
                {
                    Invoke("QuitGame", transitionDelay);
                }
            }
        }

        private void LoadGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        
        private IEnumerator LoadGameWithFade()
        {
            yield return new WaitForSeconds(transitionDelay);
            yield return StartCoroutine(SceneFadeController.Instance.FadeOut());
            LoadGame();
        }

        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            yield return StartCoroutine(SceneFadeController.Instance.FadeOut());
            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator LoadSceneWithFade(int sceneIndex)
        {
            yield return StartCoroutine(SceneFadeController.Instance.FadeOut());
            SceneManager.LoadScene(sceneIndex);
        }

        private IEnumerator QuitGameWithFade()
        {
            yield return new WaitForSeconds(transitionDelay);
            yield return StartCoroutine(SceneFadeController.Instance.FadeOut());
            QuitGame();
        }
    }
} 