using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixPauseMenuScaling : EditorWindow
{
    [MenuItem("Tools/Fix Pause Menu Scaling")]
    public static void FixScaling()
    {
        GameObject canvasGo = GameObject.Find("PauseMenuCanvas");
        if (canvasGo == null)
        {
            Debug.LogError("PauseMenuCanvas not found in scene!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvasGo, "Fix Pause Menu Scaling");

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.localScale = Vector3.one;
            Debug.Log("Set PauseMenuCanvas scale to (1,1,1)");
        }

        Transform panelTransform = canvasGo.transform.Find("PausePanel");
        if (panelTransform != null)
        {
            panelTransform.localScale = Vector3.one;
            Debug.Log("Set PausePanel scale to (1,1,1)");

            // Fix Title
            Transform titleTransform = panelTransform.Find("PauseTitle");
            if (titleTransform != null)
            {
                titleTransform.localScale = Vector3.one;
                RectTransform titleRect = titleTransform.GetComponent<RectTransform>();
                if (titleRect != null)
                {
                    titleRect.sizeDelta = new Vector2(400, 100);
                    titleRect.anchoredPosition = new Vector2(0, 150);
                }
                TMP_Text titleText = titleTransform.GetComponentInChildren<TMP_Text>();
                if (titleText != null)
                {
                    titleText.fontSize = 60;
                    titleText.alignment = TextAlignmentOptions.Center;
                }
            }

            // Fix Buttons
            string[] buttonNames = { "CONTINUE", "CONTROLS", "QuitButton" };
            float startY = 30;
            float spacing = 100;

            for (int i = 0; i < buttonNames.Length; i++)
            {
                Transform bTransform = panelTransform.Find(buttonNames[i]);
                if (bTransform != null)
                {
                    bTransform.localScale = Vector3.one;
                    RectTransform bRect = bTransform.GetComponent<RectTransform>();
                    if (bRect != null)
                    {
                        bRect.sizeDelta = new Vector2(300, 80);
                        bRect.anchoredPosition = new Vector2(0, startY - (i * spacing));
                    }

                    TMP_Text bText = bTransform.GetComponentInChildren<TMP_Text>();
                    if (bText != null)
                    {
                        bText.fontSize = 32;
                    }
                }
            }
        }

        // Fix Controls Panel
        GameObject controlsGo = GameObject.Find("ControlsCanvas");
        if (controlsGo == null)
        {
            // Try finding it even if inactive
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.name == "ControlsCanvas" && go.scene.isLoaded)
                {
                    controlsGo = go;
                    break;
                }
            }
        }

        if (controlsGo != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(controlsGo, "Fix Controls Scaling");
            
            // Move it to PauseMenuCanvas if it's nested elsewhere
            if (controlsGo.transform.parent != canvasGo.transform)
            {
                Undo.SetTransformParent(controlsGo.transform, canvasGo.transform, "Move ControlsCanvas");
                Debug.Log("Moved ControlsCanvas to PauseMenuCanvas root");
            }

            RectTransform controlsRect = controlsGo.GetComponent<RectTransform>();
            if (controlsRect != null)
            {
                controlsRect.localScale = Vector3.one;
                controlsRect.anchorMin = Vector2.zero;
                controlsRect.anchorMax = Vector2.one;
                controlsRect.sizeDelta = Vector2.zero;
                controlsRect.anchoredPosition = Vector2.zero;
            }

            // Fix Secondary Background (the dim/blur)
            Transform bgTransform = controlsGo.transform.Find("SecondaryBackgroundImage");
            if (bgTransform != null)
            {
                bgTransform.localScale = Vector3.one;
                RectTransform bgRect = bgTransform.GetComponent<RectTransform>();
                if (bgRect != null)
                {
                    bgRect.anchorMin = Vector2.zero;
                    bgRect.anchorMax = Vector2.one;
                    bgRect.sizeDelta = Vector2.zero;
                    bgRect.anchoredPosition = Vector2.zero;
                }
            }

            // Fix Main Controls Image
            Transform mainImgTransform = controlsGo.transform.Find("MainControlsImage");
            if (mainImgTransform != null)
            {
                mainImgTransform.localScale = Vector3.one;
                RectTransform mainImgRect = mainImgTransform.GetComponent<RectTransform>();
                if (mainImgRect != null)
                {
                    mainImgRect.sizeDelta = new Vector2(900, 600);
                    mainImgRect.anchoredPosition = Vector2.zero;
                }
            }

            // Fix Back Button
            Transform backBtnTransform = controlsGo.transform.Find("BackButton");
            if (backBtnTransform != null)
            {
                backBtnTransform.localScale = Vector3.one;
                RectTransform backBtnRect = backBtnTransform.GetComponent<RectTransform>();
                if (backBtnRect != null)
                {
                    backBtnRect.sizeDelta = new Vector2(200, 60);
                    backBtnRect.anchorMin = new Vector2(0.5f, 0);
                    backBtnRect.anchorMax = new Vector2(0.5f, 0);
                    backBtnRect.anchoredPosition = new Vector2(0, 100);
                }
                
                TMP_Text backText = backBtnTransform.GetComponentInChildren<TMP_Text>();
                if (backText != null)
                {
                    backText.fontSize = 28;
                }
            }
            
            Undo.RecordObject(controlsGo, "Hide ControlsCanvas");
            controlsGo.SetActive(false); // Hide by default
        }

        Debug.Log("Pause Menu Scaling Fixed!");
    }
}
