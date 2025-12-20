using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonAction : MonoBehaviour
{

    public void LoadScene(string sceneName)
    {
        SceneLoadManager.Instance.LoadScene(sceneName);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

}
