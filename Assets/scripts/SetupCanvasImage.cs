using UnityEngine;
using UnityEngine.UI;

public class SetupCanvasImage : MonoBehaviour
{
    [SerializeField] private Texture2D targetImage;
    
    private void Start()
    {
        SetupImage();
    }
    
    private void SetupImage()
    {
        if (targetImage == null)
        {

            return;
        }
        
        Image imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {

            return;
        }
        
        Sprite imageSprite = Sprite.Create(targetImage, 
            new Rect(0, 0, targetImage.width, targetImage.height), 
            new Vector2(0.5f, 0.5f));
        
        imageComponent.sprite = imageSprite;
        imageComponent.preserveAspect = true;
    }
}
