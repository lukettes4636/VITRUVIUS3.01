using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasToggleController : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private InputActionReference toggleAction;

    private void OnEnable() => toggleAction.action.Enable();
    private void OnDisable() => toggleAction.action.Disable();

    private void Update()
    {
        if(toggleAction.action.WasPerformedThisFrame())
        {
            ToggleCanvas();
        }
    }

    private void ToggleCanvas()
    {
        if(targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(!targetCanvas.gameObject.activeSelf);
        }
    }
}