using UnityEngine;
using TMPro;
using System.Text;

public class TextSizeDiagnostic : MonoBehaviour
{
    [Header("Diagnostico de Texto")]
    [SerializeField] private TextMeshProUGUI textToDiagnose;
    [SerializeField] private bool showDiagnostics = true;
    
    void Update()
    {
        if (showDiagnostics && textToDiagnose != null)
        {

        }
    }
    
    public string GetTextDiagnostics()
    {
        if (textToDiagnose == null) return "TextMeshProUGUI no asignado";
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== DIAGNOSTICO TEXTO NPC ===");
        sb.AppendLine($"Font Size: {textToDiagnose.fontSize}");
        sb.AppendLine($"Font Asset: {textToDiagnose.font?.name}");
        sb.AppendLine($"Canvas Scale: {GetCanvasScaleFactor()}");
        sb.AppendLine($"RectTransform Size: {textToDiagnose.rectTransform.rect.size}");
        sb.AppendLine($"Screen DPI: {Screen.dpi}");
        sb.AppendLine($"Screen Size: {Screen.width}x{Screen.height}");
        sb.AppendLine($"Text Length: {textToDiagnose.text?.Length} caracteres");
        
        return sb.ToString();
    }
    
    private float GetCanvasScaleFactor()
    {
        Canvas canvas = textToDiagnose.GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1f;
    }
    
    [ContextMenu("Mostrar Diagnostico")]
    public void ShowDiagnostics()
    {

    }
}
