using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference navigateAction;
    public InputActionReference submitAction;
    
    [Header("UI Configuration")]
    public Button[] menuButtons;
    public int defaultSelectedIndex = 0;
    
    private int currentSelectedIndex;
    private bool navigationEnabled = true;
    
    private void OnEnable()
    {
        navigateAction.action.Enable();
        submitAction.action.Enable();
        
        navigateAction.action.performed += OnNavigate;
        submitAction.action.performed += OnSubmit;
        
        SetSelectedButton(defaultSelectedIndex);
    }
    
    private void OnDisable()
    {
        navigateAction.action.performed -= OnNavigate;
        submitAction.action.performed -= OnSubmit;
        
        navigateAction.action.Disable();
        submitAction.action.Disable();
    }
    
    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!navigationEnabled) return;
        
        Vector2 input = context.ReadValue<Vector2>();
        
        if (input.y > 0.5f)
        {
            NavigateUp();
        }
        else if (input.y < -0.5f)
        {
            NavigateDown();
        }
    }
    
    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!navigationEnabled) return;
        
        if (context.performed)
        {
            menuButtons[currentSelectedIndex].onClick.Invoke();
        }
    }
    
    private void NavigateUp()
    {
        int newIndex = currentSelectedIndex - 1;
        if (newIndex < 0) newIndex = menuButtons.Length - 1;
        SetSelectedButton(newIndex);
    }
    
    private void NavigateDown()
    {
        int newIndex = currentSelectedIndex + 1;
        if (newIndex >= menuButtons.Length) newIndex = 0;
        SetSelectedButton(newIndex);
    }
    
    private void SetSelectedButton(int index)
    {
        if (menuButtons.Length == 0) return;
        
        if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
        {
            menuButtons[currentSelectedIndex].OnDeselect(null);
        }
        
        currentSelectedIndex = index;
        menuButtons[currentSelectedIndex].Select();
        menuButtons[currentSelectedIndex].OnSelect(null);
    }
    
    public void EnableNavigation()
    {
        navigationEnabled = true;
        SetSelectedButton(currentSelectedIndex);
    }
    
    public void DisableNavigation()
    {
        navigationEnabled = false;
    }
}