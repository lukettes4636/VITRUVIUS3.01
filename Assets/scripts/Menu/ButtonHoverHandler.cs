using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private PauseController pauseController;
    private int buttonIndex;
    
    public void SetupHover(Button btn, PauseController controller, int index)
    {
        button = btn;
        pauseController = controller;
        buttonIndex = index;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pauseController != null)
        {
            pauseController.SetHoveredButton(buttonIndex);
            pauseController.PlayHoverSound();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}