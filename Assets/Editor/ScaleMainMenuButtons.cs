using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ScaleMainMenuButtons : MonoBehaviour
{
    public static void ScaleButtons()
    {
        GameObject buttonList = GameObject.Find("Button List");
        if (buttonList == null)
        {
            // Try finding by searching all objects
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.hideFlags != HideFlags.None)
                    continue;
                
                if (go.name == "Button List")
                {
                    buttonList = go;
                    break;
                }
            }
        }

        if (buttonList == null)
        {
            Debug.LogError("Button List not found!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(buttonList, "Scale Buttons");

        foreach (Transform child in buttonList.transform)
        {
            bool isTarget = false;
            string childName = child.name.ToUpper();
            
            // Check name
            if (childName.Contains("START") || childName.Contains("QUIT") || childName.Contains("PLAY") || childName.Contains("EXIT"))
            {
                isTarget = true;
            }

            // Check text component
            var tmp = child.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                string text = tmp.text.ToUpper();
                if (text.Contains("START") || text.Contains("QUIT") || text.Contains("PLAY") || text.Contains("EXIT"))
                {
                    isTarget = true;
                }
            }
            else
            {
                var txt = child.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    string text = txt.text.ToUpper();
                    if (text.Contains("START") || text.Contains("QUIT") || text.Contains("PLAY") || text.Contains("EXIT"))
                    {
                        isTarget = true;
                    }
                }
            }

            if (isTarget)
            {
                Debug.Log($"Scaling button: {child.name}");
                Undo.RegisterCompleteObjectUndo(child, "Scale Button Transform");
                
                // Check if already scaled to avoid double application on LayoutElement
                bool alreadyScaled = Mathf.Approximately(child.localScale.x, 1.5f);

                // Scale RectTransform
                child.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                if (!alreadyScaled)
                {
                    // Check for LayoutElement and scale it if present
                    var le = child.GetComponent<LayoutElement>();
                    if (le != null)
                    {
                        Undo.RegisterCompleteObjectUndo(le, "Scale Layout Element");
                        if (le.minWidth > 0) le.minWidth *= 1.5f;
                        if (le.minHeight > 0) le.minHeight *= 1.5f;
                        if (le.preferredWidth > 0) le.preferredWidth *= 1.5f;
                        if (le.preferredHeight > 0) le.preferredHeight *= 1.5f;
                    }
                }
            }
        }
        
        Debug.Log("Button scaling complete.");
    }
}
