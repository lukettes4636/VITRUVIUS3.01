using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {

        }
    }

    void Update()
    {
        if (mainCameraTransform == null)
            return;

        
        
        

        
        
        

        
        Vector3 lookDirection = mainCameraTransform.position - transform.position;
        lookDirection.y = 0; 

        
        
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(-lookDirection);
        }
    }
}
