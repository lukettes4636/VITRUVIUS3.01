using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickMenuNavigation : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference navigateAction;
    public InputActionReference submitAction;
    public InputActionReference startAction;
    
    [Header("UI Configuration")]
    public GameObject defaultSelected;
    
    private void OnEnable()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();
        startAction.action.Enable();
        
        navigateAction.action.performed += OnNavigate;
        submitAction.action.performed += OnSubmit;
        startAction.action.performed += OnStart;
        
        SetDefaultSelection();
    }
    
    private void OnDisable()
    {
        navigateAction.action.performed -= OnNavigate;
        submitAction.action.performed -= OnSubmit;
        startAction.action.performed -= OnStart;
        
        navigateAction.action.Disable();
        submitAction.action.Disable();
        startAction.action.Disable();
    }
    
    private void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        if (input.y > 0.5f)
        {
            MoveSelectionUp();
        }
        else if (input.y < -0.5f)
        {
            MoveSelectionDown();
        }
        else if (input.x > 0.5f)
        {
            MoveSelectionRight();
        }
        else if (input.x < -0.5f)
        {
            MoveSelectionLeft();
        }
    }
    
    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SubmitCurrentSelection();
        }
    }
    
    private void OnStart(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SubmitCurrentSelection();
        }
    }
    
    private void SetDefaultSelection()
    {
        if (defaultSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultSelected);
        }
    }
    
    private void MoveSelectionUp()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SetDefaultSelection();
            return;
        }
        
        Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
        if (current != null && current.FindSelectableOnUp() != null)
        {
            current.FindSelectableOnUp().Select();
        }
    }
    
    private void MoveSelectionDown()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SetDefaultSelection();
            return;
        }
        
        Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
        if (current != null && current.FindSelectableOnDown() != null)
        {
            current.FindSelectableOnDown().Select();
        }
    }
    
    private void MoveSelectionLeft()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SetDefaultSelection();
            return;
        }
        
        Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
        if (current != null && current.FindSelectableOnLeft() != null)
        {
            current.FindSelectableOnLeft().Select();
        }
    }
    
    private void MoveSelectionRight()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            SetDefaultSelection();
            return;
        }
        
        Selectable current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
        if (current != null && current.FindSelectableOnRight() != null)
        {
            current.FindSelectableOnRight().Select();
        }
    }
    
    private void SubmitCurrentSelection()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            UnityEngine.UI.Button button = EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
        }
    }
}