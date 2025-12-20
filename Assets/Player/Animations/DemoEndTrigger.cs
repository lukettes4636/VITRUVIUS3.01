using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoEndTrigger : MonoBehaviour
{
    public Animator fadeAnimator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            fadeAnimator.SetTrigger("FadeOut");
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
