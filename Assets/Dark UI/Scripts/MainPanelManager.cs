using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.Dark
{
    public class MainPanelManager : MonoBehaviour
    {
        [Header("PANEL LIST")]
        public List<GameObject> panels = new List<GameObject>();

        [Header("RESOURCES")]
        public BlurManager homeBlurManager;

        [Header("SETTINGS")]
        public int currentPanelIndex = 0;
        public bool enableBrushAnimation = true;
        public bool enableHomeBlur = true;
         
        private GameObject currentPanel;
        private GameObject nextPanel;
        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;

        string panelFadeIn = "Panel In";
        string panelFadeOut = "Panel Out";

        PanelBrushManager currentBrush;
        PanelBrushManager nextBrush;

        IEnumerator Start()
        {
            // Intro Sequence
            GameObject introObj = new GameObject("Intro_FL");
            Canvas mainCanvas = GetComponentInParent<Canvas>();
            if (mainCanvas != null) introObj.transform.SetParent(mainCanvas.transform, false);
            else introObj.transform.SetParent(this.transform, false);
            
            introObj.transform.SetAsLastSibling();

            UnityEngine.UI.Image img = introObj.AddComponent<UnityEngine.UI.Image>();
            Texture2D tex = null;
            tex = Resources.Load<Texture2D>("Intro/FL3");
            if (tex != null)
            {
                Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                img.sprite = s;
                img.SetNativeSize();
            }
            
            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = new Vector3(0.175f, 0.175f, 1f);

            CanvasGroup cg = introObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            float fadeIn = 1.0f;
            float stay = 2.0f;
            float fadeOut = 1.0f;
            float timer = 0f;

            // Fade In
            while (timer < fadeIn)
            {
                timer += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, timer / fadeIn);
                yield return null;
            }
            cg.alpha = 1f;

            // Stay
            yield return new WaitForSeconds(stay);

            // Fade Out
            timer = 0f;
            while (timer < fadeOut)
            {
                timer += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, timer / fadeOut);
                yield return null;
            }
            cg.alpha = 0f;
            Destroy(introObj);

            ScaleButtons();

            // Original Initialization
            currentPanel = panels[currentPanelIndex];
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            if (enableHomeBlur == true && homeBlurManager != null)
                homeBlurManager.BlurInAnim();
        }

        public void OpenFirstTab()
        {
            currentPanel = panels[currentPanelIndex];
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            if (enableHomeBlur == true && homeBlurManager != null)
                homeBlurManager.BlurInAnim();
        }

        public void PanelAnim(int newPanel)
        {
            if (newPanel != currentPanelIndex)
            {
                currentPanel = panels[currentPanelIndex];

                currentPanelIndex = newPanel;
                nextPanel = panels[currentPanelIndex];

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBrushAnimation == true)
                {
                    currentBrush = currentPanel.GetComponent<PanelBrushManager>();
                    if (currentBrush.brushAnimator != null)
                        currentBrush.BrushSplashOut();
                    nextBrush = nextPanel.GetComponent<PanelBrushManager>();
                    if (nextBrush.brushAnimator != null)
                        nextBrush.BrushSplashIn();
                }

                if (currentPanelIndex == 0 && enableHomeBlur == true && homeBlurManager != null)
                    homeBlurManager.BlurInAnim();
                else if (currentPanelIndex != 0 && enableHomeBlur == true && homeBlurManager != null)
                    homeBlurManager.BlurOutAnim();
            }
        }

        public void NextPage()
        {
            if (currentPanelIndex <= panels.Count - 2)
            {
                currentPanel = panels[currentPanelIndex];
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                currentPanelIndex += 1;
                nextPanel = panels[currentPanelIndex];

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBrushAnimation == true)
                {
                    currentBrush = currentPanel.GetComponent<PanelBrushManager>();
                    if (currentBrush.brushAnimator != null)
                        currentBrush.BrushSplashOut();
                    nextBrush = nextPanel.GetComponent<PanelBrushManager>();
                    if (nextBrush.brushAnimator != null)
                        nextBrush.BrushSplashIn();
                }

                if (currentPanelIndex == 0 && enableHomeBlur == true && homeBlurManager != null)
                    homeBlurManager.BlurInAnim();
                else if (currentPanelIndex != 0 && enableHomeBlur == true && homeBlurManager != null)
                    homeBlurManager.BlurOutAnim();
            }
        }

        public void PrevPage()
        {
            if (currentPanelIndex >= 1)
            {
                currentPanel = panels[currentPanelIndex];
                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentPanelAnimator.Play(panelFadeOut);

                currentPanelIndex -= 1;
                nextPanel = panels[currentPanelIndex];

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);

                if (enableBrushAnimation == true)
                {
                    currentBrush = currentPanel.GetComponent<PanelBrushManager>();
                    if (currentBrush.brushAnimator != null)
                        currentBrush.BrushSplashOut();
                    nextBrush = nextPanel.GetComponent<PanelBrushManager>();
                    if (nextBrush.brushAnimator != null)
                        nextBrush.BrushSplashIn();
                }

                if (currentPanelIndex == 0 && enableHomeBlur == true)
                    homeBlurManager.BlurInAnim();
                else if (currentPanelIndex != 0 && enableHomeBlur == true)
                    homeBlurManager.BlurOutAnim();
            }
        }

        void ScaleButtons()
        {
            var buttons = FindObjectsOfType<MainPanelButton>();
            foreach (var btn in buttons)
            {
                bool isTarget = false;
                string btnName = btn.name.ToUpper();
                if (btnName.Contains("START") || btnName.Contains("QUIT") || btnName.Contains("PLAY") || btnName.Contains("EXIT"))
                {
                    isTarget = true;
                }

                if (!isTarget)
                {
                    var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
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
                        var txt = btn.GetComponentInChildren<Text>();
                        if (txt != null)
                        {
                            string text = txt.text.ToUpper();
                            if (text.Contains("START") || text.Contains("QUIT") || text.Contains("PLAY") || text.Contains("EXIT"))
                            {
                                isTarget = true;
                            }
                        }
                    }
                }

                if (isTarget)
                {
                    if (!Mathf.Approximately(btn.transform.localScale.x, 1.5f))
                    {
                        btn.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        var le = btn.GetComponent<LayoutElement>();
                        if (le != null)
                        {
                            if (le.minWidth > 0) le.minWidth *= 1.5f;
                            if (le.minHeight > 0) le.minHeight *= 1.5f;
                            if (le.preferredWidth > 0) le.preferredWidth *= 1.5f;
                            if (le.preferredHeight > 0) le.preferredHeight *= 1.5f;
                        }
                    }
                }
            }
        }
    }
}
