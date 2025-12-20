using UnityEngine;





public class BillboardEffect : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField] private bool freezeXAxis = false;
    [SerializeField] private bool freezeYAxis = false;
    [SerializeField] private bool freezeZAxis = false;

    [Header("Camera Reference")]
    private Camera mainCamera;

    [Header("Performance")]
    [SerializeField] private bool updateInLateUpdate = true;

    void Start()
    {
        
        mainCamera = Camera.main;

        if (mainCamera == null)
        {

        }
    }

    void Update()
    {
        if (!updateInLateUpdate)
        {
            UpdateBillboard();
        }
    }

    void LateUpdate()
    {
        if (updateInLateUpdate)
        {
            UpdateBillboard();
        }
    }

    private void UpdateBillboard()
    {
        if (mainCamera == null) return;

        
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;

        
        if (freezeXAxis) directionToCamera.x = 0;
        if (freezeYAxis) directionToCamera.y = 0;
        if (freezeZAxis) directionToCamera.z = 0;

        
        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    
    
    
    
    public void SetCamera(Camera newCamera)
    {
        mainCamera = newCamera;
    }
}
