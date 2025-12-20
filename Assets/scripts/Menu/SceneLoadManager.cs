using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class SceneLoadManager : MonoBehaviour
{
    #region Singleton
    public static SceneLoadManager Instance; 

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    #endregion

    [Header("UI")]
    [SerializeField] private RawImage _loadingBG;
    [SerializeField] private float _fadeTime = 0.5f;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Image _barBG;
    [SerializeField] private Image _barFill;

    private bool _isLoading = false;

    private void Start()
    {
        _barFill.fillAmount = 0.0f;
        _stateText.text = $"";
        _loadingBG.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        _loadingBG.enabled = false;
        _stateText.enabled = false;
        _barBG.enabled = false;
        _barFill.enabled = false;
    }

    public void LoadScene(string sceneName)
    {
        if(!_isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        _isLoading = true;

        _loadingBG.enabled = true;

        float t = 0.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime / _fadeTime;

            _loadingBG.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(0.0f, 1.0f, t));

            yield return null;
        }

        _loadingBG.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        _stateText.enabled = true;
        _stateText.text = $"Cargando...";
        _barBG.enabled = true;
        _barFill.enabled = true;
        _barFill.color = Color.red;

        AsyncOperation AsyncOp = SceneManager.LoadSceneAsync(sceneName);

        AsyncOp.allowSceneActivation = false;

        while(AsyncOp.progress < 0.9f)
        {
            _barFill.fillAmount = AsyncOp.progress / 0.9f;

            yield return null;
        }

        _stateText.text = $"Presiona cualquier tecla para continuar.";
        _barFill.color = Color.red ;

        while(!Input.anyKey) 
        {
            yield return null;
        }

        AsyncOp.allowSceneActivation = true;

        _stateText.enabled = false;
        _barBG.enabled = false;
        _barFill.enabled = false;

         t = 0.0f;

        while (t < 1.0f)
        {
            t += Time.deltaTime / _fadeTime;

            _loadingBG.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Lerp(1.0f, 0.0f, t));

            yield return null;
        }

        _loadingBG.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        _isLoading = false;

    }
}


