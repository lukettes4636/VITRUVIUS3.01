using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeypadUIManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button[] keypadButtons;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Cdigo correcto (4 dgitos)")]
    [SerializeField] private string correctCode = "1234";

    [Header("Configuracin")]
    [SerializeField] private float navigateCooldown = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;

    private string enteredCode = "";
    private int selectedIndex = 0;
    private float lastNavigateTime;

    private KeypadDoorController doorController;
    private PlayerInput activePlayerInput;

    private void Start()
    {
        
        UpdateVisualSelection();
    }

    public void Open(KeypadDoorController controller, PlayerInput playerInput)
    {
        doorController = controller;

        
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        enteredCode = "";
        selectedIndex = 0;
        UpdateDisplay();

        activePlayerInput = playerInput;

        if (activePlayerInput != null)
        {
            activePlayerInput.SwitchCurrentActionMap("UI");
            activePlayerInput.actions["Navigate"].performed += OnNavigate;
            activePlayerInput.actions["Submit"].performed += OnSubmitInput;
            activePlayerInput.actions["Cancel"].performed += OnCancelInput;
        }

        UpdateVisualSelection();
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();
        if (Time.time - lastNavigateTime < navigateCooldown)
            return;

        lastNavigateTime = Time.time;

        int cols = 3;
        int total = keypadButtons.Length;

        if (Mathf.Abs(dir.x) > 0.5f)
        {
            selectedIndex = (selectedIndex + (dir.x > 0 ? 1 : -1) + total) % total;
        }
        else if (Mathf.Abs(dir.y) > 0.5f)
        {
            int delta = (dir.y > 0 ? -cols : cols);
            selectedIndex = (selectedIndex + delta + total) % total;
        }

        UpdateVisualSelection();
    }

    private void OnSubmitInput(InputAction.CallbackContext ctx)
    {
        Button selectedButton = keypadButtons[selectedIndex];
        if (selectedButton != null)
            selectedButton.onClick.Invoke();
    }

    private void OnCancelInput(InputAction.CallbackContext ctx)
    {
        CloseUI();
    }

    public void PressNumber(string num)
    {
        if (enteredCode.Length >= 4) return;
        enteredCode += num;
        UpdateDisplay();
    }

    public void PressClear()
    {
        enteredCode = "";
        UpdateDisplay();
    }

    public void PressEnter()
    {
        if (enteredCode == correctCode)
        {
            if (audioSource != null && correctSound != null)
                audioSource.PlayOneShot(correctSound);

            displayText.text = "Correcto";
            DialogueManager.ShowKeypadCodeResultDialogue(activePlayerInput != null ? activePlayerInput.gameObject : null, true);
            StartCoroutine(CorrectSequence());
        }
        else
        {
            if (audioSource != null && incorrectSound != null)
                audioSource.PlayOneShot(incorrectSound);

            displayText.text = "Incorrecto";
            DialogueManager.ShowKeypadCodeResultDialogue(activePlayerInput != null ? activePlayerInput.gameObject : null, false);
            StartCoroutine(ResetAfterDelay());
        }
    }

    private IEnumerator CorrectSequence()
    {
        yield return new WaitForSeconds(0.5f);
        doorController.OpenDoor();
        CloseUI();
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(0.8f);
        enteredCode = "";
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = enteredCode.PadRight(4, '_');
    }

    private void UpdateVisualSelection()
    {
        for (int i = 0; i < keypadButtons.Length; i++)
        {
            var colors = keypadButtons[i].colors;
            colors.normalColor = (i == selectedIndex) ? selectedColor : normalColor;
            keypadButtons[i].colors = colors;
        }
    }

    private void CloseUI()
    {
        
        if (activePlayerInput != null)
        {
            activePlayerInput.actions["Navigate"].performed -= OnNavigate;
            activePlayerInput.actions["Submit"].performed -= OnSubmitInput;
            activePlayerInput.actions["Cancel"].performed -= OnCancelInput;
            activePlayerInput.SwitchCurrentActionMap("Player");
        }

        if (doorController != null)
            doorController.ReleasePlayer();

        gameObject.SetActive(false);
    }
}
