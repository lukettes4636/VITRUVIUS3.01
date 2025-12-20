using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUpDOS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTMPro;
    [SerializeField] private string[] animatedText;
    [SerializeField] private float timeBetweenText = 3f;
    [SerializeField] private string mainMenuScene = "";
    private int actualIndex = 0;
    private bool isFinished = false;

    void Start()
    {
        // Start Music (Music1)
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayMusic(MusicID.Music1, true);
        }
        StartCoroutine(TextChangeAnim());
    }

    void Update()
    {
        if (!isFinished && Input.anyKeyDown)
        {
            ChangeToMainMenu();
        }
    }

    IEnumerator TextChangeAnim()
    {
        while (actualIndex < animatedText.Length)
        {
            SoundManager.instance.PlaySound(SoundID.MSDos);
            textTMPro.text = animatedText[actualIndex];
            Debug.Log(textTMPro.text);
            actualIndex++;
            yield return new WaitForSeconds(timeBetweenText);
        }

        isFinished = true;
        yield return new WaitForSeconds(0.0f);
        ChangeToMainMenu();
    }

    private void ChangeToMainMenu()
    {
        SceneManager.LoadSceneAsync(mainMenuScene);
    }
}
