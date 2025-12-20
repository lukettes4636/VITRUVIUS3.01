using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectDragger : MonoBehaviour
{
    [SerializeField] float dragSpeed = 3f;
    [SerializeField] float dragDistance = 2f;
    [SerializeField] LayerMask draggableLayer = -1;

    private Vector2 moveInput;
    private GameObject draggedObject;
    private Rigidbody draggedRigidbody;
    private bool isDragging = false;

    void FixedUpdate()
    {
        if (isDragging && draggedRigidbody != null)
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            Vector3 newPosition = draggedRigidbody.position + movement * dragSpeed * Time.fixedDeltaTime;
            draggedRigidbody.MovePosition(newPosition);
        }
    }

    void Update()
    {
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.magnitude > 0.1f && !isDragging)
        {
            TryStartDrag();
        }
        else if (moveInput.magnitude <= 0.1f && isDragging)
        {
            StopDrag();
        }
    }

    void TryStartDrag()
    {
        Vector3 rayDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Ray ray = new Ray(transform.position, rayDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, dragDistance, draggableLayer))
        {
            if (hit.collider.CompareTag("Draggable") && hit.collider.GetComponent<Rigidbody>() != null)
            {
                draggedObject = hit.collider.gameObject;
                draggedRigidbody = draggedObject.GetComponent<Rigidbody>();
                isDragging = true;
            }
        }
    }

    void StopDrag()
    {
        isDragging = false;
        draggedObject = null;
        draggedRigidbody = null;
    }
}